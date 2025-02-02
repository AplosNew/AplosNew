using Aplos.Controllers;
using Aplos.Properties;
using ConnectionManager;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Payroll;
using Library.Model.HumanResources;
using Library.Model.Setups;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Microsoft.Reporting.WebForms;
using OTSBD;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class AttendanceSlipController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly PayrollReportsService _payrollReportsService;
        private readonly IEmployeeProfileService _employeeProfileService;




        public AttendanceSlipController(IEmployeeProfileService employeeProfileService
             , ISqlRepository sqlRepository
            )
        {
            _payrollReportsService = new PayrollReportsService();
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
        public ActionResult GetEmployeeAttdnSlip(string month, string year, string salaryProcessId, Dictionary<string, string> parameters, string languageId, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = "Attendacnce Slip" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                #region Variable
                ReportUtility ru = null;

                clsReport objRpt = null;

                DataSet dsHeading = null;
                DataSet dsCmp = null;
                DataSet dsFactory = null;
                DataSet dsEmpLoyeeInfo = null;
                DataTable dtEmpLoyeeInfo = null;

                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                int xlsRow = 1, xlsCol = 1, endXlsCol = 1, startCol = 1;
                int headerStartXlsRow = 0;
                int headerEndxlsCol = 0;
                string FactoryName = "";
                string CmpName = "";

                double _OTRate = 0;
                double _OTHours = 0;

                #endregion Variable
                ru = new ReportUtility();
                objRpt = new clsReport();
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
                var ldateOfMonth = daysInMonth + "-" + monthName + "-" + year;
                var fdateOfMonth = "1" + "-" + monthName + "-" + year;

                var labelList = ru.LocalLanguageLabelList(identity.PlantId, languageId);
                var localLanguage = "";

                var printFont = "";
                bool isLocalLanguage = false;
                localLanguage = ru.LocalLanguageListSql(identity.PlantId, languageId, out isLocalLanguage);
                if (localLanguage == "Bengali")
                {
                    printFont = "SolaimanLipi";
                }
                else if (localLanguage == "Hindi")
                {
                    printFont = "SHREE-DV0726-OT";
                }
                else
                {
                    printFont = "Arial Narrow";

                }


                GetEmployeeInfoDetail(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fdateOfMonth, ldateOfMonth, languageId, parameters, isActive, isSeperated, isMaternity, out dsEmpLoyeeInfo);//Sql Query For Salary  Data
                Dictionary<string, List<DataRow>> dicMonthlyAttdnSummary = GetEmployeeAttdnSummaryInformation(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fdateOfMonth, ldateOfMonth, languageId, parameters, isActive, isSeperated, isMaternity);

                dtEmpLoyeeInfo = dsEmpLoyeeInfo.Tables[0];

                //objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                objRpt.SelectedPlantWiseCompany(identity.PlantId, languageId, out dsCmp);

                //objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                if (dtEmpLoyeeInfo.Rows.Count == 0)
                {
                    var ex = new Exception("No Data Found.......");
                    throw ex;
                }
                else
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;
                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = false;
                    int employeeInfoEndCol = 0;
                    int attdnInfoStartCol = 0;
                    int employeeInfoStartRow = xlsRow;
                    int attdnInfoStartRow = 0;
                    int loopCount = 1;
                    string employeeCode = "";
                    workbook.Version = ExcelVersion.Excel2016;
                    int empCount = 0;
                    int employeeOddRowNumber = xlsRow;
                    var employeeName = "";
                    try
                    {
                        for (int i = 0; i < dtEmpLoyeeInfo.Rows.Count; i++)
                        {
                            try
                            {
                                empCount++;
                                if ((i + 1) % 2 == 1)
                                {
                                    xlsCol = 1;
                                    employeeOddRowNumber = xlsRow;
                                    xlsRow += 2;
                                }
                                else
                                {
                                    xlsCol = 8;
                                    xlsRow = employeeOddRowNumber;
                                    employeeInfoStartRow = xlsRow;
                                    xlsRow += 2;
                                }

                                loopCount++;
                                #region Company And Report Header

                                string FactoryAddress = string.Empty;
                                headerEndxlsCol = xlsCol + 6;
                                headerStartXlsRow = xlsRow;
                                if (dsCmp.Tables[0].Rows.Count > 0)
                                {
                                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyNameLocal"].ToString();
                                }
                                else
                                {
                                    CmpName = "";
                                }
                                if (dsCmp.Tables[0].Rows.Count > 0)
                                {
                                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantNameLocal"].ToString();
                                }
                                else
                                {
                                    FactoryName = "";
                                }
                                sheet1.Range[xlsRow, xlsCol].Text = CmpName + "::" + FactoryName + ".";
                                sheet1.Range[xlsRow, xlsCol].WrapText = true;
                                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 6].Merge();
                                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                                //xlsCol = 1;
                                xlsRow += 1;
                                string strRptDateRange = "";
                                var attendanceTr = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Attendance.ToString(), "Attendance");
                                var slipTr = ru.GetLabelname(labelList, "Slip", "Slip");

                                strRptDateRange = attendanceTr + " " + slipTr + "    " + Convert.ToDateTime(ldateOfMonth).ToString("MMMM") + ", " + Convert.ToDateTime(ldateOfMonth).ToString("yyyy");
                                sheet1.Range[xlsRow, xlsCol].Text = strRptDateRange;
                                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 6].Merge();
                                sheet1.Range[headerStartXlsRow, xlsCol, xlsRow, headerEndxlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                                sheet1.Range[headerStartXlsRow, 1, xlsRow, headerEndxlsCol].CellStyle.Font.Bold = true;
                                sheet1.Range[headerStartXlsRow, 1, xlsRow, headerEndxlsCol].CellStyle.Font.FontName = "Arial Narrow";

                                sheet1.Range[headerStartXlsRow, 1, xlsRow, headerEndxlsCol].RowHeight = 20;

                                xlsRow += 1;

                                #endregion

                                #region 1st  Column

                                #region Employee Info
                                employeeInfoStartRow = xlsRow;


                                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].BorderAround(ExcelLineStyle.Hair);

                                sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.EmployeeInformation.ToString(), "Employee Information");
                                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.FontName = printFont;
                                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                                xlsRow++;

                                sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.EmployeeCode.ToString(), "Emp Code");
                                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 18;
                                sheet1.Range[xlsRow, xlsCol + 1].ColumnWidth = 26;
                                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.FontName = printFont;
                                sheet1.Range[xlsRow, xlsCol + 1].Text = ":   " + dtEmpLoyeeInfo.Rows[i]["EmployeeCode"].ToString();
                                xlsRow++;


                                if (string.IsNullOrEmpty(languageId))
                                {
                                    employeeName = dtEmpLoyeeInfo.Rows[i]["EmployeeName"].ToString();
                                }
                                else
                                {
                                    employeeName = dtEmpLoyeeInfo.Rows[i]["EmployeeNameLocal"].ToString();

                                }

                                sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Name.ToString(), "Name");
                                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.FontName = printFont;
                                sheet1.Range[xlsRow, xlsCol + 1].Text = ":   " + employeeName;
                                sheet1.Range[xlsRow, xlsCol + 1].CellStyle.Font.FontName = printFont;
                                xlsRow++;

                                sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Designation.ToString(), "Designation");
                                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.FontName = printFont;
                                sheet1.Range[xlsRow, xlsCol + 1].Text = ":   " + dtEmpLoyeeInfo.Rows[i]["DesignationLocal"].ToString();
                                sheet1.Range[xlsRow, xlsCol + 1].CellStyle.Font.FontName = printFont;
                                xlsRow++;

                                sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Department.ToString(), "Department");
                                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.FontName = printFont;

                                sheet1.Range[xlsRow, xlsCol + 1].Text = ":   " + dtEmpLoyeeInfo.Rows[i]["DepartmentName"].ToString();
                                sheet1.Range[xlsRow, xlsCol + 1].CellStyle.Font.FontName = printFont;

                                xlsRow++;

                                sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Grade.ToString(), "Grade");
                                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.FontName = printFont;

                                sheet1.Range[xlsRow, xlsCol + 1].Text = ":   " + dtEmpLoyeeInfo.Rows[i]["GradeCode"].ToString();

                                xlsRow++;

                                sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.DOJ.ToString(), "DOJ");
                                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.FontName = printFont;
                                sheet1.Range[xlsRow, xlsCol + 1].Text = ":   " + ru.GetFormatedDate(dtEmpLoyeeInfo.Rows[i]["DOJ"].ToString(), localLanguage);
                                sheet1.Range[xlsRow, xlsCol + 1].CellStyle.Font.FontName = printFont;



                                employeeInfoEndCol = xlsCol + 2;
                                #endregion

                                #region Attendance Information

                                attdnInfoStartCol = employeeInfoEndCol;
                                if (dicMonthlyAttdnSummary.ContainsKey(dtEmpLoyeeInfo.Rows[i]["SystemID"].ToString()))
                                {
                                    List<DataRow> drAttdnSummaryCollection = dicMonthlyAttdnSummary[dtEmpLoyeeInfo.Rows[i]["SystemID"].ToString()];
                                    attdnInfoStartRow = employeeInfoStartRow;
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol, employeeInfoStartRow, attdnInfoStartCol + 3].BorderAround(ExcelLineStyle.Hair);
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.AttendanceInfo.ToString(), "Attendance Information");
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol, employeeInfoStartRow, attdnInfoStartCol + 3].Merge();
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol].CellStyle.Font.FontName = printFont;

                                    employeeInfoStartRow++;
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Present.ToString(), "Present Days") + ":   ";
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol].CellStyle.Font.FontName = printFont;

                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol, employeeInfoStartRow, attdnInfoStartCol + 1].Merge();
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 2].Text = ":";
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 3].Number = Convert.ToDouble(clsStaticInfo.dbl(drAttdnSummaryCollection[0]["TotalPresent"].ToString()) + clsStaticInfo.dbl(drAttdnSummaryCollection[0]["TotalLate"].ToString()));
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 3].NumberFormat = ru.GetDecimalFormatlocal(2, localLanguage);
                                    employeeInfoStartRow++;

                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.WeeklyLeaveDays.ToString(), "Week Offs") + ":   ";
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol].CellStyle.Font.FontName = printFont;
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol, employeeInfoStartRow, attdnInfoStartCol + 1].Merge();
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 2].Text = ":";
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 3].Number = +clsStaticInfo.dbl(drAttdnSummaryCollection[0]["TotalWeekOff"].ToString());
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 3].NumberFormat = ru.GetDecimalFormatlocal(2, localLanguage);
                                    employeeInfoStartRow++;

                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.AvailedLeave.ToString(), "Availed Leave") + ":   ";
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol].CellStyle.Font.FontName = printFont;
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol, employeeInfoStartRow, attdnInfoStartCol + 1].Merge();
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 2].Text = ":";
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 3].Number = clsStaticInfo.dbl(drAttdnSummaryCollection[0]["TotalLv"].ToString());
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 3].NumberFormat = ru.GetDecimalFormatlocal(2, localLanguage);

                                    employeeInfoStartRow++;

                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.HoliDay.ToString(), "Holiday") + ":   ";
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol].CellStyle.Font.FontName = printFont;
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol, employeeInfoStartRow, attdnInfoStartCol + 1].Merge();
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 2].Text = ":";
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 3].Number = clsStaticInfo.dbl(drAttdnSummaryCollection[0]["TotalHoliDay"].ToString());
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 3].NumberFormat = ru.GetDecimalFormatlocal(2, localLanguage);

                                    employeeInfoStartRow++;

                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.AbsentDays.ToString(), "Absent") + ":   ";
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol].CellStyle.Font.FontName = printFont;
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol, employeeInfoStartRow, attdnInfoStartCol + 1].Merge();
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 2].Text = ":";
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 3].Number = clsStaticInfo.dbl(drAttdnSummaryCollection[0]["TotalAbsent"].ToString());
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 3].NumberFormat = ru.GetDecimalFormatlocal(2, localLanguage);

                                    employeeInfoStartRow++;

                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.TotalAttendance.ToString(), "Total Work Days") + ":   ";
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol].CellStyle.Font.FontName = printFont;
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol, employeeInfoStartRow, attdnInfoStartCol + 1].Merge();
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 2].Text = ":";
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 3].Number = clsStaticInfo.dbl(drAttdnSummaryCollection[0]["TotalProcDate"].ToString());
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 3].NumberFormat = ru.GetDecimalFormatlocal(2, localLanguage);

                                    employeeInfoStartRow++;
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OTHours.ToString(), "OT Hrs") + ":   ";
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol].CellStyle.Font.FontName = printFont;
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol, employeeInfoStartRow, attdnInfoStartCol + 1].Merge();
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 2].Text = ":";
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 3].Number = clsStaticInfo.dbl(drAttdnSummaryCollection[0]["TotalOTHr"].ToString()) / 60;
                                    sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 3].NumberFormat = ru.GetDecimalFormatlocal(2, localLanguage);

                                    //sheet1.Range[employeeInfoStartRow, attdnInfoStartCol + 2, employeeInfoStartRow, attdnInfoStartCol + 4].Merge();

                                    //employeeInfoStartRow++;
                                    //sheet1.Range[employeeInfoStartRow, attdnInfoStartCol, employeeInfoStartRow, attdnInfoStartCol + 1].Merge();
                                    sheet1.Range[attdnInfoStartRow + 1, attdnInfoStartCol, employeeInfoStartRow, attdnInfoStartCol + 3].BorderAround(ExcelLineStyle.Hair);
                                    //sheet1.Range[attdnInfoStartRow + 1, attdnInfoStartCol, employeeInfoStartRow, attdnInfoStartCol + 2].CellStyle.Font.FontName = printFont;
                                }
                                sheet1.Range[attdnInfoStartRow + 1, xlsCol, employeeInfoStartRow, xlsCol + 2].BorderAround(ExcelLineStyle.Hair);

                                #endregion

                                #endregion

                                xlsRow = employeeInfoStartRow;
                                if (empCount % 16 == 0)
                                {
                                    sheet1.HPageBreaks.Add(sheet1[xlsRow + 1, 1]);
                                }

                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }






                // workbook = _payrollReportsService.GetEmployeePaySlip(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, parameters, languageId,  isActive,  isSeperated,  isMaternity);




                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                //sheet1.UsedRange.CellStyle.Font.FontName = printFont;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.2;
                sheet1.PageSetup.BottomMargin = 0.5;
                //sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.UserId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.3;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                sheet1.Name = "AttendanceSlip";
                #endregion




                fileName = month + "-" + year + "AttendanceSlip" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
                string fullPathPDF = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);


                workbook.SaveAs(fullPathPDF);
                //var converter = new ExcelToPdfConverter(workbook);
                //var pdfDoc = converter.Convert();
                //fileName = month + "-" + year + "AttendanceSlip" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".pdf";
                //string fullPathPDF = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
                //pdfDoc.Save(fullPathPDF);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetEmpInfo(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var wcPayrollGroup = "";
            var wcSalaryProcess = "";
            var salaryProcessJoin = "";
            var salaryProcessColumn = "";
            var strDOJ = "";
            string param = "";
            string salaryProcessFlag = "";
            string wcEmpStatus = "";
            wcEmpStatus = " Where (1=0 ";

            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " Where (1=1 ";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus += " OR CurrentMonthEmployeeStatus ='Regular'";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus += " OR CurrentMonthEmployeeStatus ='SEPARATED'";
                }

            }
            wcEmpStatus += ")";

            param = "E.GroupID='" + identity.CompanyGroupId + "' AND E.CompanyId='" + identity.CompanyId + "' AND E.PlantId='" + identity.PlantId + "'";

            var cmdText = @"SELECT * fROM (  SELECT   dISTINCT        [CheckBoxSelect] = Convert(bit, 'False'),
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId                                     
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId                                     
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    , CASE WHEN MONTH(DOS) =  MONTH('" + effectiveDate + @"')  AND YEAR(DOS) = YEAR('" + effectiveDate + @"') then 'Separated' else 'Regular' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    " + salaryProcessFlag + @"
                                    " + salaryProcessColumn + @"
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName

                                    FROM EmployeeInformation e
                                    
									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=e.LegalDesignationId

                                   
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    LEFT OUTER JOIN ORG.Line eL on eL.id=mpb.LineId

                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
			                                       
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
									Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    " + salaryProcessJoin + @"
                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
									left join [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									left join [HKP].[Bank] bb on bb.Id = ebi.BankSystemID

                                     WHERE " + param + @" " + strDOJ + @"
                                            " + wcPayrollGroup + @"  " + wcSalaryProcess + @"  
                                                    and
									(E.DOS IS NULL OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + effectiveDate + @"')) 
                                     ) DD " + wcEmpStatus + @" ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";



            return Json(_sqlRepository.GetDataCollection(cmdText), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations

        #region Customized Functions(SQL and Others)
        public void GetEmployeeInfoDetail(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string languageId, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            var _wc = string.Empty;
            try
            {
                string wcEmpStatus = " Where (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " Where (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR CurrentMonthEmployeeStatus ='Regular'";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR CurrentMonthEmployeeStatus ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR CurrentMonthEmployeeStatus ='MLV_PRE'";

                    }
                }

                wcEmpStatus += ")";

                strSQL = @"SELECT * FROM (SELECT Distinct  E.SystemID,  E.EmployeeCode , E.EmployeeName,ISnull(E.EmployeeNameLocal,EmployeeName) EmployeeNameLocal,E.FatherName,  Format(E.DOJ,'dd-MMM-yyy') DOJ, Format(E.DOB,'dd-MMM-yyy') DOB,Format(E.DOS,'dd-MMM-yyy') DOS, E.EmployeeStatus,E.PaymentMode,
											 E.PlantID,Unit.UserName UnitName,Unit.Sequence UnitSequence, Division.Id DivisionID,
											 Division.UserName DivisionName,Division.Sequence DivisionSequence
											,Department.Id DepartmentID, Department.UserName DepartmentName,Department.Sequence DepartmentSequence,
											Section.Id SectionID, Section.UserName SectionName,Section.Sequence SectionSequence,
											 SubSection.Id SubSectionID, SubSection.UserName SubSectionName,SubSection.Sequence SubSectionSequence,EC.Id EmployeeCategorySystemID,
											EC.UserName EmpCategoryName,EC.Sequence EmployeeCategorySequence--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo
                                            ,ENT.UserName EntitySequence,e.SalaryRuleMasterSystemID,LD.UserName LegalDesignation--,eoe.IsOTEntitle
											,IsOTEntitle = Case  when ISNULL(EOE.IsOTEntitle,0) = 1 then EOE.IsOTEntitle else ISNULL(DMCT.IsOTEntitled,0) end
										   ,ISNULL(LD.Id,'') DesignationId,LD.UserName DesignationName,LD.Sequence DesignationSequence,ISNULL(EmpC.Id,'') EmployeeCategoryId,ISNULL(EmpC.UserName,'') EmployeeCategoryName
                                            , ISNULL(LD.UserName,'') LDDesignationGD,LSalGr.Code GradeCode,E.EmployeeCodePreFix, E.EmployeeCodeNumeric
										  , CASE WHEN MONTH(DOS) =  MONTH('" + fromDate + @"')  AND YEAR(DOS) = YEAR('" + fromDate + @"') THEN 'Separated' else 'Regular' end CurrentMonthEmployeeStatus
                                            
		                                  , ISNULL(LocLangLD.Name,LD.UserName) DesignationLocal
											,OverTimePmtPolicyMasterID=case when isnull(eoe.EmpSystemID,'')<>'' then (select id from OverTimePmtPolicyMaster where IsDefault=1)
											when DMCT.IsOTEntitled=1 and isnull(DMCT.OverTimePmtPolicyMasterID,'')<>'' then DMCT.OverTimePmtPolicyMasterID
											else null end, bb.BankAccNo,bb.BankName, '' BankNameFull
                                     ,otd.FormulaDesID,otd.FormulaDes
                                           FROM EmployeeInformation E
												
                                                LEFT JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								                LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId

  
									LEFT OUTER JOIN ORG.Position PO ON MB.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON MB.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = PO.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    Left join org.Line on Line.Id = MB.LineId
												LEFT JOIN HKP.LegalDesignation LD ON LD.Id=E.LegalDesignationId
												LEFT JOIN HKP.Designation GVD ON GVD.Id=E.GivenDesignationId
                                                LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LD.Id and E.PlantId = LSGD.PlantId
                                                LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = LSGD.LegalSalaryGradeId  and E.PlantId = LSalGr.PlantId
												LEFT JOIN org.SubDivision subDV ON PO.SubdivisionID = subDV.Id
												LEFT JOIN  MST.DesignationMaster DMOT ON DMOT.DesignationId = E.GivenDesignationId
 
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DMOT.EmployeeCategoryId
												LEFT JOIN  SCS.DesignationMasterConfiguration DMCT ON DMCT.DesignationMasterId = DMOT.Id and DMCT.plantid='" + plantId + @"'
	                                            LEFT JOIN EmployeeOTEntitle EOE ON EOE.EmpSystemID = E.SystemId AND EOE.IsOTEntitle=1
												
								            	LEFT JOIN (select * from OverTimePmtPolicyDetails where OverTimeDayType='Working Day') otd on otd.OverTimePmtPolicyID=case when isnull(eoe.EmpSystemID,'')<>'' then (select id from OverTimePmtPolicyMaster where IsDefault=1)
											WHEN DMCT.IsOTEntitled=1 and isnull(DMCT.OverTimePmtPolicyMasterID,'')<>'' then DMCT.OverTimePmtPolicyMasterID else null end
												LEFT JOIN
                                                (
                                                SELECT ECT.Id, ECT.UserName,ECT.Sequence , DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
												LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
												)EC ON EC.DesignationId=E.GivenDesignationId
                                                LEFT JOIN HKP.LocalLanguage LocLangLD ON LocLangLD.LegalDesignationId = E.LegalDesignationId AND LocLangLD.LanguageId = '" + languageId + @"'
                                                LEFT JOIN HKP.LocalLanguage LocLangGD ON LocLangGD.DesignationId = E.GivenDesignationId AND LocLangGD.LanguageId = '" + languageId + @"'
									      
												 
                                                LEFT JOIN (
                                                SELECT bb.UserName BankName,b.BankAccNo,b.EmpSystemID FROM [dbo].[EmployeeBankInfo] b
                                                LEFT JOIN hkp.BankBranch bb ON b.BankBranchId=bb.Id
                                                ) BB ON BB.EmpSystemID = E.SystemId
                                            WHERE E.PlantId = '" + plantId + @"' 
                                                 ";


                strSQL += @")dd " + wcEmpStatus + @" and
									(DOS IS NULL OR CONVERT(DATE,DOS) >= CONVERT(DATE,'" + fromDate + @"')) ";
                if (parameters.Count > 0)
                {
                    if (parameters.Keys.ElementAt(0) != "")
                    {
                        strSQL += @" AND SystemID IN(" + parameters["EmpSystemId"] + ")";
                    }
                }

                strSQL += "ORDER BY  SystemID";


                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSQL, out dsRef);


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

        public Dictionary<string, List<DataRow>> GetEmployeeAttdnSummaryInformation(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string languageId, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            string strSQL;
            DataSet dsRef = null;
            Dictionary<string, List<DataRow>> dicAttdn = new Dictionary<string, List<DataRow>>();

            try
            {

                string wcEmpStatus = " AND (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " Where (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR CurrentMonthEmployeeStatus ='Regular' ";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR CurrentMonthEmployeeStatus ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                    }
                }


                wcEmpStatus += ")";

                strSQL = @"Select * from(SELECT  EEI.COMPANYID,  EEI.PLANTID, EEI.SystemID EmpInfoSystemID , CASE WHEN MONTH(DOS) =  MONTH('" + fromDate + @"')  AND YEAR(DOS) = YEAR('" + fromDate + @"') then 'Separated' else 'Regular' end CurrentMonthEmployeeStatus
                                    ,[EmpSystemID], [MonthNo], [YearNo], [FromDate], [ToDate], [TotalProcDate], [TotalPresent], [TotalLate], [TotalAbsent], [TotalLv], [TotalMLv], [TotalCompAssignLv], [TotalWeekOff]
                                    , [TotalHoliDay], [TotalWeekOffHoliDay], [TotalOTHr], [TotalNormalOTHr], [TotalExtraOTHr], [IsDisbusted],  [TotalLWP] FROM ATTDNDATAMONTHLYSUMMARY ATDMS 
                                INNER JOIN EMPLOYEEINFORMATION EEI ON EEI.SYSTEMID = ATDMS.EMPSYSTEMID) dd
                                         WHERE COMPANYID = '" + companyId + @"' AND PLANTID = '" + plantId + @"' AND YEARNO = YEAR('" + fromDate + @"') AND MONTHNO = MONTH('" + fromDate + @"')  " + wcEmpStatus + " ";


                if (parameters.Count > 0)
                {
                    if (parameters.Keys.ElementAt(0) != "")
                    {
                        strSQL += @" AND EmpInfoSystemID IN(" + parameters["EmpSystemId"] + ")";
                    }
                }

                strSQL += " ORDER BY EmpInfoSystemID";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSQL, out dsRef);



                DataTable dt = dsRef.Tables[0];
                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (empId != dt.Rows[i]["EmpInfoSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        dicAttdn.Add(dt.Rows[i]["EmpInfoSystemID"].ToString(), _data);
                    }
                    _data.Add(dt.Rows[i]);

                    empId = dt.Rows[i]["EmpInfoSystemID"].ToString();
                }

                return dicAttdn;


            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //objCon = null;
            }
        }//End Function

        #endregion
    }
}