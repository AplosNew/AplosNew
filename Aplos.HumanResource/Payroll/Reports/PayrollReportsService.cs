#region Using
using clsAttendance;
using Library.Core;
using Library.Service.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.HumanResources;
using Library.Model.Setups;
using Library.Service.Helpers;
using Library.Service.Systems;
using Library.ViewModel.Organizations;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using static Library.Service.Helpers.ReportUtility;
using Library.Service.Logs;
using System.Reflection;
using Library.Service.Enums;
using ConnectionManager;
using System.Web.Hosting;
using System.IO;
using Library.Model.Enums;
using Library.Service.Payrolls.OT;
using Library.HumanResource.Report.Payroll;

#endregion Using

namespace Library.HumanResource.Payroll
{
    public class PayrollReportsService
    {
        #region Constructor

        private readonly SqlRepository _sqlRepository;


        public PayrollReportsService()
        {
            _sqlRepository = new SqlRepository();
        }

        #endregion Constructor


        public IWorkbook GetEmployeeSalaryStructure(string companyGroupId, string companyId, string plantId, string userId, string effectiveDate, string payRollGroup, Dictionary<string, string> parameters)
        {
            #region Variable

            clsReport objRpt = null;

            DataSet dsSlrProc = null;
            DataView dvEmp = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

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

                #endregion Variable


                #region DataSet

                List<SalaryStructureReport> listdsSlrProc = new List<SalaryStructureReport>();
                GetEmpSalaryInformationRpt(plantId, effectiveDate, payRollGroup, parameters, out dsSlrProc);
                dvEmp = new DataView();
                dvEmp.Table = dsSlrProc.Tables[0];

                if (dsSlrProc.Tables[0].Rows.Count > 0)
                {
                    listdsSlrProc = dsSlrProc.Tables[0].ToList<SalaryStructureReport>();
                }
                else
                {
                    throw new Exception("No Data Found");
                }
                DataTable dtEmployees = dvEmp.ToTable(true, "SystemID", "Department", "Designation", "LegalDesignation", "GivenDesignation", "DOJ", "DOS", "DOB", "Grade", "GradeCode", "EmployeeName", "EmployeeCode", "SalaryHeadValue", "Line", "Gender", "PayRollGroup", "JobLocation", "PaymentMode", "BankName", "Section", "Unit");


                objRpt.SelectedPlantWiseCompany(para.PlantId, out dsCmp);

                objRpt.SelectedPlant(para.PlantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 5;
                xlsCol = 1;

                int ColSr = 0;
                int ColIDNo = 0;
                int ColName = 0;
                int ColDOJ = 0;
                int ColDOB = 0;
                int ColDOs = 0;
                int ColGrade = 0;
                int ColGVDG = 0;
                int ColGrs = 0;
                int ColDepartment = 0;
                int ColSection = 0;
                int ColUnit = 0;
                int ColLine = 0;
                int ColpayrollGroup = 0;
                int ColpaymentMode = 0;
                int ColJobLocation = 0;
                int ColGender = 0;


                //1
                ru.SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);
                ru.SetCellValue("ID No.", sheet1, xlsRow, ref xlsCol, out ColIDNo, 8);
                ru.SetCellValue("Name", sheet1, xlsRow, ref xlsCol, out ColName, 30);
                ru.SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out ColDOJ, 12);
                ru.SetCellValue("DOB", sheet1, xlsRow, ref xlsCol, out ColDOB, 12);
                ru.SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out ColDOs, 12);
                ru.SetCellValue("Designation", sheet1, xlsRow, ref xlsCol, out ColGVDG, 20);
                ru.SetCellValue("Department", sheet1, xlsRow, ref xlsCol, out ColDepartment, 20);
                ru.SetCellValue("Section", sheet1, xlsRow, ref xlsCol, out ColSection, 20);
                ru.SetCellValue("Unit", sheet1, xlsRow, ref xlsCol, out ColUnit, 20);
                ru.SetCellValue("Line", sheet1, xlsRow, ref xlsCol, out ColLine, 20);
                ru.SetCellValue("Payroll Group", sheet1, xlsRow, ref xlsCol, out ColpayrollGroup, 20);
                ru.SetCellValue("Payment Mode", sheet1, xlsRow, ref xlsCol, out ColpaymentMode, 20);
                ru.SetCellValue("Job Location", sheet1, xlsRow, ref xlsCol, out ColJobLocation, 20);
                ru.SetCellValue("Gender", sheet1, xlsRow, ref xlsCol, out ColGender, 20);
                ru.SetCellValue("Grade", sheet1, xlsRow, ref xlsCol, out ColGrade, 20);


                //SR to
                sheet1.Range[xlsRow, ColSr].Text = "Employee Information";
                sheet1.Range[xlsRow, ColSr, xlsRow, ColGrade].Merge();
                ColGrs = ColGrade;

                DataView dvSalaryHead = new DataView(dsSlrProc.Tables[0]);
                dvSalaryHead.Sort = "HeadType desc,Sequence";
                DataTable dtSalaryHead = dvSalaryHead.ToTable(true, "SalaryHeadID", "SalaryHead", "HeadType", "Sequence", "HeadCategory", "IntegerInDisb", "DecimalNo");

                int _count_earning_head = 0;
                int _count_earning_ctchead = 0;
                int _count_deducting_head = 0;
                int _total_head_count = 0;

                List<SalaryHeadSequence> list = null;

                CreateDynamicSHead(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out list);

                if (_count_earning_ctchead > 0)
                {
                    sheet1.Range[xlsRow, ColGrs + 1].Text = "Earning";
                    sheet1.Range[xlsRow, ColGrs + 1, xlsRow, ColGrs + _count_earning_ctchead].Merge();
                }

                int ds = ColGrs + _count_earning_ctchead + 1;

                if (_count_deducting_head > 0)
                {
                    sheet1.Range[xlsRow, ds].Text = "Deduction";
                    sheet1.Range[xlsRow, ds, xlsRow, ds + _count_deducting_head - 1].Merge();
                }
                int np = 0;
                if (list.Count > 0)
                {
                    xlsCol++;
                    np = ColGrs + list.Count + 1;
                    sheet1.Range[xlsRow, np].Text = "Net Payable";
                    sheet1.Range[xlsRow, np].ColumnWidth = 10;
                    sheet1.Range[xlsRow, np, xlsRow + 1, np].Merge();
                }

                xlsCol++;
                int MinWage = ColGrs + list.Count + 2;
                sheet1.Range[xlsRow, MinWage].Text = "Minimum Wage";
                sheet1.Range[xlsRow, MinWage].ColumnWidth = 10;
                sheet1.Range[xlsRow, MinWage, xlsRow + 1, MinWage].Merge();

                sheet1.Range[xlsRow, 1, xlsRow + 1, MinWage].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                sheet1.Range[xlsRow, 1, xlsRow + 1, MinWage].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow + 1, MinWage].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow + 1, MinWage].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow + 1, MinWage].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow + 1, MinWage].VerticalAlignment = ExcelVAlign.VAlignCenter;

                endXlsCol = MinWage;
                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                Param param = new Param();
                param.CompanyGroupId = companyGroupId;
                param.CompanyId = companyId;
                ru.Header(ref sheet1, param, endXlsCol, "Employee Salary Information");

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------

                int SrNo = 0;
                string x = "";

                ReportUtility oRU = new ReportUtility();

                xlsRow = RowIndex;


                xlsRow--;
                //Test();
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    #region EmpInfo
                    SrNo += 1;
                    x = dtEmployees.Rows[i]["SystemID"].ToString().Trim();

                    //1
                    sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                    sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                        sheet1.Range[xlsRow, ColIDNo].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, ColIDNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColIDNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //3
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                        sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJ"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJ"].ToString();
                    sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //4.2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOB"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOB].Text = dtEmployees.Rows[i]["DOB"].ToString();
                    sheet1.Range[xlsRow, ColDOB].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOB].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4.2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOS"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOs].Text = dtEmployees.Rows[i]["DOS"].ToString();
                    sheet1.Range[xlsRow, ColDOs].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOs].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4.3
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["GradeCode"].ToString()) == false)
                        sheet1.Range[xlsRow, ColGrade].Text = dtEmployees.Rows[i]["GradeCode"].ToString();
                    sheet1.Range[xlsRow, ColGrade].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColGrade].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LegalDesignation"].ToString()) == false)
                        sheet1.Range[xlsRow, ColGVDG].Text = dtEmployees.Rows[i]["LegalDesignation"].ToString();
                    sheet1.Range[xlsRow, ColGVDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColGVDG].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                        sheet1.Range[xlsRow, ColGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                    sheet1.Range[xlsRow, ColGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PayRollGroup"].ToString()) == false)
                        sheet1.Range[xlsRow, ColpayrollGroup].Text = dtEmployees.Rows[i]["PayRollGroup"].ToString();
                    sheet1.Range[xlsRow, ColpayrollGroup].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColpayrollGroup].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PaymentMode"].ToString()) == false)
                        sheet1.Range[xlsRow, ColpaymentMode].Text = dtEmployees.Rows[i]["PaymentMode"].ToString();
                    sheet1.Range[xlsRow, ColpaymentMode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColpaymentMode].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Department"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDepartment].Text = dtEmployees.Rows[i]["Department"].ToString();
                    sheet1.Range[xlsRow, ColDepartment].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDepartment].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Section"].ToString()) == false)
                        sheet1.Range[xlsRow, ColSection].Text = dtEmployees.Rows[i]["Section"].ToString();
                    sheet1.Range[xlsRow, ColSection].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColSection].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Line"].ToString()) == false)
                        sheet1.Range[xlsRow, ColLine].Text = dtEmployees.Rows[i]["Line"].ToString();
                    sheet1.Range[xlsRow, ColLine].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColLine].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PayRollGroup"].ToString()) == false)
                        sheet1.Range[xlsRow, ColpayrollGroup].Text = dtEmployees.Rows[i]["PayRollGroup"].ToString();
                    sheet1.Range[xlsRow, ColpayrollGroup].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColpayrollGroup].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["JobLocation"].ToString()) == false)
                        sheet1.Range[xlsRow, ColJobLocation].Text = dtEmployees.Rows[i]["JobLocation"].ToString();
                    sheet1.Range[xlsRow, ColJobLocation].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColJobLocation].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Line"].ToString()) == false)
                        sheet1.Range[xlsRow, ColLine].Text = dtEmployees.Rows[i]["Line"].ToString();
                    sheet1.Range[xlsRow, ColLine].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColLine].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Unit"].ToString()) == false)
                        sheet1.Range[xlsRow, ColUnit].Text = dtEmployees.Rows[i]["Unit"].ToString();
                    sheet1.Range[xlsRow, ColUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    #endregion

                    int _total_head_count_body = 0;

                    for (int ci = 0; ci < list.Count; ci++)
                    {
                        #region Head wise loop
                        var ob = list[ci];
                        if (ob.SalaryHead.Length > 0)
                        {
                            var hId = ob.SalaryHeadId;
                            _total_head_count_body++;

                            var _data = listdsSlrProc.Where(r => r.SalaryHeadID == hId && r.EmpInfoSystemID == x).FirstOrDefault();

                            if (_data != null)
                            {
                                //if (ob.HeadCategory.ToUpper() == "Net Payable".ToUpper())
                                //{
                                //    sheet1.Range[xlsRow, np].Number = Convert.ToDouble(_data.EntryAmount.ToString());
                                //    sheet1.Range[xlsRow, np].NumberFormat = oRU.NumberFormatInt();
                                //    sheet1.Range[xlsRow, np].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                //    sheet1.Range[xlsRow, np].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                //}
                                sheet1.Range[xlsRow, ob.XLColIndex].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(_data.EntryAmount > 0 ? _data.EntryAmount.ToString() : (_data.EntryAmount * (-1)).ToString()));

                                sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = GetDecimalFormat(ob);// oRU.NumberFormatInt();
                                sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }//row found
                        }// 
                        #endregion
                        //});
                    }//for dtSalaryHead

                    //DataView dvSheetNetPay = new DataView(dsSlrProc.Tables[0]);
                    //dvSheetNetPay.RowFilter = "HeadCategory = 'Net Payable' and SystemID=" + x + "";
                    DataView dvStructNetPay = new DataView(dsSlrProc.Tables[0]);
                    dvStructNetPay.RowFilter = "HeadCategory = 'Net Payable' and SystemID=" + x + "";
                    if (dvStructNetPay.Count > 0)
                    {
                        sheet1.Range[xlsRow, np].Number = Convert.ToDouble(dvStructNetPay[0]["EntryAmount"].ToString());

                    }
                    double _minWage = 0;
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SalaryHeadValue"].ToString()) == false)
                    {
                        _minWage = Convert.ToDouble(bplib.clsWebLib.GetNumData(dtEmployees.Rows[i]["SalaryHeadValue"].ToString()));
                    }

                    sheet1.Range[xlsRow, MinWage].Number = _minWage;
                    sheet1.Range[xlsRow, MinWage].NumberFormat = oRU.NumberFormatInt();
                    sheet1.Range[xlsRow, MinWage].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, MinWage].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow++;
                }//for emp count

                #endregion ----------------------Data-----------------------

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 7;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "SalaryInformation";
                #endregion

                workbook.Version = ExcelVersion.Excel2013;
                string strFileName = "SalaryInfo" + bplib.clsWebLib.DateData_DBToApp(DateTime.Now.Date, bplib.clsWebLib.STD_DATE_FORMAT).ToString("dd-MMM-yyyy") + ".xls";

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
            }
        }



        public IWorkbook GetEmployeeSalaryStructurePlantWise(string companyGroupId, string companyId, string plantIdList, string userId, string effectiveDate, Dictionary<string, string> parameters)
        {
            #region Variable

            clsReport objRpt = null;

            DataSet dsSlrProc = null;
            DataView dvEmp = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

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

                #endregion Variable


                #region DataSet

                List<SalaryStructureReport> listdsSlrProc = new List<SalaryStructureReport>();
                GetEmpSalaryInformationRptPlantWise(plantIdList, effectiveDate, out dsSlrProc);
                // GetEmpSalaryInformationRpt(plantId, effectiveDate, payRollGroup, parameters, out dsSlrProc);
                dvEmp = new DataView();
                dvEmp.Table = dsSlrProc.Tables[0];

                if (dsSlrProc.Tables[0].Rows.Count > 0)
                {
                    listdsSlrProc = dsSlrProc.Tables[0].ToList<SalaryStructureReport>();
                }
                else
                {
                    throw new Exception("No Data Found");
                }
                DataTable dtEmployees = dvEmp.ToTable(true, "SystemID", "Department", "Designation", "LegalDesignation", "GivenDesignation", "DOJ", "DOS", "DOB", "Grade", "GradeCode", "EmployeeName", "EmployeeCode", "SalaryHeadValue", "Line", "Gender", "PayRollGroup", "JobLocation", "PaymentMode", "BankName", "Section", "Unit");


                objRpt.SelectedPlantWiseCompany(para.PlantId, out dsCmp);

                objRpt.SelectedPlant(para.PlantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 5;
                xlsCol = 1;

                int ColSr = 0;
                int ColIDNo = 0;
                int ColName = 0;
                int ColDOJ = 0;
                int ColDOB = 0;
                int ColDOs = 0;
                int ColGrade = 0;
                int ColGVDG = 0;
                int ColGrs = 0;
                int ColDepartment = 0;
                int ColSection = 0;
                int ColUnit = 0;
                int ColLine = 0;
                int ColpayrollGroup = 0;
                int ColpaymentMode = 0;
                int ColJobLocation = 0;
                int ColGender = 0;


                //1
                ru.SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);
                ru.SetCellValue("ID No.", sheet1, xlsRow, ref xlsCol, out ColIDNo, 8);
                ru.SetCellValue("Name", sheet1, xlsRow, ref xlsCol, out ColName, 30);
                ru.SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out ColDOJ, 12);
                ru.SetCellValue("DOB", sheet1, xlsRow, ref xlsCol, out ColDOB, 12);
                ru.SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out ColDOs, 12);
                ru.SetCellValue("Designation", sheet1, xlsRow, ref xlsCol, out ColGVDG, 20);
                ru.SetCellValue("Department", sheet1, xlsRow, ref xlsCol, out ColDepartment, 20);
                ru.SetCellValue("Section", sheet1, xlsRow, ref xlsCol, out ColSection, 20);
                ru.SetCellValue("Unit", sheet1, xlsRow, ref xlsCol, out ColUnit, 20);
                ru.SetCellValue("Line", sheet1, xlsRow, ref xlsCol, out ColLine, 20);
                ru.SetCellValue("Payroll Group", sheet1, xlsRow, ref xlsCol, out ColpayrollGroup, 20);
                ru.SetCellValue("Payment Mode", sheet1, xlsRow, ref xlsCol, out ColpaymentMode, 20);
                ru.SetCellValue("Job Location", sheet1, xlsRow, ref xlsCol, out ColJobLocation, 20);
                ru.SetCellValue("Gender", sheet1, xlsRow, ref xlsCol, out ColGender, 20);
                ru.SetCellValue("Grade", sheet1, xlsRow, ref xlsCol, out ColGrade, 20);


                //SR to
                sheet1.Range[xlsRow, ColSr].Text = "Employee Information";
                sheet1.Range[xlsRow, ColSr, xlsRow, ColGrade].Merge();
                ColGrs = ColGrade;

                DataView dvSalaryHead = new DataView(dsSlrProc.Tables[0]);
                dvSalaryHead.Sort = "HeadType desc,Sequence";
                DataTable dtSalaryHead = dvSalaryHead.ToTable(true, "SalaryHeadID", "SalaryHead", "HeadType", "Sequence", "HeadCategory", "IntegerInDisb", "DecimalNo");

                int _count_earning_head = 0;
                int _count_earning_ctchead = 0;
                int _count_deducting_head = 0;
                int _total_head_count = 0;

                List<SalaryHeadSequence> list = null;

                CreateDynamicSHead(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out list);

                if (_count_earning_ctchead > 0)
                {
                    sheet1.Range[xlsRow, ColGrs + 1].Text = "Earning";
                    sheet1.Range[xlsRow, ColGrs + 1, xlsRow, ColGrs + _count_earning_ctchead].Merge();
                }

                int ds = ColGrs + _count_earning_ctchead + 1;

                if (_count_deducting_head > 0)
                {
                    sheet1.Range[xlsRow, ds].Text = "Deduction";
                    sheet1.Range[xlsRow, ds, xlsRow, ds + _count_deducting_head - 1].Merge();
                }
                int np = 0;
                if (list.Count > 0)
                {
                    xlsCol++;
                    np = ColGrs + list.Count + 1;
                    sheet1.Range[xlsRow, np].Text = "Net Payable";
                    sheet1.Range[xlsRow, np].ColumnWidth = 10;
                    sheet1.Range[xlsRow, np, xlsRow + 1, np].Merge();
                }

                xlsCol++;
                int MinWage = ColGrs + list.Count + 2;
                sheet1.Range[xlsRow, MinWage].Text = "Minimum Wage";
                sheet1.Range[xlsRow, MinWage].ColumnWidth = 10;
                sheet1.Range[xlsRow, MinWage, xlsRow + 1, MinWage].Merge();

                sheet1.Range[xlsRow, 1, xlsRow + 1, MinWage].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                sheet1.Range[xlsRow, 1, xlsRow + 1, MinWage].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow + 1, MinWage].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow + 1, MinWage].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow + 1, MinWage].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow + 1, MinWage].VerticalAlignment = ExcelVAlign.VAlignCenter;

                endXlsCol = MinWage;
                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                Param param = new Param();
                param.CompanyGroupId = companyGroupId;
                param.CompanyId = companyId;
                ru.Header(ref sheet1, param, endXlsCol, "Employee Salary Information");

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------

                int SrNo = 0;
                string x = "";

                ReportUtility oRU = new ReportUtility();

                xlsRow = RowIndex;


                xlsRow--;
                //Test();
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    #region EmpInfo
                    SrNo += 1;
                    x = dtEmployees.Rows[i]["SystemID"].ToString().Trim();

                    //1
                    sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                    sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                        sheet1.Range[xlsRow, ColIDNo].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, ColIDNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColIDNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //3
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                        sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJ"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJ"].ToString();
                    sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //4.2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOB"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOB].Text = dtEmployees.Rows[i]["DOB"].ToString();
                    sheet1.Range[xlsRow, ColDOB].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOB].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4.2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOS"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOs].Text = dtEmployees.Rows[i]["DOS"].ToString();
                    sheet1.Range[xlsRow, ColDOs].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOs].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4.3
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["GradeCode"].ToString()) == false)
                        sheet1.Range[xlsRow, ColGrade].Text = dtEmployees.Rows[i]["GradeCode"].ToString();
                    sheet1.Range[xlsRow, ColGrade].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColGrade].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LegalDesignation"].ToString()) == false)
                        sheet1.Range[xlsRow, ColGVDG].Text = dtEmployees.Rows[i]["LegalDesignation"].ToString();
                    sheet1.Range[xlsRow, ColGVDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColGVDG].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                        sheet1.Range[xlsRow, ColGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                    sheet1.Range[xlsRow, ColGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PayRollGroup"].ToString()) == false)
                        sheet1.Range[xlsRow, ColpayrollGroup].Text = dtEmployees.Rows[i]["PayRollGroup"].ToString();
                    sheet1.Range[xlsRow, ColpayrollGroup].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColpayrollGroup].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PaymentMode"].ToString()) == false)
                        sheet1.Range[xlsRow, ColpaymentMode].Text = dtEmployees.Rows[i]["PaymentMode"].ToString();
                    sheet1.Range[xlsRow, ColpaymentMode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColpaymentMode].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Department"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDepartment].Text = dtEmployees.Rows[i]["Department"].ToString();
                    sheet1.Range[xlsRow, ColDepartment].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDepartment].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Section"].ToString()) == false)
                        sheet1.Range[xlsRow, ColSection].Text = dtEmployees.Rows[i]["Section"].ToString();
                    sheet1.Range[xlsRow, ColSection].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColSection].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Line"].ToString()) == false)
                        sheet1.Range[xlsRow, ColLine].Text = dtEmployees.Rows[i]["Line"].ToString();
                    sheet1.Range[xlsRow, ColLine].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColLine].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PayRollGroup"].ToString()) == false)
                        sheet1.Range[xlsRow, ColpayrollGroup].Text = dtEmployees.Rows[i]["PayRollGroup"].ToString();
                    sheet1.Range[xlsRow, ColpayrollGroup].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColpayrollGroup].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["JobLocation"].ToString()) == false)
                        sheet1.Range[xlsRow, ColJobLocation].Text = dtEmployees.Rows[i]["JobLocation"].ToString();
                    sheet1.Range[xlsRow, ColJobLocation].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColJobLocation].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Line"].ToString()) == false)
                        sheet1.Range[xlsRow, ColLine].Text = dtEmployees.Rows[i]["Line"].ToString();
                    sheet1.Range[xlsRow, ColLine].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColLine].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Unit"].ToString()) == false)
                        sheet1.Range[xlsRow, ColUnit].Text = dtEmployees.Rows[i]["Unit"].ToString();
                    sheet1.Range[xlsRow, ColUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    #endregion

                    int _total_head_count_body = 0;

                    for (int ci = 0; ci < list.Count; ci++)
                    {
                        #region Head wise loop
                        var ob = list[ci];
                        if (ob.SalaryHead.Length > 0)
                        {
                            var hId = ob.SalaryHeadId;
                            _total_head_count_body++;

                            var _data = listdsSlrProc.Where(r => r.SalaryHeadID == hId && r.EmpInfoSystemID == x).FirstOrDefault();

                            if (_data != null)
                            {
                                //if (ob.HeadCategory.ToUpper() == "Net Payable".ToUpper())
                                //{
                                //    sheet1.Range[xlsRow, np].Number = Convert.ToDouble(_data.EntryAmount.ToString());
                                //    sheet1.Range[xlsRow, np].NumberFormat = oRU.NumberFormatInt();
                                //    sheet1.Range[xlsRow, np].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                //    sheet1.Range[xlsRow, np].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                //}
                                sheet1.Range[xlsRow, ob.XLColIndex].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(_data.EntryAmount > 0 ? _data.EntryAmount.ToString() : (_data.EntryAmount * (-1)).ToString()));

                                sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = GetDecimalFormat(ob);// oRU.NumberFormatInt();
                                sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }//row found
                        }// 
                        #endregion
                        //});
                    }//for dtSalaryHead

                    //DataView dvSheetNetPay = new DataView(dsSlrProc.Tables[0]);
                    //dvSheetNetPay.RowFilter = "HeadCategory = 'Net Payable' and SystemID=" + x + "";
                    DataView dvStructNetPay = new DataView(dsSlrProc.Tables[0]);
                    dvStructNetPay.RowFilter = "HeadCategory = 'Net Payable' and SystemID=" + x + "";
                    if (dvStructNetPay.Count > 0)
                    {
                        sheet1.Range[xlsRow, np].Number = Convert.ToDouble(dvStructNetPay[0]["EntryAmount"].ToString());

                    }
                    double _minWage = 0;
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SalaryHeadValue"].ToString()) == false)
                    {
                        _minWage = Convert.ToDouble(bplib.clsWebLib.GetNumData(dtEmployees.Rows[i]["SalaryHeadValue"].ToString()));
                    }

                    sheet1.Range[xlsRow, MinWage].Number = _minWage;
                    sheet1.Range[xlsRow, MinWage].NumberFormat = oRU.NumberFormatInt();
                    sheet1.Range[xlsRow, MinWage].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, MinWage].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow++;
                }//for emp count

                #endregion ----------------------Data-----------------------

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 7;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "SalaryInformation";
                #endregion

                workbook.Version = ExcelVersion.Excel2013;
                string strFileName = "SalaryInfo" + bplib.clsWebLib.DateData_DBToApp(DateTime.Now.Date, bplib.clsWebLib.STD_DATE_FORMAT).ToString("dd-MMM-yyyy") + ".xls";

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
            }
        }



        public IWorkbook GetSeparatedEmployeeStructure(string companyGroupId, string companyId, string plantId, string userId, string effectiveDate, string FromDate, string ToDate, string payRollGroup, Dictionary<string, string> parameters)
        {
            #region Variable

            clsReport objRpt = null;

            DataSet dsSlrProc = null;
            DataView dvEmp = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

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

                #endregion Variable


                #region DataSet

                List<SalaryStructureReport> listdsSlrProc = new List<SalaryStructureReport>();
                GetSeparatedEmployeeSalaryInformationRpt(plantId, effectiveDate, FromDate, ToDate, payRollGroup, parameters, out dsSlrProc);
                dvEmp = new DataView();
                dvEmp.Table = dsSlrProc.Tables[0];

                if (dsSlrProc.Tables[0].Rows.Count > 0)
                {
                    listdsSlrProc = dsSlrProc.Tables[0].ToList<SalaryStructureReport>();
                }
                else
                {
                    throw new Exception("No Data Found");
                }
                DataTable dtEmployees = dvEmp.ToTable(true, "SystemID", "Department", "Designation", "LegalDesignation", "GivenDesignation", "DOJ", "DOS", "DOB", "Grade", "EmployeeName", "EmployeeCode", "SalaryHeadValue");


                objRpt.SelectedPlantWiseCompany(para.PlantId, out dsCmp);

                objRpt.SelectedPlant(para.PlantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 5;
                xlsCol = 1;

                int ColSr = 0;
                int ColIDNo = 0;
                int ColName = 0;
                int ColDOJ = 0;
                int ColDOB = 0;
                int ColDOs = 0;
                int ColGrade = 0;
                int ColGVDG = 0;
                int ColGrs = 0;

                //1
                ru.SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);
                ru.SetCellValue("ID No.", sheet1, xlsRow, ref xlsCol, out ColIDNo, 8);
                ru.SetCellValue("Name", sheet1, xlsRow, ref xlsCol, out ColName, 30);
                ru.SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out ColDOJ, 12);
                ru.SetCellValue("DOB", sheet1, xlsRow, ref xlsCol, out ColDOB, 12);
                ru.SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out ColDOs, 12);
                ru.SetCellValue("Designation", sheet1, xlsRow, ref xlsCol, out ColGVDG, 20);

                ru.SetCellValue("Grade", sheet1, xlsRow, ref xlsCol, out ColGrade, 20);


                //SR to
                sheet1.Range[xlsRow, ColSr].Text = "Employee Information";
                sheet1.Range[xlsRow, ColSr, xlsRow, ColGrade].Merge();
                ColGrs = ColGrade;

                DataView dvSalaryHead = new DataView(dsSlrProc.Tables[0]);
                dvSalaryHead.Sort = "HeadType desc,Sequence";
                DataTable dtSalaryHead = dvSalaryHead.ToTable(true, "SalaryHeadID", "SalaryHead", "HeadType", "Sequence", "HeadCategory", "IntegerInDisb", "DecimalNo");

                int _count_earning_head = 0;
                int _count_earning_ctchead = 0;
                int _count_deducting_head = 0;
                int _total_head_count = 0;

                List<SalaryHeadSequence> list = null;

                CreateDynamicSHead(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out list);

                if (_count_earning_ctchead > 0)
                {
                    sheet1.Range[xlsRow, ColGrs + 1].Text = "Earning";
                    sheet1.Range[xlsRow, ColGrs + 1, xlsRow, ColGrs + _count_earning_ctchead].Merge();
                }

                int ds = ColGrs + _count_earning_ctchead + 1;

                if (_count_deducting_head > 0)
                {
                    sheet1.Range[xlsRow, ds].Text = "Deduction";
                    sheet1.Range[xlsRow, ds, xlsRow, ds + _count_deducting_head - 1].Merge();
                }
                int np = 0;
                if (list.Count > 0)
                {
                    xlsCol++;
                    np = ColGrs + list.Count + 1;
                    sheet1.Range[xlsRow, np].Text = "Net Payable";
                    sheet1.Range[xlsRow, np].ColumnWidth = 10;
                    sheet1.Range[xlsRow, np, xlsRow + 1, np].Merge();
                }

                xlsCol++;
                int MinWage = ColGrs + list.Count + 2;
                sheet1.Range[xlsRow, MinWage].Text = "Minimum Wage";
                sheet1.Range[xlsRow, MinWage].ColumnWidth = 10;
                sheet1.Range[xlsRow, MinWage, xlsRow + 1, MinWage].Merge();

                sheet1.Range[xlsRow, 1, xlsRow + 1, MinWage].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                sheet1.Range[xlsRow, 1, xlsRow + 1, MinWage].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow + 1, MinWage].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow + 1, MinWage].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow + 1, MinWage].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow + 1, MinWage].VerticalAlignment = ExcelVAlign.VAlignCenter;

                endXlsCol = MinWage;
                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                Param param = new Param();
                param.CompanyGroupId = companyGroupId;
                param.CompanyId = companyId;
                ru.Header(ref sheet1, param, endXlsCol, "Separated Employee Salary Information");

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------

                int SrNo = 0;
                string x = "";

                ReportUtility oRU = new ReportUtility();

                xlsRow = RowIndex;


                xlsRow--;
                //Test();
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    #region EmpInfo
                    SrNo += 1;
                    x = dtEmployees.Rows[i]["SystemID"].ToString().Trim();

                    //1
                    sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                    sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                        sheet1.Range[xlsRow, ColIDNo].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, ColIDNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColIDNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //3
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                        sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJ"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJ"].ToString();
                    sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //4.2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOB"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOB].Text = dtEmployees.Rows[i]["DOB"].ToString();
                    sheet1.Range[xlsRow, ColDOB].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOB].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4.2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOS"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOs].Text = dtEmployees.Rows[i]["DOS"].ToString();
                    sheet1.Range[xlsRow, ColDOs].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOs].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4.3
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Grade"].ToString()) == false)
                        sheet1.Range[xlsRow, ColGrade].Text = dtEmployees.Rows[i]["Grade"].ToString();
                    sheet1.Range[xlsRow, ColGrade].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColGrade].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LegalDesignation"].ToString()) == false)
                        sheet1.Range[xlsRow, ColGVDG].Text = dtEmployees.Rows[i]["LegalDesignation"].ToString();
                    sheet1.Range[xlsRow, ColGVDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColGVDG].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    #endregion

                    int _total_head_count_body = 0;

                    for (int ci = 0; ci < list.Count; ci++)
                    {
                        #region Head wise loop
                        var ob = list[ci];
                        if (ob.SalaryHead.Length > 0)
                        {
                            var hId = ob.SalaryHeadId;
                            _total_head_count_body++;

                            var _data = listdsSlrProc.Where(r => r.SalaryHeadID == hId && r.EmpInfoSystemID == x).FirstOrDefault();

                            if (_data != null)
                            {
                                sheet1.Range[xlsRow, ob.XLColIndex].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(_data.EntryAmount > 0 ? _data.EntryAmount.ToString() : (_data.EntryAmount * (-1)).ToString()));

                                sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = GetDecimalFormat(ob);// oRU.NumberFormatInt();
                                sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }
                        }
                        #endregion
                    }
                    DataView dvStructNetPay = new DataView(dsSlrProc.Tables[0]);
                    dvStructNetPay.RowFilter = "HeadCategory = 'Net Payable' and SystemID=" + x + "";
                    if (dvStructNetPay.Count > 0)
                    {
                        sheet1.Range[xlsRow, np].Number = Convert.ToDouble(dvStructNetPay[0]["EntryAmount"].ToString());

                    }
                    double _minWage = 0;
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SalaryHeadValue"].ToString()) == false)
                    {
                        _minWage = Convert.ToDouble(bplib.clsWebLib.GetNumData(dtEmployees.Rows[i]["SalaryHeadValue"].ToString()));
                    }

                    sheet1.Range[xlsRow, MinWage].Number = _minWage;
                    sheet1.Range[xlsRow, MinWage].NumberFormat = oRU.NumberFormatInt();
                    sheet1.Range[xlsRow, MinWage].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, MinWage].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow++;
                }//for emp count

                #endregion ----------------------Data-----------------------

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 7;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "SalaryInformation";
                #endregion

                workbook.Version = ExcelVersion.Excel2013;
                string strFileName = "SalaryInfo" + bplib.clsWebLib.DateData_DBToApp(DateTime.Now.Date, bplib.clsWebLib.STD_DATE_FORMAT).ToString("dd-MMM-yyyy") + ".xls";

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
            }
        }

        public IWorkbook GetEmployeeSalaryStructureWithProcessed(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            clsReport objRpt = null;

            try
            {
                #region Variable

                DataView dvEmp = null;
                DataSet dsCmp = null;
                DataSet dsFactory = null;
                //DataSet dsEmpAttdnInfo = null;
                //DataView dvLeaveEmp = null;
                DataSet dsLeaveInfo = null;
                DataSet dsSlrSheet = null;
                DataView dvSlrSheet = null;
                DataRow[] netPayRows;
                DataSet dsEmpLoyeeInfo = null;
                DataTable dtEmployees = null;

                ReportUtility ru = null;
                var FactoryName = string.Empty;
                var CmpName = string.Empty;

                int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
                var para = new ParamList();
                var leavePara = new ParamList();
                string strPath = "";
                var attdnProcessParam = new ParamList();
                Image companyLogo = null;
                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion Variable
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
                var ldateOfMonth = daysInMonth + "-" + monthName + "-" + year;
                var fdateOfMonth = "1" + "-" + monthName + "-" + year;

                ru = new ReportUtility();
                objRpt = new clsReport();

                #region DataSet

                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                objRpt.GetExtraAbsent(plantId, parameters, month.ToInt(), year.ToInt(), out dsExtraAbsent);

                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);

                //Sql Salary Structure 
                List<SalarySheetReportUD> listdsSlrStr = new List<SalarySheetReportUD>();

                //Sql Salary Process 

                List<SalarySheetReportUD> listdsSlrProc = new List<SalarySheetReportUD>();
                DataTable dtSalaryHeadSheet;
                List<SalarySheetReportUD> listdsSlrProcUd = new List<SalarySheetReportUD>();
                GetEmployeeInfoDetailSalaryLogWise(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, "", payRollGroup, parameters, isActive, isSeperated, isMaternity, out dsEmpLoyeeInfo);//Sql Query For Salary  Data
                Dictionary<string, List<DataRow>> dicEmpSalry = GetEmployeeSalaryInfoDetail(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, "", payRollGroup, parameters, out dtSalaryHeadSheet);

                if (dicEmpSalry.First().Value[0].Table.Rows.Count > 0)
                {
                    listdsSlrProc = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    listdsSlrStr = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    dtEmployees = dsEmpLoyeeInfo.Tables[0];
                }
                else
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }

                dvSlrSheet = new DataView();

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);

                objRpt.SelectedPlant(plantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 6;
                xlsCol = 1;

                var ColSr = 0;
                var ColIDNo = 0;
                var ColName = 0;
                var ColDOJ = 0;
                var ColDOS = 0;
                var cDept = 0;
                var cSec = 0;
                var cSubSec = 0;
                var cUnit = 0;
                var cLine = 0;
                var cPayrollGroup = 0;
                var cJoblocation = 0;
                var cGender = 0;
                var cPaymentMode = 0;
                var colEmpCurrentStat = 0;
                var colEmpStatus = 0;

                var cGrade = 0;
                var ColGVDG = 0;
                var ColSC = 0;
                var ColGrs = 0;
                var colPayDays = 0;
                var ColPdDy = 0;
                var ColLate = 0;
                var ColAbDy = 0;
                var ColHlDy = 0;
                var ColWkOf = 0;
                var ColLv = 0;
                var ColMLv = 0;

                var ColLWP = 0;
                var ColExtraAbsent = 0;
                int ColDMP = 0;
                int ColDMPCost = 0;
                int ColTotalOtHr = 0;

                //1
                SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);
                SetCellValue("ID No.", sheet1, xlsRow, ref xlsCol, out ColIDNo, 12);
                SetCellValue("Name", sheet1, xlsRow, ref xlsCol, out ColName, 17);
                SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out ColDOJ, 12);
                SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out ColDOS, 12);
                SetCellValue("Designation", sheet1, xlsRow, ref xlsCol, out ColGVDG, 25);

                SetCellValue("Employee Category", sheet1, xlsRow, ref xlsCol, out ColSC, 25);
                SetCellValue("Employee Current Status", sheet1, xlsRow, ref xlsCol, out colEmpCurrentStat, 25);
                SetCellValue("Employee Status", sheet1, xlsRow, ref xlsCol, out colEmpStatus, 25);
                SetCellValue("Department", sheet1, xlsRow, ref xlsCol, out cDept, 25);
                SetCellValue("Section", sheet1, xlsRow, ref xlsCol, out cSec, 25);
                SetCellValue("SubSection", sheet1, xlsRow, ref xlsCol, out cSubSec, 25);
                SetCellValue("Unit", sheet1, xlsRow, ref xlsCol, out cUnit, 25);
                SetCellValue("Line", sheet1, xlsRow, ref xlsCol, out cLine, 25);
                SetCellValue("Payroll Group", sheet1, xlsRow, ref xlsCol, out cPayrollGroup, 25);
                SetCellValue("Job Location", sheet1, xlsRow, ref xlsCol, out cJoblocation, 25);
                SetCellValue("Gender", sheet1, xlsRow, ref xlsCol, out cGender, 25);
                SetCellValue("Payment Mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                SetCellValue("Grade", sheet1, xlsRow, ref xlsCol, out cGrade, 25);
                //SetCellValue("Direct Manpower", sheet1, xlsRow, ref xlsCol, out ColDMP, 25);
                SetCellValue("Direct Manpower Cost", sheet1, xlsRow, ref xlsCol, out ColDMPCost, 25);
                SetCellValue("Pay Days", sheet1, xlsRow, ref xlsCol, out colPayDays, 5);
                SetCellValue("Present", sheet1, xlsRow, ref xlsCol, out ColPdDy, 9);
                SetCellValue("Late", sheet1, xlsRow, ref xlsCol, out ColLate, 9);
                SetCellValue("Absent(Inc.Ext)", sheet1, xlsRow, ref xlsCol, out ColAbDy, 13);
                SetCellValue("LWP", sheet1, xlsRow, ref xlsCol, out ColLWP, 9);
                SetCellValue("Extra Absent", sheet1, xlsRow, ref xlsCol, out ColExtraAbsent, 9);
                SetCellValue("Holiday", sheet1, xlsRow, ref xlsCol, out ColHlDy, 9);
                SetCellValue("WeekOff", sheet1, xlsRow, ref xlsCol, out ColWkOf, 9);
                SetCellValue("Leave", sheet1, xlsRow, ref xlsCol, out ColLv, 11);
                SetCellValue("Maternity Leave", sheet1, xlsRow, ref xlsCol, out ColMLv, 17);
                SetCellValue("Total OT Hr", sheet1, xlsRow, ref xlsCol, out ColTotalOtHr, 25);

                //SR to
                sheet1.Range[xlsRow, ColSr].Text = "Employee Information";
                sheet1.Range[xlsRow, ColSr, xlsRow, ColTotalOtHr].Merge();
                //xlsCol += 1;
                ColGrs = ColTotalOtHr;
                // 9

                var _count_earning_head = 0;
                var _count_earning_ctchead = 0;
                var _count_deducting_head = 0;
                var _total_head_count = 0;
                List<SalaryHeadSequence> strList = null;

                Dictionary<string, SalaryHeadSequence> strListNew = null;

                CreateDynamicSHead(dtSalaryHeadSheet, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out strListNew);

                xlsCol--;

                //Header Col
                if (_count_earning_ctchead > 0)
                {
                    sheet1.Range[xlsRow, ColGrs + 1].Text = "Earning head";
                    sheet1.Range[xlsRow, ColGrs + 1, xlsRow, ColGrs + _count_earning_head + _count_earning_ctchead].Merge();
                }

                var ds = ColGrs + 1 + _count_earning_head + _count_earning_ctchead;

                if (_count_deducting_head > 0)
                {
                    sheet1.Range[xlsRow, ds].Text = "Deduction head";
                    sheet1.Range[xlsRow, ds, xlsRow, ds + _count_deducting_head - 1].Merge();
                }
                var npstruct = 0;
                if (strListNew.Count > 0)
                {
                    xlsCol++;
                    npstruct = ColGrs + strListNew.Count + 1;
                    sheet1.Range[xlsRow, npstruct].Text = "Net Payable";
                    sheet1.Range[xlsRow, npstruct].ColumnWidth = 14;
                    sheet1.Range[xlsRow, npstruct, xlsRow + 1, npstruct].Merge();
                }

                xlsCol++;
                var MinWage = ColGrs + strListNew.Count + 2;
                sheet1.Range[xlsRow, MinWage].Text = "Minimum Wage";
                sheet1.Range[xlsRow, MinWage].ColumnWidth = 14;
                sheet1.Range[xlsRow, MinWage, xlsRow + 1, MinWage].Merge();

                sheet1.Range[xlsRow - 1, ColGrs].Text = "Salary Structure";
                sheet1.Range[xlsRow - 1, ColGrs].ColumnWidth = 14;
                sheet1.Range[xlsRow - 1, ColGrs, xlsRow - 1, MinWage].Merge();
                var _count_earning_head_sheet = 0;
                var _count_earning_ctchead_sheet = 0;
                var _count_deducting_head_sheet = 0;
                var _total_head_count_sheet = 0;

                Dictionary<string, SalaryHeadSequence> shtListNew = null;

                CreateDynamicSHead(dtSalaryHeadSheet, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref MinWage, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out shtListNew);

                if (_count_earning_ctchead_sheet > 0)
                {
                    sheet1.Range[xlsRow, MinWage + 1].Text = "Earning Head";
                    sheet1.Range[xlsRow, MinWage + 1, xlsRow, MinWage + _count_earning_head_sheet + _count_earning_ctchead_sheet].Merge();
                }

                var dsheet = MinWage + 1 + _count_earning_head_sheet + _count_earning_ctchead_sheet;

                if (_count_deducting_head_sheet > 0)
                {
                    sheet1.Range[xlsRow, dsheet].Text = "Deduction Head";
                    sheet1.Range[xlsRow, dsheet, xlsRow, dsheet + _count_deducting_head_sheet - 1].Merge();
                }
                var npSheet = 0;
                if (shtListNew.Count > 0)
                {
                    xlsCol++;
                    npSheet = MinWage + shtListNew.Count + 1;
                    sheet1.Range[xlsRow, npSheet].Text = "Net Payable";
                    sheet1.Range[xlsRow, npSheet].ColumnWidth = 14;
                    sheet1.Range[xlsRow, npSheet, xlsRow + 1, npSheet].Merge();
                }

                xlsCol++;
                sheet1.Range[xlsRow - 1, MinWage + 1].Text = "Salary Sheet";
                sheet1.Range[xlsRow - 1, MinWage + 1].ColumnWidth = 14;
                sheet1.Range[xlsRow - 1, MinWage + 1, xlsRow - 1, npSheet].Merge();
                endXlsCol = npSheet;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


                #endregion------------------Column Header------------------
                try
                {

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
                catch (Exception ex)
                {
                }
                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 3;
                Param param = new Param();
                param.CompanyGroupId = companyGroupId;
                param.CompanyId = companyId;

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
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 30;
                sheet1.Range[xlsRow, 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 2, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

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
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Salary Structure And Sheet";
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                var strRptDateRange = "";
                strRptDateRange = "For The Month Of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                sheet1.Range[xlsRow, xlsCol].Text = strRptDateRange;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 1].Text = "Report Ref No.";
                sheet1.Range[xlsRow, 1, xlsRow, 3].Merge();
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                var SrNo = 0;
                var x = "";
                decimal ColGrsSlr = 0;
                decimal ColCTCSlr = 0;
                var oRU = new ReportUtility();

                xlsRow = RowIndex;

                xlsRow--;

                objRpt.GetEmpLeaveInfo(month, year, out dsLeaveInfo);
                List<ReportLeaveInfo> LINF = new List<ReportLeaveInfo>();
                LINF = dsLeaveInfo.Tables[0].ToList<ReportLeaveInfo>();

                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    //xlsRow++;
                    #region EmpInfo
                    SrNo += 1;
                    x = dtEmployees.Rows[i]["EmpSystemID"].ToString().Trim();
                    ColGrsSlr = 0;
                    //1
                    sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                    sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                        sheet1.Range[xlsRow, ColIDNo].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, ColIDNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColIDNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //3
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                        sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJ"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJ"].ToString();
                    sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOS"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOS].Text = dtEmployees.Rows[i]["DOS"].ToString();
                    sheet1.Range[xlsRow, ColDOS].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOS].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString()) == false)
                        sheet1.Range[xlsRow, colEmpCurrentStat].Text = dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString();
                    sheet1.Range[xlsRow, colEmpCurrentStat].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, colEmpCurrentStat].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeStatus"].ToString()) == false)
                        sheet1.Range[xlsRow, colEmpStatus].Text = dtEmployees.Rows[i]["EmployeeStatus"].ToString();
                    sheet1.Range[xlsRow, colEmpStatus].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, colEmpStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LegalDesignation"].ToString()) == false)
                        sheet1.Range[xlsRow, ColGVDG].Text = dtEmployees.Rows[i]["LegalDesignation"].ToString();
                    sheet1.Range[xlsRow, ColGVDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColGVDG].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmpCategoryName"].ToString()) == false)// EmployeeCategory Need to Make Correct
                        sheet1.Range[xlsRow, ColSC].Text = dtEmployees.Rows[i]["EmpCategoryName"].ToString();
                    sheet1.Range[xlsRow, ColSC].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColSC].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4.2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DepartmentName"].ToString()) == false)
                        sheet1.Range[xlsRow, cDept].Text = dtEmployees.Rows[i]["DepartmentName"].ToString();
                    sheet1.Range[xlsRow, cDept].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, cDept].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SectionName"].ToString()) == false)
                        sheet1.Range[xlsRow, cSec].Text = dtEmployees.Rows[i]["SectionName"].ToString();
                    sheet1.Range[xlsRow, cSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, cSec].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LineName"].ToString()) == false)
                        sheet1.Range[xlsRow, cLine].Text = dtEmployees.Rows[i]["LineName"].ToString();
                    sheet1.Range[xlsRow, cLine].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, cLine].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PayRollGroup"].ToString()) == false)
                        sheet1.Range[xlsRow, cPayrollGroup].Text = dtEmployees.Rows[i]["PayRollGroup"].ToString();
                    sheet1.Range[xlsRow, cPayrollGroup].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, cPayrollGroup].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["JobLocation"].ToString()) == false)
                        sheet1.Range[xlsRow, cJoblocation].Text = dtEmployees.Rows[i]["JobLocation"].ToString();
                    sheet1.Range[xlsRow, cJoblocation].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, cJoblocation].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SubSectionName"].ToString()) == false)
                        sheet1.Range[xlsRow, cSubSec].Text = dtEmployees.Rows[i]["SubSectionName"].ToString();
                    sheet1.Range[xlsRow, cSubSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, cSubSec].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["UnitName"].ToString()) == false)
                        sheet1.Range[xlsRow, cUnit].Text = dtEmployees.Rows[i]["UnitName"].ToString();
                    sheet1.Range[xlsRow, cUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, cUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PaymentMode"].ToString()) == false)
                        sheet1.Range[xlsRow, cPaymentMode].Text = dtEmployees.Rows[i]["PaymentMode"].ToString();
                    sheet1.Range[xlsRow, cPaymentMode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, cPaymentMode].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //5
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["GradeCode"].ToString()) == false)
                        sheet1.Range[xlsRow, cGrade].Text = dtEmployees.Rows[i]["GradeCode"].ToString();
                    sheet1.Range[xlsRow, cGrade].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, cGrade].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                        sheet1.Range[xlsRow, cGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                    sheet1.Range[xlsRow, cGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, cGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //if (string.IsNullOrEmpty(dtEmployees.Rows[i]["IsDirect"].ToString()) == false)
                    //    sheet1.Range[xlsRow, ColDMP].Text = dtEmployees.Rows[i]["IsDirect"].ToString();
                    //sheet1.Range[xlsRow, ColDMP].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    //sheet1.Range[xlsRow, ColDMP].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DirectManpowerCost"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDMPCost].Text = dtEmployees.Rows[i]["DirectManpowerCost"].ToString();
                    sheet1.Range[xlsRow, ColDMPCost].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDMPCost].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //5 "Section", "SubSection", 

                    #endregion
                    #region Attendance Data

                    double _ExtraAbsent = 0;
                    dvExtraAbsent.RowFilter = "EmpSystemID='" + dtEmployees.Rows[i]["EmpSystemID"].ToString() + "' ";
                    _ExtraAbsent = dvExtraAbsent.Count;

                    var payDays = 0.00;
                    // clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"]);
                    if (!String.IsNullOrEmpty(dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper()))
                    {
                        if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOffAndHoliday.ToString().ToUpper())
                        {
                            payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalHoliDay"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString());

                        }
                        if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOff.ToString().ToUpper())
                        {
                            payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString());
                        }
                    }
                    else
                    {
                        payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString());
                    }

                    SetCellTextAttdn(sheet1, xlsRow, colPayDays, payDays);
                    SetCellTextAttdn(sheet1, xlsRow, ColPdDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalPresent"].ToString()));
                    SetCellTextAttdn(sheet1, xlsRow, ColLate, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLate"].ToString()));
                    SetCellTextAttdn(sheet1, xlsRow, ColAbDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()));
                    SetCellTextAttdn(sheet1, xlsRow, ColLWP, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLWP"].ToString()));
                    SetCellTextAttdn(sheet1, xlsRow, ColExtraAbsent, _ExtraAbsent);
                    SetCellTextAttdn(sheet1, xlsRow, ColHlDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalHoliDay"].ToString()));
                    SetCellTextAttdn(sheet1, xlsRow, ColWkOf, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString()));
                    SetCellTextAttdn(sheet1, xlsRow, ColLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLv"].ToString()));
                    SetCellTextAttdn(sheet1, xlsRow, ColMLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalMLv"].ToString()));

                    SetCellTextDR(sheet1, xlsRow, ColTotalOtHr, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalOTHr"].ToString()) / 60);

                    //}
                    #endregion
                    try
                    {
                        if (dicEmpSalry.ContainsKey(dtEmployees.Rows[i]["EmpSystemID"].ToString()))
                        {

                            List<DataRow> drSalaryHeadCollection = dicEmpSalry[dtEmployees.Rows[i]["EmpSystemID"].ToString()];
                            for (int CI = 0; CI < drSalaryHeadCollection.Count; CI++)
                            {
                                if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "NET PAYABLE")
                                {
                                    sheet1.Range[xlsRow, npstruct].Number = Convert.ToDouble(drSalaryHeadCollection[CI]["EntryAmount"].ToString());
                                    continue;
                                }
                                try
                                {
                                    SalaryHeadSequence xx = strListNew[drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();
                                    if (xx != null)
                                    {
                                        if (drSalaryHeadCollection[CI]["HeadType"].ToString() == "D")
                                        {
                                            sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["EntryAmount"].ToString());// * (-1);
                                        }

                                        else
                                        {
                                            sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["EntryAmount"].ToString());
                                        }

                                        sheet1.Range[xlsRow, xx.XLColIndex].NumberFormat = oRU.NumberFormatInt();
                                        sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                        sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }
                                }
                                catch (Exception ex)
                                {

                                    throw ex;
                                }

                            }
                            double _minWage = 0;
                            _minWage = Convert.ToDouble(dtEmployees.Rows[i]["MinimumWage"].ToString());
                            sheet1.Range[xlsRow, MinWage].Number = _minWage;
                            sheet1.Range[xlsRow, MinWage].NumberFormat = oRU.NumberFormatInt();
                            sheet1.Range[xlsRow, MinWage].HorizontalAlignment = ExcelHAlign.HAlignRight;
                            sheet1.Range[xlsRow, MinWage].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            #region ------------------------------------Salary Sheet----------------------------------


                            for (int CI = 0; CI < drSalaryHeadCollection.Count; CI++)
                            {
                                if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "NET PAYABLE")
                                {
                                    sheet1.Range[xlsRow, npSheet].Number = Convert.ToDouble(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                    continue;
                                }
                                try
                                {
                                    SalaryHeadSequence xx = shtListNew[drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();
                                    if (xx != null)
                                    {
                                        if (drSalaryHeadCollection[CI]["HeadType"].ToString() == "D")
                                        {
                                            sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()) * (-1);
                                        }

                                        else
                                        {
                                            sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                        }

                                        sheet1.Range[xlsRow, xx.XLColIndex].NumberFormat = oRU.NumberFormatInt();
                                        sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                        sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }
                                }
                                catch (Exception ex)
                                {

                                    throw ex;
                                }

                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    sheet1.Range[xlsRow, npSheet].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, npSheet].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    #endregion

                    xlsRow++;
                }//for emp count

                #endregion ----------------------Data-----------------------

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var freezePan = RowIndex - 1;
                sheet1.UsedRange["A" + freezePan].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 10;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "EmpSalaryInfo";
                #endregion

                workbook.Version = ExcelVersion.Excel2016;
                var strFileName = "EmpSalaryStrSheet-" + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + Convert.ToDateTime(fdateOfMonth).ToString("yyyy") + "-" + para.SalaryProcessId + ".xls";

                return workbook;
                //}
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
            }
        }

        public IWorkbook GetEmployeeSalaryProcessedReport(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool sa, bool ca, bool isTopSheet)
        {
            #region Variable
            clsReport objRpt = null;
            DataView dvEmp = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsEmpLoyeeInfo = null;
            DataTable dtEmployees = null;

            DataView dvSlrSheet = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            int endGenericColumn = 0;
            #endregion Variable

            try
            {
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
                var ldateOfMonth = daysInMonth + "-" + monthName + "-" + year;
                var fdateOfMonth = "1" + "-" + monthName + "-" + year;
                string strPath = "";
                Image companyLogo = null;
                string companyLogoName = _sqlRepository.GetDataTable(@"select * from ORG.Company where Id = '" + companyId + @"'").Rows[0]["Image"].ToString();

                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyLogoName);  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();

                #endregion Variable

                #region DataSet

                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                objRpt.GetExtraAbsent(plantId, parameters, month.ToInt(), year.ToInt(), out dsExtraAbsent);

                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);

                //Sql Salary Structure 
                List<SalarySheetReportUD> listdsSlrStr = new List<SalarySheetReportUD>();

                //Sql Salary Process 
                DataTable dtSalaryHeadSheet;
                List<SalarySheetReportUD> listdsSlrProc = new List<SalarySheetReportUD>();
                GetEmployeeInfoDetail(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, sa, ca, userId, out dsEmpLoyeeInfo);//Sql Query For Salary  Data
                Dictionary<string, List<DataRow>> dicEmpSalry = GetEmployeeSalaryInfoDetail(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, salaryProcessId, payRollGroup, parameters, out dtSalaryHeadSheet);

                if (dicEmpSalry.First().Value[0].Table.Rows.Count > 0)
                {
                    listdsSlrProc = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    listdsSlrStr = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    dtEmployees = dsEmpLoyeeInfo.Tables[0];//dicEmpSalry.First().Value[0].Table;
                }
                else
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }

                dvSlrSheet = new DataView();

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);

                objRpt.SelectedPlant(plantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                if (isTopSheet == true)
                {
                    workbook = application.Workbooks.Create(5);
                }
                else
                {
                    workbook = application.Workbooks.Create(1);
                }

                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 6;
                xlsCol = 1;

                #region Column Variables
                int ColSr = 0, ColIDNo = 0, ColName = 0, ColDOJ = 0, ColDOS = 0, cDept = 0, cSec = 0, cSubSec = 0, cLine = 0, cPayrollGroup = 0, cJobLocation = 0, cGender = 0,
                    cGrade = 0, ColGVDG = 0, ColGrs = 0, colPayDays = 0, ColPdDy = 0, ColLate = 0, ColAbDy = 0, ColHlDy = 0, ColWkOf = 0, ColLv = 0, ColMLv = 0, colBank = 0, colBankAccountNo = 0
                   , ColLWP = 0, cDMP = 0, ColExtraAbsent = 0, colEmpCurrentStat = 0, colEmpStatus = 0, cPaymentMode = 0, cUnit = 0, ColTotalOTHR = 0, colDirectManpowerCost = 0;
                int npstruct = 0;

                #endregion

                //1
                SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);
                SetCellValue("ID No.", sheet1, xlsRow, ref xlsCol, out ColIDNo, 12);
                SetCellValue("Name", sheet1, xlsRow, ref xlsCol, out ColName, 17);
                SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out ColDOJ, 12);
                SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out ColDOS, 12);
                SetCellValue("EmployeeCurrentStatus", sheet1, xlsRow, ref xlsCol, out colEmpCurrentStat, 12);
                SetCellValue("EmployeeSatatus", sheet1, xlsRow, ref xlsCol, out colEmpStatus, 12);
                SetCellValue("Gender", sheet1, xlsRow, ref xlsCol, out cGender, 12);
                SetCellValue("Designation", sheet1, xlsRow, ref xlsCol, out ColGVDG, 25);
                SetCellValue("Employee Category", sheet1, xlsRow, ref xlsCol, out int colEmpCategory, 25);
                SetCellValue("Department", sheet1, xlsRow, ref xlsCol, out cDept, 25);
                SetCellValue("Section", sheet1, xlsRow, ref xlsCol, out cSec, 25);
                SetCellValue("SubSection", sheet1, xlsRow, ref xlsCol, out cSubSec, 25);
                SetCellValue("Unit", sheet1, xlsRow, ref xlsCol, out cUnit, 25);
                SetCellValue("Line", sheet1, xlsRow, ref xlsCol, out cLine, 25);
                SetCellValue("JobLocation", sheet1, xlsRow, ref xlsCol, out cJobLocation, 25);
                SetCellValue("Payroll group", sheet1, xlsRow, ref xlsCol, out cPayrollGroup, 25);
                SetCellValue("Payment mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                SetCellValue("Bank", sheet1, xlsRow, ref xlsCol, out colBank, 25);
                SetCellValue("Bank Acc No.", sheet1, xlsRow, ref xlsCol, out colBankAccountNo, 25);
                SetCellValue("Grade", sheet1, xlsRow, ref xlsCol, out cGrade, 25);
                SetCellValue("Direct Manpower Cost", sheet1, xlsRow, ref xlsCol, out colDirectManpowerCost, 25);

                SetCellValue("Pay Days", sheet1, xlsRow, ref xlsCol, out colPayDays, 5);
                SetCellValue("Present", sheet1, xlsRow, ref xlsCol, out ColPdDy, 9);
                SetCellValue("Late", sheet1, xlsRow, ref xlsCol, out ColLate, 9);
                SetCellValue("Absent", sheet1, xlsRow, ref xlsCol, out ColAbDy, 9);
                SetCellValue("LWP", sheet1, xlsRow, ref xlsCol, out ColLWP, 9);
                SetCellValue("Extra Absent", sheet1, xlsRow, ref xlsCol, out ColExtraAbsent, 9);
                SetCellValue("Holiday", sheet1, xlsRow, ref xlsCol, out ColHlDy, 9);
                SetCellValue("WeekOff", sheet1, xlsRow, ref xlsCol, out ColWkOf, 9);
                SetCellValue("Leave", sheet1, xlsRow, ref xlsCol, out ColLv, 11);
                SetCellValue("Maternity Leave", sheet1, xlsRow, ref xlsCol, out ColMLv, 20);
                SetCellValue("Total Ot Hr", sheet1, xlsRow, ref xlsCol, out ColTotalOTHR, 11);
                endGenericColumn = xlsCol;

                //SR to
                sheet1.Range[xlsRow, ColSr].Text = "Employee Information";
                sheet1.Range[xlsRow, ColSr, xlsRow, ColTotalOTHR].Merge();
                //xlsCol += 1;
                ColGrs = ColTotalOTHR;
                // 9

                int _count_earning_head = 0;
                int _count_earning_ctchead = 0;
                int _count_deducting_head = 0;
                int _total_head_count = 0;
                double totalBankPayDisbusmentAmount = 0.00;
                double totalCashPayDisbusmentAmount = 0.00;

                Dictionary<string, SalaryHeadSequence> shtList = null;

                CreateDynamicSHead(dtSalaryHeadSheet, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out shtList);




                List<SalaryHeadSequence> salList = new List<SalaryHeadSequence>();
                salList.AddRange(shtList.Values);

                xlsCol--;

                //Header Col
                if (_count_earning_ctchead > 0)
                {
                    sheet1.Range[xlsRow, ColGrs + 1].Text = "Earning head";
                    sheet1.Range[xlsRow, ColGrs + 1, xlsRow, ColGrs + _count_earning_head + _count_earning_ctchead].Merge();
                }

                var ds = ColGrs + 1 + _count_earning_head + _count_earning_ctchead;

                if (_count_deducting_head > 0)
                {
                    sheet1.Range[xlsRow, ds].Text = "Deduction head";
                    sheet1.Range[xlsRow, ds, xlsRow, ds + _count_deducting_head - 1].Merge();
                }
                npstruct = 0;
                //int endxlsCol = 0;
                if (shtList.Count > 0)
                {
                    xlsCol++;
                    npstruct = ColGrs + shtList.Count + 1;
                    sheet1.Range[xlsRow + 1, npstruct].Text = "Net Payable";
                }
                endXlsCol = npstruct;

                int colBankPaymentPercentage = 0;
                int colCashPaymentPercentage = 0;

                DataTable dtbankCash = _sqlRepository.GetDataTable("SELECT * FROM EmployeeWiseBankCashAmount WHERE PlantId = '" + plantId + "' AND MonthNo = '" + month + @"' AND YearNo  ='" + year + @"'");


                if (dtbankCash.Rows.Count > 0)
                {
                    xlsCol++;

                    colBankPaymentPercentage = npstruct + 1;
                    sheet1.Range[xlsRow + 1, colBankPaymentPercentage].Text = "Bank";
                    sheet1.Range[xlsRow + 1, colBankPaymentPercentage].ColumnWidth = 10;
                    sheet1.Range[xlsRow + 1, colBankPaymentPercentage].CellStyle.Font.Size = 8;
                    xlsCol++;
                    colCashPaymentPercentage = colBankPaymentPercentage + 1;
                    sheet1.Range[xlsRow + 1, colCashPaymentPercentage].Text = "Cash";
                    sheet1.Range[xlsRow + 1, colCashPaymentPercentage].ColumnWidth = 10;
                    sheet1.Range[xlsRow + 1, colCashPaymentPercentage].CellStyle.Font.Size = 8;

                    endXlsCol = colCashPaymentPercentage;
                }

                xlsCol++;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No.";
                sheet1.Range[xlsRow - 1, 1].ColumnWidth = 14;
                sheet1.Range[xlsRow - 1, 1, xlsRow - 1, 3].Merge();
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //endXlsCol = endxlsCol;


                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                Param param = new Param();
                param.CompanyGroupId = companyGroupId;
                param.CompanyId = companyId;

                string FactoryAddress = string.Empty;
                try
                {

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
                catch (Exception ex)
                {
                }


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
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
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
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Salary Sheet For The Month Of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                var SrNo = 0;
                var x = "";

                var oRU = new ReportUtility();

                xlsRow = RowIndex;

                xlsRow--;
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    #region EmpInfo
                    try
                    {
                        SrNo += 1;
                        x = dtEmployees.Rows[i]["EmpSystemID"].ToString().Trim();

                        //1
                        sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                        sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //2
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, ColIDNo].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, ColIDNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColIDNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //3
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                            sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJ"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJ"].ToString();
                        sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOS"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOS].Text = dtEmployees.Rows[i]["DOS"].ToString();
                        sheet1.Range[xlsRow, ColDOS].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOS].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString()) == false)
                            sheet1.Range[xlsRow, colEmpCurrentStat].Text = dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString();
                        sheet1.Range[xlsRow, colEmpCurrentStat].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCurrentStat].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeStatus"].ToString()) == false)
                            sheet1.Range[xlsRow, colEmpStatus].Text = dtEmployees.Rows[i]["EmployeeStatus"].ToString();
                        sheet1.Range[xlsRow, colEmpStatus].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LegalDesignation"].ToString()) == false)
                            sheet1.Range[xlsRow, ColGVDG].Text = dtEmployees.Rows[i]["LegalDesignation"].ToString();
                        sheet1.Range[xlsRow, ColGVDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColGVDG].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmpCategoryName"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colEmpCategory].Text = dtEmployees.Rows[i]["EmpCategoryName"].ToString();
                        sheet1.Range[xlsRow, colEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4.2
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DepartmentName"].ToString()) == false)
                            sheet1.Range[xlsRow, cDept].Text = dtEmployees.Rows[i]["DepartmentName"].ToString();
                        sheet1.Range[xlsRow, cDept].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cDept].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SectionName"].ToString()) == false)
                            sheet1.Range[xlsRow, cSec].Text = dtEmployees.Rows[i]["SectionName"].ToString();
                        sheet1.Range[xlsRow, cSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cSec].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SubSectionName"].ToString()) == false)
                            sheet1.Range[xlsRow, cSubSec].Text = dtEmployees.Rows[i]["SubSectionName"].ToString();
                        sheet1.Range[xlsRow, cSubSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cSubSec].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["UnitName"].ToString()) == false)
                            sheet1.Range[xlsRow, cUnit].Text = dtEmployees.Rows[i]["UnitName"].ToString();
                        sheet1.Range[xlsRow, cUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PaymentMode"].ToString()) == false)
                            sheet1.Range[xlsRow, cPaymentMode].Text = dtEmployees.Rows[i]["PaymentMode"].ToString();
                        sheet1.Range[xlsRow, cPaymentMode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cPaymentMode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                            sheet1.Range[xlsRow, cGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                        sheet1.Range[xlsRow, cGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["JobLocation"].ToString()) == false)
                            sheet1.Range[xlsRow, cJobLocation].Text = dtEmployees.Rows[i]["JobLocation"].ToString();
                        sheet1.Range[xlsRow, cJobLocation].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cJobLocation].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LineName"].ToString()) == false)
                            sheet1.Range[xlsRow, cLine].Text = dtEmployees.Rows[i]["LineName"].ToString();
                        sheet1.Range[xlsRow, cLine].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cLine].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PayRollGroup"].ToString()) == false)
                            sheet1.Range[xlsRow, cPayrollGroup].Text = dtEmployees.Rows[i]["PayRollGroup"].ToString();
                        sheet1.Range[xlsRow, cPayrollGroup].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cPayrollGroup].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //5
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["GradeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, cGrade].Text = dtEmployees.Rows[i]["GradeCode"].ToString();
                        sheet1.Range[xlsRow, cGrade].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGrade].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["BankName"].ToString()) == false)
                            sheet1.Range[xlsRow, colBank].Text = dtEmployees.Rows[i]["BankName"].ToString();
                        sheet1.Range[xlsRow, colBank].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBank].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["BankAccNo"].ToString()) == false)
                            sheet1.Range[xlsRow, colBankAccountNo].Text = dtEmployees.Rows[i]["BankAccNo"].ToString();
                        sheet1.Range[xlsRow, colBankAccountNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBankAccountNo].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DirectManpowerCost"].ToString()) == false)
                            sheet1.Range[xlsRow, colDirectManpowerCost].Text = dtEmployees.Rows[i]["DirectManpowerCost"].ToString();
                        sheet1.Range[xlsRow, colDirectManpowerCost].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colDirectManpowerCost].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                            sheet1.Range[xlsRow, cGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                        sheet1.Range[xlsRow, cGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGender].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        #endregion
                        #region Attendance Data
                        double _ExtraAbsent = 0;
                        dvExtraAbsent.RowFilter = "EmpSystemID='" + dtEmployees.Rows[i]["EmpSystemID"].ToString() + "' ";
                        _ExtraAbsent = dvExtraAbsent.Count;
                        var payDays = 0.00;// clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString());
                        if (!String.IsNullOrEmpty(dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper()))
                        {
                            if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOffAndHoliday.ToString().ToUpper())
                            {
                                payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalHoliDay"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString());
                            }
                            if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOff.ToString().ToUpper())
                            {
                                payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString());
                            }
                        }
                        else
                        {
                            payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString());
                        }

                        SetCellTextAttdn(sheet1, xlsRow, colPayDays, payDays);
                        SetCellTextAttdn(sheet1, xlsRow, ColPdDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalPresent"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLate, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLate"].ToString()));
                        SetCellTextNumber(sheet1, xlsRow, ColAbDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLWP"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLWP, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLWP"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColExtraAbsent, _ExtraAbsent);
                        SetCellTextAttdn(sheet1, xlsRow, ColHlDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalHoliDay"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColWkOf, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLv"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColMLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalMLv"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColTotalOTHR, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalOTHr"].ToString()) / 60);

                        //}
                        #endregion

                        #region ------------------------------------Salary Sheet----------------------------------
                        if (dicEmpSalry.ContainsKey(dtEmployees.Rows[i]["EmpSystemID"].ToString()))
                        {
                            List<DataRow> drSalaryHeadCollection = dicEmpSalry[dtEmployees.Rows[i]["EmpSystemID"].ToString()];
                            if (drSalaryHeadCollection.Count > 0)
                            {
                                for (int CI = 0; CI < drSalaryHeadCollection.Count; CI++)
                                {
                                    if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "NET PAYABLE")
                                    {
                                        sheet1.Range[xlsRow, npstruct].Number = Convert.ToDouble(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                        continue;
                                    }
                                    try
                                    {
                                        SalaryHeadSequence xx = shtList[drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();
                                        if (xx != null)
                                        {
                                            if (drSalaryHeadCollection[CI]["HeadType"].ToString() == "D")
                                            {
                                                sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()) * (-1);
                                            }

                                            else
                                            {

                                                sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                            }

                                            sheet1.Range[xlsRow, xx.XLColIndex].NumberFormat = oRU.NumberFormatInt();
                                            sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                            sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                        throw ex;
                                    }
                                    if (dtbankCash.Rows.Count > 0)
                                    {
                                        dtbankCash.DefaultView.RowFilter = "EmpSystemId = '" + dtEmployees.Rows[i]["EmpsystemId"].ToString() + @"'";

                                        if (dtbankCash.DefaultView.Count > 0)
                                        {
                                            sheet1.Range[xlsRow, colBankPaymentPercentage, xlsRow, colBankPaymentPercentage].Number = Convert.ToDouble(dtbankCash.DefaultView[0]["BankAmount"].ToString());
                                            sheet1.Range[xlsRow, colBankPaymentPercentage, xlsRow, colBankPaymentPercentage].NumberFormat = ru.GetDecimalFormatlocalNetPay(Convert.ToBoolean(drSalaryHeadCollection[CI]["IntegerInDisb"].ToString()), Convert.ToInt32(drSalaryHeadCollection[CI]["DecimalNo"].ToString()), "");
                                            sheet1.Range[xlsRow, colBankPaymentPercentage].CellStyle.Font.Size = 34;

                                            sheet1.Range[xlsRow, colCashPaymentPercentage, xlsRow, colCashPaymentPercentage].Number = Convert.ToDouble(dtbankCash.DefaultView[0]["CashAmount"].ToString());
                                            sheet1.Range[xlsRow, colCashPaymentPercentage, xlsRow, colCashPaymentPercentage].NumberFormat = ru.GetDecimalFormatlocalNetPay(Convert.ToBoolean(drSalaryHeadCollection[CI]["IntegerInDisb"].ToString()), Convert.ToInt32(drSalaryHeadCollection[CI]["DecimalNo"].ToString()), "");
                                            sheet1.Range[xlsRow, colCashPaymentPercentage].CellStyle.Font.Size = 34;

                                            totalBankPayDisbusmentAmount += clsStaticInfo.dbl(dtbankCash.DefaultView[0]["BankAmount"].ToString());
                                            totalCashPayDisbusmentAmount += clsStaticInfo.dbl(dtbankCash.DefaultView[0]["CashAmount"].ToString());
                                        }

                                    }

                                }

                            }
                        }

                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }


                    #endregion

                    xlsRow++;
                }//for emp count
                int sheetEndXlsRow = xlsRow - 1;
                #endregion ----------------------Data-----------------------

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var freezePan = RowIndex - 1;
                sheet1.UsedRange["A" + freezePan].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 10;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "EmpSalaryInfo";
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                #endregion

                workbook.Version = ExcelVersion.Excel2016;

                if (isTopSheet == true)
                {
                    #region Salary Summary
                    string filePath = HostingEnvironment.MapPath("~/") + "TempSalaeySummary.xlsx";
                    workbook.SaveAs(filePath);
                    workbook = application.Workbooks.Open(filePath);

                    IWorksheet worksheet = workbook.Worksheets[0];
                    worksheet.Move(4);

                    #region PivotSheet 1 EmployeeStatus, PaymentMode, Department   
                    IWorksheet pivotSheet = workbook.Worksheets[0];
                    pivotSheet.Name = "Summary 1";

                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = pivotSheet.GetColumnWidth(1) + pivotSheet.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((pivotSheet.GetRowHeight(1) + pivotSheet.GetRowHeight(2) + pivotSheet.GetRowHeight(3) + pivotSheet.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = pivotSheet.Pictures.AddPicture(1, 1, companyLogo);

                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    #region Report Header
                    xlsRow = 1;
                    xlsCol = 1;


                    pivotSheet.Range[xlsRow, 3].Text = CmpName;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow += 1;

                    pivotSheet.Range[xlsRow, 3].Text = FactoryName;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    xlsRow += 1;

                    pivotSheet.Range[xlsRow, 3].Text = FactoryAddress;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    xlsRow += 1;
                    pivotSheet.Range[xlsRow, 3].Text = "Salary Summary for the month of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    // pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    #endregion

                    pivotSheet.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;
                    int tableColRange = 0;
                    if (dtbankCash.Rows.Count > 0)
                    {
                        tableColRange = colCashPaymentPercentage;
                    }
                    else
                    {
                        tableColRange = npstruct;
                    }

                    IRange iRange = worksheet["A7:" + clsStaticInfo.GetxlsCol(tableColRange) + (sheetEndXlsRow)];
                    IPivotCache cache2 = workbook.PivotCaches.Add(iRange);
                    IPivotCache cache = workbook.PivotCaches.Add(iRange);


                    #region Second Pivot table
                    pivotSheet.Range[xlsRow + 2, 1].Text = "EmployeeStatus, PaymentMode, Department Wise Salary Summary";
                    pivotSheet.Range[xlsRow + 2, 1, xlsRow + 2, 5].Merge();
                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Size = 12;

                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable2 = pivotSheet.PivotTables.Add("PivotTable2", pivotSheet["A8"], cache);

                    pivotTable2.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[cPaymentMode - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[cDept - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[cSec - 1].Axis = PivotAxisTypes.Row;


                    IPivotTable pivotTable2_1 = pivotSheet.PivotTables["PivotTable2"];
                    pivotTable2_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable2_1.Options.ShowDrillIndicators = false;

                    pivotTable2_1.DisplayFieldCaptions = true;

                    //Add data field
                    IPivotField field2 = pivotTable2_1.Fields[ColSr - 1];
                    pivotTable2_1.DataFields.Add(field2, "Total Employees", PivotSubtotalTypes.Count);
                    int pivotColumnCount = 0;
                    IPivotField fieldGross = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                            }
                            try
                            {
                                if (ob.HeadType == "D")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            catch (Exception ex)
                            {

                                //throw ex;
                            }

                        }
                    }
                    try
                    {
                        fieldGross = null;
                        pivotColumnCount++;
                        fieldGross = pivotTable2_1.Fields[npstruct - 1];
                        pivotTable2_1.DataFields.Add(fieldGross, "Net Payable", PivotSubtotalTypes.Sum);
                        fieldGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                        if (dtbankCash.Rows.Count > 0)
                        {
                            fieldGross = null;
                            pivotColumnCount++;
                            fieldGross = pivotTable2_1.Fields[colBankPaymentPercentage - 1];
                            pivotTable2_1.DataFields.Add(fieldGross, "Employee", PivotSubtotalTypes.Count);
                            fieldGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");
                            pivotColumnCount++;
                            fieldGross = pivotTable2_1.Fields[colBankPaymentPercentage - 1];
                            pivotTable2_1.DataFields.Add(fieldGross, "Bank", PivotSubtotalTypes.Sum);
                            fieldGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");
                            pivotColumnCount++;
                            fieldGross = pivotTable2_1.Fields[colCashPaymentPercentage - 1];
                            pivotTable2_1.DataFields.Add(fieldGross, "Cash", PivotSubtotalTypes.Sum);
                            fieldGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                        }

                    }
                    catch (Exception)
                    {

                    }

                    pivotTable2_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;

                    int totalColumns = pivotTable2_1.RowFields.Count + pivotColumnCount;

                    int lastCloumn = totalColumns + 2;

                    #endregion


                    pivotSheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    pivotSheet.IsGridLinesVisible = false;
                    pivotSheet.IsDisplayZeros = false;

                    pivotSheet.UsedRange.WrapText = false;
                    pivotSheet.PageSetup.TopMargin = 0.5;
                    pivotSheet.PageSetup.BottomMargin = 0.7;
                    pivotSheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    pivotSheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    pivotSheet.PageSetup.LeftMargin = 0.5;
                    pivotSheet.PageSetup.RightMargin = 0.2;
                    pivotSheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    pivotSheet.PageSetup.FitToPagesTall = 0;
                    pivotSheet.PageSetup.FitToPagesWide = 1;
                    pivotSheet.PageSetup.PaperSize = ExcelPaperSize.PaperLegal;

                    #endregion


                    #region PivotSheet 2 Employee Category  No 
                    IWorksheet pivotSheet2EmpC = workbook.Worksheets[1];
                    pivotSheet2EmpC.Name = "Summary 2";

                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = pivotSheet2EmpC.GetColumnWidth(1) + pivotSheet2EmpC.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((pivotSheet2EmpC.GetRowHeight(1) + pivotSheet2EmpC.GetRowHeight(2) + pivotSheet2EmpC.GetRowHeight(3) + pivotSheet2EmpC.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = pivotSheet2EmpC.Pictures.AddPicture(1, 1, companyLogo);

                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    #region Report Header
                    xlsRow = 1;
                    xlsCol = 1;


                    pivotSheet2EmpC.Range[xlsRow, 3].Text = CmpName;
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet2EmpC.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet2EmpC.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet2EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet2EmpC.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow += 1;

                    pivotSheet2EmpC.Range[xlsRow, 3].Text = FactoryName;
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;

                    pivotSheet2EmpC.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet2EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet2EmpC.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    xlsRow += 1;

                    pivotSheet2EmpC.Range[xlsRow, 3].Text = FactoryAddress;
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet2EmpC.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet2EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet2EmpC.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //pivotSheet2EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    xlsRow += 1;
                    pivotSheet2EmpC.Range[xlsRow, 3].Text = "Salary Summary for the month of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet2EmpC.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet2EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    pivotSheet2EmpC.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet2EmpC.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //pivotSheet2EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    #endregion

                    pivotSheet2EmpC.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                    pivotSheet2EmpC.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                    pivotSheet2EmpC.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;

                    #region Second Pivot table

                    lastCloumn = 1;
                    pivotSheet2EmpC.Range[xlsRow + 2, lastCloumn].Text = "Employee Category Wise Salary Summary";
                    pivotSheet2EmpC.Range[xlsRow + 2, lastCloumn].CellStyle.Font.Size = 12;
                    pivotSheet2EmpC.Range[xlsRow + 2, lastCloumn, xlsRow + 2, lastCloumn + 5].Merge();
                    pivotSheet2EmpC.Range[xlsRow + 2, lastCloumn].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable = pivotSheet2EmpC.PivotTables.Add("PivotTable1", pivotSheet2EmpC["A8"], cache);

                    //Add Pivot table fields (Row and Column fields)
                    pivotTable.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[colEmpCategory - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[cDept - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[cSec - 1].Axis = PivotAxisTypes.Row;


                    IPivotTable pivotTable1 = pivotSheet2EmpC.PivotTables["PivotTable1"];
                    pivotTable1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable1.Options.ShowDrillIndicators = false;

                    pivotTable1.DisplayFieldCaptions = true;
                    pivotTable1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;

                    //Add data field
                    IPivotField field = pivotTable.Fields[ColSr - 1];
                    pivotTable.DataFields.Add(field, "Total Employee", PivotSubtotalTypes.Count);

                    int pivot2ColumnCount = 0;
                    IPivotField fieldGross2 = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross2 = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            if (ob.HeadType == "D")
                            {
                                pivot2ColumnCount++;
                                fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                            }

                        }
                    }
                    fieldGross2 = null;
                    pivot2ColumnCount++;
                    fieldGross2 = pivotTable.Fields[npstruct - 1];
                    pivotTable.DataFields.Add(fieldGross2, "Net Payable", PivotSubtotalTypes.Sum);
                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                    if (dtbankCash.Rows.Count > 0)
                    {
                        fieldGross2 = null;
                        pivotColumnCount++;
                        fieldGross2 = pivotTable.Fields[colBankPaymentPercentage - 1];
                        pivotTable.DataFields.Add(fieldGross2, "Employee Bank", PivotSubtotalTypes.Count);
                        fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(0, "");
                        pivotColumnCount++;
                        fieldGross2 = pivotTable.Fields[colBankPaymentPercentage - 1];
                        pivotTable.DataFields.Add(fieldGross2, "Bank", PivotSubtotalTypes.Sum);
                        fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(0, "");
                        pivotColumnCount++;
                        fieldGross2 = pivotTable.Fields[colCashPaymentPercentage - 1];
                        pivotTable.DataFields.Add(fieldGross2, "Cash", PivotSubtotalTypes.Sum);
                        fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                    }




                    pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;
                    totalColumns = 0;
                    totalColumns = pivotTable.RowFields.Count + pivotColumnCount;
                    //lastCloumn = 0;
                    //lastCloumn = totalColumns + 2;

                    #endregion

                    pivotSheet2EmpC.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    pivotSheet2EmpC.IsGridLinesVisible = false;
                    pivotSheet2EmpC.IsDisplayZeros = false;

                    pivotSheet2EmpC.UsedRange.WrapText = false;
                    pivotSheet2EmpC.PageSetup.TopMargin = 0.5;
                    pivotSheet2EmpC.PageSetup.BottomMargin = 0.7;
                    pivotSheet2EmpC.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    pivotSheet2EmpC.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    pivotSheet2EmpC.PageSetup.LeftMargin = 0.5;
                    pivotSheet2EmpC.PageSetup.RightMargin = 0.2;
                    pivotSheet2EmpC.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    pivotSheet2EmpC.PageSetup.FitToPagesTall = 0;
                    pivotSheet2EmpC.PageSetup.FitToPagesWide = 1;
                    pivotSheet2EmpC.PageSetup.PaperSize = ExcelPaperSize.PaperLegal;
                    #endregion


                    #region PivotSheet 3 EmployeeStatus ,Employee Category and Department
                    IWorksheet pivotSheet3EmpC = workbook.Worksheets[2];
                    pivotSheet3EmpC.Name = "Summary 3";

                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = pivotSheet3EmpC.GetColumnWidth(1) + pivotSheet3EmpC.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((pivotSheet3EmpC.GetRowHeight(1) + pivotSheet3EmpC.GetRowHeight(2) + pivotSheet3EmpC.GetRowHeight(3) + pivotSheet3EmpC.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = pivotSheet3EmpC.Pictures.AddPicture(1, 1, companyLogo);

                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    #region Report Header
                    xlsRow = 1;
                    xlsCol = 1;


                    pivotSheet3EmpC.Range[xlsRow, 3].Text = CmpName;
                    pivotSheet3EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet3EmpC.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet3EmpC.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet3EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    pivotSheet3EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet3EmpC.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow += 1;

                    pivotSheet3EmpC.Range[xlsRow, 3].Text = FactoryName;
                    pivotSheet3EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet3EmpC.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet3EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                    pivotSheet3EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet3EmpC.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    xlsRow += 1;

                    pivotSheet3EmpC.Range[xlsRow, 3].Text = FactoryAddress;
                    pivotSheet3EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet3EmpC.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet3EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    pivotSheet3EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet3EmpC.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow += 1;
                    pivotSheet3EmpC.Range[xlsRow, 3].Text = "Salary Summary for the month of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                    pivotSheet3EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet3EmpC.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet3EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    pivotSheet3EmpC.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet3EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet3EmpC.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    #endregion

                    pivotSheet3EmpC.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                    pivotSheet3EmpC.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                    pivotSheet3EmpC.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;

                    #region Second Pivot table

                    totalColumns += pivotTable.RowFields.Count + pivot2ColumnCount;

                    lastCloumn = 1;

                    pivotSheet3EmpC.Range[xlsRow + 2, lastCloumn].Text = "EmployeeStatus ,Employee Category and Department Wise  Salary Summary";
                    pivotSheet3EmpC.Range[xlsRow + 2, lastCloumn].CellStyle.Font.Size = 12;
                    pivotSheet3EmpC.Range[xlsRow + 2, lastCloumn, xlsRow + 2, lastCloumn + 5].Merge();
                    pivotSheet3EmpC.Range[xlsRow + 2, lastCloumn].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable3 = pivotSheet3EmpC.PivotTables.Add("PivotTable13", pivotSheet3EmpC["A8"], cache);

                    pivotTable3.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable3.Fields[cDept - 1].Axis = PivotAxisTypes.Row;
                    pivotTable3.Fields[colEmpCategory - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable13_1 = pivotSheet3EmpC.PivotTables["PivotTable13"];
                    pivotTable13_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable13_1.Options.ShowDrillIndicators = false;

                    pivotTable13_1.DisplayFieldCaptions = true;
                    pivotTable13_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;

                    IPivotField fields3 = pivotTable13_1.Fields[ColSr - 1];
                    pivotTable13_1.DataFields.Add(fields3, "Total Employee", PivotSubtotalTypes.Count);

                    int pivot3ColumnCount = 0;
                    IPivotField fieldGross3 = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross3 = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivot3ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivot2ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }

                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            if (ob.HeadType == "D")
                            {
                                pivot2ColumnCount++;
                                fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                            }

                        }
                    }
                    fieldGross3 = null;
                    pivot2ColumnCount++;
                    fieldGross3 = pivotTable13_1.Fields[npstruct - 1];
                    pivotTable13_1.DataFields.Add(fieldGross3, "Net Payable", PivotSubtotalTypes.Sum);
                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                    if (dtbankCash.Rows.Count > 0)
                    {
                        fieldGross3 = null;
                        pivotColumnCount++;
                        fieldGross3 = pivotTable13_1.Fields[colBankPaymentPercentage - 1];
                        pivotTable13_1.DataFields.Add(fieldGross3, "Employee Bank", PivotSubtotalTypes.Count);
                        fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(0, "");
                        pivotColumnCount++;
                        fieldGross3 = pivotTable13_1.Fields[colBankPaymentPercentage - 1];
                        pivotTable13_1.DataFields.Add(fieldGross3, "Bank", PivotSubtotalTypes.Sum);
                        fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(0, "");
                        pivotColumnCount++;
                        fieldGross3 = pivotTable13_1.Fields[colCashPaymentPercentage - 1];
                        pivotTable13_1.DataFields.Add(fieldGross3, "Cash", PivotSubtotalTypes.Sum);
                        fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                    }
                    #endregion

                    pivotSheet3EmpC.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    pivotSheet3EmpC.IsGridLinesVisible = false;
                    pivotSheet3EmpC.IsDisplayZeros = false;

                    pivotSheet3EmpC.UsedRange.WrapText = false;
                    pivotSheet3EmpC.PageSetup.TopMargin = 0.5;
                    pivotSheet3EmpC.PageSetup.BottomMargin = 0.7;
                    pivotSheet3EmpC.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    pivotSheet3EmpC.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    pivotSheet3EmpC.PageSetup.LeftMargin = 0.5;
                    pivotSheet3EmpC.PageSetup.RightMargin = 0.2;
                    pivotSheet3EmpC.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    pivotSheet3EmpC.PageSetup.FitToPagesTall = 0;
                    pivotSheet3EmpC.PageSetup.FitToPagesWide = 1;
                    pivotSheet3EmpC.PageSetup.PaperSize = ExcelPaperSize.PaperLegal;
                    #endregion




                    #region PivotSheet 4 bank Sheet No 4
                    IWorksheet pivotSheet2 = workbook.Worksheets[3];
                    pivotSheet2.Name = "Bank Summary";

                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = pivotSheet2.GetColumnWidth(1) + pivotSheet2.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((pivotSheet2.GetRowHeight(1) + pivotSheet2.GetRowHeight(2) + pivotSheet2.GetRowHeight(3) + pivotSheet2.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = pivotSheet2.Pictures.AddPicture(1, 1, companyLogo);

                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    #region Report Header
                    xlsRow = 1;
                    xlsCol = 1;


                    pivotSheet2.Range[xlsRow, 3].Text = CmpName;
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow += 1;

                    pivotSheet2.Range[xlsRow, 3].Text = FactoryName;
                    pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                    pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    xlsRow += 1;

                    pivotSheet2.Range[xlsRow, 3].Text = FactoryAddress;
                    pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;

                    pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow += 1;
                    pivotSheet2.Range[xlsRow, 3].Text = "Salary Summary for the month of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;

                    pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    #endregion

                    pivotSheet2.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                    pivotSheet2.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                    pivotSheet2.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;
                    //tableColRange = 0;
                    //if (dtbankCash.Rows.Count > 0)
                    //{
                    //    tableColRange = colCashPaymentPercentage;
                    //}
                    //else
                    //{
                    //    tableColRange = npstruct;
                    //}

                    ///IRange iRange = worksheet["A7:" + clsStaticInfo.GetxlsCol(tableColRange) + (sheetEndXlsRow)];
                    //IPivotCache cache2 = workbook.PivotCaches.Add(iRange);
                    //IPivotCache cache = workbook.PivotCaches.Add(iRange);


                    #region Second Pivot table
                    pivotSheet2.Range[xlsRow + 2, 1].Text = "EmployeeStatus, PaymentMode, Bank Wise Salary Summary";
                    pivotSheet2.Range[xlsRow + 2, 1, xlsRow + 2, 5].Merge();
                    pivotSheet2.Range[xlsRow + 2, 1].CellStyle.Font.Size = 12;

                    pivotSheet2.Range[xlsRow + 2, 1].CellStyle.Font.Bold = true;

                    IPivotTable pivotTableBank = pivotSheet2.PivotTables.Add("PivotTableBank", pivotSheet2["A8"], cache);

                    pivotTableBank.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTableBank.Fields[cPaymentMode - 1].Axis = PivotAxisTypes.Row;
                    pivotTableBank.Fields[colBank - 1].Axis = PivotAxisTypes.Row;
                    //pivotTableBank.Fields[cSec - 1].Axis = PivotAxisTypes.Row;


                    IPivotTable pivotTableBank_1 = pivotSheet2.PivotTables["PivotTableBank"];
                    pivotTableBank_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTableBank_1.Options.ShowDrillIndicators = false;

                    pivotTableBank_1.DisplayFieldCaptions = true;

                    //Add data field
                    IPivotField fieldbank = pivotTableBank_1.Fields[ColSr - 1];
                    pivotTableBank_1.DataFields.Add(fieldbank, "Total Employees", PivotSubtotalTypes.Count);
                    pivotColumnCount = 0;
                    IPivotField fieldBankGross = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldBankGross = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivotColumnCount++;
                                    fieldBankGross = pivotTableBank_1.Fields[ob.XLColIndex - 1];
                                    pivotTableBank_1.DataFields.Add(fieldBankGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldBankGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivotColumnCount++;
                                    fieldBankGross = pivotTableBank_1.Fields[ob.XLColIndex - 1];
                                    pivotTableBank_1.DataFields.Add(fieldBankGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldBankGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivotColumnCount++;
                                    fieldBankGross = pivotTableBank_1.Fields[ob.XLColIndex - 1];
                                    pivotTableBank_1.DataFields.Add(fieldBankGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldBankGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                            }
                            try
                            {
                                if (ob.HeadType == "D")
                                {
                                    pivotColumnCount++;
                                    fieldBankGross = pivotTableBank_1.Fields[ob.XLColIndex - 1];
                                    pivotTableBank_1.DataFields.Add(fieldBankGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldBankGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            catch (Exception ex)
                            {

                                //throw ex;
                            }

                        }
                    }
                    try
                    {
                        fieldBankGross = null;
                        pivotColumnCount++;
                        fieldBankGross = pivotTableBank_1.Fields[npstruct - 1];
                        pivotTableBank_1.DataFields.Add(fieldBankGross, "Net Payable", PivotSubtotalTypes.Sum);
                        fieldBankGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                        if (dtbankCash.Rows.Count > 0)
                        {
                            fieldBankGross = null;
                            pivotColumnCount++;
                            fieldBankGross = pivotTableBank_1.Fields[colBankPaymentPercentage - 1];
                            pivotTableBank_1.DataFields.Add(fieldBankGross, "Employee", PivotSubtotalTypes.Count);
                            fieldBankGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");
                            pivotColumnCount++;
                            fieldBankGross = pivotTableBank_1.Fields[colBankPaymentPercentage - 1];
                            pivotTableBank_1.DataFields.Add(fieldBankGross, "Bank", PivotSubtotalTypes.Sum);
                            fieldBankGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");
                            pivotColumnCount++;
                            fieldBankGross = pivotTableBank_1.Fields[colCashPaymentPercentage - 1];
                            pivotTableBank_1.DataFields.Add(fieldBankGross, "Cash", PivotSubtotalTypes.Sum);
                            fieldBankGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                        }

                    }
                    catch (Exception)
                    {

                    }

                    pivotTableBank_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;
                    totalColumns = 0;
                    totalColumns = pivotTableBank_1.RowFields.Count + pivotColumnCount;
                    //lastCloumn = 0;
                    //lastCloumn = totalColumns + 2;

                    #endregion

                    pivotSheet2.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    pivotSheet2.IsGridLinesVisible = false;
                    pivotSheet2.IsDisplayZeros = false;

                    pivotSheet2.UsedRange.WrapText = false;
                    pivotSheet2.PageSetup.TopMargin = 0.5;
                    pivotSheet2.PageSetup.BottomMargin = 0.7;
                    pivotSheet2.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    pivotSheet2.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    pivotSheet2.PageSetup.LeftMargin = 0.5;
                    pivotSheet2.PageSetup.RightMargin = 0.2;
                    pivotSheet2.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    pivotSheet2.PageSetup.FitToPagesTall = 0;
                    pivotSheet2.PageSetup.FitToPagesWide = 1;
                    pivotSheet2.PageSetup.PaperSize = ExcelPaperSize.PaperLegal;
                    #endregion
                    #endregion

                    workbook.ActiveSheetIndex = 0;
                }

                return workbook;
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //objRpt = null;
                //excelEngine = null;
                //application = null;
                //workbook = null;
            }
        }



        public IWorkbook GetEmployeeSalaryProcessedReportSalaryLogWise(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool isTopSheet)
        {
            #region Variable
            clsReport objRpt = null;

            DataView dvEmp = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsEmpLoyeeInfo = null;
            DataTable dtEmployees = null;

            DataView dvSlrSheet = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            int endGenericColumn = 0;
            #endregion Variable

            try
            {
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
                var ldateOfMonth = daysInMonth + "-" + monthName + "-" + year;
                var fdateOfMonth = "1" + "-" + monthName + "-" + year;
                string strPath = "";
                Image companyLogo = null;

                string companyLogoName = _sqlRepository.GetDataTable(@"select * from ORG.Company where Id = '" + companyId + @"'").Rows[0]["Image"].ToString();

                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyLogoName);  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();

                #endregion Variable

                #region DataSet

                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                objRpt.GetExtraAbsent(plantId, parameters, month.ToInt(), year.ToInt(), out dsExtraAbsent);

                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);

                //Sql Salary Structure 
                List<SalarySheetReportUD> listdsSlrStr = new List<SalarySheetReportUD>();

                //Sql Salary Process 
                DataTable dtSalaryHeadSheet;
                List<SalarySheetReportUD> listdsSlrProc = new List<SalarySheetReportUD>();
                GetEmployeeInfoDetailSalaryLogWise(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, out dsEmpLoyeeInfo);//Sql Query For Salary  Data
                Dictionary<string, List<DataRow>> dicEmpSalry = GetEmployeeSalaryInfoDetail(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, salaryProcessId, payRollGroup, parameters, out dtSalaryHeadSheet);

                if (dicEmpSalry.First().Value[0].Table.Rows.Count > 0)
                {
                    listdsSlrProc = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    listdsSlrStr = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    dtEmployees = dsEmpLoyeeInfo.Tables[0];//dicEmpSalry.First().Value[0].Table;
                }
                else
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }

                dvSlrSheet = new DataView();

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);

                objRpt.SelectedPlant(plantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(2);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 6;
                xlsCol = 1;

                #region Column Variables
                int ColSr = 0, ColIDNo = 0, ColName = 0, ColDOJ = 0, ColDOS = 0, cDept = 0, cSec = 0, cSubSec = 0, cLine = 0, cPayrollGroup = 0, cJobLocation = 0, cGender = 0,
                    cGrade = 0, ColGVDG = 0, ColGrs = 0, colPayDays = 0, ColPdDy = 0, ColLate = 0, ColAbDy = 0, ColHlDy = 0, ColWkOf = 0, ColLv = 0, ColMLv = 0
                   , ColLWP = 0, colBank = 0, cDMP = 0, colBankAccountNo = 0, ColExtraAbsent = 0, colEmpCurrentStat = 0, colEmpStatus = 0, cPaymentMode = 0, cUnit = 0, ColTotalOTHR = 0, colDirectManpowerCost = 0;
                int npstruct = 0;

                #endregion

                //1
                SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);
                SetCellValue("ID No.", sheet1, xlsRow, ref xlsCol, out ColIDNo, 12);
                SetCellValue("Name", sheet1, xlsRow, ref xlsCol, out ColName, 17);
                SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out ColDOJ, 12);
                SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out ColDOS, 12);
                SetCellValue("EmployeeCurrentStatus", sheet1, xlsRow, ref xlsCol, out colEmpCurrentStat, 12);
                SetCellValue("EmployeeSatatus", sheet1, xlsRow, ref xlsCol, out colEmpStatus, 12);
                SetCellValue("Gender", sheet1, xlsRow, ref xlsCol, out cGender, 12);
                SetCellValue("Designation", sheet1, xlsRow, ref xlsCol, out ColGVDG, 25);
                SetCellValue("Employee Category", sheet1, xlsRow, ref xlsCol, out int colEmpCategory, 25);
                SetCellValue("Department", sheet1, xlsRow, ref xlsCol, out cDept, 25);
                SetCellValue("Section", sheet1, xlsRow, ref xlsCol, out cSec, 25);
                SetCellValue("SubSection", sheet1, xlsRow, ref xlsCol, out cSubSec, 25);
                SetCellValue("Unit", sheet1, xlsRow, ref xlsCol, out cUnit, 25);
                SetCellValue("Line", sheet1, xlsRow, ref xlsCol, out cLine, 25);
                SetCellValue("JobLocation", sheet1, xlsRow, ref xlsCol, out cJobLocation, 25);
                SetCellValue("Payroll group", sheet1, xlsRow, ref xlsCol, out cPayrollGroup, 25);
                //SetCellValue("Payment mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                SetCellValue("Payment mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                SetCellValue("Bank", sheet1, xlsRow, ref xlsCol, out colBank, 25);
                SetCellValue("Bank Acc No.", sheet1, xlsRow, ref xlsCol, out colBankAccountNo, 25);
                SetCellValue("IFSCCode", sheet1, xlsRow, ref xlsCol, out int colBankIFSCCode, 25);

                SetCellValue("Grade", sheet1, xlsRow, ref xlsCol, out cGrade, 25);
                //SetCellValue("Direct Manpower", sheet1, xlsRow, ref xlsCol, out cDMP, 25);
                SetCellValue("Direct Manpower Cost", sheet1, xlsRow, ref xlsCol, out colDirectManpowerCost, 25);

                SetCellValue("Pay Days", sheet1, xlsRow, ref xlsCol, out colPayDays, 5);
                SetCellValue("Present", sheet1, xlsRow, ref xlsCol, out ColPdDy, 9);
                SetCellValue("Late", sheet1, xlsRow, ref xlsCol, out ColLate, 9);
                SetCellValue("Absent", sheet1, xlsRow, ref xlsCol, out ColAbDy, 9);
                SetCellValue("LWP", sheet1, xlsRow, ref xlsCol, out ColLWP, 9);
                SetCellValue("Extra Absent", sheet1, xlsRow, ref xlsCol, out ColExtraAbsent, 9);
                SetCellValue("Holiday", sheet1, xlsRow, ref xlsCol, out ColHlDy, 9);
                SetCellValue("WeekOff", sheet1, xlsRow, ref xlsCol, out ColWkOf, 9);
                SetCellValue("Leave", sheet1, xlsRow, ref xlsCol, out ColLv, 11);
                SetCellValue("Maternity Leave", sheet1, xlsRow, ref xlsCol, out ColMLv, 20);
                SetCellValue("Total Ot Hr", sheet1, xlsRow, ref xlsCol, out ColTotalOTHR, 11);
                endGenericColumn = xlsCol;

                //SR to
                sheet1.Range[xlsRow, ColSr].Text = "Employee Information";
                sheet1.Range[xlsRow, ColSr, xlsRow, ColTotalOTHR].Merge();
                //xlsCol += 1;
                ColGrs = ColTotalOTHR;
                // 9

                var _count_earning_head = 0;
                var _count_earning_ctchead = 0;
                var _count_deducting_head = 0;
                var _total_head_count = 0;

                Dictionary<string, SalaryHeadSequence> shtList = null;

                CreateDynamicSHead(dtSalaryHeadSheet, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out shtList);

                List<SalaryHeadSequence> salList = new List<SalaryHeadSequence>();
                salList.AddRange(shtList.Values);

                xlsCol--;

                //Header Col
                if (_count_earning_ctchead > 0)
                {
                    sheet1.Range[xlsRow, ColGrs + 1].Text = "Earning head";
                    sheet1.Range[xlsRow, ColGrs + 1, xlsRow, ColGrs + _count_earning_head + _count_earning_ctchead].Merge();
                }

                var ds = ColGrs + 1 + _count_earning_head + _count_earning_ctchead;

                if (_count_deducting_head > 0)
                {
                    sheet1.Range[xlsRow, ds].Text = "Deduction head";
                    sheet1.Range[xlsRow, ds, xlsRow, ds + _count_deducting_head - 1].Merge();
                }
                npstruct = 0;
                if (shtList.Count > 0)
                {
                    xlsCol++;
                    npstruct = ColGrs + shtList.Count + 1;
                    sheet1.Range[xlsRow + 1, npstruct].Text = "Net Payable";
                    //sheet1.Range[xlsRow, npstruct].ColumnWidth = 14;
                    //sheet1.Range[xlsRow, npstruct, xlsRow + 1, npstruct].Merge();
                }

                xlsCol++;


                xlsCol++;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No.";
                sheet1.Range[xlsRow - 1, 1].ColumnWidth = 14;
                sheet1.Range[xlsRow - 1, 1, xlsRow - 1, 3].Merge();
                sheet1.Range[xlsRow, 1, xlsRow + 1, npstruct].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].VerticalAlignment = ExcelVAlign.VAlignCenter;
                endXlsCol = npstruct;


                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                Param param = new Param();
                param.CompanyGroupId = companyGroupId;
                param.CompanyId = companyId;

                string FactoryAddress = string.Empty;
                try
                {

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
                catch (Exception ex)
                {
                }


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
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
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
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Salary Sheet For The Month Of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                var SrNo = 0;
                var x = "";

                var oRU = new ReportUtility();

                xlsRow = RowIndex;

                xlsRow--;
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    #region EmpInfo
                    try
                    {
                        SrNo += 1;
                        x = dtEmployees.Rows[i]["EmpSystemID"].ToString().Trim();

                        //1
                        sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                        sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //2
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, ColIDNo].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, ColIDNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColIDNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //3
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                            sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJ"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJ"].ToString();
                        sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOS"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOS].Text = dtEmployees.Rows[i]["DOS"].ToString();
                        sheet1.Range[xlsRow, ColDOS].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOS].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString()) == false)
                            sheet1.Range[xlsRow, colEmpCurrentStat].Text = dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString();
                        sheet1.Range[xlsRow, colEmpCurrentStat].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCurrentStat].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeStatus"].ToString()) == false)
                            sheet1.Range[xlsRow, colEmpStatus].Text = dtEmployees.Rows[i]["EmployeeStatus"].ToString();
                        sheet1.Range[xlsRow, colEmpStatus].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LegalDesignation"].ToString()) == false)
                            sheet1.Range[xlsRow, ColGVDG].Text = dtEmployees.Rows[i]["LegalDesignation"].ToString();
                        sheet1.Range[xlsRow, ColGVDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColGVDG].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmpCategoryName"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colEmpCategory].Text = dtEmployees.Rows[i]["EmpCategoryName"].ToString();
                        sheet1.Range[xlsRow, colEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4.2
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DepartmentName"].ToString()) == false)
                            sheet1.Range[xlsRow, cDept].Text = dtEmployees.Rows[i]["DepartmentName"].ToString();
                        sheet1.Range[xlsRow, cDept].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cDept].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SectionName"].ToString()) == false)
                            sheet1.Range[xlsRow, cSec].Text = dtEmployees.Rows[i]["SectionName"].ToString();
                        sheet1.Range[xlsRow, cSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cSec].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SubSectionName"].ToString()) == false)
                            sheet1.Range[xlsRow, cSubSec].Text = dtEmployees.Rows[i]["SubSectionName"].ToString();
                        sheet1.Range[xlsRow, cSubSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cSubSec].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["UnitName"].ToString()) == false)
                            sheet1.Range[xlsRow, cUnit].Text = dtEmployees.Rows[i]["UnitName"].ToString();
                        sheet1.Range[xlsRow, cUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PaymentMode"].ToString()) == false)
                            sheet1.Range[xlsRow, cPaymentMode].Text = dtEmployees.Rows[i]["PaymentMode"].ToString();
                        sheet1.Range[xlsRow, cPaymentMode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cPaymentMode].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["BankName"].ToString()) == false)
                            sheet1.Range[xlsRow, colBank].Text = dtEmployees.Rows[i]["BankName"].ToString();
                        sheet1.Range[xlsRow, colBank].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBank].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["BankAccNo"].ToString()) == false)
                            sheet1.Range[xlsRow, colBankAccountNo].Text = dtEmployees.Rows[i]["BankAccNo"].ToString();
                        sheet1.Range[xlsRow, colBankAccountNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBankAccountNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["IFSCCode"].ToString()) == false)
                            sheet1.Range[xlsRow, colBankIFSCCode].Text = dtEmployees.Rows[i]["IFSCCode"].ToString();
                        sheet1.Range[xlsRow, colBankIFSCCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBankIFSCCode].VerticalAlignment = ExcelVAlign.VAlignCenter;



                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                            sheet1.Range[xlsRow, cGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                        sheet1.Range[xlsRow, cGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["JobLocation"].ToString()) == false)
                            sheet1.Range[xlsRow, cJobLocation].Text = dtEmployees.Rows[i]["JobLocation"].ToString();
                        sheet1.Range[xlsRow, cJobLocation].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cJobLocation].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LineName"].ToString()) == false)
                            sheet1.Range[xlsRow, cLine].Text = dtEmployees.Rows[i]["LineName"].ToString();
                        sheet1.Range[xlsRow, cLine].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cLine].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PayRollGroup"].ToString()) == false)
                            sheet1.Range[xlsRow, cPayrollGroup].Text = dtEmployees.Rows[i]["PayRollGroup"].ToString();
                        sheet1.Range[xlsRow, cPayrollGroup].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cPayrollGroup].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //5
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["GradeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, cGrade].Text = dtEmployees.Rows[i]["GradeCode"].ToString();
                        sheet1.Range[xlsRow, cGrade].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGrade].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DirectManpowerCost"].ToString()) == false)
                            sheet1.Range[xlsRow, colDirectManpowerCost].Text = dtEmployees.Rows[i]["DirectManpowerCost"].ToString();
                        sheet1.Range[xlsRow, colDirectManpowerCost].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colDirectManpowerCost].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                            sheet1.Range[xlsRow, cGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                        sheet1.Range[xlsRow, cGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //5 "Section", "SubSection", 

                        #endregion
                        #region Attendance Data
                        //if (dtEmpAttdnInfo.Rows.Count > 0)
                        //{
                        double _ExtraAbsent = 0;
                        dvExtraAbsent.RowFilter = "EmpSystemID='" + dtEmployees.Rows[i]["EmpSystemID"].ToString() + "' ";
                        _ExtraAbsent = dvExtraAbsent.Count;
                        var payDays = 0.00;
                        // clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"]);
                        if (!String.IsNullOrEmpty(dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper()))
                        {
                            if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOffAndHoliday.ToString().ToUpper())
                            {
                                payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalHoliDay"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString());

                            }
                            if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOff.ToString().ToUpper())
                            {
                                payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString());
                            }
                        }
                        else
                        {
                            payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString());
                        }
                        SetCellTextAttdn(sheet1, xlsRow, colPayDays, payDays);
                        SetCellTextAttdn(sheet1, xlsRow, ColPdDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalPresent"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLate, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLate"].ToString()));
                        SetCellTextNumber(sheet1, xlsRow, ColAbDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLWP"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLWP, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLWP"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColExtraAbsent, _ExtraAbsent);
                        SetCellTextAttdn(sheet1, xlsRow, ColHlDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalHoliDay"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColWkOf, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLv"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColMLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalMLv"].ToString()));

                        SetCellTextAttdn(sheet1, xlsRow, ColTotalOTHR, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalOTHr"].ToString()) / 60);

                        //}
                        #endregion


                        //var _total_head_count_body = 0;

                        #region ------------------------------------Salary Sheet----------------------------------
                        if (dicEmpSalry.ContainsKey(dtEmployees.Rows[i]["EmpSystemID"].ToString()))
                        {
                            List<DataRow> drSalaryHeadCollection = dicEmpSalry[dtEmployees.Rows[i]["EmpSystemID"].ToString()];
                            if (drSalaryHeadCollection.Count > 0)
                            {
                                for (int CI = 0; CI < drSalaryHeadCollection.Count; CI++)
                                {
                                    if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "NET PAYABLE")
                                    {
                                        sheet1.Range[xlsRow, npstruct].Number = Convert.ToDouble(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                        continue;
                                    }
                                    try
                                    {
                                        SalaryHeadSequence xx = shtList[drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();
                                        if (xx != null)
                                        {
                                            if (drSalaryHeadCollection[CI]["HeadType"].ToString() == "D")
                                            {
                                                sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()) * (-1);
                                            }

                                            else
                                            {

                                                sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                            }

                                            sheet1.Range[xlsRow, xx.XLColIndex].NumberFormat = oRU.NumberFormatInt();
                                            sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                            sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                        throw ex;
                                    }

                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }


                    #endregion

                    xlsRow++;
                }//for emp count
                int sheetEndXlsRow = xlsRow - 1;
                #endregion ----------------------Data-----------------------

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var freezePan = RowIndex - 1;
                sheet1.UsedRange["A" + freezePan].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 10;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "EmpSalaryInfo";
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                #endregion

                workbook.Version = ExcelVersion.Excel2016;
                //var strFileName = "EmpSalaryStrSheet-" + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + Convert.ToDateTime(fdateOfMonth).ToString("yyyy") + "-" + para.SalaryProcessId + ".xls";

                if (isTopSheet == true)
                {
                    #region Salary Summary
                    string filePath = HostingEnvironment.MapPath("~/") + "TempSalaeySummary.xlsx";
                    workbook.SaveAs(filePath);
                    workbook = application.Workbooks.Open(filePath);

                    IWorksheet worksheet = workbook.Worksheets[0];
                    worksheet.Move(1);

                    #region PivotSheet1
                    IWorksheet pivotSheet = workbook.Worksheets[0];
                    pivotSheet.Name = "Summary";

                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = pivotSheet.GetColumnWidth(1) + pivotSheet.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((pivotSheet.GetRowHeight(1) + pivotSheet.GetRowHeight(2) + pivotSheet.GetRowHeight(3) + pivotSheet.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = pivotSheet.Pictures.AddPicture(1, 1, companyLogo);

                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    #region Report Header
                    xlsRow = 1;
                    xlsCol = 1;


                    pivotSheet.Range[xlsRow, 3].Text = CmpName;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow += 1;

                    pivotSheet.Range[xlsRow, 3].Text = FactoryName;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    xlsRow += 1;

                    pivotSheet.Range[xlsRow, 3].Text = FactoryAddress;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    xlsRow += 1;
                    pivotSheet.Range[xlsRow, 3].Text = "Salary Summary for the month of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    #endregion

                    pivotSheet.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;

                    IRange iRange = worksheet["A7:" + clsStaticInfo.GetxlsCol(npstruct) + (sheetEndXlsRow)];
                    IPivotCache cache2 = workbook.PivotCaches.Add(iRange);
                    IPivotCache cache = workbook.PivotCaches.Add(iRange);


                    #region Second Pivot table
                    pivotSheet.Range[xlsRow + 2, 1].Text = "EmployeeStatus, PaymentMode, Department Wise Salary Summary";
                    pivotSheet.Range[xlsRow + 2, 1, xlsRow + 2, 5].Merge();
                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Size = 12;

                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable2 = pivotSheet.PivotTables.Add("PivotTable2", pivotSheet["A8"], cache);

                    pivotTable2.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[cPaymentMode - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[cDept - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable2_1 = pivotSheet.PivotTables["PivotTable2"];
                    pivotTable2_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable2_1.Options.ShowDrillIndicators = false;

                    pivotTable2_1.DisplayFieldCaptions = true;

                    //Add data field
                    IPivotField field2 = pivotTable2_1.Fields[ColSr - 1];
                    pivotTable2_1.DataFields.Add(field2, "Total Employees", PivotSubtotalTypes.Count);
                    int pivotColumnCount = 0;
                    IPivotField fieldGross = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                            }
                            try
                            {
                                if (ob.HeadType == "D")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            catch (Exception ex)
                            {

                                //throw ex;
                            }

                        }
                    }
                    try
                    {
                        fieldGross = null;
                        pivotColumnCount++;
                        fieldGross = pivotTable2_1.Fields[npstruct - 1];
                        pivotTable2_1.DataFields.Add(fieldGross, "Net Payable", PivotSubtotalTypes.Sum);
                        fieldGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                    }
                    catch (Exception)
                    {

                    }

                    pivotTable2_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;

                    int totalColumns = pivotTable2_1.RowFields.Count + pivotColumnCount;

                    int lastCloumn = totalColumns + 2;

                    #endregion
                    #region PivotTable2

                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].Text = "Employee Category Wise Salary Summary";
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2, xlsRow + 2, lastCloumn + 5].Merge();
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet[clsStaticInfo.GetxlsCol(lastCloumn + 2) + "8"], cache);

                    //Add Pivot table fields (Row and Column fields)
                    pivotTable.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[colEmpCategory - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[cDept - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable1 = pivotSheet.PivotTables["PivotTable1"];
                    pivotTable1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable1.Options.ShowDrillIndicators = false;

                    pivotTable1.DisplayFieldCaptions = true;
                    pivotTable1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;

                    //Add data field
                    IPivotField field = pivotTable.Fields[ColSr - 1];
                    pivotTable.DataFields.Add(field, "Total Employee", PivotSubtotalTypes.Count);

                    int pivot2ColumnCount = 0;
                    IPivotField fieldGross2 = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross2 = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            if (ob.HeadType == "D")
                            {
                                pivot2ColumnCount++;
                                fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                            }

                        }
                    }
                    fieldGross2 = null;
                    pivot2ColumnCount++;
                    fieldGross2 = pivotTable.Fields[npstruct - 1];
                    pivotTable.DataFields.Add(fieldGross2, "Net Payable", PivotSubtotalTypes.Sum);
                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(0, "");


                    #endregion

                    #region PivotTable3

                    totalColumns += pivotTable.RowFields.Count + pivot2ColumnCount;


                    lastCloumn += totalColumns - 10;

                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].Text = "EmployeeStatus ,Employee Category and Department Wise  Salary Summary";
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2, xlsRow + 2, lastCloumn + 5].Merge();
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Bold = true;

                    //Create "PivotTable1" with the cache at the specified range
                    IPivotTable pivotTable3 = pivotSheet.PivotTables.Add("PivotTable13", pivotSheet[clsStaticInfo.GetxlsCol(lastCloumn + 2) + "8"], cache);

                    //Add Pivot table fields (Row and Column fields)
                    pivotTable3.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable3.Fields[cDept - 1].Axis = PivotAxisTypes.Row;
                    pivotTable3.Fields[colEmpCategory - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable13_1 = pivotSheet.PivotTables["PivotTable13"];
                    pivotTable13_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable13_1.Options.ShowDrillIndicators = false;

                    pivotTable13_1.DisplayFieldCaptions = true;
                    pivotTable13_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;


                    //Add data field
                    IPivotField fields3 = pivotTable13_1.Fields[ColSr - 1];
                    pivotTable13_1.DataFields.Add(fields3, "Total Employee", PivotSubtotalTypes.Count);

                    int pivot3ColumnCount = 0;
                    IPivotField fieldGross3 = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross3 = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivot3ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivot2ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }

                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            if (ob.HeadType == "D")
                            {
                                pivot2ColumnCount++;
                                fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                            }

                        }
                    }
                    fieldGross3 = null;
                    pivot2ColumnCount++;
                    fieldGross3 = pivotTable13_1.Fields[npstruct - 1];
                    pivotTable13_1.DataFields.Add(fieldGross3, "Net Payable", PivotSubtotalTypes.Sum);
                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(0, "");


                    #endregion
                    pivotSheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    pivotSheet.IsGridLinesVisible = false;
                    pivotSheet.IsDisplayZeros = false;

                    pivotSheet.UsedRange.WrapText = false;

                    #endregion
                    #endregion

                    workbook.ActiveSheetIndex = 0;
                }

                return workbook;
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //objRpt = null;
                //excelEngine = null;
                //application = null;
                //workbook = null;
            }
        }

        public IWorkbook GetEmployeeSalaryProcessedOTQtyAmountReport(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool isTopSheet, double budgetedOT)
        {
            #region Variable
            clsReport objRpt = null;

            DataView dvEmp = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsEmpLoyeeInfo = null;
            DataTable dtEmployees = null;

            DataView dvSlrSheet = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            int endGenericColumn = 0;
            #endregion Variable

            try
            {
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
                var ldateOfMonth = daysInMonth + "-" + monthName + "-" + year;
                var fdateOfMonth = "1" + "-" + monthName + "-" + year;
                string strPath = "";
                Image companyLogo = null;


                string companyLogoName = _sqlRepository.GetDataTable(@"select * from ORG.Company where Id = '" + companyId + @"'").Rows[0]["Image"].ToString();
                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyLogoName);  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();

                #endregion Variable

                #region DataSet

                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                objRpt.GetExtraAbsent(plantId, parameters, month.ToInt(), year.ToInt(), out dsExtraAbsent);

                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);

                //Sql Salary Structure 
                List<SalarySheetReportUD> listdsSlrStr = new List<SalarySheetReportUD>();

                //Sql Salary Process 
                DataTable dtSalaryHeadSheet;
                List<SalarySheetReportUD> listdsSlrProc = new List<SalarySheetReportUD>();
                GetEmployeeInfoDetailSalaryLogWiseOT(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, out dsEmpLoyeeInfo, budgetedOT);//Sql Query For Salary  Data
                //Dictionary<string, List<DataRow>> dicEmpSalry = GetEmployeeSalaryInfoDetail(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, salaryProcessId, payRollGroup, parameters, out dtSalaryHeadSheet);

                if (dsEmpLoyeeInfo.Tables[0].Rows.Count > 0)
                {
                    dtEmployees = dsEmpLoyeeInfo.Tables[0];//dicEmpSalry.First().Value[0].Table;
                }
                else
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }

                dvSlrSheet = new DataView();

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);

                objRpt.SelectedPlant(plantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(2);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 6;
                xlsCol = 1;

                #region Column Variables
                int ColSr = 0, ColIDNo = 0, ColName = 0, ColDOJ = 0, ColDOS = 0, cDept = 0, cSec = 0, cSubSec = 0, cLine = 0, cPayrollGroup = 0, cJobLocation = 0, cGender = 0,
                    cGrade = 0, ColGVDG = 0, ColGrs = 0, colPayDays = 0, ColPdDy = 0, ColLate = 0, ColAbDy = 0, ColHlDy = 0, ColWkOf = 0, ColLv = 0, ColMLv = 0
                   , ColLWP = 0, colBank = 0, cDMP = 0, colBankAccountNo = 0, ColExtraAbsent = 0, colEmpCurrentStat = 0, colEmpStatus = 0, cPaymentMode = 0, cUnit = 0, ColTotalOTHR = 0, colDirectManpowerCost = 0;
                int npstruct = 0;

                #endregion

                //1
                SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);
                SetCellValue("ID No.", sheet1, xlsRow, ref xlsCol, out ColIDNo, 12);
                SetCellValue("Name", sheet1, xlsRow, ref xlsCol, out ColName, 17);
                SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out ColDOJ, 12);
                SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out ColDOS, 12);
                SetCellValue("EmployeeCurrentStatus", sheet1, xlsRow, ref xlsCol, out colEmpCurrentStat, 12);
                SetCellValue("EmployeeSatatus", sheet1, xlsRow, ref xlsCol, out colEmpStatus, 12);
                SetCellValue("Gender", sheet1, xlsRow, ref xlsCol, out cGender, 12);
                SetCellValue("Designation", sheet1, xlsRow, ref xlsCol, out ColGVDG, 25);
                SetCellValue("Employee Category", sheet1, xlsRow, ref xlsCol, out int colEmpCategory, 25);
                SetCellValue("Department", sheet1, xlsRow, ref xlsCol, out cDept, 25);
                SetCellValue("Section", sheet1, xlsRow, ref xlsCol, out cSec, 25);
                SetCellValue("SubSection", sheet1, xlsRow, ref xlsCol, out cSubSec, 25);
                SetCellValue("Unit", sheet1, xlsRow, ref xlsCol, out cUnit, 25);
                SetCellValue("Line", sheet1, xlsRow, ref xlsCol, out cLine, 25);
                SetCellValue("JobLocation", sheet1, xlsRow, ref xlsCol, out cJobLocation, 25);
                SetCellValue("Payroll group", sheet1, xlsRow, ref xlsCol, out cPayrollGroup, 25);
                //SetCellValue("Payment mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                SetCellValue("Payment mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                SetCellValue("Bank", sheet1, xlsRow, ref xlsCol, out colBank, 25);
                SetCellValue("Bank Acc No.", sheet1, xlsRow, ref xlsCol, out colBankAccountNo, 25);
                SetCellValue("IFSCCode", sheet1, xlsRow, ref xlsCol, out int colBankIFSCCode, 25);

                SetCellValue("Grade", sheet1, xlsRow, ref xlsCol, out cGrade, 25);
                //SetCellValue("Direct Manpower", sheet1, xlsRow, ref xlsCol, out cDMP, 25);
                SetCellValue("Direct Manpower Cost", sheet1, xlsRow, ref xlsCol, out colDirectManpowerCost, 25);

                SetCellValue("Pay Days", sheet1, xlsRow, ref xlsCol, out colPayDays, 5);
                SetCellValue("Present", sheet1, xlsRow, ref xlsCol, out ColPdDy, 9);
                SetCellValue("Late", sheet1, xlsRow, ref xlsCol, out ColLate, 9);
                SetCellValue("Absent", sheet1, xlsRow, ref xlsCol, out ColAbDy, 9);
                SetCellValue("LWP", sheet1, xlsRow, ref xlsCol, out ColLWP, 9);
                SetCellValue("Extra Absent", sheet1, xlsRow, ref xlsCol, out ColExtraAbsent, 9);
                SetCellValue("Holiday", sheet1, xlsRow, ref xlsCol, out ColHlDy, 9);
                SetCellValue("WeekOff", sheet1, xlsRow, ref xlsCol, out ColWkOf, 9);
                SetCellValue("Leave", sheet1, xlsRow, ref xlsCol, out ColLv, 11);
                SetCellValue("Maternity Leave", sheet1, xlsRow, ref xlsCol, out ColMLv, 20);
                SetCellValue("Ot Rate", sheet1, xlsRow, ref xlsCol, out int colOTRate, 11);

                SetCellValue("Total Ot Hr", sheet1, xlsRow, ref xlsCol, out ColTotalOTHR, 11);
                SetCellValue("Total Ot Amount", sheet1, xlsRow, ref xlsCol, out int colTotalOTHRAmount, 11);

                SetCellValue("Total Budgeted OT Hr", sheet1, xlsRow, ref xlsCol, out int ColBudgetedTotalOTHR, 11);
                SetCellValue("Budgeted OT Amount", sheet1, xlsRow, ref xlsCol, out int colBudgetedOTAmount, 11);
                SetCellValue("Total Non Budgeted OT Hr", sheet1, xlsRow, ref xlsCol, out int ColNonBudgetedTotalOTHR, 11);
                SetCellValue("Non Budgeted OT Amount", sheet1, xlsRow, ref xlsCol, out int colNONBudgetedOTAmount, 11);

                //endGenericColumn = xlsCol;
                endXlsCol = xlsCol;

                //SR to
                sheet1.Range[xlsRow, ColSr].Text = "Employee Information";
                sheet1.Range[xlsRow, ColSr, xlsRow, ColTotalOTHR].Merge();
                //xlsCol += 1;



                //CreateDynamicSHead(dtSalaryHeadSheet, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out shtList);

                //List<SalaryHeadSequence> salList = new List<SalaryHeadSequence>();
                // salList.AddRange(shtList.Values);



                //Header Col



                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No.";
                sheet1.Range[xlsRow - 1, 1].ColumnWidth = 14;
                sheet1.Range[xlsRow - 1, 1, xlsRow - 1, 3].Merge();
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;



                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                Param param = new Param();
                param.CompanyGroupId = companyGroupId;
                param.CompanyId = companyId;

                string FactoryAddress = string.Empty;
                try
                {

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
                catch (Exception ex)
                {
                }


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
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
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
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Budgetary OT for the month of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                var SrNo = 0;
                var x = "";

                var oRU = new ReportUtility();

                xlsRow = RowIndex;

                xlsRow--;
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    #region EmpInfo
                    try
                    {
                        SrNo += 1;
                        x = dtEmployees.Rows[i]["EmpSystemID"].ToString().Trim();

                        //1
                        sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                        sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //2
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, ColIDNo].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, ColIDNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColIDNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //3
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                            sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJ"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJ"].ToString();
                        sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOS"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOS].Text = dtEmployees.Rows[i]["DOS"].ToString();
                        sheet1.Range[xlsRow, ColDOS].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOS].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString()) == false)
                            sheet1.Range[xlsRow, colEmpCurrentStat].Text = dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString();
                        sheet1.Range[xlsRow, colEmpCurrentStat].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCurrentStat].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeStatus"].ToString()) == false)
                            sheet1.Range[xlsRow, colEmpStatus].Text = dtEmployees.Rows[i]["EmployeeStatus"].ToString();
                        sheet1.Range[xlsRow, colEmpStatus].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LegalDesignation"].ToString()) == false)
                            sheet1.Range[xlsRow, ColGVDG].Text = dtEmployees.Rows[i]["LegalDesignation"].ToString();
                        sheet1.Range[xlsRow, ColGVDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColGVDG].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmpCategoryName"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colEmpCategory].Text = dtEmployees.Rows[i]["EmpCategoryName"].ToString();
                        sheet1.Range[xlsRow, colEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4.2
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DepartmentName"].ToString()) == false)
                            sheet1.Range[xlsRow, cDept].Text = dtEmployees.Rows[i]["DepartmentName"].ToString();
                        sheet1.Range[xlsRow, cDept].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cDept].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SectionName"].ToString()) == false)
                            sheet1.Range[xlsRow, cSec].Text = dtEmployees.Rows[i]["SectionName"].ToString();
                        sheet1.Range[xlsRow, cSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cSec].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SubSectionName"].ToString()) == false)
                            sheet1.Range[xlsRow, cSubSec].Text = dtEmployees.Rows[i]["SubSectionName"].ToString();
                        sheet1.Range[xlsRow, cSubSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cSubSec].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["UnitName"].ToString()) == false)
                            sheet1.Range[xlsRow, cUnit].Text = dtEmployees.Rows[i]["UnitName"].ToString();
                        sheet1.Range[xlsRow, cUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PaymentMode"].ToString()) == false)
                            sheet1.Range[xlsRow, cPaymentMode].Text = dtEmployees.Rows[i]["PaymentMode"].ToString();
                        sheet1.Range[xlsRow, cPaymentMode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cPaymentMode].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["BankName"].ToString()) == false)
                            sheet1.Range[xlsRow, colBank].Text = dtEmployees.Rows[i]["BankName"].ToString();
                        sheet1.Range[xlsRow, colBank].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBank].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["BankAccNo"].ToString()) == false)
                            sheet1.Range[xlsRow, colBankAccountNo].Text = dtEmployees.Rows[i]["BankAccNo"].ToString();
                        sheet1.Range[xlsRow, colBankAccountNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBankAccountNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["IFSCCode"].ToString()) == false)
                            sheet1.Range[xlsRow, colBankIFSCCode].Text = dtEmployees.Rows[i]["IFSCCode"].ToString();
                        sheet1.Range[xlsRow, colBankIFSCCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBankIFSCCode].VerticalAlignment = ExcelVAlign.VAlignCenter;



                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                            sheet1.Range[xlsRow, cGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                        sheet1.Range[xlsRow, cGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["JobLocation"].ToString()) == false)
                            sheet1.Range[xlsRow, cJobLocation].Text = dtEmployees.Rows[i]["JobLocation"].ToString();
                        sheet1.Range[xlsRow, cJobLocation].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cJobLocation].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LineName"].ToString()) == false)
                            sheet1.Range[xlsRow, cLine].Text = dtEmployees.Rows[i]["LineName"].ToString();
                        sheet1.Range[xlsRow, cLine].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cLine].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PayRollGroup"].ToString()) == false)
                            sheet1.Range[xlsRow, cPayrollGroup].Text = dtEmployees.Rows[i]["PayRollGroup"].ToString();
                        sheet1.Range[xlsRow, cPayrollGroup].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cPayrollGroup].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //5
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["GradeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, cGrade].Text = dtEmployees.Rows[i]["GradeCode"].ToString();
                        sheet1.Range[xlsRow, cGrade].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGrade].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DirectManpowerCost"].ToString()) == false)
                            sheet1.Range[xlsRow, colDirectManpowerCost].Text = dtEmployees.Rows[i]["DirectManpowerCost"].ToString();
                        sheet1.Range[xlsRow, colDirectManpowerCost].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colDirectManpowerCost].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                            sheet1.Range[xlsRow, cGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                        sheet1.Range[xlsRow, cGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, ColBudgetedTotalOTHR].Number = clsStaticInfo.dbl(dtEmployees.Rows[i]["BudgetedOT"].ToString());
                        sheet1.Range[xlsRow, ColBudgetedTotalOTHR].NumberFormat = clsStaticInfo.NumberFormat(0);
                        sheet1.Range[xlsRow, ColBudgetedTotalOTHR].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, colOTRate].Number = clsStaticInfo.dbl(dtEmployees.Rows[i]["OTRate"].ToString());
                        sheet1.Range[xlsRow, colOTRate].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet1.Range[xlsRow, colOTRate].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, colBudgetedOTAmount].Formula = clsStaticInfo.GetxlsCol(ColBudgetedTotalOTHR) + xlsRow + "*" + clsStaticInfo.GetxlsCol(colOTRate) + xlsRow;
                        sheet1.Range[xlsRow, colBudgetedOTAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet1.Range[xlsRow, colBudgetedOTAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, ColNonBudgetedTotalOTHR].Number = clsStaticInfo.dbl(dtEmployees.Rows[i]["NonBudgetedOT"].ToString());
                        sheet1.Range[xlsRow, ColNonBudgetedTotalOTHR].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet1.Range[xlsRow, ColNonBudgetedTotalOTHR].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, colNONBudgetedOTAmount].Formula = clsStaticInfo.GetxlsCol(ColNonBudgetedTotalOTHR) + xlsRow + "*" + clsStaticInfo.GetxlsCol(colOTRate) + xlsRow;
                        sheet1.Range[xlsRow, colNONBudgetedOTAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet1.Range[xlsRow, colNONBudgetedOTAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        sheet1.Range[xlsRow, colTotalOTHRAmount].Formula = clsStaticInfo.GetxlsCol(ColTotalOTHR) + xlsRow + "*" + clsStaticInfo.GetxlsCol(colOTRate) + xlsRow;
                        sheet1.Range[xlsRow, colTotalOTHRAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                        sheet1.Range[xlsRow, colTotalOTHRAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //5 "Section", "SubSection", 

                        #endregion
                        #region Attendance Data
                        //if (dtEmpAttdnInfo.Rows.Count > 0)
                        //{
                        double _ExtraAbsent = 0;
                        dvExtraAbsent.RowFilter = "EmpSystemID='" + dtEmployees.Rows[i]["EmpSystemID"].ToString() + "' ";
                        _ExtraAbsent = dvExtraAbsent.Count;
                        var payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString());
                        SetCellTextAttdn(sheet1, xlsRow, colPayDays, payDays);
                        SetCellTextAttdn(sheet1, xlsRow, ColPdDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalPresent"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLate, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLate"].ToString()));
                        SetCellTextNumber(sheet1, xlsRow, ColAbDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLWP"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLWP, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLWP"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColExtraAbsent, _ExtraAbsent);
                        SetCellTextAttdn(sheet1, xlsRow, ColHlDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalHoliDay"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColWkOf, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLv"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColMLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalMLv"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColTotalOTHR, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalOTHr"].ToString()) / 60);

                        #endregion


                        //var _total_head_count_body = 0;

                        #region ------------------------------------Salary Sheet----------------------------------
                        //if (dicEmpSalry.ContainsKey(dtEmployees.Rows[i]["EmpSystemID"].ToString()))
                        //{
                        //    List<DataRow> drSalaryHeadCollection = dicEmpSalry[dtEmployees.Rows[i]["EmpSystemID"].ToString()];
                        //    if (drSalaryHeadCollection.Count > 0)
                        //    {
                        //        for (int CI = 0; CI < drSalaryHeadCollection.Count; CI++)
                        //        {
                        //            if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "NET PAYABLE")
                        //            {
                        //                sheet1.Range[xlsRow, npstruct].Number = Convert.ToDouble(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                        //                continue;
                        //            }
                        //            try
                        //            {
                        //                SalaryHeadSequence xx = shtList[drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();
                        //                if (xx != null)
                        //                {
                        //                    if (drSalaryHeadCollection[CI]["HeadType"].ToString() == "D")
                        //                    {
                        //                        sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()) * (-1);
                        //                    }

                        //                    else
                        //                    {

                        //                        sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                        //                    }

                        //                    sheet1.Range[xlsRow, xx.XLColIndex].NumberFormat = oRU.NumberFormatInt();
                        //                    sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        //                    sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //                }
                        //            }
                        //            catch (Exception ex)
                        //            {

                        //                throw ex;
                        //            }

                        //        }
                        //    }
                        //}

                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }


                    #endregion

                    xlsRow++;
                }//for emp count
                int sheetEndXlsRow = xlsRow - 1;
                #endregion ----------------------Data-----------------------

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var freezePan = RowIndex - 1;
                sheet1.UsedRange["A" + freezePan].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 10;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "EmpSalaryInfo";
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                #endregion

                workbook.Version = ExcelVersion.Excel2016;



                return workbook;
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //objRpt = null;
                //excelEngine = null;
                //application = null;
                //workbook = null;
            }
        }


        public IWorkbook GetEmployeeSalaryProcessedReportSalaryLogWiseCompliance(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool isTopSheet)
        {
            #region Variable
            clsReport objRpt = null;

            DataView dvEmp = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsEmpLoyeeInfo = null;
            DataTable dtEmployees = null;

            DataView dvSlrSheet = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            int endGenericColumn = 0;
            #endregion Variable

            try
            {
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
                var ldateOfMonth = daysInMonth + "-" + monthName + "-" + year;
                var fdateOfMonth = "1" + "-" + monthName + "-" + year;
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }


                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();

                #endregion Variable

                #region DataSet

                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                objRpt.GetExtraAbsent(plantId, parameters, month.ToInt(), year.ToInt(), out dsExtraAbsent);

                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);

                //Sql Salary Structure 
                List<SalarySheetReportUD> listdsSlrStr = new List<SalarySheetReportUD>();

                //Sql Salary Process 

                Dictionary<string, List<DataRow>> dicEmpComplianceInfo = GetEmployeeSalaryInfoCompliance(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, salaryProcessId, payRollGroup, parameters);


                DataTable dtSalaryHeadSheet;
                List<SalarySheetReportUD> listdsSlrProc = new List<SalarySheetReportUD>();
                GetEmployeeInfoDetailSalaryLogWise(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, out dsEmpLoyeeInfo);//Sql Query For Salary  Data
                Dictionary<string, List<DataRow>> dicEmpSalry = GetEmployeeSalaryInfoDetail(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, salaryProcessId, payRollGroup, parameters, out dtSalaryHeadSheet);

                if (dicEmpSalry.First().Value[0].Table.Rows.Count > 0)
                {
                    listdsSlrProc = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    listdsSlrStr = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    dtEmployees = dsEmpLoyeeInfo.Tables[0];//dicEmpSalry.First().Value[0].Table;
                }
                else
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }

                dvSlrSheet = new DataView();

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);

                objRpt.SelectedPlant(plantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(2);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 6;
                xlsCol = 1;

                #region Column Variables
                int ColSr = 0, ColIDNo = 0, ColName = 0, ColDOJ = 0, ColDOS = 0, cDept = 0, cSec = 0, cSubSec = 0, cLine = 0, cPayrollGroup = 0, cJobLocation = 0, cGender = 0,
                    cGrade = 0, ColGVDG = 0, ColGrs = 0, colPayDays = 0, ColPdDy = 0, ColLate = 0, ColAbDy = 0, ColHlDy = 0, ColWkOf = 0, ColLv = 0, ColMLv = 0
                   , ColLWP = 0, cDMP = 0, ColExtraAbsent = 0, colEmpCurrentStat = 0, colEmpStatus = 0, cPaymentMode = 0, cUnit = 0, ColTotalOTHR = 0, colDirectManpowerCost = 0;
                int npstruct = 0;
                int colExtOTRate = 0; int colExtOTAmount = 0; int colNightBillAmount = 0; int colNightBillRate = 0;

                double netPaymentAmount = 0.00;


                #endregion

                //1
                SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);
                SetCellValue("ID No.", sheet1, xlsRow, ref xlsCol, out ColIDNo, 12);
                SetCellValue("Name", sheet1, xlsRow, ref xlsCol, out ColName, 17);
                SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out ColDOJ, 12);
                SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out ColDOS, 12);
                SetCellValue("EmployeeCurrentStatus", sheet1, xlsRow, ref xlsCol, out colEmpCurrentStat, 12);
                SetCellValue("EmployeeSatatus", sheet1, xlsRow, ref xlsCol, out colEmpStatus, 12);
                SetCellValue("Gender", sheet1, xlsRow, ref xlsCol, out cGender, 12);
                SetCellValue("Designation", sheet1, xlsRow, ref xlsCol, out ColGVDG, 25);
                SetCellValue("Employee Category", sheet1, xlsRow, ref xlsCol, out int colEmpCategory, 25);
                SetCellValue("Department", sheet1, xlsRow, ref xlsCol, out cDept, 25);
                SetCellValue("Section", sheet1, xlsRow, ref xlsCol, out cSec, 25);
                SetCellValue("SubSection", sheet1, xlsRow, ref xlsCol, out cSubSec, 25);
                SetCellValue("Unit", sheet1, xlsRow, ref xlsCol, out cUnit, 25);
                SetCellValue("Line", sheet1, xlsRow, ref xlsCol, out cLine, 25);
                SetCellValue("JobLocation", sheet1, xlsRow, ref xlsCol, out cJobLocation, 25);
                SetCellValue("Payroll group", sheet1, xlsRow, ref xlsCol, out cPayrollGroup, 25);
                SetCellValue("Payment mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                SetCellValue("Grade", sheet1, xlsRow, ref xlsCol, out cGrade, 25);
                SetCellValue("Direct Manpower Cost", sheet1, xlsRow, ref xlsCol, out colDirectManpowerCost, 25);

                SetCellValue("Pay Days", sheet1, xlsRow, ref xlsCol, out colPayDays, 5);
                SetCellValue("Present", sheet1, xlsRow, ref xlsCol, out ColPdDy, 9);
                SetCellValue("Late", sheet1, xlsRow, ref xlsCol, out ColLate, 9);
                SetCellValue("Absent", sheet1, xlsRow, ref xlsCol, out ColAbDy, 9);
                SetCellValue("LWP", sheet1, xlsRow, ref xlsCol, out ColLWP, 9);
                SetCellValue("Extra Absent", sheet1, xlsRow, ref xlsCol, out ColExtraAbsent, 9);
                SetCellValue("Holiday", sheet1, xlsRow, ref xlsCol, out ColHlDy, 9);
                SetCellValue("WeekOff", sheet1, xlsRow, ref xlsCol, out ColWkOf, 9);
                SetCellValue("Leave", sheet1, xlsRow, ref xlsCol, out ColLv, 11);
                SetCellValue("Maternity Leave", sheet1, xlsRow, ref xlsCol, out ColMLv, 20);
                SetCellValue("Total Ot Hr", sheet1, xlsRow, ref xlsCol, out ColTotalOTHR, 11);
                endGenericColumn = xlsCol;

                //SR to
                sheet1.Range[xlsRow, ColSr].Text = "Employee Information";
                sheet1.Range[xlsRow, ColSr, xlsRow, ColTotalOTHR].Merge();
                //xlsCol += 1;
                ColGrs = ColTotalOTHR;
                // 9

                var _count_earning_head = 0;
                var _count_earning_ctchead = 0;
                var _count_deducting_head = 0;
                var _total_head_count = 0;

                Dictionary<string, SalaryHeadSequence> shtList = null;

                CreateDynamicSHead(dtSalaryHeadSheet, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out shtList);

                List<SalaryHeadSequence> salList = new List<SalaryHeadSequence>();
                salList.AddRange(shtList.Values);

                xlsCol--;

                //Header Col
                if (_count_earning_ctchead > 0)
                {
                    sheet1.Range[xlsRow, ColGrs + 1].Text = "Earning head";
                    sheet1.Range[xlsRow, ColGrs + 1, xlsRow, ColGrs + _count_earning_head + _count_earning_ctchead].Merge();
                }

                var ds = ColGrs + 1 + _count_earning_head + _count_earning_ctchead;

                if (_count_deducting_head > 0)
                {
                    sheet1.Range[xlsRow, ds].Text = "Deduction head";
                    sheet1.Range[xlsRow, ds, xlsRow, ds + _count_deducting_head - 1].Merge();
                }
                npstruct = 0;
                if (shtList.Count > 0)
                {
                    xlsCol++;
                    npstruct = ColGrs + shtList.Count + 1;
                    //sheet1.Range[xlsRow + 1, npstruct].Text = "Net Payable";
                    //sheet1.Range[xlsRow, npstruct].ColumnWidth = 14;
                    //sheet1.Range[xlsRow, npstruct, xlsRow + 1, npstruct].Merge();
                    xlsCol = npstruct;
                }

                string shift = @"	 select distinct da.AllowanceDailyId,ad.UserName,ad.Id,ad.IsVoucherPayment
							 from [dbo].[DailyAllowanceTransaction] as da
							 left join [HKP].[AllowanceDaily] ad on ad.Id=da.AllowanceDailyId
							 where da.PlantId='" + plantId + @"' and da.WorkDate between '" + fdateOfMonth + @"' and '" + ldateOfMonth + @"' 
                            and ad.IsVoucherPayment=1";
                DataTable dt = _sqlRepository.GetDataTable(shift);

                Dictionary<string, int> dicShift = new Dictionary<string, int>();

                int COL = npstruct;
                int startColForDailyAllowance = COL;

                dt.Columns.Add("RateCol", typeof(string));
                dt.Columns.Add("AmountCol", typeof(string));


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dicShift.Add(dt.Rows[i]["Id"].ToString(), COL);

                    sheet1[xlsRow, COL].Text = dt.Rows[i]["UserName"].ToString();
                    sheet1.Range[xlsRow, COL, xlsRow, COL + 1].Merge();
                    sheet1.Range[xlsRow, COL, xlsRow, COL + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, COL, xlsRow, COL + 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, COL, xlsRow, COL + 1].BorderAround(ExcelLineStyle.Thin);
                    sheet1.Range[xlsRow, COL, xlsRow, COL + 1].CellStyle.Interior.ColorIndex = ExcelKnownColors.LightGreen;

                    sheet1.Range[xlsRow + 1, COL].Text = "Rate";
                    sheet1.Range[xlsRow + 1, COL].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow + 1, COL].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow + 1, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow + 1, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow + 1, COL].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    dt.Rows[i]["RateCol"] = COL.ToString();
                    COL++;
                    sheet1.Range[xlsRow + 1, COL].Text = "Amount";
                    sheet1.Range[xlsRow + 1, COL].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow + 1, COL].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow + 1, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow + 1, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow + 1, COL].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    //newColumn2.DefaultValue = COL.ToString();
                    dt.Rows[i]["AmountCol"] = COL.ToString();

                    COL++;
                }



                //SetCellValue("Ext OT Rate", sheet1, xlsRow, ref xlsCol, out colExtOTRate, 11);
                //SetCellValue("Ext OT Amount", sheet1, xlsRow, ref xlsCol, out colExtOTAmount, 11);
                //sheet1.Range[xlsRow, colExtOTRate].Text = "Extra OT";
                //sheet1.Range[xlsRow, colExtOTRate, xlsRow, colExtOTAmount].Merge();
                //SetCellValue("Night Bill Rate", sheet1, xlsRow, ref xlsCol, out colNightBillRate, 11);
                //SetCellValue("Night Bill Amount", sheet1, xlsRow, ref xlsCol, out colNightBillAmount, 11);
                //sheet1.Range[xlsRow, colNightBillRate].Text = "Night Bill";
                //sheet1.Range[xlsRow, colNightBillRate, xlsRow, colNightBillAmount].Merge();

                npstruct = COL;
                sheet1.Range[xlsRow + 1, npstruct].Text = "Net Payable";
                sheet1.Range[xlsRow, npstruct].ColumnWidth = 14;
                sheet1.Range[xlsRow, npstruct, xlsRow + 1, npstruct].Merge();

                xlsCol++;


                xlsCol++;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No.";
                sheet1.Range[xlsRow - 1, 1].ColumnWidth = 14;
                sheet1.Range[xlsRow - 1, 1, xlsRow - 1, 3].Merge();
                sheet1.Range[xlsRow, 1, xlsRow + 1, npstruct].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].VerticalAlignment = ExcelVAlign.VAlignCenter;
                endXlsCol = npstruct;


                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                Param param = new Param();
                param.CompanyGroupId = companyGroupId;
                param.CompanyId = companyId;

                string FactoryAddress = string.Empty;
                try
                {

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
                catch (Exception ex)
                {
                }


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
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
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
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Salary Sheet For The Month Of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                //var strRptDateRange = "";
                //strRptDateRange = "For The Month Of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                //sheet1.Range[xlsRow, 3].Text = strRptDateRange;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                //sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                //sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                var SrNo = 0;
                var x = "";

                var oRU = new ReportUtility();

                xlsRow = RowIndex;

                xlsRow--;
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    #region EmpInfo
                    try
                    {
                        netPaymentAmount = 0;
                        SrNo += 1;
                        x = dtEmployees.Rows[i]["EmpSystemID"].ToString().Trim();

                        //1
                        sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                        sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //2
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, ColIDNo].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, ColIDNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColIDNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //3
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                            sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJ"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJ"].ToString();
                        sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOS"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOS].Text = dtEmployees.Rows[i]["DOS"].ToString();
                        sheet1.Range[xlsRow, ColDOS].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOS].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString()) == false)
                            sheet1.Range[xlsRow, colEmpCurrentStat].Text = dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString();
                        sheet1.Range[xlsRow, colEmpCurrentStat].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCurrentStat].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeStatus"].ToString()) == false)
                            sheet1.Range[xlsRow, colEmpStatus].Text = dtEmployees.Rows[i]["EmployeeStatus"].ToString();
                        sheet1.Range[xlsRow, colEmpStatus].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LegalDesignation"].ToString()) == false)
                            sheet1.Range[xlsRow, ColGVDG].Text = dtEmployees.Rows[i]["LegalDesignation"].ToString();
                        sheet1.Range[xlsRow, ColGVDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColGVDG].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmpCategoryName"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colEmpCategory].Text = dtEmployees.Rows[i]["EmpCategoryName"].ToString();
                        sheet1.Range[xlsRow, colEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4.2
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DepartmentName"].ToString()) == false)
                            sheet1.Range[xlsRow, cDept].Text = dtEmployees.Rows[i]["DepartmentName"].ToString();
                        sheet1.Range[xlsRow, cDept].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cDept].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SectionName"].ToString()) == false)
                            sheet1.Range[xlsRow, cSec].Text = dtEmployees.Rows[i]["SectionName"].ToString();
                        sheet1.Range[xlsRow, cSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cSec].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SubSectionName"].ToString()) == false)
                            sheet1.Range[xlsRow, cSubSec].Text = dtEmployees.Rows[i]["SubSectionName"].ToString();
                        sheet1.Range[xlsRow, cSubSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cSubSec].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["UnitName"].ToString()) == false)
                            sheet1.Range[xlsRow, cUnit].Text = dtEmployees.Rows[i]["UnitName"].ToString();
                        sheet1.Range[xlsRow, cUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PaymentMode"].ToString()) == false)
                            sheet1.Range[xlsRow, cPaymentMode].Text = dtEmployees.Rows[i]["PaymentMode"].ToString();
                        sheet1.Range[xlsRow, cPaymentMode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cPaymentMode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                            sheet1.Range[xlsRow, cGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                        sheet1.Range[xlsRow, cGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["JobLocation"].ToString()) == false)
                            sheet1.Range[xlsRow, cJobLocation].Text = dtEmployees.Rows[i]["JobLocation"].ToString();
                        sheet1.Range[xlsRow, cJobLocation].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cJobLocation].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LineName"].ToString()) == false)
                            sheet1.Range[xlsRow, cLine].Text = dtEmployees.Rows[i]["LineName"].ToString();
                        sheet1.Range[xlsRow, cLine].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cLine].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PayRollGroup"].ToString()) == false)
                            sheet1.Range[xlsRow, cPayrollGroup].Text = dtEmployees.Rows[i]["PayRollGroup"].ToString();
                        sheet1.Range[xlsRow, cPayrollGroup].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cPayrollGroup].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //5
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["GradeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, cGrade].Text = dtEmployees.Rows[i]["GradeCode"].ToString();
                        sheet1.Range[xlsRow, cGrade].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGrade].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //if (string.IsNullOrEmpty(dtEmployees.Rows[i]["IsDirect"].ToString()) == false)
                        //    sheet1.Range[xlsRow, cDMP].Text = dtEmployees.Rows[i]["IsDirect"].ToString();
                        //sheet1.Range[xlsRow, cDMP].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        //sheet1.Range[xlsRow, cDMP].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DirectManpowerCost"].ToString()) == false)
                            sheet1.Range[xlsRow, colDirectManpowerCost].Text = dtEmployees.Rows[i]["DirectManpowerCost"].ToString();
                        sheet1.Range[xlsRow, colDirectManpowerCost].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colDirectManpowerCost].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                            sheet1.Range[xlsRow, cGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                        sheet1.Range[xlsRow, cGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (dicEmpComplianceInfo.ContainsKey(dtEmployees.Rows[i]["EmpSystemID"].ToString()))
                        {
                            List<DataRow> drCompliance = dicEmpComplianceInfo[dtEmployees.Rows[i]["EmpSystemID"].ToString()];

                            if (dt.Rows.Count > 0)
                            {
                                //for (int cii = 0; cii < dt.Rows.Count; cii++) 
                                //{
                                //    try
                                //    {
                                for (int CI = 0; CI < drCompliance.Count; CI++)
                                {
                                    dt.DefaultView.RowFilter = "Id='" + drCompliance[CI]["AllowanceDailyId"] + "'";
                                    sheet1[xlsRow, Convert.ToInt16(dt.DefaultView[0]["RateCol"])].Number = Convert.ToDouble(drCompliance[CI]["Rate"].ToString());
                                    sheet1[xlsRow, Convert.ToInt16(dt.DefaultView[0]["RateCol"])].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1[xlsRow, Convert.ToInt16(dt.DefaultView[0]["RateCol"])].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    //startColForDailyAllowance++;
                                    netPaymentAmount += clsStaticInfo.dbl(drCompliance[CI]["Amount"].ToString());
                                    sheet1[xlsRow, Convert.ToInt16(dt.DefaultView[0]["AmountCol"])].Number = clsStaticInfo.dbl(drCompliance[CI]["Amount"].ToString());
                                    sheet1[xlsRow, Convert.ToInt16(dt.DefaultView[0]["AmountCol"])].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1[xlsRow, Convert.ToInt16(dt.DefaultView[0]["AmountCol"])].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    //startColForDailyAllowance++; 
                                    //startColForDailyAllowance++;
                                }
                                //}
                                //catch (Exception)
                                //{


                                //}

                                //if (drCompliance[CI]["AllowanceType"].ToString().ToUpper() == "EXTRA OT")
                                //{
                                //    sheet1.Range[xlsRow, colExtOTRate].Number = Convert.ToDouble(drCompliance[CI]["Rate"].ToString());
                                //    sheet1.Range[xlsRow, colExtOTRate].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                //    sheet1.Range[xlsRow, colExtOTRate].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                //    netPaymentAmount += clsStaticInfo.dbl(drCompliance[CI]["Amount"].ToString());
                                //    sheet1.Range[xlsRow, colExtOTAmount].Number = clsStaticInfo.dbl(drCompliance[CI]["Amount"].ToString());
                                //    sheet1.Range[xlsRow, colExtOTAmount].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                //    sheet1.Range[xlsRow, colExtOTAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                //}
                                //if (drCompliance[CI]["AllowanceType"].ToString().ToUpper() == "NIGHT ALLOWANCE")
                                //{
                                //    sheet1.Range[xlsRow, colNightBillRate].Number = Convert.ToDouble(drCompliance[CI]["Rate"].ToString());
                                //    sheet1.Range[xlsRow, colNightBillAmount].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                //    sheet1.Range[xlsRow, colNightBillAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                //    netPaymentAmount += clsStaticInfo.dbl(drCompliance[CI]["Amount"].ToString());

                                //    sheet1.Range[xlsRow, colNightBillAmount].Number = clsStaticInfo.dbl(drCompliance[CI]["Amount"].ToString());
                                //    sheet1.Range[xlsRow, colNightBillAmount].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                //    sheet1.Range[xlsRow, colNightBillAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                //}
                            }
                            //}
                        }

                        //5 "Section", "SubSection", 

                        #endregion
                        #region Attendance Data
                        //if (dtEmpAttdnInfo.Rows.Count > 0)
                        //{
                        double _ExtraAbsent = 0;
                        dvExtraAbsent.RowFilter = "EmpSystemID='" + dtEmployees.Rows[i]["EmpSystemID"].ToString() + "' ";
                        _ExtraAbsent = dvExtraAbsent.Count;
                        var payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString());
                        SetCellTextAttdn(sheet1, xlsRow, colPayDays, payDays);
                        SetCellTextAttdn(sheet1, xlsRow, ColPdDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalPresent"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLate, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLate"].ToString()));
                        SetCellTextNumber(sheet1, xlsRow, ColAbDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLWP"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLWP, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLWP"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColExtraAbsent, _ExtraAbsent);
                        SetCellTextAttdn(sheet1, xlsRow, ColHlDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalHoliDay"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColWkOf, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLv"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColMLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalMLv"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColTotalOTHR, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalOTHr"].ToString()) / 60);

                        //}
                        #endregion

                        #region ------------------------------------Salary Sheet----------------------------------
                        if (dicEmpSalry.ContainsKey(dtEmployees.Rows[i]["EmpSystemID"].ToString()))
                        {
                            List<DataRow> drSalaryHeadCollection = dicEmpSalry[dtEmployees.Rows[i]["EmpSystemID"].ToString()];
                            if (drSalaryHeadCollection.Count > 0)
                            {
                                for (int CI = 0; CI < drSalaryHeadCollection.Count; CI++)
                                {
                                    if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "NET PAYABLE")
                                    {
                                        sheet1.Range[xlsRow, npstruct].Number = netPaymentAmount + Convert.ToDouble(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                        continue;
                                    }
                                    try
                                    {
                                        SalaryHeadSequence xx = shtList[drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();
                                        if (xx != null)
                                        {
                                            if (drSalaryHeadCollection[CI]["HeadType"].ToString() == "D")
                                            {
                                                sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()) * (-1);
                                            }
                                            else
                                            {

                                                sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                            }
                                            sheet1.Range[xlsRow, xx.XLColIndex].NumberFormat = oRU.NumberFormatInt();
                                            sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                            sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                        throw ex;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }


                    #endregion

                    xlsRow++;
                }//for emp count
                int sheetEndXlsRow = xlsRow - 1;
                #endregion ----------------------Data-----------------------

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var freezePan = RowIndex - 1;
                sheet1.UsedRange["A" + freezePan].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 10;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "EmpSalaryInfo";
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                #endregion

                workbook.Version = ExcelVersion.Excel2016;
                //var strFileName = "EmpSalaryStrSheet-" + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + Convert.ToDateTime(fdateOfMonth).ToString("yyyy") + "-" + para.SalaryProcessId + ".xls";

                if (isTopSheet == true)
                {
                    #region Salary Summary
                    string filePath = HostingEnvironment.MapPath("~/") + "TempSalaeySummary.xlsx";
                    workbook.SaveAs(filePath);
                    workbook = application.Workbooks.Open(filePath);

                    IWorksheet worksheet = workbook.Worksheets[0];
                    worksheet.Move(1);

                    #region PivotSheet1
                    IWorksheet pivotSheet = workbook.Worksheets[0];
                    pivotSheet.Name = "Summary";

                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = pivotSheet.GetColumnWidth(1) + pivotSheet.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((pivotSheet.GetRowHeight(1) + pivotSheet.GetRowHeight(2) + pivotSheet.GetRowHeight(3) + pivotSheet.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = pivotSheet.Pictures.AddPicture(1, 1, companyLogo);

                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    #region Report Header
                    xlsRow = 1;
                    xlsCol = 1;


                    pivotSheet.Range[xlsRow, 3].Text = CmpName;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow += 1;

                    pivotSheet.Range[xlsRow, 3].Text = FactoryName;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    xlsRow += 1;

                    pivotSheet.Range[xlsRow, 3].Text = FactoryAddress;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    xlsRow += 1;
                    pivotSheet.Range[xlsRow, 3].Text = "Salary Summary for the month of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    #endregion

                    pivotSheet.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;

                    IRange iRange = worksheet["A7:" + clsStaticInfo.GetxlsCol(npstruct) + (sheetEndXlsRow)];
                    IPivotCache cache2 = workbook.PivotCaches.Add(iRange);
                    IPivotCache cache = workbook.PivotCaches.Add(iRange);


                    #region Second Pivot table
                    pivotSheet.Range[xlsRow + 2, 1].Text = "EmployeeStatus, PaymentMode, Department Wise Salary Summary";
                    pivotSheet.Range[xlsRow + 2, 1, xlsRow + 2, 5].Merge();
                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Size = 12;

                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable2 = pivotSheet.PivotTables.Add("PivotTable2", pivotSheet["A8"], cache);

                    pivotTable2.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[cPaymentMode - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[cDept - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable2_1 = pivotSheet.PivotTables["PivotTable2"];
                    pivotTable2_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable2_1.Options.ShowDrillIndicators = false;

                    pivotTable2_1.DisplayFieldCaptions = true;

                    //Add data field
                    IPivotField field2 = pivotTable2_1.Fields[ColSr - 1];
                    pivotTable2_1.DataFields.Add(field2, "Total Employees", PivotSubtotalTypes.Count);
                    int pivotColumnCount = 0;
                    IPivotField fieldGross = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                            }
                            try
                            {
                                if (ob.HeadType == "D")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            catch (Exception ex)
                            {

                                //throw ex;
                            }

                        }
                    }
                    try
                    {
                        fieldGross = null;
                        pivotColumnCount++;
                        fieldGross = pivotTable2_1.Fields[npstruct - 1];
                        pivotTable2_1.DataFields.Add(fieldGross, "Net Payable", PivotSubtotalTypes.Sum);
                        fieldGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                    }
                    catch (Exception)
                    {

                    }

                    pivotTable2_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;

                    int totalColumns = pivotTable2_1.RowFields.Count + pivotColumnCount;

                    int lastCloumn = totalColumns + 2;

                    #endregion
                    #region PivotTable2

                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].Text = "Employee Category Wise Salary Summary";
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2, xlsRow + 2, lastCloumn + 5].Merge();
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet[clsStaticInfo.GetxlsCol(lastCloumn + 2) + "8"], cache);

                    //Add Pivot table fields (Row and Column fields)
                    pivotTable.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[colEmpCategory - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[cDept - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable1 = pivotSheet.PivotTables["PivotTable1"];
                    pivotTable1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable1.Options.ShowDrillIndicators = false;

                    pivotTable1.DisplayFieldCaptions = true;
                    pivotTable1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;

                    //Add data field
                    IPivotField field = pivotTable.Fields[ColSr - 1];
                    pivotTable.DataFields.Add(field, "Total Employee", PivotSubtotalTypes.Count);

                    int pivot2ColumnCount = 0;
                    IPivotField fieldGross2 = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross2 = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            if (ob.HeadType == "D")
                            {
                                pivot2ColumnCount++;
                                fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                            }

                        }
                    }
                    fieldGross2 = null;
                    pivot2ColumnCount++;
                    fieldGross2 = pivotTable.Fields[npstruct - 1];
                    pivotTable.DataFields.Add(fieldGross2, "Net Payable", PivotSubtotalTypes.Sum);
                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(0, "");


                    #endregion

                    #region PivotTable3

                    totalColumns += pivotTable.RowFields.Count + pivot2ColumnCount;


                    lastCloumn += totalColumns - 10;

                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].Text = "EmployeeStatus ,Employee Category and Department Wise  Salary Summary";
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2, xlsRow + 2, lastCloumn + 5].Merge();
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Bold = true;

                    //Create "PivotTable1" with the cache at the specified range
                    IPivotTable pivotTable3 = pivotSheet.PivotTables.Add("PivotTable13", pivotSheet[clsStaticInfo.GetxlsCol(lastCloumn + 2) + "8"], cache);

                    //Add Pivot table fields (Row and Column fields)
                    pivotTable3.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable3.Fields[cDept - 1].Axis = PivotAxisTypes.Row;
                    pivotTable3.Fields[colEmpCategory - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable13_1 = pivotSheet.PivotTables["PivotTable13"];
                    pivotTable13_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable13_1.Options.ShowDrillIndicators = false;

                    pivotTable13_1.DisplayFieldCaptions = true;
                    pivotTable13_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;


                    //Add data field
                    IPivotField fields3 = pivotTable13_1.Fields[ColSr - 1];
                    pivotTable13_1.DataFields.Add(fields3, "Total Employee", PivotSubtotalTypes.Count);

                    int pivot3ColumnCount = 0;
                    IPivotField fieldGross3 = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross3 = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivot3ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivot2ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }

                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            if (ob.HeadType == "D")
                            {
                                pivot2ColumnCount++;
                                fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                            }

                        }
                    }
                    fieldGross3 = null;
                    pivot2ColumnCount++;
                    fieldGross3 = pivotTable13_1.Fields[npstruct - 1];
                    pivotTable13_1.DataFields.Add(fieldGross3, "Net Payable", PivotSubtotalTypes.Sum);
                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(0, "");


                    #endregion
                    pivotSheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    pivotSheet.IsGridLinesVisible = false;
                    pivotSheet.IsDisplayZeros = false;

                    pivotSheet.UsedRange.WrapText = false;

                    #endregion
                    #endregion

                    workbook.ActiveSheetIndex = 0;
                }

                return workbook;
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //objRpt = null;
                //excelEngine = null;
                //application = null;
                //workbook = null;
            }
        }


        #region PaySlip
        public IWorkbook GetEmployeePaySlip(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, Dictionary<string, string> parameters, string languageId, bool isActive, bool isSeperated, bool isMaternity)
        {
            #region Variable
            ReportUtility ru = null;

            clsReport objRpt = null;

            DataSet dsSlrProc = null;
            DataSet dsSlrStructure = null;
            DataView dvSlrProc = null;
            DataSet dsHeading = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsLeaveInfo = null;
            DataView dvLeaveEmp = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            int _x = 0;
            double _basic = 0;
            double _netPay = 0;


            #endregion Variable

            try
            {
                ru = new ReportUtility();
                objRpt = new clsReport();

                var labelList = ru.LocalLanguageLabelList(plantId, languageId);
                var localLanguage = "";

                var printFont = "";
                bool isLocalLanguage = false;
                localLanguage = ru.LocalLanguageListSql(plantId, languageId, out isLocalLanguage);
                if (localLanguage == "Bengali")
                {
                    printFont = "SolaimanLipi";
                }
                if (localLanguage == "Hindi")
                {
                    printFont = "Aparajita";
                }
                else
                {
                    printFont = "Arial Narrow";

                }


                ParamList para = new ParamList();

                para.PlantId = plantId;
                //para.EmployeeId = lblEmpSystemID.Text;
                para.FromDate = "01-" + bplib.clsWebLib.GetMonthName(month) + "-" + year;

                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month

                para.ToDate = daysInMonth + "-" + bplib.clsWebLib.GetMonthName(month) + "-" + year;

                //para.DepartmentId = ddlDepartment.SelectedValue.ToString().Trim();
                para.UnitId = "ALL";
                para.SubSectionId = "ALL";
                para.SectionId = "ALL";
                para.DivisionId = "ALL";
                //para.SystemAdmin = (string)Session["sa"].ToString().Trim();
                //para.ControlAdmin = (string)Session["ca"].ToString().Trim();

                #region DataSet
                //GetSalaryInfoSlrProcIDWiseCombinedForPaySlip(plantId, para.FromDate, para.ToDate, salaryProcessId, "", parameters, languageId, isActive, isSeperated, isMaternity, out dsSlrProc);//Sql Query For Salary  Data
                DataSet dsEmpLoyeeInfo = null;
                DataTable dtSalaryHeadSheet = null;
                GetEmployeeInfoDetailSalaryLogWise(companyGroupId, companyId, plantId, para.FromDate, para.ToDate, salaryProcessId, "", parameters, isActive, isSeperated, isMaternity, out dsEmpLoyeeInfo);//Sql Query For Salary  Data

                Dictionary<string, List<DataRow>> dicEmpSalry = GetEmployeeSalaryInfoDetailPaySlip(companyGroupId, companyId, plantId, para.FromDate, para.ToDate, languageId, parameters, isActive, isSeperated, isMaternity, out dtSalaryHeadSheet);

                Dictionary<string, string> dicPF = GetEmployeePFESIC("PF");
                Dictionary<string, string> dicESIC = GetEmployeePFESIC("ESIC");


                // GetEmpSalaryInformationRpt(plantId, para.ToDate, "", parameters, out dsSlrStructure);

                DataSet dsGrade = null;
                objRpt.GetGrade(parameters, month, year, out dsGrade);//GetGrade
                //objRpt.GetSalaryInfoSlrProcIDWise(ddlSlrProcID.Text.Trim(), ddlPlant.SelectedValue.Trim(), lblEmpSystemID.Text, ddlStatus.SelectedValue.Trim(), out dsSlrProc);
                dvSlrProc = new DataView();
                //dvSlrProc.Table = dsSlrProc.Tables[0];
                DataTable dtEmpInfo = dsEmpLoyeeInfo.Tables[0];//dvSlrProc.ToTable(true, "EmpInfoSystemID", "LeaveDays", "TotalLWP", "EmployeeName", "PaymentMode", "EmployeeCode", "legalDesignation", "Grade", "DOJ", "EmployeeStatus", "DOS", "DOB", "UnitName", "Section", "SubSection", "Department", "BankAccNo", "BankName", "UANNo", "ESICNo", "EmpCategoryName", "PresentDays", "AbsentDays", "WeekOff", "Holiday", "NationalID");

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);

                objRpt.SelectedPlant(plantId, out dsFactory);

                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                objRpt.GetExtraAbsent(plantId, parameters, Convert.ToInt32(month), Convert.ToInt32(year), out dsExtraAbsent);
                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);
                #endregion DataSet
                if (dtEmpInfo.Rows.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;
                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;

                    #region Variable For Data
                    double EL = 0.00;
                    double CL = 0.00;
                    double SL = 0.00;
                    double SLESIC = 0.00;
                    double LWP = 0.00;

                    int startRow = 1;
                    int EmpCounter = 0; int SrNo = 0; int ColGrossHd = 0;
                    string x = "";
                    #endregion Variable For Data
                    int _Info_Last_Row = 0;
                    int headerEndxlsCol = 0;
                    int headerStartXlsRow = 0;

                    int empDetailFirstXlsRow = 0;

                    int empPaySlipDetailXlsRow = 0;
                    Dictionary<string, List<DataRow>> dicLeaveEmp = objRpt.GetEmpLeaveInfoPaySlipSaad(para);
                    xlsRow = startRow;


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
                    for (int i = 0; i <= dtEmpInfo.Rows.Count - 1; i++)
                    {
                        #region ******************Report Header******************

                        xlsCol = 1;
                        string FactoryAddress = string.Empty;
                        headerEndxlsCol = xlsCol + 14;
                        headerStartXlsRow = xlsRow;

                        sheet1.Range[xlsRow, xlsCol].Text = CmpName + "::" + FactoryName + ".";
                        sheet1.Range[xlsRow, xlsCol].WrapText = true;
                        sheet1.Range[xlsRow, 1, xlsRow, xlsCol + 11].Merge();
                        sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;

                        string strRptDateRange = "";
                        strRptDateRange = "Pay Slip For " + Convert.ToDateTime(para.FromDate).ToString("MMM") + ", " + Convert.ToDateTime(para.FromDate).ToString("yyyy");
                        sheet1.Range[xlsRow, xlsCol + 12].Text = strRptDateRange;
                        sheet1.Range[xlsRow, xlsCol + 12, xlsRow, xlsCol + 14].Merge();
                        sheet1.Range[headerStartXlsRow, xlsCol + 11, xlsRow, headerEndxlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;

                        sheet1.Range[headerStartXlsRow, 1, xlsRow, headerEndxlsCol].CellStyle.Font.Bold = true;
                        sheet1.Range[headerStartXlsRow, 1, xlsRow, headerEndxlsCol].CellStyle.Font.FontName = "Arial Narrow";

                        sheet1.Range[headerStartXlsRow, 1, xlsRow, headerEndxlsCol].RowHeight = 20;

                        xlsRow += 1;
                        xlsCol = 1;
                        #endregion ******************Report Header******************

                        para.EmployeeId = dtEmpInfo.Rows[i]["EmpSystemId"].ToString();
                        List<DataRow> drLeaveEmp = null;
                        EL = 0.00;
                        CL = 0.00;
                        SL = 0.00;
                        SLESIC = 0.00;
                        LWP = 0.00;
                        //if (dicLeaveEmp.ContainsKey(dtEmpInfo.Rows[i]["EmpSystemID"].ToString()))
                        //{
                        //    drLeaveEmp = dicLeaveEmp[dtEmpInfo.Rows[i]["EmpSystemID"].ToString()];

                        //    EL = GetLeaveEmp(drLeaveEmp, "PL");
                        //    CL = GetLeaveEmp(drLeaveEmp, "CL");
                        //    SL = GetLeaveEmp(drLeaveEmp, "SL");
                        //    SLESIC = GetLeaveEmp(drLeaveEmp, "SL (ESIC)");
                        //    LWP = GetLeaveEmp(drLeaveEmp, "LWP");
                        //}


                        int _maxRow = 0;
                        empDetailFirstXlsRow = 0;
                        if ((string.Compare(x.ToUpper(), dtEmpInfo.Rows[i]["EmpSystemId"].ToString().Trim().ToUpper())) != 0)
                        {
                            xlsCol = 1;
                            empPaySlipDetailXlsRow = 0;

                            xlsRow += 1;
                            xlsCol = 1;

                            empDetailFirstXlsRow = xlsRow;

                            //_OTHours = 0.00;//Convert.ToDouble(dtEmpInfo.Rows[i]["TotalOTHr"].ToString());//
                            #region------------------Header------------------

                            sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.EmployeeCode.ToString(), "Emp Code");
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                            sheet1.Range[xlsRow, xlsCol + 2].Text = ":   " + dtEmpInfo.Rows[i]["EmployeeCode"].ToString();
                            sheet1.Range[xlsRow, xlsCol + 2, xlsRow, xlsCol + 4].Merge();

                            sheet1.Range[xlsRow, xlsCol + 10].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.ModeOfPayment.ToString(), "Payment Mode");
                            sheet1.Range[xlsRow, xlsCol + 10, xlsRow, xlsCol + 11].Merge();
                            sheet1.Range[xlsRow, xlsCol + 12].Text = ":   " + dtEmpInfo.Rows[i]["PaymentMode"].ToString();//PaymentMode
                            sheet1.Range[xlsRow, xlsCol + 12, xlsRow, xlsCol + 14].Merge();

                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Name.ToString(), "Name");
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                            sheet1.Range[xlsRow + 2, xlsCol + 1].ColumnWidth = 12;
                            sheet1.Range[xlsRow, xlsCol + 2].Text = ":   " + dtEmpInfo.Rows[i]["EmployeeName"].ToString();//
                            sheet1.Range[xlsRow, xlsCol + 2, xlsRow, xlsCol + 9].Merge();

                            sheet1.Range[xlsRow, xlsCol + 10].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Grade.ToString(), "Grade");
                            sheet1.Range[xlsRow, xlsCol + 10, xlsRow, xlsCol + 11].Merge();
                            string _grade = string.Empty;
                            if (dsGrade.Tables[0].Rows.Count > 0)
                            {
                                _grade = dsGrade.Tables[0].Rows[0]["Grade"].ToString();
                            }
                            sheet1.Range[xlsRow, xlsCol + 12].Text = ":   " + _grade;
                            sheet1.Range[xlsRow, xlsCol + 12, xlsRow, xlsCol + 14].Merge();

                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.DOJ.ToString(), "DOJ");
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                            sheet1.Range[xlsRow, xlsCol + 2].Text = ":   " + dtEmpInfo.Rows[i]["DOJ"].ToString();
                            sheet1.Range[xlsRow, xlsCol + 2, xlsRow, xlsCol + 4].Merge();
                            sheet1.Range[xlsRow, xlsCol + 5].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.EmpStatus.ToString(), "Status");
                            sheet1.Range[xlsRow, xlsCol + 5, xlsRow, xlsCol + 6].Merge();
                            sheet1.Range[xlsRow, xlsCol + 7].Text = ":   " + dtEmpInfo.Rows[i]["EmployeeStatus"].ToString();
                            sheet1.Range[xlsRow, xlsCol + 7, xlsRow, xlsCol + 9].Merge();

                            sheet1.Range[xlsRow, xlsCol + 10].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.DOS.ToString(), "DOS");
                            sheet1.Range[xlsRow, xlsCol + 10, xlsRow, xlsCol + 11].Merge();
                            sheet1.Range[xlsRow, xlsCol + 12].Text = ":   " + dtEmpInfo.Rows[i]["DOS"].ToString();
                            sheet1.Range[xlsRow, xlsCol + 12, xlsRow, xlsCol + 14].Merge();

                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.DOB.ToString(), "DOB");
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                            sheet1.Range[xlsRow, xlsCol + 2].Text = ":   " + dtEmpInfo.Rows[i]["DOB"].ToString();
                            sheet1.Range[xlsRow, xlsCol + 2, xlsRow, xlsCol + 4].Merge();

                            sheet1.Range[xlsRow, xlsCol + 5].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.AdharCardNo.ToString(), "Adhar Card No.");
                            sheet1.Range[xlsRow, xlsCol + 5, xlsRow, xlsCol + 6].Merge();
                            sheet1.Range[xlsRow, xlsCol + 7].Text = ":   " + dtEmpInfo.Rows[i]["NationalID"].ToString();
                            sheet1.Range[xlsRow, xlsCol + 7, xlsRow, xlsCol + 9].Merge();

                            sheet1.Range[xlsRow, xlsCol + 10].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Designation.ToString(), "Designation");
                            sheet1.Range[xlsRow, xlsCol + 10, xlsRow, xlsCol + 11].Merge();
                            sheet1.Range[xlsRow, xlsCol + 12].Text = ":   " + dtEmpInfo.Rows[i]["LegalDesignation"].ToString();
                            sheet1.Range[xlsRow, xlsCol + 12, xlsRow, xlsCol + 14].Merge();

                            xlsRow += 1;

                            sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Unit.ToString(), "Unit");
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                            sheet1.Range[xlsRow, xlsCol + 2].Text = ":   " + dtEmpInfo.Rows[i]["UnitName"].ToString();
                            sheet1.Range[xlsRow, xlsCol + 2, xlsRow, xlsCol + 4].Merge();

                            sheet1.Range[xlsRow, xlsCol + 5].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Section.ToString(), "Section");
                            sheet1.Range[xlsRow, xlsCol + 5, xlsRow, xlsCol + 6].Merge();
                            sheet1.Range[xlsRow, xlsCol + 7].Text = ":   " + dtEmpInfo.Rows[i]["SectionName"].ToString();
                            sheet1.Range[xlsRow, xlsCol + 7, xlsRow, xlsCol + 9].Merge();

                            sheet1.Range[xlsRow, xlsCol + 10].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.SubSection.ToString(), "Sub Section");
                            sheet1.Range[xlsRow, xlsCol + 10, xlsRow, xlsCol + 11].Merge();
                            sheet1.Range[xlsRow, xlsCol + 12].Text = ":   " + dtEmpInfo.Rows[i]["SubSectionName"].ToString();
                            sheet1.Range[xlsRow, xlsCol + 12, xlsRow, xlsCol + 14].Merge();

                            #region Dept
                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Department.ToString(), "Department");
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                            sheet1.Range[xlsRow, xlsCol + 2].Text = ":   " + dtEmpInfo.Rows[i]["DepartmentName"].ToString();
                            sheet1.Range[xlsRow, xlsCol + 2, xlsRow, xlsCol + 4].Merge();

                            sheet1.Range[xlsRow, xlsCol + 5].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.BankAccountNo.ToString(), "Bank A/c No");
                            sheet1.Range[xlsRow, xlsCol + 5, xlsRow, xlsCol + 6].Merge();
                            sheet1.Range[xlsRow, xlsCol + 7].Text = ":   " + dtEmpInfo.Rows[i]["BankAccNo"].ToString();
                            sheet1.Range[xlsRow, xlsCol + 7, xlsRow, xlsCol + 9].Merge();

                            sheet1.Range[xlsRow, xlsCol + 10].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.BankName.ToString(), "Bank Name");
                            sheet1.Range[xlsRow, xlsCol + 10, xlsRow, xlsCol + 11].Merge();
                            sheet1.Range[xlsRow, xlsCol + 12].Text = ":   " + dtEmpInfo.Rows[i]["BankName"].ToString();
                            sheet1.Range[xlsRow, xlsCol + 12, xlsRow, xlsCol + 14].Merge();
                            #endregion

                            #region PF
                            string pfESICNo = "";//dicPF[x];

                            if (dicPF.ContainsKey(dtEmpInfo.Rows[i]["EmpSystemId"].ToString().Trim().ToUpper()))
                            {
                                pfESICNo = dicPF[dtEmpInfo.Rows[i]["EmpSystemId"].ToString().Trim().ToUpper()];
                            }

                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.PFUANNo.ToString(), "UAN No.");//"UAN No";
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                            sheet1.Range[xlsRow, xlsCol + 2].Text = ":   " + pfESICNo;
                            sheet1.Range[xlsRow, xlsCol + 2, xlsRow, xlsCol + 4].Merge();
                            pfESICNo = "";
                            if (dicESIC.ContainsKey(dtEmpInfo.Rows[i]["EmpSystemId"].ToString().Trim().ToUpper()))
                            {
                                pfESICNo = dicESIC[dtEmpInfo.Rows[i]["EmpSystemId"].ToString().Trim().ToUpper()];
                            }

                            sheet1.Range[xlsRow, xlsCol + 5].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.ESICNo.ToString(), "ESICNo.");// "ESIC No";
                            sheet1.Range[xlsRow, xlsCol + 5, xlsRow, xlsCol + 6].Merge();
                            sheet1.Range[xlsRow, xlsCol + 7].Text = ":   " + pfESICNo;
                            sheet1.Range[xlsRow, xlsCol + 7, xlsRow, xlsCol + 9].Merge();

                            sheet1.Range[xlsRow, xlsCol + 10].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.EmployeeCategory.ToString(), "Emp Category");//"Emp Category";
                            sheet1.Range[xlsRow, xlsCol + 10, xlsRow, xlsCol + 11].Merge();
                            sheet1.Range[xlsRow, xlsCol + 12].Text = ":   " + dtEmpInfo.Rows[i]["EmpCategoryName"].ToString();
                            sheet1.Range[xlsRow, xlsCol + 12, xlsRow, xlsCol + 14].Merge();
                            #endregion

                            sheet1.Range[empDetailFirstXlsRow, 1, xlsRow, xlsCol + 12].CellStyle.Font.Size = 9;

                            ColGrossHd = xlsCol + 13;

                            #endregion------------------Header------------------

                            #region ------------------Body Part-01----------------------

                            xlsRow += 1;
                            empPaySlipDetailXlsRow = xlsRow;
                            sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.SrNo.ToString(), "SrNo");//"SL.";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 4;

                            sheet1.Range[xlsRow + 2, xlsCol].Text = (1 + SrNo).ToString();
                            sheet1.Range[xlsRow, xlsCol, xlsRow + 2, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            //
                            #region General Info     
                            xlsCol += 1;
                           
                           
                            sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.LeaveInformation.ToString(), "Leave Info");//"Leave Info";
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                           
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;

                            //string userIDFromDictionaryByKey = dicLeaveEmp["EmpLTCode"].ToString();
                            if (dicLeaveEmp.ContainsKey(dtEmpInfo.Rows[i]["EmpSystemID"].ToString()))
                            {
                                //EL = GetLeaveEmp(drLeaveEmp, "PL");
                                //CL = GetLeaveEmp(drLeaveEmp, "CL");
                                //SL = GetLeaveEmp(drLeaveEmp, "SL");
                                //SLESIC = GetLeaveEmp(drLeaveEmp, "SL (ESIC)");
                                //LWP = GetLeaveEmp(drLeaveEmp, "LWP");


                                int k = 1;
                                drLeaveEmp = dicLeaveEmp[dtEmpInfo.Rows[i]["EmpSystemID"].ToString()];
                                foreach (var item in drLeaveEmp)
                                {


                                    sheet1[1, xlsCol].ColumnWidth = 16;
                                    sheet1[1, xlsCol + 1].ColumnWidth = 6;

                                    sheet1.Range[xlsRow + k, xlsCol].Text = ru.GetLabelname(labelList, item["Code"].ToString(), item["Code"].ToString()); //"Casual Leave";
                                    sheet1.Range[xlsRow + k, xlsCol + 1].Number = clsStaticInfo.dbl(item["AvailedLeave"].ToString());
                                    k++;
                                }
                            }
                            //sheet1.Range[xlsRow + 2, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.CasualLeave.ToString(), "CL"); //"Casual Leave";
                            //sheet1.Range[xlsRow + 2, xlsCol].ColumnWidth = 11;

                            //sheet1.Range[xlsRow + 2, xlsCol + 1].Number = Convert.ToDouble(CL);
                            //sheet1.Range[xlsRow + 2, xlsCol + 1].ColumnWidth = 5;

                            //sheet1.Range[xlsRow + 3, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.SickLeave.ToString(), "SL");//"Sick Leave";
                            //sheet1.Range[xlsRow + 3, xlsCol + 1].Number = Convert.ToDouble(SL);

                            //sheet1.Range[xlsRow + 4, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.EarnLeave.ToString(), "EL");//"Earn Leave";

                            //sheet1.Range[xlsRow + 4, xlsCol + 1].Number = Convert.ToDouble(EL);

                            //sheet1.Range[xlsRow + 5, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.LWP.ToString(), "LWP");//"Leave Without Pay";
                            //sheet1.Range[xlsRow + 5, xlsCol + 1].Number = Convert.ToDouble(LWP);

                            //sheet1.Range[xlsRow + 6, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.SickLeaveESIC.ToString(), "SL Esic"); //"Sick Leave Esic";
                            //sheet1.Range[xlsRow + 6, xlsCol + 1].Number = Convert.ToDouble(SLESIC);

                            //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;

                            _Info_Last_Row = xlsRow + 7;

                            #endregion

                            #region Attendance
                            xlsCol += 2;
                            double EarningDays = 0;
                            double DeductingDays = 0;

                            string _pd = "";
                            string _ad = "";
                            string _wod = "";
                            string _hd = "";
                            string _ld = "";
                            double PDay = 0;

                            _pd = dtEmpInfo.Rows[i]["TotalPresent"].ToString();
                            _ad = (clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalLWP"].ToString())).ToString();
                            _wod = dtEmpInfo.Rows[i]["TotalWeekOff"].ToString();
                            _hd = dtEmpInfo.Rows[i]["TotalHoliDay"].ToString();
                            _ld = dtEmpInfo.Rows[i]["TotalLv"].ToString();


                            if (!String.IsNullOrEmpty(dtEmpInfo.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper()))
                            {
                                if (dtEmpInfo.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOffAndHoliday.ToString().ToUpper())
                                {
                                    PDay = clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalHoliDay"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalWeekOff"].ToString());
                                }
                                if (dtEmpInfo.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOff.ToString().ToUpper())
                                {
                                    PDay = clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalWeekOff"].ToString());
                                }
                            }
                            else
                            {
                                PDay = clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalAbsent"].ToString());
                            }


                            sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.WorkDaysDetail.ToString(), "Work Days Detail"); //"Attendance Info";
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();

                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;

                            _x = 1;
                            sheet1[1, xlsCol].ColumnWidth = 14;
                            sheet1[1, xlsCol + 1].ColumnWidth = 6;
                            sheet1.Range[xlsRow + _x, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Present.ToString(), "Present");//"Present";
                            sheet1.Range[xlsRow + _x, xlsCol + 1].Number = Convert.ToDouble(dtEmpInfo.Rows[i]["TotalPresent"].ToString()) + Convert.ToDouble(dtEmpInfo.Rows[i]["TotalLate"].ToString());
                            sheet1.Range[xlsRow + _x, xlsCol + 1].CellStyle.Font.Size = 10;
                            //GetEarningDays(ref EarningDays, _pd);

                            _x++;
                            sheet1.Range[xlsRow + _x, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Absent.ToString(), "Absent"); // "Absent";
                            sheet1.Range[xlsRow + _x, xlsCol + 1].Number = clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalLWP"].ToString());
                            //GetEarningDays(ref DeductingDays, _ad);
                            //GetEarningDays(ref EarningDays, _ad);

                            _x++;
                            sheet1.Range[xlsRow + _x, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.LWP.ToString(), "LWP"); //"LWP";
                            sheet1.Range[xlsRow + _x, xlsCol + 1].Number = clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalLWP"].ToString()) + Convert.ToDouble(SLESIC);

                            _x++;
                            sheet1.Range[xlsRow + _x, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.WorkOff.ToString(), "W.Off"); //"W.Off";
                            sheet1.Range[xlsRow + _x, xlsCol + 1].Number = clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalWeekOff"].ToString());

                            //GetEarningDays(ref EarningDays, _wod);
                            _x++;
                            sheet1.Range[xlsRow + _x, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.HoliDay.ToString(), "Holiday");//"Holiday";
                            sheet1.Range[xlsRow + _x, xlsCol + 1].Number = clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalHoliDay"].ToString());

                            //GetEarningDays(ref EarningDays, _wod);
                            _x++;
                            sheet1.Range[xlsRow + _x, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.LeaveInformation.ToString(), "Leave");//"Leave";
                            sheet1.Range[xlsRow + _x, xlsCol + 1].Number = clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalLv"].ToString());


                            sheet1.Range[xlsRow + _x, xlsCol + 1].CellStyle.Font.Size = 10;
                            //GetEarningDays(ref EarningDays, _ld);

                            _x++;
                            sheet1.Range[xlsRow + _x, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.PayDays.ToString(), "Pay days");
                            sheet1.Range[xlsRow + _x, xlsCol + 1].Number = PDay;

                            _x++;
                            //decimal totalOT = Convert.ToDecimal(dtEmpInfo.Rows[i]["TotalOTHr"].ToString()) / 60;

                            sheet1.Range[xlsRow + _x, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.OTHours.ToString(), "Total OTHr");//"OTHr";
                            if (dtEmpInfo.Rows[i]["TotalOTHr"].ToString() == "0.00")
                            {
                                sheet1.Range[xlsRow + _x, xlsCol + 1].Text = "";
                                //sheet1[xlsRow + _x, xlsCol + 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                            }
                            else
                            {
                                sheet1.Range[xlsRow + _x, xlsCol + 1].Number = clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalOTHr"].ToString()) / 60;
                                sheet1[xlsRow + _x, xlsCol + 1].NumberFormat = clsStaticInfo.NumberFormat(2);
                            }
                            _x++;

                            if (_x > _maxRow)
                                _maxRow = _x;
                            #endregion

                            xlsCol = 6;

                            sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.EarnedSalary.ToString(), "Earned Heads"); //"Earning Heads";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 16;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();


                            sheet1.Range[xlsRow, xlsCol + 2].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Value.ToString(), "Value");// "Act Value";
                            sheet1.Range[xlsRow, xlsCol + 3].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.PayValue.ToString(), "Pay Value");// "Pay Value";

                            xlsCol = 10;

                            sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Deduction.ToString(), "Deduction Heads"); //"Deduction Heads";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 16;

                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                            sheet1.Range[xlsRow, xlsCol + 2].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Value.ToString(), "Value");//"Act Value";

                            sheet1.Range[xlsRow, xlsCol + 3].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.PayValue.ToString(), "Pay Value");

                            xlsCol = 14;
                            sheet1.Range[xlsRow, xlsCol].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.TotalPay.ToString(), "Total Pay"); //"Total Pay";
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();

                            sheet1.Range[xlsRow, 1].RowHeight = sheet1.Range[xlsRow, 1].RowHeight * 2;
                            sheet1.Range[xlsRow, 1, xlsRow, headerEndxlsCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[xlsRow, 1, xlsRow, headerEndxlsCol].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, 1, xlsRow, headerEndxlsCol].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Hair;


                            #endregion ------------------Body Part-01----------------------

                            xlsRow += 1;

                            EmpCounter += 1;
                            SrNo++;
                        }
                        x = dtEmpInfo.Rows[i]["EmpSystemId"].ToString().Trim();



                        int xlsColEarning = 6;
                        double _Total_Earning = 0.00;
                        double _Total_Deduction = 0.00;

                        //  DataTable dtSalaryHead = dtSalaryHeadSheet;
                        // DataView dvStruct = null;// new DataView(dsSlrStructure.Tables[0]);
                        // DataView dvSheet = new DataView(dsSlrStructure.Tables[0]);//Processed Value
                        List<DataRow> drSalaryHeadCollection = null;
                        if (dicEmpSalry.ContainsKey(dtEmpInfo.Rows[i]["EmpSystemID"].ToString()))
                        {
                            drSalaryHeadCollection = dicEmpSalry[dtEmpInfo.Rows[i]["EmpSystemID"].ToString()];
                        }

                        // dvStruct.RowFilter = "SystemID='" + x + "'";//SystemId = EmployeeSystemId
                        //dvSheet.RowFilter = "EmpInfoSystemID='" + x + "'";//SystemId = EmployeeSystemId


                        int _startRow = xlsRow;
                        LoadSalaryHead_CurrLess(ref sheet1, dtSalaryHeadSheet, xlsRow, xlsColEarning, out int _tempMaxRow, out _Total_Earning, "E", localLanguage, drSalaryHeadCollection);
                        if (_tempMaxRow > _maxRow)
                            _maxRow = _tempMaxRow;

                        int xlsColDeduc = 10;
                        int _maxRowDeduct = 0;
                        LoadSalaryHead_CurrLess(ref sheet1, dtSalaryHeadSheet, xlsRow, xlsColDeduc, out _maxRowDeduct, out _Total_Deduction, "D", localLanguage, drSalaryHeadCollection);



                        var result = drSalaryHeadCollection.Where(row => row["HeadCategory"].Equals("Basic")).FirstOrDefault();

                        if (result != null)
                        {

                            _basic = clsStaticInfo.dbl(result["DisbusmentAmount"].ToString());


                        }
                        //if (dvBasic.Count > 0)
                        //{
                        //    _basic = Convert.ToDouble(dvBasic[0]["DisbusmentAmount"].ToString());
                        //}

                        if (_maxRowDeduct > _maxRow)
                        {
                            _maxRow = _maxRowDeduct;
                        }
                        else
                        {

                        }

                        //DataView dvNetPay = new DataView(dtSalaryHead);
                        //dvNetPay.RowFilter = "HeadCategory='Net Payable'";

                        result = drSalaryHeadCollection.Where(row => row["HeadCategory"].Equals("Net Payable")).FirstOrDefault();
                        if (result != null)
                        {
                            _netPay = clsStaticInfo.dbl(result["DisbusmentAmount"].ToString());
                        }

                        _maxRow++;
                        _maxRow++;

                        sheet1.Range[_maxRow + 1, xlsColDeduc + 4].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.NetPayable.ToString(), "Net Pay");//"Net Disb. :";

                        sheet1.Range[_maxRow + 1, xlsColEarning + 1].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.TotalEarning.ToString(), "Total Earning");//"Total Earning";
                        sheet1.Range[_maxRow + 1, xlsColEarning, _maxRow + 1, xlsColEarning + 1].Merge();


                        var actualSalaryFormula = "=SUM(" + ru.GetColumnNameForXls(xlsColEarning + 2) + _startRow + ":" + ru.GetColumnNameForXls(xlsColEarning + 2) + _maxRow + ")";
                        sheet1.Range[_maxRow + 1, xlsColEarning + 2].Formula = actualSalaryFormula;// + Convert.ToInt32(ColGrsSlrDif);
                        sheet1.Range[_maxRow + 1, xlsColEarning + 2].NumberFormat = ru.NumberFormatIntLocal(localLanguage);

                        var earnedSalaryFormula = "=SUM(" + ru.GetColumnNameForXls(xlsColEarning + 3) + _startRow + ":" + ru.GetColumnNameForXls(xlsColEarning + 3) + _maxRow + ")";
                        sheet1.Range[_maxRow + 1, xlsColEarning + 3].Formula = earnedSalaryFormula;
                        sheet1.Range[_maxRow + 1, xlsColEarning + 3].NumberFormat = ru.NumberFormatIntLocal(localLanguage);

                        sheet1.Range[_maxRow + 1, xlsColDeduc].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.TotalDeduction.ToString(), "Total Deduction");//"Total Deduction";

                        sheet1.Range[_maxRow + 1, xlsColDeduc, _maxRow + 1, xlsColDeduc + 1].Merge();


                        var actualSalaryFormulaDeduction = "=SUM(" + ru.GetColumnNameForXls(xlsColDeduc + 2) + _startRow + ":" + ru.GetColumnNameForXls(xlsColDeduc + 2) + _maxRow + ")";
                        sheet1.Range[_maxRow + 1, xlsColDeduc + 2].Formula = actualSalaryFormulaDeduction;
                        sheet1.Range[_maxRow + 1, xlsColDeduc + 2].NumberFormat = ru.NumberFormatIntLocal(localLanguage);

                        var earnedSalaryFormulaDeduction = "=SUM(" + ru.GetColumnNameForXls(xlsColDeduc + 3) + _startRow + ":" + ru.GetColumnNameForXls(xlsColDeduc + 3) + _maxRow + ")";
                        sheet1.Range[_maxRow + 1, xlsColDeduc + 3].Formula = earnedSalaryFormulaDeduction;
                        sheet1.Range[_maxRow + 1, xlsColDeduc + 3].NumberFormat = ru.NumberFormatIntLocal(localLanguage);
                        int xlsColTot = 14;

                        #region Total Earning
                        sheet1.Range[xlsRow, xlsColTot].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.TotalEarning.ToString(), "Total Earning");
                        //sheet1.Range[xlsRow, xlsColTot].Text = "Total Earning";
                        var xlstColTotVal = xlsColTot + 1;
                        sheet1.Range[xlsRow, xlstColTotVal].Number = (double)_Total_Earning;


                        sheet1.Range[xlsRow, xlsColTot].ColumnWidth = 12.43;

                        sheet1.Range[xlsRow, xlstColTotVal].CellStyle.Font.Size = 10;
                        #endregion Total Earning

                        #region Total Deduction
                        sheet1.Range[xlsRow + 1, xlsColTot].Text = ru.GetLabelname(labelList, LabelNameInLocalLanguage.Deduction.ToString(), "Total Deduction");  //"Total Deduction";
                        double totDeduction = 0.00;
                        if ((double)_Total_Deduction > 0)
                        {
                            totDeduction = (double)_Total_Deduction;
                        }
                        else
                        {
                            totDeduction = (double)_Total_Deduction * (-1);
                        }
                        sheet1.Range[xlsRow + 1, xlsColTot + 1].Number = totDeduction;
                        sheet1.Range[xlsRow + 1, xlsColTot + 1].NumberFormat = ru.NumberFormatIntLocal(localLanguage);

                        #endregion Total Deduction




                        #region Net Exchange Gain

                        #endregion Net Exchange Gain

                        #region Net Disburseable

                        sheet1.Range[_maxRow + 1, xlsColTot + 1].Number = (double)(_netPay);
                        sheet1.Range[_maxRow + 1, xlsColTot + 1].NumberFormat = ru.NumberFormatIntLocal(localLanguage);

                        #endregion Net Disburseable

                        #region Line Setup
                        //sr
                        sheet1.Range[xlsRow, 1, _maxRow, 1].Merge();

                        //info

                        //ATTENDANCE


                        sheet1.Range[xlsRow - 2, 1, _maxRow, xlsColTot + 1].WrapText = true;
                        #endregion


                        //_maxRow++;
                        startRow = _maxRow + 4;


                        if ((EmpCounter % 3) == 0)
                        {
                            sheet1.HPageBreaks.Add(sheet1[(_maxRow + 4), xlsColTot + 1]);
                        }
                        //else
                        //{
                        //    startRow = _maxRow + 5;
                        //}
                        sheet1.Range[_maxRow + 2, 1].Text = "— — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — — ";
                        sheet1.Range[_maxRow + 2, 1, _maxRow + 2, xlsColTot + 1].Merge();

                        sheet1.Range[empPaySlipDetailXlsRow, 1, _maxRow + 1, headerEndxlsCol].CellStyle.Font.Size = 10;
                        sheet1.Range[_maxRow + 1, 6, _maxRow + 1, headerEndxlsCol].CellStyle.Font.Bold = true;
                        sheet1.Range[_maxRow + 1, 6, _maxRow + 1, headerEndxlsCol].NumberFormat = ru.NumberFormatIntLocal(localLanguage);


                        //ReportUtility 
                        sheet1.Range[empPaySlipDetailXlsRow, 1, _maxRow + 1, headerEndxlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[empPaySlipDetailXlsRow, 1, _maxRow + 1, headerEndxlsCol].CellStyle.Font.FontName = printFont;

                        sheet1.Range[empPaySlipDetailXlsRow, 1, _maxRow + 1, 15].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Hair;
                        sheet1.Range[_maxRow + 1, 1, _maxRow + 1, 15].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Hair;
                        xlsRow = startRow;
                    }

                }
                #region Freeze Panes
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                //sheet1.UsedRange.CellStyle.Font.FontName = printFont;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                //sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                sheet1.Name = "EmpSalaryPaySlip";
                #endregion


                string strFileName = DateTime.Now.ToString("ddMMyy") + " SalaryPaySlip";

                workbook.Version = ExcelVersion.Excel2016;
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
            }
        }



        #endregion


        private void FormatText(ref IWorksheet sheet1, ref IRichTextString rtf, string NewText, double FontSize)
        {
            IFont font = sheet1.Workbook.CreateFont();
            font.Color = ExcelKnownColors.Black;
            font.Size = FontSize;

            int oldPos = 0;
            if (rtf.Text.Length > 0)
                oldPos = rtf.Text.Length - 1;

            rtf.Append(NewText, font);
            rtf.SetFont(oldPos, (oldPos + NewText.Length) - 1, font);
        }
        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 4;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 10;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 7;
            ColIndex = xlsCol;
            xlsCol += 1;
        }
        void GetEarningDays(ref double EarningDays, string pvalue)
        {
            try
            {
                var vl = (string.IsNullOrEmpty(pvalue) ? "0" : pvalue);
                EarningDays += Convert.ToDouble(vl);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 10;

            ColIndex = xlsCol;
            xlsCol += 1;
        }
        private void SetCellValueBangla(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width, string printFont, int rotationDegree)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = printFont;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Rotation = rotationDegree;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 24;

            ColIndex = xlsCol;
            xlsCol += 1;
        }

        private void SetCellValueRotate(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 24;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Rotation = 90;
            ColIndex = xlsCol;
            xlsCol += 1;
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
        private void CreateDynamicSHead(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out Dictionary<string, SalaryHeadSequence> list)
        {
            try
            {
                list = new Dictionary<string, SalaryHeadSequence>();
                _total_head_count = 0;
                _count_earning_head = 0;
                _count_deducting_head = 0;
                _count_earning_ctchead = 0;
                int countGrossPostion = 0;
                string deductionFormula = "";

                xlsCol += 1;
                countGrossPostion++;

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
                            salaryHeadSequence.IsNetPayEffect = Convert.ToBoolean(dtSalaryHead.Rows[ci]["PartOfNetPay"]);
                            salaryHeadSequence.IsGrossComponent = Convert.ToBoolean(dtSalaryHead.Rows[ci]["IsGrossComponent"]);
                            salaryHeadSequence.IsCTCComponent = Convert.ToBoolean(dtSalaryHead.Rows[ci]["IsCTCComponent"]);

                            salaryHeadSequence.Sequence = ci;
                            salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;

                            list.Add(dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString(), salaryHeadSequence);
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


                            list.Add(dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString(), salaryHeadSequence);

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

        private double GetLeaveEmp(List<DataRow> drLeave, string LeaveCode)
        {
            double leaveValue = 0.00;
            try
            {
                if (drLeave != null)
                {
                    var leave = drLeave.Where(row => row["code"].Equals(LeaveCode)).FirstOrDefault();

                    if (leave != null)
                    {
                        leaveValue = clsStaticInfo.dbl(leave["AvailedLeave"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return leaveValue;
        }

        private string GetLeaveType(DataView dvLeaveType, string leaveCode)
        {
            var localLeaveType = string.Empty;
            try
            {

                var basic = from r in dvLeaveType.ToTable().AsEnumerable()
                            where r.Field<string>("code") == leaveCode
                            select r;
                if (basic.Count() > 0)
                {
                    DataTable dtt = basic.CopyToDataTable();
                    localLeaveType = dtt.Rows[0]["lName"].ToString();
                }
                return localLeaveType;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private decimal GetLWPEmp(DataView dvEmpLeaveInfo, string LeaveType)
        {
            var basicValue = 0.00m;
            try
            {

                var basic = from r in dvEmpLeaveInfo.ToTable().AsEnumerable()
                            where r.Field<string>("LeaveType") == LeaveType
                            select r;
                if (basic.Count() > 0)
                {

                    DataTable dtt = basic.CopyToDataTable();
                    basicValue = Convert.ToDecimal(dtt.Rows[0]["AvailedLeave"].ToString());


                }
                return basicValue;
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

        private string GetDecimalFormat(SalaryHeadSequence shs)
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

        private void LoadSalaryHead_CurrLess(ref IWorksheet sheet1, DataTable dt, int xlsRow, int xlsColEarning, out int maxRow, out double TotalED, string EorD, string localLanguage, List<DataRow> drSalaryHeadCollection)
        {

            string NumberFormatString = "#,##0;(#,##0)";
            ReportUtility ru = null;
            var formulaStartRow = 0;
            double ColGrsSlr = 0.00;
            var ColGrsSlrDif = 0.0m;
            TotalED = 0;
            try
            {
                ru = new ReportUtility();

                formulaStartRow = xlsRow;
                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    bool _IsGrossComponent = bplib.clsWebLib.GetBoolData(dt.Rows[i]["IsGrossComponent"].ToString());
                    bool _IsCTCComponent = bplib.clsWebLib.GetBoolData(dt.Rows[i]["IsCTCComponent"].ToString());
                    bool _IsNetPayEffect = bplib.clsWebLib.GetBoolData(dt.Rows[i]["PartOfNetPay"].ToString());
                    double structureValue = 0.00;
                    if (dt.Rows[i]["HeadType"].ToString().ToUpper() == "E".ToUpper() && Convert.ToInt32(dt.Rows[i]["PartOfNetPay"]) == 1 && EorD == "E")
                    {
                        ColGrsSlr = 0; ColGrsSlrDif = 0;
                        sheet1.Range[xlsRow, xlsColEarning].Text = dt.Rows[i]["SalaryHeadLocal"].ToString();
                        sheet1.Range[xlsRow, xlsColEarning, xlsRow, xlsColEarning + 1].Merge();
                        //get str value
                        structureValue = 0.00;
                        var processedValue = string.Empty;
                        bool isDecimal = true;
                        var decimalNo = 0;

                        //var _data = listdsSlrStruct.Where(r => r.SalaryHeadID == dt.Rows[i]["SalaryHeadId"].ToString()).FirstOrDefault();

                        var result = drSalaryHeadCollection.Where(row => row["SalaryHeadId"].Equals(dt.Rows[i]["SalaryHeadId"].ToString())).FirstOrDefault();

                        if (result != null)
                        {
                            structureValue = clsStaticInfo.dbl(result["EntryAmount"].ToString());
                            ColGrsSlr = clsStaticInfo.dbl(result["DisbusmentAmount"].ToString());

                            isDecimal = Convert.ToBoolean(result["IntegerInDisb"].ToString());
                            decimalNo = (int)clsStaticInfo.dbl(result["DecimalNo"].ToString());
                        }
                        else
                        {
                            ColGrsSlr = 0.00;
                        }


                        sheet1.Range[xlsRow, xlsColEarning + 2].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(structureValue.ToString()));// + Convert.ToInt32(ColGrsSlrDif);
                        sheet1.Range[xlsRow, xlsColEarning + 2].NumberFormat = ru.NumberFormatIntLocal(localLanguage);



                        TotalED += ColGrsSlr;

                        if (ColGrsSlr < 0)
                        {
                            ColGrsSlr = ColGrsSlr * (-1);
                        }
                        sheet1.Range[xlsRow, xlsColEarning + 3].Number = Convert.ToInt32(ColGrsSlr);
                        sheet1.Range[xlsRow, xlsColEarning + 3].NumberFormat = ru.NumberFormatIntLocal(localLanguage);

                        xlsRow++;
                    }//HeadType E D 

                    if (dt.Rows[i]["HeadType"].ToString().ToUpper() == "D".ToUpper() && EorD == "D")
                    {
                        ColGrsSlr = 0; ColGrsSlrDif = 0;
                        sheet1.Range[xlsRow, xlsColEarning].Text = dt.Rows[i]["SalaryHeadLocal"].ToString();
                        sheet1.Range[xlsRow, xlsColEarning, xlsRow, xlsColEarning + 1].Merge();
                        //get str value
                        structureValue = 0.00;
                        var processedValue = string.Empty;
                        bool isDecimal = true;
                        var decimalNo = 0;

                        var result = drSalaryHeadCollection.Where(row => row["SalaryHeadId"].Equals(dt.Rows[i]["SalaryHeadId"].ToString())).FirstOrDefault();

                        if (result != null)
                        {
                            structureValue = clsStaticInfo.dbl(result["EntryAmount"].ToString());
                            ColGrsSlr = clsStaticInfo.dbl(result["DisbusmentAmount"].ToString());

                            isDecimal = Convert.ToBoolean(result["IntegerInDisb"].ToString());
                            decimalNo = (int)clsStaticInfo.dbl(result["DecimalNo"].ToString());
                        }
                        else
                        {
                            ColGrsSlr = 0.00;
                            structureValue = 0.00;
                        }
                        if (structureValue < 0)
                        {
                            structureValue = structureValue * (-1);
                        }
                        sheet1.Range[xlsRow, xlsColEarning + 2].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(structureValue.ToString()));// + Convert.ToInt32(ColGrsSlrDif);
                        sheet1.Range[xlsRow, xlsColEarning + 2].NumberFormat = ru.NumberFormatIntLocal(localLanguage);


                        TotalED += ColGrsSlr;

                        if (ColGrsSlr < 0)
                        {
                            ColGrsSlr = ColGrsSlr * (-1);
                        }
                        sheet1.Range[xlsRow, xlsColEarning + 3].Number = Convert.ToInt32(ColGrsSlr);
                        sheet1.Range[xlsRow, xlsColEarning + 3].NumberFormat = ru.NumberFormatIntLocal(localLanguage);

                        xlsRow++;
                    }//HeadType E D                      
                }//for

                maxRow = xlsRow;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public class PayRollReportParamList
        {

            public string EmployeeId { get; set; }
            public string payGroup { get; set; }
            public string userId { get; set; }
            public string Month { get; set; }
            public string Year { get; set; }
            public string PlantId { get; set; }
            public string UnitId { get; set; }
            public string DivisionId { get; set; }
            public string DepartmentId { get; set; }
            public string SectionId { get; set; }
            public string SubSectionId { get; set; }
            public string LineId { get; set; }
            public string SubSecStrucId { get; set; }
            public string EmpCategoryId { get; set; }
            public string DesignationGroupId { get; set; }
            public string DesignationId { get; set; }
            public string FromDate { get; set; }
            public string EmpStatus { get; set; }
            public string SalaryProcessId { get; set; }
            public string CompanyGroupId { get; set; }
            public string CompanyId { get; set; }
            public string ToDate { get; set; }
            public string PayGroup { get; set; }
            public string SystemID { get; set; }
            public string PaymentMode { get; set; }
            public string LanguageId { get; set; }
            public string SystemAdmin { get; set; }
            public string ControlAdmin { get; set; }
            public string MinWageEffectiveDate { get; set; }
            public string UserId { get; set; }
            public System.Web.HttpResponse Response { get; set; }
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
        public IEnumerable<ComboModel> GetSalaryprocessIdCbo(string compnayGroupId, string companyId, string plantId, string MonthNo, string YearNo, string IsCompleteMonth)
        {
            var plant = string.Empty;
            var strSQL = string.Empty;
            if (plantId == null)
            {
                plant = "";
                strSQL = @"SELECT * FROM SalaryProcMaster
                                    WHERE MonthNo = '" + MonthNo + @"' AND YearNo = '" + YearNo + @"' and SystemID IN (select SlrProcMstSystemID  from SalaryProcChild ) --AND IsCompleteMonth = " + IsCompleteMonth + @"
                            --GROUP BY SalaryProcID";
            }
            else
            {

                strSQL = @"SELECT * FROM SalaryProcMaster
                                      WHERE SystemID IN (SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = '" + MonthNo + @"' AND YearNo = '" + YearNo + @"' --AND IsCompleteMonth = " + IsCompleteMonth + @"
                                      --GROUP BY SalaryProcID";
            }

            return _sqlRepository.GetCombo(strSQL, "SystemId", "Description");
        }

        public static int closestNumber(int n, int m)
        {
            // find the quotient 
            int q = n / m;

            // 1st possible closest number 
            int n1 = m * q;

            // 2nd possible closest number 
            int n2 = (n * m) > 0 ? (m * (q + 1)) : (m * (q - 1));

            // if true, then n1 is the required closest number 
            if (Math.Abs(n - n1) < Math.Abs(n - n2))
                return n1;

            // else n2 is the required closest number 
            return n2;
        }

        public IEnumerable<ComboModel> GetPayRollGroupCbo(bool sa, bool ca, string plantId, string userId)
        {

            try
            {
                var plant = string.Empty;
                var strSQL = string.Empty;
                if (ca == true || sa == true)
                {
                    strSQL = @"	SELECT Distinct ISNULL(HPG.Id,'NG') Id, ISNULL(HPG.UserName,'No Group') UserName ,hpg.Sequence
                            FROM  EmployeeInformation EEI 
							Left Join MST.PayrollGroupMaster PGM ON PGM.EmployeeId = EEI.SystemId
							LEFT JOIN  HKP.PayrollGroup HPG ON PGM.PayrollGroupId = HPG.Id 	
							where EEI.PlantId = '" + plantId + @"'					 

                            ORDER BY sequence";
                }
                else
                {
                    strSQL = @"SELECT Distinct ISNULL(HPG.Id,'NG') Id, ISNULL(HPG.UserName,'No Group') UserName ,hpg.Sequence
                            FROM  EmployeeInformation EEI 
							Left Join MST.PayrollGroupMaster PGM ON PGM.EmployeeId = EEI.SystemId
							LEFT JOIN  HKP.PayrollGroup HPG ON PGM.PayrollGroupId = HPG.Id 	
							where EEI.PlantId = '" + plantId + @"' and  HPG.Id IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup WHERE UserId = '" + userId + @"') ORDER BY HPG.Sequence";

                }





                return _sqlRepository.GetCombo(strSQL, "Id", "UserName");
            }
            catch (Exception)
            {

                throw;
            }


        }

        public IEnumerable<object> GetEmpInfo(string companyGroupId, string plantId, string effectiveDate, string salaryProcessId, bool sa, bool ca, string userId, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                var wcPayrollGroup = "";
                var wcSalaryProcess = "";
                var salaryProcessJoin = "";
                var salaryProcessColumn = "";
                var strDOJ = "";
                string salaryProcessFlag = "";
                string wcEmpStatus = " Where (1=0 ";

                if (sa == true || ca == true)
                {
                    wcPayrollGroup = @"";
                }
                else
                {
                    string inPayrollGroup = "' '";
                    DataTable dtPayRollGrpEmpId = _sqlRepository.GetDataTable("SELECT employeeid FROM MST.PayrollGroupMaster WHERE PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"') AND PlantID = '" + plantId + @"'");
                    DataTable dtNotPayRollGrpEmpId = _sqlRepository.GetDataTable(@"SELECT SystemId FROM EmployeeInformation E 
                    WHERE SystemId NOT IN (SELECT employeeid from MST.PayrollGroupMaster where PlantID = '" + plantId + @"')  AND E.PlantID = '" + plantId + @"'");

                    if (dtPayRollGrpEmpId.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtPayRollGrpEmpId.Rows.Count; i++)
                        {
                            inPayrollGroup += ",'" + dtPayRollGrpEmpId.Rows[i]["employeeid"].ToString() + "'";
                        }
                        if (dtNotPayRollGrpEmpId.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtNotPayRollGrpEmpId.Rows.Count; i++)
                            {
                                inPayrollGroup += ",'" + dtNotPayRollGrpEmpId.Rows[i]["SystemId"].ToString() + "'";
                            }
                        }
                        wcPayrollGroup = @"AND E.SystemId  IN (" + inPayrollGroup + @")";
                    }
                    else
                    {
                        wcPayrollGroup = @"";
                    }
                    //wcPayrollGroup = @"AND E.SystemId  IN (SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"'))";
                }
                if (salaryProcessId == "STRUCTURE")
                {
                    salaryProcessColumn = "";
                    salaryProcessJoin = "";
                    wcSalaryProcess = "";
                    strDOJ = "AND DOJ<='" + effectiveDate + @"' AND (DOS is null OR DOS>= '" + effectiveDate + @"')";


                }
                else if (!string.IsNullOrEmpty(salaryProcessId))
                {
                    salaryProcessColumn = ",ISNULL(SPM.Description,'') SalaryProcess";
                    salaryProcessJoin = @"  LEFT OUTER JOIN SalaryProcChild SPC ON SPC.EmpInfoSystemID = E.SystemId
                                    LEFT OUTER JOIN SalaryProcMaster SPM ON SPM.SystemID = spc.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')";
                    wcSalaryProcess = @"AND SPC.SlrProcMstSystemID IN('" + salaryProcessId + @"')";

                }
                else if (string.IsNullOrEmpty(salaryProcessId) == true && salaryProcessId != "STRUCTURE")
                {
                    salaryProcessColumn = ",ISNULL(SPM.Description,'') SalaryProcess";
                    salaryProcessJoin = @"  LEFT OUTER JOIN SalaryProcChild SPC ON SPC.EmpInfoSystemID = E.SystemId
                                    LEFT OUTER JOIN SalaryProcMaster SPM ON SPM.SystemID = spc.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')";

                    wcSalaryProcess = @" AND SPC.SlrProcMstSystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo =  MONTH('" + effectiveDate + @"') AND YearNo =  YEAR('" + effectiveDate + @"')  )";
                }
                if (salaryProcessId == "STRUCTURE")
                {
                    wcEmpStatus = " Where (1=1 ";
                    salaryProcessFlag = "";
                }
                else
                {
                    salaryProcessFlag = ", Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag";
                    wcEmpStatus = " Where (1=0 ";

                    if (isActive == true && isSeperated == true && isMaternity == true)
                    {
                        wcEmpStatus = " Where (1=1 ";
                    }
                    else
                    {
                        if (isActive == true)
                        {
                            wcEmpStatus += " OR SalaryProcFlag ='Regular'";
                        }
                        if (isSeperated == true)
                        {
                            wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                        }
                        if (isMaternity == true)
                        {
                            wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                        }
                    }
                }

                wcEmpStatus += ")";

                var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
                var param = string.Empty;
                if (!string.IsNullOrEmpty(companyGroupId) && !string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "' AND E.PlantId='" + plantId + "'";
                else if (!string.IsNullOrEmpty(companyGroupId) && string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "'";

                var cmdText = @"SELECT [isSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'False'),* FROM (  SELECT   dISTINCT   
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
                                    , CASE WHEN MONTH(DOS) =  MONTH('" + effectiveDate + @"')  AND YEAR(DOS) = YEAR('" + effectiveDate + @"') then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    " + salaryProcessFlag + @"
                                    " + salaryProcessColumn + @"
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName

                                    FROM EmployeeInformation e
                                
                                   -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=e.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
                                    LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=e.LegalDesignationId

                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									FROM mst.DesignationMaster dm
									LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
		                           -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=dm.DesignationGroupId
                                   -- left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									AND egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
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
                                     ) DD " + wcEmpStatus + @" ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public IEnumerable<object> GetEmpInfoSalaryPorcessed(string companyGroupId, string plantId, string effectiveDate, string salaryProcessId, bool sa, bool ca, string userId, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                var wcPayrollGroup = "";
                var wcSalaryProcess = "";
                var salaryProcessJoin = "";
                var salaryProcessColumn = "";
                var strDOJ = "";
                string salaryProcessFlag = "";
                string wcEmpStatus = " Where (1=0 ";
                //string salaryProcessID = "";

                if (sa == true || ca == true)
                {
                    wcPayrollGroup = @"";
                }
                else
                {
                    string inPayrollGroup = "";
                    DataTable dtPayRollGrpEmpId = _sqlRepository.GetDataTable("SELECT employeeid FROM MST.PayrollGroupMaster WHERE PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"') AND PlantID = '" + plantId + @"'");
                    DataTable dtNotPayRollGrpEmpId = _sqlRepository.GetDataTable(@"SELECT SystemId FROM EmployeeInformation E 
                    WHERE SystemId NOT IN (SELECT employeeid from MST.PayrollGroupMaster where PlantID = '" + plantId + @"')  AND E.PlantID = '" + plantId + @"'");

                    if (dtPayRollGrpEmpId.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtPayRollGrpEmpId.Rows.Count; i++)
                        {
                            inPayrollGroup += ",'" + dtPayRollGrpEmpId.Rows[i]["employeeid"].ToString() + "'";
                        }
                        if (dtNotPayRollGrpEmpId.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtNotPayRollGrpEmpId.Rows.Count; i++)
                            {
                                inPayrollGroup += ",'" + dtNotPayRollGrpEmpId.Rows[i]["SystemId"].ToString() + "'";
                            }
                        }
                        wcPayrollGroup = @"AND E.SystemId  IN (" + inPayrollGroup + @")";
                    }
                    else
                    {
                        wcPayrollGroup = @"";
                    }

                    //wcPayrollGroup = @"AND E.SystemId  IN (SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"'))";
                }
                if (salaryProcessId == "STRUCTURE")
                {
                    salaryProcessColumn = "";
                    salaryProcessJoin = "";
                    wcSalaryProcess = "";
                    strDOJ = "AND DOJ<='" + effectiveDate + @"' AND (DOS is null OR DOS>= '" + effectiveDate + @"')";


                }
                else if (!string.IsNullOrEmpty(salaryProcessId))
                {
                    salaryProcessColumn = ",ISNULL(SPM.Description,'') SalaryProcess";
                    salaryProcessJoin = @"  LEFT OUTER JOIN SalaryProcChild SPC ON SPC.EmpInfoSystemID = E.SystemId
                                    LEFT OUTER JOIN SalaryProcMaster SPM ON SPM.SystemID = spc.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')";
                    wcSalaryProcess = @"AND SPC.SlrProcMstSystemID IN('" + salaryProcessId + @"')";

                }
                else if (string.IsNullOrEmpty(salaryProcessId) == true && salaryProcessId != "STRUCTURE")
                {
                    salaryProcessColumn = ",ISNULL(SPM.Description,'') SalaryProcess";
                    salaryProcessJoin = @"  LEFT OUTER JOIN SalaryProcChild SPC ON SPC.EmpInfoSystemID = E.SystemId
                                    LEFT OUTER JOIN SalaryProcMaster SPM ON SPM.SystemID = spc.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')";

                    string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo =  MONTH('" + effectiveDate + @"') AND YearNo =  YEAR('" + effectiveDate + @"')";

                    DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);
                    salaryProcessId = "''";
                    for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
                    {
                        salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
                    }
                    wcSalaryProcess = @" AND SPC.SlrProcMstSystemID IN( " + salaryProcessId + @"  )";
                }
                if (salaryProcessId == "STRUCTURE")
                {
                    wcEmpStatus = " Where (1=1 ";
                    salaryProcessFlag = "";
                }
                else
                {
                    salaryProcessFlag = ", Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag";
                    wcEmpStatus = " Where (1=0 ";

                    if (isActive == true && isSeperated == true && isMaternity == true)
                    {
                        wcEmpStatus = " Where (1=1 ";
                    }
                    else
                    {
                        if (isActive == true)
                        {
                            wcEmpStatus += " OR SalaryProcFlag ='Regular'";
                        }
                        if (isSeperated == true)
                        {
                            wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                        }
                        if (isMaternity == true)
                        {
                            wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                        }
                    }
                }





                wcEmpStatus += ")";

                var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
                var param = string.Empty;
                if (!string.IsNullOrEmpty(companyGroupId) && !string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "' AND E.PlantId='" + plantId + "'";
                else if (!string.IsNullOrEmpty(companyGroupId) && string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "'";

                var cmdText = @"SELECT [isSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'False'),* FROM (  SELECT   dISTINCT   
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId                                     
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId                                     
                                    ,isnull(ld.UserName,'') Designation                                       
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
                                    , CASE WHEN MONTH(DOS) =  MONTH('" + effectiveDate + @"')  AND YEAR(DOS) = YEAR('" + effectiveDate + @"') then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    " + salaryProcessFlag + @"
                                    " + salaryProcessColumn + @"
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(SPLD.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName

                                     FROM EmployeeInformation e
                                
                                    JOIN (
                                     SELECT DISTINCT EmpInfoSystemID,SlrProcMstSystemID,PlantID ,m.Description,m.SalaryProcFlag
                                    FROM SalaryProcChild c
                                   INNER JOIN SalaryProcMaster m on M.MonthNo= MONTH('" + effectiveDate + @"') AND M.YearNo=YEAR('" + effectiveDate + @"') AND M.SystemID=C.SlrProcMstSystemID
                                   
                                    ) SPM ON spm.EmpInfoSystemID=e.SystemId and " + param + @"
									  JOIN SalaryProcessLogDetail SPLD ON 
								
									  SPLD.SalaryProcessId=SPM.SlrProcMstSystemID
									 AND SPM.EmpInfoSystemID = SPLD.EmpSystemId 

                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=SPLD.LegalDesignationId
                                   
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=SPLD.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    
                                    LEFT OUTER JOIN ORG.Line eL on eL.id=mpb.LineId

                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = SPLD.EmployeeCategoryId
			                                       
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
									Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    
								    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
									left join [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									left join [HKP].[Bank] bb on bb.Id = SPLD.BankSystemID
									left join [HKP].[BankBranch] bbranch on bbranch.Id = SPLD.BankBranchId
   
                                     WHERE 1=1 " + strDOJ + @"
                                            " + wcPayrollGroup + @"                                
                                     ) DD " + wcEmpStatus + @" ORDER BY ISNULL(EmployeeCodePreFix,''),ISNULL(EmployeeCodeNumeric,0)";
                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetSeparatedEmpInfo(string companyGroupId, string plantId, string effectiveDate, string FromDate, string ToDate, string salaryProcessId, bool sa, bool ca, string userId)
        {
            try
            {
                var wcPayrollGroup = "";
                var wcSalaryProcess = "";
                var salaryProcessJoin = "";
                var salaryProcessColumn = "";
                var strDOJ = "";
                //payRollGroup = "'" + payRollGroup.Replace(",", "','") + "'";
                if (sa == true || ca == true)
                {
                    wcPayrollGroup = @"";
                }
                else
                {
                    wcPayrollGroup = @"AND E.SystemId  IN (SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"'))";
                }
                if (salaryProcessId == "STRUCTURE")
                {
                    salaryProcessColumn = "";
                    salaryProcessJoin = "";
                    wcSalaryProcess = "";
                    strDOJ = "AND (DOS is not null and  DOS Between '" + FromDate + @"' and '" + ToDate + @"')";

                }
                else if (!string.IsNullOrEmpty(salaryProcessId))
                {
                    salaryProcessColumn = ",ISNULL(SPM.Description,'') SalaryProcess";
                    salaryProcessJoin = @"  LEFT OUTER JOIN SalaryProcChild SPC ON SPC.EmpInfoSystemID = E.SystemId
                                    LEFT OUTER JOIN SalaryProcMaster SPM ON SPM.SystemID = spc.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')";
                    wcSalaryProcess = @"AND SPC.SlrProcMstSystemID IN('" + salaryProcessId + @"')";

                }
                else if (string.IsNullOrEmpty(salaryProcessId) == true && salaryProcessId != "STRUCTURE")
                {
                    salaryProcessColumn = ",ISNULL(SPM.Description,'') SalaryProcess";
                    salaryProcessJoin = @"  LEFT OUTER JOIN SalaryProcChild SPC ON SPC.EmpInfoSystemID = E.SystemId
                                    LEFT OUTER JOIN SalaryProcMaster SPM ON SPM.SystemID = spc.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')";

                    wcSalaryProcess = @" AND SPC.SlrProcMstSystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo =  MONTH('" + effectiveDate + @"') AND YearNo =  YEAR('" + effectiveDate + @"')  )";
                }

                var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
                var param = string.Empty;
                if (!string.IsNullOrEmpty(companyGroupId) && !string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "' AND E.PlantId='" + plantId + "'";
                else if (!string.IsNullOrEmpty(companyGroupId) && string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "'";

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
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    " + salaryProcessColumn + @"
									,ISNULL(PG.UserName,'') PayRollGroup

                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName

                                    FROM EmployeeInformation e
                                    LEFT OUTER JOIN ORG.Department edept on edept.id=e.DepartmentId
                                    LEFT OUTER JOIN ORG.Line eL on eL.id=e.LineId
                                    LEFT OUTER JOIN ORG.Division ediv on ediv.id=e.DivisionId
                                    LEFT OUTER JOIN ORG.SubDivision esdiv on esdiv.id=e.SubDivisionId
                                    LEFT OUTER JOIN ORG.Section es on es.id=e.SectionId
                                    LEFT OUTER JOIN ORG.SubSection ess on ess.id=e.SubSectionId
                                    LEFT OUTER JOIN ORG.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN ORG.Unit eu on eu.id=e.UnitId
                                   -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=e.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
                                    LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=e.LegalDesignationId

                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									FROM mst.DesignationMaster dm
									LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
		                           -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=dm.DesignationGroupId
                                   -- left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									AND egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    

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
                                     ) DD ORDER BY CONVERT(INT,EmployeeCode)";
                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<SalaryRegisterSorting> GetPlantWiseSalaryRegisterSortingParameters(string companyGroupId, string companyId, string plantId)
        {
            try
            {
                string strSQL = "";
                strSQL = @"SELECT Parameter,Sequence FROM PlantWiseSalaryRegisterSortingParameters WHERE CompanyGroupId = '" + companyGroupId + @"' AND CompanyId = '" + companyId + @"' AND PlantId = '" + plantId + "'";
                return _sqlRepository.GetModelCollection<SalaryRegisterSorting>(strSQL, null);
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }



        #region Cell Style
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

        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, string Text)
        {
            //if (string.IsNullOrEmpty(Text) == false)
            //{
            sheet.Range[xlsRow, xlsCol].Text = Text;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //}
        }

        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, double Value)
        {
            // string NumberFormatString = "#,##0;(#,##0)";
            //if (string.IsNullOrEmpty(Value.to) == false)
            //{
            // if (dvSlrProc[i]["SalaryHeadID"].ToString() == "SHD2017-1" & string.IsNullOrEmpty(dvSlrProc[i]["SalaryHeadID"].ToString()) == false)
            // ColBasSlr += Convert.ToDecimal(dvSlrProc[i]["DisbusmentAmount"].ToString());

            sheet.Range[xlsRow - 1, xlsCol].Number = Value;
            sheet.Range[xlsRow - 1, xlsCol].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[xlsRow - 1, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow - 1, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //}
        }
        private void SetCellTextAttdn(IWorksheet sheet, int xlsRow, int xlsCol, double Value)
        {
            //string NumberFormatString = "#,##0;(#,##0)";
            //if (string.IsNullOrEmpty(Value.to) == false)
            //{
            // if (dvSlrProc[i]["SalaryHeadID"].ToString() == "SHD2017-1" & string.IsNullOrEmpty(dvSlrProc[i]["SalaryHeadID"].ToString()) == false)
            // ColBasSlr += Convert.ToDecimal(dvSlrProc[i]["DisbusmentAmount"].ToString());

            sheet.Range[xlsRow, xlsCol].Number = Value;
            sheet.Range[xlsRow, xlsCol].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //}
        }
        private void SetCellTextDR(IWorksheet sheet, int xlsRow, int xlsCol, double Value)
        {
            //string NumberFormatString = "#,##0;(#,##0)";
            //if (string.IsNullOrEmpty(Value.to) == false)
            //{
            // if (dvSlrProc[i]["SalaryHeadID"].ToString() == "SHD2017-1" & string.IsNullOrEmpty(dvSlrProc[i]["SalaryHeadID"].ToString()) == false)
            // ColBasSlr += Convert.ToDecimal(dvSlrProc[i]["DisbusmentAmount"].ToString());

            sheet.Range[xlsRow, xlsCol].Number = Value;
            sheet.Range[xlsRow, xlsCol].NumberFormat = clsStaticInfo.NumberFormat(0);
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //}
        }

        private void SetCellTextNumber(IWorksheet sheet, int xlsRow, int xlsCol, double Value)
        {
            //string NumberFormatString = "#,##0;(#,##0)";
            //if (string.IsNullOrEmpty(Value.to) == false)
            //{
            // if (dvSlrProc[i]["SalaryHeadID"].ToString() == "SHD2017-1" & string.IsNullOrEmpty(dvSlrProc[i]["SalaryHeadID"].ToString()) == false)
            // ColBasSlr += Convert.ToDecimal(dvSlrProc[i]["DisbusmentAmount"].ToString());

            sheet.Range[xlsRow, xlsCol].Number = Value;
            sheet.Range[xlsRow, xlsCol].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //}
        }
        #endregion

        #region Salary Structure Query
        public void GetEmpSalaryInformationRpt(string plantId, string effectiveDate, string payRollGroup, Dictionary<string, string> parameters, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            clsStaticInfo obs = null;
            try
            {

                obs = new clsStaticInfo();
                strSql = @"SELECT * FROM
                          (
                           SELECT E.SystemID,  E.EmployeeCode EmployeeCode, E.EmployeeName, REPLACE(Convert(VARCHAR(11), E.DOB, 106), ' ', '-') AS DOB,
	                              E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID,
	                              E.GenderID GenderName, REPLACE(Convert(VARCHAR(11), E.DOJ, 106), ' ', '-') AS DOJ,
                                  REPLACE(Convert(VARCHAR(11), E.DOS, 106), ' ', '-') AS DOS,
	                              REPLACE(Convert(VARCHAR(11), E.DOC, 106), ' ', '-') AS DOC, DG.UserName DesignationGroup, D.UserName Designation,ISNULL(LG.UserName,'') LegalDesignation,
								  D.UserName GivenDesignation, L.UserName Line, U.UserName Unit, Dv.UserName Division, Dp.UserName Department,
								  S.UserName Section, SB.UserName SubSection, EC.UserName AS EmpCategory, Cm.UserName CompanyName, CAM.Address1,
	                              CAM.Address2, E.EmployeeCategorySystemID, E.UnitID, E.DivisionID, E.DepartmentID, E.DesignationSystemID,
	                              E.SectionID, E.SubSectionID, E.LineID, E.DesignationGroupID, E.SubSecStrucSystemID, E.EmployeeStatus,
	                              P.UserName PlantName, (PAM.[Address1] + ', ' + PAM.[Address2] + ', ' + PAMC.UserName + ' - ' + PAM.Postcode) FactoryAddress,
	                              GC.UserName GroupName, (CGAM.[Address1] + ', ' + CGAM.[Address2] + ', ' + CT.UserName + ' - ' + CGAM.Postcode + ', Contact: ' + CGAM.Phone) GroupAddress,
	                              E.PlantID, BK.UserName BankNameShort, E.BankAccNo, 
								  EmpSlr.SalaryHeadID, SH.SalaryHead, ISNULL(PSH.Sequence, 99) Sequence, SH.HeadType, ISNULL(SH.HeadCategory,'') HeadCategory, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount,
	                              EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, EmpSlr.AmtDefinitionCurrencyID, EmpSlr.AmtDefinationRate
	                              , EmpSlr.EmpInfoSystemID, MW.SalaryHeadValue                                
	                            ,CRC.IntegerInDisb, CRC.DecimalNo, MW.Grade,CRC.IsDecimalInDisb IsDecimal
                                ,ISNULL(E.GenderID,'') Gender,ISNULL(LSalGr.Code,'') GradeCode


											,ISNULL(PG.UserName,'') PayRollGroup

                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
				            FROM (SELECT * FROM EmployeeInformation  WHERE (EmployeeStatus != 'Separated' or DOS is null or DOS >='" + effectiveDate + @"')) AS E

                                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                                            LEFT JOIN ORG.Section S ON E.SectionID = S.Id
                                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                                           LEFT JOIN[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode

                                            LEFT JOIN ORG.Line L ON MB.LineID = L.Id
											LEFT JOIN [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
												  LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									LEFT JOIN [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
                                    LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId

									LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                            LEFT JOIN HKP.DesignationGroup DG ON E.DesignationGroupID = Dg.Id
                                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                                            LEFT JOIN HKP.LegalDesignation LG ON E.LegalDesignationId = LG.Id
                                            LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LG.Id and E.PlantId = LSGD.PlantId
                                            LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = LSGD.LegalSalaryGradeId  and E.PlantId = LSalGr.PlantId
												
											LEFT JOIN ORG.Plant AS p ON E.PlantId = p.Id
											LEFT JOIN ORG.Company AS Cm ON E.CompanyID = Cm.Id
											LEFT JOIN ORG.CompanyGroup AS GC ON E.GroupID = GC.Id
											LEFT JOIN HKP.Bank AS BK ON E.BankSystemID = BK.Id
											LEFT JOIN MST.AddressMaster AS CAM ON Cm.AddressMasterId = CAM.Id
											LEFT JOIN MST.AddressMaster AS PAM ON P.AddressMasterId = PAM.Id
											LEFT JOIN MST.AddressMaster AS CGAM ON GC.AddressMasterId = CGAM.Id
											LEFT JOIN SCS.City AS PAMC ON PAM.CityId = PAMC.Id
											LEFT JOIN SCS.City AS CT ON CGAM.CityId = CT.Id
                                            LEFT JOIN
													(
													 SELECT ECT.Id, ECT.UserName, DM.DesignationId 
													  FROM [HKP].[EmployeeCategory] ECT
																	LEFT JOIN MST.DesignationMaster DM ON ECT.Id = DM.EmployeeCategoryId
													) EC ON EC.DesignationId = E.GivenDesignationId
											LEFT JOIN 
													(
													 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
														FROM EmployeeInformation E   
																LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
																LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                                                                                                AND E.PlantId = gd.PlantId
																LEFT JOIN (
																			SELECT MAX(EffectiveDate) EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
																				FROM MST.LegalSalaryStructure 
																				WHERE EffectiveDate <= '" + effectiveDate + @"'
																			GROUP BY LegalSalaryGradeId, EmployeeLocationId 
																		  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
																LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                                                                                            AND SS.EmployeeLocationId = S.EmployeeLocationId 
                                                                                            AND SS.EffectiveDate = S.EffectiveDate
																LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 	
                                                                left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId	
														GROUP BY E.SystemId,LSG.UserName
													) MW ON MW.SystemId = E.SystemId

										
												INNER JOIN (
													SELECT * FROM
																(
																 --SELECT MST.EmpInfoSystemID, EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, 
																	--	EmpSlr.AmtDefinitionCurrencyID AmtDefinationCurrencyID, EmpSlr.AmtDefinitionRate AmtDefinationRate, MST.SalaryRuleMasterSystemID
																-- FROM SalaryInfoDefine EmpSlr
																	--	INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID 
                                                                   Select SalaryDetails.* from  ( SELECT MAX(EffectiveDate) EffectiveDate,EmpInfoSystemID--,SalaryHead,SalaryHeadID,EntryCurrencyID
	 FROM (
	           SELECT MST.EmpInfoSystemID,SH.SalaryHead, EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, 
				EmpSlr.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, EmpSlr.AmtDefinitionRate AmtDefinationRate, MST.SalaryRuleMasterSystemID,MST.EffectiveDate
					FROM SalaryInfoDefine EmpSlr
					INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID AND MST.IsApproved = 1 
					left outer join SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID
					--where EmpInfoSystemID = '1800118'
					UNION
					SELECT SBM.EmpInfoSystemID,SH.SalaryHead,SIB.SalaryHeadID,SIB.EntryCurrencyID,SIB.EntryAmount,SIB.DefineCurrencyID,SIB.DefineAmount
					,SIB.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, SIB.AmtDefinitionRate, SBM.SalaryRuleMasterSystemID,SBM.EffectiveDate from SalaryInfoBack SIB
					INNER JOIN SalaryInfoBackMaster SBM ON SIB.SalaryID = SBM.SystemID 
					left outer join SalaryHead SH ON SH.SalaryHeadID = SIB.SalaryHeadID
					--where EmpInfoSystemID = '1800118'
                        )dd where EffectiveDate <= '" + effectiveDate + @"' 					

					GROUP BY EmpInfoSystemID) effDateSalary


					Inner JOIN
					
            ( SELECT EmpInfoSystemID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount 
			,AmtDefinitionCurrencyID , AmtDefinationRate, SalaryRuleMasterSystemID,EffectiveDate
	            FROM (
	           SELECT MST.EmpInfoSystemID,SH.SalaryHead, EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, 
				EmpSlr.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, EmpSlr.AmtDefinitionRate AmtDefinationRate, MST.SalaryRuleMasterSystemID,MST.EffectiveDate
					FROM SalaryInfoDefine EmpSlr
					INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID AND MST.IsApproved = 1
					LEFT OUTER JOIN SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID
				--	WHERE EmpInfoSystemID = '1800118'
					UNION
					SELECT SBM.EmpInfoSystemID,SH.SalaryHead,SIB.SalaryHeadID,SIB.EntryCurrencyID,SIB.EntryAmount,SIB.DefineCurrencyID,SIB.DefineAmount
					,SIB.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, SIB.AmtDefinitionRate, SBM.SalaryRuleMasterSystemID,SBM.EffectiveDate from SalaryInfoBack SIB
					INNER JOIN SalaryInfoBackMaster SBM ON SIB.SalaryID = SBM.SystemID 
					left outer join SalaryHead SH ON SH.SalaryHeadID = SIB.SalaryHeadID
				--	where EmpInfoSystemID = '1800118'
                )dd where EffectiveDate <= '" + effectiveDate + @"'  ) SalaryDetails ON effDateSalary.EffectiveDate= SalaryDetails.EffectiveDate and effDateSalary.EmpInfoSystemID = SalaryDetails.EmpInfoSystemID



                                                                  -----------------------AND MST.IsApproved = 1---------------------
																) A
																
													) EmpSlr ON E.SystemID = EmpSlr.EmpInfoSystemID
										LEFT JOIN SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID
										LEFT JOIN (SELECT * FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId='" + plantId + @"') PSH ON PSH.SalaryHeadId = EmpSlr.SalaryHeadID
										
										LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EmpSlr.SalaryRuleMasterSystemID 
										--LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID	AND SRG.SalaryHeadID = SH.SalaryHeadID									
                                        LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = SH.SalaryHeadID

                                        
                         ) A  where  ISNULL(EmpInfoSystemID,'')<>'' AND PlantID = '" + plantId + @"' AND
                            Convert(date ,DOJ) <='" + effectiveDate + @"' AND (DOS IS NULL OR DOS >='" + effectiveDate + @"') ";

                if (parameters.Count > 0)
                {
                    if (parameters.Keys.ElementAt(0) != "")
                    {
                        strSql += @"and EmpInfoSystemID IN(" + parameters["EmpSystemId"] + ")";

                    }
                }


                strSql = strSql + @" ORDER BY EmployeeCode";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSql, out dsRef);

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetEmpSalaryInformationRptPlantWise(string plantList, string effectiveDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            clsStaticInfo obs = null;
            try
            {

                obs = new clsStaticInfo();
                strSql = @"SELECT * FROM
                          (
                           SELECT E.SystemID,  E.EmployeeCode EmployeeCode, E.EmployeeName, REPLACE(Convert(VARCHAR(11), E.DOB, 106), ' ', '-') AS DOB,
	                              E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID,
	                              E.GenderID GenderName, REPLACE(Convert(VARCHAR(11), E.DOJ, 106), ' ', '-') AS DOJ,
                                  REPLACE(Convert(VARCHAR(11), E.DOS, 106), ' ', '-') AS DOS,
	                              REPLACE(Convert(VARCHAR(11), E.DOC, 106), ' ', '-') AS DOC, DG.UserName DesignationGroup, D.UserName Designation,ISNULL(LG.UserName,'') LegalDesignation,
								  D.UserName GivenDesignation, L.UserName Line, U.UserName Unit, Dv.UserName Division, Dp.UserName Department,
								  S.UserName Section, SB.UserName SubSection, EC.UserName AS EmpCategory, Cm.UserName CompanyName, CAM.Address1,
	                              CAM.Address2, E.EmployeeCategorySystemID, E.UnitID, E.DivisionID, E.DepartmentID, E.DesignationSystemID,
	                              E.SectionID, E.SubSectionID, E.LineID, E.DesignationGroupID, E.SubSecStrucSystemID, E.EmployeeStatus,
	                              P.UserName PlantName, (PAM.[Address1] + ', ' + PAM.[Address2] + ', ' + PAMC.UserName + ' - ' + PAM.Postcode) FactoryAddress,
	                              GC.UserName GroupName, (CGAM.[Address1] + ', ' + CGAM.[Address2] + ', ' + CT.UserName + ' - ' + CGAM.Postcode + ', Contact: ' + CGAM.Phone) GroupAddress,
	                              E.PlantID, BK.UserName BankNameShort, E.BankAccNo, 
								  EmpSlr.SalaryHeadID, SH.SalaryHead, ISNULL(PSH.Sequence, 99) Sequence, SH.HeadType, ISNULL(SH.HeadCategory,'') HeadCategory, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount,
	                              EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, EmpSlr.AmtDefinitionCurrencyID, EmpSlr.AmtDefinationRate
	                              , EmpSlr.EmpInfoSystemID, MW.SalaryHeadValue                                
	                            ,CRC.IntegerInDisb, CRC.DecimalNo, MW.Grade,CRC.IsDecimalInDisb IsDecimal
                                ,ISNULL(E.GenderID,'') Gender,ISNULL(LSalGr.Code,'') GradeCode


											,ISNULL(PG.UserName,'') PayRollGroup

                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
				            FROM (SELECT * FROM EmployeeInformation  WHERE (EmployeeStatus != 'Separated' or DOS is null or DOS >='" + effectiveDate + @"')) AS E

                                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                                            LEFT JOIN ORG.Section S ON E.SectionID = S.Id
                                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                                           LEFT JOIN[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode

                                            LEFT JOIN ORG.Line L ON MB.LineID = L.Id
											LEFT JOIN [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
												  LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									LEFT JOIN [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
                                    LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId

									LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                            LEFT JOIN HKP.DesignationGroup DG ON E.DesignationGroupID = Dg.Id
                                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                                            LEFT JOIN HKP.LegalDesignation LG ON E.LegalDesignationId = LG.Id
                                            LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LG.Id and E.PlantId = LSGD.PlantId
                                            LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = LSGD.LegalSalaryGradeId  and E.PlantId = LSalGr.PlantId
												
											LEFT JOIN ORG.Plant AS p ON E.PlantId = p.Id
											LEFT JOIN ORG.Company AS Cm ON E.CompanyID = Cm.Id
											LEFT JOIN ORG.CompanyGroup AS GC ON E.GroupID = GC.Id
											LEFT JOIN HKP.Bank AS BK ON E.BankSystemID = BK.Id
											LEFT JOIN MST.AddressMaster AS CAM ON Cm.AddressMasterId = CAM.Id
											LEFT JOIN MST.AddressMaster AS PAM ON P.AddressMasterId = PAM.Id
											LEFT JOIN MST.AddressMaster AS CGAM ON GC.AddressMasterId = CGAM.Id
											LEFT JOIN SCS.City AS PAMC ON PAM.CityId = PAMC.Id
											LEFT JOIN SCS.City AS CT ON CGAM.CityId = CT.Id
                                            LEFT JOIN
													(
													 SELECT ECT.Id, ECT.UserName, DM.DesignationId 
													  FROM [HKP].[EmployeeCategory] ECT
																	LEFT JOIN MST.DesignationMaster DM ON ECT.Id = DM.EmployeeCategoryId
													) EC ON EC.DesignationId = E.GivenDesignationId
											LEFT JOIN 
													(
													 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
														FROM EmployeeInformation E   
																LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
																LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                                                                                                AND E.PlantId = gd.PlantId
																LEFT JOIN (
																			SELECT MAX(EffectiveDate) EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
																				FROM MST.LegalSalaryStructure 
																				WHERE EffectiveDate <= '" + effectiveDate + @"'
																			GROUP BY LegalSalaryGradeId, EmployeeLocationId 
																		  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
																LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                                                                                            AND SS.EmployeeLocationId = S.EmployeeLocationId 
                                                                                            AND SS.EffectiveDate = S.EffectiveDate
																LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 	
                                                                left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId	
														GROUP BY E.SystemId,LSG.UserName
													) MW ON MW.SystemId = E.SystemId

										
												INNER JOIN (
													SELECT * FROM
																(
																 --SELECT MST.EmpInfoSystemID, EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, 
																	--	EmpSlr.AmtDefinitionCurrencyID AmtDefinationCurrencyID, EmpSlr.AmtDefinitionRate AmtDefinationRate, MST.SalaryRuleMasterSystemID
																-- FROM SalaryInfoDefine EmpSlr
																	--	INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID 
                                                                   Select SalaryDetails.* from  ( SELECT MAX(EffectiveDate) EffectiveDate,EmpInfoSystemID--,SalaryHead,SalaryHeadID,EntryCurrencyID
	 FROM (
	           SELECT MST.EmpInfoSystemID,SH.SalaryHead, EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, 
				EmpSlr.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, EmpSlr.AmtDefinitionRate AmtDefinationRate, MST.SalaryRuleMasterSystemID,MST.EffectiveDate
					FROM SalaryInfoDefine EmpSlr
					INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID AND MST.IsApproved = 1 
					left outer join SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID
					--where EmpInfoSystemID = '1800118'
					UNION
					SELECT SBM.EmpInfoSystemID,SH.SalaryHead,SIB.SalaryHeadID,SIB.EntryCurrencyID,SIB.EntryAmount,SIB.DefineCurrencyID,SIB.DefineAmount
					,SIB.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, SIB.AmtDefinitionRate, SBM.SalaryRuleMasterSystemID,SBM.EffectiveDate from SalaryInfoBack SIB
					INNER JOIN SalaryInfoBackMaster SBM ON SIB.SalaryID = SBM.SystemID 
					left outer join SalaryHead SH ON SH.SalaryHeadID = SIB.SalaryHeadID
					--where EmpInfoSystemID = '1800118'
                        )dd where EffectiveDate <= '" + effectiveDate + @"' 					

					GROUP BY EmpInfoSystemID) effDateSalary


					Inner JOIN
					
            ( SELECT EmpInfoSystemID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount 
			,AmtDefinitionCurrencyID , AmtDefinationRate, SalaryRuleMasterSystemID,EffectiveDate
	            FROM (
	           SELECT MST.EmpInfoSystemID,SH.SalaryHead, EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, 
				EmpSlr.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, EmpSlr.AmtDefinitionRate AmtDefinationRate, MST.SalaryRuleMasterSystemID,MST.EffectiveDate
					FROM SalaryInfoDefine EmpSlr
					INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID AND MST.IsApproved = 1
					LEFT OUTER JOIN SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID
				--	WHERE EmpInfoSystemID = '1800118'
					UNION
					SELECT SBM.EmpInfoSystemID,SH.SalaryHead,SIB.SalaryHeadID,SIB.EntryCurrencyID,SIB.EntryAmount,SIB.DefineCurrencyID,SIB.DefineAmount
					,SIB.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, SIB.AmtDefinitionRate, SBM.SalaryRuleMasterSystemID,SBM.EffectiveDate from SalaryInfoBack SIB
					INNER JOIN SalaryInfoBackMaster SBM ON SIB.SalaryID = SBM.SystemID 
					left outer join SalaryHead SH ON SH.SalaryHeadID = SIB.SalaryHeadID
				--	where EmpInfoSystemID = '1800118'
                )dd where EffectiveDate <= '" + effectiveDate + @"'  ) SalaryDetails ON effDateSalary.EffectiveDate= SalaryDetails.EffectiveDate and effDateSalary.EmpInfoSystemID = SalaryDetails.EmpInfoSystemID



                                                                  -----------------------AND MST.IsApproved = 1---------------------
																) A
																
													) EmpSlr ON E.SystemID = EmpSlr.EmpInfoSystemID
										LEFT JOIN SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID
										LEFT JOIN (SELECT * FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId IN(" + plantList + @")) PSH ON PSH.SalaryHeadId = EmpSlr.SalaryHeadID
										
										LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EmpSlr.SalaryRuleMasterSystemID 
										--LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID	AND SRG.SalaryHeadID = SH.SalaryHeadID									
                                        LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = SH.SalaryHeadID

                                        
                         ) A  where  ISNULL(EmpInfoSystemID,'')<>'' AND PlantID IN(" + plantList + @") AND
                            Convert(date ,DOJ) <='" + effectiveDate + @"' AND (DOS IS NULL OR DOS >='" + effectiveDate + @"') ";

                //if (parameters.Count > 0)
                //{
                //    if (parameters.Keys.ElementAt(0) != "")
                //    {
                //        strSql += @"and EmpInfoSystemID IN(" + parameters["EmpSystemId"] + ")";

                //    }
                //}


                strSql = strSql + @" ORDER BY EmployeeCode";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSql, out dsRef);

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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


        #endregion

        #region Salary Structure Query
        public void GetSeparatedEmployeeSalaryInformationRpt(string plantId, string effectiveDate, string FromDate, string ToDate, string payRollGroup, Dictionary<string, string> parameters, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            clsStaticInfo obs = null;
            try
            {
                obs = new clsStaticInfo();
                strSql = @"SELECT * FROM
                          (
                           SELECT E.SystemID, convert(int, E.EmployeeCode)EmployeeCode, E.EmployeeName, REPLACE(Convert(VARCHAR(11), E.DOB, 106), ' ', '-') AS DOB,
	                              E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID,
	                              E.GenderID GenderName, REPLACE(Convert(VARCHAR(11), E.DOJ, 106), ' ', '-') AS DOJ,
                                  REPLACE(Convert(VARCHAR(11), E.DOS, 106), ' ', '-') AS DOS,
	                              REPLACE(Convert(VARCHAR(11), E.DOC, 106), ' ', '-') AS DOC, DG.UserName DesignationGroup, D.UserName Designation,ISNULL(LG.UserName,'') LegalDesignation,
								  D.UserName GivenDesignation, L.UserName Line, U.UserName Unit, Dv.UserName Division, Dp.UserName Department,
								  S.UserName Section, SB.UserName SubSection, EC.UserName AS EmpCategory, Cm.UserName CompanyName, CAM.Address1,
	                              CAM.Address2, E.EmployeeCategorySystemID, E.UnitID, E.DivisionID, E.DepartmentID, E.DesignationSystemID,
	                              E.SectionID, E.SubSectionID, E.LineID, E.DesignationGroupID, E.SubSecStrucSystemID, E.EmployeeStatus,
	                              P.UserName PlantName, (PAM.[Address1] + ', ' + PAM.[Address2] + ', ' + PAMC.UserName + ' - ' + PAM.Postcode) FactoryAddress,
	                              GC.UserName GroupName, (CGAM.[Address1] + ', ' + CGAM.[Address2] + ', ' + CT.UserName + ' - ' + CGAM.Postcode + ', Contact: ' + CGAM.Phone) GroupAddress,
	                              E.PlantID, BK.UserName BankNameShort, E.BankAccNo, 
								  EmpSlr.SalaryHeadID, SH.SalaryHead, ISNULL(PSH.Sequence, 99) Sequence, SH.HeadType, ISNULL(SH.HeadCategory,'') HeadCategory, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount,
	                              EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, EmpSlr.AmtDefinitionCurrencyID, EmpSlr.AmtDefinationRate
	                              , EmpSlr.EmpInfoSystemID, MW.SalaryHeadValue                                
	                            ,CRC.IntegerInDisb, CRC.DecimalNo, MW.Grade,CRC.IsDecimalInDisb IsDecimal
				            FROM (SELECT * FROM EmployeeInformation  WHERE (DOS is not null and  DOS Between '" + FromDate + @"' and '" + ToDate + @"')) AS E

                                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                                            LEFT JOIN ORG.Section S ON E.SectionID = S.Id
                                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                                            LEFT JOIN ORG.Line L ON E.LineID = L.Id
                                            LEFT JOIN HKP.DesignationGroup DG ON E.DesignationGroupID = Dg.Id
                                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                                            LEFT JOIN HKP.LegalDesignation LG ON E.LegalDesignationId = LG.Id
											LEFT JOIN ORG.Plant AS p ON E.PlantId = p.Id
											LEFT JOIN ORG.Company AS Cm ON E.CompanyID = Cm.Id
											LEFT JOIN ORG.CompanyGroup AS GC ON E.GroupID = GC.Id
											LEFT JOIN HKP.Bank AS BK ON E.BankSystemID = BK.Id
											LEFT JOIN MST.AddressMaster AS CAM ON Cm.AddressMasterId = CAM.Id
											LEFT JOIN MST.AddressMaster AS PAM ON P.AddressMasterId = PAM.Id
											LEFT JOIN MST.AddressMaster AS CGAM ON GC.AddressMasterId = CGAM.Id
											LEFT JOIN SCS.City AS PAMC ON PAM.CityId = PAMC.Id
											LEFT JOIN SCS.City AS CT ON CGAM.CityId = CT.Id
                                            LEFT JOIN
													(
													 SELECT ECT.Id, ECT.UserName, DM.DesignationId 
													  FROM [HKP].[EmployeeCategory] ECT
																	LEFT JOIN MST.DesignationMaster DM ON ECT.Id = DM.EmployeeCategoryId
													) EC ON EC.DesignationId = E.GivenDesignationId
											LEFT JOIN 
													(
													 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
														FROM EmployeeInformation E   
																LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
																LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                                                                                                AND E.PlantId = gd.PlantId
																LEFT JOIN (
																			SELECT MAX(EffectiveDate) EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
																				FROM MST.LegalSalaryStructure 
																				--WHERE EffectiveDate <= '" + effectiveDate + @"'
																			GROUP BY LegalSalaryGradeId, EmployeeLocationId 
																		  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
																LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                                                                                            AND SS.EmployeeLocationId = S.EmployeeLocationId 
                                                                                            AND SS.EffectiveDate = S.EffectiveDate
																LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 	
                                                                left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId	
														GROUP BY E.SystemId,LSG.UserName
													) MW ON MW.SystemId = E.SystemId

										
												INNER JOIN (
													SELECT * FROM
																(
																 --SELECT MST.EmpInfoSystemID, EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, 
																	--	EmpSlr.AmtDefinitionCurrencyID AmtDefinationCurrencyID, EmpSlr.AmtDefinitionRate AmtDefinationRate, MST.SalaryRuleMasterSystemID
																-- FROM SalaryInfoDefine EmpSlr
																	--	INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID 
                                                                   Select SalaryDetails.* from  ( SELECT MAX(EffectiveDate) EffectiveDate,EmpInfoSystemID--,SalaryHead,SalaryHeadID,EntryCurrencyID
	 FROM (
	           SELECT MST.EmpInfoSystemID,SH.SalaryHead, EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, 
				EmpSlr.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, EmpSlr.AmtDefinitionRate AmtDefinationRate, MST.SalaryRuleMasterSystemID,MST.EffectiveDate
					FROM SalaryInfoDefine EmpSlr
					INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID AND MST.IsApproved = 1 
					left outer join SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID
					
					UNION
					SELECT SBM.EmpInfoSystemID,SH.SalaryHead,SIB.SalaryHeadID,SIB.EntryCurrencyID,SIB.EntryAmount,SIB.DefineCurrencyID,SIB.DefineAmount
					,SIB.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, SIB.AmtDefinitionRate, SBM.SalaryRuleMasterSystemID,SBM.EffectiveDate from SalaryInfoBack SIB
					INNER JOIN SalaryInfoBackMaster SBM ON SIB.SalaryID = SBM.SystemID 
					left outer join SalaryHead SH ON SH.SalaryHeadID = SIB.SalaryHeadID
					
                        )dd --where EffectiveDate <= '" + effectiveDate + @"' 					

					GROUP BY EmpInfoSystemID) effDateSalary


					Inner JOIN
					
            ( SELECT EmpInfoSystemID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount 
			,AmtDefinitionCurrencyID , AmtDefinationRate, SalaryRuleMasterSystemID,EffectiveDate
	            FROM (
	           SELECT MST.EmpInfoSystemID,SH.SalaryHead, EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, 
				EmpSlr.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, EmpSlr.AmtDefinitionRate AmtDefinationRate, MST.SalaryRuleMasterSystemID,MST.EffectiveDate
					FROM SalaryInfoDefine EmpSlr
					INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID AND MST.IsApproved = 1
					LEFT OUTER JOIN SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID
				
					UNION
					SELECT SBM.EmpInfoSystemID,SH.SalaryHead,SIB.SalaryHeadID,SIB.EntryCurrencyID,SIB.EntryAmount,SIB.DefineCurrencyID,SIB.DefineAmount
					,SIB.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, SIB.AmtDefinitionRate, SBM.SalaryRuleMasterSystemID,SBM.EffectiveDate from SalaryInfoBack SIB
					INNER JOIN SalaryInfoBackMaster SBM ON SIB.SalaryID = SBM.SystemID 
					left outer join SalaryHead SH ON SH.SalaryHeadID = SIB.SalaryHeadID
			
                )dd --where EffectiveDate <= '" + effectiveDate + @"'  
                ) SalaryDetails ON effDateSalary.EffectiveDate= SalaryDetails.EffectiveDate and effDateSalary.EmpInfoSystemID = SalaryDetails.EmpInfoSystemID



                                                                  -----------------------AND MST.IsApproved = 1---------------------
																) A
																
													) EmpSlr ON E.SystemID = EmpSlr.EmpInfoSystemID
										LEFT JOIN SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID
										LEFT JOIN (SELECT * FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId='" + plantId + @"') PSH ON PSH.SalaryHeadId = EmpSlr.SalaryHeadID
										
										LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EmpSlr.SalaryRuleMasterSystemID 
										--LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID	AND SRG.SalaryHeadID = SH.SalaryHeadID									
                                        LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = SH.SalaryHeadID

                                        
                         ) A  where  ISNULL(EmpInfoSystemID,'')<>'' AND PlantID = '" + plantId + @"' AND
                            --Convert(date ,DOJ) <='" + effectiveDate + @"' AND 
                        (DOS is not null and  DOS between '" + FromDate + @"' and '" + ToDate + @"') ";

                if (parameters.Count > 0)
                {
                    if (parameters.Keys.ElementAt(0) != "")
                    {
                        strSql += @"and EmployeeCode IN(" + parameters["EmployeeCode"] + ")";

                    }
                }


                strSql = strSql + @" ORDER BY EmployeeCode";

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
        #endregion

        #region SalaryProcessed Query
        public void GetSalaryInfoSlrProcIDWiseCombined(string plantId, string fromDate, string toDate, string salaryProcessSystemId, string payRollGroup, Dictionary<string, string> parameters, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            var _wc = string.Empty;
            var wcSalaryProcessSystemIdStr = "";
            //var wcPayrollGroup = "";
            //payRollGroup = "'" + payRollGroup.Replace(",", "','") + "'";
            //if (payRollGroup.Contains("NG") == false)
            //{

            //    wcPayrollGroup = @"AND EmpInfoSystemID IN (SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId IN (" + payRollGroup + @"))";
            //}
            //if (payRollGroup.Contains("NG") == true && payRollGroup.Length == 2)
            //{

            //    wcPayrollGroup = @"AND EmpInfoSystemID Not IN (SELECT employeeid from MST.PayrollGroupMaster)";
            //}
            //else
            //{
            //    wcPayrollGroup = @"AND (EmpInfoSystemID NOT IN (SELECT employeeid from MST.PayrollGroupMaster) OR EmpInfoSystemID  IN (SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId IN (" + payRollGroup + @")) )";
            //}

            if (!string.IsNullOrEmpty(salaryProcessSystemId) && salaryProcessSystemId != "undefined" && salaryProcessSystemId != "null")
            {
                wcSalaryProcessSystemIdStr = "SystemID IN ('" + salaryProcessSystemId + @"')";
            }
            else
            {
                wcSalaryProcessSystemIdStr = @"SystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + fromDate + "') AND YearNo = Year('" + fromDate + "')  )";
            }


            try
            {
                strSQL = @"SELECT EmpSlr.EmpInfoSystemID, EmpBasic.EmployeeCode, EmpBasic.EmployeeName, Replace(Convert(varchar(11),EmpBasic.DOJ,106),' ','-') DOJ,DOB,
                                EmpBasic.EmployeeStatus,EmpBasic.EmployeeCurrentStatus, EmpBasic.UnitID, EmpBasic.UnitName Unit, EmpBasic.DivisionID, EmpBasic.DivisionName Division,
                                EmpBasic.DepartmentID, EmpBasic.DepartmentName Department, EmpBasic.SectionID, EmpBasic.SectionName Section, EmpBasic.SubSectionID,EmpBasic.UnitName,EmpBasic.SectionName,
                                EmpBasic.SubSectionName SubSection , EmpBasic.EmployeeCategorySystemID, EmpBasic.DesignationGroupName,
                                EmpBasic.DesignationSystemID, EmpBasic.DesignationName GivenDesignation,EmpBasic.GivenDesignationGroup, EmpBasic.PlantName Plant,
								EmpBasic.EmpCategoryName EmployeeCategory,EmpBasic.DOJ,EmpBasic.DOS,EmpBasic.legalDesignation,ISNULL(EmpBasic.PaymentMode,'') PaymentMode,
                                '' BankName, '' BankNameFull, '' BankAccNo,ISNULL(EmpBasic.NationalID,'') NationalID,PF.UANNo,ESIC.ESICNo,  
								(ISNULL(MMDSA.TotalPresent, 0) + ISNULL(MMDSA.TotalLate, 0))  PresentDays,ISNULL(MMDSA.TotalProcDate, 0) TotalProcDate,
								ISNULL(MMDSA.AbsentDays, 0) AbsentDays, ISNULL(MMDSA.TotalHoliDay, 0) HoliDay, ISNULL(MMDSA.TotalWeekOff, 0) WeekOff,
								(ISNULL(MMDSA.TotalLv, 0) + ISNULL(MMDSA.TotalMLv, 0)) LeaveDays,ISNULL(MMDSA.TotalLWP,0) TotalLWP, EmpSlr.SlrProcChdSysID, EmpSlr.SlrProcMstSystemID, EmpSlr.SalaryProcID,
                                EmpSlr.PlantID, EmpSlr.FromDate, EmpSlr.ToDate, EmpSlr.MonthNo, EmpSlr.YearNo, EmpSlr.PayAbleShSystemID,
                                EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount,
                                EmpSlr.DisbusmentCurrencyID, EmpSlr.DisbusmentAmount, EmpSlr.AcltExcDisbSlrHDID, EmpSlr.AcltExcDisbSlrHDAmt,
                                EmpSlr.PlantWiseExchangeCR, EmpSlr.ExchangeRate, EmpSlr.AmtDefinitionCurrencyID, EmpSlr.AmtDefinitionCurrency,
                                EmpSlr.AmtDefinitionCurrencyRate, EmpSlr.IsNetPayEffect,EmpCategoryName,EmpSlr.HeadType,EmpSlr.IsCTCComponent,EmpSlr.IsGrossComponent
                                ,EmpSlr.SalaryHead,EmpSlr.HeadCategory,EmpSlr.HeadType,ISNULL(psh.Sequence,'99')  Sequence,CRC.IntegerInDisb,CRC.DecimalNo,CRC.IsDecimalInDisb
                                , ISNULL(MW.SalaryHeadValue,0) MinimumWage,MW.Grade, EmpSlr.PartOfNetPay --,ISNULL(EmpSlr.SalaryHeadBangla,EmpSlr.SalaryHead) SalaryHeadBangla  
                            FROM
                                    (
									 SELECT E.SystemID, E.EmployeeCode, E.EmployeeName, E.EmployeeStatus,E.PaymentMode,E.EmployeeCurrentStatus
											, DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,
											'' UserGroupSystemID, E.PlantID, F.UserName PlantName, E.UnitID,
											FU.UserName UnitName, E.DivisionID, DV.UserName DivisionName, E.DepartmentID, DP.UserName DepartmentName,
											E.SectionID, S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID,
											EC.UserName EmpCategoryName--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo
                                            ,egdsgg.GivenDesignationGroup,e.SalaryRuleMasterSystemID,Format(E.DOJ,'dd-MMM-yyyy') DOJ,Format(E.DOS,'dd-MMM-yyyy') DOS,Format(E.DOB,'dd-MMM-yyyy') DOB
											,ISNULL(LDS.UserName,'') LegalDesignation,ISNULL(E.NationalID,'') NationalID
                                     FROM EmployeeInformation E
												LEFT JOIN ORG.Plant F ON E.PlantID = F.Id
												LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupId = DG.ID
												LEFT JOIN hkp.Designation DE ON E.GivenDesignationId = DE.Id
												LEFT JOIN hkp.LegalDesignation LDS ON E.LegalDesignationId = LDS.Id

												LEFT JOIN org.Unit FU ON E.UnitID = FU.Id
												LEFT JOIN org.Division DV ON E.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON E.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON E.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON E.SubSectionID = SS.Id
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
													CR.Name AS AmtDefinitionCurrency, SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect,SH.IsCTCComponent,SH.IsGrossComponent
                                                    ,sh.SalaryHead,sh.HeadCategory,sh.HeadType,SH.PartOfNetPay
                                                    
											 FROM SalaryProcChild SPC
																INNER JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
																							AND SPM." + wcSalaryProcessSystemIdStr + @"
                                                        LEFT JOIN SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
														--LEFT JOIN (select * from [MST].[PlantSalaryHeadSequence] where PlantId='" + plantId + @"' ) psh
																		-- psh.SalaryHeadId=spc.SalaryHeadID

														LEFT JOIN scs.Currency CR ON SPM.AmtDefinitionCurrencyID = CR.Id
														LEFT JOIN (
																   SELECT * FROM ExchangerateDateWiseForHR
																   WHERE FromDate IN (   SELECT MAX(FromDate) FromDate FROM SalaryProcMaster
																															   
																											WHERE " + wcSalaryProcessSystemIdStr + @"
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
												( select ed.DocNumber UANNo,ED.EmpSystemID from 
												EmployeeDocument ED 
												inner JOIN (select * from HKP.ComplianceDocument where ProfileType = 'PF') CD ON CD.Id = ED.ComplianceDocumentId 
												) pf on EmpBasic.SystemId = pf.EmpSystemID

												left join
												( select ed.DocNumber ESICNo,ED.EmpSystemID from 
												EmployeeDocument ED 
												inner JOIN (select * from HKP.ComplianceDocument where ProfileType = 'ESIC') CD ON CD.Id = ED.ComplianceDocumentId 
												) esic on EmpBasic.SystemId = esic.EmpSystemID

                                    LEFT JOIN
		                                    (
											 SELECT EmpSystemID, MonthNo, YearNo, TotalProcDate, TotalPresent, TotalLate,TotalLWP,
													TotalAbsent AbsentDays, TotalLv, TotalMLv, TotalCompAssignLv, TotalWeekOff, TotalHoliDay,
													TotalWeekOffHoliDay, TotalOTHr, TotalNormalOTHr, TotalExtraOTHr
				                              FROM AttdnDataMonthlySummary
											) MMDSA ON EmpSlr.EmpInfoSystemID = MMDSA.EmpSystemID AND EmpSlr.MonthNo = MMDSA.MonthNo AND
						                               EmpSlr.YearNo = MMDSA.YearNo
                                        WHERE (EmpBasic.EmployeeStatus != 'Separated' OR ISNULL(EmpBasic.DOS,'') = ''  OR COnvert(date,EmpBasic.DOS) >= Convert(Date,'" + fromDate + "')) AND COnvert(date,EmpBasic.DOJ) <=  Convert(Date,'" + toDate + "')"
                                        + _wc + @"";

                if (parameters.Count > 0)
                {
                    if (parameters.Keys.ElementAt(0) != "")
                    {
                        strSQL += @"and EmployeeCode IN(" + parameters["EmployeeCode"] + ")";

                    }
                }


                //strSQL = strSQL + @"ORDER BY EmpBasic.DivisionId,EmpBasic.SubdivisionID,EmpBasic.UnitId,EmpBasic.DepartmentId, EmpBasic.EmployeeCode";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
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

        public void xxGetEmployeeInfoDetail(string plantId, string fromDate, string toDate, string salaryProcessSystemId, string payRollGroup, Dictionary<string, string> parameters, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            var _wc = string.Empty;
            var wcSalaryProcessSystemIdStr = "";


            if (!string.IsNullOrEmpty(salaryProcessSystemId) && salaryProcessSystemId != "undefined" && salaryProcessSystemId != "null")
            {
                wcSalaryProcessSystemIdStr = "SystemID IN ('" + salaryProcessSystemId + @"')";
            }
            else
            {
                wcSalaryProcessSystemIdStr = @"SystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + fromDate + "') AND YearNo = Year('" + fromDate + "')  )";
            }


            try
            {
                strSQL = @"SELECT EmpBasic.*,MMDSA.*,ISNULL(MW.Grade,'') Grade,ISNULL(MW.SalaryHeadValue,0) MinimumWage

                            FROM

                                    
		                                    (
												select EmpSystemID,MonthNo,YearNo, ISNULL(TotalProcDate,0) TotalProcDate,IsNULL(TotalPresent,0) TotalPresent,ISNULL(TotalLate,0) TotalLate,ISNULL(TotalAbsent,'') TotalAbsent
										,ISNULL(TotalLv,0) TotalLv
										,ISNULL(TotalMLv,0) TotalMLv,ISNULL(TotalCompAssignLv,0) TotalCompAssignLv,ISNULL(TotalWeekOff,0) TotalWeekOff, ISNULL(TotalWeekOffHoliDay,0) TotalWeekOffHoliDay
										,ISNULL(TotalOTHr,0) TotalOTHr,ISNULL(TotalNormalOTHr,0) TotalNormalOTHr,ISNULL(TotalExtraOTHr,0) TotalExtraOTHr,ISNULL(WeekOffOTHr,0) WeekOffOTHr
										,ISNULL(HoliDayOTHr,0) HoliDayOTHr,ISNULL(TotalLWP,0) TotalLWP,ISNULL(IsOTEntitled,0) IsOTEntitled,ISNULL(OTRate,0) OTRate
										  FROM SalaryProceAttdnData MMDSA where MMDSA.MonthNo = MONTH('" + fromDate + @"') AND
						                               MMDSA.YearNo = YEAR('" + fromDate + @"')
											) MMDSA 
INNER JOIN
                                    (
									 SELECT E.SystemID EmpSystemId, E.EmployeeCode, E.EmployeeName, E.EmployeeStatus,E.EmployeeCurrentStatus
											, DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,
											'' UserGroupSystemID, E.PlantID, F.UserName PlantName, E.UnitID,
											FU.UserName UnitName, E.DivisionID, DV.UserName DivisionName, E.DepartmentID, DP.UserName DepartmentName,
											E.SectionID, S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID,
											EC.UserName EmpCategoryName--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo
                                            ,egdsgg.GivenDesignationGroup,e.SalaryRuleMasterSystemID,Format(E.DOJ,'dd-MMM-yyyy') DOJ,Format(E.DOS,'dd-MMM-yyyy') DOS,Format(E.DOB,'dd-MMM-yyyy') DOB
											,ISNULL(LDS.UserName,'') LegalDesignation,ISNULL(E.NationalID,'') NationalID
											,ISNULL(Line.UserName,'') LineName
											,ISNULL(E.GenderID,'') Gender


											,ISNULL(PG.UserName,'') PayRollGroup

                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                     FROM EmployeeInformation E
												LEFT JOIN ORG.Plant F ON E.PlantID = F.Id
												LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupId = DG.ID
												LEFT JOIN hkp.Designation DE ON E.GivenDesignationId = DE.Id
												LEFT JOIN hkp.LegalDesignation LDS ON E.LegalDesignationId = LDS.Id
								LEFT outer join[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode

												LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
												  Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
												  left join [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									left join [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId

									Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId

												LEFT JOIN org.Unit FU ON E.UnitID = FU.Id
												LEFT JOIN org.Division DV ON E.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON E.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON E.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON E.SubSectionID = SS.Id
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
									            ) egdsgg ON egdsgg.DesignationId=e.GivenDesignationId
									            AND egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID

									) EmpBasic ON EmpBasic.EmpSystemID = MMDSA.EmpSystemID
                                   LEFT JOIN 
													(
													 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
														FROM EmployeeInformation E   
																LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
																LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                                                                                                AND E.PlantId = gd.PlantId
																LEFT JOIN (
																			SELECT MAX(EffectiveDate) EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
																				FROM MST.LegalSalaryStructure 
																				WHERE EffectiveDate <= '" + fromDate + @"'
																			GROUP BY LegalSalaryGradeId, EmployeeLocationId 
																		  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
																LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                                                                                            AND SS.EmployeeLocationId = S.EmployeeLocationId 
                                                                                            AND SS.EffectiveDate = S.EffectiveDate
																LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 	
                                                                left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId	
														GROUP BY E.SystemId,LSG.UserName
													) MW ON MW.SystemId = EmpBasic.EmpSystemId
 
                                            where EmpBasic.PlantId ='" + plantId + @"' ";

                if (parameters.Count > 0)
                {
                    if (parameters.Keys.ElementAt(0) != "")
                    {
                        strSQL += @"and EmployeeCode IN(" + parameters["EmployeeCode"] + ")";

                    }
                }

                strSQL += @"Order by EmpBasic.EmployeeCode ";

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
        public void GetEmployeeInfoDetail(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string salaryProcessSystemId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool sa, bool ca, string userId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            var _wc = string.Empty;
            var wcSalaryProcessSystemIdStr = "";


            if (!string.IsNullOrEmpty(salaryProcessSystemId) && salaryProcessSystemId != "undefined" && salaryProcessSystemId != "null")
            {
                wcSalaryProcessSystemIdStr = "SystemID IN ('" + salaryProcessSystemId + @"')";
            }
            else
            {
                wcSalaryProcessSystemIdStr = @"SystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + fromDate + "') AND YearNo = Year('" + fromDate + "')  )";
            }
            string wcEmpStatus = " AND (1=0 ";

            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND (1=1 ";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='Regular'";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='MLV_PRE'";

                }
            }

            wcEmpStatus += ")";
            string wcPayrollGroup = "";
            if (sa == true || ca == true)
            {
                wcPayrollGroup = @"";
            }
            else
            {
                string inPayrollGroup = "' '";
                DataTable dtPayRollGrpEmpId = _sqlRepository.GetDataTable("SELECT employeeid FROM MST.PayrollGroupMaster WHERE PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"') AND PlantID = '" + plantId + @"'");
                DataTable dtNotPayRollGrpEmpId = _sqlRepository.GetDataTable(@"SELECT SystemId FROM EmployeeInformation E 
                    WHERE SystemId NOT IN (SELECT employeeid from MST.PayrollGroupMaster where PlantID = '" + plantId + @"')  AND E.PlantID = '" + plantId + @"'");

                if (dtPayRollGrpEmpId.Rows.Count > 0)
                {
                    for (int i = 0; i < dtPayRollGrpEmpId.Rows.Count; i++)
                    {
                        inPayrollGroup += ",'" + dtPayRollGrpEmpId.Rows[i]["employeeid"].ToString() + "'";
                    }
                    if (dtNotPayRollGrpEmpId.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtNotPayRollGrpEmpId.Rows.Count; i++)
                        {
                            inPayrollGroup += ",'" + dtNotPayRollGrpEmpId.Rows[i]["SystemId"].ToString() + "'";
                        }
                    }
                    wcPayrollGroup = @"AND EmpBasic.EmpSystemId  IN (" + inPayrollGroup + @")";
                }
                else
                {
                    wcPayrollGroup = @"";
                }

            }

            try
            {
                strSQL = @"SELECT EmpBasic.*,MMDSA.*,ISNULL(MW.Grade,'') Grade,ISNULL(MW.SalaryHeadValue,0) MinimumWage

                            FROM
                                    (
									 SELECT DISTINCT E.SystemID EmpSystemId,E.GroupID CompanyGroupId,E.CompanyId, E.EmployeeCode, E.EmployeeName, E.EmployeeStatus EmployeeStatusReal,E.EmployeeCurrentStatus
											, DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,
											'' UserGroupSystemID, E.PlantID, F.UserName PlantName, E.UnitID,
											FU.UserName UnitName, E.DivisionID, DV.UserName DivisionName, E.DepartmentID, DP.UserName DepartmentName
											,E.SectionID, S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID
											,EC.UserName EmpCategoryName , EC.WorkingDaysInAMonth--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo
                                            ,e.SalaryRuleMasterSystemID,Format(E.DOJ,'dd-MMM-yyyy') DOJ,Format(E.DOS,'dd-MMM-yyyy') DOS,Format(E.DOB,'dd-MMM-yyyy') DOB
											,ISNULL(LDS.UserName,'') LegalDesignation,ISNULL(E.NationalID,'') NationalID
											,ISNULL(Line.UserName,'') LineName
											,ISNULL(E.GenderID,'') Gender
                                            ,ISNULL(LSalGr.Code,'') GradeCode

											,ISNULL(PG.UserName,'') PayRollGroup
                                    , CASE WHEN ISNULL(SPM.SalaryProcFlag,'') = '' THEN 'Regular' ELSE SalaryProcFlag END EmployeeStatus
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
									,ISNULL(SPLD.BankAccNo,'') BankAccNo 
                                    ,CASE WHEN ISNULL(PO.IsDirect,0) = 0 THEN 'No' ELSE 'Yes' END IsDirect
                                    ,CASE WHEN ISNULL(PO.DirectManpowerCost,0) = 0 THEN 'No' ELSE 'Yes' END DirectManpowerCost

                                     FROM EmployeeInformation E
                                                LEFT OUTER JOIN SalaryProcChild SPC ON SPC.EmpInfoSystemID = E.SystemId
												LEFT OUTER JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID AND SPM.MonthNo = MONTH('" + fromDate + @"') AND SPM.YearNo = YEAR('" + fromDate + @"')
												Left JOin SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId = SPM.SystemId AND SPC.EmpInfoSystemID = SPLD.EmpSystemId
                                     
												LEFT JOIN ORG.Plant F ON E.PlantID = F.Id
												LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupId = DG.ID
												LEFT JOIN hkp.Designation DE ON E.GivenDesignationId = DE.Id
												LEFT JOIN hkp.LegalDesignation LDS ON E.LegalDesignationId = LDS.Id
								LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN [ORG].[Position] AS PO ON PO.Id = MB.PositionId

												LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
												  LEFT JOIN [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
												  LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									LEFT JOIN [HKP].[Bank] bb on bb.Id = SPLD.BankSystemID
                                    LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId

									LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                                LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LDS.Id and E.PlantId = LSGD.PlantId
                                                LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = LSGD.LegalSalaryGradeId  and E.PlantId = LSalGr.PlantId
												
												LEFT JOIN org.Unit FU ON E.UnitID = FU.Id
												LEFT JOIN org.Division DV ON E.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON E.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON E.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON E.SubSectionID = SS.Id

											left join HKP.EmployeeCategory EC ON  EC.Id = spld.EmployeeCategoryId
												
                                            Where SPC.SlrProcMstSystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo =  MONTH('" + fromDate + @"') AND YearNo =  YEAR('" + fromDate + @"')  )   

									) EmpBasic
                                   LEFT JOIN 
													(
													 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
														FROM EmployeeInformation E   
																LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
																LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                                                                                                AND E.PlantId = gd.PlantId
																LEFT JOIN (
																			SELECT MAX(EffectiveDate) EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
																				FROM MST.LegalSalaryStructure 
																				WHERE EffectiveDate <= '" + fromDate + @"'
																			GROUP BY LegalSalaryGradeId, EmployeeLocationId 
																		  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
																LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                                                                                            AND SS.EmployeeLocationId = S.EmployeeLocationId 
                                                                                            AND SS.EffectiveDate = S.EffectiveDate
																LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 	
                                                                left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId	
														GROUP BY E.SystemId,LSG.UserName
													) MW ON MW.SystemId = EmpBasic.EmpSystemId

                                    INNER JOIN
		                                    (
													SELECT EmpSystemID,MonthNo,YearNo, ISNULL(TotalProcDate,0) TotalProcDate,IsNULL(TotalPresent,0) TotalPresent,ISNULL(TotalLate,0) TotalLate,ISNULL(TotalAbsent,'') TotalAbsent
										,ISNULL(TotalLv,0) TotalLv
										,ISNULL(TotalMLv,0) TotalMLv,ISNULL(TotalCompAssignLv,0) TotalCompAssignLv,ISNULL(TotalWeekOff,0) +  ISNULL(TotalWeekOffHoliDay,0) TotalWeekOff, ISNULL(TotalWeekOffHoliDay,0) TotalWeekOffHoliDay
										,ISNULL(TotalOTHr,0) TotalOTHr,ISNULL(TotalNormalOTHr,0) TotalNormalOTHr,ISNULL(TotalExtraOTHr,0) TotalExtraOTHr,ISNULL(WeekOffOTHr,0) WeekOffOTHr
										,ISNULL(HoliDayOTHr,0) HoliDayOTHr,ISNULL(TotalLWP,0) TotalLWP,ISNULL(IsOTEntitled,0) IsOTEntitled,ISNULL(OTRate,0) OTRate,ISNULL(TotalHoliDay,0) TotalHoliDay
										  FROM SalaryProceAttdnData MMDSA where MMDSA.MonthNo = MONTH('" + fromDate + @"') AND
						                               MMDSA.YearNo = YEAR('" + fromDate + @"')
											) MMDSA ON EmpBasic.EmpSystemID = MMDSA.EmpSystemID 
                                            WHERE EmpBasic.CompanyGroupId = '" + companyGroupId + @"' AND EmpBasic.CompanyId ='" + companyId + @"' AND EmpBasic.PlantId ='" + plantId + @"' " + wcEmpStatus + @" " + wcPayrollGroup + @"";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            strSQL += @"and EmpBasic.EmpSystemId IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }



                strSQL += @"Order by EmpBasic.EmployeeCode ";

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

        public void GetEmployeeInfoDetailSalaryLogWise(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string salaryProcessSystemId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            string salaryProcessId = "";
            var _wc = string.Empty;
            var wcSalaryProcessSystemIdStr = "";


            if (!string.IsNullOrEmpty(salaryProcessSystemId) && salaryProcessSystemId != "undefined" && salaryProcessSystemId != "null")
            {
                wcSalaryProcessSystemIdStr = "SystemID IN ('" + salaryProcessSystemId + @"')";
            }
            else
            {
                wcSalaryProcessSystemIdStr = @"SystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + fromDate + "') AND YearNo = Year('" + fromDate + "')  )";


                string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo =  MONTH('" + fromDate + @"') AND YearNo =  YEAR('" + fromDate + @"')";

                DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);
                salaryProcessId = "''";
                for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
                {
                    salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
                }
            }
            string wcEmpStatus = " AND (1=0 ";

            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND (1=1 ";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='Regular'";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='MLV_PRE'";

                }
            }

            wcEmpStatus += ")";

            try
            {
                strSQL = @"SELECT EmpBasic.*,MMDSA.*,ISNULL(MW.Grade,'') Grade,ISNULL(MW.SalaryHeadValue,0) MinimumWage
                            FROM
                                    (
									SELECT DISTINCT E.SystemID EmpSystemId,ISNULL(EmployeeCodePreFix,'') EmployeeCodePreFix,ISNULL(EmployeeCodeNumeric,0) EmployeeCodeNumeric,E.GroupID CompanyGroupId,E.CompanyId, E.EmployeeCode, E.EmployeeName, E.EmployeeStatus EmployeeStatusReal,E.EmployeeCurrentStatus
											, DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,
											'' UserGroupSystemID,  F.Id PlantID, F.UserName PlantName, 
											FU.UserName UnitName,  DV.UserName DivisionName,  DP.UserName DepartmentName,
											 S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID,
											EC.UserName EmpCategoryName,EC.WorkingDaysInAMonth--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo
                                            ,e.SalaryRuleMasterSystemID,Format(E.DOJ,'dd-MMM-yyyy') DOJ,Format(E.DOS,'dd-MMM-yyyy') DOS,Format(E.DOB,'dd-MMM-yyyy') DOB
											,ISNULL(LDS.UserName,'') LegalDesignation,ISNULL(E.NationalID,'') NationalID
											,ISNULL(Line.UserName,'') LineName
											,ISNULL(E.GenderID,'') Gender
                                            ,ISNULL(LSalGr.Code,'') GradeCode
											,ISNULL(PG.UserName,'') PayRollGroup
                                    , CASE WHEN ISNULL(SPM.SalaryProcFlag,'') = '' THEN 'Regular' ELSE SalaryProcFlag END EmployeeStatus
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(SPLD.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                    ,ISNULL(spld.BankAccNo,'') BankAccNo
                                    ,ISNULL(spld.IFSCCode,'') IFSCCode
                                    ,CASE WHEN ISNULL(PO.IsDirect,0) = 0 THEN 'No' ELSE 'Yes' END IsDirect
                                    ,CASE WHEN ISNULL(PO.DirectManpowerCost,0) = 0 THEN 'No' ELSE 'Yes' END DirectManpowerCost

                                     FROM EmployeeInformation E
                                          Left JOIN (
                                    SELECT DISTINCT EmpInfoSystemID,SlrProcMstSystemID,PlantID ,m.Description,m.SalaryProcFlag
                                    FROM SalaryProcChild c
                                    JOIN SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID
                                    WHERE SlrProcMstSystemID IN(" + salaryProcessId + @") 
                                    ) SPM ON spm.EmpInfoSystemID=e.SystemId
									 JOIN SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId  IN(" + salaryProcessId + @") AND e.SystemId = SPLD.EmpSystemId  --SPLD.SalaryProcessId = SPM.SystemId AND SPC.EmpInfoSystemID = SPLD.EmpSystemId and SPLD.PlantId = '202022' 
                         
									 			LEFT JOIN ORG.Plant F ON SPLD.PlantID = F.Id
												LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupId = DG.ID
												LEFT JOIN hkp.Designation DE ON E.GivenDesignationId = DE.Id
												LEFT JOIN hkp.LegalDesignation LDS ON SPLD.LegalDesignationId = LDS.Id
								LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = SPLD.BudgetCode
								LEFT OUTER JOIN [ORG].[Position] AS PO ON PO.Id = MB.PositionId
                                LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId

												LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
												  LEFT JOIN [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
												  LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									LEFT JOIN [HKP].[Bank] bb on bb.Id = SPLD.BankSystemID
                                    LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId

									LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                                LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LDS.Id and E.PlantId = LSGD.PlantId
                                                LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = SPLD.LegalSalaryGradeId  --and SPLD.PlantId = LSalGr.PlantId
												
												LEFT JOIN org.Unit FU ON ENT.UnitID = FU.Id
												LEFT JOIN org.Division DV ON PO.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON PO.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON PO.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON PO.SubSectionID = SS.Id

												LEFT JOIN
                                                --hkp.EmployeeCategory EC ON E.EmployeeCategorySystemID = EC.Id
            --                                    (
            --                                    SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
												--LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
												--)EC ON EC.DesignationId=E.GivenDesignationId
												[HKP].[EmployeeCategory] EC ON EC.Id = SPLD.EmployeeCategoryId
											

                               
									) EmpBasic
                                   LEFT JOIN 
													(
													 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
														FROM EmployeeInformation E   
																LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
																LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                                                                                                AND E.PlantId = gd.PlantId
																LEFT JOIN (
																			SELECT MAX(EffectiveDate) EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
																				FROM MST.LegalSalaryStructure 
																				WHERE EffectiveDate <= '" + fromDate + @"'
																			GROUP BY LegalSalaryGradeId, EmployeeLocationId 
																		  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
																LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                                                                                            AND SS.EmployeeLocationId = S.EmployeeLocationId 
                                                                                            AND SS.EffectiveDate = S.EffectiveDate
																LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 	
                                                                left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId	
														GROUP BY E.SystemId,LSG.UserName
													) MW ON MW.SystemId = EmpBasic.EmpSystemId
                                    INNER JOIN
		                                    (
													SELECT EmpSystemID,MonthNo,YearNo, ISNULL(TotalProcDate,0) TotalProcDate,IsNULL(TotalPresent,0) TotalPresent,ISNULL(TotalLate,0) TotalLate,ISNULL(TotalAbsent,'') TotalAbsent
										,ISNULL(TotalLv,0) TotalLv
										,ISNULL(TotalMLv,0) TotalMLv,ISNULL(TotalCompAssignLv,0) TotalCompAssignLv,ISNULL(TotalWeekOff,0) +  ISNULL(TotalWeekOffHoliDay,0) TotalWeekOff, ISNULL(TotalWeekOffHoliDay,0) TotalWeekOffHoliDay
										,ISNULL(TotalOTHr,0) TotalOTHr,ISNULL(TotalNormalOTHr,0) TotalNormalOTHr,ISNULL(TotalExtraOTHr,0) TotalExtraOTHr,ISNULL(WeekOffOTHr,0) WeekOffOTHr
										,ISNULL(HoliDayOTHr,0) HoliDayOTHr,ISNULL(TotalLWP,0) TotalLWP,ISNULL(IsOTEntitled,0) IsOTEntitled,ISNULL(OTRate,0) OTRate,ISNULL(TotalHoliDay,0) TotalHoliDay
										  FROM SalaryProceAttdnData MMDSA where MMDSA.MonthNo = MONTH('" + fromDate + @"') AND
						                               MMDSA.YearNo = YEAR('" + fromDate + @"') AND MMDSA.PlantID = '" + plantId + @"' 
											) MMDSA ON EmpBasic.EmpSystemID = MMDSA.EmpSystemID 
                                            WHERE EmpBasic.CompanyGroupId = '" + companyGroupId + @"'  AND EmpBasic.PlantId ='" + plantId + @"' " + wcEmpStatus + @"";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            strSQL += @"and EmpBasic.EmpSystemId IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                strSQL += @"Order by EmpBasic.EmployeeCodePreFix,EmpBasic.EmployeeCodeNumeric ";

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

        public void GetEmployeeInfoDetailSalaryLogWiseOT(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string salaryProcessSystemId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, out DataSet dsRef, double budgetedOT)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            string salaryProcessId = "";
            var _wc = string.Empty;
            var wcSalaryProcessSystemIdStr = "";
            budgetedOT = budgetedOT * 60;

            if (!string.IsNullOrEmpty(salaryProcessSystemId) && salaryProcessSystemId != "undefined" && salaryProcessSystemId != "null")
            {
                wcSalaryProcessSystemIdStr = "SystemID IN ('" + salaryProcessSystemId + @"')";
            }
            else
            {
                wcSalaryProcessSystemIdStr = @"SystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + fromDate + "') AND YearNo = Year('" + fromDate + "')  )";


                string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo =  MONTH('" + fromDate + @"') AND YearNo =  YEAR('" + fromDate + @"')";

                DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);
                salaryProcessId = "''";
                for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
                {
                    salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
                }
            }
            string wcEmpStatus = " AND (1=0 ";

            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND (1=1 ";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='Regular'";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='MLV_PRE'";

                }
            }

            wcEmpStatus += ")";

            try
            {
                strSQL = @"SELECT EmpBasic.*,MMDSA.*,ISNULL(MW.Grade,'') Grade,ISNULL(MW.SalaryHeadValue,0) MinimumWage,BOT.*
                            FROM
                                    (
									SELECT DISTINCT E.SystemID EmpSystemId,ISNULL(EmployeeCodePreFix,'') EmployeeCodePreFix,ISNULL(EmployeeCodeNumeric,0) EmployeeCodeNumeric,E.GroupID CompanyGroupId,E.CompanyId, E.EmployeeCode, E.EmployeeName, E.EmployeeStatus EmployeeStatusReal,E.EmployeeCurrentStatus
											, DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,
											'' UserGroupSystemID,  F.Id PlantID, F.UserName PlantName, 
											FU.UserName UnitName,  DV.UserName DivisionName,  DP.UserName DepartmentName,
											 S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID,
											EC.UserName EmpCategoryName--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo
                                            ,egdsgg.GivenDesignationGroup,e.SalaryRuleMasterSystemID,Format(E.DOJ,'dd-MMM-yyyy') DOJ,Format(E.DOS,'dd-MMM-yyyy') DOS,Format(E.DOB,'dd-MMM-yyyy') DOB
											,ISNULL(LDS.UserName,'') LegalDesignation,ISNULL(E.NationalID,'') NationalID
											,ISNULL(Line.UserName,'') LineName
											,ISNULL(E.GenderID,'') Gender
                                            ,ISNULL(LSalGr.Code,'') GradeCode
											,ISNULL(PG.UserName,'') PayRollGroup
                                    , CASE WHEN ISNULL(SPM.SalaryProcFlag,'') = '' THEN 'Regular' ELSE SalaryProcFlag END EmployeeStatus
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(SPLD.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                    ,ISNULL(spld.BankAccNo,'') BankAccNo
                                    ,ISNULL(spld.IFSCCode,'') IFSCCode
                                    ,CASE WHEN ISNULL(PO.IsDirect,0) = 0 THEN 'No' ELSE 'Yes' END IsDirect
                                    ,CASE WHEN ISNULL(PO.DirectManpowerCost,0) = 0 THEN 'No' ELSE 'Yes' END DirectManpowerCost

                                     FROM EmployeeInformation E
                                    JOIN (
                                    SELECT DISTINCT EmpInfoSystemID,SlrProcMstSystemID,PlantID ,m.Description,m.SalaryProcFlag
                                    FROM SalaryProcChild c
                                    JOIN SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID
                                    WHERE SlrProcMstSystemID in (SELECT systemid FROM SalaryProcMaster WHERE MonthNo= MONTH('" + fromDate + @"') AND YearNo=YEAR('" + toDate + @"'))
                                    AND PlantID = '" + plantId + @"'
                                    ) SPM ON spm.EmpInfoSystemID=e.SystemId
									 JOIN SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId  IN(" + salaryProcessId + @") AND e.SystemId = SPLD.EmpSystemId  --SPLD.SalaryProcessId = SPM.SystemId AND SPC.EmpInfoSystemID = SPLD.EmpSystemId and SPLD.PlantId = '202022' 
                         
									 			LEFT JOIN ORG.Plant F ON SPLD.PlantID = F.Id
												LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupId = DG.ID
												LEFT JOIN hkp.Designation DE ON E.GivenDesignationId = DE.Id
												LEFT JOIN hkp.LegalDesignation LDS ON SPLD.LegalDesignationId = LDS.Id
								LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = SPLD.BudgetCode
								LEFT OUTER JOIN [ORG].[Position] AS PO ON PO.Id = MB.PositionId
                                LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId

												LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
												  LEFT JOIN [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
												  LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									LEFT JOIN [HKP].[Bank] bb on bb.Id = SPLD.BankSystemID
                                    LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId

									LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                                LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LDS.Id and E.PlantId = LSGD.PlantId
                                                LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = SPLD.LegalSalaryGradeId  --and SPLD.PlantId = LSalGr.PlantId
												
												LEFT JOIN org.Unit FU ON ENT.UnitID = FU.Id
												LEFT JOIN org.Division DV ON PO.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON PO.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON PO.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON PO.SubSectionID = SS.Id
												LEFT JOIN [HKP].[EmployeeCategory] EC ON EC.Id = SPLD.EmployeeCategoryId
												LEFT JOIN (SELECT dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									            ,dg.UserName GivenDesignationGroup
									            FROM MST.DesignationMaster dm
									            LEFT JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
									            ) egdsgg ON egdsgg.DesignationId=e.GivenDesignationId
									            AND egdsgg.EmployeeCategoryId=SPLD.EmployeeCategoryId

                                      --Where SPC.SlrProcMstSystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      --WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        --WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        --AND MonthNo =   MONTH('" + fromDate + @"') AND YearNo =  YEAR('" + fromDate + @"')   )   
									) EmpBasic
                                   LEFT JOIN 
													(
													 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
														FROM EmployeeInformation E   
																LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
																LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                                                                                                AND E.PlantId = gd.PlantId
																LEFT JOIN (
																			SELECT MAX(EffectiveDate) EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
																				FROM MST.LegalSalaryStructure 
																				WHERE EffectiveDate <= '" + fromDate + @"'
																			GROUP BY LegalSalaryGradeId, EmployeeLocationId 
																		  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
																LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                                                                                            AND SS.EmployeeLocationId = S.EmployeeLocationId 
                                                                                            AND SS.EffectiveDate = S.EffectiveDate
																LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 	
                                                                left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId	
														GROUP BY E.SystemId,LSG.UserName
													) MW ON MW.SystemId = EmpBasic.EmpSystemId
                                    INNER JOIN
		                                    (
													SELECT EmpSystemID,MonthNo,YearNo, ISNULL(TotalProcDate,0) TotalProcDate,IsNULL(TotalPresent,0) TotalPresent,ISNULL(TotalLate,0) TotalLate,ISNULL(TotalAbsent,'') TotalAbsent
										,ISNULL(TotalLv,0) TotalLv
										,ISNULL(TotalMLv,0) TotalMLv,ISNULL(TotalCompAssignLv,0) TotalCompAssignLv,ISNULL(TotalWeekOff,0) +  ISNULL(TotalWeekOffHoliDay,0) TotalWeekOff, ISNULL(TotalWeekOffHoliDay,0) TotalWeekOffHoliDay
										,ISNULL(TotalOTHr,0) TotalOTHr,ISNULL(TotalNormalOTHr,0) TotalNormalOTHr,ISNULL(TotalExtraOTHr,0) TotalExtraOTHr,ISNULL(WeekOffOTHr,0) WeekOffOTHr
										,ISNULL(HoliDayOTHr,0) HoliDayOTHr,ISNULL(TotalLWP,0) TotalLWP,ISNULL(IsOTEntitled,0) IsOTEntitled,ISNULL(OTRate,0) OTRate,ISNULL(TotalHoliDay,0) TotalHoliDay
										  FROM SalaryProceAttdnData MMDSA where MMDSA.MonthNo = MONTH('" + fromDate + @"') AND
						                               MMDSA.YearNo = YEAR('" + fromDate + @"') AND MMDSA.PlantID = '" + plantId + @"' 
											) MMDSA ON EmpBasic.EmpSystemID = MMDSA.EmpSystemID 
                                        INNER Join 
											(
											
												SELECT EmpSystemID,Sum(budgeted)/60 BudgetedOT,Sum(Nonbudgeted)/60 NonBudgetedOT, sum(TotalOTHr)/60 TotalOTHr from
												(
												SELECT EmpSystemID, WorkDate, ISNULL(TotalOTHr,0.00) TotalOTHr, 
												CASE WHEN ISNULL(TotalOTHr,0.00) <= " + budgetedOT + @"  THEN ISNULL(TotalOTHr,0.00)  ELSE " + budgetedOT + @"    END Budgeted
												,CASE WHEN ISNULL(TotalOTHr,0.00) >= " + budgetedOT + @"  THEN ISNULL(TotalOTHr,0.00) -" + budgetedOT + @"   ELSE TotalOTHr  END Nonbudgeted
												FROM FinalOt WHERE MONTH(WorkDate) =  month('" + fromDate + @"')
												
												) dd 
												
												GROUP BY EmpSystemID

											) BOT  ON BOT.EmpSystemID =EmpBasic.EmpSystemId
                                            WHERE EmpBasic.CompanyGroupId = '" + companyGroupId + @"'  AND EmpBasic.PlantId ='" + plantId + @"' " + wcEmpStatus + @"";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            strSQL += @"and EmpBasic.EmpSystemId IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                strSQL += @"Order by EmpBasic.EmployeeCodePreFix,EmpBasic.EmployeeCodeNumeric";

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

        public Dictionary<string, List<DataRow>> GetEmployeeSalaryInfoDetail(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string salaryProcessSystemId, string payRollGroup, Dictionary<string, string> parameters, out DataTable distinctSalaryHead)
        {
            string strSQL;
            DataSet dsRef = null;
            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
            distinctSalaryHead = new DataTable("Tmp");
            string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + fromDate + @"') AND YearNo = Year('" + fromDate + @"')";
            DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);

            string salaryProcessID = "''";
            for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
            {
                salaryProcessID += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
            }


            try
            {
                strSQL = @"SELECT EmpSlr.*,PSH.Sequence,ISNULL(crc.IsDecimalInDisb,0) IsDecimalInDisb,ISNULL(CRC.IntegerInDisb,1) IntegerInDisb,ISNULL(CRC.DecimalNo,0) DecimalNo FROM(SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
                                                    SPC.EmpInfoSystemID EmpSystemID, SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
                                                    SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
                                                    SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
                                                    CRE.Name AS PlantWiseExchangeCR, EXR.ToCurrencyBuying ExchangeRate, SPM.AmtDefinitionCurrencyID,
                                                    CR.Name AS AmtDefinitionCurrency, SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect, ISNULL(SH.IsCTCComponent,0) IsCTCComponent, ISNULL(SH.IsGrossComponent,0) IsGrossComponent
                                                    , sh.SalaryHead, sh.HeadCategory, sh.HeadType, ISNULL(SH.PartOfNetPay,0) PartOfNetPay

                                     FROM SalaryProcChild SPC

                                        left JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID



                                                        LEFT JOIN SalaryHead sh on sh.SalaryHeadID= spc.SalaryHeadID


                                                        LEFT JOIN scs.Currency CR ON SPM.AmtDefinitionCurrencyID = CR.Id

                                                        LEFT JOIN (
                                                                   SELECT* FROM ExchangerateDateWiseForHR

                                                                   WHERE FromDate IN (SELECT MAX(FromDate) FromDate FROM SalaryProcMaster


                                                                                                            WHERE SystemID IN(" + salaryProcessID + @")
																  )) EXR ON SPM.AmtDefinitionCurrencyID = EXR.FromCurrencyCode

                                                                                            AND SPC.PlantID = Exr.PlantID

                                                        LEFT JOIN SCS.Currency CRE ON EXR.FromCurrencyCode = CRE.Id

                                                        where isnull(SPC.SlrProcMstSystemID,'')  IN(" + salaryProcessID + @")) EmpSlr--ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID AND EmpBasic.PlantID = EmpSlr.PlantID

                                            Inner join EmployeeInformation EEI ON EEI.SystemId = EmpSlr.EmpSystemID

                                         LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EEI.SalaryRuleMasterSystemID

                                        LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID  AND SRG.SalaryHeadID = EmpSlr.SalaryHeadID
                                        LEFT JOIN(SELECT* FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId = '" + plantId + @"') PSH
                                                                       ON PSH.SalaryHeadId = EmpSlr.SalaryHeadID
                                        LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = EmpSlr.SalaryHeadID

                                                WHERE EEI.GroupID = '" + companyGroupId + @"' AND  EmpSlr.PlantId = '" + plantId + @"'";

                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            strSQL += @"AND EmpSlr.EmpSystemID IN(" + parameters["EmpSystemId"] + ")";

                        }
                    }
                }
                catch (Exception)
                {
                }
                strSQL += "ORDER BY EmpSystemId ";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSQL, out dsRef);

                distinctSalaryHead = dsRef.Tables[0].DefaultView.ToTable(true, "SalaryHeadID", "SalaryHead", "HeadType", "Sequence", "HeadCategory", "IntegerInDisb", "DecimalNo", "PartOfNetPay", "IsCTCComponent", "IsGrossComponent");
                distinctSalaryHead.DefaultView.Sort = "Sequence";
                distinctSalaryHead = distinctSalaryHead.DefaultView.ToTable();

                DataTable dt = dsRef.Tables[0];
                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (empId != dt.Rows[i]["EmpSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        dicBonus.Add(dt.Rows[i]["EmpSystemID"].ToString(), _data);
                    }
                    _data.Add(dt.Rows[i]);

                    empId = dt.Rows[i]["EmpSystemID"].ToString();
                }

                return dicBonus;


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

        public Dictionary<string, List<DataRow>> GetEmployeeSalaryInfoDetailPaySlip(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string languageId, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, out DataTable distinctSalaryHead)
        {
            string strSQL;
            DataSet dsRef = null;
            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
            distinctSalaryHead = new DataTable("Tmp");
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
                        wcEmpStatus += " OR SalaryProcFlag ='Regular' ";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                    }
                }
                DataTable dtslProcId = _sqlRepository.GetDataTable(@" SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + toDate + @"') AND YearNo = Year('" + toDate + @"') ");
                string inSalaryProcParam = "' '";

                for (int i = 0; i < dtslProcId.Rows.Count; i++)
                {
                    inSalaryProcParam += ",'" + dtslProcId.Rows[i]["SystemID"].ToString() + "'";
                }

                wcEmpStatus += ")";

                strSQL = @"SELECT EmpSlr.*,ISNULL(PSH.Sequence,99) Sequence,ISNULL(crc.IsDecimalInDisb,0) IsDecimalInDisb,ISNULL(CRC.IntegerInDisb,1) IntegerInDisb,ISNULL(CRC.DecimalNo,0) DecimalNo FROM(SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
                                                    SPC.EmpInfoSystemID , SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
                                                    SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
                                                    SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
                                                    CRE.Name AS PlantWiseExchangeCR, EXR.ToCurrencyBuying ExchangeRate, SPM.AmtDefinitionCurrencyID,
                                                    CR.Name AS AmtDefinitionCurrency, SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect, ISNULL(SH.IsCTCComponent,0) IsCTCComponent, ISNULL(SH.IsGrossComponent,0) IsGrossComponent
                                                    , sh.SalaryHead,ISNULL(ISNULL(BSH.Name,SH.SalaryHead),'') SalaryHeadLocal, sh.HeadCategory, sh.HeadType, ISNULL(SH.PartOfNetPay,0) PartOfNetPay
													, Case when Isnull(SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
                                     FROM SalaryProcChild SPC

                                        left JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
                                                        LEFT JOIN SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
                                                        LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE SalaryHeadId IS NOT NULL AND LanguageId = '" + languageId + @"') AS BSH ON BSH.SalaryHeadId = sh.SalaryHeadID --BanglaSalaryHead
                                                        LEFT JOIN scs.Currency CR ON SPM.AmtDefinitionCurrencyID = CR.Id
                                                        LEFT JOIN (
                                                                   SELECT* FROM ExchangerateDateWiseForHR
                                                                   WHERE FromDate IN (SELECT MAX(FromDate) FromDate FROM SalaryProcMaster
                                                                                                           WHERE SystemID IN(" + inSalaryProcParam + @")
																  )) EXR ON SPM.AmtDefinitionCurrencyID = EXR.FromCurrencyCode
                                                                                            AND SPC.PlantID = Exr.PlantID
                                                        LEFT JOIN SCS.Currency CRE ON EXR.FromCurrencyCode = CRE.Id

                                                        WHERE ISNULL(SPC.SlrProcMstSystemID,'')  IN(" + inSalaryProcParam + @")) EmpSlr--ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID AND EmpBasic.PlantID = EmpSlr.PlantID

                                            Inner join EmployeeInformation EEI ON EEI.SystemId = EmpSlr.EmpInfoSystemID

                                         LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EEI.SalaryRuleMasterSystemID

                                        LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID  AND SRG.SalaryHeadID = EmpSlr.SalaryHeadID
                                        LEFT JOIN(SELECT* FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId = '" + plantId + @"') PSH
                                                                       ON PSH.SalaryHeadId = EmpSlr.SalaryHeadID
                                        LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = EmpSlr.SalaryHeadID

                                                WHERE EEI.GroupID = '" + companyGroupId + @"' AND  EmpSlr.PlantId = '" + plantId + @"'";
                if (parameters.Count > 0)
                {
                    if (parameters.Keys.ElementAt(0) != "")
                    {
                        strSQL += @" AND EmpSlr.EmpInfoSystemID IN(" + parameters["EmpSystemId"] + ")";
                    }
                }

                strSQL += "ORDER BY EmpSlr.EmpInfoSystemID,Sequence";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSQL, out dsRef);

                distinctSalaryHead = dsRef.Tables[0].DefaultView.ToTable(true, "SalaryHeadID", "SalaryHead", "SalaryHeadLocal", "HeadType", "Sequence", "HeadCategory", "IntegerInDisb", "DecimalNo", "IsGrossComponent", "IsCTCComponent", "PartOfNetPay");
                distinctSalaryHead.DefaultView.Sort = "Sequence";
                distinctSalaryHead = distinctSalaryHead.DefaultView.ToTable();

                DataTable dt = dsRef.Tables[0];
                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (empId != dt.Rows[i]["EmpInfoSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        dicBonus.Add(dt.Rows[i]["EmpInfoSystemID"].ToString(), _data);
                    }
                    _data.Add(dt.Rows[i]);

                    empId = dt.Rows[i]["EmpInfoSystemID"].ToString();
                }
                return dicBonus;
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


        public Dictionary<string, string> GetEmployeePFESIC(string profileType)
        {
            string strSQL;
            DataTable dtTable = null;
            Dictionary<string, string> dicPF = new Dictionary<string, string>();
            try
            {
                strSQL = @"SELECT ISNULL(ed.DocNumber,'') DocNumber,ED.EmpSystemID from 
												EmployeeDocument ED 											
												INNER JOIN (SELECT * FROM HKP.ComplianceDocument WHERE ProfileType = '" + profileType + @"') CD ON CD.Id = ED.ComplianceDocumentId ";

                dtTable = _sqlRepository.GetDataTable(strSQL);

                for (int i = 0; i < dtTable.Rows.Count; i++)
                {
                    if (dicPF.ContainsKey(dtTable.Rows[i]["EmpSystemID"].ToString()) == false)
                        dicPF.Add(dtTable.Rows[i]["EmpSystemID"].ToString(), dtTable.Rows[i]["DocNumber"].ToString());
                }
                return dicPF;
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


        public Dictionary<string, List<DataRow>> GetEmployeeSalaryInfoCompliance(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string salaryProcessSystemId, string payRollGroup, Dictionary<string, string> parameters)
        {// For Sadma Compliance (Night Bill and Extra OT)
            string strSQL;
            DataSet dsRef = null;
            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();

            try
            {
                strSQL = @"select 
                            e.EmployeeCode,e.EmployeeName ,e.SystemId EmpSystemID , e.EmployeeCodePreFix ,e.EmployeeCodeNumeric
                            ,ld.UserName as Designation, d.UserName as Department,se.UserName as Section,ss.UserName as SubSection,l.UserName AS Line
                            ,ad.UserName as AllowanceType,format(e.DOJ,'dd-MMM-yyyy')as DOJ,format(e.DOC,'dd-MMM-yyyy')as DOC
                            ,format(e.DOS,'dd-MMM-yyyy')as DOS  ,eu.UserName AS Unit
                            ,x.Quantity,x.Amount,x.AllowanceDailyId,x.Rate
                            FROM EmployeeInformation as e 
                            INNER JOIN (SELECT SUM(dat.Quantity) as Quantity,sum(dat.Amount) as Amount,dat.Rate
							,dat.EmpSystemId,dat.AllowanceDailyId
							from [dbo].[DailyAllowanceTransaction] as dat 
							where dat.WorkDate between '" + fromDate + @"' and '" + toDate + @"'
							Group by  EmpSystemId,AllowanceDailyId,Rate) x on e.SystemId=x.EmpSystemId
                            left join [HKP].[AllowanceDaily] as ad on ad.Id=x.AllowanceDailyId
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
                             e.PlantId='" + plantId + @"' and ad.IsVoucherPayment=1 ";

                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            strSQL += @"AND EmpSystemID IN(" + parameters["EmpSystemId"] + ")";

                        }
                    }
                }
                catch (Exception)
                {
                }
                strSQL += " ORDER BY  EmployeeCodePreFix ,EmployeeCodeNumeric";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSQL, out dsRef);



                DataTable dt = dsRef.Tables[0];
                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (empId != dt.Rows[i]["EmpSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        dicBonus.Add(dt.Rows[i]["EmpSystemID"].ToString(), _data);
                    }
                    _data.Add(dt.Rows[i]);

                    empId = dt.Rows[i]["EmpSystemID"].ToString();
                }

                return dicBonus;


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

        public void GetSalaryInfoSlrProcIDWiseCombinedForPaySlip(string plantId, string fromDate, string toDate, string salaryProcessSystemId, string payRollGroup, Dictionary<string, string> parameters, string languageId, bool isActive, bool isSeperated, bool isMaternity, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            var _wc = string.Empty;
            string salaryProcessId = "";
            var wcSalaryProcessSystemIdStr = "";

            if (!string.IsNullOrEmpty(salaryProcessSystemId) && salaryProcessSystemId != "undefined" && salaryProcessSystemId != "null")
            {
                wcSalaryProcessSystemIdStr = "SystemID IN ('" + salaryProcessSystemId + @"')";
            }
            else
            {
                wcSalaryProcessSystemIdStr = @"SystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + fromDate + "') AND YearNo = Year('" + fromDate + "'))";


                string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo =  MONTH('" + fromDate + @"') AND YearNo =  YEAR('" + fromDate + @"')";

                DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);
                salaryProcessId = "''";
                for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
                {
                    salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
                }
            }
            string wcEmpStatus = " Where (1=0 ";

            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " Where (1=1 ";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus += " OR EmpSlr.SalaryProcFlag ='Regular'";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus += " OR EmpSlr.SalaryProcFlag ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    wcEmpStatus += " OR EmpSlr.SalaryProcFlag ='MLV_PRE'";

                }
            }

            wcEmpStatus += ")";


            try
            {
                strSQL = @"SELECT EmpSlr.EmpInfoSystemID,EmpBasic.SystemID, EmpBasic.EmployeeCode, EmpBasic.EmployeeName, Replace(Convert(varchar(11),EmpBasic.DOJ,106),' ','-') DOJ,DOB,
                                EmpBasic.EmployeeStatus, EmpBasic.UnitID, EmpBasic.UnitName Unit, EmpBasic.DivisionID, EmpBasic.DivisionName Division,
                                EmpBasic.DepartmentID, EmpBasic.DepartmentName Department, EmpBasic.SectionID, EmpBasic.SectionName Section, EmpBasic.SubSectionID,EmpBasic.UnitName,EmpBasic.SectionName,
                                EmpBasic.SubSectionName SubSection , EmpBasic.EmployeeCategorySystemID, 
                                EmpBasic.DesignationSystemID,   EmpBasic.PlantName Plant,
								EmpBasic.EmpCategoryName EmployeeCategory,EmpBasic.DOJ,EmpBasic.DOS,EmpBasic.legalDesignation,ISNULL(EmpBasic.PaymentMode,'') PaymentMode,
                                BankName, BankNameFull BankNameFull,  BankAccNo,ISNULL(EmpBasic.NationalID,'') NationalID,PF.UANNo,ESIC.ESICNo,  
								(ISNULL(MMDSA.TotalPresent, 0) + ISNULL(MMDSA.TotalLate, 0))  PresentDays,ISNULL(MMDSA.TotalProcDate, 0) TotalProcDate,
								ISNULL(MMDSA.AbsentDays, 0) AbsentDays, ISNULL(MMDSA.TotalHoliDay, 0) HoliDay, ISNULL(MMDSA.TotalWeekOff, 0) WeekOff,
								(ISNULL(MMDSA.TotalLv, 0) + ISNULL(MMDSA.TotalMLv, 0)) LeaveDays,ISNULL(MMDSA.TotalLWP,0) TotalLWP, EmpSlr.SlrProcChdSysID, EmpSlr.SlrProcMstSystemID, EmpSlr.SalaryProcID,
                                EmpSlr.PlantID, EmpSlr.FromDate, EmpSlr.ToDate, EmpSlr.MonthNo, EmpSlr.YearNo, EmpSlr.PayAbleShSystemID,
                                EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount,
                                EmpSlr.DisbusmentCurrencyID, EmpSlr.DisbusmentAmount, EmpSlr.AcltExcDisbSlrHDID, EmpSlr.AcltExcDisbSlrHDAmt,
                                EmpSlr.PlantWiseExchangeCR, EmpSlr.ExchangeRate, EmpSlr.AmtDefinitionCurrencyID, EmpSlr.AmtDefinitionCurrency,
                                EmpSlr.AmtDefinitionCurrencyRate, EmpSlr.IsNetPayEffect,EmpCategoryName,EmpSlr.HeadType,EmpSlr.IsCTCComponent,EmpSlr.IsGrossComponent
                                ,EmpSlr.SalaryHead,EmpSlr.HeadCategory,EmpSlr.HeadType,ISNULL(psh.Sequence,'99')  Sequence,CRC.IntegerInDisb,CRC.DecimalNo,CRC.IsDecimalInDisb
                                ,EmpSlr.SalaryProcFlag, ISNULL(MW.SalaryHeadValue,0) MinimumWage,EmpBasic.Grade, EmpSlr.PartOfNetPay ,ISNULL(EmpSlr.SalaryHeadLocal,EmpSlr.SalaryHead) SalaryHeadLocal  
                            FROM
                                    (
									 SELECT E.SystemID , E.EmployeeCode, E.EmployeeName, E.EmployeeStatus,SPLD.PaymentMode,Bank.ShortName BankName,Bank.UserName BankNameFull,SPLD.BankAccNo
											, E.DesignationSystemID
											,'' UserGroupSystemID, E.PlantID, F.UserName PlantName, E.UnitID,
											FU.UserName UnitName, E.DivisionID, DV.UserName DivisionName, E.DepartmentID, DP.UserName DepartmentName,
											E.SectionID, S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID,
											EC.UserName EmpCategoryName,LSalGr.Code Grade
                                            ,e.SalaryRuleMasterSystemID,Format(E.DOJ,'dd-MMM-yyyy') DOJ,Format(E.DOS,'dd-MMM-yyyy') DOS,Format(E.DOB,'dd-MMM-yyyy') DOB
											,ISNULL(LDS.UserName,'') LegalDesignation,ISNULL(E.NationalID,'') NationalID
                                     FROM EmployeeInformation E
                                        LEFT OUTER JOIN SalaryProcChild SPC ON SPC.EmpInfoSystemID = E.SystemId
                                    LEFT OUTER JOIN SalaryProcMaster SPM ON SPM.SystemID IN (" + salaryProcessId + @") 

                                    INNER JOin SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId = SPM.SystemId AND SPC.EmpInfoSystemID = SPLD.EmpSystemId
                         
												LEFT JOIN ORG.Plant F ON SPLD.PlantID = F.Id
												LEFT JOIN hkp.LegalDesignation LDS ON SPLD.LegalDesignationId = LDS.Id
												LEFT JOIN EmployeeBankInfo EBI ON E.SystemId = EBI.EmpSystemID	
												LEFT JOIN HKP.Bank Bank ON Bank.Id = SPLD.BankSystemID
								LEFT JOIN HKP.BankBranch BankBr ON BankBr.Id = SPLD.BankBranchId	
									     LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = SPLD.LegalSalaryGradeId 
												LEFT JOIN  hkp.EmployeeCategory EC ON SPLD.EmployeeCategoryId = EC.Id
												LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=SPLD.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department]  DP ON DP.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division]  DV ON DV.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] S ON S.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] SS ON SS.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] FU ON FU.Id = EN.UnitId                                    
                                    LEFT OUTER JOIN ORG.Line eL on eL.id=MPB.LineId
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
													CR.Name AS AmtDefinitionCurrency, SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect,SH.IsCTCComponent,SH.IsGrossComponent
                                                     , CASE WHEN ISNULL(SPM.SalaryProcFlag,'') = '' THEN 'Regular' ELSE SalaryProcFlag END SalaryProcFlag,sh.SalaryHead,BSH.Name SalaryHeadLocal,sh.HeadCategory,sh.HeadType,SH.PartOfNetPay
                                                    
											 FROM SalaryProcChild SPC
																INNER JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
																							AND SPM.SystemID IN(" + salaryProcessId + @")
                                                        LEFT JOIN SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
														--LEFT JOIN (select * from [MST].[PlantSalaryHeadSequence] where PlantId='" + plantId + @"' ) psh
																		-- psh.SalaryHeadId=spc.SalaryHeadID

														LEFT JOIN scs.Currency CR ON SPM.AmtDefinitionCurrencyID = CR.Id
														LEFT JOIN (
																   SELECT * FROM ExchangerateDateWiseForHR
																   WHERE FromDate IN (   SELECT MAX(FromDate) FromDate FROM SalaryProcMaster
																															   
																											WHERE SPM.SystemID IN(" + salaryProcessId + @")
                                                                                    )
																  ) EXR ON SPM.AmtDefinitionCurrencyID = EXR.FromCurrencyCode
																							AND SPC.PlantID = Exr.PlantID
														LEFT JOIN SCS.Currency CRE ON EXR.FromCurrencyCode = CRE.Id
                                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE SalaryHeadId IS NOT NULL AND LanguageId = '" + languageId + @"') AS BSH ON BSH.SalaryHeadId = sh.SalaryHeadID
											) EmpSlr ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID AND EmpBasic.PlantID = EmpSlr.PlantID
                             	LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EmpBasic.SalaryRuleMasterSystemID 
										--LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID	AND SRG.SalaryHeadID = EmpSlr.SalaryHeadID									
                                        LEFT JOIN (SELECT * FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId='" + plantId + @"' ) PSH
																		ON PSH.SalaryHeadId=EmpSlr.SalaryHeadID
                                        LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = EmpSlr.SalaryHeadID
                                        	 LEFT JOIN  
												( select ed.DocNumber UANNo,ED.EmpSystemID from 
												EmployeeDocument ED 
												inner JOIN (select * from HKP.ComplianceDocument where ProfileType = 'PF') CD ON CD.Id = ED.ComplianceDocumentId 
												) pf on EmpBasic.SystemId = pf.EmpSystemID

												left join
												( select ed.DocNumber ESICNo,ED.EmpSystemID from 
												EmployeeDocument ED 
												inner JOIN (select * from HKP.ComplianceDocument where ProfileType = 'ESIC') CD ON CD.Id = ED.ComplianceDocumentId 
												) esic on EmpBasic.SystemId = esic.EmpSystemID

                                    LEFT JOIN
		                                    (
                                            SELECT EmpSystemID,MonthNo, YearNo,ISNULL(TotalProcDate,0) TotalProcDate, ISNULL(TotalPresent,0) TotalPresent, 
                                            ISNULL(TotalPresent,0)+ISNULL(TotalLate,0) TotalPresentLate
                                            ,ISNULL(TotalLate,0) TotalLate
                                            ,ISNULL(TotalAbsent,0) TotalAbsent, ISNULL(TotalAbsent,0) - ISNULL(TotalLWP,0) AbsentDays 
                                            ,ISNULL(TotalLWP,0) TotalLWP,ISNULL(TotalWeekOff,0) + ISNULL(TotalWeekOffHoliDay,0) TotalWeekOffPlusWeekOffHoliDay,
                                            ISNULL(TotalLv,0) TotalLv, ISNULL(TotalMLv,0) TotalMLv, ISNULL(TotalCompAssignLv,0) TotalCompAssignLv, ISNULL(TotalWeekOff,0) TotalWeekOff, ISNULL(TotalHoliDay,0) TotalHoliDay,
                                            													ISNULL(TotalWeekOffHoliDay,0) TotalWeekOffHoliDay, ISNULL(TotalOTHr,0) TotalOTHr, ISNULL(TotalNormalOTHr,0) TotalNormalOTHr, ISNULL(TotalExtraOTHr,0) TotalExtraOTHr
                                             FROM SalaryProceAttdnData

											 --SELECT EmpSystemID, MonthNo, YearNo, TotalProcDate, TotalPresent, TotalLate,TotalLWP,
											--		TotalAbsent AbsentDays, TotalLv, TotalMLv, TotalCompAssignLv, TotalWeekOff, TotalHoliDay,
											--		TotalWeekOffHoliDay, TotalOTHr, TotalNormalOTHr, TotalExtraOTHr
				                             -- FROM AttdnDataMonthlySummary
											) MMDSA ON EmpSlr.EmpInfoSystemID = MMDSA.EmpSystemID AND EmpSlr.MonthNo = MMDSA.MonthNo AND
						                               EmpSlr.YearNo = MMDSA.YearNo
                                   --     WHERE (EmpBasic.EmployeeStatus != 'Separated' OR ISNULL(EmpBasic.DOS,'') = ''  OR COnvert(date,EmpBasic.DOS) >= Convert(Date,'" + fromDate + "')) AND COnvert(date,EmpBasic.DOJ) <=  Convert(Date,'" + toDate + "')" +
                                    "--" + _wc + @" 
                            " + wcEmpStatus + @"";

                if (parameters.Count > 0)
                {
                    if (parameters.Keys.ElementAt(0) != "")
                    {
                        strSQL += @"and EmpBasic.SystemID IN(" + parameters["EmpSystemId"] + ")";

                    }
                }

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
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
        #endregion

        #region Employee Salary Payable

        public void GetEmployeeInfoDetailSalaryLogWiseDirectInDirect(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string salaryProcessSystemId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, out DataSet dsRef, bool IsDirectInDirect)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            string salaryProcessId = "";
            var _wc = string.Empty;
            var wcSalaryProcessSystemIdStr = "";


            if (!string.IsNullOrEmpty(salaryProcessSystemId) && salaryProcessSystemId != "undefined" && salaryProcessSystemId != "null")
            {
                wcSalaryProcessSystemIdStr = "SystemID IN ('" + salaryProcessSystemId + @"')";
            }
            else
            {
                wcSalaryProcessSystemIdStr = @"SystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + fromDate + "') AND YearNo = Year('" + fromDate + "')  )";


                string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo =  MONTH('" + fromDate + @"') AND YearNo =  YEAR('" + fromDate + @"')";

                DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);
                salaryProcessId = "''";
                for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
                {
                    salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
                }
            }
            string wcEmpStatus = " AND (1=0 ";

            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND (1=1 ";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='Regular'";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='MLV_PRE'";

                }
            }

            wcEmpStatus += ")";

            try
            {
                strSQL = @"SELECT EmpBasic.*,MMDSA.*,ISNULL(MW.Grade,'') Grade,ISNULL(MW.SalaryHeadValue,0) MinimumWage
                            FROM
                                    (
									SELECT DISTINCT E.SystemID EmpSystemId,ISNULL(EmployeeCodePreFix,'') EmployeeCodePreFix,ISNULL(EmployeeCodeNumeric,0) EmployeeCodeNumeric,E.GroupID CompanyGroupId,E.CompanyId, E.EmployeeCode, E.EmployeeName, E.EmployeeStatus EmployeeStatusReal,E.EmployeeCurrentStatus
											, DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,
											'' UserGroupSystemID,  F.Id PlantID, F.UserName PlantName, 
											FU.UserName UnitName,  DV.UserName DivisionName,  DP.UserName DepartmentName,
											 S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID,
											EC.UserName EmpCategoryName,EC.WorkingDaysInAMonth--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo
                                            ,e.SalaryRuleMasterSystemID,Format(E.DOJ,'dd-MMM-yyyy') DOJ,Format(E.DOS,'dd-MMM-yyyy') DOS,Format(E.DOB,'dd-MMM-yyyy') DOB
											,ISNULL(LDS.UserName,'') LegalDesignation,ISNULL(E.NationalID,'') NationalID
											,ISNULL(Line.UserName,'') LineName
											,ISNULL(E.GenderID,'') Gender
                                            ,ISNULL(LSalGr.Code,'') GradeCode
											,ISNULL(PG.UserName,'') PayRollGroup
                                    , CASE WHEN ISNULL(SPM.SalaryProcFlag,'') = '' THEN 'Regular' ELSE SalaryProcFlag END EmployeeStatus
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(SPLD.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                    ,ISNULL(spld.BankAccNo,'') BankAccNo
                                    ,ISNULL(spld.IFSCCode,'') IFSCCode
                                    ,CASE WHEN ISNULL(PO.IsDirect,0) = 0 THEN 'No' ELSE 'Yes' END IsDirect
                                    ,CASE WHEN ISNULL(PO.DirectManpowerCost,0) = 0 THEN 'No' ELSE 'Yes' END DirectManpowerCost

                                     FROM EmployeeInformation E
                                          Left JOIN (
                                    SELECT DISTINCT EmpInfoSystemID,SlrProcMstSystemID,PlantID ,m.Description,m.SalaryProcFlag
                                    FROM SalaryProcChild c
                                    JOIN SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID
                                    WHERE SlrProcMstSystemID IN(" + salaryProcessId + @") 
                                    ) SPM ON spm.EmpInfoSystemID=e.SystemId
									 JOIN SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId  IN(" + salaryProcessId + @") AND e.SystemId = SPLD.EmpSystemId  --SPLD.SalaryProcessId = SPM.SystemId AND SPC.EmpInfoSystemID = SPLD.EmpSystemId and SPLD.PlantId = '202022' 
                         
									 			LEFT JOIN ORG.Plant F ON SPLD.PlantID = F.Id
												LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupId = DG.ID
												LEFT JOIN hkp.Designation DE ON E.GivenDesignationId = DE.Id
												LEFT JOIN hkp.LegalDesignation LDS ON SPLD.LegalDesignationId = LDS.Id
								LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = SPLD.BudgetCode
								LEFT OUTER JOIN [ORG].[Position] AS PO ON PO.Id = MB.PositionId
                                LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId

												LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
												  LEFT JOIN [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
												  LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									LEFT JOIN [HKP].[Bank] bb on bb.Id = SPLD.BankSystemID
                                    LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId

									LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                                LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LDS.Id and E.PlantId = LSGD.PlantId
                                                LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = SPLD.LegalSalaryGradeId  --and SPLD.PlantId = LSalGr.PlantId
												
												LEFT JOIN org.Unit FU ON ENT.UnitID = FU.Id
												LEFT JOIN org.Division DV ON PO.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON PO.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON PO.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON PO.SubSectionID = SS.Id

												LEFT JOIN
                                                --hkp.EmployeeCategory EC ON E.EmployeeCategorySystemID = EC.Id
            --                                    (
            --                                    SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
												--LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
												--)EC ON EC.DesignationId=E.GivenDesignationId
												[HKP].[EmployeeCategory] EC ON EC.Id = SPLD.EmployeeCategoryId
												where PO.DirectManpowerCost= '" + IsDirectInDirect + @"'

                               
									) EmpBasic
                                   LEFT JOIN 
													(
													 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
														FROM EmployeeInformation E   
																LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
																LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                                                                                                AND E.PlantId = gd.PlantId
																LEFT JOIN (
																			SELECT MAX(EffectiveDate) EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
																				FROM MST.LegalSalaryStructure 
																				WHERE EffectiveDate <= '" + fromDate + @"'
																			GROUP BY LegalSalaryGradeId, EmployeeLocationId 
																		  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
																LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                                                                                            AND SS.EmployeeLocationId = S.EmployeeLocationId 
                                                                                            AND SS.EffectiveDate = S.EffectiveDate
																LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 	
                                                                left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId	
														GROUP BY E.SystemId,LSG.UserName
													) MW ON MW.SystemId = EmpBasic.EmpSystemId
                                    INNER JOIN
		                                    (
													SELECT EmpSystemID,MonthNo,YearNo, ISNULL(TotalProcDate,0) TotalProcDate,IsNULL(TotalPresent,0) TotalPresent,ISNULL(TotalLate,0) TotalLate,ISNULL(TotalAbsent,'') TotalAbsent
										,ISNULL(TotalLv,0) TotalLv
										,ISNULL(TotalMLv,0) TotalMLv,ISNULL(TotalCompAssignLv,0) TotalCompAssignLv,ISNULL(TotalWeekOff,0) +  ISNULL(TotalWeekOffHoliDay,0) TotalWeekOff, ISNULL(TotalWeekOffHoliDay,0) TotalWeekOffHoliDay
										,ISNULL(TotalOTHr,0) TotalOTHr,ISNULL(TotalNormalOTHr,0) TotalNormalOTHr,ISNULL(TotalExtraOTHr,0) TotalExtraOTHr,ISNULL(WeekOffOTHr,0) WeekOffOTHr
										,ISNULL(HoliDayOTHr,0) HoliDayOTHr,ISNULL(TotalLWP,0) TotalLWP,ISNULL(IsOTEntitled,0) IsOTEntitled,ISNULL(OTRate,0) OTRate,ISNULL(TotalHoliDay,0) TotalHoliDay
										  FROM SalaryProceAttdnData MMDSA where MMDSA.MonthNo = MONTH('" + fromDate + @"') AND
						                               MMDSA.YearNo = YEAR('" + fromDate + @"') AND MMDSA.PlantID = '" + plantId + @"' 
											) MMDSA ON EmpBasic.EmpSystemID = MMDSA.EmpSystemID 
                                            WHERE EmpBasic.CompanyGroupId = '" + companyGroupId + @"'  AND EmpBasic.PlantId ='" + plantId + @"' " + wcEmpStatus + @"";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            strSQL += @"and EmpBasic.EmpSystemId IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                strSQL += @"Order by EmpBasic.EmployeeCodePreFix,EmpBasic.EmployeeCodeNumeric ";

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


        public Dictionary<string, List<DataRow>> GetEmployeeSalaryInfoDetailDirectInDirect(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string salaryProcessSystemId, string payRollGroup, Dictionary<string, string> parameters, out DataTable distinctSalaryHead)
        {
            string strSQL;
            DataSet dsRef = null;
            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
            distinctSalaryHead = new DataTable("Tmp");
            string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + fromDate + @"') AND YearNo = Year('" + fromDate + @"')";
            DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);

            string salaryProcessID = "''";
            for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
            {
                salaryProcessID += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
            }


            try
            {
                strSQL = @"SELECT EmpSlr.*,PSH.Sequence,ISNULL(crc.IsDecimalInDisb,0) IsDecimalInDisb,ISNULL(CRC.IntegerInDisb,1) IntegerInDisb,ISNULL(CRC.DecimalNo,0) DecimalNo FROM(SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
                                                    SPC.EmpInfoSystemID EmpSystemID, SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
                                                    SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
                                                    SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
                                                    CRE.Name AS PlantWiseExchangeCR, EXR.ToCurrencyBuying ExchangeRate, SPM.AmtDefinitionCurrencyID,
                                                    CR.Name AS AmtDefinitionCurrency, SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect, ISNULL(SH.IsCTCComponent,0) IsCTCComponent, ISNULL(SH.IsGrossComponent,0) IsGrossComponent
                                                    , sh.SalaryHead, sh.HeadCategory, sh.HeadType, ISNULL(SH.PartOfNetPay,0) PartOfNetPay

                                     FROM SalaryProcChild SPC

                                        left JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID



                                                        LEFT JOIN SalaryHead sh on sh.SalaryHeadID= spc.SalaryHeadID


                                                        LEFT JOIN scs.Currency CR ON SPM.AmtDefinitionCurrencyID = CR.Id

                                                        LEFT JOIN (
                                                                   SELECT* FROM ExchangerateDateWiseForHR

                                                                   WHERE FromDate IN (SELECT MAX(FromDate) FromDate FROM SalaryProcMaster


                                                                                                            WHERE SystemID IN(" + salaryProcessID + @")
																  )) EXR ON SPM.AmtDefinitionCurrencyID = EXR.FromCurrencyCode

                                                                                            AND SPC.PlantID = Exr.PlantID

                                                        LEFT JOIN SCS.Currency CRE ON EXR.FromCurrencyCode = CRE.Id

                                                        where isnull(SPC.SlrProcMstSystemID,'')  IN(" + salaryProcessID + @")) EmpSlr--ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID AND EmpBasic.PlantID = EmpSlr.PlantID

                                            Inner join EmployeeInformation EEI ON EEI.SystemId = EmpSlr.EmpSystemID

                                         LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EEI.SalaryRuleMasterSystemID

                                        LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID  AND SRG.SalaryHeadID = EmpSlr.SalaryHeadID
                                        LEFT JOIN(SELECT* FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId = '" + plantId + @"') PSH
                                                                       ON PSH.SalaryHeadId = EmpSlr.SalaryHeadID
                                        LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = EmpSlr.SalaryHeadID

                                                WHERE EEI.GroupID = '" + companyGroupId + @"' AND  EmpSlr.PlantId = '" + plantId + @"'";

                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            strSQL += @"AND EmpSlr.EmpSystemID IN(" + parameters["EmpSystemId"] + ")";

                        }
                    }
                }
                catch (Exception)
                {
                }
                strSQL += "ORDER BY EmpSystemId ";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSQL, out dsRef);

                distinctSalaryHead = dsRef.Tables[0].DefaultView.ToTable(true, "SalaryHeadID", "SalaryHead", "HeadType", "Sequence", "HeadCategory", "IntegerInDisb", "DecimalNo", "PartOfNetPay", "IsCTCComponent", "IsGrossComponent");
                distinctSalaryHead.DefaultView.Sort = "Sequence";
                distinctSalaryHead = distinctSalaryHead.DefaultView.ToTable();

                DataTable dt = dsRef.Tables[0];
                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (empId != dt.Rows[i]["EmpSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        dicBonus.Add(dt.Rows[i]["EmpSystemID"].ToString(), _data);
                    }
                    _data.Add(dt.Rows[i]);

                    empId = dt.Rows[i]["EmpSystemID"].ToString();
                }

                return dicBonus;


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
        public IWorkbook GetEmployeeSalaryProcessedReportSalLogWiseDirectInDirectSalaryPayable(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool isTopSheet, bool IsDirectInDirect)
        {
            #region Variable
            clsReport objRpt = null;

            DataView dvEmp = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsEmpLoyeeInfo = null;
            DataTable dtEmployees = null;

            DataView dvSlrSheet = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            int endGenericColumn = 0;
            #endregion Variable

            try
            {
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
                var ldateOfMonth = daysInMonth + "-" + monthName + "-" + year;
                var fdateOfMonth = "1" + "-" + monthName + "-" + year;
                string strPath = "";
                Image companyLogo = null;

                string companyLogoName = _sqlRepository.GetDataTable(@"select * from ORG.Company where Id = '" + companyId + @"'").Rows[0]["Image"].ToString();

                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyLogoName);  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();

                #endregion Variable

                #region DataSet

                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                objRpt.GetExtraAbsent(plantId, parameters, month.ToInt(), year.ToInt(), out dsExtraAbsent);

                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);

                //Sql Salary Structure 
                List<SalarySheetReportUD> listdsSlrStr = new List<SalarySheetReportUD>();

                //Sql Salary Process 
                DataTable dtSalaryHeadSheet;
                List<SalarySheetReportUD> listdsSlrProc = new List<SalarySheetReportUD>();
                GetEmployeeInfoDetailSalaryLogWiseDirectInDirect(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, out dsEmpLoyeeInfo, IsDirectInDirect);//Sql Query For Salary  Data
                Dictionary<string, List<DataRow>> dicEmpSalry = GetEmployeeSalaryInfoDetailDirectInDirect(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, salaryProcessId, payRollGroup, parameters, out dtSalaryHeadSheet);

                if (dicEmpSalry.First().Value[0].Table.Rows.Count > 0)
                {
                    listdsSlrProc = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    listdsSlrStr = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    dtEmployees = dsEmpLoyeeInfo.Tables[0];//dicEmpSalry.First().Value[0].Table;
                }
                else
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }

                dvSlrSheet = new DataView();

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);

                objRpt.SelectedPlant(plantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(2);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 6;
                xlsCol = 1;

                #region Column Variables
                int ColSr = 0, ColIDNo = 0, ColName = 0, ColDOJ = 0, ColDOS = 0, cDept = 0, cSec = 0, cSubSec = 0, cLine = 0, cPayrollGroup = 0, cJobLocation = 0, cGender = 0,
                    cGrade = 0, ColGVDG = 0, ColGrs = 0, colPayDays = 0, ColPdDy = 0, ColLate = 0, ColAbDy = 0, ColHlDy = 0, ColWkOf = 0, ColLv = 0, ColMLv = 0
                   , ColLWP = 0, colBank = 0, cDMP = 0, colBankAccountNo = 0, ColExtraAbsent = 0, colEmpCurrentStat = 0, colEmpStatus = 0, cPaymentMode = 0, cUnit = 0, ColTotalOTHR = 0, colDirectManpowerCost = 0;
                int npstruct = 0;

                #endregion

                //1
                SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);
                SetCellValue("ID No.", sheet1, xlsRow, ref xlsCol, out ColIDNo, 12);
                SetCellValue("Name", sheet1, xlsRow, ref xlsCol, out ColName, 17);
                SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out ColDOJ, 12);
                SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out ColDOS, 12);
                SetCellValue("EmployeeCurrentStatus", sheet1, xlsRow, ref xlsCol, out colEmpCurrentStat, 12);
                SetCellValue("EmployeeSatatus", sheet1, xlsRow, ref xlsCol, out colEmpStatus, 12);
                SetCellValue("Gender", sheet1, xlsRow, ref xlsCol, out cGender, 12);
                SetCellValue("Designation", sheet1, xlsRow, ref xlsCol, out ColGVDG, 25);
                SetCellValue("Employee Category", sheet1, xlsRow, ref xlsCol, out int colEmpCategory, 25);
                SetCellValue("Department", sheet1, xlsRow, ref xlsCol, out cDept, 25);
                SetCellValue("Section", sheet1, xlsRow, ref xlsCol, out cSec, 25);
                SetCellValue("SubSection", sheet1, xlsRow, ref xlsCol, out cSubSec, 25);
                SetCellValue("Unit", sheet1, xlsRow, ref xlsCol, out cUnit, 25);
                SetCellValue("Line", sheet1, xlsRow, ref xlsCol, out cLine, 25);
                SetCellValue("JobLocation", sheet1, xlsRow, ref xlsCol, out cJobLocation, 25);
                SetCellValue("Payroll group", sheet1, xlsRow, ref xlsCol, out cPayrollGroup, 25);
                //SetCellValue("Payment mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                SetCellValue("Payment mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                SetCellValue("Bank", sheet1, xlsRow, ref xlsCol, out colBank, 25);
                SetCellValue("Bank Acc No.", sheet1, xlsRow, ref xlsCol, out colBankAccountNo, 25);
                SetCellValue("IFSCCode", sheet1, xlsRow, ref xlsCol, out int colBankIFSCCode, 25);

                SetCellValue("Grade", sheet1, xlsRow, ref xlsCol, out cGrade, 25);
                //SetCellValue("Direct Manpower", sheet1, xlsRow, ref xlsCol, out cDMP, 25);
                SetCellValue("Direct Manpower Cost", sheet1, xlsRow, ref xlsCol, out colDirectManpowerCost, 25);

                SetCellValue("Pay Days", sheet1, xlsRow, ref xlsCol, out colPayDays, 5);
                SetCellValue("Present", sheet1, xlsRow, ref xlsCol, out ColPdDy, 9);
                SetCellValue("Late", sheet1, xlsRow, ref xlsCol, out ColLate, 9);
                SetCellValue("Absent", sheet1, xlsRow, ref xlsCol, out ColAbDy, 9);
                SetCellValue("LWP", sheet1, xlsRow, ref xlsCol, out ColLWP, 9);
                SetCellValue("Extra Absent", sheet1, xlsRow, ref xlsCol, out ColExtraAbsent, 9);
                SetCellValue("Holiday", sheet1, xlsRow, ref xlsCol, out ColHlDy, 9);
                SetCellValue("WeekOff", sheet1, xlsRow, ref xlsCol, out ColWkOf, 9);
                SetCellValue("Leave", sheet1, xlsRow, ref xlsCol, out ColLv, 11);
                SetCellValue("Maternity Leave", sheet1, xlsRow, ref xlsCol, out ColMLv, 20);
                SetCellValue("Total Ot Hr", sheet1, xlsRow, ref xlsCol, out ColTotalOTHR, 11);
                endGenericColumn = xlsCol;

                //SR to
                sheet1.Range[xlsRow, ColSr].Text = "Employee Information";
                sheet1.Range[xlsRow, ColSr, xlsRow, ColTotalOTHR].Merge();
                //xlsCol += 1;
                ColGrs = ColTotalOTHR;
                // 9

                var _count_earning_head = 0;
                var _count_earning_ctchead = 0;
                var _count_deducting_head = 0;
                var _total_head_count = 0;

                Dictionary<string, SalaryHeadSequence> shtList = null;

                CreateDynamicSHead(dtSalaryHeadSheet, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out shtList);

                List<SalaryHeadSequence> salList = new List<SalaryHeadSequence>();
                salList.AddRange(shtList.Values);

                xlsCol--;

                //Header Col
                if (_count_earning_ctchead > 0)
                {
                    sheet1.Range[xlsRow, ColGrs + 1].Text = "Earning head";
                    sheet1.Range[xlsRow, ColGrs + 1, xlsRow, ColGrs + _count_earning_head + _count_earning_ctchead].Merge();
                }

                var ds = ColGrs + 1 + _count_earning_head + _count_earning_ctchead;

                if (_count_deducting_head > 0)
                {
                    sheet1.Range[xlsRow, ds].Text = "Deduction head";
                    sheet1.Range[xlsRow, ds, xlsRow, ds + _count_deducting_head - 1].Merge();
                }
                npstruct = 0;
                if (shtList.Count > 0)
                {
                    xlsCol++;
                    npstruct = ColGrs + shtList.Count + 1;
                    sheet1.Range[xlsRow + 1, npstruct].Text = "Net Payable";
                    //sheet1.Range[xlsRow, npstruct].ColumnWidth = 14;
                    //sheet1.Range[xlsRow, npstruct, xlsRow + 1, npstruct].Merge();
                }

                xlsCol++;


                xlsCol++;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No.";
                sheet1.Range[xlsRow - 1, 1].ColumnWidth = 14;
                sheet1.Range[xlsRow - 1, 1, xlsRow - 1, 3].Merge();
                sheet1.Range[xlsRow, 1, xlsRow + 1, npstruct].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, npstruct].VerticalAlignment = ExcelVAlign.VAlignCenter;
                endXlsCol = npstruct;


                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                Param param = new Param();
                param.CompanyGroupId = companyGroupId;
                param.CompanyId = companyId;

                string FactoryAddress = string.Empty;
                try
                {

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
                catch (Exception ex)
                {
                }


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
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
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
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Salary Sheet For The Month Of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                var SrNo = 0;
                var x = "";

                var oRU = new ReportUtility();

                xlsRow = RowIndex;

                xlsRow--;
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    #region EmpInfo
                    try
                    {
                        SrNo += 1;
                        x = dtEmployees.Rows[i]["EmpSystemID"].ToString().Trim();

                        //1
                        sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                        sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //2
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, ColIDNo].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, ColIDNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColIDNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //3
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                            sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJ"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJ"].ToString();
                        sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOS"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOS].Text = dtEmployees.Rows[i]["DOS"].ToString();
                        sheet1.Range[xlsRow, ColDOS].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOS].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString()) == false)
                            sheet1.Range[xlsRow, colEmpCurrentStat].Text = dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString();
                        sheet1.Range[xlsRow, colEmpCurrentStat].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCurrentStat].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeStatus"].ToString()) == false)
                            sheet1.Range[xlsRow, colEmpStatus].Text = dtEmployees.Rows[i]["EmployeeStatus"].ToString();
                        sheet1.Range[xlsRow, colEmpStatus].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LegalDesignation"].ToString()) == false)
                            sheet1.Range[xlsRow, ColGVDG].Text = dtEmployees.Rows[i]["LegalDesignation"].ToString();
                        sheet1.Range[xlsRow, ColGVDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColGVDG].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmpCategoryName"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colEmpCategory].Text = dtEmployees.Rows[i]["EmpCategoryName"].ToString();
                        sheet1.Range[xlsRow, colEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4.2
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DepartmentName"].ToString()) == false)
                            sheet1.Range[xlsRow, cDept].Text = dtEmployees.Rows[i]["DepartmentName"].ToString();
                        sheet1.Range[xlsRow, cDept].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cDept].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SectionName"].ToString()) == false)
                            sheet1.Range[xlsRow, cSec].Text = dtEmployees.Rows[i]["SectionName"].ToString();
                        sheet1.Range[xlsRow, cSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cSec].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SubSectionName"].ToString()) == false)
                            sheet1.Range[xlsRow, cSubSec].Text = dtEmployees.Rows[i]["SubSectionName"].ToString();
                        sheet1.Range[xlsRow, cSubSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cSubSec].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["UnitName"].ToString()) == false)
                            sheet1.Range[xlsRow, cUnit].Text = dtEmployees.Rows[i]["UnitName"].ToString();
                        sheet1.Range[xlsRow, cUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PaymentMode"].ToString()) == false)
                            sheet1.Range[xlsRow, cPaymentMode].Text = dtEmployees.Rows[i]["PaymentMode"].ToString();
                        sheet1.Range[xlsRow, cPaymentMode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cPaymentMode].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["BankName"].ToString()) == false)
                            sheet1.Range[xlsRow, colBank].Text = dtEmployees.Rows[i]["BankName"].ToString();
                        sheet1.Range[xlsRow, colBank].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBank].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["BankAccNo"].ToString()) == false)
                            sheet1.Range[xlsRow, colBankAccountNo].Text = dtEmployees.Rows[i]["BankAccNo"].ToString();
                        sheet1.Range[xlsRow, colBankAccountNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBankAccountNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["IFSCCode"].ToString()) == false)
                            sheet1.Range[xlsRow, colBankIFSCCode].Text = dtEmployees.Rows[i]["IFSCCode"].ToString();
                        sheet1.Range[xlsRow, colBankIFSCCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBankIFSCCode].VerticalAlignment = ExcelVAlign.VAlignCenter;



                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                            sheet1.Range[xlsRow, cGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                        sheet1.Range[xlsRow, cGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["JobLocation"].ToString()) == false)
                            sheet1.Range[xlsRow, cJobLocation].Text = dtEmployees.Rows[i]["JobLocation"].ToString();
                        sheet1.Range[xlsRow, cJobLocation].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cJobLocation].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LineName"].ToString()) == false)
                            sheet1.Range[xlsRow, cLine].Text = dtEmployees.Rows[i]["LineName"].ToString();
                        sheet1.Range[xlsRow, cLine].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cLine].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PayRollGroup"].ToString()) == false)
                            sheet1.Range[xlsRow, cPayrollGroup].Text = dtEmployees.Rows[i]["PayRollGroup"].ToString();
                        sheet1.Range[xlsRow, cPayrollGroup].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cPayrollGroup].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //5
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["GradeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, cGrade].Text = dtEmployees.Rows[i]["GradeCode"].ToString();
                        sheet1.Range[xlsRow, cGrade].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGrade].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DirectManpowerCost"].ToString()) == false)
                            sheet1.Range[xlsRow, colDirectManpowerCost].Text = dtEmployees.Rows[i]["DirectManpowerCost"].ToString();
                        sheet1.Range[xlsRow, colDirectManpowerCost].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colDirectManpowerCost].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                            sheet1.Range[xlsRow, cGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                        sheet1.Range[xlsRow, cGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //5 "Section", "SubSection", 

                        #endregion
                        #region Attendance Data
                        //if (dtEmpAttdnInfo.Rows.Count > 0)
                        //{
                        double _ExtraAbsent = 0;
                        dvExtraAbsent.RowFilter = "EmpSystemID='" + dtEmployees.Rows[i]["EmpSystemID"].ToString() + "' ";
                        _ExtraAbsent = dvExtraAbsent.Count;
                        var payDays = 0.00;
                        // clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"]);
                        if (!String.IsNullOrEmpty(dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper()))
                        {
                            if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOffAndHoliday.ToString().ToUpper())
                            {
                                payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalHoliDay"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString());

                            }
                            if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOff.ToString().ToUpper())
                            {
                                payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString());
                            }
                        }
                        else
                        {
                            payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString());
                        }
                        SetCellTextAttdn(sheet1, xlsRow, colPayDays, payDays);
                        SetCellTextAttdn(sheet1, xlsRow, ColPdDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalPresent"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLate, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLate"].ToString()));
                        SetCellTextNumber(sheet1, xlsRow, ColAbDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLWP"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLWP, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLWP"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColExtraAbsent, _ExtraAbsent);
                        SetCellTextAttdn(sheet1, xlsRow, ColHlDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalHoliDay"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColWkOf, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLv"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColMLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalMLv"].ToString()));

                        SetCellTextAttdn(sheet1, xlsRow, ColTotalOTHR, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalOTHr"].ToString()) / 60);

                        //}
                        #endregion


                        //var _total_head_count_body = 0;

                        #region ------------------------------------Salary Sheet----------------------------------
                        if (dicEmpSalry.ContainsKey(dtEmployees.Rows[i]["EmpSystemID"].ToString()))
                        {
                            List<DataRow> drSalaryHeadCollection = dicEmpSalry[dtEmployees.Rows[i]["EmpSystemID"].ToString()];
                            if (drSalaryHeadCollection.Count > 0)
                            {
                                for (int CI = 0; CI < drSalaryHeadCollection.Count; CI++)
                                {
                                    if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "NET PAYABLE")
                                    {
                                        sheet1.Range[xlsRow, npstruct].Number = Convert.ToDouble(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                        continue;
                                    }
                                    try
                                    {
                                        SalaryHeadSequence xx = shtList[drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();
                                        if (xx != null)
                                        {
                                            if (drSalaryHeadCollection[CI]["HeadType"].ToString() == "D")
                                            {
                                                sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()) * (-1);
                                            }

                                            else
                                            {

                                                sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                            }

                                            sheet1.Range[xlsRow, xx.XLColIndex].NumberFormat = oRU.NumberFormatInt();
                                            sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                            sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                        throw ex;
                                    }

                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }


                    #endregion

                    xlsRow++;
                }//for emp count
                int sheetEndXlsRow = xlsRow - 1;
                #endregion ----------------------Data-----------------------

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var freezePan = RowIndex - 1;
                sheet1.UsedRange["A" + freezePan].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 10;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "EmpSalaryInfo";
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                #endregion

                workbook.Version = ExcelVersion.Excel2016;
                //var strFileName = "EmpSalaryStrSheet-" + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + Convert.ToDateTime(fdateOfMonth).ToString("yyyy") + "-" + para.SalaryProcessId + ".xls";

                if (isTopSheet == true)
                {
                    #region Salary Summary
                    string filePath = HostingEnvironment.MapPath("~/") + "TempSalaeySummary.xlsx";
                    workbook.SaveAs(filePath);
                    workbook = application.Workbooks.Open(filePath);

                    IWorksheet worksheet = workbook.Worksheets[0];
                    worksheet.Move(1);

                    #region PivotSheet1
                    IWorksheet pivotSheet = workbook.Worksheets[0];
                    pivotSheet.Name = "Summary";

                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = pivotSheet.GetColumnWidth(1) + pivotSheet.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((pivotSheet.GetRowHeight(1) + pivotSheet.GetRowHeight(2) + pivotSheet.GetRowHeight(3) + pivotSheet.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = pivotSheet.Pictures.AddPicture(1, 1, companyLogo);

                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    #region Report Header
                    xlsRow = 1;
                    xlsCol = 1;


                    pivotSheet.Range[xlsRow, 3].Text = CmpName;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow += 1;

                    pivotSheet.Range[xlsRow, 3].Text = FactoryName;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    xlsRow += 1;

                    pivotSheet.Range[xlsRow, 3].Text = FactoryAddress;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    xlsRow += 1;
                    pivotSheet.Range[xlsRow, 3].Text = "Salary Summary for the month of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    #endregion

                    pivotSheet.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;

                    IRange iRange = worksheet["A7:" + clsStaticInfo.GetxlsCol(npstruct) + (sheetEndXlsRow)];
                    IPivotCache cache2 = workbook.PivotCaches.Add(iRange);
                    IPivotCache cache = workbook.PivotCaches.Add(iRange);


                    #region Second Pivot table
                    pivotSheet.Range[xlsRow + 2, 1].Text = "EmployeeStatus, PaymentMode, Department Wise Salary Summary";
                    pivotSheet.Range[xlsRow + 2, 1, xlsRow + 2, 5].Merge();
                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Size = 12;

                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable2 = pivotSheet.PivotTables.Add("PivotTable2", pivotSheet["A8"], cache);

                    pivotTable2.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[cPaymentMode - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[cDept - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable2_1 = pivotSheet.PivotTables["PivotTable2"];
                    pivotTable2_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable2_1.Options.ShowDrillIndicators = false;

                    pivotTable2_1.DisplayFieldCaptions = true;

                    //Add data field
                    IPivotField field2 = pivotTable2_1.Fields[ColSr - 1];
                    pivotTable2_1.DataFields.Add(field2, "Total Employees", PivotSubtotalTypes.Count);
                    int pivotColumnCount = 0;
                    IPivotField fieldGross = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                            }
                            try
                            {
                                if (ob.HeadType == "D")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            catch (Exception ex)
                            {

                                //throw ex;
                            }

                        }
                    }
                    try
                    {
                        fieldGross = null;
                        pivotColumnCount++;
                        fieldGross = pivotTable2_1.Fields[npstruct - 1];
                        pivotTable2_1.DataFields.Add(fieldGross, "Net Payable", PivotSubtotalTypes.Sum);
                        fieldGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                    }
                    catch (Exception)
                    {

                    }

                    pivotTable2_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;

                    int totalColumns = pivotTable2_1.RowFields.Count + pivotColumnCount;

                    int lastCloumn = totalColumns + 2;

                    #endregion
                    #region PivotTable2

                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].Text = "Employee Category Wise Salary Summary";
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2, xlsRow + 2, lastCloumn + 5].Merge();
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet[clsStaticInfo.GetxlsCol(lastCloumn + 2) + "8"], cache);

                    //Add Pivot table fields (Row and Column fields)
                    pivotTable.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[colEmpCategory - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[cDept - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable1 = pivotSheet.PivotTables["PivotTable1"];
                    pivotTable1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable1.Options.ShowDrillIndicators = false;

                    pivotTable1.DisplayFieldCaptions = true;
                    pivotTable1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;

                    //Add data field
                    IPivotField field = pivotTable.Fields[ColSr - 1];
                    pivotTable.DataFields.Add(field, "Total Employee", PivotSubtotalTypes.Count);

                    int pivot2ColumnCount = 0;
                    IPivotField fieldGross2 = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross2 = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            if (ob.HeadType == "D")
                            {
                                pivot2ColumnCount++;
                                fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                            }

                        }
                    }
                    fieldGross2 = null;
                    pivot2ColumnCount++;
                    fieldGross2 = pivotTable.Fields[npstruct - 1];
                    pivotTable.DataFields.Add(fieldGross2, "Net Payable", PivotSubtotalTypes.Sum);
                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(0, "");


                    #endregion

                    #region PivotTable3

                    totalColumns += pivotTable.RowFields.Count + pivot2ColumnCount;


                    lastCloumn += totalColumns - 10;

                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].Text = "EmployeeStatus ,Employee Category and Department Wise  Salary Summary";
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2, xlsRow + 2, lastCloumn + 5].Merge();
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Bold = true;

                    //Create "PivotTable1" with the cache at the specified range
                    IPivotTable pivotTable3 = pivotSheet.PivotTables.Add("PivotTable13", pivotSheet[clsStaticInfo.GetxlsCol(lastCloumn + 2) + "8"], cache);

                    //Add Pivot table fields (Row and Column fields)
                    pivotTable3.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable3.Fields[cDept - 1].Axis = PivotAxisTypes.Row;
                    pivotTable3.Fields[colEmpCategory - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable13_1 = pivotSheet.PivotTables["PivotTable13"];
                    pivotTable13_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable13_1.Options.ShowDrillIndicators = false;

                    pivotTable13_1.DisplayFieldCaptions = true;
                    pivotTable13_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;


                    //Add data field
                    IPivotField fields3 = pivotTable13_1.Fields[ColSr - 1];
                    pivotTable13_1.DataFields.Add(fields3, "Total Employee", PivotSubtotalTypes.Count);

                    int pivot3ColumnCount = 0;
                    IPivotField fieldGross3 = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross3 = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivot3ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivot2ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }

                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            if (ob.HeadType == "D")
                            {
                                pivot2ColumnCount++;
                                fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                            }

                        }
                    }
                    fieldGross3 = null;
                    pivot2ColumnCount++;
                    fieldGross3 = pivotTable13_1.Fields[npstruct - 1];
                    pivotTable13_1.DataFields.Add(fieldGross3, "Net Payable", PivotSubtotalTypes.Sum);
                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(0, "");


                    #endregion
                    pivotSheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    pivotSheet.IsGridLinesVisible = false;
                    pivotSheet.IsDisplayZeros = false;

                    pivotSheet.UsedRange.WrapText = false;

                    #endregion
                    #endregion

                    workbook.ActiveSheetIndex = 0;
                }

                return workbook;
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //objRpt = null;
                //excelEngine = null;
                //application = null;
                //workbook = null;
            }
        }
        #endregion Employee Salary Payable

    }
    public class ReportLeaveInfo
    {
        public string EmpSystemID { get; set; }
        public string Code { get; set; }
        public string LeaveType { get; set; }
        public decimal AvailedLeave { get; set; }
    }
    public class ReportAttendanceSummmary
    {
        public string EmpSystemID { get; set; }
        public decimal TotalPresent { get; set; }
        public decimal TotalPresentLate { get; set; }
        public decimal TotalLWPAbsent { get; set; }
        public decimal TotalProcDate { get; set; }
        public decimal TotalLate { get; set; }
        public decimal TotalLWP { get; set; }
        public decimal TotalAbsentAct { get; set; }
        public int TotalHoliDay { get; set; }
        public int TotalWeekOff { get; set; }
        public decimal TotalLv { get; set; }
        public decimal TotalAbsent { get; set; }
        public int TotalWeekOffPlusWeekOffHoliDay { get; set; }
    }
    class ParaDynamicHead
    {
        public DataTable dtSalaryHead { get; set; }
        public int _total_head_count { get; set; }
        public IWorksheet sheet1 { get; set; }
        public int xlsRow { get; set; }
        public int xlsCol { get; set; }
        public int ColGrs { get; set; }
        public int _count_earning_head { get; set; }
        public int _count_deducting_head { get; set; }
        public int _count_earning_ctchead { get; set; }
        public List<SalaryHeadSequence> list { get; set; }
    }
    class SalarySheetReport
    {
        public string EmpInfoSystemID { get; set; }
        public string SalaryHeadID { get; set; }
        public string HeadCategory { get; set; }
        public decimal DisbusmentAmount { get; set; } = 0;
        public decimal EntryAmount { get; set; } = 0;
    }
    class SalarySheetReportUD
    {
        public string EmpSystemID { get; set; }
        public string SalaryHeadID { get; set; }
        public string HeadCategory { get; set; }
        public decimal DisbusmentAmount { get; set; } = 0;
        public decimal EntryAmount { get; set; } = 0;
    }
    class SalarySheetReportStructure //basic and Gross Value Structure and  
    {
        public string EmpInfoSystemID { get; set; }
        public string SalaryHeadID { get; set; }
        public decimal DisbusmentAmount { get; set; } = 0;
        public decimal EntryAmount { get; set; } = 0;
    }
    class SalaryStructureReport
    {
        public string EmpInfoSystemID { get; set; }
        public string SalaryHeadID { get; set; }
        public decimal EntryAmount { get; set; } = 0;
    }
    public class SalaryRegisterSorting : BaseModel
    {
        public string Parameter { get; set; }
        public string Sequence { get; set; }
    }
    class SalaryStructurePaySlip
    {
        public string SystemId { get; set; }
        public string SalaryHeadID { get; set; }
        public decimal EntryAmount { get; set; } = 0;
        public bool isDecimal { get; set; }
        public int DecimalNo { get; set; }
    }
    class SalarySheetPaySlip
    {
        //public string SystemId { get; set; }
        public string SalaryHeadID { get; set; }
        //public decimal EntryAmount { get; set; } = 0;
        public decimal DisbusmentAmount { get; set; } = 0;
        public bool IsDecimalInDisb { get; set; }
        public int DecimalNo { get; set; }
    }


}