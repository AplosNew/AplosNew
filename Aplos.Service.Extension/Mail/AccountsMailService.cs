using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using Library.Model.Accounts;
using Library.Service.Core;
using Library.Model.Inventory;
using Library.Service.Systems;
using Library.Data;
using Syncfusion.XlsIO;

namespace Library.Service.Extension.Mail
{
   //public interface IRrportUtility
   // {
   //     void PageSetup(ref IWorksheet sheet, int xlsColumnHeader, ExcelPageOrientation po);
   //     void PlantHeader(ref IWorksheet sheet, int lastCol, string sheetHeader, string plantId);
   // }
    public class AccountsMailService
    {
        SqlRepository _sqlRepository;
              //public AccountsMailService()
        //{
        //    _sqlRepository = new SqlRepository();
        //}
        public AccountsMailService()
        {
            _sqlRepository = new SqlRepository();
        }
      

        #region auto mail
        public DataTable GetAutoMailLastFewDaysPayableCreatedData(string companyGroupId, string companyId, string plantId)
        {
            var sql = @"select * from (
	                                 SELECT IVD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, IVD.BudgetMasterId, B.UserName AS BudgetName
									 , IVD.ActivityId, EN.UserName AS EntityName, A.UserName AS ActivityName,V.VoucherNo, format(V.VoucherDate,'dd-MMM-yyyy') EntryDate   --Replace(Convert(varchar(11), V.VoucherDate, 106), ' ', '-') EntryDate 
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

        public DataTable GetAutoMailLastFewDaysPaymentMadeReportData(string companyGroupId, string companyId, string plantId)
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

        public DataTable GetDTaccountDelayPosting(string companyGroupId, string plantId)
        {
            try
            {
                string strSql = @"Select  ItemWExpense.BudgetName,ItemWExpense.BudgetId,ItemWExpense.BudgetCategoryName,ItemWExpense.BudgetSubCategoryName
                                , SUM(CASE WHEN ISNULL(ItemWExpense.DRcumulative, 0) = 0 THEN ItemWExpense.CRcumulative ELSE ItemWExpense.DRcumulative END) Amount
		                        , CategorySequence,SubcategorySequence,ItemSequence,PostingPeriod,EntryPeriod,PostingPeriodId,EntryPeriodId,VoucherNo,AddedDate,PostingDate

                                  FROM(
                                         SELECT B.UserName AS BudgetName, B.Id AS BudgetId,
                                BC.UserName  AS BudgetCategoryName,
                                BSC.UserName AS BudgetSubCategoryName


                                   , DRcumulative = CASE WHEN ACT.BalanceType = 'Debit' THEN SUM(VDC.DrAmount) - SUM(VDC.CrAmount) ELSE 0 END
                                , CRcumulative = CASE WHEN ACT.BalanceType = 'Credit' THEN SUM(VDC.crAmount) - SUM(VDC.DrAmount) ELSE 0 END
                                , BC.Sequence CategorySequence, BSC.Sequence SubcategorySequence, B.Sequence ItemSequence
                                , V.VoucherNo, FORMAT(V.AddedDate, 'dd-MMM-yyyy') AddedDate, FORMAT(V.PostingDate, 'dd-MMM-yyyy') PostingDate
                                --, SUM(VDC.DrAmount) Amount
                                , EFYP.PeriodName PostingPeriod, FYPA.PeriodName EntryPeriod, EFYP.Id PostingPeriodId, FYPA.EntryPeriodId

                                FROM TRN.VoucherDetailCurrency AS VDC

                                JOIN TRN.VoucherDetail AS VD ON VD.Id = VDC.VoucherDetailId

                                JOIN TRN.Voucher AS V ON V.Id = VD.VoucherId

                                LEFT JOIN ORG.Company AS CMP on CMP.Id = V.CompanyId

                                LEFT JOIN ORG.CompanyGroup AS CMPGR on CMPGR.Id = V.CompanyGroupId

                                LEFT JOIN MST.BudgetMaster AS BM ON BM.Id = VD.BudgetMasterId

                                LEFT JOIN HKP.Budget AS B ON B.Id = BM.BudgetId

                                LEFT JOIN HKP.BudgetSubCategory AS BSC ON BSC.Id = BM.BudgetSubCategoryId

                                LEFT JOIN HKP.BudgetCategory AS BC ON BC.Id = BM.BudgetCategoryId

                                LEFT JOIN SCS.FiscalYearPeriod AS EFYP ON EFYP.Id = VD.FiscalYearPeriodId


                                LEFT JOIN ORG.Entity AS E ON E.Id = V.EntityId

                                LEFT JOIN ORG.Company AS C ON C.Id = V.CompanyId

                                LEFT JOIN[ORG].[Plant] ON Plant.Id = E.PlantId

                                LEFT JOIN[ORG].[Division] ON Division.Id = E.DivisionId

                                LEFT JOIN[ORG].[SubDivision] ON SubDivision.Id = E.SubDivisionId

                                LEFT JOIN[ORG].[Unit] ON Unit.Id = E.UnitId

                                LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id = BM.GLGeneralInfoId

                                LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GL.AccountGroupId

                                LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId
                                LEFT OUTER JOIN(SELECT Id AS EntryPeriodId, PeriodName, StartDate, EndDate FROM SCS.FiscalYearPeriod)AS

                                    FYPA ON MONTH(CONVERT(DATE, FYPA.EndDate)) = MONTH(CONVERT(DATE, VD.AddedDate))

                                    AND  YEAR(CONVERT(DATE, FYPA.EndDate)) = YEAR(CONVERT(DATE, VD.AddedDate))

                                LEFT JOIN(SELECT * FROM SCS.CompanyParallelCurrency WHERE ParallelCurrencyType = 'CompanyCurrency') as
                                 CPC ON CPC.CurrencyId = VDC.ParallelCurrencyId

                                WHERE   ACT.IsBalanceSheet = 0  AND ACT.Id = 'Expense' AND V.IsPark = 0   AND  VD.BudgetMasterId  IS NOT NULL


                            AND DATEDIFF(Day, FORMAT(V.PostingDate, 'dd-MMM-yyyy'), FORMAT(V.AddedDate, 'dd-MMM-yyyy')) > 10                     

                                 AND  C.Id = 'C20171'GROUP BY B.Id, B.UserName, BC.UserName, BSC.UserName, ACT.BalanceType
                                , BC.Sequence, BSC.Sequence, B.Sequence, EFYP.PeriodName, FYPA.PeriodName, EFYP.Id, FYPA.EntryPeriodId, V.VoucherNo, V.AddedDate, V.PostingDate) ItemWExpense
                                   where Convert(Date,ItemWExpense.AddedDate) = Convert(date,GETDATE()-1)

                                 GROUP BY ItemWExpense.BudgetName,ItemWExpense.BudgetId,ItemWExpense.BudgetCategoryName,ItemWExpense.BudgetSubCategoryName
                                  ,CategorySequence,SubcategorySequence,ItemSequence,EntryPeriod,PostingPeriod,PostingPeriodId,EntryPeriodId,VoucherNo,AddedDate,PostingDate";

                return _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
