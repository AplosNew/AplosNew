using Aplos.Controllers;
using Library.Accounting.Accounts;
using Library.Crosscutting.Security;
using Library.Model.Enums;
using Library.Model.Parties;
using System;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class TaxReportController : BaseController
    {

        private readonly TaxReportService _taxReportServiceService;

        public TaxReportController(
               TaxReportService taxReportServiceService)
        {
            _taxReportServiceService = taxReportServiceService;
        }
        public ActionResult RCMTaxPayable()
        {
            return View();
        }
        public ActionResult RCMTaxPayableSales()
        {
            return View();
        }
        //RCMTaxReceivable
        public ActionResult RCMTaxReceivable()
        {
            return View();
        }
        public ActionResult RCMTaxReceivableSales()
        {
            return View();
        }
        public ActionResult TDSDeductionReport()
        {
            return View();
        }

        public ActionResult GSTReceivableReport()
        {
            return View();
        }
        public ActionResult DebitNoteCreditNoteTaxReport()
        {
            return View();
        }
        public ActionResult PaymentPendingforSetOffReport()
        {
            return View();
        }
        public ActionResult GSTPayableSalesReport()
        {
            return View();
        }
        
        public ActionResult GSTR2()
        {
            return View();
        }



        [HttpGet, Authorize]
        public ActionResult GetRCMPayableReport(ReportFormat reportFormat, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _taxReportServiceService.GetRCMPayableReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName,  fromDate, toDate,identity.Name);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " RCM Payable Report";
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
        public ActionResult GetRCMPayableSalesReport(ReportFormat reportFormat, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _taxReportServiceService.GetRCMPayableSalesReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, identity.Name);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " RCM Payable Sales Report";
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
        public ActionResult GetRCMReceivableReport(ReportFormat reportFormat, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _taxReportServiceService.GetRCMReceivableReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, identity.Name);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " RCM Receivable Report";
            switch (reportFormat)
            {
                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetRCMReceivableSalesReport(ReportFormat reportFormat, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _taxReportServiceService.GetRCMReceivableSalesReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, identity.Name);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " RCM Receivable Sales Report";
            switch (reportFormat)
            {
                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetGSTReceivableReport(ReportFormat reportFormat, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _taxReportServiceService.GetGSTReceivableReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, identity.Name);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + "GST Report";
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
        public ActionResult GetGSTReceivableReport2(ReportFormat reportFormat, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _taxReportServiceService.GetGSTReceivableReport2(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, identity.Name);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " GST Report";
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
        public ActionResult GetGSTReceivableReport3(ReportFormat reportFormat, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _taxReportServiceService.GetGSTReceivableReport3(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, identity.Name);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + "GST Report";
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcelx(workbook, reportFileName);

                default:
                    return RenderReportAsExcelx(workbook, reportFileName);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetGSTReceivableReport4(ReportFormat reportFormat, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _taxReportServiceService.GetGSTReceivableReport4(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, identity.Name);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + "GST Report";
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcelx(workbook, reportFileName);

                default:
                    return RenderReportAsExcelx(workbook, reportFileName);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetDebitNoteCreditNoteTaxReport(ReportFormat reportFormat, string fromDate, string toDate, PartyType partyType, string noteType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _taxReportServiceService.GetDebitNoteCreditNoteTaxReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, partyType, noteType, identity.Name);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + "DebitNoteCreditNoteStatusReport";
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcelx(workbook, reportFileName);

                default:
                    return RenderReportAsExcelx(workbook, reportFileName);
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetPaymentPendingforSetOffReport(ReportFormat reportFormat, string reportType, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Syncfusion.XlsIO.IWorkbook workbook = null;
            var reportFileName = "";
            if(reportType== "Advance")
            {
                workbook = _taxReportServiceService.GetAdvancePaymentPendingforSetOffReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, identity.Name);
                reportFileName = DateTime.Now.ToString("yyMMdd") + "AdvancePaymentPendingforSetOffReport";
            }
            if (reportType == "DebitNote")
            {
                workbook = _taxReportServiceService.GetDebitNotePaymentPendingforSetOffReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, identity.Name);
                reportFileName = DateTime.Now.ToString("yyMMdd") + "DebitNotePaymentPendingforSetOffReport";
            }
            if (reportType == "CreditNote")
            {
                workbook = _taxReportServiceService.GetCreditNotePaymentPendingforSetOffReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, identity.Name);
                reportFileName = DateTime.Now.ToString("yyMMdd") + "CreditNotePaymentPendingforSetOffReport";
            }

            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcelx(workbook, reportFileName);

                default:
                    return RenderReportAsExcelx(workbook, reportFileName);
            }
        }

        #region GST Payable

        #endregion GST Payable





        [HttpGet, Authorize]
        public ActionResult GetGSTPayableSalesReport(ReportFormat reportFormat, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _taxReportServiceService.GetGSTPayableSalesReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, identity.Name);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + "GST Payable Sales Report";
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
        public ActionResult GetGSTPayableSalesReport2(ReportFormat reportFormat, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _taxReportServiceService.GetGSTPayableSalesReport2(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, identity.Name);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " GST Payable Sales Report";
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
        public ActionResult GetGSTPayableSalesReport3(ReportFormat reportFormat, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _taxReportServiceService.GetGSTPayableSalesReport3(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, identity.Name);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " GST Payable Sales Report";
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
        public ActionResult GetGSTR2Report(ReportFormat reportFormat, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _taxReportServiceService.GetGSTR2Report(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, identity.Name);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " GST Report";
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



        //GetTdsDeductionReport
        [HttpGet, Authorize]
        public ActionResult GetTdsDeductionReport(ReportFormat reportFormat, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _taxReportServiceService.GetTdsDeductionReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, identity.Name);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " TDS Deduction Report";
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
        public ActionResult GetTCSDeductionReport(ReportFormat reportFormat, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _taxReportServiceService.GetTCSDeductionReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fromDate, toDate, identity.Name);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " TCS Deduction Report";
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

        [HttpPost, Authorize]
        public ActionResult GetGSTDetailReport(string FromDate, string ToDate)
        {
            try
            {
                string fileName = ""; 
                fileName = _taxReportServiceService.GSTDetailReport(FromDate, ToDate, "GST Detail Report");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }


}