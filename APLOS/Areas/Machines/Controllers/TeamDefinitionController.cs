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
    public class TeamDefinitionController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public TeamDefinitionController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [Authorize, HttpGet]
        public decimal GetAutoSequenceNo()
        {
            try
            {
                DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(Sequence),0) AS Sequence FROM TRN.TeamDefinition");
                if (dt.Rows.Count > 0)
                    return (decimal)clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

                return 1;
            }
            catch (Exception ex)
            {
                return 1.00M;
            }
        }

        [Authorize, HttpGet]
        public decimal GetCategorySequenceNo()
        {
            try
            {
                DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(Sequence),0) AS Sequence FROM HKP.EmployeeActivityCategory");
                if (dt.Rows.Count > 0)
                    return (decimal)clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

                return 1;
            }
            catch (Exception ex)
            {
                return 1.00M;
            }
        }

        [Authorize, HttpGet]
        public decimal GetTeamCategorySequenceNo()
        {
            try
            {
                DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(Sequence),0) AS Sequence FROM HKP.TeamCategory");
                if (dt.Rows.Count > 0)
                    return (decimal)clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

                return 1;
            }
            catch (Exception ex)
            {
                return 1.00M;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetEActivityCategoryList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            
            var sql = @"select Id as Value,UserName as Text from HKP.EmployeeActivityCategory";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=MB.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
                            WHERE EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadTeamLeaderList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * ,(select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=TD.TeamLeaderId) as TeamLeader FROM [TRN].[TeamDefinition] TD order by TD.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadTeamDefinitionEditData(string TeamID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT * ,(select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=TD.TeamLeaderId) as TeamLeader FROM [TRN].[TeamDefinition] TD where TD.Id='" + TeamID + @"'";
            return Json(new { team = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(Dictionary<string, object> TeamData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from TRN.TeamDefinition where ShortName='" + TeamData["ShortName"] + "'", out DataSet dsTeamShortNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from TRN.TeamDefinition where StandardName='" + TeamData["StandardName"] + "'", out DataSet dsTeamStandardNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from TRN.TeamDefinition where UserName='" + TeamData["UserName"] + "'", out DataSet dsTeamUserNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from TRN.TeamDefinition where Code='" + TeamData["Code"] + "'", out DataSet dsTeamCodeValidation, false, "1");

                DataSet dsTeamDefinition;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[TeamDefinition] where Id='" + TeamData["Id"] + "'", out dsTeamDefinition, false, "1");
                string _Id = "";

                #region data update
                if (dsTeamDefinition.Tables[0].Rows.Count == 0)
                {
                    if (dsTeamShortNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Short Name Already Exist.");
                    }
                    else if (dsTeamStandardNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Standared Name Already Exist.");
                    }
                    else if (dsTeamUserNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("User Name Already Exist.");
                    }
                    else if (dsTeamCodeValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Code Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("TeamDefinition", out _Id);
                        _Id = "TD" + _Id;
                        TeamData["Id"] = _Id;
                        AddNewRow(dsTeamDefinition.Tables[0], TeamData);
                    }
                }
                else
                {
                    _Id = TeamData["Id"].ToString();
                    EditRow(dsTeamDefinition.Tables[0].Rows[0], TeamData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsTeamDefinition);

                return Json(new { Error = false, Data = TeamData, Message = AplosMessage.Insert });
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

        [Authorize, HttpPost]
        public ActionResult TeamDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                //ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                //DataSet AssetCount, ItemCount, StoresCount, BudgetCount;

                //conRack = new ConnectionManager.DAL.ConManager("1");
                //conRack.OpenDataSetThroughAdapter("select * from [TRN].[MaintenanceMachineAsset] where MaintenanceSchedulingId='" + id + "'", out AssetCount, false, "1");
                //conRack.OpenDataSetThroughAdapter("select * from [TRN].[MaintenanceItem] where MaintenanceSchedulingId ='" + id + "'", out ItemCount, false, "1");
                //conRack.OpenDataSetThroughAdapter("select * from [TRN].[MaintenanceStoresConsumable] where MaintenanceSchedulingId ='" + id + "'", out StoresCount, false, "1");
                //conRack.OpenDataSetThroughAdapter("select * from [TRN].[MaintenancePersonBudgetCode] where MaintenanceSchedulingId ='" + id + "'", out BudgetCount, false, "1");

                //if (AssetCount.Tables[0].Rows.Count == 0 || ItemCount.Tables[0].Rows.Count == 0 || StoresCount.Tables[0].Rows.Count == 0 || BudgetCount.Tables[0].Rows.Count == 0)
                //{

                    conC.BeginTransaction();
                    conC.executeQuery("delete from [TRN].[TeamDefinition] where Id ='" + id + @"'");
                    conC.CommitTransaction();
                //}
                //else
                //{
                //    throw new Exception("Transaction are Exists!");
                //}
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult EACategoryDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                
                conC.BeginTransaction();
                conC.executeQuery("delete from HKP.EmployeeActivityCategory where Id ='" + id + @"'");
                conC.CommitTransaction();
               
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult TeamCategoryDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();

                conC.BeginTransaction();
                conC.executeQuery("delete from HKP.TeamCategory where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadBudgetCodeDetails(string TeamId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select distinct CAST (CASE WHEN TBC.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,TBC.Id,MP.Id BudgetCodeId, MP.Code, E.UserName Entity, P.UserName Position,P.Activity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection from MST.ManpowerBudget MP
                            left join ORG.Entity E on E.Id = MP.EntityId
                            left join ORG.Position P on P.Id = MP.PositionId
							left join EmployeeInformation EI on EI.BudgetCode=MP.Id and EI.EmployeeStatus='Active'
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
							LEFT JOIN TRN.TeamBudgetCode TBC ON TBC.BudgetCodeId=MP.Id and TBC.TeamDefinitionId='"+ TeamId + @"'
                            where MP.Active = 1 
							order by TBC.Id  desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadEntityDetails(string TeamId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN TE.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,TE.Id,E.Id EntityId,E.EntityType,E.UserName Entity,E.Code,TE.Remarks 
                            from ORG.Entity E
							LEFT JOIN TRN.TeamEntity TE ON TE.EntityId=E.Id and TE.TeamDefinitionId='" + TeamId + @"'
                            where E.Active = 1 order by TE.EntityId  desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadTeamDefinitionCategoryDetails(string TeamId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN TDC.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,TDC.Id,TC.Id TeamCategoryId,TC.ShortName,TC.StandardName,TC.UserName,TC.Code,TDC.Remarks 
                            from HKP.TeamCategory TC
							LEFT JOIN TRN.TeamDefinitionCategory TDC ON TDC.TeamCategoryId=TC.Id and TDC.TeamDefinitionId='" + TeamId + @"'
                            where TC.Active = 1 order by TDC.TeamCategoryId  desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadEACategoryDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * from HKP.EmployeeActivityCategory";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadTeamCategoryDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * from HKP.TeamCategory";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadEACategoryEditData(string CategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT * from HKP.EmployeeActivityCategory where Id ='" + CategoryId + @"'";
            return Json(new { category = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadTeamCategoryEditData(string TeamCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT * from HKP.TeamCategory where Id ='" + TeamCategoryId + @"'";
            return Json(new { teamcategory = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadEmployeeDetails(string TeamId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"Select CAST (CASE WHEN TDE.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,TDE.Id,MB.Code Budgetcode,EI.SystemId as EmployeeId,EI.EmployeeName as EmployeeName,DEG.UserName as Designation,TDE.PlanHours,TDE.ResponsibilityLevel,TDE.EmployeeActiviyCategory,
TDE.Remarks,SD.UserName as Shift,S.UserName as Section,SS.UserName as SubSection,DEP.UserName AS Department,format(EI.DOJ,'dd-MMM-yyyy') as DOJ,EI.EmployeeStatus,EI.EmployeeCurrentStatus EmplCurrentStatus,P.Activity EmployeeActivity,EC.UserName EmployeeCategory,EI.BudgetCode as BudgetCodeId
FROM dbo.EmployeeInformation AS EI
LEFT JOIN [MST].[ManpowerBudget]  MB ON MB.Id=EI.BudgetCode
left join ORG.Position P on P.Id = MB.PositionId
LEFT JOIN ShiftDefination SD ON SD.SystemID=MB.ShiftDefinationId
LEFT JOIN HKP.Designation DEG ON DEG.Id=P.DesignationID
LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
left join MST.DesignationMaster DM on DM.DesignationId = P.DesignationId
left join HKP.EmployeeCategory EC on EC.Id=DM.EmployeeCategoryId
left Join [TRN].[TeamDefinitionEmployee] TDE ON TDE.EmployeeId=EI.SystemId and TDE.TeamDefinitionId='" + TeamId + @"'
where EI.EmployeeStatus='Active' and EI.BudgetCode in (select BudgetCodeId from [TRN].[TeamBudgetCode] where TeamDefinitionId='" + TeamId + "') order by TDE.Id  desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult createBudgetCode(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "TRN.TeamBudgetCode";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                    conC.BeginTransaction();
                    conC.executeQuery("delete from " + TableName + " where TeamDefinitionId='" + Pid + "'");
                    conC.CommitTransaction();
                  
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and TeamDefinitionId='" + item["TeamDefinitionId"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "TBC" + _Id;
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

                    ConnectionManager.clsConnection conC1 = new ConnectionManager.clsConnection();
                    conC1.BeginTransaction();
                    conC1.executeQuery("delete from [TRN].[TeamDefinitionEmployee] where TeamDefinitionId = '" + Pid + "' and BudgetCodeId not in (select BudgetCodeId from[TRN].[TeamBudgetCode] where TeamDefinitionId = '" + Pid + "')");
                    conC1.CommitTransaction();
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpPost]
        public ActionResult createEntity(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "TRN.TeamEntity";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                    conC.BeginTransaction();
                    conC.executeQuery("delete from " + TableName + " where TeamDefinitionId='" + Pid + "'");
                    conC.CommitTransaction();

                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and TeamDefinitionId='" + item["TeamDefinitionId"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "TE" + _Id;
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

        [Authorize, HttpPost]
        public ActionResult createTeamDefinitionCategory(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "TRN.TeamDefinitionCategory";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");

                if (DataList != null)
                {
                    ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                    conC.BeginTransaction();
                    conC.executeQuery("delete from " + TableName + " where TeamDefinitionId='" + Pid + "'");
                    conC.CommitTransaction();

                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and TeamDefinitionId='" + item["TeamDefinitionId"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "TDC" + _Id;
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

        [HttpPost, Authorize]
        public JsonResult createEACategory(Dictionary<string, object> EACategoryData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from HKP.EmployeeActivityCategory where ShortName='" + EACategoryData["ShortName"] + "'", out DataSet dsTeamShortNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from HKP.EmployeeActivityCategory where StandardName='" + EACategoryData["StandardName"] + "'", out DataSet dsTeamStandardNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from HKP.EmployeeActivityCategory where UserName='" + EACategoryData["UserName"] + "'", out DataSet dsTeamUserNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from HKP.EmployeeActivityCategory where Code='" + EACategoryData["Code"] + "'", out DataSet dsTeamCodeValidation, false, "1");

                DataSet dsEmployeeActivityCategory;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from HKP.EmployeeActivityCategory where Id='" + EACategoryData["Id"] + "'", out dsEmployeeActivityCategory, false, "1");
                string _Id = "";

                #region data update
                if (dsEmployeeActivityCategory.Tables[0].Rows.Count == 0)
                {
                    if (dsTeamShortNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Short Name Already Exist.");
                    }
                    else if (dsTeamStandardNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Standared Name Already Exist.");
                    }
                    else if (dsTeamUserNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("User Name Already Exist.");
                    }
                    else if (dsTeamCodeValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Code Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("EmployeeActivityCategory", out _Id);
                        _Id = "EAC" + _Id;
                        EACategoryData["Id"] = _Id;
                        AddNewRow(dsEmployeeActivityCategory.Tables[0], EACategoryData);
                    }
                }
                else
                {
                    _Id = EACategoryData["Id"].ToString();
                    EditRow(dsEmployeeActivityCategory.Tables[0].Rows[0], EACategoryData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEmployeeActivityCategory);

                return Json(new { Error = false, Data = EACategoryData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public JsonResult createTeamCategory(Dictionary<string, object> TeamCategoryData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from HKP.TeamCategory where ShortName='" + TeamCategoryData["ShortName"] + "'", out DataSet dsTeamShortNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from HKP.TeamCategory where StandardName='" + TeamCategoryData["StandardName"] + "'", out DataSet dsTeamStandardNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from HKP.TeamCategory where UserName='" + TeamCategoryData["UserName"] + "'", out DataSet dsTeamUserNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from HKP.TeamCategory where Code='" + TeamCategoryData["Code"] + "'", out DataSet dsTeamCodeValidation, false, "1");

                DataSet dsTeamCategory;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from HKP.TeamCategory where Id='" + TeamCategoryData["Id"] + "'", out dsTeamCategory, false, "1");
                string _Id = "";

                #region data update
                if (dsTeamCategory.Tables[0].Rows.Count == 0)
                {
                    if (dsTeamShortNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Short Name Already Exist.");
                    }
                    else if (dsTeamStandardNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Standared Name Already Exist.");
                    }
                    else if (dsTeamUserNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("User Name Already Exist.");
                    }
                    else if (dsTeamCodeValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Code Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("TeamCategory", out _Id);
                        _Id = "TC" + _Id;
                        TeamCategoryData["Id"] = _Id;
                        AddNewRow(dsTeamCategory.Tables[0], TeamCategoryData);
                    }
                }
                else
                {
                    _Id = TeamCategoryData["Id"].ToString();
                    EditRow(dsTeamCategory.Tables[0].Rows[0], TeamCategoryData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsTeamCategory);

                return Json(new { Error = false, Data = TeamCategoryData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpPost]
        public ActionResult createEmployee(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "TRN.TeamDefinitionEmployee";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                    conC.BeginTransaction();
                    conC.executeQuery("delete from " + TableName + " where TeamDefinitionId='" + Pid + "'");
                    conC.CommitTransaction();

                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and TeamDefinitionId='" + item["TeamDefinitionId"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "TDE" + _Id;
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

        #endregion -- Operations
    }
}