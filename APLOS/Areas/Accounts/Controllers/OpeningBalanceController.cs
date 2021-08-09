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
using Library.Service.Enums;
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

    }
}