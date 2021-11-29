using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Attendances;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Data;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class LeavesChecklistReportNewController : BaseController
    {
        #region Constructor

        private readonly IAttdnProcessDataService _AttendanceProcessDataService;
        private readonly ISqlRepository _sqlRepository;
        public LeavesChecklistReportNewController(
              IAttdnProcessDataService workGroupService, ISqlRepository sqlRepository
            )
        {
            _AttendanceProcessDataService = workGroupService; _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages
        
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion -- Pages

        #region Function
        [HttpGet, Authorize]
        public JsonResult GetPlantList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var str = @"select Id PlantId,UserName PlantName  from ORG.PLANT where CompanyId='" + identity.CompanyId + "'";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetLeaveType()
        {
            var str = @"SELECT lt.Id AS LeaveTypeId, lt.UserName AS LeaveType FROM LeaveType AS lt";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetleavesChecklistReport(ReportFormat reportFormat, string FromDate, string ToDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = GetleavesChecklistReport(identity.Name, identity.PlantId, identity.CompanyId, identity.CompanyGroupId, identity.PlantName, FromDate, ToDate);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "leaves Checklist Report";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName, false);
                    case ReportFormat.PdfView:
                        return RenderReportAsPdf(workbook, reportFileName);
                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }
        public IWorkbook GetleavesChecklistReport(string username, string plantId, string companyId, string companyGroupId, string plantName, string FromDate, string ToDate)
        {

            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsLeavesCheckList = null;
            DataTable dtLeavesCheckList = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string FactoryAddress = string.Empty;
            string OTConsiderOn = string.Empty;
            #endregion
            try
            {
                objStatic.GetPlantWiseHRMSSetting(companyGroupId, plantId, out dsLocalHRMSSetting);
                if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
                {
                    OTConsiderOn = dsLocalHRMSSetting.Tables[0].Rows[0]["OTConsiderOn"].ToString().Trim();
                }
                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                #region Validation
                if (string.IsNullOrEmpty(FromDate) == true || bplib.clsWebLib.IsDateOK(ToDate) == false)
                {
                    Exception ex = new Exception("Please define access Date..! (allowed format is  dd-MMM-yyyy ex: '01-jan-2008')...");
                    throw (ex);
                }
                #endregion Validation
                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                objRpt.GetLeaveschecklistReport(FromDate, ToDate, plantId, companyId, companyGroupId, out dsLeavesCheckList);
                dtLeavesCheckList = dsLeavesCheckList.Tables[0];

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);
                objRpt.SelectedPlant(plantId, out dsFactory);
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                var iFatHusName = 0;
                var iName = 0;
                var iEmployeeCode = 0;
                var iDesignation = 0;
                var iTO = 0;
                var iDOJ = 0;
                var iLeave = 0;
                var iFrom = 0;
                var iDays = 0;
                var iDateAdded = 0;
                var iDepartment = 0;
                var isl = 0;
                var SLNo = 1;
                //CountSheet(out totalSheet, dtLeavesCheckList);
                workbook = application.Workbooks.Create(1);
                #region ManualOutTime
                if (dtLeavesCheckList.Rows.Count == 0)
                {
                    Exception ex = new Exception("No Data Found....");
                    throw (ex);
                }

                if (dtLeavesCheckList.Rows.Count > 0)
                {
                    IWorksheet sheet1 = null;

                    sheet1 = workbook.Worksheets[0];
                    xlsRow = 6;

                    #region ------------------Column Header------------------
                    isl = xlsCol;
                    sheet1.Range[xlsRow, isl].Text = "SL";
                    sheet1.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet1.Range[xlsRow, iEmployeeCode].Text = "Emp Code";
                    sheet1.Range[xlsRow, iEmployeeCode].ColumnWidth = 10;

                    xlsCol += 1;
                    iName = xlsCol;
                    sheet1.Range[xlsRow, iName].Text = "Name";
                    sheet1.Range[xlsRow, iName].ColumnWidth = 18;

                    xlsCol += 1;
                    iFatHusName = xlsCol;
                    sheet1.Range[xlsRow, iFatHusName].Text = "Fat/Hus Name";
                    sheet1.Range[xlsRow, iFatHusName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet1.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet1.Range[xlsRow, iDesignation].ColumnWidth = 20;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet1.Range[xlsRow, iDepartment].Text = "Department";
                    sheet1.Range[xlsRow, iDepartment].ColumnWidth = 18;

                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet1.Range[xlsRow, iDOJ].ColumnWidth = 14;

                    xlsCol += 1;
                    iLeave = xlsCol;
                    sheet1.Range[xlsRow, iLeave].Text = "Leave";
                    sheet1.Range[xlsRow, iLeave].ColumnWidth = 10;

                    xlsCol += 1;
                    iFrom = xlsCol;
                    sheet1.Range[xlsRow, iFrom].Text = "From";
                    sheet1.Range[xlsRow, iFrom].ColumnWidth = 12;

                    xlsCol += 1;
                    iTO = xlsCol;
                    sheet1.Range[xlsRow, iTO].Text = "To";
                    sheet1.Range[xlsRow, iTO].ColumnWidth = 12;

                    xlsCol += 1;
                    iDays = xlsCol;
                    sheet1.Range[xlsRow, iDays].Text = "Days";
                    sheet1.Range[xlsRow, iDays].ColumnWidth = 11;

                    xlsCol += 1;
                    iDateAdded = xlsCol;
                    sheet1.Range[xlsRow, iDateAdded].Text = "Added Date";
                    sheet1.Range[xlsRow, iDateAdded].ColumnWidth = 11;

                    endXlsCol = xlsCol;

                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Size = 11;


                    string employeeid = "";


                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;

                    xlsRow++;

                    #endregion ------------------Column Header------------------


                    for (int i = 0; i < dtLeavesCheckList.Rows.Count; i++)
                    {
                        #region ----------------------Data-----------------------

                        if (employeeid != dtLeavesCheckList.Rows[i]["EmpSystemId"].ToString())
                        {
                            sheet1.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet1.Range[xlsRow, iEmployeeCode].Text = dtLeavesCheckList.Rows[i]["EmployeeCode"].ToString();
                            sheet1.Range[xlsRow, iName].Text = dtLeavesCheckList.Rows[i]["EmployeeName"].ToString();
                            sheet1.Range[xlsRow, iFatHusName].Text = dtLeavesCheckList.Rows[i]["FatherName"].ToString();
                            sheet1.Range[xlsRow, iDesignation].Text = dtLeavesCheckList.Rows[i]["designation"].ToString();
                            sheet1.Range[xlsRow, iDepartment].Text = dtLeavesCheckList.Rows[i]["Department"].ToString();
                            sheet1.Range[xlsRow, iDOJ].Text = dtLeavesCheckList.Rows[i]["DOJ"].ToString();
                        }
                        employeeid = dtLeavesCheckList.Rows[i]["EmpSystemId"].ToString();


                        sheet1.Range[xlsRow, iLeave].Text = dtLeavesCheckList.Rows[i]["Code"].ToString();
                        sheet1.Range[xlsRow, iFrom].Text = dtLeavesCheckList.Rows[i]["FromDate"].ToString();
                        sheet1.Range[xlsRow, iTO].Text = dtLeavesCheckList.Rows[i]["ToDate"].ToString();
                        sheet1.Range[xlsRow, iDays].Text = dtLeavesCheckList.Rows[i]["LeaveDays"].ToString();
                        sheet1.Range[xlsRow, iDateAdded].Text = dtLeavesCheckList.Rows[i]["DateAdded"].ToString();
                        xlsRow++;
                        SLNo++;
                    }
                    sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                    #endregion ----------------------Data-----------------------

                    #region ******************Report Header******************
                    xlsRow = 1;
                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 17;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet1.Range[xlsRow, xlsCol].Text = FactoryAddress;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 22;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "LEAVES CHECKLIST REPORT:" + FromDate + " To Date: " + ToDate;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet1.IsDisplayZeros = false;
                    sheet1.UsedRange["A7"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 10;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
                    sheet1.PageSetup.PrintTitleRows = "$1:$5";
                    sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet1.PageSetup.LeftMargin = 0.5;
                    sheet1.PageSetup.RightMargin = 0.2;
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet1.IsDisplayZeros = false;
                    sheet1.Name = "LEAVES CHECKLIST REPORT";
                    #endregion Page Setup
                }
                #endregion  ManualOutTime

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}