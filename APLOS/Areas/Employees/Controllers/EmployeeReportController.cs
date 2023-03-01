using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Model.Enums;
using Library.Service.Employees;
using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;
using Library.Service.Helpers;
using Microsoft.Reporting.WebForms;
using Library.Accounting.Accounts;

namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeReportController : BaseController
    {
        private readonly IEmployeeReportService _employeeReportService;
        private readonly AccountVoucherReportService _accountVoucherReportService;

        public EmployeeReportController(IEmployeeReportService employeeReportService, AccountVoucherReportService accountVoucherReportService)
        {
            _employeeReportService = employeeReportService;
            _accountVoucherReportService = accountVoucherReportService;
        }

        
        public ActionResult EmployeeLedgerReport()
        {
            return View("~/Areas/Employees/Views/EmployeeLedgerReport.cshtml");
        }
        [Authorize]
        public ActionResult MyappEmployeeLedger()
        {
            return View("~/Areas/Employees/Views/MyappEmployeeLedger.cshtml");
        }
        [HttpGet, Authorize]
        public ActionResult GetEmployeeLedgerReport(ReportFormat reportFormat, string employeeId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountVoucherReportService.GetEmployeeLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, employeeId, fromDate, toDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Employee Ledger";
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
        public ActionResult GetMyAppEmployeeLedgerReport(ReportFormat reportFormat, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountVoucherReportService.GetEmployeeLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, identity.EmployeeId, fromDate, toDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Employee Ledger";
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

        public ActionResult EmployeeLedgerOpeningBalanceReport()
        {
            return View("~/Areas/Employees/Views/EmployeeLedgerOpeningBalanceReport.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeLedgerOpeningBalanceReport(ReportFormat reportFormat, string fiscalYearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _employeeReportService.GetEmployeeOpeningBalanceLedgerReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fiscalYearId);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Employee Opening Balance Ledger";
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

        
        public ActionResult EmployeeExpenseBookingReport()
        {
            return View("~/Areas/Employees/Views/EmployeeExpenseBookingReport.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeExpenseBookingReport(ReportFormat reportFormat, string employeeId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _employeeReportService.GetEmployeeExpenseBookingReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, employeeId, fromDate, toDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Employee Expense";
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
        public ActionResult GetAssetRegisterExpenseBookingReport(ReportFormat reportFormat, string fixedAssetRegisterId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _employeeReportService.GetAssetRegisterExpenseBookingReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, fixedAssetRegisterId, fromDate, toDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + " Asset Expense";
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
        public ActionResult GetExpensesBookingReport(ReportFormat reportFormat, string expensesBookingId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "ExpensesBooking " + expensesBookingId + "";
            var workbook = _employeeReportService.GetExpensesBookingReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, expensesBookingId);
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
        public ActionResult GetCashExpenseReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _employeeReportService.GetCashExpenseReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.CashExpenses);

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

        public ActionResult GetEmployeePayableReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountVoucherReportService.GetEmployeePayableReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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

        public ActionResult GetEmployeeSalaryPayableReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountVoucherReportService.GetEmployeeSalaryPayableReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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
        public ActionResult GetEmployeePayableExpenseReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _employeeReportService.GetEmployeePayableExpenseBookingReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
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
        public ActionResult GetEmployeePaymentReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _employeeReportService.GetEmployeePayment(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcelx(workbook, reportFileName);

                default:
                    return View();
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetEmployeePaymentRdlcReport(string voucherId)
        {

            string PdfLocation = string.Empty;
            try
            {


                ReportUtility oReportUtility = new ReportUtility();

                DataTable dtBioDvAC = null;
                DataTable dtCompanny = null;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                dtCompanny = _employeeReportService.CompanyHeader(identity.CompanyId);


                LocalReport localReport = new LocalReport
                {
                    ReportPath = Server.MapPath("~/Areas/Accounts/Reports/EmployeePaymentReport.rdlc")
                };
                //localReport.ReportPath = Server.MapPath("/EmployeePaymentReport.rdlc");


                ReportDataSource reportDataSource = new ReportDataSource
                {
                    Name = "AccountsDataSet"
                };


                dtBioDvAC = _employeeReportService.GetEmployeePayablePayment(identity.CompanyId, voucherId);
                // dtBioDvAC = dsBioDvAC.Tables[0];

                reportDataSource.Value = dtBioDvAC;


                string CompanyName = string.Empty;
                string CompanyAddress = string.Empty;
                string TotalAmmountInWord = string.Empty;
                double TotalAmmount = 0;
                if (dtCompanny.Rows.Count > 0)
                {

                    CompanyName = dtCompanny.Rows[0]["UserName"].ToString();
                    CompanyAddress = dtCompanny.Rows[0]["Address1"].ToString();


                }
                //if (dtBioDvAC.Rows.Count > 0)
                //{
                //    for (int i = 0; i < dtBioDvAC.Rows.Count; i++)
                //    {
                //        if (Convert.ToDouble(dtBioDvAC.Rows[i]["DrAmount"].ToString())>0)
                //        {
                //            TotalAmmount = TotalAmmount + Convert.ToDouble(dtBioDvAC.Rows[0]["DrAmount"].ToString());
                //        }

                //    }

                //}
                TotalAmmount = Convert.ToDouble(dtBioDvAC.Compute("SUM(DrAmount)", string.Empty));
                TotalAmmountInWord = oReportUtility.InWord(TotalAmmount, dtBioDvAC.Rows[0]["CurrencyId"].ToString());
                //CompanyName = identity.CompanyName;
                //CompanyAddress = oReportUtility.CompanyHeader();

                ReportParameter[] parameter = new ReportParameter[]
                {
                    new ReportParameter("CompanyName", CompanyName),
                    new ReportParameter("CompanyAddress", CompanyAddress),
                    new ReportParameter("TotalAmmountInWord", TotalAmmountInWord),
                    new ReportParameter("UserID", identity.Name)

                };
                localReport.SetParameters(parameter);
                //reportDataSource.Value = db.OnlineApplications.Where(x => x.StudentCode == "2019-Three-001").FirstOrDefault();

                localReport.DataSources.Add(reportDataSource);

                string ReportType = "pdf";
                string reportType = ReportType;
                String mimeType = string.Empty;
                String encoding = string.Empty;
                String extension = ReportType == "Excel" ? "xlsx" : "pdf";
                //String extension =  "png";
                Warning[] warnings = null;
                string[] streamids = null;
                Byte[] bytes = null;

                bytes = localReport.Render(reportType, "", out mimeType, out encoding, out extension, out streamids, out warnings);
                string PDFPath = new DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath("~/")) + "PDF\\";
                string fileName = "EmployeePaymentReport" + DateTime.Now.ToString("dd-MMM-yyyy") + identity.UserId + ".pdf";
                //string fileName = "iDCard" + DateTime.Now.ToFileTime() + ".png";
                bool IsExitsPDF = System.IO.File.Exists(PDFPath + fileName);
                FileStream fs = new FileStream(PDFPath + fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                byte[] data = new byte[fs.Length];
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
              var keyname=  System.Configuration.ConfigurationManager.AppSettings["APP_NAME"];
                //PdfLocation =   keyname+"/PDF/" + fileName;
                PdfLocation = "/odysseypop/PDF/" + fileName;

                //report.Attributes["src"] = PdfLocation;
                ViewBag.ReportPath = PdfLocation;
                string path = Server.MapPath("/PDF/");

                fileName = "EmployeePaymentReport" + DateTime.Now.AddDays(-1).ToString("dd-MMM-yyyy") + identity.UserId + ".pdf"; ;
                if (System.IO.File.Exists(path + fileName))
                {
                    try
                    {
                        System.IO.File.Delete(path + fileName);
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }


            }
            catch (Exception ex)
            {
                throw ex;

            }




            //  var workbook = _employeeReportService.GetEmployeePayment(out string reportFileName, identity.CompanyId, identity.PlantName, voucherId);
            //return File(PdfLocation, "application/pdf");
            return View("~/Areas/Accounts/Views/EmployeePaymentReport.cshtml");

        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeLedgerRdlcReport(string voucherId)
        {
            FileStream fs;
            string fileName;
            string PdfLocation = string.Empty;
            try
            {


                ReportUtility oReportUtility = new ReportUtility();

                DataTable dtBioDvAC = null;
                DataTable dtCompanny = null;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                dtCompanny = _employeeReportService.CompanyHeader(identity.CompanyId);


                LocalReport localReport = new LocalReport();
                localReport.ReportPath = Server.MapPath("~/Areas/Accounts/Reports/EmployeePaymentReport.rdlc");
                //localReport.ReportPath = Server.MapPath("/EmployeePaymentReport.rdlc");


                ReportDataSource reportDataSource = new ReportDataSource();
                reportDataSource.Name = "AccountsDataSet";


                dtBioDvAC = _employeeReportService.GetEmployeePayablePayment(identity.CompanyId, voucherId);
                // dtBioDvAC = dsBioDvAC.Tables[0];

                reportDataSource.Value = dtBioDvAC;






                string CompanyName = string.Empty;
                string CompanyAddress = string.Empty;
                string TotalAmmountInWord = string.Empty;
                double TotalAmmount = 0;
                if (dtCompanny.Rows.Count > 0)
                {

                    CompanyName = dtCompanny.Rows[0]["UserName"].ToString();
                    CompanyAddress = dtCompanny.Rows[0]["Address1"].ToString();


                }
                //if (dtBioDvAC.Rows.Count > 0)
                //{
                //    for (int i = 0; i < dtBioDvAC.Rows.Count; i++)
                //    {
                //        if (Convert.ToDouble(dtBioDvAC.Rows[i]["DrAmount"].ToString())>0)
                //        {
                //            TotalAmmount = TotalAmmount + Convert.ToDouble(dtBioDvAC.Rows[0]["DrAmount"].ToString());
                //        }

                //    }

                //}
                TotalAmmount = Convert.ToDouble(dtBioDvAC.Compute("SUM(DrAmount)", string.Empty));
                TotalAmmountInWord = oReportUtility.InWord(TotalAmmount, dtBioDvAC.Rows[0]["CurrencyId"].ToString());
                //CompanyName = identity.CompanyName;
                //CompanyAddress = oReportUtility.CompanyHeader();

                ReportParameter[] parameter = new ReportParameter[]
                {
                    new ReportParameter("CompanyName", CompanyName),
                    new ReportParameter("CompanyAddress", CompanyAddress),
                    new ReportParameter("TotalAmmountInWord", TotalAmmountInWord),
                    new ReportParameter("UserID", identity.Name)

                };
                localReport.SetParameters(parameter);
                //reportDataSource.Value = db.OnlineApplications.Where(x => x.StudentCode == "2019-Three-001").FirstOrDefault();

                localReport.DataSources.Add(reportDataSource);

                string ReportType = "pdf";
                string reportType = ReportType;
                String mimeType = string.Empty;
                String encoding = string.Empty;
                String extension = ReportType == "Excel" ? "xlsx" : "pdf";
                //String extension =  "png";
                Warning[] warnings = null;
                string[] streamids = null;
                Byte[] bytes = null;

                bytes = localReport.Render(reportType, "", out mimeType, out encoding, out extension, out streamids, out warnings);
                string PDFPath = new DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath("~/")) + "PDF\\";
                fileName = "EmployeePaymentReport" + DateTime.Now.ToString("dd-MMM-yyyy") + identity.UserId + ".pdf";
                //string fileName = "iDCard" + DateTime.Now.ToFileTime() + ".png";
                bool IsExitsPDF = System.IO.File.Exists(PDFPath + fileName);

                //FileStream fs = new FileStream(PDFPath + fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                //fs = new FileStream(PDFPath + fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                byte[] data = new byte[fs.Length];
                fs.Write(bytes, 0, bytes.Length);
                //fs.Close();



                PdfLocation = "/PDF/" + fileName;
                //report.Attributes["src"] = PdfLocation;
                ViewBag.ReportPath = PdfLocation;
                string path = Server.MapPath("/PDF/");

                fileName = "EmployeePaymentReport" + DateTime.Now.AddDays(-1).ToString("dd-MMM-yyyy") + identity.UserId + ".pdf"; ;
                if (System.IO.File.Exists(path + fileName))
                {
                    try
                    {
                        System.IO.File.Delete(path + fileName);
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }


            }
            catch (Exception ex)
            {
                throw ex;

            }
            //MemoryStream ms = new MemoryStream();

            //byte[] byteInfo = fs;
            //ms.Write(byteInfo, 0, byteInfo.Length);
            //ms.Position = 0;


            HttpContext.Response.AddHeader("content-disposition",
                "attachment; filename=form.pdf");

            return File(fileName, "application/pdf");

            //  var workbook = _employeeReportService.GetEmployeePayment(out string reportFileName, identity.CompanyId, identity.PlantName, voucherId);
            //return File(PdfLocation, "application/pdf");
            //return View("~/Areas/Accounts/Views/EmployeePaymentReport.cshtml");

        }
    }
}