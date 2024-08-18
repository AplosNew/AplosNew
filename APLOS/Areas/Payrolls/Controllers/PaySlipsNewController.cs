using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Payroll;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Microsoft.Reporting.WebForms;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class PaySlipsNewController : BaseController
    {
        #region Constructor

        private readonly PayrollReportsService _payrollReportsService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;
        Library.HumanResource.Report.Payroll.clsPayRegister _payRegisterBDReportService = new Library.HumanResource.Report.Payroll.clsPayRegister();



        public PaySlipsNewController(IEmployeeProfileService employeeProfileService, ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
            _payrollReportsService = new PayrollReportsService();
            _employeeProfileService = employeeProfileService;
            _payRegisterBDReportService = new Library.HumanResource.Report.Payroll.clsPayRegister();

        }

        #endregion Constructor

        #region -- Pages

        public ActionResult PaySlipsNew()
        {
            return View();
        }
        public ActionResult SalaryAdvice()
        {
            return View();
        }
        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetDisbursementAdviceCbo(string yearNo, string monthNo, string paymentMode,string ReportType)
        {
            try
            {
                return Json(_payrollReportsService.GetDisbursementAdviceCbo(yearNo, monthNo, paymentMode, ReportType), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetPaymentModeCbo()
        {
            try
            {
                return Json(_payrollReportsService.GetPaymentModeCbo(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetMonthNoCbo()
        {
            try
            {
                return Json(_payrollReportsService.GetMonthNoCbo(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetYearNoCbo()
        {
            try
            {
                return Json(_payrollReportsService.GetYearNoCbo(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetEmployeePaySlip(string month, string year, string salaryProcessId, Dictionary<string, string> parameters, string languageId, bool isActive, bool isSeperated, bool isMaternity, bool IsIncludingZeroHeads, bool singleEmployee, string reportFormat)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = "PaySlip" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                //GetEmployeePaySlipWithBal
                var workbook = _payrollReportsService.GetEmployeePaySlipNew(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, parameters, languageId, isActive, isSeperated, isMaternity, IsIncludingZeroHeads, singleEmployee);

                workbook.Version = ExcelVersion.Excel2016;
                //workbook.SaveAs(fullPath);
                if (reportFormat == "Pdf")
                {
                    var converter = new ExcelToPdfConverter(workbook);
                    ExcelToPdfConverterSettings _settings = new ExcelToPdfConverterSettings();
                    _settings.AutoDetectComplexScript = true;
                    var pdfDoc = converter.Convert(_settings);

                    fileName = month + "-" + year + "PaySlip" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".pdf";
                    string fullPathPDF = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
                    pdfDoc.Save(fullPathPDF);
                }
                else
                {
                    if (identity.IsSysAdmin == true)
                    {
                        workbook.SaveAs(fullPath);
                        workbook.Close();
                    }
                    else
                    {
                        throw new Exception("Contact to Admin.");
                    }
                }
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetEmployeePaySlipContractor(string month, string year, string salaryProcessId, Dictionary<string, string> parameters, string languageId, string contractorId, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = "PaySlipCotractor" + month + year + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                var workbook = _payRegisterBDReportService.GetEmployeePaySlipContractor(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, parameters, languageId, contractorId, isActive, isSeperated, isMaternity);

                workbook.Version = ExcelVersion.Excel2016;
                //workbook.SaveAs(fullPath);
                var converter = new ExcelToPdfConverter(workbook);
                var pdfDoc = converter.Convert();
                fileName = month + "-" + year + "PaySlip" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".pdf";
                string fullPathPDF = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
                pdfDoc.Save(fullPathPDF);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetEmpInfo(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_payrollReportsService.GetEmpInfoSalaryPorcessed(identity.CompanyGroupId, identity.PlantId, effectiveDate, salaryProcessId, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId, isActive, isSeperated, isMaternity), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPayRollGroupCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_payrollReportsService.GetPayRollGroupCbo(identity.IsSysAdmin, identity.IsControlAdmin, identity.PlantId, identity.UserId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetSalaryAdviseReportPdf(ReportFormat reportFormat, string empcat, string adviceId, string yearNo, string monthNo, string monthName,string status, string ReportType)
        {
            try
            {
                string fileName = "";

                IWorkbook workbook = GetSalaryAdviseWorkbook("Data", empcat, adviceId, yearNo, monthNo, monthName, status, ReportType);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "BankAdvice";
                // return RenderReportAsPdf(workbook, reportFileName);
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        PdfDocument document = new PdfDocument();
                        ExcelToPdfConverterSettings settings = new ExcelToPdfConverterSettings();
                        settings.TemplateDocument = document;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document = converter1.Convert(settings);
                        }
                        document.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);
                        return null;

                    case ReportFormat.PdfView:
                        PdfDocument document1 = new PdfDocument();
                        ExcelToPdfConverterSettings settings1 = new ExcelToPdfConverterSettings();
                        settings1.TemplateDocument = document1;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document1 = converter1.Convert(settings1);
                        }
                        document1.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Open);
                        //return RenderReportAsPdf(document1, reportFileName);
                        return RenderReportAsPdf(workbook, reportFileName);
                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IWorkbook GetSalaryAdviseWorkbook(string SheetName, string empcat, string adviceId, string yearNo, string monthNo,string monthName,string status, string ReportType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Data";
                sheet = workbook.Worksheets[0];
                DataTable dtOrder = null;
                string sql = "";
                if (ReportType== "Salary")
                {
                    sql = @"Select EI.EmployeeCode,EI.EmployeeName,NP.NetPay, EB.BankAccNo, B.UserName BankName,EB.IFSCCode,FORMAT(SL.AddedDate,'dd-MMM-yyyy') DisbursmentDate,SL.UpdatedBy 
from dbo.SalaryLock SL
LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=SL.EmpSystemId 
LEFT JOIN(
  SELECT cast(spc.DisbusmentAmount AS decimal(18,0))NetPay,spc.EmpInfoSystemID,sl.YearNo,sl.MonthNo FROM SalaryProcChild AS spc
LEFT JOIN SalaryProcMaster AS spm ON spm.SystemID = spc.SlrProcMstSystemID 
LEFT JOIN SalaryHead AS sh ON sh.SalaryHeadID = spc.SalaryHeadID
LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID 
WHERE  sh.SalaryHead='Net Pay' 
   AND isnull(SPC.SlrProcMstSystemID,'') IN(Select SystemId from SalaryProcMaster Where YearNo='" + yearNo + @"' AND MonthNo='" + monthNo + @"')
   AND sl.YearNo='" + yearNo + @"' AND sl.MonthNo='" + monthNo + @"'
)NP ON NP.EmpInfoSystemID=SL.EmpSystemId
LEFT JOIN EmployeeBankInfo EB ON EB.EmpSystemID = EI.SystemId
AND EB.RowID=(Select top(1) RowID from EmployeeBankInfo Where EmpSystemID=EB.EmpSystemID AND  IsApproved=1 Order BY DateAdded DESC)
 LEFT JOIN HKP.Bank B ON B.Id = EB.BankSystemID
 LEFT JOIN TRN.Voucher V oN V.Id=SL.DisbursementVoucherId
Where SL.DisbursementAdviceId='" + adviceId + "' AND sl.YearNo='" + yearNo + @"' AND sl.MonthNo='" + monthNo + @"' AND V.IsPark=" + status + ""; 
                }
                if (ReportType == "Bonus")
                {
                    sql = @"Select EI.EmployeeCode,EI.EmployeeName,NP.NetPay, EB.BankAccNo, B.UserName BankName,EB.IFSCCode 
from dbo.SalaryLock SL
LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId = SL.EmpSystemId
LEFT JOIN(
  SELECT cast(spc.DisbusmentAmount AS decimal(18,0))NetPay,spc.EmpInfoSystemID,sl.YearNo,sl.MonthNo FROM SalaryProcChild AS spc
LEFT JOIN SalaryProcMaster AS spm ON spm.SystemID = spc.SlrProcMstSystemID
LEFT JOIN SalaryHead AS sh ON sh.SalaryHeadID = spc.SalaryHeadID
LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId = spc.EmpInfoSystemID
WHERE sh.SalaryHead = 'Net Pay'
   AND isnull(SPC.SlrProcMstSystemID,'') IN(Select SystemId from SalaryProcMaster Where YearNo = 2024 AND MonthNo = 5)
   AND sl.YearNo = 2024 AND sl.MonthNo = 5
)NP ON NP.EmpInfoSystemID = SL.EmpSystemId
LEFT JOIN EmployeeBankInfo EB ON EB.EmpSystemID = EI.SystemId
AND EB.RowID = (Select top(1) RowID from EmployeeBankInfo Where EmpSystemID = EB.EmpSystemID AND IsApproved = 1 Order BY DateAdded DESC)
 LEFT JOIN HKP.Bank B ON B.Id = EB.BankSystemID
 LEFT JOIN TRN.Voucher V oN V.Id = SL.DisbursementVoucherId
Where SL.BonusDisbursementAdviceId = '" + adviceId + "' AND sl.YearNo='" + yearNo + @"' AND sl.MonthNo='" + monthNo + @"' AND V.IsPark=" + status + "";
                }
                else
                {
                    ///
                }


                dtOrder = _sqlRepository.GetDataTable(sql);


                if (dtOrder.Rows.Count == 0)
                {
                    throw new Exception("No Data Found.");
                }
                ReportUtility reportUtility = new ReportUtility();

                int ROW = 4; int COL = 1;
                sheet.Range[ROW, COL].Text = "Bank Advice";
                sheet.Range[ROW, 1, ROW, 7].Merge();
                ROW++;
                sheet.Range[ROW, COL].Text = "Status :";
                sheet.Range[ROW, 2].Text = empcat;//status=="1"?"Parked" :"Posted";
                sheet.Range[ROW, 3].Text = "Disbursment Id :"+ adviceId;
                sheet.Range[ROW, 5].Text = "Disbursment Date :"+ dtOrder.Rows[0]["DisbursmentDate"].ToString();

                ROW++;
                ROW++;
                #region ColumnsHeader

                sheet[ROW, COL].Text = "SNo"; sheet[ROW, COL].ColumnWidth = 10.50; int colSL = COL; COL++;
                sheet[ROW, COL].Text = "Employee Code"; sheet[ROW, COL].ColumnWidth = 16.50; int colEC = COL; COL++;
                sheet[ROW, COL].Text = "Employee Name"; sheet[ROW, COL].ColumnWidth = 20.50; int colEN = COL; COL++;
                sheet[ROW, COL].Text = "Net Payable"; sheet[ROW, COL].ColumnWidth = 13.50; int colNP = COL; COL++;
                sheet[ROW, COL].Text = "Bank"; sheet[ROW, COL].ColumnWidth = 28; int colBN = COL; COL++;
                sheet[ROW, COL].Text = "A/C No"; sheet[ROW, COL].ColumnWidth = 13.50; int colAC = COL; COL++;
                sheet[ROW, COL].Text = "IFSC"; sheet[ROW, COL].ColumnWidth = 12; int colIFSC = COL; COL++;
                sheet[ROW, COL].Text = "Emp Signature"; sheet[ROW, COL].ColumnWidth = 36;


                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                #endregion columns

                ROW++;
                int startRow = ROW;
                int cnt = 0;
                #region DataPlot
                double NetPayable = 0;
                for (int i = 0; i < dtOrder.Rows.Count; i++)
                {

                    cnt++;
                    sheet[ROW, colSL].Number = Library.Service.Extension.clsStaticInfo.dbl(cnt.ToString());
                    sheet[ROW, colEC].Text = dtOrder.Rows[i]["EmployeeCode"].ToString();
                    sheet[ROW, colEN].Text = dtOrder.Rows[i]["EmployeeName"].ToString();
                    sheet[ROW, colNP].Text = dtOrder.Rows[i]["NetPay"].ToString();
                    sheet[ROW, colBN].Text = dtOrder.Rows[i]["BankName"].ToString();
                    sheet[ROW, colAC].Text = dtOrder.Rows[i]["BankAccNo"].ToString();
                    sheet[ROW, colIFSC].Text = dtOrder.Rows[i]["IFSCCode"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }
                #endregion
                int edCRow = ROW;

                edCRow++;
                edCRow++;
                edCRow++;


                sheet.Range[edCRow - 1, 2].Text = dtOrder.Rows[0]["UpdatedBy"].ToString();
                sheet.Range[edCRow, 2].Text = "Prepared By";

                sheet.Range[edCRow - 1, 4].Text = "";
                sheet.Range[edCRow, 4].Text = "Checked By";

                sheet.Range[edCRow - 1, 6].Text = "";
                sheet.Range[edCRow, 6].Text = "Approved By";

                edCRow++;

                #region ReportHeader
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.UsedRange.HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();


                reportUtility.CompanyHeader(ref sheet, 3, ""+ ReportType+ " Disbursed Report - "+monthName+" "+yearNo+"", identity.CompanyId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);

                sheet.PageSetup.CenterHorizontally = true;
                #endregion


                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion -- Operations
    }
}