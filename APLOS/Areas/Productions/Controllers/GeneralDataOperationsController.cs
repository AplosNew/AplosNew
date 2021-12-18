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
using Library.OrderManagement.Production;
//using TBS;

namespace Aplos.Areas.Productions.Controllers
{

    public class GeneralDataOperationsController : BaseController
    {
        // add a header verification - 1. Basic Authentication .... 2. Payload

        #region Constructor
        /// <summary>   The separationTypeService service. </summary>

        private readonly ISqlRepository _sqlRepository;

        GeneralDataOperationsService rs = new GeneralDataOperationsService();
        public GeneralDataOperationsController(ISqlRepository sqlRepository)
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



        //[HttpGet, Authorize]
        //public ActionResult getCurrentList()
        //{
        //    return Json(rs.getCurrentList(), JsonRequestBehavior.AllowGet);
        //}


        //The Second Page 
        /// 
        //The Getting of Sample Report

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
            var reportFileName = "GeneralDataUpload-" + date;
            var workbook = GetGeneralDataUploadWorkSheet();
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

        private IWorkbook GetGeneralDataUploadWorkSheet()
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            var sheet2 = workbook.Worksheets[1];

            /// Sheet 1 
            DataTable data = rs.getGeneralDataUploadFile();

            sheet.Name = "General-Data-Upload";



            int ROW = 1;
            int endCol = 1;
            int COL = 1;

            #region Headers
            //report.SetHeaderText(ref sheet, ROW, COL, "Employee Id ", 12, ExcelHAlign.HAlignLeft);
            //int ColEmpSystemId = COL;
            //COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "MasterId", 8, ExcelHAlign.HAlignLeft);
            int ColMasterId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "TransactionDate", 8, ExcelHAlign.HAlignLeft);
            int ColTrnDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ByWhomCode", 8, ExcelHAlign.HAlignLeft);
            int ColByWhom = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remark", 8, ExcelHAlign.HAlignLeft);
            int ColRemark = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Value", 8, ExcelHAlign.HAlignLeft);
            int ColValue = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ReferenceNumber", 8, ExcelHAlign.HAlignLeft);
            int ColRefNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "EmpSystemId", 8, ExcelHAlign.HAlignLeft);
            int ColEmpId = COL;
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
                sheet[ROW, ColMasterId].Text = data.Rows[i]["MasterId"].ToString();
                sheet[ROW, ColTrnDate].Text = data.Rows[i]["TransactionDate"].ToString();
                sheet[ROW, ColByWhom].Text = data.Rows[i]["ByWhom"].ToString();
                sheet[ROW, ColRemark].Text = data.Rows[i]["Remark"].ToString();
                sheet[ROW, ColValue].Text = data.Rows[i]["Value"].ToString();
                sheet[ROW, ColRefNo].Text = data.Rows[i]["ReferenceNumber"].ToString();
                sheet[ROW, ColEmpId].Text = data.Rows[i]["EmpSystemId"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }
            endRow = ROW - 1;


            //Sheet 2

            DataTable data2 = rs.getGeneralMasterFile();

            sheet2.Name = "MasterTable";



            int ROW2 = 1;
            int endCol2 = 1;
            int COL2 = 1;

            #region Headers
            report.SetHeaderText(ref sheet2, ROW2, COL2, "Id", 12, ExcelHAlign.HAlignLeft);
            int ColId = COL2;
            COL2++;

            report.SetHeaderText(ref sheet2, ROW2, COL2, "User Name", 8, ExcelHAlign.HAlignLeft);
            int ColUsrName = COL2;
            COL2++;

            report.SetHeaderText(ref sheet2, ROW2, COL2, "Value Type", 8, ExcelHAlign.HAlignLeft);
            int ColValType = COL2;
            COL2++;

            report.SetHeaderText(ref sheet2, ROW2, COL2, "Category", 8, ExcelHAlign.HAlignLeft);
            int ColCat = COL2;
            COL2++;

            report.SetHeaderText(ref sheet2, ROW2, COL2, "Sub Category", 8, ExcelHAlign.HAlignLeft);
            int ColSCat = COL2;
            COL2++;

            endCol2 = COL2;
            #endregion Headers
            ROW2++;
            var startRow2 = 0;
            var endRow2 = 0;
            int RowIndex2 = ROW2;
            startRow2 = ROW2;
            for (int i = 0; i < data2.Rows.Count; i++)
            {
                sheet2[ROW2, ColId].Text = data2.Rows[i]["Id"].ToString();
                sheet2[ROW2, ColUsrName].Text = data2.Rows[i]["UserName"].ToString();
                sheet2[ROW2, ColValType].Text = data2.Rows[i]["ValueType"].ToString();
                sheet2[ROW2, ColCat].Text = data2.Rows[i]["Category"].ToString();
                sheet2[ROW2, ColSCat].Text = data2.Rows[i]["SubCategory"].ToString();

                sheet2.Range[ROW2, 1, ROW2, endCol2].BorderInside(ExcelLineStyle.Hair);
                sheet2.Range[ROW2, 1, ROW2, endCol2].BorderAround(ExcelLineStyle.Hair);

                ROW2++;

            }
            endRow2 = ROW2 - 1;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


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

        public List<object> ReadData(string path)
        {

            DataSet dsExcel = null;
            try
            {
                List<DUpload> data = new List<DUpload>();
                List<object> ret = new List<object>();
                ReadFile(path, out dsExcel);

                data = dsExcel.Tables[0].ToList<DUpload>();
                

                DataTable emps = rs.getEmployeesAll();

                if (data.Count > 0)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                                emps.DefaultView.RowFilter = @"EmployeeCode='" + data[i].ByWhomCode + "'";
                                if (emps.DefaultView.Count > 0)
                                {
                                    data[i].ByWhom = emps.DefaultView[0]["SystemId"].ToString();
                                    ret.Add(data[i]);
                                }
                                else
                                {
                                    throw new Exception("This Employee Code doesn't exists - " + data[i].ByWhomCode);
                                }
                                //ret.Add(data[i]);
                           

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
                dt.Columns.Add("ByWhom", typeof(String));
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

        // The 2nd Tab Downloading Operations
        [HttpPost , Authorize]
        public ActionResult getMasters()
        {
            return Json(rs.getMasters(), JsonRequestBehavior.AllowGet);
        }

        [Authorize , HttpPost]
        public ActionResult getReport(string ToDate, string FromDate, string MasterId)
        {
            return Json(rs.getReport(ToDate, FromDate, MasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult downloadTheReport(string ToDate, string FromDate, string MasterId)
        {

            try
            {
                var workbook = GetFilterData(ToDate,FromDate , MasterId);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + "-" + "GDReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IWorkbook GetFilterData(string ToDate, string FromDate, string MasterId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "General Data Report";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;

            DataTable dtData = rs.getReportDownload( ToDate,  FromDate,  MasterId);


            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Transaction ID", 13, ExcelHAlign.HAlignCenter);
            int ColTrId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Master Name", 13, ExcelHAlign.HAlignCenter);
            int ColMsName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Transaction Date", 13, ExcelHAlign.HAlignCenter);
            int ColTrDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remark", 13, ExcelHAlign.HAlignCenter);
            int ColRemarks = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Value", 13, ExcelHAlign.HAlignCenter);
            int ColVal = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Value Type", 13, ExcelHAlign.HAlignCenter);
            int ColValType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "By Whom", 13, ExcelHAlign.HAlignCenter);
            int ColByWhom = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Category", 13, ExcelHAlign.HAlignCenter);
            int ColCat = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Sub Category", 15, ExcelHAlign.HAlignCenter);
            int ColSCat = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Reference Number", 13, ExcelHAlign.HAlignCenter);
            int ColRefNo = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 13, ExcelHAlign.HAlignCenter);
            int ColEmName = COL;
            COL++;


            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                sheet[ROW, ColTrId].Text = dtData.Rows[i]["TransactionId"].ToString();
                sheet[ROW, ColMsName].Text = dtData.Rows[i]["MasterName"].ToString();
                sheet[ROW, ColTrDate].Text = dtData.Rows[i]["TransactionDate"].ToString();
                sheet[ROW, ColRemarks].Text = dtData.Rows[i]["Remark"].ToString();
                sheet[ROW, ColVal].Text = dtData.Rows[i]["Value"].ToString();
                sheet[ROW, ColValType].Text = dtData.Rows[i]["ValueType"].ToString();
                sheet[ROW, ColByWhom].Text = dtData.Rows[i]["ByWhom"].ToString();
                sheet[ROW, ColCat].Text = dtData.Rows[i]["Category"].ToString();
                sheet[ROW, ColSCat].Text = dtData.Rows[i]["SubCategory"].ToString();
                sheet[ROW, ColRefNo].Text = dtData.Rows[i]["ReferenceNumber"].ToString();
                sheet[ROW, ColEmName].Text = dtData.Rows[i]["EmpName"].ToString();
                

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }

            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.CompanyHeader(ref sheet, endCol, "General Data Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }




        public class DUpload
        {
            public string MasterId { get; set; }
            public string TransactionDate { get; set; }
            public string Remark { get; set; }
            public string Value { get; set; }
            public string ReferenceNumber { get; set; }
            public string EmpSystemId { get; set; }
            public string ByWhomCode { get; set; }
            public string ByWhom { get; set; }

        }
    }

   
}