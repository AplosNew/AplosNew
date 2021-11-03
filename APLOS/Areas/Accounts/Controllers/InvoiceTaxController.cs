using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Enums;
using Library.Service.Invoices;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class InvoiceTaxController : BaseController
    {
        #region -- Constrator

        private readonly ITaxPaymentService _taxPaymentService;
        private readonly IInvoiceTaxService _invoiceTaxService;

        public InvoiceTaxController(
              ITaxPaymentService taxPaymentService
            , IInvoiceTaxService invoiceTaxService
            )
        {
            _taxPaymentService = taxPaymentService;
            _invoiceTaxService = invoiceTaxService;
        }

        #endregion -- Constrator

        #region TaxPayment

        [Authorize, HttpGet]
        public ActionResult TaxPayment()
        {
            return View();
        }

        [Authorize, HttpGet]
        public ActionResult TaxPayableReport()
        {
            return View();
        }

        [Authorize, HttpPost]
        public JsonResult GetTaxPaymentDataList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_taxPaymentService.GetTaxPaymentDataList(column, value, identity.CompanyId,identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInvoiceTaxPayableList(DateTime fromDate, DateTime toDate,string taxCategoryId, string partyType, string partyId, string partyPlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_taxPaymentService.GetInvoiceTaxPayableList(identity.CompanyGroupId, identity.CompanyId, taxCategoryId, fromDate, toDate, partyType, partyId, partyPlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult InsertTaxPayment(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            if (voucherVM.DocDate == DateTime.MinValue)
                throw new CustomException("Doc Date is null!");
            if (string.IsNullOrEmpty(voucherVM.DocRefNo))
                throw new CustomException("Doc Ref is null!");
            if (string.IsNullOrEmpty(voucherVM.Narration))
                throw new CustomException("Narration is null!");
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            _taxPaymentService.InsertTaxPayment(voucherVM, voucherDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        #endregion TaxPayment

        [HttpGet, Authorize]
        public JsonResult GetTDSPayableList(string advanceId)
        {
            return Json(_invoiceTaxService.GetTDSPayableList(advanceId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTaxPayableReport(ReportFormat reportFormat, string fromDate, string toDate, string taxCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _taxPaymentService.GetTaxPayableReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, taxCategoryId, fromDate, toDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Tax Payable Report";
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
    }
}