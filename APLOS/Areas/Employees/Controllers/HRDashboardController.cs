#region Using

using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Service.Employees;
using Library.ViewModel.Accounts;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
    public class HRDashboardController : BaseController
    {
        #region Constructor

        private readonly Library.HumanResource.Dashboard.HRDashboardService _HRDashboardService;

        public HRDashboardController()
        {
            _HRDashboardService = new Library.HumanResource.Dashboard.HRDashboardService();
        }

        #endregion Constructor
        public ActionResult Aplos()
        {
            return View();
        }
        [HttpGet, Authorize]
        public ActionResult HROverAllStatusDefault(string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_HRDashboardService.HROverAllStatusDefault(identity.CompanyGroupId, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult HROverAllStatusDynamic(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_HRDashboardService.HROverAllStatusDynamic(ChartColumnList, seq, identity.CompanyGroupId, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult HRLongAbsentismDefault(string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.HRLongAbsentismDefault(identity.CompanyGroupId, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult ConsecutiveLateStatsDynamic(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ConsecutiveLateStatsDynamic(ChartColumnList, seq, identity.CompanyGroupId, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ConsecutiveAbsentStatsDynamic(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ConsecutiveAbsentStatsDynamic(ChartColumnList, seq, identity.CompanyGroupId, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ConsecutivePresentStatusDynamic(string CompanyId, string hrFromDate, string hrToDate, string dayCount,string presentComparator)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_HRDashboardService.ModalConsecutivePresentDateList( identity.CompanyGroupId, identity.CompanyId, identity.PlantId, hrFromDate, hrToDate, dayCount , presentComparator), JsonRequestBehavior.AllowGet);

            var jsondata = Json(_HRDashboardService.ModalConsecutivePresentDateList(identity.CompanyGroupId, CompanyId, identity.PlantId, hrFromDate, hrToDate, dayCount, presentComparator), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;

            return jsondata;
         
        }


        [HttpPost, Authorize]
        public ActionResult GetEEmpJobCardInfoWithInDateTimes(string companyId, string wrHrFromDate, string wrHrToDate, string hours, string presentComparator)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_HRDashboardService.GetEEmpJobCardInfoWithInDateTimes(wrHrFromDate,wrHrToDate, companyId, hours, presentComparator), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;

            return jsondata;
            //return Json(_HRDashboardService.GetEEmpJobCardInfoWithInDateTimes(wrHrDate, companyId, hours, presentComparator), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult PrintPresent(string hrFromDate, string hrToDate, string dayCount, string presentComparator)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = "attdStatus" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                var workbook = _HRDashboardService.GetEmployeePresentStatusReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, hrFromDate, hrToDate, dayCount, presentComparator,identity.UserId);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpGet, Authorize]
        public ActionResult OrgStructureListColList(string CompanyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.OrgStructureListColList(identity.CompanyGroupId, CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult JoiningStatusDaily(string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_HRDashboardService.JoiningStatusDaily(identity.CompanyGroupId, identity.CompanyId, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult DynamicJoiningOrSeparationStatusDaily(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.DynamicJoiningOrSeparationStatusDaily(ChartColumnList, seq, identity.CompanyGroupId, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult AbsentismStatusDaily(string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_HRDashboardService.AbsentismStatusDaily(identity.CompanyGroupId, identity.CompanyId, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult DynamicAbsentismStatusDaily(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.DynamicAbsentismStatusDaily(ChartColumnList, seq, identity.CompanyGroupId, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }

        

        [HttpPost, Authorize]
        public ActionResult DynamicLeaveStatus(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.DynamicLeaveStatus(ChartColumnList, seq, identity.CompanyGroupId, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult ListProbationOverDue(IEnumerable<ChartColumnList> ChartColumnList, int seq, string condition, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var jsondata = Json(_HRDashboardService.ListProbationOverDue(ChartColumnList, seq, condition, identity.CompanyGroupId, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;


            return jsondata;
        }


        [HttpPost, Authorize]
        public ActionResult ListSeparationStatus(IEnumerable<ChartColumnList> ChartColumnList, int seq, string condition, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ListSeparationStatus(ChartColumnList, seq, condition, identity.CompanyGroupId, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult DefaultAttnStatus(string hrDate, string EmplyeeTypeOrCategoryId,string PODirectIndirectStatus)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.DefaultAttnStatus(identity.CompanyGroupId, hrDate, EmplyeeTypeOrCategoryId, PODirectIndirectStatus), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DrillDownAttnStatus(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.DrillDownAttnStatus(ChartColumnList, seq, identity.CompanyGroupId, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalHRDailyPresentStatusList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_HRDashboardService.ModalHRDailyPresentStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult ModalOnRoleEmployeeList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_HRDashboardService.ModalOnRoleEmployeeList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult ModalHRDailyAbsentStatusList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_HRDashboardService.ModalHRDailyAbsentStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult ModalLongAbsenteismStatusList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_HRDashboardService.ModalLongAbsenteismStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult ModalHRDailyLateStatusList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_HRDashboardService.ModalHRDailyLateStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult ModalHRDailyLeaveStatusList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_HRDashboardService.ModalHRDailyLeaveStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
            //return Json(_HRDashboardService.ModalHRDailyLeaveStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult ModalHRDailyLateInStatusList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_HRDashboardService.ModalHRDailyLateInStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
            //return Json(_HRDashboardService.ModalHRDailyLeaveStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult ModalHRDailyEarlyOutStatusList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_HRDashboardService.ModalHRDailyEarlyOutStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
            //return Json(_HRDashboardService.ModalHRDailyLeaveStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult ModalHRDailyLunchOutStatusList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_HRDashboardService.ModalHRDailyLunchOutStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
            //return Json(_HRDashboardService.ModalHRDailyLeaveStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalHRDailyShiftNotAssignedStatusList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_HRDashboardService.ModalHRDailyShiftNotAssignedStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
            //return Json(_HRDashboardService.ModalHRDailyShiftNotAssignedStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalHRDailyAttdnNotProcessedStatusList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_HRDashboardService.ModalHRDailyAttdnNotProcessedStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
            //return Json(_HRDashboardService.ModalHRDailyAttdnNotProcessedStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }
        //[HttpPost, Authorize]
        //public ActionResult ModalHRDailyLateStatusList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    var jsondata = Json(_HRDashboardService.ModalHRDailyLateStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        //    jsondata.MaxJsonLength = int.MaxValue;
        //    return jsondata;
        //}

        [HttpPost, Authorize]
        public ActionResult ModalHRDailyOffDayStatusList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ModalHRDailyOffDayStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalOthersDetail(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ModalOthersDetail(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult ConsecutiveAbsentStats(string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ConsecutiveAbsentStats(hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult ConsecutiveLateStats(string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ConsecutiveLateStats(hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalConsecutiveLateStats(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ModalConsecutiveLateStats(ChartColumnList, seq, identity.CompanyGroupId, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalConsecutiveAbsentStats(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ModalConsecutiveAbsentStats(ChartColumnList, seq, identity.CompanyGroupId, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ListOfIncrementDue(IEnumerable<ChartColumnList> ChartColumnList, int seq, string condition, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ListOfIncrementDue(ChartColumnList, seq, condition, identity.CompanyGroupId, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult LeaveStatus(string companyGroupId, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.LeaveStatus(identity.CompanyGroupId, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult DateWiseAbsentList(string companyId, string hrFromDate, string hrToDate, string dayCount, string comparator)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_HRDashboardService.DateWiseAbsentList(identity.CompanyGroupId, companyId, hrFromDate, hrToDate, dayCount, comparator), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
            //return Json(_HRDashboardService.DateWiseAbsentList(identity.CompanyGroupId, companyId, plantId, hrFromDate, hrToDate, dayCount, comparator), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetReportingPersonCbo(string companyId, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.GetReportingPersonCbo(identity.CompanyGroupId, companyId, plantId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult DateWiseLatetListStatus(string companyId,  string hrFromDate, string hrToDate, string dayCount, string comparator)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_HRDashboardService.DateWiseLatetListStatus(identity.CompanyGroupId, companyId,  hrFromDate, hrToDate, dayCount, comparator), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
            //return Json(_HRDashboardService.DateWiseLatetListStatus(identity.CompanyGroupId, companyId, plantId, hrFromDate, hrToDate, dayCount, comparator), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DateJoiningStatus(string companyId, string plantId, string hrFromDate, string hrToDate, string dayCount, string comparator)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.DateJoiningStatus(identity.CompanyGroupId, companyId, plantId, hrFromDate, hrToDate, dayCount, comparator), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DateSepartaionStatus(string companyId, string plantId, string hrFromDate, string hrToDate, string dayCount, string comparator)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.DateSepartaionStatus(identity.CompanyGroupId, companyId, plantId, hrFromDate, hrToDate, dayCount, comparator), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult ROPersonWiseAttnStatus(string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ROPersonWiseAttnStatus(identity.CompanyGroupId, companyId, plantId, hrFromDate, hrToDate, reportingPersonId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult ROPersonWisePresentStatusList(string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ROPersonWisePresentStatusList(identity.CompanyGroupId, companyId, plantId, hrFromDate, hrToDate, reportingPersonId, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult ROPersonWiseAbsentStatusList(string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ROPersonWiseAbsentStatusList(identity.CompanyGroupId, companyId, plantId, hrFromDate, hrToDate, reportingPersonId, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult ROPersonWiseLatetStatusList(string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ROPersonWiseLatetStatusList(identity.CompanyGroupId, companyId, plantId, hrFromDate, hrToDate, reportingPersonId, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult ROPersonWiseLeavetStatusList(string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ROPersonWiseLeavetStatusList(identity.CompanyGroupId, companyId, plantId, hrFromDate, hrToDate, reportingPersonId, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult ROPersonWiseWeekOffHolidayStatusList(string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ROPersonWiseWeekOffHolidayStatusList(identity.CompanyGroupId, companyId, plantId, hrFromDate, hrToDate, reportingPersonId, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult ModalROPHRDailyShiftNotAssignedStatusList(string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ModalROPHRDailyShiftNotAssignedStatusList(identity.CompanyGroupId, companyId, plantId, hrFromDate, hrToDate, reportingPersonId, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult ModalROHRDailyAttdnNotProcessedStatusList(string companyId, string plantId, string hrFromDate, string hrToDate, string reportingPersonId, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ModalROHRDailyAttdnNotProcessedStatusList(identity.CompanyGroupId, companyId, plantId, hrFromDate, hrToDate, reportingPersonId, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult ModalEmployeeWiseAbsentDateList(string companyId, string plantId, string hrFromDate, string hrToDate, string employeeCode, string dayCount, string comparator)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ModalEmployeeWiseAbsentDateList(identity.CompanyGroupId, companyId, plantId, hrFromDate, hrToDate, employeeCode, dayCount, comparator), JsonRequestBehavior.AllowGet);
        }



        [HttpGet, Authorize]
        public ActionResult ModalEmployeeWiseLateDateList(string companyId, string plantId, string hrFromDate, string hrToDate, string employeeCode, string dayCount, string comparator)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ModalEmployeeWiseLateDateList(identity.CompanyGroupId, companyId, plantId, hrFromDate, hrToDate, employeeCode, dayCount, comparator), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult ModalEmployeeWisePresentStatusDateWiseList(string companyId, string plantId, string hrFromDate, string hrToDate, string EmpSystemId, string dayCount, string comparator)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ModalEmployeeWisePresentStatusDateWiseList(identity.CompanyGroupId, companyId, plantId, hrFromDate, hrToDate, EmpSystemId, dayCount, comparator), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult ModalEmployeeWisePresentDateList(string companyId, string plantId, string hrFromDate, string hrToDate, string EmpSystemId, string dayCount, string comparator)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ModalEmployeeWisePresentDateList(identity.CompanyGroupId, companyId, plantId, hrFromDate, hrToDate, EmpSystemId, dayCount, comparator), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult ModalConsecutiveAbsentDateList(string companyId, string plantId, string empSystemID, string hrDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ModalConsecutiveAbsentDateList(identity.CompanyGroupId, companyId, plantId, empSystemID, hrDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult ModalConsecutiveLateDateList(string companyId, string plantId, string empSystemID, string hrDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ModalConsecutiveLateDateList(identity.CompanyGroupId, companyId, plantId, empSystemID, hrDate), JsonRequestBehavior.AllowGet);
        }


        #region Excel Reports
        [HttpPost, Authorize]
        public ActionResult GetchartColumnList(IEnumerable<ChartColumnList> chartColumnList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.GetChartColumnList(chartColumnList), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetDailyAbsentReportfromDashboardData(IEnumerable<ChartColumnList> chartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "AbsentList" + hrDate + ".xls";
            var workbook = _HRDashboardService.HRDailyAbsentStatusListForExcel(chartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId);
            workbook.Version = ExcelVersion.Excel97to2003;
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
            return null;
        }//DateWiseAbsentListInExcel

        [HttpGet, Authorize]
        public ActionResult DateWiseAbsentListInExcel(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string dayCount, string comparator)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "AbsentList" + hrFromDate + ".xls";
            var workbook = _HRDashboardService.DateWiseAbsentListInExcel(identity.CompanyGroupId, companyId, plantId, hrFromDate, hrToDate, dayCount, comparator);
            workbook.Version = ExcelVersion.Excel97to2003;
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
            return null;
        }//DateWiseAbsentListInExcel

        [HttpGet, Authorize]
        public ActionResult DateWiseJoiningListInExcel(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string dayCount, string comparator)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "JoiningList" + hrFromDate + ".xls";
            var workbook = _HRDashboardService.DateWiseJoiningListInExcel(identity.CompanyGroupId, companyId, plantId, hrFromDate, hrToDate, dayCount, comparator);
            workbook.Version = ExcelVersion.Excel97to2003;
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
            return null;
        }//DateWiseAbsentListInExcel
        #endregion
    }
}