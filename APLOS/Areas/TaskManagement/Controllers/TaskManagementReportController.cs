#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.General.TaskScheduler;
using Library.Model.Enums;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Setups;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.TaskManagement.Controllers
{
    public class TaskManagementReportController : BaseController
    {


        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        TasksService tasksService = new TasksService();
        public TaskManagementReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }


        [HttpGet, Authorize]
        public ActionResult getFiltersData(string fromDate, string todate)
        {
            try
            {
                var sql = @"SELECT distinct TA.ResponsiblePersonId,DG.Id DesignationGroupId,DG.UserName DesignationGroup,DP.Id DepartmentId,DP.UserName Department,E.Id EntityId,E.UserName Entity,p.UserReportGroup, TYPE='Issue'
  FROM TaskAudit TA
LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=TA.ResponsiblePersonId
LEFT JOIN HKP.DesignationGroup AS DG ON DG.Id=ei.DesignationGroupId
LEFT JOIN ORG.Department AS DP ON DP.Id=ei.DepartmentId
LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=ei.BudgetCode
LEFT JOIN ORG.Entity AS e ON e.Id=mb.EntityId
LEFT JOIN ORG.Position p ON p.Id=mb.PositionId 
WHERE ei.EmployeeStatus='Active' AND (Convert(date,TA.DueDate) Between Convert(date,'" + fromDate + @"') AND Convert(date, '" + todate + @"'))
UNION
SELECT distinct TA.ResponsiblePersonId,DG.Id DesignationGroupId,DG.UserName DesignationGroup,DP.Id DepartmentId,DP.UserName Department,E.Id EntityId,E.UserName Entity,p.UserReportGroup, TYPE='TNA'
  FROM TaskAudit TA
LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=TA.ResponsiblePersonId
LEFT JOIN HKP.DesignationGroup AS DG ON DG.Id=ei.DesignationGroupId
LEFT JOIN ORG.Department AS DP ON DP.Id=ei.DepartmentId
LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=ei.BudgetCode
LEFT JOIN ORG.Entity AS e ON e.Id=mb.EntityId
LEFT JOIN ORG.Position p ON p.Id=mb.PositionId 
WHERE ei.EmployeeStatus='Active' AND (Convert(date,TA.DueDate) Between Convert(date,'" + fromDate + @"') AND Convert(date, '" + todate + @"'))
UNION
SELECT distinct TA.ResponsiblePersonId,DG.Id DesignationGroupId,DG.UserName DesignationGroup,DP.Id DepartmentId,DP.UserName Department,E.Id EntityId,E.UserName Entity,p.UserReportGroup, TYPE='To Do'
  FROM TaskAudit TA
LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=TA.ResponsiblePersonId
LEFT JOIN HKP.DesignationGroup AS DG ON DG.Id=ei.DesignationGroupId
LEFT JOIN ORG.Department AS DP ON DP.Id=ei.DepartmentId
LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=ei.BudgetCode
LEFT JOIN ORG.Entity AS e ON e.Id=mb.EntityId
LEFT JOIN ORG.Position p ON p.Id=mb.PositionId 
WHERE ei.EmployeeStatus='Active' AND (Convert(date,TA.DueDate) Between Convert(date,'" + fromDate + @"') AND Convert(date, '" + todate + @"'))";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetTaskManagementReport(Dictionary<string, string> parameters, string fromDate, string todate)
        {

            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string fileName = "";
                fileName = GetTaskManagementReportXL(parameters, fromDate,todate,identity.CompanyGroupId, identity.CompanyId, identity.PlantId, "Task");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


        }

        public string GetTaskManagementReportXL(Dictionary<string, string> parameters, string fromDate, string todate, string CompanyGroupId, string CompanyId, string PlantId, string SheetName)
        {
            clsReport objRpt = null;
            clsReport objRptSR = null;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                var reportUtility = new ReportUtility();
                workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Worksheets[0].Name = SheetName;
                sheet = workbook.Worksheets[0];
                DataTable data;


                //Add rich-text Excel comment
                IFont fontCaption = workbook.CreateFont();
                fontCaption.Size = 8f;
                IFont fontRegular = workbook.CreateFont();
                fontRegular.Italic = true;
                fontRegular.Size = 6f;

                // ExcelEngine excelEngine = null;

                workbook.Version = ExcelVersion.Excel2013;
                var sheet1 = workbook.Worksheets[0];

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompanyImage = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE ID = '" + CompanyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyImage.Rows[0]["Image"].ToString());
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                DataTable dtTask = null;
                dtTask = tasksService.GetTaskManagementData(fromDate, todate, CompanyGroupId, CompanyId, PlantId, parameters);
                if (dtTask.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }

                DataTable dtCmp = objRptSR.SelectedCompanyDT(PlantId);

                DataTable dtFactory = objRptSR.SelectedPlantDT(PlantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                xlsRow = 6;

                sheet1[xlsRow, xlsCol].Text = "SL. No";
                int colSLNO = xlsCol;
                sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                xlsCol++;

                int iEmployeeCode = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Employee Code";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                xlsCol++;

                int iEmployeeName = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Employee Name";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                xlsCol++;

                int iDesignation = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Designation";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                xlsCol++;

                int colDepartment = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Department";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                //xlsCol++;

                xlsCol++;
                int iTaskCreated = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Created";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;

                xlsCol++;
                int iPerTotalTask = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "% Of Total Task";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;


                xlsCol++;
                int iTaskUnread = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Unread";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;

                xlsCol++;

                int iTaskDue = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Due";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;

                xlsCol++;

                int iPerOfDueTask = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "% Of Due Task";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;

                xlsCol++;

                int iTaskCompletedOnTime = xlsCol;
                sheet1[xlsRow, xlsCol].Text = "Task Completed-On Time";
                sheet1[xlsRow, xlsCol].ColumnWidth = 20;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;


                xlsCol++;
                int iTaskCompletedLate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Task Completed-Late";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 16;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;

                xlsCol++;

                int iOverdueTask = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Overdue Task ";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;


                xlsCol++;
                int iPeriviousPeriodOverdueTask = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Perivious Period Overdue Task";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 24;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;


                xlsCol++;
                int iPerformance = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Performance";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;

                xlsCol++;
                int iTotalStoryPoints = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Total Story Points";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;

                xlsCol++;
                int iCompletedStoryPoints = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Completed Story Points";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 18.5;
                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                string voucherNo = "";
                /// string Percentage = "";
                int startRow = 0;
                int perStartRow = 0;
                string formula = "";
                string formula2 = "";
                // string totalFormula = "";

                //string lineItemPercentageType = "";
                xlsRow++;
                startRow = xlsRow;
                perStartRow = xlsRow;

                double TotalCreatedTask = clsStaticInfo.dbl(dtTask.Compute("SUM(CreatedTask)", null));
                double TotalTaskDue = clsStaticInfo.dbl(dtTask.Compute("SUM(TaskDue)", null));

                for (int i = 0; i < dtTask.Rows.Count; i++)
                {
                    sheet1[xlsRow, colSLNO].Number = (i + 1);
                    sheet1.Range[xlsRow, iEmployeeCode].Text = dtTask.Rows[i]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, iEmployeeName].Text = dtTask.Rows[i]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, iDesignation].Text = dtTask.Rows[i]["Designation"].ToString();
                    sheet1.Range[xlsRow, colDepartment].Text = dtTask.Rows[i]["Department"].ToString();
                    sheet1.Range[xlsRow, iTaskCreated].Number = clsStaticInfo.dbl(dtTask.Rows[i]["CreatedTask"].ToString());

                    double PerTotalTask =Math.Round((Convert.ToDouble(dtTask.Rows[i]["CreatedTask"].ToString()) / TotalCreatedTask)*100);
                    sheet1.Range[xlsRow, iPerTotalTask].Number = PerTotalTask;//formula 
                    sheet1.Range[xlsRow, iPerTotalTask].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    IRange range = sheet1[xlsRow, iPerTotalTask];
                    ICommentShape shape = range.AddComment();
                    shape.RichText.Append("Emp Task Created FP / Total Task Created  FP", fontCaption);
                    shape.IsTextLocked = false;
                    shape.AutoSize = false;


                    sheet1.Range[xlsRow, iTaskUnread].Number = clsStaticInfo.dbl(dtTask.Rows[i]["UnRead"].ToString());
                    sheet1.Range[xlsRow, iTaskDue].Number = clsStaticInfo.dbl(dtTask.Rows[i]["TaskDue"].ToString());

                    double PerTaskDue = Math.Round((Convert.ToDouble(dtTask.Rows[i]["TaskDue"].ToString()) / TotalTaskDue) * 100);
                    sheet1.Range[xlsRow, iPerOfDueTask].Number = PerTaskDue;//formula 
                    sheet1.Range[xlsRow, iPerOfDueTask].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    IRange range1 = sheet1[xlsRow, iPerOfDueTask];
                    ICommentShape shape1 = range1.AddComment();
                    shape1.RichText.Append("Emp Due Task FP / Total Due Task FP", fontCaption);
                    shape1.IsTextLocked = false;
                    shape1.AutoSize = false;


                    sheet1.Range[xlsRow, iTaskCompletedOnTime].Number = clsStaticInfo.dbl(dtTask.Rows[i]["OnTimeTask"].ToString());
                    sheet1.Range[xlsRow, iTaskCompletedLate].Number = clsStaticInfo.dbl(dtTask.Rows[i]["LateTask"].ToString());

                    sheet1[xlsRow, iOverdueTask].Number = Convert.ToDouble(dtTask.Rows[i]["TaskDue"].ToString())- Convert.ToDouble(dtTask.Rows[i]["OnTimeTask"].ToString())- Convert.ToDouble(dtTask.Rows[i]["LateTask"].ToString());//formula
                    

                    sheet1[xlsRow, iPeriviousPeriodOverdueTask].Number = clsStaticInfo.dbl(dtTask.Rows[i]["PeriviousPeriodOverdueTask"].ToString());

                    sheet1[xlsRow, iPerformance].Number = (((Convert.ToDouble(dtTask.Rows[i]["OnTimeTask"].ToString())*2)+(Convert.ToDouble(dtTask.Rows[i]["LateTask"].ToString())*1)* PerTaskDue)- (Convert.ToDouble(dtTask.Rows[i]["UnRead"].ToString())));//formula
                    IRange range2 = sheet1[xlsRow, iPerformance];
                    ICommentShape shape2 = range2.AddComment();
                    shape2.RichText.Append("(((Task Completed On Time*2)+(Task Completed Late*1))* % of Due task)-Task Unread", fontCaption);
                    shape2.IsTextLocked = false;
                    shape2.AutoSize = false;


                    sheet1[xlsRow, iTotalStoryPoints].Number = clsStaticInfo.dbl(dtTask.Rows[i]["TotalStoryPoint"].ToString());
                    sheet1[xlsRow, iCompletedStoryPoints].Number = clsStaticInfo.dbl(dtTask.Rows[i]["ColsedStoryPoint"].ToString());


                    xlsRow++;
                }
                sheet1[perStartRow, iTaskUnread, xlsRow - 1, iTaskUnread].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaskCompletedOnTime, xlsRow - 1, iTaskCompletedOnTime].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, colDepartment, xlsRow - 1, colDepartment].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iEmployeeName, xlsRow - 1, iEmployeeName].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iEmployeeCode, xlsRow - 1, iEmployeeCode].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iDesignation, xlsRow - 1, iDesignation].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTaskCompletedLate, xlsRow - 1, iTaskCompletedLate].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPerOfDueTask, xlsRow - 1, iPerOfDueTask].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iOverdueTask, xlsRow - 1, iOverdueTask].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iTotalStoryPoints, xlsRow - 1, iTotalStoryPoints].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iCompletedStoryPoints, xlsRow - 1, iCompletedStoryPoints].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPeriviousPeriodOverdueTask, xlsRow - 1, iPeriviousPeriodOverdueTask].BorderAround(ExcelLineStyle.Hair);
                sheet1[perStartRow, iPerformance, xlsRow - 1, iPerformance].BorderAround(ExcelLineStyle.Hair);


                formula = "SUM(" + clsStaticInfo.GetxlsCol(iTaskCreated) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTaskCreated) + (xlsRow - 1) + ")";
                sheet1[xlsRow, endXlsCol, xlsRow, endXlsCol].Formula = formula;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;


                //formula = "SUM(" + clsStaticInfo.GetxlsCol(iPeriviousPeriodOverdueTask) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iPeriviousPeriodOverdueTask) + (xlsRow - 1) + ")";
                //sheet1[xlsRow, iPeriviousPeriodOverdueTask, xlsRow, iPeriviousPeriodOverdueTask].Formula = formula;
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;

                //formula = "SUM(" + clsStaticInfo.GetxlsCol(iPerformance) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iPerformance) + (xlsRow - 1) + ")";
                //sheet1[xlsRow, iPerformance, xlsRow, iPerformance].Formula = formula;
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;

                //formula = "SUM(" + clsStaticInfo.GetxlsCol(iTotalStoryPoints) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iTotalStoryPoints) + (xlsRow - 1) + ")";
                //sheet1[xlsRow, iTotalStoryPoints, xlsRow, iTotalStoryPoints].Formula = formula;
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;

                //formula = "SUM(" + clsStaticInfo.GetxlsCol(iCompletedStoryPoints) + perStartRow + ":" + clsStaticInfo.GetxlsCol(iCompletedStoryPoints) + (xlsRow - 1) + ")";
                //sheet1[xlsRow, iCompletedStoryPoints, xlsRow, iCompletedStoryPoints].Formula = formula;
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;

                xlsRow++;
                xlsRow++;


                #region ******************Report Header******************

                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(endXlsCol);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                //sheet1.Range[xlsRow, 3].Text = "Aging Report From " + fromDate + " To " + toDate;
                sheet1.Range[xlsRow, 3].Text = "Task Management Report";
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + SheetName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                #endregion Page Setup


                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TaskManagementReport.xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }


    }
}