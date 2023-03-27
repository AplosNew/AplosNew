#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class BlackListController : BaseController
    {
       
        string TableName = "dbo.BlackList";


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public BlackListController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


    
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from dbo.BlackList where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select bl.*,FORMAT(bl.Date,'dd-MMM-yyyy')as BLDate,FORMAT(bl.BlacklistingDate,'dd-MMM-yyyy')as BLBlacklistingDate,EMP.EmployeeStatus,EMP.EmployeeName as ResponsiblePerson,EMP.EmployeeCode AS EmployeeCode,EMP.FatherName,EMP.MotherName,FORMAT(EMP.DOB,'dd-MMM-yyyy')as EMPDOB,PL.UserName Plant
                                  ,empl.EmployeeStatus as EmpStatus,empl.EmployeeName as ByWhom,empl.EmployeeCode AS EmpCode
                                  from dbo.BlackList bl left join dbo.EmployeeInformation EMP on bl.EmpSystemId=EMP.SystemId
                                  left join dbo.EmployeeInformation empl on empl.SystemId=bl.ByWhomId
								  left join ORG.Plant PL on PL.Id=EMP.PlantId
                                  WHERE " + strkey + " order by bl.Date desc ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "BlackList", out sID);
            return sID;
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where AadharNumber='" + data["AadharNumber"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same National Id already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = "BL" + GetPK();

                    dr["Date"] = data["Date"];
                    dr["AadharNumber"] = data["AadharNumber"];
                    dr["CompanyEmployeeOutsider"] = data["CompanyEmployeeOutsider"];
                    dr["EmpSystemId"] = data["EmpSystemId"];

                    dr["OutsiderName"] = data["OutsiderName"];
                    dr["OutsiderFatherName"] = data["OutsiderFatherName"];
                    dr["OutsiderMotherName"] = data["OutsiderMotherName"];
                    dr["Reason"] = data["Reason"];
                    dr["ByWhomId"] = data["ByWhomId"];
                    dr["BlacklistingDate"] = data["BlacklistingDate"];
                    dr["Remarks"] = data["Remarks"];

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["Date"] = data["Date"];
                    dr["AadharNumber"] = data["AadharNumber"];
                    dr["CompanyEmployeeOutsider"] = data["CompanyEmployeeOutsider"];
                    dr["EmpSystemId"] = data["EmpSystemId"];

                    dr["OutsiderName"] = data["OutsiderName"];
                    dr["OutsiderFatherName"] = data["OutsiderFatherName"];
                    dr["OutsiderMotherName"] = data["OutsiderMotherName"];
                    dr["Reason"] = data["Reason"];
                    dr["ByWhomId"] = data["ByWhomId"];
                    dr["BlacklistingDate"] = data["BlacklistingDate"];
                    dr["Remarks"] = data["Remarks"];

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data= data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }

        [HttpPost, Authorize]
        public ActionResult LoadAllEmpDetailsForSelection(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                        EMP.EmployeeName,EMP.EmployeeCode AS Code,EMP.FatherName,EMP.MotherName,FORMAT(EMP.DOB,'dd-MMM-yyyy') as EMPDOB,emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric,EMP.EmpPicPath,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation, EMP.NationalID,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            EMP.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.GroupID='" + identity.CompanyGroupId + @"' --and emp.EmployeeStatus='Active' and emp.EmpType='Local'
                   AND isnull(Emp.SystemID,'') not in (select isnull(EmpSystemId,'') from dbo.BlackList where Id='" + Id + @"')
                  order by EmployeeCodePreFix,EmployeeCodeNumeric";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult LoadAllByWhomDetailsForSelection(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                        EMP.EmployeeName,EMP.EmployeeCode AS Code,EMP.FatherName,EMP.MotherName,FORMAT(EMP.DOB,'dd-MMM-yyyy') as EMPDOB,emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric,EMP.EmpPicPath,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            EMP.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.EmployeeStatus='Active' and emp.EmpType='Local'
                   AND isnull(Emp.SystemID,'') not in (select isnull(ByWhomId,'') from dbo.BlackList where Id='" + Id + @"')
                  order by EmployeeCodePreFix,EmployeeCodeNumeric";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

    }
}