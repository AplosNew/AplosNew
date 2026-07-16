#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using Library.Service.EmployeeServices;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using Library.HumanResource.NewAttendanceProcess;
using System.IO;
using Library.Data;
using Library.Service.Extension;
#endregion Using

namespace Aplos.Areas.Leave.Controllers
{
    public class AnnualLeaveProcessController : BaseController
    {

        #region Constructor

        LeaveOpeningUploadService _leave = new LeaveOpeningUploadService();
        AnnualLeaveProcessingService alp = new AnnualLeaveProcessingService();
        RegularEncashmentService reg = new RegularEncashmentService();
        private readonly ISqlRepository _sqlRepository;

        public AnnualLeaveProcessController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Constructor

        #region Other Functions

        [HttpGet, Authorize]
        public ActionResult getCurrentList(string PlantId, string YearId)
        {
            try
            {
                return Json(_leave.getCurrentList(PlantId, YearId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult getCompany()
        {
            try
            {
                return Json(_leave.getCompany(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpGet, Authorize]
        public ActionResult getPlants(string cmp)
        {
            try
            {
                return Json(_leave.getPlants(cmp), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult getLeaveYear(string PlantId)
        {
            try
            {
                return Json(_leave.getLeaveYear(PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult SaveFileList(List<Dictionary<string, object>> data, string PlantId, string YearId)
        {
            try
            {
                _leave.SaveFileList(data, PlantId, YearId);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        #endregion

        #region Excel Download

        [HttpGet, Authorize]
        public ActionResult GetSampleReport(string PlantId, string name, string LvYearId, ReportFormat reportFormat)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string date = DateTime.Now.Date.ToString("dd-MMM");
            var reportFileName = "LeaveOpeningUpload-" + name + "-" + date;
            var workbook = GetWorkSheet(PlantId, LvYearId);
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

        private IWorkbook GetWorkSheet(string PlantId, string LvId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            DataTable data = _leave.getSampleFile(PlantId, LvId);

            sheet.Name = "LeaveOpeningUpload";

            int ROW = 1;
            int endCol = 1;
            int COL = 1;

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "EmployeeId", 15, ExcelHAlign.HAlignLeft);
            int ColEmpId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "LeaveYear", 16, ExcelHAlign.HAlignLeft);
            int ColLvYear = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "LeaveYearId", 14, ExcelHAlign.HAlignLeft);
            int ColLvYearId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "LeaveType", 16, ExcelHAlign.HAlignLeft);
            int ColLvType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "LeaveTypeId", 14, ExcelHAlign.HAlignLeft);
            int ColLvTypeId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plant", 14, ExcelHAlign.HAlignLeft);
            int ColPlant = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Earned", 10, ExcelHAlign.HAlignLeft);
            int ColEarned = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Availed", 10, ExcelHAlign.HAlignLeft);
            int ColAvailed = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "RegularEncashment", 16, ExcelHAlign.HAlignLeft);
            int ColRegEncashment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Adjustment", 12, ExcelHAlign.HAlignLeft);
            int ColAdjustment = COL;
            endCol = COL;
            #endregion Headers

            ROW++;
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;
            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColEmpId].Text = data.Rows[i]["EmpId"].ToString();
                sheet[ROW, ColLvYear].Text = data.Rows[i]["LeaveYear"].ToString();
                sheet[ROW, ColLvType].Text = data.Rows[i]["LeaveType"].ToString();
                sheet[ROW, ColLvTypeId].Text = data.Rows[i]["LeaveTypeId"].ToString();
                sheet[ROW, ColPlant].Text = data.Rows[i]["Plant"].ToString();
                sheet[ROW, ColRegEncashment].Number = OTSBD.clsStaticInfo.dbl(data.Rows[i]["RegularEncashment"].ToString());
                sheet[ROW, ColEarned].Number = OTSBD.clsStaticInfo.dbl(data.Rows[i]["Earned"].ToString());
                sheet[ROW, ColAvailed].Number = OTSBD.clsStaticInfo.dbl(data.Rows[i]["Availed"].ToString());
                sheet[ROW, ColAdjustment].Number = OTSBD.clsStaticInfo.dbl(data.Rows[i]["Adjustment"].ToString());
                sheet[ROW, ColLvYearId].Text = data.Rows[i]["LeaveYearId"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }
            endRow = ROW - 1;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        #endregion

        #region Import Data

        [HttpPost, Authorize]
        public ActionResult ImportData()
        {
            string path;

            try
            {
                var file = Request.Files["file"];
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveFile(out path);
                var data = ReadData(path);

                var json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public List<LeaveOpeningData> ReadData(string path)
        {

            DataSet dsExcel = null;
            try
            {
                List<LeaveOpeningData> data = new List<LeaveOpeningData>();
                List<LeaveOpeningData> ret = new List<LeaveOpeningData>();
                ReadFile(path, out dsExcel);

                data = dsExcel.Tables[0].ToList<LeaveOpeningData>();

                if (data.Count > 0)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        string Earning = clsWebLib.RetValidLen(data[i].Earned).ToString();
                        string Availed = clsWebLib.RetValidLen(data[i].Availed).ToString();
                        string Adjustment = clsWebLib.RetValidLen(data[i].Adjustment).ToString();
                        string EmpId = clsWebLib.RetValidLen(data[i].EmployeeId).ToString();
                        string LvtypeId = clsWebLib.RetValidLen(data[i].LeaveTypeId).ToString();


                        if (Earning != "" && Adjustment != ""
                            && Availed != "" && EmpId != "")
                        {
                            ret.Add(data[i]);
                        }
                        else
                        {
                            throw new Exception("Plz Enter Valid Data for" + data[i].EmployeeId);
                        }
                    }
                }

                return ret;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveFile(out string path)
        {
            path = "";
            try
            {
                var file = Request.Files["file"];
                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {
                    }
                    else
                        throw new CustomException(Resources.ExcelUploadError);
                }
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetOTManualFile(), file.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                        file.SaveAs(path);
                    }
                    else
                    {
                        file.SaveAs(path);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ReadFile(string path, out DataSet dsExcel)
        {
            FileInfo docFile;
            dsExcel = null;
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = excelEngine.Excel.Workbooks.Open(path);
                DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);

                dsExcel = new DataSet();
                dsExcel.Tables.Add(dt);
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    docFile.Delete();
                }
            }
            catch (Exception ex)
            {
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    docFile.Delete();
                }
                throw (ex);
            }
        }

        #endregion

        #region Leave Processing Functions

        [HttpGet, Authorize]
        public ActionResult getNewLeaveYear(string PlantId, string LvYearId)
        {
            try
            {
                return Json(alp.GetNewLvYear(PlantId, LvYearId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetLeaveType()
        {
            try
            {
                return Json(alp.GetLeaveType(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetEmpCategory()
        {
            try
            {
                return Json(alp.GetEmpCategory(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult LoadData(string PlantId, string LvYearId, List<string> LvTypeId, List<string> EmpCategory)
        {
            try
            {
                return Json(alp.LoadData(PlantId, LvYearId, LvTypeId, EmpCategory), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public ActionResult ProcessData(string Data, string PlantId, string CurrentLvYearId, decimal MaxCarryForward,
            decimal MaxEncash, decimal MaxLapse, string NewYear, List<string> LeaveTypeList)
        {
            try
            {
                alp.ProcessData(Data, PlantId, CurrentLvYearId, MaxCarryForward, MaxEncash, MaxLapse, NewYear, LeaveTypeList);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.ToString() }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { Error = false, Message = "Annual Leave Process Ran Successfully..." }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost, Authorize]
        public ActionResult ProcessELData()
        {
            try
            {
                alp.SaveELData();
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.ToString() }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { Error = false, Message = "EL Leave Process Ran Successfully..." }, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region Regular Encashment Functions

        [HttpGet, Authorize]
        public ActionResult GetEmpInfo(string PlantId, string From, string To, string Year)
        {
            try
            {
                return Json(reg.GetEmpInfo(PlantId, From, To, Year), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public ActionResult ProcessRegData(string Data, string PlantId, string CurrentLvYearId,
          decimal MaxEncash, List<string> LeaveTypeList)
        {
            try
            {
                reg.ProcessRegData(Data, PlantId, CurrentLvYearId, MaxEncash, LeaveTypeList);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.ToString() }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { Error = false, Message = "Regular Encashment Process Ran Successfully..." }, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region LeaveDataReport

        [HttpPost, Authorize]
        public ActionResult LeaveDataReportXls(List<Dictionary<string, object>> data, string reportFileName,string plantId)
        {
            try
            {
                string fileName = "";
                fileName = LeaveDataReport(data, "", reportFileName, plantId);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string LeaveDataReport(List<Dictionary<string, object>> data, string ReportHeader, string reportFileName, string plantId)
        {
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
                workbook.Worksheets[0].Name = "LeaveDataReport";
                sheet = workbook.Worksheets[0];

                int ROW = 6; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "EmployeeId"; sheet[ROW, COL].ColumnWidth = 16; int colEmployeeId = COL; COL++; int colstart = COL;
                sheet[ROW, COL].Text = "LeaveType"; sheet[ROW, COL].ColumnWidth = 16; int colLeaveType = COL; COL++;
                sheet[ROW, COL].Text = "Opening"; sheet[ROW, COL].ColumnWidth = 16; int colOpening = COL; COL++;
                sheet[ROW, COL].Text = "Earned"; sheet[ROW, COL].ColumnWidth = 16; int colEarned = COL; COL++;
                sheet[ROW, COL].Text = "Availed"; sheet[ROW, COL].ColumnWidth = 16; int colAvailed = COL; COL++;
                sheet[ROW, COL].Text = "RegularEncashment"; sheet[ROW, COL].ColumnWidth = 16; int colRegularEncashment = COL; COL++;
                sheet[ROW, COL].Text = "Adjustment"; sheet[ROW, COL].ColumnWidth = 16; int colAdjustment = COL; COL++;
                sheet[ROW, COL].Text = "Closing"; sheet[ROW, COL].ColumnWidth = 16; int colClosing = COL;



                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;
                int LastRow = ROW + (data.Count - 1);

                for (int i = 0; i < data.Count; i++)
                {
                    sheet[ROW, colEmployeeId].Text = data[i]["EmpId"].ToString();
                    sheet[ROW, colLeaveType].Text = data[i]["LeaveType"].ToString();
                    sheet[ROW, colOpening].Number = OTSBD.clsStaticInfo.dbl(data[i]["Opening"].ToString());
                    sheet[ROW, colEarned].Number = OTSBD.clsStaticInfo.dbl(data[i]["Earned"].ToString());
                    sheet[ROW, colAvailed].Number = OTSBD.clsStaticInfo.dbl(data[i]["Availed"].ToString());
                    sheet[ROW, colRegularEncashment].Number = OTSBD.clsStaticInfo.dbl(data[i]["RegularEncashment"].ToString());
                    sheet[ROW, colAdjustment].Number = OTSBD.clsStaticInfo.dbl(data[i]["Adjustment"].ToString());
                    sheet[ROW, colClosing].Number = OTSBD.clsStaticInfo.dbl(data[i]["Closing"].ToString());

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, ROW, endCol];
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Leave Data Report", plantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetLeaveSummaryDataReportXls(string reportFileName, string plantId, string fromdate, string todate)
        {
            try
            {
                string fileName = "";
                fileName = GetLeaveSummaryDataReport(reportFileName, plantId, fromdate, todate);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string GetLeaveSummaryDataReport(string reportFileName, string plantId,string fromdate, string todate)
        {
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
                workbook.Worksheets[0].Name = "LeaveSummaryDataReport";
                sheet = workbook.Worksheets[0];
                var data = alp.GetEmpYearEarnAvailSummaryData(fromdate, todate, plantId);
                int ROW = 6; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "EmployeeCode"; sheet[ROW, COL].ColumnWidth = 16; int colEmployeeCode = COL; COL++; int colstart = COL;
                sheet[ROW, COL].Text = "EmpSystemID"; sheet[ROW, COL].ColumnWidth = 16; int colEmpSystemID = COL; COL++;
                sheet[ROW, COL].Text = "EmployeeName"; sheet[ROW, COL].ColumnWidth = 16; int colEmployeeName = COL; COL++;
                sheet[ROW, COL].Text = "DOJ"; sheet[ROW, COL].ColumnWidth = 16; int colDOJ  = COL; COL++;
                sheet[ROW, COL].Text = "DOS"; sheet[ROW, COL].ColumnWidth = 16; int colDOS = COL; COL++;
                sheet[ROW, COL].Text = "DayStatus Count"; sheet[ROW, COL].ColumnWidth = 16; int colDayStatus = COL; COL++;
                sheet[ROW, COL].Text = "LeaveType"; sheet[ROW, COL].ColumnWidth = 16; int colLeaveType = COL; COL++;
                sheet[ROW, COL].Text = "EarnValue"; sheet[ROW, COL].ColumnWidth = 16; int colEarnValue = COL; COL++;
                sheet[ROW, COL].Text = "AvailedValue"; sheet[ROW, COL].ColumnWidth = 16; int colAvailedValue = COL;



                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;
                int LastRow = ROW + (data.Count - 1);

                for (int i = 0; i < data.Count; i++)
                {
                    sheet[ROW, colEmployeeCode].Text = data[i]["EmployeeCode"].ToString();
                    sheet[ROW, colEmpSystemID].Text = data[i]["EmpSystemID"].ToString();
                    sheet[ROW, colEmployeeName].Text = data[i]["EmployeeName"].ToString();
                    sheet[ROW, colDOJ].Text = data[i]["DOJ"].ToString();
                    sheet[ROW, colDOS].Text = data[i]["DOS"].ToString();
                    sheet[ROW, colLeaveType].Text = data[i]["LeaveType"].ToString();
                    sheet[ROW, colDayStatus].Number = OTSBD.clsStaticInfo.dbl(data[i]["DayStatus"].ToString());
                    sheet[ROW, colEarnValue].Number = OTSBD.clsStaticInfo.dbl(data[i]["EarnValue"].ToString());
                    sheet[ROW, colAvailedValue].Number = OTSBD.clsStaticInfo.dbl(data[i]["AvailedValue"].ToString());

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, ROW, endCol];
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Leave Summary Data Report", plantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetEmpYearEarnAvailData(string fromdate, string todate, string empId)
        {
            var jsondata = Json(alp.GetEmpYearEarnAvailData(fromdate, todate, empId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        #endregion
    }

    public class LeaveOpeningData
    {
        public string Earned { get; set; }
        public string Availed { get; set; }
        public string Adjustment { get; set; }
        public string EmployeeId { get; set; }
        public string LeaveYearId { get; set; }
        public string LeaveTypeId { get; set; }
        public string LeaveType { get; set; }
        public string RegularEncashment { get; set; }
    }

}