using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Service.Accounts;
using Library.Service.Currencies;
using Library.Service.Helpers;
using Library.Service.Organizations;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Library.Service.Invoices
{
    public class InvoiceReportService : IInvoiceReportService
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly ICompanyService _companyService;
        private readonly CompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IPlantService _plantService;

        public InvoiceReportService(
            ISqlRepository sqlRepository
            , ICompanyService companyService
            , CompanyParallelCurrencyService companyParallelCurrencyService
            , IPlantService plantService)
        {
            _sqlRepository = sqlRepository;
            _companyService = companyService;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _plantService = plantService;
        }



        //Customer invoice report New
        public IWorkbook GetCustomerInvoiceReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetCustomerInvoiceHeader(companyGroupId, companyId, plantId, voucherId, SourceType.CustomerInvoice);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetCustomerInvoiceVoucher(voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

        

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

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer:");
            reportUtility.SetText(ref sheet, row, 2, header["Customer"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer Plant");
            reportUtility.SetText(ref sheet, row, 2, header["CustomerPlant"].ToString());
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
                
                formulaEndRow = row -1;
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

                sheet.Range[row, colinrDebit, row , colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, colinrDebit, row , colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency /*&& _plantService.Find(plantId).IsShowFCInWord*/)
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
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked/Posted By", true);
                sheet[row, 3].ColumnWidth = 25;

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId,plantId, plantName, null);
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
                reportUtility.CompanyPlantHeader(ref sheet, 7, header["VoucherTypeName"].ToString(), companyId,plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Portrait);
            }
        
            return workbook;
        }

        //Customer invoice header data old & NEW
        private Dictionary<string, object> GetCustomerInvoiceHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
							,AddedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.AddedBy END
							,PostedBy=CASE WHEN UP.FullName<>'' THEN UP.FullName ELSE V.PostedBy END
							, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , P.UserName AS Customer, PP.UserName AS CustomerPlant, BJ.CurrencyId, C.Code AS CurrencyCode
                            FROM [TRN].[Invoice] AS BJ
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

        private Dictionary<string, object> GetVendorPaymentReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
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

        //customer invoice receipt header old and new data
        private Dictionary<string, object> GetCustomerInvoiceReceiptHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"
                            SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
							, AddedBy=case when u.FullName <>'' then u.FullName else v.AddedBy end
							 ,PostedBy=CASE WHEN UP.FullName<>'' THEN UP.FullName ELSE V.PostedBy END
							, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , P.UserName AS Customer, PP.UserName AS CustomerPlant, BJ.CurrencyId, C.Code AS CurrencyCode
                            FROM [TRN].[InvoiceWriteOff] AS BJ
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=BJ.VoucherId
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [HKP].[Party] AS P ON P.Id=BJ.PartyId
							LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=BJ.PartyPlantId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
							left join SEC.[User] u on u.UserId= v.AddedBy
                            LEFT JOIN SEC.[User] UP ON UP.UserId=V.PostedBy
                            WHERE BJ.Archive=0 AND BJ.CompanyGroupId='" + companyGroupId + "' AND BJ.CompanyId='" + companyId + "' AND BJ.PlantId='" + plantId + "' AND BJ.VoucherId='" + voucherId + "' AND BJ.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }

        //Customer Invoice  data old and New
        private DataTable GetCustomerInvoiceVoucher(string voucherId)
        {
            try
            {
                var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                            , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS DrAmount, VD.CrAmount AS CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget

                            ,Activity=CASE WHEN VD.CashMasterId<>'' THEN  CM.UserName  WHEN VD.BankMasterId<>'' THEN BNM.AccountTitle Else ACT.UserName end 
                            ,CM.UserName AS CashMasterName
                            ,[ParticularName]=CASE
								WHEN BNM.AccountTitle<>'' THEN BNM.AccountTitle
								WHEN I.DocRefNo<>'' THEN I.DocRefNo 
								WHEN CM.UserName<>'' THEN CM.UserName
                                WHEN PP.UserName<>'' THEN PP.UserName
								ELSE ''	END
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [TRN].[InvoiceWriteOffDetail] AS IVD ON IVD.Id=VD.InvoiceWriteOffDetailId
                            LEFT JOIN [TRN].[InvoiceWriteOff] AS IV ON IV.Id=IVD.InvoiceWriteOffId
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

        private DataTable GetVendorPaymentVoucher(string voucherId)
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
								WHEN VD.EmployeeId<>'' THEN EI.EmployeeCode+'-'+EI.EmployeeName
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
                            LEFT JOIN [DBO].[EmployeeInformation] EI ON EI.SystemId=VD.EmployeeId 
                            WHERE V.Archive=0 AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

       

        //Vendor Invoice report new
        public IWorkbook GetVendorInvoiceReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetVendorInvoiceHeader(companyGroupId, companyId, plantId, voucherId, SourceType.VendorInvoice);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetCustomerInvoiceVoucher(voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

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
            reportUtility.SetMasterHeaderText(ref sheet, row,4, "Voucher Date");
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

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor:");
            reportUtility.SetText(ref sheet, row, 2, header["Vendor"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString());
            //sheet[row, 5].ColumnWidth = 15;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor Plant");
            reportUtility.SetText(ref sheet, row, 2, header["VendorPlant"].ToString());

            

            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Status");
            reportUtility.SetText(ref sheet, row, 5, header["Status"].ToString());
            //sheet[row, 5].ColumnWidth = 15;
            row++;
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Entity");
            reportUtility.SetText(ref sheet, row, 5, header["EntityName"].ToString());
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

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
            }
            else
            {

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit",14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit",14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
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
                        reportUtility.SetTextDecimalThree(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetTextDecimalThree(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetTextDecimalThree(ref sheet, row, colusdDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetTextDecimalThree(ref sheet, row, colusdCradit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetTextDecimalThree(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetTextDecimalThree(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);

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
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalThree();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalThree();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalThree();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (formulaEndRow) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalThree();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalThree();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalThree();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[row, colinrDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, colinrDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);

                //sheet.Range[row, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Thin);
                //sheet.Range[row, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Thin);

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
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked/Posted By", true);
                sheet[row, 3].ColumnWidth = 25;

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId,plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

            
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 7, header["VoucherTypeName"].ToString(), companyId,plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }
        public IWorkbook GetIncentiveReceivableInvoiceReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetVendorInvoiceHeader(companyGroupId, companyId, plantId, voucherId, SourceType.ReceivableFromOthers);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetCustomerInvoiceVoucher(voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

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

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer:");
            reportUtility.SetText(ref sheet, row, 2, header["Vendor"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString());
            //sheet[row, 5].ColumnWidth = 15;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer Plant");
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

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
            }
            else
            {

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
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
                        reportUtility.SetTextDecimalThree(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetTextDecimalThree(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetTextDecimalThree(ref sheet, row, colusdDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetTextDecimalThree(ref sheet, row, colusdCradit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetTextDecimalThree(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetTextDecimalThree(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);

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
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalThree();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalThree();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalThree();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (formulaEndRow) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalThree();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalThree();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalThree();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[row, colinrDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, colinrDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);

                //sheet.Range[row, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Thin);
                //sheet.Range[row, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Thin);

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

                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
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


        public IWorkbook GetInvoiceOverheadReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetVendorInvoiceHeader(companyGroupId, companyId, plantId, voucherId, SourceType.InvoiceOverhead);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetCustomerInvoiceVoucher(voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

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

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor");
            reportUtility.SetText(ref sheet, row, 2, header["Vendor"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 4, header["DocRefNo"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor Plant");
            reportUtility.SetText(ref sheet, row, 2, header["VendorPlant"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Status");
            reportUtility.SetText(ref sheet, row, 4, header["Status"].ToString());

            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            //sheet[1, 2].ColumnWidth = 100;

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
            sheet.Range[row, colGl, row, colGl + 2].BorderAround(ExcelLineStyle.Hair);
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
                sheet[1, 2].ColumnWidth = 60;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 2, header["PostedBy"].ToString());
                sheet.Range[row, 2].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 2, "Checked/Posted By", true);

                sheet.Range[row, 4].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 4, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Invoice Overhead", companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, "Invoice Overhead", companyId,plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }
        private Dictionary<string, object> GetVendorInvoiceHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"
                            SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
							, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
							, AddedBy =case when u.FullName<>'' then u.FullName else v.AddedBy end
							,PostedBy = case when up.FullName<>'' then up.FullName else v.PostedBy end

							--, AddedBy =case when u.FullName<>'' then u.FullName else v.AddedBy end
							--,PostedBy = case when u.FullName<>'' then u.FullName else v.PostedBy end
							 , UPPER(V.Narration) AS Narration 
					    	, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , P.UserName AS Vendor,PP.UserName+' - '+'GSTIN:'+'('+PP.GSTIN+')' AS VendorPlant
                            , BJ.CurrencyId, C.Code AS CurrencyCode,P.TINNO GSTINNo,E.UserName EntityName
                            FROM [TRN].[Invoice] AS BJ
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=BJ.VoucherId
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [HKP].[Party] AS P ON P.Id=BJ.PartyId
							LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=BJ.PartyPlantId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
							LEFT JOIN [SEC].[User] U on U.UserId=v.AddedBy
							LEFT JOIN [SEC].[User] up on up.UserId=v.PostedBy
							LEFT JOIN [ORG].[Entity] E on E.Id=BJ.EntityId
                            WHERE BJ.Archive=0 --AND BJ.CompanyGroupId='" + companyGroupId + @"' AND BJ.CompanyId='" + companyId + @"' AND BJ.PlantId='" + plantId + @"' 
                            AND BJ.VoucherId='" + voucherId + @"' AND BJ.SourceType='" + sourceType + @"'";
            return _sqlRepository.GetData(cmdText);
        }

        private DataTable GetVendorInvoiceVoucher(string voucherId)
        {
            try
            {
                var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                            , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS DrAmount, VD.CrAmount AS CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Vendor, PP.UserName AS VendorPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget
                            , ACT.UserName AS Activity, CM.UserName AS CashMasterName
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [TRN].[InvoiceDetail] AS IVD ON IVD.Id=VD.InvoiceDetailId
                            LEFT JOIN [TRN].[Invoice] AS IV ON IV.VoucherId=V.Id
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
                            WHERE V.Archive=0 AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        

        public IWorkbook GetVendorPaymentReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetVendorPaymentReportHeader(companyGroupId, companyId, plantId, voucherId, SourceType.VendorPayment);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetVendorPaymentVoucher(voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

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
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor");
            reportUtility.SetText(ref sheet, row, 2, header["Vendor"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor Plant");
            reportUtility.SetText(ref sheet, row, 2, header["VendorPlant"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Status");
            reportUtility.SetText(ref sheet, row, 5, header["Status"].ToString());

            row++;
            colLast = companyCurrencyId == transcationCurrency ? 6 : 8;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 5, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 5, row, 6].Merge();
                sheet[row, 5, row, 6].BorderAround(ExcelLineStyle.Thin);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 5, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 5, row, 6].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
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
                    sheet[reportUtility.GetColumnNameForXls(colParticulars -1) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();


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

                sheet.Range[13, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[13, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Hair);

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

                //sheet.UsedRange.AutofitColumns();
                sheet[1, 2].ColumnWidth = 35;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, colGl, header["AddedBy"].ToString());
                sheet.Range[row, colGl].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                sheet[row, colGl].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                reportUtility.SetTextMiddle(ref sheet, row, colGl, "Prepared By", true);
               
                reportUtility.SetSignatureText(ref sheet, row - 1, colParticulars - 1, header["PostedBy"].ToString());
                sheet.Range[row, colParticulars - 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colParticulars-1, "Checked/Posted By", true);
                
                sheet.Range[row, 6].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colLast, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, 6, header["VoucherTypeName"].ToString(), companyId,plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
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



        //Customer invoice receipt report new
        public IWorkbook GetCustomerInvoiceReceiptReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetCustomerInvoiceReceiptHeader(companyGroupId, companyId, plantId, voucherId, SourceType.CustomerReceipt);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetCustomerInvoiceVoucher(voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

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

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer:");
            reportUtility.SetText(ref sheet, row, 2, header["Customer"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString());
            //sheet[row, 5].ColumnWidth = 15;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer Plant");
            reportUtility.SetText(ref sheet, row, 2, header["CustomerPlant"].ToString());
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

            
            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
                sheet[row, 6, row, 7].BorderAround(ExcelLineStyle.Thin);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 6, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 7, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 8, row, 9].Merge();
                sheet[row, 8, row, 9].BorderAround(ExcelLineStyle.Thin);
            }
            
            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL", 20, ExcelHAlign.HAlignLeft);
            colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge(); xlsCol++;

            xlsCol++;
            xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars", 5); colParticulars = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colParticulars - 1) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();
            sheet[row, colParticulars - 1].ColumnWidth = 20;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;
                

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, colinrDebit, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            else
            {

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, colinrDebit, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

            }


            int formulaStartRow = 0;
            int formulaEndRow = 0;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++; //?? 12
                //sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Thin);
                //sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Thin);
                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 2) + row].Merge();

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

                    sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);

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

                //sheet.Range[row, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Thin);
                //sheet.Range[row, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Thin);

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
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked/Posted By", true);
                sheet[row, 3].ColumnWidth = 25;

                sheet.Range[row, 6].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 6, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
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
                reportUtility.CompanyPlantHeader(ref sheet, 7, header["VoucherTypeName"].ToString(), companyId,plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }

        //Govt Subsidy
        private Dictionary<string, object> GetCustomerInvoiceReceiptGovtSubsidyHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"
                            SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
							, AddedBy=case when u.FullName <>'' then u.FullName else v.AddedBy end
							, PostedBy= case when u.FullName <>'' then u.FullName else v.PostedBy end
							, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , P.UserName AS Customer, PP.UserName AS CustomerPlant, BJ.CurrencyId, C.Code AS CurrencyCode
                            FROM [TRN].[InvoiceWriteOff] AS BJ
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=BJ.VoucherId
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [HKP].[Party] AS P ON P.Id=BJ.PartyId
							LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=BJ.PartyPlantId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
							left join SEC.[User] u on u.UserId= v.AddedBy
                            WHERE BJ.Archive=0 AND BJ.CompanyGroupId='" + companyGroupId + "' AND BJ.CompanyId='" + companyId + "' AND BJ.PlantId='" + plantId + "' AND BJ.VoucherId='" + voucherId + "' AND BJ.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }

        private DataTable GetCustomerInvoiceGovtSubsidyVoucher(string voucherId)
        {
            try
            {
                var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                            , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS DrAmount, VD.CrAmount AS CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,P.Code+' - '+ P.UserName AS Customer, PP.UserName AS CustomerPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget

                            ,Activity=CASE WHEN VD.CashMasterId<>'' THEN  CM.UserName  WHEN VD.BankMasterId<>'' THEN BNM.AccountTitle Else ACT.UserName end 
                            ,CM.UserName AS CashMasterName

                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [TRN].[InvoiceWriteOffDetail] AS IVD ON IVD.Id=VD.InvoiceWriteOffDetailId
                            LEFT JOIN [TRN].[InvoiceWriteOff] AS IV ON IV.Id=IVD.InvoiceWriteOffId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId
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
                            WHERE V.Archive=0 AND V.Id='"+voucherId+@"' ORDER BY VD.DrAmount DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetCustomerInvoiceReceiptGovtSubsidyReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetCustomerInvoiceReceiptGovtSubsidyHeader(companyGroupId, companyId, plantId, voucherId, SourceType.CustomerReceipt);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetCustomerInvoiceGovtSubsidyVoucher(voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

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
            //sheet[row, 1].ColumnWidth = 25;
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
            //sheet[row, 5].ColumnWidth = 15;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer:");
            reportUtility.SetText(ref sheet, row, 2, header["Customer"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString());
            //sheet[row, 5].ColumnWidth = 15;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer Plant");
            reportUtility.SetText(ref sheet, row, 2, header["CustomerPlant"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Status");
            reportUtility.SetText(ref sheet, row, 5, header["Status"].ToString());
            //sheet[row, 5].ColumnWidth = 15;
            row++;

            colLast = companyCurrencyId == transcationCurrency ? 6 : 8;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet[row, 2].ColumnWidth = 30;

            row++;  //10

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 5, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 5, row, 6].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 5, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 5, row, 6].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 7, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 7, row, 8].Merge();
            }
            sheet[row, 7].ColumnWidth = 15;
            sheet[row, 8].ColumnWidth = 15;
            sheet.Range[row, 5, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, 5, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;


            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();
            //sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Thin);
            //sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Thin);
            xlsCol++;
            xlsCol++;

            int colParticulas = xlsCol;
            reportUtility.SetHeaderText(ref sheet, row, colParticulas, "Particulars"); colParticulas = xlsCol; xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;


                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, colinrDebit, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            else
            {

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, colinrDebit, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

            }


            int formulaStartRow = 0;
            int formulaEndRow = 0;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++; //?? 12
                //sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Thin);
                //sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Thin);
                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 2) + row].Merge();
                    reportUtility.SetText(ref sheet, row, colParticulas, dsLocal.Rows[i]["Customer"].ToString());


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

                    sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);

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

                //sheet.Range[row, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Thin);
                //sheet.Range[row, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Thin);

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
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked/Posted By", true);
                sheet[row, 3].ColumnWidth = 25;

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Govt. Subsidy", /*header["VoucherTypeName"].ToString(),*/ companyId, plantName, null);
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
                reportUtility.CompanyPlantHeader(ref sheet, 7, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }

        public IWorkbook GetInvoiceChargesReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetCustomerInvoiceReceiptHeader(companyGroupId, companyId, plantId, voucherId, SourceType.InvoiceCharge);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetCustomerInvoiceVoucher(voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

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
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
            sheet.Range[row, colGl, row, colGl + 2].BorderAround(ExcelLineStyle.Hair);
            xlsCol++;

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
                sheet[1, 2].ColumnWidth = 60;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 2, header["PostedBy"].ToString());
                sheet.Range[row, 2].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 2, "Checked/Posted By", true);

                sheet.Range[row, 4].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 4, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantId , plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantId , plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }

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

        private DataTable GetCustomerCheckByCompany(string companyId)
        {
            var sql = @"SELECT IsVoucherFromBudget, IsBudgetPeriod, IsCostCenterApplicable, IsProfitCenterApplicable
		                FROM [ORG].[Company]
		                WHERE Id='" + companyId + @"' AND Active=1 AND Archive=0";
            return _sqlRepository.GetDataTable(sql);
        }

        private DataTable GetVendorInvoicePayment(string companyGroupId, string companyId, string voucherId)
        {
            var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate
                    , [Park/Post]=CASE WHEN v.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') DocDate, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') VoucherDate
                    , V.VoucherNo, V.Narration, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
                    , VD.DrAmount+VD.CrAmount AS Value, VDC.DrAmount, VDC.CrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode AS GLGeneralInfoCode
                    , Replace(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') InvoiceDate, VD.DocRefNo AS InvoiceNo, P.UserName AS Customer, '('+BN.UserName +' - ' AS Bank, BR.UserName +' - 'AS Branch, BM.AccountNumber +')' AS AccountNumber
                    , +GL.AccountCode+' - '+GL.UserName+' - '+BM.AccountTitle AS BankMain, +GL.AccountCode+' - '+GL.UserName+' - '+CM.UserName AS CashMain, VD.RefCode AS Ref, VD.Narration AS DetailNarration
                    , ENT.UserName AS Entity, BUD.UserName AS Budget, ACT.UserName AS Activity
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT JOIN TRN.InvoiceWriteOffDetail AS IWD ON IWD.Id=VD.InvoiceWriteOffDetailId
                    LEFT JOIN TRN.InvoiceWriteOff AS IW ON IW.Id=IWD.InvoiceWriteOffId
                    LEFT JOIN HKP.Party AS P ON P.Id=IW.PartyId
                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN SCS.Currency AS CU1 ON CU1.Id=V.CurrencyId
                    LEFT JOIN SCS.FiscalYear AS FY ON FY.Id=V.FiscalYearId
                    LEFT JOIN SCS.FiscalYearPeriod AS FYP ON FYP.Id=V.FiscalYearPeriodId
                    LEFT JOIN [MST].[BankMaster] AS BM ON BM.id=VD.BankMasterId
                    LEFT JOIN [MST].[CashMaster] AS CM ON CM.id=VD.CashMasterId
                    LEFT JOIN [HKP].[Bank] BN ON BN.Id=BM.BankId
                    LEFT JOIN [HKP].[BankBranch] BR ON BR.Id=BM.BankBranchId
                    LEFT JOIN MST.BudgetMaster BUM ON VD.BudgetMasterId=BUM.Id
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUM.BudgetId
                    LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                    LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
                    WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.SourceType='" + SourceType.VendorPayment + "' AND V.Id='" + voucherId + "'";
            return _sqlRepository.GetDataTable(sql);
        }
        private DataTable GetCustomerInvoiceReceipt(string companyGroupId, string companyId, string voucherId, string sourceType)
        {
            var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') PostingDate
                    , [Park/Post]=CASE WHEN v.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') DocDate, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') VoucherDate
                    , V.VoucherNo, V.Narration, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
                    , VD.DrAmount+VD.CrAmount AS Value, VDC.DrAmount, VDC.CrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId,GL.UserName AS GL,GL.AccountCode AS GLGeneralInfoCode
                    , Replace(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') InvoiceDate, VD.DocRefNo AS InvoiceNo, P.UserName AS Customer, '('+BN.UserName +' - ' AS Bank, BR.UserName +' - 'AS Branch, BM.AccountNumber +')' AS AccountNumber
                    , +GL.AccountCode+' - '+GL.UserName+' - '+BM.AccountTitle AS BankMain, +GL.AccountCode+' - '+GL.UserName+' - '+CM.UserName AS CashMain, VD.RefCode AS Ref, VD.Narration AS DetailNarration
                    , ENT.UserName AS Entity, BUD.UserName AS Budget, ACT.UserName AS Activity
                    FROM TRN.VoucherDetailCurrency AS VDC
                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
                    LEFT JOIN TRN.InvoiceWriteOffDetail AS IWD ON IWD.Id=VD.InvoiceWriteOffDetailId
                    LEFT JOIN TRN.InvoiceWriteOff AS IW ON IW.Id=IWD.InvoiceWriteOffId
                    LEFT JOIN HKP.Party AS P ON P.Id=IW.PartyId
                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                    LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
                    LEFT JOIN SCS.Currency AS CU1 ON CU1.Id=V.CurrencyId
                    LEFT JOIN SCS.FiscalYear AS FY ON FY.Id=V.FiscalYearId
                    LEFT JOIN SCS.FiscalYearPeriod AS FYP ON FYP.Id=V.FiscalYearPeriodId
                    LEFT JOIN [MST].[BankMaster] AS BM ON BM.id=VD.BankMasterId
                    LEFT JOIN [MST].[CashMaster] AS CM ON CM.id=VD.CashMasterId
                    LEFT JOIN [HKP].[Bank] BN ON BN.Id=BM.BankId
                    LEFT JOIN [HKP].[BankBranch] BR ON BR.Id=BM.BankBranchId
                    LEFT JOIN MST.BudgetMaster BUM ON VD.BudgetMasterId=BUM.Id
                    LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUM.BudgetId
                    LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                    LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
                    WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.SourceType='" + sourceType + "' AND V.Id='" + voucherId + "'";
            return _sqlRepository.GetDataTable(sql);
        }

        private DataTable GetCustomerForVendorInvoicePayment(string voucherId)
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

        public static int GetCurrencyColIndex(ArrayList al, string paraCar)
        {
            var result = 0;
            try
            {
                for (int i = 0; i < al.Count; i++)
                {
                    var v = (Dictionary<string, int>)al[i];
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

        public IWorkbook GetCustomerInvoiceReceive(string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                var sheet1 = workbook.Worksheets[0];
                CreateSheetReceive(ref sheet1, reportUtility, "Payment Receipt", "Report", companyGroupId, companyId, plantId, plantName, voucherId);
                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateSheetReceive(ref IWorksheet sheet, ReportUtility reportUtility, string sheetHeader, string sheetName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            DataTable dtGeneralVoucher = null;
            DataTable dtCustomerCheckByCompany = null;

            #region List data

            var GeneralVoucherList = GetCustomerInvoiceReceive(companyGroupId, companyId, plantId, voucherId);
            dtGeneralVoucher = GeneralVoucherList;

            var CustomerCheckByCompanyList = GetCustomerCheckByCompany(companyId);
            dtCustomerCheckByCompany = CustomerCheckByCompanyList;
            if (dtGeneralVoucher.Rows.Count == 0)
                throw new Exception("No Data Found!");

            var dvAccountCode = new DataView(GeneralVoucherList);
            var dtAccountCode = dvAccountCode.ToTable(true, "GLGeneralInfoCode", "AccountCodeId");

            var dvParallelCurrency = new DataView(GeneralVoucherList);
            var dtParallelCurrency = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");

            #region CustomerName

            DataTable dtGetCustomerForInvoiceReceive = null;
            var getCustomerForInvoiceReceiveList = GetCustomerForInvoiceReceive(voucherId);
            dtGetCustomerForInvoiceReceive = getCustomerForInvoiceReceiveList;

            #endregion CustomerName

            var dvMainBody = new DataView(GeneralVoucherList)
            {
                Sort = "DRCR, Value DESC"
            };
            var dtMainBody = dvMainBody.ToTable(true, "VoucherDetailId", "Park/Post", "IsPark", "GLGeneralInfoCode", "GL", "Bank", "Branch", "AccountNumber", "DetailNarration", "Ref", "InvoiceNo", "InvoiceDate", "TrnCurrency", "Value", "DRCR", "Entity", "Budget", "Activity", "Cost Center", "Budget Fiscal Year", "Budget Fiscal Year Period", "Budget Period No", "BankMain");

            #region Customer Check By Company

            var dvCustomerCheckByCompanyBody = new DataView(CustomerCheckByCompanyList);
            var dtCustomerCheckByCompanyBody = dvCustomerCheckByCompanyBody.ToTable(false, "IsVoucherFromBudget", "IsBudgetPeriod", "IsCostCenterApplicable", "IsProfitCenterApplicable");
            var Budget = dtCustomerCheckByCompanyBody.Rows[0]["IsVoucherFromBudget"].ToString();
            var BudgetPeriod = dtCustomerCheckByCompanyBody.Rows[0]["IsBudgetPeriod"].ToString();
            var CostCenter = dtCustomerCheckByCompanyBody.Rows[0]["IsCostCenterApplicable"].ToString();
            var ProfitCenter = dtCustomerCheckByCompanyBody.Rows[0]["IsProfitCenterApplicable"].ToString();

            #endregion Customer Check By Company

            #endregion List data

            var _col = 1;
            var _row = 5;
            var shet2EndxlsCol = _col;

            var _col3 = 3;

            reportUtility.SetMasterHeaderText(ref sheet, _row, _col, "Voucher No");
            sheet[reportUtility.GetColumnNameForXls(_col) + _row + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _row].Merge();
            reportUtility.SetText(ref sheet, _row, _col + 2, dtGeneralVoucher.Rows[0]["VoucherNo"].ToString()); _row++;
            sheet[reportUtility.GetColumnNameForXls(_col3) + _row + ":" + reportUtility.GetColumnNameForXls(_col3 + 2) + _row].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, _row, _col, "Doc Date");
            sheet[reportUtility.GetColumnNameForXls(_col) + _row + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _row].Merge();
            reportUtility.SetText(ref sheet, _row, _col + 2, dtGeneralVoucher.Rows[0]["DocDate"].ToString()); _row++;
            sheet[reportUtility.GetColumnNameForXls(_col3) + _row + ":" + reportUtility.GetColumnNameForXls(_col3 + 2) + _row].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, _row, _col, "Posting Date");
            sheet[reportUtility.GetColumnNameForXls(_col) + _row + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _row].Merge();
            reportUtility.SetText(ref sheet, _row, _col + 2, dtGeneralVoucher.Rows[0]["PostingDate"].ToString()); _row++;
            sheet[reportUtility.GetColumnNameForXls(_col3) + _row + ":" + reportUtility.GetColumnNameForXls(_col3 + 2) + _row].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, _row, _col, "Customer");
            sheet[reportUtility.GetColumnNameForXls(_col) + _row + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _row].Merge();
            reportUtility.SetText(ref sheet, _row, _col + 2, dtGetCustomerForInvoiceReceive.Rows[0]["Customer"].ToString()); _row++;
            sheet[reportUtility.GetColumnNameForXls(_col3) + _row + ":" + reportUtility.GetColumnNameForXls(_col3 + 2) + _row].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, _row, _col, "Narration");
            sheet[reportUtility.GetColumnNameForXls(_col) + _row + ":" + reportUtility.GetColumnNameForXls(_col + 1) + _row].Merge();
            reportUtility.SetText(ref sheet, _row, _col + 2, dtGeneralVoucher.Rows[0]["Narration"].ToString()); _row++;
            sheet[reportUtility.GetColumnNameForXls(_col3) + _row + ":" + reportUtility.GetColumnNameForXls(_col3 + 2) + _row].Merge();

            var _rowR = 5;
            var _colR = 6;
            var _col8 = 8;

            reportUtility.SetMasterHeaderText(ref sheet, _rowR, _colR, "Voucher Date");
            sheet[reportUtility.GetColumnNameForXls(_colR) + _rowR + ":" + reportUtility.GetColumnNameForXls(_colR + 1) + _rowR].Merge();
            reportUtility.SetText(ref sheet, _rowR, _colR + 2, dtGeneralVoucher.Rows[0]["VoucherDate"].ToString()); _rowR++;
            sheet[reportUtility.GetColumnNameForXls(_col8) + _rowR + ":" + reportUtility.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, _rowR, _colR, "Doc No");
            sheet[reportUtility.GetColumnNameForXls(_colR) + _rowR + ":" + reportUtility.GetColumnNameForXls(_colR + 1) + _rowR].Merge();
            reportUtility.SetText(ref sheet, _rowR, _colR + 2, dtGeneralVoucher.Rows[0]["DocRefNo"].ToString()); _rowR++;
            sheet[reportUtility.GetColumnNameForXls(_col8) + _rowR + ":" + reportUtility.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, _rowR, _colR, "Fiscal Year");
            sheet[reportUtility.GetColumnNameForXls(_colR) + _rowR + ":" + reportUtility.GetColumnNameForXls(_colR + 1) + _rowR].Merge();
            reportUtility.SetText(ref sheet, _rowR, _colR + 2, dtGeneralVoucher.Rows[0]["FiscalYearName"].ToString()); _rowR++;
            sheet[reportUtility.GetColumnNameForXls(_col8) + _rowR + ":" + reportUtility.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, _rowR, _colR, "Fiscal Year Period");
            sheet[reportUtility.GetColumnNameForXls(_colR) + _rowR + ":" + reportUtility.GetColumnNameForXls(_colR + 1) + _rowR].Merge();
            reportUtility.SetText(ref sheet, _rowR, _colR + 2, dtGeneralVoucher.Rows[0]["PeriodName"] + " (" + dtGeneralVoucher.Rows[0]["PeriodNo"] + ")"); _rowR++;
            sheet[reportUtility.GetColumnNameForXls(_col8) + _rowR + ":" + reportUtility.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, _rowR, _colR, "Park/ Post");
            sheet[reportUtility.GetColumnNameForXls(_colR) + _rowR + ":" + reportUtility.GetColumnNameForXls(_colR + 1) + _rowR].Merge();
            reportUtility.SetText(ref sheet, _rowR, _colR + 2, dtGeneralVoucher.Rows[0]["Park/Post"].ToString()); _row++;
            sheet[reportUtility.GetColumnNameForXls(_col8) + _rowR + ":" + reportUtility.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

            var _rowL = 11;
            _rowL++;

            var headreColIndex = 1;
            var mainColIndex = 1;

            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "GL", 32);
            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;

            if (Budget == "True")
            {
                reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, nameof(Budget), 22);
                sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
                reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Activity", 22);
                sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
            }

            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Detail Narration", 12);
            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Doc Ref No", 10);
            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Doc Date", 10);
            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;

            if (BudgetPeriod == "True")
            {
                reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Budget Fiscal Year", 8);
                sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
                reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Budget Fiscal Year Period", 8);
                sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
            }

            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Currency", 7);
            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;
            reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Trn Value", 10, ExcelHAlign.HAlignRight);
            sheet[_rowL - 1, headreColIndex, _rowL, headreColIndex].Merge(); headreColIndex++;

            double _Total_Amount = 0;
            var plCurrencyId = string.Empty;
            var plCurrencyCode = string.Empty;
            var alParaCurrency = new ArrayList();

            for (int n = 0; n < dtParallelCurrency.Rows.Count; n++)
            {
                reportUtility.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, dtParallelCurrency.Rows[n]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].Merge();

                var dic = new Dictionary<string, int>
                {
                    { dtParallelCurrency.Rows[n]["ParallelCurrencyId"].ToString(), headreColIndex }
                };
                alParaCurrency.Add(dic);
                reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", 10, ExcelHAlign.HAlignRight); headreColIndex++;
                reportUtility.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", 10, ExcelHAlign.HAlignRight); headreColIndex++;

                if (n == 0)
                {
                    plCurrencyCode = dtParallelCurrency.Rows[n]["CurrencyCode"].ToString();
                }
            }
            shet2EndxlsCol = headreColIndex - 1;

            double vAmount = 0;
            var drcrCol = 0;
            var totCol = 0;
            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < dtMainBody.Rows.Count; n++)
            {
                _rowL++;
                var AccountCodeId = dtMainBody.Rows[n]["GLGeneralInfoCode"].ToString();
                var _VoucherDetailId = dtMainBody.Rows[n]["VoucherDetailId"].ToString();
                var Bank = dtMainBody.Rows[n]["BankMain"].ToString();

                if (!string.IsNullOrEmpty(Bank))
                {
                    reportUtility.SetText(ref sheet, _rowL, mainColIndex, Bank); mainColIndex++;
                }
                else
                {
                    reportUtility.SetText(ref sheet, _rowL, mainColIndex, AccountCodeId + " - " + dtMainBody.Rows[n]["GL"]); mainColIndex++;
                }

                if (Budget == "True")
                {
                    reportUtility.SetText(ref sheet, _rowL, mainColIndex, dtMainBody.Rows[n][nameof(Budget)].ToString()); mainColIndex++;
                    reportUtility.SetText(ref sheet, _rowL, mainColIndex, dtMainBody.Rows[n]["Activity"].ToString()); mainColIndex++;
                }
                reportUtility.SetText(ref sheet, _rowL, mainColIndex, dtMainBody.Rows[n]["DetailNarration"].ToString()); mainColIndex++;
                reportUtility.SetText(ref sheet, _rowL, mainColIndex, dtMainBody.Rows[n]["InvoiceNo"].ToString()); mainColIndex++;
                reportUtility.SetText(ref sheet, _rowL, mainColIndex, dtMainBody.Rows[n]["InvoiceDate"].ToString()); mainColIndex++;

                if (BudgetPeriod == "True")
                {
                    reportUtility.SetText(ref sheet, _rowL, mainColIndex, dtMainBody.Rows[n]["Budget Fiscal Year"].ToString()); mainColIndex++;
                    reportUtility.SetText(ref sheet, _rowL, mainColIndex, dtMainBody.Rows[n]["Budget Fiscal Year Period"] + " (" + dtMainBody.Rows[n]["Budget Period No"] + ")"); mainColIndex++;
                }
                reportUtility.SetText(ref sheet, _rowL, mainColIndex, dtMainBody.Rows[n]["TrnCurrency"].ToString()); mainColIndex++;
                reportUtility.SetText(ref sheet, _rowL, mainColIndex, Convert.ToDouble(dtMainBody.Rows[n]["Value"]));

                vAmount += Convert.ToDouble(dtMainBody.Rows[n]["Value"].ToString());
                drcrCol = mainColIndex;
                totCol = mainColIndex;

                for (int p = 0; p < dtParallelCurrency.Rows.Count; p++)
                {
                    var ParallelCurrencyId = dtParallelCurrency.Rows[p]["ParallelCurrencyId"].ToString();
                    var dvDrCr = new DataView(GeneralVoucherList)
                    {
                        RowFilter = "ParallelCurrencyId='" + ParallelCurrencyId + "' AND GLGeneralInfoCode='" + AccountCodeId + "' AND VoucherDetailId='" + _VoucherDetailId + "'"
                    };

                    if (p == 0)
                    {
                        plCurrencyId = dtParallelCurrency.Rows[p][nameof(ParallelCurrencyId)].ToString();
                    }

                    var _pcCol = GetCurrencyColIndex(alParaCurrency, ParallelCurrencyId);
                    var dtDrCr = dvDrCr.ToTable();
                    if (dtDrCr.Rows.Count != 0)
                    {
                        reportUtility.SetText(ref sheet, _rowL, _pcCol, Convert.ToDouble(dtDrCr.Rows[0]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, _rowL, _pcCol + 1, Convert.ToDouble(dtDrCr.Rows[0]["CrAmount"].ToString()));
                        if (p == 0)
                        {
                            _Total_Amount += Convert.ToDouble(dtDrCr.Rows[0]["DrAmount"].ToString());
                        }
                    }
                }
                mainColIndex = 1;
            }

            #region sumCalc

            _rowL++;
            var sumdrcrCol = totCol;
            sheet.Range[reportUtility.GetColumnNameForXls(1) + _rowL + ":" + reportUtility.GetColumnNameForXls(totCol - 1) + _rowL].Merge();
            sheet.Range[_rowL, totCol].Text = "Total ";
            sheet.Range[_rowL, totCol].CellStyle.Font.Bold = true;
            sheet.Range[_rowL, totCol].BorderAround(ExcelLineStyle.Hair);

            //DR
            for (int s = 0; s < dtParallelCurrency.Rows.Count; s++)
            {
                sumdrcrCol++;
                sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
                sheet.Range[_rowL, sumdrcrCol].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);

                sumdrcrCol++;
                sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
                sheet.Range[_rowL, sumdrcrCol].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
            }

            #endregion sumCalc

            var _Currency = string.Empty;
            var _Currency2 = string.Empty;
            var _CurrencyId = string.Empty;
            var _CurrencyId2 = string.Empty;

            _Currency = dtGeneralVoucher.Rows[0]["TrnCurrency"].ToString();
            _CurrencyId = dtGeneralVoucher.Rows[0]["CurrencyId"].ToString();

            sheet.Range[13, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);

            #region InWord

            vAmount = vAmount / 2;
            var _amountValue = reportUtility.InWord(vAmount, _CurrencyId);
            var _amount = reportUtility.InWord(_Total_Amount, plCurrencyId);

            _rowL += 1;

            reportUtility.SetText(ref sheet, _rowL, _col, "In Word:", true);
            _col = 2;
            sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].Text = _amount;
            sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL + ":" + reportUtility.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
            sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[reportUtility.GetColumnNameForXls(_col) + _rowL].CellStyle.Font.Bold = true;

            #endregion InWord

            _rowL = _rowL + 4;

            #region Signature

            sheet.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[_rowL, 4].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[_rowL, 10].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

            reportUtility.SetText(ref sheet, _rowL, 1, "Prepared By", true);
            reportUtility.SetText(ref sheet, _rowL, 4, "Checked By", true);
            reportUtility.SetText(ref sheet, _rowL, 10, "HOD (Finance)", true);

            #endregion Signature

            sheet.Name = sheetName;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            reportUtility.CompanyPlantHeader(ref sheet, shet2EndxlsCol, sheetHeader, identity.CompanyId, plantName, null);
            reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
        }

        private DataTable GetCustomerInvoiceReceive(string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, V.SourceType, Replace(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate
                        , [Park/Post]=CASE WHEN v.IsPark=1 THEN 'Park' ELSE 'Post' END, Replace(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, Replace(CONVERT(VARCHAR(11), v.VoucherDate, 106), ' ', '-') AS VoucherDate
                        , V.VoucherNo, V.Narration, V.CurrencyId,CU1.Code AS TrnCurrency, CU2.Code AS TrnCurrency2, VD.CurrencyId AS DetailCurrencyId, V.AddedBy AS PreparedBy, VDC.ParallelCurrencyId
                        , CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount+VD.CrAmount AS Value, VDC.DrAmount, VDC.CrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                        , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, Replace(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS InvoiceDate, VD.DocRefNo AS InvoiceNo, P.UserName AS Customer
                        , '('+BN.UserName +' - ' AS Bank, BR.UserName +' - 'AS Branch, BM.AccountNumber +')' AS AccountNumber, GL.UserName+' - '+BM.AccountTitle+' - '+BM.AccountNumber AS BankMain, VD.RefCode AS Ref
                        , VD.Narration AS DetailNarration, CO.UserName AS CompanyName,AM.Address1 AS AddressLine, ENT.UserName AS Entity, BUD.UserName AS Budget, ACT.UserName AS Activity, CST.UserName AS [Cost Center]
                        , BFY.FiscalYearName AS [Budget Fiscal Year], BFYP.PeriodName AS [Budget Fiscal Year Period], BFYP.PeriodNo AS [Budget Period No]
                        FROM [TRN].[VoucherDetailCurrency] AS VDC
                        INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id =VDC.VoucherDetailId
                        INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        LEFT JOIN [TRN].[InvoiceWriteOffDetail] AS IWD ON IWD.Id=VD.InvoiceWriteOffDetailId
                        LEFT JOIN [TRN].[InvoiceWriteOff] AS IW ON IW.Id=IWD.InvoiceWriteOffId
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=IW.PartyId
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                        LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                        LEFT JOIN [SCS].Currency AS CU1 ON CU1.Id=V.CurrencyId
                        LEFT JOIN [SCS].Currency AS CU2 ON CU2.Id=VD.CurrencyId
                        LEFT JOIN [ORG].Company AS CO ON CO.Id=V.CompanyId
                        LEFT JOIN [MST].AddressMaster AS AM ON AM.Id=CO.AddressMasterId
                        LEFT JOIN [SCS].FiscalYear AS FY ON FY.Id=V.FiscalYearId
                        LEFT JOIN [SCS].FiscalYearPeriod AS FYP ON FYP.Id=V.FiscalYearPeriodId
                        LEFT JOIN [MST].[BankMaster] AS BM ON BM.id=VD.BankMasterId
                        LEFT JOIN [HKP].[Bank] BN ON BN.Id=BM.BankId
                        LEFT JOIN [HKP].[BankBranch] BR ON BR.Id=BM.BankBranchId
                        LEFT JOIN [MST].BudgetMaster BUM ON VD.BudgetMasterId=BUM.Id
                        LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                        LEFT JOIN [ORG].[CostCenter] AS CST ON CST.Id = VD.CostCenterId
                        LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
                        LEFT JOIN [SCS].[FiscalYear] AS BFY ON BFY.Id=VD.FiscalYearId
                        LEFT JOIN [SCS].[FiscalYearPeriod] AS BFYP ON BFYP.Id=VD.FiscalYearPeriodId
                        where V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' AND V.SourceType='" + SourceType.CustomerReceipt + "'";
            return _sqlRepository.GetDataTable(sql);
        }

        public IWorkbook GetCustomerInvoiceSettlementReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string bankJournalId)
        {
            var excelEngine = new ExcelEngine();
            var reportUtility = new ReportUtility();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var dsLocal = GetCustomerInvoiceSettlementBank(companyGroupId, companyId, bankJournalId);
            var dsLocalwriteoff = GetCustomerInvoiceSettlementWriteOff(companyGroupId, companyId, bankJournalId);

            var _CurrencyId = dsLocal.Rows[0]["CurrencyId"].ToString();
            var trnCurrency = dsLocal.Rows[0]["CurrencyCode"].ToString();
            var plCurrencyCode = dsLocal.Rows[0]["CurrencyCode"].ToString();
            var dvNarration = new DataView(dsLocal)
            {
                RowFilter = "Narration IS NOT NULL"
            };
            var dtNarration = dvNarration.ToTable(true, "Narration");
            if (dsLocal.Rows.Count == 0)
                throw new Exception("No Data Found!");

            // Set report Name
            reportFileName = Convert.ToDateTime(dsLocal.Rows[0]["PostingDate"]).ToString("yyMMdd") + " " + dsLocal.Rows[0]["VoucherNo"];

            //reportUtility.SetMasterHeaderText(ref sheet, 5, 1, "Voucher No");
            //reportUtility.SetText(ref sheet, 5, 2, dsLocal.Rows[0]["VoucherNo"].ToString());

            //reportUtility.SetMasterHeaderText(ref sheet, 5, 3, "Voucher Date");
            //reportUtility.SetText(ref sheet, 5, 4, dsLocal.Rows[0]["VoucherDate"].ToString());
            //sheet.Range[5, 4, 5, 5].Merge();

            //reportUtility.SetMasterHeaderText(ref sheet, 6, 1, "Doc Date");
            //reportUtility.SetText(ref sheet, 6, 2, dsLocal.Rows[0]["DocDate"].ToString());

            //reportUtility.SetMasterHeaderText(ref sheet, 6, 3, "Doc No");
            //reportUtility.SetText(ref sheet, 6, 4, dsLocal.Rows[0]["DocRefNo"].ToString());
            //sheet.Range[6, 4, 6, 5].Merge();

            //reportUtility.SetMasterHeaderText(ref sheet, 7, 1, "Posting Date");
            //reportUtility.SetText(ref sheet, 7, 2, dsLocal.Rows[0]["PostingDate"].ToString());

            //reportUtility.SetMasterHeaderText(ref sheet, 7, 3, "Fiscal Year");
            //reportUtility.SetText(ref sheet, 7, 4, dsLocal.Rows[0]["PeriodName"].ToString());
            //sheet.Range[7, 4, 7, 5].Merge();

            //reportUtility.SetMasterHeaderText(ref sheet, 8, 1, "Customer");
            //reportUtility.SetText(ref sheet, 8, 2, dsLocal.Rows[0]["Customer"].ToString());

            //reportUtility.SetMasterHeaderText(ref sheet, 8, 3, "Customer Plant");
            //reportUtility.SetText(ref sheet, 8, 4, dsLocal.Rows[0]["CustomerPlant"].ToString());
            //sheet.Range[8, 4, 8, 5].Merge();

            //reportUtility.SetMasterHeaderText(ref sheet, 9, 1, "Narration");
            //reportUtility.SetText(ref sheet, 9, 2, dtNarration.Rows[0]["Narration"].ToString());

            //reportUtility.SetMasterHeaderText(ref sheet, 9, 3, "Status");
            //reportUtility.SetText(ref sheet, 9, 4, dsLocal.Rows[0]["Park/Post"].ToString());
            //sheet.Range[9, 4, 9, 5].Merge();

            var col = 1;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Customer Plant", 22); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "VoucherNo", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "PostingDate", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "DocDate", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "DocRefNo", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Bank/Cash Info", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Payment Receipt Amt.", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Currency", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Current Settlement", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Total Settlement", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Balance", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Exchange Rate", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Current Settlement (BC)", 11); col++;
            // reportUtility.SetHeaderText(ref sheet, 11, col, "Exchange Gain/(Loss)", 11); col++;

            var summerCol = col - 1;

            var colLast = col;
            var row = 12;
            var startRow = row;
            double _Total_Amount = 0;
            double vAmount = 0;
            for (int n = 0; n < dsLocal.Rows.Count; n++)
            {
                col = 1;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["CustomerPlant"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["VoucherNo"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["PostingDate"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["DocDate"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["DocRefNo"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["BankCashInfo"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["PaymentReceiptAmt"].ToString())); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["CurrencyCode"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["CurrentSettlement"].ToString())); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["TotalSettlement"].ToString())); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["Balance"].ToString())); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["CompanyCurrencyRate"].ToString())); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["CurrentSettlementBC"].ToString()));
                row++;
            }

            for (int n = 0; n < dsLocalwriteoff.Rows.Count; n++)
            {
                col = 1;
                reportUtility.SetText(ref sheet, row, col, dsLocalwriteoff.Rows[n]["CustomerPlant"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocalwriteoff.Rows[n]["VoucherNo"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocalwriteoff.Rows[n]["PostingDate"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocalwriteoff.Rows[n]["DocDate"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocalwriteoff.Rows[n]["DocRefNo"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocalwriteoff.Rows[n]["PINo"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocalwriteoff.Rows[n]["PaymentReceiptAmt"].ToString())); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocalwriteoff.Rows[n]["CurrencyCode"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocalwriteoff.Rows[n]["CurrentSettlement"].ToString())); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocalwriteoff.Rows[n]["TotalSettlement"].ToString())); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocalwriteoff.Rows[n]["Balance"].ToString())); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocalwriteoff.Rows[n]["CompanyCurrencyRate"].ToString())); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocalwriteoff.Rows[n]["CurrentSettlementBC"].ToString()));
                row++;
            }
            var lastRow = row;

            //////reportUtility.SetHeaderText(ref sheet, 11, col, "Customer Plant", 22); lastRow++;
            //////reportUtility.SetHeaderText(ref sheet, 11, col, "VoucherNo", 11); lastRow++;
            //////reportUtility.SetHeaderText(ref sheet, 11, col, "PostingDate", 11); lastRow++;
            //////reportUtility.SetHeaderText(ref sheet, 11, col, "DocDate", 11); lastRow++;
            //////reportUtility.SetHeaderText(ref sheet, 11, col, "DocRefNo", 11); lastRow++;
            //////reportUtility.SetHeaderText(ref sheet, 11, col, "Bank/Cash Info", 11); lastRow++;
            //////reportUtility.SetHeaderText(ref sheet, 11, col, "Payment Receipt Amt.", 11); lastRow++;
            //////reportUtility.SetHeaderText(ref sheet, 11, col, "Currency", 11); col++;
            //////reportUtility.SetHeaderText(ref sheet, 11, col, "Current Settlement", 11); lastRow++;
            //////reportUtility.SetHeaderText(ref sheet, 11, col, "Total Settlement", 11); lastRow++;
            //////reportUtility.SetHeaderText(ref sheet, 11, col, "Balance", 11); lastRow++;
            //////reportUtility.SetHeaderText(ref sheet, 11, col, "Exchange Rate", 11); lastRow++;
            //////reportUtility.SetHeaderText(ref sheet, 11, col, "Current Settlement (BC)", 11); lastRow++;
            //////reportUtility.SetHeaderText(ref sheet, 11, col, "Exchange Gain/(Loss)", 11); lastRow++;
            //#region sumCalc

            //reportUtility.SetText(ref sheet, lastRow, 1, "Total:", true);
            //sheet.Range[reportUtility.GetColumnNameForXls(1) + lastRow + ":" + reportUtility.GetColumnNameForXls(summerCol) + lastRow].Merge();
            //if (_CurrencyId != plCurrencyId)
            //{
            //    for (int i = 0; i < 4; i++)
            //    {
            //        summerCol++;
            //        sheet.Range[lastRow, summerCol].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(summerCol) + startRow + ":" + reportUtility.GetColumnNameForXls(summerCol) + (lastRow - 1) + ")";
            //        sheet.Range[lastRow, summerCol].NumberFormat = reportUtility.NumberFormatDecimalTwo();
            //        sheet.Range[lastRow, summerCol].CellStyle.Font.Bold = true;
            //        sheet.Range[lastRow, summerCol].BorderAround(ExcelLineStyle.Hair);
            //    }
            //}
            //else
            //{
            //    for (int i = 0; i < 2; i++)
            //    {
            //        summerCol++;
            //        sheet.Range[lastRow, summerCol].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(summerCol) + startRow + ":" + reportUtility.GetColumnNameForXls(summerCol) + (lastRow - 1) + ")";
            //        sheet.Range[lastRow, summerCol].NumberFormat = reportUtility.NumberFormatDecimalTwo();
            //        sheet.Range[lastRow, summerCol].CellStyle.Font.Bold = true;
            //        sheet.Range[lastRow, summerCol].BorderAround(ExcelLineStyle.Hair);
            //    }
            //}

            //#endregion sumCalc

            sheet.Range[12, 1, lastRow, colLast].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[12, 1, lastRow, colLast].BorderAround(ExcelLineStyle.Hair);

            #region InWord

            var _amountValue = reportUtility.InWord(vAmount, _CurrencyId);
            //var _amount = reportUtility.InWord(_Total_Amount, plCurrencyId);
            row++;

            reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

            //if (_CurrencyId != plCurrencyId)
            //{
            //    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = _amountValue;
            //    sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast - 2) + row].Merge();
            //    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            //    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
            //    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
            //    row++;
            //}
            //sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = _amount;
            //sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            //sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            //sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

            #endregion InWord

            row = row + 4;

            #region Signature

            //reportUtility.SetSignatureText(ref sheet, row - 1, 1, dsLocal.Rows[0]["AddedBy"].ToString());
            //sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            //reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

            //reportUtility.SetSignatureText(ref sheet, row - 1, 3, dsLocal.Rows[0]["PostedBy"].ToString());
            //sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            //reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

            //sheet.Range[row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            //reportUtility.SetTextMiddle(ref sheet, row, colLast, "Authorized By", true);

            #endregion Signature

            sheet.UsedRange.AutofitColumns();
            sheet.UsedRange.CellStyle.Font.Size = 8;
            reportUtility.CompanyPlantHeader(ref sheet, 7, "Invoice Settlement Voucher", companyId, plantName, null);
            reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Portrait);

            return workbook;
        }

        private DataTable GetCustomerInvoiceSettlementBank(string companyGroupId, string companyId, string bankJournalId)
        {
            try
            {
                var sql = @"SELECT  PP.UserName AS CustomerPlant, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), AM.PostingDate, 106), ' ', '-') AS PostingDate, Replace(CONVERT(VARCHAR(11), AM.DocDate, 106), ' ', '-') AS DocDate
								, AM.DocRefNo ,NULL [PINo.], BM.AccountTitle AS BankCashInfo, AM.Amount AS PaymentReceiptAmt, C.Code AS CurrencyCode
								,AD.WrittenOffAmount AS CurrentSettlement , AD.WrittenOffAmount AS TotalSettlement, AD.Amount - AD.WrittenOffAmount AS Balance
								, CC.CompanyCurrencyRate, AD.WrittenOffAmount*CC.CompanyCurrencyRate AS CurrentSettlementBC, NULL ExchangeGainLoss
								, AM.Narration, AM.CurrencyId, P.Code AS PartyCode, P.UserName AS PartyName
							    FROM [TRN].[BankJournalDetail] AS AD
                                LEFT JOIN [TRN].[BankJournal] AS AM ON AD.BankJournalId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.BankJournalDetailId=AD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
								LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=AM.BankMasterId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AM.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=AM.EntityId
								LEFT JOIN [HKP].[Party] AS P ON P.Id=AM.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AM.PartyPlantId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS CC ON CC.VoucherDetailId=VD.Id
                                WHERE AM.Archive=0 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.CompanyGroupId='" + companyGroupId + "' AND AM.CompanyId='" + companyId + @"' 
								 AND AD.PartyType='" + PartyType.Customer.ToString() + "' AND Am.Id='" + bankJournalId + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private DataTable GetCustomerInvoiceSettlementWriteOff(string companyGroupId, string companyId, string bankJournalId)
        {
            try
            {
                var sql = @"SELECT PP.UserName AS CustomerPlant, V.VoucherNo, Replace(CONVERT(VARCHAR(11) , I.PostingDate, 106), ' ', '-') AS PostingDate,  Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, I.DocRefNo
									,I.SalesOrderNo AS PINo, ISNULL(IV.Amount,0) AS PaymentReceiptAmt, C.Code AS CurrencyCode,(ISNULL(ID.Amount,0)) AS CurrentSettlement,(ISNULL(ID.Amount,0)) AS TotalSettlement
									, (ISNULL(I.Amount,0) - (ISNULL(ID.Amount,0))) AS Balance, (ISNULL(CC.CompanyCurrencyRate,0)) AS CompanyCurrencyRate, (ISNULL(ID.Amount,0))* (ISNULL(CC.CompanyCurrencyRate,0)) AS CurrentSettlementBC, NULL ExchangeGainLoss
									, I.Narration, I.CurrencyId, P.Code AS PartyCode, P.UserName AS PartyName
								
                                    FROM [TRN].[InvoiceWriteOffDetail] AS ID
									LEFT JOIN [TRN].[InvoiceWriteOff] AS IW ON IW.Id=ID.InvoiceWriteOffId
                                    LEFT JOIN [TRN].[Invoice] AS I ON I.Id=ID.InvoiceId
									 LEFT JOIN [TRN].[InvoiceDetail] AS IV ON IV.InvoiceId=I.Id
									LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
									LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                    LEFT JOIN [TRN].[AdjustmentNoteDetail] AS AJD ON AJD.InvoiceDetailId=ID.Id
                                    LEFT JOIN [TRN].[AdjustmentNote] AS AJ ON AJ.Id=AJD.AdjustmentNoteId
                                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IV.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=I.VoucherId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                    LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
									
                                     WHERE I.Archive=0 AND I.IsWrittenOff=0  AND (I.SourceType='CustomerInvoice' OR I.SourceType='SalesInvoice')
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + @"'  AND IW.BankJournalId='" + bankJournalId + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetSettlementGainLossReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var excelEngine = new ExcelEngine();
            var reportUtility = new ReportUtility();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var dsLocal = GetSettlementGainLossVoucher(voucherId);
            var _CurrencyId = dsLocal.Rows[0]["CurrencyId"].ToString();
            var plCurrencyId = dsLocal.Rows[0]["ParallelCurrencyId"].ToString();
            var trnCurrency = dsLocal.Rows[0]["TrnCurrency"].ToString();
            var plCurrencyCode = dsLocal.Rows[0]["CurrencyCode"].ToString();
            var dvNarration = new DataView(dsLocal)
            {
                RowFilter = "Narration IS NOT NULL"
            };
            var dtNarration = dvNarration.ToTable(true, "Narration");
            if (dsLocal.Rows.Count == 0)
                throw new Exception("No Data Found!");

            // Set report Name
            reportFileName = Convert.ToDateTime(dsLocal.Rows[0]["PostingDate"]).ToString("yyMMdd") + " " + dsLocal.Rows[0]["VoucherNo"];

            reportUtility.SetMasterHeaderText(ref sheet, 5, 1, "Voucher No");
            reportUtility.SetText(ref sheet, 5, 2, dsLocal.Rows[0]["VoucherNo"].ToString());

            reportUtility.SetMasterHeaderText(ref sheet, 5, 3, "Voucher Date");
            reportUtility.SetText(ref sheet, 5, 4, dsLocal.Rows[0]["VoucherDate"].ToString());
            sheet.Range[5, 4, 5, 5].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 6, 1, "Doc Date");
            reportUtility.SetText(ref sheet, 6, 2, dsLocal.Rows[0]["DocDate"].ToString());

            reportUtility.SetMasterHeaderText(ref sheet, 6, 3, "Doc No");
            reportUtility.SetText(ref sheet, 6, 4, dsLocal.Rows[0]["DocRefNo"].ToString());
            sheet.Range[6, 4, 6, 5].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 7, 1, "Posting Date");
            reportUtility.SetText(ref sheet, 7, 2, dsLocal.Rows[0]["PostingDate"].ToString());

            reportUtility.SetMasterHeaderText(ref sheet, 7, 3, "Fiscal Year");
            reportUtility.SetText(ref sheet, 7, 4, dsLocal.Rows[0]["PeriodName"].ToString());
            sheet.Range[7, 4, 7, 5].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 8, 1, "Customer");
            reportUtility.SetText(ref sheet, 8, 2, dsLocal.Rows[0]["Customer"].ToString());

            reportUtility.SetMasterHeaderText(ref sheet, 8, 3, "Customer Plant");
            reportUtility.SetText(ref sheet, 8, 4, dsLocal.Rows[0]["CustomerPlant"].ToString());
            sheet.Range[8, 4, 8, 5].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 9, 1, "Narration");
            reportUtility.SetText(ref sheet, 9, 2, dtNarration.Rows[0]["Narration"].ToString());

            reportUtility.SetMasterHeaderText(ref sheet, 9, 3, "Status");
            reportUtility.SetText(ref sheet, 9, 4, dsLocal.Rows[0]["Park/Post"].ToString());
            sheet.Range[9, 4, 9, 5].Merge();

            var col = 1;
            reportUtility.SetHeaderText(ref sheet, 11, col, "GL", 22); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Budget", 28); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Activity", 34); col++;
            var summerCol = col - 1;
            if (_CurrencyId != plCurrencyId)
            {
                reportUtility.SetHeaderText(ref sheet, 10, col, dsLocal.Rows[0]["TrnCurrency"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[10, col, 10, col + 1].Merge();
                reportUtility.SetHeaderText(ref sheet, 11, col, "Debit", ExcelHAlign.HAlignRight); col++;
                reportUtility.SetHeaderText(ref sheet, 11, col, "Credit", ExcelHAlign.HAlignRight); col++;
            }
            reportUtility.SetHeaderText(ref sheet, 10, col, dsLocal.Rows[0]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
            sheet[10, col, 10, col + 1].Merge();
            reportUtility.SetHeaderText(ref sheet, 11, col, "Debit", ExcelHAlign.HAlignRight); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Credit", ExcelHAlign.HAlignRight);
            var colLast = col;
            var row = 12;
            var startRow = row;
            double _Total_Amount = 0;
            double vAmount = 0;
            for (int n = 0; n < dsLocal.Rows.Count; n++)
            {
                col = 1;
                var AccountCodeId = dsLocal.Rows[n]["GLGeneralInfoCode"].ToString();
                reportUtility.SetText(ref sheet, row, col, AccountCodeId + " - " + dsLocal.Rows[n]["GL"]); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["Budget"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["Activity"].ToString()); col++;
                if (_CurrencyId != plCurrencyId)
                {
                    reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["TDrAmount"].ToString())); col++;
                    reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["TCrAmount"].ToString())); col++;
                    vAmount += Convert.ToDouble(dsLocal.Rows[n]["TCrAmount"].ToString());
                }
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["DrAmount"].ToString())); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["CrAmount"].ToString()));
                _Total_Amount += Convert.ToDouble(dsLocal.Rows[n]["CrAmount"].ToString());
                row++;
            }
            var lastRow = row;

            #region sumCalc

            reportUtility.SetText(ref sheet, lastRow, 1, "Total:", true);
            sheet.Range[reportUtility.GetColumnNameForXls(1) + lastRow + ":" + reportUtility.GetColumnNameForXls(summerCol) + lastRow].Merge();
            if (_CurrencyId != plCurrencyId)
            {
                for (int i = 0; i < 4; i++)
                {
                    summerCol++;
                    sheet.Range[lastRow, summerCol].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(summerCol) + startRow + ":" + reportUtility.GetColumnNameForXls(summerCol) + (lastRow - 1) + ")";
                    sheet.Range[lastRow, summerCol].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[lastRow, summerCol].CellStyle.Font.Bold = true;
                    sheet.Range[lastRow, summerCol].BorderAround(ExcelLineStyle.Hair);
                }
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    summerCol++;
                    sheet.Range[lastRow, summerCol].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(summerCol) + startRow + ":" + reportUtility.GetColumnNameForXls(summerCol) + (lastRow - 1) + ")";
                    sheet.Range[lastRow, summerCol].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[lastRow, summerCol].CellStyle.Font.Bold = true;
                    sheet.Range[lastRow, summerCol].BorderAround(ExcelLineStyle.Hair);
                }
            }

            #endregion sumCalc

            sheet.Range[12, 1, lastRow, colLast].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[12, 1, lastRow, colLast].BorderAround(ExcelLineStyle.Hair);

            #region InWord

            var _amountValue = reportUtility.InWord(vAmount, _CurrencyId);
            var _amount = reportUtility.InWord(_Total_Amount, plCurrencyId);
            row++;

            reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

            if (_CurrencyId != plCurrencyId)
            {
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = _amountValue;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast - 2) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
                row++;
            }
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = _amount;
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

            #endregion InWord

            row = row + 4;

            #region Signature

            reportUtility.SetSignatureText(ref sheet, row - 1, 1, dsLocal.Rows[0]["AddedBy"].ToString());
            sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

            reportUtility.SetSignatureText(ref sheet, row - 1, 3, dsLocal.Rows[0]["PostedBy"].ToString());
            sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked/Posted By", true);

            sheet.Range[row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, colLast, "Authorized By", true);

            #endregion Signature

            sheet.UsedRange.AutofitColumns();
            sheet.UsedRange.CellStyle.Font.Size = 8;
            reportUtility.CompanyPlantHeader(ref sheet, 7, "Customer SettlementGainLoss Voucher", companyId, plantName, null);
            reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Portrait);

            return workbook;
        }

        private DataTable GetSettlementGainLossVoucher(string voucherId)
        {
            try
            {
                var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                            , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS TDrAmount, VD.CrAmount AS TCrAmount, VDC.DrAmount, VDC.CrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget
                            , ACT.UserName AS Activity, CM.UserName AS CashMasterName
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [TRN].[InvoiceWriteOffDetail] AS IVD ON IVD.Id=VD.InvoiceWriteOffDetailId
                            LEFT JOIN [TRN].[InvoiceWriteOff] AS IV ON IVD.InvoiceWriteOffId=IV.Id
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                            WHERE V.Archive=0 AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetPartyReconciliationReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherWriteOffId)
        {
            var excelEngine = new ExcelEngine();
            var reportUtility = new ReportUtility();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var dsLocal = GetPartyReconciliationVoucherWriteOff(companyGroupId, companyId, plantId, voucherWriteOffId);
            var dsLocalwriteoff = GetPartyReconciliationVoucherWriteOffDetail(companyGroupId, companyId, plantId, voucherWriteOffId);

            var _CurrencyId = dsLocal.Rows[0]["CurrencyId"].ToString();
            var trnCurrency = dsLocal.Rows[0]["CurrencyCode"].ToString();
            var plCurrencyCode = dsLocal.Rows[0]["CurrencyCode"].ToString();
            var dvNarration = new DataView(dsLocal)
            {
                RowFilter = "Narration IS NOT NULL"
            };
            var dtNarration = dvNarration.ToTable(true, "Narration");
            if (dsLocal.Rows.Count == 0)
                throw new Exception("No Data Found!");

            // Set report Name
            reportFileName = Convert.ToDateTime(dsLocal.Rows[0]["PostingDate"]).ToString("yyMMdd") + " " + dsLocal.Rows[0]["VoucherNo"];

            reportUtility.SetMasterHeaderText(ref sheet, 8, 1, "Customer");
            reportUtility.SetText(ref sheet, 8, 2, dsLocal.Rows[0]["PartyName"].ToString());

            var col = 1;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Customer Plant", 22); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "VoucherNo", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "PostingDate", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "DocDate", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "DocRefNo", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Currency", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Receipt Amount", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Current Settlement", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Total Settlement", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 11, col, "Balance", 11); col++;

            var summerCol = col - 1;

            var colLast = col;
            var row = 12;
            var startRow = row;
            double _Total_Amount = 0;
            double vAmount = 0;
            for (int n = 0; n < dsLocal.Rows.Count; n++)
            {
                col = 1;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["PartyPlantName"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["VoucherNo"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["PostingDate"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["DocDate"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["DocRefNo"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["CurrencyCode"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["Receivable"].ToString())); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["CurrentSettlement"].ToString())); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["TotalSettlement"].ToString())); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["Balance"].ToString())); col++;
                row++;
            }

            var col1 = 1;
            reportUtility.SetHeaderText(ref sheet, 14, col1, "Customer Plant", 22); col1++;
            reportUtility.SetHeaderText(ref sheet, 14, col1, "VoucherNo", 11); col1++;
            reportUtility.SetHeaderText(ref sheet, 14, col1, "PostingDate", 11); col1++;
            reportUtility.SetHeaderText(ref sheet, 14, col1, "DocDate", 11); col1++;
            reportUtility.SetHeaderText(ref sheet, 14, col1, "DocRefNo", 11); col1++;
            reportUtility.SetHeaderText(ref sheet, 14, col1, "Currency", 11); col1++;
            reportUtility.SetHeaderText(ref sheet, 14, col1, "Receivable Amount", 11); col1++;
            reportUtility.SetHeaderText(ref sheet, 14, col1, "Current Settlement", 11); col1++;
            reportUtility.SetHeaderText(ref sheet, 14, col1, "Total Settlement", 11); col1++;
            reportUtility.SetHeaderText(ref sheet, 14, col1, "Balance", 11); col1++;
            var row1 = 15;

            for (int n = 0; n < dsLocalwriteoff.Rows.Count; n++)
            {
                col1 = 1;
                reportUtility.SetText(ref sheet, row1, col1, dsLocalwriteoff.Rows[n]["PartyPlantName"].ToString()); col1++;
                reportUtility.SetText(ref sheet, row1, col1, dsLocalwriteoff.Rows[n]["VoucherNo"].ToString()); col1++;
                reportUtility.SetText(ref sheet, row1, col1, dsLocalwriteoff.Rows[n]["PostingDate"].ToString()); col1++;
                reportUtility.SetText(ref sheet, row1, col1, dsLocalwriteoff.Rows[n]["DocDate"].ToString()); col1++;
                reportUtility.SetText(ref sheet, row1, col1, dsLocalwriteoff.Rows[n]["DocRefNo"].ToString()); col1++;
                reportUtility.SetText(ref sheet, row1, col1, dsLocalwriteoff.Rows[n]["CurrencyCode"].ToString()); col1++;
                reportUtility.SetText(ref sheet, row1, col1, Convert.ToDouble(dsLocalwriteoff.Rows[n]["Receivable"].ToString())); col1++;
                reportUtility.SetText(ref sheet, row1, col1, Convert.ToDouble(dsLocalwriteoff.Rows[n]["CurrentSettlement"].ToString())); col1++;
                reportUtility.SetText(ref sheet, row1, col1, Convert.ToDouble(dsLocalwriteoff.Rows[n]["TotalSettlement"].ToString())); col1++;
                reportUtility.SetText(ref sheet, row1, col1, Convert.ToDouble(dsLocalwriteoff.Rows[n]["Balance"].ToString())); col1++;
                row1++;
            }
            var lastRow = row1;


            #region Signature
            row1 = row1 + 4;
            reportUtility.SetSignatureText(ref sheet, row1 - 1, 1, dsLocal.Rows[0]["AddedBy"].ToString());
            sheet.Range[row1, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row1, 1, "Prepared By", true);

            sheet.Range[row1, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row1, 3, "Checked/Posted By", true);

            sheet.Range[row1, 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row1, 7, "Authorized By", true);

            #endregion Signature

            sheet.UsedRange.AutofitColumns();
            sheet.UsedRange.CellStyle.Font.Size = 8;
            reportUtility.CompanyPlantHeader(ref sheet, 7, "Party Reconciliation Report", companyId, plantName, null);
            reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Landscape);

            return workbook;
        }

        private DataTable GetPartyReconciliationVoucherWriteOff(string companyGroupId, string companyId, string plantId, string voucherWriteOffId)
        {
            try
            {
                var sql = @"SELECT  VD.VoucherId,VD.Id, VD.PartyType, VD.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, VD.PartyPlantId, PP.UserName AS PartyPlantName, VD.Id AS VoucherDetailId, VD.EntityId
								, EN.UserName AS EntityName, VD.CurrencyId, C.Code AS CurrencyCode, VD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
								, VD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, VD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS DocDate
                                , REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.DocRefNo, VD.Narration
								,VDW.Amount AS CurrentSettlement, ISNULL(AM.Amount,0)+ ISNULL(VDW.Amount,0) AS TotalSettlement, CC.CompanyCurrencyAmount AS Receivable
                                ,(ISNULL(CC.CompanyCurrencyAmount,0)-ISNULL(AM.Amount,0)- ISNULL(VDW.Amount,0)) AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion
                                ,VDW.AddedBy,REPLACE(CONVERT(VARCHAR(11), VDW.DocDate, 106), ' ', '-')  AS ReconciliationDate
                                FROM 
								[TRN].[VoucherDetail] AS VD
                                LEFT JOIN(select VW.VoucherDetailId,SUM(VW.Amount) AS Amount from  [TRN].[VoucherWriteOff] VW  where vw.IsWrittenOff=0 AND VW.Id!='" + voucherWriteOffId + @"' GROUP BY VW.VoucherDetailId) AS AM ON VD.Id=AM.VoucherDetailId
								LEFT JOIN (SELECT * FROM TRN.VoucherWriteOff WHERE Id='" + voucherWriteOffId + @"') AS VDW ON VDW.VoucherDetailId=VD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=VD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=V.EntityId
								LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS CC ON CC.VoucherDetailId=VD.Id
                                WHERE  V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND VD.PartyType='" + PartyType.Customer.ToString() + @"' 
                                AND VD.PartyId<>'' AND VD.DrAmount=0 AND (CC.CompanyCurrencyAmount-ISNULL(AM.Amount,0))!=0 AND VDW.Id='" + voucherWriteOffId + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private DataTable GetPartyReconciliationVoucherWriteOffDetail(string companyGroupId, string companyId, string plantId, string voucherWriteOffId)
        {
            try
            {
                var sql = @"SELECT  VD.VoucherId, VWD.Id, VWD.VoucherWriteOffId,VWD.VoucherDetailId
, VD.PartyType, VD.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, VD.PartyPlantId, PP.UserName AS PartyPlantName,   VD.EntityId
								, EN.UserName AS EntityName, VD.CurrencyId, C.Code AS CurrencyCode, VD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
								, VD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, VD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS DocDate
                                 , REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.DocRefNo, VD.Narration, CC.CompanyCurrencyAmount AS Receivable
								, VWD.Amount AS CurrentSettlement, ISNULL(AD.Amount,0)+VWD.Amount AS TotalSettlement
                               ,(ISNULL(CC.CompanyCurrencyAmount,0)-ISNULL(AD.Amount,0)-VWD.Amount) AS Balance
							
								, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion
                                FROM [TRN].[VoucherWriteOffDetail] AS VWD 
								LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VWD.VoucherDetailId
                                LEFT JOIN(SELECT VW.VoucherDetailId, SUM(ISNULL(VW.Amount,0)) Amount FROM  [TRN].[VoucherWriteOffDetail] 
								AS VW WHERE VW.VoucherWriteOffId != '" + voucherWriteOffId + @"'  GROUP BY VW.VoucherDetailId) AS AD  ON VWD.VoucherDetailId=AD.VoucherDetailId
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=VD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=VD.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=V.EntityId
								LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS CC ON CC.VoucherDetailId=VD.Id
                                WHERE  V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND VD.PartyType='" + PartyType.Customer.ToString() + @"' 
                                AND VD.PartyId<>'' AND VD.CrAmount=0 AND (CC.CompanyCurrencyAmount-ISNULL(AD.Amount,0))!=0 AND VWD.VoucherWriteOffId='" + voucherWriteOffId + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }


        private Dictionary<string, object> GetPurchaseLCChargesHeader(string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var cmdText = @" SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.AddedBy, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , P.UserName AS Vendor, P.UserName AS VendorPlant, V.CurrencyId, C.Code AS CurrencyCode
                            FROM [dbo].[PurchaseLCCharges] PLC 
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=PLC.VoucherId
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [dbo].PurchaseLC PL ON PL.Id=PLC.PurchaseLCId
							LEFT JOIN [HKP].[Party] AS P ON P.Id=PL.VendorId
							--LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=BJ.PartyPlantId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                            WHERE  PLC.VoucherId='" + voucherId + "' AND V.SourceType='PurchaseLCOpeningCharges'";
            return _sqlRepository.GetData(cmdText);
        }

        private DataTable GetPurchaseLCChargesVoucher(string voucherId)
        {
            try
            {
                var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                            , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS DrAmount, VD.CrAmount AS CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget
                            , CM.UserName AS CashMasterName,
							Activity=case when VD.BankMasterId <>'' then BM.AccountTitle 
											when VD.CashMasterId<>'' then CM.UserName
											else ACT.UserName end
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
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                            WHERE V.Archive=0 AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public IWorkbook GetPurchaseLCChargesReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetPurchaseLCChargesHeader(companyGroupId, companyId, plantId, voucherId);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];



            var dsLocal = GetPurchaseLCChargesVoucher(voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

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

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor");
            reportUtility.SetText(ref sheet, row, 2, header["Vendor"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "LC Ref.");
            reportUtility.SetText(ref sheet, row, 4, header["DocRefNo"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor Plant");
            reportUtility.SetText(ref sheet, row, 2, header["VendorPlant"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Status");
            reportUtility.SetText(ref sheet, row, 4, header["Status"].ToString());

            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            //sheet[1, 2].ColumnWidth = 100;

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
                sheet[1, 2].ColumnWidth = 60;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 2, header["PostedBy"].ToString());
                sheet.Range[row, 2].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 2, "Checked/Posted By", true);

                sheet.Range[row, 4].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 4, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "LC Charges", companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, "LC Charges", companyId,plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }

        private Dictionary<string, object> GetDocumentAcceptanceHeader(string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var cmdText = @"   SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.AddedBy, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , P.UserName AS Vendor, P.UserName AS VendorPlant, V.CurrencyId, C.Code AS CurrencyCode
							,PDA.InvoiceNo, REPLACE(CONVERT(VARCHAR(11), PDA.InvoiceDate, 106), ' ', '-') AS InvoiceDate,PDA.PurchaseLCId,pl.LCANo
                            ,LCRef

                            ,CASE WHEN PDA.IsNonCreditable=1 THEN 'Yes' ELSE 'No' END AS IsNonCreditable
                            FROM [TRN].[Voucher] AS V
                            LEFT JOIN TRN.PurchaseDocAcceptance PDA ON PDA.VoucherId=V.Id
	                        left join dbo.PurchaseLC pl on pl.Id =PDA.PurchaseLCId
							LEFT JOIN TRN.VoucherDetail VD ON VD.VoucherId=V.Id AND VD.PartyId<>''
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
							LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                            WHERE  V.Id='" + voucherId + "' AND V.SourceType='" + SourceType.PurchaseDocAcceptance.ToString() + "'";
            return _sqlRepository.GetData(cmdText);
        }

        private DataTable GetDocumentAcceptanceVoucher(string voucherId)
        {
            try
            {
                var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                            , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS DrAmount, VD.CrAmount AS CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.UserName AS Customer, PP.UserName AS CustomerPlant, VD.Narration AS DetailNarration, BUD.UserName AS Budget
                            , CM.UserName AS CashMasterName,
							Activity=case when VD.BankMasterId <>'' then BM.AccountTitle 
											when VD.CashMasterId<>'' then CM.UserName
											else ACT.UserName end
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
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                            WHERE V.Archive=0 AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public IWorkbook DocumentAcceptanceVoucher(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetDocumentAcceptanceHeader(companyGroupId, companyId, plantId, voucherId);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetDocumentAcceptanceVoucher(voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

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


            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Invoice Date");
            reportUtility.SetText(ref sheet, row, 2, header["InvoiceDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "LC No");
            reportUtility.SetText(ref sheet, row, 4, header["LCRef"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Invoice No");
            reportUtility.SetText(ref sheet, row, 2, header["InvoiceNo"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Is Non Creditable");
            reportUtility.SetText(ref sheet, row, 4, header["IsNonCreditable"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor");
            reportUtility.SetText(ref sheet, row, 2, header["Vendor"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Acceptance Ref No.");
            reportUtility.SetText(ref sheet, row, 4, header["DocRefNo"].ToString());
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Vendor Plant");
            reportUtility.SetText(ref sheet, row, 2, header["VendorPlant"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Status");
            reportUtility.SetText(ref sheet, row, 4, header["Status"].ToString());
            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            //sheet[1, 2].ColumnWidth = 100;
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
                sheet[1, 2].ColumnWidth = 60;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 2, header["PostedBy"].ToString());
                sheet.Range[row, 2].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 2, "Checked/Posted By", true);

                sheet.Range[row, 4].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 4, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId,plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
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
       
        
    }
}