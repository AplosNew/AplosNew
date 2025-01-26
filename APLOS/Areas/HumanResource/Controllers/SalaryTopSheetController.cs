using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.HumanResources;
using Library.Model.Setups;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Microsoft.Reporting.WebForms;
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
using static Library.Service.Enums.SalaryHeadEnum;
using Library.Service.Enums;
using Syncfusion.ExcelToPdfConverter;
using System.Collections.Specialized;
using static OTSBD.clsReport;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class SalaryTopSheetController : BaseController
    {
        #region Constructor

        private readonly IPayRegisterBDReportService _payRegisterBDReportService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;
        Library.HumanResource.Report.Payroll.clsPayRegister _clspayRegisterBDReportService = new Library.HumanResource.Report.Payroll.clsPayRegister();



        public SalaryTopSheetController(
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

      
        public ActionResult Aplos()
        {
            return View();
        }
      
        public ActionResult DynamicTopSheet()
        {
            return View();
        }

       
        public ActionResult TopSheetDetails()
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
        //$scope.parameters = 'month=' + $scope.month + '&year=' + $scope.year + '&salaryProcessId=' + $scope.salaryProcessId + '&divisionId=' + $scope.divisionId + '&unitId=' + $scope.unitId + '&sectionId=' + $scope.sectionId + '&subSectionId=' + $scope.subSectionId + '&departmentId=' + $scope.departmentId + '&payGroupId=' + $scope.payGroupId + '&employeeCategoryId=' + $scope.employeeCategoryId + '&paymentDate=' + $scope.paymentDate + '&paymentMode=' + $scope.paymentMode + '&languageId=' + $scope.languageId + '&SalaryTopSheetCategory=' + $scope.SalaryTopSheetCategory ;
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

        public IWorkbook xSalaryTopSheetExcel(string groupId, string companyId, string plantId, string monthNo, string yearNo, string salaryPorcId, string monthStartDate, string monthEndDate, string SalaryTopSheetCategory)
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
                sheet1[ROW - 1, mergeColStartEarning].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1[ROW - 1, mergeColStartEarning].VerticalAlignment = ExcelVAlign.VAlignCenter;


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
			,sum(CTC.GrossDisbusmentAmount) AS NetPayable
			,sum(isnull(SPAD.TotalOTHr, 0) / 60) AS TotalOThr
			,sum(ot.GrossDisbusmentAmount) AS TotalOTAmount
			,count(ei.SystemId)  NoOfEmployees
			--ei.SystemId,ei.EmployeeCode,
			," + Column + @"
		FROM EmployeeInformation EI
 LEFT JOIN MST.ManpowerBudget mb ON mb.Id = eI.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
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
                AND SH.HeadCategory = 'TOTAL GROSS'
                --AND SH.IsGrossComponent = 1
				--AND sh.IsCTCComponent = 1
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
				---AND sh.IsCTCComponent = 1
            AND SH.HeadCategory = 'Net payable'
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

left join ORG.Department DP ON DP.Id=PR.DepartmentId
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
        public DataTable xGetSalaryTopSheetDataSql(string groupId, string companyId, string plantId, string monthNo, string yearNo, string salaryPorcId, string monthStartDate, string monthEndDate, string SalaryTopSheetCategory)
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
                                LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
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
        private void SetHeadTextLA(IWorksheet sheet, int xlsRow, int xlsCol, string text)
        {
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
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
        private void SetCellTextJustify(IWorksheet sheet, int xlsRow, int xlsCol, string Text)
        {
            //if (string.IsNullOrEmpty(Text) == false)
            //{
            sheet.Range[xlsRow, xlsCol].Text = Text;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignJustify;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignTop;
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

                    SetHeadText("Entity", sheet1, xlsRow, ref xlsCol, out ColFirstValue, 17);
                    SetHeadText("Department", sheet1, xlsRow, ref xlsCol, out ColSecondValue, 23);
                    SetHeadText("Subsection", sheet1, xlsRow, ref xlsCol, out ColThirdValue, 23);
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
                    //CreateDynamicSHeadTopSheet(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColThirdValue, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out list);
                    CreateDynamicSHead(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColThirdValue, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out list);
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
                        //sheet1.Range[xlsRow, np].Text = "Total Deduction";
                        //sheet1.Range[xlsRow, np].ColumnWidth = 14;
                        ////sheet1.Range[xlsRow, np -1, xlsRow, np].Merge();

                        sheet1.Range[xlsRow, np].Text = "Net Payable";
                        sheet1.Range[xlsRow, np].ColumnWidth = 14;
                        //sheet1.Range[xlsRow, np, xlsRow, np].Merge();
                    }
                    xlsCol = np + 1;
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
                    StringCollection strColSum = new StringCollection();
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
                                strColSum.Add(xlsRow.ToString());
                                foreach (var item in dictSalaryProcess)//Loop Last Summation in SalaryPorcessss
                                {
                                    try
                                    {
                                        //sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Number = Convert.ToDouble(item.Value);
                                        //sheet1.Range[xlsR 1, Convert.ToInt32(item.Ke 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
                                        sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Formula = "SUM(" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(item.Key)) + catFRow.ToString() + ":" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(item.Key)) + (xlsRow - 1).ToString() + ")";
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



                                //var grossIndexSubStructOSideST = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.XLColIndex).FirstOrDefault();
                                //var ctcIndexSubStructOSideST = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.XLColIndex).FirstOrDefault();

                                //var grossSubStructOsideST = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.SalaryHead).FirstOrDefault();
                                //var ctcSubStructOsideST = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.SalaryHead).FirstOrDefault();

                                //var dedFormulaStructOSideST = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.SalaryHead).FirstOrDefault();

                                //var grossAddSubOSideST = oRU.SetFormula((grossIndexSubStructOSideST).ToString(), xlsRow);
                                //var ctcAddSubOSideST = oRU.SetFormula((ctcIndexSubStructOSideST).ToString(), xlsRow);
                                //var dedAddStructOSideST = oRU.SetFormula(dedFormulaStructOSideST, xlsRow);

                                //sheet1.Range[xlsRow, grossIndexSubStructOSideST].Formula = "=" + oRU.SetFormula(grossSubStructOsideST, xlsRow);
                                //sheet1.Range[xlsRow, grossIndexSubStructOSideST].BorderAround(ExcelLineStyle.Thin);
                                ////sheet1.Range[xlsRow + 1, grossIndexSubStructOSide - 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage); ;

                                //sheet1.Range[xlsRow, grossIndexSubStructOSideST].CellStyle.Font.Size = 28;
                                //sheet1.Range[xlsRow, grossIndexSubStructOSideST].CellStyle.Font.Bold = true;
                                ////sheet1.Range[xlsRow + 1, grossIndexSubStructOSide, xlsRow + 1, grossIndexSubStructOSide].Merge();

                                //sheet1.Range[xlsRow, ctcIndexSubStructOSideST].Formula = "=" + oRU.SetFormula(ctcSubStructOsideST, xlsRow);
                                ////sheet1.Range[xlsR 1, ctcIndexSubStructOSi 1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage); ;
                                //sheet1.Range[xlsRow, ctcIndexSubStructOSideST].BorderAround(ExcelLineStyle.Thin);


                                //sheet1.Range[xlsRow, ctcIndexSubStructOSideST].CellStyle.Font.Size = 28;
                                //sheet1.Range[xlsRow, ctcIndexSubStructOSideST].CellStyle.Font.Bold = true;
                                ////sheet1.Range[xlsRow + 1, ctcIndexSubStructOSide, xlsRow + 1, ctcIndexSubStructOSide].Merge();


                                //var dedAddSubSalStructOSideST = oRU.SetFormula(grossSubStructOsideST, xlsRow);

                                //   sheet1.Range[xlsRow, np].Formula = "=" + dedAddStructOSideST;//Total Deduction
                                //sheet1.Range[xlsRow, np].Text =                                                            //sheet1.Range[xlsR 1,  1].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
                                sheet1.Range[xlsRow, np].CellStyle.Font.Size = 28;
                                sheet1.Range[xlsRow, np].CellStyle.Font.Bold = true;
                                sheet1.Range[xlsRow, np].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                sheet1.Range[xlsRow, np].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, np, xlsRow, np].BorderAround(ExcelLineStyle.Thin);
                                //sheet1.Range[xlsRow + 1, np - 1, xlsRow + 1, np].Merge();

                                // sheet1.Range[xlsRow, np + 1].Formula = "=" + ctcAddSubOSideST + "-(" + dedAddStructOSideST + ")";//Net Payable
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
                                //if (ob.SalaryHeadId.ToUpper() == "CTC" || ob.SalaryHeadId.ToUpper() == "GROSS")
                                //{
                                //    var formula = ob.SalaryHead;
                                //    var hId = ob.SalaryHeadId;
                                //    _total_head_count_body++;

                                //    sheet1.Range[xlsRow, ob.XLColIndex].Formula = "=" + oRU.SetFormula(formula, xlsRow);
                                //    sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = oRU.NumberFormatDecimalTwo();
                                //    sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                //    sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                //}//ctc , gross
                                //else
                                //{
                                var hId = ob.SalaryHeadId;
                                _total_head_count_body++;

                                DataView dvBody = new DataView(dsSlrProc.Tables[0]);
                                dvBody.RowFilter = "SalaryHeadID='" + hId + "'and EntityName = '" + x + "' and DepartmentName = '" + y + "' and SubSectionName='" + z + "'";

                                if (dvBody.Count > 0)
                                {
                                    //if (ob.Deduction == "Deduction")
                                    //{
                                    if (ob.HeadType == "D")
                                    {


                                        sheet1.Range[xlsRow, ob.XLColIndex].Number = Convert.ToDouble(dvBody[0]["DisbusmentAmount"].ToString()) * (-1);

                                        //getTotalAmount(ob.XLColIndex.ToString(), Convert.ToDouble(dvBody[0]["DisbusmentAmount"].ToString()) , ref totalDictSalaryProcess);//dictSalaryProcess
                                        //getTotalAmount(ob.XLColIndex.ToString(), Convert.ToDouble(dvBody[0]["DisbusmentAmount"].ToString()) , ref dictSalaryProcess);//dictSalaryProcess
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
                                 //}

                                DataView dvNetPay = new DataView(dsSlrProc.Tables[0]);
                                dvNetPay.RowFilter = "HeadCategory ='Net Payable'and EntityName = '" + x + "' and DepartmentName = '" + y + "' and SubSectionName='" + z + "'";

                                sheet1.Range[xlsRow, np].Number = Convert.ToDouble(dvNetPay[0]["DisbusmentAmount"].ToString());
                                sheet1.Range[xlsRow, np].NumberFormat = oRU.NumberFormatDecimalTwo();
                                sheet1.Range[xlsRow, np].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                sheet1.Range[xlsRow, np].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                getTotalAmount(np.ToString(), Convert.ToDouble(dvNetPay[0]["DisbusmentAmount"].ToString()), ref totalDictSalaryProcess);
                                getTotalAmount(np.ToString(), Convert.ToDouble(dvNetPay[0]["DisbusmentAmount"].ToString()), ref dictSalaryProcess);
                                dvNetPay.RowFilter = null;

                            }//
                        }//for dtSalaryHead

                        //var grossIndex = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.XLColIndex).FirstOrDefault();
                        //var dedIndex = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.XLColIndex).FirstOrDefault();
                        //var dedFormula = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.SalaryHead).FirstOrDefault();
                        //var ctcFormula = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.SalaryHead).FirstOrDefault();


                        //var grossAdd = oRU.SetFormula(grossIndex.ToString(), xlsRow);
                        //var dedAdd = oRU.SetFormula(dedFormula, xlsRow);
                        //var ctcAdd = oRU.SetFormula(ctcFormula, xlsRow);


                        //sheet1.Range[xlsRow, np].Formula = "=" + dedAdd;
                        //sheet1.Range[xlsRow, np].NumberFormat = oRU.NumberFormatDecimalTwo();
                        //sheet1.Range[xlsRow, np].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        //sheet1.Range[xlsRow, np].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //sheet1.Range[xlsRow, np + 1].Number = "=" + ctcAdd + "-(" + dedAdd + ")";
                        //sheet1.Range[xlsRow, np + 1].NumberFormat = oRU.NumberFormatDecimalTwo();
                        //sheet1.Range[xlsRow, np + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        //sheet1.Range[xlsRow, np + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsRow++;

                    }//for emp count
                    SetHeadText(sheet1, xlsRow, ColFirstValue, " Subtotal:");
                    strColSum.Add(xlsRow.ToString());
                    foreach (var item in dictSalaryProcess)//Loop Last Summation in SalaryPorcessss
                    {
                        try
                        {
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Formula = "SUM(" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(item.Key)) + catFRow.ToString() + ":" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(item.Key)) + (xlsRow - 1).ToString() + ")";
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
                    }
                    xlsRow++;
                    //Loop End Last Summation in SalaryPorcess
                    #region SubTotal
                    SetHeadText(sheet1, xlsRow, ColFirstValue, " Grand Total:");
                    sheet1.Range[xlsRow, 1, xlsRow, 3].Merge();
                    foreach (var item in dictSalaryProcess)//Loop Last Summation in SalaryPorcessss
                    {
                        try
                        {
                            string s = "";
                            foreach (string SumItem in strColSum)
                                s += "+" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(item.Key)) + SumItem;
                            sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Formula = s;
                            //sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Number = Convert.ToDouble(item.Value);
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
                    //var grossIndexSubStructOSideSTL = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.XLColIndex).FirstOrDefault();
                    //var ctcIndexSubStructOSideSTL = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.XLColIndex).FirstOrDefault();

                    //var grossSubStructOsideSTL = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.SalaryHead).FirstOrDefault();
                    //var ctcSubStructOsideSTL = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.SalaryHead).FirstOrDefault();

                    //var dedFormulaStructOSideSTL = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.SalaryHead).FirstOrDefault();

                    //var grossAddSubOSideSTL = oRU.SetFormula((grossIndexSubStructOSideSTL).ToString(), xlsRow);
                    //var ctcAddSubOSideSTL = oRU.SetFormula((ctcIndexSubStructOSideSTL).ToString(), xlsRow);
                    //var dedAddStructOSideSTL = oRU.SetFormula(dedFormulaStructOSideSTL, xlsRow);

                    //sheet1.Range[xlsRow, grossIndexSubStructOSideSTL].Formula = "=" + oRU.SetFormula(grossSubStructOsideSTL, xlsRow);
                    //sheet1.Range[xlsRow, grossIndexSubStructOSideSTL].BorderAround(ExcelLineStyle.Thin);

                    //sheet1.Range[xlsRow, grossIndexSubStructOSideSTL].CellStyle.Font.Size = 28;
                    //sheet1.Range[xlsRow, grossIndexSubStructOSideSTL].CellStyle.Font.Bold = true;

                    //sheet1.Range[xlsRow, ctcIndexSubStructOSideSTL].Formula = "=" + oRU.SetFormula(ctcSubStructOsideSTL, xlsRow);
                    //sheet1.Range[xlsRow, ctcIndexSubStructOSideSTL].BorderAround(ExcelLineStyle.Thin);


                    //sheet1.Range[xlsRow, ctcIndexSubStructOSideSTL].CellStyle.Font.Size = 28;
                    //sheet1.Range[xlsRow, ctcIndexSubStructOSideSTL].CellStyle.Font.Bold = true;

                    //var dedAddSubSalStructOSideSTL = oRU.SetFormula(grossSubStructOsideSTL, xlsRow);

                    //sheet1.Range[xlsRow, np].Formula = "=" + dedAddStructOSideSTL;//Total Deduction
                    //sheet1.Range[xlsRow, np].CellStyle.Font.Size = 28;
                    //sheet1.Range[xlsRow, np].CellStyle.Font.Bold = true;
                    //sheet1.Range[xlsRow, np].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    //sheet1.Range[xlsRow, np].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, np, xlsRow, np].BorderAround(ExcelLineStyle.Thin);
                    ////sheet1.Range[xlsRow + 1, np - 1, xlsRow + 1, np].Merge();

                    //sheet1.Range[xlsRow, np + 1].Formula = "=" + ctcAddSubOSideSTL + "-(" + dedAddStructOSideSTL + ")";//Net Payable
                    //                                                                                                   //sheet1.Range[xlsR 1,  + 1colNetpayable].NumberFormat = oRU.NumberFormatIntLocal(localLanguage);
                    //sheet1.Range[xlsRow, np + 1].CellStyle.Font.Size = 28;
                    //sheet1.Range[xlsRow, np + 1].CellStyle.Font.Bold = true;
                    //sheet1.Range[xlsRow, np + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    //sheet1.Range[xlsRow, np + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, np + 1, xlsRow, np + 1].BorderAround(ExcelLineStyle.Thin);
                    #endregion
                    xlsRow++;
                    #region Total
                    //sheet1.Range[xlsRow, 1].Text = "Total";
                    ////sheet1.Range[xlsRow, 1].NumberFormat = oRU.NumberFormatDecimalTwo();
                    //sheet1.Range[xlsRow, 1, xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    //sheet1.Range[xlsRow, 1, xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, 1, xlsRow, 3].CellStyle.Font.Bold = true;
                    //sheet1.Range[xlsRow, 1, xlsRow, 3].Merge();// = true;
                    //sheet1.Range[xlsRow, 1, xlsRow, 3].BorderAround(ExcelLineStyle.Thin);


                    ////gross-deduction
                    //foreach (var item in totalDictSalaryProcess)//Loop Last Summation in SalaryPorcessss
                    //{
                    //    try
                    //    {
                    //        sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Number = Convert.ToDouble(item.Value);
                    //        sheet1.Range[xlsRow, Convert.ToInt32(item.Key), xlsRow, Convert.ToInt32(item.Key)].BorderAround(ExcelLineStyle.Thin);
                    //        sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].RowHeight = 40;
                    //        sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Size = 28;
                    //        sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Bold = true;
                    //        sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    //        sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //    }
                    //    catch (Exception exe)
                    //    {
                    //        throw exe;
                    //    }
                    //}//Loop End Last Summation in SalaryPorcess



                    var grossIndexSubStructOSide = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.XLColIndex).FirstOrDefault();
                    var ctcIndexSubStructOSide = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.XLColIndex).FirstOrDefault();

                    var grossSubStructOside = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.SalaryHead).FirstOrDefault();
                    var ctcSubStructOside = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.SalaryHead).FirstOrDefault();

                    var dedFormulaStructOSide = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.SalaryHead).FirstOrDefault();

                    //var grossAddSubOSide = oRU.SetFormula((grossIndexSubStructOSide).ToString(), xlsRow);
                    //var ctcAddSubOSide = oRU.SetFormula((ctcIndexSubStructOSide).ToString(), xlsRow);
                    //var dedAddStructOSide = oRU.SetFormula(dedFormulaStructOSide, xlsRow);

                    //sheet1.Range[xlsRow, grossIndexSubStructOSide].Formula = "=" + oRU.SetFormula(grossSubStructOside, xlsRow);
                    //sheet1.Range[xlsRow, grossIndexSubStructOSide].BorderAround(ExcelLineStyle.Thin);
                    //sheet1.Range[xlsRow, grossIndexSubStructOSide].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1.Range[xlsRow, grossIndexSubStructOSide].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //sheet1.Range[xlsRow, grossIndexSubStructOSide].CellStyle.Font.Size = 28;
                    //sheet1.Range[xlsRow, grossIndexSubStructOSide].CellStyle.Font.Bold = true;

                    //sheet1.Range[xlsRow, ctcIndexSubStructOSide].Formula = "=" + oRU.SetFormula(ctcSubStructOside, xlsRow);
                    //sheet1.Range[xlsRow, ctcIndexSubStructOSide].BorderAround(ExcelLineStyle.Thin);
                    //sheet1.Range[xlsRow, ctcIndexSubStructOSide].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1.Range[xlsRow, ctcIndexSubStructOSide].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, ctcIndexSubStructOSide].CellStyle.Font.Size = 28;
                    //sheet1.Range[xlsRow, ctcIndexSubStructOSide].CellStyle.Font.Bold = true;


                    //var dedAddSubSalStructOSide = oRU.SetFormula(grossSubStructOside, xlsRow);

                    //sheet1.Range[xlsRow, np].Formula = "=" + dedAddStructOSide;//Total Deduction
                    //sheet1.Range[xlsRow, np].CellStyle.Font.Size = 28;
                    //sheet1.Range[xlsRow, np].CellStyle.Font.Bold = true;
                    //sheet1.Range[xlsRow, np].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    //sheet1.Range[xlsRow, np].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, np, xlsRow, np].BorderAround(ExcelLineStyle.Thin);

                    //sheet1.Range[xlsRow, np + 1].Formula = "=" + ctcAddSubOSide + "-(" + dedAddStructOSide + ")";//Net Payable
                    //sheet1.Range[xlsRow, np + 1].CellStyle.Font.Size = 28;
                    //sheet1.Range[xlsRow, np + 1].CellStyle.Font.Bold = true;
                    //sheet1.Range[xlsRow, np + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    //sheet1.Range[xlsRow, np + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, np + 1, xlsRow, np + 1].BorderAround(ExcelLineStyle.Thin);

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
        [HttpGet, Authorize]
        public ActionResult XlsSalaryTopLailaSheet(string year, string month, string salaryProcessId, string groupBy, string employeeCategoryId, string employeeStatus)
        {
            #region Variable
            groupBy = "DepartmentEmployeeCategory";
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
            IWorksheet sheet2 = null;


            excelEngine = new ExcelEngine();
            application = excelEngine.Excel;

            workbook = application.Workbooks.Create(2);
            sheet1 = workbook.Worksheets[1];
            sheet2 = workbook.Worksheets[0];

            sheet1.IsGridLinesVisible = true;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            string NumberFormatString = "#,##0;(#,##0)";

            string FactoryName = "";
            string CmpName = "";

            #endregion Variable
            string[] strEmpStatus = new string[] { "" };
            //somewhere in your code
            strEmpStatus = employeeStatus.Split(',');
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
                int startXlsRow = 5;
                var daysInMonth = 0;
                daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(para.Year), Convert.ToInt32(para.Month));//Number of Days in a month

                para.CompanyGroupId = identity.CompanyGroupId;
                para.CompanyId = identity.CompanyId;
                para.PlantId = identity.PlantId;
                para.FromDate = "01-" + bplib.clsWebLib.GetMonthName(month) + "-" + year;
                para.ToDate = daysInMonth + "-" + bplib.clsWebLib.GetMonthName(para.Month) + "-" + para.Year;
                para.SalaryProcessId = salaryProcessId;
                para.CompanyId = identity.CompanyId;
                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);
                #endregion Variable

                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                Dictionary<string, DataTable> dicStatus = new Dictionary<string, DataTable>();
                List<DataRow> _data = new List<DataRow>();


                #region Employee Status
                for (int ies = 0; ies < strEmpStatus.Length; ies++)
                {

                    #region DataSet                
                    objRpt.GetSalaryTopSheet(para, groupBy, strEmpStatus[ies], employeeCategoryId, out dsSlrProc);
                    dicStatus.Add(strEmpStatus[ies], dsSlrProc.Tables[0]);

                    dvSlrProc = new DataView();
                    dvSlrProc.Table = dsSlrProc.Tables[0];
                    DataView dvEmp = new DataView();
                    dvEmp.Table = dsSlrProc.Tables[0];
                    DataTable dtEmployees = new DataTable();
                    dtEmployees = null;
                    if (groupBy == "DepartmentEmployeeCategory")
                    {
                        dtEmployees = dvEmp.ToTable(true, "EntityName", "EntityId", "SectionName", "SectionId", "SubSectionName", "Line", "LineId", "SubSectionId", "DepartmentName", "DepartmentID", "EmpCategoryName", "EmployeeCategorySystemID", "IntegerInDisb", "DecimalNo");

                    }
                    if (groupBy == "DepartmentSubSctionEmployeeCatagory")
                    {
                        dtEmployees = dvEmp.ToTable(true, "DepartmentName", "DepartmentID", "SubSectionName", "SubSectionID", "EmpCategoryName", "EmployeeCategorySystemID", "IntegerInDisb", "DecimalNo");

                    }
                    //if (dtEmployees.Rows.Count == 0)
                    //{
                    //    Exception ex = new Exception("No Data found...");
                    //    throw (ex);
                    //}


                    #endregion DataSet
                    IDictionary<string, string> dictGroup = new Dictionary<string, string>();

                    if (dtEmployees.Rows.Count > 0)
                    {
                        dictGroup = null;

                        #region------------------Column Header------------------
                        xlsRow = startXlsRow;
                        sheet1.Range[xlsRow, 1].Text = strEmpStatus[ies];
                        sheet1.Range[xlsRow, 1, xlsRow, 2].Merge();
                        sheet1.Range[xlsRow, 1].RowHeight = 30;
                        sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 25;


                        xlsRow++;

                        xlsCol = 1;

                        int colTotalEmp = 0;
                        bool isFirst = true;
                        bool isFirstGrp2 = true;

                        Dictionary<string, double> dictSalaryProcess = new Dictionary<string, double>();
                        Dictionary<string, double> dictSalaryProcessGrp2 = new Dictionary<string, double>();


                        int ColFirstValue = xlsCol;
                        int ColSecondValue = xlsCol;
                        int ColThirdValue = xlsCol;
                        int ColFourthValue = xlsCol;
                        int ColFifthValue = xlsCol;
                        int ColSixthValue = xlsCol;

                        xlsRow += 1;

                        if (groupBy == "DepartmentSubSctionEmployeeCatagory")
                        {
                            SetHeadText("Unit", sheet1, xlsRow, ref xlsCol, out ColFirstValue, 17);
                            SetHeadText("Department", sheet1, xlsRow, ref xlsCol, out ColSecondValue, 17);
                            SetHeadText("Section", sheet1, xlsRow, ref xlsCol, out ColThirdValue, 23);
                            SetHeadText("Subsection", sheet1, xlsRow, ref xlsCol, out ColFourthValue, 23);
                            SetHeadText("Line", sheet1, xlsRow, ref xlsCol, out ColFifthValue, 23);
                            SetHeadText("EmployeeCategory", sheet1, xlsRow, ref xlsCol, out ColSixthValue, 23);

                            SetHeadText("Total Employee", sheet1, xlsRow, ref xlsCol, out colTotalEmp, 23);

                        }
                        if (groupBy == "DepartmentEmployeeCategory")
                        {
                            SetHeadText("Unit", sheet1, xlsRow, ref xlsCol, out ColFirstValue, 17);
                            SetHeadText("Department", sheet1, xlsRow, ref xlsCol, out ColSecondValue, 17);
                            SetHeadText("Section", sheet1, xlsRow, ref xlsCol, out ColThirdValue, 23);
                            SetHeadText("Subsection", sheet1, xlsRow, ref xlsCol, out ColFourthValue, 23);
                            SetHeadText("Line", sheet1, xlsRow, ref xlsCol, out ColFifthValue, 23);
                            SetHeadText("EmployeeCategory", sheet1, xlsRow, ref xlsCol, out ColSixthValue, 23);
                            SetHeadText("Total Employee", sheet1, xlsRow, ref xlsCol, out colTotalEmp, 23);
                        }

                        var endGenericCol = xlsCol - 2;

                        var totalDictSalaryStruct = new Dictionary<string, double>();
                        var totalDictSalaryProcess = new Dictionary<string, double>();

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
                        endGenericCol += 1;
                        //CreateDynamicSHeadTopSheet(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColThirdValue, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out list);
                        CreateDynamicSHead(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref endGenericCol, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out list);

                        int ds = endGenericCol + _count_earning_head + _count_earning_ctchead;


                        if (_count_earning_head > 0)
                        {
                            sheet1.Range[xlsRow - 1, endGenericCol + 1].Text = "Earning";
                            sheet1.Range[xlsRow - 1, endGenericCol + 1, xlsRow - 1, ds - 1].Merge();
                        }

                        if (_count_deducting_head > 0)
                        {
                            sheet1.Range[xlsRow - 1, ds].Text = "Deduction";
                            sheet1.Range[xlsRow - 1, ds, xlsRow - 1, ds + _count_deducting_head].Merge();
                        }
                        endGenericCol -= 1;
                        int np = 0;
                        if (list.Count > 0)
                        {
                            xlsCol++;
                            np = ds + _count_deducting_head + 1;
                            sheet1.Range[xlsRow, np].Text = "Net Payable";
                            sheet1.Range[xlsRow, np].ColumnWidth = 14;
                            //sheet1.Range[xlsRow, np, xlsRow, np].Merge();
                        }
                        xlsCol = np;
                        //sheet1.Range[xlsRow, xlsCol].Text = "Signature";
                        //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 26;
                        //int ColSigna = xlsCol;
                        //sheet1.Range[xlsRow, ColSigna, xlsRow, ColSigna].Merge();

                        sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(13, 177, 241);
                        sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                        sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        endXlsCol = xlsCol;
                        #endregion------------------Column Header------------------

                        int RowIndex = xlsRow + 3;
                        #region ----------------------Data-----------------------

                        int SrNo = 0;
                        string x = "";
                        string x1 = "";
                        string x2 = "";
                        string x3 = "";
                        string x4 = "";
                        string x5 = "";



                        ReportUtility oRU = new ReportUtility();

                        xlsRow = RowIndex;

                        string _grp1 = string.Empty;
                        string _grp2 = string.Empty;
                        string _grp3 = string.Empty;
                        string _grp4 = string.Empty;
                        string _grp5 = string.Empty;
                        string _grp6 = string.Empty;

                        xlsRow--;
                        xlsRow--;
                        var catFRow = xlsRow;
                        var catGrp2FRow = xlsRow;
                        var catGrp3FRow = xlsRow;
                        int catGrp4FRow = xlsRow;
                        int catGrp5FRow = xlsRow;
                        int catGrp6FRow = xlsRow;
                        ArrayList rowList = new ArrayList();
                        var lastGenColValue = string.Empty;
                        StringCollection strColSum = new StringCollection();
                        StringCollection strColSumForEntity = new StringCollection();


                        #region Active Employees

                        for (int i = 0; i < dtEmployees.Rows.Count; i++)
                        {
                            //xlsRow++;
                            #region empinfo col Data                    

                            //****************************************
                            var catLRow = xlsRow;
                            if (_grp1 != dtEmployees.Rows[i]["EntityName"].ToString() && string.IsNullOrEmpty(dtEmployees.Rows[i]["EntityName"].ToString()) == false)
                            {
                                try
                                {
                                    _grp1 = dtEmployees.Rows[i]["EntityName"].ToString();

                                    #region Subtotal
                                    if (catFRow < xlsRow)
                                    {

                                        #region Group2 Subtotal Sum
                                        lastGenColValue = _grp2;
                                        rowList.Add(xlsRow);//_grp2
                                        SetHeadTextLA(sheet1, xlsRow, ColSixthValue, _grp2 + " Subtotal:");
                                        sheet1.Range[xlsRow, ColSecondValue, xlsRow, ColSixthValue].Merge();// = Convert.ToDouble(item.Value);

                                        foreach (var item in dictSalaryProcessGrp2)//Loop Last Summation in SalaryPorcessss
                                        {
                                            try
                                            {
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Number = Convert.ToDouble(item.Value);
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key), xlsRow, Convert.ToInt32(item.Key)].BorderAround(ExcelLineStyle.Thin);
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].RowHeight = 18;
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Size = 8;
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Bold = true;
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                            }
                                            catch (Exception exe)
                                            {
                                                throw exe;
                                            }
                                        }//Loop End Last Summation in SalaryPorcess
                                        sheet1.Range[xlsRow, np].Formula = "=SUM(" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(np.ToString())) + catFRow.ToString() + ":" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(np.ToString())) + (xlsRow - 1).ToString() + ")";
                                        sheet1.Range[xlsRow, ColSecondValue, xlsRow, np].CellStyle.Interior.Color = System.Drawing.Color.LightBlue;

                                        //strColSum.Add(xlsRow.ToString());
                                        if (isFirstGrp2 == false)
                                        {
                                            //strColSumForEntity = new StringCollection();
                                            dictSalaryProcessGrp2 = new Dictionary<string, double>();
                                        }
                                        xlsRow++;
                                        #endregion


                                        lastGenColValue = _grp1;
                                        rowList.Add(xlsRow);
                                        SetHeadTextLA(sheet1, xlsRow, ColFirstValue, dtEmployees.Rows[i - 1]["EntityName"].ToString() + " Subtotal:");
                                        sheet1.Range[xlsRow, ColFirstValue, xlsRow, ColSixthValue].Merge();// = Convert.ToDouble(item.Value);

                                        foreach (var item in dictSalaryProcess)//Loop Last Summation in SalaryPorcessss
                                        {
                                            try
                                            {
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Number = Convert.ToDouble(item.Value);
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key), xlsRow, Convert.ToInt32(item.Key)].BorderAround(ExcelLineStyle.Thin);
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].RowHeight = 18;
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Size = 8;
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Bold = true;
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                            }
                                            catch (Exception exe)
                                            {
                                                throw exe;
                                            }
                                        }//Loop End Last Summation in SalaryPorcess
                                        //strColSumForEntity.Add(xlsRow.ToString());

                                        sheet1.Range[xlsRow, np].Formula = "=SUM(" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(np.ToString())) + catFRow.ToString() + ":" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(np.ToString())) + (xlsRow - 1).ToString() + ")";

                                        strColSum.Add(xlsRow.ToString());
                                        if (isFirst == false)
                                        {
                                            dictSalaryProcess = new Dictionary<string, double>();
                                            sheet1.Range[xlsRow, ColFirstValue, xlsRow, np].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(254, 191, 0);

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
                                    _grp3 = dtEmployees.Rows[i]["SectionName"].ToString();
                                    SetCellText(sheet1, xlsRow, ColThirdValue, _grp3);
                                    _grp4 = dtEmployees.Rows[i]["SubSectionName"].ToString();
                                    SetCellText(sheet1, xlsRow, ColFourthValue, _grp4);
                                    _grp5 = dtEmployees.Rows[i]["Line"].ToString();
                                    SetCellText(sheet1, xlsRow, ColFifthValue, _grp5);
                                    _grp6 = dtEmployees.Rows[i]["EmpCategoryName"].ToString();
                                    SetCellText(sheet1, xlsRow, ColSixthValue, _grp6);

                                    if (catFRow < xlsRow)
                                    {
                                        catFRow = xlsRow;
                                        catGrp2FRow = xlsRow;
                                        catGrp3FRow = xlsRow;
                                        catGrp4FRow = xlsRow;
                                        catGrp5FRow = xlsRow;
                                        catGrp6FRow = xlsRow;
                                    }
                                }
                                catch (Exception ex)
                                {

                                    throw ex;
                                }
                            }

                            else if (_grp2 != dtEmployees.Rows[i]["DepartmentName"].ToString())
                            {
                                try
                                {
                                    #region Subtotal
                                    if (catGrp2FRow < xlsRow)
                                    {

                                        lastGenColValue = _grp2;
                                        rowList.Add(xlsRow);
                                        SetHeadTextLA(sheet1, xlsRow, ColSixthValue, _grp2 + " Subtotal:");
                                        sheet1.Range[xlsRow, ColSecondValue, xlsRow, ColSixthValue].Merge();// = Convert.ToDouble(item.Value);

                                        foreach (var item in dictSalaryProcessGrp2)//Loop Last Summation in SalaryPorcessss
                                        {
                                            try
                                            {
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Number = Convert.ToDouble(item.Value);
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key), xlsRow, Convert.ToInt32(item.Key)].BorderAround(ExcelLineStyle.Thin);
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].RowHeight = 18;
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Size = 8;
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Bold = true;
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                            }
                                            catch (Exception exe)
                                            {
                                                throw exe;
                                            }
                                        }//Loop End Last Summation in SalaryPorcess
                                        sheet1.Range[xlsRow, np].Formula = "=SUM(" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(np.ToString())) + catGrp2FRow.ToString() + ":" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(np.ToString())) + (xlsRow - 1).ToString() + ")";
                                        strColSumForEntity.Add(xlsRow.ToString());

                                        //strColSum.Add(xlsRow.ToString());
                                        if (isFirstGrp2 == false)
                                        {
                                            dictSalaryProcessGrp2 = new Dictionary<string, double>();
                                            //strColSumForEntity = new StringCollection();
                                            sheet1.Range[xlsRow, ColSecondValue, xlsRow, np].CellStyle.Interior.Color = System.Drawing.Color.LightBlue;
                                        }
                                        xlsRow++;
                                    }
                                    #endregion

                                    _grp2 = dtEmployees.Rows[i]["DepartmentName"].ToString();
                                    //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                                    sheet1.Range[xlsRow, ColSecondValue].Text = _grp2;
                                    sheet1.Range[xlsRow, ColSecondValue, xlsRow, ColSecondValue].BorderAround(ExcelLineStyle.Hair);
                                    sheet1.Range[xlsRow, ColSecondValue].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                                    sheet1.Range[xlsRow, ColSecondValue].VerticalAlignment = ExcelVAlign.VAlignTop;



                                    _grp3 = dtEmployees.Rows[i]["SectionName"].ToString();
                                    SetCellTextJustify(sheet1, xlsRow, ColThirdValue, _grp3);
                                    _grp4 = dtEmployees.Rows[i]["SubSectionName"].ToString();
                                    SetCellTextJustify(sheet1, xlsRow, ColFourthValue, _grp4);
                                    _grp5 = dtEmployees.Rows[i]["Line"].ToString();
                                    SetCellTextJustify(sheet1, xlsRow, ColFifthValue, _grp5);
                                    _grp6 = dtEmployees.Rows[i]["EmpCategoryName"].ToString();
                                    SetCellTextJustify(sheet1, xlsRow, ColSixthValue, _grp6);
                                    //sheet1.Range[catGrp2FRow, ColSecondValue, xlsRow, ColSecondValue].Merge();
                                    sheet1.Range[catGrp2FRow, ColSecondValue, xlsRow, ColSecondValue].BorderAround(ExcelLineStyle.Hair);
                                    if (catGrp2FRow < xlsRow)
                                    {
                                        catGrp2FRow = xlsRow;

                                        catGrp3FRow = xlsRow;
                                        catGrp4FRow = xlsRow;
                                        catGrp5FRow = xlsRow;
                                        catGrp6FRow = xlsRow;


                                    }
                                }
                                catch (Exception ex)
                                {

                                    throw ex;
                                }
                            }
                            else if (_grp3 != dtEmployees.Rows[i]["SectionName"].ToString())
                            {
                                if (_grp3 == "Sewing")
                                {
                                    var dt = "Got it";
                                }
                                try
                                {
                                    _grp3 = dtEmployees.Rows[i]["SectionName"].ToString();
                                    SetCellText(sheet1, xlsRow, ColThirdValue, dtEmployees.Rows[i]["SectionName"].ToString());

                                    //sheet1.Range[catFRow, ColFirstValue, xlsRow, ColFirstValue].Merge();
                                    //sheet1.Range[catFRow, ColFirstValue, xlsRow, ColFirstValue].BorderAround(ExcelLineStyle.Hair);
                                    //sheet1.Range[catGrp2FRow, ColSecondValue, xlsRow, ColSecondValue].Merge();
                                    //sheet1.Range[catGrp2FRow, ColSecondValue, xlsRow, ColSecondValue].BorderAround(ExcelLineStyle.Hair);
                                    //sheet1.Range[catGrp3FRow, ColThirdValue, xlsRow, ColThirdValue].Merge();
                                    //sheet1.Range[catGrp3FRow, ColThirdValue, xlsRow, ColThirdValue].BorderAround(ExcelLineStyle.Hair);


                                    _grp4 = dtEmployees.Rows[i]["SubSectionName"].ToString();
                                    SetCellTextJustify(sheet1, xlsRow, ColFourthValue, _grp4);
                                    _grp5 = dtEmployees.Rows[i]["Line"].ToString();
                                    SetCellTextJustify(sheet1, xlsRow, ColFifthValue, _grp5);
                                    _grp6 = dtEmployees.Rows[i]["EmpCategoryName"].ToString();
                                    SetCellTextJustify(sheet1, xlsRow, ColSixthValue, _grp6);
                                    if (catGrp3FRow < xlsRow)
                                    {
                                        catGrp4FRow = xlsRow;
                                        catGrp5FRow = xlsRow;
                                        catGrp6FRow = xlsRow;


                                    }
                                }
                                catch (Exception ex)
                                {

                                    throw ex;
                                }
                            }
                            else if (_grp4 != dtEmployees.Rows[i]["SubSectionName"].ToString())
                            {

                                try
                                {
                                    _grp4 = dtEmployees.Rows[i]["SubSectionName"].ToString();
                                    SetCellText(sheet1, xlsRow, ColFourthValue, _grp4);

                                    //sheet1.Range[catFRow, ColFirstValue, xlsRow, ColFirstValue].Merge();
                                    //sheet1.Range[catFRow, ColFirstValue, xlsRow, ColFirstValue].BorderAround(ExcelLineStyle.Hair);
                                    //sheet1.Range[catGrp2FRow, ColSecondValue, xlsRow, ColSecondValue].Merge();
                                    //sheet1.Range[catGrp2FRow, ColSecondValue, xlsRow, ColSecondValue].BorderAround(ExcelLineStyle.Hair);
                                    //sheet1.Range[catGrp3FRow, ColThirdValue, xlsRow, ColThirdValue].Merge();
                                    //sheet1.Range[catGrp3FRow, ColThirdValue, xlsRow, ColThirdValue].BorderAround(ExcelLineStyle.Hair);
                                    //sheet1.Range[catGrp4FRow, ColFourthValue, xlsRow, ColFourthValue].Merge();
                                    //sheet1.Range[catGrp4FRow, ColFourthValue, xlsRow, ColFourthValue].BorderAround(ExcelLineStyle.Hair);


                                    _grp5 = dtEmployees.Rows[i]["Line"].ToString();
                                    SetCellTextJustify(sheet1, xlsRow, ColFifthValue, _grp5);
                                    _grp6 = dtEmployees.Rows[i]["EmpCategoryName"].ToString();
                                    SetCellTextJustify(sheet1, xlsRow, ColSixthValue, _grp6);
                                    if (catGrp4FRow < xlsRow)
                                    {
                                        catGrp5FRow = xlsRow;
                                        catGrp6FRow = xlsRow;
                                    }
                                }
                                catch (Exception ex)
                                {

                                    throw ex;
                                }
                            }
                            else if (_grp5 != dtEmployees.Rows[i]["Line"].ToString())
                            {

                                try
                                {
                                    _grp5 = dtEmployees.Rows[i]["Line"].ToString();
                                    SetCellText(sheet1, xlsRow, ColFifthValue, _grp5);
                                    //sheet1.Range[catFRow, ColFirstValue, xlsRow, ColFirstValue].Merge();
                                    //sheet1.Range[catFRow, ColFirstValue, xlsRow, ColFirstValue].BorderAround(ExcelLineStyle.Hair);
                                    //sheet1.Range[catGrp2FRow, ColSecondValue, xlsRow, ColSecondValue].Merge();
                                    //sheet1.Range[catGrp2FRow, ColSecondValue, xlsRow, ColSecondValue].BorderAround(ExcelLineStyle.Hair);
                                    //sheet1.Range[catGrp3FRow, ColThirdValue, xlsRow, ColThirdValue].Merge();
                                    //sheet1.Range[catGrp3FRow, ColThirdValue, xlsRow, ColThirdValue].BorderAround(ExcelLineStyle.Hair);
                                    //sheet1.Range[catGrp4FRow, ColFourthValue, xlsRow, ColFourthValue].Merge();
                                    //sheet1.Range[catGrp4FRow, ColFourthValue, xlsRow, ColFourthValue].BorderAround(ExcelLineStyle.Hair);
                                    //sheet1.Range[catGrp4FRow, ColFifthValue, xlsRow, ColFifthValue].Merge();
                                    //sheet1.Range[catGrp4FRow, ColFifthValue, xlsRow, ColFifthValue].BorderAround(ExcelLineStyle.Hair);

                                    _grp6 = dtEmployees.Rows[i]["EmpCategoryName"].ToString();
                                    SetCellTextJustify(sheet1, xlsRow, ColSixthValue, _grp6);
                                    if (catGrp5FRow < xlsRow)
                                    {

                                        catGrp6FRow = xlsRow;
                                    }
                                }
                                catch (Exception ex)
                                {

                                    throw ex;
                                }
                            }
                            else if (_grp6 != dtEmployees.Rows[i]["EmpCategoryName"].ToString())
                            {

                                try
                                {
                                    _grp6 = dtEmployees.Rows[i]["EmpCategoryName"].ToString();
                                    SetCellText(sheet1, xlsRow, ColSixthValue, _grp6);

                                    //sheet1.Range[catFRow, ColFirstValue, xlsRow, ColFirstValue].Merge();
                                    //sheet1.Range[catFRow, ColFirstValue, xlsRow, ColFirstValue].BorderAround(ExcelLineStyle.Hair);
                                    //sheet1.Range[catGrp2FRow, ColSecondValue, xlsRow, ColSecondValue].Merge();
                                    //sheet1.Range[catGrp2FRow, ColSecondValue, xlsRow, ColSecondValue].BorderAround(ExcelLineStyle.Hair);
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }

                            }

                            SrNo += 1;//colTotalEmp

                            #endregion
                            x = dtEmployees.Rows[i]["EntityName"].ToString().Trim();
                            //y = dtEmployees.Rows[i]["DepartmentName"].ToString().Trim();
                            //z = dtEmployees.Rows[i]["SubSectionName"].ToString().Trim();
                            //*****************************************
                            if (isFirst == true)
                            {
                                isFirst = false;
                            }
                            if (isFirstGrp2 == true)
                            {
                                isFirstGrp2 = false;
                            }

                            SrNo += 1;//colTotalEmp

                            #endregion
                            if (groupBy == "DepartmentSubSctionEmployeeCatagory")
                            {
                                x = dtEmployees.Rows[i]["DepartmentID"].ToString().Trim();
                                //y = dtEmployees.Rows[i]["SubSectionID"].ToString().Trim();
                                //z = dtEmployees.Rows[i]["EmployeeCategorySystemID"].ToString().Trim();
                            }
                            if (groupBy == "DepartmentEmployeeCategory")
                            {
                                x = dtEmployees.Rows[i]["EntityId"].ToString().Trim();
                                x1 = dtEmployees.Rows[i]["DepartmentID"].ToString().Trim();
                                x2 = dtEmployees.Rows[i]["SectionId"].ToString().Trim();
                                x3 = dtEmployees.Rows[i]["SubSectionId"].ToString().Trim();
                                x4 = dtEmployees.Rows[i]["LineId"].ToString().Trim();
                                x5 = dtEmployees.Rows[i]["EmployeeCategorySystemID"].ToString().Trim();


                            }

                            int _total_head_count_body = 0;
                            DataView dvTotalEmp = new DataView(dsSlrProc.Tables[0]);
                            if (groupBy == "DepartmentSubSctionEmployeeCatagory")
                            {

                                //dvTotalEmp.RowFilter = "HeadCategory='Basic' and EntityId = '" + x + "' and DepartmentID = '" + y + "' and EmployeeCategorySystemID='" + z + "'";
                                //getTotalAmount(colTotalEmp.ToString(), Convert.ToDouble(dvTotalEmp[0]["TotalEmp"].ToString()), ref totalDictSalaryProcess);
                                //getTotalAmount(colTotalEmp.ToString(), Convert.ToDouble(dvTotalEmp[0]["TotalEmp"].ToString()), ref dictSalaryProcess);

                                sheet1.Range[xlsRow, colTotalEmp].Number = Convert.ToDouble(dvTotalEmp[0]["TotalEmp"].ToString());

                            }
                            if (groupBy == "DepartmentEmployeeCategory")
                            {

                                dvTotalEmp.RowFilter = "ISNULL(HeadCategory,'')='Basic'and ISNULL(EntityId,'') = '" + x + "'and ISNULL(DepartmentID,'')='" + x1 + "' and ISNULL(SectionId,'')='" + x2 + "' and ISNULL(SubSectionId,'')='" + x3 + "' and ISNULL(LineId,'')='" + x4 + "' and ISNULL(EmployeeCategorySystemID,'')='" + x5 + "'";
                                getTotalAmount(colTotalEmp.ToString(), Convert.ToDouble(dvTotalEmp[0]["TotalEmp"].ToString()), ref totalDictSalaryProcess);
                                getTotalAmount(colTotalEmp.ToString(), Convert.ToDouble(dvTotalEmp[0]["TotalEmp"].ToString()), ref dictSalaryProcess);
                                getTotalAmount(colTotalEmp.ToString(), Convert.ToDouble(dvTotalEmp[0]["TotalEmp"].ToString()), ref dictSalaryProcessGrp2);

                                sheet1.Range[xlsRow, colTotalEmp].Number = Convert.ToDouble(dvTotalEmp[0]["TotalEmp"].ToString());
                            }

                            for (int ci = 0; ci < list.Count; ci++)
                            {
                                try
                                {
                                    var ob = list[ci];
                                    if (ob.SalaryHead.Length > 0)
                                    {

                                        var hId = ob.SalaryHeadId;
                                        _total_head_count_body++;

                                        DataView dvBody = new DataView(dsSlrProc.Tables[0]);
                                        if (groupBy == "DepartmentSubSctionEmployeeCatagory")
                                        {

                                            //dvBody.RowFilter = "HeadCategory='Basic' and DepartmentID = '" + x + "' and SubSectionID = '" + y + "' and EmployeeCategorySystemID='" + z + "'";
                                            //getTotalAmount(colTotalEmp.ToString(), Convert.ToDouble(dvBody[0]["TotalEmp"].ToString()), ref totalDictSalaryProcess);
                                            //getTotalAmount(colTotalEmp.ToString(), Convert.ToDouble(dvBody[0]["TotalEmp"].ToString()), ref dictSalaryProcess);

                                            //sheet1.Range[xlsRow, colTotalEmp].Number = Convert.ToDouble(dvBody[0]["TotalEmp"].ToString());
                                            dvBody.RowFilter = "SalaryHeadID='" + hId + "'and ISNULL(EntityId,'') = '" + x + "'and ISNULL(DepartmentID,'')='" + x1 + "' and ISNULL(SectionId,'')='" + x2 + "' and ISNULL(SubSectionId,'')='" + x3 + "' and ISNULL(LineId,'')='" + x4 + "' and ISNULL(EmployeeCategorySystemID,'')='" + x5 + "'";

                                        }
                                        if (groupBy == "DepartmentEmployeeCategory")
                                        {
                                            if (x == "20189")
                                            {

                                            }
                                            //dvBody.RowFilter = "HeadCategory='Basic'and DepartmentID = '" + x + "' and EmployeeCategorySystemID='" + y + "'";
                                            //getTotalAmount(colTotalEmp.ToString(), Convert.ToDouble(dvBody[0]["TotalEmp"].ToString()), ref totalDictSalaryProcess);
                                            //getTotalAmount(colTotalEmp.ToString(), Convert.ToDouble(dvBody[0]["TotalEmp"].ToString()), ref dictSalaryProcess);

                                            //sheet1.Range[xlsRow, colTotalEmp].Number = Convert.ToDouble(dvBody[0]["TotalEmp"].ToString());

                                            dvBody.RowFilter = "SalaryHeadID='" + hId + "'and ISNULL(EntityId,'') = '" + x + "'and ISNULL(DepartmentID,'')='" + x1 + "' and ISNULL(SectionId,'')='" + x2 + "' and ISNULL(SubSectionId,'')='" + x3 + "' and ISNULL(LineId,'')='" + x4 + "' and ISNULL(EmployeeCategorySystemID,'')='" + x5 + "'";


                                        }

                                        if (dvBody.Count > 0)
                                        {
                                            try
                                            {
                                                if (ob.HeadType == "D")
                                                {
                                                    sheet1.Range[xlsRow, ob.XLColIndex].Number = Convert.ToDouble(dvBody[0]["DisbusmentAmount"].ToString()) * (-1);

                                                    getTotalAmount(ob.XLColIndex.ToString(), Convert.ToDouble(dvBody[0]["DisbusmentAmount"].ToString()) * (-1), ref totalDictSalaryProcess);//dictSalaryProcess
                                                    getTotalAmount(ob.XLColIndex.ToString(), Convert.ToDouble(dvBody[0]["DisbusmentAmount"].ToString()) * (-1), ref dictSalaryProcess);//dictSalaryProcess
                                                    getTotalAmount(ob.XLColIndex.ToString(), Convert.ToDouble(dvBody[0]["DisbusmentAmount"].ToString()) * (-1), ref dictSalaryProcessGrp2);//dictSalaryProcess


                                                }
                                                else
                                                {
                                                    sheet1.Range[xlsRow, ob.XLColIndex].Number = Convert.ToDouble(dvBody[0]["DisbusmentAmount"].ToString());
                                                    getTotalAmount(ob.XLColIndex.ToString(), Convert.ToDouble(dvBody[0]["DisbusmentAmount"].ToString()), ref totalDictSalaryProcess);
                                                    getTotalAmount(ob.XLColIndex.ToString(), Convert.ToDouble(dvBody[0]["DisbusmentAmount"].ToString()), ref dictSalaryProcess);
                                                    getTotalAmount(ob.XLColIndex.ToString(), Convert.ToDouble(dvBody[0]["DisbusmentAmount"].ToString()), ref dictSalaryProcessGrp2);

                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                throw ex;
                                            }
                                            sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = GetDecimalFormat(Convert.ToBoolean(dvBody[0]["IntegerInDisb"].ToString()), dvBody[0]["DecimalNo"].ToString());

                                            //sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = oRU.NumberFormatDecimalTwo();
                                            sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                            sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                        }


                                    }//
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }
                            #region Net Pay
                            try
                            {
                                DataView dvNetPay = new DataView(dsSlrProc.Tables[0]);
                                if (groupBy == "DepartmentEmployeeCategory")
                                {
                                    dvNetPay.RowFilter = "HeadCategory ='Net Payable'and ISNULL(EntityId,'') = '" + x + "'and ISNULL(DepartmentID,'')='" + x1 + "' and ISNULL(SectionId,'')='" + x2 + "' and ISNULL(SubSectionId,'')='" + x3 + "' and ISNULL(LineId,'')='" + x4 + "' and ISNULL(EmployeeCategorySystemID,'')='" + x5 + "'";
                                }

                                sheet1.Range[xlsRow, np].Number = Convert.ToDouble(dvNetPay[0]["DisbusmentAmount"].ToString());
                                sheet1.Range[xlsRow, np].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                sheet1.Range[xlsRow, np].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                getTotalAmount(np.ToString(), Convert.ToDouble(dvNetPay[0]["DisbusmentAmount"].ToString()), ref totalDictSalaryProcess);
                                getTotalAmount(np.ToString(), Convert.ToDouble(dvNetPay[0]["DisbusmentAmount"].ToString()), ref dictSalaryProcess);
                                getTotalAmount(np.ToString(), Convert.ToDouble(dvNetPay[0]["DisbusmentAmount"].ToString()), ref dictSalaryProcessGrp2);

                                dvNetPay.RowFilter = null;
                            }
                            catch (Exception ex)
                            {

                                throw ex;
                            }
                            #endregion
                            xlsRow++;

                        }//for emp count
                        SetHeadTextLA(sheet1, xlsRow, ColSecondValue, _grp2 + " Subtotal:");
                        sheet1.Range[xlsRow, ColSecondValue, xlsRow, endGenericCol].Merge();
                        //strColSum.Add(xlsRow.ToString());
                        foreach (var item in dictSalaryProcessGrp2)//Loop Last Summation in SalaryPorcessss
                        {
                            try
                            {
                                //string s = "";
                                //foreach (string SumItem in strColSumForEntity)
                                //    s += "+" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(item.Key)) + SumItem;
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Number = Convert.ToDouble(item.Value);

                                //sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Formula =s;// "SUM(" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(item.Key)) + catFRow.ToString() + ":" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(item.Key)) + (xlsRow - 1).ToString() + ")";
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key), xlsRow, Convert.ToInt32(item.Key)].BorderAround(ExcelLineStyle.Thin);
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].RowHeight = 18;
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Size = 8;
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Bold = true;
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }
                            catch (Exception exe)
                            {
                                throw exe;
                            }
                        }
                        sheet1.Range[xlsRow, ColSecondValue, xlsRow, np].CellStyle.Interior.Color = System.Drawing.Color.LightBlue;

                        xlsRow++;
                        SetHeadTextLA(sheet1, xlsRow, ColFirstValue, _grp1 + " Subtotal:");
                        sheet1.Range[xlsRow, 1, xlsRow, endGenericCol].Merge();
                        //strColSum.Add(xlsRow.ToString());
                        foreach (var item in dictSalaryProcess)//Loop Last Summation in SalaryPorcessss
                        {
                            try
                            {
                                //string s = "";
                                //foreach (string SumItem in strColSumForEntity)
                                //    s += "+" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(item.Key)) + SumItem;
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Number = Convert.ToDouble(item.Value);

                                //sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Formula = s;// "SUM(" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(item.Key)) + catFRow.ToString() + ":" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(item.Key)) + (xlsRow - 1).ToString() + ")";
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key), xlsRow, Convert.ToInt32(item.Key)].BorderAround(ExcelLineStyle.Thin);
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].RowHeight = 18;
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Size = 8;
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Bold = true;
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }
                            catch (Exception exe)
                            {
                                throw exe;
                            }
                        }

                        sheet1.Range[xlsRow, ColFirstValue, xlsRow, np].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(254, 191, 0);

                        xlsRow++;
                        //Loop End Last Summation in SalaryPorcess
                        //-----------------------------------------------GT---------------------------------------
                        #region Grand Total
                        SetHeadTextLA(sheet1, xlsRow, ColFirstValue, strEmpStatus[ies] + "  Total:");
                        sheet1.Range[xlsRow, 1, xlsRow, endGenericCol].Merge();
                        foreach (var item in totalDictSalaryProcess)//Loop Last Summation in SalaryPorcessss
                        {
                            try
                            {
                                //string s = "";
                                //foreach (string SumItem in strColSum)
                                //    s += "+" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(item.Key)) + SumItem;
                                //sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Formula = s;
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].Number = Convert.ToDouble(item.Value);
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key), xlsRow, Convert.ToInt32(item.Key)].BorderAround(ExcelLineStyle.Thin);
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].RowHeight = 18;
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Size = 8;
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].CellStyle.Font.Bold = true;
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                sheet1.Range[xlsRow, Convert.ToInt32(item.Key)].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }
                            catch (Exception exe)
                            {
                                throw exe;
                            }
                        }
                        sheet1.Range[xlsRow, ColFirstValue, xlsRow, np].CellStyle.Interior.Color = System.Drawing.Color.Yellow;

                        #endregion
                        xlsRow++;

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

                        #endregion Employee Status End
                        startXlsRow = xlsRow;
                    }
                }

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

                var sheet2Header = "Salary Summary For The  Month of " + Convert.ToDateTime(para.FromDate).ToString("MMMMM") + ", " + para.Year;
                CreateTopSheetSummary(sheet2, dicStatus, bplib.clsWebLib.GetMonthName(para.Month), para.Year, sheet2Header, CmpName, FactoryName);


                #endregion ******************Report Header******************
                #region Freeze Panes
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                //sheet1.UsedRange.CellStyle.Font.Size = 8;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.Range["A3"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$4";
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

                sheet1.Name = "TopSheet";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
                excelEngine.Dispose();
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


        public void CreateTopSheetSummary(IWorksheet sheet, Dictionary<string, DataTable> dicStatus, string monthName, string year, string sheetHeader, string cmpName, string FactoryName)
        {
            try
            {
                #region Variable
                clsReport objRpt = new clsReport();


                DataSet dsCmp = null;
                DataSet dsFactory = null;
                clsStaticInfo objs = null;

                ReportUtility ru = null;
                sheet.IsGridLinesVisible = true;

                int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
                //string NumberFormatString = "#,##0;(#,##0)";
                string NumberFormatString = "#,##0;(#,##0)";
                //string USDNumberFormatString = "#,##0.00;(#,##0.00)";
                //string FactoryName = "";
                //string CmpName = "";

                int colEmployeeStatus = 0;
                int colEmployeeCategory = 0;
                int colTotalEmployee = 0;
                int colTotalGross = 0;
                int colTotalAdv = 0;
                int colTotalNetPay = 0;
                int colAvg = 0;

                double totalEmployee = 0.00;
                double totalGross = 0.00;
                double totalAdv = 0.00;
                double totalNetPayable = 0.00;
                double totalAvg = 0.00;

                #endregion Variable


                try
                {
                    objRpt = new clsReport();
                    objs = new OTSBD.clsStaticInfo();
                    ru = new ReportUtility();
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                    string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                    #region DataSet

                    objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                    objRpt.SelectedPlant(identity.PlantId, out dsFactory);
                    int startRow = 0;
                    xlsRow = 5;
                    SetHeadText("Emp Status", sheet, xlsRow, ref xlsCol, out colEmployeeStatus, 40);
                    SetHeadText("Employee Category", sheet, xlsRow, ref xlsCol, out colEmployeeCategory, 40);
                    SetHeadText("Total Employee", sheet, xlsRow, ref xlsCol, out colTotalEmployee, 35);
                    SetHeadText("Total Gross", sheet, xlsRow, ref xlsCol, out colTotalGross, 35);
                    SetHeadText("Total Advance", sheet, xlsRow, ref xlsCol, out colTotalAdv, 35);
                    SetHeadText("Total NetPay", sheet, xlsRow, ref xlsCol, out colTotalNetPay, 35);
                    SetHeadText("Avg/Person", sheet, xlsRow, ref xlsCol, out colAvg, 35);

                    sheet.Range[xlsRow, 1, xlsRow, colAvg].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(13, 177, 241);
                    sheet.Range[xlsRow, 1, xlsRow, colAvg].CellStyle.Font.Color = ExcelKnownColors.White;

                    xlsRow++;
                    var startRoww = 0;
                    startRoww = xlsRow;
                    foreach (var item in dicStatus.Keys)
                    {
                        DataTable dtTemp = dicStatus[item];
                        DataTable dvTempSummary = dtTemp.DefaultView.ToTable(true, "EmployeeCategorySystemID", "EmpCategoryName");
                        SetCellText(sheet, xlsRow, colEmployeeStatus, item);
                        startRoww = xlsRow;
                        for (int CT = 0; CT < dvTempSummary.Rows.Count; CT++)
                        {//TotalEmp
                            SetCellText(sheet, xlsRow, colEmployeeCategory, dvTempSummary.Rows[CT]["EmpCategoryName"].ToString());
                            double empCount = clsStaticInfo.dbl(dtTemp.Compute("SUM(TotalEmp)", "HeadCategory='Gross' AND EmployeeCategorySystemID='" + dvTempSummary.Rows[CT]["EmployeeCategorySystemID"].ToString() + "'").ToString());
                            SetCellText(sheet, xlsRow, colTotalEmployee, clsStaticInfo.dbl(empCount.ToString()));
                            double TotalGross = clsStaticInfo.dbl(dtTemp.Compute("SUM(DisbusmentAmount)", "HeadCategory='TOTAL GROSS' AND EmployeeCategorySystemID='" + dvTempSummary.Rows[CT]["EmployeeCategorySystemID"].ToString() + "'").ToString());
                            SetCellText(sheet, xlsRow, colTotalGross, TotalGross);
                            double TotalAdvance = clsStaticInfo.dbl(dtTemp.Compute("SUM(DisbusmentAmount)", "SalaryHead='Advance' AND EmployeeCategorySystemID='" + dvTempSummary.Rows[CT]["EmployeeCategorySystemID"].ToString() + "'").ToString());
                            SetCellText(sheet, xlsRow, colTotalAdv, TotalAdvance);
                            double TotalNetPay = clsStaticInfo.dbl(dtTemp.Compute("SUM(DisbusmentAmount)", "HeadCategory='Net Payable' AND EmployeeCategorySystemID='" + dvTempSummary.Rows[CT]["EmployeeCategorySystemID"].ToString() + "'").ToString());
                            SetCellText(sheet, xlsRow, colTotalNetPay, TotalNetPay);

                            double TotAvg = TotalNetPay / empCount;
                            SetCellText(sheet, xlsRow, colAvg, TotAvg);


                            totalEmployee += empCount;
                            totalGross += TotalGross;
                            totalAdv += TotalAdvance;
                            totalNetPayable += TotalNetPay;
                            totalAvg += TotAvg;

                            xlsRow++;

                        }
                        sheet.Range[xlsRow, colEmployeeStatus].Text = item + "Total";// "SUM(" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(item.Key)) + catFRow.ToString() + ":" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(item.Key)) + (xlsRow - 1).ToString() + ")";

                        sheet.Range[xlsRow, colTotalEmployee].Formula = "SUM(" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(colTotalEmployee.ToString())) + startRoww.ToString() + ":" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(colTotalEmployee.ToString())) + (xlsRow - 1).ToString() + ")";

                        sheet.Range[xlsRow, colTotalGross].Formula = "SUM(" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(colTotalGross.ToString())) + startRoww.ToString() + ":" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(colTotalGross.ToString())) + (xlsRow - 1).ToString() + ")";
                        sheet.Range[xlsRow, colTotalAdv].Formula = "SUM(" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(colTotalAdv.ToString())) + startRoww.ToString() + ":" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(colTotalAdv.ToString())) + (xlsRow - 1).ToString() + ")";
                        sheet.Range[xlsRow, colTotalNetPay].Formula = "SUM(" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(colTotalNetPay.ToString())) + startRoww.ToString() + ":" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(colTotalNetPay.ToString())) + (xlsRow - 1).ToString() + ")";
                        sheet.Range[xlsRow, colAvg].Formula = "SUM(" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(colTotalGross.ToString())) + xlsRow.ToString() + "/" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(colTotalEmployee.ToString())) + (xlsRow).ToString() + ")";

                        sheet.Range[xlsRow, 1, xlsRow, colAvg].CellStyle.Font.Color = ExcelKnownColors.White;
                        sheet.Range[xlsRow, colTotalEmployee, xlsRow, colAvg].NumberFormat = NumberFormatString;
                        sheet.Range[xlsRow, 1, xlsRow, colAvg].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(13, 177, 241);

                        xlsRow++;
                    }
                    sheet.Range[xlsRow, colEmployeeStatus].Text = "Grand Total";// "SUM(" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(item.Key)) + catFRow.ToString() + ":" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(item.Key)) + (xlsRow - 1).ToString() + ")";

                    sheet.Range[xlsRow, colTotalEmployee].Number = totalEmployee;//"SUM(" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(colTotalEmployee.ToString())) + startRoww.ToString() + ":" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(colTotalEmployee.ToString())) + (xlsRow - 1).ToString() + ")";

                    sheet.Range[xlsRow, colTotalGross].Number = totalGross; //"SUM(" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(colTotalGross.ToString())) + startRoww.ToString() + ":" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(colTotalGross.ToString())) + (xlsRow - 1).ToString() + ")";
                    sheet.Range[xlsRow, colTotalAdv].Number = totalAdv;
                    sheet.Range[xlsRow, colTotalNetPay].Number = totalNetPayable; //"SUM(" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(colTotalNetPay.ToString())) + startRoww.ToString() + ":" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(colTotalNetPay.ToString())) + (xlsRow - 1).ToString() + ")";
                    sheet.Range[xlsRow, colAvg].Formula = "SUM(" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(colTotalGross.ToString())) + xlsRow.ToString() + "/" + clsStaticInfo.GetxlsCol((int)clsStaticInfo.dbl(colTotalEmployee.ToString())) + (xlsRow).ToString() + ")"; ;
                    //sheet.Range[xlsRow, 1, xlsRow, colAvg].CellStyle.Font.Color = ExcelKnownColors.White;

                    sheet.Range[xlsRow, 1, xlsRow, colAvg].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(254, 191, 0);
                    sheet.Range[xlsRow, colTotalEmployee, xlsRow, colAvg].NumberFormat = NumberFormatString;



                    int RowIndex = xlsRow + 3;
                    #region ******************Report Header******************
                    xlsRow = 1;
                    xlsCol = 1;
                    //string FactoryAddress = string.Empty;

                    //if (dsCmp.Tables[0].Rows.Count > 0)
                    //{
                    //    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    //}
                    //else
                    //{
                    //    CmpName = "";
                    //}
                    //sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 30;
                    //sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    //xlsRow += 1;
                    //if (dsCmp.Tables[0].Rows.Count > 0)
                    //{
                    //    //FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    //    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                    //}
                    //else
                    //{
                    //    FactoryName = "";
                    //}
                    //sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    //sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    //xlsRow += 1;
                    //sheet1.Range[xlsRow, xlsCol].Text = "Salary Top Sheet";
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    //sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    endXlsCol = colAvg;
                    //strRptDateRange = sheetHeader;
                    sheet.Range[xlsRow, xlsCol].Text = cmpName + " " + "(" + FactoryName + ")";
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 35;
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;


                    sheet.Range[xlsRow, xlsCol].Text = sheetHeader;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 35;
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************                  

                    #region Line Setup
                    if (RowIndex >= (xlsRow - 1))
                    {
                        xlsRow = RowIndex + 2;
                    }
                    //sheet1.Range[5, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    //sheet1.Range[5, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[5, 1, xlsRow - 1, endXlsCol].WrapText = true;
                    #endregion

                    #region Freeze Panes
                    sheet.UsedRange["A7"].FreezePanes();
                    sheet.FirstVisibleColumn = 1;
                    sheet.FirstVisibleRow = 6;
                    #endregion

                    #region UsedRange Alignment
                    sheet.UsedRange.WrapText = true;
                    sheet.UsedRange.CellStyle.Font.Size = 20;
                    sheet.UsedRange.CellStyle.Font.FontName = "ArialNarrow";
                    sheet.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet.PageSetup.TopMargin = 0.5;
                    sheet.PageSetup.BottomMargin = 0.7;
                    sheet.PageSetup.PrintTitleRows = "$1:$7";
                    sheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + (string)Session["USER"] + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet.PageSetup.LeftMargin = 0.5;
                    sheet.PageSetup.RightMargin = 0.2;
                    sheet.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet.PageSetup.IsFitToPage = true;
                    sheet.PageSetup.FitToPagesTall = 1;
                    sheet.PageSetup.Zoom = 55;


                    //sheet1.PageSetup. = 0;

                    sheet.PageSetup.FitToPagesWide = 1;
                    sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet.Name = "TopSheetSummary";
                    #endregion
                    #endregion

                }
                catch (Exception)
                {
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
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


                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "Net Payable".ToUpper()
                            && (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == "GROSS".ToUpper() || Convert.ToBoolean(dtSalaryHead.Rows[ci]["IsGrossComponent"]) == false)
                            )
                        {
                            _total_head_count++;


                            sheet1.Range[xlsRow, ColGrs + countCTCPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();

                            sheet1.Range[xlsRow, ColGrs + countCTCPosition].CellStyle.Font.FontName = "Arial Narrow";
                            sheet1.Range[xlsRow, ColGrs + countCTCPosition].CellStyle.Font.Size = 10;
                            //sheet1.Range[xlsRo 1, ColGrs + countCTCPosition, xlsRow + 1, ColGrs + countCTCPosition + 1].Merge();
                            sheet1.Range[xlsRow, ColGrs + countCTCPosition].CellStyle.ShrinkToFit = true;


                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                            {
                                sheet1.Range[xlsRow, ColGrs + countCTCPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
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


                _count_earning_head = countCTCPosition - 1;

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

                            sheet1.Range[xlsRow, ColGrs + countDeductionPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            sheet1.Range[xlsRow, ColGrs + countDeductionPosition].CellStyle.Font.Size = 10;
                            sheet1.Range[xlsRow, ColGrs + countDeductionPosition].CellStyle.Font.FontName = "Arial Narrow";
                            //sheet1.Range[xlsRo,1, ColGrs + countDeductionPosition, xlsRow + 1, ColGrs + countDeductionPosition + 1].Merge();
                            sheet1.Range[xlsRow, ColGrs + countDeductionPosition].CellStyle.ShrinkToFit = true;


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

        string GetDecimalFormat(bool integerInDisbusment, string decimalNo)
        {
            try
            {
                var ob = new ReportUtility();
                if (integerInDisbusment)
                {
                    return ob.NumberFormatInt();
                }
                else
                {
                    return ob.GetDynamicDecimalPlace(Convert.ToInt32(decimalNo));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult XlsSalaryTopSheetUpdated(string year, string month, string salaryProcessId)
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
                //if (dtEmployees.Rows.Count == 0)
                //{
                //    Exception ex = new Exception("No Data found...");
                //    throw (ex);
                //}
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

                    SetHeadText("Entity", sheet1, xlsRow, ref xlsCol, out ColFirstValue, 17);
                    SetHeadText("Department", sheet1, xlsRow, ref xlsCol, out ColSecondValue, 23);
                    SetHeadText("Subsection", sheet1, xlsRow, ref xlsCol, out ColThirdValue, 23);
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
                    CreateDynamicSHeadTopSheetUpdate(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColThirdValue, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out list);

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
                        sheet1.Range[xlsRow, np].Text = "Net Payable";
                        sheet1.Range[xlsRow, np].ColumnWidth = 14;

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
                                    dvBody.RowFilter = "SalaryHeadID='" + hId + "'and EntityName = '" + x + "' and DepartmentName = '" + y + "' and SubSectionName='" + z + "'";

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
                        var ctcFormula = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.SalaryHead).FirstOrDefault();


                        var grossAdd = oRU.SetFormula(grossIndex.ToString(), xlsRow);
                        var dedAdd = oRU.SetFormula(dedFormula, xlsRow);
                        var ctcAdd = oRU.SetFormula(ctcFormula, xlsRow);


                        sheet1.Range[xlsRow, np].Formula = "=" + dedAdd;
                        sheet1.Range[xlsRow, np].NumberFormat = oRU.NumberFormatDecimalTwo();
                        sheet1.Range[xlsRow, np].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, np].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, np + 1].Formula = "=" + ctcAdd + "-(" + dedAdd + ")";
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
                    sheet1.Range[xlsRow, 1, xlsRow, 3].CellStyle.Font.Bold = true;
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

        private void CreateDynamicSHeadTopSheetUpdate(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out List<SalaryHeadSequence> list)
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
                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region loop gross e
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E")
                        {
                            _total_head_count++;
                            countGrossPostion++;
                            sheet1.Range[xlsRow, ColGrs + countGrossPostion].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            sheet1.Range[xlsRow, ColGrs + countGrossPostion].CellStyle.Font.Size = 24;
                            sheet1.Range[xlsRow, ColGrs + countGrossPostion].CellStyle.ShrinkToFit = true;

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
                        //    }//IsGrossComponent
                        //}//CTC/Gross
                    }//SalaryHead 
                    #endregion
                }//for
                xlsCol += 1;
                _count_earning_head++;
                int countCTCPosition = countGrossPostion;

                xlsCol += 1;

                countCTCPosition++;

                int countDeductionPosition = countCTCPosition;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region deduction
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
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
                    }//SalaryHead 
                    #endregion
                }//for            
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

        [HttpPost, Authorize]
        public ActionResult XlsSalarySummary(string year, string month, string salaryProcessId, string joblocationId)
        {
            #region Variable

            clsReport objRpt = null;

            DataTable dtSalarySummary = null;
            DataTable dtPaymentModeSalarySummary = null;
            DataView dvEarnedSalarySummary = null;
            DataTable dtEarnedSalarySummary = null;
            DataView dvDeductionSalarySummary = null;
            DataTable dtDeductionSalarySummary = null;
            DataTable dtProvidentFundSummay = null;
            DataTable dtESICSummary = null;
            DataTable dtTotalEmployee = null;
            DataTable dtSalarySummaryVPF = null;


            DataSet dsCmp = null;
            DataSet dsFactory = null;
            clsStaticInfo objs = null;


            var earningHeadXlsCol = 0;
            var earningValueXlsCol = 0;
            var deductionHeadXlsCol = 0;
            var deductionValueXlsCol = 0;

            var paymentModeHeadXlsCol = 0;
            var paymentModeValueXlsCol = 0;


            var totalEarning = 0.00;
            var totalDeduction = 0.00;
            var totalNetPayable = 0.00;

            var totalNetPayableCash = 0.00;
            var totalNetPayableCheck = 0.00;
            var totalNetPayableBank = 0.00;
            var totalNetPayableTransfer = 0.00;

            var salaryHeadStartRow = 0;
            var salaryHeadEarningEndRow = 0;
            var salaryHeadDeductionEndRow = 0;
            var paymentModeEndRow = 0;

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
                var totalVPF = 0.00;

                #endregion Variable

                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                #region DataSet

                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);


                dtSalarySummary = GetSalarySummarySql(identity.PlantId, year, month, para.FromDate, para.ToDate, salaryProcessId, "", "", "", "", joblocationId);
                dtPaymentModeSalarySummary = GetSalarySummarySql(identity.PlantId, year, month, para.FromDate, para.ToDate, salaryProcessId, "PaymentMode", "", "", "", joblocationId);
                dtProvidentFundSummay = GetPFandESICSummarySql(identity.CompanyId, identity.PlantId, year, month, para.FromDate, para.ToDate, salaryProcessId, SalaryHeadEnum.PF.ToString(), "", "", joblocationId);
                dtESICSummary = GetPFandESICSummarySql(identity.CompanyId, identity.PlantId, year, month, para.FromDate, para.ToDate, salaryProcessId, SalaryHeadEnum.ESIC.ToString(), "", "", joblocationId);
                dtTotalEmployee = GetTotalEmployeeAsPerSalaryPorcessSql(identity.PlantId, year, month, para.FromDate, para.ToDate, salaryProcessId, SalaryHeadEnum.ESIC.ToString(), "", "", joblocationId);
                dtSalarySummaryVPF = GetSalarySummarySql(identity.PlantId, year, month, para.FromDate, para.ToDate, salaryProcessId, "", "PF Voluntary", "", "", joblocationId);
                var totalEmployee = 0.00;
                if (dtSalarySummaryVPF.Rows.Count > 0)
                {
                    totalVPF = clsStaticInfo.dbl(dtSalarySummaryVPF.Rows[0]["DisbusmentAmount"].ToString());
                }

                #endregion DataSet

                if (dtSalarySummary.Rows.Count == 0)
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }
                else
                {
                    #region------------------Column Header------------------
                    xlsRow = 5;
                    xlsCol = 1;
                    xlsRow++;
                    totalEmployee = clsStaticInfo.dbl(dtTotalEmployee.Compute("SUM(TotalEmployee)", "").ToString());
                    sheet1.Range[xlsRow, xlsCol].Text = "Number of Employees :" + totalEmployee.ToString();
                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();
                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].RowHeight = 34.50;

                    sheet1.Range[xlsRow, xlsCol + 3].Text = "Active Employees :" + clsStaticInfo.dbl(dtTotalEmployee.Compute("SUM(TotalEmployee)", "EmployeeStatus = 'Active'").ToString()).ToString();
                    sheet1.Range[xlsRow, xlsCol + 3, xlsRow, xlsCol + 5].Merge();

                    sheet1.Range[xlsRow, xlsCol + 6].Text = "Separted Employees :" + clsStaticInfo.dbl(dtTotalEmployee.Compute("SUM(TotalEmployee)", "EmployeeStatus = 'Separated'").ToString()).ToString();
                    sheet1.Range[xlsRow, xlsCol + 6, xlsRow, xlsCol + 7].Merge();
                    sheet1.Range[xlsRow, xlsCol].RowHeight = 60;

                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol + 7].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol + 7].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;


                    xlsRow++;
                    salaryHeadStartRow = xlsRow;
                    #endregion
                    #region Earning Heads
                    using (dvEarnedSalarySummary = new DataView(dtSalarySummary)
                    {
                        RowFilter = "HeadType = 'E' AND PartOfNetPay = 1 "
                    })
                    {
                        earningHeadXlsCol = xlsCol;
                        earningValueXlsCol = xlsCol + 2;
                        sheet1.Range[xlsRow, earningHeadXlsCol].Text = "Earning";
                        sheet1[xlsRow, earningHeadXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1[xlsRow, earningHeadXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, earningHeadXlsCol, xlsRow, earningValueXlsCol].Merge();
                        sheet1.Range[xlsRow, earningHeadXlsCol].CellStyle.Font.Bold = true;
                        sheet1.Range[xlsRow, earningHeadXlsCol].RowHeight = 40.50;

                        dtEarnedSalarySummary = dvEarnedSalarySummary.ToTable();
                        for (int i = 0; i < dtEarnedSalarySummary.Rows.Count; i++)
                        {
                            xlsRow++;
                            sheet1.Range[xlsRow, earningHeadXlsCol].Text = dtEarnedSalarySummary.Rows[i]["SalaryHead"].ToString() + " :";
                            sheet1.Range[xlsRow, earningHeadXlsCol, xlsRow, earningHeadXlsCol + 1].Merge();
                            sheet1.Range[xlsRow, earningHeadXlsCol].RowHeight = 40.50;

                            sheet1.Range[xlsRow, earningValueXlsCol].Number = Convert.ToDouble(dtEarnedSalarySummary.Rows[i]["DisbusmentAmount"].ToString());
                            //sheet1.Range[xlsRow, earningValueXlsCol].ColumnWidth = 20;
                            sheet1.Range[xlsRow, earningValueXlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                            sheet1.Range[xlsRow, earningValueXlsCol].IndentLevel = 1;

                        }
                        totalEarning = clsStaticInfo.dbl(dtEarnedSalarySummary.Compute("SUM(DisbusmentAmount)", "").ToString());
                        xlsRow++;
                        salaryHeadEarningEndRow = xlsRow;
                    }
                    #endregion
                    #region Deduction heads
                    using (dvDeductionSalarySummary = new DataView(dtSalarySummary)
                    {
                        RowFilter = "HeadType = 'D'"
                    })
                    {
                        xlsRow = salaryHeadStartRow;
                        deductionHeadXlsCol = earningValueXlsCol + 1;
                        deductionValueXlsCol = deductionHeadXlsCol + 2;
                        sheet1.Range[xlsRow, deductionHeadXlsCol].Text = "Deduction";
                        sheet1.Range[xlsRow, deductionHeadXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, deductionHeadXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, deductionHeadXlsCol, xlsRow, deductionValueXlsCol].Merge();
                        sheet1.Range[xlsRow, deductionHeadXlsCol].CellStyle.Font.Bold = true;
                        sheet1.Range[xlsRow, deductionHeadXlsCol].RowHeight = 34.50;


                        dtDeductionSalarySummary = dvDeductionSalarySummary.ToTable();
                        for (int i = 0; i < dtDeductionSalarySummary.Rows.Count; i++)
                        {
                            xlsRow++;
                            sheet1.Range[xlsRow, deductionHeadXlsCol].Text = dtDeductionSalarySummary.Rows[i]["SalaryHead"].ToString() + " :";
                            sheet1.Range[xlsRow, deductionHeadXlsCol, xlsRow, deductionHeadXlsCol + 1].Merge();
                            sheet1.Range[xlsRow, deductionValueXlsCol].Number = clsStaticInfo.dbl(dtDeductionSalarySummary.Rows[i]["DisbusmentAmount"].ToString()) * -1;
                            sheet1.Range[xlsRow, deductionValueXlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                            sheet1.Range[xlsRow, deductionValueXlsCol].IndentLevel = 1;
                        }
                        xlsRow++;
                        salaryHeadDeductionEndRow = xlsRow;
                        totalDeduction = clsStaticInfo.dbl(dtDeductionSalarySummary.Compute("SUM(DisbusmentAmount)", "").ToString()) * -1;
                    }
                    #endregion

                    #region Payment Mode wise break down
                    xlsRow = salaryHeadStartRow;
                    paymentModeHeadXlsCol = deductionValueXlsCol + 1;
                    paymentModeValueXlsCol = paymentModeHeadXlsCol + 1;
                    sheet1.Range[xlsRow, paymentModeHeadXlsCol].Text = "Already paid :";
                    sheet1.Range[xlsRow, paymentModeHeadXlsCol].RowHeight = 34.50;

                    var paymentModeStartRow = 0;
                    paymentModeStartRow = xlsRow;
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].Number = 0.00;
                    using (dvEarnedSalarySummary = new DataView(dtSalarySummary)
                    {
                        RowFilter = "HeadType = 'E' AND  HeadCategory = 'Net Payable' "
                    })
                    {
                        totalNetPayable = clsStaticInfo.dbl(dvEarnedSalarySummary.ToTable().Compute("SUM(DisbusmentAmount)", "").ToString());

                        xlsRow++;
                        sheet1.Range[xlsRow, paymentModeHeadXlsCol].Text = "Payable Amount :";
                        sheet1.Range[xlsRow, paymentModeValueXlsCol].Number = totalNetPayable;
                        sheet1.Range[xlsRow, paymentModeValueXlsCol].RowHeight = 34.50;


                    }

                    #region Payment Modes

                    xlsRow++;
                    totalNetPayableCash = clsStaticInfo.dbl(dtPaymentModeSalarySummary.Compute("SUM(DisbusmentAmount)", "PaymentMode = 'Cash' AND HeadCategory = 'Net Payable' ").ToString());
                    sheet1.Range[xlsRow, paymentModeHeadXlsCol].Text = "By Cash :";
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].Number = totalNetPayableCash;
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].RowHeight = 34.50;

                    xlsRow++;
                    totalNetPayableCheck = clsStaticInfo.dbl(dtPaymentModeSalarySummary.Compute("SUM(DisbusmentAmount)", "PaymentMode = 'Check' AND HeadCategory = 'Net Payable' ").ToString());
                    sheet1.Range[xlsRow, paymentModeHeadXlsCol].Text = "By Check :";
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].Number = totalNetPayableCheck;
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].RowHeight = 34.50;

                    xlsRow++;
                    totalNetPayableBank = clsStaticInfo.dbl(dtPaymentModeSalarySummary.Compute("SUM(DisbusmentAmount)", "PaymentMode = 'Bank' AND HeadCategory = 'Net Payable' ").ToString());
                    sheet1.Range[xlsRow, paymentModeHeadXlsCol].Text = "By Bank :";
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].Number = totalNetPayableBank;
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].RowHeight = 34.50;

                    xlsRow++;
                    totalNetPayableTransfer = clsStaticInfo.dbl(dtPaymentModeSalarySummary.Compute("SUM(DisbusmentAmount)", "PaymentMode = 'Transfer' AND HeadCategory = 'Net Payable' ").ToString());
                    sheet1.Range[xlsRow, paymentModeHeadXlsCol].Text = "By Transfer :";
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].Number = totalNetPayableTransfer;
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].RowHeight = 34.50;

                    xlsRow++;
                    sheet1.Range[xlsRow, paymentModeHeadXlsCol].Text = "Mode of Payment";
                    sheet1.Range[xlsRow, paymentModeHeadXlsCol, xlsRow, paymentModeValueXlsCol].Merge();
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].RowHeight = 34.50;
                    sheet1.Range[xlsRow, paymentModeHeadXlsCol, xlsRow, paymentModeValueXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    xlsRow++;
                    totalNetPayableCash = clsStaticInfo.dbl(dtPaymentModeSalarySummary.Compute("SUM(DisbusmentAmount)", "PaymentMode = 'Cash'  AND HeadCategory = 'Net Payable' ").ToString());
                    sheet1.Range[xlsRow, paymentModeHeadXlsCol].Text = "By Cash :";
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].Number = totalNetPayableCash;
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].RowHeight = 34.50;

                    xlsRow++;
                    totalNetPayableCheck = clsStaticInfo.dbl(dtPaymentModeSalarySummary.Compute("SUM(DisbusmentAmount)", "PaymentMode = 'Check' AND HeadCategory = 'Net Payable' ").ToString());
                    sheet1.Range[xlsRow, paymentModeHeadXlsCol].Text = "By Check :";
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].Number = totalNetPayableCheck;
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].RowHeight = 34.50;

                    xlsRow++;
                    totalNetPayableBank = clsStaticInfo.dbl(dtPaymentModeSalarySummary.Compute("SUM(DisbusmentAmount)", "PaymentMode = 'Bank' AND HeadCategory = 'Net Payable' ").ToString());
                    sheet1.Range[xlsRow, paymentModeHeadXlsCol].Text = "By Bank :";
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].Number = totalNetPayableBank;
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].RowHeight = 34.50;

                    xlsRow++;
                    paymentModeEndRow = xlsRow;
                    totalNetPayableTransfer = clsStaticInfo.dbl(dtPaymentModeSalarySummary.Compute("SUM(DisbusmentAmount)", "PaymentMode = 'Transfer' AND HeadCategory = 'Net Payable' ").ToString());
                    sheet1.Range[xlsRow, paymentModeHeadXlsCol].Text = "By Transfer :";
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].Number = totalNetPayableTransfer;
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].RowHeight = 34.50;

                    sheet1.Range[paymentModeStartRow, paymentModeValueXlsCol, xlsRow, paymentModeValueXlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[paymentModeStartRow, paymentModeValueXlsCol, xlsRow, paymentModeValueXlsCol].IndentLevel = 1;

                    #endregion


                    #endregion


                    xlsRow = getLargestAmongThree(salaryHeadEarningEndRow, salaryHeadDeductionEndRow, paymentModeEndRow);//Placing Total Earning, Total Deduction, Total NetPayable 
                    xlsRow++;
                    sheet1.Range[xlsRow, earningHeadXlsCol].Text = "Total Earning :";
                    sheet1.Range[xlsRow, earningHeadXlsCol, xlsRow, earningHeadXlsCol + 1].Merge();
                    sheet1.Range[xlsRow, earningHeadXlsCol].RowHeight = 40.50;

                    sheet1.Range[xlsRow, earningValueXlsCol].Number = totalEarning;
                    sheet1.Range[xlsRow, deductionHeadXlsCol].Text = "Total Deduction :";
                    sheet1.Range[xlsRow, deductionHeadXlsCol, xlsRow, deductionHeadXlsCol + 1].Merge();


                    sheet1.Range[xlsRow, deductionValueXlsCol].Number = totalDeduction;
                    sheet1.Range[xlsRow, paymentModeHeadXlsCol].Text = "Net Pay :";
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].Number = totalNetPayable;
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol + 7].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol + 7].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol + 7].IndentLevel = 1;


                    sheet1.Range[salaryHeadStartRow, xlsCol, xlsRow, deductionValueXlsCol].CellStyle.Font.Bold = true;

                    sheet1.Range[xlsRow, earningHeadXlsCol, xlsRow, paymentModeValueXlsCol].CellStyle.Font.Bold = true;

                    sheet1.Range[xlsRow, earningHeadXlsCol].ColumnWidth = 52;
                    sheet1.Range[xlsRow, earningHeadXlsCol + 1].ColumnWidth = 30;
                    sheet1.Range[xlsRow, deductionHeadXlsCol].ColumnWidth = 30;
                    sheet1.Range[xlsRow, deductionHeadXlsCol + 1].ColumnWidth = 30;

                    sheet1.Range[xlsRow, deductionValueXlsCol].ColumnWidth = 20;
                    sheet1.Range[xlsRow, earningValueXlsCol].ColumnWidth = 20;
                    sheet1.Range[xlsRow, paymentModeValueXlsCol].ColumnWidth = 20;
                    sheet1.Range[xlsRow, paymentModeHeadXlsCol].ColumnWidth = 33;

                    #region PF Summary
                    var colNoOfEmpPF = 0;
                    var colNoOfEmpEPS = 0;
                    var colWagesPF = 0;
                    var colWagesEPS = 0;
                    var colEmployeeShare = 0;
                    var colEmployeeVPF = 0;
                    var colEmployeeSharePF = 0;
                    var colEmployeeShareEPS = 0;

                    if (dtProvidentFundSummay.Rows.Count > 0)
                    {
                        xlsRow++;
                        xlsRow++;


                        xlsCol = 1;
                        sheet1.Range[xlsRow, xlsCol].Text = "No. of Employees PF";
                        colNoOfEmpPF = xlsCol;
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = "No. of Employees EPS";
                        colNoOfEmpEPS = xlsCol;
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = "Wages PF";
                        colWagesPF = xlsCol;
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = "Wages EPS";
                        colWagesEPS = xlsCol;
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = "Employee Share";
                        colEmployeeShare = xlsCol;
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = "VPF";
                        colEmployeeVPF = xlsCol;
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = "Employer Share PF";
                        colEmployeeSharePF = xlsCol;
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = "Employer Share EPS";
                        colEmployeeShareEPS = xlsCol;
                        sheet1.Range[xlsRow, colNoOfEmpPF, xlsRow, colEmployeeShareEPS].CellStyle.Font.Bold = true;
                        sheet1.Range[xlsRow, colNoOfEmpPF].RowHeight = 80;


                        sheet1.Range[xlsRow - 1, colNoOfEmpPF].Text = "PF SUMMARY";
                        sheet1.Range[xlsRow - 1, colNoOfEmpPF].RowHeight = 60;
                        sheet1.Range[xlsRow - 1, colNoOfEmpPF, xlsRow - 1, colEmployeeShareEPS].Merge();
                        sheet1.Range[xlsRow - 1, colNoOfEmpPF, xlsRow - 1, colEmployeeShareEPS].CellStyle.Font.Bold = true;
                        sheet1.Range[xlsRow - 1, colNoOfEmpPF, xlsRow - 1, colEmployeeShareEPS].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, colNoOfEmpPF, xlsRow - 1, colEmployeeShareEPS].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                        var pfSummaryRow = xlsRow;
                        xlsRow++;
                        sheet1.Range[xlsRow, colNoOfEmpPF].Number = clsStaticInfo.dbl(dtProvidentFundSummay.Compute("Max(TotalEmployee)", "").ToString());
                        sheet1.Range[xlsRow, colNoOfEmpEPS].Number = clsStaticInfo.dbl(dtProvidentFundSummay.Compute("Max(TotalEmployee)", "").ToString());
                        sheet1.Range[xlsRow, colWagesPF].Number = clsStaticInfo.dbl(dtProvidentFundSummay.Compute("SUM(DisbusmentAmount)", "Cat = 'B'").ToString());

                        sheet1.Range[xlsRow, colWagesEPS].Number = GetPositiveValues(clsStaticInfo.dbl(dtProvidentFundSummay.Rows[0]["EPS"].ToString()));
                        sheet1.Range[xlsRow, colEmployeeShare].Number = GetPositiveValues(clsStaticInfo.dbl(dtProvidentFundSummay.Compute("SUM(DisbusmentAmount)", "Cat = 'EE'").ToString()));//EE
                        sheet1.Range[xlsRow, colEmployeeVPF].Number = GetPositiveValues(totalVPF);//VPF
                        sheet1.Range[xlsRow, colEmployeeSharePF].Number = GetPositiveValues(clsStaticInfo.dbl(dtProvidentFundSummay.Compute("SUM(DisbusmentAmount)", "Cat = 'ER'").ToString()));////EE
                        sheet1.Range[xlsRow, colEmployeeShareEPS].Number = GetPositiveValues(clsStaticInfo.dbl(dtProvidentFundSummay.Compute("SUM(DisbusmentAmount)", "Cat = 'P'").ToString()));
                        sheet1.Range[xlsRow, colNoOfEmpPF, xlsRow, colEmployeeShareEPS].CellStyle.Font.Bold = true;
                        sheet1.Range[xlsRow, colWagesPF].RowHeight = 40;
                        xlsRow++;
                        var pfWageBrDownRow = xlsRow;



                        sheet1.Range[xlsRow, colNoOfEmpPF].Text = "No. of Employees :";
                        sheet1.Range[xlsRow, colNoOfEmpEPS].Number = totalEmployee;
                        sheet1.Range[xlsRow, 1].RowHeight = 40;
                        xlsRow++;
                        sheet1.Range[xlsRow, colNoOfEmpPF].Text = "No. of PF Contributors :";
                        sheet1.Range[xlsRow, colNoOfEmpEPS].Number = clsStaticInfo.dbl(dtProvidentFundSummay.Compute("Max(TotalEmployee)", "").ToString());
                        sheet1.Range[xlsRow, 1].RowHeight = 40;
                        xlsRow++;
                        sheet1.Range[xlsRow, colNoOfEmpPF].Text = "No. of Non PF Contributors :";
                        sheet1.Range[xlsRow, colNoOfEmpEPS].Number = clsStaticInfo.dbl(dtSalarySummary.Compute("Max(TotalEmployee)", "").ToString()) - clsStaticInfo.dbl(dtProvidentFundSummay.Compute("Max(TotalEmployee)", "").ToString());
                        sheet1.Range[xlsRow, 1].RowHeight = 40;
                        xlsRow++;
                        sheet1.Range[xlsRow, 1].RowHeight = 40;
                        sheet1.Range[xlsRow, colNoOfEmpEPS].Text = "Wages";
                        sheet1.Range[xlsRow, colEmployeeShare].Text = "Employee";
                        sheet1.Range[xlsRow, colEmployeeShare, xlsRow, colEmployeeShare + 1].Merge();

                        //sheet1.Range[xlsRow, colEmployeeShare].Text = "Employee";


                        xlsRow++;
                        sheet1.Range[xlsRow, 1].RowHeight = 40;
                        sheet1.Range[xlsRow, colNoOfEmpPF].Text = "PF Wages :";
                        sheet1.Range[xlsRow, colNoOfEmpEPS].Number = GetPositiveValues(clsStaticInfo.dbl(dtProvidentFundSummay.Compute("SUM(DisbusmentAmount)", "Cat = 'B'").ToString()));
                        sheet1.Range[xlsRow, colEmployeeShare].Number = GetPositiveValues(clsStaticInfo.dbl(dtProvidentFundSummay.Compute("SUM(DisbusmentAmount)", "Cat = 'EE'").ToString()));
                        sheet1.Range[xlsRow, colEmployeeVPF].Number = GetPositiveValues(clsStaticInfo.dbl(dtProvidentFundSummay.Compute("SUM(DisbusmentAmount)", "Cat = 'VPF'").ToString()));

                        xlsRow++;
                        sheet1.Range[xlsRow, 1].RowHeight = 40;
                        sheet1.Range[xlsRow, colEmployeeShare].Text = "Employer";
                        sheet1.Range[xlsRow, colEmployeeShare, xlsRow, colEmployeeShare + 1].Merge();


                        xlsRow++;
                        sheet1.Range[xlsRow, 1].RowHeight = 40;
                        sheet1.Range[xlsRow, colNoOfEmpPF].Text = "Non Pensionable wages :";
                        sheet1.Range[xlsRow, colNoOfEmpEPS].Number = GetPositiveValues(clsStaticInfo.dbl(dtProvidentFundSummay.Rows[0]["Above15"].ToString()));

                        sheet1.Range[xlsRow, colEmployeeShare].Number = GetPositiveValues(clsStaticInfo.dbl(dtProvidentFundSummay.Compute("SUM(DisbusmentAmount)", "Cat = 'P'").ToString()));
                        //sheet1.Range[xlsRow, colEmployeeShare, xlsRow + 1, colEmployeeShare].Number = GetPositiveValues(clsStaticInfo.dbl(dtProvidentFundSummay.Compute("SUM(DisbusmentAmount)", "Cat = 'P'").ToString()));

                        sheet1.Range[xlsRow, colEmployeeVPF].Number = GetPositiveValues(totalVPF);


                        //sheet1.Range[xlsRow,colWagesPF].Number = clsStaticInfo.dbl(dtProvidentFundSummay.Rows[0]["EPS"].ToString()); 
                        //sheet1.Range[xlsRow, colWagesEPS].Text = "12%";
                        //sheet1.Range[xlsRow, colWagesEPS].CellStyle.Font.Bold = true;
                        xlsRow++;
                        sheet1.Range[xlsRow, 1].RowHeight = 40;
                        sheet1.Range[xlsRow, colNoOfEmpPF].Text = "Pensionable wages :";
                        sheet1.Range[xlsRow, colNoOfEmpEPS].Number = GetPositiveValues(clsStaticInfo.dbl(dtProvidentFundSummay.Rows[0]["EPS"].ToString()));
                        //sheet1.Range[xlsRow, colWagesEPS].Text = "3.67%";
                        //sheet1.Range[xlsRow, colWagesEPS].CellStyle.Font.Bold = true;

                        sheet1.Range[pfWageBrDownRow, colEmployeeSharePF].Text = "CHALLAN DETAILS";
                        sheet1.Range[pfWageBrDownRow, colEmployeeSharePF, pfWageBrDownRow, colEmployeeShareEPS].Merge();

                        sheet1.Range[pfWageBrDownRow, colEmployeeSharePF, pfWageBrDownRow, colEmployeeShareEPS].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[pfWageBrDownRow, colEmployeeSharePF, pfWageBrDownRow, colEmployeeShareEPS].IndentLevel = 1;
                        sheet1.Range[pfWageBrDownRow, colEmployeeSharePF, pfWageBrDownRow, colEmployeeShareEPS].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;


                        pfWageBrDownRow++;
                        var formulaStartRow = pfWageBrDownRow;
                        sheet1.Range[pfWageBrDownRow, colEmployeeSharePF].Text = "For A/C No(01)";
                        sheet1.Range[pfWageBrDownRow, colEmployeeShareEPS].Number = GetPositiveValues(clsStaticInfo.dbl(dtProvidentFundSummay.Compute("SUM(DisbusmentAmount)", "Cat = 'EE'").ToString())) +
                                                                                    GetPositiveValues(clsStaticInfo.dbl(dtProvidentFundSummay.Compute("SUM(DisbusmentAmount)", "Cat = 'ER'").ToString())) +
                                                                                    GetPositiveValues(clsStaticInfo.dbl(dtProvidentFundSummay.Compute("SUM(DisbusmentAmount)", "Cat = 'VPF'").ToString()));
                        pfWageBrDownRow++;
                        sheet1.Range[pfWageBrDownRow, colEmployeeSharePF].Text = "For A/C No(10)";
                        sheet1.Range[pfWageBrDownRow, colEmployeeShareEPS].Number = GetPositiveValues(clsStaticInfo.dbl(dtProvidentFundSummay.Compute("SUM(DisbusmentAmount)", "Cat = 'P'").ToString()));
                        pfWageBrDownRow++;
                        sheet1.Range[pfWageBrDownRow, colEmployeeSharePF].Text = "For A/C No(02)";
                        sheet1.Range[pfWageBrDownRow, colEmployeeShareEPS].Number = Math.Round(GetPositiveValues(clsStaticInfo.dbl(dtProvidentFundSummay.Compute("SUM(DisbusmentAmount)", "Cat = 'B'").ToString())) * 0.5 / 100, MidpointRounding.ToEven);
                        pfWageBrDownRow++;
                        sheet1.Range[pfWageBrDownRow, colEmployeeSharePF].Text = "For A/C No(21)";
                        sheet1.Range[pfWageBrDownRow, colEmployeeShareEPS].Number = Math.Round(GetPositiveValues(clsStaticInfo.dbl(dtProvidentFundSummay.Rows[0]["EPS"].ToString())) * 0.5 / 100, MidpointRounding.ToEven);

                        pfWageBrDownRow++;
                        sheet1.Range[pfWageBrDownRow, colEmployeeSharePF].Text = "Total";
                        sheet1.Range[pfWageBrDownRow, colEmployeeShareEPS].Formula = "=SUM(" + ru.GetColumnNameForXls(colEmployeeShareEPS) + formulaStartRow + ":" + ru.GetColumnNameForXls(colEmployeeShareEPS) + (pfWageBrDownRow - 1) + ")";
                        sheet1.Range[formulaStartRow - 1, colEmployeeSharePF, xlsRow, colEmployeeShareEPS].CellStyle.Font.Bold = true;


                        sheet1.Range[salaryHeadStartRow, 1, xlsRow, xlsCol].CellStyle.Font.Size = 20;
                        sheet1.Range[pfSummaryRow, 1, xlsRow, colEmployeeShareEPS].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[pfSummaryRow, 1, xlsRow, colEmployeeShareEPS].IndentLevel = 1;
                        sheet1.Range[pfSummaryRow, 1, xlsRow, colEmployeeShareEPS].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;

                    }

                    #endregion


                    endXlsCol = xlsCol;

                    #region ESIC Summary

                    if (dtESICSummary.Rows.Count > 0)
                    {
                        xlsRow++;
                        xlsRow++;

                        var colNoOfEmpESI = 0;
                        var colWagesESI = 0;
                        var colEmployeeShareESIC = 0;
                        var colEmployerShareESIC = 0;
                        var colTotal = 0;

                        xlsCol = 1;
                        sheet1.Range[xlsRow, xlsCol].Text = "No. of Employees ESI";
                        colNoOfEmpESI = xlsCol;
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = "ESI Wages";
                        colWagesESI = xlsCol;
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = "Employee Share";
                        colEmployeeShareESIC = xlsCol;
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = "Employer Share";
                        colEmployerShareESIC = xlsCol;
                        xlsCol++;
                        sheet1.Range[xlsRow, xlsCol].Text = "Total";
                        colTotal = xlsCol;
                        sheet1.Range[xlsRow, colNoOfEmpESI, xlsRow, colTotal].CellStyle.Font.Bold = true;


                        sheet1.Range[xlsRow - 1, colNoOfEmpESI].Text = "ESI SUMMARY";
                        sheet1.Range[xlsRow - 1, colNoOfEmpESI, xlsRow - 1, endXlsCol].Merge();
                        sheet1.Range[xlsRow - 1, colNoOfEmpESI, xlsRow - 1, endXlsCol].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, colNoOfEmpESI, xlsRow - 1, endXlsCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, colNoOfEmpESI, xlsRow - 1, endXlsCol].CellStyle.Font.Bold = true;

                        sheet1.Range[xlsRow, colNoOfEmpESI].RowHeight = 80;
                        sheet1.Range[xlsRow - 1, colNoOfEmpESI].RowHeight = 60;


                        xlsRow++;

                        sheet1.Range[xlsRow, colNoOfEmpESI].Number = clsStaticInfo.dbl(dtESICSummary.Compute("Max(TotalEmployee)", "").ToString());
                        sheet1.Range[xlsRow, colWagesESI].Number = clsStaticInfo.dbl(dtESICSummary.Compute("SUM(DisbusmentAmount)", "Cat = 'GROSS'").ToString());
                        sheet1.Range[xlsRow, colEmployeeShareESIC].Number = GetPositiveValues(clsStaticInfo.dbl(dtESICSummary.Compute("SUM(DisbusmentAmount)", "Cat = 'ESICEE'").ToString()));//EE
                        sheet1.Range[xlsRow, colEmployerShareESIC].Number = GetPositiveValues(clsStaticInfo.dbl(dtESICSummary.Compute("SUM(DisbusmentAmount)", "Cat = 'ESICER'").ToString()));//EE
                        sheet1.Range[xlsRow, colTotal].Number = GetPositiveValues(clsStaticInfo.dbl(dtESICSummary.Compute("SUM(DisbusmentAmount)", "Cat = 'ESICEE'").ToString())) + GetPositiveValues(clsStaticInfo.dbl(dtESICSummary.Compute("SUM(DisbusmentAmount)", "Cat = 'ESICER'").ToString()));
                        sheet1.Range[xlsRow, colNoOfEmpESI, xlsRow, colTotal].CellStyle.Font.Bold = true;
                        sheet1.Range[xlsRow, colNoOfEmpESI, xlsRow, colTotal].RowHeight = 40;

                        sheet1.Range[salaryHeadStartRow - 1, 1, xlsRow, colEmployeeShareEPS].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow, 1].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;//(ExcelLineStyle.Hair);

                        sheet1.Range[xlsRow, 2, xlsRow, colEmployeeShareEPS].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;//(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow, 2, xlsRow, colEmployeeShareEPS].IndentLevel = 1;//(ExcelLineStyle.Hair);


                        sheet1.Range[salaryHeadStartRow - 1, 1, xlsRow, colEmployeeShareEPS].BorderInside(ExcelLineStyle.Hair);
                    }
                    #endregion

                    int RowIndex = xlsRow + 3;

                    #region ******************Report Header******************
                    xlsRow = 1;
                    xlsCol = 1;
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

                    #region Line Setup
                    if (RowIndex >= (xlsRow - 1))
                    {
                        xlsRow = RowIndex + 2;
                    }
                    //sheet1.Range[5, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    //sheet1.Range[5, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[5, 1, xlsRow - 1, endXlsCol].WrapText = true;
                    #endregion

                    #region Freeze Panes
                    sheet1.UsedRange["A7"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 20;
                    sheet1.UsedRange.CellStyle.Font.FontName = "ArialNarrow";
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
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet1.PageSetup.IsFitToPage = true;
                    sheet1.PageSetup.FitToPagesTall = 1;

                    //sheet1.PageSetup. = 0;

                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet1.Name = "SalaryTopSheet";
                    #endregion
                    //}
                    //workbook.Version = ExcelVersion.Excel2013;
                    string strFileName = "SalaryTopSheet" + bplib.clsWebLib.DateData_DBToApp(DateTime.Now.Date, bplib.clsWebLib.STD_DATE_FORMAT).ToString("dd-MMM-yyyy") + ".xlsx";

                    //sheet1.Name = "SalaryTopSheet" +month+"-"+year+identity.PlantId;
                    sheet1.Name = "TopSheet";
                    //workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);

                    workbook.Version = ExcelVersion.Excel2016;
                    var converter = new ExcelToPdfConverter(workbook);
                    var pdfDoc = converter.Convert();
                    strFileName = month + "-" + year + "TopSheet" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".pdf";
                    string fullPathPDF = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                    pdfDoc.Save(fullPathPDF);
                    //string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);

                    ////var workbook = _payRegisterBDReportService.EmployeeSalaryRegister(PayRegisterParam, paymentDate, sqlInStatement, withStructure);
                    //workbook.Version = ExcelVersion.Excel97to2003;
                    //workbook.SaveAs(fullPath);

                    return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
                }


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

        public DataTable GetSalarySummarySql(string plantId, string year, string month, string fromDate, string toDate, string salaryProcessId, string paymentMode, string specificSalaryHead, string empStatus, string employeeCategoryId, string jobLocationId)
        {
            try
            {
                string paymentModeStr = "";
                string paymentModeGroupBy = "";
                string specificSalaryHeadWC = "";
                string wcEmpStatus = "";
                string wcEmployeeCategory = "";
                string wcEmployeeCategoryUD = "";
                string strJobLocation = "";
                string wcJobLocation = "";
                if (string.IsNullOrEmpty(specificSalaryHead))
                {
                    specificSalaryHeadWC = "";

                }
                else
                {
                    specificSalaryHeadWC = "AND EmpSlr.HeadCategory = '" + specificSalaryHead + "'";
                }
                if (string.IsNullOrEmpty(jobLocationId))
                {
                    strJobLocation = "";
                    wcJobLocation = "";
                }
                else
                {
                    strJobLocation = ",JoblocationId,JobLocation";
                    wcJobLocation = "AND JoblocationId in(" + jobLocationId + ")";
                }
                if (string.IsNullOrEmpty(paymentMode) == false)
                {
                    paymentModeStr = ",EmpBasic.PaymentMode";
                    paymentModeGroupBy = ",EmpBasic.PaymentMode";
                }
                if (!string.IsNullOrEmpty(employeeCategoryId) && employeeCategoryId != "null")
                {
                    wcEmployeeCategory = @"AND EmpBasic.EmployeeCategorySystemID IN(" + employeeCategoryId + @")";
                    wcEmployeeCategoryUD = @"WHERE 
                                              EmployeeCategorySystemID IN(" + employeeCategoryId + @") ";
                }
                if (empStatus == "Active" || empStatus == "Separated")
                {


                    if (empStatus == "Separated")
                    {
                        wcEmpStatus = @" AND EmpBasic.empStatus = '" + empStatus + @"' and Month(EmpBasic.DOS) = " + month + " and YEAR(EmpBasic.Dos) =  " + year + "";

                    }

                    if (empStatus == "Active")
                    {
                        wcEmpStatus = @" AND  EmpBasic.empStatus = '" + empStatus + @"' OR( EmpBasic.DOS > '" + toDate + "' and EmpBasic.DOJ <=  '" + toDate + "')";

                    }
                }
                if (empStatus == "Maternity")
                {
                    wcEmpStatus = @"AND EmpBasic.SystemID IN (

                                                 SELECT LTR.EmpSystemID from LeaveTransaction LTR

                                                 LEFT JOIN LeaveType LT ON LT.Id = LTR.LTSystemID

                                                  WHERE LT.LeaveType = 'Maternity' AND

                                                ((MONTH(Convert(date, (FromDate) - 1)) = '" + month + @"' and YEar(Convert(date, (FromDate) - 1)) = '" + year + @"')

                                                 OR
                                                 (MONTH(Convert(date, (ToDate) + 1)) = '" + month + @"' and YEar(Convert(date, (ToDate) + 1)) = '" + year + @"')
												 )
												 )";
                }
                string sqlText = @"SELECT      Count(EmpBasic.SystemId) TotalEmployee,SUM(EmpSlr.EntryAmount) StructureAmount,sum(EmpSlr.DisbusmentAmount) DisbusmentAmount
								,EmpSlr.PlantID
                                        " + paymentModeStr + @"
								,EmpSlr.MonthNo, EmpSlr.YearNo
                                , EmpSlr.EntryCurrencyID
                               , EmpSlr.DisbusmentCurrencyID
						

								 ,EmpSlr.AcltExcDisbSlrHDID, EmpSlr.AcltExcDisbSlrHDAmt,
                                EmpSlr.PlantWiseExchangeCR, EmpSlr.ExchangeRate,
								 EmpSlr.AmtDefinitionCurrencyID, EmpSlr.AmtDefinitionCurrency,
                                EmpSlr.AmtDefinitionCurrencyRate
                                ,EmpSlr.SalaryHead,EmpSlr.HeadCategory,
								EmpSlr.HeadType,ISNULL(PSH.Sequence,'99')  Sequence
								 ,CRC.IntegerInDisb,CRC.DecimalNo,EmpSlr.PartOfNetPay
        
                               
                            FROM
                                    (
									 SELECT E.SystemID, E.EmployeeCode, E.EmployeeName, E.EmployeeStatus,E.PaymentMode,JL.SystemID JoblocationId, JL.JobLocation
											, DE.UserName DesignationName,E.EmployeeStatus empStatus
											,'' UserGroupSystemID, E.PlantID, F.UserName PlantName, EN.UnitID
											,FU.UserName UnitName, PR.DivisionID, DV.UserName DivisionName, PR.DepartmentID, DP.UserName DepartmentName
											,PR.SectionID, S.UserName SectionName, PR.SubSectionID, SS.UserName SubSectionName, EC.Id EmployeeCategorySystemID
											,EC.UserName EmpCategoryName--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo
                                            ,egdsgg.GivenDesignationGroup,e.SalaryRuleMasterSystemID,Format(E.DOJ,'dd-MMM-yyyy') DOJ,Format(E.DOS,'dd-MMM-yyyy') DOS
											,ISNULL(LDS.UserName,'') LegalDesignation
                                     FROM EmployeeInformation E
 LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity EN ON MB.EntityId=EN.Id
												LEFT JOIN ORG.Plant F ON E.PlantID = F.Id
												LEFT JOIN hkp.Designation DE ON E.GivenDesignationId = DE.Id
												LEFT JOIN hkp.LegalDesignation LDS ON E.LegalDesignationId = LDS.Id
												LEFT JOIN JobLocation JL ON JL.SystemID = E.JobLocationID

												LEFT JOIN org.Unit FU ON EN.UnitID = FU.Id
												LEFT JOIN org.Division DV ON PR.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON PR.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON PR.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON PR.SubSectionID = SS.Id
												LEFT JOIN
                                                --hkp.EmployeeCategory EC ON E.EmployeeCategorySystemID = EC.Id
                                                (
                                                SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
												LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
												)EC ON EC.DesignationId=E.GivenDesignationId
												LEFT JOIN (SELECT dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									            ,dg.UserName GivenDesignationGroup
									            FROM MST.DesignationMaster dm
									            LEFT JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
									            ) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									            and egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                            " + wcEmployeeCategoryUD + @"
									) EmpBasic
                                    LEFT JOIN 
													(
													 SELECT E.SystemID EmpSystemId, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
														FROM EmployeeInformation E   
																LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
																LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                                                                                                AND E.PlantId = gd.PlantId
																LEFT JOIN (
																			SELECT MAX(EffectiveDate) EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
																				FROM MST.LegalSalaryStructure 
																				WHERE EffectiveDate <= '" + toDate + @"'
																			GROUP BY LegalSalaryGradeId, EmployeeLocationId 
																		  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
																LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                                                                                            AND SS.EmployeeLocationId = S.EmployeeLocationId 
                                                                                            AND SS.EffectiveDate = S.EffectiveDate
																LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 	
                                                                left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId	
														GROUP BY E.SystemId,LSG.UserName
													) MW ON MW.EmpSystemId = EmpBasic.SystemId
                                    INNER JOIN
											(
											 SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
													SPC.EmpInfoSystemID, SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
													SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
													SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
													CRE.Name AS PlantWiseExchangeCR, EXR.ToCurrencyBuying ExchangeRate, SPM.AmtDefinitionCurrencyID,
													CR.Name AS AmtDefinitionCurrency, SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect
                                                    ,sh.SalaryHead,sh.HeadCategory,sh.HeadType,SH.PartOfNetPay
                                                    
											 FROM SalaryProcChild SPC
																INNER JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
																							AND SPM.SystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = '" + month + @"' AND YearNo = '" + year + @"' )
                                                        LEFT JOIN SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
														--LEFT JOIN (select * from [MST].[PlantSalaryHeadSequence] where PlantId='" + plantId + @"' ) psh
																		-- psh.SalaryHeadId=spc.SalaryHeadID

														LEFT JOIN scs.Currency CR ON SPM.AmtDefinitionCurrencyID = CR.Id
														LEFT JOIN (
																   SELECT * FROM ExchangerateDateWiseForHR
																   WHERE FromDate IN (   SELECT MAX(FromDate) FromDate FROM SalaryProcMaster
																															   
																											WHERE SystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = '" + month + @"' AND YearNo = '" + year + @"')
                                                                                    )
																  ) EXR ON SPM.AmtDefinitionCurrencyID = EXR.FromCurrencyCode
																							AND SPC.PlantID = Exr.PlantID
														LEFT JOIN SCS.Currency CRE ON EXR.FromCurrencyCode = CRE.Id
											) EmpSlr ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID AND EmpBasic.PlantID = EmpSlr.PlantID
                             	LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EmpBasic.SalaryRuleMasterSystemID 
										--LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID	AND SRG.SalaryHeadID = EmpSlr.SalaryHeadID									
                                        LEFT JOIN (SELECT * FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId='" + plantId + @"' ) PSH
																		ON PSH.SalaryHeadId=EmpSlr.SalaryHeadID
                                        LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = EmpSlr.SalaryHeadID
                                        
                                    LEFT JOIN
		                                    (
											 SELECT EmpSystemID, MonthNo, YearNo, TotalProcDate, TotalPresent, TotalLate,
													TotalAbsent AbsentDays, TotalLv, TotalMLv, TotalCompAssignLv, TotalWeekOff, TotalHoliDay,
													TotalWeekOffHoliDay, TotalOTHr, TotalNormalOTHr, TotalExtraOTHr
				                              FROM AttdnDataMonthlySummary
											) MMDSA ON EmpSlr.EmpInfoSystemID = MMDSA.EmpSystemID AND EmpSlr.MonthNo = MMDSA.MonthNo AND
						                               EmpSlr.YearNo = MMDSA.YearNo 
                                        WHERE ISNULL(EmpSlr.PlantID,'') != ''  " + wcEmpStatus + @"" + wcEmployeeCategory + @" " + specificSalaryHeadWC + @" " + wcJobLocation + @"--(EmpBasic.EmployeeStatus != 'Separated' OR ISNULL(EmpBasic.DOS,'') = ''  OR COnvert(date,EmpBasic.DOS) >= Convert(Date,'1-September-2019')) AND COnvert(date,EmpBasic.DOJ) <=  Convert(Date,'30-September-2019')


										GROUP BY 	EmpSlr.PlantID--, EmpSlr.FromDate, EmpSlr.ToDate,
								,EmpSlr.MonthNo, EmpSlr.YearNo
                               , EmpSlr.EntryCurrencyID, 
                                EmpSlr.DisbusmentCurrencyID								
								 ,EmpSlr.AcltExcDisbSlrHDID, EmpSlr.AcltExcDisbSlrHDAmt,
                                EmpSlr.PlantWiseExchangeCR, EmpSlr.ExchangeRate,
								 EmpSlr.AmtDefinitionCurrencyID, EmpSlr.AmtDefinitionCurrency,
                                EmpSlr.AmtDefinitionCurrencyRate
                                ,EmpSlr.SalaryHead,EmpSlr.HeadCategory,
								EmpSlr.HeadType,psh.Sequence,EmpSlr.PartOfNetPay
								,CRC.IntegerInDisb,CRC.DecimalNo " + paymentModeGroupBy + @" ORDER BY CONVERT(int, psh.Sequence)";

               return _sqlRepository.GetDataTable(sqlText);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public DataTable GetPFandESICSummarySql(string companyId, string plantId, string year, string month, string fromDate, string toDate, string salaryProcessId, string summaryType, string empStatus, string employeeCategoryId, string jobLocationId)
        {
            try
            {
                string salaryHeads = "";
                string strwcJobLocation = "";
                if (summaryType.ToUpper() == "PF")
                {
                    salaryHeads = @"(SELECT *,'EE' Cat from SalaryHead where HeadCategory in ('PF Employee Contribution'))
									UNION
									(SELECT *,'ER' Cat from SalaryHead where HeadCategory in ('PF Employer Contribution'))
									UNION
									(SELECT *,'B' Cat FROM SalaryHead where HeadCategory in ('Basic'))
									UNION
									(SELECT *,'P' Cat FROM SalaryHead where HeadCategory in ('Pension'))
									UNION
									(SELECT *,'VPF' Cat  FROM SalaryHead  WHERE HeadCategory in ('PF Voluntary'))";
                }
                if (summaryType.ToUpper() == "ESIC")
                {
                    salaryHeads = @"   (SELECT *,'B' Cat FROM SalaryHead where HeadCategory in ('Basic'))
										 UNION														
										SELECT *,'ESICER' FROM SalaryHead WHERE HeadCategory IN ('ESIC Employer Contribution')
										UNION
										SELECT *,'ESICEE' FROM SalaryHead WHERE HeadCategory IN ('ESIC Employee Contribution')
                                        UNION
                                        SELECT *,'GROSS' FROM SalaryHead WHERE HeadCategory = 'GROSS'";
                }
                if (string.IsNullOrEmpty(jobLocationId))
                {
                    strwcJobLocation = "";
                }
                else
                {
                    strwcJobLocation = "Where JL.SystemID IN (" + jobLocationId + @")";
                }

                string sqlText = @"SELECT  EmpSlr.PlantID, EmpSlr.FromDate, EmpSlr.ToDate,
								 EmpSlr.MonthNo, EmpSlr.YearNo,EmpSlr.Cat
                                , EmpSlr.SalaryHead,EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.DisbusmentCurrencyID
								, Count(EmpSlr.EmpInfoSystemID) TotalEmployee
								, SUM(EmpSlr.EntryAmount) EntryAmount
                                , SUM(EmpSlr.DisbusmentAmount) DisbusmentAmount		
								, SUM(CASE WHEN EmpSlr.SalaryHead ='Basic' THEN 
								  (CASE WHEN EmpSlr.EntryAmount >= 15000 THEN (CASE WHEN EmpSlr.DisbusmentAmount <15000 THEN EmpSlr.DisbusmentAmount WHEN EmpSlr.DisbusmentAmount >=15000 THEN 15000 end) when EmpSlr.EntryAmount <= 15000 then EmpSlr.DisbusmentAmount end) end) as EPS
								, SUM(CASE WHEN EmpSlr.SalaryHead ='Basic' THEN 
								  (CASE WHEN EmpSlr.EntryAmount >= 15000 THEN (CASE WHEN EmpSlr.DisbusmentAmount >15000 THEN  EmpSlr.DisbusmentAmount - 15000 ELSE 0 END) ELSE 0 END) END) AS Above15								
                            FROM
                                    (
										 SELECT ISNULL(ED.IsActive,0) IsActive,ISNULL(ED.IsApproved,0) IsApproved,E.SystemID, E.EmployeeCode, E.EmployeeName,E.DOB, E.DOJ,E.DOS, E.EmployeeStatus,ed.DocNumber,DATEDIFF(YY,E.DOB,'30-Sep-2019') As Age,DE.UserName DesignationName,GVDE.UserName GivenDesignationName,
											'' UserGroupSystemID, E.CompanyId, E.PlantID, F.UserName PlantName,
											FU.UserName UnitName,DV.UserName DivisionName, DP.UserName DepartmentName,
											S.UserName SectionName, SS.UserName SubSectionName,
											EC.UserName EmpCategoryName--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo,JL.SystemID JoblocationId, JL.JobLocation
                                            ,egdsgg.GivenDesignationGroup
                                            FROM EmployeeInformation E
LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN [ORG].[Position] AS PO ON PO.Id = MB.PositionId
                                LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId
												LEFT JOIN org.Plant F ON E.PlantID = F.Id
												LEFT JOIN hkp.Designation DE ON E.GivenDesignationId = DE.Id
												LEFT JOIN hkp.Designation GVDE ON E.GivenDesignationId = GVDE.Id
												LEFT JOIN org.Unit FU ON EN.UnitID = FU.Id
												LEFT JOIN org.Division DV ON PO.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON PO.DepartmentID = DP.Id
												LEFT JOIN JobLocation JL ON JL.SystemID = E.JobLocationID

												LEFT JOIN org.Section S ON PO.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON PO.SubSectionID = SS.Id
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
											       Left JOIN (SELECT ED.EmpSystemID,ED.DocNumber,ISNULL(PF.IsActive,0) IsActive,ISNULL(PF.IsApproved,0) IsApproved FROM EmployeeDocument ED Left join
                                                [PFEligibleEmployee] PF ON PF.EmpSystemID = ED.EmpSystemID
                                                 WHERE ComplianceDocumentId = 
												(
												SELECT Id	FROM HKP.ComplianceDocument WHERE ProfileType = 'PF'
												) and PF.StartDate < '" + toDate + @"' AND ISNULL(PF.isActive,0) = 1 and ISNULL(PF.IsApproved,0) = 1) ED on ED.EmpSystemID = E.SystemId
                                                " + strwcJobLocation + @" 
						

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
                                        AND MonthNo = '" + month + @"' AND YearNo = '" + year + @"' )
														 INNER JOIN (--0														
																			" + salaryHeads + @"
																	)--0 
														SH ON SH.SalaryHeadID=SPC.SalaryHeadID
                                                                     ---    INNER JOIN 
                                            --(
                                             -- SELECT EESHE.EmpSystemId,EESHE.SalaryStructureId,EESHE.IsEligible,EESHE.SalaryHeadEnum,SalStruc.SalaryRuleMasterSystemID FROM [EmployeeEligibleForSalaryHeadEnum] EESHE
																--			INNER JOIN  
																	--		( -- Salary Structure Start
                                                                      --  SELECT * from  (select SystemID SalaryId,EffectiveDate efd,EmpInfoSystemID eid ,SalaryRuleMasterSystemID from SalaryInfoDefineMaster
                                                                               --     union
                                                                                    --select SystemID SalaryId,EffectiveDate efd,EmpInfoSystemID eid ,SalaryRuleMasterSystemID from SalaryInfoBackMaster
                                                                                    --)
                                                                                     --mm 
                                                                                    --inner join (
                                                                                    --select MAX(EffectiveDate)EffectiveDate,EmpInfoSystemID from (
                                                                                    --select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster where IsApproved=1 and EffectiveDate<='" + toDate + @"'
                                                                                    --union
                                                                                     --select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster where IsApproved=1 and EffectiveDate<='" + toDate + @"'
                                                                                     --) x 
                                                                                     --group by EmpInfoSystemID
                                                                                    --)m on mm.efd=m.EffectiveDate and m.EmpInfoSystemID=mm.eid
                                                                                   -- Salary Structure End
                                                                                    --) 
																				--SalStruc on EESHE.SalaryStructureId = SalStruc.SalaryId and EESHE.EmpSystemId = SalStruc.EmpInfoSystemID
																				
																				
																				 --where EESHE.SalaryHeadEnum = '" + summaryType + @"' --AND EESHE.PlantId = '" + plantId + @"'-- as OF Monir via suggested for Invalid plant data Entry in Eligible table  
																				 --AND IsEligible = 1
																				
																				
																				--) 
																				--PFELIGIBLE ON SPC.SalaryID = PFELIGIBLE.SalaryStructureId and SPC.EmpInfoSystemID = PFELIGIBLE.EmpSystemId
																	LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = SPC.SlrProcMstSystemID
																	LEFT JOIN CurrencyRuleMaster CRM ON CRM.SystemID = SRM.CurrencyRuleSystemID                                                                    
										LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = SH.SalaryHeadID
														
											) EmpSlr ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID AND EmpBasic.PlantID = EmpSlr.PlantID
                                    LEFT JOIN
		                                    (
											 SELECT EmpSystemID, MonthNo, YearNo, TotalProcDate, TotalPresent, TotalLate,
													TotalAbsent AbsentDays, TotalLv, TotalMLv, TotalCompAssignLv, TotalWeekOff, TotalHoliDay,
													TotalWeekOffHoliDay, TotalOTHr, TotalNormalOTHr, TotalExtraOTHr
				                              FROM SalaryProceAttdnData  WHERE MonthNo = '" + month + @"' AND YearNo = '" + year + @"'
											) MMDSA ON EmpSlr.EmpInfoSystemID = MMDSA.EmpSystemID 											   
													   WHERE EmpBasic.CompanyId = '" + companyId + @"' AND 
														EmpBasic.PlantId = '" + plantId + @"' AND
													EmpSlr.MonthNo = '" + month + @"' and EmpSlr.YearNo = '" + year + @"' 
                                                AND EmpBasic.SystemId in 
                                                       (
																
																 SELECT EESHE.EmpSystemId FROM [EmployeeEligibleForSalaryHeadEnum] EESHE
																			INNER JOIN  
																			( -- Salary Structure Start
                                                                        SELECT * from  (select SystemID SalaryId,EffectiveDate efd,EmpInfoSystemID eid ,SalaryRuleMasterSystemID from SalaryInfoDefineMaster
                                                                                    union
                                                                                    select SystemID SalaryId,EffectiveDate efd,EmpInfoSystemID eid ,SalaryRuleMasterSystemID from SalaryInfoBackMaster
                                                                                    )
                                                                                     mm 
                                                                                    inner join (
                                                                                    select MAX(EffectiveDate)EffectiveDate,EmpInfoSystemID from (
                                                                                    select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster where IsApproved=1 and EffectiveDate<='" + toDate + @"'
                                                                                    union
                                                                                     select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster where IsApproved=1 and EffectiveDate<='" + toDate + @"'
                                                                                     ) x 
                                                                                     group by EmpInfoSystemID
                                                                                    )m on mm.efd=m.EffectiveDate and m.EmpInfoSystemID=mm.eid
                                                                                   -- Salary Structure End
                                                                                    ) 
																				SalStruc on EESHE.SalaryStructureId = SalStruc.SalaryId and EESHE.EmpSystemId = SalStruc.EmpInfoSystemID
																				
																				
																				 where EESHE.SalaryHeadEnum = '" + summaryType + @"'  --AND EESHE.PlantId = '20188'-- as OF Monir via suggested for Invalid plant data Entry in Eligible table  
																				 AND IsEligible = 1
																				
																				
																				


													)                                        
                                         GROUP BY  EmpSlr.PlantID, EmpSlr.FromDate, EmpSlr.ToDate,
								 EmpSlr.MonthNo, EmpSlr.YearNo,EmpSlr.SalaryHead
                                ,EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.DisbusmentCurrencyID,EmpSlr.Cat";

                return _sqlRepository.GetDataTable(sqlText);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public DataTable GetTotalEmployeeAsPerSalaryPorcessSql(string plantId, string year, string month, string fromDate, string toDate, string salaryProcessId, string paymentMode, string empStatus, string employeeCategoryId, string jobLocationId)
        {
            try
            {
                string wcJobLocation = "";
                var wcEmpStatus = "";
                if (empStatus == "Active" || empStatus == "Separated")
                {


                    if (empStatus == "Separated")
                    {
                        wcEmpStatus = @" AND EI.EmployeeStatus = '" + empStatus + @"' and Month(EI.DOS) = " + month + " and YEAR(EI.Dos) =  " + year + "";

                    }

                    if (empStatus == "Active")
                    {
                        wcEmpStatus = @"AND EI.EmployeeStatus = '" + empStatus + @"' AND EI.DOJ <=  '" + toDate + "'";

                    }
                }
                if (empStatus == "Maternity")
                {
                    wcEmpStatus = @"AND EI.SystemID IN (

                                                 SELECT LTR.EmpSystemID from LeaveTransaction LTR

                                                 LEFT JOIN LeaveType LT ON LT.Id = LTR.LTSystemID

                                                  WHERE LT.LeaveType = 'Maternity' AND

                                                ((MONTH(Convert(date, (FromDate) - 1)) = '" + month + @"' and YEar(Convert(date, (FromDate) - 1)) = '" + year + @"')

                                                 OR
                                                 (MONTH(Convert(date, (ToDate) + 1)) = '" + month + @"' and YEar(Convert(date, (ToDate) + 1)) = '" + year + @"')
												 )
												 )";
                }
                if (!string.IsNullOrEmpty(jobLocationId))
                {
                    wcJobLocation = "AND JL.SystemID IN (" + jobLocationId + @")";
                }
                string sqlText = "";
                sqlText = @"SELECT Count(DISTINCT SPC.EmpInfoSystemID) TotalEmployee, EI.EmployeeStatus, SPC.PlantID, SPM.MonthNo, SPM.YearNo
                                    FROM SalaryProcChild SPC
                                    INNER JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
	                                    AND SPM.SystemID IN (
		                                    SELECT SystemID
		                                    FROM SalaryProcMaster
		                                    WHERE SystemID IN (
				                                    SELECT SlrProcMstSystemID
				                                    FROM SalaryProcChild
				                                    WHERE PlantID = '" + plantId + @"'
				                                    GROUP BY SlrProcMstSystemID
				                                    )
			                                    AND MonthNo = '" + month + @"'
			                                    AND YearNo = '" + year + @"'
			                             
		                                    )
                                    INNER JOIN EmployeeInformation EI ON EI.SystemId = SPC.EmpInfoSystemID
									LEFT JOIN JobLocation JL ON JL.SystemID = EI.JobLocationID

                                        
                                    WHERE MonthNo = '" + month + @"'
			                                    AND YearNo = '" + year + @"'
	                                    AND SPC.PlantID = '" + plantId + @"'" + wcEmpStatus + @"" + wcJobLocation + @"
                                    GROUP BY SPC.PlantID, SPM.MonthNo, SPM.YearNo, EI.EmployeeStatus";
                return _sqlRepository.GetDataTable(sqlText);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public int getLargestAmongThree(int num1, int num2, int num3)
        {
            int result = 0;
            if (num1 > num2)
            {
                if (num1 > num3)
                {
                    result = num1;
                }
                else
                {
                    result = num3;
                }
            }
            else if (num2 > num3)
                result = num2;
            else
                result = num3;

            return result;
        }

        public double GetPositiveValues(double number)
        {
            return number < 0 ? number * (-1) : number;
        }

        [HttpGet,Authorize]
        public ActionResult GetEmployeeStatusWithMLVCbo()
        {
            string strSql = string.Empty;
            try
            {
                strSql = @" select distinct EmployeeStatus
                                    from EmployeeInformation
                                    Union
                                    select 'Maternity' EmployeeStatus ";
                return Json(_sqlRepository.GetCombo(strSql, "EmployeeStatus", "EmployeeStatus"), JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {

                throw;
            }
        }
    }

}