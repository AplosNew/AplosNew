using Library.Core;
using Library.Data.Sql;
using Library.Model.External;
using Library.Service.Helpers;
using Syncfusion.XlsIO;
using System;
using System.Collections.Specialized;
using System.Data;

namespace Library.Service.External
{
    public class ReportEmployeeInfo
    {
        private readonly ISqlRepository _sqlRepository;

        public ReportEmployeeInfo(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }

        #region Overall Status

        public IWorkbook EmployeeInfo(ExcelEngine excelEngine, ReportParam param)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            IWorksheet sheet2 = null;
            IWorksheet sheet3 = null;
            try
            {
                oRU = new ReportUtility();
                DataSet dsLocal = GetEmployeeInfo(param);
                DataSet dsDetail = Activity_Document_KPI(param);
                workbook = oRU.GetWorkbook(ref excelEngine, 3);
                sheet1 = workbook.Worksheets[0];
                sheet2 = workbook.Worksheets[1];
                sheet3 = workbook.Worksheets[2];

                CS_EMP(ref sheet1, oRU, "Overall Status", "Overall Status", dsLocal, param);
                CS_ActivityDetail(ref sheet2, oRU, "Activity Document KPI", "Activity Document KPI", dsLocal, dsDetail, param);
                CS_RawActivityDetail(ref sheet3, oRU, "Raw Activity Document KPI", "Raw Activity Document KPI", dsDetail, param);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CS_RawActivityDetail(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dsDetail, ReportParam status)
        {
            string _Currency = string.Empty;
            string _CurrencyId = string.Empty;
            DataView dvVoucher = null;
            DataTable dtVoucher = null;

            var xlsRow = 1;
            var xlsCol = 1;
            var startXlsRow = 1;
            var shet2EndxlsCol = 1;
            try
            {
                #region Data

                dvVoucher = new DataView(dsDetail.Tables[0])
                {
                    Sort = "Name,Activity,Document,KPI"
                };
                dtVoucher = dvVoucher.ToTable();
                if (dtVoucher.Rows.Count == 0)
                {
                    throw (new Exception("No Employee Found !!!"));
                }

                #endregion Data

                #region Sheet2 Data                xlsRow = 5;

                var ExtraRow = 1;
                startXlsRow = xlsRow;
                xlsRow += ExtraRow;//Header

                #region Detail Header

                xlsCol = 1;

                var cSR = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "SR", 7);
                xlsCol = xlsCol + 1;

                var cId = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Id");
                xlsCol = xlsCol + 1;

                var cCode = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Code");
                xlsCol = xlsCol + 1;

                var cName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Name");
                xlsCol = xlsCol + 1;

                var cCompanyName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "CompanyName");
                xlsCol = xlsCol + 1;

                var caName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Activity Name", 40);
                xlsCol = xlsCol + 1;

                var caDetail = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Activity Detail", 50);
                xlsCol = xlsCol + 1;

                var cf = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Frequency", 8);
                xlsCol = xlsCol + 1;

                var cp = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Period", 12);
                xlsCol = xlsCol + 1;

                var cat = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Avg Time (Min)", 12);
                xlsCol = xlsCol + 1;

                var cdoc = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Document", 30);
                xlsCol = xlsCol + 1;

                var cdocD = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "File Name", 20);
                xlsCol = xlsCol + 1;

                var ck = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, nameof(KPI), 15);
                xlsCol = xlsCol + 1;

                var ckd = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "KPI Detail", 20);
                //xlsCol = xlsCol + 1;

                shet2EndxlsCol = xlsCol;

                string IsWithActivity = "";
                if (status.withoutactivity)
                {
                    IsWithActivity = "Employees Without Activity";
                }
                else
                {
                    IsWithActivity = "Employees With Activity";
                }

                oRU.SetHeaderText(ref sheet, startXlsRow - 1, cSR, IsWithActivity);
                sheet.Range[startXlsRow - 1, cSR].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[oRU.GetColumnNameForXls(cSR) + (startXlsRow - 1) + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + (startXlsRow - 1)].Merge();

                xlsRow += 1;

                #endregion Detail Header

                //xlsRow += 1;
                var Row_Total_Start = xlsRow;
                var _index = 0;
                //StringCollection scEmp = new StringCollection();
                for (int a = 0; a < dtVoucher.Rows.Count; a++)
                {
                    _index += 1;

                    #region body

                    #region Activity

                    string empid = dtVoucher.Rows[a]["Id"].ToString();
                    //DataView dvdk = new DataView(dsDetail.Tables[0])
                    //{
                    //    RowFilter = "Id='" + empid + "'",
                    //    Sort = "Activity"
                    //};
                    //DataTable dtdk = dvdk.ToTable(true, "ActivityId", "Activity", "ActivityDetail", "Frequency", "Period", "AverageTime",
                    //    "Document", "FileName", "KPIDetail", "KPI");
                    var Start_Row = xlsRow;
                    //if (dtdk.Rows.Count > 0)
                    //{
                    //    for (int a = 0; a < dtdk.Rows.Count; a++)
                    //    {
                    oRU.SetText(ref sheet, xlsRow, cSR, _index);
                    oRU.SetText(ref sheet, xlsRow, cId, dtVoucher.Rows[a]["Id"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cCode, dtVoucher.Rows[a]["Code"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cName, dtVoucher.Rows[a]["Name"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cCompanyName, dtVoucher.Rows[a]["CompanyName"].ToString());

                    string actid = dtVoucher.Rows[a]["ActivityId"].ToString();
                    oRU.SetText(ref sheet, xlsRow, caName, dtVoucher.Rows[a]["Activity"].ToString());
                    oRU.SetText(ref sheet, xlsRow, caDetail, dtVoucher.Rows[a]["ActivityDetail"].ToString());
                    //int number = 0;
                    //int.TryParse(dtdk.Rows[a]["Frequency"].ToString(),out number);
                    oRU.SetText(ref sheet, xlsRow, cf, dtVoucher.Rows[a]["Frequency"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cp, dtVoucher.Rows[a]["Period"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cat, dtVoucher.Rows[a]["AverageTime"].ToString());

                    oRU.SetText(ref sheet, xlsRow, cdoc, dtVoucher.Rows[a]["Document"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cdocD, dtVoucher.Rows[a]["FileName"].ToString());
                    oRU.SetText(ref sheet, xlsRow, ck, dtVoucher.Rows[a][nameof(KPI)].ToString());
                    oRU.SetText(ref sheet, xlsRow, ckd, dtVoucher.Rows[a]["KPIDetail"].ToString());

                    xlsRow += 1;
                    //    }//for a
                    //}
                    //else
                    //{
                    //    xlsRow += 1;
                    //}

                    //merge
                    //int _end_row = Start_Row;
                    //if (xlsRow > Start_Row)
                    //{
                    //    //_index += 1;
                    //    _end_row = xlsRow - 1;
                    //}

                    #endregion Activity

                    #endregion body
                }

                //border
                //sheet2.Range[(Row_Total_Start), 1, xlsRow, shet2EndxlsCol].BorderAround(ExcelLineStyle.Thin);
                sheet.Range[(Row_Total_Start), 1, xlsRow, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                xlsRow = xlsRow + 6;

                #endregion Sheet2 Data                xlsRow = 5;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                //oRU.CompanyGroupHeaderPhoenix(ref sheet, shet2EndxlsCol, SheetHeader, status.CompanyGroupId);
                oRU.CompanyGroupHeaderPhoenix(ref sheet, shet2EndxlsCol, SheetHeader, status.CompanyGroupId);
                oRU.FreezePage(ref sheet, 1, startXlsRow + ExtraRow + 1);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait, status.EmployeeName);
                sheet.Name = SheetName;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void XCS_ActivityDetail(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dslocal, DataSet dsDetail, ReportParam status)
        {
            string _Currency = string.Empty;
            string _CurrencyId = string.Empty;
            DataView dvVoucher = null;
            DataTable dtVoucher = null;

            var xlsRow = 1;
            var xlsCol = 1;
            var startXlsRow = 1;
            var shet2EndxlsCol = 1;
            try
            {
                #region Data

                dvVoucher = new DataView(dslocal.Tables[0])
                {
                    Sort = "Name"
                };
                dtVoucher = dvVoucher.ToTable();
                if (dtVoucher.Rows.Count == 0)
                {
                    throw (new Exception("No Employee Found !!!"));
                }

                #endregion Data

                #region Sheet2 Data                xlsRow = 5;

                var ExtraRow = 1;
                startXlsRow = xlsRow;
                xlsRow += ExtraRow;//Header

                #region Detail Header

                xlsCol = 1;

                var cSR = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "SR", 7);
                xlsCol = xlsCol + 1;

                var cId = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Id");
                xlsCol = xlsCol + 1;

                var cCode = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Code");
                xlsCol = xlsCol + 1;

                var cName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Name");
                xlsCol = xlsCol + 1;

                var cCompanyName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "CompanyName");
                xlsCol = xlsCol + 1;

                var caName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Activity Name", 40);
                xlsCol = xlsCol + 1;

                var caDetail = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Activity Detail", 50);
                xlsCol = xlsCol + 1;

                var cf = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Frequency", 8);
                xlsCol = xlsCol + 1;

                var cp = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Period", 12);
                xlsCol = xlsCol + 1;

                var cat = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Avg Time (Min)", 12);
                xlsCol = xlsCol + 1;

                var cdoc = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Document", 30);
                xlsCol = xlsCol + 1;

                var cdocD = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "File Name", 20);
                xlsCol = xlsCol + 1;

                var ck = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, nameof(KPI), 15);
                xlsCol = xlsCol + 1;

                var ckd = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "KPI Detail", 20);
                //xlsCol = xlsCol + 1;

                shet2EndxlsCol = xlsCol;

                string IsWithActivity = "";
                if (status.withoutactivity)
                {
                    IsWithActivity = "Employees Without Activity";
                }
                else
                {
                    IsWithActivity = "Employees With Activity";
                }

                oRU.SetHeaderText(ref sheet, startXlsRow - 1, cSR, IsWithActivity);
                sheet.Range[startXlsRow - 1, cSR].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[oRU.GetColumnNameForXls(cSR) + (startXlsRow - 1) + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + (startXlsRow - 1)].Merge();

                xlsRow += 1;

                #endregion Detail Header

                //xlsRow += 1;
                var Row_Total_Start = xlsRow;
                var _index = 0;
                for (int icount = 0; icount < dtVoucher.Rows.Count; icount++)
                {
                    _index += 1;

                    #region body

                    oRU.SetText(ref sheet, xlsRow, cSR, _index);
                    oRU.SetText(ref sheet, xlsRow, cId, dtVoucher.Rows[icount]["Id"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cCode, dtVoucher.Rows[icount]["Code"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cName, dtVoucher.Rows[icount]["Name"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cCompanyName, dtVoucher.Rows[icount]["CompanyName"].ToString());

                    #region Activity

                    string empid = dtVoucher.Rows[icount]["Id"].ToString();
                    DataView dvdk = new DataView(dsDetail.Tables[0])
                    {
                        RowFilter = "Id='" + empid + "'",
                        Sort = "Activity"
                    };
                    DataTable dtdk = dvdk.ToTable(true, "ActivityId", "Activity", "ActivityDetail", "Frequency", "Period", "AverageTime");
                    var Start_Row = xlsRow;
                    if (dtdk.Rows.Count > 0)
                    {
                        for (int a = 0; a < dtdk.Rows.Count; a++)
                        {
                            string actid = dtdk.Rows[a]["ActivityId"].ToString();
                            oRU.SetText(ref sheet, xlsRow, caName, dtdk.Rows[a]["Activity"].ToString());
                            oRU.SetText(ref sheet, xlsRow, caDetail, dtdk.Rows[a]["ActivityDetail"].ToString());
                            //int number = 0;
                            //int.TryParse(dtdk.Rows[a]["Frequency"].ToString(),out number);
                            oRU.SetText(ref sheet, xlsRow, cf, dtdk.Rows[a]["Frequency"].ToString());
                            oRU.SetText(ref sheet, xlsRow, cp, dtdk.Rows[a]["Period"].ToString());
                            oRU.SetText(ref sheet, xlsRow, cat, dtdk.Rows[a]["AverageTime"].ToString());

                            DocKpi(dsDetail, empid, actid, ref xlsRow, ref sheet, caName, cdoc, ck, oRU);

                            //xlsRow += 1;
                        }
                    }
                    else
                    {
                        xlsRow += 1;
                    }

                    //merge
                    var _end_row = Start_Row;
                    if (xlsRow > Start_Row)
                    {
                        //_index += 1;
                        _end_row = xlsRow - 1;
                    }

                    #endregion Activity

                    sheet.Range[oRU.GetColumnNameForXls(cSR) + (Start_Row) + ":" + oRU.GetColumnNameForXls(cSR) + _end_row].Merge();
                    sheet.Range[oRU.GetColumnNameForXls(cId) + (Start_Row) + ":" + oRU.GetColumnNameForXls(cId) + _end_row].Merge();
                    sheet.Range[oRU.GetColumnNameForXls(cCode) + (Start_Row) + ":" + oRU.GetColumnNameForXls(cCode) + _end_row].Merge();
                    sheet.Range[oRU.GetColumnNameForXls(cName) + (Start_Row) + ":" + oRU.GetColumnNameForXls(cName) + _end_row].Merge();
                    sheet.Range[oRU.GetColumnNameForXls(cCompanyName) + (Start_Row) + ":" + oRU.GetColumnNameForXls(cCompanyName) + _end_row].Merge();

                    #endregion body
                }

                //oRU.SetText(ref sheet, xlsRow, cact - 1, "Total: ");
                //string Formula_Act = "=sum(" + oRU.GetColumnNameForXls(cact) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(cact) + (xlsRow - 1) + ")";
                //oRU.SetFormula(ref sheet, xlsRow, cact, Formula_Act, false);

                //string Formula_Doc = "=sum(" + oRU.GetColumnNameForXls(cdoc) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(cdoc) + (xlsRow - 1) + ")";
                //oRU.SetFormula(ref sheet, xlsRow, cdoc, Formula_Doc, false);

                //string Formula_Kpi = "=sum(" + oRU.GetColumnNameForXls(ckpi) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(ckpi) + (xlsRow - 1) + ")";
                //oRU.SetFormula(ref sheet, xlsRow, ckpi, Formula_Kpi, false);

                //border
                //sheet2.Range[(Row_Total_Start), 1, xlsRow, shet2EndxlsCol].BorderAround(ExcelLineStyle.Thin);
                sheet.Range[(Row_Total_Start), 1, xlsRow, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                xlsRow = xlsRow + 6;

                #endregion Sheet2 Data                xlsRow = 5;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, SheetHeader, status.CompanyGroupId);
                oRU.FreezePage(ref sheet, 1, startXlsRow + ExtraRow + 1);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait, status.EmployeeName);
                sheet.Name = SheetName;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DocKpi(DataSet dsDetail, string empid, string actid, ref int xlsRow, ref IWorksheet sheet, int ca, int cd, int ck, ReportUtility oRU)
        {
            try
            {
                DataView dvdc = new DataView(dsDetail.Tables[0])
                {
                    RowFilter = "Id='" + empid + "' and ActivityId='" + actid + "'",
                    Sort = "Document"
                };
                DataTable dtdk = dvdc.ToTable(true, "ActivityId", "DocumentId", "Document", "FileName", "KPIId", nameof(KPI), "KPIDetail");

                var Start_Row = xlsRow;
                if (dtdk.Rows.Count > 0)
                {
                    for (int a = 0; a < dtdk.Rows.Count; a++)
                    {
                        oRU.SetText(ref sheet, xlsRow, cd, dtdk.Rows[a]["Document"].ToString());
                        oRU.SetText(ref sheet, xlsRow, cd + 1, dtdk.Rows[a]["FileName"].ToString());
                        oRU.SetText(ref sheet, xlsRow, ck, dtdk.Rows[a][nameof(KPI)].ToString());
                        oRU.SetText(ref sheet, xlsRow, ck + 1, dtdk.Rows[a]["KPIDetail"].ToString());
                        xlsRow += 1;
                    }
                }
                else
                {
                    xlsRow += 1;
                }
                //merge
                var _end_row = Start_Row;
                if (xlsRow > Start_Row)
                {
                    _end_row = xlsRow - 1;
                }
                sheet.Range[oRU.GetColumnNameForXls(ca) + (Start_Row) + ":" + oRU.GetColumnNameForXls(ca) + _end_row].Merge();
                sheet.Range[oRU.GetColumnNameForXls(ca + 1) + (Start_Row) + ":" + oRU.GetColumnNameForXls(ca + 1) + _end_row].Merge();
                sheet.Range[oRU.GetColumnNameForXls(ca + 2) + (Start_Row) + ":" + oRU.GetColumnNameForXls(ca + 2) + _end_row].Merge();
                sheet.Range[oRU.GetColumnNameForXls(ca + 3) + (Start_Row) + ":" + oRU.GetColumnNameForXls(ca + 3) + _end_row].Merge();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CS_EMP(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dslocal, ReportParam status)
        {
            string _Currency = string.Empty;
            string _CurrencyId = string.Empty;
            DataView dvVoucher = null;
            DataTable dtVoucher = null;

            var xlsRow = 1;
            var xlsCol = 1;
            var startXlsRow = 1;
            var shet2EndxlsCol = 1;
            try
            {
                #region Data

                DataSet dsCol = GetColumnInfo(status.CompanyGroupId);
                DataView dvCol = new DataView(dsCol.Tables[0])
                {
                    Sort = "Sequence"
                };
                DataTable dtCol = dvCol.ToTable();
                // DataSet dslocal = GetVoucherInfo(masterid);
                dvVoucher = new DataView(dslocal.Tables[0])
                {
                    Sort = "Name"
                };
                dtVoucher = dvVoucher.ToTable();
                if (dtVoucher.Rows.Count == 0)
                {
                    throw (new Exception("No Employee Found !!!"));
                }

                #endregion Data

                #region Sheet2 Data                xlsRow = 5;

                var ExtraRow = 1;
                startXlsRow = xlsRow;
                xlsRow += ExtraRow;//Header

                #region Detail Header

                xlsCol = 1;

                var cSR = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "SR", 7);
                xlsCol = xlsCol + 1;

                var cId = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Id");
                // sheet2.Range[GetColumnNameForXls(xlsCol) + (xlsRow - 1) + ":" + GetColumnNameForXls(xlsCol) + xlsRow].Merge();
                xlsCol = xlsCol + 1;

                var cCode = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Code");
                xlsCol = xlsCol + 1;

                var cName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Name");
                xlsCol = xlsCol + 1;

                var cCompanyName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "CompanyName");
                xlsCol = xlsCol + 1;

                for (int c = 0; c < dtCol.Rows.Count; c++)
                {
                    string ClinetColumnName = dtCol.Rows[c]["AplosColumnName"].ToString();
                    oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, ClinetColumnName, 20);
                    xlsCol = xlsCol + 1;
                }

                var cDob = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Date of Birth");
                xlsCol = xlsCol + 1;

                var cact = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Activity", ExcelHAlign.HAlignRight);
                xlsCol = xlsCol + 1;

                var cdoc = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Doc", ExcelHAlign.HAlignRight);
                xlsCol = xlsCol + 1;

                var ckpi = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, nameof(KPI), ExcelHAlign.HAlignRight);
                xlsCol = xlsCol + 1;

                var cStatus = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Status");
                xlsCol = xlsCol + 1;

                var crCode = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Code");
                xlsCol = xlsCol + 1;

                var crName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Name");

                oRU.SetHeaderText(ref sheet, xlsRow - 1, crCode, "Reporting Person", ExcelHAlign.HAlignCenter);
                sheet.Range[oRU.GetColumnNameForXls(crCode) + (xlsRow - 1) + ":" + oRU.GetColumnNameForXls(crName) + (xlsRow - 1)].Merge();
                //xlsCol = xlsCol + 1;

                //int cSubmitted = xlsCol;
                //oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Submitted");

                shet2EndxlsCol = xlsCol;

                string IsWithActivity = "";
                if (status.withoutactivity)
                {
                    IsWithActivity = "Employees Without Activity";
                }
                else
                {
                    IsWithActivity = "Employees With Activity";
                }

                oRU.SetHeaderText(ref sheet, startXlsRow - 1, cSR, IsWithActivity);
                sheet.Range[startXlsRow - 1, cSR].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[oRU.GetColumnNameForXls(cSR) + (startXlsRow - 1) + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + (startXlsRow - 1)].Merge();
                //sheet.Range["A2" + ":" + GetColumnNameForXls(lastcol) + "2"].Merge();

                xlsRow += 1;
                //sheet2.Range[(xlsRow - 2), cACCode, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);

                #endregion Detail Header

                //xlsRow += 1;
                var Row_Total_Start = xlsRow;
                var _index = 0;
                for (int icount = 0; icount < dtVoucher.Rows.Count; icount++)
                {
                    _index += 1;

                    #region body

                    oRU.SetText(ref sheet, xlsRow, cSR, _index);
                    oRU.SetText(ref sheet, xlsRow, cId, dtVoucher.Rows[icount]["Id"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cCode, dtVoucher.Rows[icount]["Code"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cName, dtVoucher.Rows[icount]["Name"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cCompanyName, dtVoucher.Rows[icount]["CompanyName"].ToString());

                    var col = cCompanyName;
                    for (int c = 0; c < dtCol.Rows.Count; c++)
                    {
                        string val = dtCol.Rows[c]["ColumnName"].ToString();
                        col += 1;
                        oRU.SetText(ref sheet, xlsRow, col, dtVoucher.Rows[icount][val].ToString());
                    }
                    oRU.SetText(ref sheet, xlsRow, cDob, dtVoucher.Rows[icount]["DOB2"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cact, Convert.ToInt32(dtVoucher.Rows[icount]["act"].ToString()));
                    oRU.SetText(ref sheet, xlsRow, cdoc, Convert.ToInt32(dtVoucher.Rows[icount]["doc"].ToString()));
                    oRU.SetText(ref sheet, xlsRow, ckpi, Convert.ToInt32(dtVoucher.Rows[icount]["kpi"].ToString()));
                    oRU.SetText(ref sheet, xlsRow, cStatus, dtVoucher.Rows[icount]["Status"].ToString());
                    oRU.SetText(ref sheet, xlsRow, crCode, dtVoucher.Rows[icount]["RCode"].ToString());
                    oRU.SetText(ref sheet, xlsRow, crName, dtVoucher.Rows[icount]["RName"].ToString());
                    //oRU.SetText(ref sheet, xlsRow, cSubmitted, dtVoucher.Rows[icount]["Submitted"].ToString());
                    xlsRow += 1;

                    #endregion body
                }

                oRU.SetText(ref sheet, xlsRow, cact - 1, "Total: ");
                string Formula_Act = "=sum(" + oRU.GetColumnNameForXls(cact) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(cact) + (xlsRow - 1) + ")";
                oRU.SetFormula(ref sheet, xlsRow, cact, Formula_Act, false);

                string Formula_Doc = "=sum(" + oRU.GetColumnNameForXls(cdoc) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(cdoc) + (xlsRow - 1) + ")";
                oRU.SetFormula(ref sheet, xlsRow, cdoc, Formula_Doc, false);

                string Formula_Kpi = "=sum(" + oRU.GetColumnNameForXls(ckpi) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(ckpi) + (xlsRow - 1) + ")";
                oRU.SetFormula(ref sheet, xlsRow, ckpi, Formula_Kpi, false);

                //border
                //sheet2.Range[(Row_Total_Start), 1, xlsRow, shet2EndxlsCol].BorderAround(ExcelLineStyle.Thin);
                sheet.Range[(Row_Total_Start), 1, xlsRow, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                xlsRow = xlsRow + 6;

                #endregion Sheet2 Data                xlsRow = 5;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                //oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, SheetHeader, status.CompanyGroupId);
                oRU.CompanyGroupHeaderPhoenix(ref sheet, shet2EndxlsCol, SheetHeader, status.CompanyGroupId);
                oRU.FreezePage(ref sheet, 1, startXlsRow + ExtraRow + 1);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait, status.EmployeeName);
                sheet.Name = SheetName;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CS_ActivityDetail(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dslocal, DataSet dsDetail, ReportParam status)
        {
            string _Currency = string.Empty;
            string _CurrencyId = string.Empty;
            DataView dvVoucher = null;
            DataTable dtVoucher = null;

            var xlsRow = 1;
            var xlsCol = 1;
            var startXlsRow = 1;
            var shet2EndxlsCol = 1;
            try
            {
                #region Data

                dvVoucher = new DataView(dslocal.Tables[0])
                {
                    Sort = "Name"
                };
                dtVoucher = dvVoucher.ToTable();
                if (dtVoucher.Rows.Count == 0)
                {
                    throw (new Exception("No Employee Found !!!"));
                }

                #endregion Data

                #region Sheet2 Data                xlsRow = 5;

                var ExtraRow = 1;
                startXlsRow = xlsRow;
                xlsRow += ExtraRow;//Header

                #region Detail Header

                xlsCol = 1;

                var cSR = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "SR", 7);
                xlsCol = xlsCol + 1;

                var cId = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Id");
                xlsCol = xlsCol + 1;

                var cCode = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Code");
                xlsCol = xlsCol + 1;

                var cName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Name");
                xlsCol = xlsCol + 1;

                var cCompanyName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "CompanyName");
                xlsCol = xlsCol + 1;

                var caName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Activity Name", 40);
                xlsCol = xlsCol + 1;

                var caDetail = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Activity Detail", 50);
                xlsCol = xlsCol + 1;

                var cf = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Frequency", 8);
                xlsCol = xlsCol + 1;

                var cp = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Period", 12);
                xlsCol = xlsCol + 1;

                var cat = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Avg Time (Min)", 12);
                xlsCol = xlsCol + 1;

                var cdoc = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Document", 30);
                xlsCol = xlsCol + 1;

                var cdocD = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "File Name", 20);
                xlsCol = xlsCol + 1;

                var ck = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, nameof(KPI), 15);
                xlsCol = xlsCol + 1;

                var ckd = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "KPI Detail", 20);
                //xlsCol = xlsCol + 1;

                shet2EndxlsCol = xlsCol;

                string IsWithActivity = "";
                if (status.withoutactivity)
                {
                    IsWithActivity = "Employees Without Activity";
                }
                else
                {
                    IsWithActivity = "Employees With Activity";
                }

                oRU.SetHeaderText(ref sheet, startXlsRow - 1, cSR, IsWithActivity);
                sheet.Range[startXlsRow - 1, cSR].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[oRU.GetColumnNameForXls(cSR) + (startXlsRow - 1) + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + (startXlsRow - 1)].Merge();

                xlsRow += 1;

                #endregion Detail Header

                //xlsRow += 1;
                var Row_Total_Start = xlsRow;
                var _index = 0;
                for (int icount = 0; icount < dtVoucher.Rows.Count; icount++)
                {
                    _index += 1;

                    #region body

                    oRU.SetText(ref sheet, xlsRow, cSR, _index);
                    oRU.SetText(ref sheet, xlsRow, cId, dtVoucher.Rows[icount]["Id"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cCode, dtVoucher.Rows[icount]["Code"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cName, dtVoucher.Rows[icount]["Name"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cCompanyName, dtVoucher.Rows[icount]["CompanyName"].ToString());

                    #region Activity

                    string empid = dtVoucher.Rows[icount]["Id"].ToString();
                    DataView dvdk = new DataView(dsDetail.Tables[0])
                    {
                        RowFilter = "Id='" + empid + "'",
                        Sort = "Activity,Document,KPI"
                    };
                    DataTable dtdk = dvdk.ToTable();
                    var Start_Row = xlsRow;
                    StringCollection scActivity = new StringCollection();
                    StringCollection scDocuments = new StringCollection();
                    StringCollection scKpi = new StringCollection();
                    if (dtdk.Rows.Count > 0)
                    {
                        for (int a = 0; a < dtdk.Rows.Count; a++)
                        {
                            //Activity
                            string actid = dtdk.Rows[a]["ActivityId"].ToString();
                            if (!scActivity.Contains(actid))
                            {
                                scActivity.Add(actid);
                                scDocuments.Clear();
                                scKpi.Clear();
                                oRU.SetText(ref sheet, xlsRow, caName, dtdk.Rows[a]["Activity"].ToString());
                                oRU.SetText(ref sheet, xlsRow, caDetail, dtdk.Rows[a]["ActivityDetail"].ToString());
                                oRU.SetText(ref sheet, xlsRow, cf, dtdk.Rows[a]["Frequency"].ToString());
                                oRU.SetText(ref sheet, xlsRow, cp, dtdk.Rows[a]["Period"].ToString());
                                oRU.SetText(ref sheet, xlsRow, cat, dtdk.Rows[a]["AverageTime"].ToString());
                            }
                            //Document
                            string _DocumentId = dtdk.Rows[a]["DocumentId"].ToString();
                            if (!scDocuments.Contains(_DocumentId))
                            {
                                scDocuments.Add(_DocumentId);
                                oRU.SetText(ref sheet, xlsRow, cdoc, dtdk.Rows[a]["Document"].ToString());
                                oRU.SetText(ref sheet, xlsRow, cdoc + 1, dtdk.Rows[a]["FileName"].ToString());
                            }
                            //KPI
                            string _KPIId = dtdk.Rows[a]["KPIId"].ToString();
                            if (!scKpi.Contains(_KPIId))
                            {
                                scKpi.Add(_KPIId);
                                oRU.SetText(ref sheet, xlsRow, ck, dtdk.Rows[a][nameof(KPI)].ToString());
                                oRU.SetText(ref sheet, xlsRow, ck + 1, dtdk.Rows[a]["KPIDetail"].ToString());
                            }
                            // DocKpi(dsDetail, empid, actid, ref xlsRow, ref sheet, caName, cdoc, ck, oRU);

                            xlsRow += 1;
                        }
                    }
                    else
                    {
                        xlsRow += 1;
                    }

                    //merge
                    var _end_row = Start_Row;
                    if (xlsRow > Start_Row)
                    {
                        //_index += 1;
                        _end_row = xlsRow - 1;
                    }

                    #endregion Activity

                    sheet.Range[oRU.GetColumnNameForXls(cSR) + (Start_Row) + ":" + oRU.GetColumnNameForXls(cSR) + _end_row].Merge();
                    sheet.Range[oRU.GetColumnNameForXls(cId) + (Start_Row) + ":" + oRU.GetColumnNameForXls(cId) + _end_row].Merge();
                    sheet.Range[oRU.GetColumnNameForXls(cCode) + (Start_Row) + ":" + oRU.GetColumnNameForXls(cCode) + _end_row].Merge();
                    sheet.Range[oRU.GetColumnNameForXls(cName) + (Start_Row) + ":" + oRU.GetColumnNameForXls(cName) + _end_row].Merge();
                    sheet.Range[oRU.GetColumnNameForXls(cCompanyName) + (Start_Row) + ":" + oRU.GetColumnNameForXls(cCompanyName) + _end_row].Merge();

                    #endregion body
                }

                //border
                //sheet2.Range[(Row_Total_Start), 1, xlsRow, shet2EndxlsCol].BorderAround(ExcelLineStyle.Thin);
                sheet.Range[(Row_Total_Start), 1, xlsRow, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                xlsRow = xlsRow + 6;

                #endregion Sheet2 Data                xlsRow = 5;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                //oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, SheetHeader, status.CompanyGroupId);
                oRU.CompanyGroupHeaderPhoenix(ref sheet, shet2EndxlsCol, SheetHeader, status.CompanyGroupId);
                oRU.FreezePage(ref sheet, 1, startXlsRow + ExtraRow + 1);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait, status.EmployeeName);
                sheet.Name = SheetName;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetColumnInfo(string CompanyGroupId)
        {
            GridParameter parameters = null;
            //string wc = "";
            //string wc_count = "";
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @" select
                                        t.AplosEmpFieldId,
                                        t.ColumnName
                                        ,t.ClinetColumnName
                                        ,f.AplosColumnName,t.Sequence
                                        from
                                        [dbo].[AplosEmpFieldTag] t
                                        left outer join [AplosEmpField] f on f.Id=t.AplosEmpFieldId
                                        where t.CompanyGroupId='" + CompanyGroupId + @"' and t.IsAplicable=1
                                       ";

                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet Activity_Document_KPI(ReportParam param)
        {
            GridParameter parameters = null;
            //string wc = "";
            //string wc_count = "";
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = string.Empty;

                parameters.CmdText = "Select emp.Id eId,emp.CompanyName,act.* from (" + SQLEmpStatus(param) + @") emp
                            left outer join
                                                ( select
                                            e.Id,isnull(s.Name,'')+e.Name Name,e.Code,e.FatherName,e.MotherName
                                             ,Replace(CONVERT(VARCHAR(11), e.DOB, 106), ' ', '-') DOB
                                             ,Replace(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-') DOJ
                                            ,c.Name Company
                                            ,a.Id ActivityId,isnull(a.Frequency,0) Frequency,p.Name [Period]
                                            ,a.Name Activity,a.ActivityDetail, a.AverageTime
                                            ,d.Id DocumentId
                                            ,d.Name Document,d.FileName
                                            ,k.Id KPIId
                                            ,k.Name KPI,k.KPIDetail
                                            ,r.Name ReportingPerson
                                            from [dbo].[Employee] e
                                            left outer join [dbo].[ActivityEmp] a on a.EmployeeId=e.Id
                                            left outer join [period] p on p.Id=a.PeriodId
                                            left outer join [dbo].[DocumentActivity] d on d.ActivityId = a.Id
                                            left outer join [dbo].[KPI] k on k.ActivityId=a.Id
                                            left outer join [dbo].[Company] c on c.Id=e.CompanyId
                                            left outer join Salutation s on s.Id=e.Id
                                            left outer join [dbo].[Employee] r on r.Id=e.ReportingOfficerId
                                        where c.CompanyGroupId='" + param.CompanyGroupId + @"'
                                                ) act on emp.Id=act.Id
                                       ";

                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetEmployeeInfo(ReportParam status)
        {
            GridParameter parameters = null;
            // var identity = null;

            try
            {
                parameters = new GridParameter();
                parameters.CmdText = string.Empty;
                //         if (status.notloggedin)
                //         {
                //             wc = "  status='Not Loggedin'";
                //         }
                //         //else
                //         //{
                //         //    wc = " where IsFirstLogin=1";
                //         //}

                //         if (status.Submitted)
                //         {
                //             if (wc.Length > 0)
                //             {
                //                 wc += " or status='Submitted'";
                //             }
                //             else
                //             {
                //                 wc = " status='Submitted'";
                //             }
                //         }
                //         //else
                //         //{
                //         //    wc += " or Submit=0";
                //         //}

                //         if (status.NotSubmitted)
                //         {
                //             //wc += " or status='Not Submitted' ";
                //             if (wc.Length > 0)
                //             {
                //                 wc += " or status='Not Submitted'";
                //             }
                //             else
                //             {
                //                 wc = " status='Not Submitted'";
                //             }
                //         }
                //         //else
                //         //{
                //         //    wc += " or Submit=1 ";
                //         //}

                //         if (wc.Length > 0)
                //         {
                //             wc = " and (" + wc + ") ";
                //         }

                //         if (status.withoutactivity)
                //         {
                //             wc_count = " and isnull(act.c,0)=0 ";
                //         }

                // identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                //         string _sql = @" select
                //                     cg.Name  GroupName
                //,e.*,Replace(CONVERT(VARCHAR(11), e.DOB, 106), ' ', '-') DOB2
                //,c.Name CompanyName
                //                     ,isnull(doc.c,0) doc
                //,isnull(kpi.c,0) kpi
                //,isnull(act.c,0) act
                //                     ,e.IsFirstLogin Loggedin
                //                     ,e.Submit Submitted
                //,r.Code RCode
                //,r.Name RName
                //                      from
                //                     (SELECT *,[Status]=case  when IsFirstLogin=0 then 'Not Loggedin'
                //				when (IsFirstLogin=1 and Submit=0) then 'Not Submitted'
                //				else 'Submitted'
                //				end
                //                         FROM Employee

                //                     ) e
                //                     left outer join Employee r on r.Id=e.ReportingOfficerId
                //                     left outer join dbo.Company c on c.id = e.CompanyId
                //                     left outer join dbo.CompanyGroup cg on cg.id = c.CompanyGroupId

                //                     left outer join
                //  (
                //  select count(id) c,EmployeeId from [dbo].[DocumentActivity] group by EmployeeId
                //  ) doc on doc.EmployeeId=e.Id

                //  	left outer join
                //  (
                //  select count(id) c,EmployeeId from [dbo].KPI group by EmployeeId
                //  ) kpi on kpi.EmployeeId=e.Id

                //  	left outer join
                //  (
                //  select count(id) c,EmployeeId from [dbo].ActivityEmp group by EmployeeId
                //  ) act on act.EmployeeId=e.Id

                //                     where c.CompanyGroupId='" + status.CompanyGroupId + @"' " + wc_count + @" " + wc + @"
                //                                     ";
                parameters.CmdText = SQLEmpStatus(status);

                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string SQLEmpStatus(ReportParam status)
        {
            string wc = "";
            string wc_count = "";
            try
            {
                if (status.notloggedin)
                {
                    wc = "  status='Not Loggedin'";
                }
                //else
                //{
                //    wc = " where IsFirstLogin=1";
                //}

                if (status.Submitted)
                {
                    if (wc.Length > 0)
                    {
                        wc += " or status='Submitted'";
                    }
                    else
                    {
                        wc = " status='Submitted'";
                    }
                }
                //else
                //{
                //    wc += " or Submit=0";
                //}

                if (status.NotSubmitted)
                {
                    //wc += " or status='Not Submitted' ";
                    if (wc.Length > 0)
                    {
                        wc += " or status='Not Submitted'";
                    }
                    else
                    {
                        wc = " status='Not Submitted'";
                    }
                }
                //else
                //{
                //    wc += " or Submit=1 ";
                //}

                if (wc.Length > 0)
                {
                    wc = " and (" + wc + ") ";
                }

                if (status.withoutactivity)
                {
                    wc_count = " and isnull(act.c,0)=0 ";
                }

                string _sql = @" select
                            cg.Name  GroupName
							,e.*,Replace(CONVERT(VARCHAR(11), e.DOB, 106), ' ', '-') DOB2
							,c.Name CompanyName
                            ,isnull(doc.c,0) doc
							,isnull(kpi.c,0) kpi
							,isnull(act.c,0) act
                            ,e.IsFirstLogin Loggedin
                            ,e.Submit Submitted
							,r.Code RCode
							,r.Name RName
                             from
                            (SELECT *,[Status]=case  when IsFirstLogin=0 then 'Not Loggedin'
											when (IsFirstLogin=1 and Submit=0) then 'Not Submitted'
											else 'Submitted'
											end
                                FROM Employee

                            ) e
                            left outer join Employee r on r.Id=e.ReportingOfficerId
                            left outer join dbo.Company c on c.id = e.CompanyId
                            left outer join dbo.CompanyGroup cg on cg.id = c.CompanyGroupId

                            left outer join
							  (
							  select count(id) c,EmployeeId from [dbo].[DocumentActivity] group by EmployeeId
							  ) doc on doc.EmployeeId=e.Id

							  	left outer join
							  (
							  select count(id) c,EmployeeId from [dbo].KPI group by EmployeeId
							  ) kpi on kpi.EmployeeId=e.Id

							  	left outer join
							  (
							  select count(id) c,EmployeeId from [dbo].ActivityEmp group by EmployeeId
							  ) act on act.EmployeeId=e.Id

                            where c.CompanyGroupId='" + status.CompanyGroupId + @"' " + wc_count + @" " + wc + @"
                                            ";

                return _sql;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Overall Status

        #region DatewiseStatus

        public IWorkbook ActivityInfo(ExcelEngine excelEngine, ReportParam status, string fd, string td)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                string filterInfo = "From [" + fd + "] To [" + td + "]";
                oRU = new ReportUtility();
                DataSet dsLocal = GetSelectedDatewiseActivityInfo(status, fd, td);
                DataSet dsOther = GetOtherThanTheseDates(status, fd, td);
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheet_Activity(ref sheet1, oRU, "Day-wise Status", "Day-wise Status", filterInfo, dsLocal, dsOther, status);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_Activity(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, string filterInfo, DataSet dslocal, DataSet dsOther, ReportParam status)
        {
            string _Currency = string.Empty;
            string _CurrencyId = string.Empty;
            DataView dvVoucher = null;
            DataTable dtVoucher = null;

            DataView dvOther = null;
            var xlsRow = 1;
            var xlsCol = 1;
            var startXlsRow = 1;
            var shet2EndxlsCol = 1;
            try
            {
                #region Data

                dvOther = new DataView(dsOther.Tables[0]);
                //DataSet dsCol = GetColumnInfo(status.CompanyGroupId);
                //DataView dvCol = new DataView(dsCol.Tables[0]);
                //dvCol.Sort = "Sequence";
                //DataTable dtCol = dvCol.ToTable();
                // DataSet dslocal = GetVoucherInfo(masterid);
                dvVoucher = new DataView(dslocal.Tables[0])
                {
                    Sort = "Name"
                };
                dtVoucher = dvVoucher.ToTable();
                if (dtVoucher.Rows.Count == 0)
                {
                    throw (new Exception("No Employee Found !!!"));
                }

                #endregion Data

                #region Sheet2 Data

                xlsRow = 5;
                var ExtraRow = 5;
                xlsRow += ExtraRow;//Header
                startXlsRow = xlsRow;

                #region Detail Header

                xlsCol = 1;

                var cSR = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Sr", 7);
                xlsCol = xlsCol + 1;

                var cId = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Id", 5);
                // sheet2.Range[GetColumnNameForXls(xlsCol) + (xlsRow - 1) + ":" + GetColumnNameForXls(xlsCol) + xlsRow].Merge();
                xlsCol = xlsCol + 1;

                var cCode = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Code", 12);
                xlsCol = xlsCol + 1;

                var cName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Name", 25);
                xlsCol = xlsCol + 1;

                var cCompanyName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Company Name", 25);
                xlsCol = xlsCol + 1;

                //for (int c = 0; c < dtCol.Rows.Count; c++)
                //{
                //    string ClinetColumnName = dtCol.Rows[c]["AplosColumnName"].ToString();
                //    oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, ClinetColumnName, 20);
                //    xlsCol = xlsCol + 1;
                //}

                ////log
                var clCurrent = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Newly Logged in", 10);
                xlsCol = xlsCol + 1;
                //submitted
                var cSubmitted = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Submitted", 10);
                xlsCol = xlsCol + 1;
                //int clTotal = xlsCol;
                //oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Total");
                //xlsCol = xlsCol + 1;
                //activity
                var caCurrent = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow - 1, caCurrent, "Activity");
                sheet.Range[xlsRow - 1, caCurrent].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[oRU.GetColumnNameForXls(caCurrent) + (xlsRow - 1) + ":" + oRU.GetColumnNameForXls(caCurrent + 1) + (xlsRow - 1)].Merge();

                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Current", 7);
                xlsCol = xlsCol + 1;
                var caTotal = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Total", 7);
                xlsCol = xlsCol + 1;

                //doc
                var cdCurrent = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow - 1, cdCurrent, "Documents");
                sheet.Range[xlsRow - 1, cdCurrent].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[oRU.GetColumnNameForXls(cdCurrent) + (xlsRow - 1) + ":" + oRU.GetColumnNameForXls(cdCurrent + 1) + (xlsRow - 1)].Merge();

                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Current", 7);
                xlsCol = xlsCol + 1;
                var cdTotal = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Total", 7);
                xlsCol = xlsCol + 1;

                //kpi
                var ckCurrent = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow - 1, ckCurrent, nameof(KPI));
                sheet.Range[xlsRow - 1, ckCurrent].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[oRU.GetColumnNameForXls(ckCurrent) + (xlsRow - 1) + ":" + oRU.GetColumnNameForXls(ckCurrent + 1) + (xlsRow - 1)].Merge();

                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Current", 7);
                xlsCol = xlsCol + 1;
                var ckTotal = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Total", 7);
                //xlsCol = xlsCol + 1;

                //int cStatus = xlsCol;
                //oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Status");
                //xlsCol = xlsCol + 1;

                //int cSubmitted = xlsCol;
                //oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Submitted");

                shet2EndxlsCol = xlsCol;

                oRU.SetHeaderText(ref sheet, startXlsRow - ExtraRow, cSR, filterInfo);
                sheet.Range[startXlsRow - ExtraRow, cSR].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[oRU.GetColumnNameForXls(cSR) + (startXlsRow - ExtraRow) + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + (startXlsRow - ExtraRow)].Merge();
                //sheet.Range["A2" + ":" + GetColumnNameForXls(lastcol) + "2"].Merge();

                xlsRow += 1;
                //sheet2.Range[(xlsRow - 2), cACCode, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);

                #endregion Detail Header

                //xlsRow += 1;
                var Row_Total_Start = xlsRow;
                var _index = 0;
                //company	Id	Code	Name	FirstLoginTime
                //CurrentLoggedin	CurrentActivity	CurrentDoc	CurrentKpi
                //OtherDay_log	OtherDay_act	OtherDay_doc	OtherDay_kpi
                var _Count_New_Log = 0;
                var _Count_New_Sub = 0;
                var _Count_Activity = 0; var curr_act_person = 0;
                var _Count_Doc = 0; var curr_doc_person = 0;
                var _Count_Kpi = 0; var curr_kpi_person = 0;
                for (int icount = 0; icount < dtVoucher.Rows.Count; icount++)
                {
                    #region body

                    string _id = dtVoucher.Rows[icount]["Id"].ToString();
                    //other starts
                    var OtherDay_act = 0;
                    var OtherDay_doc = 0;
                    var OtherDay_kpi = 0;

                    var curr_sub = 0;
                    var curr_log = 0;
                    var curr_act = 0;
                    var curr_doc = 0;
                    var curr_kpi = 0;

                    dvOther.RowFilter = "Id='" + dtVoucher.Rows[icount]["Id"] + "'";
                    if (dvOther.Count > 0)
                    {
                        OtherDay_act = Convert.ToInt32(dvOther[0]["AllActivity"].ToString());
                        OtherDay_doc = Convert.ToInt32(dvOther[0]["AllDoc"].ToString());
                        OtherDay_kpi = Convert.ToInt32(dvOther[0]["AllKpi"].ToString());
                    }
                    //other ends
                    curr_log = Convert.ToInt32(dtVoucher.Rows[icount]["CurrentLoggedin"].ToString());
                    curr_sub = Convert.ToInt32(dtVoucher.Rows[icount]["CurrentSubmitted"].ToString());
                    curr_act = Convert.ToInt32(dtVoucher.Rows[icount]["CurrentActivity"].ToString());
                    curr_doc = Convert.ToInt32(dtVoucher.Rows[icount]["CurrentDoc"].ToString());
                    curr_kpi = Convert.ToInt32(dtVoucher.Rows[icount]["CurrentKpi"].ToString());

                    if (curr_sub == 0 && curr_log == 0 && curr_act == 0 && curr_doc == 0 && curr_kpi == 0)
                    {// && OtherDay_act == 0 && OtherDay_doc == 0 && OtherDay_kpi == 0
                        continue;
                    }
                    else
                    {
                        _Count_New_Log += curr_log;
                        _Count_New_Sub += curr_sub;
                        _Count_Activity += curr_act;
                        _Count_Doc += curr_doc;
                        _Count_Kpi += curr_kpi;

                        if (curr_act > 0)
                        {
                            curr_act_person++;
                        }
                        if (curr_doc > 0)
                        {
                            curr_doc_person++;
                        }
                        if (curr_kpi > 0)
                        {
                            curr_kpi_person++;
                        }

                        _index += 1;
                        oRU.SetText(ref sheet, xlsRow, cSR, _index);
                        oRU.SetText(ref sheet, xlsRow, cId, _id);
                        oRU.SetText(ref sheet, xlsRow, cCode, dtVoucher.Rows[icount]["Code"].ToString());
                        oRU.SetText(ref sheet, xlsRow, cName, dtVoucher.Rows[icount]["Name"].ToString());
                        oRU.SetText(ref sheet, xlsRow, cCompanyName, dtVoucher.Rows[icount]["CompanyName"].ToString());

                        oRU.SetText(ref sheet, xlsRow, clCurrent, curr_log);
                        oRU.SetText(ref sheet, xlsRow, cSubmitted, curr_sub);
                        //oRU.SetText(ref sheet, xlsRow, clTotal, dtVoucher.Rows[icount]["OtherDay_log"].ToString());
                        oRU.SetText(ref sheet, xlsRow, caCurrent, curr_act);
                        oRU.SetText(ref sheet, xlsRow, caTotal, OtherDay_act);
                        oRU.SetText(ref sheet, xlsRow, cdCurrent, curr_doc);
                        oRU.SetText(ref sheet, xlsRow, cdTotal, OtherDay_doc);
                        oRU.SetText(ref sheet, xlsRow, ckCurrent, curr_kpi);
                        oRU.SetText(ref sheet, xlsRow, ckTotal, OtherDay_kpi);
                        //oRU.SetText(ref sheet, xlsRow, cSubmitted, dtVoucher.Rows[icount]["Submitted"].ToString());
                        xlsRow += 1;
                    }
                    dvOther.RowFilter = null;

                    #endregion body
                }
                //border

                //Set sammary in the header
                oRU.SetHeaderText(ref sheet, startXlsRow - 4, cSR, "Newly Logged in", ExcelHAlign.HAlignRight, false);
                oRU.SetText(ref sheet, startXlsRow - 4, cSR + 1, _Count_New_Log, ExcelHAlign.HAlignLeft);

                oRU.SetHeaderText(ref sheet, startXlsRow - 4, clCurrent, "Total Activity", ExcelHAlign.HAlignRight, false);
                oRU.SetText(ref sheet, startXlsRow - 4, clCurrent + 1, _Count_Activity + " (of " + curr_act_person + " Person)", ExcelHAlign.HAlignLeft);
                sheet.Range[oRU.GetColumnNameForXls(clCurrent + 1) + (startXlsRow - 4) + ":" + oRU.GetColumnNameForXls(clCurrent + 4) + (startXlsRow - 4)].Merge();

                //2nd row
                oRU.SetHeaderText(ref sheet, startXlsRow - 3, cSR, "Newly Submitted", ExcelHAlign.HAlignRight, false);
                oRU.SetText(ref sheet, startXlsRow - 3, cSR + 1, _Count_New_Sub, ExcelHAlign.HAlignLeft);
                //sheet.Range[oRU.GetColumnNameForXls(cSR + 1) + (startXlsRow - 3) + ":" + oRU.GetColumnNameForXls(cSR + 3) + (startXlsRow -3)].Merge();

                oRU.SetHeaderText(ref sheet, startXlsRow - 3, clCurrent, "Total KPI", ExcelHAlign.HAlignRight, false);
                oRU.SetText(ref sheet, startXlsRow - 3, clCurrent + 1, _Count_Kpi + " (of " + curr_kpi_person + " Person)", ExcelHAlign.HAlignLeft);
                sheet.Range[oRU.GetColumnNameForXls(clCurrent + 1) + (startXlsRow - 3) + ":" + oRU.GetColumnNameForXls(clCurrent + 4) + (startXlsRow - 3)].Merge();

                //3rd row
                oRU.SetHeaderText(ref sheet, startXlsRow - 2, cSR, "Total Docs", ExcelHAlign.HAlignRight, false);
                oRU.SetText(ref sheet, startXlsRow - 2, cSR + 1, _Count_Doc + " (of " + curr_doc_person + " Person)", ExcelHAlign.HAlignLeft);
                sheet.Range[oRU.GetColumnNameForXls(cSR + 1) + (startXlsRow - 2) + ":" + oRU.GetColumnNameForXls(cSR + 3) + (startXlsRow - 2)].Merge();

                //Summary at the end
                oRU.SetText(ref sheet, xlsRow, clCurrent - 1, "Total: ", true);
                string Formula_L = "=sum(" + oRU.GetColumnNameForXls(clCurrent) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(clCurrent) + (xlsRow - 1) + ")";
                oRU.SetFormula(ref sheet, xlsRow, clCurrent, Formula_L, false);
                //submit
                string Formula_S = "=sum(" + oRU.GetColumnNameForXls(cSubmitted) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(cSubmitted) + (xlsRow - 1) + ")";
                oRU.SetFormula(ref sheet, xlsRow, cSubmitted, Formula_S, false);

                string Formula_Act = "=sum(" + oRU.GetColumnNameForXls(caCurrent) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(caCurrent) + (xlsRow - 1) + ")";
                oRU.SetFormula(ref sheet, xlsRow, caCurrent, Formula_Act, false);

                string Formula_Doc = "=sum(" + oRU.GetColumnNameForXls(cdCurrent) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(cdCurrent) + (xlsRow - 1) + ")";
                oRU.SetFormula(ref sheet, xlsRow, cdCurrent, Formula_Doc, false);

                string Formula_Kpi = "=sum(" + oRU.GetColumnNameForXls(ckCurrent) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(ckCurrent) + (xlsRow - 1) + ")";
                oRU.SetFormula(ref sheet, xlsRow, ckCurrent, Formula_Kpi, false);

                //sheet2.Range[(Row_Total_Start), 1, xlsRow, shet2EndxlsCol].BorderAround(ExcelLineStyle.Thin);
                sheet.Range[(Row_Total_Start), 1, xlsRow, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                xlsRow = xlsRow + 6;

                #endregion Sheet2 Data

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                oRU.CompanyGroupHeaderPhoenix(ref sheet, shet2EndxlsCol, SheetHeader, status.CompanyGroupId);
                oRU.FreezePage(ref sheet, 3, startXlsRow + 1);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait, status.EmployeeName);
                sheet.Name = SheetName;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet Datewisestatus(ReportParam status)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"
                                            select
                                            x.FirstLoginTime
                                            ,a.AllLoggedin
                                            ,b.CurrentLoggedin
                                            ,act.AllActivity
                                            ,actc.CurrentActivity
                                            ,d.AllDoc
                                            ,dc.CurrentDoc
                                            ,k.AllKpi
                                            ,kc.CurrentKpi
                                             from

                                            (select distinct Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 106), ' ', '-') FirstLoginTime  from .[Employee] e) x
                                             --LOGGGEDIN
                                            left outer join
                                            (
                                            select
                                            count(Id) AllLoggedin
                                            ,Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 106), ' ', '-') FirstLoginTime
                                            from [dbo].[Employee] e
                                            where Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 106), ' ', '-')<>'14-Oct-2017'
                                            group by Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 106), ' ', '-')
                                             ) a on x.FirstLoginTime=a.FirstLoginTime
                                            left outer join

                                            (
                                            select
                                            count(Id) CurrentLoggedin
                                            ,Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 106), ' ', '-') FirstLoginTime
                                            from [dbo].[Employee] e
                                            where Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 106), ' ', '-')='14-Oct-2017'
                                            group by Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 106), ' ', '-')
                                            ) b on x.FirstLoginTime=b.FirstLoginTime

                                            --ACTIVITY
                                            left outer join
                                            (
                                            select
                                            count(Id) AllActivity
                                            ,Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-') FirstLoginTime
                                            from [dbo].ActivityEmp e
                                            where Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-')<>'14-Oct-2017'
                                            group by Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-')
                                             ) act on x.FirstLoginTime=act.FirstLoginTime

                                             left outer join
                                            (
                                            select
                                            count(Id) CurrentActivity
                                            ,Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-') FirstLoginTime
                                            from [dbo].ActivityEmp e
                                            where Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-')='14-Oct-2017'
                                            group by Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-')
                                             ) actc on x.FirstLoginTime=actc.FirstLoginTime
                                            --DOC
                                              left outer join
                                            (
                                            select
                                            count(Id) CurrentDoc
                                            ,Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-') FirstLoginTime
                                            from [dbo].DocumentActivity e
                                            where Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-')='14-Oct-2017'
                                            group by Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-')
                                             ) dc on x.FirstLoginTime=dc.FirstLoginTime

                                               left outer join
                                            (
                                            select
                                            count(Id) AllDoc
                                            ,Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-') FirstLoginTime
                                            from [dbo].DocumentActivity e
                                            where Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-')<>'14-Oct-2017'
                                            group by Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-')
                                             ) d on x.FirstLoginTime=d.FirstLoginTime
                                            --KPI
                                                left outer join
                                            (
                                            select
                                            count(Id) AllKpi
                                            ,Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-') FirstLoginTime
                                            from [dbo].KPI e
                                            where Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-')<>'14-Oct-2017'
                                            group by Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-')
                                             ) k on x.FirstLoginTime=k.FirstLoginTime

                                                 left outer join
                                            (
                                            select
                                            count(Id) CurrentKpi
                                            ,Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-') FirstLoginTime
                                            from [dbo].KPI e
                                            where Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-')='14-Oct-2017'
                                            group by Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-')
                                             ) kc on x.FirstLoginTime=kc.FirstLoginTime
                                            ";
                //order by e.plant,e.division,e.department,e.Name

                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetSelectedDatewiseActivityInfo(ReportParam param, string fromdate, string todate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                //string _sql = @"
                //                     select
                //                    c.Name CompanyName
                //                    ,x.Id
                //                    ,x.Code
                //                    ,x.Name
                //                    ,x.FirstLoginTime
                //                    ,isnull(b.CurrentLoggedin,0) CurrentLoggedin
                //                    ,isnull(actc.CurrentActivity,0) CurrentActivity
                //                    ,isnull(dc.CurrentDoc,0) CurrentDoc
                //                    ,isnull(kc.CurrentKpi,0) CurrentKpi
                //                    ,0 OtherDay_log
                //                    ,0 OtherDay_act
                //                    ,0 OtherDay_doc
                //                    ,0 OtherDay_kpi
                //                     from

                //                    (select distinct Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 106), ' ', '-') FirstLoginTime
                //                    ,e.Id,e.CompanyId,e.Name,e.Code
                //                    from .[Employee] e) x
                //                    inner join (select * from Company where CompanyGroupId='" + param.CompanyGroupId + @"') c on c.Id=x.CompanyId

                //                    left outer join
                //                    (
                //                    select
                //                    count(Id) CurrentLoggedin
                //                    ,Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 106), ' ', '-') FirstLoginTime,e.Id
                //                    from [dbo].[Employee] e
                //                    where Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 106), ' ', '-') between '" + fromdate + @"' and '" + todate + @"'
                //                    group by Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 106), ' ', '-'),e.Id
                //                    ) b on x.FirstLoginTime=b.FirstLoginTime and x.Id=b.Id

                //                    --activity
                //                     left outer join
                //                    (
                //                    select
                //                    count(Id) CurrentActivity
                //                    ,e.EmployeeId
                //                    from [dbo].ActivityEmp e
                //                    where Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-') between '" + fromdate + @"' and '" + todate + @"'
                //                    group by e.EmployeeId
                //                     ) actc on  x.Id=actc.EmployeeId

                //                      left outer join
                //                    (
                //                    select
                //                    count(Id) CurrentDoc
                //                    ,e.EmployeeId
                //                    from [dbo].DocumentActivity e
                //                    where Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-') between '" + fromdate + @"' and '" + todate + @"'
                //                    group by e.EmployeeId
                //                     ) dc on  x.Id=dc.EmployeeId

                //                         left outer join
                //                    (
                //                    select
                //                    count(Id) CurrentKpi,e.EmployeeId
                //                    from [dbo].KPI e
                //                    where Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-') between '" + fromdate + @"' and '" + todate + @"'
                //                    group by e.EmployeeId
                //                     ) kc on  x.Id=kc.EmployeeId
                //                            ";
                parameters.CmdText = @"
                                     select
                                    c.Name CompanyName
                                    ,x.Id
                                    ,x.Code
                                    ,x.Name
                                    ,x.FirstLoginTime
                                    ,isnull(b.CurrentLoggedin,0) CurrentLoggedin
                                    ,isnull(s.CurrentSubmitted,0) CurrentSubmitted
                                    ,isnull(actc.CurrentActivity,0) CurrentActivity
                                    ,isnull(dc.CurrentDoc,0) CurrentDoc
                                    ,isnull(kc.CurrentKpi,0) CurrentKpi
                                    ,0 OtherDay_log
                                    ,0 OtherDay_act
                                    ,0 OtherDay_doc
                                    ,0 OtherDay_kpi
                                     from

                                    (select distinct Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 106), ' ', '-') FirstLoginTime
                                    ,e.Id,e.CompanyId,e.Name,e.Code
                                    ,Replace(CONVERT(VARCHAR(11), e.SubmitTime, 106), ' ', '-') SubmitTime
                                    from .[Employee] e) x
                                    inner join (select * from Company where CompanyGroupId='" + param.CompanyGroupId + @"') c on c.Id=x.CompanyId
                                    --loggedin
                                    left outer join
                                    (
                                    select
                                    count(Id) CurrentLoggedin
                                    ,Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 106), ' ', '-') FirstLoginTime,e.Id
                                    from [dbo].[Employee] e
                                    where Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 106), ' ', '-') between '" + fromdate + @"' and '" + todate + @"'
                                    group by Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 106), ' ', '-'),e.Id
                                    ) b on x.FirstLoginTime=b.FirstLoginTime and x.Id=b.Id
                                    --submitted
									 left outer join
                                    (
                                    select
                                    count(Id) CurrentSubmitted
                                    ,Replace(CONVERT(VARCHAR(11), e.SubmitTime, 106), ' ', '-') SubmitTime,e.Id
                                    from [dbo].[Employee] e
                                    where Replace(CONVERT(VARCHAR(11), e.SubmitTime, 106), ' ', '-') between '27-Oct-2017' and '27-Oct-2017'
                                    group by Replace(CONVERT(VARCHAR(11), e.SubmitTime, 106), ' ', '-'),e.Id
                                    ) s on x.SubmitTime=s.SubmitTime and x.Id=s.Id
                                    --activity
                                     left outer join
                                    (
                                    select
                                    count(Id) CurrentActivity
                                    ,e.EmployeeId
                                    from [dbo].ActivityEmp e
                                    where Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-') between '" + fromdate + @"' and '" + todate + @"'
                                    group by e.EmployeeId
                                     ) actc on  x.Id=actc.EmployeeId

                                      left outer join
                                    (
                                    select
                                    count(Id) CurrentDoc
                                    ,e.EmployeeId
                                    from [dbo].DocumentActivity e
                                    where Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-') between '" + fromdate + @"' and '" + todate + @"'
                                    group by e.EmployeeId
                                     ) dc on  x.Id=dc.EmployeeId

                                         left outer join
                                    (
                                    select
                                    count(Id) CurrentKpi,e.EmployeeId
                                    from [dbo].KPI e
                                    where Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-') between '" + fromdate + @"' and '" + todate + @"'
                                    group by e.EmployeeId
                                     ) kc on  x.Id=kc.EmployeeId
                                            ";
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetOtherThanTheseDates(ReportParam param, string fromdate, string todate)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"

                                       select
                                        c.Name company
                                        ,x.Id
                                        ,x.Code
                                        ,x.Name
                                        --,x.FirstLoginTime
                                        ,isnull(a.AllLoggedin,0) AllLoggedin
                                        ,isnull(act.AllActivity,0) AllActivity
                                        ,isnull(d.AllDoc,0) AllDoc
                                        ,isnull(k.AllKpi,0) AllKpi
                                         from

                                        (select distinct Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 106), ' ', '-') FirstLoginTime
                                        ,e.Id,e.CompanyId,e.Name,e.Code
                                        from .[Employee] e) x
                                        left outer join  (select * from Company where CompanyGroupId='" + param.CompanyGroupId + @"') c on c.Id=x.CompanyId
                                        left outer join
                                        (
                                        select
                                        count(Id) AllLoggedin
                                        ,Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 106), ' ', '-') FirstLoginTime,e.Id
                                        from [dbo].[Employee] e
                                        where Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 106), ' ', '-')  between '" + fromdate + @"' and '" + todate + @"'
                                        group by Replace(CONVERT(VARCHAR(11), e.FirstLoginTime, 106), ' ', '-'),e.Id
                                         ) a on x.FirstLoginTime=a.FirstLoginTime and x.Id=a.Id
                                        --activity
                                        left outer join
                                        (
                                        select
                                        count(Id) AllActivity
                                        ,e.EmployeeId
                                        from [dbo].ActivityEmp e
                                        --where Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-') not between '" + fromdate + @"' and '" + todate + @"'
                                        group by e.EmployeeId
                                         ) act on x.Id=act.EmployeeId

                                           left outer join
                                        (
                                        select
                                        count(Id) AllDoc
                                        ,e.EmployeeId
                                        from [dbo].DocumentActivity e
                                        --where Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-') not between '" + fromdate + @"' and '" + todate + @"'
                                        group by e.EmployeeId
                                         ) d on  x.Id=d.EmployeeId

                                            left outer join
                                        (
                                        select
                                        count(Id) AllKpi
                                        ,e.EmployeeId
                                        from [dbo].KPI e
                                        --where Replace(CONVERT(VARCHAR(11), e.AddedDateTime, 106), ' ', '-') not between '" + fromdate + @"' and '" + todate + @"'
                                        group by e.EmployeeId
                                         ) k on x.Id=k.EmployeeId

                                            ";
                //order by e.plant,e.division,e.department,e.Name

                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion DatewiseStatus

        #region Exception

        public IWorkbook ExceptionDocKpi(ExcelEngine excelEngine, ReportParam status)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                //DateTime dtt = new DateTime();
                //dtt.ToDbDate()

                //FromDate = "14-Oct-2017";
                //ToDate = "15-Oct-2017";
                //string filterInfo = "From [" + fd + "] To [" + td + "]";
                oRU = new ReportUtility();
                DataSet dsLocal = GetException(status);
                //DataSet dsOther = GetOtherThanTheseDates(status, fd, td);
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheet_Exception(ref sheet1, oRU, "Exception Status", "Exception Status", dsLocal, status);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
            }
        }

        private DataSet GetException(ReportParam param)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"
                                     select
                                            e.Id
                                            ,e.Code
                                            ,e.Name
                                            ,a.Id ActivityId
                                            --,a.Code ActivityCode
                                            ,a.Name ActivityName

                                             ,Doc = case a.Documents when 1 then 1
											when 0 then 0
											else 2 end

											,KPI = case a.KPI when 1 then 1
											when 0 then 0
											else 2 end

                                            ,isnull(d.c,0) cCount
                                            ,isnull(k.c,0) kCount
                                            ,Status=case e.Submit when 1 then 'Submitted'
											else 'Not Submitted' end
                                            ,e.IsFirstlogin,c.Name CompanyName
                                            from [dbo].[Employee] e
                                            left outer join [dbo].[ActivityEmp] a on e.Id=a.EmployeeId
                                            left outer join Company c on c.id=e.CompanyId
                                            left outer join (select count(id) c,ActivityId from DocumentActivity
                                            group by ActivityId
                                            ) d on d.ActivityId=a.Id

                                            left outer join (select count(id) c,ActivityId from KPI
                                            group by ActivityId
                                            ) k on k.ActivityId=a.Id

                                          Where (isnull(e.IsFirstlogin,0)=1
											and c.CompanyGroupId='" + param.CompanyGroupId + @"'
											and ((isnull(a.Documents,0)=1 and isnull(d.c,0) =0)
											or (isnull(a.KPI,0)=1 and isnull(k.c,0) =0)
                                            ))
                                            ";

                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_Exception(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dslocal, ReportParam status)
        {
            var xlsRow = 1;
            var xlsCol = 1;
            var startXlsRow = 1;
            var shet2EndxlsCol = 1;

            try
            {
                #region Data

                DataView dvEmp = new DataView(dslocal.Tables[0])
                {
                    Sort = "Name"
                };
                DataTable dtEmp = dvEmp.ToTable(true, "Id", "Code", "Name", "CompanyName", "Status");

                if (dtEmp.Rows.Count == 0)
                {
                    throw (new Exception("No Employee Found !!!"));
                }

                #endregion Data

                #region Sheet2 Data

                xlsRow = 5;
                var ExtraRow = 0;
                xlsRow += ExtraRow;//Header
                startXlsRow = xlsRow;

                #region Detail Header

                xlsCol = 1;

                var cSR = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Sr", 7);
                xlsCol = xlsCol + 1;

                var cId = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Id", 5);
                // sheet2.Range[GetColumnNameForXls(xlsCol) + (xlsRow - 1) + ":" + GetColumnNameForXls(xlsCol) + xlsRow].Merge();
                xlsCol = xlsCol + 1;

                var cCode = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Code", 12);
                xlsCol = xlsCol + 1;

                var cName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Name", 25);
                xlsCol = xlsCol + 1;

                var cCompanyName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Company Name", 25);
                xlsCol = xlsCol + 1;

                var cStatus = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, cStatus, "Status");
                xlsCol = xlsCol + 1;

                var caName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, caName, "Activity Name", 50);
                xlsCol = xlsCol + 1;

                ////doc
                //int cd = xlsCol;
                //oRU.SetHeaderText(ref sheet, xlsRow, cd, "Documents");
                //xlsCol = xlsCol + 1;

                ////kpi
                //int ck = xlsCol;
                //oRU.SetHeaderText(ref sheet, xlsRow, ck, "KPI");
                //xlsCol = xlsCol + 1;

                var cds = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, cds, "Doc. Status");
                xlsCol = xlsCol + 1;

                var cdc = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, cdc, "Document Count");
                xlsCol = xlsCol + 1;

                var cks = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, cks, "KPI Status");
                xlsCol = xlsCol + 1;

                var ckc = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, ckc, "KPI Count");
                //xlsCol = xlsCol + 1;

                shet2EndxlsCol = xlsCol;

                //oRU.SetHeaderText(ref sheet, startXlsRow - ExtraRow, cSR, filterInfo);
                //sheet.Range[startXlsRow - ExtraRow, cSR].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet.Range[oRU.GetColumnNameForXls(cSR) + (startXlsRow - ExtraRow) + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + (startXlsRow - ExtraRow)].Merge();

                xlsRow += 1;

                #endregion Detail Header

                var Row_Total_Start = xlsRow;
                var _index = 1;

                for (int icount = 0; icount < dtEmp.Rows.Count; icount++)
                {
                    #region body

                    string _id = dtEmp.Rows[icount]["Id"].ToString();
                    var Start_Row = xlsRow;

                    oRU.SetText(ref sheet, xlsRow, cSR, _index.ToString());
                    oRU.SetText(ref sheet, xlsRow, cId, _id);
                    oRU.SetText(ref sheet, xlsRow, cCode, dtEmp.Rows[icount]["Code"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cName, dtEmp.Rows[icount]["Name"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cCompanyName, dtEmp.Rows[icount]["CompanyName"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cStatus, dtEmp.Rows[icount]["Status"].ToString());
                    if (dtEmp.Rows[icount]["Status"].ToString().ToUpper() == "SUBMITTED")
                    {
                        sheet.Range[xlsRow, cStatus].CellStyle.Font.Color = ExcelKnownColors.Red;
                    }

                    //if(_id=="871")
                    //{
                    //}
                    DataView dva = new DataView(dslocal.Tables[0])
                    {
                        RowFilter = "Id='" + _id + "'",
                        Sort = "ActivityName"
                    };
                    DataTable dta = dva.ToTable(true, "ActivityId", "ActivityName", "Doc", nameof(KPI), "cCount", "kCount", "Status");

                    for (int a = 0; a < dta.Rows.Count; a++)
                    {
                        #region activity

                        var d = dta.Rows[a]["Doc"].ToString();
                        var k = dta.Rows[a][nameof(KPI)].ToString();
                        if (d == "1" || k == "1")
                        {
                            var dc = Convert.ToInt32(dta.Rows[a]["cCount"].ToString());
                            var kc = Convert.ToInt32(dta.Rows[a]["kCount"].ToString());

                            if (dc == 0 || kc == 0)
                            {
                                oRU.SetText(ref sheet, xlsRow, caName, dta.Rows[a]["ActivityName"].ToString());
                                //oRU.SetText(ref sheet, xlsRow, cStatus, dta.Rows[a]["Status"].ToString());

                                if (d == "1")
                                {
                                    oRU.SetText(ref sheet, xlsRow, cds, "Yes");
                                    oRU.SetText(ref sheet, xlsRow, cdc, dta.Rows[a]["cCount"].ToString());
                                    if (dc > 0)
                                    {
                                        // sheet.Range[xlsRow, cdc].CellStyle.ColorIndex = ExcelKnownColors.Green;
                                        // sheet.Range[xlsRow, cdc].CellStyle.Font.Color = ExcelKnownColors.White;
                                    }
                                    else
                                    {
                                        sheet.Range[xlsRow, cds].CellStyle.ColorIndex = ExcelKnownColors.Red;
                                        sheet.Range[xlsRow, cds].CellStyle.Font.Color = ExcelKnownColors.White;
                                        sheet.Range[xlsRow, cdc].CellStyle.ColorIndex = ExcelKnownColors.Red;
                                        sheet.Range[xlsRow, cdc].CellStyle.Font.Color = ExcelKnownColors.White;
                                    }
                                }
                                else if (dta.Rows[a]["Doc"].ToString() == "0")
                                {
                                }
                                else
                                {
                                }

                                //kpi
                                if (k == "1")
                                {
                                    oRU.SetText(ref sheet, xlsRow, cks, "Yes");
                                    oRU.SetText(ref sheet, xlsRow, ckc, dta.Rows[a]["kCount"].ToString());
                                    if (kc > 0)
                                    {
                                        // sheet.Range[xlsRow, ckc].CellStyle.ColorIndex = ExcelKnownColors.Green;
                                        // sheet.Range[xlsRow, ckc].CellStyle.Font.Color = ExcelKnownColors.White;
                                    }
                                    else
                                    {
                                        sheet.Range[xlsRow, ckc].CellStyle.ColorIndex = ExcelKnownColors.Red;
                                        sheet.Range[xlsRow, ckc].CellStyle.Font.Color = ExcelKnownColors.White;
                                        sheet.Range[xlsRow, cks].CellStyle.ColorIndex = ExcelKnownColors.Red;
                                        sheet.Range[xlsRow, cks].CellStyle.Font.Color = ExcelKnownColors.White;
                                    }
                                }

                                xlsRow += 1;
                            }//dc kc
                        }//dk

                        #endregion activity
                    }//for

                    var _end_row = Start_Row;
                    if (xlsRow > Start_Row)
                    {
                        _index += 1;
                        _end_row = xlsRow - 1;
                    }
                    sheet.Range[oRU.GetColumnNameForXls(cSR) + (Start_Row) + ":" + oRU.GetColumnNameForXls(cSR) + _end_row].Merge();
                    sheet.Range[oRU.GetColumnNameForXls(cId) + (Start_Row) + ":" + oRU.GetColumnNameForXls(cId) + _end_row].Merge();
                    sheet.Range[oRU.GetColumnNameForXls(cCode) + (Start_Row) + ":" + oRU.GetColumnNameForXls(cCode) + _end_row].Merge();
                    sheet.Range[oRU.GetColumnNameForXls(cName) + (Start_Row) + ":" + oRU.GetColumnNameForXls(cName) + _end_row].Merge();
                    sheet.Range[oRU.GetColumnNameForXls(cCompanyName) + (Start_Row) + ":" + oRU.GetColumnNameForXls(cCompanyName) + _end_row].Merge();
                    sheet.Range[oRU.GetColumnNameForXls(cStatus) + (Start_Row) + ":" + oRU.GetColumnNameForXls(cStatus) + _end_row].Merge();

                    #endregion body
                }
                //border

                //sheet2.Range[(Row_Total_Start), 1, xlsRow, shet2EndxlsCol].BorderAround(ExcelLineStyle.Thin);
                sheet.Range[(Row_Total_Start), 1, xlsRow, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                xlsRow = xlsRow + 2;

                #endregion Sheet2 Data

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                oRU.CompanyGroupHeaderPhoenix(ref sheet, shet2EndxlsCol, SheetHeader, status.CompanyGroupId);
                oRU.FreezePage(ref sheet, 3, startXlsRow + 1);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait, status.EmployeeName);
                sheet.Name = SheetName;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Exception

        #region Individual

        public IWorkbook IndividualDocKpi(ExcelEngine excelEngine, ReportParam status)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                //DateTime dtt = new DateTime();
                //dtt.ToDbDate()

                //FromDate = "14-Oct-2017";
                //ToDate = "15-Oct-2017";
                //string filterInfo = "From [" + fd + "] To [" + td + "]";
                oRU = new ReportUtility();
                DataSet dsLocal = GetIndividualInfo(status);
                //DataSet dsOther = GetOtherThanTheseDates(status, fd, td);
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheet_Individual(ref sheet1, oRU, "Individual Status", "Individual Status", dsLocal, status);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetIndividualInfo(ReportParam param)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"
                                     select
                                            e.Id,isnull(s.Name,'')+e.Name Name,e.Code,e.FatherName,e.MotherName
                                             ,Replace(CONVERT(VARCHAR(11), e.DOB, 106), ' ', '-') DOB
                                             ,Replace(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-') DOJ
                                            ,c.Name Company
                                            ,a.Id ActivityId
                                            ,a.Name Activity,a.ActivityDetail
                                            ,d.Id DocumentId
                                            ,d.Name Document,d.FileName
                                            ,k.Id KPIId
                                            ,k.Name KPI,k.KPIDetail
                                            ,r.Name ReportingPerson
                                            from [dbo].[Employee] e
                                            left outer join [dbo].[ActivityEmp] a on a.EmployeeId=e.Id
                                            left outer join [dbo].[DocumentActivity] d on d.ActivityId = a.Id
                                            left outer join [dbo].[KPI] k on k.ActivityId=a.Id
                                            left outer join [dbo].[Company] c on c.Id=e.CompanyId
                                            left outer join Salutation s on s.Id=e.Id
                                            left outer join [dbo].[Employee] r on r.Id=e.ReportingOfficerId
                                            where e.id='" + param.EmployeeId + @"' and c.CompanyGroupId='" + param.CompanyGroupId + @"'
                                            ";

                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_Individual(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dslocal, ReportParam status)
        {
            var xlsRow = 1;
            var xlsCol = 1;
            var startXlsRow = 1;
            var shet2EndxlsCol = 1;

            try
            {
                #region Data

                DataView dvAct = new DataView(dslocal.Tables[0])
                {
                    Sort = "Activity"
                };
                DataTable dtAct = dvAct.ToTable(true, "ActivityId", "Activity", "ActivityDetail");

                if (dtAct.Rows.Count == 0)
                {
                    throw (new Exception("No Activity Found !!!"));
                }

                #endregion Data

                #region Sheet2 Data

                xlsRow = 5;
                var ExtraRow = 5;
                xlsRow += ExtraRow;

                startXlsRow = xlsRow;

                #region Detail Header

                xlsCol = 1;

                var cSR = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Sr", 7);
                xlsCol = xlsCol + 1;

                var caName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Activity Name", 50);
                xlsCol = xlsCol + 1;

                var caDetail = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Activity Detail", 50);
                xlsCol = xlsCol + 1;

                var cdName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Document Name", 30);
                xlsCol = xlsCol + 1;

                var cdDetail = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "File", 30);
                xlsCol = xlsCol + 1;

                var ckName = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, nameof(KPI), 30);
                xlsCol = xlsCol + 1;

                var ckDetail = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "KPI Detail", 30);

                shet2EndxlsCol = xlsCol;
                //oRU.SetHeaderText(ref sheet, startXlsRow - ExtraRow, cSR, filterInfo);
                //sheet.Range[startXlsRow - ExtraRow, cSR].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet.Range[oRU.GetColumnNameForXls(cSR) + (startXlsRow - ExtraRow) + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + (startXlsRow - ExtraRow)].Merge();

                xlsRow += 1;

                #endregion Detail Header

                var Row_Total_Start = xlsRow;
                var TotalDocuments = 0;
                var TotalFile = 0;
                var TotalKPI = 0;
                var _index = 1;

                for (int icount = 0; icount < dtAct.Rows.Count; icount++)
                {
                    #region body

                    string _id = dtAct.Rows[icount]["ActivityId"].ToString();
                    var Start_Row = xlsRow;

                    oRU.SetText(ref sheet, xlsRow, cSR, _index.ToString());
                    oRU.SetText(ref sheet, xlsRow, caName, dtAct.Rows[icount]["Activity"].ToString());
                    oRU.SetText(ref sheet, xlsRow, caDetail, dtAct.Rows[icount]["ActivityDetail"].ToString());

                    DataView dvdk = new DataView(dslocal.Tables[0])
                    {
                        RowFilter = "ActivityId='" + _id + "'",
                        Sort = "Document"
                    };
                    DataTable dtdk = dvdk.ToTable(true, "DocumentId", "Document", "FileName", "KPIId", nameof(KPI), "KPIDetail");

                    for (int a = 0; a < dtdk.Rows.Count; a++)
                    {
                        oRU.SetText(ref sheet, xlsRow, cdName, dtdk.Rows[a]["Document"].ToString());
                        oRU.SetText(ref sheet, xlsRow, cdDetail, dtdk.Rows[a]["FileName"].ToString());
                        oRU.SetText(ref sheet, xlsRow, ckName, dtdk.Rows[a][nameof(KPI)].ToString());
                        oRU.SetText(ref sheet, xlsRow, ckDetail, dtdk.Rows[a]["KPIDetail"].ToString());

                        if (dtdk.Rows[a]["DocumentId"].ToString().Trim().Length > 0)
                        {
                            TotalDocuments++;
                        }
                        if (dtdk.Rows[a]["FileName"].ToString().Trim().Length > 0)
                        {
                            TotalFile++;
                        }
                        if (dtdk.Rows[a]["KPIId"].ToString().Trim().Length > 0)
                        {
                            TotalKPI++;
                        }
                        xlsRow += 1;
                    }//for

                    var _end_row = Start_Row;
                    if (xlsRow > Start_Row)
                    {
                        _index += 1;
                        _end_row = xlsRow - 1;
                    }
                    sheet.Range[oRU.GetColumnNameForXls(cSR) + (Start_Row) + ":" + oRU.GetColumnNameForXls(cSR) + _end_row].Merge();
                    sheet.Range[oRU.GetColumnNameForXls(caName) + (Start_Row) + ":" + oRU.GetColumnNameForXls(caName) + _end_row].Merge();
                    sheet.Range[oRU.GetColumnNameForXls(caDetail) + (Start_Row) + ":" + oRU.GetColumnNameForXls(caDetail) + _end_row].Merge();

                    #endregion body
                }
                //border

                //Header Start

                oRU.SetHeaderText(ref sheet, startXlsRow - 5, caName, "Id", ExcelHAlign.HAlignRight, false);
                oRU.SetText(ref sheet, startXlsRow - 5, caName + 1, dslocal.Tables[0].Rows[0]["Id"].ToString(), ExcelHAlign.HAlignLeft);

                oRU.SetHeaderText(ref sheet, startXlsRow - 5, cdName, "Name", ExcelHAlign.HAlignRight, false);
                oRU.SetText(ref sheet, startXlsRow - 5, cdName + 1, dslocal.Tables[0].Rows[0]["Name"].ToString(), ExcelHAlign.HAlignLeft);

                oRU.SetHeaderText(ref sheet, startXlsRow - 4, caName, "Father Name", ExcelHAlign.HAlignRight, false);
                oRU.SetText(ref sheet, startXlsRow - 4, caName + 1, dslocal.Tables[0].Rows[0]["FatherName"].ToString(), ExcelHAlign.HAlignLeft);

                oRU.SetHeaderText(ref sheet, startXlsRow - 4, cdName, "Mother Name", ExcelHAlign.HAlignRight, false);
                oRU.SetText(ref sheet, startXlsRow - 4, cdName + 1, dslocal.Tables[0].Rows[0]["MotherName"].ToString(), ExcelHAlign.HAlignLeft);

                oRU.SetHeaderText(ref sheet, startXlsRow - 3, caName, "Date of Birth", ExcelHAlign.HAlignRight, false);
                oRU.SetText(ref sheet, startXlsRow - 3, caName + 1, dslocal.Tables[0].Rows[0]["DOB"].ToString(), ExcelHAlign.HAlignLeft);

                oRU.SetHeaderText(ref sheet, startXlsRow - 3, cdName, "Date of Join", ExcelHAlign.HAlignRight, false);
                oRU.SetText(ref sheet, startXlsRow - 3, cdName + 1, dslocal.Tables[0].Rows[0]["DOJ"].ToString(), ExcelHAlign.HAlignLeft);

                oRU.SetHeaderText(ref sheet, startXlsRow - 2, caName, "Reporting Person", ExcelHAlign.HAlignRight, false);
                oRU.SetText(ref sheet, startXlsRow - 2, caName + 1, dslocal.Tables[0].Rows[0]["ReportingPerson"].ToString(), ExcelHAlign.HAlignLeft);

                oRU.SetHeaderText(ref sheet, startXlsRow - 2, cdName, "Company", ExcelHAlign.HAlignRight, false);
                oRU.SetText(ref sheet, startXlsRow - 2, cdName + 1, dslocal.Tables[0].Rows[0]["Company"].ToString(), ExcelHAlign.HAlignLeft);
                //sheet.Range[oRU.GetColumnNameForXls(caName + 1) + (startXlsRow - 5) + ":" + oRU.GetColumnNameForXls(clCurrent + 4) + (startXlsRow - 5)].Merge();

                //Header End

                //Summary
                oRU.SetText(ref sheet, xlsRow, cSR, "Total Activity: ", true);
                //string Formula_L = "=sum(" + oRU.GetColumnNameForXls(caName) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(caName) + (xlsRow - 1) + ")";
                oRU.SetText(ref sheet, xlsRow, caName, _index.ToString(), true);
                //Documents

                oRU.SetText(ref sheet, xlsRow, caDetail, "Total Documents: ", true);
                //string Formula_Doc = "=sum(" + oRU.GetColumnNameForXls(cdName) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(cdName) + (xlsRow - 1) + ")";
                oRU.SetText(ref sheet, xlsRow, cdName, TotalDocuments.ToString(), true);
                //string Formula_File = "=sum(" + oRU.GetColumnNameForXls(cdDetail) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(cdDetail) + (xlsRow - 1) + ")";
                oRU.SetText(ref sheet, xlsRow, cdDetail, TotalFile.ToString(), true);
                //KPI
                oRU.SetText(ref sheet, xlsRow, ckName, "Total KPI: ", true);
                //string Formula_KPI = "=sum(" + oRU.GetColumnNameForXls(ckDetail) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(ckDetail) + (xlsRow - 1) + ")";
                oRU.SetText(ref sheet, xlsRow, ckDetail, TotalKPI.ToString(), true);

                //sheet2.Range[(Row_Total_Start), 1, xlsRow, shet2EndxlsCol].BorderAround(ExcelLineStyle.Thin);
                sheet.Range[(Row_Total_Start), 1, xlsRow, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                xlsRow = xlsRow + 2;

                #endregion Sheet2 Data

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                oRU.CompanyGroupHeaderPhoenix(ref sheet, shet2EndxlsCol, SheetHeader, status.CompanyGroupId);
                oRU.FreezePage(ref sheet, 3, startXlsRow + 1);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape, status.EmployeeName);
                sheet.Name = SheetName;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Individual
    }
}