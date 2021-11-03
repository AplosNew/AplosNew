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
    public class CashOpeningBalanceController : BaseController
    {
        private readonly IOpeningBalanceService _openingBalanceService;

        public CashOpeningBalanceController(IOpeningBalanceService openingBalanceService)
        {
            _openingBalanceService = openingBalanceService;
        }

        [HttpGet, Authorize]
        public ActionResult CashOpeningBalance()
        {
            return View("~/Areas/Banks/Views/CashOpeningBalance.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult Cash()
        {
            return View();
        }

        [HttpPost]
        public JsonResult InsertCash(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.PartyType = PartyType.Cash.ToString();
            openingBalance.SourceType = SourceType.CashJournal.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                openingBalanceDetailVM.PartyType = PartyType.Cash.ToString();
                if (string.IsNullOrEmpty(openingBalanceDetailVM.CashMasterId))
                    throw new CustomException($"({openingBalanceDetailVM.CashName}) Id is null!");
                else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($"Cash ({openingBalanceDetailVM.CashName}) Doc Date is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($"Cash ({openingBalanceDetailVM.CashName}) Doc Ref is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($"Cash ({openingBalanceDetailVM.CashName}) Narration is null!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Cash ({openingBalanceDetailVM.CashName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Cash ({openingBalanceDetailVM.CashName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Cash ({openingBalanceDetailVM.CashName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
            }
            _openingBalanceService.Insert(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [Authorize, HttpGet]
        public JsonResult GetCashList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.CashJournal.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateCash(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.PartyType = PartyType.Cash.ToString();
            openingBalance.SourceType = SourceType.CashJournal.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                openingBalanceDetailVM.PartyType = PartyType.Cash.ToString();
                if (string.IsNullOrEmpty(openingBalanceDetailVM.CashMasterId))
                    throw new CustomException($"({openingBalanceDetailVM.CashName}) Id is null!");
                else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($"Cash ({openingBalanceDetailVM.CashName}) Doc Date is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($"Cash ({openingBalanceDetailVM.CashName}) Doc Ref is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($"Cash ({openingBalanceDetailVM.CashName}) Narration is null!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Cash ({openingBalanceDetailVM.CashName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Cash ({openingBalanceDetailVM.CashName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Cash ({openingBalanceDetailVM.CashName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
            }
            _openingBalanceService.Update(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Updated });
        }
    }
}