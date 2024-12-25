using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Accounts;
using Library.Model.Commercial;
using Library.Model.Enums;
using Library.Model.Invoices;
using Library.Model.Payments;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Invoices;
using Library.ViewModel.Accounts;
using Library.ViewModel.Banks;
using Library.ViewModel.Invoices;
using Library.ViewModel.Vouchers;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Library.Service.Advances;
using Syncfusion.Pdf;
using Syncfusion.ExcelToPdfConverter;
using Library.ViewModel.OrderManagements;
using Library.Security.Core;
using Library.OrderManagement.Sales;
using Library.Model.Vouchers;

namespace Aplos.Areas.Accounts.Controllers
{
    public class InvoiceController : BaseController
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoiceWriteOffService _invoiceWriteOffService;
        private readonly IInvoiceReportService _invoiceReportService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IEmployeePayableService _employeePayableService;
        private readonly IAdvanceWriteOffService _advanceWriteOffService;
        clsSales clsSales = new clsSales();
        public InvoiceController(
              IInvoiceService invoiceService
            , IInvoiceWriteOffService invoiceWriteOffService
            , IInvoiceReportService invoiceReportService
            , ISqlRepository sqlRepository
            , IEmployeePayableService employeePayableService
            , IAdvanceWriteOffService advanceWriteOffService
              )
        {
            _invoiceService = invoiceService;
            _invoiceWriteOffService = invoiceWriteOffService;
            _invoiceReportService = invoiceReportService;
            _sqlRepository = sqlRepository;
            _employeePayableService = employeePayableService;
            _advanceWriteOffService = advanceWriteOffService;

        }


        public ActionResult CustomerInvoice()
        {
            return View("~/Areas/Accounts/Views/CustomerInvoice.cshtml");
        }


        public ActionResult VendorInvoice()
        {
            return View("~/Areas/Accounts/Views/VendorInvoice.cshtml");
        }

        public ActionResult InvoiceOverhead()
        {
            return View("~/Areas/Accounts/Views/InvoiceOverhead.cshtml");
        }
        public ActionResult InvoiceOverheadPost()
        {
            return View("~/Areas/Accounts/Views/InvoiceOverheadPost.cshtml");
        }
        public ActionResult paymentadvice()
        {
            return View("~/Areas/Accounts/Views/paymentadvice.cshtml");
        }
        [AllowAnonymous]
        public ActionResult multipleVP()
        {
            return View("~/Areas/Accounts/Views/multipleVP.cshtml");
        }


        [HttpGet, Authorize]
        public ActionResult GetMultiVendorPaymentReport(ReportFormat reportFormat, string mpdId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountsInvoiceService.MultiVendorPaymentReportSheet(out string reportFileName, mpdId);

            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }
        [HttpGet, Authorize]
        public ActionResult DownloadUsingFullPath(string FullPath, string fileName)
        {
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();
                //string fullPath = HostingEnvironment.MapPath("~/") + FileName;
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(FullPath);
                try
                {
                    System.IO.File.Delete(FullPath);
                }
                catch (Exception)
                {
                }

                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;

            }
            catch (Exception ex)
            {


            }
            return null;
        }
        //[HttpPost, Authorize]
        //public ActionResult GetMultiVendorPaymentReport(string mpdId)
        //{
        //    try
        //    {
        //        AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
        //        var workbook = _accountsInvoiceService.MultiVendorPaymentReportSheet(mpdId);

        //        var fileName = "Multi Vendor Payment Report.xlsx";
        //        string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
        //        workbook.SaveAs(fullPath);

        //        return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //}


        [HttpGet, Authorize]
        public JsonResult GetVendorAvailableInvoiceNewList(GridParameter parameters)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.GetVendorAvailableInvoiceNewList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetVendorAvailableInvoiceList(GridParameter parameters, string partyId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.GetVendorAvailableInvoiceList(parameters, identity.CompanyGroupId, identity.CompanyId, partyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetVendorAvailableInvoiceList1()
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.GetVendorAvailableInvoiceList(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetVendorAllInvoiceList(string column, string value)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.GetVendorAllInvoiceList(identity.CompanyGroupId, identity.CompanyId, column, value), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetInvoicePurchasesAvailable(string voucherId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            return Json(_accountsInvoiceService.GetInvoicePurchasesAvailable(voucherId), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult InsertCustomerInvoice(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList, OtherInvoice otherInvoiceVM)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            if (!voucherVM.IsExcludingTax && voucherVM.Amount != voucherDetailVMList.Sum(r => r.TotalAmount))
                throw new CustomException("Total Amount and Invoice Amount not match!");
            else if (voucherVM.IsExcludingTax && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Total Amount and Invoice Amount not match!");
            voucherVM.SourceType = SourceType.CustomerInvoice.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceService.InsertCustomerInvoice(voucherVM, voucherDetailVMList, taxDetailVMList, otherInvoiceVM)) });
        }

        [HttpPost]
        public ActionResult PostCustomerInvoice(string invoiceId)
        {
            _invoiceService.Post(invoiceId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public ActionResult UpdateCustomerInvoice()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DeleteCustomerInvoice(string invoiceId, string voucherId, string deletedRemarks)
        {
            if (deletedRemarks == null || deletedRemarks == "")
                throw new CustomException("Deleted Remarks is required!");
            _invoiceService.DeleteInvoice(invoiceId, voucherId, deletedRemarks);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetOtherInvoiceJournal(string otherInvoieId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            return Json(_accountsInvoiceService.GetOtherInvoiceJournal(identity.CompanyId, identity.PlantId, otherInvoieId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult InsertOtherInvoiceJournal(string otherInvoiceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
            voucherVM.SourceType = SourceType.CustomerReceipt.ToString();
            voucherVM.PaymentSource = PaymentSource.Journal.ToString();
            voucherVM.PartyType = "Customer";
            _invoiceWriteOffService.InsertOtherInvicePost(voucherVM, otherInvoiceId, voucherDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        #region Vendor Invoice
        [HttpGet, Authorize]
        public JsonResult GetVendorInvoiceList(GridParameter parameters)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.InvoiceQuery(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.VendorInvoice), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInvoiceSetOffDetailByInvoice(string invoiceId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.GetInvoiceSetOffDetailByInvoice(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.VendorPayment, invoiceId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetInvoiceSetOffDetailByInvoiceId(string invoiceId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.GetInvoiceSetOffDetailByInvoiceId(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, invoiceId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetFiscalInvoiceTotalAmountByParty(string partyId, DateTime postingDate)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.GetFiscalInvoiceTotalAmountByParty(identity.PlantId, partyId, postingDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetVendorInvoiceList1(GridParameter parameters)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.InvoiceQuery(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.VendorInvoice), JsonRequestBehavior.AllowGet);
        }



        [HttpGet, Authorize]
        public JsonResult GetInvoiceGLBudgetActivityDetail(string voucherId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            return Json(_accountsInvoiceService.GetInvoiceGLBudgetActivityDetail(voucherId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetInvoiceTaxDetail(string invoiceId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            return Json(_accountsInvoiceService.GetInvoiceTaxDetail(invoiceId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertVendorInvoice(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<InvoiceTaxViewModel> taxDetailVMList, IEnumerable<InvoiceTaxViewModel> tdsVMList, IEnumerable<InvoiceDetailCharges> invoiceDetailChargesList, IEnumerable<VoucherViewModel> existingLoanList, IEnumerable<MachineMasterAssetSeviceDistribution> machineMasterAssetSeviceDistributionList)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            if (voucherVM.CompanyCurrencyRate == 0)
                throw new CustomException("Rate can not Empty!");
            if (voucherVM.PaymentSource == PaymentSource.GL.ToString())
            {
                if (voucherVM.IsExcludingTax == false && voucherVM.Amount != voucherDetailVMList.Sum(r => r.TotalAmount))
                    throw new CustomException("Total Amount and Invoice Amount not match!");
                else if (voucherVM.IsExcludingTax == true && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                    throw new CustomException("Net Amount and Invoice Amount not match!");
            }
            if (voucherVM.PaymentSource == PaymentSource.Loan.ToString())
            {
                if (voucherVM.CurrencyId == existingLoanList.FirstOrDefault().CurrencyId)
                {
                    if (voucherVM.IsExcludingTax == false && voucherVM.Amount != existingLoanList.Sum(r => r.LoanSetOffAmount))
                        throw new CustomException("Total Amount and Loan SetOff Amount not match!");
                }
                else
                {
                    if (voucherVM.IsExcludingTax == false && (Math.Round((voucherVM.Amount * voucherVM.CompanyCurrencyRate), 2) != existingLoanList.Sum(r => r.LoanSetOffAmount)))
                        throw new CustomException("Total Amount and Loan SetOff Amount not match!");
                }

            }
            if (voucherVM.ApprovedById != null)
            {
                voucherVM.ApprovedByStatus = "ToBeApproved";
            }
            if (voucherVM.PaymentSource == PaymentSource.Cash.ToString() && voucherVM.CashMasterId == null)
                throw new CustomException(Resources.SelectCash);
            if (voucherVM.PaymentSource == PaymentSource.Bank.ToString() && voucherVM.BankMasterId == null)
                throw new CustomException(Resources.SelectBank);
            voucherVM.SourceType = SourceType.VendorInvoice.ToString();
            if (voucherVM.BeneficiaryType == NewBeneficiaryType.Vendor.ToString())
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceService.InsertVendorInvoice(voucherVM, voucherDetailVMList, taxDetailVMList, tdsVMList, invoiceDetailChargesList, existingLoanList, machineMasterAssetSeviceDistributionList)) });
            else
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceService.InsertVendorInvoiceBeneficiaryEmployee(voucherVM, voucherDetailVMList, taxDetailVMList, tdsVMList)) });

        }
        [HttpPost, Authorize]
        public JsonResult InsertIncentiveReceivableInvoice(VoucherViewModel voucherVM, IEnumerable<IncentiveReceivableMap> incentiveReceivableMapList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            if (voucherVM.CompanyCurrencyRate == 0)
                throw new CustomException("Rate can not Empty!");
            voucherVM.SourceType = SourceType.ReceivableFromOthers.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceService.InsertIncentiveReceivableInvoice(voucherVM, incentiveReceivableMapList)) });

        }

        [HttpPost, Authorize]
        public JsonResult InsertAdditionalTaxPayable(string additionalTaxId, VoucherViewModel voucherVM)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
            voucherVM.SourceType = SourceType.VendorPayment.ToString();
            voucherVM.PaymentSource = PaymentSource.Tax.ToString();
            voucherVM.PartyType = "Vendor";
            _invoiceWriteOffService.InsertAdditionalTaxPayable(voucherVM, additionalTaxId);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public ActionResult UpdateVendorInvoice(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            if (voucherVM.CompanyCurrencyRate == 0)
                throw new CustomException("Rate can not Empty!");
            if (voucherVM.PaymentSource == PaymentSource.GL.ToString())
            {
                if (voucherVM.IsExcludingTax == false && voucherVM.Amount != voucherDetailVMList.Sum(r => r.TotalAmount))
                    throw new CustomException("Total Amount and Invoice Amount not match!");
                else if (voucherVM.IsExcludingTax == true && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                    throw new CustomException("Net Amount and Invoice Amount not match!");
            }

            if (voucherVM.PaymentSource == PaymentSource.Cash.ToString() && voucherVM.CashMasterId == null)
                throw new CustomException(Resources.SelectCash);
            voucherVM.SourceType = SourceType.VendorInvoice.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceService.UpdateVendorInvoice(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public ActionResult PostVendorInvoice(string invoiceId, string type)
        {
            if (type == NewBeneficiaryType.Vendor.ToString())
                _invoiceService.Post(invoiceId);
            if (type == NewBeneficiaryType.Employee.ToString())
                _employeePayableService.Post(invoiceId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public ActionResult PostVoucher(Voucher voucher,string invoiceId, string type,IEnumerable<VoucherDetailViewModel> voucherDetailList)
        {
            if (type == NewBeneficiaryType.Vendor.ToString())
                _invoiceService.PostVoucher(voucher,invoiceId, type, voucherDetailList);
            if (type == NewBeneficiaryType.Employee.ToString())
                _employeePayableService.PostVoucher(voucher, invoiceId, type, voucherDetailList);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public ActionResult PostIncentiveReceivableInvoice(string invoiceId)
        {
            _invoiceService.Post(invoiceId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public ActionResult DeleteVendorInvoice(string invoiceId, string voucherId, string type, string tDSVoucherId, string tDSVoucherNo, string deletedRemarks)
        {
            if (tDSVoucherId != null)
                throw new CustomException("TDS voucher no  " + tDSVoucherNo + "need to delete first!");
            if (deletedRemarks == null || deletedRemarks == "")
                throw new CustomException("Deleted Remarks is required!");

            if (type == NewBeneficiaryType.Vendor.ToString())
                _invoiceService.DeleteInvoice(invoiceId, voucherId, deletedRemarks);
            if (type == NewBeneficiaryType.Employee.ToString())
                _employeePayableService.DeleteInvoiceBeneficiaryEmployee(invoiceId, voucherId, deletedRemarks);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public ActionResult DeleteIncentiveReceivableInvoice(string invoiceId, string voucherId)
        {
            _invoiceService.DeleteIncentiveReceivableInvoice(invoiceId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }


        [HttpGet, Authorize]
        public ActionResult ReportVendorInvoice(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _invoiceReportService.GetVendorInvoiceReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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
        [HttpGet, Authorize]
        public ActionResult ReportIncentiveReceivableInvoice(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _invoiceReportService.GetIncentiveReceivableInvoiceReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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
        [HttpGet, Authorize]
        public ActionResult ReportVendorInvoiceExpenseDistribution(ReportFormat reportFormat, string voucherId)
        {
            AccountsInvoiceReportService accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = accountsInvoiceReportService.GetVendorInvoiceReportExpenseDistribution(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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

        [HttpGet, Authorize]
        public ActionResult ReportVendorInvoiceAssetDistribution(ReportFormat reportFormat, string voucherId)
        {
            AccountsInvoiceReportService accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = accountsInvoiceReportService.GetVendorInvoiceReportAssetDistribution(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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

        [HttpPost]
        public ActionResult DeleteInventoryPayable(string grnId, string voucherId, string invoiceId,string otherVendorId, string type, string tDSTaxVoucherId, string tDSVoucherNo, string deletedRemarks)
        {
            if (deletedRemarks == null || deletedRemarks == "")
                throw new CustomException("Deleted Remarks is required!");
            if (tDSTaxVoucherId != null)
                throw new CustomException("TDS voucher no  " + tDSVoucherNo + "need to delete first!");

            if (type == NewBeneficiaryType.Vendor.ToString())
                _invoiceService.DeleteInventoryPayable(grnId, invoiceId,otherVendorId, voucherId, deletedRemarks);
            if (type == NewBeneficiaryType.Employee.ToString())
                _employeePayableService.DeleteGRNBeneficiaryEmployee(grnId, invoiceId, voucherId, deletedRemarks);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public ActionResult DeleteInventorySales(string salesId, string voucherId, string InventoryVoucherId)
        {
            _invoiceService.DeleteInventorySales(salesId, voucherId, InventoryVoucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public ActionResult DeleteServicePayable(string serviceAckId, string voucherId, string invoiceId, string tDSTaxVoucherId, string tDSVoucherNo)
        {
            if (tDSTaxVoucherId != null)
                throw new CustomException("TDS voucher no  " + tDSVoucherNo + "need to delete first!");

            //if (type == NewBeneficiaryType.Vendor.ToString())
            _invoiceService.DeleteServicePayable(serviceAckId, invoiceId, voucherId);
            //if (type == NewBeneficiaryType.Employee.ToString())
            // _employeePayableService.DeleteServiceBeneficiaryEmployee(serviceAckId, invoiceId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }


        #endregion

        #region Auto Mail

        [HttpGet, Authorize]
        public ActionResult GetAutoMailReport()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInvoiceReportService accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
            try
            {

                ExcelEngine excelEngine = new ExcelEngine();

                //IWorkbook workbook = IssueReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, checkbox);
                // IWorkbook workbook = OperationReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
                IWorkbook workbook = accountsInvoiceReportService.GetAutoMailReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);


                string strFileName = "DateRangePayableList.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (CustomException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }


        [HttpGet, Authorize]
        public ActionResult GetAutoMailVPaymentReport()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInvoiceReportService accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();
                IWorkbook workbook = accountsInvoiceReportService.GetAutoMailVPaymentReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
                string strFileName = "DateRangePaymentList.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (CustomException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }


        #endregion

        #region DateRange Wise Payable List and Payment

        //[HttpPost, Authorize]
        //public ActionResult getDateRangeWisePayableData()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    AccountsInvoiceReportService accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
        //    //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
        //    return Json(new { DATA = accountsInvoiceReportService.getDateRangeWisePayableData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);

        //}

        //[HttpPost, Authorize]
        //public ActionResult getDateRangeWisePaymentData(string fromDate, string toDate)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    AccountsInvoiceReportService accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);

        //    //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
        //    return Json(new { DATA = accountsInvoiceReportService.getDateRangeWisePaymentData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate,toDate), Error = false }, JsonRequestBehavior.AllowGet);

        //}

        //[HttpPost, Authorize]
        //public ActionResult getDateRangeWisePaymentDataBarChart(string fromDate, string toDate)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    AccountsInvoiceReportService accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);

        //    //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
        //    return Json(accountsInvoiceReportService.getDateRangeWisePaymentDataBarChart(identity.CompanyGroupId, identity.CompanyId, identity.PlantId,fromDate,toDate), JsonRequestBehavior.AllowGet);

        //}


        //[HttpPost, Authorize]
        //public ActionResult getDateRangeWisePaymentPopUpData(string id, string type, string fromDate, string toDate)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    AccountsInvoiceReportService accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);

        //    //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
        //    return Json(new { DATA = accountsInvoiceReportService.GetPartyPaymentDetailPopUpListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, id,type, fromDate,toDate), Error = false }, JsonRequestBehavior.AllowGet);

        //}

        //[HttpGet, Authorize]
        //public ActionResult getDateRangeWisePayableData()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    AccountsInvoiceReportService accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
        //    //return Json(  accountsInvoiceReportService.getDateRangeWisePayableData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        //    return Json(new { DATA = accountsInvoiceReportService.getDateRangeWisePayableData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
        //    //return Json(accountsCommonService.getvoucherlistforCashchequeReprinting(parameters), JsonRequestBehavior.AllowGet);
        //}


        #endregion

        //Customer invoice report new
        [HttpGet, Authorize]
        public ActionResult GetCustomerInvoiceVoucherReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _invoiceReportService.GetCustomerInvoiceReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }

        [HttpGet, Authorize]
        public ActionResult CustomerReceipt()
        {
            return View("~/Areas/Accounts/Views/CustomerReceipt.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetCustomerReceiptList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_invoiceWriteOffService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerReceipt), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertCustomerReceipt(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceWriteOffService.InsertReceived(voucherVM, voucherDetailVMList, voucherDetailCurrencyVMList)) });
        }

        [HttpPost]
        public ActionResult UpdateCustomerReceipt()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult CustomerInvoiceReceiveReport(string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _invoiceReportService.GetCustomerInvoiceReceive(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
            workbook.SaveAs(DateTime.Now.ToString("yyMMdd") + " Payment Receipt Voucher.xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        [HttpGet, Authorize]
        public ActionResult CustomerInterPlantCompanyReceipt()
        {
            return View();
        }


        public ActionResult VendorPayment()
        {
            return View("~/Areas/Accounts/Views/VendorPayment.cshtml");
        }

        public ActionResult VendorPaymentApproval()
        {
            return View("~/Areas/Accounts/Views/VendorPaymentApproval.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetVendorPaymentList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_invoiceWriteOffService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.VendorPayment), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetVendorPaymentParkedNonPostedList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_invoiceWriteOffService.GetVendorPaymentParkedNonPostedList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.VendorPayment), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult ApproveVendorPayment(InvoiceWriteOff invoiceWriteOff)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            OTSBD.IdentityParameter para = new OTSBD.IdentityParameter
            {
                CompanyGroupId = identity.CompanyGroupId,
                CompanyId = identity.CompanyId,
                PlantId = identity.PlantId,
                AddedBy = identity.Name,
                AddedDate = DateTime.Now,
                AddedFromIP = identity.IPAddress,
                UpdatedBy = identity.Name,
                UpdatedDate = DateTime.Now,
                UpdatedFromIP = identity.IPAddress
            };

            _invoiceWriteOffService.ApproveVendorPayment(invoiceWriteOff, para);
            return Json(new { Message = AplosMessage.Posted });
        }


        [HttpPost]
        public JsonResult InsertVendorPayment(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<PurchaseLCChargesViewModel> purchaseLCChargesVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList, IEnumerable<VoucherDetailViewModel> glVMList, IEnumerable<VoucherViewModel> advanceVMList, IEnumerable<VoucherViewModel> existingLoanList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.VendorPayment.ToString();
            voucherVM.IsPark = true;
            if (voucherVM.CompanyCurrencyRate <= 0)
                throw new CustomException("Please Input Rate.");
            if ((voucherVM.PaymentSource == "Bank") && (voucherVM.BankMasterId == null))
                throw new CustomException(Resources.SelectBank);
            if ((voucherVM.PaymentSource == "Bank") && (voucherVM.BankAmount == 0))
                throw new CustomException("Please input Bank Amount");
            if ((voucherVM.PaymentSource == "Cash") && (voucherVM.CashMasterId == null))
                throw new CustomException(Resources.SelectCash);
            if ((voucherVM.PaymentSource == "Vendor") && (voucherVM.OtherPartyId == null))
                throw new CustomException("Please select Vendor");
            if ((voucherVM.PaymentSource == "Vendor") && (voucherVM.FinancingTypeId == null))
                throw new CustomException("Please select transaction type");
            if ((voucherVM.PaymentSource == "Employee") && (voucherVM.AdvanceId == null))
                throw new CustomException("Please select Employee Advance");

            foreach (var advanceDetailVM in voucherDetailVMList)
            {
                if (advanceDetailVM.Amount == 0 || advanceDetailVM.Amount.ToString() == null)
                    throw new CustomException("Amount should more than 0");
                if (voucherVM.CurrencyId != advanceDetailVM.CurrencyId)
                    throw new CustomException("Transaction currency and Payable currency should be same.!!!");
            }
            if (voucherVM.PaymentSource == "Employee")
            {
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _advanceWriteOffService.InsertVendorPaymentEmployeeAdvanceWriteOff(voucherVM, voucherDetailVMList, bankChargeDetailVMList, advanceVMList)) });
            }
            else
            {
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceWriteOffService.InsertVendorPayment(voucherVM, voucherDetailVMList, bankChargeDetailVMList, purchaseLCChargesVMList, taxDetailVMList, glVMList, existingLoanList)) });
            }

        }

        [Authorize, HttpGet]
        public JsonResult GetInvoiceToAcceptancePostList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_invoiceWriteOffService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.InvoiceToAcceptance), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult InsertInvoiceToAcceptancePost(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
           , IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList, IEnumerable<VoucherDetailViewModel> glVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.InvoiceToAcceptance.ToString();
            voucherVM.IsPark = true;
            if (voucherVM.CompanyCurrencyRate <= 0)
                throw new CustomException("Please Input Rate.");

            foreach (var advanceDetailVM in voucherDetailVMList)
            {
                if (advanceDetailVM.Amount == 0 || advanceDetailVM.Amount.ToString() == null)
                    throw new CustomException("Amount should more than 0");
                if (voucherVM.CurrencyId != advanceDetailVM.CurrencyId)
                    throw new CustomException("Transaction currency and Payable currency should be same.!!!");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceWriteOffService.InsertInvoiceToAcceptancePost(voucherVM, voucherDetailVMList, bankChargeDetailVMList, taxDetailVMList, glVMList)) });
        }
        [HttpPost]
        public ActionResult PostInvoiceToAcceptance(string invoiceWriteOffId)
        {
            _invoiceWriteOffService.PostInvoiceToAcceptance(invoiceWriteOffId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public ActionResult DeleteInvoiceToAcceptance(string invoiceWriteOffId, string voucherId)
        {
            _invoiceWriteOffService.DeleteInvoiceToAcceptance(invoiceWriteOffId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public ActionResult UpdateVendorPayment()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PostVendorPayment(string invoiceWriteOffId)
        {
            _invoiceWriteOffService.Post(invoiceWriteOffId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public ActionResult DeleteWriteOff(string invoiceWriteOffId, string voucherId, string deletedRemarks)
        {
            if (deletedRemarks == null || deletedRemarks == "")
                throw new CustomException("Deleted Remarks is required!");
            _invoiceWriteOffService.DeleteWriteOff(invoiceWriteOffId, voucherId, deletedRemarks);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public ActionResult DeleteInvoiceRoundOff(string invoiceWriteOffId, string voucherId, string deletedRemarks)
        {
            //if (deletedRemarks == null || deletedRemarks == "")
            //    throw new CustomException("Deleted Remarks is required!");
            _invoiceWriteOffService.DeleteWriteOff(invoiceWriteOffId, voucherId, deletedRemarks);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult VendorInvoicePaymentReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _invoiceReportService.GetVendorPaymentReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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


        #region Customer Invoice Receipt


        public ActionResult CustomerInvoiceReceipt()
        {
            return View("~/Areas/Accounts/Views/CustomerInvoiceReceipt.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetCustomerInvoiceReceiptList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_invoiceWriteOffService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerReceipt), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertCustomerInvoiceReceipt(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.CustomerReceipt.ToString();
            voucherVM.IsPark = true;
            if (voucherVM.CompanyCurrencyRate <= 0)
                throw new CustomException("Please Input Rate.");
            if ((voucherVM.PaymentSource == "Bank") && (voucherVM.BankMasterId == null))
                throw new CustomException(Resources.SelectBank);
            if ((voucherVM.PaymentSource == "Cash") && (voucherVM.CashMasterId == null))
                throw new CustomException(Resources.SelectCash);
            foreach (var advanceDetailVM in voucherDetailVMList)
            {
                if (advanceDetailVM.Amount == 0 || advanceDetailVM.Amount.ToString() == null)
                    throw new CustomException("Amount should more than 0");
                if (voucherVM.CurrencyId != advanceDetailVM.CurrencyId)
                    throw new CustomException("Transaction currency and Payable currency should be same.!!!");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceWriteOffService.InsertCustomerInvoiceReceipt(voucherVM, voucherDetailVMList, bankChargeDetailVMList, taxDetailVMList)) });
        }

        [HttpPost, Authorize]
        public JsonResult ParkInvoiceRoundOffJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.JournalVoucher.ToString();
            if (voucherDetailVMList == null)
                throw new CustomException("Please Add GL.");
            if (voucherDetailVMList.Sum(r => r.DrAmount) != voucherDetailVMList.Sum(r => r.CrAmount))
                throw new CustomException("Dr Cr not match!");
            foreach (var item in voucherDetailVMList)
            {
                if ((item.DrAmount + item.CrAmount == 0) || (item.DrAmount + item.CrAmount < 0))
                    throw new CustomException("Please input amount !");
                if (string.IsNullOrEmpty(item.EntityId))
                {
                    item.EntityId = voucherVM.EntityId;
                }
            }
            voucherVM.IsPark = true;
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceWriteOffService.InsertInvoiceRoundOffJournal(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public ActionResult UpdateCustomerInvoiceReceipt()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PostCustomerInvoiceReceipt(string invoiceWriteOffId)
        {
            _invoiceWriteOffService.Post(invoiceWriteOffId);
            return Json(new { Message = AplosMessage.Posted });
        }


        // CustomerInvoiceReceiptReport new
        [HttpGet, Authorize]
        public ActionResult CustomerInvoiceReceiptReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _invoiceReportService.GetCustomerInvoiceReceiptReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.CustomerReceipt.ToString());
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

        //CustomerInvoiceReceiptGovtSubsidyReport
        [HttpGet, Authorize]
        public ActionResult CustomerInvoiceReceiptGovtSubsidyReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _invoiceReportService.GetCustomerInvoiceReceiptGovtSubsidyReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.CustomerReceipt.ToString());
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

        [HttpGet, Authorize]
        public ActionResult CustomerInvoiceChargeSetOffReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _invoiceReportService.GetInvoiceChargesReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.InvoiceCharge.ToString());
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
        #endregion
        #region Customer Invoice Banks Receipt

        public ActionResult CustomerInvoiceBanksReceipt()
        {
            return View("~/Areas/Accounts/Views/CustomerInvoiceBanksReceipt.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetCustomerInvoiceBanksQueryList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_invoiceWriteOffService.CustomerInvoiceBanksQuery(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerBanksReceipt), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertCustomerInvoiceBanksReceipt(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
           , IEnumerable<VoucherDetailViewModel> banksDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.CustomerBanksReceipt.ToString();
            voucherVM.IsPark = true;
            if (voucherVM.CompanyCurrencyRate <= 0)
                throw new CustomException("Please Input Rate.");
            var bankamount = banksDetailVMList.Sum(r => r.Amount);
            var bankChargeamount = bankChargeDetailVMList == null ? 0 : bankChargeDetailVMList.Sum(r => r.Amount);

            var Totalbankamount = bankamount + bankChargeamount;
            if (bankChargeDetailVMList != null && voucherDetailVMList.Sum(r => r.Amount) != banksDetailVMList.Sum(r => r.Amount) + bankChargeDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Invoice Total amount should same Bank Total Amount and Bank Charges Amount");
            if (bankChargeDetailVMList == null && voucherDetailVMList.Sum(r => r.Amount) != banksDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Invoice Total amount should same Bank Total Amount");

            foreach (var advanceDetailVM in voucherDetailVMList)
            {
                if (advanceDetailVM.Amount == 0 || advanceDetailVM.Amount.ToString() == null)
                    throw new CustomException("Amount should more than 0");
                if (voucherVM.CurrencyId != advanceDetailVM.CurrencyId)
                    throw new CustomException("Transaction currency and Payable currency should be same.!!!");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceWriteOffService.InsertCustomerInvoiceBanksReceipt(voucherVM, voucherDetailVMList, banksDetailVMList, bankChargeDetailVMList)) });
        }






        [HttpPost]
        public ActionResult PostCustomerBanksReceipt(string invoiceWriteOffNo)
        {
            _invoiceWriteOffService.CustomerBanksPost(invoiceWriteOffNo);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public ActionResult DeleteCustomerBanksReceipt(string invoiceWriteOffGroupNo)
        {
            _invoiceWriteOffService.DeleteCustomerBanksReceipt(invoiceWriteOffGroupNo, SourceType.CustomerBanksReceipt);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult CustomerInvoiceReceiptBanksReport(ReportFormat reportFormat, string invoiceWriteOffGroupNo)
        {
            AccountsInvoiceReportService _accInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accInvoiceReportService.GetCustomerInvoiceReceiptBanksReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, invoiceWriteOffGroupNo, SourceType.CustomerBanksReceipt.ToString());
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


        [HttpGet, Authorize]
        public ActionResult GetCustomerInvoiceReceiptBanksReportPdf(ReportFormat reportFormat, string invoiceWriteOffGroupNo)
        {
            try
            {

                AccountsInvoiceReportService _accInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var workbook = _accInvoiceReportService.GetCustomerInvoiceReceiptBanksReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, invoiceWriteOffGroupNo, SourceType.CustomerBanksReceipt.ToString());
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        PdfDocument document = new PdfDocument();
                        ExcelToPdfConverterSettings settings = new ExcelToPdfConverterSettings();
                        settings.TemplateDocument = document;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document = converter1.Convert(settings);
                        }
                        document.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);
                        return null;

                    case ReportFormat.PdfView:
                        PdfDocument document1 = new PdfDocument();
                        ExcelToPdfConverterSettings settings1 = new ExcelToPdfConverterSettings();
                        settings1.TemplateDocument = document1;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document1 = converter1.Convert(settings1);
                        }
                        document1.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Open);
                        //return RenderReportAsPdf(document1, reportFileName);
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


        [HttpGet, Authorize]
        public ActionResult CustomerInvoiceDetailsReceiptBanksReport(ReportFormat reportFormat, string invoiceWriteOffGroupNo)
        {
            AccountsInvoiceReportService _accInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accInvoiceReportService.GetCustomerInvoiceDetailsReceiptBanksReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, invoiceWriteOffGroupNo, SourceType.CustomerBanksReceipt.ToString());
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

        #endregion

        #region Purchase Realization

        public ActionResult SuspensePayable()
        {
            return View("~/Areas/Accounts/Views/SuspensePayable.cshtml");
        }
        [HttpGet]
        public JsonResult GetSuspensesPayableList(GridParameter parameters)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.SuspensesPayableQuery(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.SuspensePayable), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult InsertSuspensesPayable(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
          , IEnumerable<VoucherDetailViewModel> banksDetailVMList, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.SuspensePayable.ToString();
            voucherVM.IsPark = true;
            if (voucherVM.CompanyCurrencyRate <= 0)
                throw new CustomException("Please Input Rate.");
            if (bankChargeDetailVMList != null && voucherDetailVMList.Sum(r => r.Amount) != banksDetailVMList.Sum(r => r.Amount) + bankChargeDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Invoice Total amount should same Bank Total Amount and Bank Charges Amount");
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceWriteOffService.InsertPurchaseRealizationService(voucherVM, voucherDetailVMList, banksDetailVMList, bankChargeDetailVMList)) });
        }
        [HttpPost]
        public ActionResult PostSuspensesPayable(string invoiceGroupNo)
        {
            _invoiceWriteOffService.SuspensePayablePost(invoiceGroupNo);
            return Json(new { Message = AplosMessage.Posted });
        }
        [HttpGet, Authorize]
        public ActionResult SuspensePayableReport(ReportFormat reportFormat, string invoiceGroupNo)
        {
            AccountsInvoiceReportService _accInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var workbook = _accInvoiceReportService.GetSuspensPayableReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, invoiceGroupNo, SourceType.SuspensePayable.ToString());
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
        #endregion Purchase Realization


        public ActionResult MultipleVendorPayment()
        {
            return View("~/Areas/Accounts/Views/MultipleVendorPayment.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult MultipleVendorPaymentApproved()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetMultipleVendorAvailableInvoiceList(GridParameter parameters, string doctate, string docType, string entityId, string partyId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.GetMultipleVendorAvailableInvoiceList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, doctate, docType, entityId, partyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetMultipleVendorList(string column, string value, GridParameter parameters, string docdate, string docType, string entityId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var res = _accountsInvoiceService.GetMultipleVendorListQuery(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value, parameters, docdate, docType, entityId);
            var jsondata = Json(res, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }



        [HttpPost]
        public JsonResult InsertMultipleVendorPayment(MultiplePayment multiplePayment, IEnumerable<MultiplePaymentDetail> multiplePaymentDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            multiplePayment.CompanyGroupId = identity.CompanyGroupId;
            multiplePayment.CompanyId = identity.CompanyId;
            multiplePayment.PlantId = identity.PlantId;
            multiplePayment.SourceType = SourceType.VendorPayment.ToString();
            return Json(new { Message = string.Format(_invoiceService.InsertMultiplePaymnet(multiplePayment, multiplePaymentDetailList)) });
        }

        [HttpPost]
        public JsonResult PostMultipleVendorPayment(VoucherViewModel voucherVM, IEnumerable<MultiplePaymentViewModel> mpSummarylist, IEnumerable<MultiplePaymentDetailViewModel> multiplePaymentDetailList
                , IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.VendorPayment.ToString();
            voucherVM.IsPark = false;
            if (voucherVM.CompanyCurrencyRate <= 0)
                throw new CustomException("Please Input Rate.");

            foreach (var advanceDetailVM in multiplePaymentDetailList)
            {
                if (advanceDetailVM.Amount == 0 || advanceDetailVM.Amount.ToString() == null)
                    throw new CustomException("Amount should more than 0");
                if (voucherVM.CurrencyId != advanceDetailVM.CurrencyId)
                    throw new CustomException("Transaction currency and Payable currency should be same.!!!");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceWriteOffService.PostMultipleVendorPayment(voucherVM, mpSummarylist, multiplePaymentDetailList, bankChargeDetailVMList, taxDetailVMList)) });
        }

        [HttpPost]
        public JsonResult DeleteMultipleVendorRow(IEnumerable<MultiplePayment> multiplePaymentlist, IEnumerable<MultiplePaymentDetail> multiplePaymentDetailList)
        {
            return Json(new { Message = string.Format(AplosMessage.Deleted, _invoiceWriteOffService.DeleteMultipleVendorRow(multiplePaymentlist, multiplePaymentDetailList)) });
        }

        [HttpGet, Authorize]
        public JsonResult GetMultiplePaymentData()
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.GetMultiplePaymentData(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMultiplePaymentParkAndUnApprovedList()
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.GetMultiplePaymentParkList(identity.PlantId), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetMultiplePaymentVoucherList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_invoiceWriteOffService.GetMultiplePaymentVoucherList(parameters, identity.PlantId, SourceType.VendorPayment), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMultipleVendorAvailableDetailList(string multiplePaymentId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.GetMultipleVendorAvailableDetailList(identity.CompanyId, identity.PlantId, multiplePaymentId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMultipleVendorApprovalList(GridParameter parameters, string multiplePaymentId)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.GetMultipleVendorApprovalList(parameters, identity.CompanyGroupId, identity.CompanyId, multiplePaymentId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMultiplePaymentPendingList(GridParameter parameters)
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsInvoiceService.GetMultiplePaymentPendingList(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }



        [HttpPost, Authorize]
        public JsonResult InsertMultipleVendorPaymentApproved(IEnumerable<MultipleVendorIdViewModel> partyIdList, VoucherViewModel voucherVM,
            IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            _invoiceService.InsertMultipleVendorAvailableApproved(partyIdList, voucherVM, voucherDetailVMList, voucherDetailCurrencyVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpGet, Authorize]
        public ActionResult CustomerInvoiceSettlement()
        {
            return View("~/Areas/Accounts/Views/CustomerInvoiceSettlement.cshtml");
        }

        [HttpGet]
        public JsonResult GetCustomerInvoiceSettlementList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_invoiceWriteOffService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.CustomerAdvanceWriteOff), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ParkCustomerInvoiceSettlement(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.CustomerAdvanceWriteOff.ToString();
            voucherVM.VoucherDate = DateTime.Now;
            voucherVM.PostingDate = DateTime.Now;
            voucherVM.DocDate = DateTime.Now;
            _invoiceWriteOffService.InsertCustomerInvoiceWriteOff(voucherVM, voucherDetailVMList);
            return Json(new { Message = string.Format(AplosMessage.Success) });
        }

        [HttpPost]
        public JsonResult UpdateCustomerInvoiceSettlement()
        {
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public ActionResult CustomerInvoiceSettlementReport(ReportFormat reportFormat, string bankJournalId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _invoiceReportService.GetCustomerInvoiceSettlementReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, bankJournalId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSettlementGainLossReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _invoiceReportService.GetSettlementGainLossReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }




        [Authorize, HttpGet]
        public JsonResult GetVoucherWriteOffList(string voucherWriteOffId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_invoiceWriteOffService.GetVoucherWriteOffList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, voucherWriteOffId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetVoucherWriteOffDetailList(string voucherWriteOffId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_invoiceWriteOffService.GetVoucherWriteOffDetailList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, voucherWriteOffId), JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult InsertPartyReconciliation(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.PartyReconcilliation.ToString();
            voucherVM.VoucherDate = DateTime.Now;
            voucherVM.PostingDate = DateTime.Now;
            voucherVM.DocDate = DateTime.Now;

            if (voucherVM.Balance < voucherDetailVMList.Sum(r => r.DrAmount))
                throw new CustomException(" Payment's Amount should not greater than Receivable Balance Amount!");
            if (voucherDetailVMList == null)
                throw new CustomException("Please select Payment!");
            foreach (var advanceDetailVM in voucherDetailVMList)
            {
                if (advanceDetailVM.DrAmount == 0 || advanceDetailVM.DrAmount.ToString() == null)
                    throw new CustomException("Amount should more than 0");
            }
            _invoiceWriteOffService.InsertPartyReconciliation(voucherVM, voucherDetailVMList);
            return Json(new { Message = string.Format(AplosMessage.Success) });
        }

        [HttpPost]
        public JsonResult UpdatePartyReconciliation(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.PartyReconcilliation.ToString();
            voucherVM.VoucherDate = DateTime.Now;
            voucherVM.PostingDate = DateTime.Now;
            voucherVM.DocDate = DateTime.Now;
            //if (voucherVM.Balance < voucherDetailVMList.Sum(r => r.DrAmount))
            //    throw new CustomException(" Payment's Amount should not greater than Receivable Balance Amount!");
            if (voucherDetailVMList == null)
                throw new CustomException("Please select Payment!");
            foreach (var advanceDetailVM in voucherDetailVMList)
            {
                if (advanceDetailVM.DrAmount == 0 || advanceDetailVM.DrAmount.ToString() == null)
                    throw new CustomException("Amount should more than 0");
            }
            _invoiceWriteOffService.UpdatePartyReconciliation(voucherVM, voucherDetailVMList);
            return Json(new { Message = string.Format(AplosMessage.Success) });
        }

        [HttpGet, Authorize]
        public ActionResult PartyReconciliationReport(ReportFormat reportFormat, string voucherWriteOffId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _invoiceReportService.GetPartyReconciliationReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherWriteOffId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }

        [HttpGet, Authorize]
        public ActionResult CustomerInvoiceDetailsReceiptBanksIndividualReport(ReportFormat reportFormat, string invoiceWriteOffGroupNo)
        {
            AccountsInvoiceReportService _accInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accInvoiceReportService.GetCustomerInvoiceDetailsReceiptBanksIndividualReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, invoiceWriteOffGroupNo, SourceType.CustomerBanksReceipt.ToString());
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


        [HttpPost]
        public JsonResult PostPartyReconciliation()
        {
            return Json(new { Message = AplosMessage.Updated });
        }

        #region Invoice Overehead

        [HttpGet, Authorize]
        public ActionResult GetInvoiceOvereheadList()
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            return Json(_accountsInvoiceService.GetInvoiceOvereheadList(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetInvoiceServiceMasterChargesDetail(string invServiceMasterChargesId)
        {
            string sql = @"SELECT SMCD.*,SMC.UserName ServiceMasterCharges  FROM [TRN].[InvoiceServiceMasterChargesDetail] SMCD 
                LEFT JOIN HKP.OverHeadType SMC ON SMC.Id=SMCD.OverHeadTypeId 
                WHERE SMCD.InvoiceServiceMasterChargesId='" + invServiceMasterChargesId + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetInvoiceServiceMasterChargesTax(string invServiceMasterChargesId)
        {
            string sql = @"select ISMD.OverHeadTypeId,ISCT.* from [TRN].[InvoiceServiceMasterChargesTax] ISCT 
                    LEFT JOIN [TRN].[InvoiceServiceMasterChargesDetail] ISMD ON ISMD.Id=ISCT.InvoiceServiceMasterChargesDetailId
                        WHERE ISCT.InvoiceServiceMasterChargesId='" + invServiceMasterChargesId + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetInboundInvoiceDetailCharges(string invServiceMasterChargesId)
        {
            string sql = @"select IDC.*,IV.CompanyCurrencyRate,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') AS PostingDate,P.UserName PartyPlantName,C.Code CurrencyCode,(IV.CompanyCurrencyRate*IDC.Amount) BooksAmount
                            ,V.VoucherNo 
                            from [TRN].[InvoiceDetailCharges] IDC 
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IDC.InvoiceId
                            LEFT JOIN TRN.Voucher V ON V.Id=IV.VoucherId
                            LEFT JOIN HKP.Party P ON P.Id=IV.PartyId
                            LEFT JOIN SCS.Currency C ON C.Id=IV.CurrencyId
                            WHERE IDC.InvoiceServiceMasterChargesId='" + invServiceMasterChargesId + "' AND IDC.InvoiceType='" + InvoiceTypeEnum.InBound.ToString() + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetOutboundInvoiceDetailCharges(string invServiceMasterChargesId)
        {
            string sql = @"select IDC.*,IV.CompanyCurrencyRate,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') AS PostingDate,P.UserName PartyPlantName,C.Code CurrencyCode
                            ,(IV.CompanyCurrencyRate*IDC.Amount) BooksAmount
                            ,V.VoucherNo 
                            from [TRN].[InvoiceDetailCharges] IDC 
                            LEFT JOIN TRN.Invoice IV ON IV.Id=IDC.InvoiceId
                            LEFT JOIN TRN.Voucher V ON V.Id=IV.VoucherId
                            LEFT JOIN HKP.Party P ON P.Id=IV.PartyId
                            LEFT JOIN SCS.Currency C ON C.Id=IV.CurrencyId
                            WHERE IDC.InvoiceServiceMasterChargesId='" + invServiceMasterChargesId + "' AND IDC.InvoiceType='" + InvoiceTypeEnum.OutBound.ToString() + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertInvoiceOverhead(VoucherViewModel voucherVM, IEnumerable<ServiceChargesViewModel> voucherDetailVMList, IEnumerable<ServiceChargesTaxViewModel> taxDetailVMList, IEnumerable<InvoiceDetailCharges> invoiceDetailChargesList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            if (voucherVM.CompanyCurrencyRate == 0)
                throw new CustomException("Rate can not Empty!");

            voucherVM.SourceType = SourceType.InvoiceOverhead.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceService.InsertInvoiceOverhead(voucherVM, voucherDetailVMList, taxDetailVMList, invoiceDetailChargesList)) });
        }


        [HttpPost]
        public ActionResult UpdateInvoiceOverhead(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            if (voucherVM.CompanyCurrencyRate == 0)
                throw new CustomException("Rate can not Empty!");
            if (voucherVM.PaymentSource == PaymentSource.GL.ToString())
            {
                if (voucherVM.IsExcludingTax == false && voucherVM.Amount != voucherDetailVMList.Sum(r => r.TotalAmount))
                    throw new CustomException("Total Amount and Invoice Amount not match!");
                else if (voucherVM.IsExcludingTax == true && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                    throw new CustomException("Net Amount and Invoice Amount not match!");
            }

            if (voucherVM.PaymentSource == PaymentSource.Cash.ToString() && voucherVM.CashMasterId == null)
                throw new CustomException(Resources.SelectCash);
            voucherVM.SourceType = SourceType.VendorInvoice.ToString();
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _invoiceService.UpdateVendorInvoice(voucherVM, voucherDetailVMList)) });
        }

        [HttpGet]
        public ActionResult GetInvoiceOvereheadPostingList()
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            return Json(_accountsInvoiceService.GetInvoiceOvereheadPostingList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetInvoiceOvereheadPostedList()
        {
            AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
            return Json(_accountsInvoiceService.GetInvoiceOvereheadPostedList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetInvoiceServiceMasterChargesDetailPosting(string invServiceMasterChargesId)
        {
            string sql = @"SELECT SMCD.*,OHT.UserName ServiceMasterCharges,OHG.ExpensesGLId,OHG.ExpensesBudgetMasterId,OHG.ExpensesActivityId  
						FROM [TRN].[InvoiceServiceMasterChargesDetail] SMCD 
						LEFT JOIN HKP.OverHeadType OHT ON OHT.Id=SMCD.OverHeadTypeId 
						LEFT JOIN HKP.OverHeadTypeGL OHG ON OHG.OverHeadTypeId=OHT.Id  
                WHERE SMCD.InvoiceServiceMasterChargesId='" + invServiceMasterChargesId + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PostInvoiceOverhead(VoucherViewModel voucherVM, IEnumerable<ServiceChargesViewModel> voucherDetailVMList)
        {
            voucherVM.PostingDate = voucherVM.DocDate;
            voucherVM.SourceType = SourceType.InvoiceOverhead.ToString();
            _invoiceService.InsertInvoiceOverheadPost(voucherVM, voucherDetailVMList);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public ActionResult DeleteInvoiceOverhead(string invoiceId, string voucherId)
        {
            _invoiceService.DeleteInvoiceOverhead(invoiceId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpGet, Authorize]
        public ActionResult ReportInvoiceOverheadVoucher(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _invoiceReportService.GetInvoiceOverheadReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }

        }

        #endregion

        #region Date range wise payable and Payment report

        //[HttpGet, Authorize]
        //public ActionResult GetDateRangeWiseReport( string fromDate, string toDate)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    AccountsInvoiceReportService accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
        //    try
        //    {
        //        ExcelEngine excelEngine = new ExcelEngine();
        //        //IWorkbook workbook = IssueReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, checkbox);
        //        // IWorkbook workbook = OperationReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
        //        IWorkbook workbook = accountsInvoiceReportService.GetDateRangeWiseReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate);

        //        string strFileName = "DateRangePayableList.xlsx";
        //        workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
        //        workbook.Close();
        //    }
        //    catch (CustomException ex)
        //    {
        //        return Json(ex.Message, JsonRequestBehavior.AllowGet);

        //    }
        //    return null;
        //}

        [HttpGet, Authorize]
        public ActionResult GetDateRangeWisePaymentReport(string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInvoiceReportService accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();
                IWorkbook workbook = accountsInvoiceReportService.GetDateRangeWisePaymentReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate);
                string strFileName = "DateRangePaymentList.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (CustomException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }

        //GetDateRangeWiseDetailPaymentPoPUpReport
        //[HttpGet, Authorize]
        //public ActionResult GetDateRangeWiseDetailPaymentPoPUpReport(string fromDate, string toDate, string id, string type)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    AccountsInvoiceReportService accountsInvoiceReportService = new AccountsInvoiceReportService(_sqlRepository);
        //    try
        //    {
        //        ExcelEngine excelEngine = new ExcelEngine();
        //        IWorkbook workbook = accountsInvoiceReportService.GetDateRangeWiseDetailPaymentPoPUpReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate,id, type);
        //        string strFileName = "DateRangePaymentDetailList.xlsx";
        //        workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
        //        workbook.Close();
        //    }
        //    catch (CustomException ex)
        //    {
        //        return Json(ex.Message, JsonRequestBehavior.AllowGet);

        //    }
        //    return null;
        //}
        #endregion


        #region Attachment

        [HttpPost, Authorize]
        public ActionResult SaveDefault(IEnumerable<HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\"", "");
                if (string.IsNullOrEmpty(UploadDefault_data))
                    throw new Exception("Save the order first");

                foreach (var file in UploadDefault)
                {

                    var fileName = Path.GetFileName(UploadDefault_data + new FileInfo(file.FileName).Extension);
                    var fileN = file.FileName;
                    var destinationPath = Path.Combine(ResourcesPathReader.GetInvoiceDocumentPath(), fileName);

                    var directory = ResourcesPathReader.GetInvoiceDocumentPath();
                    var path = Path.Combine(directory);

                    if (System.IO.Directory.Exists(ResourcesPathReader.GetInvoiceDocumentPath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.GetInvoiceDocumentPath());
                        }
                        catch (Exception)
                        {

                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "SELECT * FROM [TRN].[Invoice] WHERE Id='" + UploadDefault_data + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();
                    var FN = dsLocal.Tables[0].Rows[0]["FileName"].ToString();
                    if (fileN != FN)
                        if (System.IO.File.Exists(path + UploadDefault_data + Path.GetExtension(FN)))
                            System.IO.File.Delete(path + UploadDefault_data + Path.GetExtension(FN));

                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        dsLocal.Tables[0].Rows[0].BeginEdit();

                        dsLocal.Tables[0].Rows[0]["FileName"] = fileN;

                        dsLocal.Tables[0].Rows[0].EndEdit();

                        file.SaveAs(destinationPath);
                        OTSBD.clsStaticInfo info = new OTSBD.clsStaticInfo();
                        info.SaveDataSets(dsLocal);



                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.Status = "204 No Content";
                Response.StatusDescription = ex.Message;
                Response.End();

                return Content("");
            }

        }



        #endregion

        #region Payment Advice
        //[Authorize, HttpGet]
        //public JsonResult GatePaymentAdviceData(string fromDate, string toDate, string BankMasterId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

        //    var jsondata = Json(_invoiceWriteOffService.GetGatePaymentAdviceData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate, BankMasterId), JsonRequestBehavior.AllowGet);
        //    jsondata.MaxJsonLength = int.MaxValue;
        //    return jsondata;
        //}

        [HttpGet, Authorize]
        public ActionResult GetPaymentAdviceReport(ReportFormat reportFormat, string adviceNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _invoiceWriteOffService.PaymentAdviceReportxlx(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, adviceNo);
            switch (reportFormat)
            {
                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);
                case ReportFormat.Pdf:
                    return RenderReportAsExcel(workbook, reportFileName);
                default:
                    return View();
            }
        }

        #endregion Payment Advice

        #region Multiple Vendor Payment Start
        [HttpGet, Authorize]
        public JsonResult GetMultiplePaymentMyAppData(string tabType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = "";
            if (tabType == "UnApprovedList")
            {
                sql = @"SELECT MP.Id,MP.CompanyGroupId,MP.CompanyId,MP.PlantId,MP.SourceType,Replace(CONVERT(VARCHAR(11), MP.DueUpToDate, 106), ' ', '-') DueUpToDate
                            ,Replace(CONVERT(VARCHAR(11), MP.TentativeDate, 106), ' ', '-') TentativeDate
                            ,MP.BankMasterId,MP.IsFifo,MP.IsPark ,BM.AccountTitle, 0 flag --,P.UserName PartyName,MPD.PartyId
                            ,SUM(MPD.Amount) Amount
							,ParkStatus=case when MP.IsPark=1 then 'Parked' else 'Posted' end
                            FROM TRN.MultiplePaymentDetail MPD 
							LEFT JOIN TRN.MultiplePayment MP ON MP.Id=MPD.MultiplePaymentId
							LEFT JOIN HKP.Party P ON P.Id=MPD.PartyId
							LEFT JOIN MST.BankMaster BM ON BM.Id=MP.BankMasterId
							where  MP.PlantId='" + identity.PlantId + @"' and MP.ApprovedBy='" + identity.EmployeeId + @"' and MP.ApprovalStatus='Pending'
							group by MP.Id,MP.CompanyGroupId,MP.CompanyId,MP.PlantId,MP.SourceType,MP.DueUpToDate
                            , MP.TentativeDate,MPD.MultiplePaymentId
                            ,MP.BankMasterId,MP.IsFifo,MP.IsPark ,BM.AccountTitle ";

            }
            else if (tabType == "HoldRejectList")
            {
                sql = @"SELECT MP.Id,MP.CompanyGroupId,MP.CompanyId,MP.PlantId,MP.SourceType,Replace(CONVERT(VARCHAR(11), MP.DueUpToDate, 106), ' ', '-') DueUpToDate
                            ,Replace(CONVERT(VARCHAR(11), MP.TentativeDate, 106), ' ', '-') TentativeDate
                            ,MP.BankMasterId,MP.IsFifo,MP.IsPark ,BM.AccountTitle, 0 flag ,P.UserName PartyName,MPD.PartyId,SUM(MPD.Amount) Amount
							,ParkStatus=case when MP.IsPark=1 then 'Parked' else 'Posted' end
                            FROM TRN.MultiplePaymentDetail MPD 
							LEFT JOIN TRN.MultiplePayment MP ON MP.Id=MPD.MultiplePaymentId
							LEFT JOIN HKP.Party P ON P.Id=MPD.PartyId
							LEFT JOIN MST.BankMaster BM ON BM.Id=MP.BankMasterId
							where  MP.PlantId='" + identity.PlantId + @"' and MP.ApprovedBy='" + identity.EmployeeId + @"'  and MP.ApprovalStatus='Reject'
							group by MP.Id,MP.CompanyGroupId,MP.CompanyId,MP.PlantId,MP.SourceType,MP.DueUpToDate
                            , MP.TentativeDate,MPD.MultiplePaymentId
                            ,MP.BankMasterId,MP.IsFifo,MP.IsPark ,BM.AccountTitle,P.UserName,MPD.PartyId ";

            }
            else
            {
                sql = @"SELECT MP.Id,MP.CompanyGroupId,MP.CompanyId,MP.PlantId,MP.SourceType,Replace(CONVERT(VARCHAR(11), MP.DueUpToDate, 106), ' ', '-') DueUpToDate
                            ,Replace(CONVERT(VARCHAR(11), MP.TentativeDate, 106), ' ', '-') TentativeDate
                            ,MP.BankMasterId,MP.IsFifo,MP.IsPark ,BM.AccountTitle, 0 flag ,P.UserName PartyName,MPD.PartyId,SUM(MPD.Amount) Amount
							,ParkStatus=case when MP.IsPark=1 then 'Parked' else 'Posted' end
                            FROM TRN.MultiplePaymentDetail MPD 
							LEFT JOIN TRN.MultiplePayment MP ON MP.Id=MPD.MultiplePaymentId
							LEFT JOIN HKP.Party P ON P.Id=MPD.PartyId
							LEFT JOIN MST.BankMaster BM ON BM.Id=MP.BankMasterId
							where  MP.PlantId='" + identity.PlantId + @"' and MP.ApprovedBy='" + identity.EmployeeId + @"'  and MP.ApprovalStatus='Approved'
							group by MP.Id,MP.CompanyGroupId,MP.CompanyId,MP.PlantId,MP.SourceType,MP.DueUpToDate
                            , MP.TentativeDate,MPD.MultiplePaymentId
                            ,MP.BankMasterId,MP.IsFifo,MP.IsPark ,BM.AccountTitle,P.UserName,MPD.PartyId ";

            }
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CreateUnApproveBy(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TRN.MultiplePayment where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                #region data update
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    data["ApprovalStatus"] = data["ApprovedByStatus"];
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update 

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        [Authorize, HttpGet]
        public JsonResult GetMultipleVendorPaymentApproveByCboList()
        {
            return Json(clsSales.GetMultipleVendorPaymentApproveByCboList(), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]

        public ActionResult MultipleVendorPaymentReport(ReportFormat reportFormat, string mvpId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = clsSales.MultipleVendorPaymentReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, mvpId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }

        #endregion Multiple Vendor Payment End

        #region InvoiceReviseMatureDate
        public ActionResult InvoiceReviseMatureDate()
        {
            return View("~/Areas/Accounts/Views/InvoiceReviseMatureDate.cshtml");
        }

        [HttpPost, Authorize]
        public ActionResult GetInvoiceReviseMatureDateList(string partyType,string FromDate, string ToDate, bool DateRange)
        {
            try
            {
                AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var jsondata = Json(_accountsInvoiceService.GetInvoiceReviseMatureDateList(identity.CompanyGroupId, identity.CompanyId, partyType, FromDate, ToDate, DateRange), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost]
        public void UpdateInvoiceReviseDate(string reviseDate,List<Invoice> invoiceList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string IdLoop = "";
                foreach (var item in invoiceList)
                {
                    if (IdLoop == "")
                    {
                        IdLoop = "'" + item.Id + "'"; ;
                    }
                    else
                    {
                        IdLoop += ",'" + item.Id + "'";

                    }
                }

                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";
                vendorAdWrsql = @"update [TRN].[Invoice] set RevisedDueDate='"+ reviseDate + "', UpdatedBy='" + identity.Name + "', UpdatedDate='" + DateTime.Now + "', UpdatedFromIP='" + identity.IPAddress + "' where Id IN (" + IdLoop + @") ";
                vendorAdWr.Append(vendorAdWrsql);
                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

    }
}