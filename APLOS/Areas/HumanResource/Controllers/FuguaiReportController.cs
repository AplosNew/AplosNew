#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Library.HumanResource.Employee;
using Syncfusion.XlsIO;
using System.Drawing;
using System.IO;
using System.Linq;
using Library.Service.Helpers;
#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class FuguaiReportController : Controller
    {
        FuguaiReportService fr = new FuguaiReportService();
        private readonly ISqlRepository _sqlRepository;
        public FuguaiReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #region Page
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion Page
        #region All Get
        [Authorize, HttpPost]
        public ActionResult getByWhom()
        {
            try
            {
                return Json(fr.getByWhom(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getResponsiblePerson()
        {
            try
            {
                return Json(fr.getResponsiblePerson(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getCategory()
        {
            try
            {
                return Json(fr.getCategory(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getFuguai(string categoryText)
        {
            try
            {
                return Json(fr.getFuguai(categoryText), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getFinalStatus(string categoryText)
        {
            try
            {
                return Json(fr.getFinalStatus(categoryText), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getFuguaiTransaction(string SystemId, string ObservedById)
        {
            try
            {
                return Json(fr.getFuguaiTransaction(SystemId, ObservedById), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion All Get

        public ActionResult RenderReportAsExcel(IWorkbook workbook, string fileName)
        {
            workbook.SaveAs(fileName + ".xls", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }
        
        public ActionResult RenderReportAsExcelx(IWorkbook workbook, string fileName)
        {
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }
        
        [HttpGet, Authorize]
        public ActionResult GetEmployeeServiceFixedReport(string FromDate, string ToDate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Fuguai Report";
            var workbook = GetReportWorkSheet(FromDate, ToDate);
            return RenderReportAsExcel(workbook, reportFileName);
        }


        private IWorkbook GetReportWorkSheet(string FromDate, string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 2);
            workbook.Version = ExcelVersion.Excel2016;
            string FactoryName = string.Empty;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            string CmpName = "";
            string FactoryAddress = string.Empty;
            
            var sheet = workbook.Worksheets[0];

            sheet.Name = "Fuguai Report";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = GetData(FromDate, ToDate);

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 12, ExcelHAlign.HAlignLeft);
            int ColEmpId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 25, ExcelHAlign.HAlignLeft);
            int ColEmpName = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 15, ExcelHAlign.HAlignLeft);
            int ColEmpEntity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Depertment", 15, ExcelHAlign.HAlignLeft);
            int ColEmpDepertment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 15, ExcelHAlign.HAlignLeft);
            int ColDesignation = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Service Name", 15, ExcelHAlign.HAlignLeft);
            int ColServiceName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Service Category", 15, ExcelHAlign.HAlignLeft);
            int ColServiceCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "UOM", 15, ExcelHAlign.HAlignLeft);
            int ColUOM = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "From", 15, ExcelHAlign.HAlignRight);
            int ColFrom = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "To", 15, ExcelHAlign.HAlignRight);
            int ColTo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Quantity", 15, ExcelHAlign.HAlignRight);
            int ColQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Currency", 15, ExcelHAlign.HAlignLeft);
            int ColCurrency = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rate", 15, ExcelHAlign.HAlignRight);
            int ColRate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Amount", 15, ExcelHAlign.HAlignRight);
            int ColAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Final Amount", 15, ExcelHAlign.HAlignRight);
            int ColFinalAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Chargable", 15, ExcelHAlign.HAlignLeft);
            int ColChargable = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "NonChargable", 15, ExcelHAlign.HAlignLeft);
            int ColNonChargable = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 15, ExcelHAlign.HAlignLeft);
            int ColRemarks = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Added By", 15, ExcelHAlign.HAlignLeft);
            int ColAddedBy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Time", 15, ExcelHAlign.HAlignLeft);
            int ColAddedDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Date", 15, ExcelHAlign.HAlignLeft);
            int ColActualDate = COL;
            COL++;

            sheet.Range[ROW, 1, ROW, COL].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            sheet.Range[ROW, 1, ROW, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, COL].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true;

            endCol = COL;
            #endregion Headers

            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;
            ROW++;
            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColEmpId].Text = data.Rows[i]["EmpId"].ToString();
                sheet[ROW, ColEmpName].Text = data.Rows[i]["EmpName"].ToString();
                sheet[ROW, ColEmpEntity].Text = data.Rows[i]["EmpEntity"].ToString();
                sheet[ROW, ColEmpDepertment].Text = data.Rows[i]["EmpDepertment"].ToString();
                sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                sheet[ROW, ColServiceName].Text = data.Rows[i]["ServiceName"].ToString();
                sheet[ROW, ColUOM].Text = data.Rows[i]["UOM"].ToString();
                sheet[ROW, ColFinalAmount].Number = Convert.ToDouble(data.Rows[i]["FinalAmount"].ToString());
                sheet[ROW, ColCurrency].Text = data.Rows[i]["Currency"].ToString();
                sheet[ROW, ColServiceCategory].Text = data.Rows[i]["ServiceCategory"].ToString();
                sheet[ROW, ColFrom].Number = Convert.ToDouble(data.Rows[i]["From"].ToString());
                sheet[ROW, ColTo].Number = Convert.ToDouble(data.Rows[i]["To"].ToString());
                sheet[ROW, ColQty].Number = Convert.ToDouble(data.Rows[i]["Qty"].ToString());
                sheet[ROW, ColRate].Number = clsStaticInfo.dbl(data.Rows[i]["Rate"].ToString());
                sheet[ROW, ColAmount].Number = Convert.ToDouble(data.Rows[i]["Amount"].ToString());
                sheet[ROW, ColChargable].Text = data.Rows[i]["Chargable"].ToString();
                sheet[ROW, ColNonChargable].Text = data.Rows[i]["NonChargable"].ToString();
                sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
                sheet[ROW, ColAddedBy].Text = data.Rows[i]["AddedBy"].ToString();
                sheet[ROW, ColAddedDate].Text = data.Rows[i]["Time"].ToString();
                sheet[ROW, ColActualDate].Text = data.Rows[i]["Date"].ToString();
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                ROW++;
            }

            sheet.Range[6, 1, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[6, 1, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[6, 1, ROW - 1, endCol].WrapText = true;

           

            #region Report Header
            try
            {
                string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), identity.CompanyId + ".jpg");  // IDCardEng.xlsx
                Image companyLogo = Image.FromFile(strPath);
                if (companyLogo != null)
                {
                    double totalWidth = sheet.GetColumnWidth(1) + sheet.GetColumnWidth(2);
                    int totalWidthPixel = (int)(totalWidth * 7.25);
                    int totalheight = (int)((sheet.GetRowHeight(1) + sheet.GetRowHeight(2) + sheet.GetRowHeight(3) + sheet.GetRowHeight(3)) * 1.50);

                    companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                    IPictureShape pic = null;

                    pic = sheet.Pictures.AddPicture(1, 1, companyLogo);

                }


            }
            catch (Exception)
            {


            }

            ROW = 1;
            COL = 1;

            if (dsCmp.Tables[0].Rows.Count > 0)
            {
                CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
            }
            else
            {
                CmpName = "";
            }
            sheet.Range[ROW, 3].Text = CmpName;
            sheet.Range[ROW, 3, COL, endCol].Merge();
            sheet.Range[ROW, 3].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 3].CellStyle.Font.Size = 12;
            sheet.Range[ROW, 3, COL, endCol].RowHeight = 20;
            sheet.Range[ROW, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[ROW, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, 3, COL, endCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

            ROW += 1;
            if (dsFactory.Tables[0].Rows.Count > 0)
            {

                FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
            }
            else
            {
                FactoryName = "";
            }
            sheet.Range[ROW, 3].Text = FactoryName;
            sheet.Range[ROW, 3, ROW, endCol].Merge();
            sheet.Range[ROW, 3].CellStyle.Font.Size = 10;
            sheet.Range[ROW, 3, ROW, endCol].RowHeight = 20;
            sheet.Range[ROW, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[ROW, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, 3, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

            ROW += 1;
            if (dsFactory.Tables[0].Rows.Count > 0)
            {
                FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
            }
            else
            {
                FactoryAddress = "";
            }
            sheet.Range[ROW, 3].Text = FactoryAddress;
            sheet.Range[ROW, 3, ROW, endCol].Merge();
            sheet.Range[ROW, 3].CellStyle.Font.Size = 22;
            sheet.Range[ROW, 3, ROW, endCol].RowHeight = 17;
            sheet.Range[ROW, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[ROW, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, 3, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

            ROW += 1;
            sheet.Range[ROW, 3].Text = "Employee Service Variable: " + FromDate + " To " + ToDate;
            sheet.Range[ROW, 3, ROW, endCol].Merge();
            sheet.Range[ROW, 3].CellStyle.Font.Size = 10;
            sheet.Range[ROW, 3, ROW, endCol].RowHeight = 20;
            sheet.Range[ROW, 3].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[ROW, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, 3, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

            #endregion Report Header

            #region UsedRange Alignment

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            sheet.Range["A1"].CellStyle.Font.Size = 14;
            sheet.Range["A2"].CellStyle.Font.Size = 10;
            sheet.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

            #endregion UsedRange Alignment

            #region Page Setup
            sheet.PageSetup.TopMargin = 0.5;
            sheet.PageSetup.BottomMargin = 0.7;
            sheet.PageSetup.PrintTitleRows = "$1:$5";
            sheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
            sheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
            sheet.PageSetup.LeftMargin = 0.5;
            sheet.PageSetup.RightMargin = 0.2;
            sheet.PageSetup.Orientation = ExcelPageOrientation.Portrait;
            sheet.PageSetup.FitToPagesTall = 0;
            sheet.PageSetup.FitToPagesWide = 1;
            sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
            sheet.IsDisplayZeros = false;
            #endregion Page Setup


            #region Freeze Panes

            sheet.IsDisplayZeros = false;
            sheet.UsedRange["A7"].FreezePanes();
            sheet.FirstVisibleColumn = 1;
            sheet.FirstVisibleRow = 6;

            #endregion Freeze Panes

            return workbook;
        }

        private DataTable GetData(string FromDate, string ToDate)
        {
            try
            {
                
                string sql = @"";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}