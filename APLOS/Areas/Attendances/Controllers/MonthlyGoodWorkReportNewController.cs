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
using Library.HumanResource.Attendance;

namespace Aplos.Areas.Attendances.Controllers
{
    public class MonthlyGoodWorkReportNewController : BaseController
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
        MonthlyGoodWorkReport gw = new MonthlyGoodWorkReport();

        public MonthlyGoodWorkReportNewController(
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



        public ActionResult Aplos()
        {
            return View();
        }

        #region -- Operations

        [HttpPost, Authorize]
        public ActionResult XlsGoodWorkReport(string Month, string Year , Dictionary<string , string> parameters, string typeId)
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
                string typeid = "'" + typeId.Replace(",", "','") + "'";//replaced with ""

                gw.GetMonthlyGoodWorkReportNew( parameters, typeid, dtFrmDt.ToString("dd-MMM-yyyy"), dtEndDate.ToString("dd-MMM-yyyy"), sUnit, sDevi, sDept, sSect, sSbSe, sLine, sEmpC, sDeGr, sDesi, out dsOT );
                dtOT = dsOT.Tables[0];
                dvOT = new DataView();
                dvOT.Table = dsOT.Tables[0];
                var ListOT = dsOT.Tables[0].ToList<OTReport>();
                DataView dvEmp = new DataView(dsOT.Tables[0]);
                DataTable dtEmp = dvEmp.ToTable(true, "EmployeeCode", "EmployeeName", "Plant" ,"DOJ", "Unit", "Department", "Section", "Designation", "GivenDesignation", "LegalDG", "EmployeeCodeType","Basic", "GWPaymentAdviseId");
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
                int PlantName = 0;
                int iDOJ = 0;
                int iUnit = 0;
                int iDepart = 0;
                int iSec = 0;
                int iDesig = 0;
                int iBasic = 0;
                int iGWPaymentAdviseId = 0;
                int iTotal = 0;
                int iLine = 0;
                int iType = 0;

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
                PlantName = xlsCol;
                sheet1.Range[xlsRow, PlantName].Text = "Plant Name";
                sheet1.Range[xlsRow, PlantName].ColumnWidth = 22;
                sheet1.Range[xlsRow, PlantName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, PlantName].VerticalAlignment = ExcelVAlign.VAlignCenter;
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
                iBasic = xlsCol;
                sheet1.Range[xlsRow, iBasic].Text = "Basic";
                sheet1.Range[xlsRow, iBasic].ColumnWidth = 15;
                sheet1.Range[xlsRow, iBasic].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iBasic].VerticalAlignment = ExcelVAlign.VAlignCenter;

                xlsCol += 1;
                iGWPaymentAdviseId = xlsCol;
                sheet1.Range[xlsRow, iGWPaymentAdviseId].Text = "GWPaymentAdviseId";
                sheet1.Range[xlsRow, iGWPaymentAdviseId].ColumnWidth = 15;
                sheet1.Range[xlsRow, iGWPaymentAdviseId].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iGWPaymentAdviseId].VerticalAlignment = ExcelVAlign.VAlignCenter;

                xlsCol += 1;
                iType = xlsCol;
                sheet1.Range[xlsRow, iType].Text = "EmployeeCode Type";
                sheet1.Range[xlsRow, iType].ColumnWidth = 15;
                sheet1.Range[xlsRow, iType].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iType].VerticalAlignment = ExcelVAlign.VAlignCenter;

                xlsCol = iType;
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
                sheet1.Range[xlsRow, iTotal].Text = "Total";
                sheet1.Range[xlsRow, iTotal].ColumnWidth = 15;
                sheet1.Range[xlsRow, iTotal].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iTotal].VerticalAlignment = ExcelVAlign.VAlignCenter;

                #endregion ------------------Details Header-----------------

                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
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
                    chequeAmount = dsOT.Tables[0].Compute(@"Sum(TotalOTHr)", "EmployeeCode ='" + dtEmp.Rows[i]["EmployeeCode"].ToString().Trim() + "'");
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

                    sheet1.Range[xlsRow, PlantName].Text = dtEmp.Rows[i]["Plant"].ToString().Trim();
                    sheet1.Range[xlsRow, PlantName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, PlantName].VerticalAlignment = ExcelVAlign.VAlignCenter;

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

                    sheet1.Range[xlsRow, iBasic].Text = dtEmp.Rows[i]["Basic"].ToString().Trim();
                    sheet1.Range[xlsRow, iBasic].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, iBasic].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, iGWPaymentAdviseId].Text = dtEmp.Rows[i]["GWPaymentAdviseId"].ToString().Trim();
                    sheet1.Range[xlsRow, iGWPaymentAdviseId].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, iGWPaymentAdviseId].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, iType].Text = dtEmp.Rows[i]["EmployeeCodeType"].ToString().Trim();
                    sheet1.Range[xlsRow, iType].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, iType].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    dtFrmDt = Convert.ToDateTime(Month + "/" + "01/" + Year);
                    string _m = bplib.clsWebLib.GetMonthName(Month);
                    dtFrmDt = Convert.ToDateTime("01-" + _m + "-" + Year);
                    xlsCol = iType;
                    otStartCol = iType + 1;
                    while (dtFrmDt <= dtEndDate)
                    {
                        xlsCol += 1;
                        string ecode = dtEmp.Rows[i]["EmployeeCode"].ToString().Trim();
                        var _ddd = Convert.ToDateTime(dtFrmDt.ToString("dd-MMM-yyyy"));
                        var _data = ListOT.Where(r => r.EmployeeCode == ecode && r.workdate == _ddd).FirstOrDefault();
                        if (_data != null)
                        {
                            string yot = string.Empty;//OTConsiderOn
                            GetOT(dsOT.Tables[0].Rows[0]["OTConsiderOn"].ToString(), _data.TotalOTHr.ToString(), out yot);
                            //if (string.IsNullOrEmpty(dsOT.Tables[0].Rows[0]["OTConsiderOn"].ToString()))
                            //{

                            //    sheet1.Range[xlsRow, xlsCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Red;
                            //}
                            sheet1.Range[xlsRow, xlsCol].Text = yot.ToString();
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }
                        dtFrmDt = dtFrmDt.AddDays(1);
                    }
                    otEndCol = xlsCol;
                    string jot = string.Empty;//OTConsiderOn
                    GetOT(dsOT.Tables[0].Rows[0]["OTConsiderOn"].ToString(), chequeAmount.ToString(), out jot);
                    var tt = jot;

                    sheet1.Range[xlsRow, otEndCol + 1].Text = jot;
                    sheet1.Range[xlsRow, otEndCol + 1].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                    sheet1.Range[xlsRow, otEndCol + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
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

                //xlsRow += 1;
                //if (dsFactory.Tables[0].Rows.Count > 0)
                //{
                //    FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                //    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                //}
                //else
                //{
                //    FactoryName = "";
                //}
                //sheet1.Range[xlsRow, 3].Text = FactoryName;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                //sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                //xlsRow += 1;
                //if (dsFactory.Tables[0].Rows.Count > 0)
                //{
                //    FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                //}
                //else
                //{
                //    FactoryAddress = "";
                //}
                //sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 26;
                //sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Monthly Good Work Information";
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 11;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Report Ref  No.";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 9;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Year No:- " + Year + " and Month No:- " + Month;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 9;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

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

                sheet1.Name = "Monthly Good Work Information";

                #endregion Page Setup

                workbook.Version = ExcelVersion.Excel97to2003;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "MonthlyGoodWork.xls";
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


        [HttpGet, Authorize]
        public ActionResult getFilters()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(gw.getFilters(identity.CompanyId) , JsonRequestBehavior.AllowGet);
        }
      
        class OTReport
        {
            public decimal TotalOTHr { get; set; }
            public string EmployeeCode { get; set; }
            public DateTime workdate { get; set; }

        }

        private void GetOT(string OTConsiderOn, string OTHr, out string OT_output)
        {
            OT_output = string.Empty;
            try
            {
                string yot = string.Empty;
                if (string.IsNullOrEmpty(OTHr))
                {
                    yot = "0";
                }
                else
                {
                    yot = OTHr;
                }
                if (OTConsiderOn.ToUpper() == "HOUR MINUTE VALUE")//
                {
                    int hh = Convert.ToInt32(Math.Floor(Convert.ToDouble(bplib.clsWebLib.GetNumData(yot)))) / 60;
                    decimal mm = Convert.ToDecimal(bplib.clsWebLib.GetNumData(yot)) % 60;
                    if (mm == 0)
                    {
                        string minute = mm.ToString("F").TrimStart();
                        minute = minute.Substring(2, minute.Length - 2);
                        OT_output = hh + "." + minute;
                    }
                    else
                    {
                        decimal deciminute = Convert.ToDecimal((mm) / 60);
                        string substringvalue = deciminute.ToString().Substring(2, 2);
                        OT_output = hh + "." + substringvalue;
                    }


                }
                else
                {
                    double hh = Convert.ToDouble(bplib.clsWebLib.GetNumData(yot)) / 60;
                    //OT_output = hh.ToString();
                    OT_output = hh.ToString("0.##");


                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion -- Operations  
    }
}
    
