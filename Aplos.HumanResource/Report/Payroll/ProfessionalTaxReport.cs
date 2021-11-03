using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Extension;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Library.Service.Helpers.ReportUtility;

namespace Library.HumanResource.Payroll.Report
{
    public class ProfessionalTaxReport
    {
        ISqlRepository _sqlRepository;
        public ProfessionalTaxReport()
        {
            _sqlRepository = new SqlRepository();
        }

        public IWorkbook GetProfessionalTaxReport(string yearId, string companyGroupId, string companyId, string plantId, string userName, string FromDate, string ToDate)
        {

            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            #region Variable

            clsReport objRpt = null;
            int slCount = 0;

            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsMonth = null;
            //DataSet dsEmpAttdn = null;
            // DataTable dtEmpAttdn = null;

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
                ru = new ReportUtility();

                objRpt = new clsReport(_sqlRepository);

                #region Variable
                ParamList para = new ParamList();
                ParamList leavePara = new ParamList();
                ParamList attdnProcessParam = new ParamList();

                var FactoryName = "";
                var CmpName = "";

                para.PlantId = plantId;


                #endregion Variable
                //string FromDate = "";
                //string ToDate = "";
                #region DataSet
                DataTable dtTaxYear = null;
                dtTaxYear = _sqlRepository.GetDataTable("SELECT * FROM SCS.TaxYear WHERE TaxYearName = '" + yearId + @"'");

                int fromYear = Convert.ToDateTime(dtTaxYear.Rows[0]["StartDate"]).Year;//EndDate
                int toYear = Convert.ToDateTime(dtTaxYear.Rows[0]["EndDate"]).Year;

                //if(dateRange == null)
                //{
                //    FromDate = Convert.ToDateTime(dtTaxYear.Rows[0]["StartDate"]).ToString("dd-MMM-yyyy");
                //    ToDate = Convert.ToDateTime(dtTaxYear.Rows[0]["EndDate"]).ToString("dd-MMM-yyyy");
                //}
                //else
                //{
                //    string strsqlMinDate = @"SELECT Format(Min(v),'dd-MMM-yyyy') MinDate 
                //             FROM (VALUES " + dateRange + @") AS value(v)";
                //    FromDate = _sqlRepository.GetDataTable(strsqlMinDate).Rows[0]["MinDate"].ToString();

                //    string strsqlMxDate = @"SELECT Format(Max(v),'dd-MMM-yyyy') MaxDate 
                //             FROM (VALUES " + dateRange + @") AS value(v)";
                //    ToDate = _sqlRepository.GetDataTable(strsqlMxDate).Rows[0]["MaxDate"].ToString();

                int DaysInMonth = DateTime.DaysInMonth(Convert.ToInt16(Convert.ToDateTime(ToDate).ToString("yyyy")), Convert.ToInt16(Convert.ToDateTime(ToDate).ToString("MM")));

                ToDate = DaysInMonth + "-" + Convert.ToDateTime(ToDate).ToString("MMM") + "-" + Convert.ToDateTime(ToDate).ToString("yyyy");
                //}







                string sqlTaxMaxMin = @"SELECT SDP.* from TaxPolicyMaster tpm 
                                            INNER JOIN TaxPolicyPlantWise w on tpm.SystemID = w.TaxPolicyId
                                            LEFT JOIN TaxSlabDefineProfessional SDP on SDP.TaxPolicyMasterId = tpm.SystemID 
                                            where w.PlantId = '" + plantId + @"'";

                DataTable dtTaxMaxMin = _sqlRepository.GetDataTable(sqlTaxMaxMin);




                objRpt.GetFiscalMonthListSql(FromDate, ToDate, out dsMonth);
                //objRpt.GetMonthWiseEmpMonthlyAttdnInfo(FromDate, ToDate, dsMonth.Tables[0], out dsEmpAttdn);
                //dtEmpAttdn = dsEmpAttdn.Tables[0];GetFiscalYearWiseSalaryHeadValue
                Dictionary<string, List<DataRow>> dicPFTax = GetMonthWiseProfessionalTaxReports(FromDate, ToDate, companyGroupId, companyId, plantId, dsMonth.Tables[0]);
                // Dictionary<string, List<DataRow>> dicSalaryValue = objRpt.GetFiscalYearWiseSalaryHeadValue(FromDate, ToDate, identity.CompanyGroupId, identity.CompanyId, plantId, dsMonth.Tables[0]);

                DataTable dtMonthInfo = dsMonth.Tables[0];

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);

                objRpt.SelectedPlant(plantId, out dsFactory);

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
                SetHeaderValue("DOJ", sheet1, xlsRow, ref xlsCol, out int colDOJ, 25);
                SetHeaderValue("Fixed Gross", sheet1, xlsRow, ref xlsCol, out int colFixedGross, 11);
                SetHeaderValue("Professional tax Amount", sheet1, xlsRow, ref xlsCol, out int colProTaxAmount, 13);

                SetHeaderValue("Total Deduction", sheet1, xlsRow, ref xlsCol, out colTotalAmount, 12);


                //SetHeaderValue("", sheet1, xlsRow, ref xlsCol, out colWageLabel, 9.86);
                //sheet1.Range[xlsRow, colEmpName, xlsRow, colWageLabel].Merge();
                var colStart = colTotalAmount;
                CreateDynamicMonthHead(dtMonthInfo, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref colStart, out list);
                xlsCol--;
                //SetHeaderValue("%", sheet1, xlsRow, ref xlsCol, out colBonusPercentage, 12);
                //SetHeaderValue("Bonus Amt", sheet1, xlsRow, ref xlsCol, out colBonusAmount, 12);
                //SetHeaderValue("DOS", sheet1, xlsRow, ref xlsCol, out colDOS, 12);
                endXlsCol = xlsCol;
                var fPanRow = xlsRow + 1;

                #region ******************Report Header******************
                DataView view = new DataView(dicPFTax.Values.ElementAt(0)[0].Table);
                DataTable dtEmpInfo = view.ToTable(true, "EmpSystemId", "EmployeeCode", "EmployeeName", "BankName", "BankShortName", "BankAccNo", "DOS", "DOJ", "PaymentMode");

                double SalaryAmount = 0.00;


                xlsRow++;
                for (int dti = 0; dti < dtEmpInfo.Rows.Count; dti++)
                {
                    string empSystemId = dtEmpInfo.Rows[dti]["EmpSystemId"].ToString();
                    slCount++;
                    sheet1.Range[xlsRow, colSr].Text = slCount.ToString();
                    sheet1.Range[xlsRow, colSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, colSr].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1.Range[xlsRow, colSr, xlsRow + 1, colSr].Merge();
                    //sheet1.Range[xlsRow, colSr, xlsRow + 1, colSr].BorderAround(ExcelLineStyle.Hair);


                    sheet1.Range[xlsRow, colEmpCode].Text = dtEmpInfo.Rows[dti]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, colEmpCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, colEmpCode].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    if (dtEmpInfo.Rows[dti]["EmployeeCode"].ToString() == "10001167")
                    {
                        string dt = "";
                    }

                    //sheet1.Range[xlsRow, colEmpCode, xlsRow + 1, colEmpCode].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow, colEmpCode, xlsRow + 1, colEmpCode].Merge();


                    sheet1.Range[xlsRow, colEmpName].Text = dtEmpInfo.Rows[dti]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, colDOJ].Text = dtEmpInfo.Rows[dti]["DOJ"].ToString();
                    //sheet1.Range[xlsRow + 1, colEmpName].RowHeight = 19;

                    sheet1.Range[xlsRow, colSr, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);

                    SalaryAmount = 0.00;
                    try
                    {
                        if (dicPFTax.ContainsKey(empSystemId))
                        {
                            //double totalPayDay = 0.00;
                            double earningBonusAmount = 0.00;
                            bool isDecimal = false;
                            double decimalNo = 0;


                            List<DataRow> BonusList = dicPFTax[empSystemId];
                            try
                            {

                                earningBonusAmount = 0.00;
                                for (int BNS = 0; BNS < BonusList.Count; BNS++)
                                {

                                    //totalPayDay = 0.00;



                                    try
                                    {
                                        List<FiscalYearMonthSequence> _seq = list.Where(ee => ee.MonthNo == BonusList[BNS]["MonthNo"].ToString() && ee.MonthYear == BonusList[BNS]["YearNo"].ToString()).ToList();
                                        if (_seq.Count > 0)
                                        {
                                            //totalPayDay = clsStaticInfo.dbl(BonusList[BNS]["PayDays"].ToString());
                                            //if (BonusList[BNS]["HeadCategory"].ToString().ToUpper() == "GROSS")
                                            //{
                                            //    GrossAmount += clsStaticInfo.dbl(BonusList[BNS]["EntryAmount"].ToString());
                                            //    //totalPayDayYearly += clsStaticInfo.dbl(totalPayDay.ToString());

                                            //}
                                            if (BonusList[BNS]["HeadCategory"].ToString().ToUpper() == "PROFESSIONALTAX")
                                            {
                                                earningBonusAmount = Service.Extension.clsStaticInfo.dbl(BonusList[BNS]["DisbusmentAmount"].ToString()) * -1;
                                            }
                                            else
                                            {
                                                SalaryAmount += Service.Extension.clsStaticInfo.dbl(BonusList[BNS]["EntryAmount"].ToString());
                                            }
                                            //totalEarningAmountYearly += Convert.ToDouble(earningAmount);
                                            //totalEarningBonusAmountYearly += Convert.ToDouble(earningBonusAmount);

                                            isDecimal = bplib.clsWebLib.GetBoolData(BonusList[BNS]["IntegerInDisb"].ToString());
                                            decimalNo = Service.Extension.clsStaticInfo.dbl(BonusList[BNS]["DecimalNo"].ToString());
                                            if (earningBonusAmount == 0)
                                            {
                                                sheet1.Range[xlsRow, _seq[0].XLColIndex].Text = "-";// + Environment.NewLine + totalPayDay;                              
                                                sheet1.Range[xlsRow, _seq[0].XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                                sheet1.Range[xlsRow, _seq[0].XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                            }
                                            else
                                            {
                                                sheet1.Range[xlsRow, _seq[0].XLColIndex].Number = Convert.ToDouble(earningBonusAmount);// + Environment.NewLine + totalPayDay;
                                                sheet1.Range[xlsRow, _seq[0].XLColIndex].NumberFormat = GetDecimalFormat(isDecimal, Convert.ToInt32(decimalNo));
                                                sheet1.Range[xlsRow, _seq[0].XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                                sheet1.Range[xlsRow, _seq[0].XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                            }

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


                            var totalEarningSalaryFormula = "=SUM(" + ru.GetColumnNameForXls(colTotalAmount + 1) + xlsRow + ":" + ru.GetColumnNameForXls(colTotalAmount + dsMonth.Tables[0].Rows.Count) + xlsRow + ")";
                            //var totalPayDaysFormula = "=SUM(" + ru.GetColumnNameForXls(colWageLabel + 1) + (xlsRow + 1) + ":" + ru.GetColumnNameForXls(colWageLabel + 12) + (xlsRow + 1) + ")";
                            sheet1.Range[xlsRow, colFixedGross].Number = SalaryAmount;//totalEarningAmountYearly.ToString();
                            sheet1.Range[xlsRow, colFixedGross].NumberFormat = GetDecimalFormat(isDecimal, Convert.ToInt32(decimalNo));
                            var totalFixedPTax = 0.00;
                            for (int i = 0; i < dtTaxMaxMin.Rows.Count; i++)
                            {
                                if (SalaryAmount >= Service.Extension.clsStaticInfo.dbl(dtTaxMaxMin.Rows[i]["YearlyMinValue"].ToString()) && SalaryAmount <= Service.Extension.clsStaticInfo.dbl(dtTaxMaxMin.Rows[i]["YearlyMaxValue"].ToString()))
                                {
                                    totalFixedPTax = Service.Extension.clsStaticInfo.dbl(dtTaxMaxMin.Rows[i]["YearlyTaxAmount"].ToString());
                                }
                            }
                            sheet1.Range[xlsRow, colProTaxAmount].Number = totalFixedPTax;//totalEarningAmountYearly.ToString();


                            sheet1.Range[xlsRow, colTotalAmount].Formula = totalEarningSalaryFormula;//totalEarningAmountYearly.ToString();
                            sheet1.Range[xlsRow, colTotalAmount].NumberFormat = GetDecimalFormat(isDecimal, Convert.ToInt32(decimalNo));

                            sheet1.Range[xlsRow, colTotalAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, colTotalAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                            sheet1.Range[xlsRow, colSr, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, colSr, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);



                            xlsRow += 1;

                        }
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }

                }
                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);
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
                sheet1.Range[xlsRow, xlsCol].Text = "Professional Tax Report As of " + Convert.ToDateTime(FromDate).ToString("MMMM") + " : " + Convert.ToDateTime(FromDate).Year.ToString() + " TO " + Convert.ToDateTime(ToDate).ToString("MMMM") + "," + Convert.ToDateTime(ToDate).Year.ToString();
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
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;

                sheet1.Name = "ProfessionalTaxReport" + para.SalaryProcessId;
                #endregion

                return workbook;
                //return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);

                //}
            }
            catch (Exception ex)
            {
                throw ex;
                //throw new Exception(ex.Message);
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

        public Dictionary<string, List<DataRow>> GetMonthWiseProfessionalTaxReports(string fromDate, string toDate, string companyGroupId, string companyId, string plantId, DataTable dtMonth)
        {
            string salaryProcessId = "";
            string strSqlSal = @"SELECT  m.SystemID FROM SalaryProcMaster m
                                    INNER JOIN SalaryProcChild c on c.SlrProcMstSystemID=m.SystemID and c.PlantID='" + plantId + @"'
                                        WHERE 1=1 
                                        " + getMonthYear(fromDate, toDate, "YearNo", "MonthNo") + @"";

            DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSqlSal);
            salaryProcessId = "''";
            dtSalPrcId = dtSalPrcId.DefaultView.ToTable(true, "SystemID");
            for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
            {
                salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
            }


            string sqlTaxSalaryHead = @"select tpg.* from TaxPolicyMaster tpm 
                                    INNER join TaxPolicyPlantWise w on tpm.SystemID = w.TaxPolicyId
                                    left join TaxPolicyGeneral  TPG on TPG.TaxPolicyMstId = tpm.SystemID 
                                    where w.PlantId = '" + plantId + @"'";

            DataTable dtTaxSalaryHeadList = _sqlRepository.GetDataTable(sqlTaxSalaryHead);
            string salaryHeadId = "''";

            for (int si = 0; si < dtTaxSalaryHeadList.Rows.Count; si++)
            {
                salaryHeadId += ",'" + dtTaxSalaryHeadList.Rows[si]["SalaryHeadID"].ToString() + "'";
            }



            string strSql = @"SELECT  EmpSlr.PlantID, EmpSlr.FromDate, EmpSlr.ToDate,
								 EmpSlr.MonthNo, EmpSlr.YearNo
                                , EmpSlr.SalaryHead,EmpSlr.HeadCategory,EmpSlr.SalaryHeadID
                                , EmpSlr.EntryCurrencyID, EmpSlr.DisbusmentCurrencyID
								, EmpSlr.EmpInfoSystemID --TotalEmployee
								, EmpSlr.EntryAmount
                                , EmpSlr.DisbusmentAmount
								, EmpBasic.SystemId EmpSystemId
								, EmpBasic.EmployeeCode 
								, EmpBasic.EmployeeName
								, EmpBasic.PaymentMode
								, EmpBasic.BankName
								, EmpBasic.BankAccNo
								, EmpBasic.BankShortName
								, FORMAT(EmpBasic.DOS,'dd-MMM-yyyy') DOS
								, FORMAT(EmpBasic.DOJ,'dd-MMM-yyyy') DOJ
                                , ISNULL(MMDSA.TotalProcDate,0) - ISNULL(MMDSA.AbsentDays,0) PayDays
                                , ISNULL(EmpSlr.IntegerInDisb,0) IntegerInDisb
								, ISNULL(EmpSlr.DecimalNo,0)  DecimalNo

							FROM
                                    (
										 SELECT  E.SystemID, E.EmployeeCode, E.EmployeeName,E.DOB, E.DOJ,E.DOS, E.EmployeeStatus,DATEDIFF(YY,E.DOB,'" + fromDate + @"') As Age
											--DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,GVDE.UserName GivenDesignationName,
											,'' UserGroupSystemID, E.PlantID, F.UserName PlantName, E.UnitID,
											FU.UserName UnitName, E.DivisionID, DV.UserName DivisionName, E.DepartmentID, DP.UserName DepartmentName,
											E.SectionID, S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID,
											EC.UserName EmpCategoryName, Bank.ShortName BankShortName, Bank.UserName BankName, EBI.BankAccNo
                                            ,E.PaymentMode
                                     FROM EmployeeInformation E
												LEFT JOIN org.Plant F ON E.PlantID = F.Id
												LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupId = DG.ID
												LEFT JOIN hkp.LegalDesignation DE ON E.LegalDesignationId = DE.Id
												LEFT JOIN hkp.Designation GVDE ON E.GivenDesignationId = GVDE.Id
												LEFT JOIN org.Unit FU ON E.UnitID = FU.Id
												LEFT JOIN org.Division DV ON E.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON E.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON E.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON E.SubSectionID = SS.Id
												left join EmployeeBankInfo EBI ON EBI.EmpSystemID = E.SystemId
												left join  HKP.Bank Bank ON EBI.BankSystemID = Bank.Id
												LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.LegalDesignationId
                                    LEFT JOIN [HKP].EmployeeCategory EC ON EC.Id = DesM.EmployeeCategoryId												    
                                    --WHERE E.PlantId='" + plantId + @"'
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
                                                    ,sh.IsCTCComponent,sh.IsGrossComponent
                                                    ,CRC.IntegerInDisb, CRC.DecimalNo
											 FROM SalaryProcChild SPC
												INNER JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
																							AND SPM.SystemID IN(" + salaryProcessId + @")
												INNER JOIN SalaryHead SH ON SH.SalaryHeadID=SPC.SalaryHeadID                                                                      
                                            LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = SPC.SlrProcMstSystemID
																	LEFT JOIN CurrencyRuleMaster CRM ON CRM.SystemID = SRM.CurrencyRuleSystemID                                                                    
										LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = SH.SalaryHeadID
														--WHERE	SPC.PlantId = 	'" + plantId + @"'
											) EmpSlr ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID --AND EmpBasic.PlantID = EmpSlr.PlantID
                                    LEFT JOIN
		                                    (
											 SELECT EmpSystemID, MonthNo, YearNo, TotalProcDate, TotalPresent, TotalLate,
													TotalAbsent AbsentDays, TotalLv, TotalMLv, TotalCompAssignLv, TotalWeekOff, TotalHoliDay,
													TotalWeekOffHoliDay, TotalOTHr, TotalNormalOTHr, TotalExtraOTHr
				                              FROM SalaryProceAttdnData  WHERE " + getMonthYearWithoutAnd(fromDate, toDate, "YearNo", "MonthNo") + @" 
											) MMDSA ON EmpSlr.EmpInfoSystemID = MMDSA.EmpSystemID AND EmpSlr.MonthNo =  MMDSA.MonthNo AND EMPslr.YearNo = MMDSA.YearNo												   
													   WHERE 1=1
														and EmpBasic.PlantId = '" + plantId + @"' 
                                                " + getMonthYear(fromDate, toDate, "EmpSlr.YearNo", "EmpSlr.MonthNo") + @"
													AND (EmpSlr.HeadCategory IN ('ProfessionalTax') OR EmpSlr.SalaryHeadID IN (" + salaryHeadId + @"))  ORDER BY EmpSystemId";
            DataTable dt = _sqlRepository.GetDataTable(strSql);

            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
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




        public Dictionary<string, List<DataRow>> GetFiscalYearWiseSalaryHeadValue(string fromDate, string toDate, string companyGroupId, string companyId, string plantId, DataTable dtMonth)
        {
            string salaryProcessId = "";
            string strSqlSal = @"SELECT  m.SystemID FROM SalaryProcMaster m
                                    INNER JOIN SalaryProcChild c on c.SlrProcMstSystemID=m.SystemID and c.PlantID='" + plantId + @"'
                                        WHERE 1=1 
                                        " + getMonthYear(fromDate, toDate, "YearNo", "MonthNo") + @"";

            DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSqlSal);
            salaryProcessId = "''";
            dtSalPrcId = dtSalPrcId.DefaultView.ToTable(true, "SystemID");
            for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
            {
                salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
            }



            string strSql = @"SELECT  EmpSlr.PlantID, EmpSlr.FromDate, EmpSlr.ToDate,
								 EmpSlr.MonthNo, EmpSlr.YearNo
                                , EmpSlr.SalaryHead,EmpSlr.HeadCategory,EmpSlr.SalaryHeadID
                                , EmpSlr.EntryCurrencyID, EmpSlr.DisbusmentCurrencyID
								, EmpSlr.EmpInfoSystemID --TotalEmployee
								, EmpSlr.EntryAmount
                                , EmpSlr.DisbusmentAmount
								, EmpBasic.SystemId EmpSystemId
								, EmpBasic.EmployeeCode 
								, EmpBasic.EmployeeName
								, EmpBasic.PaymentMode
								, EmpBasic.BankName
								, EmpBasic.BankAccNo
								, EmpBasic.BankShortName
								, FORMAT(EmpBasic.DOS,'dd-MMM-yyyy') DOS
								, FORMAT(EmpBasic.DOJ,'dd-MMM-yyyy') DOJ
                                , ISNULL(MMDSA.TotalProcDate,0) - ISNULL(MMDSA.AbsentDays,0) PayDays
                                , ISNULL(EmpSlr.IntegerInDisb,0) IntegerInDisb
								, ISNULL(EmpSlr.DecimalNo,0)  DecimalNo

							FROM
                                    (
										 SELECT  E.SystemID, E.EmployeeCode, E.EmployeeName,E.DOB, E.DOJ,E.DOS, E.EmployeeStatus,DATEDIFF(YY,E.DOB,'" + fromDate + @"') As Age
											--DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,GVDE.UserName GivenDesignationName,
											,'' UserGroupSystemID, E.PlantID, F.UserName PlantName, E.UnitID,
											FU.UserName UnitName, E.DivisionID, DV.UserName DivisionName, E.DepartmentID, DP.UserName DepartmentName,
											E.SectionID, S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID,
											EC.UserName EmpCategoryName, Bank.ShortName BankShortName, Bank.UserName BankName, EBI.BankAccNo
                                            ,E.PaymentMode
                                     FROM EmployeeInformation E
												LEFT JOIN org.Plant F ON E.PlantID = F.Id
												LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupId = DG.ID
												LEFT JOIN hkp.LegalDesignation DE ON E.LegalDesignationId = DE.Id
												LEFT JOIN hkp.Designation GVDE ON E.GivenDesignationId = GVDE.Id
												LEFT JOIN org.Unit FU ON E.UnitID = FU.Id
												LEFT JOIN org.Division DV ON E.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON E.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON E.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON E.SubSectionID = SS.Id
												left join EmployeeBankInfo EBI ON EBI.EmpSystemID = E.SystemId
												left join  HKP.Bank Bank ON EBI.BankSystemID = Bank.Id
												LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.LegalDesignationId
                                    LEFT JOIN [HKP].EmployeeCategory EC ON EC.Id = DesM.EmployeeCategoryId												    
                                    --WHERE E.PlantId='" + plantId + @"'
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
                                                    ,sh.IsCTCComponent,sh.IsGrossComponent
                                                    ,CRC.IntegerInDisb, CRC.DecimalNo
											 FROM SalaryProcChild SPC
												INNER JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
																							AND SPM.SystemID IN(" + salaryProcessId + @")
												INNER JOIN SalaryHead SH ON SH.SalaryHeadID=SPC.SalaryHeadID                                                                      
                                            LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = SPC.SlrProcMstSystemID
																	LEFT JOIN CurrencyRuleMaster CRM ON CRM.SystemID = SRM.CurrencyRuleSystemID                                                                    
										LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = SH.SalaryHeadID
														--WHERE	SPC.PlantId = 	'" + plantId + @"'
											) EmpSlr ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID --AND EmpBasic.PlantID = EmpSlr.PlantID
                                    LEFT JOIN
		                                    (
											 SELECT EmpSystemID, MonthNo, YearNo, TotalProcDate, TotalPresent, TotalLate,
													TotalAbsent AbsentDays, TotalLv, TotalMLv, TotalCompAssignLv, TotalWeekOff, TotalHoliDay,
													TotalWeekOffHoliDay, TotalOTHr, TotalNormalOTHr, TotalExtraOTHr
				                              FROM SalaryProceAttdnData  WHERE " + getMonthYearWithoutAnd(fromDate, toDate, "YearNo", "MonthNo") + @" 
											) MMDSA ON EmpSlr.EmpInfoSystemID = MMDSA.EmpSystemID AND EmpSlr.MonthNo =  MMDSA.MonthNo AND EMPslr.YearNo = MMDSA.YearNo												   
													   WHERE 1=1
														and EmpBasic.PlantId = '" + plantId + @"' 
                                                " + getMonthYear(fromDate, toDate, "EmpSlr.YearNo", "EmpSlr.MonthNo") + @"
													AND EmpSlr.HeadCategory IN ('Gross','ProfessionalTax') ORDER BY EmpSystemId";
            DataTable dt = _sqlRepository.GetDataTable(strSql);

            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
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
        public string getMonthYear(string fromDate, string toDate, string monthNo, string yearNo)
        {
            var r = "";
            var _fDate = Convert.ToDateTime(fromDate);
            var _tDate = Convert.ToDateTime(toDate);
            while (_fDate < _tDate)
            {
                if (r.Length == 0)
                {
                    r = " (" + monthNo + " =" + _fDate.Year + " AND " + yearNo + " =" + _fDate.Month + ")";
                }
                else
                {
                    r += " OR (" + monthNo + " =" + _fDate.Year + " AND " + yearNo + " =" + _fDate.Month + ")";

                }
                _fDate = _fDate.AddMonths(1);

            }
            if (r.Length > 0)
            {
                r = " AND (" + r + ")";
            }

            return r;
        }
        public string getMonthYearWithoutAnd(string fromDate, string toDate, string monthNo, string yearNo)
        {
            var r = "";
            var _fDate = Convert.ToDateTime(fromDate);
            var _tDate = Convert.ToDateTime(toDate);
            while (_fDate < _tDate)
            {
                if (r.Length == 0)
                {
                    r = " (" + monthNo + " =" + _fDate.Year + " AND " + yearNo + " =" + _fDate.Month + ")";
                }
                else
                {
                    r += " OR (" + monthNo + " =" + _fDate.Year + " AND " + yearNo + " =" + _fDate.Month + ")";

                }
                _fDate = _fDate.AddMonths(1);

            }
            if (r.Length > 0)
            {
                r = " (" + r + ")";
            }

            return r;
        }

    }
}
