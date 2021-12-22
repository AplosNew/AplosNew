using Library.Data.Sql;
using Library.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.MaterialManagement.Material
{
    public class clsInvoiceTagWithLc
    {
        ISqlRepository _sqlRepository;
        public clsInvoiceTagWithLc()
        {
            _sqlRepository = new SqlRepository();
        }
        public IEnumerable<object> VendorAvailableInvoiceList(string companyGroupId, string companyId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @" SELECT IVD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, IVD.BudgetMasterId, B.UserName AS BudgetName, IVD.ActivityId, EN.UserName AS EntityName, A.UserName AS ActivityName,
		                                V.VoucherNo, Replace(CONVERT(VARCHAR(11), IV.DocDate, 106), ' ', '-') DocDate ,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate, IV.DocRefNo, IV.Narration, IV.Id AS InvoiceId,VD.EntityId,VD.PlantId, IVD.Id AS InvoiceDetailId, IV.VoucherId,
		                                VD.Id AS VoucherDetailId, IV.CurrencyId, C.Code AS CurrencyCode, IV.PartyId, IVD.Amount AS Receivable,V.ExchangeType, 0 ExchangeAmount,
		                                IVD.WrittenOffAmount AS Received, IVD.Amount-IVD.WrittenOffAmount AS Balance, IV.PartyPlantId, PP.UserName AS PartyPlantName,
										CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion,
										GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion,
										HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion
								                                ,Particular=REPLACE(REPLACE(
										STUFF((SELECT DISTINCT ','+xpo.UserName from
											hkp.Activity xpo
											INNER JOin TRN.VoucherDetail xPDAMAP on xpo.id=xPDAMAP.ActivityId
											WHERE VD.ActivityId!=xPDAMAP.ActivityId and xPDAMAP.VoucherId=V.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
								                                	,AcceptanceNo=STUFF((select distinct ','+ XPDA.AcceptanceNo from
										TRN.PurchaseDocAcceptance XPDA  
										LEFT JOIN TRN.Voucher XV ON XV.Id=XPDA.VoucherId
										where XV.Id=V.Id   for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

										,LCRef=STUFF((select distinct ','+ XLC.LCRef from
										dbo.PurchaseLC  XLC LEFT JOIN TRN.PurchaseDocAcceptance XPDA  ON XPDA.PurchaseLCId=XLC.Id
										LEFT JOIN TRN.Voucher XV ON XV.Id=XPDA.VoucherId
										where XV.Id=V.Id   for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								                                ,ContractNo=STUFF((select distinct ','+ XC.ContractNo from
										dbo.PurchaseLC  XLC JOIN TRN.PurchaseDocAcceptance XPDA  ON XPDA.PurchaseLCId=XLC.Id
								                                left join dbo.Contract XC ON XC.Id=XLC.ContractId
										LEFT JOIN TRN.Voucher XV ON XV.Id=XPDA.VoucherId
										where XV.Id=V.Id   for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

										 ,Customer=STUFF((select distinct ','+ XP.UserName from
										dbo.PurchaseLC  XLC JOIN TRN.PurchaseDocAcceptance XPDA  ON XPDA.PurchaseLCId=XLC.Id
								                                left join dbo.Contract XC ON XC.Id=XLC.ContractId
										LEFT JOIN HKP.Party XP ON XP.Id=XC.CustomerId
										LEFT JOIN TRN.Voucher XV ON XV.Id=XPDA.VoucherId
										where XV.Id=V.Id   for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

										,MasterLCNo=STUFF((select distinct ','+ MLC.LCRef from
										dbo.PurchaseLC  XLC JOIN TRN.PurchaseDocAcceptance XPDA  ON XPDA.PurchaseLCId=XLC.Id
								                                left join dbo.Contract XC ON XC.Id=XLC.ContractId
										LEFT JOIN dbo.MasterLC MLC ON MLC.Id=XC.MasterLCId
										LEFT JOIN TRN.Voucher XV ON XV.Id=XPDA.VoucherId
										where XV.Id=V.Id   for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                                FROM [TRN].[InvoiceDetail] AS IVD
								                                LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId=IV.Id
									    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
								                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IVD.Id
								                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=IV.VoucherId
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
								                                WHERE IV.Archive=0 AND IV.IsWrittenOff=0 AND IVD.IsWrittenOff=0  AND V.IsPark=0 AND IVD.IsBlock=0 AND IV.SourceType in ('" + SourceType.VendorInvoice + "','" + SourceType.PurchaseDocAcceptance + "','" + SourceType.SuspensePayable + "','" + SourceType.ServicePayable + "','" + SourceType.EmployeePayable + @"')
								                                AND IV.CompanyGroupId='" + companyGroupId + @"' AND IV.CompanyId='" + companyId + @"' 
								                            UNION ALL
								                            SELECT IVD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, IVD.BudgetMasterId, B.UserName AS BudgetName, IVD.ActivityId, EN.UserName AS EntityName, A.UserName AS ActivityName,
								                                V.VoucherNo, Replace(CONVERT(VARCHAR(11), IV.DocDate, 106), ' ', '-') DocDate ,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate, IV.DocRefNo, IV.Narration, IV.Id AS InvoiceId,VD.EntityId,VD.PlantId, IVD.Id AS InvoiceDetailId, IV.VoucherId,
								                                VD.Id AS VoucherDetailId, IV.CurrencyId, C.Code AS CurrencyCode, IV.PartyId, IVD.NetAmount AS Receivable,V.ExchangeType, 0 ExchangeAmount,
								                                IVD.WrittenOffAmount AS Received, IVD.NetAmount-IVD.WrittenOffAmount AS Balance, IV.PartyPlantId, PP.UserName AS PartyPlantName,
										CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion,
										GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion,
										HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion
								                                ,Particular=REPLACE(REPLACE(
										STUFF((SELECT DISTINCT ','+xpo.UserName from
											hkp.Activity xpo
											INNER JOin TRN.VoucherDetail xPDAMAP on xpo.id=xPDAMAP.ActivityId
											WHERE VD.ActivityId!=xPDAMAP.ActivityId and xPDAMAP.VoucherId=V.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
								                                ,NULL AcceptanceNo ,NULL LCRef,NULL ContractNo,NULL Customer, NULL MasterLCNo

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
								                                LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IV.InventoryReceiveId
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
								                                WHERE IV.Archive=0 AND IV.IsWrittenOff=0 AND IVD.IsWrittenOff=0  AND V.IsPark=0 AND IVD.IsBlock=0 AND IV.SourceType in ('" + SourceType.InventoryPayable + @"')
								                                AND IV.CompanyGroupId='" + companyGroupId + @"' AND IV.CompanyId='" + companyId + @"'  AND IR.PurchaseDocumentAcceptanceId IS NULL";
				return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
    }
}
