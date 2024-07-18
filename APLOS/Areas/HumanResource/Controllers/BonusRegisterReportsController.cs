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
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.Helpers.ReportUtility;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class BonusRegisterReportsController : BaseController
    {
        #region Constructor

        private readonly IAttendanceManagementService _AttendanceManagementService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;
        public BonusRegisterReportsController(
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
        public ActionResult BonusC()
        {
            return View();
        }
        public ActionResult BonusProvison()
        {
            return View();
        }
        #endregion -- Pages

        #region Get Bonus Register
        [HttpPost, Authorize]
        public ActionResult GetBonusRegister(string yearId, bool withBonusValue, string FromDate, string ToDate)
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

            //DataSet dsEmpBonus = null;
            //DataTable dtEmpBonus = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            #endregion Variable

            try
            {
                Library.HumanResource.Report.Payroll.PayrollReports prr = new Library.HumanResource.Report.Payroll.PayrollReports();
                int rowTobeAdded = 1;

                if (withBonusValue == true)
                {
                    rowTobeAdded = 2;
                }

                ru = new ReportUtility();

                objRpt = new clsReport(_sqlRepository);

                #region Variable
                ParamList para = new ParamList();
                ParamList leavePara = new ParamList();
                ParamList attdnProcessParam = new ParamList();

                var FactoryName = "";
                var CmpName = "";

                para.PlantId = identity.PlantId;
                //string FromDate = "";
                //string ToDate = "";

                #endregion Variable

                #region DataSet
                DataTable dtTaxYear = null;
                dtTaxYear = _sqlRepository.GetDataTable("SELECT * FROM SCS.TaxYear WHERE TaxYearName = '" + yearId + @"'");

                int fromYear = Convert.ToDateTime(dtTaxYear.Rows[0]["StartDate"]).Year;//EndDate
                int toYear = Convert.ToDateTime(dtTaxYear.Rows[0]["EndDate"]).Year;

              
                    int DaysInMonth = DateTime.DaysInMonth(Convert.ToInt16(Convert.ToDateTime(ToDate).ToString("yyyy")), Convert.ToInt16(Convert.ToDateTime(ToDate).ToString("MM")));

                    ToDate = DaysInMonth + "-" + Convert.ToDateTime(ToDate).ToString("MMM") + "-" + Convert.ToDateTime(ToDate).ToString("yyyy");
               
                objRpt.GetFiscalMonthListSql(FromDate, ToDate, out dsMonth);
                objRpt.GetMonthWiseEmpMonthlyAttdnInfo(FromDate, ToDate, dsMonth.Tables[0], out dsEmpAttdn);
                dtEmpAttdn = dsEmpAttdn.Tables[0];
                Dictionary<string, List<DataRow>> dicBonus = prr.GetMonthWiseEmpBonusInfo(FromDate, ToDate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, dsMonth.Tables[0]);

                DataTable dtMonthInfo = dsMonth.Tables[0];

                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

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
                var colEmpCode = 0;
                var colEmpName = 0;
                var colTotalAmount = 0;
                var colBonusPercentage = 0;
                var colBonusAmount = 0;
                var colDOS = 0;
                var colWageLabel = 0;

                #endregion------------------Column Header------------------


                var oRU = new ReportUtility();


                var _total_head_count = 0;
                List<FiscalYearMonthSequence> list = null;

                SetHeaderValue("S.No.", sheet1, xlsRow, ref xlsCol, out colSr, 6);
                SetHeaderValue("EmpCode", sheet1, xlsRow, ref xlsCol, out colEmpCode, 9);
                SetHeaderValue("Name", sheet1, xlsRow, ref xlsCol, out colEmpName, 25);

                SetHeaderValue("", sheet1, xlsRow, ref xlsCol, out colWageLabel, 9.86);
                sheet1.Range[xlsRow, colEmpName, xlsRow, colWageLabel].Merge();
                var colStart = colWageLabel;
                CreateDynamicMonthHead(dtMonthInfo, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref colStart, out list);
                //xlsCol--;
                SetHeaderValue("Total Amount", sheet1, xlsRow, ref xlsCol, out colTotalAmount, 12);
                SetHeaderValue("%", sheet1, xlsRow, ref xlsCol, out colBonusPercentage, 12);
                SetHeaderValue("Bonus Amt", sheet1, xlsRow, ref xlsCol, out colBonusAmount, 12);
                SetHeaderValue("Date of Leaving", sheet1, xlsRow, ref xlsCol, out colDOS, 12);
                endXlsCol = colDOS;
                var fPanRow = xlsRow + 1;

                #region ******************Report Header******************
                DataView view = new DataView(dicBonus.Values.ElementAt(0)[0].Table);
                DataTable dtEmpInfo = view.ToTable(true, "EmpSystemId", "EmployeeCode", "EmployeeName", "BankName", "BankShortName", "BankAccNo", "DOS", "PaymentMode", "EmployeeCategory", "WorkingDaysInAMonth");

                var totalEarningAmountYearly = 0.00;
                var totalEarningBonusAmountYearly = 0.00;
                double totalBonusAmount = 0.00;
                var totalPayDayYearly = 0.00;

                xlsRow++;
                for (int dti = 0; dti < dtEmpInfo.Rows.Count; dti++)
                {
                    totalEarningAmountYearly = 0.00;
                    totalEarningBonusAmountYearly = 0.00;

                    totalPayDayYearly = 0.00;
                    totalBonusAmount = 0.00;
                    if(dtEmpInfo.Rows[dti]["EmployeeCode"].ToString() == "162")
                    {

                    }
                    
                    string empSystemId = dtEmpInfo.Rows[dti]["EmpSystemId"].ToString();
                    slCount++;
                    sheet1.Range[xlsRow, colSr].Text = slCount.ToString();
                    sheet1.Range[xlsRow, colSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, colSr].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    sheet1.Range[xlsRow, colSr, xlsRow + rowTobeAdded, colSr].Merge();
                    sheet1.Range[xlsRow, colSr, xlsRow + rowTobeAdded, colSr].BorderAround(ExcelLineStyle.Hair);


                    sheet1.Range[xlsRow, colEmpCode].Text = dtEmpInfo.Rows[dti]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, colEmpCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, colEmpCode].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                    sheet1.Range[xlsRow, colEmpCode, xlsRow + rowTobeAdded, colEmpCode].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, colEmpCode, xlsRow + rowTobeAdded, colEmpCode].Merge();


                    sheet1.Range[xlsRow, colEmpName].Text = dtEmpInfo.Rows[dti]["EmployeeName"].ToString() + Environment.NewLine + dtEmpInfo.Rows[dti]["BankShortName"].ToString() + Environment.NewLine + dtEmpInfo.Rows[dti]["BankAccNo"].ToString();
                    sheet1.Range[xlsRow, colEmpName].RowHeight = 19;
                    sheet1.Range[xlsRow + 1, colEmpName].RowHeight = 19;

                    sheet1.Range[xlsRow, colEmpName, xlsRow + rowTobeAdded, colWageLabel].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, colEmpName, xlsRow + rowTobeAdded, colEmpName].Merge();

                    sheet1.Range[xlsRow, colWageLabel].Text = "Wages->";
                    sheet1.Range[xlsRow, colWageLabel].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    if (withBonusValue)
                    {
                        sheet1.Range[xlsRow + 1, colWageLabel].Text = "Bonus->";
                        sheet1.Range[xlsRow + 1, colWageLabel].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow + 2, colWageLabel].Text = "Days->";
                        sheet1.Range[xlsRow + 2, colWageLabel].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colWageLabel, xlsRow + rowTobeAdded, colWageLabel].BorderAround(ExcelLineStyle.Hair);

                    }
                    else
                    {
                        sheet1.Range[xlsRow + 1, colWageLabel].Text = "Days->";
                        sheet1.Range[xlsRow + 1, colWageLabel].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colWageLabel, xlsRow + rowTobeAdded, colWageLabel].BorderAround(ExcelLineStyle.Hair);


                    }



                    try
                    {
                        if (empSystemId == "2010030")
                        {

                        }

                        if (dicBonus.ContainsKey(empSystemId))
                        {
                            double totalPayDay = 0.00;
                            double earningAmount = 0.00;
                            double earningBonusAmount = 0.00;
                            bool isDecimal = false;
                            double decimalNo = 0;

                            string Month = "";

                            List<DataRow> BonusList = dicBonus[empSystemId];
                            try
                            {
                               
                                for (int BNS = 0; BNS < BonusList.Count; BNS++)
                                {
                                    //earningAmount = 0.00;
                                    //earningBonusAmount = 0.00;
                                    totalPayDay = 0.00;
                                    if(Month!=BonusList[BNS]["MonthNo"].ToString()+ BonusList[BNS]["YearNo"].ToString())
                                    {
                                        totalPayDay = 0.00;
                                        earningAmount = 0.00;
                                        earningBonusAmount = 0.00;
                                        isDecimal = false;
                                        decimalNo = 0;

                                    }
                                    Month = BonusList[BNS]["MonthNo"].ToString() + BonusList[BNS]["YearNo"].ToString();
                                    try
                                    {
                                        List<FiscalYearMonthSequence> _seq = list.Where(ee => ee.MonthNo == BonusList[BNS]["MonthNo"].ToString() && ee.MonthYear == BonusList[BNS]["YearNo"].ToString()).ToList();
                                        if (_seq.Count > 0)
                                        {
                                            //totalPayDay = clsStaticInfo.dbl(BonusList[BNS]["PayDays"].ToString());


                                            if (!String.IsNullOrEmpty(dtEmpInfo.Rows[dti]["WorkingDaysInAMonth"].ToString().ToUpper()))
                                            {
                                                if (dtEmpInfo.Rows[dti]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOffAndHoliday.ToString().ToUpper())
                                                {
                                                    totalPayDay = (Convert.ToDouble(BonusList[BNS]["TotalProcDate"]) - clsStaticInfo.dbl(BonusList[BNS]["TotalWeekOff"].ToString()) - clsStaticInfo.dbl(BonusList[BNS]["TotalHoliDay"].ToString()) - clsStaticInfo.dbl(BonusList[BNS]["TotalAbsent"].ToString()));
                                                }
                                                if (dtEmpInfo.Rows[dti]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOff.ToString().ToUpper())
                                                {
                                                    totalPayDay = (clsStaticInfo.dbl(BonusList[BNS]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(BonusList[BNS]["TotalWeekOff"].ToString()) - clsStaticInfo.dbl(BonusList[BNS]["TotalAbsent"].ToString()));

                                                }
                                            }
                                            else
                                            {
                                                totalPayDay = (clsStaticInfo.dbl(BonusList[BNS]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(BonusList[BNS]["TotalAbsent"].ToString()));
                                            }




                                            if (BonusList[BNS]["HeadCategory"].ToString().ToUpper() == "BASIC")
                                            {
                                                earningAmount = clsStaticInfo.dbl(BonusList[BNS]["DisbusmentAmount"].ToString());
                                                if (earningAmount <= 0)
                                                {
                                                    sheet1.Range[xlsRow, _seq[0].XLColIndex].Text = "-";// + Environment.NewLine + totalPayDay;                              
                                                    sheet1.Range[xlsRow, _seq[0].XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                                    sheet1.Range[xlsRow, _seq[0].XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                                }
                                                else
                                                {
                                                    sheet1.Range[xlsRow, _seq[0].XLColIndex].Number = Convert.ToDouble(earningAmount);// + Environment.NewLine + totalPayDay;
                                                    sheet1.Range[xlsRow, _seq[0].XLColIndex].NumberFormat = GetDecimalFormat(isDecimal, Convert.ToInt32(decimalNo));
                                                    sheet1.Range[xlsRow, _seq[0].XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                                    sheet1.Range[xlsRow, _seq[0].XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                                }
                                                totalPayDayYearly += clsStaticInfo.dbl(totalPayDay.ToString());
                                                totalEarningAmountYearly += Convert.ToDouble(earningAmount);
                                                earningAmount = 0;

                                            }
                                            if (BonusList[BNS]["HeadCategory"].ToString().ToUpper() == "OTHER BONUS")
                                            {
                                                earningBonusAmount += clsStaticInfo.dbl(BonusList[BNS]["DisbusmentAmount"].ToString());
                                                totalEarningBonusAmountYearly += Convert.ToDouble(earningBonusAmount);
                                            }

                                            if (BonusList[BNS]["HeadCategory"].ToString().ToUpper() == "RetainedBonus".ToUpper())
                                            {
                                                earningBonusAmount += clsStaticInfo.dbl(BonusList[BNS]["DisbusmentAmount"].ToString());
                                                totalEarningBonusAmountYearly += Convert.ToDouble(earningBonusAmount);
                                            }

                                            if (BonusList[BNS]["HeadCategory"].ToString().ToUpper() == "Monthly Bonus Retain".ToUpper())
                                            {
                                                earningBonusAmount += clsStaticInfo.dbl(BonusList[BNS]["DisbusmentAmount"].ToString());
                                                totalEarningBonusAmountYearly += Convert.ToDouble(earningBonusAmount);
                                            }
                                            if (BonusList[BNS]["HeadCategory"].ToString().ToUpper() == "Annual Bonus Retain".ToUpper())
                                            {
                                                earningBonusAmount += clsStaticInfo.dbl(BonusList[BNS]["DisbusmentAmount"].ToString());
                                                totalEarningBonusAmountYearly += Convert.ToDouble(earningBonusAmount);
                                            }
                                            isDecimal = bplib.clsWebLib.GetBoolData(BonusList[BNS]["IntegerInDisb"].ToString());
                                            decimalNo = clsStaticInfo.dbl(BonusList[BNS]["DecimalNo"].ToString());
                                            

                                            if (withBonusValue)
                                            {
                                                if (earningBonusAmount == 0)
                                                {
                                                    sheet1.Range[xlsRow + rowTobeAdded - 1, _seq[0].XLColIndex].Text = "-";// + Environment.NewLine + totalPayDay;                              
                                                    sheet1.Range[xlsRow + rowTobeAdded - 1, _seq[0].XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                                    sheet1.Range[xlsRow + rowTobeAdded - 1, _seq[0].XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                                                }
                                                else
                                                {
                                                    sheet1.Range[xlsRow + rowTobeAdded - 1, _seq[0].XLColIndex].Number = Convert.ToDouble(earningBonusAmount);// + Environment.NewLine + totalPayDay;
                                                    sheet1.Range[xlsRow + rowTobeAdded - 1, _seq[0].XLColIndex].NumberFormat = GetDecimalFormat(isDecimal, Convert.ToInt32(decimalNo));
                                                    sheet1.Range[xlsRow + rowTobeAdded - 1, _seq[0].XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                                    sheet1.Range[xlsRow + rowTobeAdded - 1, _seq[0].XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                                    
                                                }



                                                sheet1.Range[xlsRow + rowTobeAdded, _seq[0].XLColIndex].Number = totalPayDay;// + Environment.NewLine + totalPayDay;
                                                sheet1.Range[xlsRow + rowTobeAdded, _seq[0].XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                                sheet1.Range[xlsRow + rowTobeAdded, _seq[0].XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                                sheet1.Range[xlsRow, _seq[0].XLColIndex, xlsRow + rowTobeAdded, _seq[0].XLColIndex].BorderAround(ExcelLineStyle.Hair);

                                            }
                                            else
                                            {
                                                sheet1.Range[xlsRow + rowTobeAdded, _seq[0].XLColIndex].Number = totalPayDay;// + Environment.NewLine + totalPayDay;
                                                sheet1.Range[xlsRow + rowTobeAdded, _seq[0].XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                                sheet1.Range[xlsRow + rowTobeAdded, _seq[0].XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                                            }

                                            sheet1.Range[xlsRow, _seq[0].XLColIndex, xlsRow + rowTobeAdded, _seq[0].XLColIndex].BorderAround(ExcelLineStyle.Hair);


                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        throw ex;
                                    }
                                }

                            }
                            catch (Exception ex)
                            {

                                throw ex;
                            }
                            var totalEarningSalaryFormula = "";// "=SUM(" + ru.GetColumnNameForXls(colWageLabel + 1) + xlsRow + ":" + ru.GetColumnNameForXls(colWageLabel + 12) + xlsRow + ")";
                            var totalPayDaysFormula = ""; //"=SUM(" + ru.GetColumnNameForXls(colWageLabel + 1) + (xlsRow + 1) + ":" + ru.GetColumnNameForXls(colWageLabel + 12) + (xlsRow + 1) + ")";
                            var totalBonusFormula = "";
                            if (withBonusValue)
                            {
                                totalEarningSalaryFormula = "=SUM(" + ru.GetColumnNameForXls(colWageLabel + 1) + xlsRow + ":" + ru.GetColumnNameForXls(colWageLabel + dsMonth.Tables[0].Rows.Count) + xlsRow + ")";
                                totalBonusFormula = "=SUM(" + ru.GetColumnNameForXls(colWageLabel + 1) + (xlsRow + 1) + ":" + ru.GetColumnNameForXls(colWageLabel + dsMonth.Tables[0].Rows.Count) + (xlsRow + 1) + ")";
                                totalPayDaysFormula = "=SUM(" + ru.GetColumnNameForXls(colWageLabel + 1) + (xlsRow + 2) + ":" + ru.GetColumnNameForXls(colWageLabel + dsMonth.Tables[0].Rows.Count) + (xlsRow + 2) + ")";


                                sheet1.Range[xlsRow, colTotalAmount].Formula = totalEarningSalaryFormula;// Earning Total
                                sheet1.Range[xlsRow, colTotalAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, colTotalAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow + rowTobeAdded - 1, colTotalAmount].Formula = totalBonusFormula;// Bonus Total
                                sheet1.Range[xlsRow + rowTobeAdded - 1, colTotalAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow + rowTobeAdded - 1, colTotalAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                                sheet1.Range[xlsRow + rowTobeAdded, colTotalAmount].Formula = totalPayDaysFormula;// PayDays Total
                                sheet1.Range[xlsRow + rowTobeAdded, colTotalAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow + rowTobeAdded, colTotalAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                                sheet1.Range[xlsRow, colTotalAmount, xlsRow + rowTobeAdded, colTotalAmount].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, colBonusPercentage, xlsRow + rowTobeAdded, colBonusPercentage].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, colBonusPercentage, xlsRow + rowTobeAdded, colBonusPercentage].Merge();


                                sheet1.Range[xlsRow, colBonusAmount].Formula = totalBonusFormula;
                                sheet1.Range[xlsRow, colBonusAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, colBonusAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                                sheet1.Range[xlsRow + rowTobeAdded, colBonusAmount].Text = dtEmpInfo.Rows[dti]["PaymentMode"].ToString();
                                sheet1.Range[xlsRow + rowTobeAdded, colBonusAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow + rowTobeAdded, colBonusAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                                sheet1.Range[xlsRow, colBonusAmount, xlsRow + rowTobeAdded, colBonusAmount].BorderAround(ExcelLineStyle.Hair);

                            }
                            else
                            {
                                totalEarningSalaryFormula = "=SUM(" + ru.GetColumnNameForXls(colWageLabel + 1) + xlsRow + ":" + ru.GetColumnNameForXls(colWageLabel + 12) + xlsRow + ")";
                                totalPayDaysFormula = "=SUM(" + ru.GetColumnNameForXls(colWageLabel + 1) + (xlsRow + 1) + ":" + ru.GetColumnNameForXls(colWageLabel + 12) + (xlsRow + 1) + ")";
                                //totalPayDaysFormula = "=SUM(" + ru.GetColumnNameForXls(colWageLabel + 1) + (xlsRow + 2) + ":" + ru.GetColumnNameForXls(colWageLabel + 12) + (xlsRow + 2) + ")";

                                sheet1.Range[xlsRow, colTotalAmount].Formula = totalEarningSalaryFormula;//totalEarningAmountYearly.ToString();
                                sheet1.Range[xlsRow, colTotalAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, colTotalAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow + rowTobeAdded, colTotalAmount].Formula = totalPayDaysFormula;// totalPayDayYearly.ToString();
                                sheet1.Range[xlsRow + rowTobeAdded, colTotalAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow + rowTobeAdded, colTotalAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, colTotalAmount, xlsRow + rowTobeAdded, colTotalAmount].BorderAround(ExcelLineStyle.Hair);



                                sheet1.Range[xlsRow, colBonusAmount].Text = totalEarningBonusAmountYearly > 0.00 ? totalEarningBonusAmountYearly.ToString() : "-";
                                sheet1.Range[xlsRow, colBonusAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, colBonusAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                                sheet1.Range[xlsRow + rowTobeAdded, colBonusAmount].Text = dtEmpInfo.Rows[dti]["PaymentMode"].ToString();
                                sheet1.Range[xlsRow + rowTobeAdded, colBonusAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow + rowTobeAdded, colBonusAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                                sheet1.Range[xlsRow, colBonusAmount, xlsRow + 1, colBonusAmount].BorderAround(ExcelLineStyle.Hair);

                            }




                            //Bonus Percentage
                            var bonusPercentage = "";
                            bonusPercentage = "8.33";

                            sheet1.Range[xlsRow, colBonusPercentage].Text = bonusPercentage;
                            sheet1.Range[xlsRow, colBonusPercentage].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, colBonusPercentage].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                            sheet1.Range[xlsRow, colBonusPercentage, xlsRow + rowTobeAdded, colBonusPercentage].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, colBonusPercentage, xlsRow + rowTobeAdded, colBonusPercentage].Merge();






                            //Date of Separation
                            sheet1.Range[xlsRow, colDOS].Text = dtEmpInfo.Rows[dti]["DOS"].ToString();
                            sheet1.Range[xlsRow, colDOS].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, colDOS, xlsRow + rowTobeAdded, colDOS].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, colDOS, xlsRow + rowTobeAdded, colDOS].Merge();




                            xlsRow += 1 + rowTobeAdded;


                            if(withBonusValue)
                            {
                                if (slCount % 12 == 0)
                                {
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Hair;
                                    //xlsRow++;
                                    sheet1[xlsRow, 1].RowHeight = 2;

                                    sheet1.HPageBreaks.Add(sheet1[xlsRow, 1]);
                                }
                            }
                            else
                            {
                                if (slCount % 16 == 0)
                                {
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Hair;
                                    //xlsRow++;
                                    sheet1[xlsRow, 1].RowHeight = 2;

                                    sheet1.HPageBreaks.Add(sheet1[xlsRow, 1]);
                                }
                            }

                           



                        }
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }

                }
                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                xlsRow = 1;
                xlsCol = 1;
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
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 13;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryAddress;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 13;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Bonus Register Report As of " + Convert.ToDateTime(FromDate).ToString("MMMM") + " : " + Convert.ToDateTime(FromDate).Year.ToString() + " TO " + Convert.ToDateTime(ToDate).ToString("MMMM") + "," + Convert.ToDateTime(ToDate).Year.ToString();
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

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

                sheet1.Name = "BonusStatement" + para.SalaryProcessId;
                #endregion

                workbook.Version = ExcelVersion.Excel97to2003;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "BonusRegister.xls";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);

                //}
            }

            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Indian govt Bonus Sheet
        /// </summary>
        /// <param name="yearId"></param>
        /// <param name="withBonusValue"></param>
        /// <param name="FromDate"></param>
        /// <param name="ToDate"></param>
        /// <returns></returns>
        [HttpPost, Authorize]
        public ActionResult GetBonusFormC(string yearId, bool withBonusValue, string FromDate, string ToDate)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = yearId + "- BonusFormC" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;
                Library.HumanResource.Report.Payroll.PayrollReports prr = new Library.HumanResource.Report.Payroll.PayrollReports();

                var workbook = prr.GetBonusReportC(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.Name, yearId,withBonusValue,FromDate,ToDate);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }
        /// <summary>
        /// Indian govt Bonus Sheet
        /// </summary>
        /// <param name="yearId"></param>
        /// <param name="withBonusValue"></param>
        /// <param name="FromDate"></param>
        /// <param name="ToDate"></param>
        /// <returns></returns>
        [HttpPost, Authorize]
        public ActionResult GetBonusReportProvisional(string yearId, bool withBonusValue, string FromDate, string ToDate)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = yearId + "- BonusProvision"+ DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;
                Library.HumanResource.Report.Payroll.PayrollReports prr = new Library.HumanResource.Report.Payroll.PayrollReports();

                var workbook = prr.GetBonusReportProvisional(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.Name, yearId, withBonusValue, FromDate, ToDate);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
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
    }
}
#endregion
