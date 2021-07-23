using Aplos.Helpers;
using Newtonsoft.Json;
using Syncfusion.Pdf.Parsing;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web;
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
        public JsonResult ExcelExportJson(object obj, string ReportHeader = "")
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

                string filename = GridToExcelReport(dt, ReportHeader);


                return Json(new { FileName = filename, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

            //return View();
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
    }
}