using Aplos.Controllers;
using Library.Accounting.Accounts;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Banks;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Banks.Controllers
{
    public class BankReportController : BaseController
    {
        private readonly IBankReportService _bankReportService;
        private readonly ISqlRepository _sqlRepository;
        public BankReportController(
            IBankReportService bankReportService, ISqlRepository sqlRepository)
        {
            _bankReportService = bankReportService;
            _sqlRepository = sqlRepository;
        }




        public ActionResult BankOpeningBalanceLedger()
        {
            return View("~/Areas/Banks/Views/BankOpeningBalanceLedger.cshtml");
        }




        [HttpGet, Authorize]
        public ActionResult GetBankJournalReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _bankReportService.GetPaymentByBankReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.BankJournal);
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
        public ActionResult GetPaymentByBankReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _bankReportService.GetPaymentByBankReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.PaymentByBank);
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
        public ActionResult GetReceiptByBankReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _bankReportService.GetPaymentByBankReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.ReceiptByBank);
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
        public ActionResult GetBankOpeningBalanceLedgerReport(string fiscalYearId, bool isCompanyCurrency)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _bankReportService.GetBankOpeningBalanceLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fiscalYearId, isCompanyCurrency);
            workbook.SaveAs(DateTime.Now.ToString("yy") + " Bank Opening Balance Ledger.xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
            return null;
        }


        public ActionResult BankLedgerReport()
        {
            return View("~/Areas/Banks/Views/BankLedgerReport.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetBankLedgerReport(ReportFormat reportFormat, string bankMasterId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _bankReportService.GetBankLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, bankMasterId, fromDate, toDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Bank Ledger";
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


        public ActionResult BankReconcileReport()
        {
            return View("~/Areas/Banks/Views/BankReconcileReport.cshtml");
        }
        [HttpGet, Authorize]
        public ActionResult GetBankReconcileReport(ReportFormat reportFormat, string bankMasterId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _bankReportService.GetBankReconcileReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, bankMasterId, fromDate, toDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Bank Ledger";
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


        public ActionResult BankBookReport()
        {
            return View("~/Areas/Banks/Views/BankBookReport.cshtml");
        }

        public ActionResult BankSheetGeneration()
        {
            return View("~/Areas/Banks/Views/BankSheetGeneration.cshtml");
        }

        #region Bank Sheet Generation Report
        [HttpGet, Authorize]
        public ActionResult GetBankSheetGenerationReport(string fromDate, string toDate, string bankMasterId, string PartyList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsBankService accountsBankService = new AccountsBankService(_sqlRepository);
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();

                IWorkbook workbook = accountsBankService.GetBankSheetGenerationReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate, bankMasterId, PartyList);
                string strFileName = "BankSheetGeneration.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (CustomException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }

        [HttpPost, Authorize]
        public ActionResult GetPartyDateWise(string fromDate, string toDate)
        {
            return Json(GetPartyDateWiseData(fromDate, toDate), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> GetPartyDateWiseData(string fromDate, string toDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = @"DECLARE @companyGroupId VARCHAR(10)='" + identity.CompanyGroupId + @"';
            DECLARE @companyId VARCHAR(10)='" + identity.CompanyId + @"';
            DECLARE @plantId VARCHAR(10)='" + identity.PlantId + @"';
            DECLARE @bankMasterId VARCHAR(10) = 'null';
                SELECT  
               PartyId = STUFF((select distinct ',' + XP.Id from
               TRN.VoucherDetail XVD JOIN[HKP].[Party] AS XP ON XP.Id = XVD.PartyId
            where XVD.VoucherId = V.Id AND XVD.PartyId <> ''  for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
            ,IsSelect = CONVERT(bit,'False')
			,PartyCode = STUFF((select distinct ',' + XP.Code from
               TRN.VoucherDetail XVD JOIN[HKP].[Party] AS XP ON XP.Id = XVD.PartyId
            where XVD.VoucherId = V.Id AND XVD.PartyId <> ''  for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

			,Party = STUFF((select distinct ',' + XP.UserName from
               TRN.VoucherDetail XVD JOIN[HKP].[Party] AS XP ON XP.Id = XVD.PartyId
            where XVD.VoucherId = V.Id AND XVD.PartyId <> ''  for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

			,PartyAccountGroupName = STUFF((select distinct ',' + PAG.UserName from
               TRN.VoucherDetail XVD JOIN[HKP].[Party] AS XP ON XP.Id = XVD.PartyId

            LEFT JOIN[HKP].[CompanyParty] AS CP ON CP.PartyId = XP.Id
            LEFT JOIN[HKP].[PartyAccountGroup] AS PAG ON PAG.Id = CP.PartyAccountGroupId
            where XVD.VoucherId = V.Id AND XVD.PartyId <> ''  for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

			,Currency = STUFF((select distinct ',' + CU.Code from
               TRN.VoucherDetail XVD JOIN[HKP].[Party] AS XP ON XP.Id = XVD.PartyId

            LEFT JOIN[HKP].[CompanyParty] AS CP ON CP.PartyId = XP.Id
            LEFT JOIN SCS.Currency AS CU ON CU.Id = CP.CurrencyId
            where XVD.VoucherId = V.Id AND XVD.PartyId <> ''  for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
			,Country = STUFF((select distinct ',' + C.UserName from
               TRN.VoucherDetail XVD JOIN[HKP].[Party] AS XP ON XP.Id = XVD.PartyId

            left join MST.AddressMaster am on am.Id = XP.AddressMasterId

            left join SCS.Country C on C.Id = am.CountryId
            where XVD.VoucherId = V.Id AND XVD.PartyId <> ''  for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

			,PartyState = STUFF((select distinct ',' + C.UserName from
               TRN.VoucherDetail XVD JOIN[HKP].[Party] AS XP ON XP.Id = XVD.PartyId

            left join MST.AddressMaster am on am.Id = XP.AddressMasterId

            left join SCS.State C on C.Id = am.StateId
            where XVD.VoucherId = V.Id AND XVD.PartyId <> ''  for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

            FROM[TRN].[VoucherDetail] AS VD
            LEFT JOIN[TRN].[Voucher] AS V ON V.Id = VD.VoucherId
            LEFT JOIN[MST].[BankMaster] AS BM ON BM.Id = VD.BankMasterId
            LEFT JOIN[HKP].[Bank] AS BN ON BN.Id = BM.BankId
            LEFT JOIN[MST].[CashMaster] AS CM ON CM.Id = VD.CashMasterId
            LEFT JOIN[HKP].[Party] AS P ON P.Id = VD.PartyId
            WHERE V.Archive = 0 AND V.IsPark =1 AND V.CompanyGroupId = @companyGroupId AND V.CompanyId = @companyId AND V.PlantId = @plantId
           AND VD.BankMasterId <> ''
            AND V.PostingDate BETWEEN '" + fromDate + "' AND '" + toDate + @"' AND V.SourceType = 'VendorPayment'

            UNION ALL
			
			SELECT distinct  MPD.PartyId,IsSelect = CONVERT(bit,'False'),P.Code PartyCode,P.UserName Party
			,PAG.UserName PartyAccountGroupName,CU.Code Currency,C.UserName Country,S.UserName PartyState
			FROM TRN.MultiplePaymentDetail MPD 
			JOIN TRN.MultiplePayment MU ON MU.Id=MPD.MultiplePaymentId 
			LEFT JOIN [HKP].[Party] AS P ON P.Id = MPD.PartyId
			LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId = P.Id and CP.PartyType='Vendor' and CP.PlantId=@plantId
            LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id = CP.PartyAccountGroupId and PAG.AccountType='Vendor'
			LEFT JOIN SCS.Currency CU ON CU.Id=CP.CurrencyId
			left join MST.AddressMaster am on am.Id = P.AddressMasterId
            left join SCS.Country C on C.Id = am.CountryId
			left join SCS.[State] S on S.Id = am.StateId
			WHERE MU.IsPark=1 AND MU.TentativeDate BETWEEN '" + fromDate + "' AND '" + toDate + @"' AND MU.SourceType = 'VendorPayment'

            ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion Bank Sheet Generation Report



        [HttpGet, Authorize]
        public ActionResult GetBankBookReport(ReportFormat reportFormat, string bankMasterId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _bankReportService.GetBankBookReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, bankMasterId, fromDate, toDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Bank Book";
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
}