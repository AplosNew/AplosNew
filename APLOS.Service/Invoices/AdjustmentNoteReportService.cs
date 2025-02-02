using Library.Data;
using Library.Data.Sql;
using Library.Model.Currencies;
using Library.Model.Enums;
using Library.Service.Accounts;
using Library.Service.Helpers;
using Library.Service.Organizations;
using Library.Service.Properties;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.Advances
{
    public class AdjustmentNoteReportService : IAdjustmentNoteReportService
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly ICompanyService _companyService;

        public AdjustmentNoteReportService(ISqlRepository sqlRepository, ICompanyService companyService)
        {
            _sqlRepository = sqlRepository;
            _companyService = companyService;
        }


        private bool GetPlantIsShowFCInWord(string plantId)
        {
            return bplib.clsWebLib.GetBoolData(_sqlRepository.GetDataCollection(@"SELECT IsShowFCInWord FROM ORG.Plant WHERE Id='" + plantId + "'")[0]["IsShowFCInWord"].ToString());
        }

        public void GetParallelCurrency(string companyId, out string companyCurrencyId, out string companyCurrencyCode)
        {
            var companyParallelCurrency = GetCompanyCurrencyId(companyId);
            if (null == companyParallelCurrency["CurrencyId"].ToString())
                throw new CustomException(ResourcesCore.CompanyParallelCurrencyNotConfigured);
            companyCurrencyId = companyParallelCurrency["CurrencyId"].ToString();
            companyCurrencyCode = companyParallelCurrency["CurrencyCode"].ToString();
        }
        private Dictionary<string, object> GetCompanyCurrencyId(string companyId)
        {
            var cmdText = @"select cpc.CurrencyId,C.Code CurrencyCode from SCS.CompanyParallelCurrency cpc
                            LEFT JOIN SCS.Currency C ON C.Id = CPC.CurrencyId where cpc.ParallelCurrencyType = '" + ParallelCurrencyType.CompanyCurrency.ToString() + "'";
            return _sqlRepository.GetData(cmdText);
        }


        //public IWorkbook GetDebitNoteReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType)
        //{
        //    var excelEngine = new ExcelEngine();
        //    var report = new ReportUtility();
        //    var workbook = report.GetWorkbook(ref excelEngine, 1);
        //    workbook.Version = ExcelVersion.Excel2013;
        //    var sheet = workbook.Worksheets[0];
        //    sheet.Name = "Voucher";
        //    var advanceDataList = GetAdvanceData(companyGroupId, companyId, plantId, voucherId, sourceType);
        //    var dvCustomer = new DataView(advanceDataList)
        //    {
        //        RowFilter = "Customer IS NOT NULL"
        //    };
        //    var dtCustomer = dvCustomer.ToTable(true, "Customer");
        //    var dvLocation = new DataView(advanceDataList)
        //    {
        //        RowFilter = "PartyPlant IS NOT NULL"
        //    };
        //    var dtLocation = dvLocation.ToTable(true, "PartyPlant");
        //    var dtGeneralVoucher = advanceDataList;
        //    var plCurrencyCode = dtGeneralVoucher.Rows[0]["CurrencyCode"].ToString();
        //    var _Currency = dtGeneralVoucher.Rows[0]["TrnCurrency"].ToString();
        //    var plCurrencyId = dtGeneralVoucher.Rows[0]["ParallelCurrencyId"].ToString();
        //    var _CurrencyId = dtGeneralVoucher.Rows[0]["CurrencyId"].ToString();

        //    var _col = 1;
        //    var _row = 5;
        //    var shet2EndxlsCol = _col;
        //    const int _col3 = 2;
        //    var sumdrcrCol = 0;

        //    // Set report Name
        //    reportFileName = Convert.ToDateTime(dtGeneralVoucher.Rows[0]["PostingDate"]).ToString("yyMMdd") + " " + dtGeneralVoucher.Rows[0]["VoucherNo"];

        //    report.SetMasterHeaderText(ref sheet, _row, _col, "Voucher No");
        //    report.SetMiddleAlignmentText(ref sheet, _row, _col3, dtGeneralVoucher.Rows[0]["VoucherNo"].ToString());
        //    _row++;

        //    report.SetMasterHeaderText(ref sheet, _row, _col, "Doc No");
        //    report.SetMiddleAlignmentText(ref sheet, _row, _col3, dtGeneralVoucher.Rows[0]["DocRefNo"].ToString());
        //    _row++;
        //    report.SetMasterHeaderText(ref sheet, _row, _col, "Fiscal Year");
        //    report.SetMiddleAlignmentText(ref sheet, _row, _col3, dtGeneralVoucher.Rows[0]["FiscalYearName"] + " (" + dtGeneralVoucher.Rows[0]["PeriodNo"] + ")");
        //    _row++;

        //    var party = "Party";
        //    var reportName = "Debit Note";
        //    report.SetMasterHeaderText(ref sheet, _row, _col, party);
        //    if (dtCustomer.Rows.Count > 0)
        //        report.SetMiddleAlignmentText(ref sheet, _row, _col3, dtCustomer.Rows[0]["Customer"].ToString());
        //    _row++;
        //    report.SetMasterHeaderText(ref sheet, _row, _col, "Narration");
        //    report.SetMiddleAlignmentText(ref sheet, _row, _col3, dtGeneralVoucher.Rows[0]["Narration"].ToString());
        //    sheet[report.GetColumnNameForXls(_col3) + _row + ":" + report.GetColumnNameForXls(_col3 + 3) + _row].Merge();
        //    _row++;
        //    var _rowR = 5;
        //    const int _colR = 3;
        //    const int _col8 = 4;

        //    report.SetMasterHeaderText(ref sheet, _rowR, _colR, "Voucher Date");
        //    report.SetMiddleAlignmentText(ref sheet, _rowR, _col8, dtGeneralVoucher.Rows[0]["VoucherDate"].ToString());
        //    sheet[report.GetColumnNameForXls(_col8) + _rowR + ":" + report.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();
        //    _rowR++;
        //    report.SetMasterHeaderText(ref sheet, _rowR, _colR, "Doc Date");
        //    report.SetMiddleAlignmentText(ref sheet, _rowR, _col8, dtGeneralVoucher.Rows[0]["DocDate"].ToString());
        //    sheet[report.GetColumnNameForXls(_col8) + _rowR + ":" + report.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

        //    _rowR++;
        //    report.SetMasterHeaderText(ref sheet, _rowR, _colR, "Posting Date");
        //    report.SetMiddleAlignmentText(ref sheet, _rowR, _col8, dtGeneralVoucher.Rows[0]["PostingDate"].ToString());
        //    sheet[report.GetColumnNameForXls(_col8) + _rowR + ":" + report.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();
        //    _rowR++;
        //    report.SetMasterHeaderText(ref sheet, _rowR, _colR, "Location");
        //    report.SetMiddleAlignmentText(ref sheet, _rowR, _col8, dtLocation.Rows[0]["PartyPlant"].ToString());
        //    sheet[report.GetColumnNameForXls(_col8) + _rowR + ":" + report.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();
        //    var row = 10;
        //    var _rowL = 11;
        //    var headreColIndex = 1;
        //    report.SetHeaderText(ref sheet, _rowL, headreColIndex, "GL", 32); headreColIndex++;
        //    report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Budget", 32); headreColIndex++;
        //    report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Activity", 24);
        //    sumdrcrCol = headreColIndex;
        //    headreColIndex++;
        //    if (plCurrencyId != _CurrencyId)
        //    {
        //        report.SetHeaderText(ref sheet, row, headreColIndex, _Currency, ExcelHAlign.HAlignCenter);
        //        sheet.Range[row, headreColIndex, row, headreColIndex + 1].Merge();

        //        report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", ExcelHAlign.HAlignRight);
        //        headreColIndex++;
        //        report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight);
        //        headreColIndex++;
        //    }
        //    double _Total_Amount = 0;

        //    report.SetHeaderText(ref sheet, row, headreColIndex, plCurrencyCode, ExcelHAlign.HAlignCenter);
        //    sheet.Range[row, headreColIndex, row, headreColIndex + 1].Merge();
        //    report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", ExcelHAlign.HAlignRight);
        //    headreColIndex++;
        //    report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight);

        //    shet2EndxlsCol = headreColIndex;
        //    double vAmount = 0;
        //    var Row_Total_Start = _rowL + 1;
        //    for (int n = 0; n < dtGeneralVoucher.Rows.Count; n++)
        //    {
        //        _rowL++;
        //        headreColIndex = 1;
        //        var AccountCodeId = dtGeneralVoucher.Rows[n]["GLGeneralInfoCode"].ToString();
        //        var _VoucherDetailId = dtGeneralVoucher.Rows[n]["VoucherDetailId"].ToString();
        //        var Bank = dtGeneralVoucher.Rows[n]["BankMain"].ToString();

        //        if (!string.IsNullOrEmpty(Bank))
        //        {
        //            report.SetText(ref sheet, _rowL, headreColIndex, Bank);
        //        }
        //        else
        //        {
        //            report.SetText(ref sheet, _rowL, headreColIndex, AccountCodeId + " - " + dtGeneralVoucher.Rows[n]["GL"]);
        //        }
        //        headreColIndex++;
        //        report.SetText(ref sheet, _rowL, headreColIndex, dtGeneralVoucher.Rows[n]["Budget"].ToString()); headreColIndex++;
        //        report.SetText(ref sheet, _rowL, headreColIndex, dtGeneralVoucher.Rows[n]["Activity"].ToString()); headreColIndex++;
        //        if (plCurrencyId != _CurrencyId)
        //        {
        //            report.SetText(ref sheet, _rowL, headreColIndex, Convert.ToDouble(dtGeneralVoucher.Rows[n]["TDrAmount"])); headreColIndex++;
        //            report.SetText(ref sheet, _rowL, headreColIndex, Convert.ToDouble(dtGeneralVoucher.Rows[n]["TCrAmount"])); headreColIndex++;
        //            vAmount += Convert.ToDouble(dtGeneralVoucher.Rows[n]["TCrAmount"].ToString());
        //        }

        //        report.SetText(ref sheet, _rowL, headreColIndex, Convert.ToDouble(dtGeneralVoucher.Rows[n]["DrAmount"].ToString()));
        //        headreColIndex++;
        //        report.SetText(ref sheet, _rowL, headreColIndex, Convert.ToDouble(dtGeneralVoucher.Rows[n]["CrAmount"].ToString()));
        //        _Total_Amount += Convert.ToDouble(dtGeneralVoucher.Rows[n]["CrAmount"].ToString());
        //    }

        //    #region sumCalc

        //    _rowL++;
        //    report.SetText(ref sheet, _rowL, sumdrcrCol, "Total :", true);
        //    sheet[_rowL, 1, _rowL, sumdrcrCol].Merge();

        //    if (plCurrencyId != _CurrencyId)
        //    {
        //        sumdrcrCol++;
        //        sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
        //        sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
        //        sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
        //        sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);

        //        sumdrcrCol++;
        //        sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
        //        sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
        //        sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
        //        sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
        //    }
        //    sumdrcrCol++;
        //    sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
        //    sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
        //    sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
        //    sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);

        //    sumdrcrCol++;
        //    sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
        //    sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
        //    sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
        //    sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);

        //    #endregion sumCalc

        //    shet2EndxlsCol = headreColIndex;
        //    sheet.Range[12, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
        //    sheet.Range[12, 1, _rowL, shet2EndxlsCol].BorderAround(ExcelLineStyle.Hair);

        //    #region InWord

        //    _rowL++;

        //    report.SetText(ref sheet, _rowL, _col, "In Word :", true);
        //    _col = 2;
        //    if (plCurrencyId != _CurrencyId)
        //    {
        //        var _amountValue = report.InWord(vAmount, _CurrencyId);
        //        sheet.Range[report.GetColumnNameForXls(_col) + _rowL].Text = _amountValue;
        //        sheet.Range[report.GetColumnNameForXls(_col) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
        //        sheet.Range[report.GetColumnNameForXls(_col) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //        sheet.Range[report.GetColumnNameForXls(_col) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
        //        sheet.Range[report.GetColumnNameForXls(_col) + _rowL].CellStyle.Font.Bold = true;
        //        _rowL++;
        //    }
        //    var _amount = report.InWord(_Total_Amount, plCurrencyId);
        //    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].Text = _amount;
        //    sheet.Range[report.GetColumnNameForXls(_col) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
        //    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
        //    sheet.Range[report.GetColumnNameForXls(_col) + _rowL].CellStyle.Font.Bold = true;

        //    #endregion InWord

        //    sheet.UsedRange.WrapText = true;
        //    sheet.UsedRange.CellStyle.Font.Size = 8;

        //    _rowL = _rowL + 4;
        //    report.SetSignatureText(ref sheet, _rowL - 1, 1, dtGeneralVoucher.Rows[0]["AddedBy"].ToString());
        //    sheet.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //    report.SetTextMiddle(ref sheet, _rowL, 1, "Prepared By", true);

        //    report.SetSignatureText(ref sheet, _rowL - 1, 3, dtGeneralVoucher.Rows[0]["PostedBy"].ToString());
        //    sheet.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //    report.SetTextMiddle(ref sheet, _rowL, 3, "Checked By", true);

        //    sheet.Range[_rowL, shet2EndxlsCol].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //    report.SetTextMiddle(ref sheet, _rowL, shet2EndxlsCol, "Authorized By", true);

        //    report.CompanyPlantHeader(ref sheet, shet2EndxlsCol, reportName, companyId, plantName, null);
        //    report.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
        //    return workbook;
        //}

        #region CreditNoteReport
        //New format  Credit Note  data
        private DataTable GetVendorInvoiceChargeData(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT V.Id, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, Replace(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.VoucherNo, UPPER(V.Narration) AS Narration, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId
                            , VDC.ToCurrencyRate, VD.DrAmount, VD.CrAmount, VDC.DrAmount as CompanyCurrencyDrAmount, VDC.CrAmount as CompanyCurrencyCrAmount, V.SourceType, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId
                            , GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,GL.AccountCode+' - '+ BM.AccountTitle AS BankMain, P.UserName AS Customer, PP.UserName AS PartyPlant, VD.RefCode AS Ref
                            , BUD.UserName AS Budget, ACT.UserName AS Activity
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [TRN].[AdjustmentNoteDetail] CID ON CID.Id=VD.AdjustmentNoteDetailId
                            LEFT JOIN [TRN].[AdjustmentNote] AS CI ON CI.Id=CID.AdjustmentNoteId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=CI.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.id=VD.BankMasterId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.SourceType='" + sourceType + "' AND V.Id = '" + voucherId + "' order by VD.DrAmount desc";
                     return _sqlRepository.GetDataTable(cmdText);
        }

        //Credit Note header data  NEW
        private Dictionary<string, object> GetVendorInvoiceHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
            ,AddedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.AddedBy END
            ,PostedBy=CASE WHEN UP.FullName<>'' THEN UP.FullName ELSE V.PostedBy END
            , UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
            , P.UserName  + CASE WHEN P.TINNO IS NOT NULL THEN  ' ( '+ P.TINNO + ' )' ELSE '' END AS Party
			, PP.UserName + CASE WHEN PP.GSTIN IS NOT NULL THEN  ' ( '+ PP.GSTIN + ' )' ELSE '' END AS VendorPlant
			, PartyType=bj.PartyType, BJ.CurrencyId, C.Code AS CurrencyCode,FY.FiscalYearName,E.UserName EntityName
            FROM [TRN].[AdjustmentNote] AS BJ
            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=BJ.VoucherId
            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
            LEFT JOIN [HKP].[Party] AS P ON P.Id=BJ.PartyId
            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=BJ.PartyPlantId
            LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
            LEFT JOIN SEC.[User] U ON U.UserId=V.AddedBy
            LEFT JOIN SEC.[User] UP ON UP.UserId=V.PostedBy
	        LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
            LEFT JOIN [ORG].[Entity] E ON E.Id=BJ.EntityId
            WHERE BJ.Archive=0 AND BJ.CompanyGroupId='" + companyGroupId + "' AND BJ.CompanyId='" + companyId + "' AND BJ.PlantId='" + plantId + "'  AND BJ.VoucherId='" + voucherId + "' AND BJ.SourceType='" + sourceType + "'" +
            "";
            return _sqlRepository.GetData(cmdText);
        }


        public IWorkbook GetCreditNoteReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            //    var advanceDataList = GetVendorInvoiceChargeData(companyGroupId, companyId, plantId, voucherId, sourceType);
            //    var dtGeneralVoucher = advanceDataList;

            var header = GetVendorInvoiceHeader(companyGroupId, companyId, plantId, voucherId, SourceType.CreditNote);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetVendorInvoiceChargeData(companyGroupId, companyId, plantId, voucherId, SourceType.CreditNote);

            var transcationCurrency = header["CurrencyId"].ToString();
            GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);


            var row = 5;
            var colLast = 1;
            int xlsCol = 1;
            int colGl = 0;

            //int colinrDebit = 0; 
           // int colinrCredit = 0;
            //int colusdDebit = 0; 
            //int colusdCradit = 0;

            int DebitCompCurCode = 0;
            int CreditCompCurCode = 0;

            int DebitTranCurCode = 0;
            int CreditTranCurCode = 0;

            int colVoucherNo = xlsCol; xlsCol++;
            int colVoucherNoValue = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherNo, "Voucher No");
            reportUtility.SetText(ref sheet, row, colVoucherNoValue, header["VoucherNo"].ToString());

            //reportUtility.SetMasterHeaderText(ref sheet, row, middleColumnCaption, "");
            //sheet[row, 3].ColumnWidth = 25;
            //reportUtility.SetText(ref sheet, row, middleColumnCaption, header[""].ToString());
            xlsCol++; //3
            int colCheckBy = xlsCol; //3
            xlsCol++; //4
            int colVoucherDate = xlsCol;
            xlsCol++; //5
            int colVoucherDateValue = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherDate, "Voucher Date");
            reportUtility.SetText(ref sheet, row, colVoucherDateValue, header["VoucherDate"].ToString());
            sheet[row, 4].ColumnWidth = 15;
            sheet[row, 5].ColumnWidth = 15;
            row++;

            int colPostingDate  = colVoucherNo; 
            int colPostingDateValue  = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colPostingDate, "Posting Date");
            reportUtility.SetText(ref sheet, row, colPostingDateValue, header["PostingDate"].ToString());

            int colDocDate = colVoucherDate;
            int colDocDateValue = colVoucherDateValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocDate, "DocDate");
            reportUtility.SetText(ref sheet, row, colDocDateValue, header["DocDate"].ToString());
            row++;

            int colParty = colVoucherNo;
            int colPartyValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colParty, "Party:");
            reportUtility.SetText(ref sheet, row, colPartyValue, header["Party"].ToString());

            int colPartyType = colVoucherDate;
            int colPartyTypeValue = colVoucherDateValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colPartyType, "Party Type");
            reportUtility.SetText(ref sheet, row, colPartyTypeValue, header["PartyType"].ToString());

            
            row++;

            int colDocRefNo = colVoucherNo;
            int colDocRefNoValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocRefNo, "Doc Ref");
            reportUtility.SetText(ref sheet, row, colDocRefNoValue, header["DocRefNo"].ToString());

            int colFiscalYearName = colVoucherDate;
            int colFiscalYearNameValue = colVoucherDateValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colFiscalYearName, "Fiscal Year ");
            reportUtility.SetText(ref sheet, row, colFiscalYearNameValue, header["FiscalYearName"].ToString());
            row++;

            int colStatus = colVoucherNo;
            int colStatusValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colStatus, "Status");
            reportUtility.SetText(ref sheet, row, colStatusValue, header["Status"].ToString());

            int colEntity = colVoucherDate;
            int colEntityValue = colVoucherDateValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colEntity, "Entity");
            reportUtility.SetText(ref sheet, row, colEntityValue, header["EntityName"].ToString());

            row++;

            

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            int colNarration  = colVoucherNo;
            int colNarrationValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colNarration, "Narration");
            reportUtility.SetText(ref sheet, row, colNarrationValue, header["Narration"].ToString());
           //sheet[reportUtility.GetColumnNameForXls(colDocRefNo) + row + ":" + reportUtility.GetColumnNameForXls(colNarrationValue) + row].Merge();
            //sheet[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet[row, colVoucherNoValue].ColumnWidth = 30;


            row++;  //10

            int colCompanyCurrencyCode = colVoucherDateValue + 1;
            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, colVoucherDate, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colVoucherDate, row, colVoucherDateValue].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, colVoucherDate, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, colVoucherDate, row, colVoucherDateValue].Merge();

                reportUtility.SetHeaderText(ref sheet, row, colCompanyCurrencyCode, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colCompanyCurrencyCode, row, colLast].Merge();
            }
            sheet[row, colCompanyCurrencyCode].ColumnWidth = 15;
            //sheet[row, 6].RowHeight = 15;
            sheet[row, colLast].ColumnWidth = 15;
            sheet.Range[row, colVoucherDate, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, colVoucherDate, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;

            colGl = colVoucherNo;
            reportUtility.SetHeaderText(ref sheet, row, colGl, "GL");  
            int colGLMarge = colVoucherNoValue + 1;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGLMarge) + row].Merge();

   
            DebitTranCurCode = colVoucherDate;
            CreditTranCurCode = colVoucherDateValue;

            DebitCompCurCode = colVoucherDate;
            CreditCompCurCode = colVoucherDateValue;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, DebitTranCurCode, "Debit", 13, ExcelHAlign.HAlignRight); DebitTranCurCode = colVoucherDate;  //xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, CreditTranCurCode, "Credit", 13, ExcelHAlign.HAlignRight); CreditTranCurCode = colVoucherDateValue;  //xlsCol++;

                colVoucherDateValue++;
                DebitCompCurCode = colVoucherDateValue;
                reportUtility.SetHeaderText(ref sheet, row, DebitCompCurCode, "Debit", 13, ExcelHAlign.HAlignRight); DebitCompCurCode = colVoucherDateValue; //xlsCol++;

                colVoucherDateValue++;
                CreditCompCurCode = colVoucherDateValue;
                reportUtility.SetHeaderText(ref sheet, row, CreditCompCurCode, "Credit", 13, ExcelHAlign.HAlignRight); CreditCompCurCode = colVoucherDateValue;
                colLast = colVoucherDateValue;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, colGl, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            else
            {

            
                DebitCompCurCode = colVoucherDate;
                reportUtility.SetHeaderText(ref sheet, row, DebitCompCurCode, "Debit", 13, ExcelHAlign.HAlignRight); 

                CreditCompCurCode = colVoucherDateValue;
                reportUtility.SetHeaderText(ref sheet, row, CreditCompCurCode, "Credit", 13, ExcelHAlign.HAlignRight);
                colLast = colVoucherDateValue;

                //sheet.Range[row, 4, row, colLast].BorderAround(ExcelLineStyle.Thin);
                //sheet.Range[row, 4, row, colLast].BorderInside(ExcelLineStyle.Thin);

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, 4, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }


            int formulaStartRow = 0;
            int formulaEndRow = 0;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++; //?? 12

                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGLMarge) + row].Merge();

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, DebitTranCurCode, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, CreditTranCurCode, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));

                        reportUtility.SetText(ref sheet, row, DebitCompCurCode, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, CreditCompCurCode, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                       
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {

                        reportUtility.SetText(ref sheet, row, DebitCompCurCode, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, CreditCompCurCode, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);

                    //glName = string.Empty;

                    row++;
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, colGLMarge, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                    //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                    //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                    //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet.Range[row, DebitTranCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(DebitTranCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(DebitTranCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, DebitTranCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, DebitTranCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, DebitTranCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, DebitTranCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, DebitTranCurCode].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, CreditTranCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(CreditTranCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(CreditTranCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, CreditTranCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, CreditTranCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, CreditTranCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, CreditTranCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, CreditTranCurCode].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, DebitCompCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(DebitCompCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(DebitCompCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, DebitCompCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, DebitCompCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, DebitCompCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, DebitCompCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, DebitCompCurCode].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, CreditCompCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(CreditCompCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(CreditCompCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, CreditCompCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, CreditCompCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, CreditCompCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, CreditCompCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, CreditCompCurCode].BorderAround(ExcelLineStyle.Hair);

            
                }
                else
                {
                    sheet.Range[row, DebitCompCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(DebitCompCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(DebitCompCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, DebitCompCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, DebitCompCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, DebitCompCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, DebitCompCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, DebitCompCurCode].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, CreditCompCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(CreditCompCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(CreditCompCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, CreditCompCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, CreditCompCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, CreditCompCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, CreditCompCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, CreditCompCurCode].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[row, DebitCompCurCode, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, DebitCompCurCode, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, colGl, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && GetPlantIsShowFCInWord(plantId))
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].CellStyle.Font.Bold = true;
                    row++;
                }

                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].CellStyle.Font.Bold = true;

                //sheet.UsedRange.AutofitColumns();
                //sheet[1, 2].ColumnWidth = 60;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, colGl, header["AddedBy"].ToString());
                sheet.Range[row, colGl].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colGl, "Prepared By", true);
                sheet[row, colGl].ColumnWidth = 25;

                reportUtility.SetSignatureText(ref sheet, row - 1, colCheckBy, header["PostedBy"].ToString());
                sheet.Range[row, colCheckBy].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colCheckBy, "Checked By", true);
                sheet[row, colCheckBy].ColumnWidth = 25;

                sheet.Range[row, colVoucherDateValue].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colVoucherDateValue, "Authorized By", true);
                
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Credit Note", companyId,plantId, plantName, null);
                //reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

                //    //else
                //    //{
                //    //    sheet.UsedRange.WrapText = true;
                //    //    sheet.UsedRange.CellStyle.Font.Size = 8;
                //    //    reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                //    //    reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Credit Note", companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }

        public IWorkbook GetDebitNoteReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            //    var advanceDataList = GetVendorInvoiceChargeData(companyGroupId, companyId, plantId, voucherId, sourceType);
            //    var dtGeneralVoucher = advanceDataList;

            var header = GetVendorInvoiceHeader(companyGroupId, companyId, plantId, voucherId, SourceType.DebitNote);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetVendorInvoiceChargeData(companyGroupId, companyId, plantId, voucherId, SourceType.DebitNote);

            var transcationCurrency = header["CurrencyId"].ToString();
            GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);


            var row = 5;
            var colLast = 1;
            int xlsCol = 1;
            int colGl = 0;

            //int colinrDebit = 0; 
            // int colinrCredit = 0;
            //int colusdDebit = 0; 
            //int colusdCradit = 0;

            int DebitCompCurCode = 0;
            int CreditCompCurCode = 0;

            int DebitTranCurCode = 0;
            int CreditTranCurCode = 0;

            int colVoucherNo = xlsCol; xlsCol++;
            int colVoucherNoValue = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherNo, "Voucher No");
            reportUtility.SetText(ref sheet, row, colVoucherNoValue, header["VoucherNo"].ToString());

            //reportUtility.SetMasterHeaderText(ref sheet, row, middleColumnCaption, "");
            //sheet[row, 3].ColumnWidth = 25;
            //reportUtility.SetText(ref sheet, row, middleColumnCaption, header[""].ToString());
            xlsCol++; //3
            int colCheckBy = xlsCol; //3
            xlsCol++; //4
            int colVoucherDate = xlsCol;
            xlsCol++; //5
            int colVoucherDateValue = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherDate, "Voucher Date");
            reportUtility.SetText(ref sheet, row, colVoucherDateValue, header["VoucherDate"].ToString());
            sheet[row, 4].ColumnWidth = 15;
            sheet[row, 5].ColumnWidth = 15;
            row++;

            int colPostingDate = colVoucherNo;
            int colPostingDateValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colPostingDate, "Posting Date");
            reportUtility.SetText(ref sheet, row, colPostingDateValue, header["PostingDate"].ToString());

            int colDocDate = colVoucherDate;
            int colDocDateValue = colVoucherDateValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocDate, "DocDate");
            reportUtility.SetText(ref sheet, row, colDocDateValue, header["DocDate"].ToString());
            row++;

            
            int colParty = colVoucherNo;
            int colPartyValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colParty, "Party:");
            reportUtility.SetText(ref sheet, row, colPartyValue, header["Party"].ToString());

            int colPatyType = colVoucherDate;
            int colPartyTypeValue = colVoucherDateValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colPatyType, "PartyType");
            reportUtility.SetText(ref sheet, row, colPartyTypeValue, header["PartyType"].ToString());

            row++;

            int colFiscalYearName = colVoucherNo;
            int colFiscalYearNameValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colFiscalYearName, "Fiscal Year ");
            reportUtility.SetText(ref sheet, row, colFiscalYearNameValue, header["FiscalYearName"].ToString());

            int colStatus = colVoucherDate;
            int colStatusValue = colVoucherDateValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colStatus, "Status");
            reportUtility.SetText(ref sheet, row, colStatusValue, header["Status"].ToString());

            row++;
            int colDocRefNo = colVoucherNo;
            int colDocRefNoValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocRefNo, "Doc Ref");
            reportUtility.SetText(ref sheet, row, colDocRefNoValue, header["DocRefNo"].ToString());
           

            int colEntity = colVoucherDate;
            int colEntityValue = colVoucherDateValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colEntity, "Entity");
            reportUtility.SetText(ref sheet, row, colEntityValue, header["EntityName"].ToString());

            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            int colNarration = colVoucherNo;
            int colNarrationValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colNarration, "Narration");
            reportUtility.SetText(ref sheet, row, colNarrationValue, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet[row, colVoucherNoValue].ColumnWidth = 30;


            row++;  //10

            int colCompanyCurrencyCode = colVoucherDateValue + 1;
            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, colVoucherDate, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colVoucherDate, row, colVoucherDateValue].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, colVoucherDate, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, colVoucherDate, row, colVoucherDateValue].Merge();

                reportUtility.SetHeaderText(ref sheet, row, colCompanyCurrencyCode, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colCompanyCurrencyCode, row, colLast].Merge();
            }
            sheet[row, colCompanyCurrencyCode].ColumnWidth = 15;
           
            sheet[row, colLast].ColumnWidth = 15;
            sheet.Range[row, colVoucherDate, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, colVoucherDate, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;

            colGl = colVoucherNo;
            reportUtility.SetHeaderText(ref sheet, row, colGl, "GL");
            int colGLMarge = colVoucherNoValue + 1;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGLMarge) + row].Merge();


            DebitTranCurCode = colVoucherDate;
            CreditTranCurCode = colVoucherDateValue;

            DebitCompCurCode = colVoucherDate;
            CreditCompCurCode = colVoucherDateValue;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, DebitTranCurCode, "Debit", 13, ExcelHAlign.HAlignRight); DebitTranCurCode = colVoucherDate;  //xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, CreditTranCurCode, "Credit", 13, ExcelHAlign.HAlignRight); CreditTranCurCode = colVoucherDateValue;  //xlsCol++;

                colVoucherDateValue++;
                DebitCompCurCode = colVoucherDateValue;
                reportUtility.SetHeaderText(ref sheet, row, DebitCompCurCode, "Debit", 13, ExcelHAlign.HAlignRight); DebitCompCurCode = colVoucherDateValue; //xlsCol++;

                colVoucherDateValue++;
                CreditCompCurCode = colVoucherDateValue;
                reportUtility.SetHeaderText(ref sheet, row, CreditCompCurCode, "Credit", 13, ExcelHAlign.HAlignRight); CreditCompCurCode = colVoucherDateValue;
                colLast = colVoucherDateValue;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, colGl, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            else
            {


                DebitCompCurCode = colVoucherDate;
                reportUtility.SetHeaderText(ref sheet, row, DebitCompCurCode, "Debit", 13, ExcelHAlign.HAlignRight);

                CreditCompCurCode = colVoucherDateValue;
                reportUtility.SetHeaderText(ref sheet, row, CreditCompCurCode, "Credit", 13, ExcelHAlign.HAlignRight);
                colLast = colVoucherDateValue;

                //sheet.Range[row, 4, row, colLast].BorderAround(ExcelLineStyle.Thin);
                //sheet.Range[row, 4, row, colLast].BorderInside(ExcelLineStyle.Thin);

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, 4, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }


            int formulaStartRow = 0;
            int formulaEndRow = 0;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++; //?? 12

                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGLMarge) + row].Merge();

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, DebitTranCurCode, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, CreditTranCurCode, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));

                        reportUtility.SetText(ref sheet, row, DebitCompCurCode, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, CreditCompCurCode, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));

                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {

                        reportUtility.SetText(ref sheet, row, DebitCompCurCode, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, CreditCompCurCode, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);

                    //glName = string.Empty;

                    row++;
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, colGLMarge, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                    //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                    //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                    //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet.Range[row, DebitTranCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(DebitTranCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(DebitTranCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, DebitTranCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, DebitTranCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, DebitTranCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, DebitTranCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, DebitTranCurCode].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, CreditTranCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(CreditTranCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(CreditTranCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, CreditTranCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, CreditTranCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, CreditTranCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, CreditTranCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, CreditTranCurCode].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, DebitCompCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(DebitCompCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(DebitCompCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, DebitCompCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, DebitCompCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, DebitCompCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, DebitCompCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, DebitCompCurCode].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, CreditCompCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(CreditCompCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(CreditCompCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, CreditCompCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, CreditCompCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, CreditCompCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, CreditCompCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, CreditCompCurCode].BorderAround(ExcelLineStyle.Hair);


                }
                else
                {
                    sheet.Range[row, DebitCompCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(DebitCompCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(DebitCompCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, DebitCompCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, DebitCompCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, DebitCompCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, DebitCompCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, DebitCompCurCode].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, CreditCompCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(CreditCompCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(CreditCompCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, CreditCompCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, CreditCompCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, CreditCompCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, CreditCompCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, CreditCompCurCode].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[row, DebitCompCurCode, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, DebitCompCurCode, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, colGl, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && GetPlantIsShowFCInWord(plantId))
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].CellStyle.Font.Bold = true;
                    row++;
                }

                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].CellStyle.Font.Bold = true;

                //sheet.UsedRange.AutofitColumns();
                //sheet[1, 2].ColumnWidth = 60;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, colGl, header["AddedBy"].ToString());
                sheet.Range[row, colGl].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colGl, "Prepared By", true);

                sheet[row, colGl].ColumnWidth = 25;
                sheet[row, colGl+1].ColumnWidth = 15;


                reportUtility.SetSignatureText(ref sheet, row - 1, colCheckBy, header["PostedBy"].ToString());
                sheet.Range[row, colCheckBy].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colCheckBy, "Checked By", true);
                sheet[row, colCheckBy].ColumnWidth = 25;

                sheet.Range[row, colVoucherDateValue].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colVoucherDateValue, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Debit Note", companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

                //    //else
                //    //{
                //    //    sheet.UsedRange.WrapText = true;
                //    //    sheet.UsedRange.CellStyle.Font.Size = 8;
                //    //    reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                //    //    reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Debit Note", companyId,plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }

        #endregion GetCreditNoteReport

        #region Credit Note Set-Off & debit Note set-ff report
        //New format  Credit Note  data
        private DataTable GetCreditNoteSetOffData(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT V.Id, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, Replace(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.VoucherNo, UPPER(V.Narration) AS Narration, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId
                            , VDC.ToCurrencyRate, VD.DrAmount, VD.CrAmount, VDC.DrAmount as CompanyCurrencyDrAmount, VDC.CrAmount as CompanyCurrencyCrAmount, V.SourceType, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId
                            , GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,GL.AccountCode+' - '+ BM.AccountTitle AS BankMain, P.UserName AS Customer, PP.UserName AS PartyPlant, VD.RefCode AS Ref
                            , BUD.UserName AS Budget, Activity=case when VD.BankMasterId<>'' then BM.AccountTitle else  ACT.UserName end 
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [TRN].[AdjustmentNoteDetail] CID ON CID.Id=VD.AdjustmentNoteDetailId
                            LEFT JOIN [TRN].[AdjustmentNote] AS CI ON CI.Id=CID.AdjustmentNoteId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=CI.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.id=VD.BankMasterId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.SourceType='" + sourceType + "' AND V.Id = '" + voucherId + "'";
            return _sqlRepository.GetDataTable(cmdText);
        }

        //Credit Note header data  NEW
        private Dictionary<string, object> GetCreditNoteSetOffHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
            ,AddedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.AddedBy END
            ,PostedBy=CASE WHEN U.FullName<>'' THEN isnull(UP.FullName,'') ELSE V.PostedBy END
            , UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
            , P.UserName AS Party, PP.UserName AS VendorPlant, BJ.CurrencyId, C.Code AS CurrencyCode
	        ,FY.FiscalYearName
            FROM [TRN].[InvoiceWriteOff] AS BJ
            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=BJ.VoucherId
            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
            LEFT JOIN [HKP].[Party] AS P ON P.Id=BJ.PartyId
            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=BJ.PartyPlantId
            LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
            LEFT JOIN SEC.[User] U ON U.UserId=V.AddedBy
            LEFT JOIN SEC.[User] UP ON UP.UserId=V.PostedBy
	        LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
            WHERE BJ.Archive=0 AND BJ.CompanyGroupId='" + companyGroupId + "' AND BJ.CompanyId='" + companyId + "' AND BJ.PlantId='" + plantId + "'  AND BJ.VoucherId='" + voucherId + "' AND BJ.SourceType='" + sourceType + "'" +
            "";
            return _sqlRepository.GetData(cmdText);
        }


        public IWorkbook CreditNoteSetOffReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "CreditNoteSet-Off";

            //    var advanceDataList = GetVendorInvoiceChargeData(companyGroupId, companyId, plantId, voucherId, sourceType);
            //    var dtGeneralVoucher = advanceDataList;

            var header = GetCreditNoteSetOffHeader(companyGroupId, companyId, plantId, voucherId, SourceType.CreditNoteSetOff);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetCreditNoteSetOffData(companyGroupId, companyId, plantId, voucherId, SourceType.CreditNoteSetOff);

            var transcationCurrency = header["CurrencyId"].ToString();
            GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);


            var row = 5;
            var colLast = 1;
            int xlsCol = 1;
            int colGl = 0;

            //int colinrDebit = 0; 
            // int colinrCredit = 0;
            //int colusdDebit = 0; 
            //int colusdCradit = 0;

            int DebitCompCurCode = 0;
            int CreditCompCurCode = 0;

            int DebitTranCurCode = 0;
            int CreditTranCurCode = 0;

            int colVoucherNo = xlsCol; xlsCol++;
            int colVoucherNoValue = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherNo, "Voucher No");
            reportUtility.SetText(ref sheet, row, colVoucherNoValue, header["VoucherNo"].ToString());

            //reportUtility.SetMasterHeaderText(ref sheet, row, middleColumnCaption, "");
            //sheet[row, 3].ColumnWidth = 25;
            //reportUtility.SetText(ref sheet, row, middleColumnCaption, header[""].ToString());
            xlsCol++; //3
            int colCheckBy = xlsCol; //3
            xlsCol++; //4
            int colVoucherDate = xlsCol;
            xlsCol++; //5
            int colVoucherDateValue = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherDate, "Voucher Date");
            reportUtility.SetText(ref sheet, row, colVoucherDateValue, header["VoucherDate"].ToString());
            sheet[row, 4].ColumnWidth = 15;
            sheet[row, 5].ColumnWidth = 15;
            row++;

            int colPostingDate = colVoucherNo;
            int colPostingDateValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colPostingDate, "Posting Date");
            reportUtility.SetText(ref sheet, row, colPostingDateValue, header["PostingDate"].ToString());

            int colDocDate = colVoucherDate;
            int colDocDateValue = colVoucherDateValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocDate, "DocDate");
            reportUtility.SetText(ref sheet, row, colDocDateValue, header["DocDate"].ToString());
            row++;

            int colParty = colVoucherNo;
            int colPartyValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colParty, "Party:");
            reportUtility.SetText(ref sheet, row, colPartyValue, header["Party"].ToString());

            int colDocRefNo = colVoucherDate;
            int colDocRefNoValue = colVoucherDateValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocRefNo, "Doc Ref");
            reportUtility.SetText(ref sheet, row, colDocRefNoValue, header["DocRefNo"].ToString());
            row++;

            int colFiscalYearName = colVoucherNo;
            int colFiscalYearNameValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colFiscalYearName, "Fiscal Year ");
            reportUtility.SetText(ref sheet, row, colFiscalYearNameValue, header["FiscalYearName"].ToString());

            int colStatus = colDocRefNo;
            int colStatusValue = colDocRefNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colStatus, "Status");
            reportUtility.SetText(ref sheet, row, colStatusValue, header["Status"].ToString());

            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            int colNarration = colVoucherNo;
            int colNarrationValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colNarration, "Narration");
            reportUtility.SetText(ref sheet, row, colNarrationValue, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet[row, colVoucherNoValue].ColumnWidth = 30;


            row++;  //10

            int colCompanyCurrencyCode = colVoucherDateValue + 1;
            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, colVoucherDate, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colVoucherDate, row, colVoucherDateValue].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, colVoucherDate, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, colVoucherDate, row, colVoucherDateValue].Merge();

                reportUtility.SetHeaderText(ref sheet, row, colCompanyCurrencyCode, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colCompanyCurrencyCode, row, colLast].Merge();
            }
            sheet[row, colCompanyCurrencyCode].ColumnWidth = 15;
            //sheet[row, 6].RowHeight = 15;
            sheet[row, colLast].ColumnWidth = 15;
            sheet.Range[row, colVoucherDate, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, colVoucherDate, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;

            colGl = colVoucherNo;
            reportUtility.SetHeaderText(ref sheet, row, colGl, "GL");
            int colGLMarge = colVoucherNoValue + 1;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGLMarge) + row].Merge();


            DebitTranCurCode = colVoucherDate;
            CreditTranCurCode = colVoucherDateValue;

            DebitCompCurCode = colVoucherDate;
            CreditCompCurCode = colVoucherDateValue;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, DebitTranCurCode, "Debit", 13, ExcelHAlign.HAlignRight); DebitTranCurCode = colVoucherDate;  //xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, CreditTranCurCode, "Credit", 13, ExcelHAlign.HAlignRight); CreditTranCurCode = colVoucherDateValue;  //xlsCol++;

                colVoucherDateValue++;
                DebitCompCurCode = colVoucherDateValue;
                reportUtility.SetHeaderText(ref sheet, row, DebitCompCurCode, "Debit", 13, ExcelHAlign.HAlignRight); DebitCompCurCode = colVoucherDateValue; //xlsCol++;

                colVoucherDateValue++;
                CreditCompCurCode = colVoucherDateValue;
                reportUtility.SetHeaderText(ref sheet, row, CreditCompCurCode, "Credit", 13, ExcelHAlign.HAlignRight); CreditCompCurCode = colVoucherDateValue;
                colLast = colVoucherDateValue;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, colGl, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            else
            {


                DebitCompCurCode = colVoucherDate;
                reportUtility.SetHeaderText(ref sheet, row, DebitCompCurCode, "Debit", 13, ExcelHAlign.HAlignRight);

                CreditCompCurCode = colVoucherDateValue;
                reportUtility.SetHeaderText(ref sheet, row, CreditCompCurCode, "Credit", 13, ExcelHAlign.HAlignRight);
                colLast = colVoucherDateValue;

                //sheet.Range[row, 4, row, colLast].BorderAround(ExcelLineStyle.Thin);
                //sheet.Range[row, 4, row, colLast].BorderInside(ExcelLineStyle.Thin);

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, 4, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }


            int formulaStartRow = 0;
            int formulaEndRow = 0;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++; //?? 12

                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGLMarge) + row].Merge();

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, DebitTranCurCode, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, CreditTranCurCode, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));

                        reportUtility.SetText(ref sheet, row, DebitCompCurCode, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, CreditCompCurCode, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));

                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {

                        reportUtility.SetText(ref sheet, row, DebitCompCurCode, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, CreditCompCurCode, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);

                    //glName = string.Empty;

                    row++;
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, colGLMarge, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                    //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                    //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                    //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet.Range[row, DebitTranCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(DebitTranCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(DebitTranCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, DebitTranCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, DebitTranCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, DebitTranCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, DebitTranCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, DebitTranCurCode].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, CreditTranCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(CreditTranCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(CreditTranCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, CreditTranCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, CreditTranCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, CreditTranCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, CreditTranCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, CreditTranCurCode].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, DebitCompCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(DebitCompCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(DebitCompCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, DebitCompCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, DebitCompCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, DebitCompCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, DebitCompCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, DebitCompCurCode].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, CreditCompCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(CreditCompCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(CreditCompCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, CreditCompCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, CreditCompCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, CreditCompCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, CreditCompCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, CreditCompCurCode].BorderAround(ExcelLineStyle.Hair);


                }
                else
                {
                    sheet.Range[row, DebitCompCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(DebitCompCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(DebitCompCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, DebitCompCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, DebitCompCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, DebitCompCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, DebitCompCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, DebitCompCurCode].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, CreditCompCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(CreditCompCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(CreditCompCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, CreditCompCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, CreditCompCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, CreditCompCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, CreditCompCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, CreditCompCurCode].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[row, DebitCompCurCode, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, DebitCompCurCode, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, colGl, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && GetPlantIsShowFCInWord(plantId))
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].CellStyle.Font.Bold = true;
                    row++;
                }

                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].CellStyle.Font.Bold = true;

                //sheet.UsedRange.AutofitColumns();
                //sheet[1, 2].ColumnWidth = 60;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, colGl, header["AddedBy"].ToString());
                sheet.Range[row, colGl].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colGl, "Prepared By", true);
                sheet[row, colGl].ColumnWidth = 25;

                reportUtility.SetSignatureText(ref sheet, row - 1, colCheckBy, header["PostedBy"].ToString());
                sheet.Range[row, colCheckBy].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colCheckBy, "Checked By", true);
                sheet[row, colCheckBy].ColumnWidth = 25;

                sheet.Range[row, colVoucherDateValue].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colVoucherDateValue, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Credit Note Set-Off", companyId,plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

                //    //else
                //    //{
                //    //    sheet.UsedRange.WrapText = true;
                //    //    sheet.UsedRange.CellStyle.Font.Size = 8;
                //    //    reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                //    //    reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Credit Note Set Off", companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }

        public IWorkbook DebitNoteSetOffReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            //    var advanceDataList = GetVendorInvoiceChargeData(companyGroupId, companyId, plantId, voucherId, sourceType);
            //    var dtGeneralVoucher = advanceDataList;

            var header = GetCreditNoteSetOffHeader(companyGroupId, companyId, plantId, voucherId, SourceType.DebitNoteSetOff);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetCreditNoteSetOffData(companyGroupId, companyId, plantId, voucherId, SourceType.DebitNoteSetOff);

            var transcationCurrency = header["CurrencyId"].ToString();
            GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);


            var row = 5;
            var colLast = 1;
            int xlsCol = 1;
            int colGl = 0;

            //int colinrDebit = 0; 
            // int colinrCredit = 0;
            //int colusdDebit = 0; 
            //int colusdCradit = 0;

            int DebitCompCurCode = 0;
            int CreditCompCurCode = 0;

            int DebitTranCurCode = 0;
            int CreditTranCurCode = 0;

            int colVoucherNo = xlsCol; xlsCol++;
            int colVoucherNoValue = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherNo, "Voucher No");
            reportUtility.SetText(ref sheet, row, colVoucherNoValue, header["VoucherNo"].ToString());

            //reportUtility.SetMasterHeaderText(ref sheet, row, middleColumnCaption, "");
            //sheet[row, 3].ColumnWidth = 25;
            //reportUtility.SetText(ref sheet, row, middleColumnCaption, header[""].ToString());
            xlsCol++; //3
            int colCheckBy = xlsCol; //3
            xlsCol++; //4
            int colVoucherDate = xlsCol;
            xlsCol++; //5
            int colVoucherDateValue = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherDate, "Voucher Date");
            reportUtility.SetText(ref sheet, row, colVoucherDateValue, header["VoucherDate"].ToString());
            sheet[row, 4].ColumnWidth = 15;
            sheet[row, 5].ColumnWidth = 15;
            row++;

            int colPostingDate = colVoucherNo;
            int colPostingDateValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colPostingDate, "Posting Date");
            reportUtility.SetText(ref sheet, row, colPostingDateValue, header["PostingDate"].ToString());

            int colDocDate = colVoucherDate;
            int colDocDateValue = colVoucherDateValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocDate, "DocDate");
            reportUtility.SetText(ref sheet, row, colDocDateValue, header["DocDate"].ToString());
            row++;

            int colParty = colVoucherNo;
            int colPartyValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colParty, "Party:");
            reportUtility.SetText(ref sheet, row, colPartyValue, header["Party"].ToString());

            int colDocRefNo = colVoucherDate;
            int colDocRefNoValue = colVoucherDateValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocRefNo, "Doc Ref");
            reportUtility.SetText(ref sheet, row, colDocRefNoValue, header["DocRefNo"].ToString());
            row++;

            int colFiscalYearName = colVoucherNo;
            int colFiscalYearNameValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colFiscalYearName, "Fiscal Year ");
            reportUtility.SetText(ref sheet, row, colFiscalYearNameValue, header["FiscalYearName"].ToString());

            int colStatus = colDocRefNo;
            int colStatusValue = colDocRefNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colStatus, "Status");
            reportUtility.SetText(ref sheet, row, colStatusValue, header["Status"].ToString());

            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            int colNarration = colVoucherNo;
            int colNarrationValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colNarration, "Narration");
            reportUtility.SetText(ref sheet, row, colNarrationValue, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet[row, colVoucherNoValue].ColumnWidth = 30;


            row++;  //10

            int colCompanyCurrencyCode = colVoucherDateValue + 1;
            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, colVoucherDate, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colVoucherDate, row, colVoucherDateValue].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, colVoucherDate, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, colVoucherDate, row, colVoucherDateValue].Merge();

                reportUtility.SetHeaderText(ref sheet, row, colCompanyCurrencyCode, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colCompanyCurrencyCode, row, colLast].Merge();
            }
            sheet[row, colCompanyCurrencyCode].ColumnWidth = 15;
            //sheet[row, 6].RowHeight = 15;
            sheet[row, colLast].ColumnWidth = 15;
            sheet.Range[row, colVoucherDate, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, colVoucherDate, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;

            colGl = colVoucherNo;
            reportUtility.SetHeaderText(ref sheet, row, colGl, "GL");
            int colGLMarge = colVoucherNoValue + 1;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGLMarge) + row].Merge();


            DebitTranCurCode = colVoucherDate;
            CreditTranCurCode = colVoucherDateValue;

            DebitCompCurCode = colVoucherDate;
            CreditCompCurCode = colVoucherDateValue;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, DebitTranCurCode, "Debit", 13, ExcelHAlign.HAlignRight); DebitTranCurCode = colVoucherDate;  //xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, CreditTranCurCode, "Credit", 13, ExcelHAlign.HAlignRight); CreditTranCurCode = colVoucherDateValue;  //xlsCol++;

                colVoucherDateValue++;
                DebitCompCurCode = colVoucherDateValue;
                reportUtility.SetHeaderText(ref sheet, row, DebitCompCurCode, "Debit", 13, ExcelHAlign.HAlignRight); DebitCompCurCode = colVoucherDateValue; //xlsCol++;

                colVoucherDateValue++;
                CreditCompCurCode = colVoucherDateValue;
                reportUtility.SetHeaderText(ref sheet, row, CreditCompCurCode, "Credit", 13, ExcelHAlign.HAlignRight); CreditCompCurCode = colVoucherDateValue;
                colLast = colVoucherDateValue;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, colGl, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            else
            {


                DebitCompCurCode = colVoucherDate;
                reportUtility.SetHeaderText(ref sheet, row, DebitCompCurCode, "Debit", 13, ExcelHAlign.HAlignRight);

                CreditCompCurCode = colVoucherDateValue;
                reportUtility.SetHeaderText(ref sheet, row, CreditCompCurCode, "Credit", 13, ExcelHAlign.HAlignRight);
                colLast = colVoucherDateValue;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, 4, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }


            int formulaStartRow = 0;
            int formulaEndRow = 0;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++; //?? 12

                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGLMarge) + row].Merge();

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, DebitTranCurCode, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, CreditTranCurCode, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));

                        reportUtility.SetText(ref sheet, row, DebitCompCurCode, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, CreditCompCurCode, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));

                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {

                        reportUtility.SetText(ref sheet, row, DebitCompCurCode, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, CreditCompCurCode, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);

                    //glName = string.Empty;

                    row++;
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, colGLMarge, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                    //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                    //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                    //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet.Range[row, DebitTranCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(DebitTranCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(DebitTranCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, DebitTranCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, DebitTranCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, DebitTranCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, DebitTranCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, DebitTranCurCode].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, CreditTranCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(CreditTranCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(CreditTranCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, CreditTranCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, CreditTranCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, CreditTranCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, CreditTranCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, CreditTranCurCode].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, DebitCompCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(DebitCompCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(DebitCompCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, DebitCompCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, DebitCompCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, DebitCompCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, DebitCompCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, DebitCompCurCode].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, CreditCompCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(CreditCompCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(CreditCompCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, CreditCompCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, CreditCompCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, CreditCompCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, CreditCompCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, CreditCompCurCode].BorderAround(ExcelLineStyle.Hair);


                }
                else
                {
                    sheet.Range[row, DebitCompCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(DebitCompCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(DebitCompCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, DebitCompCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, DebitCompCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, DebitCompCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, DebitCompCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, DebitCompCurCode].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, CreditCompCurCode].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(CreditCompCurCode) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(CreditCompCurCode) + (formulaEndRow) + ")";
                    sheet.Range[row, CreditCompCurCode].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, CreditCompCurCode].CellStyle.Font.Bold = true;
                    sheet.Range[row, CreditCompCurCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, CreditCompCurCode].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, CreditCompCurCode].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[row, DebitCompCurCode, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, DebitCompCurCode, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, colGl, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && GetPlantIsShowFCInWord(plantId))
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].CellStyle.Font.Bold = true;
                    row++;
                }

                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].CellStyle.Font.Bold = true;

                //sheet.UsedRange.AutofitColumns();
                //sheet[1, 2].ColumnWidth = 60;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, colGl, header["AddedBy"].ToString());
                sheet.Range[row, colGl].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colGl, "Prepared By", true);
                sheet[row, colGl].ColumnWidth = 25;
                sheet[row, colGl+1].ColumnWidth = 15;


                reportUtility.SetSignatureText(ref sheet, row - 1, colCheckBy, header["PostedBy"].ToString());
                sheet.Range[row, colCheckBy].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colCheckBy, "Checked By", true);
                sheet[row, colCheckBy].ColumnWidth = 25;

                sheet.Range[row, colVoucherDateValue].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colVoucherDateValue, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Debit Note Set-Off", companyId,plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

                //    //else
                //    //{
                //    //    sheet.UsedRange.WrapText = true;
                //    //    sheet.UsedRange.CellStyle.Font.Size = 8;
                //    //    reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                //    //    reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Debit Note Set Off", companyId,plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }

        #endregion Credit Note Set-Off

        private DataTable GetCustomerForInvoiceReceive(string voucherId)
        {
            var sql = @"SELECT V.Id, VDC.VoucherDetailId, V.VoucherNo, P.UserName AS Customer
	                    FROM TRN.VoucherDetailCurrency AS VDC
		                LEFT JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                LEFT JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                LEFT JOIN TRN.InvoiceWriteOffDetail AS IWD ON IWD.Id=VD.InvoiceWriteOffDetailId
		                LEFT JOIN TRN.InvoiceWriteOff AS IW ON IW.Id=IWD.InvoiceWriteOffId
		                LEFT JOIN HKP.Party AS P ON P.Id=IW.PartyId
                        WHERE V.Archive=0 AND V.Id='" + voucherId + @"' AND P.UserName IS NOT NULL";
            return _sqlRepository.GetDataTable(sql);
        }
        private DataTable GetDebitNoteParty(string voucherId)
        {
            try
            {
                var sql = @" SELECT V.Id,VDC.VoucherDetailId,
		                                    V.VoucherNo ,
                                            P.UserName AS Vendor,vd.PartyId
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    LEFT JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    LEFT JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN TRN.InvoiceWriteOffDetail AS IWD ON IWD.Id=VD.InvoiceWriteOffDetailId
		                                    LEFT JOIN TRN.InvoiceWriteOff AS IW ON IW.Id=IWD.InvoiceWriteOffId
		                                    LEFT JOIN HKP.Party AS P ON P.Id=VD.PartyId
                                            where V.Archive = 0 AND V.Id = '" + voucherId + @"' AND P.UserName IS NOT NULL";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}