using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Model.Enums;
using Library.Service.Invoices;
using Library.MaterialManagement.Reports;
using Library.Service.Vouchers;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Library.Data;
using System.Linq;
using Library.Core;
using Library.ViewModel.Materials;
using Library.ViewModel.Inventory;
using Library.ViewModel.Invoices;
using Library.Model.Payments;
using Library.Accounting.Accounts;
using Library.Data.Sql;
using Library.Model.Accounts;

namespace Aplos.Areas.Accounts.Controllers
{
    public class InvoicePostController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly IInventoryPayableService _inventoryPayableService;

        public InvoicePostController(
            IInventoryPayableService inventoryPayableService
             , ISqlRepository sqlRepository
            )
        {
            _inventoryPayableService = inventoryPayableService;
            _sqlRepository = sqlRepository;
        }



        #region GRN Payable



        [Authorize,HttpPost]
        public JsonResult GRNPost(string receiveId, string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
         , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList
         , IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
         , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList
         , IEnumerable<InvoiceTaxViewModel> tdsTaxList, IEnumerable<VoucherDetailViewModel> otherVendorChargesList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            if (voucherVM.IsInvoice && voucherVM.EmployeeId == null && voucherVM.PaymentTermId == null)
                throw new CustomException("Please select Payment Term");
            if (voucherVM.IsInvoice && voucherVM.BaseOnDueDate == null)
                throw new CustomException("Please inpute BaseOnDueDate Term");
            if (voucherVM.IsInvoice && voucherVM.EmployeeId == null && voucherVM.BaseNoOfDays == 0 || voucherVM.IsInvoice && voucherVM.BaseNoOfDays < 0)
                throw new CustomException("Please inpute BaseNoOfDays Term");

            if (voucherDetailVMList != null)
            {
                foreach (var item in voucherDetailVMList)
                {
                    if(item.BudgetActive==false || item.BudgetMasterActivityActive == false)
                    {
                        throw new CustomException(item.ActivityName+" Budget Or Activity is not Active");
                    }
                    if (item.IsAsset)
                    {
                        if (item.GLGeneralInfoId == null)
                            throw new CustomException("AUC GL is Not Mapped !");
                        if (item.BudgetMasterId == null)
                            throw new CustomException("AUC Budget is Not Mapped !");
                        if (item.ActivityId == null)
                            throw new CustomException(" AUC Activity is Not Mapped!");
                    }
                    else
                    {
                        if (item.GLGeneralInfoId == null)
                            throw new CustomException("GL is Not Mapped !");
                        if (item.BudgetMasterId == null)
                            throw new CustomException("Budget is Not Mapped !");
                        if (item.ActivityId == null)
                            throw new CustomException("Activity is Not Mapped!");
                    }

                }

                if (voucherDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != voucherDetailVMList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                    throw new CustomException("Dr Cr Amount not equal");
            }
            else
                throw new CustomException("No Journal");

            if (voucherVM.EmployeeId != null)
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _inventoryPayableService.InsertEmployeePayable(receiveId, voucherVM, voucherDetailVMList, voucherDetailCurrencyVMList, inventoryReceiveDetailVMList)) });
            else
                return Json(new
                {
                    Message = string.Format(AplosMessage.VoucherSave, _inventoryPayableService.InsertInventoryPayable(receiveId, acceptanceId, voucherVM, voucherDetailVMList, voucherDetailCurrencyVMList
                        , inventoryPayableVMList, inventoryReceiveDetailVMList, tdsTaxList,otherVendorChargesList))
                });


        }



        [HttpPost, Authorize]
        public JsonResult InsertAdditionalTaxPayable(string additionalTaxId, VoucherViewModel voucherVM)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.VendorPayment.ToString();
            voucherVM.PaymentSource = PaymentSource.Tax.ToString();
            voucherVM.PartyType = "Vendor";
            _inventoryPayableService.InsertAdditionalTaxPayable(voucherVM, additionalTaxId);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost, Authorize]
        public JsonResult InsertShortageDebitNote( VoucherViewModel voucherVM,string grnId, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.DebitNote.ToString();
            voucherVM.PartyType = "Vendor";
            voucherVM.DocRefNo = "DN-" + voucherVM.DocRefNo;
            _inventoryPayableService.InsertShortageDebitNote(voucherVM, grnId, voucherDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }


        [HttpPost, Authorize]
        public JsonResult InsertCreditNoteAdditionalTaxPost(string additionalTaxId, VoucherViewModel voucherVM)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.VendorPayment.ToString();
            voucherVM.PaymentSource = PaymentSource.Tax.ToString();
            voucherVM.PartyType = "Customer";
            _inventoryPayableService.InsertCreditNoteAdditionalTaxPost(voucherVM, additionalTaxId);
            return Json(new { Message = AplosMessage.Insert });
        }



        #endregion

        #region Service Payable

        [Authorize, HttpPost]
        public JsonResult ServicePost(string receiveId, string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
        , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList
        , IEnumerable<VoucherDetailViewModel> serviceDetailGLList
        , IEnumerable<ServiceAcknowledgementDetailViewModel> inventoryReceiveDetailVMList, IEnumerable<InvoiceTaxViewModel> tdsTaxList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            if (acceptanceId == null && voucherVM.PaymentTermId == null)
                throw new CustomException("Please select Payment Term");

            if (voucherDetailVMList != null)
            {
                foreach (var item in voucherDetailVMList)
                {
                    if (item.IsAsset)
                    {
                        if (item.GLGeneralInfoId == null)
                            throw new CustomException("AUC GL is Not Mapped !");
                        if (item.BudgetMasterId == null)
                            throw new CustomException("AUC Budget is Not Mapped !");
                        if (item.ActivityId == null)
                            throw new CustomException(" AUC Activity is Not Mapped!");
                    }
                    else
                    {
                        if (item.GLGeneralInfoId == null)
                            throw new CustomException("GL is Not Mapped !");
                        if (item.BudgetMasterId == null)
                            throw new CustomException("Budget is Not Mapped !");
                        if (item.ActivityId == null)
                            throw new CustomException("Activity is Not Mapped!");
                    }

                }

                if (voucherDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != voucherDetailVMList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                    throw new CustomException("Dr Cr Amount not equal");
            }
            else
                throw new CustomException("No Journal");


            _inventoryPayableService.InsertServicePayable(receiveId, voucherVM, voucherDetailVMList, voucherDetailCurrencyVMList, serviceDetailGLList, inventoryReceiveDetailVMList, tdsTaxList);

            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public ActionResult DeleteTDSPostServicePayable( string voucherId, string serviceAckId, string invoiceWriteOffId)
        {
            _inventoryPayableService.DeleteTDSPostServicePayable(invoiceWriteOffId,voucherId,  serviceAckId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public ActionResult DeleteTDSServicePayable(string additionalTaxId,string voucherId)
        {
            _inventoryPayableService.DeleteTDSServicePayable(additionalTaxId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion

        #region Issue Journal



        [Authorize,HttpPost]
        public JsonResult CreateIssue(string issueId, string voucherTypeId, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<InventoryMaterialViewModel> invIssueDetailList, IEnumerable<InventoryMaterialViewModel> invIssueDetailGLList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var voucherVM = new VoucherViewModel
            {
                CompanyGroupId = identity.CompanyGroupId,
                CompanyId = identity.CompanyId,
                PlantId = identity.PlantId,
                VoucherTypeId = voucherTypeId,
                CompanyCurrencyRate = 1,
                PostingDate = DateTime.Now
            };

            foreach (var item in voucherDetailVMList)
            {
                if (item.GLGeneralInfoId == null)
                    throw new CustomException("GL is Not Mapped !");
                if (item.BudgetMasterId == null)
                    throw new CustomException("Budget is Not Mapped !");
                if (item.ActivityId == null)
                    throw new CustomException("Activity is Not Mapped!");

            }
            foreach (var issDetail in invIssueDetailList)
            {
                if (issDetail.BudgetMasterId == null)
                    throw new CustomException("Budget is Not Mapped !");
                if (issDetail.ActivityId == null)
                    throw new CustomException("Activity is Not Mapped!");
            }
            if (voucherDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != voucherDetailVMList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not equal");
            _inventoryPayableService.InsertIssueJournal(issueId, voucherVM, voucherDetailVMList, invIssueDetailList, invIssueDetailGLList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public ActionResult DeleteIssueJournal(string issueId, string voucherId)
        {
            _inventoryPayableService.DeleteIssueJournal(issueId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion

        #region Issue Journal



        [Authorize, HttpPost]
        public JsonResult IssueReturnJournal(string issueId, string voucherTypeId, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
            , IEnumerable<InventoryMaterialViewModel> invIssueDetailList, IEnumerable<InventoryMaterialViewModel> invIssueDetailGLList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var voucherVM = new VoucherViewModel
            {
                CompanyGroupId = identity.CompanyGroupId,
                CompanyId = identity.CompanyId,
                PlantId = identity.PlantId,
                VoucherTypeId = voucherTypeId,
                PostingDate = DateTime.Now
            };

            foreach (var item in voucherDetailVMList)
            {
                if (item.GLGeneralInfoId == null)
                    throw new CustomException("GL is Not Mapped !");
                if (item.BudgetMasterId == null)
                    throw new CustomException("Budget is Not Mapped !");
                if (item.ActivityId == null)
                    throw new CustomException("Activity is Not Mapped!");

            }
            foreach (var issDetail in invIssueDetailList)
            {
                if (issDetail.BudgetMasterId == null)
                    throw new CustomException("Budget is Not Mapped !");
                if (issDetail.ActivityId == null)
                    throw new CustomException("Activity is Not Mapped!");
            }
            if (voucherDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != voucherDetailVMList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not equal");
            _inventoryPayableService.InsertIssueReturnJournal(issueId, voucherVM, voucherDetailVMList, invIssueDetailList, invIssueDetailGLList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public ActionResult DeleteIssueReturnJournal(string issueId, string voucherId)
        {
            _inventoryPayableService.DeleteIssueReturnJournal(issueId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion

        #region Shortage




        [HttpPost, Authorize]
        public JsonResult CreateShortagePayable(string receiveId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            if (voucherDetailVMList != null)
            {
                foreach (var item in voucherDetailVMList)
                {
                    if (item.GLGeneralInfoId == null)
                        throw new CustomException("GL is Not Mapped !");
                    if (item.BudgetMasterId == null)
                        throw new CustomException("Budget is Not Mapped !");
                    if (item.ActivityId == null)
                        throw new CustomException("Activity is Not Mapped!");
                }
                if (voucherDetailVMList.Sum(r => r.DrAmount) != voucherDetailVMList.Sum(r => r.CrAmount))
                    throw new CustomException("Dr Cr Amount not equal");
            }
            _inventoryPayableService.InsertInventoryShortagePayable(receiveId, voucherVM, voucherDetailVMList, voucherDetailCurrencyVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        #endregion

        #region Reject




        [HttpPost, Authorize]
        public JsonResult CreateRejectPayable(string receiveId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList, IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            if (voucherDetailVMList != null)
            {
                foreach (var item in voucherDetailVMList)
                {
                    if (item.GLGeneralInfoId == null)
                        throw new CustomException("GL is Not Mapped !");
                    if (item.BudgetMasterId == null)
                        throw new CustomException("Budget is Not Mapped !");
                    if (item.ActivityId == null)
                        throw new CustomException("Activity is Not Mapped!");
                }
                if (voucherDetailVMList.Sum(r => r.DrAmount) != voucherDetailVMList.Sum(r => r.CrAmount))
                    throw new CustomException("Dr Cr Amount not equal");
            }
            _inventoryPayableService.InsertInventoryRejectPayable(receiveId, voucherVM, voucherDetailVMList, voucherDetailCurrencyVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        #endregion

        #region Inventory Sales Posting

        [HttpPost, Authorize]
        public JsonResult PostInventorySales(string receiveId, string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
   , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
   , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList, IEnumerable<VoucherDetailViewModel> inventoryJVList, bool IsInventorySalesBook, OtherInvoice otherInvoiceVM)
        {
            if (IsInventorySalesBook)
            {
                
                return Json(InventorySalesMultipleJournalPosting(receiveId, acceptanceId, voucherVM, voucherDetailVMList
                        , voucherDetailCurrencyVMList, inventoryPayableVMList, inventoryReceiveDetailVMList, inventoryJVList, otherInvoiceVM));
            }
            else
            {
                return Json(InventorySalesSingleJournalPosting(receiveId, acceptanceId, voucherVM, voucherDetailVMList
                       , voucherDetailCurrencyVMList, inventoryPayableVMList, inventoryReceiveDetailVMList, otherInvoiceVM));
            }

        }

        [HttpPost, Authorize]
        public JsonResult InventorySalesSingleJournalPosting(string receiveId, string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
         , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList
         , IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
         , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList, OtherInvoice otherInvoiceVM)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            if (acceptanceId == null && voucherVM.PaymentTermId == null)
                throw new CustomException("Please select Payment Term");

            if (voucherDetailVMList != null)
            {
                foreach (var item in voucherDetailVMList)
                {

                    if (item.GLGeneralInfoId == null)
                        throw new CustomException("GL is Not Mapped !");
                    if (item.BudgetMasterId == null)
                        throw new CustomException("Budget is Not Mapped !");
                    if (item.ActivityId == null)
                        throw new CustomException("Activity is Not Mapped!");

                }

                if (voucherDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != voucherDetailVMList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                    throw new CustomException("Dr Cr Amount not equal");
            }
            else
                throw new CustomException("No Journal");

            _inventoryPayableService.PostSingleJournalSales(receiveId, acceptanceId, voucherVM, voucherDetailVMList, voucherDetailCurrencyVMList, inventoryPayableVMList, inventoryReceiveDetailVMList, otherInvoiceVM);

            return Json(new { Message = AplosMessage.Insert });
        }
        [HttpPost, Authorize]
        public JsonResult InventorySalesMultipleJournalPosting(string receiveId, string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
        , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
        , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList, IEnumerable<VoucherDetailViewModel> inventoryJVList, OtherInvoice otherInvoiceVM)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            if (acceptanceId == null && voucherVM.PaymentTermId == null)
                throw new CustomException("Please select Payment Term");

            if (voucherDetailVMList != null)
            {
                foreach (var item in voucherDetailVMList)
                {

                    if (item.GLGeneralInfoId == null)
                        throw new CustomException("GL is Not Mapped !");
                    if (item.BudgetMasterId == null)
                        throw new CustomException("Budget is Not Mapped !");
                    if (item.ActivityId == null)
                        throw new CustomException("Activity is Not Mapped!");

                }

                if (voucherDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != voucherDetailVMList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                    throw new CustomException("Dr Cr Amount not equal");
            }
            if (inventoryJVList != null)
            {
                foreach (var item in inventoryJVList)
                {

                    if (item.GLGeneralInfoId == null)
                        throw new CustomException("Inventory GL is Not Mapped !");
                    if (item.BudgetMasterId == null)
                        throw new CustomException("Inventory Budget is Not Mapped !");
                    if (item.ActivityId == null)
                        throw new CustomException("Inventory Activity is Not Mapped!");

                }

                if (inventoryJVList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != inventoryJVList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                    throw new CustomException("Inventory Dr Cr Amount not equal");
            }
            else
                throw new CustomException("No Journal");

            _inventoryPayableService.PostMultipleJournalSales(receiveId, acceptanceId, voucherVM, voucherDetailVMList, voucherDetailCurrencyVMList, inventoryPayableVMList, inventoryReceiveDetailVMList, inventoryJVList, otherInvoiceVM);

            return Json(new { Message = AplosMessage.Insert });
        }

     

        [HttpGet, Authorize]
        public ActionResult ReceivableJournal(ReportFormat reportFormat, string inventoryReceiveId, string employeeId, bool isReversCharge, bool isFoc, string otherVendorId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
            var reportFileName = "GRN";
            var workbook = accountsInventoryPayableReportService.PabyableJournal(identity.CompanyId, identity.PlantId, inventoryReceiveId, employeeId, isReversCharge, isFoc, reportFileName, otherVendorId);
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
        #region Inventory Sales Return
        [HttpPost, Authorize]
        public JsonResult InventorySalesReturnMultipleJournalPosting(string receiveId, string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
        , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
        , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList, IEnumerable<VoucherDetailViewModel> inventoryJVList, OtherInvoice otherInvoiceVM)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            if (acceptanceId == null && voucherVM.PaymentTermId == null)
                throw new CustomException("Please select Payment Term");

            if (voucherDetailVMList != null)
            {
                foreach (var item in voucherDetailVMList)
                {

                    if (item.GLGeneralInfoId == null)
                        throw new CustomException("GL is Not Mapped !");
                    if (item.BudgetMasterId == null)
                        throw new CustomException("Budget is Not Mapped !");
                    if (item.ActivityId == null)
                        throw new CustomException("Activity is Not Mapped!");

                }

                if (voucherDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != voucherDetailVMList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                    throw new CustomException("Dr Cr Amount not equal");
            }
            if (inventoryJVList != null)
            {
                foreach (var item in inventoryJVList)
                {

                    if (item.GLGeneralInfoId == null)
                        throw new CustomException("Inventory GL is Not Mapped !");
                    if (item.BudgetMasterId == null)
                        throw new CustomException("Inventory Budget is Not Mapped !");
                    if (item.ActivityId == null)
                        throw new CustomException("Inventory Activity is Not Mapped!");

                }

                if (inventoryJVList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != inventoryJVList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                    throw new CustomException("Inventory Dr Cr Amount not equal");
            }
            else
                throw new CustomException("No Journal");

            _inventoryPayableService.PostMultipleJournalSalesReturn(receiveId, acceptanceId, voucherVM, voucherDetailVMList, voucherDetailCurrencyVMList, inventoryPayableVMList, inventoryReceiveDetailVMList, inventoryJVList, otherInvoiceVM);

            return Json(new { Message = AplosMessage.Insert });
        }


        #endregion
        #region Inventory Transfer Posting

        [HttpPost, Authorize]
        public JsonResult PostInventoryTransfer(string receiveId, VoucherViewModel voucherVM
       , IEnumerable<VoucherDetailViewModel> fromPlantInventoryTransferJVList
       , IEnumerable<VoucherDetailViewModel> toPlantInventoryTransferJVList
       , IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
        )
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;

            if (fromPlantInventoryTransferJVList != null)
            {
                foreach (var item in fromPlantInventoryTransferJVList)
                {
                    if (item.IsAsset)
                    {
                        if (item.GLGeneralInfoId == null)
                            throw new CustomException("AUC GL is Not Mapped !");
                        if (item.BudgetMasterId == null)
                            throw new CustomException("AUC Budget is Not Mapped !");
                        if (item.ActivityId == null)
                            throw new CustomException(" AUC Activity is Not Mapped!");
                    }
                    else
                    {
                        if (item.GLGeneralInfoId == null)
                            throw new CustomException("GL is Not Mapped !");
                        if (item.BudgetMasterId == null)
                            throw new CustomException("Budget is Not Mapped !");
                        if (item.ActivityId == null)
                            throw new CustomException("Activity is Not Mapped!");
                    }

                }
            }
            if (toPlantInventoryTransferJVList != null)
            {
                foreach (var item in toPlantInventoryTransferJVList)
                {
                    if (item.IsAsset)
                    {
                        if (item.GLGeneralInfoId == null)
                            throw new CustomException("AUC GL is Not Mapped !");
                        if (item.BudgetMasterId == null)
                            throw new CustomException("AUC Budget is Not Mapped !");
                        if (item.ActivityId == null)
                            throw new CustomException(" AUC Activity is Not Mapped!");
                    }
                    else
                    {
                        if (item.GLGeneralInfoId == null)
                            throw new CustomException("GL is Not Mapped !");
                        if (item.BudgetMasterId == null)
                            throw new CustomException("Budget is Not Mapped !");
                        if (item.ActivityId == null)
                            throw new CustomException("Activity is Not Mapped!");
                    }

                }


            }
            else
                throw new CustomException("No Journal");
            if (fromPlantInventoryTransferJVList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != fromPlantInventoryTransferJVList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not equal");
            if (toPlantInventoryTransferJVList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != toPlantInventoryTransferJVList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not equal");
            return Json(new
            {
                Message = string.Format(AplosMessage.VoucherSave, _inventoryPayableService.InsertInventoryTransferPayable(receiveId, voucherVM, fromPlantInventoryTransferJVList, toPlantInventoryTransferJVList, inventoryPayableVMList))
            });


        }

        #endregion

        #region Inventory OutSource Received
        [HttpPost, Authorize]
        public JsonResult InventoryOutSourceReceivedPost(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> inventoryJobWorkWIPList
        , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList
        , IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
        , IEnumerable<VoucherDetailViewModel> changeInInventoryList
        , IEnumerable<VoucherDetailViewModel> inventoryJobWorkGIRIList
        , VoucherViewModel ServiceVM
        )
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            //if (voucherVM.IsInvoice && voucherVM.EmployeeId == null && voucherVM.PaymentTermId == null)
            //    throw new CustomException("Please select Payment Term");
            //if (voucherVM.IsInvoice && voucherVM.BaseOnDueDate == null)
            //    throw new CustomException("Please inpute BaseOnDueDate Term");
            //if (voucherVM.IsInvoice && voucherVM.EmployeeId == null && voucherVM.BaseNoOfDays == 0 || voucherVM.IsInvoice && voucherVM.BaseNoOfDays < 0)
            //    throw new CustomException("Please inpute BaseNoOfDays Term");

            if (changeInInventoryList != null)
            {
                foreach (var item in changeInInventoryList)
                {
                    
                        if (item.GLGeneralInfoId == null)
                            throw new CustomException("AUC GL is Not Mapped !");
                        if (item.BudgetMasterId == null)
                            throw new CustomException("AUC Budget is Not Mapped !");
                        if (item.ActivityId == null)
                            throw new CustomException(" AUC Activity is Not Mapped!");
                }

                
                if (changeInInventoryList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != changeInInventoryList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                    throw new CustomException("Dr Cr Amount not equal");
            }
            if (inventoryJobWorkWIPList != null)
            {
                foreach (var item in inventoryJobWorkWIPList)
                {

                    if (item.GLGeneralInfoId == null)
                        throw new CustomException("AUC GL is Not Mapped !");
                    if (item.BudgetMasterId == null)
                        throw new CustomException("AUC Budget is Not Mapped !");
                    if (item.ActivityId == null)
                        throw new CustomException(" AUC Activity is Not Mapped!");
                }

                if (inventoryJobWorkWIPList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != inventoryJobWorkWIPList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                    throw new CustomException("Dr Cr Amount not equal");
            }
            if (inventoryJobWorkGIRIList != null)
            {
                foreach (var item in inventoryJobWorkGIRIList)
                {

                    if (item.GLGeneralInfoId == null)
                        throw new CustomException("AUC GL is Not Mapped !");
                    if (item.BudgetMasterId == null)
                        throw new CustomException("AUC Budget is Not Mapped !");
                    if (item.ActivityId == null)
                        throw new CustomException(" AUC Activity is Not Mapped!");
                }

                if (inventoryJobWorkGIRIList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) != inventoryJobWorkGIRIList.Where(a => a.TrnType == "Cr").Sum(r => r.Amount))
                    throw new CustomException("Dr Cr Amount not equal");
            }
            else
                throw new CustomException("No Journal");

                return Json(new
                {
                    Message = string.Format(AplosMessage.VoucherSave, _inventoryPayableService.InventoryOSReceivedPost(voucherVM, inventoryJobWorkWIPList, inventoryReceiveDetailVMList, inventoryPayableVMList, changeInInventoryList
                        , inventoryJobWorkGIRIList, ServiceVM))
                });


        }
        #endregion
    }
}