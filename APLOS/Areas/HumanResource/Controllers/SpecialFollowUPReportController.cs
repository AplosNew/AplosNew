using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.HumanResources;
using Library.Service.HumanResources;
using Syncfusion.XlsIO;
using System;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.HumanResources.SpecialFollowUPReportService;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class SpecialFollowUPReportController : BaseController
    {
        #region Constructor

        private readonly ISpecialFollowUPReportService _SpecialFollowUPReportService;

        public SpecialFollowUPReportController(
              ISpecialFollowUPReportService SpecialFollowUPReportService
            )
        {
            _SpecialFollowUPReportService = SpecialFollowUPReportService;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet]
        public ActionResult GetSpecialFollowUPReportSummaryExcel(string PlantId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var fileName = "specialFollowUp" + DateTime.Now.ToString("yyMMdd")+ ".xls";
            var workbook = _SpecialFollowUPReportService.GetSpecialFollowUPReportSummaryExcel(identity.PlantId,fromDate,toDate);
            workbook.Version = ExcelVersion.Excel97to2003;
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
            return null;
        }
      
        #endregion -- Operations
    }
}