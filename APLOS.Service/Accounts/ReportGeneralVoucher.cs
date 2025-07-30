using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace Library.Service.Accounts
{
    public class ReportGeneralVoucher
    {
        private readonly ISqlRepository _sqlRepository;

        public ReportGeneralVoucher()
        {
            _sqlRepository = new SqlRepository();
        }

        public IWorkbook GL_Voucher(ref ExcelEngine excelEngine, string masterid)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();

                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheet_VG(ref sheet1, oRU, "General Voucher", "GLV_" + masterid, masterid);
                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_VG(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, string masterid)
        {
            var _Currency = string.Empty;
            var _CurrencyId = string.Empty;
            DataView dvVoucher = null;
            DataTable dtVoucher = null;

            var xlsRow = 1;
            var xlsCol = 1;
            var startXlsRow = 1;
            var shet2EndxlsCol = 1;
            const int _MasterHeaderTextLeft = 1;
            const int _MasterHeaderValueLeft = 3;
            const int _MasterHeaderTextRight = 6;
            const int _MasterHeaderValueRight = 8;
            try
            {
                #region Data

                var dslocal = GetVoucherInfo(masterid);
                using (dvVoucher = new DataView(dslocal.Tables[0]))
                {
                    dtVoucher = dvVoucher.ToTable(true, "Id", "VoucherNo", "Vouchertype", "VoucherDate", "Narration", "PostingDate", "DocRefNo", "DocDate", "CurrencyId", "Currency");
                    if (dtVoucher.Rows.Count == 0)
                    {
                        throw new Exception("No Voucher Found !!!");
                    }
                    _Currency = dtVoucher.Rows[0]["Currency"].ToString();
                    _CurrencyId = dtVoucher.Rows[0]["CurrencyId"].ToString();//CurrencyId

                    #endregion Data

                    #region Sheet2 Data

                    xlsRow = 5;
                    startXlsRow = xlsRow;
                    sheet.Range[oRU.GetColumnNameForXls(1) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderValueRight + 1) + xlsRow].Merge();

                    xlsRow += 1;

                    ///master header

                    #region Row1

                    oRU.SetMasterHeaderText(ref sheet, xlsRow, _MasterHeaderTextLeft, "Voucher Type");
                    sheet.Range[oRU.GetColumnNameForXls(_MasterHeaderTextLeft) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderTextLeft + 1) + xlsRow].Merge();
                    ///value
                    oRU.SetText(ref sheet, xlsRow, _MasterHeaderValueLeft, dtVoucher.Rows[0]["VoucherType"].ToString(), ExcelHAlign.HAlignLeft);
                    sheet.Range[oRU.GetColumnNameForXls(_MasterHeaderValueLeft) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderValueLeft + 1) + xlsRow].Merge();

                    ///text
                    oRU.SetMasterHeaderText(ref sheet, xlsRow, _MasterHeaderTextRight, "Posting Date");
                    sheet.Range[oRU.GetColumnNameForXls(_MasterHeaderTextRight) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderTextRight + 1) + xlsRow].Merge();
                    ///value
                    oRU.SetText(ref sheet, xlsRow, _MasterHeaderValueRight, dtVoucher.Rows[0]["PostingDate"].ToString(), ExcelHAlign.HAlignLeft);
                    sheet.Range[oRU.GetColumnNameForXls(_MasterHeaderValueRight) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderValueRight + 1) + xlsRow].Merge();
                    xlsRow += 1;

                    #endregion Row1

                    #region Row2

                    xlsCol = 1;
                    oRU.SetMasterHeaderText(ref sheet, xlsRow, _MasterHeaderTextLeft, "Voucher No");
                    sheet.Range[oRU.GetColumnNameForXls(_MasterHeaderTextLeft) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderTextLeft + 1) + xlsRow].Merge();
                    ///value
                    xlsCol = 3;
                    oRU.SetText(ref sheet, xlsRow, _MasterHeaderValueLeft, dtVoucher.Rows[0]["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft);
                    sheet.Range[oRU.GetColumnNameForXls(_MasterHeaderValueLeft) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderValueLeft + 1) + xlsRow].Merge();

                    ///text
                    xlsCol = 5;
                    oRU.SetMasterHeaderText(ref sheet, xlsRow, _MasterHeaderTextRight, "Voucher Date");
                    sheet.Range[oRU.GetColumnNameForXls(_MasterHeaderTextRight) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderTextRight + 1) + xlsRow].Merge();
                    ///value
                    xlsCol = 7;
                    oRU.SetText(ref sheet, xlsRow, _MasterHeaderValueRight, dtVoucher.Rows[0]["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft);
                    sheet.Range[oRU.GetColumnNameForXls(_MasterHeaderValueRight) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderValueRight + 1) + xlsRow].Merge();
                    xlsRow += 1;

                    #endregion Row2

                    #region Row3

                    xlsCol = 1;
                    oRU.SetMasterHeaderText(ref sheet, xlsRow, _MasterHeaderTextLeft, "Narration");
                    sheet.Range[oRU.GetColumnNameForXls(_MasterHeaderTextLeft) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderTextLeft + 1) + xlsRow].Merge();
                    ///value
                    xlsCol = 3;
                    oRU.SetText(ref sheet, xlsRow, _MasterHeaderValueLeft, dtVoucher.Rows[0]["Narration"].ToString(), ExcelHAlign.HAlignLeft);
                    sheet.Range[oRU.GetColumnNameForXls(_MasterHeaderValueLeft) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderValueRight + 1) + xlsRow].Merge();
                    xlsRow += 1;

                    #endregion Row3

                    #region Row4

                    xlsCol = 1;
                    oRU.SetMasterHeaderText(ref sheet, xlsRow, _MasterHeaderTextLeft, "Currency");
                    sheet.Range[oRU.GetColumnNameForXls(_MasterHeaderTextLeft) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderTextLeft + 1) + xlsRow].Merge();
                    ///value
                    xlsCol = 3;
                    oRU.SetText(ref sheet, xlsRow, _MasterHeaderValueLeft, dtVoucher.Rows[0]["Currency"].ToString(), ExcelHAlign.HAlignLeft);
                    sheet.Range[oRU.GetColumnNameForXls(_MasterHeaderValueLeft) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderValueLeft + 1) + xlsRow].Merge();

                    #endregion Row4

                    xlsRow += 1;
                    sheet.Range[oRU.GetColumnNameForXls(1) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderValueRight + 1) + xlsRow].Merge();

                    xlsRow += 1;//Header

                    #region Detail Header

                    xlsCol = 1;
                    var cACCode = xlsCol;
                    oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "GL Code", 10);
                    xlsCol = xlsCol + 1;

                    var cACDescription = xlsCol;
                    oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "GL Description");
                    sheet.Range[oRU.GetColumnNameForXls(xlsCol) + xlsRow + ":" + oRU.GetColumnNameForXls(xlsCol + 1) + xlsRow].Merge();
                    xlsCol = xlsCol + 2;
                    var cNarration = xlsCol;
                    oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Narration");
                    sheet.Range[oRU.GetColumnNameForXls(xlsCol) + xlsRow + ":" + oRU.GetColumnNameForXls(xlsCol + 1) + xlsRow].Merge();
                    xlsCol = xlsCol + 2;
                    var cDocRefNo = xlsCol;
                    oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Doc Ref No");
                    xlsCol = xlsCol + 1;
                    var cDocDate = xlsCol;
                    oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Doc Date", 10);
                    xlsCol = xlsCol + 1;
                    var cDebit = xlsCol;
                    oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Debit", ExcelHAlign.HAlignRight);
                    xlsCol = xlsCol + 1;
                    var cCredit = xlsCol;
                    oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Credit", ExcelHAlign.HAlignRight);
                    //sheet2.Range[GetColumnNameForXls(xlsCol) + (xlsRow - 1) + ":" + GetColumnNameForXls(xlsCol) + xlsRow].Merge();
                    shet2EndxlsCol = xlsCol;

                    xlsRow += 1;
                    //sheet2.Range[(xlsRow - 2), cACCode, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);

                    #endregion Detail Header

                    //xlsRow += 1;
                    var Row_Total_Start = xlsRow;
                    double _Total_Amount = 0;
                    for (int icount = 0; icount < dslocal.Tables[0].Rows.Count; icount++)
                    {
                        oRU.SetText(ref sheet, xlsRow, cACCode, dslocal.Tables[0].Rows[icount]["AccountCode"].ToString());
                        oRU.SetText(ref sheet, xlsRow, cACDescription, dslocal.Tables[0].Rows[icount]["Description"].ToString());
                        sheet.Range[oRU.GetColumnNameForXls(cACDescription) + xlsRow + ":" + oRU.GetColumnNameForXls(cACDescription + 1) + xlsRow].Merge();
                        oRU.SetText(ref sheet, xlsRow, cNarration, dslocal.Tables[0].Rows[icount]["dNarration"].ToString());
                        sheet.Range[oRU.GetColumnNameForXls(cNarration) + xlsRow + ":" + oRU.GetColumnNameForXls(cNarration + 1) + xlsRow].Merge();
                        oRU.SetText(ref sheet, xlsRow, cDocRefNo, dslocal.Tables[0].Rows[icount]["dDocRefNo"].ToString());
                        oRU.SetText(ref sheet, xlsRow, cDocDate, dslocal.Tables[0].Rows[icount]["dDocDate"].ToString());
                        oRU.SetText(ref sheet, xlsRow, cDebit, Convert.ToDouble(dslocal.Tables[0].Rows[icount]["DrAmount"].ToString()));
                        oRU.SetText(ref sheet, xlsRow, cCredit, Convert.ToDouble(dslocal.Tables[0].Rows[icount]["CrAmount"].ToString()));
                        _Total_Amount += Convert.ToDouble(dslocal.Tables[0].Rows[icount]["DrAmount"].ToString());
                        xlsRow += 1;
                    }

                    #region Total

                    sheet.Range[oRU.GetColumnNameForXls(1) + xlsRow + ":" + oRU.GetColumnNameForXls(cDocRefNo) + xlsRow].Merge();

                    sheet.Range[xlsRow, cDocDate].Text = "Total ";
                    sheet.Range[xlsRow, cDocDate].CellStyle.Font.Bold = true;
                    sheet.Range[xlsRow, cDocDate].BorderAround(ExcelLineStyle.Hair);
                    //DR
                    sheet.Range[xlsRow, cDebit].Formula = "=SUM(" + oRU.GetColumnNameForXls(cDebit) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(cDebit) + (xlsRow - 1) + ")";
                    sheet.Range[xlsRow, cDebit].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[xlsRow, cDebit].CellStyle.Font.Bold = true;
                    sheet.Range[xlsRow, cDebit].BorderAround(ExcelLineStyle.Hair);
                    //CR
                    sheet.Range[xlsRow, cCredit].Formula = "=SUM(" + oRU.GetColumnNameForXls(cCredit) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(cCredit) + (xlsRow - 1) + ")";
                    sheet.Range[xlsRow, cCredit].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[xlsRow, cCredit].CellStyle.Font.Bold = true;
                    sheet.Range[xlsRow, cCredit].BorderAround(ExcelLineStyle.Hair);

                    #endregion Total

                    //border
                    //sheet2.Range[(Row_Total_Start), 1, xlsRow, shet2EndxlsCol].BorderAround(ExcelLineStyle.Thin);
                    sheet.Range[Row_Total_Start, 1, xlsRow, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);

                    #region InWord

                    var _amount = oRU.InWord(_Total_Amount, _CurrencyId);
                    xlsRow += 1;
                    xlsCol = 1;
                    sheet.Range[oRU.GetColumnNameForXls(xlsCol) + xlsRow].Text = _amount;
                    sheet.Range[oRU.GetColumnNameForXls(xlsCol) + xlsRow + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + xlsRow].Merge();
                    sheet.Range[oRU.GetColumnNameForXls(xlsCol) + xlsRow].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[oRU.GetColumnNameForXls(xlsCol) + xlsRow].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[oRU.GetColumnNameForXls(xlsCol) + xlsRow].CellStyle.Font.Bold = true;

                    #endregion InWord

                    xlsRow = xlsRow + 6;

                    #region Signature

                    sheet.Range[xlsRow, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                    sheet.Range[xlsRow, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                    sheet.Range[xlsRow, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                    sheet.Range[xlsRow, 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                    sheet.Range[xlsRow, 9].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

                    oRU.SetText(ref sheet, xlsRow, 1, "Received By"); xlsCol += 1;
                    oRU.SetText(ref sheet, xlsRow, 3, "Prepared By"); xlsCol += 1;
                    oRU.SetText(ref sheet, xlsRow, 5, "Checked By"); xlsCol += 1;
                    oRU.SetText(ref sheet, xlsRow, 7, "HOD (Finance)"); xlsCol += 1;
                    oRU.SetText(ref sheet, xlsRow, 9, "CEO/Director"); xlsCol += 1;

                    #endregion Signature

                    #endregion Sheet2 Data

                    sheet.UsedRange.WrapText = true;
                    sheet.UsedRange.CellStyle.Font.Size = 8;
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    oRU.CompanyHeader(ref sheet, shet2EndxlsCol, SheetHeader, identity.CompanyId);
                    //oRU.FreezePage(ref sheet, 1, 5);
                    oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                    sheet.Name = SheetName;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetVoucherInfo(string masterid)
        {
            GridParameter parameters = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"
                                    SELECT v.Id
	                                    ,v.VoucherNo
	                                    --,v.VoucherDate
                                        ,Replace(CONVERT(VARCHAR(11), v.VoucherDate, 106), ' ', '-') VoucherDate
                                        ,Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate
	                                    ,v.Narration
	                                    ,v.TransactionRefNo
	                                    --,v.PostingDate
	                                    ,v.DocRefNo
	                                    --,v.DocDate
                                        ,Replace(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') DocDate
	                                    ,v.CurrencyId
	                                    ,d.Narration dNarration
	                                    ,d.DocRefNo dDocRefNo
	                                    --,d.DocDate dDocDate
                                        ,Replace(CONVERT(VARCHAR(11), d.DocDate, 106), ' ', '-') dDocDate
                                        ,isnull(d.DrAmount,0) DrAmount
                                        ,isnull(d.CrAmount,0) CrAmount
	                                    ,g.AccountCode
	                                    ,g.[Description]
	                                    ,c.[Name] Currency
                                        ,t.UserName Vouchertype
                                    FROM [TRN].[Voucher] v
                                    LEFT JOIN [TRN].[VoucherDetail] d ON d.VoucherId = v.Id
                                    LEFT JOIN [HKP].[GLGeneralInfo] g ON d.GLGeneralInfoId = g.Id
                                    LEFT JOIN [SCS].[Currency] c ON v.CurrencyId = c.Id
                                    left outer join [SCS].[VoucherType] t on v.VoucherTypeId=t.Id
                                    WHERE v.Id = '" + masterid + @"'
                                        ";
                //ORDER BY bd.[Sequence]
                //                         ,op.[Sequence]
                //                         ,z.Code
                //                         ,c.[Sequence]
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook Coa_Report(ref ExcelEngine excelEngine)
        {
            DataView dvVoucher = null;
            DataTable dtVoucher = null;
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];

                #region data

                var dslocal = GetCOA();
                dvVoucher = new DataView(dslocal.Tables[0]);
                dtVoucher = dvVoucher.ToTable(true, "Code", "UserName", "LengthOfGL");
                if (dtVoucher.Rows.Count == 0)
                {
                    throw new Exception("No Coa Found !!!");
                }

                #endregion data

                sheet1.Name = "Coa";
                sheet1["A1"].Text = "Chart Of Account";
                sheet1["A1"].CellStyle.ColorIndex = ExcelKnownColors.Red;
                sheet1["A1:C3"].Merge();
                var _row = 5;
                sheet1["A" + _row].Text = "Code";
                sheet1["B" + _row].Text = "UserName";
                sheet1["C" + _row].Text = "LengthOfGL";
                for (int i = 0; i < dtVoucher.Rows.Count; i++)
                {
                    _row++;
                    sheet1["A" + _row].Text = dtVoucher.Rows[i]["Code"].ToString();
                    sheet1["B" + _row].Text = dtVoucher.Rows[i]["UserName"].ToString();
                    sheet1["C" + _row].Text = dtVoucher.Rows[i]["LengthOfGL"].ToString();
                }

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetCOA()
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @" SELECT Code,UserName,LengthOfGL FROM [HKP].[COA]";
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GL_DateRangeWise(ref ExcelEngine excelEngine, string gLId, string fromDate, string toDate)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                var dsLocal = GetGLInfo(gLId, fromDate, toDate);

                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheet_GL(ref sheet1, oRU, "SheetHeader", "SheetName", dsLocal, fromDate, toDate);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void AccumulativeAmountCalculation(string BalanceType, double DrAmount, double CrAmount, ref double AccumulativeValue)
        {
            if (BalanceType.ToUpper() == "CREDIT")
            {
                AccumulativeValue += CrAmount - DrAmount;
            }
            else
            {
                AccumulativeValue += DrAmount - CrAmount;
            }
        }

        private static string GetBalancetype(string balanceType, double accumulativeValue)
        {
            var _r = balanceType;
            if (accumulativeValue < 0)
                _r = balanceType.ToUpper() == "CREDIT" ? "Debit" : "Credit";
            return _r;
        }

        private void CreateSheet_GL(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dslocal, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var _Currency = string.Empty;
            var _CurrencyId = string.Empty;
            var _GL = string.Empty;
            var _Balancetype = string.Empty;
            var _GLCode = string.Empty;
            DataView dvLocal = null;
            DataTable dtLocal = null;

            var xlsRow = 1;
            var xlsCol = 1;
            var startXlsRow = 1;
            var shet2EndxlsCol = 1;
            var _MasterHeaderTextLeft = 1;
            var _MasterHeaderValueLeft = 3;
            var _MasterHeaderValueRight = 8;
            try
            {
                #region Data

                //DataSet dslocal = GetVoucherInfo(masterid);GLGeneralInfoId
                dvLocal = new DataView(dslocal.Tables[0])
                {
                    Sort = "PostingDateSort, AddedDate"
                };
                dtLocal = dvLocal.ToTable();
                if (dtLocal.Rows.Count == 0)
                {
                    throw new Exception("No Voucher Found !!!");
                }
                var dsOpenning = GetGLOpenning(dtLocal.Rows[0]["GLGeneralInfoId"].ToString(), fromDate);

                _Currency = dtLocal.Rows[0]["Currency"].ToString();
                _CurrencyId = dtLocal.Rows[0]["CurrencyId"].ToString();//CurrencyId
                _GL = dtLocal.Rows[0]["GL"].ToString();
                _Balancetype = dtLocal.Rows[0]["Balancetype"].ToString();//GLGeneralInfoCode
                _GLCode = dtLocal.Rows[0]["GLGeneralInfoCode"].ToString();//

                #endregion Data

                #region Sheet2 Data

                xlsRow = 5;
                startXlsRow = xlsRow;
                sheet.Range[oRU.GetColumnNameForXls(1) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderValueRight + 1) + xlsRow].Merge();

                xlsRow += 1;

                ///master header

                #region Row1

                oRU.SetMasterHeaderText(ref sheet, xlsRow, _MasterHeaderTextLeft, "GL");
                sheet.Range[oRU.GetColumnNameForXls(_MasterHeaderTextLeft) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderTextLeft + 1) + xlsRow].Merge();
                ///value
                oRU.SetText(ref sheet, xlsRow, _MasterHeaderValueLeft, _GL + " (" + _GLCode + ")", ExcelHAlign.HAlignLeft);
                sheet.Range[oRU.GetColumnNameForXls(_MasterHeaderValueLeft) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderValueLeft + 1) + xlsRow].Merge();
                xlsRow += 1;

                #endregion Row1

                #region Row2

                xlsCol = 1;
                oRU.SetMasterHeaderText(ref sheet, xlsRow, _MasterHeaderTextLeft, "Date Range");
                sheet.Range[oRU.GetColumnNameForXls(_MasterHeaderTextLeft) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderTextLeft + 1) + xlsRow].Merge();
                ///value
                xlsCol = 3;
                oRU.SetText(ref sheet, xlsRow, _MasterHeaderValueLeft, "[" + fromDate + "] To [" + toDate + "]", ExcelHAlign.HAlignLeft);
                sheet.Range[oRU.GetColumnNameForXls(_MasterHeaderValueLeft) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderValueLeft + 1) + xlsRow].Merge();
                xlsRow += 1;

                #endregion Row2

                xlsRow += 1;
                sheet.Range[oRU.GetColumnNameForXls(1) + xlsRow + ":" + oRU.GetColumnNameForXls(_MasterHeaderValueRight + 1) + xlsRow].Merge();

                xlsRow += 1;//Header

                #region Detail Header

                xlsCol = 1;
                var cPostingDate = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Posting Date", 12);
                // sheet2.Range[GetColumnNameForXls(xlsCol) + (xlsRow - 1) + ":" + GetColumnNameForXls(xlsCol) + xlsRow].Merge();
                xlsCol = xlsCol + 1;

                var cVoucherNo = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Voucher No");
                // sheet.Range[oRU.GetColumnNameForXls(xlsCol) + (xlsRow) + ":" + oRU.GetColumnNameForXls(xlsCol + 1) + xlsRow].Merge();
                xlsCol = xlsCol + 1;
                var cVoucherDate = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Voucher Date");
                //sheet.Range[oRU.GetColumnNameForXls(xlsCol) + (xlsRow) + ":" + oRU.GetColumnNameForXls(xlsCol + 1) + xlsRow].Merge();
                xlsCol = xlsCol + 1;
                var cDocRefNo = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Doc Ref No");
                //sheet2.Range[GetColumnNameForXls(xlsCol) + (xlsRow - 1) + ":" + GetColumnNameForXls(xlsCol) + xlsRow].Merge();
                xlsCol = xlsCol + 1;
                var cDocDate = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Doc Date", 10);
                //
                //xlsCol = xlsCol + 1;
                //int cParty = xlsCol;
                //oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Party", 20);
                //
                xlsCol = xlsCol + 1;
                var cCurrency = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Currency", 10);
                //sheet2.Range[GetColumnNameForXls(xlsCol) + (xlsRow - 1) + ":" + GetColumnNameForXls(xlsCol) + xlsRow].Merge();
                xlsCol = xlsCol + 1;
                var cDebit = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Debit", ExcelHAlign.HAlignRight);
                //sheet2.Range[GetColumnNameForXls(xlsCol) + (xlsRow - 1) + ":" + GetColumnNameForXls(xlsCol) + xlsRow].Merge();
                xlsCol = xlsCol + 1;
                var cCredit = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Credit", ExcelHAlign.HAlignRight);

                xlsCol = xlsCol + 1;
                var cCumulative = xlsCol;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Balance", ExcelHAlign.HAlignRight);
                //sheet2.Range[GetColumnNameForXls(xlsCol) + (xlsRow - 1) + ":" + GetColumnNameForXls(xlsCol) + xlsRow].Merge();
                shet2EndxlsCol = xlsCol;

                xlsRow += 1;
                //sheet2.Range[(xlsRow - 2), cACCode, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);

                #endregion Detail Header

                //xlsRow += 1;
                var Row_Total_Start = xlsRow;
                double _Cumulative_Amount = 0;

                double _OpeningDr = 0;
                double _OpeningCr = 0;
                if (dsOpenning.Tables[0].Rows.Count > 0)
                {
                    _OpeningDr = Convert.ToDouble(dsOpenning.Tables[0].Rows[0]["DrAmount"].ToString());
                    _OpeningCr = Convert.ToDouble(dsOpenning.Tables[0].Rows[0]["CrAmount"].ToString());
                }
                AccumulativeAmountCalculation(_Balancetype, _OpeningDr, _OpeningCr, ref _Cumulative_Amount);

                oRU.SetText(ref sheet, xlsRow, cVoucherNo, "Opening Balance", true);

                //oRU.SetText(ref sheet, xlsRow, cDebit, _OpeningDr);
                //oRU.SetText(ref sheet, xlsRow, cCredit, _OpeningCr);
                oRU.SetText(ref sheet, xlsRow, cCumulative, _Cumulative_Amount, true);
                xlsRow += 1;
                for (int icount = 0; icount < dtLocal.Rows.Count; icount++)
                {
                    oRU.SetText(ref sheet, xlsRow, cPostingDate, dtLocal.Rows[icount]["PostingDate"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cVoucherNo, dtLocal.Rows[icount]["VoucherNo"].ToString());
                    //sheet.Range[oRU.GetColumnNameForXls(cACDescription) + (xlsRow) + ":" + oRU.GetColumnNameForXls(cACDescription + 1) + xlsRow].Merge();
                    oRU.SetText(ref sheet, xlsRow, cVoucherDate, dtLocal.Rows[icount]["VoucherDate"].ToString());
                    //sheet.Range[oRU.GetColumnNameForXls(cNarration) + (xlsRow) + ":" + oRU.GetColumnNameForXls(cNarration + 1) + xlsRow].Merge();
                    oRU.SetText(ref sheet, xlsRow, cDocRefNo, dtLocal.Rows[icount]["dDocRefNo"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cDocDate, dtLocal.Rows[icount]["dDocDate"].ToString());
                    //oRU.SetText(ref sheet, xlsRow, cParty, dtLocal.Rows[icount]["Party"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cCurrency, dtLocal.Rows[icount]["Currency"].ToString());
                    var _dr = Convert.ToDouble(dtLocal.Rows[icount]["DrAmount"].ToString());
                    var _cr = Convert.ToDouble(dtLocal.Rows[icount]["CrAmount"].ToString());
                    oRU.SetText(ref sheet, xlsRow, cDebit, _dr);
                    oRU.SetText(ref sheet, xlsRow, cCredit, _cr);
                    AccumulativeAmountCalculation(_Balancetype, _dr, _cr, ref _Cumulative_Amount);
                    oRU.SetText(ref sheet, xlsRow, cCumulative, _Cumulative_Amount);
                    xlsRow += 1;
                }

                #region Total

                oRU.SetText(ref sheet, xlsRow, cVoucherNo, "Closing Balance", true);
                sheet.Range[oRU.GetColumnNameForXls(cVoucherNo) + xlsRow + ":" + oRU.GetColumnNameForXls(cVoucherNo + 1) + xlsRow].Merge();

                oRU.SetText(ref sheet, xlsRow, cCumulative, _Cumulative_Amount, true, ExcelLineStyle.Hair);
                // sheet.Range[xlsRow, cCumulative].BorderAround(ExcelLineStyle.Hair);

                #endregion Total

                sheet.Range[Row_Total_Start, 1, xlsRow, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);

                #region InWord

                var _amount = oRU.InWord(_Cumulative_Amount, _CurrencyId);
                xlsRow += 1;
                xlsCol = 1;
                sheet.Range[oRU.GetColumnNameForXls(xlsCol) + xlsRow].Text = "In Words (" + GetBalancetype(_Balancetype, _Cumulative_Amount) + " Balance) : " + _amount;
                sheet.Range[oRU.GetColumnNameForXls(xlsCol) + xlsRow + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + xlsRow].Merge();
                sheet.Range[oRU.GetColumnNameForXls(xlsCol) + xlsRow].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[oRU.GetColumnNameForXls(xlsCol) + xlsRow].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[oRU.GetColumnNameForXls(xlsCol) + xlsRow].CellStyle.Font.Bold = true;

                #endregion InWord

                xlsRow = xlsRow + 6;

                #endregion Sheet2 Data

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                oRU.CompanyHeader(ref sheet, shet2EndxlsCol, "GL-" + _GL, identity.CompanyId);
                //oRU.FreezePage(ref sheet, 1, 5);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                sheet.Name = _GL;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetGLInfo(string gLId, string fromDate, string toDate)
        {
            GridParameter parameters = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"
                                    SELECT v.Id
	                                    ,v.VoucherNo
	                                    --,v.VoucherDate
                                        ,Replace(CONVERT(VARCHAR(11), v.VoucherDate, 106), ' ', '-') VoucherDate
                                        ,Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate
	                                    ,v.Narration
	                                    ,v.PostingDate PostingDateSort
	                                    ,v.DocRefNo
	                                    --,v.DocDate
                                        ,Replace(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') DocDate
	                                    ,v.CurrencyId
	                                    ,d.Narration dNarration
	                                    ,d.DocRefNo dDocRefNo
	                                    --,d.DocDate dDocDate
                                        ,Replace(CONVERT(VARCHAR(11), d.DocDate, 106), ' ', '-') dDocDate
	                                    ,isnull(d.DrAmount,0) DrAmount
	                                    ,isnull(d.CrAmount,0) CrAmount
	                                    ,g.AccountCode
	                                    ,g.[Description]
	                                    ,c.[Name] Currency
                                        ,t.UserName Vouchertype
                                        ,i.AccountCode AS GLGeneralInfoCode
										,i.UserName GL
                                        ,d.GLGeneralInfoId
										--,p.UserName Party
										,a.BalanceType
                                        ,d.AddedDate
                                    FROM [TRN].[Voucher] v
                                    LEFT JOIN [TRN].[VoucherDetail] d ON d.VoucherId = v.Id
                                    LEFT JOIN [HKP].[GLGeneralInfo] g ON d.GLGeneralInfoId = g.Id
                                    LEFT JOIN [SCS].[Currency] c ON v.CurrencyId = c.Id
                                    left outer join [SCS].[VoucherType] t on v.VoucherTypeId=t.Id
                                    left outer join [HKP].[GLGeneralInfo] i on d.GLGeneralInfoId=i.Id
                                    left outer join [HKP].[AccountGroup] a on i.AccountGroupId=a.Id
                                    --left outer join [HKP].[Party] p on v.PartyId=p.Id
                                    where d.GLGeneralInfoId='GGI147'
                                    and v.PostingDate between '" + fromDate.ToDbDate() + @"' and '" + toDate.ToDbDate() + @"'
                                ";
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetGLOpenning(string gLId, string fromDate)
        {
            GridParameter parameters = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                var _sql = @"
                                     SELECT
	                                     isnull(sum(isnull(d.DrAmount,0)),0) DrAmount
	                                    ,isnull(sum(isnull(d.CrAmount,0)),0) CrAmount
                                    FROM [TRN].[Voucher] v
                                    LEFT JOIN [TRN].[VoucherDetail] d ON d.VoucherId = v.Id
                                    where d.GLGeneralInfoId='" + gLId + @"'
                                    and v.PostingDate < '" + fromDate.ToDbDate() + @"'
                                ";
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetFixedAssetMaster()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @" SELECT FA.Id FixedAssetId, CLS.Id FixedAssetClassId,SCLS.Id FixedAssetSubClassId,CAT.Id FixedAssetCategoryId,SCAT.Id FixedAssetSubCategoryId,CLS.UserName AS Class, SCLS.UserName AS SubClass,
                                 CAT.UserName AS Category, SCAT.UserName AS SubCategory, FA.UserName AS Asset,FAM.Id AS FixedAssetMasterId,FAM.UserName AS AssetMaster
								,GLGI1.AccountCode AS AssetGLCode
								,GLGI1.UserName AS  [AssetGL]
								,GLGI2.AccountCode AS AccDepreciationGLCode
	                            ,GLGI2.UserName AS AccDepreciationGL
	                            ,GLGI3.AccountCode DepreciationGLCode
	                            ,GLGI3.UserName AS DepreciationGL
	                            ,GLGI4.AccountCode AS AUCGLCode,GLGI4.UserName AS AUCGL
                                FROM MST.FixedAssetMaster AS FAM
                                LEFT OUTER JOIN HKP.FixedAssetClass AS CLS ON CLS.Id=FAM.FixedAssetClassId
                                LEFT OUTER JOIN HKP.FixedAssetSubClass AS SCLS ON SCLS.Id=FAM.FixedAssetSubClassId
                                LEFT OUTER JOIN HKP.FixedAssetCategory AS CAT ON CAT.Id=FAM.FixedAssetCategoryId
                                LEFT OUTER JOIN HKP.FixedAssetSubCategory AS SCAT ON SCAT.Id=FAM.FixedAssetSubCategoryId
                                LEFT OUTER JOIN HKP.FixedAsset AS FA ON FA.Id=FAM.FixedAssetId
								LEFT OUTER JOIN HKP.FixedAssetGL AS FAD ON FAD.FixedAssetMasterId=FAM.Id
								LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=FAD.FixedAssetGLId
								LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=FAD.AccumulatedDepreciationGLId
								LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=FAD.DepreciationGLId
								LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=FAD.AssetUnderConstructionGLId
                                WHERE FAM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND FAM.Active=1 ";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook FixedAssetMaster_Report(ExcelEngine excelEngine)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            IWorksheet sheet2 = null;
            try
            {
                oRU = new ReportUtility();

                workbook = oRU.GetWorkbook(ref excelEngine, 2);
                sheet1 = workbook.Worksheets[0];
                sheet2 = workbook.Worksheets[1];
                CreateSheetF1(ref sheet1, oRU, "Fixed Asset Master List", "Fixed Asset Master Report");
                CreateSheetF2(ref sheet2, oRU, "Fixed Asset Master List", "Fixed Asset Master Data");

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheetF1(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataTable dtFixedAssetMaster = null;

            #region List data

            var FixedAssetMasterList = GetFixedAssetMaster();
            dtFixedAssetMaster = FixedAssetMasterList.Tables[0];
            var dvFixedAssetClass = new DataView(FixedAssetMasterList.Tables[0]);
            var dtFixedAssetClass = dvFixedAssetClass.ToTable(true, "Class", "FixedAssetClassId");
            dvFixedAssetClass.Sort = "Class";

            DataView dvFixedAssetSubClass = null;
            DataTable dtFixedAssetSubClass = null;

            DataView dvFixedAssetCategory = null;
            DataTable dtFixedAssetCategory = null;

            DataView dvFixedAssetSubCategory = null;
            DataTable dtFixedAssetSubCategory = null;

            DataView dvFixedAsset = null;
            DataTable dtFixedAsset = null;

            DataView dvFixedAssetMaster = null;
            DataTable dtFixedAssetMst = null;

            if (dtFixedAssetMaster.Rows.Count == 0)
            {
                throw new Exception("No Data Found !!!");
            }

            #endregion List data

            var _col = 1;
            var _rowL = 5;
            var _colIndex = 0;
            var shet2EndxlsCol = _col;
            var classColIndex = 1;
            var subClassColIndex = 2;
            var categoryColIndex = 3;
            var subCategoryColIndex = 4;
            var fixedAssetColIndex = 5;
            var fixedAssetMasterColIndex = 6;

            for (int i = 0; i < dtFixedAssetMaster.Columns.Count; i++)
            {
                if (dtFixedAssetMaster.Columns[i].ColumnName != "TotalRows" && dtFixedAssetMaster.Columns[i].ColumnName != "FixedAssetMasterId" && dtFixedAssetMaster.Columns[i].ColumnName != "FixedAssetId" && dtFixedAssetMaster.Columns[i].ColumnName != "FixedAssetClassId" && dtFixedAssetMaster.Columns[i].ColumnName != "FixedAssetSubClassId" && dtFixedAssetMaster.Columns[i].ColumnName != "FixedAssetCategoryId" && dtFixedAssetMaster.Columns[i].ColumnName != "FixedAssetSubCategoryId")
                {
                    _colIndex++;
                    oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtFixedAssetMaster.Columns[i].ColumnName);
                }
            }
            shet2EndxlsCol = _colIndex;

            for (int m = 0; m < dtFixedAssetClass.Rows.Count; m++)
            {
                _rowL++;
                string classId = dtFixedAssetClass.Rows[m]["FixedAssetClassId"].ToString();
                dvFixedAssetSubClass = new DataView(dtFixedAssetMaster);
                dvFixedAssetClass.Sort = "SubClass";
                dvFixedAssetSubClass.RowFilter = "FixedAssetClassId='" + classId + "'";
                dtFixedAssetSubClass = dvFixedAssetSubClass.ToTable(true, "SubClass", "FixedAssetSubClassId");
                var rowStartClass = _rowL;
                oRU.SetText(ref sheet, _rowL, classColIndex, dtFixedAssetClass.Rows[m]["Class"].ToString(), 26);

                for (int n = 0; n < dtFixedAssetSubClass.Rows.Count; n++)
                {
                    var subClassId = dtFixedAssetSubClass.Rows[n]["FixedAssetSubClassId"].ToString();
                    dvFixedAssetCategory = new DataView(dtFixedAssetMaster);
                    dvFixedAssetClass.Sort = "Category";
                    dvFixedAssetCategory.RowFilter = "FixedAssetSubClassId='" + subClassId + "' and FixedAssetClassId='" + classId + "'";
                    dtFixedAssetCategory = dvFixedAssetCategory.ToTable(true, "Category", "FixedAssetCategoryId");
                    var rowStartSubClass = _rowL;
                    oRU.SetText(ref sheet, _rowL, subClassColIndex, dtFixedAssetSubClass.Rows[n]["SubClass"].ToString(), 26);

                    for (int o = 0; o < dtFixedAssetCategory.Rows.Count; o++)
                    {
                        string categoryId = dtFixedAssetCategory.Rows[o]["FixedAssetCategoryId"].ToString();
                        dvFixedAssetSubCategory = new DataView(dtFixedAssetMaster);
                        dvFixedAssetClass.Sort = "SubCategory";
                        dvFixedAssetSubCategory.RowFilter = "FixedAssetCategoryId='" + categoryId + "' and FixedAssetSubClassId='" + subClassId + "' and FixedAssetClassId='" + classId + "'";
                        dtFixedAssetSubCategory = dvFixedAssetSubCategory.ToTable(true, "SubCategory", "FixedAssetSubCategoryId");
                        var rowStartCategory = _rowL;
                        oRU.SetText(ref sheet, _rowL, categoryColIndex, dtFixedAssetCategory.Rows[o]["Category"].ToString(), 26);

                        for (int p = 0; p < dtFixedAssetSubCategory.Rows.Count; p++)
                        {
                            var subCategoryId = dtFixedAssetSubCategory.Rows[p]["FixedAssetSubCategoryId"].ToString();
                            dvFixedAsset = new DataView(dtFixedAssetMaster);
                            dvFixedAssetClass.Sort = "Asset";
                            dvFixedAsset.RowFilter = "FixedAssetSubCategoryId='" + subCategoryId + "' and FixedAssetCategoryId='" + categoryId + "' and FixedAssetSubClassId='" + subClassId + "' and FixedAssetClassId='" + classId + "'";
                            dtFixedAsset = dvFixedAsset.ToTable(true, "Asset", "FixedAssetId");
                            var rowStartSubCategory = _rowL;
                            oRU.SetText(ref sheet, _rowL, subCategoryColIndex, dtFixedAssetSubCategory.Rows[p]["SubCategory"].ToString(), 26);

                            for (int i = 0; i < dtFixedAsset.Rows.Count; i++)
                            {
                                string fixedAssetId = dtFixedAsset.Rows[i]["FixedAssetId"].ToString();
                                dvFixedAssetMaster = new DataView(dtFixedAssetMaster);
                                dvFixedAssetClass.Sort = "AssetMaster";
                                dvFixedAssetMaster.RowFilter = "FixedAssetId='" + fixedAssetId + "' and FixedAssetSubCategoryId='" + subCategoryId + "' and FixedAssetCategoryId='" + categoryId + "' and FixedAssetSubClassId='" + subClassId + "' and FixedAssetClassId='" + classId + "'";
                                dtFixedAssetMst = dvFixedAssetMaster.ToTable(true, "AssetMaster", "FixedAssetMasterId", "AssetGLCode", "AssetGL", "AccDepreciationGLCode", "AccDepreciationGL", "DepreciationGLCode", "DepreciationGL", "AUCGLCode", "AUCGL");
                                var rowStartFixedAsset = _rowL;
                                oRU.SetText(ref sheet, _rowL, fixedAssetColIndex, dtFixedAsset.Rows[i]["Asset"].ToString(), 26);

                                fixedAssetMasterColIndex = 5;
                                for (int q = 0; q < dtFixedAssetMst.Rows.Count; q++)
                                {
                                    fixedAssetMasterColIndex++;
                                    oRU.SetText(ref sheet, _rowL, fixedAssetMasterColIndex, dtFixedAssetMst.Rows[q]["AssetMaster"].ToString(), 26); fixedAssetMasterColIndex++;
                                    oRU.SetText(ref sheet, _rowL, fixedAssetMasterColIndex, dtFixedAssetMst.Rows[q]["AssetGLCode"].ToString()); fixedAssetMasterColIndex++;
                                    oRU.SetText(ref sheet, _rowL, fixedAssetMasterColIndex, dtFixedAssetMst.Rows[q]["AssetGL"].ToString(), 26); fixedAssetMasterColIndex++;
                                    oRU.SetText(ref sheet, _rowL, fixedAssetMasterColIndex, dtFixedAssetMst.Rows[q]["AccDepreciationGLCode"].ToString()); fixedAssetMasterColIndex++;
                                    oRU.SetText(ref sheet, _rowL, fixedAssetMasterColIndex, dtFixedAssetMst.Rows[q]["AccDepreciationGL"].ToString(), 26); fixedAssetMasterColIndex++;
                                    oRU.SetText(ref sheet, _rowL, fixedAssetMasterColIndex, dtFixedAssetMst.Rows[q]["DepreciationGLCode"].ToString()); fixedAssetMasterColIndex++;
                                    oRU.SetText(ref sheet, _rowL, fixedAssetMasterColIndex, dtFixedAssetMst.Rows[q]["DepreciationGL"].ToString(), 26); fixedAssetMasterColIndex++;
                                    oRU.SetText(ref sheet, _rowL, fixedAssetMasterColIndex, dtFixedAssetMst.Rows[q]["AUCGLCode"].ToString()); fixedAssetMasterColIndex++;
                                    oRU.SetText(ref sheet, _rowL, fixedAssetMasterColIndex, dtFixedAssetMst.Rows[q]["AUCGL"].ToString(), 26);
                                    _rowL++;
                                }
                                //columnIndex = 1;
                            }
                            sheet[rowStartSubCategory, subCategoryColIndex, _rowL - 1, subCategoryColIndex].Merge();
                        }//SubCategory
                        sheet[rowStartCategory, categoryColIndex, _rowL - 1, categoryColIndex].Merge();
                    }//Category
                    sheet[rowStartSubClass, subClassColIndex, _rowL - 1, subClassColIndex].Merge();
                }//SubClass
                sheet[rowStartClass, classColIndex, _rowL, classColIndex].Merge();
            }//Class

            sheet.Range[5, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Name = SheetName;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Fixed Asset Master List", identity.CompanyGroupId);
            oRU.FreezePage(ref sheet, 1, 5);
            oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
        }

        private void CreateSheetF2(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataTable dtFixedAssetMaster = null;

            #region List data

            var FixedAssetMasterList = GetFixedAssetMaster();
            dtFixedAssetMaster = FixedAssetMasterList.Tables[0];
            if (dtFixedAssetMaster.Rows.Count == 0)
            {
                throw new Exception("No Data Found !!!");
            }

            #endregion List data

            const int _col = 1;
            var _rowL = 5;
            var _colIndex = 0;
            var shet2EndxlsCol = _col;

            for (int i = 0; i < dtFixedAssetMaster.Columns.Count; i++)
            {
                if (dtFixedAssetMaster.Columns[i].ColumnName != "TotalRows" && dtFixedAssetMaster.Columns[i].ColumnName != "FixedAssetId" && dtFixedAssetMaster.Columns[i].ColumnName != "FixedAssetClassId" && dtFixedAssetMaster.Columns[i].ColumnName != "FixedAssetSubClassId" && dtFixedAssetMaster.Columns[i].ColumnName != "FixedAssetCategoryId" && dtFixedAssetMaster.Columns[i].ColumnName != "FixedAssetSubCategoryId")
                {
                    _colIndex++;
                    oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtFixedAssetMaster.Columns[i].ColumnName);
                }
            }
            shet2EndxlsCol = _colIndex;

            for (int i = 0; i < dtFixedAssetMaster.Rows.Count; i++)
            {
                _rowL++;

                oRU.SetText(ref sheet, _rowL, 1, dtFixedAssetMaster.Rows[i]["Class"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 2, dtFixedAssetMaster.Rows[i]["SubClass"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 3, dtFixedAssetMaster.Rows[i]["Category"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 4, dtFixedAssetMaster.Rows[i]["SubCategory"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 5, dtFixedAssetMaster.Rows[i]["Asset"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 6, dtFixedAssetMaster.Rows[i]["FixedAssetMasterId"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 7, dtFixedAssetMaster.Rows[i]["AssetMaster"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 8, dtFixedAssetMaster.Rows[i]["AssetGLCode"].ToString());
                oRU.SetText(ref sheet, _rowL, 9, dtFixedAssetMaster.Rows[i]["AssetGL"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 10, dtFixedAssetMaster.Rows[i]["AccDepreciationGLCode"].ToString());
                oRU.SetText(ref sheet, _rowL, 11, dtFixedAssetMaster.Rows[i]["AccDepreciationGL"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 12, dtFixedAssetMaster.Rows[i]["DepreciationGLCode"].ToString());
                oRU.SetText(ref sheet, _rowL, 13, dtFixedAssetMaster.Rows[i]["DepreciationGL"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 14, dtFixedAssetMaster.Rows[i]["AUCGLCode"].ToString());
                oRU.SetText(ref sheet, _rowL, 15, dtFixedAssetMaster.Rows[i]["AUCGL"].ToString(), 26);
            }

            sheet.Range[5, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Name = SheetName;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Fixed Asset Master List", identity.CompanyGroupId);
            oRU.FreezePage(ref sheet, 1, 5);
            oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
        }

        private DataSet GetCustomerCheckByCompany(string companyId)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @" SELECT IsVoucherFromBudget
		                                , IsBudgetPeriod
		                                , IsCostCenterApplicable
		                                , IsProfitCenterApplicable
		                                FROM [ORG].[Company]
		                                WHERE Id='" + companyId + @"' AND Active=1 AND Archive=0";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook IncomeStatement_Report(ExcelEngine excelEngine, string companyId, string plantId, string plantName, string date, string[] parallelCurrencies, bool isBudgetLevel, bool isActivityLevel)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                DataSet dsLocal = GetIncomeStatementInfo(companyId, plantId, date, parallelCurrencies, isBudgetLevel, isActivityLevel);
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheet_IncomeStatement(ref sheet1, oRU, "Income Statement", "Income Statement Report", dsLocal, companyId, plantId, plantName, date, parallelCurrencies, isBudgetLevel, isActivityLevel);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_IncomeStatement(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dslocal, string companyId, string plantId, string plantName, string date, string[] parallelCurrency, bool isBudgetLevel, bool isActivityLevel)
        {
            DataTable dtGeneralVoucher = null;
            DataTable dtCustomerCheckByCompany = null;
            DataTable dtMainBody;
            DataView dvDr;
            DataTable dtDr = null;
            DataView dvCr;
            DataTable dtCr = null;
            #region List data

            DataSet dsLocal = GetIncomeStatementInfo(companyId, plantId, date, parallelCurrency, isBudgetLevel, isActivityLevel);
            dtGeneralVoucher = dsLocal.Tables[0];

            DataSet CustomerCheckByCompanyList = GetCustomerCheckByCompany(companyId);
            dtCustomerCheckByCompany = CustomerCheckByCompanyList.Tables[0];

            if (dtGeneralVoucher.Rows.Count > 0)
            {
                DataView dvAccountCode = new DataView(dsLocal.Tables[0]);
                DataTable dtAccountCode;
                if (isBudgetLevel == true)
                {
                    dtAccountCode = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId", "BudgetMasterId");
                }
                if (isActivityLevel == true)
                {
                    dtAccountCode = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId", "BudgetMasterId", "ActivityId");
                }
                if (isActivityLevel == false && isBudgetLevel == false)
                {
                    dtAccountCode = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId");
                }

                DataView dvParallelCurrency = new DataView(dsLocal.Tables[0])
                {
                    Sort = "CurrencyCode ASC"
                };
                DataTable dtParallelCurrency = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");
                DataView dvMainBody = new DataView(dsLocal.Tables[0]);

                if (isBudgetLevel == true)
                {
                    dtMainBody = dvMainBody.ToTable(true, "GLGeneralInfoCode", "GL", "Budget", "BudgetMasterId");
                    dvDr = new DataView(dsLocal.Tables[0])
                    {
                        RowFilter = "MainHead='Expense'",
                        Sort = "GLGeneralInfoCode, GL, Budget"
                    };
                    dtDr = dvDr.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget");
                    dvCr = new DataView(dsLocal.Tables[0])
                    {
                        RowFilter = "MainHead='Revenue'",
                        Sort = "GLGeneralInfoCode, GL, Budget"
                    };
                    dtCr = dvCr.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget");

                }
                if (isActivityLevel == true)
                {
                    dtMainBody = dvMainBody.ToTable(true, "GLGeneralInfoCode", "GL", "Budget", "BudgetMasterId", "Activity", "ActivityId");
                    dvDr = new DataView(dsLocal.Tables[0])
                    {
                        RowFilter = "MainHead='Expense'",
                        Sort = "GLGeneralInfoCode, GL, Budget,Activity"
                    };
                    dtDr = dvDr.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget", "ActivityId", "Activity");
                    dvCr = new DataView(dsLocal.Tables[0])
                    {
                        RowFilter = "MainHead='Revenue'",
                        Sort = "GLGeneralInfoCode, GL, Budget,Activity"
                    };
                    dtCr = dvCr.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget", "ActivityId", "Activity");
                }
                if (isActivityLevel == false && isBudgetLevel == false)
                {
                    dtMainBody = dvMainBody.ToTable(true, "GLGeneralInfoCode", "GL");
                    dvDr = new DataView(dsLocal.Tables[0])
                    {
                        RowFilter = "MainHead='Expense'",
                        Sort = "GLGeneralInfoCode, GL"
                    };
                    dtDr = dvDr.ToTable(true, "GLGeneralInfoCode", "GL");
                    dvCr = new DataView(dsLocal.Tables[0])
                    {
                        RowFilter = "MainHead='Revenue'",
                        Sort = "GLGeneralInfoCode, GL"
                    };
                    dtCr = dvCr.ToTable(true, "GLGeneralInfoCode", "GL");
                }

                #region Customer Check By Company

                DataView dvCustomerCheckByCompanyBody = new DataView(CustomerCheckByCompanyList.Tables[0]);
                DataTable dtCustomerCheckByCompanyBody = dvCustomerCheckByCompanyBody.ToTable(false, "IsVoucherFromBudget");
                string Budget = dtCustomerCheckByCompanyBody.Rows[0]["IsVoucherFromBudget"].ToString();

                #endregion Customer Check By Company

                #endregion List data

                var _col = 1;
                var shet2EndxlsCol = _col;

                var _rowL = 6;
                _rowL++;

                var headreColIndex = 1;
                var mainColIndex = 1;

                oRU.SetHeaderText(ref sheet, _rowL, headreColIndex, "GL", 38); headreColIndex++;

                if (isBudgetLevel == true)
                {
                    oRU.SetHeaderText(ref sheet, _rowL, headreColIndex, nameof(Budget), 38); headreColIndex++;
                }
                if (isActivityLevel == true)
                {
                    oRU.SetHeaderText(ref sheet, _rowL, headreColIndex, nameof(Budget), 38); headreColIndex++;
                    oRU.SetHeaderText(ref sheet, _rowL, headreColIndex, "Activity", 38); headreColIndex++;
                }
                double _Total_Amount = 0;
                string plCurrencyId = string.Empty;
                string plCurrencyCode = string.Empty;

                ArrayList alParaCurrency = new ArrayList();

                for (int n = 0; n < dtParallelCurrency.Rows.Count; n++)
                {
                    oRU.SetHeaderText(ref sheet, _rowL, headreColIndex, dtParallelCurrency.Rows[n]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter); headreColIndex++;
                    Dictionary<string, int> dic = new Dictionary<string, int>
                {
                    { dtParallelCurrency.Rows[n]["ParallelCurrencyId"].ToString(), headreColIndex-1 }
                };
                    alParaCurrency.Add(dic);

                    if (n == 0)
                    {
                        plCurrencyCode = dtParallelCurrency.Rows[n]["CurrencyCode"].ToString();
                    }
                }
                shet2EndxlsCol = headreColIndex - 1;

                _rowL++;
                var revenueLevelRow = _rowL;
                oRU.SetText(ref sheet, revenueLevelRow, 1, "Total Revenue:", true);
                var drcrCol = 0;
                var Row_Total_Start = _rowL + 1;
                var RowTotal_current = _rowL;
                var Row_Total_End = 0;
                var sumdrcrCol1 = 0;
                string BudgetMasterId = null;
                string ActivityId = null;
                string AccountCodeId = null;

                for (int n = 0; n < dtCr.Rows.Count; n++)
                {
                    _rowL++;
                    if (isActivityLevel == true)
                    {
                        AccountCodeId = dtCr.Rows[n]["GLGeneralInfoCode"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtCr.Rows[n]["GL"]); mainColIndex++;
                        BudgetMasterId = dtCr.Rows[n]["BudgetMasterId"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtCr.Rows[n]["Budget"].ToString()); mainColIndex++;
                        ActivityId = dtCr.Rows[n]["ActivityId"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtCr.Rows[n]["Activity"].ToString()); mainColIndex++;

                    }
                    if (isBudgetLevel == true)
                    {
                        AccountCodeId = dtCr.Rows[n]["GLGeneralInfoCode"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtCr.Rows[n]["GL"]); mainColIndex++;
                        BudgetMasterId = dtCr.Rows[n]["BudgetMasterId"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtCr.Rows[n]["Budget"].ToString()); mainColIndex++;
                    }
                    if (isBudgetLevel == false && isActivityLevel == false)
                    {
                        AccountCodeId = dtCr.Rows[n]["GLGeneralInfoCode"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtCr.Rows[n]["GL"]); mainColIndex++;
                    }


                    sumdrcrCol1 = mainColIndex - 1;
                    drcrCol = mainColIndex;

                    for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                    {
                        string ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();

                        if (!string.IsNullOrEmpty(BudgetMasterId) && string.IsNullOrEmpty(ActivityId))
                        {
                            DataView dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                            };

                            if (p == 0)
                            {
                                plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                            }

                            var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                            DataTable dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count != 0)
                            {
                                oRU.SetText(ref sheet, _rowL, _pcCol, Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString()));
                                if (p == 0)
                                {
                                    _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(ActivityId))
                        {
                            DataView dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "'"
                            };

                            if (p == 0)
                            {
                                plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                            }

                            var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                            DataTable dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count != 0)
                            {
                                oRU.SetText(ref sheet, _rowL, _pcCol, Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString()));
                                if (p == 0)
                                {
                                    _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(AccountCodeId))
                        {
                            DataView dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                            };

                            if (p == 0)
                            {
                                plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                            }

                            var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                            DataTable dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count != 0)
                            {
                                oRU.SetText(ref sheet, _rowL, _pcCol, Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString()));
                                if (p == 0)
                                {
                                    _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                }
                            }
                        }
                    }
                    mainColIndex = 1;
                }//CR
                _rowL++;

                Row_Total_End = _rowL;
                if (sumdrcrCol1 > 0)
                {

                    TotalRevenue(ref sheet, oRU, dtParallelCurrency, sumdrcrCol1, RowTotal_current, Row_Total_Start, Row_Total_End);
                }

                _rowL++;

                oRU.SetText(ref sheet, _rowL, 1, "Total Expense:", true);
                var drcrCol2 = 0;
                var totCol2 = 0;
                var Row_Total_Start2 = _rowL + 1;
                var RowTotal_current2 = _rowL;
                var Row_Total_End2 = 0;
                var sumdrcrCol2 = 0;

                for (int n = 0; n < dtDr.Rows.Count; n++)
                {
                    _rowL++;
                    if (isActivityLevel == true)
                    {
                        AccountCodeId = dtDr.Rows[n]["GLGeneralInfoCode"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtDr.Rows[n]["GL"]); mainColIndex++;
                        BudgetMasterId = dtDr.Rows[n]["BudgetMasterId"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtDr.Rows[n]["Budget"].ToString()); mainColIndex++;
                        ActivityId = dtDr.Rows[n]["ActivityId"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtDr.Rows[n]["Activity"].ToString()); mainColIndex++;

                    }
                    if (isBudgetLevel == true)
                    {
                        AccountCodeId = dtDr.Rows[n]["GLGeneralInfoCode"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtDr.Rows[n]["GL"]); mainColIndex++;
                        BudgetMasterId = dtDr.Rows[n]["BudgetMasterId"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtDr.Rows[n]["Budget"].ToString()); mainColIndex++;
                        //oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtDr.Rows[n]["GL"]); mainColIndex++;
                    }
                    if (isBudgetLevel == false && isActivityLevel == false)
                    {
                        AccountCodeId = dtDr.Rows[n]["GLGeneralInfoCode"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtDr.Rows[n]["GL"]); mainColIndex++;
                    }
                    sumdrcrCol2 = mainColIndex - 1;
                    totCol2 = mainColIndex;
                    drcrCol2 = mainColIndex;

                    for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                    {
                        string ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();

                        if (!string.IsNullOrEmpty(BudgetMasterId) && string.IsNullOrEmpty(ActivityId))
                        {
                            DataView dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                            };
                            if (p == 0)
                            {
                                plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                            }

                            var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                            DataTable dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count != 0)
                            {
                                oRU.SetText(ref sheet, _rowL, _pcCol, Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString()));
                                if (p == 0)
                                {
                                    _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(ActivityId))
                        {
                            DataView dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "'"
                            };
                            if (p == 0)
                            {
                                plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                            }

                            var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                            DataTable dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count != 0)
                            {
                                oRU.SetText(ref sheet, _rowL, _pcCol, Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString()));
                                if (p == 0)
                                {
                                    _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(AccountCodeId))
                        {
                            DataView dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                            };
                            if (p == 0)
                            {
                                plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                            }

                            var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                            DataTable dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count != 0)
                            {
                                //drcrCol2++;
                                oRU.SetText(ref sheet, _rowL, _pcCol, Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString()));
                                if (p == 0)
                                {
                                    _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                }
                            }
                        }
                    }
                    mainColIndex = 1;
                }//DR
                Row_Total_End2 = _rowL;
                TotalExpense(ref sheet, oRU, dtParallelCurrency, sumdrcrCol2, RowTotal_current2, Row_Total_Start2, Row_Total_End2);

                #region sumCalc

                _rowL++;
                var sumdrcrCol = totCol2 - 1;
                sheet.Range[_rowL, 1].Text = "Profit/Loss ";
                sheet.Range[_rowL, 1].CellStyle.Font.Bold = true;
                sheet.Range[_rowL, 1].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);

                //DR
                for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
                {
                    sumdrcrCol++;
                    sheet.Range[_rowL, sumdrcrCol].Formula = "=(" + oRU.GetColumnNameForXls(sumdrcrCol) + RowTotal_current + "-" + oRU.GetColumnNameForXls(sumdrcrCol) + RowTotal_current2 + ")";
                    sheet.Range[_rowL, sumdrcrCol].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                    sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
                }

                #endregion sumCalc

                //shet2EndxlsCol = drcrCol2;
                sheet.Range[8, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);

                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyPlantHeader(ref sheet, shet2EndxlsCol, SheetHeader, identity.CompanyId, plantName, null);
                //oRU.SetText(ref sheet, 5, 2, "Date " + fromDate + " To Date " + toDate + "", ExcelHAlign.HAlignCenter);

                oRU.SetText(ref sheet, 5, 2, "As On " + date + "", ExcelHAlign.HAlignCenter);
                sheet.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + 5].Merge();
                sheet.Range[oRU.GetColumnNameForXls(1) + 4 + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + 4].Merge();
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.Name = "Income Statement";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyPlantHeader(ref sheet, 5, SheetHeader, identity.CompanyId, plantName, null);
                oRU.SetText(ref sheet, 5, 3, "No Data Found !", ExcelHAlign.HAlignCenter);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
        }
        public IWorkbook IncomeStatement_YearClosed_Report(ExcelEngine excelEngine, string companyId, string plantId, string plantName, string fiscalYearCloseId, string fiscalYearName, bool isBudgetLevel, bool isActivityLevel)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheet_IncomeStatement_YearClosed(ref sheet1, oRU, "Year Closed Income Statement", "Year Closed Income Statement Report", companyId, plantId, plantName, fiscalYearCloseId, fiscalYearName, isBudgetLevel, isActivityLevel);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_IncomeStatement_YearClosed(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, string companyId, string plantId, string plantName, string fiscalYearCloseId, string fiscalYearName, bool isBudgetLevel, bool isActivityLevel)
        {
            DataTable dtGeneralVoucher = null;
            DataTable dtCustomerCheckByCompany = null;
            DataTable dtMainBody;
            DataView dvDr;
            DataTable dtDr = null;
            DataView dvCr;
            DataTable dtCr = null;
            #region List data

            DataSet dsLocal = GetIncomeStatementInfo_YearClosed(companyId, plantId, fiscalYearCloseId, isBudgetLevel, isActivityLevel);
            dtGeneralVoucher = dsLocal.Tables[0];

            DataSet CustomerCheckByCompanyList = GetCustomerCheckByCompany(companyId);
            dtCustomerCheckByCompany = CustomerCheckByCompanyList.Tables[0];

            if (dtGeneralVoucher.Rows.Count > 0)
            {
                DataView dvAccountCode = new DataView(dsLocal.Tables[0]);
                DataTable dtAccountCode;
                if (isBudgetLevel == true)
                {
                    dtAccountCode = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId", "BudgetMasterId");
                }
                if (isActivityLevel == true)
                {
                    dtAccountCode = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId", "BudgetMasterId", "ActivityId");
                }
                if (isActivityLevel == false && isBudgetLevel == false)
                {
                    dtAccountCode = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId");
                }

                DataView dvParallelCurrency = new DataView(dsLocal.Tables[0])
                {
                    Sort = "CurrencyCode ASC"
                };
                DataTable dtParallelCurrency = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");
                DataView dvMainBody = new DataView(dsLocal.Tables[0]);

                if (isBudgetLevel == true)
                {
                    dtMainBody = dvMainBody.ToTable(true, "GLGeneralInfoCode", "GL", "Budget", "BudgetMasterId");
                    dvDr = new DataView(dsLocal.Tables[0])
                    {
                        RowFilter = "MainHead='Expense'",
                        Sort = "GLGeneralInfoCode, GL, Budget"
                    };
                    dtDr = dvDr.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget");
                    dvCr = new DataView(dsLocal.Tables[0])
                    {
                        RowFilter = "MainHead='Revenue'",
                        Sort = "GLGeneralInfoCode, GL, Budget"
                    };
                    dtCr = dvCr.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget");

                }
                if (isActivityLevel == true)
                {
                    dtMainBody = dvMainBody.ToTable(true, "GLGeneralInfoCode", "GL", "Budget", "BudgetMasterId", "Activity", "ActivityId");
                    dvDr = new DataView(dsLocal.Tables[0])
                    {
                        RowFilter = "MainHead='Expense'",
                        Sort = "GLGeneralInfoCode, GL, Budget,Activity"
                    };
                    dtDr = dvDr.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget", "ActivityId", "Activity");
                    dvCr = new DataView(dsLocal.Tables[0])
                    {
                        RowFilter = "MainHead='Revenue'",
                        Sort = "GLGeneralInfoCode, GL, Budget,Activity"
                    };
                    dtCr = dvCr.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget", "ActivityId", "Activity");
                }
                if (isActivityLevel == false && isBudgetLevel == false)
                {
                    dtMainBody = dvMainBody.ToTable(true, "GLGeneralInfoCode", "GL");
                    dvDr = new DataView(dsLocal.Tables[0])
                    {
                        RowFilter = "MainHead='Expense'",
                        Sort = "GLGeneralInfoCode, GL"
                    };
                    dtDr = dvDr.ToTable(true, "GLGeneralInfoCode", "GL");
                    dvCr = new DataView(dsLocal.Tables[0])
                    {
                        RowFilter = "MainHead='Revenue'",
                        Sort = "GLGeneralInfoCode, GL"
                    };
                    dtCr = dvCr.ToTable(true, "GLGeneralInfoCode", "GL");
                }

                #region Customer Check By Company

                DataView dvCustomerCheckByCompanyBody = new DataView(CustomerCheckByCompanyList.Tables[0]);
                DataTable dtCustomerCheckByCompanyBody = dvCustomerCheckByCompanyBody.ToTable(false, "IsVoucherFromBudget");
                string Budget = dtCustomerCheckByCompanyBody.Rows[0]["IsVoucherFromBudget"].ToString();

                #endregion Customer Check By Company

                #endregion List data

                var _col = 1;
                var shet2EndxlsCol = _col;

                var _rowL = 6;
                _rowL++;

                var headreColIndex = 1;
                var mainColIndex = 1;

                oRU.SetHeaderText(ref sheet, _rowL, headreColIndex, "GL", 38); headreColIndex++;

                if (isBudgetLevel == true)
                {
                    oRU.SetHeaderText(ref sheet, _rowL, headreColIndex, nameof(Budget), 38); headreColIndex++;
                }
                if (isActivityLevel == true)
                {
                    oRU.SetHeaderText(ref sheet, _rowL, headreColIndex, nameof(Budget), 38); headreColIndex++;
                    oRU.SetHeaderText(ref sheet, _rowL, headreColIndex, "Activity", 38); headreColIndex++;
                }
                double _Total_Amount = 0;
                string plCurrencyId = string.Empty;
                string plCurrencyCode = string.Empty;

                ArrayList alParaCurrency = new ArrayList();

                for (int n = 0; n < dtParallelCurrency.Rows.Count; n++)
                {
                    oRU.SetHeaderText(ref sheet, _rowL, headreColIndex, dtParallelCurrency.Rows[n]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter); headreColIndex++;
                    Dictionary<string, int> dic = new Dictionary<string, int>
                {
                    { dtParallelCurrency.Rows[n]["ParallelCurrencyId"].ToString(), headreColIndex-1 }
                };
                    alParaCurrency.Add(dic);

                    if (n == 0)
                    {
                        plCurrencyCode = dtParallelCurrency.Rows[n]["CurrencyCode"].ToString();
                    }
                }
                shet2EndxlsCol = headreColIndex - 1;

                _rowL++;
                var revenueLevelRow = _rowL;
                oRU.SetText(ref sheet, revenueLevelRow, 1, "Total Revenue:", true);
                var drcrCol = 0;
                var Row_Total_Start = _rowL + 1;
                var RowTotal_current = _rowL;
                var Row_Total_End = 0;
                var sumdrcrCol1 = 0;
                string BudgetMasterId = null;
                string ActivityId = null;
                string AccountCodeId = null;

                for (int n = 0; n < dtCr.Rows.Count; n++)
                {
                    _rowL++;
                    if (isActivityLevel == true)
                    {
                        AccountCodeId = dtCr.Rows[n]["GLGeneralInfoCode"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtCr.Rows[n]["GL"]); mainColIndex++;
                        BudgetMasterId = dtCr.Rows[n]["BudgetMasterId"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtCr.Rows[n]["Budget"].ToString()); mainColIndex++;
                        ActivityId = dtCr.Rows[n]["ActivityId"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtCr.Rows[n]["Activity"].ToString()); mainColIndex++;

                    }
                    if (isBudgetLevel == true)
                    {
                        AccountCodeId = dtCr.Rows[n]["GLGeneralInfoCode"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtCr.Rows[n]["GL"]); mainColIndex++;
                        BudgetMasterId = dtCr.Rows[n]["BudgetMasterId"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtCr.Rows[n]["Budget"].ToString()); mainColIndex++;
                    }
                    if (isBudgetLevel == false && isActivityLevel == false)
                    {
                        AccountCodeId = dtCr.Rows[n]["GLGeneralInfoCode"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtCr.Rows[n]["GL"]); mainColIndex++;
                    }


                    sumdrcrCol1 = mainColIndex - 1;
                    drcrCol = mainColIndex;

                    for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                    {
                        string ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();

                        if (!string.IsNullOrEmpty(BudgetMasterId) && string.IsNullOrEmpty(ActivityId))
                        {
                            DataView dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                            };

                            if (p == 0)
                            {
                                plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                            }

                            var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                            DataTable dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count != 0)
                            {
                                oRU.SetText(ref sheet, _rowL, _pcCol, Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString()));
                                if (p == 0)
                                {
                                    _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(ActivityId))
                        {
                            DataView dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "'"
                            };

                            if (p == 0)
                            {
                                plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                            }

                            var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                            DataTable dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count != 0)
                            {
                                oRU.SetText(ref sheet, _rowL, _pcCol, Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString()));
                                if (p == 0)
                                {
                                    _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(AccountCodeId))
                        {
                            DataView dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                            };

                            if (p == 0)
                            {
                                plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                            }

                            var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                            DataTable dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count != 0)
                            {
                                oRU.SetText(ref sheet, _rowL, _pcCol, Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString()));
                                if (p == 0)
                                {
                                    _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                }
                            }
                        }
                    }
                    mainColIndex = 1;
                }//CR
                _rowL++;

                Row_Total_End = _rowL;
                if (sumdrcrCol1 > 0)
                {

                    TotalRevenue(ref sheet, oRU, dtParallelCurrency, sumdrcrCol1, RowTotal_current, Row_Total_Start, Row_Total_End);
                }

                _rowL++;

                oRU.SetText(ref sheet, _rowL, 1, "Total Expense:", true);
                var drcrCol2 = 0;
                var totCol2 = 0;
                var Row_Total_Start2 = _rowL + 1;
                var RowTotal_current2 = _rowL;
                var Row_Total_End2 = 0;
                var sumdrcrCol2 = 0;

                for (int n = 0; n < dtDr.Rows.Count; n++)
                {
                    _rowL++;
                    if (isActivityLevel == true)
                    {
                        AccountCodeId = dtDr.Rows[n]["GLGeneralInfoCode"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtDr.Rows[n]["GL"]); mainColIndex++;
                        BudgetMasterId = dtDr.Rows[n]["BudgetMasterId"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtDr.Rows[n]["Budget"].ToString()); mainColIndex++;
                        ActivityId = dtDr.Rows[n]["ActivityId"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtDr.Rows[n]["Activity"].ToString()); mainColIndex++;

                    }
                    if (isBudgetLevel == true)
                    {
                        AccountCodeId = dtDr.Rows[n]["GLGeneralInfoCode"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtDr.Rows[n]["GL"]); mainColIndex++;
                        BudgetMasterId = dtDr.Rows[n]["BudgetMasterId"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtDr.Rows[n]["Budget"].ToString()); mainColIndex++;
                        //oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtDr.Rows[n]["GL"]); mainColIndex++;
                    }
                    if (isBudgetLevel == false && isActivityLevel == false)
                    {
                        AccountCodeId = dtDr.Rows[n]["GLGeneralInfoCode"].ToString();
                        oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtDr.Rows[n]["GL"]); mainColIndex++;
                    }
                    sumdrcrCol2 = mainColIndex - 1;
                    totCol2 = mainColIndex;
                    drcrCol2 = mainColIndex;

                    for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                    {
                        string ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();

                        if (!string.IsNullOrEmpty(BudgetMasterId) && string.IsNullOrEmpty(ActivityId))
                        {
                            DataView dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                            };
                            if (p == 0)
                            {
                                plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                            }

                            var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                            DataTable dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count != 0)
                            {
                                oRU.SetText(ref sheet, _rowL, _pcCol, Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString()));
                                if (p == 0)
                                {
                                    _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(ActivityId))
                        {
                            DataView dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "'"
                            };
                            if (p == 0)
                            {
                                plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                            }

                            var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                            DataTable dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count != 0)
                            {
                                oRU.SetText(ref sheet, _rowL, _pcCol, Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString()));
                                if (p == 0)
                                {
                                    _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(AccountCodeId))
                        {
                            DataView dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                            };
                            if (p == 0)
                            {
                                plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                            }

                            var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                            DataTable dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count != 0)
                            {
                                //drcrCol2++;
                                oRU.SetText(ref sheet, _rowL, _pcCol, Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString()));
                                if (p == 0)
                                {
                                    _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                }
                            }
                        }
                    }
                    mainColIndex = 1;
                }//DR
                Row_Total_End2 = _rowL;
                TotalExpense(ref sheet, oRU, dtParallelCurrency, sumdrcrCol2, RowTotal_current2, Row_Total_Start2, Row_Total_End2);

                #region sumCalc

                _rowL++;
                var sumdrcrCol = totCol2 - 1;
                sheet.Range[_rowL, 1].Text = "Profit/Loss ";
                sheet.Range[_rowL, 1].CellStyle.Font.Bold = true;
                sheet.Range[_rowL, 1].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);

                //DR
                for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
                {
                    sumdrcrCol++;
                    sheet.Range[_rowL, sumdrcrCol].Formula = "=(" + oRU.GetColumnNameForXls(sumdrcrCol) + RowTotal_current + "-" + oRU.GetColumnNameForXls(sumdrcrCol) + RowTotal_current2 + ")";
                    sheet.Range[_rowL, sumdrcrCol].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                    sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
                }

                #endregion sumCalc

                //shet2EndxlsCol = drcrCol2;
                sheet.Range[8, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);

                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyPlantHeader(ref sheet, shet2EndxlsCol, SheetHeader, identity.CompanyId, plantName, null);
                //oRU.SetText(ref sheet, 5, 2, "Date " + fromDate + " To Date " + toDate + "", ExcelHAlign.HAlignCenter);

                oRU.SetText(ref sheet, 5, 2, "Fiscal Year: " + fiscalYearName + "", ExcelHAlign.HAlignCenter);
                sheet.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + 5].Merge();
                sheet.Range[oRU.GetColumnNameForXls(1) + 4 + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + 4].Merge();
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.Name = "Year Closed Income Statement";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyPlantHeader(ref sheet, 5, SheetHeader, identity.CompanyId, plantName, null);
                oRU.SetText(ref sheet, 5, 3, "No Data Found !", ExcelHAlign.HAlignCenter);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
        }

        private void TotalRevenue(ref IWorksheet sheet, ReportUtility oRU, DataTable dtParallelCurrency, int sumdrcrCol1, int RowTotal_current, int Row_Total_Start, int Row_total_End)
        {
            for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
            {
                //Row_Total_Start = _rowL;
                sumdrcrCol1++;
                sheet.Range[RowTotal_current, sumdrcrCol1].Formula = "=SUM(" + oRU.GetColumnNameForXls(sumdrcrCol1) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(sumdrcrCol1) + Row_total_End + ")";
                sheet.Range[RowTotal_current, sumdrcrCol1].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[RowTotal_current, sumdrcrCol1].CellStyle.Font.Bold = true;
                sheet.Range[RowTotal_current, sumdrcrCol1].BorderAround(ExcelLineStyle.Hair);
            }
        }

        private void TotalExpense(ref IWorksheet sheet, ReportUtility oRU, DataTable dtParallelCurrency, int sumdrcrCol2, int RowTotal_current2, int Row_Total_Start2, int Row_Total_End2)
        {
            for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
            {
                sumdrcrCol2++;
                sheet.Range[RowTotal_current2, sumdrcrCol2].Formula = "=SUM(" + oRU.GetColumnNameForXls(sumdrcrCol2) + Row_Total_Start2 + ":" + oRU.GetColumnNameForXls(sumdrcrCol2) + Row_Total_End2 + ")";
                sheet.Range[RowTotal_current2, sumdrcrCol2].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[RowTotal_current2, sumdrcrCol2].CellStyle.Font.Bold = true;
                sheet.Range[RowTotal_current2, sumdrcrCol2].BorderAround(ExcelLineStyle.Hair);
            }
        }

        private DataSet GetIncomeStatementInfo(string companyId, string plantId, string date, string[] parallelCurrencies, bool isBudgetLevel, bool isActivityLevel)
        {
            GridParameter parameters = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var parallelCurrency = "";
                parallelCurrency = parallelCurrencies.Length > 0 ? string.Join(",", parallelCurrencies.Select(item => "'" + item + "'")) : "' '";
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                if (isActivityLevel)
                {
                    parameters.CmdText = @"SELECT GL.Id AS AccountCodeId,Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                    sum(VDC.DrAmount) as DrAmount,
		                                    sum(VDC.CrAmount) as CrAmount,
                                            sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
											sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
											ACT.BalanceType,
                                            ACT.Id AS [MainHead],
											AG.UserName AS [Level],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId, BUD.UserName AS Budget,
											A.UserName AS Activity,
                                            A.Id AS ActivityId
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                            LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                                            LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                                            where act.IsBalanceSheet=0 AND v.PostingDate <= '" + date + @"' AND V.CompanyId='" + companyId + @"'  AND V.PlantId='" + plantId + @"'
                                            and VDC.ParallelCurrencyId IN (" + parallelCurrency + @") and v.IsPark=0
                                            AND VDC.VoucherDetailId NOT IN ( SELECT VD.Id FROM  TRN.VoucherDetail AS VD  
																INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
																LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
																LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
																LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
																WHERE ACT.Id IN('Revenue','Expense') AND V.FiscalYearId in(select FiscalYearId from [SCS].[FiscalYearClose] ))
                                             group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName, GL.AccountCode,v.PostingDate,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName,A.UserName,A.Id";

                    return _sqlRepository.GetGridData(parameters).Source;
                }
                else if (isBudgetLevel && !isActivityLevel)
                {
                    parameters.CmdText = @"SELECT GL.Id AS AccountCodeId,Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                    sum(VDC.DrAmount) as DrAmount,
		                                    sum(VDC.CrAmount) as CrAmount,
                                            sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
											sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
											ACT.BalanceType,
                                            ACT.Id AS [MainHead],
											AG.UserName AS [Level],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId, BUD.UserName AS Budget
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                            LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                                            LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                                            where act.IsBalanceSheet=0 AND v.PostingDate <= '" + date + @"' AND V.CompanyId='" + companyId + @"'  AND V.PlantId='" + plantId + @"'
                                            and VDC.ParallelCurrencyId IN (" + parallelCurrency + @") and v.IsPark=0
                                            AND VDC.VoucherDetailId NOT IN ( SELECT VD.Id FROM  TRN.VoucherDetail AS VD  
																INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
																LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
																LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
																LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
																WHERE ACT.Id IN('Revenue','Expense') AND V.FiscalYearId in(select FiscalYearId from [SCS].[FiscalYearClose] ))
                                             group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName, GL.AccountCode,v.PostingDate,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName";

                    return _sqlRepository.GetGridData(parameters).Source;

                }
                else
                {

                    parameters.CmdText = @"SELECT GL.Id AS AccountCodeId,Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
		                                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
		                                    sum(VDC.DrAmount) as DrAmount,
		                                    sum(VDC.CrAmount) as CrAmount,
                                            sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
											sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
											ACT.BalanceType,
                                            ACT.Id AS [MainHead],
											AG.UserName AS [Level],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
		                                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                                            LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                                            LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                                            where act.IsBalanceSheet=0 AND v.PostingDate <= '" + date + @"' AND V.CompanyId='" + companyId + @"'  AND V.PlantId='" + plantId + @"'
                                            and VDC.ParallelCurrencyId IN (" + parallelCurrency + @") and v.IsPark=0
                                            AND VDC.VoucherDetailId NOT IN ( SELECT VD.Id FROM  TRN.VoucherDetail AS VD  
																INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
																LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
																LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
																LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
																WHERE ACT.Id IN('Revenue','Expense') AND V.FiscalYearId in(select FiscalYearId from [SCS].[FiscalYearClose] ))
                                             group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName, GL.AccountCode,v.PostingDate,ACT.BalanceType,AG.UserName,ACT.Id";

                    return _sqlRepository.GetGridData(parameters).Source;

                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetIncomeStatementInfo_YearClosed(string companyId, string plantId, string fiscalYearCloseId, bool isBudgetLevel, bool isActivityLevel)
        {
            GridParameter parameters = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                if (isActivityLevel)
                {
                    parameters.CmdText = @"SELECT [AccountCodeId], [ParallelCurrencyId], [CurrencyCode],SUM(CAST([DRcumulative] AS DECIMAL(18,2))) DRcumulative
									, SUM(CAST([CRcumulative] AS DECIMAL(18,2)))CRcumulative
									, [BalanceType], [MainHead], [GLGeneralInfoId], [GL], [GLGeneralInfoCode], [Budget], [BudgetMasterId], [Activity], [ActivityId]
									FROM [TRN].[FiscalYearCloseTrialBalance]  FYCB WHERE FYCB.FiscalYearCloseId= '" + fiscalYearCloseId + @"' AND FYCB.CompanyId='" + companyId + @"'  AND FYCB.PlantId='" + plantId + @"'   
                                    GROUP BY [AccountCodeId], [ParallelCurrencyId], [CurrencyCode], [BalanceType], [MainHead], [GLGeneralInfoId], [GL], [GLGeneralInfoCode]
									, [Budget], [BudgetMasterId], [Activity], [ActivityId]";

                    return _sqlRepository.GetGridData(parameters).Source;
                }
                else if (isBudgetLevel && !isActivityLevel)
                {
                    parameters.CmdText = @"SELECT [AccountCodeId], [ParallelCurrencyId], [CurrencyCode],SUM(CAST([DRcumulative] AS DECIMAL(18,2))) DRcumulative
									, SUM(CAST([CRcumulative] AS DECIMAL(18,2)))CRcumulative
									, [BalanceType], [MainHead], [GLGeneralInfoId], [GL], [GLGeneralInfoCode], [Budget], [BudgetMasterId]
									FROM [TRN].[FiscalYearCloseTrialBalance]  FYCB WHERE FYCB.FiscalYearCloseId= '" + fiscalYearCloseId + @"' AND FYCB.CompanyId='" + companyId + @"'  AND FYCB.PlantId='" + plantId + @"'   
                                    GROUP BY [AccountCodeId], [ParallelCurrencyId], [CurrencyCode], [BalanceType], [MainHead], [GLGeneralInfoId], [GL], [GLGeneralInfoCode]
									, [Budget], [BudgetMasterId]";

                    return _sqlRepository.GetGridData(parameters).Source;

                }
                else
                {
                    parameters.CmdText = @"SELECT [AccountCodeId], [ParallelCurrencyId], [CurrencyCode],SUM(CAST([DRcumulative] AS DECIMAL(18,2))) DRcumulative
									, SUM(CAST([CRcumulative] AS DECIMAL(18,2)))CRcumulative
									, [BalanceType], [MainHead], [GLGeneralInfoId], [GL], [GLGeneralInfoCode]
									FROM [TRN].[FiscalYearCloseTrialBalance]  FYCB WHERE FYCB.FiscalYearCloseId= '" + fiscalYearCloseId + @"' AND FYCB.CompanyId='" + companyId + @"'  AND FYCB.PlantId='" + plantId + @"'   
                                    GROUP BY [AccountCodeId], [ParallelCurrencyId], [CurrencyCode], [BalanceType], [MainHead], [GLGeneralInfoId], [GL], [GLGeneralInfoCode] ";

                    return _sqlRepository.GetGridData(parameters).Source;

                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        //Income statement date wise range

        private DataSet GetCustomerCheckByCompany_DateRange(string companyId)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @" SELECT IsVoucherFromBudget
		                                , IsBudgetPeriod
		                                , IsCostCenterApplicable
		                                , IsProfitCenterApplicable
		                                FROM [ORG].[Company]
		                                WHERE Id='" + companyId + @"' AND Active=1 AND Archive=0";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook IncomeStatement_Report_DateRange(ExcelEngine excelEngine, string companyId, string plantId, string plantName, string fromDate, string toDate, string[] parallelCurrencies, bool isBudgetLevel, bool isActivityLevel)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                // DataSet dsLocal = GetIncomeStatementInfoDateRange(companyId, plantId, fromDate, toDate, parallelCurrencies);
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheet_IncomeStatement_DateRange(ref sheet1, oRU, "Income Statement", "Income Statement Report", companyId, plantId, plantName, fromDate, toDate, parallelCurrencies, isBudgetLevel, isActivityLevel);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IWorkbook EntityWiseExpenseandEarning_Report_DateRange(ExcelEngine excelEngine, string companyId, string plantId, string plantName, string fromDate, string toDate, string entityId, string entity, string[] parallelCurrencies)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                // DataSet dsLocal = GetIncomeStatementInfoDateRange(companyId, plantId, fromDate, toDate, parallelCurrencies);
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheet_EntityWiseExpenseAndEarning_DateRange(ref sheet1, oRU, "Entity Wise Expense And Earning Report", "EntityWiseExpenseandEarning Report", companyId, plantId, plantName, fromDate, toDate, entityId, entity, parallelCurrencies);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public IWorkbook EntityWiseExpenseandEarning_Report_DateRange_ActivityLevel(ExcelEngine excelEngine, string companyId, string plantId, string plantName, string fromDate, string toDate, string entityId, string entity, string[] parallelCurrencies)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheet_EntityWiseExpenseAndEarning_DateRange_ActivityLevel(ref sheet1, oRU, "Entity Wise Expense And Earning Report", "EntityWiseExpenseandEarning Report", companyId, plantId, plantName, fromDate, toDate, entityId, entity, parallelCurrencies);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        #region BL
        public IWorkbook BalanceSheet_Report_DateRange(ExcelEngine excelEngine, string companyId, string plantName, string fromDate, string toDate)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                // DataSet dsLocal = GetBalanceSheetInfoDateRange(companyId, fromDate, toDate);
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheet_BalanceSheet_DateRange(ref sheet1, oRU, "Balance Sheet", "Balance Sheet Report", companyId, plantName, fromDate, toDate);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }


        private void CreateSheet_BalanceSheet_DateRange(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, /*DataSet dslocal,*/ string companyId, string plantName, string fromDate, string toDate)
        {
            DataTable dtGeneralVoucher = null;
            DataTable dtCustomerCheckByCompany = null;

            #region List data

            DataSet dsLocal = GetBalanceSheetInfoDateRange(companyId, fromDate, toDate);
            DataTable dtLocalFTP = GetBalanceSheetInfoDateRangeForThePeriod(companyId, fromDate, toDate);
            DataTable dtLocalFTPMaster = GetBalanceSheetInfoDateRangeForThePeriodMaster(companyId, fromDate, toDate);

            dtGeneralVoucher = dsLocal.Tables[0];

            DataSet CustomerCheckByCompanyList = GetCustomerCheckByCompany_DateRange(companyId);
            dtCustomerCheckByCompany = CustomerCheckByCompanyList.Tables[0];

            if (dtLocalFTPMaster.Rows.Count > 0)
            {
                DataView dvAccountCode = new DataView(dsLocal.Tables[0]);
                DataTable dtAccountCode = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId", "BudgetMasterId");

                DataView dvParallelCurrency = new DataView(dsLocal.Tables[0])
                {
                    Sort = "CurrencyCode ASC"
                };
                DataTable dtParallelCurrency = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");

                DataView dvMainBody = new DataView(dsLocal.Tables[0]);
                DataTable dtMainBody = dvMainBody.ToTable(true, "GLGeneralInfoCode", "GL", "Budget", "BudgetMasterId");

                DataView dvDr = new DataView(dtLocalFTPMaster)
                {
                    RowFilter = "MainHead='Expense'",
                    Sort = "GLGeneralInfoCode, GL, Budget"
                };
                DataTable dtDr = dvDr.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget");

                DataView dvCr = new DataView(dtLocalFTPMaster)
                {
                    RowFilter = "MainHead='Revenue'",
                    Sort = "GLGeneralInfoCode, GL, Budget"
                };
                DataTable dtCr = dvCr.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget");

                if (dtLocalFTP.Rows.Count > 0)
                {
                    DataView dvAccountCodeFTP = new DataView(dtLocalFTP);
                    DataTable dtAccountCodeFTP = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId", "BudgetMasterId");

                    DataView dvParallelCurrencyFTP = new DataView(dtLocalFTP)
                    {
                        Sort = "CurrencyCode ASC"
                    };
                    DataTable dtParallelCurrencyFTP = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");

                    DataView dvMainBodyFTP = new DataView(dtLocalFTP);
                    DataTable dtMainBodyFTP = dvMainBody.ToTable(true, "GLGeneralInfoCode", "GL", "Budget", "BudgetMasterId");

                    DataView dvDrFTP = new DataView(dtLocalFTP)
                    {
                        RowFilter = "MainHead='Expense'",
                        Sort = "GLGeneralInfoCode, GL, Budget"
                    };
                    DataTable dtDrFTP = dvDrFTP.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget");

                    DataView dvCrFTP = new DataView(dtLocalFTP)
                    {
                        RowFilter = "MainHead='Revenue'",
                        Sort = "GLGeneralInfoCode, GL, Budget"
                    };
                    DataTable dtCrFTP = dvCrFTP.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget");

                }



                #region Customer Check By Company

                DataView dvCustomerCheckByCompanyBody = new DataView(CustomerCheckByCompanyList.Tables[0]);
                DataTable dtCustomerCheckByCompanyBody = dvCustomerCheckByCompanyBody.ToTable(false, "IsVoucherFromBudget");
                string Budget = dtCustomerCheckByCompanyBody.Rows[0]["IsVoucherFromBudget"].ToString();

                #endregion Customer Check By Company

                #endregion List data

                var _col = 1;
                var shet2EndxlsCol = _col;

                var _rowL = 6;
                _rowL++;

                var headreColIndex = 1;
                var mainColIndex = 1;

                oRU.SetHeaderText(ref sheet, _rowL, headreColIndex, "Account Name", 38); headreColIndex++;
                //sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;

                if (Budget == "True")
                {
                    oRU.SetHeaderText(ref sheet, _rowL, headreColIndex, nameof(Budget), 38); headreColIndex++;
                }

                double _Total_Amount = 0;
                double _Total_Amount_DateRange = 0;
                string plCurrencyId = string.Empty;
                string plCurrencyCode = string.Empty;

                ArrayList alParaCurrency = new ArrayList();

                for (int n = 0; n < dtParallelCurrency.Rows.Count; n++)
                {
                    oRU.SetHeaderText(ref sheet, _rowL, headreColIndex, dtParallelCurrency.Rows[n]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter); headreColIndex++;
                    //sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].Merge();
                    //oRU.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", ExcelHAlign.HAlignRight); headreColIndex++;
                    //oRU.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight); headreColIndex++;

                    Dictionary<string, int> dic = new Dictionary<string, int>
                {
                    { dtParallelCurrency.Rows[n]["ParallelCurrencyId"].ToString(), headreColIndex-1 }
                };
                    alParaCurrency.Add(dic);

                    if (n == 0)
                    {
                        plCurrencyCode = dtParallelCurrency.Rows[n]["CurrencyCode"].ToString();
                    }
                }




                shet2EndxlsCol = headreColIndex - 1;

                int colHeaderForThePeriod = headreColIndex;
                oRU.SetHeaderText(ref sheet, _rowL, colHeaderForThePeriod, "For The Period", 15); headreColIndex++;
                int colHeaderClosingBalance = headreColIndex;
                oRU.SetHeaderText(ref sheet, _rowL, colHeaderClosingBalance, "Closing Balance", 15); headreColIndex++;

                _rowL++;

                oRU.SetText(ref sheet, _rowL, 1, "Total Revenue:", true);
                var drcrCol = 0;
                var Row_Total_Start = _rowL + 1;
                var RowTotal_current = _rowL;
                var Row_Total_End = 0;
                var sumdrcrColDateRange = 0;

                for (int n = 0; n < dtCr.Rows.Count; n++)
                {
                    _rowL++;
                    string AccountCodeId = dtCr.Rows[n]["GLGeneralInfoCode"].ToString();
                    string BudgetMasterId = dtCr.Rows[n]["BudgetMasterId"].ToString();
                    oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtCr.Rows[n]["GL"]); ; mainColIndex++;


                    if (BudgetMasterId != "")
                    {
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtLocalFTPMaster.Rows[n][nameof(Budget)].ToString());
                    }

                    // sumdrcrCol1 = mainColIndex;
                    sumdrcrColDateRange = mainColIndex;
                    drcrCol = mainColIndex;

                    for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                    {
                        string ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();

                        //if (BudgetMasterId != "")
                        //{
                        DataView dvDrCr = new DataView(dsLocal.Tables[0])
                        {
                            RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                        };
                        DataView dvDrCrFTP = new DataView(dtLocalFTP)
                        {
                            RowFilter = " GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                        };
                        if (p == 0)
                        {
                            plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                        }

                        var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                        // var _pcCol1 = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                        DataTable dtDrCr = dvDrCr.ToTable();
                        if (dtDrCr.Rows.Count != 0)
                        {
                            try
                            {
                                oRU.SetText(ref sheet, _rowL, _pcCol, Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString()));
                                //oRU.SetText(ref sheet, _rowL, colHeaderForThePeriod, Convert.ToDouble(dtDrCr.Rows[1]["FRCRcumulative"].ToString()));
                                if (p == 0)
                                {
                                    _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                    //_Total_Amount_DateRange += Convert.ToDouble(dtDrCr.Rows[1]["FRCRcumulative"].ToString());
                                }
                            }
                            catch (Exception ex)
                            {


                            }
                        }

                        if (dvDrCrFTP.ToTable().Rows.Count != 0)
                        {
                            try
                            {
                                oRU.SetText(ref sheet, _rowL, colHeaderForThePeriod, Convert.ToDouble(dvDrCrFTP.ToTable().Rows[0]["FRCRcumulative"].ToString()));
                                if (p == 0)
                                {
                                    _Total_Amount_DateRange += Convert.ToDouble(dtDrCr.Rows[0]["FRCRcumulative"].ToString());
                                }
                            }
                            catch (Exception ex)
                            {


                            }
                        }

                    }
                    sheet.Range[_rowL, colHeaderClosingBalance].Formula = "=SUM(" + oRU.GetColumnNameForXls(colHeaderForThePeriod) + (_rowL) + "+" + oRU.GetColumnNameForXls(colHeaderForThePeriod - 1) + _rowL + /*"-" + oRU.GetColumnNameForXls(col - 1) + _rowL +*/ ")";

                    mainColIndex = 1;
                }//CR
                Row_Total_End = _rowL;
                TotalRevenue_DateRange(ref sheet, oRU, dtParallelCurrency, sumdrcrColDateRange, RowTotal_current, Row_Total_Start, Row_Total_End);
                TotalRevenue_DateRange(ref sheet, oRU, dtParallelCurrency, colHeaderForThePeriod - 1, RowTotal_current, Row_Total_Start, Row_Total_End);



                //TotalExpense_DateRange(ref sheet, oRU, dtParallelCurrency, sumdrcrCol2, RowTotal_current2, Row_Total_Start2, Row_Total_End2);
                //TotalExpense_DateRange(ref sheet, oRU, dtParallelCurrency, colHeaderForThePeriod - 1, RowTotal_current2, Row_Total_Start2, Row_Total_End2);

                _rowL++;

                sheet.Range[_rowL, colHeaderClosingBalance].Formula = "=SUM(" + oRU.GetColumnNameForXls(colHeaderForThePeriod - 1) + (_rowL) + "+" + oRU.GetColumnNameForXls(colHeaderForThePeriod) + _rowL + /*"-" + oRU.GetColumnNameForXls(col - 1) + _rowL +*/ ")";
                sheet[_rowL, colHeaderClosingBalance].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[_rowL, colHeaderClosingBalance].NumberFormat = reportUtility.NumberFormatDecimalTwo(); //col++;
                sheet.Range[_rowL, colHeaderClosingBalance].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[_rowL, colHeaderClosingBalance].CellStyle.Font.Bold = true;


                oRU.SetText(ref sheet, _rowL, 1, "Total Expense:", true);
                var drcrCol2 = 0;
                var totCol2 = 0;
                var Row_Total_Start2 = _rowL + 1;
                var RowTotal_current2 = _rowL;
                var Row_Total_End2 = 0;
                var sumdrcrCol2 = 0;

                for (int n = 0; n < dtDr.Rows.Count; n++)
                {
                    _rowL++;
                    string AccountCodeId = dtDr.Rows[n]["GLGeneralInfoCode"].ToString();
                    string BudgetMasterId = dtDr.Rows[n]["BudgetMasterId"].ToString();
                    oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtDr.Rows[n]["GL"]); mainColIndex++;

                    if (BudgetMasterId != "")
                    {
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtDr.Rows[n][nameof(Budget)].ToString());
                    }

                    sumdrcrCol2 = mainColIndex;
                    totCol2 = mainColIndex;
                    drcrCol2 = mainColIndex;

                    for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                    {
                        string ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();

                        //if (BudgetMasterId != "")
                        //{
                        DataView dvDrCr = new DataView(dsLocal.Tables[0])
                        {
                            RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                        };
                        DataView dvDrCrFTP = new DataView(dtLocalFTP)
                        {
                            RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                        };
                        if (p == 0)
                        {
                            plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                        }

                        var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                        DataTable dtDrCr = dvDrCr.ToTable();
                        if (dtDrCr.Rows.Count != 0)
                        {
                            try
                            {
                                oRU.SetText(ref sheet, _rowL, _pcCol, Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString()));
                                //oRU.SetText(ref sheet, _rowL, colHeaderForThePeriod, Convert.ToDouble(dtDrCr.Rows[1]["FPDRcumulative"].ToString()));

                                if (p == 0)
                                {
                                    _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                    // _Total_Amount_DateRange += Convert.ToDouble(dtDrCr.Rows[1]["FPDRcumulative"].ToString());
                                }
                            }
                            catch (Exception ex)
                            {


                            }
                        }

                        if (dvDrCrFTP.ToTable().Rows.Count != 0)
                        {
                            try
                            {
                                oRU.SetText(ref sheet, _rowL, colHeaderForThePeriod, Convert.ToDouble(dvDrCrFTP.ToTable().Rows[0]["FRDRcumulative"].ToString()));

                                _Total_Amount_DateRange += Convert.ToDouble(dvDrCrFTP.ToTable().Rows[0]["FRDRcumulative"].ToString());

                                //_Total_Amount_DateRange += Convert.ToDouble(dvDrCrFTP.ToTable[0]["FRDRcumulative"].ToString());

                            }
                            catch (Exception ex)
                            {


                            }
                        }
                        //}
                        //else
                        //{
                        //    DataView dvDrCr = new DataView(dsLocal.Tables[0])
                        //    {
                        //        RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                        //    };
                        //    if (p == 0)
                        //    {
                        //        plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                        //    }

                        //    var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                        //    DataTable dtDrCr = dvDrCr.ToTable();
                        //    if (dtDrCr.Rows.Count != 0)
                        //    {
                        //        //drcrCol2++;
                        //        oRU.SetText(ref sheet, _rowL, _pcCol, Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString()));
                        //        if (p == 0)
                        //        {
                        //            _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                        //        }
                        //    }
                        //}
                    }
                    sheet.Range[_rowL, colHeaderClosingBalance].Formula = "=SUM(" + oRU.GetColumnNameForXls(colHeaderForThePeriod) + (_rowL) + "+" + oRU.GetColumnNameForXls(colHeaderForThePeriod - 1) + _rowL + /*"-" + oRU.GetColumnNameForXls(col - 1) + _rowL +*/ ")";
                    sheet.Range[_rowL, colHeaderClosingBalance].NumberFormat = oRU.NumberFormatDecimalTwo();
                    mainColIndex = 1;
                }//DR
                Row_Total_End2 = _rowL;
                TotalExpense_DateRange(ref sheet, oRU, dtParallelCurrency, sumdrcrCol2, RowTotal_current2, Row_Total_Start2, Row_Total_End2);
                TotalExpense_DateRange(ref sheet, oRU, dtParallelCurrency, colHeaderForThePeriod - 1, RowTotal_current2, Row_Total_Start2, Row_Total_End2);


                //#region sumCalc

                _rowL++;
                var sumdrcrCol = totCol2;
                sheet.Range[_rowL, 1].Text = "Profit/Loss ";
                sheet.Range[_rowL, 1].CellStyle.Font.Bold = true;
                sheet.Range[_rowL, 1].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);

                //DR
                for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
                {
                    sumdrcrCol++;
                    sheet.Range[_rowL, sumdrcrCol].Formula = "=(" + oRU.GetColumnNameForXls(sumdrcrCol) + RowTotal_current + "-" + oRU.GetColumnNameForXls(sumdrcrCol) + RowTotal_current2 + ")";
                    sheet.Range[_rowL, sumdrcrCol].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                    sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
                }


                for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
                {
                    sumdrcrCol++;
                    sheet.Range[_rowL, sumdrcrCol].Formula = "=(" + oRU.GetColumnNameForXls(sumdrcrCol) + RowTotal_current + "-" + oRU.GetColumnNameForXls(sumdrcrCol) + RowTotal_current2 + ")";
                    sheet.Range[_rowL, sumdrcrCol].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                    sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
                }
                sheet.Range[RowTotal_current, colHeaderClosingBalance].Formula = "=SUM(" + oRU.GetColumnNameForXls(colHeaderForThePeriod - 1) + (RowTotal_current) + "+" + oRU.GetColumnNameForXls(colHeaderForThePeriod) + RowTotal_current + /*"-" + oRU.GetColumnNameForXls(col - 1) + _rowL +*/ ")";
                sheet[RowTotal_current, colHeaderClosingBalance].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[_rowL, colHeaderClosingBalance].NumberFormat = reportUtility.NumberFormatDecimalTwo(); //col++;
                sheet.Range[RowTotal_current, colHeaderClosingBalance].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[RowTotal_current, colHeaderClosingBalance].CellStyle.Font.Bold = true;

                for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
                {
                    sumdrcrCol++;
                    sheet.Range[_rowL, sumdrcrCol].Formula = "=(" + oRU.GetColumnNameForXls(sumdrcrCol) + RowTotal_current + "-" + oRU.GetColumnNameForXls(sumdrcrCol) + RowTotal_current2 + ")";
                    sheet.Range[_rowL, sumdrcrCol].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                    sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
                }


                //shet2EndxlsCol = drcrCol2;
                sheet.Range[8, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);

                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyPlantHeader(ref sheet, shet2EndxlsCol, SheetHeader, identity.CompanyId, plantName, null);
                oRU.SetText(ref sheet, 5, 2, "From Date " + fromDate + " To Date " + toDate + "", ExcelHAlign.HAlignCenter);

                //oRU.PlantHeader(ref sheet, shet2EndxlsCol, "From Date " + fromDate + " To Date " + toDate + "", ExcelHAlign.HAlignCenter, identity.PlantId);
                // oRU.PlantHeader(ref sheet, shet2EndxlsCol, "Contract NO#" + dtOrderMaster.Rows[0]["ContractNo"].ToString(), identity.PlantId);
                //oRU.PlantHeaderWithOutLogo(ref sheet, shet2EndxlsCol, identity.CompanyId, identity.PlantId);


                sheet.Range[oRU.GetColumnNameForXls(1) + 4 + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + 4].Merge();
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.Name = "Income Statement";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyPlantHeader(ref sheet, 5, SheetHeader, identity.CompanyId, plantName, null);
                oRU.SetText(ref sheet, 5, 3, "No Data Found !", ExcelHAlign.HAlignCenter);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
        }

        private DataSet GetBalanceSheetInfoDateRange(string companyId, string fromDate, string toDate)
        {
            GridParameter parameters = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var parallelCurrency = "";
                // parallelCurrency = parallelCurrencies.Length > 0 ? string.Join(",", parallelCurrencies.Select(item => "'" + item + "'")) : "' '";
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"

                    SELECT GL.Id AS AccountCodeId,--Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
                    sum(VDC.DrAmount) as DrAmount,
                    sum(VDC.CrAmount) as CrAmount,
                    sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
                    sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                   
                    ACT.BalanceType,
                    ACT.Id AS [MainHead],
                    AG.UserName AS [Level],
                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                    VD.BudgetMasterId, BUD.UserName AS Budget
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                    LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                    where act.IsBalanceSheet=0 AND v.PostingDate < '" + fromDate + @"' AND V.CompanyId='" + companyId + @"'
                    and VDC.ParallelCurrencyId IN (" + parallelCurrency + @") and v.IsPark=0
                    group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName, GL.AccountCode
                  --  ,v.PostingDate
					,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName";

                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetBalanceSheetInfoDateRangeForThePeriod(string companyId, string fromDate, string toDate)
        {
            string strSql = "";
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var parallelCurrency = "";
                // parallelCurrency = parallelCurrencies.Length > 0 ? string.Join(",", parallelCurrencies.Select(item => "'" + item + "'")) : "' '";
                //parameters = new GridParameter
                //{
                //    ExportType = "DATASET"
                //};
                strSql = @"                   
                    SELECT GL.Id AS AccountCodeId,--Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
                    
                    sum(VDC.DrAmount) as FPDrAmount,
                    sum(VDC.CrAmount) as FPCrAmount,
                    sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as FRDRcumulative,
                    sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as FRCRcumulative,
                    ACT.BalanceType,
                    ACT.Id AS [MainHead],
                    AG.UserName AS [Level],
                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                    VD.BudgetMasterId, BUD.UserName AS Budget
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                    LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                    where act.IsBalanceSheet=0 AND v.PostingDate between '" + fromDate + @"' and '" + toDate + @"' AND V.CompanyId='" + companyId + @"'
                    and VDC.ParallelCurrencyId IN (" + parallelCurrency + @") and v.IsPark=0 
                    group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName, GL.AccountCode--,v.PostingDate
					,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName ";

                return _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetBalanceSheetInfoDateRangeForThePeriodMaster(string companyId, string fromDate, string toDate)//, string[] parallelCurrencies
        {
            string strSql = "";
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var parallelCurrency = "";
                // parallelCurrency = parallelCurrencies.Length > 0 ? string.Join(",", parallelCurrencies.Select(item => "'" + item + "'")) : "' '";
                //parameters = new GridParameter
                //{
                //    ExportType = "DATASET"
                //};
                strSql = @"                   
                    SELECT GL.Id AS AccountCodeId,
                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
                    ACT.BalanceType,
                    ACT.Id AS [MainHead],
                    AG.UserName AS [Level],
                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                    VD.BudgetMasterId, BUD.UserName AS Budget
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                    LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                    where act.IsBalanceSheet=0 AND v.PostingDate <= '" + toDate + @"' AND V.CompanyId='" + companyId + @"'
                   -- and VDC.ParallelCurrencyId IN (" + parallelCurrency + @")
                    and v.IsPark=0
                    group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName, GL.AccountCode
					,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName";

                return _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion BL
        private void CreateSheet_IncomeStatement_DateRange(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, string companyId, string plantId, string plantName, string fromDate, string toDate, string[] parallelCurrency, bool isBudgetLevel, bool isActivityLevel)
        {
            DataTable dtGeneralVoucher = null;
            DataTable dtCustomerCheckByCompany = null;
            DataTable dtCr = null;
            #region List data

            DataSet dsLocal = GetIncomeStatementInfoDateRange(companyId, plantId, fromDate, toDate, parallelCurrency, isBudgetLevel, isActivityLevel);
            DataTable dtLocalFTP = GetIncomeStatementInfoDateRangeForThePeriod(companyId, fromDate, toDate, parallelCurrency, isBudgetLevel, isActivityLevel);
            //DataTable dtLocalFTPMaster = GetIncomeStatementInfoDateRangeForThePeriodMaster(companyId, fromDate, toDate, parallelCurrency);

            DataTable dtTemp = dsLocal.Tables[0].Clone();
            dtTemp.Merge(dsLocal.Tables[0]);
            dtTemp.Merge(dtLocalFTP);
            DataTable dtLocalFTPMaster = null;
            if (isBudgetLevel == true)
            {
                dtLocalFTPMaster = dtTemp.DefaultView.ToTable(true, "AccountCodeId", "ParallelCurrencyId", "CurrencyCode", "BalanceType", "MainHead", "Level", "GLGeneralInfoId", "GL", "GLGeneralInfoCode", "BudgetMasterId", "Budget");

            }
            if (isActivityLevel == true)
            {
                dtLocalFTPMaster = dtTemp.DefaultView.ToTable(true, "AccountCodeId", "ParallelCurrencyId", "CurrencyCode", "BalanceType", "MainHead", "Level", "GLGeneralInfoId", "GL", "GLGeneralInfoCode", "BudgetMasterId", "Budget", "ActivityId", "Activity");

            }
            if (isBudgetLevel == false && isActivityLevel == false)
            {
                dtLocalFTPMaster = dtTemp.DefaultView.ToTable(true, "AccountCodeId", "ParallelCurrencyId", "CurrencyCode", "BalanceType", "MainHead", "Level", "GLGeneralInfoId", "GL", "GLGeneralInfoCode");

            }

            dtGeneralVoucher = dsLocal.Tables[0];

            DataSet CustomerCheckByCompanyList = GetCustomerCheckByCompany_DateRange(companyId);
            dtCustomerCheckByCompany = CustomerCheckByCompanyList.Tables[0];

            if (dtLocalFTPMaster.Rows.Count > 0)
            {
                DataView dvAccountCode = new DataView(dsLocal.Tables[0]);
                DataTable dtAccountCode;
                if (isBudgetLevel == true)
                {
                    dtAccountCode = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId", "BudgetMasterId");
                }
                if (isActivityLevel == true)
                {
                    dtAccountCode = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId", "BudgetMasterId", "ActivityId");
                }
                if (isActivityLevel == false && isBudgetLevel == false)
                {
                    dtAccountCode = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId");
                }

                DataView dvParallelCurrency = new DataView(dtLocalFTPMaster)
                {
                    Sort = "CurrencyCode ASC"
                };
                DataTable dtParallelCurrency = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");
                DataView dvMainBody = new DataView(dsLocal.Tables[0]);
                DataTable dtDr = null;
                DataView dvDr;
                DataView dvCr; DataTable dtMainBody;


                if (isBudgetLevel == true)
                {
                    dtMainBody = dvMainBody.ToTable(true, "GLGeneralInfoCode", "GL", "Budget", "BudgetMasterId");
                    dvDr = new DataView(dtLocalFTPMaster)
                    {
                        RowFilter = "MainHead='Expense'",
                        Sort = "GLGeneralInfoCode, GL, Budget"
                    };
                    dtDr = dvDr.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget");
                    dvCr = new DataView(dtLocalFTPMaster)
                    {
                        RowFilter = "MainHead='Revenue'",
                        Sort = "GLGeneralInfoCode, GL, Budget"
                    };

                    dtCr = dvCr.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget");

                }
                if (isActivityLevel == true)
                {
                    dtMainBody = dvMainBody.ToTable(true, "GLGeneralInfoCode", "GL", "Budget", "BudgetMasterId", "Activity", "ActivityId");
                    dvDr = new DataView(dsLocal.Tables[0])
                    {
                        RowFilter = "MainHead='Expense'",
                        Sort = "GLGeneralInfoCode, GL, Budget,Activity"
                    };
                    dtDr = dvDr.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget", "ActivityId", "Activity");
                    dvCr = new DataView(dsLocal.Tables[0])
                    {
                        RowFilter = "MainHead='Revenue'",
                        Sort = "GLGeneralInfoCode, GL, Budget,Activity"
                    };
                    dtCr = dvCr.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget", "ActivityId", "Activity");
                }
                if (isActivityLevel == false && isBudgetLevel == false)
                {
                    dtMainBody = dvMainBody.ToTable(true, "GLGeneralInfoCode", "GL");
                    dvDr = new DataView(dsLocal.Tables[0])
                    {
                        RowFilter = "MainHead='Expense'",
                        Sort = "GLGeneralInfoCode, GL"
                    };
                    dtDr = dvDr.ToTable(true, "GLGeneralInfoCode", "GL");
                    dvCr = new DataView(dsLocal.Tables[0])
                    {
                        RowFilter = "MainHead='Revenue'",
                        Sort = "GLGeneralInfoCode, GL"
                    };
                    dtCr = dvCr.ToTable(true, "GLGeneralInfoCode", "GL");
                }
                if (dtLocalFTP.Rows.Count > 0)

                {
                    DataTable dtAccountCodeFTP; DataTable dtMainBodyFTP;
                    DataView dvMainBodyFTP = new DataView(dtLocalFTP);
                    DataView dvAccountCodeFTP = new DataView(dtLocalFTP);
                    DataView dvDrFTP; DataTable dtDrFTP; DataView dvCrFTP;
                    DataTable dtCrFTP;
                    if (isBudgetLevel == true)
                    {
                        dtAccountCodeFTP = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId");
                        dtAccountCodeFTP = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId", "BudgetMasterId");
                        dtMainBodyFTP = dvMainBody.ToTable(true, "GLGeneralInfoCode", "GL", "Budget", "BudgetMasterId");
                        dvDrFTP = new DataView(dtLocalFTP)
                        {
                            RowFilter = "MainHead='Expense'",
                            Sort = "GLGeneralInfoCode, GL, Budget"
                        };
                        dtDrFTP = dvDrFTP.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget");
                        dvCrFTP = new DataView(dtLocalFTP)
                        {
                            RowFilter = "MainHead='Revenue'",
                            Sort = "GLGeneralInfoCode, GL, Budget"
                        };
                        dtCrFTP = dvCrFTP.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget");

                    }
                    if (isActivityLevel == true)
                    {
                        dtAccountCodeFTP = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId");
                        dtAccountCodeFTP = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId", "BudgetMasterId");
                        dtAccountCodeFTP = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId", "BudgetMasterId", "Activity", "ActivityId");
                        dtMainBodyFTP = dvMainBody.ToTable(true, "GLGeneralInfoCode", "GL", "Budget", "BudgetMasterId", "Activity", "ActivityId");
                        dvDrFTP = new DataView(dtLocalFTP)
                        {
                            RowFilter = "MainHead='Expense'",
                            Sort = "GLGeneralInfoCode, GL, Budget,Activity"
                        };
                        dtDrFTP = dvDrFTP.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget", "Activity", "ActivityId");
                        dvCrFTP = new DataView(dtLocalFTP)
                        {
                            RowFilter = "MainHead='Revenue'",
                            Sort = "GLGeneralInfoCode, GL, Budget,Activity"
                        };
                        dtCrFTP = dvCrFTP.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget", "Activity", "ActivityId");

                    }
                    if (isActivityLevel == false && isBudgetLevel == false)
                    {
                        dtAccountCodeFTP = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId");
                        dtMainBodyFTP = dvMainBody.ToTable(true, "GLGeneralInfoCode", "GL");
                        dvDrFTP = new DataView(dtLocalFTP)
                        {
                            RowFilter = "MainHead='Expense'",
                            Sort = "GLGeneralInfoCode, GL"
                        };
                        dtDrFTP = dvDrFTP.ToTable(true, "GLGeneralInfoCode", "GL");
                        dvCrFTP = new DataView(dtLocalFTP)
                        {
                            RowFilter = "MainHead='Revenue'",
                            Sort = "GLGeneralInfoCode, GL"
                        };
                        dtCrFTP = dvCrFTP.ToTable(true, "GLGeneralInfoCode", "GL");

                    }

                    DataView dvParallelCurrencyFTP = new DataView(dtLocalFTP)
                    {
                        Sort = "CurrencyCode ASC"
                    };
                    DataTable dtParallelCurrencyFTP = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");
                }

                #region Customer Check By Company

                DataView dvCustomerCheckByCompanyBody = new DataView(CustomerCheckByCompanyList.Tables[0]);
                DataTable dtCustomerCheckByCompanyBody = dvCustomerCheckByCompanyBody.ToTable(false, "IsVoucherFromBudget");
                string Budget = dtCustomerCheckByCompanyBody.Rows[0]["IsVoucherFromBudget"].ToString();

                #endregion Customer Check By Company

                #endregion List data

                var _col = 1;
                var shet2EndxlsCol = _col;

                var _rowL = 6;
                _rowL++;

                var headreColIndex = 1;
                var mainColIndex = 1;

                oRU.SetHeaderText(ref sheet, _rowL, headreColIndex, "Account Name", 38); headreColIndex++;
                if (isBudgetLevel == true)
                {
                    oRU.SetHeaderText(ref sheet, _rowL, headreColIndex, nameof(Budget), 38); headreColIndex++;
                }
                if (isActivityLevel == true)
                {
                    oRU.SetHeaderText(ref sheet, _rowL, headreColIndex, nameof(Budget), 38); headreColIndex++;
                    oRU.SetHeaderText(ref sheet, _rowL, headreColIndex, "Activity", 38); headreColIndex++;
                }

                double _Total_Amount = 0;
                double _Total_Amount_DateRange = 0;
                string plCurrencyId = string.Empty;
                string plCurrencyCode = string.Empty;

                ArrayList alParaCurrency = new ArrayList();


                int colOpeningBalance = headreColIndex;

                oRU.SetHeaderText(ref sheet, _rowL, colOpeningBalance, "Opening Balance", 15); headreColIndex++;
                shet2EndxlsCol = headreColIndex - 1;
                int colHeaderForThePeriod = headreColIndex;
                oRU.SetHeaderText(ref sheet, _rowL, colHeaderForThePeriod, "For The Period", 15); headreColIndex++;
                int colHeaderClosingBalance = headreColIndex;

                oRU.SetHeaderText(ref sheet, _rowL, colHeaderClosingBalance, "Closing Balance", 15);

                _rowL++;

                oRU.SetText(ref sheet, _rowL, 1, "Total Revenue:", true);
                var drcrCol = 0;
                var Row_Total_Start = _rowL + 1;
                var RowTotal_current = _rowL;
                var Row_Total_End = 0;
                var sumdrcrColDateRange = 0;


                for (int n = 0; n < dtCr.Rows.Count; n++)
                {
                    mainColIndex = 1;
                    _rowL++;
                    string ActivityId = "";
                    string AccountCodeId = "";
                    string BudgetMasterId = "";
                    if (isActivityLevel == true)
                    {
                        AccountCodeId = dtCr.Rows[n]["GLGeneralInfoCode"].ToString();
                        BudgetMasterId = dtCr.Rows[n]["BudgetMasterId"].ToString();
                        ActivityId = dtCr.Rows[n]["ActivityId"].ToString();
                    }
                    else if (isBudgetLevel == true)
                    {
                        AccountCodeId = dtCr.Rows[n]["GLGeneralInfoCode"].ToString();
                        BudgetMasterId = dtCr.Rows[n]["BudgetMasterId"].ToString();
                    }
                    else
                    {
                        AccountCodeId = dtCr.Rows[n]["GLGeneralInfoCode"].ToString();
                    }

                    if (!string.IsNullOrEmpty(BudgetMasterId) && string.IsNullOrEmpty(ActivityId))
                    {
                        oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtCr.Rows[n]["GL"]); mainColIndex++;
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtCr.Rows[n][nameof(Budget)].ToString()); mainColIndex++;
                    }
                    else if (!string.IsNullOrEmpty(ActivityId))
                    {
                        oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtCr.Rows[n]["GL"]); mainColIndex++;
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtCr.Rows[n][nameof(Budget)].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtCr.Rows[n]["Activity"].ToString()); mainColIndex++;

                    }
                    else if (!string.IsNullOrEmpty(AccountCodeId))
                    {
                        oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtCr.Rows[n]["GL"]); mainColIndex++;
                    }

                    // sumdrcrCol1 = mainColIndex;
                    sumdrcrColDateRange = mainColIndex;
                    drcrCol = mainColIndex;

                    for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                    {
                        DataView dvDrCr = null;
                        DataView dvDrCrFTP = null;
                        string ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();
                        if (isBudgetLevel == true)
                        {
                            dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                            };
                            dvDrCrFTP = new DataView(dtLocalFTP)
                            {
                                RowFilter = " GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                            };
                        }
                        else if (isActivityLevel == true)
                        {
                            dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "'"
                            };
                            dvDrCrFTP = new DataView(dtLocalFTP)
                            {
                                RowFilter = "GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "'"
                            };
                        }
                        else if (isBudgetLevel == false && isActivityLevel == false)
                        {
                            dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                            };
                            dvDrCrFTP = new DataView(dtLocalFTP)
                            {
                                RowFilter = " GLGeneralInfoCode='" + AccountCodeId + "'"
                            };
                        }


                        if (p == 0)
                        {
                            plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                        }

                        var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);

                        // var _pcCol1 = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                        DataTable dtDrCr = dvDrCr.ToTable();
                        if (dtDrCr.Rows.Count != 0)
                        {
                            try
                            {
                                oRU.SetText(ref sheet, _rowL, colOpeningBalance, Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString()));
                                //oRU.SetText(ref sheet, _rowL, colHeaderForThePeriod, Convert.ToDouble(dtDrCr.Rows[1]["FRCRcumulative"].ToString()));
                                if (p == 0)
                                {
                                    _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                    //_Total_Amount_DateRange += Convert.ToDouble(dtDrCr.Rows[1]["FRCRcumulative"].ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                        DataTable dtDrCrFTP = dvDrCrFTP.ToTable();
                        if (dtDrCrFTP.Rows.Count != 0)
                        {
                            try
                            {
                                oRU.SetText(ref sheet, _rowL, colHeaderForThePeriod, Convert.ToDouble(dvDrCrFTP.ToTable().Rows[0]["CRcumulative"].ToString()));
                                if (p == 0)
                                {
                                    _Total_Amount_DateRange += Convert.ToDouble(dtDrCrFTP.Rows[0]["CRcumulative"].ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }

                    }
                    sheet.Range[_rowL, colHeaderClosingBalance].Formula = oRU.GetColumnNameForXls(colHeaderForThePeriod) + (_rowL) + "+" + oRU.GetColumnNameForXls(colOpeningBalance) + _rowL;
                    //sheet.Range[_rowL, colHeaderClosingBalance].CellStyle.Font.Bold = true;
                }
                //CR
                Row_Total_End = _rowL;

                //TotalRevenue_DateRange(ref sheet, oRU, dtParallelCurrency, colOpeningBalance - 1, RowTotal_current, Row_Total_Start, Row_Total_End);
                for (int CL = colOpeningBalance; CL <= colHeaderClosingBalance; CL++)
                {
                    sheet.Range[RowTotal_current, CL].Formula = "=SUM(" + oRU.GetColumnNameForXls(CL) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(CL) + Row_Total_End + ")";
                    sheet.Range[RowTotal_current, CL].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[RowTotal_current, CL].CellStyle.Font.Bold = true;

                }

                _rowL++;

                sheet.Range[_rowL, colHeaderClosingBalance].Formula = oRU.GetColumnNameForXls(colOpeningBalance) + (_rowL) + "+" + oRU.GetColumnNameForXls(colHeaderForThePeriod) + _rowL;
                sheet[_rowL, colHeaderClosingBalance].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[_rowL, colHeaderClosingBalance].NumberFormat = reportUtility.NumberFormatDecimalTwo(); //col++;
                sheet.Range[_rowL, colHeaderClosingBalance].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[_rowL, colHeaderClosingBalance].CellStyle.Font.Bold = true;
                //Profit/Loss
                //      RowTotal_current = _rowL;


                _rowL++;

                oRU.SetText(ref sheet, _rowL, 1, "Total Expense:", true);
                var drcrCol2 = 0;
                var totCol2 = 0;
                var Row_Total_Start2 = _rowL + 1;
                var RowTotal_current2 = _rowL;
                var Row_Total_End2 = 0;
                var sumdrcrCol2 = 0;

                for (int n = 0; n < dtDr.Rows.Count; n++)
                {
                    mainColIndex = 1;
                    _rowL++;
                    string ActivityId = "";
                    string AccountCodeId = "";
                    string BudgetMasterId = "";
                    if (isActivityLevel == true)
                    {
                        AccountCodeId = dtDr.Rows[n]["GLGeneralInfoCode"].ToString();
                        BudgetMasterId = dtDr.Rows[n]["BudgetMasterId"].ToString();
                        ActivityId = dtDr.Rows[n]["ActivityId"].ToString();
                    }
                    else if (isBudgetLevel == true && isActivityLevel == false)
                    {
                        AccountCodeId = dtDr.Rows[n]["GLGeneralInfoCode"].ToString();
                        BudgetMasterId = dtDr.Rows[n]["BudgetMasterId"].ToString();
                    }
                    else
                    {
                        AccountCodeId = dtDr.Rows[n]["GLGeneralInfoCode"].ToString();
                    }

                    oRU.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtDr.Rows[n]["GL"]); mainColIndex++;

                    if (!string.IsNullOrEmpty(BudgetMasterId) && string.IsNullOrEmpty(ActivityId))
                    {
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtDr.Rows[n][nameof(Budget)].ToString()); mainColIndex++;
                    }
                    if (!string.IsNullOrEmpty(ActivityId))
                    {
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtDr.Rows[n][nameof(Budget)].ToString()); mainColIndex++;
                        oRU.SetText(ref sheet, _rowL, mainColIndex, dtDr.Rows[n]["Activity"].ToString()); mainColIndex++;
                    }

                    sumdrcrCol2 = mainColIndex;
                    totCol2 = mainColIndex;
                    drcrCol2 = mainColIndex;

                    for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                    {
                        string ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();
                        DataView dvDrCr = new DataView(dsLocal.Tables[0]);
                        DataView dvDrCrFTP = new DataView(dtLocalFTP);
                        if (isBudgetLevel == true)
                        {
                            dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                            };
                            dvDrCrFTP = new DataView(dtLocalFTP)
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                            };
                        }
                        else if (isActivityLevel == true)
                        {
                            dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "'"
                            };
                            dvDrCrFTP = new DataView(dtLocalFTP)
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "' AND ActivityId='" + ActivityId + "'"
                            };
                        }
                        else if (isBudgetLevel == false && isActivityLevel == false)
                        {
                            dvDrCr = new DataView(dsLocal.Tables[0])
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                            };
                            dvDrCrFTP = new DataView(dtLocalFTP)
                            {
                                RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "'"
                            };
                        }


                        if (p == 0)
                        {
                            plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                        }

                        var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                        DataTable dtDrCr = dvDrCr.ToTable();
                        if (dtDrCr.Rows.Count != 0)
                        {
                            try
                            {
                                oRU.SetText(ref sheet, _rowL, colOpeningBalance, Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString()));
                                //oRU.SetText(ref sheet, _rowL, colHeaderForThePeriod, Convert.ToDouble(dtDrCr.Rows[1]["FPDRcumulative"].ToString()));

                                if (p == 0)
                                {
                                    _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                    // _Total_Amount_DateRange += Convert.ToDouble(dtDrCr.Rows[1]["FPDRcumulative"].ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }

                        if (dvDrCrFTP.ToTable().Rows.Count != 0)
                        {
                            try
                            {
                                oRU.SetText(ref sheet, _rowL, colHeaderForThePeriod, Convert.ToDouble(dvDrCrFTP.ToTable().Rows[0]["DRcumulative"].ToString()));

                                _Total_Amount_DateRange += Convert.ToDouble(dvDrCrFTP.ToTable().Rows[0]["DRcumulative"].ToString());

                                //_Total_Amount_DateRange += Convert.ToDouble(dvDrCrFTP.ToTable[0]["FRDRcumulative"].ToString());

                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }

                    }
                    sheet.Range[_rowL, colHeaderClosingBalance].Formula = oRU.GetColumnNameForXls(colOpeningBalance) + (_rowL) + "+" + oRU.GetColumnNameForXls(colHeaderForThePeriod) + _rowL;
                    sheet.Range[_rowL, colHeaderClosingBalance].NumberFormat = oRU.NumberFormatDecimalTwo();
                    // sheet.Range[_rowL, colHeaderClosingBalance].CellStyle.Font.Bold = true;
                    mainColIndex = 1;
                }//DR



                Row_Total_End2 = _rowL;

                //TotalExpense_DateRange(ref sheet, oRU, dtParallelCurrency, colOpeningBalance - 1, RowTotal_current2, Row_Total_Start2, Row_Total_End2);

                for (int CL = colOpeningBalance; CL <= colHeaderClosingBalance; CL++)
                {
                    sheet.Range[RowTotal_current2, CL].Formula = "=SUM(" + oRU.GetColumnNameForXls(CL) + Row_Total_Start2 + ":" + oRU.GetColumnNameForXls(CL) + Row_Total_End2.ToString() + ")";
                    sheet.Range[RowTotal_current2, CL].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[RowTotal_current2, CL].CellStyle.Font.Bold = true;
                }

                //#region sumCalc

                _rowL++;
                var sumdrcrCol = totCol2;
                sheet.Range[_rowL, 1].Text = "Profit/Loss ";
                sheet.Range[_rowL, 1].CellStyle.Font.Bold = true;
                sheet.Range[_rowL, 1].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[_rowL, 1].BorderAround(ExcelLineStyle.Hair);


                for (int CL = colOpeningBalance; CL <= colHeaderClosingBalance; CL++)
                {
                    sheet.Range[_rowL, CL].Formula = oRU.GetColumnNameForXls(CL) + RowTotal_current + "-" + oRU.GetColumnNameForXls(CL) + RowTotal_current2.ToString();
                    sheet.Range[_rowL, CL].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[_rowL, CL].CellStyle.Font.Bold = true;

                }
                sheet.Range[RowTotal_current, colHeaderClosingBalance].Formula = oRU.GetColumnNameForXls(colOpeningBalance) + (RowTotal_current) + "+" + oRU.GetColumnNameForXls(colHeaderForThePeriod) + RowTotal_current;
                sheet[RowTotal_current, colHeaderClosingBalance].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[_rowL, colHeaderClosingBalance].NumberFormat = reportUtility.NumberFormatDecimalTwo(); //col++;
                sheet.Range[RowTotal_current, colHeaderClosingBalance].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[RowTotal_current, colHeaderClosingBalance].CellStyle.Font.Bold = true;

                for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
                {
                    sumdrcrCol++;
                    sheet.Range[_rowL, sumdrcrCol].Formula = oRU.GetColumnNameForXls(sumdrcrCol) + RowTotal_current + "-" + oRU.GetColumnNameForXls(sumdrcrCol) + RowTotal_current2;
                    sheet.Range[_rowL, sumdrcrCol].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                    sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
                }


                //shet2EndxlsCol = drcrCol2;
                sheet.Range[8, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);

                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyPlantHeader(ref sheet, shet2EndxlsCol, SheetHeader, identity.CompanyId, plantName, null);
                oRU.SetText(ref sheet, 5, 2, "From Date " + fromDate + " To Date " + toDate + "", ExcelHAlign.HAlignCenter);

                sheet.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + 5].Merge();
                sheet.Range[oRU.GetColumnNameForXls(1) + 4 + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + 4].Merge();
                sheet.Range[Row_Total_Start, 1, _rowL, colHeaderClosingBalance].BorderAround(ExcelLineStyle.Hair);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.Name = "Income Statement";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyPlantHeader(ref sheet, 5, SheetHeader, identity.CompanyId, plantName, null);
                oRU.SetText(ref sheet, 5, 3, "No Data Found !", ExcelHAlign.HAlignCenter);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
        }

        private void CreateSheet_EntityWiseExpenseAndEarning_DateRange(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, string companyId, string plantId, string plantName, string fromDate, string toDate, string entityId, string entity, string[] parallelCurrency)
        {
            DataTable dtGeneralVoucher = null;
            DataTable dtCustomerCheckByCompany = null;

            #region List data

            DataSet dsLocal = GetEntityWiseExpenseAndEarningInfoDateRange(companyId, plantId, fromDate, toDate, entityId, parallelCurrency);
            DataTable dtLocalFTP = GetEntityWiseExpenseAndEarningInfoDateRangeForThePeriod(companyId, fromDate, toDate, entityId, parallelCurrency);
            //DataTable dtLocalFTPMaster = GetIncomeStatementInfoDateRangeForThePeriodMaster(companyId, fromDate, toDate, parallelCurrency);

            DataTable dtTemp = dsLocal.Tables[0].Clone();
            dtTemp.Merge(dsLocal.Tables[0]);
            dtTemp.Merge(dtLocalFTP);
            DataTable dtLocalFTPMaster = dtTemp.DefaultView.ToTable(true, "AccountCodeId", "ParallelCurrencyId", "CurrencyCode", "BalanceType", "MainHead", "Level", "GLGeneralInfoId", "GL", "GLGeneralInfoCode", "BudgetMasterId", "Budget");

            dtGeneralVoucher = dsLocal.Tables[0];

            DataSet CustomerCheckByCompanyList = GetCustomerCheckByCompany_DateRange(companyId);
            dtCustomerCheckByCompany = CustomerCheckByCompanyList.Tables[0];

            if (dtLocalFTPMaster.Rows.Count > 0)
            {
                DataView dvAccountCode = new DataView(dsLocal.Tables[0]);
                DataTable dtAccountCode = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId", "BudgetMasterId");

                DataView dvParallelCurrency = new DataView(dtLocalFTPMaster)
                {
                    Sort = "CurrencyCode ASC"
                };
                DataTable dtParallelCurrency = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");

                DataView dvMainBody = new DataView(dsLocal.Tables[0]);
                DataTable dtMainBody = dvMainBody.ToTable(true, "GLGeneralInfoCode", "GL", "Budget", "BudgetMasterId");

                DataView dvDr = new DataView(dtLocalFTPMaster)
                {
                    RowFilter = "MainHead='Expense'",
                    Sort = "GLGeneralInfoCode, GL, Budget"
                };
                DataTable dtDr = dvDr.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget");

                DataView dvCr = new DataView(dtLocalFTPMaster)
                {
                    RowFilter = "MainHead='Revenue'",
                    Sort = "GLGeneralInfoCode, GL, Budget"
                };
                DataTable dtCr = dvCr.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget");

                if (dtLocalFTP.Rows.Count > 0)
                {
                    DataView dvAccountCodeFTP = new DataView(dtLocalFTP);
                    DataTable dtAccountCodeFTP = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId", "BudgetMasterId");

                    DataView dvParallelCurrencyFTP = new DataView(dtLocalFTP)
                    {
                        Sort = "CurrencyCode ASC"
                    };
                    DataTable dtParallelCurrencyFTP = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");

                    DataView dvMainBodyFTP = new DataView(dtLocalFTP);
                    DataTable dtMainBodyFTP = dvMainBody.ToTable(true, "GLGeneralInfoCode", "GL", "Budget", "BudgetMasterId");

                    DataView dvDrFTP = new DataView(dtLocalFTP)
                    {
                        RowFilter = "MainHead='Expense'",
                        Sort = "GLGeneralInfoCode, GL, Budget"
                    };
                    DataTable dtDrFTP = dvDrFTP.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget");

                    DataView dvCrFTP = new DataView(dtLocalFTP)
                    {
                        RowFilter = "MainHead='Revenue'",
                        Sort = "GLGeneralInfoCode, GL, Budget"
                    };
                    DataTable dtCrFTP = dvCrFTP.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget");

                }



                #region Customer Check By Company

                DataView dvCustomerCheckByCompanyBody = new DataView(CustomerCheckByCompanyList.Tables[0]);
                DataTable dtCustomerCheckByCompanyBody = dvCustomerCheckByCompanyBody.ToTable(false, "IsVoucherFromBudget");
                string Budget = dtCustomerCheckByCompanyBody.Rows[0]["IsVoucherFromBudget"].ToString();

                #endregion Customer Check By Company

                #endregion List data

                var _col = 1;
                var shet2EndxlsCol = _col;

                var _rowL = 6;
                _rowL++;

                var headreColIndex = 1;
                var mainColIndex = 1;
                int colAccountName = headreColIndex;
                oRU.SetHeaderText(ref sheet, _rowL, colAccountName, "Account Name", 38); headreColIndex++;
                //sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
                int colBudget = headreColIndex;
                if (Budget == "True")
                {
                    oRU.SetHeaderText(ref sheet, _rowL, colBudget, nameof(Budget), 38); headreColIndex++;
                }

                double _Total_Amount = 0;
                double _Total_Amount_DateRange = 0;
                string plCurrencyId = string.Empty;
                string plCurrencyCode = string.Empty;

                ArrayList alParaCurrency = new ArrayList();




                int colOpeningBalance = headreColIndex;


                oRU.SetHeaderText(ref sheet, _rowL, colOpeningBalance, "Opening Balance", 15);
                shet2EndxlsCol = headreColIndex - 1;
                headreColIndex++;

                int colHeaderForThePeriod = headreColIndex;
                oRU.SetHeaderText(ref sheet, _rowL, colHeaderForThePeriod
                    , "For The Period", 15); headreColIndex++;
                int colHeaderClosingBalance = headreColIndex;

                oRU.SetHeaderText(ref sheet, _rowL, colHeaderClosingBalance, "Closing Balance", 15);

                _rowL++;

                oRU.SetText(ref sheet, _rowL, 1, "Total Revenue:", true);
                var drcrCol = 0;
                var Row_Total_Start = _rowL + 1;
                var RowTotal_current = _rowL;
                var Row_Total_End = 0;
                var sumdrcrColDateRange = 0;

                for (int n = 0; n < dtCr.Rows.Count; n++)
                {
                    _rowL++;
                    string AccountCodeId = dtCr.Rows[n]["GLGeneralInfoCode"].ToString();
                    string BudgetMasterId = dtCr.Rows[n]["BudgetMasterId"].ToString();
                    oRU.SetText(ref sheet, _rowL, colAccountName, AccountCodeId + " - " + dtCr.Rows[n]["GL"]); ; mainColIndex++;


                    if (BudgetMasterId != "")
                    {
                        //Budget = dtCustomerCheckByCompanyBody.Rows[n]["IsVoucherFromBudget"].ToString();
                        oRU.SetText(ref sheet, _rowL, colBudget, dtCr.Rows[n][nameof(Budget)].ToString());
                    }

                    // sumdrcrCol1 = mainColIndex;
                    sumdrcrColDateRange = mainColIndex;
                    drcrCol = mainColIndex;

                    for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                    {
                        string ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();


                        DataView dvDrCr = new DataView(dsLocal.Tables[0])
                        {
                            RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                        };
                        DataView dvDrCrFTP = new DataView(dtLocalFTP)
                        {
                            RowFilter = " GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                        };
                        if (p == 0)
                        {
                            plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                        }

                        var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);

                        // var _pcCol1 = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                        DataTable dtDrCr = dvDrCr.ToTable();
                        if (dtDrCr.Rows.Count != 0)
                        {
                            try
                            {
                                oRU.SetText(ref sheet, _rowL, colOpeningBalance, Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString()));
                                //oRU.SetText(ref sheet, _rowL, colHeaderForThePeriod, Convert.ToDouble(dtDrCr.Rows[1]["FRCRcumulative"].ToString()));
                                if (p == 0)
                                {
                                    _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                    //_Total_Amount_DateRange += Convert.ToDouble(dtDrCr.Rows[1]["FRCRcumulative"].ToString());
                                }
                            }
                            catch (Exception ex)
                            {


                            }
                        }

                        if (dvDrCrFTP.ToTable().Rows.Count != 0)
                        {
                            try
                            {
                                oRU.SetText(ref sheet, _rowL, colHeaderForThePeriod, Convert.ToDouble(dvDrCrFTP.ToTable().Rows[0]["CRcumulative"].ToString()));
                                if (p == 0)
                                {
                                    _Total_Amount_DateRange += Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                                }
                            }
                            catch (Exception ex)
                            {


                            }
                        }

                    }
                    sheet.Range[_rowL, colHeaderClosingBalance].Formula = oRU.GetColumnNameForXls(colHeaderForThePeriod) + (_rowL) + "+" + oRU.GetColumnNameForXls(colOpeningBalance) + _rowL;
                    //sheet.Range[_rowL, colHeaderClosingBalance].CellStyle.Font.Bold = true;
                }
                //CR
                Row_Total_End = _rowL;

                //TotalRevenue_DateRange(ref sheet, oRU, dtParallelCurrency, colOpeningBalance - 1, RowTotal_current, Row_Total_Start, Row_Total_End);
                for (int CL = colOpeningBalance; CL <= colHeaderClosingBalance; CL++)
                {
                    sheet.Range[RowTotal_current, CL].Formula = "=SUM(" + oRU.GetColumnNameForXls(CL) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(CL) + Row_Total_End + ")";
                    sheet.Range[RowTotal_current, CL].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[RowTotal_current, CL].CellStyle.Font.Bold = true;

                }

                _rowL++;

                sheet.Range[_rowL, colHeaderClosingBalance].Formula = oRU.GetColumnNameForXls(colOpeningBalance) + (_rowL) + "+" + oRU.GetColumnNameForXls(colHeaderForThePeriod) + _rowL;
                sheet[_rowL, colHeaderClosingBalance].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[_rowL, colHeaderClosingBalance].NumberFormat = reportUtility.NumberFormatDecimalTwo(); //col++;
                sheet.Range[_rowL, colHeaderClosingBalance].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[_rowL, colHeaderClosingBalance].CellStyle.Font.Bold = true;
                //Profit/Loss
                //      RowTotal_current = _rowL;


                _rowL++;

                oRU.SetText(ref sheet, _rowL, 1, "Total Expense:", true);
                var drcrCol2 = 0;
                var totCol2 = 0;
                var Row_Total_Start2 = _rowL + 1;
                var RowTotal_current2 = _rowL;
                var Row_Total_End2 = 0;
                var sumdrcrCol2 = 0;

                for (int n = 0; n < dtDr.Rows.Count; n++)
                {
                    _rowL++;
                    string AccountCodeId = dtDr.Rows[n]["GLGeneralInfoCode"].ToString();
                    string BudgetMasterId = dtDr.Rows[n]["BudgetMasterId"].ToString();
                    oRU.SetText(ref sheet, _rowL, colAccountName, AccountCodeId + " - " + dtDr.Rows[n]["GL"]); mainColIndex++;

                    if (BudgetMasterId != "")
                    {
                        oRU.SetText(ref sheet, _rowL, colBudget, dtDr.Rows[n][nameof(Budget)].ToString());
                    }

                    sumdrcrCol2 = mainColIndex;
                    totCol2 = mainColIndex;
                    drcrCol2 = mainColIndex;

                    for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                    {
                        string ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();

                        //if (BudgetMasterId != "")
                        //{
                        DataView dvDrCr = new DataView(dsLocal.Tables[0])
                        {
                            RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                        };
                        DataView dvDrCrFTP = new DataView(dtLocalFTP)
                        {
                            RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                        };
                        if (p == 0)
                        {
                            plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                        }

                        var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                        DataTable dtDrCr = dvDrCr.ToTable();
                        if (dtDrCr.Rows.Count != 0)
                        {
                            try
                            {
                                oRU.SetText(ref sheet, _rowL, colOpeningBalance, Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString()));
                                //oRU.SetText(ref sheet, _rowL, colHeaderForThePeriod, Convert.ToDouble(dtDrCr.Rows[1]["FPDRcumulative"].ToString()));

                                if (p == 0)
                                {
                                    _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                                    // _Total_Amount_DateRange += Convert.ToDouble(dtDrCr.Rows[1]["FPDRcumulative"].ToString());
                                }
                            }
                            catch (Exception ex)
                            {


                            }
                        }

                        if (dvDrCrFTP.ToTable().Rows.Count != 0)
                        {
                            try
                            {
                                oRU.SetText(ref sheet, _rowL, colHeaderForThePeriod, Convert.ToDouble(dvDrCrFTP.ToTable().Rows[0]["DRcumulative"].ToString()));

                                _Total_Amount_DateRange += Convert.ToDouble(dvDrCrFTP.ToTable().Rows[0]["DRcumulative"].ToString());

                                //_Total_Amount_DateRange += Convert.ToDouble(dvDrCrFTP.ToTable[0]["FRDRcumulative"].ToString());

                            }
                            catch (Exception ex)
                            {


                            }
                        }

                    }
                    sheet.Range[_rowL, colHeaderClosingBalance].Formula = oRU.GetColumnNameForXls(colOpeningBalance) + (_rowL) + "+" + oRU.GetColumnNameForXls(colHeaderForThePeriod) + _rowL;
                    sheet.Range[_rowL, colHeaderClosingBalance].NumberFormat = oRU.NumberFormatDecimalTwo();
                    // sheet.Range[_rowL, colHeaderClosingBalance].CellStyle.Font.Bold = true;
                    mainColIndex = 1;
                }//DR



                Row_Total_End2 = _rowL;

                //TotalExpense_DateRange(ref sheet, oRU, dtParallelCurrency, colOpeningBalance - 1, RowTotal_current2, Row_Total_Start2, Row_Total_End2);

                for (int CL = colOpeningBalance; CL <= colHeaderClosingBalance; CL++)
                {
                    sheet.Range[RowTotal_current2, CL].Formula = "=SUM(" + oRU.GetColumnNameForXls(CL) + Row_Total_Start2 + ":" + oRU.GetColumnNameForXls(CL) + Row_Total_End2.ToString() + ")";
                    sheet.Range[RowTotal_current2, CL].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[RowTotal_current2, CL].CellStyle.Font.Bold = true;
                }

                //#region sumCalc

                _rowL++;
                var sumdrcrCol = totCol2;
                sheet.Range[_rowL, 1].Text = "Profit/Loss ";
                sheet.Range[_rowL, 1].CellStyle.Font.Bold = true;
                sheet.Range[_rowL, 1].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);


                for (int CL = colOpeningBalance; CL <= colHeaderClosingBalance; CL++)
                {
                    sheet.Range[_rowL, CL].Formula = oRU.GetColumnNameForXls(CL) + RowTotal_current + "-" + oRU.GetColumnNameForXls(CL) + RowTotal_current2.ToString();
                    sheet.Range[_rowL, CL].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[_rowL, CL].CellStyle.Font.Bold = true;

                }
                sheet.Range[RowTotal_current, colHeaderClosingBalance].Formula = oRU.GetColumnNameForXls(colOpeningBalance) + (RowTotal_current) + "+" + oRU.GetColumnNameForXls(colHeaderForThePeriod) + RowTotal_current;
                sheet[RowTotal_current, colHeaderClosingBalance].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[_rowL, colHeaderClosingBalance].NumberFormat = reportUtility.NumberFormatDecimalTwo(); //col++;
                sheet.Range[RowTotal_current, colHeaderClosingBalance].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[RowTotal_current, colHeaderClosingBalance].CellStyle.Font.Bold = true;

                for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
                {
                    sumdrcrCol++;
                    sheet.Range[_rowL, sumdrcrCol].Formula = oRU.GetColumnNameForXls(sumdrcrCol) + RowTotal_current + "-" + oRU.GetColumnNameForXls(sumdrcrCol) + RowTotal_current2;
                    sheet.Range[_rowL, sumdrcrCol].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                    sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
                }


                //shet2EndxlsCol = drcrCol2;
                sheet.Range[8, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);

                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyPlantHeader(ref sheet, shet2EndxlsCol, SheetHeader, identity.CompanyId, plantName, null);
                oRU.SetTextEntity(ref sheet, 5, 2, entity, ExcelHAlign.HAlignCenter);

                oRU.SetText(ref sheet, 6, 2, "From Date " + fromDate + " To Date " + toDate + "", ExcelHAlign.HAlignCenter);

                sheet.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + 5].Merge();
                sheet.Range[oRU.GetColumnNameForXls(1) + 4 + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + 4].Merge();
                sheet.Range[oRU.GetColumnNameForXls(1) + 6 + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + 6].Merge();

                sheet.Range[Row_Total_Start, 1, _rowL, colHeaderClosingBalance].BorderAround(ExcelLineStyle.Hair);
                oRU.PageSetup(ref sheet, 6, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.Name = "Entity Wise Expense and Earning report";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyPlantHeader(ref sheet, 6, SheetHeader, identity.CompanyId, plantName, null);
                oRU.SetText(ref sheet, 6, 3, "No Data Found !", ExcelHAlign.HAlignCenter);
                oRU.PageSetup(ref sheet, 6, ExcelPageOrientation.Portrait);
            }
        }

        private void CreateSheet_EntityWiseExpenseAndEarning_DateRange_ActivityLevel(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, string companyId, string plantId, string plantName, string fromDate, string toDate, string entityId, string entity, string[] parallelCurrency)
        {
            DataTable dtGeneralVoucher = null;
            DataTable dtCustomerCheckByCompany = null;

            #region List data

            //DataSet dsLocal = GetEntityWiseExpenseAndEarningInfoDateRange(companyId, plantId, fromDate, toDate, entityId, parallelCurrency);
            DataTable dtLocalFTP = GetEntityWiseExpenseAndEarningInfoDateRangeForThePeriod_ActivityLevel(companyId, fromDate, toDate, entityId, parallelCurrency);
            

            //DataTable dtTemp = dsLocal.Tables[0].Clone();
            //dtTemp.Merge(dsLocal.Tables[0]);
            //dtTemp.Merge(dtLocalFTP);
            DataTable dtLocalFTPMaster = dtLocalFTP.DefaultView.ToTable(true, "AccountCodeId", "ParallelCurrencyId", "CurrencyCode", "BalanceType", "MainHead", "Level", "GLGeneralInfoId", "GL", "GLGeneralInfoCode", "BudgetMasterId", "BudgetCategory", "BudgetSubCategory", "Budget", "ActivityId", "Activity", "ControlId", "ForTheDay", "DRcumulative", "CRcumulative", "ForTheFiscalYear");

            //dtGeneralVoucher = dsLocal.Tables[0];

            DataSet CustomerCheckByCompanyList = GetCustomerCheckByCompany_DateRange(companyId);
            dtCustomerCheckByCompany = CustomerCheckByCompanyList.Tables[0];

            if (dtLocalFTPMaster.Rows.Count > 0)
            {
                //DataView dvAccountCode = new DataView(dsLocal.Tables[0]);
                //DataTable dtAccountCode = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId", "BudgetMasterId");

                DataView dvParallelCurrency = new DataView(dtLocalFTPMaster)
                {
                    Sort = "CurrencyCode ASC"
                };
                DataTable dtParallelCurrency = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");

                //DataView dvMainBody = new DataView(dsLocal.Tables[0]);
                //DataTable dtMainBody = dvMainBody.ToTable(true, "GLGeneralInfoCode", "GL", "Budget", "BudgetMasterId");

                DataView dvDr = new DataView(dtLocalFTPMaster)
                {
                    RowFilter = "MainHead='Expense'",
                    Sort = "GLGeneralInfoCode, GL, Budget ,Activity"
                };
                DataTable dtDr = dvDr.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "MainHead", "BudgetCategory", "BudgetSubCategory", "Budget", "ActivityId", "Activity", "ControlId", "ForTheDay", "DRcumulative", "CRcumulative", "ForTheFiscalYear");

                DataView dvCr = new DataView(dtLocalFTPMaster)
                {
                    RowFilter = "MainHead='Revenue'",
                    Sort = "GLGeneralInfoCode, GL, Budget ,Activity"
                };
                DataTable dtCr = dvCr.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "MainHead", "BudgetCategory", "BudgetSubCategory", "Budget", "ActivityId", "Activity", "ControlId", "ForTheDay", "DRcumulative", "CRcumulative", "ForTheFiscalYear");

                //if (dtLocalFTP.Rows.Count > 0)
                //{
                //    DataView dvAccountCodeFTP = new DataView(dtLocalFTP);
                //    //DataTable dtAccountCodeFTP = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId", "BudgetMasterId");

                //    DataView dvParallelCurrencyFTP = new DataView(dtLocalFTP)
                //    {
                //        Sort = "CurrencyCode ASC"
                //    };
                //    DataTable dtParallelCurrencyFTP = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");

                //    DataView dvMainBodyFTP = new DataView(dtLocalFTP);
                //    //DataTable dtMainBodyFTP = dvMainBody.ToTable(true, "GLGeneralInfoCode", "GL", "Budget", "BudgetMasterId");

                //    DataView dvDrFTP = new DataView(dtLocalFTP)
                //    {
                //        RowFilter = "MainHead='Expense'",
                //        Sort = "GLGeneralInfoCode, GL, Budget ,Activity"
                //    };
                //    DataTable dtDrFTP = dvDrFTP.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget", "ActivityId", "Activity");

                //    DataView dvCrFTP = new DataView(dtLocalFTP)
                //    {
                //        RowFilter = "MainHead='Revenue'",
                //        Sort = "GLGeneralInfoCode, GL, Budget ,Activity"
                //    };
                //    DataTable dtCrFTP = dvCrFTP.ToTable(true, "GLGeneralInfoCode", "GL", "BudgetMasterId", "Budget", "ActivityId", "Activity");

                //}



                //#region Customer Check By Company

                //DataView dvCustomerCheckByCompanyBody = new DataView(CustomerCheckByCompanyList.Tables[0]);
                //DataTable dtCustomerCheckByCompanyBody = dvCustomerCheckByCompanyBody.ToTable(false, "IsVoucherFromBudget");
                //string Budget = dtCustomerCheckByCompanyBody.Rows[0]["IsVoucherFromBudget"].ToString();

                //#endregion Customer Check By Company

                #endregion List data

                var _col = 1;
                var shet2EndxlsCol = _col;

                var _rowL = 6;
                _rowL++;

                var headreColIndex = 1;
                var mainColIndex = 1;
                int colAccountName = headreColIndex;
                oRU.SetHeaderText(ref sheet, _rowL, colAccountName, "Account Type", 25); headreColIndex++;
                int colBudgetCategory = headreColIndex;
                oRU.SetHeaderText(ref sheet, _rowL, colBudgetCategory, "Budget Category", 20); headreColIndex++;
                int colBudgetSubCategory = headreColIndex;
                oRU.SetHeaderText(ref sheet, _rowL, colBudgetSubCategory, "Budget Sub Category", 20); headreColIndex++;
                int colBudget = headreColIndex;
                oRU.SetHeaderText(ref sheet, _rowL, colBudget, "Budget", 20); headreColIndex++;
                int colActivity = headreColIndex;
                oRU.SetHeaderText(ref sheet, _rowL, colActivity, "Activity", 20); headreColIndex++;
                int colControlId = headreColIndex;
                oRU.SetHeaderText(ref sheet, _rowL, colControlId, "ControlId", 10); headreColIndex++;
                int colForTheDay = headreColIndex;
                oRU.SetHeaderText(ref sheet, _rowL, colForTheDay, "For The Day", 15); headreColIndex++;
                int colForThePeriod = headreColIndex;
                oRU.SetHeaderText(ref sheet, _rowL, colForThePeriod, "For The Period", 15); headreColIndex++;
                int colForTheFiscalYear = headreColIndex;
                oRU.SetHeaderText(ref sheet, _rowL, colForTheFiscalYear, "For The FiscalYear", 15); 


                double _Total_Amount = 0;
                double _Total_Amount_DateRange = 0;
                string plCurrencyId = string.Empty;
                string plCurrencyCode = string.Empty;

                ArrayList alParaCurrency = new ArrayList();




                //int colOpeningBalance = headreColIndex;


                //oRU.SetHeaderText(ref sheet, _rowL, colOpeningBalance, "Opening Balance", 15);
                //shet2EndxlsCol = headreColIndex - 1;
                //headreColIndex++;

                //int colHeaderForThePeriod = headreColIndex;
                //oRU.SetHeaderText(ref sheet, _rowL, colHeaderForThePeriod
                //    , "For The Period", 15); headreColIndex++;
                //int colHeaderClosingBalance = headreColIndex;

                //oRU.SetHeaderText(ref sheet, _rowL, colHeaderClosingBalance, "Closing Balance", 15);

                _rowL++;

                oRU.SetText(ref sheet, _rowL, 1, "Total Revenue:", true);
                var drcrCol = 0;
                var Row_Total_Start = _rowL + 1;
                var RowTotal_current = _rowL;
                var Row_Total_End = 0;
                var sumdrcrColDateRange = 0;

                for (int n = 0; n < dtCr.Rows.Count; n++)
                {
                    _rowL++;
                    //string AccountCodeId = dtCr.Rows[n]["GLGeneralInfoCode"].ToString();
                    //string BudgetMasterId = dtCr.Rows[n]["BudgetMasterId"].ToString();
                    oRU.SetText(ref sheet, _rowL, colAccountName,dtCr.Rows[n]["MainHead"].ToString()); mainColIndex++;
                    oRU.SetText(ref sheet, _rowL, colBudgetCategory, dtCr.Rows[n]["BudgetCategory"].ToString()); 
                    oRU.SetText(ref sheet, _rowL, colBudgetSubCategory, dtCr.Rows[n]["BudgetSubCategory"].ToString()); 
                    oRU.SetText(ref sheet, _rowL, colBudget, dtCr.Rows[n]["Budget"].ToString());
                    oRU.SetText(ref sheet, _rowL, colActivity, dtCr.Rows[n]["Activity"].ToString()); 
                    oRU.SetText(ref sheet, _rowL, colControlId, dtCr.Rows[n]["ControlId"].ToString()); 
                    oRU.SetText(ref sheet, _rowL, colForTheDay, Convert.ToDouble(dtCr.Rows[n]["ForTheDay"].ToString())); 
                    oRU.SetText(ref sheet, _rowL, colForThePeriod, Convert.ToDouble(dtCr.Rows[n]["CRcumulative"].ToString())); 
                    oRU.SetText(ref sheet, _rowL, colForTheFiscalYear, Convert.ToDouble(dtCr.Rows[n]["ForTheFiscalYear"].ToString())); 


                    //if (BudgetMasterId != "")
                    //{
                    //    //Budget = dtCustomerCheckByCompanyBody.Rows[n]["IsVoucherFromBudget"].ToString();
                    //    oRU.SetText(ref sheet, _rowL, colBudget, dtCr.Rows[n][nameof(Budget)].ToString());
                    //}

                    // sumdrcrCol1 = mainColIndex;
                    sumdrcrColDateRange = mainColIndex;
                    drcrCol = mainColIndex;

                    //for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                    //{
                    //    string ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();


                    //    //DataView dvDrCr = new DataView(dsLocal.Tables[0])
                    //    //{
                    //    //    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                    //    //};
                    //    DataView dvDrCrFTP = new DataView(dtLocalFTP)
                    //    {
                    //        RowFilter = " GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                    //    };
                    //    if (p == 0)
                    //    {
                    //        plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                    //    }

                    //    var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);

                    //    // var _pcCol1 = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                    //    //DataTable dtDrCr = dvDrCr.ToTable();
                    //    //if (dtDrCr.Rows.Count != 0)
                    //    //{
                    //    //    try
                    //    //    {
                    //    //        oRU.SetText(ref sheet, _rowL, colOpeningBalance, Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString()));
                    //    //        //oRU.SetText(ref sheet, _rowL, colHeaderForThePeriod, Convert.ToDouble(dtDrCr.Rows[1]["FRCRcumulative"].ToString()));
                    //    //        if (p == 0)
                    //    //        {
                    //    //            _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                    //    //            //_Total_Amount_DateRange += Convert.ToDouble(dtDrCr.Rows[1]["FRCRcumulative"].ToString());
                    //    //        }
                    //    //    }
                    //    //    catch (Exception ex)
                    //    //    {


                    //    //    }
                    //    //}

                    //    if (dvDrCrFTP.ToTable().Rows.Count != 0)
                    //    {
                    //        try
                    //        {
                    //            oRU.SetText(ref sheet, _rowL, colHeaderForThePeriod, Convert.ToDouble(dvDrCrFTP.ToTable().Rows[0]["CRcumulative"].ToString()));
                    //            //if (p == 0)
                    //            //{
                    //            //    _Total_Amount_DateRange += Convert.ToDouble(dtDrCr.Rows[0]["CRcumulative"].ToString());
                    //            //}
                    //        }
                    //        catch (Exception ex)
                    //        {


                    //        }
                    //    }

                    //}
                    //sheet.Range[_rowL, colHeaderClosingBalance].Formula = oRU.GetColumnNameForXls(colHeaderForThePeriod) + (_rowL) + "+" + oRU.GetColumnNameForXls(colOpeningBalance) + _rowL;
                    ////sheet.Range[_rowL, colHeaderClosingBalance].CellStyle.Font.Bold = true;
                }
                //CR
                Row_Total_End = _rowL;

                //TotalRevenue_DateRange(ref sheet, oRU, dtParallelCurrency, colOpeningBalance - 1, RowTotal_current, Row_Total_Start, Row_Total_End);
                for (int CL = colForTheDay; CL <= colForTheFiscalYear; CL++)
                {
                    sheet.Range[RowTotal_current, CL].Formula = "=SUM(" + oRU.GetColumnNameForXls(CL) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(CL) + Row_Total_End + ")";
                    sheet.Range[RowTotal_current, CL].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[RowTotal_current, CL].CellStyle.Font.Bold = true;

                }

                //_rowL++;

                //sheet.Range[_rowL, colHeaderClosingBalance].Formula = oRU.GetColumnNameForXls(colOpeningBalance) + (_rowL) + "+" + oRU.GetColumnNameForXls(colHeaderForThePeriod) + _rowL;
                //sheet[_rowL, colHeaderClosingBalance].VerticalAlignment = ExcelVAlign.VAlignTop;
                ////sheet.Range[_rowL, colHeaderClosingBalance].NumberFormat = reportUtility.NumberFormatDecimalTwo(); //col++;
                //sheet.Range[_rowL, colHeaderClosingBalance].NumberFormat = oRU.NumberFormatDecimalTwo();
                //sheet.Range[_rowL, colHeaderClosingBalance].CellStyle.Font.Bold = true;
                //Profit/Loss
                //      RowTotal_current = _rowL;


                _rowL++;

                oRU.SetText(ref sheet, _rowL, 1, "Total Expense:", true);
                var drcrCol2 = 0;
                var totCol2 = 0;
                var Row_Total_Start2 = _rowL + 1;
                var RowTotal_current2 = _rowL;
                var Row_Total_End2 = 0;
                var sumdrcrCol2 = 0;

                for (int n = 0; n < dtDr.Rows.Count; n++)
                {
                    _rowL++;
                    //string AccountCodeId = dtDr.Rows[n]["GLGeneralInfoCode"].ToString();
                    //string BudgetMasterId = dtDr.Rows[n]["BudgetMasterId"].ToString();
                    oRU.SetText(ref sheet, _rowL, colAccountName, dtDr.Rows[n]["MainHead"].ToString()); mainColIndex++;
                    oRU.SetText(ref sheet, _rowL, colBudgetCategory, dtDr.Rows[n]["BudgetCategory"].ToString()); 
                    oRU.SetText(ref sheet, _rowL, colBudgetSubCategory, dtDr.Rows[n]["BudgetSubCategory"].ToString()); 
                    oRU.SetText(ref sheet, _rowL, colBudget, dtDr.Rows[n]["Budget"].ToString()); 
                    oRU.SetText(ref sheet, _rowL, colActivity, dtDr.Rows[n]["Activity"].ToString()); 
                    oRU.SetText(ref sheet, _rowL, colControlId, dtDr.Rows[n]["ControlId"].ToString()); 
                    oRU.SetText(ref sheet, _rowL, colForTheDay, Convert.ToDouble(dtDr.Rows[n]["ForTheDay"].ToString())); 
                    oRU.SetText(ref sheet, _rowL, colForThePeriod, Convert.ToDouble(dtDr.Rows[n]["DRcumulative"].ToString())); 
                    oRU.SetText(ref sheet, _rowL, colForTheFiscalYear, Convert.ToDouble(dtDr.Rows[n]["ForTheFiscalYear"].ToString()));

                    //if (BudgetMasterId != "")
                    //{
                    //    oRU.SetText(ref sheet, _rowL, colBudget, dtDr.Rows[n][nameof(Budget)].ToString());
                    //}

                    sumdrcrCol2 = mainColIndex;
                    totCol2 = mainColIndex;
                    drcrCol2 = mainColIndex;

                    //for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                    //{
                    //    string ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();

                       
                    //    //DataView dvDrCr = new DataView(dsLocal.Tables[0])
                    //    //{
                    //    //    RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                    //    //};
                    //    DataView dvDrCrFTP = new DataView(dtLocalFTP)
                    //    {
                    //        RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND BudgetMasterId='" + BudgetMasterId + "'"
                    //    };
                    //    if (p == 0)
                    //    {
                    //        plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                    //    }

                    //    var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                    //    //DataTable dtDrCr = dvDrCr.ToTable();
                    //    //if (dtDrCr.Rows.Count != 0)
                    //    //{
                    //    //    try
                    //    //    {
                    //    //        oRU.SetText(ref sheet, _rowL, colOpeningBalance, Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString()));
                    //    //        //oRU.SetText(ref sheet, _rowL, colHeaderForThePeriod, Convert.ToDouble(dtDrCr.Rows[1]["FPDRcumulative"].ToString()));

                    //    //        if (p == 0)
                    //    //        {
                    //    //            _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["DRcumulative"].ToString());
                    //    //            // _Total_Amount_DateRange += Convert.ToDouble(dtDrCr.Rows[1]["FPDRcumulative"].ToString());
                    //    //        }
                    //    //    }
                    //    //    catch (Exception ex)
                    //    //    {


                    //    //    }
                    //    //}

                    //    if (dvDrCrFTP.ToTable().Rows.Count != 0)
                    //    {
                    //        try
                    //        {
                    //            oRU.SetText(ref sheet, _rowL, colHeaderForThePeriod, Convert.ToDouble(dvDrCrFTP.ToTable().Rows[0]["DRcumulative"].ToString()));

                    //            _Total_Amount_DateRange += Convert.ToDouble(dvDrCrFTP.ToTable().Rows[0]["DRcumulative"].ToString());

                    //            //_Total_Amount_DateRange += Convert.ToDouble(dvDrCrFTP.ToTable[0]["FRDRcumulative"].ToString());

                    //        }
                    //        catch (Exception ex)
                    //        {


                    //        }
                    //    }

                    //}
                    //sheet.Range[_rowL, colHeaderClosingBalance].Formula = oRU.GetColumnNameForXls(colOpeningBalance) + (_rowL) + "+" + oRU.GetColumnNameForXls(colHeaderForThePeriod) + _rowL;
                    //sheet.Range[_rowL, colHeaderClosingBalance].NumberFormat = oRU.NumberFormatDecimalTwo();
                    // sheet.Range[_rowL, colHeaderClosingBalance].CellStyle.Font.Bold = true;
                    //mainColIndex = 1;
                }//DR



                Row_Total_End2 = _rowL;

                //TotalExpense_DateRange(ref sheet, oRU, dtParallelCurrency, colOpeningBalance - 1, RowTotal_current2, Row_Total_Start2, Row_Total_End2);

                for (int CL = colForTheDay; CL <= colForTheFiscalYear; CL++)
                {
                    sheet.Range[RowTotal_current2, CL].Formula = "=SUM(" + oRU.GetColumnNameForXls(CL) + Row_Total_Start2 + ":" + oRU.GetColumnNameForXls(CL) + Row_Total_End2.ToString() + ")";
                    sheet.Range[RowTotal_current2, CL].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[RowTotal_current2, CL].CellStyle.Font.Bold = true;
                }

                //#region sumCalc

                _rowL++;
                var sumdrcrCol = totCol2;
                sheet.Range[_rowL, 1].Text = "Profit/Loss ";
                sheet.Range[_rowL, 1].CellStyle.Font.Bold = true;
                sheet.Range[_rowL, 1].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);


                for (int CL = colForTheDay; CL <= colForTheFiscalYear; CL++)
                {
                    sheet.Range[_rowL, CL].Formula = oRU.GetColumnNameForXls(CL) + RowTotal_current + "-" + oRU.GetColumnNameForXls(CL) + RowTotal_current2.ToString();
                    sheet.Range[_rowL, CL].NumberFormat = oRU.NumberFormatDecimalTwo();
                    sheet.Range[_rowL, CL].CellStyle.Font.Bold = true;

                }
                //sheet.Range[RowTotal_current, colHeaderClosingBalance].Formula = oRU.GetColumnNameForXls(colOpeningBalance) + (RowTotal_current) + "+" + oRU.GetColumnNameForXls(colHeaderForThePeriod) + RowTotal_current;
                //sheet[RowTotal_current, colHeaderClosingBalance].VerticalAlignment = ExcelVAlign.VAlignTop;
                ////sheet.Range[_rowL, colHeaderClosingBalance].NumberFormat = reportUtility.NumberFormatDecimalTwo(); //col++;
                //sheet.Range[RowTotal_current, colHeaderClosingBalance].NumberFormat = oRU.NumberFormatDecimalTwo();
                //sheet.Range[RowTotal_current, colHeaderClosingBalance].CellStyle.Font.Bold = true;

                //for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
                //{
                //    sumdrcrCol++;
                //    sheet.Range[_rowL, sumdrcrCol].Formula = oRU.GetColumnNameForXls(sumdrcrCol) + RowTotal_current + "-" + oRU.GetColumnNameForXls(sumdrcrCol) + RowTotal_current2;
                //    sheet.Range[_rowL, sumdrcrCol].NumberFormat = oRU.NumberFormatDecimalTwo();
                //    sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                //    sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
                //}


                //shet2EndxlsCol = drcrCol2;
                sheet.Range[8, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);

                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyPlantHeader(ref sheet, shet2EndxlsCol, SheetHeader, identity.CompanyId, plantName, null);
                oRU.SetTextEntity(ref sheet, 5, 2, entity, ExcelHAlign.HAlignCenter);

                oRU.SetText(ref sheet, 6, 2, "From Date " + fromDate + " To Date " + toDate + "", ExcelHAlign.HAlignCenter);

                sheet.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + 5].Merge();
                sheet.Range[oRU.GetColumnNameForXls(1) + 4 + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + 4].Merge();
                sheet.Range[oRU.GetColumnNameForXls(1) + 6 + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + 6].Merge();

                sheet.Range[Row_Total_Start, 1, _rowL, colForTheFiscalYear].BorderAround(ExcelLineStyle.Hair);
                oRU.PageSetup(ref sheet, 6, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.Name = "Entity Wise Expense and Earning report";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyPlantHeader(ref sheet, 6, SheetHeader, identity.CompanyId, plantName, null);
                oRU.SetText(ref sheet, 6, 3, "No Data Found !", ExcelHAlign.HAlignCenter);
                oRU.PageSetup(ref sheet, 6, ExcelPageOrientation.Portrait);
            }
        }


        private void TotalRevenue_DateRange(ref IWorksheet sheet, ReportUtility oRU, DataTable dtParallelCurrency, int sumdrcrColDateRange, int RowTotal_current, int Row_Total_Start, int Row_total_End)
        {
            for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
            {
                //Row_Total_Start = _rowL;
                sumdrcrColDateRange++;
                sheet.Range[RowTotal_current, sumdrcrColDateRange].Formula = "=SUM(" + oRU.GetColumnNameForXls(sumdrcrColDateRange) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(sumdrcrColDateRange) + Row_total_End + ")";
                sheet.Range[RowTotal_current, sumdrcrColDateRange].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[RowTotal_current, sumdrcrColDateRange].CellStyle.Font.Bold = true;
                sheet.Range[RowTotal_current, sumdrcrColDateRange].BorderAround(ExcelLineStyle.Hair);
            }

        }
        private void TotalRevenue_DateRangeNew(ref IWorksheet sheet, ReportUtility oRU, DataTable dtParallelCurrency, int sumdrcrColDateRange, int RowTotal_current, int Row_Total_Start, int Row_total_End)
        {
            for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
            {

                sheet.Range[RowTotal_current, sumdrcrColDateRange + s].Formula = "=SUM(" + oRU.GetColumnNameForXls(sumdrcrColDateRange + s) + Row_Total_Start + ":" + oRU.GetColumnNameForXls(sumdrcrColDateRange + s) + Row_total_End + ")";
                sheet.Range[RowTotal_current, sumdrcrColDateRange + s].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[RowTotal_current, sumdrcrColDateRange + s].CellStyle.Font.Bold = true;
                sheet.Range[RowTotal_current, sumdrcrColDateRange + s].BorderAround(ExcelLineStyle.Hair);
            }

        }

        private void TotalExpense_DateRange(ref IWorksheet sheet, ReportUtility oRU, DataTable dtParallelCurrency, int sumdrcrCol2, int RowTotal_current2, int Row_Total_Start2, int Row_Total_End2)
        {
            for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
            {
                sumdrcrCol2++;
                sheet.Range[RowTotal_current2, sumdrcrCol2].Formula = "=SUM(" + oRU.GetColumnNameForXls(sumdrcrCol2) + Row_Total_Start2 + ":" + oRU.GetColumnNameForXls(sumdrcrCol2) + Row_Total_End2 + ")";
                sheet.Range[RowTotal_current2, sumdrcrCol2].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[RowTotal_current2, sumdrcrCol2].CellStyle.Font.Bold = true;
                sheet.Range[RowTotal_current2, sumdrcrCol2].BorderAround(ExcelLineStyle.Hair);
            }
        }
        private void TotalExpense_DateRangeNew(ref IWorksheet sheet, ReportUtility oRU, DataTable dtParallelCurrency, int sumdrcrCol2, int RowTotal_current2, int Row_Total_Start2, int Row_Total_End2)
        {
            for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
            {

                sheet.Range[RowTotal_current2, sumdrcrCol2].Formula = "=SUM(" + oRU.GetColumnNameForXls(sumdrcrCol2) + Row_Total_Start2 + ":" + oRU.GetColumnNameForXls(sumdrcrCol2) + Row_Total_End2 + ")";
                sheet.Range[RowTotal_current2, sumdrcrCol2].NumberFormat = oRU.NumberFormatDecimalTwo();
                sheet.Range[RowTotal_current2, sumdrcrCol2].CellStyle.Font.Bold = true;
                sheet.Range[RowTotal_current2, sumdrcrCol2].BorderAround(ExcelLineStyle.Hair);
            }
        }
        private DataSet GetIncomeStatementInfoDateRange(string companyId, string plantId, string fromDate, string toDate, string[] parallelCurrencies, bool isBudgetLevel, bool isActivityLevel)
        {
            GridParameter parameters = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var parallelCurrency = "";
                parallelCurrency = parallelCurrencies.Length > 0 ? string.Join(",", parallelCurrencies.Select(item => "'" + item + "'")) : "' '";
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                if (isActivityLevel)
                {
                    parameters.CmdText = @"

                    SELECT GL.Id AS AccountCodeId,--Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
                    sum(VDC.DrAmount) as DrAmount,
                    sum(VDC.CrAmount) as CrAmount,
                    sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
                    sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                   
                    ACT.BalanceType,
                    ACT.Id AS [MainHead],
                    AG.UserName AS [Level],
                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                    VD.BudgetMasterId, BUD.UserName AS Budget,
					A.UserName AS Activity,
                    A.Id AS ActivityId
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                    LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                    where act.IsBalanceSheet=0 AND v.PostingDate < '" + fromDate + @"' AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                    and VDC.ParallelCurrencyId IN (" + parallelCurrency + @") and v.IsPark=0
                    AND VDC.VoucherDetailId NOT IN ( SELECT VD.Id FROM  TRN.VoucherDetail AS VD  
																INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
																LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
																LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
																LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
																WHERE ACT.Id IN('Revenue','Expense') AND V.FiscalYearId in(select FiscalYearId from [SCS].[FiscalYearClose] ))
                    group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName, GL.AccountCode
                  --  ,v.PostingDate
					,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName,A.UserName,A.Id
UNION ALL
					SELECT GL.Id AS AccountCodeId,--Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
                    0 DrAmount,
                    0 CrAmount,
                    0 DRcumulative,
                    0 CRcumulative,
                    ACT.BalanceType,
                    ACT.Id AS [MainHead],
                    AG.UserName AS [Level],
                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                    VD.BudgetMasterId, BUD.UserName AS Budget,
					A.UserName AS Activity,
                    A.Id AS ActivityId
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                    LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                    where act.IsBalanceSheet=0 AND v.PostingDate between '" + fromDate + @"' and '" + toDate + @"' 
                     AND A.Id NOT IN (SELECT VDA.ActivityId
                                       FROM TRN.Voucher VA INNER JOIN TRN.VoucherDetail VDA ON VA.ID=VDA.VoucherId WHERE VA.PostingDate < '" + fromDate + @"')
                    AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"'
                    and VDC.ParallelCurrencyId IN (" + parallelCurrency + @") and v.IsPark=0
                    group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName
					,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName,A.UserName,A.Id";

                    return _sqlRepository.GetGridData(parameters).Source;
                }
                else if (isBudgetLevel && !isActivityLevel)
                {
                    parameters.CmdText = @"

                    SELECT GL.Id AS AccountCodeId,--Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
                    sum(VDC.DrAmount) as DrAmount,
                    sum(VDC.CrAmount) as CrAmount,
                    sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
                    sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                   
                    ACT.BalanceType,
                    ACT.Id AS [MainHead],
                    AG.UserName AS [Level],
                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                    VD.BudgetMasterId, BUD.UserName AS Budget
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                    LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                    where act.IsBalanceSheet=0 AND v.PostingDate < '" + fromDate + @"' AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                    and VDC.ParallelCurrencyId IN (" + parallelCurrency + @") and v.IsPark=0
                    AND VDC.VoucherDetailId NOT IN ( SELECT VD.Id FROM  TRN.VoucherDetail AS VD  
																INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
																LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
																LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
																LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
																WHERE ACT.Id IN('Revenue','Expense') AND V.FiscalYearId in(select FiscalYearId from [SCS].[FiscalYearClose] ))
                    group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName, GL.AccountCode
                  --  ,v.PostingDate
					,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName

UNION ALL
					
					 SELECT GL.Id AS AccountCodeId,--Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
                    0 DrAmount,
                    0 CrAmount,
                    0 DRcumulative,
                    0 CRcumulative,
                   
                    ACT.BalanceType,
                    ACT.Id AS [MainHead],
                    AG.UserName AS [Level],
                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                    VD.BudgetMasterId, BUD.UserName AS Budget
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                    LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                    where act.IsBalanceSheet=0 AND  v.PostingDate between '" + fromDate + @"' and '" + toDate + @"' 
                     AND A.Id NOT IN (SELECT VDA.ActivityId
                                       FROM TRN.Voucher VA INNER JOIN TRN.VoucherDetail VDA ON VA.ID=VDA.VoucherId WHERE VA.PostingDate < '" + fromDate + @"')
                    AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"'
                    and VDC.ParallelCurrencyId IN (" + parallelCurrency + @") and v.IsPark=0
                    group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName, GL.AccountCode
                  --  ,v.PostingDate
					,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName";

                    return _sqlRepository.GetGridData(parameters).Source;

                }
                else
                {
                    parameters.CmdText = @"

                    SELECT GL.Id AS AccountCodeId,--Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
                    sum(VDC.DrAmount) as DrAmount,
                    sum(VDC.CrAmount) as CrAmount,
                    sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
                    sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                   
                    ACT.BalanceType,
                    ACT.Id AS [MainHead],
                    AG.UserName AS [Level],
                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                    LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                    where act.IsBalanceSheet=0 AND v.PostingDate < '" + fromDate + @"' AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                    and VDC.ParallelCurrencyId IN (" + parallelCurrency + @") and v.IsPark=0
                    AND VDC.VoucherDetailId NOT IN ( SELECT VD.Id FROM  TRN.VoucherDetail AS VD  
																INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
																LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
																LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
																LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
																WHERE ACT.Id IN('Revenue','Expense') AND V.FiscalYearId in(select FiscalYearId from [SCS].[FiscalYearClose] ))
                    group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName, GL.AccountCode
                  --  ,v.PostingDate
					,ACT.BalanceType,AG.UserName,ACT.Id


	UNION ALL
					
				    SELECT GL.Id AS AccountCodeId,--Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
                    0 DrAmount,
                    0 CrAmount,
                    0 DRcumulative,
                    0 CRcumulative,
                    
                    ACT.BalanceType,
                    ACT.Id AS [MainHead],
                    AG.UserName AS [Level],
                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                    LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                    where act.IsBalanceSheet=0 AND v.PostingDate between '" + fromDate + @"' and '" + toDate + @"' 
                     AND A.Id NOT IN (SELECT VDA.ActivityId
                                       FROM TRN.Voucher VA INNER JOIN TRN.VoucherDetail VDA ON VA.ID=VDA.VoucherId WHERE VA.PostingDate < '" + fromDate + @"')
                     AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"'
                    and VDC.ParallelCurrencyId IN (" + parallelCurrency + @") and v.IsPark=0
                    group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName, GL.AccountCode
                  --  ,v.PostingDate
					,ACT.BalanceType,AG.UserName,ACT.Id";

                    return _sqlRepository.GetGridData(parameters).Source;

                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetEntityWiseExpenseAndEarningInfoDateRange(string companyId, string plantId, string fromDate, string toDate, string entityId, string[] parallelCurrencies)
        {
            GridParameter parameters = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var parallelCurrency = "";
                parallelCurrency = parallelCurrencies.Length > 0 ? string.Join(",", parallelCurrencies.Select(item => "'" + item + "'")) : "' '";
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"

                    SELECT GL.Id AS AccountCodeId,--Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
                    sum(VDC.DrAmount) as DrAmount,
                    sum(VDC.CrAmount) as CrAmount,
                    sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
                    sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                   
                    ACT.BalanceType,
                    ACT.Id AS [MainHead],
                    AG.UserName AS [Level],
                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                    VD.BudgetMasterId, BUD.UserName AS Budget
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                    LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                    where act.IsBalanceSheet=0 AND v.PostingDate < '" + fromDate + @"' AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"'
                            and V.EntityId='" + entityId + @"'
                    and VDC.ParallelCurrencyId IN (" + parallelCurrency + @") and v.IsPark=0
                    group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName, GL.AccountCode
                  --  ,v.PostingDate
					,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName";

                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }


        private DataTable GetIncomeStatementInfoDateRangeForThePeriod(string companyId, string fromDate, string toDate, string[] parallelCurrencies, bool isBudgetLevel, bool isActivityLevel)
        {
            string strSql = "";
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var parallelCurrency = "";
                parallelCurrency = parallelCurrencies.Length > 0 ? string.Join(",", parallelCurrencies.Select(item => "'" + item + "'")) : "' '";
                //parameters = new GridParameter
                //{
                //    ExportType = "DATASET"
                //};
                if (isBudgetLevel)
                {
                    strSql = @"                   
                    SELECT GL.Id AS AccountCodeId,--Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
                    
                    sum(VDC.DrAmount) as DrAmount,
                    sum(VDC.CrAmount) as CrAmount,
                    sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
                    sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                    ACT.BalanceType,
                    ACT.Id AS [MainHead],
                    AG.UserName AS [Level],
                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                    VD.BudgetMasterId, BUD.UserName AS Budget
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                    LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                    where act.IsBalanceSheet=0 AND v.PostingDate between '" + fromDate + @"' and '" + toDate + @"' AND V.CompanyId='" + companyId + @"'
                    and VDC.ParallelCurrencyId IN (" + parallelCurrency + @") and v.IsPark=0 and  vd.OpeningBalanceDetailId IS NULL
                    group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName, GL.AccountCode--,v.PostingDate
					,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName ";

                    return _sqlRepository.GetDataTable(strSql);
                }
                else if (isActivityLevel && !isBudgetLevel)
                {
                    strSql = @"                
                    SELECT GL.Id AS AccountCodeId,--Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
                    
                    sum(VDC.DrAmount) as DrAmount,
                    sum(VDC.CrAmount) as CrAmount,
                    sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
                    sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, A.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                    ACT.BalanceType,
                    ACT.Id AS [MainHead],
                    AG.UserName AS [Level],
                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                    VD.BudgetMasterId, BUD.UserName AS Budget,A.UserName AS Activity,A.Id AS ActivityId
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                    LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                    where act.IsBalanceSheet=0 AND v.PostingDate between '" + fromDate + @"' and '" + toDate + @"' AND V.CompanyId='" + companyId + @"'
                    and VDC.ParallelCurrencyId IN (" + parallelCurrency + @") and v.IsPark=0 and  vd.OpeningBalanceDetailId IS NULL
                    group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName, GL.AccountCode--,v.PostingDate
					,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName,A.UserName,A.Id ";

                    return _sqlRepository.GetDataTable(strSql);
                }
                else
                {
                    strSql = @"                   
                    SELECT GL.Id AS AccountCodeId,--Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
                    
                    sum(VDC.DrAmount) as DrAmount,
                    sum(VDC.CrAmount) as CrAmount,
                    sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
                    sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                    ACT.BalanceType,
                    ACT.Id AS [MainHead],
                    AG.UserName AS [Level],
                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                    LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                    where act.IsBalanceSheet=0 AND v.PostingDate between '" + fromDate + @"' and '" + toDate + @"' AND V.CompanyId='" + companyId + @"'
                    and VDC.ParallelCurrencyId IN (" + parallelCurrency + @") and v.IsPark=0 and  vd.OpeningBalanceDetailId IS NULL
                    group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName, GL.AccountCode--,v.PostingDate
					,ACT.BalanceType,AG.UserName,ACT.Id ";

                    return _sqlRepository.GetDataTable(strSql);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetEntityWiseExpenseAndEarningInfoDateRangeForThePeriod(string companyId, string fromDate, string toDate, string entityId, string[] parallelCurrencies)
        {
            string strSql = "";
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var parallelCurrency = "";
                parallelCurrency = parallelCurrencies.Length > 0 ? string.Join(",", parallelCurrencies.Select(item => "'" + item + "'")) : "' '";
                //parameters = new GridParameter
                //{
                //    ExportType = "DATASET"
                //};
                strSql = @"                   
                    SELECT GL.Id AS AccountCodeId,--Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
                    
                    sum(VDC.DrAmount) as DrAmount,
                    sum(VDC.CrAmount) as CrAmount,
                    sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
                    sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                    ACT.BalanceType,
                    ACT.Id AS [MainHead],
                    AG.UserName AS [Level],
                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                    VD.BudgetMasterId, BUD.UserName AS Budget
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                    LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                    where act.IsBalanceSheet=0 AND v.PostingDate between '" + fromDate + @"' and '" + toDate + @"' AND V.CompanyId='" + companyId + @"'
                            and V.EntityId='" + entityId + @"'
                    and VDC.ParallelCurrencyId IN (" + parallelCurrency + @") and v.IsPark=0 and  vd.OpeningBalanceDetailId IS NULL
                    group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName, GL.AccountCode--,v.PostingDate
					,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName ";

                return _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetEntityWiseExpenseAndEarningInfoDateRangeForThePeriod_ActivityLevel(string companyId, string fromDate, string toDate, string entityId, string[] parallelCurrencies)
        {
            string strSql = "";
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var parallelCurrency = "";
                parallelCurrency = parallelCurrencies.Length > 0 ? string.Join(",", parallelCurrencies.Select(item => "'" + item + "'")) : "' '";
                //parameters = new GridParameter
                //{
                //    ExportType = "DATASET"
                //};
                strSql = @"                   
                    DECLARE @fromDate varchar(50)='" + fromDate + @"',@toDate varchar(50)='" + toDate + @"', @entityId varchar(10)='" + entityId + @"', @parallelCurrencyId varchar(10)=" + parallelCurrency + @", @companyGroupId varchar(10)='" + identity.CompanyGroupId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + identity.PlantId + @"'
				
				   SELECT GL.Id AS AccountCodeId,VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,sum(VDC.DrAmount) as DrAmount,sum(VDC.CrAmount) as CrAmount,
                    sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VD.ActivityId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
                    sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VD.ActivityId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                    ACT.BalanceType,ACT.Id AS [MainHead],AG.UserName AS [Level],VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                    VD.BudgetMasterId, BC.UserName AS BudgetCategory,BSC.UserName AS BudgetSubCategory, BUD.UserName AS Budget,VD.ActivityId, A.UserName AS Activity,BMA.Id ControlId
					,(ISNULL((SELECT CASE WHEN DRcumulative=0 THEN CRcumulative ELSE DRcumulative END ForTheDay FROM
										(SELECT VD.ActivityId,  sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VD.ActivityId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
										sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VD.ActivityId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
										FROM TRN.VoucherDetailCurrency AS VDC
										INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
										INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
										LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
										LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
										left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
										WHERE v.PostingDate = @toDate  
										AND act.IsBalanceSheet=0  AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId
										and V.EntityId=@entityId  and VDC.ParallelCurrencyId IN (@parallelCurrencyId) and v.IsPark=0 and  VD.OpeningBalanceDetailId IS NULL
										GROUP BY ACT.BalanceType,GL.Id, VD.BudgetMasterId, VD.ActivityId, VDC.ParallelCurrencyId )T
										WHERE ActivityId=VD.ActivityId),0))ForTheDay
						,(ISNULL((SELECT  top(1) CASE WHEN DRcumulative=0 THEN CRcumulative ELSE DRcumulative END ForTheFiscalYear FROM
										(SELECT VDFY.ActivityId,  sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VDFY.BudgetMasterId, VDFY.ActivityId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
										sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDFY.BudgetMasterId, VDFY.ActivityId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
										FROM TRN.VoucherDetailCurrency AS VDC
										INNER JOIN TRN.VoucherDetail AS VDFY ON VDFY.Id =VDC.VoucherDetailId
										INNER JOIN TRN.Voucher AS V ON V.Id=VDFY.VoucherId
										LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VDFY.GLGeneralInfoId
										LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
										left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
										WHERE V.FiscalYearId IN (SELECT Id FROM [SCS].[FiscalYear] WHERE  @toDate between StartDate and EndDate) 
										AND VDFY.ActivityId=VD.ActivityId
										AND act.IsBalanceSheet=0  AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId
										and V.EntityId=@entityId  and VDC.ParallelCurrencyId IN (@parallelCurrencyId) and v.IsPark=0 and  VDFY.OpeningBalanceDetailId IS NULL
										GROUP BY ACT.BalanceType,GL.Id, VDFY.BudgetMasterId, VDFY.ActivityId, VDC.ParallelCurrencyId )T
										),0))ForTheFiscalYear
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
					LEFT JOIN [HKP].[BudgetSubCategory] AS BSC ON BSC.Id=BM.BudgetSubCategoryId
                    LEFT JOIN [HKP].[BudgetCategory] AS BC ON BC.Id=BM.BudgetCategoryId
                    LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                    LEFT JOIN ORG.Entity E on V.EntityId=E.Id
                    LEFT JOIN [MST].[BudgetMasterActivity] AS BMA  on BMA.BudgetMasterId=BM.Id AND BMA.ActivityId=A.Id
                    WHERE act.IsBalanceSheet=0 AND v.PostingDate between @fromDate and @toDate 
					AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId
                    AND V.EntityId=@entityId AND V.SourceType NOT IN ('AdvanceJournalVoucher','JournalVoucher')
					AND VDC.ParallelCurrencyId IN (@parallelCurrencyId) and v.IsPark=0 and  vd.OpeningBalanceDetailId IS NULL
                    group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName, GL.AccountCode
					,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId, BC.UserName,BSC.UserName,BUD.UserName ,VD.ActivityId, A.UserName,BMA.Id 

					UNION ALL
					 SELECT GL.Id AS AccountCodeId,VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,sum(VDC.DrAmount) as DrAmount,sum(VDC.CrAmount) as CrAmount,
                    sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VD.ActivityId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
                    sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VD.ActivityId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative,
                    ACT.BalanceType,ACT.Id AS [MainHead],AG.UserName AS [Level],VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                    VD.BudgetMasterId, BC.UserName AS BudgetCategory,BSC.UserName AS BudgetSubCategory, BUD.UserName AS Budget,VD.ActivityId, A.UserName AS Activity,BMA.Id ControlId
					,(ISNULL((SELECT CASE WHEN DRcumulative=0 THEN CRcumulative ELSE DRcumulative END ForTheDay FROM
										(SELECT VD.ActivityId,  sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VD.ActivityId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
										sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, VD.ActivityId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
										FROM TRN.VoucherDetailCurrency AS VDC
										INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
										INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
										LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
										LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
										left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
										WHERE v.PostingDate = @toDate  
										AND act.IsBalanceSheet=0  AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId
										and V.EntityId=@entityId  and VDC.ParallelCurrencyId IN (@parallelCurrencyId) and v.IsPark=0 and  VD.OpeningBalanceDetailId IS NULL
										GROUP BY ACT.BalanceType,GL.Id, VD.BudgetMasterId, VD.ActivityId, VDC.ParallelCurrencyId )T
										WHERE ActivityId=VD.ActivityId),0))ForTheDay
						,(ISNULL((SELECT  top(1)  CASE WHEN DRcumulative=0 THEN CRcumulative ELSE DRcumulative END ForTheFiscalYear FROM
										(SELECT VDFY.ActivityId,  sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VDFY.BudgetMasterId, VDFY.ActivityId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative,
										sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VDFY.BudgetMasterId, VDFY.ActivityId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative
										FROM TRN.VoucherDetailCurrency AS VDC
										INNER JOIN TRN.VoucherDetail AS VDFY ON VDFY.Id =VDC.VoucherDetailId
										INNER JOIN TRN.Voucher AS V ON V.Id=VDFY.VoucherId
										LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VDFY.GLGeneralInfoId
										LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
										left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
										WHERE V.FiscalYearId IN (SELECT Id FROM [SCS].[FiscalYear] WHERE  @toDate between StartDate and EndDate) 
										AND VDFY.ActivityId=VD.ActivityId
										AND act.IsBalanceSheet=0  AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId
										and V.EntityId=@entityId  and VDC.ParallelCurrencyId IN (@parallelCurrencyId) and v.IsPark=0 and  VDFY.OpeningBalanceDetailId IS NULL
										GROUP BY ACT.BalanceType,GL.Id, VDFY.BudgetMasterId, VDFY.ActivityId, VDC.ParallelCurrencyId )T
										),0))ForTheFiscalYear
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
					LEFT JOIN [HKP].[BudgetSubCategory] AS BSC ON BSC.Id=BM.BudgetSubCategoryId
                    LEFT JOIN [HKP].[BudgetCategory] AS BC ON BC.Id=BM.BudgetCategoryId
                    LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                    LEFT JOIN ORG.Entity EN on VD.EntityId=EN.Id
                    LEFT JOIN [MST].[BudgetMasterActivity] AS BMA  on BMA.BudgetMasterId=BM.Id AND BMA.ActivityId=A.Id
                    WHERE act.IsBalanceSheet=0 AND v.PostingDate between @fromDate and @toDate 
					AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId
                    AND VD.EntityId=@entityId AND V.SourceType  IN ('AdvanceJournalVoucher','JournalVoucher')
					AND VDC.ParallelCurrencyId IN (@parallelCurrencyId) and v.IsPark=0 and  vd.OpeningBalanceDetailId IS NULL
                    group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName, GL.AccountCode
					,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId, BC.UserName,BSC.UserName,BUD.UserName ,VD.ActivityId, A.UserName,BMA.Id  ";
                return _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception)
            {
                throw;
            }
        }


        //Master query for income statement
        private DataTable GetIncomeStatementInfoDateRangeForThePeriodMaster(string companyId, string fromDate, string toDate, string[] parallelCurrencies)
        {
            string strSql = "";
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var parallelCurrency = "";
                parallelCurrency = parallelCurrencies.Length > 0 ? string.Join(",", parallelCurrencies.Select(item => "'" + item + "'")) : "' '";
                //parameters = new GridParameter
                //{
                //    ExportType = "DATASET"
                //};
                strSql = @"                   
                    SELECT GL.Id AS AccountCodeId,
                    VDC.ParallelCurrencyId,CU.Code AS CurrencyCode,
                    ACT.BalanceType,
                    ACT.Id AS [MainHead],
                    AG.UserName AS [Level],
                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                    VD.BudgetMasterId, BUD.UserName AS Budget
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT OUTER JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                    left outer join [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                    LEFT OUTER JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BM.BudgetId
                    LEFT JOIN HKP.Activity A on VD.ActivityId=A.Id
                    where act.IsBalanceSheet=0 AND v.PostingDate <= '" + toDate + @"' AND V.CompanyId='" + companyId + @"'
                    and VDC.ParallelCurrencyId IN (" + parallelCurrency + @") and v.IsPark=0
                    group by GL.Id, GL.AccountCode, VDC.ParallelCurrencyId,CU.Code,vd.GLGeneralInfoId,GL.UserName, GL.AccountCode
					,ACT.BalanceType,AG.UserName,ACT.Id, VD.BudgetMasterId,BUD.UserName";

                return _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception)
            {
                throw;
            }
        }


        private DataSet GetMaterialGroupMaster()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT MGM.Id AS GroupMasterId, MG1.Id AS Group1Id, MG2.Id AS Group2Id, MG3.Id AS Group3Id, MG4.Id AS Group4Id
								, MG1.UserName AS Group1, MG2.UserName AS Group2, MG3.UserName AS Group3, MG4.UserName AS Group4
	                            , MGM.Code, MGM.UserName AS GroupMaster, H.Description AS HSNCode, MT.Description AS MaterialType, U.UserName AS BaseUoM
                                FROM MST.[MaterialGroupMaster] AS MGM
                                LEFT JOIN HKP.MaterialType AS MT ON MT.Id=MGM.MaterialTypeId
                                LEFT JOIN HKP.MaterialGroup1 AS MG1 ON MG1.Id=MGM.MaterialGroup1Id
                                LEFT JOIN HKP.MaterialGroup2 AS MG2 ON MG2.Id=MGM.MaterialGroup2Id
                                LEFT JOIN HKP.MaterialGroup3 AS MG3 ON MG3.Id=MGM.MaterialGroup3Id
                                LEFT JOIN HKP.MaterialGroup4 AS MG4 ON MG4.Id=MGM.MaterialGroup4Id
                                LEFT JOIN HKP.HSNCode AS H ON H.Id=MGM.HSNCodeId
                                LEFT JOIN SCS.UnitOfMeasurement AS U ON U.Id=MGM.BaseUoMId
                                LEFT JOIN HKP.CompanyGroupWiseMaterialGroupMaster AS CM ON CM.MaterialGroupMasterId=MGM.Id
                                WHERE CM.CompanyGroupId='" + identity.CompanyGroupId + "' AND MGM.Active=1 AND MGM.Archive=0";
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook MaterialGroupMaster_Report(ExcelEngine excelEngine)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            IWorksheet sheet2 = null;
            try
            {
                oRU = new ReportUtility();

                workbook = oRU.GetWorkbook(ref excelEngine, 2);
                sheet1 = workbook.Worksheets[0];
                sheet2 = workbook.Worksheets[1];
                CreateSheetM1(ref sheet1, oRU, "Material Group Master Report", "Material Group Master Report");
                CreateSheetM2(ref sheet2, oRU, "Material Group Master List", "Material Group Master Data");

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                #region final

                //excelEngine = null;
                //application = null;
                //workbook = null;
                //sheet = null;
                //sheet2 = null;

                #endregion final
            }
        }

        private void CreateSheetM1(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName)
        {
            try
            {
                DataTable dtMaterialGroupMaster = null;

                #region List data

                DataSet MaterialGroupMasterList = GetMaterialGroupMaster();
                dtMaterialGroupMaster = MaterialGroupMasterList.Tables[0];

                DataView dvGroup1 = new DataView(MaterialGroupMasterList.Tables[0]);
                DataTable dtGroup1 = dvGroup1.ToTable(true, "Group1", "Group1Id");
                dvGroup1.Sort = "Group1";

                DataView dvGroup2 = null;
                DataTable dtGroup2 = null;

                DataView dvGroup3 = null;
                DataTable dtGroup3 = null;

                DataView dvGroupMaster = null;
                DataTable dtGroupMaster = null;

                if (dtMaterialGroupMaster.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;
                var group1ColIndex = 1;
                var group2ColIndex = 2;
                var group3ColIndex = 3;
                var group4ColIndex = 4;
                var codeColIndex = 5;
                var groupMasterColIndex = 6;
                var hsnCodeColIndex = 7;
                var materialTypeColIndex = 8;
                var baseUoMColIndex = 9;

                for (int i = 0; i < dtMaterialGroupMaster.Columns.Count; i++)
                {
                    if (dtMaterialGroupMaster.Columns[i].ColumnName != "TotalRows" && dtMaterialGroupMaster.Columns[i].ColumnName != "GroupMasterId" && dtMaterialGroupMaster.Columns[i].ColumnName != "Group1Id" && dtMaterialGroupMaster.Columns[i].ColumnName != "Group2Id" && dtMaterialGroupMaster.Columns[i].ColumnName != "Group3Id" && dtMaterialGroupMaster.Columns[i].ColumnName != "Group4Id")
                    {
                        _colIndex++;
                        oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtMaterialGroupMaster.Columns[i].ColumnName);
                    }
                }
                shet2EndxlsCol = _colIndex;

                for (int m = 0; m < dtGroup1.Rows.Count; m++)
                {
                    _rowL++;
                    string group1Id = dtGroup1.Rows[m]["Group1Id"].ToString();
                    dvGroup2 = new DataView(dtMaterialGroupMaster);
                    dvGroup1.Sort = "Group2";
                    dvGroup2.RowFilter = "Group1Id='" + group1Id + "'";
                    dtGroup2 = dvGroup2.ToTable(true, "Group2", "Group2Id");
                    var rowStartGroup1 = _rowL;
                    oRU.SetText(ref sheet, _rowL, group1ColIndex, dtGroup1.Rows[m]["Group1"].ToString(), 26);

                    for (int n = 0; n < dtGroup2.Rows.Count; n++)
                    {
                        string group2Id = dtGroup2.Rows[n]["Group2Id"].ToString();
                        dvGroup3 = new DataView(dtMaterialGroupMaster);
                        dvGroup1.Sort = "Group3";
                        dvGroup3.RowFilter = "Group2Id='" + group2Id + "' and Group1Id='" + group1Id + "'";
                        dtGroup3 = dvGroup3.ToTable(true, "Group3", "Group3Id");
                        var rowStartGroup2 = _rowL;
                        oRU.SetText(ref sheet, _rowL, group2ColIndex, dtGroup2.Rows[n]["Group2"].ToString(), 26);

                        for (int o = 0; o < dtGroup3.Rows.Count; o++)
                        {
                            string group3Id = dtGroup3.Rows[o]["Group3Id"].ToString();
                            dvGroupMaster = new DataView(dtMaterialGroupMaster);
                            dvGroup1.Sort = "GroupMaster";
                            dvGroupMaster.RowFilter = "Group3Id='" + group3Id + "' and Group2Id='" + group2Id + "' and Group1Id='" + group1Id + "'";
                            dtGroupMaster = dvGroupMaster.ToTable(true, "GroupMaster", "GroupMasterId", "Group4", "Code", "HSNCode", "MaterialType", "BaseUoM");
                            var rowStartGroup3 = _rowL;
                            oRU.SetText(ref sheet, _rowL, group3ColIndex, dtGroup3.Rows[o]["Group3"].ToString(), 26);

                            for (int q = 0; q < dtGroupMaster.Rows.Count; q++)
                            {
                                oRU.SetText(ref sheet, _rowL, group4ColIndex, dtGroupMaster.Rows[q]["Group4"].ToString(), 26);
                                oRU.SetText(ref sheet, _rowL, codeColIndex, dtGroupMaster.Rows[q]["Code"].ToString(), 15);
                                oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtGroupMaster.Rows[q]["GroupMaster"].ToString(), 26);
                                oRU.SetText(ref sheet, _rowL, hsnCodeColIndex, dtGroupMaster.Rows[q]["HSNCode"].ToString(), 26);
                                oRU.SetText(ref sheet, _rowL, materialTypeColIndex, dtGroupMaster.Rows[q]["MaterialType"].ToString(), 26);
                                oRU.SetText(ref sheet, _rowL, baseUoMColIndex, dtGroupMaster.Rows[q]["BaseUoM"].ToString(), 26);
                                _rowL++;

                                group4ColIndex = 4;
                                codeColIndex = 5;
                                groupMasterColIndex = 6;
                                hsnCodeColIndex = 7;
                                materialTypeColIndex = 8;
                                baseUoMColIndex = 9;
                            }

                            if (dtGroup3.Rows.Count > 0)
                            {
                                sheet[rowStartGroup3, group3ColIndex, _rowL - 1, group3ColIndex].Merge();
                            }
                        }//Group3
                        if (dtGroup2.Rows.Count > 0)
                        {
                            sheet[rowStartGroup2, group2ColIndex, _rowL - 1, group2ColIndex].Merge();
                        }
                    }//Group2
                    if (dtGroup1.Rows.Count > 0)
                    {
                        sheet[rowStartGroup1, group1ColIndex, _rowL - 1, group1ColIndex].Merge();
                        _rowL--;
                    }
                }//Group1

                sheet.Range[5, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Material Group Master Report", identity.CompanyGroupId);
                oRU.FreezePage(ref sheet, 1, 6);
                oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheetM2(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName)
        {
            DataTable dtMaterialGroupMaster = null;

            #region List data

            DataSet MaterialGroupMasterList = GetMaterialGroupMaster();
            dtMaterialGroupMaster = MaterialGroupMasterList.Tables[0];
            if (dtMaterialGroupMaster.Rows.Count == 0)
            {
                throw new Exception("No Data Found !!!");
            }

            #endregion List data

            var _col = 1;
            var _rowL = 5;
            var _colIndex = 0;
            var shet2EndxlsCol = _col;

            for (int i = 0; i < dtMaterialGroupMaster.Columns.Count; i++)
            {
                if (dtMaterialGroupMaster.Columns[i].ColumnName != "TotalRows" && dtMaterialGroupMaster.Columns[i].ColumnName != "GroupMasterId" && dtMaterialGroupMaster.Columns[i].ColumnName != "Group1Id" && dtMaterialGroupMaster.Columns[i].ColumnName != "Group2Id" && dtMaterialGroupMaster.Columns[i].ColumnName != "Group3Id" && dtMaterialGroupMaster.Columns[i].ColumnName != "Group4Id")
                {
                    _colIndex++;
                    oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtMaterialGroupMaster.Columns[i].ColumnName);
                }
            }
            shet2EndxlsCol = _colIndex;

            for (int i = 0; i < dtMaterialGroupMaster.Rows.Count; i++)
            {
                _rowL++;

                oRU.SetText(ref sheet, _rowL, 1, dtMaterialGroupMaster.Rows[i]["Group1"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 2, dtMaterialGroupMaster.Rows[i]["Group2"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 3, dtMaterialGroupMaster.Rows[i]["Group3"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 4, dtMaterialGroupMaster.Rows[i]["Group4"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 5, dtMaterialGroupMaster.Rows[i]["Code"].ToString(), 15);
                oRU.SetText(ref sheet, _rowL, 6, dtMaterialGroupMaster.Rows[i]["GroupMaster"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 7, dtMaterialGroupMaster.Rows[i]["HSNCode"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 8, dtMaterialGroupMaster.Rows[i]["MaterialType"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 9, dtMaterialGroupMaster.Rows[i]["BaseUoM"].ToString(), 26);
            }

            sheet.Range[5, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Name = SheetName;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Material Group Master List", identity.CompanyGroupId);
            oRU.FreezePage(ref sheet, 1, 6);
            oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
        }

        public static int GetCurrencyColIndex(ArrayList al, string paraCar)
        {
            var result = 0;
            try
            {
                for (int i = 0; i < al.Count; i++)
                {
                    Dictionary<string, int> v = (Dictionary<string, int>)al[i];
                    if (v.ContainsKey(paraCar))
                    {
                        result = v[paraCar];
                        break;
                    }
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetMaterialMasterInfo(string materialTypeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT MM.Id, MM.Code
                                         ,MM.UserName AS [Material Name]
                                         ,MGP.UserName AS [Material Group]
                                         ,MG.[Description] AS Grid
										 ,COUNT(SM.MaterialMasterId) AS [No. of Submaterial]
                                         ,PM.UserName AS [Product Master]
	                                     ,HSN.Description AS [HSN Code]
                                         ,UOMB.UserName AS [Base UoM]
										 ,COUNT(AU.MaterialMasterId) AS [No. of Alternative UoM]
										 ,TS.UserName AS [Testing Standard]
										 ,MT.Description AS MaterialType
                                   FROM [MST].[MaterialMaster] AS MM
                                   LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
                                   LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                                   LEFT OUTER JOIN [HKP].[MaterialGrid] AS MG ON MM.MaterialGridId = MG.Id
                                   LEFT OUTER JOIN [MST].[ProductMaster] AS PM ON MM.ProductMasterId = PM.Id
								   LEFT OUTER JOIN [SCS].[TestingStandard] AS TS ON TS.Id = MM.TestingStandardId
                                   LEFT OUTER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
								   LEFT OUTER JOIN [HKP].[HSNCode] AS HSN ON HSN.id=MM.HSNCodeId
								   LEFT OUTER JOIN [MST].[SubMaterial] AS SM ON SM.MaterialMasterId=MM.Id
								   LEFT OUTER JOIN [MST].[MaterialMasterAlternativeUOM] AS AU ON AU.MaterialMasterId=MM.Id
								   WHERE MM.CompanyGroupId = '" + identity.CompanyGroupId + @"' AND MM.Archive = 0 AND MM.Active = 1 AND MM.MaterialTypeId='" + materialTypeId + @"'
								   GROUP BY MM.UserName, MGP.UserName,MG.[Description], SM.MaterialMasterId, PM.UserName, HSN.Description, UOMB.UserName, AU.MaterialMasterId, TS.UserName, MT.Description, MM.Id, MM.Code";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetMaterialMasterDetailsInfo(string materialTypeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT  MM.Id
                                        ,MM.Code
                                        ,MM.UserName AS [Material Name]
                                        ,MGP.UserName AS [Material Group]
                                        ,UOMB.UserName AS [Base UoM]
									    ,TS.UserName AS [Testing Standard]
									    ,SM.StandardName AS Submaterial
									    --,MA.UserName AS [Material Attribute]
									    --,MAV.Description AS [Material Attribute Value]
                                        ,SM.Id AS SubMaterialId
									    ,MT.Description AS MaterialType
									    --,MAMV.MaterialAttributeId
									    --,MAMV.SubMaterialId
                                   FROM [MST].[MaterialMaster] AS MM
                                   LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
                                   LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
								   LEFT OUTER JOIN [SCS].[TestingStandard] AS TS ON TS.Id = MM.TestingStandardId
                                   LEFT OUTER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
								   LEFT OUTER JOIN [MST].[SubMaterial] AS SM ON SM.MaterialMasterId = MM.Id
								   --LEFT OUTER JOIN [MST] .[MaterialMasterAttributeValue] AS MAMV ON MAMV.SubMaterialId = SM.Id
								   --LEFT OUTER JOIN [HKP].[MaterialAttribute] AS MA ON MA.Id = MAMV.MaterialAttributeId
								   --LEFT OUTER JOIN [HKP].[MaterialAttributeValue] AS MAV ON MAV.Id = MAMV.MaterialAttributeValueId
								   WHERE MM.CompanyGroupId = '" + identity.CompanyGroupId + @"' AND MM.Archive = 0 AND MM.Active = 1 AND MM.MaterialTypeId='" + materialTypeId + @"'";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetMaterialMasterDetailsInfo2(string materialTypeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT
                                     MGP.UserName AS [Material Group]
									,MA.UserName AS [Material Attribute]
									,[Material Attribute Value]=CASE WHEN MAV.Description IS NULL THEN MAMV.MaterialAttributeValueFreeText ELSE MAV.Description END
									,MAMV.MaterialAttributeId
									,MAMV.MaterialAttributeValueId
									,MAMV.SubMaterialId
                                   FROM [MST].[MaterialMaster] AS MM
                                   LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
                                   LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
								   LEFT OUTER JOIN [MST].[SubMaterial] AS SM ON SM.MaterialMasterId = MM.Id
								   LEFT OUTER JOIN [MST] .[MaterialMasterAttributeValue] AS MAMV ON MAMV.SubMaterialId = SM.Id
								   LEFT OUTER JOIN [HKP].[MaterialAttribute] AS MA ON MA.Id = MAMV.MaterialAttributeId
								   LEFT OUTER JOIN [HKP].[MaterialAttributeValue] AS MAV ON MAV.Id = MAMV.MaterialAttributeValueId
								   WHERE MM.CompanyGroupId = '" + identity.CompanyGroupId + @"' AND MM.Archive = 0 AND MM.Active = 1 AND MM.MaterialTypeId='" + materialTypeId + @"'";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private int GetMaxAttributeCounted(DataTable dtMain, DataTable dtAttribute)
        {
            var result = 0;
            DataView dv = null;
            try
            {
                for (int i = 0; i < dtMain.Rows.Count; i++)
                {
                    string MaterialAttributeId = dtMain.Rows[i]["SubMaterialId"].ToString();
                    dv = new DataView(dtAttribute)
                    {
                        RowFilter = "SubMaterialId='" + MaterialAttributeId + "'"
                    };
                    if (result < dv.Count)
                    {
                        result = dv.Count;
                    }
                    dv.RowFilter = null;
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetAttributeData(string groupMasterId, DataTable dtAttribute)
        {
            DataView dv = null;
            try
            {
                dv = new DataView(dtAttribute)
                {
                    RowFilter = "SubMaterialId='" + groupMasterId + "'"
                };
                return dv.ToTable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook MaterialMaster_Report(ExcelEngine excelEngine, string materialTypeId, bool withSubmaterial)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                DataSet dsLocal = GetMaterialMasterInfo(materialTypeId);
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];

                if (withSubmaterial)
                {
                    CreateSheet_MaterialMasterDetails(ref sheet1, oRU, "Material Master With Submaterial", "Material Master With Submaterial", dsLocal, materialTypeId);
                }
                else
                {
                    CreateSheet_MaterialMaster(ref sheet1, oRU, "Material Master", "Material Master", dsLocal, materialTypeId);
                }

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public IWorkbook MaterialMasterReport2(ExcelEngine excelEngine, string materialTypeId, bool withSubmaterial)
        //{
        //    ReportUtility oRU = null;
        //    IWorkbook workbook = null;
        //    IWorksheet sheet1 = null;
        //    try
        //    {
        //        oRU = new ReportUtility();
        //        DataSet dsLocal = GetMaterialMasterInfo(materialTypeId);
        //        workbook = oRU.GetWorkbook(ref excelEngine, 1);
        //        sheet1 = workbook.Worksheets[0];

        //        if (withSubmaterial)
        //        {
        //            CreateSheet_MaterialMasterDetails(ref sheet1, oRU, "Material Master With Submaterial", "Material Master With Submaterial", dsLocal, materialTypeId);
        //        }
        //        else
        //        {
        //            CreateSheet_MaterialMaster(ref sheet1, oRU, "Material Master", "Material Master", dsLocal, materialTypeId);
        //        }

        //        workbook.Version = ExcelVersion.Excel2013;
        //        return workbook;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}



        private void CreateSheet_MaterialMaster(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dslocal, string materialTypeId)
        {
            try
            {
                DataTable dtMaterialMaster = null;

                #region List data

                DataSet MaterialMasterList = GetMaterialMasterInfo(materialTypeId);
                DataView dvMainBody = new DataView(MaterialMasterList.Tables[0])
                {
                    Sort = "Material Name"
                };
                dtMaterialMaster = dvMainBody.ToTable(true, "Id", "Code", "Material Name", "Material Group", "Grid", "No. of Submaterial", "Product Master", "HSN Code", "Base UoM", "No. of Alternative UoM", "Testing Standard", "MaterialType");

                if (dtMaterialMaster.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;

                var _col3 = 3;

                oRU.SetMasterHeaderText(ref sheet, _rowL, _col, "Material Type");
                sheet[oRU.GetColumnNameForXls(_col) + _rowL + ":" + oRU.GetColumnNameForXls(_col + 1) + _rowL].Merge();
                oRU.SetText(ref sheet, _rowL, _col + 2, dtMaterialMaster.Rows[0]["MaterialType"].ToString()); _rowL++;
                sheet[oRU.GetColumnNameForXls(_col3) + _rowL + ":" + oRU.GetColumnNameForXls(_col3 + 2) + _rowL].Merge();

                _rowL = 6;
                _rowL++;

                for (int i = 0; i < dtMaterialMaster.Columns.Count; i++)
                {
                    if (dtMaterialMaster.Columns[i].ColumnName != "TotalRows" && dtMaterialMaster.Columns[i].ColumnName != "Id" && dtMaterialMaster.Columns[i].ColumnName != "MaterialType")
                    {
                        _colIndex++;
                        oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtMaterialMaster.Columns[i].ColumnName);
                    }
                }
                shet2EndxlsCol = _colIndex;

                for (int q = 0; q < dtMaterialMaster.Rows.Count; q++)
                {
                    _rowL++;
                    oRU.SetText(ref sheet, _rowL, 1, dtMaterialMaster.Rows[q]["Code"].ToString(), 15);
                    oRU.SetText(ref sheet, _rowL, 2, dtMaterialMaster.Rows[q]["Material Name"].ToString(), 26);
                    oRU.SetText(ref sheet, _rowL, 3, dtMaterialMaster.Rows[q]["Material Group"].ToString(), 26);
                    oRU.SetText(ref sheet, _rowL, 4, dtMaterialMaster.Rows[q]["Grid"].ToString(), 26);
                    oRU.SetText(ref sheet, _rowL, 5, dtMaterialMaster.Rows[q]["No. of Submaterial"].ToString(), 26);
                    oRU.SetText(ref sheet, _rowL, 6, dtMaterialMaster.Rows[q]["Product Master"].ToString(), 26);
                    oRU.SetText(ref sheet, _rowL, 7, dtMaterialMaster.Rows[q]["HSN Code"].ToString(), 26);
                    oRU.SetText(ref sheet, _rowL, 8, dtMaterialMaster.Rows[q]["Base UoM"].ToString(), 26);
                    oRU.SetText(ref sheet, _rowL, 9, dtMaterialMaster.Rows[q]["No. of Alternative UoM"].ToString(), 26);
                    oRU.SetText(ref sheet, _rowL, 10, dtMaterialMaster.Rows[q]["Testing Standard"].ToString(), 26);
                }

                sheet.Range[7, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Material Master", identity.CompanyGroupId);
                oRU.FreezePage(ref sheet, 1, 8);
                oRU.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_MaterialMasterDetails(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dslocal, string materialTypeId)
        {
            try
            {
                DataTable dtMaterialMaster = null;

                #region List data

                DataSet MaterialMasterList = GetMaterialMasterDetailsInfo(materialTypeId);
                DataView dvMainBody = new DataView(MaterialMasterList.Tables[0])
                {
                    Sort = "Material Group"
                };
                dtMaterialMaster = dvMainBody.ToTable(true, "Id", "Code", "Material Name", "Material Group", "Base UoM", "Testing Standard", "Submaterial", "SubMaterialId", "MaterialType");

                DataSet MaterialMasterAtrList = GetMaterialMasterDetailsInfo2(materialTypeId);
                DataView dvAttribute = new DataView(MaterialMasterAtrList.Tables[0])
                {
                    Sort = "Material Group"
                };
                DataTable dtAttribute = dvAttribute.ToTable(false, "Material Attribute", "MaterialAttributeId", "SubMaterialId", "Material Attribute Value", "MaterialAttributeValueId");

                var maxAttributeCol = GetMaxAttributeCounted(dtMaterialMaster, dtAttribute);

                if (dtMaterialMaster.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;
                var groupMasterColIndex = 1;

                var _col3 = 3;

                oRU.SetMasterHeaderText(ref sheet, _rowL, _col, "Material Type");
                sheet[oRU.GetColumnNameForXls(_col) + _rowL + ":" + oRU.GetColumnNameForXls(_col + 1) + _rowL].Merge();
                oRU.SetText(ref sheet, _rowL, _col + 2, dtMaterialMaster.Rows[0]["MaterialType"].ToString()); _rowL++;
                sheet[oRU.GetColumnNameForXls(_col3) + _rowL + ":" + oRU.GetColumnNameForXls(_col3 + 2) + _rowL].Merge();

                _rowL = 6;
                _rowL++;

                for (int i = 0; i < dtMaterialMaster.Columns.Count; i++)
                {
                    if (dtMaterialMaster.Columns[i].ColumnName != "TotalRows" && dtMaterialMaster.Columns[i].ColumnName != "Id" && dtMaterialMaster.Columns[i].ColumnName != "MaterialType" && dtMaterialMaster.Columns[i].ColumnName != "Material Attribute" && dtMaterialMaster.Columns[i].ColumnName != "SubMaterialId")
                    {
                        _colIndex++;
                        oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtMaterialMaster.Columns[i].ColumnName);
                    }
                }

                var attributeCount = 0;
                for (int i = 0; i < maxAttributeCol; i++)
                {
                    attributeCount++;
                    _colIndex++;
                    oRU.SetHeaderText(ref sheet, _rowL, _colIndex, "Attribute" + attributeCount);
                }

                shet2EndxlsCol = _colIndex;

                for (int q = 0; q < dtMaterialMaster.Rows.Count; q++)
                {
                    _rowL++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtMaterialMaster.Rows[q]["Code"].ToString(), 15); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtMaterialMaster.Rows[q]["Material Name"].ToString(), 26); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtMaterialMaster.Rows[q]["Material Group"].ToString(), 26); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtMaterialMaster.Rows[q]["Base UoM"].ToString(), 26); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtMaterialMaster.Rows[q]["Testing Standard"].ToString(), 26); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtMaterialMaster.Rows[q]["Submaterial"].ToString(), 26); groupMasterColIndex++;

                    string groupMasterId = dtMaterialMaster.Rows[q]["SubMaterialId"].ToString();

                    DataTable dtAttributeData = GetAttributeData(groupMasterId, dtAttribute);
                    var rowCount = _rowL;
                    for (int a = 0; a < dtAttributeData.Rows.Count; a++)
                    {
                        if (a == 0)
                            _rowL++;
                        oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtAttributeData.Rows[a]["Material Attribute Value"].ToString());
                        if (rowCount == _rowL) rowCount++;
                        oRU.SetText(ref sheet, rowCount, groupMasterColIndex, dtAttributeData.Rows[a]["Material Attribute"].ToString(), true); groupMasterColIndex++;
                    }
                    if (maxAttributeCol > dtAttributeData.Rows.Count)
                    {
                        for (int c = 0; c < maxAttributeCol - dtAttributeData.Rows.Count; c++)
                        {
                            oRU.SetText(ref sheet, _rowL, groupMasterColIndex, ""); groupMasterColIndex++;
                        }
                    }
                    groupMasterColIndex = 1;
                }

                sheet.Range[7, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Material Master With Submaterial", identity.CompanyGroupId);
                oRU.FreezePage(ref sheet, 1, 8);
                oRU.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region Testing Standard Report

        private DataSet GetTestingStandardInfo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT TS.Id
                                , TS.Code
                                , TS.UserName AS [Testing Standard]
                                , TS.Description
                                ,( select  COUNT(*) from SCS.TestingStandardDetail where TestingStandardId=TS.Id)  AS [No. of Testing]
                                ,( select  COUNT(*) from SCS.TestingStandardBuyer where TestingStandardId=TS.Id)  AS [No. of Buyer]
                                FROM [SCS].[TestingStandard] AS TS
                                WHERE TS.CompanyGroupId = '" + identity.CompanyGroupId + @"'
                                GROUP BY  TS.Id, TS.Code, TS.UserName, TS.Description";

                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetTestingStandardDetailsInfo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT TS.Id AS TestingStandardId
                                , TS.Code
                                , TS.UserName AS [Testing Standard]
                                , TS.Description
								, TS.Code AS Category
								, TS.Code AS Testing
								, TS.Code AS Buyer
                                FROM [SCS].[TestingStandard] AS TS
                                WHERE TS.CompanyGroupId = '" + identity.CompanyGroupId + @"'";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetTestingStandardDetailsInfoTesting()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT TS.Id AS TestingStandardId
                                , TS.UserName AS [Testing Standard]
                                , T.TestingCategoryId
                                , TC.UserName AS Category
                                , TSD.TestingId
                                , T.UserName AS Testing
                                FROM [SCS].[TestingStandard] AS TS
                                LEFT OUTER JOIN [SCS].[TestingStandardDetail] AS TSD ON TS.Id=TSD.TestingStandardId
                                LEFT OUTER JOIN [SCS].[Testing] AS T ON T.Id = TSD.TestingId
                                LEFT OUTER JOIN [HKP].[TestingCategory] AS TC ON TC.Id = T.TestingCategoryId
                                WHERE TS.CompanyGroupId = '" + identity.CompanyGroupId + @"'";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetTestingStandardDetailsInfoBuyer()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT TS.Id AS TestingStandardId
                                , TS.UserName AS [Testing Standard]
                                , TSB.BuyerId
                                , B.UserName AS Buyer
                                FROM [SCS].[TestingStandard] AS TS
                                LEFT OUTER JOIN [SCS].[TestingStandardBuyer] AS TSB ON TS.Id=TSB.TestingStandardId
                                LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = TSB.BuyerId
                                WHERE TS.CompanyGroupId = '" + identity.CompanyGroupId + @"'";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetTestingData(string testingStandardId, DataTable dtTesting)
        {
            DataView dv = null;
            try
            {
                dv = new DataView(dtTesting)
                {
                    RowFilter = "TestingStandardId='" + testingStandardId + "'"
                };
                return dv.ToTable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetBuyerData(string testingStandardId, DataTable dtBuyer)
        {
            DataView dv = null;
            try
            {
                dv = new DataView(dtBuyer)
                {
                    RowFilter = "TestingStandardId='" + testingStandardId + "'"
                };
                return dv.ToTable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook TestingStandard_Report(ExcelEngine excelEngine, string testing)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                DataSet dsLocal = GetTestingStandardInfo();
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];

                if (testing == "WithTesting")
                {
                    CreateSheet_TestingStandardDetails(ref sheet1, oRU, "Testing Standard", "Testing Standard", dsLocal);
                }
                else
                {
                    CreateSheet_TestingStandard(ref sheet1, oRU, "Testing Standard", "Testing Standard", dsLocal);
                }

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_TestingStandard(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dslocal)
        {
            try
            {
                DataTable dtTestingStandard = null;

                #region List data

                DataSet TestingStandardList = GetTestingStandardInfo();
                DataView dvMainBody = new DataView(TestingStandardList.Tables[0])
                {
                    Sort = "Testing Standard"
                };
                dtTestingStandard = dvMainBody.ToTable(true, "Id", "Code", "Testing Standard", "Description", "No. of Testing", "No. of Buyer");

                if (dtTestingStandard.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 4;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;

                _rowL = 5;
                _rowL++;

                for (int i = 0; i < dtTestingStandard.Columns.Count; i++)
                {
                    if (dtTestingStandard.Columns[i].ColumnName != "TotalRows" && dtTestingStandard.Columns[i].ColumnName != "Id")
                    {
                        _colIndex++;
                        oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtTestingStandard.Columns[i].ColumnName);
                    }
                }
                shet2EndxlsCol = _colIndex;

                for (int q = 0; q < dtTestingStandard.Rows.Count; q++)
                {
                    _rowL++;
                    oRU.SetText(ref sheet, _rowL, 1, dtTestingStandard.Rows[q]["Code"].ToString(), 15);
                    oRU.SetText(ref sheet, _rowL, 2, dtTestingStandard.Rows[q]["Testing Standard"].ToString(), 40);
                    oRU.SetText(ref sheet, _rowL, 3, dtTestingStandard.Rows[q]["Description"].ToString(), 40);
                    oRU.SetText(ref sheet, _rowL, 4, dtTestingStandard.Rows[q]["No. of Testing"].ToString(), 15);
                    oRU.SetText(ref sheet, _rowL, 5, dtTestingStandard.Rows[q]["No. of Buyer"].ToString(), 15);
                }

                sheet.Range[6, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Testing Standard", identity.CompanyGroupId);
                oRU.FreezePage(ref sheet, 1, 7);
                oRU.PageSetup(ref sheet, 7, ExcelPageOrientation.Portrait);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_TestingStandardDetails(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dslocal)
        {
            try
            {
                DataTable dtTestingStandard = null;

                #region List data

                DataSet TestingStandardList = GetTestingStandardDetailsInfo();
                DataView dvMainBody = new DataView(TestingStandardList.Tables[0])
                {
                    Sort = "Testing Standard"
                };
                dtTestingStandard = dvMainBody.ToTable(true, "TestingStandardId", "Code", "Testing Standard", "Description", "Category", "Testing", "Buyer");

                DataSet TestingStandardTestingList = GetTestingStandardDetailsInfoTesting();
                DataView dvTesting = new DataView(TestingStandardTestingList.Tables[0])
                {
                    Sort = "Testing Standard"
                };
                DataTable dtTesting = dvTesting.ToTable(false, "TestingStandardId", "TestingCategoryId", "Category", "TestingId", "Testing");

                DataSet TestingStandardBuyerList = GetTestingStandardDetailsInfoBuyer();
                DataView dvBuyer = new DataView(TestingStandardBuyerList.Tables[0])
                {
                    Sort = "Testing Standard"
                };
                DataTable dtBuyer = dvBuyer.ToTable(false, "TestingStandardId", "BuyerId", "Buyer");

                if (dtTestingStandard.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 4;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;
                var groupMasterColIndex = 1;

                _rowL = 5;
                _rowL++;

                for (int i = 0; i < dtTestingStandard.Columns.Count; i++)
                {
                    if (dtTestingStandard.Columns[i].ColumnName != "TotalRows" && dtTestingStandard.Columns[i].ColumnName != "TestingStandardId")
                    {
                        _colIndex++;
                        oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtTestingStandard.Columns[i].ColumnName);
                    }
                }

                shet2EndxlsCol = _colIndex;

                var buyerRow = 0;
                for (int q = 0; q < dtTestingStandard.Rows.Count; q++)
                {
                    if (buyerRow > _rowL)
                    {
                        _rowL = buyerRow;
                    }
                    _rowL++;
                    buyerRow = _rowL;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtTestingStandard.Rows[q]["Code"].ToString(), 15); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtTestingStandard.Rows[q]["Testing Standard"].ToString(), 26); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtTestingStandard.Rows[q]["Description"].ToString(), 40); groupMasterColIndex++;

                    string buyerId = dtTestingStandard.Rows[q]["TestingStandardId"].ToString();
                    DataTable dtBuyerData = GetBuyerData(buyerId, dtBuyer);

                    for (int a = 0; a < dtBuyerData.Rows.Count; a++)
                    {
                        oRU.SetText(ref sheet, buyerRow, 6, dtBuyerData.Rows[a]["Buyer"].ToString(), 26);
                        buyerRow++;
                    }

                    string testingId = dtTestingStandard.Rows[q]["TestingStandardId"].ToString();
                    DataTable dtTestingData = GetTestingData(testingId, dtTesting);
                    if (dtTestingData.Rows.Count > 1)
                    {
                        for (int a = 0; a < dtTestingData.Rows.Count; a++)
                        {
                            oRU.SetText(ref sheet, _rowL, 4, dtTestingData.Rows[a]["Category"].ToString(), 20);
                            oRU.SetText(ref sheet, _rowL, 5, dtTestingData.Rows[a]["Testing"].ToString(), 26);
                            _rowL++;
                        }
                    }
                    groupMasterColIndex = 1;
                }

                sheet.Range[6, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Testing Standard", identity.CompanyGroupId);
                oRU.FreezePage(ref sheet, 1, 7);
                oRU.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Testing Standard Report

        #region Process Set Report

        private DataSet GetProcessInfo(string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT CP.Id AS CompanyProcessId
	                                   , C.UserName AS Company
                                       , P.Code
	                                   , P.UserName AS [Name]
                                       , P.StandardName [Local Name]
                                       , P.ShortName
	                                   , [Production Process]= CASE WHEN P.IsProductionProcess=1 THEN 'Yes' ELSE 'No' END
	                                   , [Process Routing]= CASE WHEN P.IsProcessRouting=1 THEN 'Yes' ELSE 'No' END
	                                   , [Locked]= CASE WHEN P.IsLocked=1 THEN 'Yes' ELSE 'No' END
                                       FROM [MST].[CompanyProcess] AS CP
                                       LEFT OUTER JOIN [HKP].[Process] AS P ON CP.ProcessId=P.Id
	                                   LEFT OUTER JOIN [ORG].[Company] AS C ON C.Id=CP.CompanyId
                                       WHERE CP.CompanyGroupId = '" + identity.CompanyGroupId + @"' AND CP.CompanyId='" + companyId + @"' AND CP.Archive=0";

                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetProcessDetailsInfo1(string companyId, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT PS.Id AS ProcessSetId
	                            ,C.UserName AS Company
	                            ,E.UserName AS Entity
	                            ,PS.Code
	                            ,PS.[Description]
	                            ,PS.RequiredTimeUnit AS [Required Time Unit]
	                            ,PCAT.UserName AS Category
	                            ,PCRI.UserName AS Criteria
                                ,PS.Code AS Process
                                FROM [HKP].[ProcessSet] AS PS
                                LEFT OUTER JOIN [ORG].[Entity] AS E ON PS.EntityId=E.Id
                                LEFT OUTER JOIN [HKP].[ProcessCategory] AS PCAT ON PCAT.Id=PS.ProcessCategoryId
                                LEFT OUTER JOIN [HKP].[ProcessCriteria] AS PCRI ON PCRI.Id=PS.ProcessCriteriaId
		                        LEFT OUTER JOIN [ORG].[Company] AS C ON C.Id=PS.CompanyId
                                WHERE PS.CompanyId = '" + companyId + @"' AND PS.EntityId='" + entityId + @"'";

                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetProcessDetailsInfo2(string companyId, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT PS.Id AS ProcessSetId
                                ,PS.Code
	                            ,P.UserName AS Process
		                        ,PSD.Sequence
                                FROM [HKP].[ProcessSet] AS PS
		                        LEFT OUTER JOIN [HKP].[ProcessSetDetail] AS PSD ON PSD.ProcessSetId=PS.Id
		                        LEFT OUTER JOIN [HKP].[Process] AS P ON P.Id=PSD.ProcessId
                                WHERE PS.CompanyId = '" + companyId + @"' AND PS.EntityId='" + entityId + @"'";

                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetProcessDetailsAllEntityInfo1(string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT PS.Id AS ProcessSetId
	                            ,C.UserName AS Company
	                            ,E.UserName AS Entity
	                            ,PS.Code
	                            ,PS.[Description]
	                            ,PS.RequiredTimeUnit AS [Required Time Unit]
	                            ,PCAT.UserName AS Category
	                            ,PCRI.UserName AS Criteria
                                ,PS.Code AS Process
                                FROM [HKP].[ProcessSet] AS PS
                                LEFT OUTER JOIN [ORG].[Entity] AS E ON PS.EntityId=E.Id
                                LEFT OUTER JOIN [HKP].[ProcessCategory] AS PCAT ON PCAT.Id=PS.ProcessCategoryId
                                LEFT OUTER JOIN [HKP].[ProcessCriteria] AS PCRI ON PCRI.Id=PS.ProcessCriteriaId
		                        LEFT OUTER JOIN [ORG].[Company] AS C ON C.Id=PS.CompanyId
                                WHERE PS.CompanyId = '" + companyId + @"'";

                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetProcessDetailsAllEntityInfo2(string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT PS.Id AS ProcessSetId
	                            ,E.UserName AS Entity
                                ,PS.Code
	                            ,P.UserName AS Process
		                        ,PSD.Sequence
                                FROM [HKP].[ProcessSet] AS PS
		                        LEFT OUTER JOIN [HKP].[ProcessSetDetail] AS PSD ON PSD.ProcessSetId=PS.Id
		                        LEFT OUTER JOIN [HKP].[Process] AS P ON P.Id=PSD.ProcessId
		                        LEFT OUTER JOIN [ORG].[Entity] AS E ON PS.EntityId=E.Id
                                WHERE PS.CompanyId = '" + companyId + @"'";

                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetProcessMainData(string processSetId, DataTable dtProcess)
        {
            DataView dv = null;
            try
            {
                dv = new DataView(dtProcess)
                {
                    RowFilter = "ProcessSetId='" + processSetId + "'"
                };
                return dv.ToTable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetProcessMainDataEntity(string entity, string processSetId, DataTable dtProcess)
        {
            try
            {
                DataView dv = new DataView(dtProcess)
                {
                    RowFilter = "Entity='" + entity + "' AND ProcessSetId='" + processSetId + "'"
                };
                return dv.ToTable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook ProcessSet_Report(ExcelEngine excelEngine, string companyId, string entityId, string process)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                DataSet dsLocal = GetProcessInfo(companyId);
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];

                if (process == "Process")
                {
                    CreateSheet_Process(ref sheet1, oRU, "Process", "Process", dsLocal, companyId);
                }
                else if (process == "ProcessSet" && entityId == "null ")
                {
                    CreateSheet_ProcessSetAllEntity(ref sheet1, oRU, "Process Set", "Process Set", dsLocal, companyId);
                }
                else
                {
                    CreateSheet_ProcessSet(ref sheet1, oRU, "Process Set", "Process Set", dsLocal, companyId, entityId);
                }

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_Process(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dslocal, string companyId)
        {
            try
            {
                DataTable dtProcess = null;

                #region List data

                DataSet ProcessList = GetProcessInfo(companyId);
                DataView dvMainBody = new DataView(ProcessList.Tables[0])
                {
                    Sort = "Name"
                };
                dtProcess = dvMainBody.ToTable(true, "CompanyProcessId", "Company", "Code", "Name", "Local Name", "Alias", "Production Process", "Process Routing", "Locked");

                if (dtProcess.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;
                var _col3 = 3;

                oRU.SetMasterHeaderText(ref sheet, _rowL, _col, "Company");
                sheet[oRU.GetColumnNameForXls(_col) + _rowL + ":" + oRU.GetColumnNameForXls(_col + 1) + _rowL].Merge();
                oRU.SetText(ref sheet, _rowL, _col + 2, dtProcess.Rows[0]["Company"].ToString()); _rowL++;
                sheet[oRU.GetColumnNameForXls(_col3) + _rowL + ":" + oRU.GetColumnNameForXls(_col3 + 2) + _rowL].Merge();

                _rowL = 6;
                _rowL++;

                for (int i = 0; i < dtProcess.Columns.Count; i++)
                {
                    if (dtProcess.Columns[i].ColumnName != "TotalRows" && dtProcess.Columns[i].ColumnName != "CompanyProcessId" && dtProcess.Columns[i].ColumnName != "Company")
                    {
                        _colIndex++;
                        oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtProcess.Columns[i].ColumnName);
                    }
                }
                shet2EndxlsCol = _colIndex;

                for (int q = 0; q < dtProcess.Rows.Count; q++)
                {
                    _rowL++;
                    oRU.SetText(ref sheet, _rowL, 1, dtProcess.Rows[q]["Code"].ToString(), 15);
                    oRU.SetText(ref sheet, _rowL, 2, dtProcess.Rows[q]["Name"].ToString(), 26);
                    oRU.SetText(ref sheet, _rowL, 3, dtProcess.Rows[q]["Local Name"].ToString(), 26);
                    oRU.SetText(ref sheet, _rowL, 4, dtProcess.Rows[q]["Alias"].ToString(), 26);
                    oRU.SetText(ref sheet, _rowL, 5, dtProcess.Rows[q]["Production Process"].ToString(), 20);
                    oRU.SetText(ref sheet, _rowL, 6, dtProcess.Rows[q]["Process Routing"].ToString(), 20);
                    oRU.SetText(ref sheet, _rowL, 7, dtProcess.Rows[q]["Locked"].ToString(), 20);
                }

                sheet.Range[7, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Process", identity.CompanyGroupId);
                oRU.FreezePage(ref sheet, 1, 8);
                oRU.PageSetup(ref sheet, 8, ExcelPageOrientation.Portrait);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_ProcessSet(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dslocal, string companyId, string entityId)
        {
            try
            {
                DataTable dtProcessSet = null;

                #region List data

                DataSet ProcessSetList = GetProcessDetailsInfo1(companyId, entityId);
                DataView dvMainBody = new DataView(ProcessSetList.Tables[0])
                {
                    Sort = "Code"
                };
                dtProcessSet = dvMainBody.ToTable(true, "ProcessSetId", "Company", "Entity", "Code", "Description", "Required Time Unit", "Category", "Criteria", "Process");

                DataSet ProcessList = GetProcessDetailsInfo2(companyId, entityId);
                DataView dvProcess = new DataView(ProcessList.Tables[0])
                {
                    Sort = "Code, Sequence"
                };
                DataTable dtProcess = dvProcess.ToTable(false, "ProcessSetId", "Code", "Process", "Sequence");

                if (dtProcessSet.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;
                var groupMasterColIndex = 1;

                var _col3 = 3;

                oRU.SetMasterHeaderText(ref sheet, _rowL, _col, "Company");
                sheet[oRU.GetColumnNameForXls(_col) + _rowL + ":" + oRU.GetColumnNameForXls(_col + 1) + _rowL].Merge();
                oRU.SetText(ref sheet, _rowL, _col + 2, dtProcessSet.Rows[0]["Company"].ToString()); _rowL++;
                sheet[oRU.GetColumnNameForXls(_col3) + _rowL + ":" + oRU.GetColumnNameForXls(_col3 + 2) + _rowL].Merge();

                var _rowR = 5;
                var _colR = 4;
                var _col8 = 6;

                oRU.SetMasterHeaderText(ref sheet, _rowR, _colR, "Entity");
                sheet[oRU.GetColumnNameForXls(_colR) + _rowR + ":" + oRU.GetColumnNameForXls(_colR + 1) + _rowR].Merge();
                oRU.SetText(ref sheet, _rowR, _colR + 2, dtProcessSet.Rows[0]["Entity"].ToString()); _rowR++;
                sheet[oRU.GetColumnNameForXls(_col8) + _rowR + ":" + oRU.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

                _rowL = 6;
                _rowL++;

                for (int i = 0; i < dtProcessSet.Columns.Count; i++)
                {
                    if (dtProcessSet.Columns[i].ColumnName != "TotalRows" && dtProcessSet.Columns[i].ColumnName != "ProcessSetId" && dtProcessSet.Columns[i].ColumnName != "Company" && dtProcessSet.Columns[i].ColumnName != "Entity")
                    {
                        _colIndex++;
                        oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtProcessSet.Columns[i].ColumnName);
                    }
                }

                shet2EndxlsCol = _colIndex;

                for (int q = 0; q < dtProcessSet.Rows.Count; q++)
                {
                    _rowL++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtProcessSet.Rows[q]["Code"].ToString(), 15); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtProcessSet.Rows[q]["Description"].ToString(), 26); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtProcessSet.Rows[q]["Required Time Unit"].ToString(), 26); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtProcessSet.Rows[q]["Category"].ToString(), 26); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtProcessSet.Rows[q]["Criteria"].ToString(), 26); groupMasterColIndex++;

                    string processId = dtProcessSet.Rows[q]["ProcessSetId"].ToString();
                    DataTable dtProcessData = GetProcessMainData(processId, dtProcess);
                    if (dtProcessData.Rows.Count > 1)
                    {
                        for (int a = 0; a < dtProcessData.Rows.Count; a++)
                        {
                            oRU.SetText(ref sheet, _rowL, 6, dtProcessData.Rows[a]["Process"].ToString(), 26);
                            _rowL++;
                        }
                    }
                    groupMasterColIndex = 1;
                }

                sheet.Range[7, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Process Set", identity.CompanyGroupId);
                oRU.FreezePage(ref sheet, 1, 8);
                oRU.PageSetup(ref sheet, 8, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_ProcessSetAllEntity(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dslocal, string companyId)
        {
            try
            {
                DataTable dtProcessSet = null;

                #region List data

                DataSet ProcessSetList = GetProcessDetailsAllEntityInfo1(companyId);
                DataView dvMainBody = new DataView(ProcessSetList.Tables[0])
                {
                    Sort = "Entity, Code"
                };
                dtProcessSet = dvMainBody.ToTable(true, "ProcessSetId", "Company", "Entity", "Code", "Description", "Required Time Unit", "Category", "Criteria", "Process");

                DataSet ProcessList = GetProcessDetailsAllEntityInfo2(companyId);
                DataView dvProcess = new DataView(ProcessList.Tables[0])
                {
                    Sort = "Entity, Code, Sequence"
                };
                DataTable dtProcess = dvProcess.ToTable(false, "ProcessSetId", "Entity", "Code", "Process", "Sequence");

                if (dtProcessSet.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;
                var groupMasterColIndex = 1;

                var _col3 = 3;

                oRU.SetMasterHeaderText(ref sheet, _rowL, _col, "Company");
                sheet[oRU.GetColumnNameForXls(_col) + _rowL + ":" + oRU.GetColumnNameForXls(_col + 1) + _rowL].Merge();
                oRU.SetText(ref sheet, _rowL, _col + 2, dtProcessSet.Rows[0]["Company"].ToString()); _rowL++;
                sheet[oRU.GetColumnNameForXls(_col3) + _rowL + ":" + oRU.GetColumnNameForXls(_col3 + 2) + _rowL].Merge();

                _rowL = 6;
                _rowL++;

                for (int i = 0; i < dtProcessSet.Columns.Count; i++)
                {
                    if (dtProcessSet.Columns[i].ColumnName != "TotalRows" && dtProcessSet.Columns[i].ColumnName != "ProcessSetId" && dtProcessSet.Columns[i].ColumnName != "Company")
                    {
                        _colIndex++;
                        oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtProcessSet.Columns[i].ColumnName);
                    }
                }

                shet2EndxlsCol = _colIndex;

                for (int q = 0; q < dtProcessSet.Rows.Count; q++)
                {
                    _rowL++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtProcessSet.Rows[q]["Entity"].ToString(), 26); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtProcessSet.Rows[q]["Code"].ToString(), 15); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtProcessSet.Rows[q]["Description"].ToString(), 26); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtProcessSet.Rows[q]["Required Time Unit"].ToString(), 26); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtProcessSet.Rows[q]["Category"].ToString(), 26); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtProcessSet.Rows[q]["Criteria"].ToString(), 26); groupMasterColIndex++;

                    string processId = dtProcessSet.Rows[q]["ProcessSetId"].ToString();
                    string entity = dtProcessSet.Rows[q]["Entity"].ToString();
                    DataTable dtProcessData = GetProcessMainDataEntity(entity, processId, dtProcess);
                    for (int a = 0; a < dtProcessData.Rows.Count; a++)
                    {
                        oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtProcessData.Rows[a]["Process"].ToString(), 26);
                        _rowL++;
                    }
                    groupMasterColIndex = 1;
                }

                sheet.Range[7, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Process Set", identity.CompanyGroupId);
                oRU.FreezePage(ref sheet, 1, 8);
                oRU.PageSetup(ref sheet, 8, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Process Set Report

        #region Legal Salary Report

        private DataSet GetLegalSalaryGrade(string effectiveDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT LS.Id LegalSalaryStructureId,LS.EffectiveDate
                                , LG.UserName AS Grade
                                , LG.Id AS LegalSalaryGradeId
                                FROM [MST].[LegalSalaryStructure] AS LS
                                LEFT OUTER JOIN [SCS].[LegalSalaryGrade] AS LG ON LG.Id=LS.LegalSalaryGradeId
                                WHERE LG.CompanyGroupId='" + identity.CompanyGroupId + @"' AND LG.Archive = 0 AND LG.Active = 1
                                AND CAST(LS.EffectiveDate AS DATE)=(SELECT MAX(EffectiveDate) AS EffectiveDate FROM MST.LegalSalaryStructure WHERE LegalSalaryGradeId=LS.LegalSalaryGradeId AND CAST(EffectiveDate AS DATE)<='" + effectiveDate + @"' GROUP BY LegalSalaryGradeId)";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetLegalSalaryInfo(string effectiveDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT LS.Id AS LegalSalaryStructureId,LS.EffectiveDate, LG.Sequence
                                , LG.UserName AS Grade
                                , LG.Id AS LegalSalaryGradeId
                                , SH.SalaryHeadID
                                , SH.SalaryHead AS Head
                                , LSV.Id AS ValueId
                                , LSV.SalaryHeadValue AS Value
                                FROM [MST].[LegalSalaryStructureValue] AS LSV
                                LEFT OUTER JOIN [MST].[LegalSalaryStructure] AS LS ON LS.Id =LSV.LegalSalaryStructureId
                                LEFT OUTER JOIN [SCS].[LegalSalaryGrade] AS LG ON LG.Id=LS.LegalSalaryGradeId
                                LEFT OUTER JOIN [SalaryHead] AS SH ON SH.SalaryHeadID=LSV.SalaryHeadId
                                WHERE LG.CompanyGroupId='" + identity.CompanyGroupId + @"' AND LG.Archive = 0 AND LG.Active = 1
                                AND CAST(LS.EffectiveDate AS DATE)=(SELECT MAX(EffectiveDate) AS EffectiveDate FROM MST.LegalSalaryStructure WHERE LegalSalaryGradeId=LS.LegalSalaryGradeId AND CAST(EffectiveDate AS DATE)<='" + effectiveDate + @"' GROUP BY LegalSalaryGradeId)";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetLegalSalaryGrade1(string effectiveDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT LS.Id LegalSalaryStructureId,LS.EffectiveDate
                                , LG.UserName AS Grade
                                , LG.Id AS LegalSalaryGradeId
                                FROM [MST].[LegalSalaryStructure] AS LS
                                LEFT OUTER JOIN [SCS].[LegalSalaryGrade] AS LG ON LG.Id=LS.LegalSalaryGradeId
                                WHERE LG.CompanyGroupId='" + identity.CompanyGroupId + @"' AND LG.Archive = 0 AND LG.Active = 1
                                AND CAST(LS.EffectiveDate AS DATE)=(SELECT MAX(EffectiveDate) AS EffectiveDate FROM MST.LegalSalaryStructure WHERE LegalSalaryGradeId=LS.LegalSalaryGradeId AND CAST(EffectiveDate AS DATE)<='" + effectiveDate + @"' GROUP BY LegalSalaryGradeId)";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetLegalSalaryInfo1(string effectiveDate, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT LS.Id LegalSalaryStructureId,LS.EffectiveDate, LG.Sequence
                                , LD.Code
                                , LD.UserName AS [Legal Designation]
                                , LG.UserName AS Grade
                                , LG.Id AS LegalSalaryGradeId
                                , C.UserName AS Company
                                , P.UserName AS Plant
                                FROM [MST].[LegalSalaryStructure] AS LS
                                LEFT OUTER JOIN [SCS].[LegalSalaryGrade] AS LG ON LG.Id=LS.LegalSalaryGradeId
                                LEFT OUTER JOIN [MST].[LegalSalaryGradeDesignation] AS LGD ON LGD.LegalSalaryGradeId =LG.Id
                                LEFT OUTER JOIN [HKP].[LegalDesignation] AS LD ON LD.Id=LGD.LegalDesignationId
                                LEFT OUTER JOIN [ORG].[Plant] AS P ON P.Id=LGD.PlantId
                                LEFT OUTER JOIN [ORG].[Company] AS C ON C.Id=P.CompanyId
                                WHERE LG.CompanyGroupId='" + identity.CompanyGroupId + @"' AND LG.Archive = 0 AND LG.Active = 1
                                AND CAST(LS.EffectiveDate AS DATE)=(SELECT MAX(EffectiveDate) AS EffectiveDate FROM MST.LegalSalaryStructure WHERE LegalSalaryGradeId=LS.LegalSalaryGradeId AND CAST(EffectiveDate AS DATE)<='" + effectiveDate + @"' GROUP BY LegalSalaryGradeId)
                                AND LGD.PlantId='" + plantId + @"'";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetLegalSalaryInfo2(string effectiveDate, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT LS.Id AS LegalSalaryStructureId,LS.EffectiveDate, LG.Sequence
                                , LG.UserName AS Grade
                                , LG.Id AS LegalSalaryGradeId
                                , SH.SalaryHeadID
                                , SH.SalaryHead AS Head
                                , LSV.Id AS ValueId
                                , LSV.SalaryHeadValue AS Value
                                FROM [MST].[LegalSalaryStructureValue] AS LSV
                                LEFT OUTER JOIN [MST].[LegalSalaryStructure] AS LS ON LS.Id =LSV.LegalSalaryStructureId
                                LEFT OUTER JOIN [SCS].[LegalSalaryGrade] AS LG ON LG.Id=LS.LegalSalaryGradeId
                                LEFT OUTER JOIN [SalaryHead] AS SH ON SH.SalaryHeadID=LSV.SalaryHeadId
                                WHERE LG.CompanyGroupId='" + identity.CompanyGroupId + @"' AND LG.Archive = 0 AND LG.Active = 1
                                AND CAST(LS.EffectiveDate AS DATE)=(SELECT MAX(EffectiveDate) AS EffectiveDate FROM MST.LegalSalaryStructure WHERE LegalSalaryGradeId=LS.LegalSalaryGradeId AND CAST(EffectiveDate AS DATE)<='" + effectiveDate + @"' GROUP BY LegalSalaryGradeId)";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private int GetMaxHeadCounted(DataTable dtMain, DataTable dtHead)
        {
            var result = 0;
            DataView dv = null;
            try
            {
                for (int i = 0; i < dtMain.Rows.Count; i++)
                {
                    string gradeId = dtMain.Rows[i]["LegalSalaryGradeId"].ToString();
                    dv = new DataView(dtHead)
                    {
                        RowFilter = "LegalSalaryGradeId='" + gradeId + "'"
                    };
                    if (result < dv.Count)
                    {
                        result = dv.Count;
                    }
                    dv.RowFilter = null;
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetHeadData(string gradeId, DataTable dtHead)
        {
            try
            {
                DataView dv = new DataView(dtHead)
                {
                    RowFilter = "LegalSalaryGradeId='" + gradeId + "'"
                };
                return dv.ToTable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetGradeData(string gradeId, DataTable dtGrade)
        {
            DataView dv = null;
            try
            {
                dv = new DataView(dtGrade)
                {
                    RowFilter = "LegalSalaryGradeId='" + gradeId + "'"
                };
                return dv.ToTable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook LegalSalary_Report(ExcelEngine excelEngine, string effectiveDate, string plantId)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                DataSet dsLocal = GetLegalSalaryInfo(effectiveDate);
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];

                if (plantId != "null")
                {
                    CreateSheet_LegalSalary1(ref sheet1, oRU, "Legal Salary", "Legal Salary", dsLocal, effectiveDate, plantId);
                }
                else
                {
                    CreateSheet_LegalSalary(ref sheet1, oRU, "Legal Salary", "Legal Salary", dsLocal, effectiveDate);
                }

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_LegalSalary(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dslocal, string effectiveDate)
        {
            try
            {
                DataTable dtLegalSalary = null;

                #region List data

                DataSet LegalSalaryList = GetLegalSalaryInfo(effectiveDate);
                DataView dvMainBody = new DataView(LegalSalaryList.Tables[0])
                {
                    Sort = "Grade, Sequence"
                };
                dtLegalSalary = dvMainBody.ToTable(true, "LegalSalaryStructureId", "Grade", "LegalSalaryGradeId");

                DataSet LegalSalaryGradeList = GetLegalSalaryGrade(effectiveDate);
                DataView dvGrade = new DataView(LegalSalaryGradeList.Tables[0])
                {
                    Sort = "Grade"
                };
                DataTable dtLegalSalaryGrade = dvGrade.ToTable(true, "Grade", "LegalSalaryGradeId");

                DataSet LegalSalaryHeadList = GetLegalSalaryInfo(effectiveDate);
                DataView dvHead = new DataView(LegalSalaryHeadList.Tables[0])
                {
                    Sort = "Grade, Sequence"
                };
                DataTable dtHead = dvHead.ToTable(false, "Grade", "LegalSalaryGradeId", "SalaryHeadID", "Head", "ValueId", "Value");

                var maxHeadCol = GetMaxHeadCounted(dtLegalSalary, dtHead);

                if (dtLegalSalary.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;
                var groupMasterColIndex = 2;

                _rowL++;

                for (int i = 0; i < dtLegalSalary.Columns.Count; i++)
                {
                    if (dtLegalSalary.Columns[i].ColumnName != "LegalSalaryStructureId" && dtLegalSalary.Columns[i].ColumnName != "LegalSalaryGradeId")
                    {
                        _colIndex++;
                        oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtLegalSalary.Columns[i].ColumnName);
                    }
                }

                var headCount = 0;
                for (int i = 0; i < maxHeadCol; i++)
                {
                    headCount++;
                    _colIndex++;
                    oRU.SetHeaderText(ref sheet, _rowL, _colIndex, "");
                }

                shet2EndxlsCol = _colIndex;

                for (int q = 0; q < dtLegalSalaryGrade.Rows.Count; q++)
                {
                    _rowL++;

                    oRU.SetText(ref sheet, _rowL, 1, dtLegalSalaryGrade.Rows[q]["Grade"].ToString());

                    string gradeId0 = dtLegalSalaryGrade.Rows[q]["LegalSalaryGradeId"].ToString();
                    DataTable dtHeadData0 = GetHeadData(gradeId0, dtHead);
                    for (int k = 0; k < dtHeadData0.Rows.Count; k++)
                    {
                        oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtHeadData0.Rows[k]["Head"].ToString(), 20); groupMasterColIndex++;
                    }
                    if (maxHeadCol > dtHeadData0.Rows.Count)
                    {
                        for (int c = 0; c < maxHeadCol - dtHeadData0.Rows.Count; c++)
                        {
                            oRU.SetText(ref sheet, _rowL, groupMasterColIndex, ""); groupMasterColIndex++;
                        }
                    }

                    string gradeId1 = dtLegalSalaryGrade.Rows[q]["LegalSalaryGradeId"].ToString();
                    DataTable dtLegalSalary1 = GetGradeData(gradeId1, dtLegalSalary);
                    for (int a = 0; a < dtLegalSalary1.Rows.Count; a++)
                    {
                        _rowL++;
                        groupMasterColIndex = 2;

                        string gradeId = dtLegalSalary1.Rows[a]["LegalSalaryGradeId"].ToString();
                        DataTable dtHeadData = GetHeadData(gradeId, dtHead);
                        var rowCount = _rowL;
                        for (int k = 0; k < dtHeadData.Rows.Count; k++)
                        {
                            oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtHeadData.Rows[k]["Value"].ToString()); groupMasterColIndex++;
                        }
                    }
                    groupMasterColIndex = 2;
                }

                sheet.Range[6, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Legal Salary", identity.CompanyGroupId);
                oRU.SetText(ref sheet, 4, shet2EndxlsCol, "( As on " + "" + effectiveDate + " )", ExcelHAlign.HAlignCenter);
                sheet.Range[oRU.GetColumnNameForXls(1) + 4 + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + 4].Merge();
                //oRU.FreezePage(ref sheet, 1, 7);
                oRU.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_LegalSalary1(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dslocal, string effectiveDate, string plantId)
        {
            try
            {
                DataTable dtLegalSalary = null;

                #region List data

                DataSet LegalSalaryList = GetLegalSalaryInfo1(effectiveDate, plantId);
                DataView dvMainBody = new DataView(LegalSalaryList.Tables[0])
                {
                    Sort = "Grade, Sequence"
                };
                dtLegalSalary = dvMainBody.ToTable(true, "LegalSalaryStructureId", "Grade", "Code", "Legal Designation", "LegalSalaryGradeId", "Company", "Plant");

                DataSet LegalSalaryGradeList = GetLegalSalaryGrade1(effectiveDate);
                DataView dvGrade = new DataView(LegalSalaryGradeList.Tables[0])
                {
                    Sort = "Grade"
                };
                DataTable dtLegalSalaryGrade = dvGrade.ToTable(true, "Grade", "LegalSalaryGradeId");

                DataSet LegalSalaryHeadList = GetLegalSalaryInfo2(effectiveDate, plantId);
                DataView dvHead = new DataView(LegalSalaryHeadList.Tables[0])
                {
                    Sort = "Grade, Sequence"
                };
                DataTable dtHead = dvHead.ToTable(false, "Grade", "LegalSalaryGradeId", "SalaryHeadID", "Head", "ValueId", "Value");

                var maxHeadCol = GetMaxHeadCounted(dtLegalSalary, dtHead);

                if (dtLegalSalary.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 6;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;
                var groupMasterColIndex = 4;

                var _col3 = 3;

                oRU.SetMasterHeaderText(ref sheet, _rowL, _col, "Company");
                sheet[oRU.GetColumnNameForXls(_col) + _rowL + ":" + oRU.GetColumnNameForXls(_col + 1) + _rowL].Merge();
                oRU.SetText(ref sheet, _rowL, _col + 2, dtLegalSalary.Rows[0]["Company"].ToString()); _rowL++;
                sheet[oRU.GetColumnNameForXls(_col3) + _rowL + ":" + oRU.GetColumnNameForXls(_col3 + 2) + _rowL].Merge();

                var _rowR = 6;
                var _colR = 4;
                var _col8 = 5;

                oRU.SetMasterHeaderText(ref sheet, _rowR, _colR, "Plant");
                //sheet[oRU.GetColumnNameForXls(_colR) + _rowR + ":" + oRU.GetColumnNameForXls(_colR + 1) + _rowR].Merge();
                oRU.SetText(ref sheet, _rowR, _colR + 1, dtLegalSalary.Rows[0]["Plant"].ToString()); _rowL++;
                sheet[oRU.GetColumnNameForXls(_col8) + _rowR + ":" + oRU.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

                _rowL = 7;
                _rowL++;

                for (int i = 0; i < dtLegalSalary.Columns.Count; i++)
                {
                    if (dtLegalSalary.Columns[i].ColumnName != "LegalSalaryStructureId" && dtLegalSalary.Columns[i].ColumnName != "LegalSalaryGradeId" && dtLegalSalary.Columns[i].ColumnName != "Company" && dtLegalSalary.Columns[i].ColumnName != "Plant")
                    {
                        _colIndex++;
                        oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtLegalSalary.Columns[i].ColumnName);
                    }
                }

                var headCount = 0;
                for (int i = 0; i < maxHeadCol; i++)
                {
                    headCount++;
                    _colIndex++;
                    oRU.SetHeaderText(ref sheet, _rowL, _colIndex, "");
                }

                shet2EndxlsCol = _colIndex;

                for (int q = 0; q < dtLegalSalaryGrade.Rows.Count; q++)
                {
                    _rowL++;

                    oRU.SetText(ref sheet, _rowL, 1, dtLegalSalaryGrade.Rows[q]["Grade"].ToString());

                    string gradeId0 = dtLegalSalaryGrade.Rows[q]["LegalSalaryGradeId"].ToString();
                    DataTable dtHeadData0 = GetHeadData(gradeId0, dtHead);
                    for (int k = 0; k < dtHeadData0.Rows.Count; k++)
                    {
                        oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtHeadData0.Rows[k]["Head"].ToString(), 20); groupMasterColIndex++;
                    }
                    if (maxHeadCol > dtHeadData0.Rows.Count)
                    {
                        for (int c = 0; c < maxHeadCol - dtHeadData0.Rows.Count; c++)
                        {
                            oRU.SetText(ref sheet, _rowL, groupMasterColIndex, ""); groupMasterColIndex++;
                        }
                    }

                    string gradeId1 = dtLegalSalaryGrade.Rows[q]["LegalSalaryGradeId"].ToString();
                    DataTable dtLegalSalary1 = GetGradeData(gradeId1, dtLegalSalary);
                    for (int a = 0; a < dtLegalSalary1.Rows.Count; a++)
                    {
                        _rowL++;
                        oRU.SetText(ref sheet, _rowL, 2, dtLegalSalary1.Rows[a]["Code"].ToString(), 15);
                        oRU.SetText(ref sheet, _rowL, 3, dtLegalSalary1.Rows[a]["Legal Designation"].ToString(), 40);
                        groupMasterColIndex = 4;

                        string gradeId = dtLegalSalary1.Rows[a]["LegalSalaryGradeId"].ToString();
                        DataTable dtHeadData = GetHeadData(gradeId, dtHead);
                        var rowCount = _rowL;
                        for (int k = 0; k < dtHeadData.Rows.Count; k++)
                        {
                            oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtHeadData.Rows[k]["Value"].ToString()); groupMasterColIndex++;
                        }
                    }
                    groupMasterColIndex = 4;
                }

                sheet.Range[8, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Legal Salary", identity.CompanyGroupId);
                oRU.SetText(ref sheet, 4, shet2EndxlsCol, "( As on " + "" + effectiveDate + " )", ExcelHAlign.HAlignCenter);
                sheet.Range[oRU.GetColumnNameForXls(1) + 4 + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + 4].Merge();
                //oRU.FreezePage(ref sheet, 1, 7);
                oRU.PageSetup(ref sheet, 8, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Legal Salary Report

        #region Compliance Document Report

        private DataSet GetComplianceDocumentList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT CD.Id AS ComplianceDocumentId
                                , CDC.UserName AS Category
                                , CDSC.UserName AS Subcategory
                                , CD.Code
                                , CD.UserName AS Name
                                , CD.DocumentType AS Type
                                , CD.Importance
                                , CD.EmploymentStage AS [Employment Stage]
                                , CD.DependateDate AS [Dependate Date]
                                , CD.EmpType AS [Emp Type]
                                , [Global]= CASE WHEN CD.IsGlobalDocument=1 THEN 'Yes' ELSE 'No' END
                                , CD.DocumentationBy AS [Documentation By]
                                , CD.LeadOrLagDays AS [Lead or Lag Days]
                                , CD.OptionalOrMandatory [Optional or Mandatory]
                                , MB.Code AS [Responsible Person Code]
                                , [Recurring]= CASE WHEN CD.IsRecurring=1 THEN 'Yes' ELSE 'No' END
                                , [Skilled]= CASE WHEN CD.IsSkillBased=1 THEN 'Yes' ELSE 'No' END
                                FROM [HKP].[ComplianceDocument] AS CD
                                LEFT OUTER JOIN [HKP].[ComplianceDocumentCategory] AS CDC ON CD.ComplianceDocumentCategoryId= CDC.Id
                                LEFT OUTER JOIN [HKP].[ComplianceDocumentSubCategory] AS CDSC ON CD.ComplianceDocumentSubCategoryId= CDSC.Id
                                LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB ON CD.ResponsiblePersonId= MB.Id
                                WHERE CD.CompanyGroupId='" + identity.CompanyGroupId + @"' AND CD.Active=1 AND CD.Archive=0";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetComplianceDocumentPosition()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT CD.Id AS ComplianceDocumentId
                                , PO.UserName AS Position
                                FROM [HKP].[ComplianceDocumentPositonCode] AS CDP
                                LEFT OUTER JOIN [HKP].[ComplianceDocument] AS CD ON CD.Id = CDP.ComplianceDocumentId
                                LEFT OUTER JOIN [ORG].[Position] AS PO ON PO.Id=CDP.PositionId
                                WHERE CD.CompanyGroupId='" + identity.CompanyGroupId + @"' AND CD.Active=1 AND CD.Archive=0";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private int GetMaxPositionCounted(DataTable dtMain, DataTable dtPosition)
        {
            var result = 0;
            DataView dv = null;
            try
            {
                for (int i = 0; i < dtMain.Rows.Count; i++)
                {
                    string complianceDocumentId = dtMain.Rows[i]["ComplianceDocumentId"].ToString();
                    dv = new DataView(dtPosition)
                    {
                        RowFilter = "ComplianceDocumentId='" + complianceDocumentId + "'"
                    };
                    if (result < dv.Count)
                    {
                        result = dv.Count;
                    }
                    dv.RowFilter = null;
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetPositionData(string complianceDocumentId, DataTable dtPosition)
        {
            try
            {
                DataView dv = new DataView(dtPosition)
                {
                    RowFilter = "ComplianceDocumentId='" + complianceDocumentId + "'"
                };
                return dv.ToTable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetComplianceDocumentSet()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT CDS.Id AS ComplianceDocumentSetId
                                , CDS.UserName AS [Set]
                                , CD.Id AS ComplianceDocumentId
                                , CDC.UserName AS Category
                                , CDSC.UserName AS Subcategory
                                , CD.Code
                                , CD.UserName AS Name
                                , CD.DocumentType AS Type
                                , CD.Importance
                                , CD.EmploymentStage AS [Employment Stage]
                                , CD.DependateDate AS [Dependate Date]
                                , CD.EmpType AS [Emp Type]
                                , [Global]= CASE WHEN CD.IsGlobalDocument=1 THEN 'Yes' ELSE 'No' END
                                , CD.DocumentationBy AS [Documentation By]
                                , [Skilled]= CASE WHEN CD.IsSkillBased=1 THEN 'Yes' ELSE 'No' END
                                , CD.LeadOrLagDays AS [Lead or Lag Days]
                                , CD.OptionalOrMandatory [Optional or Mandatory]
                                , MB.Code AS [Responsible Person Code]
                                , [Recurring]= CASE WHEN CD.IsRecurring=1 THEN 'Yes' ELSE 'No' END
                                FROM [HKP].[ComplianceDocument] AS CD
                                LEFT OUTER JOIN [HKP].[ComplianceDocumentSetDetail] AS CDSD ON CDSD.ComplianceDocumentId= CD.Id
                                LEFT OUTER JOIN [HKP].[ComplianceDocumentSet] AS CDS ON CDS.Id= CDSD.ComplianceDocumentSetId
                                LEFT OUTER JOIN [HKP].[ComplianceDocumentCategory] AS CDC ON CD.ComplianceDocumentCategoryId= CDC.Id
                                LEFT OUTER JOIN [HKP].[ComplianceDocumentSubCategory] AS CDSC ON CD.ComplianceDocumentSubCategoryId= CDSC.Id
                                LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB ON CD.ResponsiblePersonId= MB.Id
                                WHERE CD.CompanyGroupId='" + identity.CompanyGroupId + @"' AND CD.Active=1 AND CD.Archive=0";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetComplianceDocumentSetPlantWise(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @" SELECT CDS.Id AS ComplianceDocumentSetId
                                , E.UserName AS [Employee Type]
                                , CDS.UserName AS [Set]
                                , CD.Id AS ComplianceDocumentId
                                , CDC.UserName AS Category
                                , CDSC.UserName AS Subcategory
                                , CD.Code
                                , CD.UserName AS Name
                                , CD.DocumentType AS Type
                                , CD.Importance
                                , CD.EmploymentStage AS [Employment Stage]
                                , CD.DependateDate AS [Dependate Date]
                                , CD.EmpType AS [Emp Type]
                                , [Global]= CASE WHEN CD.IsGlobalDocument=1 THEN 'Yes' ELSE 'No' END
                                , CD.DocumentationBy AS [Documentation By]
                                , [Skilled]= CASE WHEN CD.IsSkillBased=1 THEN 'Yes' ELSE 'No' END
                                , CD.LeadOrLagDays AS [Lead or Lag Days]
                                , CD.OptionalOrMandatory [Optional or Mandatory]
                                , MB.Code AS [Responsible Person Code]
                                , [Recurring]= CASE WHEN CD.IsRecurring=1 THEN 'Yes' ELSE 'No' END
                                , C.UserName AS Company
                                , P.UserName AS Plant
                                FROM [HKP].[ComplianceDocument] AS CD
                                LEFT OUTER JOIN [HKP].[ComplianceDocumentSetDetail] AS CDSD ON CDSD.ComplianceDocumentId= CD.Id
                                LEFT OUTER JOIN [HKP].[ComplianceDocumentSet] AS CDS ON CDS.Id= CDSD.ComplianceDocumentSetId
                                LEFT OUTER JOIN [HKP].[ComplianceDocumentCategory] AS CDC ON CD.ComplianceDocumentCategoryId= CDC.Id
                                LEFT OUTER JOIN [HKP].[ComplianceDocumentSubCategory] AS CDSC ON CD.ComplianceDocumentSubCategoryId= CDSC.Id
                                LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB ON CD.ResponsiblePersonId= MB.Id
                                LEFT OUTER JOIN [HKP].[DocumentConfigurationDesignationGroup] AS DC ON DC.ComplianceDocumentSetId = CDS.Id
                                LEFT OUTER JOIN [ORG].[Plant] AS P ON P.Id=DC.PlantId
                                LEFT OUTER JOIN [ORG].[Company] AS C ON C.Id=P.CompanyId
                                LEFT OUTER JOIN [HKP].[EmployeeCategory] AS E ON E.Id=DC.EmployeeCategoryId
                                WHERE CD.CompanyGroupId='" + identity.CompanyGroupId + @"' AND CD.Active=1 AND CD.Archive=0 AND DC.PlantId='" + plantId + @"'";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook ComplianceDocument_Report(ExcelEngine excelEngine, string documentLevel, string plantId)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                var dsLocal = GetComplianceDocumentList();
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];

                if (documentLevel == "Document")
                {
                    CreateSheet_DocumentList(ref sheet1, oRU, "Compliance Document List", "Compliance Document List", dsLocal);
                }
                else if (documentLevel == "DocumentSet")
                {
                    CreateSheet_DocumentSet(ref sheet1, oRU, "Compliance Document Set", "Compliance Document Set", dsLocal);
                }
                else
                {
                    CreateSheet_DocumentSetPlantWise(ref sheet1, oRU, "Compliance Document Set", "Compliance Document Set", dsLocal, plantId);
                }

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_DocumentList(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dslocal)
        {
            try
            {
                DataTable dtComplianceDocument = null;

                #region List data

                DataSet ComplianceDocumentList = GetComplianceDocumentList();
                DataView dvMainBody = new DataView(ComplianceDocumentList.Tables[0])
                {
                    Sort = "ComplianceDocumentId"
                };
                dtComplianceDocument = dvMainBody.ToTable(true, "ComplianceDocumentId", "Category", "Subcategory", "Code", "Name", "Type", "Importance", "Employment Stage", "Dependate Date", "Emp Type", "Global", "Documentation By", "Lead or Lag Days", "Optional or Mandatory", "Responsible Person Code", "Recurring", "Skilled");

                DataSet PositionList = GetComplianceDocumentPosition();
                DataView dvHead = new DataView(PositionList.Tables[0])
                {
                    Sort = "ComplianceDocumentId"
                };
                DataTable dtHead = dvHead.ToTable(false, "ComplianceDocumentId", "Position");

                var maxHeadCol = GetMaxPositionCounted(dtComplianceDocument, dtHead);

                if (dtComplianceDocument.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;
                var groupMasterColIndex = 1;

                _rowL++;

                for (int i = 0; i < dtComplianceDocument.Columns.Count; i++)
                {
                    if (dtComplianceDocument.Columns[i].ColumnName != "ComplianceDocumentId")
                    {
                        _colIndex++;
                        oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtComplianceDocument.Columns[i].ColumnName);
                    }
                }

                var headCount = 0;
                for (int i = 0; i < maxHeadCol; i++)
                {
                    headCount++;
                    _colIndex++;
                    oRU.SetHeaderText(ref sheet, _rowL, _colIndex, "Position" + headCount);
                }

                shet2EndxlsCol = _colIndex;

                for (int q = 0; q < dtComplianceDocument.Rows.Count; q++)
                {
                    _rowL++;

                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Category"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Subcategory"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Code"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Name"].ToString(), 40); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Type"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Importance"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Employment Stage"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Dependate Date"].ToString(), 24); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Emp Type"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Global"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Documentation By"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Lead or Lag Days"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Optional or Mandatory"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Responsible Person Code"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Recurring"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Skilled"].ToString()); groupMasterColIndex++;

                    string complianceDocumentId = dtComplianceDocument.Rows[q]["ComplianceDocumentId"].ToString();

                    DataTable dtHeadData = GetPositionData(complianceDocumentId, dtHead);
                    var rowCount = _rowL;
                    for (int a = 0; a < dtHeadData.Rows.Count; a++)
                    {
                        oRU.SetText(ref sheet, rowCount, groupMasterColIndex, dtHeadData.Rows[a]["Position"].ToString(), 26); groupMasterColIndex++;
                    }
                    if (maxHeadCol > dtHeadData.Rows.Count)
                    {
                        for (int c = 0; c < maxHeadCol - dtHeadData.Rows.Count; c++)
                        {
                            oRU.SetText(ref sheet, _rowL, groupMasterColIndex, ""); groupMasterColIndex++;
                        }
                    }

                    groupMasterColIndex = 1;
                }

                sheet.Range[6, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Compliance Document List", identity.CompanyGroupId);
                //oRU.FreezePage(ref sheet, 1, 7);
                oRU.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_DocumentSet(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dslocal)
        {
            try
            {
                DataTable dtComplianceDocument = null;

                #region List data

                DataSet ComplianceDocumentList = GetComplianceDocumentSet();
                DataView dvMainBody = new DataView(ComplianceDocumentList.Tables[0])
                {
                    Sort = "ComplianceDocumentSetId, ComplianceDocumentId"
                };
                dtComplianceDocument = dvMainBody.ToTable(true, "ComplianceDocumentSetId", "Set", "ComplianceDocumentId", "Category", "Subcategory", "Code", "Name", "Type", "Importance", "Employment Stage", "Dependate Date", "Emp Type", "Global", "Documentation By", "Lead or Lag Days", "Optional or Mandatory", "Responsible Person Code", "Recurring", "Skilled");

                DataSet PositionList = GetComplianceDocumentPosition();
                DataView dvHead = new DataView(PositionList.Tables[0])
                {
                    Sort = "ComplianceDocumentId"
                };
                DataTable dtHead = dvHead.ToTable(false, "ComplianceDocumentId", "Position");

                var maxHeadCol = GetMaxPositionCounted(dtComplianceDocument, dtHead);

                if (dtComplianceDocument.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;
                var groupMasterColIndex = 1;

                _rowL++;

                for (int i = 0; i < dtComplianceDocument.Columns.Count; i++)
                {
                    if (dtComplianceDocument.Columns[i].ColumnName != "ComplianceDocumentSetId" && dtComplianceDocument.Columns[i].ColumnName != "ComplianceDocumentId")
                    {
                        _colIndex++;
                        oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtComplianceDocument.Columns[i].ColumnName);
                    }
                }

                var headCount = 0;
                for (int i = 0; i < maxHeadCol; i++)
                {
                    headCount++;
                    _colIndex++;
                    oRU.SetHeaderText(ref sheet, _rowL, _colIndex, "Position" + headCount);
                }

                shet2EndxlsCol = _colIndex;

                for (int q = 0; q < dtComplianceDocument.Rows.Count; q++)
                {
                    _rowL++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Set"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Category"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Subcategory"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Code"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Name"].ToString(), 40); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Type"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Importance"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Employment Stage"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Dependate Date"].ToString(), 24); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Emp Type"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Global"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Documentation By"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Lead or Lag Days"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Optional or Mandatory"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Responsible Person Code"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Recurring"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Skilled"].ToString()); groupMasterColIndex++;

                    string complianceDocumentId = dtComplianceDocument.Rows[q]["ComplianceDocumentId"].ToString();

                    DataTable dtHeadData = GetPositionData(complianceDocumentId, dtHead);
                    var rowCount = _rowL;
                    for (int a = 0; a < dtHeadData.Rows.Count; a++)
                    {
                        oRU.SetText(ref sheet, rowCount, groupMasterColIndex, dtHeadData.Rows[a]["Position"].ToString(), 26); groupMasterColIndex++;
                    }
                    if (maxHeadCol > dtHeadData.Rows.Count)
                    {
                        for (int c = 0; c < maxHeadCol - dtHeadData.Rows.Count; c++)
                        {
                            oRU.SetText(ref sheet, _rowL, groupMasterColIndex, ""); groupMasterColIndex++;
                        }
                    }

                    groupMasterColIndex = 1;
                }

                sheet.Range[6, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Compliance Document Set", identity.CompanyGroupId);
                //oRU.FreezePage(ref sheet, 1, 7);
                oRU.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_DocumentSetPlantWise(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, DataSet dslocal, string plantId)
        {
            try
            {
                DataTable dtComplianceDocument = null;

                #region List data

                DataSet ComplianceDocumentList = GetComplianceDocumentSetPlantWise(plantId);
                DataView dvMainBody = new DataView(ComplianceDocumentList.Tables[0])
                {
                    Sort = "ComplianceDocumentSetId, ComplianceDocumentId"
                };
                dtComplianceDocument = dvMainBody.ToTable(true, "ComplianceDocumentSetId", "Employee Type", "Set", "ComplianceDocumentId", "Category", "Subcategory", "Code", "Name", "Type", "Importance", "Employment Stage", "Dependate Date", "Emp Type", "Global", "Documentation By", "Lead or Lag Days", "Optional or Mandatory", "Responsible Person Code", "Recurring", "Skilled", "Company", "Plant");

                DataSet PositionList = GetComplianceDocumentPosition();
                DataView dvHead = new DataView(PositionList.Tables[0])
                {
                    Sort = "ComplianceDocumentId"
                };
                DataTable dtHead = dvHead.ToTable(false, "ComplianceDocumentId", "Position");

                var maxHeadCol = GetMaxPositionCounted(dtComplianceDocument, dtHead);

                if (dtComplianceDocument.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;
                var groupMasterColIndex = 1;

                var _col3 = 3;

                oRU.SetMasterHeaderText(ref sheet, _rowL, _col, "Company");
                sheet[oRU.GetColumnNameForXls(_col) + _rowL + ":" + oRU.GetColumnNameForXls(_col + 1) + _rowL].Merge();
                oRU.SetText(ref sheet, _rowL, _col + 2, dtComplianceDocument.Rows[0]["Company"].ToString());
                sheet[oRU.GetColumnNameForXls(_col3) + _rowL + ":" + oRU.GetColumnNameForXls(_col3 + 2) + _rowL].Merge();

                var _rowR = 5;
                var _colR = 6;
                var _col8 = 8;

                oRU.SetMasterHeaderText(ref sheet, _rowR, _colR, "Plant");
                sheet[oRU.GetColumnNameForXls(_colR) + _rowR + ":" + oRU.GetColumnNameForXls(_colR + 1) + _rowR].Merge();
                oRU.SetText(ref sheet, _rowR, _colR + 2, dtComplianceDocument.Rows[0]["Plant"].ToString());
                sheet[oRU.GetColumnNameForXls(_col8) + _rowR + ":" + oRU.GetColumnNameForXls(_col8 + 2) + _rowR].Merge();

                _rowL = 6;
                _rowL++;

                for (int i = 0; i < dtComplianceDocument.Columns.Count; i++)
                {
                    if (dtComplianceDocument.Columns[i].ColumnName != "ComplianceDocumentSetId" && dtComplianceDocument.Columns[i].ColumnName != "ComplianceDocumentId" && dtComplianceDocument.Columns[i].ColumnName != "Company" && dtComplianceDocument.Columns[i].ColumnName != "Plant")
                    {
                        _colIndex++;
                        oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtComplianceDocument.Columns[i].ColumnName);
                    }
                }

                var headCount = 0;
                for (int i = 0; i < maxHeadCol; i++)
                {
                    headCount++;
                    _colIndex++;
                    oRU.SetHeaderText(ref sheet, _rowL, _colIndex, "Position" + headCount);
                }

                shet2EndxlsCol = _colIndex;

                for (int q = 0; q < dtComplianceDocument.Rows.Count; q++)
                {
                    _rowL++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Employee Type"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Set"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Category"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Subcategory"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Code"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Name"].ToString(), 40); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Type"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Importance"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Employment Stage"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Dependate Date"].ToString(), 24); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Emp Type"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Global"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Documentation By"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Lead or Lag Days"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Optional or Mandatory"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Responsible Person Code"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Recurring"].ToString()); groupMasterColIndex++;
                    oRU.SetText(ref sheet, _rowL, groupMasterColIndex, dtComplianceDocument.Rows[q]["Skilled"].ToString()); groupMasterColIndex++;

                    string complianceDocumentId = dtComplianceDocument.Rows[q]["ComplianceDocumentId"].ToString();

                    DataTable dtHeadData = GetPositionData(complianceDocumentId, dtHead);
                    var rowCount = _rowL;
                    for (int a = 0; a < dtHeadData.Rows.Count; a++)
                    {
                        oRU.SetText(ref sheet, rowCount, groupMasterColIndex, dtHeadData.Rows[a]["Position"].ToString(), 26); groupMasterColIndex++;
                    }
                    if (maxHeadCol > dtHeadData.Rows.Count)
                    {
                        for (int c = 0; c < maxHeadCol - dtHeadData.Rows.Count; c++)
                        {
                            oRU.SetText(ref sheet, _rowL, groupMasterColIndex, ""); groupMasterColIndex++;
                        }
                    }

                    groupMasterColIndex = 1;
                }

                sheet.Range[7, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Compliance Document Set", identity.CompanyGroupId);
                //oRU.FreezePage(ref sheet, 1, 7);
                oRU.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook EmployeeDocument_Report(ExcelEngine excelEngine)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();

                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheet_EmployeeDocument(ref sheet1, oRU, "Candidate Document", "Candidate Document Report");

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_EmployeeDocument(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName)
        {
            DataTable dtDoc = null;

            #region List data

            var dtManin = GetEmployeeDocumentStatus();
            using (var dVDoc = new DataView(dtManin))
            {
                dVDoc.Sort = "Sequence";
                dtDoc = dVDoc.ToTable(true, "DocumentId", "DocumentName");
                if (dtDoc.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }
                using (var dVEmp = new DataView(dtManin))
                {
                    dVEmp.Sort = "EmployeeName";
                    var dtEmp = dVEmp.ToTable(true, "EmployeeId", "EmployeeName", "EmployeeCode");

                    #endregion List data

                    var _col = 1;
                    var _rowL = 5;
                    var _colIndex = 1;
                    var shet2EndxlsCol = _col;
                    _rowL++;
                    oRU.SetHeaderText(ref sheet, _rowL, _colIndex, "Sr"); _colIndex++;
                    oRU.SetHeaderText(ref sheet, _rowL, _colIndex, "Employee Name"); _colIndex++;
                    oRU.SetHeaderText(ref sheet, _rowL, _colIndex, "Employee Code");
                    for (int i = 0; i < dtDoc.Rows.Count; i++)
                    {
                        _colIndex++;
                        var docId = dtDoc.Rows[i]["DocumentId"].ToString();
                        //ob.Add(docId, _colIndex);
                        var docName = dtDoc.Rows[i]["DocumentName"].ToString();
                        oRU.SetHeaderText(ref sheet, _rowL, _colIndex, docName);
                    }
                    shet2EndxlsCol = _colIndex;
                    _rowL++;

                    for (int i = 0; i < dtEmp.Rows.Count; i++)
                    {
                        _colIndex = 1;
                        var EmployeeId = dtEmp.Rows[i]["EmployeeId"].ToString();

                        if (!string.IsNullOrEmpty(EmployeeId))
                        {
                            var FullName = dtEmp.Rows[i]["EmployeeName"].ToString();
                            var EmpCode = dtEmp.Rows[i]["EmployeeCode"].ToString();
                            oRU.SetText(ref sheet, _rowL, _colIndex, Convert.ToString(i)); _colIndex++;
                            oRU.SetText(ref sheet, _rowL, _colIndex, FullName); _colIndex++;
                            oRU.SetText(ref sheet, _rowL, _colIndex, EmpCode);

                            for (int c = 0; c < dtDoc.Rows.Count; c++)
                            {
                                _colIndex++;
                                var docId = dtDoc.Rows[c]["DocumentId"].ToString();

                                using (var dvEd = new DataView(dtManin))
                                {
                                    dvEd.RowFilter = "EmployeeId='" + EmployeeId + "' AND DocumentId='" + docId + "'";
                                    if (dvEd.Count > 0)
                                    {
                                        var FileId = dvEd[0]["FileId"].ToString();
                                        if (string.IsNullOrEmpty(FileId))
                                        {
                                            sheet.Range[_rowL, _colIndex].CellStyle.ColorIndex = ExcelKnownColors.Red;
                                            sheet.Range[_rowL, _colIndex].CellStyle.Font.Color = ExcelKnownColors.White;
                                            oRU.SetText(ref sheet, _rowL, _colIndex, "Not Done");
                                        }
                                        else
                                        {
                                            sheet.Range[_rowL, _colIndex].CellStyle.ColorIndex = ExcelKnownColors.Green;
                                            sheet.Range[_rowL, _colIndex].CellStyle.Font.Color = ExcelKnownColors.White;
                                            oRU.SetText(ref sheet, _rowL, _colIndex, "Done");
                                        }
                                    }
                                    else
                                    {
                                        oRU.SetText(ref sheet, _rowL, _colIndex, "NA");
                                    }
                                }
                            }
                            _rowL++;
                        }
                    }

                    //sheet.Range[(7), 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Name = SheetName;
                    sheet.UsedRange.WrapText = true;
                    sheet.UsedRange.CellStyle.Font.Size = 8;
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    oRU.CompanyHeader(ref sheet, shet2EndxlsCol, "Candidate Document Report", identity.CompanyId);
                    oRU.FreezePage(ref sheet, 1, 7);
                    oRU.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
                }
            }
        }

        private DataTable GetEmployeeDocumentStatus()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT PE.Id EmployeeId,PE.FullName EmployeeName,PE.EmployeeCode
                            ,CD.Id DocumentId,CD.UserName DocumentName,cd.Sequence,PD.FileId,PD.FileName
                             FROM ( SELECT * FROM HKP.ComplianceDocument WHERE Type='EmployeeRelated') CD
                            LEFT outer JOIN PreRecruitmentDocument PD  ON CD.Id=PD.ComplianceDocumentId
                            LEFT JOIN  (select * from PreRecruitmentEmployee where CompanyId='" + identity.CompanyId + "') PE on PE.Id= PD.PreRecruitmentEmployeeId";

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Compliance Document Report

        private DataSet GetDesignationMasterWithSalaryRule(string plantId)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT DM.Id
                                , DM.DesignationGroupId
                                , DG.Code AS [Designation Group Code]
                                , DG.UserName AS [Designation Group]
                                , DM.DesignationId
                                , D.Code AS [Designation Code]
                                , D.UserName AS Designation
                                , E.UserName AS [Employee Type]
								, S.SalaryRuleName AS [Salary Rule]
								, DL.LegalDesignationId
                                , L.Code AS [Legal Designation Code]
                                , L.UserName AS [Legal Designation]
								, PL.UserName AS Plant
                                FROM MST.DesignationMaster AS DM
                                LEFT JOIN MST.DesignationMasterLegalDesignation AS DL ON DL.DesignationMasterId=DM.Id
                                LEFT JOIN HKP.DesignationGroup AS DG ON DG.Id=DM.DesignationGroupId
                                LEFT JOIN HKP.Designation AS D ON D.Id=DM.DesignationId
                                LEFT JOIN HKP.EmployeeCategory AS E ON E.Id=DM.EmployeeCategoryId
                                LEFT JOIN HKP.LegalDesignation AS L ON L.Id=DL.LegalDesignationId
								LEFT JOIN [ORG].[PlantDesignationGroupSalaryRule] AS P ON P.DesignationGroupId = DG.id
								LEFT JOIN SalaryRuleMaster AS S ON S.SystemID = P.SalaryRuleMasterId
								LEFT JOIN [ORG].[Plant] AS PL ON PL.Id = P.PlantId
                                WHERE P.PlantId='" + plantId + @"' AND DM.Archive=0";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook DesignationMasterWithSalaryRule_Report(ExcelEngine excelEngine, string plantId)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            IWorksheet sheet2 = null;
            try
            {
                oRU = new ReportUtility();

                workbook = oRU.GetWorkbook(ref excelEngine, 2);
                sheet1 = workbook.Worksheets[0];
                sheet2 = workbook.Worksheets[1];
                CreateSheet_DesignationMasterWithSalaryRule1(ref sheet1, oRU, "Designation Master Report", "Designation Master Report", plantId);
                CreateSheet_DesignationMasterWithSalaryRule2(ref sheet2, oRU, "Designation Master List", "Designation Master Data", plantId);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_DesignationMasterWithSalaryRule1(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, string plantId)
        {
            try
            {
                DataTable dtDesignationMaster = null;

                #region List data

                DataSet DesignationMasterList = GetDesignationMasterWithSalaryRule(plantId);
                dtDesignationMaster = DesignationMasterList.Tables[0];

                DataView dvDesignationGroup = new DataView(DesignationMasterList.Tables[0]);
                DataTable dtDesignationGroup = dvDesignationGroup.ToTable(true, "Designation Group", "DesignationGroupId", "Designation Group Code");
                dvDesignationGroup.Sort = "Designation Group";

                DataView dvDesignation = null;
                DataTable dtDesignation = null;

                DataView dvLegalDesignation = null;
                DataTable dtLegalDesignation = null;

                if (dtDesignationMaster.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;
                var designationGroupCodeColIndex = 1;
                var designationGroupColIndex = 2;
                var designationCodeColIndex = 3;
                var designationColIndex = 4;
                var employeeTypeColIndex = 5;
                var salaryRuleColIndex = 6;
                var legalDesignationCodeColIndex = 7;
                var legalDesignationColIndex = 8;
                var _col3 = 3;

                oRU.SetMasterHeaderText(ref sheet, _rowL, _col, "Plant");
                sheet[oRU.GetColumnNameForXls(_col) + _rowL + ":" + oRU.GetColumnNameForXls(_col + 1) + _rowL].Merge();
                oRU.SetText(ref sheet, _rowL, _col + 2, dtDesignationMaster.Rows[0]["Plant"].ToString()); _rowL++;
                sheet[oRU.GetColumnNameForXls(_col3) + _rowL + ":" + oRU.GetColumnNameForXls(_col3 + 2) + _rowL].Merge();

                _rowL = 6;
                _rowL++;

                for (int i = 0; i < dtDesignationMaster.Columns.Count; i++)
                {
                    if (dtDesignationMaster.Columns[i].ColumnName != "Id" && dtDesignationMaster.Columns[i].ColumnName != "TotalRows" && dtDesignationMaster.Columns[i].ColumnName != "DesignationGroupId" && dtDesignationMaster.Columns[i].ColumnName != "DesignationId" && dtDesignationMaster.Columns[i].ColumnName != "LegalDesignationId" && dtDesignationMaster.Columns[i].ColumnName != "Plant")
                    {
                        _colIndex++;
                        oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtDesignationMaster.Columns[i].ColumnName);
                    }
                }
                shet2EndxlsCol = _colIndex;

                for (int p = 0; p < dtDesignationGroup.Rows.Count; p++)
                {
                    _rowL++;
                    string designationGroupId = dtDesignationGroup.Rows[p]["DesignationGroupId"].ToString();
                    dvDesignation = new DataView(dtDesignationMaster)
                    {
                        Sort = "Designation",
                        RowFilter = "DesignationGroupId='" + designationGroupId + "'"
                    };
                    dtDesignation = dvDesignation.ToTable(true, "Designation", "DesignationId", "Employee Type", "Salary Rule", "Designation Code");
                    var rowStartDesignationGroup = _rowL;
                    oRU.SetText(ref sheet, _rowL, designationGroupCodeColIndex, dtDesignationGroup.Rows[p]["Designation Group Code"].ToString(), 20);
                    oRU.SetText(ref sheet, _rowL, designationGroupColIndex, dtDesignationGroup.Rows[p]["Designation Group"].ToString(), 26);

                    for (int i = 0; i < dtDesignation.Rows.Count; i++)
                    {
                        string designationId = dtDesignation.Rows[i]["DesignationId"].ToString();
                        dvLegalDesignation = new DataView(dtDesignationMaster)
                        {
                            Sort = "Legal Designation",
                            RowFilter = "DesignationId='" + designationId + "' and DesignationGroupId='" + designationGroupId + "'"
                        };
                        dtLegalDesignation = dvLegalDesignation.ToTable(true, "Legal Designation", "LegalDesignationId", "Legal Designation Code");
                        var rowStartDesignation = _rowL;

                        oRU.SetText(ref sheet, _rowL, designationCodeColIndex, dtDesignation.Rows[i]["Designation Code"].ToString(), 20);
                        oRU.SetText(ref sheet, _rowL, designationColIndex, dtDesignation.Rows[i]["Designation"].ToString(), 26);
                        oRU.SetText(ref sheet, _rowL, employeeTypeColIndex, dtDesignation.Rows[i]["Employee Type"].ToString(), 15);
                        oRU.SetText(ref sheet, _rowL, salaryRuleColIndex, dtDesignation.Rows[i]["Salary Rule"].ToString(), 15);

                        for (int q = 0; q < dtLegalDesignation.Rows.Count; q++)
                        {
                            oRU.SetText(ref sheet, _rowL, legalDesignationCodeColIndex, dtLegalDesignation.Rows[q]["Legal Designation Code"].ToString(), 20);
                            oRU.SetText(ref sheet, _rowL, legalDesignationColIndex, dtLegalDesignation.Rows[q]["Legal Designation"].ToString(), 26);
                            _rowL++;
                            designationGroupCodeColIndex = 1;
                            designationGroupColIndex = 2;
                            designationCodeColIndex = 3;
                            designationColIndex = 4;
                            employeeTypeColIndex = 5;
                            salaryRuleColIndex = 6;
                            legalDesignationCodeColIndex = 7;
                            legalDesignationColIndex = 8;
                        }
                        //If Legal Designation number more than 0 apply for marge
                        if (dtLegalDesignation.Rows.Count > 0)
                        {
                            sheet[rowStartDesignation, designationCodeColIndex, _rowL - 1, designationCodeColIndex].Merge();
                            sheet[rowStartDesignation, employeeTypeColIndex, _rowL - 1, employeeTypeColIndex].Merge();
                            sheet[rowStartDesignation, salaryRuleColIndex, _rowL - 1, salaryRuleColIndex].Merge();
                            sheet[rowStartDesignation, designationColIndex, _rowL - 1, designationColIndex].Merge();
                        }
                    }//Designation
                    sheet[rowStartDesignationGroup, designationGroupCodeColIndex, _rowL - 1, designationGroupCodeColIndex].Merge();
                    sheet[rowStartDesignationGroup, designationGroupColIndex, _rowL - 1, designationGroupColIndex].Merge();
                    _rowL--;
                }//Designation Group

                sheet.Range[7, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = SheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Designation Master Report", identity.CompanyGroupId);
                //oRU.FreezePage(ref sheet, 1, 6);
                oRU.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheet_DesignationMasterWithSalaryRule2(ref IWorksheet sheet, ReportUtility oRU, string SheetHeader, string SheetName, string plantId)
        {
            DataTable dtDesignationMaster = null;

            #region List data

            DataSet DesignationMasterList = GetDesignationMasterWithSalaryRule(plantId);
            DataView dvDesignationMaster = new DataView(DesignationMasterList.Tables[0])
            {
                Sort = "Designation Code"
            };
            dtDesignationMaster = dvDesignationMaster.ToTable();
            if (dtDesignationMaster.Rows.Count == 0)
            {
                throw new Exception("No Data Found !!!");
            }

            #endregion List data

            var _col = 1;
            var _rowL = 5;
            var _colIndex = 0;
            var shet2EndxlsCol = _col;
            var _col3 = 3;

            oRU.SetMasterHeaderText(ref sheet, _rowL, _col, "Plant");
            sheet[oRU.GetColumnNameForXls(_col) + _rowL + ":" + oRU.GetColumnNameForXls(_col + 1) + _rowL].Merge();
            oRU.SetText(ref sheet, _rowL, _col + 2, dtDesignationMaster.Rows[0]["Plant"].ToString()); _rowL++;
            sheet[oRU.GetColumnNameForXls(_col3) + _rowL + ":" + oRU.GetColumnNameForXls(_col3 + 2) + _rowL].Merge();

            _rowL = 6;
            _rowL++;

            for (int i = 0; i < dtDesignationMaster.Columns.Count; i++)
            {
                if (dtDesignationMaster.Columns[i].ColumnName != "Id" && dtDesignationMaster.Columns[i].ColumnName != "TotalRows" && dtDesignationMaster.Columns[i].ColumnName != "DesignationGroupId" && dtDesignationMaster.Columns[i].ColumnName != "DesignationId" && dtDesignationMaster.Columns[i].ColumnName != "LegalDesignationId" && dtDesignationMaster.Columns[i].ColumnName != "Plant")
                {
                    _colIndex++;
                    oRU.SetHeaderText(ref sheet, _rowL, _colIndex, dtDesignationMaster.Columns[i].ColumnName);
                }
            }
            shet2EndxlsCol = _colIndex;

            for (int i = 0; i < dtDesignationMaster.Rows.Count; i++)
            {
                _rowL++;

                oRU.SetText(ref sheet, _rowL, 1, dtDesignationMaster.Rows[i]["Designation Group Code"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 2, dtDesignationMaster.Rows[i]["Designation Group"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 3, dtDesignationMaster.Rows[i]["Designation Code"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 4, dtDesignationMaster.Rows[i]["Designation"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 5, dtDesignationMaster.Rows[i]["Employee Type"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 6, dtDesignationMaster.Rows[i]["Salary Rule"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 7, dtDesignationMaster.Rows[i]["Legal Designation Code"].ToString(), 26);
                oRU.SetText(ref sheet, _rowL, 8, dtDesignationMaster.Rows[i]["Legal Designation"].ToString(), 26);
            }

            sheet.Range[7, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Name = SheetName;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            oRU.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Designation Master List", identity.CompanyGroupId);
            //oRU.FreezePage(ref sheet, 1, 7);
            oRU.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
        }
    }
}