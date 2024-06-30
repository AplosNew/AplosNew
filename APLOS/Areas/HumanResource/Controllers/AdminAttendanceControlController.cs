#region Using

using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using Newtonsoft.Json;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using OTSBD;
using System.Linq;
using clsAttendance;
using System.Web.Script.Serialization;
using Library.HumanResource.Attendance.Manual;
using SetINOUT;
using Library.HumanResource.NewAttendanceProcess;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using Library.Model.Enums;
using Library.Data;
using System.IO;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class AdminAttendanceControlController : BaseController
    {

        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;


        public AdminAttendanceControlController(IUnitOfWork U, ISqlRepository R)
        {

            _unitOfWork = U;
            _sqlRepository = R;
        }

        #endregion Constructor
        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        [HttpPost, Authorize]
        public ActionResult getAllEmployees(string fromdate, string todate, string PlantId)
        {
            TimeSpan ts = Convert.ToDateTime(todate).Subtract(Convert.ToDateTime(fromdate));
            if (Math.Abs(ts.TotalDays) > 31)
                return Json(new { Error = true, Message = "Timespan between from and to date cannot be greater than 31 days" }, JsonRequestBehavior.AllowGet);

            string sql = @"
                        SELECT distinct Emp.SystemID AS Id,
                        EMP.EmployeeName
                        ,EMP.EmployeeCode,emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric
                        ,EMP.EmpPicPath,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName Department,S.UserName Section,
                            EMP.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant,emp.PlantId as PlantID
                            FROM EmployeeInformation EMP
                            INNER JOIN AttdnProcessData O ON EMP.SystemID=o.EmpSystemID 
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.PlantId='" + PlantId + @"' AND o.WorkDate BETWEEN '" + fromdate + @"' AND '" + todate + @"'
    order by EmployeeCodePreFix,EmployeeCodeNumeric

                    ";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [HttpPost, Authorize]
        public ActionResult getAttendanceData(string employeeid, string fromdate, string todate, string PlantId)
        {
            AdminAttendanceControlService app = new AdminAttendanceControlService();
            string sql = app.stringAttendanceData(employeeid, fromdate, todate, PlantId);

            string shiftSQL = @" SELECT * FROM ShiftDefination AS sd WHERE sd.IsActive=1 and sd.PlantID='" + PlantId + @"'";

            var jsondata = Json(new { data = _sqlRepository.GetModelCollection<AttendanceProcessNewProcess>(sql), shift = _sqlRepository.GetDataCollection(shiftSQL) }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult getShift(string systemid, string WorkDate)
        {
            try
            {

                AdminAttendanceControlService mau = new AdminAttendanceControlService();

                return Json(mau.GetShiftData(systemid, WorkDate), JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult getAttendance(string empsystemid, string WorkDate)
        {
            string sql = @"SELECT 
                            FORMAT(pdate,'dd-MMM-yyyy') AS PDate,FORMAT(ptime,'hh:mm:ss tt') AS PTime,PType

                             FROM AttdnRawData WHERE LogDownLoadNum='" + empsystemid + @"' AND PDate BETWEEN DATEADD(DAY,-1,'" + WorkDate + @"') AND DATEADD(DAY,1,'" + WorkDate + @"')

                            ORDER BY AttdnRawData.PDate,AttdnRawData.PTime ASC";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetDayStatus(string PlantId)
        {
            try
            {
                AdminAttendanceControlService mau = new AdminAttendanceControlService();

                return Json(mau.GetDayStatus(PlantId), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpPost]
        public ActionResult SaveSingleEmployee(List<AttendanceProcessNewProcess> data , string Remarks)
        {
            AdminAttendanceControlService mau = new AdminAttendanceControlService();
            RTx _rt = mau.Savex(data , Remarks);

            if (_rt.IsError)
            {
                return Json(new { Message = _rt.msg, Error = true, Data = _rt.data }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Error = false, Message = _rt.msg, Data = _rt.data }, JsonRequestBehavior.AllowGet);
            }
        }

        // For the Update Tab

        [Authorize, HttpGet]
        public ActionResult getEmployees(string plantId)
        {
            try
            {
                AdminAttendanceControlService mau = new AdminAttendanceControlService();
                return Json(mau.getEmployees(plantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetSampleReport(string PlId, string FD, string TD, string Emps)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var fileName = "ManualUpload" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
            string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

            IWorkbook workbook = GetWorkSheet(PlId, FD, TD, Emps);
            workbook.Version = ExcelVersion.Excel2016;
            workbook.SaveAs(fullPath);

            return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

        }

        private IWorkbook GetWorkSheet(string PlId, string FD, string TD, string Emps)
        {
            AdminAttendanceControlService mau = new AdminAttendanceControlService();
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];


            /// Sheet 1 
            DataTable data = mau.getCurrentFile(PlId, FD, TD, Emps);

            sheet.Name = "Current-Data";



            int ROW = 1;
            int endCol = 1;
            int COL = 1;

            #region Headers

            report.SetHeaderText(ref sheet, ROW, COL, "RowId", 8, ExcelHAlign.HAlignLeft);
            int ColRowId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "EmpSystemID", 8, ExcelHAlign.HAlignLeft);
            int ColEmpId = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Employee  Name", 25, ExcelHAlign.HAlignLeft);
            int ColEN = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Department", 25, ExcelHAlign.HAlignLeft);
            int ColDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 20, ExcelHAlign.HAlignLeft);
            int ColSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SubSection", 28, ExcelHAlign.HAlignLeft);
            int ColSS = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Line", 8, ExcelHAlign.HAlignLeft);
            int ColLine = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "WorkDate", 8, ExcelHAlign.HAlignLeft);
            int ColWD = COL;
            COL++;
          

            report.SetHeaderText(ref sheet, ROW, COL, "InTime", 8, ExcelHAlign.HAlignLeft);
            int ColInT = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "OutTime", 8, ExcelHAlign.HAlignLeft);
            int ColOuT = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ShiftSystemID", 8, ExcelHAlign.HAlignLeft);
            int ColShId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "DayStatus", 8, ExcelHAlign.HAlignLeft);
            int ColDS = COL;
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
                sheet[ROW, ColRowId].Text = data.Rows[i]["RowId"].ToString();
                sheet[ROW, ColEmpId].Text = data.Rows[i]["EmpSystemID"].ToString();
                sheet[ROW, ColWD].Text = data.Rows[i]["WorkDate"].ToString();
                sheet[ROW, ColInT].Text = data.Rows[i]["InTime"].ToString();
                sheet[ROW, ColOuT].Text = data.Rows[i]["OutTime"].ToString();
                sheet[ROW, ColShId].Text = data.Rows[i]["ShiftSystemID"].ToString();
                sheet[ROW, ColDS].Text = data.Rows[i]["DayStatus"].ToString();
                sheet[ROW, ColEN].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                sheet[ROW, ColSS].Text = data.Rows[i]["SubSection"].ToString();
                sheet[ROW, ColLine].Text = data.Rows[i]["Line"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }
            endRow = ROW - 1;



            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);

            return workbook;
        }

        //Importing
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
                List<DataMod> data = new List<DataMod>();
                List<object> ret = new List<object>();
                ReadFile(path, out dsExcel);

                data = dsExcel.Tables[0].ToList<DataMod>();


                if (data.Count > 0)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        string empId = bplib.clsWebLib.RetValidLen(data[i].EmpSystemID).ToString();
                        string rowId = bplib.clsWebLib.RetValidLen(data[i].RowId).ToString();
                        string wd = bplib.clsWebLib.RetValidLen(data[i].WorkDate).ToString();
                        if (empId != "" && rowId != "" && wd != "")
                        {
                            ret.Add(data[i]);
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

        [HttpPost]
        public ActionResult SaveFileList(List<Dictionary<string, object>> data, string PlId, string FD, string TD, string Emps , string Remarks)
        {
            try
            {
                AdminAttendanceControlService mau = new AdminAttendanceControlService();
                mau.SaveFileList(data, PlId, FD, TD, Emps, Remarks);
                return Json(new { Error = false, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        public class DataMod
        {

            public string RowId { get; set; }
            public string EmpSystemID { get; set; }
            public string WorkDate { get; set; }
            public string InTime { get; set; }
            public string OutTime { get; set; }
            public string ShiftSystemID { get; set; }
            public string DayStatus { get; set; }

        }
    }
}