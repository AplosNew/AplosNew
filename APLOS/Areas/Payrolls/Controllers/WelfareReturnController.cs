using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
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
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.Helpers.ReportUtility;
using static Library.Service.HumanResources.PayRegisterBDReportService;
using static OTSBD.clsReport;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class WelfareReturnController : BaseController
    {
        Library.HumanResource.Report.Payroll.clsPayRegister _clspayRegisterBDReportService = new Library.HumanResource.Report.Payroll.clsPayRegister();

        #region Constructor

        private readonly IPayRegisterBDReportService _payRegisterBDReportService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;



        public WelfareReturnController(
              IPayRegisterBDReportService payRegisterBDReportService, IEmployeeProfileService employeeProfileService,
              ISqlRepository sqlRepository
            )
        {
            _payRegisterBDReportService = payRegisterBDReportService;
            _employeeProfileService = employeeProfileService;
            _sqlRepository = sqlRepository;
            _clspayRegisterBDReportService = new Library.HumanResource.Report.Payroll.clsPayRegister();
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

        [HttpGet, Authorize]
        public ActionResult GetSalaryprocessIdCbo(string month, string year, string IsCompletedMonth)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_clspayRegisterBDReportService.GetSalaryprocessIdCbo(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, month, year, IsCompletedMonth), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetLanguageIdCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeProfileService.GetDefaultCbo(identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPayGroupCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_clspayRegisterBDReportService.GetPayGroupCbo(identity.IsControlAdmin, identity.IsSysAdmin, identity.UserId), JsonRequestBehavior.AllowGet);
        }



        [HttpGet, Authorize]
        public ActionResult GetSalaryTopSheet(string month, string year, string salaryProcessId, string divisionId, string unitId, string sectionId, string subSectionId, string departmentId, string payGroupId, string employeeCategoryId, string paymentDate, string paymentMode, string languageId, string selPaymentMode, string selEmpCatg, string SalaryTopSheetCategory)
        {
            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month.ToInt());
            var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
            PayRegisterParamList PayRegisterParam = new PayRegisterParamList();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            PayRegisterParam.PlantId = identity.PlantId;
            PayRegisterParam.CompanyGroupId = identity.CompanyGroupId;
            PayRegisterParam.CompanyId = identity.CompanyId;
            PayRegisterParam.FromDate = 1 + "-" + monthName + "-" + year;
            PayRegisterParam.ToDate = daysInMonth + "-" + monthName + "-" + year;
            PayRegisterParam.Month = month;
            PayRegisterParam.Year = year;
            PayRegisterParam.SalaryProcessId = salaryProcessId;
            PayRegisterParam.UnitId = unitId;
            PayRegisterParam.DivisionId = divisionId;
            PayRegisterParam.SubSectionId = subSectionId;
            PayRegisterParam.SectionId = sectionId;
            PayRegisterParam.DepartmentId = departmentId;
            PayRegisterParam.PayGroup = payGroupId;
            PayRegisterParam.EmpCategoryId = employeeCategoryId;
            PayRegisterParam.PaymentMode = paymentMode;
            PayRegisterParam.LanguageId = languageId;
            var ldateOfMonth = daysInMonth + "-" + monthName + "-" + year;
            var fdateOfMonth = "1" + "-" + monthName + "-" + year;

            var fileName = monthName + "-" + year + "SalaryTopSheet" + DateTime.Now.ToString("yyMMdd") + ".xls";
            var workbook = SalaryTopSheetExcel(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, month, year, salaryProcessId, fdateOfMonth, ldateOfMonth, SalaryTopSheetCategory);
            workbook.Version = ExcelVersion.Excel97to2003;
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
            return null;
        }

        [HttpGet, Authorize]
        public ActionResult GetDynamicSalaryTopSheet(string month, string year, string salaryProcessId, string divisionId, string unitId, string sectionId, string subSectionId, string departmentId, string payGroupId, string employeeCategoryId, string paymentDate, string paymentMode, string languageId, string selPaymentMode, string selEmpCatg, string SalaryTopSheetCategory)
        {
            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month.ToInt());
            var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
            PayRegisterParamList PayRegisterParam = new PayRegisterParamList();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            PayRegisterParam.PlantId = identity.PlantId;
            PayRegisterParam.CompanyGroupId = identity.CompanyGroupId;
            PayRegisterParam.CompanyId = identity.CompanyId;
            PayRegisterParam.FromDate = 1 + "-" + monthName + "-" + year;
            PayRegisterParam.ToDate = daysInMonth + "-" + monthName + "-" + year;
            PayRegisterParam.Month = month;
            PayRegisterParam.Year = year;
            PayRegisterParam.SalaryProcessId = salaryProcessId;
            PayRegisterParam.UnitId = unitId;
            PayRegisterParam.DivisionId = divisionId;
            PayRegisterParam.SubSectionId = subSectionId;
            PayRegisterParam.SectionId = sectionId;
            PayRegisterParam.DepartmentId = departmentId;
            PayRegisterParam.PayGroup = payGroupId;
            PayRegisterParam.EmpCategoryId = employeeCategoryId;
            PayRegisterParam.PaymentMode = paymentMode;
            PayRegisterParam.LanguageId = languageId;
            var ldateOfMonth = daysInMonth + "-" + monthName + "-" + year;
            var fdateOfMonth = "1" + "-" + monthName + "-" + year;

            var fileName = monthName + "-" + year + "SalaryTopSheet" + DateTime.Now.ToString("yyMMdd") + ".xls";
            var workbook = DynamicSalaryTopSheetExcel(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, month, year, salaryProcessId, fdateOfMonth, ldateOfMonth, SalaryTopSheetCategory);
            workbook.Version = ExcelVersion.Excel97to2003;
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
            return null;
        }

        #endregion -- Operations

        public IWorkbook SalaryTopSheetExcel(string groupId, string companyId, string plantId, string monthNo, string yearNo, string salaryPorcId, string monthStartDate, string monthEndDate, string SalaryTopSheetCategory)
        {

            #region Variable

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = new ReportUtility();
            clsReport objRpt = new clsReport();
            #endregion Variable

            try
            {
                var baseCurrency = "";

                var dtSalaryTopSheet = GetSalaryTopSheetDataSql(groupId, companyId, plantId, monthNo, yearNo, salaryPorcId, monthStartDate, monthEndDate, SalaryTopSheetCategory);

                var Currency = GetPlantCurrency();
                if (Currency.Rows.Count > 0)
                {
                    baseCurrency = Currency.Rows[0]["Id"].ToString();
                }

                #region Variable             

                var FactoryName = "";
                var CmpName = "";


                #endregion Variable
                DateTime dtFrmDt = DateTime.Now;
                DateTime dtEndDate = DateTime.Now;

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;
                sheet1.IsDisplayZeros = false;

                var colSl = 0;
                var colSectionName = 0;
                var colNumberofEmployee = 0;
                var colTotalSalaryGross = 0;
                var colTotalOTHours = 0;
                var colTotalOTSalaryAmount = 0;
                var colTotalNetAmount = 0;
                var colDepartment = 0;
                var colEmployeeCatagory = 0;


                #region------------------Column Header------------------
                xlsRow = 4;
                xlsCol = 1;



                //sheet1[xlsRow, xlsCol].ColumnWidth = 15;
                if (SalaryTopSheetCategory == "PayrollGroup")
                {
                    SetCellHeaderValue("SL", sheet1, xlsRow, ref xlsCol, out colSl);

                    sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    SetCellHeaderValue("Section Name", sheet1, xlsRow, ref xlsCol, out colSectionName);

                }
                if (SalaryTopSheetCategory == "DepartmentEmployeeCategory")
                {
                    sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    SetCellHeaderValue("Department", sheet1, xlsRow, ref xlsCol, out colDepartment);

                    sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    SetCellHeaderValue("Employee Catagory", sheet1, xlsRow, ref xlsCol, out colEmployeeCatagory);

                }


                //sheet1[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                SetCellHeaderValue("Numberof Employee", sheet1, xlsRow, ref xlsCol, out colNumberofEmployee);

                //sheet1[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                SetCellHeaderValue("Total Salary Amount", sheet1, xlsRow, ref xlsCol, out colTotalSalaryGross);

                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                SetCellHeaderValue("Total Overtime Hours", sheet1, xlsRow, ref xlsCol, out colTotalOTHours);

                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                SetCellHeaderValue("Total Overtime Amount", sheet1, xlsRow, ref xlsCol, out colTotalOTSalaryAmount);

                //sheet1[xlsRow, xlsCol].ColumnWidth = 15;
                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                SetCellHeaderValue("Gross Pay", sheet1, xlsRow, ref xlsCol, out colTotalNetAmount);


                endXlsCol = --xlsCol;

                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Blue;
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Color=ExcelKnownColors.White;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);

                #endregion------------------Column Header------------------

                int RowIndex = xlsRow++;
                xlsRow++;
                var SL = 0;

                var totalValueForWord = 0.00;

                #region ----------------------Data-----------------------
                int startRow = xlsRow;

                var _empDEP = "";
                String lastEmpCat = "";
                ArrayList al = new ArrayList();
                var catFRow = xlsRow;
                if (dtSalaryTopSheet.Rows.Count > 0)
                {

                    for (int i = 0; i <= dtSalaryTopSheet.Rows.Count - 1; i++)
                    {

                        if (SalaryTopSheetCategory == "PayrollGroup")
                        {
                            SL++;

                            sheet1[xlsRow, colSl].Text = SL.ToString();
                            sheet1[xlsRow, colSectionName].Text = dtSalaryTopSheet.Rows[i]["PayrollGroup"].ToString();
                        }

                        if (SalaryTopSheetCategory == "DepartmentEmployeeCategory")
                        {


                            if (_empDEP != dtSalaryTopSheet.Rows[i]["Department"].ToString() && string.IsNullOrEmpty(dtSalaryTopSheet.Rows[i]["Department"].ToString()) == false)
                            {
                                _empDEP = dtSalaryTopSheet.Rows[i]["Department"].ToString();

                                #region Subtotal
                                if (catFRow < xlsRow)
                                {
                                    lastEmpCat = _empDEP;
                                    al.Add(xlsRow);
                                    SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                                    sheet1.Range[xlsRow, 1, xlsRow, (colNumberofEmployee - 1)].Merge();
                                    sheet1.Range[xlsRow, colNumberofEmployee].Formula = "=SUM(" + ru.GetColumnNameForXls(colNumberofEmployee) + catFRow + ":" + ru.GetColumnNameForXls(colNumberofEmployee) + (xlsRow - 1) + ")";
                                    sheet1.Range[xlsRow, colNumberofEmployee].Formula = "=SUM(" + ru.GetColumnNameForXls(colNumberofEmployee) + catFRow + ":" + ru.GetColumnNameForXls(colNumberofEmployee) + (xlsRow - 1) + ")";
                                    sheet1.Range[xlsRow, colTotalSalaryGross].Formula = "=SUM(" + ru.GetColumnNameForXls(colTotalSalaryGross) + catFRow + ":" + ru.GetColumnNameForXls(colTotalSalaryGross) + (xlsRow - 1) + ")";
                                    sheet1.Range[xlsRow, colTotalOTHours].Formula = "=SUM(" + ru.GetColumnNameForXls(colTotalOTHours) + catFRow + ":" + ru.GetColumnNameForXls(colTotalOTHours) + (xlsRow - 1) + ")";
                                    sheet1.Range[xlsRow, colTotalOTSalaryAmount].Formula = "=SUM(" + ru.GetColumnNameForXls(colTotalOTSalaryAmount) + catFRow + ":" + ru.GetColumnNameForXls(colTotalOTSalaryAmount) + (xlsRow - 1) + ")";
                                    sheet1.Range[xlsRow, colTotalNetAmount].Formula = "=SUM(" + ru.GetColumnNameForXls(colTotalNetAmount) + catFRow + ":" + ru.GetColumnNameForXls(colTotalNetAmount) + (xlsRow - 1) + ")";
                                    sheet1.Range[xlsRow, colNumberofEmployee, xlsRow, colTotalNetAmount].CellStyle.Font.Bold = true;
                                    xlsRow++;
                                }
                                #endregion
                                SetCellText(sheet1, xlsRow, colDepartment, _empDEP);
                                //_department = dtAttdnSummary.Rows[i]["Department"].ToString();
                                //SetCellText(sheet1, xlsRow, colDepartment, _department);
                                //_section = dtAttdnSummary.Rows[i]["Section"].ToString();
                                //SetCellText(sheet1, xlsRow, colSec, _section);
                                //_DesignationGroup = dtAttdnSummary.Rows[i]["DesignationGroup"].ToString();
                                //SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);

                                if (catFRow < xlsRow)
                                {

                                    catFRow = xlsRow;
                                }
                            }
                            //sheet1[xlsRow, colDepartment].Text = dtSalaryTopSheet.Rows[i]["Department"].ToString();
                            sheet1[xlsRow, colEmployeeCatagory].Text = dtSalaryTopSheet.Rows[i]["EmployeeCatagory"].ToString();
                        }

                        sheet1[xlsRow, colNumberofEmployee].Number = clsStaticInfo.dbl(dtSalaryTopSheet.Rows[i]["NoOfEmployees"].ToString());
                        sheet1[xlsRow, colTotalSalaryGross].Number = clsStaticInfo.dbl(dtSalaryTopSheet.Rows[i]["GrossDisbusmentAmount"].ToString());
                        sheet1[xlsRow, colTotalOTHours].Number = clsStaticInfo.dbl(dtSalaryTopSheet.Rows[i]["TotalOThr"].ToString());
                        sheet1[xlsRow, colTotalOTSalaryAmount].Number = clsStaticInfo.dbl(dtSalaryTopSheet.Rows[i]["TotalOTAmount"].ToString());
                        totalValueForWord += clsStaticInfo.dbl(dtSalaryTopSheet.Rows[i]["NetPayable"].ToString());
                        sheet1[xlsRow, colTotalNetAmount].Number = clsStaticInfo.dbl(dtSalaryTopSheet.Rows[i]["NetPayable"].ToString());

                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);

                        xlsRow++;

                    }
                }
                var lastRow = xlsRow - 1;
                if (SalaryTopSheetCategory == "DepartmentEmployeeCategory")
                {
                    #region Last subtotal
                    al.Add(xlsRow);
                    SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                    sheet1.Range[xlsRow, 1, xlsRow, (colNumberofEmployee - 1)].Merge();
                    sheet1.Range[xlsRow, colNumberofEmployee].Formula = "=SUM(" + ru.GetColumnNameForXls(colNumberofEmployee) + catFRow + ":" + ru.GetColumnNameForXls(colNumberofEmployee) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colTotalSalaryGross].Formula = "=SUM(" + ru.GetColumnNameForXls(colTotalSalaryGross) + catFRow + ":" + ru.GetColumnNameForXls(colTotalSalaryGross) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colTotalOTHours].Formula = "=SUM(" + ru.GetColumnNameForXls(colTotalOTHours) + catFRow + ":" + ru.GetColumnNameForXls(colTotalOTHours) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colTotalOTSalaryAmount].Formula = "=SUM(" + ru.GetColumnNameForXls(colTotalOTSalaryAmount) + catFRow + ":" + ru.GetColumnNameForXls(colTotalOTSalaryAmount) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colTotalNetAmount].Formula = "=SUM(" + ru.GetColumnNameForXls(colTotalNetAmount) + catFRow + ":" + ru.GetColumnNameForXls(colTotalNetAmount) + (xlsRow - 1) + ")";
                    //sheet1.Range[xlsRow, colWeekOffHoliday].Formula = "=SUM(" + ru.GetColumnNameForXls(colWeekOffHoliday) + catFRow + ":" + ru.GetColumnNameForXls(colWeekOffHoliday) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colNumberofEmployee, xlsRow, colTotalNetAmount].CellStyle.Font.Bold = true;
                    xlsRow++;
                    #endregion
                    #region Grand Total
                    SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                    sheet1.Range[xlsRow, 1, xlsRow, (colNumberofEmployee - 1)].Merge();


                    sheet1.Range[xlsRow, colNumberofEmployee].Formula = GetFormulaGrandTotal(al, colNumberofEmployee);
                    sheet1.Range[xlsRow, colTotalSalaryGross].Formula = GetFormulaGrandTotal(al, colTotalSalaryGross);
                    sheet1.Range[xlsRow, colTotalOTHours].Formula = GetFormulaGrandTotal(al, colTotalOTHours);
                    sheet1.Range[xlsRow, colTotalOTSalaryAmount].Formula = GetFormulaGrandTotal(al, colTotalOTSalaryAmount);
                    sheet1.Range[xlsRow, colTotalNetAmount].Formula = GetFormulaGrandTotal(al, colTotalNetAmount);
                    //sheet1.Range[xlsRow, colWeekOffHoliday].Formula = GetFormulaGrandTotal(al, colWeekOffHoliday);
                    sheet1.Range[xlsRow, colNumberofEmployee, xlsRow, colTotalNetAmount].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, colTotalSalaryGross, xlsRow, colTotalSalaryGross].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, colTotalOTHours, xlsRow, colTotalOTHours].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, colTotalOTSalaryAmount, xlsRow, colTotalOTSalaryAmount].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, colTotalNetAmount, xlsRow, colTotalNetAmount].CellStyle.Font.Bold = true;


                    #endregion
                }
                if (SalaryTopSheetCategory == "PayrollGroup")
                {
                    ru.SetText(ref sheet1, xlsRow, 2, "Total: ", true);


                    for (int i = colNumberofEmployee; i < endXlsCol + 1; i++)
                    {

                        sheet1.Range[xlsRow, i].Formula = "=SUM(" + ru.GetColumnNameForXls(i) + startRow + ":" + ru.GetColumnNameForXls(i) + (lastRow) + ")";
                        sheet1.Range[xlsRow, i].NumberFormat = ru.NumberFormatDecimalTwo();
                        sheet1.Range[xlsRow, i].CellStyle.Font.Bold = true;

                    }
                    // sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    // sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                    xlsRow += 2;
                    //var colLast = 1;
                    var colLast = xlsCol;

                    ru.SetText(ref sheet1, xlsRow, 1, "In Word:", true);

                    //var chequeAmount = 0.00;
                    //chequeAmount = Convert.ToDouble(dtSalaryTopSheet.Compute(@"Sum(GrossDisbusmentAmount2)", ""));

                    sheet1.Range[ru.GetColumnNameForXls(2) + xlsRow].Text = ru.InWord(totalValueForWord, baseCurrency);
                    sheet1.Range[ru.GetColumnNameForXls(2) + xlsRow + ":" + ru.GetColumnNameForXls(colLast) + xlsRow].Merge();
                    sheet1.Range[ru.GetColumnNameForXls(2) + xlsRow].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[ru.GetColumnNameForXls(2) + xlsRow].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet1.Range[ru.GetColumnNameForXls(2) + xlsRow].CellStyle.Font.Bold = true;

                    sheet1.UsedRange.NumberFormat = "#,##0.00_);(#,##0.00)";
                    sheet1.Range[startRow, colNumberofEmployee, lastRow + 1, colNumberofEmployee].NumberFormat = "#,##0_);(#,##0)";
                    sheet1.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                }


                xlsRow += 3;
                sheet1.Range[xlsRow, 2].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                ru.SetTextMiddle(ref sheet1, xlsRow, 2, "GM(AHRC)", true);
                //sheet1[ru.GetColumnNameForXls(1) + xlsRow + ":" + ru.GetColumnNameForXls(2) + xlsRow].Merge();

                sheet1.Range[xlsRow, 4].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                ru.SetTextMiddle(ref sheet1, xlsRow, 4, "CEO", true);

                sheet1.Range[xlsRow, 6].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                ru.SetTextMiddle(ref sheet1, xlsRow, 6, "MD/Chairman", true);
                //sheet1[ru.GetColumnNameForXls(5) + xlsRow + ":" + ru.GetColumnNameForXls(6) + xlsRow].Merge();
                xlsRow++;

                #endregion

                #region ******************Report Header******************

                DataSet dsCmp;
                objRpt.SelectedPlantWiseCompany(plantId, "", out dsCmp);
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
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddress"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 1].Text = FactoryName;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 22;
                sheet1.Range[xlsRow, 1].CellStyle.Font.FontName = "Aerial Narrow";

                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 26;

                xlsRow++;
                sheet1.Range[xlsRow, 1].Text = FactoryAddress;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 1].CellStyle.Font.FontName = "Aerial Narrow";
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 26;
                sheet1.Range[xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "SALARY STATEMENT FOR THE MONTH OF " + Convert.ToDateTime(monthEndDate).ToString("MMM") + "-" + Convert.ToDateTime(monthStartDate).Year.ToString() + "(TOP SHEET)"; //_payRegisterLocal + "," + ru.ChangeMonth(Convert.ToDateTime(para.FromDate).ToString("MMM"), "Bengali") + "," + yearLocal;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.FontName = "ArialNarrow";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 17;
                sheet1.Range[xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************
                #region Freeze Panes
                sheet1.UsedRange["A6"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 5;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.2;
                sheet1.PageSetup.BottomMargin = 0.7;

                sheet1.PageSetup.PrintTitleRows = "$A$1:$IV$6";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&15" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;


                xlsRow++;

                sheet1.UsedRange.AutofitColumns();
                sheet1[1, 2].ColumnWidth = 30;
                //sheet1.UsedRange.CellStyle.Font.Size = 10;

                sheet1.Name = "SalaryTopSheet" + salaryPorcId;
                #endregion          
                return workbook;
            }

            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }
        }

        public static bool hasSpecialChar(string input)
        {
            string specialChar = @"\|!#$%&/()=?»«@£§€{}.-;'<>_,";
            foreach (var item in specialChar)
            {
                if (input.Contains(item)) return true;
            }

            return false;
        }

        public IWorkbook XDynamicSalaryTopSheetExcel(string groupId, string companyId, string plantId, string monthNo, string yearNo, string salaryPorcId, string monthStartDate, string monthEndDate, string SalaryTopSheetCategory)
        {

            #region Variable

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = new ReportUtility();
            clsReport objRpt = new clsReport();
            #endregion Variable

            try
            {
                var baseCurrency = "";

                var dtSalaryTopSheet = GetDynamicSalaryTopSheetDataSql(groupId, companyId, plantId, monthNo, yearNo, salaryPorcId, monthStartDate, monthEndDate, SalaryTopSheetCategory);

                var Currency = GetPlantCurrency();
                if (Currency.Rows.Count > 0)
                {
                    baseCurrency = Currency.Rows[0]["Id"].ToString();
                }

                #region Variable             

                var FactoryName = "";
                var CmpName = "";


                #endregion Variable



                DateTime dtFrmDt = DateTime.Now;
                DateTime dtEndDate = DateTime.Now;

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;
                sheet1.IsDisplayZeros = false;

                var colSl = 0;
                var colSectionName = 0;
                var colNumberofEmployee = 0;
                var colTotalSalaryGross = 0;
                var colTotalOTHours = 0;
                var colTotalOTSalaryAmount = 0;
                var colTotalNetAmount = 0;
                var colDepartment = 0;
                var colEmployeeCatagory = 0;


                #region------------------Column Header------------------
                xlsRow = 4;
                xlsCol = 1;


                List<string> dlst = new List<string>();
                List<string> elst = new List<string>();
                foreach (DataColumn item in dtSalaryTopSheet.Columns)
                {

                    string stringAfterChar = "";
                    string stringBeforeChar = "";
                    var colvalue = item.ColumnName;
                    var fv = hasSpecialChar(colvalue);
                    if (fv)
                    {
                        stringAfterChar = colvalue.Substring(colvalue.IndexOf(",") + 1);

                        stringBeforeChar = colvalue.Substring(0, colvalue.IndexOf(","));
                        if (stringAfterChar == "E")
                        {
                            elst.Add(stringBeforeChar);
                        }
                        else
                        {
                            dlst.Add(stringBeforeChar);
                        }
                    }
                    else
                    {
                        sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        SetCellHeaderValue(colvalue, sheet1, xlsRow, ref xlsCol, out colNumberofEmployee);
                    }
                }

                foreach (var eitem in elst)
                {
                    sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    SetCellHeaderValue(eitem, sheet1, xlsRow, ref xlsCol, out colNumberofEmployee);
                }
                foreach (var ditem in dlst)
                {
                    sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    SetCellHeaderValue(ditem, sheet1, xlsRow, ref xlsCol, out colNumberofEmployee);
                }

                xlsRow++;
                for (int i = 0; i < dtSalaryTopSheet.Rows.Count; i++)
                {
                    string stringBeChar = "";
                    xlsCol = 1;
                    foreach (DataColumn item in dtSalaryTopSheet.Columns)
                    {
                        string clmn = item.ColumnName;
                        var fv = hasSpecialChar(clmn);
                        if (fv)
                        {
                            stringBeChar = clmn.Substring(0, clmn.IndexOf(","));
                            if (elst.Contains(stringBeChar))
                            {
                                SetCellValue(dtSalaryTopSheet.Rows[i][clmn].ToString(), sheet1, xlsRow, ref xlsCol, out colNumberofEmployee);
                            }
                            if (dlst.Contains(stringBeChar))
                            {
                                SetCellValue((Convert.ToDouble(dtSalaryTopSheet.Rows[i][clmn]) * (-1)).ToString(), sheet1, xlsRow, ref xlsCol, out colNumberofEmployee);
                            }

                        }
                        else
                        {
                            SetCellValue(dtSalaryTopSheet.Rows[i][clmn].ToString(), sheet1, xlsRow, ref xlsCol, out colNumberofEmployee);
                        }

                    }
                    xlsRow++;
                }


                endXlsCol = --xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);

                #endregion------------------Column Header------------------

                int RowIndex = xlsRow++;
                xlsRow++;
                var SL = 0;

                var totalValueForWord = 0.00;



                #region ******************Report Header******************

                DataSet dsCmp;
                objRpt.SelectedPlantWiseCompany(plantId, "", out dsCmp);
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
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddress"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 1].Text = FactoryName;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 22;
                sheet1.Range[xlsRow, 1].CellStyle.Font.FontName = "Aerial Narrow";

                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 26;

                xlsRow++;
                sheet1.Range[xlsRow, 1].Text = FactoryAddress;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 1].CellStyle.Font.FontName = "Aerial Narrow";
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 26;
                sheet1.Range[xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "SALARY STATEMENT FOR THE MONTH OF " + Convert.ToDateTime(monthEndDate).ToString("MMM") + "-" + Convert.ToDateTime(monthStartDate).Year.ToString() + "(TOP SHEET)"; //_payRegisterLocal + "," + ru.ChangeMonth(Convert.ToDateTime(para.FromDate).ToString("MMM"), "Bengali") + "," + yearLocal;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.FontName = "ArialNarrow";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 17;
                sheet1.Range[xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************
                #region Freeze Panes
                sheet1.UsedRange["A6"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 5;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.2;
                sheet1.PageSetup.BottomMargin = 0.7;

                sheet1.PageSetup.PrintTitleRows = "$A$1:$IV$6";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&15" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;


                xlsRow++;

                sheet1.UsedRange.AutofitColumns();
                sheet1[1, 2].ColumnWidth = 30;
                //sheet1.UsedRange.CellStyle.Font.Size = 10;

                sheet1.Name = "SalaryTopSheet" + salaryPorcId;
                #endregion          
                return workbook;
            }

            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }
        }
        public IWorkbook DynamicSalaryTopSheetExcel(string groupId, string companyId, string plantId, string monthNo, string yearNo, string salaryPorcId, string monthStartDate, string monthEndDate, string SalaryTopSheetCategory)
        {

            #region Variable

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = new ReportUtility();
            clsReport objRpt = new clsReport();
            #endregion Variable

            try
            {
                var baseCurrency = "";

                var dtSalaryTopSheet = GetDynamicSalaryTopSheetDataSql(groupId, companyId, plantId, monthNo, yearNo, salaryPorcId, monthStartDate, monthEndDate, SalaryTopSheetCategory);

                var Currency = GetPlantCurrency();
                if (Currency.Rows.Count > 0)
                {
                    baseCurrency = Currency.Rows[0]["Id"].ToString();
                }

                #region Variable             

                var FactoryName = "";
                var CmpName = "";


                #endregion Variable



                DateTime dtFrmDt = DateTime.Now;
                DateTime dtEndDate = DateTime.Now;

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;
                sheet1.IsDisplayZeros = false;

                int ROW = 6;
                int COL = 1;

                sheet1[ROW, COL].Text = "Entity";
                int colEntity = COL; COL++;
                sheet1[ROW, COL].Text = "Department";
                int colDepartment = COL; COL++;
                sheet1[ROW, COL].Text = "Section";
                int colSection = COL; COL++;

                Dictionary<string, int> dicColIndex = new Dictionary<string, int>();
                DataView dvCol = new DataView(dtSalaryTopSheet.DefaultView.ToTable(true, "SalaryHead", "SalaryHeadId", "HeadType"));
                dvCol.RowFilter = "HeadType='E'";
                int mergeColStartEarning = COL;
                for (int i = 0; i < dvCol.Count; i++)
                {
                    sheet1[ROW, COL].Text = dvCol[i]["SalaryHead"].ToString();
                    dicColIndex.Add(dvCol[i]["SalaryHeadId"].ToString(), COL);
                    COL++;
                }
                sheet1[ROW - 1, mergeColStartEarning].Text = "Earning";
                sheet1.Range[ROW - 1, mergeColStartEarning, ROW - 1, COL].Merge();



                dvCol.RowFilter = "HeadType='D'";
                int mergeColStartDeduction = COL;
                for (int i = 0; i < dvCol.Count; i++)
                {
                    sheet1[ROW, COL].Text = dvCol[i]["SalaryHead"].ToString();
                    dicColIndex.Add(dvCol[i]["SalaryHeadId"].ToString(), COL);
                    COL++;
                }
                sheet1[ROW - 1, mergeColStartDeduction].Text = "Deduction";
                sheet1.Range[ROW - 1, mergeColStartDeduction, ROW - 1, COL].Merge();

                #region------------------Column Header------------------
                xlsRow = 4;
                int endCol = COL;

                sheet1.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;
                #endregion------------------Column Header------------------

                int DataStartRow = ROW;

                for (int i = 0; i < dtSalaryTopSheet.Rows.Count; i++)
                {

                    sheet1[ROW, colDepartment].Text = dtSalaryTopSheet.Rows[i]["Department"].ToString();
                    sheet1[ROW, colEntity].Text = dtSalaryTopSheet.Rows[i]["Entity"].ToString();
                    sheet1[ROW, colSection].Text = dtSalaryTopSheet.Rows[i]["Section"].ToString();


                    sheet1[ROW, dicColIndex[dtSalaryTopSheet.Rows[i]["SalaryHeadId"].ToString()]].Number = clsStaticInfo.dbl(dtSalaryTopSheet.Rows[i]["Amount"].ToString());


                    sheet1.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }

                #region ******************Report Header******************

                DataSet dsCmp;
                objRpt.SelectedPlantWiseCompany(plantId, "", out dsCmp);
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
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddress"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 1].Text = FactoryName;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 22;
                sheet1.Range[xlsRow, 1].CellStyle.Font.FontName = "Aerial Narrow";

                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                sheet1.Range[xlsRow, 1, xlsRow, endCol - 1].Merge();
                sheet1.Range[xlsRow, 1, xlsRow, endCol - 1].RowHeight = 26;

                xlsRow++;
                sheet1.Range[xlsRow, 1].Text = FactoryAddress;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 1].CellStyle.Font.FontName = "Aerial Narrow";
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                sheet1.Range[xlsRow, 1, xlsRow, endCol - 1].Merge();
                sheet1.Range[xlsRow, 1, xlsRow, endCol - 1].RowHeight = 26;
                sheet1.Range[xlsRow, endCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "SALARY STATEMENT FOR THE MONTH OF " + Convert.ToDateTime(monthEndDate).ToString("MMM") + "-" + Convert.ToDateTime(monthStartDate).Year.ToString() + "(TOP SHEET)"; //_payRegisterLocal + "," + ru.ChangeMonth(Convert.ToDateTime(para.FromDate).ToString("MMM"), "Bengali") + "," + yearLocal;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.FontName = "ArialNarrow";
                sheet1.Range[xlsRow, 1, xlsRow, endCol - 1].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                sheet1.Range[xlsRow, 1, xlsRow, endCol - 1].RowHeight = 17;
                sheet1.Range[xlsRow, endCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************
                #region Freeze Panes
                sheet1.UsedRange["A6"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 5;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.2;
                sheet1.PageSetup.BottomMargin = 0.7;

                sheet1.PageSetup.PrintTitleRows = "$A$1:$IV$6";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&15" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;


                xlsRow++;

                sheet1.UsedRange.AutofitColumns();
                sheet1[1, 2].ColumnWidth = 30;
                //sheet1.UsedRange.CellStyle.Font.Size = 10;

                sheet1.Name = "SalaryTopSheet" + salaryPorcId;
                #endregion          
                return workbook;
            }

            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }
        }

        public DataTable GetSalaryTopSheetDataSql(string groupId, string companyId, string plantId, string monthNo, string yearNo, string salaryPorcId, string monthStartDate, string monthEndDate, string SalaryTopSheetCategory)
        {
            var Column = "";
            var GroupBy = "";
            var OrderBy = "";
            if (SalaryTopSheetCategory == "PayrollGroup")
            {
                Column = @"PGM.PayrollGroupId
			,ISNULL(PG.UserName,'No Group') PayrollGroup
            ,PG.Sequence";
            }
            if (SalaryTopSheetCategory == "DepartmentEmployeeCategory")
            {
                Column = @"DP.UserName Department,EC.UserName EmployeeCatagory,DP.Sequence";
            }
            if (SalaryTopSheetCategory == "PayrollGroup")
            {
                GroupBy = @"PGM.PayrollGroupId
			,PG.UserName
          ,PG.Sequence";
            }
            if (SalaryTopSheetCategory == "DepartmentEmployeeCategory")
            {
                GroupBy = @"DP.UserName 
	    ,EC.UserName
		,DP.Sequence";
            }
            if (SalaryTopSheetCategory == "PayrollGroup")
            {
                OrderBy = @"PG.Sequence";
            }
            if (SalaryTopSheetCategory == "DepartmentEmployeeCategory")
            {
                OrderBy = @"DP.Sequence";
            }


            if (!string.IsNullOrEmpty(salaryPorcId) && salaryPorcId != "undefined" && salaryPorcId != "null")
            {
                salaryPorcId = "SystemId IN ('" + salaryPorcId + @"')";
            }
            else
            {
                salaryPorcId = @"SystemId IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + monthStartDate + "') AND YearNo = Year('" + monthEndDate + "')  and IsCompleteMonth = 1)";
            }

            try
            {
                var strSql = @"
		SELECT sum(GROSS.GrossDisbusmentAmount) AS GrossDisbusmentAmount
			,sum(CTC.GrossDisbusmentAmount) AS CTCDisbusmentAmount
			,sum(DED.GrossDisbusmentAmount) AS Deductioin
			,sum(CTC.GrossDisbusmentAmount) + sum(DED.GrossDisbusmentAmount) AS NetPayable
			,sum(isnull(SPAD.TotalOTHr, 0) / 60) AS TotalOThr
			,sum(ot.GrossDisbusmentAmount) AS TotalOTAmount
			,count(ei.SystemId)  NoOfEmployees
			--ei.SystemId,ei.EmployeeCode,
			," + Column + @"
		FROM EmployeeInformation EI
		-------------------------------------GROSS------------------------
		LEFT OUTER JOIN (
			SELECT spc.EmpInfoSystemID
				,SUM(spc.DisbusmentAmount) GrossDisbusmentAmount
			FROM SalaryProcChild SPC
			INNER JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID
			INNER JOIN SalaryHead SH ON SH.SalaryHeadID = SPC.SalaryHeadID
			WHERE SPM.MonthNo = '" + monthNo + @"'
				AND SPM.YearNo = '" + yearNo + @"'

                AND SPM." + salaryPorcId + @"

                AND SH.IsGrossComponent = 1
				AND sh.IsCTCComponent = 1
			GROUP BY spc.EmpInfoSystemID
			) AS GROSS ON EI.SystemId = GROSS.EmpInfoSystemID
		-------------------------------------CTC------------------------
		LEFT OUTER JOIN (
			SELECT spc.EmpInfoSystemID
				,SUM(spc.DisbusmentAmount) GrossDisbusmentAmount
			FROM SalaryProcChild SPC
			INNER JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID
			INNER JOIN SalaryHead SH ON SH.SalaryHeadID = SPC.SalaryHeadID
			WHERE SPM.MonthNo = '" + monthNo + @"'
				AND SPM.YearNo = '" + yearNo + @"'
				AND SPM." + salaryPorcId + @"
				AND sh.IsCTCComponent = 1
			GROUP BY spc.EmpInfoSystemID
			) AS CTC ON EI.SystemId = CTC.EmpInfoSystemID
		-------------------------------------DEDUC------------------------
		LEFT OUTER JOIN (
			SELECT spc.EmpInfoSystemID
				,SUM(spc.DisbusmentAmount) GrossDisbusmentAmount
			FROM SalaryProcChild SPC
			INNER JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID
			INNER JOIN SalaryHead SH ON SH.SalaryHeadID = SPC.SalaryHeadID
			WHERE SPM.MonthNo = '" + monthNo + @"'
				AND SPM.YearNo = '" + yearNo + @"'
				AND SPM." + salaryPorcId + @"
				AND sh.HeadType = 'D'
				AND sh.SalaryHead <> 'Total Deduction'
			GROUP BY spc.EmpInfoSystemID
			) AS DED ON EI.SystemId = DED.EmpInfoSystemID
		-------------------------------------OT------------------------------------------
		LEFT OUTER JOIN (
			SELECT spc.EmpInfoSystemID
				,SUM(spc.DisbusmentAmount) GrossDisbusmentAmount
			FROM SalaryProcChild SPC
			INNER JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID
			INNER JOIN SalaryHead SH ON SH.SalaryHeadID = SPC.SalaryHeadID
			WHERE SPM.MonthNo ='" + monthNo + @"'
				AND SPM.YearNo = '" + yearNo + @"'
				AND SPM." + salaryPorcId + @"
				AND sh.HeadCategory = 'OverTime'
			GROUP BY spc.EmpInfoSystemID
			) AS OT ON EI.SystemId = OT.EmpInfoSystemID
		LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.EmployeeId = EI.SystemId

left join ORG.Department DP ON DP.Id=EI.DepartmentId
		LEFT JOIN
	--hkp.EmployeeCategory EC ON E.EmployeeCategorySystemID = EC.Id
	(
	SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
	LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
	)EC ON EC.DesignationId=EI.GivenDesignationId
       
        LEFT OUTER JOIN (select * from SalaryProceAttdnData where IsOTEntitled=1) SPAD ON SPAD.EmpSystemID = EI.SystemId and SPAD.MonthNo = " + monthNo + @" and SPAD.YearNo = " + yearNo + @"
		LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.Id = PGM.PayrollGroupId

		WHERE EI.GroupID = '" + groupId + @"'
			AND EI.CompanyId = '" + companyId + @"'
			AND ei.SystemId IN (
				SELECT c.EmpInfoSystemID
				FROM SalaryProcChild C
				INNER JOIN SalaryProcMaster M ON m.SystemID = c.SlrProcMstSystemID
				WHERE M." + salaryPorcId + @"
				)
		GROUP BY 
		" + GroupBy + @"

		ORDER BY
		" + OrderBy + @"";

                return _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public DataTable GetDynamicSalaryTopSheetDataSql(string groupId, string companyId, string plantId, string monthNo, string yearNo, string salaryPorcId, string monthStartDate, string monthEndDate, string SalaryTopSheetCategory)
        {
            var Column = "";
            var GroupBy = "";
            var OrderBy = "";
            if (SalaryTopSheetCategory == "PayrollGroup")
            {
                Column = @"PGM.PayrollGroupId
			,ISNULL(PG.UserName,'No Group') PayrollGroup
            ,PG.Sequence";
            }
            if (SalaryTopSheetCategory == "DepartmentEmployeeCategory")
            {
                Column = @"EN.UserName Entity,DP.UserName Department,EC.UserName EmployeeCatagory,DP.Sequence";
            }
            if (SalaryTopSheetCategory == "PayrollGroup")
            {
                GroupBy = @"PGM.PayrollGroupId
			,PG.UserName
          ,PG.Sequence";
            }
            if (SalaryTopSheetCategory == "DepartmentEmployeeCategory")
            {
                GroupBy = @"EN.UserName, DP.UserName, EC.UserName, DP.Sequence";
            }
            if (SalaryTopSheetCategory == "PayrollGroup")
            {
                OrderBy = @"PG.Sequence";
            }
            if (SalaryTopSheetCategory == "DepartmentEmployeeCategory")
            {
                OrderBy = @"DP.Sequence";
            }
            try
            {
                var strSql = @"select spc.SalaryHeadID,
                                e.UserName AS Entity,DEPT.UserName AS Department,S.username AS Section,ss.UserName AS SubSection,
                                sh.SalaryHead,SUM(abs(isnull(spc.DisbusmentAmount,0))) AS Amount,sh.HeadType
                                  FROM SalaryProcMaster AS spm
                                INNER JOIN SalaryProcChild AS spc ON spc.SlrProcMstSystemID=spm.SystemID
                                INNER JOIN SalaryHead AS sh ON sh.SalaryHeadID=spc.SalaryHeadID
                                INNER JOIN EmployeeInformation EMP ON emp.SystemId=spc.EmpInfoSystemID
                                LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                WHERE spm.SystemID='" + salaryPorcId + @"' and spc.DisbusmentAmount <> 0
                                GROUP BY spc.SalaryHeadID,
                                e.UserName,DEPT.UserName,S.username,ss.UserName,
                                sh.SalaryHead,sh.HeadType";

                return _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public DataTable GetPlantCurrency()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var strSql = @"select c.Id,c.Code from org.Company p
					inner join scs.Currency c on c.Id=p.BaseCurrencyId

					where p.Id='" + identity.CompanyId + "'";

                return _sqlRepository.GetDataTable(strSql);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 10;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 18;
            ColIndex = xlsCol;
            xlsCol += 1;
        }
        private void SetCellHeaderValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 11;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 18;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Yellow;

            ColIndex = xlsCol;
            xlsCol += 1;
        }

        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 24;

            ColIndex = xlsCol;
            xlsCol += 1;
        }
        private void SetHeadText(IWorksheet sheet, int xlsRow, int xlsCol, string text)
        {
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
        }
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

        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, double Value)
        {
            string NumberFormatString = "#,##0;(#,##0)";
            //if (string.IsNullOrEmpty(Value.to) == false)
            //{
            // if (dvSlrProc[i]["SalaryHeadID"].ToString() == "SHD2017-1" & string.IsNullOrEmpty(dvSlrProc[i]["SalaryHeadID"].ToString()) == false)
            // ColBasSlr += Convert.ToDecimal(dvSlrProc[i]["DisbusmentAmount"].ToString());

            sheet.Range[xlsRow, xlsCol].Number = Value;
            sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //}
        }
        //Create Dynamic head Function 
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
        [HttpGet, Authorize]
        public ActionResult XlsSalaryTopSheet(string year, string month, string salaryProcessId)
        {
            #region Variable

            clsReport objRpt = null;

            DataSet dsSlrProc, dsBonus = null;
            DataView dvSlrProc = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            clsStaticInfo objs = null;

            ReportUtility ru = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;


            excelEngine = new ExcelEngine();
            application = excelEngine.Excel;

            workbook = application.Workbooks.Create(1);
            sheet1 = workbook.Worksheets[0];
            sheet1.IsGridLinesVisible = true;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            string NumberFormatString = "#,##0;(#,##0)";
            //string USDNumberFormatString = "#,##0.00;(#,##0.00)";
            string FactoryName = "";
            string CmpName = "";

            #endregion Variable

            try
            {
                objRpt = new clsReport();
                objs = new OTSBD.clsStaticInfo();
                ru = new ReportUtility();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                #region Variable



                PayRegisterParamList para = new PayRegisterParamList();
                para.Month = month;
                para.Year = year;

                var daysInMonth = 0;
                daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(para.Year), Convert.ToInt32(para.Month));//Number of Days in a month

                para.CompanyGroupId = identity.CompanyGroupId;
                para.CompanyId = identity.CompanyId;
                para.PlantId = identity.PlantId;


                //para.EmployeeId = lblEmpSystemID.Text;
                para.FromDate = "01-" + bplib.clsWebLib.GetMonthName(month) + "-" + year;
                para.ToDate = daysInMonth + "-" + bplib.clsWebLib.GetMonthName(para.Month) + "-" + para.Year;
                para.SalaryProcessId = salaryProcessId;


                #endregion Variable

                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                #region DataSet

                objRpt.GetSalaryInfoSlrProcIDWiseForTopSheet(para, out dsSlrProc);

                dvSlrProc = new DataView();
                dvSlrProc.Table = dsSlrProc.Tables[0];

                DataView dvEmp = new DataView();
                dvEmp.Table = dsSlrProc.Tables[0];
                DataTable dtEmployees = dvEmp.ToTable(true, "EntityName", "DepartmentName", "SubSectionName", "IntegerInDisb", "DecimalNo");
                if (dtEmployees.Rows.Count == 0)
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }
                //get

                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet

                if (dtEmployees.Rows.Count > 0)
                {


                    #region------------------Column Header------------------
                    xlsRow = 5;
                    xlsCol = 1;

                    int ColSr = 0;
                    int ColIDNo = 0;
                    int ColName = 0;
                    int ColDOJ = 0;
                    int ColDOS = 0;
                    int ColDG = 0;
                    int ColDGG = 0;
                    int ColGVDG = 0;
                    int ColGVDGG = 0;
                    int ColStCt = 0;
                    int ColUnit = 0;
                    int ColDvN = 0;
                    int ColDpN = 0;
                    int ColSec = 0;
                    int ColSecS = 0;
                    int colTotalEmp = 0;
                    bool isFirst = true;

                    Dictionary<string, double> dictSalaryStruct = null;
                    Dictionary<string, double> dictSalaryProcess = new Dictionary<string, double>();

                    int ColFirstValue = xlsCol;
                    int ColSecondValue = xlsCol;
                    int ColThirdValue = xlsCol;

                    xlsRow += 1;

                    SetHeadText("Entity", sheet1, xlsRow, ref xlsCol, out ColFirstValue,17);
                    SetHeadText("Department", sheet1, xlsRow, ref xlsCol, out ColSecondValue,23);
                    SetHeadText("Subsection", sheet1, xlsRow, ref xlsCol, out ColThirdValue,23);
                    //SetHeadText("No Of. Emploees", sheet1, xlsRow, ref xlsCol, out colTotalEmp);


                    var endGenericCol = xlsCol;

                    var totalDictSalaryStruct = new Dictionary<string, double>();
                    var totalDictSalaryProcess = new Dictionary<string, double>();

                    //-------------------------
                    DataView dvSalaryHead = new DataView(dsSlrProc.Tables[0]);
                    dvSalaryHead.Sort = "HeadType desc,Sequence";
                    DataTable dtSalaryHead = dvSalaryHead.ToTable(true, "SalaryHeadID", "SalaryHead", "HeadType", "Sequence", "HeadCategory", "IsCTCComponent", "IsGrossComponent", "IntegerInDisb", "DecimalNo");

                    #region VPF n Bonus                    
                    #endregion

                    int _count_earning_head = 0;
                    int _count_deducting_head = 0;
                    int _total_head_count = 0;
                    int _count_earning_ctchead = 0;
                    List<SalaryHeadSequence> list = null;
                    CreateDynamicSHeadTopSheet(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColThirdValue, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out list);

                    // xlsCol--;
                    //Header Col


                    int ds = endGenericCol + _count_earning_head + _count_earning_ctchead;

                    if (_count_earning_head > 0)
                    {
                        sheet1.Range[xlsRow - 1, endGenericCol].Text = "Earning";
                        sheet1.Range[xlsRow - 1, endGenericCol, xlsRow - 1, ds - 1].Merge();
                    }

                    if (_count_deducting_head > 0)
                    {
                        sheet1.Range[xlsRow - 1, ds].Text = "Deduction";
                        sheet1.Range[xlsRow - 1, ds, xlsRow - 1, ds + _count_deducting_head - 1].Merge();
                    }

                    int np = 0;
                    if (list.Count > 0)
                    {
                        xlsCol++;
                        np = ds + _count_deducting_head;
                        sheet1.Range[xlsRow, np].Text = "Total Deduction";
                        sheet1.Range[xlsRow, np].ColumnWidth = 14;
                        //sheet1.Range[xlsRow, np -1, xlsRow, np].Merge();

                        sheet1.Range[xlsRow, np + 1].Text = "Net Payable";
                        sheet1.Range[xlsRow, np + 1].ColumnWidth = 14;
                        //sheet1.Range[xlsRow, np, xlsRow, np].Merge();
                    }
                    xlsCol++;
                    sheet1.Range[xlsRow, xlsCol].Text = "Signature";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 26;
                    int ColSigna = xlsCol;
                    sheet1.Range[xlsRow, ColSigna, xlsRow, ColSigna].Merge();

                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    endXlsCol = xlsCol;
                    #endregion------------------Column Header------------------

                    int RowIndex = xlsRow + 3;

                    #region ******************Report Header******************
                    xlsRow = 1;
                    xlsCol = 1;
                    string FactoryAddress = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        //CmpName = dsCmp.Tables[0].Rows[0]["UserName"].ToString();
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 30;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        //FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                        FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Salary Top Sheet";
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    var strRptDateRange = "";
                    strRptDateRange = "For The Month Of " + bplib.clsWebLib.GetMonthName(month) + ", " + year;
                    sheet1.Range[xlsRow, xlsCol].Text = strRptDateRange;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region ----------------------Data-----------------------

                    int SrNo = 0;
                    string x = "";
                    string y = "";
                    string z = "";
                    decimal ColGrsSlr = 0;
                    decimal ColCTCSlr = 0;
                    ReportUtility oRU = new ReportUtility();

                    xlsRow = RowIndex;

                    string _grp1 = string.Empty;
                    string _grp2 = string.Empty;
                    string _grp3 = string.Empty;

                    //#endregion

                    xlsRow--;
                    xlsRow--;
                    var catFRow = xlsRow;
                    var catGrp2FRow = xlsRow;
                    var catGrp3FRow = xlsRow;
                    ArrayList rowList = new ArrayList();
                    var lastGenColValue = string.Empty;
                    for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                    {
                        //xlsRow++;
                        #region empinfo col Data

                        var catLRow = xlsRow;
                        if (_grp1 != dtEmployees.Rows[i]["EntityName"].ToString() && string.IsNullOrEmpty(dtEmployees.Rows[i]["EntityName"].ToString()) == false)
                        {
                            _grp1 = dtEmployees.Rows[i]["EntityName"].ToString();

                            #region Subtotal
                            if (catFRow < xlsRow)
                            {
                                lastGenColValue = _grp1;
                                rowList.Add(xlsRow);
                                SetHeadText(sheet1, xlsRow, ColFirstValue, " Subtotal:");
                                sheet1.Range[xlsRow, 1, xlsRow, 3].Merge();// = Convert.ToDouble(item.Value);

                                foreach (var item in dictSalaryProcess)//Loop Last Summation in SalaryPorcessss
                                {
                                    try
                                    {
                                        sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Number = Convert.ToDouble(item.Value);
                                        //sheet1.Range[xlsR 1, Convert.ToInt32(item.Ke 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
                                        sheet1.Range[xlsRow, Convert.ToInt32(item.Key), xlsRow, Convert.ToInt32(item.Key)].BorderAround(ExcelLineStyle.Thin);
                                        //sheet1.Range[xlsRow, Convert.ToInt32(item.Key), xlsRow + 1, Convert.ToInt32(item.Key)].Merge();
                                        sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].RowHeight = 40;
                                        sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Size = 28;
                                        sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Bold = true;
                                        sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                        sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                    }
                                    catch (Exception exe)
                                    {
                                        throw exe;
                                    }
                                }//Loop End Last Summation in SalaryPorcess



                                var grossIndexSubStructOSideST = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.XLColIndex).FirstOrDefault();
                                var ctcIndexSubStructOSideST = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.XLColIndex).FirstOrDefault();

                                var grossSubStructOsideST = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.SalaryHead).FirstOrDefault();
                                var ctcSubStructOsideST = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.SalaryHead).FirstOrDefault();

                                var dedFormulaStructOSideST = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.SalaryHead).FirstOrDefault();

                                var grossAddSubOSideST = oRU.SetFormula((grossIndexSubStructOSideST).ToString(), xlsRow);
                                var ctcAddSubOSideST = oRU.SetFormula((ctcIndexSubStructOSideST).ToString(), xlsRow);
                                var dedAddStructOSideST = oRU.SetFormula(dedFormulaStructOSideST, xlsRow);

                                sheet1.Range[xlsRow, grossIndexSubStructOSideST].Formula = "=" + oRU.SetFormula(grossSubStructOsideST, xlsRow);
                                sheet1.Range[xlsRow, grossIndexSubStructOSideST].BorderAround(ExcelLineStyle.Thin);
                                //sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage); ;

                                sheet1.Range[xlsRow, grossIndexSubStructOSideST].CellStyle.Font.Size = 28;
                                sheet1.Range[xlsRow, grossIndexSubStructOSideST].CellStyle.Font.Bold = true;
                                //sheet1.Range[xlsRow + 1, grossIndexSubStructOSide, xlsRow + 1, grossIndexSubStructOSide].Merge();

                                sheet1.Range[xlsRow, ctcIndexSubStructOSideST].Formula = "=" + oRU.SetFormula(ctcSubStructOsideST, xlsRow);
                                //sheet1.Range[xlsR 1, ctcIndexSubStructOSi 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage); ;
                                sheet1.Range[xlsRow, ctcIndexSubStructOSideST].BorderAround(ExcelLineStyle.Thin);


                                sheet1.Range[xlsRow, ctcIndexSubStructOSideST].CellStyle.Font.Size = 28;
                                sheet1.Range[xlsRow, ctcIndexSubStructOSideST].CellStyle.Font.Bold = true;
                                //sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide, xlsRow + 1, ctcIndexSubStructOSide].Merge();


                                var dedAddSubSalStructOSideST = oRU.SetFormula(grossSubStructOsideST, xlsRow);

                                sheet1.Range[xlsRow, np].Formula = "=" + dedAddStructOSideST;//Total Deduction
                                                                                           //sheet1.Range[xlsR 1,  1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
                                sheet1.Range[xlsRow, np].CellStyle.Font.Size = 28;
                                sheet1.Range[xlsRow, np].CellStyle.Font.Bold = true;
                                sheet1.Range[xlsRow, np].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                sheet1.Range[xlsRow, np].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, np, xlsRow, np].BorderAround(ExcelLineStyle.Thin);
                                //sheet1.Range[xlsRow + 1, np - 1, xlsRow + 1, np].Merge();

                                sheet1.Range[xlsRow, np + 1].Formula = "=" + ctcAddSubOSideST + "-(" + dedAddStructOSideST + ")";//Net Payable
                                                                                                                             //sheet1.Range[xlsR 1,  + 1colNetpayable].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
                                sheet1.Range[xlsRow, np + 1].CellStyle.Font.Size = 28;
                                sheet1.Range[xlsRow, np + 1].CellStyle.Font.Bold = true;
                                sheet1.Range[xlsRow, np + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                sheet1.Range[xlsRow, np + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, np + 1, xlsRow, np + 1].BorderAround(ExcelLineStyle.Thin);

                                if (isFirst == false)
                                {
                                    dictSalaryProcess = new Dictionary<string, double>();
                                }
                                xlsRow++;
                            }
                            #endregion

                            sheet1.Range[xlsRow, ColFirstValue].Text = _grp1;
                            sheet1.Range[xlsRow, ColFirstValue, xlsRow, ColFirstValue].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, ColFirstValue].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, ColFirstValue].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _grp2 = dtEmployees.Rows[i]["DepartmentName"].ToString();
                            SetCellText(sheet1, xlsRow, ColSecondValue, _grp2);
                            _grp3 = dtEmployees.Rows[i]["SubSectionName"].ToString();
                            SetCellText(sheet1, xlsRow, ColThirdValue, _grp3);

                            if (catFRow < xlsRow)
                            {
                                catFRow = xlsRow;
                                catGrp2FRow = xlsRow;
                            }
                        }

                        else if (_grp2 != dtEmployees.Rows[i]["DepartmentName"].ToString())
                        {
                            _grp2 = dtEmployees.Rows[i]["DepartmentName"].ToString();
                            //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                            sheet1.Range[xlsRow, ColSecondValue].Text = _grp2;
                            sheet1.Range[xlsRow, ColSecondValue, xlsRow, ColSecondValue].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, ColSecondValue].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, ColSecondValue].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _grp3 = dtEmployees.Rows[i]["SubSectionName"].ToString();
                            SetCellText(sheet1, xlsRow, ColThirdValue, _grp3);
                            if (catGrp2FRow < xlsRow)
                            {
                                catGrp2FRow = xlsRow;
                            }
                        }
                        else if (_grp3 != dtEmployees.Rows[i]["SubSectionName"].ToString())
                        {

                            _grp3 = dtEmployees.Rows[i]["SubSectionName"].ToString();
                            SetCellText(sheet1, xlsRow, ColThirdValue, _grp3);

                            sheet1.Range[catFRow, ColFirstValue, xlsRow, ColFirstValue].Merge();
                            sheet1.Range[catFRow, ColFirstValue, xlsRow, ColFirstValue].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[catGrp2FRow, ColSecondValue, xlsRow, ColSecondValue].Merge();
                            sheet1.Range[catGrp2FRow, ColSecondValue, xlsRow, ColSecondValue].BorderAround(ExcelLineStyle.Hair);

                        }
                        
                        if (isFirst == true)
                        {
                            isFirst = false;
                        }
                        
                        SrNo += 1;//colTotalEmp

                        #endregion
                        x = dtEmployees.Rows[i]["EntityName"].ToString().Trim();
                        y = dtEmployees.Rows[i]["DepartmentName"].ToString().Trim();
                        z = dtEmployees.Rows[i]["SubSectionName"].ToString().Trim();
                                               
                        int _total_head_count_body = 0;
                        for (int ci = 0; ci < list.Count; ci++)
                        {
                            var ob = list[ci];
                            if (ob.SalaryHead.Length > 0)
                            {
                                if (ob.SalaryHeadId.ToUpper() == "CTC" || ob.SalaryHeadId.ToUpper() == "GROSS")
                                {
                                    var formula = ob.SalaryHead;
                                    var hId = ob.SalaryHeadId;
                                    _total_head_count_body++;

                                    sheet1.Range[xlsRow, ob.XLColIndex].Formula = "=" + oRU.SetFormula(formula, xlsRow);
                                    sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = oRU.NumberFormatDecimalTwo();
                                    sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                    sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }//ctc , gross
                                else
                                {
                                    var hId = ob.SalaryHeadId;
                                    _total_head_count_body++;

                                    DataView dvBody = new DataView(dsSlrProc.Tables[0]);
                                    dvBody.RowFilter = "SalaryHeadID='" + hId + "'and EntityName = '" + x+ "' and DepartmentName = '" + y+"' and SubSectionName='" + z + "'";

                                    if (dvBody.Count > 0)
                                    {
                                        if (ob.Deduction == "Deduction")
                                        {
                                            sheet1.Range[xlsRow, ob.XLColIndex].Number = Convert.ToDouble(dvBody[0]["DisbusmentAmount"].ToString()) * (-1);
                                            //sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = oRU.GetDynamicDecimalPlace();

                                            getTotalAmount(ob.XLColIndex.ToString(), Convert.ToDouble(dvBody[0]["DisbusmentAmount"].ToString()) * (-1), ref totalDictSalaryProcess);//dictSalaryProcess
                                            getTotalAmount(ob.XLColIndex.ToString(), Convert.ToDouble(dvBody[0]["DisbusmentAmount"].ToString()) * (-1), ref dictSalaryProcess);//dictSalaryProcess

                                        }
                                        else
                                        {
                                            sheet1.Range[xlsRow, ob.XLColIndex].Number = Convert.ToDouble(dvBody[0]["DisbusmentAmount"].ToString());
                                            getTotalAmount(ob.XLColIndex.ToString(), Convert.ToDouble(dvBody[0]["DisbusmentAmount"].ToString()), ref totalDictSalaryProcess);
                                            getTotalAmount(ob.XLColIndex.ToString(), Convert.ToDouble(dvBody[0]["DisbusmentAmount"].ToString()), ref dictSalaryProcess);
                                        }
                                        sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = oRU.NumberFormatDecimalTwo();
                                        sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                        sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }//row found
                                }
                            }//
                        }//for dtSalaryHead

                        var grossIndex = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.XLColIndex).FirstOrDefault();
                        var dedIndex = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.XLColIndex).FirstOrDefault();
                        var dedFormula = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.SalaryHead).FirstOrDefault();

                        var grossAdd = oRU.SetFormula(grossIndex.ToString(), xlsRow);
                        var dedAdd = oRU.SetFormula(dedFormula, xlsRow);

                        sheet1.Range[xlsRow, np ].Formula = "=" + dedAdd;
                        sheet1.Range[xlsRow, np ].NumberFormat = oRU.NumberFormatDecimalTwo();
                        sheet1.Range[xlsRow, np ].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, np ].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, np + 1].Formula = "=" + grossAdd + "-(" + dedAdd + ")";
                        sheet1.Range[xlsRow, np + 1].NumberFormat = oRU.NumberFormatDecimalTwo();
                        sheet1.Range[xlsRow, np + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, np + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsRow++;
                      
                    }//for emp count
                    #region SubTotal
                    SetHeadText(sheet1, xlsRow, ColFirstValue, " Subtotal:");
                    sheet1.Range[xlsRow, 1, xlsRow, 3].Merge();
                    foreach (var item in dictSalaryProcess)//Loop Last Summation in SalaryPorcessss
                    {
                        try
                        {
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Number = Convert.ToDouble(item.Value);
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key), xlsRow, Convert.ToInt32(item.Key)].BorderAround(ExcelLineStyle.Thin);
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].RowHeight = 40;
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Size = 28;
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }
                        catch (Exception exe)
                        {
                            throw exe;
                        }
                    }//Loop End Last Summation in SalaryPorcess
                    var grossIndexSubStructOSideSTL = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.XLColIndex).FirstOrDefault();
                    var ctcIndexSubStructOSideSTL = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.XLColIndex).FirstOrDefault();

                    var grossSubStructOsideSTL = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.SalaryHead).FirstOrDefault();
                    var ctcSubStructOsideSTL = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.SalaryHead).FirstOrDefault();

                    var dedFormulaStructOSideSTL = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.SalaryHead).FirstOrDefault();

                    var grossAddSubOSideSTL = oRU.SetFormula((grossIndexSubStructOSideSTL).ToString(), xlsRow);
                    var ctcAddSubOSideSTL = oRU.SetFormula((ctcIndexSubStructOSideSTL).ToString(), xlsRow);
                    var dedAddStructOSideSTL = oRU.SetFormula(dedFormulaStructOSideSTL, xlsRow);

                    sheet1.Range[xlsRow, grossIndexSubStructOSideSTL].Formula = "=" + oRU.SetFormula(grossSubStructOsideSTL, xlsRow);
                    sheet1.Range[xlsRow, grossIndexSubStructOSideSTL].BorderAround(ExcelLineStyle.Thin);

                    sheet1.Range[xlsRow, grossIndexSubStructOSideSTL].CellStyle.Font.Size = 28;
                    sheet1.Range[xlsRow, grossIndexSubStructOSideSTL].CellStyle.Font.Bold = true;

                    sheet1.Range[xlsRow, ctcIndexSubStructOSideSTL].Formula = "=" + oRU.SetFormula(ctcSubStructOsideSTL, xlsRow);
                    sheet1.Range[xlsRow, ctcIndexSubStructOSideSTL].BorderAround(ExcelLineStyle.Thin);


                    sheet1.Range[xlsRow, ctcIndexSubStructOSideSTL].CellStyle.Font.Size = 28;
                    sheet1.Range[xlsRow, ctcIndexSubStructOSideSTL].CellStyle.Font.Bold = true;

                    var dedAddSubSalStructOSideSTL = oRU.SetFormula(grossSubStructOsideSTL, xlsRow);

                    sheet1.Range[xlsRow, np].Formula = "=" + dedAddStructOSideSTL;//Total Deduction
                    sheet1.Range[xlsRow, np].CellStyle.Font.Size = 28;
                    sheet1.Range[xlsRow, np].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, np].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, np].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, np, xlsRow, np].BorderAround(ExcelLineStyle.Thin);
                    //sheet1.Range[xlsRow + 1, np - 1, xlsRow + 1, np].Merge();

                    sheet1.Range[xlsRow, np + 1].Formula = "=" + ctcAddSubOSideSTL + "-(" + dedAddStructOSideSTL + ")";//Net Payable
                                                                                                                     //sheet1.Range[xlsR 1,  + 1colNetpayable].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
                    sheet1.Range[xlsRow, np + 1].CellStyle.Font.Size = 28;
                    sheet1.Range[xlsRow, np + 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, np + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, np + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, np + 1, xlsRow, np + 1].BorderAround(ExcelLineStyle.Thin);
                    #endregion
                    xlsRow++;
                    #region Total
                    sheet1.Range[xlsRow, 1].Text = "Total";
                    //sheet1.Range[xlsRow, 1].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet1.Range[xlsRow, 1, xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, 1, xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, 3].CellStyle.Font.Bold= true;
                    sheet1.Range[xlsRow, 1, xlsRow, 3].Merge();// = true;
                    sheet1.Range[xlsRow, 1, xlsRow, 3].BorderAround(ExcelLineStyle.Thin);


                    //gross-deduction
                    foreach (var item in totalDictSalaryProcess)//Loop Last Summation in SalaryPorcessss
                    {
                        try
                        {
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Number = Convert.ToDouble(item.Value);
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key), xlsRow, Convert.ToInt32(item.Key)].BorderAround(ExcelLineStyle.Thin);
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].RowHeight = 40;
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Size = 28;
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }
                        catch (Exception exe)
                        {
                            throw exe;
                        }
                    }//Loop End Last Summation in SalaryPorcess



                    var grossIndexSubStructOSide = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.XLColIndex).FirstOrDefault();
                    var ctcIndexSubStructOSide = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.XLColIndex).FirstOrDefault();

                    var grossSubStructOside = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.SalaryHead).FirstOrDefault();
                    var ctcSubStructOside = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.SalaryHead).FirstOrDefault();

                    var dedFormulaStructOSide = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.SalaryHead).FirstOrDefault();

                    var grossAddSubOSide = oRU.SetFormula((grossIndexSubStructOSide).ToString(), xlsRow);
                    var ctcAddSubOSide = oRU.SetFormula((ctcIndexSubStructOSide).ToString(), xlsRow);
                    var dedAddStructOSide = oRU.SetFormula(dedFormulaStructOSide, xlsRow);

                    sheet1.Range[xlsRow, grossIndexSubStructOSide].Formula = "=" + oRU.SetFormula(grossSubStructOside, xlsRow);
                    sheet1.Range[xlsRow, grossIndexSubStructOSide].BorderAround(ExcelLineStyle.Thin);
                    sheet1.Range[xlsRow, grossIndexSubStructOSide].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, grossIndexSubStructOSide].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, grossIndexSubStructOSide].CellStyle.Font.Size = 28;
                    sheet1.Range[xlsRow, grossIndexSubStructOSide].CellStyle.Font.Bold = true;

                    sheet1.Range[xlsRow, ctcIndexSubStructOSide].Formula = "=" + oRU.SetFormula(ctcSubStructOside, xlsRow);
                    sheet1.Range[xlsRow, ctcIndexSubStructOSide].BorderAround(ExcelLineStyle.Thin);
                    sheet1.Range[xlsRow, ctcIndexSubStructOSide].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ctcIndexSubStructOSide].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, ctcIndexSubStructOSide].CellStyle.Font.Size = 28;
                    sheet1.Range[xlsRow, ctcIndexSubStructOSide].CellStyle.Font.Bold = true;


                    var dedAddSubSalStructOSide = oRU.SetFormula(grossSubStructOside, xlsRow);

                    sheet1.Range[xlsRow, np].Formula = "=" + dedAddStructOSide;//Total Deduction
                    sheet1.Range[xlsRow, np].CellStyle.Font.Size = 28;
                    sheet1.Range[xlsRow, np].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, np].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, np].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, np, xlsRow, np].BorderAround(ExcelLineStyle.Thin);

                    sheet1.Range[xlsRow, np + 1].Formula = "=" + ctcAddSubOSide + "-(" + dedAddStructOSide + ")";//Net Payable
                    sheet1.Range[xlsRow, np + 1].CellStyle.Font.Size = 28;
                    sheet1.Range[xlsRow, np + 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, np + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, np + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, np + 1, xlsRow, np + 1].BorderAround(ExcelLineStyle.Thin);

                    #endregion

                    #endregion ----------------------Data-----------------------

                    #region Line Setup
                    if (RowIndex >= (xlsRow - 1))
                    {
                        xlsRow = RowIndex + 2;
                    }
                    sheet1.Range[5, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[5, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[5, 1, xlsRow - 1, endXlsCol].WrapText = true;
                    #endregion

                    #region Freeze Panes
                    sheet1.UsedRange["A7"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.Range["A3"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
                    sheet1.PageSetup.PrintTitleRows = "$1:$7";
                    sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + (string)Session["USER"] + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet1.PageSetup.LeftMargin = 0.5;
                    sheet1.PageSetup.RightMargin = 0.2;
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet1.Name = "SalaryTopSheet";
                    #endregion
                    //}
                    workbook.Version = ExcelVersion.Excel2013;
                    string strFileName = "SalaryTopSheet" + bplib.clsWebLib.DateData_DBToApp(DateTime.Now.Date, bplib.clsWebLib.STD_DATE_FORMAT).ToString("dd-MMM-yyyy") + ".xlsx";

                    //sheet1.Name = "SalaryTopSheet" +month+"-"+year+identity.PlantId;
                    sheet1.Name = "TopSheet";
                    workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                    workbook.Close();
                    excelEngine.Dispose();


                }
                else
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                dsSlrProc = null;
                dvSlrProc = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }

        }//End Function



        #region Header and Cell Style
        private void SetHeadText(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = 10;
            ColIndex = xlsCol;
            xlsCol += 1;
        }

        private void SetHeadText(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            ColIndex = xlsCol;
            xlsCol += 1;
        }


        #endregion

        private void CreateDynamicSHeadTopSheet(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out List<SalaryHeadSequence> list)
        {
            try
            {
                var ru = new ReportUtility();

                list = new List<SalaryHeadSequence>();
                _total_head_count = 0;
                _count_earning_head = 0;
                _count_deducting_head = 0;
                _count_earning_ctchead = 0;
                int countGrossPostion = 0;
                string grossFormula = "";
                string deductionFormula = "";
                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region loop gross e
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
                        {
                            if (bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString()))
                            {
                                if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E")
                                {
                                    _total_head_count++;
                                    countGrossPostion++;
                                    sheet1.Range[xlsRow, ColGrs + countGrossPostion].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                                    //sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.FontName = printFont;
                                    sheet1.Range[xlsRow, ColGrs + countGrossPostion].CellStyle.Font.Size = 24;
                                    sheet1.Range[xlsRow, ColGrs + countGrossPostion].CellStyle.ShrinkToFit = true;
                                    //sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion, xlsRow + 1, ColGrs + countGrossPostion + 1].Merge();

                                    if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                                    {
                                        sheet1.Range[xlsRow, ColGrs + countGrossPostion].CellStyle.Font.Color = ExcelKnownColors.Red;
                                    }
                                    xlsCol += 1;
                                    SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                                    salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
                                    if (grossFormula.Length == 0)
                                    {
                                        grossFormula += salaryHeadSequence.XLColIndex.ToString();
                                    }
                                    else
                                    {
                                        grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
                                    }
                                    //countGrossPostion++;

                                    salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                                    salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                                    salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                                    salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                                    salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
                                    salaryHeadSequence.Sequence = ci;
                                    salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper();
                                    salaryHeadSequence.XLColIndex = ColGrs + countGrossPostion;
                                    list.Add(salaryHeadSequence);
                                    _count_earning_head += 1;
                                }
                            }//IsGrossComponent
                        }//CTC/Gross
                    }//SalaryHead 
                    #endregion
                }//for
                xlsCol += 1;
                countGrossPostion++;


                sheet1.Range[xlsRow, ColGrs + countGrossPostion].Text = "GROSS";//ru.GetLabelname(LabelNameInLocalLanguage.TotalSalary.ToString(), "GROSS");

                //sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion].CellStyle.Font.FontName = printFont;
                sheet1.Range[xlsRow, ColGrs + countGrossPostion].CellStyle.Font.Size = 24;
                //sheet1.Range[xlsRow + 1, ColGrs + countGrossPostion, xlsRow + 1, ColGrs + countGrossPostion + 1].Merge();
                //countGrossPostion++;
                _count_earning_head++;
                SalaryHeadSequence salaryHSGross = new SalaryHeadSequence();

                salaryHSGross.SalaryHead = grossFormula;
                salaryHSGross.SalaryHeadId = "Gross";
                salaryHSGross.XLColIndex = ColGrs + countGrossPostion;
                list.Add(salaryHSGross);

                int countCTCPosition = countGrossPostion;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region loop ctc
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
                        {
                            if (bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsCTCComponent"].ToString()) == true && bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString()) == false)
                            {
                                if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E")
                                {
                                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().ToUpper() == "Total Gross".ToUpper())
                                    {

                                    }
                                    else
                                    {
                                        _total_head_count++;
                                        countCTCPosition++;

                                        sheet1.Range[xlsRow, ColGrs + countCTCPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();

                                        //sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.FontName = printFont;
                                        sheet1.Range[xlsRow, ColGrs + countCTCPosition].CellStyle.Font.Size = 24;
                                        //sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition, xlsRow + 1, ColGrs + countCTCPosition + 1].Merge();
                                        sheet1.Range[xlsRow, ColGrs + countCTCPosition].CellStyle.ShrinkToFit = true;


                                        if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                                        {
                                            sheet1.Range[xlsRow, ColGrs + countCTCPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
                                        }
                                        xlsCol += 1;
                                        SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                                        salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;

                                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == bplib.clsWebLib.FESTIVAL_BONUS)//FESTIVAL_BONUS
                                        {
                                            salaryHeadSequence.HeadCategory = bplib.clsWebLib.FESTIVAL_BONUS;
                                        }

                                        if (grossFormula.Length == 0)
                                        {
                                            grossFormula += salaryHeadSequence.XLColIndex.ToString();
                                        }
                                        else
                                        {
                                            grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
                                        }
                                        //countCTCPosition++;
                                        salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                                        salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                                        salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                                        salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                                        salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
                                        salaryHeadSequence.Sequence = ci;
                                        salaryHeadSequence.Earning = "Earning";
                                        salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;
                                        list.Add(salaryHeadSequence);
                                        _count_earning_ctchead += 1;
                                    }
                                }
                            }//IsCTCComponent
                        }//CTC/Gross
                    }//SalaryHead 
                    #endregion
                }//for
                xlsCol += 1;

                countCTCPosition++;
                _count_earning_ctchead++;
                sheet1.Range[xlsRow, ColGrs + countCTCPosition].Text = "CTC";//ru.GetLabelname(labelList, LabelNameInLocalLanguage.CTC.ToString(), "CTC");
                sheet1.Range[xlsRow, ColGrs + countCTCPosition].CellStyle.Font.Size = 24;
                //sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition, xlsRow + 1, ColGrs + countCTCPosition + 1].Merge();
                //countCTCPosition++;

                SalaryHeadSequence salaryHSCTC = new SalaryHeadSequence();

                salaryHSCTC.SalaryHead = grossFormula;
                salaryHSCTC.SalaryHeadId = "CTC";
                salaryHSCTC.XLColIndex = ColGrs + countCTCPosition;
                list.Add(salaryHSCTC);

                int countDeductionPosition = countCTCPosition;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region deduction
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "TOTAL DEDUCTION" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
                        {
                            if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "D")
                            {
                                _total_head_count++;
                                countDeductionPosition++;

                                sheet1.Range[xlsRow, ColGrs + countDeductionPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                                sheet1.Range[xlsRow, ColGrs + countDeductionPosition].CellStyle.Font.Size = 24;
                                sheet1.Range[xlsRow, ColGrs + countDeductionPosition].CellStyle.ShrinkToFit = true;


                                if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                                {
                                    sheet1.Range[xlsRow, ColGrs + countDeductionPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
                                }
                                xlsCol += 1;
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


                                salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                                salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                                salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                                salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                                salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
                                salaryHeadSequence.Sequence = ci;
                                salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
                                salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();
                                salaryHeadSequence.Deduction = "Deduction";

                                list.Add(salaryHeadSequence);

                                _count_deducting_head += 1;
                            }
                        }//CTC/Gross
                    }//SalaryHead 
                    #endregion
                }//for
                SalaryHeadSequence salaryHSDed = new SalaryHeadSequence();

                salaryHSDed.SalaryHead = deductionFormula;
                salaryHSDed.SalaryHeadId = "Deduction";
                salaryHSDed.HeadType = "Deduction";
                salaryHSDed.XLColIndex = ColGrs + countDeductionPosition;
                list.Add(salaryHSDed);
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
        private void getFormulaValue(int startValue, int lastValue, List<SalaryHeadSequence> list, out string structureCell, out string salaryCell)
        {
            try
            {
                ReportUtility ru = new ReportUtility();
                structureCell = string.Empty;
                salaryCell = string.Empty;
                for (int i = 0; i < list.Count; i++)
                {
                    //var cCount = lastValue - startValue;
                    for (int c = startValue; c < lastValue; c += 2)
                    {
                        structureCell = ru.GetColumnNameForXls(list[i].XLColIndex) + c;
                        salaryCell = ru.GetColumnNameForXls(list[i].XLColIndex) + c + 1;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void getTotalAmount(string colIndex, double Amount, ref Dictionary<string, double> dict)
        {
            try
            {
                if (dict.ContainsKey(colIndex))//If has Same head
                {
                    var value = dict[colIndex];
                    double totalAmount = Convert.ToDouble(Amount) + Convert.ToDouble(value);
                    dict[colIndex] = totalAmount;

                }
                else // If New Head
                {
                    dict.Add(colIndex, Amount);
                }

            }
            catch (Exception)
            {

                throw;
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
    }
}