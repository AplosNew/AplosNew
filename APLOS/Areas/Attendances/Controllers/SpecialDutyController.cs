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

namespace Aplos.Areas.Attendances.Controllers
{
    public class SpecialDutyController : BaseController
    {

        string TableName = "dbo.SpecialDuty";

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public SpecialDutyController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetList(string workDate)
        {

            string sql = @"Select ISNULL(SD.IsApproved,0)IsApproved,SD.Id,E.SystemId EmpSystemId,E.EmployeeCode,E.EmployeeName,FORMAT(SD.WorkDate,'dd-MMM-yyyy')WorkDate,CONVERT(varchar(15),CAST(SD.Intime AS TIME),100) InTime
,CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100) OutTime,ISNULL(SD.InputMinute,0)InputMinute
,ABS(DATEDIFF(MINUTE, SD.InTime, SD.OutTime)) AS CalculatedMinute
,ApprovedMinute=CASE WHEN ISNULL(SD.InputMinute,0)<ABS(DATEDIFF(MINUTE, SD.InTime, SD.OutTime)) THEN ISNULL(SD.InputMinute,0) ELSE ABS(DATEDIFF(MINUTE, SD.InTime, SD.OutTime)) END
,LD.UserName LegalDesignation,DEPT.UserName AS Department ,DV.UserName AS Division,SC.UserName AS Section,SS.UserName SubSection
 ,FORMAT(E.DOJ,'dd-MMM-yyyy') DOJ,EC.UserName EmployeeCategory
,E.EmployeeStatus
from dbo.SpecialDuty SD
LEFT JOIN EmployeeInformation E ON E.SystemId=SD.EmpSystemId
LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=PR.DesignationId
LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
LEFT JOIN ORG.Department DEPT ON E.DepartmentId = DEPT.Id
LEFT JOIN ORG.Division DV ON E.DivisionId = DV.Id
LEFT JOIN ORG.Section SC ON E.SectionId = SC.Id
LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
LEFT JOIN ORG.SubSection SS ON SS.Id=E.SubSectionId
Where WorkDate='" + workDate + @"' AND ISNULL(SD.IsApproved,0)=0";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetSDApprovedData(string workDate)
        {

            string sql = @"Select ISNULL(SD.IsApproved,0)IsApproved,SD.Id,E.SystemId EmpSystemId,E.EmployeeCode,E.EmployeeName,FORMAT(SD.WorkDate,'dd-MMM-yyyy')WorkDate,CONVERT(varchar(15),CAST(SD.Intime AS TIME),100) InTime
,CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100) OutTime,ISNULL(SD.InputMinute,0)InputMinute,SD.CalculatedMinute,SD.ApprovedMinute
,LD.UserName LegalDesignation,DEPT.UserName AS Department ,DV.UserName AS Division,SC.UserName AS Section,SS.UserName SubSection
 ,FORMAT(E.DOJ,'dd-MMM-yyyy') DOJ,EC.UserName EmployeeCategory
,E.EmployeeStatus
from dbo.SpecialDuty SD
LEFT JOIN EmployeeInformation E ON E.SystemId=SD.EmpSystemId
LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=PR.DesignationId
LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
LEFT JOIN ORG.Department DEPT ON E.DepartmentId = DEPT.Id
LEFT JOIN ORG.Division DV ON E.DivisionId = DV.Id
LEFT JOIN ORG.Section SC ON E.SectionId = SC.Id
LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
LEFT JOIN ORG.SubSection SS ON SS.Id=E.SubSectionId
Where WorkDate='" + workDate + @"' AND ISNULL(SD.IsApproved,0)=1";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        
        [HttpPost]
        public JsonResult SaveSDData(List<Dictionary<string, object>> data, string workDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsChild;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("Select * from dbo.SpecialDuty Where WorkDate='" + workDate + "' AND ISNULL(IsApproved,0)=0", out dsChild, false, "1");

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count > 0)
                        {
                          
                            DataRow drmo = dv[0].Row;
                            drmo["IsApproved"] = true;
                            drmo["ApproveBy"] = identity.EmployeeId;
                            drmo["UpdatedBy"] = identity.Name;
                            drmo["UpdatedDate"] = DateTime.Now.ToString();
                            drmo["UpdatedFromIP"] = identity.IPAddress;
                            EditRow(drmo, item);
                        }
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsChild);
                }


                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
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

    }
}