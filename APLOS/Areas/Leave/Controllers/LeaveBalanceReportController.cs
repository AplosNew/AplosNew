using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.Leave;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Biometrics;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Leave.Controllers
{
    public class LeaveBalanceReportController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private readonly IAttendanceManagementService _AttendanceManagementService;
        private readonly ILeaveTransectionService _leaveTransactionService;
        private DataSet dsRef;

        public LeaveBalanceReportController(
            ILeaveTransectionService leaveTransactionService,
              IMaternityLeavePolicyService LeavePolicyService,
               IAttendanceManagementService AttendanceManagementService,
            ISqlRepository sqlRepository
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _AttendanceManagementService = AttendanceManagementService;
            _sqlRepository = sqlRepository;
            _leaveTransactionService = leaveTransactionService;
        }

        #endregion Constructor

        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetEmp(string YearId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsLeaveBalance ep = new clsLeaveBalance();
                return Json(ep.GetEmp(identity.PlantId, identity.CompanyId, YearId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetLeaveBalance(string year, string empId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsLeaveBalance ep = new clsLeaveBalance();
                return Json(_leaveTransactionService.LoadGrdAllocatedLvDetails(identity.CompanyGroupId, identity.PlantId, empId, year), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        #region Report

        [HttpGet, Authorize]
        public ActionResult GetReport(ReportFormat reportFormat, string Year)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsLeaveBalance ep = new clsLeaveBalance();
                var reportFileName = "Leave Register Report";
                var workbook = ep.XlsLeaveBalanceRpt(identity.PlantId, identity.CompanyId, Year);
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName);

                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        #endregion

        #endregion -- Operations
    }
}