using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Payroll.Allowance;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.HumanResources;
using Syncfusion.XlsIO;
using System;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.HumanResources.PayRegisterBDReportService;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class DailyDayStatusController : BaseController
    {


        #region Constructor

        private readonly IManpowerAttendanceSummary _manpowerAttendanceSummary;

        public DailyDayStatusController(
              IManpowerAttendanceSummary manpowerAttendanceSummary
            )
        {
            _manpowerAttendanceSummary = manpowerAttendanceSummary;
        }

        #endregion Constructor

        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        [Authorize]
        public JsonResult GetSectionCboByDepartment(string deptID)
        {
            return Json(_manpowerAttendanceSummary.GetSectionCboByDepartment(deptID), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult GetSubSectionCboBySection(string secID)
        {
            return Json(_manpowerAttendanceSummary.GetSubSectionCboBySection(secID), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult GetLineCboBySubSection(string subsecID)
        {
            return Json(_manpowerAttendanceSummary.GetLineCboBySubSection(subsecID), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult GetAttendanceDayStatus()
        {
            return Json(_manpowerAttendanceSummary.GetAttendanceDayStatus(), JsonRequestBehavior.AllowGet);
        }

        //[HttpPost, Authorize]
        //public ActionResult GetDailyDayStatusReport(string workDate,  string sDepID, string PrevWorkDate,string sSecID, string sSubSecID, string sLineID,string dayStatus,string Dep,string Sec,string employeeCategory,string shift,string entity)
        //{

        //    try
        //    {
        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        var fileName = "DailyDayStatus" + DateTime.Now.ToString("yyMMdd") + ".xls";
        //        var workbook = _manpowerAttendanceSummary.ExcelDailyDayStatus(identity.PlantId, PrevWorkDate, identity.CompanyId, workDate, sDepID, sSecID, sSubSecID, sLineID, dayStatus, Dep, Sec,employeeCategory,shift,entity);
        //        workbook.Version = ExcelVersion.Excel2016;
        //        workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);

        //        // return null;
        //        return Json(new { fileName, Error = false }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //      throw  ex;
        //        //return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

        //    }

        //}


        [HttpGet, Authorize]
        public ActionResult GetDailyDayStatusReport(ReportFormat reportFormat, string workDate, string sDepID, string PrevWorkDate, string sSecID, string sSubSecID, string sLineID, string dayStatus, string Dep, string Sec, string employeeCategory, string shift, string entity)
        {
            try
            {
                string Dstatus = "'" + dayStatus.Replace(",", "','") + "'";//replaced with ""
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var reportFileName = "DailyDayStatus";
                var workbook = _manpowerAttendanceSummary.ExcelDailyDayStatus(identity.PlantId, PrevWorkDate, identity.CompanyId, workDate, sDepID, sSecID, sSubSecID, sLineID, Dstatus, Dep, Sec, employeeCategory, shift, entity);
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName);

                    case ReportFormat.Excel:
                        //return RenderReportAsExcel(workbook, reportFileName);
                        workbook.SaveAs(reportFileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
                        return null;
                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }

            }
            catch (Exception ex)
            {
                // throw ex;
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
            //workbook.Version = ExcelVersion.Excel97to2003;
            //workbook.SaveAs(reportFileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
            //
        }

        [HttpGet, Authorize]
        public ActionResult GetDailyDayStatusReportView(string workDate, string sDepID, string PrevWorkDate, string sSecID, string sSubSecID, string sLineID, string dayStatus, string Dep, string Sec, string employeeCategory, string shift, string entity)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var fileName = "DailyDayStatus";
                var workbook = _manpowerAttendanceSummary.ExcelDailyDayStatus(identity.PlantId, PrevWorkDate, identity.CompanyId, workDate, sDepID, sSecID, sSubSecID, sLineID, dayStatus, Dep, Sec, employeeCategory, shift, entity);
                workbook.Version = ExcelVersion.Excel2016;
                //workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);

                return RenderReportAsPdf(workbook, fileName);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }


        }

        // #endregion -- Operations

        #region Get 
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

        [HttpGet, Authorize]
        public ActionResult GetEmployeeCategoryList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsDiscreteAllowanceReport ep = new clsDiscreteAllowanceReport();
                return Json(ep.GetEmployeeCategoryList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetEntityList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsDiscreteAllowanceReport ep = new clsDiscreteAllowanceReport();
                return Json(ep.GetEntityList(identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetSection(string DeptId)
        {
            try
            {
                string Dept = "'" + DeptId.Replace(",", "','") + "'";//replaced with ""
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsDiscreteAllowanceReport ep = new clsDiscreteAllowanceReport();
                return Json(ep.GetSection(Dept), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetSubSection(string SecId)
        {
            try
            {
                string ssub = "'" + SecId.Replace(",", "','") + "'";//replaced with ""
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsDiscreteAllowanceReport ep = new clsDiscreteAllowanceReport();
                return Json(ep.GetSubSection(ssub), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetDeptListList(string EntityId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsDiscreteAllowanceReport ep = new clsDiscreteAllowanceReport();
                return Json(ep.GetDeptListList(EntityId,identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        #endregion


    }
}