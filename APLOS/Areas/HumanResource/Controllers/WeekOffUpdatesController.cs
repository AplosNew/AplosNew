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
                //dt.Columns.Add("EmpSystemId", typeof(String));
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
        public ActionResult getWeekOffCbo()
        {
            return Json(rs.getWeekOffCbo(), JsonRequestBehavior.AllowGet);
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

        [HttpGet , Authorize]
        public ActionResult getBudgets()
        {
            return Json(rs.getBudgets(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost , Authorize]
        public ActionResult getWeekOffsLists(string EmpID)
        {
            return Json(rs.getWeekOffsLists(EmpID) , JsonRequestBehavior.AllowGet);
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
        public ActionResult ProcessAttendance(string EffectiveDate,string EmpData)
        {
            try
            {
                WeekOffUpdatesService rep = new WeekOffUpdatesService();
                string result = rep.ProcessAttendance(EffectiveDate, EmpData);
                if (result != "true")
                {
                    return Json(new { Error = true, Message = result }, JsonRequestBehavior.AllowGet);

                }
                return Json(new { Error = false, Message = "Saved SuccessFully..." }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetWeekOffSampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = GetWeekOffSampleFileWB(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "Employee Week Off Data upload Sample File";

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
        [HttpPost, Authorize]
        //public ActionResult ImportWeekOffData()
        //{
        //    string path;

        //    try
        //    {
        //        var file = Request.Files["file"];
        //        //string plantId = Request.Files["plantId"].ToString();
        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        SaveFile(out path);
        //        var data = EMPWeekOffReadData(path);

        //        var json = Json(data, JsonRequestBehavior.AllowGet);
        //        json.MaxJsonLength = int.MaxValue;
        //        return json;
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Error = true, Message = ex.Message });
        //    }
        //}
        public List<object> EMPWeekOffReadData(string path)
        {

            DataSet dsExcel = null;
            try
            {
                List<rosWeek> data = new List<rosWeek>();
                List<object> ret = new List<object>();
                ReadFile(path, out dsExcel);

                data = dsExcel.Tables[0].ToList<rosWeek>();
                List<string> RostersList = rs.getWeekOffList();

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
                                    data[i].EmpSystemId = emps.DefaultView[0]["EmpSystemId"].ToString();
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

        [HttpPost, Authorize]
        public JsonResult ImportWeekOffData(FormCollection form)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<UploadedWeekOffDataViewModel> data = new List<UploadedWeekOffDataViewModel>();

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
                else
                {
                    throw new CustomException(Resources.ExcelUploadError);
                }
                string path = "";
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetAttendanceRawData(), file.FileName);
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
                FileInfo docFile;
                string exception = "\r\n";
                try
                {
                    try
                    {
                        string connString = string.Empty;
                        ExcelEngine excelEngine = null;
                        IApplication application = null;
                        IWorkbook workbook = null;

                        excelEngine = new ExcelEngine();
                        application = excelEngine.Excel;
                        workbook = excelEngine.Excel.Workbooks.Open(path);

                        DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);
                        DataSet dsExcel = new DataSet();
                        dsExcel.Tables.Add(dt);


                        docFile = new FileInfo(path);
                        if (docFile.Exists)
                        {
                            exception += "\r\nTrying to delete";
                            docFile.Delete();
                        }

                        if (dsExcel.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                            {
                                UploadedWeekOffDataViewModel vm = new UploadedWeekOffDataViewModel();

                                vm.EmpSystemId = dsExcel.Tables[0].Rows[i][0].ToString().Trim();
                                vm.WOHeaderId = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
                                vm.EffectiveDate = dsExcel.Tables[0].Rows[i][2].ToString().Trim();
                                //vm.EmployeeCode = dsExcel.Tables[0].Rows[i][3].ToString().Trim();
                                //vm.EmployeeName = dsExcel.Tables[0].Rows[i][4].ToString().Trim();

                                data.Add(vm);

                            }
                        }
                        else
                        {
                            throw new Exception("Please Select File");
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
                catch (Exception ex)
                {
                    //throw ex;
                }
                finally
                {
                }
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }
        public DataTable GetWeekOffData()
        {
            var cmdText = @"SELECT * FROM [DBO].[EmployeeWeeklyOff] where 1=2  ";
            return _sqlRepository.GetDataTable(cmdText);
        }

        public IWorkbook GetWeekOffSampleFileWB(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName)
        {
            #region declare
            clsReport objRpt = null;
            OTSBD.clsStaticInfo objStatic = null;
            objStatic = new OTSBD.clsStaticInfo();
            string OTConsiderOn = string.Empty;

            #endregion
            try
            {
                ReportUtility ru = new ReportUtility();

                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = ru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;

                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;

                #region Lunch Out
                IWorksheet sheet1 = null;
                sheet1 = workbook.Worksheets[0];
                IWorksheet sheetSource = null;
                sheetSource = workbook.Worksheets[1];
                xlsRow = 1;

                #region ------------------Column Header------------------


                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmpSystemId"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 16; int colEmpSystemId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "WOHeaderId"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10; int colWOHeaderId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EffectiveDate"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10; int colEffectiveDate = xlsCol; xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeCode"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 16; int colEmployeeCode = xlsCol; xlsCol += 1;
                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeName"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20; int colEmployeeName = xlsCol; xlsCol += 1;
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                xlsRow++;

                #endregion ------------------Column Header------------------
                DataTable dtData = GetWeekOffData();
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    sheet1[xlsRow, colEmpSystemId].Text = dtData.Rows[i]["EmpSystemId"].ToString();
                    sheet1[xlsRow, colWOHeaderId].Text = dtData.Rows[i]["WOHeaderId"].ToString();
                    sheet1[xlsRow, colEffectiveDate].Text = dtData.Rows[i]["EffectiveDate"].ToString();
                    //sheet1[xlsRow, colEmployeeCode].Text = dtData.Rows[i]["EmployeeCode"].ToString();
                    //sheet1[xlsRow, colEmployeeName].Text = dtData.Rows[i]["EmployeeName"].ToString();
                    xlsRow++;
                }
                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 10;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Sheet1";
                #endregion Page Setup

                #endregion  Lunch Out

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }




            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }


            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        } 

        [HttpPost, Authorize]
        public JsonResult SaveUploadedWeekOffData(List<Dictionary<string, object>> data)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsBC=null;
            string _Id = string.Empty;
            try
            {
                #region Entity 
                //objCon = new ConnectionManager.DAL.ConManager("1");

                //string strSQL = "Delete FROM [DBO].[EmployeeWeeklyOff]";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenConnection("1");
                //objCon.BeginTransaction();
                //objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                //objCon.CommitTransaction();

                objCon = new ConnectionManager.DAL.ConManager("1");

                if (data != null)
                {
                    DataTable no = _sqlRepository.GetDataTable("Select top 1 Id as Nos from dbo.EmployeeWeeklyOff order by Cast(Id as numeric) desc");
                    int id = int.Parse(no.Rows[0]["Nos"].ToString()) + 1;
                    foreach (var item in data)
                    {
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [DBO].[EmployeeWeeklyOff] where EmpSystemId='" + Convert.ToInt64(item["EmpSystemId"]) + "' and EffectiveDate='" + Convert.ToDateTime(item["EffectiveDate"]).ToString("yyyy-MM-dd") + "'", out dsBC, false, "1");
                        DataView dv = new DataView(dsBC.Tables[0]);
                        dv.RowFilter = "EmpSystemId='" + Convert.ToInt64(item["EmpSystemId"]) + "' and EffectiveDate='"+ Convert.ToDateTime(item["EffectiveDate"]).ToString("yyyy-MM-dd") +"'";
                        
                        if (dv.Count == 0)
                        {
                            //item["EffectiveDate"] = DateTime.Now;
                            item["Id"] = id.ToString();
                            AddNewRow(dsBC.Tables[0], item);
                            id++;
                            OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                            obj.SaveDataSets(dsBC);
                        }
                        //else
                        //{
                        //    DataRow drmo = dv[0].Row;
                        //    //item["EffectiveDate"] = DateTime.Now;
                        //    EditRow(drmo, item);
                        //}
                    }


                }
                #endregion
                
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public class UploadedWeekOffDataViewModel
        {
            //public string EmployeeCode { get; set; }
            //public string EmployeeName { get; set; }
            public string EmpSystemId { get; set; }
            public string WOHeaderId { get; set; }
            public string EffectiveDate { get; set; }

        }
    }


}