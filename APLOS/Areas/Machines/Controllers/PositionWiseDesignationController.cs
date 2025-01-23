using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Materials;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Machines.Controllers
{
    public class PositionWiseDesignationController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public PositionWiseDesignationController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

       
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetEmployeeCategoryList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,UserName as Text from HKP.EmployeeCategory";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadPositionWiseDesignationList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * ,(select MP.Code from ORG.Position MP where MP.Id=PWD.PositionCodeId) as PositionCode, 
(select E.EmployeeName from EmployeeInformation E where E.SystemId=PWD.ResponsiblePersonId) as ResponsiblePerson,
(select EC.UserName from HKP.EmployeeCategory EC where EC.Id=PWD.EmployeeCategoryId) as EmployeeCategory
                            FROM [TRN].[PositionWiseDesignation] PWD";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetPositionCode(string EmployeeCategoryid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select P.Id,P.Code,P.UserName Position,P.Activity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection
from ORG.Position P	
LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
left outer join MST.DesignationMaster DM ON DM.DesignationId=P.DesignationId
where P.Active = 1 and DM.EmployeeCategoryId='"+ EmployeeCategoryid + "'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=mb.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
                            WHERE EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadPositionEditData(string PositionID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT * ,(select MP.Code from ORG.Position MP where MP.Id=PWD.PositionCodeId) as PositionCode, 
(select E.EmployeeName from EmployeeInformation E where E.SystemId=PWD.ResponsiblePersonId) as ResponsiblePerson,
(select EC.UserName from HKP.EmployeeCategory EC where EC.Id=PWD.EmployeeCategoryId) as EmployeeCategory
                            FROM [TRN].[PositionWiseDesignation] PWD where PWD.Id='" + PositionID + @"'";
            return Json(new { position = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> PositionData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                //conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagement] where ScheduleCode='" + ScheduleData["ScheduleCode"] + "'", out DataSet dsSkillManagementCodeValidation, false, "1");
               
                

                DataSet dsPositionWiseDesignation;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [Trn].[PositionWiseDesignation] where Id='" + PositionData["Id"] + "'", out dsPositionWiseDesignation, false, "1");
                string _Id = "";

                #region data update
                if (dsPositionWiseDesignation.Tables[0].Rows.Count == 0)
                {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("PositionWiseDesignation", out _Id);
                        _Id = "PWD" + _Id;
                        PositionData["Id"] = _Id;
                        AddNewRow(dsPositionWiseDesignation.Tables[0], PositionData);
                }
                else
                {
                    _Id = PositionData["Id"].ToString();
                    EditRow(dsPositionWiseDesignation.Tables[0].Rows[0], PositionData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsPositionWiseDesignation);

                return Json(new { Error = false, Data = PositionData, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        [HttpPost]
        public ActionResult PositionDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from TRN.PositionWiseDesignation where Id ='" + id + @"'");
                conC.CommitTransaction();
               
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadDesignationGroupDetails(string PositionId,string EmpCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select distinct CAST (CASE WHEN PDG.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,PDG.Id,DG.Id DesignationGroupId,DG.UserName DesignationGroup,DG.Code,EC.UserName EmployeeCategory,EC.Id EmployeeCategoryId 
                            from hkp.DesignationGroup DG
							Left Join MST.DesignationMaster DM ON DM.DesignationGroupId=DG.Id
							LEFT JOIN [TRN].[PositionWiseDesignationGroup] PDG ON PDG.DesignationGroupId=DM.DesignationGroupId and PDG.EmployeeCategoryId=DM.EmployeeCategoryId and PDG.PDID='" + PositionId + @"'
							left join hkp.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
                            where DG.Active = 1 and DM.EmployeeCategoryId='" + EmpCategoryId + @"'order by PDG.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult createDesignationGroup(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "TRN.PositionWiseDesignationGroup";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and PDID='" + item["PDID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "PDG" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadPositionWiseDesignationDetails(string PositionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select distinct CAST (CASE WHEN PDGL.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,PDGL.Id,D.Id DesignationId,D.UserName Designation,
LD.Id as LegalDesignationId,LD.UserName as LegalDesignation,DG.UserName as DesignationGroup,DG.Id as DesignationGroupId,PDGL.SkillCategory
from MST.DesignationMaster DM
left Join hkp.Designation D ON D.Id=DM.DesignationId and D.Active=1
left join MST.DesignationMasterLegalDesignation LMD ON LMD.DesignationMasterId=DM.Id
left join hkp.LegalDesignation LD ON LD.Id=LMD.LegalDesignationId
left join hkp.DesignationGroup DG ON DG.Id=DM.DesignationGroupId
left join TRN.PositionWiseDesignationGivenLegal PDGL ON PDGL.PDID='" + PositionId + @"' and PDGL.DesignationId=D.Id and PDGL.LegalDesignationId=LD.Id
where DM.Active=1 and DM.DesignationGroupId in (select PDG.DesignationGroupId from [TRN].[PositionWiseDesignationGroup] PDG where PDG.PDID='" + PositionId + @"') order by PDGL.Id  desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult createPositionWiseDesignation(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "TRN.PositionWiseDesignationGivenLegal";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and PDID='" + item["PDID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "DGL" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        [Authorize, HttpGet]
        public ActionResult LoadPositionWiseDesignationReports()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select distinct E.UserName as EmployeeCategory,D.UserName as Division,DEP.UserName as Department,S.UserName as Section,SS.UserName as SubSection,
P.Activity,P.UserName as Process,P.Code as PositionCode,RP.EmployeeName as ResponsiblePerson,PWD.PositionCategory,PWD.CostReviewCategory,PWD.Remarks,
DG.UserName as DesignationGroup,isnull(DE.UserName,'') as Designation,isnull(LD.UserName,'') as LegalDesignation,isnull(PWDGL.SkillCategory,'') as SkillCategory from 
TRN.PositionWiseDesignation PWD
LEFT OUTER JOIN hkp.EmployeeCategory E ON E.Id=PWD.EmployeeCategoryId
LEFT OUTER JOIN org.Position P ON P.Id=PWD.PositionCodeId
LEFT OUTER join EmployeeInformation EI ON EI.PositionID=P.Id
left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
							left join ORG.Position PP on PP.Id=mPB.PositionID
LEFT OUTER join EmployeeInformation RP ON RP.SystemId=PWD.ResponsiblePersonId
LEFT OUTER JOIN ORG.Department DEP ON DEP.Id=PP.DepartmentId
LEFT OUTER join ORG.Division D ON D.Id=PP.DivisionId
LEFT OUTER JOIN ORG.Section S ON S.Id=PP.SectionId
LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=PP.SubSectionId
LEFT OUTER JOIN hkp.Process PS ON PS.Id=P.ProcessId
LEFT OUTER JOIN TRN.PositionWiseDesignationGroup PWDG ON PWDG.PDID=PWD.Id
LEFT OUTER JOIN hkp.DesignationGroup DG ON DG.Id=PWDG.DesignationGroupId
LEFT OUTER JOIN MST.DesignationMaster DM ON DM.DesignationGroupId=DG.Id
LEFT OUTER JOIN MST.DesignationMasterLegalDesignation LMD ON LMD.DesignationMasterId=DM.Id
LEFT OUTER JOIN TRN.PositionWiseDesignationGivenLegal PWDGL ON PWDGL.PDID=PWD.Id 
LEFT OUTER JOIN hkp.Designation DE ON DE.Id=PWDGL.DesignationId
LEFT OUTER JOIN hkp.LegalDesignation LD ON LD.Id=PWDGL.LegalDesignationId
where PWD.IsActive=1";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations
    }
}