using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Currencies;
using Library.Model.Enums;
using Library.Model.Vouchers;
using Library.Service.Currencies;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Organizations;
using Library.Service.Properties;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Accounting.Accounts
{
    public class AccountsInvoiceReportService
    {
        private readonly ISqlRepository _sqlRepository;

        public AccountsInvoiceReportService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
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
        //testing 
        //private bool GetPlantIsShowFCInWord(string plantId)
        //{
        //   var IsShowFCInWord = @"SELECT IsShowFCInWord FROM ORG.Plant WHERE Id='"+ plantId + "'";
        //    return bool.Parse(IsShowFCInWord);
        //}

        private bool GetPlantIsShowFCInWord(string plantId)
        {
            return bplib.clsWebLib.GetBoolData(_sqlRepository.GetDataCollection(@"SELECT IsShowFCInWord FROM ORG.Plant WHERE Id='" + plantId + "'")[0]["IsShowFCInWord"].ToString());
        }

        private Dictionary<string, object> GetSuspensPayableHeader(string companyGroupId, string companyId, string plantId, string invoiceGroupNo, SourceType sourceType)
        {
            var cmdText = @"SELECT AW.InvoiceGroupNo, P.Code AS PartyCode, P.UserName AS PartyName, AW.PostingDate, AW.DocDate, AW.DocRefNo, C.Code AS CurrencyCode,SUM(IWD.Amount) Amount
                                    , AW.PartyPlantId, PP.UserName AS PartyPlantName , UPPER(AW.Narration) AS Narration
                                     ,v.VoucherDate
                                    ,VoucherNo=STUFF((SELECT DISTINCT ','+xpo.VoucherNo from
                                    			[TRN].Voucher xpo
                                    			INNER JOin trn.[Invoice] xPDAMAP on xpo.Id=xPDAMAP.VoucherId
                                    			WHERE AW.InvoiceGroupNo=xPDAMAP.InvoiceGroupNo for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												, VT.UserName AS VoucherTypeName,AW.AddedBy,NULL PostedBy, CASE WHEN AW.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                                    ,AW.CurrencyId
                                    FROM [TRN].[Invoice] AS AW
                                    LEFT JOIN (
                                    SELECT InvoiceId,SUM(Amount) Amount FROM [TRN].[InvoiceDetail] Group BY InvoiceId
                                    ) AS IWD ON IWD.InvoiceId=AW.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=AW.VoucherId
									LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=AW.VoucherTypeId
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=AW.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AW.PartyPlantId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=AW.CurrencyId
                                    WHERE AW.Archive=0 AND AW.CompanyGroupId='" + companyGroupId + "' AND AW.CompanyId='" + companyId + "' AND AW.PlantId='" + plantId + "' AND AW.InvoiceGroupNo='" + invoiceGroupNo + @"' AND AW.[SourceType]='SuspensePayable'
                                    Group BY  P.Code , P.UserName, AW.PostingDate,VT.UserName,AW.AddedBy,AW.Narration,AW.CurrencyId
                                    , AW.DocDate, AW.DocRefNo, C.Code, AW.PartyPlantId, PP.UserName, AW.IsPark,AW.InvoiceGroupNo ,v.VoucherDate";
            return _sqlRepository.GetData(cmdText);
        }

        private DataTable GetSuspensPayableVoucher(string invoiceGroupNo)
        {
            try
            {
                var sql = @"SELECT  GL.Id AS AccountCodeId--, VDC.VoucherDetailId
							, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark
							, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END
							, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo--, V.VoucherNo
							, UPPER(V.Narration) AS Narration
                            , V.CurrencyId
							, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId
							 , VDC.ToCurrencyRate, SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount, SUM(VDC.DrAmount) AS CompanyCurrencyDrAmount, SUM(VDC.CrAmount) AS CompanyCurrencyCrAmount
							, [DRCR]=CASE WHEN SUM(VDC.DrAmount)>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget
                            , Activity= CASE WHEN VD.BankMasterId<>'' THEN ACT.UserName+' - '+ BM.AccountTitle ELSE ACT.UserName END,VD.PartyType
                            FROM 
							 [TRN].[Invoice] AS IV 
                            LEFT JOIN [TRN].[Voucher] AS V  ON IV.VoucherId=V.Id
							LEFT JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherId=V.Id
                            LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            LEFT JOIN [TRN].[InvoiceDetail] AS IVD ON IVD.Id=VD.InvoiceDetailId
                            
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                            WHERE V.Archive=0 AND IV.InvoiceGroupNo='" + invoiceGroupNo + @"' 
							GROUP BY  GL.Id 
							, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, V.PostingDate
                            , V.IsPark , v.DocDate, V.DocRefNo, V.Narration
                            , V.CurrencyId, CU1.Code 
							, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code 
                            , VDC.FromCurrencyId, VDC.ToCurrencyId
							, VDC.ToCurrencyRate--, VD.DrAmount, VD.CrAmount, VDC.DrAmount, VDC.CrAmount
                            , VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, P.UserName, PP.UserName , VD.Narration, BUD.UserName
                            ,  VD.BankMasterId,ACT.UserName,BM.AccountTitle ,VD.PartyType
							ORDER BY SUM(VD.DrAmount) DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IWorkbook GetSuspensPayableReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string invoiceGroupNo, string sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetSuspensPayableHeader(companyGroupId, companyId, plantId, invoiceGroupNo, SourceType.SuspensePayable);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetSuspensPayableVoucher(invoiceGroupNo);

            var transcationCurrency = header["CurrencyId"].ToString();
            GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 4, header["VoucherDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "DocDate");
            reportUtility.SetText(ref sheet, row, 4, header["DocDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor:");
            reportUtility.SetText(ref sheet, row, 2, header["PartyName"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 4, header["DocRefNo"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor Plant");
            reportUtility.SetText(ref sheet, row, 2, header["PartyPlantName"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Status");
            reportUtility.SetText(ref sheet, row, 4, header["Status"].ToString());

            row++;



            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 3, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 3, row, 4].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 3, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 3, row, 4].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 5, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 5, row, 6].Merge();
            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();

                    if (companyCurrencyId != transcationCurrency)
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

                reportUtility.SetText(ref sheet, row, 2, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(3) + 12 + ":" + reportUtility.GetColumnNameForXls(3) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 12 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + 12 + ":" + reportUtility.GetColumnNameForXls(6) + (row - 1) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(3) + 12 + ":" + reportUtility.GetColumnNameForXls(3) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[13, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[13, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && GetPlantIsShowFCInWord(plantId))
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
                sheet[1, 2].ColumnWidth = 60;
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

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Suspense Payable", companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, "Suspense Payable", companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }

        private Dictionary<string, object> GetCustomerBanksReceiptHeader(string companyGroupId, string companyId, string plantId, string invoiceWriteOffGroupNo, string sourceType)
        {
            var cmdText = @"SELECT AW.InvoiceWriteOffGroupNo, REPLACE(CONVERT(VARCHAR(11), AW.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , P.Code AS PartyCode, P.UserName AS Customer, REPLACE(CONVERT(VARCHAR(11), AW.PostingDate, 106), ' ', '-') AS PostingDate 
                            , REPLACE(CONVERT(VARCHAR(11), AW.DocDate, 106), ' ', '-') AS DocDate, AW.DocRefNo, C.Code AS CurrencyCode,SUM(IWD.Amount) Amount
                            , AW.PartyPlantId, PP.UserName AS CustomerPlant,  AW.BankJournalId, UPPER(AW.Narration) AS Narration
                            , VT.UserName AS VoucherTypeName,AW.AddedBy,AW.UpdatedBy PostedBy, CASE WHEN AW.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            ,AW.CurrencyId
                            ,VoucherNo=STUFF((SELECT DISTINCT ','+xpo.VoucherNo from
                            			[TRN].Voucher xpo
                            			INNER JOin trn.[InvoiceWriteOff] xPDAMAP on xpo.Id=xPDAMAP.VoucherId
                            			WHERE AW.InvoiceWriteOffGroupNo=xPDAMAP.InvoiceWriteOffGroupNo for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            
                            FROM [TRN].[InvoiceWriteOff] AS AW
                            LEFT JOIN (
                            SELECT InvoiceWriteOffId,SUM(Amount) Amount FROM [TRN].[InvoiceWriteOffDetail] Group BY InvoiceWriteOffId 
                            ) AS IWD ON IWD.InvoiceWriteOffId=AW.Id
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=AW.VoucherTypeId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=AW.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AW.PartyPlantId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=AW.CurrencyId
                            WHERE AW.Archive=0 AND AW.CompanyGroupId='" + companyGroupId + "' AND AW.CompanyId='" + companyId + "' AND AW.PlantId='" + plantId + "' AND AW.InvoiceWriteOffGroupNo='" + invoiceWriteOffGroupNo + "' AND AW.[SourceType]='" + sourceType + @"'
                            Group BY AW.InvoiceWriteOffGroupNo, AW.VoucherDate
                            , P.Code , P.UserName, AW.PostingDate,VT.UserName,AW.AddedBy,AW.UpdatedBy,AW.Narration
                            , AW.DocDate, AW.DocRefNo, C.Code, AW.PartyPlantId, PP.UserName, AW.IsPark, AW.BankJournalId,AW.CurrencyId";
            return _sqlRepository.GetData(cmdText);
        }

        private DataTable GetCustomerBanksReceiptVoucher(string invoiceWriteOffGroupNo)
        {
            try
            {
                var sql = @"SELECT  GL.Id AS AccountCodeId--, VDC.VoucherDetailId
							, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark
							, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END
							, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo--, V.VoucherNo
							, UPPER(V.Narration) AS Narration
                            , V.CurrencyId
							, REPLACE(CONVERT(VARCHAR(11), IV.VoucherDate, 106), ' ', '-') AS VoucherDate
							, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId
							 , VDC.ToCurrencyRate, SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount, SUM(VDC.DrAmount) AS CompanyCurrencyDrAmount, SUM(VDC.CrAmount) AS CompanyCurrencyCrAmount
							, [DRCR]=CASE WHEN SUM(VDC.DrAmount)>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget
                            , Activity= CASE WHEN VD.BankMasterId<>'' THEN ACT.UserName+' - '+ BM.AccountTitle ELSE ACT.UserName END,VD.PartyType
                            FROM 
							 [TRN].[InvoiceWriteOff] AS IV 
                            LEFT JOIN [TRN].[Voucher] AS V  ON IV.VoucherId=V.Id
							LEFT JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherId=V.Id
                            LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            LEFT JOIN [TRN].[InvoiceWriteOffDetail] AS IVD ON IVD.Id=VD.InvoiceWriteOffDetailId
                            
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                            WHERE V.Archive=0 AND IV.InvoiceWriteOffGroupNo='" + invoiceWriteOffGroupNo + @"' 
							GROUP BY  GL.Id 
							, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, V.PostingDate
                            , V.IsPark , v.DocDate, V.DocRefNo, V.Narration
                            , V.CurrencyId, IV.VoucherDate, CU1.Code , V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code 
                            , VDC.FromCurrencyId, VDC.ToCurrencyId
							, VDC.ToCurrencyRate--, VD.DrAmount, VD.CrAmount, VDC.DrAmount, VDC.CrAmount
                            , VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, P.UserName, PP.UserName , VD.Narration, BUD.UserName
                            ,  VD.BankMasterId,ACT.UserName,BM.AccountTitle ,VD.PartyType
							ORDER BY SUM(VD.DrAmount) DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IWorkbook GetCustomerInvoiceReceiptBanksReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string invoiceWriteOffGroupNo, string sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetCustomerBanksReceiptHeader(companyGroupId, companyId, plantId, invoiceWriteOffGroupNo, sourceType);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetCustomerBanksReceiptVoucher(invoiceWriteOffGroupNo);

            var transcationCurrency = header["CurrencyId"].ToString();
            GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            // reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            sheet.Range[row, 1].Text = "Voucher No:";
            sheet.Range[row, 1].CellStyle.Font.Bold = true;
            sheet.Range[row, 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, 1].RowHeight = 24;
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            sheet.Range[row, 2].WrapText = true;
            sheet.Range[row, 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Voucher Date");
            sheet.Range[row, 3].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[row, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
            reportUtility.SetText(ref sheet, row, 4, header["VoucherDate"].ToString());
            sheet.Range[row, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[row, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "DocDate");
            reportUtility.SetText(ref sheet, row, 4, header["DocDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer:");
            reportUtility.SetText(ref sheet, row, 2, header["Customer"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 4, header["DocRefNo"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer Plant");
            reportUtility.SetText(ref sheet, row, 2, header["CustomerPlant"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Status");
            reportUtility.SetText(ref sheet, row, 4, header["Status"].ToString());

            row++;



            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 3, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 3, row, 4].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 3, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 3, row, 4].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 5, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 5, row, 6].Merge();
            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();

                    if (companyCurrencyId != transcationCurrency)
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

                reportUtility.SetText(ref sheet, row, 2, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(3) + 12 + ":" + reportUtility.GetColumnNameForXls(3) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 12 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + 12 + ":" + reportUtility.GetColumnNameForXls(6) + (row - 1) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(3) + 12 + ":" + reportUtility.GetColumnNameForXls(3) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[13, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[13, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && GetPlantIsShowFCInWord(plantId))
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
                sheet[1, 2].ColumnWidth = 60;
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

        private DataTable GetCustomerBanksReceiptVoucherInvoiceDetails(string invoiceWriteOffGroupNo)
        {
            try
            {
                var sql = @"SELECT  GL.Id AS AccountCodeId--, VDC.VoucherDetailId
							, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark
							, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END
							, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo--, V.VoucherNo
							, UPPER(V.Narration) AS Narration
                            , V.CurrencyId
							, REPLACE(CONVERT(VARCHAR(11), IV.VoucherDate, 106), ' ', '-') AS VoucherDate
							, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId
							 , VDC.ToCurrencyRate, SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount, SUM(VDC.DrAmount) AS CompanyCurrencyDrAmount, SUM(VDC.CrAmount) AS CompanyCurrencyCrAmount
							, [DRCR]=CASE WHEN SUM(VDC.DrAmount)>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget
                            , Activity= CASE WHEN VD.BankMasterId<>'' THEN ACT.UserName+' - '+ BM.AccountTitle ELSE ACT.UserName END,VD.PartyType
                            ,CASE WHEN I.DocRefNo IS NOT NULL THEN I.DocRefNo
										WHEN LOAN.DocRefNo IS NOT NULL THEN LOAN.DocRefNo
										ELSE '' END InvoiceNo
                            FROM 
							 [TRN].[InvoiceWriteOff] AS IV 
                            LEFT JOIN [TRN].[Voucher] AS V  ON IV.VoucherId=V.Id
							LEFT JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherId=V.Id
                            LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            LEFT JOIN [TRN].[InvoiceWriteOffDetail] AS IVD ON IVD.Id=VD.InvoiceWriteOffDetailId
                            LEFT JOIN [TRN].[Invoice] AS I ON I.Id=IVD.InvoiceId
                            
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                            LEFT JOIN (select F.DocRefNo,FST.VoucherDetailId from TRN.FinancingSubsequentTransaction FST
												INNER JOIN TRN.Financing F ON F.Id=FST.FinancingId)LOAN ON LOAN.VoucherDetailId=VD.Id
                            WHERE V.Archive=0 AND IV.InvoiceWriteOffGroupNo='" + invoiceWriteOffGroupNo + @"' 
							GROUP BY  GL.Id 
							, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, V.PostingDate
                            , V.IsPark , v.DocDate, V.DocRefNo, V.Narration
                            , V.CurrencyId, IV.VoucherDate, CU1.Code , V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code 
                            , VDC.FromCurrencyId, VDC.ToCurrencyId
							, VDC.ToCurrencyRate--, VD.DrAmount, VD.CrAmount, VDC.DrAmount, VDC.CrAmount
                            , VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, P.UserName, PP.UserName , VD.Narration, BUD.UserName
                            ,  VD.BankMasterId,ACT.UserName,BM.AccountTitle ,VD.PartyType,I.DocRefNo,LOAN.DocRefNo
							ORDER BY SUM(VD.DrAmount) DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IWorkbook GetCustomerInvoiceDetailsReceiptBanksReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string invoiceWriteOffGroupNo, string sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetCustomerBanksReceiptHeader(companyGroupId, companyId, plantId, invoiceWriteOffGroupNo, sourceType);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetCustomerBanksReceiptVoucherInvoiceDetails(invoiceWriteOffGroupNo);

            var transcationCurrency = header["CurrencyId"].ToString();
            GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 4, header["VoucherDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "DocDate");
            reportUtility.SetText(ref sheet, row, 4, header["DocDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer:");
            reportUtility.SetText(ref sheet, row, 2, header["Customer"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 4, header["DocRefNo"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer Plant");
            reportUtility.SetText(ref sheet, row, 2, header["CustomerPlant"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Status");
            reportUtility.SetText(ref sheet, row, 4, header["Status"].ToString());

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
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars"); int colInvoiceNo = xlsCol; xlsCol++;
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();

                    reportUtility.SetText(ref sheet, row, colInvoiceNo, dsLocal.Rows[i]["InvoiceNo"].ToString());
                    //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();

                    if (companyCurrencyId != transcationCurrency)
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

                reportUtility.SetText(ref sheet, row, 2, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 12 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + 12 + ":" + reportUtility.GetColumnNameForXls(6) + (row - 1) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(7) + 12 + ":" + reportUtility.GetColumnNameForXls(7) + (row - 1) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 12 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[13, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[13, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && GetPlantIsShowFCInWord(plantId))
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
                sheet[1, 2].ColumnWidth = 60;
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

        private Dictionary<string, object> GetInventoryReturnPayableHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
            ,AddedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.AddedBy END
            ,PostedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.PostedBy END
            , UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
            , P.UserName AS Party, PP.UserName AS VendorPlant, V.CurrencyId, C.Code AS CurrencyCode
	        ,FY.FiscalYearName
           FROM  [TRN].[Voucher] AS V
			LEFT JOIN TRN.PurchaseReturn PR ON PR.VoucherId=V.Id
            LEFT JOIN [HKP].[Party] AS P ON P.Id=PR.PartyId
            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=PR.InvoicingPartyPlantId
            LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
            LEFT JOIN SEC.[User] U ON U.UserId=V.AddedBy
	        LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
            WHERE V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "'  AND V.Id='" + voucherId + "' AND V.SourceType='" + sourceType + "'" +
            "";
            return _sqlRepository.GetData(cmdText);
        }
        public IWorkbook GetInventoryReturnPayableReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            //    var advanceDataList = GetVendorInvoiceChargeData(companyGroupId, companyId, plantId, voucherId, sourceType);
            //    var dtGeneralVoucher = advanceDataList;

            var header = GetInventoryReturnPayableHeader(companyGroupId, companyId, plantId, voucherId, SourceType.InventoryReturnPayable);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetVendorInvoiceChargeData(companyGroupId, companyId, plantId, voucherId, SourceType.InventoryReturnPayable);

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

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Purchase Return", companyId, plantId, plantName, null);
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
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Debit Note", companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }

        public IWorkbook GetCustomerInvoiceDetailsReceiptBanksIndividualReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string invoiceWriteOffGroupNo, string sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetCustomerBanksReceiptHeaderIndividual(companyGroupId, companyId, plantId, invoiceWriteOffGroupNo, sourceType);
            reportFileName = Convert.ToDateTime(header.Rows[0]["PostingDate"]).ToString("yyMMdd") + " " + header.Rows[0]["VoucherNo"];

            var dsLocal = GetCustomerBanksReceiptVoucherInvoiceDetailsIndividual(invoiceWriteOffGroupNo);

            var transcationCurrency = header.Rows[0]["CurrencyId"].ToString();
            GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            for (int h = 0; h < header.Rows.Count; h++)
            {
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
                sheet.Range[row, 1].ColumnWidth = 15;
                reportUtility.SetText(ref sheet, row, 2, header.Rows[h]["VoucherNo"].ToString());
                sheet.Range[row, 2].ColumnWidth = 30;
                reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Voucher Date");
                reportUtility.SetText(ref sheet, row, 4, header.Rows[h]["VoucherDate"].ToString());
                row++;

                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
                reportUtility.SetText(ref sheet, row, 2, header.Rows[h]["PostingDate"].ToString());
                reportUtility.SetMasterHeaderText(ref sheet, row, 3, "DocDate");
                reportUtility.SetText(ref sheet, row, 4, header.Rows[h]["DocDate"].ToString());
                row++;

                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer:");
                reportUtility.SetText(ref sheet, row, 2, header.Rows[h]["Customer"].ToString());
                reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Doc Ref");
                reportUtility.SetText(ref sheet, row, 4, header.Rows[h]["DocRefNo"].ToString());
                row++;

                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer Plant");
                reportUtility.SetText(ref sheet, row, 2, header.Rows[h]["CustomerPlant"].ToString());
                reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Status");
                reportUtility.SetText(ref sheet, row, 4, header.Rows[h]["Status"].ToString());

                row++;



                colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
                reportUtility.SetText(ref sheet, row, 2, header.Rows[h]["Narration"].ToString());
                sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                row++;

                if (companyCurrencyId == transcationCurrency)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet[row, 4, row, 5].Merge();
                }
                else
                {
                    reportUtility.SetHeaderText(ref sheet, row, 4, header.Rows[h]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                    sheet[row, 4, row, 5].Merge();

                    reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet[row, 6, row, 7].Merge();
                }
                row++;
                xlsCol = 1;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL");
                sheet.Range[row, 1].ColumnWidth = 15;
                colGl = xlsCol; xlsCol++;
                //sheet[row, colGl, row, colGl+1].Merge();
                sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 1) + row].Merge();
                xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars"); int colInvoiceNo = xlsCol; xlsCol++;
                //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;

                if (companyCurrencyId != transcationCurrency)
                {
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight);
                    sheet.Range[row, xlsCol].ColumnWidth = 15;
                    colinrDebit = xlsCol; xlsCol++;
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight);
                    sheet.Range[row, xlsCol].ColumnWidth = 15;
                    colinrCredit = xlsCol; xlsCol++;

                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight);
                    sheet.Range[row, xlsCol].ColumnWidth = 15;
                    colusdDebit = xlsCol; xlsCol++;
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight);
                    sheet.Range[row, xlsCol].ColumnWidth = 15;
                    colusdCradit = xlsCol;
                    colLast = xlsCol;
                }
                else
                {
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight);
                    sheet.Range[row, xlsCol].ColumnWidth = 15;
                    colinrDebit = xlsCol; xlsCol++;
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight);
                    sheet.Range[row, xlsCol].ColumnWidth = 15;
                    colinrCredit = xlsCol;
                    colLast = xlsCol;
                }

                if (dsLocal.Count > 0)
                {
                    double totalTranAmount = 0;
                    double totalBookCurrencyAmount = 0;
                    row++;
                    foreach (var detail in dsLocal.Where(r => r["VoucherId"].ToString() == header.Rows[h]["VoucherId"].ToString()))
                    {
                        var glName = detail["Budget"].ToString();


                        reportUtility.SetText(ref sheet, row, colGl, detail["GLGeneralInfoCode"] + " - " + glName + " - " + detail["Activity"]);

                        sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 1) + row].Merge();

                        reportUtility.SetText(ref sheet, row, colInvoiceNo, detail["InvoiceNo"].ToString());
                        //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();

                        if (companyCurrencyId != transcationCurrency)
                        {
                            reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(detail["DrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(detail["CrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, colusdDebit, Convert.ToDouble(detail["CompanyCurrencyDrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, colusdCradit, Convert.ToDouble(detail["CompanyCurrencyCrAmount"].ToString()));
                            totalTranAmount += Convert.ToDouble(detail["DrAmount"].ToString());
                        }
                        else
                        {
                            reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(detail["CompanyCurrencyDrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(detail["CompanyCurrencyCrAmount"].ToString()));
                        }
                        totalBookCurrencyAmount += Convert.ToDouble(detail["CompanyCurrencyDrAmount"].ToString());

                        sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                        sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                        row++;

                        glName = string.Empty;

                    }

                    reportUtility.SetText(ref sheet, row, 1, "Total: ", true);

                    if (companyCurrencyId != transcationCurrency)
                    {
                        sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                        sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                        sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                        sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 12 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                        sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                        sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                        sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + 12 + ":" + reportUtility.GetColumnNameForXls(6) + (row - 1) + ")";
                        sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                        sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                        sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(7) + 12 + ":" + reportUtility.GetColumnNameForXls(7) + (row - 1) + ")";
                        sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                        sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                    }
                    else
                    {
                        sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                        sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                        sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                        sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 12 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                        sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                        sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                    }

                    sheet.Range[13, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[13, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Hair);

                    row++;
                    reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                    if (companyCurrencyId != transcationCurrency && GetPlantIsShowFCInWord(plantId))
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
                    sheet[1, 2].ColumnWidth = 60;
                    sheet.UsedRange.CellStyle.Font.Size = 8;
                    row += 4;

                }
                else
                {
                    sheet.UsedRange.WrapText = true;
                    sheet.UsedRange.CellStyle.Font.Size = 8;
                    reportUtility.PlantHeader(ref sheet, 5, header.Rows[0]["VoucherTypeName"].ToString(), plantId);
                    //reportUtility.PlantHeader(ref sheet, endCol, "Material Issue Report", identity.PlantId);

                    #region ReportHeader 
                    sheet.UsedRange.WrapText = true;
                    sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.UsedRange.HorizontalAlignment = ExcelHAlign.HAlignLeft;

                    sheet.PageSetup.TopMargin = 0.2;
                    sheet.PageSetup.BottomMargin = 0.8;
                    sheet.PageSetup.LeftMargin = 0.2;
                    sheet.PageSetup.RightMargin = 0.2;
                    sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    sheet.PageSetup.FitToPagesTall = 0;
                    sheet.PageSetup.FitToPagesWide = 1;
                    sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet.PageSetup.CenterHorizontally = true;
                    #endregion


                    reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                }
            }
            reportUtility.SetSignatureText(ref sheet, row - 1, 1, header.Rows[0]["AddedBy"].ToString());
            sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

            reportUtility.SetSignatureText(ref sheet, row - 1, 2, header.Rows[0]["PostedBy"].ToString());
            sheet.Range[row, 2].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, 2, "Checked By", true);

            sheet.Range[row, 4].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, 4, "Authorized By", true);

            reportUtility.CompanyPlantHeader(ref sheet, colLast, header.Rows[0]["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
            reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);


            return workbook;
        }
        private DataTable GetCustomerBanksReceiptHeaderIndividual(string companyGroupId, string companyId, string plantId, string invoiceWriteOffGroupNo, string sourceType)
        {
            var cmdText = @"SELECT AW.InvoiceWriteOffGroupNo,V.Id VoucherId,V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), AW.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , P.Code AS PartyCode, P.UserName AS Customer, REPLACE(CONVERT(VARCHAR(11), AW.PostingDate, 106), ' ', '-') AS PostingDate 
                            , REPLACE(CONVERT(VARCHAR(11), AW.DocDate, 106), ' ', '-') AS DocDate, AW.DocRefNo, C.Code AS CurrencyCode
                            , AW.PartyPlantId, PP.UserName AS CustomerPlant,  AW.BankJournalId, UPPER(AW.Narration) AS Narration
                            , VT.UserName AS VoucherTypeName,AW.AddedBy,AW.UpdatedBy PostedBy, CASE WHEN AW.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            ,AW.CurrencyId
                            
                            FROM [TRN].[InvoiceWriteOff] AS AW
                             
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=AW.VoucherTypeId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=AW.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AW.PartyPlantId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=AW.CurrencyId
							left join [TRN].Voucher V on V.Id=AW.VoucherId
                            WHERE AW.Archive=0 AND AW.CompanyGroupId='" + companyGroupId + "' AND AW.CompanyId='" + companyId + "' AND AW.PlantId='" + plantId + "' AND AW.InvoiceWriteOffGroupNo='" + invoiceWriteOffGroupNo + "' AND AW.[SourceType]='" + sourceType + @"'";
            return _sqlRepository.GetDataTable(cmdText);
        }
        private List<Dictionary<string, object>> GetCustomerBanksReceiptVoucherInvoiceDetailsIndividual(string invoiceWriteOffGroupNo)
        {
            try
            {
                var sql = @"SELECT  GL.Id AS AccountCodeId, VD.VoucherId
							, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark
							, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END
							, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo--, V.VoucherNo
							, UPPER(V.Narration) AS Narration
                            , V.CurrencyId
							, REPLACE(CONVERT(VARCHAR(11), IV.VoucherDate, 106), ' ', '-') AS VoucherDate
							, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId
							 , VDC.ToCurrencyRate,VD.DrAmount,VD.CrAmount,VDC.DrAmount CompanyCurrencyDrAmount,VDC.CrAmount AS CompanyCurrencyCrAmount
							, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget
                            , Activity= CASE WHEN VD.BankMasterId<>'' THEN ACT.UserName+' - '+ BM.AccountTitle ELSE ACT.UserName END,VD.PartyType
                            ,CASE WHEN I.DocRefNo IS NOT NULL THEN I.DocRefNo
							        WHEN LOAN.DocRefNo IS NOT NULL THEN LOAN.DocRefNo
							        ELSE '' END InvoiceNo
                            FROM 
							 [TRN].[InvoiceWriteOff] AS IV 
                            LEFT JOIN [TRN].[Voucher] AS V  ON IV.VoucherId=V.Id
							LEFT JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherId=V.Id
                            LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            LEFT JOIN [TRN].[InvoiceWriteOffDetail] AS IVD ON IVD.Id=VD.InvoiceWriteOffDetailId
                            LEFT JOIN [TRN].[Invoice] AS I ON I.Id=IVD.InvoiceId
                            
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                            LEFT JOIN (select F.DocRefNo,FST.VoucherDetailId from TRN.FinancingSubsequentTransaction FST
									    INNER JOIN TRN.Financing F ON F.Id=FST.FinancingId)LOAN ON LOAN.VoucherDetailId=VD.Id
                            WHERE V.Archive=0 AND IV.InvoiceWriteOffGroupNo='" + invoiceWriteOffGroupNo + @"'  
							ORDER BY VD.VoucherId,VD.DrAmount DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Dictionary<string, object> GetCustomerAdvanceGroupHeader(string companyGroupId, string companyId, string plantId, string invoiceWriteOffGroupNo, string sourceType)
        {
            var cmdText = @"SELECT VoucherNo=STUFF((SELECT DISTINCT ','+xpo.VoucherNo from
                                    			[TRN].Voucher xpo
                                    			INNER JOin trn.[Advance] xPDAMAP on xpo.Id=xPDAMAP.VoucherId
                                    			WHERE A.AdvanceGroupNo=xPDAMAP.AdvanceGroupNo for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
												, REPLACE(CONVERT(VARCHAR(11), A.VoucherDate, 106), ' ', '-') AS VoucherDate,NULL  Id, NULL AdvanceId,NULL BankChargeId, A.PartyId, P.Code AS PartyCode, P.UserName AS Customer, A.PartyPlantId, PP.UserName AS CustomerPlant, A.EmployeeId, EI.EmployeeCode
                                 , EI.EmployeeName, EIR.EmployeeCode AS ResponsibleCode,EIR.EmployeeName AS ResponsibleName, NULL VoucherId, A.PostingDate, A.DocDate, A.DocRefNo
                                 , A.CurrencyId, C.Code AS CurrencyCode, SUM(A.Amount) Amount, A.IsWrittenOff, SUM(A.WrittenOffAmount) WrittenOffAmount, A.IsPark, UPPER(A.Narration) AS Narration
								 , A.IsInterTransaction, A.IsPosted, SUM(AD.NetAmount) NetAmount
                                 , Status = case when A.IsPark = 0 then 'Posted' else 'Parked' end,A.AdvanceGroupNo
                                , VT.UserName AS VoucherTypeName,A.AddedBy,A.UpdatedBy PostedBy
                                 FROM [TRN].[Advance] AS A
                                 LEFT JOIN [HKP].[Party] AS P ON P.Id=A.PartyId
                                 LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=A.PartyPlantId
                                 LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=A.EmployeeId
                                 LEFT JOIN [dbo].[EmployeeInformation] AS EIR ON EIR.SystemId=A.ResponsiblePersonId
                                 LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                                 LEFT JOIN [TRN].[Voucher] AS V ON V.Id=A.VoucherId
                                 LEFT JOIN [TRN].[BankCharge] AS BC ON BC.AdvanceId=A.Id
                                LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=A.VoucherTypeId
                                LEFT JOIN (SELECT AdvanceId, PartyId, NetAmount FROM [TRN].[AdvanceDetail]
                                ) AS AD ON AD.AdvanceId=A.Id AND AD.PartyId=A.PartyId
                                WHERE A.OpeningBalanceId IS NULL AND A.Archive=0 AND V.Archive=0 
								AND  A.CompanyId='" + companyId + "' AND A.PlantId='" + plantId + @"' AND A.SourceType='CustomerAdvance' and A.AdvanceGroupNo='" + invoiceWriteOffGroupNo + @"'
								Group By A.PartyId, P.Code, P.UserName, A.PartyPlantId, PP.UserName, A.EmployeeId, EI.EmployeeCode
                                 , EI.EmployeeName, EIR.EmployeeCode,EIR.EmployeeName, A.PostingDate, A.DocDate, A.DocRefNo
                                 , A.CurrencyId, C.Code  , A.IsWrittenOff,A.AdvanceGroupNo,A.IsPark , A.IsInterTransaction, A.IsPosted,A.VoucherDate,A.Narration,VT.UserName,A.AddedBy,A.UpdatedBy";
            return _sqlRepository.GetData(cmdText);
        }
        private DataTable GetCustomerAdvanceGroupDetails(string advanceGroupNo)
        {
            try
            {
                var sql = @"SELECT  GL.Id AS AccountCodeId--, VDC.VoucherDetailId
							, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark
							, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END
							, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo--, V.VoucherNo
							, UPPER(V.Narration) AS Narration
                            , V.CurrencyId
							, REPLACE(CONVERT(VARCHAR(11), IV.VoucherDate, 106), ' ', '-') AS VoucherDate
							, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId
							 , VDC.ToCurrencyRate, SUM(VD.DrAmount) AS DrAmount, SUM(VD.CrAmount) AS CrAmount, SUM(VDC.DrAmount) AS CompanyCurrencyDrAmount, SUM(VDC.CrAmount) AS CompanyCurrencyCrAmount
							, [DRCR]=CASE WHEN SUM(VDC.DrAmount)>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget
                            , Activity= CASE WHEN VD.BankMasterId<>'' THEN ACT.UserName+' - '+ BM.AccountTitle ELSE ACT.UserName END,VD.PartyType
                             ,CASE WHEN LOAN.DocRefNo IS NOT NULL THEN LOAN.DocRefNo
										WHEN VD.BankMasterId<>'' THEN BM.AccountNumber
										ELSE '' END InvoiceNo
                            FROM 
							 [TRN].[Advance] AS IV 
                            LEFT JOIN [TRN].[Voucher] AS V  ON IV.VoucherId=V.Id
							LEFT JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherId=V.Id
                            LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            LEFT JOIN [TRN].[AdvanceDetail] AS IVD ON IVD.Id=VD.AdvanceDetailId
                            
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                            LEFT JOIN (select F.DocRefNo,FST.VoucherDetailId from TRN.FinancingSubsequentTransaction FST
												INNER JOIN TRN.Financing F ON F.Id=FST.FinancingId)LOAN ON LOAN.VoucherDetailId=VD.Id
                            WHERE V.Archive=0 AND IV.AdvanceGroupNo='" + advanceGroupNo + @"' 
							GROUP BY  GL.Id 
							, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, V.PostingDate
                            , V.IsPark , v.DocDate, V.DocRefNo, V.Narration
                            , V.CurrencyId, IV.VoucherDate, CU1.Code , V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code 
                            , VDC.FromCurrencyId, VDC.ToCurrencyId,BM.AccountNumber
							, VDC.ToCurrencyRate--, VD.DrAmount, VD.CrAmount, VDC.DrAmount, VDC.CrAmount
                            , VD.GLGeneralInfoId, GL.UserName, GL.AccountCode, P.UserName, PP.UserName , VD.Narration, BUD.UserName
                            ,  VD.BankMasterId,ACT.UserName,BM.AccountTitle ,VD.PartyType,IV.DocRefNo,LOAN.DocRefNo
							ORDER BY SUM(VD.DrAmount) DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IWorkbook GetCustomerAdvanceGroupReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string advanceGroupNo, string sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetCustomerAdvanceGroupHeader(companyGroupId, companyId, plantId, advanceGroupNo, sourceType);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetCustomerAdvanceGroupDetails(advanceGroupNo);

            var transcationCurrency = header["CurrencyId"].ToString();
            GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 4, header["VoucherDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "DocDate");
            reportUtility.SetText(ref sheet, row, 4, header["DocDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer:");
            reportUtility.SetText(ref sheet, row, 2, header["Customer"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 4, header["DocRefNo"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer Plant");
            reportUtility.SetText(ref sheet, row, 2, header["CustomerPlant"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Status");
            reportUtility.SetText(ref sheet, row, 4, header["Status"].ToString());

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
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars"); int colInvoiceNo = xlsCol; xlsCol++;
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();

                    reportUtility.SetText(ref sheet, row, colInvoiceNo, dsLocal.Rows[i]["InvoiceNo"].ToString());
                    //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();

                    if (companyCurrencyId != transcationCurrency)
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

                reportUtility.SetText(ref sheet, row, 2, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 12 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + 12 + ":" + reportUtility.GetColumnNameForXls(6) + (row - 1) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(7) + 12 + ":" + reportUtility.GetColumnNameForXls(7) + (row - 1) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 12 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[13, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[13, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && GetPlantIsShowFCInWord(plantId))
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
                sheet[1, 2].ColumnWidth = 60;
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


        #region vendor charge set-off



        //old vendor invoice charge set-off data
        private DataTable GetVendorInvoiceChargeData(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                            , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS DrAmount, VD.CrAmount AS CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget

                            ,Activity=CASE WHEN VD.CashMasterId<>'' THEN  CM.UserName  WHEN VD.BankMasterId<>'' THEN BNM.AccountTitle Else ACT.UserName end 
                            ,CM.UserName AS CashMasterName
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [TRN].[InvoiceWriteOffDetail] AS IVD ON IVD.Id=VD.InvoiceWriteOffDetailId
                            LEFT JOIN [TRN].[InvoiceWriteOff] AS IV ON IV.Id=IVD.InvoiceWriteOffId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [MST].[BankMaster] AS BNM ON BNM.Id=VD.BankMasterId
                            WHERE V.Archive=0 AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(cmdText);
        }

        //vendor invoice header data old & NEW
        private Dictionary<string, object> GetVendorInvoiceHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
            ,AddedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.AddedBy END
            ,PostedBy=CASE WHEN UP.FullName<>'' THEN UP.FullName ELSE V.PostedBy END
            , UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
            , P.UserName AS Vendor, PP.UserName AS VendorPlant, BJ.CurrencyId, C.Code AS CurrencyCode
            FROM [TRN].[InvoiceWriteOff] AS BJ
            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=BJ.VoucherId
            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
            LEFT JOIN [HKP].[Party] AS P ON P.Id=BJ.PartyId
            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=BJ.PartyPlantId
            LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
            LEFT JOIN SEC.[User] U ON U.UserId=V.AddedBy
            LEFT JOIN SEC.[User] UP ON UP.UserId=V.PostedBy
                            WHERE BJ.Archive=0 AND BJ.CompanyGroupId='" + companyGroupId + "' AND BJ.CompanyId='" + companyId + "' AND BJ.PlantId='" + plantId + "' AND BJ.VoucherId='" + voucherId + "' AND BJ.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }




        public IWorkbook GetVendorInvoiceChargeReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            //    var advanceDataList = GetVendorInvoiceChargeData(companyGroupId, companyId, plantId, voucherId, sourceType);
            //    var dtGeneralVoucher = advanceDataList;

            var header = GetVendorInvoiceHeader(companyGroupId, companyId, plantId, voucherId, SourceType.VendorInvoiceCharge);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetVendorInvoiceChargeData(companyGroupId, companyId, plantId, voucherId, SourceType.VendorInvoiceCharge);

            var transcationCurrency = header["CurrencyId"].ToString();
            GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);


            var row = 5;
            var colLast = 1;
            int xlsCol = 1;
            int colGl = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());

            //reportUtility.SetMasterHeaderText(ref sheet, row, middleColumnCaption, "");
            //sheet[row, 3].ColumnWidth = 25;
            //reportUtility.SetText(ref sheet, row, middleColumnCaption, header[""].ToString());

            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString());
            sheet[row, 4].ColumnWidth = 15;
            sheet[row, 5].ColumnWidth = 15;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor:");
            reportUtility.SetText(ref sheet, row, 2, header["Vendor"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor Plant");
            reportUtility.SetText(ref sheet, row, 2, header["VendorPlant"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Status");
            reportUtility.SetText(ref sheet, row, 5, header["Status"].ToString());
            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet[row, 2].ColumnWidth = 30;

            row++;  //10

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
            sheet[row, 6].ColumnWidth = 15;
            //sheet[row, 6].RowHeight = 15;
            sheet[row, 7].ColumnWidth = 15;
            sheet.Range[row, 4, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, 4, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;


            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            xlsCol++; xlsCol++;


            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;


                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;

                //sheet.Range[row, 4, row, colLast].BorderAround(ExcelLineStyle.Thin);
                //sheet.Range[row, 4, row, colLast].BorderInside(ExcelLineStyle.Thin);

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, colGl, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            else
            {

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;

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

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 2) + row].Merge();

                    if (companyCurrencyId != transcationCurrency)
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

                    glName = string.Empty;

                    row++;
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, 3, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                    //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                    //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                    //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (formulaEndRow) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[row, colinrDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, colinrDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && GetPlantIsShowFCInWord(plantId))
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

                //sheet.UsedRange.AutofitColumns();
                //sheet[1, 2].ColumnWidth = 60;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);
                sheet[row, 1].ColumnWidth = 25;

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);
                sheet[row, 3].ColumnWidth = 25;

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Vendor Invoice Charge Set-Off", companyId, plantId, plantName, null);
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
                reportUtility.CompanyPlantHeader(ref sheet, 7, "Vendor Invoice Charge Set-Off", companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }

        public IEnumerable<object> GetExpenseDistributionSql(string fromDate, string toDate)
        {
            try
            {
                var _sql = @"select   InvoceNo,InvoiceDate,Customer,CustomerPlant 
                            ,Activity ,ISNULL(DistributedAmount,0) DistributedAmount,ISNULL(DistributedAmount,0) GrossTotal --,CompanyCurrencyDrAmount,DrAmount
                        into #tempOT from
                        (
                            SELECT GL.Id AS AccountCodeId, VD.DrAmount AS DrAmount, VD.CrAmount AS CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant
							,PV.UserName Vendor,PPV.UserName VendorPlant,  BUD.UserName AS Budget
                            ,Activity=ISNULL(ACT.UserName,'')
                            ,ID.InvoiceType,ISNULL(ID.DistributedAmount,0) DistributedAmount,IV.Id AS InvoiceId,IV.DocRefNo InvoceNo,ID.Amount,V.VoucherNo,V.DocRefNo,V.DocDate InvoiceDate,V.PostingDate,vd.VoucherId
                             FROM TRN.InvoiceDetailCharges ID
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=ID.VoucherDetailId
							JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherDetailId=ID.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                             JOIN [TRN].[Invoice] AS IV ON IV.Id=ID.InvoiceId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
							LEFT JOIN [TRN].[Invoice] AS VIV ON VIV.VoucherId=V.Id
							LEFT JOIN [HKP].[Party] AS PV ON PV.Id=VIV.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PPV ON PPV.Id=VIV.PartyPlantId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [MST].[BankMaster] AS BNM ON BNM.Id=VD.BankMasterId
                            WHERE V.Archive=0  AND ID.InvoiceType='OutboundInvoice' --and id.VoucherDetailId='2022299640001'
							AND convert(Date,V.PostingDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'
							--AND V.Id='202319446'
							--ORDER BY VD.DrAmount DESC
							)B
							DECLARE @sql nvarchar(max), @col nvarchar(max)
                            SELECT @col = (
                                SELECT DISTINCT ','+QUOTENAME(REPLACE(CONVERT(VARCHAR(40), Activity, 113), '-', '-'))
                                FROM #tempOT 
                                FOR XML PATH ('')
                            ) 
							SELECT @sql = N'
                            (SELECT *
                            FROM #tempOT
                            PIVOT (
                                MAX([DistributedAmount]) FOR [Activity] IN ('+STUFF(@col,1,1,'')+')
                            ) as pvt)' 
                            EXEC sp_executesql @sql
                            drop table #tempOT";

                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        #endregion vendor charge set-off


        #region auto mail
        private DataTable GetAutoMailReportData(string companyGroupId, string companyId, string plantId)
        {
            var sql = @"select * from (
	                                 SELECT IVD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, IVD.BudgetMasterId, B.UserName AS BudgetName
									 , IVD.ActivityId, EN.UserName AS EntityName, A.UserName AS ActivityName,V.VoucherNo, format(V.VoucherDate,'dd-MM-yyyy') EntryDate   --Replace(Convert(varchar(11), V.VoucherDate, 106), ' ', '-') EntryDate 
									 , Replace(CONVERT(VARCHAR(11), IV.DocDate, 106), ' ', '-') DocDate ,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate, IV.DocRefNo, IV.Narration,VD.EntityId
									 ,VD.PlantId, IVD.Id AS InvoiceDetailId, IV.VoucherId, VD.Id AS VoucherDetailId, IV.CurrencyId ,v.SourceType
									 , ParticularName= case when iv.PartyId<>'' then  PP.UserName else '' end
	                                , Type= case when iv.PartyId<>'' then  'Vendor' else '' end
									 , C.Code AS CurrencyCode,  IVD.NetAmount AS Payable, IVD.WrittenOffAmount AS Payment, IVD.NetAmount-IVD.WrittenOffAmount AS Balance, CC.CompanyCurrencyId
									 , CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion,GC.CompanyGroupCurrencyId
									 , GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion,HC.HardCurrencyId, HC.HardFromCurrencyId
									 , HC.HardCurrencyRate, HC.HardCurrencyConversion , NULL GRNNo, null GRNDate, Details=REPLACE(REPLACE(
										STUFF((SELECT DISTINCT ','+xpo.UserName from
											hkp.Activity xpo
											INNER JOin TRN.VoucherDetail xPDAMAP on xpo.id=xPDAMAP.ActivityId
											WHERE VD.ActivityId!=xPDAMAP.ActivityId and xPDAMAP.VoucherId=V.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
										,IVD.NetAmount*CC.CompanyCurrencyRate PayableBooks

										--IV.PartyPlantId, PP.UserName AS PartyPlantName,
                                        FROM [TRN].[InvoiceDetail] AS IVD
                                        LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId=IV.Id
									    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IVD.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=IVD.GLGeneralInfoId
										LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=IVD.BudgetMasterId
										LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
										LEFT JOIN [HKP].[Activity] AS A ON A.Id=IVD.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
									LEFT JOIN (
									SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.DrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS GC ON GC.VoucherDetailId=VD.Id
									LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS HardCurrencyId, VDC.FromCurrencyId AS HardFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS HardCurrencyRate, VDC.ToCurrencyConversion AS HardCurrencyConversion, VDC.DrAmount AS HardCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS HC ON HC.VoucherDetailId=VD.Id 
                                        WHERE IV.Archive=0 AND IV.IsWrittenOff=0 AND IVD.IsWrittenOff=0  AND V.IsPark=0 AND IVD.IsBlock=0 AND IV.SourceType in ('VendorInvoice','PurchaseDocAcceptance','SuspensePayable','EmployeePayable')
                                        AND IV.CompanyGroupId='" + companyGroupId + "' AND IV.CompanyId='" + companyId + @"' 
                                        and DATEDIFF(DAY, GETDATE(),V.VoucherDate) >-10

                                    UNION ALL
                                    SELECT IVD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, IVD.BudgetMasterId, B.UserName AS BudgetName
									, IVD.ActivityId, EN.UserName AS EntityName, A.UserName AS ActivityName,V.VoucherNo, Replace(Convert(varchar(11), V.VoucherDate, 106), ' ', '-') EntryDate
									, Replace(CONVERT(VARCHAR(11), IV.DocDate, 106), ' ', '-') DocDate ,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate, IV.DocRefNo, IV.Narration, VD.EntityId
									,VD.PlantId, IVD.Id AS InvoiceDetailId, IV.VoucherId,VD.Id AS VoucherDetailId, IV.CurrencyId ,v.SourceType
									, ParticularName= case when iv.PartyId<>'' then  PP.UserName else '' end
	                                , Type= case when iv.PartyId<>'' then  'Vendor' else '' end
									, C.Code AS CurrencyCode,  IVD.NetAmount AS Payable, IVD.WrittenOffAmount AS Payment, IVD.NetAmount-IVD.WrittenOffAmount AS Balance, CC.CompanyCurrencyId
									, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion,GC.CompanyGroupCurrencyId
									, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion,HC.HardCurrencyId, HC.HardFromCurrencyId
									, HC.HardCurrencyRate, HC.HardCurrencyConversion,IR.Id GRNNo,Replace(Convert(varchar(11), IR.GRNDate, 106), ' ', '-') GRNDate,   Details=REPLACE(REPLACE(
										STUFF((SELECT DISTINCT ','+xpo.UserName from
											hkp.Activity xpo
											INNER JOin TRN.VoucherDetail xPDAMAP on xpo.id=xPDAMAP.ActivityId
											WHERE VD.ActivityId!=xPDAMAP.ActivityId and xPDAMAP.VoucherId=V.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
										,IVD.NetAmount*CC.CompanyCurrencyRate PayableBooks

										--IV.PartyPlantId, PP.UserName AS PartyPlantName,

                                        FROM [TRN].[InvoiceDetail] AS IVD
                                        LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId=IV.Id
									    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IVD.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=IVD.GLGeneralInfoId
										LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=IVD.BudgetMasterId
										LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
										LEFT JOIN [HKP].[Activity] AS A ON A.Id=IVD.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId
                                        LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
									LEFT JOIN (
									SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.DrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS GC ON GC.VoucherDetailId=VD.Id
									LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS HardCurrencyId, VDC.FromCurrencyId AS HardFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS HardCurrencyRate, VDC.ToCurrencyConversion AS HardCurrencyConversion, VDC.DrAmount AS HardCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS HC ON HC.VoucherDetailId=VD.Id 
                                        WHERE IV.Archive=0 AND IV.IsWrittenOff=0 AND IVD.IsWrittenOff=0  AND V.IsPark=0 AND IVD.IsBlock=0 AND IV.SourceType in ('InventoryPayable')
                                        AND IV.CompanyGroupId='" + companyGroupId + "' AND IV.CompanyId='" + companyId + @"'  
			                            and DATEDIFF(DAY, GETDATE(),V.VoucherDate) >-10

                                        AND IR.PurchaseDocumentAcceptanceId IS NULL

										Union all
                                SELECT EPD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, EPD.BudgetMasterId, B.UserName AS BudgetName
								, EPD.ActivityId,  E.UserName AS EntityName, A.UserName AS ActivityName, V.VoucherNo,Replace(Convert(varchar(11), V.VoucherDate, 106), ' ', '-') EntryDate
								, Replace(CONVERT(VARCHAR(11), EP.DocDate, 106), ' ', '-') DocDate,Replace(CONVERT(VARCHAR(11), EP.PostingDate, 106), ' ', '-') PostingDate,EP.DocRefNo, EP.Narration, VD.EntityId
								, VD.PlantId,VD.Id AS VoucherDetailId, EP.VoucherId,  VD.Id AS VoucherDetailId, EP.CurrencyId,v.SourceType
								, ParticularName= case when ep.EmployeeId<>'' then empi.EmployeeCode+' - '+ EMPI.EmployeeName else '' end
	                        	 , Type= case when ep.EmployeeId<>''  then  'Employee' else '' end
								, C.Code AS CurrencyCode,  EPD.NetAmount AS Payable,EPD.WrittenOffAmount AS Payment, EPD.NetAmount-EPD.WrittenOffAmount AS Balance,
										CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion,
										GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion,
										HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion
                                        ,IR.Id GRNNo, Replace(Convert(varchar(11), IR.GRNDate, 106), ' ', '-') GRNDate, Details=REPLACE(REPLACE(
										STUFF((SELECT DISTINCT ','+xpo.UserName from
											hkp.Activity xpo
											INNER JOin TRN.VoucherDetail xPDAMAP on xpo.id=xPDAMAP.ActivityId
											WHERE VD.ActivityId!=xPDAMAP.ActivityId and xPDAMAP.VoucherId=V.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
										,EPD.NetAmount*CC.CompanyCurrencyRate PayableBooks
                                        FROM [TRN].[EmployeePayableDetail] AS EPD
                                        LEFT JOIN [TRN].[EmployeePayable] AS EP ON EPD.EmployeePayableId=EP.Id
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.EmployeePayableDetailId=EPD.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=EPD.GLGeneralInfoId
										LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=EPD.BudgetMasterId
										LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
										LEFT JOIN [HKP].[Activity] AS A ON A.Id=EPD.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=EP.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS E ON E.Id=VD.EntityId
									    LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
	                                    left join dbo.EmployeeInformation EMPI ON EMPI.SystemId=EP.EmployeeId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
									LEFT JOIN (
									SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.DrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS GC ON GC.VoucherDetailId=VD.Id
									LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS HardCurrencyId, VDC.FromCurrencyId AS HardFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS HardCurrencyRate, VDC.ToCurrencyConversion AS HardCurrencyConversion, VDC.DrAmount AS HardCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS HC ON HC.VoucherDetailId=VD.Id
                                        WHERE EP.Archive=0 AND EP.IsPark=0 AND EP.IsWrittenOff=0 AND EPD.IsWrittenOff=0 AND EPD.IsBlock=0 AND EP.SourceType IN ('EmployeePayable','SalaryPayable','InventoryPayable')
                                        AND EP.CompanyGroupId='" + companyGroupId + "' AND EP.CompanyId='" + companyId + "' and DATEDIFF(DAY, GETDATE(),V.VoucherDate) >-10 AND EP.PlantId='" + plantId + @"' AND (EPD.NetAmount-EPD.WrittenOffAmount)>0 
                                        ) x
										order by x.EntryDate desc  -- AND EP.EmployeeId='1800165'  ";

            return _sqlRepository.GetDataTable(sql);
        }

        public IWorkbook GetAutoMailReport(string CompanyGroupId, string CompanyId, string PlantId)  //, bool checkbox
        {
            // var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            DataTable dtAutoMailReportList = GetAutoMailReportData(CompanyGroupId, CompanyId, PlantId);
            DataTable dtCompanyCurrency = _sqlRepository.GetDataTable(@"select CR.* from org.Company c
                                                        inner join scs.Currency CR ON CR.Id=c.BaseCurrencyId
                                                        where C.Id='" + CompanyId + "'");

            if (dtAutoMailReportList.Rows.Count == 0)
                throw new Exception("No data found");

            worksheet.Name = "DateRangeWisePayableList";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            worksheet[ROW, COL].Text = "SL. No";
            int colSLNO = COL;
            worksheet[ROW, COL].ColumnWidth = 5;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Vendor/Employee";
            int colPartyPlantName = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Type";
            int colType = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Voucher No";
            int colVoucherNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Posting Date";
            int colPostingDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            COL++;

            worksheet[ROW, COL].Text = "Entry Date";
            int colVoucherDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            COL++;

            worksheet[ROW, COL].Text = "DocRef No";
            int colDocRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Doc Date";
            int colDocDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            COL++;

            worksheet[ROW, COL].Text = "GRN No.";
            int colGRNNo = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GRN Date.";
            int colGRNDate = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            COL++;

            worksheet[ROW, COL].Text = "Tran. Currency";
            int colCurrencyCode = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Tran. Payable";
            int colPayable = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Payable " + '(' + dtCompanyCurrency.Rows[0]["Code"].ToString() + ')';
            int colBooksPayable = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Narration";
            int colNarration = COL;
            worksheet[ROW, COL].ColumnWidth = 60;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            // sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Black;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            ROW++;

            for (int i = 0; i < dtAutoMailReportList.Rows.Count; i++)
            {
                worksheet[ROW, colSLNO].Number = (i + 1);

                worksheet[ROW, colGRNNo].Text = dtAutoMailReportList.Rows[i]["GRNNo"].ToString();
                worksheet[ROW, colVoucherNo].Text = dtAutoMailReportList.Rows[i]["VoucherNo"].ToString();
                worksheet[ROW, colDocRefNo].Text = dtAutoMailReportList.Rows[i]["DocRefNo"].ToString();

                //worksheet[ROW, colDocDate].Text = dtAutoMailReportList.Rows[i]["DocDate"].ToString();

                worksheet[ROW, colDocDate].DateTime = Convert.ToDateTime(dtAutoMailReportList.Rows[i]["DocDate"].ToString());
                worksheet[ROW, colDocDate].NumberFormat = "dd-MMM-yyyy";
                // worksheet.Range[ROW, colDocDate].NumberFormat = "hh:mm AM/PM";
                //sheet1.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                //sheet1.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dvBioDvAC[i]["InTimeShow"].ToString());

                worksheet[ROW, colVoucherDate].DateTime = Convert.ToDateTime(dtAutoMailReportList.Rows[i]["EntryDate"].ToString());
                worksheet[ROW, colVoucherDate].NumberFormat = "dd-MMM-yyyy";
                worksheet[ROW, colPostingDate].DateTime = Convert.ToDateTime(dtAutoMailReportList.Rows[i]["PostingDate"].ToString());
                worksheet[ROW, colPostingDate].NumberFormat = "dd-MMM-yyyy";

                worksheet[ROW, colPayable].Number = clsStaticInfo.dbl(dtAutoMailReportList.Rows[i]["Payable"].ToString());
                worksheet[ROW, colPayable].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colNarration].Text = dtAutoMailReportList.Rows[i]["Narration"].ToString();
                if (dtAutoMailReportList.Rows[i]["GRNDate"].ToString() != "")
                {
                    worksheet[ROW, colGRNDate].DateTime = Convert.ToDateTime(dtAutoMailReportList.Rows[i]["GRNDate"].ToString());
                    worksheet[ROW, colGRNDate].NumberFormat = "dd-MMM-yyyy";
                }
                else
                {
                    worksheet[ROW, colGRNDate].Text = dtAutoMailReportList.Rows[i]["GRNDate"].ToString();

                }

                worksheet[ROW, colCurrencyCode].Text = dtAutoMailReportList.Rows[i]["CurrencyCode"].ToString();
                worksheet[ROW, colPartyPlantName].Text = dtAutoMailReportList.Rows[i]["ParticularName"].ToString();
                worksheet[ROW, colType].Text = dtAutoMailReportList.Rows[i]["Type"].ToString();
                worksheet[ROW, colBooksPayable].Number = clsStaticInfo.dbl(dtAutoMailReportList.Rows[i]["PayableBooks"].ToString());
                worksheet[ROW, colBooksPayable].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;



            ReportUtility reportUtility = new ReportUtility();

            reportUtility.PlantHeader(ref worksheet, endCol, "Last 10 Days Creation Payable List", PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes



            return workbook;
        }

        private DataTable GetAutoMailVpaymentReportData(string companyGroupId, string companyId, string plantId)
        {
            var sql = @"select V.VoucherNo,V.SourceType,BM.AccountTitle UserName,VD.DrAmount
                ,VD.CrAmount TranPaymentAmount,IR.Id GRNNo,Replace(Convert(varchar(11), IR.GRNDate, 106), ' ', '-') GRNDate,V.Narration

                ,V.DocRefNo, Replace(Convert(Varchar(11), V.DocDate,106),'','-') DocDate, Replace(Convert(Varchar(11), V.VoucherDate,106),'','-') EntryDate,Replace(Convert( Varchar(11),V.PostingDate,106),'','-') PostingDate, c.Code CurrencyCode
				 ,ParticularName =concat(STUFF((select distinct ','+xp.UserName from TRN.VoucherDetail XVD JOIN hkp.Party AS XP ON XP.Id=XVD.PartyId
                 where XVD.VoucherId=V.Id AND XVD.PartyId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

				 --empi.EmployeeCode+' - '+ 
                ,STUFF((select distinct ','+xp.EmployeeCode+ '- ' +xp.EmployeeName from TRN.VoucherDetail XVD JOIN dbo.EmployeeInformation AS XP ON XP.SystemId=XVD.EmployeeId
                where XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))

				 ,[Type] =concat(STUFF((select distinct ','+'Vendor' from TRN.VoucherDetail XVD JOIN hkp.Party AS XP ON XP.Id=XVD.PartyId
                where XVD.VoucherId=V.Id AND XVD.PartyId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                ,STUFF((select distinct ','+'Employee' from TRN.VoucherDetail XVD JOIN dbo.EmployeeInformation AS XP ON XP.SystemId=XVD.EmployeeId
                where XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))

					
				    ,isnull(VD.CrAmount,0) * isnull(vdc.ToCurrencyRate,0) BooksPayment
			   		--,IVD.NetAmount*CC.CompanyCurrencyRate PayableBooks
                from
                TRN.VoucherDetail VD
                LEFT JOIN TRN.VoucherDetailCurrency Vdc ON Vdc.VoucherDetailId=VD.Id
                LEFT JOIN TRN.Voucher V ON V.Id=VD.VoucherId
                LEFT JOIN MST.BankMaster BM ON BM.Id=VD.BankMasterId
                LEFT JOIN TRN.VoucherDetail XVD ON XVD.VoucherId=V.Id AND XVD.BankMasterId<>'' AND XVD.DrAmount>0
                LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId

                WHERE VD.BankMasterId<>'' AND XVD.BankMasterId IS NULL AND VD.CrAmount>0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + @"' 
			    and DATEDIFF(DAY, GETDATE(),V.VoucherDate) >-10
                
				union all



                select V.VoucherNo,V.SourceType,BM.UserName,VD.DrAmount
                ,VD.CrAmount TranPaymentAmount,IR.Id GRNNo,Replace(Convert(varchar(11), IR.GRNDate, 106), ' ', '-') GRNDate,V.Narration

                ,V.DocRefNo, Replace(Convert(Varchar(11), V.DocDate,106),'','-') DocDate, Replace(Convert(Varchar(11), V.VoucherDate,106),'','-') EntryDate,Replace(Convert( Varchar(11),V.PostingDate,106),'','-') PostingDate, c.Code CurrencyCode
				 ,ParticularName =concat(STUFF((select distinct ','+xp.UserName from TRN.VoucherDetail XVD JOIN hkp.Party AS XP ON XP.Id=XVD.PartyId
                where XVD.VoucherId=V.Id AND XVD.PartyId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                ,STUFF((select distinct ','+xp.EmployeeCode+ '- ' +xp.EmployeeName from TRN.VoucherDetail XVD JOIN dbo.EmployeeInformation AS XP ON XP.SystemId=XVD.EmployeeId
                where XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
				 
				 ,[Type] =concat(STUFF((select distinct ','+'Vendor' from TRN.VoucherDetail XVD JOIN hkp.Party AS XP ON XP.Id=XVD.PartyId
                where XVD.VoucherId=V.Id AND XVD.PartyId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                ,STUFF((select distinct ','+'Employee' from TRN.VoucherDetail XVD JOIN dbo.EmployeeInformation AS XP ON XP.SystemId=XVD.EmployeeId
                where XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))

		
			  ,VD.CrAmount * vdc.ToCurrencyRate BooksPayment
                from
                TRN.VoucherDetail VD
                LEFT JOIN TRN.VoucherDetailCurrency Vdc ON Vdc.VoucherDetailId=VD.Id
                LEFT JOIN TRN.Voucher V ON V.Id=VD.VoucherId
                LEFT JOIN MST.CashMaster BM ON BM.Id=VD.CashMasterId
                LEFT JOIN TRN.VoucherDetail XVD ON XVD.VoucherId=V.Id AND XVD.CashMasterId<>'' AND XVD.DrAmount>0
                LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                WHERE VD.CashMasterId<>'' AND XVD.CashMasterId IS NULL AND VD.CrAmount>0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + @"' 
			    and DATEDIFF(DAY, GETDATE(),V.VoucherDate) >-10";

            return _sqlRepository.GetDataTable(sql);
        }

        public IWorkbook GetAutoMailVPaymentReport(string CompanyGroupId, string CompanyId, string PlantId)  //, bool checkbox
        {
            // var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            DataTable dtAutoMailReportList = GetAutoMailVpaymentReportData(CompanyGroupId, CompanyId, PlantId);

            DataTable dtCompanyCurrency = _sqlRepository.GetDataTable(@"select CR.* from org.Company c
                                                        inner join scs.Currency CR ON CR.Id=c.BaseCurrencyId
                                                        where C.Id='" + CompanyId + "'");

            if (dtAutoMailReportList.Rows.Count == 0)
                throw new Exception("No data found");

            worksheet.Name = "DateRangeWisePaymentList";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            worksheet[ROW, COL].Text = "SL. No";
            int colSLNO = COL;
            worksheet[ROW, COL].ColumnWidth = 5;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Vendor/Employee";
            int colPartyPlantName = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Type";
            int colType = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Voucher No";
            int colVoucherNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Posting Date";
            int colPostingDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            COL++;

            worksheet[ROW, COL].Text = "Entry Date";
            int colVoucherDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            COL++;

            worksheet[ROW, COL].Text = "DocRef No";
            int colDocRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Doc Date";
            int colDocDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            COL++;

            worksheet[ROW, COL].Text = "GRN No.";
            int colGRNNo = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GRN Date.";
            int colGRNDate = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Tran. Currency";
            int colCurrencyCode = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Tran. Payment";
            int colTranPaymentAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Payment" + '(' + dtCompanyCurrency.Rows[0]["Code"].ToString() + ')';
            //worksheet[ROW, COL].Text = "Payable " + '(' + dtCompanyCurrency.Rows[0]["Code"].ToString() + ')';
            int colBooksPayment = COL;

            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Narration";
            int colNarration = COL;
            worksheet[ROW, COL].ColumnWidth = 60;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            //int colTaskDetail = 0;
            //if (checkbox == true)
            //{
            //    COL++;
            //    colTaskDetail = COL;

            //    worksheet[ROW, COL].Text = "Sub Task";
            //    worksheet[ROW, COL].ColumnWidth = 40;
            //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //}
            //COL++;

            //worksheet[ROW, COL].Text = "SubTaskStatus";
            //int colSubTaskStatus  = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            ////COL++;

            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;

            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Black;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            ROW++;

            for (int i = 0; i < dtAutoMailReportList.Rows.Count; i++)
            {
                worksheet[ROW, colSLNO].Number = (i + 1);

                //worksheet[ROW, colSequence].Number = clsStaticInfo.dbl(dtAutoMailReportList.Rows[i]["Sequence"].ToString());
                //worksheet[ROW, colSequence].NumberFormat = clsStaticInfo.NumberFormat(2);

                //worksheet[ROW, colGRNNo].Text = dtAutoMailReportList.Rows[i]["GRNNo"].ToString();
                if (dtAutoMailReportList.Rows[i]["GRNDate"].ToString() != "")
                {
                    worksheet[ROW, colGRNDate].DateTime = Convert.ToDateTime(dtAutoMailReportList.Rows[i]["GRNDate"].ToString());
                    worksheet[ROW, colGRNDate].NumberFormat = "dd-MMM-yyyy";
                }
                else
                {
                    worksheet[ROW, colGRNDate].Text = dtAutoMailReportList.Rows[i]["GRNDate"].ToString();

                }
                worksheet[ROW, colVoucherNo].Text = dtAutoMailReportList.Rows[i]["VoucherNo"].ToString();
                worksheet[ROW, colDocRefNo].Text = dtAutoMailReportList.Rows[i]["DocRefNo"].ToString();

                worksheet[ROW, colDocDate].DateTime = Convert.ToDateTime(dtAutoMailReportList.Rows[i]["DocDate"].ToString());
                worksheet[ROW, colDocDate].NumberFormat = "dd-MMM-yyyy";

                worksheet[ROW, colVoucherDate].DateTime = Convert.ToDateTime(dtAutoMailReportList.Rows[i]["EntryDate"].ToString());
                worksheet[ROW, colVoucherDate].NumberFormat = "dd-MMM-yyyy";

                worksheet[ROW, colPostingDate].DateTime = Convert.ToDateTime(dtAutoMailReportList.Rows[i]["PostingDate"].ToString());
                worksheet[ROW, colPostingDate].NumberFormat = "dd-MMM-yyyy";

                //worksheet[ROW, colDocDate].Text = dtAutoMailReportList.Rows[i]["DocDate"].ToString();
                //worksheet[ROW, colVoucherDate].Text = dtAutoMailReportList.Rows[i]["EntryDate"].ToString();
                //worksheet[ROW, colPostingDate].Text = dtAutoMailReportList.Rows[i]["PostingDate"].ToString();
                worksheet[ROW, colBooksPayment].Number = clsStaticInfo.dbl(dtAutoMailReportList.Rows[i]["BooksPayment"].ToString());
                worksheet[ROW, colBooksPayment].NumberFormat = clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colNarration].Text = dtAutoMailReportList.Rows[i]["Narration"].ToString();
                worksheet[ROW, colType].Text = dtAutoMailReportList.Rows[i]["Type"].ToString();
                // worksheet[ROW, colType].Text = dtAutoMailReportList.Rows[i]["TranPaymentAmount"].ToString();
                worksheet[ROW, colTranPaymentAmount].Number = clsStaticInfo.dbl(dtAutoMailReportList.Rows[i]["TranPaymentAmount"].ToString());
                worksheet[ROW, colTranPaymentAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colCurrencyCode].Text = dtAutoMailReportList.Rows[i]["CurrencyCode"].ToString();
                worksheet[ROW, colPartyPlantName].Text = dtAutoMailReportList.Rows[i]["ParticularName"].ToString();

                //if (checkbox == true)
                //{

                //    worksheet[ROW, colTaskDetail].Text = dtIssueReportList.Rows[i]["TaskDetail"].ToString();

                //}

                // worksheet[ROW, colPurchasePrice].NumberFormat = clsStaticInfo.NumberFormat();
                // worksheet[ROW, colScantionAmount].Number = clsStaticInfo.dbl(dtAllLoanRegisterList.Rows[i]["ScantionAmount"].ToString());
                //worksheet[ROW, colFGComponent].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["FGComponent"].ToString());

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref worksheet, endCol, " Last 10 Days Creation Payment List", PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes



            return workbook;
        }

        #endregion

        #region date range wise payable  Payment List and Report
        public DataTable GetDateRangeWisePaymentReportData(string companyGroupId, string companyId, string plantId, string fromDate, string toDate)
        {
            var sql = @"select sum(x.DrAmount) DrAmount,sum(x.TranPaymentAmount)TranPaymentAmount,x.CurrencyCode,x.ParticularName,x.PartyId,x.EmployeeId,x.Type,SUM(x.BooksPayment) BooksPayment
                    ,x.ActivityId 
                    from(

                    select --BM.AccountTitle UserName,
                    VD.DrAmount  ,VD.CrAmount TranPaymentAmount , c.Code CurrencyCode
				     ,ParticularName =concat(STUFF((select distinct ','+xp.Code+ '-' +xp.UserName from TRN.VoucherDetail XVD JOIN hkp.Party AS XP ON XP.Id=XVD.PartyId
										    where XVD.VoucherId=V.Id AND XVD.PartyId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										    ,STUFF((select distinct ','+xp.EmployeeCode+ '- ' +xp.EmployeeName from TRN.VoucherDetail XVD JOIN dbo.EmployeeInformation AS XP ON XP.SystemId=XVD.EmployeeId
										    where XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									    ,STUFF((select distinct ','+XA.Code+ '- ' +XA.UserName from TRN.VoucherDetail XVD JOIN HKP.Activity AS XA ON XA.Id=XVD.ActivityId
										    where XVD.VoucherId=V.Id AND XVD.PartyId IS NULL and XVD.EmployeeId IS NULL AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))

										    --,STUFF((select distinct ','+XG.AccountCode+ '- ' +XG.UserName from TRN.VoucherDetail XVD JOIN HKP.GLGeneralInfo AS XG ON XG.Id=XVD.GLGeneralInfoId
										    --where XVD.VoucherId=V.Id AND XVD.GLGeneralInfoId<>'' AND VD.GLGeneralInfoId!=XVD.GLGeneralInfoId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
						


				     ,PartyId =STUFF((select distinct ','+xp.Id from TRN.VoucherDetail XVD JOIN hkp.Party AS XP ON XP.Id=XVD.PartyId
								    where XVD.VoucherId=V.Id AND XVD.PartyId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
					
				    ,EmployeeId =STUFF((select distinct ','+xp.SystemId from TRN.VoucherDetail XVD JOIN dbo.EmployeeInformation AS XP ON XP.SystemId=XVD.EmployeeId
								    where XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
				 
				    ,[Type] =concat(STUFF((select distinct ','+'Vendor' from TRN.VoucherDetail XVD JOIN hkp.Party AS XP ON XP.Id=XVD.PartyId
							    where XVD.VoucherId=V.Id AND XVD.PartyId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							    ,STUFF((select distinct ','+'Employee' from TRN.VoucherDetail XVD JOIN dbo.EmployeeInformation AS XP ON XP.SystemId=XVD.EmployeeId
							    where XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
						,STUFF((select distinct ','+'GL' from TRN.VoucherDetail XVD JOIN HKP.Activity AS XA ON XA.Id=XVD.ActivityId
							where XVD.VoucherId=V.Id AND XVD.PartyId IS NULL and XVD.EmployeeId IS NULL AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	
                                )
							
				    ,ActivityId=STUFF((select distinct ','+XA.id from TRN.VoucherDetail XVD JOIN HKP.Activity AS XA ON XA.Id=XVD.ActivityId
							    where XVD.VoucherId=V.Id AND XVD.PartyId IS NULL and XVD.EmployeeId IS NULL AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')			
							
							
				       ,isnull(VD.CrAmount,0) * isnull(vdc.ToCurrencyRate,0) BooksPayment
			   		    --,IVD.NetAmount*CC.CompanyCurrencyRate PayableBooks
                    from
                    TRN.VoucherDetail VD
                    LEFT JOIN TRN.VoucherDetailCurrency Vdc ON Vdc.VoucherDetailId=VD.Id
                    LEFT JOIN TRN.Voucher V ON V.Id=VD.VoucherId
                    LEFT JOIN MST.BankMaster BM ON BM.Id=VD.BankMasterId
                    LEFT JOIN TRN.VoucherDetail XVD ON XVD.VoucherId=V.Id AND XVD.BankMasterId<>'' AND XVD.DrAmount>0
                    LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId

                    WHERE VD.BankMasterId<>'' AND XVD.BankMasterId IS NULL AND VD.CrAmount>0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + @"' 
                    AND V.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"'
			        --and DATEDIFF(DAY, GETDATE(),V.VoucherDate) >-10
                
				    union all

                    select --BM.UserName,
				    VD.DrAmount ,VD.CrAmount TranPaymentAmount, c.Code CurrencyCode
				
				      ,ParticularName =concat(STUFF((select distinct ',' +xp.Code+ '-' +xp.UserName from TRN.VoucherDetail XVD JOIN hkp.Party AS XP ON XP.Id=XVD.PartyId
										    where XVD.VoucherId=V.Id AND XVD.PartyId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										    ,STUFF((select distinct ','+xp.EmployeeCode+ '- ' +xp.EmployeeName from TRN.VoucherDetail XVD JOIN dbo.EmployeeInformation AS XP ON XP.SystemId=XVD.EmployeeId
										    where XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									    ,STUFF((select distinct ','+XA.Code+ '- ' +XA.UserName from TRN.VoucherDetail XVD JOIN HKP.Activity AS XA ON XA.Id=XVD.ActivityId
										    where XVD.VoucherId=V.Id AND XVD.PartyId IS NULL and XVD.EmployeeId IS NULL AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))

										    --,STUFF((select distinct ','+XG.AccountCode+ '- ' +XG.UserName from TRN.VoucherDetail XVD JOIN HKP.GLGeneralInfo AS XG ON XG.Id=XVD.GLGeneralInfoId
										    --where XVD.VoucherId=V.Id AND XVD.GLGeneralInfoId<>'' AND VD.GLGeneralInfoId!=XVD.GLGeneralInfoId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
								    --hkp.GLGeneralInfo

				     ,PartyId =STUFF((select distinct ','+xp.Id from TRN.VoucherDetail XVD JOIN hkp.Party AS XP ON XP.Id=XVD.PartyId
								    where XVD.VoucherId=V.Id AND XVD.PartyId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
					
				    ,EmployeeId =STUFF((select distinct ','+xp.SystemId from TRN.VoucherDetail XVD JOIN dbo.EmployeeInformation AS XP ON XP.SystemId=XVD.EmployeeId
								    where XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
				 
				    ,[Type] =concat(STUFF((select distinct ','+'Vendor' from TRN.VoucherDetail XVD JOIN hkp.Party AS XP ON XP.Id=XVD.PartyId
							    where XVD.VoucherId=V.Id AND XVD.PartyId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							    ,STUFF((select distinct ','+'Employee' from TRN.VoucherDetail XVD JOIN dbo.EmployeeInformation AS XP ON XP.SystemId=XVD.EmployeeId
							    where XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
						,STUFF((select distinct ','+'GL' from TRN.VoucherDetail XVD JOIN HKP.Activity AS XA ON XA.Id=XVD.ActivityId
							where XVD.VoucherId=V.Id AND XVD.PartyId IS NULL and XVD.EmployeeId IS NULL AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	
                                )
				   
			    ,ActivityId=STUFF((select distinct ','+XA.id from TRN.VoucherDetail XVD JOIN HKP.Activity AS XA ON XA.Id=XVD.ActivityId
				    where XVD.VoucherId=V.Id AND XVD.PartyId IS NULL and XVD.EmployeeId IS NULL AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	

				       ,isnull(VD.CrAmount,0) * isnull(vdc.ToCurrencyRate,0) BooksPayment
                    from
                    TRN.VoucherDetail VD
                    LEFT JOIN TRN.VoucherDetailCurrency Vdc ON Vdc.VoucherDetailId=VD.Id
                    LEFT JOIN TRN.Voucher V ON V.Id=VD.VoucherId
                    LEFT JOIN MST.CashMaster BM ON BM.Id=VD.CashMasterId
                    LEFT JOIN TRN.VoucherDetail XVD ON XVD.VoucherId=V.Id AND XVD.CashMasterId<>'' AND XVD.DrAmount>0
                    LEFT JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                    WHERE VD.CashMasterId<>'' AND XVD.CashMasterId IS NULL AND VD.CrAmount>0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + @"' 
                    AND V.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"'

			        --and DATEDIFF(DAY, GETDATE(),V.VoucherDate) >-10 

				    ) x
				    group by x.CurrencyCode,x.ParticularName,x.PartyId,x.EmployeeId,x.Type,x.PartyId,x.ActivityId";

            return _sqlRepository.GetDataTable(sql);
        }

        public IWorkbook GetDateRangeWisePaymentReport(string CompanyGroupId, string CompanyId, string PlantId, string fromDate, string toDate)  //, bool checkbox
        {
            // var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //AND V.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"'
            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            DataTable dtAutoMailReportList = GetDateRangeWisePaymentReportData(CompanyGroupId, CompanyId, PlantId, fromDate, toDate);

            DataTable dtCompanyCurrency = _sqlRepository.GetDataTable(@"select CR.* from org.Company c
                                                        inner join scs.Currency CR ON CR.Id=c.BaseCurrencyId
                                                        where C.Id='" + CompanyId + "'");

            if (dtAutoMailReportList.Rows.Count == 0)
                throw new Exception("No data found");

            worksheet.Name = "DateRangeWisePaymentList";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            worksheet[ROW, COL].Text = "SL. No";
            int colSLNO = COL;
            worksheet[ROW, COL].ColumnWidth = 5;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Vendor/Employee";
            int colPartyPlantName = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Type";
            int colType = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            //worksheet[ROW, COL].Text = "Voucher No";
            //int colVoucherNo = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            //worksheet[ROW, COL].Text = "Posting Date";
            //int colPostingDate = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;

            //worksheet[ROW, COL].Text = "Entry Date";
            //int colVoucherDate = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;

            //worksheet[ROW, COL].Text = "DocRef No";
            //int colDocRefNo = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            //worksheet[ROW, COL].Text = "Doc Date";
            //int colDocDate = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;

            //worksheet[ROW, COL].Text = "GRN No.";
            //int colGRNNo = COL;
            //worksheet[ROW, COL].ColumnWidth = 10;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            //worksheet[ROW, COL].Text = "GRN Date.";
            //int colGRNDate = COL;
            //worksheet[ROW, COL].ColumnWidth = 10;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;

            worksheet[ROW, COL].Text = "Tran. Currency";
            int colCurrencyCode = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Tran. Payment";
            int colTranPaymentAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            //worksheet[ROW, COL].Text = "Payment" + '(' + dtCompanyCurrency.Rows[0]["Code"].ToString() + ')';
            //int colBooksPayment = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;

            //worksheet[ROW, COL].Text = "Narration";
            //int colNarration = COL;
            //worksheet[ROW, COL].ColumnWidth = 60;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            //worksheet[ROW, COL].Text = "Party Id";
            //int colPartyId = COL;
            //worksheet[ROW, COL].ColumnWidth = 60;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            //worksheet[ROW, COL].Text = "Employee Id";
            //int colEmployeeId = COL;
            //worksheet[ROW, COL].ColumnWidth = 60;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            worksheet[ROW, COL].Text = "Payment" + '(' + dtCompanyCurrency.Rows[0]["Code"].ToString() + ')';
            //worksheet[ROW, COL].Text = "Books Payment";
            int colBooksPayment = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            // COL++;

            //int colTaskDetail = 0;
            //if (checkbox == true)
            //{
            //    COL++;
            //    colTaskDetail = COL;

            //    worksheet[ROW, COL].Text = "Sub Task";
            //    worksheet[ROW, COL].ColumnWidth = 40;
            //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //}
            //COL++;

            //worksheet[ROW, COL].Text = "SubTaskStatus";
            //int colSubTaskStatus  = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            ////COL++;

            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;

            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Black;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            ROW++;

            int StartDataRow = ROW;
            for (int i = 0; i < dtAutoMailReportList.Rows.Count; i++)
            {
                worksheet[ROW, colSLNO].Number = (i + 1);

                //if (dtAutoMailReportList.Rows[i]["GRNDate"].ToString() != "")
                //{
                //    worksheet[ROW, colGRNDate].DateTime = Convert.ToDateTime(dtAutoMailReportList.Rows[i]["GRNDate"].ToString());
                //    worksheet[ROW, colGRNDate].NumberFormat = "dd-MMM-yyyy";
                //}
                //else
                //{
                //    worksheet[ROW, colGRNDate].Text = dtAutoMailReportList.Rows[i]["GRNDate"].ToString();

                //}
                //worksheet[ROW, colVoucherNo].Text = dtAutoMailReportList.Rows[i]["VoucherNo"].ToString();
                //worksheet[ROW, colDocRefNo].Text = dtAutoMailReportList.Rows[i]["DocRefNo"].ToString();

                //worksheet[ROW, colDocDate].DateTime = Convert.ToDateTime(dtAutoMailReportList.Rows[i]["DocDate"].ToString());
                //worksheet[ROW, colDocDate].NumberFormat = "dd-MMM-yyyy";

                //worksheet[ROW, colVoucherDate].DateTime = Convert.ToDateTime(dtAutoMailReportList.Rows[i]["EntryDate"].ToString());
                //worksheet[ROW, colVoucherDate].NumberFormat = "dd-MMM-yyyy";

                //worksheet[ROW, colPostingDate].DateTime = Convert.ToDateTime(dtAutoMailReportList.Rows[i]["PostingDate"].ToString());
                //worksheet[ROW, colPostingDate].NumberFormat = "dd-MMM-yyyy";


                worksheet[ROW, colBooksPayment].Number = clsStaticInfo.dbl(dtAutoMailReportList.Rows[i]["BooksPayment"].ToString());
                worksheet[ROW, colBooksPayment].NumberFormat = clsStaticInfo.NumberFormat(2);
                //worksheet[ROW, colNarration].Text = dtAutoMailReportList.Rows[i]["Narration"].ToString();
                worksheet[ROW, colType].Text = dtAutoMailReportList.Rows[i]["Type"].ToString();
                worksheet[ROW, colTranPaymentAmount].Number = clsStaticInfo.dbl(dtAutoMailReportList.Rows[i]["TranPaymentAmount"].ToString());
                worksheet[ROW, colTranPaymentAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colCurrencyCode].Text = dtAutoMailReportList.Rows[i]["CurrencyCode"].ToString();
                worksheet[ROW, colPartyPlantName].Text = dtAutoMailReportList.Rows[i]["ParticularName"].ToString();
                //worksheet[ROW, colPartyId].Text = dtAutoMailReportList.Rows[i]["PartyId"].ToString();
                //worksheet[ROW, colEmployeeId].Text = dtAutoMailReportList.Rows[i]["EmployeeId"].ToString();

                //if (checkbox == true)
                //{

                //    worksheet[ROW, colTaskDetail].Text = dtIssueReportList.Rows[i]["TaskDetail"].ToString();

                //}

                // worksheet[ROW, colPurchasePrice].NumberFormat = clsStaticInfo.NumberFormat();
                // worksheet[ROW, colScantionAmount].Number = clsStaticInfo.dbl(dtAllLoanRegisterList.Rows[i]["ScantionAmount"].ToString());
                //worksheet[ROW, colFGComponent].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["FGComponent"].ToString());

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }


            worksheet[ROW, colBooksPayment - 1].Text = "Total";
            worksheet[ROW, colBooksPayment - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;

            worksheet[ROW, colBooksPayment].Formula = "SUM(" + clsStaticInfo.GetxlsCol(colBooksPayment) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(colBooksPayment) + (ROW - 1).ToString() + ")";
            worksheet[ROW, colBooksPayment].NumberFormat = "#,##0.00;(#,##0.00)";
            worksheet.Range[ROW, colBooksPayment, ROW, colBooksPayment].CellStyle.Font.Bold = true;
            worksheet[ROW, colBooksPayment].HorizontalAlignment = ExcelHAlign.HAlignRight;


            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref worksheet, endCol, "From " + fromDate + " To " + toDate + "", PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            worksheet[ROW, colBooksPayment].HorizontalAlignment = ExcelHAlign.HAlignRight;
            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes



            return workbook;
        }

        public DataTable GetToPlantInvetoryTransferData(string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var cmdText = @"SELECT V.Id, GL.Id AS AccountCodeId, GL.AccountCode, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.VoucherNo, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
                            , VD.DrAmount+VD.CrAmount AS Value, VDC.DrAmount, VDC.CrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
                            , REPLACE(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS InvoiceDate, VD.DocRefNo AS InvoiceNo, UPPER(VD.Narration) AS DetailNarration, ENT.UserName AS Entity
                            , VD.Id AS BudgetMasterId, BUD.UserName AS BudgetName, ACT.UserName AS Activity, UPPER(V.Narration) AS Narration, P.UserName AS PartyName, PP.UserName AS PartyLocation
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id =VDC.VoucherDetailId
                            INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BMT ON VD.BudgetMasterId=BMT.Id
                            LEFT JOIN [HKP].[Budget] BUD ON BUD.Id=BMT.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                            LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
							LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
							LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(cmdText);
        }
        public IWorkbook GetToPlantInvetoryTransferVoucher(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var excelEngine = new ExcelEngine();

            var reportUtility = new ReportUtility();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var dsLocal = GetToPlantInvetoryTransferData(companyGroupId, companyId, plantId, voucherId);
            if (dsLocal.Rows.Count == 0)
                throw new Exception("No data found !");

            // Set report Name
            reportFileName = Convert.ToDateTime(dsLocal.Rows[0]["PostingDate"]).ToString("yyMMdd") + " " + dsLocal.Rows[0]["VoucherNo"];

            var curCode = dsLocal.Rows[0]["CurrencyCode"].ToString();
            var trnCur = dsLocal.Rows[0]["TrnCurrency"].ToString();
            var row = 5;

            //var colLast = 1;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, dsLocal.Rows[0]["VoucherNo"].ToString());
            //sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();


            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 5, dsLocal.Rows[0]["VoucherDate"].ToString());
            if (curCode != trnCur)
            {
                sheet[reportUtility.GetColumnNameForXls(5) + 8 + ":" + reportUtility.GetColumnNameForXls(6) + 8].Merge();
            }

            row++;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Doc Date");
            reportUtility.SetText(ref sheet, row, 2, dsLocal.Rows[0]["DocDate"].ToString());
            //sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc No");
            reportUtility.SetText(ref sheet, row, 5, dsLocal.Rows[0]["DocRefNo"].ToString());
            if (curCode != trnCur)
            {
                sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            }

            row++;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, dsLocal.Rows[0]["PostingDate"].ToString());
            //sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Fiscal Year");
            reportUtility.SetText(ref sheet, row, 5, dsLocal.Rows[0]["FiscalYearName"] + " (" + dsLocal.Rows[0]["PeriodNo"] + ")");
            if (curCode != trnCur)
            {
                sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            }

            row++; //row8
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, dsLocal.Rows[0]["Narration"].ToString());
            //sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Status");
            reportUtility.SetText(ref sheet, row, 5, dsLocal.Rows[0]["Park/Post"].ToString());

            row++; //row9




            sheet.Range[9, 4, row, 5].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[9, 4, row, 5].BorderInside(ExcelLineStyle.Hair);
            row++;
            var col = 1;
            reportUtility.SetHeaderText(ref sheet, 10, col, "GL", 15); col++;
            reportUtility.SetHeaderText(ref sheet, 10, col, "Budget", 15); col++;
            reportUtility.SetHeaderText(ref sheet, 10, col, "Activity", 15); col++;
            sheet[10, 1, 10, 3].Merge();
            if (curCode != trnCur)
            {
                reportUtility.SetHeaderText(ref sheet, 10, col, "Trn Currency", 7); col++;
                reportUtility.SetHeaderText(ref sheet, 10, col, "Trn Value", 9); col++;
            }
            reportUtility.SetHeaderText(ref sheet, 9, col, curCode, ExcelHAlign.HAlignCenter);
            sheet[9, col, 9, col + 1].Merge();
            reportUtility.SetHeaderText(ref sheet, 10, col, "Debit", 11, ExcelHAlign.HAlignRight); col++;
            reportUtility.SetHeaderText(ref sheet, 10, col, "Credit", 11, ExcelHAlign.HAlignRight);
            var colLast = col;
            //row = 11;
            row++;

            double _Total_Amount = 0;
            var Row_Total_Start = row; //row11
            for (int n = 0; n < dsLocal.Rows.Count; n++)
            {
                col = 1;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["AccountCode"] + " - " + dsLocal.Rows[n]["GL"] + " - " + dsLocal.Rows[n]["BudgetName"] + " - " + dsLocal.Rows[n]["Activity"]); col++; //GL

                col++;
                col++;
                if (curCode != trnCur)
                {
                    reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["TrnCurrency"].ToString()); col++;
                    reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["Value"])); col++;
                }
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["DrAmount"].ToString()));
                _Total_Amount += Convert.ToDouble(dsLocal.Rows[n]["DrAmount"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["CrAmount"].ToString())); col++;
                sheet[row, 1, row, 3].Merge();
                row++;
            }

            var rowLast = row - 1;
            sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(colLast - 2) + row].Merge();
            reportUtility.SetText(ref sheet, row, 1, "Total : ", true);

            sheet.Range[row, colLast - 1].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colLast - 1) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(colLast - 1) + rowLast + ")"; //rowLast
            sheet.Range[row, colLast - 1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
            sheet.Range[row, colLast - 1].CellStyle.Font.Bold = true;
            // sheet.Range[row, colLast - 1].BorderAround(ExcelLineStyle.Hair);

            sheet.Range[row, colLast].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colLast) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(colLast) + rowLast + ")";
            sheet.Range[row, colLast].NumberFormat = reportUtility.NumberFormatDecimalTwo();
            sheet.Range[row, colLast].CellStyle.Font.Bold = true;
            //sheet.Range[row, colLast].BorderAround(ExcelLineStyle.Hair);

            sheet.Range[10, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[10, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row += 2;

            reportUtility.SetText(ref sheet, row, 1, "In Word : ", true);
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(_Total_Amount, dsLocal.Rows[0]["CurrencyId"].ToString()); ;
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

            sheet.UsedRange.AutofitColumns();
            sheet.UsedRange.CellStyle.Font.Size = 8;
            row = row + 4;

            reportUtility.SetSignatureText(ref sheet, row - 1, 1, dsLocal.Rows[0]["AddedBy"].ToString());
            sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

            reportUtility.SetSignatureText(ref sheet, row - 1, 3, dsLocal.Rows[0]["PostedBy"].ToString());
            sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

            sheet.Range[row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, colLast, "Authorized By", true);

            reportUtility.CompanyPlantHeader(ref sheet, colLast, "Inventory Transfer Voucher", companyId, plantId, plantName, null);
            reportUtility.FreezePage(ref sheet, 1, colLast);
            reportUtility.PageAdjustableSetup(ref sheet, 1, row + 3, ExcelPageOrientation.Portrait);



            sheet[1, 2].ColumnWidth = 30;
            sheet[1, 3].ColumnWidth = 20;
            return workbook;



        }
        #endregion

        #region Expense Distribution Report-InboundInvoice
        public IWorkbook GetVendorInvoiceReportExpenseDistribution(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Expense Distribution Report";

            var header = GetVendorInvoiceHeaderExpenseDistribution(companyGroupId, companyId, plantId, voucherId, SourceType.VendorInvoice);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetCustomerInvoiceVoucherExpenseDistribution(voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            accountsCommonService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
            //_companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;
            var colLast = 1;
            int xlsCol = 1;
            int colGl = 0;
            int colInvoiceNo = 0;
            int colVendorCustomer = 0;
            int colInvoiceAmount = 0;
            int colDistributedAmount = 0;
            int colInvoiceType = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString());
            sheet[row, 4].ColumnWidth = 15;
            sheet[row, 5].ColumnWidth = 15;
            row++;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString());
            //sheet[row, 5].ColumnWidth = 15;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor/Customer");
            reportUtility.SetText(ref sheet, row, 2, header["Vendor"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString());
            //sheet[row, 5].ColumnWidth = 15;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor/Customer Plant");
            reportUtility.SetText(ref sheet, row, 2, header["VendorPlant"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Status");
            reportUtility.SetText(ref sheet, row, 5, header["Status"].ToString());
            //sheet[row, 5].ColumnWidth = 15;
            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet[row, 2].ColumnWidth = 30;

            row++;  //10

            //if (companyCurrencyId == transcationCurrency)
            //{
            //    reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
            //    sheet[row, 4, row, 5].Merge();
            //}
            //else
            //{
            //    reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
            //    sheet[row, 4, row, 5].Merge();

            //    reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
            //    sheet[row, 6, row, 7].Merge();
            //}
            sheet[row, 6].ColumnWidth = 15;
            sheet[row, 7].ColumnWidth = 15;
            //sheet.Range[row, 4, row, colLast].BorderAround(ExcelLineStyle.Hair);
            //sheet.Range[row, 4, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;


            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL", 20); colGl = xlsCol; xlsCol++;
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            //xlsCol++; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Type", 13, ExcelHAlign.HAlignLeft); colInvoiceType = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Invoice No", 13, ExcelHAlign.HAlignLeft); colInvoiceNo = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Vendor/Customer", 13, ExcelHAlign.HAlignLeft); colVendorCustomer = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Amount", 15, ExcelHAlign.HAlignRight); colInvoiceAmount = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "DistributedAmount", 20, ExcelHAlign.HAlignRight); colDistributedAmount = xlsCol;
            colLast = xlsCol;


            int formulaStartRow = 0;
            int formulaEndRow = 0;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;

                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();
                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);
                    reportUtility.SetText(ref sheet, row, colInvoiceType, dsLocal.Rows[i]["InvoiceType"].ToString());
                    reportUtility.SetText(ref sheet, row, colInvoiceNo, dsLocal.Rows[i]["DocRefNo"].ToString());
                    reportUtility.SetText(ref sheet, row, colVendorCustomer, dsLocal.Rows[i]["CustomerPlant"].ToString());
                    reportUtility.SetTextDecimalThree(ref sheet, row, colInvoiceAmount, Convert.ToDouble(dsLocal.Rows[i]["Amount"].ToString()));
                    reportUtility.SetTextDecimalThree(ref sheet, row, colDistributedAmount, Convert.ToDouble(dsLocal.Rows[i]["DistributedAmount"].ToString()));

                    totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DistributedAmount"].ToString());

                    sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);

                    glName = string.Empty;

                    row++;
                }


                formulaEndRow = row - 1;


                //reportUtility.SetText(ref sheet, row, 3, "Total: ", true);


                //if (companyCurrencyId != transcationCurrency)
                //{
                //    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (formulaEndRow) + ")";
                //    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalThree();
                //    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                //    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                //    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (formulaEndRow) + ")";
                //    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalThree();
                //    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                //    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                //    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (formulaEndRow) + ")";
                //    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalThree();
                //    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                //    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                //    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (formulaEndRow) + ")";
                //    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalThree();
                //    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                //    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                //}
                //else
                //{
                //    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (formulaEndRow) + ")";
                //    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalThree();
                //    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                //    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                //    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (formulaEndRow) + ")";
                //    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalThree();
                //    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                //    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                //}

                //sheet.Range[row, colinrDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, colinrDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);

                //sheet.Range[row, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Thin);
                //sheet.Range[row, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Thin);

                //row += 2;
                //reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                //if (companyCurrencyId != transcationCurrency)
                //{
                //    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                //    sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                //    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                //    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
                //    row++;
                //}

                //sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                //sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                //sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                //sheet.UsedRange.AutofitColumns();
                //sheet[1, 2].ColumnWidth = 60;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);
                sheet[row, 1].ColumnWidth = 25;

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);
                sheet[row, 3].ColumnWidth = 25;

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Expense Distribution Report", companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);


            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 7, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }

        private DataTable GetCustomerInvoiceVoucherExpenseDistribution(string voucherId)
        {
            try
            {
                var sql = @"SELECT GL.Id AS AccountCodeId, VD.DrAmount AS DrAmount, VD.CrAmount AS CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant,  BUD.UserName AS Budget
                            ,Activity=CASE WHEN VD.CashMasterId<>'' THEN  CM.UserName  WHEN VD.BankMasterId<>'' THEN BNM.AccountTitle Else ACT.UserName end 
                            ,ID.InvoiceType,ID.DistributedAmount,IV.Id AS InvoiceId,IV.DocRefNo,ID.Amount
                            FROM TRN.InvoiceDetailCharges ID
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=ID.VoucherDetailId
							JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherDetailId=ID.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [TRN].[Invoice] AS IV ON IV.Id=ID.InvoiceId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [MST].[BankMaster] AS BNM ON BNM.Id=VD.BankMasterId
                            WHERE V.Archive=0 AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private Dictionary<string, object> GetVendorInvoiceHeaderExpenseDistribution(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
							, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
							, AddedBy =case when u.FullName<>'' then u.FullName else v.AddedBy end
							,PostedBy = case when up.FullName<>'' then up.FullName else v.PostedBy end
							 , UPPER(V.Narration) AS Narration 
					    	, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , ISNULL(P.UserName,PAN.UserName) AS Vendor, ISNULL(PP.UserName,PPAN.UserName) AS VendorPlant
							, V.CurrencyId, C.Code AS CurrencyCode
                            FROM [TRN].[Voucher] AS V 
							LEFT JOIN [TRN].[Invoice] AS BJ ON V.Id=BJ.VoucherId
                            LEFT JOIN [TRN].[AdjustmentNote] AS AN ON V.Id=AN.VoucherId
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [HKP].[Party] AS P ON P.Id=BJ.PartyId
							LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=BJ.PartyPlantId
							LEFT JOIN [HKP].[Party] AS PAN ON PAN.Id=AN.PartyId
							LEFT JOIN [HKP].[PartyPlant] AS PPAN ON PPAN.Id=AN.PartyPlantId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
							left join [sec].[User] U on U.UserId=v.AddedBy
							left join sec.[User] up on up.UserId=v.PostedBy
                            WHERE  V.Id='" + voucherId + @"' ";
            return _sqlRepository.GetData(cmdText);
        }
        #endregion

        #region Expense Distribution Report-InboundInvoice
        public IWorkbook GetVendorInvoiceReportAssetDistribution(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Asset Distribution Report";

            var header = GetVendorInvoiceHeaderExpenseDistribution(companyGroupId, companyId, plantId, voucherId, SourceType.VendorInvoice);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetCustomerInvoiceVoucherAssetDistribution(voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            accountsCommonService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
            //_companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;
            var colLast = 1;
            int xlsCol = 1;
            int colGl = 0;
            int colInvoiceNo = 0;
            int colMachineMasterId = 0;
            int colDistributedAmount = 0;
            int colInvoiceType = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString());
            sheet[row, 4].ColumnWidth = 15;
            sheet[row, 5].ColumnWidth = 15;
            row++;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString());
            //sheet[row, 5].ColumnWidth = 15;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor/Customer");
            reportUtility.SetText(ref sheet, row, 2, header["Vendor"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString());
            //sheet[row, 5].ColumnWidth = 15;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor/Customer Plant");
            reportUtility.SetText(ref sheet, row, 2, header["VendorPlant"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Status");
            reportUtility.SetText(ref sheet, row, 5, header["Status"].ToString());
            //sheet[row, 5].ColumnWidth = 15;
            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet[row, 2].ColumnWidth = 30;

            row++;  //10

            //if (companyCurrencyId == transcationCurrency)
            //{
            //    reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
            //    sheet[row, 4, row, 5].Merge();
            //}
            //else
            //{
            //    reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
            //    sheet[row, 4, row, 5].Merge();

            //    reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
            //    sheet[row, 6, row, 7].Merge();
            //}
            sheet[row, 6].ColumnWidth = 15;
            sheet[row, 7].ColumnWidth = 15;
            //sheet.Range[row, 4, row, colLast].BorderAround(ExcelLineStyle.Hair);
            //sheet.Range[row, 4, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;


            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL", 20); colGl = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Machine Master Id", 13, ExcelHAlign.HAlignLeft); colMachineMasterId = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Machine Master", 13, ExcelHAlign.HAlignLeft); colInvoiceType = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Asset Name", 13, ExcelHAlign.HAlignLeft); colInvoiceNo = xlsCol; xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Distributed Amount", 20, ExcelHAlign.HAlignRight); colDistributedAmount = xlsCol;
            colLast = xlsCol;


            int formulaStartRow = 0;
            int formulaEndRow = 0;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;

                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();
                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);
                    reportUtility.SetText(ref sheet, row, colMachineMasterId, dsLocal.Rows[i]["MachineMasterId"].ToString());
                    reportUtility.SetText(ref sheet, row, colInvoiceType, dsLocal.Rows[i]["MachineMaster"].ToString());
                    reportUtility.SetText(ref sheet, row, colInvoiceNo, dsLocal.Rows[i]["AssetName"].ToString());
                    reportUtility.SetTextDecimalThree(ref sheet, row, colDistributedAmount, Convert.ToDouble(dsLocal.Rows[i]["DistributedAmount"].ToString()));

                    totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DistributedAmount"].ToString());

                    sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);

                    glName = string.Empty;

                    row++;
                }


                formulaEndRow = row - 1;



                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);
                sheet[row, 1].ColumnWidth = 25;

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);
                sheet[row, 3].ColumnWidth = 25;

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Asset Distribution Report", companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);


            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 7, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }

        private DataTable GetCustomerInvoiceVoucherAssetDistribution(string voucherId)
        {
            try
            {
                var sql = @"SELECT GL.Id AS AccountCodeId, VD.DrAmount AS DrAmount, VD.CrAmount AS CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant,  BUD.UserName AS Budget
                            ,Activity=CASE WHEN VD.CashMasterId<>'' THEN  CM.UserName  WHEN VD.BankMasterId<>'' THEN BNM.AccountTitle Else ACT.UserName end 
                            ,MMA.MachineMasterId,MM.UserName MachineMaster,MMA.AssetName,AD.DistributedAmount,V.DocRefNo
                            FROM TRN.MachineMasterAssetSeviceDistribution AD
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=AD.VoucherDetailId
							JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherDetailId=AD.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [dbo].[MachineMasterAsset]  AS MMA ON MMA.Id=AD.MachineMasterAssetId
							LEFT JOIN [MST].[MachineMaster] MM ON MM.Id=MMA.MachineMasterId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [MST].[BankMaster] AS BNM ON BNM.Id=VD.BankMasterId
                            WHERE V.Archive=0 AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Invoice To Acceptance Post Report
        public IWorkbook GetAcceptancePostReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetAcceptancePostReportHeader(companyGroupId, companyId, plantId, voucherId, SourceType.InvoiceToAcceptance);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetAcceptancePostVoucher(voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
            accountsCommonService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);


            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;
            int colParticulars = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            row++;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            row++;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor");
            reportUtility.SetText(ref sheet, row, 2, header["Vendor"].ToString());
            row++;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor Plant");
            reportUtility.SetText(ref sheet, row, 2, header["VendorPlant"].ToString());
            row++;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());

            colLast = companyCurrencyId == transcationCurrency ? 6 : 8;
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();


            //colLast = companyCurrencyId == transcationCurrency ? 6 : 8;
            //sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();

            row = 5;
            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Voucher Date");
                reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString());
                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
                reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString());
                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
                reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString());
                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Status");
                reportUtility.SetText(ref sheet, row, 5, header["Status"].ToString());
                row++;
                row++;
                reportUtility.SetHeaderText(ref sheet, row, 5, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 5, row, 6].Merge();
                sheet[row, 5, row, 6].BorderAround(ExcelLineStyle.Thin);
            }
            else
            {
                reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Voucher Date");
                reportUtility.SetText(ref sheet, row, 7, header["VoucherDate"].ToString());
                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 6, "DocDate");
                reportUtility.SetText(ref sheet, row, 7, header["DocDate"].ToString());
                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Doc Ref");
                reportUtility.SetText(ref sheet, row, 7, header["DocRefNo"].ToString());
                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Status");
                reportUtility.SetText(ref sheet, row, 7, header["Status"].ToString());
                row++;
                row++;
                reportUtility.SetHeaderText(ref sheet, row, 5, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 5, row, 6].Merge();
                sheet[row, 5, row, 6].BorderAround(ExcelLineStyle.Thin);

                reportUtility.SetHeaderText(ref sheet, row, 7, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 7, row, 8].Merge();
                sheet[row, 7, row, 8].BorderAround(ExcelLineStyle.Thin);
            }
            row++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL", 20, ExcelHAlign.HAlignLeft);
            // reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); 
            colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].BorderAround(ExcelLineStyle.Thin);
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;


            xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars", 5); colParticulars = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colParticulars - 1) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();
            sheet[row, colParticulars - 1].ColumnWidth = 20;
            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 20, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 20, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 20, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 20, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 20, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 20, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();

                    reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());
                    sheet[reportUtility.GetColumnNameForXls(colParticulars - 1) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();


                    if (companyCurrencyId != transcationCurrency)
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

                reportUtility.SetText(ref sheet, row, 1, "Total: ", true);

                sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Thin);

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + 12 + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + 12 + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + 12 + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (row - 1) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + 12 + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (row - 1) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + 12 + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + 12 + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                //sheet.Range[13, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[13, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency)
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

                //sheet.UsedRange.AutofitColumns();
                sheet[1, 2].ColumnWidth = 35;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, colGl, header["AddedBy"].ToString());
                sheet.Range[row, colGl].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet[row, colGl].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                reportUtility.SetTextMiddle(ref sheet, row, colGl, "Prepared By", true);
                if (companyCurrencyId != transcationCurrency)
                {
                    reportUtility.SetSignatureText(ref sheet, row - 1, colParticulars + 1, header["PostedBy"].ToString());
                    sheet.Range[row, colParticulars + 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                    reportUtility.SetTextMiddle(ref sheet, row, colParticulars + 1, "Checked By", true);
                }
                else
                {
                    reportUtility.SetSignatureText(ref sheet, row - 1, colParticulars - 1, header["PostedBy"].ToString());
                    sheet.Range[row, colParticulars - 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                    reportUtility.SetTextMiddle(ref sheet, row, colParticulars - 1, "Checked By", true);
                }

                sheet.Range[row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colLast, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Invoice To Acceptance", companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, "Invoice To Acceptance", companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }
        private Dictionary<string, object> GetAcceptancePostReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.AddedBy, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , P.UserName AS Vendor, PP.UserName AS VendorPlant, BJ.CurrencyId, C.Code AS CurrencyCode
                            FROM [TRN].[InvoiceWriteOff] AS BJ
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=BJ.VoucherId
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [HKP].[Party] AS P ON P.Id=BJ.PartyId
							LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=BJ.PartyPlantId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                            WHERE BJ.Archive=0 AND BJ.CompanyGroupId='" + companyGroupId + "' AND BJ.CompanyId='" + companyId + "' AND BJ.PlantId='" + plantId + "' AND BJ.VoucherId='" + voucherId + "' AND BJ.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }
        private DataTable GetAcceptancePostVoucher(string voucherId)
        {
            try
            {
                var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                            , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS DrAmount, VD.CrAmount AS CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Vendor, PP.UserName AS VendorPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget
                            , ACT.UserName AS Activity, CM.UserName AS CashMasterName
							,[ParticularName]=CASE
								WHEN BKM.AccountTitle<>'' THEN BKM.AccountTitle
								WHEN I.DocRefNo<>'' THEN I.DocRefNo 
								WHEN CM.UserName<>'' THEN CM.UserName
                                WHEN PPN.UserName<>'' THEN PPN.UserName
								ELSE ''	END
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [TRN].[InvoiceWriteOffDetail] AS IVD ON IVD.Id=VD.InvoiceWriteOffDetailId
                            LEFT JOIN [TRN].[InvoiceWriteOff] AS IV ON IV.Id=IVD.InvoiceWriteOffId
                            LEFT JOIN [TRN].[InvoiceDetail] AS ID ON ID.Id=IVD.InvoiceDetailId
                            LEFT JOIN [TRN].[Invoice] AS I ON I.Id=ID.InvoiceId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                            LEFT JOIN [HKP].[PartyPlant] AS PPN ON PPN.Id=VD.PartyPlantId and VD.CrAmount>0
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [MST].[BankMaster] AS BKM ON BKM.Id=VD.BankMasterId
                            WHERE V.Archive=0 AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        /*
  * =====================================================================================
  *  RECEIPTS & EXPENDITURE STATEMENT REPORT
  * =====================================================================================
  *  Mirrors the pattern used in GetWeeklyReceiptAndPaymnetWorkBook (Syncfusion XlsIO,
  *  ReportUtility.PlantHeader/PageSetup, clsStaticInfo, _sqlRepository.GetDataTable).
  *
  *  DATA SOURCE: a single combined query (Receipts CTE FULL OUTER JOINed to Expenses
  *  CTE by ROW_NUMBER) - the same shape as your latest result-set screenshot:
  *      [DATE] [RECEIPTS] [AMOUNT] [DATE] [EXPENSES] [AMOUNT]
  *  Because the result set has two columns named DATE and two named AMOUNT, ADO.NET
  *  auto-renames the duplicates when filling a DataTable (DATE, DATE1, AMOUNT, AMOUNT1).
  *  To avoid relying on that renaming behavior, the code below reads by ORDINAL
  *  position instead of column name:
  *      0 = DATE (receipt side)   1 = RECEIPTS   2 = AMOUNT (receipt side)
  *      3 = DATE (expense side)   4 = EXPENSES   5 = AMOUNT (expense side)
  *
  *  LAYOUT: even though the query row-aligns Receipts and Expenses by RN, the target
  *  Excel layout does NOT render them row-aligned - Expenses fill top-to-bottom
  *  continuously, while Receipts (Opening Balance + actual receipt vouchers) are
  *  pulled out and rendered as a short block near the bottom, with blank rows in
  *  between. So this code reads the single DataTable once, then splits it into two
  *  independent lists for rendering:
  *      - expenseRows  -> every row where the expense columns (3,4,5) are not null
  *      - receiptRows  -> every row where the receipt columns (0,1,2) are not null
  *                        ("Opening Balance" row is detected by its RECEIPTS text and
  *                        rendered bold, without a date, matching the screenshot)
  *
  *  DASHED SEPARATOR LINE:
  *  ---------------------------------------------------------------------------------
  *  A dashed line appears after the 3rd expense row in your sample - it does NOT
  *  correspond to a date change. I could not infer the rule that decides where this
  *  line goes (cash limit cutoff? batch boundary? manual annotation?), so it's
  *  exposed as the `dashedSeparatorAfterRowIndices` parameter - pass in the 0-based
  *  row indices (within the rendered expense list) after which a dashed bottom-border
  *  should be drawn.
  * =====================================================================================
  */


        private const string OPENING_BALANCE_LABEL = "Opening Balance";
        public IWorkbook GetWeeklyReceiptAndPaymnetWorkBook(
          out string reportFileName,
          string companyGroupId,
          string companyId,
          string plantId,
          string plantName,
          DateTime fromDate,
          DateTime toDate, string cashMasterId,
          IEnumerable<int> dashedSeparatorAfterRowIndices = null)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var separatorRows = new HashSet<int>(dashedSeparatorAfterRowIndices ?? new int[0]);

            ExcelEngine excelEngine = new ExcelEngine();
            IApplication application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2013;

            IWorkbook workbook = application.Workbooks.Create(1);
            IWorksheet worksheet = workbook.Worksheets[0];
            worksheet.Name = "WEEKLY RECEIPT AND EXPENSES STATEMENT";
            reportFileName = "WEEKLY RECEIPT AND EXPENSES STATEMENT " + toDate.ToString("dd-MMM-yyyy");

            // ---- Pull + split data -----------------------------------------------------
            DataTable dtCombined = GetCombinedStatementData(plantId, fromDate, toDate,cashMasterId);

            List<ReceiptLine> receiptRows;
            List<ExpenseLine> expenseRows;
            SplitCombinedData(dtCombined, out receiptRows, out expenseRows);

            // ---- Column layout ---------------------------------------------------------
            // 1=DATE(R) 2=VoucherNo(R) 3=RECEIPTS 4=AMOUNT(R) 5=DATE(E) 6=VoucherNo(E) 7=EXPENSES 8=AMOUNT(E)
            const int COL_R_DATE = 1;
            const int COL_R_VNO = 2;
            const int COL_R_DESC = 3;
            const int COL_R_AMT = 4;
            const int COL_E_DATE = 5;
            const int COL_E_VNO = 6;
            const int COL_E_DESC = 7;
            const int COL_E_AMT = 8;
            const int END_COL = COL_E_AMT;

            // ---- Title rows --------------------------------------------------------
            worksheet.Range[1, 1, 1, END_COL].Merge();
            worksheet.Range[1, 1].Text = identity.CompanyName;
            worksheet.Range[1, 1].CellStyle.Font.Bold = true;
            worksheet.Range[1, 1].CellStyle.Font.Size = 14f;
            worksheet.Range[1, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;

            worksheet.Range[2, 1, 2, END_COL].Merge();
            worksheet.Range[2, 1].Text = "EXPENDITURE STATEMENT FROM "
                + fromDate.ToString("dd.MM.yyyy") + " to " + toDate.ToString("dd.MM.yyyy");
            worksheet.Range[2, 1].CellStyle.Font.Bold = true;
            worksheet.Range[2, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;

            // ---- Header row ----------------------------------------------------------
            const int HEADER_ROW = 4;
            worksheet[HEADER_ROW, COL_R_DATE].Text = "DATE";
            worksheet[HEADER_ROW, COL_R_VNO].Text = "VoucherNo";
            worksheet[HEADER_ROW, COL_R_DESC].Text = "RECEIPTS";
            worksheet[HEADER_ROW, COL_R_AMT].Text = "AMOUNT";
            worksheet[HEADER_ROW, COL_E_DATE].Text = "DATE";
            worksheet[HEADER_ROW, COL_E_VNO].Text = "VoucherNo";
            worksheet[HEADER_ROW, COL_E_DESC].Text = "EXPENSES";
            worksheet[HEADER_ROW, COL_E_AMT].Text = "AMOUNT";

            worksheet.Range[HEADER_ROW, 1, HEADER_ROW, END_COL].CellStyle.Font.Bold = true;
            worksheet.Range[HEADER_ROW, 1, HEADER_ROW, END_COL].BorderAround(ExcelLineStyle.Dashed);
            worksheet.Range[HEADER_ROW, 1, HEADER_ROW, END_COL].BorderInside(ExcelLineStyle.Thin);

            worksheet.SetColumnWidth(COL_R_DATE, 10);
            worksheet.SetColumnWidth(COL_R_VNO, 16);
            worksheet.SetColumnWidth(COL_R_DESC, 32);
            worksheet.SetColumnWidth(COL_R_AMT, 14);
            worksheet.SetColumnWidth(COL_E_DATE, 10);
            worksheet.SetColumnWidth(COL_E_VNO, 16);
            worksheet.SetColumnWidth(COL_E_DESC, 40);
            worksheet.SetColumnWidth(COL_E_AMT, 14);

            // ---- Expense block (right side) --------------------------------------------
            int expRow = HEADER_ROW + 1;
            decimal totalExpenseLines = 0;

            for (int i = 0; i < expenseRows.Count; i++)
            {
                ExpenseLine line = expenseRows[i];

                worksheet[expRow, COL_E_DATE].Text = line.PostingDate.ToString("dd.MM.yy");
                worksheet[expRow, COL_E_VNO].Text = line.VoucherNo;
                worksheet[expRow, COL_E_DESC].Text = line.Description;

                worksheet[expRow, COL_E_AMT].Number = (double)line.Amount;
                worksheet[expRow, COL_E_AMT].NumberFormat = "#,##0.00";
               // worksheet[expRow, COL_E_AMT].CellStyle.Color = System.Drawing.Color.FromArgb(198, 224, 180); // green fill

                if (separatorRows.Contains(i))
                {
                    worksheet.Range[expRow, COL_E_DATE, expRow, COL_E_AMT].CellStyle.Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Dashed;
                    worksheet.Range[expRow, COL_E_DATE, expRow, COL_E_AMT].CellStyle.Borders[ExcelBordersIndex.EdgeBottom].Color = ExcelKnownColors.Blue;
                }

                totalExpenseLines += line.Amount;
                expRow++;
            }

            int expenseBlockEndRow = expRow - 1;

            // ---- Receipts block (left side, top-aligned with Expenses) -----------------
            int recRow = HEADER_ROW + 1;
            decimal openingBalance = 0;
            decimal totalOtherReceipts = 0;

            foreach (ReceiptLine line in receiptRows)
            {
                bool isOpeningBalance = string.Equals(line.Description, OPENING_BALANCE_LABEL, StringComparison.OrdinalIgnoreCase);

                if (isOpeningBalance)
                {
                    worksheet[recRow, COL_R_DESC].Text = "OPENING BALANCE";
                    worksheet[recRow, COL_R_DESC].CellStyle.Font.Bold = true;
                    worksheet[recRow, COL_R_AMT].CellStyle.Font.Bold = true;
                    openingBalance = line.Amount;
                    // no date/voucher printed for Opening Balance
                }
                else
                {
                    worksheet[recRow, COL_R_DATE].Text = line.PostingDate.ToString("dd.MM.yy");
                    worksheet[recRow, COL_R_VNO].Text = line.VoucherNo;
                    worksheet[recRow, COL_R_DESC].Text = line.Description;
                    totalOtherReceipts += line.Amount;
                }

                worksheet[recRow, COL_R_AMT].Number = (double)line.Amount;
                worksheet[recRow, COL_R_AMT].NumberFormat = "#,##0.00";

                recRow++;
            }

            int receiptBlockEndRow = recRow - 1;

            // ---- Cash In Hand (balancing figure) = Total Receipts - Total Expenses -----
            decimal totalReceipts = openingBalance + totalOtherReceipts;
            decimal cashInHand = totalReceipts - totalExpenseLines;

            int cashInHandRow = expenseBlockEndRow + 2; // one blank row gap
            worksheet[cashInHandRow, COL_E_DESC].Text = "CASH IN HAND";
            worksheet[cashInHandRow, COL_E_DESC].CellStyle.Font.Bold = true;
            worksheet[cashInHandRow, COL_E_AMT].Number = (double)cashInHand;
            worksheet[cashInHandRow, COL_E_AMT].NumberFormat = "#,##0.00";
            worksheet[cashInHandRow, COL_E_AMT].CellStyle.Font.Bold = true;

            // ---- TOTAL boxes on both sides, aligned on the same row --------------------
            int totalRow = Math.Max(receiptBlockEndRow, cashInHandRow) + 2; // one blank row gap

            decimal totalExpensesGrand = totalExpenseLines + cashInHand; // == totalReceipts by construction

            worksheet[totalRow, COL_R_DESC].Text = "TOTAL";
            worksheet[totalRow, COL_R_DESC].CellStyle.Font.Bold = true;
            worksheet[totalRow, COL_R_AMT].Number = (double)totalReceipts;
            worksheet[totalRow, COL_R_AMT].NumberFormat = "#,##0.00";
            worksheet[totalRow, COL_R_AMT].CellStyle.Font.Bold = true;
            worksheet.Range[totalRow, COL_R_DESC, totalRow, COL_R_AMT].BorderAround(ExcelLineStyle.Dashed);

            worksheet[totalRow, COL_E_DESC].Text = "TOTAL";
            worksheet[totalRow, COL_E_DESC].CellStyle.Font.Bold = true;
            worksheet[totalRow, COL_E_AMT].Number = (double)totalExpensesGrand;
            worksheet[totalRow, COL_E_AMT].NumberFormat = "#,##0.00";
            worksheet[totalRow, COL_E_AMT].CellStyle.Font.Bold = true;
            worksheet.Range[totalRow, COL_E_DESC, totalRow, COL_E_AMT].BorderAround(ExcelLineStyle.Dashed);

            // ---- Cosmetics / page setup, matching your existing report style ----------
            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 9f;
            worksheet.Range[1, 1].CellStyle.Font.Size = 14f;

            string from = fromDate.ToString("yyyy-MM-dd");
            string to = toDate.ToString("yyyy-MM-dd");

            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref worksheet, END_COL, " WEEKLY STATEMENT OF "+ from + " TO " +to, identity.PlantId);
            reportUtility.PageSetup(ref worksheet, HEADER_ROW, ExcelPageOrientation.Landscape);

            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes
            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A" + (HEADER_ROW + 1)].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = HEADER_ROW + 1;
            #endregion

            return workbook;
        }

        // =====================================================================================
        //  DATA SPLITTING
        // =====================================================================================

        private struct ReceiptLine
        {
            public DateTime PostingDate;
            public string VoucherNo;
            public string Description;
            public decimal Amount;
        }

        private struct ExpenseLine
        {
            public DateTime PostingDate;
            public string VoucherNo;
            public string Description;
            public decimal Amount;
        }

        /// <summary>
        /// Splits the single combined DataTable (Receipts FULL OUTER JOINed to Expenses by
        /// RN, now including VoucherNo on both sides) into two independent lists for
        /// rendering. Reads by ordinal position because the result set has duplicate
        /// column names (DATE/DATE, VoucherNo/VoucherNo, AMOUNT/AMOUNT), which ADO.NET
        /// silently renames when filling a DataTable - ordinal access sidesteps that.
        /// </summary>
        private void SplitCombinedData(DataTable dt, out List<ReceiptLine> receiptRows, out List<ExpenseLine> expenseRows)
        {
            receiptRows = new List<ReceiptLine>();
            expenseRows = new List<ExpenseLine>();

            const int COL_R_DATE = 0;
            const int COL_R_VNO = 1;
            const int COL_R_DESC = 2;
            const int COL_R_AMT = 3;
            const int COL_E_DATE = 4;
            const int COL_E_VNO = 5;
            const int COL_E_DESC = 6;
            const int COL_E_AMT = 7;

            foreach (DataRow dr in dt.Rows)
            {
                if (dr[COL_R_DESC] != DBNull.Value && dr[COL_R_DESC] != null)
                {
                    receiptRows.Add(new ReceiptLine
                    {
                        PostingDate = dr[COL_R_DATE] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dr[COL_R_DATE]),
                        VoucherNo = dr[COL_R_VNO] == DBNull.Value ? string.Empty : dr[COL_R_VNO].ToString(),
                        Description = dr[COL_R_DESC].ToString(),
                        Amount = dr[COL_R_AMT] == DBNull.Value ? 0m : Convert.ToDecimal(dr[COL_R_AMT])
                    });
                }

                if (dr[COL_E_DESC] != DBNull.Value && dr[COL_E_DESC] != null)
                {
                    expenseRows.Add(new ExpenseLine
                    {
                        PostingDate = dr[COL_E_DATE] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dr[COL_E_DATE]),
                        VoucherNo = dr[COL_E_VNO] == DBNull.Value ? string.Empty : dr[COL_E_VNO].ToString(),
                        Description = dr[COL_E_DESC].ToString(),
                        Amount = dr[COL_E_AMT] == DBNull.Value ? 0m : Convert.ToDecimal(dr[COL_E_AMT])
                    });
                }
            }
        }

        // =====================================================================================
        //  DATA ACCESS
        // =====================================================================================

        /// <summary>
        /// The combined query exactly as you supplied it (Receipts CTE incl. Opening
        /// Balance, FULL OUTER JOINed to Expenses CTE by ROW_NUMBER). Built via string
        /// concatenation to match your ISqlRepository.GetDataTable(string sql) signature.
        /// </summary>
        private DataTable GetCombinedStatementData(string plantId, DateTime fromDate, DateTime toDate,string cashMasterId)
        {
            string from = fromDate.ToString("yyyy-MM-dd");
            string to = toDate.ToString("yyyy-MM-dd");

            var sql = @"WITH Expenses AS (
    SELECT
        V.PostingDate,V.VoucherNo,HeadOfExpense= case when vd.BankMasterId IS NULL THEN  A.UserName 
        WHEN vd.BankMasterId<>'' THEN 'Cash Deposit ' + ISNULL(' (' + V.Narration + ')','') end,
        VD.DrAmount AS Amount,
        ROW_NUMBER() OVER (ORDER BY V.PostingDate, V.VoucherNo) AS RN,VD.Id
    FROM TRN.VoucherDetail VD
    LEFT JOIN TRN.Voucher V ON V.Id = VD.VoucherId
    LEFT JOIN TRN.VoucherDetailCurrency VDC ON VDC.VoucherDetailId = VD.Id
    LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id = VD.GLGeneralInfoId
    LEFT JOIN HKP.Activity A ON A.Id = VD.ActivityId
    LEFT JOIN MST.BudgetMaster BM ON BM.Id = VD.BudgetMasterId
    LEFT JOIN HKP.AccountGroup AG ON AG.Id = GL.AccountGroupId
	LEFT JOIN (SELECT CashMasterId,VoucherId FROM TRN.VoucherDetail where CrAmount>0 and CashMasterId='" + cashMasterId + @"' )XVD ON XVD.VoucherId=V.Id
   
    WHERE V.PlantId = '" + plantId + @"'
        --AND AG.AccountTypeId IN ('Expense','Asset')
        AND VD.DrAmount > 0
        AND V.PostingDate BETWEEN '"+ from + "' AND '"+to+ @"'
        AND VD.CashMasterId IS NULL AND XVD.CashMasterId='"+ cashMasterId + @"'
        --AND VD.BankMasterId IS NULL
        AND V.SourceType <> 'OpeningBalance'
),
Receipts AS (
    SELECT NULL PostingDate,NULL VoucherNo,
        'Opening Balance' AS Receipt,SUM(X.ReceiptAmount)-SUM(X.ExAmountAmount) Amount ,0 RN,NULL Id
		FROM (
SELECT 
        SUM(VD.DrAmount) AS ReceiptAmount,0 ExAmountAmount
    FROM TRN.VoucherDetail VD
    LEFT JOIN TRN.Voucher V ON V.Id = VD.VoucherId
    LEFT JOIN TRN.VoucherDetailCurrency VDC ON VDC.VoucherDetailId = VD.Id
    LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id = VD.GLGeneralInfoId
    LEFT JOIN HKP.Activity A ON A.Id = VD.ActivityId
    LEFT JOIN MST.BudgetMaster BM ON BM.Id = VD.BudgetMasterId
    LEFT JOIN HKP.AccountGroup AG ON AG.Id = GL.AccountGroupId
    LEFT JOIN MST.CashMaster CM ON CM.Id = VD.CashMasterId
    WHERE V.PlantId = '" + plantId + @"'
        AND VD.DrAmount > 0
        AND VD.CashMasterId <> ''
        AND V.PostingDate < '"+ from + @"'
        AND V.SourceType<>'OpeningBalance' AND VD.CashMasterId='" + cashMasterId + @"'
UNION ALL 
 SELECT 0 ReceiptAmount, SUM(VD.DrAmount) AS ExAmountAmount
    FROM TRN.VoucherDetail VD
    LEFT JOIN TRN.Voucher V ON V.Id = VD.VoucherId
    LEFT JOIN TRN.VoucherDetailCurrency VDC ON VDC.VoucherDetailId = VD.Id
    LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id = VD.GLGeneralInfoId
    LEFT JOIN HKP.Activity A ON A.Id = VD.ActivityId
    LEFT JOIN MST.BudgetMaster BM ON BM.Id = VD.BudgetMasterId
    LEFT JOIN HKP.AccountGroup AG ON AG.Id = GL.AccountGroupId
	LEFT JOIN (SELECT CashMasterId,VoucherId FROM TRN.VoucherDetail where CrAmount>0 and CashMasterId='" + cashMasterId + @"' )XVD ON XVD.VoucherId=V.Id

    WHERE V.PlantId = '" + plantId + @"'
       -- AND AG.AccountTypeId IN ('Expense','Asset')
        AND VD.DrAmount > 0
        AND V.PostingDate < '"+ from + @"'
        AND VD.CashMasterId IS NULL
        AND VD.BankMasterId IS NULL
        AND V.SourceType <> 'OpeningBalance' AND XVD.CashMasterId='" + cashMasterId + @"'
		UNION ALL
		 SELECT SUM(VD.DrAmount) AS ReceiptAmount,0 ExAmountAmount
    FROM TRN.VoucherDetail VD
    LEFT JOIN TRN.Voucher V ON V.Id = VD.VoucherId
    LEFT JOIN TRN.VoucherDetailCurrency VDC ON VDC.VoucherDetailId = VD.Id
    LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id = VD.GLGeneralInfoId
    LEFT JOIN HKP.Activity A ON A.Id = VD.ActivityId
    LEFT JOIN MST.BudgetMaster BM ON BM.Id = VD.BudgetMasterId
    LEFT JOIN HKP.AccountGroup AG ON AG.Id = GL.AccountGroupId
    LEFT JOIN MST.CashMaster CM ON CM.Id = VD.CashMasterId
    WHERE V.PlantId = '" + plantId + @"'
        AND VD.DrAmount > 0
        AND VD.CashMasterId <> ''
        AND V.PostingDate  <= '" + from + @"'
        AND V.SourceType = 'OpeningBalance'  AND VD.CashMasterId='" + cashMasterId + @"'

		) X

	
    UNION ALL
    SELECT
        V.PostingDate,V.VoucherNo,
        CM.UserName + '(' + V.Narration + ')' AS Receipt,
        VD.DrAmount AS Amount,
        ROW_NUMBER() OVER (ORDER BY V.PostingDate, V.VoucherNo) AS RN,VD.Id
    FROM TRN.VoucherDetail VD
    LEFT JOIN TRN.Voucher V ON V.Id = VD.VoucherId
    LEFT JOIN TRN.VoucherDetailCurrency VDC ON VDC.VoucherDetailId = VD.Id
    LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id = VD.GLGeneralInfoId
    LEFT JOIN HKP.Activity A ON A.Id = VD.ActivityId
    LEFT JOIN MST.BudgetMaster BM ON BM.Id = VD.BudgetMasterId
    LEFT JOIN HKP.AccountGroup AG ON AG.Id = GL.AccountGroupId
    LEFT JOIN MST.CashMaster CM ON CM.Id = VD.CashMasterId
    WHERE V.PlantId = '" + plantId + @"'
        AND VD.DrAmount > 0
        AND VD.CashMasterId <> ''
        AND V.PostingDate BETWEEN '"+ from + "' AND '"+to+ @"'
        AND V.SourceType <> 'OpeningBalance'  and VD.CashMasterId='" + cashMasterId + @"'
)
SELECT
    R.PostingDate AS [RDATE],R.VoucherNo RVoucherNo,
    R.Receipt     AS RECEIPTS,
    R.Amount      AS ReceiptAMOUNT,
    E.PostingDate AS [EDATE],E.VoucherNo EVoucherNo,
    E.HeadOfExpense AS [EXPENSES],
    E.Amount      AS PaymentAMOUNT
FROM Receipts R
FULL OUTER JOIN Expenses E ON R.RN = E.RN ORDER BY E.VoucherNo,E.Id";

            return _sqlRepository.GetDataTable(sql.ToString());
        }


    }

}


