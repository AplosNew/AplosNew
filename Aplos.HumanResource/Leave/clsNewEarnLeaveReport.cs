using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Leave
{
    public class clsNewEarnLeaveReport
    {
        ISqlRepository _sqlRepository;
        public clsNewEarnLeaveReport()
        {
            _sqlRepository = new SqlRepository();
        }
        #region OLD system EL Report
        public string GetReport(string FromDate, string ToDate)
        {
            #region Variable

            clsReport objRpt = null;

            ReportUtility oru = new ReportUtility();

            DataSet dsAttn = null;

            DataView dvAttn = null;
            DataSet dsFactory = null;
            DataSet dsCmp = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";

            var report = new ReportUtility();
            var iSl = 0;
            var iEmpCode = 0;
            var iEmpName = 0;
            var iGender = 0;
            var iDOJ = 0;
            var iFromDate = 0;
            var iToDate = 0;
            var iEmpCategory = 0;
            var iDepartment = 0;
            var iSection = 0;
            var iSubSection = 0;
            var iLine = 0;
            var iDesignation = 0;
            var iEarningDays = 0;
            var iRate = 0;
            var iPresentDays = 0;
            var iTotalEL = 0;
            var iEncashed = 0;
            var iTotalLeave = 0;
            var iBalance = 0;
            var iAmount = 0;
            var iStamp = 0;
            var iNetAmount = 0;
            var iSignature = 0;

            #endregion Variable

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string companyId = identity.CompanyId;
                objRpt = new clsReport();


                #region DataSet
                getEmployee(FromDate, ToDate, out dsAttn);

                dvAttn = new DataView();
                dvAttn.Table = dsAttn.Tables[0];

                objRpt.SelectedPlantWiseCompany(identity.PlantId.Trim(), out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);
                #endregion DataSet

                if (dvAttn.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;

                    xlsRow = 7;
                    int intRow = 0;

                    int strCount = 0;

                    #region ------------------Column Header------------------
                    xlsCol = 1;

                    iSl = xlsCol;
                    sheet1.Range[xlsRow, iSl].Text = "Sl No.";
                    xlsCol++;

                    iEmpCode = xlsCol;
                    sheet1.Range[xlsRow, iEmpCode].Text = "Employee Code";
                    xlsCol++;

                    iEmpName = xlsCol;
                    sheet1.Range[xlsRow, iEmpName].Text = "Employee Name";
                    xlsCol++;

                    iGender = xlsCol;
                    sheet1.Range[xlsRow, iGender].Text = "Gender";
                    xlsCol++;

                    iDOJ = xlsCol;
                    sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                    xlsCol++;

                    iFromDate = xlsCol;
                    sheet1.Range[xlsRow, iFromDate].Text = "From Date";
                    xlsCol++;

                    iToDate = xlsCol;
                    sheet1.Range[xlsRow, iToDate].Text = "To Date";
                    xlsCol++;

                    iEmpCategory = xlsCol;
                    sheet1.Range[xlsRow, iEmpCategory].Text = "Emp. Category";
                    xlsCol++;

                    iDepartment = xlsCol;
                    sheet1.Range[xlsRow, iDepartment].Text = "Department";
                    xlsCol++;

                    iSection = xlsCol;
                    sheet1.Range[xlsRow, iSection].Text = "Section";
                    xlsCol++;

                    iSubSection = xlsCol;
                    sheet1.Range[xlsRow, iSubSection].Text = "Sub Section";
                    xlsCol++;

                    iLine = xlsCol;
                    sheet1.Range[xlsRow, iLine].Text = "Line";
                    xlsCol++;

                    iDesignation = xlsCol;
                    sheet1.Range[xlsRow, iDesignation].Text = "Designation";
                    xlsCol++;

                    iPresentDays = xlsCol;
                    sheet1.Range[xlsRow, iPresentDays].Text = "Present Days";
                    xlsCol++;

                    iEarningDays = xlsCol;
                    sheet1.Range[xlsRow, iEarningDays].Text = "Calculated Earning Days";
                    xlsCol++;

                    iTotalEL = xlsCol;
                    sheet1.Range[xlsRow, iTotalEL].Text = "Total EL";
                    xlsCol++;

                    iEncashed = xlsCol;
                    sheet1.Range[xlsRow, iEncashed].Text = "Encashed";
                    xlsCol++;

                    iTotalLeave = xlsCol;
                    sheet1.Range[xlsRow, iTotalLeave].Text = "Total Avail Leave";
                    xlsCol++;

                    iBalance = xlsCol;
                    sheet1.Range[xlsRow, iBalance].Text = "Balance";
                    xlsCol++;

                    iRate = xlsCol;
                    sheet1.Range[xlsRow, iRate].Text = "Rate";
                    xlsCol++;

                    iAmount = xlsCol;
                    sheet1.Range[xlsRow, iAmount].Text = "Amount";
                    xlsCol++;

                    iStamp = xlsCol;
                    sheet1.Range[xlsRow, iStamp].Text = "Stamp";
                    xlsCol++;

                    iNetAmount = xlsCol;
                    sheet1.Range[xlsRow, iNetAmount].Text = "Net Amount";
                    xlsCol++;

                    iSignature = xlsCol;
                    sheet1.Range[xlsRow, iSignature].Text = "Signature";
                    sheet1.Range[xlsRow, iSignature].ColumnWidth = 17;

                    endXlsCol = xlsCol;

                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Rotation = 90;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 81;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].ColumnWidth = 81;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                    xlsCol = 1;
                    xlsRow += 1;
                    int startRow = xlsRow;
                    #endregion ------------------Column Header------------------
                    strCount = 0;
                    for (int i = 0; i < dvAttn.Count; i++)
                    {
                        xlsCol = 1;
                        xlsRow += intRow;
                        intRow = 1;
                        #region ----------------------Data-----------------------
                        strCount++;
                        sheet1.Range[xlsRow, iSl].Text = strCount.ToString();
                        sheet1.Range[xlsRow, iEmpCode].Text = dvAttn[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, iEmpName].Text = dvAttn[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, iGender].Text = dvAttn[i]["GenderID"].ToString();
                        sheet1.Range[xlsRow, iDOJ].Text = dvAttn[i]["DOJ"].ToString();
                        sheet1.Range[xlsRow, iFromDate].Text = dvAttn[i]["FromDate"].ToString();
                        sheet1.Range[xlsRow, iToDate].Text = dvAttn[i]["ToDate"].ToString();
                        sheet1.Range[xlsRow, iEmpCategory].Text = dvAttn[i]["EmployeeCategory"].ToString();
                        sheet1.Range[xlsRow, iDepartment].Text = dvAttn[i]["Department"].ToString();
                        sheet1.Range[xlsRow, iSection].Text = dvAttn[i]["Section"].ToString();
                        sheet1.Range[xlsRow, iSubSection].Text = dvAttn[i]["SubSection"].ToString();
                        sheet1.Range[xlsRow, iLine].Text = dvAttn[i]["Line"].ToString();
                        sheet1.Range[xlsRow, iDesignation].Text = dvAttn[i]["Designation"].ToString();
                        sheet1.Range[xlsRow, iEarningDays].Number = clsStaticInfo.dbl(dvAttn[i]["CalculatedEarningDays"].ToString());
                        sheet1.Range[xlsRow, iRate].Number = clsStaticInfo.dbl(dvAttn[i]["Rate"].ToString());
                        sheet1.Range[xlsRow, iPresentDays].Number = clsStaticInfo.dbl(dvAttn[i]["TotalPresent"].ToString());
                        sheet1.Range[xlsRow, iTotalEL].Number = clsStaticInfo.dbl(dvAttn[i]["TotalEarnLeave"].ToString());
                        sheet1.Range[xlsRow, iEncashed].Number = clsStaticInfo.dbl(dvAttn[i]["Encashed"].ToString());
                        sheet1.Range[xlsRow, iTotalLeave].Number = clsStaticInfo.dbl(dvAttn[i]["AvailedLeave"].ToString());
                        sheet1.Range[xlsRow, iBalance].Number = clsStaticInfo.dbl(dvAttn[i]["Balance"].ToString());

                        sheet1.Range[xlsRow, iAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(iRate) + xlsRow + "*" + clsStaticInfo.GetxlsCol(iEncashed) + (xlsRow) + ")";
                        sheet1[xlsRow, iAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                        sheet1.Range[xlsRow, iNetAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(iAmount) + xlsRow + "-" + clsStaticInfo.GetxlsCol(iStamp) + (xlsRow) + ")";
                        sheet1[xlsRow, iNetAmount].NumberFormat = "#,##0.00;(#,##0.00)";



                        #endregion 

                    }
                    int EndRow = xlsRow;
                    sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                    sheet1.Range[startRow, 1, EndRow, endXlsCol].RowHeight = 56;

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

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
                    sheet1.Range[xlsRow, 4].Text = CmpName;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 4].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 4].CellStyle.Font.Size = 12;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].RowHeight = 30;
                    sheet1.Range[xlsRow, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                        //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, 4].Text = FactoryName;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 4].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet1.Range[xlsRow, 4].Text = FactoryAddress;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 4].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].RowHeight = 26;
                    sheet1.Range[xlsRow, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, 4].Text = "Earn Leave Report";
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 4].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 4].CellStyle.Font.Size = 11;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 7;
                    #endregion

                    #region Page Setup
                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
                    //sheet1.PageSetup.PrintTitleRows = "$1:$2";
                    sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.UserId.Trim() + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    sheet1.PageSetup.LeftMargin = 0.5;
                    sheet1.PageSetup.RightMargin = 0.2;
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                    sheet1.Name = "Earn Leave";
                    #endregion             

                    workbook.Version = ExcelVersion.Excel97to2003;
                    report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);

                    // return workbook;

                    var filePath = "";
                    var SheetName = "Earn Leave";
                    //return workbook;
                    workbook.Version = ExcelVersion.Excel97to2003;
                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xls");
                    workbook.SaveAs(filePath);
                    workbook.Close();
                    excelEngine.Dispose();
                    return filePath;
                }
                else
                {
                    throw new Exception("No Data found...");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }
        public void getEmployee(string FromDate, string ToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT *
                                	,(ISNULL(t.TotalEarnLeave, 0) - ISNULL(t.Encashed, 0) - ISNULL(t.AvailedLeave, 0)) Balance --TotalEarnleaveValue=(TotalEarnLeave*Rate)
                                FROM (
                                	SELECT ei.EmployeeCode
                                		,ei.EmployeeName
                                		,ei.GenderID
                                		,FORMAT(ei.DOJ, 'dd-MMM-yyyy') DOJ
                                		,En.UserName EmployeeCategory
                                		,dp.UserName Department
                                		,SE.UserName Section
                                		,ISNULL(Li.UserName, '') Line
                                		,SuS.Username SubSection
                                		,Deg.UserName Designation
                                		,FORMAT(els.FromDate, 'dd-MMM-yyyy') FromDate
                                		,FORMAT(els.ToDate, 'dd-MMM-yyyy') ToDate
                                		,L.Rate
                                		,els.CalculatedEarningDays
                                		,els.CurrentYearAllocation CurrentPeriodAllocation
                                		,(els.CurrentYearAllocation + els.BroughtForward + els.CarryForwardOpeningBalance) TotalEarnLeave
                                		,L.Days Encashed
                                		,(
                                			SELECT SUM(ltdx.LeaveDuration)
                                			FROM LeaveTransaction AS ltx
                                			JOIN LeaveTransactionDetails AS ltdx ON ltdx.LvTrnsSystemID = ltx.SystemID
                                			WHERE ltx.IsApproved = 1
                                				AND ltdx.WorkDate BETWEEN els.FromDate
                                					AND els.ToDate
                                				AND ltx.EmpSystemID = L.EmpSystemId
                                				AND ltx.LTSystemID = els.LeaveTypeId
                                			) AvailedLeave
                                		--,Encashed-
                                		,(
                                			SELECT COUNT(*)
                                			FROM AttdnProcessData AS apdx
                                			JOIN DayType AS dtx ON dtx.DayType = apdx.DayStatus
                                			WHERE dtx.Category IN (
                                					'Present'
                                					,'Late'
                                					)
                                				AND apdx.WorkDate BETWEEN els.FromDate
                                					AND els.ToDate
                                				AND apdx.EmpSystemID = L.EmpSystemId
                                			) AS TotalPresent
                                	FROM LeaveEncashmentTransaction L
                                	JOIN TRN.EmployeeLeaveSummary AS els ON els.EmployeeId = L.EmpSystemId
                                		AND L.EncashmentDate = els.ToDate
                                	JOIN EmployeeInformation AS ei ON ei.SystemId = L.EmpSystemId
                                		AND ei.SystemId = els.EmployeeId
                                	LEFT JOIN MST.ManpowerBudget PMB ON ei.BudgetCode = PMB.Id
                                	LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                                	LEFT JOIN ORG.Entity En ON PMB.EntityId = En.Id
                                	LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                                	LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = L.LegalDesignationId
                                	LEFT JOIN [MST].[DesignationMasterLegalDesignation] dmld ON dmld.LegalDesignationId = LGD.Id
                                	LEFT JOIN [MST].[DesignationMaster] dm ON dm.Id = dmld.DesignationMasterId
                                	LEFT JOIN HKP.Designation DeG ON DeG.Id = dm.DesignationId
                                	LEFT JOIN HKP.EmployeeCategory EC ON EC.Id = dm.EmployeeCategoryId
                                	LEFT JOIN ORG.Section SE ON SE.Id = PR.SectionId
                                	LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                                	LEFT JOIN ORG.Line AS Li ON Li.Id = PMB.LineId
                                	WHERE L.EncashmentDate BETWEEN ('" + FromDate + @"')
                                			AND ('" + ToDate + @"')
                                	) AS T ORDER BY T.EmployeeCode";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                objCon.getDataSet(strSql, out dsRef);
                objCon.CommitTransaction();
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

        #region New system EL Report
        public string GetNewReport(string FromDate, string ToDate)
        {
            #region Variable

            clsReport objRpt = null;

            ReportUtility oru = new ReportUtility();

            DataSet dsAttn = null;

            DataView dvAttn = null;
            DataSet dsFactory = null;
            DataSet dsCmp = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";

            var report = new ReportUtility();
            var iSl = 0;
            var iEmpCode = 0;
            var iEmpName = 0;
            var iGender = 0;
            var iDOJ = 0;
            var iFromDate = 0;
            var iToDate = 0;
            var iEmpCategory = 0;
            var iDepartment = 0;
            var iSection = 0;
            var iSubSection = 0;
            var iLine = 0;
            var iDesignation = 0;
            var iEarningDays = 0;
            var iRate = 0;
            var iPresentDays = 0;
            var iTotalEL = 0;
            var iEncashed = 0;
            var iTotalLeave = 0;
            var iBalance = 0;
            var iAmount = 0;
            var iStamp = 0;
            var iNetAmount = 0;
            var iSignature = 0;

            #endregion Variable

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string companyId = identity.CompanyId;
                objRpt = new clsReport();


                #region DataSet
                getNewEmployee(FromDate, ToDate, out dsAttn);

                dvAttn = new DataView();
                dvAttn.Table = dsAttn.Tables[0];

                objRpt.SelectedPlantWiseCompany(identity.PlantId.Trim(), out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);
                #endregion DataSet

                if (dvAttn.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;

                    xlsRow = 7;
                    int intRow = 0;

                    int strCount = 0;

                    #region ------------------Column Header------------------
                    xlsCol = 1;

                    iSl = xlsCol;
                    sheet1.Range[xlsRow, iSl].Text = "Sl No.";
                    xlsCol++;

                    iEmpCode = xlsCol;
                    sheet1.Range[xlsRow, iEmpCode].Text = "Employee Code";
                    xlsCol++;

                    iEmpName = xlsCol;
                    sheet1.Range[xlsRow, iEmpName].Text = "Employee Name";
                    xlsCol++;

                    iGender = xlsCol;
                    sheet1.Range[xlsRow, iGender].Text = "Gender";
                    xlsCol++;

                    iDOJ = xlsCol;
                    sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                    xlsCol++;

                    iFromDate = xlsCol;
                    sheet1.Range[xlsRow, iFromDate].Text = "From Date";
                    xlsCol++;

                    iToDate = xlsCol;
                    sheet1.Range[xlsRow, iToDate].Text = "To Date";
                    xlsCol++;

                    iEmpCategory = xlsCol;
                    sheet1.Range[xlsRow, iEmpCategory].Text = "Emp. Category";
                    xlsCol++;

                    iDepartment = xlsCol;
                    sheet1.Range[xlsRow, iDepartment].Text = "Department";
                    xlsCol++;

                    iSection = xlsCol;
                    sheet1.Range[xlsRow, iSection].Text = "Section";
                    xlsCol++;

                    iSubSection = xlsCol;
                    sheet1.Range[xlsRow, iSubSection].Text = "Sub Section";
                    xlsCol++;

                    iLine = xlsCol;
                    sheet1.Range[xlsRow, iLine].Text = "Line";
                    xlsCol++;

                    iDesignation = xlsCol;
                    sheet1.Range[xlsRow, iDesignation].Text = "Designation";
                    xlsCol++;

                    iPresentDays = xlsCol;
                    sheet1.Range[xlsRow, iPresentDays].Text = "Present Days";
                    xlsCol++;

                    iEarningDays = xlsCol;
                    sheet1.Range[xlsRow, iEarningDays].Text = "Calculated Earning Days";
                    xlsCol++;

                    iTotalEL = xlsCol;
                    sheet1.Range[xlsRow, iTotalEL].Text = "Total EL";
                    xlsCol++;

                    iEncashed = xlsCol;
                    sheet1.Range[xlsRow, iEncashed].Text = "Encashed";
                    xlsCol++;

                    iTotalLeave = xlsCol;
                    sheet1.Range[xlsRow, iTotalLeave].Text = "Total Avail Leave";
                    xlsCol++;

                    iBalance = xlsCol;
                    sheet1.Range[xlsRow, iBalance].Text = "Balance";
                    xlsCol++;

                    iRate = xlsCol;
                    sheet1.Range[xlsRow, iRate].Text = "Rate";
                    xlsCol++;

                    iAmount = xlsCol;
                    sheet1.Range[xlsRow, iAmount].Text = "Amount";
                    xlsCol++;

                    iStamp = xlsCol;
                    sheet1.Range[xlsRow, iStamp].Text = "Stamp";
                    xlsCol++;

                    iNetAmount = xlsCol;
                    sheet1.Range[xlsRow, iNetAmount].Text = "Net Amount";
                    xlsCol++;

                    iSignature = xlsCol;
                    sheet1.Range[xlsRow, iSignature].Text = "Signature";
                    sheet1.Range[xlsRow, iSignature].ColumnWidth = 17;

                    endXlsCol = xlsCol;

                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Rotation = 90;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 81;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].ColumnWidth = 81;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                    xlsCol = 1;
                    xlsRow += 1;
                    int startRow = xlsRow;
                    #endregion ------------------Column Header------------------
                    strCount = 0;
                    for (int i = 0; i < dvAttn.Count; i++)
                    {
                        xlsCol = 1;
                        xlsRow += intRow;
                        intRow = 1;
                        #region ----------------------Data-----------------------
                        strCount++;
                        sheet1.Range[xlsRow, iSl].Text = strCount.ToString();
                        sheet1.Range[xlsRow, iEmpCode].Text = dvAttn[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, iEmpName].Text = dvAttn[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, iGender].Text = dvAttn[i]["GenderID"].ToString();
                        sheet1.Range[xlsRow, iDOJ].Text = dvAttn[i]["DOJ"].ToString();
                        sheet1.Range[xlsRow, iFromDate].Text = dvAttn[i]["FromDate"].ToString();
                        sheet1.Range[xlsRow, iToDate].Text = dvAttn[i]["ToDate"].ToString();
                        sheet1.Range[xlsRow, iEmpCategory].Text = dvAttn[i]["EmployeeCategory"].ToString();
                        sheet1.Range[xlsRow, iDepartment].Text = dvAttn[i]["Department"].ToString();
                        sheet1.Range[xlsRow, iSection].Text = dvAttn[i]["Section"].ToString();
                        sheet1.Range[xlsRow, iSubSection].Text = dvAttn[i]["SubSection"].ToString();
                        sheet1.Range[xlsRow, iLine].Text = dvAttn[i]["Line"].ToString();
                        sheet1.Range[xlsRow, iDesignation].Text = dvAttn[i]["Designation"].ToString();
                        sheet1.Range[xlsRow, iEarningDays].Number = clsStaticInfo.dbl(dvAttn[i]["CalculatedEarningDays"].ToString());
                        sheet1.Range[xlsRow, iRate].Number = clsStaticInfo.dbl(dvAttn[i]["Rate"].ToString());
                        sheet1.Range[xlsRow, iPresentDays].Number = clsStaticInfo.dbl(dvAttn[i]["TotalPresent"].ToString());
                        sheet1.Range[xlsRow, iTotalEL].Number = clsStaticInfo.dbl(dvAttn[i]["TotalEarnLeave"].ToString());
                        sheet1.Range[xlsRow, iEncashed].Number = clsStaticInfo.dbl(dvAttn[i]["Encashed"].ToString());
                        sheet1.Range[xlsRow, iTotalLeave].Number = clsStaticInfo.dbl(dvAttn[i]["AvailedLeave"].ToString());
                        sheet1.Range[xlsRow, iBalance].Number = clsStaticInfo.dbl(dvAttn[i]["Balance"].ToString());

                        sheet1.Range[xlsRow, iAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(iRate) + xlsRow + "*" + clsStaticInfo.GetxlsCol(iEncashed) + (xlsRow) + ")";
                        sheet1[xlsRow, iAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                        sheet1.Range[xlsRow, iNetAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(iAmount) + xlsRow + "-" + clsStaticInfo.GetxlsCol(iStamp) + (xlsRow) + ")";
                        sheet1[xlsRow, iNetAmount].NumberFormat = "#,##0.00;(#,##0.00)";



                        #endregion 

                    }
                    int EndRow = xlsRow;
                    sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                    sheet1.Range[startRow, 1, EndRow, endXlsCol].RowHeight = 56;

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

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
                    sheet1.Range[xlsRow, 4].Text = CmpName;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 4].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 4].CellStyle.Font.Size = 12;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].RowHeight = 30;
                    sheet1.Range[xlsRow, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                        //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, 4].Text = FactoryName;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 4].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet1.Range[xlsRow, 4].Text = FactoryAddress;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 4].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].RowHeight = 26;
                    sheet1.Range[xlsRow, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, 4].Text = "Earn Leave Report";
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 4].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 4].CellStyle.Font.Size = 11;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 7;
                    #endregion

                    #region Page Setup
                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
                    //sheet1.PageSetup.PrintTitleRows = "$1:$2";
                    sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.UserId.Trim() + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    sheet1.PageSetup.LeftMargin = 0.5;
                    sheet1.PageSetup.RightMargin = 0.2;
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                    sheet1.Name = "Earn Leave";
                    #endregion             

                    workbook.Version = ExcelVersion.Excel97to2003;
                    report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);

                    // return workbook;

                    var filePath = "";
                    var SheetName = "Earn Leave";
                    //return workbook;
                    workbook.Version = ExcelVersion.Excel97to2003;
                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xls");
                    workbook.SaveAs(filePath);
                    workbook.Close();
                    excelEngine.Dispose();
                    return filePath;
                }
                else
                {
                    throw new Exception("No Data found...");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }
        public void getNewEmployee(string FromDate, string ToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT *,(ISNULL(t.TotalEarnLeave, 0) - ISNULL(t.Encashed, 0) - ISNULL(t.AvailedLeave, 0)) Balance --TotalEarnleaveValue=(TotalEarnLeave*Rate)
                                FROM (SELECT ei.EmployeeCode,ei.EmployeeName,ei.GenderID,FORMAT(ei.DOJ, 'dd-MMM-yyyy') DOJ,En.UserName EmployeeCategory
                                ,dp.UserName Department,SE.UserName Section,ISNULL(Li.UserName, '') Line,SuS.Username SubSection,Deg.UserName Designation
                                ,FORMAT(els.FromDate, 'dd-MMM-yyyy') FromDate,FORMAT(els.ToDate, 'dd-MMM-yyyy') ToDate,L.Rate,els.CalculatedEarningDays
                                ,els.CurrentYearAllocation CurrentPeriodAllocation,(els.CurrentYearAllocation + els.BroughtForward + els.CarryForwardOpeningBalance) TotalEarnLeave
                                ,L.Days Encashed,(SELECT SUM(apdx.LvValue)
                                FROM AttdnProcessData AS apdx
                                WHERE apdx.WorkDate BETWEEN els.FromDate AND els.ToDate AND apdx.EmpSystemID = L.EmpSystemId AND apdx.LTSystemID = els.LeaveTypeId) AvailedLeave--,Encashed-
                                ,( SELECT sum(apdx.PresentValue) FROM AttdnProcessData AS apdx
                                WHERE apdx.WorkDate BETWEEN els.FromDate AND els.ToDate AND apdx.EmpSystemID = L.EmpSystemId) AS TotalPresent
                                	FROM LeaveEncashmentTransaction L
                                	JOIN TRN.EmployeeLeaveSummary AS els ON els.EmployeeId = L.EmpSystemId AND L.EncashmentDate = els.ToDate
                                	JOIN EmployeeInformation AS ei ON ei.SystemId = L.EmpSystemId AND ei.SystemId = els.EmployeeId
                                	LEFT JOIN MST.ManpowerBudget PMB ON ei.BudgetCode = PMB.Id
                                	LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                                	LEFT JOIN ORG.Entity En ON PMB.EntityId = En.Id
                                	LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                                	LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = L.LegalDesignationId
                                	LEFT JOIN [MST].[DesignationMasterLegalDesignation] dmld ON dmld.LegalDesignationId = LGD.Id
                                	LEFT JOIN [MST].[DesignationMaster] dm ON dm.Id = dmld.DesignationMasterId
                                	LEFT JOIN HKP.Designation DeG ON DeG.Id = dm.DesignationId
                                	LEFT JOIN HKP.EmployeeCategory EC ON EC.Id = dm.EmployeeCategoryId
                                	LEFT JOIN ORG.Section SE ON SE.Id = PR.SectionId
                                	LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                                	LEFT JOIN ORG.Line AS Li ON Li.Id = PMB.LineId
                                	WHERE L.EncashmentDate BETWEEN ('" + FromDate + "') AND ('" + ToDate + @"') ) AS T ORDER BY T.EmployeeCode";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                objCon.getDataSet(strSql, out dsRef);
                objCon.CommitTransaction();
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
