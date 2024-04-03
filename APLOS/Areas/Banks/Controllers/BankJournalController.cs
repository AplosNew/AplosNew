using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Banks;
using Library.Model.Enums;
using Library.Service.Banks;
using Library.Service.Vouchers;
using Library.ViewModel.Banks;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Banks.Controllers
{
    public class BankJournalController : BaseController
    {
        private readonly IBankJournalService _bankJournalService;
        private readonly ICommonAccountsSetOffService _commonAccountsSetOffService;
        private readonly IBankReportService _bankReportService;
        private readonly ISqlRepository _sqlRepository;
        private readonly AccountsBankService _accountsBankService;

        public BankJournalController(
            IBankJournalService bankJournalService
            , IBankReportService bankReportService
            , ICommonAccountsSetOffService commonAccountsSetOffService
            , ISqlRepository sqlRepository
            , AccountsBankService accountsBankService
            )
        {
            _bankJournalService = bankJournalService;
            _commonAccountsSetOffService = commonAccountsSetOffService;
            _bankReportService = bankReportService;
            _sqlRepository = sqlRepository;
            _accountsBankService = accountsBankService;
        }

        [HttpGet]
        public ActionResult CurrentFundPosition()
        {
            return View("~/Areas/Banks/Views/CurrentFundPosition.cshtml");
        }

        [HttpGet]
        public ActionResult BankJournal()
        {
            return View("~/Areas/Banks/Views/BankJournal.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetBankJournalList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetBankJournalList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.BankJournal), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertBankJournal(VoucherViewModel voucherVM, IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.BankJournal.ToString();
            voucherVM.IsPark = true;
            if (voucherVM.ApprovedById != null)
            {
                voucherVM.ApprovedByStatus = "ToBeApproved";
            }
            if (voucherVM.BankJournalType == BankJournalType.CashExpense.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please select GL!");
            if (voucherVM.BankJournalType == BankJournalType.BankCharge.ToString() && bankChargeDetailVMList == null)
                throw new CustomException("Please select GL!");
            if (voucherVM.BankJournalType == BankJournalType.ProfitEarn.ToString() && voucherVM.FinancingTypeId == null)
                throw new CustomException("Please select Investment Type!");

            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _bankJournalService.InsertBankJournal(voucherVM, voucherDetailVMList, bankChargeDetailVMList)) });
        }



        [HttpPost]
        public JsonResult UpdateBankJournal(VoucherViewModel voucherVM, IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            if (voucherVM.ApprovedById != null && voucherVM.ApprovedByStatus == "Approved")
                throw new CustomException("Updated is not allowed after approved !!");
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.BankJournal.ToString();
            voucherVM.IsPark = true;
           

            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _bankJournalService.UpdateBankJournal(voucherVM, voucherDetailVMList, bankChargeDetailVMList)) });
        }
        [HttpPost,Authorize]
        public JsonResult InsertExpenseToBankReconcil(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.BankJournal.ToString();
            voucherVM.IsPark = true;
            if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please select GL!");
           
            if (voucherVM.BankJournalType == BankJournalType.ProfitEarn.ToString() && voucherVM.FinancingTypeId == null)
                throw new CustomException("Please select Investment Type!");

            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _commonAccountsSetOffService.InsertExpenseToBankReconcil(voucherVM, voucherDetailVMList)) });
        }
        [HttpPost]
        public JsonResult PostBankJournal(string id)
        {
            _bankJournalService.PostBankJournal(id);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public JsonResult DeleteBankJournal(string bankJournalId,string voucherId)
        {
            _bankJournalService.DeleteBankJournal(bankJournalId, voucherId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpGet, Authorize]
        public JsonResult GetBankJournal(string id)
        {
            return Json(_bankJournalService.GetBankJournal(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankJournalDetailList(string id)
        {
            return Json(_bankJournalService.GetBankChargeList(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAdvanceBankChargeList(string bankChargeId)
        {
            return Json(_bankJournalService.GetAdvanceBankChargeList(bankChargeId), JsonRequestBehavior.AllowGet);
        }

        #region Telly

       
        public ActionResult PaymentByBank()
        {
            return View("~/Areas/Banks/Views/PaymentByBank.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetPaymentByBankList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetBankCashPaymentList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.PaymentByBank), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankCashPaymentDetailList(GridParameter parameters, string bankJournalId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetBankCashPaymentDetailList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.PaymentByBank, bankJournalId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertPaymentByBank(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.PaymentByBank.ToString();
            if (voucherVM.BankJournalType == BankJournalType.BankToBank.ToString() && voucherVM.OtherBankMasterId == null)
                throw new CustomException("Please Select To  Bank!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCash.ToString() && voucherVM.OtherCashMasterId == null)
                throw new CustomException("Please Select To  Cash!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToVendor.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Vendor!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToVendor.ToString() && voucherVM.PartyPlantId == null)
                throw new CustomException("Please Select Invoicing Vendor!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeId == null)
                throw new CustomException("Please Select Employee!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeTransactionTypeId == null)
                throw new CustomException("Please Select Transaction Type!");
            else if (voucherVM.Amount == 0 || voucherVM.Amount < 0)
                throw new CustomException("Please Input Amount!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please select GL!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not match!");
            voucherVM.IsPark = true;
            var no = _bankJournalService.InsertBankPayment(voucherVM, voucherDetailVMList);
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, no) });
        }

        [HttpPost]
        public JsonResult UpdatePaymentByBank(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.PaymentByBank.ToString();
            if (voucherVM.BankJournalType == BankJournalType.BankToBank.ToString() && voucherVM.OtherBankMasterId == null)
                throw new CustomException("Please Select To  Bank!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCash.ToString() && voucherVM.OtherCashMasterId == null)
                throw new CustomException("Please Select To  Cash!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToVendor.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Vendor!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToVendor.ToString() && voucherVM.PartyPlantId == null)
                throw new CustomException("Please Select Invoicing Vendor!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeId == null)
                throw new CustomException("Please Select Employee!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeTransactionTypeId == null)
                throw new CustomException("Please Select Transaction Type!");
            else if (voucherVM.Amount == 0 || voucherVM.Amount < 0)
                throw new CustomException("Please Input Amount!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please select GL!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not match!");
            voucherVM.IsPark = true;
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _bankJournalService.UpdateBankPayment(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult PostPaymentByBank(string id)
        {
            _bankJournalService.PostBankJournal(id);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpGet]
        public ActionResult ReceiptByBank()
        {
            return View("~/Areas/Banks/Views/ReceiptByBank.cshtml");
        }

      
        [HttpGet, Authorize]
        public JsonResult GetReceiptByBankList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetBankCashPaymentList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.ReceiptByBank), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankCashReceiptDetailList(GridParameter parameters, string bankJournalId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetBankCashPaymentDetailList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.ReceiptByBank, bankJournalId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertReceiptByBank(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.ReceiptByBank.ToString();
            if (voucherVM.BankJournalType == BankJournalType.BankToBank.ToString() && voucherVM.OtherBankMasterId == null)
                throw new CustomException("Please Select To Bank!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCash.ToString() && voucherVM.OtherCashMasterId == null)
                throw new CustomException("Please Select To Cash!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCustomer.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Customer!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCustomer.ToString() && voucherVM.PartyPlantId == null)
                throw new CustomException("Please Select Invoicing Customer!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeId == null)
                throw new CustomException("Please Select Employee!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeTransactionTypeId == null)
                throw new CustomException("Please Select Transaction Type!");
            else if (voucherVM.Amount == 0 || voucherVM.Amount < 0)
                throw new CustomException("Please Input Amount!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please select GL!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not match!");
            voucherVM.IsPark = true;
            var no = _bankJournalService.InsertBankReceipt(voucherVM, voucherDetailVMList);
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, no) });

        }

        [HttpPost]
        public JsonResult UpdateReceiptByBank(VoucherViewModel voucherVM, IEnumerable<BankChargeViewModel> bankChargeDetailVMList, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.ReceiptByBank.ToString();
            if (voucherVM.BankJournalType == BankJournalType.BankToBank.ToString() && voucherVM.OtherBankMasterId == null)
                throw new CustomException("Please Select To Bank!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCash.ToString() && voucherVM.OtherCashMasterId == null)
                throw new CustomException("Please Select To Cash!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCustomer.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Customer!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToCustomer.ToString() && voucherVM.PartyPlantId == null)
                throw new CustomException("Please Select Invoicing Customer!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeId == null)
                throw new CustomException("Please Select Employee!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToEmployee.ToString() && voucherVM.EmployeeTransactionTypeId == null)
                throw new CustomException("Please Select Transaction Type!");
            else if (voucherVM.Amount == 0 || voucherVM.Amount < 0)
                throw new CustomException("Please Input Amount!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherDetailVMList == null)
                throw new CustomException("Please select GL!");
            else if (voucherVM.BankJournalType == BankJournalType.BankToGL.ToString() && voucherVM.Amount != voucherDetailVMList.Sum(r => r.Amount))
                throw new CustomException("Dr Cr Amount not match!");
            voucherVM.IsPark = true;
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _bankJournalService.UpdateBankReceipt(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult PostReceiptByBank(string id)
        {
            _bankJournalService.PostBankJournal(id);
            return Json(new { Message = AplosMessage.Posted });
        }

        #endregion Telly

        [Authorize, HttpGet]
        public JsonResult GetAvilabeCustomerPaymentList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_bankJournalService.GetAvilabeCustomerPaymentList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }


        //Current Fund Position start//

        [HttpGet]
        public ActionResult getCurrentFundPositionlist(DateTime PostingDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT ROW_NUMBER() OVER (ORDER BY  AccountTitle) AS SLNo,Category,AccountTitle Bank_CashName,AccountNumber,Currency,SUM(DrAmount) - SUM(CrAmount) AS Amount
                         ,LimitAmount,0 TotalAvailableAmount,'' Remark,PDCOverDue,PDCInNext_7_Days,0 PaymentOverdue,0 PaymentOverdueInNext_7_Days
						 ,0 Surplus_Short_AsOnDate,0 Short_SurplusInNext_7_Days
						FROM (
                        SELECT  'Bank' Category,BM.AccountTitle,BM.AccountNumber,CU.Code Currency,BM.LimitAmount,SUM(GLTD.DrAmount) AS DrAmount, SUM(GLTD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId
                         ,PDCOverDue=  SUM(CASE WHEN DATEDIFF(DAY, GETDATE(),PDC.PostingDate)<0 THEN PDC.Amount else 0 end) OVER (partition by VD.BankMasterId) 
                         ,PDCInNext_7_Days=  SUM(CASE WHEN DATEDIFF(DAY, GETDATE(),PDC.PostingDate)>=0 AND DATEDIFF(DAY, GETDATE(),PDC.PostingDate)<7 THEN PDC.Amount else 0 end) OVER (partition by VD.BankMasterId) 
						 
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
						LEFT JOIN MST.BankMaster BM ON BM.Id=VD.BankMasterId
						LEFT JOIN TRN.PostDepositCheque PDC ON PDC.BankMasterId=BM.Id
						LEFT JOIN SCS.Currency CU ON CU.Id=BM.CurrencyId
                        LEFT JOIN [TRN].[GLTransactionDetail] AS GLTD ON GLTD.VoucherDetailId=VD.Id AND GLTD.BankMasterId=VD.BankMasterId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + identity.CompanyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"'
						AND V.PostingDate <= '" + PostingDate + @"' and VD.BankMasterId<>''
                        GROUP BY CC.CompanyCurrencyId ,BM.AccountTitle,BM.AccountNumber,CU.Code,BM.LimitAmount,PDC.PostingDate,VD.BankMasterId,PDC.Amount

						UNION
						SELECT 'Cash' Category,CM.UserName AccountTitle, '' AccountNumber,CU.Code Currency,0 LimitAmount,SUM(GLTD.DrAmount) AS DrAmount, SUM(GLTD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId
                         ,0 PDCOverDue,0PDCInNext_7_Days
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
						LEFT JOIN MST.CashMaster CM ON CM.Id=VD.CashMasterId
						LEFT JOIN SCS.Currency CU ON CU.Id=CM.CurrencyId
                        LEFT JOIN [TRN].[GLTransactionDetail] AS GLTD ON GLTD.VoucherDetailId=VD.Id AND GLTD.CashMasterId=VD.CashMasterId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + identity.CompanyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' 
						AND V.PostingDate <= '" + PostingDate + @"' and VD.CashMasterId<>''
                        GROUP BY CC.CompanyCurrencyId ,CM.UserName,CU.Code
						  ) AS X GROUP BY X.CompanyCurrencyId,X.AccountTitle,x.Currency,x.LimitAmount,x.Category,x.AccountNumber,x.PDCOverDue,X.PDCInNext_7_Days";

            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetCurrentFundPositionReport(DateTime PostingDate)
        {
            try
            {
                AccountsBankService accountsBankService = new AccountsBankService(_sqlRepository);
                string fileName = "";
                fileName = _accountsBankService.CurrentFundPositionReport(PostingDate, "Post Date Cheque Report");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //Current Fund Position end//
    }
}