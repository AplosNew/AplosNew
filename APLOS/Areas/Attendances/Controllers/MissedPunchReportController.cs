using Aplos.Controllers;
using Aplos.Helpers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Payroll.Allowance;
using Library.HumanResource.Report.Attendance;
using Library.Model.HumanResources;
using Library.Service.HumanResources;
using Syncfusion.XlsIO;
using System;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.HumanResources.PayRegisterBDReportService;

namespace Aplos.Areas.Attendances.Controllers
{
    public class MissedPunchReportController : BaseController
    {
        #region Constructor

        private readonly IManpowerAttendanceSummary _manpowerAttendanceSummary;
        private readonly ISqlRepository _sqlRepository;
        public MissedPunchReportController(ISqlRepository sqlRepository,
              IManpowerAttendanceSummary manpowerAttendanceSummary
            )
        {
            _manpowerAttendanceSummary = manpowerAttendanceSummary;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages

        
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages


        [HttpGet, Authorize]
        public JsonResult GetData(string date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var str = @"Select distinct isnull(E.UserName, '') as Entity
                        , dep.UserName as Department
                        ,LG.Id DesignationId,LG.UserName Designation
                        ,ec.UserName EmpCategory
                        ,ec.Id EmpCategoryId 
                        ,sec.UserName as Section
                        ,ssec.UserName as SubSection
                        ,ISNULL( L.UserName,'') Line
                        ,L.Id LineId
                        ,E.Id as EntityId 
                        ,dep.Id as DepId 
                        ,sec.Id as SecId 
                        ,ssec.Id as SubSecId,J.JobLocation,J.SystemID JobLocationId
                        from org.Position p
                        left join mst.ManpowerBudget mpb on mpb.PositionId = p.Id
                        left join org.Entity e on e.Id = mpb.EntityId
                        left join org.Section sec on sec.id = p.SectionId
                        left join org.SubSection ssec on ssec.Id = p.SubSectionId
                        left join org.Department dep on dep.Id = p.DepartmentId
						LEFT JOIN  (select distinct LegalDesignationId,BudgetCode,DOJ,DOS,JobLocationID from  dbo.EmployeeInformation ) ei on ei.BudgetCode = mpb.Id 
						left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
						left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
						left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
						left join hkp.LegalDesignation LG on LG.Id = ei.LegalDesignationId
                        LEFT JOIN org.Line L ON L.Id = mpb.LineId
                        LEFT JOIN JobLocation J ON J.SystemID = ei.JobLocationID
						where ei.BudgetCode is not null
                        and e.PlantId='" + identity.PlantId + @"' and ei.DOJ <= ( '" + date + @"') and (ei.DOS is null or ei.DOS >= '" + date + @"')";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetShift()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsDiscreteAllowanceReport ep = new clsDiscreteAllowanceReport();
                return Json(ep.GetShift(identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        
        [HttpPost,Authorize]
        public JsonResult GetMissedPunchReport(string workDate, string sDepID, string sSecID, string sSubSecID, string sLineID, bool chkIntime, bool chkoutTime, string shiftList, string JobLocation,string designationList,string enttyList,string empCategoryList,bool WithFatherName)
        {
            string LineId = string.Empty;
            if (sLineID.Contains("'null'"))
            {
                LineId = sLineID.Replace("'null'", "''");
            }
            else
            {
                LineId = sLineID;
            }
            string ShiftId = "'" + shiftList.Replace(",", "','") + "'";//replaced with ""

            clsMissedPunchReport mpr = new clsMissedPunchReport();
            var workbook = mpr.MissedPunchReport(out ExcelEngine excelEngine, workDate, sDepID, sSecID, sSubSecID, LineId, chkIntime, chkoutTime, ShiftId, JobLocation,designationList,enttyList,empCategoryList,WithFatherName);
            return Json(new { FileName = workbook, Error = false }, JsonRequestBehavior.AllowGet);
            //return excelEngine.SaveAsActionResult(workbook, "MissedPunchReport" + workDate + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog, ExcelHttpContentType.Excel2013);
        }
        // #endregion -- Operations
    }
}