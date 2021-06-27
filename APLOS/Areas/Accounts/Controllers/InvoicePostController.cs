using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Model.Enums;
using Library.Service.Invoices;
using Library.Service.Reports;
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



        [HttpPost]
        public JsonResult GRNPost(string receiveId, string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
         , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList
         , IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
         , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList
         , IEnumerable<InvoiceTaxViewModel> tdsTaxList)
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
                        , inventoryPayableVMList, inventoryReceiveDetailVMList, tdsTaxList))
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



        #endregion

        #region Service Payable



        [HttpPost]
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





        #endregion

        #region Issue Journal



        [HttpPost]
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


        #endregion
        #region Shortage




        [HttpPost]
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




        [HttpPost]
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

        [HttpPost]
        public JsonResult PostInventorySales(string receiveId, string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
   , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
   , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList, IEnumerable<VoucherDetailViewModel> inventoryJVList, bool IsInventorySalesBook)
        {
            if (IsInventorySalesBook)
            {
                ;
                return Json(InventorySalesMultipleJournalPosting(receiveId, acceptanceId, voucherVM, voucherDetailVMList
                        , voucherDetailCurrencyVMList, inventoryPayableVMList, inventoryReceiveDetailVMList, inventoryJVList));
            }
            else
            {
                return Json(InventorySalesSingleJournalPosting(receiveId, acceptanceId, voucherVM, voucherDetailVMList
                       , voucherDetailCurrencyVMList, inventoryPayableVMList, inventoryReceiveDetailVMList));
            }

        }

        [HttpPost]
        public JsonResult InventorySalesSingleJournalPosting(string receiveId, string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
         , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList
         , IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
         , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList)
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

            _inventoryPayableService.PostSingleJournalSales(receiveId, acceptanceId, voucherVM, voucherDetailVMList, voucherDetailCurrencyVMList, inventoryPayableVMList, inventoryReceiveDetailVMList);

            return Json(new { Message = AplosMessage.Insert });
        }
        [HttpPost]
        public JsonResult InventorySalesMultipleJournalPosting(string receiveId, string acceptanceId, VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList
        , IEnumerable<VoucherDetailCurrencyViewModel> voucherDetailCurrencyVMList, IEnumerable<VoucherDetailViewModel> inventoryPayableVMList
        , IEnumerable<VoucherDetailViewModel> inventoryReceiveDetailVMList, IEnumerable<VoucherDetailViewModel> inventoryJVList)
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

            _inventoryPayableService.PostMultipleJournalSales(receiveId, acceptanceId, voucherVM, voucherDetailVMList, voucherDetailCurrencyVMList, inventoryPayableVMList, inventoryReceiveDetailVMList, inventoryJVList);

            return Json(new { Message = AplosMessage.Insert });
        }



        [HttpGet, Authorize]
        public ActionResult ReceivableJournal(ReportFormat reportFormat, string inventoryReceiveId, string employeeId, bool isReversCharge, bool isFoc)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
            var reportFileName = "GRN";
            var workbook = accountsInventoryPayableReportService.PabyableJournal(identity.CompanyId, identity.PlantId, inventoryReceiveId, employeeId, isReversCharge, isFoc, reportFileName);
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

        #region Inventory Transfer Posting

        [HttpPost]
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
    }
}