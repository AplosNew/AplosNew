using Aplos.Controllers;
using Aplos.Helpers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Advances;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Model.Vouchers;
using Library.Service.Advances;
using Library.Service.Calendars;
using Library.Service.Core;
using Library.Service.Currencies;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.Service.Taxations;
using Library.Service.Vouchers;
using Library.ViewModel.Vouchers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Accounts.Controllers
{
    public class VoucherController : BaseController
    {
        private readonly IVoucherService _voucharService;
        private readonly IVoucherReportService _voucharReportService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IAdvanceService _advanceService;
        private readonly IEmployeePayableService _employeePayableService;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IFiscalYearService _fiscalYearService;
        private readonly ICompanyFiscalYearService _companyFiscalYearService;
        private readonly ICompanyTaxYearService _companyTaxYearService;
        private readonly AccountVoucherReportService _accountVoucherReportService;
        private readonly IRepositoryAsync<EmployeeSalaryAdvance> _employeeSalaryAdvanceRepository;

        public VoucherController(
               IPKGeneratorService pkGeneratorService
              , IUnitOfWork unitOfWork
             , ISqlRepository sqlRepository
             , IVoucherService voucharService
             , IVoucherReportService voucharReportService
             , ICompanyParallelCurrencyService companyParallelCurrencyService
             , IAdvanceService advanceService
             , IEmployeePayableService employeePayableService
             , IFiscalYearService fiscalYearService
             , ICompanyTaxYearService companyTaxYearService
             , ICompanyFiscalYearService companyFiscalYearService
             , IRepositoryAsync<EmployeeSalaryAdvance> employeeSalaryAdvanceRepository
             , AccountVoucherReportService accountVoucherReportService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _voucharService = voucharService;
            _voucharReportService = voucharReportService;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _advanceService = advanceService;
            _employeePayableService = employeePayableService;
            _pkGeneratorService = pkGeneratorService;
            _fiscalYearService = fiscalYearService;
            _companyTaxYearService = companyTaxYearService;
            _companyFiscalYearService = companyFiscalYearService;
            _employeeSalaryAdvanceRepository = employeeSalaryAdvanceRepository;
            _accountVoucherReportService = accountVoucherReportService;
        }

        #region Journal

        [HttpGet]
        public ActionResult Journal()
        {
            return View("~/Areas/Accounts/Views/Journal.cshtml");
        }

        [HttpPost]
        public JsonResult ParkJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
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
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _voucharService.InsertVoucher(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult UpdateJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            if (voucherDetailVMList == null)
                throw new CustomException("Please Add GL.");
            if (voucherDetailVMList.Sum(r => r.DrAmount) != voucherDetailVMList.Sum(r => r.CrAmount))
                throw new CustomException("Dr Cr not match!");
            foreach (var item in voucherDetailVMList)
            {
                if ((item.DrAmount + item.CrAmount == 0) || (item.DrAmount + item.CrAmount < 0))
                    throw new CustomException("Please input amount !");
            }
            voucherVM.IsPark = true;
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _voucharService.UpdateVoucher(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult PostJournal(string id)
        {
            _voucharService.PostJournalVoucher(id);
            return Json(new { Message = AplosMessage.Posted });
        }


        public JsonResult GetJournalVoucherList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucharService.GetJournalVoucherList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetJournalVoucherDetailList(string voucherId)
        {
            return Json(_voucharService.GetJournalVoucherDetailList(voucherId), JsonRequestBehavior.AllowGet);
        }

       
        [HttpPost]
        public ActionResult DeleteJV( string voucherId)
        {
            _voucharService.DeleteJV(voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetJournalVoucherReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _voucharReportService.GetGeneralVoucher(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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

        #region AdvanceJournal


        public ActionResult AdvanceJournal()
        {
            return View("~/Areas/Accounts/Views/AdvanceJournal.cshtml");
        }


        public ActionResult NormalJournal()
        {
            return View("~/Areas/Accounts/Views/NormalJournal.cshtml");
        }
        public ActionResult PFESICDisbursement()
        {
            return View("~/Areas/Accounts/Views/PFESICDisbursement.cshtml");
        }

        [HttpPost]
        public JsonResult ParkPFESICDisbursement(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.PFESICDisbursement.ToString();
            if (voucherVM.CompanyCurrencyRate == 0)
                throw new CustomException("Rate can not Empty!");
            if (voucherVM.CurrencyId == null)
                throw new CustomException("Please Select Currency!");
            if (voucherDetailVMList == null)
                throw new CustomException("Please Add Item.");
            if (voucherDetailVMList.Sum(r => r.DrAmount) != voucherDetailVMList.Sum(r => r.CrAmount))
                throw new CustomException("Dr Cr not match!");

            foreach (var item in voucherDetailVMList)
            {
                if (item.PartyType != "Director" && item.PartyId != null && item.PartyPlantId == null)
                    throw new CustomException("Please select Location!");
                if ((item.DrAmount + item.CrAmount == 0) || (item.DrAmount + item.CrAmount < 0))
                    throw new CustomException("Please input amount !");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _voucharService.InsertAdvanceJournal(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult ParkAdvanceJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.SourceType = SourceType.AdvanceJournalVoucher.ToString();
            if (voucherVM.CompanyCurrencyRate == 0)
                throw new CustomException("Rate can not Empty!");
            if (voucherVM.CurrencyId == null)
                throw new CustomException("Please Select Currency!");
            if (voucherDetailVMList == null)
                throw new CustomException("Please Add Item.");
            if (voucherDetailVMList.Sum(r => r.DrAmount) != voucherDetailVMList.Sum(r => r.CrAmount))
                throw new CustomException("Dr Cr not match!");

            foreach (var item in voucherDetailVMList)
            {
                if (item.PartyType != "Director" && item.PartyId != null && item.PartyPlantId == null)
                    throw new CustomException("Please select Location!");
                if ((item.DrAmount + item.CrAmount == 0) || (item.DrAmount + item.CrAmount < 0))
                    throw new CustomException("Please input amount !");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _voucharService.InsertAdvanceJournal(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult UpdateAdvanceJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            if (voucherDetailVMList.Sum(r => r.DrAmount) != voucherDetailVMList.Sum(r => r.CrAmount))
                throw new CustomException("Dr Cr not match!");
            foreach (var item in voucherDetailVMList)
            {
                if (item.PartyType != "Director" && item.PartyId != null && item.PartyPlantId == null)
                    throw new CustomException("Please select Location!");
                if ((item.DrAmount + item.CrAmount == 0) || (item.DrAmount + item.CrAmount < 0))
                    throw new CustomException("Please input amount !");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _voucharService.UpdateAdvanceJournal(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult DeleteVoucherDetail(string Id, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _voucharService.DeleteVoucherDetail(Id, voucherId, identity.PlantId);
            return Json(new { Message = AplosMessage.Updated });
        }

       

        [HttpPost]
        public JsonResult PostAdvanceJournal(string id)
        {
            _voucharService.PostJournalVoucher(id);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public JsonResult PostPFESICDisbursement(string id)
        {
            _voucharService.PostJournalVoucher(id);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpGet, Authorize]
        public JsonResult GetAdvanceJournalList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucharService.GetAdvanceJournalVoucherList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPFESICDisbursementList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            FiscalYearCloseService _fiscalYearCloseService = new FiscalYearCloseService(_sqlRepository);
            return Json(_fiscalYearCloseService.GetPFESICDisbursementList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        public void DeletePFESICDisbursement(string companyId, string plantId, string voucherId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucharService.FindVoucher(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";

                vendorAdWrsql = @"delete from trn.GLTransactionDetail where VoucherDetailId in (select Id from TRN.VoucherDetail  where VoucherId in (select Id from TRN.Voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.PFESICDisbursement.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.VoucherDetailCurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.PFESICDisbursement.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.VoucherDetail where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.PFESICDisbursement.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.voucher  where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.PFESICDisbursement.ToString() + "' AND Id = '" + voucherId + "'";
                vendorAdWr.Append(vendorAdWrsql);
                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        [HttpPost]
        public JsonResult DeletePFESICDisbursement(string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            DeletePFESICDisbursement(identity.CompanyId, identity.PlantId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public JsonResult GetAdvanceJournalVoucherDetailList(GridParameter parameters, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_voucharService.GetAdvanceJournalDetail(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, voucherId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAdvanceJournalVoucherReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountVoucherReportService.GetAdvanceJournalVoucherReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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
        public ActionResult GetAdvanceJournalVoucherReport1(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountVoucherReportService.GetAdvanceJournalVoucherReport1(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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
        public ActionResult GetDashBoardJournalVoucherReport(ReportFormat reportFormat, string CompanyGroupId, string CompanyId, string PlantId, string voucherId)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountVoucherReportService.GetDashboardJournalVoucherReport(out string reportFileName, CompanyGroupId, CompanyId, PlantId, voucherId);
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



        #endregion AdvanceJournal

        #region ExchangeVoucher


        public ActionResult ExchangeVoucher()
        {
            return View("~/Areas/Accounts/Views/ExchangeVoucher.cshtml");
        }

        [HttpPost]
        public JsonResult ParkExchangeVoucher(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            foreach (var advanceDetailVM in voucherDetailVMList)
            {
                if (advanceDetailVM.PartyPlantId == null)
                    throw new CustomException(" Please select Party Plant !");
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _voucharService.InsertExchangeLossGain(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult UpdateExchangeVoucher(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            return Json(new { Message = string.Format(AplosMessage.VoucherUpdate, _voucharService.UpdateExchangeLossGain(voucherVM, voucherDetailVMList)) });
        }

        [HttpPost]
        public JsonResult PostExchangeJournal(string id)
        {
            _voucharService.PostJournalVoucher(id);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpGet]
        public JsonResult GetExchangeVoucherList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_voucharService.GetExchangeVoucherList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetExchangeVoucherDetailList(string voucherId)
        {
            return Json(_voucharService.GetExchangeVoucherDetailList(voucherId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetExchangeVoucherReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _voucharReportService.GetExchangeVoucher(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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

        #endregion ExchangeVoucher


       
        [Authorize]
        public ActionResult IntSalesOrderInvoice()
        {
            return View("~/Areas/Accounts/Views/IntSalesOrderInvoice.cshtml");
        }

        [Authorize]
        public ActionResult IntSalesOrderInvoiceEdit()
        {
            return View("~/Areas/Accounts/Views/IntSalesOrderInvoiceEdit.cshtml");
        }

        [Authorize]
        public ActionResult IntSalesOrderInvoicePost()
        {
            return View("~/Areas/Accounts/Views/IntSalesOrderInvoicePost.cshtml");
        }


        public ActionResult IncomeStatementReportPage()
        {
            return View("~/Areas/Accounts/Views/IncomeStatementReportPage.cshtml");
        }


        public ActionResult BalanceSheetOpeningBalanceReport()
        {
            return View("~/Areas/Accounts/Views/BalanceSheetOpeningBalanceReport.cshtml");
        }


        public ActionResult BalanceSheetDetailsReportPage()
        {
            return View("~/Areas/Accounts/Views/BalanceSheetDetailsReportPage.cshtml");
        }

        public ActionResult FixedAssetObReport()
        {
            return View("~/Areas/Accounts/Views/FixedAssetObReport.cshtml");
        }

        public ActionResult DailyTransactionReportPage()
        {
            return View("~/Areas/Accounts/Views/DailyTransactionReportPage.cshtml");
        }

        public ActionResult BalanceSheetReportGroupWise()
        {
            return View("~/Areas/Accounts/Views/BalanceSheetReportGroupWise.cshtml");
        }


        public ActionResult TrialBalanceReportGroupWise()
        {
            return View("~/Areas/Accounts/Views/TrialBalanceReportGroupWise.cshtml");
        }

        //public ActionResult PartyPaymentStatus()
        //{
        //    return View("~/Areas/Accounts/Views/PartyPaymentStatus.cshtml");  EntityWiseExpenseAndEarning
        //}

        public ActionResult EntityWiseExpenseAndEarning()
        {
            return View("~/Areas/Accounts/Views/EntityWiseExpenseAndEarning.cshtml");
        }


        //PartyPaymentStatusDetail
        [HttpGet, Authorize]
        public ActionResult PartyPaymentStatusDetail()
        {
            return View("~/Areas/Accounts/Views/PartyPaymentStatusDetail.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetVoucharById(string id)
        {
            return Json(_voucharService.FindVoucher(id), JsonRequestBehavior.AllowGet);
        }



        [HttpGet, Authorize]
        public JsonResult GetCompanyCurrencyRate(string voucherId)
        {
            return Json(_voucharService.GetCompanyCurrencyRate(voucherId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get voucher list for check printing. (where bank is not null and checkDetail is null)
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>

        //

        [HttpGet, Authorize]
        public JsonResult getvoucherlistforcheckprinting(GridParameter parameters)
        
        
        {
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            return Json(accountsCommonService.Getvoucherlistforcheckprinting(parameters), JsonRequestBehavior.AllowGet);
        }



        [HttpGet, Authorize]
        public JsonResult GetVoucherListForCashCheckPrinting(GridParameter parameters)
        {
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            return Json(accountsCommonService.GetVoucherListForCashCheckPrinting(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetVoucherListForCheckVoidPrinting(GridParameter parameters)
        {
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            return Json(accountsCommonService.GetVoucherListForCheckVoidPrinting(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult getvoucherlistforcheckReprinting(GridParameter parameters)
        {
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            return Json(accountsCommonService.getvoucherlistforcheckReprinting(parameters), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult getvoucherlistforCashchequeReprinting(GridParameter parameters)
        {
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            return Json(accountsCommonService.getvoucherlistforCashchequeReprinting(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GenerateGLVoucher(string masterid)
        {
            var workbook = _voucharReportService.GetGLVoucher(out ExcelEngine excelEngine, masterid);
            return excelEngine.SaveAsActionResult(workbook, "GLVoucher_" + masterid + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog, ExcelHttpContentType.Excel2013);
        }

        [HttpPost]
        public ActionResult GenerateGLDateWise(string masterid, string fromDate, string toDate)
        {
            var workbook = _voucharReportService.GetGLDateWise(out ExcelEngine excelEngine, masterid, fromDate, toDate);
            return excelEngine.SaveAsActionResult(workbook, "GL_" + masterid + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog, ExcelHttpContentType.Excel2013);
        }


        public ActionResult GeneralLedgerReport()
        {
            return View("~/Areas/Accounts/Views/GeneralLedgerReport.cshtml");
        }
        public ActionResult LCLedgerReport()
        {
            return View("~/Areas/Accounts/Views/LCLedgerReport.cshtml");
        }
        public ActionResult GSTLedgerReport()
        {
            return View("~/Areas/Accounts/Views/GSTLedgerReport.cshtml");
        }

        //General ledger report
        [HttpGet, Authorize]
        public ActionResult GetGeneralLedgerReport(ReportFormat reportFormat, string glId, string budgetMasterId, string activityId, string fromDate, string toDate,bool active,bool IsGroupBy)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            if (active)
            {

                IWorkbook workbook = null;
                if (IsGroupBy == true && activityId == null)
                {
                    workbook = _accountVoucherReportService.GetGeneralLedgerGroupByReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, glId, budgetMasterId, activityId, fromDate, toDate, active, IsGroupBy);
                }
                else if (IsGroupBy == true && activityId != null)
                {
                    workbook = _accountVoucherReportService.GetGeneralLedgerReportWithDocRef(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, glId, budgetMasterId, activityId, fromDate, toDate, active);
                }
                else
                {
                    workbook = _accountVoucherReportService.GetGeneralLedgerReportWithDocRef(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, glId, budgetMasterId, activityId, fromDate, toDate, active);
                }
               // var workbook = _accountVoucherReportService.GetGeneralLedgerReportWithDocRef(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, glId, budgetMasterId, activityId, fromDate, toDate,active);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + " General Ledger";
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
            else
            {
                IWorkbook workbook = null;
                if (IsGroupBy==true && activityId == null)
                {
                    workbook = _accountVoucherReportService.GetGeneralLedgerGroupByReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, glId, budgetMasterId, activityId, fromDate, toDate, active, IsGroupBy);
                }
                else if(IsGroupBy == true && activityId != null)
                {
                    workbook = _accountVoucherReportService.GetGeneralLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, glId, budgetMasterId, activityId, fromDate, toDate, active);
                }
                else
                {
                    workbook = _accountVoucherReportService.GetGeneralLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, glId, budgetMasterId, activityId, fromDate, toDate, active);
                }

                var reportFileName = DateTime.Now.ToString("yyMMdd") + " General Ledger";
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
            
        }
        //General ledger report
        //General LC report
        [HttpGet, Authorize]
        public ActionResult GetLCLedgerReport(ReportFormat reportFormat, string fromDate, string toDate, string lCRef)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = null;
            workbook = _accountVoucherReportService.GetLCLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, lCRef);

            var reportFileName = DateTime.Now.ToString("yyMMdd") + " LC Ledger";
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
        //General LC report
        [HttpGet, Authorize]
        public ActionResult GetGeneralLedgerGSTReport(ReportFormat reportFormat, string glId, string budgetMasterId, string activityId, string fromDate, string toDate, bool active, bool IsGroupBy)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = null;
               
                workbook = _accountVoucherReportService.GetGeneralLedgerGSTReportWithDocRef(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, glId, budgetMasterId, activityId, fromDate, toDate, active);
                
                var reportFileName = DateTime.Now.ToString("yyMMdd") + " GST Ledger";
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

        //public ActionResult GetGeneralLedgerReportWithDocRef(ReportFormat reportFormat, string glId, string budgetMasterId, string activityId, string fromDate, string toDate)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    var workbook = _accountVoucherReportService.GetGeneralLedgerReportWithDocRef(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, glId, budgetMasterId, activityId, fromDate, toDate);
        //    var reportFileName = DateTime.Now.ToString("yyMMdd") + " General Ledger";
        //    switch (reportFormat)
        //    {
        //        case ReportFormat.Pdf:
        //            return RenderReportAsPdf(workbook, reportFileName);

        //        case ReportFormat.Excel:
        //            return RenderReportAsExcel(workbook, reportFileName);

        //        default:
        //            return RenderReportAsExcel(workbook, reportFileName);
        //    }
        //}

        public ActionResult GeneralLedgerOpeningBalanceReport()
        {
            return View("~/Areas/Accounts/Views/GeneralLedgerOpeningBalanceReport.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetGeneralLedgerOpeningBalanceReport(ReportFormat reportFormat, string fiscalYearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountVoucherReportService.GetGeneralOpeningBalanceLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fiscalYearId, true);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " General Opening Balance Ledger";
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
        #region Trail Balance

        public ActionResult TrialBalanceReportPage()
        {
            return View("~/Areas/Accounts/Views/TrialBalanceReportPage.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult TrialBalanceReportCompanyLevel(ReportFormat reportFormat, string date, bool isBudgetLevel, bool isActivityLevel, bool isDetailLevel)
        {
            AccountsTrialBalanceService accountsTrialBalanceService = new AccountsTrialBalanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = accountsTrialBalanceService.GetTrialBalanceReport(identity.CompanyId, date, isBudgetLevel, isActivityLevel, isDetailLevel);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Trial Balance Sheet";
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
        public ActionResult TrialBalanceReport(ReportFormat reportFormat, string date, bool isBudgetLevel, bool isActivityLevel,bool isDetailLevel,string partyId,string partyPlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = GetTrialBalanceReport(identity.CompanyId, identity.PlantId, identity.PlantName, date, isBudgetLevel, isActivityLevel, isDetailLevel, partyId, partyPlantId);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Trial Balance Sheet";
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

       
        public IWorkbook GetTrialBalanceReport(string companyId, string plantId, string plantName, string toDate, bool isBudgetLevel, bool isActivityLevel, bool isDetailLevel, string partyId, string partyPlantId)
        {
            var excelEngine = new ExcelEngine();
            var oRU = new ReportUtility();
            var dsLocal = GetTrialBalanceInfo(companyId, plantId, toDate, isBudgetLevel, isActivityLevel, isDetailLevel, partyId, partyPlantId);
            var workbook = oRU.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            var dtLocal = dsLocal.Tables[0];
            if (dtLocal.Rows.Count > 0)
            {
                var dvParallelCurrency = new DataView(dsLocal.Tables[0])
                {
                    Sort = "CurrencyCode ASC"
                };
                var dtParallelCurrency = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");

                var dvMainBody = new DataView(dsLocal.Tables[0])
                {
                    Sort = "GLGeneralInfoCode"
                };
                var dtMainBody = dvMainBody.ToTable();

                var col = 1;
                var shet2EndxlsCol = col;

                var row = 6;
                row++;
                var headreColIndex = 1;
                var mainColIndex = 1;

                oRU.SetHeaderText(ref sheet, row, headreColIndex, "GL", 32);
                headreColIndex++;
                if (isBudgetLevel)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Budget Name", 32);
                    headreColIndex++;
                }
                if (isActivityLevel)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Budget Name", 32);
                    headreColIndex++;

                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Activity Name", 32);
                    headreColIndex++;

            
                }
                if(isDetailLevel)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Budget Name", 32);
                    headreColIndex++;

                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Activity Name", 32);
                    headreColIndex++;
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Particulars", 32);
                    headreColIndex++;


                }
                var colSum = headreColIndex - 1;
                int colCurrencyIndex = headreColIndex;
                var plCurrencyId = string.Empty;
                var plCurrencyCode = string.Empty;

                var alParaCurrency = new ArrayList();

                for (int n = 0; n < dtParallelCurrency.Rows.Count; n++)
                {
                    oRU.SetHeaderText(ref sheet, row - 1, headreColIndex, dtParallelCurrency.Rows[n]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                    sheet[row - 1, headreColIndex, row - 1, headreColIndex + 1].Merge();
                    var dic = new Dictionary<string, int>
                {
                    { dtParallelCurrency.Rows[n]["ParallelCurrencyId"].ToString(), headreColIndex }
                };
                    alParaCurrency.Add(dic);

                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Dr", ExcelHAlign.HAlignRight); headreColIndex++;
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Cr", ExcelHAlign.HAlignRight); //headreColIndex++;

                    if (n == 0)
                    {
                        plCurrencyCode = dtParallelCurrency.Rows[n]["CurrencyCode"].ToString();
                    }

                    sheet.Range[row - 1, colCurrencyIndex, row - 1, headreColIndex].BorderAround(ExcelLineStyle.Hair);
                }
                shet2EndxlsCol = headreColIndex - 1;

                var drcrCol = 0;
                var Row_Total_Start = row + 1;

                if (isActivityLevel)
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        row++;
                        var AccountCodeId = dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString();
                        var BudgetMasterId = dtMainBody.Rows[n]["BudgetMasterId"].ToString();
                        var ActivityId = dtMainBody.Rows[n]["ActivityId"].ToString();
                        //var BankMasterId = dtMainBody.Rows[n]["BankMasterId"].ToString();
                      
                        var Balancetype = dtMainBody.Rows[n]["Balancetype"].ToString();

                        mainColIndex = 1;

                        oRU.SetText(ref sheet, row, mainColIndex, AccountCodeId + " - " + dtMainBody.Rows[n]["GL"]); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Budget"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Activity"].ToString()); mainColIndex++;
                        // oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Particulars"].ToString()); mainColIndex++;

                        for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                        {
                            var ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();



                            var dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ISNULL(ParallelCurrencyId,'')='" + ParallelCurrencyId + "' AND ISNULL(GLGeneralInfoCode,'')='" + AccountCodeId + "' AND ISNULL(BudgetMasterId,'')='" + BudgetMasterId + "' AND ISNULL(ActivityId,'')='" + ActivityId + "'"
                            };
                          var dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count != 0)
                            {
                                var _drPC = Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                var _crPC = Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                if (_drPC < 0)
                                {
                                    _crPC += _drPC * -1;
                                    _drPC = 0.00;
                                }
                                if (_crPC < 0)
                                {
                                    _drPC += _crPC * -1;
                                    _crPC = 0.00;
                                }
                                oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                            }
                        
                        }
                    }

                }
                else if (isBudgetLevel)
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        row++;
                        var AccountCodeId = dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString();
                        var BudgetMasterId = dtMainBody.Rows[n]["BudgetMasterId"].ToString();
                        var _Balancetype = dtMainBody.Rows[n]["Balancetype"].ToString();
                        mainColIndex = 1;
                        oRU.SetText(ref sheet, row, mainColIndex, AccountCodeId + " - " + dtMainBody.Rows[n]["GL"]); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Budget"].ToString()); mainColIndex++;

                        for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                        {
                            var ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();

                            var dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ISNULL(ParallelCurrencyId,'')='" + ParallelCurrencyId + "' AND ISNULL(GLGeneralInfoCode,'')='" + AccountCodeId + "' AND ISNULL(BudgetMasterId,'')='" + BudgetMasterId + "'"
                            };

                            var dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count != 0)
                            {
                                var _drPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                var _crPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                if (_drPC < 0)
                                {
                                    _crPC += _drPC * -1;
                                    _drPC = 0.00;
                                }
                                if (_crPC < 0)
                                {
                                    _drPC += _crPC * -1;
                                    _crPC = 0.00;
                                }
                                oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                            }
                        }
                    }
                }
                else if(isDetailLevel)
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        //if(dtMainBody.Rows[n]["Activity"].ToString() == "Issued Share Capital")
                        //{

                        //}

                        row++;
                        var AccountCodeId = dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString();
                        var BudgetMasterId = dtMainBody.Rows[n]["BudgetMasterId"].ToString();
                        var ActivityId = dtMainBody.Rows[n]["ActivityId"].ToString();
                        var BankMasterId = dtMainBody.Rows[n]["BankMasterId"].ToString();
                        var CashMasterId = dtMainBody.Rows[n]["CashMasterId"].ToString();
                        var PartyId = dtMainBody.Rows[n]["PartyId"].ToString();
                        //var PartyPlantId = dtMainBody.Rows[n]["PartyPlantId"].ToString();
                        var Balancetype = dtMainBody.Rows[n]["Balancetype"].ToString();
                        
                        mainColIndex = 1;

                        oRU.SetText(ref sheet, row, mainColIndex, AccountCodeId + " - " + dtMainBody.Rows[n]["GL"]); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Budget"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Activity"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Particulars"].ToString()); mainColIndex++;

                        for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                        {
                            var ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();
                            if (!string.IsNullOrEmpty(BankMasterId))
                            {
                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ISNULL(ParallelCurrencyId,'')='" + ParallelCurrencyId + "' AND ISNULL(GLGeneralInfoCode,'')='" + AccountCodeId + "' AND ISNULL(BudgetMasterId,'')='" + BudgetMasterId + "' AND ISNULL(ActivityId,'')='" + ActivityId + "' AND ISNULL(BankMasterId,'') = '" + BankMasterId + "'"
                                };
                                var dtDrCr = dvDrCr.ToTable();
                                if (dtDrCr.Rows.Count != 0)
                                {
                                    var _drPC = Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                    var _crPC = Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                    if (_drPC < 0)
                                    {
                                        _crPC += _drPC * -1;
                                        _drPC = 0.00;
                                    }
                                    if (_crPC < 0)
                                    {
                                        _drPC += _crPC * -1;
                                        _crPC = 0.00;
                                    }

                                    oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                    oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                                }
                            }
                            else if (!string.IsNullOrEmpty(CashMasterId))
                            {
                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ISNULL(ParallelCurrencyId,'')='" + ParallelCurrencyId + "' AND ISNULL(GLGeneralInfoCode,'')='" + AccountCodeId + "' AND ISNULL(BudgetMasterId,'')='" + BudgetMasterId + "' AND ISNULL(ActivityId,'')='" + ActivityId + "'  AND ISNULL(CashMasterId,'') = '" + CashMasterId + "'"
                                };
                                var dtDrCr = dvDrCr.ToTable();
                                if (dtDrCr.Rows.Count != 0)
                                {
                                    var _drPC = Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                    var _crPC = Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                    if (_drPC < 0)
                                    {
                                        _crPC += _drPC * -1;
                                        _drPC = 0.00;
                                    }
                                    if (_crPC < 0)
                                    {
                                        _drPC += _crPC * -1;
                                        _crPC = 0.00;
                                    }
                                    oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                    oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                                }
                            }
                            else if (!string.IsNullOrEmpty(PartyId))
                            {
                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ISNULL(ParallelCurrencyId,'')='" + ParallelCurrencyId + "' AND ISNULL(GLGeneralInfoCode,'')='" + AccountCodeId + "' AND ISNULL(BudgetMasterId,'')='" + BudgetMasterId + "' AND ISNULL(ActivityId,'')='" + ActivityId + "'  AND ISNULL(PartyId,'') = '" + PartyId + "' " //AND ISNULL(PartyPlantId,'') = '" + PartyPlantId + "'
                                };
                                var dtDrCr = dvDrCr.ToTable();
                                if (dtDrCr.Rows.Count != 0)
                                {
                                    var _drPC = Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                    var _crPC = Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                    if (_drPC < 0)
                                    {
                                        _crPC += _drPC * -1;
                                        _drPC = 0.00;
                                    }
                                    if (_crPC < 0)
                                    {
                                        _drPC += _crPC * -1;
                                        _crPC = 0.00;
                                    }
                                    oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                    oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                                }
                            }
                            else
                            {
                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ISNULL(ParallelCurrencyId,'')='" + ParallelCurrencyId + "' AND ISNULL(GLGeneralInfoCode,'')='" + AccountCodeId + "' AND ISNULL(BudgetMasterId,'')='" + BudgetMasterId + "' AND ISNULL(ActivityId,'')='" + ActivityId + "' AND ISNULL(BankMasterId,'') = '' AND ISNULL(CashMasterId,'') = '' AND ISNULL(PartyId,'') = '' " //AND ISNULL(PartyPlantId,'') = ''
                                };
                                var dtDrCr = dvDrCr.ToTable();
                                if (dtDrCr.Rows.Count != 0)
                                {
                                    var _drPC = Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                    var _crPC = Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                    if (_drPC < 0)
                                    {
                                        _crPC += _drPC * -1;
                                        _drPC = 0.00;
                                    }
                                    if (_crPC < 0)
                                    {
                                        _drPC += _crPC * -1;
                                        _crPC = 0.00;
                                    }
                                    oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                    oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                                }
                            }
                        }
                    }

                }
                else
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        if (Convert.ToDouble(dtMainBody.Rows[n]["DRcumulative"].ToString()) + Convert.ToDouble(dtMainBody.Rows[n]["CRcumulative"].ToString()) != 0)
                        {
                            row++;
                            var AccountCodeId = dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString();
                            var _Balancetype = dtMainBody.Rows[n]["Balancetype"].ToString();
                            oRU.SetText(ref sheet, row, mainColIndex, AccountCodeId + " - " + dtMainBody.Rows[n]["GL"]);
                            mainColIndex++;
                            for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                            {
                                var ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();

                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ISNULL(ParallelCurrencyId,'')='" + ParallelCurrencyId + "' AND ISNULL(GLGeneralInfoCode,'')='" + AccountCodeId + "'"
                                };
                                var dtDrCr = dvDrCr.ToTable();
                                if (dtDrCr.Rows.Count != 0)
                                {
                                    drcrCol++;
                                    var _drPC = Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                    var _crPC = Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                    if (_drPC < 0)
                                    {
                                        _crPC += _drPC * -1;
                                        _drPC = 0.00;
                                    }
                                    if (_crPC < 0)
                                    {
                                        _drPC += _crPC * -1;
                                        _crPC = 0.00;
                                    }
                                    oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                    oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                                }
                            }
                            mainColIndex = 1;
                        }
                    }
                }

                row++;

                oRU.SetMasterHeaderText(ref sheet, row, colSum, "Total ");
                sheet.Range[oRU.GetColumnNameForXls(1) + row + ": " + oRU.GetColumnNameForXls(colSum) + row].Merge();

                var sumdrcrCol = colSum + 1;
                for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
                {
                    sheet.Range[row, sumdrcrCol].Formula = "=SUM(" + oRU.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(sumdrcrCol) + (row - 1) + ")";
                    sheet.Range[row, sumdrcrCol].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[row, sumdrcrCol].CellStyle.Font.Bold = true;
                    sheet.Range[row, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);

                    sumdrcrCol++;
                    sheet.Range[row, sumdrcrCol].Formula = "=SUM(" + oRU.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(sumdrcrCol) + (row - 1) + ")";
                    sheet.Range[row, sumdrcrCol].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[row, sumdrcrCol].CellStyle.Font.Bold = true;
                    sheet.Range[row, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
                }

                var colLast = sumdrcrCol;
                sheet.Range[8, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[8, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);

                sheet.Name = "Sheet";
                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                oRU.CompanyPlantHeader(ref sheet, colLast, "Trial Balance", companyId, plantId, plantName, null);
                oRU.SetText(ref sheet, 5, colLast, "As On " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(colLast) + 5].Merge();
                if (isActivityLevel)
                {
                    oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
                }
                else
                {
                    oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                }
            }
            else
            {
                sheet.Name = "Sheet";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyPlantHeader(ref sheet, 5, "Trial Balance", identity.CompanyId, plantId, plantName, null);
                oRU.SetText(ref sheet, 5, 3, "No Data Found", ExcelHAlign.HAlignCenter);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }
        [HttpGet, Authorize]
        public ActionResult DateRangeWiseTrialBalanceReport(ReportFormat reportFormat, string fromDate, string toDate, bool isBudgetLevel, bool isActivityLevel, bool isDetailLevel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = GetDateRangeWiseTrialBalanceReport(identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, isBudgetLevel, isActivityLevel, isDetailLevel);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Trial Balance Sheet";
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
        public ActionResult DateRangeWiseTrialBalanceReportCompanyLevel(ReportFormat reportFormat, string fromDate, string toDate, bool isBudgetLevel, bool isActivityLevel,bool isDetailLevel)
        {
            AccountsTrialBalanceService accountsTrialBalanceService = new AccountsTrialBalanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = accountsTrialBalanceService.GetDateRangeWiseTrialBalanceReport(identity.CompanyId, fromDate, toDate, isBudgetLevel, isActivityLevel,isDetailLevel);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Trial Balance Sheet";
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

        public IWorkbook GetDateRangeWiseTrialBalanceReport(string companyId, string plantId, string plantName, string fromDate, string toDate, bool isBudgetLevel, bool isActivityLevel,bool isDetailLevel)
        {
            var excelEngine = new ExcelEngine();
            var oRU = new ReportUtility();
            var dsLocal = GetDateRangeWiseTrialBalanceInfo(companyId, plantId, fromDate, toDate, isBudgetLevel, isActivityLevel,isDetailLevel);
            var workbook = oRU.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            var dtLocal = dsLocal.Tables[0];
            var obDebit = 0;
            var obCredit = 0;
            var Debit = 0;
            var Credit = 0;
            var cbDebit = 0;
            var cbCredit = 0;

            if (dtLocal.Rows.Count > 0)
            {
                var dvParallelCurrency = new DataView(dsLocal.Tables[0])
                {
                    Sort = "CurrencyCode ASC"
                };
                var dtParallelCurrency = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");

                var dvMainBody = new DataView(dsLocal.Tables[0])
                {
                    Sort = "GLGeneralInfoCode"
                };
                var dtMainBody = dvMainBody.ToTable();

                var col = 1;
                var shet2EndxlsCol = col;
                var colParticulers = 0;
                var row = 6;
                row++;
                var headreColIndex = 1;
                var mainColIndex = 1;

                oRU.SetHeaderText(ref sheet, row, headreColIndex, "GL", 32);
                headreColIndex++;
                if (isBudgetLevel)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Budget Name", 32);
                    headreColIndex++;
                }
                if (isActivityLevel)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Budget Name", 32);
                    headreColIndex++;

                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Activity Name", 35);
                
                    headreColIndex++;
                }
                if(isDetailLevel)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Budget Name", 32);
                    headreColIndex++;

                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Activity Name", 35);
                    headreColIndex++;

                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Particulars", 35);

                    colParticulers = headreColIndex;
                    headreColIndex++;


                }
                var colSum = headreColIndex - 1;
                var plCurrencyId = string.Empty;
                var plCurrencyCode = string.Empty;

                var alParaCurrency = new ArrayList();

                for (int n = 0; n < dtParallelCurrency.Rows.Count; n++)
                {

                    row++;
                    row++;

                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Dr", ExcelHAlign.HAlignRight); obDebit = headreColIndex; headreColIndex++;
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Cr", ExcelHAlign.HAlignRight); obCredit = headreColIndex; headreColIndex++;
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Dr", ExcelHAlign.HAlignRight); Debit = headreColIndex; headreColIndex++;
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Cr", ExcelHAlign.HAlignRight); Credit = headreColIndex; headreColIndex++;
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Dr", ExcelHAlign.HAlignRight); cbDebit = headreColIndex; headreColIndex++;
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Cr", ExcelHAlign.HAlignRight); cbCredit = headreColIndex; headreColIndex++;

                    oRU.SetHeaderText(ref sheet, row - 1, obDebit, "Openning Balance", ExcelHAlign.HAlignCenter);
                    sheet[row - 1, obDebit, row - 1, obCredit].Merge();
                    oRU.SetHeaderText(ref sheet, row - 1, Debit, "For the Period", ExcelHAlign.HAlignCenter);
                    sheet[row - 1, Debit, row - 1, Credit].Merge();
                    oRU.SetHeaderText(ref sheet, row - 1, cbDebit, "Closing Balance", ExcelHAlign.HAlignCenter);
                    sheet[row - 1, cbDebit, row - 1, cbCredit].Merge();


                    oRU.SetHeaderText(ref sheet, row - 2, obDebit, dtParallelCurrency.Rows[n]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                    sheet[row - 2, obDebit, row - 2, cbCredit].Merge();
                    var dic = new Dictionary<string, int>
                {
                    { dtParallelCurrency.Rows[n]["ParallelCurrencyId"].ToString(), headreColIndex }
                };
                    alParaCurrency.Add(dic);


                    if (n == 0)
                    {
                        plCurrencyCode = dtParallelCurrency.Rows[n]["CurrencyCode"].ToString();
                    }
                }
                shet2EndxlsCol = headreColIndex - 1;

                var drcrCol = 0;
                var Row_Total_Start = row + 1;
                if (isActivityLevel)
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        row++;
                        var AccountCodeId = dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString();
                        var BudgetMasterId = dtMainBody.Rows[n]["BudgetMasterId"].ToString();
                        var ActivityId = dtMainBody.Rows[n]["ActivityId"].ToString();
                       
                        var _Balancetype = dtMainBody.Rows[n]["Balancetype"].ToString();
                        mainColIndex = 1;

                        oRU.SetText(ref sheet, row, mainColIndex, AccountCodeId + " - " + dtMainBody.Rows[n]["GL"]); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Budget"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Activity"].ToString()); mainColIndex++;

                        //oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Particulars"].ToString()); mainColIndex++;
                        //if(dtMainBody.Rows[n]["Particulars"].ToString().Length >= 34)
                        //{
                        //    sheet.Range[row, colParticulers].RowHeight = 12.75 * 2;
                        //    sheet.Range[row, colParticulers].WrapText = true;

                        //    if (dtMainBody.Rows[n]["Particulars"].ToString().Length >= 70)
                        //    {
                        //        sheet.Range[row, colParticulers].RowHeight = 12.75 * 3;
                        //    }
                        //}
                        for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                        {
                            var ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();
                         
                          

                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "'" 

                                    // RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "'  AND PartyId = '" + PartyId + "' AND PartyPlantId = '" + PartyPlantId + "'"
                                };
                                var dtActDrCr = dvDrCr.ToTable();
                                if (dtActDrCr.Rows.Count != 0)
                                {
                                    for (int acp = 0; acp < dtParallelCurrency.Rows.Count; acp++)
                                    {
                                        var acpParallelCurrencyId = dtParallelCurrency.Rows[acp]["ParallelCurrencyId"].ToString();

                                        //var dvActDrCr = new DataView(dsLocal.Tables[0])
                                        //{
                                        //    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                                        //};
                                        //var dtActDrCr = dvActDrCr.ToTable();
                                        if (dtActDrCr.Rows.Count != 0)
                                        {
                                            drcrCol++;
                                            var _obDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBDRcumulative"].ToString());
                                            var _obCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBCRcumulative"].ToString());
                                            if (_obDrPC < 0)
                                            {
                                                _obCrPC += _obDrPC * -1;
                                                _obDrPC = 0.00;
                                            }
                                            if (_obCrPC < 0)
                                            {
                                                _obDrPC += _obCrPC * -1;
                                                _obCrPC = 0.00;
                                            }
                                            var _drPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["DRcumulative"].ToString());
                                            var _crPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CRcumulative"].ToString());
                                            if (_drPC < 0)
                                            {
                                                _crPC += _drPC * -1;
                                                _drPC = 0.00;
                                            }
                                            if (_crPC < 0)
                                            {
                                                _drPC += _crPC * -1;
                                                _crPC = 0.00;
                                            }
                                            var _cbDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBDRcumulative"].ToString());
                                            var _cbCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBCRcumulative"].ToString());
                                            if (_cbDrPC < 0)
                                            {
                                                _cbCrPC = _cbDrPC * -1;
                                                _cbDrPC = 0.00;
                                            }
                                            if (_cbCrPC < 0)
                                            {
                                                _cbDrPC = _cbCrPC * -1;
                                                _cbCrPC = 0.00;
                                            }
                                            oRU.SetText(ref sheet, row, obDebit, _obDrPC);
                                            oRU.SetText(ref sheet, row, obCredit, _obCrPC);
                                            oRU.SetText(ref sheet, row, Debit, _drPC);
                                            oRU.SetText(ref sheet, row, Credit, _crPC);
                                            oRU.SetText(ref sheet, row, cbDebit, _cbDrPC);
                                            oRU.SetText(ref sheet, row, cbCredit, _cbCrPC);
                                        }
                                    }
                                    //oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                    //oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                                }
                            
                        }
                    }
                }
                else if (isBudgetLevel)
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        row++;
                        var AccountCodeId = dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString();
                        var BudgetMasterId = dtMainBody.Rows[n]["BudgetMasterId"].ToString();
                        var _Balancetype = dtMainBody.Rows[n]["Balancetype"].ToString();
                        mainColIndex = 1;
                        oRU.SetText(ref sheet, row, mainColIndex, AccountCodeId + " - " + dtMainBody.Rows[n]["GL"]); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Budget"].ToString()); mainColIndex++;

                        for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                        {
                            var ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();

                            var dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                            };

                            var dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count != 0)
                            {
                                drcrCol++;
                                var _obDrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["OBDRcumulative"].ToString());
                                var _obCrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["OBCRcumulative"].ToString());
                                if (_obDrPC < 0)
                                {
                                    _obCrPC += _obDrPC * -1;
                                    _obDrPC = 0.00;
                                }
                                if (_obCrPC < 0)
                                {
                                    _obDrPC += _obCrPC * -1;
                                    _obCrPC = 0.00;
                                }
                                var _drPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                var _crPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                if (_drPC < 0)
                                {
                                    _crPC += _drPC * -1;
                                    _drPC = 0.00;
                                }
                                if (_crPC < 0)
                                {
                                    _drPC += _crPC * -1;
                                    _crPC = 0.00;
                                }
                                var _pDrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["PDRcumulative"].ToString());
                                var _pCrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["PCRcumulative"].ToString());
                                if (_pDrPC < 0)
                                {
                                    _pCrPC += _pDrPC * -1;
                                    _pDrPC = 0.00;
                                }
                                if (_pCrPC < 0)
                                {
                                    _pCrPC += _pCrPC * -1;
                                    _pCrPC = 0.00;
                                }
                                var _cbDrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["CBDRcumulative"].ToString());
                                var _cbCrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["CBCRcumulative"].ToString());
                                if (_cbDrPC < 0)
                                {
                                    _cbCrPC = _cbDrPC * -1;
                                    _cbDrPC = 0.00;
                                }
                                if (_cbCrPC < 0)
                                {
                                    _cbDrPC = _cbCrPC * -1;
                                    _cbCrPC = 0.00;
                                }
                                oRU.SetText(ref sheet, row, obDebit, _obDrPC);
                                oRU.SetText(ref sheet, row, obCredit, _obCrPC);
                                oRU.SetText(ref sheet, row, Debit, _pDrPC);
                                oRU.SetText(ref sheet, row, Credit, _pCrPC);
                                oRU.SetText(ref sheet, row, cbDebit, _cbDrPC);
                                oRU.SetText(ref sheet, row, cbCredit, _cbCrPC);
                            }
                        }
                    }
                }
                else if(isDetailLevel)
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        row++;
                        var AccountCodeId = dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString();
                        var BudgetMasterId = dtMainBody.Rows[n]["BudgetMasterId"].ToString();
                        var ActivityId = dtMainBody.Rows[n]["ActivityId"].ToString();
                        var BankMasterId = dtMainBody.Rows[n]["BankMasterId"].ToString();
                        var CashMasterId = dtMainBody.Rows[n]["CashMasterId"].ToString();
                        var PartyId = dtMainBody.Rows[n]["PartyId"].ToString();
                        var PartyPlantId = dtMainBody.Rows[n]["PartyPlantId"].ToString();
                        var _Balancetype = dtMainBody.Rows[n]["Balancetype"].ToString();
                        mainColIndex = 1;

                        oRU.SetText(ref sheet, row, mainColIndex, AccountCodeId + " - " + dtMainBody.Rows[n]["GL"]); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Budget"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Activity"].ToString()); mainColIndex++;

                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Particulars"].ToString()); mainColIndex++;
                        //if(dtMainBody.Rows[n]["Particulars"].ToString().Length >= 34)
                        //{
                        //    sheet.Range[row, colParticulers].RowHeight = 12.75 * 2;
                        //    sheet.Range[row, colParticulers].WrapText = true;

                        //    if (dtMainBody.Rows[n]["Particulars"].ToString().Length >= 70)
                        //    {
                        //        sheet.Range[row, colParticulers].RowHeight = 12.75 * 3;
                        //    }
                        //}
                        for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                        {
                            var ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();
                            if (!string.IsNullOrEmpty(BankMasterId))
                            {
                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ISNULL(ParallelCurrencyId,'')='" + ParallelCurrencyId + "' AND ISNULL(GLGeneralInfoCode,'')='" + AccountCodeId + "' AND ISNULL(BudgetMasterId,'')='" + BudgetMasterId + "' AND ISNULL(ActivityId,'')='" + ActivityId + "' AND ISNULL(BankMasterId,'') = '" + BankMasterId + "'"
                                };
                                var dtActDrCr = dvDrCr.ToTable();
                                if (dtActDrCr.Rows.Count != 0)
                                {
                                    for (int acp = 0; acp < dtParallelCurrency.Rows.Count; acp++)
                                    {
                                        var acpParallelCurrencyId = dtParallelCurrency.Rows[acp]["ParallelCurrencyId"].ToString();

                                        //var dvActDrCr = new DataView(dsLocal.Tables[0])
                                        //{
                                        //    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                                        //};
                                        //var dtActDrCr = dvActDrCr.ToTable();
                                        if (dtActDrCr.Rows.Count != 0)
                                        {
                                            drcrCol++;
                                            var _obDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBDRcumulative"].ToString());
                                            var _obCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBCRcumulative"].ToString());
                                            if (_obDrPC < 0)
                                            {
                                                _obCrPC += _obDrPC * -1;
                                                _obDrPC = 0.00;
                                            }
                                            if (_obCrPC < 0)
                                            {
                                                _obDrPC += _obCrPC * -1;
                                                _obCrPC = 0.00;
                                            }
                                            var _drPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["DRcumulative"].ToString());
                                            var _crPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CRcumulative"].ToString());
                                            if (_drPC < 0)
                                            {
                                                _crPC += _drPC * -1;
                                                _drPC = 0.00;
                                            }
                                            if (_crPC < 0)
                                            {
                                                _drPC += _crPC * -1;
                                                _crPC = 0.00;
                                            }
                                            var _pDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["PDRcumulative"].ToString());
                                            var _pCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["PCRcumulative"].ToString());
                                            if (_pDrPC < 0)
                                            {
                                                _pCrPC += _pDrPC * -1;
                                                _pDrPC = 0.00;
                                            }
                                            if (_pCrPC < 0)
                                            {
                                                _pCrPC += _pCrPC * -1;
                                                _pCrPC = 0.00;
                                            }
                                            var _cbDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBDRcumulative"].ToString());
                                            var _cbCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBCRcumulative"].ToString());
                                            if (_cbDrPC < 0)
                                            {
                                                _cbCrPC = _cbDrPC * -1;
                                                _cbDrPC = 0.00;
                                            }
                                            if (_cbCrPC < 0)
                                            {
                                                _cbDrPC = _cbCrPC * -1;
                                                _cbCrPC = 0.00;
                                            }
                                            oRU.SetText(ref sheet, row, obDebit, _obDrPC);
                                            oRU.SetText(ref sheet, row, obCredit, _obCrPC);
                                            oRU.SetText(ref sheet, row, Debit, _pDrPC);
                                            oRU.SetText(ref sheet, row, Credit, _pCrPC);
                                            oRU.SetText(ref sheet, row, cbDebit, _cbDrPC);
                                            oRU.SetText(ref sheet, row, cbCredit, _cbCrPC);
                                        }
                                    }
                                    //oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                    //oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                                }
                            }
                            else if (!string.IsNullOrEmpty(CashMasterId))
                            {
                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "'  AND CashMasterId = '" + CashMasterId + "'"
                                };
                                var dtActDrCr = dvDrCr.ToTable();
                                if (dtActDrCr.Rows.Count != 0)
                                {
                                    for (int acp = 0; acp < dtParallelCurrency.Rows.Count; acp++)
                                    {
                                        var acpParallelCurrencyId = dtParallelCurrency.Rows[acp]["ParallelCurrencyId"].ToString();

                                        //var dvActDrCr = new DataView(dsLocal.Tables[0])
                                        //{
                                        //    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                                        //};
                                        //var dtActDrCr = dvActDrCr.ToTable();
                                        if (dtActDrCr.Rows.Count != 0)
                                        {
                                            drcrCol++;
                                            var _obDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBDRcumulative"].ToString());
                                            var _obCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBCRcumulative"].ToString());
                                            if (_obDrPC < 0)
                                            {
                                                _obCrPC += _obDrPC * -1;
                                                _obDrPC = 0.00;
                                            }
                                            if (_obCrPC < 0)
                                            {
                                                _obDrPC += _obCrPC * -1;
                                                _obCrPC = 0.00;
                                            }
                                            var _drPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["DRcumulative"].ToString());
                                            var _crPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CRcumulative"].ToString());
                                            if (_drPC < 0)
                                            {
                                                _crPC += _drPC * -1;
                                                _drPC = 0.00;
                                            }
                                            if (_crPC < 0)
                                            {
                                                _drPC += _crPC * -1;
                                                _crPC = 0.00;
                                            }
                                            var _pDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["PDRcumulative"].ToString());
                                            var _pCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["PCRcumulative"].ToString());
                                            if (_pDrPC < 0)
                                            {
                                                _pCrPC += _pDrPC * -1;
                                                _pDrPC = 0.00;
                                            }
                                            if (_pCrPC < 0)
                                            {
                                                _pCrPC += _pCrPC * -1;
                                                _pCrPC = 0.00;
                                            }
                                            var _cbDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBDRcumulative"].ToString());
                                            var _cbCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBCRcumulative"].ToString());
                                            if (_cbDrPC < 0)
                                            {
                                                _cbCrPC = _cbDrPC * -1;
                                                _cbDrPC = 0.00;
                                            }
                                            if (_cbCrPC < 0)
                                            {
                                                _cbDrPC = _cbCrPC * -1;
                                                _cbCrPC = 0.00;
                                            }
                                            oRU.SetText(ref sheet, row, obDebit, _obDrPC);
                                            oRU.SetText(ref sheet, row, obCredit, _obCrPC);
                                            oRU.SetText(ref sheet, row, Debit, _pDrPC);
                                            oRU.SetText(ref sheet, row, Credit, _pCrPC);
                                            oRU.SetText(ref sheet, row, cbDebit, _cbDrPC);
                                            oRU.SetText(ref sheet, row, cbCredit, _cbCrPC);
                                        }
                                    }
                                    //oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                    //oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                                }
                            }
                            else if (!string.IsNullOrEmpty(PartyId))
                            {
                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "'  AND PartyId = '" + PartyId + "' AND PartyPlantId = '" + PartyPlantId + "'"
                                };
                                var dtActDrCr = dvDrCr.ToTable();
                                if (dtActDrCr.Rows.Count != 0)
                                {
                                    for (int acp = 0; acp < dtParallelCurrency.Rows.Count; acp++)
                                    {
                                        var acpParallelCurrencyId = dtParallelCurrency.Rows[acp]["ParallelCurrencyId"].ToString();

                                        //var dvActDrCr = new DataView(dsLocal.Tables[0])
                                        //{
                                        //    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                                        //};
                                        //var dtActDrCr = dvActDrCr.ToTable();
                                        if (dtActDrCr.Rows.Count != 0)
                                        {
                                            drcrCol++;
                                            var _obDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBDRcumulative"].ToString());
                                            var _obCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBCRcumulative"].ToString());
                                            if (_obDrPC < 0)
                                            {
                                                _obCrPC += _obDrPC * -1;
                                                _obDrPC = 0.00;
                                            }
                                            if (_obCrPC < 0)
                                            {
                                                _obDrPC += _obCrPC * -1;
                                                _obCrPC = 0.00;
                                            }
                                            var _drPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["DRcumulative"].ToString());
                                            var _crPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CRcumulative"].ToString());
                                            if (_drPC < 0)
                                            {
                                                _crPC += _drPC * -1;
                                                _drPC = 0.00;
                                            }
                                            if (_crPC < 0)
                                            {
                                                _drPC += _crPC * -1;
                                                _crPC = 0.00;
                                            }
                                            var _pDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["PDRcumulative"].ToString());
                                            var _pCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["PCRcumulative"].ToString());
                                            if (_pDrPC < 0)
                                            {
                                                _pCrPC += _pDrPC * -1;
                                                _pDrPC = 0.00;
                                            }
                                            if (_pCrPC < 0)
                                            {
                                                _pCrPC += _pCrPC * -1;
                                                _pCrPC = 0.00;
                                            }
                                            var _cbDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBDRcumulative"].ToString());
                                            var _cbCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBCRcumulative"].ToString());
                                            if (_cbDrPC < 0)
                                            {
                                                _cbCrPC = _cbDrPC * -1;
                                                _cbDrPC = 0.00;
                                            }
                                            if (_cbCrPC < 0)
                                            {
                                                _cbDrPC = _cbCrPC * -1;
                                                _cbCrPC = 0.00;
                                            }
                                            oRU.SetText(ref sheet, row, obDebit, _obDrPC);
                                            oRU.SetText(ref sheet, row, obCredit, _obCrPC);
                                            oRU.SetText(ref sheet, row, Debit, _pDrPC);
                                            oRU.SetText(ref sheet, row, Credit, _pCrPC);
                                            oRU.SetText(ref sheet, row, cbDebit, _cbDrPC);
                                            oRU.SetText(ref sheet, row, cbCredit, _cbCrPC);
                                        }
                                    }
                                    //oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                    //oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                                }
                            }
                            else
                            {

                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "' AND ISNULL(BankMasterId,'') = '' AND ISNULL(CashMasterId,'') = '' AND ISNULL(PartyId,'') = '' AND ISNULL(PartyPlantId,'') = ''"

                                    // RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "'  AND PartyId = '" + PartyId + "' AND PartyPlantId = '" + PartyPlantId + "'"
                                };
                                var dtActDrCr = dvDrCr.ToTable();
                                if (dtActDrCr.Rows.Count != 0)
                                {
                                    for (int acp = 0; acp < dtParallelCurrency.Rows.Count; acp++)
                                    {
                                        var acpParallelCurrencyId = dtParallelCurrency.Rows[acp]["ParallelCurrencyId"].ToString();

                                        //var dvActDrCr = new DataView(dsLocal.Tables[0])
                                        //{
                                        //    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                                        //};
                                        //var dtActDrCr = dvActDrCr.ToTable();
                                        if (dtActDrCr.Rows.Count != 0)
                                        {
                                            drcrCol++;
                                            var _obDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBDRcumulative"].ToString());
                                            var _obCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["OBCRcumulative"].ToString());
                                            if (_obDrPC < 0)
                                            {
                                                _obCrPC += _obDrPC * -1;
                                                _obDrPC = 0.00;
                                            }
                                            if (_obCrPC < 0)
                                            {
                                                _obDrPC += _obCrPC * -1;
                                                _obCrPC = 0.00;
                                            }
                                            var _drPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["DRcumulative"].ToString());
                                            var _crPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CRcumulative"].ToString());
                                            if (_drPC < 0)
                                            {
                                                _crPC += _drPC * -1;
                                                _drPC = 0.00;
                                            }
                                            if (_crPC < 0)
                                            {
                                                _drPC += _crPC * -1;
                                                _crPC = 0.00;
                                            }
                                            var _pDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["PDRcumulative"].ToString());
                                            var _pCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["PCRcumulative"].ToString());
                                            if (_pDrPC < 0)
                                            {
                                                _pCrPC += _pDrPC * -1;
                                                _pDrPC = 0.00;
                                            }
                                            if (_pCrPC < 0)
                                            {
                                                _pCrPC += _pCrPC * -1;
                                                _pCrPC = 0.00;
                                            }
                                            var _cbDrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBDRcumulative"].ToString());
                                            var _cbCrPC = clsStaticInfo.dbl(dtActDrCr.Rows[0]["CBCRcumulative"].ToString());
                                            if (_cbDrPC < 0)
                                            {
                                                _cbCrPC = _cbDrPC * -1;
                                                _cbDrPC = 0.00;
                                            }
                                            if (_cbCrPC < 0)
                                            {
                                                _cbDrPC = _cbCrPC * -1;
                                                _cbCrPC = 0.00;
                                            }
                                            oRU.SetText(ref sheet, row, obDebit, _obDrPC);
                                            oRU.SetText(ref sheet, row, obCredit, _obCrPC);
                                            //oRU.SetText(ref sheet, row, Debit, _drPC);
                                            //oRU.SetText(ref sheet, row, Credit, _crPC);
                                            oRU.SetText(ref sheet, row, Debit, _pDrPC);
                                            oRU.SetText(ref sheet, row, Credit, _pCrPC);
                                            oRU.SetText(ref sheet, row, cbDebit, _cbDrPC);
                                            oRU.SetText(ref sheet, row, cbCredit, _cbCrPC);
                                        }
                                    }
                                    //oRU.SetText(ref sheet, row, mainColIndex, _drPC); mainColIndex++;
                                    //oRU.SetText(ref sheet, row, mainColIndex, _crPC);
                                }
                            }
                        }
                    }

                }
                
                else
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        if (clsStaticInfo.dbl(dtMainBody.Rows[n]["OBDRcumulative"].ToString()) + clsStaticInfo.dbl(dtMainBody.Rows[n]["OBCRcumulative"].ToString()) + clsStaticInfo.dbl(dtMainBody.Rows[n]["DRcumulative"].ToString()) + clsStaticInfo.dbl(dtMainBody.Rows[n]["CRcumulative"].ToString()) + clsStaticInfo.dbl(dtMainBody.Rows[n]["CBDRcumulative"].ToString()) + clsStaticInfo.dbl(dtMainBody.Rows[n]["CBCRcumulative"].ToString()) != 0)
                        {
                            row++;
                            var AccountCodeId = dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString();
                            var _Balancetype = dtMainBody.Rows[n]["Balancetype"].ToString();
                            oRU.SetText(ref sheet, row, mainColIndex, AccountCodeId + " - " + dtMainBody.Rows[n]["GL"]);
                            mainColIndex++;
                            for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                            {
                                var ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();

                                var dvDrCr = new DataView(dsLocal.Tables[0])
                                {
                                    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                                };
                                var dtDrCr = dvDrCr.ToTable();
                                if (dtDrCr.Rows.Count != 0)
                                {
                                    drcrCol++;
                                    var _obDrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["OBDRcumulative"].ToString());
                                    var _obCrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["OBCRcumulative"].ToString());
                                    if (_obDrPC < 0)
                                    {
                                        _obCrPC += _obDrPC * -1;
                                        _obDrPC = 0.00;
                                    }
                                    if (_obCrPC < 0)
                                    {
                                        _obDrPC += _obCrPC * -1;
                                        _obCrPC = 0.00;
                                    }
                                    var _drPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                    var _crPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                    if (_drPC < 0)
                                    {
                                        _crPC += _drPC * -1;
                                        _drPC = 0.00;
                                    }
                                    if (_crPC < 0)
                                    {
                                        _drPC += _crPC * -1;
                                        _crPC = 0.00;
                                    }
                                    var _pDrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["PDRcumulative"].ToString());
                                    var _pCrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["PCRcumulative"].ToString());
                                    if (_pDrPC < 0)
                                    {
                                        _pCrPC += _pDrPC * -1;
                                        _pDrPC = 0.00;
                                    }
                                    if (_pCrPC < 0)
                                    {
                                        _pCrPC += _pCrPC * -1;
                                        _pCrPC = 0.00;
                                    }
                                    var _cbDrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["CBDRcumulative"].ToString());
                                    var _cbCrPC = clsStaticInfo.dbl(dtDrCr.Rows[0]["CBCRcumulative"].ToString());
                                    if (_cbDrPC < 0)
                                    {
                                        _cbCrPC = _cbDrPC * -1;
                                        _cbDrPC = 0.00;
                                    }
                                    if (_cbCrPC < 0)
                                    {
                                        _cbDrPC = _cbCrPC * -1;
                                        _cbCrPC = 0.00;
                                    }
                                    oRU.SetText(ref sheet, row, obDebit, _obDrPC);
                                    oRU.SetText(ref sheet, row, obCredit, _obCrPC);
                                    oRU.SetText(ref sheet, row, Debit, _pDrPC);
                                    oRU.SetText(ref sheet, row, Credit, _pCrPC);
                                    oRU.SetText(ref sheet, row, cbDebit, _cbDrPC);
                                    oRU.SetText(ref sheet, row, cbCredit, _cbCrPC);
                                }
                            }
                            mainColIndex = 1;
                        }
                    }
                }

                row++;

                oRU.SetMasterHeaderText(ref sheet, row, colSum, "Total ");
                sheet.Range[oRU.GetColumnNameForXls(1) + row + ": " + oRU.GetColumnNameForXls(colSum) + row].Merge();

                var sumdrcrCol = colSum + 1;
                for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
                {
                    sheet.Range[row, obDebit].Formula = "=SUM(" + oRU.GetColumnNameForXls(obDebit) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(obDebit) + (row - 1) + ")";
                    sheet.Range[row, obDebit].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[row, obDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, obDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, obCredit].Formula = "=SUM(" + oRU.GetColumnNameForXls(obCredit) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(obCredit) + (row - 1) + ")";
                    sheet.Range[row, obCredit].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[row, obCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, obCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, Credit].Formula = "=SUM(" + oRU.GetColumnNameForXls(Credit) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(Credit) + (row - 1) + ")";
                    sheet.Range[row, Credit].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[row, Credit].CellStyle.Font.Bold = true;
                    sheet.Range[row, Credit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, Debit].Formula = "=SUM(" + oRU.GetColumnNameForXls(Debit) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(Debit) + (row - 1) + ")";
                    sheet.Range[row, Debit].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[row, Debit].CellStyle.Font.Bold = true;
                    sheet.Range[row, Debit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, cbCredit].Formula = "=SUM(" + oRU.GetColumnNameForXls(cbCredit) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(cbCredit) + (row - 1) + ")";
                    sheet.Range[row, cbCredit].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[row, cbCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, cbCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, cbDebit].Formula = "=SUM(" + oRU.GetColumnNameForXls(cbDebit) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(cbDebit) + (row - 1) + ")";
                    sheet.Range[row, cbDebit].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[row, cbDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, cbDebit].BorderAround(ExcelLineStyle.Hair);
                }

                var colLast = cbCredit;

                sheet.Range[8, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[8, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);

                sheet.Name = "Trial Balance";
                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                oRU.CompanyPlantHeader(ref sheet, colLast, "Trial Balance", companyId, plantName, null);
                oRU.SetText(ref sheet, 5, colLast, "Between " + fromDate + " AND " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(colLast) + 5].Merge();
                if (isActivityLevel)
                {
                    oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
                    //sheet["A" + colParticulers.ToString()].ColumnWidth = 35;

                }
                else
                {
                    oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);

                }

                //sheet.PageSetup.PrintTitleRows = "$A$1:$IV$" + titleRow;
            }
            else
            {
                sheet.Name = "Trial Balance";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyPlantHeader(ref sheet, 5, "Trial Balance", identity.CompanyId, plantName, null);
                oRU.SetText(ref sheet, 5, 3, "No Data Found", ExcelHAlign.HAlignCenter);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }

        private DataSet GetDateRangeWiseTrialBalanceInfo(string companyId, string plantId, string fromDate, string toDate, bool isBudgetLevel, bool isActivityLevel,bool isDetailLevel)
        {
            GridParameter parameters = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                if (isActivityLevel)
                {
                    parameters.CmdText = @"
                                            SELECT * 


                                            FROM(SELECT  AccountCodeId,ParallelCurrencyId,CurrencyCode,
		                                  SuM(OBDRcumulative + FROBDRcumulative) OBDRcumulative, SUM(OBCRcumulative + FROBCRcumulative) OBCRcumulative
										, SUM(DRcumulative) DRcumulative, SUM(CRcumulative) CRcumulative
                                            , SUM(OBDRcumulative + DRcumulative+FROBDRcumulative) CBDRcumulative, SUm(OBCRcumulative + CRcumulative+FROBCRcumulative) CBCRcumulative
                                           , SUM(PDRcumulative) PDRcumulative, SUM(PCRcumulative) PCRcumulative
										   ,BalanceType,[MainHead],GLGeneralInfoId,GL,GLGeneralInfoCode,Budget
										 ,ISNULL(BudgetMasterId,'') BudgetMasterId
										 ,Activity,ISNULL(ActivityId,'') ActivityId
		                                 FROM
		                                ( SELECT distinct	GL.Id AS AccountCodeId,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                        SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS OBDRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS OBCRcumulative, 0 DRcumulative, 0 CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative,0 FROBDRcumulative, 0 FROBCRcumulative,0 PDRcumulative,0 PCRcumulative,       
										    ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget,
											A.UserName AS Activity,
											
                                            A.Id AS ActivityId
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
											LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId=BUD.Id
											LEFT JOIN HKP.Activity A ON VD.ActivityId=A.Id
											LEFT JOIN [MST].BankMaster AS BA ON BA.Id=VD.BankMasterId
											LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
											LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
											LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
                                            WHERE v.PostingDate < '" + fromDate + @"' and v.CompanyId ='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                            AND  v.IsPark=0
                                            AND VDC.VoucherDetailId NOT IN ( SELECT VD.Id FROM  TRN.VoucherDetail AS VD  
																INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
																LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
																LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
																LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
																WHERE ACT.Id IN('Revenue','Expense') AND V.FiscalYearId in(select FiscalYearId from [SCS].[FiscalYearClose] ))
                                            GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id
											
											UNION 

										
											   SELECT distinct	GL.Id AS AccountCodeId,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,0 OBDRcumulative,0 OBCRcumulative,
		                                        SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS DRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS CRcumulative
                                 
                                           , 0 CBDRcumulative, 0 CBCRcumulative,0 FROBDRcumulative, 0 FROBCRcumulative   
										    , SUM(CASE WHEN SUM(VDC.DrAmount)<>0 THEN (SUM(VDC.DrAmount)) 
																		 ELSE 0 END
															) OVER (
			                                           PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS PDRcumulative
															
															, SUM(CASE WHEN SUM(VDC.CrAmount)<>0 THEN (SUM(VDC.CrAmount)) 
																		 ELSE 0 END
															) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS PCRcumulative,
										    ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget,
											A.UserName AS Activity,
											

                                            A.Id AS ActivityId
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
											LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId=BUD.Id
											LEFT JOIN HKP.Activity A ON VD.ActivityId=A.Id
											LEFT JOIN [MST].BankMaster AS BA ON BA.Id=VD.BankMasterId
											LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
											LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
											LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
                                            WHERE CONVERT(DATE, v.PostingDate) BETWEEN CONVERT(DATE, '" + fromDate + "') AND CONVERT(DATE, '" + toDate + @"') AND SourceType!='OpeningBalance' AND v.CompanyId ='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                            AND  V.IsPark=0
                                           GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id
                                            UNION
                                                        SELECT DISTINCT GL.Id AS AccountCodeId, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, 0 OBDRcumulative,0 OBCRcumulative, 0 DRcumulative, 0 CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative ,
															SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS FROBDRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS FROBCRcumulative,0 PDRcumulative,0 PCRcumulative
															, ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget,
											A.UserName AS Activity,
											

                                            A.Id AS ActivityId
	                                                FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
											LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId=BUD.Id
											LEFT JOIN HKP.Activity A ON VD.ActivityId=A.Id
											LEFT JOIN [MST].BankMaster AS BA ON BA.Id=VD.BankMasterId
											LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
											LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
											LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
	                                               
                                                    WHERE V.PostingDate = '" + fromDate + @"' AND V.CompanyId = '" + companyId + @"' AND V.PlantId = '" + plantId + @"' AND v.IsPark = 0 and v.SourceType='OpeningBalance'

                                                
                                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id, BA.AccountTitle, CM.UserName
											
											) TOTAL	
											GROUP BY AccountCodeId,ParallelCurrencyId,CurrencyCode,BalanceType,[MainHead],GLGeneralInfoId,GL,GLGeneralInfoCode,Budget
		                                    ,BudgetMasterId,Activity,ActivityId
                                            )ttd 
                                               WHERE ISNULL(DRcumulative,0.00) <> 0.00 OR ISNULL(CRcumulative,0) <> 0.00 OR
											ISNULL(OBDRcumulative,0.00) <> 0.00 OR ISNULL(OBCRcumulative,0) <> 0.00 OR
											ISNULL(CBDRcumulative,0.00) <> 0.00 OR ISNULL(CBCRcumulative,0) <> 0.00";

                    return _sqlRepository.GetGridData(parameters).Source;
                }
                else if (isBudgetLevel)
                {
                    parameters.CmdText = @" SELECT * FROM (SELECT AccountCodeId, ParallelCurrencyId, CurrencyCode, SuM(OBDRcumulative + FROBDRcumulative) OBDRcumulative, SUM(OBCRcumulative + FROBCRcumulative) OBCRcumulative
                                            , SUM(DRcumulative) DRcumulative, SUM(CRcumulative) CRcumulative
                                            , SUM(OBDRcumulative + DRcumulative+FROBDRcumulative) CBDRcumulative, SUm(OBCRcumulative + CRcumulative+FROBCRcumulative) CBCRcumulative
                                           , SUM(PDRcumulative) PDRcumulative, SUM(PCRcumulative) PCRcumulative
                                           , BalanceType, [MainHead],ISNULL(GLGeneralInfoId,'') GLGeneralInfoId, GL, GLGeneralInfoCode
                                           , ISNULL(BudgetMasterId,'') BudgetMasterId, Budget
                                                    FROM (
                                                    		SELECT DISTINCT GL.Id AS AccountCodeId, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (
                                                    			PARTITION BY GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
                                                    			) AS OBDRcumulative, SUM(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (
                                                    			PARTITION BY GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
                                                    			) AS OBCRcumulative, 0 DRcumulative, 0 CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative,0 FROBDRcumulative,0 FROBCRcumulative,0 PDRcumulative,0 PCRcumulative, ACT.BalanceType, ACT.Id AS [MainHead], VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, VD.BudgetMasterId, BUD.UserName AS Budget
                                                    	FROM TRN.VoucherDetailCurrency AS VDC
                                                    	INNER JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId
                                                    	INNER JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId
                                                    	LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId
                                                    	LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId
                                                    	LEFT JOIN [HKP].[AccountType] act ON act.Id = AG.AccountTypeId
                                                    	LEFT JOIN SCS.Currency AS CU ON CU.Id = VDC.ParallelCurrencyId
                                                    	LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId = BM.Id
                                                    	LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId = BUD.Id
                                                    	WHERE v.PostingDate < '" + fromDate + @"' AND v.CompanyId = '" + companyId + @"' AND V.PlantId = '" + plantId + @"' AND v.IsPark = 0 
                                                        AND VDC.VoucherDetailId NOT IN ( SELECT VD.Id FROM  TRN.VoucherDetail AS VD 
																INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
																LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
																LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
																LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
																WHERE ACT.Id IN('Revenue','Expense') AND V.FiscalYearId in(select FiscalYearId from [SCS].[FiscalYearClose] ))
                                                    	GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, BUD.UserName, v.PostingDate
                                                    	
                                                    	UNION
                                                    	
                                                    	SELECT DISTINCT GL.Id AS AccountCodeId, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, 0 OBDRcumulative, 0 OBCRcumulative
                                                              , SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (
			                                                PARTITION BY GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS DRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (SUM(VDC.CrAmount) - SUM(VDC.DrAmount)) ELSE 0 END) OVER (
			                                               PARTITION BY GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative,0 FROBDRcumulative, 0 FROBCRcumulative
                                                          , SUM(CASE WHEN SUM(VDC.DrAmount)<>0 THEN (SUM(VDC.DrAmount)) 
																		 ELSE 0 END
															) OVER (
			                                               PARTITION BY GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS PDRcumulative
															
															, SUM(CASE WHEN SUM(VDC.CrAmount)<>0 THEN (SUM(VDC.CrAmount)) 
																		 ELSE 0 END
															) OVER (
			                                               PARTITION BY GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS PCRcumulative
														, ACT.BalanceType, ACT.Id AS [MainHead], VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, VD.BudgetMasterId, BUD.UserName AS Budget
                                                    	FROM TRN.VoucherDetailCurrency AS VDC
                                                    	INNER JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId
                                                    	INNER JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId
                                                    	LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId
                                                    	LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId
                                                    	LEFT JOIN [HKP].[AccountType] act ON act.Id = AG.AccountTypeId
                                                    	LEFT JOIN SCS.Currency AS CU ON CU.Id = VDC.ParallelCurrencyId
                                                    	LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId = BM.Id
                                                    	LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId = BUD.Id
                                                    	WHERE Convert(DATE, v.PostingDate) BETWEEN Convert(DATE, '" + fromDate + @"') AND Convert(DATE, '" + toDate + @"') AND SourceType!='OpeningBalance' AND v.CompanyId = '" + companyId + @"' AND V.PlantId = '" + plantId + @"' AND v.IsPark = 0
                                                    	GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, BUD.UserName, v.PostingDate
                                                    	UNION
                                                        SELECT DISTINCT GL.Id AS AccountCodeId, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, 0 OBDRcumulative,0 OBCRcumulative, 0 DRcumulative, 0 CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative ,
															SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (
                                                    			PARTITION BY GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
                                                    			) AS FROBDRcumulative, SUM(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (
                                                    			PARTITION BY GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
                                                    			) AS FROBCRcumulative,0 PDRcumulative,0 PCRcumulative
															 ,ACT.BalanceType, ACT.Id AS [MainHead], VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, VD.BudgetMasterId, BUD.UserName AS Budget
	                                               FROM TRN.VoucherDetailCurrency AS VDC
                                                    	INNER JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId
                                                    	INNER JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId
                                                    	LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId
                                                    	LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId
                                                    	LEFT JOIN [HKP].[AccountType] act ON act.Id = AG.AccountTypeId
                                                    	LEFT JOIN SCS.Currency AS CU ON CU.Id = VDC.ParallelCurrencyId
                                                    	LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId = BM.Id
                                                    	LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId = BUD.Id
                                                    	WHERE Convert(DATE, v.PostingDate) = Convert(DATE, '" + fromDate + @"') AND v.CompanyId = '" + companyId + @"' AND V.PlantId = '" + plantId + @"' AND v.IsPark = 0  AND SourceType='OpeningBalance'
                                                    	GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, BUD.UserName, v.PostingDate
                                                    	
                                                        ) TOTAL
                                                    GROUP BY AccountCodeId, ParallelCurrencyId, CurrencyCode, BalanceType, [MainHead], GLGeneralInfoId, GL, GLGeneralInfoCode, BudgetMasterId, Budget)ttd 
                                            WHERE ISNULL(DRcumulative,0.00) <> 0.00 OR ISNULL(CRcumulative,0) <> 0.00 OR
											ISNULL(OBDRcumulative,0.00) <> 0.00 OR ISNULL(OBCRcumulative,0) <> 0.00 OR
											ISNULL(CBDRcumulative,0.00) <> 0.00 OR ISNULL(CBCRcumulative,0) <> 0.00";
                    return _sqlRepository.GetGridData(parameters).Source;
                }
                else if(isDetailLevel)
                {

                    parameters.CmdText = @"SELECT * FROM(SELECT  AccountCodeId,ParallelCurrencyId,CurrencyCode,
		                                  SuM(OBDRcumulative + FROBDRcumulative) OBDRcumulative, SUM(OBCRcumulative + FROBCRcumulative) OBCRcumulative
										, SUM(DRcumulative) DRcumulative, SUM(CRcumulative) CRcumulative
                                            , SUM(OBDRcumulative + DRcumulative+FROBDRcumulative) CBDRcumulative, SUm(OBCRcumulative + CRcumulative+FROBCRcumulative) CBCRcumulative
                                           , SUM(PDRcumulative) PDRcumulative, SUM(PCRcumulative) PCRcumulative
										   ,BalanceType,[MainHead],GLGeneralInfoId,GL,GLGeneralInfoCode,Budget
										 ,ISNULL(BudgetMasterId,'') BudgetMasterId
										 ,Activity,Particulars,ISNULL(ActivityId,'') ActivityId,ISNULL(BankMasterId,'') BankMasterId
										 ,ISNULL(CashMasterId,'') CashMasterId,ISNULL(PartyId,'') PartyId,ISNULL(PartyPlantId,'') PartyPlantId
		                                 FROM
		                                ( SELECT distinct	GL.Id AS AccountCodeId,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                        SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS OBDRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS OBCRcumulative, 0 DRcumulative, 0 CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative,0 FROBDRcumulative, 0 FROBCRcumulative,0 PDRcumulative,0 PCRcumulative,       
										    ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget,
											A.UserName AS Activity,
											[Particulars]=CASE 
											WHEN BA.AccountTitle<>'' THEN BA.AccountTitle
											WHEN CM.UserName<>'' THEN CM.UserName
											WHEN P.UserName<>'' THEN PP.UserName
											ELSE ''	END,

                                            A.Id AS ActivityId, VD.BankMasterId, VD.CashMasterId, VD.PartyId, VD.PartyPlantId
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
											LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId=BUD.Id
											LEFT JOIN HKP.Activity A ON VD.ActivityId=A.Id
											LEFT JOIN [MST].BankMaster AS BA ON BA.Id=VD.BankMasterId
											LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
											LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
											LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
                                            WHERE v.PostingDate < '" + fromDate + @"' and v.CompanyId ='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                            AND  v.IsPark=0
                                            AND VDC.VoucherDetailId NOT IN ( SELECT VD.Id FROM  TRN.VoucherDetail AS VD  
																INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
																LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
																LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
																LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
																WHERE ACT.Id IN('Revenue','Expense') AND V.FiscalYearId in(select FiscalYearId from [SCS].[FiscalYearClose] ))
                                            GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id, BA.AccountTitle, CM.UserName
											,VD.BankMasterId, VD.CashMasterId, P.UserName, PP.UserName, VD.PartyId, VD.PartyPlantId

											UNION 

											   SELECT distinct	GL.Id AS AccountCodeId,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,0 OBDRcumulative,0 OBCRcumulative,
		                                        SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS DRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS CRcumulative
                                 
                                           , 0 CBDRcumulative, 0 CBCRcumulative,0 FROBDRcumulative, 0 FROBCRcumulative   
										    , SUM(CASE WHEN SUM(VDC.DrAmount)<>0 THEN (SUM(VDC.DrAmount)) 
																		 ELSE 0 END
															) OVER (
			                                           PARTITION BY GL.Id, VD.BudgetMasterId,A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS PDRcumulative
															
															, SUM(CASE WHEN SUM(VDC.CrAmount)<>0 THEN (SUM(VDC.CrAmount)) 
																		 ELSE 0 END
															) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS PCRcumulative,
										    ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget,
											A.UserName AS Activity,
											[Particulars]=CASE 
											WHEN BA.AccountTitle<>'' THEN BA.AccountTitle 
											WHEN CM.UserName<>'' THEN CM.UserName
											WHEN P.UserName<>'' THEN PP.UserName
											ELSE ''	END,

                                            A.Id AS ActivityId, VD.BankMasterId, VD.CashMasterId, VD.PartyId, VD.PartyPlantId
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
											LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId=BUD.Id
											LEFT JOIN HKP.Activity A ON VD.ActivityId=A.Id
											LEFT JOIN [MST].BankMaster AS BA ON BA.Id=VD.BankMasterId
											LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
											LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
											LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
                                            WHERE CONVERT(DATE, v.PostingDate) BETWEEN CONVERT(DATE, '" + fromDate + "') AND CONVERT(DATE, '" + toDate + @"') AND SourceType!='OpeningBalance' AND v.CompanyId ='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                            AND  V.IsPark=0
                                            GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id, BA.AccountTitle, CM.UserName
											,VD.BankMasterId, VD.CashMasterId, P.UserName, PP.UserName, VD.PartyId, VD.PartyPlantId
											 
                                            UNION
                                                        SELECT DISTINCT GL.Id AS AccountCodeId, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, 0 OBDRcumulative,0 OBCRcumulative, 0 DRcumulative, 0 CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative ,
															SUM(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS FROBDRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (PARTITION BY GL.Id, VD.BudgetMasterId,A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId
			                                                ) AS FROBCRcumulative,0 PDRcumulative,0 PCRcumulative
															, ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget,
											A.UserName AS Activity,
											[Particulars]=CASE 
											WHEN BA.AccountTitle<>'' THEN BA.AccountTitle
											WHEN CM.UserName<>'' THEN CM.UserName
											WHEN P.UserName<>'' THEN PP.UserName
											ELSE ''	END,

                                            A.Id AS ActivityId, VD.BankMasterId, VD.CashMasterId, VD.PartyId, VD.PartyPlantId
	                                                FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
											LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId=BUD.Id
											LEFT JOIN HKP.Activity A ON VD.ActivityId=A.Id
											LEFT JOIN [MST].BankMaster AS BA ON BA.Id=VD.BankMasterId
											LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
											LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
											LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
	                                               
                                                    WHERE V.PostingDate = '" + fromDate + @"' AND V.CompanyId = '" + companyId + @"' AND V.PlantId = '" + plantId + @"' AND v.IsPark = 0 and v.SourceType='OpeningBalance'

                                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id, BA.AccountTitle, CM.UserName
											,VD.BankMasterId, VD.CashMasterId, P.UserName, PP.UserName, VD.PartyId, VD.PartyPlantId
											) TOTAL

											GROUP BY AccountCodeId,ParallelCurrencyId,CurrencyCode,BalanceType,[MainHead],GLGeneralInfoId,GL,GLGeneralInfoCode,Budget
		                                    ,BudgetMasterId,Activity,Particulars,ActivityId,BankMasterId,CashMasterId,PartyId,PartyPlantId
                                           
                                            )ttd 
                                            WHERE ISNULL(DRcumulative,0.00) <> 0.00 OR ISNULL(CRcumulative,0) <> 0.00 OR
											ISNULL(OBDRcumulative,0.00) <> 0.00 OR ISNULL(OBCRcumulative,0) <> 0.00 OR
											ISNULL(CBDRcumulative,0.00) <> 0.00 OR ISNULL(CBCRcumulative,0) <> 0.00
                                            OR ISNULL(PDRcumulative,0.00) <> 0.00 OR ISNULL(PCRcumulative,0) <> 0.00";

                    return _sqlRepository.GetGridData(parameters).Source;

                }
                else
                {
                    parameters.CmdText = @" SELECT * FROM(SELECT AccountCodeId, ParallelCurrencyId, CurrencyCode, SuM(OBDRcumulative + FROBDRcumulative) OBDRcumulative, SUM(OBCRcumulative + FROBCRcumulative) OBCRcumulative
                            , SUM(DRcumulative) DRcumulative
                            , SUM(CRcumulative) CRcumulative, SUM(OBDRcumulative + DRcumulative+FROBDRcumulative) CBDRcumulative, SUm(OBCRcumulative + CRcumulative+FROBCRcumulative) CBCRcumulative
                            , SUM(PDRcumulative) PDRcumulative, SUM(PCRcumulative) PCRcumulative
												, BalanceType, [MainHead], ISNULL(GLGeneralInfoId,'') GLGeneralInfoId, ISNULL(GL,'') GL, ISNULL(GLGeneralInfoCode,'') GLGeneralInfoCode
                                                FROM (
	                                                SELECT DISTINCT GL.Id AS AccountCodeId, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (
			                                                PARTITION BY GL.Id, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS OBDRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (
			                                                PARTITION BY GL.Id, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS OBCRcumulative, 0 DRcumulative, 0 CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative,0 FROBDRcumulative, 0 FROBCRcumulative
															, 0 PDRcumulative, 0 PCRcumulative, ACT.BalanceType, ACT.Id AS [MainHead], VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
	                                                
													FROM TRN.VoucherDetailCurrency AS VDC
	                                                INNER JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId
	                                                INNER JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId
	                                                LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId
	                                                LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId
	                                                LEFT JOIN [HKP].[AccountType] act ON act.Id = AG.AccountTypeId
	                                                LEFT JOIN SCS.Currency AS CU ON CU.Id = VDC.ParallelCurrencyId
	                                               
                                                    WHERE v.PostingDate < '" + fromDate + @"' AND v.CompanyId = '" + companyId + @"' AND V.PlantId = '" + plantId + @"' AND v.IsPark = 0
                                                    AND VDC.VoucherDetailId NOT IN ( SELECT VD.Id FROM  TRN.VoucherDetail AS VD  
																INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
																LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
																LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
																LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
																WHERE ACT.Id IN('Revenue','Expense') AND V.FiscalYearId in(select FiscalYearId from [SCS].[FiscalYearClose] ))
	                                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, ACT.BalanceType, ACT.Id, v.PostingDate
	
	                                                UNION
	
	                                                 SELECT DISTINCT GL.Id AS AccountCodeId, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, 0 OBDRcumulative, 0 OBcRcumulative, sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (
			                                                PARTITION BY GL.Id, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS DRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (SUM(VDC.CrAmount) - SUM(VDC.DrAmount)) ELSE 0 END) OVER (
			                                                PARTITION BY GL.Id, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative,0 FROBDRcumulative, 0 FROBCRcumulative
                                                          , sum(CASE WHEN SUM(VDC.DrAmount)<>0 THEN (SUM(VDC.DrAmount)) 
																		 ELSE 0 END
															) OVER (
			                                                PARTITION BY GL.Id, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS PDRcumulative
															
															, sum(CASE WHEN SUM(VDC.CrAmount)<>0 THEN (SUM(VDC.CrAmount)) 
																		 ELSE 0 END
															) OVER (
			                                                PARTITION BY GL.Id, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS PCRcumulative


													, ACT.BalanceType, ACT.Id AS [MainHead], VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
	                                                FROM TRN.VoucherDetailCurrency AS VDC
	                                                INNER JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId
	                                                INNER JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId
	                                                LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId
	                                                LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId
	                                                LEFT JOIN [HKP].[AccountType] act ON act.Id = AG.AccountTypeId
	                                                LEFT JOIN SCS.Currency AS CU ON CU.Id = VDC.ParallelCurrencyId
	                                                WHERE CONVERT(DATE, v.PostingDate) BETWEEN CONVERT(DATE, '" + fromDate + @"') AND CONVERT(DATE, '" + toDate + @"') AND SourceType!='OpeningBalance' AND v.CompanyId = '" + companyId + @"' AND V.PlantId = '" + plantId + @"' AND v.IsPark = 0
	                                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, ACT.BalanceType, ACT.Id, v.PostingDate	
	                                                 
                                                    UNION

													 SELECT DISTINCT GL.Id AS AccountCodeId, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, 0 OBDRcumulative,0 OBCRcumulative, 0 DRcumulative, 0 CRcumulative, 0 CBDRcumulative, 0 CBCRcumulative ,
															sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) OVER (
			                                                PARTITION BY GL.Id, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS FROBDRcumulative, sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) OVER (
			                                                PARTITION BY GL.Id, VDC.ParallelCurrencyId ORDER BY VDC.ParallelCurrencyId
			                                                ) AS FROBCRcumulative
															, 0 PDRcumulative, 0 PCRcumulative
															, ACT.BalanceType, ACT.Id AS [MainHead], VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
	                                                FROM TRN.VoucherDetailCurrency AS VDC
	                                                INNER JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId
	                                                INNER JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId
	                                                LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId
	                                                LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId
	                                                LEFT JOIN [HKP].[AccountType] act ON act.Id = AG.AccountTypeId
	                                                LEFT JOIN SCS.Currency AS CU ON CU.Id = VDC.ParallelCurrencyId
	                                               
                                                    WHERE V.PostingDate = '" + fromDate + @"' AND V.CompanyId = '" + companyId + @"' AND V.PlantId = '" + plantId + @"' AND v.IsPark = 0 and v.SourceType='OpeningBalance'
                                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, ACT.BalanceType, ACT.Id, v.PostingDate
	    
                                                ) TOTAL
                                                GROUP BY AccountCodeId, ParallelCurrencyId, CurrencyCode, BalanceType, [MainHead], GLGeneralInfoId, GL, GLGeneralInfoCode )ttd
                                            WHERE ISNULL(DRcumulative, 0.00) <> 0.00 OR ISNULL(CRcumulative,0) <> 0.00 OR

                                            ISNULL(OBDRcumulative, 0.00) <> 0.00 OR ISNULL(OBCRcumulative,0) <> 0.00 OR

                                            ISNULL(CBDRcumulative, 0.00) <> 0.00 OR ISNULL(CBCRcumulative,0) <> 0.00";

                    return _sqlRepository.GetGridData(parameters).Source;
                }



            }
            catch (Exception)
            {
                throw;
            }
        }


        private DataSet GetTrialBalanceInfo(string companyId, string plantId, string toDate, bool isBudgetLevel, bool isActivityLevel, bool isDetailLevel,string partyId, string partyPlantId)
        {
            GridParameter parameters = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (partyId == "null") { partyId = null; }
                if (partyPlantId == "null") { partyPlantId = null; }
                var tempSql = "";
                if (!string.IsNullOrEmpty(partyId) && !string.IsNullOrEmpty(partyPlantId))
                {
                    tempSql = " AND VD.PartyId='"+ partyId + "' AND VD.PartyPlantId='"+ partyPlantId + @"'";
                } else if(partyId != null && partyPlantId == null)
                    {
                        tempSql = " AND VD.PartyId='" + partyId + @"'";
                    }
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                if (isActivityLevel)
                {
                    parameters.CmdText = @"SELECT * FROM( SELECT distinct	GL.Id AS AccountCodeId,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                         sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative ,
                                            ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget,
											A.UserName AS Activity,
									
                                            A.Id AS ActivityId
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
											LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId=BUD.Id
											LEFT JOIN HKP.Activity A ON VD.ActivityId=A.Id
											LEFT JOIN [MST].BankMaster AS BA ON BA.Id=VD.BankMasterId
											LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
											LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
											LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
                                            WHERE v.PostingDate <= '" + toDate + @"' and v.CompanyId ='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                            AND  v.IsPark=0
                                              GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id
											 ) ttd 
                                            WHERE ISNULL(DRcumulative,0.00) <> 0.00 OR ISNULL(CRcumulative,0) <> 0.00";

                    return _sqlRepository.GetGridData(parameters).Source;
                }
                else if (isBudgetLevel)
                {
                    parameters.CmdText = @"  SELECT * FROM (SELECT distinct	GL.Id AS AccountCodeId,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                    sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                            , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative ,
                                            ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
											LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId=BUD.Id
                                            where v.PostingDate <= '" + toDate + @"' and v.CompanyId ='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                            and  v.IsPark=0
                                            GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName, GL.AccountCode, ACT.BalanceType,ACT.Id,VD.BudgetMasterId,BUD.UserName,v.PostingDate) ttd 
                                            WHERE ISNULL(DRcumulative,0.00) <> 0.00 OR ISNULL(CRcumulative,0) <> 0.00";

                    return _sqlRepository.GetGridData(parameters).Source;
                }
                else if (isDetailLevel)
                {
                    parameters.CmdText = @"SELECT * FROM( SELECT distinct	GL.Id AS AccountCodeId,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                         sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative--, VD.PartyPlantId
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative ,--, VD.PartyPlantId
                                            ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget,
											A.UserName AS Activity,
											[Particulars]=CASE 
											WHEN BA.AccountTitle<>'' THEN BA.AccountTitle
											WHEN CM.UserName<>'' THEN CM.UserName
											WHEN P.UserName<>'' THEN P.UserName
											ELSE ''	END,

                                            A.Id AS ActivityId, VD.BankMasterId, VD.CashMasterId, VD.PartyId--, VD.PartyPlantId
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
											LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId=BUD.Id
											LEFT JOIN HKP.Activity A ON VD.ActivityId=A.Id
											LEFT JOIN [MST].BankMaster AS BA ON BA.Id=VD.BankMasterId
											LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
											LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
											--LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
                                            WHERE v.PostingDate <= '" + toDate + @"' and v.CompanyId ='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                            AND  v.IsPark=0 "+ tempSql + @"
                                            GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id, BA.AccountTitle, CM.UserName
											,VD.BankMasterId, VD.CashMasterId, P.UserName, VD.PartyId ) ttd --, PP.UserName, VD.PartyPlantId
                                            WHERE ISNULL(DRcumulative,0.00) <> 0.00 OR ISNULL(CRcumulative,0) <> 0.00";

                    return _sqlRepository.GetGridData(parameters).Source;


                }

                else
                {
                    parameters.CmdText = @" SELECT * FROM (SELECT  distinct	GL.Id AS AccountCodeId,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                 sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                         , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative ,
                                            ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                            where v.PostingDate <= '" + toDate + @"' and v.CompanyId ='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                            and  v.IsPark=0
                                            group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName, GL.AccountCode, ACT.BalanceType,ACT.Id,v.PostingDate) ttd 
                                            WHERE ISNULL(DRcumulative,0.00) <> 0.00 OR ISNULL(CRcumulative,0) <> 0.00";

                    return _sqlRepository.GetGridData(parameters).Source;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetYearClosedTrialBalanceInfo(string companyId, string plantId, string fiscalYearCloseId, bool isBudgetLevel, bool isActivityLevel, bool isDetailLevel)
        {
            GridParameter parameters = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                if (isActivityLevel)
                {
                    parameters.CmdText = @"SELECT * FROM (SELECT [AccountCodeId], [ParallelCurrencyId], [CurrencyCode],SUM(CAST([DRcumulative] AS DECIMAL(18,2))) DRcumulative
									, SUM(CAST([CRcumulative] AS DECIMAL(18,2)))CRcumulative
									, [BalanceType], [MainHead], [GLGeneralInfoId], [GL], [GLGeneralInfoCode], [Budget], [BudgetMasterId], [Activity], [ActivityId]
									FROM [TRN].[FiscalYearCloseTrialBalance]  FYCB WHERE FYCB.FiscalYearCloseId= '" + fiscalYearCloseId + @"' AND FYCB.CompanyId='" + companyId + @"'  AND FYCB.PlantId='" + plantId + @"'   
                                    GROUP BY [AccountCodeId], [ParallelCurrencyId], [CurrencyCode], [BalanceType], [MainHead], [GLGeneralInfoId], [GL], [GLGeneralInfoCode]
									, [Budget], [BudgetMasterId], [Activity], [ActivityId] )T
									WHERE DRcumulative+CRcumulative<>0 ";

                    return _sqlRepository.GetGridData(parameters).Source;
                }
                else if (isBudgetLevel)
                {
                    parameters.CmdText = @"SELECT * FROM ( SELECT [AccountCodeId], [ParallelCurrencyId], [CurrencyCode],SUM(CAST([DRcumulative] AS DECIMAL(18,2))) DRcumulative
									, SUM(CAST([CRcumulative] AS DECIMAL(18,2)))CRcumulative
									, [BalanceType], [MainHead], [GLGeneralInfoId], [GL], [GLGeneralInfoCode], [Budget], [BudgetMasterId]
									FROM [TRN].[FiscalYearCloseTrialBalance]  FYCB WHERE FYCB.FiscalYearCloseId= '" + fiscalYearCloseId + @"' AND FYCB.CompanyId='" + companyId + @"'  AND FYCB.PlantId='" + plantId + @"'   
                                    GROUP BY [AccountCodeId], [ParallelCurrencyId], [CurrencyCode], [BalanceType], [MainHead], [GLGeneralInfoId], [GL], [GLGeneralInfoCode]
									, [Budget], [BudgetMasterId] )T
									WHERE DRcumulative+CRcumulative<>0 ";

                    return _sqlRepository.GetGridData(parameters).Source;
                }
                else if (isDetailLevel)
                {
                    parameters.CmdText = @"SELECT * FROM (SELECT [AccountCodeId], [ParallelCurrencyId], [CurrencyCode],CAST([DRcumulative] AS DECIMAL(18,2)) DRcumulative
									, CAST([CRcumulative] AS DECIMAL(18,2))CRcumulative
									, [BalanceType], [MainHead], [GLGeneralInfoId], [GL], [GLGeneralInfoCode], [Budget], [BudgetMasterId], [Activity], [ActivityId],[Particulars]
                                    , BankMasterId, CashMasterId, PartyId,PartyPlantId
									FROM [TRN].[FiscalYearCloseTrialBalance]  FYCB WHERE FYCB.FiscalYearCloseId= '" + fiscalYearCloseId + @"' AND FYCB.CompanyId='" + companyId + @"'  AND FYCB.PlantId='" + plantId + @"'   
                                    )T
									WHERE DRcumulative+CRcumulative<>0 ";

                    return _sqlRepository.GetGridData(parameters).Source;


                }

                else
                {
                    parameters.CmdText = @"SELECT * FROM ( SELECT [AccountCodeId], [ParallelCurrencyId], [CurrencyCode],SUM(CAST([DRcumulative] AS DECIMAL(18,2))) DRcumulative
									, SUM(CAST([CRcumulative] AS DECIMAL(18,2)))CRcumulative
									, [BalanceType], [MainHead], [GLGeneralInfoId], [GL], [GLGeneralInfoCode]
									FROM [TRN].[FiscalYearCloseTrialBalance]  FYCB WHERE FYCB.FiscalYearCloseId= '" + fiscalYearCloseId + @"' AND FYCB.CompanyId='" + companyId + @"'  AND FYCB.PlantId='" + plantId + @"'   
                                    GROUP BY [AccountCodeId], [ParallelCurrencyId], [CurrencyCode], [BalanceType], [MainHead], [GLGeneralInfoId], [GL], [GLGeneralInfoCode] )T
									WHERE DRcumulative+CRcumulative<>0 ";

                    return _sqlRepository.GetGridData(parameters).Source;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet, Authorize]
        public ActionResult TrialBalanceYearClosedReport(ReportFormat reportFormat, string fiscalYearCloseId, string fiscalYearName, bool isBudgetLevel, bool isActivityLevel, bool isDetailLevel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = GetTrialBalanceYearClosedReport(identity.CompanyId, identity.PlantId, identity.PlantName, fiscalYearCloseId, fiscalYearName, isBudgetLevel, isActivityLevel, isDetailLevel);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + "Year Closed Trial Balance Sheet";
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


        public IWorkbook GetTrialBalanceYearClosedReport(string companyId, string plantId, string plantName, string fiscalYearCloseId, string fiscalYearName, bool isBudgetLevel, bool isActivityLevel, bool isDetailLevel)
        {
            var excelEngine = new ExcelEngine();
            var oRU = new ReportUtility();
            var dsLocal = GetYearClosedTrialBalanceInfo(companyId, plantId, fiscalYearCloseId, isBudgetLevel, isActivityLevel, isDetailLevel);
            var workbook = oRU.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            var dtLocal = dsLocal.Tables[0];
            if (dtLocal.Rows.Count > 0)
            {
                var dvParallelCurrency = new DataView(dsLocal.Tables[0])
                {
                    Sort = "CurrencyCode ASC"
                };
                var dtParallelCurrency = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");

                var dvMainBody = new DataView(dsLocal.Tables[0])
                {
                    Sort = "GLGeneralInfoCode"
                };
                var dtMainBody = dvMainBody.ToTable();

                var col = 1;
                var shet2EndxlsCol = col;

                var row = 6;
                row++;
                var headreColIndex = 1;
                var mainColIndex = 1;

                oRU.SetHeaderText(ref sheet, row, headreColIndex, "GL", 32);
                headreColIndex++;
                if (isBudgetLevel)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Budget Name", 32);
                    headreColIndex++;
                }
                if (isActivityLevel)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Budget Name", 32);
                    headreColIndex++;

                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Activity Name", 32);
                    headreColIndex++;


                }
                if (isDetailLevel)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Budget Name", 32);
                    headreColIndex++;

                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Activity Name", 32);
                    headreColIndex++;
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Particulars", 32);
                    headreColIndex++;


                }
                var colSum = headreColIndex - 1;
                int colCurrencyIndex = headreColIndex;
                var plCurrencyId = string.Empty;
                var plCurrencyCode = string.Empty;

                var alParaCurrency = new ArrayList();

                for (int n = 0; n < dtParallelCurrency.Rows.Count; n++)
                {
                    oRU.SetHeaderText(ref sheet, row - 1, headreColIndex, dtParallelCurrency.Rows[n]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                    sheet[row - 1, headreColIndex, row - 1, headreColIndex + 1].Merge();
                    var dic = new Dictionary<string, int>
                {
                    { dtParallelCurrency.Rows[n]["ParallelCurrencyId"].ToString(), headreColIndex }
                };
                    alParaCurrency.Add(dic);

                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Dr", ExcelHAlign.HAlignRight); headreColIndex++;
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Cr", ExcelHAlign.HAlignRight); //headreColIndex++;

                    if (n == 0)
                    {
                        plCurrencyCode = dtParallelCurrency.Rows[n]["CurrencyCode"].ToString();
                    }

                    sheet.Range[row - 1, colCurrencyIndex, row - 1, headreColIndex].BorderAround(ExcelLineStyle.Hair);
                }
                shet2EndxlsCol = headreColIndex - 1;

                var drcrCol = 0;
                var Row_Total_Start = row + 1;

                if (isActivityLevel)
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        row++;
                        mainColIndex = 1;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString() + " - " + dtMainBody.Rows[n]["GL"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Budget"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Activity"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(dtMainBody.Rows[n]["DRcumulative"].ToString())); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(dtMainBody.Rows[n]["CRcumulative"].ToString()));
                    }

                }
                else if (isBudgetLevel)
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        row++;
                        mainColIndex = 1;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString() + " - " + dtMainBody.Rows[n]["GL"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Budget"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(dtMainBody.Rows[n]["DRcumulative"].ToString())); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(dtMainBody.Rows[n]["CRcumulative"].ToString())); 
                    }
                }
                else if (isDetailLevel)
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        row++;
                        mainColIndex = 1;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString() + " - " + dtMainBody.Rows[n]["GL"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Budget"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Activity"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["Particulars"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(dtMainBody.Rows[n]["DRcumulative"].ToString())); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(dtMainBody.Rows[n]["CRcumulative"].ToString())); 
                    }

                }
                else
                {
                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        row++;
                        mainColIndex = 1;
                        oRU.SetText(ref sheet, row, mainColIndex, dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString() + " - " + dtMainBody.Rows[n]["GL"].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(dtMainBody.Rows[n]["DRcumulative"].ToString())); mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(dtMainBody.Rows[n]["CRcumulative"].ToString()));  
                    }
                }

                row++;

                oRU.SetMasterHeaderText(ref sheet, row, colSum, "Total ");
                sheet.Range[oRU.GetColumnNameForXls(1) + row + ": " + oRU.GetColumnNameForXls(colSum) + row].Merge();

                var sumdrcrCol = colSum + 1;
                for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
                {
                    sheet.Range[row, sumdrcrCol].Formula = "=SUM(" + oRU.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(sumdrcrCol) + (row - 1) + ")";
                    sheet.Range[row, sumdrcrCol].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[row, sumdrcrCol].CellStyle.Font.Bold = true;
                    sheet.Range[row, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);

                    sumdrcrCol++;
                    sheet.Range[row, sumdrcrCol].Formula = "=SUM(" + oRU.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(sumdrcrCol) + (row - 1) + ")";
                    sheet.Range[row, sumdrcrCol].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[row, sumdrcrCol].CellStyle.Font.Bold = true;
                    sheet.Range[row, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
                }

                var colLast = sumdrcrCol;
                sheet.Range[8, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[8, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);

                sheet.Name = "Sheet";
                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                oRU.CompanyPlantHeader(ref sheet, colLast, "Year Closed Trial Balance", companyId, plantId, plantName, null);
                oRU.SetText(ref sheet, 5, colLast, "Fiscal Year: " + fiscalYearName + "", ExcelHAlign.HAlignCenter);
                sheet.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(colLast) + 5].Merge();
                if (isActivityLevel)
                {
                    oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
                }
                else
                {
                    oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                }
            }
            else
            {
                sheet.Name = "Sheet";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyPlantHeader(ref sheet, 5, "Year Closed Trial Balance", identity.CompanyId, plantId, plantName, null);
                oRU.SetText(ref sheet, 5, 3, "No Data Found", ExcelHAlign.HAlignCenter);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }

        #endregion


        //GetDateRangeWiseTrialBalanceReport(string companyId, string plantId, string plantName, string toDate, bool isBudgetLevel, bool isActivityLevel)
        [HttpGet, Authorize]
        public ActionResult IncomeStatementReport(string date, string parallelCurrency, bool isBudgetLevel, bool isActivityLevel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Income Statement Report " + DateTime.Now.ToString("ddMMMyyyy") + ".xlsx";
            var workbook = _voucharReportService.GetIncomeStatementReport(identity.CompanyId, identity.PlantId, identity.PlantName, date, new JavaScriptSerializer().Deserialize<string[]>(parallelCurrency),  isBudgetLevel,  isActivityLevel);
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        [HttpGet, Authorize]
        public ActionResult IncomeStatementYearClosedReport(string fiscalYearCloseId, string fiscalYearName, bool isBudgetLevel, bool isActivityLevel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Year Closed Income Statement Report " + DateTime.Now.ToString("ddMMMyyyy") + ".xlsx";
            var workbook = _voucharReportService.GetIncomeStatementYearClosedReport(identity.CompanyId, identity.PlantId, identity.PlantName, fiscalYearCloseId, fiscalYearName, isBudgetLevel, isActivityLevel);
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        //GetDateRangeWiseTrialBalanceReport(string companyId, string plantId, string plantName, string toDate, bool isBudgetLevel, bool isActivityLevel)
        [HttpGet, Authorize]
        public ActionResult incomestatementreportDateWise(string fromDate, string toDate, string parallelCurrency, bool isBudgetLevel, bool isActivityLevel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Income Statement Report " + DateTime.Now.ToString("ddMMMyyyy") + ".xlsx";
            var workbook = _voucharReportService.GetIncomeStatementReportDateWise(identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, new JavaScriptSerializer().Deserialize<string[]>(parallelCurrency),  isBudgetLevel, isActivityLevel);
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        [Authorize]
        public ActionResult EntityWiseExpenseAndEarningreportDateWise(string fromDate, string toDate, string entityId, string entity, string parallelCurrency)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "EntityWiseExpenseAndEarning Report " + DateTime.Now.ToString("ddMMMyyyy") + ".xlsx";
            var workbook = _voucharReportService.GetEntityWiseExpenseAndEarningReportDateWise(identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, entityId, entity, new JavaScriptSerializer().Deserialize<string[]>(parallelCurrency));
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }
        [Authorize]
        public ActionResult EntityWiseExpenseAndEarningreportDateWiseActivityLevel(string fromDate, string toDate, string entityId, string entity, string parallelCurrency)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "EntityWiseExpenseAndEarning Report " + DateTime.Now.ToString("ddMMMyyyy") + ".xlsx";
            var workbook = _voucharReportService.EntityWiseExpenseAndEarningreportDateWiseActivityLevel(identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, entityId, entity, new JavaScriptSerializer().Deserialize<string[]>(parallelCurrency));
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }


        #region BalanceSheet

        public ActionResult BalanceSheetReportPage()
        {
            return View("~/Areas/Accounts/Views/BalanceSheetReportPage.cshtml");
        }
        public ActionResult BalanceSheetReportTreeView()
        {
            return View("~/Areas/Accounts/Views/BalanceSheetReportTreeView.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult BalanceSheetReport(ReportFormat reportFormat, string date, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = GetBalanceSheetReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, date, isBudgetLevel, isActivityLevel, isACGroupLevel);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Balance Sheet";
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

        public IWorkbook GetBalanceSheetReport(string companyGroupId, string companyId, string plantId, string plantName, string date, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel)
        {
            var excelEngine = new ExcelEngine();
            var oRU = new ReportUtility();
            var workbook = oRU.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            var row = 6;
            var headreColIndex = 1;
            var mainColIndex = 1;
            var dsLocal = GetBalanceSheetInfo(companyGroupId, companyId, plantId, date, isBudgetLevel, isActivityLevel, isACGroupLevel);
            var dsLocalPL = GetBalanceSheetInfoPL(companyGroupId, companyId, plantId, date, isBudgetLevel, isActivityLevel, isACGroupLevel);
            var dvParallelCurrency = new DataView(dsLocal)
            {
                Sort = "CurrencyCode ASC"
            };
            var dtParallelCurrency = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");

            var dvDr = new DataView(dsLocal)
            {
                RowFilter = "MainHead='Equity' OR  MainHead='Liability'",
                Sort = "MainHead desc, AccountCode"
            };
            var dtDr = dvDr.ToTable();

            var dvCr = new DataView(dsLocal)
            {
                RowFilter = "MainHead='Asset'",
                Sort = "AccountCode"
            };
            var dtCr = dvCr.ToTable();

            if (dsLocal.Rows.Count > 0)
            {
                row++;
                if (isACGroupLevel)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Account Group", 36); headreColIndex++;
                }

                oRU.SetHeaderText(ref sheet, row, headreColIndex, "GL", 36); headreColIndex++;
                if (isBudgetLevel)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Budget", 36); headreColIndex++;
                }
                if (isActivityLevel)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Activity", 36); headreColIndex++;
                }
                oRU.SetHeaderText(ref sheet, row, headreColIndex, dsLocal.Rows[0]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);

                //row++;
                // oRU.SetText(ref sheet, row, 1, "Total Assets:", true);
                var Row_Total_Start = row + 1;
                var RowTotal_current = row;
                var Row_Total_End = 0;
                var sumdrcrCol1 = 0;

                for (int i = 0; i < dtCr.Rows.Count; i++)
                {
                    
                    
                        row++;
                        mainColIndex = 1;
                        if (isACGroupLevel)
                        {
                            oRU.SetText(ref sheet, row, mainColIndex, dtCr.Rows[i]["Level"].ToString()); mainColIndex++;
                        }
                        oRU.SetText(ref sheet, row, mainColIndex, dtCr.Rows[i]["AccountCode"] + " - " + dtCr.Rows[i]["GL"]); mainColIndex++;
                        if (isBudgetLevel)
                        {
                            oRU.SetText(ref sheet, row, mainColIndex, dtCr.Rows[i]["Budget"].ToString()); mainColIndex++;
                        }
                        if (isActivityLevel)
                        {
                            oRU.SetText(ref sheet, row, mainColIndex, dtCr.Rows[i]["Activity"].ToString()); mainColIndex++;
                        }
                        oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(dtCr.Rows[i]["DRcumulative"].ToString()));
                    
                }
                sumdrcrCol1 = mainColIndex;
                Row_Total_End = row;
                //TotalRevenue_BalanceSheet(ref sheet, oRU, dtParallelCurrency, sumdrcrCol1, RowTotal_current, TotalAssetSumRow,  Row_Total_Start, Row_Total_End);
                row++;
                int TotalAssetSumRow = row;

                TotalRevenue_BalanceSheet(ref sheet, oRU, dtParallelCurrency, sumdrcrCol1, RowTotal_current, TotalAssetSumRow, Row_Total_Start, Row_Total_End);

                oRU.SetText(ref sheet, row, 1, "Total Assets:", true);
                //sheet.Range[TotalAssetSumRow, mainColIndex].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                //sheet.Range[TotalAssetSumRow, mainColIndex].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Double;
                row++;
                //row++;


                //oRU.SetText(ref sheet, row, 1, "Equity & Liability:", true);
                var Row_Total_Start2 = row + 1;
                var RowTotal_current2 = row;
                var Row_Total_End2 = 0;
                var sumdrcrCol2 = 0;
                for (int i = 0; i < dtDr.Rows.Count; i++)
                {   
                        row++;
                        mainColIndex = 1;
                        if (isACGroupLevel)
                        {
                            oRU.SetText(ref sheet, row, mainColIndex, dtDr.Rows[i]["Level"].ToString()); mainColIndex++;
                        }
                        oRU.SetText(ref sheet, row, mainColIndex, dtDr.Rows[i]["AccountCode"] + " - " + dtDr.Rows[i]["GL"]); mainColIndex++;
                        if (isBudgetLevel)
                        {
                            oRU.SetText(ref sheet, row, mainColIndex, dtDr.Rows[i]["Budget"].ToString()); mainColIndex++;
                        }
                        if (isActivityLevel)
                        {
                            oRU.SetText(ref sheet, row, mainColIndex, dtDr.Rows[i]["Activity"].ToString()); mainColIndex++;
                        }

                        oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(dtDr.Rows[i]["CRcumulative"].ToString()));
                    
                }
                Row_Total_End2 = row;
                sumdrcrCol2 = mainColIndex;
                row++;
                var dvTotPL = new DataView(dsLocalPL.Tables[0]);
                var dtTotPL = dvTotPL.ToTable();
                if (dtTotPL.Rows.Count != 0)
                {
                    sheet.Range[row, 1].Text = "Current Profit/Loss ";
                    sheet.Range[row, 1].CellStyle.Font.Italic = true;
                    sheet.Range[row, 1].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[row, 1].BorderAround(ExcelLineStyle.Hair);
                }
                if (dtTotPL.Rows.Count != 0)
                {
                    oRU.SetText(ref sheet, row, sumdrcrCol2, Convert.ToDouble(dtTotPL.Rows[0]["TotalPL"].ToString()));
                    sheet.Range[row, sumdrcrCol2].CellStyle.Font.Italic = true;
                    sheet.Range[row, sumdrcrCol2].CellStyle.Font.Bold = true;
                    sheet.Range[row, sumdrcrCol2].BorderAround(ExcelLineStyle.Hair);
                }
                //TotalExpense_BalanceSheet(ref sheet, oRU, dtParallelCurrency, sumdrcrCol2, RowTotal_current2, Row_Total_Start2, Row_Total_End2);
                var colLast = sumdrcrCol2;
                sheet.Range[8, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[8, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row++;
                row++;
                int TotalEquityandLiabilitySumRow = row;

                TotalExpense_BalanceSheet(ref sheet, oRU, dtParallelCurrency, sumdrcrCol2, RowTotal_current2, TotalEquityandLiabilitySumRow, Row_Total_Start2, Row_Total_End2);


                oRU.SetText(ref sheet, row, 1, "Total Equity & Liability:", true);

                sheet.Range[TotalAssetSumRow, mainColIndex].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet.Range[TotalAssetSumRow, mainColIndex].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Double;

                sheet.Range[TotalEquityandLiabilitySumRow, mainColIndex].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet.Range[TotalEquityandLiabilitySumRow, mainColIndex].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Double;


                sheet.Name = "Sheet";
                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                oRU.CompanyPlantHeader(ref sheet, colLast, "Balance Sheet", companyId, plantId, plantName, null);
                oRU.SetText(ref sheet, 5, colLast, "As On " + date + "", ExcelHAlign.HAlignCenter);
                sheet.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(colLast) + 5].Merge();
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.Name = "Sheet";
                oRU.CompanyPlantHeader(ref sheet, 5, "Balance Sheet", companyId, plantId, plantName, null);
                oRU.SetText(ref sheet, 5, 3, "No Data Found !", ExcelHAlign.HAlignCenter);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }
        private static void TotalRevenue_BalanceSheet(ref IWorksheet sheet, ReportUtility reportUtility, DataTable dtParallelCurrency, int sumdrcrCol1, int RowTotal_current, int TotalAssetSumRow, int Row_Total_Start, int Row_total_End)
        {
            for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
            {
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(sumdrcrCol1) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(sumdrcrCol1) + Row_total_End + ")";
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].CellStyle.Font.Bold = true;
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].BorderAround(ExcelLineStyle.Hair);
            }
        }
        private static void TotalExpense_BalanceSheet(ref IWorksheet sheet, ReportUtility reportUtility, DataTable dtParallelCurrency, int sumdrcrCol2, int RowTotal_current2, int TotalEquityandLiabilitySumRow, int Row_Total_Start2, int Row_Total_End2)
        {
            var Row_Total_End3 = Row_Total_End2 + 1;
            for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
            {
                sheet.Range[TotalEquityandLiabilitySumRow, sumdrcrCol2].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(sumdrcrCol2) + Row_Total_Start2 + ":" + reportUtility.GetColumnNameForXls(sumdrcrCol2) + Row_Total_End2 + ")" + "+" + reportUtility.GetColumnNameForXls(sumdrcrCol2) + Row_Total_End3 + "";
                sheet.Range[TotalEquityandLiabilitySumRow, sumdrcrCol2].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[TotalEquityandLiabilitySumRow, sumdrcrCol2].CellStyle.Font.Bold = true;
                sheet.Range[TotalEquityandLiabilitySumRow, sumdrcrCol2].BorderAround(ExcelLineStyle.Hair);
            }
        }
    
        private DataTable GetBalanceSheetInfo(string companyGroupId, string companyId, string plantId, string date, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel)
        {
            if (isActivityLevel)
            {
                var cmdText = @"select * FROM (SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode
                              , sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
                                , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode, VD.BudgetMasterId, BM.RefNo+' - '+BUD.UserName AS Budget
                                , A.UserName AS Activity, A.Id as ActivityId
	                            FROM TRN.VoucherDetailCurrency AS VDC
		                        INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                        INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                        LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                        LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                LEFT JOIN MST.BudgetMaster BM ON BM.Id=VD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                                LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                                WHERE act.IsBalanceSheet=1 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"'
                                AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                AND V.IsPark=0
                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code,
VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id, VD.BudgetMasterId, BM.RefNo, BUD.UserName, A.UserName, A.Id
) AS K where k.DRcumulative<>0  OR 	k.CRcumulative<>0";
                return _sqlRepository.GetDataTable(cmdText);
            }
            else if (isBudgetLevel && !isActivityLevel)
            {
                var cmdText = @"select * FROM (SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode
                              , sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
                                , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode, VD.BudgetMasterId, BM.RefNo+' - '+BUD.UserName AS Budget
	                            FROM TRN.VoucherDetailCurrency AS VDC
		                        INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                        INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                        LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                        LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                LEFT JOIN MST.BudgetMaster BM ON BM.Id=VD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                                WHERE act.IsBalanceSheet=1 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"'
                                AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                AND V.IsPark=0
                                GROUP BY  GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id, VD.BudgetMasterId, BM.RefNo, BUD.UserName
) AS K where k.DRcumulative<>0  OR 	k.CRcumulative<>0";
                return _sqlRepository.GetDataTable(cmdText);
            }
            else
            {
                var cmdText = @"select * FROM (SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
								sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id,  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
                                , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode
	                            FROM TRN.VoucherDetailCurrency AS VDC
		                        INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                        INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                        LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                        LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                WHERE act.IsBalanceSheet=1 AND v.PostingDate <='" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"'
                                AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                AND V.IsPark=0
                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id

			) AS K where k.DRcumulative<>0  OR 	k.CRcumulative<>0";
                return _sqlRepository.GetDataTable(cmdText);
            }
        }

        private DataSet GetBalanceSheetInfoPL(string companyGroupId, string companyId, string plantId, string date, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                if (isActivityLevel)
                {
                    parameters.CmdText = @"SELECT 	GL.Id AS AccountCodeId,
                                            Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                    sum(VDC.DrAmount) as DrAmount,
		                                    sum(VDC.CrAmount) as CrAmount,
                                            sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
											sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                                            sum(CASE WHEN ACT.BalanceType = 'Credit'  THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId)
												 -sum(CASE WHEN ACT.BalanceType = 'Debit'  THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END)
												 over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) AS TotalPL,
											ACT.BalanceType,
                                            ACT.Id AS [MainHead],
											AG.UserName AS [Level],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode,
                                            VD.BudgetMasterId,
                                            A.Id AS ActivityId,
		                                    BUD.UserName AS Budget,
                                            A.UserName AS Activity
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                            LEFT JOIN [MST].[BudgetMaster] AS BUDM ON BUDM.Id = VD.BudgetMasterId
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUDM.BudgetId
                                            LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                                            WHERE act.IsBalanceSheet=0 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"'
                                            AND V.PlantId='" + plantId + @"'
                                            AND v.IsPark=0
                                            group by GL.Id, GL.AccountCode,VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName,GL.AccountCode,v.PostingDate,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName, A.UserName,A.Id";
                    return _sqlRepository.GetGridData(parameters).Source;
                }
                else if (isBudgetLevel && !isActivityLevel)
                {
                    parameters.CmdText = @"SELECT 	GL.Id AS AccountCodeId,
                                            Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                    sum(VDC.DrAmount) as DrAmount,
		                                    sum(VDC.CrAmount) as CrAmount,
                                            sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
											sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                                            sum(CASE WHEN ACT.BalanceType = 'Credit'  THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId)
												 -sum(CASE WHEN ACT.BalanceType = 'Debit'  THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END)
												 over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) AS TotalPL,
											ACT.BalanceType,
                                            ACT.Id AS [MainHead],
											AG.UserName AS [Level],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                            LEFT JOIN [MST].[BudgetMaster] AS BUDM ON BUDM.Id = VD.BudgetMasterId
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUDM.BudgetId
                                            WHERE act.IsBalanceSheet=0 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"'
                                            AND V.PlantId='" + plantId + @"'
                                            AND v.IsPark=0
                                            group by GL.Id, GL.AccountCode,VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName,GL.AccountCode,v.PostingDate,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName";
                    return _sqlRepository.GetGridData(parameters).Source;
                }
                else
                {
                    parameters.CmdText = @"SELECT 	GL.Id AS AccountCodeId,
                                            Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                    sum(VDC.DrAmount) as DrAmount,
		                                    sum(VDC.CrAmount) as CrAmount,
                                            sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
											sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                                            sum(CASE WHEN ACT.BalanceType = 'Credit'  THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId)
												 -sum(CASE WHEN ACT.BalanceType = 'Debit'  THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END)
												 over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) AS TotalPL,
											ACT.BalanceType,
                                            ACT.Id AS [MainHead],
											AG.UserName AS [Level],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                            WHERE act.IsBalanceSheet=0 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"'
                                            AND V.PlantId='" + plantId + @"'
                                            AND v.IsPark=0
                                            group by GL.Id, GL.AccountCode,VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName,GL.AccountCode,v.PostingDate,ACT.BalanceType,AG.UserName,ACT.Id";
                    return _sqlRepository.GetGridData(parameters).Source;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region Balance sheet extent

        [HttpGet, Authorize]
        public ActionResult BalanceSheetExtentReport(ReportFormat reportFormat, string date/*, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel*/)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = GetBalanceSheetExtentReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, date/*, isBudgetLevel, isActivityLevel, isACGroupLevel*/);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Balance Sheet GroupWise";
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

        public IWorkbook GetBalanceSheetExtentReport(string companyGroupId, string companyId, string plantId, string plantName, string date/*, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel*/)
        {
            var excelEngine = new ExcelEngine();
            var oRU = new ReportUtility();
            var workbook = oRU.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            var row = 6;

            var headreColIndex = 1;
            var mainColIndex = 1;
            DataTable dtLocal = GetBalanceSheetExtentInfo(companyGroupId, companyId, plantId, date/*, isBudgetLevel, isActivityLevel, isACGroupLevel*/);
            var dsLocalPL = GetBalanceSheetInfoExtentPL(companyGroupId, companyId, plantId, date/*, isBudgetLevel, isActivityLevel, isACGroupLevel*/);

    
            DataView dvDr = new DataView(dtLocal)
            {
                RowFilter = "MainHead='Equity' OR  MainHead='Liability'",
                Sort = "MainHead desc, AccountCode"
            };
            DataTable dtDr = dvDr.ToTable();

            DataView dvCr = new DataView(dtLocal)
            {
                RowFilter = "MainHead='Asset'",
                Sort = "AccountCode"
            };
            DataTable dtCr = dvCr.ToTable();

            if (dtLocal.Rows.Count > 0)
            {
                row++;
                int colTitle = headreColIndex;
                oRU.SetHeaderTextBL(ref sheet, row, colTitle, "", 36);
                headreColIndex++;
                int colNotes = headreColIndex;
                oRU.SetHeaderTextBL(ref sheet, row, colNotes, "NOTES", 15);


                // oRU.SetText(ref sheet, row, 3, dtLocal.Rows[0]["" + date + ""].ToString());

                headreColIndex++;
                int colDateYear = headreColIndex;
                var hiestDate1 = date;
                var hiestDate2 = hiestDate1.Substring(3);

                oRU.SetHeaderTextBL(ref sheet, row, colDateYear, "" + hiestDate2 + "", ExcelHAlign.HAlignCenter);




                //if (isACGroupLevel)
                //{
                //    oRU.SetHeaderTextBL(ref sheet, row, headreColIndex, "Account Group", 36); headreColIndex++;
                //}

                ////oRU.SetHeaderTextBL(ref sheet, row, headreColIndex, "GL", 36); headreColIndex++;
                //if (isBudgetLevel)
                //{
                //    oRU.SetHeaderTextBL(ref sheet, row, headreColIndex, "Budget", 36); headreColIndex++;
                //}
                //if (isActivityLevel)
                //{
                //    oRU.SetHeaderTextBL(ref sheet, row, headreColIndex, "Activity", 36); headreColIndex++;
                //}
                //oRU.SetHeaderTextBL(ref sheet, row, headreColIndex, dtLocal.Rows[0]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);

                //row++;
                //oRU.SetText(ref sheet, row, 1, "Total Assets:", true);
                row++;
                oRU.SetHeaderTextBL(ref sheet, row, 1, "Assets", 10);


                var sumdrcrCol1 = 0;
                row++;
                int levelStartRow = 0;
                int sumStrRow = row;

                string totalAssetFormaula = "";

                DataTable dtcrLevel = dtCr.DefaultView.ToTable(true, "Level");
                for (int il = 0; il < dtcrLevel.Rows.Count; il++)
                {
                    oRU.SetText(ref sheet, row, 1, "" + dtcrLevel.Rows[il]["Level"].ToString() + " :", true);
                    levelStartRow = row;

                    dtCr.DefaultView.RowFilter = "Level ='" + dtcrLevel.Rows[il]["Level"].ToString() + "'";


                    for (int i = 0; i < dtCr.DefaultView.Count; i++)
                    {
                        row++;
                        sumStrRow = row;
                        mainColIndex = 1;
                        //if (isACGroupLevel)
                        //{
                        //    oRU.SetText(ref sheet, row, mainColIndex, dtCr.DefaultView[i]["Level"].ToString()); mainColIndex++;
                        //}
                        oRU.SetText(ref sheet, row, mainColIndex, dtCr.DefaultView[i]["AccountCode"] + " - " + dtCr.DefaultView[i]["GL"]); mainColIndex++;
                        //if (isBudgetLevel)
                        //{
                        //    oRU.SetText(ref sheet, row, mainColIndex, dtCr.DefaultView[i]["Budget"].ToString()); mainColIndex++;
                        //}
                        //if (isActivityLevel)
                        //{
                        //    oRU.SetText(ref sheet, row, mainColIndex, dtCr.DefaultView[i]["Activity"].ToString()); mainColIndex++;
                        //}
                        mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(dtCr.DefaultView[i]["DRcumulative"].ToString()));



                    }
                    sheet.Range[levelStartRow + 1, mainColIndex, row, mainColIndex].BorderAround(ExcelLineStyle.Thin);

                    row++;
                    sheet[levelStartRow, mainColIndex].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(mainColIndex) + (levelStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(mainColIndex) + row + ")";
                    sheet.Range[levelStartRow, mainColIndex].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[levelStartRow, mainColIndex].CellStyle.Font.Bold = true;
                    // sheet.Range[levelStartRow, mainColIndex].BorderAround(ExcelLineStyle.Hair);
                    totalAssetFormaula += clsStaticInfo.GetxlsCol(mainColIndex) + (levelStartRow) + "+";
                    row++;
                }
                sumdrcrCol1 = mainColIndex;

                totalAssetFormaula = totalAssetFormaula.Remove(totalAssetFormaula.Length - 1);
                // Row_Total_End = row;

                #region TotalCalculation

                #endregion

                int TotalAssetSumRow = row;
                oRU.SetText(ref sheet, TotalAssetSumRow, 1, "Total Assets:", true);

                row = TotalAssetSumRow;
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].Formula = totalAssetFormaula;
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].CellStyle.Font.Bold = true;
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Double;

                row++;
                row++;
                oRU.SetText(ref sheet, row, 1, "Equity & Liabilities", true);

                var sumdrcrCol2 = 0;
                row++;
                row++;
                int levelLiabilityStartRow = 0;

                string totalLiabilityFormaula = "(";

                DataTable dtdrLevel = dtDr.DefaultView.ToTable(true, "Level");
                for (int il = 0; il < dtdrLevel.Rows.Count; il++)
                {
                    oRU.SetText(ref sheet, row, 1, "" + dtdrLevel.Rows[il]["Level"].ToString() + " :", true);

                    levelLiabilityStartRow = row;
                    dtDr.DefaultView.RowFilter = "Level ='" + dtdrLevel.Rows[il]["Level"].ToString() + "'";

                    for (int i = 0; i < dtDr.DefaultView.Count; i++)
                    {
                        row++;
                        mainColIndex = 1;
                        //if (isACGroupLevel)
                        //{
                        //    oRU.SetText(ref sheet, row, mainColIndex, dtDr.DefaultView[i]["Level"].ToString()); mainColIndex++;
                        //}
                        oRU.SetText(ref sheet, row, mainColIndex, dtDr.DefaultView[i]["AccountCode"] + " - " + dtDr.DefaultView[i]["GL"]); mainColIndex++;
                        //if (isBudgetLevel)
                        //{
                        //    oRU.SetText(ref sheet, row, mainColIndex, dtDr.DefaultView[i]["Budget"].ToString()); mainColIndex++;
                        //}
                        //if (isActivityLevel)
                        //{
                        //    oRU.SetText(ref sheet, row, mainColIndex, dtDr.DefaultView[i]["Activity"].ToString()); mainColIndex++;
                        //}
                        mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(dtDr.DefaultView[i]["CRcumulative"].ToString()));
                        //sheet.Range[row, mainColIndex].BorderAround(ExcelLineStyle.Hair);
                        //sheet.Range[row, mainColIndex].BorderNone(ExcelLineStyle.Hair);

                    }
                    sheet.Range[levelLiabilityStartRow + 1, mainColIndex, row, mainColIndex].BorderAround(ExcelLineStyle.Thin);

                    row++;

                    sheet[levelLiabilityStartRow, mainColIndex].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(mainColIndex) + (levelLiabilityStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(mainColIndex) + row + ")";
                    sheet.Range[levelLiabilityStartRow, mainColIndex].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[levelLiabilityStartRow, mainColIndex].CellStyle.Font.Bold = true;

                    totalLiabilityFormaula += clsStaticInfo.GetxlsCol(mainColIndex) + (levelLiabilityStartRow) + "+";
                    row++;
                }

                sumdrcrCol2 = mainColIndex;

                totalLiabilityFormaula = totalLiabilityFormaula.Remove(totalLiabilityFormaula.Length - 1);
                totalLiabilityFormaula += ")";



                var dvTotPL = new DataView(dsLocalPL.Tables[0]);
                var dtTotPL = dvTotPL.ToTable();
                if (dtTotPL.Rows.Count != 0)
                {
                    row--;
                    sheet.Range[row, 1].Text = "Current Profit/Loss ";
                    sheet.Range[row, 1].CellStyle.Font.Italic = true;
                    oRU.SetText(ref sheet, row, sumdrcrCol2, Convert.ToDouble(dtTotPL.Rows[0]["TotalPL"].ToString()));
                    sheet.Range[row, sumdrcrCol2].CellStyle.Font.Italic = true;
                    sheet.Range[row, sumdrcrCol2].CellStyle.Font.Bold = true;

                    sheet[levelLiabilityStartRow, mainColIndex].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(mainColIndex) + (levelLiabilityStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(mainColIndex) + row + ")";
                    sheet.Range[levelLiabilityStartRow, mainColIndex].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[levelLiabilityStartRow, mainColIndex].CellStyle.Font.Bold = true;


                }
                sheet.Range[levelLiabilityStartRow + 1, mainColIndex, row, mainColIndex].BorderAround(ExcelLineStyle.Thin);

                sheet.Range[levelLiabilityStartRow + 1, mainColIndex, row, mainColIndex].BorderAround(ExcelLineStyle.None);
                sheet.Range[levelLiabilityStartRow + 1, mainColIndex, row, mainColIndex].BorderInside(ExcelLineStyle.None);

                sheet.Range[levelLiabilityStartRow + 1, mainColIndex, row, mainColIndex].BorderAround(ExcelLineStyle.Thin);
                // sheet.Range[levelLiabilityStartRow + 1, mainColIndex, row, mainColIndex].Merge();
                //sheet.Range[oRU.GetColumnNameForXls(mainColIndex) + levelLiabilityStartRow + 1 + ":" + oRU.GetColumnNameForXls(mainColIndex) + row].Merge();

                var colLast = sumdrcrCol2;

                row++;
                row++;

                oRU.SetText(ref sheet, row, 1, "Total Equity & Liabilities:", true);
                int TotalLiabilitySumRow = row;

                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].Formula = totalLiabilityFormaula;
                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].CellStyle.Font.Bold = true;
                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Double;

                //sheet.Range[row, 1].BorderAround(ExcelLineStyle.None);
                sheet.Name = "Sheet";
                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                oRU.CompanyPlantHeader(ref sheet, colLast, "Balance Sheet", companyId, plantId, plantName, null);
                oRU.SetText(ref sheet, 5, colLast, "As On " + date + "", ExcelHAlign.HAlignCenter);
                sheet.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(colLast) + 5].Merge();
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                sheet.ShowColumn(2, false);

            }
            else
            {
                sheet.Name = "Sheet";
                oRU.CompanyPlantHeader(ref sheet, 5, "Balance Sheet", companyId, plantId, plantName, null);
                oRU.SetText(ref sheet, 5, 3, "No Data Found !", ExcelHAlign.HAlignCenter);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);

            }
            return workbook;
        }



        private DataTable GetBalanceSheetExtentInfo(string companyGroupId, string companyId, string plantId, string date/*, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel*/)
        {
            
            var cmdText = @"select * FROM (SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
								sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id,  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
                                , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode
	                            FROM TRN.VoucherDetailCurrency AS VDC
		                        INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                        INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                        LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                        LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                WHERE act.IsBalanceSheet=1 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"'
                                AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                AND V.IsPark=0
                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id
                            ) AS K where k.DRcumulative<>0  OR 	k.CRcumulative<>0";
            return _sqlRepository.GetDataTable(cmdText);
            //}
        }

        private DataSet GetBalanceSheetInfoExtentPL(string companyGroupId, string companyId, string plantId, string date/*, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel*/)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
              
                parameters.CmdText = @"SELECT 	GL.Id AS AccountCodeId,
                                            Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                    sum(VDC.DrAmount) as DrAmount,
		                                    sum(VDC.CrAmount) as CrAmount,
                                            sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
											sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                                            sum(CASE WHEN ACT.BalanceType = 'Credit'  THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId)
												 -sum(CASE WHEN ACT.BalanceType = 'Debit'  THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END)
												 over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) AS TotalPL,
											ACT.BalanceType,
                                            ACT.Id AS [MainHead],
											AG.UserName AS [Level],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                            WHERE act.IsBalanceSheet=0 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"'
                                            AND V.PlantId='" + plantId + @"'
                                            AND v.IsPark=0
                                            group by GL.Id, GL.AccountCode,VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName,GL.AccountCode,v.PostingDate,ACT.BalanceType,AG.UserName,ACT.Id";
                return _sqlRepository.GetGridData(parameters).Source;
                // }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion balance sheet extent


        #region BL DATE RANGE

        [HttpGet, Authorize]
        public ActionResult balanceSheetreportDateWise(ReportFormat reportFormat, string fromDate, string toDate/*, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel*/)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = GetBalanceSheetDateRangeReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate/*, isBudgetLevel, isActivityLevel, isACGroupLevel*/);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Balance Sheet GroupWise";
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

        public IWorkbook GetBalanceSheetDateRangeReport(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate)
        {
            var excelEngine = new ExcelEngine();
            var oRU = new ReportUtility();
            var workbook = oRU.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            var row = 6;

            var headreColIndex = 1;
            var mainColIndex = 1;
            var mainColIndexTodate = 1;

            //    SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
            //sum(CASE WHEN ACT.BalanceType = 'Debit' THEN(sum(VDC.DrAmount) - sum(VDC.CrAmount)) ELSE 0 END) over(partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
            //                        , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN(sum(VDC.CrAmount) - sum(VDC.DrAmount)) ELSE 0 END) over(partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
            //                        , ACT.BalanceType, ACT.Id AS[MainHead], AG.UserName AS[Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode
            //                      FROM TRN.VoucherDetailCurrency AS VDC
            //                      INNER JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId

            //                        INNER JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId

            //                        LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id = VD.GLGeneralInfoId
            //                        LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId
            //                        left outer join[HKP].[AccountType] act on act.Id = AG.AccountTypeId

            //                        LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id = VDC.ParallelCurrencyId
            //                        WHERE act.IsBalanceSheet = 1 AND v.PostingDate >= '" + fromDate + @"'   AND V.CompanyGroupId = '" + companyGroupId + @"'
            //                        AND V.CompanyId = '" + companyId + @"' AND V.PlantId = '" + plantId + @"'
            //                        AND V.IsPark = 0
            //                        GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id

            DataTable DtFromDate = GetBalanceSheetDateRangeInfo(companyGroupId, companyId, plantId, fromDate/*, isBudgetLevel, isActivityLevel, isACGroupLevel*/);
            DataTable dtToDate = GetBalanceSheetToDateRangeInfo(companyGroupId, companyId, plantId, toDate/*, isBudgetLevel, isActivityLevel, isACGroupLevel*/);
            var dsLocalPL = GetBalanceSheetInfoDateRangePL(companyGroupId, companyId, plantId, fromDate/*, isBudgetLevel, isActivityLevel, isACGroupLevel*/);
            var dsLocalToDatePL = GetBalanceSheetInfoToDateRangePL(companyGroupId, companyId, plantId, toDate/*, isBudgetLevel, isActivityLevel, isACGroupLevel*/);

            DataTable dtLocal = DtFromDate.DefaultView.ToTable();
            dtLocal.Merge(dtToDate);

            dtLocal = dtLocal.DefaultView.ToTable(true, "AccountCodeId", "BalanceType", "MainHead", "Level", "GLGeneralInfoId", "GL", "AccountCode");

            DataView dvDr = new DataView(dtLocal)
            {
                RowFilter = "MainHead='Equity' OR  MainHead='Liability'",
                Sort = "MainHead desc, AccountCode"
            };
            DataTable dtDr = dvDr.ToTable();

            DataView dvCr = new DataView(dtLocal)
            {
                RowFilter = "MainHead='Asset'",
                Sort = "AccountCode"
            };
            DataTable dtCr = dvCr.ToTable();

            if (dtLocal.Rows.Count > 0)
            {
                row++;
                int colTitle = headreColIndex;
                oRU.SetHeaderTextBL(ref sheet, row, colTitle, "", 36);
                headreColIndex++;
                int colNotes = headreColIndex;
                oRU.SetHeaderTextBL(ref sheet, row, colNotes, "NOTES", 15);

                headreColIndex++;
                int colDateYear = headreColIndex;
                var hiestDate1 = fromDate;
                var hiestDate2 = hiestDate1.Substring(3);
                oRU.SetHeaderTextBL(ref sheet, row, colDateYear, "" + hiestDate2 + "", ExcelHAlign.HAlignCenter);

                headreColIndex++;
                int colToDateYear = headreColIndex;
                var hiestToDate = toDate;
                var hiestDate3 = hiestToDate.Substring(3);
                oRU.SetHeaderTextBL(ref sheet, row, colToDateYear, "" + hiestDate3 + "", ExcelHAlign.HAlignCenter);

                row++;
                oRU.SetHeaderTextBL(ref sheet, row, 1, "Assets", 10);

                var sumdrcrCol1 = 0;
                var sumdrcrColTodate = 0;
                row++;
                int levelStartRow = 0;
                int sumStrRow = row;

                string totalAssetFormaula = "";
                string totalAssetToDateFormaula = "";

                DataTable dtcrLevel = dtCr.DefaultView.ToTable(true, "Level");
                for (int il = 0; il < dtcrLevel.Rows.Count; il++)
                {
                    levelStartRow = row;
                    oRU.SetText(ref sheet, row, 1, "" + dtcrLevel.Rows[il]["Level"].ToString() + " :", true);

                    dtCr.DefaultView.RowFilter = "Level ='" + dtcrLevel.Rows[il]["Level"].ToString() + "'";

                    for (int i = 0; i < dtCr.DefaultView.Count; i++)
                    {
                        row++;
                        sumStrRow = row;
                        mainColIndex = 1;
                        mainColIndexTodate = 1;
                        oRU.SetText(ref sheet, row, mainColIndex, dtCr.DefaultView[i]["AccountCode"] + " - " + dtCr.DefaultView[i]["GL"]);
                        mainColIndex++;
                        mainColIndex++;
                        //oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(dtCr.DefaultView[i]["DRcumulative"].ToString()));
                        sheet.Range[row, mainColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;

                        //oRU.SetText(ref sheet, row, mainColIndexTodate, Convert.ToDouble(dtCr.DefaultView[i]["DRcumulative"].ToString()));
                        DtFromDate.DefaultView.RowFilter = "AccountCodeId='" + dtCr.DefaultView[i]["AccountCodeId"] + "'";
                        if (DtFromDate.DefaultView.Count > 0)
                        {
                            oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(DtFromDate.DefaultView[0]["DRcumulative"].ToString()));
                        }

                        mainColIndexTodate++;
                        mainColIndexTodate++;
                        mainColIndexTodate++;
                        dtToDate.DefaultView.RowFilter = "AccountCodeId='" + dtCr.DefaultView[i]["AccountCodeId"] + "'";
                        if (dtToDate.DefaultView.Count > 0)
                        {
                            oRU.SetText(ref sheet, row, mainColIndexTodate, Convert.ToDouble(dtToDate.DefaultView[0]["DRcumulative"].ToString()));
                        }


                    }
                    sheet.Range[levelStartRow + 1, mainColIndex, row, mainColIndex].BorderAround(ExcelLineStyle.Thin);
                    sheet.Range[levelStartRow + 1, mainColIndexTodate, row, mainColIndexTodate].BorderAround(ExcelLineStyle.Thin);

                    row++;
                    sheet[levelStartRow, mainColIndex].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(mainColIndex) + (levelStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(mainColIndex) + row + ")";
                    sheet.Range[levelStartRow, mainColIndex].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[levelStartRow, mainColIndex].CellStyle.Font.Bold = true;
                    // sheet.Range[levelStartRow, mainColIndex].BorderAround(ExcelLineStyle.Hair);
                    totalAssetFormaula += clsStaticInfo.GetxlsCol(mainColIndex) + (levelStartRow) + "+";

                    sheet[levelStartRow, mainColIndexTodate].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(mainColIndexTodate) + (levelStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(mainColIndexTodate) + row + ")";
                    sheet.Range[levelStartRow, mainColIndexTodate].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[levelStartRow, mainColIndexTodate].CellStyle.Font.Bold = true;
                    // sheet.Range[levelStartRow, mainColIndex].BorderAround(ExcelLineStyle.Hair);
                    totalAssetToDateFormaula += clsStaticInfo.GetxlsCol(mainColIndexTodate) + (levelStartRow) + "+";

                    row++;
                }
                sumdrcrCol1 = mainColIndex;
                sumdrcrColTodate = mainColIndexTodate;

                totalAssetFormaula = totalAssetFormaula.Remove(totalAssetFormaula.Length - 1);
                totalAssetToDateFormaula = totalAssetToDateFormaula.Remove(totalAssetToDateFormaula.Length - 1);

                int TotalAssetSumRow = row;
                oRU.SetText(ref sheet, TotalAssetSumRow, 1, "Total Assets:", true);

                row = TotalAssetSumRow;
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].Formula = totalAssetFormaula;
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].CellStyle.Font.Bold = true;
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Double;

                int TotalAssetToDateSumRow = row;
                row = TotalAssetToDateSumRow;
                sheet.Range[TotalAssetToDateSumRow, sumdrcrColTodate].Formula = totalAssetToDateFormaula;
                sheet.Range[TotalAssetToDateSumRow, sumdrcrColTodate].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[TotalAssetToDateSumRow, sumdrcrColTodate].CellStyle.Font.Bold = true;
                sheet.Range[TotalAssetToDateSumRow, sumdrcrColTodate].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet.Range[TotalAssetToDateSumRow, sumdrcrColTodate].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Double;

                row++;
                row++;
                oRU.SetText(ref sheet, row, 1, "Equity & Liabilities", true);

                var sumdrcrCol2 = 0;
                var sumdrcrColLiaTodate = 0;
                row++;
                row++;
                int levelLiabilityStartRow = 0;

                string totalLiabilityFormaula = "(";
                string totalLiabilityToDateFormaula = "(";

                DataTable dtdrLevel = dtDr.DefaultView.ToTable(true, "Level");
                for (int il = 0; il < dtdrLevel.Rows.Count; il++)
                {
                    oRU.SetText(ref sheet, row, 1, "" + dtdrLevel.Rows[il]["Level"].ToString() + " :", true);

                    levelLiabilityStartRow = row;
                    dtDr.DefaultView.RowFilter = "Level ='" + dtdrLevel.Rows[il]["Level"].ToString() + "'";

                    for (int i = 0; i < dtDr.DefaultView.Count; i++)
                    {
                        row++;
                        mainColIndex = 1;
                        mainColIndexTodate = 1;

                        oRU.SetText(ref sheet, row, mainColIndex, dtDr.DefaultView[i]["AccountCode"] + " - " + dtDr.DefaultView[i]["GL"]); mainColIndex++;
                        mainColIndex++;

                        //sheet.Range[row, mainColIndex].BorderAround(ExcelLineStyle.Hair);
                        //sheet.Range[row, mainColIndex].BorderNone(ExcelLineStyle.Hair);

                        DtFromDate.DefaultView.RowFilter = "AccountCodeId='" + dtDr.DefaultView[i]["AccountCodeId"] + "'";
                        if (DtFromDate.DefaultView.Count > 0)
                        {
                            oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(DtFromDate.DefaultView[0]["CRcumulative"].ToString()));
                        }

                        mainColIndexTodate++;
                        mainColIndexTodate++;
                        mainColIndexTodate++;
                        dtToDate.DefaultView.RowFilter = "AccountCodeId='" + dtDr.DefaultView[i]["AccountCodeId"] + "'";
                        if (dtToDate.DefaultView.Count > 0)
                        {
                            oRU.SetText(ref sheet, row, mainColIndexTodate, Convert.ToDouble(dtToDate.DefaultView[0]["CRcumulative"].ToString()));
                        }




                    }

                    sheet.Range[levelLiabilityStartRow + 1, mainColIndex, row, mainColIndex].BorderAround(ExcelLineStyle.Thin);
                    sheet.Range[levelLiabilityStartRow + 1, mainColIndexTodate, row, mainColIndexTodate].BorderAround(ExcelLineStyle.Thin);

                    row++;

                    sheet[levelLiabilityStartRow, mainColIndex].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(mainColIndex) + (levelLiabilityStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(mainColIndex) + row + ")";
                    sheet.Range[levelLiabilityStartRow, mainColIndex].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[levelLiabilityStartRow, mainColIndex].CellStyle.Font.Bold = true;

                    totalLiabilityFormaula += clsStaticInfo.GetxlsCol(mainColIndex) + (levelLiabilityStartRow) + "+";

                    sheet[levelLiabilityStartRow, mainColIndexTodate].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(mainColIndexTodate) + (levelLiabilityStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(mainColIndexTodate) + row + ")";
                    sheet.Range[levelLiabilityStartRow, mainColIndexTodate].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[levelLiabilityStartRow, mainColIndexTodate].CellStyle.Font.Bold = true;

                    totalLiabilityToDateFormaula += clsStaticInfo.GetxlsCol(mainColIndexTodate) + (levelLiabilityStartRow) + "+";
                    row++;
                }

                sumdrcrCol2 = mainColIndex;
                sumdrcrColLiaTodate = mainColIndexTodate;

                totalLiabilityFormaula = totalLiabilityFormaula.Remove(totalLiabilityFormaula.Length - 1);
                totalLiabilityFormaula += ")";

                totalLiabilityToDateFormaula = totalLiabilityToDateFormaula.Remove(totalLiabilityToDateFormaula.Length - 1);
                totalLiabilityToDateFormaula += ")";

                var dvTotPL = new DataView(dsLocalPL.Tables[0]);
                var dtTotPL = dvTotPL.ToTable();

                var dvTotToDatePL = new DataView(dsLocalToDatePL.Tables[0]);
                var dtTotToDatePL = dvTotToDatePL.ToTable();
                if (dtTotPL.Rows.Count != 0 || dtTotToDatePL.Rows.Count != 0)
                {
                    row--;
                    sheet.Range[row, 1].Text = "Current Profit/Loss";
                    sheet.Range[row, 1].CellStyle.Font.Italic = true;

                    if (dtTotPL.Rows.Count > 0)
                        oRU.SetText(ref sheet, row, sumdrcrCol2, Convert.ToDouble(dtTotPL.Rows[0]["TotalPL"].ToString()));

                    if (dtTotToDatePL.Rows.Count > 0)
                        oRU.SetText(ref sheet, row, mainColIndexTodate, Convert.ToDouble(dtTotToDatePL.Rows[0]["TotalPL"].ToString()));
                    // sheet.Range[row, sumdrcrCol2].CellStyle.Font.Italic = true;
                    sheet.Range[row, sumdrcrCol2].CellStyle.Font.Bold = true;
                    sheet.Range[row, mainColIndexTodate].CellStyle.Font.Bold = true;

                    sheet[levelLiabilityStartRow, mainColIndex].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(mainColIndex) + (levelLiabilityStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(mainColIndex) + row + ")";
                    sheet[levelLiabilityStartRow, mainColIndexTodate].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(mainColIndexTodate) + (levelLiabilityStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(mainColIndexTodate) + row + ")";
                    sheet.Range[levelLiabilityStartRow, mainColIndex].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[levelLiabilityStartRow, mainColIndex].CellStyle.Font.Bold = true;

                    //sheet[levelLiabilityStartRow, mainColIndexTodate].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(mainColIndexTodate) + (levelLiabilityStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(mainColIndexTodate) + row + ")";
                    //sheet.Range[levelLiabilityStartRow, mainColIndexTodate].NumberFormat = oRU.NumberFormatDecimalTwo();
                    //sheet.Range[levelLiabilityStartRow, mainColIndexTodate].CellStyle.Font.Bold = true;


                }


                //if (dtTotToDatePL.Rows.Count != 0)
                //{
                //    row--;
                //    sheet.Range[row, 1].Text = "Current Profit/Loss ";
                //    sheet.Range[row, 1].CellStyle.Font.Italic = true;
                //    //oRU.SetText(ref sheet, row, sumdrcrCol2, Convert.ToDouble(dtTotPL.Rows[0]["TotalPL"].ToString()));
                //    //sheet.Range[row, sumdrcrCol2].CellStyle.Font.Italic = true;
                //    //sheet.Range[row, sumdrcrCol2].CellStyle.Font.Bold = true;


                //    oRU.SetText(ref sheet, row, sumdrcrColLiaTodate, Convert.ToDouble(dtTotToDatePL.Rows[0]["TotalPL"].ToString()));
                //    sheet.Range[row, sumdrcrColLiaTodate].CellStyle.Font.Italic = true;
                //    sheet.Range[row, sumdrcrColLiaTodate].CellStyle.Font.Bold = true;


                //    //sheet[levelLiabilityStartRow, mainColIndex].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(mainColIndex) + (levelLiabilityStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(mainColIndex) + row + ")";
                //    //sheet.Range[levelLiabilityStartRow, mainColIndex].NumberFormat = oRU.NumberFormatDecimalTwo();
                //    //sheet.Range[levelLiabilityStartRow, mainColIndex].CellStyle.Font.Bold = true;

                //    sheet[levelLiabilityStartRow, mainColIndexTodate].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(mainColIndexTodate) + (levelLiabilityStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(mainColIndexTodate) + row + ")";
                //    sheet.Range[levelLiabilityStartRow, mainColIndexTodate].NumberFormat = oRU.NumberFormatDecimalTwo();
                //    sheet.Range[levelLiabilityStartRow, mainColIndexTodate].CellStyle.Font.Bold = true;


                //}


                sheet.Range[levelLiabilityStartRow + 1, mainColIndex, row, mainColIndex].BorderAround(ExcelLineStyle.Thin);

                sheet.Range[levelLiabilityStartRow + 1, mainColIndex, row, mainColIndex].BorderAround(ExcelLineStyle.None);
                sheet.Range[levelLiabilityStartRow + 1, mainColIndex, row, mainColIndex].BorderInside(ExcelLineStyle.None);
                sheet.Range[levelLiabilityStartRow + 1, mainColIndexTodate, row, mainColIndexTodate].BorderInside(ExcelLineStyle.None);

                sheet.Range[levelLiabilityStartRow + 1, mainColIndex, row, mainColIndex].BorderAround(ExcelLineStyle.Thin);
                sheet.Range[levelLiabilityStartRow + 1, mainColIndexTodate, row, mainColIndexTodate].BorderAround(ExcelLineStyle.Thin);
                // sheet.Range[levelLiabilityStartRow + 1, mainColIndex, row, mainColIndex].Merge();
                //sheet.Range[oRU.GetColumnNameForXls(mainColIndex) + levelLiabilityStartRow + 1 + ":" + oRU.GetColumnNameForXls(mainColIndex) + row].Merge();

                var colLast = sumdrcrCol2;

                row++;
                row++;

                oRU.SetText(ref sheet, row, 1, "Total Equity & Liabilities:", true);
                int TotalLiabilitySumRow = row;
                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].Formula = totalLiabilityFormaula;
                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].CellStyle.Font.Bold = true;
                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Double;


                int TotalLiabilityToDateSumRow = row;
                sheet.Range[TotalLiabilityToDateSumRow, sumdrcrColLiaTodate].Formula = totalLiabilityToDateFormaula;
                sheet.Range[TotalLiabilityToDateSumRow, sumdrcrColLiaTodate].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[TotalLiabilityToDateSumRow, sumdrcrColLiaTodate].CellStyle.Font.Bold = true;
                sheet.Range[TotalLiabilityToDateSumRow, sumdrcrColLiaTodate].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet.Range[TotalLiabilityToDateSumRow, sumdrcrColLiaTodate].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Double;

                //sheet.Range[row, 1].BorderAround(ExcelLineStyle.None);
                sheet.Name = "Sheet";
                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                oRU.CompanyPlantHeader(ref sheet, colLast, "Balance Sheet", companyId, plantName, null);
                oRU.SetText(ref sheet, 5, colLast, ("From " + fromDate + "" + " To " + toDate + " "), ExcelHAlign.HAlignCenter);
                sheet.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(colLast) + 5].Merge();
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                sheet.ShowColumn(2, false);

            }
            else
            {
                sheet.Name = "Sheet";
                oRU.CompanyHeader(ref sheet, 5, "Balance Sheet", companyId);
                oRU.SetText(ref sheet, 5, 3, "No Data Found !", ExcelHAlign.HAlignCenter);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);

            }
            return workbook;
        }
        private DataTable GetBalanceSheetDateRangeInfo(string companyGroupId, string companyId, string plantId, string fromDate /*, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel*/)
        {
            //if (isActivityLevel)
            //{
            //    var cmdText = @"SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode
            //                  , sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
            //                    , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
            //                    , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode, VD.BudgetMasterId, BM.RefNo+' - '+BUD.UserName AS Budget
            //                    , A.UserName AS Activity, A.Id as ActivityId
            //                 FROM TRN.VoucherDetailCurrency AS VDC
            //              INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
            //              INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
            //              LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
            //                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
            //                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
            //              LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
            //                    LEFT JOIN MST.BudgetMaster BM ON BM.Id=VD.BudgetMasterId
            //                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
            //                    LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
            //                    WHERE act.IsBalanceSheet=1 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"'
            //                    AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
            //                    AND V.IsPark=0
            //                    GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id, VD.BudgetMasterId, BM.RefNo, BUD.UserName, A.UserName, A.Id";
            //    return _sqlRepository.GetDataTable(cmdText);
            //}
            //else if (isBudgetLevel && !isActivityLevel)
            //{
            //    var cmdText = @"SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode
            //                  , sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
            //                    , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
            //                    , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode, VD.BudgetMasterId, BM.RefNo+' - '+BUD.UserName AS Budget
            //                 FROM TRN.VoucherDetailCurrency AS VDC
            //              INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
            //              INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
            //              LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
            //                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
            //                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
            //              LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
            //                    LEFT JOIN MST.BudgetMaster BM ON BM.Id=VD.BudgetMasterId
            //                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
            //                    WHERE act.IsBalanceSheet=1 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"'
            //                    AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
            //                    AND V.IsPark=0
            //                    GROUP BY  GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id, VD.BudgetMasterId, BM.RefNo, BUD.UserName";
            //    return _sqlRepository.GetDataTable(cmdText);
            //}
            //else
            //{
            var cmdText = @"SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
								sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id,  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
                                , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode
	                            FROM TRN.VoucherDetailCurrency AS VDC
		                        INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                        INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                        LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                        LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                WHERE act.IsBalanceSheet=1 AND v.PostingDate <= '" + fromDate + @"'   AND V.CompanyGroupId='" + companyGroupId + @"'
                                AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                AND V.IsPark=0
                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id";
            return _sqlRepository.GetDataTable(cmdText);
            //}
        }
        private DataTable GetBalanceSheetToDateRangeInfo(string companyGroupId, string companyId, string plantId, string toDate/*, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel*/)
        {
            //if (isActivityLevel)
            //{
            //    var cmdText = @"SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode
            //                  , sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
            //                    , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
            //                    , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode, VD.BudgetMasterId, BM.RefNo+' - '+BUD.UserName AS Budget
            //                    , A.UserName AS Activity, A.Id as ActivityId
            //                 FROM TRN.VoucherDetailCurrency AS VDC
            //              INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
            //              INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
            //              LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
            //                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
            //                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
            //              LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
            //                    LEFT JOIN MST.BudgetMaster BM ON BM.Id=VD.BudgetMasterId
            //                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
            //                    LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
            //                    WHERE act.IsBalanceSheet=1 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"'
            //                    AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
            //                    AND V.IsPark=0
            //                    GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id, VD.BudgetMasterId, BM.RefNo, BUD.UserName, A.UserName, A.Id";
            //    return _sqlRepository.GetDataTable(cmdText);
            //}
            //else if (isBudgetLevel && !isActivityLevel)
            //{
            //    var cmdText = @"SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode
            //                  , sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
            //                    , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
            //                    , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode, VD.BudgetMasterId, BM.RefNo+' - '+BUD.UserName AS Budget
            //                 FROM TRN.VoucherDetailCurrency AS VDC
            //              INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
            //              INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
            //              LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
            //                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
            //                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
            //              LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
            //                    LEFT JOIN MST.BudgetMaster BM ON BM.Id=VD.BudgetMasterId
            //                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
            //                    WHERE act.IsBalanceSheet=1 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"'
            //                    AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
            //                    AND V.IsPark=0
            //                    GROUP BY  GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id, VD.BudgetMasterId, BM.RefNo, BUD.UserName";
            //    return _sqlRepository.GetDataTable(cmdText);
            //}
            //else
            //{
            var cmdText = @"SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
								sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id,  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
                                , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode
	                            FROM TRN.VoucherDetailCurrency AS VDC
		                        INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                        INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                        LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                        LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                WHERE act.IsBalanceSheet=1 AND v.PostingDate <= '" + toDate + @"'  AND V.CompanyGroupId='" + companyGroupId + @"'
                                AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                AND V.IsPark=0
                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id";
            return _sqlRepository.GetDataTable(cmdText);
            //}
        }
        private DataSet GetBalanceSheetInfoDateRangePL(string companyGroupId, string companyId, string plantId, string fromDate/*, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel*/)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                //     if (isActivityLevel)
                //     {
                //         parameters.CmdText = @"SELECT 	GL.Id AS AccountCodeId,
                //                                 Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
                //                           VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
                //                           sum(VDC.DrAmount) as DrAmount,
                //                           sum(VDC.CrAmount) as CrAmount,
                //                                 sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
                //sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                //                                 sum(CASE WHEN ACT.BalanceType = 'Credit'  THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId)
                //	 -sum(CASE WHEN ACT.BalanceType = 'Debit'  THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END)
                //	 over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) AS TotalPL,
                //ACT.BalanceType,
                //                                 ACT.Id AS [MainHead],
                //AG.UserName AS [Level],
                //                           VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode,
                //                                 VD.BudgetMasterId,
                //                                 A.Id AS ActivityId,
                //                           BUD.UserName AS Budget,
                //                                 A.UserName AS Activity
                //                              FROM TRN.VoucherDetailCurrency AS VDC
                //                           INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                //                           INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                //                           LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                //                                 LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                //                                 left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                //                           LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                //                                 LEFT JOIN [MST].[BudgetMaster] AS BUDM ON BUDM.Id = VD.BudgetMasterId
                //                                 LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUDM.BudgetId
                //                                 LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                //                                 WHERE act.IsBalanceSheet=0 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"'
                //                                 AND V.PlantId='" + plantId + @"'
                //                                 AND v.IsPark=0
                //                                 group by GL.Id, GL.AccountCode,VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName,GL.AccountCode,v.PostingDate,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName, A.UserName,A.Id";
                //         return _sqlRepository.GetGridData(parameters).Source;
                //     }
                //     else if (isBudgetLevel && !isActivityLevel)
                //     {
                //         parameters.CmdText = @"SELECT 	GL.Id AS AccountCodeId,
                //                                 Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
                //                           VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
                //                           sum(VDC.DrAmount) as DrAmount,
                //                           sum(VDC.CrAmount) as CrAmount,
                //                                 sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
                //sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                //                                 sum(CASE WHEN ACT.BalanceType = 'Credit'  THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId)
                //	 -sum(CASE WHEN ACT.BalanceType = 'Debit'  THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END)
                //	 over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) AS TotalPL,
                //ACT.BalanceType,
                //                                 ACT.Id AS [MainHead],
                //AG.UserName AS [Level],
                //                           VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode,
                //                                 VD.BudgetMasterId,
                //                           BUD.UserName AS Budget
                //                              FROM TRN.VoucherDetailCurrency AS VDC
                //                           INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                //                           INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                //                           LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                //                                 LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                //                                 left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                //                           LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                //                                 LEFT JOIN [MST].[BudgetMaster] AS BUDM ON BUDM.Id = VD.BudgetMasterId
                //                                 LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUDM.BudgetId
                //                                 WHERE act.IsBalanceSheet=0 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"'
                //                                 AND V.PlantId='" + plantId + @"'
                //                                 AND v.IsPark=0
                //                                 group by GL.Id, GL.AccountCode,VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName,GL.AccountCode,v.PostingDate,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName";
                //         return _sqlRepository.GetGridData(parameters).Source;
                //     }
                //     else
                //     {
                parameters.CmdText = @"SELECT 	GL.Id AS AccountCodeId,
                                            Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                    sum(VDC.DrAmount) as DrAmount,
		                                    sum(VDC.CrAmount) as CrAmount,
                                            sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
											sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                                            sum(CASE WHEN ACT.BalanceType = 'Credit'  THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId)
												 -sum(CASE WHEN ACT.BalanceType = 'Debit'  THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END)
												 over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) AS TotalPL,
											ACT.BalanceType,
                                            ACT.Id AS [MainHead],
											AG.UserName AS [Level],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                            WHERE act.IsBalanceSheet=0 AND v.PostingDate <= '" + fromDate + @"'  AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"'
                                            AND V.PlantId='" + plantId + @"'
                                            AND v.IsPark=0
                                            group by GL.Id, GL.AccountCode,VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName,GL.AccountCode,v.PostingDate,ACT.BalanceType,AG.UserName,ACT.Id";
                return _sqlRepository.GetGridData(parameters).Source;
                // }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private DataSet GetBalanceSheetInfoToDateRangePL(string companyGroupId, string companyId, string plantId, string toDate/*, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel*/)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                //     if (isActivityLevel)
                //     {
                //         parameters.CmdText = @"SELECT 	GL.Id AS AccountCodeId,
                //                                 Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
                //                           VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
                //                           sum(VDC.DrAmount) as DrAmount,
                //                           sum(VDC.CrAmount) as CrAmount,
                //                                 sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
                //sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                //                                 sum(CASE WHEN ACT.BalanceType = 'Credit'  THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId)
                //	 -sum(CASE WHEN ACT.BalanceType = 'Debit'  THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END)
                //	 over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) AS TotalPL,
                //ACT.BalanceType,
                //                                 ACT.Id AS [MainHead],
                //AG.UserName AS [Level],
                //                           VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode,
                //                                 VD.BudgetMasterId,
                //                                 A.Id AS ActivityId,
                //                           BUD.UserName AS Budget,
                //                                 A.UserName AS Activity
                //                              FROM TRN.VoucherDetailCurrency AS VDC
                //                           INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                //                           INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                //                           LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                //                                 LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                //                                 left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                //                           LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                //                                 LEFT JOIN [MST].[BudgetMaster] AS BUDM ON BUDM.Id = VD.BudgetMasterId
                //                                 LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUDM.BudgetId
                //                                 LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                //                                 WHERE act.IsBalanceSheet=0 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"'
                //                                 AND V.PlantId='" + plantId + @"'
                //                                 AND v.IsPark=0
                //                                 group by GL.Id, GL.AccountCode,VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName,GL.AccountCode,v.PostingDate,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName, A.UserName,A.Id";
                //         return _sqlRepository.GetGridData(parameters).Source;
                //     }
                //     else if (isBudgetLevel && !isActivityLevel)
                //     {
                //         parameters.CmdText = @"SELECT 	GL.Id AS AccountCodeId,
                //                                 Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
                //                           VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
                //                           sum(VDC.DrAmount) as DrAmount,
                //                           sum(VDC.CrAmount) as CrAmount,
                //                                 sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
                //sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                //                                 sum(CASE WHEN ACT.BalanceType = 'Credit'  THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId)
                //	 -sum(CASE WHEN ACT.BalanceType = 'Debit'  THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END)
                //	 over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) AS TotalPL,
                //ACT.BalanceType,
                //                                 ACT.Id AS [MainHead],
                //AG.UserName AS [Level],
                //                           VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode,
                //                                 VD.BudgetMasterId,
                //                           BUD.UserName AS Budget
                //                              FROM TRN.VoucherDetailCurrency AS VDC
                //                           INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                //                           INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                //                           LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                //                                 LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                //                                 left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                //                           LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                //                                 LEFT JOIN [MST].[BudgetMaster] AS BUDM ON BUDM.Id = VD.BudgetMasterId
                //                                 LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUDM.BudgetId
                //                                 WHERE act.IsBalanceSheet=0 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"'
                //                                 AND V.PlantId='" + plantId + @"'
                //                                 AND v.IsPark=0
                //                                 group by GL.Id, GL.AccountCode,VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName,GL.AccountCode,v.PostingDate,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName";
                //         return _sqlRepository.GetGridData(parameters).Source;
                //     }
                //     else
                //     {
                parameters.CmdText = @"SELECT 	GL.Id AS AccountCodeId,
                                            Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                    sum(VDC.DrAmount) as DrAmount,
		                                    sum(VDC.CrAmount) as CrAmount,
                                            sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
											sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                                            sum(CASE WHEN ACT.BalanceType = 'Credit'  THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId)
												 -sum(CASE WHEN ACT.BalanceType = 'Debit'  THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END)
												 over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) AS TotalPL,
											ACT.BalanceType,
                                            ACT.Id AS [MainHead],
											AG.UserName AS [Level],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                            WHERE act.IsBalanceSheet=0 AND v.PostingDate <=  '" + toDate + "' AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"'
                                            AND V.PlantId='" + plantId + @"'
                                            AND v.IsPark=0
                                            group by GL.Id, GL.AccountCode,VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName,GL.AccountCode,v.PostingDate,ACT.BalanceType,AG.UserName,ACT.Id";
                return _sqlRepository.GetGridData(parameters).Source;
                // }
            }
            catch (Exception)
            {
                throw;
            }
        }


        [HttpGet, Authorize]
        public ActionResult balanceSheetreportForThePeriod(ReportFormat reportFormat, string fromDate, string toDate/*, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel*/)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = GetbalanceSheetreportForThePeriodReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate/*, isBudgetLevel, isActivityLevel, isACGroupLevel*/);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Balance Sheet GroupWise";
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

        public IWorkbook GetbalanceSheetreportForThePeriodReport(string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate)
        {
            var excelEngine = new ExcelEngine();
            var oRU = new ReportUtility();
            var workbook = oRU.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            var row = 6;

            var headreColIndex = 1;
            var mainColIndex = 1;
            var mainColIndexTodate = 1;


            DataTable DtFromDate = GetBalanceSheetForThePeriodInfo(companyGroupId, companyId, plantId, fromDate/*, isBudgetLevel, isActivityLevel, isACGroupLevel*/);
            DataTable dtToDate = GetBalanceSheetToDateForThePeriodInfo(companyGroupId, companyId, plantId, fromDate, toDate/*, isBudgetLevel, isActivityLevel, isACGroupLevel*/);
            DataTable dtCLToDate = GetBalanceSheetCLForThePeriodInfo(companyGroupId, companyId, plantId, toDate/*, isBudgetLevel, isActivityLevel, isACGroupLevel*/);
            var dsLocalOBPL = GetBalanceSheetInfoOBForThePeriodPL(companyGroupId, companyId, plantId, fromDate/*, isBudgetLevel, isActivityLevel, isACGroupLevel*/);
            var dsLocalFTPPL = GetBalanceSheetInfoDateForThePeriodPL(companyGroupId, companyId, plantId, fromDate, toDate/*, isBudgetLevel, isActivityLevel, isACGroupLevel*/);
            var dsLocalCLToDatePL = GetBalanceSheetInfoCLToDateForThePeriodPL(companyGroupId, companyId, plantId, toDate/*, isBudgetLevel, isActivityLevel, isACGroupLevel*/);

            DataTable dtLocal = DtFromDate.DefaultView.ToTable();
            dtLocal.Merge(dtToDate);

            dtLocal = dtLocal.DefaultView.ToTable(true, "AccountCodeId", "BalanceType", "MainHead", "Level", "GLGeneralInfoId", "GL", "AccountCode");

            DataView dvDr = new DataView(dtLocal)
            {
                RowFilter = "MainHead='Equity' OR  MainHead='Liability'",
                Sort = "MainHead desc, AccountCode"
            };
            DataTable dtDr = dvDr.ToTable();

            DataView dvCr = new DataView(dtLocal)
            {
                RowFilter = "MainHead='Asset'",
                Sort = "AccountCode"
            };
            DataTable dtCr = dvCr.ToTable();

            if (dtLocal.Rows.Count > 0)
            {
                row++;
                int colTitle = headreColIndex;
                oRU.SetHeaderTextBL(ref sheet, row, colTitle, "", 36);
                headreColIndex++;
                int colNotes = headreColIndex;
                oRU.SetHeaderTextBL(ref sheet, row, colNotes, "NOTES", 15);
                
                headreColIndex++;
                int colClosingBalance = headreColIndex;
                oRU.SetHeaderTextBL(ref sheet, row, colClosingBalance, " "+ toDate + " ", 15);
                sheet[row, colClosingBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;
                headreColIndex++;
                int colForThePeriod = headreColIndex;
                oRU.SetHeaderTextBL(ref sheet, row, colForThePeriod, "For The Period", 15);
                sheet[row, colForThePeriod].HorizontalAlignment = ExcelHAlign.HAlignRight;
                headreColIndex++;
                int colOpeningBalance = headreColIndex;
                oRU.SetHeaderTextBL(ref sheet, row, colOpeningBalance, ""+Convert.ToDateTime( fromDate).AddDays(-1).ToString("dd-MMM-yyyy")+"", 15);
                sheet[row, colOpeningBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;

                //oRU.SetText(ref sheet, 5, colLast, ("From " + fromDate + " " + " To " + toDate + " "), ExcelHAlign.HAlignCenter);

                row++;
                oRU.SetHeaderTextBL(ref sheet, row, 1, "Assets", 10);

                var sumdrcrCol1 = 0;
                var sumdrcrColTodate = 0;
                var sumdrcrColCLTodate = 0;
                row++;
                int levelStartRow = 0;
                int sumStrRow = row;

                string totalAssetFormaula = "";
                string totalAssetToDateFormaula = "";
                string totalAssetCLFormaula = "";

                DataTable dtcrLevel = dtCr.DefaultView.ToTable(true, "Level");
                for (int il = 0; il < dtcrLevel.Rows.Count; il++)
                {
                    levelStartRow = row;
                    oRU.SetText(ref sheet, row, 1, "" + dtcrLevel.Rows[il]["Level"].ToString() + " :", true);

                    dtCr.DefaultView.RowFilter = "Level ='" + dtcrLevel.Rows[il]["Level"].ToString() + "'";

                    for (int i = 0; i < dtCr.DefaultView.Count; i++)
                    {
                        row++;
                        sumStrRow = row;
                        mainColIndex = 1;
                        mainColIndexTodate = 1;
                        oRU.SetText(ref sheet, row, mainColIndex, dtCr.DefaultView[i]["AccountCode"] + " - " + dtCr.DefaultView[i]["GL"]);
                        mainColIndex++;
                        mainColIndex++;
                        //oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(dtCr.DefaultView[i]["DRcumulative"].ToString()));
                        sheet.Range[row, mainColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;

                        //oRU.SetText(ref sheet, row, mainColIndexTodate, Convert.ToDouble(dtCr.DefaultView[i]["DRcumulative"].ToString()));
                        DtFromDate.DefaultView.RowFilter = "AccountCodeId='" + dtCr.DefaultView[i]["AccountCodeId"] + "'";
                        if (DtFromDate.DefaultView.Count > 0)
                        {
                            oRU.SetText(ref sheet, row, colOpeningBalance, Convert.ToDouble(DtFromDate.DefaultView[0]["DRcumulative"].ToString()));
                        }

                        mainColIndexTodate++;
                        mainColIndexTodate++;
                        mainColIndexTodate++;
                        colForThePeriod = mainColIndexTodate;
                        // mainColIndexTodate++;
                        dtToDate.DefaultView.RowFilter = "AccountCodeId='" + dtCr.DefaultView[i]["AccountCodeId"] + "'";
                        if (dtToDate.DefaultView.Count > 0)
                        {
                            oRU.SetText(ref sheet, row, colForThePeriod, Convert.ToDouble(dtToDate.DefaultView[0]["DRcumulative"].ToString()));
                        }

                        //dtCLToDate
                        mainColIndexTodate++;
                        dtCLToDate.DefaultView.RowFilter = "AccountCodeId='" + dtCr.DefaultView[i]["AccountCodeId"] + "'";
                        if (dtCLToDate.DefaultView.Count > 0)
                        {
                            oRU.SetText(ref sheet, row, colClosingBalance, Convert.ToDouble(dtCLToDate.DefaultView[0]["DRcumulative"].ToString()));
                        }
                    }

                    sheet.Range[levelStartRow + 1, colOpeningBalance, row, colOpeningBalance].BorderAround(ExcelLineStyle.Thin);
                    sheet.Range[levelStartRow + 1, colForThePeriod, row, colForThePeriod].BorderAround(ExcelLineStyle.Thin);
                    sheet.Range[levelStartRow + 1, colClosingBalance, row, colClosingBalance].BorderAround(ExcelLineStyle.Thin);

                    row++;
                    sheet[levelStartRow, colOpeningBalance].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(colOpeningBalance) + (levelStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(colOpeningBalance) + row + ")";
                    sheet.Range[levelStartRow, colOpeningBalance].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[levelStartRow, colOpeningBalance].CellStyle.Font.Bold = true;
                    // sheet.Range[levelStartRow, mainColIndex].BorderAround(ExcelLineStyle.Hair);
                    totalAssetFormaula += clsStaticInfo.GetxlsCol(colOpeningBalance) + (levelStartRow) + "+";

                    sheet[levelStartRow, colForThePeriod].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(colForThePeriod) + (levelStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(colForThePeriod) + row + ")";
                    sheet.Range[levelStartRow, colForThePeriod].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[levelStartRow, colForThePeriod].CellStyle.Font.Bold = true;
                    // sheet.Range[levelStartRow, mainColIndex].BorderAround(ExcelLineStyle.Hair);
                    totalAssetToDateFormaula += clsStaticInfo.GetxlsCol(colForThePeriod) + (levelStartRow) + "+";


                    sheet[levelStartRow, colClosingBalance].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(colClosingBalance) + (levelStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(colClosingBalance) + row + ")";
                    sheet.Range[levelStartRow, colClosingBalance].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[levelStartRow, colClosingBalance].CellStyle.Font.Bold = true;
                    // sheet.Range[levelStartRow, mainColIndex].BorderAround(ExcelLineStyle.Hair);
                    totalAssetCLFormaula += clsStaticInfo.GetxlsCol(colClosingBalance) + (levelStartRow) + "+";

                    row++;
                }
                sumdrcrCol1 = colOpeningBalance;
                sumdrcrColTodate = colForThePeriod;
                sumdrcrColCLTodate = colClosingBalance;

                totalAssetFormaula = totalAssetFormaula.Remove(totalAssetFormaula.Length - 1);
                totalAssetToDateFormaula = totalAssetToDateFormaula.Remove(totalAssetToDateFormaula.Length - 1);
                totalAssetCLFormaula = totalAssetCLFormaula.Remove(totalAssetCLFormaula.Length - 1);

                int TotalAssetSumRow = row;
                oRU.SetText(ref sheet, TotalAssetSumRow, 1, "Total Assets:", true);

                row = TotalAssetSumRow;
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].Formula = totalAssetFormaula;
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].CellStyle.Font.Bold = true;
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Double;

                int TotalAssetToDateSumRow = row;
                row = TotalAssetToDateSumRow;
                sheet.Range[TotalAssetToDateSumRow, sumdrcrColTodate].Formula = totalAssetToDateFormaula;
                sheet.Range[TotalAssetToDateSumRow, sumdrcrColTodate].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[TotalAssetToDateSumRow, sumdrcrColTodate].CellStyle.Font.Bold = true;
                sheet.Range[TotalAssetToDateSumRow, sumdrcrColTodate].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet.Range[TotalAssetToDateSumRow, sumdrcrColTodate].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Double;

                //totalAssetCLFormaula
                int TotalAssetCLSumRow = row;
                row = TotalAssetCLSumRow;
                sheet.Range[TotalAssetCLSumRow, sumdrcrColCLTodate].Formula = totalAssetCLFormaula;
                sheet.Range[TotalAssetCLSumRow, sumdrcrColCLTodate].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[TotalAssetCLSumRow, sumdrcrColCLTodate].CellStyle.Font.Bold = true;
                sheet.Range[TotalAssetCLSumRow, sumdrcrColCLTodate].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet.Range[TotalAssetCLSumRow, sumdrcrColCLTodate].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Double;


                row++;
                row++;
                oRU.SetText(ref sheet, row, 1, "Equity & Liabilities", true);

                var sumdrcrCol2 = 0;
                var sumdrcrColLiaTodate = 0;
                var sumdrcrColLiaCLTodate = 0;
                row++;
                row++;
                int levelLiabilityStartRow = 0;

                string totalLiabilityFormaula = "(";
                string totalLiabilityToDateFormaula = "(";
                string totalLiabilityCLToDateFormaula = "(";

                DataTable dtdrLevel = dtDr.DefaultView.ToTable(true, "Level");
                for (int il = 0; il < dtdrLevel.Rows.Count; il++)
                {
                    oRU.SetText(ref sheet, row, 1, "" + dtdrLevel.Rows[il]["Level"].ToString() + " :", true);

                    levelLiabilityStartRow = row;
                    dtDr.DefaultView.RowFilter = "Level ='" + dtdrLevel.Rows[il]["Level"].ToString() + "'";

                    for (int i = 0; i < dtDr.DefaultView.Count; i++)
                    {
                        row++;
                        mainColIndex = 1;
                        mainColIndexTodate = 1;

                        oRU.SetText(ref sheet, row, mainColIndex, dtDr.DefaultView[i]["AccountCode"] + " - " + dtDr.DefaultView[i]["GL"]);
                        mainColIndex++;
                        mainColIndex++;

                        //sheet.Range[row, mainColIndex].BorderAround(ExcelLineStyle.Hair);
                        //sheet.Range[row, mainColIndex].BorderNone(ExcelLineStyle.Hair);

                        DtFromDate.DefaultView.RowFilter = "AccountCodeId='" + dtDr.DefaultView[i]["AccountCodeId"] + "'";
                        if (DtFromDate.DefaultView.Count > 0)
                        {
                            oRU.SetText(ref sheet, row, colOpeningBalance, Convert.ToDouble(DtFromDate.DefaultView[0]["CRcumulative"].ToString()));
                        }

                        mainColIndexTodate++;
                        mainColIndexTodate++;
                        mainColIndexTodate++;
                        colForThePeriod = mainColIndexTodate;
                        dtToDate.DefaultView.RowFilter = "AccountCodeId='" + dtDr.DefaultView[i]["AccountCodeId"] + "'";
                        if (dtToDate.DefaultView.Count > 0)
                        {
                            oRU.SetText(ref sheet, row, colForThePeriod, Convert.ToDouble(dtToDate.DefaultView[0]["CRcumulative"].ToString()));
                        }

                        mainColIndexTodate++;
                        dtCLToDate.DefaultView.RowFilter = "AccountCodeId='" + dtDr.DefaultView[i]["AccountCodeId"] + "'";
                        if (dtCLToDate.DefaultView.Count > 0)
                        {
                            oRU.SetText(ref sheet, row, colClosingBalance, Convert.ToDouble(dtCLToDate.DefaultView[0]["CRcumulative"].ToString()));
                        }

                    }

                    sheet.Range[levelLiabilityStartRow + 1, colOpeningBalance, row, colOpeningBalance].BorderAround(ExcelLineStyle.Thin);
                    sheet.Range[levelLiabilityStartRow + 1, colForThePeriod, row, colForThePeriod].BorderAround(ExcelLineStyle.Thin);
                    sheet.Range[levelLiabilityStartRow + 1, colClosingBalance, row, colClosingBalance].BorderAround(ExcelLineStyle.Thin);

                    row++;

                    sheet[levelLiabilityStartRow, colOpeningBalance].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(colOpeningBalance) + (levelLiabilityStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(colOpeningBalance) + row + ")";
                    sheet.Range[levelLiabilityStartRow, colOpeningBalance].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[levelLiabilityStartRow, colOpeningBalance].CellStyle.Font.Bold = true;
                    totalLiabilityFormaula += clsStaticInfo.GetxlsCol(colOpeningBalance) + (levelLiabilityStartRow) + "+";

                    sheet[levelLiabilityStartRow, colForThePeriod].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(colForThePeriod) + (levelLiabilityStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(colForThePeriod) + row + ")";
                    sheet.Range[levelLiabilityStartRow, colForThePeriod].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[levelLiabilityStartRow, colForThePeriod].CellStyle.Font.Bold = true;
                    totalLiabilityToDateFormaula += clsStaticInfo.GetxlsCol(colForThePeriod) + (levelLiabilityStartRow) + "+";

                    sheet[levelLiabilityStartRow, colClosingBalance].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(colClosingBalance) + (levelLiabilityStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(colClosingBalance) + row + ")";
                    sheet.Range[levelLiabilityStartRow, colClosingBalance].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[levelLiabilityStartRow, colClosingBalance].CellStyle.Font.Bold = true;
                    totalLiabilityCLToDateFormaula += clsStaticInfo.GetxlsCol(colClosingBalance) + (levelLiabilityStartRow) + "+";
                    row++;
                }

                sumdrcrCol2 = colOpeningBalance;
                sumdrcrColLiaTodate = colForThePeriod;
                sumdrcrColLiaCLTodate = colClosingBalance;

                totalLiabilityFormaula = totalLiabilityFormaula.Remove(totalLiabilityFormaula.Length - 1);
                totalLiabilityFormaula += ")";

                totalLiabilityToDateFormaula = totalLiabilityToDateFormaula.Remove(totalLiabilityToDateFormaula.Length - 1);
                totalLiabilityToDateFormaula += ")";

                totalLiabilityCLToDateFormaula = totalLiabilityCLToDateFormaula.Remove(totalLiabilityCLToDateFormaula.Length - 1);
                totalLiabilityCLToDateFormaula += ")";


                var dvTotToDatePL = new DataView(dsLocalOBPL.Tables[0]);
                var dtTotOBPL = dvTotToDatePL.ToTable();

                var dvTotPL = new DataView(dsLocalFTPPL.Tables[0]);
                var dtTotFTPPL = dvTotPL.ToTable();

                var dvTotCLToDatePL = new DataView(dsLocalCLToDatePL.Tables[0]);
                var dtTotCLToDatePL = dvTotCLToDatePL.ToTable();

                if (dtTotOBPL.Rows.Count != 0 || dtTotFTPPL.Rows.Count != 0 || dtTotCLToDatePL.Rows.Count != 0)
                {
                    row--;
                    sheet.Range[row, 1].Text = "Current Profit/Loss";
                    sheet.Range[row, 1].CellStyle.Font.Italic = true;


                    if (dtTotOBPL.Rows.Count > 0)
                        oRU.SetText(ref sheet, row, colOpeningBalance, Convert.ToDouble(dtTotOBPL.Rows[0]["TotalPL"].ToString()));

                    if (dtTotFTPPL.Rows.Count > 0)
                        oRU.SetText(ref sheet, row, colForThePeriod, Convert.ToDouble(dtTotFTPPL.Rows[0]["TotalPL"].ToString()));

                    if (dtTotCLToDatePL.Rows.Count > 0)
                        oRU.SetText(ref sheet, row, colClosingBalance, Convert.ToDouble(dtTotCLToDatePL.Rows[0]["TotalPL"].ToString()));

                    // sheet.Range[row, sumdrcrCol2].CellStyle.Font.Italic = true;
                    sheet.Range[row, colOpeningBalance].CellStyle.Font.Bold = true;
                    sheet.Range[row, colForThePeriod].CellStyle.Font.Bold = true;
                    sheet.Range[row, colClosingBalance].CellStyle.Font.Bold = true;

                    sheet[levelLiabilityStartRow, colOpeningBalance].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(colOpeningBalance) + (levelLiabilityStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(colOpeningBalance) + row + ")";
                    sheet[levelLiabilityStartRow, colForThePeriod].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(colForThePeriod) + (levelLiabilityStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(colForThePeriod) + row + ")";
                    sheet[levelLiabilityStartRow, colClosingBalance].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(colClosingBalance) + (levelLiabilityStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(colClosingBalance) + row + ")";
                    sheet.Range[levelLiabilityStartRow, colOpeningBalance].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[levelLiabilityStartRow, colOpeningBalance].CellStyle.Font.Bold = true;

                    //sheet[levelLiabilityStartRow, mainColIndexTodate].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(mainColIndexTodate) + (levelLiabilityStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(mainColIndexTodate) + row + ")";
                    //sheet.Range[levelLiabilityStartRow, mainColIndexTodate].NumberFormat = oRU.NumberFormatDecimalTwo();
                    //sheet.Range[levelLiabilityStartRow, mainColIndexTodate].CellStyle.Font.Bold = true;


                }

                sheet.Range[levelLiabilityStartRow + 1, colOpeningBalance, row, colOpeningBalance].BorderAround(ExcelLineStyle.Thin);

                sheet.Range[levelLiabilityStartRow + 1, colOpeningBalance, row, colOpeningBalance].BorderAround(ExcelLineStyle.None);
                sheet.Range[levelLiabilityStartRow + 1, colOpeningBalance, row, colOpeningBalance].BorderInside(ExcelLineStyle.None);
                sheet.Range[levelLiabilityStartRow + 1, colForThePeriod, row, colForThePeriod].BorderInside(ExcelLineStyle.None);

                sheet.Range[levelLiabilityStartRow + 1, colOpeningBalance, row, colOpeningBalance].BorderAround(ExcelLineStyle.Thin);
                sheet.Range[levelLiabilityStartRow + 1, colForThePeriod, row, colForThePeriod].BorderAround(ExcelLineStyle.Thin);
                sheet.Range[levelLiabilityStartRow + 1, colClosingBalance, row, colClosingBalance].BorderAround(ExcelLineStyle.Thin);
                // sheet.Range[levelLiabilityStartRow + 1, mainColIndex, row, mainColIndex].Merge();
                //sheet.Range[oRU.GetColumnNameForXls(mainColIndex) + levelLiabilityStartRow + 1 + ":" + oRU.GetColumnNameForXls(mainColIndex) + row].Merge();

                var colLast = sumdrcrCol2;

                row++;
                row++;

                oRU.SetText(ref sheet, row, 1, "Total Equity & Liabilities:", true);
                int TotalLiabilitySumRow = row;
                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].Formula = totalLiabilityFormaula;
                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].CellStyle.Font.Bold = true;
                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Double;


                int TotalLiabilityToDateSumRow = row;
                sheet.Range[TotalLiabilityToDateSumRow, sumdrcrColLiaTodate].Formula = totalLiabilityToDateFormaula;
                sheet.Range[TotalLiabilityToDateSumRow, sumdrcrColLiaTodate].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[TotalLiabilityToDateSumRow, sumdrcrColLiaTodate].CellStyle.Font.Bold = true;
                sheet.Range[TotalLiabilityToDateSumRow, sumdrcrColLiaTodate].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet.Range[TotalLiabilityToDateSumRow, sumdrcrColLiaTodate].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Double;



                int TotalLiabilityCLToDateSumRow = row;
                sheet.Range[TotalLiabilityCLToDateSumRow, sumdrcrColLiaCLTodate].Formula = totalLiabilityCLToDateFormaula;
                sheet.Range[TotalLiabilityCLToDateSumRow, sumdrcrColLiaCLTodate].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[TotalLiabilityCLToDateSumRow, sumdrcrColLiaCLTodate].CellStyle.Font.Bold = true;
                sheet.Range[TotalLiabilityCLToDateSumRow, sumdrcrColLiaCLTodate].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet.Range[TotalLiabilityCLToDateSumRow, sumdrcrColLiaCLTodate].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Double;

                //sheet.Range[row, 1].BorderAround(ExcelLineStyle.None);
                sheet.Name = "Sheet";
                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                oRU.CompanyPlantHeader(ref sheet, colLast, "Balance Sheet", companyId,plantId, plantName, null);
                oRU.SetText(ref sheet, 5, colLast, ("From " + fromDate + "" + " To " + toDate + " "), ExcelHAlign.HAlignCenter);
                sheet.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(colLast) + 5].Merge();
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                sheet.ShowColumn(2, false);

            }
            else
            {
                sheet.Name = "Sheet";
                oRU.CompanyHeader(ref sheet, 5, "Balance Sheet", companyId);
                oRU.SetText(ref sheet, 5, 3, "No Data Found !", ExcelHAlign.HAlignCenter);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);

            }
            return workbook;
        }
        private DataTable GetBalanceSheetForThePeriodInfo(string companyGroupId, string companyId, string plantId, string fromDate /*, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel*/)
        {
          
            var cmdText = @"select * FROM (SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
								sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id,  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
                                , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode
	                            FROM TRN.VoucherDetailCurrency AS VDC
		                        INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                        INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                        LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                        LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                WHERE act.IsBalanceSheet=1 AND v.PostingDate <= '" + fromDate + @"'   AND V.CompanyGroupId='" + companyGroupId + @"'
                                AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                AND V.IsPark=0
                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id
                                   ) AS K where k.DRcumulative<>0  OR 	k.CRcumulative<>0";
            return _sqlRepository.GetDataTable(cmdText);
            //}
        }
        private DataTable GetBalanceSheetToDateForThePeriodInfo(string companyGroupId, string companyId, string plantId, string fromDate, string toDate/*, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel*/)
        {
            var cmdText = @"select * FROM (SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
								sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id,  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
                                , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode
	                            FROM TRN.VoucherDetailCurrency AS VDC
		                        INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                        INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                        LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                        LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                WHERE act.IsBalanceSheet=1 AND v.PostingDate between '" + Convert.ToDateTime(fromDate).AddDays(1).ToString("dd-MMM-yyyy") + "' and '" + toDate + @"'  AND V.CompanyGroupId='" + companyGroupId + @"'
                                AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                AND V.IsPark=0
                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id
) AS K where k.DRcumulative<>0  OR 	k.CRcumulative<>0";
            return _sqlRepository.GetDataTable(cmdText);
            //}
        }

        private DataTable GetBalanceSheetCLForThePeriodInfo(string companyGroupId, string companyId, string plantId, string toDate/*, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel*/)
        {
           
            var cmdText = @"select * FROM (SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
								sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id,  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
                                , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode
	                            FROM TRN.VoucherDetailCurrency AS VDC
		                        INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                        INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                        LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                        LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                WHERE act.IsBalanceSheet=1 AND v.PostingDate <=  '" + toDate + @"'  AND V.CompanyGroupId='" + companyGroupId + @"'
                                AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                AND V.IsPark=0
                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id
) AS K where k.DRcumulative<>0  OR 	k.CRcumulative<>0";
            return _sqlRepository.GetDataTable(cmdText);
           
        }

        private DataSet GetBalanceSheetInfoDateForThePeriodPL(string companyGroupId, string companyId, string plantId, string fromDate, string toDate/*, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel*/)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
              
                parameters.CmdText = @"select * FROM (SELECT 	GL.Id AS AccountCodeId,
                                            Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                    sum(VDC.DrAmount) as DrAmount,
		                                    sum(VDC.CrAmount) as CrAmount,
                                            sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
											sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                                            sum(CASE WHEN ACT.BalanceType = 'Credit'  THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId)
												 -sum(CASE WHEN ACT.BalanceType = 'Debit'  THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END)
												 over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) AS TotalPL,
											ACT.BalanceType,
                                            ACT.Id AS [MainHead],
											AG.UserName AS [Level],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                            WHERE act.IsBalanceSheet=0 AND v.PostingDate Between '" + Convert.ToDateTime(fromDate).AddDays(1).ToString("dd-MMM-yyyy") + "' and '" + toDate + @"'   AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"'
                                            AND V.PlantId='" + plantId + @"'
                                            AND v.IsPark=0
                                            group by GL.Id, GL.AccountCode,VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName,GL.AccountCode,v.PostingDate,ACT.BalanceType,AG.UserName,ACT.Id
) AS K where k.DRcumulative<>0  OR 	k.CRcumulative<>0";
                return _sqlRepository.GetGridData(parameters).Source;
                // }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private DataSet GetBalanceSheetInfoOBForThePeriodPL(string companyGroupId, string companyId, string plantId, string fromDate/*, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel*/)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
               
                parameters.CmdText = @"SELECT 	GL.Id AS AccountCodeId,
                                            Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                    sum(VDC.DrAmount) as DrAmount,
		                                    sum(VDC.CrAmount) as CrAmount,
                                            sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
											sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                                            sum(CASE WHEN ACT.BalanceType = 'Credit'  THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId)
												 -sum(CASE WHEN ACT.BalanceType = 'Debit'  THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END)
												 over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) AS TotalPL,
											ACT.BalanceType,
                                            ACT.Id AS [MainHead],
											AG.UserName AS [Level],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                            WHERE act.IsBalanceSheet=0 AND v.PostingDate <=  '" + fromDate + "' AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"'
                                            AND V.PlantId='" + plantId + @"'
                                            AND v.IsPark=0
                                            group by GL.Id, GL.AccountCode,VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName,GL.AccountCode,v.PostingDate,ACT.BalanceType,AG.UserName,ACT.Id";
                return _sqlRepository.GetGridData(parameters).Source;
                // }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //GetBalanceSheetInfoCLToDateForThePeriodPL
        private DataSet GetBalanceSheetInfoCLToDateForThePeriodPL(string companyGroupId, string companyId, string plantId, string toDate/*, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel*/)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                
                parameters.CmdText = @"SELECT 	GL.Id AS AccountCodeId,
                                            Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                    sum(VDC.DrAmount) as DrAmount,
		                                    sum(VDC.CrAmount) as CrAmount,
                                            sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
											sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                                            sum(CASE WHEN ACT.BalanceType = 'Credit'  THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId)
												 -sum(CASE WHEN ACT.BalanceType = 'Debit'  THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END)
												 over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) AS TotalPL,
											ACT.BalanceType,
                                            ACT.Id AS [MainHead],
											AG.UserName AS [Level],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                            WHERE act.IsBalanceSheet=0 AND v.PostingDate <=  '" + toDate + "' AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"'
                                            AND V.PlantId='" + plantId + @"'
                                            AND v.IsPark=0
                                            group by GL.Id, GL.AccountCode,VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName,GL.AccountCode,v.PostingDate,ACT.BalanceType,AG.UserName,ACT.Id";
                return _sqlRepository.GetGridData(parameters).Source;
                // }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion BL DATE RANGE

        #region TrialBalanceGroupWiseReport

        [HttpGet, Authorize]
        public ActionResult TrialBalanceGroupWiseReport(ReportFormat reportFormat, string date, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = GetTrialBalanceGroupWiseReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, date, isBudgetLevel, isActivityLevel, isACGroupLevel);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + "Trial Balance GroupWise";
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

        public IWorkbook GetTrialBalanceGroupWiseReport(string companyGroupId, string companyId, string plantId, string plantName, string date, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel)
        {
            var excelEngine = new ExcelEngine();
            var oRU = new ReportUtility();
            var workbook = oRU.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            var row = 6;

            var headreColIndex = 1;
            var mainColIndex = 1;
            DataTable dtLocal = GetTrialBalanceGroupWiseReportInfo(companyGroupId, companyId, plantId, date, isBudgetLevel, isActivityLevel, isACGroupLevel);
            var dsLocalPL = GetTrialBalanceGroupWiseReportInfoPL(companyGroupId, companyId, plantId, date, isBudgetLevel, isActivityLevel, isACGroupLevel);

            //var dvParallelCurrency = new DataView(dtLocal)
            //{
            //    Sort = "CurrencyCode ASC"
            //};
            //var dtParallelCurrency = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");

            DataView dvDr = new DataView(dtLocal)
            {
                RowFilter = "MainHead='Equity' OR  MainHead='Liability'",
                Sort = "MainHead desc, AccountCode"
            };
            DataTable dtDr = dvDr.ToTable();

            DataView dvCr = new DataView(dtLocal)
            {
                RowFilter = "MainHead='Asset'",
                Sort = "AccountCode"
            };
            DataTable dtCr = dvCr.ToTable();

            if (dtLocal.Rows.Count > 0)
            {
                row++;
                int colTitle = headreColIndex;
                oRU.SetHeaderTextBL(ref sheet, row, colTitle, "", 36);
                headreColIndex++;
                int colNotes = headreColIndex;
                oRU.SetHeaderTextBL(ref sheet, row, colNotes, "NOTES", 15);


                // oRU.SetText(ref sheet, row, 3, dtLocal.Rows[0]["" + date + ""].ToString());

                headreColIndex++;
                int colDateYear = headreColIndex;
                //var hiestDate1 = date;
                //var hiestDate2 = hiestDate1.Substring(3);

                //oRU.SetHeaderTextBL(ref sheet, row, colDateYear, "" + hiestDate2 + "", ExcelHAlign.HAlignCenter);

                oRU.SetHeaderTextBL(ref sheet, row, colDateYear, "Dr", ExcelHAlign.HAlignCenter);
                headreColIndex++;
                int colCrAmount = headreColIndex;
                oRU.SetHeaderTextBL(ref sheet, row, colCrAmount, "Cr", ExcelHAlign.HAlignCenter);

                //if (isACGroupLevel)
                //{
                //    oRU.SetHeaderTextBL(ref sheet, row, headreColIndex, "Account Group", 36); headreColIndex++;
                //}

                ////oRU.SetHeaderTextBL(ref sheet, row, headreColIndex, "GL", 36); headreColIndex++;
                //if (isBudgetLevel)
                //{
                //    oRU.SetHeaderTextBL(ref sheet, row, headreColIndex, "Budget", 36); headreColIndex++;
                //}
                //if (isActivityLevel)
                //{
                //    oRU.SetHeaderTextBL(ref sheet, row, headreColIndex, "Activity", 36); headreColIndex++;
                //}
                //oRU.SetHeaderTextBL(ref sheet, row, headreColIndex, dtLocal.Rows[0]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);

                //row++;
                //oRU.SetText(ref sheet, row, 1, "Total Assets:", true);
                row++;
                oRU.SetHeaderTextBL(ref sheet, row, 1, "Assets", 10);


                var sumdrcrCol1 = 0;
                // var sumdrcrColTodate = 0;
                row++;
                int levelStartRow = 0;
                int sumStrRow = row;

                string totalAssetFormaula = "";

                DataTable dtcrLevel = dtCr.DefaultView.ToTable(true, "Level");
                for (int il = 0; il < dtcrLevel.Rows.Count; il++)
                {
                    oRU.SetText(ref sheet, row, 1, "" + dtcrLevel.Rows[il]["Level"].ToString() + " :", true);
                    levelStartRow = row;

                    dtCr.DefaultView.RowFilter = "Level ='" + dtcrLevel.Rows[il]["Level"].ToString() + "'";


                    for (int i = 0; i < dtCr.DefaultView.Count; i++)
                    {
                        row++;
                        sumStrRow = row;
                        mainColIndex = 1;
                        //if (isACGroupLevel)
                        //{
                        //    oRU.SetText(ref sheet, row, mainColIndex, dtCr.DefaultView[i]["Level"].ToString()); mainColIndex++;
                        //}
                        oRU.SetText(ref sheet, row, mainColIndex, dtCr.DefaultView[i]["AccountCode"] + " - " + dtCr.DefaultView[i]["GL"]); mainColIndex++;
                        //if (isBudgetLevel)
                        //{
                        //    oRU.SetText(ref sheet, row, mainColIndex, dtCr.DefaultView[i]["Budget"].ToString()); mainColIndex++;
                        //}
                        //if (isActivityLevel)
                        //{
                        //    oRU.SetText(ref sheet, row, mainColIndex, dtCr.DefaultView[i]["Activity"].ToString()); mainColIndex++;
                        //}
                        mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(dtCr.DefaultView[i]["DRcumulative"].ToString()));



                    }
                    sheet.Range[levelStartRow + 1, mainColIndex, row, mainColIndex].BorderAround(ExcelLineStyle.Thin);

                    row++;
                    sheet[levelStartRow, mainColIndex].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(mainColIndex) + (levelStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(mainColIndex) + row + ")";
                    sheet.Range[levelStartRow, mainColIndex].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[levelStartRow, mainColIndex].CellStyle.Font.Bold = true;
                    // sheet.Range[levelStartRow, mainColIndex].BorderAround(ExcelLineStyle.Hair);
                    totalAssetFormaula += clsStaticInfo.GetxlsCol(mainColIndex) + (levelStartRow) + "+";
                    row++;
                }
                sumdrcrCol1 = mainColIndex;

                totalAssetFormaula = totalAssetFormaula.Remove(totalAssetFormaula.Length - 1);
                // Row_Total_End = row;

                #region TotalCalculation

                #endregion

                int TotalAssetSumRow = row;
                oRU.SetText(ref sheet, TotalAssetSumRow, 1, "Total Assets:", true);

                row = TotalAssetSumRow;
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].Formula = totalAssetFormaula;
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].CellStyle.Font.Bold = true;
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet.Range[TotalAssetSumRow, sumdrcrCol1].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Double;

                row++;
                row++;
                oRU.SetText(ref sheet, row, 1, "Equity & Liabilities", true);

                var sumdrcrCol2 = 0;
                row++;
                row++;
                int levelLiabilityStartRow = 0;

                string totalLiabilityFormaula = "(";

                DataTable dtdrLevel = dtDr.DefaultView.ToTable(true, "Level");
                for (int il = 0; il < dtdrLevel.Rows.Count; il++)
                {
                    oRU.SetText(ref sheet, row, 1, "" + dtdrLevel.Rows[il]["Level"].ToString() + " :", true);

                    levelLiabilityStartRow = row;
                    dtDr.DefaultView.RowFilter = "Level ='" + dtdrLevel.Rows[il]["Level"].ToString() + "'";

                    for (int i = 0; i < dtDr.DefaultView.Count; i++)
                    {
                        row++;
                        mainColIndex = 1;
                        //if (isACGroupLevel)
                        //{
                        //    oRU.SetText(ref sheet, row, mainColIndex, dtDr.DefaultView[i]["Level"].ToString()); mainColIndex++;
                        //}
                        oRU.SetText(ref sheet, row, mainColIndex, dtDr.DefaultView[i]["AccountCode"] + " - " + dtDr.DefaultView[i]["GL"]); mainColIndex++;
                        //if (isBudgetLevel)
                        //{
                        //    oRU.SetText(ref sheet, row, mainColIndex, dtDr.DefaultView[i]["Budget"].ToString()); mainColIndex++;
                        //}
                        //if (isActivityLevel)
                        //{
                        //    oRU.SetText(ref sheet, row, mainColIndex, dtDr.DefaultView[i]["Activity"].ToString()); mainColIndex++;
                        //}
                        mainColIndex++;
                        mainColIndex++;
                        oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(dtDr.DefaultView[i]["CRcumulative"].ToString()));
                        //sheet.Range[row, mainColIndex].BorderAround(ExcelLineStyle.Hair);
                        //sheet.Range[row, mainColIndex].BorderNone(ExcelLineStyle.Hair);

                    }
                    sheet.Range[levelLiabilityStartRow + 1, mainColIndex, row, mainColIndex].BorderAround(ExcelLineStyle.Thin);

                    row++;

                    sheet[levelLiabilityStartRow, mainColIndex].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(mainColIndex) + (levelLiabilityStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(mainColIndex) + row + ")";
                    sheet.Range[levelLiabilityStartRow, mainColIndex].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[levelLiabilityStartRow, mainColIndex].CellStyle.Font.Bold = true;

                    totalLiabilityFormaula += clsStaticInfo.GetxlsCol(mainColIndex) + (levelLiabilityStartRow) + "+";
                    row++;
                }

                sumdrcrCol2 = mainColIndex;

                totalLiabilityFormaula = totalLiabilityFormaula.Remove(totalLiabilityFormaula.Length - 1);
                totalLiabilityFormaula += ")";



                var dvTotPL = new DataView(dsLocalPL.Tables[0]);
                var dtTotPL = dvTotPL.ToTable();
                if (dtTotPL.Rows.Count != 0)
                {
                    row--;
                    sheet.Range[row, 1].Text = "Current Profit/Loss ";
                    sheet.Range[row, 1].CellStyle.Font.Italic = true;
                    oRU.SetText(ref sheet, row, sumdrcrCol2, Convert.ToDouble(dtTotPL.Rows[0]["TotalPL"].ToString()));
                    sheet.Range[row, sumdrcrCol2].CellStyle.Font.Italic = true;
                    sheet.Range[row, sumdrcrCol2].CellStyle.Font.Bold = true;

                    sheet[levelLiabilityStartRow, mainColIndex].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(mainColIndex) + (levelLiabilityStartRow + 1) + ":" + clsStaticInfo.GetxlsCol(mainColIndex) + row + ")";
                    sheet.Range[levelLiabilityStartRow, mainColIndex].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[levelLiabilityStartRow, mainColIndex].CellStyle.Font.Bold = true;


                }
                sheet.Range[levelLiabilityStartRow + 1, mainColIndex, row, mainColIndex].BorderAround(ExcelLineStyle.Thin);

                sheet.Range[levelLiabilityStartRow + 1, mainColIndex, row, mainColIndex].BorderAround(ExcelLineStyle.None);
                sheet.Range[levelLiabilityStartRow + 1, mainColIndex, row, mainColIndex].BorderInside(ExcelLineStyle.None);

                sheet.Range[levelLiabilityStartRow + 1, mainColIndex, row, mainColIndex].BorderAround(ExcelLineStyle.Thin);
                // sheet.Range[levelLiabilityStartRow + 1, mainColIndex, row, mainColIndex].Merge();
                //sheet.Range[oRU.GetColumnNameForXls(mainColIndex) + levelLiabilityStartRow + 1 + ":" + oRU.GetColumnNameForXls(mainColIndex) + row].Merge();

                var colLast = sumdrcrCol2;

                row++;
                row++;

                oRU.SetText(ref sheet, row, 1, "Total Equity & Liabilities:", true);
                int TotalLiabilitySumRow = row;

                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].Formula = totalLiabilityFormaula;
                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].CellStyle.Font.Bold = true;
                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet.Range[TotalLiabilitySumRow, sumdrcrCol2].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Double;

                //sheet.Range[row, 1].BorderAround(ExcelLineStyle.None);
                sheet.Name = "Sheet";
                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                oRU.CompanyPlantHeader(ref sheet, colLast, "Trial Balance ", companyId, plantName, null);
                oRU.SetText(ref sheet, 5, colLast, "As On " + date + "", ExcelHAlign.HAlignCenter);
                sheet.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(colLast) + 5].Merge();
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                sheet.ShowColumn(2, false);

            }
            else
            {
                sheet.Name = "Sheet";
                oRU.CompanyHeader(ref sheet, 5, "Trial Balance ", companyId);
                oRU.SetText(ref sheet, 5, 3, "No Data Found !", ExcelHAlign.HAlignCenter);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);

            }
            return workbook;
        }
        //private static void TotalRevenue_BalanceSheet_Extent(ref IWorksheet sheet, ReportUtility reportUtility, DataTable dtParallelCurrency, int sumdrcrCol1, int RowTotal_current, int Row_Total_Start, int Row_total_End)
        //{
        //    for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
        //    {
        //        sheet.Range[RowTotal_current, sumdrcrCol1].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(sumdrcrCol1) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(sumdrcrCol1) + Row_total_End + ")";
        //        sheet.Range[RowTotal_current, sumdrcrCol1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
        //        sheet.Range[RowTotal_current, sumdrcrCol1].CellStyle.Font.Bold = true;
        //        sheet.Range[RowTotal_current, sumdrcrCol1].BorderAround(ExcelLineStyle.Hair);
        //    }
        //}
        //private static void TotalExpense_BalanceSheet_Extent(ref IWorksheet sheet, ReportUtility reportUtility, DataTable dtParallelCurrency, int sumdrcrCol2, int RowTotal_current2, int Row_Total_Start2, int Row_Total_End2)
        //{
        //    var Row_Total_End3 = Row_Total_End2 + 1;
        //    for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
        //    {
        //        sheet.Range[RowTotal_current2, sumdrcrCol2].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(sumdrcrCol2) + Row_Total_Start2 + ":" + reportUtility.GetColumnNameForXls(sumdrcrCol2) + Row_Total_End2 + ")" + "+" + reportUtility.GetColumnNameForXls(sumdrcrCol2) + Row_Total_End3 + "";
        //        sheet.Range[RowTotal_current2, sumdrcrCol2].NumberFormat = reportUtility.NumberFormatDecimalTwo();
        //        sheet.Range[RowTotal_current2, sumdrcrCol2].CellStyle.Font.Bold = true;
        //        sheet.Range[RowTotal_current2, sumdrcrCol2].BorderAround(ExcelLineStyle.Hair);
        //    }
        //}


        private DataTable GetTrialBalanceGroupWiseReportInfo(string companyGroupId, string companyId, string plantId, string date, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel)
        {
            if (isActivityLevel)
            {
                var cmdText = @"SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode
                              , sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
                                , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode, VD.BudgetMasterId, BM.RefNo+' - '+BUD.UserName AS Budget
                                , A.UserName AS Activity, A.Id as ActivityId
	                            FROM TRN.VoucherDetailCurrency AS VDC
		                        INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                        INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                        LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                        LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                LEFT JOIN MST.BudgetMaster BM ON BM.Id=VD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                                LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                                WHERE act.IsBalanceSheet=1 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"'
                                AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                AND V.IsPark=0
                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id, VD.BudgetMasterId, BM.RefNo, BUD.UserName, A.UserName, A.Id";
                return _sqlRepository.GetDataTable(cmdText);
            }
            else if (isBudgetLevel && !isActivityLevel)
            {
                var cmdText = @"SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode
                              , sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
                                , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode, VD.BudgetMasterId, BM.RefNo+' - '+BUD.UserName AS Budget
	                            FROM TRN.VoucherDetailCurrency AS VDC
		                        INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                        INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                        LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                        LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                LEFT JOIN MST.BudgetMaster BM ON BM.Id=VD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                                WHERE act.IsBalanceSheet=1 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"'
                                AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                AND V.IsPark=0
                                GROUP BY  GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id, VD.BudgetMasterId, BM.RefNo, BUD.UserName";
                return _sqlRepository.GetDataTable(cmdText);
            }
            else
            {
                var cmdText = @"SELECT * FROM (SELECT  distinct	GL.Id AS AccountCodeId,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                 sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                         , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative ,
                                            ACT.BalanceType,
                                            ACT.Id AS [MainHead],AG.UserName Level,
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode 
											--AS GLGeneralInfoCode
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                     

											where v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"'  AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                            and  v.IsPark=0
                                            group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName, GL.AccountCode, ACT.BalanceType,ACT.Id,v.PostingDate,AG.UserName) ttd 
                                            WHERE ISNULL(DRcumulative,0.00) <> 0.00 OR ISNULL(CRcumulative,0) <> 0.00";
                return _sqlRepository.GetDataTable(cmdText);
            }
        }

        private DataSet GetTrialBalanceGroupWiseReportInfoPL(string companyGroupId, string companyId, string plantId, string date, bool isBudgetLevel, bool isActivityLevel, bool isACGroupLevel)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                if (isActivityLevel)
                {
                    parameters.CmdText = @"SELECT 	GL.Id AS AccountCodeId,
                                            Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                    sum(VDC.DrAmount) as DrAmount,
		                                    sum(VDC.CrAmount) as CrAmount,
                                            sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
											sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                                            sum(CASE WHEN ACT.BalanceType = 'Credit'  THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId)
												 -sum(CASE WHEN ACT.BalanceType = 'Debit'  THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END)
												 over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) AS TotalPL,
											ACT.BalanceType,
                                            ACT.Id AS [MainHead],
											AG.UserName AS [Level],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode,
                                            VD.BudgetMasterId,
                                            A.Id AS ActivityId,
		                                    BUD.UserName AS Budget,
                                            A.UserName AS Activity
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                            LEFT JOIN [MST].[BudgetMaster] AS BUDM ON BUDM.Id = VD.BudgetMasterId
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUDM.BudgetId
                                            LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                                            WHERE act.IsBalanceSheet=0 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"'
                                            AND V.PlantId='" + plantId + @"'
                                            AND v.IsPark=0
                                            group by GL.Id, GL.AccountCode,VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName,GL.AccountCode,v.PostingDate,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName, A.UserName,A.Id";
                    return _sqlRepository.GetGridData(parameters).Source;
                }
                else if (isBudgetLevel && !isActivityLevel)
                {
                    parameters.CmdText = @"SELECT 	GL.Id AS AccountCodeId,
                                            Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                    sum(VDC.DrAmount) as DrAmount,
		                                    sum(VDC.CrAmount) as CrAmount,
                                            sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
											sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                                            sum(CASE WHEN ACT.BalanceType = 'Credit'  THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId)
												 -sum(CASE WHEN ACT.BalanceType = 'Debit'  THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END)
												 over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) AS TotalPL,
											ACT.BalanceType,
                                            ACT.Id AS [MainHead],
											AG.UserName AS [Level],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                            LEFT JOIN [MST].[BudgetMaster] AS BUDM ON BUDM.Id = VD.BudgetMasterId
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUDM.BudgetId
                                            WHERE act.IsBalanceSheet=0 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"'
                                            AND V.PlantId='" + plantId + @"'
                                            AND v.IsPark=0
                                            group by GL.Id, GL.AccountCode,VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName,GL.AccountCode,v.PostingDate,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName";
                    return _sqlRepository.GetGridData(parameters).Source;
                }
                else
                {
                    parameters.CmdText = @"SELECT 	GL.Id AS AccountCodeId,
                                            Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                    sum(VDC.DrAmount) as DrAmount,
		                                    sum(VDC.CrAmount) as CrAmount,
                                            sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
											sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                                            sum(CASE WHEN ACT.BalanceType = 'Credit'  THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId)
												 -sum(CASE WHEN ACT.BalanceType = 'Debit'  THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END)
												 over (partition by  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) AS TotalPL,
											ACT.BalanceType,
                                            ACT.Id AS [MainHead],
											AG.UserName AS [Level],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                            WHERE act.IsBalanceSheet=0 AND v.PostingDate <= '" + date + @"' AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"'
                                            AND V.PlantId='" + plantId + @"'
                                            AND v.IsPark=0
                                            group by GL.Id, GL.AccountCode,VDC.ParallelCurrencyId,CU.Code,VD.GLGeneralInfoId,GL.UserName,GL.AccountCode,v.PostingDate,ACT.BalanceType,AG.UserName,ACT.Id";
                    return _sqlRepository.GetGridData(parameters).Source;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion balance sheet extent


        [HttpGet, Authorize]
        public ActionResult BalanceSheetObReport(string fiscalYearId, bool isBudgetLevel, bool isActivityLevel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Opening Balance Sheet Report " + DateTime.Now.ToString("ddMMMyyyy") + ".xlsx";
            var workbook = GetBalanceSheetObReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fiscalYearId, isBudgetLevel, isActivityLevel);
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        public IWorkbook GetBalanceSheetObReport(string companyGroupId, string companyId, string plantId, string plantName, string fiscalYearId, bool isBudgetLevel, bool isActivityLevel)
        {
            var excelEngine = new ExcelEngine();
            var oRU = new ReportUtility();
            var workbook = oRU.GetWorkbook(ref excelEngine, 1);
            var sheet = workbook.Worksheets[0];
            workbook.Version = ExcelVersion.Excel2013;

            var row = 6;
            var headreColIndex = 1;
            var mainColIndex = 1;

            var dsLocal = GetBalanceSheetObInfo(companyGroupId, companyId, plantId, fiscalYearId, isBudgetLevel, isActivityLevel);
            var fiscalYear = _sqlRepository.GetData("SELECT FiscalYearCode, FiscalYearName, StartDate, EndDate FROM [SCS].[FiscalYear] WHERE Id='" + fiscalYearId + "'");

            var dvParallelCurrency = new DataView(dsLocal)
            {
                Sort = "CurrencyCode ASC"
            };
            var dtParallelCurrency = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");

            var dvDr = new DataView(dsLocal)
            {
                RowFilter = "MainHead='Equity' OR  MainHead='Liability'",
                Sort = "MainHead desc,AccountCode"
            };
            var dtDr = dvDr.ToTable();

            var dvCr = new DataView(dsLocal)
            {
                RowFilter = "MainHead='Asset'",
                Sort = "AccountCode"
            };
            var dtCr = dvCr.ToTable();

            if (dsLocal.Rows.Count > 0)
            {
                row++;
                oRU.SetHeaderText(ref sheet, row, headreColIndex, "GL", 36); headreColIndex++;
                if (isBudgetLevel == true)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Budget", 36); headreColIndex++;
                }
                if (isActivityLevel == true)
                {
                    oRU.SetHeaderText(ref sheet, row, headreColIndex, "Activity", 36); headreColIndex++;
                }
                oRU.SetHeaderText(ref sheet, row, headreColIndex, dsLocal.Rows[0]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);

                row++;
                oRU.SetText(ref sheet, row, 1, "Total Assets:", true);
                var Row_Total_Start = row + 1;
                var RowTotal_current = row;
                var Row_Total_End = 0;
                var sumdrcrCol1 = 0;

                for (int i = 0; i < dtCr.Rows.Count; i++)
                {
                    row++;
                    mainColIndex = 1;
                    oRU.SetText(ref sheet, row, mainColIndex, dtCr.Rows[i]["AccountCode"] + " - " + dtCr.Rows[i]["GL"]); mainColIndex++;
                    if (isBudgetLevel == true)
                    {
                        oRU.SetText(ref sheet, row, mainColIndex, dtCr.Rows[i]["Budget"].ToString()); mainColIndex++;
                    }
                    if (isActivityLevel == true)
                    {
                        oRU.SetText(ref sheet, row, mainColIndex, dtCr.Rows[i]["Activity"].ToString()); mainColIndex++;
                    }
                    oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(dtCr.Rows[i]["DRcumulative"].ToString()));
                }
                sumdrcrCol1 = mainColIndex;
                Row_Total_End = row;
                int TotalAssetSumRow = 0; //???Parameter
                TotalRevenue_BalanceSheet(ref sheet, oRU, dtParallelCurrency, sumdrcrCol1, RowTotal_current, TotalAssetSumRow, Row_Total_Start, Row_Total_End);
                row++;
                oRU.SetText(ref sheet, row, 1, "Equity & Liability:", true);
                var Row_Total_Start2 = row + 1;
                var RowTotal_current2 = row;
                var Row_Total_End2 = 0;
                var sumdrcrCol2 = 0;
                for (int i = 0; i < dtDr.Rows.Count; i++)
                {
                    row++;
                    mainColIndex = 1;

                    oRU.SetText(ref sheet, row, mainColIndex, dtDr.Rows[i]["AccountCode"] + " - " + dtDr.Rows[i]["GL"]); mainColIndex++;
                    if (isBudgetLevel == true)
                    {
                        oRU.SetText(ref sheet, row, mainColIndex, dtDr.Rows[i]["Budget"].ToString()); mainColIndex++;
                    }
                    if (isActivityLevel == true)
                    {
                        oRU.SetText(ref sheet, row, mainColIndex, dtDr.Rows[i]["Activity"].ToString()); mainColIndex++;
                    }
                    oRU.SetText(ref sheet, row, mainColIndex, Convert.ToDouble(dtDr.Rows[i]["CRcumulative"].ToString()));
                }
                Row_Total_End2 = row;
                sumdrcrCol2 = mainColIndex;
                row++;
                int TotalEquityandLiabilitySumRow = 0;
                TotalExpense_BalanceSheet(ref sheet, oRU, dtParallelCurrency, sumdrcrCol2, RowTotal_current2, TotalEquityandLiabilitySumRow, Row_Total_Start2, Row_Total_End2);
                var colLast = sumdrcrCol2;

                sheet.Range[8, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[8, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                oRU.CompanyPlantHeader(ref sheet, colLast, "Opening Balance Sheet Details", companyId, plantName, null);
                oRU.SetText(ref sheet, 5, colLast, "Fiscal Year: " + fiscalYear["FiscalYearName"], ExcelHAlign.HAlignCenter);
                sheet.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(colLast) + 5].Merge();
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            else
            {
                oRU.CompanyHeader(ref sheet, 5, "Opening Balance Sheet Details", companyId);
                oRU.SetText(ref sheet, 5, 3, "No Data Found !", ExcelHAlign.HAlignCenter);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }

        private DataTable GetBalanceSheetObInfo(string companyGroupId, string companyId, string plantId, string fiscalYearId, bool isBudgetLevel, bool isActivityLevel)
        {
            try
            {
                if (isActivityLevel)
                {
                    var cmdText = @"SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode
                              , sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
                                , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode, VD.BudgetMasterId, BM.RefNo+' - '+BUD.UserName AS Budget
                                , A.UserName AS Activity, A.Id as ActivityId,VD.FiscalYearId
	                            FROM TRN.VoucherDetailCurrency AS VDC
		                        INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                        INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                        LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                        LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                LEFT JOIN MST.BudgetMaster BM ON BM.Id=VD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                                LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                                LEFT JOIN SCS.FiscalYear AS FY ON FY.Id=VD.FiscalYearId
                                WHERE act.IsBalanceSheet=1 AND VD.FiscalYearId = '" + fiscalYearId + @"' AND V.CompanyGroupId='" + companyGroupId + @"'
                                AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                AND V.IsPark=0 AND V.SourceType='OpeningBalance'
                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id, VD.BudgetMasterId, BM.RefNo, BUD.UserName, A.UserName, A.Id, VD.FiscalYearId";
                    return _sqlRepository.GetDataTable(cmdText);
                }
                else if (isBudgetLevel && !isActivityLevel)
                {
                    var cmdText = @"SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode
                              , sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
                                , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode, VD.BudgetMasterId, BM.RefNo+' - '+BUD.UserName AS Budget,VD.FiscalYearId
	                            FROM TRN.VoucherDetailCurrency AS VDC
		                        INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                        INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                        LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                        LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                LEFT JOIN MST.BudgetMaster BM ON BM.Id=VD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                                LEFT JOIN SCS.FiscalYear AS FY ON FY.Id=VD.FiscalYearId
                                WHERE act.IsBalanceSheet=1 AND VD.FiscalYearId = '" + fiscalYearId + @"' AND V.CompanyGroupId='" + companyGroupId + @"'
                                AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                AND V.IsPark=0 AND SourceType='OpeningBalance'
                                GROUP BY  GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id, VD.BudgetMasterId, BM.RefNo, BUD.UserName, VD.FiscalYearId";
                    return _sqlRepository.GetDataTable(cmdText);
                }
                else
                {
                    var cmdText = @"SELECT distinct GL.Id AS AccountCodeId, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
								sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id,  VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
                                , ACT.BalanceType, ACT.Id AS [MainHead], AG.UserName AS [Level], VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode,VD.FiscalYearId
	                            FROM TRN.VoucherDetailCurrency AS VDC
		                        INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                        INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                        LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                        LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                LEFT JOIN SCS.FiscalYear AS FY ON FY.Id=VD.FiscalYearId
                                WHERE act.IsBalanceSheet=1 AND VD.FiscalYearId = '" + fiscalYearId + @"' AND V.CompanyGroupId='" + companyGroupId + @"'
                                AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                                AND V.IsPark=0 AND SourceType='OpeningBalance'
                                GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, V.PostingDate, ACT.BalanceType, AG.UserName, ACT.Id, VD.FiscalYearId";
                    return _sqlRepository.GetDataTable(cmdText);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        private DataTable GetBalanceSheetDetailData(string companyGroupId, string companyId, string plantId, string fiscalYearId)
        {
            var cmdText = @"SELECT ATY.Id AS ACCType, GLGI.AccountCode AS GLCode,VD.PartyType, GLGI.UserName AS GLGeneralInfoName, BUDM.RefNo, BUD.UserName AS Budget, ACT.UserName AS Activity,FY.FiscalYearName
                            ,[AccountTitle]=CASE WHEN P.UserName<>'' THEN P.UserName
                            WHEN EMP.EmployeeName<>'' THEN EMP.EmployeeCode+' - '+EMP.EmployeeName
                            WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
                            WHEN CM.UserName<>'' THEN CM.UserName
                            WHEN ACT.UserName<>'' THEN ACT.UserName
                            WHEN FBM.AccountTitle<>'' THEN FBM.AccountTitle
                            ELSE CM.UserName END
                            ,[AccountTitleDetail]=CASE WHEN PP.UserName<>'' THEN PP.UserName
                            WHEN BM.AccountNumber<>'' THEN BM.AccountNumber
                            WHEN CM.UserName<>'' THEN CM.UserName
                            WHEN FBM.AccountTitle<>'' THEN FBM.AccountTitle
                            ELSE CM.UserName END
                            , sum(CC.CompanyCurrencyDrAmount)-SUM(CC.CompanyCurrencyCrAmount) AS Amount
                            FROM [TRN].[VoucherDetail] AS VD
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
                            LEFT JOIN [HKP].[AccountType] AS ATY ON ATY.Id=AG.AccountTypeId
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                            LEFT JOIN [HKP].[Bank] BN ON BN.Id=BM.BankId
                            LEFT JOIN [HKP].[BankBranch] BR ON BR.Id=BM.BankBranchId
                            LEFT JOIN [MST].[BudgetMaster] AS BUDM ON BUDM.Id = VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUDM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId
                            LEFT JOIN [dbo].[EmployeeInformation] AS EMP ON EMP.SystemId=VD.EmployeeId
                            LEFT JOIN [TRN].[FinancingDetail] AS FD ON FD.Id=VD.FinancingDetailId
                            LEFT JOIN [TRN].[Financing] AS F ON F.Id=FD.FinancingId
                            LEFT JOIN [MST].[BankMaster] AS FBM ON FBM.Id=F.BankMasterId
                            LEFT JOIN SCS.FiscalYear AS FY ON FY.Id=VD.FiscalYearId
                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                            ) AS CC ON CC.VoucherDetailId=VD.Id
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.SourceType='OpeningBalance' AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + @"' 
                            AND V.FiscalYearId='" + fiscalYearId + @"'AND ATY.Id='Asset'
                            GROUP BY ATY.Id , GLGI.AccountCode, GLGI.UserName, BUDM.RefNo, BUD.UserName, ACT.UserName, FY.FiscalYearName,P.UserName,EMP.EmployeeCode,EMP.EmployeeName,
							BM.AccountTitle, CM.UserName, ACT.UserName, FBM.AccountTitle, PP.UserName, BM.AccountNumber, VD.BudgetMasterId, ACT.Id, CC.CompanyCurrencyId
							,VD.PartyType
							 UNION ALL
							 SELECT ATY.Id AS ACCType, GLGI.AccountCode AS GLCode,VD.PartyType, GLGI.UserName AS GLGeneralInfoName, BUDM.RefNo, BUD.UserName AS Budget, ACT.UserName AS Activity,FY.FiscalYearName
                            ,[AccountTitle]=CASE WHEN P.UserName<>'' THEN P.UserName
                            WHEN EMP.EmployeeName<>'' THEN EMP.EmployeeCode+' - '+EMP.EmployeeName
                            WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
                            WHEN CM.UserName<>'' THEN CM.UserName
                            WHEN ACT.UserName<>'' THEN ACT.UserName
                            WHEN FBM.AccountTitle<>'' THEN FBM.AccountTitle
                            ELSE CM.UserName END
                            ,[AccountTitleDetail]=CASE WHEN PP.UserName<>'' THEN PP.UserName
                            WHEN BM.AccountNumber<>'' THEN BM.AccountNumber
                            WHEN CM.UserName<>'' THEN CM.UserName
                            WHEN FBM.AccountTitle<>'' THEN FBM.AccountTitle
                            ELSE CM.UserName END
                            , sum(CC.CompanyCurrencyCrAmount)-SUM(CC.CompanyCurrencyDrAmount) AS Amount
                            FROM [TRN].[VoucherDetail] AS VD
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
                            LEFT JOIN [HKP].[AccountType] AS ATY ON ATY.Id=AG.AccountTypeId
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                            LEFT JOIN [HKP].[Bank] BN ON BN.Id=BM.BankId
                            LEFT JOIN [HKP].[BankBranch] BR ON BR.Id=BM.BankBranchId
                            LEFT JOIN [MST].[BudgetMaster] AS BUDM ON BUDM.Id = VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUDM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId
                            LEFT JOIN [dbo].[EmployeeInformation] AS EMP ON EMP.SystemId=VD.EmployeeId
                            LEFT JOIN [TRN].[FinancingDetail] AS FD ON FD.Id=VD.FinancingDetailId
                            LEFT JOIN [TRN].[Financing] AS F ON F.Id=FD.FinancingId
                            LEFT JOIN [MST].[BankMaster] AS FBM ON FBM.Id=F.BankMasterId
                            LEFT JOIN SCS.FiscalYear AS FY ON FY.Id=VD.FiscalYearId
                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                            ) AS CC ON CC.VoucherDetailId=VD.Id
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.SourceType='OpeningBalance' AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + @"' 
                            AND V.FiscalYearId='" + fiscalYearId + @"' AND ATY.Id IN ('Equity','Liability')
                            GROUP BY ATY.Id , GLGI.AccountCode, GLGI.UserName, BUDM.RefNo, BUD.UserName, ACT.UserName, FY.FiscalYearName,P.UserName,EMP.EmployeeCode,EMP.EmployeeName,
							BM.AccountTitle, CM.UserName, ACT.UserName, FBM.AccountTitle, PP.UserName, BM.AccountNumber, VD.BudgetMasterId, ACT.Id, CC.CompanyCurrencyId
							,VD.PartyType
							 ORDER BY 1, 2";
            return _sqlRepository.GetDataTable(cmdText);
        }


        [HttpGet, Authorize]
        public ActionResult BalanceSheetDetailReport(string fiscalYearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Balance Sheet Deatil.xlsx";
            var workbook = GetBalanceSheetDetailReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fiscalYearId);
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        public IWorkbook GetBalanceSheetDetailReport(string companyGroupId, string companyId, string plantId, string plantName, string fiscalYearId)
        {
            try
            {
                var row = 6;
                var colLast = row;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Report";

                var fiscalYear = _sqlRepository.GetData("SELECT FiscalYearCode, FiscalYearName, StartDate, EndDate FROM [SCS].[FiscalYear] WHERE Id='" + fiscalYearId + "'");
                // Get Balance Sheet Detail Data
                var dataList = GetBalanceSheetDetailData(companyGroupId, companyId, plantId, fiscalYearId);

                row++;
                reportUtility.SetHeaderText(ref sheet, row, 1, "GL", 28);
                reportUtility.SetHeaderText(ref sheet, row, 2, "Ref No", 5);
                reportUtility.SetHeaderText(ref sheet, row, 3, "Budget", 29);
                reportUtility.SetHeaderText(ref sheet, row, 4, "Account Title", 24);
                reportUtility.SetHeaderText(ref sheet, row, 5, "Account Title Detail", 32);
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                row++;
                reportUtility.SetText(ref sheet, row, 1, "Total Assets:", true);
                sheet[row, 1, row, 4].Merge();

                var assetList = dataList.Select("ACCType='Asset'");//.OrderBy(r => r.Field<string>("GLCode"));
                var totalAssetRow = assetList.Count();
                if (totalAssetRow > 0)
                {
                    sheet.Range[row, 6].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + (row + 1) + ":" + reportUtility.GetColumnNameForXls(6) + (totalAssetRow + row) + ")";
                    sheet.Range[row, 6].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 6].CellStyle.Font.Bold = true;
                    sheet.Range[row, 6].BorderAround(ExcelLineStyle.Hair);
                    for (int i = 0; i < totalAssetRow; i++)
                    {
                        row++;
                        reportUtility.SetText(ref sheet, row, 1, assetList[i]["GLCode"] + " - " + assetList[i]["GLGeneralInfoName"]);
                        reportUtility.SetText(ref sheet, row, 2, assetList[i]["RefNo"].ToString());
                        reportUtility.SetText(ref sheet, row, 3, assetList[i]["Budget"].ToString());
                        reportUtility.SetText(ref sheet, row, 4, assetList[i]["AccountTitle"].ToString());
                        reportUtility.SetText(ref sheet, row, 5, assetList[i]["AccountTitleDetail"].ToString());
                        reportUtility.SetText(ref sheet, row, 6, Convert.ToDouble(assetList[i]["Amount"].ToString()));
                    }
                }

                row++;
                reportUtility.SetText(ref sheet, row, 1, "Equity & Liability:", true);
                sheet[row, 1, row, 4].Merge();

                var laibilityList = dataList.Select("ACCType='Equity' OR  ACCType='Liability'");
                var totallaibilityRow = laibilityList.Count();
                if (totallaibilityRow > 0)
                {
                    sheet.Range[row, 6].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + (row + 1) + ":" + reportUtility.GetColumnNameForXls(6) + (totallaibilityRow + row) + ")";
                    sheet.Range[row, 6].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 6].CellStyle.Font.Bold = true;
                    sheet.Range[row, 6].BorderAround(ExcelLineStyle.Hair);
                    for (int i = 0; i < totallaibilityRow; i++)
                    {
                        row++;
                        reportUtility.SetText(ref sheet, row, 1, laibilityList[i]["GLCode"] + " - " + laibilityList[i]["GLGeneralInfoName"]);
                        reportUtility.SetText(ref sheet, row, 2, laibilityList[i]["RefNo"].ToString());
                        reportUtility.SetText(ref sheet, row, 3, laibilityList[i]["Budget"].ToString());
                        reportUtility.SetText(ref sheet, row, 4, laibilityList[i]["AccountTitle"].ToString());
                        reportUtility.SetText(ref sheet, row, 5, laibilityList[i]["AccountTitleDetail"].ToString());
                        reportUtility.SetText(ref sheet, row, 6, Convert.ToDouble(laibilityList[i]["Amount"].ToString()));
                    }
                }

                row++;
                sheet.Range[8, 1, row, 6].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[8, 1, row, 6].BorderAround(ExcelLineStyle.Hair);
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 6, "Balance Sheet Details", companyId, plantName, null);
                reportUtility.SetText(ref sheet, 5, 6, "Fiscal Year: " + fiscalYear["FiscalYearName"], ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(6) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        [HttpGet, Authorize]
        public ActionResult FixedAssetReport(string fiscalYearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Fixed Asset.xlsx";
            var workbook = _accountVoucherReportService.GetFixedAssetObReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fiscalYearId);
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        public ActionResult GetDailyTransactionReport(ReportFormat reportFormat, DateTime date, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountVoucherReportService.GetDailyTransactionReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, date, entityId);
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

        #region Salary JV

        public ActionResult SalaryJournal()
        {
            return View("~/Areas/Accounts/Views/SalaryJournal.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetSalaryJournalList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetSalaryJournalVoucherList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        public GridModel GetSalaryJournalVoucherList(GridParameter parameters, string companyGroupId, string companyId, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT V.Id, V.VoucherDate, V.PostingDate, V.DocRefNo, V.VoucherTypeId, V.CurrencyId, V.DocDate, V.EntityId, C.Code AS CurrencyCode, VD.DrAmount, V.VoucherNo
                                    , V.IsPark, V.Narration
                                    FROM TRN.[Voucher] AS V
                                    LEFT JOIN SCS.Currency AS C ON C.Id = V.CurrencyId
                                    LEFT JOIN (SELECT SUM(VD.DrAmount) AS DrAmount, VD.VoucherId FROM [TRN].[VoucherDetail] AS VD WHERE VD.DrAmount <> 0 GROUP BY VD.VoucherId
                                    ) AS VD ON VD.VoucherId=V.Id
                                    WHERE V.ExchangeType IS NULL AND V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "'AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.SourceType='" + SourceType.SalaryJournal + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        private string GetEmployeeSalaryAdvancePK()
        {
            return _pkGeneratorService.GetAutoNumber("EmployeeSalaryAdvance", PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public ActionResult ParkSalaryJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var flag = false;
            if (voucherDetailVMList.Sum(r => r.DrAmount) != voucherDetailVMList.Sum(r => r.CrAmount))
                throw new CustomException("Dr Cr not match!");


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;

            try
            {
                _companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                _companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                // INSERT INTO Voucher TABLE
                voucherVM.IsPark = true;



                voucherVM.SourceType = SourceType.SalaryJournal.ToString();
                var voucher = _voucharService.InsertVoucher(voucherVM);

                var advancePk = _advanceService.GetMaxNumber(nameof(Advance), PKGeneratorEnum.Yearly, null, voucherVM.VoucherDate);
                var employeePayablePk = _employeePayableService.GetMaxNumber();

                var currentRecord = 0;
                foreach (var voucherDetailVM in voucherDetailVMList)
                {
                    // Set to currency
                    voucherDetailVM.ToCurrencyId = companyCurrencyId;

                    // INSERT INTO VOUCHER DETAIL
                    var voucherDetail = new VoucherDetail
                    {
                        VoucherId = voucher.Id,
                        PlantId = voucherDetailVM.PlantId,
                        EntityId = voucherDetailVM.IsOB ? voucherDetailVM.EntityId : voucherVM.EntityId,
                        FiscalYearId = voucherVM.FiscalYearId,
                        FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                        PartyType = voucherDetailVM.PartyType,
                        EmployeeId = voucherDetailVM.EmployeeId,
                        PartyId = voucherDetailVM.PartyId,
                        PartyPlantId = voucherDetailVM.PartyPlantId,
                        GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                        BudgetMasterId = voucherDetailVM.BudgetMasterId,
                        CashMasterId = voucherDetailVM.CashMasterId,
                        BankMasterId = voucherDetailVM.BankMasterId,
                        ActivityId = voucherDetailVM.ActivityId,
                        CurrencyId = voucherVM.CurrencyId,
                        DrAmount = voucherDetailVM.DrAmount,
                        CrAmount = voucherDetailVM.CrAmount,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        Narration = voucherVM.Narration,
                        AddedBy = voucherVM.AddedBy,
                        AddedDate = voucherVM.AddedDate,
                        AddedFromIP = voucherVM.AddedFromIP,
                        //FixedAssetMasterId= voucherDetailVM.FixedAssetMasterId,
                        FAType = voucherDetailVM.FAType,
                    };
                    currentRecord++;
                    _voucharService.InsertVoucherDetail(voucher, voucherDetail, currentRecord);


                    //Employee Payable
                    if (voucherDetailVM.PartyType == PartyType.Employee.ToString() && voucherDetailVM.TransactionTypeId != null && voucherDetailVM.TrnType == "Payable")
                    {
                        employeePayablePk.MaxNumber++;
                        var employeePayable = new EmployeePayable
                        {
                            Id = voucherVM.VoucherDate.Year + employeePayablePk.MaxNumber.ToString(),
                            CompanyGroupId = voucherVM.CompanyGroupId,
                            CompanyId = voucherVM.CompanyId,
                            PlantId = voucherVM.PlantId,
                            EntityId = voucherDetailVM.EntityId,
                            FiscalYearId = voucherVM.FiscalYearId,
                            FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                            TaxYearId = voucherVM.TaxYearId,
                            TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                            CurrencyId = voucherVM.CurrencyId,
                            VoucherId = voucher.Id,
                            VoucherTypeId = voucherVM.VoucherTypeId,
                            EmployeeTransactionTypeId = voucherDetailVM.EmployeeTransactionTypeId,
                            EmployeeId = voucherDetailVM.EmployeeId,
                            Amount = voucherDetailVM.CrAmount,
                            PostingDate = voucherVM.PostingDate,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            Narration = voucherDetailVM.Narration,
                            SourceType = SourceType.EmployeePayable.ToString(),
                            PartyType = PartyType.Employee.ToString(),
                            VoucherDate = voucherVM.VoucherDate
                        };
                        _employeePayableService.InsertEmployeePayable(employeePayable);

                        var employeePayableDetail = new EmployeePayableDetail
                        {
                            EmployeePayableId = employeePayable.Id,
                            GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                            BudgetMasterId = voucherDetailVM.BudgetMasterId,
                            ActivityId = voucherDetailVM.ActivityId,
                            Amount = voucherDetailVM.Amount,
                            NetAmount = employeePayable.Amount
                        };
                        _employeePayableService.InsertEmployeePayableDetail(employeePayable, employeePayableDetail, 1);

                        // Set InvoiceDetail Id to voucher detail.
                        voucherDetail.EmployeePayableDetailId = employeePayableDetail.Id;
                        voucherDetail.PartyType = employeePayable.PartyType;
                    }
                    //Employee Advance
                    else if (voucherDetailVM.PartyType == PartyType.Employee.ToString() && voucherDetailVM.CrAmount > 0 && voucherDetailVM.TrnType == SalaryJVType.Advance.ToString())
                    {
                        var employeeSalaryAdvance = new EmployeeSalaryAdvance
                        {
                            CompanyGroupId = voucherVM.CompanyGroupId,
                            CompanyId = voucherVM.CompanyId,
                            PlantId = voucherVM.PlantId,
                            EntityId = voucherVM.EntityId,
                            VoucherTypeId = voucherVM.VoucherTypeId,
                            EmployeeAdvanceRequisitionId = voucherVM.RequisitionId,
                            PartyId = voucherVM.PartyId,
                            PartyPlantId = voucherVM.PartyPlantId,
                            PartyType = voucherVM.PartyType,
                            CurrencyId = voucherVM.CurrencyId,
                            Amount = voucherDetailVM.CrAmount,
                            EmployeeId = voucherDetailVM.EmployeeId,
                            VoucherDate = voucherVM.VoucherDate,
                            PostingDate = voucherVM.PostingDate,
                            DocDate = voucherVM.DocDate,
                            DocRefNo = voucherVM.DocRefNo,
                            TransactionType = EmployeeSalaryAdvanceType.AdvanceSetOff.ToString(),
                            Narration = voucherVM.Narration,
                            SourceType = voucherVM.SourceType.ToString(),
                            IsPark = voucherVM.IsPark,
                            Id = GetEmployeeSalaryAdvancePK(),
                            VoucherDetailId = voucherDetail.Id,
                            VoucherId = voucher.Id
                        };
                        AuditService.AddedLog(employeeSalaryAdvance);
                        _employeeSalaryAdvanceRepository.Insert(employeeSalaryAdvance);
                    }

                    // Set company currency.
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                    {
                        _voucharService.InsertVoucherDetailCompanyCurrency(voucherDetail, new VoucherDetailCurrency
                        {
                            DrAmount = voucherDetailVM.DrAmount * voucherVM.CompanyCurrencyRate,
                            CrAmount = voucherDetailVM.CrAmount * voucherVM.CompanyCurrencyRate,
                            FromCurrencyId = voucherVM.CurrencyId,
                            ParallelCurrencyId = companyCurrencyId,
                            ToCurrencyId = voucherVM.CurrencyId,
                            ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate,
                            ToCurrencyRate = voucherVM.CompanyCurrencyRate
                        });
                    }

                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return Json(new { Message = voucherVM.VoucherNo });
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, voucherVM.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        [HttpPost]
        public JsonResult PostSalaryJournal(string id)
        {
            _voucharService.PostJournalVoucher(id);
            return Json(new { Message = AplosMessage.Posted });
        }
        [HttpGet, Authorize]
        public ActionResult GetSalaryJournalVoucherReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountVoucherReportService.GetSalaryJournalVoucherReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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

        public void DeleteSalaryJournal(string companyId, string plantId, string voucherId)
        {
            var flag = false;
            try
            {


                // Delete Salary Journal
                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucharService.FindVoucher(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";

                //vendorAdWrsql = @"delete from trn.GLTransactionDetail where VoucherDetailId in (select Id from TRN.VoucherDetail  where VoucherId in (select Id from TRN.Voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.LoanInterestPayable.ToString() + "' AND Id = '" + voucherId + "'))";
                //vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.VoucherDetailCurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.SalaryJournal.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.VoucherDetail where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.SalaryJournal.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.voucher  where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.SalaryJournal.ToString() + "' AND Id = '" + voucherId + "'";
                vendorAdWr.Append(vendorAdWrsql);
                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        [HttpPost]
        public JsonResult DeleteSalaryJournal(string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            DeleteSalaryJournal(identity.CompanyId, identity.PlantId, voucherId);
            return Json(new { Message = AplosMessage.Updated });
        }

        #region Party Payment Status


        //[HttpPost, Authorize]
        //public ActionResult GetPartyPaymentStatusInvoiceList()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
        //}


        [HttpPost, Authorize]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            //return Json(accountsCommonService.getvoucherlistforCashchequeReprinting(parameters), JsonRequestBehavior.AllowGet);
        }


        //Invoice summary Report Downloard
        //[HttpGet, Authorize]
        //public ActionResult PartyPaymentStatusReport(string[] MasterLCList)
        //{

        //    try
        //    {
        //        //if (string.IsNullOrEmpty(MasterLCList))
        //        //    throw new Exception("Please select at least one Invoice");

        //        string masterLCList = "";

        //        foreach (var item in MasterLCList)
        //        {
        //            if (string.IsNullOrEmpty(masterLCList))
        //            {
        //                masterLCList += "''," + item;
        //            }
        //            else
        //            {
        //                masterLCList += "," + item;
        //            }

        //        }

        //        //if (string.IsNullOrEmpty(masterLCList))
        //        //   throw new Exception("Please select at least one Invoice");

        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

        //        ExcelEngine excelEngine = new ExcelEngine();

        //        IWorkbook workbook = _accountVoucherReportService.GetPartyPaymentStatusReport(excelEngine, masterLCList, identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

        //        string strFileName = "PayableSummary.xlsx";
        //        workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
        //        workbook.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(ex.Message, JsonRequestBehavior.AllowGet);

        //    }


        //    return null;
        //}

        //Details report
        [Authorize]
        public ActionResult PartyPaymentStatusDetailReport(string[] MasterLCList)
        {

            try
            {
             

                string masterLCList = "";

                foreach (var item in MasterLCList)
                {
                    if (string.IsNullOrEmpty(masterLCList))
                    {
                        masterLCList += "''," + item;
                    }
                    else
                    {
                        masterLCList += "," + item;
                    }

                }
                //if (string.IsNullOrEmpty(masterLCList))
                //    throw new Exception("Please select at least one master LC");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ExcelEngine excelEngine = new ExcelEngine();

                IWorkbook workbook = _accountVoucherReportService.GetPartyPaymentStatusDetailReport(excelEngine, masterLCList, identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
                // return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
                string strFileName = "PayableDetail.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
        }


        //PartyPaymentStatusAgingReport
        //[Authorize]
        //public ActionResult PartyPaymentStatusAgingReport(string MasterLCList)
        //{

        //    try
        //    {
        //        //if (string.IsNullOrEmpty(MasterLCList))
        //        //throw new Exception("Please select at least one Invoice");

        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        ExcelEngine excelEngine = new ExcelEngine();

        //        IWorkbook workbook = _accountVoucherReportService.GetPartyPaymentStatusAgingReport(excelEngine, MasterLCList, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.Name);
        //        // return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
        //        string strFileName = "PayableAging.xlsx";
        //        workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
        //        workbook.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(ex.Message, JsonRequestBehavior.AllowGet);

        //    }


        //    return null;
        //}


        // [HttpGet, Authorize]
        //public ActionResult GetRCMPayableReport(ReportFormat reportFormat, string fromDate, string toDate)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    var workbook = _taxReportServiceService.GetRCMPayableReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, identity.Name);
        //    var reportFileName = DateTime.Now.ToString("yyMMdd") + " RCM Payable Report";
        //    switch (reportFormat)
        //    {
        //        case ReportFormat.Pdf:
        //            return RenderReportAsPdf(workbook, reportFileName);

        //        case ReportFormat.Excel:
        //            return RenderReportAsExcel(workbook, reportFileName);

        //        default:
        //            return RenderReportAsExcel(workbook, reportFileName);
        //    }
        //}

        [HttpGet, Authorize]
        public ActionResult getDetailData(string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"SELECT ISNULL( IV.PartyId,'')PartyId
			, isnull( IV.PartyPlantId,'')PartyPlantId
			,isnull( p.Code,'') PartyCode
			,isnull(  P.UserName,'') PartyName
			,isnull( PP.UserName,'') AS PartyPlantName
               ,isnull( V.Id,'') VoucherId 
			   ,isnull( V.VoucherNo,'')VoucherNo
			   ,isnull( V.DocRefNo,'') InvoiceNo
			   ,isnull( V.SourceType,'')SourceType
		        , REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate 
				, REPLACE(CONVERT(VARCHAR(11),iv.DocDate, 106), ' ', '-') AS DocDate
				, REPLACE(CONVERT(VARCHAR(11),iv.ActualDueDate , 106), ' ', '-') AS ActualDueDate 
				 ,C.Code TrnCurrency
                , ISNULL(IVD.Amount,0) AS Gross
				--,  0 DebitNoteAmount
				--,isnull( DIWD.DiscountAmount,0)as TranDiscountAmount
				--, isnull( IWD.TaxAmount ,0)TaxAmount
              --  , SetOff=ISNULL(IVD.WrittenOffAmount, 0) -ISNULL(IWD.TaxAmount,0)-isnull( DIWD.DiscountAmount,0)
                , SetOff=ISNULL(IVD.WrittenOffAmount, 0) 
				, ISNULL(IVD.Amount-IVD.WrittenOffAmount,0) AS Balance

						 , ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0) AS BooksGross
					--	,isnull( 0*CC.CompanyCurrencyRate ,0) BooksDebitNoteAmount
					--	 ,isnull( DIWD.DiscountAmount*CC.CompanyCurrencyRate,0)as BooksDiscountAmount
				  --  ,ISNULL(IWD.TaxAmount*CC.CompanyCurrencyRate,0) BooksTaxAmount
				--,ISNULL(IVD.WrittenOffAmount*CC.CompanyCurrencyRate,0)-ISNULL(IWD.TaxAmount*CC.CompanyCurrencyRate,0)-isnull( DIWD.DiscountAmount*CC.CompanyCurrencyRate,0) AS BooksSetOff
				,ISNULL(IVD.WrittenOffAmount*CC.CompanyCurrencyRate,0) AS BooksSetOff
              , ISNULL((IVD.Amount*CC.CompanyCurrencyRate)-(IVD.WrittenOffAmount*CC.CompanyCurrencyRate),0) AS BooksBalance

                        ,NULL InventoryReceiveId  
						
				--, ISNULL(IVD.Amount,0) AS GrossTranAmount
				--,0 DebitNoteTranAmount
				--,isnull( IWD.Amount,0)as TranAmount


                FROM [TRN].[InvoiceDetail] AS IVD
                LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId=IV.Id
                LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IVD.Id
                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId
                LEFT JOIN (SELECT wd.InvoiceDetailId,sum(wd.Amount) TaxAmount  FROM TRN.InvoiceWriteOffDetail wd 
								LEFT JOIN  TRN.InvoiceWriteOff w on wd.InvoiceWriteOffId =w.id
								where w.PaymentSource='Tax'
								group by wd.InvoiceDetailId
								) IWD ON IWD.InvoiceDetailId=IVD.Id

			LEFT JOIN (SELECT wd.InvoiceDetailId,sum(wd.Amount) DiscountAmount  FROM TRN.InvoiceWriteOffDetail wd 
					    LEFT JOIN  TRN.InvoiceWriteOff w on wd.InvoiceWriteOffId =w.id
								where w.PaymentSource='Discount'
								group by wd.InvoiceDetailId
								) DIWD ON DIWD.InvoiceDetailId=IVD.Id


                LEFT JOIN (
                SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
                VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='"+identity.CompanyId+@"'
                ) AS CC ON CC.VoucherDetailId=VD.Id

                WHERE IV.Archive=0 AND IV.IsWrittenOff=0 AND IVD.IsWrittenOff=0 AND V.IsPark=0 AND IVD.IsBlock=0 AND IV.SourceType in ('VendorInvoice','PurchaseDocAcceptance','SuspensePayable','EmployeePayable')
                AND IV.CompanyGroupId='"+identity.CompanyGroupId+"' AND IV.CompanyId='"+identity.CompanyId+"'  AND IV.PlantId='"+identity.PlantId+@"'
                --GROUP BY IV.PartyId, IV.PartyPlantId, PP.UserName,P.UserName
                 AND IV.PartyId in('"+partyId+ @"')

                UNION ALL
                SELECT ISNULL( IV.PartyId,'')PartyId
			, isnull( IV.PartyPlantId,'')PartyPlantId
			,isnull( p.Code,'') PartyCode
			,isnull(  P.UserName,'') PartyName
			,isnull( PP.UserName,'') AS PartyPlantName
               ,isnull( V.Id,'') VoucherId 
			   ,isnull( V.VoucherNo,'')VoucherNo
			   ,isnull( V.DocRefNo,'') InvoiceNo
			   ,isnull( V.SourceType,'')SourceType
		        , REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate 
				, REPLACE(CONVERT(VARCHAR(11),iv.DocDate, 106), ' ', '-') AS DocDate
				, REPLACE(CONVERT(VARCHAR(11),iv.ActualDueDate , 106), ' ', '-') AS ActualDueDate 
				 ,C.Code TrnCurrency
                , ISNULL(IVD.Amount,0) AS Gross
				--,  0 DebitNoteAmount
				--,isnull( DIWD.DiscountAmount,0)as TranDiscountAmount
				--, isnull( IWD.TaxAmount ,0)TaxAmount
              --  , SetOff=ISNULL(IVD.WrittenOffAmount, 0) -ISNULL(IWD.TaxAmount,0)-isnull( DIWD.DiscountAmount,0)
                , SetOff=ISNULL(IVD.WrittenOffAmount, 0) 
				, ISNULL(IVD.Amount-IVD.WrittenOffAmount,0) AS Balance

						 , ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0) AS BooksGross
					--	,isnull( 0*CC.CompanyCurrencyRate ,0) BooksDebitNoteAmount
					--	 ,isnull( DIWD.DiscountAmount*CC.CompanyCurrencyRate,0)as BooksDiscountAmount
				  --  ,ISNULL(IWD.TaxAmount*CC.CompanyCurrencyRate,0) BooksTaxAmount
				--,ISNULL(IVD.WrittenOffAmount*CC.CompanyCurrencyRate,0)-ISNULL(IWD.TaxAmount*CC.CompanyCurrencyRate,0)-isnull( DIWD.DiscountAmount*CC.CompanyCurrencyRate,0) AS BooksSetOff
				,ISNULL(IVD.WrittenOffAmount*CC.CompanyCurrencyRate,0) AS BooksSetOff
              , ISNULL((IVD.Amount*CC.CompanyCurrencyRate)-(IVD.WrittenOffAmount*CC.CompanyCurrencyRate),0) AS BooksBalance
                 ,ISNULL( IR.Id,'') InventoryReceiveId

                FROM[TRN].[InvoiceDetail] AS IVD
                LEFT JOIN[TRN].[Invoice] AS IV ON IVD.InvoiceId = IV.Id
                LEFT JOIN[HKP].[Party] AS P ON P.Id = IV.PartyId
                LEFT JOIN[HKP].[PartyPlant] AS PP ON PP.Id = IV.PartyPlantId
                LEFT JOIN[TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId = IVD.Id
                LEFT JOIN[TRN].[Voucher] AS V ON V.Id = VD.VoucherId
                LEFT JOIN[SCS].[Currency] AS C ON C.Id = IV.CurrencyId
                LEFT JOIN[ORG].[Entity] AS EN ON EN.Id = IV.EntityId
                LEFT JOIN (SELECT wd.InvoiceDetailId,sum(wd.Amount) TaxAmount  FROM TRN.InvoiceWriteOffDetail wd 
								LEFT JOIN  TRN.InvoiceWriteOff w on wd.InvoiceWriteOffId =w.id
								where w.PaymentSource='Tax'
								group by wd.InvoiceDetailId
								) IWD ON IWD.InvoiceDetailId=IVD.Id

			LEFT JOIN (SELECT wd.InvoiceDetailId,sum(wd.Amount) DiscountAmount  FROM TRN.InvoiceWriteOffDetail wd 
					    LEFT JOIN  TRN.InvoiceWriteOff w on wd.InvoiceWriteOffId =w.id
								where w.PaymentSource='Discount'
								group by wd.InvoiceDetailId
								) DIWD ON DIWD.InvoiceDetailId=IVD.Id

                LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId = V.Id
                LEFT JOIN(
                SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
                VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                JOIN[SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId= VDC.ParallelCurrencyId
                WHERE CPC.ParallelCurrencyType= 'CompanyCurrency' AND CPC.CompanyId= '"+identity.CompanyId+@"'
                ) AS CC ON CC.VoucherDetailId = VD.Id

                WHERE IV.Archive = 0 AND IV.IsWrittenOff = 0 AND IVD.IsWrittenOff = 0 AND V.IsPark = 0 AND IVD.IsBlock = 0 AND IV.SourceType in ('InventoryPayable')
                AND IV.CompanyGroupId='" + identity.CompanyGroupId + "' AND IV.CompanyId='" + identity.CompanyId + "'  AND IV.PlantId='" + identity.PlantId + @"'
                AND IR.PurchaseDocumentAcceptanceId IS NULL
                AND IV.PartyId in('"+partyId+ @"')
                UNION ALL
                SELECT ISNULL( IV.PartyId,'')PartyId
			, isnull( IV.PartyPlantId,'')PartyPlantId
			,isnull( p.Code,'') PartyCode
			,isnull(  P.UserName,'') PartyName
			,isnull( PP.UserName,'') AS PartyPlantName
               ,isnull( V.Id,'') VoucherId 
			   ,isnull( V.VoucherNo,'')VoucherNo
			   ,isnull( V.DocRefNo,'') InvoiceNo
			   ,isnull( V.SourceType,'')SourceType
		        , REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate 
				, REPLACE(CONVERT(VARCHAR(11),iv.DocDate, 106), ' ', '-') AS DocDate
				, REPLACE(CONVERT(VARCHAR(11),iv.ActualDueDate , 106), ' ', '-') AS ActualDueDate 
				 ,C.Code TrnCurrency
                , ISNULL(IVD.Amount,0) AS Gross
				--,  0 DebitNoteAmount
				--,isnull( DIWD.DiscountAmount,0)as TranDiscountAmount
				--, isnull( IWD.TaxAmount ,0)TaxAmount
              --  , SetOff=ISNULL(IVD.WrittenOffAmount, 0) -ISNULL(IWD.TaxAmount,0)-isnull( DIWD.DiscountAmount,0)
                , SetOff=ISNULL(IVD.WrittenOffAmount, 0) 
				, ISNULL(IVD.Amount-IVD.WrittenOffAmount,0) AS Balance

						 , ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0) AS BooksGross
					--	,isnull( 0*CC.CompanyCurrencyRate ,0) BooksDebitNoteAmount
					--	 ,isnull( DIWD.DiscountAmount*CC.CompanyCurrencyRate,0)as BooksDiscountAmount
				  --  ,ISNULL(IWD.TaxAmount*CC.CompanyCurrencyRate,0) BooksTaxAmount
				--,ISNULL(IVD.WrittenOffAmount*CC.CompanyCurrencyRate,0)-ISNULL(IWD.TaxAmount*CC.CompanyCurrencyRate,0)-isnull( DIWD.DiscountAmount*CC.CompanyCurrencyRate,0) AS BooksSetOff
				,ISNULL(IVD.WrittenOffAmount*CC.CompanyCurrencyRate,0) AS BooksSetOff
              , ISNULL((IVD.Amount*CC.CompanyCurrencyRate)-(IVD.WrittenOffAmount*CC.CompanyCurrencyRate),0) AS BooksBalance
                     ,isnull( IR.Id ,'')InventoryReceiveId

              --  FROM [TRN].[AdjustmentNoteDetail] AS IVD
             --   LEFT JOIN [TRN].[AdjustmentNote] AS IV ON IVD.AdjustmentNoteId = IV.Id

				  FROM[TRN].[InvoiceDetail] AS IVD
                LEFT JOIN[TRN].[Invoice] AS IV ON IVD.InvoiceId = IV.Id

                LEFT JOIN [HKP].[Party] AS P ON P.Id = IV.PartyId
                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id = IV.PartyPlantId
              --  LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdjustmentNoteDetailId = IVD.Id
				 LEFT JOIN[TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId = IVD.Id
                LEFT JOIN [TRN].[Voucher] AS V ON V.Id = VD.VoucherId
                LEFT JOIN [SCS].[Currency] AS C ON C.Id = IV.CurrencyId
                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id = IV.EntityId
                LEFT JOIN (SELECT wd.InvoiceDetailId,sum(wd.Amount) TaxAmount  FROM TRN.InvoiceWriteOffDetail wd 
								LEFT JOIN  TRN.InvoiceWriteOff w on wd.InvoiceWriteOffId =w.id
								where w.PaymentSource='Tax'
								group by wd.InvoiceDetailId
								) IWD ON IWD.InvoiceDetailId=IVD.Id

			LEFT JOIN (SELECT wd.InvoiceDetailId,sum(wd.Amount) DiscountAmount  FROM TRN.InvoiceWriteOffDetail wd 
					    LEFT JOIN  TRN.InvoiceWriteOff w on wd.InvoiceWriteOffId =w.id
								where w.PaymentSource='Discount'
								group by wd.InvoiceDetailId
								) DIWD ON DIWD.InvoiceDetailId=IVD.Id

                LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId = V.Id
                LEFT JOIN(
                SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
                VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                JOIN[SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId= VDC.ParallelCurrencyId
                WHERE CPC.ParallelCurrencyType= 'CompanyCurrency' AND CPC.CompanyId= '"+identity.CompanyId+@"'
                ) AS CC ON CC.VoucherDetailId = VD.Id

                WHERE IV.Archive = 0 AND IV.IsWrittenOff = 0 AND IVD.IsWrittenOff = 0 AND V.IsPark = 0  AND IV.SourceType in ('DebitNote','VendorPayment')
                AND IV.CompanyGroupId='" + identity.CompanyGroupId + "' AND IV.CompanyId='" + identity.CompanyId + "'  AND IV.PlantId='" + identity.PlantId + @"'
                AND IV.PartyId in('"+partyId+@"')
                order by isnull(  P.UserName,'') ";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        //GetPartyPaymentStatusAgingList
        //[HttpPost, Authorize]
        //public ActionResult GetPartyPaymentStatusAgingList()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_accountVoucherReportService.GetPartyPaymentStatusAgingPiaChartData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        //}

        //Aging Due List Popup
        //[HttpGet, Authorize]
        //public ActionResult GetPartyAgingDueList(string overDueDetailAmount)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json( _accountVoucherReportService.GetPartyAgingDueData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, overDueDetailAmount), JsonRequestBehavior.AllowGet);
        //}

        #endregion Party Payment Status

        #region Fixed Assets 

        //[HttpPost, Authorize]
        //public ActionResult GetFixedAssetsList()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(new { DATA = _accountVoucherReportService.GetFixedAssetsListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost, Authorize]
        //public ActionResult GetFixedArticalList(string materialMasterId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(new { DATA = _accountVoucherReportService.GetFixedArticalListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, materialMasterId), Error = false }, JsonRequestBehavior.AllowGet);
        //}
        #endregion

        #region Round Off Journal
       
        public ActionResult RoundOffJournal()
        {
            return View("~/Areas/Accounts/Views/RoundOffJournal.cshtml");
        }

        [HttpPost, Authorize]
        public JsonResult GetTrailBalanceRoundOffList(string trnType)
        {
            AccountsGLService accountsGLService = new AccountsGLService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var res = accountsGLService.GetTrailBalanceRoundOffList(identity.PlantId, trnType);
            var jsondata = Json(res, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [HttpPost]
        public JsonResult ParkRoundOffJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
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
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _voucharService.InsertVoucher(voucherVM, voucherDetailVMList)) });
        }
        [HttpPost]
        public JsonResult PostRoundOffJournal(string id)
        {
            _voucharService.PostJournalVoucher(id);
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpPost]
        public ActionResult DeleteRoundOffJV(string voucherId)
        {
            _voucharService.DeleteJV(voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }


}