using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.Helpers.ReportUtility;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class LeaveWithWeagesRegistersController : BaseController
    {
        #region Constructor

        private readonly IAttendanceManagementService _AttendanceManagementService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;
        public LeaveWithWeagesRegistersController(
              IAttendanceManagementService AttendanceManagementService, IEmployeeProfileService employeeProfileService, ISqlRepository R
            )
        {
            _AttendanceManagementService = AttendanceManagementService;
            _employeeProfileService = employeeProfileService;
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region Leave With Weages Registers    

        [HttpGet,Authorize]
        public ActionResult GetLeaveWithWeagesRegisters(ReportFormat reportFormat, string year, string empId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            #region Variable

            clsReport objRpt = null;
            int slCount = 0;

            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsMonth = null;
            DataSet dsEmpAttdn = null;
            DataTable dtEmpAttdn = null;
            DataTable dtEmployeeWiseLeaveTransactions = null;

            DataSet dsEmpSalary = null;
            DataTable dtEmpSalary = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            #endregion Variable

            try
            {
                ru = new ReportUtility();

                objRpt = new clsReport();

                #region Variable
                ParamList para = new ParamList();
                ParamList leavePara = new ParamList();
                ParamList attdnProcessParam = new ParamList();

                var FactoryName = "";
                var CmpName = "";

                para.PlantId = identity.PlantId;


                #endregion Variable

                #region DataSet
                string fromYear = year;
                string toYear = year;
                object totalLeaveDaysObj = null;
                if (toYear != fromYear)
                {
                    throw new Exception("Date must be in same year");
                }
                else
                {
                    //objRpt.GetFiscalMonthListSql(FromDate, ToDate, out dsMonth);
                    objRpt.GetEmployeeData(year, empId, identity.PlantId, out dsEmpAttdn);
                    dtEmpAttdn = dsEmpAttdn.Tables[0];

                    objRpt.GetEmployeeSalaryData(year, empId, identity.PlantId, out dsEmpSalary);
                    dtEmpSalary = dsEmpSalary.Tables[0];

                    objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                    objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                    DataTable dtLeavePolicy = GetDtLeavePolicy(identity.PlantId, empId);
                    DataTable dtTotalPayDays = dtTotalPayDaysYearly(identity.PlantId, year, empId);
                    DataTable dtTotalHalfDayPresent = dtHalfDayPresentYearly(identity.PlantId, year, empId);
                    DataTable dtTotalHalfDayLeave = dtHalfDayLeaveYearly(identity.PlantId, year, empId);


                    if(dtLeavePolicy.Rows.Count == 0)
                    {
                        throw new Exception("Please add Leave Policy!!!");
                    }


                    DataTable dtTotalPayDaysMonthly = null;
                    DataTable dtTotalHalfDayPresentMonthly = null;
                    DataTable dtTotalHalfDayLeaveMonthly = null;





                    #endregion DataSet

                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;

                    #region------------------Column Header------------------
                    xlsRow = 5;
                    xlsCol = 1;

                    var colSr = 0;
                    var colWP = 0;
                    var colWE = 0;
                    var colNoW = 0;
                    var colNoLO = 0;
                    var colNoML = 0;
                    var colNoMLT = 0;
                    var colBL = 0;
                    var colEL = 0;
                    var colFrom = 0;
                    var colTo = 0;
                    var colscheme = 0;
                    var colNOD = 0;
                    var colBLC = 0;
                    var colNormal = 0;
                    var colTotal4and7 = 0;
                    var colTotal9and10 = 0;

                    var coladvantage = 0;
                    var colLP = 0;
                    var colRemarks = 0;
                    var colEmpName = 0;
                    var colTotalAmount = 0;
                    var colBonusPercentage = 0;
                    var colBonusAmount = 0;
                    var colDOS = 0;
                    var colWageLabel = 0;
                    var colWageLabel1 = 0;
                    var colWageLabel2 = 0;
                    var colLeaveEarned = 0;
                    var leaveCreditBalance = 0.00;
                    double wageDisbusmentAmount = 0.00;
                    double totalwageDisbusmentAmount = 0.00;
                    double wageEntryAmount = 0.00;
                    double totalWageEntryAmount = 0.00;
                    double totalPayDays = 0.00;
                    double totalLeaveDays = 0.00;
                    double _BroughtForward = 0.00;
                    double _CurrentYearAllocation = 0.00;
                    double _CurrentYearAvailedOpeningBalance = 0.00;
                    double _BroughtForwardCumulitive = 0.00;


                    #endregion------------------Column Header------------------
                    xlsRow = 5;
                    xlsRow++;

                    var oRU = new ReportUtility();
                    _BroughtForward = clsStaticInfo.dbl(dtEmpAttdn.DefaultView[0]["BroughtForward"].ToString());
                    _CurrentYearAllocation = clsStaticInfo.dbl(dtEmpAttdn.DefaultView[0]["CurrentYearAllocationAsPerPolicy"].ToString());
                    _CurrentYearAvailedOpeningBalance = clsStaticInfo.dbl(dtEmpAttdn.DefaultView[0]["CurrentYearAvailedOpeningBalance"].ToString());

                    double totalyearlyPayDays = 0.00;
                    double totalyearlyHalfPresentDays = 0.00;
                    double totalyearlyHalfLeaveDays = 0.00;

                    if(dtTotalPayDays.Rows.Count > 0)
                    {
                         totalyearlyPayDays = clsStaticInfo.dbl(dtTotalPayDays.Rows[0]["TotalPayDays"].ToString());

                    }
                    if (dtTotalHalfDayPresent.Rows.Count > 0)
                    {
                        totalyearlyHalfPresentDays = clsStaticInfo.dbl(dtTotalHalfDayPresent.Rows[0]["TotalHalfPresent"].ToString());
                    }
                    if (dtTotalHalfDayLeave.Rows.Count > 0)
                    {
                        totalyearlyHalfLeaveDays = clsStaticInfo.dbl(dtTotalHalfDayLeave.Rows[0]["TotalHalfLeave"].ToString());
                    }

                    double totalPayDaysUpdated = (totalyearlyPayDays + (totalyearlyHalfPresentDays / 2)) - (totalyearlyHalfLeaveDays / 2);

                    double totalEarnLeave = (totalPayDaysUpdated / clsStaticInfo.dbl(dtLeavePolicy.Rows[0]["EncashWorkingDaysQty"].ToString())); // 20 will come from policy

                    totalEarnLeave = (double)GetRoundValue(dtLeavePolicy.Rows[0]["LeaveCalculationRoundOption"].ToString(), Convert.ToDecimal(totalEarnLeave));

                    xlsRow++;
                    SetHeaderValue("Calender Year of Service", sheet1, xlsRow, ref xlsCol, out colSr, 6, -1, 0);  // 1
                    SetHeaderValue("Wages Period FROM TO", sheet1, xlsRow, ref xlsCol, out colWP, 6, -1, 0);       // 2
                    SetHeaderValue("Wages Earned during the wages period", sheet1, xlsRow, ref xlsCol, out colWE, 10);    // 3

                    SetHeaderValue("No. of days work performed", sheet1, xlsRow, ref xlsCol, out colNoW, 6);    // 4
                    SetHeaderValue("No. of days lays Off", sheet1, xlsRow, ref xlsCol, out colNoLO, 6);          // 5
                    SetHeaderValue("No. of days of maternity leave", sheet1, xlsRow, ref xlsCol, out colNoML, 6);   // 6
                    SetHeaderValue("No. of days of leave enjoyed E/L", sheet1, xlsRow, ref xlsCol, out colEL, 6);   // 7
                    SetHeaderValue("Total of columns 4 to 7", sheet1, xlsRow, ref xlsCol, out colTotal4and7, 6, -1, 0);    // 8
                    SetHeaderValue("Balance of leave from preceeding year", sheet1, xlsRow, ref xlsCol, out colBL, 6);    // 9
                    SetHeaderValue("Leave earned during the year mentioned in col.", sheet1, xlsRow, ref xlsCol, out colLeaveEarned, 8);   // 10
                    SetHeaderValue("Total of columns 9 to 10", sheet1, xlsRow, ref xlsCol, out colTotal9and10, 6, -1, 0);                // 11
                    SetHeaderValue("Whether leave in accordence with scheme under Sec 79(8) was refused", sheet1, xlsRow, ref xlsCol, out colscheme, 10, -1, 0);   // 12
                    SetHeaderValueRotationLess("From", sheet1, xlsRow, ref xlsCol, out colFrom, 13);  // 13
                    SetHeaderValueRotationLess("To", sheet1, xlsRow, ref xlsCol, out colTo, 13);   // 14
                    SetHeaderValue("No. of days", sheet1, xlsRow, ref xlsCol, out colNOD, 6);   // 15
                    SetHeaderValue("Balance of leave or credit", sheet1, xlsRow, ref xlsCol, out colBLC, 6, -1, 0);   //  16
                    SetHeaderValue("Normal rate of wages per month/P.W.D", sheet1, xlsRow, ref xlsCol, out colNormal, 8, -1, 0);  // 17
                    SetHeaderValue("Cash equivalent of advantage according through concesstional rate of food against other articles", sheet1, xlsRow, ref xlsCol, out coladvantage, 14, -1, 0);   // 18
                    SetHeaderValue("Rate of wages for the leave period. (total other articles)", sheet1, xlsRow, ref xlsCol, out colLP, 8, -1, 0);   // 19
                    SetHeaderValue(" ", sheet1, xlsRow, ref xlsCol, out colWageLabel, 6, -1, 0);  // 20
                    SetHeaderValue(" ", sheet1, xlsRow, ref xlsCol, out colWageLabel1, 6, -1, 0);  // 21
                    SetHeaderValue(" ", sheet1, xlsRow, ref xlsCol, out colWageLabel2, 6, -1, 0);   // 22
                    SetHeaderValue("Remarks", sheet1, xlsRow, ref xlsCol, out colRemarks, 6, -1, 0);   // 23



                    #region Merged Cells

                    sheet1.Range[xlsRow - 1, colWE].Text = "No. of days worked during the calender year.";
                    sheet1.Range[xlsRow - 1, colWE].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow - 1, colWE].RowHeight = 34;

                    sheet1.Range[xlsRow - 1, colWE].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow - 1, colWE].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow - 1, colWE, xlsRow - 1, colEL].BorderAround(ExcelLineStyle.Thin);
                    sheet1.Range[xlsRow - 1, colWE, xlsRow - 1, colEL].Merge();

                    sheet1.Range[xlsRow - 1, colBL].Text = "Leave of Credit.";
                    sheet1.Range[xlsRow - 1, colBL].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow - 1, colBL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow - 1, colBL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow - 1, colBL, xlsRow - 1, colLeaveEarned].BorderAround(ExcelLineStyle.Thin);
                    sheet1.Range[xlsRow - 1, colBL, xlsRow - 1, colLeaveEarned].Merge();
                    sheet1.Range[xlsRow - 1, colFrom].Text = "Leave Enjoyed.";
                    sheet1.Range[xlsRow - 1, colFrom].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow - 1, colFrom].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow - 1, colFrom].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow - 1, colFrom, xlsRow - 1, colNOD].BorderAround(ExcelLineStyle.Thin);
                    sheet1.Range[xlsRow - 1, colFrom, xlsRow - 1, colNOD].Merge();

                    xlsRow++;
                    for (int col = 1; col <= colRemarks; col++)
                    {
                        SetTextValueWithoutRefernce(col.ToString(), sheet1, xlsRow, col);
                    }

                    #endregion

                    endXlsCol = colRemarks;
                    var fPanRow = xlsRow;
                    xlsRow++;
                    oRU.SetCellTextBold(sheet1, xlsRow, 1, "B/F");

                    oRU.SetCellTextBold(sheet1, xlsRow, colBL, Convert.ToDouble(_BroughtForward));
                    oRU.SetCellTextBold(sheet1, xlsRow, colLeaveEarned, Convert.ToDouble(totalEarnLeave));
                    leaveCreditBalance = _BroughtForward +  totalEarnLeave;
                    _BroughtForwardCumulitive = leaveCreditBalance;
                    //_CurrentYearAvailedOpeningBalance
                    oRU.SetCellTextBold(sheet1, xlsRow, colNOD, Convert.ToDouble(_CurrentYearAvailedOpeningBalance));

                    oRU.SetCellTextBold(sheet1, xlsRow, colBLC, Convert.ToDouble(leaveCreditBalance));
                    //sheet1.Range[xlsRow, colLeaveEarned].Text = _CurrentYearAllocation.ToString(); // 23
                    sheet1.Range[xlsRow, colLeaveEarned].CellStyle.Font.Bold = true;
                    oRU.SetCellTextBold(sheet1, xlsRow, colTotal9and10, clsStaticInfo.dbl((_BroughtForward + totalEarnLeave).ToString()));

                    //sheet1.Range[xlsRow, colTotal9and10].Text = .ToString(); // 23


                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    double _paydays = 0.00;
                    double paydaysFromDB = 0.00;

                    double _halfPaydays = 0.00;
                    double _halfLeavedays = 0.00;
                    double _leaveDays = 0.00;
                    


                    #region DataSet
                    string empSystemId = dtEmpAttdn.DefaultView[0]["EmpSystemId"].ToString();
                    for (int i = 1; i <= 12; i++)
                    {

                        string monthName = new DateTime(Convert.ToInt32(year), i, 1).ToString("MMM", CultureInfo.InvariantCulture);
                        var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(i));//Number of Days in a month
                        string fromDate = 1 + "-" + monthName + "-" + year;
                        string toDate = daysInMonth + "-" + monthName + "-" + year;

                        dtEmployeeWiseLeaveTransactions = getEmployeeWiseLeaveTransactions(empSystemId, fromDate, toDate);

                        dtTotalPayDaysMonthly = dtEmpWiseLeavePayDaysMonthly(empSystemId, i.ToString(), year, identity.PlantId);
                        dtTotalHalfDayPresentMonthly = dtHalfDayPresentMonthly(empSystemId, i.ToString(), year, identity.PlantId);
                        dtTotalHalfDayLeaveMonthly = dtHalfDayLeaveMonthly(empSystemId, i.ToString(), year, identity.PlantId);


                        #region ----------------------Data-----------------------
                         _paydays = 0.00;
                         paydaysFromDB = 0.00;

                         _halfPaydays = 0.00;
                         _halfLeavedays = 0.00;

                         _leaveDays = 0.00;

                        dtEmpAttdn.DefaultView.RowFilter = "MonthName='" + monthName + "'";
                        if (dtTotalPayDaysMonthly.DefaultView.Count > 0)
                        {
                            if (dtEmployeeWiseLeaveTransactions.Rows.Count > 0)
                            {
                                totalLeaveDaysObj = dtEmployeeWiseLeaveTransactions.Compute(@"Sum(LeaveDays)", "");
                                _leaveDays = Convert.ToDouble(totalLeaveDaysObj);
                            }

                            if (dtTotalPayDaysMonthly.Rows.Count > 0)
                            {
                                paydaysFromDB = clsStaticInfo.dbl(dtTotalPayDaysMonthly.Rows[0]["TotalPayDays"].ToString());

                            }
                            if (dtTotalHalfDayPresentMonthly.Rows.Count > 0)
                            {
                                _halfPaydays = clsStaticInfo.dbl(dtTotalHalfDayPresentMonthly.Rows[0]["TotalHalfPresent"].ToString());
                            }
                            if (dtTotalHalfDayLeaveMonthly.Rows.Count > 0)
                            {
                                _halfLeavedays = clsStaticInfo.dbl(dtTotalHalfDayLeaveMonthly.Rows[0]["TotalHalfLeave"].ToString());
                            }

                            //paydaysFromDB = clsStaticInfo.dbl(dtTotalPayDaysMonthly.DefaultView[0]["TotalPayDays"].ToString());
                            //_halfPaydays = clsStaticInfo.dbl(dtTotalHalfDayPresentMonthly.DefaultView[0]["TotalPayDays"].ToString());
                           // _halfLeavedays = clsStaticInfo.dbl(dtTotalHalfDayLeaveMonthly.DefaultView[0]["TotalPayDays"].ToString());
                            _paydays = (paydaysFromDB + (_halfPaydays / 2)) - _halfLeavedays / 2;

                            totalPayDays += _paydays;
                            totalLeaveDays += _leaveDays;
                        }

                        xlsRow += 1;
                        sheet1.Range[xlsRow, colSr].Text = toYear.ToString();                           //1
                        sheet1.Range[xlsRow, colSr].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colWP].Text = monthName;  //monthname   // 2
                        sheet1.Range[xlsRow, colWP].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colWP].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        DataView dvWages = new DataView(dsEmpSalary.Tables[0]);
                        wageDisbusmentAmount = 0.00;
                        wageEntryAmount = 0.00;
                        dvWages.RowFilter = "MonthNameVal = '" + monthName + "' and EmpInfoSystemID ='" + dtEmpAttdn.Rows[0]["EmpSystemId"].ToString() + "'";


                        if (dvWages.Count > 0)
                        {
                            wageDisbusmentAmount = Convert.ToDouble(dvWages[0]["DisbusmentAmount"]);
                            totalwageDisbusmentAmount += Convert.ToDouble(dvWages[0]["DisbusmentAmount"]);
                            wageEntryAmount = Convert.ToDouble(dvWages[0]["EntryAmount"]);
                            totalWageEntryAmount += Convert.ToDouble(dvWages[0]["EntryAmount"]);
                        }
                        sheet1.Range[xlsRow, colWE].Number = wageDisbusmentAmount;  //payday  //4
                        sheet1.Range[xlsRow, colWE].NumberFormat = oRU.NumberFormatDecimalZero();  //payday  //4
                        sheet1.Range[xlsRow, colWE].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colWE].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, colNoW].Number = _paydays;  //payday  //4
                        sheet1.Range[xlsRow, colNoW].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colNoW].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        // sheet1.Range[xlsRow, colNoLO].Text = "";//dtEmpAttdn.Rows[i][""].ToString().Substring(0, 3);  //nolayoff  //5
                        sheet1.Range[xlsRow, colNoLO].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colNoLO].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        // sheet1.Range[xlsRow, colNoML].Text = dtEmpAttdn.Rows[i][""].ToString();   // nomlv  //6
                        sheet1.Range[xlsRow, colNoML].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colNoML].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        sheet1.Range[xlsRow, colEL].Number = _leaveDays;   /// no el  //7
                        sheet1.Range[xlsRow, colEL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colEL].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        //sheet1.Range[xlsRow, colNoW].Text = dtEmpAttdn.Rows[i]["PayDays"].ToString();   // nototal  //8
                        sheet1.Range[xlsRow, colNoW].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colNoW].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //sheet1.Range[xlsRow, colBL].Number = _BroughtForward;   /// no brought  //9
                        sheet1.Range[xlsRow, colBL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colBL].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        //  sheet1.Range[xlsRow, colduring].Text = dtEmpAttdn.Rows[i]["DaysCanBeSanctioned"].ToString();   // earnleave  //10
                        sheet1.Range[xlsRow, colLeaveEarned].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colLeaveEarned].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, colTotal4and7].Number = _paydays + _leaveDays;  // totalcolumn   //11
                        sheet1.Range[xlsRow, colTotal4and7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colTotal4and7].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        //   sheet1.Range[xlsRow, colscheme].Text = dtEmpAttdn.Rows[i][""].ToString();  //schema  //12
                        sheet1.Range[xlsRow, colscheme].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colscheme].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        string fromLeave = "";
                        string toLeave = "";
                        double totalLeaves = 0.00;
                        if (dtEmployeeWiseLeaveTransactions.Rows.Count > 0)
                        {
                            sheet1.Range[xlsRow, colFrom].RowHeight = 55;
                            for (int lt = 0; lt < dtEmployeeWiseLeaveTransactions.Rows.Count; lt++)
                            {
                                fromLeave += dtEmployeeWiseLeaveTransactions.Rows[lt]["FromDate"].ToString() + Environment.NewLine;
                                toLeave += dtEmployeeWiseLeaveTransactions.Rows[lt]["ToDate"].ToString() + Environment.NewLine;
                                totalLeaves += Convert.ToDouble(dtEmployeeWiseLeaveTransactions.Rows[lt]["LeaveDays"]);
                                _BroughtForwardCumulitive -= Convert.ToDouble(dtEmployeeWiseLeaveTransactions.Rows[lt]["LeaveDays"]);
                            }
                        }
                        sheet1.Range[xlsRow, colFrom].Text = fromLeave;  // from   //13

                        //sheet1.Range[xlsRow, colFrom].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //sheet1.Range[xlsRow, colFrom].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, colTo].Text = toLeave;  //to  //14
                        //sheet1.Range[xlsRow, colTo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //sheet1.Range[xlsRow, colTo].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if(wageEntryAmount > 0 || totalLeaves > 0)
                        {
                            sheet1.Range[xlsRow, colNOD].Text = totalLeaves.ToString();  // noofdays   //15
                            sheet1.Range[xlsRow, colNOD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, colNOD].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, colBLC].Text = _BroughtForwardCumulitive.ToString();   //balance of leave  //16
                            sheet1.Range[xlsRow, colBLC].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, colBLC].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }

                        sheet1.Range[xlsRow, colNormal].Number = wageEntryAmount;  // normal rate pwd   //17
                        sheet1.Range[xlsRow, colNormal].NumberFormat = oRU.NumberFormatDecimalZero();
                        sheet1.Range[xlsRow, colNormal].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colNormal].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //   sheet1.Range[xlsRow, coladvantage].Text = dtEmpAttdn.Rows[i][""].ToString();  // cash  //18
                        sheet1.Range[xlsRow, coladvantage].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, coladvantage].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //   sheet1.Range[xlsRow, colLP].Text = dtEmpAttdn.Rows[i][""].ToString();  //rate of wages   //19
                        sheet1.Range[xlsRow, colLP].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colLP].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        //   sheet1.Range[xlsRow, colWageLabel].Text = dtEmpAttdn.Rows[i][""].ToString();  //20
                        sheet1.Range[xlsRow, colWageLabel].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colWageLabel].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        //   sheet1.Range[xlsRow, colWageLabel].Text = dtEmpAttdn.Rows[i][""].ToString(); // 21
                        sheet1.Range[xlsRow, colWageLabel1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colWageLabel1].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        //   sheet1.Range[xlsRow, colWageLabel2].Text = dtEmpAttdn.Rows[i][""].ToString(); // 22
                        sheet1.Range[xlsRow, colWageLabel2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colWageLabel2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //   sheet1.Range[xlsRow, colRemarks].Text = dtEmpAttdn.Rows[i][""].ToString(); // 23
                        sheet1.Range[xlsRow, colRemarks].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colRemarks].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet1.Range[xlsRow, 1, xlsRow, colRemarks].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow, 1, xlsRow, colRemarks].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow, 1, xlsRow, colRemarks].WrapText = true;

                        #endregion Line Setup
                    }
                    endXlsCol = colRemarks;
                    xlsRow++;
                    sheet1.Range[xlsRow, 1].Text = "Total"; // 23 colNOD
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true; // 23 colNOD
                    sheet1.Range[xlsRow, 1, xlsRow, 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, 2].Merge();
                    sheet1.Range[xlsRow, 1].RowHeight = 15;


                    sheet1.Range[xlsRow, colWE].Text = totalwageDisbusmentAmount.ToString(); // 23 colNOD
                    sheet1.Range[xlsRow, colWE].NumberFormat = oRU.NumberFormatDecimalZero();

                    sheet1.Range[xlsRow, colWE].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, colWE].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, colNormal].Number = totalWageEntryAmount; // 23
                    sheet1.Range[xlsRow, colNormal].NumberFormat = oRU.NumberFormatDecimalZero();

                    sheet1.Range[xlsRow, colNormal].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, colNormal].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, colNoW].Number = totalPayDays;  //payday  //4
                    sheet1.Range[xlsRow, colNoW].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, colNoW].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, colEL].Text = totalLeaveDays.ToString(); // 23
                    sheet1.Range[xlsRow, colEL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, colEL].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, colTotal4and7].Text = (totalPayDays + totalLeaveDays).ToString(); // 23
                    sheet1.Range[xlsRow, colTotal4and7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, colTotal4and7].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, colNOD].Text = totalLeaveDays.ToString(); // 23
                    sheet1.Range[xlsRow, colNOD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, colNOD].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, colBL].Text = _BroughtForward.ToString(); // 23
                    sheet1.Range[xlsRow, colBL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, colBL].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, colLeaveEarned].Text = _CurrentYearAllocation.ToString(); // 23
                    sheet1.Range[xlsRow, colLeaveEarned].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, colLeaveEarned].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, colTotal9and10].Text = (_BroughtForward + _CurrentYearAllocation).ToString(); // 23
                    sheet1.Range[xlsRow, colTotal9and10].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, colTotal9and10].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, colBLC].Text = (_BroughtForwardCumulitive + _CurrentYearAllocation).ToString(); // 23
                    sheet1.Range[xlsRow, colBLC].Text = (_BroughtForwardCumulitive).ToString(); // 23

                    sheet1.Range[xlsRow, colBLC].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, colBLC].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                    xlsRow++;
                    sheet1.Range[xlsRow, 1].Text = "Signature of Employee Recieve Leave Book";
                    sheet1.Range[xlsRow, 1].RowHeight = 28;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 13;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    #endregion


                    #region ******************Report Header******************
                    #region Left side Emp Information


                    xlsCol = 1;
                    xlsRow = 1;

                    sheet1.Range[xlsRow, xlsCol].Text = "Department";
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();

                    sheet1.Range[xlsRow, xlsCol + 2].Text = ": " + dtEmpAttdn.Rows[0]["Department"].ToString().Trim();
                    sheet1.Range[xlsRow, xlsCol + 2, xlsRow, xlsCol + 6].Merge();
                    sheet1.Range[xlsRow, xlsCol + 2, xlsRow, xlsCol + 6].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin; ;


                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "S.No. in Register Adult/Child Worker";
                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].Merge();


                    sheet1.Range[xlsRow, xlsCol + 4].Text = "";
                    sheet1.Range[xlsRow, xlsCol + 4, xlsRow, xlsCol + 6].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, xlsCol + 4, xlsRow, xlsCol + 6].Merge();
                    sheet1.Range[xlsRow, xlsCol + 4, xlsRow, xlsCol + 6].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin; ;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Date of Join";
                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].Merge();

                    sheet1.Range[xlsRow, xlsCol + 4].Text = ": " + dtEmpAttdn.Rows[0]["DOJ"].ToString().Trim();
                    sheet1.Range[xlsRow, xlsCol + 4, xlsRow, xlsCol + 6].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, xlsCol + 4, xlsRow, xlsCol + 6].Merge();
                    sheet1.Range[xlsRow, xlsCol + 4, xlsRow, xlsCol + 6].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin; ;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Date of Separation";
                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].Merge();
                    sheet1.Range[xlsRow, xlsCol + 4].Text = ": " + dtEmpAttdn.Rows[0]["DOS"].ToString().Trim();
                    sheet1.Range[xlsRow, xlsCol + 4, xlsRow, xlsCol + 6].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, xlsCol + 4, xlsRow, xlsCol + 6].Merge();
                    sheet1.Range[xlsRow, xlsCol + 4, xlsRow, xlsCol + 6].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin; ;

                    xlsRow += 1;
                    #endregion

                    ///------------------

                    xlsCol = 1;
                    xlsRow = 1;
                    sheet1.Range[xlsRow, endXlsCol - 8].Text = "Emp Code";
                    sheet1.Range[xlsRow, endXlsCol - 8, xlsRow, endXlsCol - 5].Merge();

                    sheet1.Range[xlsRow, endXlsCol - 4].Text = dtEmpAttdn.Rows[0]["EmployeeCode"].ToString().Trim();
                    sheet1.Range[xlsRow, endXlsCol - 4, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, endXlsCol - 4, xlsRow, endXlsCol].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin; ;

                    //sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    //sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    xlsRow += 1;
                    sheet1.Range[xlsRow, endXlsCol - 8].Text = "Name";
                    sheet1.Range[xlsRow, endXlsCol - 8, xlsRow, endXlsCol - 5].Merge();
                    sheet1.Range[xlsRow, endXlsCol - 4].Text = dtEmpAttdn.Rows[0]["EmployeeName"].ToString().Trim();
                    sheet1.Range[xlsRow, endXlsCol - 4, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, endXlsCol - 4, xlsRow, endXlsCol].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin; ;


                    xlsRow += 1;
                    sheet1.Range[xlsRow, endXlsCol - 8].Text = "Father's Name";
                    sheet1.Range[xlsRow, endXlsCol - 8, xlsRow, endXlsCol - 5].Merge();

                    sheet1.Range[xlsRow, endXlsCol - 4].Text = dtEmpAttdn.Rows[0]["FatherName"].ToString().Trim();
                    sheet1.Range[xlsRow, endXlsCol - 4, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, endXlsCol - 4, xlsRow, endXlsCol].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin; ;

                    if(!string.IsNullOrEmpty(dtEmpAttdn.Rows[0]["ContractorId"].ToString().Trim()))
                    {
                        xlsRow += 1;
                        sheet1.Range[xlsRow, endXlsCol - 8].Text = "Contractor";
                        sheet1.Range[xlsRow, endXlsCol - 8, xlsRow, endXlsCol - 5].Merge();

                        sheet1.Range[xlsRow, endXlsCol - 4].Text = dtEmpAttdn.Rows[0]["ContractorName"].ToString().Trim();
                        sheet1.Range[xlsRow, endXlsCol - 4, xlsRow, endXlsCol].Merge();
                        sheet1.Range[xlsRow, endXlsCol - 4, xlsRow, endXlsCol].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin; ;

                    }

                    xlsRow += 1;
                    sheet1.Range[xlsRow, endXlsCol - 8].Text = "Date & Amount of payment made in lieu of leave due ";
                    sheet1.Range[xlsRow, endXlsCol - 8, xlsRow, endXlsCol - 3].Merge();
                    xlsRow++;

                    objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                    xlsRow = 1;
                    xlsCol = 1;


                    sheet1.Range[xlsRow, xlsCol + 7].Text = "Form No.15 (Rule No. 94)";
                    sheet1.Range[xlsRow, xlsCol + 7, xlsRow, endXlsCol - 9].Merge();
                    sheet1.Range[xlsRow, xlsCol + 7].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol + 7].CellStyle.Font.Size = 12;
                    sheet1.Range[xlsRow, xlsCol + 7, xlsRow, endXlsCol - 9].RowHeight = 18;
                    sheet1.Range[xlsRow, xlsCol + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, xlsCol + 7, xlsRow, endXlsCol - 9].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    xlsRow++;
                    FactoryName = string.Empty;
                    var FactoryAddress = string.Empty;
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet1.Range[xlsRow, xlsCol + 7].Text = CmpName;
                    sheet1.Range[xlsRow, xlsCol + 7, xlsRow, endXlsCol - 9].Merge();
                    sheet1.Range[xlsRow, xlsCol + 7].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol + 7].CellStyle.Font.Size = 12;
                    sheet1.Range[xlsRow, xlsCol + 7, xlsRow, endXlsCol - 9].RowHeight = 18;
                    sheet1.Range[xlsRow, xlsCol + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, xlsCol + 7, xlsRow, endXlsCol - 9].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    //xlsRow += 1;
                    //if (dsCmp.Tables[0].Rows.Count > 0)
                    //{
                    //    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                    //}
                    //else
                    //{
                    //    FactoryName = "";
                    //}
                    //sheet1.Range[xlsRow, xlsCol + 4].Text = FactoryName;
                    //sheet1.Range[xlsRow, xlsCol + 4, xlsRow, endXlsCol - 8].Merge();
                    //sheet1.Range[xlsRow, xlsCol + 4].CellStyle.Font.Size = 10;
                    //sheet1.Range[xlsRow, xlsCol + 4, xlsRow, endXlsCol - 8].RowHeight = 13;
                    //sheet1.Range[xlsRow, xlsCol + 4].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1.Range[xlsRow, xlsCol + 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, xlsCol + 4, xlsRow, endXlsCol - 8].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet1.Range[xlsRow, xlsCol + 7].Text = FactoryAddress;
                    sheet1.Range[xlsRow, xlsCol + 7, xlsRow, endXlsCol - 9].Merge();
                    sheet1.Range[xlsRow, xlsCol + 7].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, xlsCol + 7, xlsRow, endXlsCol - 9].RowHeight = 13;
                    sheet1.Range[xlsRow, xlsCol + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, xlsCol + 7, xlsRow, endXlsCol - 9].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol + 7].Text = "Leave With Weages Registers";
                    sheet1.Range[xlsRow, xlsCol + 7, xlsRow, endXlsCol - 9].Merge();
                    sheet1.Range[xlsRow, xlsCol + 7].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, xlsCol + 7, xlsRow, endXlsCol - 9].RowHeight = 18;
                    sheet1.Range[xlsRow, xlsCol + 7].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol + 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol + 7].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, xlsCol + 7, xlsRow, endXlsCol - 9].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    #endregion ******************Report Header******************

                    #region Freeze Panes
                    sheet1.UsedRange["A" + fPanRow].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 5;

                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
                    sheet1.PageSetup.PrintTitleRows = "$A$5:$IV$5";
                    sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    sheet1.PageSetup.LeftMargin = 0.5;
                    sheet1.PageSetup.RightMargin = 0.2;
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet1.IsDisplayZeros = false;

                    sheet1.Name = "LeaveWithWeagesRegisters" + para.SalaryProcessId;
                    #endregion

                    workbook.Version = ExcelVersion.Excel97to2003;
                    var reportFileName = DateTime.Now.ToString("yyMMdd") + " " + "LeaveWithWeagesRegisters";
                    //string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                    //workbook.SaveAs(fullPath);

                    switch (reportFormat)
                    {
                        case ReportFormat.Pdf:
                            return RenderReportAsPdf(workbook, reportFileName);

                        case ReportFormat.Excel:
                            return RenderReportAsExcel(workbook, reportFileName);
                       
                        default:
                            return RenderReportAsExcel(workbook, reportFileName);
                            //default:
                            //    return View();
                    }


                }
            }

            catch (Exception ex)
            {

                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }

        static decimal GetRoundValue(string LeaveCalculationRoundOption, decimal Input)
        {
            decimal r = 0;
            try
            {
                Input =Convert.ToDecimal( Convert.ToDouble(Input).ToString("F10"));// addded By Mamun-- Powered By Mr Tareq (For the value 1.9  In the case of EXACT it is throwing "Index and length must refer to a location within the string. Parameter name: length")

                if (string.IsNullOrEmpty(LeaveCalculationRoundOption))
                {
                    var _product = Math.Round(Input, 2);
                    r = Math.Round(_product);
                }
                else
                {
                    if (LeaveCalculationRoundOption.ToUpper() == "ROUND")
                    {
                        var _product = Math.Round(Input, MidpointRounding.AwayFromZero);
                        r = Math.Round(_product, MidpointRounding.AwayFromZero);
                    }
                    else if (LeaveCalculationRoundOption.ToUpper() == "ROUND UP")//no decimal value
                    {
                        var _product = Math.Ceiling(Input).ToString("0.00");
                        r = Math.Ceiling(Convert.ToDecimal(_product));
                    }
                    else if (LeaveCalculationRoundOption.ToUpper() == "ROUND DOWN")//no decimal value
                    {
                        var _product = Math.Floor(Input).ToString("0.00");
                        r = Math.Floor(Convert.ToDecimal(_product));
                    }
                    else if (LeaveCalculationRoundOption.ToUpper() == "EXACT")
                    {
                        string k = string.Empty;
                        int idx = Input.ToString().IndexOf(".");
                        if (idx != -1)
                        {
                            k = Input.ToString().Substring(0, idx + 3);
                        }
                        else
                        {
                            k = Input.ToString("0.00");
                        }

                        r = Convert.ToDecimal(k);
                    }
                    else//as the first one : Round
                    {
                        var _product = Math.Round(Input, 2, MidpointRounding.AwayFromZero);
                        r = Math.Round(_product, 2, MidpointRounding.AwayFromZero);
                    }
                }
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private DataTable dtTotalPayDaysYearly(string plantID, string year, string empSystemId)
        {
            try
            {
                string sql = "";
                sql = @"SELECT Count(Apd.DayStatus) TotalPayDays, EmpSystemID FROM EmployeeInformation EEI
				LEFT JOIN HKP.LegalDesignation LD ON LD.Id = EEI.LegalDesignationId
				LEFT JOIN MSt.DesignationMasterLegalDesignation DMLD ON LD.ID = DMLD.LegalDesignationId
				LEFT JOIN MSt.DesignationMaster DM ON DM.Id = DMLD.DesignationMasterId
				LEFT JOIN SCS.DesignationMasterConfiguration C ON C.DesignationMasterId = DM.Id and C.PlantId = '" + plantID + @"'
				LEFT JOIN LeavePolicyMaster LPM ON LPM.SystemID = C.LeavePolicyMasterId 
				LEFT JOIN LeavePolicyDetail LPD ON LPM.SystemID = LPD.LPMSystemID 
				LEFT JOIN (SELECT * FROM LeaveType where LeaveType = 'Earn') LT ON LT.ID = LPD.LTSystemID
				LEFT JOIN  LeavePolicyWorkingDays LPWD ON LPWD.LPDetailID = LPD.SystemID and LPWD.LPMasterID = LPM.SystemID
			    LEFT JOIN AttdnProcessData  APD ON APD.EmpSystemID = EEI.SystemId  and APD.DayStatus = LPWD.DayType
				where Year(Apd.WorkDate) = " + year + @" and EmpSystemID = '" + empSystemId + @"'
				Group By EmpSystemID";

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {

                throw;
            }
        }




        private DataTable dtHalfDayPresentYearly(string plantId, string year, string empSystemId)
        {
            try
            {
                string sql = "";
                sql = @"select EmpSystemID, Count(Apd.WorkDate) TotalHalfPresent from AttdnProcessData APD
                                    where Year(Apd.WorkDate) = " + year + @"
                                    and PlantID='" + plantId + @"' and DayStatus='HDP' and EmpSystemID = '" + empSystemId + @"'
                            Group by EmpSystemID
                                ";

                return _sqlRepository.GetDataTable(sql);

            }
            catch (Exception)
            {

                throw;
            }
        }
        private DataTable dtHalfDayLeaveYearly(string plantId, string year, string empSystemId)
        {
            try
            {
                string strSQL = "";
                strSQL = @" select EmpSystemID, Count(Apd.WorkDate) TotalHalfLeave from AttdnProcessData Apd
                                    WHERE Year(Apd.WorkDate) = " + year + @" and EmpSystemID = '" + empSystemId + @"'
                                    AND PlantID='" + plantId + @"' AND DayStatus<>'HDP' AND IsHalfDayLeave=1 
                                    AND DayStatus in
                                    (select DayType FROM LeavePolicyWorkingDays WHERE LPDetailID in
						             (SELECT SystemID FROM [LeavePolicyDetail] WHERE  LTSystemID=
						             (select id from LeaveType where LeaveType='Earn')))
                                        GROUP BY EmpSystemID ";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (Exception)
            {

                throw;
            }
        }

        #region Monthly Pay Days


        private DataTable dtEmpWiseLeavePayDaysMonthly(string empSystemId, string month, string year, string plantId)
        {
            try
            {
                string sql = "";
                sql = @"SELECT Count(Apd.DayStatus) TotalPayDays, EmpSystemID FROM EmployeeInformation EEI
				LEFT JOIN HKP.LegalDesignation LD ON LD.Id = EEI.LegalDesignationId
				LEFT JOIN MSt.DesignationMasterLegalDesignation DMLD ON LD.ID = DMLD.LegalDesignationId
				LEFT JOIN MSt.DesignationMaster DM ON DM.Id = DMLD.DesignationMasterId
				LEFT JOIN SCS.DesignationMasterConfiguration C ON C.DesignationMasterId = DM.Id and C.PlantId = '" + plantId + @"'
				LEFT JOIN LeavePolicyMaster LPM ON LPM.SystemID = C.LeavePolicyMasterId 
				LEFT JOIN LeavePolicyDetail LPD ON LPM.SystemID = LPD.LPMSystemID 
				LEFT JOIN (SELECT * FROM LeaveType where LeaveType = 'Earn') LT ON LT.ID = LPD.LTSystemID
				LEFT JOIN  LeavePolicyWorkingDays LPWD ON LPWD.LPDetailID = LPD.SystemID and LPWD.LPMasterID = LPM.SystemID
			    LEFT JOIN AttdnProcessData  APD ON APD.EmpSystemID = EEI.SystemId  and APD.DayStatus = LPWD.DayType
				where Year(Apd.WorkDate) = " + year + @" and Month(Apd.WorkDate) = " + month + @" and EmpSystemID = '" + empSystemId + @"'
				Group By EmpSystemID";

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {

                throw;
            }
        }


        private DataTable dtHalfDayPresentMonthly(string empSystemId , string month, string year, string plantId)
        {
            try
            {
                string sql = "";
                sql = @"SELECT EmpSystemID, COUNT(APD.WorkDate) TotalHalfPresent FROM AttdnProcessData APD
                                    WHERE YEAR(Apd.WorkDate) = " + year + @" and Month(Apd.WorkDate) = " + month + @"
                                    AND PlantID='" + plantId + @"' AND DayStatus='HDP' AND EmpSystemID = '" + empSystemId + @"'
                            GROUP BY EmpSystemID
                                ";

                return _sqlRepository.GetDataTable(sql);

            }
            catch (Exception)
            {

                throw;
            }
        }
        private DataTable dtHalfDayLeaveMonthly(string empSystemId, string month, string year, string plantId)
        {
            try
            {
                string strSQL = "";
                strSQL = @" select EmpSystemID, Count(APD.WorkDate) TotalHalfLeave from AttdnProcessData APD
                                    WHERE Year(Apd.WorkDate) = " + year + @"  and Month(Apd.WorkDate) = " + month + @" and EmpSystemID = '" + empSystemId + @"'
                                    AND PlantID='" + plantId + @"' AND DayStatus<>'HDP' AND IsHalfDayLeave=1 
                                    AND DayStatus in
                                    (select DayType FROM LeavePolicyWorkingDays WHERE LPDetailID in
						             (SELECT SystemID FROM [LeavePolicyDetail] WHERE  LTSystemID=
						             (select id from LeaveType where LeaveType='Earn')))
                                        GROUP BY EmpSystemID ";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        private DataTable GetDtLeavePolicy(string plantId, string empSystemId)
        {
            try
            {
                string strSql = "";
                strSql = @"SELECT LPD.EncashWorkingDaysQty,LPD.LeaveCalculationRoundOption,EEI.SystemId FROM EmployeeInformation EEI
				LEFT JOIN HKP.LegalDesignation LD ON LD.Id = EEI.LegalDesignationId
				LEFT JOIN MSt.DesignationMasterLegalDesignation DMLD ON LD.ID = DMLD.LegalDesignationId
				LEFT JOIN MSt.DesignationMaster DM ON DM.Id = DMLD.DesignationMasterId
				LEFT JOIN SCS.DesignationMasterConfiguration C ON C.DesignationMasterId = DM.Id and C.PlantId = '" + plantId + @"'
				LEFT JOIN LeavePolicyMaster LPM ON LPM.SystemID = C.LeavePolicyMasterId 
				LEFT JOIN LeavePolicyDetail LPD ON LPM.SystemID = LPD.LPMSystemID 
				--LEFT JOIN (SELECT * FROM LeaveType where LeaveType = 'Earn') LT ON LT.ID = LPD.LTSystemID

				where  LPM.PlantId = '" + plantId + @"' AND EEI.SystemId = '" + empSystemId + @"' and LPD.LTSystemID=(SELECT Id FROM LeaveType where LeaveType = 'Earn')";

                return _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception)
            {

                throw;
            }
        }

        void getData(ref IWorksheet sheet1, DataSet ds, DataSet dsFL, ref int xlsRow)
        {
            try
            {
                string _month = string.Empty;
                int _CL = 0;
                int _SL = 0;
                string _FL = "";
                string _Dates = string.Empty;
                string _LeaveTypes = string.Empty;
                string _IsApproved = string.Empty;

                string[] monthsName = { "Jan", "Feb", "Mar", "Apr", "May", "June", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

                int[] months = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
                foreach (var m in months)
                {
                    _month = string.Empty;
                    _CL = 0;
                    _SL = 0;
                    _FL = "";
                    _Dates = string.Empty;
                    _LeaveTypes = string.Empty;
                    _IsApproved = string.Empty;

                    DataView dvFL = new DataView(dsFL.Tables[0]);
                    dvFL.RowFilter = "MonthsN=" + m + "";
                    if (dvFL.Count > 0)
                    {
                        _FL = dvFL[0]["LeaveDays"].ToString();
                    }


                    DataView dv = new DataView(ds.Tables[0]);
                    dv.RowFilter = "monthno=" + m + "";
                    if (dv.Count > 0)
                    {
                        //1
                        _month = dv[0]["monthname"].ToString();
                        for (int i = 0; i < dv.Count; i++)
                        {
                            //get leave count 3,4
                            if (dv[i]["LeaveType"].ToString() == "CL")
                            {
                                _CL++;
                            }
                            if (dv[i]["LeaveType"].ToString() == "SL")
                            {
                                _SL++;
                            }
                            //get leave dates 5
                            if (_Dates.Length == 0)
                            {
                                _Dates = dv[i]["WorkDate"].ToString();
                            }
                            else
                            {
                                _Dates += ", " + dv[i]["WorkDate"].ToString();
                            }
                            //get leave type 6
                            if (_LeaveTypes.Length == 0)
                            {
                                _LeaveTypes = dv[i]["LeaveType"].ToString();
                            }
                            else
                            {
                                _LeaveTypes += ", " + dv[i]["LeaveType"].ToString();
                            }
                            //get leave type 7
                            if (_IsApproved.Length == 0)
                            {
                                _IsApproved = dv[i]["IsApproved"].ToString();
                            }
                            else
                            {
                                _IsApproved += ", " + dv[i]["IsApproved"].ToString();
                            }
                        }
                    }//month ends
                    else
                    {
                        _month = monthsName[m - 1];
                    }

                    #region DATA
                    #region ----------------------Data-----------------------

                    xlsRow += 1;
                    int col = 1;
                    sheet1.Range[xlsRow, col].Text = _month;
                    sheet1.Range[xlsRow, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, col].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    #endregion ----------------------Data-----------------------
                    #endregion

                    //xlsRow++;
                }//month loop
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void SetHeaderValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow, xlsCol].RowHeight = 120;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
            sheet.Range[xlsRow, xlsCol].CellStyle.Rotation = 90;
            ColIndex = xlsCol;
            xlsCol += 1;
        }
        private void SetHeaderValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width, int RowMergeValue, int ColumnMergeValue)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow, xlsCol].RowHeight = 120;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
            sheet.Range[xlsRow, xlsCol].CellStyle.Rotation = 90;
            if (RowMergeValue != 0)
            {
                if (RowMergeValue > 0)
                {
                    sheet.Range[xlsRow, xlsCol, xlsRow + RowMergeValue, xlsCol].Merge();

                }
                else
                {
                    sheet.Range[xlsRow + RowMergeValue, xlsCol, xlsRow, xlsCol].Merge();

                }

            }
            if (ColumnMergeValue != 0)
            {
                if (ColumnMergeValue > 0)
                {
                    sheet.Range[xlsRow, xlsCol, xlsRow, xlsCol + ColumnMergeValue].Merge();

                }
                else
                {
                    sheet.Range[xlsRow, xlsCol + ColumnMergeValue, xlsRow, xlsCol].Merge();

                }

            }
            if (RowMergeValue != 0 && ColumnMergeValue != 0)
            {
                sheet.Range[xlsRow, xlsCol, xlsRow + RowMergeValue, xlsCol + ColumnMergeValue].Merge();

            }

            ColIndex = xlsCol;
            xlsCol += 1;
        }

        private void SetHeaderValue(string text, IWorksheet sheet, int xlsRow, int xlsCol, double width, int RowMergeValue, int ColumnMergeValue)
        {

            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow, xlsCol].RowHeight = 120;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
            sheet.Range[xlsRow, xlsCol].CellStyle.Rotation = 90;
            if (RowMergeValue != 0)
            {
                if (RowMergeValue > 0)
                {
                    sheet.Range[xlsRow, xlsCol, xlsRow + RowMergeValue, xlsCol].Merge();

                }
                else
                {
                    sheet.Range[xlsRow + RowMergeValue, xlsCol, xlsRow, xlsCol].Merge();

                }

            }
            if (ColumnMergeValue != 0)
            {
                if (ColumnMergeValue > 0)
                {
                    sheet.Range[xlsRow, xlsCol, xlsRow, xlsCol + ColumnMergeValue].Merge();

                }
                else
                {
                    sheet.Range[xlsRow, xlsCol + ColumnMergeValue, xlsRow, xlsCol].Merge();

                }

            }
            if (RowMergeValue != 0 && ColumnMergeValue != 0)
            {
                sheet.Range[xlsRow, xlsCol, xlsRow + RowMergeValue, xlsCol + ColumnMergeValue].Merge();

            }
            if (RowMergeValue != 0 && ColumnMergeValue != 0)
            {
                sheet.Range[xlsRow, xlsCol, xlsRow + RowMergeValue, xlsCol + ColumnMergeValue].Merge();

            }
        }

        private void SetHeaderValueRotationLess(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow, xlsCol].RowHeight = 120;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
            sheet.Range[xlsRow, xlsCol].CellStyle.Rotation = 0;
            ColIndex = xlsCol;
            xlsCol += 1;
        }
        private void SetTextValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow, xlsCol].RowHeight = 120;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
            ColIndex = xlsCol;
            xlsCol += 1;
        }
        private void SetTextValueWithoutRefernce(string text, IWorksheet sheet, int xlsRow, int xlsCol)
        {

            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].RowHeight = 10;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = false;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);

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
        string GetDecimalFormat(SalaryHeadSequence shs)
        {
            try
            {
                var ob = new ReportUtility();
                if (shs.IsInt)
                {
                    return ob.NumberFormatInt();
                }
                else
                {
                    return ob.GetDynamicDecimalPlace(shs.DecimalNo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
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
        private DataTable getEmployeeWiseLeaveTransactions(string EmpSystemID, string FromDate, String ToDate)
        {
            try
            {
                var sql = @"Select LT.EmpSystemID,L.Code, format(LT.FromDate,'dd-MMM-yyyy') FromDate,format(LT.ToDate,'dd-MMM-yyyy') ToDate,ISNUll(LT.LeaveDays,0) LeaveDays
             ,FORMAT(LT.AppliedDate,'dd-MMM-yyy')AppliedDate,format(LT.ApprovedDate,'dd-MMM-yyyy')ApprovedDate
                 from  LeaveTransaction  LT
             	INNER JOIN(SELECT * FROM AttdnProcessData WHERE  --DayStatus='LV' and 
                        WorkDate between '" + FromDate + @"' and '" + ToDate + @"') 
				APD ON APD.EmpSystemID=LT.EmpSystemID AND APD.WorkDate=LT.FromDate
                LEFT OUTER JOIN LeaveType AS L ON L.Id=LT.LTSystemID				
                where LT.EmpSystemID='" + EmpSystemID + @"'  and LT.IsApproved=1 and L.LeaveType = 'Earn'
                order by LT.FromDate ,LT.ToDate ";
                var list = _sqlRepository.GetDataTable(sql);

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetEmployeeInformation(string fromDate, string toDate, string criteria)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(_AttendanceManagementService.GetEmpInfo(identity.CompanyGroupId, identity.PlantId, fromDate, toDate, criteria), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

        }
        #endregion
    }
}
