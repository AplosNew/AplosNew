using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.HumanResources;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.HumanResources.PayRegisterBDReportService;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class ManpowerAttendanceSummaryController : BaseController
    {
        #region Constructor

        private readonly IManpowerAttendanceSummary _manpowerAttendanceSummary;
        private readonly ISqlRepository _sqlRepository;

        public ManpowerAttendanceSummaryController(
              IManpowerAttendanceSummary manpowerAttendanceSummary, ISqlRepository sqlRepository
            )
        {
            _manpowerAttendanceSummary = manpowerAttendanceSummary;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages

       
        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult AplosNew()
        {
            return View();
        }

        public ActionResult AttendanceGroup()
        {
            return View();
        }
       
        public ActionResult CustomAttdnSummary()
        {
            return View();
        }
        #endregion -- Pages
        [HttpGet, Authorize]
        public ActionResult GetmanpowerAttendanceSummaryrViewReportOld(string workDate, bool withLine, string PlantId, string typeList, bool WithoutTBS, bool WithoutLA)
        {
            string PlantIds = string.Empty;
            string typeLists = string.Empty;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "ManpowerSummary";
            #region Validation
            if (!string.IsNullOrEmpty(typeList))
            {
                typeLists = "'" + typeList.Replace(",", "','") + "'";
            }
            else
            {
                throw new Exception("Please Select the Employee Code Type");
            }
            if (!string.IsNullOrEmpty(PlantId))
            {
                PlantIds = "'" + PlantId.Replace(",", "','") + "'";
            }
            else
            {
                throw new Exception("Please Select Plant");
            }
            #endregion
            if (!withLine)
            {
                var workbook = _manpowerAttendanceSummary.GetSummaryManpowerAttendanceExcel(identity.CompanyGroupId, identity.CompanyId,workDate, withLine,true, PlantIds, typeLists, WithoutTBS, WithoutLA);

                workbook.Version = ExcelVersion.Excel97to2003;
                //workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return RenderReportAsPdf(workbook, fileName);
            }
            else
            {
                var workbook = _manpowerAttendanceSummary.GetSummaryManpowerAttendanceExcelWithLine(identity.CompanyGroupId, identity.CompanyId, PlantIds, workDate, withLine, typeLists, WithoutTBS, WithoutLA);
               
                workbook.Version = ExcelVersion.Excel97to2003;
                return RenderReportAsPdf(workbook, fileName);
            }

            //return null;
        }

        [HttpGet, Authorize]
        public ActionResult GetmanpowerAttendanceSummaryrReportOld(string workDate, bool withLine, bool withDesignation, string PlantId,string typeList,bool WithoutTBS,bool WithoutLA)
        {
            string typeLists = string.Empty;
            string PlantIds = string.Empty;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "ManpowerSummary" + DateTime.Now.ToString("yyMMdd") + ".xls";
            #region Validation
            if (!string.IsNullOrEmpty(typeList))
            {
                typeLists = "'" + typeList.Replace(",", "','") + "'";
            }
            else
            {
                throw new Exception("Please Select the Employee Code Type");
            }
            if (!string.IsNullOrEmpty(PlantId))
            {
                PlantIds = "'" + PlantId.Replace(",", "','") + "'";
            }
            else
            {
                throw new Exception("Please Select Plant");
            }
            #endregion
            if (!withLine)
            {
                var workbook = _manpowerAttendanceSummary.GetSummaryManpowerAttendanceExcelNew(identity.CompanyGroupId, identity.CompanyId, workDate, withLine,withDesignation, PlantIds, typeLists, WithoutTBS,WithoutLA);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);

            }
            else
            {
                var workbook = _manpowerAttendanceSummary.GetSummaryManpowerAttendanceExcelNew(identity.CompanyGroupId, identity.CompanyId, workDate, withLine, withDesignation, PlantIds, typeLists, WithoutTBS, WithoutLA);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
            }

            return null;
        }


        [HttpGet, Authorize]
        public ActionResult GetmanpowerAttendanceSummaryrViewReport(string workDate, bool withLine)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "ManpowerSummary";
            if (!withLine)
            {
               // var workbook = _manpowerAttendanceSummary.GetSummaryManpowerAttendanceExcel(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, workDate, withLine);
                var workbook = _manpowerAttendanceSummary.GetSummaryManpowerAttendanceExcelNew(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, workDate, withLine);
                workbook.Version = ExcelVersion.Excel97to2003;
                //workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return RenderReportAsPdf(workbook, fileName);
            }
            else
            {
                //var workbook = _manpowerAttendanceSummary.GetSummaryManpowerAttendanceExcelWithLine(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, workDate, withLine);
                var workbook = _manpowerAttendanceSummary.GetSummaryManpowerAttendanceExcelWithLineNew(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, workDate, withLine);
                workbook.Version = ExcelVersion.Excel97to2003;
                return RenderReportAsPdf(workbook, fileName);
            }

            //return null;
        }

        [HttpGet,Authorize]
        public ActionResult GetmanpowerAttendanceSummaryrReport(string workDate, bool withLine)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "ManpowerSummary" + DateTime.Now.ToString("yyMMdd") + ".xls";
            if(!withLine)
            {
                var workbook = _manpowerAttendanceSummary.GetSummaryManpowerAttendanceExcelNew(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, workDate, withLine);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);

            }
            else
            {
                var workbook = _manpowerAttendanceSummary.GetSummaryManpowerAttendanceExcelWithLineNew(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, workDate, withLine);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
            }

            return null;
        }

        [HttpGet, Authorize]
        public ActionResult GetCustomizedAttendanceSummaryReport(string workDate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "ManpowerAttdnSummary" + DateTime.Now.ToString("yyMMdd") + ".xls";
         
                var workbook = _manpowerAttendanceSummary.GetCustomizedAttendanceSummaryReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, workDate);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);          
                
                //workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);

            return null;
        }

        [HttpGet, Authorize]
        public ActionResult GetCustomizedAttendanceSummaryViewReport(string workDate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "ManpowerAttdnSummary";

            var workbook = _manpowerAttendanceSummary.GetCustomizedAttendanceSummaryReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, workDate);
            workbook.Version = ExcelVersion.Excel97to2003;
            //workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
            return RenderReportAsPdf(workbook, fileName);
            //workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);

            //return null;
        }
        [HttpGet, Authorize]
        public ActionResult GetSummaryManpowerAttendanceGroupWiseExcel(string workDate, string sUnitID, string sDivID, string sDepID, string sSecID, string sSubSecID)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "ManpowerSummary" + DateTime.Now.ToString("yyMMdd") + ".xls";
            var workbook = _manpowerAttendanceSummary.GetSummaryManpowerAttendanceGroupWiseExcel(identity.PlantId, identity.CompanyId, workDate, sUnitID, sDivID, sDepID, sSecID, sSubSecID);
            workbook.Version = ExcelVersion.Excel97to2003;
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);

            return null;
        }
        [HttpGet, Authorize]
        public ActionResult GetAttendancFromAppReport(string PlantId, string companyId, string workDate, string sUnitID, string sDivID, string sDepID, string sSecID, string sSubSecID)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Attendanc Summary" + DateTime.Now.ToString("yyMMdd") + ".xls";
            var workbook = _manpowerAttendanceSummary.GetAttendancFromAppSummaryExcel1(identity.PlantId, identity.CompanyId, workDate, sUnitID, sDivID, sDepID, sSecID, sSubSecID);
            workbook.Version = ExcelVersion.Excel97to2003;
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);

            return null;
        }


        public void GetAttendanceSummarySql(string WorkDate, out DataSet dsRef)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            clsStaticInfo obs = null;

            try
            {
                string wc = string.Empty;

                obs = new clsStaticInfo();
                strSql = @"SELECT   Division.UserName DivisionName,Division.Id DivisionId, SubSection.UserName SubSectionName,SubSection.Id SubSectionId,Designation.UserName DesignationName,Designation.Id DesignationId
                        ,Division.Sequence,SubSection.Sequence, Designation.Sequence
                            ,SUM (case when dt.Category IN ('Present','Late') then 1 else 0 end ) SUM_PRESENT 
                            ,SUM (case when dt.Category='Late'then 1 else 0  end ) SUM_Late 
                            ,SUM (case when dt.Category='Absent'then 1 else 0  end ) SUM_Absent
                            ,SUM (case when dt.Category IN ('Leave','Holiday','Weekend')then 1 else 0  end ) SUM_OFF
                            ,SUM (case when dt.Category IN ('Holiday','Weekend') then 1 else 0  end ) SUM_Others
                            --,SUM (case when e.SystemId Not in(select EmpSystemID  from AttdnProcessData  where WorkDate = '10-Feb-2020') then 1 else 0  end ) SUM_AttdnNotProcessed

                            ,count (E.SystemId) ONROLL
                            FROM AttdnProcessData apd
                            left join EmployeeInformation e on e.SystemId=apd.EmpSystemID        
                            left join [ORG].[Division] Division on Division.Id=e.DivisionId
                            left join [ORG].[SubSection] SubSection on SubSection.Id=e.SubSectionId
                            left join [HKP].[Designation] Designation on Designation.Id=e.DesignationSystemID
                            jOIN DayType Dt ON Dt.DayType=apd.DayStatus
                            WHERE APD.WorkDate='" + WorkDate + @"' and E.PlantId = '" + identity.PlantId + @"'
                            GROUP BY Division.UserName,Division.Id,SubSection.UserName,SubSection.Id,Designation.UserName,Designation.Id
							 ,Division.Sequence,SubSection.Sequence, Designation.Sequence
                            order by Division.Sequence,SubSection.Sequence, Designation.Sequence";

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

        #region -----------------------------------Excel Report--------------------------------------------------
        [HttpGet]
        public ActionResult GetDailyAttendanceSummary(string WorkDate)//XlsDailyAttendanceSummaryRpt()
        {

            #region Variable
            clsReport objRpt = null;
            DataSet dsAttdnSummary = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;
            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;

            #endregion Variable

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ru = new ReportUtility();
                objRpt = new clsReport();
                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();
                para.PlantId = identity.PlantId;
                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion Variable

                #region DataSet

                GetAttendanceSummarySql(WorkDate, out dsAttdnSummary);

                DataTable dtAttdnSummary = dsAttdnSummary.Tables[0].DefaultView.ToTable();
                if (dtAttdnSummary.Rows.Count == 0)
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }
                DataView dvAttendance = new DataView(dsAttdnSummary.Tables[0]);

                object totalPresentDays;
                totalPresentDays = dvAttendance.ToTable().Compute(@"Sum(SUM_PRESENT)", null);

                object totalAbsentDays;
                totalAbsentDays = dvAttendance.ToTable().Compute(@"Sum(SUM_Absent)", null);

                object totalOFFDays;
                totalOFFDays = dvAttendance.ToTable().Compute(@"Sum(SUM_OFF)", null);

                object totalLeaveDays;
                totalLeaveDays = dvAttendance.ToTable().Compute(@"Sum(SUM_Late)", null);

                #endregion DataSet
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                xlsRow = 7;
                xlsCol = 1;

                #region------------------Column Header------------------

                SetHeadText("Division", sheet1, xlsRow, ref xlsCol, out int colDivision, 37);
                SetHeadText("SubSection", sheet1, xlsRow, ref xlsCol, out int colSubSection, 24);
                SetHeadText("Designation", sheet1, xlsRow, ref xlsCol, out int colDesignation, 16);
                SetHeadText("OnRole", sheet1, xlsRow, ref xlsCol, out int colOnRole, 16);
                SetHeadText("Present", sheet1, xlsRow, ref xlsCol, out int colPresent, 16);
                SetHeadText("Absent", sheet1, xlsRow, ref xlsCol, out int colAbsent, 16);
                SetHeadText("Late", sheet1, xlsRow, ref xlsCol, out int colLate, 16);
                SetHeadText("Leave", sheet1, xlsRow, ref xlsCol, out int colLeave, 16);
                SetHeadText("Others", sheet1, xlsRow, ref xlsCol, out int colOthers, 16);
                SetHeadText("Remarks", sheet1, xlsRow, ref xlsCol, out int ColRemarks, 16);





                int COL = ColRemarks + 1;

                #endregion


                int RowHeaderLimit = xlsRow;
                #endregion------------------Column Header------------------

                endXlsCol = (COL - 1);
                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                //Param param = new Param();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;

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
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 30;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Daily Attendance Summary";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                var strRptDateRange = "";
                strRptDateRange = WorkDate;
                sheet1.Range[xlsRow, xlsCol].Text = strRptDateRange;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                xlsRow = 8;
                int startRow = xlsRow;
                string groupid = "";
                string SubSection = "";
                string Designation = "";

                for (int i = 0; i < dsAttdnSummary.Tables[0].Rows.Count; i++)
                {
                    if (groupid == dsAttdnSummary.Tables[0].Rows[i]["DivisionId"].ToString())
                        continue;

                    int RowDepartment = xlsRow;
                    SubSection = "";
                    groupid = dsAttdnSummary.Tables[0].Rows[i]["DivisionId"].ToString();

                    sheet1[xlsRow, colDivision].Text = dsAttdnSummary.Tables[0].Rows[i]["DivisionName"].ToString();

                    dsAttdnSummary.Tables[0].DefaultView.RowFilter = "DivisionId='" + dsAttdnSummary.Tables[0].Rows[i]["DivisionId"].ToString() + "'";


                    for (int J = 0; J < dsAttdnSummary.Tables[0].DefaultView.Count; J++)
                    {
                        if (SubSection == dsAttdnSummary.Tables[0].DefaultView[J]["SubSectionId"].ToString())
                            continue;
                        SubSection = dsAttdnSummary.Tables[0].DefaultView[J]["SubSectionId"].ToString();

                        sheet1[xlsRow, colSubSection].Text = dsAttdnSummary.Tables[0].DefaultView[J]["SubSectionName"].ToString();

                        dtAttdnSummary.DefaultView.RowFilter = "DivisionId='" + dsAttdnSummary.Tables[0].Rows[i]["DivisionId"].ToString() + "' AND SubSectionId='" + dsAttdnSummary.Tables[0].DefaultView[J]["SubSectionId"].ToString() + "'";

                        for (int JD = 0; JD < dtAttdnSummary.Rows.Count; JD++)
                        {

                            //dtAttdnSummary.DefaultView.RowFilter = "DivisionId='" + dsAttdnSummary.Tables[0].Rows[i]["DivisionId"].ToString() + "' AND SubSectionId='" + dsAttdnSummary.Tables[0].DefaultView[J]["SubSectionId"].ToString() + "' AND DesignationId='" + dsAttdnSummary.Tables[0].DefaultView[JD]["DesignationId"].ToString() + "'";

                            sheet1[xlsRow, colOnRole].Text = dtAttdnSummary.Rows[JD]["ONROLL"].ToString();
                            sheet1[xlsRow, colPresent].Text = dtAttdnSummary.Rows[JD]["SUM_PRESENT"].ToString();
                            sheet1[xlsRow, colAbsent].Text = dtAttdnSummary.Rows[JD]["SUM_Absent"].ToString();
                            sheet1[xlsRow, colLeave].Text = dtAttdnSummary.Rows[JD]["SUM_OFF"].ToString();
                            sheet1[xlsRow, colOthers].Text = dtAttdnSummary.Rows[JD]["SUM_Others"].ToString();
                            if (Designation == dtAttdnSummary.Rows[JD]["DesignationId"].ToString())
                                continue;
                            Designation = dtAttdnSummary.Rows[JD]["DesignationId"].ToString();

                            sheet1[xlsRow, colDesignation].Text = dtAttdnSummary.Rows[JD]["DesignationName"].ToString();

                        }
                        xlsRow++;
                    }
                    //total for department

                    xlsRow++;
                }

                //grand total
                sheet1[xlsRow, colDivision].Text = "Grand Total";
                sheet1.Range[xlsRow, colDivision, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, colDivision, xlsRow, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                xlsRow += 1;

                sheet1.IsDisplayZeros = false;
                #endregion ----------------------Data-----------------------

                var endXlsRow = xlsRow;

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }
                #endregion

                #region Freeze Panes
                var xx = RowHeaderLimit + 1;
                sheet1.UsedRange["A" + xx].FreezePanes();

                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                sheet1.Name = "Attendance Summary";
                #endregion

                workbook.Version = ExcelVersion.Excel97to2003;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "Attdn Summary.xls";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
        }

        string GetFormulaGrandTotal(ArrayList al, int col)
        {
            string _formula = string.Empty;
            ReportUtility ru = new ReportUtility();
            try
            {
                for (int i = 0; i < al.Count; i++)
                {
                    if (_formula.Length == 0)
                    {
                        _formula = "=" + ru.GetColumnNameForXls(col) + al[i];
                    }
                    else
                    {
                        _formula += "+" + ru.GetColumnNameForXls(col) + al[i];
                    }
                }
                return _formula;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, string Text)
        {
            //if (string.IsNullOrEmpty(Text) == false)
            //{
            sheet.Range[xlsRow, xlsCol].Text = Text;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            //}
        }
        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, double Number)
        {
            //if (string.IsNullOrEmpty(Text) == false)
            //{
            sheet.Range[xlsRow, xlsCol].Number = Number;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            //}
        }
        private void SetHeadText(IWorksheet sheet, int xlsRow, int xlsCol, string text)
        {
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
        }
        private void SetHeadText(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.AliceBlue;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ColIndex = xlsCol;
            xlsCol += 1;
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
    }
}