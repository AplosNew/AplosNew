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
    public class AnnualLeaveProcessController  : BaseController
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
        public ActionResult getCurrentList(string PlantId,string YearId)
        {
            try
            {
                return Json(_leave.getCurrentList(PlantId,YearId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult getCompany()
        {
            try {
                return Json(_leave.getCompany(), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
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
        public ActionResult SaveFileList(List<Dictionary<string, object>> data, string PlantId,string YearId)
        {
            try
            {
                _leave.SaveFileList(data, PlantId,YearId);
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
        public ActionResult GetSampleReport(string PlantId, string name,string LvYearId, ReportFormat reportFormat)
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

        private IWorkbook GetWorkSheet(string PlantId,string LvId)
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
                sheet[ROW, ColEmpId].Text = data.Rows[i]["EmpId"].ToString();
                sheet[ROW, ColLvYear].Text = data.Rows[i]["LeaveYear"].ToString();
                sheet[ROW, ColLvType].Text = data.Rows[i]["LeaveType"].ToString();
                sheet[ROW, ColLvYear].Text = data.Rows[i]["LeaveYear"].ToString();
                sheet[ROW, ColLvType].Text = data.Rows[i]["LeaveType"].ToString();
                sheet[ROW, ColPlant].Text = data.Rows[i]["Plant"].ToString();
                sheet[ROW, ColRegEncashment].Text = data.Rows[i]["RegularEncashment"].ToString();
                sheet[ROW, ColEarned].Text = data.Rows[i]["Earned"].ToString();
                sheet[ROW, ColAvailed].Text = data.Rows[i]["Availed"].ToString();
                sheet[ROW, ColAdjustment].Text = data.Rows[i]["Adjustment"].ToString();
                sheet[ROW, ColLvYearId].Text = data.Rows[i]["LeaveYearId"].ToString();
                sheet[ROW, ColLvTypeId].Text = data.Rows[i]["LeaveTypeId"].ToString();

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
                        string LvtypeId= clsWebLib.RetValidLen(data[i].LeaveTypeId).ToString();


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
        public ActionResult getNewLeaveYear(string PlantId,string LvYearId)
        {
            try
            {
                return Json(alp.GetNewLvYear(PlantId,LvYearId), JsonRequestBehavior.AllowGet);
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
        public ActionResult LoadData(string PlantId, string LvYearId,List<string> LvTypeId,List<string> EmpCategory)
        {
            try
            {
                return Json(alp.LoadData(PlantId, LvYearId,LvTypeId,EmpCategory), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public ActionResult ProcessData(string Data, string PlantId, string CurrentLvYearId,decimal MaxCarryForward,
            decimal MaxEncash,decimal MaxLapse,string NewYear,List<string> LeaveTypeList)
        {
            try
            {
                alp.ProcessData(Data, PlantId, CurrentLvYearId,MaxCarryForward,MaxEncash,MaxLapse,NewYear,LeaveTypeList);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.ToString() }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { Error = false, Message = "Annual Leave Process Ran Successfully..." }, JsonRequestBehavior.AllowGet);

        }


        #endregion

        #region Regular Encashment Functions

        [HttpGet, Authorize]
        public ActionResult GetEmpInfo(string PlantId, string From, string To,string Year)
        {
            try
            {
                return Json(reg.GetEmpInfo(PlantId, From, To,Year), JsonRequestBehavior.AllowGet);
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
    }

    public class LeaveOpeningData
    {
        public string Earned { get; set; }
        public string Availed { get; set; }
        public string Adjustment { get; set; }
        public string EmployeeId { get; set;}
        public string LeaveYearId{ get; set; }
        public string LeaveTypeId { get; set; }
        public string LeaveType { get; set; }
        public string RegularEncashment { get; set; }
    }

}