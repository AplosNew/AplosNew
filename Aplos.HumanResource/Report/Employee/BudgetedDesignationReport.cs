using Library.Data.Sql;
using Library.Service.Helpers;
//using Library.Service.Payrolls.OT;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Report.Employee
{
    public class BudgetedDesignationReport
    {

        SqlRepository _sqlRepository = null;

        public BudgetedDesignationReport()
        {
            _sqlRepository = new SqlRepository();
        }

        public IWorkbook GetBudgetedDesignation(string PlantId, string CompanyId, string userName)
        {

            #region declare
            clsReport objRpt = null;
            clsReport objRptSR = null;
            ReportUtility oru = new ReportUtility();
            DataTable dtMPBudgetDesig = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();

            #endregion
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;

                IWorkbook workbook = application.Workbooks.Create(1);

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompany = _sqlRepository.GetDataTable("SELECT * FROM ORG.Company WHERE  Id = '" + CompanyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompany.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
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

                #endregion

                dtMPBudgetDesig = GetManpowerBudgetDataTable(CompanyId, PlantId, DateTime.Now.ToString("dd-MMM-yyyy"));


                var dtCmp = objRptSR.SelectedCompanyDT(CompanyId);

                //var dtFactory = objRptSR.SelectedPlantDT(PlantId);
                #region Variable
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                int colBudgetCode = 0;
                int colActivity = 0;
                int colBudgtedMP = 0;
                int colTotalMP = 0;


                int colDiff = 0;
                int colRemarks = 0;
                int colSl = 0;
                int colLegalDesignation = 0;


                int SLNo = 1;
                #endregion

                if (dtMPBudgetDesig.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }
                #region Hourly Ot
                //workbook = application.Workbooks.Create(4);
                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];

                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;

                #region ------------------Column Header------------------
                colSl = xlsCol;
                sheet1.Range[xlsRow, colSl].Text = "SL";
                sheet1.Range[xlsRow, colSl].ColumnWidth = 7;

                xlsCol += 1;
                int colPlant = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Plant";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;

                xlsCol += 1;
                int colDiv = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Division";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;

                xlsCol += 1;
                int colEntity = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Entity";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;

                xlsCol += 1;
                int colUnit = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Unit";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                

                xlsCol += 1;
                int colDepartment = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Department";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                
                xlsCol += 1;
                int colSection = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Section";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                
                xlsCol += 1;
                int colSubSection = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "SubSection";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;

                xlsCol += 1;
                colActivity = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Activity";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;

                xlsCol += 1;
                int colEmpType = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Employee Type";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;

                xlsCol += 1;
                int colDesignation = xlsCol;
                sheet1.Range[xlsRow, colDesignation].Text = "Designation";
                sheet1.Range[xlsRow, colDesignation].ColumnWidth = 25;

                //xlsCol += 1;
                //int colFactor = xlsCol;
                //sheet1.Range[xlsRow, xlsCol].Text = "Factor";
                //sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;

                xlsCol += 1;
                int colShift = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Shift";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;

                xlsCol += 1;
                colBudgetCode = xlsCol;
                sheet1.Range[xlsRow, colBudgetCode].Text = "Budget Code";
                sheet1.Range[xlsRow, colBudgetCode].ColumnWidth = 10;


                xlsCol += 1;
                colTotalMP = xlsCol;
                sheet1.Range[xlsRow, colTotalMP].Text = "On Role";
                sheet1.Range[xlsRow, colTotalMP].ColumnWidth = 25;

                xlsCol += 1;
                colBudgtedMP = xlsCol;
                sheet1.Range[xlsRow, colBudgtedMP].Text = "Bugeted MP";
                sheet1.Range[xlsRow, colBudgtedMP].ColumnWidth = 14;

                xlsCol += 1;
                int colDepMP = xlsCol;
                sheet1.Range[xlsRow, colDepMP].Text = "Deployed MP";
                sheet1.Range[xlsRow, colDepMP].ColumnWidth = 14;

                xlsCol += 1;
                colDiff = xlsCol;
                sheet1.Range[xlsRow, colDiff].Text = "Diff (Budget - On Role)";
                sheet1.Range[xlsRow, colDiff].ColumnWidth = 14;

                xlsCol += 1;
                int colRequirement = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Requirement";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;
                xlsCol += 1;
                int colDiffReq = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Diff (Req - On Role)";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;


                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, colSl, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;


                xlsRow++;

                #endregion ------------------Column Header------------------
                #region ----------------------Data-----------------------  

                int startXlsRow = xlsRow;
                for (int i = 0; i < dtMPBudgetDesig.Rows.Count; i++)
                {
                    try
                    {
                        sheet1.Range[xlsRow, colSl].Text = SLNo.ToString();
                        sheet1.Range[xlsRow, colPlant].Text = dtMPBudgetDesig.Rows[i]["PlantName"].ToString();
                        sheet1.Range[xlsRow, colDiv].Text = dtMPBudgetDesig.Rows[i]["Division"].ToString();
                        sheet1.Range[xlsRow, colEntity].Text = dtMPBudgetDesig.Rows[i]["EntityName"].ToString();
                        sheet1.Range[xlsRow, colUnit].Text = dtMPBudgetDesig.Rows[i]["Unit"].ToString();
                        sheet1.Range[xlsRow, colDepartment].Text = dtMPBudgetDesig.Rows[i]["DepartmentName"].ToString();
                        sheet1.Range[xlsRow, colSection].Text = dtMPBudgetDesig.Rows[i]["SectionName"].ToString();
                        sheet1.Range[xlsRow, colSubSection].Text = dtMPBudgetDesig.Rows[i]["SubSectionName"].ToString();
                        sheet1.Range[xlsRow, colActivity].Text = dtMPBudgetDesig.Rows[i]["Activity"].ToString();
                        sheet1.Range[xlsRow, colEmpType].Text = dtMPBudgetDesig.Rows[i]["EmployeeType"].ToString();
                        sheet1.Range[xlsRow, colDesignation].Text = dtMPBudgetDesig.Rows[i]["GivenDesignation"].ToString();
                        //sheet1.Range[xlsRow, colFactor].Text = dtMPBudgetDesig.Rows[i]["Factor"].ToString();
                        sheet1.Range[xlsRow, colShift].Text = dtMPBudgetDesig.Rows[i]["Shifts"].ToString();

                        sheet1.Range[xlsRow, colBudgetCode].Text = dtMPBudgetDesig.Rows[i]["BudgetCode"].ToString();
                       
                        sheet1.Range[xlsRow, colRequirement].Text = dtMPBudgetDesig.Rows[i]["Requirement"].ToString();

                        //sheet1.Range[xlsRow, colActivity].Text = dtMPBudgetDesig.Rows[i]["Activity"].ToString();
                        //sheet1.Range[xlsRow, colRemarks].Text = dtMPBudgetDesig.Rows[i]["Remarks"].ToString();
                        sheet1.Range[xlsRow, colBudgtedMP].Number = clsStaticInfo.dbl(dtMPBudgetDesig.Rows[i]["BudgetNo"].ToString());
                        sheet1.Range[xlsRow, colDepMP].Number = clsStaticInfo.dbl(dtMPBudgetDesig.Rows[i]["Deployed"].ToString());
                        sheet1.Range[xlsRow, colTotalMP].Number = clsStaticInfo.dbl(dtMPBudgetDesig.Rows[i]["totalEmp"].ToString());
                        sheet1.Range[xlsRow, colDiff].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colBudgtedMP) + xlsRow + "-" + clsStaticInfo.GetxlsCol(colTotalMP) + (xlsRow) + ")";
                        sheet1.Range[xlsRow, colDiffReq].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colRequirement) + xlsRow + "-" + clsStaticInfo.GetxlsCol(colTotalMP) + (xlsRow) + ")";

                        xlsRow++;
                        SLNo++;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                sheet1.Range[xlsRow, colBudgetCode].Text = "Total";

                sheet1.Range[xlsRow, colTotalMP].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colTotalMP) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(colTotalMP) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colDiff].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colDiff) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(colDiff) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colRequirement].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colRequirement) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(colRequirement) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colDiffReq].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colDiffReq) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(colDiffReq) + (xlsRow - 1) + ")";

                sheet1.Range[xlsRow, colBudgetCode, xlsRow, colDiffReq].CellStyle.Font.Bold = true;


                sheet1.Range[xlsRow, colBudgtedMP].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colBudgtedMP) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(colBudgtedMP) + (xlsRow - 1) + ")";
                sheet1.Range[6, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow, endXlsCol].WrapText = true;
                int sheetEndXlsRow = xlsRow;
                #endregion ----------------------Data-----------------------

                #region ******************Report Header******************



                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(3);
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
                if (dtCmp.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtCmp.Rows[0]["CompanyName"].ToString();
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
                if (dtCmp.Rows.Count > 0)
                {
                    FactoryAddress = dtCmp.Rows[0]["Address1"].ToString();
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
                sheet1.Range[xlsRow, 3].Text = "Budgeted Designation";
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
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "BudgetedDesignation";
                #endregion Page Setup

                #endregion  Individual Daily OT

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }





        public IWorkbook GetBudgetedDesignationDetail(string PlantId, string CompanyId, string userName, string workDate)
        {

            #region declare
            clsReport objRpt = null;
            clsReport objRptSR = null;
            ReportUtility oru = new ReportUtility();
            DataTable dtMPBudgetDesig = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();

            Dictionary<string, DataRow> dicWorkingHours = null; // GetWorkingHourInfo(PlantId,workDate);
            Dictionary<string, DataRow> dicDayStatusInfo = null; //GetDayStatusInfo(PlantId, workDate);
            Dictionary<string, DataRow> dicGross = null;

            #endregion
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;

                IWorkbook workbook = application.Workbooks.Create(1);

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompany = _sqlRepository.GetDataTable("SELECT * FROM ORG.Company WHERE  Id = '" + CompanyId + @"'");

                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompany.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
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

                #endregion

                dtMPBudgetDesig = GetManpowerBudgetDataTable(CompanyId, PlantId, workDate);
                GetEmployeesGrossSalary(out dicGross, CompanyId, PlantId, workDate);
                dicWorkingHours = GetWorkingHourInfo(CompanyId, PlantId, workDate);
                dicDayStatusInfo = GetDayStatusInfo(PlantId, workDate,CompanyId);

                var dtCmp = objRptSR.SelectedCompanyDT(CompanyId);

                //var dtFactory = objRptSR.SelectedPlantDT(PlantId);
                #region Variable
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";



                int SLNo = 1;
                #endregion

                if (dtMPBudgetDesig.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }
                #region Hourly Ot
                //workbook = application.Workbooks.Create(4);
                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];

                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;

                #region ------------------Column Header------------------
                int colSl = xlsCol;
                sheet1.Range[xlsRow, colSl].Text = "SL";
                sheet1.Range[xlsRow, colSl].ColumnWidth = 7;

                xlsCol += 1;
                int colBudgetCode = xlsCol;
                sheet1.Range[xlsRow, colBudgetCode].Text = "Budget Code";
                sheet1.Range[xlsRow, colBudgetCode].ColumnWidth = 10;



                xlsCol += 1;
                int colPlant = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Plant";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol += 1;
                int colEntity = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Entity";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol += 1;
                int colDepartment = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Department";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol += 1;
                int colSection = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Section";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                xlsCol += 1;
                int colSubSection = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "SubSection";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;

                xlsCol += 1;
                int colLegalDesignation = xlsCol;
                sheet1.Range[xlsRow, colLegalDesignation].Text = "Legal Designation";
                sheet1.Range[xlsRow, colLegalDesignation].ColumnWidth = 25;
                //xlsCol += 1;
                //int colActivity = xlsCol;
                //sheet1.Range[xlsRow, colActivity].Text = "Activity";
                //sheet1.Range[xlsRow, colActivity].ColumnWidth = 20;


                xlsCol += 1;
                int colTotalMP = xlsCol;
                sheet1.Range[xlsRow, colTotalMP].Text = "On Role";
                sheet1.Range[xlsRow, colTotalMP].ColumnWidth = 25;

                xlsCol += 1;
                int colBudgtedMP = xlsCol;
                sheet1.Range[xlsRow, colBudgtedMP].Text = "Bugeted MP";
                sheet1.Range[xlsRow, colBudgtedMP].ColumnWidth = 14;

                xlsCol += 1;
                int colDiff = xlsCol;
                sheet1.Range[xlsRow, colDiff].Text = "Diff (Budget - On Role)";
                sheet1.Range[xlsRow, colDiff].ColumnWidth = 14;

                xlsCol += 1;
                int colRequirement = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Requirement";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;
                xlsCol += 1;
                int colActManDays = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Actual Man Days";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;

                xlsCol += 1;
                int colManDaysExSH = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Man Days Excess/Short";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;

                xlsCol += 1;
                int colAverageExcessSalary = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = " Average Excess Salary Paid(Rs.)";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;


                xlsCol += 1;
                int colDiffReq = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Diff (Req - On Role)";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;

                xlsCol += 1;
                int colPresent = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Present";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;

                xlsCol += 1;
                int colAbsent = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Absent";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;

                xlsCol += 1;
                int colLate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Late";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;

                xlsCol += 1;
                int colLeave = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Leave";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;
                xlsCol += 1;
                int colAbsentPercent = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Abs %";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;

                xlsCol += 1;
                int colLeavePercent = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Leave %";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;

                xlsCol += 1;
                int colRemarks = xlsCol;
                sheet1.Range[xlsRow, colRemarks].Text = "Remarks";
                sheet1.Range[xlsRow, colRemarks].ColumnWidth = 25;

                endXlsCol = xlsCol;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, colSl, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;


                xlsRow++;

                #endregion ------------------Column Header------------------
                #region ----------------------Data-----------------------  
                string dicKey = "";
                DataRow drMPBudgetDesig = null;
                int startXlsRow = xlsRow;
                for (int i = 0; i < dtMPBudgetDesig.Rows.Count; i++)
                {

                    try
                    {
                        drMPBudgetDesig = null;
                        dicKey = dtMPBudgetDesig.Rows[i]["BudgetCodeId"].ToString() + "-" + dtMPBudgetDesig.Rows[i]["LegalDesignationId"].ToString() + dtMPBudgetDesig.Rows[i]["PlantId"].ToString() + dtMPBudgetDesig.Rows[i]["EntityId"].ToString() + dtMPBudgetDesig.Rows[i]["DepartmentId"].ToString() + dtMPBudgetDesig.Rows[i]["Sectionid"].ToString() + dtMPBudgetDesig.Rows[i]["SubSectionId"].ToString();

                        //dicKey = dtMPBudgetDesig.Rows[i]["BudgetCodeId"].ToString() + "-" + dtMPBudgetDesig.Rows[i]["LegalDesignationId"].ToString();
                        sheet1.Range[xlsRow, colSl].Text = SLNo.ToString();
                        sheet1.Range[xlsRow, colBudgetCode].Text = dtMPBudgetDesig.Rows[i]["BudgetCode"].ToString();
                        sheet1.Range[xlsRow, colLegalDesignation].Text = dtMPBudgetDesig.Rows[i]["LegalDesignation"].ToString();
                        sheet1.Range[xlsRow, colPlant].Text = dtMPBudgetDesig.Rows[i]["PlantName"].ToString();
                        sheet1.Range[xlsRow, colEntity].Text = dtMPBudgetDesig.Rows[i]["EntityName"].ToString();
                        sheet1.Range[xlsRow, colDepartment].Text = dtMPBudgetDesig.Rows[i]["DepartmentName"].ToString();
                        sheet1.Range[xlsRow, colSection].Text = dtMPBudgetDesig.Rows[i]["SectionName"].ToString();
                        sheet1.Range[xlsRow, colSubSection].Text = dtMPBudgetDesig.Rows[i]["SubSectionName"].ToString();
                        sheet1.Range[xlsRow, colRequirement].Number = clsStaticInfo.dbl(dtMPBudgetDesig.Rows[i]["Requirement"].ToString());

                        //sheet1.Range[xlsRow, colActivity].Text = dtMPBudgetDesig.Rows[i]["Activity"].ToString();
                        sheet1.Range[xlsRow, colRemarks].Text = "";
                        sheet1.Range[xlsRow, colBudgtedMP].Number = clsStaticInfo.dbl(dtMPBudgetDesig.Rows[i]["BudgetNo"].ToString());
                        sheet1.Range[xlsRow, colTotalMP].Number = clsStaticInfo.dbl(dtMPBudgetDesig.Rows[i]["totalEmp"].ToString());
                        sheet1.Range[xlsRow, colManDaysExSH].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colRequirement) + xlsRow + "-" + clsStaticInfo.GetxlsCol(colActManDays) + (xlsRow) + ")";
                        sheet1.Range[xlsRow, colManDaysExSH].NumberFormat = oru.NumberFormatDecimalTwo();

                        if (dicWorkingHours.ContainsKey(dicKey))
                        {
                            drMPBudgetDesig = dicWorkingHours[dicKey];

                            sheet1.Range[xlsRow, colActManDays].Number = (clsStaticInfo.dbl(drMPBudgetDesig["WorkingHour"].ToString().ToUpper()) / 60) / 8;
                            sheet1.Range[xlsRow, colActManDays].NumberFormat = oru.NumberFormatDecimalTwo();
                        }

                        if (dicDayStatusInfo.ContainsKey(dicKey))
                        {
                            drMPBudgetDesig = null;
                            drMPBudgetDesig = dicDayStatusInfo[dicKey];
                            if(clsStaticInfo.dbl(dtMPBudgetDesig.Rows[i]["totalEmp"].ToString())>0)
                            {

                                sheet1.Range[xlsRow, colPresent].Number = clsStaticInfo.dbl(drMPBudgetDesig["TOTALPRESENT"].ToString().ToUpper());
                                sheet1.Range[xlsRow, colLate].Number = clsStaticInfo.dbl(drMPBudgetDesig["TOTALLATE"].ToString().ToUpper());
                                sheet1.Range[xlsRow, colAbsent].Number = clsStaticInfo.dbl(drMPBudgetDesig["TOTALABSENT"].ToString().ToUpper());
                                sheet1.Range[xlsRow, colLeave].Number = clsStaticInfo.dbl(drMPBudgetDesig["TOTALLEAVE"].ToString().ToUpper());
                            }
                        }
                        if (dicGross.ContainsKey(dicKey))
                        {
                            drMPBudgetDesig = dicGross[dicKey];

                            sheet1.Range[xlsRow, colAverageExcessSalary].Formula = "SUM(" + (clsStaticInfo.dbl(drMPBudgetDesig["DefineAmount"].ToString().ToUpper()) / 26).ToString() + "*" + clsStaticInfo.GetxlsCol(colTotalMP) + (xlsRow) + ")";
                            sheet1.Range[xlsRow, colAverageExcessSalary].NumberFormat = oru.NumberFormatDecimalTwo();
                        }
                        if (clsStaticInfo.dbl(dtMPBudgetDesig.Rows[i]["Requirement"].ToString()) > 0)
                        {
                            sheet1.Range[xlsRow, colAbsentPercent].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colAbsent) + xlsRow + "/" + clsStaticInfo.GetxlsCol(colRequirement) + (xlsRow) + ")*100";
                            sheet1.Range[xlsRow, colAbsentPercent].NumberFormat = oru.NumberFormatDecimalTwo();

                            sheet1.Range[xlsRow, colLeavePercent].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colLeave) + xlsRow + "/" + clsStaticInfo.GetxlsCol(colRequirement) + (xlsRow) + ")*100";
                            sheet1.Range[xlsRow, colLeavePercent].NumberFormat = oru.NumberFormatDecimalTwo();

                        }

                        sheet1.Range[xlsRow, colDiff].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colBudgtedMP) + xlsRow + "-" + clsStaticInfo.GetxlsCol(colTotalMP) + (xlsRow) + ")";
                        sheet1.Range[xlsRow, colDiffReq].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colRequirement) + xlsRow + "-" + clsStaticInfo.GetxlsCol(colTotalMP) + (xlsRow) + ")";

                        xlsRow++;
                        SLNo++;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                //sheet1.Range[xlsRow, colActivity].Text = "Total";

                sheet1.Range[xlsRow, colTotalMP].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colTotalMP) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(colTotalMP) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colDiff].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colDiff) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(colDiff) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colRequirement].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colRequirement) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(colRequirement) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colDiffReq].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colDiffReq) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(colDiffReq) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colPresent].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colPresent) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(colPresent) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colAbsent].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colAbsent) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(colAbsent) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colLeave].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colLeave) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(colLeave) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colLate].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colLate) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(colLate) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colActManDays].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colActManDays) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(colActManDays) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colAverageExcessSalary].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colAverageExcessSalary) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(colAverageExcessSalary) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colAbsentPercent].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colAbsentPercent) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(colAbsentPercent) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colManDaysExSH].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colManDaysExSH) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(colManDaysExSH) + (xlsRow - 1) + ")";





                //sheet1.Range[xlsRow, colActivity, xlsRow, endXlsCol].CellStyle.Font.Bold = true;


                sheet1.Range[xlsRow, colBudgtedMP].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colBudgtedMP) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(colBudgtedMP) + (xlsRow - 1) + ")";
                sheet1.Range[6, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow, endXlsCol].WrapText = true;
                int sheetEndXlsRow = xlsRow;
                #endregion ----------------------Data-----------------------

                #region ******************Report Header******************



                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(3);
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
                if (dtCmp.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtCmp.Rows[0]["CompanyName"].ToString();
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
                if (dtCmp.Rows.Count > 0)
                {
                    FactoryAddress = dtCmp.Rows[0]["Address1"].ToString();
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
                sheet1.Range[xlsRow, 3].Text = "Budgeted Designation";
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
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "BudgetedDesignation";
                #endregion Page Setup

                #endregion  Individual Daily OT

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        private DataTable GetManpowerBudgetDataTable(string companyId, string plantIds, string Date)
        {


            //   string strSql = @"SELECT plant.UserName PlantName, plant.Id PlantId, dd.UserName as Division ,ENt.UserName EntityName,ENt.Id EntityId ,
            //                       uu.UserName as Unit, Dept.UserName DepartmentName,Dept.Id DepartmentId, Sec.UserName SectionName, Sec.id Sectionid,
            //                       SSec.Id SubSectionId,SSec.UserName SubSectionName, DB.Activity , ec.UserName as EmployeeType,B.LegalDesignationId,ld.UserName LegalDesignation ,
            //                       ld.Factor , sd.UserName as Shifts
            //                       ,MB.Code BudgetCode, isnull(Sum(Cast (Deployment as numeric)),0) as Deployed
            //,SUM(B.BudgetNo) AS BudgetNo,SUM(Actual) AS totalEmp,sum(B.Requirement) Requirement 
            //                           FROM  (
            //SELECT BudgetCodeId,LegalDesignationId,BudgetNo,0 AS Actual,ISNULL(Activity,'') Activity,ISNULL(Remarks,'') Remarks,ISNULL(Requirement,0) Requirement FROM DesignationBudget
            //UNION ALL
            //SELECT BudgetCode,LegalDesignationId,0 AS BudgetNo,COUNT(*),'' Activity,'' Remarks,0 Requirement FROM EmployeeInformation 
            //                           WHERE (DOJ<='" + Date + @"' AND (DOS IS NULL OR DOS >= '" + Date + @"'))
            //GROUP BY BudgetCode,LegalDesignationId
            //) B
            //  LEFT JOIN HKP.LegalDesignation LD ON LD.Id = B.LegalDesignationId 
            //                        LEFT JOIN MST.ManpowerBudget MB ON MB.Id = B.BudgetCodeId
            //                        LEFT JOIN DesignationBudget DB ON DB.BudgetCodeId = B.BudgetCodeId

            //                        LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
            //                        LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
            //                        lEFT JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
            //                        lEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId

            //                        LEFT OUTER JOIN [ORG].[Plant] AS plant ON plant.id = ENT.PlantId
            //                        LEFT OUTER JOIN [ORG].Department AS Dept ON Dept.Id = PO.DepartmentId
            //                        LEFT OUTER JOIN [ORG].Section AS Sec ON Sec.Id = PO.SectionId
            //                        LEFT OUTER JOIN [ORG].SubSection AS SSec ON SSec.Id = PO.SubSectionId 
            //                        left outer join org.Division dd on dd.Id = PO.DivisionId
            //left outer join org.Unit uu on uu.Id = ENT.UnitId
            //left join mst.DesignationMasterLegalDesignation ddm on ddm.LegalDesignationId = B.LegalDesignationId
            //left join mst.DesignationMaster dm on dm.id = ddm.DesignationMasterId
            //left join hkp.EmployeeCategory ec on ec.ID = dm.EmployeeCategoryId
            //left join ShiftDefination sd on sd.SystemID = mb.ShiftDefinationId";
            string strSql = @"SELECT plant.UserName PlantName, plant.Id PlantId, dd.UserName as Division ,ENt.UserName EntityName,ENt.Id EntityId ,
                                uu.UserName as Unit, Dept.UserName DepartmentName,Dept.Id DepartmentId, Sec.UserName SectionName, Sec.id Sectionid,
                                SSec.Id SubSectionId,SSec.UserName SubSectionName, DB.Activity , ec.UserName as EmployeeType,B.GivenDesignationId,ld.UserName GivenDesignation,LDG.UserName LegalDesignation  ,
                                 sd.UserName as Shifts
                                ,B.BudgetCodeId,MB.Code BudgetCode,B.LegalDesignationId, isnull(Sum(Cast (Deployment as numeric)),0) as Deployed
								 ,SUM(B.BudgetNo) AS BudgetNo,SUM(Actual) AS totalEmp,sum(B.Requirement) Requirement 
                                    FROM  (
								 --SELECT BudgetCodeId,LegalDesignationId,BudgetNo,0 AS Actual,ISNULL(Activity,'') Activity,ISNULL(Remarks,'') Remarks,ISNULL(Requirement,0) Requirement FROM DesignationBudget
								 --UNION ALL
								 --SELECT BudgetCode,LegalDesignationId,0 AS BudgetNo,COUNT(*),'' Activity,'' Remarks,0 Requirement FROM EmployeeInformation 
         --                            WHERE (DOJ<='" + Date + @"' AND (DOS IS NULL OR DOS >= '" + Date + @"'))
								 --GROUP BY BudgetCode,LegalDesignationId

								 SELECT BudgetCode as BudgetCodeId,GivenDesignationId,LegalDesignationId,0 AS BudgetNo,COUNT(*) as Actual,'' Activity,'' Remarks,0 Requirement FROM EmployeeInformation 
                                    WHERE (DOJ<='" + Date + @"' AND (DOS IS NULL OR DOS >= '" + Date + @"'))
								 GROUP BY BudgetCode,GivenDesignationId,LegalDesignationId
								 ) B
								   LEFT JOIN HKP.Designation LD ON LD.Id = B.GivenDesignationId
LEFT JOIN HKP.LegalDesignation LDG ON LDG.Id = B.LegalDesignationId
                                 LEFT JOIN MST.ManpowerBudget MB ON MB.Id = B.BudgetCodeId
                                 LEFT JOIN DesignationBudget DB ON DB.BudgetCodeId = B.BudgetCodeId

                                 LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                 LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                                 lEFT JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                 lEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId

                                 LEFT OUTER JOIN [ORG].[Plant] AS plant ON plant.id = ENT.PlantId
                                 LEFT OUTER JOIN [ORG].Department AS Dept ON Dept.Id = PO.DepartmentId
                                 LEFT OUTER JOIN [ORG].Section AS Sec ON Sec.Id = PO.SectionId
                                 LEFT OUTER JOIN [ORG].SubSection AS SSec ON SSec.Id = PO.SubSectionId 
                                 left outer join org.Division dd on dd.Id = PO.DivisionId
								 left outer join org.Unit uu on uu.Id = ENT.UnitId
								 --left join mst.DesignationMasterLegalDesignation ddm on ddm.LegalDesignationId = B.GivenDesignationId
								 left join mst.DesignationMaster dm on dm.DesignationId = B.GivenDesignationId
								 left join hkp.EmployeeCategory ec on ec.ID = dm.EmployeeCategoryId
								 left join ShiftDefination sd on sd.SystemID = mb.ShiftDefinationId";
            if (string.IsNullOrEmpty(plantIds))
            {

                strSql += @" WHERE C.Id = '" + companyId + @"'";
            }
            else
            {

                strSql += @" WHERE C.Id = '" + companyId + @"' and plant.Id IN (" + plantIds + @")";
            }
            strSql += @"  GROUP BY  B.BudgetCodeId,B.GivenDesignationId,B.LegalDesignationId,MB.Code,  DB.Activity, DB.Remarks
								 ,plant.UserName ,ENt.UserName ,ld.UserName,LDG.UserName
								 ,Dept.UserName , Sec.UserName , SSec.UserName 
								 ,plant.Id ,ENt.Id ,ld.Id
								 ,Dept.Id , Sec.Id , SSec.Id, dd.UserName , uu.UserName , ec.UserName  , sd.UserName  ";

            return _sqlRepository.GetDataTable(strSql);
        }

        private Dictionary<string, DataRow> GetDayStatusInfo(string PlantIds, string workDate,string companyId)
        {
            try
            {
                string strSql = "";
                Dictionary<string, DataRow> dicDaySattus = null;

                strSql = @"SELECT EEI.BudgetCode,EEI.LegalDesignationId
,plant.UserName PlantName,plant.Id PlantId,ENt.UserName EntityName,ENt.Id EntityId
								 ,Dept.UserName DepartmentName,Dept.Id DepartmentId, Sec.UserName SectionName, Sec.id Sectionid, SSec.Id SubSectionId,SSec.UserName SubSectionName
                        ,SUM(CASE WHEN DT.CATEGORY = 'PRESENT' THEN 1 ELSE 0 END) AS TOTALPRESENT 
                        ,SUM(CASE WHEN DT.CATEGORY = 'LATE' THEN 1 ELSE 0 END) AS TOTALLATE 
                        ,SUM(CASE WHEN DT.CATEGORY = 'LEAVE' THEN 1 ELSE 0 END) AS TOTALLEAVE
                        ,SUM(CASE WHEN DT.CATEGORY = 'ABSENT' THEN 1 ELSE 0 END) AS TOTALABSENT
                        
                        FROM AttdnProcessData APD
                        LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = APD. EmpSystemID
                        LEFT JOIN DayType DT ON DT.DayType = APD.DayStatus
						LEFT JOIN MST.ManpowerBudget MB ON MB.Id = EEI.BudgetCode
                    LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                    LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                    lEFT JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                    lEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId

                    LEFT OUTER JOIN [ORG].[Plant] AS plant ON plant.id = ENT.PlantId
                    LEFT OUTER JOIN [ORG].Department AS Dept ON Dept.Id = PO.DepartmentId
                    LEFT OUTER JOIN [ORG].Section AS Sec ON Sec.Id = PO.SectionId
                    LEFT OUTER JOIN [ORG].SubSection AS SSec ON SSec.Id = PO.SubSectionId
                        WHERE WorkDate = '" + workDate + @"'and C.Id = '" + companyId + @"' and apd.PlantID IN (" + PlantIds + @")

                           Group by EEI.BudgetCode,EEI.LegalDesignationId,APD.WorkDate
					 ,plant.UserName ,ENt.UserName --,ld.UserName
								 ,Dept.UserName , Sec.UserName , SSec.UserName 
								 ,plant.Id ,ENt.Id ,EEI.LegalDesignationId
								 ,Dept.Id , Sec.Id , SSec.Id 
                        order by EEI.BudgetCode,EEI.LegalDesignationId";

                DataTable dtTable = _sqlRepository.GetDataTable(strSql);
                dicDaySattus = new Dictionary<string, DataRow>();
                for (int i = 0; i < dtTable.Rows.Count; i++)
                {
                    string budgetLegalDs = dtTable.Rows[i]["BudgetCode"].ToString() + "-" + dtTable.Rows[i]["LegalDesignationId"].ToString() + dtTable.Rows[i]["PlantId"].ToString() + dtTable.Rows[i]["EntityId"].ToString() + dtTable.Rows[i]["DepartmentId"].ToString() + dtTable.Rows[i]["Sectionid"].ToString() + dtTable.Rows[i]["SubSectionId"].ToString();

                    //string budgetLegalDs = dtTable.Rows[i]["BudgetCode"].ToString() + "-" + dtTable.Rows[i]["LegalDesignationId"].ToString();
                    dicDaySattus.Add(budgetLegalDs, dtTable.Rows[i]);
                }

                return dicDaySattus;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        private Dictionary<string, DataRow> GetWorkingHourInfo(string companyId, string PlantIds, string workDate)
        {
            try
            {
                string strSql = "";
                Dictionary<string, DataRow> dicDaySattus = null;

                strSql = @"SELECT EEI.BudgetCode,EEI.LegalDesignationId, FORMAT(APD.WorkDate,'dd-MMM-yyyy') WorkDate
,plant.UserName PlantName,plant.Id PlantId,ENt.UserName EntityName,ENt.Id EntityId
								 ,Dept.UserName DepartmentName,Dept.Id DepartmentId, Sec.UserName SectionName, Sec.id Sectionid, SSec.Id SubSectionId,SSec.UserName SubSectionName,
                    COUNT(EEI.systemId) totalPresent,
                    SUM(CASE WHEN SHD.IncludeBreakTimeInOT = 1 THEN SHD.WorkingHour When SHD.IncludeBreakTimeInOT = 0 
                    THEN SHD.WorkingHour-SHD.BreakPeriod END) WorkingHour
                    from AttdnProcessData APD
                    Left join EmployeeInformation EEI ON EEI.SystemId = APD. EmpSystemID
                    left join ShiftDefination SHD ON SHD.SystemID = APD.ShiftSystemID
                    left join DayType DT ON DT.DayType = APD.DayStatus
                    LEFT JOIN MST.ManpowerBudget MB ON MB.Id = EEI.BudgetCode
                    LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                    LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                    lEFT JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                    lEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId

                    LEFT OUTER JOIN [ORG].[Plant] AS plant ON plant.id = ENT.PlantId
                    LEFT OUTER JOIN [ORG].Department AS Dept ON Dept.Id = PO.DepartmentId
                    LEFT OUTER JOIN [ORG].Section AS Sec ON Sec.Id = PO.SectionId
                    LEFT OUTER JOIN [ORG].SubSection AS SSec ON SSec.Id = PO.SubSectionId
                    where DT.Category = 'Present' AND WorkDate = '" + workDate + @"' and  C.Id = '" + companyId + @"' and apd.PlantID IN (" + PlantIds + @")
                     Group by EEI.BudgetCode,EEI.LegalDesignationId,APD.WorkDate
					 ,plant.UserName ,ENt.UserName --,ld.UserName
								 ,Dept.UserName , Sec.UserName , SSec.UserName 
								 ,plant.Id ,ENt.Id ,EEI.LegalDesignationId
								 ,Dept.Id , Sec.Id , SSec.Id 
                    order by EEI.BudgetCode,EEI.LegalDesignationId";

                DataTable dtTable = _sqlRepository.GetDataTable(strSql);
                dicDaySattus = new Dictionary<string, DataRow>();
                for (int i = 0; i < dtTable.Rows.Count; i++)
                {
                    string budgetLegalDs = dtTable.Rows[i]["BudgetCode"].ToString() + "-" + dtTable.Rows[i]["LegalDesignationId"].ToString() + dtTable.Rows[i]["PlantId"].ToString() + dtTable.Rows[i]["EntityId"].ToString() + dtTable.Rows[i]["DepartmentId"].ToString() + dtTable.Rows[i]["Sectionid"].ToString() + dtTable.Rows[i]["SubSectionId"].ToString();
                    dicDaySattus.Add(budgetLegalDs, dtTable.Rows[i]);
                }

                return dicDaySattus;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void GetEmployeesGrossSalary(out Dictionary<string, DataRow> Data, string companyId, string PlantId, string Date)
        {
            DataSet dsRef = new DataSet();
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT LD.Id LegalDesignationId, MB.Code BudgetCode,MB.Id BudgetCodeId
                    ,plant.UserName PlantName,plant.Id PlantId,ENt.UserName EntityName,ENt.Id EntityId
								 ,Dept.UserName DepartmentName,Dept.Id DepartmentId, Sec.UserName SectionName, Sec.id Sectionid, SSec.Id SubSectionId,SSec.UserName SubSectionName
, SUM(CONVERT(NUMERIC(10,2), DefineAmount)) DefineAmount
	                            FROM (
		                            -----------new	---
		                            SELECT m.EffectiveDate, m.EmpInfoSystemID, m.systemid
		                            FROM (
			                            SELECT EffectiveDate, EmpInfoSystemID, systemid
			                            FROM SalaryInfoDefineMaster --where IsApproved=1
			                            UNION
			                            SELECT EffectiveDate, EmpInfoSystemID, systemid
			                            FROM SalaryInfobackMaster --where IsApproved=1
			                            ) m
		                            INNER JOIN (
			                            SELECT MAX(EffectiveDate) EffectiveDate, EmpInfoSystemID
			                            FROM (
				                            SELECT MAX(EffectiveDate) EffectiveDate, EmpInfoSystemID
				                            FROM SalaryInfoDefineMaster
				                            WHERE IsApproved = 1
				                            GROUP BY EmpInfoSystemID
				                            UNION
				                            SELECT MAX(EffectiveDate) EffectiveDate, EmpInfoSystemID
				                            FROM SalaryInfobackMaster
				                            WHERE IsApproved = 1
				                            GROUP BY EmpInfoSystemID
				                            ) x
			                            GROUP BY EmpInfoSystemID
			                            ) dd ON m.EffectiveDate = dd.EffectiveDate AND m.EmpInfoSystemID = dd.EmpInfoSystemID
			                            -----------new	---
		                            ) sidmA
	                            INNER JOIN (
		                            SELECT SystemID, SalaryID, SalaryHeadID, DefineAmount
		                            FROM SalaryInfoDefine
		                            UNION
		                            SELECT SystemID, SalaryID, SalaryHeadID, DefineAmount
		                            FROM SalaryInfoBack
		                            ) AS sidBasicA ON sidBasicA.SalaryID = sidmA.SystemID
	                            INNER JOIN dbo.SalaryHead SH ON SH.SalaryHeadID = sidBasicA.SalaryHeadID AND Sh.HeadCategory = 'GROSS'
								LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = sidmA.EmpInfoSystemID 
								   LEFT JOIN HKP.LegalDesignation LD ON LD.Id = EEI.LegalDesignationId 
                                 LEFT JOIN MST.ManpowerBudget MB ON MB.Id = EEI.BudgetCode
 LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                    LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                    lEFT JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                    lEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId

                    LEFT OUTER JOIN [ORG].[Plant] AS plant ON plant.id = ENT.PlantId
                    LEFT OUTER JOIN [ORG].Department AS Dept ON Dept.Id = PO.DepartmentId
                    LEFT OUTER JOIN [ORG].Section AS Sec ON Sec.Id = PO.SectionId
                    LEFT OUTER JOIN [ORG].SubSection AS SSec ON SSec.Id = PO.SubSectionId
								where C.Id = '" + companyId + @"' and EEI.PlantId IN (" + PlantId + @") 

									GROUP BY  LD.Id , MB.Code , MB.Id,LegalDesignationId								
								 ,plant.UserName ,ENt.UserName ,ld.UserName
								 ,Dept.UserName , Sec.UserName , SSec.UserName 
								 ,plant.Id ,ENt.Id ,ld.Id
								 ,Dept.Id , Sec.Id , SSec.Id 
                                    ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
                Data = new Dictionary<string, DataRow>();
                DataTable dtTable = dsRef.Tables[0];

                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    try
                    {
                        string budgetLegalDs = dtTable.Rows[i]["BudgetCodeId"].ToString() + "-" + dtTable.Rows[i]["LegalDesignationId"].ToString() + dtTable.Rows[i]["PlantId"].ToString() + dtTable.Rows[i]["EntityId"].ToString() + dtTable.Rows[i]["DepartmentId"].ToString() + dtTable.Rows[i]["Sectionid"].ToString() + dtTable.Rows[i]["SubSectionId"].ToString();

                        //string dicKey = dsRef.Tables[0].Rows[i]["BudgetCodeId"].ToString() + "-" + dsRef.Tables[0].Rows[i]["LegalDesignationId"].ToString();
                        Data.Add(budgetLegalDs, dsRef.Tables[0].Rows[i]);
                    }
                    catch (Exception)
                    {


                    }

                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }
    }
}
