using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Organizations;
using Library.Model.Parties;
using Library.Service.Currencies;
using Library.Service.Extension;
using Library.Service.Helpers;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace Library.Service.Parties
{
    public class PartyReportService : IPartyReportService
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly IPartyService _partyService;
        private readonly IRepositoryAsync<PartyPlant> _partyPlantRepository;
        private readonly IRepositoryAsync<Company> _companyRepository;
        private readonly IRepositoryAsync<Plant> _plantRepository;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IPartyCategoryService _partyCategoryService;

        public PartyReportService(
            ISqlRepository sqlRepository
            , IPartyService partyService
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , IRepositoryAsync<PartyPlant> partyPlantRepository
            , IRepositoryAsync<Plant> plantRepository
            , IRepositoryAsync<Company> companyRepository
            , IPartyCategoryService partyCategoryService)
        {
            _sqlRepository = sqlRepository;
            _partyService = partyService;
            _plantRepository = plantRepository;
            _companyRepository = companyRepository;
            _partyPlantRepository = partyPlantRepository;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _partyCategoryService = partyCategoryService;
        }

        public IWorkbook PartyOutstadningReport(string companyGroupId, string companyId, string plantId, string plantName, string reportName, SourceType sourceType, DateTime postingDate)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 1);
                var sheet1 = workbook.Worksheets[0];
                CreatePartyOutstadningReportSheet(ref sheet1, report, reportName, "Outstanding", companyGroupId, companyId, plantId, plantName, sourceType, postingDate);
                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CreatePartyOutstadningReportSheet(ref IWorksheet sheet, ReportUtility report, string sheetHeader, string sheetName, string companyGroupId, string companyId, string plantId, string plantName, SourceType sourceType, DateTime postingDate)
        {
            var cmdText = @"SELECT NULL PINo, V.DocRefNo AS InvoiceNo,Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') InvoiceDate, CU.Code AS [CurrencyCode]
							, DATEDIFF(day, IV.PostingDate, IV.BaseOnDueDate) AS Aeging, (ISNULL(IV.Amount,0)) AS InvAmount
							, (ISNULL(IVD.NetAmount,0)- (ISNULL(IVD.WrittenOffAmount,0) )) AS PendingInvAmount, 0 PeindingForAdjustment
							, '' DateRemarks, V.Id,GL.Id AS AccountCodeId,VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark
                            , REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') PostingDate, [Park/Post]=CASE WHEN v.IsPark=1 THEN 'Park' ELSE 'Post' END
		                    , Replace(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') DocDate, V.DocRefNo
		                    , Replace(CONVERT(VARCHAR(11), v.VoucherDate, 106), ' ', '-') VoucherDate, V.VoucherNo, v.Narration, V.CurrencyId,CU1.Code AS TrnCurrency
                            , V.AddedBy AS PreparedBy, VDC.ParallelCurrencyId, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount+VD.CrAmount AS Value
		                    , VDC.DrAmount, VDC.CrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
							, (ISNULL(IVD.WrittenOffAmount,0) + ISNULL(IVD.NetAmount,0)) AS Received
                            , VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName As PlantName
							, Replace(CONVERT(VARCHAR(11), IV.BaseOnDueDate, 106), ' ', '-') MaturateDate, VD.RefCode AS Ref, VD.Narration AS DetailNarration
		                    , CO.UserName AS CompanyName,AM.Address1 AS AddressLine, BUD.UserName AS Budget, ACT.UserName AS Activity
	                        FROM TRN.VoucherDetailCurrency AS VDC
		                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                    LEFT JOIN TRN.InvoiceDetail AS IVD ON IVD.Id=VD.InvoiceDetailId
                            LEFT JOIN TRN.Invoice AS IV ON IV.VoucherId=V.Id
		                    LEFT JOIN HKP.Party AS P ON P.Id=IV.PartyId
		                    LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IV.PartyPlantId
		                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
		                    LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
		                    LEFT JOIN SCS.Currency AS CU1 ON CU1.Id=V.CurrencyId
		                    LEFT JOIN ORG.Company AS CO ON CO.Id=V.CompanyId
		                    LEFT JOIN MST.AddressMaster AS AM ON AM.Id=CO.AddressMasterId
                            LEFT JOIN SCS.FiscalYear AS FY ON FY.Id=V.FiscalYearId
							LEFT JOIN SCS.FiscalYearPeriod AS FYP ON FYP.Id=V.FiscalYearPeriodId
							LEFT JOIN MST.BudgetMaster BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUM.BudgetId
		                    LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                            WHERE V.Archive = 0 AND IV.IsWrittenOff=0 AND (IV.SourceType='" + sourceType + @"' OR IV.SourceType='" + sourceType + @"')
                            AND VD.InvoiceDetailId<>'' AND IV.CompanyGroupId='" + companyGroupId + @"' AND IV.CompanyId='" + companyId + @"' AND IV.PlantId='" + plantId + "' and v.PostingDate < '" + postingDate + "' ";
            var advanceDataList = _sqlRepository.GetDataTable(cmdText);

            var dtGeneralVoucher = advanceDataList;
            if (dtGeneralVoucher.Rows.Count == 0)
                throw new Exception("No Data Found!");

            var plCurrencyId = dtGeneralVoucher.Rows[0]["ParallelCurrencyId"].ToString();
            var _rowL = 6;
            _rowL++;

            var headreColIndex = 1;

            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "PI No", 32);
            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Invoice No", 32);
            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Invoice Date", 16);
            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Currency", 16);
            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Aeging", 26);
            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Inv Amount");
            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Pending Inv Amount");
            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Peinding For Adjustment", 12);
            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Date Remarks", ExcelHAlign.HAlignRight);
            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge();

            var Row_Total_Start = _rowL + 1;

            for (int n = 0; n < dtGeneralVoucher.Rows.Count; n++)
            {
                _rowL++;
                var AccountCodeId = dtGeneralVoucher.Rows[n]["GLGeneralInfoCode"].ToString();
                var _VoucherDetailId = dtGeneralVoucher.Rows[n]["VoucherDetailId"].ToString();
                report.SetText(ref sheet, _rowL, 1, dtGeneralVoucher.Rows[n]["PINo"].ToString());
                report.SetText(ref sheet, _rowL, 2, dtGeneralVoucher.Rows[n]["InvoiceNo"].ToString());
                report.SetText(ref sheet, _rowL, 3, dtGeneralVoucher.Rows[n]["InvoiceDate"].ToString());
                report.SetText(ref sheet, _rowL, 4, dtGeneralVoucher.Rows[n]["TrnCurrency"].ToString());
                report.SetText(ref sheet, _rowL, 5, dtGeneralVoucher.Rows[n]["Aeging"].ToString());
                report.SetText(ref sheet, _rowL, 6, Convert.ToDouble(dtGeneralVoucher.Rows[n]["InvAmount"]));
                report.SetText(ref sheet, _rowL, 7, Convert.ToDouble(dtGeneralVoucher.Rows[n]["PendingInvAmount"]));
                report.SetText(ref sheet, _rowL, 8, Convert.ToDouble(dtGeneralVoucher.Rows[n]["PeindingForAdjustment"]));
                report.SetText(ref sheet, _rowL, 9, dtGeneralVoucher.Rows[n]["DateRemarks"].ToString());
            }
            _rowL++;
            var shet2EndxlsCol = 9;
            sheet.Range[8, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[8, 1, _rowL, shet2EndxlsCol].BorderAround(ExcelLineStyle.Hair);

            _rowL = _rowL + 4;

            #region Signature

            sheet.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[_rowL, shet2EndxlsCol].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

            report.SetText(ref sheet, _rowL, 1, "Received By", true);
            report.SetText(ref sheet, _rowL, 3, "Prepared By", true);
            report.SetText(ref sheet, _rowL, shet2EndxlsCol, "HOD (Finance)", true);

            #endregion Signature

            sheet.Name = sheetName;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, shet2EndxlsCol, sheetHeader, companyId, plantId, plantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
        }

        public IWorkbook GetPartyOpeningBalanceLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string partyId, string partyPlantId, string fiscalYearId)
        {
            try
            {
                var row = 6;
                var col = 1;
                var shet2EndxlsCol = 1;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";

                var fiscalYear = _sqlRepository.GetData("SELECT FiscalYearCode, FiscalYearName, StartDate, EndDate FROM [SCS].[FiscalYear] WHERE Id='" + fiscalYearId + "'");
                var partyLedgerData = GetPartyOpeningBalanceLedger(companyGroupId, companyId, plantId, partyId, partyPlantId, fiscalYearId);
                if (partyLedgerData.Rows.Count > 0)
                {
                    // Set PartyName
                    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party");
                    sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, partyLedgerData.Rows[0]["PartyId"] + " - " + partyLedgerData.Rows[0]["Party"]);
                    sheet.Range[reportUtility.GetColumnNameForXls(3) + row + ": " + reportUtility.GetColumnNameForXls(5) + row].Merge();

                    // Set Party PlantName
                    if (!string.IsNullOrEmpty(partyPlantId))
                    {
                        row += 1;
                        reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party Plant");
                        sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                        reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, partyLedgerData.Rows[0]["PartyPlant"].ToString());
                        sheet.Range[reportUtility.GetColumnNameForXls(3) + row + ": " + reportUtility.GetColumnNameForXls(5) + row].Merge();
                    }

                    row += 2;
                    reportUtility.SetHeaderText(ref sheet, row, string.IsNullOrEmpty(partyPlantId) ? 8 : 7, "Transaction", ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(string.IsNullOrEmpty(partyPlantId) ? 8 : 7) + row + ":" + reportUtility.GetColumnNameForXls(string.IsNullOrEmpty(partyPlantId) ? 10 : 9) + row].Merge();
                    row += 1;

                    // Detail Header
                    col = 1;
                    var cGL = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "GL", 24);
                    var cPartyPlant = 0;
                    if (string.IsNullOrEmpty(partyPlantId))
                    {
                        col += 1;
                        cPartyPlant = col;
                        reportUtility.SetHeaderText(ref sheet, row, col, "Party Plant", 32);
                    }
                    col += 1;
                    var cPostingDate = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 10);
                    col = col + 1;
                    var cVoucherNo = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 12);
                    col = col + 1;
                    var cVoucherDate = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Voucher Date", 10);
                    col = col + 1;
                    var cDocRefNo = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Doc Ref", 12);
                    col = col + 1;
                    var cDocDate = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Doc Date", 10);
                    col = col + 1;
                    var cCurrency = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Currency", 7);
                    col = col + 1;
                    var cDebit = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 10, ExcelHAlign.HAlignRight);
                    col = col + 1;
                    var cCredit = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 10, ExcelHAlign.HAlignRight);
                    col = col + 1;
                    for (int n = 0; n < 1; n++)
                    {
                        reportUtility.SetHeaderText(ref sheet, row - 1, col, partyLedgerData.Rows[n]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                        sheet[row - 1, col, row - 1, col + 3].Merge();
                        reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 10, ExcelHAlign.HAlignRight); col++;
                        reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 10, ExcelHAlign.HAlignRight); col++;
                        reportUtility.SetHeaderText(ref sheet, row, col, "Balance", ExcelHAlign.HAlignRight); col++;
                        reportUtility.SetHeaderText(ref sheet, row, col, "Dr/Cr", 4, ExcelHAlign.HAlignRight); col++;
                    }
                    shet2EndxlsCol = col - 1;
                    row += 1;
                    var Row_Total_Start = row;
                    var CurrStartRow = row;
                    for (int icount = 0; icount < partyLedgerData.Rows.Count; icount++)
                    {
                        var accountCodeId = partyLedgerData.Rows[icount]["AccountCode"].ToString();
                        reportUtility.SetText(ref sheet, row, cGL, accountCodeId + " - " + partyLedgerData.Rows[icount]["GL"]);
                        if (string.IsNullOrEmpty(partyPlantId))
                            reportUtility.SetText(ref sheet, row, cPartyPlant, partyLedgerData.Rows[icount]["PartyPlant"].ToString());
                        reportUtility.SetText(ref sheet, row, cPostingDate, partyLedgerData.Rows[icount]["PostingDate"].ToString());
                        reportUtility.SetText(ref sheet, row, cVoucherNo, partyLedgerData.Rows[icount]["VoucherNo"].ToString());
                        reportUtility.SetText(ref sheet, row, cVoucherDate, partyLedgerData.Rows[icount]["VoucherDate"].ToString());
                        reportUtility.SetText(ref sheet, row, cDocRefNo, partyLedgerData.Rows[icount]["dDocRefNo"].ToString());
                        reportUtility.SetText(ref sheet, row, cDocDate, partyLedgerData.Rows[icount]["dDocDate"].ToString());
                        reportUtility.SetText(ref sheet, row, cCurrency, partyLedgerData.Rows[icount]["CurrencyCode"].ToString());
                        reportUtility.SetText(ref sheet, row, cDebit, Convert.ToDouble(partyLedgerData.Rows[icount]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, cCredit, Convert.ToDouble(partyLedgerData.Rows[icount]["CrAmount"].ToString()));
                        row += 1;
                    }
                    var drcrCol = cCredit + 1;
                    for (int p = 0; p < 1; p++)
                    {
                        row = CurrStartRow;
                        var drc = drcrCol++;
                        var crc = drcrCol++;
                        var blc = drcrCol++;
                        var fscount = 0;
                        var parallelCurrencyId = partyLedgerData.Rows[p]["ParallelCurrencyId"].ToString();
                        var dvDrCr = new DataView(partyLedgerData);
                        for (int icount = 0; icount < partyLedgerData.Rows.Count; icount++)
                        {
                            var voucherDetailId = partyLedgerData.Rows[icount]["VoucherDetailId"].ToString();
                            var voucherNoId = partyLedgerData.Rows[icount]["VoucherNo"].ToString();
                            dvDrCr.RowFilter = "ParallelCurrencyId='" + parallelCurrencyId + "' AND VoucherDetailId='" + voucherDetailId + "' AND VoucherNo='" + voucherNoId + "' ";
                            var dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count > 0)
                            {
                                reportUtility.SetText(ref sheet, row, drc, Convert.ToDouble(dtDrCr.Rows[0]["DrAmountPC"].ToString()));
                                reportUtility.SetText(ref sheet, row, crc, Convert.ToDouble(dtDrCr.Rows[0]["CrAmountPC"].ToString()));
                                if (fscount == 0)
                                {
                                    sheet.Range[row, blc].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(drc) + row + "-" + reportUtility.GetColumnNameForXls(crc) + row + ")";
                                }
                                else
                                {
                                    sheet.Range[row, blc].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(blc) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(drc) + row + "-" + reportUtility.GetColumnNameForXls(crc) + row + ")";
                                }
                                sheet.Range[row, blc].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                var formula = "IF(" + reportUtility.GetColumnNameForXls(blc) + row + ">= 0, \"Dr\", \"Cr\")";
                                sheet.Range[row, blc + 1].Formula = formula;
                                fscount++;
                            }
                            row += 1;
                        }
                    }
                    row = row + 6;
                    sheet.UsedRange.AutofitColumns();
                    sheet.UsedRange.CellStyle.Font.Size = 8;
                    reportUtility.CompanyPlantHeader(ref sheet, shet2EndxlsCol, "Party Opening Balance Ledger", companyId, plantName, null);
                    reportUtility.SetText(ref sheet, 4, shet2EndxlsCol, "Fiscal Year " + fiscalYear["FiscalYearName"], ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(1) + 4 + ":" + reportUtility.GetColumnNameForXls(shet2EndxlsCol) + 4].Merge();
                    reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
                }
                else
                {
                    reportUtility.CompanyHeader(ref sheet, shet2EndxlsCol, "Party Opening Balance Ledger", companyId);
                    reportUtility.SetText(ref sheet, 4, shet2EndxlsCol, "Fiscal Year " + fiscalYear["FiscalYearName"], ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(1) + 4 + ":" + reportUtility.GetColumnNameForXls(shet2EndxlsCol) + 4].Merge();
                    reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                }
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetPartyOpeningBalanceLedger(string companyGroupId, string companyId, string plantId, string partyId, string partyPlantId, string fiscalYearId)
        {
            var cmdText = @"SELECT V.Id, VD.Id AS VoucherDetailId, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate
                            , V.Narration, V.PostingDate AS PostingDateSort, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.CurrencyId, VD.Narration dNarration, VD.DocRefNo AS dDocRefNo
                            , REPLACE(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') dDocDate, ISNULL(VD.DrAmount,0) DrAmount, ISNULL(VD.CrAmount,0) CrAmount, VDC.ParallelCurrencyId, ISNULL(VDC.DrAmount,0) AS DrAmountPC
                            , ISNULL(VDC.CrAmount,0) AS CrAmountPC, GLGI.AccountCode, GLGI.[Description], TC.Code AS TrnCurrency, PC.Code AS CurrencyCode, GLGI.AccountCode GLGeneralInfoCode, GLGI.UserName GL
                            , VD.GLGeneralInfoId, VD.PartyId, p.UserName AS Party, ACT.BalanceType, VD.AddedDate, VD.PartyPlantId, pp.UserName AS PartyPlant
                            , [V_Type]=CASE WHEN V.SourceType='OpeningBalance' THEN 'Yes' ELSE 'No' END
                            FROM [TRN].[Voucher] AS V
                            LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId = v.Id
                            LEFT JOIN [HKP].[GLGeneralInfo] GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [TRN].[VoucherDetailCurrency]  AS VDC ON VDC.VoucherDetailId =VD.Id
                            LEFT JOIN [SCS].[Currency] AS PC ON PC.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS TC ON TC.Id=V.CurrencyId
                            LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
                            LEFT JOIN [HKP].[AccountType] AS ACT on ACT.Id=AG.AccountTypeId
                            LEFT JOIN [HKP].[GLAccountType] AS AT ON AT.GLGeneralInfoId=GLGI.Id
                            LEFT JOIN [HKP].[Party] P ON P.Id=VD.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND VD.PartyId='" + partyId + "' AND V.FiscalYearId='" + fiscalYearId + @"'
                            AND V.SourceType='OpeningBalance'";
            if (!string.IsNullOrEmpty(partyPlantId))
                cmdText += " AND VD.PartyPlantId='" + partyPlantId + "'";
            cmdText += " ORDER BY 7 ASC";
            return _sqlRepository.GetDataTable(cmdText);
        }

        public IWorkbook GetPartyLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId)
        {
            try
            {
                var row = 9;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";
                var colLast = 6;
                var colLast1 = 6;
                var col = 1;
                var StartRow = 9;

                //sheet = null;

                // Get Party Master
                var partyMaster = _partyService.Find(partyType, companyId, plantId, partyId);
                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party");
                sheet.Range[row, 1, row, 2].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, partyMaster["PartyCode"] + " - " + partyMaster["PartyName"]);
                sheet.Range[row, 3, row, 5].Merge();
                sheet.Range[row, 3, row, 5].RowHeight = 30;

                reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Account Group");
                sheet.Range[row, 6, row, 7].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 8, partyMaster["PartyAccountGroupName"].ToString());

                row++;
                if (!string.IsNullOrEmpty(partyPlantId))
                {
                    var partyPlant = _partyPlantRepository.Find(partyPlantId);
                    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party Plant");
                    sheet.Range[row, 1, row, 2].Merge();
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, partyPlant?.UserName);
                    sheet.Range[row, 3, row, 5].Merge();

                    colLast = colLast - 1;
                    colLast1 = colLast;
                }
                if (!string.IsNullOrEmpty(gSTINId))
                {
                    reportUtility.SetMasterHeaderText(ref sheet, row, 7, "Party GSTIN");
                    sheet.Range[row, 7, row, 8].Merge();
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, 9, gSTINId);
                    sheet.Range[row, 9, row, 11].Merge();
                }

                row++;
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                {
                    reportUtility.SetHeaderText(ref sheet, row, colLast + 1, "Transaction", ExcelHAlign.HAlignCenter);
                    sheet.Range[row, colLast + 1, row, colLast + 3].Merge();
                    colLast = colLast + 3;
                }
                reportUtility.SetHeaderText(ref sheet, row, colLast + 1, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet.Range[row, colLast + 1, row, colLast + 4].Merge();
                sheet.Range[row, colLast + 1, row, colLast + 4].BorderAround();
                // Set Row Header
            row++;

                reportUtility.SetHeaderText(ref sheet, row, col, "GL", 28); col++;
                if (string.IsNullOrEmpty(partyPlantId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Party Plant", 18); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 30); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 30); col++;                        
                reportUtility.SetHeaderText(ref sheet, row, col, "Particulars", 45); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 50); col++;

               sheet.Range[row, col].WrapText = true;

                if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Currency", 15, ExcelHAlign.HAlignLeft); col++;
                    
                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 37, ExcelHAlign.HAlignRight); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 37, ExcelHAlign.HAlignRight); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 35, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 35, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Balance", 40, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Dr/Cr", 12, ExcelHAlign.HAlignRight);
                sheet[row, 1, row, col].RowHeight = 70;
              //  sheet[row, 1, row, col].WrapText = true;

                row++;
                reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].RowHeight = 30;
                // Get party opening balance data.
                var obVal = GetPartyOpeningBalance(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, partyType.ToString());
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                        reportUtility.SetText(ref sheet, row, col - 1, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);

                    sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                    sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;

                }

                var ledgerData = GetPartyPlantLedger(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId, partyType.ToString());
                row++;
                int sumStrRow = 0;
                // Get bank transaction data.
                if (ledgerData.Rows.Count > 0)
                {
                    sumStrRow = row;
                    col = 1;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        //sumStrRow = row;
                        col = 1;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["GLGeneralInfoCode"] + " - " + ledgerData.Rows[i]["GLGeneralInfoName"]); col++;
                        if (string.IsNullOrEmpty(partyPlantId))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PartyPlantName"].ToString()); col++;
                        }
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PostingDate"].ToString(), ExcelHAlign.HAlignLeft); col++;
                     
                        //reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft); col++;
                        ////reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocRefNo"].ToString(), 9, ExcelHAlign.HAlignLeft); col++;
                        ////reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocDate"].ToString(), 9, ExcelHAlign.HAlignLeft); col++;
                        //sheet[row, col].ColumnWidth = 50;
                        sheet.Range[row, col].WrapText = true;


                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Particular"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Narration"].ToString()); col++;

                        sheet.Range[row, col].WrapText = true;
                        if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["CurrencyCode"].ToString()); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString())); col++;
                        }
                        // Base currency checking
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString()));col++;
                       // sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatDecimalTwo(); col++;
                       
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;
                        sheet.Range[row, col].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(col - 2) + row + "-" + reportUtility.GetColumnNameForXls(col - 1) + row + ")";
                   
                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;
                        sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        row++;
                    }
                }

                reportUtility.SetText(ref sheet, row, 1, "Closing Balance",true);
                sheet[row, col].RowHeight = 30;
                //sheet[row, 7].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(7) + (row - 1) + ":" + clsStaticInfo.GetxlsCol(7) + row + ")";
                sheet.Range[row, col - 3].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col - 3) + (sumStrRow) + ":" + reportUtility.GetColumnNameForXls(col - 3) + (row - 1) + ")";
                sheet.Range[row, col - 3].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
               // sheet.Range[row, col - 3].CellStyle.Font.Bold = true;
                sheet.Range[row, col - 3].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[row, col - 2].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col - 2) + (sumStrRow) + ":" + reportUtility.GetColumnNameForXls(col - 2) + (row - 1) + ")";
                sheet.Range[row, col - 2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //sheet.Range[row, col - 2].CellStyle.Font.Bold = true;
                sheet.Range[row, col - 2].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1 - 1) + row].Merge();
                sheet.Range[row, col - 1].Formula = "=" + reportUtility.GetColumnNameForXls(col - 1) + (row - 1);
                sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
               //sheet.Range[row, col - 1].CellStyle.Font.Bold = true;
                sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;

                var endCol = col;

               sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, row, endCol].CellStyle.Font.Size = 27;
                
            
                //reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Portrait);
                reportUtility.PageSetup3(ref sheet, 6, ExcelPageOrientation.Portrait);



                sheet[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[StartRow+3, 1, row, endCol].BorderInside(ExcelLineStyle.Thin);
                sheet.Range[StartRow+3, 1, row, endCol].BorderAround(ExcelLineStyle.Thin);

                reportUtility.CompanyPlantHeader(ref sheet, col, "Party Ledger", companyId, plantId, plantName, "From " + fromDate + " To " + toDate + "");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(col) + 5].Merge();

                sheet[1, 1, 1, endCol].CellStyle.Font.Size = 45;
                sheet[1, 1, 1, endCol].RowHeight = 40;

                sheet[2, 1, 2, endCol].CellStyle.Font.Size = 40;
                sheet[2, 1, 2, endCol].RowHeight = 35;
                sheet[3, 1, 3, endCol].CellStyle.Font.Size = 30;
                sheet[3, 1, 3, endCol].RowHeight = 30;
                sheet[4, 1, 4, endCol].CellStyle.Font.Size = 30;
                sheet[4, 1, 4, endCol].RowHeight = 30;
                sheet[5, 1, 5, endCol].CellStyle.Font.Size = 30;
                sheet[5, 1, 5, endCol].RowHeight = 30;




                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private Dictionary<string, object> FindParty( string companyId, string plantId, string partyId)
        {
            var sql = @"SELECT top 1 P.Id AS PartyId, P.Code AS PartyCode, P.UserName AS PartyName, CP.CurrencyId, C.Code AS CurrencyCode, PAG.UserName AS PartyAccountGroupName
                        FROM [HKP].[Party] AS P
                        JOIN [HKP].[CompanyParty] AS CP ON P.Id=CP.PartyId
                        JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
                        JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
                        WHERE CP.CompanyId='" + companyId + "' AND CP.PlantId='" + plantId + "' AND CP.PartyId='" + partyId + "'";
            return _sqlRepository.GetData(sql);
        }
        public IWorkbook GetPartyLedgerReportBothCustomerVendor(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId)
        {
            try
            {
                var row = 6;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";
                var colLast = 6;
                var colLast1 = 6;
                var col = 1;
                var StartRow = 9;

                //sheet = null;

                // Get Party Master
                var partyMaster = FindParty(companyId, plantId, partyId);
                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 2, partyMaster["PartyCode"] + " - " + partyMaster["PartyName"]);
                sheet.Range[row, 2, row, 4].Merge();
               
                //int colAccountGroup = 7;
                //reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Account Group");
                //sheet.Range[row, 6].ColumnWidth = 13;
                //reportUtility.SetMiddleAlignmentText(ref sheet, row, colAccountGroup, partyMaster["PartyAccountGroupName"].ToString());
                //sheet.Range[row, colAccountGroup, row, colAccountGroup + 2].Merge();

                row++;
                if (!string.IsNullOrEmpty(partyPlantId))
                {
                    var partyPlant = _partyPlantRepository.Find(partyPlantId);
                    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party Plant");
                    sheet.Range[row, 1, row, 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, 2, partyPlant?.UserName);
                    sheet.Range[row, 2, row, 4].Merge();

                    colLast = colLast - 1;
                    colLast1 = colLast;
                    row++;
                }
                if (!string.IsNullOrEmpty(gSTINId))
                {
                    reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Party GSTIN");
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, 7, gSTINId);
                    sheet.Range[row, 7, row, 9].Merge();
                    row++;
                }

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer");
                sheet.Range[row+1, 1, row+1, 6].Merge();
                row++;
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                {
                    reportUtility.SetHeaderText(ref sheet, row, colLast + 1, "Transaction", ExcelHAlign.HAlignCenter);
                    sheet.Range[row, colLast + 1, row, colLast + 3].Merge();
                    sheet.Range[row, colLast + 1, row, colLast + 3].BorderAround(ExcelLineStyle.Thin);

                    colLast = colLast + 3;
                }
                reportUtility.SetHeaderText(ref sheet, row, colLast + 1, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet.Range[row, colLast + 1, row, colLast + 4].Merge();
                sheet.Range[row, colLast + 1, row, colLast + 4].BorderAround();
                // Set Row Header

                #region Customer Data
                row++;
                reportUtility.SetHeaderText(ref sheet, row, col, "GL", 15); col++;
                if (string.IsNullOrEmpty(partyPlantId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Party Plant", 10); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 15); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 15); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Particulars", 20); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 15); col++;

                sheet.Range[row, col].WrapText = true;

                if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Currency", 8, ExcelHAlign.HAlignLeft); col++;

                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Balance", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Dr/Cr", 10, ExcelHAlign.HAlignRight);
                
                row++;
                reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].RowHeight = 20;
                // Get party opening balance data.
                var obVal = GetPartyOpeningBalance(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, "Customer");
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                        reportUtility.SetText(ref sheet, row, col - 1, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);

                    sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                    sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;

                }

                var ledgerData = GetPartyPlantLedger(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId, "Customer");
                row++;
                int sumStrRow = 0;
                // Get bank transaction data.
                if (ledgerData.Rows.Count > 0)
                {
                    sumStrRow = row;
                    col = 1;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        //sumStrRow = row;
                        col = 1;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["GLGeneralInfoCode"] + " - " + ledgerData.Rows[i]["GLGeneralInfoName"]); col++;
                        if (string.IsNullOrEmpty(partyPlantId))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PartyPlantName"].ToString()); col++;
                        }
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PostingDate"].ToString(), ExcelHAlign.HAlignLeft); col++;

                        
                        sheet.Range[row, col].WrapText = true;


                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Particular"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Narration"].ToString()); col++;

                        sheet.Range[row, col].WrapText = true;
                        if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["CurrencyCode"].ToString()); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString())); col++;
                        }
                        // Base currency checking
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                        // sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatDecimalTwo(); col++;

                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;
                        sheet.Range[row, col].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(col - 2) + row + "-" + reportUtility.GetColumnNameForXls(col - 1) + row + ")";

                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;
                        sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        row++;
                    }
                }

                reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);
                //sheet[row, col].RowHeight = 30;
                //sheet[row, 7].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(7) + (row - 1) + ":" + clsStaticInfo.GetxlsCol(7) + row + ")";
                sheet.Range[row, col - 3].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col - 3) + (sumStrRow) + ":" + reportUtility.GetColumnNameForXls(col - 3) + (row - 1) + ")";
                sheet.Range[row, col - 3].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                // sheet.Range[row, col - 3].CellStyle.Font.Bold = true;
                sheet.Range[row, col - 3].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[row, col - 2].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col - 2) + (sumStrRow) + ":" + reportUtility.GetColumnNameForXls(col - 2) + (row - 1) + ")";
                sheet.Range[row, col - 2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //sheet.Range[row, col - 2].CellStyle.Font.Bold = true;
                sheet.Range[row, col - 2].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1 - 1) + row].Merge();
                sheet.Range[row, col - 1].Formula = "=" + reportUtility.GetColumnNameForXls(col - 1) + (row - 1);
                sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //sheet.Range[row, col - 1].CellStyle.Font.Bold = true;
                sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;

                #endregion

                #region Vendor Data
                row++; row++; row++;
               
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor");
                sheet.Range[row, 2, row, 6].Merge();
                row++;
                col = 1;
                reportUtility.SetHeaderText(ref sheet, row, col, "GL", 15); col++;
                if (string.IsNullOrEmpty(partyPlantId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Party Plant", 10); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 15); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 15); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Particulars", 20); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 15); col++;

                sheet.Range[row, col].WrapText = true;

                if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Currency", 8, ExcelHAlign.HAlignLeft); col++;

                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Balance", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Dr/Cr", 10, ExcelHAlign.HAlignRight);

                row++;
                reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].RowHeight = 20;
                // Get party opening balance data.
                var obValVendor = GetPartyOpeningBalance(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, "Vendor");
                if (obValVendor.Count > 0)
                {
                    // Set Opening Balance
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                        reportUtility.SetText(ref sheet, row, col - 1, Convert.ToDouble(obValVendor[0]["CompanyCurrencyOB"]), true);

                    sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                    sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;

                }

                var ledgerDataVendor = GetPartyPlantLedger(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId, "Vendor");
                row++;
                int sumStrRowVendor = 0;
                // Get bank transaction data.
                if (ledgerDataVendor.Rows.Count > 0)
                {
                    sumStrRow = row;
                    col = 1;
                    for (int i = 0; i < ledgerDataVendor.Rows.Count; i++)
                    {
                        //sumStrRow = row;
                        col = 1;
                        reportUtility.SetText(ref sheet, row, col, ledgerDataVendor.Rows[i]["GLGeneralInfoCode"] + " - " + ledgerDataVendor.Rows[i]["GLGeneralInfoName"]); col++;
                        if (string.IsNullOrEmpty(partyPlantId))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerDataVendor.Rows[i]["PartyPlantName"].ToString()); col++;
                        }
                        reportUtility.SetText(ref sheet, row, col, ledgerDataVendor.Rows[i]["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerDataVendor.Rows[i]["PostingDate"].ToString(), ExcelHAlign.HAlignLeft); col++;


                        sheet.Range[row, col].WrapText = true;


                        reportUtility.SetText(ref sheet, row, col, ledgerDataVendor.Rows[i]["Particular"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerDataVendor.Rows[i]["Narration"].ToString()); col++;

                        sheet.Range[row, col].WrapText = true;
                        if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerDataVendor.Rows[i]["CurrencyCode"].ToString()); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerDataVendor.Rows[i]["DrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerDataVendor.Rows[i]["CrAmount"].ToString())); col++;
                        }
                        // Base currency checking
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerDataVendor.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                        // sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatDecimalTwo(); col++;

                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerDataVendor.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;
                        sheet.Range[row, col].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(col - 2) + row + "-" + reportUtility.GetColumnNameForXls(col - 1) + row + ")";

                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;
                        sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        row++;
                    }
                }

                reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);
                //sheet[row, col].RowHeight = 30;
                //sheet[row, 7].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(7) + (row - 1) + ":" + clsStaticInfo.GetxlsCol(7) + row + ")";
                sheet.Range[row, col - 3].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col - 3) + (sumStrRowVendor) + ":" + reportUtility.GetColumnNameForXls(col - 3) + (row - 1) + ")";
                sheet.Range[row, col - 3].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                // sheet.Range[row, col - 3].CellStyle.Font.Bold = true;
                sheet.Range[row, col - 3].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[row, col - 2].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col - 2) + (sumStrRowVendor) + ":" + reportUtility.GetColumnNameForXls(col - 2) + (row - 1) + ")";
                sheet.Range[row, col - 2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //sheet.Range[row, col - 2].CellStyle.Font.Bold = true;
                sheet.Range[row, col - 2].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1 - 1) + row].Merge();
                sheet.Range[row, col - 1].Formula = "=" + reportUtility.GetColumnNameForXls(col - 1) + (row - 1);
                sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //sheet.Range[row, col - 1].CellStyle.Font.Bold = true;
                sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                #endregion

                var endCol = col;
                sheet.UsedRange.CellStyle.Font.Size = 8;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[StartRow, 1, row, endCol].CellStyle.Font.Size = 27;

                //reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Portrait);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Portrait);

                sheet[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[StartRow, 1, row, endCol].BorderInside(ExcelLineStyle.Thin);
                sheet.Range[StartRow, 1, row, endCol].BorderAround(ExcelLineStyle.Thin);

                reportUtility.CompanyPlantHeader(ref sheet, col, "Party Ledger Both Customer and Vendor", companyId, plantId, plantName, "From " + fromDate + " To " + toDate + "");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(col) + 5].Merge();

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IWorkbook GetPartyLedgerReportXls(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId)
        {
            try
            {
                var row = 6;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";
                var colLast = 6;
                var colLast1 = 6;
                var col = 1;
                var StartRow = 9;

                //sheet = null;

                // Get Party Master
                var partyMaster = _partyService.Find(partyType, companyId, plantId, partyId);
                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party");
                //sheet.Range[row, 1, row, 2].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 2, partyMaster["PartyCode"] + " - " + partyMaster["PartyName"]);
                sheet.Range[row, 2, row, 4].Merge();
                // sheet.Range[row, 3, row, 5].RowHeight = 30;
                int colAccountGroup = 7;
                reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Account Group");
                sheet.Range[row, 6].ColumnWidth = 13;
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colAccountGroup, partyMaster["PartyAccountGroupName"].ToString());
                sheet.Range[row, colAccountGroup, row, colAccountGroup+2].Merge();

                row++;
                if (!string.IsNullOrEmpty(partyPlantId))
                {
                    var partyPlant = _partyPlantRepository.Find(partyPlantId);
                    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party Plant");
                    sheet.Range[row, 1, row, 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, 2, partyPlant?.UserName);
                    sheet.Range[row, 2, row, 4].Merge();

                    colLast = colLast - 1;
                    colLast1 = colLast;
                    row++;
                }
                if (!string.IsNullOrEmpty(gSTINId))
                {
                    reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Party GSTIN");
                    //sheet.Range[row, 7, row, 8].Merge();
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, 7, gSTINId);
                    sheet.Range[row, 7, row, 9].Merge();
                    row++;
                }

                //row++;
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                {
                    reportUtility.SetHeaderText(ref sheet, row, colLast + 1, "Transaction", ExcelHAlign.HAlignCenter);
                    sheet.Range[row, colLast + 1, row, colLast + 3].Merge();
                    sheet.Range[row, colLast + 1, row, colLast + 3].BorderAround(ExcelLineStyle.Thin);

                    colLast = colLast + 3;
                }
                reportUtility.SetHeaderText(ref sheet, row, colLast + 1, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet.Range[row, colLast + 1, row, colLast + 4].Merge();
                sheet.Range[row, colLast + 1, row, colLast + 4].BorderAround();
                // Set Row Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, col, "GL", 15); col++;
                if (string.IsNullOrEmpty(partyPlantId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Party Plant", 10); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 15); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 15); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Particulars", 20); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 15); col++;

                sheet.Range[row, col].WrapText = true;

                if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Currency", 8, ExcelHAlign.HAlignLeft); col++;

                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Balance", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Dr/Cr", 10, ExcelHAlign.HAlignRight);
                //sheet[row, 1, row, col].RowHeight = 70;
                //  sheet[row, 1, row, col].WrapText = true;

                row++;
                reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].RowHeight = 20;
                // Get party opening balance data.
                var obVal = GetPartyOpeningBalance(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, partyType.ToString());
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                        reportUtility.SetText(ref sheet, row, col - 1, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);

                    sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                    sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;

                }

                var ledgerData = GetPartyPlantLedger(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId, partyType.ToString());
                row++;
                int sumStrRow = 0;
                // Get bank transaction data.
                if (ledgerData.Rows.Count > 0)
                {
                    sumStrRow = row;
                    col = 1;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        //sumStrRow = row;
                        col = 1;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["GLGeneralInfoCode"] + " - " + ledgerData.Rows[i]["GLGeneralInfoName"]); col++;
                        if (string.IsNullOrEmpty(partyPlantId))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PartyPlantName"].ToString()); col++;
                        }
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PostingDate"].ToString(), ExcelHAlign.HAlignLeft); col++;

                        //reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft); col++;
                        ////reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocRefNo"].ToString(), 9, ExcelHAlign.HAlignLeft); col++;
                        ////reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocDate"].ToString(), 9, ExcelHAlign.HAlignLeft); col++;
                        //sheet[row, col].ColumnWidth = 50;
                        sheet.Range[row, col].WrapText = true;


                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Particular"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Narration"].ToString()); col++;

                        sheet.Range[row, col].WrapText = true;
                        if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["CurrencyCode"].ToString()); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString())); col++;
                        }
                        // Base currency checking
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                        // sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatDecimalTwo(); col++;

                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;
                        sheet.Range[row, col].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(col - 2) + row + "-" + reportUtility.GetColumnNameForXls(col - 1) + row + ")";

                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;
                        sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        row++;
                    }
                }

                reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);
                //sheet[row, col].RowHeight = 30;
                //sheet[row, 7].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(7) + (row - 1) + ":" + clsStaticInfo.GetxlsCol(7) + row + ")";
                sheet.Range[row, col - 3].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col - 3) + (sumStrRow) + ":" + reportUtility.GetColumnNameForXls(col - 3) + (row - 1) + ")";
                sheet.Range[row, col - 3].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                // sheet.Range[row, col - 3].CellStyle.Font.Bold = true;
                sheet.Range[row, col - 3].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[row, col - 2].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col - 2) + (sumStrRow) + ":" + reportUtility.GetColumnNameForXls(col - 2) + (row - 1) + ")";
                sheet.Range[row, col - 2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //sheet.Range[row, col - 2].CellStyle.Font.Bold = true;
                sheet.Range[row, col - 2].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1 - 1) + row].Merge();
                sheet.Range[row, col - 1].Formula = "=" + reportUtility.GetColumnNameForXls(col - 1) + (row - 1);
                sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //sheet.Range[row, col - 1].CellStyle.Font.Bold = true;
                sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;

                var endCol = col;
                sheet.UsedRange.CellStyle.Font.Size = 8;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[StartRow, 1, row, endCol].CellStyle.Font.Size = 27;

                //reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Portrait);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Portrait);

                sheet[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[StartRow , 1, row, endCol].BorderInside(ExcelLineStyle.Thin);
                sheet.Range[StartRow , 1, row, endCol].BorderAround(ExcelLineStyle.Thin);

                reportUtility.CompanyPlantHeader(ref sheet, col, "Party Ledger", companyId, plantId, plantName, "From " + fromDate + " To " + toDate + "");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(col) + 5].Merge();

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IWorkbook GetPartyLedgerReportLongSizeXls(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId)
        {
            try
            {
                var row = 6;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";
                var colLast = 6;
                var colLast1 = 6;
                var col = 1;
                var StartRow = 9;

                //sheet = null;

                // Get Party Master
                var partyMaster = _partyService.Find(partyType, companyId, plantId, partyId);
                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party");
                //sheet.Range[row, 1, row, 2].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 2, partyMaster["PartyCode"] + " - " + partyMaster["PartyName"]);
                sheet.Range[row, 2, row, 4].Merge();
                // sheet.Range[row, 3, row, 5].RowHeight = 30;
                int colAccountGroup = 7;
                reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Account Group");
                //sheet.Range[row, colAccountGroup, row, colAccountGroup + 1].Merge();
                //sheet.Range[row, 6].HorizontalAlignment = ExcelHAlign.HAlignRight;
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colAccountGroup, partyMaster["PartyAccountGroupName"].ToString());
                sheet.Range[row, colAccountGroup, row, colAccountGroup + 2].Merge();

                row++;
                if (!string.IsNullOrEmpty(partyPlantId))
                {
                    var partyPlant = _partyPlantRepository.Find(partyPlantId);
                    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party Plant");
                    sheet.Range[row, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, 2, partyPlant?.UserName);
                    sheet.Range[row, 2, row, 4].Merge();

                    colLast = colLast - 1;
                    colLast1 = colLast;
                }
                if (!string.IsNullOrEmpty(gSTINId))
                {
                    reportUtility.SetMasterHeaderText(ref sheet, row, 7, "Party GSTIN");
                    sheet.Range[row, 7, row, 8].Merge();
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, 9, gSTINId);
                    sheet.Range[row, 9, row, 11].Merge();
                }

                row++;
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                {
                    reportUtility.SetHeaderText(ref sheet, row, colLast + 1, "Transaction", ExcelHAlign.HAlignCenter);
                    sheet.Range[row, colLast + 1, row, colLast + 3].Merge();
                    sheet.Range[row, colLast + 1, row, colLast + 3].BorderAround(ExcelLineStyle.Thin);

                    colLast = colLast + 3;
                }
                reportUtility.SetHeaderText(ref sheet, row, colLast + 1, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet.Range[row, colLast + 1, row, colLast + 4].Merge();
                sheet.Range[row, colLast + 1, row, colLast + 4].BorderAround();
                // Set Row Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, col, "GL", 15); col++;
                if (string.IsNullOrEmpty(partyPlantId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Party Plant", 10); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 15); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 15); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Ref No", 15); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Date", 15); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Particulars", 20); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 15); col++;

                sheet.Range[row, col].WrapText = true;

                if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Currency", 8, ExcelHAlign.HAlignLeft); col++;

                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Balance", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Dr/Cr", 4, ExcelHAlign.HAlignRight);
                //sheet[row, 1, row, col].RowHeight = 70;
                //  sheet[row, 1, row, col].WrapText = true;

                row++;
                reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].RowHeight = 20;
                // Get party opening balance data.
                var obVal = GetPartyOpeningBalance(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, partyType.ToString());
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                        reportUtility.SetText(ref sheet, row, col - 1, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);

                    sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                    sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;

                }

                var ledgerData = GetPartyPlantLedger(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId, partyType.ToString());
                row++;
                int sumStrRow = 0;
                // Get bank transaction data.
                if (ledgerData.Rows.Count > 0)
                {
                    sumStrRow = row;
                    col = 1;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        //sumStrRow = row;
                        col = 1;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["GLGeneralInfoCode"] + " - " + ledgerData.Rows[i]["GLGeneralInfoName"]); col++;
                        if (string.IsNullOrEmpty(partyPlantId))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PartyPlantName"].ToString()); col++;
                        }
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PostingDate"].ToString(), ExcelHAlign.HAlignLeft); col++;

                        //reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocRefNo"].ToString(), 9, ExcelHAlign.HAlignLeft); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocDate"].ToString(), 9, ExcelHAlign.HAlignLeft); col++;
                        //sheet[row, col].ColumnWidth = 50;
                        sheet.Range[row, col].WrapText = true;


                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Particular"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Narration"].ToString()); col++;

                        sheet.Range[row, col].WrapText = true;
                        if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["CurrencyCode"].ToString()); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString())); col++;
                        }
                        // Base currency checking
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                        // sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatDecimalTwo(); col++;

                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;
                        sheet.Range[row, col].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(col - 2) + row + "-" + reportUtility.GetColumnNameForXls(col - 1) + row + ")";

                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;
                        sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        row++;
                    }
                }

                reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);
                //sheet[row, col].RowHeight = 30;
                //sheet[row, 7].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(7) + (row - 1) + ":" + clsStaticInfo.GetxlsCol(7) + row + ")";
                sheet.Range[row, col - 3].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col - 3) + (sumStrRow) + ":" + reportUtility.GetColumnNameForXls(col - 3) + (row - 1) + ")";
                sheet.Range[row, col - 3].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                // sheet.Range[row, col - 3].CellStyle.Font.Bold = true;
                sheet.Range[row, col - 3].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[row, col - 2].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col - 2) + (sumStrRow) + ":" + reportUtility.GetColumnNameForXls(col - 2) + (row - 1) + ")";
                sheet.Range[row, col - 2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //sheet.Range[row, col - 2].CellStyle.Font.Bold = true;
                sheet.Range[row, col - 2].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1 - 1) + row].Merge();
                sheet.Range[row, col - 1].Formula = "=" + reportUtility.GetColumnNameForXls(col - 1) + (row - 1);
                sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //sheet.Range[row, col - 1].CellStyle.Font.Bold = true;
                sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;

                var endCol = col;
                sheet.UsedRange.CellStyle.Font.Size = 8;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[StartRow, 1, row, endCol].CellStyle.Font.Size = 27;

                //reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Portrait);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Portrait);

                sheet[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[StartRow, 1, row, endCol].BorderInside(ExcelLineStyle.Thin);
                sheet.Range[StartRow, 1, row, endCol].BorderAround(ExcelLineStyle.Thin);

                reportUtility.CompanyPlantHeader(ref sheet, col, "Party Ledger", companyId, plantId, plantName, "From " + fromDate + " To " + toDate + "");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(col) + 5].Merge();

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        private List<Dictionary<string, object>> GetPartyOpeningBalance(string companyGroupId, string companyId, string plantId, string partyId, string partyPlantId, string fromDate, string partyType)
        {
            string tempPartyType = null;
            if (partyType == "Vendor" || partyType == "Customer" || partyType == "Director")
            {
                tempPartyType = partyType;
            }
            if (partyType == null || partyType == "null")
            {
                tempPartyType = "Vendor" + "','" + "Customer" + "','" + "Director";
            }
            var sql = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                        SELECT SUM(DrAmount) - SUM(CrAmount) AS OB, CompanyCurrencyId, SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyCurrencyOB FROM (
                        SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.PartyId='" + partyId + "' AND VD.PartyType IN ('" + tempPartyType + "') AND V.PostingDate < '" + fromDate.ToDbDate() + "'";
            if (!string.IsNullOrEmpty(partyPlantId))
                sql += " AND VD.PartyPlantId='" + partyPlantId + "'";
            sql += @" GROUP BY CC.CompanyCurrencyId
                    UNION
                    SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount, CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                    FROM [TRN].[Voucher] AS V
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                    LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                    FROM [TRN].[VoucherDetailCurrency] AS VDC
	                    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                    ) AS CC ON CC.VoucherDetailId=VD.Id
                    WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.PartyId='" + partyId + "' AND VD.PartyType IN ('" + tempPartyType + "') AND V.PostingDate ='" + fromDate.ToDbDate() + "' AND V.SourceType='OpeningBalance'";
            if (!string.IsNullOrEmpty(partyPlantId))
                sql += " AND VD.PartyPlantId='" + partyPlantId + "'";
            sql += " GROUP BY CC.CompanyCurrencyId) AS X GROUP BY X.CompanyCurrencyId";
            return _sqlRepository.GetDataCollection(sql);
        }

        
        private DataTable GetPartyPlantLedger(string companyGroupId, string companyId, string plantId, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId, string partyType)
        {
            string tempPartyType = null;
            if (partyType == "Vendor" || partyType == "Customer" || partyType == "Director")
            {
                tempPartyType = partyType;
            }
            if (partyType == null || partyType == "null")
            {
                tempPartyType = "Vendor" + "','" + "Customer" + "','" + "Director";
            }
            var cmdText = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                            SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration, ISNULL(VD.DrAmount,0) AS DrAmount, ISNULL(VD.CrAmount,0) AS CrAmount
                            , CC.CompanyCurrencyId, ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) AS CompanyCurrencyCrAmount, C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                            , VD.GLGeneralInfoId,GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName
                             ,Particular =concat( STUFF((select distinct ','+xpA.UserName+ ' '+'('+ xp.UserName+')' from
														TRN.VoucherDetail XVD JOIN [HKP].[Party] AS XP ON XP.Id=XVD.PartyId
                                                        JOIN HKP.Activity AS XPA ON XPA.Id=XVD.ActivityId
													    where	XVD.VoucherId=V.Id AND XVD.PartyId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												,STUFF((select distinct ','+xp.AccountTitle from
														TRN.VoucherDetail XVD JOIN MST.BankMaster AS XP ON XP.Id=XVD.BankMasterId
													where	XVD.VoucherId=V.Id AND XVD.BankMasterId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												 , STUFF((select distinct ','+xp.UserName from
														TRN.VoucherDetail XVD JOIN MST.CashMaster AS XP ON XP.Id=XVD.CashMasterId
													where	XVD.VoucherId=V.Id AND XVD.CashMasterId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												 ,STUFF((select distinct ','+xp.EmployeeName from
														TRN.VoucherDetail XVD JOIN [dbo].[EmployeeInformation] AS XP ON XP.SystemId=XVD.EmployeeId
													where	XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                ,STUFF((select distinct ','+'('+XV.DocrefNo +') ' from  TRN.Voucher AS XV where V.Id=XV.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                
                                                , STUFF((select distinct ','+xp.UserName from
														TRN.VoucherDetail XVD JOIN HKP.Activity AS XP ON XP.Id=XVD.ActivityId
													where	XVD.VoucherId=V.Id AND XVD.PartyId is null AND XVD.CashMasterId IS NULL AND XVD.BankMasterId IS NULL AND XVD.EmployeeId IS NULL
													 AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                                       
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId AND P.Id=VD.PartyId
                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                            ) AS CC ON CC.VoucherDetailId=VD.Id
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND VD.PartyId='" + partyId + "' AND VD.PartyType IN ('" + tempPartyType + "') AND V.PostingDate BETWEEN '" + fromDate.ToDbDate() + "' AND '" + toDate + @"'
                            AND V.SourceType<>'OpeningBalance'";
            if (!string.IsNullOrEmpty(partyPlantId))
                cmdText += " AND VD.PartyPlantId='" + partyPlantId + "'";
            if (!string.IsNullOrEmpty(gSTINId))
                cmdText += " AND PP.GSTIN='" + gSTINId + "'";
            if (active)
                cmdText += " ORDER BY VD.GLGeneralInfoId, V.PostingDate, V.VoucherNo ASC";
            else
                cmdText += " ORDER BY V.PostingDate, V.VoucherNo ASC";

            return _sqlRepository.GetDataTable(cmdText);
        }

        private DataTable GetPartyData(string partyId, string partyPlantId)
        {
            var cmdText = @"SELECT P.Code ,P.UserName PartyName,PP.UserName PlantPartyName 
                            FROM HKP.Party P LEFT JOIN HKP.PartyPlant PP ON PP.PartyId=P.Id
                            WHERE P.Id='" + partyId + @"'";
            if (!string.IsNullOrEmpty(partyPlantId))
                cmdText += " AND PP.Id='" + partyPlantId + "'";
            return _sqlRepository.GetDataTable(cmdText);
        }
        private DataTable GetPartyOpeningBalanceGroupByGL(string companyGroupId, string companyId, string plantId, string partyId, string partyPlantId, string fromDate, string gl, string partyType)
        {
            string tempPartyType = null;
            if (partyType == "Vendor" || partyType == "Customer")
            {
                tempPartyType = partyType;
            }
            if (partyType == null || partyType == "null")
            {
                tempPartyType = "Vendor" + "','" + "Customer";
            }
            var sql = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                        SELECT SUM(DrAmount) - SUM(CrAmount) AS OB, CompanyCurrencyId, SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyCurrencyOB 
						, GLGeneralInfoId
						FROM (
                        SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        , VD.GLGeneralInfoId
						FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.GLGeneralInfoId='" + gl + "' AND VD.PartyId='" + partyId + "' AND VD.PartyType IN ('" + tempPartyType + "') AND V.PostingDate < '" + fromDate.ToDbDate() + "'";
            if (!string.IsNullOrEmpty(partyPlantId))
                sql += " AND VD.PartyPlantId='" + partyPlantId + "'";
            sql += @" GROUP BY CC.CompanyCurrencyId, VD.GLGeneralInfoId
                    UNION
                    SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount, CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                    , VD.GLGeneralInfoId
					FROM [TRN].[Voucher] AS V
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                    LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                    FROM [TRN].[VoucherDetailCurrency] AS VDC
	                    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                    ) AS CC ON CC.VoucherDetailId=VD.Id
                    WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.GLGeneralInfoId='" + gl + "' AND VD.PartyId='" + partyId + "' AND VD.PartyType IN ('" + tempPartyType + "') AND V.PostingDate ='" + fromDate.ToDbDate() + "' AND V.SourceType='OpeningBalance'";
            if (!string.IsNullOrEmpty(partyPlantId))
                sql += " AND VD.PartyPlantId='" + partyPlantId + "'";
            sql += " GROUP BY CC.CompanyCurrencyId ,VD.GLGeneralInfoId) AS X GROUP BY X.CompanyCurrencyId, X.GLGeneralInfoId";
            return _sqlRepository.GetDataTable(sql);

        }

        public IWorkbook GetPartyLedgerReportGroupByGL(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId)
        {
            try
            {
                var row = 6;
                var colLast = row;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";

                // Get BankMaster data
                var partyMaster = GetPartyData(partyId, partyPlantId).Select().FirstOrDefault();

                //// Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, partyMaster["Code"].ToString() + " - " + partyMaster["PartyName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(3) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                //var cashCurrencyId = partyMaster["CurrencyId"].ToString();
                var cashCurrencyId = companyCurrencyId.ToString();
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(6) + row + ":" + reportUtility.GetColumnNameForXls(8) + row].Merge();
                    colLast = 9;
                }

                // Detail Header
                row++;
                int COL = 1;
                reportUtility.SetHeaderText(ref sheet, row, COL, "GL", 20); int colGL = COL; COL++;//1
                reportUtility.SetHeaderText(ref sheet, row, COL, "Posting Date", 10); int colPostingDate = COL; COL++;//2
                reportUtility.SetHeaderText(ref sheet, row, COL, "Voucher No", 12); int colVoucherNo = COL; COL++;//3
                //reportUtility.SetHeaderText(ref sheet, row, COL, "Doc Ref", 10); int colDocRef = COL; COL++;//4
                //reportUtility.SetHeaderText(ref sheet, row, COL, "Doc Date", 10); int colDocDate = COL; COL++;//5
                reportUtility.SetHeaderText(ref sheet, row, COL, "Particulars", 30); int colParticulars = COL; COL++;//6


                reportUtility.SetHeaderText(ref sheet, row, COL, "Narration", 20); int colNarration = COL; COL++;//7
                reportUtility.SetHeaderText(ref sheet, row, COL, "Debit", 14, ExcelHAlign.HAlignRight); int colDebit = COL; COL++;//8
                reportUtility.SetHeaderText(ref sheet, row, COL, "Credit", 14, ExcelHAlign.HAlignRight); int colCredit = COL; COL++;//9
                reportUtility.SetHeaderText(ref sheet, row, COL, "Balance", 12, ExcelHAlign.HAlignRight); int colBalance = COL; COL++;//10
                reportUtility.SetHeaderText(ref sheet, row, COL, "Dr/Cr", 4, ExcelHAlign.HAlignRight); int colCrDr = COL; //COL++;//11

                //reportUtility.SetHeaderText(ref sheet, row, 11, "Party Balance", 14, ExcelHAlign.HAlignRight);
                //reportUtility.SetHeaderText(ref sheet, row, 12, "Dr/Cr", 4, ExcelHAlign.HAlignRight);

                int colDebit2 = 0;
                int colCredit2 = 0;
                int colBalance2 = 0;


                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    COL = colCrDr;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Debit", 14, ExcelHAlign.HAlignRight);colDebit2 = COL; COL++;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Credit", 14, ExcelHAlign.HAlignRight);  colCredit2 = COL; COL++;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Balance", ExcelHAlign.HAlignRight); colBalance2 = COL; COL++;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Dr/Cr", 4, ExcelHAlign.HAlignRight); colCrDr = COL;

                }
                colLast = COL;
                row++;

                // Get Cash transaction data.
                var ledgerData = GetPartyPlantLedger(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId, partyType.ToString());
                var obValParty = GetPartyOpeningBalance(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, partyType.ToString());
                var clValParty = GetPartyOpeningBalance(companyGroupId, companyId, plantId, partyId, partyPlantId, toDate, partyType.ToString());

                if (ledgerData.Rows.Count > 0)
                {
                    var dt = ledgerData.AsEnumerable().OrderBy(r => r["GLGeneralInfoId"])
                            .GroupBy(r => new { GLGeneralInfoId = r["GLGeneralInfoId"] })
                            .Select(g => g.OrderBy(r => r["GLGeneralInfoId"]).First())
                            .CopyToDataTable();
                    var isOB = true;
                    var lastClosing = string.Empty;

                    reportUtility.SetTextLeftAlign(ref sheet, row, colGL, "Party Opening Balance", true, ExcelHAlign.HAlignLeft);
                    sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

                    if (obValParty.Count > 0)
                    {
                        var obparty = Convert.ToDouble(obValParty[0]["OB"]); ;

                        reportUtility.SetText(ref sheet, row,colCredit , obparty, true);
                        sheet.Range[row, colCredit].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        row++;
                    }


                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        var data = ledgerData.AsEnumerable()
                            .Where(r => r.Field<string>("GLGeneralInfoId") == dt.Rows[j]["GLGeneralInfoId"].ToString())
                            .OrderBy(r => r["PostingDate"])
                            .CopyToDataTable();

                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colPostingDate) + row].Merge();
                        reportUtility.SetText(ref sheet, row, colGL, data.Rows[0]["GLGeneralInfoCode"].ToString() + "-" + data.Rows[0]["GLGeneralInfoName"].ToString());
                        sheet.Range[row, colGL].CellStyle.Font.Bold = true;
                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colCrDr) + row].BorderAround(ExcelLineStyle.Hair);
                        row++;

                        reportUtility.SetText(ref sheet, row, colGL, "Opening Balance", true);
                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

                        // Get Cash opening balance data.
                        //if (obVal.Rows.Count > 0)//&& isOB
                        //{
                        var obVal = GetPartyOpeningBalanceGroupByGL(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, dt.Rows[j]["GLGeneralInfoId"].ToString(), partyType.ToString()).Select().FirstOrDefault();
                        if (obVal != null)
                        {
                            var ob = Convert.ToDouble(obVal["OB"]); 
                            reportUtility.SetText(ref sheet, row, colBalance, ob, true);
                            sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colBalance) + row + ">= 0, \"  Dr\", \"  Cr\")";

                            if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                            {
                                reportUtility.SetText(ref sheet, row, colBalance2, ob, true);
                                sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colBalance2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                            }

                            isOB = false;
                        }
                        row++;
                        for (int i = 0; i < data.Rows.Count; i++)
                        {

                            reportUtility.SetText(ref sheet, row, colGL, data.Rows[i]["GLGeneralInfoCode"].ToString() + "-" + data.Rows[i]["GLGeneralInfoName"].ToString());
                            reportUtility.SetText(ref sheet, row, colPostingDate, data.Rows[i]["PostingDate"].ToString());
                            reportUtility.SetTextWrapText(ref sheet, row, colVoucherNo, data.Rows[i]["VoucherNo"].ToString());
                            ////reportUtility.SetTextWrapText(ref sheet, row, colDocRef, data.Rows[i]["DocRefNo"].ToString());
                            ////reportUtility.SetTextWrapText(ref sheet, row, colDocDate, data.Rows[i]["DocDate"].ToString());

                            reportUtility.SetTextWrapText(ref sheet, row, colParticulars, data.Rows[i]["Particular"].ToString());

                            reportUtility.SetTextWrapText(ref sheet, row, colNarration, data.Rows[i]["Narration"].ToString());
                            sheet[row, colNarration].ColumnWidth = 20;
                            sheet.Range[row, colNarration].WrapText = true;
                            reportUtility.SetText(ref sheet, row, colDebit, Convert.ToDouble(data.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, colCredit, Convert.ToDouble(data.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                            sheet.Range[row, colBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colDebit) + row + "-" + reportUtility.GetColumnNameForXls(colCredit) + row + ")";
                            sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet.Range[row, colBalance].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";

                            // Base currency checking
                            if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                            {
                                reportUtility.SetText(ref sheet, row, colDebit2, Convert.ToDouble(data.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                                reportUtility.SetText(ref sheet, row, colCredit2, Convert.ToDouble(data.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                                sheet.Range[row, colBalance2].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colCredit2) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(10) + row + "-" + reportUtility.GetColumnNameForXls(11) + row + ")";
                                sheet.Range[row, colBalance2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                                sheet.Range[row, colBalance2].VerticalAlignment = ExcelVAlign.VAlignTop;
                                sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                            }
                            sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                            sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                            row++;
                        }

                        reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);

                        //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                        //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                        //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                        //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                        //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                        //sheet[row, 7].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(7) + (row - 1) + ":" + clsStaticInfo.GetxlsCol(7) + row + ")";
                        sheet.Range[row, colNarration].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colNarration) + (row - 1) + ":" + reportUtility.GetColumnNameForXls(colNarration) + row + ")";
                        // sheet.Range[row, 7].NumberFormat = oRU.NumberFormatDecimalTwo();
                        sheet.Range[row, colNarration].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();

                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

                        sheet.Range[row, colCredit].Formula = "=" + reportUtility.GetColumnNameForXls(colBalance) + (row - 1);
                        lastClosing = "=" + reportUtility.GetColumnNameForXls(7) + (row - 1);
                        sheet.Range[row, colCredit].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCredit].CellStyle.Font.Bold = true;
                        sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";

                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                        {
                            sheet.Range[row, colCredit2].Formula = "=" + reportUtility.GetColumnNameForXls(colCredit2) + (row - 1);
                            sheet.Range[row, colCredit2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet.Range[row, colCredit2].CellStyle.Font.Bold = true;
                            sheet.Range[row, colBalance2].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        }
                        row++;

                    }
                    row++;
                    reportUtility.SetTextLeftAlign(ref sheet, row, colGL, "Party Closing Balance", true, ExcelHAlign.HAlignLeft);
                    sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();
                    if (clValParty.Count > 0)
                    {
                        var clparty = Convert.ToDouble(clValParty[0]["OB"]); ;
                        reportUtility.SetText(ref sheet, row, colBalance, clparty, true);
                        sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        row++;
                    }
                    //sheet.Range[row, 9].Formula = "=" + reportUtility.GetColumnNameForXls(9) + (row - 1);
                    //lastClosing = "=" + reportUtility.GetColumnNameForXls(9) + (row - 1);
                    //sheet.Range[row, 9].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                    //sheet.Range[row, 9].CellStyle.Font.Bold = true;
                    //sheet.Range[row, 10].Formula = "IF(" + reportUtility.GetColumnNameForXls(10 - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                    if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                    {
                        sheet.Range[row, colCredit2].Formula = "=" + reportUtility.GetColumnNameForXls(colCredit2) + (row - 1);
                        sheet.Range[row, colCredit2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCredit2].CellStyle.Font.Bold = true;
                        sheet.Range[row, colBalance2].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                    }

                }
                sheet.UsedRange.WrapText = true;
                //sheet.UsedRange.AutofitRows();
               
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Party Ledger", companyId,plantId,plantName, null);
                reportUtility.SetText(ref sheet, 5, colLast, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(colGL) + 5 + ":" + reportUtility.GetColumnNameForXls(colLast) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetPartyLedgerReportGroupByGLXls(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId)
        {
            try
            {
                var row = 6;
                var colLast = row;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";

                // Get BankMaster data
                var partyMaster = GetPartyData(partyId, partyPlantId).Select().FirstOrDefault();

                //// Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, partyMaster["Code"].ToString() + " - " + partyMaster["PartyName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(3) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                //var cashCurrencyId = partyMaster["CurrencyId"].ToString();
                var cashCurrencyId = companyCurrencyId.ToString();
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(6) + row + ":" + reportUtility.GetColumnNameForXls(8) + row].Merge();
                    colLast = 9;
                }

                // Detail Header
                row++;
                int COL = 1;
                reportUtility.SetHeaderText(ref sheet, row, COL, "GL", 20); int colGL = COL; COL++;//1
                reportUtility.SetHeaderText(ref sheet, row, COL, "Posting Date", 10); int colPostingDate = COL; COL++;//2
                reportUtility.SetHeaderText(ref sheet, row, COL, "Voucher No", 10); int colVoucherNo = COL; COL++;//3
                //reportUtility.SetHeaderText(ref sheet, row, COL, "Doc Ref", 10); int colDocRef = COL; COL++;//4
                //reportUtility.SetHeaderText(ref sheet, row, COL, "Doc Date", 10); int colDocDate = COL; COL++;//5
                reportUtility.SetHeaderText(ref sheet, row, COL, "Particulars", 20); int colParticulars = COL; COL++;//6


                reportUtility.SetHeaderText(ref sheet, row, COL, "Narration", 20); int colNarration = COL; COL++;//7
                reportUtility.SetHeaderText(ref sheet, row, COL, "Debit", 10, ExcelHAlign.HAlignRight); int colDebit = COL; COL++;//8
                reportUtility.SetHeaderText(ref sheet, row, COL, "Credit", 10, ExcelHAlign.HAlignRight); int colCredit = COL; COL++;//9
                reportUtility.SetHeaderText(ref sheet, row, COL, "Balance", 10, ExcelHAlign.HAlignRight); int colBalance = COL; COL++;//10
                reportUtility.SetHeaderText(ref sheet, row, COL, "Dr/Cr", 4, ExcelHAlign.HAlignRight); int colCrDr = COL; COL++;//11

      
                int colDebit2 = 0;
                int colCredit2 = 0;
                int colBalance2 = 0;


                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    COL = colCrDr;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Debit", 10, ExcelHAlign.HAlignRight); colDebit2 = COL; COL++;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Credit", 10, ExcelHAlign.HAlignRight); colCredit2 = COL; COL++;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Balance",10, ExcelHAlign.HAlignRight); colBalance2 = COL; COL++;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Dr/Cr", 4, ExcelHAlign.HAlignRight); colCrDr = COL;

                }
                colLast = COL;
                row++;

                // Get Cash transaction data.
                var ledgerData = GetPartyPlantLedger(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId, partyType.ToString());
                var obValParty = GetPartyOpeningBalance(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, partyType.ToString());
                var clValParty = GetPartyOpeningBalance(companyGroupId, companyId, plantId, partyId, partyPlantId, toDate, partyType.ToString());


                if (ledgerData.Rows.Count > 0)
                {
                    var dt = ledgerData.AsEnumerable().OrderBy(r => r["GLGeneralInfoId"])
                            .GroupBy(r => new { GLGeneralInfoId = r["GLGeneralInfoId"] })
                            .Select(g => g.OrderBy(r => r["GLGeneralInfoId"]).First())
                            .CopyToDataTable();
                    var isOB = true;
                    var lastClosing = string.Empty;

                    reportUtility.SetTextLeftAlign(ref sheet, row, colGL, "Party Opening Balance", true, ExcelHAlign.HAlignLeft);
                    sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

                    if (obValParty.Count > 0)
                    {
                        var obparty = Convert.ToDouble(obValParty[0]["OB"]); ;

                        reportUtility.SetText(ref sheet, row, colCredit, obparty, true);
                        sheet.Range[row, colCredit].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        row++;
                    }


                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        var data = ledgerData.AsEnumerable()
                            .Where(r => r.Field<string>("GLGeneralInfoId") == dt.Rows[j]["GLGeneralInfoId"].ToString())
                            .OrderBy(r => r["PostingDate"])
                            .CopyToDataTable();

                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colPostingDate) + row].Merge();
                        reportUtility.SetText(ref sheet, row, colGL, data.Rows[0]["GLGeneralInfoCode"].ToString() + "-" + data.Rows[0]["GLGeneralInfoName"].ToString());
                        sheet.Range[row, colGL].CellStyle.Font.Bold = true;
                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colCrDr) + row].BorderAround(ExcelLineStyle.Hair);
                        row++;

                        reportUtility.SetText(ref sheet, row, colGL, "Opening Balance", true);
                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

                        // Get Cash opening balance data.
                        //if (obVal.Rows.Count > 0)//&& isOB
                        //{
                        var obVal = GetPartyOpeningBalanceGroupByGL(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, dt.Rows[j]["GLGeneralInfoId"].ToString(), partyType.ToString()).Select().FirstOrDefault();
                        if (obVal != null)
                        {
                            var ob = Convert.ToDouble(obVal["OB"]); ;
                            reportUtility.SetText(ref sheet, row, colCredit, ob, true);
                            sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet.Range[row, colCredit].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";

                            if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                            {
                                reportUtility.SetText(ref sheet, row, colCredit2, ob, true);
                                sheet.Range[row, colBalance2].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                            }

                            isOB = false;
                        }
                        row++;
                        for (int i = 0; i < data.Rows.Count; i++)
                        {

                            reportUtility.SetText(ref sheet, row, colGL, data.Rows[i]["GLGeneralInfoCode"].ToString() + "-" + data.Rows[i]["GLGeneralInfoName"].ToString());
                            reportUtility.SetText(ref sheet, row, colPostingDate, data.Rows[i]["PostingDate"].ToString());
                            reportUtility.SetTextWrapText(ref sheet, row, colVoucherNo, data.Rows[i]["VoucherNo"].ToString());
                            ////reportUtility.SetTextWrapText(ref sheet, row, colDocRef, data.Rows[i]["DocRefNo"].ToString());
                            ////reportUtility.SetTextWrapText(ref sheet, row, colDocDate, data.Rows[i]["DocDate"].ToString());

                            reportUtility.SetTextWrapText(ref sheet, row, colParticulars, data.Rows[i]["Particular"].ToString());

                            reportUtility.SetTextWrapText(ref sheet, row, colNarration, data.Rows[i]["Narration"].ToString());
                            sheet[row, colNarration].ColumnWidth = 20;
                            sheet.Range[row, colNarration].WrapText = true;
                            reportUtility.SetText(ref sheet, row, colDebit, Convert.ToDouble(data.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, colCredit, Convert.ToDouble(data.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                            sheet.Range[row, colBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colDebit) + row + "-" + reportUtility.GetColumnNameForXls(colCredit) + row + ")";
                            sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet.Range[row, colBalance].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";

                            // Base currency checking
                            if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                            {
                                reportUtility.SetText(ref sheet, row, colDebit2, Convert.ToDouble(data.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                                reportUtility.SetText(ref sheet, row, colCredit2, Convert.ToDouble(data.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                                sheet.Range[row, colBalance2].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colCredit2) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(10) + row + "-" + reportUtility.GetColumnNameForXls(11) + row + ")";
                                sheet.Range[row, colBalance2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                                sheet.Range[row, colBalance2].VerticalAlignment = ExcelVAlign.VAlignTop;
                                sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                            }
                            row++;
                        }

                        reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);

                        //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                        //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                        //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                        //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                        //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                        //sheet[row, 7].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(7) + (row - 1) + ":" + clsStaticInfo.GetxlsCol(7) + row + ")";
                        sheet.Range[row, colNarration].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colNarration) + (row - 1) + ":" + reportUtility.GetColumnNameForXls(colNarration) + row + ")";
                        // sheet.Range[row, 7].NumberFormat = oRU.NumberFormatDecimalTwo();
                        sheet.Range[row, colNarration].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();

                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

                        sheet.Range[row, colCredit].Formula = "=" + reportUtility.GetColumnNameForXls(colBalance) + (row - 1);
                        lastClosing = "=" + reportUtility.GetColumnNameForXls(7) + (row - 1);
                        sheet.Range[row, colCredit].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCredit].CellStyle.Font.Bold = true;
                        sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";

                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                        {
                            sheet.Range[row, colCredit2].Formula = "=" + reportUtility.GetColumnNameForXls(colCredit2) + (row - 1);
                            sheet.Range[row, colCredit2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet.Range[row, colCredit2].CellStyle.Font.Bold = true;
                            sheet.Range[row, colBalance2].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        }
                        row++;

                    }
                    row++;
                    reportUtility.SetTextLeftAlign(ref sheet, row, colGL, "Party Closing Balance", true, ExcelHAlign.HAlignLeft);
                    sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();
                    if (clValParty.Count > 0)
                    {
                        var clparty = Convert.ToDouble(clValParty[0]["OB"]); ;
                        reportUtility.SetText(ref sheet, row, colBalance, clparty, true);
                        sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        row++;
                    }
                    //sheet.Range[row, 9].Formula = "=" + reportUtility.GetColumnNameForXls(9) + (row - 1);
                    //lastClosing = "=" + reportUtility.GetColumnNameForXls(9) + (row - 1);
                    //sheet.Range[row, 9].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                    //sheet.Range[row, 9].CellStyle.Font.Bold = true;
                    //sheet.Range[row, 10].Formula = "IF(" + reportUtility.GetColumnNameForXls(10 - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                    if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                    {
                        sheet.Range[row, colCredit2].Formula = "=" + reportUtility.GetColumnNameForXls(colCredit2) + (row - 1);
                        sheet.Range[row, colCredit2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCredit2].CellStyle.Font.Bold = true;
                        sheet.Range[row, colBalance2].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                    }

                }
                sheet.UsedRange.WrapText = true;
                //sheet.UsedRange.AutofitRows();

                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Party Ledger", companyId, plantId, plantName, null);
                reportUtility.SetText(ref sheet, 5, colLast, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(colGL) + 5 + ":" + reportUtility.GetColumnNameForXls(colLast) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetPartyLedgerReportGroupByGLReportLongSizeXls(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId)
        {
            try
            {
                var row = 6;
                var colLast = row;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";

                // Get BankMaster data
                var partyMaster = GetPartyData(partyId, partyPlantId).Select().FirstOrDefault();

                //// Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, partyMaster["Code"].ToString() + " - " + partyMaster["PartyName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(3) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                //var cashCurrencyId = partyMaster["CurrencyId"].ToString();
                var cashCurrencyId = companyCurrencyId.ToString();
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(6) + row + ":" + reportUtility.GetColumnNameForXls(8) + row].Merge();
                    colLast = 9;
                }

                // Detail Header
                row++;
                int COL = 1;
                reportUtility.SetHeaderText(ref sheet, row, COL, "GL", 20); int colGL = COL; COL++;//1
                reportUtility.SetHeaderText(ref sheet, row, COL, "Posting Date", 10); int colPostingDate = COL; COL++;//2
                reportUtility.SetHeaderText(ref sheet, row, COL, "Voucher No", 10); int colVoucherNo = COL; COL++;//3
                reportUtility.SetHeaderText(ref sheet, row, COL, "Doc Ref", 10); int colDocRef = COL; COL++;//4
                reportUtility.SetHeaderText(ref sheet, row, COL, "Doc Date", 10); int colDocDate = COL; COL++;//5
                reportUtility.SetHeaderText(ref sheet, row, COL, "Particulars", 20); int colParticulars = COL; COL++;//6


                reportUtility.SetHeaderText(ref sheet, row, COL, "Narration", 20); int colNarration = COL; COL++;//7
                reportUtility.SetHeaderText(ref sheet, row, COL, "Debit", 10, ExcelHAlign.HAlignRight); int colDebit = COL; COL++;//8
                reportUtility.SetHeaderText(ref sheet, row, COL, "Credit", 10, ExcelHAlign.HAlignRight); int colCredit = COL; COL++;//9
                reportUtility.SetHeaderText(ref sheet, row, COL, "Balance", 10, ExcelHAlign.HAlignRight); int colBalance = COL; COL++;//10
                reportUtility.SetHeaderText(ref sheet, row, COL, "Dr/Cr", 4, ExcelHAlign.HAlignRight); int colCrDr = COL; COL++;//11


                int colDebit2 = 0;
                int colCredit2 = 0;
                int colBalance2 = 0;


                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    COL = colCrDr;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Debit", 10, ExcelHAlign.HAlignRight); colDebit2 = COL; COL++;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Credit", 10, ExcelHAlign.HAlignRight); colCredit2 = COL; COL++;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Balance", 10, ExcelHAlign.HAlignRight); colBalance2 = COL; COL++;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Dr/Cr", 4, ExcelHAlign.HAlignRight); colCrDr = COL;

                }
                colLast = COL;
                row++;

                // Get Cash transaction data.
                var ledgerData = GetPartyPlantLedger(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId, partyType.ToString());
                var obValParty = GetPartyOpeningBalance(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, partyType.ToString());
                var clValParty = GetPartyOpeningBalance(companyGroupId, companyId, plantId, partyId, partyPlantId, toDate, partyType.ToString());


                if (ledgerData.Rows.Count > 0)
                {
                    var dt = ledgerData.AsEnumerable().OrderBy(r => r["GLGeneralInfoId"])
                            .GroupBy(r => new { GLGeneralInfoId = r["GLGeneralInfoId"] })
                            .Select(g => g.OrderBy(r => r["GLGeneralInfoId"]).First())
                            .CopyToDataTable();
                    var isOB = true;
                    var lastClosing = string.Empty;

                    reportUtility.SetTextLeftAlign(ref sheet, row, colGL, "Party Opening Balance", true, ExcelHAlign.HAlignLeft);
                    sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

                    if (obValParty.Count > 0)
                    {
                        var obparty = Convert.ToDouble(obValParty[0]["OB"]); ;

                        reportUtility.SetText(ref sheet, row, colCredit, obparty, true);
                        sheet.Range[row, colCredit].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        row++;
                    }


                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        var data = ledgerData.AsEnumerable()
                            .Where(r => r.Field<string>("GLGeneralInfoId") == dt.Rows[j]["GLGeneralInfoId"].ToString())
                            .OrderBy(r => r["PostingDate"])
                            .CopyToDataTable();

                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colPostingDate) + row].Merge();
                        reportUtility.SetText(ref sheet, row, colGL, data.Rows[0]["GLGeneralInfoCode"].ToString() + "-" + data.Rows[0]["GLGeneralInfoName"].ToString());
                        sheet.Range[row, colGL].CellStyle.Font.Bold = true;
                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colCrDr) + row].BorderAround(ExcelLineStyle.Hair);
                        row++;

                        reportUtility.SetText(ref sheet, row, colGL, "Opening Balance", true);
                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

                        // Get Cash opening balance data.
                        //if (obVal.Rows.Count > 0)//&& isOB
                        //{
                        var obVal = GetPartyOpeningBalanceGroupByGL(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, dt.Rows[j]["GLGeneralInfoId"].ToString(), partyType.ToString()).Select().FirstOrDefault();
                        if (obVal != null)
                        {
                            var ob = Convert.ToDouble(obVal["OB"]); ;
                            reportUtility.SetText(ref sheet, row, colCredit, ob, true);
                            sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet.Range[row, colCredit].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";

                            if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                            {
                                reportUtility.SetText(ref sheet, row, colCredit2, ob, true);
                                sheet.Range[row, colBalance2].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                            }

                            isOB = false;
                        }
                        row++;
                        for (int i = 0; i < data.Rows.Count; i++)
                        {

                            reportUtility.SetText(ref sheet, row, colGL, data.Rows[i]["GLGeneralInfoCode"].ToString() + "-" + data.Rows[i]["GLGeneralInfoName"].ToString());
                            reportUtility.SetText(ref sheet, row, colPostingDate, data.Rows[i]["PostingDate"].ToString());
                            reportUtility.SetTextWrapText(ref sheet, row, colVoucherNo, data.Rows[i]["VoucherNo"].ToString());
                            reportUtility.SetTextWrapText(ref sheet, row, colDocRef, data.Rows[i]["DocRefNo"].ToString());
                            reportUtility.SetTextWrapText(ref sheet, row, colDocDate, data.Rows[i]["DocDate"].ToString());

                            reportUtility.SetTextWrapText(ref sheet, row, colParticulars, data.Rows[i]["Particular"].ToString());

                            reportUtility.SetTextWrapText(ref sheet, row, colNarration, data.Rows[i]["Narration"].ToString());
                            sheet[row, colNarration].ColumnWidth = 20;
                            sheet.Range[row, colNarration].WrapText = true;
                            reportUtility.SetText(ref sheet, row, colDebit, Convert.ToDouble(data.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, colCredit, Convert.ToDouble(data.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                            sheet.Range[row, colBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colDebit) + row + "-" + reportUtility.GetColumnNameForXls(colCredit) + row + ")";
                            sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet.Range[row, colBalance].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";

                            // Base currency checking
                            if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                            {
                                reportUtility.SetText(ref sheet, row, colDebit2, Convert.ToDouble(data.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                                reportUtility.SetText(ref sheet, row, colCredit2, Convert.ToDouble(data.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                                sheet.Range[row, colBalance2].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colCredit2) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(10) + row + "-" + reportUtility.GetColumnNameForXls(11) + row + ")";
                                sheet.Range[row, colBalance2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                                sheet.Range[row, colBalance2].VerticalAlignment = ExcelVAlign.VAlignTop;
                                sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                            }
                            row++;
                        }

                        reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);

                        //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                        //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                        //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                        //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                        //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                        //sheet[row, 7].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(7) + (row - 1) + ":" + clsStaticInfo.GetxlsCol(7) + row + ")";
                        sheet.Range[row, colNarration].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colNarration) + (row - 1) + ":" + reportUtility.GetColumnNameForXls(colNarration) + row + ")";
                        // sheet.Range[row, 7].NumberFormat = oRU.NumberFormatDecimalTwo();
                        sheet.Range[row, colNarration].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();

                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

                        sheet.Range[row, colCredit].Formula = "=" + reportUtility.GetColumnNameForXls(colBalance) + (row - 1);
                        lastClosing = "=" + reportUtility.GetColumnNameForXls(7) + (row - 1);
                        sheet.Range[row, colCredit].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCredit].CellStyle.Font.Bold = true;
                        sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";

                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                        {
                            sheet.Range[row, colCredit2].Formula = "=" + reportUtility.GetColumnNameForXls(colCredit2) + (row - 1);
                            sheet.Range[row, colCredit2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet.Range[row, colCredit2].CellStyle.Font.Bold = true;
                            sheet.Range[row, colBalance2].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        }
                        row++;

                    }
                    row++;
                    reportUtility.SetTextLeftAlign(ref sheet, row, colGL, "Party Closing Balance", true, ExcelHAlign.HAlignLeft);
                    sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();
                    if (clValParty.Count > 0)
                    {
                        var clparty = Convert.ToDouble(clValParty[0]["OB"]); ;
                        reportUtility.SetText(ref sheet, row, colBalance, clparty, true);
                        sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        row++;
                    }
                    //sheet.Range[row, 9].Formula = "=" + reportUtility.GetColumnNameForXls(9) + (row - 1);
                    //lastClosing = "=" + reportUtility.GetColumnNameForXls(9) + (row - 1);
                    //sheet.Range[row, 9].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                    //sheet.Range[row, 9].CellStyle.Font.Bold = true;
                    //sheet.Range[row, 10].Formula = "IF(" + reportUtility.GetColumnNameForXls(10 - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                    if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                    {
                        sheet.Range[row, colCredit2].Formula = "=" + reportUtility.GetColumnNameForXls(colCredit2) + (row - 1);
                        sheet.Range[row, colCredit2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCredit2].CellStyle.Font.Bold = true;
                        sheet.Range[row, colBalance2].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                    }

                }
                sheet.UsedRange.WrapText = true;
                //sheet.UsedRange.AutofitRows();

                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Party Ledger", companyId, plantId, plantName, null);
                reportUtility.SetText(ref sheet, 5, colLast, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(colGL) + 5 + ":" + reportUtility.GetColumnNameForXls(colLast) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetPartyCategoryLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string partyType, string partyCategoryId, string fromDate, string toDate)
        {
            try
            {
                ReportUtility reportUtility = new ReportUtility();
                string Budgetsql = PartyCategorySql(partyType, partyCategoryId, fromDate, toDate);
                var gl = _partyCategoryService.Find(partyCategoryId);
                //var budget = _budgetMasterService.GetBudgetMasterData(budgetMasterId);

                //Instantiate the Excel application object
                //DataTable dtGroupBalance = _sqlRepository.GetDataTable(sql);
                DataTable dtGroupBalanceBudgets = _sqlRepository.GetDataTable(Budgetsql);
                
                var dtGroupBalanceBudget = dtGroupBalanceBudgets.AsEnumerable()
                        .OrderBy(r => r["PartyName"])
                        .CopyToDataTable();

                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Party Category Report";


                int ROW = 6;
                int COL = 1;

                #region Header
                sheet[ROW, COL].Text = "Party Type :";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                COL++;
                sheet[ROW, COL].Text = partyType.ToString();
                sheet.Range[reportUtility.GetColumnNameForXls(COL) + ROW + ":" + reportUtility.GetColumnNameForXls(COL + 1) + ROW].Merge();
                int colAccountType = COL;
                COL++;
                

                //sheet[ROW, COL].Text = "Account Group :";
                //sheet[ROW, COL].CellStyle.Font.Bold = true;
                ////sheet.Range[reportUtility.GetColumnNameForXls(COL) + ROW + ":" + reportUtility.GetColumnNameForXls(COL + 1) + ROW].Merge();
                //COL++;
                //sheet[ROW, COL].Text = gl["AccountGroupName"].ToString();
                //sheet.Range[reportUtility.GetColumnNameForXls(COL) + ROW + ":" + reportUtility.GetColumnNameForXls(COL + 2) + ROW].Merge();
                //int colAccountGroup = COL;
                //ROW++;
                //COL = 1;
                //sheet[ROW, COL].Text = "GL:";
                //sheet[ROW, COL].CellStyle.Font.Bold = true;
                //COL++;

                //sheet[ROW, COL].Text = gl["GLGeneralInfoCode"] + " - " + gl["GLGeneralInfoName"];
                //sheet.Range[reportUtility.GetColumnNameForXls(COL) + ROW + ":" + reportUtility.GetColumnNameForXls(COL + 1) + ROW].Merge();
                //int colGL = COL;

                

                ROW++;
                COL = 1;
                #endregion
                int colActivity = 0;
               
               
                sheet[ROW, COL].Text = "Party";
                sheet[ROW, COL].ColumnWidth = 30;
                colActivity = COL;
                COL++;
                
                sheet[ROW, COL].Text = "Openning Balance";
                sheet[ROW, COL].ColumnWidth = 18;
                int colOpenningCR = COL;
                COL++;
                sheet[ROW, COL].Text = "Dr/Cr";
                sheet[ROW, COL].ColumnWidth = 5;
                int colOpenningDRCR = COL;
                COL++;
                sheet[ROW, COL].Text = "Periodic Dr.";
                sheet[ROW, COL].ColumnWidth = 18;
                int colPeriodicDr = COL;
                COL++;
                sheet[ROW, COL].Text = "Periodic Cr.";
                sheet[ROW, COL].ColumnWidth = 18;
                int colPeriodicCR = COL;
                COL++;
                sheet[ROW, COL].Text = "Balance";
                sheet[ROW, COL].ColumnWidth = 18;
                int colBalanceDrCr = COL;
                COL++;
                sheet[ROW, COL].Text = "Dr/Cr";
                sheet[ROW, COL].ColumnWidth = 5;
                int colCRDR = COL;

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                int StartRow = ROW; //row 20
                
                    for (int i = 0; i < dtGroupBalanceBudget.Rows.Count; i++)
                    {

                        sheet[ROW, colActivity].Text = dtGroupBalanceBudget.Rows[i]["PartyName"].ToString();
                        sheet[ROW, colOpenningCR].Number = clsStaticInfo.dbl(dtGroupBalanceBudget.Rows[i]["PartyOpeningBalance"].ToString());
                        sheet.Range[ROW, colOpenningCR].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet[ROW, colPeriodicDr].Number = clsStaticInfo.dbl(dtGroupBalanceBudget.Rows[i]["CompanyCurrencyDrAmount"].ToString());
                        sheet[ROW, colPeriodicDr].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, colPeriodicCR].Number = clsStaticInfo.dbl(dtGroupBalanceBudget.Rows[i]["CompanyCurrencyCrAmount"].ToString());
                        sheet[ROW, colPeriodicCR].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, colBalanceDrCr].Number = clsStaticInfo.dbl(dtGroupBalanceBudget.Rows[i]["PartyClosingBalance"].ToString());
                        sheet.Range[ROW, colBalanceDrCr].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        if (Convert.ToInt32(dtGroupBalanceBudget.Rows[i]["PartyClosingBalance"]) != 0)
                        {
                            sheet[ROW, colCRDR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCRDR - 1) + ROW + ">= 0, \"Dr\", \"Cr\")";
                            sheet[ROW, colCRDR].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        }
                        if (Convert.ToInt32(dtGroupBalanceBudget.Rows[i]["PartyOpeningBalance"]) != 0)
                        {
                            sheet[ROW, colOpenningDRCR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colOpenningDRCR - 1) + ROW + ">= 0, \"Dr\", \"Cr\")";
                            sheet[ROW, colOpenningDRCR].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        }

                        sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                        ROW++;

                    }
                
                
                sheet[ROW, 1].Text = "Total :";
                sheet[ROW, colOpenningCR].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colOpenningCR) + (StartRow) + ":" + reportUtility.GetColumnNameForXls(colOpenningCR) + (ROW - 1) + ")";
                sheet.Range[ROW, colOpenningCR].CellStyle.Font.Bold = true;
                //sheet[ROW, colOpenningCR].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet.Range[ROW, colOpenningCR].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();


                sheet[ROW, colPeriodicDr].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colPeriodicDr) + (StartRow) + ":" + reportUtility.GetColumnNameForXls(colPeriodicDr) + (ROW - 1) + ")";
                sheet.Range[ROW, colPeriodicDr].CellStyle.Font.Bold = true;
                //sheet[ROW, colPeriodicDr].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet.Range[ROW, colPeriodicDr].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();


                sheet[ROW, colBalanceDrCr].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBalanceDrCr) + (StartRow) + ":" + reportUtility.GetColumnNameForXls(colBalanceDrCr) + (ROW - 1) + ")";
                sheet.Range[ROW, colBalanceDrCr].CellStyle.Font.Bold = true;
                //sheet[ROW, colBalanceDrCr].NumberFormat = "#,##0.00;(#,##0.00)";

                sheet.Range[ROW, colBalanceDrCr].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //  " IF(B125 > 0, "Dr", IF(B125 < 0, "Cr", IF(B125 = 0, "")));
                sheet[ROW, colCRDR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCRDR - 1) + ROW + "> 0, \"Dr\",IF(" + reportUtility.GetColumnNameForXls(colCRDR - 1) + ROW + "< 0,\"Cr\",IF(" + reportUtility.GetColumnNameForXls(colCRDR - 1) + ROW + "= 0 ,\" \")))";
                sheet[ROW, colCRDR].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet[ROW, colCRDR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCRDR - 1) + ROW + "> 0, \"Dr\", \"Cr\")";
                //sheet[ROW, colCRDR].HorizontalAlignment = ExcelHAlign.HAlignRight;

                //sheet[ROW, colOpenningDRCR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colOpenningDRCR - 1) + ROW + "> 0, \"Dr\", \"Cr\")";
                sheet[ROW, colOpenningDRCR].Formula = "IF(" + reportUtility.GetColumnNameForXls(colOpenningDRCR - 1) + ROW + "> 0, \"Dr\",IF(" + reportUtility.GetColumnNameForXls(colOpenningDRCR - 1) + ROW + "< 0,\"Cr\",IF(" + reportUtility.GetColumnNameForXls(colOpenningDRCR - 1) + ROW + "= 0 ,\" \")))";
                sheet[ROW, colOpenningDRCR].HorizontalAlignment = ExcelHAlign.HAlignRight;


                sheet[ROW, colPeriodicCR].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colPeriodicCR) + (StartRow) + ":" + reportUtility.GetColumnNameForXls(colPeriodicCR) + (ROW - 1) + ")";
                sheet.Range[ROW, colPeriodicCR].CellStyle.Font.Bold = true;
                //sheet[ROW, colPeriodicCR].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet.Range[ROW, colPeriodicCR].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();

                sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                //sheet["A" + StartRow.ToString()].FreezePanes();

                //reportUtility.PlantHeader(ref sheet, endCol, "Group Balance", identity.PlantId);
                reportUtility.CompanyPlantHeader(ref sheet, endCol, gl.UserName.ToString(), companyId, plantName, null);
                reportUtility.SetText(ref sheet, 5, 1, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                //sheet.Range[ROW, COL, ROW, endCol].CellStyle.Font.Bold = true;
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                //string strFileName = "Party Group Report.xls";
                
                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        private string PartyCategorySql(string partyType, string partyCategoryId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"DECLARE @companyId VARCHAR(10)='" + identity.CompanyId + @"';
                            SELECT 
                             SUM(ISNULL(CC.CompanyCurrencyDrAmount, 0)) AS CompanyCurrencyDrAmount, SUM(ISNULL(CC.CompanyCurrencyCrAmount, 0)) AS CompanyCurrencyCrAmount,P.Id PartyId, P.UserName AS PartyName
                           ,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyOB
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.PartyId=P.Id  AND V.PostingDate < '" + fromDate + @"' AND V.SourceType NOT IN ('OpeningBalance')
                        AND V.Id NOT IN(SELECT VoucherId FROM TRN.InvoiceWriteOff  WHERE SourceType='VendorAdvanceWriteOff')  
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.PartyId=P.Id  AND V.PostingDate <='" + fromDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)PartyOpeningBalance
						,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyCL
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.PartyId=P.Id  AND V.PostingDate <= '" + toDate + @"' AND V.SourceType NOT IN ('OpeningBalance')
                        AND V.Id NOT IN(SELECT VoucherId FROM TRN.InvoiceWriteOff  WHERE SourceType='VendorAdvanceWriteOff')
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.PartyId=P.Id  AND V.PostingDate <='" + toDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)PartyClosingBalance
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] V ON V.Id=VD.VoucherId
                            LEFT join HKP.Party as P on VD.PartyId = p.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=VD.EmployeeId
                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                            ) AS CC ON CC.VoucherDetailId=VD.Id
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND P.PartyCategoryId='" + partyCategoryId + @"'  AND VD.PartyType='" + partyType + @"' AND V.SourceType NOT IN ('OpeningBalance') 
                            AND V.Id NOT IN(SELECT VoucherId FROM TRN.InvoiceWriteOff  WHERE SourceType='VendorAdvanceWriteOff')
                            AND   V.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' 
							GROUP BY P.Id , P.UserName


	union
	SELECT * FROM 
							  (SELECT 
                             0 CompanyCurrencyDrAmount, 0 CompanyCurrencyCrAmount,P.Id PartyId, P.UserName AS PartyName
                           ,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyOB
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.PartyId=P.Id  AND V.PostingDate < '" + fromDate + @"' AND V.SourceType NOT IN ('OpeningBalance')
                        AND V.Id NOT IN(SELECT VoucherId FROM TRN.InvoiceWriteOff  WHERE SourceType='VendorAdvanceWriteOff')
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.PartyId=P.Id  AND V.PostingDate <='" + fromDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)PartyOpeningBalance
						,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyCL
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.PartyId=P.Id  AND V.PostingDate <= '" + toDate + @"' AND V.SourceType NOT IN ('OpeningBalance')
                        AND V.Id NOT IN(SELECT VoucherId FROM TRN.InvoiceWriteOff  WHERE SourceType='VendorAdvanceWriteOff')
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.PartyId=P.Id  AND V.PostingDate <='" + toDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)PartyClosingBalance
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] V ON V.Id=VD.VoucherId
                            LEFT join HKP.Party as P on VD.PartyId = p.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=VD.EmployeeId
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND P.PartyCategoryId='" + partyCategoryId + @"'  AND VD.PartyType='" + partyType + @"' AND V.SourceType NOT IN ('OpeningBalance') 
                            AND V.Id NOT IN(SELECT VoucherId FROM TRN.InvoiceWriteOff  WHERE SourceType='VendorAdvanceWriteOff')
                            AND V.PostingDate < '" + toDate + @"'
							AND VD.PartyId NOT IN(SELECT  VDO.PartyId 
							 FROM [TRN].[VoucherDetail] AS VDO
                            LEFT JOIN [TRN].[Voucher] VO ON VO.Id=VDO.VoucherId
                            LEFT join HKP.Party as PO on VDO.PartyId = PO.Id
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VDO.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VDO.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VDO.ActivityId
                            WHERE VO.Archive=0 AND VO.IsPark=0 AND VO.CompanyGroupId='" + identity.CompanyGroupId + @"' AND VO.CompanyId='" + identity.CompanyId + @"' AND VO.PlantId='" + identity.PlantId + @"' AND PO.PartyCategoryId='" + partyCategoryId + @"'  AND VDO.PartyType='" + partyType + @"' AND VO.SourceType NOT IN ('OpeningBalance')
                            AND VO.Id NOT IN(SELECT VoucherId FROM TRN.InvoiceWriteOff  WHERE SourceType='VendorAdvanceWriteOff')
                            AND VO.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' )
							GROUP BY P.Id , P.UserName)T
							WHERE T.PartyOpeningBalance<>0
union
	SELECT * FROM 
							  (SELECT 
                             0 CompanyCurrencyDrAmount, 0 CompanyCurrencyCrAmount,P.Id PartyId, P.UserName AS PartyName
                           ,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyOB
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.PartyId=P.Id  AND V.PostingDate < '" + fromDate + @"' AND V.SourceType NOT IN ('OpeningBalance')
                        AND V.Id NOT IN(SELECT VoucherId FROM TRN.InvoiceWriteOff  WHERE SourceType='VendorAdvanceWriteOff')
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.PartyId=P.Id  AND V.PostingDate <='" + fromDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)PartyOpeningBalance
						,ISNULL(( SELECT SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyncyCL
                         FROM (
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.PartyId=P.Id  AND V.PostingDate <= '" + toDate + @"' AND V.SourceType NOT IN ('OpeningBalance')
                        AND V.Id NOT IN(SELECT VoucherId FROM TRN.InvoiceWriteOff  WHERE SourceType='VendorAdvanceWriteOff')
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount, CC.CompanyCurrencyId
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND VD.PartyId=P.Id  AND V.PostingDate <='" + toDate + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId ),0)PartyClosingBalance
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] V ON V.Id=VD.VoucherId
                            LEFT join HKP.Party as P on VD.PartyId = p.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=VD.EmployeeId
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.PlantId='" + identity.PlantId + @"' AND P.PartyCategoryId='" + partyCategoryId + @"'  AND VD.PartyType='" + partyType + @"' AND V.SourceType='OpeningBalance' 
							AND VD.PartyId NOT IN(SELECT  VDO.PartyId 
							 FROM [TRN].[VoucherDetail] AS VDO
                            LEFT JOIN [TRN].[Voucher] VO ON VO.Id=VDO.VoucherId
                            LEFT join HKP.Party as PO on VDO.PartyId = PO.Id
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VDO.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VDO.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VDO.ActivityId
                            WHERE VO.Archive=0 AND VO.IsPark=0 AND VO.CompanyGroupId='" + identity.CompanyGroupId + @"' AND VO.CompanyId='" + identity.CompanyId + @"' AND VO.PlantId='" + identity.PlantId + @"' AND PO.PartyCategoryId='" + partyCategoryId + @"'  AND VDO.PartyType='" + partyType + @"' AND VO.SourceType NOT IN ('OpeningBalance')
                            AND VO.Id NOT IN(SELECT VoucherId FROM TRN.InvoiceWriteOff  WHERE SourceType='VendorAdvanceWriteOff')) 
							GROUP BY P.Id , P.UserName)T ";
        }
        #region Inter Party Leadger


        public IWorkbook GetInterPartyLedger(string companyGroupId, string CompanyId, string PlantId, string PlantName, string FromDate, string ToDate)
        {
            try
            {

                var row = 6;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Inter Ledger";
                var colLast = 8;
                var colLast1 = 8;
                var col = 1;

                // Get Party Master
                var company = _companyRepository.Find(CompanyId);
                var plant = _plantRepository.Find(PlantId);
                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Company");
                sheet.Range[row, 1, row, 2].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, company.UserName.ToString());
                sheet.Range[row, 3, row, 5].Merge();
                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Plant");
                sheet.Range[row, 1, row, 2].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, plant.UserName.ToString());
                sheet.Range[row, 3, row, 5].Merge();

                //row++;
                //if (!string.IsNullOrEmpty(partyPlantId))
                //{
                //    var partyPlant = _partyPlantRepository.Find(partyPlantId);
                //    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party Plant");
                //    sheet.Range[row, 1, row, 2].Merge();
                //    reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, partyPlant?.UserName);
                //    sheet.Range[row, 3, row, 5].Merge();

                //    colLast = colLast - 1;
                //    colLast1 = colLast;
                //}
                //if (!string.IsNullOrEmpty(gSTINId))
                //{
                //    reportUtility.SetMasterHeaderText(ref sheet, row, 7, "Party GSTIN");
                //    sheet.Range[row, 7, row, 8].Merge();
                //    reportUtility.SetMiddleAlignmentText(ref sheet, row, 9, gSTINId);
                //    sheet.Range[row, 9, row, 11].Merge();
                //}

                row++;
                _companyParallelCurrencyService.GetParallelCurrency(CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                //if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                //{
                //    reportUtility.SetHeaderText(ref sheet, row, colLast + 1, "Transaction", ExcelHAlign.HAlignCenter);
                //    sheet.Range[row, colLast + 1, row, colLast + 3].Merge();
                //    colLast = colLast + 3;
                //}
                reportUtility.SetHeaderText(ref sheet, row, colLast - 1, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet.Range[row, colLast - 1, row, colLast + 2].Merge();

                // Set Row Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, col, "GL", 32); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 11); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 11); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Ref", 11); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Date", 11); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 50); col++;
                //if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                //{
                //    reportUtility.SetHeaderText(ref sheet, row, col, "Currency", 8, ExcelHAlign.HAlignRight); col++;
                //    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 10, ExcelHAlign.HAlignRight); col++;
                //    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 10, ExcelHAlign.HAlignRight); col++;
                //}
                reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 11, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 11, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Balance", 13, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Dr/Cr", 1, ExcelHAlign.HAlignRight);

                row++;
                reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();

                // Get party opening balance data.
                var obVal = GetPartyOBInterPartyLeadger(companyGroupId, CompanyId, PlantId, FromDate, ToDate);

                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                        reportUtility.SetText(ref sheet, row, col - 1, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);
                    sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();

                    sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                }

                var ledgerData = GetPartyInterPartyLeadger(companyGroupId, CompanyId, PlantId, FromDate, ToDate);

                row++;
                // Get bank transaction data.
                if (ledgerData.Rows.Count > 0)
                {
                    col = 1;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        col = 1;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["GLGeneralInfoCode"] + " - " + ledgerData.Rows[i]["GLGeneralInfoName"]); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PostingDate"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherNo"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocRefNo"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocDate"].ToString()); col++;
                        sheet[row, col].ColumnWidth = 50;
                        sheet.Range[row, col].WrapText = true;
                        reportUtility.SetTextWrapText(ref sheet, row, col, ledgerData.Rows[i]["Narration"].ToString(), 50, ExcelHAlign.HAlignLeft); col++;
                        //if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                        //{
                        //    reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["CurrencyCode"].ToString()); col++;
                        //    reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString())); col++;
                        //    reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString())); col++;
                        //}

                        //Base currency checking
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString())); col++;
                        sheet.Range[row, col].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(col - 2) + row + "-" + reportUtility.GetColumnNameForXls(col - 1) + row + ")";
                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;

                        sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        row++;
                    }
                }

                reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();
                sheet.Range[row, col - 1].Formula = "=" + reportUtility.GetColumnNameForXls(col - 1) + (row - 1);
                sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                sheet.Range[row, col - 1].CellStyle.Font.Bold = true;
                sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                //sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, col, "Inter Transaction Ledger", CompanyId, PlantName, "From " + FromDate + " To " + ToDate + "");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(col) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
        #region Inter Party Leadger
        private List<Dictionary<string, object>> GetPartyOBInterPartyLeadger(string companyGroupId, string CompanyId, string plantId, string fromDate, string toDate)
        {
            var sql = @"SELECT SUM(DrAmount) - SUM(CrAmount) AS OB, CompanyCurrencyId, SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyCurrencyOB FROM (
                        SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
                        FROM [TRN].[VoucherDetailCurrency] AS VDC
                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + CompanyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + CompanyId + @"' AND VD.PlantId='" + plantId + @"' 
                        ---AND VD.PartyId='2019464' 
                        AND V.PostingDate < '" + fromDate + @"' GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount, CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
                        FROM [TRN].[VoucherDetailCurrency] AS VDC
                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + CompanyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + CompanyId + @"' AND VD.PlantId='" + plantId + @"' AND V.PostingDate <='" + fromDate + @"'
                            --AND V.SourceType='OpeningBalance' 
                                GROUP BY CC.CompanyCurrencyId) AS X GROUP BY X.CompanyCurrencyId
                                                                                            ";
            return _sqlRepository.GetDataCollection(sql);
        }

        private DataTable GetPartyInterPartyLeadger(string companyGroupId, string CompanyId, string plantId, string fromDate, string toDate)
        {
            var cmdText = @"DECLARE @companyId VARCHAR(10)='" + CompanyId + @"';
                            SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, VD.Narration, ISNULL(VD.DrAmount,0) AS DrAmount, ISNULL(VD.CrAmount,0) AS CrAmount
                            , CC.CompanyCurrencyId, ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) AS CompanyCurrencyCrAmount, C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode--, PP.GSTIN
                            , GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName,CO.UserName CompanyName,PL.UserName PlantName
                            ,V.CurrencyId, A.UserName AS ActivityName, P.Code AS PartyCode, P.UserName AS PartyName--, PP.UserName AS PartyPlantName
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                            LEFT JOIN [ORG].Company AS CO ON CO.Id=V.CompanyId
                            LEFT JOIN [ORG].Plant AS PL ON PL.Id=VD.PlantId
                            --LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId AND P.Id=VD.PartyId
                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + CompanyId + @"'
                            ) AS CC ON CC.VoucherDetailId=VD.Id
                            WHERE V.Archive=0 AND V.IsPark=0 
                            AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + CompanyId + @"' AND 
                            VD.PlantId='" + plantId + @"' 
                    -- AND V.PostingDate BETWEEN '2019-01-01' AND '14-Nov-2019'
                            AND V.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' order by v.PostingDate ";
            return _sqlRepository.GetDataTable(cmdText);
        }

        #endregion
        public IWorkbook GetPartyOutstandingReport(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId)
        {
            try
            {
                var row = 6;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";
                var colLast = 8;
                var colLast1 = 8;
                var col = 1;

                // Get Party Master
                var partyMaster = _partyService.Find(partyType, companyId, plantId, partyId);
                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party");
                sheet.Range[row, 1, row, 2].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, partyMaster["PartyCode"] + " - " + partyMaster["PartyName"]);
                sheet.Range[row, 3, row, 5].Merge();

                reportUtility.SetMasterHeaderText(ref sheet, row, 7, "Account Group");
                sheet.Range[row, 7, row, 8].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 9, partyMaster["PartyAccountGroupName"].ToString());

                row++;
                if (!string.IsNullOrEmpty(partyPlantId))
                {
                    var partyPlant = _partyPlantRepository.Find(partyPlantId);
                    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party Plant");
                    sheet.Range[row, 1, row, 2].Merge();
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, partyPlant?.UserName);
                    sheet.Range[row, 3, row, 5].Merge();

                    colLast = colLast - 1;
                    colLast1 = colLast;
                }
                if (!string.IsNullOrEmpty(gSTINId))
                {
                    reportUtility.SetMasterHeaderText(ref sheet, row, 7, "Party GSTIN");
                    sheet.Range[row, 7, row, 8].Merge();
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, 9, gSTINId);
                    sheet.Range[row, 9, row, 11].Merge();
                    colLast = colLast - 1;
                    colLast1 = colLast;
                }

                row++;
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                {
                    reportUtility.SetHeaderText(ref sheet, row, colLast + 1, "Transaction", ExcelHAlign.HAlignCenter);
                    sheet.Range[row, colLast + 1, row, colLast + 3].Merge();
                    colLast = colLast + 3;
                }
                reportUtility.SetHeaderText(ref sheet, row, colLast + 1, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet.Range[row, colLast + 1, row, colLast + 3].Merge();

                // Set Row Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, col, "GL", 22); col++;
                if (string.IsNullOrEmpty(partyPlantId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Party Plant", 25); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 12); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 12); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher Date", 12); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Ref", 12); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Date", 12); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 20); col++;
                if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Currency", 8, ExcelHAlign.HAlignRight); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 10, ExcelHAlign.HAlignRight); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 10, ExcelHAlign.HAlignRight); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 10, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 10, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Balance", ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Dr/Cr", 4, ExcelHAlign.HAlignRight);

                row++;
                reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();

                // Get party opening balance data.
                var obVal = GetPartyOpeningBalance(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, partyType.ToString());
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                        reportUtility.SetText(ref sheet, row, col - 1, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);
                    sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                }

                var ledgerData = partyType == PartyType.Customer ? GetCustomerOutstandingData(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId)
                                                                : GetVendorOutstandingData(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId);
                row++;
                // Get bank transaction data.
                if (ledgerData.Rows.Count > 0)
                {
                    col = 1;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        col = 1;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["GLGeneralInfoCode"] + " - " + ledgerData.Rows[i]["GLGeneralInfoName"]); col++;
                        if (string.IsNullOrEmpty(partyPlantId))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PartyPlantName"].ToString()); col++;
                        }
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PostingDate"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherNo"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherDate"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocRefNo"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocDate"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Narration"].ToString()); col++;
                        if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["CurrencyCode"].ToString()); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString())); col++;
                        }
                        // Base currency checking
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString())); col++;
                        sheet.Range[row, col].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(col - 2) + row + "-" + reportUtility.GetColumnNameForXls(col - 1) + row + ")";
                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatDecimalTwo(); col++;
                        sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        row++;
                    }
                }

                reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();
                sheet.Range[row, col - 1].Formula = "=" + reportUtility.GetColumnNameForXls(col - 1) + (row - 1);
                sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, col - 1].CellStyle.Font.Bold = true;
                sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, col, "Party Outstanding Ledger", companyId, plantName, "From " + fromDate + " To " + toDate + "");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(col) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetCustomerOutstandingData(string companyGroupId, string companyId, string plantId, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId)
        {
            var cmdText = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                            SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, VD.Narration, ISNULL(VD.DrAmount,0) AS DrAmount, ISNULL(VD.CrAmount,0) AS CrAmount
                            , CC.CompanyCurrencyId, ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) AS CompanyCurrencyCrAmount, C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                            , GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId AND P.Id=VD.PartyId
                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                            ) AS CC ON CC.VoucherDetailId=VD.Id
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND VD.PartyId='" + partyId + "' AND V.PostingDate BETWEEN '" + fromDate.ToDbDate() + "' AND '" + toDate + @"'
                            AND V.SourceType<>'OpeningBalance' AND VD.AdvanceWriteOffDetailId IS NULL AND VD.InvoiceWriteOffDetailId IS NULL";
            if (!string.IsNullOrEmpty(partyPlantId))
                cmdText += " AND VD.PartyPlantId='" + partyPlantId + "'";
            if (!string.IsNullOrEmpty(gSTINId))
                cmdText += " AND PP.GSTIN='" + gSTINId + "'";
            cmdText += " ORDER BY V.PostingDate, V.VoucherNo ASC";
            return _sqlRepository.GetDataTable(cmdText);
        }

        private DataTable GetVendorOutstandingData(string companyGroupId, string companyId, string plantId, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId)
        {
            var cmdText = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                            SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, VD.Narration, ISNULL(VD.DrAmount,0) AS DrAmount, ISNULL(VD.CrAmount,0) AS CrAmount
                            , CC.CompanyCurrencyId, ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) AS CompanyCurrencyCrAmount, C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                            , GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId AND P.Id=VD.PartyId
                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                            ) AS CC ON CC.VoucherDetailId=VD.Id
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND VD.PartyId='" + partyId + "' AND V.PostingDate BETWEEN '" + fromDate.ToDbDate() + "' AND '" + toDate + @"'
                            AND V.SourceType<>'OpeningBalance' AND V.SourceType !='VendorAdvanceWriteOff'";
            if (!string.IsNullOrEmpty(partyPlantId))
                cmdText += " AND VD.PartyPlantId='" + partyPlantId + "'";
            if (!string.IsNullOrEmpty(gSTINId))
                cmdText += " AND PP.GSTIN='" + gSTINId + "'";
            cmdText += " ORDER BY V.PostingDate, V.VoucherNo ASC";
            return _sqlRepository.GetDataTable(cmdText);
        }

        public IWorkbook PartyReport(string type, string companyGroupId, string companyId, string plantId)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];

                if (type == "Both " && companyGroupId != "null " && companyId == "null")
                    CreateBothCompanyGroupSheet(ref sheet, reportUtility, "Party", "Party", companyGroupId);
                else if (type == "Both " && companyGroupId == "null " && companyId != "null" && plantId != "null")
                    CreateBothCompanyPlantSheet(ref sheet, reportUtility, "Party", "Party", companyId, plantId);
                else if (type == "Customer " && companyGroupId == "null " && companyId != "null" && plantId != "null")
                    CreatePlantCustomerSheet(ref sheet, reportUtility, "Customer", "Customer", companyId, plantId);
                else if (type == "Vendor " && companyGroupId == "null " && companyId != "null" && plantId != "null")
                    CreatePlantVendorSheet(ref sheet, reportUtility, "Vendor", "Vendor", companyId, plantId);
                else if (type == "Both " && companyGroupId == "null " && companyId != "null" && plantId == "null")
                    CreateBothCompanySheet(ref sheet, reportUtility, "Party", "Party", companyId, companyGroupId);
                else if (type == "Customer " && companyGroupId == "null " && companyId != "null" && plantId == "null")
                    CreateCustomerSheet(ref sheet, reportUtility, "Customer", "Customer", companyId, companyGroupId);
                else if (type == "Vendor " && companyGroupId == "null " && companyId != "null" && plantId == "null")
                    CreateVendorSheet(ref sheet, reportUtility, "Vendor", "Vendor", companyId, companyGroupId);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetPartyCustomerInfo(string companyId)
        {
            try
            {
                var sql = @"SELECT P.Id, COM.UserName AS Company, P.Code, P.UserName AS [Customer Name], P.VATResistrationNo AS [VAT Resistration No]
							, P.TradeLicenseNo AS [Trade License No],GLA.UserName AS AdditionalGL,GLD.UserName AS DownPaymentGL, GLR.UserName AS ReconciliationGL, P.DebitLimit AS [Debit Limit]
							, P.CreditLimit AS [Credit Limit], PAG.UserName AS [Party Account Group], C.Code AS [Currency], PT.UserName AS [Payment Term]
							, [Tax Exemption]=CASE WHEN CP.IsPaymentTermChangeable=1 THEN 'Yes' ELSE 'No' END
                            , GLR.AccountCode AS [ReconciliationGL Code], BGMR.RefNo [Reconciliation Budget RefNo], BGR.UserName AS [Reconciliation Budget], AR.UserName AS [Reconciliation Activity]
							, GLD.AccountCode AS [DownPaymentGL Code], BGMD.RefNo [DownPayment Budget RefNo], BGD.UserName AS [DownPayment Budget], AD.UserName AS [DownPayment Activity]
							, PG.UserName [Party Group],PC.UserName [Party Category],PSC.UserName [Party Sub Category]
                            FROM [HKP].[Party] AS P
							LEFT JOIN HKP.CompanyParty CP ON P.Id=CP.PartyId AND CP.PartyType='Customer'
							LEFT OUTER JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
							LEFT JOIN HKP.CompanyPartyGL CPG ON P.Id=CPG.PartyId AND CPG.PartyGLType='ReconciliationGL'
							LEFT JOIN HKP.CompanyPartyGL CPA ON P.Id=CPA.PartyId AND CPA.PartyGLType='AdditionalGL'
							LEFT JOIN HKP.CompanyPartyGL CPD ON P.Id=CPD.PartyId AND CPD.PartyGLType='DownPaymentGL'
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLR ON GLR.Id= CPG.GLGeneralInfoId
							LEFT JOIN [MST].[BudgetMaster] AS BGMR ON BGMR.Id=CPG.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BGR ON BGR.Id=BGMR.BudgetId
                            LEFT JOIN [HKP].[Activity] AS AR ON AR.Id=CPG.ActivityId
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLA ON GLA.Id= CPA.GLGeneralInfoId
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLD ON GLD.Id= CPD.GLGeneralInfoId
							LEFT JOIN [MST].[BudgetMaster] AS BGMD ON BGMD.Id=CPD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BGD ON BGD.Id=BGMD.BudgetId
                            LEFT JOIN [HKP].[Activity] AS AD ON AD.Id=CPD.ActivityId
							LEFT JOIN [HKP].[PartyGroup] AS PG ON PG.Id=P.PartyGroupId
                            LEFT JOIN [HKP].[PartyCategory] AS PC ON PC.Id=P.PartyCategoryId
                            LEFT JOIN [HKP].[PartySubCategory] AS PSC ON PSC.Id=P.PartySubCategoryId
                            LEFT OUTER JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
							LEFT OUTER JOIN [MST].[PaymentTerm] AS PT ON PT.Id =CP.PaymentTermId
							LEFT OUTER JOIN [ORG].[Company] AS COM ON COM.id=CP.CompanyId
                            WHERE P.Active=1 AND P.Archive=0 AND cp.CompanyId='" + companyId + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetPartyVendorInfo(string companyId)
        {
            try
            {
                var sql = @"SELECT P.Id, COM.UserName AS Company, P.Code, P.UserName AS [Vendor Name], P.VATResistrationNo AS [VAT Resistration No]
							, P.TradeLicenseNo AS [Trade License No],GLA.UserName AS AdditionalGL,GLD.UserName AS DownPaymentGL, GLR.UserName AS ReconciliationGL, P.DebitLimit AS [Debit Limit]
							, P.CreditLimit AS [Credit Limit], PAG.UserName AS [Party Account Group], C.Code AS [Currency], PT.UserName AS [Payment Term]
							, [Tax Exemption]=CASE WHEN CP.IsPaymentTermChangeable=1 THEN 'Yes' ELSE 'No' END
                            , GLR.AccountCode AS [ReconciliationGL Code], BGMR.RefNo [Reconciliation Budget RefNo], BGR.UserName AS [Reconciliation Budget], AR.UserName AS [Reconciliation Activity]
							, GLD.AccountCode AS [DownPaymentGL Code], BGMD.RefNo [DownPayment Budget RefNo], BGD.UserName AS [DownPayment Budget], AD.UserName AS [DownPayment Activity]
							, PG.UserName [Party Group],PC.UserName [Party Category],PSC.UserName [Party Sub Category]
                            FROM [HKP].[Party] AS P
							LEFT JOIN HKP.CompanyParty CP ON P.Id=CP.PartyId AND CP.PartyType='Vendor'
							LEFT OUTER JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
							LEFT JOIN HKP.CompanyPartyGL CPG ON P.Id=CPG.PartyId AND CPG.PartyGLType='ReconciliationGL'
							LEFT JOIN HKP.CompanyPartyGL CPA ON P.Id=CPA.PartyId AND CPA.PartyGLType='AdditionalGL'
							LEFT JOIN HKP.CompanyPartyGL CPD ON P.Id=CPD.PartyId AND CPD.PartyGLType='DownPaymentGL'
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLR ON GLR.Id= CPG.GLGeneralInfoId
							LEFT JOIN [MST].[BudgetMaster] AS BGMR ON BGMR.Id=CPG.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BGR ON BGR.Id=BGMR.BudgetId
                            LEFT JOIN [HKP].[Activity] AS AR ON AR.Id=CPG.ActivityId
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLA ON GLA.Id= CPA.GLGeneralInfoId
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLD ON GLD.Id= CPD.GLGeneralInfoId
							LEFT JOIN [MST].[BudgetMaster] AS BGMD ON BGMD.Id=CPD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BGD ON BGD.Id=BGMD.BudgetId
                            LEFT JOIN [HKP].[Activity] AS AD ON AD.Id=CPD.ActivityId
							LEFT JOIN [HKP].[PartyGroup] AS PG ON PG.Id=P.PartyGroupId
                            LEFT JOIN [HKP].[PartyCategory] AS PC ON PC.Id=P.PartyCategoryId
                            LEFT JOIN [HKP].[PartySubCategory] AS PSC ON PSC.Id=P.PartySubCategoryId
                            LEFT OUTER JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
							LEFT OUTER JOIN [MST].[PaymentTerm] AS PT ON PT.Id =CP.PaymentTermId
							LEFT OUTER JOIN [ORG].[Company] AS COM ON COM.id=CP.CompanyId
                            WHERE P.Active=1 AND P.Archive=0 AND CP.CompanyId='" + companyId + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetPartyBothInfo(string companyId)
        {
            try
            {
                var sql = @"SELECT P.Id, COM.UserName AS Company, P.Code, P.UserName AS [Party Name], P.VATResistrationNo AS [VAT Resistration No], P.TradeLicenseNo AS [Trade License No]
							, GLA.UserName AS AdditionalGL, GLD.UserName AS DownPaymentGL, GLR.UserName AS ReconciliationGL, P.DebitLimit AS [Debit Limit], P.CreditLimit AS [Credit Limit]
							, PAG.UserName AS [Party Account Group], C.Code AS [Currency], PT.UserName AS [Payment Term], [Tax Exemption]=CASE WHEN CP.IsPaymentTermChangeable=1 THEN 'Yes' ELSE 'No' END
							, [Customer]=CASE WHEN CP.PartyType='Customer' THEN 'Yes' ELSE '' END, [Vendor]=CASE WHEN CP.PartyType='Vendor' THEN 'Yes' ELSE '' END
                            , GLR.AccountCode AS [ReconciliationGL Code], BGMR.RefNo [Reconciliation Budget RefNo], BGR.UserName AS [Reconciliation Budget], AR.UserName AS [Reconciliation Activity]
							, GLD.AccountCode AS [DownPaymentGL Code], BGMD.RefNo [DownPayment Budget RefNo], BGD.UserName AS [DownPayment Budget], AD.UserName AS [DownPayment Activity]
							, PG.UserName [Party Group],PC.UserName [Party Category],PSC.UserName [Party Sub Category]
                            FROM [HKP].[Party] AS P
							LEFT JOIN HKP.CompanyParty CP ON P.Id=CP.PartyId
							LEFT OUTER JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
							LEFT JOIN HKP.CompanyPartyGL CPG ON P.Id=CPG.PartyId AND CPG.PartyGLType='ReconciliationGL'
							LEFT JOIN HKP.CompanyPartyGL CPA ON P.Id=CPA.PartyId AND CPA.PartyGLType='AdditionalGL'
							LEFT JOIN HKP.CompanyPartyGL CPD ON P.Id=CPD.PartyId AND CPD.PartyGLType='DownPaymentGL'
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLR ON GLR.Id= CPG.GLGeneralInfoId
							LEFT JOIN [MST].[BudgetMaster] AS BGMR ON BGMR.Id=CPG.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BGR ON BGR.Id=BGMR.BudgetId
                            LEFT JOIN [HKP].[Activity] AS AR ON AR.Id=CPG.ActivityId
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLA ON GLA.Id= CPA.GLGeneralInfoId
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLD ON GLD.Id= CPD.GLGeneralInfoId
							LEFT JOIN [MST].[BudgetMaster] AS BGMD ON BGMD.Id=CPD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BGD ON BGD.Id=BGMD.BudgetId
                            LEFT JOIN [HKP].[Activity] AS AD ON AD.Id=CPD.ActivityId
							LEFT JOIN [HKP].[PartyGroup] AS PG ON PG.Id=P.PartyGroupId
                            LEFT JOIN [HKP].[PartyCategory] AS PC ON PC.Id=P.PartyCategoryId
                            LEFT JOIN [HKP].[PartySubCategory] AS PSC ON PSC.Id=P.PartySubCategoryId
							LEFT OUTER JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
							LEFT OUTER JOIN [MST].[PaymentTerm] AS PT ON PT.Id =CP.PaymentTermId
							LEFT OUTER JOIN [ORG].[Company] AS COM ON COM.id=CP.CompanyId
                            WHERE P.Active=1 AND P.Archive=0 AND CP.CompanyId='" + companyId + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetPartyBothCGInfo(string companyGroupId)
        {
            try
            {
                var sql = @"SELECT P.Id, CG.UserName AS CompanyGroup, P.Code, P.UserName AS [Party Name], P.VATResistrationNo AS [VAT Resistration No], P.TradeLicenseNo AS [Trade License No]
							, GLA.UserName AS AdditionalGL, GLD.UserName AS DownPaymentGL, GLR.UserName AS ReconciliationGL, P.DebitLimit AS [Debit Limit], P.CreditLimit AS [Credit Limit]
							, PAG.UserName AS [Party Account Group], C.Code AS [Currency], PT.UserName AS [Payment Term]
							, [Tax Exemption]=CASE WHEN CP.IsPaymentTermChangeable=1 THEN 'Yes' ELSE 'No' END
							, [Customer]=CASE WHEN CP.PartyType='Customer' THEN 'Yes' ELSE '' END
							, [Vendor]=CASE WHEN CP.PartyType='Vendor' THEN 'Yes' ELSE '' END
                            , GLR.AccountCode AS [ReconciliationGL Code], BGMR.RefNo [Reconciliation Budget RefNo], BGR.UserName AS [Reconciliation Budget], AR.UserName AS [Reconciliation Activity]
							, GLD.AccountCode AS [DownPaymentGL Code], BGMD.RefNo [DownPayment Budget RefNo], BGD.UserName AS [DownPayment Budget], AD.UserName AS [DownPayment Activity]
							, PG.UserName [Party Group],PC.UserName [Party Category],PSC.UserName [Party Sub Category]
                            FROM [HKP].[Party] AS P
							LEFT JOIN HKP.CompanyParty CP ON P.Id=CP.PartyId
							LEFT OUTER JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
							LEFT JOIN HKP.CompanyPartyGL CPG ON P.Id=CPG.PartyId AND CPG.PartyGLType='ReconciliationGL'
							LEFT JOIN HKP.CompanyPartyGL CPA ON P.Id=CPA.PartyId AND CPA.PartyGLType='AdditionalGL'
							LEFT JOIN HKP.CompanyPartyGL CPD ON P.Id=CPD.PartyId AND CPD.PartyGLType='DownPaymentGL'
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLR ON GLR.Id= CPG.GLGeneralInfoId
							LEFT JOIN [MST].[BudgetMaster] AS BGMR ON BGMR.Id=CPG.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BGR ON BGR.Id=BGMR.BudgetId
                            LEFT JOIN [HKP].[Activity] AS AR ON AR.Id=CPG.ActivityId
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLA ON GLA.Id= CPA.GLGeneralInfoId
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLD ON GLD.Id= CPD.GLGeneralInfoId
							LEFT JOIN [MST].[BudgetMaster] AS BGMD ON BGMD.Id=CPD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BGD ON BGD.Id=BGMD.BudgetId
                            LEFT JOIN [HKP].[Activity] AS AD ON AD.Id=CPD.ActivityId
							LEFT JOIN [HKP].[PartyGroup] AS PG ON PG.Id=P.PartyGroupId
                            LEFT JOIN [HKP].[PartyCategory] AS PC ON PC.Id=P.PartyCategoryId
                            LEFT JOIN [HKP].[PartySubCategory] AS PSC ON PSC.Id=P.PartySubCategoryId
							LEFT OUTER JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
							LEFT OUTER JOIN [MST].[PaymentTerm] AS PT ON PT.Id =CP.PaymentTermId
							LEFT OUTER JOIN [ORG].[CompanyGroup] AS CG ON CG.id=P.CompanyGroupId
                            WHERE P.Active=1 AND P.Archive=0 AND CG.Id='" + companyGroupId + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetPartyPlantCustomerInfo(string companyId, string plantId)
        {
            try
            {
                var sql = @"SELECT P.Id, COM.UserName AS Company,PL.UserName Plant, P.Code, P.UserName AS [Customer Name], P.VATResistrationNo AS [VAT Resistration No]
							, P.TradeLicenseNo AS [Trade License No],GLA.UserName AS AdditionalGL,GLD.UserName AS DownPaymentGL, GLR.UserName AS ReconciliationGL, P.DebitLimit AS [Debit Limit]
							, P.CreditLimit AS [Credit Limit], PAG.UserName AS [Party Account Group], C.Code AS [Currency], PT.UserName AS [Payment Term]
							, [Tax Exemption]=CASE WHEN CP.IsPaymentTermChangeable=1 THEN 'Yes' ELSE 'No' END
                            , GLR.AccountCode AS [ReconciliationGL Code], BGMR.RefNo [Reconciliation Budget RefNo], BGR.UserName AS [Reconciliation Budget], AR.UserName AS [Reconciliation Activity]
							, GLD.AccountCode AS [DownPaymentGL Code], BGMD.RefNo [DownPayment Budget RefNo], BGD.UserName AS [DownPayment Budget], AD.UserName AS [DownPayment Activity]
							, PG.UserName [Party Group],PC.UserName [Party Category],PSC.UserName [Party Sub Category]
                            FROM [HKP].[Party] AS P
							LEFT JOIN HKP.CompanyParty CP ON P.Id=CP.PartyId AND CP.PartyType='Customer'
							LEFT OUTER JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
							LEFT JOIN HKP.CompanyPartyGL CPG ON P.Id=CPG.PartyId AND CPG.PartyGLType='ReconciliationGL'
							LEFT JOIN HKP.CompanyPartyGL CPA ON P.Id=CPA.PartyId AND CPA.PartyGLType='AdditionalGL'
							LEFT JOIN HKP.CompanyPartyGL CPD ON P.Id=CPD.PartyId AND CPD.PartyGLType='DownPaymentGL'
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLR ON GLR.Id= CPG.GLGeneralInfoId
							LEFT JOIN [MST].[BudgetMaster] AS BGMR ON BGMR.Id=CPG.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BGR ON BGR.Id=BGMR.BudgetId
                            LEFT JOIN [HKP].[Activity] AS AR ON AR.Id=CPG.ActivityId
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLA ON GLA.Id= CPA.GLGeneralInfoId
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLD ON GLD.Id= CPD.GLGeneralInfoId
							LEFT JOIN [MST].[BudgetMaster] AS BGMD ON BGMD.Id=CPD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BGD ON BGD.Id=BGMD.BudgetId
                            LEFT JOIN [HKP].[Activity] AS AD ON AD.Id=CPD.ActivityId
							LEFT JOIN [HKP].[PartyGroup] AS PG ON PG.Id=P.PartyGroupId
                            LEFT JOIN [HKP].[PartyCategory] AS PC ON PC.Id=P.PartyCategoryId
                            LEFT JOIN [HKP].[PartySubCategory] AS PSC ON PSC.Id=P.PartySubCategoryId
                            LEFT OUTER JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
							LEFT OUTER JOIN [MST].[PaymentTerm] AS PT ON PT.Id =CP.PaymentTermId
							LEFT OUTER JOIN [ORG].[Company] AS COM ON COM.id=CP.CompanyId
							LEFT OUTER JOIN ORG.Plant AS PL ON CP.PlantId=PL.Id
                            WHERE P.Active=1 AND P.Archive=0 AND cp.CompanyId='" + companyId + "' And CP.PlantId='" + plantId + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetPartyPlantVendorInfo(string companyId, string plantId)
        {
            try
            {
                var sql = @"SELECT P.Id, COM.UserName AS Company,PL.UserName Plant, P.Code, P.UserName AS [Vendor Name], P.VATResistrationNo AS [VAT Resistration No]
							, P.TradeLicenseNo AS [Trade License No],GLA.UserName AS AdditionalGL,GLD.UserName AS DownPaymentGL, GLR.UserName AS ReconciliationGL, P.DebitLimit AS [Debit Limit]
							, P.CreditLimit AS [Credit Limit], PAG.UserName AS [Party Account Group], C.Code AS [Currency], PT.UserName AS [Payment Term]
							, [Tax Exemption]=CASE WHEN CP.IsPaymentTermChangeable=1 THEN 'Yes' ELSE 'No' END
                            , GLR.AccountCode AS [ReconciliationGL Code], BGMR.RefNo [Reconciliation Budget RefNo], BGR.UserName AS [Reconciliation Budget], AR.UserName AS [Reconciliation Activity]
							, GLD.AccountCode AS [DownPaymentGL Code], BGMD.RefNo [DownPayment Budget RefNo], BGD.UserName AS [DownPayment Budget], AD.UserName AS [DownPayment Activity]
							, PG.UserName [Party Group],PC.UserName [Party Category],PSC.UserName [Party Sub Category]
                            FROM [HKP].[Party] AS P
							LEFT JOIN HKP.CompanyParty CP ON P.Id=CP.PartyId AND CP.PartyType='Vendor'
							LEFT OUTER JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
							LEFT JOIN HKP.CompanyPartyGL CPG ON P.Id=CPG.PartyId AND CPG.PartyGLType='ReconciliationGL'
							LEFT JOIN HKP.CompanyPartyGL CPA ON P.Id=CPA.PartyId AND CPA.PartyGLType='AdditionalGL'
							LEFT JOIN HKP.CompanyPartyGL CPD ON P.Id=CPD.PartyId AND CPD.PartyGLType='DownPaymentGL'
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLR ON GLR.Id= CPG.GLGeneralInfoId
							LEFT JOIN [MST].[BudgetMaster] AS BGMR ON BGMR.Id=CPG.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BGR ON BGR.Id=BGMR.BudgetId
                            LEFT JOIN [HKP].[Activity] AS AR ON AR.Id=CPG.ActivityId
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLA ON GLA.Id= CPA.GLGeneralInfoId
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLD ON GLD.Id= CPD.GLGeneralInfoId
							LEFT JOIN [MST].[BudgetMaster] AS BGMD ON BGMD.Id=CPD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BGD ON BGD.Id=BGMD.BudgetId
                            LEFT JOIN [HKP].[Activity] AS AD ON AD.Id=CPD.ActivityId
							LEFT JOIN [HKP].[PartyGroup] AS PG ON PG.Id=P.PartyGroupId
                            LEFT JOIN [HKP].[PartyCategory] AS PC ON PC.Id=P.PartyCategoryId
                            LEFT JOIN [HKP].[PartySubCategory] AS PSC ON PSC.Id=P.PartySubCategoryId
                            LEFT OUTER JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
							LEFT OUTER JOIN [MST].[PaymentTerm] AS PT ON PT.Id =CP.PaymentTermId
							LEFT OUTER JOIN [ORG].[Company] AS COM ON COM.id=CP.CompanyId
							LEFT OUTER JOIN ORG.Plant AS PL ON CP.PlantId=PL.Id
                            WHERE P.Active=1 AND P.Archive=0 AND cp.CompanyId='" + companyId + "' And CP.PlantId='" + plantId + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetPartyPlantBothInfo(string companyId, string plantId)
        {
            try
            {
                var sql = @"SELECT P.Id, COM.UserName AS Company, PL.UserName Plant, P.Code, P.UserName AS [Party Name], P.VATResistrationNo AS [VAT Resistration No]
							, P.TradeLicenseNo AS [Trade License No], GLA.UserName AS AdditionalGL, GLD.UserName AS DownPaymentGL, GLR.UserName AS ReconciliationGL
							, P.DebitLimit AS [Debit Limit], P.CreditLimit AS [Credit Limit], PAG.UserName AS [Party Account Group], C.Code AS [Currency]
							, PT.UserName AS [Payment Term], [Tax Exemption]=CASE WHEN CP.IsPaymentTermChangeable=1 THEN 'Yes' ELSE 'No' END
							, [Customer]=CASE WHEN CP.PartyType='Customer' THEN 'Yes' ELSE '' END, [Vendor]=CASE WHEN CP.PartyType='Vendor' THEN 'Yes' ELSE '' END
                            , GLR.AccountCode AS [ReconciliationGL Code], BGMR.RefNo [Reconciliation Budget RefNo], BGR.UserName AS [Reconciliation Budget], AR.UserName AS [Reconciliation Activity]
							, GLD.AccountCode AS [DownPaymentGL Code], BGMD.RefNo [DownPayment Budget RefNo], BGD.UserName AS [DownPayment Budget], AD.UserName AS [DownPayment Activity]
							, PG.UserName [Party Group],PC.UserName [Party Category],PSC.UserName [Party Sub Category]
                            FROM [HKP].[Party] AS P
							LEFT JOIN HKP.CompanyParty CP ON P.Id=CP.PartyId
							LEFT OUTER JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
							LEFT JOIN HKP.CompanyPartyGL CPG ON P.Id=CPG.PartyId AND CPG.PartyGLType='ReconciliationGL'
							LEFT JOIN HKP.CompanyPartyGL CPA ON P.Id=CPA.PartyId AND CPA.PartyGLType='AdditionalGL'
							LEFT JOIN HKP.CompanyPartyGL CPD ON P.Id=CPD.PartyId AND CPD.PartyGLType='DownPaymentGL'
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLR ON GLR.Id= CPG.GLGeneralInfoId
							LEFT JOIN [MST].[BudgetMaster] AS BGMR ON BGMR.Id=CPG.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BGR ON BGR.Id=BGMR.BudgetId
                            LEFT JOIN [HKP].[Activity] AS AR ON AR.Id=CPG.ActivityId
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLA ON GLA.Id= CPA.GLGeneralInfoId
							LEFT OUTER JOIN [HKP].[GLGeneralInfo] AS GLD ON GLD.Id= CPD.GLGeneralInfoId
							LEFT JOIN [MST].[BudgetMaster] AS BGMD ON BGMD.Id=CPD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BGD ON BGD.Id=BGMD.BudgetId
                            LEFT JOIN [HKP].[Activity] AS AD ON AD.Id=CPD.ActivityId
							LEFT JOIN [HKP].[PartyGroup] AS PG ON PG.Id=P.PartyGroupId
                            LEFT JOIN [HKP].[PartyCategory] AS PC ON PC.Id=P.PartyCategoryId
                            LEFT JOIN [HKP].[PartySubCategory] AS PSC ON PSC.Id=P.PartySubCategoryId
							LEFT OUTER JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
							LEFT OUTER JOIN [MST].[PaymentTerm] AS PT ON PT.Id =CP.PaymentTermId
							LEFT OUTER JOIN [ORG].[Company] AS COM ON COM.id=CP.CompanyId
							LEFT OUTER JOIN ORG.Plant AS PL ON CP.PlantId=PL.Id
                            WHERE P.Active=1 AND P.Archive=0 AND cp.CompanyId='" + companyId + "' And CP.PlantId='" + plantId + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateCustomerSheet(ref IWorksheet sheet, ReportUtility reportUtility, string sheetHeader, string sheetName, string companyId, string companyGroupId)
        {
            try
            {
                #region List data

                var dtParty = GetPartyCustomerInfo(companyId);
                var dvMainBody = new DataView(dtParty)
                {
                    Sort = "Customer Name"
                };
                
                dtParty = dvMainBody.ToTable(true, "Id", "Company", "Party Group", "Party Category", "Party Sub Category", "Code", "Customer Name", "VAT Resistration No", "Trade License No", "Debit Limit", "Credit Limit", "Party Account Group", "Currency", "Payment Term", "Tax Exemption", "DownPaymentGL Code", "DownPaymentGL", "DownPayment Budget", "DownPayment Activity", "DownPayment Budget RefNo", "ReconciliationGL Code", "ReconciliationGL", "Reconciliation Budget", "Reconciliation Activity", "Reconciliation Budget RefNo", "AdditionalGL");
                if (dtParty.Rows.Count == 0)
                    throw new Exception("No data found!");

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;

                var _col3 = 3;

                reportUtility.SetMasterHeaderText(ref sheet, _rowL, _col, "Company");
                sheet[reportUtility.GetColumnNameForXls(_col) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _rowL].Merge();
                reportUtility.SetText(ref sheet, _rowL, _col + 2, dtParty.Rows[0]["Company"].ToString()); _rowL++;
                sheet[reportUtility.GetColumnNameForXls(_col3) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col3 + 2) + _rowL].Merge();

                _rowL = 6;
                _rowL++;

                for (int i = 0; i < dtParty.Columns.Count; i++)
                {
                    if (dtParty.Columns[i].ColumnName != "TotalRows" && dtParty.Columns[i].ColumnName != "Id" && dtParty.Columns[i].ColumnName != "Company")
                    {
                        _colIndex++;
                        reportUtility.SetHeaderText(ref sheet, _rowL, _colIndex, dtParty.Columns[i].ColumnName);
                    }
                }
                shet2EndxlsCol = _colIndex;

                for (int q = 0; q < dtParty.Rows.Count; q++)
                {
                    _rowL++;
                    reportUtility.SetText(ref sheet, _rowL, 1, dtParty.Rows[q]["Party Group"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 2, dtParty.Rows[q]["Party Category"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 3, dtParty.Rows[q]["Party Sub Category"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 4, dtParty.Rows[q]["Code"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 5, dtParty.Rows[q]["Customer Name"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 6, dtParty.Rows[q]["VAT Resistration No"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 7, dtParty.Rows[q]["Trade License No"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 8, dtParty.Rows[q]["Debit Limit"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 9, dtParty.Rows[q]["Credit Limit"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 10, dtParty.Rows[q]["Party Account Group"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 11, dtParty.Rows[q]["Currency"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 12, dtParty.Rows[q]["Payment Term"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 13, dtParty.Rows[q]["Tax Exemption"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 14, dtParty.Rows[q]["DownPaymentGL Code"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 15, dtParty.Rows[q]["DownPaymentGL"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 16, dtParty.Rows[q]["DownPayment Budget"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 17, dtParty.Rows[q]["DownPayment Activity"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 18, dtParty.Rows[q]["DownPayment Budget RefNo"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 19, dtParty.Rows[q]["ReconciliationGL Code"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 20, dtParty.Rows[q]["ReconciliationGL"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 21, dtParty.Rows[q]["Reconciliation Budget"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 22, dtParty.Rows[q]["Reconciliation Activity"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 23, dtParty.Rows[q]["Reconciliation Budget RefNo"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 24, dtParty.Rows[q]["AdditionalGL"].ToString(), 26);
                }

                sheet.Range[7, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = sheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyHeader(ref sheet, shet2EndxlsCol, "Customer", companyId);
                reportUtility.FreezePage(ref sheet, 1, 8);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateVendorSheet(ref IWorksheet sheet, ReportUtility reportUtility, string sheetHeader, string sheetName, string companyId, string companyGroupId)
        {
            try
            {
                #region List data

                var dtParty = GetPartyVendorInfo(companyId);
                var dvMainBody = new DataView(dtParty)
                {
                    Sort = "Vendor Name"
                };
                
                dtParty = dvMainBody.ToTable(true, "Id", "Company", "Party Group", "Party Category", "Party Sub Category", "Code", "Vendor Name", "VAT Resistration No", "Trade License No", "Debit Limit", "Credit Limit", "Party Account Group", "Currency", "Payment Term", "Tax Exemption", "DownPaymentGL Code", "DownPaymentGL", "DownPayment Budget", "DownPayment Activity", "DownPayment Budget RefNo", "ReconciliationGL Code", "ReconciliationGL", "Reconciliation Budget", "Reconciliation Activity", "Reconciliation Budget RefNo", "AdditionalGL");
                if (dtParty.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;

                var _col3 = 3;

                reportUtility.SetMasterHeaderText(ref sheet, _rowL, _col, "Company");
                sheet[reportUtility.GetColumnNameForXls(_col) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _rowL].Merge();
                reportUtility.SetText(ref sheet, _rowL, _col + 2, dtParty.Rows[0]["Company"].ToString()); _rowL++;
                sheet[reportUtility.GetColumnNameForXls(_col3) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col3 + 2) + _rowL].Merge();

                _rowL = 6;
                _rowL++;

                for (int i = 0; i < dtParty.Columns.Count; i++)
                {
                    if (dtParty.Columns[i].ColumnName != "TotalRows" && dtParty.Columns[i].ColumnName != "Id" && dtParty.Columns[i].ColumnName != "Company")
                    {
                        _colIndex++;
                        reportUtility.SetHeaderText(ref sheet, _rowL, _colIndex, dtParty.Columns[i].ColumnName);
                    }
                }
                shet2EndxlsCol = _colIndex;

                for (int q = 0; q < dtParty.Rows.Count; q++)
                {
                    _rowL++;
                    reportUtility.SetText(ref sheet, _rowL, 1, dtParty.Rows[q]["Party Group"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 2, dtParty.Rows[q]["Party Category"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 3, dtParty.Rows[q]["Party Sub Category"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 4, dtParty.Rows[q]["Code"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 5, dtParty.Rows[q]["Vendor Name"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 6, dtParty.Rows[q]["VAT Resistration No"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 7, dtParty.Rows[q]["Trade License No"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 8, dtParty.Rows[q]["Debit Limit"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 9, dtParty.Rows[q]["Credit Limit"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 10, dtParty.Rows[q]["Party Account Group"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 11, dtParty.Rows[q]["Currency"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 12, dtParty.Rows[q]["Payment Term"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 13, dtParty.Rows[q]["Tax Exemption"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 14, dtParty.Rows[q]["DownPaymentGL Code"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 15, dtParty.Rows[q]["DownPaymentGL"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 16, dtParty.Rows[q]["DownPayment Budget"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 17, dtParty.Rows[q]["DownPayment Activity"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 18, dtParty.Rows[q]["DownPayment Budget RefNo"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 19, dtParty.Rows[q]["ReconciliationGL Code"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 20, dtParty.Rows[q]["ReconciliationGL"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 21, dtParty.Rows[q]["Reconciliation Budget"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 22, dtParty.Rows[q]["Reconciliation Activity"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 23, dtParty.Rows[q]["Reconciliation Budget RefNo"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 24, dtParty.Rows[q]["AdditionalGL"].ToString(), 26);
                }

                sheet.Range[7, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = sheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyHeader(ref sheet, shet2EndxlsCol, "Vendor", companyId);
                reportUtility.FreezePage(ref sheet, 1, 8);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateBothCompanySheet(ref IWorksheet sheet, ReportUtility reportUtility, string sheetHeader, string sheetName, string companyId, string companyGroupId)
        {
            try
            {
                #region List data

                var dtParty = GetPartyBothInfo(companyId);
                var dvMainBody = new DataView(dtParty)
                {
                    Sort = "Party Name"
                };
                
                dtParty = dvMainBody.ToTable(true, "Id", "Company", "Party Group", "Party Category", "Party Sub Category", "Code", "Party Name", "VAT Resistration No", "Trade License No", "Debit Limit", "Credit Limit", "Party Account Group", "Currency", "Payment Term", "Tax Exemption", "DownPaymentGL Code", "DownPaymentGL", "DownPayment Budget", "DownPayment Activity", "DownPayment Budget RefNo", "ReconciliationGL Code", "ReconciliationGL", "Reconciliation Budget", "Reconciliation Activity", "Reconciliation Budget RefNo", "Customer", "Vendor", "AdditionalGL");
                if (dtParty.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;

                var _col3 = 3;

                reportUtility.SetMasterHeaderText(ref sheet, _rowL, _col, "Company");
                sheet[reportUtility.GetColumnNameForXls(_col) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _rowL].Merge();
                reportUtility.SetText(ref sheet, _rowL, _col + 2, dtParty.Rows[0]["Company"].ToString()); _rowL++;
                sheet[reportUtility.GetColumnNameForXls(_col3) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col3 + 2) + _rowL].Merge();

                _rowL = 6;
                _rowL++;

                for (int i = 0; i < dtParty.Columns.Count; i++)
                {
                    if (dtParty.Columns[i].ColumnName != "TotalRows" && dtParty.Columns[i].ColumnName != "Id" && dtParty.Columns[i].ColumnName != "Company")
                    {
                        _colIndex++;
                        reportUtility.SetHeaderText(ref sheet, _rowL, _colIndex, dtParty.Columns[i].ColumnName);
                    }
                }
                shet2EndxlsCol = _colIndex;

                for (int q = 0; q < dtParty.Rows.Count; q++)
                {
                    _rowL++;
                    
                    reportUtility.SetText(ref sheet, _rowL, 1, dtParty.Rows[q]["Party Group"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 2, dtParty.Rows[q]["Party Category"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 3, dtParty.Rows[q]["Party Sub Category"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 4, dtParty.Rows[q]["Code"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 5, dtParty.Rows[q]["Party Name"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 6, dtParty.Rows[q]["VAT Resistration No"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 7, dtParty.Rows[q]["Trade License No"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 8, dtParty.Rows[q]["Debit Limit"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 9, dtParty.Rows[q]["Credit Limit"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 10, dtParty.Rows[q]["Party Account Group"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 11, dtParty.Rows[q]["Currency"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 12, dtParty.Rows[q]["Payment Term"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 13, dtParty.Rows[q]["Tax Exemption"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 14, dtParty.Rows[q]["DownPaymentGL Code"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 15, dtParty.Rows[q]["DownPaymentGL"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 16, dtParty.Rows[q]["DownPayment Budget"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 17, dtParty.Rows[q]["DownPayment Activity"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 18, dtParty.Rows[q]["DownPayment Budget RefNo"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 19, dtParty.Rows[q]["ReconciliationGL Code"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 20, dtParty.Rows[q]["ReconciliationGL"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 21, dtParty.Rows[q]["Reconciliation Budget"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 22, dtParty.Rows[q]["Reconciliation Activity"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 23, dtParty.Rows[q]["Reconciliation Budget RefNo"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 24, dtParty.Rows[q]["Customer"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 25, dtParty.Rows[q]["Vendor"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 26, dtParty.Rows[q]["AdditionalGL"].ToString(), 26);
                }

                sheet.Range[7, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = sheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyHeader(ref sheet, shet2EndxlsCol, "Party", companyId);
                reportUtility.FreezePage(ref sheet, 1, 8);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateBothCompanyGroupSheet(ref IWorksheet sheet, ReportUtility reportUtility, string sheetHeader, string sheetName, string companyGroupId)
        {
            try
            {
                #region List data

                var dtParty = GetPartyBothCGInfo(companyGroupId);
                var dvMainBody = new DataView(dtParty)
                {
                    Sort = "Party Name"
                };
                dtParty = dvMainBody.ToTable(true, "Id", "CompanyGroup", "Code", "Party Name", "VAT Resistration No", "Trade License No", "Debit Limit", "Credit Limit", "Party Account Group", "Currency", "Payment Term", "Tax Exemption", "AdditionalGL", "DownPaymentGL", "ReconciliationGL", "Customer", "Vendor");

                if (dtParty.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;

                var _col3 = 3;

                reportUtility.SetMasterHeaderText(ref sheet, _rowL, _col, "Company Group");
                sheet[reportUtility.GetColumnNameForXls(_col) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _rowL].Merge();
                reportUtility.SetText(ref sheet, _rowL, _col + 2, dtParty.Rows[0]["CompanyGroup"].ToString()); _rowL++;
                sheet[reportUtility.GetColumnNameForXls(_col3) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col3 + 2) + _rowL].Merge();

                _rowL = 6;
                _rowL++;

                for (int i = 0; i < dtParty.Columns.Count; i++)
                {
                    if (dtParty.Columns[i].ColumnName != "TotalRows" && dtParty.Columns[i].ColumnName != "Id" && dtParty.Columns[i].ColumnName != "CompanyGroup")
                    {
                        _colIndex++;
                        reportUtility.SetHeaderText(ref sheet, _rowL, _colIndex, dtParty.Columns[i].ColumnName);
                    }
                }
                shet2EndxlsCol = _colIndex;

                for (int q = 0; q < dtParty.Rows.Count; q++)
                {
                    _rowL++;
                    reportUtility.SetText(ref sheet, _rowL, 1, dtParty.Rows[q]["Code"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 2, dtParty.Rows[q]["Party Name"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 3, dtParty.Rows[q]["VAT Resistration No"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 4, dtParty.Rows[q]["Trade License No"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 5, dtParty.Rows[q]["Debit Limit"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 6, dtParty.Rows[q]["Credit Limit"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 7, dtParty.Rows[q]["Party Account Group"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 8, dtParty.Rows[q]["Currency"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 9, dtParty.Rows[q]["Payment Term"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 10, dtParty.Rows[q]["Tax Exemption"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 11, dtParty.Rows[q]["AdditionalGL"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 12, dtParty.Rows[q]["DownPaymentGL"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 13, dtParty.Rows[q]["ReconciliationGL"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 14, dtParty.Rows[q]["Customer"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 15, dtParty.Rows[q]["Vendor"].ToString(), 26);
                }

                sheet.Range[7, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = sheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyGroupHeader(ref sheet, shet2EndxlsCol, "Party", companyGroupId);
                reportUtility.FreezePage(ref sheet, 1, 8);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateBothCompanyPlantSheet(ref IWorksheet sheet, ReportUtility reportUtility, string sheetHeader, string sheetName, string companyId, string plantId)
        {
            try
            {
                #region List data

                var dtParty = GetPartyPlantBothInfo(companyId, plantId);
                var dvMainBody = new DataView(dtParty)
                {
                    Sort = "Party Name"
                };
                dtParty = dvMainBody.ToTable(true, "Id", "Company", "Plant", "Party Group", "Party Category", "Party Sub Category", "Code", "Party Name", "VAT Resistration No", "Trade License No", "Debit Limit", "Credit Limit", "Party Account Group", "Currency", "Payment Term", "Tax Exemption", "DownPaymentGL Code", "DownPaymentGL", "DownPayment Budget", "DownPayment Activity", "DownPayment Budget RefNo", "ReconciliationGL Code", "ReconciliationGL", "Reconciliation Budget", "Reconciliation Activity", "Reconciliation Budget RefNo", "Customer", "Vendor", "AdditionalGL");

                if (dtParty.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;

                reportUtility.SetMasterHeaderText(ref sheet, _rowL, _col, "Company");
                sheet[reportUtility.GetColumnNameForXls(_col) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _rowL].Merge();
                reportUtility.SetText(ref sheet, _rowL, _col + 2, dtParty.Rows[0]["Company"].ToString()); _rowL++;
                reportUtility.SetMasterHeaderText(ref sheet, _rowL, _col, "Plant");
                sheet[reportUtility.GetColumnNameForXls(_col) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _rowL].Merge();
                reportUtility.SetText(ref sheet, _rowL, _col + 2, dtParty.Rows[0]["Plant"].ToString());
                _rowL = 6;
                _rowL++;

                for (int i = 0; i < dtParty.Columns.Count; i++)
                {
                    if (dtParty.Columns[i].ColumnName != "TotalRows" && dtParty.Columns[i].ColumnName != "Id" && dtParty.Columns[i].ColumnName != "Company" && dtParty.Columns[i].ColumnName != "Plant")
                    {
                        _colIndex++;
                        reportUtility.SetHeaderText(ref sheet, _rowL, _colIndex, dtParty.Columns[i].ColumnName);
                    }
                }
                shet2EndxlsCol = _colIndex;

                for (int q = 0; q < dtParty.Rows.Count; q++)
                {
                    _rowL++;
                    reportUtility.SetText(ref sheet, _rowL, 1, dtParty.Rows[q]["Party Group"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 2, dtParty.Rows[q]["Party Category"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 3, dtParty.Rows[q]["Party Sub Category"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 4, dtParty.Rows[q]["Code"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 5, dtParty.Rows[q]["Party Name"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 6, dtParty.Rows[q]["VAT Resistration No"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 7, dtParty.Rows[q]["Trade License No"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 8, dtParty.Rows[q]["Debit Limit"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 9, dtParty.Rows[q]["Credit Limit"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 10, dtParty.Rows[q]["Party Account Group"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 11, dtParty.Rows[q]["Currency"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 12, dtParty.Rows[q]["Payment Term"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 13, dtParty.Rows[q]["Tax Exemption"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 14, dtParty.Rows[q]["DownPaymentGL Code"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 15, dtParty.Rows[q]["DownPaymentGL"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 16, dtParty.Rows[q]["DownPayment Budget"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 17, dtParty.Rows[q]["DownPayment Activity"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 18, dtParty.Rows[q]["DownPayment Budget RefNo"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 19, dtParty.Rows[q]["ReconciliationGL Code"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 20, dtParty.Rows[q]["ReconciliationGL"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 21, dtParty.Rows[q]["Reconciliation Budget"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 22, dtParty.Rows[q]["Reconciliation Activity"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 23, dtParty.Rows[q]["Reconciliation Budget RefNo"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 24, dtParty.Rows[q]["Customer"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 25, dtParty.Rows[q]["Vendor"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 26, dtParty.Rows[q]["AdditionalGL"].ToString(), 26);
                }

                sheet.Range[7, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = sheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyHeader(ref sheet, shet2EndxlsCol, "Party", companyId);
                reportUtility.FreezePage(ref sheet, 1, 8);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreatePlantCustomerSheet(ref IWorksheet sheet, ReportUtility reportUtility, string sheetHeader, string sheetName, string companyId, string plantId)
        {
            try
            {
                #region List data

                var dtParty = GetPartyPlantCustomerInfo(companyId, plantId);
                var dvMainBody = new DataView(dtParty)
                {
                    Sort = "Customer Name"
                };

                dtParty = dvMainBody.ToTable(true, "Id", "Company", "Plant", "Party Group", "Party Category", "Party Sub Category", "Code", "Customer Name", "VAT Resistration No", "Trade License No", "Debit Limit", "Credit Limit", "Party Account Group", "Currency", "Payment Term", "Tax Exemption", "DownPaymentGL Code", "DownPaymentGL", "DownPayment Budget", "DownPayment Activity", "DownPayment Budget RefNo", "ReconciliationGL Code", "ReconciliationGL", "Reconciliation Budget", "Reconciliation Activity", "Reconciliation Budget RefNo", "AdditionalGL");
                
                if (dtParty.Rows.Count == 0)
                    throw new Exception("No data found!");

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;

                reportUtility.SetMasterHeaderText(ref sheet, _rowL, _col, "Company");
                sheet[reportUtility.GetColumnNameForXls(_col) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _rowL].Merge();
                reportUtility.SetText(ref sheet, _rowL, _col + 2, dtParty.Rows[0]["Company"].ToString());

                _rowL++;
                reportUtility.SetMasterHeaderText(ref sheet, _rowL, _col, "Plant");
                sheet[reportUtility.GetColumnNameForXls(_col) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _rowL].Merge();
                reportUtility.SetText(ref sheet, _rowL, _col + 2, dtParty.Rows[0]["Plant"].ToString());
                _rowL = 6;
                _rowL++;

                for (int i = 0; i < dtParty.Columns.Count; i++)
                {
                    if (dtParty.Columns[i].ColumnName != "TotalRows" && dtParty.Columns[i].ColumnName != "Id" && dtParty.Columns[i].ColumnName != "Company" && dtParty.Columns[i].ColumnName != "Plant")
                    {
                        _colIndex++;
                        reportUtility.SetHeaderText(ref sheet, _rowL, _colIndex, dtParty.Columns[i].ColumnName);
                    }
                }
                shet2EndxlsCol = _colIndex;

                for (int q = 0; q < dtParty.Rows.Count; q++)
                {
                    _rowL++;
                    
                    reportUtility.SetText(ref sheet, _rowL, 1, dtParty.Rows[q]["Party Group"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 2, dtParty.Rows[q]["Party Category"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 3, dtParty.Rows[q]["Party Sub Category"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 4, dtParty.Rows[q]["Code"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 5, dtParty.Rows[q]["Customer Name"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 6, dtParty.Rows[q]["VAT Resistration No"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 7, dtParty.Rows[q]["Trade License No"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 8, dtParty.Rows[q]["Debit Limit"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 9, dtParty.Rows[q]["Credit Limit"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 10, dtParty.Rows[q]["Party Account Group"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 11, dtParty.Rows[q]["Currency"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 12, dtParty.Rows[q]["Payment Term"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 13, dtParty.Rows[q]["Tax Exemption"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 14, dtParty.Rows[q]["DownPaymentGL Code"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 15, dtParty.Rows[q]["DownPaymentGL"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 16, dtParty.Rows[q]["DownPayment Budget"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 17, dtParty.Rows[q]["DownPayment Activity"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 18, dtParty.Rows[q]["DownPayment Budget RefNo"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 19, dtParty.Rows[q]["ReconciliationGL Code"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 20, dtParty.Rows[q]["ReconciliationGL"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 21, dtParty.Rows[q]["Reconciliation Budget"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 22, dtParty.Rows[q]["Reconciliation Activity"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 23, dtParty.Rows[q]["Reconciliation Budget RefNo"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 24, dtParty.Rows[q]["AdditionalGL"].ToString(), 26);
                }

                sheet.Range[7, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = sheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyHeader(ref sheet, shet2EndxlsCol, "Customer", companyId);
                reportUtility.FreezePage(ref sheet, 1, 8);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreatePlantVendorSheet(ref IWorksheet sheet, ReportUtility reportUtility, string sheetHeader, string sheetName, string companyId, string plantId)
        {
            try
            {
                #region List data

                var dtParty = GetPartyPlantVendorInfo(companyId, plantId);
                var dvMainBody = new DataView(dtParty)
                {
                    Sort = "Vendor Name"
                };
                
                dtParty = dvMainBody.ToTable(true, "Id", "Company", "Plant", "Party Group", "Party Category", "Party Sub Category", "Code", "Vendor Name", "VAT Resistration No", "Trade License No", "Debit Limit", "Credit Limit", "Party Account Group", "Currency", "Payment Term", "Tax Exemption", "DownPaymentGL Code", "DownPaymentGL", "DownPayment Budget", "DownPayment Activity", "DownPayment Budget RefNo", "ReconciliationGL Code", "ReconciliationGL", "Reconciliation Budget", "Reconciliation Activity", "Reconciliation Budget RefNo", "AdditionalGL");

                if (dtParty.Rows.Count == 0)
                {
                    throw new Exception("No Data Found !!!");
                }

                #endregion List data

                var _col = 1;
                var _rowL = 5;
                var _colIndex = 0;
                var shet2EndxlsCol = _col;

                reportUtility.SetMasterHeaderText(ref sheet, _rowL, _col, "Company");
                sheet[reportUtility.GetColumnNameForXls(_col) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _rowL].Merge();
                reportUtility.SetText(ref sheet, _rowL, _col + 2, dtParty.Rows[0]["Company"].ToString());
                _rowL++;
                reportUtility.SetMasterHeaderText(ref sheet, _rowL, _col, "Plant");
                sheet[reportUtility.GetColumnNameForXls(_col) + _rowL + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _rowL].Merge();
                reportUtility.SetText(ref sheet, _rowL, _col + 2, dtParty.Rows[0]["Plant"].ToString());
                _rowL = 6;
                _rowL++;

                for (int i = 0; i < dtParty.Columns.Count; i++)
                {
                    if (dtParty.Columns[i].ColumnName != "TotalRows" && dtParty.Columns[i].ColumnName != "Id" && dtParty.Columns[i].ColumnName != "Company" && dtParty.Columns[i].ColumnName != "Plant")
                    {
                        _colIndex++;
                        reportUtility.SetHeaderText(ref sheet, _rowL, _colIndex, dtParty.Columns[i].ColumnName);
                    }
                }
                shet2EndxlsCol = _colIndex;

                for (int q = 0; q < dtParty.Rows.Count; q++)
                {
                    _rowL++;
                    
                    reportUtility.SetText(ref sheet, _rowL, 1, dtParty.Rows[q]["Party Group"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 2, dtParty.Rows[q]["Party Category"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 3, dtParty.Rows[q]["Party Sub Category"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 4, dtParty.Rows[q]["Code"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 5, dtParty.Rows[q]["Vendor Name"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 6, dtParty.Rows[q]["VAT Resistration No"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 7, dtParty.Rows[q]["Trade License No"].ToString(), 20);
                    reportUtility.SetText(ref sheet, _rowL, 8, dtParty.Rows[q]["Debit Limit"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 9, dtParty.Rows[q]["Credit Limit"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 10, dtParty.Rows[q]["Party Account Group"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 11, dtParty.Rows[q]["Currency"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 12, dtParty.Rows[q]["Payment Term"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 13, dtParty.Rows[q]["Tax Exemption"].ToString(), 15);
                    reportUtility.SetText(ref sheet, _rowL, 14, dtParty.Rows[q]["DownPaymentGL Code"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 15, dtParty.Rows[q]["DownPaymentGL"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 16, dtParty.Rows[q]["DownPayment Budget"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 17, dtParty.Rows[q]["DownPayment Activity"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 18, dtParty.Rows[q]["DownPayment Budget RefNo"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 19, dtParty.Rows[q]["ReconciliationGL Code"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 20, dtParty.Rows[q]["ReconciliationGL"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 21, dtParty.Rows[q]["Reconciliation Budget"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 22, dtParty.Rows[q]["Reconciliation Activity"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 23, dtParty.Rows[q]["Reconciliation Budget RefNo"].ToString(), 26);
                    reportUtility.SetText(ref sheet, _rowL, 24, dtParty.Rows[q]["AdditionalGL"].ToString(), 26);
                }

                sheet.Range[7, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Name = sheetName;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyHeader(ref sheet, shet2EndxlsCol, "Vendor", companyId);
                reportUtility.FreezePage(ref sheet, 1, 8);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region Party payment status report
        private DataTable GetPartyPaymentStatusReport(string companyGroupId, string companyId, string plantId, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId)
        {
            var cmdText = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                            SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration, ISNULL(VD.DrAmount,0) AS DrAmount, ISNULL(VD.CrAmount,0) AS CrAmount
                            , CC.CompanyCurrencyId, ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) AS CompanyCurrencyCrAmount, C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                            , VD.GLGeneralInfoId,GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName
                             ,Particular =concat( STUFF((select distinct ','+xpA.UserName+ ' '+'('+ xp.UserName+')' from
														TRN.VoucherDetail XVD JOIN [HKP].[Party] AS XP ON XP.Id=XVD.PartyId
                                                        JOIN HKP.Activity AS XPA ON XPA.Id=XVD.ActivityId
													    where	XVD.VoucherId=V.Id AND XVD.PartyId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												,STUFF((select distinct ','+xp.AccountTitle from
														TRN.VoucherDetail XVD JOIN MST.BankMaster AS XP ON XP.Id=XVD.BankMasterId
													where	XVD.VoucherId=V.Id AND XVD.BankMasterId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												 , STUFF((select distinct ','+xp.UserName from
														TRN.VoucherDetail XVD JOIN MST.CashMaster AS XP ON XP.Id=XVD.CashMasterId
													where	XVD.VoucherId=V.Id AND XVD.CashMasterId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												 ,STUFF((select distinct ','+xp.EmployeeName from
														TRN.VoucherDetail XVD JOIN [dbo].[EmployeeInformation] AS XP ON XP.SystemId=XVD.EmployeeId
													where	XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                , STUFF((select distinct ','+xp.UserName from
														TRN.VoucherDetail XVD JOIN HKP.Activity AS XP ON XP.Id=XVD.ActivityId
													where	XVD.VoucherId=V.Id AND XVD.PartyId is null AND XVD.CashMasterId IS NULL AND XVD.BankMasterId IS NULL AND XVD.EmployeeId IS NULL
													 AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                                       
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId AND P.Id=VD.PartyId
                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                            ) AS CC ON CC.VoucherDetailId=VD.Id
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND VD.PartyId='" + partyId + "' AND V.PostingDate BETWEEN '" + fromDate.ToDbDate() + "' AND '" + toDate + @"'
                            AND V.SourceType<>'OpeningBalance'";
            if (!string.IsNullOrEmpty(partyPlantId))
                cmdText += " AND VD.PartyPlantId='" + partyPlantId + "'";
            if (!string.IsNullOrEmpty(gSTINId))
                cmdText += " AND PP.GSTIN='" + gSTINId + "'";
            if (active)
                cmdText += " ORDER BY VD.GLGeneralInfoId, V.PostingDate, V.VoucherNo ASC";
            else
                cmdText += " ORDER BY V.PostingDate, V.VoucherNo ASC";

            return _sqlRepository.GetDataTable(cmdText);
        }

        private List<Dictionary<string, object>> GetPartyPaymentStatusOpeningBalance(string companyGroupId, string companyId, string plantId, string partyId, string partyPlantId, string fromDate)
        {
            var sql = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                        SELECT SUM(DrAmount) - SUM(CrAmount) AS OB, CompanyCurrencyId, SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyCurrencyOB FROM (
                        SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.PartyId='" + partyId + "' AND V.PostingDate < '" + fromDate.ToDbDate() + "'";
            if (!string.IsNullOrEmpty(partyPlantId))
                sql += " AND VD.PartyPlantId='" + partyPlantId + "'";
            sql += @" GROUP BY CC.CompanyCurrencyId
                    UNION
                    SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount, CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                    FROM [TRN].[Voucher] AS V
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                    LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                    FROM [TRN].[VoucherDetailCurrency] AS VDC
	                    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                    ) AS CC ON CC.VoucherDetailId=VD.Id
                    WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.PartyId='" + partyId + "' AND V.PostingDate ='" + fromDate.ToDbDate() + "' AND V.SourceType='OpeningBalance'";
            if (!string.IsNullOrEmpty(partyPlantId))
                sql += " AND VD.PartyPlantId='" + partyPlantId + "'";
            sql += " GROUP BY CC.CompanyCurrencyId) AS X GROUP BY X.CompanyCurrencyId";
            return _sqlRepository.GetDataCollection(sql);
        }
        private List<Dictionary<string, object>> GetPartyPaymentOpeningBalance(string companyGroupId, string companyId, string plantId, string partyId, string partyPlantId, string fromDate)
        {
            var sql = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                        SELECT SUM(DrAmount) - SUM(CrAmount) AS OB, CompanyCurrencyId, SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyCurrencyOB FROM (
                        SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.PartyId='" + partyId + "' AND V.PostingDate < '" + fromDate.ToDbDate() + "'";
            if (!string.IsNullOrEmpty(partyPlantId))
                sql += " AND VD.PartyPlantId='" + partyPlantId + "'";
            sql += @" GROUP BY CC.CompanyCurrencyId
                    UNION
                    SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount, CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                    FROM [TRN].[Voucher] AS V
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                    LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                    FROM [TRN].[VoucherDetailCurrency] AS VDC
	                    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                    ) AS CC ON CC.VoucherDetailId=VD.Id
                    WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.PartyId='" + partyId + "' AND V.PostingDate ='" + fromDate.ToDbDate() + "' AND V.SourceType='OpeningBalance'";
            if (!string.IsNullOrEmpty(partyPlantId))
                sql += " AND VD.PartyPlantId='" + partyPlantId + "'";
            sql += " GROUP BY CC.CompanyCurrencyId) AS X GROUP BY X.CompanyCurrencyId";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IWorkbook GetPartyPaymentStatusReportGL(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId)
        {
            try
            {
                var row = 6;
                var colLast = row;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "PartyPaymentStatusReport";

                // Get BankMaster data
                var partyMaster = GetPartyData(partyId, partyPlantId).Select().FirstOrDefault();

                //// Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, partyMaster["Code"].ToString() + " - " + partyMaster["PartyName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(3) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                //var cashCurrencyId = partyMaster["CurrencyId"].ToString();
                var cashCurrencyId = companyCurrencyId.ToString();
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(6) + row + ":" + reportUtility.GetColumnNameForXls(8) + row].Merge();
                    colLast = 9;
                }

                // Detail Header
                row++;
                int COL = 1;
                reportUtility.SetHeaderText(ref sheet, row, COL, "GL", 20); int colGL = COL; COL++;//1
                reportUtility.SetHeaderText(ref sheet, row, COL, "Posting Date", 10); int colPostingDate = COL; COL++;//2
                reportUtility.SetHeaderText(ref sheet, row, COL, "Voucher No", 12); int colVoucherNo = COL; COL++;//3
                //reportUtility.SetHeaderText(ref sheet, row, COL, "Doc Ref", 10); int colDocRef = COL; COL++;//4
                //reportUtility.SetHeaderText(ref sheet, row, COL, "Doc Date", 10); int colDocDate = COL; COL++;//5
                reportUtility.SetHeaderText(ref sheet, row, COL, "Particulars", 30); int colParticulars = COL; COL++;//6


                reportUtility.SetHeaderText(ref sheet, row, COL, "Narration", 20); int colNarration = COL; COL++;//7
                reportUtility.SetHeaderText(ref sheet, row, COL, "Debit", 14, ExcelHAlign.HAlignRight); int colDebit = COL; COL++;//8
                reportUtility.SetHeaderText(ref sheet, row, COL, "Credit", 14, ExcelHAlign.HAlignRight); int colCredit = COL; COL++;//9
                reportUtility.SetHeaderText(ref sheet, row, COL, "Balance", 12, ExcelHAlign.HAlignRight); int colBalance = COL; COL++;//10
                reportUtility.SetHeaderText(ref sheet, row, COL, "Dr/Cr", 4, ExcelHAlign.HAlignRight); int colCrDr = COL; COL++;//11

                //reportUtility.SetHeaderText(ref sheet, row, 11, "Party Balance", 14, ExcelHAlign.HAlignRight);
                //reportUtility.SetHeaderText(ref sheet, row, 12, "Dr/Cr", 4, ExcelHAlign.HAlignRight);

                int colDebit2 = 0;
                int colCredit2 = 0;
                int colBalance2 = 0;


                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    COL = colCrDr;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Debit", 14, ExcelHAlign.HAlignRight); colDebit2 = COL; COL++;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Credit", 14, ExcelHAlign.HAlignRight); colCredit2 = COL; COL++;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Balance", ExcelHAlign.HAlignRight); colBalance2 = COL; COL++;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Dr/Cr", 4, ExcelHAlign.HAlignRight); colCrDr = COL;

                }
                colLast = COL;
                row++;

                // Get Cash transaction data.
                var ledgerData = GetPartyPaymentStatusReport(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId);
                var obValParty = GetPartyPaymentStatusOpeningBalance(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate);
                var clValParty = GetPartyPaymentOpeningBalance(companyGroupId, companyId, plantId, partyId, partyPlantId, toDate);


                if (ledgerData.Rows.Count > 0)
                {
                    var dt = ledgerData.AsEnumerable().OrderBy(r => r["GLGeneralInfoId"])
                            .GroupBy(r => new { GLGeneralInfoId = r["GLGeneralInfoId"] })
                            .Select(g => g.OrderBy(r => r["GLGeneralInfoId"]).First())
                            .CopyToDataTable();
                    var isOB = true;
                    var lastClosing = string.Empty;

                    reportUtility.SetTextLeftAlign(ref sheet, row, colGL, "Party Opening Balance", true, ExcelHAlign.HAlignLeft);
                    sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

                    if (obValParty.Count > 0)
                    {
                        var obparty = Convert.ToDouble(obValParty[0]["OB"]); ;

                        reportUtility.SetText(ref sheet, row, colCredit, obparty, true);
                        sheet.Range[row, colCredit].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        row++;
                    }


                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        var data = ledgerData.AsEnumerable()
                            .Where(r => r.Field<string>("GLGeneralInfoId") == dt.Rows[j]["GLGeneralInfoId"].ToString())
                            .OrderBy(r => r["PostingDate"])
                            .CopyToDataTable();

                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colPostingDate) + row].Merge();
                        reportUtility.SetText(ref sheet, row, colGL, data.Rows[0]["GLGeneralInfoCode"].ToString() + "-" + data.Rows[0]["GLGeneralInfoName"].ToString());
                        sheet.Range[row, colGL].CellStyle.Font.Bold = true;
                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colCrDr) + row].BorderAround(ExcelLineStyle.Hair);
                        row++;

                        reportUtility.SetText(ref sheet, row, colGL, "Opening Balance", true);
                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

                        // Get Cash opening balance data.
                        //if (obVal.Rows.Count > 0)//&& isOB
                        //{
                        var obVal = GetPartyOpeningBalanceGroupByGL(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, dt.Rows[j]["GLGeneralInfoId"].ToString(), partyType.ToString()).Select().FirstOrDefault();
                        if (obVal != null)
                        {
                            var ob = Convert.ToDouble(obVal["OB"]); ;
                            reportUtility.SetText(ref sheet, row, colCredit, ob, true);
                            sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet.Range[row, colCredit].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";

                            if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                            {
                                reportUtility.SetText(ref sheet, row, colCredit2, ob, true);
                                sheet.Range[row, colBalance2].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                            }

                            isOB = false;
                        }
                        row++;
                        for (int i = 0; i < data.Rows.Count; i++)
                        {

                            reportUtility.SetText(ref sheet, row, colGL, data.Rows[i]["GLGeneralInfoCode"].ToString() + "-" + data.Rows[i]["GLGeneralInfoName"].ToString());
                            reportUtility.SetText(ref sheet, row, colPostingDate, data.Rows[i]["PostingDate"].ToString());
                            reportUtility.SetTextWrapText(ref sheet, row, colVoucherNo, data.Rows[i]["VoucherNo"].ToString());
                            ////reportUtility.SetTextWrapText(ref sheet, row, colDocRef, data.Rows[i]["DocRefNo"].ToString());
                            ////reportUtility.SetTextWrapText(ref sheet, row, colDocDate, data.Rows[i]["DocDate"].ToString());

                            reportUtility.SetTextWrapText(ref sheet, row, colParticulars, data.Rows[i]["Particular"].ToString());

                            reportUtility.SetTextWrapText(ref sheet, row, colNarration, data.Rows[i]["Narration"].ToString());
                            sheet[row, colNarration].ColumnWidth = 20;
                            sheet.Range[row, colNarration].WrapText = true;
                            reportUtility.SetText(ref sheet, row, colDebit, Convert.ToDouble(data.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, colCredit, Convert.ToDouble(data.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                            sheet.Range[row, colBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colDebit) + row + "-" + reportUtility.GetColumnNameForXls(colCredit) + row + ")";
                            sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet.Range[row, colBalance].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";

                            // Base currency checking
                            if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                            {
                                reportUtility.SetText(ref sheet, row, colDebit2, Convert.ToDouble(data.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                                reportUtility.SetText(ref sheet, row, colCredit2, Convert.ToDouble(data.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                                sheet.Range[row, colBalance2].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colCredit2) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(10) + row + "-" + reportUtility.GetColumnNameForXls(11) + row + ")";
                                sheet.Range[row, colBalance2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                                sheet.Range[row, colBalance2].VerticalAlignment = ExcelVAlign.VAlignTop;
                                sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                            }
                            row++;
                        }

                        reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);

                        //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                        //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                        //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                        //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                        //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                        //sheet[row, 7].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(7) + (row - 1) + ":" + clsStaticInfo.GetxlsCol(7) + row + ")";
                        sheet.Range[row, colNarration].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colNarration) + (row - 1) + ":" + reportUtility.GetColumnNameForXls(colNarration) + row + ")";
                        // sheet.Range[row, 7].NumberFormat = oRU.NumberFormatDecimalTwo();
                        sheet.Range[row, colNarration].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();

                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

                        sheet.Range[row, colCredit].Formula = "=" + reportUtility.GetColumnNameForXls(colBalance) + (row - 1);
                        lastClosing = "=" + reportUtility.GetColumnNameForXls(7) + (row - 1);
                        sheet.Range[row, colCredit].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCredit].CellStyle.Font.Bold = true;
                        sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";

                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                        {
                            sheet.Range[row, colCredit2].Formula = "=" + reportUtility.GetColumnNameForXls(colCredit2) + (row - 1);
                            sheet.Range[row, colCredit2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet.Range[row, colCredit2].CellStyle.Font.Bold = true;
                            sheet.Range[row, colBalance2].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        }
                        row++;

                    }
                    row++;
                    reportUtility.SetTextLeftAlign(ref sheet, row, colGL, "Party Closing Balance", true, ExcelHAlign.HAlignLeft);
                    sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();
                    if (clValParty.Count > 0)
                    {
                        var clparty = Convert.ToDouble(clValParty[0]["OB"]); ;
                        reportUtility.SetText(ref sheet, row, colBalance, clparty, true);
                        sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        row++;
                    }
                    //sheet.Range[row, 9].Formula = "=" + reportUtility.GetColumnNameForXls(9) + (row - 1);
                    //lastClosing = "=" + reportUtility.GetColumnNameForXls(9) + (row - 1);
                    //sheet.Range[row, 9].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                    //sheet.Range[row, 9].CellStyle.Font.Bold = true;
                    //sheet.Range[row, 10].Formula = "IF(" + reportUtility.GetColumnNameForXls(10 - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                    if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                    {
                        sheet.Range[row, colCredit2].Formula = "=" + reportUtility.GetColumnNameForXls(colCredit2) + (row - 1);
                        sheet.Range[row, colCredit2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCredit2].CellStyle.Font.Bold = true;
                        sheet.Range[row, colBalance2].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                    }

                }
                sheet.UsedRange.WrapText = true;
                //sheet.UsedRange.AutofitRows();

                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Party Payment status Report", companyId, plantId, plantName, null);
                reportUtility.SetText(ref sheet, 5, colLast, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(colGL) + 5 + ":" + reportUtility.GetColumnNameForXls(colLast) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }


        private DataTable GetPartyPaymentStatusPlantLedgerByGL(string companyGroupId, string companyId, string plantId, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId)
        {
            var cmdText = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                            SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration, ISNULL(VD.DrAmount,0) AS DrAmount, ISNULL(VD.CrAmount,0) AS CrAmount
                            , CC.CompanyCurrencyId, ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) AS CompanyCurrencyCrAmount, C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                            , VD.GLGeneralInfoId,GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName
                             ,Particular =concat( STUFF((select distinct ','+xpA.UserName+ ' '+'('+ xp.UserName+')' from
														TRN.VoucherDetail XVD JOIN [HKP].[Party] AS XP ON XP.Id=XVD.PartyId
                                                        JOIN HKP.Activity AS XPA ON XPA.Id=XVD.ActivityId
													    where	XVD.VoucherId=V.Id AND XVD.PartyId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												,STUFF((select distinct ','+xp.AccountTitle from
														TRN.VoucherDetail XVD JOIN MST.BankMaster AS XP ON XP.Id=XVD.BankMasterId
													where	XVD.VoucherId=V.Id AND XVD.BankMasterId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												 , STUFF((select distinct ','+xp.UserName from
														TRN.VoucherDetail XVD JOIN MST.CashMaster AS XP ON XP.Id=XVD.CashMasterId
													where	XVD.VoucherId=V.Id AND XVD.CashMasterId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												 ,STUFF((select distinct ','+xp.EmployeeName from
														TRN.VoucherDetail XVD JOIN [dbo].[EmployeeInformation] AS XP ON XP.SystemId=XVD.EmployeeId
													where	XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                , STUFF((select distinct ','+xp.UserName from
														TRN.VoucherDetail XVD JOIN HKP.Activity AS XP ON XP.Id=XVD.ActivityId
													where	XVD.VoucherId=V.Id AND XVD.PartyId is null AND XVD.CashMasterId IS NULL AND XVD.BankMasterId IS NULL AND XVD.EmployeeId IS NULL
													 AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                                       
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId AND P.Id=VD.PartyId
                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                            ) AS CC ON CC.VoucherDetailId=VD.Id
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND VD.PartyId='" + partyId + "' AND V.PostingDate BETWEEN '" + fromDate.ToDbDate() + "' AND '" + toDate + @"'
                            AND V.SourceType<>'OpeningBalance'";
            if (!string.IsNullOrEmpty(partyPlantId))
                cmdText += " AND VD.PartyPlantId='" + partyPlantId + "'";
            if (!string.IsNullOrEmpty(gSTINId))
                cmdText += " AND PP.GSTIN='" + gSTINId + "'";
            if (active)
                cmdText += " ORDER BY VD.GLGeneralInfoId, V.PostingDate, V.VoucherNo ASC";
            else
                cmdText += " ORDER BY V.PostingDate, V.VoucherNo ASC";

            return _sqlRepository.GetDataTable(cmdText);
        }

        private List<Dictionary<string, object>> GetPartyPaymentStatusOpeningBalanceByGL(string companyGroupId, string companyId, string plantId, string partyId, string partyPlantId, string fromDate)
        {
            var sql = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                        SELECT SUM(DrAmount) - SUM(CrAmount) AS OB, CompanyCurrencyId, SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyCurrencyOB FROM (
                        SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.PartyId='" + partyId + "' AND V.PostingDate < '" + fromDate.ToDbDate() + "'";
            if (!string.IsNullOrEmpty(partyPlantId))
                sql += " AND VD.PartyPlantId='" + partyPlantId + "'";
            sql += @" GROUP BY CC.CompanyCurrencyId
                    UNION
                    SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount, CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                    FROM [TRN].[Voucher] AS V
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                    LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                    FROM [TRN].[VoucherDetailCurrency] AS VDC
	                    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                    ) AS CC ON CC.VoucherDetailId=VD.Id
                    WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.PartyId='" + partyId + "' AND V.PostingDate ='" + fromDate.ToDbDate() + "' AND V.SourceType='OpeningBalance'";
            if (!string.IsNullOrEmpty(partyPlantId))
                sql += " AND VD.PartyPlantId='" + partyPlantId + "'";
            sql += " GROUP BY CC.CompanyCurrencyId) AS X GROUP BY X.CompanyCurrencyId";
            return _sqlRepository.GetDataCollection(sql);
        }

        private List<Dictionary<string, object>> GetPartyPaymentStatusOpeningBalanceByGL1(string companyGroupId, string companyId, string plantId, string partyId, string partyPlantId, string fromDate)
        {
            var sql = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                        SELECT SUM(DrAmount) - SUM(CrAmount) AS OB, CompanyCurrencyId, SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyCurrencyOB FROM (
                        SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.PartyId='" + partyId + "' AND V.PostingDate < '" + fromDate.ToDbDate() + "'";
            if (!string.IsNullOrEmpty(partyPlantId))
                sql += " AND VD.PartyPlantId='" + partyPlantId + "'";
            sql += @" GROUP BY CC.CompanyCurrencyId
                    UNION
                    SELECT SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount, CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                    FROM [TRN].[Voucher] AS V
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                    LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                    FROM [TRN].[VoucherDetailCurrency] AS VDC
	                    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                    ) AS CC ON CC.VoucherDetailId=VD.Id
                    WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND V.PlantId='" + plantId + "' AND VD.PartyId='" + partyId + "' AND V.PostingDate ='" + fromDate.ToDbDate() + "' AND V.SourceType='OpeningBalance'";
            if (!string.IsNullOrEmpty(partyPlantId))
                sql += " AND VD.PartyPlantId='" + partyPlantId + "'";
            sql += " GROUP BY CC.CompanyCurrencyId) AS X GROUP BY X.CompanyCurrencyId";
            return _sqlRepository.GetDataCollection(sql);
        }


        public IWorkbook GetPartyPaymentStatusReportGroupByGLXls(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId)
        {
            try
            {
                var row = 6;
                var colLast = row;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";

                // Get BankMaster data
                var partyMaster = GetPartyData(partyId, partyPlantId).Select().FirstOrDefault();

                //// Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, partyMaster["Code"].ToString() + " - " + partyMaster["PartyName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(3) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                //var cashCurrencyId = partyMaster["CurrencyId"].ToString();
                var cashCurrencyId = companyCurrencyId.ToString();
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(6) + row + ":" + reportUtility.GetColumnNameForXls(8) + row].Merge();
                    colLast = 9;
                }

                // Detail Header
                row++;
                int COL = 1;
                reportUtility.SetHeaderText(ref sheet, row, COL, "GL", 20); int colGL = COL; COL++;//1
                reportUtility.SetHeaderText(ref sheet, row, COL, "Posting Date", 10); int colPostingDate = COL; COL++;//2
                reportUtility.SetHeaderText(ref sheet, row, COL, "Voucher No", 10); int colVoucherNo = COL; COL++;//3
                //reportUtility.SetHeaderText(ref sheet, row, COL, "Doc Ref", 10); int colDocRef = COL; COL++;//4
                //reportUtility.SetHeaderText(ref sheet, row, COL, "Doc Date", 10); int colDocDate = COL; COL++;//5
                reportUtility.SetHeaderText(ref sheet, row, COL, "Particulars", 20); int colParticulars = COL; COL++;//6


                reportUtility.SetHeaderText(ref sheet, row, COL, "Narration", 20); int colNarration = COL; COL++;//7
                reportUtility.SetHeaderText(ref sheet, row, COL, "Debit", 10, ExcelHAlign.HAlignRight); int colDebit = COL; COL++;//8
                reportUtility.SetHeaderText(ref sheet, row, COL, "Credit", 10, ExcelHAlign.HAlignRight); int colCredit = COL; COL++;//9
                reportUtility.SetHeaderText(ref sheet, row, COL, "Balance", 10, ExcelHAlign.HAlignRight); int colBalance = COL; COL++;//10
                reportUtility.SetHeaderText(ref sheet, row, COL, "Dr/Cr", 4, ExcelHAlign.HAlignRight); int colCrDr = COL; COL++;//11


                int colDebit2 = 0;
                int colCredit2 = 0;
                int colBalance2 = 0;


                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    COL = colCrDr;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Debit", 10, ExcelHAlign.HAlignRight); colDebit2 = COL; COL++;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Credit", 10, ExcelHAlign.HAlignRight); colCredit2 = COL; COL++;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Balance", 10, ExcelHAlign.HAlignRight); colBalance2 = COL; COL++;
                    reportUtility.SetHeaderText(ref sheet, row, COL, "Dr/Cr", 4, ExcelHAlign.HAlignRight); colCrDr = COL;

                }
                colLast = COL;
                row++;

                // Get Cash transaction data.
                var ledgerData = GetPartyPaymentStatusPlantLedgerByGL(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId);
                var obValParty = GetPartyPaymentStatusOpeningBalanceByGL(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate);
                var clValParty = GetPartyPaymentStatusOpeningBalanceByGL1(companyGroupId, companyId, plantId, partyId, partyPlantId, toDate);


                if (ledgerData.Rows.Count > 0)
                {
                    var dt = ledgerData.AsEnumerable().OrderBy(r => r["GLGeneralInfoId"])
                            .GroupBy(r => new { GLGeneralInfoId = r["GLGeneralInfoId"] })
                            .Select(g => g.OrderBy(r => r["GLGeneralInfoId"]).First())
                            .CopyToDataTable();
                    var isOB = true;
                    var lastClosing = string.Empty;

                    reportUtility.SetTextLeftAlign(ref sheet, row, colGL, "Party Opening Balance", true, ExcelHAlign.HAlignLeft);
                    sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

                    if (obValParty.Count > 0)
                    {
                        var obparty = Convert.ToDouble(obValParty[0]["OB"]); ;

                        reportUtility.SetText(ref sheet, row, colCredit, obparty, true);
                        sheet.Range[row, colCredit].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        row++;
                    }


                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        var data = ledgerData.AsEnumerable()
                            .Where(r => r.Field<string>("GLGeneralInfoId") == dt.Rows[j]["GLGeneralInfoId"].ToString())
                            .OrderBy(r => r["PostingDate"])
                            .CopyToDataTable();

                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colPostingDate) + row].Merge();
                        reportUtility.SetText(ref sheet, row, colGL, data.Rows[0]["GLGeneralInfoCode"].ToString() + "-" + data.Rows[0]["GLGeneralInfoName"].ToString());
                        sheet.Range[row, colGL].CellStyle.Font.Bold = true;
                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colCrDr) + row].BorderAround(ExcelLineStyle.Hair);
                        row++;

                        reportUtility.SetText(ref sheet, row, colGL, "Opening Balance", true);
                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

                        // Get Cash opening balance data.
                        //if (obVal.Rows.Count > 0)//&& isOB
                        //{
                        var obVal = GetPartyOpeningBalanceGroupByGL(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, dt.Rows[j]["GLGeneralInfoId"].ToString(), partyType.ToString()).Select().FirstOrDefault();
                        if (obVal != null)
                        {
                            var ob = Convert.ToDouble(obVal["OB"]); ;
                            reportUtility.SetText(ref sheet, row, colCredit, ob, true);
                            sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet.Range[row, colCredit].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";

                            if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                            {
                                reportUtility.SetText(ref sheet, row, colCredit2, ob, true);
                                sheet.Range[row, colBalance2].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                            }

                            isOB = false;
                        }
                        row++;
                        for (int i = 0; i < data.Rows.Count; i++)
                        {

                            reportUtility.SetText(ref sheet, row, colGL, data.Rows[i]["GLGeneralInfoCode"].ToString() + "-" + data.Rows[i]["GLGeneralInfoName"].ToString());
                            reportUtility.SetText(ref sheet, row, colPostingDate, data.Rows[i]["PostingDate"].ToString());
                            reportUtility.SetTextWrapText(ref sheet, row, colVoucherNo, data.Rows[i]["VoucherNo"].ToString());
                            ////reportUtility.SetTextWrapText(ref sheet, row, colDocRef, data.Rows[i]["DocRefNo"].ToString());
                            ////reportUtility.SetTextWrapText(ref sheet, row, colDocDate, data.Rows[i]["DocDate"].ToString());

                            reportUtility.SetTextWrapText(ref sheet, row, colParticulars, data.Rows[i]["Particular"].ToString());

                            reportUtility.SetTextWrapText(ref sheet, row, colNarration, data.Rows[i]["Narration"].ToString());
                            sheet[row, colNarration].ColumnWidth = 20;
                            sheet.Range[row, colNarration].WrapText = true;
                            reportUtility.SetText(ref sheet, row, colDebit, Convert.ToDouble(data.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, colCredit, Convert.ToDouble(data.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                            sheet.Range[row, colBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBalance) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colDebit) + row + "-" + reportUtility.GetColumnNameForXls(colCredit) + row + ")";
                            sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet.Range[row, colBalance].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";

                            // Base currency checking
                            if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                            {
                                reportUtility.SetText(ref sheet, row, colDebit2, Convert.ToDouble(data.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                                reportUtility.SetText(ref sheet, row, colCredit2, Convert.ToDouble(data.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                                sheet.Range[row, colBalance2].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colCredit2) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(10) + row + "-" + reportUtility.GetColumnNameForXls(11) + row + ")";
                                sheet.Range[row, colBalance2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                                sheet.Range[row, colBalance2].VerticalAlignment = ExcelVAlign.VAlignTop;
                                sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                            }
                            row++;
                        }

                        reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);

                        //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                        //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                        //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                        //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                        //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                        //sheet[row, 7].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(7) + (row - 1) + ":" + clsStaticInfo.GetxlsCol(7) + row + ")";
                        sheet.Range[row, colNarration].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colNarration) + (row - 1) + ":" + reportUtility.GetColumnNameForXls(colNarration) + row + ")";
                        // sheet.Range[row, 7].NumberFormat = oRU.NumberFormatDecimalTwo();
                        sheet.Range[row, colNarration].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();

                        sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

                        sheet.Range[row, colCredit].Formula = "=" + reportUtility.GetColumnNameForXls(colBalance) + (row - 1);
                        lastClosing = "=" + reportUtility.GetColumnNameForXls(7) + (row - 1);
                        sheet.Range[row, colCredit].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCredit].CellStyle.Font.Bold = true;
                        sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";

                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                        {
                            sheet.Range[row, colCredit2].Formula = "=" + reportUtility.GetColumnNameForXls(colCredit2) + (row - 1);
                            sheet.Range[row, colCredit2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet.Range[row, colCredit2].CellStyle.Font.Bold = true;
                            sheet.Range[row, colBalance2].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        }
                        row++;

                    }
                    row++;
                    reportUtility.SetTextLeftAlign(ref sheet, row, colGL, "Party Closing Balance", true, ExcelHAlign.HAlignLeft);
                    sheet.Range[reportUtility.GetColumnNameForXls(colGL) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();
                    if (clValParty.Count > 0)
                    {
                        var clparty = Convert.ToDouble(clValParty[0]["OB"]); ;
                        reportUtility.SetText(ref sheet, row, colBalance, clparty, true);
                        sheet.Range[row, colBalance].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCrDr].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        row++;
                    }
                    //sheet.Range[row, 9].Formula = "=" + reportUtility.GetColumnNameForXls(9) + (row - 1);
                    //lastClosing = "=" + reportUtility.GetColumnNameForXls(9) + (row - 1);
                    //sheet.Range[row, 9].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                    //sheet.Range[row, 9].CellStyle.Font.Bold = true;
                    //sheet.Range[row, 10].Formula = "IF(" + reportUtility.GetColumnNameForXls(10 - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                    if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                    {
                        sheet.Range[row, colCredit2].Formula = "=" + reportUtility.GetColumnNameForXls(colCredit2) + (row - 1);
                        sheet.Range[row, colCredit2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, colCredit2].CellStyle.Font.Bold = true;
                        sheet.Range[row, colBalance2].Formula = "IF(" + reportUtility.GetColumnNameForXls(colCredit2) + row + ">= 0, \"  Dr\", \"  Cr\")";
                    }

                }
                sheet.UsedRange.WrapText = true;
                //sheet.UsedRange.AutofitRows();

                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Party Ledger", companyId, plantId, plantName, null);
                reportUtility.SetText(ref sheet, 5, colLast, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(colGL) + 5 + ":" + reportUtility.GetColumnNameForXls(colLast) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }


        private DataTable GetPartyPaymentStatusPlantLedger3(string companyGroupId, string companyId, string plantId, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId, string partyType)
        {
            string tempPartyType = null;
            if (partyType == "Vendor" || partyType == "Customer" || partyType == "Director")
            {
                tempPartyType = partyType;
            }
            if (partyType == null || partyType == "null")
            {
                tempPartyType = "Vendor" + "','" + "Customer" + "','" + "Director";
            }
            var cmdText = @"
                            DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                            SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate
							, V.VoucherNo
							, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.DocRefNo
							, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate
							, V.Narration
							, SUM(ISNULL(VD.DrAmount,0)) AS DrAmount
							, SUM(ISNULL(VD.CrAmount,0)) AS CrAmount
                            , CC.CompanyCurrencyId
							, SUM(ISNULL(CC.CompanyCurrencyDrAmount, 0)) AS CompanyCurrencyDrAmount
							, SUM(ISNULL(CC.CompanyCurrencyCrAmount, 0)) AS CompanyCurrencyCrAmount

							, C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                            , VD.GLGeneralInfoId,GLGI.UserName AS GLGeneralInfoName
							, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName
							, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName

                             ,Particular =concat( STUFF((select distinct ','+xpA.UserName+ ' '+'('+ xp.UserName+')' from
														TRN.VoucherDetail XVD JOIN [HKP].[Party] AS XP ON XP.Id=XVD.PartyId
                                                        JOIN HKP.Activity AS XPA ON XPA.Id=XVD.ActivityId
													    where	XVD.VoucherId=V.Id AND XVD.PartyId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												,STUFF((select distinct ','+xp.AccountTitle from
														TRN.VoucherDetail XVD JOIN MST.BankMaster AS XP ON XP.Id=XVD.BankMasterId
													where	XVD.VoucherId=V.Id AND XVD.BankMasterId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												 , STUFF((select distinct ','+xp.UserName from
														TRN.VoucherDetail XVD JOIN MST.CashMaster AS XP ON XP.Id=XVD.CashMasterId
													where	XVD.VoucherId=V.Id AND XVD.CashMasterId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												 ,STUFF((select distinct ','+xp.EmployeeName from
														TRN.VoucherDetail XVD JOIN [dbo].[EmployeeInformation] AS XP ON XP.SystemId=XVD.EmployeeId
													where	XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                , STUFF((select distinct ','+xp.UserName from
														TRN.VoucherDetail XVD JOIN HKP.Activity AS XP ON XP.Id=XVD.ActivityId
													where	XVD.VoucherId=V.Id AND XVD.PartyId is null AND XVD.CashMasterId IS NULL AND XVD.BankMasterId IS NULL AND XVD.EmployeeId IS NULL
													 AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                                       
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId AND P.Id=VD.PartyId

                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                            ) AS CC ON CC.VoucherDetailId=VD.Id

                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + @"' 
							AND V.PlantId='" + plantId + "' AND VD.PartyId='" + partyId + "' AND VD.PartyType IN ('" + tempPartyType + "') AND V.PostingDate BETWEEN '" + fromDate + "' AND '" + toDate + @"'
                            AND V.SourceType<>'OpeningBalance' 
                            AND V.Id NOT IN(SELECT VoucherId FROM TRN.InvoiceWriteOff  WHERE SourceType='VendorAdvanceWriteOff')
                            --AND V.SourceType NOT IN ('OpeningBalance','VendorAdvanceWriteOff')
                            
                                ";

            if (!string.IsNullOrEmpty(partyPlantId))
                cmdText += " AND VD.PartyPlantId='" + partyPlantId + "'";
            if (!string.IsNullOrEmpty(gSTINId))
                cmdText += " AND PP.GSTIN='" + gSTINId + "'";
            if (active)
                cmdText += @" GROUP BY V.PostingDate, V.VoucherNo, V.VoucherDate
                            , V.DocRefNo, V.DocDate, V.Narration
                            , CC.CompanyCurrencyId, C.Code, GLGI.AccountCode, PP.GSTIN,V.Id,VD.ActivityId
                            , VD.GLGeneralInfoId,GLGI.UserName, BGM.RefNo, BG.UserName,V.CurrencyId, A.UserName, P.Code , P.UserName , PP.UserName ORDER BY VD.GLGeneralInfoId, V.PostingDate, V.VoucherNo ASC";
            else
                cmdText += @"					 GROUP BY V.PostingDate, V.VoucherNo, V.VoucherDate
                            , V.DocRefNo, V.DocDate, V.Narration
                            , CC.CompanyCurrencyId, C.Code, GLGI.AccountCode, PP.GSTIN,V.Id,VD.ActivityId
                            , VD.GLGeneralInfoId,GLGI.UserName, BGM.RefNo, BG.UserName,V.CurrencyId, A.UserName, P.Code , P.UserName , PP.UserName ORDER BY V.PostingDate, V.VoucherNo ASC";

            return _sqlRepository.GetDataTable(cmdText);
        }

        public IWorkbook GetPartyPaymentStatusLedgerReport3(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId)
        {
            try
            {
                var row = 9;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";
                var colLast = 6;
                var colLast1 = 6;
                var col = 1;
                var StartRow = 9;

                //sheet = null;

                // Get Party Master
                var partyMaster = _partyService.Find(partyType, companyId, plantId, partyId);
                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party");
                sheet.Range[row, 1, row, 2].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, partyMaster["PartyCode"] + " - " + partyMaster["PartyName"]);
                sheet.Range[row, 3, row, 5].Merge();
                sheet.Range[row, 3, row, 5].RowHeight = 30;

                reportUtility.SetMasterHeaderText(ref sheet, row, 7, "Account Group");
                sheet.Range[row, 7, row, 8].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 9, partyMaster["PartyAccountGroupName"].ToString());

                row++;
                if (!string.IsNullOrEmpty(partyPlantId))
                {
                    var partyPlant = _partyPlantRepository.Find(partyPlantId);
                    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party Plant");
                    sheet.Range[row, 1, row, 2].Merge();
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, partyPlant?.UserName);
                    sheet.Range[row, 3, row, 5].Merge();

                    colLast = colLast - 1;
                    colLast1 = colLast;
                }
                if (!string.IsNullOrEmpty(gSTINId))
                {
                    reportUtility.SetMasterHeaderText(ref sheet, row, 7, "Party GSTIN");
                    sheet.Range[row, 7, row, 8].Merge();
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, 9, gSTINId);
                    sheet.Range[row, 9, row, 11].Merge();
                }

                row++;
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                {
                    reportUtility.SetHeaderText(ref sheet, row, colLast + 1, "Transaction", ExcelHAlign.HAlignCenter);
                    sheet.Range[row, colLast + 1, row, colLast + 3].Merge();
                    colLast = colLast + 3;
                }
                reportUtility.SetHeaderText(ref sheet, row, colLast + 1, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet.Range[row, colLast + 1, row, colLast + 4].Merge();
                sheet.Range[row, colLast + 1, row, colLast + 4].BorderAround();
                // Set Row Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, col, "GL", 28); col++;
                if (string.IsNullOrEmpty(partyPlantId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Party Plant", 18); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 30); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 30); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Particulars", 45); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 50); col++;

                sheet.Range[row, col].WrapText = true;

                if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Currency", 15, ExcelHAlign.HAlignLeft); col++;

                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 37, ExcelHAlign.HAlignRight); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 37, ExcelHAlign.HAlignRight); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 37, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 37, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Balance", 42, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Dr/Cr", 8, ExcelHAlign.HAlignRight);
                sheet[row, 1, row, col].RowHeight = 70;
                //  sheet[row, 1, row, col].WrapText = true;

                row++;
                reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].RowHeight = 30;
                // Get party opening balance data.
                var obVal = GetPartyOpeningBalance(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, partyType.ToString());
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                        reportUtility.SetText(ref sheet, row, col - 1, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);

                    sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                    sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;

                }

                var ledgerData = GetPartyPaymentStatusPlantLedger3(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId, partyType.ToString());
                row++;
                int sumStrRow = 0;
                // Get bank transaction data.
                if (ledgerData.Rows.Count > 0)
                {
                    sumStrRow = row;
                    col = 1;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        //sumStrRow = row;
                        col = 1;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["GLGeneralInfoCode"] + " - " + ledgerData.Rows[i]["GLGeneralInfoName"]); col++;
                        if (string.IsNullOrEmpty(partyPlantId))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PartyPlantName"].ToString()); col++;
                        }
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PostingDate"].ToString(), ExcelHAlign.HAlignLeft); col++;

                        //reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft); col++;
                        ////reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocRefNo"].ToString(), 9, ExcelHAlign.HAlignLeft); col++;
                        ////reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocDate"].ToString(), 9, ExcelHAlign.HAlignLeft); col++;
                        //sheet[row, col].ColumnWidth = 50;
                        sheet.Range[row, col].WrapText = true;


                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Particular"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Narration"].ToString()); col++;

                        sheet.Range[row, col].WrapText = true;
                        if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["CurrencyCode"].ToString()); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString())); col++;
                        }
                        // Base currency checking
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                        // sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatDecimalTwo(); col++;

                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;
                        sheet.Range[row, col].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(col - 2) + row + "-" + reportUtility.GetColumnNameForXls(col - 1) + row + ")";

                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;
                        sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        row++;
                    }
                }

                reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);
                sheet[row, col].RowHeight = 30;
                //sheet[row, 7].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(7) + (row - 1) + ":" + clsStaticInfo.GetxlsCol(7) + row + ")";
                sheet.Range[row, col - 3].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col - 3) + (sumStrRow) + ":" + reportUtility.GetColumnNameForXls(col - 3) + (row - 1) + ")";
                sheet.Range[row, col - 3].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                // sheet.Range[row, col - 3].CellStyle.Font.Bold = true;
                sheet.Range[row, col - 3].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[row, col - 2].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col - 2) + (sumStrRow) + ":" + reportUtility.GetColumnNameForXls(col - 2) + (row - 1) + ")";
                sheet.Range[row, col - 2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //sheet.Range[row, col - 2].CellStyle.Font.Bold = true;
                sheet.Range[row, col - 2].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1 - 1) + row].Merge();
                sheet.Range[row, col - 1].Formula = "=" + reportUtility.GetColumnNameForXls(col - 1) + (row - 1);
                sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //sheet.Range[row, col - 1].CellStyle.Font.Bold = true;
                sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;

                var endCol = col;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, row, endCol].CellStyle.Font.Size = 27;


                //reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Portrait);
                reportUtility.PageSetup3(ref sheet, 6, ExcelPageOrientation.Portrait);



                sheet[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[StartRow + 3, 1, row, endCol].BorderInside(ExcelLineStyle.Thin);
                sheet.Range[StartRow + 3, 1, row, endCol].BorderAround(ExcelLineStyle.Thin);

                reportUtility.CompanyPlantHeader(ref sheet, col, "Party Ledger", companyId, plantId, plantName, "From " + fromDate + " To " + toDate + "");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(col) + 5].Merge();

                sheet[1, 1, 1, endCol].CellStyle.Font.Size = 45;
                sheet[1, 1, 1, endCol].RowHeight = 40;

                sheet[2, 1, 2, endCol].CellStyle.Font.Size = 40;
                sheet[2, 1, 2, endCol].RowHeight = 35;
                sheet[3, 1, 3, endCol].CellStyle.Font.Size = 30;
                sheet[3, 1, 3, endCol].RowHeight = 30;
                sheet[4, 1, 4, endCol].CellStyle.Font.Size = 30;
                sheet[4, 1, 4, endCol].RowHeight = 30;
                sheet[5, 1, 5, endCol].CellStyle.Font.Size = 30;
                sheet[5, 1, 5, endCol].RowHeight = 30;




                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }


        private DataTable GetPartyPaymentStatuPlantLedger3(string companyGroupId, string companyId, string plantId, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId, string partyType)
        {
            string tempPartyType = null;
            if (partyType == "Vendor" || partyType == "Customer" || partyType == "Director")
            {
                tempPartyType = partyType;
            }
            if (partyType == null || partyType == "null")
            {
                tempPartyType = "Vendor" + "','" + "Customer" + "','" + "Director";
            }
            var cmdText = @"--Modify query

                            DECLARE @companyId VARCHAR(10)='"+companyId+@"';
                            SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate
							, V.VoucherNo
							, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.DocRefNo
							, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate
							, V.Narration
							, SUM(ISNULL(VD.DrAmount,0)) AS DrAmount
							, SUM(ISNULL(VD.CrAmount,0)) AS CrAmount
                            , CC.CompanyCurrencyId
							, SUM(ISNULL(CC.CompanyCurrencyDrAmount, 0)) AS CompanyCurrencyDrAmount
							, SUM(ISNULL(CC.CompanyCurrencyCrAmount, 0)) AS CompanyCurrencyCrAmount

							, C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                            , VD.GLGeneralInfoId,GLGI.UserName AS GLGeneralInfoName
							, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName
							, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName

                             ,Particular =concat( STUFF((select distinct ','+xpA.UserName+ ' '+'('+ xp.UserName+')' from
														TRN.VoucherDetail XVD JOIN [HKP].[Party] AS XP ON XP.Id=XVD.PartyId
                                                        JOIN HKP.Activity AS XPA ON XPA.Id=XVD.ActivityId
													    where	XVD.VoucherId=V.Id AND XVD.PartyId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												,STUFF((select distinct ','+xp.AccountTitle from
														TRN.VoucherDetail XVD JOIN MST.BankMaster AS XP ON XP.Id=XVD.BankMasterId
													where	XVD.VoucherId=V.Id AND XVD.BankMasterId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												 , STUFF((select distinct ','+xp.UserName from
														TRN.VoucherDetail XVD JOIN MST.CashMaster AS XP ON XP.Id=XVD.CashMasterId
													where	XVD.VoucherId=V.Id AND XVD.CashMasterId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												 ,STUFF((select distinct ','+xp.EmployeeName from
														TRN.VoucherDetail XVD JOIN [dbo].[EmployeeInformation] AS XP ON XP.SystemId=XVD.EmployeeId
													where	XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                , STUFF((select distinct ','+xp.UserName from
														TRN.VoucherDetail XVD JOIN HKP.Activity AS XP ON XP.Id=XVD.ActivityId
													where	XVD.VoucherId=V.Id AND XVD.PartyId is null AND XVD.CashMasterId IS NULL AND XVD.BankMasterId IS NULL AND XVD.EmployeeId IS NULL
													 AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                                       
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId AND P.Id=VD.PartyId

                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                            ) AS CC ON CC.VoucherDetailId=VD.Id

                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='"+companyGroupId+"' AND V.CompanyId='"+companyId+@"' 
							AND V.PlantId='"+plantId+"' AND VD.PartyId='"+partyId+ "' AND VD.PartyType IN ('" + tempPartyType + "') AND V.PostingDate BETWEEN '" + fromDate+"' AND '"+toDate+ @"'
                            AND V.SourceType<>'OpeningBalance' 
                            AND V.Id NOT IN(SELECT VoucherId FROM TRN.InvoiceWriteOff  WHERE SourceType='VendorAdvanceWriteOff')
                            --AND V.SourceType NOT IN ('OpeningBalance','VendorAdvanceWriteOff')
                            
                                ";

            if (!string.IsNullOrEmpty(partyPlantId))
                cmdText += " AND VD.PartyPlantId='" + partyPlantId + "'";
            if (!string.IsNullOrEmpty(gSTINId))
                cmdText += " AND PP.GSTIN='" + gSTINId + "'";
            if (active)
                cmdText += @" GROUP BY V.PostingDate, V.VoucherNo, V.VoucherDate
                            , V.DocRefNo, V.DocDate, V.Narration
                            , CC.CompanyCurrencyId, C.Code, GLGI.AccountCode, PP.GSTIN,V.Id,VD.ActivityId
                            , VD.GLGeneralInfoId,GLGI.UserName, BGM.RefNo, BG.UserName,V.CurrencyId, A.UserName, P.Code , P.UserName , PP.UserName ORDER BY VD.GLGeneralInfoId, V.PostingDate, V.VoucherNo ASC";
            else
                cmdText += @"					 GROUP BY V.PostingDate, V.VoucherNo, V.VoucherDate
                            , V.DocRefNo, V.DocDate, V.Narration
                            , CC.CompanyCurrencyId, C.Code, GLGI.AccountCode, PP.GSTIN,V.Id,VD.ActivityId
                            , VD.GLGeneralInfoId,GLGI.UserName, BGM.RefNo, BG.UserName,V.CurrencyId, A.UserName, P.Code , P.UserName , PP.UserName ORDER BY V.PostingDate, V.VoucherNo ASC";

            return _sqlRepository.GetDataTable(cmdText);
        }


        public IWorkbook GetPartyPaymentStatusLedgerReportXls(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId)
        {
            try
            {
                var row = 6;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";
                var colLast = 6;
                var colLast1 = 6;
                var col = 1;
                var StartRow = 9;

                //sheet = null;

                // Get Party Master
                var partyMaster = _partyService.Find(partyType, companyId, plantId, partyId);
                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party");
                sheet.Range[row, 1, row, 2].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, partyMaster["PartyCode"] + " - " + partyMaster["PartyName"]);
                sheet.Range[row, 3, row, 5].Merge();
                // sheet.Range[row, 3, row, 5].RowHeight = 30;
                int colAccountGroup = 7;
                reportUtility.SetMasterHeaderText(ref sheet, row, 7, "Account Group");
                sheet.Range[row, colAccountGroup, row, colAccountGroup + 1].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colAccountGroup + 2, partyMaster["PartyAccountGroupName"].ToString());
                sheet.Range[row, colAccountGroup + 2, row, colAccountGroup + 4].Merge();

                row++;
                if (!string.IsNullOrEmpty(partyPlantId))
                {
                    var partyPlant = _partyPlantRepository.Find(partyPlantId);
                    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party Plant");
                    sheet.Range[row, 1, row, 2].Merge();
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, partyPlant?.UserName);
                    sheet.Range[row, 3, row, 5].Merge();

                    colLast = colLast - 1;
                    colLast1 = colLast;
                }
                if (!string.IsNullOrEmpty(gSTINId))
                {
                    reportUtility.SetMasterHeaderText(ref sheet, row, 7, "Party GSTIN");
                    sheet.Range[row, 7, row, 8].Merge();
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, 9, gSTINId);
                    sheet.Range[row, 9, row, 11].Merge();
                }

                row++;
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                {
                    reportUtility.SetHeaderText(ref sheet, row, colLast + 1, "Transaction", ExcelHAlign.HAlignCenter);
                    sheet.Range[row, colLast + 1, row, colLast + 3].Merge();
                    sheet.Range[row, colLast + 1, row, colLast + 3].BorderAround(ExcelLineStyle.Thin);

                    colLast = colLast + 3;
                }
                reportUtility.SetHeaderText(ref sheet, row, colLast + 1, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet.Range[row, colLast + 1, row, colLast + 4].Merge();
                sheet.Range[row, colLast + 1, row, colLast + 4].BorderAround();
                // Set Row Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, col, "GL", 15); col++;
                if (string.IsNullOrEmpty(partyPlantId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Party Plant", 10); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 15); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 15); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Particulars", 20); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 15); col++;

                sheet.Range[row, col].WrapText = true;

                if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Currency", 8, ExcelHAlign.HAlignLeft); col++;

                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Balance", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Dr/Cr", 4, ExcelHAlign.HAlignRight);
                //sheet[row, 1, row, col].RowHeight = 70;
                //  sheet[row, 1, row, col].WrapText = true;

                row++;
                reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].RowHeight = 30;
                // Get party opening balance data.
                var obVal = GetPartyOpeningBalance(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, partyType.ToString());
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                        reportUtility.SetText(ref sheet, row, col - 1, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);

                    sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                    sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;

                }

                var ledgerData = GetPartyPaymentStatuPlantLedger3(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId, partyType.ToString());
                row++;
                int sumStrRow = 0;
                // Get bank transaction data.
                if (ledgerData.Rows.Count > 0)
                {
                    sumStrRow = row;
                    col = 1;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        //sumStrRow = row;
                        col = 1;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["GLGeneralInfoCode"] + " - " + ledgerData.Rows[i]["GLGeneralInfoName"]); col++;
                        if (string.IsNullOrEmpty(partyPlantId))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PartyPlantName"].ToString()); col++;
                        }
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PostingDate"].ToString(), ExcelHAlign.HAlignLeft); col++;

                        //reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft); col++;
                        ////reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocRefNo"].ToString(), 9, ExcelHAlign.HAlignLeft); col++;
                        ////reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocDate"].ToString(), 9, ExcelHAlign.HAlignLeft); col++;
                        //sheet[row, col].ColumnWidth = 50;
                        sheet.Range[row, col].WrapText = true;


                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Particular"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Narration"].ToString()); col++;

                        sheet.Range[row, col].WrapText = true;
                        if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["CurrencyCode"].ToString()); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString())); col++;
                        }
                        // Base currency checking
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                        // sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatDecimalTwo(); col++;

                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;
                        sheet.Range[row, col].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(col - 2) + row + "-" + reportUtility.GetColumnNameForXls(col - 1) + row + ")";

                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;
                        sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        row++;
                    }
                }

                reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);
                //sheet[row, col].RowHeight = 30;
                //sheet[row, 7].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(7) + (row - 1) + ":" + clsStaticInfo.GetxlsCol(7) + row + ")";
                sheet.Range[row, col - 3].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col - 3) + (sumStrRow) + ":" + reportUtility.GetColumnNameForXls(col - 3) + (row - 1) + ")";
                sheet.Range[row, col - 3].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                // sheet.Range[row, col - 3].CellStyle.Font.Bold = true;
                sheet.Range[row, col - 3].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[row, col - 2].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col - 2) + (sumStrRow) + ":" + reportUtility.GetColumnNameForXls(col - 2) + (row - 1) + ")";
                sheet.Range[row, col - 2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //sheet.Range[row, col - 2].CellStyle.Font.Bold = true;
                sheet.Range[row, col - 2].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1 - 1) + row].Merge();
                sheet.Range[row, col - 1].Formula = "=" + reportUtility.GetColumnNameForXls(col - 1) + (row - 1);
                sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //sheet.Range[row, col - 1].CellStyle.Font.Bold = true;
                sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;

                var endCol = col;
                sheet.UsedRange.CellStyle.Font.Size = 8;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[StartRow, 1, row, endCol].CellStyle.Font.Size = 27;

                //reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Portrait);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Portrait);

                sheet[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[StartRow, 1, row, endCol].BorderInside(ExcelLineStyle.Thin);
                sheet.Range[StartRow, 1, row, endCol].BorderAround(ExcelLineStyle.Thin);

                reportUtility.CompanyPlantHeader(ref sheet, col, "Party Ledger", companyId, plantId, plantName, "From " + fromDate + " To " + toDate + "");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(col) + 5].Merge();

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable GetShortPartyPaymentStatuPlantLedger(string companyGroupId, string companyId, string plantId, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId, string partyType)
        {
            string tempPartyType = null;
            if (partyType == "Vendor" || partyType == "Customer" || partyType == "Director")
            {
                tempPartyType = partyType;
            }
            if (partyType == null || partyType == "null")
            {
                tempPartyType = "Vendor" + "','" + "Customer" + "','" + "Director";
            }
            var cmdText = @"--Modify query

                            DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                            SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate
							, V.VoucherNo
							, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.DocRefNo
							, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate
							, V.Narration
							, SUM(ISNULL(VD.DrAmount,0)) AS DrAmount
							, SUM(ISNULL(VD.CrAmount,0)) AS CrAmount
                            , CC.CompanyCurrencyId
							, SUM(ISNULL(CC.CompanyCurrencyDrAmount, 0)) AS CompanyCurrencyDrAmount
							, SUM(ISNULL(CC.CompanyCurrencyCrAmount, 0)) AS CompanyCurrencyCrAmount

							, C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                            , VD.GLGeneralInfoId,GLGI.UserName AS GLGeneralInfoName
							, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName
							, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName

                             ,Particular =concat( STUFF((select distinct ','+xpA.UserName+ ' '+'('+ xp.UserName+')' from
														TRN.VoucherDetail XVD JOIN [HKP].[Party] AS XP ON XP.Id=XVD.PartyId
                                                        JOIN HKP.Activity AS XPA ON XPA.Id=XVD.ActivityId
													    where	XVD.VoucherId=V.Id AND XVD.PartyId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												,STUFF((select distinct ','+xp.AccountTitle from
														TRN.VoucherDetail XVD JOIN MST.BankMaster AS XP ON XP.Id=XVD.BankMasterId
													where	XVD.VoucherId=V.Id AND XVD.BankMasterId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												 , STUFF((select distinct ','+xp.UserName from
														TRN.VoucherDetail XVD JOIN MST.CashMaster AS XP ON XP.Id=XVD.CashMasterId
													where	XVD.VoucherId=V.Id AND XVD.CashMasterId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												 ,STUFF((select distinct ','+xp.EmployeeName from
														TRN.VoucherDetail XVD JOIN [dbo].[EmployeeInformation] AS XP ON XP.SystemId=XVD.EmployeeId
													where	XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                , STUFF((select distinct ','+xp.UserName from
														TRN.VoucherDetail XVD JOIN HKP.Activity AS XP ON XP.Id=XVD.ActivityId
													where	XVD.VoucherId=V.Id AND XVD.PartyId is null AND XVD.CashMasterId IS NULL AND XVD.BankMasterId IS NULL AND XVD.EmployeeId IS NULL
													 AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                                       
                            FROM [TRN].[VoucherDetail] AS VD
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                            LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId AND P.Id=VD.PartyId

                            LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            FROM [TRN].[VoucherDetailCurrency] AS VDC
	                            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                            ) AS CC ON CC.VoucherDetailId=VD.Id

                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + @"' 
							AND V.PlantId='" + plantId + "' AND VD.PartyId='" + partyId + "' AND VD.PartyType IN ('" + tempPartyType + "') AND V.PostingDate BETWEEN '" + fromDate + "' AND '" + toDate + @"'
                            AND V.SourceType<>'OpeningBalance' 
                            AND V.Id NOT IN(SELECT VoucherId FROM TRN.InvoiceWriteOff  WHERE SourceType IN('VendorAdvanceWriteOff','CustomerAdvanceWriteOff','CreditNoteSetOff'))
                            --AND V.SourceType NOT IN ('OpeningBalance','VendorAdvanceWriteOff')
                            
                                ";

            if (!string.IsNullOrEmpty(partyPlantId))
                cmdText += " AND VD.PartyPlantId='" + partyPlantId + "'";
            if (!string.IsNullOrEmpty(gSTINId))
                cmdText += " AND PP.GSTIN='" + gSTINId + "'";
            if (active)
                cmdText += @" GROUP BY V.PostingDate, V.VoucherNo, V.VoucherDate
                            , V.DocRefNo, V.DocDate, V.Narration
                            , CC.CompanyCurrencyId, C.Code, GLGI.AccountCode, PP.GSTIN,V.Id,VD.ActivityId
                            , VD.GLGeneralInfoId,GLGI.UserName, BGM.RefNo, BG.UserName,V.CurrencyId, A.UserName, P.Code , P.UserName , PP.UserName ORDER BY VD.GLGeneralInfoId, V.PostingDate, V.VoucherNo ASC";
            else
                cmdText += @"					 GROUP BY V.PostingDate, V.VoucherNo, V.VoucherDate
                            , V.DocRefNo, V.DocDate, V.Narration
                            , CC.CompanyCurrencyId, C.Code, GLGI.AccountCode, PP.GSTIN,V.Id,VD.ActivityId
                            , VD.GLGeneralInfoId,GLGI.UserName, BGM.RefNo, BG.UserName,V.CurrencyId, A.UserName, P.Code , P.UserName , PP.UserName ORDER BY V.PostingDate, V.VoucherNo ASC";

            return _sqlRepository.GetDataTable(cmdText);
        }

        public IWorkbook GetShortPartyPaymentStatusLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, PartyType partyType, string partyId, string partyPlantId, string fromDate, string toDate, string glId, bool active, string gSTINId, string partyName)
        {
            try
            {
                var row = 6;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";
                var colLast = 6;
                var colLast1 = 6;
                var col = 1;
                var StartRow = 9;

                //sheet = null;

                // Get Party Master
                var partyMaster = _partyService.Find(partyType, companyId, plantId, partyId);
                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party");
                sheet.Range[row, 1, row, 2].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, partyMaster["PartyCode"] + " - " + partyMaster["PartyName"]);
                sheet.Range[row, 3, row, 5].Merge();
                // sheet.Range[row, 3, row, 5].RowHeight = 30;
                int colAccountGroup = 7;
                reportUtility.SetMasterHeaderText(ref sheet, row, 7, "Account Group");
                sheet.Range[row, colAccountGroup, row, colAccountGroup + 1].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colAccountGroup + 2, partyMaster["PartyAccountGroupName"].ToString());
                sheet.Range[row, colAccountGroup + 2, row, colAccountGroup + 4].Merge();

                row++;
                if (!string.IsNullOrEmpty(partyPlantId))
                {
                    var partyPlant = _partyPlantRepository.Find(partyPlantId);
                    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party Plant");
                    sheet.Range[row, 1, row, 2].Merge();
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, partyPlant?.UserName);
                    sheet.Range[row, 3, row, 5].Merge();

                    colLast = colLast - 1;
                    colLast1 = colLast;
                }
                if (!string.IsNullOrEmpty(gSTINId))
                {
                    reportUtility.SetMasterHeaderText(ref sheet, row, 7, "Party GSTIN");
                    sheet.Range[row, 7, row, 8].Merge();
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, 9, gSTINId);
                    sheet.Range[row, 9, row, 11].Merge();
                }

                row++;
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                {
                    reportUtility.SetHeaderText(ref sheet, row, colLast + 1, "Transaction", ExcelHAlign.HAlignCenter);
                    sheet.Range[row, colLast + 1, row, colLast + 2].Merge();
                    sheet.Range[row, colLast + 1, row, colLast + 2].BorderAround(ExcelLineStyle.Thin);

                    colLast = colLast + 3;
                }
                reportUtility.SetHeaderText(ref sheet, row, colLast + 1, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet.Range[row, colLast + 1, row, colLast + 3].Merge();
                sheet.Range[row, colLast + 1, row, colLast + 3].BorderAround();
                // Set Row Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, col, "GL", 15); col++;
                if (string.IsNullOrEmpty(partyPlantId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Party Plant", 10); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 15); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 15); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 15); col++;

                sheet.Range[row, col].WrapText = true;

                if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Currency", 8, ExcelHAlign.HAlignLeft); col++;

                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); col++;
                }
                reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Balance", 12, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Dr/Cr", 4, ExcelHAlign.HAlignRight);
                //sheet[row, 1, row, col].RowHeight = 70;
                //  sheet[row, 1, row, col].WrapText = true;

                row++;
                reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].RowHeight = 30;
                // Get party opening balance data.
                var obVal = GetPartyOpeningBalance(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, partyType.ToString());
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    if (!string.IsNullOrEmpty(companyCurrencyId))
                        reportUtility.SetText(ref sheet, row, col - 1, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);

                    sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                    sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;

                }

                var ledgerData = GetShortPartyPaymentStatuPlantLedger(companyGroupId, companyId, plantId, partyId, partyPlantId, fromDate, toDate, glId, active, gSTINId, partyType.ToString());
                row++;
                int sumStrRow = 0;
                // Get bank transaction data.
                if (ledgerData.Rows.Count > 0)
                {
                    sumStrRow = row;
                    col = 1;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        //sumStrRow = row;
                        col = 1;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["GLGeneralInfoCode"] + " - " + ledgerData.Rows[i]["GLGeneralInfoName"]); col++;
                        if (string.IsNullOrEmpty(partyPlantId))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PartyPlantName"].ToString()); col++;
                        }
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PostingDate"].ToString(), ExcelHAlign.HAlignLeft); col++;

                        //reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft); col++;
                        ////reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocRefNo"].ToString(), 9, ExcelHAlign.HAlignLeft); col++;
                        ////reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocDate"].ToString(), 9, ExcelHAlign.HAlignLeft); col++;
                        //sheet[row, col].ColumnWidth = 50;
                        sheet.Range[row, col].WrapText = true;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Narration"].ToString()); col++;

                        sheet.Range[row, col].WrapText = true;
                        if (companyCurrencyId != partyMaster["CurrencyId"].ToString())
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["CurrencyCode"].ToString()); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString())); col++;
                        }
                        // Base currency checking
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                        // sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatDecimalTwo(); col++;

                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;
                        sheet.Range[row, col].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(col - 2) + row + "-" + reportUtility.GetColumnNameForXls(col - 1) + row + ")";

                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo(); col++;
                        sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        row++;
                    }
                }

                reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);
                //sheet[row, col].RowHeight = 30;
                //sheet[row, 7].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(7) + (row - 1) + ":" + clsStaticInfo.GetxlsCol(7) + row + ")";
                sheet.Range[row, col - 3].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col - 3) + (sumStrRow) + ":" + reportUtility.GetColumnNameForXls(col - 3) + (row - 1) + ")";
                sheet.Range[row, col - 3].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                // sheet.Range[row, col - 3].CellStyle.Font.Bold = true;
                sheet.Range[row, col - 3].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[row, col - 2].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(col - 2) + (sumStrRow) + ":" + reportUtility.GetColumnNameForXls(col - 2) + (row - 1) + ")";
                sheet.Range[row, col - 2].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //sheet.Range[row, col - 2].CellStyle.Font.Bold = true;
                sheet.Range[row, col - 2].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1 - 1) + row].Merge();
                sheet.Range[row, col - 1].Formula = "=" + reportUtility.GetColumnNameForXls(col - 1) + (row - 1);
                sheet.Range[row, col - 1].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //sheet.Range[row, col - 1].CellStyle.Font.Bold = true;
                sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignRight;

                var endCol = col;
                sheet.UsedRange.CellStyle.Font.Size = 8;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[StartRow, 1, row, endCol].CellStyle.Font.Size = 27;

                //reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Portrait);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Portrait);

                sheet[row, col].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[StartRow, 1, row, endCol].BorderInside(ExcelLineStyle.Thin);
                sheet.Range[StartRow, 1, row, endCol].BorderAround(ExcelLineStyle.Thin);

                reportUtility.CompanyPlantHeader(ref sheet, col, "Party Ledger", companyId, plantId, plantName, "From " + fromDate + " To " + toDate + "");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(col) + 5].Merge();

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion party payment staus report
    }
}