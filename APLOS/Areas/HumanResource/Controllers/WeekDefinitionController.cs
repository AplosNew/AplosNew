using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;
using Library.Service.Attendances;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using Library.HumanResource.NewAttendanceProcess;
using System.Security.Cryptography;
//using TBS;

namespace Aplos.Areas.HumanResource.Controllers
{

    public class WeekDefinitionController : BaseController
    {
        // add a header verification - 1. Basic Authentication .... 2. Payload

        #region Constructor
        /// <summary>   The separationTypeService service. </summary>

        private readonly ISqlRepository _sqlRepository;

        WeekDefinitionService rs = new WeekDefinitionService();
        public WeekDefinitionController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        #endregion


        #region Aplos       
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        //#region -- Operations

        [HttpPost, Authorize]
        public ActionResult SaveFileList(List<Dictionary<string, object>> data)
        {
            try
            {
                rs.SaveFileList(data);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetSampleReport(ReportFormat reportFormat)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string date = DateTime.Now.Date.ToString("dd-MMM");//.Substring(0, DateTime.Now.Date.ToString().Length - 12);
            var reportFileName = "WeekDefinition-" + date;
            var workbook = GetWeekDefWS();
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

        private IWorkbook GetWeekDefWS()
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            var sheet2 = workbook.Worksheets[1];

            /// Sheet 1 
            DataTable data = rs.getWeekDef();

            sheet.Name = "Week-Definition";



            int ROW = 1;
            int endCol = 1;
            int COL = 1;

            #region Headers
            //report.SetHeaderText(ref sheet, ROW, COL, "Employee Id ", 12, ExcelHAlign.HAlignLeft);
            //int ColEmpSystemId = COL;
            //COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "DayNo", 8, ExcelHAlign.HAlignLeft);
            int ColDay = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Days31", 8, ExcelHAlign.HAlignLeft);
            int Col31 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Days30", 8, ExcelHAlign.HAlignLeft);
            int Col30 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Days29", 8, ExcelHAlign.HAlignLeft);
            int Col29 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Days28", 8, ExcelHAlign.HAlignLeft);
            int Col28 = COL;
            COL++;

            endCol = COL;
            #endregion Headers
            ROW++;
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;
            for (int i = 0; i < data.Rows.Count; i++)
            {
                //sheet[ROW, ColEmpSystemId].Text = data.Rows[i]["EmpSystemId"].ToString();
                sheet[ROW, ColDay].Number = clsStaticInfo.dbl(data.Rows[i]["DayNo"].ToString());
                sheet[ROW, Col31].Number = clsStaticInfo.dbl(data.Rows[i]["Days31"].ToString());
                sheet[ROW, Col30].Number = clsStaticInfo.dbl(data.Rows[i]["Days30"].ToString());
                sheet[ROW, Col29].Number = clsStaticInfo.dbl(data.Rows[i]["Days29"].ToString());
                sheet[ROW, Col28].Number = clsStaticInfo.dbl(data.Rows[i]["Days28"].ToString());

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }
            endRow = ROW - 1;

            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            report.PageSetup(ref sheet2, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }


        [HttpPost, Authorize]
        public ActionResult ImportData()
        {
            string path;

            try
            {
                var file = Request.Files["file"];
                //string plantId = Request.Files["plantId"].ToString();
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

        public List<rosWeek> ReadData(string path)
        {

            DataSet dsExcel = null;
            try
            {
                List<rosWeek> data = new List<rosWeek>();
                List<object> ret = new List<object>();
                ReadFile(path, out dsExcel);

                data = dsExcel.Tables[0].ToList<rosWeek>();
                //List<string> RostersList = rs.getRostersList();

                //if (data.Count > 0)
                //{

                //}

                return data;
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
                dt.Columns.Add("EmpSystemId", typeof(String));
                dsExcel = new DataSet();
                dsExcel.Tables.Add(dt);
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    //exception += "\r\nTrying to delete";
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

        public class rosWeek
        {

            public string DayNo { get; set; }
            public string Days31 { get; set; }
            public string Days30 { get; set; }
            public string Days29 { get; set; }
            public string Days28 { get; set; }

        }

    }

   
}