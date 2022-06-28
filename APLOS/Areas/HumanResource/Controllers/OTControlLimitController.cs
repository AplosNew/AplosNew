#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.NewOTProcess;
using Library.Model.Enums;
using Library.Model.Payrolls;
using Library.Security.Core;
using Library.Service.Helpers;
using Library.Service.HumanResources.Profile;
using Library.Service.Setups;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class OTControlLimitController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        OTControlLimitService oTControlLimitService = new OTControlLimitService();
        public OTControlLimitController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        #region SampleFile
        [HttpPost, Authorize]
        public ActionResult GetSampleFile(ReportFormat reportFormat)
        {
            string fileName = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            fileName = GetSampleXLFile(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "OTControlLimitTemplate";
            return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

        }

        public string GetSampleXLFile(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName)
        {
            #region declare
            clsReport objRpt = null;
            OTSBD.clsStaticInfo objStatic = null;
            objStatic = new OTSBD.clsStaticInfo();
            string OTConsiderOn = string.Empty;

            int maxRow = 5001;

            #endregion

            try
            {
                var filePath = "";
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


                #region XL
                IWorksheet sheet1 = null;
                sheet1 = workbook.Worksheets[0];
                IWorksheet sheetSource = null;
                sheetSource = workbook.Worksheets[1];

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;

                #region ------------------Column Header------------------

                int colBudgetCodeId = 0; int colBudgetCode = 0; int colEntity = 0; int colDpt = 0; int colSec = 0; int colSSec = 0; int colDeg = 0;
                int colAct = 0; int colShift, colDeployment, colEmployeeCategory, colPositionCode, colResponsiblePerson = 0; int colDailyOTLimit = 0; int colWeeklyOTLimit = 0; int colWeekOffOTLimit, colONRoll = 0;
                int colMonthlyOTLimit = 0; int colRemarks, colROBudgetCode, colPRBudgetCode, colAttendanceGroup, colUserGroup2, colDirect = 0;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "BudgetCodeId"); colBudgetCodeId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Deployment"); colDeployment = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "BudgetCode"); colBudgetCode = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PositionCode"); colPositionCode = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeCat"); colEmployeeCategory = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Entity"); colEntity = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Department"); colDpt = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Section"); colSec = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SubSection"); colSSec = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Designation"); colDeg = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Activity"); colAct = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Shift"); colShift = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ONRoll"); colONRoll = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ResponsiblePerson"); colResponsiblePerson = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ROBudgetCode"); colROBudgetCode = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PRBudgetCode"); colPRBudgetCode = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "AttendanceGroup"); colAttendanceGroup = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "UserGroup2"); colUserGroup2 = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Direct"); colDirect = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DailyOTLimit"); colDailyOTLimit = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "WeeklyOTLimit"); colWeeklyOTLimit = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "WeekOffOTLimit"); colWeekOffOTLimit = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "MonthlyOTLimit"); colMonthlyOTLimit = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remarks"); colRemarks = xlsCol;

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;

                xlsRow++;

                #endregion ------------------Column Header------------------
                int count = 0;
                #region DataPlot
                DataTable dtData = oTControlLimitService.GetBudgetDataToUpload();
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    sheet1[xlsRow, colBudgetCodeId].Text = dtData.Rows[i]["BudgetCodeId"].ToString();
                    sheet1[xlsRow, colBudgetCode].Text = dtData.Rows[i]["BudgetCode"].ToString();
                    sheet1[xlsRow, colEntity].Text = dtData.Rows[i]["Entity"].ToString();
                    sheet1[xlsRow, colDpt].Text = dtData.Rows[i]["Department"].ToString();
                    sheet1[xlsRow, colDeg].Text = dtData.Rows[i]["Designation"].ToString();
                    sheet1[xlsRow, colSec].Text = dtData.Rows[i]["Section"].ToString();
                    sheet1[xlsRow, colSSec].Text = dtData.Rows[i]["SubSection"].ToString();
                    sheet1[xlsRow, colAct].Text = dtData.Rows[i]["Activity"].ToString();
                    sheet1[xlsRow, colShift].Text = dtData.Rows[i]["ShiftDefinationName"].ToString();
                    sheet1[xlsRow, colONRoll].Text = dtData.Rows[i]["ONRoll"].ToString();
                    sheet1[xlsRow, colDeployment].Text = dtData.Rows[i]["Deployment"].ToString();
                    sheet1[xlsRow, colPositionCode].Text = dtData.Rows[i]["PositionCode"].ToString();
                    sheet1[xlsRow, colEmployeeCategory].Text = dtData.Rows[i]["EmployeeCategory"].ToString();
                    sheet1[xlsRow, colResponsiblePerson].Text = dtData.Rows[i]["ResponsiblePerson"].ToString();
                    sheet1[xlsRow, colROBudgetCode].Text = dtData.Rows[i]["ROBudgetCode"].ToString();
                    sheet1[xlsRow, colPRBudgetCode].Text = dtData.Rows[i]["PRBudgetCode"].ToString();
                    sheet1[xlsRow, colAttendanceGroup].Text = dtData.Rows[i]["AttendanceGroup"].ToString();
                    sheet1[xlsRow, colUserGroup2].Text = dtData.Rows[i]["UserDefineGroup2"].ToString();
                    sheet1[xlsRow, colDirect].Text = dtData.Rows[i]["Direct"].ToString();

                    sheet1[xlsRow, colDailyOTLimit].Text = dtData.Rows[i]["DailyOTLimit"].ToString();
                    sheet1[xlsRow, colWeeklyOTLimit].Text = dtData.Rows[i]["WeeklyOTLimit"].ToString();
                    sheet1[xlsRow, colWeekOffOTLimit].Text = dtData.Rows[i]["WeekOffOTLimit"].ToString();
                    sheet1[xlsRow, colMonthlyOTLimit].Text = dtData.Rows[i]["MonthlyOTLimit"].ToString();
                    sheet1[xlsRow, colRemarks].Text = dtData.Rows[i]["Remarks"].ToString();

                    sheet1.Range[xlsRow, colDailyOTLimit, xlsRow, colMonthlyOTLimit].DataValidation.IsEmptyCellAllowed = true;
                    sheet1.Range[xlsRow, colDailyOTLimit, xlsRow, colMonthlyOTLimit].DataValidation.AllowType = ExcelDataType.Decimal;
                    sheet1.Range[xlsRow, colDailyOTLimit, xlsRow, colMonthlyOTLimit].DataValidation.CompareOperator = ExcelDataValidationComparisonOperator.GreaterOrEqual;
                    sheet1.Range[xlsRow, colDailyOTLimit, xlsRow, colMonthlyOTLimit].DataValidation.FirstFormula = "0";
                    sheet1.Range[xlsRow, colDailyOTLimit, xlsRow, colMonthlyOTLimit].DataValidation.ErrorStyle = ExcelErrorStyle.Stop;
                    sheet1.Range[xlsRow, colDailyOTLimit, xlsRow, colMonthlyOTLimit].DataValidation.ErrorBoxText = "Only positive decimal/numbers are allowed for Length";
                    sheet1.Range[xlsRow, colDailyOTLimit, xlsRow, colMonthlyOTLimit].DataValidation.ErrorBoxTitle = "Number Error";
                    sheet1.Range[xlsRow, colDailyOTLimit, xlsRow, colRemarks].CellStyle.Locked = false;

                    xlsRow++;
                }

                xlsRow++;

                #endregion

                #region UsedRange Alignment

                sheet1.Protect(bplib.clsWebLib.REPORT_LOCK_PASSWORD, ExcelSheetProtection.Filtering | ExcelSheetProtection.All);
                workbook.Worksheets[1].Protect(bplib.clsWebLib.REPORT_LOCK_PASSWORD);
                workbook.Protect(false, true, bplib.clsWebLib.REPORT_LOCK_PASSWORD);

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

                //sheetSource.Protect("2020", ExcelSheetProtection.Content);


                #endregion  Lunch Out

                //return workbook;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FabricRollManage" + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        [HttpPost, Authorize]
        public JsonResult ImportData()
        {
            string path;
            clsTemplateReadProfile objR = null;
            try
            {
                objR = new clsTemplateReadProfile();
                var file = Request.Files["file"];
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                SaveFiles(out path);
                var data = ReadData(identity.PlantId, path);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        public void SaveFiles(out string path)
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
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<OTControlLimitDetail> ReadData(string plantid, string path)
        {
            List<OTControlLimitDetail> data = null;
            //string path = "";
            DataSet dsExcel = null;
            try
            {
                data = new List<OTControlLimitDetail>();
                ReadFile(path, out dsExcel);
                Validation(dsExcel, plantid);
                data = dsExcel.Tables[0].ToList<OTControlLimitDetail>();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Validation(DataSet dsExcel, string plantid)
        {

            try
            {

                if (dsExcel.Tables[0].Rows.Count > 0)
                {
                        for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                        {
                            string strTempPDate = "";
                            string strTempPTimee = "";
                            string strTempPType = "";

                            strTempPDate = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
                            strTempPTimee = dsExcel.Tables[0].Rows[i][2].ToString().Trim();
                            strTempPType = dsExcel.Tables[0].Rows[i][3].ToString().Trim().ToUpper();

                        }//for

                }
                else
                {
                    throw new Exception("Please Select File");
                }
            }
            catch (Exception ex)
            {
                throw;
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
                DataTable dt = workbook.Worksheets[0].ExportDataTable(1, 1, 5000, 24, ExcelExportDataTableOptions.ColumnNames);
                dt.DefaultView.RowFilter = "isnull(BudgetCodeId,'')<>''";
                dt = dt.DefaultView.ToTable();
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

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, List<Dictionary<string, object>> detailList)
        {
            oTControlLimitService.SaveData(data, detailList);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            return Json(oTControlLimitService.GetList(), JsonRequestBehavior.AllowGet);
        }
        #endregion -- Operations
    }

    public class OTControlLimitDetail
    {

        public string BudgetCode { get; set; }
        public string Deployment { get; set; }
        public string PositionCode { get; set; }
        public string EmployeeCat{ get; set; }
        public string BudgetCodeId { get; set; }
        public string Entity { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string SubSection { get; set; }
        public string Designation { get; set; }
        public string Activity { get; set; }
        public string Shift { get; set; }
        public string DailyOTLimit { get; set; }
        public string WeeklyOTLimit { get; set; }
        public string WeekOffOTLimit { get; set; }
        public string MonthlyOTLimit { get; set; }
        public string Remarks { get; set; }
        public string ROBudgetCode { get; set; }
        public string PRBudgetCode { get; set; }
        public string AttendanceGroup { get; set; }
        public string ResponsiblePerson { get; set; }
        public string UserGroup2 { get; set; }
        public string Direct { get; set; }
        public string ONRoll { get; set; }

    }
}