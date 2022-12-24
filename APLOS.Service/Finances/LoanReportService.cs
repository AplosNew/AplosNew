using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Finances;
using Library.Model.Parties;
using Library.Service.Currencies;
using Library.Service.Helpers;
using Library.Service.Organizations;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.Finances
{
    public class LoanReportService : ILoanReportService
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly IPlantService _plantService;
        private readonly ILoanService _loanService;
        private readonly IRepositoryAsync<Financing> _loanRepository;

        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;

        public LoanReportService(ISqlRepository sqlRepository
            , IPlantService plantService
            , ILoanService loanService
            , IRepositoryAsync<Financing> loanRepository
            , ICompanyParallelCurrencyService companyParallelCurrencyService)
        {
            _sqlRepository = sqlRepository;
            _plantService = plantService;
            _loanService = loanService;
            _loanRepository = loanRepository;
            _companyParallelCurrencyService = companyParallelCurrencyService;
        }

        public IWorkbook GetLoanReport(out string reportFileName, string companyGroupId, string companyId, string plantName, string plantId, string voucherId, string sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetLoanReportHeader(companyGroupId, companyId, plantId, voucherId, sourceType);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetLoanData(companyGroupId, companyId, voucherId, sourceType);

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
                reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;
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
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 2, header["PostedBy"].ToString());
                sheet.Range[row, 2].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 2, "Checked By", true);

                sheet.Range[row, 4].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 4, "Authorized By", true);

                if (sourceType == "AutoLoan")
                {
                    reportUtility.CompanyPlantHeader(ref sheet, colLast, "Auto Loan", companyId, plantId, plantName, null);
                    reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
                }
                else
                {
                    reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
                    reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
                }

            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId,plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }

        private DataTable GetLoanData(string companyGroupId, string companyId, string voucherId, string sourceType)
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
							WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
							WHEN CM.UserName<>'' THEN CM.UserName 
							WHEN ACT.UserName<>'' THEN ACT.UserName 
							ELSE ''	END
						 , CST.UserName AS [Cost Center]
                        , BFY.FiscalYearName AS [Budget Fiscal Year], BFYP.PeriodName AS [Budget Fiscal Year Period], BFYP.PeriodNo AS [Budget Period No]
						,[ParticularName]=CASE
								WHEN VI.TransactionType='LoanGiven'  THEN FT.AssetUserName
								WHEN VI.TransactionType='LoanTaken' THEN FT.LiabilityUserName
								WHEN P.UserName<>'' AND isnull(IV.DocRefNo,'')<>'' THEN P.UserName +' ('+ isnull(IV.DocRefNo,'')+')'
								WHEN P.UserName<>'' THEN P.UserName
                                WHEN FS.DocRefNo<>'' THEN FS.DocRefNo
								WHEN ACT.UserName<>'' THEN ACT.UserName
								ELSE ''	END
	                    FROM TRN.VoucherDetailCurrency AS VDC
		                 JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                 JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId

                        LEFT JOIN TRN.InvoiceWriteOffDetail AS IVWD ON IVWD.Id=VD.InvoiceWriteOffDetailId
						LEFT JOIN TRN.Invoice AS IV ON IV.Id=IVWD.InvoiceId
                        LEFT JOIN TRN.FinancingDetail AS VID ON VID.Id=VD.FinancingDetailId
                        LEFT JOIN TRN.Financing AS VI ON VI.Id=VID.FinancingId
                        LEFT JOIN HKP.FinancingType AS FT ON FT.Id=VI.FinancingTypeId
						LEFT JOIN (select Id,PartyId,VoucherId from TRN.VoucherDetail where ISNULL(PartyId,'')<>''  ) AS PD ON PD.VoucherId=V.Id AND PD.Id=VDC.VoucherDetailId
		                LEFT JOIN HKP.Party AS P ON P.Id=PD.PartyId
		                LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
						 LEFT JOIN TRN.FinancingSubsequentTransaction F ON F.VoucherDetailId=VD.Id
						 LEFT JOIN TRN.Financing FS ON FS.Id=F.SetOffFinancingId
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
        private Dictionary<string, object> GetLoanReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, string sourceType)
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

        public IWorkbook GetLoanWriteOffReport(out string reportFileName, string companyGroupId, string companyId, string plantId,string plantName, string voucherId, string sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetLoanWriteOffReportHeader(companyGroupId, companyId, plantId, voucherId, SourceType.LoanPayment);

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
            sheet[row , 2, row+1, 2].Merge();
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

                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantId,plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantId,plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
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
        private DataTable GetLoanOpeningBalanceLedger(string companyGroupId, string companyId, string plantId, string partyId, string partyPlantId, string fiscalYearId)
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
        private List<Dictionary<string, object>> GetLoanOpeningBalance(string companyGroupId, string companyId, string plantId, string partyId, string partyPlantId, string fromDate)
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


        //All Register Report Data
        private DataTable GetLoanLedger(string companyGroupId, string companyId, string plantId, string voucherId, string financingId)
        {
            var cmdText = @"
                  
                    DECLARE @companyId VARCHAR(10)='"+companyId+ @"';
                    SELECT x.PostingDate,x.VoucherNo,x.VoucherDate,x.DocRefNo,x.DocDate,x.Narration,x.DrAmount,x.CrAmount,x.InterestDrAmount,x.InterestCrAmount,x.CompanyCurrencyId,x.CompanyCurrencyDrAmount,x.CompanyCurrencyCrAmount,x.InterestCompanyCurrencyDrAmount,x.InterestCompanyCurrencyCrAmount,x.CurrencyCode,x.GLGeneralInfoCode,x.GLGeneralInfoName,x.GLGeneralInfoId,x.GSTIN
                    ,x.RefNo,x.BudgetName,x.CurrencyId,x.ActivityName,x.PartyCode,x.PartyName,x.PartyPlantName,x.FinancingId
                    FROM(
                    SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                    , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration, ISNULL(VD.DrAmount,0) AS DrAmount, ISNULL(VD.CrAmount,0) AS CrAmount, 0 InterestDrAmount,0 InterestCrAmount
                    , CC.CompanyCurrencyId, ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) AS CompanyCurrencyCrAmount,0 InterestCompanyCurrencyDrAmount, 0 InterestCompanyCurrencyCrAmount
                    , C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                    , VD.GLGeneralInfoId,GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName
                    ,F.Id FinancingId,v.PostingDate PostingDateNew,v.AddedDate
                    FROM
                    [TRN].[Financing] AS F
                    LEFT JOIN [TRN].[FinancingDetail] AS FD ON FD.FinancingId=F.Id
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON FD.Id=VD.FinancingDetailId
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                    --LEFT JOIN [TRN].[FinancingWriteOff] AS FW ON FW.FinancingId=F.Id
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
                    where f.Id='" + financingId+ @"' AND VD.FinancingDetailId<>'' AND F.IsPark=0

                    
                    UNION

                    SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                    , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration, 0 DrAmount, 0 CrAmount, ISNULL(VD.DrAmount,0) AS InterestDrAmount, ISNULL(VD.CrAmount,0) AS InterestCrAmount
                    , CC.CompanyCurrencyId,0 CompanyCurrencyDrAmount, 0 CompanyCurrencyCrAmount,ISNULL(CC.CompanyCurrencyDrAmount, 0) AS InterestCompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) AS InterestCompanyCurrencyCrAmount
                    , C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                    , VD.GLGeneralInfoId,GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName
                    ,LIP.FinancingId,v.PostingDate PostingDateNew,v.AddedDate
                    FROM
                    [TRN].FinancingSubsequentTransaction AS LIP
                    LEFT JOIN TRN.Financing F ON F.Id=LIP.FinancingId
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=LIP.VoucherId
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON LIP.VoucherDetailId=VD.Id 
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
                    WHERE LIP.FinancingId='" + financingId+ @"' and lip.IsPark=0  AND LIP.TransactionType in ('InterestPayable','OtherExpensesPayable','AccrulInterestPayment','InterestPayableReverse','ChargesPayableReverse')
                    UNION

					 SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                    , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration, VD.DrAmount, VD.CrAmount, 0 InterestDrAmount, 0 InterestCrAmount
                    , CC.CompanyCurrencyId,ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) CompanyCurrencyCrAmount,0 InterestCompanyCurrencyDrAmount,  0 InterestCompanyCurrencyCrAmount
                    , C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                    , VD.GLGeneralInfoId,GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName
                    ,LIP.FinancingId,v.PostingDate PostingDateNew,v.AddedDate
                    FROM
                    [TRN].FinancingSubsequentTransaction AS LIP
                    LEFT JOIN TRN.Financing F ON F.Id=LIP.FinancingId
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=LIP.VoucherId
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON LIP.VoucherDetailId=VD.Id 
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
                    WHERE LIP.FinancingId='" + financingId+ @"' and lip.IsPark=0  AND LIP.TransactionType in ('AdditionalLoanPayable')
                    UNION
                    SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                    , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration, VD.DrAmount, VD.CrAmount, 0 InterestDrAmount, 0 InterestCrAmount
                    , CC.CompanyCurrencyId,ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) CompanyCurrencyCrAmount,0 InterestCompanyCurrencyDrAmount, 0 InterestCompanyCurrencyCrAmount
                    , C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                    , VD.GLGeneralInfoId,GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName
                    ,LIP.FinancingId,v.PostingDate PostingDateNew,v.AddedDate
                    FROM
                    [TRN].FinancingSubsequentTransaction AS LIP
                    LEFT JOIN TRN.Financing F ON F.Id=LIP.FinancingId
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=LIP.VoucherId
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON LIP.VoucherDetailId=VD.Id
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
                    WHERE LIP.FinancingId='" + financingId + @"' --and lip.IsPark=0 
                    AND LIP.TransactionType in ('LoanPayment') and LIP.SetOffFinancingId IS NULL
                    UNION
                    SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                    , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration, VD.DrAmount, VD.CrAmount, 0 InterestDrAmount, 0 InterestCrAmount
                    , CC.CompanyCurrencyId,ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) CompanyCurrencyCrAmount,0 InterestCompanyCurrencyDrAmount, 0 InterestCompanyCurrencyCrAmount
                    , C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                    , VD.GLGeneralInfoId,GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName
                    ,LIP.FinancingId,v.PostingDate PostingDateNew,v.AddedDate
                    FROM
                    [TRN].FinancingSubsequentTransaction AS LIP
                    LEFT JOIN TRN.Financing F ON F.Id=LIP.FinancingId
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=LIP.VoucherId
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON LIP.VoucherDetailId=VD.Id
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
                    WHERE LIP.SetOffFinancingId='" + financingId + @"' --and lip.IsPark=0 
					AND LIP.TransactionType in ('LoanPayment') and LIP.SetOffFinancingId<>''
                    --SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                    --, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration, ISNULL(VD.DrAmount,0) AS DrAmount, ISNULL(VD.CrAmount,0) AS CrAmount , 0 InterestDrAmount,0 InterestCrAmount
                    --, CC.CompanyCurrencyId, ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) AS CompanyCurrencyCrAmount,0 InterestCompanyCurrencyDrAmount, 0 InterestCompanyCurrencyCrAmount
                    --, C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                    --, VD.GLGeneralInfoId,GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName
                    --,FW.FinancingId,v.PostingDate PostingDateNew
                    --FROM
                    --[TRN].[FinancingWriteOff] AS FW
                    --LEFT JOIN TRN.FinancingDetailWriteOff FWD ON FWD.FinancingWriteOffId=FW.Id
                    --LEFT JOIN [TRN].[VoucherDetail] AS VD ON FWD.Id=VD.FinancingDetailWriteOffId
                    --LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                    --LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                    --LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                    --LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=VD.BudgetMasterId
                    --LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                    --LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                    --LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                    --LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId AND P.Id=VD.PartyId
                    --LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
                    --FROM [TRN].[VoucherDetailCurrency] AS VDC
                    --JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                    --WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                    --) AS CC ON CC.VoucherDetailId=VD.Id
                    --WHERE FW.FinancingId='" + financingId+ @"' AND VD.FinancingDetailWriteOffId<>'' AND FW.IsPark=0

                    ) x
                     ORDER BY x.PostingDateNew,x.AddedDate asc";
          
            return _sqlRepository.GetDataTable(cmdText);
        }

       
  
        private Dictionary<string, object> GetLoanData(string financingId, string voucherId)
        {
            var cmdText = @"SELECT F.DocRefNo,F.PartyType,F.CurrencyId,F.CashMasterId,F.BankMasterId,F.Narration,F.TransactionType
                            ,Particulars=case when F.PartyId<>'' THEN P.UserName 
            				 WHEN F.BankMasterId<>'' THEN BM.AccountTitle
            				 WHEN F.CashMasterId <>'' THEN CM.UserName	END 
                             FROM 
                             TRN.Financing F 
                             LEFT JOIN MST.BankMaster BM ON BM.Id=F.BankMasterId
                             LEFT JOIN MST.CashMaster CM ON CM.Id=F.CashMasterId
                             LEFT JOIN HKP.Party P ON P.Id=F.PartyId
                             WHERE F.Id='"+ financingId + "'";
            return _sqlRepository.GetData(cmdText);

        }
        //Specify loan ledger report
        public IWorkbook GetLoanLedgerReport(string companyGroupId, string companyId, string plantId,string plantName ,TransactionType transactionType, string voucherId, string financingId)
        {
            try
            {
                
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Register";

                var row = 6;
                var colLast = 8;
                var col = 1;

                int colPrincipleBalance = 0;
                int colInterestBalance = 0;
                //sheet = null;

                var loanMaster = GetLoanData(financingId, voucherId); //Header loan  query
            
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Loan No");
                sheet.Range[row, 1, row, 2].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, loanMaster["DocRefNo"].ToString());
                sheet.Range[row, 3, row, 4].Merge();

                if(transactionType== TransactionType.LoanTaken)
                reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Loan From");
                else
                    reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Loan To");

                sheet.Range[row, 6, row, 7].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 8, loanMaster["Particulars"].ToString());

                row++;  //7

                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                if (companyCurrencyId != Convert.ToString(loanMaster["CurrencyId"]))
                {
                    reportUtility.SetHeaderText(ref sheet, row, colLast + 1, "Transaction", ExcelHAlign.HAlignCenter);
                    sheet.Range[row, colLast + 1, row, colLast + 3].Merge();
                    colLast = colLast + 3;
                    reportUtility.SetHeaderText(ref sheet, row, colLast + 1, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[row, colLast + 1, row, colLast + 3].Merge();
                    row++;
                }

                row++;

                // Set Row Header


                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 12); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No",13 ); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Ref", 14); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Date", 12); col++;

                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 40); col++;

                sheet.Range[row, col].WrapText = true;

                if (companyCurrencyId != Convert.ToString(loanMaster["CurrencyId"]))
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Currency", 9, ExcelHAlign.HAlignRight); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); col++;
                }


                reportUtility.SetHeaderText(ref sheet, row-1, col, "Principle", 12, ExcelHAlign.HAlignRight); int colPrinciple = col;
                reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); int PrincipleDebitFormula = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); int PrincipleCreditFormula = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Balance",13, ExcelHAlign.HAlignRight); colPrincipleBalance = col; col++;
                sheet.Range[row-1, colPrinciple, row-1, colPrincipleBalance].Merge();
                sheet.Range[row - 1, colPrinciple, row - 1, colPrincipleBalance].BorderAround(ExcelLineStyle.Thin);
                sheet.Range[row - 1, colPrinciple, row - 1, colPrincipleBalance].BorderInside(ExcelLineStyle.Thin);
                sheet.Range[row - 1, colPrinciple, row - 1, colPrincipleBalance].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet[row, colPrincipleBalance].VerticalAlignment = ExcelVAlign.VAlignTop;

                reportUtility.SetHeaderText(ref sheet, row - 1, col, "Interest", 12, ExcelHAlign.HAlignRight); int colInterest = col;
                reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); int colInterestDebitAmount = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); int colInterestCreditAmount = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Balance", 13, ExcelHAlign.HAlignRight); colInterestBalance = col; col++;
                sheet.Range[row - 1, colInterest, row - 1, colInterestBalance].Merge();
                sheet.Range[row - 1, colInterest, row - 1, colInterestBalance].BorderAround(ExcelLineStyle.Thin);
                sheet.Range[row - 1, colInterest, row - 1, colInterestBalance].BorderInside(ExcelLineStyle.Thin);
                sheet.Range[row - 1, colInterest, row - 1, colInterestBalance].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                reportUtility.SetHeaderText(ref sheet, row, col, "RemaningBalance", 17, ExcelHAlign.HAlignRight);  col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Dr/Cr", 7, ExcelHAlign.HAlignRight);

                row++;
                

                var ledgerData = GetLoanLedger(companyGroupId, companyId, plantId,voucherId, financingId);
                row++;
                var firstRow = row;
                string principleBalanceFormula = "";
                string interestBalanceFormula = "";
                // Get bank transaction data.
                if (ledgerData.Rows.Count > 0)
                {
                    col = 1;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        col = 1;
                       
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PostingDate"].ToString(), 12, ExcelHAlign.HAlignLeft); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherNo"].ToString(), 13, ExcelHAlign.HAlignLeft); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocRefNo"].ToString(), 14, ExcelHAlign.HAlignLeft); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocDate"].ToString(), 12, ExcelHAlign.HAlignLeft); col++;
                        sheet[row, col].ColumnWidth = 40;
                        sheet.Range[row, col].WrapText = true;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Narration"].ToString());
                       
                        col++;

                        sheet.Range[row, col].WrapText = true;
                        if (companyCurrencyId != Convert.ToString(loanMaster["CurrencyId"]))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["CurrencyCode"].ToString()); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString())); col++;
                        }
                        // Base currency checking
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString())); col++;
                        principleBalanceFormula = "(" + reportUtility.GetColumnNameForXls(col) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(col - 1) + row + ")-" + reportUtility.GetColumnNameForXls(col - 2) + row + "";
                        sheet.Range[row, col].Formula = principleBalanceFormula;
                        sheet.Range[row, col].NumberFormat = clsStaticInfo.NumberFormat();
                        sheet[row, col].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
                        col++;

                        sheet.Range[row, col].WrapText = true;
                        if (companyCurrencyId != Convert.ToString(loanMaster["CurrencyId"]))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["CurrencyCode"].ToString()); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["InterestDrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["InterestCrAmount"].ToString())); col++;
                        }
                        
                        // Base currency checking
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["InterestCompanyCurrencyDrAmount"].ToString())); col++;
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["InterestCompanyCurrencyCrAmount"].ToString())); col++;
                        //Interest Balance
                        interestBalanceFormula = "(" + reportUtility.GetColumnNameForXls(col) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(col - 1) + row + ")-" + reportUtility.GetColumnNameForXls(col - 2) + row + "";
                        sheet.Range[row, col].Formula = interestBalanceFormula; 
                        sheet.Range[row, col].NumberFormat = clsStaticInfo.NumberFormat();
                        sheet[row, col].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
                        col++;

                        //Remaning Balance
                        sheet.Range[row, col].Formula = "(" + reportUtility.GetColumnNameForXls(colPrincipleBalance) + (row) + "+" + reportUtility.GetColumnNameForXls(colInterestBalance) + row+ ")";
                        sheet.Range[row, col].NumberFormat = clsStaticInfo.NumberFormat();
                        sheet[row, col].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
                        col++;

                        //sheet.Range[row, col].Formula = "(" + reportUtility.GetColumnNameForXls(colDebitAmount) + (row) + "+" + reportUtility.GetColumnNameForXls(colDebitAmount) + row + ")";
                        //sheet.Range[row, col].NumberFormat = clsStaticInfo.NumberFormat(4);//reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();

                        sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + "<= 0, \"  Dr\", \"  Cr\")";
                        sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
                        row++;
                    }
                }


             


                //worksheet[ROW, COL].Text = "IsFollowUpApplicable";
                //int colIsFollowUpApplicable = COL;
                //worksheet[ROW, COL].ColumnWidth = 15;

                var lastRow = row;
                reportUtility.SetHeaderText(ref sheet, lastRow, 5, "Total :", ExcelHAlign.HAlignRight);
                sheet.Range[row, 1].BorderAround(ExcelLineStyle.Thin);
                sheet.Range[row, 1, row, 4].Merge();
                //reportUtility.SetText(ref sheet, lastRow, 5, "Total :", true);

                sheet.Range[lastRow, PrincipleDebitFormula].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(PrincipleDebitFormula) + firstRow + ":" + reportUtility.GetColumnNameForXls(PrincipleDebitFormula) + (lastRow - 1) + ")";
                sheet.Range[lastRow, PrincipleDebitFormula].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[lastRow, PrincipleDebitFormula].CellStyle.Font.Bold = true;
               // sheet.Range[lastRow, PrincipleDebitFormula].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;

                sheet.Range[lastRow, PrincipleDebitFormula].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[lastRow, PrincipleCreditFormula].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(PrincipleCreditFormula) + firstRow + ":" + reportUtility.GetColumnNameForXls(PrincipleCreditFormula) + (lastRow - 1) + ")";
                sheet.Range[lastRow, PrincipleCreditFormula].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[lastRow, PrincipleCreditFormula].CellStyle.Font.Bold = true;
                sheet.Range[lastRow, PrincipleCreditFormula].BorderAround(ExcelLineStyle.Hair);


                sheet.Range[lastRow, colPrincipleBalance].Formula = principleBalanceFormula; // "=SUM(" + reportUtility.GetColumnNameForXls(colPrincipleBalance) + firstRow + ":" + reportUtility.GetColumnNameForXls(colPrincipleBalance) + (lastRow - 1) + ")";
                sheet.Range[lastRow, colPrincipleBalance].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[lastRow, colPrincipleBalance].CellStyle.Font.Bold = true;
                sheet.Range[lastRow, colPrincipleBalance].BorderAround(ExcelLineStyle.Hair);



                sheet.Range[lastRow, colInterestDebitAmount].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colInterestDebitAmount) + firstRow + ":" + reportUtility.GetColumnNameForXls(colInterestDebitAmount) + (lastRow - 1) + ")";
                sheet.Range[lastRow, colInterestDebitAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[lastRow, colInterestDebitAmount].CellStyle.Font.Bold = true;
                sheet.Range[lastRow, colInterestDebitAmount].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                sheet.Range[lastRow, colInterestDebitAmount].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[lastRow, colInterestCreditAmount].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colInterestCreditAmount) + firstRow + ":" + reportUtility.GetColumnNameForXls(colInterestCreditAmount) + (lastRow - 1) + ")";
                sheet.Range[lastRow, colInterestCreditAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[lastRow, colInterestCreditAmount].CellStyle.Font.Bold = true;
                sheet.Range[lastRow, colInterestCreditAmount].CellStyle.Interior.ColorIndex = ExcelKnownColors.Yellow;
                sheet.Range[lastRow, colInterestCreditAmount].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[lastRow, colInterestBalance].Formula = interestBalanceFormula;//"=SUM(" + reportUtility.GetColumnNameForXls(colDebitAmount) + firstRow + ":" + reportUtility.GetColumnNameForXls(colDebitAmount) + (lastRow - 1) + ")";
                sheet.Range[lastRow, colInterestBalance].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[lastRow, colInterestBalance].CellStyle.Font.Bold = true;
                sheet.Range[lastRow, colInterestBalance].CellStyle.Interior.ColorIndex = ExcelKnownColors.Green;
                sheet.Range[lastRow, colInterestBalance].BorderAround(ExcelLineStyle.Hair);
                row++;
                row++;

                //sheet.Range[row, 2].Text = "Interest Expense";
                //sheet.Range[row, 2].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sheet.Range[row, 3].Text = "Paid";
                //sheet.Range[row, 3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 3].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sheet.Range[row, 4].Text = "Balance";
                //sheet.Range[row, 4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
                //row++;

                //sheet.Range[row, 1].Text = "Accrual Basic";
                //sheet.Range[row, 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;


                //var InterestData = GetLoanInterestData(companyGroupId, companyId, plantId, voucherId, financingId);

                //sheet.Range[row, 2].Number = Convert.ToDouble(InterestData.Rows[0]["InterestAccrualAmount"]);
                //sheet.Range[row, 2].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[row, 2].CellStyle.Interior.ColorIndex = ExcelKnownColors.Yellow;
                //sheet.Range[row, 2].NumberFormat = clsStaticInfo.NumberFormat();//reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                //sheet[row, 2].NumberFormat = "#,##0.00;(#,##0.00)";
                //sheet[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;


                //sheet.Range[row, 3].Number = Convert.ToDouble(InterestData.Rows[0]["InterestPaidAmount"]);
                //sheet.Range[row, 3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 3].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[row, 3].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                //sheet.Range[row, 3].NumberFormat = clsStaticInfo.NumberFormat();
                //sheet[row, 3].NumberFormat = "#,##0.00;(#,##0.00)";

                //sheet.Range[row, 4].Number = Convert.ToDouble(InterestData.Rows[0]["InterestAccrualAmount"]) - Convert.ToDouble(InterestData.Rows[0]["InterestPaidAmount"]);
                //sheet.Range[row, 4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[row, 4].CellStyle.Interior.ColorIndex = ExcelKnownColors.Green;
                //sheet.Range[row, 4].NumberFormat = clsStaticInfo.NumberFormat();
                //sheet[row, 4].NumberFormat = "#,##0.00;(#,##0.00)";
                //row++;

                //sheet.Range[row, 1].Text = "Cash Basic";
                //sheet.Range[row, 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
          

                //sheet.Range[row, 2].Number = Convert.ToDouble(InterestData.Rows[0]["InterestCashAmount"]);
                //sheet.Range[row, 2].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[row, 2].NumberFormat = clsStaticInfo.NumberFormat();
                //sheet[row, 2].NumberFormat = "#,##0.00;(#,##0.00)";

                //sheet.Range[row, 3].Number = Convert.ToDouble(InterestData.Rows[0]["InterestCashAmount"]);
                //sheet.Range[row, 3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 3].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[row, 3].NumberFormat = clsStaticInfo.NumberFormat();
                //sheet[row, 3].NumberFormat = "#,##0.00;(#,##0.00)";
                //row++;

                //sheet.Range[row, 1].Text = "Reverse";
                //sheet.Range[row, 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;


                //sheet.Range[row, 2].Number = Convert.ToDouble(InterestData.Rows[0]["InterestReverseAmount"]);
                //sheet.Range[row, 2].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[row, 2].NumberFormat = clsStaticInfo.NumberFormat();
                //sheet[row, 2].NumberFormat = "#,##0.00;(#,##0.00)";

                //sheet.Range[row, 3].Number = Convert.ToDouble(InterestData.Rows[0]["InterestReverseAmount"]);
                //sheet.Range[row, 3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 3].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[row, 3].NumberFormat = clsStaticInfo.NumberFormat();
                //sheet[row, 3].NumberFormat = "#,##0.00;(#,##0.00)";
                //sheet.Range[row, 3].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                //row++;

                //sheet.Range[row, 1].Text = "Total:";
                //sheet.Range[row, 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sheet.Range[row, 2].Number = Convert.ToDouble(InterestData.Rows[0]["InterestAccrualAmount"]) + Convert.ToDouble(InterestData.Rows[0]["InterestCashAmount"]) - Convert.ToDouble(InterestData.Rows[0]["InterestReverseAmount"]);
                //sheet.Range[row, 2].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[row, 2].NumberFormat = clsStaticInfo.NumberFormat();
                //sheet[row, 2].NumberFormat = "#,##0.00;(#,##0.00)";

                //sheet.Range[row, 3].Number = Convert.ToDouble(InterestData.Rows[0]["InterestPaidAmount"]) + Convert.ToDouble(InterestData.Rows[0]["InterestCashAmount"]);
                //sheet.Range[row, 3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 3].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[row, 3].NumberFormat = clsStaticInfo.NumberFormat();
                //sheet[row, 3].NumberFormat = "#,##0.00;(#,##0.00)";

                //sheet.Range[row, 4].Number = Convert.ToDouble(InterestData.Rows[0]["InterestAccrualAmount"]) - Convert.ToDouble(InterestData.Rows[0]["InterestPaidAmount"]);
                //sheet.Range[row, 4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[row, 4].NumberFormat = clsStaticInfo.NumberFormat();
                //sheet[row, 4].NumberFormat = "#,##0.00;(#,##0.00)";


                // sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, col, "Loan Register", companyId,plantId, plantName, null);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(col) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);


                #region Freeze Panes

                sheet.IsDisplayZeros = false;
                sheet.UsedRange["A9"].FreezePanes();
                sheet.FirstVisibleColumn = 1;
                sheet.FirstVisibleRow = 9;

                #endregion Freeze Panes
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //All Register Report Data
        private DataTable GetLoanInterestLedger(string companyGroupId, string companyId, string plantId, string voucherId, string financingId)
        {
            var cmdText = @"
                  
                    DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                    SELECT x.PostingDate,x.VoucherNo,x.VoucherDate,x.DocRefNo,x.DocDate,x.Narration,x.DrAmount,x.CrAmount,x.InterestDrAmount,x.InterestCrAmount,x.CompanyCurrencyId,x.CompanyCurrencyDrAmount,x.CompanyCurrencyCrAmount,x.InterestCompanyCurrencyDrAmount,x.InterestCompanyCurrencyCrAmount,x.CurrencyCode,x.GLGeneralInfoCode,x.GLGeneralInfoName,x.GLGeneralInfoId,x.GSTIN
                    ,x.RefNo,x.BudgetName,x.CurrencyId,x.ActivityName,x.PartyCode,x.PartyName,x.PartyPlantName,x.FinancingId
                    FROM(
                    SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                    , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration, ISNULL(VD.DrAmount,0) AS DrAmount, ISNULL(VD.CrAmount,0) AS CrAmount, 0 InterestDrAmount,0 InterestCrAmount
                    , CC.CompanyCurrencyId, ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) AS CompanyCurrencyCrAmount,0 InterestCompanyCurrencyDrAmount, 0 InterestCompanyCurrencyCrAmount
                    , C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                    , VD.GLGeneralInfoId,GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName
                    ,F.Id FinancingId,v.PostingDate PostingDateNew,v.AddedDate
                    FROM
                    [TRN].[Financing] AS F
                    LEFT JOIN [TRN].[FinancingDetail] AS FD ON FD.FinancingId=F.Id
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON FD.Id=VD.FinancingDetailId
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                    --LEFT JOIN [TRN].[FinancingWriteOff] AS FW ON FW.FinancingId=F.Id
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
                    where f.Id='" + financingId + @"' AND VD.FinancingDetailId<>'' AND F.IsPark=0
                    
                    UNION

                    SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                    , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration, VD.DrAmount, VD.CrAmount, ISNULL(VD.DrAmount,0) AS InterestDrAmount, ISNULL(VD.CrAmount,0) AS InterestCrAmount
                    , CC.CompanyCurrencyId,ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) AS CompanyCurrencyCrAmount,ISNULL(CC.CompanyCurrencyDrAmount, 0) AS InterestCompanyCurrencyDrAmount
                    , ISNULL(CC.CompanyCurrencyCrAmount, 0) AS InterestCompanyCurrencyCrAmount
                    , C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                    , VD.GLGeneralInfoId,GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName
                    ,LIP.FinancingId,v.PostingDate PostingDateNew,v.AddedDate
                    FROM
                    [TRN].FinancingSubsequentTransaction AS LIP
                    LEFT JOIN TRN.Financing F ON F.Id=LIP.FinancingId
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=LIP.VoucherId
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON LIP.VoucherDetailId=VD.Id 
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
                    WHERE LIP.FinancingId='" + financingId + @"' and lip.IsPark=0  AND LIP.TransactionType in ('InterestPayable','LoanTax','OtherExpensesPayable','AccrulInterestPayment','InterestPayableReverse','ChargesPayableReverse')
                    UNION

					 SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                    , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration, VD.DrAmount, VD.CrAmount, 0 InterestDrAmount, 0 InterestCrAmount
                    , CC.CompanyCurrencyId,ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) CompanyCurrencyCrAmount,0 InterestCompanyCurrencyDrAmount,  0 InterestCompanyCurrencyCrAmount
                    , C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                    , VD.GLGeneralInfoId,GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName
                    ,LIP.FinancingId,v.PostingDate PostingDateNew,v.AddedDate
                    FROM
                    [TRN].FinancingSubsequentTransaction AS LIP
                    LEFT JOIN TRN.Financing F ON F.Id=LIP.FinancingId
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=LIP.VoucherId
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON LIP.VoucherDetailId=VD.Id 
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
                    WHERE LIP.FinancingId='" + financingId + @"' and lip.IsPark=0  AND LIP.TransactionType in ('AdditionalLoanPayable')
                    UNION
                    SELECT REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                    , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.Narration, VD.DrAmount, VD.CrAmount, 0 InterestDrAmount, 0 InterestCrAmount
                    , CC.CompanyCurrencyId,ISNULL(CC.CompanyCurrencyDrAmount, 0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount, 0) CompanyCurrencyCrAmount,0 InterestCompanyCurrencyDrAmount, 0 InterestCompanyCurrencyCrAmount
                    , C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, PP.GSTIN
                    , VD.GLGeneralInfoId,GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName,V.CurrencyId, A.UserName AS ActivityName, P.Code AS PartyCode, P.UserName AS PartyName, PP.UserName AS PartyPlantName
                    ,LIP.FinancingId,v.PostingDate PostingDateNew,v.AddedDate
                    FROM
                    [TRN].FinancingSubsequentTransaction AS LIP
                    LEFT JOIN TRN.Financing F ON F.Id=LIP.FinancingId
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=LIP.VoucherId
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON LIP.VoucherDetailId=VD.Id
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
                    WHERE LIP.SetOffFinancingId='" + financingId + @"' and lip.IsPark=0 AND LIP.TransactionType in ('LoanPayment')
                    ) x
                     ORDER BY x.PostingDateNew,x.AddedDate asc";

            return _sqlRepository.GetDataTable(cmdText);
        }

        //Loan data
        private Dictionary<string, object> GetLoanInterestData(string financingId, string voucherId)
        {
            var cmdText = @"SELECT F.DocRefNo,F.PartyType,F.CurrencyId,F.CashMasterId,F.BankMasterId,F.Narration,F.TransactionType
                            ,Particulars=case when F.PartyId<>'' THEN P.UserName 
            				 WHEN F.BankMasterId<>'' THEN BM.AccountTitle
            				 WHEN F.CashMasterId <>'' THEN CM.UserName	END 
                             FROM 
                             TRN.Financing F 
                             LEFT JOIN MST.BankMaster BM ON BM.Id=F.BankMasterId
                             LEFT JOIN MST.CashMaster CM ON CM.Id=F.CashMasterId
                             LEFT JOIN HKP.Party P ON P.Id=F.PartyId
                             WHERE F.Id='" + financingId + "'";
            return _sqlRepository.GetData(cmdText);

        }
        //Specify loan ledger report
        public IWorkbook GetLoanRegisterLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, TransactionType transactionType, string voucherId, string financingId)
        {
            try
            {

                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Register";

                var row = 6;
                var colLast = 8;
                var col = 1;

                int colPrincipleBalance = 0;
                //int colInterestBalance = 0;
                //sheet = null;

                var loanMaster = GetLoanInterestData(financingId, voucherId); //Header loan  query

                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Loan No");
                sheet.Range[row, 1, row, 2].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, loanMaster["DocRefNo"].ToString());
                sheet.Range[row, 3, row, 4].Merge();

                if (transactionType == TransactionType.LoanTaken)
                    reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Loan From");
                else
                    reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Loan To");

                sheet.Range[row, 6, row, 7].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 8, loanMaster["Particulars"].ToString());

                row++;  //7

                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                if (companyCurrencyId != Convert.ToString(loanMaster["CurrencyId"]))
                {
                    reportUtility.SetHeaderText(ref sheet, row, colLast + 1, "Transaction", ExcelHAlign.HAlignCenter);
                    sheet.Range[row, colLast + 1, row, colLast + 3].Merge();
                    colLast = colLast + 3;
                    reportUtility.SetHeaderText(ref sheet, row, colLast + 1, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[row, colLast + 1, row, colLast + 3].Merge();
                    row++;
                }

                row++;

                // Set Row Header


                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 12); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 13); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Ref", 14); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Date", 12); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 50); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "GL", 40); col++;

                sheet.Range[row, col].WrapText = true;

                if (companyCurrencyId != Convert.ToString(loanMaster["CurrencyId"]))
                {
                    reportUtility.SetHeaderText(ref sheet, row, col, "Currency", 9, ExcelHAlign.HAlignRight); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); col++;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); col++;
                }


                reportUtility.SetHeaderText(ref sheet, row - 1, col, "Principle", 12, ExcelHAlign.HAlignRight); int colPrinciple = col;
                reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); int PrincipleDebitFormula = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); int PrincipleCreditFormula = col; col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Balance", 13, ExcelHAlign.HAlignRight); colPrincipleBalance = col; col++;
                sheet.Range[row - 1, colPrinciple, row - 1, colPrincipleBalance].Merge();
                sheet.Range[row - 1, colPrinciple, row - 1, colPrincipleBalance].BorderAround(ExcelLineStyle.Thin);
                sheet.Range[row - 1, colPrinciple, row - 1, colPrincipleBalance].BorderInside(ExcelLineStyle.Thin);
                sheet.Range[row - 1, colPrinciple, row - 1, colPrincipleBalance].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet[row, colPrincipleBalance].VerticalAlignment = ExcelVAlign.VAlignTop;

                //reportUtility.SetHeaderText(ref sheet, row - 1, col, "Interest", 12, ExcelHAlign.HAlignRight); int colInterest = col;
                //reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 12, ExcelHAlign.HAlignRight); int colInterestDebitAmount = col; col++;
                //reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 12, ExcelHAlign.HAlignRight); int colInterestCreditAmount = col; col++;
                //reportUtility.SetHeaderText(ref sheet, row, col, "Balance", 13, ExcelHAlign.HAlignRight); colInterestBalance = col; col++;
                //sheet.Range[row - 1, colInterest, row - 1, colInterestBalance].Merge();
                //sheet.Range[row - 1, colInterest, row - 1, colInterestBalance].BorderAround(ExcelLineStyle.Thin);
                //sheet.Range[row - 1, colInterest, row - 1, colInterestBalance].BorderInside(ExcelLineStyle.Thin);
                //sheet.Range[row - 1, colInterest, row - 1, colInterestBalance].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                reportUtility.SetHeaderText(ref sheet, row, col, "RemaningBalance", 17, ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Dr/Cr", 7, ExcelHAlign.HAlignRight);

                row++;

                //Header Data 
                var ledgerData = GetLoanInterestLedger(companyGroupId, companyId, plantId, voucherId, financingId);
                row++;
                var firstRow = row;
                string principleBalanceFormula = "";
               // string interestBalanceFormula = "";
                // Get bank transaction data.
                if (ledgerData.Rows.Count > 0)
                {
                    col = 1;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        col = 1;

                        var glName = ledgerData.Rows[i]["BudgetName"].ToString();

                        //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["PostingDate"].ToString(), 12, ExcelHAlign.HAlignLeft); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherNo"].ToString(), 13, ExcelHAlign.HAlignLeft); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocRefNo"].ToString(), 14, ExcelHAlign.HAlignLeft); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocDate"].ToString(), 12, ExcelHAlign.HAlignLeft); col++;
                        sheet[row, col].ColumnWidth = 40;
                        sheet.Range[row, col].WrapText = true;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Narration"].ToString());
                        col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + ledgerData.Rows[i]["ActivityName"]); col++;



                        sheet.Range[row, col].WrapText = true;
                        if (companyCurrencyId != Convert.ToString(loanMaster["CurrencyId"]))
                        {
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["CurrencyCode"].ToString()); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString())); col++;
                        }
                        // Base currency checking
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString())); col++;
                        principleBalanceFormula = "(" + reportUtility.GetColumnNameForXls(col) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(col - 1) + row + ")-" + reportUtility.GetColumnNameForXls(col - 2) + row + "";
                        sheet.Range[row, col].Formula = principleBalanceFormula;
                        sheet.Range[row, col].NumberFormat = clsStaticInfo.NumberFormat();
                        sheet[row, col].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
                        col++;

                        sheet.Range[row, col].WrapText = true;
                        //if (companyCurrencyId != Convert.ToString(loanMaster["CurrencyId"]))
                        //{
                        //    reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["CurrencyCode"].ToString()); col++;
                        //    reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["InterestDrAmount"].ToString())); col++;
                        //    reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["InterestCrAmount"].ToString())); col++;
                        //}

                        //// Base currency checking
                        //reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["InterestCompanyCurrencyDrAmount"].ToString())); col++;
                        //reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["InterestCompanyCurrencyCrAmount"].ToString())); col++;
                        ////Interest Balance
                        //interestBalanceFormula = "(" + reportUtility.GetColumnNameForXls(col) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(col - 1) + row + ")-" + reportUtility.GetColumnNameForXls(col - 2) + row + "";
                        //sheet.Range[row, col].Formula = interestBalanceFormula;
                        //sheet.Range[row, col].NumberFormat = clsStaticInfo.NumberFormat();
                        //sheet[row, col].NumberFormat = "#,##0.00;(#,##0.00)";
                        //sheet[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
                        //col++;

                        //Remaning Balance
                        sheet.Range[row, col].Formula = "(" + reportUtility.GetColumnNameForXls(colPrincipleBalance) + (row) + /*"+" + reportUtility.GetColumnNameForXls(colInterestBalance) + row +*/ ")";
                        sheet.Range[row, col].NumberFormat = clsStaticInfo.NumberFormat();
                        sheet[row, col].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
                        col++;

                        //sheet.Range[row, col].Formula = "(" + reportUtility.GetColumnNameForXls(colDebitAmount) + (row) + "+" + reportUtility.GetColumnNameForXls(colDebitAmount) + row + ")";
                        //sheet.Range[row, col].NumberFormat = clsStaticInfo.NumberFormat(4);//reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();

                        sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + "<= 0, \"  Dr\", \"  Cr\")";
                        sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
                        row++;
                    }
                }


                //worksheet[ROW, COL].Text = "IsFollowUpApplicable";
                //int colIsFollowUpApplicable = COL;
                //worksheet[ROW, COL].ColumnWidth = 15;

                var lastRow = row;
                reportUtility.SetHeaderText(ref sheet, lastRow, 5, "Total :", ExcelHAlign.HAlignRight);
                sheet.Range[row, 1].BorderAround(ExcelLineStyle.Thin);
                sheet.Range[row, 1, row, 4].Merge();
                //reportUtility.SetText(ref sheet, lastRow, 5, "Total :", true);

                sheet.Range[lastRow, PrincipleDebitFormula].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(PrincipleDebitFormula) + firstRow + ":" + reportUtility.GetColumnNameForXls(PrincipleDebitFormula) + (lastRow - 1) + ")";
                sheet.Range[lastRow, PrincipleDebitFormula].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[lastRow, PrincipleDebitFormula].CellStyle.Font.Bold = true;
                // sheet.Range[lastRow, PrincipleDebitFormula].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;

                sheet.Range[lastRow, PrincipleDebitFormula].BorderAround(ExcelLineStyle.Hair);

                sheet.Range[lastRow, PrincipleCreditFormula].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(PrincipleCreditFormula) + firstRow + ":" + reportUtility.GetColumnNameForXls(PrincipleCreditFormula) + (lastRow - 1) + ")";
                sheet.Range[lastRow, PrincipleCreditFormula].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[lastRow, PrincipleCreditFormula].CellStyle.Font.Bold = true;
                sheet.Range[lastRow, PrincipleCreditFormula].BorderAround(ExcelLineStyle.Hair);


                sheet.Range[lastRow, colPrincipleBalance].Formula = principleBalanceFormula; // "=SUM(" + reportUtility.GetColumnNameForXls(colPrincipleBalance) + firstRow + ":" + reportUtility.GetColumnNameForXls(colPrincipleBalance) + (lastRow - 1) + ")";
                sheet.Range[lastRow, colPrincipleBalance].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[lastRow, colPrincipleBalance].CellStyle.Font.Bold = true;
                sheet.Range[lastRow, colPrincipleBalance].BorderAround(ExcelLineStyle.Hair);



                //sheet.Range[lastRow, colInterestDebitAmount].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colInterestDebitAmount) + firstRow + ":" + reportUtility.GetColumnNameForXls(colInterestDebitAmount) + (lastRow - 1) + ")";
                //sheet.Range[lastRow, colInterestDebitAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                //sheet.Range[lastRow, colInterestDebitAmount].CellStyle.Font.Bold = true;
                //sheet.Range[lastRow, colInterestDebitAmount].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                //sheet.Range[lastRow, colInterestDebitAmount].BorderAround(ExcelLineStyle.Hair);

                //sheet.Range[lastRow, colInterestCreditAmount].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colInterestCreditAmount) + firstRow + ":" + reportUtility.GetColumnNameForXls(colInterestCreditAmount) + (lastRow - 1) + ")";
                //sheet.Range[lastRow, colInterestCreditAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                //sheet.Range[lastRow, colInterestCreditAmount].CellStyle.Font.Bold = true;
                //sheet.Range[lastRow, colInterestCreditAmount].CellStyle.Interior.ColorIndex = ExcelKnownColors.Yellow;
                //sheet.Range[lastRow, colInterestCreditAmount].BorderAround(ExcelLineStyle.Hair);

                //sheet.Range[lastRow, colInterestBalance].Formula = interestBalanceFormula;
                //sheet.Range[lastRow, colInterestBalance].CellStyle.Font.Bold = true;
                //sheet.Range[lastRow, colInterestBalance].CellStyle.Interior.ColorIndex = ExcelKnownColors.Green;
                //sheet.Range[lastRow, colInterestBalance].BorderAround(ExcelLineStyle.Hair);
                row++;
                row++;

                //sheet.Range[row, 2].Text = "Interest Expense";
                //sheet.Range[row, 2].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sheet.Range[row, 3].Text = "Paid";
                //sheet.Range[row, 3].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 3].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sheet.Range[row, 4].Text = "Balance";
                //sheet.Range[row, 4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
                //row++;

                //sheet.Range[row, 1].Text = "Accrual Basic";
                //sheet.Range[row, 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;


                // sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, col, "Loan Register", companyId, plantId, plantName, null);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(col) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);


                #region Freeze Panes

                sheet.IsDisplayZeros = false;
                sheet.UsedRange["A9"].FreezePanes();
                sheet.FirstVisibleColumn = 1;
                sheet.FirstVisibleRow = 9;

                #endregion Freeze Panes
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }


        private Dictionary<string, object> GetLoanInterestPayableReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, string sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.AddedBy, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , P.UserName AS Vendor, PP.UserName AS VendorPlant, BJ.CurrencyId, C.Code AS CurrencyCode
                            FROM [TRN].[FinancingSubsequentTransaction] AS BJ
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=BJ.VoucherId
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [HKP].[Party] AS P ON P.Id=BJ.PartyId
							LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=BJ.PartyPlantId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                            WHERE BJ.Archive=0 AND BJ.CompanyGroupId='" + companyGroupId + "' AND BJ.CompanyId='" + companyId + "' AND BJ.PlantId='" + plantId + "' AND BJ.VoucherId='" + voucherId + "' AND BJ.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }
       
        public IWorkbook GetLoanInterestPayableReport(out string reportFileName, string companyGroupId, string companyId, string plantId,string plantName, string voucherId, string sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";
            
            var header = GetLoanInterestPayableReportHeader(companyGroupId, companyId, plantId, voucherId, sourceType);

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
            reportUtility.SetText(ref sheet, row, 4, header["DocRefNo"].ToString(), false, true);

            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();

            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();

            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;
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
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 2, header["PostedBy"].ToString());
                sheet.Range[row, 2].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 2, "Checked By", true);

                sheet.Range[row, 4].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 4, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantId,plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantId,plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }

    }
}