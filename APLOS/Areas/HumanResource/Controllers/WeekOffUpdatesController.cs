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
//using TBS;

namespace Aplos.Areas.HumanResource.Controllers
{

    public class WeekOffUpdatesController : BaseController
    {
        // add a header verification - 1. Basic Authentication .... 2. Payload

        #region Constructor
        /// <summary>   The separationTypeService service. </summary>

        private readonly ISqlRepository _sqlRepository;

        WeekOffUpdatesService rs = new WeekOffUpdatesService();
        public WeekOffUpdatesController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        #endregion


        #region Aplos       
        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult EWeekUpdate()
        {
            return View();
        }
        #endregion

        //#region -- Operations



        [HttpGet, Authorize]
        public ActionResult getCurrentList()
        {
            return Json(rs.getCurrentList(), JsonRequestBehavior.AllowGet);
        }


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
            var reportFileName = "EmployeeWeeklyOff-" + date;
            var workbook = GetEmployeeWeekRosterWorkSheet();
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

        private IWorkbook GetEmployeeWeekRosterWorkSheet()
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            var sheet2 = workbook.Worksheets[1];

            /// Sheet 1 
            DataTable data = rs.getEmployeeWeekRosterFile();

            sheet.Name = "Employee-Week";



            int ROW = 1;
            int endCol = 1;
            int COL = 1;

            #region Headers
            //report.SetHeaderText(ref sheet, ROW, COL, "Employee Id ", 12, ExcelHAlign.HAlignLeft);
            //int ColEmpSystemId = COL;
            //COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "EmployeeCode", 8, ExcelHAlign.HAlignLeft);
            int ColEmployeeCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "WOHeaderId", 8, ExcelHAlign.HAlignLeft);
            int ColRosterId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "EffectiveDate", 8, ExcelHAlign.HAlignLeft);
            int ColEffective = COL;
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
                sheet[ROW, ColEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                sheet[ROW, ColRosterId].Text = data.Rows[i]["RosterId"].ToString();
                sheet[ROW, ColEffective].Text = data.Rows[i]["EffectiveDate"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }
            endRow = ROW - 1;


            //Sheet 2

            DataTable data2 = rs.getRostersFile();

            sheet2.Name = "WeekOffTable";



            int ROW2 = 1;
            int endCol2 = 1;
            int COL2 = 1;

            #region Headers
            report.SetHeaderText(ref sheet2, ROW2, COL2, "Week Off Id ", 12, ExcelHAlign.HAlignLeft);
            int ColRostersId = COL2;
            COL2++;

            report.SetHeaderText(ref sheet2, ROW2, COL2, "Week Off Standard Name", 8, ExcelHAlign.HAlignLeft);
            int ColStdName = COL2;
            COL2++;

            report.SetHeaderText(ref sheet2, ROW2, COL2, "Week Off User Name", 8, ExcelHAlign.HAlignLeft);
            int ColUsrName = COL2;
            COL2++;

            report.SetHeaderText(ref sheet2, ROW2, COL2, "Week Off Description", 8, ExcelHAlign.HAlignLeft);
            int ColDes = COL2;
            COL2++;

            report.SetHeaderText(ref sheet2, ROW2, COL2, "Week Off Remarks", 8, ExcelHAlign.HAlignLeft);
            int ColRems = COL2;
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
                sheet2[ROW2, ColRostersId].Text = data2.Rows[i]["Id"].ToString();
                sheet2[ROW2, ColStdName].Text = data2.Rows[i]["StandardName"].ToString();
                sheet2[ROW2, ColUsrName].Text = data2.Rows[i]["UserName"].ToString();
                sheet2[ROW2, ColDes].Text = data2.Rows[i]["Description"].ToString();
                sheet2[ROW2, ColRems].Text = data2.Rows[i]["Remarks"].ToString();

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
                List<rosWeek> data = new List<rosWeek>();
                List<object> ret = new List<object>();
                ReadFile(path, out dsExcel);

                data = dsExcel.Tables[0].ToList<rosWeek>();
                List<string> RostersList = rs.getRostersList();

                DataTable emps = rs.getEmployeesAll();

                if (data.Count > 0)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        if (data[i].WOHeaderId != null)
                        {
                            if (RostersList.Contains(data[i].WOHeaderId))
                            {
                                emps.DefaultView.RowFilter = @"EmployeeCode='" + data[i].EmployeeCode + "'";
                                if (emps.DefaultView.Count > 0)
                                {
                                    data[i].EmpSystemId = emps.DefaultView[0]["SystemId"].ToString();
                                    data[i].EffectiveDate = Convert.ToDateTime(data[i].EffectiveDate).ToString();
                                    ret.Add(data[i]);
                                }
                                else
                                {
                                    throw new Exception("This Employee Code doesn't exists - " + data[i].EmployeeCode);
                                }
                                //ret.Add(data[i]);
                            }
                            else
                            {
                                throw new Exception("The Week Off Roster in Employee Code - " + data[i].EmployeeCode + " is not present!!");
                            }

                        }
                    }

                    //for(int i = 0; i< data.Count; i++)
                    //{

                    //}
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

            public string WOHeaderId { get; set; }
            public string EmployeeCode { get; set; }
            public string EffectiveDate { get; set; }
            public string EmpSystemId { get; set; }

        }


        // The First Tab Controllers
        [HttpGet, Authorize]
        public ActionResult getWeekOff()
        {
            return Json(rs.getWeekOff(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getEmployees()
        {
            return Json(rs.getEmployees(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getEmpWeekOff(string EmpId)
        {
            return Json(rs.getEmpWeekOff(EmpId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult saveSingle(string EmpId, string EffectiveDate, string WeekId)
        {
            try
            {
                rs.saveSingle(EmpId, EffectiveDate, WeekId);
                return Json(new { Error = false, Data = EmpId, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        // 2nd TAB Controllers

        [Authorize, HttpPost]
        public ActionResult getDistinctEmployeesToBeProcessed(string EffectiveDate)
        {
            try
            {
                return Json(rs.getDistinctEmployeesToBeProcessed(EffectiveDate), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [Authorize, HttpPost]
        public ActionResult ProcessAttendance(string EffectiveDate,DataModel data)
        {
            try
            {
                return Json(rs.ProcessAttendance(EffectiveDate,data), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


    }

   
}