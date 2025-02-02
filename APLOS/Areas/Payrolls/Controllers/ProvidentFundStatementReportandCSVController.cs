using Aplos.Controllers;
using ConnectionManager;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Mvc;



using static Library.Service.Helpers.ReportUtility;
using static Library.Service.HumanResources.PayRegisterBDReportService;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class ProvidentFundStatementReportandCSVController : BaseController
    {
        #region Constructor

        private readonly IPayRegisterBDReportService _payRegisterBDReportService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;



        public ProvidentFundStatementReportandCSVController(
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
        public ActionResult GetProvidentFundStatement(string month, string year, bool isPFEligible, bool isActive = true, bool isSeperated = true)
        {
            try
            {
                CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsReport objRpt = null;
                DataView dvEmpInfo = null;
                DataSet dsCmp = null;
                DataSet dsFactory = null;
                DataSet dsSalaryProcessId = null;
                Dictionary<string, int> SalaryHeadIndex = new Dictionary<string, int>();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                var formulaStartRow = 0;
                var endXlsRow = 0;
                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                DataTable dt = null;
                DataSet dsEmpInfo = null;
                DataTable dtEmpInfo = new DataTable();
                dtEmpInfo = null;
                int iCount = 0;
                var fileHeader = "";
                double Total = 0;
                double GrandTotal = 0;
                //get ds

                objRpt = new clsReport();

                //get ds

                var today = DateTime.Now.Date;
                var SalaryProcessId = string.Empty;
                #region DataSet

                var date = DateTime.DaysInMonth(Convert.ToInt16(year), Convert.ToInt32(month));
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
                var lastDateMonth = date + "-" + monthName + "-" + year;
                Dictionary<string, DataRow> dicAttdnSummary = new Dictionary<string, DataRow>();
                Dictionary<string, List<DataRow>> dicSalary = new Dictionary<string, List<DataRow>>();
                List<Dictionary<string, object>> dicPFSalaryHead = new List<Dictionary<string, object>>();
                Dictionary<string, List<DataRow>> dicPFSalaryHeadWiseData = new Dictionary<string, List<DataRow>>();
                if (isPFEligible == true)
                {
                    dtEmpInfo = GetPFEmployeeInfo(identity.PlantId, Convert.ToInt32(month), year, isActive, isSeperated, isPFEligible);

                    dicSalary = GetEmpPFSalaryInfo(identity.PlantId, Convert.ToInt32(month), year, isActive, isSeperated, isPFEligible);
                    GetEmpPFSalaryHead(identity.PlantId, Convert.ToInt32(month), year, isActive, isSeperated, isPFEligible, out dt);
                    dicPFSalaryHeadWiseData = GetEmpPFSalaryHeadInfo(identity.PlantId, Convert.ToInt32(month), year, isActive, isSeperated, isPFEligible);
                    dicAttdnSummary = GetEmployeeAttdnSummary(month, year, identity.PlantId);
                }
                else
                {
                    dtEmpInfo = GetPFEmpInfo(identity.PlantId, Convert.ToInt32(month), year, isActive, isSeperated, isPFEligible);
                }


                //dtEmpInfo = dsEmpInfo.Tables[0];



                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet

                var colSrNo = 0;
                var colPaycode = 0;
                var colPFUANNo = 0;
                var colEmployeeName = 0;
                var colDays = 0;
                var colWagesAmount = 0;
                var colEmployeeShare12parcent = 0;
                var colVPF = 0;
                var col3point67parcent = 0;
                var colFPFEmployersShare8point33percent = 0;
                var colTotal = 0;
                var colWAGES8point33percent = 0;
                var colWagesAbove15000 = 0;
                var colRemarksDOL = 0;
                var colAge = 0;
                var colWagesTotal = 0;
                var slCount = 0;


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);

                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;
                DataRow drAttdnSummary = null;
                xlsRow = 6;

                //	#region ------------------Column Header------------------

                var ru = new ReportUtility();
                if (isPFEligible == true)
                {

                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "S.No", 4, 25, ExcelHAlign.HAlignCenter); colSrNo = xlsCol; xlsCol++;
                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "EmpCode", 8, 25, ExcelHAlign.HAlignCenter); colPaycode = xlsCol; xlsCol++;
                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "PF UAN No.", 12, 25, ExcelHAlign.HAlignCenter); colPFUANNo = xlsCol; xlsCol++;
                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Name of the Employee", 21, 25, ExcelHAlign.HAlignCenter); colEmployeeName = xlsCol; xlsCol++;
                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Age", 5, 25, ExcelHAlign.HAlignCenter); colAge = xlsCol; xlsCol++;
                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Days", 5, 25, ExcelHAlign.HAlignCenter); colDays = xlsCol; xlsCol++;
                    //SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Wages Amount", 8, 25, ExcelHAlign.HAlignCenter); colWagesAmount = xlsCol; xlsCol++;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        iCount = xlsCol;
                        SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, dt.Rows[i]["SalaryHead"].ToString(), 8, 25, ExcelHAlign.HAlignCenter);
                        SalaryHeadIndex.Add(dt.Rows[i]["SalaryHeadId"].ToString(), iCount);
                        xlsCol++;
                    }

                    if (dt.Rows.Count > 1)
                    {
                        SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Wages Total", 8, 25, ExcelHAlign.HAlignCenter); colWagesTotal = xlsCol; xlsCol++;
                    }

                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Employee's Share", 10, 25, ExcelHAlign.HAlignCenter); colEmployeeShare12parcent = xlsCol; xlsCol++;
                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "VPF", 5, 25, ExcelHAlign.HAlignRight); colVPF = xlsCol; xlsCol++;
                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Employers Share", 8, 25, ExcelHAlign.HAlignRight); col3point67parcent = xlsCol; xlsCol++;
                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Pension", 9, 25, ExcelHAlign.HAlignRight); colFPFEmployersShare8point33percent = xlsCol; xlsCol++;
                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Total", 8, 25, ExcelHAlign.HAlignRight); colTotal = xlsCol; xlsCol++;
                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Pensionable wage.", 15, 25, ExcelHAlign.HAlignRight); colWAGES8point33percent = xlsCol; xlsCol++;
                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "WAGES ABOVE 15000", 9, 25, ExcelHAlign.HAlignRight); colWagesAbove15000 = xlsCol; xlsCol++;
                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Remarks DOL", 9, 25, ExcelHAlign.HAlignRight); colRemarksDOL = xlsCol;
                    endXlsCol = xlsCol;
                    xlsRow++;

                    formulaStartRow = xlsRow;
                    for (int i = 0; i < dtEmpInfo.Rows.Count; i++)
                    {
                        double basic = 0.00;
                        double pension = 0.00;
                        double pfER = 0.00;
                        double pfEE = 0.00;
                        double VPF = 0.00;
                        bool basicIntegerInDisb = false;
                        int basicDecimalPoint = 0;
                        bool pfERIntegerInDisb = false;
                        int pfERDecimalPoint = 0;
                        bool pfEEIntegerInDisb = false;
                        int pfEEDecimalPoint = 0;
                        bool pensionIntegerInDisb = false;
                        int pensionDecimalPoint = 0;
                        bool vpfIntegerInDisb = false;
                        int vpfDecimalPoint = 0;
                        var wages8point33percent = 0;
                        var wagesAbove15000 = 0;


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
                                if (item["HeadCategory"].ToString() == "Pension")
                                {
                                    pension = clsStaticInfo.dbl(item["DisbusmentAmount"].ToString());
                                    pensionIntegerInDisb = bplib.clsWebLib.GetBoolData(item["IntegerInDisb"]);
                                    pensionDecimalPoint = (int)clsStaticInfo.dbl(item["DecimalNo"].ToString());
                                }
                                if (item["HeadCategory"].ToString() == "PF Employer Contribution")
                                {
                                    pfER = clsStaticInfo.dbl(item["DisbusmentAmount"].ToString());
                                    pfERIntegerInDisb = bplib.clsWebLib.GetBoolData(item["IntegerInDisb"]);
                                    pfERDecimalPoint = (int)clsStaticInfo.dbl(item["DecimalNo"].ToString());
                                }
                                if (item["HeadCategory"].ToString() == "PF Employee Contribution")
                                {
                                    pfEE = clsStaticInfo.dbl(item["DisbusmentAmount"].ToString());
                                    pfEEIntegerInDisb = bplib.clsWebLib.GetBoolData(item["IntegerInDisb"]);
                                    pfEEDecimalPoint = (int)clsStaticInfo.dbl(item["DecimalNo"].ToString());
                                }
                                if (item["HeadCategory"].ToString() == "PF Voluntary")
                                {
                                    VPF = clsStaticInfo.dbl(item["DisbusmentAmount"].ToString());
                                    vpfIntegerInDisb = bplib.clsWebLib.GetBoolData(item["IntegerInDisb"]);
                                    vpfDecimalPoint = (int)clsStaticInfo.dbl(item["DecimalNo"].ToString());
                                }
                            }
                        }




                        //var EmployeeName = GetEmployeeName(dvEmpInfo, Convert.ToString(x[i]));
                        drAttdnSummary = null;
                        var Workingdays = 0.00;// bplib.clsWebLib.GetNumData(GetWorkingDate(dvEmpInfo, Convert.ToString(x[i])));
                        if (dicAttdnSummary.ContainsKey(dtEmpInfo.Rows[i]["EmpSystemId"].ToString()))
                        {

                            drAttdnSummary = dicAttdnSummary[dtEmpInfo.Rows[i]["EmpSystemId"].ToString()];
                            if (!String.IsNullOrEmpty(dtEmpInfo.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper()))
                            {
                                if (dtEmpInfo.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOffAndHoliday.ToString().ToUpper())
                                {
                                    Workingdays = clsStaticInfo.dbl(drAttdnSummary["TotalProcDate"].ToString()) - clsStaticInfo.dbl(drAttdnSummary["TotalAbsent"].ToString()) - clsStaticInfo.dbl(drAttdnSummary["TotalHoliDay"].ToString()) - clsStaticInfo.dbl(drAttdnSummary["TotalWeekOff"].ToString());
                                }
                                if (dtEmpInfo.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOff.ToString().ToUpper())
                                {
                                    Workingdays = clsStaticInfo.dbl(drAttdnSummary["TotalProcDate"].ToString()) - clsStaticInfo.dbl(drAttdnSummary["TotalAbsent"].ToString()) - clsStaticInfo.dbl(drAttdnSummary["TotalWeekOff"].ToString());
                                }
                            }
                            else
                            {
                                Workingdays = clsStaticInfo.dbl(drAttdnSummary["TotalProcDate"].ToString()) - clsStaticInfo.dbl(drAttdnSummary["TotalAbsent"].ToString());
                            }
                        }

                        //if (dvEmpInfo.Count > 0)
                        //{

                        //}
                        if (dicPFSalaryHeadWiseData.ContainsKey(dtEmpInfo.Rows[i]["EmpSystemId"].ToString()))
                        {
                            List<DataRow> dlrPF = dicPFSalaryHeadWiseData[dtEmpInfo.Rows[i]["EmpSystemId"].ToString()];
                            Total = 0;
                            foreach (var item in dlrPF)
                            {
                                sheet1.Range[xlsRow, SalaryHeadIndex[item["SalaryHeadID"].ToString()]].Number = clsStaticInfo.dbl(item["DisbusmentAmount"].ToString());
                                Total = Total + clsStaticInfo.dbl(item["DisbusmentAmount"].ToString());
                            }
                            GrandTotal = Total + GrandTotal;
                        }
                        if (dt.Rows.Count > 1)
                        {
                            ru.SetTextBorder(ref sheet1, xlsRow, colWagesTotal, Total);
                        }

                        double EarningValueRangeTo = clsStaticInfo.dbl(dtEmpInfo.Rows[i]["EarningValueRangeTo"].ToString());
                        double age = clsStaticInfo.dbl(dtEmpInfo.Rows[i]["Age"].ToString());


                        if (Convert.ToInt16(age) > 58 && Convert.ToDouble(basic) > EarningValueRangeTo)
                        {
                            wages8point33percent =(int)clsStaticInfo.dbl(EarningValueRangeTo);
                            wagesAbove15000 = Convert.ToInt32(Total) - Convert.ToInt32(EarningValueRangeTo);
                        }
                        else if (Convert.ToInt16(age) > 58 && Convert.ToDouble(basic) < EarningValueRangeTo)
                        {
                            wages8point33percent = Convert.ToInt32(basic);
                            wagesAbove15000 = 0;
                        }
                        else if (Convert.ToInt16(age) <= 58 && Convert.ToDouble(basic) > EarningValueRangeTo)
                        {
                            wages8point33percent = Convert.ToInt32(EarningValueRangeTo);
                            wagesAbove15000 = Convert.ToInt32(Total) - Convert.ToInt32(EarningValueRangeTo);
                        }

                        else if (Convert.ToInt16(age) <= 58 && Convert.ToDouble(basic) <= EarningValueRangeTo)
                        {
                            wages8point33percent = Convert.ToInt32(basic);
                            wagesAbove15000 = 0;
                        }
                        slCount++;
                        #region Loop

                        
                        ru.SetTextBorder(ref sheet1, xlsRow, colSrNo, slCount);
                        ru.SetTextBorder(ref sheet1, xlsRow, colPaycode, dtEmpInfo.Rows[i]["EmployeeCode"].ToString());
                        ru.SetTextBorder(ref sheet1, xlsRow, colPFUANNo, dtEmpInfo.Rows[i]["DocNumber"].ToString());//dtEmpInfo.Tables[0].Rows[i][""].ToString()
                        ru.SetTextBorder(ref sheet1, xlsRow, colEmployeeName, dtEmpInfo.Rows[i]["EmployeeName"].ToString());
                        sheet1.Range[xlsRow, colEmployeeName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        ru.SetTextBorder(ref sheet1, xlsRow, colAge, age);
                        ru.SetTextBorder(ref sheet1, xlsRow, colDays, Workingdays);//dtEmpInfo.Tables[0].Rows[i][""].ToString()
                        sheet1.Range[xlsRow, colDays].NumberFormat = ru.NumberFormatNegativeSignDelimeterDecimalTwo();
                        //
                        //sheet1.Range[xlsRow, colWagesAmount].Number = Convert.ToDouble(basic);// + Environment.NewLine + totalPayDay;
                        //sheet1.Range[xlsRow, colWagesAmount].NumberFormat = GetDecimalFormat(basicIntegerInDisb, basicDecimalPoint);
                        //sheet1.Range[xlsRow, colWagesAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //sheet1.Range[xlsRow, colWagesAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        //sheet1.Range[xlsRow, colWagesAmount].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow, colEmployeeShare12parcent].Number = Convert.ToDouble(pfEE) * -1;// + Environment.NewLine + totalPayDay;
                        sheet1.Range[xlsRow, colEmployeeShare12parcent].NumberFormat = GetDecimalFormat(pfEEIntegerInDisb, pfEEDecimalPoint);
                        sheet1.Range[xlsRow, colEmployeeShare12parcent].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colEmployeeShare12parcent].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, colEmployeeShare12parcent].BorderAround(ExcelLineStyle.Hair);

                        sheet1.Range[xlsRow, colVPF].Number = Convert.ToDouble((VPF * -1));
                        sheet1.Range[xlsRow, colVPF].NumberFormat = GetDecimalFormat(vpfIntegerInDisb, vpfDecimalPoint);
                        sheet1.Range[xlsRow, colVPF].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colVPF].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, colVPF].BorderAround(ExcelLineStyle.Hair);

                        sheet1.Range[xlsRow, col3point67parcent].Number = Convert.ToDouble(pfER);
                        sheet1.Range[xlsRow, col3point67parcent].NumberFormat = GetDecimalFormat(pfERIntegerInDisb, pfERDecimalPoint);
                        sheet1.Range[xlsRow, col3point67parcent].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, col3point67parcent].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, col3point67parcent].BorderAround(ExcelLineStyle.Hair);

                        sheet1.Range[xlsRow, colFPFEmployersShare8point33percent].Number = Convert.ToDouble(pension);// + Environment.NewLine + totalPayDay;
                        sheet1.Range[xlsRow, colFPFEmployersShare8point33percent].NumberFormat = GetDecimalFormat(pensionIntegerInDisb, pensionDecimalPoint);
                        sheet1.Range[xlsRow, colFPFEmployersShare8point33percent].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colFPFEmployersShare8point33percent].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, colFPFEmployersShare8point33percent].BorderAround(ExcelLineStyle.Hair);

                        sheet1.Range[xlsRow, colTotal].Number = Convert.ToDouble(pfEE) * -1;// + Environment.NewLine + totalPayDay;
                        sheet1.Range[xlsRow, colTotal].NumberFormat = GetDecimalFormat(pfEEIntegerInDisb, pfEEDecimalPoint);
                        sheet1.Range[xlsRow, colTotal].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colTotal].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, colTotal].BorderAround(ExcelLineStyle.Hair);

                        //colWAGES8point33percent
                        sheet1.Range[xlsRow, colWAGES8point33percent].Number = wages8point33percent;// + Environment.NewLine + totalPayDay;
                        sheet1.Range[xlsRow, colWAGES8point33percent].NumberFormat = GetDecimalFormat(pfEEIntegerInDisb, pfEEDecimalPoint);
                        sheet1.Range[xlsRow, colWAGES8point33percent].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colWAGES8point33percent].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, colWAGES8point33percent].BorderAround(ExcelLineStyle.Hair);

                        ru.SetTextBorder(ref sheet1, xlsRow, colWagesAbove15000, wagesAbove15000);// dtEmpInfo.Tables[0].Rows[i][""].ToString()
                        ru.SetTextBorder(ref sheet1, xlsRow, colRemarksDOL, dtEmpInfo.Rows[i]["DOS"].ToString());// dtEmpInfo.Tables[0].Rows[i][""].ToString()

                        #endregion Loop
                        xlsRow++;

                    }
                    endXlsRow = xlsRow - 1;
                    sheet1.Range[xlsRow, colDays].Text = "Total ";
                    sheet1.Range[xlsRow, colDays].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, colDays].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, colDays].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet1.Range[xlsRow, colDays].BorderAround(ExcelLineStyle.Hair);

                    var summationRowLimit = xlsRow - 1;
                    //getTotal(ref sheet1, xlsRow, colWagesAmount, formulaStartRow, xlsRow - 1, ru);//Wages Sum
                    if (dt.Rows.Count == 1)
                    {
                        getGrandTotal(ref sheet1, xlsRow, 7, GrandTotal, ru);//dtEmpInfo.Tables[0].Rows[i][""].ToString()
                        sheet1.Range[xlsRow, 7].NumberFormat = ru.NumberFormatNegativeSignDelimeterDecimalTwo();
                    }
                    else
                    {
                        int num = dt.Rows.Count;
                        getGrandTotal(ref sheet1, xlsRow, 7 + num, GrandTotal, ru);//dtEmpInfo.Tables[0].Rows[i][""].ToString()
                        sheet1.Range[xlsRow, 7 + num].NumberFormat = ru.NumberFormatNegativeSignDelimeterDecimalTwo();
                    }
                    getTotal(ref sheet1, xlsRow, colEmployeeShare12parcent, formulaStartRow, xlsRow - 1, ru);//EmployeeShare12parcent
                    getTotal(ref sheet1, xlsRow, colVPF, formulaStartRow, xlsRow - 1, ru);//
                    getTotal(ref sheet1, xlsRow, col3point67parcent, formulaStartRow, xlsRow - 1, ru);//EmployeeShare12parcent

                    getTotal(ref sheet1, xlsRow, colFPFEmployersShare8point33percent, formulaStartRow, xlsRow - 1, ru);//colFPFEmployersShare8point33percent
                    getTotal(ref sheet1, xlsRow, colWAGES8point33percent, formulaStartRow, xlsRow - 1, ru);//colTotal
                    getTotal(ref sheet1, xlsRow, colTotal, formulaStartRow, xlsRow - 1, ru);//colTotal
                    getTotal(ref sheet1, xlsRow, colWagesAbove15000, formulaStartRow, xlsRow - 1, ru);//colTotal

                    xlsRow++;
                    sheet1.Range[ru.GetColumnNameForXls(colSrNo) + (xlsRow) + ":" + ru.GetColumnNameForXls(colDays) + xlsRow].Merge();
                    sheet1.Range[xlsRow, colSrNo].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, colSrNo].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, colSrNo].VerticalAlignment = ExcelVAlign.VAlignTop;

                    xlsRow++;
                    sheet1.Range[xlsRow, 2].Text = "No. Of  PF Contributors";
                    sheet1.Range[xlsRow, 2, xlsRow, 4].Merge();
                    sheet1.Range[xlsRow, 2].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 6].Number = slCount;
                    sheet1.Range[xlsRow, 6].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, 6, xlsRow, 7].Merge();



                    xlsRow++;
                    sheet1.Range[xlsRow, 2].Text = "Total- Wages Total";
                    sheet1.Range[xlsRow, 2, xlsRow, 4].Merge();
                    sheet1.Range[xlsRow, 2].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 6].Number = GrandTotal;
                    sheet1.Range[xlsRow, 6].NumberFormat = ru.NumberFormatDecimalFour();
                    sheet1.Range[xlsRow, 6, xlsRow, 7].Merge();

                    xlsRow++;

                    sheet1.Range[xlsRow, 2].Text = "Non Pensionable Wages";
                    sheet1.Range[xlsRow, 2, xlsRow, 4].Merge();
                    sheet1.Range[xlsRow, 2].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 6].Formula = "=SUM(" + ru.GetColumnNameForXls(colWagesAbove15000) + formulaStartRow + ":" + ru.GetColumnNameForXls(colWagesAbove15000) + (summationRowLimit) + ")";
                    sheet1.Range[xlsRow, 6].NumberFormat = ru.NumberFormatDecimalFour();
                    sheet1.Range[xlsRow, 6, xlsRow, 7].Merge();

                    xlsRow++;
                    sheet1.Range[xlsRow, 2].Text = "Pensionable Wages";
                    sheet1.Range[xlsRow, 2, xlsRow, 5].Merge();
                    sheet1.Range[xlsRow, 2].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 6].Formula = "=SUM(" + ru.GetColumnNameForXls(colWAGES8point33percent) + formulaStartRow + ":" + ru.GetColumnNameForXls(colWAGES8point33percent) + (summationRowLimit) + ")";
                    sheet1.Range[xlsRow, 6].NumberFormat = ru.NumberFormatDecimalFour();
                    sheet1.Range[xlsRow, 6, xlsRow, 7].Merge();

                    xlsRow += 2;

                    sheet1.Range[xlsRow, 2].Text = "A) Eployee Contribution";
                    sheet1.Range[xlsRow, 2, xlsRow, 4].Merge();
                    sheet1.Range[xlsRow, 2].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 6].Formula = "=Round(SUM(" + ru.GetColumnNameForXls(colEmployeeShare12parcent) + formulaStartRow + ":" + ru.GetColumnNameForXls(colEmployeeShare12parcent) + (summationRowLimit) + "),0)";
                    sheet1.Range[xlsRow, 6, xlsRow, 7].Merge();

                    xlsRow++;
                    sheet1.Range[xlsRow, 2].Text = "B) VPF";
                    sheet1.Range[xlsRow, 2, xlsRow, 4].Merge();
                    sheet1.Range[xlsRow, 2].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 6].Formula = "=Round(SUM(" + ru.GetColumnNameForXls(colVPF) + formulaStartRow + ":" + ru.GetColumnNameForXls(colVPF) + (summationRowLimit) + "),0)";
                    sheet1.Range[xlsRow, 6, xlsRow, 7].Merge();

                    xlsRow++;
                    sheet1.Range[xlsRow, 2].Text = "C) Employer Contribution(Non Pension)";
                    sheet1.Range[xlsRow, 2, xlsRow, 4].Merge();
                    sheet1.Range[xlsRow, 2].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 6].Formula = "=Round(SUM(" + ru.GetColumnNameForXls(col3point67parcent) + formulaStartRow + ":" + ru.GetColumnNameForXls(col3point67parcent) + (summationRowLimit) + "),0)";
                    sheet1.Range[xlsRow, 6, xlsRow, 7].Merge();

                    xlsRow++;
                    sheet1.Range[xlsRow, 2].Text = "D) Employer Contribution(Pension)";
                    sheet1.Range[xlsRow, 2, xlsRow, 4].Merge();
                    sheet1.Range[xlsRow, 2].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 6].Formula = "=Round(SUM(" + ru.GetColumnNameForXls(colFPFEmployersShare8point33percent) + formulaStartRow + ":" + ru.GetColumnNameForXls(colFPFEmployersShare8point33percent) + (summationRowLimit) + "),0)";
                    sheet1.Range[xlsRow, 6, xlsRow, 7].Merge();


                    xlsRow += 2;
                    var accountSumRow = xlsRow;
                    sheet1.Range[xlsRow, 2].Text = "A/C - 1(A+B+C) ->";
                    sheet1.Range[xlsRow, 2, xlsRow, 4].Merge();
                    //sheet1.Range[xlsRow, 2].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 6].Formula = "=ROUND(SUM(" + ru.GetColumnNameForXls(colEmployeeShare12parcent) + formulaStartRow + ":" + ru.GetColumnNameForXls(colEmployeeShare12parcent) + (summationRowLimit) + ") +" +
                                                        "SUM(" + ru.GetColumnNameForXls(colVPF) + formulaStartRow + ":" + ru.GetColumnNameForXls(colVPF) + (summationRowLimit) + ") + " +
                                                        "SUM(" + ru.GetColumnNameForXls(col3point67parcent) + formulaStartRow + ":" + ru.GetColumnNameForXls(col3point67parcent) + (summationRowLimit) + "),0)";
                    sheet1.Range[xlsRow, 6, xlsRow, 7].Merge();

                    xlsRow++;
                    sheet1.Range[xlsRow, 2].Text = "A/C - 10(D) ->";
                    sheet1.Range[xlsRow, 2, xlsRow, 4].Merge();
                    //sheet1.Range[xlsRow, 2].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 6].Formula = "=ROUND(SUM(" + ru.GetColumnNameForXls(colFPFEmployersShare8point33percent) + formulaStartRow + ":" + ru.GetColumnNameForXls(colFPFEmployersShare8point33percent) + (summationRowLimit) + "),0)";
                    sheet1.Range[xlsRow, 6, xlsRow, 7].Merge();


                    xlsRow++;
                    sheet1.Range[xlsRow, 2].Text = "A/C:2 (0.50% of Wages) ->";
                    sheet1.Range[xlsRow, 2, xlsRow, 4].Merge();
                    //sheet1.Range[xlsRow, 2].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 6].Number = clsStaticInfo.dbl(GrandTotal) * 0.005;
                    sheet1.Range[xlsRow, 6].NumberFormat = "#,##0.00";
                    sheet1.Range[xlsRow, 6, xlsRow, 7].Merge();

                    xlsRow++;
                    sheet1.Range[xlsRow, 2].Text = "A/C:21 (0.50% of Pensionable Wages) ->";
                    sheet1.Range[xlsRow, 2, xlsRow, 4].Merge();
                    //sheet1.Range[xlsRow, 2].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 6].Formula = "=ROUND(SUM(" + ru.GetColumnNameForXls(colWAGES8point33percent) + formulaStartRow + ":" + ru.GetColumnNameForXls(colWAGES8point33percent) + (summationRowLimit) + ") * 0.50%,0)";
                    sheet1.Range[xlsRow, 6, xlsRow, 7].Merge();

                    xlsRow++;
                    sheet1.Range[xlsRow, 2].Text = "Total";
                    sheet1.Range[xlsRow, 2, xlsRow, 4].Merge();
                    sheet1.Range[xlsRow, 2].CellStyle.Font.Bold = true;

                    sheet1.Range[xlsRow, 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 6].Formula = "=ROUND(SUM(" + ru.GetColumnNameForXls(6) + accountSumRow + ":" + ru.GetColumnNameForXls(6) + (xlsRow - 1) + "),0)";
                    sheet1.Range[xlsRow, 6, xlsRow, 7].Merge();

                }
                if (isPFEligible == false)
                {
                    fileHeader = "(PF Not Deducted)";
                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "S.No", 4, 25, ExcelHAlign.HAlignCenter); colSrNo = xlsCol; xlsCol++;
                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "EmpCode", 8, 25, ExcelHAlign.HAlignCenter); colPaycode = xlsCol; xlsCol++;
                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Name of the Employee", 21, 25, ExcelHAlign.HAlignCenter); colEmployeeName = xlsCol; xlsCol++;
                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Basic", 8, 25, ExcelHAlign.HAlignCenter); colWagesAmount = xlsCol; xlsCol++;
                    SetHeaderTextPFund(ref sheet1, xlsRow, xlsCol, "Gross", 9, 25, ExcelHAlign.HAlignRight); colRemarksDOL = xlsCol;
                    endXlsCol = xlsCol;
                    var totalBasic = 0.0m;
                    var totalGross = 0.0m;
                    xlsRow++;

                    formulaStartRow = xlsRow;
                    StringCollection stringCollection = new StringCollection();
                    for (int i = 0; i < dtEmpInfo.Rows.Count; i++)
                    {

                        if (stringCollection.Contains(dtEmpInfo.Rows[i]["EmployeeCodes"].ToString()) == true)
                            continue;

                        stringCollection.Add(dtEmpInfo.Rows[i]["EmployeeCodes"].ToString());

                        dvEmpInfo = new DataView(dtEmpInfo);
                        //dvEmpInfo.Table = dtEmpInfo;

                        dvEmpInfo.RowFilter = "EmployeeCode = '" + dtEmpInfo.Rows[i]["EmployeeCodes"].ToString() + "'";

                        var basic = GetSalaryheadValue(dvEmpInfo, "B");
                        totalBasic += basic;
                        bool basicIntegerInDisb = GetSalaryheadDecimalType(dvEmpInfo, "B");
                        int basicDecimalPoint = GetSalaryheadDecimalPoint(dvEmpInfo, "B");
                        var gross = GetSalaryheadValue(dvEmpInfo, "GROSS");
                        totalGross += gross;
                        bool grossIntegerInDisb = GetSalaryheadDecimalType(dvEmpInfo, "GROSS");
                        int grossDecimalPoint = GetSalaryheadDecimalPoint(dvEmpInfo, "GROSS");

                        var EmployeeName = GetEmployeeName(dvEmpInfo, Convert.ToString(dtEmpInfo.Rows[i]["EmployeeCodes"].ToString()));


                        slCount++;
                        #region Loop

                        ru.SetText(ref sheet1, xlsRow, colSrNo, slCount);
                        ru.SetTextBorder(ref sheet1, xlsRow, colPaycode, dtEmpInfo.Rows[i]["EmployeeCodes"].ToString());
                        ru.SetTextBorder(ref sheet1, xlsRow, colEmployeeName, EmployeeName);



                        sheet1.Range[xlsRow, colWagesAmount].Number = Convert.ToDouble(basic);
                        sheet1.Range[xlsRow, colWagesAmount].NumberFormat = GetDecimalFormat(basicIntegerInDisb, basicDecimalPoint);
                        sheet1.Range[xlsRow, colWagesAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colWagesAmount].BorderAround(ExcelLineStyle.Hair);

                        sheet1.Range[xlsRow, colRemarksDOL].Number = Convert.ToDouble(gross);
                        sheet1.Range[xlsRow, colRemarksDOL].NumberFormat = GetDecimalFormat(grossIntegerInDisb, grossDecimalPoint);
                        sheet1.Range[xlsRow, colRemarksDOL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colRemarksDOL].BorderAround(ExcelLineStyle.Hair);

                        #endregion Loop
                        xlsRow++;

                    }
                    sheet1.Range[xlsRow, colEmployeeName].Text = "Total ";

                    sheet1.Range[xlsRow, colWagesAmount].Number = Convert.ToDouble(totalBasic);
                    //sheet1.Range[xlsRow, colWagesAmount].NumberFormat = "";
                    sheet1.Range[xlsRow, colWagesAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, colWagesAmount].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, colRemarksDOL].Number = Convert.ToDouble(totalGross);
                    //sheet1.Range[xlsRow, colRemarksDOL].NumberFormat = "";
                    sheet1.Range[xlsRow, colRemarksDOL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, colRemarksDOL].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, colEmployeeName, xlsRow, colRemarksDOL].CellStyle.Font.Bold = true;

                    endXlsRow = xlsRow;

                }
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
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
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
                sheet1.Range[xlsRow, xlsCol].Text = "Provident Fund Statement " + fileHeader;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 30;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "For the Month of " + monthName + "," + year; ;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 13;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 25;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["B7"].FreezePanes();
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 8;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.Range[formulaStartRow, 1, endXlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup	
                sheet1.PageSetup.PrintTitleRows = "$A$6:$IV$6";
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
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
                string fileName = monthName + "-" + year + "Provident Fund Statement" + DateTime.Now.ToString("yyMMdd") + ".xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
                // throw ex;
            }
        }

        public Dictionary<string, DataRow> GetEmployeeAttdnSummary(string MonthNo, string YearNo, string PlantId)
        {
            Dictionary<string, DataRow> dicSummary = new Dictionary<string, DataRow>();
            string Sql = @"SELECT SPAT.* FROM SalaryProceAttdnData SPAT INNER JOIN EmployeeInformation EEI ON EEI.SystemId = SPAT.EmpSystemId
                                        WHERE MonthNo = '" + MonthNo + @"' AND YearNo = '" + YearNo + @"' AND EEI.PlantId = '" + PlantId + @"'";

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

        [HttpGet, Authorize]
        public ActionResult GetPFcsv(string month, string year, bool isActive, bool isSeperated)
        {
            DataSet dsEmpInfo = null;
            clsReport objRpt = null;
            try
            {
                CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                objRpt = new clsReport();
                //objRpt.GetPFEmpInfo(out dsEmpInfo, Convert.ToInt32(Month), Year); 
                GetPFCSV(out dsEmpInfo, month, year, identity.PlantId);
                var dataTable = dsEmpInfo.Tables[0];

                //string[] collist = "EmployeeCode,EmployeeName,";

                string attachment = "attachment; filename=" + DateTime.Now.ToString("yyMMdd") + "PFcsv.txt";
                //string attachment = "attachment; filename=MyCsvLol.csv";
                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.ClearHeaders();
                System.Web.HttpContext.Current.Response.ClearContent();
                System.Web.HttpContext.Current.Response.AddHeader("content-disposition", attachment);
                //Response.AddHeader("Content-Disposition", "attachment;filename=myfilename.xls");
                System.Web.HttpContext.Current.Response.ContentType = "application/txt";
                //HttpContext.Current.Response.AddHeader("Pragma", "public");

                StringBuilder builder = new StringBuilder();
                List<string> columnNames = new List<string>();
                List<string> rows = new List<string>();

                foreach (DataColumn column in dataTable.Columns)
                {
                    columnNames.Add(column.ColumnName);
                }

                // builder.Append(string.Join(",", columnNames.ToArray())).Append("\n");

                foreach (DataRow row in dataTable.Rows)
                {
                    List<string> currentRow = new List<string>();

                    foreach (DataColumn column in dataTable.Columns)
                    {
                        object item = row[column];

                        currentRow.Add(item.ToString());
                    }

                    rows.Add(string.Join("#~#", currentRow.ToArray()));
                }

                builder.Append(string.Join(Environment.NewLine, rows.ToArray()));
                //builder.Append(string.Join("\n", rows.ToArray()));

                //Response.Clear();
                //Response.ContentType = "text/csv";
                //Response.AddHeader("Content-Disposition", "attachment;filename=myfilename.csv");
                Response.Write(builder.ToString());
                Response.End();
                return Json(new { FileName = attachment, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion -- Operations

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

        private decimal GetSalaryheadCategoryValue(DataView dvBioDvAC, string catType)
        {
            var basicValue = 0.00m;
            try
            {

                var basic = from r in dvBioDvAC.ToTable().AsEnumerable()
                            where r.Field<string>("HeadCategory") == catType
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
        public void SetHeaderTextPFund(ref IWorksheet sheet, int row, int col, string txt, int width, int RH, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);
            sheet.Range[row, col].RowHeight = RH;
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


        private bool GetSalaryheadCategoryDecimalType(DataView dvBioDvAC, string catType)
        {
            bool basicValue = true;
            try
            {

                var basic = from r in dvBioDvAC.ToTable().AsEnumerable()
                            where r.Field<string>("HeadCategory") == catType
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
        private int GetSalaryheadCategoryDecimalPoint(DataView dvBioDvAC, string catType)
        {
            int basicValue = 0;
            try
            {

                var basic = from r in dvBioDvAC.ToTable().AsEnumerable()
                            where r.Field<string>("HeadCategory") == catType
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
            var PFNo = string.Empty;
            try
            {

                var pfList = from r in dvBioDvAC.ToTable().AsEnumerable()
                             where r.Field<string>("EmployeeCode") == EmpCode
                             select r;
                if (pfList.Count() > 0)
                {

                    DataTable dtt = pfList.CopyToDataTable();
                    PFNo = dtt.Rows[0]["DocNumber"].ToString();

                }
                return PFNo;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private string GetDOS(DataView dvBioDvAC, string EmpCode)
        {
            var DOS = string.Empty;
            try
            {

                var empList = from r in dvBioDvAC.ToTable().AsEnumerable()
                              where r.Field<string>("EmployeeCode") == EmpCode
                              select r;
                if (empList.Count() > 0)
                {

                    DataTable dtt = empList.CopyToDataTable();
                    DOS = dtt.Rows[0]["DOS"].ToString();

                }
                return DOS;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private string GetEarningValueRangeTo(DataView dvBioDvAC, string EmpCode)
        {
            var DOS = string.Empty;
            try
            {

                var empList = from r in dvBioDvAC.ToTable().AsEnumerable()
                              where r.Field<string>("EmployeeCode") == EmpCode
                              select r;
                if (empList.Count() > 0)
                {

                    DataTable dtt = empList.CopyToDataTable();
                    DOS = dtt.Rows[0]["EarningValueRangeTo"].ToString();

                }
                return DOS;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private string GetEmpAge(DataView dvBioDvAC, string EmpCode)
        {
            var Age = "";
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

        private void getTotal(ref IWorksheet sheet1, int xlsRow, int xlsCol, int Row_Total_Start, int Row_Total_end, ReportUtility ru)
        {
            try
            {
                sheet1.Range[xlsRow, xlsCol].Formula = "=SUM(" + ru.GetColumnNameForXls(xlsCol) + Row_Total_Start + ":" + ru.GetColumnNameForXls(xlsCol) + (Row_Total_end) + ")";
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
        /// <summary>
        /// SQL QUERY :: Get Provident Fund Info 
        /// </summary>
        /// <param name="SalaryProcessId"></param>
        /// <param name="dsRef"></param>
        /// <param name="monthName"></param>
        /// <param name="year"></param>
        public DataTable GetPFEmpInfo(string plantId, int monthName, string year, bool isActive, bool isSeperated, bool isPFEligible)
        {
            string strSQL;
            var days = DateTime.DaysInMonth(Convert.ToInt32(year), monthName);//Number of Days in a month
            string monthNameString = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(monthName);//Month Name from Month No
            var date = days + "-" + monthNameString + "-" + year;
            string empStatus = "";
            string strPFEligibleJoin = "";
            string strPFEligibleWC = "";

            if (isActive == true)
            {
                empStatus = @"AND EmpBasic.EmployeeStatus = 'Active'";
            }
            if (isSeperated == true)
            {
                empStatus = @"AND EmpBasic.EmployeeStatus = 'Separated'";
            }
            if (isActive == true && isSeperated == true)
            {
                empStatus = "";
            }
            if (isPFEligible == true)
            {
                strPFEligibleJoin = @"-- INNER JOIN (SELECT EESHE.EmpSystemId,EESHE.SalaryStructureId,EESHE.IsEligible,EESHE.SalaryHeadEnum,SalStruc.SalaryRuleMasterSystemID FROM [EmployeeEligibleForSalaryHeadEnum] EESHE
                                              --     INNER JOIN
                                                   --(SELECT SystemId SalaryId, EmpInfoSystemID, EffectiveDate, SalaryRuleMasterSystemID FROM SalaryInfoDefineMaster
                                                    --UNION
                                                    --SELECT SystemId SalaryId, EmpInfoSystemID, EffectiveDate, SalaryRuleMasterSystemID FROM SalaryInfoBackMaster WHERE EffectiveDate <= '" + date + @"' ) 
                        							--SalStruc on EESHE.SalaryStructureId = SalStruc.SalaryId and EESHE.EmpSystemId = SalStruc.EmpInfoSystemID where EESHE.SalaryHeadEnum = 'PF' AND EESHE.PlantId = '" + plantId + @"' AND IsEligible = 1) 
													--PFELIGIBLE ON SPC.SalaryID = PFELIGIBLE.SalaryStructureId and SPC.EmpInfoSystemID = PFELIGIBLE.EmpSystemId
                                                     --LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = PFELIGIBLE.SalaryRuleMasterSystemID";
                strPFEligibleWC = "";
            }
            if (isPFEligible == false)
            {
                strPFEligibleJoin = @"INNER JOIN (SELECT SystemId SalaryId, EmpInfoSystemID, EffectiveDate,SalaryRuleMasterSystemID FROM SalaryInfoDefineMaster
																				UNION
															    		 SELECT  SystemId SalaryId, EmpInfoSystemID, EffectiveDate,SalaryRuleMasterSystemID FROM SalaryInfoBackMaster WHERE EffectiveDate <= '" + date + @"' ) SalStruct

																		  On SalStruct.EmpInfoSystemID= spc.EmpInfoSystemID and SalStruct.SalaryId = SPC.SalaryID
																				LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = SalStruct.SalaryRuleMasterSystemID";
                strPFEligibleWC = @"WHERE E.SystemId NOT IN (SELECT EESHE.EmpSystemId FROM [EmployeeEligibleForSalaryHeadEnum] EESHE
																				INNER JOIN  
																				(SELECT SystemId SalaryId, EmpInfoSystemID, EffectiveDate,SalaryRuleMasterSystemID FROM SalaryInfoDefineMaster
																				UNION
																				SELECT  SystemId SalaryId, EmpInfoSystemID, EffectiveDate,SalaryRuleMasterSystemID FROM SalaryInfoBackMaster WHERE EffectiveDate <= '" + date + @"' ) 
																				SalStruc on EESHE.SalaryStructureId = SalStruc.SalaryId and EESHE.EmpSystemId = SalStruc.EmpInfoSystemID where EESHE.SalaryHeadEnum = 'PF' AND EESHE.PlantId = '" + plantId + @"' AND IsEligible = 1)";
            }




            try
            {
                strSQL = @"SELECT distinct EmpSlr.EmpInfoSystemID,EmpBasic.EmployeeCode,CONVERT(INT,EmpBasic.EmployeeCode) EmployeeCodes, EmpBasic.EmployeeName,DocNumber,EmpBasic.Age ,
                            REPLACE(Convert(VARCHAR(11), EmpBasic.DOB, 106), ' ', '-') AS DOB,
                            REPLACE(Convert(VARCHAR(11), EmpBasic.DOS, 106), ' ', '-') AS DOS,
                            REPLACE(Convert(VARCHAR(11), EmpBasic.DOJ, 106), ' ', '-') AS DOJ,

                    
								(ISNULL(MMDSA.TotalPresent, 0) + ISNULL(MMDSA.TotalLate, 0)) PresentDays,
								
								ISNULL(MMDSA.TotalHoliDay, 0) HoliDay, ISNULL(MMDSA.TotalWeekOff, 0) WeekOff,
								(ISNULL(MMDSA.TotalLv, 0) + ISNULL(MMDSA.TotalMLv, 0)) LeaveDays,
								(ISNULL(MMDSA.TotalPresent, 0) + ISNULL(MMDSA.TotalLate, 0)+ ISNULL(MMDSA.TotalWeekOff, 0)+ISNULL(MMDSA.TotalHoliDay, 0) + ISNULL(MMDSA.TotalLv, 0) + ISNULL(MMDSA.TotalMLv, 0)) workingDays
                                ,EmpSlr.PlantID, EmpSlr.FromDate, EmpSlr.ToDate, EmpSlr.MonthNo, EmpSlr.YearNo, EmpSlr.PayAbleShSystemID,
                                EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount,
                                EmpSlr.DisbusmentCurrencyID, EmpSlr.DisbusmentAmount, EmpSlr.AcltExcDisbSlrHDID, EmpSlr.AcltExcDisbSlrHDAmt,
                                EmpSlr.AmtDefinitionCurrencyID,
                                EmpSlr.AmtDefinitionCurrencyRate, EmpSlr.IsNetPayEffect
                                ,EMPSLR.cat,EmpSlr.SalaryHead,EmpSlr.HeadCategory,EmpSlr.HeadType,IsCTCComponent,IsGrossComponent
                                ,ISNULL(EmpSlr.IntegerInDisb,0) IntegerInDisb,ISNULL(EmpSlr.DecimalNo,0) DecimalNo
                            FROM
                                    (
										 SELECT E.SystemID, E.EmployeeCode, E.EmployeeName,E.DOB, E.DOJ,E.DOS, E.EmployeeStatus,ed.DocNumber,DATEDIFF(YY,E.DOB,'" + date + @"') As Age,
											 DE.UserName DesignationName,GVDE.UserName GivenDesignationName,
											'' UserGroupSystemID, E.PlantID, F.UserName PlantName, 
											FU.UserName UnitName,DV.UserName DivisionName, DP.UserName DepartmentName,
											S.UserName SectionName, SS.UserName SubSectionName,
											EC.UserName EmpCategoryName--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo
                                            ,egdsgg.GivenDesignationGroup
                                     FROM EmployeeInformation E
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity EN ON MB.EntityId=EN.Id
												LEFT JOIN org.Plant F ON E.PlantID = F.Id
												LEFT JOIN hkp.Designation DE ON E.GivenDesignationId = DE.Id
												LEFT JOIN hkp.Designation GVDE ON E.GivenDesignationId = GVDE.Id
												LEFT JOIN org.Unit FU ON EN.UnitID = FU.Id
												LEFT JOIN org.Division DV ON PR.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON PR.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON PR.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON PR.SubSectionID = SS.Id
												LEFT JOIN
                                                (
                                                SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
												LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
												)EC ON EC.DesignationId=E.GivenDesignationId
												LEFT JOIN (SELECT dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									            ,dg.UserName GivenDesignationGroup
									            FROM MST.DesignationMaster dm
									            LEFT JOIN HKP.DesignationGroup dg ON dg.Id=dm.DesignationGroupId
									            ) egdsgg ON egdsgg.DesignationId=e.GivenDesignationId
									            AND egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
											        LEFT JOIN EmployeeDocument ED ON ED.EmpSystemID = E.SystemId
                                                 AND ComplianceDocumentId = 
												(
												SELECT Id	FROM HKP.ComplianceDocument WHERE ProfileType = 'PF'
												)
                                                " + strPFEligibleWC + @"


									) EmpBasic
                                    INNER JOIN
											(
											 SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
													SPC.EmpInfoSystemID, SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
													SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
													SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
												    SPM.AmtDefinitionCurrencyID,
													SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect
                                                    ,sh.SalaryHead,sh.HeadCategory,sh.HeadType
                                                    ,sh.IsCTCComponent,sh.IsGrossComponent,sh.Cat
                                                    ,CRC.IntegerInDisb, CRC.DecimalNo
											 FROM SalaryProcChild SPC
															INNER JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
																							AND SPM.SystemID IN( SELECT SystemID FROM SalaryProcMaster
                                                                WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = '" + monthName + @"' AND YearNo = '" + year + @"' )
														 INNER JOIN (--0														
																			(SELECT *,'EE' Cat from SalaryHead where HeadCategory in ('PF Employee Contribution'))
																			UNION
																			(SELECT *,'ER' Cat from SalaryHead where HeadCategory in ('PF Employer Contribution'))
																			UNION
																		    (SELECT *,'B' Cat FROM SalaryHead where HeadCategory in ('Basic'))
																			UNION
																			(SELECT *,'P' Cat FROM SalaryHead where HeadCategory in ('Pension'))
																			UNION
																			(SELECT *,'VPF' Cat  FROM SalaryHead  WHERE HeadCategory in ('PF Voluntary'))
																			UNION
																			(SELECT *,'GROSS' Cat  FROM SalaryHead  WHERE HeadCategory in ('GROSS'))
																	)--0 
														SH ON SH.SalaryHeadID=SPC.SalaryHeadID
                                                                " + strPFEligibleJoin + @"
                                                                         --INNER JOIN (SELECT EESHE.EmpSystemId,EESHE.SalaryStructureId,EESHE.IsEligible,EESHE.SalaryHeadEnum,SalStruc.SalaryRuleMasterSystemID FROM [EmployeeEligibleForSalaryHeadEnum] EESHE
																				--INNER JOIN  
																				--(SELECT SystemId SalaryId, EmpInfoSystemID, EffectiveDate,SalaryRuleMasterSystemID FROM SalaryInfoDefineMaster
																				--UNION
																				--SELECT  SystemId SalaryId, EmpInfoSystemID, EffectiveDate,SalaryRuleMasterSystemID FROM SalaryInfoBackMaster WHERE EffectiveDate <= '" + date + @"' ) 
																				--SalStruc on EESHE.SalaryStructureId = SalStruc.SalaryId and EESHE.EmpSystemId = SalStruc.EmpInfoSystemID where EESHE.SalaryHeadEnum = '" + SalaryHeadEnum.PF.ToString() + @"' AND EESHE.PlantId = '" + plantId + @"' AND IsEligible = " + Convert.ToInt32(isPFEligible) + @") 
																				--PFELIGIBLE ON SPC.SalaryID = PFELIGIBLE.SalaryStructureId AND SPC.EmpInfoSystemID = PFELIGIBLE.EmpSystemId
																	
																				--LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID =  SPC.SlrProcMstSystemID

																	LEFT JOIN CurrencyRuleMaster CRM ON CRM.SystemID = SRM.CurrencyRuleSystemID
										LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = SH.SalaryHeadID

														
											) EmpSlr ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID AND EmpBasic.PlantID = EmpSlr.PlantID
                                    LEFT JOIN
		                                    (
											 SELECT EmpSystemID, MonthNo, YearNo, TotalProcDate, TotalPresent, TotalLate,
													TotalAbsent AbsentDays, TotalLv, TotalMLv, TotalCompAssignLv, TotalWeekOff, TotalHoliDay,
													TotalWeekOffHoliDay, TotalOTHr, TotalNormalOTHr, TotalExtraOTHr
				                              FROM SalaryProceAttdnData  WHERE MonthNo = MONTH(CONVERT(DATE,'" + date + @"')) AND YearNo = YEAR(CONVERT(DATE,'" + date + @"'))
											) MMDSA ON EmpSlr.EmpInfoSystemID = MMDSA.EmpSystemID 											   
													   WHERE 
														
													EmpSlr.MonthNo = " + monthName + @" " + empStatus + @"
                                                --AND EmpBasic.SystemId in 
												--	(
												--	SELECT 
												--	  EmpSystemID
												--  FROM [dbo].[PFEligibleEmployee]
												--  WHERE StartDate < '" + date + @"' AND isActive = 1 and IsApproved = 1
												--	)                                       
                                                ORDER BY  EmployeeCodes";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }//end function

        public DataTable GetPFEmpInfoExcel(string plantId, int monthName, string year, bool isActive, bool isSeperated, bool isPFEligible)
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
                strSQL = @"SELECT DISTINCT EmpSlr.EmpInfoSystemID,EmpBasic.EmployeeCode,CONVERT(INT,EmpBasic.EmployeeCode) EmployeeCodes, EmpBasic.EmployeeName,DocNumber,EmpBasic.Age ,
                            REPLACE(Convert(VARCHAR(11), EmpBasic.DOB, 106), ' ', '-') AS DOB,
                            REPLACE(Convert(VARCHAR(11), EmpBasic.DOS, 106), ' ', '-') AS DOS,
                            REPLACE(Convert(VARCHAR(11), EmpBasic.DOJ, 106), ' ', '-') AS DOJ
                            ,EmpSlr.EmpCategoryName,EmpSlr.EmpCategoryId,EmpSlr.WorkingDaysInAMonth
                    
								,(ISNULL(MMDSA.TotalPresent, 0) + ISNULL(MMDSA.TotalLate, 0)) PresentDays,ISNULL(MMDSA.TotalProcDate,0) TotalProcDate
								,ISNULL(MMDSA.AbsentDays,0) TotalAbsent
								,ISNULL(MMDSA.TotalHoliDay, 0) HoliDay, ISNULL(MMDSA.TotalWeekOff, 0) WeekOff,
								(ISNULL(MMDSA.TotalLv, 0) + ISNULL(MMDSA.TotalMLv, 0)) LeaveDays,
								(ISNULL(MMDSA.TotalPresent, 0) + ISNULL(MMDSA.TotalLate, 0)+ ISNULL(MMDSA.TotalWeekOff, 0)+ISNULL(MMDSA.TotalHoliDay, 0) + ISNULL(MMDSA.TotalLv, 0) + ISNULL(MMDSA.TotalMLv, 0)) workingDays
                                ,EmpSlr.PlantID, EmpSlr.FromDate, EmpSlr.ToDate, EmpSlr.MonthNo, EmpSlr.YearNo, EmpSlr.PayAbleShSystemID,
                                EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount,
                                EmpSlr.DisbusmentCurrencyID, EmpSlr.DisbusmentAmount, EmpSlr.AcltExcDisbSlrHDID, EmpSlr.AcltExcDisbSlrHDAmt,
                                EmpSlr.AmtDefinitionCurrencyID,
                                EmpSlr.AmtDefinitionCurrencyRate, EmpSlr.IsNetPayEffect
                                ,EmpSlr.SalaryHead,EmpSlr.HeadCategory,EmpSlr.HeadType,IsCTCComponent,IsGrossComponent
                                ,ISNULL(EmpSlr.IntegerInDisb,0) IntegerInDisb,ISNULL(EmpSlr.DecimalNo,0) DecimalNo
                            FROM
                                    (
										   SELECT E.SystemID,E.PlantId,PFPM.PFPolicyName,isnull(PFPD.EarningValueRangeTo,0) EarningValueRangeTo, E.EmployeeCode, E.EmployeeName,E.DOB, E.DOJ,E.DOS, E.EmployeeStatus,ed.DocNumber,DATEDIFF(YY,E.DOB,'30-Apr-2021') As Age
											
											,E.EmployeeCategorySystemID
                                     FROM EmployeeInformation E
												LEFT JOIN org.Plant F ON E.PlantID = F.Id
													left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=E.LegalDesignationId
                                                    left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                                    left join scs.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId = dm.Id
                                                    left join PFPolicyMaster PFPM ON PFPM.ID = DMC.PFPolicyMasterID
                                                    left join PFPolicyDetails PFPD ON PFPD.PFPolicyMasterID = PFPM.Id
                                                    left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId			
												
											        LEFT JOIN EmployeeDocument ED ON ED.EmpSystemID = E.SystemId
                                                 AND ComplianceDocumentId = 
												(
												SELECT TOP(1) Id	FROM HKP.ComplianceDocument WHERE ProfileType = 'PF'
												)
                                                
                            WHERE  E.PlantId ='" + plantId + @"'
                            
									) EmpBasic
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
                                                    ,SH.IsCTCComponent,SH.IsGrossComponent--,SH.Cat
													,crc.IsDecimalInDisb,crc.DecimalNo,crc.IntegerInDisb
											 FROM SalaryProcChild SPC
														INNER JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
																							AND SPM.SystemID IN  (" + salaryProcessID + @")
									 JOIN SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId  IN (" + salaryProcessID + @")--AND e.SystemId = SPLD.EmpSystemId  
                                                   AND SPLD.PlantId = '" + plantId + @"'   AND SPC.EmpInfoSystemID = SPLD.EmpSystemId 
												LEFT JOIN [HKP].[EmployeeCategory] EC ON EC.Id = SPLD.EmployeeCategoryId

                                                        INNER JOIN (--Salary Head
                                                                              								
																			(SELECT * from SalaryHead where HeadCategory in ('PF Employee Contribution'))
																			UNION
																			(SELECT * from SalaryHead where HeadCategory in ('PF Employer Contribution'))
																			UNION
																		  (SELECT *  FROM SalaryHead where HeadCategory in ('Basic'))
																			UNION
																			(SELECT  * FROM SalaryHead where HeadCategory in ('Pension'))
																			UNION
																			(SELECT  * FROM SalaryHead  WHERE HeadCategory in ('PF Voluntary'))
																			 
                                                                														
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
                                                EESHE.EmpSystemId = SalStruc.EmpInfoSystemID WHERE EESHE.SalaryHeadEnum IN('PF','VPF') AND EESHE.PlantId = '" + plantId + @"'  AND IsEligible = 1

                                                     ) 
													ESICELIGIBLE ON SPC.SalaryID = ESICELIGIBLE.SalaryStructureId and SPC.EmpInfoSystemID = ESICELIGIBLE.EmpSystemId
                                                         LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = ESICELIGIBLE.SalaryRuleMasterSystemID
                                                                LEFT JOIN CurrencyRuleMaster crm on crm.SystemID = sRM.CurrencyRuleSystemID
                                                                LEFT JOIN CurrencyRuleChild crc on crc.MstSystemID = CRM.SystemID and crc.SalaryHeadID=spc.SalaryHeadID			
														WHERE  spld.PlantId = '" + plantId + @"'
                                    ) EmpSlr ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID AND EmpBasic.PlantID = EmpSlr.PlantID
                                    LEFT JOIN
		                                    (
											 SELECT EmpSystemID, MonthNo, YearNo, TotalProcDate, TotalPresent, TotalLate,
													TotalAbsent AbsentDays, TotalLv, TotalMLv, TotalCompAssignLv, TotalWeekOff, TotalHoliDay,
													TotalWeekOffHoliDay, TotalOTHr, TotalNormalOTHr, TotalExtraOTHr
				                              FROM SalaryProceAttdnData  WHERE MonthNo =  MONTH(CONVERT(DATE,'" + date + @"')) AND YearNo = YEAR(CONVERT(DATE,'" + date + @"')) AND PlantId = '" + plantId + @"' 
											) MMDSA ON EmpSlr.EmpInfoSystemID = MMDSA.EmpSystemID 											   
													   WHERE 														
													EmpSlr.MonthNo =  MONTH(CONVERT(DATE,'" + date + @"')) AND EmpSlr.YearNo = YEAR(CONVERT(DATE,'" + date + @"'))  
                                                                " + empStatus + @"
                                                    --AND EmpSlr.HeadCategory IN ('PF Voluntary','Pension','PF Employer Contribution','PF Employee Contribution','Basic','Gross')
                                                ORDER BY  EmployeeCodes";

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

        public DataTable GetPFEmployeeInfo(string plantId, int monthName, string year, bool isActive, bool isSeperated, bool isPFEligible)
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
                strSQL = @" SELECT E.SystemID EmpSystemId,E.PlantId,PFPM.PFPolicyName
                            ,isnull(PFPD.EarningValueRangeTo,0) EarningValueRangeTo
                            , E.EmployeeCode, E.EmployeeName,E.DOB, E.DOJ,E.DOS, E.EmployeeStatus
                            ,ed.DocNumber
                            ,DATEDIFF(YY,E.DOB,'" + date + @"') As Age
											
											,E.EmployeeCategorySystemID,ECA.UserName EmpCategoryName,ECA.UserName EmpCategoryId
                                                    ,ISNULL(ECA.WorkingDaysInAMonth,'') WorkingDaysInAMonth
                                     FROM EmployeeInformation E
												LEFT JOIN org.Plant F ON E.PlantID = F.Id
													left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=E.LegalDesignationId
                                                    left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                                    left join scs.DesignationMasterConfiguration DMC ON DMC.DesignationMasterId = dm.Id and DMC.PlantId = '" + plantId + @"'
                                                    left join PFPolicyMaster PFPM ON PFPM.ID = DMC.PFPolicyMasterID and PFPM.PlantId = '" + plantId + @"'
                                                    left join PFPolicyDetails PFPD ON PFPD.PFPolicyMasterID = PFPM.Id
                                                    left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId		
													Inner Join(select Distinct EmpInfoSystemID,SalaryID from  SalaryProcChild where SlrProcMstSystemID  IN (" + salaryProcessID + @")) spc ON spc.EmpInfoSystemID = e.SystemId 
									  Inner JOIN SalaryProcessLogDetail SPLD ON  SPLD.EmpSystemId = E.SystemId and SPLD.SalaryProcessId  IN (" + salaryProcessID + @")--AND e.SystemId = SPLD.EmpSystemId  
                                   
												left join HKP.EmployeeCategory ECA ON ECA.Id = SPLD.EmployeeCategoryId
											        LEFT JOIN EmployeeDocument ED ON ED.EmpSystemID = E.SystemId
                                                 AND ComplianceDocumentId = 
												(
												SELECT TOP(1) Id	FROM HKP.ComplianceDocument WHERE ProfileType = 'PF'
												)
										
												WHERE                                                   
                                             E.SystemId IN (
											 Select EmpSystemId from 
											      [EmployeeEligibleForSalaryHeadEnum] EESHE   
                                                left join [dbo].[EmployeeCodeType] ect on ect.Id=e.EmployeeCodeTypeId
                                            where EESHE.SalaryStructureId = spc.SalaryId AND
                                                EESHE.EmpSystemId = e.SystemId 
												AND EESHE.SalaryHeadEnum IN('PF','VPF')   AND IsEligible = 1 and ISNULL(ect.IsOutSider,0) =0
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

        public Dictionary<string, List<DataRow>> GetEmpPFSalaryInfo(string plantId, int monthName, string year, bool isActive, bool isSeperated, bool isPFEligible)
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
														AND SH.HeadCategory IN ('PF Voluntary','Pension','Basic','PF Employer Contribution','PF Employee Contribution') 
														
                                            Inner join EmployeeInformation EEI ON EEI.SystemId = SPC.EmpInfoSystemID
														LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EEI.SalaryRuleMasterSystemID
                                                                LEFT JOIN CurrencyRuleMaster crm on crm.SystemID = sRM.CurrencyRuleSystemID
                                                                LEFT JOIN CurrencyRuleChild crc on crc.MstSystemID = CRM.SystemID and crc.SalaryHeadID=spc.SalaryHeadID			
                                                        left join [dbo].[EmployeeCodeType] ect on ect.Id=EEI.EmployeeCodeTypeId
														WHERE  EEI.PlantId = '" + plantId + @"' and ISNULL(ect.IsOutSider,0) =0  order by EmpInfoSystemID";

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
                                            left join PFPolicyMaster pf on pf.PlantID = EEI.PlantId
											join PFPolicySalaryHead pfs on pfs.PFPolicyMasterId=pf.ID 
											  INNER JOIN SalaryHead SH ON SH.SalaryHeadID=SPC.SalaryHeadID and SH.SalaryHeadID=pfs.SalaryHeadID and sh.SalaryHeadID in(pfs.SalaryHeadID)
														LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EEI.SalaryRuleMasterSystemID
                                                                LEFT JOIN CurrencyRuleMaster crm on crm.SystemID = sRM.CurrencyRuleSystemID
                                                                LEFT JOIN CurrencyRuleChild crc on crc.MstSystemID = CRM.SystemID and crc.SalaryHeadID=spc.SalaryHeadID			
														WHERE  EEI.PlantId = '" + plantId + @"' order by EmpInfoSystemID";

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

        public void GetEmpPFSalaryHead(string plantId, int monthName, string year, bool isActive, bool isSeperated, bool isPFEligible, out DataTable dt)
        {
            string strSQL;
            try
            {
                strSQL = @"select pfs.SalaryHeadID,sh.SalaryHead from PFPolicyMaster pf 
		                            join PFPolicySalaryHead pfs on pfs.PFPolicyMasterId=pf.ID 
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

        /// <summary>
        /// PFCSV Query
        /// </summary>
        /// <param name="dsRef"></param>
        /// <param name="pmonth"></param>
        /// <param name="pyear"></param>
        /// <param name="salProcId"></param>
        /// <param name="plantId"></param>
        public void GetPFCSV(out DataSet dsRef, string pmonth, string pyear, string plantId)
        {
            string _date = "01-" + CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(Convert.ToInt32(pmonth)) + "-" + pyear;
            DateTime _lastdate = Convert.ToDateTime(_date).AddMonths(1).AddDays(-1);
            //var days = DateTime.DaysInMonth(Convert.ToInt32(year), monthName);//Number of Days in a month
            //string monthNameString = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(monthName);//Month Name from Month No
            //var date = days + "-" + monthNameString + "-" + year;
            string strSQL;



            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            try
            {
                strSQL = @"select ed.DocNumber UANNo
                                --,e.SystemId
                                --,e.EmployeeCode
                                ,e.EmployeeName
                                ,CONVERT(INT,gross.DisbusmentAmount) Gross
                                ,CONVERT(INT,[Basic].DisbusmentAmount) [Basic1]
                                --,CONVERT(INT,[Basic].DisbusmentAmount) [Basic2]
                                , Basic2=case  when [Basic].DisbusmentAmount>15000 then 15000 else CONVERT(INT,[Basic].DisbusmentAmount) end
                                , Basic3=case  when [Basic].DisbusmentAmount>15000 then 15000 else CONVERT(INT,[Basic].DisbusmentAmount) end
                                --,[Basic].DisbusmentAmount [Basic32]
                                --,CONVERT(INT,spc.DisbusmentAmount*(-1)) EmployeeContribution
                                ,CONVERT(INT,spc.DisbusmentAmount*(-1)) + Convert(Int,ISNULL(VPF.DisbusmentAmount,0) *-1) EmployeeContribution
                                  ,CONVERT(INT,ISNULL(pension.DisbusmentAmount,0)) pension
                                ,CONVERT(INT,er.DisbusmentAmount) EmployerContribution
                                --,TotalPresent+TotalLv+TotalMLv+TotalCompAssignLv+TotalWeekOff+TotalHoliDay as PayDays
                               -- ,CONVERT(INT,ISNULL(AbsentDays,0)) PayDays
                                 -- , AbsentDays = case  when AbsentDays = floor(AbsentDays)
                              -- then convert(nvarchar,convert(int, AbsentDays)) -- change to int if no decimal part
                               -- else convert(nvarchar,convert(decimal(10, 1), AbsentDays)) -- else return one decimal
                               -- end
                                ,floor(AbsentDays) AbsentDays
                                ,0 Advance
                                 FROM EmployeeInformation e 
                                     LEFT JOIN EmployeeDocument ED ON ED.EmpSystemID = E.SystemId
                                     AND ComplianceDocumentId = 
									 (
										SELECT TOP(1) Id	FROM HKP.ComplianceDocument WHERE ProfileType = 'PF'
								     )
                                 LEFT JOIN
	                                (
		                                SELECT EmpSystemID, MonthNo, YearNo, TotalProcDate, TotalPresent, TotalLate,
			                                TotalAbsent AbsentDays, TotalLv, TotalMLv, TotalCompAssignLv, TotalWeekOff, TotalHoliDay,
			                                TotalWeekOffHoliDay, TotalOTHr, TotalNormalOTHr, TotalExtraOTHr
		                                FROM SalaryProceAttdnData where MonthNo=" + pmonth + @" and YearNo='" + pyear + @"'
	                                ) MMDSA ON e.SystemID = MMDSA.EmpSystemID 
 
  				                                --GROSS
                                LEFT JOIN (select * from SalaryProcChild where SalaryHeadID in (SELECT SalaryHeadID from SalaryHead where HeadCategory in ('Gross'))
		                                  ) gross  ON gross.EmpInfoSystemID=e.SystemId
                                INNER JOIN (select * FROM SalaryProcMaster WHERE YearNo='" + pyear + @"' and MonthNo=" + pmonth + @" ) SPMg on gross.SlrProcMstSystemID = SPMg.SystemID
 				                                --BASIC
                                LEFT JOIN (select * from SalaryProcChild where SalaryHeadID in (SELECT SalaryHeadID from SalaryHead where HeadCategory in ('Basic'))
		                                  ) Basic  ON Basic.EmpInfoSystemID=e.SystemId
                                INNER JOIN (select * FROM SalaryProcMaster WHERE  YearNo='" + pyear + @"' and MonthNo=" + pmonth + @" ) SPMBasic on [Basic].SlrProcMstSystemID = SPMBasic.SystemID
				                                --EE
                                LEFT JOIN (select * from SalaryProcChild where SalaryHeadID in (SELECT SalaryHeadID from SalaryHead where HeadCategory in ('PF Employee Contribution'))
		                                  ) SPC  ON spc.EmpInfoSystemID=e.SystemId
                                INNER JOIN (select * FROM SalaryProcMaster WHERE  YearNo='" + pyear + @"' and MonthNo=" + pmonth + @" ) SPMee on SPC.SlrProcMstSystemID = SPMee.SystemID
				                                --ER
                                LEFT JOIN (select * from SalaryProcChild where SalaryHeadID in (SELECT SalaryHeadID from SalaryHead where HeadCategory in ('PF Employer Contribution'))
		                                  ) er  ON er.EmpInfoSystemID=e.SystemId
                                INNER JOIN (select * FROM SalaryProcMaster WHERE  YearNo='" + pyear + @"' and MonthNo=" + pmonth + @" ) SPMer on er.SlrProcMstSystemID = SPMer.SystemID
				                                --PENSION
                                    LEFT JOIN (SELECT C.* FROM SalaryProcChild C
								 INNER JOIN (SELECT * FROM SalaryProcMaster WHERE   YearNo='" + pyear + @"' and MonthNo=" + pmonth + @" ) SPMpension on C.SlrProcMstSystemID = SPMpension.SystemID
								WHERE C.SalaryHeadID IN (SELECT SalaryHeadID from SalaryHead where HeadCategory in ('Pension'))
		                                  ) PENSION  ON pension.EmpInfoSystemID=e.SystemId
                                       --Vlountary PF
                                    LEFT JOIN (SELECT C.* FROM SalaryProcChild C
								 INNER JOIN (SELECT * FROM SalaryProcMaster WHERE   YearNo='" + pyear + @"' and MonthNo=" + pmonth + @" ) SPMVPF on C.SlrProcMstSystemID = SPMVPF.SystemID
								WHERE C.SalaryHeadID IN (SELECT SalaryHeadID from SalaryHead where HeadCategory in ('PF Voluntary'))
		                                  ) VPF  ON VPF.EmpInfoSystemID=e.SystemId
                                WHERE (E.EmployeeStatus = 'Active' OR (DOS IS NULL or CONVERT(DATE,DOS) >= CONVERT(DATE,'" + _date + @"') ))   AND e.PlantId='" + plantId + @"' AND e.SystemId in 
			                                (SELECT EESHE.EmpSystemId FROM [EmployeeEligibleForSalaryHeadEnum] EESHE
																			INNER JOIN  
																			( -- Salary Structure Start
                                                                        SELECT * from  (select SystemID SalaryId,EffectiveDate efd,EmpInfoSystemID eid ,SalaryRuleMasterSystemID from SalaryInfoDefineMaster
                                                                                    union
                                                                                    select SystemID SalaryId,EffectiveDate efd,EmpInfoSystemID eid ,SalaryRuleMasterSystemID from SalaryInfoBackMaster
                                                                                    )
                                                                                     mm 
                                                                                    inner join (
                                                                                    select MAX(EffectiveDate)EffectiveDate,EmpInfoSystemID from (
                                                                                    select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster where IsApproved=1 and EffectiveDate<='" + _lastdate + @"'
                                                                                    union
                                                                                     select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster where IsApproved=1 and EffectiveDate<='" + _lastdate + @"'
                                                                                     ) x 
                                                                                     group by EmpInfoSystemID
                                                                                    )m on mm.efd=m.EffectiveDate and m.EmpInfoSystemID=mm.eid
                                                                                   -- Salary Structure End
                                                                                    ) 
																				SalStruc on EESHE.SalaryStructureId = SalStruc.SalaryId and EESHE.EmpSystemId = SalStruc.EmpInfoSystemID
																				
																				
																				 where EESHE.SalaryHeadEnum  IN('PF','VPF') --AND EESHE.PlantId = '20188'-- as OF Monir via suggested for Invalid plant data Entry in Eligible table  
																				 AND IsEligible = 1
																				
																				)";

                con.getDataSet(strSQL, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //objCon = null;
            }
        }//end function

    }
}