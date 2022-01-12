using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.OpeningBalances;
using Library.Model.Parties;
using Library.Model.Vouchers;
using Library.Security.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.OpeningBalances;
using Library.Service.Vouchers;
using Library.ViewModel.Accounts;
using Library.ViewModel.Vouchers;
using Newtonsoft.Json;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Accounts.Controllers
{
	public class OpeningBalanceController : BaseController
    {
        private readonly IOpeningBalanceService _openingBalanceService;
        private readonly IOpeningBalanceCutOffDateService _openingBalanceCutOffDateService;
        private readonly IVoucherReportService _voucharReportService;
        private readonly ISqlRepository _sqlRepository;

        public OpeningBalanceController(IOpeningBalanceService openingBalanceService
            , IOpeningBalanceCutOffDateService openingBalanceCutOffDateService
            , IVoucherReportService voucharReportService, ISqlRepository sqlRepository)
        {
            _openingBalanceService = openingBalanceService;
            _openingBalanceCutOffDateService = openingBalanceCutOffDateService;
            _voucharReportService = voucharReportService;
            _sqlRepository = sqlRepository;
        }

        [Authorize, HttpGet]
        public JsonResult GetOpeningBalanceDetailList(string openingBalanceId, string sort)
        {
            if (string.IsNullOrEmpty(sort))
                sort = "EmployeeName";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetOpeningBalanceDetailList(identity.CompanyId, openingBalanceId, sort), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetMMOpeningBalanceDetailList(string openingBalanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetMMOpeningBalanceDetailList(identity.CompanyId, openingBalanceId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetMaterialMasterOpeningBalanceDetailList(string openingBalanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetMaterialMasterOpeningBalanceDetailList(identity.CompanyId,identity.PlantId, openingBalanceId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult GetMaterialMasterOBDetailList(string openingBalanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetMaterialMasterOBDetailList(identity.CompanyId, identity.PlantId, openingBalanceId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _openingBalanceService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult DeleteInter(string id)
        {
            _openingBalanceService.DeleteInter(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        #region  Opening Balance Report
        [Authorize]
        public ActionResult OpeningBalanceRegister()
        {
            return View();
        }

       #endregion
        #region -- CutOffDate

        [HttpGet]
        public ActionResult ACCCutOffDate()
        {
            return View();
        }

        [HttpGet]
        public ActionResult HRCutOffDate()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetACCCutOffDate()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceCutOffDateService.GetACCCutOffDate(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
       

        [Authorize, HttpGet]
        public JsonResult GetHRCutOffDate(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceCutOffDateService.GetHRCutOffDate(identity.CompanyGroupId, identity.CompanyId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCutOffDateList()
        {
            return Json(_openingBalanceCutOffDateService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetACCCutOffDateList(GridParameter parameters, string companyGroupId)
        {
            return Json(_openingBalanceCutOffDateService.Query(parameters, companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetHRCutOffDateList(GridParameter parameters, string companyGroupId, string companyId)
        {
            return Json(_openingBalanceCutOffDateService.Query(parameters, companyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateACCCutOffDate(IEnumerable<OpeningBalanceCutOffDate> openingBalanceCutOffDates)
        {
            foreach (OpeningBalanceCutOffDate openingBalanceCutOffDate in openingBalanceCutOffDates)
            {
                openingBalanceCutOffDate.PlantId = null;
                openingBalanceCutOffDate.ModuleName = "ACC";
            }
            _openingBalanceCutOffDateService.Insert(openingBalanceCutOffDates);
            return Json(new { OpeningBalanceCutOffDate = openingBalanceCutOffDates, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult CreateHRCutOffDate(IEnumerable<OpeningBalanceCutOffDate> openingBalanceCutOffDates)
        {
            foreach (OpeningBalanceCutOffDate openingBalanceCutOffDate in openingBalanceCutOffDates)
            {
                openingBalanceCutOffDate.ModuleName = "HR";
            }
            _openingBalanceCutOffDateService.Insert(openingBalanceCutOffDates);
            return Json(new { OpeningBalanceCutOffDate = openingBalanceCutOffDates, Message = AplosMessage.Insert });
        }

        #endregion -- CutOffDate

        #region -- Journal

        [HttpGet, Authorize]
        public ActionResult Journal()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetAvailableForJournalList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetAvailableForJournal(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSummaryData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertJournal(Voucher voucher, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucher.CompanyGroupId = identity.CompanyGroupId;
            voucher.CompanyId = identity.CompanyId;
            voucher.PlantId = identity.PlantId;
            voucher.IsPark = false;
            _openingBalanceService.InsertJournal(voucher, voucherDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [Authorize, HttpGet]
        public JsonResult GetJournalList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetJournalList(parameters, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Journal

        #region AdvanceJournal

        [HttpGet, Authorize]
        public ActionResult AdvanceJournal()
        {
            return View("~/Areas/Accounts/Views/OpeningBalance/AdvanceJournal.cshtml");
        }

        [HttpPost]
        public JsonResult ParkOBAdvanceJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            if (voucherDetailVMList == null)
                throw new CustomException("Please Add Item.");
            

            foreach (var item in voucherDetailVMList)
            {
               
                if (item.PartyType != "Equity" && item.PartyType != "LoanGiven")
                {
                    if(item.PartyId != null && item.PartyPlantId == null)
                        throw new CustomException("Please select Location!");

                }
                if (item.PartyType == PartyType.Bank.ToString() && item.BankCurrencyId != voucherVM.CurrencyId && item.BankAmount < 0 ||
                     item.PartyType == PartyType.Bank.ToString() && item.BankCurrencyId != voucherVM.CurrencyId && item.BankAmount == 0)
                    throw new CustomException("Please Input Bank Currency Amount");
                if (item.PartyType == PartyType.Cash.ToString() && item.CashCurrencyId != voucherVM.CurrencyId && item.BankAmount < 0 ||
                    item.PartyType == PartyType.Cash.ToString() && item.CashCurrencyId != voucherVM.CurrencyId && item.BankAmount == 0)
                    throw new CustomException("Please Input Cash Currency Amount");
                if ((item.DrAmount + item.CrAmount == 0) || (item.DrAmount + item.CrAmount < 0))
                    throw new CustomException("Please input amount !");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _openingBalanceService.InsertAdvanceJournal(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult UpdateOBAdvanceJournal(VoucherViewModel voucherVM, string voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            List<VoucherDetailViewModel> voucherDetailVM = JsonConvert.DeserializeObject<List<VoucherDetailViewModel>>(voucherDetailVMList, settings);

            foreach (var item in voucherDetailVM)
            {
                if (item.PartyType != "Equity" && item.PartyType != "LoanGiven" && item.PartyType != "LoanTaken")
                {
                    if (item.PartyId != null && item.PartyPlantId == null)
                        throw new CustomException("Please select Location!");

                }
                if (item.PartyType == PartyType.Bank.ToString() && item.BankCurrencyId != voucherVM.CurrencyId && item.BankAmount < 0 || 
                    item.PartyType == PartyType.Bank.ToString() && item.BankCurrencyId != voucherVM.CurrencyId && item.BankAmount == 0)
                    throw new CustomException("Please Input Bank Currency Amount");
                if (item.PartyType == PartyType.Cash.ToString() && item.CashCurrencyId != voucherVM.CurrencyId && item.BankAmount < 0 || 
                    item.PartyType == PartyType.Cash.ToString() && item.CashCurrencyId != voucherVM.CurrencyId && item.BankAmount == 0)
                    throw new CustomException("Please Input Cash Currency Amount");
                if ((item.DrAmount + item.CrAmount == 0) || (item.DrAmount + item.CrAmount < 0))
                    throw new CustomException("Please input amount !");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _openingBalanceService.UpdateAdvanceJournal(voucherVM, voucherDetailVM)) });
        }

        [HttpPost]
        public JsonResult ParkOBGLAdvanceJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            if (voucherDetailVMList == null)
                throw new CustomException("Please Add Item.");


            foreach (var item in voucherDetailVMList)
            {

                if (item.PartyType != "Equity" && item.PartyType != "LoanGiven")
                {
                    if (item.PartyId != null && item.PartyPlantId == null)
                        throw new CustomException("Please select Location!");

                }
                if (item.PartyType == PartyType.Bank.ToString() && item.BankCurrencyId != voucherVM.CurrencyId && item.BankAmount < 0 ||
                     item.PartyType == PartyType.Bank.ToString() && item.BankCurrencyId != voucherVM.CurrencyId && item.BankAmount == 0)
                    throw new CustomException("Please Input Bank Currency Amount");
                if (item.PartyType == PartyType.Cash.ToString() && item.CashCurrencyId != voucherVM.CurrencyId && item.BankAmount < 0 ||
                    item.PartyType == PartyType.Cash.ToString() && item.CashCurrencyId != voucherVM.CurrencyId && item.BankAmount == 0)
                    throw new CustomException("Please Input Cash Currency Amount");
                if ((item.DrAmount + item.CrAmount == 0) || (item.DrAmount + item.CrAmount < 0))
                    throw new CustomException("Please input amount !");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _openingBalanceService.InsertGLAdvanceJournal(voucherVM, voucherDetailVMList)) });
        }


        [HttpGet]
        public JsonResult GetOBAdvanceJournalList(GridParameter parameters)
        {
            AccountsOpeningBalanceService _accountsOpeningBalanceService = new AccountsOpeningBalanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsOpeningBalanceService.GetAdvanceJournalList(parameters, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetOBAdvanceJournalDetail(GridParameter parameters, string openingBalanceId)
        {
            AccountsOpeningBalanceService _accountsOpeningBalanceService = new AccountsOpeningBalanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_accountsOpeningBalanceService.GetOBAdvanceJournalDetail(parameters,identity.CompanyGroupId, identity.CompanyId, identity.PlantId, openingBalanceId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [HttpPost]
        public JsonResult PostOBAdvanceJournal(VoucherViewModel voucherVM, string voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            List<VoucherDetailViewModel> voucherDetailVM = JsonConvert.DeserializeObject<List<VoucherDetailViewModel>>(voucherDetailVMList, settings);


            if (!voucherVM.IsPark)
                throw new CustomException("Update or Delete is not allowed.");
            if (voucherDetailVM.Sum(r => r.DrAmount) != voucherDetailVM.Sum(r => r.CrAmount))
                throw new CustomException("Dr Cr not match!");
            voucherVM.IsPosted = true;
            voucherVM.IsPark = false;
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _openingBalanceService.PostInsertAdvanceJournal(voucherVM, voucherDetailVM)) });
        }

        [HttpPost]
        public JsonResult PostOpeningBalanceJournal(VoucherViewModel voucherVM,decimal DrAmount, decimal CrAmount)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            
            if (!voucherVM.IsPark)
                throw new CustomException("Update or Delete is not allowed.");
            if (DrAmount != CrAmount)
                throw new CustomException("Dr Cr not match!");
            voucherVM.IsPosted = true;
            voucherVM.IsPark = false;
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _openingBalanceService.PostOpeningBalanceJournal(voucherVM)) });
        }


        [HttpPost]
        public JsonResult DeleteOBDetailRow(OpeningBalanceDetail OBDetailVM)
        {
            return Json(new { Message = string.Format(AplosMessage.Deleted, _openingBalanceService.DeleteOBDetailRow(OBDetailVM)) });
        }

        [HttpGet, Authorize]
        public ActionResult GetJournalVoucherReport(ReportFormat reportFormat, string openingBalanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _voucharReportService.GetOBAdvanceJournalVoucher(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, openingBalanceId);
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
        #endregion Journal

        #region Party

        [HttpGet, Authorize]
        public ActionResult CustomerInvoice()
        {
            return View();
        }

        [HttpPost]
        public JsonResult InsertCustomerInvoice(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.PartyType = PartyType.Customer.ToString();
            openingBalance.SourceType = SourceType.CustomerInvoice.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                    throw new CustomException($"({openingBalanceDetailVM.PartyName}) CustomerId is null!");
                if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Doc Date is null!");
                if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Narration is null!");
                if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyName} amount is not equal!");
            }
            _openingBalanceService.Insert(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [Authorize, HttpGet]
        public JsonResult GetCustomerInvoiceList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.CustomerInvoice.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateCustomerInvoice(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.PartyType = PartyType.Customer.ToString();
            openingBalance.SourceType = SourceType.CustomerInvoice.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                    throw new CustomException($"({openingBalanceDetailVM.PartyName}) Id is null!");
                if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($"Customer {openingBalanceDetailVM.PartyName}) Doc Date is null!");
                if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Narration is null!");
                if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyName} amount is not equal!");
            }
            _openingBalanceService.Update(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public ActionResult CustomerAdvance()
        {
            return View();
        }

        [HttpPost]
        public JsonResult InsertCustomerAdvance(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.PartyType = PartyType.Customer.ToString();
            openingBalance.SourceType = SourceType.CustomerAdvance.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                    throw new CustomException($"({openingBalanceDetailVM.PartyName}) Id is null!");
                if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($"Customer {openingBalanceDetailVM.PartyName}) Doc Date is null!");
                if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Narration is null!");
                if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
            }
            _openingBalanceService.Insert(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [Authorize, HttpGet]
        public JsonResult GetCustomerAdvanceList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.CustomerAdvance.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateCustomerAdvance(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.PartyType = PartyType.Customer.ToString();
            openingBalance.SourceType = SourceType.CustomerAdvance.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                    throw new CustomException($"({openingBalanceDetailVM.PartyName}) CustomerId is null!");
                else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($"Customer {openingBalanceDetailVM.PartyName}) Doc Date is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Narration is null!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Customer ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
            }
            _openingBalanceService.Update(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public ActionResult VendorInvoice()
        {
            return View();
        }

        [HttpPost]
        public JsonResult InsertVendorInvoice(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.PartyType = PartyType.Vendor.ToString();
            openingBalance.SourceType = SourceType.VendorInvoice.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                    throw new CustomException($"({openingBalanceDetailVM.PartyName}) Id is null!");
                else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($"Vendor {openingBalanceDetailVM.PartyName}) Doc Date is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Narration is null!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
            }
            _openingBalanceService.Insert(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [Authorize, HttpGet]
        public JsonResult GetVendorInvoiceList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.VendorInvoice.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateVendorInvoice(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.PartyType = PartyType.Vendor.ToString();
            openingBalance.SourceType = SourceType.VendorInvoice.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                    throw new CustomException($"({openingBalanceDetailVM.PartyName}) Id is null!");
                else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($"Vendor {openingBalanceDetailVM.PartyName}) Doc Date is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Narration is null!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
            }
            _openingBalanceService.Update(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public ActionResult VendorAdvance()
        {
            return View();
        }

        [HttpPost]
        public JsonResult InsertVendorAdvance(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.PartyType = PartyType.Vendor.ToString();
            openingBalance.SourceType = SourceType.VendorAdvance.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                    throw new CustomException($"({openingBalanceDetailVM.PartyName}) Id is null!");
                else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Doc Date is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Narration is null!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
            }
            _openingBalanceService.Insert(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpGet]
        public JsonResult GetVendorAdvanceList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.VendorAdvance.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateVendorAdvance(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.PartyType = PartyType.Vendor.ToString();
            openingBalance.SourceType = SourceType.VendorAdvance.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Id is null!");
                else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($"Vendor {openingBalanceDetailVM.PartyName}) Doc Date is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Narration is null!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Vendor ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
            }
            _openingBalanceService.Update(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Updated });
        }

        #endregion Party

        #region Employee

        [HttpGet, Authorize]
        public ActionResult EmployeePayable()
        {
            return View();
        }

        [HttpPost]
        public JsonResult InsertEmployeePayable(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.PartyType = PartyType.Employee.ToString();
            openingBalance.SourceType = SourceType.EmployeePayable.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                openingBalanceDetailVM.PartyType = PartyType.Employee.ToString();
                if (string.IsNullOrEmpty(openingBalanceDetailVM.EmployeeId))
                    throw new CustomException($"({openingBalanceDetailVM.EmployeeName}) Id is null!");
                else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Doc Date is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Doc Ref is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Narration is null!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
            }
            _openingBalanceService.Insert(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpGet]
        public JsonResult GetEmployeePayableList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.EmployeePayable.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateEmployeePayable(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.PartyType = PartyType.Employee.ToString();
            openingBalance.SourceType = SourceType.EmployeePayable.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                openingBalanceDetailVM.PartyType = PartyType.Employee.ToString();
                if (string.IsNullOrEmpty(openingBalanceDetailVM.EmployeeId))
                    throw new CustomException($"({openingBalanceDetailVM.EmployeeName}) Id is null!");
                else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Doc Date is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Doc Ref is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Narration is null!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
            }
            _openingBalanceService.Update(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult EmployeeAdvance()
        {
            return View();
        }

        [HttpPost]
        public JsonResult InsertEmployeeAdvance(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.PartyType = PartyType.Employee.ToString();
            openingBalance.SourceType = SourceType.EmployeeAdvance.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                openingBalanceDetailVM.PartyType = PartyType.Employee.ToString();
                if (string.IsNullOrEmpty(openingBalanceDetailVM.EmployeeId))
                    throw new CustomException($"({openingBalanceDetailVM.EmployeeName}) Id is null!");
                else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Doc Date is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Doc Ref is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Narration is null!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
            }
            _openingBalanceService.Insert(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpGet]
        public JsonResult GetEmployeeAdvanceList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.EmployeeAdvance.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateEmployeeAdvance(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.PartyType = PartyType.Employee.ToString();
            openingBalance.SourceType = SourceType.EmployeeAdvance.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                openingBalanceDetailVM.PartyType = PartyType.Employee.ToString();
                if (string.IsNullOrEmpty(openingBalanceDetailVM.EmployeeId))
                    throw new CustomException($"({openingBalanceDetailVM.EmployeeName}) Id is null!");
                else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Doc Date is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Doc Ref is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Narration is null!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Employee ({openingBalanceDetailVM.EmployeeName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
            }
            _openingBalanceService.Update(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion Employee

        #region Security

       
        public ActionResult SecurityGiven()
        {
            return View();
        }

        [HttpPost]
        public JsonResult InsertSecurityGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            if (openingBalance.PartyType == null)
                openingBalance.PartyType = PartyType.Party.ToString();
            openingBalance.SourceType = SourceType.SecurityDeposit.ToString();
            openingBalance.TransactionType = TransactionType.SecurityGiven.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (openingBalanceDetailVM.PartyType !="Bank" && string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Id is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyType))
                    throw new CustomException($"Party  Type is null!");
                else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($"  Doc Date is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($"  Doc Ref is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($" Narration is null!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
            }
            _openingBalanceService.Insert(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpGet]
        public JsonResult GetSecurityGivenList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.SecurityDeposit.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateSecurityGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            if (openingBalance.PartyType == null)
                openingBalance.PartyType = PartyType.Party.ToString();
            openingBalance.SourceType = SourceType.SecurityDeposit.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (openingBalanceDetailVM.PartyType != "Bank" && string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                    throw new CustomException($"({openingBalanceDetailVM.PartyName}) Id is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyType))
                    throw new CustomException($"Party  Type is null!");
                else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($" Doc Date is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($" Doc Ref is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($" Narration is null!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
            }
            _openingBalanceService.Update(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Updated });
        }

       
        public ActionResult SecurityTaken()
        {
            return View();
        }


        [Authorize, HttpGet]
        public JsonResult GetSecurityTakenOBList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.SecurityDeposit.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, TransactionType.SecurityTaken.ToString()), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSecurityGivenOBList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.SecurityDeposit.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, TransactionType.SecurityGiven.ToString()), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSecurityGivenOBDetail(string openingBalanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetOBSecurityGivenDetailGL(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, openingBalanceId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSecurityTakenOBDetail(string openingBalanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetOBSecurityTakenDetailGL(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, openingBalanceId), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult InsertSecurityTaken(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            if (openingBalance.PartyType == null)
                openingBalance.PartyType = PartyType.Party.ToString();
            openingBalance.SourceType = SourceType.SecurityDeposit.ToString();
            openingBalance.TransactionType = TransactionType.SecurityTaken.ToString();
            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Id is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyType))
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Type is null!");
                else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Date is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Narration is null!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId &&
                    openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
            }
            _openingBalanceService.Insert(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpGet]
        public JsonResult GetSecurityTakenList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.SecurityDeposit.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateSecurityTaken(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            if(openingBalance.PartyType==null)
            openingBalance.PartyType = PartyType.Party.ToString();
            openingBalance.SourceType = SourceType.SecurityDeposit.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                    throw new CustomException($"({openingBalanceDetailVM.PartyName}) Id is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyType))
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Type is null!");
                else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Date is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Narration is null!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                    throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
            }
            _openingBalanceService.Update(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Updated });
        }

        #endregion Security

        #region Loan

       
        public ActionResult LoanTaken()
        {
            return View();
        }

        [HttpPost]
        public JsonResult InsertLoanTaken(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.SourceType = SourceType.Loan.ToString();
            openingBalance.TransactionType = TransactionType.LoanTaken.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (!string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                {
                    if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                        throw new CustomException($"({openingBalanceDetailVM.PartyName}) Id is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyType))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Type is null!");
                    else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Date is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Narration is null!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
                }
                else if (!string.IsNullOrEmpty(openingBalanceDetailVM.BankMasterId))
                {
                    if (string.IsNullOrEmpty(openingBalanceDetailVM.BankMasterId))
                        throw new CustomException($"({openingBalanceDetailVM.BankName}) Id is null!");
                    else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Doc Date is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Doc Ref is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Narration is null!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
                }
            }
            _openingBalanceService.Insert(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpGet]
        public JsonResult GetLoanTakenList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.Loan.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, TransactionType.LoanTaken.ToString()), JsonRequestBehavior.AllowGet);
        }

       
        [HttpGet, Authorize]
        public JsonResult GetOBLoanTakenDetail(string openingBalanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetOBLoanTakenDetailGL(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, openingBalanceId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetLoanGivenOBList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.Loan.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, TransactionType.LoanGiven.ToString()), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetOBLoanGivenDetail(string openingBalanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetOBLoanGivenDetailGL(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, openingBalanceId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult UpdateLoanTaken(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.SourceType = SourceType.Loan.ToString();
            openingBalance.TransactionType = TransactionType.LoanTaken.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (!string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                {
                    if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                        throw new CustomException($"({openingBalanceDetailVM.PartyName}) Id is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyType))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Type is null!");
                    else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Date is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Narration is null!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
                }
                else if (!string.IsNullOrEmpty(openingBalanceDetailVM.BankMasterId))
                {
                    if (string.IsNullOrEmpty(openingBalanceDetailVM.BankMasterId))
                        throw new CustomException($"({openingBalanceDetailVM.BankName}) Id is null!");
                    else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Doc Date is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Doc Ref is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Narration is null!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
                }
            }
            _openingBalanceService.Update(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public ActionResult LoanGiven()
        {
            return View();
        }

        [HttpPost]
        public JsonResult InsertLoanGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.SourceType = SourceType.Loan.ToString();
            openingBalance.TransactionType = TransactionType.LoanGiven.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (!string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                {
                    if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                        throw new CustomException($"({openingBalanceDetailVM.PartyName}) Id is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyType))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Type is null!");
                    else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Date is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Narration is null!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
                }
                else if (!string.IsNullOrEmpty(openingBalanceDetailVM.BankMasterId))
                {
                    if (string.IsNullOrEmpty(openingBalanceDetailVM.BankMasterId))
                        throw new CustomException($"({openingBalanceDetailVM.BankName}) Id is null!");
                    else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Doc Date is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Doc Ref is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Narration is null!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
                }
            }
            _openingBalanceService.Insert(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpGet]
        public JsonResult GetLoanGivenList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.Loan.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, TransactionType.LoanGiven.ToString()), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateLoanGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.SourceType = SourceType.Loan.ToString();
            openingBalance.TransactionType = TransactionType.LoanGiven.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (!string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                {
                    if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                        throw new CustomException($"({openingBalanceDetailVM.PartyName}) Id is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyType))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Type is null!");
                    else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Date is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Narration is null!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
                }
                else if (!string.IsNullOrEmpty(openingBalanceDetailVM.BankMasterId))
                {
                    if (string.IsNullOrEmpty(openingBalanceDetailVM.BankMasterId))
                        throw new CustomException($"({openingBalanceDetailVM.BankName}) Id is null!");
                    else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Doc Date is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Doc Ref is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Narration is null!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                        throw new CustomException($"Bank ({openingBalanceDetailVM.BankName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
                }
            }
            _openingBalanceService.Update(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Updated });
        }

       
        public ActionResult InterLoanGiven()
        {
            return View();
        }

        [HttpPost]
        public JsonResult InsertInterLoanGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.DocDate = openingBalance.DocDate;
                openingBalanceDetailVM.DocRefNo = openingBalance.DocRefNo;
                if (openingBalance.PartyType == PartyType.Company.ToString())
                {
                    openingBalanceDetailVM.PartyType = PartyType.Company.ToString();

                    if (string.IsNullOrEmpty(openingBalanceDetailVM.CompanyId))
                        throw new CustomException($"({openingBalanceDetailVM.PartyName}) Id is null!");
                    else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                        throw new CustomException($"Company ({openingBalanceDetailVM.PartyName}) Doc Date is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                        throw new CustomException($"Company ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                        throw new CustomException($"Company ({openingBalanceDetailVM.PartyName}) Narration is null!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId &&
                        openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                        throw new CustomException($"Company ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId &&
                        openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                        throw new CustomException($"Company ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId &&
                        openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                        throw new CustomException($"Company ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
                }
                else if (openingBalance.PartyType == PartyType.Entity.ToString())
                {
                    openingBalanceDetailVM.PartyType = PartyType.Entity.ToString();

                    if (string.IsNullOrEmpty(openingBalanceDetailVM.EntityId))
                        throw new CustomException($"({openingBalanceDetailVM.PartyName}) Id is null!");
                    else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                        throw new CustomException($"Entity ({openingBalanceDetailVM.PartyName}) Doc Date is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                        throw new CustomException($"Entity ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                        throw new CustomException($"Entity ({openingBalanceDetailVM.PartyName}) Narration is null!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId &&
                        openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                        throw new CustomException($"Entity ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId &&
                        openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                        throw new CustomException($"Entity ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId &&
                        openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                        throw new CustomException($"Entity ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
                }
            }
            _openingBalanceService.InsertInterLoanGiven(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [Authorize, HttpGet]
        public JsonResult GetInterLoanGivenList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetInterLoanGivenList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateInterLoanGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");
            _openingBalanceService.UpdateInterLoanGiven(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public ActionResult InterPlantLoanTaken()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetInterPlantLoanTakenList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.InterPlantLoanTaken.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult UpdateInterPlantLoanTaken(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");
            _openingBalanceService.UpdateInterPlantTaken(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpGet, Authorize]
        public ActionResult InterCompanyLoanTaken()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetInterCompanyLoanTakenList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.InterCompanyLoanTaken.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult UpdateInterCompanyLoanTaken(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");
            _openingBalanceService.UpdateInterCompanyTaken(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        #endregion Loan

        #region Investment

       
        public ActionResult InvestmentTaken()
        {
            return View();
        }

        [HttpPost]
        public JsonResult InsertInvestmentTaken(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.SourceType = SourceType.Investment.ToString();
            openingBalance.TransactionType = TransactionType.InvestmentTaken.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (!string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                {
                    if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                        throw new CustomException($"({openingBalanceDetailVM.PartyName}) Id is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyType))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Type is null!");
                    else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Date is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Narration is null!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
                }
            }
            _openingBalanceService.Insert(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpGet]
        public JsonResult GetInvestmentTakenList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.Investment.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateInvestmentTaken(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.SourceType = SourceType.Investment.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (!string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                {
                    if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                        throw new CustomException($"({openingBalanceDetailVM.PartyName}) Id is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyType))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Type is null!");
                    else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Date is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Narration is null!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
                }
            }
            _openingBalanceService.Update(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Updated });
        }

       
        public ActionResult InvestmentGiven()
        {
            return View();
        }

        [HttpPost]
        public JsonResult InsertInvestmentGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.SourceType = SourceType.Investment.ToString();
            openingBalance.TransactionType = TransactionType.InvestmentGiven.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (!string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                {
                    if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                        throw new CustomException($"({openingBalanceDetailVM.PartyName}) Id is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyType))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Type is null!");
                    else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Date is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Narration is null!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
                }
            }
            _openingBalanceService.Insert(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpGet]
        public JsonResult GetInvestmentGivenList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.Investment.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateInvestmentGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            openingBalance.SourceType = SourceType.Investment.ToString();

            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.PlantId = openingBalance.PlantId;
                if (!string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                {
                    if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                        throw new CustomException($"({openingBalanceDetailVM.PartyName}) Id is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyType))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Type is null!");
                    else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Date is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Narration is null!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
                }
            }
            _openingBalanceService.Update(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public ActionResult InterInvestmentGiven()
        {
            return View();
        }

        [HttpPost, Authorize]
        public JsonResult InsertInterInvestmentGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.DocDate = openingBalance.DocDate;
                openingBalanceDetailVM.DocRefNo = openingBalance.DocRefNo;
                if (!string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                {
                    if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                        throw new CustomException($"({openingBalanceDetailVM.PartyName}) Id is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyType))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Type is null!");
                    else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Date is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Narration is null!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
                }
            }
            _openingBalanceService.InsertInterInvestmentGiven(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        
        public JsonResult GetInterInvestmentGivenList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetInterInvestmentGivenList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateInterInvestmentGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");
            _openingBalanceService.UpdateInterInvestmentGiven(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [Authorize, HttpGet]
        public ActionResult InterPlantInvestmentTaken()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetInterPlantInvestmentTakenList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetInterPlantInvestmentTakenList(parameters, SourceType.InterPlantInvestmentTaken.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult UpdateInterPlantInvestmentTaken(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");
            _openingBalanceService.UpdateInterPlantTaken(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [Authorize, HttpGet]
        public ActionResult InterCompanyInvestmentTaken()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetInterCompanyInvestmentTakenList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.InterCompanyInvestmentTaken.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult UpdateInterCompanyInvestmentTaken(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");
            _openingBalanceService.UpdateInterCompanyTaken(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }


        [Authorize, HttpGet]
        public JsonResult GetEquityOBList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.Investment.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, TransactionType.InvestmentTaken.ToString()), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEquityOBDetail(string openingBalanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetOBEquityDetailGL(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, openingBalanceId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInvestmentOBList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.Investment.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, TransactionType.InvestmentGiven.ToString()), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetInvestmentOBDetail(string openingBalanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetOBInvestmentDetailGL(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, openingBalanceId), JsonRequestBehavior.AllowGet);
        }
        #endregion Investment

        #region Asset

        
        public ActionResult FixedAssetMaster()
        {
            return View();
        }

        [HttpPost]
        public JsonResult InsertFixedAsset(OpeningBalance openingBalance, IEnumerable<MaterialMasterOpeningBalanceDetailViewModel> materialMasterOpeningBalanceDetailList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (materialMasterOpeningBalanceDetailList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            _openingBalanceService.InsertFixedAsset(openingBalance, materialMasterOpeningBalanceDetailList);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult UpdateFixedAsset(OpeningBalance openingBalance, IEnumerable<MaterialMasterOpeningBalanceDetailViewModel> materialMasterOpeningBalanceDetailList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (materialMasterOpeningBalanceDetailList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            _openingBalanceService.UpdateFixedAsset(openingBalance, materialMasterOpeningBalanceDetailList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult DeleteFixedAsset(string id)
        {
            _openingBalanceService.DeleteFixedAsset(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult DeleteOPDetail(string id)
        {
            _openingBalanceService.DeleteOPDetail(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet]
        public ActionResult GetFixedAssetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.QueryAsset(parameters, SourceType.FixedAsset.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        #endregion Asset

        #region InterTransaction

        [HttpGet, Authorize]
        public ActionResult InterTransactionGiven()
        {
            return View();
        }

        [HttpPost, Authorize]
        public JsonResult InsertInterTransactionGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            foreach (var openingBalanceDetailVM in openingBalanceDetailVMList)
            {
                openingBalanceDetailVM.DocDate = openingBalance.DocDate;
                openingBalanceDetailVM.DocRefNo = openingBalance.DocRefNo;
                if (!string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                {
                    if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyId))
                        throw new CustomException($"({openingBalanceDetailVM.PartyName}) Id is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.PartyType))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Type is null!");
                    else if (openingBalanceDetailVM.DocDate == DateTime.MinValue)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Date is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.DocRefNo))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Doc Ref is null!");
                    else if (string.IsNullOrEmpty(openingBalanceDetailVM.Narration))
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Narration is null!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.CompanyGroupCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.CompanyGroupCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.CompanyGroupCurrencyName} amount is not equal!");
                    else if (openingBalanceDetailVM.CurrencyId == openingBalanceDetailVM.HardCurrencyId && openingBalanceDetailVM.Amount != openingBalanceDetailVM.HardCurrencyAmount)
                        throw new CustomException($"Party ({openingBalanceDetailVM.PartyName}) Transaction amount and {openingBalanceDetailVM.HardCurrencyAmount} amount is not equal!");
                }
            }
            _openingBalanceService.InsertInterTransactionGiven(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [Authorize, HttpGet]
        public JsonResult GetInterTransactionGivenList(GridParameter parameters)
        {
            AccountsOpeningBalanceService _accountsOpeningBalanceService = new AccountsOpeningBalanceService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsOpeningBalanceService.GetInterTransactionGivenList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateInterTransactionGivenGiven(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");
            _openingBalanceService.UpdateInterTransactionGiven(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [Authorize, HttpGet]
        public ActionResult InterPlantTransactionTaken()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetInterPlantTransactionTakenList(GridParameter parameters)
        {
            AccountsOpeningBalanceService _accountsOpeningBalanceService = new AccountsOpeningBalanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountsOpeningBalanceService.GetInterPlantTransactionTakenList(parameters, SourceType.InterPlantTransactionTaken.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult UpdateInterPlantTransactionTaken(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");
            _openingBalanceService.UpdateInterPlantTaken(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [Authorize, HttpGet]
        public ActionResult InterCompanyTransactionTaken()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetInterCompanyTransactionTakenList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.Query(parameters, SourceType.InterCompanyTransactionTaken.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult UpdateInterCompanyTransactionTaken(OpeningBalance openingBalance, IEnumerable<VoucherDetailViewModel> openingBalanceDetailVMList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (openingBalanceDetailVMList == null)
                throw new CustomException("Please Add Line Items.");
            _openingBalanceService.UpdateInterCompanyTaken(openingBalance, openingBalanceDetailVMList);
            return Json(new { Message = AplosMessage.Insert });
        }

        #endregion InterTransaction

       
        
        public ActionResult Report()
        {
            return View();
        }

      
        public ActionResult MaterialMasterOpeningBalanceReport()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult OpeningBalanceReport(string parallelCurrency)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Opening Balance Report " + DateTime.Now.ToString("ddMMMyyyy") + "";
            var workbook = _openingBalanceService.GetOpeningBalanceReport(identity.CompanyId, identity.PlantName, new JavaScriptSerializer().Deserialize<string[]>(parallelCurrency));
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        [HttpGet, Authorize]
        public ActionResult OpeningBalanceJournalReport(string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Opening Balance Journal Report " + DateTime.Now.ToString("ddMMMyyyy") + "";
            var workbook = _openingBalanceService.GetOpeningBalanceJournal(identity.CompanyId, identity.PlantName, voucherId);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        #region-- Material Master
       
        public ActionResult MaterialMaster()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetMaterialMasterList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.QueryAsset(parameters, SourceType.MaterialMaster.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetMaterialMasterOB(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetMaterialMasterOB(parameters, SourceType.MaterialMaster.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetNonFinancialMaterialMasterOB(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetNonFinancialMaterialMasterOB(parameters, SourceType.MaterialMaster.ToString(), identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetMaterialMasterOBGL(string openingBalanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetMaterialMasterOBGL(openingBalanceId, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertMaterialMaster(OpeningBalance openingBalance, IEnumerable<MaterialMasterOpeningBalanceDetailViewModel> materialMasterOpeningBalanceDetailList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (materialMasterOpeningBalanceDetailList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            _openingBalanceService.InsertMaterialMaster(openingBalance, materialMasterOpeningBalanceDetailList);
            return Json(new { openingBalance, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult UpdateMaterialMaster(OpeningBalance openingBalance, IEnumerable<MaterialMasterOpeningBalanceDetailViewModel> materialMasterOpeningBalanceDetailList)
        {
            if (openingBalance == null)
                throw new CustomException("null");
            if (materialMasterOpeningBalanceDetailList == null)
                throw new CustomException("Please Add Line Items.");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            openingBalance.CompanyGroupId = identity.CompanyGroupId;
            openingBalance.CompanyId = identity.CompanyId;
            openingBalance.PlantId = identity.PlantId;
            _openingBalanceService.UpdateMaterialMaster(openingBalance, materialMasterOpeningBalanceDetailList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult DeleteMaterialMaster(string id)
        {
            _openingBalanceService.DeleteMaterialMaster(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


       
        public ActionResult NonFinancialMaterialOBPost()
        {
            return View();
        }


        [HttpPost]
        public JsonResult PostNonFinancialMaterialOB(VoucherViewModel voucherVM, string voucherDetailVMList)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            List<VoucherDetailViewModel> voucherDetailVM = JsonConvert.DeserializeObject<List<VoucherDetailViewModel>>(voucherDetailVMList, settings);
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _openingBalanceService.PostNonFinancialMaterialOB(voucherVM, voucherDetailVM)) });
        }

        [HttpGet, Authorize]
        public JsonResult GetNonFinancialMaterialOBPostedList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetNonFinancialMaterialPostedList(parameters, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Delete Back Data Account CutOffDate
        [Authorize]
        public ActionResult DeleteAccCutOffDateBackData()
        {
            return View();
        }
        [Authorize, HttpGet]
        public JsonResult GetCpanelACCCutOffDate(string companyGroupId, string companyId)
        {
            return Json(_openingBalanceCutOffDateService.GetACCCutOffDate(companyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCutOffBackDateData(string companyGroupId, string companyId, string plantId, DateTime postingDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetCutOffBackDateData(companyGroupId, companyId, plantId, postingDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeePayableCutOffAfterPostingDateData(string companyGroupId, string companyId, string plantId, DateTime postingDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetEmployeePayableCutOffAfterPostingDateData(companyGroupId, companyId, plantId, postingDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetVendorPayableCutOffAfterPostingDateData(string companyGroupId, string companyId, string plantId, DateTime postingDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_openingBalanceService.GetVendorPayableCutOffAfterPostingDateData(companyGroupId, companyId, plantId, postingDate), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteEmployeePayableCutOffAfterPostingDateData(IEnumerable<VoucherDetailViewModel> voucherDetailVM)
        {
            return Json(_openingBalanceService.DeleteEmployeePayableCutOffAfterPostingDateData(voucherDetailVM), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteVendorPayableCutOffAfterPostingDateData(IEnumerable<VoucherDetailViewModel> voucherDetailVM)
        {
            return Json(_openingBalanceService.DeleteVendorPayableCutOffAfterPostingDateData(voucherDetailVM), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult PostDeleteAccCutOffDateBackData(VoucherViewModel voucherVM)
        {
            return Json(new { Message = string.Format(AplosMessage.Deleted, _openingBalanceService.PostDeleteAccCutOffDateBackData(voucherVM)) });
        }
        #endregion

        /* Mr. Taufiq u do you report from here*/
        [HttpGet]
        public ActionResult PaybleVSpaymentReport(ReportFormat reportFormat,string companyId, string plantId, string fromDate)
        {
            var reportFileName = "Payble VS payment"  ;// +fromDate + "To" + toDate + ""
             var workbook = _openingBalanceService.CreatePaybleVSpaymentReportSheet(companyId, plantId, fromDate);
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

        #region material-master-opening-balance-report


        //[HttpGet]
        //public ActionResult MaterialMasterOpeningBalanceReport(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string MaterialId, string ArticleId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    plantId = identity.PlantId;
        //    var reportFileName = "MaterialMasterOpeningBalance " + fromDate + "To" + toDate + "";
        //    var workbook = _openingBalanceService.CreateMaterialMasterOpeningBalanceReport(identity.CompanyId, plantId, fromDate, toDate, Qty, Amount, RcptIssue, MaterialId, ArticleId);
        //    switch (reportFormat)
        //    {
        //        case ReportFormat.Pdf:
        //            return RenderReportAsPdf(workbook, reportFileName);

        //        case ReportFormat.Excel:
        //            return RenderReportAsExcel(workbook, reportFileName);

        //        default:
        //            return View();
        //    }
        //}

        [HttpGet, Authorize]
        public ActionResult MaterialMasterOpeningBalanceRpt(ReportFormat reportFormat, string companyId, string plantId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string reportFileName = "Material Master Opening Balance" + fromDate + "To" + toDate + "";
            var workbook = _openingBalanceService.CreateMaterialMasterOpeningBalanceReport(identity.CompanyId, identity.PlantId, fromDate, toDate);
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


        [HttpGet, Authorize]
        public JsonResult LoadMaterialEnulType(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                var sql = @"SELECT MM.Id ,MS.TypeValue--,MMT.User
                            FROM [HKP].[MaterialMasterType] MMT 				
                            LEFT JOIN [dbo].[MaterialSetting] AS MS ON MMT.Id=MS.MaterialMasterTypeId
                            LEFT JOIN [MST].[MaterialMaster] AS MM ON MM.MaterialMasterTypeId=MS.MaterialMasterTypeId                            
					       where  MM.Id='" + Id+ "' AND  MM.MaterialMasterTypeId is not null";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Opening Balance Report code  
        public ActionResult OpeningBalanceReportExcel()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                // if (string.IsNullOrEmpty(MasterLCList))
                //   throw new Exception("Please select at least one master Order");

                ExcelEngine excelEngine = new ExcelEngine();

                IWorkbook workbook = OpeningBalanceList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

                string strFileName = "Opening Balance.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }

        [HttpGet, Authorize]
        public ActionResult OpeningBalanceReportPdf()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                // if (string.IsNullOrEmpty(MasterLCList))
                //   throw new Exception("Please select at least one master Order");

                ExcelEngine excelEngine = new ExcelEngine();

                IWorkbook workbook = OpeningBalanceList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

                string strFileName = "Opening Balance.pdf";
                ExcelToPdfConverter convert = new ExcelToPdfConverter(workbook);
                PdfDocument pdfDoc = convert.Convert();
                workbook.Close();
                pdfDoc.Save(strFileName, System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }

        private IWorkbook OpeningBalanceList(string companyGroupId, string companyId, string plantId)
        {

            //Start EmployeeAdvanceDueList


            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            DataTable dtOpeningBalanceList = _sqlRepository.GetDataTable(@"SELECT  DISTINCT  ROW_NUMBER() Over(Order by  IM.Id) As[S.N], 
                     IRD.Id GRNID,CG.UserName CompanyGroup
                    ,C.UserName Company
                    ,P.UserName Plant
                    ,E.UserName Entity
                    ,MGM.UserName AS MaterialGroupMasterName
                    ,mm.Id MaterialMasterId
                    ,MM.UserName MaterialMasterName
                    ,ART.StandardName ArticleName
                    ,ART.Id ARTId
                    ,TUoM.UserName BaseUOM
					  ,OB.Id OBID
                    ,OB.DocRefNo
                    ,ISNULL(FCV.UserName, '') AS FirstCharacteristicsValue
                    ,ISNULL(SCV.UserName, '') AS SecondCharacteristicsValue
                    ,ISNULL(TCV.UserName, '') AS ThirdCharacteristicsValue
                    ,IRD.Quantity
                    ,IRD.Amount
                    ,OB.VoucherId
                    ,OB.SourceType
                    ,REPLACE(CONVERT(CHAR(11), OB.PostingDate, 106), ' ', '-') AS PostingDate
                    ,REPLACE(CONVERT(CHAR(11), OB.DocDate, 106), ' ', '-') DocDate
                    ,OB.Narration
                    ,OB.IsPark
                    ,OB.Archive
                    ,OB.PartyType
                    ,OB.IsPosted
                    ,OB.TransactionType
                    ,OB.IsFinancial
                    ,MS.UserName StorageLocation
                    --,Cu.code Currency
                    --,OBD.AssetGLId
                    --,OBD.AssetBudgetMasterId
                    --,OBD.AssetActivityId
                    --,OBD.VendorArticulationId
                    --,OBDC.ParallelCurrencyId ParallelCurrency
                    --,OBDC.FromCurrencyId FromCurrency
                    --,OBDC.ToCurrencyId ToCurrency
                    --,OBDC.ToCurrencyConversion ToCurrencyConversion
                    ----,OBDC.Amount
                    --,OBDC.GLType


                    FROM TRN.InventoryMaterial AS IM
                    left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                    LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                    LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                    LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                    LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                    LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                    LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                    LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                    LEFT JOIN (select Id,InventoryMaterialId,MaterialStorageId,BaseUOMId,InventoryReceiveId, sum(TransactionQty) Quantity, sum(TotalMaterialBooksCurrencyAmount) Amount from TRN.InventoryReceiveDetail group by InventoryMaterialId,MaterialStorageId ,BaseUOMId,InventoryReceiveId,Id)IRD ON IRD.InventoryMaterialId= IM.Id
                    LEFT JOIN TRN.InventoryReceive AS IR On IR.Id=IRD.InventoryReceiveId
                    left JOIN TRN.OpeningBalance OB ON OB.Id=IR.OpeningBalanceId
					 LEFT JOIN ORG.Entity E ON E.Id = OB.EntityId
                    LEFT JOIN ORG.CompanyGroup CG ON CG.Id = IM.CompanyGroupId
                    LEFT JOIN ORG.Company C ON C.Id = IM.CompanyId
                    LEFT JOIN ORG.Plant P ON P.Id = IM.PlantId                   
                    LEFT JOIN [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.BaseUOMId= TUoM.Id
                    --LEFT JOIN [SCS].[Currency] AS CU ON IRD.CurrencyId= CU.Id
                    where IR.OpeningBalanceId<>'' And mm.UserName <>'' and IM.PlantId='" + plantId + @"'");

            if (dtOpeningBalanceList.Rows.Count == 0)
                throw new Exception("No data found");




            worksheet.Name = "OpeningBalanceRegister";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            // worksheet[ROW, COL].Text = "Employee Advance Due List Details:";
            // worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //  ROW++;


            worksheet[ROW, COL].Text = "S.N";
            int colSLNO = COL;
            worksheet[ROW, COL].ColumnWidth = 5;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Company Group";
            int colCompanyGroup = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Company";
            int colCompany = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Plant";
            int colPlantName = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Entity";
            int colEntity = COL;
            worksheet[ROW, COL].ColumnWidth = 18;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GRN ID";
            int colGRNId = COL;
            worksheet[ROW, COL].ColumnWidth = 18;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "OB ID";
            int colOBID = COL;
            worksheet[ROW, COL].ColumnWidth = 18;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Doc Ref No";
            int colDocRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 18;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Material Group Master";
            int colMaterialGroupMasterName = COL;
            worksheet[ROW, COL].ColumnWidth = 22;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;



            worksheet[ROW, COL].Text = "Material Master Id";
            int colMaterialMasterId = COL;
            worksheet[ROW, COL].ColumnWidth = 18;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Material ";
            int colMaterialMasterName = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Article ID";
            int colArticleID = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Article";
            int colArticleName = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "SKU1";
            int colFirstCharacteristicsValue = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "SKU2";
            int colSecondCharacteristicsValue = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "SKU3";
            int colThirdCharacteristicsValue = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "BaseUOM";
            int colBaseUOM = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Quantity";
            int colQuantity = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Amount";
            int colAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;



            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 10;
            worksheet.Range[ROW, 1, ROW, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[ROW, 1, ROW, endCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            worksheet.Range[ROW, 1, ROW, endCol].RowHeight = 22;
            ROW++;

            for (int i = 0; i < dtOpeningBalanceList.Rows.Count; i++)
            {
                // int i = 0; i < dtMasterOrderItem.Rows.Count; i++
                worksheet[ROW, colSLNO].Text = dtOpeningBalanceList.Rows[i]["S.N"].ToString();
                worksheet[ROW, colCompanyGroup].Text = dtOpeningBalanceList.Rows[i]["CompanyGroup"].ToString();
                worksheet[ROW, colCompany].Text = dtOpeningBalanceList.Rows[i]["Company"].ToString();
                worksheet[ROW, colPlantName].Text = dtOpeningBalanceList.Rows[i]["Plant"].ToString();
                worksheet[ROW, colEntity].Text = dtOpeningBalanceList.Rows[i]["Entity"].ToString();
                worksheet[ROW, colGRNId].Text = dtOpeningBalanceList.Rows[i]["GRNID"].ToString();
                worksheet[ROW, colOBID].Text = dtOpeningBalanceList.Rows[i]["OBID"].ToString();
                worksheet[ROW, colDocRefNo].Text = dtOpeningBalanceList.Rows[i]["DocRefNo"].ToString();
                worksheet[ROW, colMaterialGroupMasterName].Text = dtOpeningBalanceList.Rows[i]["MaterialGroupMasterName"].ToString();
                worksheet[ROW, colMaterialMasterId].Text = dtOpeningBalanceList.Rows[i]["MaterialMasterId"].ToString();
                worksheet[ROW, colMaterialMasterName].Text = dtOpeningBalanceList.Rows[i]["MaterialMasterName"].ToString();
                worksheet[ROW, colArticleID].Text = dtOpeningBalanceList.Rows[i]["ARTId"].ToString();
                worksheet[ROW, colArticleName].Text = dtOpeningBalanceList.Rows[i]["ArticleName"].ToString();
                worksheet[ROW, colBaseUOM].Text = dtOpeningBalanceList.Rows[i]["BaseUOM"].ToString();
                worksheet[ROW, colFirstCharacteristicsValue].Text = dtOpeningBalanceList.Rows[i]["FirstCharacteristicsValue"].ToString();
                worksheet[ROW, colSecondCharacteristicsValue].Text = dtOpeningBalanceList.Rows[i]["SecondCharacteristicsValue"].ToString();
                worksheet[ROW, colThirdCharacteristicsValue].Text = dtOpeningBalanceList.Rows[i]["ThirdCharacteristicsValue"].ToString();
                worksheet[ROW, colQuantity].Number = OTSBD.clsStaticInfo.dbl(dtOpeningBalanceList.Rows[i]["Quantity"].ToString());
                worksheet[ROW, colAmount].Number = OTSBD.clsStaticInfo.dbl(dtOpeningBalanceList.Rows[i]["Amount"].ToString());
                //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();

                // worksheet[startRowGroup1, colSLNO, ROW - 1, colSLNO].Merge();
                //worksheet[StartDataRow, colPurchaseLCAmount, ROW - 1, colPurchaseLCAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                // worksheet.Range[ROW, 1, ROW, endCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                ROW++;
            }

            //sheet1.Range[(Row_Total_Start), 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 8;

            worksheet.UsedRange.CellStyle.Font.FontName = "Tahoma";

            //worksheet.UsedRange.CellStyle.Font.Size = 8f;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Library.Service.Helpers.ReportUtility reportUtility = new Library.Service.Helpers.ReportUtility();
            // reportUtility.PlantHeaderWithOutLogo(ref worksheet, endCol, "Gatenntry Register", identity.PlantId);
            reportUtility.PlantHeader(ref worksheet, endCol, "Opening Balance Register", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            // worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            //worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;
            return workbook;
        }

        [HttpGet, Authorize]
        public JsonResult OpeningBalanceLoadOnData()

        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string _sql = @"SELECT  
                    DISTINCT IRD.Id GRNID,CG.UserName CompanyGroup
                    ,C.UserName Company
                    ,P.UserName Plant
                    ,E.UserName Entity
                    ,MGM.UserName AS MaterialGroupMasterName
                    ,mm.Id MaterialMasterId
                    ,MM.UserName MaterialMasterName
                    ,ART.StandardName ArticleName
                    ,TUoM.UserName BaseUOM
					  ,OB.Id OBID
                    ,OB.DocRefNo
                    ,ISNULL(FCV.UserName, '') AS FirstCharacteristicsValue
                    ,ISNULL(SCV.UserName, '') AS SecondCharacteristicsValue
                    ,ISNULL(TCV.UserName, '') AS ThirdCharacteristicsValue
                    ,IRD.Quantity
                    ,IRD.Amount
                    ,OB.VoucherId
                    ,OB.SourceType
                    ,REPLACE(CONVERT(CHAR(11), OB.PostingDate, 106), ' ', '-') AS PostingDate
                    ,REPLACE(CONVERT(CHAR(11), OB.DocDate, 106), ' ', '-') DocDate
                    ,OB.Narration
                    ,OB.IsPark
                    ,OB.Archive
                    ,OB.PartyType
                    ,OB.IsPosted
                    ,OB.TransactionType
                    ,OB.IsFinancial
                    ,MS.UserName StorageLocation
                    --,Cu.code Currency
                    --,OBD.AssetGLId
                    --,OBD.AssetBudgetMasterId
                    --,OBD.AssetActivityId
                    --,OBD.VendorArticulationId
                    --,OBDC.ParallelCurrencyId ParallelCurrency
                    --,OBDC.FromCurrencyId FromCurrency
                    --,OBDC.ToCurrencyId ToCurrency
                    --,OBDC.ToCurrencyConversion ToCurrencyConversion
                    ----,OBDC.Amount
                    --,OBDC.GLType


                    FROM TRN.InventoryMaterial AS IM
                    left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                    LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                    LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                    LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                    LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                    LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                    LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                    LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                    LEFT JOIN (select Id,InventoryMaterialId,MaterialStorageId,BaseUOMId,InventoryReceiveId, sum(TransactionQty) Quantity, sum(TotalMaterialBooksCurrencyAmount) Amount from TRN.InventoryReceiveDetail group by InventoryMaterialId,MaterialStorageId ,BaseUOMId,InventoryReceiveId,Id)IRD ON IRD.InventoryMaterialId= IM.Id
                    LEFT JOIN TRN.InventoryReceive AS IR On IR.Id=IRD.InventoryReceiveId
                    left JOIN TRN.OpeningBalance OB ON OB.Id=IR.OpeningBalanceId
					 LEFT JOIN ORG.Entity E ON E.Id = OB.EntityId
                    LEFT JOIN ORG.CompanyGroup CG ON CG.Id = IM.CompanyGroupId
                    LEFT JOIN ORG.Company C ON C.Id = IM.CompanyId
                    LEFT JOIN ORG.Plant P ON P.Id = IM.PlantId                   
                    LEFT JOIN [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.BaseUOMId= TUoM.Id
                    --LEFT JOIN [SCS].[Currency] AS CU ON IRD.CurrencyId= CU.Id
                    where IR.OpeningBalanceId<>'' And mm.UserName <>'' and IM.PlantId='" + identity.PlantId + @"'";

                return Json(_sqlRepository.GetDataCollection(_sql), JsonRequestBehavior.AllowGet);
                //var res=_sqlRepository.GetDataCollection(_sql);
                //var jsondata = Json(res, JsonRequestBehavior.AllowGet);
                //jsondata.MaxJsonLength = int.MaxValue;
                //return jsondata;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }

        }

        #endregion Opening Balance Report code  

        #region Opening Balance Report by Aakash  
        [HttpGet, Authorize]
        public ActionResult MaterialMasterOpenningBalanceReport(string OpenningBalanceId)
        {
            try
            {
                CreatMaterialMasterOpenningBalanceReport(OpenningBalanceId);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string OpenningBalanceSql(string OpenningBalanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"DECLARE @companyId VARCHAR(10)='" + identity.CompanyId + "',@plantId VARCHAR(10)='" + identity.PlantId + @"';
                          SELECT 
									distinct IM.Id as InventoryReceivedId
									,FOBD.Id,IRDD.Id InventoryReceiveDetailId, FOBD.OpeningBalanceId,AGL.AccountCode+' - '+AGL.UserName AS AssetGLName, ACGL.AccountCode+' - '+ACGL.UserName AS AccDepreciation
							       ,FOBD.AccumulatedDepreciationGLId,FOBD.AccumulatedDepreciationBudgetMasterId,FOBD.AccumulatedDepreciationActivityId,AB.UserName BudgetName,AC.UserName AssetActivityName,BM.BudgetCategoryId,BM.BudgetSubCategoryId,ACB.UserName ACUBudgetName
								   ,FOBD.FixedAssetMasterId,FOBD.AssetBudgetMasterId,FOBD.AssetActivityId, FAM.UserName AS FixedAssetMasterName, FOBD.MaterialMasterId MMID, FOBD.BaseUOMId, UOM.UserName AS BaseUoM, FOBD.AssetGLId, FOBD.CurrencyId, FOBD.Quantity,FOBD.Quantity QuantityOld
                                    ,CC.CompanyCurrencyId,CC.Currency CompanyCurrency, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.FACompanyCurrencyRate, CC.FACompanyCurrencyAmount,CC.FACompanyCurrencyAmount FACompanyCurrencyAmountOld, ADCC.ADCompanyCurrencyRate, ADCC.ADCompanyCurrencyAmount,
                                    GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.FACompanyGroupCurrencyRate, GC.FACompanyGroupCurrencyAmount, ADGC.ADCompanyGroupCurrencyRate, ADGC.ADCompanyGroupCurrencyAmount,
                                    HC.HardCurrencyId, HC.HardFromCurrencyId, HC.FAHardCurrencyRate, HC.FAHardCurrencyAmount, ADHC.ADHardCurrencyRate, ADHC.ADHardCurrencyAmount
									,CCD.FACompanyCurrencyDirectRate, CCD.FACompanyCurrencyDirectAmount
									,CCID.FACompanyCurrencyInDirectRate, CCID.FACompanyCurrencyInDirectAmount
									,GCD.FACompanyGroupCurrencyDirectRate, GCD.FACompanyGroupCurrencyDirectAmount
									,GCID.FACompanyGroupCurrencyInDirectRate, GCID.FACompanyGroupCurrencyInDirectAmount
									,HCD.FAHardCurrencyDirectRate, HCD.FAHardCurrencyDirectAmount
									,HCID.FAHardCurrencyInDirectRate, HCID.FAHardCurrencyInDirectAmount
									,ADCCD.ADCompanyCurrencyDirectRate, ADCCD.ADCompanyCurrencyDirectAmount
									,ADCCID.ADCompanyCurrencyInDirectRate, ADCCID.ADCompanyCurrencyInDirectAmount
									,ADGCD.ADCompanyGroupCurrencyDirectRate, ADGCD.ADCompanyGroupCurrencyDirectAmount
									,ADGCID.ADCompanyGroupCurrencyInDirectRate, ADGCID.ADCompanyGroupCurrencyInDirectAmount
									,ADHCD.ADHardCurrencyDirectRate, ADHCD.ADHardCurrencyDirectAmount
									,ADHCID.ADHardCurrencyInDirectRate, ADHCID.ADHardCurrencyInDirectAmount
									,FOBD.MaterialMasterId, MM.UserName MaterialMasterName,FOBD.ArticleId, MMA.StandardName ArticleName,FOBD.MaterialStorageId,FOBD.FirstCharacteristicsId,FOBD.FirstCharacteristicsValueId,FOBD.SecondCharacteristicsId,FOBD.SecondCharacteristicsValueId
									
									, FC.UserName AS FirstCharacteristics
									
									, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue

									
									, FC.UserName AS SecondCharacteristics
									
									, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue

									, FOBD.ThirdCharacteristicsId
									, FC.UserName AS ThirdCharacteristics
									, FOBD.ThirdCharacteristicsValueId
									, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue,FOBD.LotNumber,FOBD.Diameter,FOBD.Type
                                    FROM [TRN].[MaterialMasterOpeningBalanceDetail] AS FOBD
                                    LEFT JOIN [TRN].[OpeningBalance] AS FOB ON FOBD.OpeningBalanceId=FOB.Id
                                    --LEFT JOIN MST.MaterialMaster AS FAT ON FAT.Id = FOBD.MaterialMasterId
                                    LEFT JOIN MST.FixedAssetMaster AS FAM ON FAM.Id=FOBD.FixedAssetMasterId
									LEFT JOIN [SCS].[UnitOfMeasurement] AS UOM ON UOM.Id=FOBD.BaseUOMId
									LEFT JOIN HKP.GLGeneralInfo AGL ON FOBD.AssetGLId=AGL.Id
									LEFT JOIN HKP.GLGeneralInfo ACGL ON FOBD.AccumulatedDepreciationGLId=ACGL.Id
									LEFT JOIN MST.BudgetMaster BM ON FOBD.AssetBudgetMasterId=BM.Id
									LEFT JOIN HKP.Budget AB ON BM.BudgetId=AB.Id
									LEFT JOIN MST.BudgetMaster ACBBM ON FOBD.AccumulatedDepreciationBudgetMasterId=ACBBM.Id
									LEFT JOIN HKP.Budget ACB ON ACBBM.BudgetId=ACB.Id
                                    LEFT JOIN HKP.Activity AC ON FOBD.AssetActivityId=AC.Id
									LEFT JOIN MST.MaterialMaster MM ON FOBD.MaterialMasterId=MM.Id
									LEFT JOIN MST.MaterialMasterArticle MMA ON FOBD.ArticleId = MMA.Id
                                    LEFT JOIN HKP.Characteristics AS FC ON FOBD.FirstCharacteristicsId=FC.Id
									LEFT JOIN HKP.Characteristics AS SC ON FOBD.SecondCharacteristicsId=SC.Id
									LEFT JOIN HKP.Characteristics AS TC ON FOBD.ThirdCharacteristicsId=TC.Id
									LEFT JOIN HKP.CharacteristicsValue AS FCV ON FOBD.FirstCharacteristicsValueId=FCV.Id
									LEFT JOIN HKP.CharacteristicsValue AS SCV ON FOBD.SecondCharacteristicsValueId=SCV.Id
									LEFT JOIN HKP.CharacteristicsValue AS TCV ON FOBD.ThirdCharacteristicsValueId=TCV.Id
                                    LEFT OUTER JOIN (
	                                     SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,c.Code Currency,
	                                    OBDC.ToCurrencyRate AS FACompanyCurrencyRate, OBDC.Amount AS FACompanyCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    LEFT JOIN scs.Currency AS c ON c.Id=cpc.CurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                                    ) AS CC ON CC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                                    LEFT OUTER JOIN (
                                    SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyGroupCurrencyRate, OBDC.Amount AS FACompanyGroupCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId
                                    ) AS GC ON GC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                                    LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS FAHardCurrencyRate, OBDC.Amount AS FAHardCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId
                                    ) AS HC ON HC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                                    LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId AS CompanyToCurrencyId,
	                                    OBDC.ToCurrencyRate AS ADCompanyCurrencyRate, OBDC.Amount AS ADCompanyCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                                    ) AS ADCC ON ADCC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                                    LEFT OUTER JOIN (
                                    SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS ADCompanyGroupCurrencyRate, OBDC.Amount AS ADCompanyGroupCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId
                                    ) AS ADGC ON ADGC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                                    LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS ADHardCurrencyRate, OBDC.Amount AS ADHardCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId
                                    ) AS ADHC ON ADHC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									--DirectInDirect
									 LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyCurrencyDirectRate, OBDC.Amount AS FACompanyCurrencyDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity DirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='Direct'
                                    ) AS CCD ON CCD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyCurrencyInDirectRate, OBDC.Amount AS FACompanyCurrencyInDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity InDirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='InDirect'
                                    ) AS CCID ON CCID.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									 LEFT OUTER JOIN (
                                    SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyGroupCurrencyDirectRate, OBDC.Amount AS FACompanyGroupCurrencyDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity DirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='Direct'
                                    ) AS GCD ON GCD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									LEFT OUTER JOIN (
                                    SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS FACompanyGroupCurrencyInDirectRate, OBDC.Amount AS FACompanyGroupCurrencyInDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity InDirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='InDirect'
                                    ) AS GCID ON GCID.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									 LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId
	                                    ,OBDC.Quantity DirectQuantity,OBDC.ToCurrencyRate AS FAHardCurrencyDirectRate, OBDC.Amount AS FAHardCurrencyDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='Direct'
                                    ) AS HCD ON HCD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
																		 LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId
	                                    ,OBDC.Quantity InDirectQuantity,OBDC.ToCurrencyRate AS FAHardCurrencyInDirectRate, OBDC.Amount AS FAHardCurrencyInDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='FA' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='InDirect'
                                    ) AS HCID ON HCID.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,
	                                    OBDC.ToCurrencyRate AS ADCompanyCurrencyDirectRate, OBDC.Amount AS ADCompanyCurrencyDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity DirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='Direct'
                                    ) AS ADCCD ON ADCCD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId,
	                                    OBDC.ToCurrencyRate AS ADCompanyCurrencyInDirectRate, OBDC.Amount AS ADCompanyCurrencyInDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity InDirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='InDirect'
                                    ) AS ADCCID ON ADCCID.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									 LEFT OUTER JOIN (
                                    SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS ADCompanyGroupCurrencyDirectRate, OBDC.Amount AS ADCompanyGroupCurrencyDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity DirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='Direct'
                                    ) AS ADGCD ON ADGCD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									LEFT OUTER JOIN (
                                    SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId,
	                                    OBDC.ToCurrencyRate AS ADCompanyGroupCurrencyInDirectRate, OBDC.Amount AS ADCompanyGroupCurrencyInDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
										,OBDC.Quantity InDirectQuantity
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='InDirect'
                                    ) AS ADGCID ON ADGCID.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									 LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId
	                                    ,OBDC.Quantity DirectQuantity,OBDC.ToCurrencyRate AS ADHardCurrencyDirectRate, OBDC.Amount AS ADHardCurrencyDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='Direct'
                                    ) AS ADHCD ON ADHCD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
																		 LEFT OUTER JOIN (
	                                    SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId
	                                    ,OBDC.Quantity InDirectQuantity,OBDC.ToCurrencyRate AS ADHardCurrencyInDirectRate, OBDC.Amount AS ADHardCurrencyInDirectAmount, OBDC.MaterialMasterOpeningBalanceDetailId
	                                    FROM [TRN].[MaterialMasterOpeningBalanceDetailDirectIndirect] AS OBDC
	                                    INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
	                                    WHERE OBDC.GLType='AD' AND CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId AND OBDC.Type='InDirect'
                                    ) AS ADHCID ON ADHCID.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                                    LEFT JOIN TRN.InventoryReceive IR ON IR.OpeningBalanceId=FOB.Id
									--LEFT JOIN (select Distinct IM.Id,IM.MaterialMasterId  
									--		from TRN.InventoryReceiveDetail IRD  									
									--		left JOIN TRN.InventoryReceive IR ON IR.id = IRD.InventoryReceiveId
									--		LEft JOIn TRN.InventoryMaterial IM ON IM.id = IRD.InventoryMaterialId 
									--		--LEFT JOIN  trn.InventoryReceiveDetail IRD1 On IRD1.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									--		ANd IR.OpeningBalanceId='20191')IRD1  ON IRD1.MaterialMasterId = MM.Id 

									left join trn.InventoryReceiveDetail IRDD ON IRDD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									Left join trn.InventoryMaterial IM ON IM.Id=IRDD.InventoryMaterialId
									WHERE FOB.CompanyId=@companyId AND FOB.PlantId=@plantId AND FOB.Id='" + OpenningBalanceId + "'  Order BY FOBD.Id ASC";
        }

        private string OpenningBalanceHeaderSql(string OpenningBalanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT OB.Id, OB.CompanyGroupId, OB.CompanyId, OB.PlantId, OB.EntityId, E.UserName AS EntityName, OB.VoucherId, VD.VoucherNo, OB.EmployeeTransactionTypeId,
OB.FinancingTypeId,
                                OB.SourceType, OB.PartyType,FORMAT( OB.PostingDate,'dd-MMM-yyyy') PostingDate, FORMAT( OB.DocDate,'dd-MMM-yyyy') DocDate
                                ,CASE WHEN OB.IsFinancial=0 THEN 'No' ELSE 'Yes' END FinancialImplication , OB.DocRefNo, OB.Narration, OB.IsPark, OB.IsPosted, OB.[AddedBy], OB.[AddedDate], OB.[AddedFromIP]
                                , X.Amount,OB.MaterialStorageId,MS.UserName MaterialStorage
                                FROM [TRN].[OpeningBalance] AS OB
								LEFT JOIN [TRN].[Voucher] AS VD ON VD.Id=OB.VoucherId
                                LEFT JOIN [ORG].[Entity] AS E ON E.Id=OB.EntityId
                                LEFT JOIN  [HKP].[MaterialStorage] MS ON MS.Id=ob.MaterialStorageId
                                LEFT JOIN( SELECT OBDC.OpeningBalanceId, SUM(OBDC.Amount) AS Amount FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
								INNER JOIN [ORG].[Company] AS C ON C.BaseCurrencyId=OBDC.ParallelCurrencyId
	WHERE OBDC.GLType='FA' AND C.Id='" + identity.CompanyId + @"'
                                GROUP BY OBDC.OpeningBalanceId
								) AS X ON X.OpeningBalanceId=OB.Id
                                WHERE OB.Archive=0 AND OB.SourceType='MaterialMaster' AND OB.CompanyGroupId='" + identity.CompanyGroupId +@"' AND OB.CompanyId='" + identity.CompanyId +@"' AND OB.PlantId='" + identity.PlantId +@"'
                                AND ob.Id = '"+ OpenningBalanceId +@"'";
        }
        public void CreatMaterialMasterOpenningBalanceReport(string OpenningBalanceId)
        {
            try
            {
                var reportUtility = new ReportUtility();

                string HeaderSql = OpenningBalanceHeaderSql(OpenningBalanceId);
                string MMOBRSql = OpenningBalanceSql(OpenningBalanceId);
                //Instantiate the Excel application object

                DataTable dtHeader = _sqlRepository.GetDataTable(HeaderSql);
                DataTable dtOpenningBalance = _sqlRepository.GetDataTable(MMOBRSql);
              //  DataTable dtTermsAndConditions = _sqlRepository.GetDataTable(TermsAndConditionSql);
                if (dtOpenningBalance.Rows.Count == 0)
                    throw new Exception("No data found");
                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Material Master Openning Balance Report";

                int ROW = 6;
                int COL = 1;

                #region Header

                int StartRow = ROW;
                sheet[ROW, COL].Text = "Posting Date:";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colPostingDate = COL;
                ROW++;
                sheet[ROW, COL].Text = "Doc Ref#:";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colDocRef = COL;
                ROW++;
                sheet[ROW, COL].Text = "Storage Location:";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                sheet[ROW, COL].ColumnWidth = 10;
                int colStorageLocation  = COL;
                ROW++;
                sheet[ROW, COL].Text = "Narration :";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                sheet[ROW, COL].ColumnWidth = 10;
                int colNarration = COL;
                ROW = StartRow;
                COL =6;
                sheet[ROW, COL].Text = "Doc Date :";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                sheet[ROW, COL].ColumnWidth = 10;
                int colDocDate = COL;
                ROW++;
                sheet[ROW, COL].Text = "Entity :";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colEntity = COL;
                ROW++;
                sheet[ROW, COL].Text = "Financial Implication :";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colFinancialImplication = COL;

                sheet.Range[StartRow, colDocDate, StartRow, colDocDate + 1].Merge();
                sheet.Range[StartRow+1, colEntity, StartRow+1, colFinancialImplication + 1].Merge();
                sheet.Range[StartRow+2, colFinancialImplication, StartRow+2, colFinancialImplication + 1].Merge();
                

                // Headerdata
                ROW = 6;
                sheet[ROW, colPostingDate + 1].Text = dtHeader.Rows[0]["PostingDate"].ToString();
                ROW++;

                sheet[ROW, colDocRef + 1].Text = dtHeader.Rows[0]["DocRefNo"].ToString();
                ROW++;

                sheet[ROW, colStorageLocation + 1].Text = dtHeader.Rows[0]["MaterialStorage"].ToString();
                ROW++;

                sheet[ROW, colNarration + 1].Text = dtHeader.Rows[0]["Narration"].ToString();
                ROW = StartRow;

                sheet[ROW, colDocDate + 2].Text = dtHeader.Rows[0]["DocDate"].ToString();
                ROW++;

                sheet[ROW, colEntity + 2].Text = dtHeader.Rows[0]["EntityName"].ToString();
                ROW++;

                sheet[ROW, colFinancialImplication + 2].Text = dtHeader.Rows[0]["FinancialImplication"].ToString();
                ROW = StartRow;
            

                sheet.Range[StartRow, colPostingDate + 1, StartRow, colPostingDate + 4].Merge();
                sheet.Range[StartRow+1, colDocRef + 1, StartRow+1, colDocRef + 4].Merge();
                sheet.Range[StartRow+2, colStorageLocation + 1, StartRow+2, colStorageLocation + 4].Merge();
                sheet.Range[StartRow+3, colNarration +1, StartRow+3, colNarration + 4].Merge();
                sheet.Range[StartRow, colDocDate + 2, StartRow, colDocDate + 6].Merge();
                sheet.Range[StartRow + 1, colEntity + 2, StartRow + 1, colEntity + 6].Merge();
                sheet.Range[StartRow + 2, colFinancialImplication + 2, StartRow+2, colFinancialImplication + 6].Merge();


                sheet.Range[StartRow, colPostingDate, StartRow+4, colFinancialImplication + 6].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(232, 244, 248);

                ROW = 11;
                COL = 1;
                #endregion
                sheet[ROW, COL].Text = "Material Master";
                sheet[ROW, COL].ColumnWidth = 25;
                int colMaterialMaster = COL;
                COL++;
                sheet[ROW, COL].Text = "Article Name";
                sheet[ROW, COL].ColumnWidth = 25;
                int colArticleName = COL;
                COL++;
                sheet[ROW, COL].Text = "Sku1";
                sheet[ROW, COL].ColumnWidth = 11;
                int colSku1 = COL;
                COL++;
                sheet[ROW, COL].Text = "Sku2";
                sheet[ROW, COL].ColumnWidth = 11;
                int colSku2 = COL;
                COL++;
                sheet[ROW, COL].Text = "Sku3";
                sheet[ROW, COL].ColumnWidth = 11;
                int colSku3 = COL;
                COL++;
                sheet[ROW, COL].Text = "Quantity";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colQuantity = COL;
                COL++;
                sheet[ROW, COL].Text = "UoM";
                sheet[ROW, COL].ColumnWidth = 7;
                int colBaseUoM = COL;
                
                COL++;
                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colAmount = COL;
                COL++;
                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 8;
                int colCurrency = COL;
                COL++;
                sheet[ROW, COL].Text = "Lot Number";
                sheet[ROW, COL].ColumnWidth = 12;
                int colLotNumber = COL;
                COL++;
                sheet[ROW, COL].Text = "Diameter";
                sheet[ROW, COL].ColumnWidth = 12;
                int colDiameter = COL;
                COL++;
                sheet[ROW, COL].Text = "Type";
                sheet[ROW, COL].ColumnWidth = 8;
                int colType = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Total Amount";
                //sheet[ROW, COL].ColumnWidth = 20;
                //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int colTotalAmount = COL;

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                StartRow = ROW; //row 20
                for (int i = 0; i < dtOpenningBalance.Rows.Count; i++)
                {
                    sheet[ROW, colMaterialMaster].Text = dtOpenningBalance.Rows[i]["MaterialMasterName"].ToString();
                    sheet[ROW, colArticleName].Text = dtOpenningBalance.Rows[i]["ArticleName"].ToString();
                    sheet[ROW, colSku1].Text = dtOpenningBalance.Rows[i]["FirstCharacteristicsValue"].ToString();
                    sheet[ROW, colSku2].Text = dtOpenningBalance.Rows[i]["SecondCharacteristicsValue"].ToString();
                    sheet[ROW, colSku3].Text = dtOpenningBalance.Rows[i]["ThirdCharacteristicsValue"].ToString();

                    sheet[ROW, colQuantity].Number = clsStaticInfo.dbl(dtOpenningBalance.Rows[i]["Quantity"].ToString());
                    sheet[ROW, colQuantity].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtOpenningBalance.Rows[i]["FACompanyCurrencyAmount"].ToString());
                    sheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[ROW, colCurrency].Text = dtOpenningBalance.Rows[i]["CompanyCurrency"].ToString();
                    sheet[ROW, colDiameter].Text = dtOpenningBalance.Rows[i]["Diameter"].ToString();
                    sheet[ROW, colLotNumber].Text = dtOpenningBalance.Rows[i]["LotNumber"].ToString();
                    sheet[ROW, colType].Text = dtOpenningBalance.Rows[i]["Type"].ToString();
                    sheet[ROW, colBaseUoM].Text = dtOpenningBalance.Rows[i]["BaseUoM"].ToString();
                    


                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;
                }
                sheet[ROW, 1].Text = "Total :";
                sheet[ROW, 1].CellStyle.Font.Bold = true;
                int colTotal = colAmount;

                sheet.Range[ROW, colTotal].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colAmount) + StartRow + ":" + reportUtility.GetColumnNameForXls(colAmount) + (ROW - 1) + ")";
                //sheet.Range[ROW, colTotal].NumberFormat = clsStaticInfo.NumberFormat(2);
                //sheet[ROW, colTotal].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[ROW, 1, ROW, colTotal - 1].Merge();

                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);


                sheet.IsGridLinesVisible = false;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet[ROW, 1].CellStyle.Font.Size = 9;

                // sheet["A" + StartRow.ToString()].FreezePanes();


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                reportUtility.PlantHeader(ref sheet, endCol, "Material Master Openning Balance Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet[ROW, colTotal].HorizontalAlignment = ExcelHAlign.HAlignRight;


                string strFileName = "MaterialMasterOpenningBalanceReport.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        #endregion Opening Balance Report by Aakash 
    }
}