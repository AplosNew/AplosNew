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

    public class EmployeeBudgetUpdateController : BaseController
    {
        // add a header verification - 1. Basic Authentication .... 2. Payload

        #region Constructor
        /// <summary>   The separationTypeService service. </summary>

        private readonly ISqlRepository _sqlRepository;

        EmployeeBudgetUpdateService rs = new EmployeeBudgetUpdateService();
        public EmployeeBudgetUpdateController(ISqlRepository sqlRepository)
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

        [HttpGet, Authorize]
        public ActionResult getPlants()
        {
            return Json(rs.getPlants(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getCurrentList(string plantId)
        {
            return Json(rs.getCurrentList(plantId), JsonRequestBehavior.AllowGet);
        }


        //The Second Page 
        /// 
        //The Getting of Sample Report

        [HttpPost, Authorize]
        public ActionResult SaveFileList(List<Dictionary<string, object>> data, string plantId)
        {
            try
            {
                rs.SaveFileList(data, plantId);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetSampleReport(string plantId, string name, ReportFormat reportFormat)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string date = DateTime.Now.Date.ToString("dd-MMM");//.Substring(0, DateTime.Now.Date.ToString().Length - 12);
            var reportFileName = "EmployeeRoster-" + name + "-" + date;
            var workbook = GetEmployeeRosterWorkSheet(plantId);
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

        private IWorkbook GetEmployeeRosterWorkSheet(string plantId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            var sheet2 = workbook.Worksheets[1];

            /// Sheet 1 
            DataTable data = rs.getEmployeeRosterFile(plantId);

            sheet.Name = "EmployeeRoster";



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

            report.SetHeaderText(ref sheet, ROW, COL, "RosterId", 8, ExcelHAlign.HAlignLeft);
            int ColRosterId = COL;
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

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }
            endRow = ROW - 1;


            //Sheet 2

            DataTable data2 = rs.getRostersFile(plantId);

            sheet2.Name = "RosterTable";



            int ROW2 = 1;
            int endCol2 = 1;
            int COL2 = 1;

            #region Headers
            report.SetHeaderText(ref sheet2, ROW2, COL2, "Roster Id ", 12, ExcelHAlign.HAlignLeft);
            int ColRostersId = COL2;
            COL2++;

            report.SetHeaderText(ref sheet2, ROW2, COL2, "Roster Standard Name", 8, ExcelHAlign.HAlignLeft);
            int ColStdName = COL2;
            COL2++;

            report.SetHeaderText(ref sheet2, ROW2, COL2, "Roster User Name", 8, ExcelHAlign.HAlignLeft);
            int ColUsrName = COL2;
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
        public ActionResult ImportData(string plantId)
        {
            string path;

            try
            {
                var file = Request.Files["file"];
                //string plantId = Request.Files["plantId"].ToString();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveFile(out path);
                var data = ReadData(path, plantId);

                var json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public List<object> ReadData(string path, string plantId)
        {

            DataSet dsExcel = null;
            try
            {
                List<rosbud> data = new List<rosbud>();
                List<object> ret = new List<object>();
                ReadFile(path, out dsExcel);

                data = dsExcel.Tables[0].ToList<rosbud>();
                List<string> RostersList = rs.getRostersList(plantId);

                DataTable emps = rs.getEmployeesAll(plantId);

                if (data.Count > 0)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        if (data[i].RosterId != null)
                        {
                            if (RostersList.Contains(data[i].RosterId))
                            {
                                emps.DefaultView.RowFilter = @"EmployeeCode='" + data[i].EmployeeCode + "'";
                                if (emps.DefaultView.Count > 0)
                                {
                                    data[i].EmpSystemId = emps.DefaultView[0]["SystemId"].ToString();
                                    ret.Add(data[i]);
                                }
                                else
                                {
                                    throw new Exception("The Employee Code doesn't belong to this plant - " + data[i].EmployeeCode);
                                }
                                //ret.Add(data[i]);
                            }
                            else
                            {
                                throw new Exception("The Roster in Employee Code - " + data[i].EmployeeCode + " is either not present or doesn't belong to this plant!!");
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

        public class rosbud
        {

            public string RosterId { get; set; }
            public string EmployeeCode { get; set; }
            public string EmpSystemId { get; set; }

        }

    }
}