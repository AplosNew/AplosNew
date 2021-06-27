using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.Helpers.ReportUtility;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class ShiftSummaryController : BaseController
    {
        #region Constructor

        private readonly IAttendanceManagementService _AttendanceManagementService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;
        public ShiftSummaryController(
              IAttendanceManagementService AttendanceManagementService, IEmployeeProfileService employeeProfileService, ISqlRepository R
            )
        {
            _AttendanceManagementService = AttendanceManagementService;
            _employeeProfileService = employeeProfileService;
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages
        public void GetShiftSummarySql(string WorkDate, out DataSet dsRef)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            clsStaticInfo obs = null;

            try
            {
                string wc = string.Empty;

                obs = new clsStaticInfo();
                strSql = @"SELECT   sd.UserName AS [ShiftName],sd.SystemID ,d.UserName Department,s.UserName as [SECTION]
                            ,SUM (case when dt.Category IN ('Present','Late') then 1 else 0 end ) SUM_PRESENT 
                            ,SUM (case when dt.Category='Late'then 1 else 0  end ) SUM_Late 
                            ,SUM (case when dt.Category='Absent'then 1 else 0  end ) SUM_Absent
                            ,SUM (case when dt.Category IN ('Leave','Holiday','Weekend')then 1 else 0  end ) SUM_OFF
                            ,count (E.SystemId) ONROLL
                            FROM AttdnProcessData apd
                            left join EmployeeInformation e on e.SystemId=apd.EmpSystemID
                            left join ShiftDefination sd on sd.SystemID=apd.ShiftSystemID
                            left join [ORG].[Department] d on d.Id=e.DepartmentId
                            left join [ORG].[Section] s on s.Id=e.SectionId
                            left jOIN DayType Dt ON Dt.DayType=apd.DayStatus
                            WHERE APD.WorkDate='" + WorkDate + @"'
                            GROUP BY SD.UserName,sd.SystemID,d.UserName,S.UserName
                            order by d.UserName,S.UserName, sd.SystemID";

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

        public ActionResult Getdailyattendance(string WorkDate)//XlsDailyAttendanceSummaryRpt()
        {

            #region Variable
            clsReport objRpt = null;
            DataSet dsShiftSummary = null;
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

                GetShiftSummarySql(WorkDate, out dsShiftSummary);

                DataTable dtShiftSummary = dsShiftSummary.Tables[0].DefaultView.ToTable();
                if (dtShiftSummary.Rows.Count == 0)
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }
                DataView dvAttendance = new DataView(dsShiftSummary.Tables[0]);

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

                SetHeadText("Department", sheet1, xlsRow, ref xlsCol, out int colDepartment, 37);
                SetHeadText("Section", sheet1, xlsRow, ref xlsCol, out int colSec, 24);
                SetHeadText("OnRoll", sheet1, xlsRow, ref xlsCol, out int colDept_Strength, 16);

                #region dynamic shift
                string shift = @"SELECT DISTINCT ShiftSystemID,UserName,D.SequenceNo FROM EmpDateWiseShiftAssign a
                            INNER JOIN ShiftDefination d ON A.ShiftSystemID=D.SystemID
                            WHERE A.WorkDate='" + WorkDate + @"' AND D.IsActive=1
                            ORDER BY D.SequenceNo";
                DataTable dt = _sqlRepository.GetDataTable(shift);

                Dictionary<string, int> dicShift = new Dictionary<string, int>();

                int COL = colDept_Strength + 1;
                int startColForShift = COL;
                int _fakeCol = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dicShift.Add(dt.Rows[i]["ShiftSystemID"].ToString(), COL);

                    sheet1[xlsRow - 1, COL].Text = dt.Rows[i]["UserName"].ToString();
                    sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL + 3].Merge();
                    sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL + 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL + 3].BorderAround(ExcelLineStyle.Thin);
                    sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL + 3].CellStyle.Interior.ColorIndex = ExcelKnownColors.LightGreen;

                    SetHeadText("P", sheet1, xlsRow, ref COL, out _fakeCol, 6);
                    SetHeadText("A", sheet1, xlsRow, ref COL, out _fakeCol, 6);
                    SetHeadText("OFF", sheet1, xlsRow, ref COL, out _fakeCol, 6);
                    SetHeadText("LV", sheet1, xlsRow, ref COL, out _fakeCol, 6);


                }

                sheet1[xlsRow - 1, COL].Text = "TOTAL";
                sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL + 3].Merge();
                sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL + 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL + 3].BorderAround(ExcelLineStyle.Thin);
                sheet1.Range[xlsRow - 1, COL, xlsRow - 1, COL + 3].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;

                SetHeadText("P", sheet1, xlsRow, ref COL, out int ColTotalP, 6);
                SetHeadText("A", sheet1, xlsRow, ref COL, out int ColTotalA, 6);
                SetHeadText("OFF", sheet1, xlsRow, ref COL, out int ColTotalOff, 6);
                SetHeadText("LV", sheet1, xlsRow, ref COL, out int ColTotalLV, 6);
                SetHeadText("Remarks", sheet1, xlsRow, ref COL, out int ColRemarks, 12);

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
                string section = "";
                for (int i = 0; i < dsShiftSummary.Tables[0].Rows.Count; i++)
                {
                    if (groupid == dsShiftSummary.Tables[0].Rows[i]["Department"].ToString())
                        continue;

                    int RowDepartment = xlsRow;
                    section = "";
                    groupid = dsShiftSummary.Tables[0].Rows[i]["Department"].ToString();

                    sheet1[xlsRow, colDepartment].Text = dsShiftSummary.Tables[0].Rows[i]["Department"].ToString();

                    dsShiftSummary.Tables[0].DefaultView.RowFilter = "Department='" + dsShiftSummary.Tables[0].Rows[i]["Department"].ToString() + "'";


                    for (int J = 0; J < dsShiftSummary.Tables[0].DefaultView.Count; J++)
                    {
                        if (section == dsShiftSummary.Tables[0].DefaultView[J]["Section"].ToString())
                            continue;
                        section = dsShiftSummary.Tables[0].DefaultView[J]["Section"].ToString();

                        sheet1[xlsRow, colSec].Text = dsShiftSummary.Tables[0].DefaultView[J]["Section"].ToString();

                        dtShiftSummary.DefaultView.RowFilter = "Department='" + dsShiftSummary.Tables[0].Rows[i]["Department"].ToString() + "' AND Section='" + dsShiftSummary.Tables[0].DefaultView[J]["Section"].ToString() + "'";
                        var dd = "Department='" + dsShiftSummary.Tables[0].Rows[i]["Department"].ToString() + "' AND Section='" + dsShiftSummary.Tables[0].DefaultView[J]["Section"].ToString() + "'";
                        //if(dd.Contains("Department='HR/Personnel/Admin/Compliance' AND Section='HR'"))
                        //{
                        //    var dds = "";
                        //}
                        double P = 0, A = 0, Off = 0, LV = 0, Onroll = 0;
                        for (int D = 0; D < dtShiftSummary.DefaultView.Count; D++)
                        {
                            int CurrentShiftStartsAt = dicShift[dtShiftSummary.DefaultView[D]["SystemId"].ToString()];


                            P += clsStaticInfo.dbl(dtShiftSummary.DefaultView[D]["SUM_PRESENT"].ToString());
                            A += clsStaticInfo.dbl(dtShiftSummary.DefaultView[D]["SUM_Absent"].ToString());
                            Off += clsStaticInfo.dbl(dtShiftSummary.DefaultView[D]["SUM_OFF"].ToString());
                            LV += clsStaticInfo.dbl(dtShiftSummary.DefaultView[D]["SUM_Late"].ToString());

                            Onroll += clsStaticInfo.dbl(dtShiftSummary.DefaultView[D]["ONROLL"].ToString());

                            sheet1[xlsRow, CurrentShiftStartsAt].Number = clsStaticInfo.dbl(dtShiftSummary.DefaultView[D]["SUM_PRESENT"].ToString());
                            sheet1[xlsRow, CurrentShiftStartsAt + 1].Number = clsStaticInfo.dbl(dtShiftSummary.DefaultView[D]["SUM_Absent"].ToString());
                            sheet1[xlsRow, CurrentShiftStartsAt + 2].Number = clsStaticInfo.dbl(dtShiftSummary.DefaultView[D]["SUM_OFF"].ToString());
                            sheet1[xlsRow, CurrentShiftStartsAt + 3].Number = clsStaticInfo.dbl(dtShiftSummary.DefaultView[D]["SUM_Late"].ToString());
                            
                        }
                        sheet1[xlsRow, colDept_Strength].Number = Onroll;

                        sheet1[xlsRow, ColTotalA].Number = A;
                        sheet1[xlsRow, ColTotalLV].Number = LV;
                        sheet1[xlsRow, ColTotalOff].Number = Off;
                        sheet1[xlsRow, ColTotalP].Number = P;

                        //sheet1[xlsRow, colDept_Strength].Number = A + LV + Off + P;


                        xlsRow++;
                    }


                    //total for department
                    sheet1[xlsRow, ColTotalA].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColTotalA) + RowDepartment.ToString() + ":" + clsStaticInfo.GetxlsCol(ColTotalA) + (xlsRow - 1).ToString() + ")";
                    sheet1.Range[xlsRow, ColTotalA].CellStyle.Font.Bold = true;
                    sheet1[xlsRow, ColTotalLV].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColTotalLV) + RowDepartment.ToString() + ":" + clsStaticInfo.GetxlsCol(ColTotalLV) + (xlsRow - 1).ToString() + ")";
                    sheet1.Range[xlsRow, ColTotalLV].CellStyle.Font.Bold = true;
                    sheet1[xlsRow, ColTotalOff].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColTotalOff) + RowDepartment.ToString() + ":" + clsStaticInfo.GetxlsCol(ColTotalOff) + (xlsRow - 1).ToString() + ")";
                    sheet1.Range[xlsRow, ColTotalOff].CellStyle.Font.Bold = true;
                    sheet1[xlsRow, ColTotalP].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColTotalP) + RowDepartment.ToString() + ":" + clsStaticInfo.GetxlsCol(ColTotalP) + (xlsRow - 1).ToString() + ")";
                    sheet1.Range[xlsRow, ColTotalP].CellStyle.Font.Bold = true;

                    xlsRow++;
                }

                //grand total
                sheet1[xlsRow, colDepartment].Text = "Grand Total";
                sheet1.Range[xlsRow, colDepartment, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, colDepartment, xlsRow, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                for (int col = startColForShift; col < ColTotalP; col++)
                {
                    sheet1[xlsRow, col].Formula = "SUM(" + clsStaticInfo.GetxlsCol(col) + startRow.ToString() + ":" + clsStaticInfo.GetxlsCol(col) + (xlsRow - 1).ToString() + ")";

                }
                sheet1[xlsRow, colDept_Strength].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colDept_Strength) + startRow.ToString() + ":" + clsStaticInfo.GetxlsCol(colDept_Strength) + (xlsRow - 1).ToString() + ")";

                sheet1.Range[xlsRow, ColTotalP].Number =Convert.ToInt32( totalPresentDays.ToString());
                sheet1.Range[xlsRow, ColTotalP].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, ColTotalP].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet1.Range[xlsRow, ColTotalA].Number = Convert.ToInt32(totalAbsentDays.ToString());
                sheet1.Range[xlsRow, ColTotalA].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, ColTotalA].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet1.Range[xlsRow, ColTotalOff].Number = Convert.ToInt32(totalOFFDays.ToString());
                sheet1.Range[xlsRow, ColTotalOff].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, ColTotalOff].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet1.Range[xlsRow, ColTotalLV].Number = Convert.ToInt32(totalLeaveDays.ToString());
                sheet1.Range[xlsRow, ColTotalLV].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, ColTotalLV].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                sheet1.Range[xlsRow, ColTotalP, xlsRow, ColTotalP + 3].CellStyle.Font.Bold = true;

                //sheet1.Range[xlsRow, ColTotalP, xlsRow, ColTotalP + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet1.Range[xlsRow, ColTotalP, xlsRow, ColTotalP + 3].BorderAround(ExcelLineStyle.Hair);
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

                sheet1.Name = "Shift Summary";
                #endregion

                workbook.Version = ExcelVersion.Excel97to2003;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "Shift Summary.xls";
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
        #endregion--------------------------------------------Xls Report End----------------------------------------------------

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

        private void CreateDynamicSHead(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out List<SalaryHeadSequence> list)
        {
            try
            {
                list = new List<SalaryHeadSequence>();
                _total_head_count = 0;
                _count_earning_head = 0;
                _count_deducting_head = 0;
                _count_earning_ctchead = 0;
                int countGrossPostion = 0;
                string deductionFormula = "";

                xlsCol += 1;
                countGrossPostion++;

                //salaryHSGross.SalaryHeadId = "Gross";

                int countCTCPosition = countGrossPostion;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region loop ctc
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {


                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "Net Payable".ToUpper())
                        {
                            _total_head_count++;


                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();

                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.FontName = "Arial Narrow";
                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Size = 10;
                            //sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition, xlsRow + 1, ColGrs + countCTCPosition + 1].Merge();
                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.ShrinkToFit = true;


                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                            {
                                sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
                            }
                            xlsCol += 2;
                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                            salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;


                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();

                            salaryHeadSequence.Sequence = ci;
                            salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;



                            list.Add(salaryHeadSequence);
                            countCTCPosition++;
                        }


                    }//SalaryHead 
                    #endregion
                }//for
                xlsCol += 1;


                _count_earning_ctchead = countCTCPosition - 1;

                int countDeductionPosition = countCTCPosition - 1;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region deduction
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
                        //{
                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "D")
                        {
                            _total_head_count++;
                            countDeductionPosition++;

                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Size = 10;
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.FontName = "Arial Narrow";
                            //sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition, xlsRow + 1, ColGrs + countDeductionPosition + 1].Merge();
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.ShrinkToFit = true;


                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                            {
                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
                            }
                            xlsCol += 2;
                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
                            if (deductionFormula.Length == 0)
                            {
                                deductionFormula += salaryHeadSequence.XLColIndex.ToString();
                            }
                            else
                            {
                                deductionFormula += "," + salaryHeadSequence.XLColIndex.ToString();
                            }

                            //countDeductionPosition++;

                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();

                            salaryHeadSequence.Sequence = ci;
                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;

                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();


                            list.Add(salaryHeadSequence);

                            _count_deducting_head++;
                        }
                        //}//CTC/Gross
                    }//SalaryHead 
                    #endregion
                }//for

            }
            catch (Exception ex)
            {
                throw ex;
            }
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
    }
}
