using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.HumanResources;
using Library.Service.Attendances;
using Library.Service.HumanResources;
using Syncfusion.XlsIO;
using System;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class AttendanceReportController : BaseController
    {
        #region Constructor

        private readonly IAttdnProcessDataService _AttendanceProcessDataService;

        public AttendanceReportController(
              IAttdnProcessDataService workGroupService
            )
        {
            _AttendanceProcessDataService = workGroupService;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Attend()
        {
            return View();
        }
        #endregion -- Pages


        [HttpGet, Authorize]
        public ActionResult AttndReport( string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = DateTime.Now.ToString("yy-MM-dd") + " " + "Employee Attendance Report";
            var workbook = _AttendanceProcessDataService.AttndReport(fromDate, toDate, identity.CompanyGroupId ,identity.CompanyId ,identity.PlantId);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }
    }
}