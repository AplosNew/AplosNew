using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Enums;
using Library.Model.OpeningBalances;
using Library.Model.Parties;
using Library.Service.OpeningBalances;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Banks.Controllers
{
    public class BankOpeningBalanceController : BaseController
    {
        private readonly IOpeningBalanceService _openingBalanceService;

        public BankOpeningBalanceController(IOpeningBalanceService openingBalanceService)
        {
            _openingBalanceService = openingBalanceService;
        }

        [HttpGet, Authorize]
        public ActionResult BankOpeningBalance()
        {
            return View("~/Areas/Banks/Views/BankOpeningBalance.cshtml");
        }

        [HttpPost]
        public JsonResult InsertBank(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.PartyType = PartyType.Bank.ToString();
            openingBalance.SourceType = SourceType.BankJournal.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                openingBalanceDetailVM.PartyType = PartyType.Bank.ToString();
                if (string.IsNullOrEmpty(openingBalanceDetailVM.BankMasterId))
                    throw new CustomException($"({openingBalanceDetailVM.BankName}) Id is null!");
                else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Doc Date is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Doc Ref is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Narration is null!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
            }
            _openingBalanceService.Insert(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [Authorize, HttpGet]
        public JsonResult GetBankList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.BankJournal.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateBank(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.PartyType = PartyType.Bank.ToString();
            openingBalance.SourceType = SourceType.BankJournal.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (string.IsNullOrEmpty(openingBalanceDetailVM.BankMasterId))
                    throw new CustomException($"({openingBalanceDetailVM.BankName}) Id is null!");
                else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Doc Date is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Doc Ref is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Narration is null!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
            }
            _openingBalanceService.Update(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Updated });
        }
    }
}