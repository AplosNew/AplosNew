using Aplos.Helpers;
using Library.Crosscutting.Security;
using OTSBD;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Presentation;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Script.Serialization;


namespace Aplos.Controllers
{
    public class GridReportsController : Controller
    {
        // GET: GridReports
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost, Authorize]
        public JsonResult ExcelExportUpd(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                if (data == null)
                    throw new Exception("No data found");

                if (data.Count == 0)
                    throw new Exception("No data found");


                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                        continue;

                    dt.Columns.Add(item);
                }


                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                            continue;

                        dr[item] = data[i][item];
                    }

                    dt.Rows.Add(dr);
                }


                string filename = GridToExcelReportUpd(dt, "", reportFileName);


                return Json(new { FileName = filename, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

            //return View();
        }

        [HttpPost, Authorize]
        public JsonResult ViewExcelExportUpd(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                if (data == null)
                    throw new Exception("No data found");

                if (data.Count == 0)
                    throw new Exception("No data found");


                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                        continue;

                    dt.Columns.Add(item);
                }


                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                            continue;

                        dr[item] = data[i][item];
                    }

                    dt.Rows.Add(dr);
                }


                string filename = GridViewToExcelReportUpd(dt, "", reportFileName);


                return Json(new { FileName = filename, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

            //return View();
        }


        [HttpPost, Authorize]
        public JsonResult ExcelExportUpdate2(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                if (data == null)
                    throw new Exception("No data found");

                if (data.Count == 0)
                    throw new Exception("No data found");


                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                        continue;

                    dt.Columns.Add(item);
                }


                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                            continue;

                        dr[item] = data[i][item];
                    }

                    dt.Rows.Add(dr);
                }


                string filename = GridToExcelReportUpd(dt, "", reportFileName);


                return Json(new { FileName = filename, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

            //return View();
        }

        [HttpPost, Authorize]
        public JsonResult ExcelExport(List<Dictionary<string, object>> data)
        {
            try
            {
                if (data == null)
                    throw new Exception("No data found");

                if (data.Count == 0)
                    throw new Exception("No data found");


                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    if (item.ToUpper().Contains("ID") || item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                        continue;

                    dt.Columns.Add(item);
                }


                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        if (item.ToUpper().Contains("ID") || item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                            continue;

                        dr[item] = data[i][item];
                    }

                    dt.Rows.Add(dr);
                }


                string filename = GridToExcelReport(dt, "");


                return Json(new { FileName = filename, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

            //return View();
        }


        [HttpPost, Authorize]
        public JsonResult ExcelExportJson(object obj, string ReportHeader, string reportFileName)
        {
            //Json
            try
            {
                DataTable dt = new DataTable("APIDATA");
                var json = new JavaScriptSerializer().Serialize(obj);

                if (json != "[]")
                {
                    json = json.Replace("\\", "");

                    dt = CustomJsonResult.ToDataTable(json);
                }

                StringCollection strCol = new StringCollection();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (dt.Columns[i].ColumnName.ToUpper().Contains("ID") || dt.Columns[i].ColumnName.ToUpper().Contains("PK") || dt.Columns[i].ColumnName.ToUpper().Contains("EJVALUE"))
                    {
                        strCol.Add(dt.Columns[i].ColumnName);
                    }
                }
                foreach (string item in strCol)
                {
                    dt.Columns.Remove(item);
                }

                string filename = GridToExcelReportUpd(dt, ReportHeader, reportFileName);


                return Json(new { FileName = filename, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

            //return View();
        }
        private string GridToExcelReportUpd(DataTable data, string ReportHeader, string reportFileName)
        {
            string fileName = reportFileName + ".xlsx";
            string FactoryName = "";
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            string CmpName = "";
            string FactoryAddress = string.Empty;
            clsReport objRpt = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                objRpt = new clsReport();
                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);
                //save the file to server temp folder
                string fullPath = Path.Combine(HostingEnvironment.MapPath("~/") + fileName);

                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    IApplication application = excelEngine.Excel;
                    application.DefaultVersion = ExcelVersion.Excel2013;
                    IWorkbook workbook = application.Workbooks.Create(1);
                    IWorksheet sheet = workbook.Worksheets[0];

                    int ROW = 4;
                    sheet[ROW, 1].Text = ReportHeader;
                    sheet[ROW, 1, ROW, data.Columns.Count].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet[ROW, 1, ROW, data.Columns.Count].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet[ROW, 1].CellStyle.Font.Bold = true;
                    ROW++;

                    sheet.ImportDataTable(data, true, ROW, 1);
                    sheet[ROW, 1, ROW, data.Columns.Count].ColumnWidth = 20;
                    sheet[ROW, 1, ROW, data.Columns.Count].WrapText = true;
                    sheet[ROW, 1, ROW, data.Columns.Count].BorderAround(ExcelLineStyle.Hair);
                    sheet[ROW, 1, ROW, data.Columns.Count].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, data.Columns.Count].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                    sheet.Range[ROW, 1, ROW, data.Columns.Count].CellStyle.Font.Color = ExcelKnownColors.White;
                    sheet[ROW, 1, ROW, data.Columns.Count].CellStyle.Font.Bold = true;
                    sheet.AutoFilters.FilterRange = sheet.Range[ROW, 1, ROW, data.Columns.Count];
                    #region ******************Report Header******************
                    int endXlsCol = data.Columns.Count;
                    int xlsRow = 1, xlsCol = 1;
                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet.Range[xlsRow, xlsCol].Text = CmpName;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 17;
                    sheet.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet.Range[xlsRow, xlsCol].Text = FactoryName;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                    sheet.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet.Range[xlsRow, xlsCol].Text = FactoryAddress;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 22;
                    sheet.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet.Range[xlsRow, xlsCol].Text = reportFileName;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet.IsDisplayZeros = false;
                    sheet.UsedRange["A7"].FreezePanes();
                    sheet.FirstVisibleColumn = 1;
                    sheet.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment
                    sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.IsDisplayZeros = false;
                    sheet.UsedRange.WrapText = true;
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
                    sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    sheet.PageSetup.FitToPagesTall = 0;
                    sheet.PageSetup.FitToPagesWide = 1;
                    sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet.IsDisplayZeros = false;
                    sheet.Name = reportFileName;
                    #endregion Page Setup

                    workbook.SaveAs(fullPath);

                }
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {

            }
            return fileName;
        }

        private string GridViewToExcelReportUpd(DataTable data, string ReportHeader, string reportFileName)
        {
            string fileName = reportFileName + ".xlsx";
            string FactoryName = "";
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            string CmpName = "";
            string FactoryAddress = string.Empty;
            clsReport objRpt = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                objRpt = new clsReport();
                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);
                //save the file to server temp folder
                string fullPath = Path.Combine(HostingEnvironment.MapPath("~/") + fileName);

                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    IApplication application = excelEngine.Excel;
                    application.DefaultVersion = ExcelVersion.Excel2013;
                    IWorkbook workbook = application.Workbooks.Create(1);
                    IWorksheet sheet = workbook.Worksheets[0];

                    int ROW = 4;
                    sheet[ROW, 1].Text = ReportHeader;
                    sheet[ROW, 1, ROW, data.Columns.Count].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet[ROW, 1, ROW, data.Columns.Count].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet[ROW, 1].CellStyle.Font.Bold = true;
                    ROW++;

                    //sheet.ImportDataTable(data, true, ROW, 1);
                    //sheet[ROW, 1, ROW, data.Columns.Count].ColumnWidth = 20;
                    //sheet[ROW, 1, ROW, data.Columns.Count].WrapText = true;
                    //sheet[ROW, 1, ROW, data.Columns.Count].BorderAround(ExcelLineStyle.Hair);
                    //sheet[ROW, 1, ROW, data.Columns.Count].BorderInside(ExcelLineStyle.Hair);
                    //sheet.Range[ROW, 1, ROW, data.Columns.Count].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                    //sheet.Range[ROW, 1, ROW, data.Columns.Count].CellStyle.Font.Color = ExcelKnownColors.White;
                    //sheet[ROW, 1, ROW, data.Columns.Count].CellStyle.Font.Bold = true;
                    //sheet.AutoFilters.FilterRange = sheet.Range[ROW, 1, ROW, data.Columns.Count];


                    sheet.ImportDataTable(data, true, ROW, 1);

                    int dataStartRow = ROW + 1;
                    int dataEndRow = ROW + data.Rows.Count;

                    // Make CartonNo column Text
                    sheet.Range[dataStartRow, 6, dataEndRow, 6].NumberFormat = "@";

                    // Explicitly write CartonNo as text
                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        string cartonNo = Convert.ToString(data.Rows[i]["CartonNo"]);

                        sheet.Range[dataStartRow + i, 6].Text = cartonNo;
                    }

                    // Formatting
                    sheet.Range[ROW, 1, dataEndRow, data.Columns.Count].ColumnWidth = 20;
                    sheet.Range[ROW, 1, dataEndRow, data.Columns.Count].WrapText = true;

                    sheet.Range[ROW, 1, dataEndRow, data.Columns.Count].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[ROW, 1, dataEndRow, data.Columns.Count].BorderInside(ExcelLineStyle.Hair);



                    #region ******************Report Header******************
                    int endXlsCol = data.Columns.Count;
                    int xlsRow = 1, xlsCol = 1;
                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet.Range[xlsRow, xlsCol].Text = CmpName;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 17;
                    sheet.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet.Range[xlsRow, xlsCol].Text = FactoryName;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                    sheet.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet.Range[xlsRow, xlsCol].Text = FactoryAddress;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 22;
                    sheet.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet.Range[xlsRow, xlsCol].Text = reportFileName;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet.IsDisplayZeros = false;
                    sheet.UsedRange["A7"].FreezePanes();
                    sheet.FirstVisibleColumn = 1;
                    sheet.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment
                    sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.IsDisplayZeros = false;
                    sheet.UsedRange.WrapText = true;
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
                    sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    sheet.PageSetup.FitToPagesTall = 0;
                    sheet.PageSetup.FitToPagesWide = 1;
                    sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet.IsDisplayZeros = false;
                    sheet.Name = reportFileName;
                    #endregion Page Setup

                    workbook.SaveAs(fullPath);

                }
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {

            }
            return fileName;
        }

        private string GridToExcelReport(DataTable data, string ReportHeader)
        {
            string fileName = "GRID" + System.DateTime.Now.Ticks.ToString() + ".xlsx";
            try
            {

                //save the file to server temp folder
                string fullPath = Path.Combine(HostingEnvironment.MapPath("~/") + fileName);

                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    IApplication application = excelEngine.Excel;
                    application.DefaultVersion = ExcelVersion.Excel2013;
                    IWorkbook workbook = application.Workbooks.Create(1);
                    IWorksheet sheet = workbook.Worksheets[0];

                    int ROW = 1;
                    sheet[ROW, 1].Text = ReportHeader;
                    sheet[ROW, 1].CellStyle.Font.Bold = true;

                    ROW++;
                    sheet.ImportDataTable(data, true, ROW, 1);
                    sheet[ROW, 1, ROW, data.Columns.Count].BorderAround(ExcelLineStyle.Hair);
                    sheet[ROW, 1, ROW, data.Columns.Count].BorderInside(ExcelLineStyle.Hair);
                    sheet[ROW, 1, ROW, data.Columns.Count].CellStyle.ColorIndex = ExcelKnownColors.Gold;
                    sheet[ROW, 1, ROW, data.Columns.Count].CellStyle.Font.Bold = true;



                    workbook.SaveAs(fullPath);

                }
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {

            }
            return fileName;
        }

        [HttpGet, Authorize]
        public ActionResult Download(string FileName)
        {
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();
                string fullPath = HostingEnvironment.MapPath("~/") + FileName;
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(fullPath);
                try
                {
                    System.IO.File.Delete(fullPath);
                }
                catch (Exception)
                {
                }

                workbook.SaveAs(FileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;

            }
            catch (Exception ex)
            {


            }
            return null;
        }
        [HttpGet, Authorize]
        public ActionResult DownloadPPT(string FileName)
        {
            try
            {

                ExcelEngine excelEngine = new ExcelEngine();
                string fullPath = HostingEnvironment.MapPath("~/") + FileName;
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(fullPath);
                try
                {
                    System.IO.File.Delete(fullPath);
                }
                catch (Exception)
                {
                }

                workbook.SaveAs(FileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;

            }
            catch (Exception ex)
            {


            }
            return null;
        }

        [HttpGet, Authorize]
        public ActionResult PPTFileDownLoad(string FileName)
        {
            string fullPath = HostingEnvironment.MapPath("~/") + FileName;
            IPresentation presentation = Presentation.Open(fullPath);

            try
            {
                System.IO.File.Delete(fullPath);
            }
            catch (Exception)
            {
            }

            presentation.Save(FileName, FormatType.Pptx, System.Web.HttpContext.Current.Response);
            return null;
        }


        [HttpGet, Authorize]
        public ActionResult DownloadUsingFullPath(string FullPath, string fileName)
        {
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();
                //string fullPath = HostingEnvironment.MapPath("~/") + FileName;
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(FullPath);
                try
                {
                    System.IO.File.Delete(FullPath);
                }
                catch (Exception)
                {
                }

                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;

            }
            catch (Exception ex)
            {


            }
            return null;
        }
        [HttpGet, Authorize]
        public ActionResult DownloadPdf(string FileName)
        {
            try
            {

                string fullPath = HostingEnvironment.MapPath("~/") + FileName;
                PdfLoadedDocument loadedDocument = new PdfLoadedDocument(fullPath);
                try
                {
                    System.IO.File.Delete(fullPath);
                }
                catch (Exception)
                {
                }

                loadedDocument.Save(FileName, HttpContext.ApplicationInstance.Response, Syncfusion.Pdf.HttpReadType.Save);
                return null;

            }
            catch (Exception)
            {


            }
            return null;
        }
        [HttpGet, Authorize]
        public ActionResult DownloadCSV(string FileName)
        {
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();
                string fullPath = HostingEnvironment.MapPath("~/") + FileName;
                //IWorkbook workbook = excelEngine.Excel.Workbooks.Open(fullPath);
                try
                {
                    System.IO.File.Delete(fullPath);
                }
                catch (Exception)
                {
                }

                //workbook.SaveAs(FileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return View();

            }
            catch (Exception ex)
            {


            }
            return View();
        }

        [HttpPost, Authorize]
        public JsonResult ExcelExportJsonWithHeader(object obj, string ReportHeader = "")
        {
            //Json
            try
            {
                DataTable dt = new DataTable("APIDATA");
                var json = new JavaScriptSerializer().Serialize(obj);

                if (json != "[]")
                {
                    json = json.Replace("\\", "");

                    dt = CustomJsonResult.ToDataTable(json);
                }

                StringCollection strCol = new StringCollection();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (dt.Columns[i].ColumnName.ToUpper().Contains("ID") || dt.Columns[i].ColumnName.ToUpper().Contains("PK") || dt.Columns[i].ColumnName.ToUpper().Contains("EJVALUE"))
                    {
                        strCol.Add(dt.Columns[i].ColumnName);
                    }
                }
                foreach (string item in strCol)
                {
                    dt.Columns.Remove(item);
                }

                string filename = GridToExcelReportWithHeader(dt, ReportHeader);


                return Json(new { FileName = filename, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

            //return View();
        }
        private string GridToExcelReportWithHeader(DataTable data, string ReportHeader)
        {
            string fileName = "GRID" + System.DateTime.Now.Ticks.ToString() + ".xlsx";
            clsReport objRpt = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            string FactoryName = "";
            string CmpName = "";
            string FactoryAddress = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                objRpt = new clsReport();
                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);
                //save the file to server temp folder
                string fullPath = Path.Combine(HostingEnvironment.MapPath("~/") + fileName);

                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    IApplication application = excelEngine.Excel;
                    application.DefaultVersion = ExcelVersion.Excel2013;
                    IWorkbook workbook = application.Workbooks.Create(1);
                    IWorksheet sheet = workbook.Worksheets[0];

                    int ROW = 1;
                    sheet[ROW, 1].Text = ReportHeader;
                    sheet[ROW, 1].CellStyle.Font.Bold = true;

                    ROW++;
                    sheet.ImportDataTable(data, true, ROW, 1);
                    sheet[ROW, 1, ROW, data.Columns.Count].BorderAround(ExcelLineStyle.Hair);
                    sheet[ROW, 1, ROW, data.Columns.Count].BorderInside(ExcelLineStyle.Hair);
                    sheet[ROW, 1, ROW, data.Columns.Count].CellStyle.ColorIndex = ExcelKnownColors.Gold;
                    sheet[ROW, 1, ROW, data.Columns.Count].CellStyle.Font.Bold = true;

                    #region ******************Report Header******************
                    int xlsRow = 1, xlsCol = 1;
                    int endXlsCol = data.Columns.Count;
                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet.Range[xlsRow, xlsCol].Text = CmpName;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 17;
                    sheet.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet.Range[xlsRow, xlsCol].Text = FactoryName;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                    sheet.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet.Range[xlsRow, xlsCol].Text = FactoryAddress;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 22;
                    sheet.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet.Range[xlsRow, xlsCol].Text = "Issue List ";
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet.IsDisplayZeros = false;
                    sheet.UsedRange["A7"].FreezePanes();
                    sheet.FirstVisibleColumn = 1;
                    sheet.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment
                    sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.IsDisplayZeros = false;
                    sheet.UsedRange.WrapText = true;
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
                    sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    sheet.PageSetup.FitToPagesTall = 0;
                    sheet.PageSetup.FitToPagesWide = 1;
                    sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet.IsDisplayZeros = false;
                    sheet.Name = "Task List";
                    #endregion Page Setup

                    workbook.SaveAs(fullPath);

                }
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {

            }
            return fileName;
        }

    }
}