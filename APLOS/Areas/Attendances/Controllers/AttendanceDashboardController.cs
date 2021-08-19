#region Using
using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.HumanResource.Attendance;
using Library.HumanResource.Payroll.Allowance;
using Library.Model.Employees;
using Library.Model.Setups;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.Setups;
using Library.ViewModel.Accounts;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class AttendanceDashboardController : BaseController
    {
        #region Constructor
        private readonly Library.HumanResource.Dashboard.HRDashboardService _HRDashboardService;

        public AttendanceDashboardController()
        {
            _HRDashboardService = new Library.HumanResource.Dashboard.HRDashboardService();
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region operation

        [HttpPost, Authorize]
        public ActionResult HROverAllStatusDynamic(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_HRDashboardService.HROverAllStatusDynamic(ChartColumnList, seq, identity.CompanyGroupId, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalOthersDetail(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.ModalOthersDetail(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
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
        public ActionResult ModalHRDailyLeaveStatusList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_HRDashboardService.ModalHRDailyLeaveStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
            //return Json(_HRDashboardService.ModalHRDailyLeaveStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
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
        public ActionResult ModalHRDailyLateStatusList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_HRDashboardService.ModalHRDailyLateStatusList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
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
        public ActionResult DrillDownAttnStatus(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.DrillDownAttnStatus(ChartColumnList, seq, identity.CompanyGroupId, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult OrgStructureListColList(string CompanyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.OrgStructureListColList(identity.CompanyGroupId, CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult DefaultAttnStatus(string hrDate, string EmplyeeTypeOrCategoryId, string PODirectIndirectStatus)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboardService.DefaultAttnStatus(identity.CompanyGroupId, hrDate, EmplyeeTypeOrCategoryId, PODirectIndirectStatus), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalOnRoleEmployeeList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string hrDate, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_HRDashboardService.ModalOnRoleEmployeeList(ChartColumnList, identity.CompanyGroupId, seq, hrDate, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        #endregion
    }
}