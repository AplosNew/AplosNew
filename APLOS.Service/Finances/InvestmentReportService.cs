using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Currencies;
using Library.Service.Helpers;
using Library.Service.Organizations;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.Finances
{
    public class InvestmentReportService : IInvestmentReportService
    {
        private readonly ICompanyService _companyService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IPlantService _plantService;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;

        public InvestmentReportService(
             ICompanyService companyService
            , ISqlRepository sqlRepository
            , IPlantService plantService
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            )
        {
            _sqlRepository = sqlRepository;
            _companyService = companyService;
            _plantService = plantService;
            _companyParallelCurrencyService = companyParallelCurrencyService;
        }

       
        public IWorkbook GetInvestmentReport(out string reportFileName, string companyGroupId, string companyId, String PlantName, string plantId, string voucherId, string sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetInvestmentReportHeader(companyGroupId, companyId,plantId, voucherId, SourceType.Investment);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetInvestmentData(companyGroupId, companyId, voucherId, sourceType);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colParticulars = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 4, header["VoucherDate"].ToString());

            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "DocDate");
            reportUtility.SetText(ref sheet, row, 4, header["DocDate"].ToString());

            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Status");
            reportUtility.SetText(ref sheet, row, 2, header["Status"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 4, header["DocRefNo"].ToString());

            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();

            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();

            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 5, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 5, row, 6].Merge();
                sheet.Range[row, 5, row, 6].BorderAround(ExcelLineStyle.Thin);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 5, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 5, row, 6].Merge();
                sheet.Range[row, 5, row, 6].BorderAround(ExcelLineStyle.Thin);

                reportUtility.SetHeaderText(ref sheet, row, 7, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 7, row, 8].Merge();
                sheet.Range[row, 7, row, 8].BorderAround(ExcelLineStyle.Thin);
            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++; xlsCol++;
            sheet.Range[row, colGl, row, xlsCol].BorderAround(ExcelLineStyle.Hair);
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge(); xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars", 12); colParticulars = xlsCol; xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                var xRow = row;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

                    reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString()); if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdCradit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                    row++;

                    glName = string.Empty;

                }
                var lastRow = row - 1;

                reportUtility.SetText(ref sheet, row, 4, "Total: ", true);


                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (lastRow) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (lastRow) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
                    row++;
                }

                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                sheet.UsedRange.AutofitColumns();
                sheet[1, 2].ColumnWidth = 20;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId,plantId, PlantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId,plantId, PlantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }

        private DataTable GetInvestmentData(string companyGroupId, string companyId, string voucherId, string sourceType)
        {
            var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, v.IsPark, Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate,
                    [Park/Post]=CASE WHEN v.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') DocDate, V.DocRefNo,
                    Replace(CONVERT(VARCHAR(11), v.VoucherDate, 106), ' ', '-') VoucherDate, V.VoucherNo, v.Narration, V.CurrencyId, CU1.Code AS TrnCurrency,
                    V.AddedBy AS PreparedBy, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount+VD.CrAmount AS Value,
                    VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, VD.DrAmount,VD.CrAmount,
					[DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                    Replace(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') InvoiceDate, VD.DocRefNo AS InvoiceNo, P.UserName AS Customer, VD.RefCode AS Ref,
                    CO.UserName AS CompanyName,AM.Address1 AS AddressLine, ENT.UserName AS Entity, BUD.UserName AS Budget, ACT.UserName AS Activity, CST.UserName AS [Cost Center]
                    , BFY.FiscalYearName AS [Budget Fiscal Year], BFYP.PeriodName AS [Budget Fiscal Year Period], BFYP.PeriodNo AS [Budget Period No]
					,[ParticularName]=CASE
								WHEN VI.TransactionType='InvestmentGiven'  THEN FT.AssetUserName
								WHEN VI.TransactionType='InvestmentTaken' THEN FT.LiabilityUserName
								WHEN P.UserName<>'' THEN P.UserName 
								ELSE ''	END
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT JOIN TRN.FinancingDetail AS VID ON VID.Id=VD.FinancingDetailId
                    LEFT JOIN TRN.Financing AS VI ON VI.Id=VID.FinancingId
					LEFT JOIN HKP.FinancingType AS FT ON FT.Id=VI.FinancingTypeId
                    Left join (select PartyId,VoucherId from TRN.VoucherDetail where ISNULL(PartyId,'')<>''  ) AS PD ON PD.VoucherId=V.Id
                    LEFT JOIN HKP.Party AS P ON P.Id=PD.PartyId
                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN SCS.Currency AS CU1 ON CU1.Id=V.CurrencyId
                    LEFT JOIN ORG.Company AS CO ON CO.Id=V.CompanyId
                    LEFT JOIN MST.AddressMaster AS AM ON AM.Id=CO.AddressMasterId
                    LEFT JOIN SCS.FiscalYear AS FY ON FY.Id=V.FiscalYearId
                    LEFT JOIN SCS.FiscalYearPeriod AS FYP ON FYP.Id=V.FiscalYearPeriodId
                    LEFT JOIN [MST].[BudgetMaster] AS BUDM ON BUDM.Id = VD.BudgetMasterId
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUDM.BudgetId
                    LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                    LEFT JOIN [ORG].[CostCenter] AS CST ON CST.Id = VD.CostCenterId
                    LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
                    LEFT JOIN [SCS].[FiscalYear] AS BFY ON BFY.Id=VD.FiscalYearId
                    LEFT JOIN [SCS].[FiscalYearPeriod] AS BFYP ON BFYP.Id=VD.FiscalYearPeriodId
                    WHERE V.Archive=0 AND V.SourceType='" + sourceType + "' AND V.Id = '" + voucherId + "' AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + @"'
                    ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(sql);
        }

        private Dictionary<string, object> GetInvestmentReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.AddedBy, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , P.UserName AS Vendor, PP.UserName AS VendorPlant, BJ.CurrencyId, C.Code AS CurrencyCode
                            FROM [TRN].[Financing] AS BJ
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=BJ.VoucherId
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [HKP].[Party] AS P ON P.Id=BJ.PartyId
							LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=BJ.PartyPlantId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                            WHERE BJ.Archive=0 AND BJ.CompanyGroupId='" + companyGroupId + "' AND BJ.CompanyId='" + companyId + "' AND BJ.PlantId='" + plantId + "' AND BJ.VoucherId='" + voucherId + "' AND BJ.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }

        public IWorkbook GetInvestmentWriteOffReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetLoanWriteOffReportHeader(companyGroupId, companyId, plantId, voucherId, SourceType.InvestmentSetOff);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetLoanWriteOffData(companyGroupId, companyId, voucherId, sourceType);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colParticulars = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 4, header["VoucherDate"].ToString());

            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "DocDate");
            reportUtility.SetText(ref sheet, row, 4, header["DocDate"].ToString());

            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Status");
            reportUtility.SetText(ref sheet, row, 2, header["Status"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Doc Ref");
            //reportUtility.SetText(ref sheet, row, 4, header["DocRefNo"].ToString(), false, true);
            reportUtility.SetText(ref sheet, row, 4, header["DocRefNo"].ToString());

            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();

            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            //sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet[row, 2, row + 1, 2].Merge();
            sheet.UsedRange.WrapText = true;

            row++;
            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();
                sheet.Range[row, 4, row, 5].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, 4, row, 5].BorderAround(ExcelLineStyle.Hair);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
                sheet.Range[row, 4, row, 5].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, 4, row, 5].BorderAround(ExcelLineStyle.Hair);
            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol;
            xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
            xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars", 12); colParticulars = xlsCol; xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }
            sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                var xRow = row;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();

                    reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString()); if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdCradit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                    row++;

                    glName = string.Empty;

                }
                var lastRow = row - 1;

                reportUtility.SetText(ref sheet, row, 3, "Total: ", true);


                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (lastRow) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (lastRow) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
                    row++;
                }

                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                sheet.UsedRange.AutofitColumns();
                sheet[1, 2].ColumnWidth = 40;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet.Range[row, 1].ColumnWidth = 22;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 2, header["PostedBy"].ToString());
                sheet.Range[row, 2].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 2, "Checked By", true);

                sheet.Range[row, 4].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 4, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }
        private Dictionary<string, object> GetLoanWriteOffReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , P.UserName AS Vendor, PP.UserName AS VendorPlant, V.CurrencyId, C.Code AS CurrencyCode
                            ,UA.FullName AddedBy,U.FullName PostedBy
                            FROM  [TRN].[Voucher] AS V
                                    LEFT JOIN [TRN].[FinancingWriteOff] AS BJ ON V.Id=BJ.VoucherId
									LEFT JOIN(SELECT DISTINCT PartyId,PartyPlantId,VoucherId FROM TRN.VoucherDetail WHERE VoucherId='" + voucherId + @"' AND PartyId<>'') VD ON VD.VoucherId=V.Id 
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
							LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                            left join SEC.[User] UA on UA.UserId=V.AddedBy
							left join SEC.[User] U on U.UserId=V.PostedBy
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' AND V.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }

        private DataTable GetLoanWriteOffData(string companyGroupId, string companyId, string voucherId, string sourceType)
        {
            var _sql = @"SELECT V.Id, GL.Id AS AccountCodeId,VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo
                        , V.IsPark, Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate, [Park/Post]=CASE WHEN v.IsPark=1 THEN 'Parked' ELSE 'Posted' END
		                , Replace(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') DocDate, V.DocRefNo, Replace(CONVERT(VARCHAR(11), v.VoucherDate, 106), ' ', '-') VoucherDate
		                , V.VoucherNo, v.Narration, V.CurrencyId,CU1.Code AS TrnCurrency, v.AddedBy AS PreparedBy, VDC.ParallelCurrencyId,CU.Code AS CurrencyCode
		                , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount+VD.CrAmount AS Value
						, VD.DrAmount, VD.CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
                        , [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, GL.AccountCode
		                , Replace(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') InvoiceDate, VD.DocRefNo AS InvoiceNo, P.UserName AS Customer
		                , VD.RefCode AS Ref,VD.Narration AS DetailNarration, CO.UserName AS CompanyName,AM.Address1 AS AddressLine
						, ENT.UserName AS Entity, BUD.UserName AS Budget
						 ,[Activity]= CASE 
							WHEN BM.AccountTitle<>'' THEN ACT.UserName+' - '+ BM.AccountTitle
							WHEN CM.UserName<>'' THEN ACT.UserName+' - '+ CM.UserName 
							WHEN ACT.UserName<>'' THEN ACT.UserName
							ELSE ''	END
						 , CST.UserName AS [Cost Center]
                        , BFY.FiscalYearName AS [Budget Fiscal Year], BFYP.PeriodName AS [Budget Fiscal Year Period], BFYP.PeriodNo AS [Budget Period No]
						,[ParticularName]=CASE
								WHEN VI.TransactionType='LoanGiven'  THEN FT.AssetUserName
								WHEN VI.TransactionType='LoanTaken' THEN FT.LiabilityUserName
								WHEN P.UserName<>'' THEN P.UserName 
								ELSE ''	END
	                    FROM TRN.VoucherDetailCurrency AS VDC
		                INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                        LEFT JOIN TRN.FinancingDetailWriteOff AS VID ON VID.Id=VD.FinancingDetailWriteOffId
                        LEFT JOIN TRN.FinancingWriteOff AS VI ON VI.Id=VID.FinancingWriteOffId
                        LEFT JOIN HKP.FinancingType AS FT ON FT.Id=VI.FinancingTypeId
						LEFT JOIN (select PartyId,VoucherId from TRN.VoucherDetail where ISNULL(PartyId,'')<>''  ) AS PD ON PD.VoucherId=V.Id
		                LEFT JOIN HKP.Party AS P ON P.Id=PD.PartyId
		                LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
		                LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
		                LEFT JOIN SCS.Currency AS CU1 ON CU1.Id=V.CurrencyId
		                LEFT JOIN ORG.Company AS CO ON CO.Id=V.CompanyId
		                LEFT JOIN MST.AddressMaster AS AM ON AM.Id=CO.AddressMasterId
                        LEFT JOIN SCS.FiscalYear AS FY ON FY.Id=V.FiscalYearId
						LEFT JOIN SCS.FiscalYearPeriod AS FYP ON FYP.Id=V.FiscalYearPeriodId
						LEFT JOIN [MST].[BudgetMaster] AS BUM ON BUM.Id = VD.BudgetMasterId
						LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUM.BudgetId
		                LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
		                LEFT JOIN [ORG].[CostCenter] AS CST ON CST.Id = VD.CostCenterId
		                LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
		                LEFT JOIN [SCS].[FiscalYear] AS BFY ON BFY.Id=VD.FiscalYearId
		                LEFT JOIN [SCS].[FiscalYearPeriod] AS BFYP ON BFYP.Id=VD.FiscalYearPeriodId
						LEFT JOIN [MST].BankMaster AS BM ON BM.Id=VD.BankMasterId
						LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
                        where V.Archive=0 AND V.SourceType='" + sourceType + "' AND V.Id = '" + voucherId + "' AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + @"'
                        ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(_sql);
        }
    }
}