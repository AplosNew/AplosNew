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
using Library.OrderManagement.Production;
//using TBS;

namespace Aplos.Areas.Productions.Controllers
{

    public class StocksAdjustmentController : BaseController
    {
       

        #region Constructor
        /// <summary>   The separationTypeService service. </summary>

        private readonly ISqlRepository _sqlRepository;

        StocksAdjustmentService sa = new StocksAdjustmentService();
        public StocksAdjustmentController(ISqlRepository sqlRepository)
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

        //The Getting of Sample Report

        [HttpGet, Authorize]
        public ActionResult getCurrentList()
        {
            return Json(sa.getCurrentList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSampleReport(ReportFormat reportFormat)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string date = DateTime.Now.Date.ToString("dd-MMM");//.Substring(0, DateTime.Now.Date.ToString().Length - 12);
            var reportFileName = "StocksAdjustment-" + date;
            var workbook = GetSampAdjReport();
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

        private IWorkbook GetSampAdjReport()
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            

            /// Sheet 1 
            DataTable data = sa.GetSampAdjReport();

            sheet.Name = "Adjustment";



            int ROW = 1;
            int endCol = 1;
            int COL = 1;

            #region Headers
            

            report.SetHeaderText(ref sheet, ROW, COL, "ProductCode", 8, ExcelHAlign.HAlignLeft);
            int ColPC = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "LotNo", 8, ExcelHAlign.HAlignLeft);
            int ColLotNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "WorkDate", 8, ExcelHAlign.HAlignLeft);
            int ColEffective = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Qty", 8, ExcelHAlign.HAlignLeft);
            int ColQty = COL;
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
                sheet[ROW, ColPC].Text = data.Rows[i]["ProductCode"].ToString();
                sheet[ROW, ColLotNo].Text = data.Rows[i]["LotNo"].ToString();
                sheet[ROW, ColEffective].Text = data.Rows[i]["WorkDate"].ToString();
                sheet[ROW, ColQty].Text = data.Rows[i]["Qty"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }
            endRow = ROW - 1;
           
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
           
            return workbook;
        }


        [HttpPost, Authorize]
        public ActionResult SaveFileList(List<Dictionary<string, object>> data)
        {
            try
            {
                sa.SaveFileList(data);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
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

        public List<object> ReadData(string path)
        {

            DataSet dsExcel = null;
            try
            {
                List<sadj> data = new List<sadj>();
                List<object> ret = new List<object>();
                ReadFile(path, out dsExcel);

                data = dsExcel.Tables[0].ToList<sadj>();

                if (data.Count > 0)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        data[i].ProductCode = data[i].ProductCode.ToString().Trim();
                        data[i].LotNo = data[i].LotNo.ToString().Trim();
                        data[i].WorkDate = Convert.ToDateTime(data[i].WorkDate).ToString();
                        data[i].Qty = clsStaticInfo.dbl(data[i].Qty.ToString()).ToString();
                        ret.Add(data[i]);
                    }
                }

                return ret;
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

        public class sadj
        {

            public string ProductCode { get; set; }
            public string LotNo { get; set; }
            public string WorkDate { get; set; }
            public string Qty { get; set; }

        }

    }

   
}