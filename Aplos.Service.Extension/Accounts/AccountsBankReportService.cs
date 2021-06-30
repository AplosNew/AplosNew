using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Banks;
using Library.Service.Enums;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Library.Service.Extension.Accounts
{
    public class AccountsBankReportService
    {
        private readonly ISqlRepository _sqlRepository;
        public AccountsBankReportService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
        public DataTable GetBankBookLedgerData(string companyGroupId, string companyId, string plantId, string bankMasterId, string fromDate, string toDate)
        {
            var cmdText = @"DECLARE @companyGroupId VARCHAR(10)='" + companyGroupId + @"';
                        DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                        DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        DECLARE @bankMasterId VARCHAR(10)='" + bankMasterId + @"';
    
                        SELECT V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.CurrencyId,
                          isnull(GLT.DrAmount,0) DrAmount,
                         isnull(GLT.CrAmount,0) CrAmount
						 , V.Narration
                        ,isnull(CC.CompanyCurrencyDrAmount,0) CompanyCurrencyDrAmount, isnull(CC.CompanyCurrencyCrAmount,0) CompanyCurrencyCrAmount
						,OtherSide= STUFF((select distinct ','+ CASE 
															WHEN XPP.UserName<>'' THEN XPP.UserName
															WHEN XBM.AccountTitle<>'' THEN XBM.AccountTitle
															WHEN XCM.UserName<>'' THEN XCM.UserName
															WHEN XEI.EmployeeName <>'' THEN XEI.EmployeeName
															ELSE XA.UserName	END from
														[TRN].[VoucherDetail] XVD  join [TRN].Voucher AS XV  ON XV.Id=XVD.VoucherId
														left join HKP.PartyPlant XPP ON XPP.Id=XVD.PartyPlantId
														LEFT JOIN MST.BankMaster XBM ON XBM.Id=XVD.BankMasterId
														LEFT JOIN MST.CashMaster XCM ON XCM.Id=XVD.CashMasterId
														LEFT JOIN DBO.EmployeeInformation XEI ON XEI.SystemId=XVD.EmployeeId
														LEFT JOIN HKP.Activity XA ON XA.Id=XVD.ActivityId
													where	V.Id=XV.Id AND (isnull(XVD.BankMasterId,'')='' OR (isnull(XVD.BankMasterId,'')<>'') AND XVD.BankMasterId<>@bankMasterId)   for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                        FROM [TRN].[GLTransactionDetail] AS GLT 
                        JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=GLT.VoucherDetailId
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                                ,VDC.ToCurrencyRate
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id

                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId  AND VD.BankMasterId=@bankMasterId
						 AND V.PostingDate BETWEEN '"+fromDate+"' AND '"+toDate+@"' AND V.SourceType!='OpeningBalance'
                            UNION
                        SELECT V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, V.CurrencyId,
                          isnull(GLT.DrAmount,0) DrAmount,
                         isnull(GLT.CrAmount,0) CrAmount
						 , V.Narration
                         ,isnull(CC.CompanyCurrencyDrAmount,0) CompanyCurrencyDrAmount, isnull(CC.CompanyCurrencyCrAmount,0) CompanyCurrencyCrAmount
						,OtherSide= STUFF((select distinct ','+ CASE 
															WHEN XPP.UserName<>'' THEN XPP.UserName
															WHEN XBM.AccountTitle<>'' THEN XBM.AccountTitle
															WHEN XCM.UserName<>'' THEN XCM.UserName
															WHEN XEI.EmployeeName <>'' THEN XEI.EmployeeName
															ELSE XA.UserName	END from
														[TRN].[VoucherDetail] XVD  join [TRN].Voucher AS XV  ON XV.Id=XVD.VoucherId
														left join HKP.PartyPlant XPP ON XPP.Id=XVD.PartyPlantId
														LEFT JOIN MST.BankMaster XBM ON XBM.Id=XVD.BankMasterId
														LEFT JOIN MST.CashMaster XCM ON XCM.Id=XVD.CashMasterId
														LEFT JOIN DBO.EmployeeInformation XEI ON XEI.SystemId=XVD.EmployeeId
														LEFT JOIN HKP.Activity XA ON XA.Id=XVD.ActivityId
													where	V.Id=XV.Id AND (isnull(XVD.BankMasterId,'')='' OR (isnull(XVD.BankMasterId,'')<>'') AND XVD.BankMasterId<>@bankMasterId)   for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                        FROM [TRN].[GLTransactionDetail] AS GLT 
                        JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=GLT.VoucherDetailId
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                            ,VDC.ToCurrencyRate
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id

                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId  AND  VD.BankMasterId=@bankMasterId
						 AND V.PostingDate > '"+toDate+@"' AND V.SourceType='OpeningBalance'
                        ORDER BY PostingDate ASC";
            return _sqlRepository.GetDataTable(cmdText);
        }
        public List<Dictionary<string, object>> GetBankOpeningBalanceLedgerData(string companyGroupId, string companyId, string plantId, string bankMasterId, string fromDate)
        {
            var sql = @"SELECT SUM(DrAmount) - SUM(CrAmount) AS OB
                        , CompanyCurrencyId, SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyCurrencyOB
                         FROM (
                        SELECT SUM(GLTD.DrAmount) AS DrAmount, SUM(GLTD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN [TRN].[GLTransactionDetail] AS GLTD ON GLTD.VoucherDetailId=VD.Id AND GLTD.BankMasterId=VD.BankMasterId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='"+companyId+@"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='"+companyGroupId+"' AND V.CompanyId='"+companyId+"' AND V.PlantId='"+plantId+@"' AND VD.BankMasterId='"+bankMasterId+"' AND V.PostingDate < '"+fromDate+@"'
                        GROUP BY CC.CompanyCurrencyId
                        UNION
                        SELECT SUM(GLTD.DrAmount) AS DrAmount, SUM(GLTD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                       
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN [TRN].[GLTransactionDetail] AS GLTD ON GLTD.VoucherDetailId=VD.Id AND GLTD.BankMasterId=VD.BankMasterId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='"+companyId+@"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='"+companyGroupId+"' AND V.CompanyId='"+companyId+"' AND V.PlantId='"+plantId+@"' AND VD.BankMasterId='"+bankMasterId+"' AND V.PostingDate ='"+fromDate+@"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId";
            return _sqlRepository.GetDataCollection(sql);
        }


    }
}
