using Library.Core;
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
using System.Threading.Tasks;

namespace Library.Accounting.Accounts
{
    public class AccountsCashReportService
    {
        private readonly ISqlRepository _sqlRepository;

        public AccountsCashReportService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }

        public IWorkbook GetCashBookReportCompanyLevel(string companyGroupId, string companyId, string cashMasterId, string fromDate, string toDate)
        {
            try
            {
                var row = 6;
                var colLast = row;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Report";
                AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);
                // Get BankMaster data
                var cashMaster = GetCashMaster(cashMasterId);

                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Cash");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, cashMaster["CashName"].ToString());
                sheet[row, 3].ColumnWidth = 150;

                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "GL");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, cashMaster["GLGeneralInfoCode"] + " - " + cashMaster["GLGeneralInfoName"]);
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();


                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Cash Currency");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, cashMaster["CurrencyCode"].ToString());

                row++;
                reportUtility.SetHeaderText(ref sheet, row, 5, "Cash Currency", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ":" + reportUtility.GetColumnNameForXls(8) + row].Merge();
                reportUtility.SetHeaderText(ref sheet, row, 6, "Cash Currency", ExcelHAlign.HAlignCenter, true);
                reportUtility.SetHeaderText(ref sheet, row, 7, "Cash Currency", ExcelHAlign.HAlignCenter, true);
                reportUtility.SetHeaderText(ref sheet, row, 8, "Cash Currency", ExcelHAlign.HAlignCenter, true);
                colLast = 8;
                accountsCommonService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                var cashCurrencyId = cashMaster["CurrencyId"].ToString();
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 7, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(7) + row + ":" + reportUtility.GetColumnNameForXls(9) + row].Merge();
                    colLast = 10;
                }

                // Detail Header
                row++;
                int col = 1;
                reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 12);
                int colVoucherNo = col;
                col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 12);
                int colPostingDate = col; 
                col++;
                //reportUtility.SetHeaderText(ref sheet, row, col, "Account Name", 12);
                //int colAccountName = col; 
                //col++;                        
                reportUtility.SetHeaderText(ref sheet, row, col, "Particulars", 32);
                int colParticulars = col; 
                col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 32);
                int colNarration = col;
                col++;
                //sheet.Range[reportUtility.GetColumnNameForXls(3) + row + ": " + reportUtility.GetColumnNameForXls(4) + row].Merge();
                reportUtility.SetHeaderText(ref sheet, row, 5, "Debit", 9, ExcelHAlign.HAlignRight); int colDebit = 5;
                reportUtility.SetHeaderText(ref sheet, row, 6, "Credit", 9, ExcelHAlign.HAlignRight); int colCredit = 6;
                reportUtility.SetHeaderText(ref sheet, row, 7, "Balance", 12, ExcelHAlign.HAlignRight);
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 8, "Debit", 9, ExcelHAlign.HAlignRight);
                    reportUtility.SetHeaderText(ref sheet, row, 9, "Credit", 9, ExcelHAlign.HAlignRight);
                    reportUtility.SetHeaderText(ref sheet, row, 10, "Balance", ExcelHAlign.HAlignRight);
                }
                reportUtility.SetHeaderText(ref sheet, row, colLast, "Dr/Cr", 4, ExcelHAlign.HAlignRight);

                row++;
                reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();

                // Get Cash opening balance data.
                var obVal = GetCashOpeningBalanceLedgerData(companyGroupId, companyId, cashMasterId, fromDate);
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    var ob = Convert.ToDouble(obVal[0]["OB"]);
                    reportUtility.SetText(ref sheet, row, 7, ob, true);

                    if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                        reportUtility.SetText(ref sheet, row, 10, ob, true);
                    sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"  Dr\", \"  Cr\")";
                }

                row++;
                int StartRow = row;
                // Get Cash transaction data.
                var ledgerData = GetCashLedgerData(companyGroupId, companyId, cashMasterId, fromDate, toDate);
                if (ledgerData.Rows.Count > 0)
                {
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        reportUtility.SetText(ref sheet, row, 1, ledgerData.Rows[i]["VoucherNo"].ToString());
                        reportUtility.SetText(ref sheet, row, 2, Convert.ToDateTime(ledgerData.Rows[i]["PostingDate"].ToString()).ToString("dd-MMM-yyyy"));
                        reportUtility.SetText(ref sheet, row, 3, ledgerData.Rows[i]["OtherSide"].ToString());
                        //reportUtility.SetText(ref sheet, row, 4, ledgerData.Rows[i]["Narration"].ToString());
                        //sheet.Range[reportUtility.GetColumnNameForXls(3) + row + ": " + reportUtility.GetColumnNameForXls(4) + row].Merge();
                        //sheet.Range[row, 3,row,4].WrapText = true;
                        reportUtility.SetText(ref sheet, row, 4, ledgerData.Rows[i]["Narration"].ToString(), false, true);
                        sheet.Range[row, 3].WrapText = true;




                        // Base currency checking
                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                        {
                            reportUtility.SetText(ref sheet, row, 5, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, 6, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString()));
                            sheet.Range[row, 7].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(7) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(5) + row + "-" + reportUtility.GetColumnNameForXls(6) + row + ")";
                            sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;

                            reportUtility.SetText(ref sheet, row, 8, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, 9, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                            sheet.Range[row, 10].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(10) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(8) + row + "-" + reportUtility.GetColumnNameForXls(9) + row + ")";
                            sheet.Range[row, 10].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            sheet.Range[row, 10].VerticalAlignment = ExcelVAlign.VAlignTop;
                        }
                        else
                        {
                            reportUtility.SetText(ref sheet, row, 5, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, 6, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                            sheet.Range[row, 7].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(7) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(5) + row + "-" + reportUtility.GetColumnNameForXls(6) + row + ")";
                            sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
                        }
                        sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        row++;
                    }
                }


                reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();
                sheet.Range[row, colDebit].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colDebit) + StartRow + ":" + reportUtility.GetColumnNameForXls(colDebit) + (row - 1) + ")";
                sheet.Range[row, colDebit].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet.Range[row, colCredit].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colCredit) + StartRow + ":" + reportUtility.GetColumnNameForXls(colCredit) + (row - 1) + ")";
                sheet.Range[row, colCredit].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet.Range[row, 7].Formula = "=" + reportUtility.GetColumnNameForXls(7) + (row - 1);
                sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, 7].CellStyle.Font.Bold = true;
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    sheet.Range[row, 10].Formula = "=" + reportUtility.GetColumnNameForXls(10) + (row - 1);
                    sheet.Range[row, 10].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 10].CellStyle.Font.Bold = true;
                }
                sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"  Dr\", \"  Cr\")";

                sheet.Range[11, 4, row, 4].WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyHeader(ref sheet, colLast, "Cash Ledger", companyId);
                reportUtility.SetText(ref sheet, 5, colLast, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(colLast) + 5].Merge();


               
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;


                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public Dictionary<string, object> GetCashMaster(string cashMasterId)
        {
            var sql = @"SELECT CM.Id, CM.UserName AS CashName, CM.CurrencyId, C.Code AS CurrencyCode, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, BGM.RefNo
                        , BG.UserName AS BudgetName, A.UserName AS ActivityName
                        FROM [MST].[CashMaster] AS CM
                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=CM.CurrencyId
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=CM.GLGeneralInfoId
                        LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=CM.BudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=CM.ActivityId
                        WHERE CM.Id='" + cashMasterId + "'";
            return _sqlRepository.GetData(sql);
        }
        public List<Dictionary<string, object>> GetCashOpeningBalanceLedgerData(string companyGroupId, string companyId, string cashMasterId, string fromDate)
        {
            var sql = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                        SELECT SUM(DrAmount) - SUM(CrAmount) AS OB
                        , CompanyCurrencyId, SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyCurrencyOB
                        , CompanyGroupCurrencyId, SUM(CompanyGroupCurrencyDrAmount)-SUM(CompanyGroupCurrencyCrAmount) AS CompanyGroupCurrencyOB
                        , HardCurrencyId, SUM(HardCurrencyDrAmount)-SUM(HardCurrencyCrAmount) AS HardCurrencyOB FROM (
                        SELECT SUM(GLTD.DrAmount) AS DrAmount, SUM(GLTD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        , GC.CompanyGroupCurrencyId, SUM(GC.CompanyGroupCurrencyDrAmount) AS CompanyGroupCurrencyDrAmount, SUM(GC.CompanyGroupCurrencyCrAmount) AS CompanyGroupCurrencyCrAmount
                        , HC.HardCurrencyId, SUM(HC.HardCurrencyDrAmount) AS HardCurrencyDrAmount, SUM(HC.HardCurrencyCrAmount) AS HardCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN [TRN].[GLTransactionDetail] AS GLTD ON GLTD.VoucherDetailId=VD.Id AND GLTD.CashMasterId=VD.CashMasterId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.DrAmount AS CompanyGroupCurrencyDrAmount, VDC.CrAmount AS CompanyGroupCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId
                        ) AS GC ON GC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS HardCurrencyId, VDC.DrAmount AS HardCurrencyDrAmount, VDC.CrAmount AS HardCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId
                        ) AS HC ON HC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND VD.CashMasterId='" + cashMasterId + "' AND V.PostingDate < '" + fromDate.ToDbDate() + @"'
                        GROUP BY CC.CompanyCurrencyId, GC.CompanyGroupCurrencyId, HC.HardCurrencyId
                        UNION ALL
                        SELECT SUM(GLTD.DrAmount) AS DrAmount, SUM(GLTD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        , GC.CompanyGroupCurrencyId, SUM(GC.CompanyGroupCurrencyDrAmount) AS CompanyGroupCurrencyDrAmount, SUM(GC.CompanyGroupCurrencyCrAmount) AS CompanyGroupCurrencyCrAmount
                        , HC.HardCurrencyId, SUM(HC.HardCurrencyDrAmount) AS HardCurrencyDrAmount, SUM(HC.HardCurrencyCrAmount) AS HardCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN [TRN].[GLTransactionDetail] AS GLTD ON GLTD.VoucherDetailId=VD.Id AND GLTD.CashMasterId=VD.CashMasterId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.DrAmount AS CompanyGroupCurrencyDrAmount, VDC.CrAmount AS CompanyGroupCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId=@companyId
                        ) AS GC ON GC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS HardCurrencyId, VDC.DrAmount AS HardCurrencyDrAmount, VDC.CrAmount AS HardCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId=@companyId
                        ) AS HC ON HC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId=@companyId AND VD.CashMasterId='" + cashMasterId + "' AND V.PostingDate >='" + fromDate.ToDbDate() + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId, GC.CompanyGroupCurrencyId, HC.HardCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId, X.CompanyGroupCurrencyId, X.HardCurrencyId";
            return _sqlRepository.GetDataCollection(sql);
        }
        public DataTable GetCashLedgerData(string companyGroupId, string companyId, string cashMasterId, string fromDate, string toDate)
        {
            var cmdText = @"DECLARE @companyGroupId VARCHAR(10)='" + companyGroupId + @"';
                        DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                        DECLARE @cashMasterId VARCHAR(10)='" + cashMasterId + @"';
                        SELECT V.VoucherNo, V.PostingDate, V.CurrencyId,
                         VD.DrAmount ,
                         VD.CrAmount 
						 , V.Narration
                        , CC.CompanyCurrencyDrAmount, CC.CompanyCurrencyCrAmount
						,OtherSide = concat( STUFF((select distinct ','+XPP.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join HKP.PartyPlant XPP ON XPP.Id=XVD.PartyPlantId
                    where XVD.VoucherId=V.Id AND XVD.PartyPlantId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XEI.AccountTitle from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join mst.BankMaster XEI ON XEI.id=XVD.BankMasterId
					LEFT JOIN HKP.Bank BX ON BX.Id=XEI.BankId
                    where XVD.VoucherId=V.Id AND XVD.BankMasterId <>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    
                    ,STUFF((select distinct ','+XEI.EmployeeName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join dbo.EmployeeInformation XEI ON XEI.SystemId=XVD.EmployeeId
                    where XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XCM.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join MST.CashMaster XCM ON XCM.Id=XVD.CashMasterId
                    where XVD.VoucherId=V.Id AND XVD.CashMasterId!=vd.CashMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XA.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join HKP.Activity XA ON XA.Id=XVD.ActivityId
                    where XVD.VoucherId=V.Id AND XVD.CashMasterId IS NULL AND XVD.EmployeeId IS NULL AND XVD.PartyPlantId IS NULL for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                        FROM  [TRN].[VoucherDetail] AS VD 
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                        LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                        LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId  AND VD.CashMasterId=@cashMasterId AND V.SourceType!='OpeningBalance'
						 AND V.PostingDate BETWEEN '" + fromDate + "' AND '" + toDate + @"' AND V.SourceType!='OpeningBalance' AND VD.LoanSetOffGroupNo IS NULL
                        UNION ALL
                        SELECT V.VoucherNo, V.PostingDate, V.CurrencyId,
                         VD.DrAmount ,
                         VD.CrAmount 
						 , V.Narration
                        , CC.CompanyCurrencyDrAmount, CC.CompanyCurrencyCrAmount
						, OtherSide=CASE 
	                        WHEN P.UserName<>'' THEN P.UserName
							WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
	                        WHEN CM.UserName<>'' THEN CM.UserName
	                        ELSE ''	END
                        FROM  [TRN].[VoucherDetail] AS VD 
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                        LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                        LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId  AND VD.CashMasterId=@cashMasterId AND V.SourceType!='OpeningBalance'
						 AND V.PostingDate > '" + fromDate + @"' AND V.SourceType='OpeningBalance' AND VD.LoanSetOffGroupNo IS NULL
                         UNION ALL
						 SELECT  VoucherNo=STUFF((select distinct ','+XV.VoucherNo from
							trn.VoucherDetail XVD 
							LEFT JOIN TRN.Voucher XV ON XVD.VoucherId=XV.Id
							where  XVD.LoanSetOffGroupNo=VD.LoanSetOffGroupNo  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
						 , V.PostingDate, V.CurrencyId,
                         SUM(VD.DrAmount) DrAmount,
                         SUM(VD.CrAmount )CrAmount
						 , V.Narration
                        , SUM(CC.CompanyCurrencyDrAmount)CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount)CompanyCurrencyCrAmount
						 ,OtherSide = concat( STUFF((select distinct ','+XPP.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join HKP.PartyPlant XPP ON XPP.Id=XVD.PartyPlantId
                    where XVD.LoanSetOffGroupNo=VD.LoanSetOffGroupNo AND XVD.PartyPlantId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XEI.AccountTitle from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join mst.BankMaster XEI ON XEI.id=XVD.BankMasterId
					LEFT JOIN HKP.Bank BX ON BX.Id=XEI.BankId
                    where XVD.LoanSetOffGroupNo=VD.LoanSetOffGroupNo AND XVD.BankMasterId <>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    
                    ,STUFF((select distinct ','+XEI.EmployeeName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join dbo.EmployeeInformation XEI ON XEI.SystemId=XVD.EmployeeId
                    where XVD.LoanSetOffGroupNo=VD.LoanSetOffGroupNo AND XVD.EmployeeId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XCM.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join MST.CashMaster XCM ON XCM.Id=XVD.CashMasterId
                    where XVD.LoanSetOffGroupNo=VD.LoanSetOffGroupNo AND XVD.CashMasterId!=vd.CashMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XA.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join HKP.Activity XA ON XA.Id=XVD.ActivityId
                    where XVD.LoanSetOffGroupNo=VD.LoanSetOffGroupNo AND XVD.CashMasterId IS NULL AND XVD.EmployeeId IS NULL AND XVD.PartyPlantId IS NULL for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                        FROM  [TRN].[VoucherDetail] AS VD 
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                        LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                        LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND VD.CashMasterId=@cashMasterId AND V.SourceType!='OpeningBalance'
                        AND V.PostingDate BETWEEN '" + fromDate + "' AND '" + toDate + @"' AND V.SourceType!='OpeningBalance' AND VD.LoanSetOffGroupNo <>''
						 GROUP BY  V.PostingDate, V.CurrencyId, V.Narration,VD.LoanSetOffGroupNo,VD.CashMasterId
						 ORDER BY V.PostingDate ASC";
            return _sqlRepository.GetDataTable(cmdText);
        }
    }
}
