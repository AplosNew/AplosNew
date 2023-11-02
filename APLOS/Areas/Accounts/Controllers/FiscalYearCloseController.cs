using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Calendars;
using Library.Model.Enums;
using Library.Service.Calendars;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class FiscalYearCloseController : BaseController
    {
        private readonly IFiscalYearService _fiscalYearService;
        private readonly ISqlRepository _sqlRepository;

        public FiscalYearCloseController(
            IFiscalYearService fiscalYearService
            , ISqlRepository sqlRepository
            )
        {
            _fiscalYearService = fiscalYearService;
            _sqlRepository = sqlRepository;
        }
        #region Fiscal Year Close 
        [HttpGet]
        public ActionResult FiscalYearClose()
        {
            return View("~/Areas/Accounts/Views/FiscalYearClose.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetFiscalYearCloseList(GridParameter parameters)
        {
            FiscalYearCloseService _fiscalYearCloseService = new FiscalYearCloseService(_sqlRepository);
            return Json(_fiscalYearCloseService.GetFiscalYearCloseList(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetFiscalYearClose(string id)
        {
            return Json(_fiscalYearService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateFiscalYearClose(FiscalYearClose fiscalYearCloseVM)
        {
            FiscalYearCloseService _fiscalYearCloseService = new FiscalYearCloseService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            fiscalYearCloseVM.CompanyGroupId = identity.CompanyGroupId;
            _fiscalYearCloseService.InsertFiscalYearClose(fiscalYearCloseVM);

            return Json(new { Message = AplosMessage.Insert });
        }



        //[HttpPost]
        //public ActionResult Delete(string id)
        //{
        //    if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
        //    if (_fiscalYearService.UsingCheck(id))
        //        throw new CustomException("This Fiscal Year is already in used...");
        //    _fiscalYearPeriodService.DeleteFiscalYearById(id);
        //    _fiscalYearService.Delete(id);
        //    return Json(new { Message = AplosMessage.Deleted });
        //}

        #endregion

        #region Fiscal Year Close Post
        [HttpGet]
        public ActionResult FiscalYearClosePost()
        {
            return View("~/Areas/Accounts/Views/FiscalYearClosePost.cshtml");
        }
        [Authorize, HttpPost]
        public ActionResult GetFiscalYearClosePostedList(string column, string value)
        {
            FiscalYearCloseService _fiscalYearCloseService = new FiscalYearCloseService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_fiscalYearCloseService.GetFiscalYearClosePostedList(column, value, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetFiscalYearCloseListForPosting()
        {
            FiscalYearCloseService _fiscalYearCloseService = new FiscalYearCloseService(_sqlRepository);
            return Json(_fiscalYearCloseService.GetFiscalYearCloseListForPosting(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetFiscalYearCloseSingleJVList(string fiscalYearCloseId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            FiscalYearCloseService _fiscalYearCloseService = new FiscalYearCloseService(_sqlRepository);
            return Json(_fiscalYearCloseService.GetFiscalYearCloseSingleJVList(fiscalYearCloseId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult CreatetFiscalYearClosePost(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, Dictionary<string, object> fiscalYearClosedata)
        {
            FiscalYearCloseService _fiscalYearCloseService = new FiscalYearCloseService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            if (voucherDetailVMList.Sum(r => r.Amount) == 0)
                throw new CustomException("Dr Cr Amount not match !.");

            _fiscalYearCloseService.InsertFiscalYearClosePosting(voucherVM, voucherDetailVMList, fiscalYearClosedata);

            return Json(new { Message = AplosMessage.Insert });
        }
        [HttpGet, Authorize]
        public ActionResult FiscalYearClosePostVoucherReport(ReportFormat reportFormat, string voucherId)
        {
            FiscalYearCloseService _fiscalYearCloseService = new FiscalYearCloseService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var workbook = _fiscalYearCloseService.FiscalYearClosePostVoucherReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName, false);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }
        #endregion


    }
}