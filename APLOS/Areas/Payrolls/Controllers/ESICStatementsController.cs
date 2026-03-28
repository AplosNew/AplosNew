using Aplos.Controllers;
using ConnectionManager;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.Helpers.ReportUtility;
using static Library.Service.HumanResources.PayRegisterBDReportService;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class ESICStatementsController : BaseController
    {
        #region Constructor

        private readonly IPayRegisterBDReportService _payRegisterBDReportService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;



        public ESICStatementsController(
              IPayRegisterBDReportService payRegisterBDReportService, IEmployeeProfileService employeeProfileService,
              ISqlRepository sqlRepository
            )
        {
            _payRegisterBDReportService = payRegisterBDReportService;
            _employeeProfileService = employeeProfileService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations       
        [HttpPost, Authorize]
        public ActionResult GetESICReports(string month, string year, bool isActive, bool isSeperated, bool isMaternity, string pAction)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsReport objRpt = null;

            DataSet dsCmp = null;
            DataSet dsFactory = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            DataTable dt = null;
            int iCount = 0;
            double GrandTotal = 0;
            Dictionary<string, int> SalaryHeadIndex = new Dictionary<string, int>();
            try
            {

                objRpt = new clsReport();

                var today = DateTime.Now.Date;

                #region DataSet

                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No

                Dictionary<string, DataRow> dicAttdnSummary = new Dictionary<string, DataRow>();
                Dictionary<string, List<DataRow>> dicSalary = new Dictionary<string, List<DataRow>>();
                Dictionary<string, List<DataRow>> dicPFSalaryHeadWiseData = new Dictionary<string, List<DataRow>>();

                DataTable dtEmpInfo = GetESICEmployeeInfo(identity.PlantId, Convert.ToInt32(month), year, isActive, isSeperated, isMaternity);

                dicSalary = GetEmpESICSalaryInfo(identity.PlantId, Convert.ToInt32(month), year, isActive, isSeperated, isMaternity);
                dicAttdnSummary = GetEmployeeAttdnSummary(month, year, identity.PlantId);

                GetEmpPFSalaryHead(identity.PlantId, out dt);
                dicPFSalaryHeadWiseData = GetEmpPFSalaryHeadInfo(identity.PlantId, Convert.ToInt32(month), year, isActive, isSeperated, isMaternity);

                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet

                var colSrNo = 0;
                var colPaycode = 0;
                var colEmployeeName = 0;
                var colDays = 0;
                var colWagesAmount = 0;
                var colInsuranceNo = 0;
                var colESICER = 0;
                var colESICEE = 0;
                var colTotal = 0;
                var colWagesTotal = 0;
                var colReason = 0;
                var colLWD = 0;
                double TotalNumer = 0;

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);

                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                xlsRow = 6;

                //	#region ------------------Column Header------------------

                var ru = new ReportUtility();

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "S.No", 5); colSrNo = xlsCol; xlsCol++;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmpCode", 8); colPaycode = xlsCol; xlsCol++;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Employee Name", 18); colEmployeeName = xlsCol; xlsCol++;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Insurance No.", 12); colInsuranceNo = xlsCol; xlsCol++;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Pay Days", 8); colDays = xlsCol; xlsCol++;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    iCount = xlsCol;
                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, dt.Rows[i]["SalaryHead"].ToString(), 8, 25, ExcelHAlign.HAlignCenter);
                    SalaryHeadIndex.Add(dt.Rows[i]["SalaryHeadId"].ToString(), iCount);
                    xlsCol++;
                }
                if (dt.Rows.Count > 1)
                {
                    ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Wages Total", 10); colWagesTotal = xlsCol; xlsCol++;
                }

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Employee", 10); colESICEE = xlsCol; xlsCol++;

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Employer", 10); colESICER = xlsCol; xlsCol++;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Total", 10); colTotal = xlsCol; xlsCol++;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Reason Code for Zero Workings Days", 10); colReason = xlsCol; xlsCol++;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, " Last Working Day", 10); colLWD = xlsCol;
                DataRow drAttdnSummary = null;
                endXlsCol = xlsCol;
                xlsRow++;
                var formulaStartRow = xlsRow;
                var slCount = 0;
                for (int i = 0; i < dtEmpInfo.Rows.Count; i++)
                {
                    double basic = 0.00;
                    double gross = 0.00;
                    double esicER = 0.00;
                    double esicEE = 0.00;

                    bool basicIntegerInDisb = false;
                    int basicDecimalPoint = 0;

                    bool grossIntegerInDisb = false; //GROSS
                    int grossDecimalPoint = 0;//GROSS
                    bool esicERIntegerInDisb = false;
                    int esicERDecimalPoint = 0;
                    bool esicEEIntegerInDisb = false;
                    int esicEEDecimalPoint = 0;



                    if (dicSalary.ContainsKey(dtEmpInfo.Rows[i]["EmpSystemId"].ToString()))
                    {

                        List<DataRow> dlr = dicSalary[dtEmpInfo.Rows[i]["EmpSystemId"].ToString()];

                        foreach (var item in dlr)
                        {
                            if (item["HeadCategory"].ToString() == "Basic")
                            {
                                basic = clsStaticInfo.dbl(item["DisbusmentAmount"].ToString());
                                basicIntegerInDisb = bplib.clsWebLib.GetBoolData(item["IntegerInDisb"]);
                                basicDecimalPoint = (int)clsStaticInfo.dbl(item["DecimalNo"].ToString());
                            }
                            if (item["HeadCategory"].ToString() == "GROSS")
                            {
                                gross = clsStaticInfo.dbl(item["DisbusmentAmount"].ToString());
                                grossIntegerInDisb = bplib.clsWebLib.GetBoolData(item["IntegerInDisb"]);
                                grossDecimalPoint = (int)clsStaticInfo.dbl(item["DecimalNo"].ToString());
                            }
                            if (item["HeadCategory"].ToString() == "ESIC Employer Contribution")
                            {
                                esicER = clsStaticInfo.dbl(item["DisbusmentAmount"].ToString());
                                esicERIntegerInDisb = bplib.clsWebLib.GetBoolData(item["IntegerInDisb"]);
                                esicERDecimalPoint = (int)clsStaticInfo.dbl(item["DecimalNo"].ToString());
                            }
                            if (item["HeadCategory"].ToString() == "ESIC Employee Contribution")
                            {
                                esicEE = clsStaticInfo.dbl(item["DisbusmentAmount"].ToString());
                                esicEEIntegerInDisb = bplib.clsWebLib.GetBoolData(item["IntegerInDisb"]);
                                esicEEDecimalPoint = (int)clsStaticInfo.dbl(item["DecimalNo"].ToString());
                            }

                        }
                    }

                    var Workingdays = 0.00; //GetWorkingDate(dvBioDvAC, Convert.ToString(x[i]));
                    drAttdnSummary = null;
                    if (dicAttdnSummary.ContainsKey(dtEmpInfo.Rows[i]["EmpSystemId"].ToString()))
                    {

                        drAttdnSummary = dicAttdnSummary[dtEmpInfo.Rows[i]["EmpSystemId"].ToString()];
                        //if (!String.IsNullOrEmpty(dtEmpInfo.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper()))
                        //{
                        //    if (dtEmpInfo.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOffAndHoliday.ToString().ToUpper())
                        //    {
                        //        Workingdays = clsStaticInfo.dbl(drAttdnSummary["TotalProcDate"].ToString()) - clsStaticInfo.dbl(drAttdnSummary["TotalAbsent"].ToString()) - clsStaticInfo.dbl(drAttdnSummary["TotalHoliDay"].ToString()) - clsStaticInfo.dbl(drAttdnSummary["TotalWeekOff"].ToString());
                        //    }
                        //    if (dtEmpInfo.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOff.ToString().ToUpper())
                        //    {
                        //        Workingdays = clsStaticInfo.dbl(drAttdnSummary["TotalProcDate"].ToString()) - clsStaticInfo.dbl(drAttdnSummary["TotalAbsent"].ToString()) - clsStaticInfo.dbl(drAttdnSummary["TotalWeekOff"].ToString());
                        //    }
                        //}
                        //else
                        //{
                        //    Workingdays = clsStaticInfo.dbl(drAttdnSummary["TotalProcDate"].ToString()) - clsStaticInfo.dbl(drAttdnSummary["TotalAbsent"].ToString());
                        //}

                        Workingdays = clsStaticInfo.dbl(drAttdnSummary["TotalPayDay"].ToString());
                    }


                    var esicDocNo = dtEmpInfo.Rows[i]["DocNumber"].ToString();

                    var age = dtEmpInfo.Rows[i]["age"].ToString();

                    slCount++;
                    #region Loop

                    if (dicPFSalaryHeadWiseData.ContainsKey(dtEmpInfo.Rows[i]["EmpSystemId"].ToString()))
                    {
                        List<DataRow> dlrPF = dicPFSalaryHeadWiseData[dtEmpInfo.Rows[i]["EmpSystemId"].ToString()];
                        TotalNumer = 0;
                        foreach (var item in dlrPF)
                        {
                            sheet1.Range[xlsRow, SalaryHeadIndex[item["SalaryHeadID"].ToString()]].Number = clsStaticInfo.dbl(item["DisbusmentAmount"].ToString());
                            TotalNumer = clsStaticInfo.dbl(item["DisbusmentAmount"].ToString()) + TotalNumer;
                        }
                        GrandTotal = GrandTotal + TotalNumer;
                    }
                    SetSLText(ref sheet1, xlsRow, colSrNo, slCount);
                    if (dt.Rows.Count > 1)
                    {
                        ru.SetTextBorder(ref sheet1, xlsRow, colWagesTotal, TotalNumer);
                    }
                    ru.SetTextBorder(ref sheet1, xlsRow, colPaycode, dtEmpInfo.Rows[i]["EmployeeCode"].ToString());
                    ru.SetTextBorder(ref sheet1, xlsRow, colEmployeeName, dtEmpInfo.Rows[i]["EmployeeName"].ToString());
                    sheet1.Range[xlsRow, colEmployeeName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    ru.SetTextBorder(ref sheet1, xlsRow, colInsuranceNo, esicDocNo);
                    ru.SetTextBorder(ref sheet1, xlsRow, colDays, Workingdays);
                    sheet1.Range[xlsRow, colDays].NumberFormat = ru.NumberFormatNegativeSignDelimeterDecimalTwo();

                    //sheet1.Range[xlsRow, colWagesAmount].Number = Convert.ToDouble(gross);
                    //sheet1.Range[xlsRow, colWagesAmount].NumberFormat = GetDecimalFormat(grossIntegerInDisb, grossDecimalPoint);
                    //sheet1.Range[xlsRow, colWagesAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, colWagesAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    //sheet1.Range[xlsRow, colWagesAmount].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, colESICEE].Number = Convert.ToDouble(esicEE) * (-1.00);
                    sheet1.Range[xlsRow, colESICEE].NumberFormat = GetDecimalFormat(esicEEIntegerInDisb, esicEEDecimalPoint);
                    sheet1.Range[xlsRow, colESICEE].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, colESICEE].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, colESICEE].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, colESICER].Number = Convert.ToDouble(esicER);
                    sheet1.Range[xlsRow, colESICER].NumberFormat = GetDecimalFormat(esicERIntegerInDisb, esicERDecimalPoint);
                    sheet1.Range[xlsRow, colESICER].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, colESICER].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, colESICER].BorderAround(ExcelLineStyle.Hair);
                    ru.SetText(ref sheet1, xlsRow, colTotal, Convert.ToInt32(esicER) + Convert.ToInt32(esicEE) * (-1));
                    sheet1.Range[xlsRow, colTotal].BorderAround(ExcelLineStyle.Hair);

                    if (esicER != 0)
                    {
                        ru.SetTextBorder(ref sheet1, xlsRow, colReason, 0);
                    }
                    else if (dtEmpInfo.Rows[i]["EmployeeStatus"].ToString()== "Separated")
                    {
                        ru.SetTextBorder(ref sheet1, xlsRow, colReason, 0);
                    }
                    else 
                    {
                        ru.SetTextBorder(ref sheet1, xlsRow, colReason, 1);
                    }
                    ru.SetTextBorder(ref sheet1, xlsRow, colLWD, dtEmpInfo.Rows[i]["LastWorkingDay"].ToString());

                    #endregion Loop
                    xlsRow++;
                }

                var summationRowLimit = xlsRow - 1;
                sheet1.Range[xlsRow, colDays].Text = "Total";
                sheet1.Range[xlsRow, colDays].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, colDays].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                if (dt.Rows.Count == 1)
                {
                    getGrandTotal(ref sheet1, xlsRow, 6, GrandTotal, ru);//dtEmpInfo.Tables[0].Rows[i][""].ToString()
                    sheet1.Range[xlsRow, 6].NumberFormat = ru.NumberFormatNegativeSignDelimeterDecimalTwo();
                }
                else
                {
                    int num = dt.Rows.Count;
                    getGrandTotal(ref sheet1, xlsRow, 6 + num, GrandTotal, ru);//dtEmpInfo.Tables[0].Rows[i][""].ToString()
                    sheet1.Range[xlsRow, 6 + num].NumberFormat = ru.NumberFormatNegativeSignDelimeterDecimalTwo();
                }

                sheet1.Range[xlsRow, colESICER].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, colESICER].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colESICER].Formula = "=SUM(" + ru.GetColumnNameForXls(colESICER) + formulaStartRow + ":" + ru.GetColumnNameForXls(colESICER) + (summationRowLimit) + ")";
                sheet1.Range[xlsRow, colESICER].NumberFormat = ru.NumberFormatInt();

                sheet1.Range[xlsRow, colESICEE].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, colESICEE].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colESICEE].Formula = "=SUM(" + ru.GetColumnNameForXls(colESICEE) + formulaStartRow + ":" + ru.GetColumnNameForXls(colESICEE) + (summationRowLimit) + ")";
                sheet1.Range[xlsRow, colESICEE].NumberFormat = ru.NumberFormatInt();

                sheet1.Range[xlsRow, colTotal].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, colTotal].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colTotal].Formula = "=SUM(" + ru.GetColumnNameForXls(colTotal) + formulaStartRow + ":" + ru.GetColumnNameForXls(colTotal) + (summationRowLimit) + ")";
                sheet1.Range[xlsRow, colTotal].NumberFormat = ru.NumberFormatInt();

               

                #region ******************Report Header******************

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
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
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
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 15;
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
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 15;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "ESIC Statement";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "For the Month of " + monthName + "," + year;
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
                sheet1.FirstVisibleColumn = 2;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 8;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup	
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                //sheet1.PageSetup.PrintTitleColumns = "$A:$P";
                //sheet1.PageSetup.PrintTitleRows = "$5:$6";
                sheet1.PageSetup.PrintTitleRows = "$A$6:$IV$6";

                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                #endregion Page Setup

                workbook.Version = ExcelVersion.Excel2013;
                string fileName = monthName + "-" + year + "ESI Statement" + DateTime.Now.ToString("yyMMdd") + ".xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }
        private void getGrandTotal(ref IWorksheet sheet1, int xlsRow, int xlsCol, double txt, ReportUtility ru)
        {
            try
            {
                sheet1.Range[xlsRow, xlsCol].Number = txt;
                sheet1.Range[xlsRow, xlsCol].NumberFormat = ru.NumberFormatDecimalTwo();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;

            }
            catch (Exception)
            {

                throw;
            }
        }
        public void SetHeaderTextPFund(ref IWorksheet sheet, int row, int col, string txt, int width, int RH, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);
        }

        #endregion -- Operations

        public DataTable GetESICEmployeeInfo(string plantId, int monthName, string year, bool isActive, bool isSeperated, bool isMaternity)
        {
            string strSQL;
            var days = DateTime.DaysInMonth(Convert.ToInt32(year), monthName);//Number of Days in a month
            string monthNameString = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(monthName);//Month Name from Month No
            var date = days + "-" + monthNameString + "-" + year;
            string empStatus = "";


            if (isActive == true)
            {
                empStatus = @"AND EmpSlr.EmployeeStatus = 'Active'";
            }
            if (isSeperated == true)
            {
                empStatus = @"AND EmpSlr.EmployeeStatus = 'Separated'";
            }
            if (isActive == true && isSeperated == true)
            {
                empStatus = "";
            }
            ConnectionManager.DAL.ConManager objCon;

            string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + date + @"') AND YearNo = Year('" + date + @"')";
            DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);

            string salaryProcessID = "''";
            for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
            {
                salaryProcessID += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
            }
            try
            {
                strSQL = @"  SELECT E.SystemID EmpSystemId,E.PlantId
                            , E.EmployeeCode, E.EmployeeName,E.DOB, E.DOJ,E.DOS, E.EmployeeStatus
                            ,ed.DocNumber,LastWorkingDay=CASE WHEN E.EmployeeStatus='Separated' THEN (Select top 1 FORMAT(WorkDate,'dd-MMM-yyyy') From dbo.AttdnProcessData Where EmpSystemId=E.SystemId Order By WorkDate DESC) ELSE '' END
                            ,DATEDIFF(YY,E.DOB,'" + date + @"') As Age
											
											,E.EmployeeCategorySystemID,ECA.UserName EmpCategoryName,ECA.UserName EmpCategoryId
                                                    ,ISNULL(ECA.WorkingDaysInAMonth,'') WorkingDaysInAMonth
                                     FROM EmployeeInformation E	
													Inner Join(select Distinct EmpInfoSystemID,SalaryID from  SalaryProcChild where SlrProcMstSystemID  IN (" + salaryProcessID + @")) spc ON spc.EmpInfoSystemID = e.SystemId 
									  Inner JOIN SalaryProcessLogDetail SPLD ON  SPLD.EmpSystemId = E.SystemId and SPLD.SalaryProcessId  IN (" + salaryProcessID + @")--AND e.SystemId = SPLD.EmpSystemId  
                                   
												left join HKP.EmployeeCategory ECA ON ECA.Id = SPLD.EmployeeCategoryId
											        LEFT JOIN EmployeeDocument ED ON ED.EmpSystemID = E.SystemId
                                                 AND ComplianceDocumentId = 
												(
												SELECT TOP(1) Id	FROM HKP.ComplianceDocument WHERE ProfileType = 'ESIC'
												)
										
												WHERE                                                   
                                             E.SystemId IN (
											 Select EmpSystemId from 
											      [EmployeeEligibleForSalaryHeadEnum] EESHE    
                                                left join [dbo].[EmployeeCodeType] ect on ect.Id=e.EmployeeCodeTypeId
                                            where EESHE.SalaryStructureId = spc.SalaryId AND
                                                EESHE.EmpSystemId = e.SystemId 
												AND EESHE.SalaryHeadEnum IN('ESIC')   AND IsEligible = 1 and ISNULL(ect.IsOutSider,0) =0
											 )          
                                                and
                              E.PlantId ='" + plantId + @"'
                            ";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end function

        public Dictionary<string, List<DataRow>> GetEmpESICSalaryInfo(string plantId, int monthName, string year, bool isActive, bool isSeperated, bool isPFEligible)
        {
            string strSQL;
            Dictionary<string, List<DataRow>> dicSalary = new Dictionary<string, List<DataRow>>();
            var days = DateTime.DaysInMonth(Convert.ToInt32(year), monthName);//Number of Days in a month
            string monthNameString = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(monthName);//Month Name from Month No
            var date = days + "-" + monthNameString + "-" + year;
            string empStatus = "";
            if (isActive == true)
            {
                empStatus = @"AND EmpSlr.EmployeeStatus = 'Active'";
            }
            if (isSeperated == true)
            {
                empStatus = @"AND EmpSlr.EmployeeStatus = 'Separated'";
            }
            if (isActive == true && isSeperated == true)
            {
                empStatus = "";
            }
            ConnectionManager.DAL.ConManager objCon;

            string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"')
                                        AND MonthNo = Month('" + date + @"') AND YearNo = Year('" + date + @"')";
            DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);

            string salaryProcessID = "''";
            for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
            {
                salaryProcessID += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
            }
            try
            {
                strSQL = @" SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
													SPC.EmpInfoSystemID, SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
													SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
													SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
												    SPM.AmtDefinitionCurrencyID,SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect
                                                    ,SH.SalaryHead,SH.HeadCategory,SH.HeadType
                                                    ,CASE WHEN ISNULL(SPM.SalaryProcFlag,'') = '' THEN 'Regular' ELSE SalaryProcFlag END EmployeeStatus
                                                    ,SH.IsCTCComponent,SH.IsGrossComponent--,SH.Cat
													,crc.IsDecimalInDisb,crc.DecimalNo,crc.IntegerInDisb
											 FROM SalaryProcChild SPC 
														INNER JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
																							AND SPM.SystemID IN  (" + salaryProcessID + @")	
                                                        INNER JOIN SalaryHead SH ON SH.SalaryHeadID=SPC.SalaryHeadID 
														AND SH.HeadCategory IN ('GROSS','ESIC Employee Contribution','Basic','ESIC Employer Contribution') 
														
                                            Inner join EmployeeInformation EEI ON EEI.SystemId = SPC.EmpInfoSystemID
														LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EEI.SalaryRuleMasterSystemID
                                                                LEFT JOIN CurrencyRuleMaster crm on crm.SystemID = sRM.CurrencyRuleSystemID
                                                                LEFT JOIN CurrencyRuleChild crc on crc.MstSystemID = CRM.SystemID and crc.SalaryHeadID=spc.SalaryHeadID			
                                                                left join [dbo].[EmployeeCodeType] ect on ect.Id=eei.EmployeeCodeTypeId
														WHERE  EEI.PlantId = '" + plantId + @"' and ISNULL(ect.IsOutSider,0) =0 order by EmpInfoSystemID";

                DataTable dt = _sqlRepository.GetDataTable(strSQL);
                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (empId != dt.Rows[i]["EmpInfoSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        dicSalary.Add(dt.Rows[i]["EmpInfoSystemID"].ToString(), _data);
                    }
                    _data.Add(dt.Rows[i]);

                    empId = dt.Rows[i]["EmpInfoSystemID"].ToString();
                }
                return dicSalary;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end function
        public Dictionary<string, DataRow> GetEmployeeAttdnSummary(string MonthNo, string YearNo, string PlantId)
        {
            try
            {
                Dictionary<string, DataRow> dicSummary = new Dictionary<string, DataRow>();
                string Sql = @"select spAt.* from SalaryProceAttdnData spAt Inner Join EmployeeInformation EEI ON EEI.SystemId = spAT.EmpSystemId
                                        WHERE MonthNo = '" + MonthNo + @"' and YearNo = '" + YearNo + @"' and EEI.PlantId = '" + PlantId + @"'";

                DataTable dt = _sqlRepository.GetDataTable(Sql);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dicSummary.ContainsKey(dt.Rows[i]["EmpSystemId"].ToString()))
                    {
                        continue;
                    }
                    dicSummary.Add(dt.Rows[i]["EmpSystemId"].ToString(), dt.Rows[i]);
                }

                return dicSummary;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetEmpPFSalaryHead(string plantId, out DataTable dt)
        {
            string strSQL;
            try
            {
                strSQL = @"select pfs.SalaryHeadID,sh.SalaryHead from ESICPolicyMaster pf 
		                            join ESICPolicySalaryHead pfs on pfs.ESICPolicyMasterId=pf.ID 
		                            join SalaryHead SH on SH.SalaryHeadID=pfs.SalaryHeadID
                                    where Pf.PlantId='" + plantId + "'";

                dt = _sqlRepository.GetDataTable(strSQL);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }//end function

        public Dictionary<string, List<DataRow>> GetEmpPFSalaryHeadInfo(string plantId, int monthName, string year, bool isActive, bool isSeperated, bool isPFEligible)
        {
            string strSQL;
            Dictionary<string, List<DataRow>> dicSalary = new Dictionary<string, List<DataRow>>();
            var days = DateTime.DaysInMonth(Convert.ToInt32(year), monthName);//Number of Days in a month
            string monthNameString = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(monthName);//Month Name from Month No
            var date = days + "-" + monthNameString + "-" + year;
            string empStatus = "";
            if (isActive == true)
            {
                empStatus = @"AND EmpSlr.EmployeeStatus = 'Active'";
            }
            if (isSeperated == true)
            {
                empStatus = @"AND EmpSlr.EmployeeStatus = 'Separated'";
            }
            if (isActive == true && isSeperated == true)
            {
                empStatus = "";
            }
            ConnectionManager.DAL.ConManager objCon;

            string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"')
                                        AND MonthNo = Month('" + date + @"') AND YearNo = Year('" + date + @"')";
            DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);

            string salaryProcessID = "''";
            for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
            {
                salaryProcessID += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
            }
            try
            {
                strSQL = @" SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
													SPC.EmpInfoSystemID, SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
													SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
													SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
												    SPM.AmtDefinitionCurrencyID,SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect
                                                    ,SH.SalaryHead,SH.HeadCategory,SH.HeadType
                                                    ,CASE WHEN ISNULL(SPM.SalaryProcFlag,'') = '' THEN 'Regular' ELSE SalaryProcFlag END EmployeeStatus
                                                    ,SH.IsCTCComponent,SH.IsGrossComponent--,SH.Cat
													,crc.IsDecimalInDisb,crc.DecimalNo,crc.IntegerInDisb
											 FROM SalaryProcChild SPC 
														INNER JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
																							AND SPM.SystemID IN  (" + salaryProcessID + @")	
                                                        --INNER JOIN SalaryHead SH ON SH.SalaryHeadID=SPC.SalaryHeadID 
														--AND SH.HeadCategory IN ('PF Voluntary','Pension','Basic','PF Employer Contribution','PF Employee Contribution') 
														
                                            Inner join EmployeeInformation EEI ON EEI.SystemId = SPC.EmpInfoSystemID
                                            left join ESICPolicyMaster pf on pf.PlantID = EEI.PlantId
											join ESICPolicySalaryHead pfs on pfs.ESICPolicyMasterId=pf.ID 
											  INNER JOIN SalaryHead SH ON SH.SalaryHeadID=SPC.SalaryHeadID and SH.SalaryHeadID=pfs.SalaryHeadID and sh.SalaryHeadID in(pfs.SalaryHeadID)
														LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EEI.SalaryRuleMasterSystemID
                                                                LEFT JOIN CurrencyRuleMaster crm on crm.SystemID = sRM.CurrencyRuleSystemID
                                                                LEFT JOIN CurrencyRuleChild crc on crc.MstSystemID = CRM.SystemID and crc.SalaryHeadID=spc.SalaryHeadID			
                                                                left join [dbo].[EmployeeCodeType] ect on ect.Id=eei.EmployeeCodeTypeId
														WHERE  EEI.PlantId = '" + plantId + @"' and ISNULL(ect.IsOutSider,0) =0 order by EmpInfoSystemID";

                DataTable dt = _sqlRepository.GetDataTable(strSQL);
                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (empId != dt.Rows[i]["EmpInfoSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        dicSalary.Add(dt.Rows[i]["EmpInfoSystemID"].ToString(), _data);
                    }
                    _data.Add(dt.Rows[i]);

                    empId = dt.Rows[i]["EmpInfoSystemID"].ToString();
                }
                return dicSalary;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end function


        public void GetESICEmpInfo(out DataSet dsRef, string plantId, int monthName, string year, bool isActive, bool isSeperated, bool isMaternity)
        {
            string strSQL;
            var days = DateTime.DaysInMonth(Convert.ToInt32(year), monthName);//Number of Days in a month
            string monthNameString = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(monthName);//Month Name from Month No
            var date = days + "-" + monthNameString + "-" + year;

            string empStatus = "";

            string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = '" + monthName + @"' AND YearNo = '" + year + @"'";
            DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);

            string salaryProcessID = "''";
            for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
            {
                salaryProcessID += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
            }


            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (isActive == true)
                {
                    empStatus = @"AND EmpSlr.EmployeeStatus = 'Regular'";
                }
                if (isSeperated == true)
                {
                    empStatus = @"AND EmpSlr.EmployeeStatus = 'Separated'";
                }
                if (isMaternity == true)
                {
                    empStatus += " OR EmpBasic.EmployeeStatus ='MLV_PRE'";

                }
                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    empStatus = "";
                }

                strSQL = @"SELECT DISTINCT EmpSlr.EmpInfoSystemID, EmpBasic.EmployeeCode,CONVERT(INT,EmpBasic.EmployeeCode) EmployeeCodeS, EmpBasic.EmployeeName,DocNumber,EmpBasic.Age ,
                       			(ISNULL(MMDSA.TotalPresent, 0) + ISNULL(MMDSA.TotalLate, 0)) PresentDays,
								ISNULL(MMDSA.TotalHoliDay, 0) HoliDay, ISNULL(MMDSA.TotalWeekOff, 0) WeekOff,
                                ISNULL(MMDSA.TotalAbsent,0) TotalAbsent,
								(ISNULL(MMDSA.TotalLv, 0) + ISNULL(MMDSA.TotalMLv, 0)) LeaveDays,
                                ISNULL(MMDSA.TotalProcDate,0) TotalProcDate
                                ,EmpSlr.PlantID, EmpSlr.FromDate, EmpSlr.ToDate, EmpSlr.MonthNo, EmpSlr.YearNo, EmpSlr.PayAbleShSystemID,
                                EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount,
                                EmpSlr.DisbusmentCurrencyID, EmpSlr.DisbusmentAmount, EmpSlr.AcltExcDisbSlrHDID, EmpSlr.AcltExcDisbSlrHDAmt,
                                EmpSlr.AmtDefinitionCurrencyID,EmpSlr.EmployeeStatus,EmpSlr.EmpCategoryName,EmpSlr.EmpCategoryId,EmpSlr.WorkingDaysInAMonth
                                ,EmpSlr.AmtDefinitionCurrencyRate, EmpSlr.IsNetPayEffect
                                ,EMPSLR.cat,EmpSlr.SalaryHead,EmpSlr.HeadCategory,EmpSlr.HeadType,IsCTCComponent,IsGrossComponent
                                ,empslr.WorkingDaysInAMonth
                                ,ISNULL(empslr.IsDecimalInDisb,0) IsDecimalInDisb,ISNULL(empslr.DecimalNo,0) DecimalNo,ISNULL(empslr.IntegerInDisb,0) IntegerInDisb
                            FROM
                                    (
										 SELECT E.SystemID, E.EmployeeCode, E.EmployeeName, E.DOJ, E.EmployeeStatus,ED.DocNumber,DATEDIFF(YY,E.DOB,'" + date + @"') As Age,
											DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,GVDE.UserName GivenDesignationName,
											'' UserGroupSystemID, E.PlantID, F.UserName PlantName, E.UnitID,
											FU.UserName UnitName, E.DivisionID, DV.UserName DivisionName, E.DepartmentID, DP.UserName DepartmentName,
											E.SectionID, S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID
											--EC.UserName EmpCategoryName--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo
                                            --,egdsgg.GivenDesignationGroup
                                     FROM EmployeeInformation E
												LEFT JOIN org.Plant F ON E.PlantID = F.Id
												LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupId = DG.ID
												LEFT JOIN hkp.Designation DE ON E.GivenDesignationId = DE.Id
												LEFT JOIN hkp.Designation GVDE ON E.GivenDesignationId = GVDE.Id
												LEFT JOIN org.Unit FU ON E.UnitID = FU.Id
												LEFT JOIN org.Division DV ON E.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON E.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON E.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON E.SubSectionID = SS.Id
LEFT JOIN EmployeeDocument ED ON ED.EmpSystemID = E.SystemId
                                                 AND ComplianceDocumentId = 
												(
												SELECT Id	FROM HKP.ComplianceDocument WHERE ProfileType = 'ESIC'
												)		) EmpBasic
												--INNER  JOIN EmployeeDocument ED ON E.SystemId = ED.EmpSystemID
												--INNER JOIN HKP.ComplianceDocument CD ON CD.Id = ED.ComplianceDocumentId  AND CD.ProfileType = 'ESIC' 
												--where E.EmployeeStatus = 'Active' OR E.DOS 

									--) EmpBasic
                                    INNER JOIN
											(
											 SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
													SPC.EmpInfoSystemID, SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
													SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
													SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
												    SPM.AmtDefinitionCurrencyID,EC.UserName EmpCategoryName,EC.UserName EmpCategoryId
                                                    ,ISNULL(EC.WorkingDaysInAMonth,'') WorkingDaysInAMonth ,SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect
                                                    ,SH.SalaryHead,SH.HeadCategory,SH.HeadType
                                                    ,CASE WHEN ISNULL(SPM.SalaryProcFlag,'') = '' THEN 'Regular' ELSE SalaryProcFlag END EmployeeStatus
                                                    ,SH.IsCTCComponent,SH.IsGrossComponent,SH.Cat,crc.IsDecimalInDisb,crc.DecimalNo,crc.IntegerInDisb
											 FROM SalaryProcChild SPC
														INNER JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
																							AND SPM.SystemID IN( " + salaryProcessID + @" )
									 JOIN SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId  IN(" + salaryProcessID + @") --AND e.SystemId = SPLD.EmpSystemId  --SPLD.SalaryProcessId = SPM.SystemId 
                                                   AND SPLD.PlantId = '" + plantId + @"'   AND SPC.EmpInfoSystemID = SPLD.EmpSystemId 
												LEFT JOIN [HKP].[EmployeeCategory] EC ON EC.Id = SPLD.EmployeeCategoryId

                                                        INNER JOIN (--Salary Head
                                                                               (SELECT *,'B' Cat FROM SalaryHead where HeadCategory in ('Basic'))
																			    UNION														
																				SELECT *,'ESICER' FROM SalaryHead WHERE HeadCategory IN ('ESIC Employer Contribution')
																				UNION
																				SELECT *,'ESICEE' FROM SalaryHead WHERE HeadCategory IN ('ESIC Employee Contribution')
                                                                                UNION
                                                                                SELECT *,'GROSS' FROM SalaryHead WHERE HeadCategory = 'GROSS'																			
																	)--Salary Head 
														SH ON SH.SalaryHeadID=SPC.SalaryHeadID
                                                  INNER JOIN ( 
												  SELECT EESHE.EmpSystemId,EESHE.SalaryStructureId,EESHE.IsEligible,EESHE.SalaryHeadEnum,SalStruc.SalaryRuleMasterSystemID FROM 
                                                   (
												   
												   SELECT * from  (select SystemID SalaryId,EffectiveDate efd,EmpInfoSystemID eid ,SalaryRuleMasterSystemID from SalaryInfoDefineMaster
                                                                                 union
                                                                                 select SystemID SalaryId,EffectiveDate efd,EmpInfoSystemID eid ,SalaryRuleMasterSystemID from SalaryInfoBackMaster
                                                                                 )
                                                                                  mm 
                                                                                 inner join (
                                                                                 select MAX(EffectiveDate)EffectiveDate,EmpInfoSystemID from (
                                                                                 select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster where IsApproved=1 and EffectiveDate<='" + date + @"'
                                                                                 union
                                                                                  select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster where IsApproved=1 and EffectiveDate<='" + date + @"'
                                                                                  ) x 
                                                                                  group by EmpInfoSystemID
                                                                                 )m on mm.efd=m.EffectiveDate and m.EmpInfoSystemID=mm.eid
													) 
                        							SalStruc    INNER JOIN
                                               [EmployeeEligibleForSalaryHeadEnum] EESHE
                                                 
                                            ON EESHE.SalaryStructureId = SalStruc.SalaryId AND
                                                EESHE.EmpSystemId = SalStruc.EmpInfoSystemID WHERE EESHE.SalaryHeadEnum = 'ESIC' AND EESHE.PlantId = '" + plantId + @"' AND IsEligible = 1

                                                     ) 
													ESICELIGIBLE ON SPC.SalaryID = ESICELIGIBLE.SalaryStructureId and SPC.EmpInfoSystemID = ESICELIGIBLE.EmpSystemId
                                                         LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = ESICELIGIBLE.SalaryRuleMasterSystemID
                                                                LEFT JOIN CurrencyRuleMaster crm on crm.SystemID = sRM.CurrencyRuleSystemID
                                                                LEFT JOIN CurrencyRuleChild crc on crc.MstSystemID = CRM.SystemID and crc.SalaryHeadID=spc.SalaryHeadID			
														where  spld.PlantId = '" + plantId + @"'
											) EmpSlr ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID AND EmpBasic.PlantID = EmpSlr.PlantID
                                    LEFT JOIN
		                                    (
											 SELECT EmpSystemID, MonthNo, YearNo, TotalProcDate, TotalPresent, TotalLate,
													TotalAbsent, TotalLv, TotalMLv, TotalCompAssignLv, TotalWeekOff, TotalHoliDay,
													TotalWeekOffHoliDay, TotalOTHr, TotalNormalOTHr, TotalExtraOTHr
				                              FROM SalaryProceAttdnData
											  WHERE   MonthNo = MONTH(CONVERT(DATE,'" + date + @"')) AND
						                                YearNo = YEAR(CONVERT(DATE,'" + date + @"'))	 AND PlantId = '" + plantId + @"' 

											) MMDSA ON EmpSlr.EmpInfoSystemID = MMDSA.EmpSystemID 											   
													   WHERE 
														
													EmpSlr.MonthNo = " + monthName + @"  and EmpSlr.YearNo = YEAR(CONVERT(DATE,'" + date + @"'))
                                                    AND EmpBasic.PlantId = '" + plantId + @"' " + empStatus + @" ORDER BY EmployeeCodeS ";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSQL, out dsRef);
                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end function

        private decimal GetSalaryheadValue(DataView dvBioDvAC, string catType)
        {
            var basicValue = 0.00m;
            try
            {

                var basic = from r in dvBioDvAC.ToTable().AsEnumerable()
                            where r.Field<string>("cat") == catType
                            select r;
                if (basic.Count() > 0)
                {

                    DataTable dtt = basic.CopyToDataTable();
                    basicValue = Convert.ToDecimal(dtt.Rows[0]["DisbusmentAmount"].ToString());

                }
                return basicValue;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private bool GetSalaryheadDecimalType(DataView dvBioDvAC, string catType)
        {
            bool basicValue = true;
            try
            {

                var basic = from r in dvBioDvAC.ToTable().AsEnumerable()
                            where r.Field<string>("cat") == catType
                            select r;
                if (basic.Count() > 0)
                {

                    DataTable dtt = basic.CopyToDataTable();
                    basicValue = Convert.ToBoolean(dtt.Rows[0]["IntegerInDisb"].ToString());

                }
                return basicValue;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private int GetSalaryheadDecimalPoint(DataView dvBioDvAC, string catType)
        {
            int basicValue = 0;
            try
            {

                var basic = from r in dvBioDvAC.ToTable().AsEnumerable()
                            where r.Field<string>("cat") == catType
                            select r;
                if (basic.Count() > 0)
                {

                    DataTable dtt = basic.CopyToDataTable();
                    basicValue = Convert.ToInt32(dtt.Rows[0]["DecimalNo"].ToString());

                }
                return basicValue;
            }
            catch (Exception)
            {

                throw;
            }
        }
        string GetDecimalFormat(bool isInt, int decimalNo)
        {
            try
            {
                var ob = new ReportUtility();
                if (isInt == true)
                {
                    return ob.NumberFormatInt();
                }
                else
                {
                    return ob.GetDynamicDecimalPlace(decimalNo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GetEmployeeName(DataView dvBioDvAC, string EmpCode)
        {
            var Employeename = string.Empty;
            try
            {

                var EmployeList = from r in dvBioDvAC.ToTable().AsEnumerable()
                                  where r.Field<string>("EmployeeCode") == EmpCode
                                  select r;
                if (EmployeList.Count() > 0)
                {

                    DataTable dtt = EmployeList.CopyToDataTable();
                    Employeename = dtt.Rows[0]["EmployeeName"].ToString();

                }
                return Employeename;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private string GetWorkingDate(DataView dvBioDvAC, string EmpCode)
        {
            var WorkingDays = string.Empty;
            try
            {

                var workDaysList = from r in dvBioDvAC.ToTable().AsEnumerable()
                                   where r.Field<string>("EmployeeCode") == EmpCode
                                   select r;
                if (workDaysList.Count() > 0)
                {

                    DataTable dtt = workDaysList.CopyToDataTable();
                    WorkingDays = dtt.Rows[0]["workingDays"].ToString();

                }
                return WorkingDays;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private string GetPFNo(DataView dvBioDvAC, string EmpCode)
        {
            var docNo = string.Empty;
            try
            {

                var empList = from r in dvBioDvAC.ToTable().AsEnumerable()
                              where r.Field<string>("EmployeeCode") == EmpCode
                              select r;
                if (empList.Count() > 0)
                {

                    DataTable dtt = empList.CopyToDataTable();
                    docNo = dtt.Rows[0]["DocNumber"].ToString();

                }
                return docNo;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private string GetEmpAge(DataView dvBioDvAC, string EmpCode)
        {
            var Age = string.Empty;
            try
            {

                var AgeList = from r in dvBioDvAC.ToTable().AsEnumerable()
                              where r.Field<string>("EmployeeCode") == EmpCode
                              select r;
                if (AgeList.Count() > 0)
                {

                    DataTable dtt = AgeList.CopyToDataTable();
                    Age = dtt.Rows[0]["Age"].ToString();

                }
                return Age;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void SetSLText(ref IWorksheet sheet, int row, int col, int txt)
        {
            sheet.Range[row, col].Number = txt;
            sheet.Range[row, col].NumberFormat = NumberFormatIntWithComma();
            //sheet.Range[row, col].ColumnWidth = 15;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Hair);
        }
        public string NumberFormatIntWithComma()
        {
            return "#,#,#0;";
        }

        private void getTotal(ref IWorksheet sheet1, int xlsRow, int xlsCol, int Row_Total_Start, int Row_Total_end, ReportUtility ru)
        {
            try
            {

                sheet1.Range[xlsRow, xlsCol].Formula = "=SUM(" + ru.GetColumnNameForXls(xlsCol) + Row_Total_Start + ":" + ru.GetColumnNameForXls(xlsCol) + (Row_Total_end) + ")";
                sheet1.Range[xlsRow, xlsCol].NumberFormat = ru.NumberFormatDecimalFour();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}