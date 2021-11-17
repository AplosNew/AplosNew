#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.Data.Sql;
using System;
using OTSBD;
using System.Data;
using Library.Crosscutting.Security;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;
using Syncfusion.XlsIO;
using System.IO;
using Library.Service.Helpers;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class MachineLayoutReportController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public MachineLayoutReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpPost, Authorize]
        public ActionResult Report(string EntityId, string ProcessId, string ProductionDate, string WorkCenterMasterId,Dictionary<string,object> Data)
        {
            #region Variable
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsReport objRpt = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            string FactoryName = "";
            string CmpName = "";
            string companyId = identity.CompanyId;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            DateTime dtFrmDt = DateTime.Now;
            DateTime dtEndDate = DateTime.Now;
            string MonthName = string.Empty;
            DataTable dtEmp = null;
            #endregion Variable

            try
            {
                #region DataSet
               
                SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                SelectedPlant(identity.PlantId, out dsFactory);
                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                xlsRow = 6;
                #region Variables
                int strCount = 0;
                int iSrNo = 0;
                int iEmpCode = 0;
                int iEmpName = 0;
                int iDOJ = 0;
                int iUnit = 0;
                int iDepart = 0;
                int iSec = 0;
                int iDesig = 0;
                int iEmployeeCategory = 0;
                int iTotal = 0;
                int iLine = 0;
                int iValid = 0;
                int iLO = 0;
                int iNOO = 0;
                int iMO = 0;
                string BuyerName = "";
                #endregion Variables

                #region ------------------Column Header------------------
                //BuyerName = Data["BuyerName"].ToString().Trim();
                xlsCol = 1;
                xlsRow = 5;
                sheet1.Range[xlsRow, xlsCol].Text = "Buyer";
                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + Data["Buyer"].ToString().Trim();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                xlsCol = 1;
                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Text = "Style";
                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + Data["BuyerItemNo"].ToString().Trim();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                xlsCol = 1;
                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Item";
                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + Data["Article"].ToString().Trim();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                xlsCol = 1;
                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Colour";
                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " /*+ Data["Colour"].ToString().Trim()*/;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                xlsCol = 1;
                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                xlsRow += 1;
                xlsCol = 6;
                xlsRow = 5;

                sheet1.Range[xlsRow, xlsCol].Text = "Date";
                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + ProductionDate.ToString().Trim();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Total SPT";
                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + Data["SMV"].ToString().Trim();
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Operators";
                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " /*+ Data["Operators"].ToString().Trim()*/;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Helpers";
                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " /*+ Data["Helpers"].ToString().Trim()*/;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                xlsRow += 1;

                xlsCol = 1;
                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                xlsRow += 1;
                xlsCol = 5;
                xlsRow = 5;

                sheet1.Range[xlsRow, xlsCol].Text = "M/C-SPT";
                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + Data["SMV"].ToString().Trim();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "TGT";
                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " /*+ Data["SPT"].ToString().Trim()*/;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                
                #region ------------------Details Header-----------------

                xlsRow = 12;
                xlsCol = 1;
                iSrNo = xlsCol;
                sheet1.Range[xlsRow, iSrNo].Text = "Sl No.";
                sheet1.Range[xlsRow, iSrNo].ColumnWidth = 6;
                sheet1.Range[xlsRow, iSrNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iSrNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                xlsCol += 1;
                iEmpCode = xlsCol;
                sheet1.Range[xlsRow, iEmpCode].Text = "Employee Code";
                sheet1.Range[xlsRow, iEmpCode].ColumnWidth = 10;
                sheet1.Range[xlsRow, iEmpCode].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iEmpCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                xlsCol += 1;
                iEmpName = xlsCol;
                sheet1.Range[xlsRow, iEmpName].Text = "Employee Name";
                sheet1.Range[xlsRow, iEmpName].ColumnWidth = 22;
                sheet1.Range[xlsRow, iEmpName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iEmpName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                xlsCol += 1;
                iDOJ = xlsCol;
                sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                sheet1.Range[xlsRow, iDOJ].ColumnWidth = 9.20;
                sheet1.Range[xlsRow, iDOJ].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;
                xlsCol += 1;
                iUnit = xlsCol;
                sheet1.Range[xlsRow, iUnit].Text = "Unit";
                sheet1.Range[xlsRow, iUnit].ColumnWidth = 9;
                sheet1.Range[xlsRow, iUnit].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                xlsCol += 1;
                iDepart = xlsCol;
                sheet1.Range[xlsRow, iDepart].Text = "Department";
                sheet1.Range[xlsRow, iDepart].ColumnWidth = 15;
                sheet1.Range[xlsRow, iDepart].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iDepart].VerticalAlignment = ExcelVAlign.VAlignCenter;
                xlsCol += 1;
                iSec = xlsCol;
                sheet1.Range[xlsRow, iSec].Text = "Section";
                sheet1.Range[xlsRow, iSec].ColumnWidth = 15;
                sheet1.Range[xlsRow, iSec].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iSec].VerticalAlignment = ExcelVAlign.VAlignCenter;

                xlsCol += 1;
                iDesig = xlsCol;
                sheet1.Range[xlsRow, iDesig].Text = "Designation";
                sheet1.Range[xlsRow, iDesig].ColumnWidth = 15;
                sheet1.Range[xlsRow, iDesig].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iDesig].VerticalAlignment = ExcelVAlign.VAlignCenter;

                xlsCol += 1;
                iEmployeeCategory = xlsCol;
                sheet1.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                sheet1.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;
                sheet1.Range[xlsRow, iEmployeeCategory].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iEmployeeCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;

                
                xlsCol += 1;
                iTotal = xlsCol;
                sheet1.Range[xlsRow, iTotal].Text = "NO Punch";
                sheet1.Range[xlsRow, iTotal].ColumnWidth = 15;
                sheet1.Range[xlsRow, iTotal].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iTotal].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, iTotal].CellStyle.FillBackground = ExcelKnownColors.Yellow;

                xlsCol += 1;
                iValid = xlsCol;
                sheet1.Range[xlsRow, iValid].Text = "Valid";
                sheet1.Range[xlsRow, iValid].ColumnWidth = 15;
                sheet1.Range[xlsRow, iValid].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iValid].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, iValid].CellStyle.FillBackground = ExcelKnownColors.Green;
                sheet1.Range[xlsRow, iValid].CellStyle.Font.Color = ExcelKnownColors.White;

                xlsCol += 1;
                iLO = xlsCol;
                sheet1.Range[xlsRow, iLO].Text = "Lunch Out";
                sheet1.Range[xlsRow, iLO].ColumnWidth = 15;
                sheet1.Range[xlsRow, iLO].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iLO].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, iLO].CellStyle.FillBackground = ExcelKnownColors.Red;
                sheet1.Range[xlsRow, iLO].CellStyle.Font.Color = ExcelKnownColors.White;

                xlsCol += 1;
                iNOO = xlsCol;
                sheet1.Range[xlsRow, iNOO].Text = "NO Out";
                sheet1.Range[xlsRow, iNOO].ColumnWidth = 15;
                sheet1.Range[xlsRow, iNOO].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iNOO].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, iNOO].CellStyle.FillBackground = ExcelKnownColors.Blue;
                sheet1.Range[xlsRow, iNOO].CellStyle.Font.Color = ExcelKnownColors.White;

                xlsCol += 1;
                iMO = xlsCol;
                sheet1.Range[xlsRow, iMO].Text = "Without In";
                sheet1.Range[xlsRow, iMO].ColumnWidth = 15;
                sheet1.Range[xlsRow, iMO].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iMO].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, iMO].CellStyle.FillBackground = ExcelKnownColors.Violet;
                sheet1.Range[xlsRow, iMO].CellStyle.Font.Color = ExcelKnownColors.White;

                #endregion ------------------Details Header-----------------

                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                endXlsCol = xlsCol;
                xlsCol = 1;
                xlsRow += 1;

                #endregion ------------------Column Header------------------

                //for (int i = 0; i <= dtEmp.Rows.Count - 1; i++)
                //{
                //    xlsCol = 1;

                //    #region ----------------------Data-----------------------                    
                //    strCount += 1;
                //    sheet1.Range[xlsRow, iSrNo].Number = strCount;
                //    sheet1.Range[xlsRow, iSrNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //    sheet1.Range[xlsRow, iSrNo].VerticalAlignment = ExcelVAlign.VAlignCenter;

                //    sheet1.Range[xlsRow, iEmpCode].Text = dtEmp.Rows[i]["EmployeeCode"].ToString().Trim();
                //    sheet1.Range[xlsRow, iEmpCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //    sheet1.Range[xlsRow, iEmpCode].VerticalAlignment = ExcelVAlign.VAlignCenter;

                //    sheet1.Range[xlsRow, iEmpName].Text = dtEmp.Rows[i]["EmployeeName"].ToString().Trim();
                //    sheet1.Range[xlsRow, iEmpName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //    sheet1.Range[xlsRow, iEmpName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                //    sheet1.Range[xlsRow, iDOJ].Text = dtEmp.Rows[i]["DOJ"].ToString().Trim();
                //    sheet1.Range[xlsRow, iDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //    sheet1.Range[xlsRow, iDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                //    sheet1.Range[xlsRow, iUnit].Text = dtEmp.Rows[i]["Unit"].ToString().Trim();
                //    sheet1.Range[xlsRow, iUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //    sheet1.Range[xlsRow, iUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;

                //    sheet1.Range[xlsRow, iDepart].Text = dtEmp.Rows[i]["Department"].ToString().Trim();
                //    sheet1.Range[xlsRow, iDepart].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //    sheet1.Range[xlsRow, iDepart].VerticalAlignment = ExcelVAlign.VAlignCenter;

                //    sheet1.Range[xlsRow, iSec].Text = dtEmp.Rows[i]["Section"].ToString().Trim();
                //    sheet1.Range[xlsRow, iSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //    sheet1.Range[xlsRow, iSec].VerticalAlignment = ExcelVAlign.VAlignCenter;


                //    sheet1.Range[xlsRow, iDesig].Text = dtEmp.Rows[i]["LegalDG"].ToString().Trim();
                //    sheet1.Range[xlsRow, iDesig].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //    sheet1.Range[xlsRow, iDesig].VerticalAlignment = ExcelVAlign.VAlignCenter;

                //    sheet1.Range[xlsRow, iEmployeeCategory].Text = dtEmp.Rows[i]["EmployeeCategory"].ToString().Trim();
                //    sheet1.Range[xlsRow, iEmployeeCategory].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //    sheet1.Range[xlsRow, iEmployeeCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    
                    

                //    #endregion ----------------------Data-----------------------

                //    #region Line Setup

                //    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                //    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                //    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].WrapText = true;

                //    #endregion Line Setup
                //}

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
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 30;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

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
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
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
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 26;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Machine Layout Report";
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 11;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;



                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Month:- " + MonthName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 9;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A8"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region Page Setup

                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                sheet1.Name = "MachineLayoutReport";

                #endregion Page Setup

                workbook.Version = ExcelVersion.Excel97to2003;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "MonthlyLunchOut.xls";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                objRpt = null;
                excelEngine = null;
                application = null;
                workbook = null;
            }
        }
        public void SelectedPlantWiseCompany(string sPlantID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT p.UserName PlantName,c.UserName CompanyName ,ISNULL(a.Address1,'')+','+ ISNULL(a.Address2,'') Address1, a.Phone,a.Email
                                ,cm.Address1 cAddress1 ,cm.Address2 cAddress2
                                FROM org.Plant p
							LEFT OUTER JOIN org.Company c on c.Id=p.CompanyId
							LEFT OUTER JOIN mst.AddressMaster a on a.Id=p.AddressMasterId
							LEFT OUTER JOIN mst.AddressMaster cm on cm.Id=c.AddressMasterId
							WHERE p.Id='" + sPlantID + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end of function
        public void SelectedPlant(string sPlantID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT P.UserName,AM.Address1+','+ ISNULL(AM.Address2,'') Address1 FROM ORG.Plant P
                            LEFT OUTER JOIN MST.AddressMaster AM ON P.AddressMasterId=AM.Id
                             WHERE P.Id = '" + sPlantID + @"'";
                //strSql = @"SELECT * FROM ORG.Plant WHERE Id = '" + sPlantID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end of function
        #endregion
    }
}