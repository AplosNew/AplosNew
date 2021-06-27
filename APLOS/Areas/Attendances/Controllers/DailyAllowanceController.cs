using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Attendances;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using Library.ViewModel.Organizations;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.Helpers.ReportUtility;

namespace Aplos.Areas.Attendances.Controllers
{
    public class DailyAllowanceController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMonthlyAttendanceInformation _monthlyAttendanceInformation;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private DataSet dsRef;
        private object workbook;
        private object objRpt;
        private object excelEngine;
        private object application;

        public DailyAllowanceController(
              IMaternityLeavePolicyService LeavePolicyService,
            ISqlRepository sqlRepository,
            IMonthlyAttendanceInformation monthlyAttendanceInformation
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
            _monthlyAttendanceInformation = monthlyAttendanceInformation;
        }

        #endregion Constructor

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        [Authorize]
        public ActionResult DailyAllowanceSummary()
        {
            return View();
        }
        #endregion -- Pages


        #region -- Operations

        public ActionResult GetEmpInfo(string fromDate, string toDate, string AllowanceDailyId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var cmdText = @"select format(dat.WorkDate,'dd-MMM-yyyy') as WorkDate,dat.Quantity,dat.Rate,dat.Amount,e.EmployeeCode,e.EmployeeName
                                ,ld.UserName as Designation, d.UserName as Department,se.UserName as Section,ss.UserName as SubSection,l.UserName AS Line
                                ,ad.UserName as AllowanceType,format(e.DOJ,'dd-MMM-yyyy')as DOJ,format(e.DOC,'dd-MMM-yyyy')as DOC,dat.EmpSystemId,format(e.DOS,'dd-MMM-yyyy')as DOS
                                ,eu.UserName AS Unit
                                from [dbo].[DailyAllowanceTransaction] as dat 
                                inner join EmployeeInformation as e on e.SystemId=dat.EmpSystemId
                                left join [HKP].[AllowanceDaily] as ad on ad.Id=dat.AllowanceDailyId
                                LEFT JOIN MST.ManpowerBudget PMB ON e.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN ORG.Entity En ON PMB.EntityId=En.Id
                                left join [ORG].[Department] d on d.Id=PR.DepartmentId
                                left join [HKP].[LegalDesignation] ld on ld.Id=e.LegalDesignationId
                                left join [ORG].[Section] se on se.Id=PR.SectionId
                                left join [ORG].[SubSection] ss on ss.Id=PR.SectionId
                                left join [ORG].[Line] as l on l.Id = PMB.LineId
                                LEFT OUTER JOIN ORG.Unit eu on eu.id=EN.UnitId
                                where dat.WorkDate between '" + fromDate + @"' and '" + toDate + @"'
                                and e.PlantId='" + identity.PlantId + @"' and dat.AllowanceDailyId='" + AllowanceDailyId + @"' ";
                JsonResult json = Json(_sqlRepository.GetDataCollection(cmdText), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;

            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public ActionResult XlsAttendanceOnDayStatusReport(string fromDate, string toDate, string AllowanceDailyId, string Description, string[] employeeId, string ReportGroup)
        {
            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsAttendanceOnDayStatus = null;
            DataTable dtAttendanceOnDayStatus = null;
            DataTable dtDailyAllowanceSummary = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsDailyAllowanceSummary = null;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;
            #endregion

            try
            {

                string EmpIdLoop = "";
                foreach (string item in employeeId)
                {
                    if (EmpIdLoop == "")
                    {
                        EmpIdLoop = "'" + item + "'"; ;
                    }
                    else
                    {
                        EmpIdLoop += ",'" + item + "'";
                    }
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 2);
                workbook.Version = ExcelVersion.Excel2013;
                //workbook = application.Workbooks.Create(2);
                objRpt = new clsReport();
                objRpt.GetDailyAllowance(fromDate, toDate, AllowanceDailyId, EmpIdLoop, out dsAttendanceOnDayStatus);
                dtAttendanceOnDayStatus = dsAttendanceOnDayStatus.Tables[0];

                objRpt.GetDailyAllowanceSummary(fromDate, toDate, AllowanceDailyId, EmpIdLoop, out dsDailyAllowanceSummary);
                dtDailyAllowanceSummary = dsDailyAllowanceSummary.Tables[0];

                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                string companyId = identity.CompanyId;
                var iName = 0;
                var iEmployeeCode = 0;
                var iSubSection = 0;
                var iSection = 0;
                var iDOJ = 0;
                var iDOS = 0;
                var iLine = 0;
                var iDepartment = 0;
                var iDesignation = 0;
                var iAmount = 0;
                var iWorkDate = 0;
                var iQuantity = 0;
                var iRate = 0;
                var iUnit = 0;
                var iAllowanceType = 0;
                var isl = 0;
                var SLNo = 1;

                if (dsAttendanceOnDayStatus.Tables[0].Rows.Count == 0)
                {
                    throw new CustomException("No Data Found....");
                }
                #region Allowance Daily 

                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];
                xlsRow = 5;

                #region ------------------Column Header------------------
                isl = xlsCol;
                sheet1.Range[xlsRow, isl].Text = "SL";
                sheet1.Range[xlsRow, isl].ColumnWidth = 7;

                xlsCol += 1;
                iWorkDate = xlsCol;
                sheet1.Range[xlsRow, iWorkDate].Text = "Work Date";
                sheet1.Range[xlsRow, iWorkDate].ColumnWidth = 13;

                xlsCol += 1;
                iEmployeeCode = xlsCol;
                sheet1.Range[xlsRow, iEmployeeCode].Text = "Employee Code";
                sheet1.Range[xlsRow, iEmployeeCode].ColumnWidth = 10;

                xlsCol += 1;
                iName = xlsCol;
                sheet1.Range[xlsRow, iName].Text = "Employee Name";
                sheet1.Range[xlsRow, iName].ColumnWidth = 25;


                xlsCol += 1;
                iDepartment = xlsCol;
                sheet1.Range[xlsRow, iDepartment].Text = "Department";
                sheet1.Range[xlsRow, iDepartment].ColumnWidth = 25;

                xlsCol += 1;
                iDesignation = xlsCol;
                sheet1.Range[xlsRow, iDesignation].Text = "Designation";
                sheet1.Range[xlsRow, iDesignation].ColumnWidth = 25;

                xlsCol += 1;
                iSection = xlsCol;
                sheet1.Range[xlsRow, iSection].Text = "Section";
                sheet1.Range[xlsRow, iSection].ColumnWidth = 14;

                xlsCol += 1;
                iSubSection = xlsCol;
                sheet1.Range[xlsRow, iSubSection].Text = "Sub Section";
                sheet1.Range[xlsRow, iSubSection].ColumnWidth = 16;

                xlsCol += 1;
                iUnit = xlsCol;
                sheet1.Range[xlsRow, iUnit].Text = "Unit";
                sheet1.Range[xlsRow, iUnit].ColumnWidth = 16;

                xlsCol += 1;
                iLine = xlsCol;
                sheet1.Range[xlsRow, iLine].Text = "Line";
                sheet1.Range[xlsRow, iLine].ColumnWidth = 10;

                xlsCol += 1;
                iDOJ = xlsCol;
                sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                sheet1.Range[xlsRow, iDOJ].ColumnWidth = 11;

                xlsCol += 1;
                iDOS = xlsCol;
                sheet1.Range[xlsRow, iDOS].Text = "DOS";
                sheet1.Range[xlsRow, iDOS].ColumnWidth = 11;

                xlsCol += 1;
                iQuantity = xlsCol;
                sheet1.Range[xlsRow, iQuantity].Text = "Quantity";
                sheet1.Range[xlsRow, iQuantity].ColumnWidth = 9;

                xlsCol += 1;
                iRate = xlsCol;
                sheet1.Range[xlsRow, iRate].Text = "Rate";
                sheet1.Range[xlsRow, iRate].ColumnWidth = 7;

                xlsCol += 1;
                iAmount = xlsCol;
                sheet1.Range[xlsRow, iAmount].Text = "Amount";
                sheet1.Range[xlsRow, iAmount].ColumnWidth = 8;

                //xlsCol += 1;
                //iAllowanceType = xlsCol;
                //sheet1.Range[xlsRow, iAllowanceType].Text = "Allowance Type";
                //sheet1.Range[xlsRow, iAllowanceType].ColumnWidth = 12;

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                xlsRow++;

                #endregion ------------------Column Header------------------

                for (int i = 0; i < dtAttendanceOnDayStatus.Rows.Count; i++)
                {
                    #region ----------------------Data-----------------------   

                    sheet1.Range[xlsRow, isl].Text = SLNo.ToString();
                    sheet1.Range[xlsRow, iName].Text = dtAttendanceOnDayStatus.Rows[i]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, iEmployeeCode].Text = dtAttendanceOnDayStatus.Rows[i]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, iDesignation].Text = dtAttendanceOnDayStatus.Rows[i]["Designation"].ToString();
                    sheet1.Range[xlsRow, iDepartment].Text = dtAttendanceOnDayStatus.Rows[i]["Department"].ToString();
                    sheet1.Range[xlsRow, iSection].Text = dtAttendanceOnDayStatus.Rows[i]["Section"].ToString();
                    sheet1.Range[xlsRow, iSubSection].Text = dtAttendanceOnDayStatus.Rows[i]["SubSection"].ToString();
                    sheet1.Range[xlsRow, iLine].Text = dtAttendanceOnDayStatus.Rows[i]["Line"].ToString();
                    sheet1.Range[xlsRow, iDOJ].Text = dtAttendanceOnDayStatus.Rows[i]["DOJ"].ToString();
                    sheet1.Range[xlsRow, iDOS].Text = dtAttendanceOnDayStatus.Rows[i]["DOS"].ToString();
                    sheet1.Range[xlsRow, iQuantity].Text = dtAttendanceOnDayStatus.Rows[i]["Quantity"].ToString();
                    sheet1.Range[xlsRow, iRate].Text = dtAttendanceOnDayStatus.Rows[i]["Rate"].ToString();
                    sheet1.Range[xlsRow, iAmount].Text = dtAttendanceOnDayStatus.Rows[i]["Amount"].ToString();
                    sheet1.Range[xlsRow, iWorkDate].Text = dtAttendanceOnDayStatus.Rows[i]["WorkDate"].ToString();
                    sheet1.Range[xlsRow, iUnit].Text = dtAttendanceOnDayStatus.Rows[i]["Unit"].ToString();
                    //sheet1.Range[xlsRow, iAllowanceType].Text = dtAttendanceOnDayStatus.Rows[i]["AllowanceType"].ToString();

                    xlsRow++;
                    SLNo++;
                }

                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                #endregion ----------------------Data-----------------------

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A6"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region ******************Report Header******************
                try
                {
                    string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                    Image companyLogo = Image.FromFile(strPath);
                    if (companyLogo != null)
                    {
                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(2);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);


                    }


                }
                catch (Exception)
                {


                }
                xlsRow = 1;
                xlsCol = 1;

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 16;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;

                sheet1.Range[xlsRow, 3].Text = "Daily Allowance - From : " + fromDate + " To " + toDate + " " + "Allowance Type: " + dtAttendanceOnDayStatus.Rows[0]["AllowanceType"].ToString() + " " + ReportGroup;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

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
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Daily Allowance";
                #endregion Page Setup


                IWorksheet sheet2 = null;

                sheet2 = workbook.Worksheets[1];
                xlsRow = 5;

                #region ------------------Column Header------------------
                isl = xlsCol;
                sheet2.Range[xlsRow, isl].Text = "SL";
                sheet2.Range[xlsRow, isl].ColumnWidth = 7;

                xlsCol += 1;
                iEmployeeCode = xlsCol;
                sheet2.Range[xlsRow, iEmployeeCode].Text = "Employee Code";
                sheet2.Range[xlsRow, iEmployeeCode].ColumnWidth = 10;

                xlsCol += 1;
                iName = xlsCol;
                sheet2.Range[xlsRow, iName].Text = "Employee Name";
                sheet2.Range[xlsRow, iName].ColumnWidth = 25;


                xlsCol += 1;
                iDepartment = xlsCol;
                sheet2.Range[xlsRow, iDepartment].Text = "Department";
                sheet2.Range[xlsRow, iDepartment].ColumnWidth = 25;

                xlsCol += 1;
                iDesignation = xlsCol;
                sheet2.Range[xlsRow, iDesignation].Text = "Designation";
                sheet2.Range[xlsRow, iDesignation].ColumnWidth = 25;

                xlsCol += 1;
                iSection = xlsCol;
                sheet2.Range[xlsRow, iSection].Text = "Section";
                sheet2.Range[xlsRow, iSection].ColumnWidth = 14;

                xlsCol += 1;
                iSubSection = xlsCol;
                sheet2.Range[xlsRow, iSubSection].Text = "Sub Section";
                sheet2.Range[xlsRow, iSubSection].ColumnWidth = 16;

                xlsCol += 1;
                iUnit = xlsCol;
                sheet2.Range[xlsRow, iUnit].Text = "Unit";
                sheet2.Range[xlsRow, iUnit].ColumnWidth = 16;

                xlsCol += 1;
                iLine = xlsCol;
                sheet2.Range[xlsRow, iLine].Text = "Line";
                sheet2.Range[xlsRow, iLine].ColumnWidth = 10;

                xlsCol += 1;
                iDOJ = xlsCol;
                sheet2.Range[xlsRow, iDOJ].Text = "DOJ";
                sheet2.Range[xlsRow, iDOJ].ColumnWidth = 11;

                xlsCol += 1;
                iDOS = xlsCol;
                sheet2.Range[xlsRow, iDOS].Text = "DOS";
                sheet2.Range[xlsRow, iDOS].ColumnWidth = 11;

                xlsCol += 1;
                iQuantity = xlsCol;
                sheet2.Range[xlsRow, iQuantity].Text = "Quantity";
                sheet2.Range[xlsRow, iQuantity].ColumnWidth = 9;

                //xlsCol += 1;
                //iRate = xlsCol;
                //sheet2.Range[xlsRow, iRate].Text = "Rate";
                //sheet2.Range[xlsRow, iRate].ColumnWidth = 7;

                xlsCol += 1;
                iAmount = xlsCol;
                sheet2.Range[xlsRow, iAmount].Text = "Amount";
                sheet2.Range[xlsRow, iAmount].ColumnWidth = 8;

                //xlsCol += 1;
                //iAllowanceType = xlsCol;
                //sheet2.Range[xlsRow, iAllowanceType].Text = "Allowance Type";
                //sheet2.Range[xlsRow, iAllowanceType].ColumnWidth = 12;

                endXlsCol = xlsCol;

                sheet2.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet2.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet2.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet2.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet2.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet2.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                xlsRow++;

                #endregion ------------------Column Header------------------

                for (int i = 0; i < dtDailyAllowanceSummary.Rows.Count; i++)
                {
                    #region ----------------------Data-----------------------   

                    sheet2.Range[xlsRow, isl].Text = SLNo.ToString();
                    sheet2.Range[xlsRow, iName].Text = dtDailyAllowanceSummary.Rows[i]["EmployeeName"].ToString();
                    sheet2.Range[xlsRow, iEmployeeCode].Text = dtDailyAllowanceSummary.Rows[i]["EmployeeCode"].ToString();
                    sheet2.Range[xlsRow, iDesignation].Text = dtDailyAllowanceSummary.Rows[i]["Designation"].ToString();
                    sheet2.Range[xlsRow, iDepartment].Text = dtDailyAllowanceSummary.Rows[i]["Department"].ToString();
                    sheet2.Range[xlsRow, iSection].Text = dtDailyAllowanceSummary.Rows[i]["Section"].ToString();
                    sheet2.Range[xlsRow, iSubSection].Text = dtDailyAllowanceSummary.Rows[i]["SubSection"].ToString();
                    sheet2.Range[xlsRow, iLine].Text = dtDailyAllowanceSummary.Rows[i]["Line"].ToString();
                    sheet2.Range[xlsRow, iDOJ].Text = dtDailyAllowanceSummary.Rows[i]["DOJ"].ToString();
                    sheet2.Range[xlsRow, iDOS].Text = dtDailyAllowanceSummary.Rows[i]["DOS"].ToString();
                    sheet2.Range[xlsRow, iQuantity].Text = dtDailyAllowanceSummary.Rows[i]["Quantity"].ToString();
                    //sheet2.Range[xlsRow, iRate].Text = dtDailyAllowanceSummary.Rows[i]["Rate"].ToString();
                    sheet2.Range[xlsRow, iAmount].Text = dtDailyAllowanceSummary.Rows[i]["Amount"].ToString();
                    sheet2.Range[xlsRow, iUnit].Text = dtDailyAllowanceSummary.Rows[i]["Unit"].ToString();
                    //sheet2.Range[xlsRow, iAllowanceType].Text = dtDailyAllowanceSummary.Rows[i]["AllowanceType"].ToString();

                    xlsRow++;
                    SLNo++;
                }

                sheet2.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet2.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet2.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                #endregion ----------------------Data-----------------------

                #region Freeze Panes

                sheet2.IsDisplayZeros = false;
                sheet2.UsedRange["A6"].FreezePanes();
                sheet2.FirstVisibleColumn = 1;
                sheet2.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region ******************Report Header******************
                try
                {
                    string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                    Image companyLogo = Image.FromFile(strPath);
                    if (companyLogo != null)
                    {
                        double totalWidth = sheet2.GetColumnWidth(1) + sheet2.GetColumnWidth(2);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet2.GetRowHeight(1) + sheet2.GetRowHeight(2) + sheet2.GetRowHeight(3) + sheet2.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet2.Pictures.AddPicture(1, 1, companyLogo);


                    }


                }
                catch (Exception)
                {


                }
                xlsRow = 1;
                xlsCol = 1;

                FactoryName = string.Empty;

                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet2.Range[xlsRow, 3].Text = CmpName;
                sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet2.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet2.Range[xlsRow, 3].CellStyle.Font.Size = 16;
                sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet2.Range[xlsRow, 3].Text = FactoryName;
                sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet2.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet2.Range[xlsRow, 3].Text = FactoryAddress;
                sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet2.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;

                sheet2.Range[xlsRow, 3].Text = "Daily Allowance - From : " + fromDate + " To " + toDate + " " + "Allowance Type: " + dtAttendanceOnDayStatus.Rows[0]["AllowanceType"].ToString() + " " + ReportGroup;
                sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet2.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet2.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

                #region UsedRange Alignment

                sheet2.UsedRange.WrapText = true;
                sheet2.UsedRange.CellStyle.Font.Size = 10;
                sheet2.Range["A1"].CellStyle.Font.Size = 14;
                sheet2.Range["A2"].CellStyle.Font.Size = 10;
                sheet2.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet2.PageSetup.TopMargin = 0.5;
                sheet2.PageSetup.BottomMargin = 0.7;
                sheet2.PageSetup.PrintTitleRows = "$1:$5";
                sheet2.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet2.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet2.PageSetup.LeftMargin = 0.5;
                sheet2.PageSetup.RightMargin = 0.2;
                sheet2.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet2.PageSetup.FitToPagesTall = 0;
                sheet2.PageSetup.FitToPagesWide = 1;
                sheet2.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet2.IsDisplayZeros = false;
                sheet2.Name = "Daily Allowance Summary";
                #endregion Page Setup

                workbook.Version = ExcelVersion.Excel97to2003;
                var strFileName = "DailyAllowance.xls";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetDailyAllowanceList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select Id,UserName from  [HKP].[AllowanceDaily] where Active=1 and PlantId='"+ identity.PlantId + @"'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations   

        #region DailyAllowanceSummaryReport--------------------------

        public void GetAllowanceDailySummarySql(string fromDate, string toDate, out DataSet dsRef)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            clsStaticInfo obs = null;
            try
            {
                string wc = string.Empty;
                obs = new clsStaticInfo();
                strSql = @"select 
                            e.EmployeeCode,e.EmployeeName
                            ,ld.UserName as Designation, d.UserName as Department,se.UserName as Section,ss.UserName as SubSection,l.UserName AS Line
                            ,ad.UserName as AllowanceType,format(e.DOJ,'dd-MMM-yyyy')as DOJ,format(e.DOC,'dd-MMM-yyyy')as DOC
                            ,format(e.DOS,'dd-MMM-yyyy')as DOS  ,eu.UserName AS Unit
                            ,x.Quantity,x.Amount,x.AllowanceDailyId
                            ,ISNULL(Bank.UserName,'') BankName,ISNULL(EBI.BankAccNo,'') BankAccNo
                            from EmployeeInformation as e 
                            inner join (select sum(dat.Quantity) as Quantity,sum(dat.Amount) as Amount
							,dat.EmpSystemId,dat.AllowanceDailyId
							from [dbo].[DailyAllowanceTransaction] as dat 
							where dat.WorkDate between '" + fromDate + @"' and '" + toDate + @"'
							Group by  EmpSystemId,AllowanceDailyId) x on e.SystemId=x.EmpSystemId
                            left join [HKP].[AllowanceDaily] as ad on ad.Id=x.AllowanceDailyId
                            LEFT JOIN EmployeeBankInfo EBI ON E.SystemId = EBI.EmpSystemID	
                            LEFT JOIN HKP.Bank Bank ON Bank.Id = EBI.BankSystemID
                            LEFT JOIN MST.ManpowerBudget PMB ON e.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity En ON PMB.EntityId=En.Id
                            LEFT JOIN [ORG].[Department] d on d.Id=PR.DepartmentId
                            LEFT JOIN [HKP].[LegalDesignation] ld on ld.Id=e.LegalDesignationId
                            LEFT JOIN [ORG].[Section] se on se.Id=PR.SectionId
                            LEFT JOIN [ORG].[SubSection] ss on ss.Id=PR.SectionId
                            LEFT JOIN [ORG].[Line] as l on l.Id = PMB.LineId
                            LEFT OUTER JOIN ORG.Unit eu on eu.id=EN.UnitId
                            where
                             e.PlantId='" + identity.PlantId + @"' and ad.IsVoucherPayment=1
                                order by  EmployeeCodePreFix ,EmployeeCodeNumeric";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");


            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }

        }//End Function

        #region -----------------------------------Excel Report--------------------------------------------------

        public ActionResult Getdailyattendance(string fromDate, string toDate)//XlsDailyAttendanceSummaryRpt()
        {

            #region Variable
            clsReport objRpt = null;
            DataSet dsAllowanceDailySummary = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;
            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;

            #endregion Variable

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ru = new ReportUtility();
                objRpt = new clsReport();
                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();
                para.PlantId = identity.PlantId;
                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion Variable
                #region DataSet
                GetAllowanceDailySummarySql(fromDate, toDate, out dsAllowanceDailySummary);
                DataTable dtAllowanceDailySummary = dsAllowanceDailySummary.Tables[0].DefaultView.ToTable();
                if (dtAllowanceDailySummary.Rows.Count == 0)
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }
                DataView dvAllowanceDaily = new DataView(dsAllowanceDailySummary.Tables[0]);
                object totalQuantity = dvAllowanceDaily.ToTable().Compute(@"Sum(Quantity)", null);
                object totalAbsentDays = dvAllowanceDaily.ToTable().Compute(@"Sum(Amount)", null);

                #endregion DataSet
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                xlsRow = 7;
                xlsCol = 1;

                #region------------------Column Header------------------

                SetHeadText("EmployeeCode", sheet1, xlsRow, ref xlsCol, out int colEmployeeCode, 16);
                SetHeadText("EmployeeName", sheet1, xlsRow, ref xlsCol, out int colEmployeeName, 24);
                SetHeadText("Designation", sheet1, xlsRow, ref xlsCol, out int colDesignation, 16);
                SetHeadText("Department", sheet1, xlsRow, ref xlsCol, out int colDepartment, 16);
                SetHeadText("Section", sheet1, xlsRow, ref xlsCol, out int colSection, 16);
                SetHeadText("SubSection", sheet1, xlsRow, ref xlsCol, out int colSubSection, 16);
                SetHeadText("Line", sheet1, xlsRow, ref xlsCol, out int colLine, 16);
                SetHeadText("Unit", sheet1, xlsRow, ref xlsCol, out int colUnit, 16);
                SetHeadText("DOJ", sheet1, xlsRow, ref xlsCol, out int colDOJ, 16);
                SetHeadText("Bank", sheet1, xlsRow, ref xlsCol, out int colBankName, 16);
                SetHeadText("Bank Acc", sheet1, xlsRow, ref xlsCol, out int colBankAcc, 20);
                SetHeadText("DOS", sheet1, xlsRow, ref xlsCol, out int colDOS, 16);



                #region dynamic shift
                string shift = @"	 select distinct da.AllowanceDailyId,ad.UserName,ad.Id,ad.IsVoucherPayment
							 from [dbo].[DailyAllowanceTransaction] as da
							 left join [HKP].[AllowanceDaily] ad on ad.Id=da.AllowanceDailyId
							 where da.PlantId='" + identity.PlantId + @"' and da.WorkDate between '" + fromDate + @"' and '" + toDate + @"' 
                            and ad.IsVoucherPayment=1";
                DataTable dt = _sqlRepository.GetDataTable(shift);

                Dictionary<string, int> dicShift = new Dictionary<string, int>();

                int COL = colDOS + 1;
                int startColForDailyAllowance = COL;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dicShift.Add(dt.Rows[i]["Id"].ToString(), COL);

                    sheet1[xlsRow - 1, COL].Text = dt.Rows[i]["UserName"].ToString();
                    sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL + 1].Merge();
                    sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL + 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL + 1].BorderAround(ExcelLineStyle.Thin);
                    sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL + 1].CellStyle.Interior.ColorIndex = ExcelKnownColors.LightGreen;
                    
                    SetHeadText(sheet1, xlsRow, COL,"Quantity");
                    COL++;
                    SetHeadText(sheet1, xlsRow, COL, "Amount");
                    COL++;
                }

                sheet1[xlsRow - 1, COL].Text = "TOTAL";
                sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL].Merge();
                sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL].BorderAround(ExcelLineStyle.Thin);
                sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;

                //SetHeadText("Quantity", sheet1, xlsRow, ref COL, out int ColTotalQuantity, 10);
                //SetHeadText("Amount", sheet1, xlsRow, ref COL, out int ColTotalAmount, 10);
               //int  ColTotalQuantity = COL;
               // SetHeadText(sheet1, xlsRow, COL, "Quantity");
                //COL++;
                int ColTotalAmount = COL;
                SetHeadText(sheet1, xlsRow, COL, "Amount");
               
                #endregion

                int RowHeaderLimit = xlsRow;
                #endregion------------------Column Header------------------

                endXlsCol = (COL - 1);
                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                string companyId = identity.CompanyId;
                try
                {
                    DataTable dtCompanyId = _sqlRepository.GetDataTable(@"select * from ORG.Company where Id = '"+companyId+@"'");
                    string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompanyId.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    Image companyLogo = Image.FromFile(strPath);
                    if (companyLogo != null)
                    {
                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(2);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);


                    }


                }
                catch (Exception)
                {


                }
                xlsRow = 1;
                xlsCol = 1;

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 16;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;

                sheet1.Range[xlsRow, 3].Text = "From : " + fromDate + " To " + toDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                DataTable DTemp = new DataView(dsAllowanceDailySummary.Tables[0]).ToTable(true, "EmployeeCode", "EmployeeName", "Designation", "Department", "Section", "SubSection", "Line", "Unit", "DOJ", "DOS", "BankName", "BankAccNo");
                xlsRow = 8;
                int startRow = xlsRow;
                for (int i = 0; i < DTemp.Rows.Count; i++)
                {
                    sheet1.Range[xlsRow, colEmployeeCode].Text = DTemp.Rows[i]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, colEmployeeName].Text = DTemp.Rows[i]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, colDesignation].Text = DTemp.Rows[i]["Designation"].ToString();
                    sheet1.Range[xlsRow, colDepartment].Text = DTemp.Rows[i]["Department"].ToString();
                    sheet1.Range[xlsRow, colSection].Text = DTemp.Rows[i]["Section"].ToString();
                    sheet1.Range[xlsRow, colSubSection].Text = DTemp.Rows[i]["SubSection"].ToString();
                    sheet1.Range[xlsRow, colLine].Text = DTemp.Rows[i]["Line"].ToString();
                    sheet1.Range[xlsRow, colUnit].Text = DTemp.Rows[i]["Unit"].ToString();
                    sheet1.Range[xlsRow, colDOJ].Text = DTemp.Rows[i]["DOJ"].ToString();
                    sheet1.Range[xlsRow, colBankName].Text = DTemp.Rows[i]["BankName"].ToString();
                    sheet1.Range[xlsRow, colBankAcc].Text = DTemp.Rows[i]["BankAccNo"].ToString();
                    sheet1.Range[xlsRow, colDOS].Text = DTemp.Rows[i]["DOS"].ToString();
                    DataView Newdataview = new DataView(dtAllowanceDailySummary);
                    Newdataview.RowFilter = "EmployeeCode='" + DTemp.Rows[i]["EmployeeCode"].ToString() + @"'";
                    double Quantity = 0, Amount = 0;
                    for (int j = 0; j < Newdataview.Count; j++)
                    {
                        int _col = dicShift[Newdataview[j]["AllowanceDailyId"].ToString()];
                        Quantity += clsStaticInfo.dbl(Newdataview[j]["Quantity"].ToString());
                        Amount += clsStaticInfo.dbl(Newdataview[j]["Amount"].ToString());

                        sheet1[xlsRow, _col].Number = clsStaticInfo.dbl(Newdataview[j]["Quantity"].ToString());
                        sheet1[xlsRow, _col + 1].Number = clsStaticInfo.dbl(Newdataview[j]["Amount"].ToString());
                    }
                    //sheet1[xlsRow, ColTotalQuantity].Number = Quantity;
                    sheet1[xlsRow, ColTotalAmount].Number = Amount;
                    xlsRow++;
                }
                xlsRow += 1;

                sheet1.IsDisplayZeros = false;
                #endregion ----------------------Data-----------------------

                var endXlsRow = xlsRow;

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }
                #endregion

                #region Freeze Panes
                var xx = RowHeaderLimit + 1;
                sheet1.UsedRange["A" + xx].FreezePanes();
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                sheet1.Name = "Daily Allowance Summary";
                #endregion

                workbook.Version = ExcelVersion.Excel97to2003;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "Daily Allowance Summary.xls";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                //return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw (ex);
            }
        }

        string GetFormulaGrandTotal(ArrayList al, int col)
        {
            string _formula = string.Empty;
            ReportUtility ru = new ReportUtility();
            try
            {
                for (int i = 0; i < al.Count; i++)
                {
                    if (_formula.Length == 0)
                    {
                        _formula = "=" + ru.GetColumnNameForXls(col) + al[i];
                    }
                    else
                    {
                        _formula += "+" + ru.GetColumnNameForXls(col) + al[i];
                    }
                }
                return _formula;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion--------------------------------------------Xls Report End----------------------------------------------------

        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, string Text)
        {
            //if (string.IsNullOrEmpty(Text) == false)
            //{
            sheet.Range[xlsRow, xlsCol].Text = Text;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            //}
        }
        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, double Number)
        {
            //if (string.IsNullOrEmpty(Text) == false)
            //{
            sheet.Range[xlsRow, xlsCol].Number = Number;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            //}
        }

        private void CreateDynamicSHead(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out List<SalaryHeadSequence> list)
        {
            try
            {
                list = new List<SalaryHeadSequence>();
                _total_head_count = 0;
                _count_earning_head = 0;
                _count_deducting_head = 0;
                _count_earning_ctchead = 0;
                int countGrossPostion = 0;
                string deductionFormula = "";

                xlsCol += 1;
                countGrossPostion++;

                //salaryHSGross.SalaryHeadId = "Gross";

                int countCTCPosition = countGrossPostion;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region loop ctc
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {


                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "Net Payable".ToUpper())
                        {
                            _total_head_count++;


                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();

                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.FontName = "Arial Narrow";
                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Size = 10;
                            //sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition, xlsRow + 1, ColGrs + countCTCPosition + 1].Merge();
                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.ShrinkToFit = true;


                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                            {
                                sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
                            }
                            xlsCol += 2;
                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                            salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;


                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();

                            salaryHeadSequence.Sequence = ci;
                            salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;



                            list.Add(salaryHeadSequence);
                            countCTCPosition++;
                        }


                    }//SalaryHead 
                    #endregion
                }//for
                xlsCol += 1;


                _count_earning_ctchead = countCTCPosition - 1;

                int countDeductionPosition = countCTCPosition - 1;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region deduction
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
                        //{
                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "D")
                        {
                            _total_head_count++;
                            countDeductionPosition++;

                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Size = 10;
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.FontName = "Arial Narrow";
                            //sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition, xlsRow + 1, ColGrs + countDeductionPosition + 1].Merge();
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.ShrinkToFit = true;


                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                            {
                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
                            }
                            xlsCol += 2;
                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
                            if (deductionFormula.Length == 0)
                            {
                                deductionFormula += salaryHeadSequence.XLColIndex.ToString();
                            }
                            else
                            {
                                deductionFormula += "," + salaryHeadSequence.XLColIndex.ToString();
                            }

                            //countDeductionPosition++;

                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();

                            salaryHeadSequence.Sequence = ci;
                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;

                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();


                            list.Add(salaryHeadSequence);

                            _count_deducting_head++;
                        }
                        //}//CTC/Gross
                    }//SalaryHead 
                    #endregion
                }//for

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetHeadText(IWorksheet sheet, int xlsRow, int xlsCol, string text)
        {
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

        }
        private void SetHeadText(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            // sheet.Range[xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.AliceBlue;
            sheet.Range[xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ColIndex = xlsCol;
            xlsCol += 1;
        }

        private void SetHeaderValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            //sheet.Range[row, col].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
            ColIndex = xlsCol;
            xlsCol += 1;
        }
        private void CreateDynamicMonthHead(DataTable dtMonthList, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColStart, out List<FiscalYearMonthSequence> list)
        {
            try
            {
                list = new List<FiscalYearMonthSequence>();
                _total_head_count = 0;

                int countGross = 0;
                string grossFormula = "";
                string deductionFormula = "";
                for (int ci = 0; ci < dtMonthList.Rows.Count; ci++)
                {
                    _total_head_count++;
                    countGross++;
                    sheet1.Range[xlsRow, ColStart + countGross].Text = dtMonthList.Rows[ci]["MonthName"].ToString().Substring(0, 3) + "," + dtMonthList.Rows[ci]["MonthYear"].ToString().Substring(2, 2);
                    sheet1.Range[xlsRow, ColStart + countGross].ColumnWidth = 8;
                    sheet1.Range[xlsRow, ColStart + countGross].CellStyle.Font.Bold = true;
                    //sheet.Range[row, col].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
                    sheet1.Range[xlsRow, ColStart + countGross].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColStart + countGross].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, ColStart + countGross].BorderAround(ExcelLineStyle.Thin);

                    FiscalYearMonthSequence fiscalYearMonthSequence = new FiscalYearMonthSequence();
                    fiscalYearMonthSequence.MonthName = dtMonthList.Rows[ci]["MonthName"].ToString();
                    fiscalYearMonthSequence.MonthNo = dtMonthList.Rows[ci]["MonthNumber"].ToString();
                    fiscalYearMonthSequence.LastDayOfMonth = dtMonthList.Rows[ci]["LastDayOfMonth"].ToString();
                    fiscalYearMonthSequence.MonthYear = dtMonthList.Rows[ci]["MonthYear"].ToString();
                    fiscalYearMonthSequence.XLColIndex = ColStart + countGross;

                    list.Add(fiscalYearMonthSequence);
                    xlsCol += 1;
                }//for         
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
#endregion