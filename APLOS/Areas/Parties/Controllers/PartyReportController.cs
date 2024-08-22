using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Service.Parties;
using Syncfusion.XlsIO;
using System;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Parties.Controllers
{
    public class PartyReportController : BaseController
    {
        private readonly IPartyReportService _partyReportService;

        public PartyReportController(IPartyReportService partyReportService)
        {
            _partyReportService = partyReportService;
        }


        public ActionResult PartyLedgerReport()
        {
            return View("~/Areas/Parties/Views/PartyLedgerReport.cshtml");
        }
        public ActionResult PartyPaymentStatusReport()
        {
            return View("~/Areas/Parties/Views/partyPaymentStatusReport.cshtml");
        }
        [Authorize, HttpGet]
        public ActionResult InterpartyLeadger()
        {
            return View("~/Areas/Parties/Views/interpartyLeadger.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetPartyLedgerReport(ReportFormat reportFormat, PartyType partyType, string partyId, string partyPlantId, string gSTINId, string fromDate, string toDate, string glId, bool active, string reportLongSize)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           // var ReportLong = "reportLongSize";
            if (reportLongSize == "LongSizeReport")
            {

                if (active)
                {
                        var workbook = _partyReportService.GetPartyLedgerReportGroupByGLReportLongSizeXls(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, partyType, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId);
                        var reportFileName = DateTime.Now.ToString("yyMMdd") + " Party Ledger";

                        return RenderReportAsExcel(workbook, reportFileName);
                   
                }
                else if(partyType == PartyType.Party)
                {
                    var workbook = _partyReportService.GetPartyLedgerReportBothCustomerVendor(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, partyType, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId);
                    var reportFileName = DateTime.Now.ToString("yyMMdd") + "Party Ledger Both Customer and Vendor";

                    if (reportFormat == ReportFormat.Pdf)
                    {
                        return RenderReportAsPdf(workbook, reportFileName);
                    }
                    else
                    {
                        return RenderReportAsExcel(workbook, reportFileName);
                    }
                }
                else
                {
                    var workbook = _partyReportService.GetPartyLedgerReportLongSizeXls(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, partyType, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId);
                    var reportFileName = DateTime.Now.ToString("yyMMdd") + "Party Ledger";

                    if (reportFormat == ReportFormat.Pdf)
                    {
                        return RenderReportAsPdf(workbook, reportFileName);
                    }
                    else
                    {
                        return RenderReportAsExcel(workbook, reportFileName);
                    }
                }
            }
            else
            {

                if (active)
                {
                    var workbook = _partyReportService.GetPartyLedgerReportGroupByGL(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, partyType, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId);
                    var reportFileName = DateTime.Now.ToString("yyMMdd") + " Party Ledger";

                    if (reportFormat == ReportFormat.Pdf)
                    {
                        return RenderReportAsPdf(workbook, reportFileName);
                    }
                    else
                    {
                        return RenderReportAsExcel(workbook, reportFileName);

                    }
                }
                else
                {
                    if (partyType == PartyType.Party)
                    {
                        var workbook = _partyReportService.GetPartyLedgerReportBothCustomerVendor(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, partyType, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId);
                        var reportFileName = DateTime.Now.ToString("yyMMdd") + "Party Ledger Both Customer and Vendor";
                        
                        if (reportFormat == ReportFormat.Pdf)
                        {
                            return RenderReportAsPdf(workbook, reportFileName);
                        }
                        else
                        {
                            return RenderReportAsExcel(workbook, reportFileName);
                        }
                    }
                    else
                    {
                        var workbook = _partyReportService.GetPartyLedgerReportXls(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, partyType, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId);
                        var reportFileName = DateTime.Now.ToString("yyMMdd") + "Party Ledger";
                        if (reportFormat == ReportFormat.Pdf)
                        {
                            return RenderReportAsPdf(workbook, reportFileName);
                        }
                        else
                        {
                            return RenderReportAsExcel(workbook, reportFileName);
                        }
                    }
                    
                }
            }



        }
        [HttpGet, Authorize]
        public ActionResult GetPartyCategoryLedgerReport(ReportFormat reportFormat, string partyType, string partyCategoryId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _partyReportService.GetPartyCategoryLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, partyType, partyCategoryId, fromDate, toDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Party Category Ledger";

            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcelx(workbook, reportFileName);
            } 
        }




        #region Inter Party Leadger

        [HttpGet, Authorize]
        public ActionResult GetInterPartyLedger(ReportFormat reportFormat, string CompanyId, string PlantId, string FromDate, string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _partyReportService.GetInterPartyLedger(identity.CompanyGroupId, CompanyId, PlantId, identity.PlantName, FromDate, ToDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + "Inter Transaction Ledger";
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

        #endregion  InterPartyLeadger

        [Authorize, HttpGet]
        public ActionResult PartyOutstandingLedgerReport()
        {
            return View("~/Areas/Parties/Views/PartyLedgerOutstandingReport.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetPartyOutstandingLedgerReport(ReportFormat reportFormat, PartyType partyType, string partyId, string partyPlantId, string gSTINId, string fromDate, string toDate, string glId, bool active)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _partyReportService.GetPartyOutstandingReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, partyType, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Party Outstanding Ledger";
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

        
        public ActionResult PartyOutstandingReport()
        {
            return View("~/Areas/Parties/Views/PartyOutstandingReport.cshtml");
        }

        
        public ActionResult PartyOpeningBalanceLedger()
        {
            return View("~/Areas/Parties/Views/PartyOpeningBalanceLedger.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetPartyOpeningBalanceLedgerReport(ReportFormat reportFormat, string partyId, string partyPlantId, string fiscalYearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _partyReportService.GetPartyOpeningBalanceLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, partyId, partyPlantId, fiscalYearId);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Party Opening Balance Ledger";
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
        public ActionResult GetPartyOutstadningReport(ReportFormat reportFormat, DateTime toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _partyReportService.PartyOutstadningReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId,identity.PlantName, "Outstanding Report", SourceType.CustomerInvoice, toDate);
            workbook.SaveAs("Party Outstanding Report.xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Party Outstanding Report";
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

        #region Party payment status report
        [HttpGet, Authorize]
        public ActionResult GetPartyPaymentStatusReport(ReportFormat reportFormat, PartyType partyType, string partyId, string partyPlantId, string gSTINId, string fromDate, string toDate, string glId, bool active, string partyName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //var workbook = "";  

            if (active)
            {
                if (reportFormat == ReportFormat.Pdf)
                {
                    var workbook = _partyReportService.GetPartyPaymentStatusReportGL(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, partyType, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId);
                    var reportFileName = partyName + '-' + DateTime.Now.ToString("yyMMdd") + " Party Payment Status Report";

                    return RenderReportAsPdf(workbook, reportFileName);
                }
                else
                {
                    var workbook = _partyReportService.GetPartyPaymentStatusReportGroupByGLXls(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, partyType, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId);
                    var reportFileName = partyName + '-' + DateTime.Now.ToString("yyMMdd") + " Party Payment Status Report";

                    return RenderReportAsExcel(workbook, reportFileName);

                }
            }
            else
            {
                if (reportFormat == ReportFormat.Pdf)
                {
                    var workbook = _partyReportService.GetPartyPaymentStatusLedgerReport3(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, partyType, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId);
                    var reportFileName = partyName + '-' + DateTime.Now.ToString("yyMMdd") + " Party Payment Status Report";


                    return RenderReportAsPdf(workbook, reportFileName);
                }
                else
                {
                    var workbook = _partyReportService.GetPartyPaymentStatusLedgerReportXls(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, partyType, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId);
                    var reportFileName = partyName + '-' + DateTime.Now.ToString("yyMMdd") + " Party Payment Status Report";

                    return RenderReportAsExcel(workbook, reportFileName);
                }
            }


        }
        [HttpGet, Authorize]
        public ActionResult GetShortPartyPaymentStatusReport(ReportFormat reportFormat, PartyType partyType, string partyId, string partyPlantId, string gSTINId, string fromDate, string toDate, string glId, bool active,string partyName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //var workbook = "";  

            if (active)
            {
                var workbook = _partyReportService.GetShortPartyPaymentStatusLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, partyType, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId, partyName);
                var reportFileName = partyName+'_'+ DateTime.Now.ToString("yyMMdd") + " Party Payment Status Report";
                if (reportFormat == ReportFormat.Pdf)
                {
                    return RenderReportAsPdf(workbook, reportFileName);
                }
                else
                {
                    return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            else
            {
                var workbook = _partyReportService.GetShortPartyPaymentStatusLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, partyType, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId, partyName);
                var reportFileName = partyName + '_' + DateTime.Now.ToString("yyMMdd") + " Party Payment Status Report";
                if (reportFormat == ReportFormat.Pdf)
                {
                    return RenderReportAsPdf(workbook, reportFileName);
                }
                else
                {
                    return RenderReportAsExcel(workbook, reportFileName);
                }
            }
        }

        #endregion party payment status report

    }
}