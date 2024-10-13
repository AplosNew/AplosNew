using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Vouchers;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.ViewModel.OrderManagements;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Accounting.Accounts
{
    public class AccountsInvoiceService
    {
        private readonly ISqlRepository _sqlRepository;
        public AccountsInvoiceService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
        public GridModel GetCustomerAvailableInvoiceList(GridParameter parameters, string companyGroupId, string companyId, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT I.CompanyId, I.PlantId, VD.EntityId, EN.UserName AS EntityName, I.PartyType, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, I.PartyPlantId, PP.UserName AS PartyPlantName, I.Id AS InvoiceId
                                        , ID.Id AS InvoiceDetailId, I.VoucherId, V.VoucherNo, I.SalesTypeId, I.InvoiceNo, VD.Id AS VoucherDetailId, I.CurrencyId, C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount, B.UserName AS BudgetName, ID.ActivityId
                                        , A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11), I.PostingDate, 106), ' ', '-') AS PostingDate
                                        , I.DocRefNo, I.Narration, ISNULL(ID.NetAmount,0) AS Receivable, (ISNULL(ID.WrittenOffAmount,0)) AS Received, (ISNULL(ID.NetAmount,0)- (ISNULL(ID.WrittenOffAmount,0))) AS Balance
                                        , CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate, CC.CompanyCurrencyConversion, V.TransactionRefNo, I.SalesOrderNo
                                        FROM [TRN].[InvoiceDetail] AS ID
                                        LEFT JOIN [TRN].[Invoice] AS I ON I.Id=ID.InvoiceId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [TRN].[AdjustmentNoteDetail] AS AJD ON AJD.InvoiceDetailId=ID.Id
                                        LEFT JOIN [TRN].[AdjustmentNote] AS AJ ON AJ.Id=AJD.AdjustmentNoteId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=ID.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND I.IsWrittenOff=0 AND ID.IsWrittenOff=0 AND ID.IsBlock=0 AND (I.SourceType='" + SourceType.CustomerInvoice + "' OR I.SourceType='" + SourceType.SalesInvoice + @"')
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "' AND I.PlantId='" + plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetVendorAvailableInvoiceNewList(GridParameter parameters, string companyGroupId, string companyId, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT I.CompanyId, I.PlantId, VD.EntityId, EN.UserName AS EntityName, I.PartyType, I.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, I.PartyPlantId, PP.UserName AS PartyPlantName, I.Id AS InvoiceId
                                        , ID.Id AS InvoiceDetailId, I.VoucherId, V.VoucherNo, I.SalesTypeId, I.InvoiceNo, VD.Id AS VoucherDetailId, I.CurrencyId, C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount, B.UserName AS BudgetName, ID.ActivityId
                                        , A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11), I.PostingDate, 106), ' ', '-') AS PostingDate
                                        , I.DocRefNo, I.Narration, ISNULL(ID.NetAmount,0) AS Receivable, (ISNULL(ID.WrittenOffAmount,0)) AS Received, (ISNULL(ID.NetAmount,0)- (ISNULL(ID.WrittenOffAmount,0))) AS Balance
                                        , CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate, CC.CompanyCurrencyConversion, V.TransactionRefNo, I.SalesOrderNo
                                        FROM [TRN].[InvoiceDetail] AS ID
                                        LEFT JOIN [TRN].[Invoice] AS I ON I.Id=ID.InvoiceId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [TRN].[AdjustmentNoteDetail] AS AJD ON AJD.InvoiceDetailId=ID.Id
                                        LEFT JOIN [TRN].[AdjustmentNote] AS AJ ON AJ.Id=AJD.AdjustmentNoteId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=ID.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
                                    WHERE I.Archive=0 AND I.IsWrittenOff=0 AND ID.IsWrittenOff=0 AND ID.IsBlock=0 AND I.SourceType IN ('" + SourceType.VendorInvoice + "','" + SourceType.SuspensePayable + @"')
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "' AND I.PlantId='" + plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetCustomerAvailableInvoiceList(GridParameter parameters, string companyGroupId, string companyId, string plantId, string partyId)
        {
            try
            {
                parameters.CmdText = @" SELECT I.CompanyId, I.PlantId, I.PartyPlantId, I.PartyType, I.Id AS InvoiceId, ID.Id AS InvoiceDetailId, I.VoucherId, V.VoucherNo, VD.EntityId, EN.UserName AS EntityName, I.SalesTypeId, I.InvoiceNo, I.PartyId, VD.Id AS VoucherDetailId, I.CurrencyId
                                    , C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount
                                    , B.UserName AS BudgetName, ID.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11)
                                    , I.PostingDate, 106), ' ', '-') AS PostingDate, I.DocRefNo, I.Narration, ISNULL(ID.NetAmount,0) AS Receivable, (ISNULL(ID.WrittenOffAmount,0)) AS Received
                                    , PP.UserName AS PartyPlantName
                                    , (ISNULL(ID.NetAmount,0)- (ISNULL(ID.WrittenOffAmount,0))) AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate
                                    , CC.CompanyCurrencyConversion, V.TransactionRefNo, I.SalesOrderNo
                                    , SONo =isnull( STUFF((select distinct ','+XVD.Id from TRN.SalesOrder XVD
											LEFT JOIN TRN.SalesMaterial SM on SM.SalesOrderId=XVD.Id
											LEFT JOIN TRN.Sales S ON S.Id=SM.SalesId
											LEFT JOIN TRN.Voucher XV ON XV.Id=S.VoucherId
											where XV.Id=V.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

                                    FROM [TRN].[InvoiceDetail] AS ID
                                    LEFT JOIN [TRN].[Invoice] AS I ON I.Id=ID.InvoiceId
									LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                    LEFT JOIN [TRN].[AdjustmentNoteDetail] AS AJD ON AJD.InvoiceDetailId=ID.Id
                                    LEFT JOIN [TRN].[AdjustmentNote] AS AJ ON AJ.Id=AJD.AdjustmentNoteId
                                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=ID.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                    LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
									
                                     WHERE I.Archive=0 AND I.IsWrittenOff=0 AND ID.IsWrittenOff=0 AND I.IsPark=0 AND ID.IsBlock=0 AND I.SourceType in ('" + SourceType.CustomerInvoice + "','FixedAssetDisposeJournal','InventorySales' ,'" + SourceType.SalesInvoice + @"')
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "' AND I.PlantId='" + plantId + "' AND I.PartyId='" + partyId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public GridModel GetVendorAvailableInvoiceListForCreditNotes(GridParameter parameters, string companyGroupId, string companyId, string plantId, string partyId)
        {
            try
            {
                parameters.CmdText = @" SELECT I.CompanyId, I.PlantId, I.PartyPlantId, I.PartyType, I.Id AS InvoiceId, ID.Id AS InvoiceDetailId, I.VoucherId, V.VoucherNo, VD.EntityId, EN.UserName AS EntityName, I.SalesTypeId, I.InvoiceNo, I.PartyId, VD.Id AS VoucherDetailId, I.CurrencyId
                                    , C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount
                                    , B.UserName AS BudgetName, ID.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11)
                                    , I.PostingDate, 106), ' ', '-') AS PostingDate, I.DocRefNo, I.Narration, ISNULL(ID.NetAmount,0) AS Receivable,ID.AdditionalAmount, (ISNULL(ID.WrittenOffAmount,0)) AS Received
                                    , PP.UserName AS PartyPlantName
                                    , ((ISNULL(ID.NetAmount,0)+ID.AdditionalAmount)- (ISNULL(ID.WrittenOffAmount,0))) AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate
                                    , CC.CompanyCurrencyConversion, V.TransactionRefNo, I.SalesOrderNo
                                    , SONo =isnull( STUFF((select distinct ','+XVD.Id from TRN.SalesOrder XVD
											LEFT JOIN TRN.SalesMaterial SM on SM.SalesOrderId=XVD.Id
											LEFT JOIN TRN.Sales S ON S.Id=SM.SalesId
											LEFT JOIN TRN.Voucher XV ON XV.Id=S.VoucherId
											where XV.Id=V.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

                                    FROM [TRN].[InvoiceDetail] AS ID
                                    LEFT JOIN [TRN].[Invoice] AS I ON I.Id=ID.InvoiceId
									LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                    LEFT JOIN [TRN].[AdjustmentNoteDetail] AS AJD ON AJD.InvoiceDetailId=ID.Id
                                    LEFT JOIN [TRN].[AdjustmentNote] AS AJ ON AJ.Id=AJD.AdjustmentNoteId
                                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=ID.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                    LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
									
                                     WHERE I.Archive=0 AND I.IsWrittenOff=0 AND ID.IsWrittenOff=0 AND I.IsPark=0 AND ID.IsBlock=0 AND I.SourceType in ('VendorInvoice','PurchaseDocAcceptance','SuspensePayable','ServicePayable','EmployeePayable','PostInvoice','InvoiceToAcceptance','InventoryPayable')
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "' AND I.PlantId='" + plantId + "' AND I.PartyId='" + partyId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public List<Dictionary<string, object>> GetCustomerAvailableInvoiceList(string companyGroupId, string companyId, string plantId)
        {
            try
            {
                var CmdText = @" SELECT I.CompanyId, I.PlantId, I.PartyPlantId, I.PartyType, I.Id AS InvoiceId, ID.Id AS InvoiceDetailId, I.VoucherId, V.VoucherNo, VD.EntityId, EN.UserName AS EntityName, I.SalesTypeId, I.InvoiceNo, I.PartyId, VD.Id AS VoucherDetailId, I.CurrencyId
                                    , C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount
                                    , B.UserName AS BudgetName, ID.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11)
                                    , I.PostingDate, 106), ' ', '-') AS PostingDate, I.DocRefNo, I.Narration, ISNULL(ID.NetAmount,0) AS Receivable, (ISNULL(ID.WrittenOffAmount,0)) AS Received
                                    , PP.UserName AS PartyPlantName
                                    , (ISNULL(ID.NetAmount,0)- (ISNULL(ID.WrittenOffAmount,0))) AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate
                                    , CC.CompanyCurrencyConversion, GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion, HC.HardCurrencyId, HC.HardFromCurrencyId
                                    , HC.HardCurrencyRate, HC.HardCurrencyConversion, V.TransactionRefNo, I.SalesOrderNo
                                    FROM [TRN].[InvoiceDetail] AS ID
                                    LEFT JOIN [TRN].[Invoice] AS I ON I.Id=ID.InvoiceId
									LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                    LEFT JOIN [TRN].[AdjustmentNoteDetail] AS AJD ON AJD.InvoiceDetailId=ID.Id
                                    LEFT JOIN [TRN].[AdjustmentNote] AS AJ ON AJ.Id=AJD.AdjustmentNoteId
                                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=ID.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                    LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
									LEFT JOIN (
									SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,0 ToCurrencyRate,
										VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.DrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS GC ON GC.VoucherDetailId=VD.Id
									LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS HardCurrencyId, VDC.FromCurrencyId AS HardFromCurrencyId, VDC.ToCurrencyId,0 ToCurrencyRate,
										VDC.ToCurrencyRate AS HardCurrencyRate, VDC.ToCurrencyConversion AS HardCurrencyConversion, VDC.DrAmount AS HardCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS HC ON HC.VoucherDetailId=VD.Id
                                     WHERE I.Archive=0 AND I.IsWrittenOff=0 AND ID.IsWrittenOff=0 AND I.IsPark=0 AND ID.IsBlock=0 AND (I.SourceType='" + SourceType.CustomerInvoice + "' OR I.SourceType='" + SourceType.SalesInvoice + @"')
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "' AND I.PlantId='" + plantId + "'";
                return _sqlRepository.GetDataCollection(CmdText);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public List<Dictionary<string, object>> GetCustomerAllInvoiceList(string companyGroupId, string companyId, string plantId, string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var CmdText = @" SELECT TOP 700 * from (SELECT I.CompanyId, I.PlantId, I.PartyPlantId, I.PartyType, I.Id AS InvoiceId, ID.Id AS InvoiceDetailId, I.VoucherId, V.VoucherNo, VD.EntityId, EN.UserName AS EntityName, I.SalesTypeId, I.InvoiceNo, I.PartyId, VD.Id AS VoucherDetailId, I.CurrencyId
                                    , C.Code AS CurrencyCode, ID.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, ID.BudgetMasterId, B.Code AS BudgetCode, V.ExchangeType, 0 ExchangeAmount
                                    , B.UserName AS BudgetName, ID.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, Replace(CONVERT(VARCHAR(11), I.DocDate, 106), ' ', '-') AS DocDate, Replace(CONVERT(VARCHAR(11)
                                    , I.PostingDate, 106), ' ', '-') AS PostingDate, I.DocRefNo, I.Narration, ISNULL(ID.NetAmount,0) AS Receivable, (ISNULL(ID.WrittenOffAmount,0)) AS Received
                                    , PP.UserName AS PartyPlantName,SalesNo=case when S.Id<>'' then S.Id when ivs.Id<>'' then ivs.Id else I.DocRefNo end
                                    , (ISNULL(ID.NetAmount,0)- (ISNULL(ID.WrittenOffAmount,0))) AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, 0 ToCurrencyRate
                                    , CC.CompanyCurrencyConversion, GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion, HC.HardCurrencyId, HC.HardFromCurrencyId
                                     , HC.HardCurrencyRate, HC.HardCurrencyConversion, V.TransactionRefNo, I.SalesOrderNo,ISNULL(SM.TrnQty,0) TrnQty
                                    FROM [TRN].[InvoiceDetail] AS ID
                                    LEFT JOIN [TRN].[Invoice] AS I ON I.Id=ID.InvoiceId
									LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                    LEFT JOIN [TRN].[AdjustmentNoteDetail] AS AJD ON AJD.InvoiceDetailId=ID.Id
                                    LEFT JOIN [TRN].[AdjustmentNote] AS AJ ON AJ.Id=AJD.AdjustmentNoteId
                                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=ID.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [TRN].[Sales] AS S ON S.VoucherId=V.Id
                                    LEFT JOIN [TRN].[InventorySales] AS IVS ON IVS.VoucherId=V.Id
									LEFT JOIN (SELECT SalesId,SUM(TransactionQty) TrnQty FROM TRN.SalesMaterial group By SalesId) SM ON SM.SalesId=S.Id
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=ID.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=ID.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=ID.ActivityId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                    LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=I.EntityId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
									LEFT JOIN (
									SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,0 ToCurrencyRate,
										VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.DrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS GC ON GC.VoucherDetailId=VD.Id
									LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS HardCurrencyId, VDC.FromCurrencyId AS HardFromCurrencyId, VDC.ToCurrencyId,0 ToCurrencyRate,
										VDC.ToCurrencyRate AS HardCurrencyRate, VDC.ToCurrencyConversion AS HardCurrencyConversion, VDC.DrAmount AS HardCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS HC ON HC.VoucherDetailId=VD.Id
                                     WHERE I.Archive=0 AND I.IsPark=0 AND ID.IsBlock=0 AND (I.SourceType='" + SourceType.CustomerInvoice + "' OR I.SourceType='" + SourceType.SalesInvoice + @"')
                                    AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "' AND I.PlantId='" + plantId + @"'
                                    ) AS TEMP WHERE " + strkey + " order by PostingDate DESC ";
                return _sqlRepository.GetDataCollection(CmdText);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetInvoiceSalesAvailable(string voucherId)
        {
            try
            {
                var sql = @"SELECT VD.GLGeneralInfoId, GL.AccountCode+'-'+GL.UserName AS GLGeneralInfoName, VD.BudgetMasterId, BU.UserName AS BudgetName
                            , VD.ActivityId, ACT.UserName AS ActivityName, VDC.CrAmount AS Amount, VDC.CrAmount AS InvoiceAmount, VD.Id,ACT.IsOrderSpecific
                            FROM TRN.VoucherDetail AS VD
                            LEFT JOIN TRN.VoucherDetailCurrency AS VDC ON VD.Id=VDC.VoucherDetailId
                            LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN MST.BudgetMaster AS BM ON BM.Id=VD.BudgetMasterId
                            LEFT JOIN HKP.Budget AS BU ON BU.Id=BM.BudgetId
                            LEFT JOIN HKP.Activity AS ACT ON ACT.Id=VD.ActivityId
                            WHERE VD.VoucherId='" + voucherId + "' AND VD.TrnNature='" + TransactionNature.Sales + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetInvoicePurchasesAvailable(string voucherId)
        {
            try
            {
                var sql = @"SELECT VD.GLGeneralInfoId, GL.AccountCode+'-'+GL.UserName AS GLGeneralInfoName, VD.BudgetMasterId, BU.UserName AS BudgetName
                            , VD.ActivityId, ACT.UserName AS ActivityName, VDC.DrAmount AS Amount, VDC.DrAmount AS InvoiceAmount, VD.Id
                            FROM TRN.VoucherDetail AS VD
                            LEFT JOIN TRN.VoucherDetailCurrency AS VDC ON VD.Id=VDC.VoucherDetailId
                            LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN MST.BudgetMaster AS BM ON BM.Id=VD.BudgetMasterId
                            LEFT JOIN HKP.Budget AS BU ON BU.Id=BM.BudgetId
                            LEFT JOIN HKP.Activity AS ACT ON ACT.Id=VD.ActivityId
                            WHERE VD.VoucherId='" + voucherId + "' AND VD.TrnNature='" + TransactionNature.Purchases + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetInvoiceTaxAvailable(string invoiceId)
        {
            try
            {
                var sql = @"SELECT T.Id AS InvoiceTaxId,TD.Id AS InvoiceTaxDetailId,T.TaxCategoryId,TC.UserName AS TaxCategoryName,T.TaxAmount,T.WrittenOffAmount,TD.AType,
                            TD.Amount,TD.GLGeneralInfoId,TD.BudgetMasterId
                            FROM TRN.InvoiceTax AS T
                            LEFT JOIN TRN.InvoiceTaxDetail AS TD ON TD.InvoiceTaxId=T.Id
                            LEFT JOIN MST.TaxCategory AS TC ON TC.Id=T.TaxCategoryId
                            WHERE T.InvoiceId='" + invoiceId + "' AND T.SourceType IN ('" + SourceType.CustomerInvoiceTax + "' ,'" + SourceType.VendorInvoiceTax + "','" + SourceType.VendorInvoice + "','" + SourceType.CustomerInvoice + "') ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetSaleTypeGLBudget(string saleTypeId)
        {
            try
            {
                var sql = @"SELECT ST.Id,STG.SalesTypeGLId,STG.BudgetMasterId,STG.ActivityId
                    ,GLGI.AccountCode+' - '+ GLGI.UserName AS GLGeneralInfoName,B.Code +' - '+B.UserName AS BudgetName,A.Code +' - '+A.UserName AS ActivityName
                    FROM HKP.SalesType AS ST
                    LEFT JOIN HKP.SalesTypeGL AS STG ON STG.SalesTypeId=ST.Id
                    LEFT JOIN HKP.GLGeneralInfo AS GLGI ON GLGI.Id=STG.SalesTypeGLId
                    LEFT JOIN MST.BudgetMaster AS BM ON BM.Id=STG.BudgetMasterId
                    LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
                    LEFT JOIN HKP.Activity AS A ON A.Id=STG.ActivityId
                    WHERE ST.Id='" + saleTypeId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetVoucherGLBudget(string voucherId)
        {
            try
            {
                var sql = @"SELECT VD.Id,VD.BudgetMasterId,VD.ActivityId
                            ,GLGI.AccountCode+' - '+ GLGI.UserName AS GLGeneralInfoName,B.Code +' - '+B.UserName AS BudgetName,A.Code +' - '+A.UserName AS ActivityName
                            FROM  TRN.VoucherDetail AS VD
                            LEFT JOIN HKP.GLGeneralInfo AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN MST.BudgetMaster AS BM ON BM.Id=VD.BudgetMasterId
                            LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
                            LEFT JOIN HKP.Activity AS A ON A.Id=VD.ActivityId
                            WHERE VoucherId='" + voucherId + @"' AND CrAmount<>0 ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public List<Dictionary<string, object>> GetInvoiceSetOffDetailByInvoice(string companyGroupId, string companyId, string plantId, SourceType sourceType, string invoiceId)
        {
            var sql = @"SELECT AW.InvoiceWriteOffNo,AW.[SourceType], VD.VoucherId, V.VoucherNo, AW.Id, P.Code AS PartyCode, P.UserName AS PartyName, AW.PostingDate, AW.DocDate, AW.DocRefNo, C.Code AS CurrencyCode, SUM(IWD.Amount) AS Amount
                                    , AW.PartyPlantId, PP.UserName AS PartyPlantName, AW.IsPark, AW.BankJournalId,IWD.MultiplePaymentNo
                                    FROM [TRN].[InvoiceWriteOff] AS AW
									LEFT JOIN (SELECT WD.Id,IV.Id InvoiceId,WD.InvoiceWriteOffId,MPD.MultiplePaymentId MultiplePaymentNo,SUM(WD.Amount) Amount 
											FROM [TRN].[InvoiceWriteOffDetail] WD 
											LEFT JOIN TRN.Invoice IV ON WD.InvoiceId=IV.Id
											LEFT JOIN TRN.MultiplePaymentDetail MPD ON MPD.InvoiceId=IV.Id
											Group BY WD.Id,WD.InvoiceWriteOffId,IV.Id ,MPD.MultiplePaymentId,IV.Id) AS IWD ON IWD.InvoiceWriteOffId=AW.Id
									LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceWriteOffDetailId=IWD.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=AW.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AW.PartyPlantId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=AW.CurrencyId
                                    WHERE AW.Archive=0 AND AW.CompanyGroupId='" + companyGroupId + "' AND AW.CompanyId='" + companyId + @"' 
                                    AND AW.PlantId='" + plantId + "'  AND IWD.InvoiceId='" + invoiceId + @"'
                                    Group BY AW.InvoiceWriteOffNo, VD.VoucherId, V.VoucherNo, AW.Id, P.Code , P.UserName, AW.PostingDate
									, AW.DocDate, AW.DocRefNo, C.Code, AW.PartyPlantId, PP.UserName, AW.IsPark, AW.BankJournalId, IWD.MultiplePaymentNo,AW.[SourceType]";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetInvoiceSetOffDetailByInvoiceId(string companyGroupId, string companyId, string plantId, string invoiceId)
        {
            var sql = @"SELECT AW.InvoiceWriteOffNo,AW.[SourceType], VD.VoucherId, V.VoucherNo, AW.Id, P.Code AS PartyCode, P.UserName AS PartyName, REPLACE(CONVERT(VARCHAR(11), AW.PostingDate, 106), ' ', '-') PostingDate, REPLACE(CONVERT(VARCHAR(11), AW.DocDate, 106), ' ', '-') DocDate, AW.DocRefNo, C.Code AS CurrencyCode, SUM(IWD.Amount*CC.CompanyCurrencyRate) AS Amount
                                    , AW.PartyPlantId, PP.UserName AS PartyPlantName, AW.IsPark, AW.BankJournalId,IWD.MultiplePaymentNo
                                    FROM [TRN].[InvoiceWriteOff] AS AW
									LEFT JOIN (SELECT WD.Id,IV.Id InvoiceId,WD.InvoiceWriteOffId,MPD.MultiplePaymentId MultiplePaymentNo,SUM(WD.Amount) Amount 
											FROM [TRN].[InvoiceWriteOffDetail] WD 
											LEFT JOIN TRN.Invoice IV ON WD.InvoiceId=IV.Id
											LEFT JOIN TRN.MultiplePaymentDetail MPD ON MPD.InvoiceId=IV.Id
											Group BY WD.Id,WD.InvoiceWriteOffId,IV.Id ,MPD.MultiplePaymentId,IV.Id) AS IWD ON IWD.InvoiceWriteOffId=AW.Id
									LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceWriteOffDetailId=IWD.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=AW.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AW.PartyPlantId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=AW.CurrencyId
									LEFT JOIN (
									SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' 
									) AS CC ON CC.VoucherDetailId=VD.Id
                                    WHERE AW.Archive=0 AND AW.CompanyGroupId='" + companyGroupId + "' AND AW.CompanyId='" + companyId + @"' 
                                    AND AW.PlantId='" + plantId + "'  AND IWD.InvoiceId='" + invoiceId + @"'
                                    Group BY AW.InvoiceWriteOffNo, VD.VoucherId, V.VoucherNo, AW.Id, P.Code , P.UserName, AW.PostingDate
									, AW.DocDate, AW.DocRefNo, C.Code, AW.PartyPlantId, PP.UserName, AW.IsPark, AW.BankJournalId, IWD.MultiplePaymentNo,AW.[SourceType]";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetFiscalInvoiceTotalAmountByParty(string plantId, string partyId, DateTime postingDate)
        {
            var sql = @"SELECT V.VoucherNo,format(V.PostingDate,'dd-MMM-yyyy') PostingDate,V.DocRefNo,P.UserName PartyName,PP.UserName PartyPlantName
						,C.Code CurrencyCode,IV.Amount,IV.WrittenOffAmount,IV.Amount-IV.WrittenOffAmount Balance
						,IV.Amount*ISNULL(IV.CompanyCurrencyRate,0) BooksInvoiceAmount 
						FROM trn.Invoice IV
						LEFT JOIN SCS.FiscalYear FYP ON FYP.Id=IV.FiscalYearId
						LEFT JOIN TRN.Voucher V ON V.Id=IV.VoucherId
						LEFT JOIN HKP.Party P ON P.Id=IV.PartyId
						LEFT JOIN HKP.PartyPlant PP ON PP.Id=IV.PartyPlantId
						LEFT JOIN SCS.Currency C ON C.Id=IV.CurrencyId
						WHERE IV.PlantId='" + plantId + "' and IV.PartyId='" + partyId + "' AND FYP.StartDate <= '" + postingDate.ToDbDate() + "' AND FYP.EndDate >= '" + postingDate.ToDbDate() + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public GridModel InvoiceQuery(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            try
            {
                parameters.CmdText = @"SELECT  V.VoucherNo, P.Code AS PartyCode, P.UserName AS PartyName,  PP.UserName AS PartyPlantName, EI.EmployeeCode, EI.EmployeeName
                                        , C.Code AS CurrencyCode, P.Code+' - '+ PP.UserName Particulars,I.Id,I.Amount,I.WrittenOffAmount,I.VoucherId,I.SourceType,I.IsPark
                                        ,'Vendor' BeneficiaryType,I.PostingDate,I.DocDate,I.DocRefNo,V.VoucherDate,V.CurrencyId, ADT.TaxAmount AdditionalTax, ADT.VoucherId AdditionalTaxVoucherId, ADT.Id AdditionalTaxId
                                        ,IsTDSTaxPost=CASE WHEN ADT.VoucherId<>'' THEN 'TDSPosted' WHEN  ADT.InvoiceId IS NULL THEN '' ELSE 'TDSParked' end,V.VoucherTypeId,I.CompanyCurrencyRate
                                        ,I.PartyId,I.PartyPlantId,Null EmployeeId
                                        ,AV.VoucherNo TDSVoucherNo,ADT.VoucherId TDSVoucherId,[Status]= case when I.IsPark=1 then 'Parked' else 'Posted' end
                                        ,OI.Id OtherInvoiceId
                                        ,OtherIsPark=CASE WHEN OI.VoucherId<>'' THEN 'OtherInvoicePosted' WHEN  OI.VoucherId IS NULL THEN '' ELSE 'OtherInvoiceParked' end
                                        ,OI.VoucherId OtherInvoiceVoucherId
										,case when v.ApprovedBystatus is null then 'To Be Checked' else  v.ApprovedBystatus end CheckStatus
                                        ,IsExpenseDistribution=CASE WHEN ISNULL((select COUNT(ID.Id) from TRN.InvoiceDetailCharges ID
										INNER JOIN TRN.VoucherDetail VD ON VD.Id=ID.VoucherDetailId
										WHERE VD.VoucherId=I.VoucherId),0)>0 THEN 1 ELSE 0 END,V.ApprovedByStatus,EIA.EmployeeName ApprovedBy,V.ApprovedById,V.ApprovedDate,V.Narration
                                        FROM TRN.[Invoice] AS I
                                        JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=I.EmployeeId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=I.VoucherId
                                        LEFT JOIN TRN.AdditionalTax ADT ON ADT.InvoiceId=I.Id
                                        LEFT JOIN TRN.Voucher AV ON AV.Id=ADT.VoucherId
                                        LEFT JOIN DBO.EmployeeInformation EIA ON EIA.SystemId=V.ApprovedById
                                        LEFT JOIN TRN.OtherInvoice OI ON OI.InvoiceId=I.Id
                                        WHERE I.Archive=0 AND V.Archive=0 AND I.OpeningBalanceId IS NULL AND I.SourceType='" + sourceType + @"' 
                                        AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "' AND I.PlantId='" + plantId + @"' 
                                        UNION ALL
										SELECT  V.VoucherNo, P.Code AS PartyCode, P.UserName AS PartyName,  PP.UserName AS PartyPlantName, EI.EmployeeCode, EI.EmployeeName
                                        , C.Code AS CurrencyCode, EI.EmployeeName Particulars,I.Id,I.Amount,I.WrittenOffAmount,I.VoucherId,I.SourceType,I.IsPark
                                        ,'Employee' BeneficiaryType,I.PostingDate,I.DocDate,I.DocRefNo,V.VoucherDate,V.CurrencyId, ADT.TaxAmount AdditionalTax, ADT.VoucherId AdditionalTaxVoucherId, ADT.Id AdditionalTaxId
                                        ,IsAdditionalTaxPost=CASE WHEN ADT.VoucherId<>'' THEN 'Posted' WHEN  ADT.EmployeePayableId IS NULL THEN '' ELSE 'Parked' end,V.VoucherTypeId,I.CompanyCurrencyRate
                                        ,I.PartyId,I.PartyPlantId,I.EmployeeId
                                        ,case when AV.ApprovedBystatus is null then 'To Be Checked' else  AV.ApprovedBystatus end CheckStatus
                                        ,AV.VoucherNo TDSVoucherNo,ADT.VoucherId TDSVoucherId,[Status]= case when I.IsPark=1 then 'Parked' else 'Posted' end
                                        ,NULL OtherInvoiceId,NULL OtherIsPark,NULL OtherInvoiceVoucherId,0 IsExpenseDistribution,V.ApprovedByStatus,EIA.EmployeeName ApprovedBy,V.ApprovedById,V.ApprovedDate,V.Narration
                                        FROM TRN.[EmployeePayable] AS I
                                       LEFT JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=I.EmployeeId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=I.VoucherId
                                        LEFT JOIN TRN.AdditionalTax ADT ON ADT.EmployeePayableId=I.Id
                                        LEFT JOIN TRN.Voucher AV ON AV.Id=ADT.VoucherId
                                        LEFT JOIN DBO.EmployeeInformation EIA ON EIA.SystemId=V.ApprovedById
                                        WHERE I.Archive=0 AND V.Archive=0 AND I.OpeningBalanceId IS NULL AND I.SourceType='" + sourceType + @"' 
                                        AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "' AND I.PlantId='" + plantId + @"'  ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public GridModel GetIncentiveReceivableList(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            try
            {
                parameters.CmdText = @"SELECT  V.VoucherNo, P.Code AS PartyCode, P.UserName AS PartyName,  PP.UserName AS PartyPlantName, EI.EmployeeCode, EI.EmployeeName
                                        , C.Code AS CurrencyCode, P.Code+' - '+ PP.UserName Particulars,I.Id,I.Amount,I.WrittenOffAmount,I.VoucherId,I.SourceType,I.IsPark
                                        ,'Vendor' BeneficiaryType,I.PostingDate,I.DocDate,I.DocRefNo,V.VoucherDate,V.CurrencyId, ADT.TaxAmount AdditionalTax, ADT.VoucherId AdditionalTaxVoucherId, ADT.Id AdditionalTaxId
                                        ,IsTDSTaxPost=CASE WHEN ADT.VoucherId<>'' THEN 'TDSPosted' WHEN  ADT.InvoiceId IS NULL THEN '' ELSE 'TDSParked' end,V.VoucherTypeId,I.CompanyCurrencyRate
                                        ,I.PartyId,I.PartyPlantId,Null EmployeeId
                                        ,AV.VoucherNo TDSVoucherNo,ADT.VoucherId TDSVoucherId,[Status]= case when I.IsPark=1 then 'Parked' else 'Posted' end
                                        ,OI.Id OtherInvoiceId
                                        ,case when v.ApprovedBystatus is null then 'To Be Checked' else  v.ApprovedBystatus end CheckStatus
                                        ,OtherIsPark=CASE WHEN OI.VoucherId<>'' THEN 'OtherInvoicePosted' WHEN  OI.VoucherId IS NULL THEN '' ELSE 'OtherInvoiceParked' end
                                        ,OI.VoucherId OtherInvoiceVoucherId
                                        ,IsExpenseDistribution=CASE WHEN ISNULL((select COUNT(ID.Id) from TRN.InvoiceDetailCharges ID
										INNER JOIN TRN.VoucherDetail VD ON VD.Id=ID.VoucherDetailId
										WHERE VD.VoucherId=I.VoucherId),0)>0 THEN 1 ELSE 0 END
                                        FROM TRN.[Invoice] AS I
                                        JOIN [HKP].[Party] AS P ON P.Id=I.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=I.PartyPlantId
                                        LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=I.EmployeeId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=I.CurrencyId
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=I.VoucherId
                                        LEFT JOIN TRN.AdditionalTax ADT ON ADT.InvoiceId=I.Id
                                        LEFT JOIN TRN.Voucher AV ON AV.Id=ADT.VoucherId
                                        LEFT JOIN TRN.OtherInvoice OI ON OI.InvoiceId=I.Id
                                        WHERE I.Archive=0 AND V.Archive=0 AND I.OpeningBalanceId IS NULL AND I.SourceType='" + sourceType + @"' 
                                        AND I.CompanyGroupId='" + companyGroupId + "' AND I.CompanyId='" + companyId + "' AND I.PlantId='" + plantId + @"'  ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetInvoiceGLBudgetActivityDetail(string voucherId)
        {
            try
            {
                var sql = @"SELECT GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName,B.Code +' - '+B.UserName AS BudgetName,A.Code +' - '+A.UserName AS ActivityName
										,ISNULL(VD.DrAmount,0) AS TotalAmount,'Dr' TrnType ,VD.Id AS VoucherDetailId, VD.*,NULL InvoiceTaxViewModel 
										,(VD.DrAmount- ISNULL(ITD.Amount,0)) Amount, ISNULL(ITD.Amount,0) TotalTax
                                        FROM TRN.VoucherDetail VD
										LEFT JOIN HKP.GLGeneralInfo AS GLGI ON GLGI.Id=VD.GLGeneralInfoId
										LEFT JOIN MST.BudgetMaster AS BM ON BM.Id=VD.BudgetMasterId
										LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
										LEFT JOIN HKP.Activity AS A ON A.Id=VD.ActivityId
										left join (select SUM(TD.TaxAmount) Amount,vvd.Id AS VoucherDetailId from  TRN.InvoiceTax AS TD
										left join TRN.VoucherDetail AS vvd on vvd.Id=Td.VoucherDetailId where vvd.VoucherId='" + voucherId + @"'  group by vvd.Id) AS ITD on ITD.VoucherDetailId=vd.Id 
										WHERE VoucherId='" + voucherId + @"' AND InvoiceDetailId IS NULL AND InvoiceTaxDetailId IS NULL  AND CashMasterId IS NULL  ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetInvoiceTaxDetail(string invoiceId)
        {
            try
            {
                var sql = @"SELECT IT.VoucherDetailId,IT.VoucherId,IT.InvoiceId,IT.TaxCodeId,IT.TaxCategoryId,TC.Code,TC.UserName,ITD.Amount AS TaxAmount
										,ITD.*,1 ManuallyEditable ,TC.IsCreditable, TC.IsMerge, TC.IsWithhold
                                        FROM TRN.InvoiceTaxDetail AS ITD 
										LEFT JOIN TRN.InvoiceTax AS IT ON IT.Id=ITD.InvoiceTaxId
										LEFT JOIN MST.TaxCode AS TC ON TC.Id=IT.TaxCodeId
										WHERE IT.InvoiceId='" + invoiceId + "' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetMultipleVendorAvailableInvoiceList(GridParameter parameters, string companyGroupId, string companyId, string plantId, string maturatedate, string docType, string entityId, string partyId)
        {
            try
            {
                string temp = null;
                if (docType == "DueUpToDate")
                {
                    temp = " AND IV.ActualDueDate <='" + maturatedate + @"'";
                }
                if (docType == "DocDate")
                {
                    temp = " AND IV.DocDate <='" + maturatedate + @"'";
                }

                parameters.CmdText = @"SELECT 0 Active,IVD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode+' - '+GLGI.UserName AS GLGeneralInfoName, P.Code PartyCode, P.UserName As PartyName,PP.UserName PartyPlantName,  IVD.BudgetMasterId, B.UserName AS BudgetName, IVD.ActivityId, A.UserName AS ActivityName,
                                        V.VoucherNo, Replace(CONVERT(VARCHAR(11), IV.DocDate, 106), ' ', '-') DocDate ,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate, IV.DocRefNo, IV.Narration, IV.Id AS InvoiceId,VD.EntityId,E.UserName EntityName
                                        ,VD.PlantId, IVD.Id AS InvoiceDetailId, IV.VoucherId,
                                        VD.Id AS VoucherDetailId, IV.CurrencyId, C.Code AS CurrencyCode, IV.PartyId,IV.PartyPlantId, IVD.NetAmount AS Receivable,
                                        IVD.WrittenOffAmount AS Received, IVD.NetAmount-IVD.WrittenOffAmount AS Balance,
										CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion,
										GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion
										,Replace(CONVERT(VARCHAR(11), IV.BaseOnDueDate, 106), ' ', '-') BaseOnDueDate, NULL Amount
                                        FROM [TRN].[InvoiceDetail] AS IVD
                                        LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId=IV.Id
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IVD.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=IVD.GLGeneralInfoId
										LEFT JOIN [HKP].[Budget] AS B ON B.Id=IVD.BudgetMasterId
										LEFT JOIN [HKP].[Activity] AS A ON A.Id=IVD.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                                        LEFT JOIN [ORG].Entity E ON E.Id=IV.EntityId
                                        LEFT JOIN TRN.MultiplePaymentDetail  MPD ON MPD.InvoiceId=IV.Id  AND MPD.IsPark=1
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
									
                                        WHERE IV.Archive=0 AND IV.IsWrittenOff=0 AND IVD.IsWrittenOff=0 AND IVD.IsBlock=0 AND IV.SourceType in ('VendorInvoice','InventoryPayable')
                                        AND IV.CompanyGroupId='" + companyGroupId + "' AND IV.CompanyId='" + companyId + "' AND IV.PlantId='" + plantId + @"'  AND (IV.EntityId='" + entityId + @"' OR IV.EntityId IS NULL) and P.Id " + partyId + @"
                                        AND MPD.InvoiceId IS NULL" + temp;

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetMultipleVendorListQuery(string companyGroupId, string companyId, string plantId, string column, string value, GridParameter parameters, string maturatedate, string docType, string entityId)
        {
            try
            {
                string tem = null;

                if (docType == "DueUpToDate")
                {
                    tem = " AND IV.ActualDueDate <='" + maturatedate + @"'";
                }
                if (docType == "DocDate")
                {
                    tem = " AND IV.DocDate <='" + maturatedate + @"'";
                }

                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"SELECT * FROM (SELECT  count(IVD.Id) NoOfPendingInvoice,SUM(IVD.Amount-IVD.WrittenOffAmount) Balance, P.Code,P.Id PartyId,P.UserName,P.PartyNature,PC.UserName PartyCategory,PSC.UserName PartySubCategory
										,ppc.PartyAccountGroupCode,ppc.PartyAccountGroupName,ppc.CurrencyId,ppc.CurrencyCode
										,ppc.CurrencyName,CO.Code AS CountryCode,CO.UserName AS CountryName,S.Code AS StateCode,S.UserName AS StateName
										,CASE WHEN (SELECT COUNT(Id) FROM HKP.CompanyParty WHERE PartyId=P.Id AND PartyType='Customer')>0 THEN 'Yes' ELSE 'No' END IsCustomer
										,CASE WHEN (SELECT COUNT(Id) FROM HKP.CompanyParty WHERE PartyId=P.Id AND PartyType='Vendor')>0 THEN 'Yes' ELSE 'No' END IsVendor
										,Advance=SUM(Ad.Amount-Ad.WrittenOffAmount),PendingReceivable=ISNULL(SUM(RI.Amount-RI.WrittenOffAmount),0)
                                        FROM [TRN].[InvoiceDetail] AS IVD
                                        LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId=IV.Id 
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
										LEFT JOIN(select distinct CP.PartyId,PAG.Code AS PartyAccountGroupCode,PAG.UserName AS PartyAccountGroupName ,CP.CurrencyId
										,C.Code AS CurrencyCode,C.[Name] AS CurrencyName,CP.PartyType
										from [HKP].[CompanyParty] AS CP 
										LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
										LEFT JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId) ppc on ppc.PartyId=P.Id
										LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=P.AddressMasterId
										LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
										LEFT JOIN [SCS].[State] AS S ON S.Id=AM.StateId
										LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
										LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId 
										Left Join trn.Advance AD ON AD.PartyId=IV.PartyId AND AD.PartyType='Vendor' AND AD.Amount-AD.WrittenOffAmount>0
                                        Left Join trn.Invoice RI ON RI.PartyId=IV.PartyId AND RI.PartyType='Customer' AND RI.Amount-RI.WrittenOffAmount>0
                                    WHERE IV.Archive=0 AND IV.IsWrittenOff=0 AND IVD.IsWrittenOff=0 AND IVD.IsBlock=0 AND IV.SourceType in ('VendorInvoice','InventoryPayable') AND IV.CompanyGroupId='" + companyGroupId + "' AND IV.CompanyId='" + companyId + "' AND IV.PlantId='" + plantId + @"'  AND (IV.EntityId='" + entityId + @"' OR IV.EntityId IS NULL)  AND ppc.PartyType IN ('Vendor')
                                    GROUP BY P.Code,P.Id,P.UserName,P.PartyNature,PC.UserName ,PSC.UserName 
										,ppc.PartyAccountGroupCode,ppc.PartyAccountGroupName,ppc.CurrencyId,ppc.CurrencyCode
										,ppc.CurrencyName,CO.Code,CO.UserName,S.Code,S.UserName
                    ) AS TEMP WHERE " + strkey + "";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
        public List<Dictionary<string, object>> GetMultipleVendorAvailableInvoiceListNew(string companyGroupId, string companyId, string plantId, string maturatedate)
        {
            try
            {
                var sql = @"SELECT 0 Active,IVD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode+' - '+GLGI.UserName AS GLGeneralInfoName, P.Code PartyCode, P.UserName As PartyName,PP.UserName PartyPlantName,  IVD.BudgetMasterId, B.UserName AS BudgetName, IVD.ActivityId, A.UserName AS ActivityName,
                                        V.VoucherNo, Replace(CONVERT(VARCHAR(11), IV.DocDate, 106), ' ', '-') DocDate ,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate, IV.DocRefNo, IV.Narration, IV.Id AS InvoiceId,VD.EntityId,E.UserName EntityName
                                        ,VD.PlantId, IVD.Id AS InvoiceDetailId, IV.VoucherId,
                                        VD.Id AS VoucherDetailId, IV.CurrencyId, C.Code AS CurrencyCode, IV.PartyId,IV.PartyPlantId, IVD.NetAmount AS Receivable,
                                        IVD.WrittenOffAmount AS Received, IVD.NetAmount-IVD.WrittenOffAmount AS Balance,
										CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion,
										GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion
										,Replace(CONVERT(VARCHAR(11), IV.BaseOnDueDate, 106), ' ', '-') BaseOnDueDate, NULL Amount
                                        FROM [TRN].[InvoiceDetail] AS IVD
                                        LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId=IV.Id
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IVD.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=IVD.GLGeneralInfoId
										LEFT JOIN [HKP].[Budget] AS B ON B.Id=IVD.BudgetMasterId
										LEFT JOIN [HKP].[Activity] AS A ON A.Id=IVD.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                                        LEFT JOIN [ORG].Entity E ON E.Id=IV.EntityId
                                        LEFT JOIN TRN.MultiplePaymentDetail  MPD ON MPD.InvoiceId=IV.Id  AND MPD.IsPark=1
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
									
                                        WHERE IV.Archive=0 AND IV.IsWrittenOff=0 AND IVD.IsWrittenOff=0 AND IVD.IsBlock=0 AND IV.SourceType in ('VendorInvoice','InventoryPayable')
                                        AND IV.CompanyGroupId='" + companyGroupId + "' AND IV.CompanyId='" + companyId + "' AND IV.PlantId='" + plantId + @"' 
                                    AND IV.ActualDueDate <='" + maturatedate + @"' AND MPD.InvoiceId IS NULL";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public List<Dictionary<string, object>> GetMultipleEmployeeListQuery(string companyGroupId, string companyId, string plantId, string column, string value, GridParameter parameters)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"SELECT * FROM (SELECT X.*,ISNULL(Y.AdvanceNetBalance,0)AdvanceNetBalance FROM 
 (SELECT EP.EmployeeId,EMP.EmployeeCode,EMP.EmployeeName,LDEG.UserName LegalDesignation, DEPT.UserName Department,S.UserName Section
										,SS.UserName SubSection,L.UserName Line
										, SUM(EPD.NetAmount) AS Receivable
                                        , SUM(EPD.WrittenOffAmount) AS Received, SUM(EPD.NetAmount-EPD.WrittenOffAmount) AS Balance
                                        FROM [TRN].[EmployeePayableDetail] AS EPD
                                        LEFT JOIN [TRN].[EmployeePayable] AS EP ON EPD.EmployeePayableId=EP.Id
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.EmployeePayableDetailId=EPD.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
										LEFT JOIN EmployeeInformation AS EMP ON EP.EmployeeId=EMP.SystemId
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=EMP.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        WHERE EP.Archive=0 AND EP.IsPark=0 AND EP.IsWrittenOff=0 AND EPD.IsWrittenOff=0 AND EPD.IsBlock=0 AND EP.SourceType IN ('EmployeePayable','SalaryPayable','VendorInvoice','InventoryPayable')
                                        AND EP.CompanyGroupId='" + companyGroupId + @"' AND EP.CompanyId='" + companyId + @"' AND EP.PlantId='" + plantId + @"' AND EP.EmployeeId IS NOT NULL AND (EPD.NetAmount-EPD.WrittenOffAmount)>0 
										GROUP BY EMP.EmployeeName,EP.EmployeeId,EMP.EmployeeCode,DEPT.UserName,S.UserName,SS.UserName,LDEG.UserName, L.UserName 
										)X 
	LEFT JOIN (SELECT T.EmployeeId, T.EmployeeCode, T.EmployeeName ,SUM(Receivable)Receivable,SUM(Received)Received,SUM(Balance)AdvanceNetBalance
				FROM (SELECT  AM.EmployeeId, EI.EmployeeCode, EI.EmployeeName
								, AD.Amount AS Receivable, AD.WrittenOffAmount+ISNULL(SAVW.SalaryWrittenOffAmount,0) AS Received
                                , AD.Amount-AD.WrittenOffAmount-ISNULL(SAVW.SalaryWrittenOffAmount,0)AS Balance
                                FROM [TRN].[AdvanceDetail] AS AD
                                LEFT JOIN [TRN].[Advance] AS AM ON AD.AdvanceId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=AM.EmployeeId
								LEFT JOIN (SELECT SUM(ARS.InstallmentAmount)-ADV.WrittenOffAmount SalaryWrittenOffAmount,ADV.Id AdvanceId
                                    FROM  [TRN].EmployeeAdvanceDeduction EAD 
                                    LEFT JOIN dbo.AdvanceReqSchedule  ARS ON EAD.AdvanceReqScheduleId=ARS.Id
                                    INNER JOIN [TRN].EmployeeSalaryAdvance ESA ON ESA.Id=ARS.EmployeeSalaryAdvanceId
                                    INNER JOIN [TRN].[Advance] ADV ON ADV.VoucherId=ESA.VoucherId
                                    LEFT JOIN DBO.SalaryLock SL ON SL.YearNo=ARS.YearNo AND SL.MonthNo=ARS.MonthNo AND SL.EmpSystemId=ESA.EmployeeId AND SL.PayableVoucherId IS NULL
									GROUP BY ADV.Id,ADV.WrittenOffAmount) SAVW ON SAVW.AdvanceId=AD.AdvanceId
                                WHERE AM.Archive=0 AND AM.IsPosted=1 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 AND AM.SourceType in ('EmployeeAdvance','InterTransaction','FixedAssetDisposeJournal')
                                AND AM.CompanyGroupId='" + companyGroupId + @"' AND AM.CompanyId='" + companyId + @"' AND AM.PlantId='" + plantId + @"' AND AM.EmployeeId<>'' 
                                
                                UNION ALL
								SELECT  VD.EmployeeId, EI.EmployeeCode, EI.EmployeeName
								, AD.AdvanceAmount AS Receivable, ISNULL(AD.WrittenOffAmount,0) AS Received
                                , AD.AdvanceAmount-ISNULL(AD.WrittenOffAmount,0)AS Balance
                                FROM [TRN].[EmployeeAdvanceDetail] AS AD
                                LEFT JOIN [TRN].[EmployeeAdvance] AS AM ON AD.EmployeeAdvanceId=AM.Id
                                LEFT JOIN [TRN].[EmployeeAdvanceRequisition] AS EAR ON EAR.SystemId=AM.RequisitionId
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=AD.VoucherDetailId
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=AD.EmpSystemId
                                WHERE  AD.IsWrittenOff=0 AND (AD.AdvanceAmount-ISNULL(AD.WrittenOffAmount,0))>0 AND EAR.AdvanceType='General'
                                AND AM.CompanyGroupId='" + companyGroupId + @"' AND AM.CompanyId='" + companyId + @"' AND AM.PlantId='" + plantId + @"' AND ISNULL(AD.EmpSystemId,'')<>'' 
                                ) AS T WHERE T.Balance>0 GROUP BY T.EmployeeId, T.EmployeeCode, T.EmployeeName )Y ON Y.EmployeeId=X.EmployeeId
                                
                    ) AS TEMP WHERE " + strkey + "  ORDER BY EmployeeName ASC ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public GridModel GetMultipleEmployeeAvailableInvoiceList(GridParameter parameters, string companyGroupId, string companyId, string plantId, string employeeId)
        {
            try
            {
                parameters.CmdText = @"SELECT 0 Active,EMP.EmployeeCode,EMP.EmployeeName,EPD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode+' - '+GLGI.UserName AS GLGeneralInfoName, EPD.BudgetMasterId, B.UserName AS BudgetName, EPD.ActivityId, A.UserName AS ActivityName,
                                        V.VoucherNo, Replace(CONVERT(VARCHAR(11), EP.DocDate, 106), ' ', '-') DocDate,Replace(CONVERT(VARCHAR(11), EP.PostingDate, 106), ' ', '-') PostingDate,
                                        EP.DocRefNo, EP.Narration, EP.Id AS EmployeePayableId, EPD.Id AS EmployeePayableDetailId, EP.VoucherId, VD.EntityId, E.UserName AS EntityName, VD.PlantId,
                                        VD.Id AS VoucherDetailId, EP.CurrencyId, C.Code AS CurrencyCode, EP.EmployeeId, EPD.NetAmount AS Receivable,
                                        EPD.WrittenOffAmount AS Received, EPD.NetAmount-EPD.WrittenOffAmount AS Balance,EP.InventoryReceiveId GRNNo, ET.AdvanceType JournalType
                                        ,Particular=REPLACE(REPLACE(
										STUFF((SELECT DISTINCT ','+xpo.UserName from
											hkp.Activity xpo
											INNER JOin TRN.VoucherDetail xPDAMAP on xpo.id=xPDAMAP.ActivityId
											WHERE VD.ActivityId!=xPDAMAP.ActivityId and xPDAMAP.VoucherId=V.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
                                        FROM [TRN].[EmployeePayableDetail] AS EPD
                                        LEFT JOIN [TRN].[EmployeePayable] AS EP ON EPD.EmployeePayableId=EP.Id
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.EmployeePayableDetailId=EPD.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN EmployeeInformation AS EMP ON EP.EmployeeId=EMP.SystemId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=EPD.GLGeneralInfoId
										LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=EPD.BudgetMasterId
										LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
										LEFT JOIN [HKP].[Activity] AS A ON A.Id=EPD.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=EP.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS E ON E.Id=VD.EntityId
                                        LEFT JOIN HKP.EmployeeTransactionType ET ON ET.Id=EP.EmployeeTransactionTypeId
                                        WHERE EP.Archive=0 AND EP.IsPark=0 AND EP.IsWrittenOff=0 AND EPD.IsWrittenOff=0 AND EPD.IsBlock=0 AND EP.SourceType IN ('EmployeePayable','SalaryPayable','VendorInvoice','InventoryPayable')
                                        AND EP.CompanyGroupId='" + companyGroupId + @"' AND EP.CompanyId='" + companyId + @"' AND EP.PlantId='" + plantId + @"' AND EP.EmployeeId " + employeeId + @" AND (EPD.NetAmount-EPD.WrittenOffAmount)>0 ";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetMultiplePaymentParkList(string plantId)
        {
            try
            {
                var sql = @"SELECT MP.Id,MP.CompanyGroupId,MP.CompanyId,MP.PlantId,MP.SourceType,Replace(CONVERT(VARCHAR(11), MP.DueUpToDate, 106), ' ', '-') DueUpToDate
                            ,Replace(CONVERT(VARCHAR(11), MP.TentativeDate, 106), ' ', '-') TentativeDate
                            ,MP.BankMasterId,MP.IsFifo,MP.IsPark ,BM.AccountTitle, 0 flag 
							, MPD.Amount 
						    FROM   TRN.MultiplePayment MP 
							JOIN (SELECT SUM(MPD.Amount) Amount,MPD.MultiplePaymentId FROM TRN.MultiplePaymentDetail MPD 
									WHERE ISNULL(MPD.Id,NULL) NOT IN (SELECT MultiplePaymentDetailId FROM TRN.InvoiceWriteOffDetail 
									WHERE MultiplePaymentDetailId<>'' )
									GROUP BY MPD.MultiplePaymentId
								)MPD ON MP.Id=MPD.MultiplePaymentId
							LEFT JOIN MST.BankMaster BM ON BM.Id=MP.BankMasterId
							WHERE  MP.PlantId='" + plantId + @"' 
							AND MP.ApprovalStatus='Approved'";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

       

        public List<Dictionary<string, object>> GetMultiplePaymentData(string plantId)
        {
            try
            {
                var sql = @"SELECT MP.Id,MP.CompanyGroupId,MP.CompanyId,MP.PlantId,MP.SourceType,Replace(CONVERT(VARCHAR(11), MP.DueUpToDate, 106), ' ', '-') DueUpToDate
                            ,Replace(CONVERT(VARCHAR(11), MP.TentativeDate, 106), ' ', '-') TentativeDate
                            ,MP.BankMasterId,MP.IsFifo,MPD.IsPark ,BM.AccountTitle, 0 flag 
                            ,SUM(MPD.Amount) Amount
							,ParkStatus=case when MPD.IsPark=1 then 'Parked' else 'Posted' end
                            FROM TRN.MultiplePaymentDetail MPD 
							LEFT JOIN TRN.MultiplePayment MP ON MP.Id=MPD.MultiplePaymentId
							LEFT JOIN HKP.Party P ON P.Id=MPD.PartyId
							LEFT JOIN MST.BankMaster BM ON BM.Id=MP.BankMasterId
							where  MP.PlantId='"+ plantId + @"' 
							group by MP.Id,MP.CompanyGroupId,MP.CompanyId,MP.PlantId,MP.SourceType,MP.DueUpToDate
                            , MP.TentativeDate,MPD.MultiplePaymentId
                            ,MP.BankMasterId,MP.IsFifo,MPD.IsPark ,BM.AccountTitle ";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetMultipleVendorApprovalList(GridParameter parameters, string companyGroupId, string companyId, string multiplePaymentId)
        {
            try
            {
                parameters.CmdText = @"SELECT MP.CompanyId, MP.CompanyGroupId, MPD.Id AS MultiplePaymentDetailId,MPD.*,IVD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode+' - '+GLGI.UserName AS GLGeneralInfoName, PT.Code+' - '+PT.UserName As PartyName,  IVD.BudgetId, B.UserName AS BudgetName, IVD.ActivityId, A.UserName AS ActivityName,
                                                Replace(CONVERT(VARCHAR(11), IV.DocDate, 106), ' ', '-') DocDate ,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate, IV.DocRefNo,  VD.EntityId,VD.PlantId,
                                               IV.CurrencyId, C.Code AS CurrencyCode, IV.PartyId,
                	CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion,
                	GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion,
                	HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion
                                               FROM  TRN.MultiplePaymentDetail AS MPD
                	JOIN TRN.MultiplePayment AS MP ON MP.Id=MPD.MultiplePaymentId
                	LEFT JOIN TRN.Invoice AS IV ON IV.Id=MPD.InvoiceId
                	LEFT JOIN TRN.InvoiceDetail AS IVD ON IVD.Id=MPD.InvoiceDetailId
                                               LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IVD.Id
                                               LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                               LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=IVD.GLGeneralInfoId
                	LEFT JOIN [HKP].[Budget] AS B ON B.Id=IVD.BudgetId
                	LEFT JOIN [HKP].[Activity] AS A ON A.Id=IVD.ActivityId
                                               LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                                               LEFT JOIN [HKP].[Party] AS PT ON PT.Id=IV.PartyId
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
                                            WHERE MP.ApprovalStatus='" + ApprovalStatus.Pending + @"'
                                            AND MP.CompanyGroupId='" + companyGroupId + @"' AND MP.CompanyId='" + companyId + @"' AND MPD.MultiplePaymentId='" + multiplePaymentId + @"'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetMultiplePaymentPendingList(GridParameter parameters, string companyGroupId, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT MP.CompanyId, MP.CompanyGroupId, MPD.Id AS MultiplePaymentDetailId,MPD.*,0 Active ,IVD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode+' - '+GLGI.UserName AS GLGeneralInfoName, PT.Code+' - '+PT.UserName As PartyName,  IVD.BudgetId, B.UserName AS BudgetName, IVD.ActivityId, A.UserName AS ActivityName,
                                        V.VoucherNo, Replace(CONVERT(VARCHAR(11), IV.DocDate, 106), ' ', '-') DocDate ,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate, IV.DocRefNo, IV.Narration, VD.EntityId,VD.PlantId,  IV.VoucherId,
                                        VD.Id AS VoucherDetailId, IV.CurrencyId, C.Code AS CurrencyCode, IV.PartyId, IVD.NetAmount AS Payable,
                                        IVD.WrittenOffAmount AS PaymentMade, IVD.NetAmount-IVD.WrittenOffAmount AS Balance,
										CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion,
										GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion,
										HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion
                                               FROM  TRN.MultiplePaymentDetail AS MPD
                	JOIN TRN.MultiplePayment AS MP ON MP.Id=MPD.MultiplePaymentId
                	LEFT JOIN TRN.Invoice AS IV ON IV.Id=MPD.InvoiceId
                	LEFT JOIN TRN.InvoiceDetail AS IVD ON IVD.Id=MPD.InvoiceDetailId
                                               LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IVD.Id
                                               LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                               LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=IVD.GLGeneralInfoId
                	LEFT JOIN [HKP].[Budget] AS B ON B.Id=IVD.BudgetId
                	LEFT JOIN [HKP].[Activity] AS A ON A.Id=IVD.ActivityId
                                               LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                                               LEFT JOIN [HKP].[Party] AS PT ON PT.Id=IV.PartyId
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
                                            WHERE MP.ApprovalStatus='" + ApprovalStatus.Pending + @"'
                                            AND MP.CompanyGroupId='" + companyGroupId + @"' AND MP.CompanyId='" + companyId + @"'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetMultipleVendorAvailableDetailList(string companyId, string plantId, string multiplePaymentId)
        {
            try
            {
                var sql = @"SELECT 0 Active,IVD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode+' - '+GLGI.UserName AS GLGeneralInfoName, P.Code+' - '+P.UserName As PartyName,  IVD.BudgetMasterId, B.UserName AS BudgetName, IVD.ActivityId, A.UserName AS ActivityName,
                                        V.VoucherNo, Replace(CONVERT(VARCHAR(11), IV.DocDate, 106), ' ', '-') DocDate ,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate
                                        , IV.DocRefNo, IV.Narration, IV.Id AS InvoiceId,VD.EntityId,VD.PlantId, IVD.Id AS InvoiceDetailId, IV.VoucherId,
                                        VD.Id AS VoucherDetailId, IV.CurrencyId, C.Code AS CurrencyCode, MPD.PartyId,MPD.PartyPlantId, IVD.NetAmount AS Payable,
                                        IVD.WrittenOffAmount AS PaymentMade, IVD.NetAmount-IVD.WrittenOffAmount AS Balance,MPD.Amount ,MPD.MultiplePaymentId,MPD.Id,
										CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion
                                        ,P.UserName PartyName,PP.UserName PartyPlantName,NULL ExchangeType,0 ExchangeAmount,0 BaseDrAmount
                                        FROM TRN.MultiplePaymentDetail AS MPD
										JOIN TRN.MultiplePayment AS MP ON MP.Id=MPD.MultiplePaymentId
										LEFT JOIN [TRN].[InvoiceDetail] AS IVD ON IVD.Id=MPD.InvoiceDetailId
                                        LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId=IV.Id
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IVD.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=IVD.GLGeneralInfoId
										LEFT JOIN [HKP].[Budget] AS B ON B.Id=IVD.BudgetMasterId
										LEFT JOIN [HKP].[Activity] AS A ON A.Id=IVD.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                                        LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=MPD.PartyPlantId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
                                     WHERE   MP.PlantId='" + plantId + "' AND MPD.MultiplePaymentId='" + multiplePaymentId + @"' 
                                    AND MPD.Id NOT IN (select MultiplePaymentDetailId from TRN.InvoiceWriteOffDetail where MultiplePaymentDetailId<>'')";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public List<Dictionary<string, object>> GetVendorAvailableInvoiceList(string companyGroupId, string companyId)
        {
            try
            {
                var sql = @" SELECT 0 Active, IVD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, IVD.BudgetMasterId, B.UserName AS BudgetName, IVD.ActivityId, EN.UserName AS EntityName, A.UserName AS ActivityName,
                                        V.VoucherNo, Replace(CONVERT(VARCHAR(11), IV.DocDate, 106), ' ', '-') DocDate ,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate, IV.DocRefNo, IV.Narration, IV.Id AS InvoiceId,VD.EntityId,VD.PlantId, IVD.Id AS InvoiceDetailId, IV.VoucherId,
                                        VD.Id AS VoucherDetailId, IV.CurrencyId, C.Code AS CurrencyCode, IV.PartyId, IVD.NetAmount AS Receivable,V.ExchangeType, 0 ExchangeAmount,
                                        IVD.WrittenOffAmount AS Received, IVD.NetAmount-IVD.WrittenOffAmount AS Balance, IV.PartyPlantId, PP.UserName AS PartyPlantName,
										CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion,
										GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion
										,PDA.AcceptanceNo,PLC.ContractId,PDA.PurchaseLCId
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

                                        LEFT JOIN [TRN].[PurchaseDocAcceptance] AS PDA ON PDA.Id=IV.PurchaseDocAcceptanceId
                                        LEFT JOIN [dbo].[PurchaseLC] AS PLC ON PLC.Id=PDA.PurchaseLCId
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
                                        WHERE IV.Archive=0 AND IV.IsWrittenOff=0 AND IVD.IsWrittenOff=0  AND V.IsPark=0 AND IVD.IsBlock=0 AND IV.SourceType in ('" + SourceType.VendorInvoice + "','" + SourceType.InventoryPayable + "','" + SourceType.PurchaseDocAcceptance + "','" + SourceType.EmployeePayable + @"')
                                        AND IV.CompanyGroupId='" + companyGroupId + @"' AND IV.CompanyId='" + companyId + @"'";
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public List<Dictionary<string, object>> GetVendorAllInvoiceList(string companyGroupId, string companyId, string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"SELECT TOP 700 * from ( SELECT 0 Active, IVD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, IVD.BudgetMasterId, B.UserName AS BudgetName, IVD.ActivityId, EN.UserName AS EntityName, A.UserName AS ActivityName,
                                        V.VoucherNo, Replace(CONVERT(VARCHAR(11), IV.DocDate, 106), ' ', '-') DocDate ,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate, IV.DocRefNo, IV.Narration, IV.Id AS InvoiceId,VD.EntityId,VD.PlantId, IVD.Id AS InvoiceDetailId, IV.VoucherId,
                                        VD.Id AS VoucherDetailId, IV.CurrencyId, C.Code AS CurrencyCode, IV.PartyId, IVD.NetAmount AS Receivable,V.ExchangeType, 0 ExchangeAmount,
                                        IVD.WrittenOffAmount AS Received, IVD.NetAmount-IVD.WrittenOffAmount AS Balance, IV.PartyPlantId, PP.UserName AS PartyPlantName,
										CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion,
										GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion
										,PDA.AcceptanceNo,PLC.ContractId,PDA.PurchaseLCId,ISNULL(IRD.TrnQty,0) TrnQty,'' AdjustmentNoteId
                                        FROM [TRN].[InvoiceDetail] AS IVD
                                        LEFT JOIN [TRN].[Invoice] AS IV ON IVD.InvoiceId=IV.Id
									    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
                                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IVD.Id
                                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                        LEFT JOIN [TRN].[InventoryReceive] AS IR ON IR.Id=iv.InventoryReceiveId
										LEFT JOIN (SELECT InventoryReceiveId,SUM(TransactionQty) TrnQty FROM TRN.InventoryReceiveDetail group By InventoryReceiveId) IRD ON IRD.InventoryReceiveId=IR.Id
                                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=IVD.GLGeneralInfoId
										LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=IVD.BudgetMasterId
										LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
										LEFT JOIN [HKP].[Activity] AS A ON A.Id=IVD.ActivityId
                                        LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                                        LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId

                                        LEFT JOIN [TRN].[PurchaseDocAcceptance] AS PDA ON PDA.Id=IV.PurchaseDocAcceptanceId
                                        LEFT JOIN [dbo].[PurchaseLC] AS PLC ON PLC.Id=PDA.PurchaseLCId
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
                                        WHERE IV.Archive=0 AND V.IsPark=0 AND IVD.IsBlock=0 AND IV.SourceType in ('" + SourceType.VendorInvoice + "','" + SourceType.InventoryPayable + "','" + SourceType.PurchaseDocAcceptance + "','" + SourceType.EmployeePayable + @"')
                                        AND IV.CompanyGroupId='" + companyGroupId + @"' AND IV.CompanyId='" + companyId + @"'

                                UNION ALL
								SELECT 0 Active, IVD.GLGeneralInfoId AS GLGeneralInfoId, '' AS GLGeneralInfoCode, '' AS GLGeneralInfoName, IVD.BudgetMasterId, '' AS BudgetName, IVD.ActivityId, '' AS EntityName, '' AS ActivityName,
                                V.VoucherNo, Replace(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') DocDate ,Replace(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') PostingDate, V.DocRefNo, V.Narration, IVD.AdjustmentNoteId AS InvoiceId,V.EntityId,V.PlantId, '' AS InvoiceDetailId, II.VoucherId,
                                '' AS VoucherDetailId, V.CurrencyId, C.Code AS CurrencyCode, AN.PartyId, IVD.Amount AS Receivable,V.ExchangeType, 0 ExchangeAmount,
                                IVD.WrittenOffAmount AS Received, IVD.Amount-IVD.WrittenOffAmount AS Balance, AN.PartyPlantId, PP.UserName AS PartyPlantName,
								CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion,
								NULL CompanyGroupCurrencyId, NULL CompanyGroupFromCurrencyId, NULL CompanyGroupCurrencyRate, NULL CompanyGroupCurrencyConversion
								,'' AcceptanceNo,'' ContractId,'' PurchaseLCId,ISNULL(IID.TransactionQty,0) TrnQty,IVD.AdjustmentNoteId
                                FROM [TRN].[SalesReturn] AS II
                                JOIN (SELECT SUM(TransactionQty) TransactionQty,SalesReturnId FROM TRN.SalesReturnDetail GROUP BY SalesReturnId) AS IID ON IID.SalesReturnId=II.Id
								JOIN TRN.Sales S ON S.Id=II.SalesId
								JOIN TRN.AdjustmentNote AN ON AN.SalesReturnId=II.Id
								LEFT JOIN TRN.AdjustmentNoteDetail IVD ON IVD.AdjustmentNoteId=AN.Id
								LEFT JOIN TRN.AdditionalTax ADN ON ADN.AdjustmentNoteId=AN.Id
								LEFT JOIN TRN.Voucher V ON V.Id=II.VoucherId
								LEFT JOIN TRN.Voucher VADN ON VADN.Id=ADN.VoucherId
								LEFT JOIN HKP.Party P ON P.Id=S.PartyId
								LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AN.PartyPlantId
								LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdjustmentNoteDetailId=IVD.Id
								LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
								WHERE II.VoucherId<>'' AND V.IsPark=0 AND AN.CompanyGroupId='" + companyGroupId + @"' AND AN.CompanyId='" + companyId + @"'
                                    ) AS TEMP WHERE " + strkey + " order by PostingDate DESC ";
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetVendorAvailableInvoiceList(GridParameter parameters, string companyGroupId, string companyId, string partyId)
        {
            try
            {
                parameters.CmdText = @" SELECT IVD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, IVD.BudgetMasterId, B.UserName AS BudgetName, IVD.ActivityId, EN.UserName AS EntityName, A.UserName AS ActivityName,
                                        V.VoucherNo, Replace(CONVERT(VARCHAR(11), IV.DocDate, 106), ' ', '-') DocDate ,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate, IV.DocRefNo, IV.Narration, IV.Id AS InvoiceId,VD.EntityId,VD.PlantId, IVD.Id AS InvoiceDetailId, IV.VoucherId,
                                        VD.Id AS VoucherDetailId, IV.CurrencyId, C.Code AS CurrencyCode, IV.PartyId, IVD.Amount AS Receivable,V.ExchangeType, 0 ExchangeAmount,
                                        ISNULL(IWD.WrittenOffAmount,0) + ISNULL(ITWLC.LCTaggedAmount,0)  AS Received,IVD.AdditionalAmount ,ISNULL(ITWLC.LCTaggedAmount,0) LCTaggedAmount,((IVD.Amount+ISNULL(IVD.AdditionalAmount,0))- ISNULL(IWD.WrittenOffAmount,0) - ISNULL(ITWLC.LCTaggedAmount,0) ) AS Balance, IV.PartyPlantId, PP.UserName AS PartyPlantName,
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
										where XV.Id=V.Id   for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),IV.FileName
                                        FROM [TRN].[InvoiceDetail] AS IVD
                                        LEFT JOIN (SELECT SUM(Amount)WrittenOffAmount,InvoiceDetailId FROM trn.InvoiceWriteOffDetail   GROUP BY InvoiceDetailId) AS IWD ON IWD.InvoiceDetailId=IVD.Id
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
                                        LEFT JOIN (SELECT itwlc.InvoiceDetailId,SUM(itwlc.Amount) LCTaggedAmount FROM  InvoiceTaggingWithLCDetail itwlc 
										join InvoiceTaggingWithLCMaster lcm on lcm.Id=itwlc.InvoiceTaggingWithLCMasterId where lcm.VoucherId IS NULL group by InvoiceDetailId) ITWLC ON ITWLC.InvoiceDetailId=IVD.Id
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
                                        WHERE IV.Archive=0 AND IV.IsWrittenOff=0 AND IVD.IsWrittenOff=0  AND V.IsPark=0 AND IVD.IsBlock=0 
                                        AND IV.Id NOT IN (SELECT InvoiceId FROM InvoiceTaggingWithLCDetail)
                                        --AND ISNULL(IV.PurchaseLCId,'')=''
                                        AND IV.SourceType in ('" + SourceType.VendorInvoice + "','" + SourceType.PurchaseDocAcceptance + "','" + SourceType.SuspensePayable + "','" + SourceType.ServicePayable + "','" + SourceType.EmployeePayable + "','" + SourceType.PostInvoice + "','" + SourceType.InvoiceToAcceptance + @"')
                                        AND IV.CompanyGroupId='" + companyGroupId + @"' AND IV.CompanyId='" + companyId + @"' AND IV.PartyId='" + partyId + @"' 
                                    UNION ALL
                                    SELECT IVD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, IVD.BudgetMasterId, B.UserName AS BudgetName, IVD.ActivityId, EN.UserName AS EntityName, A.UserName AS ActivityName,
                                        V.VoucherNo, Replace(CONVERT(VARCHAR(11), IV.DocDate, 106), ' ', '-') DocDate ,Replace(CONVERT(VARCHAR(11), IV.PostingDate, 106), ' ', '-') PostingDate, IV.DocRefNo, IV.Narration, IV.Id AS InvoiceId,VD.EntityId,VD.PlantId, IVD.Id AS InvoiceDetailId, IV.VoucherId,
                                        VD.Id AS VoucherDetailId, IV.CurrencyId, C.Code AS CurrencyCode, IV.PartyId, IVD.NetAmount AS Receivable,V.ExchangeType, 0 ExchangeAmount,
                                        ISNULL(IWD.WrittenOffAmount,0) + ISNULL(ITWLC.LCTaggedAmount,0) AS Received,IVD.AdditionalAmount,ISNULL(ITWLC.LCTaggedAmount,0) LCTaggedAmount, ((IVD.NetAmount+ISNULL(IVD.AdditionalAmount,0))-ISNULL(IWD.WrittenOffAmount,0)-ISNULL(ITWLC.LCTaggedAmount,0)) AS Balance, IV.PartyPlantId, PP.UserName AS PartyPlantName,
										CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion,
										GC.CompanyGroupCurrencyId, GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion,
										HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion
                                        ,Particular=REPLACE(REPLACE(
										STUFF((SELECT DISTINCT ','+xpo.UserName from
											hkp.Activity xpo
											INNER JOin TRN.VoucherDetail xPDAMAP on xpo.id=xPDAMAP.ActivityId
											WHERE VD.ActivityId!=xPDAMAP.ActivityId and xPDAMAP.VoucherId=V.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
                                        ,NULL AcceptanceNo ,NULL LCRef,NULL ContractNo,NULL Customer, NULL MasterLCNo,IV.FileName

                                        FROM [TRN].[InvoiceDetail] AS IVD
                                        LEFT JOIN (SELECT SUM(Amount)WrittenOffAmount,InvoiceDetailId FROM trn.InvoiceWriteOffDetail   GROUP BY InvoiceDetailId) AS IWD ON IWD.InvoiceDetailId=IVD.Id
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
                                        LEFT JOIN (SELECT itwlc.InvoiceDetailId,SUM(itwlc.Amount) LCTaggedAmount FROM  InvoiceTaggingWithLCDetail itwlc 
										join InvoiceTaggingWithLCMaster lcm on lcm.Id=itwlc.InvoiceTaggingWithLCMasterId where lcm.VoucherId IS NULL group by InvoiceDetailId) ITWLC ON ITWLC.InvoiceDetailId=IVD.Id
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
                                        AND IV.CompanyGroupId='" + companyGroupId + @"' AND IV.CompanyId='" + companyId + @"' AND IV.PartyId='" + partyId + @"' AND IR.PurchaseDocumentAcceptanceId IS NULL";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public GridModel GetVendorAvailableInvoiceListForInvoiceToAcceptancePost(GridParameter parameters, string companyGroupId, string companyId, string partyId)
        {
            try
            {
                parameters.CmdText = @" SELECT IVD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, IVD.BudgetMasterId, B.UserName AS BudgetName, IVD.ActivityId, EN.UserName AS EntityName, A.UserName AS ActivityName,
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
                                        WHERE IV.Archive=0 AND IV.IsWrittenOff=0 AND IVD.IsWrittenOff=0  AND V.IsPark=0 AND IVD.IsBlock=0 
                                        AND IV.Id NOT IN (SELECT InvoiceId FROM InvoiceTaggingWithLCDetail)
                                        AND IV.SourceType in ('" + SourceType.VendorInvoice + "','" + SourceType.PurchaseDocAcceptance + "','" + SourceType.SuspensePayable + "','" + SourceType.ServicePayable + "','" + SourceType.EmployeePayable + @"')
                                        AND ISNULL(IV.PurchaseLCId,'')<>'' 
                                        AND IV.CompanyGroupId='" + companyGroupId + @"' AND IV.CompanyId='" + companyId + @"' AND IV.PartyId='" + partyId + @"' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public List<Dictionary<string, object>> GetInvoiceOvereheadList()
        {
            try
            {
                var sql = @"SELECT SMC.Id ChargesNo,P.UserName PartyName,C.Code CurrencyCode,SMD.TransactionAmount
                ,SMD.TotalTaxAmount,SMC.*
                FROM TRN.InvoiceServiceMasterCharges SMC 
                LEFT JOIN(select InvoiceServiceMasterChargesId,SUM(TransactionAmount) TransactionAmount,SUM(TotalTaxAmount) TotalTaxAmount 
							from  TRN.InvoiceServiceMasterChargesDetail group by InvoiceServiceMasterChargesId)
							 SMD ON SMD.InvoiceServiceMasterChargesId=SMC.Id
                LEFT JOIN HKP.Party P ON P.Id=SMC.PartyId
                LEFT JOIN SCS.Currency C ON C.Id=SMC.CurrencyId
                WHERE SMC.IsPark=1 ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public List<Dictionary<string, object>> GetInvoiceOvereheadPostingList()

        {
            try
            {
                var sql = @"SELECT SMC.Id ChargesNo,P.UserName PartyName,C.Code CurrencyCode,SUM(SMD.TransactionAmount) TransactionAmount
                ,SUM(SMD.TotalTaxAmount) TotalTaxAmount,SMC.Id,SMC.CurrencyId,SMC.CompanyCurrencyRate,SMC.PartyId,SMC.CompanyGroupId,SMC.CompanyId,SMC.PlantId,SMC.PartyPlantId
                 ,SMC.BaseNoOfDays,SMC.BaseOnDueDate,SMC.DeliveryPartyPlantId,SMC.IsNonCreditable,SMC.Narration,SMC.PartyType,SMC.PaymentTermId,SMC.DocDate,SMC.DocRefNo
                FROM TRN.InvoiceServiceMasterCharges SMC 
                LEFT JOIN TRN.InvoiceServiceMasterChargesDetail SMD ON SMD.InvoiceServiceMasterChargesId=SMC.Id
                LEFT JOIN HKP.Party P ON P.Id=SMC.PartyId
                LEFT JOIN SCS.Currency C ON C.Id=SMC.CurrencyId
                WHERE SMC.IsPark=1
                group by SMC.Id,P.UserName,C.Code,SMC.Id,SMC.CurrencyId,SMC.CompanyCurrencyRate,SMC.PartyId,SMC.CompanyGroupId,SMC.CompanyId,SMC.PlantId,SMC.PartyPlantId
                 ,SMC.BaseNoOfDays,SMC.BaseOnDueDate,SMC.DeliveryPartyPlantId,SMC.IsNonCreditable,SMC.Narration,SMC.PartyType,SMC.PaymentTermId,SMC.DocDate,SMC.DocRefNo ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetInvoiceOvereheadPostedList()
        {
            try
            {
                var sql = @"SELECT SMC.Id ChargesNo,IV.Id InvoiceNo,IV.VoucherId,V.VoucherNo,P.UserName PartyName,C.Code CurrencyCode,SUM(SMD.TransactionAmount) TransactionAmount
                ,SUM(SMD.TotalTaxAmount) TotalTaxAmount,SMC.Id,SMC.CurrencyId,SMC.CompanyCurrencyRate,SMC.PartyId,SMC.CompanyGroupId,SMC.CompanyId,SMC.PlantId,SMC.PartyPlantId
                 ,SMC.BaseNoOfDays,SMC.BaseOnDueDate,SMC.DeliveryPartyPlantId,SMC.IsNonCreditable,SMC.Narration,SMC.PartyType,SMC.PaymentTermId,SMC.DocDate,SMC.DocRefNo
                FROM TRN.InvoiceServiceMasterCharges SMC 
                LEFT JOIN TRN.InvoiceServiceMasterChargesDetail SMD ON SMD.InvoiceServiceMasterChargesId=SMC.Id
                LEFT JOIN HKP.Party P ON P.Id=SMC.PartyId
                LEFT JOIN SCS.Currency C ON C.Id=SMC.CurrencyId
				LEFT JOIN TRN.Invoice IV ON IV.InvoiceServiceMasterChargesId=SMC.Id
				LEFT JOIN TRN.Voucher V ON V.Id=IV.VoucherId
                WHERE SMC.IsPark=0 AND V.Archive=0
                group by SMC.Id,P.UserName,C.Code,IV.Id,IV.VoucherId,V.VoucherNo,SMC.Id,SMC.CurrencyId,SMC.CompanyCurrencyRate,SMC.PartyId,SMC.CompanyGroupId,SMC.CompanyId,SMC.PlantId,SMC.PartyPlantId
                 ,SMC.BaseNoOfDays,SMC.BaseOnDueDate,SMC.DeliveryPartyPlantId,SMC.IsNonCreditable,SMC.Narration,SMC.PartyType,SMC.PaymentTermId,SMC.DocDate,SMC.DocRefNo ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public GridModel SuspensesPayableQuery(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT AW.InvoiceGroupNo, P.Code AS PartyCode, P.UserName AS PartyName, AW.PostingDate, AW.DocDate, AW.DocRefNo, C.Code AS CurrencyCode,SUM(IWD.Amount) Amount
                                    , AW.PartyPlantId, PP.UserName AS PartyPlantName, AW.IsPark
                                    
                                    ,VoucherNo=STUFF((SELECT DISTINCT ','+xpo.VoucherNo from
                                    			[TRN].Voucher xpo
                                    			INNER JOin trn.[Invoice] xPDAMAP on xpo.Id=xPDAMAP.VoucherId
                                    			WHERE AW.InvoiceGroupNo=xPDAMAP.InvoiceGroupNo for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                    FROM [TRN].[Invoice] AS AW
                                    LEFT JOIN (
                                    SELECT InvoiceId,SUM(Amount) Amount FROM [TRN].[InvoiceDetail] Group BY InvoiceId
                                    ) AS IWD ON IWD.InvoiceId=AW.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=AW.VoucherId
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=AW.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AW.PartyPlantId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=AW.CurrencyId
                                    WHERE AW.Archive=0 AND V.Archive=0 AND AW.CompanyGroupId='" + companyGroupId + "' AND AW.CompanyId='" + companyId + "' AND AW.PlantId='" + plantId + @"' AND AW.[SourceType]='" + sourceType + @"'
                                    Group BY  P.Code , P.UserName, AW.PostingDate
                                    , AW.DocDate, AW.DocRefNo, C.Code, AW.PartyPlantId, PP.UserName, AW.IsPark,AW.InvoiceGroupNo";
            return _sqlRepository.GetGridData(parameters);
        }

        private Dictionary<string, object> GetInventoryReceive(string receivedId)
        {
            var cmdText = @"select IsNonCreditable,PartyId FROM TRN.[InventoryReceive] where Id = '" + receivedId.ToString() + "'";
            return _sqlRepository.GetData(cmdText);
        }
        private Dictionary<string, object> GetCompanyPartyGroup(string partyId, string plantId)
        {
            var cmdText = @"select PartyAccountGroupId FROM HKP.CompanyParty where PartyId = '" + partyId + "' AND PlantId='" + plantId + @"'";
            return _sqlRepository.GetData(cmdText);
        }
        public IEnumerable<object> GetInventoryPayableFOC(string companyId, string plantId, string inveReveiveId)
        {
            try
            {
                var inventoryReceiveData = GetInventoryReceive(inveReveiveId);
                var companyParty = GetCompanyPartyGroup(inventoryReceiveData["PartyId"].ToString(), plantId);
                if (Convert.ToBoolean(inventoryReceiveData["IsNonCreditable"].ToString()))
                {
                    var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						, T.Cr, T.Amount,T.InventoryReceiveDetailId,T.IsAsset 
					FROM (
						SELECT  'Material' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.IsAsset,MM.FixedAssetMasterId

							,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryGLId  ELSE FAG.AssetUnderConstructionGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryBudgetMasterId  ELSE FAG.AssetUnderConstructionBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryActivityId  ELSE FAG.AssetUnderConstructionActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END
							
							, 0 Dr, NULL Cr
							, 0 Amount
                            ,IRD.Id AS  InventoryReceiveDetailId
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
						LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAMG.AssetUnderConstructionGLId ,FAMG.AssetUnderConstructionBudgetMasterId,FAMG.AssetUnderConstructionActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT LEFT JOIN HKP.FixedAssetMasterGL FAMG ON FAMBT.FixedAssetMasterId=FAMG.FixedAssetMasterId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.AssetUnderConstructionGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.AssetUnderConstructionBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.AssetUnderConstructionActivityId= AF.Id
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,FAG.AssetUnderConstructionGLId,GLF.AccountCode,GLF.UserName
						,FAG.AssetUnderConstructionBudgetMasterId,BF.Code,BF.UserName
						,FAG.AssetUnderConstructionActivityId,AF.Code,AF.UserName,IRD.Id,MM.IsAsset
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.InventoryReceiveDetailId,T.IsAsset
                    UNION
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
						, T.Amount ,NULL InventoryReceiveDetailId,T.IsAsset
					FROM (
						SELECT 'Vendor' AS OtherName,'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
						, MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName
							, MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,NULL Dr
						,SUM(MAT.Cr)  AS Cr,
						SUM(MAT.Cr)  AS Amount ,MAT.IsAsset
						FROM (
							SELECT IR.Id, 'Vendor' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
                            ,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGPGL.GLGeneralInfoId  ELSE FAG.VendorReconGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGPGL.BudgetMasterId  ELSE FAG.VendorReconBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGPGL.ActivityId  ELSE FAG.VendorReconActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END
                            , NULL Dr, 0  Cr
							, 0 Amount,MM.IsAsset
						FROM [TRN].[InventoryReceiveDetail] AS IRD 
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
						
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Vendor')AS CP ON IR.PartyId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id

                        LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAVGL.VendorReconGLId ,FAVGL.VendorReconBudgetMasterId,FAVGL.VendorReconActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT 
						LEFT JOIN HKP.FixedAssetMasterVendorReconGL FAVGL ON 
						FAMBT.FixedAssetMasterId=FAVGL.FixedAssetMasterId  AND FAVGL.PartyAccountGroupId=@partyAccountGruopId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId

						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.VendorReconGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.VendorReconBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.VendorReconActivityId= AF.Id

						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY  IR.Id, MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName
						,MM.IsAsset,FAG.VendorReconGLId,GLF.AccountCode,GLF.UserName,FAG.VendorReconBudgetMasterId,BF.Code,BF.UserName,FAG.VendorReconActivityId,AF.Code,AF.UserName,MM.IsAsset
						) AS MAT
						LEFT OUTER JOIN (
						SELECT INS.InventoryReceiveId, sum(INS.Amount) AS Amount,sum(INS.TotalTaxAmount * IR.ToCurrencyRate) AS TotalTaxAmount
						from  [TRN].[InventoryService] AS INS
						LEFT JOIN TRN.InventoryReceive AS IR ON IR.Id=INS.InventoryReceiveId 
                        where InventoryReceiveId=@receiveId group by INS.InventoryReceiveId
						) AS SRV on SRV.InventoryReceiveId=MAT.Id
						GROUP BY  MAT.Id,MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName , MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,MAT.IsAsset
					) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr
					, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset
                UNION
				SELECT T.OtherName, T.TrnType, NULL MaterialGroupMasterId, NULL TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
						, T.Amount , NULL InventoryReceiveDetailId,0 IsAsset
					FROM (
						
							SELECT IR.Id, 'Acceptance' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
                            ,PDAD.GLGeneralInfoId ,GL.AccountCode GLGeneralInfoCode ,GL.UserName GLGeneralInfoName
							,PDAD.BudgetMasterId ,B.Code BudgetCode ,B.UserName BudgetName
							,PDAD.ActivityId ,A.Code ActivityCode ,A.UserName ActivityName
							, NULL Dr, 0   AS  Cr
							, 0  AS Amount
							,0 IsAsset
						FROM TRN.PurchaseDocAcceptance PDA
						LEFT JOIN TRN.PurchaseDocAcceptanceDetail PDAD ON PDAD.PurchaseDocAcceptanceId=PDA.Id
						LEFT JOIN TRN.InventoryReceive IR ON IR.PurchaseDocumentAcceptanceId=PDA.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON PDAD.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON PDAD.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON PDAD.ActivityId= A.Id
						WHERE IR.Id=@receiveId
						GROUP BY  IR.Id, PDAD.GLGeneralInfoId, GL.AccountCode, GL.UserName, PDAD.BudgetMasterId, B.Code, B.UserName, PDAD.ActivityId, A.Code, A.UserName
					) AS T
					GROUP BY  T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType--,T.TaxCategoryId, T.IsAsset
					ORDER BY T.TrnType DESC ";
                    return _sqlRepository.GetDataCollection(sql);
                }
                else
                {
                    var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						--, T.Dr + (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount FROM [TRN].[InventoryReceiveTax] AS IRTS 
						--JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
						--WHERE IRTS.InventoryReceiveId=@receiveId AND IR.IsNonCreditable=1 AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ()) AS Dr
						
						, T.Cr, T.Amount, T.IsAsset,T.InventoryReceiveDetailId
						--, T.Amount + (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount FROM [TRN].[InventoryReceiveTax] AS IRTS 
						--	JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
						--	WHERE IRTS.InventoryReceiveId=@receiveId AND IR.IsNonCreditable=1 AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ())AS Amount
					FROM (
						SELECT  'Material' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.FixedAssetMasterId

							,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryGLId  ELSE FAG.AssetUnderConstructionGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryBudgetMasterId  ELSE FAG.AssetUnderConstructionBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryActivityId  ELSE FAG.AssetUnderConstructionActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END
							
							, 0 Dr, NULL Cr
							, 0 Amount
                            ,MM.IsAsset,IRD.Id AS  InventoryReceiveDetailId
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
                        --LEFT JOIN (SELECT FixedAssetMasterId,AssetUnderConstructionGLId ,AssetUnderConstructionBudgetMasterId,AssetUnderConstructionActivityId
						 --FROM HKP.FixedAssetMasterGL) AS FAG ON FAG.FixedAssetMasterId=MM.FixedAssetMasterId
						LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAMG.AssetUnderConstructionGLId ,FAMG.AssetUnderConstructionBudgetMasterId,FAMG.AssetUnderConstructionActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT LEFT JOIN HKP.FixedAssetMasterGL FAMG ON FAMBT.FixedAssetMasterId=FAMG.FixedAssetMasterId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.AssetUnderConstructionGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.AssetUnderConstructionBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.AssetUnderConstructionActivityId= AF.Id
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,FAG.AssetUnderConstructionGLId,GLF.AccountCode,GLF.UserName
						,FAG.AssetUnderConstructionBudgetMasterId,BF.Code,BF.UserName
						,FAG.AssetUnderConstructionActivityId,AF.Code,AF.UserName,IRD.Id
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId
					
                    
                    UNION
					SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, 0  Dr, NULL Cr
						, 0 Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId
					FROM [TRN].[InventoryReceiveTax] AS IRT
					LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRT.InventoryReceiveDetailId=IRD.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
					LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
					LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN (SELECT C.AddressMasterId, MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
							AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE IRD.InventoryReceiveId=@receiveId AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM' AND IRT.InventoryReceiveDetailId<>'' AND IR.PurchaseDocumentAcceptanceId IS NULL
					GROUP BY  IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					UNION
					SELECT 'Svc' AS OtherName, 'Dr' AS TrnType, NULL AS MaterialGroupMasterId, IRTS.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, 0  Dr, NULL Cr
						, 0 Amount
                       ,0 IsAsset, NULL InventoryReceiveDetailId
					FROM [TRN].[InventoryReceiveTax] AS IRTS
					LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
					LEFT JOIN [TRN].[InventoryService] AS INS ON INS.Id=IRTS.InventoryServiceId
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRTS.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id AND IRTS.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE INS.InventoryReceiveId=@receiveId --AND IR.IsNonCreditable=0 
					AND IRTS.InventoryServiceId<>'' AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM' AND IR.PurchaseDocumentAcceptanceId IS NULL
					GROUP BY IRTS.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					UNION
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
                            --+ (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount FROM [TRN].[InventoryReceiveTax] AS IRTS JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
							--WHERE IRTS.InventoryReceiveId=@receiveId AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ()) AS Cr
						, T.Amount ,T.IsAsset, NULL InventoryReceiveDetailId
                            --+ (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount FROM [TRN].[InventoryReceiveTax] AS IRTS JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
							--WHERE IRTS.InventoryReceiveId=@receiveId AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ())AS Amount
					FROM (
						SELECT 'Vendor' AS OtherName,'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
						, MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName
							, MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,NULL Dr
						,SUM(MAT.Cr)  AS Cr,--+SUM(ISNULL(SRV.TotalTaxAmount,0))
						SUM(MAT.Cr)  AS Amount --+SUM(ISNULL(SRV.TotalTaxAmount,0))
                        ,0 IsAsset
						FROM (
							SELECT IR.Id, 'Vendor' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
							--, MGPGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
							--, MGPGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
							--, MGPGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName

                            ,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGPGL.GLGeneralInfoId  ELSE FAG.VendorReconGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGPGL.BudgetMasterId  ELSE FAG.VendorReconBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGPGL.ActivityId  ELSE FAG.VendorReconActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END

							, NULL Dr, 0  Cr
							, 0 AS Amount
						FROM [TRN].[InventoryReceiveDetail] AS IRD 
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						--JOIN [TRN].[InventoryService] AS INS ON IRD.InventoryReceiveId=INS.InventoryReceiveId
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
						
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Vendor')AS CP ON IR.PartyId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id

                        LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAVGL.VendorReconGLId ,FAVGL.VendorReconBudgetMasterId,FAVGL.VendorReconActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT 
						LEFT JOIN HKP.FixedAssetMasterVendorReconGL FAVGL ON 
						FAMBT.FixedAssetMasterId=FAVGL.FixedAssetMasterId  AND FAVGL.PartyAccountGroupId=@partyAccountGruopId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId

						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.VendorReconGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.VendorReconBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.VendorReconActivityId= AF.Id

						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY  IR.Id, MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName
						,MM.IsAsset,FAG.VendorReconGLId,GLF.AccountCode,GLF.UserName,FAG.VendorReconBudgetMasterId,BF.Code,BF.UserName,FAG.VendorReconActivityId,AF.Code,AF.UserName
						) AS MAT
						LEFT OUTER JOIN (
						SELECT INS.InventoryReceiveId, sum(INS.Amount) AS Amount,sum(INS.TotalTaxAmount) AS TotalTaxAmount
						from  [TRN].[InventoryService] AS INS
						LEFT JOIN TRN.InventoryReceive AS IR ON IR.Id=INS.InventoryReceiveId 
                        where InventoryReceiveId=@receiveId group by INS.InventoryReceiveId
						) AS SRV on SRV.InventoryReceiveId=MAT.Id
						GROUP BY  MAT.Id,MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName , MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName
					) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId, T.IsAsset
					UNION
                    SELECT T.OtherName, T.TrnType, NULL MaterialGroupMasterId, NULL TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
						, T.Amount ,0 IsAsset, NULL InventoryReceiveDetailId
					FROM (
						
							SELECT IR.Id, 'Acceptance' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
							
                            ,PDAD.GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,PDAD.BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,PDAD.ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName

							, NULL Dr, 0  Cr
							, 0 Amount
							,0 IsAsset
						FROM TRN.PurchaseDocAcceptance PDA
						LEFT JOIN TRN.PurchaseDocAcceptanceDetail PDAD ON PDAD.PurchaseDocAcceptanceId=PDA.Id
						LEFT JOIN TRN.InventoryReceive IR ON IR.PurchaseDocumentAcceptanceId=PDA.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON PDAD.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON PDAD.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON PDAD.ActivityId= A.Id
						WHERE IR.Id=@receiveId
						GROUP BY  IR.Id, PDAD.GLGeneralInfoId, GL.AccountCode, GL.UserName, PDAD.BudgetMasterId, B.Code, B.UserName, PDAD.ActivityId, A.Code, A.UserName
					) AS T
					GROUP BY  T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType--,T.TaxCategoryId, T.IsAsset
					ORDER BY T.TrnType DESC ";
                    return _sqlRepository.GetDataCollection(sql);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public GridModel CustomerInvoiceReceipt(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            string wc, wcc = string.Empty;
            if (parameters.searchBy == "Status" && parameters.search.ToUpper() == "POSTED")
            {
                wc = "(case when TAB.IsPark = 1 then 'Parked' else 'Posted' end)";
                wcc = "Posted";

                parameters.searchBy = wc;
                parameters.search = wcc;
            }
            else if (parameters.searchBy == "Status" && parameters.search.ToUpper() == "PARKED")
            {
                wc = "(case when TAB.IsPark = 1 then 'Parked' else 'Posted' end)";
                wcc = "Parked";

                parameters.searchBy = wc;
                parameters.search = wcc;
            }
            else
            {

            }

            parameters.CmdText = @"SELECT AW.InvoiceWriteOffNo, VD.VoucherId, V.VoucherNo, AW.Id, P.Code AS PartyCode, P.UserName AS PartyName, AW.PostingDate, AW.DocDate, AW.DocRefNo, C.Code AS CurrencyCode, SUM(IWD.Amount) AS Amount
                                    , AW.PartyPlantId, PP.UserName AS PartyPlantName, AW.BankJournalId,IWD.MultiplePaymentNo
                                   , Status = case when AW.IsPark = 0 then 'Posted' else 'Parked' end,AW.IsPark
                                    FROM [TRN].[InvoiceWriteOff] AS AW
									LEFT JOIN (SELECT WD.Id,WD.InvoiceWriteOffId,MPD.MultiplePaymentId MultiplePaymentNo,SUM(WD.Amount) Amount 
											FROM [TRN].[InvoiceWriteOffDetail] WD 
											LEFT JOIN TRN.Invoice IV ON WD.InvoiceId=IV.Id
											LEFT JOIN TRN.MultiplePaymentDetail MPD ON MPD.InvoiceId=IV.Id
											Group BY WD.Id,WD.InvoiceWriteOffId,IV.Id ,MPD.MultiplePaymentId) AS IWD ON IWD.InvoiceWriteOffId=AW.Id
									LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceWriteOffDetailId=IWD.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=AW.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AW.PartyPlantId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=AW.CurrencyId
                                    WHERE AW.Archive=0 AND V.Archive=0 AND AW.CompanyGroupId='" + companyGroupId + "' AND AW.CompanyId='" + companyId + "' AND AW.PlantId='" + plantId + "' AND AW.[SourceType]='" + sourceType + @"'
                                    Group BY AW.InvoiceWriteOffNo, VD.VoucherId, V.VoucherNo, AW.Id, P.Code , P.UserName, AW.PostingDate
									, AW.DocDate, AW.DocRefNo, C.Code, AW.PartyPlantId, PP.UserName, AW.IsPark, AW.BankJournalId, IWD.MultiplePaymentNo";
            return _sqlRepository.GetGridData(parameters);
        }

        public IEnumerable<object> GetOtherInvoiceJournal(string companyId, string plantId, string otherInvoieId)
        {
            try
            {


                var sql = @"DECLARE @otherInvoiceId varchar(10)='" + otherInvoieId + "', @companyId varchar(10)='" + companyId + "', @plantId varchar(30)='" + plantId + @"'
SELECT  P.UserName Customer, 'Dr' AS TrnType,OI.InvoiceId
							,OI.GLGeneralInfoId
							,RGL.ReconciliationGLCode GLGeneralInfoCode
							,RGL.ReconciliationGLName GLGeneralInfoName 
							,OI.BudgetMasterId BudgetMasterId
							,RGL.ReconciliationBudgetCode BudgetCode
							,RGL.ReconciliationBudgetName BudgetName
							,OI.ActivityId ActivityId
							,RGL.ReconciliationActivityCode ActivityCode
							,RGL.ReconciliationActivityName ActivityName
							, OI.Amount AS Dr, NULL  Cr
							, OI.Amount AS Amount
							, OI.Amount AS DrAmount
							, NULL CrAmount
                            , NULL InvoiceDetailId
                            ,OI.PartyId,OI.PartyPlantId
						FROM [TRN].[OtherInvoice] AS OI
						JOIN TRN.Invoice IV ON IV.Id=OI.InvoiceId
						LEFT JOIN HKP.Party P ON P.Id=OI.PartyId
						LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=OI.PartyId
						LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS ReconciliationGLId, GL.AccountCode AS ReconciliationGLCode, GL.UserName AS ReconciliationGLName
                                    , CPGL.BudgetMasterId AS ReconciliationBudgetId, B.Code AS ReconciliationBudgetCode, B.UserName AS ReconciliationBudgetName
                                    , CPGL.ActivityId AS ReconciliationActivityId, A.Code AS ReconciliationActivityCode, A.UserName AS ReconciliationActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='ReconciliationGL'
                                    ) AS RGL ON RGL.CompanyPartyId=CP.Id
						WHERE OI.Id=@otherInvoiceId AND IV.PlantId=@plantId

						union all
						SELECT  P.UserName Customer, 'Cr' AS TrnType,OI.InvoiceId
							,OI.GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName 
							,OI.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,OI.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName
							,  NULL Dr, OI.Amount Cr
							, OI.Amount AS Amount
							, NULL DrAmount
							, OI.Amount AS CrAmount
                            ,IVD.Id InvoiceDetailId
                            ,IV.PartyId,IV.PartyPlantId
						FROM [TRN].[OtherInvoice] AS OI
						JOIN TRN.Invoice IV ON IV.Id=OI.InvoiceId
						JOIN TRN.InvoiceDetail IVD ON IVD.InvoiceId=IV.Id
						LEFT JOIN HKP.Party P ON P.Id=IV.PartyId
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=IVD.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=IVD.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=IVD.ActivityId
						WHERE OI.Id=@otherInvoiceId AND IV.PlantId=@plantId
					--ORDER BY T.TrnType DESC 
";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetMasterOrderList(string companyId, string plantId)
        {
            try
            {
                var sql = @"SELECT A.Id AS  MasterOrderId, A.CompanyId, A.CommitmentId, A.PlantId, A.EntityId
                            , A.PartyId, P.UserName AS CustomerName, A.CurrencyId,CO.BaseCurrencyId, A.TotalQty	
                            , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                    , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress								    
							,A.TotalQtyUOMId,PL.UserName,A.IsReplacement,A.Type,C.Code Currency,0 Active
							,B.UserName Buyer,ISNULL(A.BuyerReferenceNo,'')BuyerReferenceNo,ISNULL(A.OwnReferenceNo,'')OwnReferenceNo
                            , CP.PaymentTermId, PT.Code AS PaymentTermCode, PT.UserName AS PaymentTermName  	
                            FROM [TRN].[MasterOrder] AS A
                            LEFT JOIN [ORG].[Company] AS CO ON CO.Id=A.CompanyId
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=A.PartyId  AND CP.PlantId=A.PlantId AND CP.PartyType='Customer'
                            LEFT JOIN [MST].[PaymentTerm] AS PT ON PT.Id=CP.PaymentTermId
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
                            LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
                            LEFT JOIN HKP.Buyer AS B ON B.Id=A.BuyerId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            WHERE A.CompanyId='" + companyId + "' AND A.PlantId='" + plantId + "' ORDER BY A.Id";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
        public IEnumerable<object> GetMasterOrderListByPartyId(string companyId, string plantId, string partyId)
        {
            try
            {
                var sql = @"SELECT A.Id AS  MasterOrderId, A.CompanyId, A.CommitmentId, A.PlantId, A.EntityId
                            , A.PartyId, P.UserName AS CustomerName, A.CurrencyId,CO.BaseCurrencyId, A.TotalQty	
                            , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                    , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress								    
							,A.TotalQtyUOMId,PL.UserName,A.IsReplacement,A.Type,C.Code Currency,0 Active
							,B.UserName Buyer,ISNULL(A.BuyerReferenceNo,'')BuyerReferenceNo,ISNULL(A.OwnReferenceNo,'')OwnReferenceNo
                            , CP.PaymentTermId, PT.Code AS PaymentTermCode, PT.UserName AS PaymentTermName  	
                            FROM [TRN].[MasterOrder] AS A
                            LEFT JOIN [ORG].[Company] AS CO ON CO.Id=A.CompanyId
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=A.PartyId  AND CP.PlantId=A.PlantId AND CP.PartyType='Customer'
                            LEFT JOIN [MST].[PaymentTerm] AS PT ON PT.Id=CP.PaymentTermId
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
                            LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
                            LEFT JOIN HKP.Buyer AS B ON B.Id=A.BuyerId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            WHERE A.CompanyId='" + companyId + "' AND A.PlantId='" + plantId + "' AND P.Id='" + partyId + @"' ORDER BY A.Id";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }


        public IWorkbook MultiVendorPaymentReportSheet(out string reportFileName, string mpdId)
        {
            var excelEngine = new ExcelEngine();
            var reportUtility = new ReportUtility();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Multi Vendor Payment Report";

            var dsLocal = MultiVendorPaymentDetailSQL(mpdId);
            var dsSummary = MultiVendorPaymentSummarySQL(mpdId);

            int row = 5;


            #region Header

            reportUtility.SetMasterHeaderText(ref sheet, 5, 1, "Entry Date");
            reportUtility.SetText(ref sheet, 5, 2, dsLocal.Rows[0]["EntryDate"].ToString());
            sheet.Range[5, 2, 5, 3].Merge();
            reportUtility.SetMasterHeaderText(ref sheet, 5, 5, "Doc Ref");
            reportUtility.SetText(ref sheet, 5, 6, dsLocal.Rows[0]["InvoiceNo"].ToString());
            sheet.Range[5, 6, 5, 7].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 6, 1, "Voucher Type");
            reportUtility.SetText(ref sheet, 6, 2, dsLocal.Rows[0]["VoucherType"].ToString());
            sheet.Range[6, 2, 6, 3].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 6, 5, "Payment Source");
            reportUtility.SetText(ref sheet, 6, 6, dsLocal.Rows[0]["PaymentSource"].ToString());
            sheet.Range[6, 6, 6, 7].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 7, 1, "Currency");
            sheet.Range[7, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            reportUtility.SetText(ref sheet, 7, 2, dsLocal.Rows[0]["Currency"].ToString());
            sheet.Range[7, 2, 7, 3].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 7, 5, "Narration");
            sheet.Range[7, 5].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            reportUtility.SetText(ref sheet, 7, 6, dsLocal.Rows[0]["Narration"].ToString());
            sheet.Range[7, 6, 7, 7].Merge();


            #endregion Header

            reportUtility.SetMasterHeaderText(ref sheet, 9, 1, "Summary");
            sheet.Range[9, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[9, 1].CellStyle.Font.Bold = true;
            sheet.Range[9, 1, 9, 1].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
            sheet.Range[9, 1, 9, 1].CellStyle.Font.Color = ExcelKnownColors.White;

            int ROW2 = 10; int COL2 = 1;

            #region columns
            sheet[ROW2, COL2].Text = "Payment No";
            sheet[ROW2, COL2].ColumnWidth = 16;
            int ColPaymentNo = COL2;
            COL2++;

            sheet[ROW2, COL2].Text = "Party";
            sheet[ROW2, COL2].ColumnWidth = 16;
            int ColPartyName = COL2;
            COL2++;

            sheet[ROW2, COL2].Text = "Bank";
            sheet[ROW2, COL2].ColumnWidth = 16;
            int ColBank = COL2;
            COL2++;

            sheet[ROW2, COL2].Text = "Due Up To Date";
            sheet[ROW2, COL2].ColumnWidth = 16;
            int ColDueUpToDate = COL2;
            COL2++;

            sheet[ROW2, COL2].Text = "Tentative Date";
            sheet[ROW2, COL2].ColumnWidth = 16;
            int ColTentativeDate = COL2;
            COL2++;

            sheet[ROW2, COL2].Text = "Amount";
            sheet[ROW2, COL2].ColumnWidth = 16;
            int ColAmount = COL2;
            COL2++;

            sheet[ROW2, COL2].Text = "Park Status";
            sheet[ROW2, COL2].ColumnWidth = 16;
            int ColParkStatus = COL2;
            COL2++;

            sheet[ROW2, COL2].Text = "PartyTaxNo";
            sheet[ROW2, COL2].ColumnWidth = 16;
            int ColPartyTaxNo = COL2;
            COL2++;

            sheet[ROW2, COL2].Text = "PDC";
            sheet[ROW2, COL2].ColumnWidth = 16;
            int ColPDC = COL2;
            COL2++;

            sheet[ROW2, COL2].Text = "Advance";
            sheet[ROW2, COL2].ColumnWidth = 16;
            int ColAdvance = COL2;

            #endregion columns

            int endCol2 = COL2;
            sheet.Range[ROW2, 1, ROW2, endCol2].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
            sheet.Range[ROW2, 1, ROW2, endCol2].CellStyle.Font.Color = ExcelKnownColors.White;
            sheet.Range[ROW2, 1, ROW2, endCol2].CellStyle.Font.Bold = true;
            sheet.Range[ROW2, 1, ROW2, endCol2].CellStyle.Font.Size = 9f;
            sheet.Range[ROW2, 1, ROW2, endCol2].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW2, 1, ROW2, endCol2].BorderAround(ExcelLineStyle.Hair);

            ROW2++;

            //int startRow = ROW;

            for (int i = 0; i < dsSummary.Rows.Count; i++)
            {
                sheet[ROW2, ColPaymentNo].Text = dsSummary.Rows[i]["Id"].ToString();
                sheet[ROW2, ColPartyName].Text = dsSummary.Rows[i]["PartyName"].ToString();
                sheet[ROW2, ColBank].Text = dsSummary.Rows[i]["AccountTitle"].ToString();
                sheet[ROW2, ColDueUpToDate].Text = dsSummary.Rows[i]["DueUpToDate"].ToString();
                sheet[ROW2, ColTentativeDate].Text = dsSummary.Rows[i]["TentativeDate"].ToString();
                sheet[ROW2, ColAmount].Number = clsStaticInfo.dbl(dsSummary.Rows[i]["Amount"].ToString());
                sheet[ROW2, ColParkStatus].Text = dsSummary.Rows[i]["IsPark"].ToString();
                sheet[ROW2, ColPartyTaxNo].Text = dsSummary.Rows[i]["PartyTaxNo"].ToString();
                sheet[ROW2, ColPDC].Number = clsStaticInfo.dbl(dsSummary.Rows[i]["PDC"].ToString());
                sheet[ROW2, ColAdvance].Number = clsStaticInfo.dbl(dsSummary.Rows[i]["Advance"].ToString());

                sheet.Range[ROW2, 1, ROW2, endCol2].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW2, 1, ROW2, endCol2].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW2, 1, ROW2, endCol2].CellStyle.Font.Size = 8f;
                ROW2++;

            }


            // Set report Name
            reportFileName = "Multi Vendor Payment Report.xlx";

            int ROW3 = ROW2 + 2; int COL3 = 1;
            reportUtility.SetMasterHeaderText(ref sheet, ROW3, COL3, "Deatils");
            sheet.Range[ROW3, COL3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[ROW3, COL3].CellStyle.Font.Bold = true;
            sheet.Range[ROW3, COL3, ROW3, COL3].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
            sheet.Range[ROW3, COL3, ROW3, COL3].CellStyle.Font.Color = ExcelKnownColors.White;

            int col = 0;
            row = 12;

            var summerCol = col - 1;

            row = 13;
            var startRow = row;

            //int ROW = 9; int COL = 1;
            int ROW = ROW2 + 3; int COL = 1;

            #region columns
            sheet[ROW, COL].Text = "Multiple Payment Detail No";
            sheet[ROW, COL].ColumnWidth = 16;
            int ColId = COL;
            COL++;

            sheet[ROW, COL].Text = "Party";
            sheet[ROW, COL].ColumnWidth = 16;
            int ColParty = COL;
            COL++;

            sheet[ROW, COL].Text = "Entry Date";
            sheet[ROW, COL].ColumnWidth = 16;
            int ColEntryDate = COL;
            COL++;

            sheet[ROW, COL].Text = "Voucher No";
            sheet[ROW, COL].ColumnWidth = 16;
            int ColVoucherNo = COL;
            COL++;

            sheet[ROW, COL].Text = "InvoiceNo";
            sheet[ROW, COL].ColumnWidth = 16;
            int ColInvoiceNo = COL;
            COL++;

            sheet[ROW, COL].Text = "Invoice Date";
            sheet[ROW, COL].ColumnWidth = 16;
            int ColInvoiceDate = COL;
            COL++;

            sheet[ROW, COL].Text = "Payment Amount";
            sheet[ROW, COL].ColumnWidth = 16;
            int ColPaymentAmount = COL;
            COL++;

            sheet[ROW, COL].Text = "Invoice Amount";
            sheet[ROW, COL].ColumnWidth = 16;
            int ColInvoiceAmount = COL;
            COL++;

            sheet[ROW, COL].Text = "Setoff";
            sheet[ROW, COL].ColumnWidth = 16;
            int ColSetoff = COL;
            COL++;

            sheet[ROW, COL].Text = "Balance";
            sheet[ROW, COL].ColumnWidth = 16;
            int ColBalance = COL;
            COL++;

            sheet[ROW, COL].Text = "PDC";
            sheet[ROW, COL].ColumnWidth = 16;
            int ColPDC2 = COL;
            COL++;

            sheet[ROW, COL].Text = "Advance";
            sheet[ROW, COL].ColumnWidth = 16;
            int ColAdvance2 = COL;

            #endregion columns

            int endCol = COL;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
            sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

            ROW++;

            //int startRow = ROW;

            for (int i = 0; i < dsLocal.Rows.Count; i++)
            {
                sheet[ROW, ColId].Text = dsLocal.Rows[i]["Id"].ToString();
                sheet[ROW, ColParty].Text = dsLocal.Rows[i]["PartyName"].ToString();
                sheet[ROW, ColEntryDate].Text = dsLocal.Rows[i]["EntryDate"].ToString();
                sheet[ROW, ColVoucherNo].Text = dsLocal.Rows[i]["VoucherNo"].ToString();
                sheet[ROW, ColInvoiceNo].Text = dsLocal.Rows[i]["InvoiceNo"].ToString();
                sheet[ROW, ColInvoiceDate].Text = dsLocal.Rows[i]["InvoiceDate"].ToString();
                sheet[ROW, ColPaymentAmount].Number = clsStaticInfo.dbl(dsLocal.Rows[i]["PaymentAmount"].ToString());
                sheet[ROW, ColInvoiceAmount].Number = clsStaticInfo.dbl(dsLocal.Rows[i]["InvoiceAmount"].ToString());
                sheet[ROW, ColSetoff].Number = clsStaticInfo.dbl(dsLocal.Rows[i]["Setoff"].ToString());
                sheet[ROW, ColBalance].Number = clsStaticInfo.dbl(dsLocal.Rows[i]["Balance"].ToString());
                sheet[ROW, ColPDC2].Number = clsStaticInfo.dbl(dsLocal.Rows[i]["PDC"].ToString());
                sheet[ROW, ColAdvance2].Number = clsStaticInfo.dbl(dsLocal.Rows[i]["Advance"].ToString());

                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                ROW++;

            }

            //var lastRow = ROW;

            row++;

            row = row + 4;

            sheet.UsedRange.AutofitColumns();
            sheet[row, 2].ColumnWidth = 22;
            sheet[row, 4].ColumnWidth = 15;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            reportUtility.CompanyPlantHeader(ref sheet, endCol, "Multi Vendor Payment", identity.CompanyId, identity.PlantName, null);

            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            //sheet.UsedRange.WrapText = true;
            sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            return workbook;
        }

        public DataTable MultiVendorPaymentDetailSQL(string mpdId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var strSQL = @"SELECT MPD.Id ,BM.AccountTitle, 0 c ,P.UserName PartyName,MPD.PartyId,0 as PDC,0 as Advance
							,FORMAT(I.AddedDate,'dd-MMM-yyyy') EntryDate
							,I.DocRefNo InvoiceNo,V.VoucherNo,Replace(CONVERT(VARCHAR(11), I.PostingDate, 106), ' ', '-') InvoiceDate,VT.UserName VoucherType,I.PaymentSource,C.Name Currency,I.Narration
							,MPD.Amount PaymentAmount,I.Amount InvoiceAmount,I.WrittenOffAmount Setoff,Balance=(I.Amount-I.WrittenOffAmount)
							FROM TRN.MultiplePaymentDetail MPD 
							LEFT JOIN TRN.Invoice I ON I.Id=MPD.InvoiceId
							left join TRN.Voucher V on V.Id=I.VoucherId
							LEFT JOIN SCS.Currency C ON C.Id=I.CurrencyId
							LEFT JOIN SCS.VoucherType VT ON VT.Id=I.VoucherTypeId
							LEFT JOIN TRN.MultiplePayment MP ON MP.Id=MPD.MultiplePaymentId
							LEFT JOIN HKP.Party P ON P.Id=MPD.PartyId
							LEFT JOIN MST.BankMaster BM ON BM.Id=MP.BankMasterId
                            where  MP.PlantId='" + identity.PlantId + @"' AND MPD.MultiplePaymentId='" + mpdId + @"'
                            order by P.UserName";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        public DataTable MultiVendorPaymentSummarySQL(string mpdId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var strSQL = @"SELECT MP.Id,MP.CompanyGroupId,MP.CompanyId,MP.PlantId,MP.SourceType,Replace(CONVERT(VARCHAR(11), MP.DueUpToDate, 106), ' ', '-') DueUpToDate
                            ,Replace(CONVERT(VARCHAR(11), MP.TentativeDate, 106), ' ', '-') TentativeDate
                            ,MP.BankMasterId,MP.IsFifo,MP.IsPark ,BM.AccountTitle, 0 flag ,P.UserName PartyName,P.TINNO PartyTaxNo,0 as PDC,0 as Advance
							,MPD.PartyId,SUM(MPD.Amount) Amount
							FROM TRN.MultiplePaymentDetail MPD 
							LEFT JOIN TRN.MultiplePayment MP ON MP.Id=MPD.MultiplePaymentId
							LEFT JOIN HKP.Party P ON P.Id=MPD.PartyId
							LEFT JOIN MST.BankMaster BM ON BM.Id=MP.BankMasterId
							where  MP.PlantId='" + identity.PlantId + @"' AND MPD.MultiplePaymentId='" + mpdId + @"'
                            group by MP.Id,MP.CompanyGroupId,MP.CompanyId,MP.PlantId,MP.SourceType,MP.DueUpToDate
                            , MP.TentativeDate,MPD.MultiplePaymentId,MP.BankMasterId
                            ,MP.IsFifo,MP.IsPark ,BM.AccountTitle,P.UserName,MPD.PartyId,P.TINNO";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
    }
}
