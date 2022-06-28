#region Using

using clsAttendance;
using Library.Core;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.Setups;
using Library.Service.Currencies;
using Library.Service.Helpers;
using Library.Service.Organizations;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using static Library.Service.Helpers.ReportUtility;

#endregion Using

namespace Library.Service.HumanResources
{
    public class SFBonusSheetReportService : ISFBonusSheetReportService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<EmployeeInformation> _EmployeeInformationRepository;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IPlantService _plantService;

        public SFBonusSheetReportService(
             IRepositoryAsync<EmployeeInformation> EmployeeInformationRepository
             , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , IPlantService plantService
            )
        {
            _EmployeeInformationRepository = EmployeeInformationRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _plantService = plantService;
        }

        #endregion Constructor


        public IWorkbook GetSFBonusSheet(string companyGroupId, string companyId, string plantId, string languageId, string paymentMode, string payGroup, string bonusPointId, string bonusType)
        {
            try
            {
                var reportUtility = new ReportUtility();
                var excelEngine = new ExcelEngine();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Bonus Sheet";
                DataTable dtLocal = null;
                var objRpt = new clsReport();
                DataSet dsCmp = null;
                var FactoryName = string.Empty;
                var CmpName = string.Empty;
                var indexS = bonusPointId.IndexOf("__");
                var policyId = bonusPointId.Substring(0, indexS);
                var cutoffdate = bonusPointId.Substring(indexS + 2);

                dtLocal = GetSFBonusSheetData(plantId, payGroup, cutoffdate, policyId, languageId, paymentMode, bonusType);
                if (dtLocal.Rows.Count == 0)
                {
                    throw new Exception("No Data Found.");
                }

                bool isLocalLanguage = false;
                var localLanguage = reportUtility.LocalLanguageListSql(plantId, languageId, out isLocalLanguage);

                var labelList = reportUtility.LocalLanguageLabelList(plantId, languageId);

                var row = 0;
                var xlscol = 1;
                var colSL = 0;
                var colIDNO = 0;
                var colNAME = 0;
                var colDESIGNATION = 0;
                var colGROSS = 0;
                var colBASIC = 0;
                var colJOINDATE = 0;
                var colServiceLength = 0;
                var colBONUS = 0;
                var colBONUSAMOUNT = 0;
                var colSTAMPDEDUCTION = 0;
                var colNETPAYABLE = 0;
                var colSIGNATURE = 0;
                var colBankName = 0;
                var colBankAccNo = 0;


                var stampDed = 0;
                var endXlsCol = 0;

                if (paymentMode == "Bank")
                {
                    stampDed = 0;
                }
                else
                {
                    stampDed = 10;
                }

                var DTpayGroup = reportUtility.payRollGroup(payGroup);

                var payGroupName = "";
                if (DTpayGroup.Rows.Count > 0)
                {
                    payGroupName = DTpayGroup.Rows[0]["UserName"].ToString();
                }
                row = 5;
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.SrNo.ToString(), "SL"), 6, 15); colSL = xlscol; xlscol++;
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.IDNo.ToString(), "ID NO."), 15, 15); colIDNO = xlscol; xlscol++;
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.Name.ToString(), "Name"), 43, 15); colNAME = xlscol; xlscol++;
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.Designation.ToString(), "Designation"), 27, 15); colDESIGNATION = xlscol; xlscol++;
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.TotalSalary.ToString(), "Total Salary"), 16, 15); colGROSS = xlscol; xlscol++;
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.Basic.ToString(), "Basic"), 16, 15); colBASIC = xlscol; xlscol++;
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.DOJ.ToString(), "DOJ"), 31, 15); colJOINDATE = xlscol; xlscol++;
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.ServiceLengthDays.ToString(), "Service Length"), 16, 15); colServiceLength = xlscol; xlscol++;

                if (bonusType == "Percentage")
                {
                    reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.Bonus.ToString(), "Bonus") + "%", 16, 15); colBONUS = xlscol; xlscol++;
                }
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.BonusAmount.ToString(), "Bonus Amount"), 16, 15); colBONUSAMOUNT = xlscol; xlscol++;
                if (paymentMode != "Bank")
                {
                    reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.StampDeduction.ToString(), "Stamp Deduction"), 16, 15); colSTAMPDEDUCTION = xlscol; xlscol++;
                }
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.NetPayable.ToString(), "Net Payable"), 16, 15); colNETPAYABLE = xlscol; xlscol++;
                if (paymentMode == "Bank")
                {
                    reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.BankName.ToString(), "Bank Name"), 34, 15); colBankName = xlscol; xlscol++;
                    reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.BankAccountNo.ToString(), "Bank Acc. No"), 34, 15); colBankAccNo = xlscol; xlscol++;
                }
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.Signature.ToString(), "Signature"), 37, 15); colSIGNATURE = xlscol;
                endXlsCol = colSIGNATURE;
                sheet.Range[row - 1, colNETPAYABLE].Text = "Cutt off Date: " + cutoffdate;
                sheet.Range[row - 1, colNETPAYABLE].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                sheet.Range[row - 1, colNETPAYABLE].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[row - 1, colNETPAYABLE].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row - 1, colNETPAYABLE].RowHeight = 18;
                sheet.Range[row - 1, colNETPAYABLE].CellStyle.Font.Bold = true;
                sheet.Range[row - 1, colNETPAYABLE].CellStyle.Font.Size = 17;

                sheet.Range[row - 1, colNETPAYABLE, row - 1, colSIGNATURE].Merge();
                sheet.Range[row - 1, colNETPAYABLE, row - 1, colSIGNATURE].CellStyle.Interior.Color = System.Drawing.Color.LightGray;


                sheet.Range[row - 1, colSL].Text = payGroupName;
                sheet.Range[row - 1, colSL].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                sheet.Range[row - 1, colSL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[row - 1, colSL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row - 1, colSL].RowHeight = 18;
                sheet.Range[row - 1, colSL].CellStyle.Font.Bold = true;
                sheet.Range[row - 1, colSL].CellStyle.Font.Size = 17;
                sheet.Range[row - 1, colSL, row - 1, colIDNO].Merge();
                sheet.Range[row - 1, colSL, row - 1, colIDNO].CellStyle.Interior.Color = System.Drawing.Color.LightGray;


                row++;
                var summerCol = xlscol - 1;
                var sl = 0;
                var Row_Total_Start = 0;
                var Row_Total_end = 0;
                Row_Total_Start = row;
                string employeeName = "";
                for (int n = 0; n < dtLocal.Rows.Count; n++)
                {
                    xlscol = 1;
                    sl++;

                    if (string.IsNullOrEmpty(languageId))
                    {
                        employeeName = dtLocal.Rows[n]["EmployeeName"].ToString();
                    }
                    else
                    {
                        if (localLanguage.ToUpper() == "English".ToUpper())
                        {
                            employeeName = dtLocal.Rows[n]["EmployeeName"].ToString();
                        }
                        else
                        {
                            employeeName = dtLocal.Rows[n]["EmployeeNameLocal"].ToString();
                        }
                    }

                    reportUtility.SetText(ref sheet, row, colSL, sl.ToString(), 0, 120, 17);
                    reportUtility.SetText(ref sheet, row, colIDNO, dtLocal.Rows[n]["EmployeeCode"].ToString(), 0, 120, 17);
                    reportUtility.SetText(ref sheet, row, colNAME, employeeName, 0, 120, 17);
                    reportUtility.SetText(ref sheet, row, colDESIGNATION, dtLocal.Rows[n]["Designation"].ToString(), 0, 120, 17);
                    sheet.Range[row, colGROSS].Number = Convert.ToDouble(dtLocal.Rows[n]["Gross"].ToString());
                    sheet.Range[row, colGROSS].NumberFormat = reportUtility.NumberFormatIntLocal(localLanguage); ;
                    sheet.Range[row, colGROSS].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                    sheet.Range[row, colGROSS].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colGROSS].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colGROSS].RowHeight = 120;
                    sheet.Range[row, colGROSS].CellStyle.Font.Size = 17;
                    sheet.Range[row, colGROSS].BorderAround(ExcelLineStyle.Hair);


                    sheet.Range[row, colBASIC].Number = Convert.ToDouble(dtLocal.Rows[n]["Basic"].ToString());
                    sheet.Range[row, colBASIC].NumberFormat = reportUtility.NumberFormatIntLocal(localLanguage); ;
                    sheet.Range[row, colBASIC].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                    sheet.Range[row, colBASIC].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colBASIC].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colBASIC].RowHeight = 120;
                    sheet.Range[row, colBASIC].CellStyle.Font.Size = 17;
                    sheet.Range[row, colBASIC].BorderAround(ExcelLineStyle.Hair);


                    reportUtility.SetText(ref sheet, row, colJOINDATE, reportUtility.GetFormatedDate(dtLocal.Rows[n]["DOJ"].ToString(), localLanguage), 0, 50, 17);

                    sheet.Range[row, colServiceLength].Number = Convert.ToDouble(dtLocal.Rows[n]["ServiceLength"].ToString());
                    sheet.Range[row, colServiceLength].NumberFormat = reportUtility.NumberFormatIntLocal(localLanguage); ;
                    sheet.Range[row, colServiceLength].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                    sheet.Range[row, colServiceLength].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colServiceLength].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colServiceLength].RowHeight = 120;
                    sheet.Range[row, colServiceLength].CellStyle.Font.Size = 17;
                    sheet.Range[row, colServiceLength].BorderAround(ExcelLineStyle.Hair);

                    if (bonusType == "Percentage")
                    {
                        sheet.Range[row, colBONUS].Number = Convert.ToDouble(dtLocal.Rows[n]["BonusPercentage"].ToString());
                        sheet.Range[row, colBONUS].NumberFormat = reportUtility.NumberFormatDecimalLocal(localLanguage); ;
                        sheet.Range[row, colBONUS].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                        sheet.Range[row, colBONUS].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet.Range[row, colBONUS].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[row, colBONUS].RowHeight = 120;
                        sheet.Range[row, colBONUS].CellStyle.Font.Size = 17;
                        sheet.Range[row, colBONUS].BorderAround(ExcelLineStyle.Hair);
                    }
                    sheet.Range[row, colBONUSAMOUNT].Number = Convert.ToDouble(dtLocal.Rows[n]["BonusAmount"].ToString());
                    sheet.Range[row, colBONUSAMOUNT].NumberFormat = reportUtility.NumberFormatIntLocal(localLanguage); ;
                    sheet.Range[row, colBONUSAMOUNT].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                    sheet.Range[row, colBONUSAMOUNT].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colBONUSAMOUNT].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colBONUSAMOUNT].RowHeight = 120;
                    sheet.Range[row, colBONUSAMOUNT].CellStyle.Font.Size = 17;
                    sheet.Range[row, colBONUSAMOUNT].BorderAround(ExcelLineStyle.Hair);

                    if (paymentMode != "Bank")
                    {

                        sheet.Range[row, colSTAMPDEDUCTION].Number = stampDed;
                        sheet.Range[row, colSTAMPDEDUCTION].NumberFormat = reportUtility.NumberFormatIntLocal(localLanguage); ;
                        sheet.Range[row, colSTAMPDEDUCTION].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                        sheet.Range[row, colSTAMPDEDUCTION].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet.Range[row, colSTAMPDEDUCTION].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[row, colSTAMPDEDUCTION].RowHeight = 120;
                        sheet.Range[row, colSTAMPDEDUCTION].CellStyle.Font.Size = 17;
                        sheet.Range[row, colSTAMPDEDUCTION].BorderAround(ExcelLineStyle.Hair);
                    }


                    sheet.Range[row, colNETPAYABLE].Number = Convert.ToInt32(dtLocal.Rows[n]["BonusAmount"]) - stampDed;
                    sheet.Range[row, colNETPAYABLE].NumberFormat = reportUtility.NumberFormatIntLocal(localLanguage); ;
                    sheet.Range[row, colNETPAYABLE].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                    sheet.Range[row, colNETPAYABLE].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colNETPAYABLE].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colNETPAYABLE].RowHeight = 120;
                    sheet.Range[row, colNETPAYABLE].CellStyle.Font.Size = 17;
                    sheet.Range[row, colNETPAYABLE].BorderAround(ExcelLineStyle.Hair);
                    if (paymentMode == "Bank")
                    {
                        reportUtility.SetText(ref sheet, row, colBankName, dtLocal.Rows[n]["BankName"].ToString(), 34, 120, 17);
                        reportUtility.SetText(ref sheet, row, colBankAccNo, dtLocal.Rows[n]["BankAccNo"].ToString(), 34, 120, 17);
                    }
                    sheet.Range[row, colSIGNATURE].Text = "";
                    sheet.Range[row, colSIGNATURE].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[row, colSIGNATURE].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colSIGNATURE].RowHeight = 120;
                    sheet.Range[row, colSIGNATURE].BorderAround(ExcelLineStyle.Hair);


                    row++;
                }//Loop-end
                Row_Total_end = row - 1;
                sheet.Range[row, colSL].Text = "Total=";
                sheet.Range[row, colSL].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                sheet.Range[row, colSL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[row, colSL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colSL].RowHeight = 18;
                sheet.Range[row, colSL].CellStyle.Font.Bold = true;
                sheet.Range[row, colSL].CellStyle.Font.Size = 17;
                sheet.Range[row, colSL, row, colNETPAYABLE - 1].Merge();



                sheet.Range[row, colNETPAYABLE].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colNETPAYABLE) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(colNETPAYABLE) + (Row_Total_end) + ")";
                sheet.Range[row, colNETPAYABLE].NumberFormat = reportUtility.NumberFormatIntLocal(localLanguage); ;
                sheet.Range[row, colNETPAYABLE].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                sheet.Range[row, colNETPAYABLE].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colNETPAYABLE].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colNETPAYABLE].RowHeight = 18;
                sheet.Range[row, colNETPAYABLE].CellStyle.Font.Bold = true;
                sheet.Range[row, colNETPAYABLE].CellStyle.Font.Size = 17;

                row += 9;

                reportUtility.SetTextMiddle(ref sheet, row, colSL, "Executive(HR)", true, 25, 15);
                sheet[reportUtility.GetColumnNameForXls(colSL) + row + ":" + reportUtility.GetColumnNameForXls(colIDNO) + row].Merge();

                reportUtility.SetTextMiddle(ref sheet, row, colDESIGNATION, "Manager(HR & Commpliance)", true, 25, 15);
                sheet[reportUtility.GetColumnNameForXls(colDESIGNATION) + row + ":" + reportUtility.GetColumnNameForXls(colGROSS) + row].Merge();

                reportUtility.SetTextMiddle(ref sheet, row, colServiceLength, "Manager(Accounts)", true, 25, 15);
                sheet[reportUtility.GetColumnNameForXls(colServiceLength) + row + ":" + reportUtility.GetColumnNameForXls(colBONUS) + row].Merge();

                reportUtility.SetTextMiddle(ref sheet, row, colNETPAYABLE, "GM (AHRC)", true, 25, 15);
                sheet[reportUtility.GetColumnNameForXls(colNETPAYABLE) + row + ":" + reportUtility.GetColumnNameForXls(colSIGNATURE) + row].Merge();

                #region Report header
                objRpt.SelectedPlantWiseCompany(plantId, languageId, out dsCmp);
                row = 1;
                xlscol = 1;

                FactoryName = string.Empty;

                var FactoryAddress = string.Empty;

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
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddressLocal"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet.Range[row, 1].Text = FactoryName;
                sheet.Range[row, 1].CellStyle.Font.Size = 40;
                sheet.Range[row, 1].CellStyle.Font.FontName = "SolaimanLipi";

                sheet.Range[row, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, 1, row, endXlsCol - 2].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                sheet.Range[row, 1, row, endXlsCol - 2].Merge();
                sheet.Range[row, 1, row, endXlsCol - 2].RowHeight = 46;

                sheet.Range[row, endXlsCol - 1].Text = "Print Date: " + DateTime.Now.ToString("dd-MMM-yyy");
                sheet.Range[row, endXlsCol - 1].CellStyle.Font.Size = 13;
                sheet.Range[row, endXlsCol - 1, row, endXlsCol].Merge();
                sheet.Range[row, endXlsCol - 1, row, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                sheet.Range[row, endXlsCol - 1].CellStyle.Font.FontName = "ArialNarrow";
                sheet.Range[row, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                string yearLocal = reportUtility.cnDgt(Convert.ToDateTime(cutoffdate).Year.ToString(), localLanguage);

                row += 1;
                sheet.Range[row, xlscol].Text = reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.BonusSheet.ToString(), "Bonus Sheet") + "::" + reportUtility.GetLabelname(labelList, dtLocal.Rows[0]["Remarks"].ToString(), dtLocal.Rows[0]["Remarks"].ToString()) + "::" + reportUtility.ChangeMonth(Convert.ToDateTime(cutoffdate).ToString("MMM"), localLanguage) + "-" + yearLocal; //_payRegisterLocal + "," + ru.ChangeMonth(Convert.ToDateTime(para.FromDate).ToString("MMM"), "Bengali") + "," + yearLocal;
                sheet.Range[row, xlscol].CellStyle.Font.FontName = "SolaimanLipi";
                sheet.Range[row, 1, row, endXlsCol - 2].Merge();
                sheet.Range[row, xlscol].CellStyle.Font.Size = 35;
                sheet.Range[row, 1].CellStyle.Font.Bold = true;
                sheet.Range[row, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, 1, row, endXlsCol - 2].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                sheet.Range[row, 1, row, endXlsCol - 2].RowHeight = 40;
                sheet.Range[row, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                #endregion

                #region Freeze Panes
                sheet.UsedRange["A6"].FreezePanes();
                sheet.FirstVisibleColumn = 1;
                sheet.FirstVisibleRow = 5;
                #endregion

                #region UsedRange Alignment
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.7;

                sheet.PageSetup.PrintTitleRows = "$A$1:$IV$5";
                sheet.PageSetup.RightFooter = "&\"Times New Roman\"&15" + "Page " + "&p" + " of " + "&N";
                sheet.PageSetup.LeftMargin = 0.5;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                #endregion
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public IWorkbook GetSFBonusSheetGrid(Dictionary<string, string> parameters, string cutoffdate, string companyId, string plantId, string languageId, string paymentMode, string bonusType, bool isStampDeductApplicable, string reportHeader, string docGrouping)
        {
            try
            {
                var reportUtility = new ReportUtility();
                var excelEngine = new ExcelEngine();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Bonus Sheet";
                DataTable dtLocal = null;
                var objRpt = new clsReport();
                DataSet dsCmp = null;
                var FactoryName = string.Empty;
                var CmpName = string.Empty;

                var printFont = "";


                dtLocal = GetSFBonusSheetDataGrid(parameters, cutoffdate, plantId, languageId);
                if (dtLocal.Rows.Count == 0)
                {
                    throw new Exception("No Data Found.");
                }

                bool isLocalLanguage = false;
                var localLanguage = reportUtility.LocalLanguageListSql(plantId, languageId, out isLocalLanguage);
                if (localLanguage == "Bengali")
                {
                    printFont = "SolaimanLipi";
                }
                else
                {
                    printFont = "Arial Narrow";
                }
                var labelList = reportUtility.LocalLanguageLabelList(plantId, languageId);

                var row = 0;
                var xlscol = 1;
                var colSL = 0;
                var colIDNO = 0;
                var colNAME = 0;
                var colDESIGNATION = 0;
                var colEmpCategory = 0;
                var colJobLocation = 0;
                var colDepartment = 0;
                var colSection = 0;
                var colSubSection = 0;
                var colBudgetedLine = 0;
                var colGROSS = 0;
                var colBASIC = 0;
                var colJOINDATE = 0;
                var colServiceLength = 0;
                var colBONUS = 0;
                var colBONUSAMOUNT = 0;
                var colSTAMPDEDUCTION = 0;
                var colNETPAYABLE = 0;
                var colSIGNATURE = 0;
                var colBankName = 0;
                var colBankAccNo = 0;
                var colServiceLengthType = 0;


                var stampDed = 0;
                var endXlsCol = 0;

                //if (paymentMode == "Bank")
                //{
                //    stampDed = 0;
                //}
                if (isStampDeductApplicable)
                {
                    stampDed = 10;
                }

                //  var DTpayGroup = reportUtility.payRollGroup(payGroup);

                var payGroupName = "";
                //if (DTpayGroup.Rows.Count > 0)
                //{
                //    payGroupName = DTpayGroup.Rows[0]["UserName"].ToString();
                //}
                //row = 5;

                row = 5;

                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.SrNo.ToString(), "SL"), 6, 15); colSL = xlscol; xlscol++;
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.IDNo.ToString(), "ID NO."), 15, 15); colIDNO = xlscol; xlscol++;
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.Name.ToString(), "Name"), 43, 15); colNAME = xlscol; xlscol++;
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.Designation.ToString(), "Designation"), 27, 15); colDESIGNATION = xlscol; xlscol++;
                //reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.EmployeeCategory.ToString(), "Employee Category"), 27, 15); colEmpCategory = xlscol; xlscol++;
                //sheet[row, xlscol].Text = "Job Location";
                //sheet[row, xlscol].ColumnWidth = 27;
                //sheet[row, xlscol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet[row, xlscol].CellStyle.Font.Bold = true;
                //sheet[row, xlscol].CellStyle.Font.Size = 15;
                //colJobLocation = xlscol;
                //xlscol++;

                //reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.Department.ToString(), "Department"), 27, 15); colDepartment = xlscol; xlscol++;
                //reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.Section.ToString(), "Section"), 27, 15); colSection = xlscol; xlscol++;
                //reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.SubSection.ToString(), "Sub Section"), 27, 15); colSubSection = xlscol; xlscol++;
                //sheet[row, xlscol].Text = "Budgeted Line";
                //sheet[row, xlscol].ColumnWidth = 27;
                //sheet[row, xlscol].CellStyle.Font.Size = 15;
                //sheet[row, xlscol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet[row, xlscol].CellStyle.Font.Bold = true;
                //colBudgetedLine = xlscol;
                //xlscol++;
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.TotalSalary.ToString(), "Total Salary"), 16, 15); colGROSS = xlscol; xlscol++;
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.Basic.ToString(), "Basic"), 16, 15); colBASIC = xlscol; xlscol++;
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.DOJ.ToString(), "DOJ"), 31, 15); colJOINDATE = xlscol; xlscol++;
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.ServiceLengthDays.ToString(), "Service Length"), 16, 15); colServiceLength = xlscol; xlscol++;
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.ServiceLengthType.ToString(), "Service Length Type"), 16, 15); colServiceLengthType = xlscol; xlscol++;


                if (bonusType == "Percentage")
                {
                    reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.Bonus.ToString(), "Bonus") + "%", 16, 15); colBONUS = xlscol; xlscol++;
                }
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.BonusAmount.ToString(), "Bonus Amount"), 16, 15); colBONUSAMOUNT = xlscol; xlscol++;
                if (isStampDeductApplicable)
                {
                    reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.StampDeduction.ToString(), "Stamp Deduction"), 16, 15); colSTAMPDEDUCTION = xlscol; xlscol++;
                }
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.NetPayable.ToString(), "Net Payable"), 16, 15); colNETPAYABLE = xlscol; xlscol++;
                //if (paymentMode == "Bank")
                //{
                string bankCash = LabelNameInLocalLanguage.Bank.ToString() + " / " + LabelNameInLocalLanguage.Cash.ToString();
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, bankCash, "Bank / Cash"), 34, 15); colBankName = xlscol; xlscol++;
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.BankAccountNo.ToString(), "Bank Acc. No"), 34, 15); colBankAccNo = xlscol; xlscol++;
                //}
                reportUtility.SetHeaderText(ref sheet, row, xlscol, reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.Signature.ToString(), "Signature"), 37, 15); colSIGNATURE = xlscol;
                endXlsCol = colSIGNATURE;
                sheet.Range[row - 1, colNETPAYABLE].Text = "Cutt off Date: " + cutoffdate;
                sheet.Range[row - 1, colNETPAYABLE].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                sheet.Range[row - 1, colNETPAYABLE].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[row - 1, colNETPAYABLE].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row - 1, colNETPAYABLE].RowHeight = 18;
                sheet.Range[row - 1, colNETPAYABLE].CellStyle.Font.Bold = true;
                sheet.Range[row - 1, colNETPAYABLE].CellStyle.Font.Size = 17;

                sheet.Range[row - 1, colNETPAYABLE, row - 1, colSIGNATURE].Merge();
                sheet.Range[row - 1, colNETPAYABLE, row - 1, colSIGNATURE].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
                sheet.Range[row, 1, row, colSIGNATURE].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                sheet.Range[row, 1, row, colSIGNATURE].WrapText = true;



                sheet.Range[row - 1, colSL].Text = docGrouping;
                sheet.Range[row - 1, colSL].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                sheet.Range[row - 1, colSL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[row - 1, colSL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row - 1, colSL].RowHeight = 18;
                sheet.Range[row - 1, colSL].CellStyle.Font.Bold = true;
                sheet.Range[row - 1, colSL].CellStyle.Font.Size = 17;
                sheet.Range[row - 1, colSL, row - 1, colIDNO + 1].Merge();
                sheet.Range[row - 1, colSL, row - 1, colIDNO + 1].CellStyle.Interior.Color = System.Drawing.Color.LightGray;


                row++;
                var summerCol = xlscol - 1;
                var sl = 0;
                var Row_Total_Start = 0;
                var Row_Total_end = 0;
                Row_Total_Start = row;
                string employeeName = "";
                string LocalDesigName = "";
                for (int n = 0; n < dtLocal.Rows.Count; n++)
                {
                    xlscol = 1;
                    sl++;

                    if (string.IsNullOrEmpty(languageId))
                    {
                        employeeName = dtLocal.Rows[n]["EmployeeName"].ToString();
                        LocalDesigName = dtLocal.Rows[n]["Designation"].ToString();
                        
                    }
                    else
                    {
                        if (localLanguage.ToUpper() == "English".ToUpper())
                        {
                            employeeName = dtLocal.Rows[n]["EmployeeName"].ToString();
                            LocalDesigName = dtLocal.Rows[n]["Designation"].ToString();
                        }
                        else
                        {
                            employeeName = dtLocal.Rows[n]["EmployeeNameLocal"].ToString();
                            LocalDesigName = dtLocal.Rows[n]["Designation"].ToString();
                        }
                    }

                    reportUtility.SetText(ref sheet, row, colSL, sl.ToString(), 0, 0, 17);
                    reportUtility.SetText(ref sheet, row, colIDNO, dtLocal.Rows[n]["EmployeeCode"].ToString(), 0, 0, 17);
                    reportUtility.SetText(ref sheet, row, colNAME, employeeName, 0, 0, 17);
                    reportUtility.SetText(ref sheet, row, colDESIGNATION, LocalDesigName, 0, 0, 17);
                    sheet.Range[row, colDESIGNATION].WrapText = true;
                    //reportUtility.SetText(ref sheet, row, colEmpCategory, dtLocal.Rows[n]["EmployeeCategory"].ToString(), 0, 0, 17);
                    //reportUtility.SetText(ref sheet, row, colJobLocation, dtLocal.Rows[n]["JobLocation"].ToString(), 0, 0, 17);
                    //reportUtility.SetText(ref sheet, row, colDepartment, dtLocal.Rows[n]["Department"].ToString(), 0, 0, 17);
                    //reportUtility.SetText(ref sheet, row, colSection, dtLocal.Rows[n]["EmployeeSection"].ToString(), 0, 0, 17);
                    //reportUtility.SetText(ref sheet, row, colSubSection, dtLocal.Rows[n]["EmployeeSubSection"].ToString(), 0, 0, 17);
                    //reportUtility.SetText(ref sheet, row, colBudgetedLine, dtLocal.Rows[n]["BudgetedLine"].ToString(), 0, 0, 17);


                    sheet.Range[row, colGROSS].Number = Convert.ToDouble(dtLocal.Rows[n]["Gross"].ToString());
                    sheet.Range[row, colGROSS].NumberFormat = reportUtility.NumberFormatIntLocal(localLanguage); ;
                    sheet.Range[row, colGROSS].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                    sheet.Range[row, colGROSS].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colGROSS].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet.Range[row, colGROSS].RowHeight = 120;
                    sheet.Range[row, colGROSS].CellStyle.Font.Size = 17;
                    sheet.Range[row, colGROSS].BorderAround(ExcelLineStyle.Hair);


                    sheet.Range[row, colBASIC].Number = Convert.ToDouble(dtLocal.Rows[n]["Basic"].ToString());
                    sheet.Range[row, colBASIC].NumberFormat = reportUtility.NumberFormatIntLocal(localLanguage); ;
                    sheet.Range[row, colBASIC].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                    sheet.Range[row, colBASIC].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colBASIC].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet.Range[row, colBASIC].RowHeight = 120;
                    sheet.Range[row, colBASIC].CellStyle.Font.Size = 17;
                    sheet.Range[row, colBASIC].BorderAround(ExcelLineStyle.Hair);


                    reportUtility.SetText(ref sheet, row, colJOINDATE, reportUtility.GetFormatedDate(dtLocal.Rows[n]["DOJ"].ToString(), localLanguage), 0, 50, 17);

                    sheet.Range[row, colServiceLength].Number = Convert.ToDouble(dtLocal.Rows[n]["ServiceLength"].ToString());
                    sheet.Range[row, colServiceLength].NumberFormat = reportUtility.NumberFormatIntLocal(localLanguage); ;
                    sheet.Range[row, colServiceLength].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                    sheet.Range[row, colServiceLength].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colServiceLength].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet.Range[row, colServiceLength].RowHeight = 120;
                    sheet.Range[row, colServiceLength].CellStyle.Font.Size = 17;
                    sheet.Range[row, colServiceLength].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colServiceLengthType].Text = reportUtility.GetLabelname(labelList, dtLocal.Rows[n]["ServiceLengthType"].ToString(), dtLocal.Rows[n]["ServiceLengthType"].ToString());
                    sheet.Range[row, colServiceLengthType].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                    sheet.Range[row, colServiceLengthType].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colServiceLengthType].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet.Range[row, colServiceLengthType].RowHeight = 120;
                    sheet.Range[row, colServiceLengthType].CellStyle.Font.Size = 17;
                    sheet.Range[row, colServiceLengthType].BorderAround(ExcelLineStyle.Hair);

                    if (bonusType == "Percentage")
                    {
                        sheet.Range[row, colBONUS].Number = Convert.ToDouble(dtLocal.Rows[n]["BonusPercentage"].ToString());
                        sheet.Range[row, colBONUS].NumberFormat = reportUtility.NumberFormatDecimalLocal(localLanguage); ;
                        sheet.Range[row, colBONUS].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                        sheet.Range[row, colBONUS].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet.Range[row, colBONUS].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //sheet.Range[row, colBONUS].RowHeight = 120;
                        sheet.Range[row, colBONUS].CellStyle.Font.Size = 17;
                        sheet.Range[row, colBONUS].BorderAround(ExcelLineStyle.Hair);
                    }
                    sheet.Range[row, colBONUSAMOUNT].Number = Convert.ToDouble(dtLocal.Rows[n]["BonusAmount"].ToString());
                    sheet.Range[row, colBONUSAMOUNT].NumberFormat = reportUtility.NumberFormatIntLocal(localLanguage); ;
                    sheet.Range[row, colBONUSAMOUNT].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                    sheet.Range[row, colBONUSAMOUNT].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colBONUSAMOUNT].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet.Range[row, colBONUSAMOUNT].RowHeight = 120;
                    sheet.Range[row, colBONUSAMOUNT].CellStyle.Font.Size = 17;
                    sheet.Range[row, colBONUSAMOUNT].BorderAround(ExcelLineStyle.Hair);

                    if (isStampDeductApplicable)
                    {

                        sheet.Range[row, colSTAMPDEDUCTION].Number = stampDed;
                        sheet.Range[row, colSTAMPDEDUCTION].NumberFormat = reportUtility.NumberFormatIntLocal(localLanguage); ;
                        sheet.Range[row, colSTAMPDEDUCTION].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                        sheet.Range[row, colSTAMPDEDUCTION].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet.Range[row, colSTAMPDEDUCTION].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //sheet.Range[row, colSTAMPDEDUCTION].RowHeight = 120;
                        sheet.Range[row, colSTAMPDEDUCTION].CellStyle.Font.Size = 17;
                        sheet.Range[row, colSTAMPDEDUCTION].BorderAround(ExcelLineStyle.Hair);
                    }


                    sheet.Range[row, colNETPAYABLE].Number = Convert.ToInt32(dtLocal.Rows[n]["BonusAmount"]) - stampDed;
                    sheet.Range[row, colNETPAYABLE].NumberFormat = reportUtility.NumberFormatIntLocal(localLanguage); ;
                    sheet.Range[row, colNETPAYABLE].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                    sheet.Range[row, colNETPAYABLE].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colNETPAYABLE].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    // sheet.Range[row, colNETPAYABLE].RowHeight = 120;
                    sheet.Range[row, colNETPAYABLE].CellStyle.Font.Size = 17;
                    sheet.Range[row, colNETPAYABLE].BorderAround(ExcelLineStyle.Hair);
                    //if (paymentMode == "Bank")
                    //{
                    reportUtility.SetText(ref sheet, row, colBankName, dtLocal.Rows[n]["BankName"].ToString(), 34, 120, 17);
                    reportUtility.SetText(ref sheet, row, colBankAccNo, dtLocal.Rows[n]["BankAccNo"].ToString(), 34, 120, 17);
                    //}
                    sheet.Range[row, colSIGNATURE].Text = "";
                    sheet.Range[row, colSIGNATURE].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[row, colSIGNATURE].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colSIGNATURE].RowHeight = 147;
                    sheet.Range[row, colSIGNATURE].BorderAround(ExcelLineStyle.Hair);

                    if (sl % 7 == 0)
                    {
                        sheet.Range[row, 1, row, colSIGNATURE].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Hair;
                        row++;
                        sheet[row, 1].RowHeight = 2;

                        sheet.HPageBreaks.Add(sheet[row, 1]);
                    }
                    row++;
                }//Loop-end
                Row_Total_end = row - 1;
                sheet.Range[row, colSL].Text = "Total=";
                sheet.Range[row, colSL].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                sheet.Range[row, colSL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[row, colSL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colSL].RowHeight = 18;
                sheet.Range[row, colSL].CellStyle.Font.Bold = true;
                sheet.Range[row, colSL].CellStyle.Font.Size = 17;
                sheet.Range[row, colSL, row, colNETPAYABLE - 1].Merge();



                sheet.Range[row, colNETPAYABLE].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colNETPAYABLE) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(colNETPAYABLE) + (Row_Total_end) + ")";
                sheet.Range[row, colNETPAYABLE].NumberFormat = reportUtility.NumberFormatIntLocal(localLanguage); ;
                sheet.Range[row, colNETPAYABLE].IgnoreErrorOptions = ExcelIgnoreError.NumberAsText;
                sheet.Range[row, colNETPAYABLE].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet.Range[row, colNETPAYABLE].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[row, colNETPAYABLE].RowHeight = 18;
                sheet.Range[row, colNETPAYABLE].CellStyle.Font.Bold = true;
                sheet.Range[row, colNETPAYABLE].CellStyle.Font.Size = 17;
                sheet.Range[row, colNETPAYABLE].ColumnWidth = 23;


                row += 9;

                reportUtility.SetTextMiddle(ref sheet, row, colSL, "Executive(HR)", true, 25, 15);
                sheet[reportUtility.GetColumnNameForXls(colSL) + row + ":" + reportUtility.GetColumnNameForXls(colIDNO) + row].Merge();

                reportUtility.SetTextMiddle(ref sheet, row, colDESIGNATION, "Manager(HR & Commpliance)", true, 25, 15);
                sheet[reportUtility.GetColumnNameForXls(colDESIGNATION) + row + ":" + reportUtility.GetColumnNameForXls(colGROSS) + row].Merge();

                reportUtility.SetTextMiddle(ref sheet, row, colServiceLength, "Manager(Accounts)", true, 25, 15);
                sheet[reportUtility.GetColumnNameForXls(colServiceLength) + row + ":" + reportUtility.GetColumnNameForXls(colBONUS) + row].Merge();

                reportUtility.SetTextMiddle(ref sheet, row, colNETPAYABLE, "GM (AHRC)", true, 25, 15);
                sheet[reportUtility.GetColumnNameForXls(colNETPAYABLE) + row + ":" + reportUtility.GetColumnNameForXls(colSIGNATURE) + row].Merge();

                

                #region ******************Report Header******************
                objRpt.SelectedPlantWiseCompany(plantId, languageId, out dsCmp);
               int xlsRow = 1;
                int xlsCol = 1;

                FactoryName = string.Empty;

                var FactoryAddress = string.Empty;

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
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddressLocal"].ToString();
                    if (FactoryAddress == "")
                    {
                        FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddress"].ToString();

                    }
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet.Range[xlsRow, 1].Text = CmpName + " ( " + FactoryName + " )";
                sheet.Range[xlsRow, 1].CellStyle.Font.Size = 40;
                sheet.Range[xlsRow, 1].CellStyle.Font.Bold = true;

                sheet.Range[xlsRow, 1].CellStyle.Font.FontName = printFont;

                sheet.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                sheet.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
                sheet.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 50;

                xlsRow++;
                sheet.Range[xlsRow, 1].Text = FactoryAddress;
                sheet.Range[xlsRow, 1].CellStyle.Font.Size = 35;
                sheet.Range[xlsRow, 1].CellStyle.Font.FontName = printFont;
                sheet.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                sheet.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
                sheet.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 38;
                sheet.Range[xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                sheet.Range[xlsRow - 1, endXlsCol].Text =  "Print Date: " + DateTime.Now.ToString("dd-MMM-yyy");
                sheet.Range[xlsRow - 1, endXlsCol].CellStyle.Font.Size = 17;
                sheet.Range[xlsRow - 1, endXlsCol].CellStyle.Font.FontName = "ArialNarrow";
                sheet.Range[xlsRow - 1, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                string yearLocal = reportUtility.cnDgt(Convert.ToDateTime(cutoffdate).Year.ToString(), localLanguage);

                xlsRow += 1;
                
                sheet.Range[xlsRow, xlsCol].Text = reportUtility.GetLabelname(labelList, LabelNameInLocalLanguage.BonusSheet.ToString(), "Bonus Sheet") + ":" + reportHeader + reportUtility.ChangeMonth(Convert.ToDateTime(cutoffdate).ToString("MMM"), localLanguage) + "-" + yearLocal; //_payRegisterLocal + "," + ru.ChangeMonth(Convert.ToDateTime(para.FromDate).ToString("MMM"), "Bengali") + "," + yearLocal;
                sheet.Range[xlsRow, xlsCol].CellStyle.Font.FontName = printFont;

                sheet.Range[xlsRow, 1, xlsRow, endXlsCol - 1].Merge();
                sheet.Range[xlsRow, xlsCol].CellStyle.Font.Size = 40;
                sheet.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[xlsRow, 1, xlsRow, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                sheet.Range[xlsRow, 1, xlsRow, endXlsCol - 1].RowHeight = 50;
                sheet.Range[xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;



                #endregion ******************Report Header******************

                #region Freeze Panes
                sheet.UsedRange["A6"].FreezePanes();
                sheet.FirstVisibleColumn = 1;
                sheet.FirstVisibleRow = 5;
                #endregion

                #region UsedRange Alignment
                //sheet.UsedRange.WrapText = true;
                sheet.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.0;

                sheet.PageSetup.PrintTitleRows = "$A$1:$IV$5";
                sheet.PageSetup.RightFooter = "&\"Times New Roman\"&15" + "Page " + "&p" + " of " + "&N";
                sheet.PageSetup.LeftMargin = 0.3;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                #endregion

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }


        private DataTable GetSFBonusSheetData(string plantId, string PayRollGroupId, string cutoffdate, string policyId, string languageId, string paymentMode, string bonusType)
        {
            try
            {

                string _paymentMode = string.Empty;
                string serviceLength = "";
                var payGroupSelection = "";
                if (paymentMode == "Bank")
                {
                    _paymentMode = "and e.PaymentMode='Bank'";
                }
                else if (paymentMode == "Cash")
                {
                    _paymentMode = "and e.PaymentMode='Cash'";
                }
                else
                {
                    throw new Exception("Theres is no employee assigned in" + paymentMode);
                }
                if (bonusType == "Percentage")
                {
                    //serviceLength = "b.ServiceLenght >= 180 AND";
                    serviceLength = "";

                }
                if (PayRollGroupId == "ALL")
                {
                    payGroupSelection = "";
                }
                else
                {
                    payGroupSelection = "AND PG.Id = '" + PayRollGroupId + @"'";
                }

                string wc = string.Empty;
                wc = " where m.systemid in (select SystemID from BonusPaymentActualMaster " +
                    "where BonusSystemID='" + policyId + "' and EffectiveDate='" + cutoffdate + "') ";

                string sqlText = @"SELECT e.SystemId,e.EmployeeCode,ISNULL(E.EmployeeNameLocal,EmployeeName) EmployeeNameLocal,E.EmployeeName
            ,ISNULL(ebi.BankAccNo,'') BankAccNo,ISNULL(locald.Name,d.UserName) Designation,ISNULL(hg.EntryAmount,0) Gross,ISNULL(hb.EntryAmount,0) [Basic]
                --,DOJ
                ,Replace(CONVERT(VARCHAR(11), DOJ, 106), ' ', '-') DOJ
                ,Replace(CONVERT(VARCHAR(11), DOS, 106), ' ', '-') DOS

                ,ISNULL(b.ServiceLenght,'')  ServiceLength
                ,ISNULL(b.BonusAmount,'') BonusAmount,b.Remarks 
                --,b.BonusPercentage
               ,ISNULL(PG.StandardName,'') PayRollGroupName,ISNULL(PG.Id,'') PayRollGroupId
                ,min(ISNULL(b.BonusPercentageValue,0)) BonusPercentage
				,ISNULL(Bank.UserName,'') BankName
                FROM EmployeeInformation e
                left join hkp.LegalDesignation d on e.LegalDesignationId=d.id
                 LEFT JOIN hkp.LocalLanguage locald on locald.LegalDesignationId=d.id and LanguageId = '" + languageId + @"'
				 LEFT JOIN EmployeeBankInfo ebi on ebi.EmpSystemID = e.SystemId
				 LEFT JOIN HKP.Bank Bank on ebi.BankSystemID = Bank.Id


                LEFT JOIN HKP.Designation DG on DG.Id=E.GivenDesignationId

                LEFT JOIN MST.payrollgroupmaster PM on PM.EmployeeId=E.SystemId---
                LEFT JOIN hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId----

                 ---------------------gross
                LEFT JOIN (
            SELECT x.EmpInfoSystemID,x.IsApproved,x.SalaryRuleMasterSystemID,x.SystemID,EntryAmount from (
					SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
					FROM(
					SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster 
					WHERE EffectiveDate<='" + cutoffdate + "' and PlantID='" + plantId + @"' Group By EmpInfoSystemID
					UNION
					SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate FROM SalaryInfoBackMaster 
					WHERE EffectiveDate<='" + cutoffdate + "' and PlantID='" + plantId + @"' Group By EmpInfoSystemID
					) U GROUP BY EmpInfoSystemID


		) b 
		LEFT JOIN (SELECT EmpInfoSystemID,IsApproved,EffectiveDate,SalaryRuleMasterSystemID,m.SystemId,d.EntryAmount
		
		 from SalaryInfoDefineMaster m
		 LEFT JOIN SalaryInfoDefine d ON m.SystemID=d.SalaryID
         LEFT JOIN SalaryHead h ON h.SalaryHeadID=d.SalaryHeadID
                  
		 WHERE PlantID='" + plantId + @"' AND h.HeadCategory='Gross'
		UNION
		SELECT EmpInfoSystemID,IsApproved,EffectiveDate,SalaryRuleMasterSystemID,SBM.SystemId,SIB.EntryAmount from SalaryInfoBackMaster SBM
		INNER JOIN SalaryInfoBack SIB ON SIB.SalaryID = SBM.SystemID 
					left outer join SalaryHead SH ON SH.SalaryHeadID = SIB.SalaryHeadID
					
		 WHERE PlantID='" + plantId + @"' AND SH.HeadCategory='Gross' ---and sb.EffectiveDate=sm.EffectiveDate 


		) x ON x.EmpInfoSystemID=b.EmpInfoSystemID and x.EffectiveDate=b.EffectiveDate) hg on hg.EmpInfoSystemID=e.SystemId
                ----------------------basic
                LEFT JOIN (
				SELECT x.EmpInfoSystemID,x.IsApproved,x.SalaryRuleMasterSystemID,x.SystemID,EntryAmount from (
					SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
					FROM(
					SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster 
					WHERE EffectiveDate<='" + cutoffdate + "' and PlantID='" + plantId + @"' Group By EmpInfoSystemID
					UNION
					SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate FROM SalaryInfoBackMaster 
					WHERE EffectiveDate<='" + cutoffdate + "' and PlantID='" + plantId + @"' Group By EmpInfoSystemID
					) u Group By EmpInfoSystemID


		) b 
		LEFT JOIN (select EmpInfoSystemID,IsApproved,EffectiveDate,SalaryRuleMasterSystemID,m.SystemId,d.EntryAmount
		
		 FROM SalaryInfoDefineMaster m
		 LEFT JOIN SalaryInfoDefine d on m.SystemID=d.SalaryID
         LEFT JOIN SalaryHead h on h.SalaryHeadID=d.SalaryHeadID
                  
		 WHERE PlantID='" + plantId + @"' and h.HeadCategory='Basic'

        UNION
        SELECT EmpInfoSystemID,IsApproved,EffectiveDate,SalaryRuleMasterSystemID,SBM.SystemId,SIB.EntryAmount from SalaryInfoBackMaster SBM

        INNER JOIN SalaryInfoBack SIB ON SIB.SalaryID = SBM.SystemID

                    left outer join SalaryHead SH ON SH.SalaryHeadID = SIB.SalaryHeadID


         WHERE PlantID = '" + plantId + @"' and SH.HeadCategory = 'Basic'-- - and sb.EffectiveDate = sm.EffectiveDate


		) x on x.EmpInfoSystemID = b.EmpInfoSystemID and x.EffectiveDate = b.EffectiveDate
                ) hb on hb.EmpInfoSystemID = e.SystemId
                ----------------Bonus
                INNER JOIN
                (
                SELECT d.BonusAmount, d.ServiceLenght, mm.BonusSystemID BonusPolicyMasterID, d.EmpSystemID, mm.Remarks
                , d.BonusPercentage
                , BonusPercentageValue = case 
                WHEN bpd.IsFixed = 1 then bpd.FixedAmount
                WHEN bpd.IsPercentage = 1 then bpd.BonusPercentage
                ELSE bpd.BonusPercentage / bpd.DivisionFactor
                END
                FROM BonusPaymentActualMaster mm
                INNER JOIN(
                                SELECT max(effectivedate) effectivedate, m.SystemID  FROM BonusPaymentActualMaster m

                                " + wc + @"
                             GROUP BY m.SystemID
                            ) m ON mm.effectivedate = m.effectivedate AND mm.SystemID = m.SystemID

                LEFT JOIN BonusPaymentActual d ON mm.SystemID = d.BnsMstSystemID

                ----------------for % ------------

                LEFT JOIN BonusPolicyMaster BPM ON BPM.SystemID = mm.BonusSystemID
                LEFT JOIN BonusPolicyDetail BPD ON BPD.BPMSystemID = bpm.SystemID 
                    and isnull(d.ServiceLenght,0) between isnull(bpd.MinServLen,0) and isnull(bpd.MaxServLen,0)


                ) b on b.EmpSystemID = e.SystemId
                WHERE " + serviceLength + "(e.EmployeeStatus = 'Active' OR(e.EmployeeStatus = 'Separated' AND E.DOS >= '" + cutoffdate + "'))" + payGroupSelection + _paymentMode + @"
             --where e.SystemId in ('1901325')--,'1800029','1800033','1800036')
                GROUP BY e.SystemId,e.EmployeeCode,EmployeeName,EmployeeNameLocal,d.UserName,locald.Name,hg.EntryAmount ,hb.EntryAmount,doj,b.ServiceLenght
                ,b.BonusAmount, PG.StandardName ,PG.Id,dg.UserName,b.Remarks,ebi.BankAccNo,DOS,Bank.UserName,EmployeeCodePreFix,EmployeeCodeNumeric
               ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";

                return _sqlRepository.GetDataTable(sqlText);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        private DataTable GetSFBonusSheetDataGrid(Dictionary<string, string> parameters, string cutoffdate, string plantId, string languageId)
        {
            try
            {

                string _paymentMode = string.Empty;
                string serviceLength = "";
                var payGroupSelection = "";


                string wc = string.Empty;
                wc = " where m.systemid in (select SystemID from BonusPaymentActualMaster " +
                    "where  EffectiveDate='" + cutoffdate + "') ";

                string sqlText = @"SELECT e.SystemId,e.EmployeeCode,ISNULL(E.EmployeeNameLocal,EmployeeName) EmployeeNameLocal,E.EmployeeName
            ,ISNULL(ebi.BankAccNo,'') BankAccNo,ISNULL(locald.Name,d.UserName) Designation, locald.Name DesignationLocal,ISNULL(hg.EntryAmount,0) Gross,ISNULL(hb.EntryAmount,0) [Basic]
                --,DOJ
                ,Replace(CONVERT(VARCHAR(11), DOJ, 106), ' ', '-') DOJ
                ,Replace(CONVERT(VARCHAR(11), DOS, 106), ' ', '-') DOS

                ,ISNULL(b.ServiceLenght,'')  ServiceLength
                ,ISNULL(b.ServiceLengthType,'')  ServiceLengthType

                ,ISNULL(b.BonusAmount,'') BonusAmount,b.Remarks 
                --,b.BonusPercentage
               ,ISNULL(PG.StandardName,'') PayRollGroupName,ISNULL(PG.Id,'') PayRollGroupId
                ,min(ISNULL(b.BonusPercentageValue,0)) BonusPercentage
				,ISNULL(Bank.UserName,e.PaymentMode) BankName
,ec.UserName EmployeeCategory,jl.JobLocation,d2.UserName Department
				,s.userName EmployeeSection,ss.UserName EmployeeSubSection,l.UserName BudgetedLine
                FROM EmployeeInformation e
                left join hkp.LegalDesignation d on e.LegalDesignationId=d.id
                 LEFT JOIN hkp.LocalLanguage locald on locald.LegalDesignationId=d.id and LanguageId = '"+ languageId + @"'
				 LEFT JOIN EmployeeBankInfo ebi on ebi.EmpSystemID = e.SystemId
				 LEFT JOIN HKP.Bank Bank on ebi.BankSystemID = Bank.Id


                LEFT JOIN HKP.Designation DG on DG.Id=E.GivenDesignationId
                LEFT JOIN MST.DesignationMaster AS dm ON dm.DesignationId=dg.Id
                LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId
                LEFT JOIN dbo.JobLocation AS jl ON jl.SystemID=e.JobLocationID
                LEFT JOIN ORG.Department AS d2 ON d2.Id=e.DepartmentId
                LEFT JOIN ORG.Section S ON S.Id=e.SectionId
                LEFT JOIN ORG.SubSection SS ON SS.Id=e.SubSectionId
                LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=e.BudgetCode
                LEFT JOIN ORG.Line AS l ON l.Id=mb.LineId

                LEFT JOIN MST.payrollgroupmaster PM on PM.EmployeeId=E.SystemId---
                LEFT JOIN hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId----

                 ---------------------gross
                LEFT JOIN (
            SELECT x.EmpInfoSystemID,x.IsApproved,x.SalaryRuleMasterSystemID,x.SystemID,EntryAmount from (
					SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
					FROM(
					SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster 
					WHERE EffectiveDate<='" + cutoffdate + "' and PlantID='" + plantId + @"' Group By EmpInfoSystemID
					UNION
					SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate FROM SalaryInfoBackMaster 
					WHERE EffectiveDate<='" + cutoffdate + "' and PlantID='" + plantId + @"' Group By EmpInfoSystemID
					) U GROUP BY EmpInfoSystemID


		) b 
		LEFT JOIN (SELECT EmpInfoSystemID,IsApproved,EffectiveDate,SalaryRuleMasterSystemID,m.SystemId,d.EntryAmount
		
		 from SalaryInfoDefineMaster m
		 LEFT JOIN SalaryInfoDefine d ON m.SystemID=d.SalaryID
         LEFT JOIN SalaryHead h ON h.SalaryHeadID=d.SalaryHeadID
                  
		 WHERE PlantID='" + plantId + @"' AND h.HeadCategory='Gross'
		UNION
		SELECT EmpInfoSystemID,IsApproved,EffectiveDate,SalaryRuleMasterSystemID,SBM.SystemId,SIB.EntryAmount from SalaryInfoBackMaster SBM
		INNER JOIN SalaryInfoBack SIB ON SIB.SalaryID = SBM.SystemID 
					left outer join SalaryHead SH ON SH.SalaryHeadID = SIB.SalaryHeadID
					
		 WHERE PlantID='" + plantId + @"' AND SH.HeadCategory='Gross' ---and sb.EffectiveDate=sm.EffectiveDate 


		) x ON x.EmpInfoSystemID=b.EmpInfoSystemID and x.EffectiveDate=b.EffectiveDate) hg on hg.EmpInfoSystemID=e.SystemId
                ----------------------basic
                LEFT JOIN (
				SELECT x.EmpInfoSystemID,x.IsApproved,x.SalaryRuleMasterSystemID,x.SystemID,EntryAmount from (
					SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
					FROM(
					SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster 
					WHERE EffectiveDate<='" + cutoffdate + "' and PlantID='" + plantId + @"' Group By EmpInfoSystemID
					UNION
					SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate FROM SalaryInfoBackMaster 
					WHERE EffectiveDate<='" + cutoffdate + "' and PlantID='" + plantId + @"' Group By EmpInfoSystemID
					) u Group By EmpInfoSystemID


		) b 
		LEFT JOIN (select EmpInfoSystemID,IsApproved,EffectiveDate,SalaryRuleMasterSystemID,m.SystemId,d.EntryAmount
		
		 FROM SalaryInfoDefineMaster m
		 LEFT JOIN SalaryInfoDefine d on m.SystemID=d.SalaryID
         LEFT JOIN SalaryHead h on h.SalaryHeadID=d.SalaryHeadID
                  
		 WHERE PlantID='" + plantId + @"' and h.HeadCategory='Basic'

        UNION
        SELECT EmpInfoSystemID,IsApproved,EffectiveDate,SalaryRuleMasterSystemID,SBM.SystemId,SIB.EntryAmount from SalaryInfoBackMaster SBM

        INNER JOIN SalaryInfoBack SIB ON SIB.SalaryID = SBM.SystemID

                    left outer join SalaryHead SH ON SH.SalaryHeadID = SIB.SalaryHeadID


         WHERE PlantID = '" + plantId + @"' and SH.HeadCategory = 'Basic'-- - and sb.EffectiveDate = sm.EffectiveDate


		) x on x.EmpInfoSystemID = b.EmpInfoSystemID and x.EffectiveDate = b.EffectiveDate
                ) hb on hb.EmpInfoSystemID = e.SystemId
                ----------------Bonus
                INNER JOIN
                (
                SELECT d.BonusAmount, d.ServiceLenght,d.ServiceLengthType, mm.BonusSystemID BonusPolicyMasterID, d.EmpSystemID, mm.Remarks
                , d.BonusPercentage
                , BonusPercentageValue = case 
                WHEN bpd.IsFixed = 1 then bpd.FixedAmount
                WHEN bpd.IsPercentage = 1 then bpd.BonusPercentage
                ELSE bpd.BonusPercentage / bpd.DivisionFactor
                END
                FROM BonusPaymentActualMaster mm
                INNER JOIN(
                                SELECT max(effectivedate) effectivedate, m.SystemID  FROM BonusPaymentActualMaster m

                                " + wc + @"
                             GROUP BY m.SystemID
                            ) m ON mm.effectivedate = m.effectivedate AND mm.SystemID = m.SystemID

                LEFT JOIN BonusPaymentActual d ON mm.SystemID = d.BnsMstSystemID

                ----------------for % ------------

                LEFT JOIN BonusPolicyMaster BPM ON BPM.SystemID = mm.BonusSystemID
                LEFT JOIN BonusPolicyDetail BPD ON BPD.BPMSystemID = bpm.SystemID 
                    and isnull(d.ServiceLenght,0) between isnull(bpd.MinServLen,0) and isnull(bpd.MaxServLen,0)


                ) b on b.EmpSystemID = e.SystemId
                WHERE " + serviceLength + "(e.EmployeeStatus = 'Active' OR(e.EmployeeStatus = 'Separated' AND E.DOS >= '" + cutoffdate + "'))and e.PlantId = '" + plantId + @"'" + payGroupSelection + _paymentMode + @"";
                if (parameters.Count > 0)
                {
                    if (parameters.Keys.ElementAt(0) != "")
                    {
                        sqlText += @"and e.SystemId IN(" + parameters["EmpSystemId"] + ")";

                    }
                }
                sqlText += @" GROUP BY e.SystemId,e.EmployeeCode,EmployeeName,EmployeeNameLocal,d.UserName,locald.Name,hg.EntryAmount ,hb.EntryAmount,doj,b.ServiceLenght
                 ,e.PaymentMode,b.BonusAmount, PG.StandardName ,PG.Id,dg.UserName,b.Remarks,ebi.BankAccNo,DOS,Bank.UserName,ec.UserName ,jl.JobLocation,d2.UserName 
				,s.userName ,ss.UserName ,l.UserName 
,EmployeeCodePreFix,EmployeeCodeNumeric,ServiceLengthType
               ORDER BY ISNULL(EmployeeCodePreFix,''),ISNULL(EmployeeCodeNumeric,0)";

                return _sqlRepository.GetDataTable(sqlText);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }


        public IEnumerable<object> GetBonusPoint()
        {
            try
            {
                string sqlText = @"SELECT DISTINCT bpm.SystemId+'__'+LTRIM( Replace(CONVERT(VARCHAR(11), bpam.EffectiveDate, 106), ' ', '-')) AS Id,bpm.PolicyName+Replace(CONVERT(VARCHAR(11), bpam.EffectiveDate, 106), ' ', '-') AS Name
                                    FROM BonusPaymentActualMaster bpam
                                    LEFT JOIN BonusPolicyMaster bpm  ON bpm.SystemId=bpam.BonusSystemID
                                    ";

                return _sqlRepository.GetDataCollection(sqlText, null);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public IEnumerable<object> GetBonusEffectiveDate()
        {
            try
            {
                string sqlText = @"SELECT DISTINCT FORMAT(bpam.EffectiveDate,'dd-MMM-yyyy') effectiveDate
                                    FROM BonusPaymentActualMaster bpam
                                    LEFT JOIN BonusPolicyMaster bpm  ON bpm.SystemId=bpam.BonusSystemID
                                    ";

                return _sqlRepository.GetDataCollection(sqlText, null);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IEnumerable<object> GetEmpInfo(string companyGroupId, string plantId, string effectiveDate, bool sa, bool ca, string userId)
        {
            try
            {
                string wc = @" where m.systemid in (select SystemID from BonusPaymentActualMaster where EffectiveDate='" + effectiveDate + "') ";

                var cmdText = @"SELECT [isSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'False'),* FROM (  SELECT  DISTINCT   
                                     ISNULL(e.SystemId,'') EmpSystemId
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
                                    --,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    ,ISNULL(FORMAT(e.DOS,'dd-MMM-yyyy'),'') DOS
                                    , CASE WHEN MONTH(DOS) =  MONTH('" + effectiveDate + @"')  AND YEAR(DOS) = YEAR('" + effectiveDate + @"') then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                    ,ISNULL(b.Remarks,'') Remarks
                                    ,ISNULL(b.bonusPolicy,'') bonusPolicy
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
                                    LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
									LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    LEFT JOIN [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
									LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									LEFT JOIN [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
                                     
                                    ----------------Bonus
                INNER JOIN
                (
                SELECT d.BonusAmount, d.ServiceLenght, mm.BonusSystemID BonusPolicyMasterID, d.EmpSystemID, mm.Remarks
                    , bpm.PolicyName+Replace(CONVERT(VARCHAR(11), mm.EffectiveDate, 106), ' ', '-') bonusPolicy
                , d.BonusPercentage
                , BonusPercentageValue = case 
                WHEN bpd.IsFixed = 1 then bpd.FixedAmount
                WHEN bpd.IsPercentage = 1 then bpd.BonusPercentage
                ELSE bpd.BonusPercentage / bpd.DivisionFactor
                END
                FROM BonusPaymentActualMaster mm
                INNER JOIN(
                                SELECT max(effectivedate) effectivedate, m.SystemID  FROM BonusPaymentActualMaster m

                                " + wc + @"
                             GROUP BY m.SystemID
                            ) m ON mm.effectivedate = m.effectivedate AND mm.SystemID = m.SystemID

                LEFT JOIN BonusPaymentActual d ON mm.SystemID = d.BnsMstSystemID

                ----------------for % ------------

                LEFT JOIN BonusPolicyMaster BPM ON BPM.SystemID = mm.BonusSystemID
                LEFT JOIN BonusPolicyDetail BPD ON BPD.BPMSystemID = bpm.SystemID 
                    and isnull(d.ServiceLenght,0) between isnull(bpd.MinServLen,0) and isnull(bpd.MaxServLen,0)


                ) b on b.EmpSystemID = e.SystemId AND
                                     E.GroupID='" + companyGroupId + "' AND E.PlantId='" + plantId + @"'                                   
                                     ) DD  ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}