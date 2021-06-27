using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.HumanResources;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.HumanResources.PayRegisterBDReportService;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class SFBonusSheetReportController : BaseController
    {
        #region Constructor

        private readonly ISFBonusSheetReportService _SFBonusSheetReportService;

        public SFBonusSheetReportController(
              ISFBonusSheetReportService SFBonusSheetReportService
            )
        {
            _SFBonusSheetReportService = SFBonusSheetReportService;
        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }


        public ActionResult GridAplos()
        {
            return View();
        }


        #endregion -- Pages

        [HttpGet, Authorize]
        public ActionResult GetSFBonusSheet(string payGroup , string paymentMode, string languageId, string bonusPointId,string bunusType)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var fileName = DateTime.Now.ToString("yyMMdd") + "-" + "Bonus Report";
                
                var workbook = _SFBonusSheetReportService.GetSFBonusSheet(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, languageId, paymentMode, payGroup, bonusPointId, bunusType);
                workbook.SaveAs(fileName + ".xls", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
                return null;
            }
            catch (Exception ex)
            {

                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult GetSFBonusSheetGrid(Dictionary<string, string> parameters, string cutoffdate, string plantId, string languageId, string paymentMode, string bonusType, bool isStampDeductApplicable,string reportHeader,string docGrouping)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var fileName = DateTime.Now.ToString("yyMMdd") + "-" + "Bonus Report.xls";

                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;
                var workbook = _SFBonusSheetReportService.GetSFBonusSheetGrid(parameters, cutoffdate, identity.CompanyId, identity.PlantId, languageId, paymentMode, bonusType, isStampDeductApplicable, reportHeader,docGrouping);

                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
        }


        [HttpGet, Authorize]
        public JsonResult GetBonusPoint()
        {
            var data = _SFBonusSheetReportService.GetBonusPoint();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBonusEffectiveDate()
        {
            var data = _SFBonusSheetReportService.GetBonusEffectiveDate();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult GetEmpInfo(string effectiveDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var jsondata = Json(_SFBonusSheetReportService.GetEmpInfo( identity.CompanyGroupId, identity.PlantId, effectiveDate,  identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
    }
}