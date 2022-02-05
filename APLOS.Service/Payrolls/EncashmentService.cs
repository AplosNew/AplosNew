#region Using

using clsAttendance;
using ConnectionManager;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Setups;
using Library.Service.Currencies;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.Payrolls.OT;
using Library.ViewModel.Organizations;
using OTSBD;
using Syncfusion.DocIO.DLS;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using static Library.Service.Helpers.ReportUtility;

#endregion Using

namespace Library.Service.HumanResources
{
    public class EncashmentService : IEncashmentService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<EmployeeInformation> _EmployeeInformationRepository;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IEmployeeInformationService _employeeInformationService;

        public EncashmentService(
             IRepositoryAsync<EmployeeInformation> EmployeeInformationRepository
             , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IEmployeeInformationService employeeInformationService
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            )
        {
            _EmployeeInformationRepository = EmployeeInformationRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _employeeInformationService = employeeInformationService;
            _companyParallelCurrencyService = companyParallelCurrencyService;
        }

        #endregion



        #region  Hourly ot report Monthly ------------------------------------------
        public string NumberFormatTwoDecimal = "#,##0.00;(#,##0.00)";
        public IWorkbook GetEncashReport(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string YearNo)
        {
            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsEncashment = null;
            DataTable dtEncashment = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;
            #endregion
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;

                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                objRpt.GetEncashReport(YearNo, PlantId, CompanyId, CompanyGroupId, out dsEncashment);
                if (dsEncashment.Tables[0].Rows.Count == 0)
                {
                    throw new CustomException("No Data Found....");
                }
                dtEncashment = dsEncashment.Tables[0];
                
                objRpt.SelectedPlantWiseCompany(PlantId, out dsCmp);
                objRpt.SelectedPlant(PlantId, out dsFactory);
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
                var iDays = 0;
                var iEncashmentDate = 0;
                var iRate = 0;
                var iAmount = 0;
                var iDIS = 0;
                var iLine = 0;
                var totalAmount = 0.00;
                var iDOJ = 0;
                var iDOS = 0;
                var iDepartment = 0;
                var iYear = 0;
                //var iLeaveType = 0;
                var iDesignation = 0;
                var iBF = 0;
                var iEarnLeave = 0;
                var iAvailed = 0;
                //var iBlance = 0;
                var iPaymentMode = 0;
                var iBankName = 0;
                var iBankBranchName = 0;
                var iBasicAmmount = 0;
                var iGrossAmmount = 0;
                var icarryforward = 0;

                var isl = 0;
                var SLNo = 1;


                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];
                xlsRow = 5;

                #region ------------------Column Header------------------
                isl = xlsCol;
                sheet1.Range[xlsRow, isl].Text = "SL";
                sheet1.Range[xlsRow, isl].ColumnWidth = 7;

                xlsCol += 1;
                iEmployeeCode = xlsCol;
                sheet1.Range[xlsRow, iEmployeeCode].Text = "Emp Code";
                sheet1.Range[xlsRow, iEmployeeCode].ColumnWidth = 10;

                xlsCol += 1;
                iName = xlsCol;
                sheet1.Range[xlsRow, iName].Text = "Emp Name";
                sheet1.Range[xlsRow, iName].ColumnWidth = 25;

                xlsCol += 1;
                iDOJ = xlsCol;
                sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                sheet1.Range[xlsRow, iDOJ].ColumnWidth = 20;

                xlsCol += 1;
                iDOS = xlsCol;
                sheet1.Range[xlsRow, iDOS].Text = "DOS";
                sheet1.Range[xlsRow, iDOS].ColumnWidth = 20;

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
                iLine = xlsCol;
                sheet1.Range[xlsRow, iLine].Text = "Line";
                sheet1.Range[xlsRow, iLine].ColumnWidth = 12;


                xlsCol += 1;
                iYear = xlsCol;
                sheet1.Range[xlsRow, iYear].Text = "Year";
                sheet1.Range[xlsRow, iYear].ColumnWidth = 15;

                xlsCol += 1;
                iEncashmentDate = xlsCol;
                sheet1.Range[xlsRow, iEncashmentDate].Text = "Encashment Date";
                sheet1.Range[xlsRow, iEncashmentDate].ColumnWidth = 15;

                xlsCol += 1;
                iBasicAmmount = xlsCol;
                sheet1.Range[xlsRow, iBasicAmmount].Text = "Basic Amount";
                sheet1.Range[xlsRow, iBasicAmmount].ColumnWidth = 17;

                xlsCol += 1;
                iGrossAmmount = xlsCol;
                sheet1.Range[xlsRow, iGrossAmmount].Text = "Gross Amount";
                sheet1.Range[xlsRow, iGrossAmmount].ColumnWidth = 17;


                xlsCol += 1;
                iBF = xlsCol;
                sheet1.Range[xlsRow, iBF].Text = "Brought Forward";
                sheet1.Range[xlsRow, iBF].ColumnWidth = 15;


                xlsCol += 1;
                iEarnLeave = xlsCol;
                sheet1.Range[xlsRow, iEarnLeave].Text = "Earn Leave";
                sheet1.Range[xlsRow, iEarnLeave].ColumnWidth = 15;

                xlsCol += 1;
                iAvailed = xlsCol;
                sheet1.Range[xlsRow, iAvailed].Text = "Availed";
                sheet1.Range[xlsRow, iAvailed].ColumnWidth = 15;

                xlsCol += 1;
                iDays = xlsCol;
                sheet1.Range[xlsRow, iDays].Text = "Encashed Days";
                sheet1.Range[xlsRow, iDays].ColumnWidth = 15;

                //xlsCol += 1;
                //iBlance = xlsCol;
                //sheet1.Range[xlsRow, iBlance].Text = "Balance";
                //sheet1.Range[xlsRow, iBlance].ColumnWidth = 15;

                xlsCol += 1;
                icarryforward = xlsCol;
                sheet1.Range[xlsRow, icarryforward].Text = "Carry Forward";
                sheet1.Range[xlsRow, icarryforward].ColumnWidth = 15;

                //xlsCol += 1;
                //iLeaveType = xlsCol;
                //sheet1.Range[xlsRow, iLeaveType].Text = "Leave Type";
                //sheet1.Range[xlsRow, iLeaveType].ColumnWidth = 15;

                xlsCol += 1;
                iRate = xlsCol;
                sheet1.Range[xlsRow, iRate].Text = "Rate";
                sheet1.Range[xlsRow, iRate].ColumnWidth = 15;

                xlsCol += 1;
                iAmount = xlsCol;
                sheet1.Range[xlsRow, iAmount].Text = "Amount";
                sheet1.Range[xlsRow, iAmount].ColumnWidth = 15;

                xlsCol += 1;
                iPaymentMode = xlsCol;
                sheet1.Range[xlsRow, iPaymentMode].Text = "Payment Mode";
                sheet1.Range[xlsRow, iPaymentMode].ColumnWidth = 15;

                xlsCol += 1;
                iBankName = xlsCol;
                sheet1.Range[xlsRow, iBankName].Text = "Bank Name";
                sheet1.Range[xlsRow, iBankName].ColumnWidth = 24;

                xlsCol += 1;
                iBankBranchName = xlsCol;
                sheet1.Range[xlsRow, iBankBranchName].Text = "Bank Branch Name";
                sheet1.Range[xlsRow, iBankBranchName].ColumnWidth = 24;

                xlsCol += 1;
                iDIS = xlsCol;
                sheet1.Range[xlsRow, iDIS].Text = "Disbursed";
                sheet1.Range[xlsRow, iDIS].ColumnWidth = 15;

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                xlsRow++;

                #endregion ------------------Column Header------------------

                for (int i = 0; i < dtEncashment.Rows.Count; i++)
                {
                    #region ----------------------Data-----------------------       

                    sheet1.Range[xlsRow, isl].Text = SLNo.ToString();
                    sheet1.Range[xlsRow, iName].Text = dtEncashment.Rows[i]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, iEmployeeCode].Text = dtEncashment.Rows[i]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, iDOJ].Text = dtEncashment.Rows[i]["DOJ"].ToString();
                    sheet1.Range[xlsRow, iDOS].Text = dtEncashment.Rows[i]["DOS"].ToString();
                    sheet1.Range[xlsRow, iDesignation].Text = dtEncashment.Rows[i]["Designation"].ToString();
                    sheet1.Range[xlsRow, iDepartment].Text = dtEncashment.Rows[i]["Department"].ToString();
                    sheet1.Range[xlsRow, iSection].Text = dtEncashment.Rows[i]["Section"].ToString();
                    sheet1.Range[xlsRow, iSubSection].Text = dtEncashment.Rows[i]["SubSection"].ToString();
                    sheet1.Range[xlsRow, iLine].Text = dtEncashment.Rows[i]["Line"].ToString();
                    sheet1.Range[xlsRow, iBF].Number = clsStaticInfo.dbl(dtEncashment.Rows[i]["BroughtForward"].ToString());
                    sheet1.Range[xlsRow, iEarnLeave].Text = dtEncashment.Rows[i]["DaysCanBeSanctioned"].ToString();
                    sheet1.Range[xlsRow, iAvailed].Number = clsStaticInfo.dbl(dtEncashment.Rows[i]["AvailedLeave"].ToString());
                    //sheet1.Range[xlsRow, iBlance].Text = dtEncashment.Rows[i]["Balance"].ToString();
                    sheet1.Range[xlsRow, iPaymentMode].Text = dtEncashment.Rows[i]["PaymentMode"].ToString();
                    sheet1.Range[xlsRow, iBankName].Text = dtEncashment.Rows[i]["BankName"].ToString();
                    sheet1.Range[xlsRow, iBankBranchName].Text = dtEncashment.Rows[i]["BankBranchName"].ToString();
                    sheet1.Range[xlsRow, iBasicAmmount].Number = clsStaticInfo.dbl(dtEncashment.Rows[i]["BasicAmmount"].ToString());
                    sheet1.Range[xlsRow, iGrossAmmount].Number = clsStaticInfo.dbl(dtEncashment.Rows[i]["GrossAmmount"].ToString());
                   
                    sheet1.Range[xlsRow, iEncashmentDate].Text = dtEncashment.Rows[i]["EncashmentDate"].ToString();
                    sheet1.Range[xlsRow, iYear].Number = clsStaticInfo.dbl(dtEncashment.Rows[i]["YearNo"].ToString());
                    sheet1.Range[xlsRow, iDIS].Text = dtEncashment.Rows[i]["Disbus"].ToString();
                    //sheet1.Range[xlsRow, iLeaveType].Text = dtEncashment.Rows[i]["LeaveType"].ToString();

                    sheet1.Range[xlsRow, icarryforward].Number = clsStaticInfo.dbl(dtEncashment.Rows[i]["CarryForward"].ToString());
                    sheet1.Range[xlsRow, icarryforward].NumberFormat = oru.NumberFormatDecimalTwo();

                    sheet1.Range[xlsRow, iDays].Number = clsStaticInfo.dbl(dtEncashment.Rows[i]["Days"].ToString());
                    sheet1.Range[xlsRow, iDays].NumberFormat = oru.NumberFormatDecimalTwo();
                    //Rate
                    sheet1.Range[xlsRow, iRate].Number = clsStaticInfo.dbl(dtEncashment.Rows[i]["Rate"].ToString());
                    sheet1.Range[xlsRow, iRate].NumberFormat = oru.NumberFormatDecimalTwo();
                    //Amount
                    sheet1.Range[xlsRow, iAmount].Number = clsStaticInfo.dbl((clsStaticInfo.dbl(dtEncashment.Rows[i]["Days"].ToString()) * clsStaticInfo.dbl(dtEncashment.Rows[i]["Rate"].ToString())).ToString());
                    sheet1.Range[xlsRow, iAmount].NumberFormat = oru.NumberFormatDecimalZero();

                    totalAmount += clsStaticInfo.dbl((clsStaticInfo.dbl(dtEncashment.Rows[i]["Days"].ToString()) * clsStaticInfo.dbl(dtEncashment.Rows[i]["Rate"].ToString())).ToString());
                    sheet1.Range[xlsRow, iAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, iAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow++;
                    SLNo++;
                       #endregion ----------------------Data-----------------------
                }

                sheet1.Range[xlsRow, iRate].Text = "Total";
                sheet1.Range[xlsRow, iRate + 1].Number = totalAmount;

                sheet1.Range[xlsRow, iRate, xlsRow, iRate + 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, iRate, xlsRow, iRate + 1].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, iRate, xlsRow, iRate + 1].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, iRate, xlsRow, iRate + 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, iRate, xlsRow, iRate + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet1.Range[xlsRow, iRate, xlsRow, iRate + 1].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                sheet1.Range[xlsRow, iRate, xlsRow, iRate + 1].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

             
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
                sheet1.Range[xlsRow, 3].Text = "Encashment Report For " + dtEncashment.Rows[0]["YearNo"].ToString();
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
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Encashment Report";
                #endregion Page Setup

               

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void GenerateRate(DataTable DTAllowPolicy, DataSet dsSalaryStruc, string _currencyId, out Dictionary<string, double> dicNW)
        {
            double nwRate = 0;
            dicNW = null;

            try
            {
                DataTable dtemp = new DataView(DTAllowPolicy).ToTable(true, "SystemID");
                dicNW = new Dictionary<string, double>();
                for (int i = 0; i < dtemp.Rows.Count; i++)
                {
                    string _empid = dtemp.Rows[i]["SystemID"].ToString();
                    //if (_empid != "205330")
                    //{
                        GetFormulaAllRate(DTAllowPolicy, dsSalaryStruc, _currencyId, _empid, out nwRate);
                        dicNW.Add(_empid, nwRate);
                    //}
                    
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void GetFormulaAllRate(DataTable dsPolicy, DataSet dsSalaryStruc, string _currencyId, string empid, out double nwRate)
        {
            nwRate = 0;
            string FormulaDesIDFromRate = string.Empty;
            try
            {
                DataView dv = new DataView(dsPolicy);
                dv.RowFilter = "SystemID='" + empid + "'";
                if (dv.Count > 0)
                {
                    //"LvEncashmentFormulaDesID", "SystemId","EmployeeCode"                 
                    FormulaDesIDFromRate = dv[0]["LvEncashmentFormulaDesID"].ToString();
                    string EmployeeCode = dv[0]["EmployeeCode"].ToString();
                    string formula = string.Empty;
                    formula = FormulaDesIDFromRate;

                    if (string.IsNullOrEmpty(formula))
                    {
                        throw new Exception("Employee " + EmployeeCode + " has no Rate Formaula in allowance setting ...");
                    }
                    DataView dvss = new DataView(dsSalaryStruc.Tables[0]);
                    dvss.RowFilter = "EmpInfoSystemID='" + empid + "'";
                    if (dvss.Count > 0)
                    {
                        string FormulaValue = string.Empty;
                        DataTable dtValue = dvss.ToTable();
                        DataTable dtSalaryHead = dvss.ToTable(true, "SalaryHeadID", "SalaryHead");

                        GetFormulValue(formula, ref dtValue, _currencyId, out nwRate, ref dtSalaryHead);
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void GetFormulValue(string FormulaDesIDN, ref DataTable dtValue, string _currencyId, out double nwRate, ref DataTable dtSalaryHead)
        {
            string FormulaValue = string.Empty;
            nwRate = 0;
            try
            {
                clsSalaryUtility su = new clsSalaryUtility();
                su.ReLoadFormulaWithValue(FormulaDesIDN, ref dtValue, _currencyId, "1", out FormulaValue, ref dtSalaryHead);
                string sFormulaResult = clsSalaryStructureAplos.Evaluate(FormulaValue).ToString();
                if (sFormulaResult == "NaN")
                {
                    throw new Exception("Salary Head is not orderly tagged in Salary Rule");
                }

                //get formula wise value
                var vv = Convert.ToDouble(sFormulaResult).ToString("00.00");
                nwRate = Convert.ToDouble(vv);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IWorkbook GetEarnLeaveReport(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string YearNo, bool isDetail, bool isActive, bool isSeperated)
        {
            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsEarnLeave = null;
            DataTable dtEarnLeave = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsCurrency = null;
            string _currencyId = string.Empty;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;
            Dictionary<string, double> dicNW = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            var workbook = oru.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            #endregion
            try
            {



                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                objRpt.GetEarnLeaveReport(YearNo, PlantId, CompanyId, CompanyGroupId, isActive, isSeperated, out dsEarnLeave);
                dtEarnLeave = dsEarnLeave.Tables[0];

                DateTime today = DateTime.Today;
                string CurrentDate = Convert.ToDateTime(today).ToString("dd-MMM-yyyy");

                DataSet dsSStructure = null;
                clsOTCalculation otc = new clsOTCalculation();
                //otc.LoadSalaryStructure(PlantId, CurrentDate, CurrentDate, out dsSStructure);
                Dictionary<string, List<DataRow>> dicEmpSalry = GetEmpSalaryInformation(PlantId, CurrentDate, CurrentDate, out dsSStructure);

                DataTable DTAllowPolicy = new DataView(dsEarnLeave.Tables[0]).ToTable(true, "LvEncashmentFormulaDesID", "SystemId", "EmployeeCode");

                objRpt.SelectedPlantWiseCompany(PlantId, out dsCmp);
                objRpt.SelectedPlant(PlantId, out dsFactory);
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                string companyId = identity.CompanyId;
                int iName = 0;
                int iEmployeeCode = 0;
                int iSubSection = 0;
                int iSection = 0;
                int ipolicyname = 0;
                int iLine = 0;
                int iDOJ = 0;
                int iDOS = 0;
                int iDepartment = 0;
                int iYear = 0;
                int iDesignation = 0;
                int iBF = 0;
                int iEarnLeave = 0;
                int iAvailed = 0;
                int iBlance = 0;
                int iEncashed = 0;
                int iWages = 0;
                int iGross = 0;
                int iRate = 0;
                int iAmount = 0;
                int isl = 0;
                int iYearEndLapse = 0;
                int SLNo = 1;
                int iFatherName = 0;
                int iDOB = 0;


                if (dsEarnLeave.Tables[0].Rows.Count == 0)
                {
                    throw new CustomException("No Data Found....");
                }

                clsSalaryInfo objSal = new clsSalaryInfo();
                objSal.GetLocalCurrency(CompanyGroupId, PlantId, out dsCurrency);
                if (dsCurrency.Tables[0].Rows.Count > 0)
                {
                    _currencyId = "" + dsCurrency.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                }
                else
                {
                    throw new Exception("No currency found...");
                }

                GenerateRate(DTAllowPolicy, dsSStructure, _currencyId, out dicNW);


                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];
                xlsRow = 5;

                #region ------------------Column Header------------------
                if (isDetail == true)
                {
                    isl = xlsCol;
                    sheet1.Range[xlsRow, isl].Text = "SL";
                    sheet1.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet1.Range[xlsRow, iEmployeeCode].Text = "Emp Code";
                    sheet1.Range[xlsRow, iEmployeeCode].ColumnWidth = 10;

                    xlsCol += 1;
                    iName = xlsCol;
                    sheet1.Range[xlsRow, iName].Text = "Emp Name";
                    sheet1.Range[xlsRow, iName].ColumnWidth = 25;

                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet1.Range[xlsRow, iDOJ].ColumnWidth = 20;

                    xlsCol += 1;
                    iDOS = xlsCol;
                    sheet1.Range[xlsRow, iDOS].Text = "DOS";
                    sheet1.Range[xlsRow, iDOS].ColumnWidth = 20;

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
                    iLine = xlsCol;
                    sheet1.Range[xlsRow, iLine].Text = "Line";
                    sheet1.Range[xlsRow, iLine].ColumnWidth = 12;


                    xlsCol += 1;
                    iYear = xlsCol;
                    sheet1.Range[xlsRow, iYear].Text = "Year";
                    sheet1.Range[xlsRow, iYear].ColumnWidth = 15;

                    xlsCol += 1;
                    iWages = xlsCol;
                    sheet1.Range[xlsRow, iWages].Text = "Wages";
                    sheet1.Range[xlsRow, iWages].ColumnWidth = 15;

                    //xlsCol += 1;
                    //iGross = xlsCol;
                    //sheet1.Range[xlsRow, iGross].Text = "Gross";
                    //sheet1.Range[xlsRow, iBasic].ColumnWidth = 15;


                    xlsCol += 1;
                    iBF = xlsCol;
                    sheet1.Range[xlsRow, iBF].Text = "Brought Forward";
                    sheet1.Range[xlsRow, iBF].ColumnWidth = 15;


                    xlsCol += 1;
                    iEarnLeave = xlsCol;
                    sheet1.Range[xlsRow, iEarnLeave].Text = "Earned Leave";
                    sheet1.Range[xlsRow, iEarnLeave].ColumnWidth = 15;

                    xlsCol += 1;
                    iAvailed = xlsCol;
                    sheet1.Range[xlsRow, iAvailed].Text = "Availed";
                    sheet1.Range[xlsRow, iAvailed].ColumnWidth = 15;

                    xlsCol += 1;
                    iEncashed = xlsCol;
                    sheet1.Range[xlsRow, iEncashed].Text = "Encashed";
                    sheet1.Range[xlsRow, iEncashed].ColumnWidth = 15;

                    xlsCol += 1;
                    iBlance = xlsCol;
                    sheet1.Range[xlsRow, iBlance].Text = "Balance";
                    sheet1.Range[xlsRow, iBlance].ColumnWidth = 15;

                    xlsCol += 1;
                    iYearEndLapse = xlsCol;
                    sheet1.Range[xlsRow, iYearEndLapse].Text = "Year End Lapse";
                    sheet1.Range[xlsRow, iYearEndLapse].ColumnWidth = 15;

                    xlsCol += 1;
                    iRate = xlsCol;
                    sheet1.Range[xlsRow, iRate].Text = "Rate";
                    sheet1.Range[xlsRow, iRate].ColumnWidth = 15;

                    xlsCol += 1;
                    iAmount = xlsCol;
                    sheet1.Range[xlsRow, iAmount].Text = "Amount";
                    sheet1.Range[xlsRow, iAmount].ColumnWidth = 15;

                    xlsCol += 1;
                    ipolicyname = xlsCol;
                    sheet1.Range[xlsRow, ipolicyname].Text = "Policy Name";
                    sheet1.Range[xlsRow, ipolicyname].ColumnWidth = 15;
                }
                else
                {
                    isl = xlsCol;
                    sheet1.Range[xlsRow, isl].Text = "SL";
                    sheet1.Range[xlsRow, isl].ColumnWidth = 7;
                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet1.Range[xlsRow, iEmployeeCode].Text = "Emp Code";
                    sheet1.Range[xlsRow, iEmployeeCode].ColumnWidth = 10;

                    xlsCol += 1;
                    iName = xlsCol;
                    sheet1.Range[xlsRow, iName].Text = "Emp Name";
                    sheet1.Range[xlsRow, iName].ColumnWidth = 25;

                    xlsCol += 1;
                    iFatherName = xlsCol;
                    sheet1.Range[xlsRow, iFatherName].Text = "Father Name";
                    sheet1.Range[xlsRow, iFatherName].ColumnWidth = 25;
                    xlsCol += 1;
                    iDOB = xlsCol;
                    sheet1.Range[xlsRow, iDOB].Text = "DOB";
                    sheet1.Range[xlsRow, iDOB].ColumnWidth = 20;

                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet1.Range[xlsRow, iDOJ].ColumnWidth = 20;

                    xlsCol += 1;
                    iDOS = xlsCol;
                    sheet1.Range[xlsRow, iDOS].Text = "DOS";
                    sheet1.Range[xlsRow, iDOS].ColumnWidth = 20;






                    xlsCol += 1;
                    iWages = xlsCol;
                    sheet1.Range[xlsRow, iWages].Text = "Wages";
                    sheet1.Range[xlsRow, iWages].ColumnWidth = 15;




                    //xlsCol += 1;
                    //iEarnLeave = xlsCol;
                    //sheet1.Range[xlsRow, iEarnLeave].Text = "Earned Leave";
                    //sheet1.Range[xlsRow, iEarnLeave].ColumnWidth = 15;



                    //xlsCol += 1;
                    //iEncashed = xlsCol;
                    //sheet1.Range[xlsRow, iEncashed].Text = "Encashed";
                    //sheet1.Range[xlsRow, iEncashed].ColumnWidth = 15;

                    xlsCol += 1;
                    iBlance = xlsCol;
                    sheet1.Range[xlsRow, iBlance].Text = "Balance";
                    sheet1.Range[xlsRow, iBlance].ColumnWidth = 15;





                    xlsCol += 1;
                    iAmount = xlsCol;
                    sheet1.Range[xlsRow, iAmount].Text = "Amount";
                    sheet1.Range[xlsRow, iAmount].ColumnWidth = 15;


                }


                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                xlsRow++;
                double wagesRate = 0.00;
                string strReplace = "";
                #endregion ------------------Column Header------------------

                for (int i = 0; i < dtEarnLeave.Rows.Count; i++)
                {
                    #region ----------------------Data-----------------------       
                    string EmpSystemid = dtEarnLeave.Rows[i]["SystemId"].ToString();


                    if (isDetail == true)
                    {
                        sheet1.Range[xlsRow, isl].Text = SLNo.ToString();
                        sheet1.Range[xlsRow, iName].Text = dtEarnLeave.Rows[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, iEmployeeCode].Text = dtEarnLeave.Rows[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, iDOJ].Text = dtEarnLeave.Rows[i]["DOJ"].ToString();
                        sheet1.Range[xlsRow, iDOS].Text = dtEarnLeave.Rows[i]["DOS"].ToString();
                        sheet1.Range[xlsRow, iDesignation].Text = dtEarnLeave.Rows[i]["Designation"].ToString();
                        sheet1.Range[xlsRow, iDepartment].Text = dtEarnLeave.Rows[i]["Department"].ToString();
                        sheet1.Range[xlsRow, iSection].Text = dtEarnLeave.Rows[i]["Section"].ToString();
                        sheet1.Range[xlsRow, iSubSection].Text = dtEarnLeave.Rows[i]["SubSection"].ToString();
                        sheet1.Range[xlsRow, iLine].Text = dtEarnLeave.Rows[i]["Line"].ToString();
                        sheet1.Range[xlsRow, iBF].Number = clsStaticInfo.dbl(dtEarnLeave.Rows[i]["BroughtForward"].ToString());
                        sheet1.Range[xlsRow, iEarnLeave].Number = clsStaticInfo.dbl(dtEarnLeave.Rows[i]["DaysCanBeSanctioned"].ToString());
                        sheet1.Range[xlsRow, iEarnLeave].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        //sheet1.Range[xlsRow, iEarnLeave].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, iAvailed].Number = clsStaticInfo.dbl(dtEarnLeave.Rows[i]["AvailedLeave"].ToString());

                        sheet1.Range[xlsRow, iBlance].Number = clsStaticInfo.dbl(dtEarnLeave.Rows[i]["Balance"].ToString());

                        sheet1.Range[xlsRow, iYear].Number = clsStaticInfo.dbl(dtEarnLeave.Rows[i]["YearNo"].ToString());
                        sheet1.Range[xlsRow, ipolicyname].Text = dtEarnLeave.Rows[i]["PolicyName"].ToString();
                        sheet1.Range[xlsRow, iEncashed].Number = clsStaticInfo.dbl(dtEarnLeave.Rows[i]["EncashedInbetween"].ToString());

                        sheet1.Range[xlsRow, iYearEndLapse].Number = clsStaticInfo.dbl(dtEarnLeave.Rows[i]["YearEndLapse"].ToString());
                        strReplace = dtEarnLeave.Rows[i]["LvEncashmentFormulaDesID"].ToString();
                        string[] allTexts = strReplace.Split(' ');

                        if (dicEmpSalry.ContainsKey(EmpSystemid))
                        {
                            List<DataRow> drSalary = dicEmpSalry[EmpSystemid];
                            wagesRate = 0.00;

                            for (int ic = 0; ic < drSalary.Count; ic++)
                            {
                                for (int alt = 0; alt < allTexts.Length; alt++)
                                {
                                    if (allTexts[alt].ToString().ToUpper() == drSalary[ic]["SalaryHeadID"].ToString().ToUpper())
                                    {
                                        strReplace = strReplace.Replace(drSalary[ic]["SalaryHeadID"].ToString().ToUpper(), drSalary[ic]["Amount"].ToString());
                                        wagesRate += clsStaticInfo.dbl(drSalary[ic]["Amount"].ToString());
                                    }
                                }

                            }
                            object value = null;
                            try
                            {

                                DataTable dt = new DataTable();
                                value = dt.Compute(strReplace, "");

                            }
                            catch (Exception ex)
                            {

                                throw ex;
                            }
                            finally
                            {
                            }

                        }

                        //rate
                        sheet1.Range[xlsRow, iWages].Number = wagesRate;

                        sheet1.Range[xlsRow, iRate].Number = clsStaticInfo.dbl(dicNW[EmpSystemid]);

                        //Amount
                        sheet1.Range[xlsRow, iAmount].Number = clsStaticInfo.dbl(dicNW[EmpSystemid]) * clsStaticInfo.dbl(dtEarnLeave.Rows[i]["Balance"].ToString());
                        sheet1.Range[xlsRow, iAmount].NumberFormat = NumberFormatTwoDecimal;
                        sheet1.Range[xlsRow, iWages].NumberFormat = NumberFormatTwoDecimal;
                        sheet1.Range[xlsRow, iEncashed].NumberFormat = NumberFormatTwoDecimal;
                        sheet1.Range[xlsRow, iBlance].NumberFormat = NumberFormatTwoDecimal;
                        sheet1.Range[xlsRow, iAvailed].NumberFormat = NumberFormatTwoDecimal;
                        sheet1.Range[xlsRow, iEarnLeave].NumberFormat = NumberFormatTwoDecimal;
                        sheet1.Range[xlsRow, iBF].NumberFormat = NumberFormatTwoDecimal;
                    }
                    else
                    {
                        sheet1.Range[xlsRow, isl].Text = SLNo.ToString();

                        sheet1.Range[xlsRow, iEmployeeCode].Text = dtEarnLeave.Rows[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, iName].Text = dtEarnLeave.Rows[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, iFatherName].Text = dtEarnLeave.Rows[i]["FatherName"].ToString();
                        sheet1.Range[xlsRow, iDOB].Text = dtEarnLeave.Rows[i]["DOB"].ToString();
                        sheet1.Range[xlsRow, iDOJ].Text = dtEarnLeave.Rows[i]["DOJ"].ToString();

                        sheet1.Range[xlsRow, iDOS].Text = dtEarnLeave.Rows[i]["DOS"].ToString();
                        sheet1.Range[xlsRow, iBlance].Number = clsStaticInfo.dbl(dtEarnLeave.Rows[i]["Balance"].ToString());

                        strReplace = dtEarnLeave.Rows[i]["LvEncashmentFormulaDesID"].ToString();
                        string[] allTexts = strReplace.Split(' ');

                        if (dicEmpSalry.ContainsKey(EmpSystemid))
                        {
                            List<DataRow> drSalary = dicEmpSalry[EmpSystemid];
                            wagesRate = 0.00;

                            for (int ic = 0; ic < drSalary.Count; ic++)
                            {
                                for (int alt = 0; alt < allTexts.Length; alt++)
                                {
                                    if (allTexts[alt].ToString().ToUpper() == drSalary[ic]["SalaryHeadID"].ToString().ToUpper())
                                    {
                                        strReplace = strReplace.Replace(drSalary[ic]["SalaryHeadID"].ToString().ToUpper(), drSalary[ic]["Amount"].ToString());
                                        wagesRate += clsStaticInfo.dbl(drSalary[ic]["Amount"].ToString());
                                    }
                                }

                            }
                            object value = null;
                            try
                            {

                                DataTable dt = new DataTable();
                                value = dt.Compute(strReplace, "");

                            }
                            catch (Exception ex)
                            {

                                throw ex;
                            }
                            finally
                            {
                                sheet1.Range[xlsRow, iWages].Number = wagesRate;


                            }

                        }


                        //rate

                        //Amount
                        sheet1.Range[xlsRow, iAmount].Number = clsStaticInfo.dbl(dicNW[EmpSystemid]) * clsStaticInfo.dbl(dtEarnLeave.Rows[i]["Balance"].ToString());
                        sheet1.Range[xlsRow, iAmount].NumberFormat = NumberFormatTwoDecimal;
                        sheet1.Range[xlsRow, iWages].NumberFormat = NumberFormatTwoDecimal;
                        sheet1.Range[xlsRow, iBlance].NumberFormat = NumberFormatTwoDecimal;
                    }



                    xlsRow++;
                    SLNo++;
                }

                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                #endregion ----------------------Data-----------------------

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
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
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
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
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
                sheet1.Range[xlsRow, 3].Text = "Earn Leave Report For " + dtEarnLeave.Rows[0]["YearNo"].ToString();
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A6"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

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
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Earn Leave Report";
                #endregion Page Setup



                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public Dictionary<string, List<DataRow>> GetEmpSalaryInformation(string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            Dictionary<string, List<DataRow>> dicSalaryStructure = new Dictionary<string, List<DataRow>>();

            clsStaticInfo obs = null;
            try
            {

                obs = new clsStaticInfo();
                string strSQL = @"
                            select m.*,d.EntryAmount Amount, d.DefineAmount,d.SalaryHeadID,d.EntryCurrencyID,h.SalaryHead,h.HeadCategory,h.HeadType from
                             (---m
                            select max(ed) ed,EmpInfoSystemID from
                            (
                            select EmpInfoSystemID, max(EffectiveDate) ed from SalaryInfoDefineMaster where IsApproved = 1 and EffectiveDate<= '" + sToDate + @"' and plantid = '" + sPlantID + @"' group by EmpInfoSystemID
                                                                                  union
                            select EmpInfoSystemID, max(EffectiveDate) ed from SalaryInfoBackMaster where IsApproved = 1 and EffectiveDate<= '" + sToDate + @"' and plantid = '" + sPlantID + @"'  group by EmpInfoSystemID
                            ) x
                            group by EmpInfoSystemID
                            ) ---m
                            mx
                            left join(
                            select SystemID, EmpInfoSystemID, EffectiveDate  from SalaryInfoDefineMaster where IsApproved = 1 and EffectiveDate <= '" + sToDate + @"' and plantid = '" + sPlantID + @"'
                            union
                            select SystemID, EmpInfoSystemID, EffectiveDate  from SalaryInfoBackMaster where IsApproved = 1 and EffectiveDate <= '" + sToDate + @"' and plantid = '" + sPlantID + @"'
                            )
                             m on m.EmpInfoSystemID = mx.EmpInfoSystemID and m.EffectiveDate = mx.ed
                            left join(
                            select systemid, EntryAmount, DefineAmount, SalaryID, SalaryHeadID, EntryCurrencyID from SalaryInfoDefine
                            union
                            select systemid, EntryAmount, DefineAmount, SalaryID, SalaryHeadID, EntryCurrencyID from SalaryInfoBack
                            )
                            d on d.SalaryID = m.SystemID
                            left join SalaryHead h on h.SalaryHeadID = d.SalaryHeadID
                            order by m.EmpInfoSystemID";

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
                        dicSalaryStructure.Add(dt.Rows[i]["EmpInfoSystemID"].ToString(), _data);
                    }
                    _data.Add(dt.Rows[i]);

                    empId = dt.Rows[i]["EmpInfoSystemID"].ToString();
                }

                return dicSalaryStructure;
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

    }

}