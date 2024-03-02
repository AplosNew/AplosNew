using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Finances;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Service.Currencies;
using Library.Service.Enums;
using Library.Service.Finances;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.ViewModel.Accounts;
using Library.ViewModel.Banks;
using Library.ViewModel.Invoices;
using Library.ViewModel.Vouchers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class LoanController : BaseController
    {
        private readonly ILoanService _loanService;
        private readonly ILoanReportService _loanReportService;
        private readonly IFinancingService _financingService;
        private readonly ISqlRepository _sqlRepository;
        public LoanController(
            ILoanService loanService
            , ILoanReportService loanReportService
            , IFinancingService financingService
            , ISqlRepository sqlRepository
            )
        {
            _loanService = loanService;
            _loanReportService = loanReportService;
            _financingService = financingService;
            _sqlRepository = sqlRepository;
        }

        #region Loan

        public ActionResult Loan()
        {
            return View("~/Areas/Accounts/Views/Loan/Loan.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetLoanList(GridParameter parameters)
        {
            AccountsLoanService _accountsLoanService = new AccountsLoanService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsLoanService.LoanQuery(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.Loan), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetLoan(string id)
        {
            AccountsLoanService _accountsLoanService = new AccountsLoanService(_sqlRepository);
            return Json(_accountsLoanService.GetLoanById(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertLoan(VoucherViewModel voucherVM, IEnumerable<VoucherViewModel> existingLoanList, IEnumerable<FinancingScheduleViewModel> loanRepaymentSchedulelist, IEnumerable<FinancingMasterOrderViewModel> financingMasterOrderlist, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.Loan.ToString();
            if (voucherVM.CurrencyId == null)
                throw new CustomException("Please Select Currency !");
            if (voucherVM.Amount < 0 || voucherVM.Amount == 0)
                throw new CustomException("Please Input Amount !");
            if (voucherVM.CompanyCurrencyRate < 0 || voucherVM.CompanyCurrencyRate == 0)
                throw new CustomException("Rate can not Empty!");
            //if (voucherVM.IsLoanSetOff == true && voucherVM.LoanSetOffAmount==0|| voucherVM.LoanSetOffAmount < 0)
            //    throw new CustomException("Please Input Loan SetOff Amount!");
            if (voucherVM.IsLoanSetOff == true && voucherVM.LoanSetOffAmount > voucherVM.Balance)
                throw new CustomException("Loan SetOff Amount can't more than Existing Loan Balance Amount!");
            if (voucherVM.TransactionType == null)
                throw new CustomException("Please Select Loan Type !");
            if (voucherVM.PartyType == PartyType.Bank.ToString() && voucherVM.OtherBankMasterId == null)
                throw new CustomException("Please Select Bank !");
            if (voucherVM.PartyType == PartyType.Customer.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Customer!");
            if (voucherVM.PartyType == PartyType.Vendor.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Vendor!");
            if (voucherVM.PartyType == PartyType.Director.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Director!");
            if (voucherVM.IsSchedule)
            {
                if (voucherVM.RepaymentStartDate == null)
                    throw new CustomException("Please Input  Repayment Date!");
                if (voucherVM.ProfitRate == 0)
                    throw new CustomException("Please Input  Profit Rate!");
                if (voucherVM.LifeOfYear == 0)
                    throw new CustomException("Please Input  Life Of Year!");
                if (voucherVM.NoOfInstallmentPerYear == 0)
                    throw new CustomException("Please Input  No Of Installment!");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _loanService.InsertLoan(voucherVM, existingLoanList, loanRepaymentSchedulelist, financingMasterOrderlist, bankChargeDetailVMList)) });
        }

        [HttpPost]
        public JsonResult UpdateLoan(VoucherViewModel voucherVM, IEnumerable<VoucherViewModel> existingLoanList, IEnumerable<FinancingScheduleViewModel> loanRepaymentSchedulelist, IEnumerable<FinancingMasterOrderViewModel> financingMasterOrderlist, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.Loan.ToString();
            if (voucherVM.CurrencyId == null)
                throw new CustomException("Please Select Currency !");
            if (voucherVM.Amount < 0 || voucherVM.Amount == 0)
                throw new CustomException("Please Select Currency !");
            if (voucherVM.CompanyCurrencyRate > 0 || voucherVM.CompanyCurrencyRate == 0)
                throw new CustomException("Rate can not Empty!");
            if (voucherVM.TransactionType == null)
                throw new CustomException("Please Select Loan Type !");
            if (voucherVM.PartyType == PartyType.Customer.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Customer!");
            if (voucherVM.PartyType == PartyType.Vendor.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Vendor!");
            if (voucherVM.PartyType == PartyType.Director.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Director!");
            if (voucherVM.IsSchedule)
            {
                if (voucherVM.RepaymentStartDate == null)
                    throw new CustomException("Please Input  Repayment Date!");
                if (voucherVM.ProfitRate == 0)
                    throw new CustomException("Please Input  Profit Rate!");
                if (voucherVM.LifeOfYear == 0)
                    throw new CustomException("Please Input  Life Of Year!");
                if (voucherVM.NoOfInstallmentPerYear == 0)
                    throw new CustomException("Please Input  No Of Installment!");
            }
            _loanService.InsertLoan(voucherVM, existingLoanList, loanRepaymentSchedulelist, financingMasterOrderlist, bankChargeDetailVMList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult PostLoan(string financingId)
        {
            _financingService.Post(financingId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public JsonResult DeleteLoan(string financingId, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _financingService.DeleteLoan(identity.CompanyId, identity.PlantId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost]
        public JsonResult DeleteAutoloanPost(string financingId, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _financingService.DeleteAutoloanPost(identity.CompanyId, identity.PlantId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult LoanReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _loanReportService.GetLoanReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantName, identity.PlantId, voucherId, SourceType.Loan.ToString());
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


        #region Loan Payment

        public ActionResult LoanPayment()
        {
            return View("~/Areas/Accounts/Views/Loan/LoanPayment.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetLoanPopUpList(string transactionType)
        {
            AccountsLoanService _accountsLoanService = new AccountsLoanService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsLoanService.GetLoanList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, transactionType), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetLoanPopUpListML(string transactionType, string partyType, string bankId)
        {
            AccountsLoanService _accountsLoanService = new AccountsLoanService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsLoanService.GetLoanListMultipleSetoff(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, transactionType, partyType, bankId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetLoanPopUpListForSalesRealization(string transactionType)
        {
            AccountsLoanService _accountsLoanService = new AccountsLoanService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsLoanService.GetLoanListForSalesRealization(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, transactionType), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetLoanPaymentList(GridParameter parameters)
        {
            AccountsLoanService _accountsLoanService = new AccountsLoanService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsLoanService.GetLoanWriteOffList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.LoanPayment), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertLoanPayment(VoucherViewModel voucherVM, VoucherViewModel loanAdditionVM, IEnumerable<FinancingScheduleViewModel> loanRepaymentSchedulelist)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.LoanPayment.ToString();
            if (voucherVM.CurrencyId == null)
                throw new CustomException("Please Select Currency !");
            if (voucherVM.Amount < 0 || voucherVM.Amount == 0)
                throw new CustomException("Please Input Total Amount !");
            if (voucherVM.Amount > voucherVM.Balance)
                throw new CustomException("Payment Amount can't more than Loan Balance Amount");

            if (voucherVM.CompanyCurrencyRate < 0 || voucherVM.CompanyCurrencyRate == 0)
                throw new CustomException("Rate can not Empty!");
            if (voucherVM.TransactionType == null)
                throw new CustomException("Please Select Loan Type !");
            if (voucherVM.PartyType == PartyType.Bank.ToString() && voucherVM.OtherBankMasterId == null)
                throw new CustomException("Please Select Other Bank !");
            if (voucherVM.PartyType == PartyType.Customer.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Customer!");
            if (voucherVM.PartyType == PartyType.Vendor.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Vendor!");
            if (voucherVM.PartyType == PartyType.Director.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Director!");
            if (voucherVM.IsSchedule)
            {
                if (voucherVM.RepaymentStartDate == null)
                    throw new CustomException("Please Input  Repayment Date!");
                if (voucherVM.ProfitRate == 0)
                    throw new CustomException("Please Input  Profit Rate!");
                if (voucherVM.LifeOfYear == 0)
                    throw new CustomException("Please Input  Life Of Year!");
                if (voucherVM.NoOfInstallmentPerYear == 0)
                    throw new CustomException("Please Input  No Of Installment!");
            }

            if (voucherVM.PaymentSource == PaymentSource.Loan.ToString())
            {
                if (voucherVM.FinancingId == loanAdditionVM.FinancingId)
                    throw new CustomException("Please Select  Different Loan!");
                if (loanAdditionVM.Amount < 0 || loanAdditionVM.Amount == 0 || loanAdditionVM.LoanSetOffAmount < 0 || loanAdditionVM.LoanSetOffAmount == 0)
                    throw new CustomException("Please Input Books Amount !");
                return Json(new { Message = string.Format(AplosMessage.VoucherSave, _loanService.InsertLoanWriteOffLoanAddition(voucherVM, loanAdditionVM, loanRepaymentSchedulelist)) });
            }
            else
            {
                if (voucherVM.IsSplit == true)
                {
                    //Change Books Amount
                    return Json(new { Message = string.Format(AplosMessage.VoucherSave, _loanService.InsertLoanWriteOffChangeBooksAmount(voucherVM, loanRepaymentSchedulelist)) });
                }
                else
                {
                    return Json(new { Message = string.Format(AplosMessage.VoucherSave, _loanService.InsertLoanWriteOff(voucherVM, loanRepaymentSchedulelist)) });
                }

            }

        }


        [HttpPost]
        public JsonResult InsertMultiLoanPayment(VoucherViewModel voucherVM, IEnumerable<VoucherViewModel> loanRepaymentlist)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.LoanPayment.ToString();
            //if (voucherVM.CurrencyId == null)
            //    throw new CustomException("Please Select Currency !");
            //if (voucherVM.Amount < 0 || voucherVM.Amount == 0)
            //    throw new CustomException("Please Input Total Amount !");
            //if (voucherVM.Amount > voucherVM.Balance)
            //    throw new CustomException("Payment Amount can't more than Loan Balance Amount");

            //if (voucherVM.CompanyCurrencyRate < 0 || voucherVM.CompanyCurrencyRate == 0)
            //    throw new CustomException("Rate can not Empty!");
            //if (voucherVM.TransactionType == null)
            //    throw new CustomException("Please Select Loan Type !");
            //if (voucherVM.PartyType == PartyType.Bank.ToString() && voucherVM.OtherBankMasterId == null)
            //    throw new CustomException("Please Select Other Bank !");
            //if (voucherVM.PartyType == PartyType.Customer.ToString() && voucherVM.PartyId == null)
            //    throw new CustomException("Please Select Customer!");
            //if (voucherVM.PartyType == PartyType.Vendor.ToString() && voucherVM.PartyId == null)
            //    throw new CustomException("Please Select Vendor!");
            //if (voucherVM.PartyType == PartyType.Director.ToString() && voucherVM.PartyId == null)
            //    throw new CustomException("Please Select Director!");
            //if (voucherVM.IsSchedule)
            //{
            //    if (voucherVM.RepaymentStartDate == null)
            //        throw new CustomException("Please Input  Repayment Date!");
            //    if (voucherVM.ProfitRate == 0)
            //        throw new CustomException("Please Input  Profit Rate!");
            //    if (voucherVM.LifeOfYear == 0)
            //        throw new CustomException("Please Input  Life Of Year!");
            //    if (voucherVM.NoOfInstallmentPerYear == 0)
            //        throw new CustomException("Please Input  No Of Installment!");
            //}

            return Json(new { Message = string.Format(AplosMessage.Insert, _loanService.InsertMultiLoanWriteOff(voucherVM, loanRepaymentlist)) });
        }

        [HttpPost]
        public JsonResult UpdateLoanPayment(VoucherViewModel voucherVM, IEnumerable<VoucherViewModel> existingLoanList, IEnumerable<FinancingScheduleViewModel> loanRepaymentSchedulelist, IEnumerable<FinancingMasterOrderViewModel> financingMasterOrderlist, IEnumerable<BankChargeViewModel> bankChargeDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.SourceType = SourceType.Loan.ToString();
            if (voucherVM.CurrencyId == null)
                throw new CustomException("Please Select Currency !");
            if (voucherVM.Amount < 0 || voucherVM.Amount == 0)
                throw new CustomException("Please Select Currency !");
            if (voucherVM.CompanyCurrencyRate > 0 || voucherVM.CompanyCurrencyRate == 0)
                throw new CustomException("Rate can not Empty!");
            if (voucherVM.TransactionType == null)
                throw new CustomException("Please Select Loan Type !");
            if (voucherVM.PartyType == PartyType.Customer.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Customer!");
            if (voucherVM.PartyType == PartyType.Vendor.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Vendor!");
            if (voucherVM.PartyType == PartyType.Director.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Director!");
            if (voucherVM.IsSchedule)
            {
                if (voucherVM.RepaymentStartDate == null)
                    throw new CustomException("Please Input  Repayment Date!");
                if (voucherVM.ProfitRate == 0)
                    throw new CustomException("Please Input  Profit Rate!");
                if (voucherVM.LifeOfYear == 0)
                    throw new CustomException("Please Input  Life Of Year!");
                if (voucherVM.NoOfInstallmentPerYear == 0)
                    throw new CustomException("Please Input  No Of Installment!");
            }
            _loanService.InsertLoan(voucherVM, existingLoanList, loanRepaymentSchedulelist, financingMasterOrderlist, bankChargeDetailVMList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult PostLoanPayment(string voucherId)
        {
            _financingService.PostFinancingWriteOff(voucherId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public JsonResult DeleteLoanPayment(string financingId, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _financingService.DeleteLoanPayment(identity.CompanyId, identity.PlantId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult LoanPaymentReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _loanReportService.GetLoanWriteOffReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.LoanPayment.ToString());
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
        public ActionResult MultiloanPaymentReport(ReportFormat reportFormat, string loanWriteOffGroupNo)
        {
            AccountsLoanService _accLoanService = new AccountsLoanService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accLoanService.MultiloanPaymentReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, loanWriteOffGroupNo, SourceType.LoanPayment.ToString());
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

        #region Loan Interest Payable


        public ActionResult LoanInterestPayable()
        {
            return View("~/Areas/Accounts/Views/Loan/LoanInterestPayable.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetLoanInterestPayableList(GridParameter parameters)
        {
            AccountsLoanService _accountsLoanService = new AccountsLoanService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsLoanService.GetLoanInterestPayableList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.LoanInterestPayable), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertLoanInterestPayable(VoucherViewModel voucherVM, IEnumerable<FinancingScheduleViewModel> loanRepaymentSchedulelist, IEnumerable<InvoiceTaxViewModel> invoiceTaxVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            if (voucherVM.CurrencyId == null)
                throw new CustomException("Please Select Currency !");
            if (voucherVM.Amount < 0 || voucherVM.Amount == 0)
                throw new CustomException("Please Input Amount !");
            if (voucherVM.CompanyCurrencyRate < 0 || voucherVM.CompanyCurrencyRate == 0)
                throw new CustomException("Rate can not Empty!");
            if (voucherVM.TransactionType == null)
                throw new CustomException("Please Select Loan Type !");
            if (voucherVM.PartyType == PartyType.Customer.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Customer!");
            if (voucherVM.PartyType == PartyType.Vendor.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Vendor!");
            if (voucherVM.PartyType == PartyType.Director.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Director!");
            if (voucherVM.IsSchedule)
            {
                if (voucherVM.RepaymentStartDate == null)
                    throw new CustomException("Please Input  Repayment Date!");
                if (voucherVM.ProfitRate == 0)
                    throw new CustomException("Please Input  Profit Rate!");
                if (voucherVM.LifeOfYear == 0)
                    throw new CustomException("Please Input  Life Of Year!");
                if (voucherVM.NoOfInstallmentPerYear == 0)
                    throw new CustomException("Please Input  No Of Installment!");
            }

            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _loanService.InsertLoanInterestPayable(voucherVM, loanRepaymentSchedulelist, invoiceTaxVMList)) });
        }

        [HttpPost]
        public JsonResult PostLoanInterestPayable(string voucherId)
        {
            _financingService.PostLoanInterestPayable(voucherId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public JsonResult DeleteLoanInterestPayable(string loanIntPayableId, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _financingService.DeleteLoanInterestPayable(identity.CompanyId, identity.PlantId, loanIntPayableId, voucherId);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public ActionResult LoanIntersetPayableReport(ReportFormat reportFormat, string voucherId, string sourceType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _loanReportService.GetLoanInterestPayableReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, sourceType);
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
        #region Loan Interest Payable Reverse



        public ActionResult LoanInterestPayableReverse()
        {
            return View("~/Areas/Accounts/Views/Loan/LoanInterestPayableReverse.cshtml");

        }

        [HttpGet, Authorize]
        public ActionResult GetLoanInterestPayableReverseList(GridParameter parameters/*string column, string value*/)
        {
            AccountsLoanService _accountsLoanService = new AccountsLoanService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsLoanService.GetLoanInterestPayableReserveList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.LoanInterestPayableReverse), JsonRequestBehavior.AllowGet);
            //string strkey = "1=1";
            //if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
            //    strkey = column + " like '%" + value + "%'";

            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //string sql = @"select top 100 * from (SELECT * FROM " + LoanInterestPayableTableName + ") AS TEMP WHERE " + strkey + " order by sequence";

            //return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult CreateLoanInterestPayableReverse(VoucherViewModel voucherVM, IEnumerable<FinancingScheduleViewModel> loanRepaymentSchedulelist)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.LoanInterestPayableReverse.ToString();
            if (voucherVM.CurrencyId == null)
                throw new CustomException("Please Select Currency !");
            if (voucherVM.Amount < 0 || voucherVM.Amount == 0)
                throw new CustomException("Please Input Amount !");
            if (voucherVM.CompanyCurrencyRate < 0 || voucherVM.CompanyCurrencyRate == 0)
                throw new CustomException("Rate can not Empty!");
            if (voucherVM.TransactionType == null)
                throw new CustomException("Please Select Loan Type !");
            if (voucherVM.PartyType == PartyType.Customer.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Customer!");
            if (voucherVM.PartyType == PartyType.Vendor.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Vendor!");
            if (voucherVM.PartyType == PartyType.Director.ToString() && voucherVM.PartyId == null)
                throw new CustomException("Please Select Director!");
            if (voucherVM.SettlementType == LoanTransactionType.ChargesPayableReverse.ToString() && voucherVM.GLGeneralInfoId == null)
                throw new CustomException("Please Select Expenses GL!");
            if (voucherVM.IsSchedule)
            {
                if (voucherVM.RepaymentStartDate == null)
                    throw new CustomException("Please Input  Repayment Date!");
                if (voucherVM.ProfitRate == 0)
                    throw new CustomException("Please Input  Profit Rate!");
                if (voucherVM.LifeOfYear == 0)
                    throw new CustomException("Please Input  Life Of Year!");
                if (voucherVM.NoOfInstallmentPerYear == 0)
                    throw new CustomException("Please Input  No Of Installment!");
            }

            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _loanService.InsertLoanInterestPayableReverse(voucherVM, loanRepaymentSchedulelist)) });
        }
        [HttpPost]
        public ActionResult DeleteLoanInterestPayableReverse(string loanIntPayableId, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _financingService.DeleteLoanInterestPayableReverse(identity.CompanyId, identity.PlantId, loanIntPayableId, voucherId);
            return Json(new { Message = AplosMessage.Updated });

        }
        [HttpPost]
        public JsonResult PostLoanInterestPayableReverse(string voucherId)
        {
            _financingService.PostLoanInterestPayable(voucherId);
            return Json(new { Message = AplosMessage.Posted });
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

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
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
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
        [HttpGet, Authorize]
        public ActionResult LoanIntersetPayableReverseReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _loanReportService.GetLoanInterestPayableReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.LoanInterestPayableReverse.ToString());
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

        #region Loan Close
        public ActionResult LoanClose()
        {
            return View("~/Areas/Accounts/Views/Loan/LoanClose.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetLoanZeroBalanceList(string transactionType)
        {
            AccountsLoanService _accountsLoanService = new AccountsLoanService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsLoanService.GetLoanZeroBalanceList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, transactionType), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertLoanClose(IEnumerable<VoucherViewModel> existingLoanList)
        {
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _loanService.InsertLoanClose(existingLoanList)) });
        }

        [Authorize, HttpGet]
        public JsonResult GetLoanClosedList(GridParameter parameters)
        {
            AccountsLoanService _accountsLoanService = new AccountsLoanService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsLoanService.GetLoanClosedList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Loan Ledger

        public ActionResult LoanLedgerReport()
        {
            return View("~/Areas/Accounts/Views/LoanLedgerReport.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetLoanRegisterList(string transactionType)
        {
            AccountsLoanService _accountsLoanService = new AccountsLoanService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsLoanService.GetLoanRegisterDataList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, transactionType), JsonRequestBehavior.AllowGet);
        }


        //Specify loan ledger report
        [HttpGet, Authorize]
        public ActionResult GetLoanLedgerReport(ReportFormat reportFormat, TransactionType transactionType, string voucherId, string financingId)
        {
            if (financingId == null)
                throw new CustomException("Please Select Loan !");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var workbook = _loanReportService.GetLoanLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, transactionType, voucherId, financingId);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Loan Register";
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


        //All register report
        [Authorize]
        public ActionResult GetAllRegisterReportExcel(TransactionType transactionType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                ExcelEngine excelEngine = new ExcelEngine();

                IWorkbook workbook = AllLoanRegisterList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, transactionType);

                string strFileName = "All Loan Register Report.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (CustomException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }


        private IWorkbook AllLoanRegisterList(string companyGroupId, string companyId, string plantId, TransactionType transactionType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ExcelEngine excelEngine = new ExcelEngine();
            IApplication application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2013;
            IWorkbook workbook = application.Workbooks.Create(1);
            IWorksheet worksheet = workbook.Worksheets[0];
            AccountsLoanService _accountsLoanService = new AccountsLoanService(_sqlRepository);

            DataTable dtAllLoanRegisterList = _accountsLoanService.GetAllLoanRegisterReportData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, transactionType);

            if (dtAllLoanRegisterList.Rows.Count == 0)
                throw new Exception("No data found");
            worksheet.Name = "AllRegisterReport";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            worksheet[ROW, COL].Text = "Financing No";
            int colFinancingNo = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Transaction Type";
            int colTransactionType = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Standard Name";
            int colStandardName = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Loan Type";
            int colLoanType = COL;
            worksheet[ROW, COL].ColumnWidth = 14;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Source Type";
            int colSourceType = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Posting Date";
            int colPostingDate = COL;
            worksheet[ROW, COL].ColumnWidth = 13;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "VoucherNo";
            int colVoucherNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Voucher Date";
            int colVoucherDate = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "DocRefNo";
            int colDocRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "DocDate";
            int colDocDate = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Narration";
            int colNarration = COL;
            worksheet[ROW, COL].ColumnWidth = 45;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Currency";
            int colCurrencyCode = COL;
            worksheet[ROW, COL].ColumnWidth = 8;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GSTIN";
            int colGSTIN = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Loan To";
            int colParticulars = COL;
            worksheet[ROW, COL].ColumnWidth = 16;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Party Type";
            int colPartyType = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Loan From";
            int colLoanFrom = COL;
            worksheet[ROW, COL].ColumnWidth = 16;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Bank Account Type ";
            int colBankAccountType = COL;
            worksheet[ROW, COL].ColumnWidth = 16;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Bank";
            int colBank = COL;
            worksheet[ROW, COL].ColumnWidth = 16;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Bank Branch";
            int colBankBranch = COL;
            worksheet[ROW, COL].ColumnWidth = 16;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Payment Source";
            int colPaymentSource = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GL";
            int colGLBUDGETACTIVITY = COL;
            worksheet[ROW, COL].ColumnWidth = 35;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GL General Info Code";
            int colGLGeneralInfoCode = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Budget";
            int colBudget = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Activity";
            int colActivity = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Is Opening";
            int colIsOpening = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Scantion Amount";
            int colScantionAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Additional Loan Amount";
            int colAdditionalLoanAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 17;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Interest Amount";
            int colInterestAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Total Loan Amount";
            int colTotalLoanAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Written Off Amount";
            int colWrittenOffAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Remaning Balance";
            int colRemaningBalance = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Loan Close Status";
            int colIsLoanClose = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;

            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            ROW++;

            for (int i = 0; i < dtAllLoanRegisterList.Rows.Count; i++)
            {
                var glName = dtAllLoanRegisterList.Rows[i]["Budget"].ToString();
                worksheet[ROW, colFinancingNo].Text = dtAllLoanRegisterList.Rows[i]["FinancingNo"].ToString();
                worksheet[ROW, colTransactionType].Text = dtAllLoanRegisterList.Rows[i]["TransactionType"].ToString();
                worksheet[ROW, colStandardName].Text = dtAllLoanRegisterList.Rows[i]["StandardName"].ToString();
                worksheet[ROW, colLoanType].Text = dtAllLoanRegisterList.Rows[i]["LoanType"].ToString();
                worksheet[ROW, colSourceType].Text = dtAllLoanRegisterList.Rows[i]["SourceType"].ToString();
                worksheet[ROW, colPostingDate].Text = (dtAllLoanRegisterList.Rows[i]["PostingDate"].ToString());
                worksheet[ROW, colVoucherNo].Text = dtAllLoanRegisterList.Rows[i]["VoucherNo"].ToString();
                worksheet[ROW, colVoucherDate].Text = dtAllLoanRegisterList.Rows[i]["VoucherDate"].ToString();
                worksheet[ROW, colDocRefNo].Text = dtAllLoanRegisterList.Rows[i]["DocRefNo"].ToString();
                worksheet[ROW, colDocDate].Text = dtAllLoanRegisterList.Rows[i]["DocDate"].ToString();
                worksheet[ROW, colNarration].Text = dtAllLoanRegisterList.Rows[i]["Narration"].ToString();
                worksheet[ROW, colCurrencyCode].Text = dtAllLoanRegisterList.Rows[i]["CurrencyCode"].ToString();
                worksheet[ROW, colGSTIN].Text = dtAllLoanRegisterList.Rows[i]["GSTIN"].ToString();
                worksheet[ROW, colParticulars].Text = dtAllLoanRegisterList.Rows[i]["LoanTo"].ToString();
                worksheet[ROW, colPartyType].Text = dtAllLoanRegisterList.Rows[i]["PartyType"].ToString();
                worksheet[ROW, colLoanFrom].Text = dtAllLoanRegisterList.Rows[i]["LoanFrom"].ToString();
                worksheet[ROW, colBankAccountType].Text = dtAllLoanRegisterList.Rows[i]["FromBankAccountType"].ToString();
                worksheet[ROW, colBank].Text = dtAllLoanRegisterList.Rows[i]["FromBankName"].ToString();
                worksheet[ROW, colBankBranch].Text = dtAllLoanRegisterList.Rows[i]["FromBankBranch"].ToString();
                worksheet[ROW, colPaymentSource].Text = dtAllLoanRegisterList.Rows[i]["PaymentSource"].ToString();
                worksheet[ROW, colGLBUDGETACTIVITY].Text = dtAllLoanRegisterList.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dtAllLoanRegisterList.Rows[i]["Activity"];
                worksheet[ROW, colGLGeneralInfoCode].Text = dtAllLoanRegisterList.Rows[i]["GLGeneralInfoCode"].ToString();
                worksheet[ROW, colBudget].Text = dtAllLoanRegisterList.Rows[i]["Budget"].ToString();
                worksheet[ROW, colActivity].Text = dtAllLoanRegisterList.Rows[i]["Activity"].ToString();
                worksheet[ROW, colIsLoanClose].Text = dtAllLoanRegisterList.Rows[i]["IsLoanClose"].ToString();

                worksheet[ROW, colScantionAmount].Number = clsStaticInfo.dbl(dtAllLoanRegisterList.Rows[i]["ScantionAmount"].ToString());
                worksheet[ROW, colScantionAmount].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, colScantionAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, colAdditionalLoanAmount].Number = clsStaticInfo.dbl(dtAllLoanRegisterList.Rows[i]["AdditionalLoanAmount"].ToString());
                worksheet[ROW, colAdditionalLoanAmount].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, colAdditionalLoanAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, colInterestAmount].Number = clsStaticInfo.dbl(dtAllLoanRegisterList.Rows[i]["InterestAmount"].ToString());
                worksheet[ROW, colInterestAmount].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, colInterestAmount].NumberFormat = "#,##0.00;(#,##0.00)";


                worksheet[ROW, colTotalLoanAmount].Number = clsStaticInfo.dbl(dtAllLoanRegisterList.Rows[i]["TotalLoanAmount"].ToString());
                worksheet[ROW, colTotalLoanAmount].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, colTotalLoanAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, colWrittenOffAmount].Number = clsStaticInfo.dbl(dtAllLoanRegisterList.Rows[i]["WrittenOffAmount"].ToString());
                worksheet[ROW, colWrittenOffAmount].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, colWrittenOffAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, colRemaningBalance].Number = clsStaticInfo.dbl(dtAllLoanRegisterList.Rows[i]["RemaningBalance"].ToString());
                worksheet[ROW, colRemaningBalance].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, colRemaningBalance].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, colIsOpening].Text = dtAllLoanRegisterList.Rows[i]["IsOpening"].ToString();

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;
            }
            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;

            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref worksheet, endCol, "All '" + transactionType + "' Register", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.Range[1, 1, 3, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes
            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes
            return workbook;
        }
        [Authorize]
        public ActionResult GetAllRegisterSummaryReportExcel(TransactionType transactionType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                ExcelEngine excelEngine = new ExcelEngine();

                IWorkbook workbook = AllLoanRegisterSummaryList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, transactionType);

                string strFileName = "All Loan Register Summary Report.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (CustomException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }


        private IWorkbook AllLoanRegisterSummaryList(string companyGroupId, string companyId, string plantId, TransactionType transactionType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ExcelEngine excelEngine = new ExcelEngine();
            IApplication application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2013;
            IWorkbook workbook = application.Workbooks.Create(1);
            IWorksheet worksheet = workbook.Worksheets[0];
            AccountsLoanService _accountsLoanService = new AccountsLoanService(_sqlRepository);

            DataTable dtAllLoanRegisterList = _accountsLoanService.GetAllLoanRegisterSummaryReportData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, transactionType);

            if (dtAllLoanRegisterList.Rows.Count == 0)
                throw new Exception("No data found");
            worksheet.Name = "AllRegisterSummaryReport";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            worksheet[ROW, COL].Text = "Bank";
            int colBank = COL;
            worksheet[ROW, COL].ColumnWidth = 16;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Budget";
            int colBudget = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Loan Amount";
            int colAdditionalLoanAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 17;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Interest Amount";
            int colInterestAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Total Loan Amount";
            int colTotalLoanAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Written Off Amount";
            int colWrittenOffAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Remaning Balance";
            int colRemaningBalance = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            

            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            ROW++;

            for (int i = 0; i < dtAllLoanRegisterList.Rows.Count; i++)
            {
                worksheet[ROW, colBank].Text = dtAllLoanRegisterList.Rows[i]["FromBankName"].ToString();
                worksheet[ROW, colBudget].Text = dtAllLoanRegisterList.Rows[i]["Budget"].ToString();

                worksheet[ROW, colAdditionalLoanAmount].Number = clsStaticInfo.dbl(dtAllLoanRegisterList.Rows[i]["LoanAmount"].ToString());
                worksheet[ROW, colAdditionalLoanAmount].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, colAdditionalLoanAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, colInterestAmount].Number = clsStaticInfo.dbl(dtAllLoanRegisterList.Rows[i]["InterestAmount"].ToString());
                worksheet[ROW, colInterestAmount].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, colInterestAmount].NumberFormat = "#,##0.00;(#,##0.00)";


                worksheet[ROW, colTotalLoanAmount].Number = clsStaticInfo.dbl(dtAllLoanRegisterList.Rows[i]["TotalLoanAmount"].ToString());
                worksheet[ROW, colTotalLoanAmount].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, colTotalLoanAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, colWrittenOffAmount].Number = clsStaticInfo.dbl(dtAllLoanRegisterList.Rows[i]["WrittenOffAmount"].ToString());
                worksheet[ROW, colWrittenOffAmount].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, colWrittenOffAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, colRemaningBalance].Number = clsStaticInfo.dbl(dtAllLoanRegisterList.Rows[i]["RemaningBalance"].ToString());
                worksheet[ROW, colRemaningBalance].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, colRemaningBalance].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;
            }
            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;

            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref worksheet, endCol, "All '" + transactionType + "' Register Summary", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.Range[1, 1, 3, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes
            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes
            return workbook;
        }

        [HttpGet, Authorize]
        public ActionResult GetLoanRegisterLedgerReport(ReportFormat reportFormat, TransactionType transactionType, string voucherId, string financingId)
        {
            if (financingId == null)
                throw new CustomException("Please Select Interest !");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var workbook = _loanReportService.GetLoanRegisterLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, transactionType, voucherId, financingId);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Loan Register";
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
        #region AutoLoan
        [HttpGet, Authorize]
        public ActionResult AutoLoanReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _loanReportService.GetLoanReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantName, identity.PlantId, voucherId, SourceType.AutoLoan.ToString());
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
        [HttpGet, Authorize]
        public JsonResult getloanBankListcbo()
        {
            AccountsLoanService _accountsLoanService = new AccountsLoanService(_sqlRepository);
            return Json(_accountsLoanService.GetloanBankListcbo(), JsonRequestBehavior.AllowGet);
        }
    }
}