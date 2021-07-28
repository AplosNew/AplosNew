#region Using
using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Attendance;
using Library.HumanResource.Payroll.Allowance;
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
        public JsonResult AuditReportSummery(string workDate, string ToDate)
        {
            try
            {
                string fileName = "";
                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //fileName = DailyStatusCountReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, "Daily Status Count", "", workDate, ToDate);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

     
        #endregion
    }
}