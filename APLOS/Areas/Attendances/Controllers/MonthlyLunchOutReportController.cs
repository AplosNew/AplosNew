using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Attendances;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Attendances.Controllers
{
    public class MonthlyLunchOutReportController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMonthlyAttendanceInformation _monthlyAttendanceInformation;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private DataSet dsRef;
        private object workbook;
        private object objRpt;
        private object excelEngine;
        private object application;

        public MonthlyLunchOutReportController(
              IMaternityLeavePolicyService LeavePolicyService,
            ISqlRepository sqlRepository,
            IMonthlyAttendanceInformation monthlyAttendanceInformation
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
            _monthlyAttendanceInformation = monthlyAttendanceInformation;
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
        public ActionResult MonthlyLunchOutRpt(string Month, string Year)
        {
            #region Variable
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsReport objRpt = null;
            DataSet dsOT = null;
            DataTable dtOT = null;
            DataView dvOT = null;
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
            #endregion Variable

            try
            {
                objRpt = new clsReport();
                ReportUtility oru = new ReportUtility();

                #region Validation
                string m = bplib.clsWebLib.GetMonthName(Month);
                dtFrmDt = Convert.ToDateTime("01-" + m + "-" + Year);
                if (Convert.ToInt32(DateTime.Now.Month) != Convert.ToInt32(Month))
                {
                    dtEndDate = dtFrmDt.AddMonths(1).AddDays(-1);
                }
                #region --Month Name--
                if (Month == "1")
                {
                    MonthName = "January";
                }
                else if (Month == "2")
                {
                    MonthName = "February";
                }
                else if (Month == "3")
                {
                    MonthName = "March";
                }
                else if (Month == "4")
                {
                    MonthName = "April";
                }
                else if (Month == "5")
                {
                    MonthName = "May";
                }
                else if (Month == "6")
                {
                    MonthName = "June";
                }
                else if (Month == "7")
                {
                    MonthName = "July";
                }
                else if (Month == "8")
                {
                    MonthName = "August";
                }
                else if (Month == "9")
                {
                    MonthName = "September";
                }
                else if (Month == "10")
                {
                    MonthName = "October";
                }
                else if (Month == "11")
                {
                    MonthName = "November";
                }
                else
                {
                    MonthName = "December";
                }
                #endregion
                #endregion Validation

                #region Variable
                string sUnit = "ALL";
                string sDevi = "ALL";
                string sDept = "ALL";
                string sSect = "ALL";
                string sSbSe = "ALL";
                string sLine = "ALL";
                string sEmpC = "ALL";
                string sDeGr = "ALL";
                string sDesi = "ALL";
                var otStartCol = 0;
                var otEndCol = 0;
                #endregion Variable

                #region DataSet

                objRpt.GetMonthlyLunchOutRpt(identity.PlantId, dtFrmDt.ToString("dd-MMM-yyyy"), dtEndDate.ToString("dd-MMM-yyyy"), sUnit, sDevi, sDept, sSect, sSbSe, sLine, sEmpC, sDeGr, sDesi, out dsOT);
                dtOT = dsOT.Tables[0];
                dvOT = new DataView();
                dvOT.Table = dsOT.Tables[0];
                var ListOT = dsOT.Tables[0].ToList<MLunchOut>();
                DataView dvEmp = new DataView(dsOT.Tables[0]);
                DataTable dtEmp = dvEmp.ToTable(true, "EmployeeCode", "EmployeeName", "DOJ", "Unit", "Department", "Section", "Designation", "GivenDesignation", "LegalDG", "EmployeeCategory");
                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet

                object chequeAmount;
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

                #endregion Variables

                #region ------------------Column Header------------------

                #region ------------------Details Header-----------------

                xlsRow += 1;

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

                xlsCol = iEmployeeCategory;
                while (dtFrmDt <= dtEndDate)
                {
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = dtFrmDt.ToString("dd");
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 5;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    dtFrmDt = dtFrmDt.AddDays(1);
                }
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

                for (int i = 0; i <= dtEmp.Rows.Count - 1; i++)
                {
                    xlsCol = 1;

                    #region ----------------------Data-----------------------                    
                    strCount += 1;
                    sheet1.Range[xlsRow, iSrNo].Number = strCount;
                    sheet1.Range[xlsRow, iSrNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iSrNo].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, iEmpCode].Text = dtEmp.Rows[i]["EmployeeCode"].ToString().Trim();
                    sheet1.Range[xlsRow, iEmpCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, iEmpCode].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, iEmpName].Text = dtEmp.Rows[i]["EmployeeName"].ToString().Trim();
                    sheet1.Range[xlsRow, iEmpName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, iEmpName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, iDOJ].Text = dtEmp.Rows[i]["DOJ"].ToString().Trim();
                    sheet1.Range[xlsRow, iDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, iDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, iUnit].Text = dtEmp.Rows[i]["Unit"].ToString().Trim();
                    sheet1.Range[xlsRow, iUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, iUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, iDepart].Text = dtEmp.Rows[i]["Department"].ToString().Trim();
                    sheet1.Range[xlsRow, iDepart].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, iDepart].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, iSec].Text = dtEmp.Rows[i]["Section"].ToString().Trim();
                    sheet1.Range[xlsRow, iSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, iSec].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    sheet1.Range[xlsRow, iDesig].Text = dtEmp.Rows[i]["LegalDG"].ToString().Trim();
                    sheet1.Range[xlsRow, iDesig].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, iDesig].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, iEmployeeCategory].Text = dtEmp.Rows[i]["EmployeeCategory"].ToString().Trim();
                    sheet1.Range[xlsRow, iEmployeeCategory].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, iEmployeeCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    dtFrmDt = Convert.ToDateTime(Month + "/" + "01/" + Year);
                    string _m = bplib.clsWebLib.GetMonthName(Month);
                    dtFrmDt = Convert.ToDateTime("01-" + _m + "-" + Year);
                    xlsCol = iEmployeeCategory;
                    otStartCol = iEmployeeCategory + 1;
                    EarlyOutStatus Eos = new EarlyOutStatus();
                    while (dtFrmDt <= dtEndDate)
                    {
                        xlsCol += 1;
                        string ecode = dtEmp.Rows[i]["EmployeeCode"].ToString().Trim();
                        var _ddd = Convert.ToDateTime(dtFrmDt.ToString("dd-MMM-yyyy"));
                        var _data = ListOT.Where(r => r.EmployeeCode == ecode && r.WorkDate == _ddd).FirstOrDefault();
                        if (_data != null)
                        {                            
                            sheet1.Range[xlsRow, xlsCol].Text =_data.DayStatus;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            if (_data.Status == "NOP")
                            {
                                Eos.NOP = Eos.NOP + 1;
                                sheet1.Range[xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.Yellow;                                
                            }
                            else if (_data.Status == "VLD")
                            {
                                Eos.VLD = Eos.VLD + 1;
                                sheet1.Range[xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.Green;
                                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.White;
                            }
                            else if (_data.Status == "LO")
                            {
                                Eos.LO = Eos.LO + 1;
                                sheet1.Range[xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.Red;
                                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.White;
                            }
                            else if (_data.Status == "NOO")
                            {
                                Eos.NOO = Eos.NOO + 1;
                                sheet1.Range[xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.Blue;
                                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.White;
                            }
                            else if (_data.Status == "OM")
                            {
                                Eos.MO = Eos.MO + 1;
                                sheet1.Range[xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.Violet;
                                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.White;
                            }
                            else
                            {
                                
                            }                            
                        }
                        dtFrmDt = dtFrmDt.AddDays(1);
                    }
                    otEndCol = xlsCol;
                    string jot = string.Empty;//OTConsiderOn
                    //oru.GetOT(dsOT.Tables[0].Rows[0]["OTConsiderOn"].ToString(), chequeAmount.ToString(), out jot);
                    var tt = jot;

                    sheet1.Range[xlsRow, iTotal ].Number = Eos.NOP;
                    sheet1.Range[xlsRow, iTotal ].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                    sheet1.Range[xlsRow, iTotal ].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //xlsRow += 1;

                    //sheet1.Range[xlsRow, iDesig].Text = dtEmp.Rows[i]["LegalDG"].ToString().Trim();
                    //sheet1.Range[xlsRow, iDesig].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    //sheet1.Range[xlsRow, iDesig].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, iValid ].Number = Eos.VLD;
                    sheet1.Range[xlsRow, iValid ].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                    sheet1.Range[xlsRow, iValid ].VerticalAlignment = ExcelVAlign.VAlignCenter;
                   // xlsRow += 1;

                    sheet1.Range[xlsRow, iLO ].Number = Eos.LO;
                    sheet1.Range[xlsRow, iLO ].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                    sheet1.Range[xlsRow, iLO ].VerticalAlignment = ExcelVAlign.VAlignCenter;
                   // xlsRow += 1;

                    sheet1.Range[xlsRow, iNOO ].Number = Eos.NOO;
                    sheet1.Range[xlsRow, iNOO ].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                    sheet1.Range[xlsRow, iNOO ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, iMO].Number = Eos.MO;
                    sheet1.Range[xlsRow, iMO].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                    sheet1.Range[xlsRow, iMO].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsRow += 1;

                    #endregion ----------------------Data-----------------------

                    #region Line Setup

                    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].WrapText = true;

                    #endregion Line Setup
                }

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
                sheet1.Range[xlsRow, 3].Text = "Monthly Lunch Out Report";
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 11;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                //xlsRow += 1;
                //sheet1.Range[xlsRow, xlsCol].Text = "Report Ref  No.";
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                //sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 9;
                //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                //sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Year:- " + Year + " and Month:- " + MonthName;
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

                sheet1.Name = "MonthlyLunchOutReport";

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
        class MLunchOut
        {
            //public decimal TotalOTHr { get; set; }
            public string EmployeeCode { get; set; }
            public string Status { get; set; }
            public string DayStatus { get; set; }
            public DateTime WorkDate { get; set; }

        }

        #endregion -- Operations  
    }
}