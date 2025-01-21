using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using Library.Service.EmployeeServices;
using bplib;
using Newtonsoft.Json;
using Library.Service.Helpers;
using System.IO;
using Syncfusion.XlsIO;
using System.Drawing;
using ConnectionManager;
using Library.Data;
using Library.Service.Payrolls.OT;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Hosting;
using System.Globalization;

namespace Library.HumanResource.NewOTProcess
{
    public class HourlyOTReportService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public HourlyOTReportService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
        public IWorkbook GetHourlyOT(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string FromDate, string ToDate)
        {

            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsHourlyOffDutyTag = null;
            DataTable dtHourlyOffDutyTag = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataView dvOT = null;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;
            #endregion
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;

                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                GetHourotReport(FromDate, ToDate, PlantId, out dsHourlyOffDutyTag);
                dtHourlyOffDutyTag = dsHourlyOffDutyTag.Tables[0];

                objRpt.SelectedPlantWiseCompany(PlantId, out dsCmp);
                objRpt.SelectedPlant(PlantId, out dsFactory);
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                var iName = 0;
                var iEmployeeCode = 0;
                var iSubSection = 0;
                var iSection = 0;
                var iDurationH = 0;
                var iLine = 0;
                var iWorkDate = 0;
                var iDOJ = 0;
                var iDepartment = 0;
                var iDesignation = 0;
                var iDayStatus = 0;
                var iGender = 0;
                var isl = 0;

                var SLNo = 1;

                if (dsHourlyOffDutyTag.Tables[0].Rows.Count == 0)
                {
                    throw new CustomException("No Data Found....");
                }
                #region Hourly Ot

                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];
                xlsRow = 6;

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
                sheet1.Range[xlsRow, iName].ColumnWidth = 18;

                xlsCol += 1;
                iDOJ = xlsCol;
                sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                sheet1.Range[xlsRow, iDOJ].ColumnWidth = 20;

                xlsCol += 1;
                iWorkDate = xlsCol;
                sheet1.Range[xlsRow, iWorkDate].Text = "Work Date";
                sheet1.Range[xlsRow, iWorkDate].ColumnWidth = 20;

                xlsCol += 1;
                iDayStatus = xlsCol;
                sheet1.Range[xlsRow, iDayStatus].Text = "Day Status";
                sheet1.Range[xlsRow, iDayStatus].ColumnWidth = 20;

                xlsCol += 1;
                iDepartment = xlsCol;
                sheet1.Range[xlsRow, iDepartment].Text = "Department";
                sheet1.Range[xlsRow, iDepartment].ColumnWidth = 30;

                xlsCol += 1;
                iDesignation = xlsCol;
                sheet1.Range[xlsRow, iDesignation].Text = "Designation";
                sheet1.Range[xlsRow, iDesignation].ColumnWidth = 30;

                xlsCol += 1;
                iGender = xlsCol;
                sheet1.Range[xlsRow, iGender].Text = "Gender";
                sheet1.Range[xlsRow, iGender].ColumnWidth = 30;

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
                iDurationH = xlsCol;
                sheet1.Range[xlsRow, iDurationH].Text = "Duration";
                sheet1.Range[xlsRow, iDurationH].ColumnWidth = 20;

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.LightYellow;

                xlsRow++;

                #endregion ------------------Column Header------------------

                for (int i = 0; i < dtHourlyOffDutyTag.Rows.Count; i++)
                {
                    #region ----------------------Data-----------------------  

                    dvOT = new DataView();
                    dvOT.Table = dsHourlyOffDutyTag.Tables[0];
                    string yot = string.Empty;
                    oru.GetOT(dsHourlyOffDutyTag.Tables[0].Rows[i]["OTConsiderOn"].ToString(), dsHourlyOffDutyTag.Tables[0].Rows[i]["Duration"].ToString(), out yot);

                    sheet1.Range[xlsRow, isl].Text = SLNo.ToString();
                    sheet1.Range[xlsRow, iEmployeeCode].Text = dtHourlyOffDutyTag.Rows[i]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, iName].Text = dtHourlyOffDutyTag.Rows[i]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, iDOJ].Text = dtHourlyOffDutyTag.Rows[i]["DOJ"].ToString();
                    sheet1.Range[xlsRow, iWorkDate].Text = dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString();
                    sheet1.Range[xlsRow, iDepartment].Text = dtHourlyOffDutyTag.Rows[i]["Department"].ToString();
                    sheet1.Range[xlsRow, iSection].Text = dtHourlyOffDutyTag.Rows[i]["Section"].ToString();
                    sheet1.Range[xlsRow, iSubSection].Text = dtHourlyOffDutyTag.Rows[i]["SubSection"].ToString();
                    sheet1.Range[xlsRow, iLine].Text = dtHourlyOffDutyTag.Rows[i]["Line"].ToString();
                    sheet1.Range[xlsRow, iDurationH].Text = yot.ToString();
                    sheet1.Range[xlsRow, iDayStatus].Text = dtHourlyOffDutyTag.Rows[i]["DayStatus"].ToString();
                    sheet1.Range[xlsRow, iDayStatus].Text = dtHourlyOffDutyTag.Rows[i]["DayStatus"].ToString();
                    sheet1.Range[xlsRow, iDesignation].Text = dtHourlyOffDutyTag.Rows[i]["Designation"].ToString();
                    sheet1.Range[xlsRow, iGender].Text = dtHourlyOffDutyTag.Rows[i]["GenderId"].ToString();

                    xlsRow++;
                    SLNo++;
                }
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                #endregion ----------------------Data-----------------------

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
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

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
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Hourly OT From : " + FromDate + " To " + ToDate;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
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
                sheet1.Name = "Hourly Ot";
                #endregion Page Setup


                #endregion  Attendance Summary Status

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetHourotReport(string FromDate, string ToDate, string plantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @" select ei.EmployeeName,ei.EmployeeCode,format(ei.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(ap.WorkDate,'dd-MMM-yyyy')WorkDate,s.UserName as Section,sb.UserName as SubSection,ld.UserName Designation
                                      ,d.UserName Department,ap.DayStatus,ei.GenderID,ap.AdditionalOT as Duration,
                                      ap.EmpSystemId,FORMAT(ap.WorkDate,'dd-MMM-yyyy hh:mm tt') WorkDate,
									  l.UserName as Line
                                      ,(cast(ap.AdditionalOT as decimal(18,2) )/60)as DurationH,
									  hr.OTConsiderOn                                        
                                      From AttdnProcessData ap 
                                      LEFT JOIN EmployeeInformation ei on ei.SystemId=ap.EmpSystemId
left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
							left join ORG.Position P on P.Id=MPB.PositionID
                                      LEFT JOIN [ORG].[Section] s on s.Id=p.SectionId
                                      LEFT JOIN [ORG].[SubSection] sb on sb.Id=p.SubSectionId
                                      LEFT JOIN [HKP].[LegalDesignation] ld on ld.Id=ei.LegalDesignationId
                                      LEFT JOIN [ORG].[Department] d on d.Id=p.DepartmentId
                                      LEFT JOIN [ORG].[Line] l on l.Id=mpb.LineId
                                      LEFT JOIN PlantWiseHRMSSetting hr on hr.PlantID=ap.PlantId     
                                    where ap.WorkDate 
									between '" + FromDate + "' and '" + ToDate + @"'  
									and ei.PlantId='" + plantId + @"' and ap.AdditionalOT is not null
                                    order by ei.EmployeeCode,ap.WorkDate";
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

        /// OT Final Information Report DataSet

        public void GetOTFinalRpt(string sPlantID, string frmDate, string toDate, string sUnit, string sDevi, string sDept, string sSect, string sSbSe, string sLine, string sEmpC, string sDeGr, string sDesi, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT A.* FROM
                                    (SELECT E.EmployeeCode, E.EmployeeName, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ,
                                            D.UserName Designation,ISNULL( LG.UserName, '') LegalDG, U.UserName Unit, Dv.UserName Division, Dp.UserName Department,
                                            S.UserName Section, SB.UserName SubSection, L.UserName Line
                                            ,ARIN.WorkDate,ARIN.StandardOT as TotalOTHr,DD.UserName GivenDesignation,hr.OTConsiderOn,E.EmployeeCodeNumeric
                                    FROM dbo.EmployeeInformation E
                                                LEFT JOIN dbo.AttdnProcessData ARIN ON E.SystemId = ARIN.EmpSystemID
                                                LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                                                LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                                                LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                                                LEFT JOIN ORG.Section S ON E.SectionID = S.Id
                                                LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                                                LEFT JOIN ORG.Line L ON E.LineID = L.Id
                                                LEFT JOIN HKP.LegalDesignation D ON E.LegalDesignationId = D.Id
                                                LEFT JOIN HKP.Designation DD ON E.GivenDesignationId = DD.Id
                                                LEFT join PlantWiseHRMSSetting hr on hr.PlantID=e.PlantId
                                                LEFT JOIN HKP.LegalDesignation LG ON LG.Id = E.LegalDesignationId
                                    WHERE E.PlantID = '" + sPlantID + @"' AND ARIN.WorkDate BETWEEN '" + frmDate +
                                    @"' AND '" + toDate + @"' AND ARIN.StandardOT > 0
AND (E.EmployeeStatus<>'Separated' OR DOS >= '" + frmDate + @"')
";


                strSql = strSql + @") A
                        GROUP BY A.EmployeeCode, A.EmployeeName, A.DOJ, A.Designation,A.LegalDG, A.Unit, A.Division, A.Department,
		                            A.Section, A.SubSection, A.Line, A.WorkDate, A.TotalOTHr, A.GivenDesignation,OTConsiderOn,A.EmployeeCodeNumeric
                        ORDER BY A.Unit, A.EmployeeCodeNumeric, A.Section, A.SubSection";

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
        }//End Fu

        /// Monthly Hourly OT Report

        #region  Hourly ot report Monthly ------------------------------------------
        public string NumberFormatTwoDecimal = "#,##0.00;(#,##0.00)";
        public IWorkbook GetHourlyOTMonthly(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string YearNo, string MonthNo, bool isActive, bool isSeperated)
        {

            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsHourlyOffDutyTag = null;
            DataTable dtHourlyOffDutyTag = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataView dvOT = null;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;

            // DataSet dsOTPolicy = null;
            DataSet dsSStructure = null;
            string _currencyId = string.Empty;
            Dictionary<string, double> dicNW = null;

            #endregion
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                objRpt.SelectedPlantWiseCompany(PlantId, out dsCmp);
                objRpt.SelectedPlant(PlantId, out dsFactory);
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                string strSql = "";
                strSql = @"	SELECT ad.* FROM 
								  hkp.AllowanceDaily ad
                                        left join DailyAllowanceRate dar on dar.DailyAllowanceId=ad.id AND dar.PlantId = ad.PlantId 
										where ad.PlantId = '" + PlantId + @"'";
                DataTable dtAllowanceRate = _sqlRepository.GetDataTable(strSql);


                Dictionary<string, string> dicStrSal = new Dictionary<string, string>();



                GetHourotmonthReport(YearNo, MonthNo, PlantId, CompanyId, CompanyGroupId, isActive, isSeperated, out dsHourlyOffDutyTag);
                dtHourlyOffDutyTag = dsHourlyOffDutyTag.Tables[0];

                DataTable DTAllowPolicy = new DataView(dsHourlyOffDutyTag.Tables[0]).ToTable(true, "IsAllDesignation", "IsFixed", "Rate", "FormulaDesID", "IsFixedFromRate", "ratear", "FormulaDesIDFromRate", "SystemId", "EmployeeCode");

                string FirstDayOfTheMonth = "01-" + MonthNo + "-" + YearNo;
                string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
                dicStrSal = objRpt.GetStructureBasic(FirstDayOfTheMonth, LastDayOfTheMonth, PlantId, "Basic");

                DataSet dsCurrency = null;
                clsOTCalculation otc = new clsOTCalculation();
                otc.LoadSalaryStructure(PlantId, FirstDayOfTheMonth, LastDayOfTheMonth, out dsSStructure);

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


                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                var iName = 0;
                var iEmployeeCode = 0;
                //var iDepartment = 0;
                var iSubSection = 0;
                var iSection = 0;
                var iBasic = 0;
                var iTotalHr = 0;
                var iRate = 0;
                var iAmount = 0;
                var iLine = 0;
                var totalAmount = 0.00;
                var iDOJ = 0;
                var iDepartment = 0;
                var iDesignation = 0;
                var itotal = 0;
                var totalEntryAmount = 0.00;
                var iGender = 0;
                var isl = 0;
                var SLNo = 1;

                if (dsHourlyOffDutyTag.Tables[0].Rows.Count == 0)
                {
                    Exception ex = new Exception("No Data Found....");
                    throw (ex);
                }
                #region Hourly Ot

                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];
                xlsRow = 6;

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
                int iDOS = xlsCol;
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
                iGender = xlsCol;
                sheet1.Range[xlsRow, iGender].Text = "Gender";
                sheet1.Range[xlsRow, iGender].ColumnWidth = 15;

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
                iTotalHr = xlsCol;
                sheet1.Range[xlsRow, iTotalHr].Text = "Total(Hrs)";
                sheet1.Range[xlsRow, iTotalHr].ColumnWidth = 15;


                xlsCol += 1;
                iBasic = xlsCol;
                sheet1.Range[xlsRow, iBasic].Text = "Basic";
                sheet1.Range[xlsRow, iBasic].ColumnWidth = 15;

                xlsCol += 1;
                iRate = xlsCol;
                sheet1.Range[xlsRow, iRate].Text = "Rate";
                sheet1.Range[xlsRow, iRate].ColumnWidth = 15;

                xlsCol += 1;
                iAmount = xlsCol;
                sheet1.Range[xlsRow, iAmount].Text = "Amount";
                sheet1.Range[xlsRow, iAmount].ColumnWidth = 15;

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;

                xlsRow++;

                #endregion ------------------Column Header------------------

                for (int i = 0; i < dtHourlyOffDutyTag.Rows.Count; i++)
                {
                    #region ----------------------Data-----------------------  
                    string yot = string.Empty;
                    string EmpSystemid = dtHourlyOffDutyTag.Rows[i]["Systemid"].ToString();
                    dvOT = new DataView();
                    dvOT.Table = dsHourlyOffDutyTag.Tables[0];

                    oru.GetOT(dsHourlyOffDutyTag.Tables[0].Rows[i]["OTConsiderOn"].ToString(), dsHourlyOffDutyTag.Tables[0].Rows[i]["Duration"].ToString(), out yot);

                    sheet1.Range[xlsRow, isl].Text = SLNo.ToString();
                    sheet1.Range[xlsRow, iName].Text = dtHourlyOffDutyTag.Rows[i]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, iEmployeeCode].Text = dtHourlyOffDutyTag.Rows[i]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, iDOJ].DateTime = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["DOJ"].ToString());
                    sheet1.Range[xlsRow, iDOJ].NumberFormat = "dd-MMM-yyyy";
                    if (!String.IsNullOrEmpty(dtHourlyOffDutyTag.Rows[i]["DOS"].ToString()))
                    {
                        sheet1.Range[xlsRow, iDOS].DateTime = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["DOS"].ToString());
                        sheet1.Range[xlsRow, iDOS].NumberFormat = "dd-MMM-yyyy";

                    }

                    sheet1.Range[xlsRow, iDesignation].Text = dtHourlyOffDutyTag.Rows[i]["Designation"].ToString();
                    sheet1.Range[xlsRow, iDepartment].Text = dtHourlyOffDutyTag.Rows[i]["Department"].ToString();
                    sheet1.Range[xlsRow, iSection].Text = dtHourlyOffDutyTag.Rows[i]["Section"].ToString();
                    sheet1.Range[xlsRow, iSubSection].Text = dtHourlyOffDutyTag.Rows[i]["SubSection"].ToString();
                    sheet1.Range[xlsRow, iLine].Text = dtHourlyOffDutyTag.Rows[i]["Line"].ToString();
                    sheet1.Range[xlsRow, iGender].Text = dtHourlyOffDutyTag.Rows[i]["GenderId"].ToString();

                    sheet1.Range[xlsRow, iTotalHr].Text = yot;
                    sheet1.Range[xlsRow, iTotalHr].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, iTotalHr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //sheet1.Range[xlsRow, itotal].Number = clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["Duration"].ToString());
                    totalEntryAmount += clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["Duration"].ToString());

                    //basic


                    string entryAmount = dicStrSal[EmpSystemid];

                    sheet1.Range[xlsRow, iBasic].Number = clsStaticInfo.dbl(entryAmount);
                    sheet1.Range[xlsRow, iBasic].NumberFormat = NumberFormatTwoDecimal;

                    //rate
                    sheet1.Range[xlsRow, iRate].Number = clsStaticInfo.dbl(dicNW[EmpSystemid]);
                    sheet1.Range[xlsRow, iRate].NumberFormat = NumberFormatTwoDecimal;

                    //Amount
                    sheet1.Range[xlsRow, iAmount].Number = clsStaticInfo.dbl(dicNW[EmpSystemid]) * clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["DurationH"].ToString());
                    sheet1.Range[xlsRow, iAmount].NumberFormat = NumberFormatTwoDecimal;
                    totalAmount += clsStaticInfo.dbl(dicNW[EmpSystemid]) * clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["DurationH"].ToString());

                    xlsRow++;
                    SLNo++;
                }

                string Zot = string.Empty;
                oru.GetOT(dsHourlyOffDutyTag.Tables[0].Rows[0]["OTConsiderOn"].ToString(), totalEntryAmount.ToString(), out Zot);

                sheet1.Range[xlsRow, iLine].Text = "Total";
                sheet1.Range[xlsRow, iLine + 1].Text = Zot;

                sheet1.Range[xlsRow, iLine + 4].Number = totalAmount;
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                #endregion ----------------------Data-----------------------

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
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
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
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Hourly OT Monthly From " + FirstDayOfTheMonth + " TO " + LastDayOfTheMonth;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
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
                sheet1.Name = "Hourly OT Monthly";
                #endregion Page Setup

                #endregion  

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetHourotmonthReport(string YearNo, string MonthNo, string plantId, string companyId, string companyGroupId, bool isActive, bool isSeperated, out DataSet dsRef)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            string FirstDayOfTheMonth = "01-" + MonthNo + "-" + YearNo;
            string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            try
            {
                string wcDos = "AND (1=0";

                if (isActive == true && isSeperated == true)
                {
                    wcDos = " AND (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') = ''";
                    }
                    if (isSeperated == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') <> ''";
                    }
                }

                wcDos += ")";

                strSql = @"select ei.SystemId,ei.EmployeeName,ei.EmployeeCode,format(ei.DOJ,'dd-MMM-yyyy') DOJ,format(ei.DOS,'dd-MMM-yyyy') DOS,s.UserName as Section,sb.UserName as SubSection,ld.UserName Designation
                           ,d.UserName Department,ei.GenderID,ap.EmpSystemId,l.UserName as Line,hr.OTConsiderOn--,YY.EntryAmount
                                 ,sum(ap.AdditionalOT)as Duration,sum(CAST(ap.AdditionalOT AS decimal)/60)as DurationH

                               ,ad.IsAllDesignation--1
                               ,isnull(ad.IsFixed,0)as IsFixed---1--rate--0-farmula
                               ,isnull(ad.Rate,0) as Rate
                               ,ad.FormulaDesID
                               ,isnull(dar.IsFixed,0)as IsFixedFromRate--1--rate--0--farmula
                               ,isnull(dar.rate,0)as ratear
                               ,dar.FormulaDesID FormulaDesIDFromRate

                                 FROM AttdnProcessData ap 
                                 LEFT JOIN EmployeeInformation ei on ei.SystemId=ap.EmpSystemId
                                 LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = ei.BudgetCode
								LEFT OUTER JOIN [ORG].[Position] AS PO ON PO.Id = MB.PositionId
                                LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId
                                 LEFT JOIN [ORG].[Section] s on s.Id=PO.SectionId
                                 LEFT JOIN [ORG].[SubSection] sb on sb.Id=PO.SubSectionId
                                 LEFT JOIN [HKP].[LegalDesignation] ld on ld.Id=ei.LegalDesignationId
                                 LEFT JOIN [ORG].[Department] d on d.Id=ent.DepartmentId
                                 LEFT JOIN [ORG].[Line] l on l.Id=MB.LineId
                                 LEFT JOIN PlantWiseHRMSSetting hr on hr.PlantID=ei.PlantId   
                                 LEFT JOIN hkp.AllowanceDaily ad on ad.PlantID=ei.PlantId AND ad.Catagory='HourlyOffDuty' AND ad.Active=1
                                 LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = EI.LegalDesignationId
                                LEFT join  [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                                   left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
                               left join HKP.Designation DeG on DeG.Id=dm.DesignationId
                                  left join HKP.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
                                 LEFT JOIN DailyAllowanceRate dar on dar.DailyAllowanceId=ad.id AND ei.PlantId = ad.PlantId AND dar.DesignationId=ei.GivenDesignationId

                               WHERE ap.AdditionalOT is not null AND ap.WorkDate between '" + FirstDayOfTheMonth + "' and '" + LastDayOfTheMonth + @"' " + wcDos + @" AND ei.plantid='" + identity.PlantId + @"' 
                              
                               GROUP BY  EmployeeName,EmployeeCode ,ei.SystemId,DOJ,s.UserName,sb.UserName,ld.UserName,d.UserName,ei.GenderID,ap.EmpSystemId,l.UserName,hr.OTConsiderOn
                              ,ad.IsAllDesignation  ,ad.IsFixed ,ad.FormulaDesID,dar.IsFixed,dar.FormulaDesID,ad.Rate,dar.rate ,ei.DOS
                             
                               ORDER BY ei.EmployeeCode
                                    ";


                clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSql, out dsRef);
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
                    GetFormulaAllRate(DTAllowPolicy, dsSalaryStruc, _currencyId, _empid, out nwRate);
                    dicNW.Add(_empid, nwRate);
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
            string AllDesignation = string.Empty;
            string IsFixed = string.Empty;
            string Rate = string.Empty;
            string FormulaDes = string.Empty;
            string IsFixedFromRate = string.Empty;
            string ratear = string.Empty;
            string FormulaDesIDFromRate = string.Empty;
            try
            {
                DataView dv = new DataView(dsPolicy);
                dv.RowFilter = "SystemID='" + empid + "'";
                if (dv.Count > 0)
                {
                    //"IsAllDesignation", "IsFixed", "Rate", "FormulaDesID", "IsFixedFromRate", "ratear", "FormulaDesIDFromRate", "SystemId"
                    AllDesignation = dv[0]["IsAllDesignation"].ToString();
                    IsFixed = dv[0]["IsFixed"].ToString();
                    Rate = dv[0]["Rate"].ToString();
                    FormulaDes = dv[0]["FormulaDesID"].ToString();
                    IsFixedFromRate = dv[0]["IsFixedFromRate"].ToString();
                    ratear = dv[0]["ratear"].ToString();
                    FormulaDesIDFromRate = dv[0]["FormulaDesIDFromRate"].ToString();
                    string EmployeeCode = dv[0]["EmployeeCode"].ToString();

                    string formula = string.Empty;
                    if (clsWebLib.RetValidLen(AllDesignation).ToString() != "")
                    {
                        if (clsWebLib.GetBoolData(AllDesignation.ToString()) == true)
                        {
                            if (clsWebLib.GetBoolData(IsFixed) == true)
                            {
                                nwRate = clsStaticInfo.dbl(Rate);
                            }
                            else
                            {
                                formula = FormulaDes;
                            }
                        }
                    }
                    else
                    {
                        if (clsWebLib.GetBoolData(IsFixedFromRate.ToString()) == true)
                        {
                            nwRate = clsStaticInfo.dbl(ratear);
                        }
                        else
                        {
                            formula = FormulaDesIDFromRate;
                        }
                    }

                    if (clsWebLib.GetBoolData(IsFixed) == false && clsWebLib.GetBoolData(IsFixedFromRate) == false)
                    {
                        if (string.IsNullOrEmpty(formula))
                        {
                            throw new Exception("Employee " + EmployeeCode + " has no Rate Formula in allowance setting ...");
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
                        else
                        {
                            throw new Exception("Employee " + EmployeeCode + " has no Salary sturcture ...");
                        }
                    }


                }//if dv

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

        #endregion

    }

    public class IndividualOTReportService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public IndividualOTReportService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        #region Functions
        void GenerateDic(DataSet dsPolicy, DataSet dsSalaryStruc, string _currencyId, out Dictionary<string, double> dicNW, out Dictionary<string, double> dicW, out Dictionary<string, double> dicH)
        {
            double nwRate = 0;
            double wRate = 0;
            double hRate = 0;
            dicNW = null;
            dicW = null;
            dicH = null;
            try
            {
                DataTable dtemp = new DataView(dsSalaryStruc.Tables[0]).ToTable(true, "EmpInfoSystemID");
                dicNW = new Dictionary<string, double>();
                dicW = new Dictionary<string, double>();
                dicH = new Dictionary<string, double>();
                for (int i = 0; i < dtemp.Rows.Count; i++)
                {
                    string _empid = dtemp.Rows[i]["EmpInfoSystemID"].ToString();
                    GetFormula(dsPolicy, dsSalaryStruc, _currencyId, _empid, out nwRate, out wRate, out hRate);
                    dicNW.Add(_empid, nwRate);
                    dicW.Add(_empid, wRate);
                    dicH.Add(_empid, hRate);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void GetFormula(DataSet dsPolicy, DataSet dsSalaryStruc, string _currencyId, string empid, out double nwRate, out double wRate, out double hRate)
        {
            nwRate = 0;
            wRate = 0;
            hRate = 0;
            //out string FormulaDesIDN, out string FormulaDesIDW, out string FormulaDesIDH
            string FormulaDesIDN = string.Empty;
            string FormulaDesIDW = string.Empty;
            string FormulaDesIDH = string.Empty;
            try
            {
                DataView dv = new DataView(dsPolicy.Tables[0]);
                dv.RowFilter = "systemid='" + empid + "'";
                if (dv.Count > 0)
                {
                    FormulaDesIDN = dv[0]["FormulaDesIDN"].ToString();
                    FormulaDesIDW = dv[0]["FormulaDesIDW"].ToString();
                    FormulaDesIDH = dv[0]["FormulaDesIDH"].ToString();
                    string EmployeeCode = dv[0]["EmployeeCode"].ToString();

                    if (string.IsNullOrEmpty(FormulaDesIDN))
                    {
                        throw new Exception("Employee " + EmployeeCode + " has no OT policy with her/his designation ...");
                    }


                    DataView dvss = new DataView(dsSalaryStruc.Tables[0]);
                    dvss.RowFilter = "EmpInfoSystemID='" + empid + "'";
                    if (dvss.Count > 0)
                    {
                        string FormulaValue = string.Empty;
                        DataTable dtValue = dvss.ToTable();
                        DataTable dtSalaryHead = dvss.ToTable(true, "SalaryHeadID", "SalaryHead");


                        GetFormulValue(FormulaDesIDH, ref dtValue, _currencyId, out hRate, ref dtSalaryHead);

                        GetFormulValue(FormulaDesIDW, ref dtValue, _currencyId, out wRate, ref dtSalaryHead);

                        GetFormulValue(FormulaDesIDN, ref dtValue, _currencyId, out nwRate, ref dtSalaryHead);

                    }//if
                }//if dv

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

        public DataTable GetIndividualDailyOT(string FromDate, string ToDate, string OTDuration, string OTfinal, string plantId, string companyId, string companyGroupId)
        {
            string strSql = string.Empty;
            try
            {
                if (OTfinal == "ConfirmOT")
                {
                    strSql = @"SELECT ei.EmployeeName,ei.systemid
                            ,ei.EmployeeCode
                            ,FORMAT(apd.WorkDate,'dd-MMM-yyyy') WorkDate
                            ,FORMAT(ei.DOJ,'dd-MMM-yyyy') DOJ
                            ,Section.UserName as Section,Section.Sequence SectionSequence,
							Section.ID SectionId,SubSection.UserName as SubSection,
							ld.UserName Designation,Department.UserName Department,
							Department.Id DepartmentId,Department.Sequence DepartmentSequence
                             
	,sum(apd.StandardOT) AS TotalOT, sum(apd.StandardOT)/60 AS TotalOTH
                            ,pwhr.OTConsiderOn
                            ,l.UserName Line
                            ,dt.OriginalDayType DayType
                        ,apd.InTime,apd.OutTime,apd.AdditionalOT as RealOt
                             FROM AttdnProcessData apd
                            left join DayType dt on dt.DayType=apd.DayStatus
                             left join EmployeeInformation ei on ei.SystemId=apd.EmpSystemID
                             LEFT JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = ei.BudgetCode
								                LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
												
									LEFT OUTER JOIN ORG.Position PO ON MB.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON MB.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    Left join org.Line L on L.Id = MB.LineId
                            LEFT JOIN [HKP].[LegalDesignation] ld ON ld.Id=ei.LegalDesignationId
                            LEFT JOIN [dbo].[PlantWiseHRMSSetting] pwhr ON pwhr.PlantID=ei.PlantID
                            WHERE apd.WorkDate BETWEEN '"+FromDate+"' AND '"+ToDate+@"'  AND 
							apd.plantid='"+plantId+@"' 
                            AND apd.IsOTComfirm=1 AND apd.IsOTEntitled=1
                            GROUP BY EI.EmployeeName,EI.EmployeeCode,apd.WorkDate,ei.DOJ,
							Section.UserName,Section.ID,SubSection.UserName
							,Department.Sequence,Section.Sequence
                            ,ld.UserName ,Department.UserName,Department.Id,pwhr.OTConsiderOn,
							l.UserName ,EmployeeCodePreFix,ei.systemid
							 ,EmployeeCodeNumeric,dt.OriginalDayType,apd.InTime,apd.OutTime,
							 apd.AdditionalOT
				            HAVING  SUM(ISNULL(apd.StandardOT,0))/60 > '"+OTDuration+@"'
                            ORDER BY ISNULL(EmployeeCodePreFix,'') ASC, 
							ISNULL(EmployeeCodeNumeric,0) ASC";
                }
                else
                {
                    strSql = @" SELECT ei.EmployeeName,ei.systemid
                            ,ei.EmployeeCode
                            ,FORMAT(apd.WorkDate,'dd-MMM-yyyy') WorkDate
                            ,format(ei.DOJ,'dd-MMM-yyyy')DOJ
 ,Section.UserName as Section,Section.Sequence SectionSequence,Section.ID SectionId,SubSection.UserName as SubSection,ld.UserName Designation,Department.UserName Department,Department.Id DepartmentId,Department.Sequence DepartmentSequence
                            ,sum(apd.StandardOT) AS TotalOT
                            ,sum(apd.StandardOT)/60 AS TotalOTH
                            ,pwhr.OTConsiderOn
                            ,l.UserName Line
                            ,apd.InTime
							,apd.OutTime
                            ,dt.OriginalDayType DayType
							,apd.AdditionalOT RealOt
                             FROM AttdnProcessData apd 
                            left join DayType dt on dt.DayType=apd.DayStatus
                             left join EmployeeInformation ei on ei.SystemId=apd.EmpSystemID
                             LEFT JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = ei.BudgetCode
								                LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
											
									LEFT OUTER JOIN ORG.Position PO ON MB.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON MB.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    Left join org.Line L on L.Id = MB.LineId
                                    LEFT JOIN HKP.LegalDesignation Ld on LD.Id = ei.LegalDesignationId									

                            LEFT join [dbo].[PlantWiseHRMSSetting] pwhr ON pwhr.PlantID=apd.PlantID
                            WHERE apd.WorkDate BETWEEN '"+FromDate+"' and '"+ToDate+@"' 
							and apd.IsOTEntitled=1 and apd.plantid='"+plantId+@"'
                             GROUP BY EI.EmployeeName,EI.EmployeeCode,ei.DOJ,Section.UserName,Section.ID,SubSection.UserName
							,Department.Sequence,Section.Sequence,apd.WorkDate,apd.InTime,apd.OutTime
                            ,ld.UserName ,Department.UserName,Department.Id,pwhr.OTConsiderOn,l.UserName ,EmployeeCodePreFix,ei.systemid
							,apd.AdditionalOT ,EmployeeCodeNumeric,dt.OriginalDayType
                            having  sum(isnull(apd.StandardOT,0))/60 > '"+OTDuration+@"'
                            ORDER BY ISNULL(EmployeeCodePreFix,'') ASC, 
							ISNULL(EmployeeCodeNumeric,0) ASC";
                }
                return _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function

        #endregion

        public IWorkbook GetIndividualDailyOT(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string FromDate, string ToDate, string OTDuration, bool CheckBox, string OTfinal, string filePath = "")
        {

            #region declare
            clsReport objRpt = null;
            clsReport objRptSR = null;
            ReportUtility oru = new ReportUtility();
            DataTable dtHourlyOffDutyTag = null;
            DataView dvOT = null;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;

            DataSet dsOTPolicy = null;//
            DataSet dsSStructure = null;
            string _currencyId = string.Empty;
            Dictionary<string, double> dicNW = null;
            Dictionary<string, double> dicW = null;
            Dictionary<string, double> dicH = null;
            #endregion
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;

                IWorkbook workbook = application.Workbooks.Create(4);

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), CompanyId + ".jpg");  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                #region OT Rate
                if (CheckBox)
                {
                    DataSet dsCurrency = null;
                    clsOTCalculation otc = new clsOTCalculation();
                    otc.LoadOverTimePolicy(PlantId, FromDate, ToDate, out dsOTPolicy);
                    otc.LoadSalaryStructure(PlantId, FromDate, ToDate, out dsSStructure);

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

                    GenerateDic(dsOTPolicy, dsSStructure, _currencyId, out dicNW, out dicW, out dicH);
                }
                #endregion

                dtHourlyOffDutyTag = GetIndividualDailyOT(FromDate, ToDate, OTDuration, OTfinal, PlantId, CompanyId, CompanyGroupId);


                var dtCmp = objRptSR.SelectedPlantWiseCompanyDT(PlantId);

                var dtFactory = objRptSR.SelectedPlantDT(PlantId);
                #region Variable
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                var iName = 0;
                var iEmployeeCode = 0;
                var iSubSection = 0;
                var iSection = 0;
                var iTotalHr = 0;
                var iTotalHrReal = 0;
                var iOutTime = 0;
                var iInTime = 0;

                var iLine = 0;
                var iDOJ = 0;
                var iDepartment = 0;
                var iDesignation = 0;
                var isl = 0;
                var iWorkDate = 0;
                var SLNo = 1;
                #endregion

                if (dtHourlyOffDutyTag.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }
                #region Hourly Ot
                //workbook = application.Workbooks.Create(4);
                IWorksheet sheet1 = null;
                IWorksheet sheet2 = null;
                IWorksheet sheet3 = null;
                IWorksheet sheet4 = null;

                sheet1 = workbook.Worksheets[0];
                sheet2 = workbook.Worksheets[1];
                sheet3 = workbook.Worksheets[2];
                sheet4 = workbook.Worksheets[3];


                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;

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
                iWorkDate = xlsCol;
                sheet1.Range[xlsRow, iWorkDate].Text = "Work Date";
                sheet1.Range[xlsRow, iWorkDate].ColumnWidth = 12;

                xlsCol += 1;
                iInTime = xlsCol;
                sheet1.Range[xlsRow, iInTime].Text = "In Time";
                sheet1.Range[xlsRow, iInTime].ColumnWidth = 12;

                xlsCol += 1;
                iOutTime = xlsCol;
                sheet1.Range[xlsRow, iOutTime].Text = "Out Time";
                sheet1.Range[xlsRow, iOutTime].ColumnWidth = 12;

                xlsCol += 1;
                iTotalHr = xlsCol;
                sheet1.Range[xlsRow, iTotalHr].Text = "OT Hours";
                sheet1.Range[xlsRow, iTotalHr].ColumnWidth = 15;
                xlsCol += 1;
                iTotalHrReal = xlsCol;
                sheet1.Range[xlsRow, iTotalHrReal].Text = "Total(Hours)";
                sheet1.Range[xlsRow, iTotalHrReal].ColumnWidth = 15;

                int inw_rate = 0;
                int iAmount = 0;
                //int iw_rate = 0;
                //int ih_rate = 0;
                if (CheckBox)
                {
                    xlsCol += 1;
                    inw_rate = xlsCol;
                    sheet1.Range[xlsRow, inw_rate].Text = "Rate";
                    sheet1.Range[xlsRow, inw_rate].ColumnWidth = 12;

                    //xlsCol += 1;
                    //iw_rate = xlsCol;
                    //sheet1.Range[xlsRow, iw_rate].Text = "Weekoff Rate";
                    //sheet1.Range[xlsRow, iw_rate].ColumnWidth = 12;

                    //xlsCol += 1;
                    //ih_rate = xlsCol;
                    //sheet1.Range[xlsRow, ih_rate].Text = "Holiday Rate";
                    //sheet1.Range[xlsRow, ih_rate].ColumnWidth = 12;

                    xlsCol += 1;
                    iAmount = xlsCol;
                    sheet1.Range[xlsRow, iAmount].Text = "Amount";
                    sheet1.Range[xlsRow, iAmount].ColumnWidth = 12;
                }

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, isl, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;


                xlsRow++;

                #endregion ------------------Column Header------------------


                int startXlsRow = xlsRow;
                for (int i = 0; i < dtHourlyOffDutyTag.Rows.Count; i++)
                {
                    #region ----------------------Data-----------------------  
                    string yot = string.Empty;
                    double nwRate = 0;
                    try
                    {
                        #region OT Rate

                        string _empid = dtHourlyOffDutyTag.Rows[i]["systemid"].ToString();
                        string _daytype = dtHourlyOffDutyTag.Rows[i]["daytype"].ToString();
                        //if(_empid== "2001587")
                        //{
                        //    //if(conv dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString()=Convert.ToDateTime("04-jul-2020"))
                        //    var kk = dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString();
                        if (CheckBox)
                        {
                            //GetFormula(dsOTPolicy, dsSStructure, _currencyId, _empid, _daytype, out nwRate);
                            ///GenerateDic(dsOTPolicy, dsSStructure, _currencyId,out dic);
                            try
                            {
                                if (_daytype.ToUpper() == "W")
                                {
                                    nwRate = dicW[_empid];
                                }
                                else if (_daytype.ToUpper() == "H")
                                {
                                    nwRate = dicH[_empid];
                                }
                                else
                                {
                                    nwRate = dicNW[_empid];
                                }
                            }
                            catch (Exception ex)
                            {


                            }

                        }
                        //}
                        #endregion

                        oru.GetOT(dtHourlyOffDutyTag.Rows[i]["OTConsiderOn"].ToString(), dtHourlyOffDutyTag.Rows[i]["TotalOT"].ToString(), out yot);
                        sheet1.Range[xlsRow, isl].Text = SLNo.ToString();
                        sheet1.Range[xlsRow, iName].Text = dtHourlyOffDutyTag.Rows[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, iEmployeeCode].Text = dtHourlyOffDutyTag.Rows[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, iDOJ].Text = dtHourlyOffDutyTag.Rows[i]["DOJ"].ToString();
                        sheet1.Range[xlsRow, iDesignation].Text = dtHourlyOffDutyTag.Rows[i]["Designation"].ToString();
                        sheet1.Range[xlsRow, iDepartment].Text = dtHourlyOffDutyTag.Rows[i]["Department"].ToString();
                        sheet1.Range[xlsRow, iSection].Text = dtHourlyOffDutyTag.Rows[i]["Section"].ToString();
                        sheet1.Range[xlsRow, iSubSection].Text = dtHourlyOffDutyTag.Rows[i]["SubSection"].ToString();
                        sheet1.Range[xlsRow, iLine].Text = dtHourlyOffDutyTag.Rows[i]["Line"].ToString();
                        sheet1.Range[xlsRow, iWorkDate].Text = dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString();

                        //sheet1.Range[xlsRow, iOutTime].NumberFormat = "hh:mm AM/PM";
                        //sheet1.Range[xlsRow, iOutTime].Text = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["OutTime"].ToString()).ToString("hh:mm tt", CultureInfo.InvariantCulture);
                        //sheet1.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";

                        if (String.IsNullOrEmpty(dtHourlyOffDutyTag.Rows[i]["OutTime"].ToString()))
                        {
                            sheet1.Range[xlsRow, iOutTime].Text = "";
                        }
                        else
                        {
                            sheet1.Range[xlsRow, iOutTime].Text = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["OutTime"].ToString()).ToString("hh:mm tt", CultureInfo.InvariantCulture);
                        }

                        if (String.IsNullOrEmpty(dtHourlyOffDutyTag.Rows[i]["InTime"].ToString()))
                        {
                            sheet1.Range[xlsRow, iInTime].Text = "";
                        }
                        else
                        {
                            sheet1.Range[xlsRow, iInTime].Text = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["InTime"].ToString()).ToString("hh:mm tt", CultureInfo.InvariantCulture);
                        }

                        sheet1.Range[xlsRow, iTotalHr].Text = yot;
                        sheet1.Range[xlsRow, iTotalHrReal].Number = clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["TotalOTH"].ToString());
                        if (CheckBox)
                        {
                            sheet1.Range[xlsRow, inw_rate].Number = nwRate;
                            double amt = clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["TotalOTH"].ToString()) * nwRate;
                            sheet1.Range[xlsRow, iAmount].Number = amt;
                        }

                        xlsRow++;
                        SLNo++;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                sheet1.Range[startXlsRow, iTotalHr, xlsRow, iTotalHr].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, iOutTime].Text = "Total";

                sheet1.Range[xlsRow, iOutTime, xlsRow, iOutTime].CellStyle.Font.Bold = true;

                string totalOT = "";
                string totalOTCal = "";

                object sumObject;
                sumObject = dtHourlyOffDutyTag.Compute("Sum(TotalOT)", "");
                totalOT = sumObject.ToString();
                oru.GetOT(dtHourlyOffDutyTag.Rows[0]["OTConsiderOn"].ToString(), totalOT, out totalOTCal);

                sheet1.Range[xlsRow, iTotalHr].Text = totalOTCal;

                sheet1.Range[xlsRow, iTotalHr, xlsRow, iTotalHr].CellStyle.Font.Bold = true;

                if (CheckBox)
                {
                    sheet1.Range[xlsRow, iAmount].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(iAmount) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(iAmount) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, iOutTime, xlsRow, iAmount].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, iOutTime, xlsRow, iAmount].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, iAmount, xlsRow, iAmount].CellStyle.Font.Bold = true;
                }
                else
                {
                    sheet1.Range[xlsRow, iOutTime, xlsRow, iTotalHr].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, iOutTime, xlsRow, iTotalHr].BorderAround(ExcelLineStyle.Hair);
                }


                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;
                int sheetEndXlsRow = xlsRow;
                #endregion ----------------------Data-----------------------

                #region ******************Report Header******************



                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(isl) + sheet1.GetColumnWidth(iEmployeeCode);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
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
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
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
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Excess OT From " + FromDate + " TO " + ToDate;
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
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
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
                sheet1.Name = "Excess OT";
                #endregion Page Setup

                #endregion  Individual Daily OT
                string fPath = filePath;
                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = HostingEnvironment.MapPath("~/") + "TempIndividualDailyOT.xlsx";
                }

                workbook.SaveAs(filePath);
                workbook = application.Workbooks.Open(filePath);
                IWorksheet worksheet = workbook.Worksheets[0];
                try
                {

                    #region PivotSheet1
                    IWorksheet pivotSheet = workbook.Worksheets[1];
                    pivotSheet.Name = "Summary";


                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = pivotSheet.GetColumnWidth(isl) + pivotSheet.GetColumnWidth(iEmployeeCode);
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
                    //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    xlsRow += 1;
                    pivotSheet.Range[xlsRow, 3].Text = "Excess OT From " + FromDate + " TO " + ToDate;
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
                    pivotSheet.Range[xlsRow + 1, 1].RowHeight = 20;
                    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;


                    IPivotCache cache = workbook.PivotCaches.Add(worksheet["A6:" + oru.GetColumnNameForXls(endXlsCol) + (sheetEndXlsRow - 1)]);

                    #region Second Pivot table
                    pivotSheet.Range[xlsRow + 2, 1].Text = "Total Manpower in OverTime";
                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable2 = pivotSheet.PivotTables.Add("PivotTable2", pivotSheet["A8"], cache);

                    //Add Pivot table fields (Row and Column fields)
                    pivotTable2.Fields[iDepartment - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[iSection - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[iTotalHrReal - 2].Axis = PivotAxisTypes.Column;


                    IPivotTable pivotTable2_1 = pivotSheet.PivotTables["PivotTable2"];
                    pivotTable2_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable2_1.Options.ShowDrillIndicators = false;

                    pivotTable2_1.DisplayFieldCaptions = true;


                    //Add data field
                    IPivotField field2 = pivotTable2_1.Fields[iTotalHrReal - 1];
                    pivotTable2_1.DataFields.Add(field2, "Total Employees", PivotSubtotalTypes.Count);
                    pivotTable2_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;


                    int totalColumns = pivotTable2_1.RowFields.Count + pivotTable2_1.ColumnFields.Count;
                    for (int i = 0; i < pivotTable2_1.ColumnFields.Count; i++)
                    {
                        totalColumns += pivotTable2_1.ColumnFields[i].Items.Count;
                    }

                    int lastCloumn = totalColumns;

                    #endregion


                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].Text = "OverTime Hours Summary";
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Bold = true;



                    //Create "PivotTable1" with the cache at the specified range
                    IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet[clsStaticInfo.GetxlsCol(lastCloumn + 2) + "8"], cache);

                    //Add Pivot table fields (Row and Column fields)
                    pivotTable.Fields[iDepartment - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[iSection - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[iTotalHrReal - 2].Axis = PivotAxisTypes.Column;


                    IPivotTable pivotTable1 = pivotSheet.PivotTables["PivotTable1"];
                    pivotTable1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable1.Options.ShowDrillIndicators = false;

                    pivotTable1.DisplayFieldCaptions = true;
                    pivotTable1.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;


                    //Add data field
                    IPivotField field = pivotTable.Fields[iTotalHrReal - 1];
                    pivotTable.DataFields.Add(field, "Total Hours", PivotSubtotalTypes.Sum);




                    pivotSheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    pivotSheet.IsGridLinesVisible = false;

                    #endregion
                    if (string.IsNullOrEmpty(fPath))
                    {
                        #region PivotSheet2

                        IWorksheet pivotSheet2 = workbook.Worksheets[2];
                        pivotSheet2.Name = "OT SUMMARY Sec";
                        pivotSheet2.IsGridLinesVisible = false;

                        try
                        {
                            if (companyLogo != null)
                            {
                                double totalWidth = pivotSheet2.GetColumnWidth(isl) + pivotSheet2.GetColumnWidth(iEmployeeCode);
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
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                        pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                        pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                        pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                        xlsRow += 1;

                        pivotSheet2.Range[xlsRow, 3].Text = FactoryName;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                        pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                        pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                        xlsRow += 1;

                        pivotSheet2.Range[xlsRow, 3].Text = FactoryAddress;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                        pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                        pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                        xlsRow += 1;
                        pivotSheet2.Range[xlsRow, 3].Text = "OT Summary From " + FromDate + " TO " + ToDate;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                        pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                        pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                        pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                        #endregion
                        pivotSheet2.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                        pivotSheet2.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                        pivotSheet2.Range[xlsRow + 1, 1].RowHeight = 20;
                        pivotSheet2.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;

                        IPivotTable pivotTableSec = pivotSheet2.PivotTables.Add("PivotTableSEC", pivotSheet2["A7"], cache);

                        //Add Pivot table fields (Row and Column fields)
                        pivotTableSec.Fields[iDepartment - 1].Axis = PivotAxisTypes.Row;
                        pivotTableSec.Fields[iSection - 1].Axis = PivotAxisTypes.Row;
                        pivotTableSec.Fields[iTotalHrReal - 2].Axis = PivotAxisTypes.Column;


                        IPivotTable pivotTableSec_1 = pivotSheet2.PivotTables["PivotTableSEC"];
                        pivotTableSec_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                        pivotTableSec_1.Options.ShowDrillIndicators = false;

                        pivotTableSec_1.DisplayFieldCaptions = true;


                        //Add data field
                        IPivotField fieldSec = pivotTableSec_1.Fields[iTotalHrReal - 1];
                        pivotTableSec_1.DataFields.Add(fieldSec, "Hours", PivotSubtotalTypes.Sum);

                        IPivotField fieldSec2 = pivotTableSec_1.Fields[iTotalHrReal - 1];
                        pivotTableSec_1.DataFields.Add(fieldSec2, "Employees", PivotSubtotalTypes.Count);

                        if (CheckBox)
                        {
                            IPivotField fieldSec3 = pivotTableSec_1.Fields[iAmount - 1];
                            pivotTableSec_1.DataFields.Add(fieldSec3, "Amount", PivotSubtotalTypes.Sum);
                        }

                        pivotTableSec_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;



                        pivotSheet2.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                        #endregion

                        #region PivotSheet3

                        IWorksheet pivotSheet3 = workbook.Worksheets[3];
                        pivotSheet3.Name = "OT SUMMARY Dept.";
                        pivotSheet3.IsGridLinesVisible = false;

                        try
                        {

                            if (companyLogo != null)
                            {
                                double totalWidth = pivotSheet3.GetColumnWidth(1) + pivotSheet3.GetColumnWidth(2);
                                int totalWidthPixel = (int)(totalWidth * 7.5);
                                int totalheight = (int)((pivotSheet3.GetRowHeight(1) + pivotSheet3.GetRowHeight(2) + pivotSheet3.GetRowHeight(3) + pivotSheet3.GetRowHeight(3)) * 1.50);

                                companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                                IPictureShape pic = null;

                                pic = pivotSheet3.Pictures.AddPicture(1, 1, companyLogo);

                            }
                        }
                        catch (Exception ex)
                        {
                        }

                        #region Report Header
                        xlsRow = 1;
                        xlsCol = 1;


                        pivotSheet3.Range[xlsRow, 3].Text = CmpName;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                        pivotSheet3.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                        pivotSheet3.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                        pivotSheet3.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        pivotSheet3.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                        xlsRow += 1;

                        pivotSheet3.Range[xlsRow, 3].Text = FactoryName;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                        pivotSheet3.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                        pivotSheet3.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        pivotSheet3.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                        xlsRow += 1;

                        pivotSheet3.Range[xlsRow, 3].Text = FactoryAddress;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                        pivotSheet3.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                        pivotSheet3.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        pivotSheet3.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                        xlsRow += 1;
                        pivotSheet3.Range[xlsRow, 3].Text = "OT Summary From " + FromDate + " TO " + ToDate;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                        pivotSheet3.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                        pivotSheet3.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                        pivotSheet3.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        pivotSheet3.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                        #endregion
                        pivotSheet3.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                        pivotSheet3.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                        pivotSheet3.Range[xlsRow + 1, 1].RowHeight = 20;
                        pivotSheet3.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;

                        IPivotTable pivotTableDept = pivotSheet3.PivotTables.Add("PivotTableDept", pivotSheet2["A7"], cache);

                        //Add Pivot table fields (Row and Column fields)
                        pivotTableDept.Fields[iDepartment - 1].Axis = PivotAxisTypes.Row;
                        //pivotTableDept.Fields[iSection - 1].Axis = PivotAxisTypes.Row;
                        pivotTableDept.Fields[iTotalHrReal - 2].Axis = PivotAxisTypes.Column;


                        IPivotTable pivotTableDept_1 = pivotSheet3.PivotTables["PivotTableDept"];
                        pivotTableDept_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                        pivotTableDept_1.Options.ShowDrillIndicators = false;

                        pivotTableDept_1.DisplayFieldCaptions = true;


                        //Add data field
                        IPivotField fieldDept = pivotTableDept_1.Fields[iTotalHrReal - 1];
                        pivotTableDept_1.DataFields.Add(fieldDept, "Hours", PivotSubtotalTypes.Sum);

                        IPivotField fieldDept2 = pivotTableDept_1.Fields[iTotalHrReal - 1];
                        pivotTableDept_1.DataFields.Add(fieldDept2, "Employees", PivotSubtotalTypes.Count);

                        if (CheckBox)
                        {
                            IPivotField fieldDept3 = pivotTableDept_1.Fields[iAmount - 1];
                            pivotTableDept_1.DataFields.Add(fieldDept3, "Amount", PivotSubtotalTypes.Sum);
                        }
                        pivotTableDept_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;



                        pivotSheet3.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                        #endregion
                    }

                }
                catch (Exception)
                {

                }
                try
                {
                    worksheet.ShowColumn(iTotalHrReal, false);
                    if (string.IsNullOrEmpty(fPath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch (Exception)
                {
                }
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}

