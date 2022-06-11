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
        public ActionResult GetFuguaiReport(string FromDate, string ToDate, string FinalStatus)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Fuguai Report";
            var workbook = GetReportWorkSheet(FromDate, ToDate, FinalStatus);
            return RenderReportAsExcel(workbook, reportFileName);
        }


        private IWorkbook GetReportWorkSheet(string FromDate, string ToDate, string FinalStatus)
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


            DataTable data = GetData(FromDate, ToDate, FinalStatus);

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Date", 12, ExcelHAlign.HAlignLeft);
            int ColDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Time", 25, ExcelHAlign.HAlignLeft);
            int ColTime = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 15, ExcelHAlign.HAlignLeft);
            int ColEmpEntity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Observed By", 15, ExcelHAlign.HAlignLeft);
            int ColObservedBy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Category", 15, ExcelHAlign.HAlignLeft);
            int ColCategory = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Tag", 15, ExcelHAlign.HAlignLeft);
            int ColTag = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Detail", 15, ExcelHAlign.HAlignLeft);
            int ColDetail = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Priority Level", 15, ExcelHAlign.HAlignLeft);
            int ColPriorityLevel = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Responsible Department", 15, ExcelHAlign.HAlignRight);
            int ColResponsibleDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Responsible Person", 15, ExcelHAlign.HAlignRight);
            int ColResponsiblePerson = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Target Date", 15, ExcelHAlign.HAlignRight);
            int ColTargetDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 15, ExcelHAlign.HAlignLeft);
            int ColRemarks = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Current Status", 15, ExcelHAlign.HAlignRight);
            int ColCurrentStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Process", 15, ExcelHAlign.HAlignRight);
            int ColProcess = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Machine", 15, ExcelHAlign.HAlignRight);
            int ColMachine = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Machine Ref No", 15, ExcelHAlign.HAlignLeft);
            int ColMachineNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Final Status", 15, ExcelHAlign.HAlignLeft);
            int ColFinalStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "CloseDate", 15, ExcelHAlign.HAlignLeft);
            int ColCloseDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Tag Color", 15, ExcelHAlign.HAlignLeft);
            int ColTagColor = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Story Point", 15, ExcelHAlign.HAlignLeft);
            int ColStoryPoint = COL;
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
                sheet[ROW, ColDate].Text = data.Rows[i]["Date"].ToString();
                sheet[ROW, ColTime].Text = data.Rows[i]["Time"].ToString();
                sheet[ROW, ColEmpEntity].Text = data.Rows[i]["Entity"].ToString();
                sheet[ROW, ColObservedBy].Text = data.Rows[i]["ObservedBy"].ToString();
                sheet[ROW, ColCategory].Text = data.Rows[i]["Category"].ToString();
                sheet[ROW, ColTag].Text = data.Rows[i]["Tag"].ToString();
                sheet[ROW, ColDetail].Text = data.Rows[i]["Detail"].ToString();
                sheet[ROW, ColPriorityLevel].Text = data.Rows[i]["PriorityLevel"].ToString();
                sheet[ROW, ColResponsibleDepartment].Text = data.Rows[i]["Department"].ToString();
                sheet[ROW, ColResponsiblePerson].Text = data.Rows[i]["ResponsiblePerson"].ToString();
                sheet[ROW, ColTargetDate].Number = Convert.ToDouble(data.Rows[i]["TargetDate"].ToString());
                sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
                sheet[ROW, ColCurrentStatus].Text = data.Rows[i]["CurrentStatus"].ToString();
                sheet[ROW, ColProcess].Text = data.Rows[i]["Process"].ToString(); ;
                sheet[ROW, ColMachine].Text = data.Rows[i]["Machine"].ToString(); ;
                sheet[ROW, ColMachineNo].Text = data.Rows[i]["MachineNo"].ToString();
                sheet[ROW, ColFinalStatus].Text = data.Rows[i]["FinalStatus"].ToString();
                sheet[ROW, ColCloseDate].Text = data.Rows[i]["CloseDate"].ToString();
                sheet[ROW, ColTagColor].Text = data.Rows[i]["TagColor"].ToString();
                sheet[ROW, ColStoryPoint].Text = data.Rows[i]["StoryPoint"].ToString();
                
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
            sheet.Range[ROW, 3].Text = "Fuguai Report: " + FromDate + " To " + ToDate;
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

        private DataTable GetData(string FromDate, string ToDate, string FinalStatus)
        {
            try
            {
               
                string sql = @"select ft.Id, cast(ft.Date as Date) as Date, et.UserName as Entity, e.EmployeeName as ResponsiblePerson, s.FullName as ObservedBy, ft.ZoneCategory as Category,
                        z.UserName as Tag, ft.Detail, ft.PriorityLevel, z.SubCategory, d.UserName as Department, p.UserName as Process,
                        cast(ft.TargetDate as Date) as TargetDate, ft.StoryPoint, ft.Remarks, ft.CurrentStatus,
                        mm.UserName as Machine, mm.ProductionMachineQty as MachineReference, ft.FinalStatus, ft.TagColor
                        from TRN.FuguaiTransaction ft
                        left join dbo.EmployeeInformation e on e.SystemId = ft.ResponsiblePersonId
                        left join SEC.[user] s on s.Id = ft.ObservedById
                        left join hkp.ZoneMaster z on z.Id = ft.ZoneMasterId
                        left join org.Entity et on et.Id = ft.EntityId
                        left join org.Department d on d.Id = ft.ResponsibleDepartmentId
                        left join MST.MachineMaster mm on mm.Id = ft.MachineMasterId
                        left join hkp.Process p on p.Id = ft.ProcessId
                        where e.SystemId = '" + FromDate + "' and s.Id = '" + ToDate + "' and ft.FinalStatus = '"+ FinalStatus + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}