using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Currencies;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Model.Vouchers;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Taxations;
using Library.ViewModel.OrderManagements;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Library.Accounting.Accounts
{
    public class AccountsPartyReconciliationService
	{
        private readonly ISqlRepository _sqlRepository;
        public AccountsPartyReconciliationService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
        public List<Dictionary<string, object>> GetPartyDrList( string companyGroupId, string companyId, string plantId, string partyId)
        {
           var sql = @"SELECT  IV.VoucherId,v.VoucherNo, v.SourceType,  VD.Id, IV.PartyType, IV.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, IV.PartyPlantId, PP.UserName AS PartyPlantName,  VD.Id AS VoucherDetailId, V.EntityId
								, EN.UserName AS EntityName, IV.CurrencyId, C.Code AS CurrencyCode, IVD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
								, IVD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, IVD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), IV.DocDate, 106), ' ', '-') AS DocDate
                                , REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.DocRefNo, IV.Narration
								, IV.Amount AS Receivable, ISNULL(IV.WrittenOffAmount,0) AS Received
                                , (ISNULL(IV.Amount,0)-ISNULL(IV.WrittenOffAmount,0)) AS Balance,  IV.CompanyCurrencyRate
                                , IVD.InvoiceId, IVD.Id InvoiceDetailId,IV.PartyType,'Invoice' OtherName
                                FROM  [TRN].InvoiceDetail AS IVD
								 JOIN TRN.Invoice IV ON IVD.InvoiceId=IV.Id
								 LEFT JOIN TRN.VoucherDetail VD ON VD.InvoiceDetailId=IVD.Id
								LEFT JOIN [TRN].[Voucher] AS V ON V.Id=IV.VoucherId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=IVD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=IVD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=IVD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=V.EntityId
								LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                                WHERE  V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND IV.PartyId='" + partyId + @"' AND IV.IsWrittenOff=0
								AND V.SourceType IN ('CustomerInvoice','SalesInvoice') 

								union all

								SELECT  VD.VoucherId,v.VoucherNo, v.SourceType,  VD.Id, VD.PartyType, VD.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, VD.PartyPlantId, PP.UserName AS PartyPlantName,  VD.Id AS VoucherDetailId, VD.EntityId
								, EN.UserName AS EntityName, VD.CurrencyId, C.Code AS CurrencyCode, VD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
								, VD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, VD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS DocDate
                                , REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.DocRefNo, VD.Narration
								, IV.Amount AS Receivable, ISNULL(IV.WrittenOffAmount,0) AS Received
                                , (ISNULL(IV.Amount,0)-ISNULL(IV.WrittenOffAmount,0)) AS Balance,  IV.CompanyCurrencyRate
                                , IVD.AdvanceId InvoiceId, IVD.Id InvoiceDetailId,IV.PartyType,'Advance' OtherName
                                FROM  [TRN].AdvanceDetail AS IVD
								 JOIN TRN.Advance IV ON IVD.AdvanceId=IV.Id
								 LEFT JOIN TRN.VoucherDetail VD ON VD.AdvanceDetailId=IVD.Id
								LEFT JOIN [TRN].[Voucher] AS V ON V.Id=IV.VoucherId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=IVD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=IVD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=IVD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=V.EntityId
								LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                                WHERE  V.CompanyGroupId='" + companyGroupId+"' AND V.CompanyId='"+companyId+"' AND V.PlantId='"+plantId+"' AND IV.PartyId='"+ partyId + @"' AND IV.IsWrittenOff=0 
								AND V.SourceType IN ('VendorAdvance')";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetPartyCrList( string companyGroupId, string companyId, string plantId, string partyId)
        {
            var sql = @"SELECT  VD.VoucherId,v.VoucherNo, v.SourceType,  VD.Id, VD.PartyType, VD.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, VD.PartyPlantId, PP.UserName AS PartyPlantName,  VD.Id AS VoucherDetailId, VD.EntityId
								, EN.UserName AS EntityName, VD.CurrencyId, C.Code AS CurrencyCode, VD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
								, VD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, VD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS DocDate
                                , REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.DocRefNo, VD.Narration
								, IV.Amount AS Receivable, ISNULL(IV.WrittenOffAmount,0) AS Received
                                , (ISNULL(IV.Amount,0)-ISNULL(IV.WrittenOffAmount,0)) AS Balance,  IV.CompanyCurrencyRate
                                , IVD.InvoiceId,IVD.Id InvoiceDetailId,IV.PartyType ,'Invoice' OtherName
                                FROM 
								 [TRN].InvoiceDetail AS IVD
								 JOIN TRN.Invoice IV ON IVD.InvoiceId=IV.Id
								 LEFT JOIN TRN.VoucherDetail VD ON VD.InvoiceDetailId=IVD.Id
								LEFT JOIN [TRN].[Voucher] AS V ON V.Id=IV.VoucherId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=IVD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=IVD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=IVD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=V.EntityId
								LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                                WHERE  V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND IV.PartyId='" + partyId + @"' AND IV.IsWrittenOff=0
								AND V.SourceType IN ('VendorInvoice','InventoryPayable') 

								UNION ALL

								SELECT  VD.VoucherId,v.VoucherNo, v.SourceType,  VD.Id, VD.PartyType, VD.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, VD.PartyPlantId, PP.UserName AS PartyPlantName,  VD.Id AS VoucherDetailId, VD.EntityId
								, EN.UserName AS EntityName, VD.CurrencyId, C.Code AS CurrencyCode, VD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
								, VD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, VD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS DocDate
                                , REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.DocRefNo, VD.Narration
								, IV.Amount AS Receivable, ISNULL(IV.WrittenOffAmount,0) AS Received
                                , (ISNULL(IV.Amount,0)-ISNULL(IV.WrittenOffAmount,0)) AS Balance,  IV.CompanyCurrencyRate
                                , IVD.AdvanceId InvoiceId, IVD.Id InvoiceDetailId,IV.PartyType,'Advance' OtherName
                                FROM 
								 [TRN].AdvanceDetail AS IVD
								 JOIN TRN.Advance IV ON IVD.AdvanceId=IV.Id
								 LEFT JOIN TRN.VoucherDetail VD ON VD.AdvanceDetailId=IVD.Id
								LEFT JOIN [TRN].[Voucher] AS V ON V.Id=IV.VoucherId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=IVD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=IVD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=IVD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=V.EntityId
								LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                                WHERE  V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND IV.PartyId='" + partyId + @"' AND IV.IsWrittenOff=0 
								AND V.SourceType IN ('CustomerAdvance')";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetPartyReconciliation(string companyGroupId, string companyId, string plantId, string column, string value, SourceType sourceType)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"DECLARE @plantId VARCHAR(10)='" + plantId + @"';
            select top 300 * from (SELECT  DISTINCT VW.VoucherId,V.VoucherNo, P.Code AS PartyCode,  REPLACE(CONVERT(VARCHAR(11), VW.PostingDate, 106), ' ', '-') AS PostingDate
									, REPLACE(CONVERT(VARCHAR(11), VW.DocDate, 106), ' ', '-') AS DocDate
									, VW.DocRefNo, C.Code AS CurrencyCode,VW.InvoiceWriteOffGroupNo,vw.InvoiceWriteOffNo
                                    , VW.PartyId,VW.PartyPlantId,P.UserName AS PartyName, PP.UserName AS PartyPlantName,VD.Amount
                                    , IsPark= case when VW.IsPark=1 then 'Parked' else 'Posted' end
                                    FROM [TRN].[InvoiceWriteOff] AS VW
									LEFT JOIN [TRN].Voucher V ON V.Id=VW.VoucherId
									LEFT JOIN(SELECT VoucherId,SUM(DrAmount) Amount FROM  TRN.VoucherDetail WHERE DrAmount>0 group by VoucherId)VD ON VD.VoucherId=V.Id  
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=VW.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VW.PartyPlantId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=VW.CurrencyId
                                    WHERE VW.Archive=0 AND VW.CompanyGroupId='" + companyGroupId + "' AND VW.CompanyId='" + companyId + "' AND VW.PlantId='" + plantId + "' AND VW.[SourceType]='" + sourceType + @"'
            ) AS TEMP WHERE " + strkey + " order by PostingDate DESC";
                return _sqlRepository.GetDataCollection(sql);
        }

        public Dictionary<string, object> GetAdvanceWriteOffReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var sql = @"SELECT top(1) E.UserName AS EntityName, FY.FiscalYearName, FY.YearPrefix, FYP.PeriodName, FYP.PeriodNo, VT.UserName AS VoucherTypeName, V.CurrencyId, C.Code AS CurrencyCode, V.VoucherNo
                        , REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.DocRefNo
                        , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, UPPER(V.Narration) AS Narration, V.IsPark, V.AddedBy, V.PostedBy, AWO.PartyType, P.Code AS PartyCode
                        , P.UserName AS PartyName, PP.UserName AS PartyPlantName, EI.EmployeeCode, EI.EmployeeName
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [ORG].[Entity] AS E ON E.Id=V.EntityId
                        LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                        LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                        LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                        LEFT JOIN [TRN].[InvoiceWriteOff] AS AWO ON AWO.VoucherId=V.Id
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=AWO.PartyId
                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AWO.PartyPlantId
                        LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=AWO.EmployeeId
                        WHERE V.Archive=0 AND V.Id='" + voucherId + "' AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(sql);
        }

        public List<Dictionary<string, object>> GetAdvanceWriteOffReportData(string companyId, string voucherId)
        {
            var sql = @"SELECT GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, B.Code AS BudgetCode, B.UserName AS BudgetName, A.Code AS ActivityCode
                    , A.UserName AS ActivityName, VD.BankMasterId, BNKM.AccountNumber, BNKM.AccountTitle, VD.DrAmount, VD.CrAmount, CC.CompanyCurrencyDrAmount, CC.CompanyCurrencyCrAmount
                        FROM [TRN].[VoucherDetail] AS VD
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=VD.BudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
						LEFT JOIN [MST].[BankMaster] AS BNKM ON BNKM.Id=VD.BankMasterId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE VD.VoucherId='" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataCollection(sql);
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

        public IWorkbook GetPartyReconciliationReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var headerData = GetAdvanceWriteOffReportHeader(companyGroupId, companyId, plantId, voucherId, SourceType.PartyReconcilliation);

            // Set report Name
            reportFileName = Convert.ToDateTime(headerData["PostingDate"]).ToString("yyMMdd") + " " + headerData["VoucherNo"];

            var row = 5;
            report.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            report.SetText(ref sheet, row, 2, headerData["VoucherNo"].ToString());
            row++;

            report.SetMasterHeaderText(ref sheet, row, 1, "Doc Date");
            report.SetText(ref sheet, row, 2, headerData["DocDate"].ToString());
            row++;

            report.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            report.SetText(ref sheet, row, 2, headerData["PostingDate"].ToString());
            row++;

            report.SetMasterHeaderText(ref sheet, row, 1, "Vendor");
            report.SetText(ref sheet, row, 2, headerData["PartyName"].ToString());
            row++;


            report.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            report.SetText(ref sheet, row, 2, headerData["Narration"].ToString());

            sheet.Range[row, 2, row, 5].Merge();

            var _rowR = 5;
            report.SetMasterHeaderText(ref sheet, _rowR, 3, "Voucher Date");
            report.SetText(ref sheet, _rowR, 4, headerData["VoucherDate"].ToString());
            sheet.Range[_rowR, 4, _rowR, 5].Merge();
            _rowR++;

            report.SetMasterHeaderText(ref sheet, _rowR, 3, "Doc No");
            report.SetText(ref sheet, _rowR, 4, headerData["DocRefNo"].ToString());
            sheet.Range[_rowR, 4, _rowR, 5].Merge();
            _rowR++;

            report.SetMasterHeaderText(ref sheet, _rowR, 3, "Fiscal Year");
            report.SetText(ref sheet, _rowR, 4, headerData["FiscalYearName"] + " (" + headerData["PeriodNo"] + ")");
            sheet.Range[_rowR, 4, _rowR, 5].Merge();
            _rowR++;

            report.SetMasterHeaderText(ref sheet, _rowR, 3, "Status");
            report.SetText(ref sheet, _rowR, 4, Convert.ToBoolean(headerData["IsPark"]) ? "Parked" : "Posted");
            sheet.Range[_rowR, 4, _rowR, 5].Merge();
            GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var _rowL = 11;
            var headreColIndex = 1;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "GL", 32); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Budget", 32); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Activity", 32); headreColIndex++;

            var sumdrcrCol = headreColIndex;
            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                report.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, headerData["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].Merge();
                report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", 12);
                headreColIndex++;
                report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight);
                headreColIndex++;
            }
            double _Total_Amount = 0;
            report.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, companyCurrencyCode, ExcelHAlign.HAlignCenter);
            sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].Merge();
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", ExcelHAlign.HAlignRight); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight);

            var shet2EndxlsCol = headreColIndex;
            double vAmount = 0;
            var col = 1;
            var Row_Total_Start = _rowL + 1;

            var data = GetAdvanceWriteOffReportData(companyId, voucherId);
            for (int n = 0; n < data.Count; n++)
            {
                _rowL++;
                col = 1;
                report.SetText(ref sheet, _rowL, col, data[n]["GLGeneralInfoCode"] + " - " + data[n]["GLGeneralInfoName"]); col++;
                report.SetText(ref sheet, _rowL, col, data[n]["BudgetName"].ToString()); col++;
                report.SetText(ref sheet, _rowL, col, data[n]["ActivityName"].ToString()); col++;
                if (companyCurrencyId != headerData["CurrencyId"].ToString())
                {
                    report.SetText(ref sheet, _rowL, col, Convert.ToDouble(data[n]["DrAmount"])); col++;
                    report.SetText(ref sheet, _rowL, col, Convert.ToDouble(data[n]["CrAmount"])); col++;
                    vAmount += Convert.ToDouble(data[n]["DrAmount"].ToString());
                }
                report.SetText(ref sheet, _rowL, col, Convert.ToDouble(data[n]["CompanyCurrencyDrAmount"].ToString())); col++;
                report.SetText(ref sheet, _rowL, col, Convert.ToDouble(data[n]["CompanyCurrencyCrAmount"].ToString()));
                _Total_Amount += Convert.ToDouble(data[n]["CrAmount"].ToString());
            }

            _rowL++;
            report.SetText(ref sheet, _rowL, 1, "Total :", true);
            sheet.Range[_rowL, 1, _rowL, sumdrcrCol - 1].Merge();

            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
                sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
                sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                sumdrcrCol++;

                sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
                sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
                sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                sumdrcrCol++;
            }

            sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
            sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
            sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
            sumdrcrCol++;

            sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
            sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
            sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
            sumdrcrCol++;

            sheet.Range[12, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[12, 1, _rowL, shet2EndxlsCol].BorderAround(ExcelLineStyle.Hair);

            _rowL += 1;
            var _col = 2;
            report.SetText(ref sheet, _rowL, 1, "In Word:", true);
            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                var _amountValue = report.InWord(vAmount, headerData["CurrencyId"].ToString());
                sheet.Range[report.GetColumnNameForXls(_col) + _rowL].Text = _amountValue;
                sheet.Range[report.GetColumnNameForXls(_col) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
                sheet.Range[report.GetColumnNameForXls(_col) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[report.GetColumnNameForXls(_col) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[report.GetColumnNameForXls(_col) + _rowL].CellStyle.Font.Bold = true;
                _rowL++;
                _col++;
            }

            var _amount = report.InWord(_Total_Amount, companyCurrencyId);
            sheet.Range[report.GetColumnNameForXls(_col) + _rowL].Text = _amount;
            sheet.Range[report.GetColumnNameForXls(_col) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
            sheet.Range[report.GetColumnNameForXls(_col) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[report.GetColumnNameForXls(_col) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[report.GetColumnNameForXls(_col) + _rowL].CellStyle.Font.Bold = true;

            _rowL = _rowL + 4;
            report.SetSignatureText(ref sheet, _rowL - 1, 1, headerData["AddedBy"].ToString());
            sheet.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, 1, "Prepared By", true);

            report.SetSignatureText(ref sheet, _rowL - 1, 3, headerData["PostedBy"].ToString());
            sheet.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, 3, "Checked By", true);

            sheet.Range[_rowL, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, 5, "Authorized By", true);

            sheet.UsedRange.AutofitColumns();
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, shet2EndxlsCol, headerData["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            return workbook;
        }

    }
}
