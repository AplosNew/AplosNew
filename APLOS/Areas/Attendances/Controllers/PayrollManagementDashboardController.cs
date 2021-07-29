#region Using
using Aplos.Controllers;
using Aplos.Helpers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Attendance;
using Library.HumanResource.Payroll.Allowance;
using Library.HumanResource.Payroll.PayrollManagementDashboard;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.Setups;
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
    public class PayrollManagementDashboardController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public PayrollManagementDashboardController(
              ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region ---Audit Report Summery---

        [HttpPost, Authorize]
        public JsonResult AuditReportSummary(string workDate, string ToDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PayrollManagementDashboard dashboard = new PayrollManagementDashboard();
                dashboard.MakeSummary(workDate, ToDate, identity.CompanyId, identity.CompanyGroupId, out DataTable dtFinalTable);

                DataTable dtTemp = dtFinalTable.Clone();
                for (int i = 0; i < dtFinalTable.Rows.Count; i++)
                {
                    if (clsStaticInfo.dbl(dtFinalTable.Rows[i]["UpToDate"].ToString()) == 0 && clsStaticInfo.dbl(dtFinalTable.Rows[i]["Yesterday"].ToString()) == 0)
                        continue;

                    dtTemp.ImportRow(dtFinalTable.Rows[i]);
                }

                return Json(new { DATA = CustomJsonResult.DataTableToJson(dtTemp), Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        [HttpPost, Authorize]
        public JsonResult GetAttendanceDetail(string workDate, string ToDate, string plantId, string ParticularsKey, bool UpToDate)
        {
            try
            {
                if (!UpToDate)
                    workDate = ToDate;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PayrollManagementDashboard dashboard = new PayrollManagementDashboard();

                List<Dictionary<string, object>> dtFinalTable = new List<Dictionary<string, object>>();

                string _dataType = "Profile";
                if (

                    ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.AbsentNoPunchTime.ToString().ToUpper()
                    || ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.AbsentWithsinglePunch.ToString().ToUpper()
                    || ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.LeaveWithPunch.ToString().ToUpper()
                    || ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.ShortDurationAbsent.ToString().ToUpper()
                    || ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.ShortDuration.ToString().ToUpper()
                    || ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.OTApplicableAndOutMissing.ToString().ToUpper()
                    || ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.OTNotApplicableAndOutMissing.ToString().ToUpper()
                    || ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.OTNotConfirm.ToString().ToUpper()
                    || ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.AttendanceNotLock.ToString().ToUpper()
                    || ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.OffdayMissingPunch.ToString().ToUpper()
                    || ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.OffdayWithPunch.ToString().ToUpper()
                    || ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.AbsentWithWrongShift.ToString().ToUpper()
                    || ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.NoOfAbsent.ToString().ToUpper())
                {
                    _dataType = "Attendance";
                    dashboard.GetAttendanceDetail(workDate, ToDate, plantId, ParticularsKey, out dtFinalTable);
                }

                else if (ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.UnApprovedProfile.ToString().ToUpper())
                {
                    dashboard.GetUNApprovedProfileEmployeeDetail(plantId, out dtFinalTable);
                }
                else if (ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.NoSalaryStructure.ToString().ToUpper())
                {
                    dashboard.GetProfileNoSalaryEmployeeDetail(workDate, plantId, identity.CompanyGroupId, ToDate, out dtFinalTable);
                }
                else if (ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.SalaryStructureNotApprove.ToString().ToUpper())
                {
                    dashboard.GetNoSalaryStructureApproveEmployeeDetail(workDate, plantId, identity.CompanyGroupId, out dtFinalTable);
                }

                else if (ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.LongAbsenteeism.ToString().ToUpper())
                {
                    dashboard.GetLongAbsentisomEmployeeDetail(plantId, out dtFinalTable);
                }
                else if (ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.TBS.ToString().ToUpper())
                {
                    dashboard.GetTBSEmployeeDetail(plantId, out dtFinalTable);
                }
                //, , , , , , , 
                else if (ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.BankStatus.ToString().ToUpper())
                {
                    dashboard.GetBankRemarkEmployeeDetail(workDate, plantId, identity.CompanyGroupId, ToDate, out dtFinalTable);
                }
                else if (ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.SalaryNotApproved.ToString().ToUpper())
                {
                    dashboard.GetSalaryNotApprovedEmployeeDetail(workDate, plantId, out dtFinalTable);
                }
                else if (ParticularsKey.ToUpper() == PayrollManagementDashboard.ReportParticulars.ShiftNotAssign.ToString().ToUpper())
                {
                    dashboard.GetShiftNotAssignEmployeeDetail(workDate, plantId, ToDate, out dtFinalTable);
                }
                var jsondata = Json(new { DATA = dtFinalTable, DataType = _dataType, Error = false }, JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #endregion
    }
}