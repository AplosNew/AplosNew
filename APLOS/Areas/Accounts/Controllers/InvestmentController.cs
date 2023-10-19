using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Service.Finances;
using Library.ViewModel.Vouchers;
using Syncfusion.XlsIO;
using System;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class InvestmentController : BaseController
    {
        private readonly IInvestmentService _investmentService;
        private readonly IInvestmentReportService _investmentReportService;
        private readonly IFinancingService _financingService;
        private readonly ISqlRepository _sqlRepository;

        public InvestmentController(
            IInvestmentService investmentService
            , IInvestmentReportService investmentReportService
            , IFinancingService financingService, ISqlRepository sqlRepository
            )
        {
            _investmentService = investmentService;
            _investmentReportService = investmentReportService;
            _financingService = financingService;
            _sqlRepository = sqlRepository;
        }


        public ActionResult Investment()
        {
            return View("~/Areas/Accounts/Views/Investment.cshtml");
        }

        public ActionResult InvestmentSettelment()
        {
            return View("~/Areas/Accounts/Views/InvestmentSettelment.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetInvestmentList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_investmentService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.Investment), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInvestment(string id)
        {
            return Json(_investmentService.GetById(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertInvestment(VoucherViewModel voucherVM)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.Investment.ToString();
            if (voucherVM.CurrencyId == null)
                throw new CustomException("Please Select Currency !");
            if (voucherVM.Amount < 0 || voucherVM.Amount == 0)
                throw new CustomException("Please Input Amount !");
            if (voucherVM.CompanyCurrencyRate < 0 || voucherVM.CompanyCurrencyRate == 0)
                throw new CustomException("Rate can not Empty!");
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
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _investmentService.InsertInvestment(voucherVM)) });
        }

        [HttpPost]
        public JsonResult UpdateInvestment()
        {
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult PostInvestment(string financingId)
        {
            _financingService.Post(financingId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public JsonResult DeleteInvestment(string financingId, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _financingService.DeleteInvestment(identity.CompanyId, identity.PlantId, voucherId);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpGet, Authorize]
        public ActionResult InvestmentReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _investmentReportService.GetInvestmentReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantName, identity.PlantId, voucherId, SourceType.Investment.ToString());
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

        [Authorize, HttpGet]
        public JsonResult GetInvestmentPopUpList(string transactionType)
        {
            AccountsLoanService _accountsLoanService = new AccountsLoanService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsLoanService.GetInvestmentList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, transactionType), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertInvestmentSetoff(VoucherViewModel voucherVM, VoucherViewModel loanAdditionVM)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = "InvestmentSetOff";//SourceType.LoanPayment.ToString();
            if (voucherVM.CurrencyId == null)
                throw new CustomException("Please Select Currency !");
            if (voucherVM.Amount < 0 || voucherVM.Amount == 0)
                throw new CustomException("Please Input Total Amount !");
            if (voucherVM.Amount > voucherVM.Balance)
                throw new CustomException("Receive Amount can't more than Investment Balance Amount");

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
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _investmentService.InsertInvestmentSetOff(voucherVM)) });
        }
        [Authorize, HttpGet]
        public JsonResult GetInvestmentSetoffList(GridParameter parameters)
        {
            AccountsLoanService _accountsLoanService = new AccountsLoanService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsLoanService.GetInvestmentSetoffList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SourceType.LoanPayment), JsonRequestBehavior.AllowGet);
        }
    }
}