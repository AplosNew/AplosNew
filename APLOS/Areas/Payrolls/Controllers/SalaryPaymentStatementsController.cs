using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;
using Library.Service.Inventory;
using static Library.Service.Helpers.ReportUtility;
using static Library.Service.HumanResources.PayRegisterBDReportService;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using System.Text;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class SalaryPaymentStatementsController : BaseController
    {
        #region Constructor
        private readonly IAttendanceManagementService _AttendanceManagementService;
        private readonly IPayRegisterBDReportService _payRegisterBDReportService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;



        public SalaryPaymentStatementsController(
              IAttendanceManagementService AttendanceManagementService, IPayRegisterBDReportService payRegisterBDReportService, IEmployeeProfileService employeeProfileService,
              ISqlRepository sqlRepository
            )
        {
            _AttendanceManagementService = AttendanceManagementService;
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
       
        public ActionResult SalaryCertificate()
        {
            return View();
        }
      
        public ActionResult BankStatementCSV()
        {
            return View();
        }
        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetEmployeeBankCbo()
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            string stSql = @"SELECT dISTINCT id BankId,UserName BankName,Bank.Sequence  FROM HKp.Bank Bank
                                INNER JOIN EmployeeBankInfo AS EBI ON Bank.Id = EBI.BankSystemID
                                INNER JOIN EmployeeInformation AS EI ON EI.SystemId = EBI.EmpSystemID
                                WHERE EI.GroupID = '" + identity.CompanyGroupId + @"' AND EI.CompanyId = '" + identity.CompanyId + @"' AND EI.PlantId = '" + identity.PlantId + @"'
                                ORDER BY Bank.Sequence";


            return Json(_sqlRepository.GetCombo(stSql, "BankId", "BankName"), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations

        [HttpPost, Authorize]
        public ActionResult GetSalaryPaymentStatement(string month, string year, string paymentMode, string bankId, string letterDate, string chequeNo, bool isActive, bool isSeperated, bool isMaternity, bool isCSV)

        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            clsReport objRpt = null;

            DataSet dsCmp = null;
            DataSet dsFactory = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;

            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            var FactoryName = "";
            var CmpName = "";

            ReportUtility oRu = null;

            DataSet dtEmpInfo = null;
            try
            {
                IWorkbook workbook = null;
                objRpt = new clsReport();
                var today = DateTime.Now.Date;
                string fileName = "";
                #region DataSet

                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No

                objRpt.GetEmployeeSalaryBankAccountStatement(out dtEmpInfo, paymentMode, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, bankId, "", Convert.ToInt32(month), year, isActive, isSeperated, isMaternity);



                var dtEmp = dtEmpInfo.Tables[0];//DataTable		

                if (dtEmp.Rows.Count == 0)
                {
                    throw new Exception("No Data Found!!");
                }



                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                objRpt.SelectedPlant(identity.PlantId.Trim(), out dsFactory);

                #endregion DataSet
                var colSrNo = 0;
                var colEmpCode = 0;
                var colACNo = 0;
                var colNetSalary = 0;
                var colEmployeeName = 0;
                var colDesignation = 0;
                object chequeAmount;
                var letterSubject = "";
                var letterSalutaion = "";
                var chequeDate = "";
                var description = "";
                var BankName = "";

                DataTable currencyCode = dtEmp.DefaultView.ToTable(true, "currCode");


                if (paymentMode.ToUpper() == "BANK")
                {
                    xlsRow = 15;
                    chequeAmount = dtEmp.Compute(@"Sum(pBankSalary)", "");

                    letterSubject = "SUB: SALARY FOR THE MONTH OF  " + monthName + "," + year;
                    letterSalutaion = "DEAR SIR,";
                    chequeDate = letterDate;
                    var checkNo = "-----------------";
                    if (string.IsNullOrEmpty(chequeNo) == false)
                    {
                        checkNo = "CHEQUE NO " + chequeNo + "";
                    }
                    description = "PLEASE FIND ENCLOSED HEREWITH " + checkNo + " FOR " + currencyCode.Rows[0]["currCode"] + " " + Convert.ToDecimal(chequeAmount).ToString("#,##0") + " DATED " + chequeDate + " DRAWN IN YOUR FAVOUR TOWARDS THE AMOUNTS TO BE CREDITED IN THE FOLLOWING SAVING BANK A/C.";
                }
                else
                {
                    xlsRow = 4;
                    chequeAmount = dtEmp.Compute(@"Sum(NetSalary)", "");
                }

                if (string.IsNullOrEmpty(chequeNo))
                {
                    chequeNo += "_ _ _ _ _ _ _ _ _ _ _";
                }

                //.Distinct().ToList();




                excelEngine = new ExcelEngine();



                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);

                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;



                //	#region ------------------Column Header------------------


                var ru = new ReportUtility();

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "S.No", 5); colSrNo = xlsCol; xlsCol++;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EMPCODE",15); colEmpCode = xlsCol; xlsCol++;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Name", 30); colEmployeeName = xlsCol; xlsCol++;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Designation", 30); colDesignation = xlsCol; xlsCol++;
                if (paymentMode.ToUpper() == "BANK")
                {
                    ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ACCOUNT NO.",16); colACNo = xlsCol; xlsCol++;
                }
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "NET  SALARY",16); colNetSalary = xlsCol; xlsCol++;
                var totalSalary = 0.00;
                endXlsCol = xlsCol;
                xlsRow++;
                var formulaStartRow = xlsRow;
                var slCount = 0;

                if (dtEmp.Rows.Count > 0)
                {
                    for (int i = 0; i < dtEmp.Rows.Count; i++)
                    {
                        slCount++;

                        ru.SetText(ref sheet1, xlsRow, colSrNo, slCount, ExcelHAlign.HAlignCenter);
                        ru.SetText(ref sheet1, xlsRow, colEmpCode, dtEmp.Rows[i]["EmployeeCode"].ToString());
                        ru.SetText(ref sheet1, xlsRow, colEmployeeName, dtEmp.Rows[i]["EmployeeName"].ToString());//dtEmpInfo.Tables[0].Rows[i][""].ToString()
                                                                                                                  //if (chkAdditionInfo.Checked == true)
                                                                                                                  //{
                        ru.SetText(ref sheet1, xlsRow, colDesignation, dtEmp.Rows[i]["Designation"].ToString());
                        sheet1.Range[xlsRow, colDesignation].BorderAround(ExcelLineStyle.Thin);
                        //}
                        if (paymentMode.ToUpper() == "BANK")
                        {
                            ru.SetText(ref sheet1, xlsRow, colACNo, dtEmp.Rows[i]["BankAccountNo"].ToString());
                        }
                        if (paymentMode.ToUpper() == "BANK")
                        {
                            sheet1.Range[xlsRow, colNetSalary].Number = Convert.ToDouble(dtEmp.Rows[i]["pBankSalary"].ToString());
                            totalSalary += Convert.ToDouble(dtEmp.Rows[i]["pBankSalary"].ToString());
                            sheet1.Range[xlsRow, colNetSalary].NumberFormat = ru.GetDecimalFormatlocalNetPay(Convert.ToBoolean(dtEmp.Rows[i]["IntegerInDisb"].ToString()), Convert.ToInt32(dtEmp.Rows[i]["DecimalNo"].ToString()), "");

                            //sheet1.Range[xlsRow, colNetSalary].NumberFormat = GetDecimalFormat(Convert.ToBoolean(dtEmp.Rows[i]["IntegerInDisb"].ToString()), Convert.ToInt16(dtEmp.Rows[i]["DecimalNo"].ToString()));
                            sheet1.Range[xlsRow, colNetSalary].HorizontalAlignment = ExcelHAlign.HAlignRight;
                            sheet1.Range[xlsRow, colNetSalary].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[xlsRow, colNetSalary].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, colACNo].BorderAround(ExcelLineStyle.Thin);
                        }
                        else
                        {
                            sheet1.Range[xlsRow, colNetSalary].Number = Convert.ToDouble(dtEmp.Rows[i]["NetSalary"].ToString());
                            totalSalary += Convert.ToDouble(dtEmp.Rows[i]["NetSalary"].ToString());

                            // sheet1.Range[xlsRow, colNetSalary].NumberFormat = GetDecimalFormat(Convert.ToBoolean(dtEmp.Rows[i]["IntegerInDisb"].ToString()), Convert.ToInt16(dtEmp.Rows[i]["DecimalNo"].ToString()));
                            sheet1.Range[xlsRow, colNetSalary].HorizontalAlignment = ExcelHAlign.HAlignRight;
                            sheet1.Range[xlsRow, colNetSalary].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[xlsRow, colNetSalary].BorderAround(ExcelLineStyle.Hair);
                        }


                        sheet1.Range[xlsRow, colSrNo].BorderAround(ExcelLineStyle.Thin);
                        sheet1.Range[xlsRow, colEmpCode].BorderAround(ExcelLineStyle.Thin);

                        sheet1.Range[xlsRow, colNetSalary].BorderAround(ExcelLineStyle.Thin);
                        sheet1.Range[xlsRow, colEmployeeName].BorderAround(ExcelLineStyle.Thin);

                        xlsRow++;
                    }
                }
                else
                {
                    throw new Exception("No Data Found.");
                }

                #region ******************Report Header******************
                ru.SetText(ref sheet1, xlsRow, colNetSalary - 1, "Total :");
                sheet1.Range[xlsRow, colNetSalary - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, 1, xlsRow, colNetSalary - 1].Merge();
                //ru.SetText(ref sheet1, xlsRow, colNetSalary, chequeAmount.ToString());
                sheet1.Range[xlsRow, colNetSalary].Number = Convert.ToDouble(chequeAmount.ToString());//GetDecimalFormat(Convert.ToBoolean(dtEmp.Rows[0]["IntegerInDisb"].ToString()), Convert.ToInt16(dtEmp.Rows[0]["DecimalNo"].ToString()));

                // sheet1.Range[xlsRow, colNetSalary].NumberFormat = GetDecimalFormat(Convert.ToBoolean(dtEmp.Rows[0]["IntegerInDisb"].ToString()), Convert.ToInt16(dtEmp.Rows[0]["DecimalNo"].ToString()));
                sheet1.Range[xlsRow, colNetSalary].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, 1, xlsRow, colNetSalary].BorderAround(ExcelLineStyle.Thin);
                sheet1.Range[xlsRow, 1, xlsRow, colNetSalary].BorderInside(ExcelLineStyle.Thin);
                sheet1.Range[xlsRow, 1, xlsRow, colNetSalary].CellStyle.Font.Bold = true;
                xlsRow++;
                xlsRow++;
                 xlsRow++;
                xlsRow++;
                string inWord ="Amount in word : "+ ru.InWord(totalSalary, dtEmp.Rows[0]["CurrencyId"].ToString());
                ru.SetText(ref sheet1, xlsRow, 1, inWord);
                sheet1.Range[xlsRow, 1, xlsRow, colNetSalary].Merge();
                sheet1.Range[xlsRow, 1, xlsRow, colNetSalary].CellStyle.Font.Bold = true;


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

                if (paymentMode.ToString() == "BANK")
                {

                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Bank Letter";
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    xlsRow++;

                    sheet1.Range[xlsRow, xlsCol].Text = "To";
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow++;
                    sheet1.Range[xlsRow, xlsCol].Text = "THE BRANCH MANAGER";
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow++;
                    sheet1.Range[xlsRow, xlsCol].Text = BankName;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //xlsRow++;
                    //sheet1.Range[xlsRow, xlsCol].Text = BankBranchName;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    //sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    //sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow++;
                    sheet1.Range[xlsRow, xlsCol].Text = "DATED:" + letterDate;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow++;
                    sheet1.Range[xlsRow, xlsCol].Text = letterSubject;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    xlsRow++;
                    xlsRow++;
                    sheet1.Range[xlsRow, xlsCol].Text = letterSalutaion;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    xlsRow += 2;

                    sheet1.Range[xlsRow, xlsCol].Text = description;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                }

                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                //sheet1.UsedRange["B6"].FreezePanes();
                //sheet1.FirstVisibleColumn = 2;
                //sheet1.FirstVisibleRow = 6;

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
                sheet1.PageSetup.BottomMargin = 0.5;
                sheet1.PageSetup.PrintTitleRows = "$A$15:$IV$15";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.UserId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                #endregion Page Setup

                fileName = monthName + "-" + year + paymentMode + "Statement" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);


                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
                // throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSalaryPaymentStatementBankCSV(string month, string year, string paymentMode, string bankId, string letterDate, string chequeNo, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            clsReport objRpt = null;

            DataSet dsCmp = null;
            DataSet dsFactory = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;

            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            var FactoryName = "";
            var CmpName = "";

            ReportUtility oRu = null;

            DataSet dtEmpInfo = null;
            try
            {
                IWorkbook workbook = null;
                objRpt = new clsReport();
                var today = DateTime.Now.Date;
                string attachmentName = "";
                #region DataSet

                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No

                objRpt.GetEmployeeSalaryBankAccountStatement(out dtEmpInfo, paymentMode, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, bankId, "", Convert.ToInt32(month), year, isActive, isSeperated, isMaternity);


                #endregion DataSet


                objRpt = new clsReport();

                var dataTable = dtEmpInfo.Tables[0];

                //string[] collist = "EmployeeCode,EmployeeName,";

                attachmentName = "attachment; filename=" + DateTime.Now.ToString("yyMMdd") + "BnakCsv.txt";
                //string attachment = "attachment; filename=MyCsvLol.csv";
                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.ClearHeaders();
                System.Web.HttpContext.Current.Response.ClearContent();
                System.Web.HttpContext.Current.Response.AddHeader("content-disposition", attachmentName);
                //Response.AddHeader("Content-Disposition", "attachment;filename=myfilename.xls");
                System.Web.HttpContext.Current.Response.ContentType = "application/txt";
                //HttpContext.Current.Response.AddHeader("Pragma", "public");
                string bankAccountNo = "";
                string bankSalaryAmount = "";

                StringBuilder builder = new StringBuilder();
                List<string> columnNames = new List<string>();
                List<string> rows = new List<string>();

                foreach (DataColumn column in dataTable.Columns)
                {
                    columnNames.Add(column.ColumnName);
                }

                // builder.Append(string.Join(",", columnNames.ToArray())).Append("\n");
                string strRow = "";
                foreach (DataRow row in dataTable.Rows)
                {
                    List<string> currentRow = new List<string>();
                    bankAccountNo = "";
                    bankSalaryAmount = "";
                    strRow = "";

                    strRow += row["BankAccountNo"].ToString() + "INR" + row["BankAccountNo"].ToString().Substring(0, 6) + "  C";
                    strRow += new String(' ', 14 - Convert.ToDouble(row["pBankSalary"]).ToString().Length);

                    
                    strRow += Convert.ToDouble(row["pBankSalary"]).ToString() + ".00" + "BY SAL.";
                    strRow += new String(' ', 128);
                                                        

                    builder.AppendLine(strRow);

                }

                //builder.Append(string.Join("\n", rows.ToArray()));

                //Response.Clear();
                //Response.ContentType = "text/csv";
                //Response.AddHeader("Content-Disposition", "attachment;filename=myfilename.csv");
                Response.Write(builder.ToString());
                Response.End();
                return Json(new { FileName = attachmentName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetBankStatmentCSV(DataTable dtEmpBankInfo, out string attachmentName)
        {

            clsReport objRpt = null;
            try
            {
                CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                objRpt = new clsReport();

                var dataTable = dtEmpBankInfo;

                //string[] collist = "EmployeeCode,EmployeeName,";

                attachmentName = "attachment; filename=" + DateTime.Now.ToString("yyMMdd") + "BnakCsv.txt";
                //string attachment = "attachment; filename=MyCsvLol.csv";
                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.ClearHeaders();
                System.Web.HttpContext.Current.Response.ClearContent();
                System.Web.HttpContext.Current.Response.AddHeader("content-disposition", attachmentName);
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

                    rows.Add(string.Join("---", currentRow.ToArray()));
                }

                builder.Append(string.Join(Environment.NewLine, rows.ToArray()));
                //builder.Append(string.Join("\n", rows.ToArray()));

                //Response.Clear();
                //Response.ContentType = "text/csv";
                //Response.AddHeader("Content-Disposition", "attachment;filename=myfilename.csv");
                Response.Write(builder.ToString());
                Response.End();
                //return Json(new { FileName = attachment, Error = false }, JsonRequestBehavior.AllowGet);

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


        #region Salary Certificate Report Start

        [HttpGet, Authorize]
        public ActionResult GetSalaryCertificateReport(string fiscalYearId, string employeeSystemId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ReportUtility ru = new ReportUtility();

            var fileName = "";
            var strPath = "";
            DataSet dsMonth = null;
            DataSet dsSalary = null;

            var File = "";

            clsReport objRpt = null;

            DataTable dtFiscalYear = null;

            objRpt = new clsReport();

            dtFiscalYear = _sqlRepository.GetDataTable(@"SELECT TaxYearName,Format(StartDate,'dd-MMM-yyyy') StartDate, Format(EndDate,'dd-MMM-yyyy') EndDate FROM SCS.TaxYear where Id = '" + fiscalYearId + @"' ");
            //objRpt.GetFiscalMonthListSql(dtFiscalYear.Rows[0]["StartDate"].ToString(), dtFiscalYear.Rows[0]["EndDate"].ToString(), out dsMonth);

            objRpt.GetEmpSalaryInfoForFiscalYear(identity.PlantId, employeeSystemId, dtFiscalYear.Rows[0]["StartDate"].ToString(), dtFiscalYear.Rows[0]["EndDate"].ToString(), out dsSalary);



            fileName = "SalaryCertificate" + identity.PlantId + ".docx";
            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);
            //Gets the paragraph at index 1
            try
            {

                WSection section = document.Sections[0];

                DataTable dtEmpMaster, dtSalary;
                dtEmpMaster = null;
                DataView dvEmp = null;
                dvEmp = new DataView();
                dvEmp.Table = dsSalary.Tables[0];

                dtEmpMaster = dvEmp.ToTable(true, "SystemId", "EmployeeName", "FatherName", "GenderID", "Salutation");
                DataView dvSalaryHead = new DataView(dsSalary.Tables[0]);

                dvSalaryHead.Sort = "HeadType desc,Sequence";
                DataTable dtSalaryHead = dvSalaryHead.ToTable(true, "SalaryHeadID", "SalaryHead", "HeadType", "Sequence", "HeadCategory", "IsGrossComponent", "IntegerInDisb", "DecimalNo", "DisbusmentAmount", "DisbusmentCurrencyID");

                Dictionary<string, string> columns = new Dictionary<string, string>();

                dtSalary = null;
                foreach (DataColumn item in dtEmpMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var earningTotal = 0.00;
                var deductionTotal = 0.00;

                var totalPayable = 0.00;
                if (dtEmpMaster.Rows.Count > 0)
                {
                    //{ServiceItems}
                    earningTotal = makeEarningSalaryDetailsTable(document, dtSalaryHead, employeeSystemId);//Service Details 
                    deductionTotal = makeDeductionSalaryDetailsTable(document, dtSalaryHead, employeeSystemId, earningTotal);//Service Details 

                    totalPayable = earningTotal - deductionTotal;

                    // document.Replace("{ServiceDetails}", "Service Details", true, true);
                }
                document.Replace("{FiscalYearName}", dtFiscalYear.Rows[0]["TaxYearName"].ToString(), true, true);
                document.Replace("{FiscalYearRange}", dtFiscalYear.Rows[0]["StartDate"].ToString() + " To " + dtFiscalYear.Rows[0]["EndDate"].ToString(), true, true);

                var totalInWords = ru.InWord((totalPayable), dtSalaryHead.Rows[0]["DisbusmentCurrencyID"].ToString());

                document.Replace("{Total}", totalPayable.ToString("#,##0.00"), true, true);
                document.Replace("{TotalInWords}", totalInWords.Remove(totalInWords.Length - 1), true, true);
                document.Replace("{TotalInWordsDetail}", totalInWords, true, true);

                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));

                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                StringCollection strColDistinct = new StringCollection();
                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());

                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dtEmpMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }

                DocToPDFConverter converter = new DocToPDFConverter(); //----ai line ta new kono report a bosanor for error asbe ---suzation thake prothm ta chose kore dita hoba----

                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                converter.Dispose();

                string Prefix = "SalaryCertificate" + employeeSystemId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                pdfDocument.Close(true);
                document.Close();

                return Json(new { FileName = Prefix, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
                // throw ex;
            }
        }
        public double makeEarningSalaryDetailsTable(WordDocument document, DataTable dtSalary, string empSystemId)
        {
            string replaceString = "{Gross}";
            ReportUtility ru = new ReportUtility();

            using (var dvSalary = new DataView(dtSalary)
            {
                RowFilter = "(IsGrossComponent = 1 and HeadType = 'E') OR HeadCategory = 'Other Bonus'",

            })
            {
                dtSalary = dvSalary.ToTable();
            }
            //clsDataContext data = new clsDataContext();

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 11f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 2;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();

            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.BorderType = BorderStyle.None;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            //WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.FontSize = 11f;

            WCharacterFormat FontSized = new WCharacterFormat(document);
            FontSized.FontSize = 11f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Head ");
            range.OwnerParagraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;

            int colSalaryHead = COL; //COL++;           
            range.ApplyCharacterFormat(FontBold);
            COL++;
            int colSalaryValue = COL;
            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Value");
            range.ApplyCharacterFormat(FontBold);
            range.OwnerParagraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            COL++;
            int colSalaryTotalAmount = COL;
            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Amount");
            range.ApplyCharacterFormat(FontBold);
            range.OwnerParagraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            //wTable.Rows.Add(TemplateRow);

            //wTable.Rows.Add(TemplateRow);
            //ROW++;
            //wTable.Rows[ROW].RowFormat.Borders.BorderType = BorderStyle.Hairline;



            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dtSalary.Rows.Count; i++)
            {
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }
                //IParagraphItem p = TROW.Cells[colSalaryHead].AddParagraph().AppendText(dtSalary.Rows[i]["SalaryHead"].ToString());
                //p.OwnerParagraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Le;


                IWParagraph p = TROW.Cells[colSalaryHead].AddParagraph();
                IWTextRange te = p.AppendText(dtSalary.Rows[i]["SalaryHead"].ToString());
                p.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
                te.ApplyCharacterFormat(FontSized);


                IWParagraph pt = TROW.Cells[colSalaryValue].AddParagraph();
                IWTextRange tpe = pt.AppendText(clsStdLib.dbl(dtSalary.Rows[i]["DisbusmentAmount"].ToString()).ToString("#,##0.00"));
                pt.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;
                tpe.ApplyCharacterFormat(FontSized);

                //p = TROW.Cells[colSalaryValue].AddParagraph().AppendText(clsStdLib.dbl(dtSalary.Rows[i]["DisbusmentAmount"].ToString()).ToString("#,##0.00"));
                //p.OwnerParagraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

                totalValue += clsStdLib.dbl(dtSalary.Rows[i]["DisbusmentAmount"].ToString());

            }
            //WTableRow _TROW = wTable.LastRow;
            //_TROW.Cells[colSalaryTotalAmount].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);
            //ROW++;
            #region Total
            int TotalRow = ROW;
            //wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;

            //_TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);
            IWParagraph para = _TROW.Cells[colSalaryTotalAmount].AddParagraph();
            IWTextRange t = para.AppendText(totalValue.ToString("#,##0.00"));
            para.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;
            t.ApplyCharacterFormat(FontBold);

            #endregion Total

            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");



            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["CurrencyId"].ToString()) + ")");

            #endregion Total


            ROW++;
            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable


            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle2 = document.AddParagraphStyle("MyStyle2");
            //Sets the formatting of the style
            myStyle2.CharacterFormat.FontSize = 8f;
            myStyle2.CharacterFormat.TextColor = Color.Black;
            //myStyle2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 120;

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle2");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;


            //primary cells merging (veritcal)






            IWParagraphStyle style2 = document.AddParagraphStyle("SubTotalStyle2");
            style2.CharacterFormat.Bold = true;
            style2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle2");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section

            wTable.Rows[0].Cells[0].CellFormat.Borders.Bottom.BorderType = BorderStyle.Hairline;
            wTable.Rows[0].Cells[1].CellFormat.Borders.Bottom.BorderType = BorderStyle.Hairline;
            wTable.Rows[0].Cells[2].CellFormat.Borders.Bottom.BorderType = BorderStyle.Hairline;


            //wTable.Rows[0].RowFormat.Borders.Bottom.BorderType = BorderStyle.Hairline;


            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return totalValue;
        }

        public double makeDeductionSalaryDetailsTable(WordDocument document, DataTable dtSalary, string empSystemId, double earnedValue)
        {
            string replaceString = "{Deduction}";
            ReportUtility ru = new ReportUtility();

            double netAmount = 0.00;

            using (var dvSalary = new DataView(dtSalary)
            {
                RowFilter = "HeadType = 'D'",

            })
            {
                dtSalary = dvSalary.ToTable();
                //set ds
            }
            //clsDataContext data = new clsDataContext();

            IWParagraphStyle rightAligned = document.AddParagraphStyle("rightAligned");
            //Sets the formatting of the style
            rightAligned.CharacterFormat.FontSize = 11f;
            rightAligned.CharacterFormat.TextColor = Color.Black;
            rightAligned.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 2;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();

            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.BorderType = BorderStyle.None;

            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);
            wTable.Rows[0].Cells[0].Width = 220;
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            FontBold.FontSize = 11f;

            //IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Head ");
            //range.OwnerParagraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;

            int colSalaryHead = COL; //COL++;           
            //range.ApplyCharacterFormat(FontBold);
            COL++;
            int colSalaryValue = COL;
            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Value");
            //range.ApplyCharacterFormat(FontBold);
            //range.OwnerParagraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;
            COL++;
            int colSalaryAmount = COL;
            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Amount");
            //range.OwnerParagraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;
            //range.ApplyCharacterFormat(FontBold);
            //wTable.Rows.Add(TemplateRow);

            //wTable.Rows.Add(TemplateRow);
            //ROW++;

            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            WTableRow TROW = null;
            for (int i = 0; i < dtSalary.Rows.Count; i++)
            {
                ROW++;
                TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }
                //IParagraphItem p = TROW.Cells[colSalaryHead].AddParagraph().AppendText(dtSalary.Rows[i]["SalaryHead"].ToString());

                //p.OwnerParagraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;


                IWParagraph wp = TROW.Cells[colSalaryHead].AddParagraph();
                IWTextRange tp = wp.AppendText(dtSalary.Rows[i]["SalaryHead"].ToString());
                wp.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
                tp.CharacterFormat.FontSize = 11f;

                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(dsServiceItems.Rows[i]["Amount"].ToString());
                //TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Tables[0].Rows[i]["FirstCharacteristicsValue"].ToString());
                //TROW.Cells[colChar2].AddParagraph().AppendText(dsOrderMaster.Tables[0].Rows[i]["SecondCharacteristicsValue"].ToString());
                //TROW.Cells[colChar3].AddParagraph().AppendText(dsOrderMaster.Tables[0].Rows[i]["ThirdCharacteristicsValue"].ToString());




                //IWParagraph p = TROW.Cells[colSalaryHead].AddParagraph();
                //IWTextRange te = p.AppendText(dtSalary.Rows[i]["SalaryHead"].ToString());
                //p.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
                //te.ApplyCharacterFormat(FontSized);


                IWParagraph pt = TROW.Cells[colSalaryValue].AddParagraph();
                IWTextRange tpe = pt.AppendText((clsStdLib.dbl(dtSalary.Rows[i]["DisbusmentAmount"].ToString()) * -1).ToString("#,##0.00"));
                pt.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;
                tpe.CharacterFormat.FontSize = 11f;


                //TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Tables[0].Rows[i]["TransactionRate"].ToString()).ToString("F2"));
                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Tables[0].Rows[i]["TrnAmount"].ToString()).ToString("F2"));

                totalValue += clsStdLib.dbl(dtSalary.Rows[i]["DisbusmentAmount"].ToString()) * -1;

                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));


                wTable.AddRow();
            }
            netAmount = earnedValue - totalValue;
            //ROW++;
            #region Total
            int TotalRow = ROW;

            //TROW = wTable.();
            //_TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);
            IWParagraph para = TROW.Cells[colSalaryAmount].AddParagraph();
            IWTextRange t = para.AppendText(totalValue.ToString("#,##0.00"));
            para.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;
            t.ApplyCharacterFormat(FontBold);
            //_TROW.Cells[colSalaryAmount].AddParagraph().AppendText(totalValue.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);
            //wTable.AddRow();
            ROW++;
            int netRow = ROW;
            wTable.AddRow();
            TROW = wTable.LastRow;
            TROW.RowFormat.Borders.BorderType = BorderStyle.Single;
            TROW.Cells[colSalaryHead].AddParagraph().AppendText("Net ").ApplyCharacterFormat(FontBold);
            TROW.Cells[colSalaryValue].AddParagraph().AppendText("");

            IWParagraph parae = TROW.Cells[colSalaryAmount].AddParagraph();
            IWTextRange te = parae.AppendText(netAmount.ToString("#,##0.00"));
            parae.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;
            te.ApplyCharacterFormat(FontBold);
            //_TROW.Cells[colSalaryAmount].AddParagraph().AppendText(netAmount.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);
            //for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            //{//|| C == colQty
            //    if (C == colSalaryHead)
            //        continue;

            //    double value = 0;
            //    for (int i = startRow; i < TotalRow; i++)
            //    {

            //        foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
            //        {
            //            value += clsStdLib.dbl(item.Text);
            //        }
            //    }
            //    _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);

            //}
            #endregion Total


            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");



            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["CurrencyId"].ToString()) + ")");

            #endregion Total


            ROW++;
            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable


            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle2 = document.AddParagraphStyle("MyStyle3");
            //Sets the formatting of the style
            myStyle2.CharacterFormat.FontSize = 8f;
            myStyle2.CharacterFormat.TextColor = Color.Black;
            myStyle2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 120;

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle3");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;


            //primary cells merging (veritcal)






            IWParagraphStyle style2 = document.AddParagraphStyle("SubTotalStyle3");
            style2.CharacterFormat.Bold = true;
            style2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle2");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section

            wTable.Rows[netRow].Cells[0].CellFormat.Borders.Top.BorderType = BorderStyle.Hairline;
            wTable.Rows[netRow].Cells[1].CellFormat.Borders.Top.BorderType = BorderStyle.Hairline;
            wTable.Rows[netRow].Cells[2].CellFormat.Borders.Top.BorderType = BorderStyle.Hairline;

            wTable.Rows[netRow].Cells[0].CellFormat.Borders.Bottom.BorderType = BorderStyle.Double;
            wTable.Rows[netRow].Cells[1].CellFormat.Borders.Bottom.BorderType = BorderStyle.Double;
            wTable.Rows[netRow].Cells[2].CellFormat.Borders.Bottom.BorderType = BorderStyle.Double;

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return totalValue;
        }
        class clsStdLib
        {
            public static string passWord = "prodDisplay";
            public clsStdLib()
            {

            }
            public enum mType
            {
                Error,
                Success,
                Information
            }
            public static bool passwordGet = true;
            public static string[] sMonth = new string[] { "<Unselect>", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

            public static string DataRankNames(int dayNo)
            {

                if (dayNo <= 0)
                    return "";

                if (dayNo.ToString().Length > 1)
                {
                    string Right = dayNo.ToString().Substring(dayNo.ToString().Length - 2, 2);
                    if (clsStdLib.dbl(Right) >= 10 && clsStdLib.dbl(Right) <= 20)
                        return dayNo + "th";
                }

                string RightString = dayNo.ToString().Substring(dayNo.ToString().Length - 1, 1);
                switch (RightString)
                {
                    case "1":
                        return dayNo + "st";
                    case "2":
                        return dayNo + "nd";
                    case "3":
                        return dayNo + "rd";
                    default:
                        return dayNo + "th";

                }




            }

            #region date related
            public static readonly string dateFormat = "dd-MMM-yyyy";
            public static readonly string sqliteDateFormat = "yyyy-MM-dd";
            public static readonly string AppToDBdateFormat = "yyyy-MM-dd hh:mm:ss";
            public static bool IsDateOK(string strdate)
            {
                try
                {
                    if (strdate.Length != 11)
                    {
                        return false;
                    }
                    if (strdate.Substring(2, 1) != "-" && strdate.Substring(6, 1) != "-")
                    {
                        return false;
                    }
                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
                finally
                {
                    //
                }
            }// end function
            private static bool DateOkCheck(string strdate)
            {
                try
                {
                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
                finally
                {
                    //
                }
            }// end function
            public static object chk_NullDateData(object dateValue)
            {
                if (DateOkCheck("" + dateValue.ToString()) == false)
                {
                    dateValue = "";
                }

                if (("" + dateValue.ToString()) == "")
                {
                    System.DateTime dt = new System.DateTime(1901, 1, 1);
                    dateValue = (object)dt;
                }
                return (object)dateValue;
            }
            public static System.DateTime AppDateConvert(object dateValue, string input_date_format, string output_date_format)
            {
                string strDate = null;
                dateValue = chk_NullDateData(dateValue);
                strDate = dateValue.ToString();
                if (strDate != "")
                {
                    if (input_date_format.Trim() != "")
                    {
                        if (output_date_format.Trim() != "")
                        {
                            System.Globalization.DateTimeFormatInfo InputFormat = new System.Globalization.DateTimeFormatInfo();
                            InputFormat.ShortDatePattern = input_date_format;
                            System.DateTime myDt = System.Convert.ToDateTime(strDate, InputFormat);
                            strDate = myDt.ToString(output_date_format);
                        }
                    }
                }
                return System.Convert.ToDateTime(strDate);
            }// End of function
            public static Object DateData_AppToDB(object dateValue, string DB_Level_date_format)
            {
                if (string.IsNullOrEmpty((string)dateValue))
                    return DBNull.Value;

                string strDate = null;
                strDate = dateValue.ToString();
                if (DB_Level_date_format != "")
                {
                    // Collecting the user terminal set format 
                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    strDate = AppDateConvert(strDate, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString(), DB_Level_date_format).ToString();
                }

                string m = System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);
                return System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);


            }// End of function
            public static System.DateTime DateData_DBToApp(object dateValue)
            {
                string strDate = null;
                strDate = dateValue.ToString();

                System.Globalization.DateTimeFormatInfo myDBDateFormat = new System.Globalization.CultureInfo("en-US", false).DateTimeFormat;
                strDate = DateData_DBToApp(dateValue, myDBDateFormat.ShortDatePattern.ToString()).ToString();
                return System.Convert.ToDateTime(strDate);
            }// End function
            public static System.DateTime DateData_DBToApp(object dateValue, string DB_Level_date_format)
            {
                string strDate = null;
                strDate = dateValue.ToString();
                if (DB_Level_date_format != "")
                {
                    // Collecting the user terminal set format 
                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    strDate = AppDateConvert(strDate, DB_Level_date_format, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString()).ToString();
                }
                return System.Convert.ToDateTime(strDate);
            }// End of function
            public static String makeBaseBlank(object dateValue)
            {
                System.DateTime dt;
                dt = System.Convert.ToDateTime(dateValue.ToString());
                if (dt.Year == 1901)
                {
                    return "";
                }
                else
                {
                    return dateValue.ToString();
                }
            }// End of function
             ///<summary>
             ///return day difference in integer. 
             ///    Example 1: firstDate[Less Than]lastDate returns positive value
             ///    Example 2: firstDate>lastDate returns negative value
             ///    Example 3: firstDate=lastDate returns 0 [zero]**/
             /// </summary>
            public static int dateDiff(string firstDate, string lastDate)
            {

                int difference = 0;
                try
                {
                    firstDate = Convert.ToDateTime(firstDate).ToString("dd-MMM-yyyy");
                    lastDate = Convert.ToDateTime(lastDate).ToString("dd-MMM-yyyy");

                    if (IsDateOK(firstDate) == false)
                    {
                        Exception ex = new Exception("Invalid [First Date]");
                        throw (ex);
                    }
                    if (IsDateOK(lastDate) == false)
                    {
                        Exception ex = new Exception("Invalid [Last Date]");
                        throw (ex);
                    }
                    DateTime dateFirstDate = Convert.ToDateTime(firstDate);
                    DateTime dateLastDate = Convert.ToDateTime(lastDate);
                    TimeSpan TimeSpan = dateLastDate.Subtract(dateFirstDate);


                    difference = TimeSpan.Days;
                }
                catch (Exception ex)
                {
                    throw (ex);
                }

                return difference;
            }



            public static string getSqliteDate(string standardDate)
            {
                return (Convert.ToDateTime(standardDate).ToString(sqliteDateFormat));
            }
            public static string getStandardDateFromSqliteDate(string SqliteDate)
            {
                if (SqliteDate.Length != 10)
                    return "";
                if (SqliteDate.Split('-').Length != 3)
                    return "";
                //many things to validate 
                //but i have less time :)
                string month = ValidLength(sMonth[Convert.ToInt32(SqliteDate.Split('-')[1])], 3).ToString();


                return SqliteDate.Split('-')[2] + "-" + month + "-" + SqliteDate.Split('-')[0];
            }
            #endregion date related

            #region numeric
            public static bool IsNumeric(string strNumber)
            {
                Double d;
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Length == 0)
                {
                    return false;
                }
                return Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d);
            } // End Function
            public static string GetNumericData(string strNumber)
            {
                double d;
                strNumber = strNumber.Replace(",", "");
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Trim() == "")
                { return "0"; }
                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
                {
                    return strNumber;
                }
                else
                {
                    return "0";
                }
            }// end function
            public static string GetNumericDataInDecimalFormat(string strNumber, int precision)
            {
                if (precision < 1)
                    return strNumber;

                string s_precision = new String('0', precision);

                double d;
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Trim() == "")
                { return "0." + s_precision; }
                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
                {
                    return string.Format("{0:0." + s_precision + "}", d);
                }
                else
                {
                    return "0." + s_precision;
                }
            }// end function
            public static double dbl(string d)
            {
                return Convert.ToDouble(GetNumericData(d));

            }
            public static int Percentage(int total, double percentage)
            {
                return (int)(total * (percentage / 100));

            }
            //validation
            public static void numericValidation(string value, bool isMandatory, bool isInteger, bool negativeAllowed, string fieldName)
            {

                try
                {



                    if (isMandatory == true)
                    {
                        if (value.Trim() == "")
                        {
                            Exception ex = new Exception("please insert [" + fieldName + "]");
                            throw (ex);
                        }
                        if (Convert.ToDouble(GetNumericData(value.Trim())) == 0)
                        {
                            Exception ex = new Exception("please insert [" + fieldName + "]");
                            throw (ex);
                        }

                        if (value.Trim() != "")
                        {
                            if (IsNumeric(value.Trim()) == false)
                            {
                                Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                                throw (ex);
                            }
                        }
                    }

                    if (value.Trim() != "")
                    {
                        if (IsNumeric(value.Trim()) == false)
                        {
                            Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                            throw (ex);
                        }
                        if (isInteger == true)
                        {

                            if (isInt(value.Trim()) == false)
                            {
                                Exception ex = new Exception("Number must be integer for the field [" + fieldName + "]");
                                throw (ex);
                            }

                        }
                        if (negativeAllowed == false)
                        {
                            if (Convert.ToDouble(GetNumericData(value.Trim())) < 0)
                            {
                                Exception ex = new Exception("Negative values are not allowed for the field [" + fieldName + "]");
                                throw (ex);
                            }
                        }
                    }



                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }


            }

            ///<summary>
            ///check whether a value is integer or not returns true if integer, 
            ///false if floating or string containing alpahnumeric
            ///</summary>
            public static bool isInt(string num)
            {

                bool isInt;
                int number;
                try
                {
                    isInt = System.Int32.TryParse(num, out number);
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }
                return isInt;
            }


            #endregion numeric

            #region string

            public static readonly string excelNegativePOsitiveSign = @"+#,##0.00;-#,##0.00;* ??;@";
            public static readonly string NegativePOsitiveSign = @"+#,##0.00;-#,##0.00;0";
            public static readonly string NumberFormatString = "#,##0.000;(#,##0.000);* ??;@";
            public static readonly string NumberFormatStringFourDecimal = "#,##0.0000;(#,##0.0000);* ??;@";
            public static readonly string NumberFormatStringFiveDecimal = "#,##0.00000;(#,##0.00000);* ??;@";
            public static readonly string NumberFormatStringTwoDecimal = "#,##0.00;(#,##0.00);* ??;@";
            public static readonly string NumberFormatStringTwoDecimalWithZero = "#,##0.00;(#,##0.00)";
            public static readonly string NumberFormatStringInteger = "#,##0;(#,##0);* ??;@";
            public static readonly string NumberFormatStringIntegerWithZero = "#,##0;(#,##0)";
            public static readonly string NumberFormatStringText = "@"; //format cell data as text


            public static object ValidLength(string str)
            {

                string removechar = "";
                if (str.Trim() == "")
                {
                    return (object)Convert.DBNull;
                }
                removechar = str.Trim();
                removechar = removechar.Replace("'", " ");

                return (object)removechar.Trim();

            }
            public static object ValidLength(string str, int length)
            {

                string removechar = "";
                if (str.Trim() == "")
                {
                    return (object)Convert.DBNull;
                }
                removechar = str.Trim();
                removechar = removechar.Replace("'", " ");


                int strLen = removechar.Length;
                if (strLen > length)
                    removechar = removechar.Substring(0, length);

                return (object)removechar.Trim();

            }
            public static string FileNameLegalChar(string fileName)
            {
                string illegalChar = @"~`!@#$%^&*=/\|>,<";
                foreach (char c in illegalChar)
                {
                    fileName = fileName.Replace(c.ToString(), " ");
                }

                return fileName;
            }
            private StringCollection getTableColumns(ref DataSet dsLocal)
            {
                StringCollection strcol = new StringCollection();
                for (int COL = 0; COL < dsLocal.Tables[0].Columns.Count; COL++)
                {
                    strcol.Add(dsLocal.Tables[0].Columns[COL].ColumnName.ToUpper());
                }

                return strcol;

            }
            public static string emptyString(string str)
            {
                //this function returns an empty string(not a null) from null or empty or '&nbsp;' from the page
                if (str == "&nbsp;")
                    str = "";
                if (string.IsNullOrEmpty(str) == true)
                    str = "";


                return str;
            }//this function returns an empty string(not a null) from null or empty '&nbsp;' from the page
            #endregion string


            #region others
            //public void copyDataset(DataSet source, ref DataSet destination)
            //{
            //    //StringCollection strColDestinationColumns = getTableColumns(ref destination);//upper case
            //    DataRow drLocal = null;
            //    for (int ROW = 0; ROW < source.Tables[0].Rows.Count; ROW++)
            //    {
            //        drLocal = destination.Tables[0].NewRow();
            //        for (int COL = 0; COL < source.Tables[0].Columns.Count; COL++)
            //        {
            //            if (strColDestinationColumns.Contains(source.Tables[0].Columns[COL].ToString().ToUpper()))
            //            {
            //                drLocal[source.Tables[0].Columns[COL].ToString()] = ValidLength(source.Tables[0].Rows[ROW][source.Tables[0].Columns[COL].ToString()].ToString());
            //            }
            //        }
            //        destination.Tables[0].Rows.Add(drLocal);
            //    }


            //}
            public static string GetxlsCol(int intCol)
            {
                //returns excel columns based on column number. tested 1 to 256 column numbers
                try
                {
                    if (intCol < 1 || intCol > 256)
                    {
                        System.Exception ex = new Exception("Invalid Column Value");
                        throw (ex);
                    }
                    intCol = intCol - 1;
                    int intFirstLetter = ((intCol) / 512) + 64;
                    int intSecondLetter = ((intCol % 512) / 26) + 64;
                    int intThirdLetter = (intCol % 26) + 65;
                    char FirstLetter;
                    char SecondLetter;
                    if (intFirstLetter > 64)
                        FirstLetter = (char)intFirstLetter;
                    else
                        FirstLetter = ' ';

                    if (intSecondLetter > 64)
                        SecondLetter = (char)intSecondLetter;
                    else
                        SecondLetter = ' ';

                    char ThirdLetter = (char)intThirdLetter;
                    return string.Concat(FirstLetter, SecondLetter, ThirdLetter).Trim();
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }
            }//returns excel columns based on column number. tested 1 to 256 column numbers
            #endregion others

            public static object RetValidLen(string Data)
            {
                if (string.IsNullOrEmpty(Data))
                    return DBNull.Value;

                return Data;
            }
            public static double sum(string columnName, DataTable dtLocal, string criteria)
            {
                double total = 0;
                DataRow[] dr = dtLocal.Select(criteria);
                foreach (DataRow d in dr)
                {
                    total += dbl(d[columnName].ToString());
                }


                return total;
            }
        }
        #endregion   Salary Certificate Report End
    }
}