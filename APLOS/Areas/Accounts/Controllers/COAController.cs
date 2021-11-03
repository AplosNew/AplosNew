using Aplos.Controllers;
using Aplos.Helpers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.ChartOfAccounts;
using Library.Service.ChartOfAccounts;
using Library.Service.Vouchers;
using Syncfusion.XlsIO;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class COAController : BaseController
    {
        private readonly IChartOfAccountService _chartOfAccountService;
        private readonly IVoucherReportService _voucharReportService;

        public COAController(
            IChartOfAccountService chartOfAccountService
            , IVoucherReportService voucharReportService)
        {
            _chartOfAccountService = chartOfAccountService;
            _voucharReportService = voucharReportService;
        }

        [Authorize, HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetCOACbo(string companyGroupId)
        {
            if (string.IsNullOrEmpty(companyGroupId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                companyGroupId = identity.CompanyGroupId;
            }
            return Json(_chartOfAccountService.GetCboCOA(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetGLLengthCbo(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_chartOfAccountService.GetCboGLLength(identity.CompanyGroupId, id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAccountGroupCOA(string coaId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_chartOfAccountService.GetAccountGroupCOA(coaId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CheckCOAIdUse(string coaId)
        {
            return Json(_chartOfAccountService.COAUseChecking(coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        // get only true or false if true then GLLevel div show and if false then GLLevel div hide.in time of COA Change it will happend.
        public ActionResult CheckLevelMandatory(string coaId)
        {
            return Json(_chartOfAccountService.CheckLevelMandatory(coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAllCOAList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_chartOfAccountService.GetSearchData(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCOAById(string id)
        {
            return Json(_chartOfAccountService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ChartOfAccount coa)
        {
            if (!ModelState.IsValid) throw new CustomException(Resources.RequiredFieldMessage);
            _chartOfAccountService.Insert(coa);
            return Json(new { ChartOfAccount = coa, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ChartOfAccount coa)
        {
            if (!ModelState.IsValid) throw new CustomException(Resources.RequiredFieldMessage);
            _chartOfAccountService.Update(coa);
            return Json(new { ChartOfAccount = coa, Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _chartOfAccountService.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public ActionResult GenerateCoaReport(string masterId)
        {
            var workbook = _voucharReportService.GetCoa(out ExcelEngine excelEngine, masterId);
            return excelEngine.SaveAsActionResult(workbook, "Coa_" + masterId + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog, ExcelHttpContentType.Excel2013);
        }
    }
}