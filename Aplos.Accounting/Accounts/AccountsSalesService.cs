using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Invoices;
using Library.Model.Vouchers;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Taxations;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Library.Accounting.Accounts
{
    public class AccountsSalesService
    {
        private readonly ISqlRepository _sqlRepository;
		public AccountsSalesService(ISqlRepository sqlRepository
			)
        {
            _sqlRepository = sqlRepository;
		}
        public List<Dictionary<string, object>> GetMasterOrderSalesList(string companyGroupId, string companyId)
        {
            try
            {
                var cmdText = @"SELECT S.Id,S.Id AS SalesId, S.PartyId, P.Code AS PartyCode, P.UserName AS PartyName,P.Code Tracenent, S.CurrencyId, C.Code AS CurrencyCode
									, DocRefNo=case when S.SourceType='MasterOrderSales' then 'MS-'+ S.DocRefNo WHEN S.SourceType='Packing' THEN 'PS-'+S.DocRefNo else 'S-'+ S.DocRefNo end
									, ISNULL(SM.Amount,0) + ISNULL(SS.Amount,0) AS Amount,
									Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') InvoiceDate,Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') DocDate,
									Replace(CONVERT(VARCHAR(11), S.EntryDate, 106), ' ', '-') VoucherDate, Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') PostingDate
                                    , S.RowState, S.DeliveryPartyPlantId, S.InvoicingPartyPlantId AS PartyPlantId, S.InvoicingPartyPlantId, S.EntityId, S.PaymentTermId
									, Replace(CONVERT(VARCHAR(11), S.MatureDate, 106), ' ', '-')  MatureDate, Replace(CONVERT(VARCHAR(11), S.BaseOnDueDate, 106), ' ', '-') BaseOnDueDate,S.BaseNoOfDays
									, InvoiceNo=case when S.SourceType='MasterOrderSales' then 'MS-'+ S.InvoiceNo WHEN S.SourceType='Packing' THEN 'PS-'+S.InvoiceNo else 'S-'+ S.InvoiceNo end
									, PPI.UserName AS BillTo, AM.StateId AS InvoicingStateId, ST.UserName AS InvoicingState, PPI.GSTIN AS InvoicingGSTIN
									, PPD.UserName AS ShipTo, STD.UserName AS DeliveryState, PPD.GSTIN AS DeliveryGSTIN, S.InvoicingByAddress, S.DeliveryByAddress, S.ToCurrencyRate
									, S.ToCurrencyRate AS CompanyCurrencyRate, S.Narration, S.PartyType, S.VoucherId, AMP.StateId AS PlantStateId
                                    , CASE  WHEN S.RowState='Parked' THEN 1 ELSE 0 END AS IsPark, CP.TaxApplicable,CP.PartyAccountGroupId,CP.IsPaymentTermChangeable
									,PPI.GSTIN GSTINNo, S.SourceType--,SP.Id SalesPackingId
									FROM [TRN].[Sales] AS S
                                    JOIN [HKP].[Party] AS P ON P.Id=S.PartyId
                                    LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND S.PlantId=CP.PlantId AND CP.PartyType='Customer'
									LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=S.InvoicingPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=S.DeliveryPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
									LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=S.CurrencyId
									LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=S.PlantId
									LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PT.AddressMasterId
									--LEFT JOIN dbo.SalesPacking SP ON SP.SalesId=S.Id
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesMaterial] M GROUP BY M.SalesId) AS SM ON SM.SalesId=S.Id
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesService] M GROUP BY M.SalesId) AS SS ON SS.SalesId=S.Id
                                    WHERE S.CompanyGroupId='" + companyGroupId + "' AND S.CompanyId='" + companyId + @"' AND ISNULL(S.VoucherId,'')='' 
									AND S.SourceType in ('MasterOrderSales','Packing') AND ISNULL(SM.Amount,0) + ISNULL(SS.Amount,0)>0";
                return _sqlRepository.GetDataCollection(cmdText);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
		public List<Dictionary<string, object>> GetMasterOrderSalesIncentiveList(string companyGroupId, string companyId)
		{
			try
			{
				var cmdText = @"SELECT S.Id,S.Id AS SalesId, S.PartyId, P.Code AS PartyCode, P.UserName AS PartyName,P.Code Tracenent, S.CurrencyId, C.Code AS CurrencyCode
									, DocRefNo=case when S.SourceType='MasterOrderSales' then 'MS-'+ S.DocRefNo WHEN S.SourceType='Packing' THEN 'PS-'+S.DocRefNo else 'S-'+ S.DocRefNo end
									, ISNULL(SM.Amount,0) + ISNULL(SS.Amount,0) AS Amount,
									Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') InvoiceDate,Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') DocDate,
									Replace(CONVERT(VARCHAR(11), S.EntryDate, 106), ' ', '-') VoucherDate, Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') PostingDate
                                    , S.RowState, S.DeliveryPartyPlantId, S.InvoicingPartyPlantId AS PartyPlantId, S.InvoicingPartyPlantId, S.EntityId, S.PaymentTermId
									, Replace(CONVERT(VARCHAR(11), S.MatureDate, 106), ' ', '-')  MatureDate, Replace(CONVERT(VARCHAR(11), S.BaseOnDueDate, 106), ' ', '-') BaseOnDueDate,S.BaseNoOfDays
									, InvoiceNo=case when S.SourceType='MasterOrderSales' then 'MS-'+ S.InvoiceNo WHEN S.SourceType='Packing' THEN 'PS-'+S.InvoiceNo else 'S-'+ S.InvoiceNo end
									, PPI.UserName AS BillTo, AM.StateId AS InvoicingStateId, ST.UserName AS InvoicingState, PPI.GSTIN AS InvoicingGSTIN
									, PPD.UserName AS ShipTo, STD.UserName AS DeliveryState, PPD.GSTIN AS DeliveryGSTIN, S.InvoicingByAddress, S.DeliveryByAddress, S.ToCurrencyRate
									, S.ToCurrencyRate AS CompanyCurrencyRate, S.Narration, S.PartyType, S.VoucherId, AMP.StateId AS PlantStateId
                                    , CASE  WHEN S.RowState='Parked' THEN 1 ELSE 0 END AS IsPark, CP.TaxApplicable,CP.PartyAccountGroupId,CP.IsPaymentTermChangeable
									, S.SourceType--,SP.Id SalesPackingId
									FROM [TRN].[Sales] AS S
                                    JOIN [HKP].[Party] AS P ON P.Id=S.PartyId
                                    LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND S.PlantId=CP.PlantId AND CP.PartyType='Customer'
									LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=S.InvoicingPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=S.DeliveryPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
									LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=S.CurrencyId
									LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=S.PlantId
									LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PT.AddressMasterId
									--LEFT JOIN dbo.SalesPacking SP ON SP.SalesId=S.Id
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesMaterial] M GROUP BY M.SalesId) AS SM ON SM.SalesId=S.Id
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesService] M GROUP BY M.SalesId) AS SS ON SS.SalesId=S.Id
                                    WHERE S.CompanyGroupId='" + companyGroupId + "' AND S.CompanyId='" + companyId + "' AND ISNULL(S.VoucherId,'')='' and S.SourceType in ('Sales','MasterOrderSales','Packing') AND S.IsIncentiveApplicable=1";
				return _sqlRepository.GetDataCollection(cmdText);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}
		public List<Dictionary<string, object>> GetSalesPackingList(string companyGroupId, string companyId)
		{
			try
			{
				var cmdText = @"SELECT S.Id,S.Id AS SalesId, S.PartyId, P.Code AS PartyCode, P.UserName AS PartyName,P.Code Tracenent, S.CurrencyId, C.Code AS CurrencyCode,'MSS-'+ S.DocRefNo DocRefNo, ISNULL(SM.Amount,0) + ISNULL(SS.Amount,0) AS Amount,
									Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') InvoiceDate,Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') DocDate,
									Replace(CONVERT(VARCHAR(11), S.EntryDate, 106), ' ', '-') VoucherDate, Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') PostingDate
                                    , S.RowState, S.DeliveryPartyPlantId, S.InvoicingPartyPlantId AS PartyPlantId, S.InvoicingPartyPlantId, S.EntityId, S.PaymentTermId
									, Replace(CONVERT(VARCHAR(11), S.MatureDate, 106), ' ', '-')  MatureDate, Replace(CONVERT(VARCHAR(11), S.BaseOnDueDate, 106), ' ', '-') BaseOnDueDate,S.BaseNoOfDays
									,InvoiceNo=case when S.SourceType='MasterOrderSales' then 'MS-'+ S.InvoiceNo else 'S-'+ S.InvoiceNo end
									, PPI.UserName AS BillTo, AM.StateId AS InvoicingStateId, ST.UserName AS InvoicingState, PPI.GSTIN AS InvoicingGSTIN
									, PPD.UserName AS ShipTo, STD.UserName AS DeliveryState, PPD.GSTIN AS DeliveryGSTIN, S.InvoicingByAddress, S.DeliveryByAddress, S.ToCurrencyRate
									, S.ToCurrencyRate AS CompanyCurrencyRate, S.Narration, S.PartyType, S.VoucherId, AMP.StateId AS PlantStateId
                                    , CASE  WHEN S.RowState='Parked' THEN 1 ELSE 0 END AS IsPark, CP.TaxApplicable,CP.PartyAccountGroupId,CP.IsPaymentTermChangeable
									FROM [TRN].[Sales] AS S
                                    JOIN [HKP].[Party] AS P ON P.Id=S.PartyId
                                    LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND S.PlantId=CP.PlantId AND CP.PartyType='Customer'
									LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=S.InvoicingPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=S.DeliveryPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
									LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=S.CurrencyId
									LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=S.PlantId
									LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PT.AddressMasterId
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesMaterial] M GROUP BY M.SalesId) AS SM ON SM.SalesId=S.Id
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesService] M GROUP BY M.SalesId) AS SS ON SS.SalesId=S.Id
                                    WHERE S.CompanyGroupId='" + companyGroupId + "' AND S.CompanyId='" + companyId + "' AND ISNULL(S.VoucherId,'')='' and S.SourceType='Packing'";
				return _sqlRepository.GetDataCollection(cmdText);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}

		public List<Dictionary<string, object>> GetMasterOrderSalesDetailList(string companyGroupId, string companyId, string salesId, string partyAccountGroup)
        {
            try
            {
                var cmdText = @"SELECT S.Id,S.Id AS SalesId,SM.Id SalesMaterialId,MGM.UserName MaterialGroup,MM.MaterialGroupMasterId,MM.UserName Material,SM.MaterialMasterId,MMA.StandardName Article,SM.ArticleId
									, SM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
									 , SM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
									 , SM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
									 , SM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
									 , SM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
									 , SM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
									 , SM.TransactionUoMId, TUoM.UserName AS TransactionUoM
									 ,C.Code CurrencyName,SM.TransactionQty,SM.TransactionRate,SM.TransactionAmount
									  , ISNULL(SM.BaseAmount*S.ToCurrencyRate,0)+ISNULL(SM.TaxAmount*S.ToCurrencyRate,0) + ISNULL(SS.Amount*S.ToCurrencyRate,0) AS BaseAmount
									 , SM.TaxAmount,HSNCode=ISNULL(MHSN.Code,HSN.Code) ,SM.IsCanceled
							,MGGL.GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName 
							,MGGL.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,MGGL.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName
									FROM TRN.SalesMaterial SM 
									JOIN  [TRN].[Sales] AS S ON S.Id=SM.SalesId
									LEFT JOIN MST.MaterialMaster MM ON MM.Id=SM.MaterialMasterId
									LEFT JOIN MST.MaterialGroupMaster MGM ON MGM.Id=MM.MaterialGroupMasterId
									LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=SM.ArticleId
                                    LEFT JOIN HKP.Characteristics AS FC ON SM.FirstCharacteristicsId=FC.Id
									LEFT JOIN HKP.Characteristics AS SC ON SM.SecondCharacteristicsId=SC.Id
									LEFT JOIN HKP.Characteristics AS TC ON SM.ThirdCharacteristicsId=TC.Id
									LEFT JOIN HKP.CharacteristicsValue AS FCV ON SM.FirstCharacteristicsValueId=FCV.Id
									LEFT JOIN HKP.CharacteristicsValue AS SCV ON SM.SecondCharacteristicsValueId=SCV.Id
									LEFT JOIN HKP.CharacteristicsValue AS TCV ON SM.ThirdCharacteristicsValueId=TCV.Id
									JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
									LEFT JOIN SCS.Currency C ON C.Id=S.CurrencyId
									LEFT JOIN (SELECT MGPA.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId 
									JOIN HKP.MaterialGroupPartyAccountGroupGL MGPA ON MGPA.MaterialGroupGLId=MGGL.Id AND MGPA.GLType='Sales' AND MGPA.PartyAccountGroupId='" + partyAccountGroup + @"'
									WHERE C.Id='"+ companyId + @"')
									AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId 

									LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.GLGeneralInfoId=GL.Id
									LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.BudgetMasterId= BM.Id
									LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
									LEFT JOIN [HKP].[Activity] AS A ON MGGL.ActivityId= A.Id
									LEFT JOIN (SELECT DISTINCT HsnCodeId,SalesMaterialId FROM TRN.SalesTax ) STH ON STH.SalesMaterialId=SM.Id
									LEFT JOIN[HKP].[HSNCode] AS MHSN ON MHSN.ID = STH.HSNCodeId
									LEFT JOIN HKP.HSNCode HSN ON HSN.Id=MM.HSNCodeId
									 		--LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesMaterial] M GROUP BY M.SalesId) AS SM ON SM.SalesId=S.Id
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesService] M GROUP BY M.SalesId) AS SS ON SS.SalesId=S.Id
                                    WHERE S.CompanyGroupId='" + companyGroupId + "' AND S.CompanyId='" + companyId + "' AND S.Id='"+ salesId + @"' AND S.VoucherId IS NULL";
                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
		public List<Dictionary<string, object>> GetMasterOrderSalesServiceDetailList(string companyGroupId, string companyId, string salesId, string partyAccountGroup)
		{
			try
			{
				var cmdText = @"SELECT SM.Id,SM.Id SalesServiceId,S.Id AS SalesId, MM.UserName ServiceMasterName,SG.UserName ServiceGroup,SG.Id ServiceGroupId
									 , ISNULL(SM.Amount,0)  AS  Amount
									 , SM.TaxAmount,MGGL.GLGeneralInfoId
									 ,GL.AccountCode GLGeneralInfoCode
									 ,GL.UserName GLGeneralInfoName 
									 ,MGGL.BudgetMasterId BudgetMasterId
									 ,B.Code BudgetCode
									 ,B.UserName BudgetName
									 ,MGGL.ActivityId ActivityId
									 ,A.Code ActivityCode
									 ,A.UserName ActivityName
									FROM TRN.SalesService SM 
									JOIN  [TRN].[Sales] AS S ON S.Id=SM.SalesId
									LEFT JOIN HKP.ServiceMaster MM ON MM.Id=SM.ServiceMasterId
									LEFT JOIN HKP.ServiceGroup SG ON SG.Id=MM.ServiceGroupId
						LEFT JOIN (SELECT MGPA.* FROM [ORG].[Company] AS C JOIN [HKP].[ServiceGroupGL] AS MGGL ON C.COAId=MGGL.COAId 
						 JOIN HKP.ServiceGroupPartyAccountGroupGL MGPA ON MGPA.ServiceGroupGLId=MGGL.Id AND MGPA.GLType='Sales' AND MGPA.PartyAccountGroupId='" + partyAccountGroup + @"'
						WHERE C.Id='" + companyId + @"')
								AS MGGL ON MM.ServiceGroupId = MGGL.ServiceGroupId 

						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.ActivityId= A.Id
                                    WHERE S.CompanyGroupId='" + companyGroupId + "' AND S.CompanyId='" + companyId + "' AND S.Id='" + salesId + @"' AND S.VoucherId IS NULL";
				return _sqlRepository.GetDataCollection(cmdText);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}
		public List<Dictionary<string, object>> GetMasterOrderSalesPostedList(string companyGroupId, string companyId, string plantId, string column, string value)
        {
            try
            {
				string strkey = "1=1";
				if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
					strkey = column + " like '%" + value + "%'";
				var sql = @"DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        select top 300 * from ( SELECT S.Id,S.Id AS SalesId, S.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, S.CurrencyId, C.Code AS CurrencyCode, S.DocRefNo, ISNULL(SM.Amount,0) + ISNULL(SS.Amount,0) AS Amount,
									Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') InvoiceDate,
									Replace(CONVERT(VARCHAR(11), S.EntryDate, 106), ' ', '-') VoucherDate, Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') PostingDate
                                    , S.RowState, S.DeliveryPartyPlantId, S.InvoicingPartyPlantId AS PartyPlantId, S.InvoicingPartyPlantId, S.EntityId, S.PaymentTermId, S.BaseNoOfDays, S.BaseOnDueDate
									, S.InvoiceNo, PPI.UserName AS BillTo, AM.StateId AS InvoicingStateId, ST.UserName AS InvoicingState, PPI.GSTIN AS InvoicingGSTIN
									, PPD.UserName AS ShipTo, STD.UserName AS DeliveryState, PPD.GSTIN AS DeliveryGSTIN, S.InvoicingByAddress, S.DeliveryByAddress, S.MatureDate, S.ToCurrencyRate
									, S.ToCurrencyRate AS CompanyCurrencyRate, S.Narration, S.PartyType, S.VoucherId, AMP.StateId AS PlantStateId
                                    , CASE  WHEN S.RowState='Parked' THEN 1 ELSE 0 END AS IsPark
									,V.VoucherNo,SalesPackingVoucherId=STUFF((select distinct ','+SP.VoucherId from dbo.SalesPacking SP                                          
							                                where SP.SalesId=S.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									FROM [TRN].[Sales] AS S
                                    JOIN [HKP].[Party] AS P ON P.Id=S.PartyId
									LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=S.InvoicingPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=S.DeliveryPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
									LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=S.CurrencyId
									LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=S.PlantId
									LEFT JOIN [TRN].Voucher V ON V.Id=S.VoucherId
									LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PT.AddressMasterId
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesMaterial] M GROUP BY M.SalesId) AS SM ON SM.SalesId=S.Id
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesService] M GROUP BY M.SalesId) AS SS ON SS.SalesId=S.Id
                                    WHERE V.Archive=0 AND S.CompanyGroupId='" + companyGroupId + "' AND S.CompanyId='" + companyId + "' AND S.PlantId='" + plantId + "' AND S.VoucherId<>''" +
									") AS TEMP WHERE " + strkey + " order by PostingDate DESC";
				return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

		public IEnumerable<object> GetMasterOrderSalesReceivable(string companyId, string plantId, string salesId,string taxApplicable,string partyAccountGroup)
		{
			try
			{
				
				if (taxApplicable=="Mandatory")
				{
					var sql = @"DECLARE @salesId varchar(10)='" + salesId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"'
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						, T.Cr, T.Amount,T.IsAsset 
					FROM (
						SELECT  'Sales' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.IsAsset,MM.FixedAssetMasterId

							,MGGL.GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName 
							,MGGL.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,MGGL.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName
							
							, NULL Dr, SUM(IRD.TransactionAmount) AS Cr
							, SUM(IRD.TransactionAmount) AS Amount
						FROM [TRN].[SalesMaterial] AS IRD
						LEFT JOIN [TRN].[Sales] AS IR ON IRD.SalesId=IR.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IRD.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGPA.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId 
						 JOIN HKP.MaterialGroupPartyAccountGroupGL MGPA ON MGPA.MaterialGroupGLId=MGGL.Id AND MGPA.GLType='Sales' AND MGPA.PartyAccountGroupId='" + partyAccountGroup + @"'
						WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId 

						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.ActivityId= A.Id
						
						WHERE IRD.SalesId=@salesId
						GROUP BY MM.MaterialGroupMasterId, MGGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGGL.BudgetMasterId, B.Code, B.UserName, MGGL.ActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,MM.IsAsset
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset
					
                   
                    UNION
					
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
						, T.Amount ,T.IsAsset
                           
					FROM (
						SELECT 'Customer' AS OtherName,'Dr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
						, MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName
							, MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,SUM(MAT.Dr)  AS Dr
						,NULL Cr,
						SUM(MAT.Dr)  AS Amount ,MAT.IsAsset
						FROM (
							SELECT IR.Id, 'Customer' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
                            ,CPGL.GLGeneralInfoId GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName 
							,CPGL.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,CPGL.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName
							
							, SUM(IRD.TransactionAmount)+ISNULL(SS.ServiceTotalAmount,0) AS Dr, 0 Cr
							, SUM(IRD.TransactionAmount)+ISNULL(SS.ServiceTotalAmount,0) AS Amount
                             ,MM.IsAsset
						FROM [TRN].[SalesMaterial] AS IRD
						LEFT JOIN [TRN].[Sales] AS IR ON IRD.SalesId=IR.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IRD.MaterialMasterId=MM.Id
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN [HKP].[CompanyParty] CP ON IR.PartyId = CP.PartyId AND CP.PlantId=@plantId AND CP.PartyType='Customer'
						LEFT JOIN HKP.CompanyPartyGL CPGL ON CPGL.CompanyPartyId=CP.Id and CPGL.PartyGLType='ReconciliationGL' 
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON CPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON CPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON CPGL.ActivityId= A.Id
						LEFT JOIN (SELECT SalesId,ISNULL(SUM(Amount),0) ServiceTotalAmount FROM [TRN].[SalesService] GROUP BY SalesId) SS ON SS.SalesId=IR.Id


						WHERE IRD.SalesId=@salesId
						GROUP BY  IR.Id, CPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, CPGL.BudgetMasterId, B.Code, B.UserName, CPGL.ActivityId, A.Code, A.UserName
						,MM.IsAsset,SS.ServiceTotalAmount
						) AS MAT
						
						GROUP BY  MAT.Id,MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName , MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,MAT.IsAsset
					) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr
					, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset
					UNION
					SELECT 'TaxPayable' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, TCGL.LiabilityGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.LiabilityBudgetMasterId BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.LiabilityActivityId ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  NULL Dr, SUM(IRT.Amount) AS Cr
						, SUM(IRT.Amount) AS Amount
                        ,0 IsAsset
					FROM [TRN].[SalesTax] AS IRT
					LEFT JOIN [TRN].[SalesMaterial] AS IRD ON IRT.SalesMaterialId=IRD.Id
                    LEFT JOIN [TRN].[Sales] AS IR ON IRD.SalesId=IR.Id
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.LiabilityGLId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.LiabilityBudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.LiabilityActivityId= A.Id
					WHERE IRD.SalesId=@salesId AND TCGL.InputTaxOutPutTax='Output' AND ISNULL(TCGL.TaxType,'')='Excluded' AND IRT.SalesMaterialId<>'' 
					GROUP BY  IRT.TaxCategoryId, TCGL.LiabilityGLId, GL.AccountCode, GL.UserName, TCGL.LiabilityBudgetMasterId, B.Code, B.UserName, TCGL.LiabilityActivityId, A.Code, A.UserName
					
					UNION
					SELECT 'TaxReceivable' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  SUM(IRT.Amount) AS Dr, NULL  Cr
						, SUM(IRT.Amount) AS Amount
                        ,0 IsAsset
					FROM [TRN].[SalesTax] AS IRT
					LEFT JOIN [TRN].[SalesMaterial] AS IRD ON IRT.SalesMaterialId=IRD.Id
                    LEFT JOIN [TRN].[Sales] AS IR ON IRD.SalesId=IR.Id
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE IRD.SalesId=@salesId AND TCGL.InputTaxOutPutTax='Output' AND ISNULL(TCGL.TaxType,'')='Excluded' AND IRT.SalesMaterialId<>'' 
					GROUP BY  IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					
					UNION
					SELECT 'SVTaxPayable' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  NULL Dr, SUM(IRT.Amount) AS Cr
						, SUM(IRT.Amount) AS Amount
                        ,0 IsAsset
					FROM [TRN].[SalesTax] AS IRT
					LEFT JOIN [TRN].[SalesService] AS IRD ON IRT.SalesServiceId=IRD.Id
                    LEFT JOIN [TRN].[Sales] AS IR ON IRD.SalesId=IR.Id
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.LiabilityGLId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.LiabilityBudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.LiabilityActivityId= A.Id
					WHERE IRD.SalesId=@salesId AND TCGL.InputTaxOutPutTax='Output' AND ISNULL(TCGL.TaxType,'')='Excluded' AND IRT.SalesServiceId<>'' 
					GROUP BY  IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					
					UNION
					SELECT 'SVTaxReceivable' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  SUM(IRT.Amount) AS Dr, NULL  Cr
						, SUM(IRT.Amount) AS Amount
                        ,0 IsAsset
					FROM [TRN].[SalesTax] AS IRT
					LEFT JOIN [TRN].[SalesService] AS IRD ON IRT.SalesServiceId=IRD.Id
                    LEFT JOIN [TRN].[Sales] AS IR ON IRD.SalesId=IR.Id
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE IRD.SalesId=@salesId AND TCGL.InputTaxOutPutTax='Output' AND ISNULL(TCGL.TaxType,'')='Excluded' AND IRT.SalesServiceId<>'' 
					GROUP BY  IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					UNION
					SELECT  'Service' AS OtherName, 'Cr' AS TrnType, SM.ServiceGroupId MaterialGroupMasterId, NULL AS TaxCategoryId

							,MGGL.GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName 
							,MGGL.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,MGGL.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName
							
							, NULL Dr, SUM(IRD.Amount) AS Cr
							, SUM(IRD.Amount) AS Amount
                            ,0 IsAsset 
						FROM [TRN].[SalesService] AS IRD
						LEFT JOIN [TRN].[Sales] AS IR ON IRD.SalesId=IR.Id
						LEFT JOIN [HKP].[ServiceMaster] AS SM ON IRD.ServiceMasterId=SM.Id
						LEFT JOIN HKP.ServiceGroup SG ON SG.Id=SM.ServiceGroupId
						LEFT JOIN (SELECT MGPA.* FROM [ORG].[Company] AS C JOIN [HKP].[ServiceGroupGL] AS MGGL ON C.COAId=MGGL.COAId 
						 JOIN HKP.ServiceGroupPartyAccountGroupGL MGPA ON MGPA.ServiceGroupGLId=MGGL.Id AND MGPA.GLType='Sales' AND MGPA.PartyAccountGroupId='" + partyAccountGroup + @"'
						WHERE C.Id=@companyId)
								AS MGGL ON SM.ServiceGroupId = MGGL.ServiceGroupId 

						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.ActivityId= A.Id
						WHERE IRD.SalesId=@salesId
						GROUP BY SM.ServiceGroupId, MGGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGGL.BudgetMasterId, B.Code, B.UserName, MGGL.ActivityId, A.Code, A.UserName
					    ,IRD.Id
					
					ORDER BY T.TrnType DESC ";
					return _sqlRepository.GetDataCollection(sql);
				}
				else
				{
					var sql = @"DECLARE @salesId varchar(10)='" + salesId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"'
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId,T.TaxCodeId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						, T.Cr, T.Amount,T.IsAsset 
					FROM (
						SELECT  'Sales' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,NULL TaxCodeId,MM.IsAsset

							,MGGL.GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName 
							,MGGL.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,MGGL.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName
							
							, NULL Dr, SUM(IRD.TransactionAmount) AS Cr
							, SUM(IRD.TransactionAmount) AS Amount
                            
						FROM [TRN].[SalesMaterial] AS IRD
						LEFT JOIN [TRN].[Sales] AS IR ON IRD.SalesId=IR.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IRD.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGPA.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId 
						 JOIN HKP.MaterialGroupPartyAccountGroupGL MGPA ON MGPA.MaterialGroupGLId=MGGL.Id AND MGPA.GLType='Sales' AND MGPA.PartyAccountGroupId='" + partyAccountGroup + @"'
						WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId 

						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.ActivityId= A.Id
						WHERE IRD.SalesId=@salesId
						GROUP BY MM.MaterialGroupMasterId, MGGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGGL.BudgetMasterId, B.Code, B.UserName, MGGL.ActivityId, A.Code, A.UserName
					    ,MM.IsAsset
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset,T.TaxCodeId
					
                   
                    UNION
					
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId,T.TaxCodeId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
						, T.Amount ,T.IsAsset
                           
					FROM (
						SELECT 'Customer' AS OtherName,'Dr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId,NULL TaxCodeId
						, MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName
							, MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,SUM(MAT.Dr)  AS Dr
						,NULL Cr,
						SUM(MAT.Dr)  AS Amount ,MAT.IsAsset
						FROM (
							SELECT IR.Id, 'Customer' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
                            ,CPGL.GLGeneralInfoId GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName 
							,CPGL.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,CPGL.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName
							
							, SUM(IRD.TransactionAmount+IRD.TaxAmount)+ISNULL(TCS.TCSAmount,0)+ISNULL(SS.ServiceTotalAmount,0) AS Dr, 0 Cr
							, SUM(IRD.TransactionAmount+IRD.TaxAmount)+ISNULL(TCS.TCSAmount,0)+ISNULL(SS.ServiceTotalAmount,0) AS Amount
                             ,MM.IsAsset
						FROM [TRN].[SalesMaterial] AS IRD
						LEFT JOIN [TRN].[Sales] AS IR ON IRD.SalesId=IR.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IRD.MaterialMasterId=MM.Id
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN [HKP].[CompanyParty] CP ON IR.PartyId = CP.PartyId AND CP.PlantId=@plantId AND CP.PartyType='Customer'
						LEFT JOIN HKP.CompanyPartyGL CPGL ON CPGL.CompanyPartyId=CP.Id and CPGL.PartyGLType='ReconciliationGL' 
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON CPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON CPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON CPGL.ActivityId= A.Id
						LEFT JOIN (SELECT SalesId,ISNULL(SUM(Amount+TaxAmount),0) ServiceTotalAmount FROM [TRN].[SalesService] GROUP BY SalesId) SS ON SS.SalesId=IR.Id
						LEFT OUTER JOIN (
						SELECT INS.SalesId, sum(INS.TaxAmount) AS TCSAmount
						from  [TRN].[SalesAdditionalTax] AS INS
						LEFT JOIN TRN.InventoryReceive AS IR ON IR.Id=INS.SalesId 
                        where SalesId=@salesId group by INS.SalesId
						) AS TCS on TCS.SalesId=@salesId
						WHERE IRD.SalesId=@salesId

						GROUP BY  IR.Id, CPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, CPGL.BudgetMasterId, B.Code, B.UserName, CPGL.ActivityId, A.Code, A.UserName
						,MM.IsAsset,SS.ServiceTotalAmount,TCS.TCSAmount
						) AS MAT
						
						GROUP BY  MAT.Id,MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName , MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,MAT.IsAsset
					) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr
					, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset,T.TaxCodeId
					
					UNION
					SELECT 'TaxPayable' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId,NULL TaxCodeId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  NULL Dr, SUM(IRT.Amount) AS Cr
						, SUM(IRT.Amount) AS Amount
                        ,0 IsAsset
					FROM [TRN].[SalesTax] AS IRT
					LEFT JOIN [TRN].[SalesMaterial] AS IRD ON IRT.SalesMaterialId=IRD.Id
                    LEFT JOIN [TRN].[Sales] AS IR ON IRD.SalesId=IR.Id
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE IRD.SalesId=@salesId AND TCGL.InputTaxOutPutTax='Output' AND  TCGL.TaxType IS NULL AND IRT.SalesMaterialId<>'' 
					GROUP BY  IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					UNION

					SELECT  'Service' AS OtherName, 'Cr' AS TrnType, SM.ServiceGroupId MaterialGroupMasterId, NULL AS TaxCategoryId,NULL TaxCodeId

							,MGGL.GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName 
							,MGGL.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,MGGL.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName
							
							, NULL Dr, SUM(IRD.Amount) AS Cr
							, SUM(IRD.Amount) AS Amount
                            ,0 IsAsset
						FROM [TRN].[SalesService] AS IRD
						LEFT JOIN [TRN].[Sales] AS IR ON IRD.SalesId=IR.Id
						LEFT JOIN [HKP].[ServiceMaster] AS SM ON IRD.ServiceMasterId=SM.Id
						LEFT JOIN HKP.ServiceGroup SG ON SG.Id=SM.ServiceGroupId
						LEFT JOIN (SELECT MGPA.* FROM [ORG].[Company] AS C JOIN [HKP].[ServiceGroupGL] AS MGGL ON C.COAId=MGGL.COAId 
						 JOIN HKP.ServiceGroupPartyAccountGroupGL MGPA ON MGPA.ServiceGroupGLId=MGGL.Id AND MGPA.GLType='Sales' AND MGPA.PartyAccountGroupId='" + partyAccountGroup + @"'
						WHERE C.Id=@companyId)
								AS MGGL ON SM.ServiceGroupId = MGGL.ServiceGroupId 

						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.ActivityId= A.Id
						WHERE IRD.SalesId=@salesId
						GROUP BY SM.ServiceGroupId, MGGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGGL.BudgetMasterId, B.Code, B.UserName, MGGL.ActivityId, A.Code, A.UserName
					    ,IRD.Id
					UNION
					SELECT 'SVTaxPayable' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId,NULL TaxCodeId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  NULL Dr, SUM(IRT.Amount) AS Cr
						, SUM(IRT.Amount) AS Amount
                        ,0 IsAsset
					FROM [TRN].[SalesTax] AS IRT
					LEFT JOIN [TRN].[SalesService] AS IRD ON IRT.SalesServiceId=IRD.Id
                    LEFT JOIN [TRN].[Sales] AS IR ON IRD.SalesId=IR.Id
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE IRD.SalesId=@salesId AND TCGL.InputTaxOutPutTax='Output' AND  TCGL.TaxType IS NULL AND IRT.SalesServiceId<>'' 
					GROUP BY  IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					UNION
					SELECT 'TCSPayable' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId,IRT.TaxCodeId
						, TGL.WithholdCreditableGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TGL.WithholdCreditableBudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TGL.WithholdCreditableActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  NULL Dr, SUM(IRT.TaxAmount) AS Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset
					FROM [TRN].[SalesAdditionalTax] AS IRT
                    LEFT JOIN [TRN].[Sales] AS IR ON IRT.SalesId=IR.Id
					LEFT JOIN MST.TaxCode TCO ON TCO.Id=IRT.TaxCodeId  
					LEFT JOIN MST.TaxCodeGL TGL ON TGL.TaxCodeId=TCO.Id 
					LEFT JOIN [MST].[TaxCategory] AS TC ON IRT.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TGL.WithholdCreditableGLId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TGL.WithholdCreditableBudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TGL.WithholdCreditableActivityId= A.Id
					WHERE IRT.SalesId=@salesId AND TCO.InputOrOutput='" + TaxCodeInputOutput.Output + @"' 
					GROUP BY  IRT.TaxCategoryId,IRT.TaxCodeId, TGL.WithholdCreditableGLId, GL.AccountCode, GL.UserName, TGL.WithholdCreditableBudgetMasterId, B.Code
					, B.UserName, TGL.WithholdCreditableActivityId, A.Code, A.UserName
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
		public IEnumerable<object> GetSalesListForInvReveivable(string column, string value, string plantId)
		{
			try
			{
				string strkey = "1=1";
				if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
					strkey = column + " like '%" + value + "%'";
				var sql = @"select top 300 * from (SELECT  IVS.Id, REPLACE(CONVERT(CHAR(11), IVS.SalesDate, 106),' ','-') AS SalesDate, IVS.CompanyGroupId, IVS.CompanyId, IVS.PlantId, IVS.CustomerId PartyId, IVS.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode, P.Code AS Tracenent
								, P.UserName AS PartyName,REPLACE(CONVERT(CHAR(11), IVS.SalesDate, 106),' ','-') AS SalesDateNew
			                    , CP.UserName AS PartyAccountGroupName
			                    , IVS.EmployeeId, EI.EmployeeCode, EI.EmployeeName
	                           , IVS.MaterialStorageId,MS.UserName MaterialStorage,IVS.EntityId,E.UserName Entity,FORMAT(IVS.DocDate,'dd-MMM-yyyy')DocDate
								, REPLACE(CONVERT(CHAR(11), IVS.AddedDate, 106),' ','-') AS EntryDate,IVS.Remarks,IVS.DocRefNo
								, IVS.CurrencyId, CU.Code AS CurrencyCode
	                            , IVS.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy,  IVS.DeliveryPartyPlantId
								, DPP.UserName AS DeliveryBy
	                            , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState
								, CP.TaxApplicable,CP.IsPaymentTermChangeable,IVS.PaymentTermId,PT.UserName PaymentTerm
								,REPLACE(CONVERT(CHAR(11), IVS.BaseOnDueDate, 106),' ','-') BaseOnDueDate,IVS.BaseNoOfDays,REPLACE(CONVERT(CHAR(11), IVS.MatureDate, 106),' ','-') MatureDate
								,[Type]=CASE WHEN IVS.EmployeeId<>'' THEN 'Employee' Else 'Customer' END
                                ,CO.BaseCurrencyId,IVS.ToCurrencyRate
                                ,IVS.NoteForAccounts
                    FROM [TRN].[InventorySales] AS IVS LEFT JOIN [HKP].[Party] AS P ON IVS.CustomerId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable,C.IsPaymentTermChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Customer') AS CP ON CP.PartyId=IVS.CustomerId AND CP.PlantId=IVS.PlantId
                    LEFT JOIN [EmployeeInformation] AS EI ON IVS.EmployeeId=EI.SystemId
                    LEFT JOIN [SCS].[Currency] AS CU ON IVS.CurrencyId=CU.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IVS.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IVS.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                    LEFT JOIN ORG.Company AS CO ON CO.Id=IVS.CompanyId
                     LEFT JOIN (SELECT A.InventorySalesId, SUM(A.TransactionQty) AS TransactionQty, SUM(ROUND(A.AvgAmount,4)) AS TransactionAmount, SUM(ROUND(A.AvgAmount,0)) AS BaseAmount 
					 FROM [TRN].[InventorySalesDetail] AS A
		                        JOIN [TRN].[InventorySales] AS B ON A.InventorySalesId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventorySalesId) AS IRD ON IRD.InventorySalesId=IVS.Id
                    LEFT JOIN (SELECT A.InventorySalesId, A.TransactionUoMId FROM [TRN].[InventorySalesDetail] AS A JOIN [TRN].[InventorySales] AS B ON A.InventorySalesId=B.Id
		                        WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventorySalesId, A.TransactionUoMId HAVING COUNT(A.InventorySalesId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventorySalesId=IVS.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
					LEFT JOIN [HKP].[MaterialStorage] MS ON MS.Id=IVS.MaterialStorageId
					LEFT JOIN ORG.Entity E ON E.Id=IVS.EntityId
					LEFT JOIN MST.PaymentTerm PT ON PT.Id=IVS.PaymentTermId
                    WHERE IVS.PlantId='" + plantId + @"' AND ISNULL(IVS.[Status],'')!='Posting' AND IVS.CustomerId<>''
					) AS TEMP WHERE " + strkey + " order by SalesDate DESC";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> GetSalesListForInvReveivable(string plantId)
		{
			try
			{
				var sql = @"SELECT  IVS.Id, REPLACE(CONVERT(CHAR(11), IVS.SalesDate, 106),' ','-') AS SalesDate, IVS.CompanyGroupId, IVS.CompanyId, IVS.PlantId, IVS.CustomerId PartyId, IVS.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode, P.Code AS Tracenent
								, P.UserName AS PartyName,REPLACE(CONVERT(CHAR(11), IVS.SalesDate, 106),' ','-') AS SalesDateNew
			                    , CP.UserName AS PartyAccountGroupName
			                    , IVS.EmployeeId, EI.EmployeeCode, EI.EmployeeName
	                           , IVS.MaterialStorageId,MS.UserName MaterialStorage,E.UserName Entity,FORMAT(IVS.DocDate,'dd-MMM-yyyy')DocDate
								, REPLACE(CONVERT(CHAR(11), IVS.AddedDate, 106),' ','-') AS EntryDate,IVS.Remarks,IVS.DocRefNo
								, IVS.CurrencyId, CU.Code AS CurrencyCode
	                            , IVS.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy,  IVS.DeliveryPartyPlantId
								, DPP.UserName AS DeliveryBy
	                            , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState
								, CP.TaxApplicable,CP.IsPaymentTermChangeable,IVS.PaymentTermId,PT.UserName PaymentTerm
								,REPLACE(CONVERT(CHAR(11), IVS.BaseOnDueDate, 106),' ','-') BaseOnDueDate,IVS.BaseNoOfDays,REPLACE(CONVERT(CHAR(11), IVS.MatureDate, 106),' ','-') MatureDate
								,[Type]=CASE WHEN IVS.EmployeeId<>'' THEN 'Employee' Else 'Customer' END
                                ,CO.BaseCurrencyId,IVS.ToCurrencyRate
                                ,IVS.NoteForAccounts
                    FROM [TRN].[InventorySales] AS IVS LEFT JOIN [HKP].[Party] AS P ON IVS.CustomerId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable,C.IsPaymentTermChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Customer') AS CP ON CP.PartyId=IVS.CustomerId AND CP.PlantId=IVS.PlantId
                    LEFT JOIN [EmployeeInformation] AS EI ON IVS.EmployeeId=EI.SystemId
                    LEFT JOIN [SCS].[Currency] AS CU ON IVS.CurrencyId=CU.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IVS.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IVS.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                    LEFT JOIN ORG.Company AS CO ON CO.Id=IVS.CompanyId
                     LEFT JOIN (SELECT A.InventorySalesId, SUM(A.TransactionQty) AS TransactionQty, SUM(ROUND(A.AvgAmount,4)) AS TransactionAmount, SUM(ROUND(A.AvgAmount,0)) AS BaseAmount 
					 FROM [TRN].[InventorySalesDetail] AS A
		                        JOIN [TRN].[InventorySales] AS B ON A.InventorySalesId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventorySalesId) AS IRD ON IRD.InventorySalesId=IVS.Id
                    LEFT JOIN (SELECT A.InventorySalesId, A.TransactionUoMId FROM [TRN].[InventorySalesDetail] AS A JOIN [TRN].[InventorySales] AS B ON A.InventorySalesId=B.Id
		                        WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventorySalesId, A.TransactionUoMId HAVING COUNT(A.InventorySalesId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventorySalesId=IVS.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
					LEFT JOIN [HKP].[MaterialStorage] MS ON MS.Id=IVS.MaterialStorageId
					LEFT JOIN ORG.Entity E ON E.Id=IVS.EntityId
					LEFT JOIN MST.PaymentTerm PT ON PT.Id=IVS.PaymentTermId
                    WHERE IVS.PlantId='" + plantId + @"' AND ISNULL(IVS.[Status],'')<>'Posting' 
					order by IVS.SalesDate DESC";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> GetPostedSalesListForReturn(string column, string value, string plantId)
		{
			try
			{
				string strkey = "1=1";
				if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
					strkey = column + " like '%" + value + "%'";
				var sql = @"select top 300 * from (SELECT  IVS.Id, REPLACE(CONVERT(CHAR(11), IVS.SalesDate, 106),' ','-') AS SalesDate, IVS.CompanyGroupId, IVS.CompanyId, IVS.PlantId, IVS.CustomerId PartyId, IVS.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode, P.Code AS Tracenent
								, P.UserName AS PartyName,REPLACE(CONVERT(CHAR(11), IVS.SalesDate, 106),' ','-') AS SalesDateNew
			                    , CP.UserName AS PartyAccountGroupName
			                    , IVS.EmployeeId, EI.EmployeeCode, EI.EmployeeName
	                           , IVS.MaterialStorageId,MS.UserName MaterialStorage,IVS.EntityId,E.UserName Entity,FORMAT(IVS.DocDate,'dd-MMM-yyyy')DocDate
								, REPLACE(CONVERT(CHAR(11), IVS.AddedDate, 106),' ','-') AS EntryDate,IVS.Remarks,IVS.DocRefNo
								, IVS.CurrencyId, CU.Code AS CurrencyCode
	                            , IVS.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy,  IVS.DeliveryPartyPlantId
								, DPP.UserName AS DeliveryBy
	                            , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState
								, CP.TaxApplicable,CP.IsPaymentTermChangeable,IVS.PaymentTermId,PT.UserName PaymentTerm
								,REPLACE(CONVERT(CHAR(11), IVS.BaseOnDueDate, 106),' ','-') BaseOnDueDate,IVS.BaseNoOfDays,REPLACE(CONVERT(CHAR(11), IVS.MatureDate, 106),' ','-') MatureDate
								,[Type]=CASE WHEN IVS.EmployeeId<>'' THEN 'Employee' Else 'Customer' END
                                ,CO.BaseCurrencyId,IVS.ToCurrencyRate
                                ,IVS.NoteForAccounts
                    FROM [TRN].[InventorySales] AS IVS LEFT JOIN [HKP].[Party] AS P ON IVS.CustomerId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable,C.IsPaymentTermChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Customer') AS CP ON CP.PartyId=IVS.CustomerId AND CP.PlantId=IVS.PlantId
                    LEFT JOIN [EmployeeInformation] AS EI ON IVS.EmployeeId=EI.SystemId
                    LEFT JOIN [SCS].[Currency] AS CU ON IVS.CurrencyId=CU.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IVS.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IVS.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                    LEFT JOIN ORG.Company AS CO ON CO.Id=IVS.CompanyId
                     LEFT JOIN (SELECT A.InventorySalesId, SUM(A.TransactionQty) AS TransactionQty, SUM(ROUND(A.AvgAmount,4)) AS TransactionAmount, SUM(ROUND(A.AvgAmount,0)) AS BaseAmount 
					 FROM [TRN].[InventorySalesDetail] AS A
		                        JOIN [TRN].[InventorySales] AS B ON A.InventorySalesId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventorySalesId) AS IRD ON IRD.InventorySalesId=IVS.Id
                    LEFT JOIN (SELECT A.InventorySalesId, A.TransactionUoMId FROM [TRN].[InventorySalesDetail] AS A JOIN [TRN].[InventorySales] AS B ON A.InventorySalesId=B.Id
		                        WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventorySalesId, A.TransactionUoMId HAVING COUNT(A.InventorySalesId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventorySalesId=IVS.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
					LEFT JOIN [HKP].[MaterialStorage] MS ON MS.Id=IVS.MaterialStorageId
					LEFT JOIN ORG.Entity E ON E.Id=IVS.EntityId
					LEFT JOIN MST.PaymentTerm PT ON PT.Id=IVS.PaymentTermId
                    WHERE IVS.PlantId='" + plantId + @"' AND ISNULL(IVS.[Status],'')='Posting' AND IVS.VoucherId IS NOT NULL
					) AS TEMP WHERE " + strkey + " order by SalesDate DESC";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public IEnumerable<object> GetInventorySalesReturnForPost(string column, string value, string plantId)
		{
			try
			{
				string strkey = "1=1";
				if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
					strkey = column + " like '%" + value + "%'";
				var sql = @"select top 300 * from (SELECT  IVS.Id,IVS.InventorySalesId, REPLACE(CONVERT(CHAR(11), IVS.SalesDate, 106),' ','-') AS SalesDate, IVS.CompanyGroupId, IVS.CompanyId, IVS.PlantId, IVS.CustomerId PartyId, IVS.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode, P.Code AS Tracenent
								, P.UserName AS PartyName,REPLACE(CONVERT(CHAR(11), IVS.SalesDate, 106),' ','-') AS SalesDateNew
			                    , CP.UserName AS PartyAccountGroupName
	                           , IVS.MaterialStorageId,MS.UserName MaterialStorage,IVS.EntityId,E.UserName Entity,FORMAT(IVS.DocDate,'dd-MMM-yyyy')DocDate
								, REPLACE(CONVERT(CHAR(11), IVS.AddedDate, 106),' ','-') AS EntryDate,IVS.Remarks,IVS.DocRefNo
								, IVS.CurrencyId, CU.Code AS CurrencyCode
	                            , IVS.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy,  IVS.DeliveryPartyPlantId
								, DPP.UserName AS DeliveryBy
	                            , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState
								, CP.TaxApplicable,CP.IsPaymentTermChangeable,IVS.PaymentTermId,PT.UserName PaymentTerm
								,REPLACE(CONVERT(CHAR(11), IVS.BaseOnDueDate, 106),' ','-') BaseOnDueDate,IVS.BaseNoOfDays,REPLACE(CONVERT(CHAR(11), IVS.MatureDate, 106),' ','-') MatureDate
								,[Type]='Customer'
                                ,CO.BaseCurrencyId,IVS.ToCurrencyRate
                                ,IVS.NoteForAccounts
                    FROM [TRN].[InventorySalesReturn] AS IVS LEFT JOIN [HKP].[Party] AS P ON IVS.CustomerId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable,C.IsPaymentTermChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Customer') AS CP ON CP.PartyId=IVS.CustomerId AND CP.PlantId=IVS.PlantId
                    LEFT JOIN [SCS].[Currency] AS CU ON IVS.CurrencyId=CU.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IVS.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IVS.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                    LEFT JOIN ORG.Company AS CO ON CO.Id=IVS.CompanyId
                     LEFT JOIN (SELECT A.InventorySalesReturnId, SUM(A.TransactionQty) AS TransactionQty, SUM(ROUND(A.AvgAmount,4)) AS TransactionAmount, SUM(ROUND(A.AvgAmount,0)) AS BaseAmount 
					 FROM [TRN].[InventorySalesReturnDetail] AS A
		                        JOIN [TRN].[InventorySalesReturn] AS B ON A.InventorySalesReturnId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventorySalesReturnId) AS IRD ON IRD.InventorySalesReturnId=IVS.Id
                    LEFT JOIN (SELECT A.InventorySalesReturnId, A.TransactionUoMId FROM [TRN].[InventorySalesReturnDetail] AS A JOIN [TRN].[InventorySalesReturn] AS B ON A.InventorySalesReturnId=B.Id
		                        WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventorySalesReturnId, A.TransactionUoMId HAVING COUNT(A.InventorySalesReturnId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventorySalesReturnId=IVS.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
					LEFT JOIN [HKP].[MaterialStorage] MS ON MS.Id=IVS.MaterialStorageId
					LEFT JOIN ORG.Entity E ON E.Id=IVS.EntityId
					LEFT JOIN MST.PaymentTerm PT ON PT.Id=IVS.PaymentTermId
                    WHERE IVS.PlantId='" + plantId + @"' AND ISNULL(IVS.[Status],'')!='Posting'
					) AS TEMP WHERE " + strkey + " order by SalesDate DESC";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public GridModel GetReceivableMaterial(GridParameter parameters,string companyId,string plantId, string inveReveiveId)
		{
			try
			{
				parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"',@companyId varchar(10)='" + companyId + @"',@plantId varchar(10)='" + plantId + @"'
                                        , @totalReceiveAmount DECIMAL(18, 4)=0
	                                  , @totalServiceAmount DECIMAL(18, 4)=0
	                                  , @totalSvcTaxAmount DECIMAL(18, 4)=0
                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SELECT IM.Id, ISD.Id AS InventoryReceiveDetailId
                            , MGM.UserName AS MaterialGroupMasterName
                            , IM.MaterialMasterId, MM.UserName
                            , IM.ArticleId, ART.StandardName
                            , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
                            , ISD.TransactionUoMId, TUoM.UserName AS TransactionUoM
                            , ISH.SalesRate AS TransactionRate
                            , CU.Code AS CurrencyName, IVS.ToCurrencyRate
                            , ISH.Amount
                            ,ISH.Qty                         
							                  
					        ,ISD.TransactionUoMId
							,ISD.BaseUOMId 
                            ,MM.IsAsset  
							,HSNC.Code HSNCode
					  from TRN.InventoryMaterial AS IM
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						LEFT JOIN [TRN].[InventorySalesDetail] ISD ON ISD.InventoryMaterialId=IM.Id AND ISD.InventorySalesId=@inventoryReceiveId
                        JOIN [SCS].[UnitOfMeasurement] AS TUoM ON ISD.TransactionUoMId=TUoM.Id
						JOIN TRN.InventorySales IVS ON IVS.Id=ISD.InventorySalesId
                        JOIN [SCS].[Currency] AS CU ON IVS.CurrencyId=CU.Id
                        LEFT JOIN HKP.HSNCode AS HSNC ON HSNC.Id=MM.HSNCodeId
						LEFT JOIN (
								SELECT SDH.InventorySalesDetailId,SUM(SDH.Qty) Qty,sum(SDH.Qty*SD.SalesRate) Amount,SD.SalesRate SalesRate
								FROM TRN.InventorySalesDetail SD 
								JOIN TRN.InventorySalesHistory SDH ON SDH.InventorySalesDetailId=SD.Id
								LEFT JOIN TRN.InventoryReceiveDetail RD ON RD.Id=SDH.InventoryReceiveDetailId
								GROUP BY SDH.InventorySalesDetailId,SD.SalesRate
								 ) ISH ON ISH.InventorySalesDetailId=ISD.Id
                        WHERE ISD.InventorySalesId=@inventoryReceiveId";
				return _sqlRepository.GetDifferentGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
			
		}

		private Dictionary<string, object> GetCompanyPartyGroup(string partyId, string plantId)
		{
			var cmdText = @"select PartyAccountGroupId FROM HKP.CompanyParty where PartyId = '" + partyId + "' AND PlantId='" + plantId + @"' and PartyType='Customer'";
			return _sqlRepository.GetData(cmdText);
		}
		public IEnumerable<object> GetBudgetActivityInSalesMaterial(string companyId, string plantId, string inveReveiveId, string partyId)
		{
			try
			{
				var companyParty = GetCompanyPartyGroup(partyId, plantId);

				var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT  'Inventory' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId
							,ISH.PostDrGLGeneralInfoId GLGeneralInfoId , GL.AccountCode GLGeneralInfoCode ,GL.UserName GLGeneralInfoName
							,ISH.PostDrBudgetMasterId BudgetMasterId ,B.Code BudgetCode  ,B.UserName BudgetName
							,ISH.PostDrActivityId ActivityId ,A.Code ActivityCode ,A.UserName  ActivityName 
							
							, 0 Dr, SUM(ISH.TotalBaseAmount) AS Cr
							, SUM(ISH.TotalBaseAmount) AS Amount
                            --,ISD.Id InventorySalesDetailId
						FROM [TRN].[InventorySalesDetail] AS ISD
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN (
								SELECT SDH.InventorySalesDetailId,SUM(SDH.TotalBaseAmount) TotalBaseAmount,SUM(SDH.Qty) Qty
								,RD.PostDrGLGeneralInfoId ,RD.PostDrBudgetMasterId,RD.PostDrActivityId
								FROM TRN.InventorySalesDetail SD 
								JOIN TRN.InventorySalesHistory SDH ON SDH.InventorySalesDetailId=SD.Id
								LEFT JOIN TRN.InventoryReceiveDetail RD ON RD.Id=SDH.InventoryReceiveDetailId
								GROUP BY SDH.InventorySalesDetailId,RD.PostDrGLGeneralInfoId ,RD.PostDrBudgetMasterId,RD.PostDrActivityId
								 ) ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						
						LEFT JOIN[MST].[BudgetMaster] AS BM ON ISH.PostDrBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Activity] AS A ON ISH.PostDrActivityId= A.Id
						

						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY ISH.PostDrGLGeneralInfoId,ISH.PostDrBudgetMasterId,ISH.PostDrActivityId,GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName--,ISD.Id

						UNION
						
                             SELECT  'CostOfGoodsSold' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId

                            ,MGGL.ExpenseGLId GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,MGGL.ExpenseBudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,MGGL.ExpenseActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName

							, SUM(ISH.TotalBaseAmount)   AS  Dr, 0 Cr
							, SUM(ISH.TotalBaseAmount)  AS Amount
							--,ISD.Id InventorySalesDetailId
						FROM [TRN].[InventorySalesDetail] AS ISD 
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventoryReceiveDetail] AS INS ON ISH.InventoryReceiveDetailId=INS.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON ISD.TransactionUoMId=TUoM.Id
						
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Customer')AS CP ON IR.CustomerId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id AND GLType='Receivable'
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGGL.ExpenseGLId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGGL.ExpenseBudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.ExpenseActivityId= A.Id

                        LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAVGL.VendorReconGLId ,FAVGL.VendorReconBudgetMasterId,FAVGL.VendorReconActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT 
						LEFT JOIN HKP.FixedAssetMasterVendorReconGL FAVGL ON 
						FAMBT.FixedAssetMasterId=FAVGL.FixedAssetMasterId  AND FAVGL.PartyAccountGroupId=@partyAccountGruopId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId

						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.VendorReconGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.VendorReconBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.VendorReconActivityId= AF.Id

						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY  IR.Id, MGGL.ExpenseGLId, GL.AccountCode, GL.UserName, MGGL.ExpenseBudgetMasterId, B.Code, B.UserName, MGGL.ExpenseActivityId, A.Code, A.UserName
						,MM.IsAsset,FAG.VendorReconGLId,GLF.AccountCode,GLF.UserName,FAG.VendorReconBudgetMasterId,BF.Code,BF.UserName,FAG.VendorReconActivityId,AF.Code,AF.UserName--,ISD.Id
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
		public IEnumerable<object> GetInventorySalesBudgetActivityInSalesMaterial(string companyId, string plantId, string inveReveiveId, string partyId,string taxapplicable)
		{
			try
			{
				var companyParty = GetCompanyPartyGroup(partyId, plantId);
                if (taxapplicable == "Mandatory")
                {
					var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"'
				SELECT AR.OtherName,AR.TrnType,AR.MaterialGroupMasterId, AR.TaxCategoryId,AR.TaxCodeId
					,AR.GLGeneralInfoId,AR.GLGeneralInfoCode,AR.GLGeneralInfoName,AR.BudgetMasterId,AR.BudgetCode,AR.BudgetName
				    ,AR.ActivityId,AR.ActivityCode,AR.ActivityName
					,AR.Dr+ISNULL(ISS.SvcAmount,0)+ISNULL(INS.TCSAmount,0) Dr
					,AR.Cr
					,AR.Dr+ISNULL(ISS.SvcAmount,0)+ISNULL(INS.TCSAmount,0) Amount
	                FROM (
                            SELECT  'A/R' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, NULL TaxCategoryId,NULL TaxCodeId
                            ,MGPGL.GLGeneralInfoId GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,MGPGL.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,MGPGL.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName
							, SUM(ISD.TransactionQty * ISD.SalesRate)   AS  Dr, 0 Cr
							, SUM(ISD.TransactionQty * ISD.SalesRate)  AS Amount
							, ISD.InventorySalesId
						FROM [TRN].[InventorySalesDetail] AS ISD 
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						--LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON ISD.TransactionUoMId=TUoM.Id
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Customer')AS CP ON IR.CustomerId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id AND GLType='Receivable'
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id
						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY  IR.Id, MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName
						,MM.IsAsset,ISD.InventorySalesId
						) AR
						LEFT OUTER JOIN (SELECT InventorySalesId ,sum(TaxAmount) TaxAmount FROM TRN.InventorySalesTax group by InventorySalesId) IST ON IST.InventorySalesId =AR.InventorySalesId
						LEFT OUTER JOIN (SELECT InventorySalesId ,sum(Amount) SvcAmount FROM TRN.InventorySalesService group by InventorySalesId) ISS ON ISS.InventorySalesId =AR.InventorySalesId
						LEFT outer JOIN ( SELECT InventorySalesId, sum(ISNULL(TaxAmount,0)) AS TCSAmount from  [TRN].[InventorySalesAdditionalTax] group by InventorySalesId) AS INS ON  INS.InventorySalesId=AR.InventorySalesId
                    UNION
					SELECT  'Sales' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL TaxCategoryId,NULL TaxCodeId

                            ,MGPGL.GLGeneralInfoId GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,MGPGL.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,MGPGL.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName
							, 0 Dr
							, SUM(ISD.TransactionQty * ISD.SalesRate)   AS   Cr
							, SUM(ISD.TransactionQty * ISD.SalesRate)  AS Amount
						FROM [TRN].[InventorySalesDetail] AS ISD 
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						--LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON ISD.TransactionUoMId=TUoM.Id
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Customer')AS CP ON IR.CustomerId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id AND GLType='Sales'
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

						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY  IR.Id, MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName
						,MM.IsAsset,FAG.VendorReconGLId,GLF.AccountCode,GLF.UserName,FAG.VendorReconBudgetMasterId,BF.Code,BF.UserName,FAG.VendorReconActivityId,AF.Code,AF.UserName,ISD.InventorySalesId
						UNION
				        SELECT  OtherName='Tax'  
							,TrnType='Dr'  , NULL MaterialGroupMasterId, ISD.TaxCategoryId,NULL TaxCodeId
							,GLGeneralInfoId=TCL.GLGeneralInfoId
							,GLGeneralInfoCode= GL.AccountCode
							,GLGeneralInfoName= GL.UserName 
							,BudgetMasterId= TCL.BudgetMasterId 
							,BudgetCode= B.Code
							,BudgetName= B.UserName
							, ActivityId= TCL.ActivityId
							,ActivityCode=A.Code
							,ActivityName=A.UserName
							, SUM(ISD.TaxAmount) Dr, 0 Cr
							, SUM(ISD.TaxAmount) AS Amount
						FROM [TRN].[InventorySalesTax] AS ISD
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN MST.TaxCategory TC ON TC.Id=ISD.TaxCategoryId
						LEFT JOIN MST.TaxCategoryGL TCL ON TCL.TaxCategoryId=TC.Id AND InputTaxOutPutTax='Output'
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON TCL.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON TCL.BudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS B ON BMF.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON TCL.ActivityId= A.Id

						WHERE ISD.InventorySalesId=@receiveId AND ISNULL(TCL.TaxType,'')='Excluded'
						GROUP BY TCL.GLGeneralInfoId ,GL.AccountCode  ,GL.UserName  ,TCL.BudgetMasterId 
							,B.Code  ,B.UserName  ,TCL.ActivityId  ,A.Code  ,A.UserName ,ISD.TaxCategoryId

						UNION
				        SELECT  OtherName='Tax'  
							,TrnType='Cr'  , NULL MaterialGroupMasterId, ISD.TaxCategoryId,NULL TaxCodeId
							,GLGeneralInfoId=TCL.LiabilityGLId
							,GLGeneralInfoCode= GL.AccountCode
							,GLGeneralInfoName= GL.UserName 
							,BudgetMasterId= TCL.LiabilityBudgetMasterId 
							,BudgetCode= B.Code
							,BudgetName= B.UserName
							, ActivityId= TCL.LiabilityActivityId
							,ActivityCode=A.Code
							,ActivityName=A.UserName
							, 0 Dr, SUM(ISD.TaxAmount) AS Cr
							, SUM(ISD.TaxAmount) AS Amount
						FROM [TRN].[InventorySalesTax] AS ISD
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN MST.TaxCategory TC ON TC.Id=ISD.TaxCategoryId
						LEFT JOIN MST.TaxCategoryGL TCL ON TCL.TaxCategoryId=TC.Id AND InputTaxOutPutTax='Output'
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON TCL.LiabilityGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON TCL.LiabilityBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS B ON BMF.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON TCL.LiabilityActivityId= A.Id

						WHERE ISD.InventorySalesId=@receiveId AND ISNULL(TCL.TaxType,'')='Excluded'
						GROUP BY TCL.LiabilityGLId ,GL.AccountCode  ,GL.UserName  ,TCL.LiabilityBudgetMasterId 
							,B.Code  ,B.UserName  ,TCL.LiabilityActivityId  ,A.Code  ,A.UserName ,ISD.TaxCategoryId

							UNION
							SELECT  OtherName='Svc'  
							,TrnType='Cr'  , NULL MaterialGroupMasterId, NULL TaxCategoryId,NULL TaxCodeId
							,GLGeneralInfoId=SGL.ServiceGLId
							,GLGeneralInfoCode= GL.AccountCode
							,GLGeneralInfoName= GL.UserName 
							,BudgetMasterId= SGL.ServiceBudgetMasterId 
							,BudgetCode= B.Code
							,BudgetName= B.UserName
							, ActivityId= SGL.ServiceActivityId
							,ActivityCode=A.Code
							,ActivityName=A.UserName
							, 0 Dr, SUM(ISS.Amount) AS Cr
							, SUM(ISS.Amount) AS Amount
						FROM [TRN].[InventorySalesService] AS ISS
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISS.InventorySalesId=IR.Id
						LEFT JOIN HKP.ServiceMaster SM ON SM.Id=ISS.ServiceMasterId
						LEFT JOIN HKP.ServiceGroup SG ON SG.Id=SM.ServiceGroupId
						LEFT JOIN HKP.ServiceGroupGL SGL ON SGL.ServiceGroupId=SG.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON SGL.ServiceGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON SGL.ServiceBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS B ON BMF.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON SGL.ServiceActivityId= A.Id

						WHERE ISS.InventorySalesId=@receiveId
						GROUP BY SGL.ServiceGLId, GL.AccountCode, GL.UserName, SGL.ServiceBudgetMasterId 
							,B.Code, B.UserName, SGL.ServiceActivityId, A.Code, A.UserName 

							UNION
					SELECT 'TCSPayable' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId,IRT.TaxCodeId
						, TGL.WithholdCreditableGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TGL.WithholdCreditableBudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TGL.WithholdCreditableActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  NULL Dr, SUM(IRT.TaxAmount) AS Cr
						, SUM(IRT.TaxAmount) AS Amount
                        
					FROM [TRN].[InventorySalesAdditionalTax] AS IRT
                    LEFT JOIN [TRN].[InventorySales] AS IR ON IRT.InventorySalesId=IR.Id
					LEFT JOIN MST.TaxCode TCO ON TCO.Id=IRT.TaxCodeId  
					LEFT JOIN MST.TaxCodeGL TGL ON TGL.TaxCodeId=TCO.Id 
					LEFT JOIN [MST].[TaxCategory] AS TC ON IRT.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TGL.WithholdCreditableGLId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TGL.WithholdCreditableBudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TGL.WithholdCreditableActivityId= A.Id
					WHERE IRT.InventorySalesId=@receiveId AND TCO.InputOrOutput='Output' 
					GROUP BY  IRT.TaxCategoryId,IRT.TaxCodeId, TGL.WithholdCreditableGLId, GL.AccountCode, GL.UserName, TGL.WithholdCreditableBudgetMasterId, B.Code
					, B.UserName, TGL.WithholdCreditableActivityId, A.Code, A.UserName, IRT.TaxCategoryId,IRT.TaxCodeId
							 ";
					return _sqlRepository.GetDataCollection(sql);
				}
                else
                {
					var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"'
				SELECT AR.OtherName,AR.TrnType,AR.MaterialGroupMasterId, AR.TaxCategoryId,AR.TaxCodeId
					,AR.GLGeneralInfoId,AR.GLGeneralInfoCode,AR.GLGeneralInfoName,AR.BudgetMasterId,AR.BudgetCode,AR.BudgetName
				    ,AR.ActivityId,AR.ActivityCode,AR.ActivityName
					,AR.Dr+ISNULL(IST.TaxAmount,0)+ISNULL(ISS.SvcAmount,0)+ISNULL(INS.TCSAmount,0) Dr
					,AR.Cr
					,AR.Dr+ISNULL(IST.TaxAmount,0)+ISNULL(ISS.SvcAmount,0)+ISNULL(INS.TCSAmount,0) Amount
	                FROM (
                            SELECT  'A/R' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, NULL TaxCategoryId,NULL TaxCodeId
                            ,MGPGL.GLGeneralInfoId GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,MGPGL.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,MGPGL.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName
							, SUM(ISD.TransactionQty * ISD.SalesRate)   AS  Dr, 0 Cr
							, SUM(ISD.TransactionQty * ISD.SalesRate)  AS Amount
							, ISD.InventorySalesId
						FROM [TRN].[InventorySalesDetail] AS ISD 
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						--LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON ISD.TransactionUoMId=TUoM.Id
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Customer')AS CP ON IR.CustomerId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id AND GLType='Receivable'
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id
						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY  IR.Id, MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName
						,MM.IsAsset,ISD.InventorySalesId
						) AR
						LEFT OUTER JOIN (SELECT InventorySalesId ,sum(TaxAmount) TaxAmount FROM TRN.InventorySalesTax group by InventorySalesId) IST ON IST.InventorySalesId =AR.InventorySalesId
						LEFT OUTER JOIN (SELECT InventorySalesId ,sum(Amount) SvcAmount FROM TRN.InventorySalesService group by InventorySalesId) ISS ON ISS.InventorySalesId =AR.InventorySalesId
						LEFT outer JOIN ( SELECT InventorySalesId, sum(ISNULL(TaxAmount,0)) AS TCSAmount from  [TRN].[InventorySalesAdditionalTax] group by InventorySalesId) AS INS ON  INS.InventorySalesId=AR.InventorySalesId
                    UNION
					SELECT  'Sales' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL TaxCategoryId,NULL TaxCodeId

                            ,MGPGL.GLGeneralInfoId GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,MGPGL.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,MGPGL.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName
							, 0 Dr
							, SUM(ISD.TransactionQty * ISD.SalesRate)   AS   Cr
							, SUM(ISD.TransactionQty * ISD.SalesRate)  AS Amount
						FROM [TRN].[InventorySalesDetail] AS ISD 
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						--LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON ISD.TransactionUoMId=TUoM.Id
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Customer')AS CP ON IR.CustomerId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id AND GLType='Sales'
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

						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY  IR.Id, MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName
						,MM.IsAsset,FAG.VendorReconGLId,GLF.AccountCode,GLF.UserName,FAG.VendorReconBudgetMasterId,BF.Code,BF.UserName,FAG.VendorReconActivityId,AF.Code,AF.UserName,ISD.InventorySalesId
						UNION
				        SELECT  OtherName='Tax'  
							,TrnType='Cr'  , NULL MaterialGroupMasterId, ISD.TaxCategoryId,NULL TaxCodeId
							,GLGeneralInfoId=TCL.GLGeneralInfoId
							,GLGeneralInfoCode= GL.AccountCode
							,GLGeneralInfoName= GL.UserName 
							,BudgetMasterId= TCL.BudgetMasterId 
							,BudgetCode= B.Code
							,BudgetName= B.UserName
							, ActivityId= TCL.ActivityId
							,ActivityCode=A.Code
							,ActivityName=A.UserName
							, 0 Dr, SUM(ISD.TaxAmount) AS Cr
							, SUM(ISD.TaxAmount) AS Amount
						FROM [TRN].[InventorySalesTax] AS ISD
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN MST.TaxCategory TC ON TC.Id=ISD.TaxCategoryId
						LEFT JOIN MST.TaxCategoryGL TCL ON TCL.TaxCategoryId=TC.Id AND InputTaxOutPutTax='Output' AND TCL.TaxType is null
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON TCL.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON TCL.BudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS B ON BMF.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON TCL.ActivityId= A.Id

						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY TCL.GLGeneralInfoId ,GL.AccountCode  ,GL.UserName  ,TCL.BudgetMasterId 
							,B.Code  ,B.UserName  ,TCL.ActivityId  ,A.Code  ,A.UserName ,ISD.TaxCategoryId
							UNION
							SELECT  OtherName='Svc'  
							,TrnType='Cr'  , NULL MaterialGroupMasterId, NULL TaxCategoryId,NULL TaxCodeId
							,GLGeneralInfoId=SGL.ServiceGLId
							,GLGeneralInfoCode= GL.AccountCode
							,GLGeneralInfoName= GL.UserName 
							,BudgetMasterId= SGL.ServiceBudgetMasterId 
							,BudgetCode= B.Code
							,BudgetName= B.UserName
							, ActivityId= SGL.ServiceActivityId
							,ActivityCode=A.Code
							,ActivityName=A.UserName
							, 0 Dr, SUM(ISS.Amount) AS Cr
							, SUM(ISS.Amount) AS Amount
						FROM [TRN].[InventorySalesService] AS ISS
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISS.InventorySalesId=IR.Id
						LEFT JOIN HKP.ServiceMaster SM ON SM.Id=ISS.ServiceMasterId
						LEFT JOIN HKP.ServiceGroup SG ON SG.Id=SM.ServiceGroupId
						LEFT JOIN HKP.ServiceGroupGL SGL ON SGL.ServiceGroupId=SG.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON SGL.ServiceGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON SGL.ServiceBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS B ON BMF.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON SGL.ServiceActivityId= A.Id

						WHERE ISS.InventorySalesId=@receiveId
						GROUP BY SGL.ServiceGLId, GL.AccountCode, GL.UserName, SGL.ServiceBudgetMasterId 
							,B.Code, B.UserName, SGL.ServiceActivityId, A.Code, A.UserName 

							UNION
					SELECT 'TCSPayable' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId,IRT.TaxCodeId
						, TGL.WithholdCreditableGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TGL.WithholdCreditableBudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TGL.WithholdCreditableActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  NULL Dr, SUM(IRT.TaxAmount) AS Cr
						, SUM(IRT.TaxAmount) AS Amount
                        
					FROM [TRN].[InventorySalesAdditionalTax] AS IRT
                    LEFT JOIN [TRN].[InventorySales] AS IR ON IRT.InventorySalesId=IR.Id
					LEFT JOIN MST.TaxCode TCO ON TCO.Id=IRT.TaxCodeId  
					LEFT JOIN MST.TaxCodeGL TGL ON TGL.TaxCodeId=TCO.Id 
					LEFT JOIN [MST].[TaxCategory] AS TC ON IRT.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TGL.WithholdCreditableGLId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TGL.WithholdCreditableBudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TGL.WithholdCreditableActivityId= A.Id
					WHERE IRT.InventorySalesId=@receiveId AND TCO.InputOrOutput='Output' 
					GROUP BY  IRT.TaxCategoryId,IRT.TaxCodeId, TGL.WithholdCreditableGLId, GL.AccountCode, GL.UserName, TGL.WithholdCreditableBudgetMasterId, B.Code
					, B.UserName, TGL.WithholdCreditableActivityId, A.Code, A.UserName, IRT.TaxCategoryId,IRT.TaxCodeId
							 ";
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

		public IEnumerable<object> GetPackingJournal(string companyId,string plantId, string salesId)
		{
			var sql = @"DECLARE @salesId varchar(10)='" + salesId + @"',  @companyId varchar(10)='" + companyId + @"'
					
						SELECT  'PackingInventory' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId
							,GLGeneralInfoId=MGGL.ExpenseGLId
							,GLGeneralInfoCode = GL.AccountCode
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId = MGGL.ExpenseBudgetMasterId
							,BudgetCode =B.Code 
							,BudgetName = B.UserName
							,ActivityId =MGGL.ExpenseActivityId 
							,ActivityCode =A.Code 
							,ActivityName =A.UserName
							, SUM(SP.Amount) AS Dr, NULL Cr
							, SUM(SP.Amount) AS Amount
                            ,SP.Id AS  SalesPackingId
						FROM dbo.SalesPacking SP
						LEFT JOIN TRN.[Sales] AS IR ON IR.Id=SP.SalesId
						LEFT JOIN dbo.[ProductLibrary] AS IM ON SP.ProductLibraryId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.ExpenseGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.ExpenseBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.ExpenseActivityId= A.Id
						
						WHERE SP.SalesId=@salesId
						GROUP BY MM.MaterialGroupMasterId, MGGL.ExpenseGLId, GL.AccountCode, GL.UserName, MGGL.ExpenseBudgetMasterId, B.Code, B.UserName, MGGL.ExpenseActivityId, A.Code, A.UserName
					    ,SP.Id
                   union
				   SELECT  'WIPPacking' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId
							,GLGeneralInfoId=MGGL.InventoryGLId
							,GLGeneralInfoCode = GL.AccountCode
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId = MGGL.InventoryBudgetMasterId
							,BudgetCode =B.Code 
							,BudgetName = B.UserName
							,ActivityId =MGGL.InventoryActivityId 
							,ActivityCode =A.Code 
							,ActivityName =A.UserName
							,  NULL Dr, SUM(SP.Amount) AS Cr
							, SUM(SP.Amount) AS Amount
                            ,SP.Id AS  SalesPackingId
						FROM dbo.SalesPacking SP
						LEFT JOIN TRN.[Sales] AS IR ON IR.Id=SP.SalesId
						LEFT JOIN dbo.[ProductLibrary] AS IM ON SP.ProductLibraryId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
						
						WHERE SP.SalesId=@salesId
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
					    ,SP.Id
				   
				  
					ORDER BY TrnType DESC 
";
			return _sqlRepository.GetDataCollection(sql);

		}

		public IEnumerable<object> GetPackingDetail(string companyId, string plantId, string salesId)
		{
			var sql = @"SELECT S.Id,S.Id AS SalesId,SP.Id SalesMaterialId,MGM.UserName MaterialGroup
			,MM.MaterialGroupMasterId,MM.UserName Material
			,PL.MaterialMasterId,MMA.StandardName Article,PL.ArticleId,SP.Qty,SP.Amount
			FROM dbo.SalesPacking SP 
			JOIN  [TRN].[Sales] AS S ON S.Id=SP.SalesId
			LEFT JOIN dbo.ProductLibrary PL ON PL.Id=SP.ProductLibraryId
			LEFT JOIN MST.MaterialMaster MM ON MM.Id=PL.MaterialMasterId
			LEFT JOIN MST.MaterialGroupMaster MGM ON MGM.Id=MM.MaterialGroupMasterId
			LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=PL.ArticleId
			where SP.SalesId='"+ salesId + "' AND S.PlantId='"+ plantId + "'  ";
			return _sqlRepository.GetDataCollection(sql);

		}

		public GridModel GetInvSalesReturnMaterial(GridParameter parameters, string companyId, string plantId, string inveReveiveId)
		{
			try
			{
				parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"',@companyId varchar(10)='" + companyId + @"',@plantId varchar(10)='" + plantId + @"'
                                         , @totalReceiveAmount DECIMAL(18, 4)=0
	                                  , @totalServiceAmount DECIMAL(18, 4)=0
	                                  , @totalSvcTaxAmount DECIMAL(18, 4)=0
                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SELECT IM.Id, ISD.Id AS InventoryReceiveDetailId
                            , MGM.UserName AS MaterialGroupMasterName
                            , IM.MaterialMasterId, MM.UserName
                            , IM.ArticleId, ART.StandardName
                            , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
                            , ISD.TransactionUoMId, TUoM.UserName AS TransactionUoM
                            , ISD.SalesRate AS TransactionRate
                            , CU.Code AS CurrencyName, IVS.ToCurrencyRate
                            , ISNULL(ISD.TotalSalesAmount,0) Amount
                            ,ISD.TransactionQty                         
							                  
					        ,ISD.TransactionUoMId
							,ISD.BaseUOMId 
                            ,MM.IsAsset  
							,HSNC.Code HSNCode
					  from TRN.InventoryMaterial AS IM
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						LEFT JOIN [TRN].[InventorySalesReturnDetail] ISD ON ISD.InventoryMaterialId=IM.Id AND ISD.InventorySalesReturnId=@inventoryReceiveId
                        JOIN [SCS].[UnitOfMeasurement] AS TUoM ON ISD.TransactionUoMId=TUoM.Id
						JOIN TRN.InventorySalesReturn IVS ON IVS.Id=ISD.InventorySalesReturnId
                        JOIN [SCS].[Currency] AS CU ON IVS.CurrencyId=CU.Id
                        LEFT JOIN HKP.HSNCode AS HSNC ON HSNC.Id=MM.HSNCodeId";
				return _sqlRepository.GetDifferentGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}

		}
		private Dictionary<string, object> GetCustomerCompanyPartyGroup(string partyId, string plantId)
		{
			var cmdText = @"select PartyAccountGroupId FROM HKP.CompanyParty where PartyId = '" + partyId + "' AND PlantId='" + plantId + @"' and PartyType='Customer'";
			return _sqlRepository.GetData(cmdText);
		}
		public IEnumerable<object> GetInventorySaleDetailGLListData(string companyId, string plantId, string inveReveiveId, string partyId)
		{
			try
			{
				var companyParty = GetCustomerCompanyPartyGroup(partyId, plantId);
				var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"'
					SELECT  X.* FROM (
					SELECT  'Inventory' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId
							,ISH.PostDrGLGeneralInfoId GLGeneralInfoId , GL.AccountCode GLGeneralInfoCode ,GL.UserName GLGeneralInfoName
							,ISH.PostDrBudgetMasterId BudgetMasterId ,B.Code BudgetCode  ,B.UserName BudgetName
							,ISH.PostDrActivityId ActivityId ,A.Code ActivityCode ,A.UserName  ActivityName 
							
							, 0 Dr, SUM(ISH.GRNRate * ISH.Qty) AS Cr
							, SUM(ISH.GRNRate * ISH.Qty) AS Amount
                            ,ISD.Id InventorySalesDetailId
						FROM [TRN].[InventorySalesDetail] AS ISD
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN (
								SELECT SDH.InventorySalesDetailId,SUM(RD.MaterialTranRate) GRNRate,SUM(SDH.Qty) Qty
								,RD.PostDrGLGeneralInfoId ,RD.PostDrBudgetMasterId,RD.PostDrActivityId
								FROM TRN.InventorySalesDetail SD 
								JOIN TRN.InventorySalesHistory SDH ON SDH.InventorySalesDetailId=SD.Id
								LEFT JOIN TRN.InventoryReceiveDetail RD ON RD.Id=SDH.InventoryReceiveDetailId
								GROUP BY SDH.InventorySalesDetailId,RD.PostDrGLGeneralInfoId ,RD.PostDrBudgetMasterId,RD.PostDrActivityId
								 ) ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						
						LEFT JOIN[MST].[BudgetMaster] AS BM ON ISH.PostDrBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Activity] AS A ON ISH.PostDrActivityId= A.Id
						

						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY ISH.PostDrGLGeneralInfoId,ISH.PostDrBudgetMasterId,ISH.PostDrActivityId,GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName,ISD.Id
						,ISD.Id 
						UNION
                            SELECT  'A/R' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId

                            ,MGPGL.GLGeneralInfoId GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,MGPGL.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,MGPGL.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName

							, SUM(ISD.SalesRate*ISH.Qty)   AS  Dr, 0 Cr
							, SUM(ISD.SalesRate*ISH.Qty)  AS Amount
							,ISD.Id InventorySalesDetailId
						FROM [TRN].[InventorySalesDetail] AS ISD 
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=ISD.Id
						--JOIN [TRN].[InventoryService] AS INS ON ISD.InventoryReceiveId=INS.InventoryReceiveId
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON ISD.TransactionUoMId=TUoM.Id
						
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Customer')AS CP ON IR.CustomerId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id AND GLType='Receivable'
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

						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY  IR.Id, MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName
						,MM.IsAsset,FAG.VendorReconGLId,GLF.AccountCode,GLF.UserName,FAG.VendorReconBudgetMasterId,BF.Code,BF.UserName,FAG.VendorReconActivityId,AF.Code,AF.UserName
						,ISD.Id 
							)X
							WHERE X.Amount>0";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public IEnumerable<object> GetBudgetActivityInSalesReturnMaterial(string companyId, string plantId, string inveReveiveId, string partyId)
		{
			try
			{
				var companyParty = GetCompanyPartyGroup(partyId, plantId);

				var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT  'Inventory' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId
							,ISD.PostCrInventoryGLId GLGeneralInfoId , GL.AccountCode GLGeneralInfoCode ,GL.UserName GLGeneralInfoName
							,ISD.PostCrInventoryBudgetMasterId BudgetMasterId ,B.Code BudgetCode  ,B.UserName BudgetName
							,ISD.PostCrInventoryActivityId ActivityId ,A.Code ActivityCode ,A.UserName  ActivityName 
							
							, SUM(IRD.TotalMaterialTranAmount) Dr, 0 AS Cr
							, SUM(IRD.TotalMaterialTranAmount) AS Amount
						FROM [TRN].[InventorySalesReturnDetail] ISRD
						LEFT JOIN [TRN].[InventorySalesDetail] AS ISD ON ISRD.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventorySalesReturn] AS ISR ON ISRD.InventorySalesReturnId=ISR.Id
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=ISD.PostCrInventoryGLId
						LEFT JOIN [MST].[BudgetMaster] AS BM ON ISD.PostCrInventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON ISD.PostCrInventoryActivityId= A.Id
						LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=ISRD.InventoryReceiveDetailId

						WHERE ISR.Id=@receiveId
						GROUP BY ISD.PostCrInventoryGLId,ISD.PostCrInventoryBudgetMasterId,ISD.PostCrInventoryActivityId,GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName--,ISD.Id

						UNION ALL
						
                             SELECT  'CostOfGoodsSold' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId

                            ,ISD.PostDrInventoryGLId GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,ISD.PostDrInventoryBudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,ISD.PostDrInventoryActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName

							, 0 Dr, SUM(IRD.TotalMaterialTranAmount) AS Cr
							, SUM(IRD.TotalMaterialTranAmount) AS Amount
						FROM [TRN].[InventorySalesReturnDetail] ISRD
						LEFT JOIN [TRN].[InventorySalesDetail] AS ISD ON ISRD.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventorySalesReturn] AS ISR ON ISRD.InventorySalesReturnId=ISR.Id
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=ISD.PostDrInventoryGLId
						LEFT JOIN [MST].[BudgetMaster] AS BM ON ISD.PostDrInventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON ISD.PostDrInventoryActivityId= A.Id
						LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=ISRD.InventoryReceiveDetailId
						WHERE ISR.Id=@receiveId
						GROUP BY ISD.PostDrInventoryGLId,ISD.PostDrInventoryBudgetMasterId,ISD.PostDrInventoryActivityId,GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName--,ISD.Id

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
		public IEnumerable<object> GetInventorySalesReturnInventorySalesBook(string companyId, string plantId, string inveReveiveId, string partyId, string taxapplicable)
		{
			try
			{
				var companyParty = GetCompanyPartyGroup(partyId, plantId);
				if (taxapplicable == "Mandatory")
				{
					var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"'
				SELECT AR.OtherName,AR.TrnType,AR.MaterialGroupMasterId, AR.TaxCategoryId,AR.TaxCodeId
					,AR.GLGeneralInfoId,AR.GLGeneralInfoCode,AR.GLGeneralInfoName,AR.BudgetMasterId,AR.BudgetCode,AR.BudgetName
				    ,AR.ActivityId,AR.ActivityCode,AR.ActivityName
					,AR.Dr+ISNULL(ISS.SvcAmount,0)+ISNULL(INS.TCSAmount,0) Dr
					,AR.Cr
					,AR.Dr+ISNULL(ISS.SvcAmount,0)+ISNULL(INS.TCSAmount,0) Amount
	                FROM (
                            SELECT  'A/R' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, NULL TaxCategoryId,NULL TaxCodeId
                            ,MGPGL.GLGeneralInfoId GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,MGPGL.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,MGPGL.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName
							, SUM(ISD.TransactionQty * ISD.SalesRate)   AS  Dr, 0 Cr
							, SUM(ISD.TransactionQty * ISD.SalesRate)  AS Amount
							, ISD.InventorySalesId
						FROM [TRN].[InventorySalesDetail] AS ISD 
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						--LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON ISD.TransactionUoMId=TUoM.Id
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Customer')AS CP ON IR.CustomerId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id AND GLType='Receivable'
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id
						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY  IR.Id, MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName
						,MM.IsAsset,ISD.InventorySalesId
						) AR
						LEFT OUTER JOIN (SELECT InventorySalesId ,sum(TaxAmount) TaxAmount FROM TRN.InventorySalesTax group by InventorySalesId) IST ON IST.InventorySalesId =AR.InventorySalesId
						LEFT OUTER JOIN (SELECT InventorySalesId ,sum(Amount) SvcAmount FROM TRN.InventorySalesService group by InventorySalesId) ISS ON ISS.InventorySalesId =AR.InventorySalesId
						LEFT outer JOIN ( SELECT InventorySalesId, sum(ISNULL(TaxAmount,0)) AS TCSAmount from  [TRN].[InventorySalesAdditionalTax] group by InventorySalesId) AS INS ON  INS.InventorySalesId=AR.InventorySalesId
                    UNION
					SELECT  'Sales' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL TaxCategoryId,NULL TaxCodeId

                            ,MGPGL.GLGeneralInfoId GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,MGPGL.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,MGPGL.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName
							, 0 Dr
							, SUM(ISD.TransactionQty * ISD.SalesRate)   AS   Cr
							, SUM(ISD.TransactionQty * ISD.SalesRate)  AS Amount
						FROM [TRN].[InventorySalesDetail] AS ISD 
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						--LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON ISD.TransactionUoMId=TUoM.Id
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Customer')AS CP ON IR.CustomerId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id AND GLType='Sales'
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

						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY  IR.Id, MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName
						,MM.IsAsset,FAG.VendorReconGLId,GLF.AccountCode,GLF.UserName,FAG.VendorReconBudgetMasterId,BF.Code,BF.UserName,FAG.VendorReconActivityId,AF.Code,AF.UserName,ISD.InventorySalesId
						UNION
				        SELECT  OtherName='Tax'  
							,TrnType='Dr'  , NULL MaterialGroupMasterId, ISD.TaxCategoryId,NULL TaxCodeId
							,GLGeneralInfoId=TCL.GLGeneralInfoId
							,GLGeneralInfoCode= GL.AccountCode
							,GLGeneralInfoName= GL.UserName 
							,BudgetMasterId= TCL.BudgetMasterId 
							,BudgetCode= B.Code
							,BudgetName= B.UserName
							, ActivityId= TCL.ActivityId
							,ActivityCode=A.Code
							,ActivityName=A.UserName
							, SUM(ISD.TaxAmount) Dr, 0 Cr
							, SUM(ISD.TaxAmount) AS Amount
						FROM [TRN].[InventorySalesTax] AS ISD
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN MST.TaxCategory TC ON TC.Id=ISD.TaxCategoryId
						LEFT JOIN MST.TaxCategoryGL TCL ON TCL.TaxCategoryId=TC.Id AND InputTaxOutPutTax='Output'
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON TCL.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON TCL.BudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS B ON BMF.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON TCL.ActivityId= A.Id

						WHERE ISD.InventorySalesId=@receiveId AND ISNULL(TCL.TaxType,'')='Excluded'
						GROUP BY TCL.GLGeneralInfoId ,GL.AccountCode  ,GL.UserName  ,TCL.BudgetMasterId 
							,B.Code  ,B.UserName  ,TCL.ActivityId  ,A.Code  ,A.UserName ,ISD.TaxCategoryId

						UNION
				        SELECT  OtherName='Tax'  
							,TrnType='Cr'  , NULL MaterialGroupMasterId, ISD.TaxCategoryId,NULL TaxCodeId
							,GLGeneralInfoId=TCL.LiabilityGLId
							,GLGeneralInfoCode= GL.AccountCode
							,GLGeneralInfoName= GL.UserName 
							,BudgetMasterId= TCL.LiabilityBudgetMasterId 
							,BudgetCode= B.Code
							,BudgetName= B.UserName
							, ActivityId= TCL.LiabilityActivityId
							,ActivityCode=A.Code
							,ActivityName=A.UserName
							, 0 Dr, SUM(ISD.TaxAmount) AS Cr
							, SUM(ISD.TaxAmount) AS Amount
						FROM [TRN].[InventorySalesTax] AS ISD
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN MST.TaxCategory TC ON TC.Id=ISD.TaxCategoryId
						LEFT JOIN MST.TaxCategoryGL TCL ON TCL.TaxCategoryId=TC.Id AND InputTaxOutPutTax='Output'
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON TCL.LiabilityGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON TCL.LiabilityBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS B ON BMF.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON TCL.LiabilityActivityId= A.Id

						WHERE ISD.InventorySalesId=@receiveId AND ISNULL(TCL.TaxType,'')='Excluded'
						GROUP BY TCL.LiabilityGLId ,GL.AccountCode  ,GL.UserName  ,TCL.LiabilityBudgetMasterId 
							,B.Code  ,B.UserName  ,TCL.LiabilityActivityId  ,A.Code  ,A.UserName ,ISD.TaxCategoryId

							UNION
							SELECT  OtherName='Svc'  
							,TrnType='Cr'  , NULL MaterialGroupMasterId, NULL TaxCategoryId,NULL TaxCodeId
							,GLGeneralInfoId=SGL.ServiceGLId
							,GLGeneralInfoCode= GL.AccountCode
							,GLGeneralInfoName= GL.UserName 
							,BudgetMasterId= SGL.ServiceBudgetMasterId 
							,BudgetCode= B.Code
							,BudgetName= B.UserName
							, ActivityId= SGL.ServiceActivityId
							,ActivityCode=A.Code
							,ActivityName=A.UserName
							, 0 Dr, SUM(ISS.Amount) AS Cr
							, SUM(ISS.Amount) AS Amount
						FROM [TRN].[InventorySalesService] AS ISS
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISS.InventorySalesId=IR.Id
						LEFT JOIN HKP.ServiceMaster SM ON SM.Id=ISS.ServiceMasterId
						LEFT JOIN HKP.ServiceGroup SG ON SG.Id=SM.ServiceGroupId
						LEFT JOIN HKP.ServiceGroupGL SGL ON SGL.ServiceGroupId=SG.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON SGL.ServiceGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON SGL.ServiceBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS B ON BMF.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON SGL.ServiceActivityId= A.Id

						WHERE ISS.InventorySalesId=@receiveId
						GROUP BY SGL.ServiceGLId, GL.AccountCode, GL.UserName, SGL.ServiceBudgetMasterId 
							,B.Code, B.UserName, SGL.ServiceActivityId, A.Code, A.UserName 

							UNION
					SELECT 'TCSPayable' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId,IRT.TaxCodeId
						, TGL.WithholdCreditableGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TGL.WithholdCreditableBudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TGL.WithholdCreditableActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  NULL Dr, SUM(IRT.TaxAmount) AS Cr
						, SUM(IRT.TaxAmount) AS Amount
                        
					FROM [TRN].[InventorySalesAdditionalTax] AS IRT
                    LEFT JOIN [TRN].[InventorySales] AS IR ON IRT.InventorySalesId=IR.Id
					LEFT JOIN MST.TaxCode TCO ON TCO.Id=IRT.TaxCodeId  
					LEFT JOIN MST.TaxCodeGL TGL ON TGL.TaxCodeId=TCO.Id 
					LEFT JOIN [MST].[TaxCategory] AS TC ON IRT.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TGL.WithholdCreditableGLId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TGL.WithholdCreditableBudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TGL.WithholdCreditableActivityId= A.Id
					WHERE IRT.InventorySalesId=@receiveId AND TCO.InputOrOutput='Output' 
					GROUP BY  IRT.TaxCategoryId,IRT.TaxCodeId, TGL.WithholdCreditableGLId, GL.AccountCode, GL.UserName, TGL.WithholdCreditableBudgetMasterId, B.Code
					, B.UserName, TGL.WithholdCreditableActivityId, A.Code, A.UserName, IRT.TaxCategoryId,IRT.TaxCodeId
							 ";
					return _sqlRepository.GetDataCollection(sql);
				}
				else
				{
					var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"'
				SELECT AR.OtherName,AR.TrnType,AR.MaterialGroupMasterId, AR.TaxCategoryId,AR.TaxCodeId
					,AR.GLGeneralInfoId,AR.GLGeneralInfoCode,AR.GLGeneralInfoName,AR.BudgetMasterId,AR.BudgetCode,AR.BudgetName
				    ,AR.ActivityId,AR.ActivityCode,AR.ActivityName
					,AR.Dr Dr
					,AR.Cr
					,AR.Amount  Amount
	                FROM (
                            SELECT  'A/R' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL TaxCategoryId,NULL TaxCodeId
                            ,ISD.PostDrGLGeneralInfoId GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,ISD.PostDrBudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,ISD.PostDrActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName
							, 0 Dr
							, SUM(ISRD.TotalSalesAmount)   AS  Cr
							, SUM(ISRD.TotalSalesAmount)  AS Amount
							, ISD.InventorySalesId

						FROM [TRN].[InventorySalesReturnDetail] AS ISRD 
						LEFT JOIN [TRN].[InventorySalesDetail] AS ISD ON  ISRD.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN [TRN].[InventorySalesReturn] AS ISR ON ISR.InventorySalesId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON ISD.TransactionUoMId=TUoM.Id
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ISD.PostDrGLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON ISD.PostDrBudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON ISD.PostDrActivityId= A.Id
						WHERE ISR.Id=@receiveId
						GROUP BY  IR.Id, ISD.PostDrGLGeneralInfoId, GL.AccountCode, GL.UserName, ISD.PostDrBudgetMasterId, B.Code, B.UserName, ISD.PostDrActivityId, A.Code, A.UserName
						,MM.IsAsset,ISD.InventorySalesId
						) AR

                    UNION
					SELECT  'Sales' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, NULL TaxCategoryId,NULL TaxCodeId

                            ,ISD.PostCRGLGeneralInfoId GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,ISD.PostCRBudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,ISD.PostCRActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName
							, SUM(ISRD.TotalSalesAmount)   AS   Dr
							, 0 Cr
							, SUM(ISRD.TotalSalesAmount)  AS Amount
						FROM TRN.InventorySalesReturnDetail AS ISRD 
						LEFT JOIN [TRN].[InventorySalesDetail] AS ISD ON ISRD.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN [TRN].[InventorySalesReturn] AS ISR ON ISR.InventorySalesId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON ISD.TransactionUoMId=TUoM.Id
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ISD.PostCRGLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON ISD.PostCRBudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON ISD.PostCRActivityId= A.Id
						
						WHERE ISR.Id=@receiveId
						GROUP BY  IR.Id, ISD.PostCRGLGeneralInfoId, GL.AccountCode, GL.UserName, ISD.PostCRBudgetMasterId, B.Code, B.UserName, ISD.PostCRActivityId, A.Code, A.UserName
						,MM.IsAsset,ISD.InventorySalesId
						UNION
				        SELECT  OtherName='Tax'  
							,TrnType='Cr'  , NULL MaterialGroupMasterId, ISD.TaxCategoryId,NULL TaxCodeId
							,GLGeneralInfoId=TCL.GLGeneralInfoId
							,GLGeneralInfoCode= GL.AccountCode
							,GLGeneralInfoName= GL.UserName 
							,BudgetMasterId= TCL.BudgetMasterId 
							,BudgetCode= B.Code
							,BudgetName= B.UserName
							, ActivityId= TCL.ActivityId
							,ActivityCode=A.Code
							,ActivityName=A.UserName
							, 0 Dr, SUM(ISD.TaxAmount) AS Cr
							, SUM(ISD.TaxAmount) AS Amount
						FROM [TRN].[InventorySalesTax] AS ISD
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN MST.TaxCategory TC ON TC.Id=ISD.TaxCategoryId
						LEFT JOIN MST.TaxCategoryGL TCL ON TCL.TaxCategoryId=TC.Id AND InputTaxOutPutTax='Output' AND TCL.TaxType is null
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON TCL.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON TCL.BudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS B ON BMF.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON TCL.ActivityId= A.Id

						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY TCL.GLGeneralInfoId ,GL.AccountCode  ,GL.UserName  ,TCL.BudgetMasterId 
							,B.Code  ,B.UserName  ,TCL.ActivityId  ,A.Code  ,A.UserName ,ISD.TaxCategoryId
							UNION
							SELECT  OtherName='Svc'  
							,TrnType='Cr'  , NULL MaterialGroupMasterId, NULL TaxCategoryId,NULL TaxCodeId
							,GLGeneralInfoId=SGL.ServiceGLId
							,GLGeneralInfoCode= GL.AccountCode
							,GLGeneralInfoName= GL.UserName 
							,BudgetMasterId= SGL.ServiceBudgetMasterId 
							,BudgetCode= B.Code
							,BudgetName= B.UserName
							, ActivityId= SGL.ServiceActivityId
							,ActivityCode=A.Code
							,ActivityName=A.UserName
							, 0 Dr, SUM(ISS.Amount) AS Cr
							, SUM(ISS.Amount) AS Amount
						FROM [TRN].[InventorySalesService] AS ISS
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISS.InventorySalesId=IR.Id
						LEFT JOIN HKP.ServiceMaster SM ON SM.Id=ISS.ServiceMasterId
						LEFT JOIN HKP.ServiceGroup SG ON SG.Id=SM.ServiceGroupId
						LEFT JOIN HKP.ServiceGroupGL SGL ON SGL.ServiceGroupId=SG.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON SGL.ServiceGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON SGL.ServiceBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS B ON BMF.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON SGL.ServiceActivityId= A.Id

						WHERE ISS.InventorySalesId=@receiveId
						GROUP BY SGL.ServiceGLId, GL.AccountCode, GL.UserName, SGL.ServiceBudgetMasterId 
							,B.Code, B.UserName, SGL.ServiceActivityId, A.Code, A.UserName 

							UNION
					SELECT 'TCSPayable' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId,IRT.TaxCodeId
						, TGL.WithholdCreditableGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TGL.WithholdCreditableBudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TGL.WithholdCreditableActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  NULL Dr, SUM(IRT.TaxAmount) AS Cr
						, SUM(IRT.TaxAmount) AS Amount
                        
					FROM [TRN].[InventorySalesAdditionalTax] AS IRT
                    LEFT JOIN [TRN].[InventorySales] AS IR ON IRT.InventorySalesId=IR.Id
					LEFT JOIN MST.TaxCode TCO ON TCO.Id=IRT.TaxCodeId  
					LEFT JOIN MST.TaxCodeGL TGL ON TGL.TaxCodeId=TCO.Id 
					LEFT JOIN [MST].[TaxCategory] AS TC ON IRT.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TGL.WithholdCreditableGLId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TGL.WithholdCreditableBudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TGL.WithholdCreditableActivityId= A.Id
					WHERE IRT.InventorySalesId=@receiveId AND TCO.InputOrOutput='Output' 
					GROUP BY  IRT.TaxCategoryId,IRT.TaxCodeId, TGL.WithholdCreditableGLId, GL.AccountCode, GL.UserName, TGL.WithholdCreditableBudgetMasterId, B.Code
					, B.UserName, TGL.WithholdCreditableActivityId, A.Code, A.UserName, IRT.TaxCategoryId,IRT.TaxCodeId
							 ";
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
		private Dictionary<string, object> GetCompanyParty(string partyId, string plantId)
		{
			var cmdText = @"select PartyAccountGroupId from hkp.CompanyParty where PartyId='" + partyId + "' AND Plantid='" + plantId + "'";
			return _sqlRepository.GetData(cmdText);
		}
		public IEnumerable<object> GetInventorySalesReturnMaterialReceivable(string companyId, string plantId, string inveReveiveId, string partyId, string taxapplicable)
		{
			try
			{
				var companyParty = GetCompanyParty(partyId, plantId);
				var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT  'Inventory' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId
							,ISH.PostDrGLGeneralInfoId GLGeneralInfoId , GL.AccountCode GLGeneralInfoCode ,GL.UserName GLGeneralInfoName
							,ISH.PostDrBudgetMasterId BudgetMasterId ,B.Code BudgetCode  ,B.UserName BudgetName
							,ISH.PostDrActivityId ActivityId ,A.Code ActivityCode ,A.UserName  ActivityName 
							
							, 0 Dr, SUM(ISH.GRNRate * ISH.Qty) AS Cr
							, SUM(ISH.GRNRate * ISH.Qty) AS Amount
                            
						FROM [TRN].[InventorySalesDetail] AS ISD
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN (
								SELECT SDH.InventorySalesDetailId,SUM(RD.MaterialTranRate) GRNRate,SUM(SDH.Qty) Qty
								,RD.PostDrGLGeneralInfoId ,RD.PostDrBudgetMasterId,RD.PostDrActivityId
								FROM TRN.InventorySalesDetail SD 
								JOIN TRN.InventorySalesHistory SDH ON SDH.InventorySalesDetailId=SD.Id
								LEFT JOIN TRN.InventoryReceiveDetail RD ON RD.Id=SDH.InventoryReceiveDetailId
								GROUP BY SDH.InventorySalesDetailId,RD.PostDrGLGeneralInfoId ,RD.PostDrBudgetMasterId,RD.PostDrActivityId
								 ) ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						
						LEFT JOIN[MST].[BudgetMaster] AS BM ON ISH.PostDrBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Activity] AS A ON ISH.PostDrActivityId= A.Id					

						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY ISH.PostDrGLGeneralInfoId,ISH.PostDrBudgetMasterId,ISH.PostDrActivityId,GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName,ISD.Id

						UNION		
						
						
                            SELECT  'A/R' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId

                            ,MGPGL.GLGeneralInfoId GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,MGPGL.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,MGPGL.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName

							, SUM(ISD.SalesRate*ISH.Qty)   AS  Dr, 0 Cr
							, SUM(ISD.SalesRate*ISH.Qty)  AS Amount
						FROM [TRN].[InventorySalesDetail] AS ISD 
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=ISD.Id
						--JOIN [TRN].[InventoryService] AS INS ON ISD.InventoryReceiveId=INS.InventoryReceiveId
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON ISD.TransactionUoMId=TUoM.Id
						
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Customer')AS CP ON IR.CustomerId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id AND GLType='Receivable'
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

						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY  IR.Id, MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName
						,MM.IsAsset,FAG.VendorReconGLId,GLF.AccountCode,GLF.UserName,FAG.VendorReconBudgetMasterId,BF.Code,BF.UserName,FAG.VendorReconActivityId,AF.Code,AF.UserName
                    

						union
                        SELECT  OtherName=case when (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 then 'Gain on Sales'  
											when (SUM(ird.MaterialTranRate)-SUM(ISD.SalesRate))*SUM(ISH.Qty)>0 then 'Loss on Sales'
											  end
							,TrnType=case when (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 then 'Cr'  
											when (SUM(ird.MaterialTranRate)-SUM(ISD.SalesRate))*SUM(ISH.Qty)>0 then 'Dr'
											  end
											  , NULL MaterialGroupMasterId
							,GLGeneralInfoId=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN GAD.GLGeneralInfoId ELSE GADL.GLGeneralInfoId END
							,GLGeneralInfoCode=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN GL.AccountCode  ELSE GLL.AccountCode END
							,GLGeneralInfoName=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN GL.UserName   ELSE GLL.UserName  END
							,BudgetMasterId=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN GAD.BudgetMasterId   ELSE GADL.BudgetMasterId  END
							,BudgetCode=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN B.Code   ELSE BL.Code  END
							,BudgetName=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN B.UserName   ELSE BL.UserName  END
							, ActivityId=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN GAD.ActivityId  ELSE GADL.ActivityId END
							,ActivityCode=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN A.Code   ELSE AL.Code  END
							,ActivityName=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN A.UserName   ELSE AL.UserName  END
							
							, Dr=CASE WHEN (SUM(ird.MaterialTranRate)-SUM(ISD.SalesRate))*SUM(ISH.Qty)>0 THEN (SUM(ird.MaterialTranRate)-SUM(ISD.SalesRate))*SUM(ISH.Qty) ELSE 0 END
							, Cr=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty) ELSE 0 END
							, Amount=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty) ELSE (SUM(ird.MaterialTranRate)-SUM(ISD.SalesRate))*SUM(ISH.Qty) END
						FROM [TRN].[InventorySalesDetail] AS ISD
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].InventoryReceiveDetail IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN ORG.Company C ON C.Id=IR.CompanyId
						LEFT JOIN [HKP].[GeneralAccountDeterminate] GAD ON C.COAId=GAD.COAId and GAD.Id='GainOnInventorySales'
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON GAD.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON GAD.BudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS B ON BMF.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON GAD.ActivityId= A.Id

						LEFT JOIN [HKP].[GeneralAccountDeterminate] GADL ON GADL.COAId=C.COAId and GADL.Id='LossOnInventorySales'
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLL ON GADL.GLGeneralInfoId=GLL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMFL ON GADL.BudgetMasterId= BMFL.Id
						LEFT JOIN [HKP].[Budget] AS BL ON BMFL.BudgetId= BL.Id
						LEFT JOIN [HKP].[Activity] AS AL ON GADL.ActivityId= AL.Id

						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY ISD.Id,ird.Id,GAD.GLGeneralInfoId ,GL.AccountCode  ,GL.UserName  ,GAD.BudgetMasterId 
							,B.Code  ,B.UserName  ,GAD.ActivityId  ,A.Code  ,A.UserName ,GADL.GLGeneralInfoId 
							,GLL.AccountCode  ,GLL.UserName  ,GADL.BudgetMasterId  ,BL.Code  ,BL.UserName  ,GADL.ActivityId 
							,AL.Code  ,AL.UserName";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public IEnumerable<object> GetPackingSalesListForReturn(string column, string value, string plantId)
		{
			try
			{
				string strkey = "1=1";
				if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
					strkey = column + " like '%" + value + "%'";
				var sql = @"select top 300 * from (SELECT  IVS.Id, REPLACE(CONVERT(CHAR(11), IVS.InvoiceDate, 106),' ','-') AS SalesDate, IVS.CompanyGroupId, IVS.CompanyId, IVS.PlantId, IVS.PartyId CustomerId , IVS.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode, P.Code AS Tracenent
								, P.UserName AS PartyName,REPLACE(CONVERT(CHAR(11), IVS.InvoiceDate, 106),' ','-') AS SalesDateNew
			                    , CP.UserName AS PartyAccountGroupName
			                    
	                            ,IVS.EntityId,E.UserName Entity,FORMAT(IVS.InvoiceDate,'dd-MMM-yyyy')DocDate
								, REPLACE(CONVERT(CHAR(11), IVS.AddedDate, 106),' ','-') AS EntryDate,IVS.Narration,IVS.DocRefNo
								, IVS.CurrencyId, CU.Code AS CurrencyCode
	                            , IVS.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy,  IVS.DeliveryPartyPlantId
								, DPP.UserName AS DeliveryBy
	                            , ISC.ReturnNetWeight TransactionQty,ISC.PackingId
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState
								, CP.TaxApplicable,CP.IsPaymentTermChangeable,IVS.PaymentTermId,PT.UserName PaymentTerm
								,REPLACE(CONVERT(CHAR(11), IVS.BaseOnDueDate, 106),' ','-') BaseOnDueDate,IVS.BaseNoOfDays,REPLACE(CONVERT(CHAR(11), IVS.MatureDate, 106),' ','-') MatureDate
								,'Customer' [Type]
                                ,CO.BaseCurrencyId,IVS.ToCurrencyRate
                                ,'' NoteForAccounts,IVS.SourceType
                    FROM [TRN].[Sales] AS IVS 
					LEFT JOIN [HKP].[Party] AS P ON IVS.PartyId=P.Id
					 JOIN (SELECT isc.SalesId,pli.PackingId,sum(ReturnNetWeight) ReturnNetWeight FROM ItemScanChild isc 
					left join [TRN].[POLotReference] plr on plr.Id=isc.packingId
					left join [TRN].PackingLineItem pli on pli.PackingLineItemId=plr.PackingLineItemId
					where  isc.returnnetweight<>0 and isc.SalesReturnId is null 
					group by isc.SalesId,pli.PackingId) ISC ON ISC.SalesId=IVS.Id

                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable,C.IsPaymentTermChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Customer') AS CP ON CP.PartyId=IVS.PartyId AND CP.PlantId=IVS.PlantId
                    LEFT JOIN [SCS].[Currency] AS CU ON IVS.CurrencyId=CU.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IVS.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IVS.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                    LEFT JOIN ORG.Company AS CO ON CO.Id=IVS.CompanyId
					LEFT JOIN ORG.Entity E ON E.Id=IVS.EntityId
					LEFT JOIN MST.PaymentTerm PT ON PT.Id=IVS.PaymentTermId
                    WHERE IVS.PlantId='"+plantId+@"'  AND IVS.VoucherId IS NOT NULL
					) AS TEMP WHERE " + strkey + " order by SalesDate DESC ";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> GetSalesListForReturn(string column, string value, string plantId)
		{
			try
			{
				string strkey = "1=1";
				if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
					strkey = column + " like '%" + value + "%'";
				var sql = @"select top 300 * from (SELECT  IVS.Id, REPLACE(CONVERT(CHAR(11), IVS.InvoiceDate, 106),' ','-') AS SalesDate, IVS.CompanyGroupId, IVS.CompanyId, IVS.PlantId, IVS.PartyId CustomerId , IVS.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode, P.Code AS Tracenent
								, P.UserName AS PartyName,REPLACE(CONVERT(CHAR(11), IVS.InvoiceDate, 106),' ','-') AS SalesDateNew
			                    , CP.UserName AS PartyAccountGroupName
			                    
	                            ,IVS.EntityId,E.UserName Entity,FORMAT(IVS.InvoiceDate,'dd-MMM-yyyy')DocDate
								, REPLACE(CONVERT(CHAR(11), IVS.AddedDate, 106),' ','-') AS EntryDate,IVS.Narration,IVS.DocRefNo
								, IVS.CurrencyId, CU.Code AS CurrencyCode
	                            , IVS.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy,  IVS.DeliveryPartyPlantId
								, DPP.UserName AS DeliveryBy
	                            , SM.TransactionQty,ISNULL(SRD.ReturnQty,0) ReturnQty,SM.TransactionAmount,SM.BooksCurrencyTransactionAmount
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState
								, CP.TaxApplicable,CP.IsPaymentTermChangeable,IVS.PaymentTermId,PT.UserName PaymentTerm
								,REPLACE(CONVERT(CHAR(11), IVS.BaseOnDueDate, 106),' ','-') BaseOnDueDate,IVS.BaseNoOfDays,REPLACE(CONVERT(CHAR(11), IVS.MatureDate, 106),' ','-') MatureDate
								,'Customer' [Type]
                                ,CO.BaseCurrencyId,IVS.ToCurrencyRate
                                ,'' NoteForAccounts,IVS.SourceType
                    FROM [TRN].[Sales] AS IVS 
					LEFT JOIN (select SUM(transactionQty) transactionQty,SUM(TransactionAmount) TransactionAmount,SUM(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount,SalesId from  TRN.SalesMaterial group by salesId) SM on SM.salesId=ivs.id
					LEFT JOIN [HKP].[Party] AS P ON IVS.PartyId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable,C.IsPaymentTermChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Customer') AS CP ON CP.PartyId=IVS.PartyId AND CP.PlantId=IVS.PlantId
                    LEFT JOIN [SCS].[Currency] AS CU ON IVS.CurrencyId=CU.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IVS.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IVS.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                    LEFT JOIN ORG.Company AS CO ON CO.Id=IVS.CompanyId
					LEFT JOIN ORG.Entity E ON E.Id=IVS.EntityId
					LEFT JOIN MST.PaymentTerm PT ON PT.Id=IVS.PaymentTermId
					LEFT JOIN (select sum(TransactionQty) ReturnQty,SalesId from trn.salesreturndetail Group By SalesId) SRD ON SRD.SalesId=IVS.Id
                    WHERE IVS.PlantId='" + plantId + @"'  AND IVS.VoucherId IS NOT NULL and IVS.SourceType not in ('Packing')
					) AS TEMP WHERE " + strkey + " order by SalesDate DESC ";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> GetSalesReturnData(string plantId)
		{
			try
			{
				string CmdText = @"SELECT  II.Id,II.DocRefNo,II.SalesReturnDate, Pt.UserName Customer,
                                SUM(IID.TransactionQty) Qty,ISNULL(ISC.ReturnNetWeight,0) Verified_Qty,II.Narration,II.SalesId
                                FROM [TRN].[SalesReturn] AS II
                                JOIN TRN.SalesReturnDetail AS IID ON IID.SalesReturnId=II.Id
								LEFT JOIN (SELECT SalesReturnId,sum(ReturnNetWeight) ReturnNetWeight FROM ItemScanChild group by SalesReturnId) ISC ON ISC.SalesReturnId=II.Id
                                left join trn.Sales SS on ss.Id = II.SalesId 
								left join hkp.Party PT on PT.Id = ss.PartyId
								GROUP BY II.Id,II.Narration,II.Id,II.SalesId,II.DocRefNo,II.SalesReturnDate,II.Addeddate,ISC.ReturnNetWeight , Pt.UserName
								Order By II.AddedDate desc";
				return _sqlRepository.GetDataCollection(CmdText);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		public IEnumerable<object> GetMaterialSalesDetailBySales(string salesId)
		{
			try
			{
				string sql = @"SELECT ''Id,IID.Id SalesMaterialId, IID.SalesId 
		                        , IID.MaterialMasterId, MM.UserName AS MaterialMasterName, IID.ArticleId, AR.StandardName AS ArticleName
		                        , IID.FirstCharacteristicsId, CH1.UserName AS FirstCharacteristics, IID.FirstCharacteristicsValueId, CHV1.UserName AS FirstCharacteristicText--FirstCharacteristicsValue
		                        , IID.SecondCharacteristicsId, CH2.UserName AS SecondCharacteristics, IID.SecondCharacteristicsValueId, CHV2.UserName AS SecondCharacteristicText--SecondCharacteristicsValue
		                        , IID.ThirdCharacteristicsId, CH3.UserName AS ThirdCharacteristics, IID.ThirdCharacteristicsValueId, CHV3.UserName AS ThirdCharacteristicText--ThirdCharacteristicsValue
		                        ,IRDUM.UserName GRNUoM, IID.TransactionUoMId, IID.BaseUOMId, UoM.UserName AS TransactionUoM, IID.TransactionRate, IID.TransactionAmount
                                ,II.ToCurrencyRate, II.DocRefNo, II.InvoiceDate , II.Narration
                                ,IRD.TransactionQty GRNQty,ISH.TotalBaseAmount InventoryAmount,IID.BaseRate SalesRate,IID.BaseRate, IID.TransactionQty,ISNULL(SRD.OtherReturnQty,0) OtherQty,(IID.TransactionQty-ISNULL(SRD.OtherReturnQty,0)) BalanceQty
								,(IID.TransactionQty-ISNULL(SRD.OtherReturnQty,0)) CurrentBalanceQty,IID.NetAmount TotalAmount,IID.TaxAmount SalesTax,0 VerifiedQty,0 ReturnAmount,0 TaxAmount,IID.BaseRate,IID.BaseUoMFactor,NULL TaxList
                        FROM  [TRN].[SalesMaterial] AS IID
                        LEFT JOIN [TRN].[Sales] AS II ON IID.SalesId=II.Id
                        LEFT JOIN [MST].[MaterialMaster] AS MM ON IID.MaterialMasterId=MM.Id
                        LEFT JOIN [MST].[MaterialMasterArticle] AS AR ON IID.ArticleId=AR.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH1 ON IID.FirstCharacteristicsId=CH1.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV1 ON IID.FirstCharacteristicsValueId=CHV1.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH2 ON IID.SecondCharacteristicsId=CH2.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV2 ON IID.SecondCharacteristicsValueId=CHV2.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH3 ON IID.ThirdCharacteristicsId=CH3.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV3 ON IID.ThirdCharacteristicsValueId=CHV3.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON IID.BaseUOMId=UoM.Id
                        LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=IID.Id
						LEFT JOIN [TRN].[InventoryReceiveDetail] IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN [SCS].[UnitOfMeasurement] AS IRDUM ON IRD.BaseUOMId=IRDUM.Id
                        LEFT JOIN (SELECT SUM(TransactionQty) OtherReturnQty,SalesMaterialId FROM TRN.SalesReturnDetail group by SalesMaterialId) SRD ON SRD.SalesMaterialId=IID.Id
						WHERE IID.SalesId='" + salesId + @"'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		public List<Dictionary<string, object>> GetPackingSalesMaterialData(string companyGroupId, string companyId, string plantId, string salesId,string packingId,string smIds)
		{
			string temId = "";
            if (smIds!= "IN('null')")
            {
				temId = "AND SA.Id='" + salesId + "' and sp.PackingId='" + packingId + "' and SM.Id " + smIds + "";

			}
            else
            {
				temId = "AND SA.Id='" + salesId + "' and sp.PackingId='" + packingId + "'";
			}
			var cmdText = @"SELECT '' Id,SP.PackingId,SM.Id SalesMaterialId,SM.SalesOrderId,SM.SalesId
				, SM.MaterialMasterId, SM.ArticleId , SM.FirstCharacteristicsId , SM.TransactionUoMId, SM.BaseUOMId
				,SM.BaseRate,SM.BaseRate TempBaseRate,SM.BaseUoMFactor
				, SM.TransactionRate, SM.TransactionRate TempTransactionRate, SM.TransactionAmount,SM.TaxAmount SalesTax,SM.NetAmount TotalAmount
                ,SA.ToCurrencyRate, SA.DocRefNo, SA.InvoiceDate , SA.Narration
				,  MGM.UserName AS MaterialGroupMasterName,MM.UserName MaterialMasterName,ART.StandardName AS ArticleName
            , BUoM.UserName AS BaseUoM, TUoM.UserName AS TransactionUoM
            , CU.Code AS Currency,NULL TaxList ,FC.ValueFreeText,FCV.UserName AS [FreeText] 
            , SCV.UserName AS SecondCharacteristicsValue,TCV.UserName AS ThirdCharacteristicsValue

			,FC.Id FirstCharacteristicsId
			,FC.CharacteristicsValueId FirstCharacteristicsValueId
			,CH.UserName FCH,FCV.UserName FirstCharacteristicText

			,CH2.UserName SCH,SCV.UserName SecondCharacteristicText
			,SC.Id SecondCharacteristicsId
            ,SC.CharacteristicsValueId SecondCharacteristicsValueId

			,CH3.UserName TCH,TCV.UserName SKU3
			,TC.Id ThirdCharacteristicsId
			,TC.CharacteristicsValueId ThirdCharacteristicsValueId
            ,MO.Id MasterOrderId,SO.Id SONo,po.PONumber, FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') DeliveryDate,DT.UserName DestinationName
			, SO.SOType,SO.Rate,ISC.ReturnNetWeight ReturnQty,(ISC.ReturnNetWeight * SM.TransactionRate) Amount
			, TaxAmount=(SM.TaxAmount/SM.BaseAmount)*(ISC.ReturnNetWeight * SM.TransactionRate),ISC.ReturnNetWeight VerifiedQty
           ,SM.TransactionQty SalesQty
                ,SM.TransactionQty ,OtherQty=ISNULL(SRD.OtherReturnQty,0),BalanceQty=SM.TransactionQty-ISNULL(SRD.OtherReturnQty,0),CurrentBalanceQty=SM.TransactionQty-ISC.ReturnNetWeight
                ,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesService] WHERE SalesId=SA.Id)/(Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id))*SM.TransactionAmount
	           ,ServiceTax=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesTax] WHERE SalesId=SA.Id  AND SalesServiceId<>'')/(Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id))*SM.TransactionAmount,ISNULL(MM.HSNCodeId,ART.HSNCodeId)HSNCodeId,ISNULL(HM.Code,HA.Code)HSNCode
            FROM TRN.SalesMaterial AS SM 
            LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId
			LEFT JOIN dbo.SalesPacking SP ON SP.SalesId=SA.Id
			LEFT JOIN (SELECT isc.SalesId,pli.PackingId,isc.Booked,pli.SOId,plr.PackingLineItemId,sum(ReturnNetWeight) ReturnNetWeight FROM ItemScanChild isc 
					left join [TRN].[POLotReference] plr on plr.Id=isc.packingId
					left join [TRN].PackingLineItem pli on pli.PackingLineItemId=plr.PackingLineItemId
					left join [TRN].[SalesOrder] SO ON SO.id=pli.SOId
					where isc.booked=1 and isc.returnnetweight<>0 and isc.SalesReturnId is null 
					group by isc.SalesId,pli.PackingId,plr.PackingLineItemId,isc.Booked,pli.SOId) ISC ON ISC.PackingId=sp.PackingId
             JOIN [TRN].[SalesOrder] AS SO ON ISC.SOId=SO.Id and SO.Id=sm.SalesOrderId
            JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
			JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
			LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
			LEFT JOIN [MST].[Destination] AS DT ON DT.Id=SO.DestinationId
			LEFT JOIN (SELECT SalesMaterialId,SUM(TransactionQty) OtherReturnQty FROM TRN.SalesReturnDetail GROUP BY SalesMaterialId) SRD ON SRD.SalesMaterialId=SM.Id
            LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=SM.MaterialMasterId
            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
            LEFT JOIN MST.MaterialMasterArticle AS ART ON SM.ArticleId=ART.Id
			LEFT JOIN HKP.HSNCode HM ON HM.Id=MM.HSNCodeId
			LEFT JOIN HKP.HSNCode HA ON HA.Id=ART.HSNCodeId
            LEFT JOIN TRN.FirstCharacteristics AS FC ON FC.Id=SM.FirstCharacteristicsId AND SM.SalesOrderId=FC.SalesOrderId
            LEFT JOIN HKP.CharacteristicsValue AS FCV ON FCV.Id=SM.FirstCharacteristicsValueId
			LEFT JOIN [HKP].[Characteristics] AS CH ON FC.CharacteristicsId=CH.Id

            LEFT JOIN TRN.SecondCharacteristics AS SC ON SC.Id=SM.SecondCharacteristicsId AND SM.SalesOrderId=SC.SalesOrderId
            LEFT JOIN HKP.CharacteristicsValue AS SCV ON SCV.Id=SM.SecondCharacteristicsValueId
			LEFT JOIN [HKP].[Characteristics] AS CH2 ON SC.CharacteristicsId=CH2.Id

            LEFT JOIN TRN.ThirdCharacteristics AS TC ON TC.Id=SM.ThirdCharacteristicsId AND SM.SalesOrderId=TC.SalesOrderId
            LEFT JOIN HKP.CharacteristicsValue AS TCV ON TCV.Id=SM.ThirdCharacteristicsValueId
			LEFT JOIN [HKP].[Characteristics] AS CH3 ON TC.CharacteristicsId=CH3.Id

            LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
            JOIN [SCS].[UnitOfMeasurement] AS BUoM ON SM.BaseUOMId=BUoM.Id
            JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
            WHERE SA.CompanyGroupId='" + companyGroupId + "' AND SA.CompanyId='" + companyId + "' AND SA.PlantId='" + plantId + "' "+ temId + "";

			return _sqlRepository.GetDataCollection(cmdText);
		}

		public IEnumerable<object> GetMaterialSalesTaxDetail(string salesId)
		{
			try
			{
				string sql = @"SELECT   '' Id,A.SalesId,a.SalesMaterialId,A.TaxCategoryId,A.HSNCodeId
								,A.[Percentage],0 Amount ,0 TaxAmount,B.Code HSNCode,B.[Description]
                                FROM TRN.SalesTax A
                                Left JOIN [HKP].[HSNCode] B On A.HSNCodeId=B.Id 
                                where A.SalesId='" + salesId + "' and A.SalesServiceId is null";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public List<Dictionary<string, object>> GetItemScanChildData(string salesId, string packingId, string soId)
		{

			var cmdText = @"SELECT isc.Id,isc.SalesId,isc.SalesReturnId,isc.PackingId,isc.LotNo,isc.RefNo,isc.NetWeight,isc.GWeight,isc.Cones,isc.NetWeight,isc.NetWeight ReturnNetWeight,isc.Shade,isc.Booked,isc.IsDespatch
								,pli.SOId SalesOrderId,pli.PackingId ActualPackingId 	from dbo.ItemScanChild isc 
                                left join trn.POLotReference pol on pol.Id = isc.PackingId
                                left join trn.PackingLineItem pli on pli.PackingLineItemId = pol.PackingLineItemId
                                where  pli.PackingId = '" + packingId + "' AND pli.SOId IN ('"+ soId + "') and isc.SalesId='"+ salesId + @"'";

			return _sqlRepository.GetDataCollection(cmdText);
		}

		public List<Dictionary<string, object>> GetItemScanChildDataByPackingId(string salesId, string packingId, string soId)
		{

			var cmdText = @"SELECT isc.Id,isc.MasterId,isc.POId,isc.ProductCode,isc.AddedBy,isc.AddedDate,isc.Cones,isc.PackedBy,isc.SalesId,isc.SalesMaterialId,isc.LocMasterId,isc.SalesReturnId,isc.PackingId,isc.LotNo,isc.RefNo,isc.NetWeight,isc.GWeight,isc.Cones,isc.NetWeight ReturnNetWeight,isc.ReturnNetWeight ReturnQty,isc.Shade,isc.Booked,isc.IsDespatch
								,pli.SOId SalesOrderId,pli.PackingId ActualPackingId,ITC.ShiftId,ITC.Grade 	
								FROM dbo.ItemScanChild isc 
								LEFT JOIN dbo.ItemScan ITC ON ITC.Id=isc.MasterId
                                left join trn.POLotReference pol on pol.Id = isc.PackingId
                                left join trn.PackingLineItem pli on pli.PackingLineItemId = pol.PackingLineItemId
                                where  pli.PackingId = '" + packingId + "' AND  isc.SalesId='" + salesId + @"' and isc.SalesReturnId IS NULL and isc.booked=1 and isc.ReturnNetWeight>0";

			return _sqlRepository.GetDataCollection(cmdText);
		}

		public IEnumerable<object> GetSalesReturnPopUpData(string column, string value, string plantId)
		{
			try
			{
				string strkey = "1=1";
				if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
					strkey = column + " like '%" + value + "%'";
				var sql = @"select top 300 * from (SELECT  SR.Id ,SR.SalesId, REPLACE(CONVERT(CHAR(11), SR.SalesReturnDate, 106),' ','-') AS SalesReturnDate, IVS.CompanyGroupId, IVS.CompanyId, IVS.PlantId, IVS.PartyId CustomerId , IVS.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode, P.Code AS Tracenent
								, P.UserName AS PartyName,REPLACE(CONVERT(CHAR(11), SR.SalesReturnDate, 106),' ','-') AS SalesReturnDateNew , CP.UserName AS PartyAccountGroupName
	                            , IVS.EntityId,E.UserName Entity,FORMAT(IVS.InvoiceDate,'dd-MMM-yyyy')DocDate , REPLACE(CONVERT(CHAR(11), IVS.AddedDate, 106),' ','-') AS EntryDate,IVS.Narration,IVS.DocRefNo
								, IVS.CurrencyId, CU.Code AS CurrencyCode , IVS.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy,  IVS.DeliveryPartyPlantId , DPP.UserName AS DeliveryBy
	                            , IRD.TransactionQty, IRD.TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState
								, CP.TaxApplicable,CP.IsPaymentTermChangeable,IVS.PaymentTermId,PT.UserName PaymentTerm
								, REPLACE(CONVERT(CHAR(11), IVS.BaseOnDueDate, 106),' ','-') BaseOnDueDate,IVS.BaseNoOfDays,REPLACE(CONVERT(CHAR(11), IVS.MatureDate, 106),' ','-') MatureDate
								, 'Customer' [Type] , CO.BaseCurrencyId,IVS.ToCurrencyRate , '' NoteForAccounts,IVS.SourceType
                    FROM [TRN].[SalesReturn] AS SR 
					LEFT JOIN TRN.Sales IVS ON ivs.Id=SR.SalesId
					LEFT JOIN [HKP].[Party] AS P ON IVS.PartyId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable,C.IsPaymentTermChangeable FROM [HKP].[CompanyParty] AS C 
					LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Customer') AS CP ON CP.PartyId=IVS.PartyId AND CP.PlantId=IVS.PlantId
                    LEFT JOIN [SCS].[Currency] AS CU ON IVS.CurrencyId=CU.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IVS.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IVS.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                    LEFT JOIN ORG.Company AS CO ON CO.Id=IVS.CompanyId
                     LEFT JOIN (SELECT A.SalesReturnId, SUM(A.TransactionQty) AS TransactionQty, SUM(ROUND(A.TransactionAmount,4)) AS TransactionAmount
					 , SUM(ROUND(A.BooksCurrencyTransactionAmount,0)) AS BaseAmount, UoM.UserName AS TransactionUoM
					 FROM [TRN].[SalesReturnDetail] AS A
		                        JOIN [TRN].[SalesReturn] AS B ON A.SalesReturnId=B.Id 
								JOIN TRN.Sales S ON S.Id=B.SalesId
								LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON A.TransactionUoMId=UoM.Id
								WHERE S.PlantId='" + plantId + @"' GROUP BY A.SalesReturnId,UoM.UserName) AS IRD ON IRD.SalesReturnId=SR.Id
					LEFT JOIN ORG.Entity E ON E.Id=IVS.EntityId
					LEFT JOIN MST.PaymentTerm PT ON PT.Id=IVS.PaymentTermId
                    WHERE IVS.PlantId='" + plantId + @"'  AND IVS.VoucherId IS NOT NULL AND SR.VoucherId IS NULL) AS TEMP WHERE " + strkey + " order by SalesReturnDate DESC ";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public IEnumerable<object> GetSalesReturnDetailBySalesReturn(string salesReturnId)
		{
			try
			{
				string sql = @"SELECT ''Id,IID.Id SalesReturnDetailId, IID.SalesId ,MG.UserName MaterialGroupName
		                        , IID.MaterialMasterId, MM.UserName AS MaterialMasterName, IID.ArticleId, AR.StandardName AS ArticleName
		                        , IID.FirstCharacteristicsId, CH1.UserName AS FirstCharacteristics, IID.FirstCharacteristicsValueId, CHV1.UserName AS FirstCharacteristicText--FirstCharacteristicsValue
		                        , IID.SecondCharacteristicsId, CH2.UserName AS SecondCharacteristics, IID.SecondCharacteristicsValueId, CHV2.UserName AS SecondCharacteristicText--SecondCharacteristicsValue
		                        , IID.ThirdCharacteristicsId, CH3.UserName AS ThirdCharacteristics, IID.ThirdCharacteristicsValueId, CHV3.UserName AS ThirdCharacteristicText--ThirdCharacteristicsValue
		                        ,IRDUM.UserName GRNUoM, IID.TransactionUoMId, IID.BaseUOMId, UoM.UserName AS TransactionUoM, IID.TransactionRate, IID.TransactionAmount ReturnAmount
                                , IID.TaxAmount , II.DocRefNo , II.Narration
                                ,IRD.TransactionQty GRNQty,ISH.TotalBaseAmount InventoryAmount,ISD.SalesRate, IID.TransactionQty,0 OtherQty,(IID.TransactionQty) BalanceQty
								,ISD.TotalAmount,IID.BaseRate,IID.BaseUoMFactor,NULL TaxList
                        FROM  [TRN].[SalesReturnDetail] AS IID
                        LEFT JOIN [TRN].[SalesReturn] AS II ON IID.SalesReturnId=II.Id
                        LEFT JOIN [MST].[MaterialMaster] AS MM ON IID.MaterialMasterId=MM.Id
                        LEFT JOIN [MST].[MaterialMasterArticle] AS AR ON IID.ArticleId=AR.Id
						LEFT JOIN MST.MaterialGroupMaster MG ON MG.Id=MM.MaterialGroupMasterId
                        LEFT JOIN [HKP].[Characteristics] AS CH1 ON IID.FirstCharacteristicsId=CH1.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV1 ON IID.FirstCharacteristicsValueId=CHV1.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH2 ON IID.SecondCharacteristicsId=CH2.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV2 ON IID.SecondCharacteristicsValueId=CHV2.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH3 ON IID.ThirdCharacteristicsId=CH3.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV3 ON IID.ThirdCharacteristicsValueId=CHV3.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON IID.BaseUOMId=UoM.Id
                        LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=IID.Id
						LEFT JOIN [TRN].[InventoryReceiveDetail] IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN [SCS].[UnitOfMeasurement] AS IRDUM ON IRD.BaseUOMId=IRDUM.Id
                        LEFT JOIN (select distinct Id,ROUND(sum(TransactionQty), 2) Qty,ROUND(sum(SalesRate), 2) SalesRate,(ROUND(sum(TransactionQty), 2) * ROUND(sum(SalesRate), 2)) TotalAmount from  TRN.InventorySalesDetail group by Id) ISD ON ISD.Id=IID.Id
						WHERE IID.SalesReturnId='" + salesReturnId + @"'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IEnumerable<object> GetSalesReturnTaxDetail(string salesId)
		{
			try
			{
				string sql = @"SELECT   '' Id,A.SalesId,a.SalesMaterialId,A.TaxCategoryId,A.HSNCodeId
								,A.[Percentage],0 Amount ,0 TaxAmount,B.Code HSNCode,B.[Description]
                                FROM TRN.SalesReturnTax A
                                Left JOIN [HKP].[HSNCode] B On A.HSNCodeId=B.Id   
                                where A.SalesId='" + salesId + "' and A.SalesServiceId is null";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		
		public IEnumerable<object> GetSalesReturnJournalData(string companyId, string plantId, string salesReturnId, string customerId)
		{
			try
			{
				var companyParty = GetCompanyPartyGroup(customerId, plantId);
				
					var sql = @"DECLARE @receiveId varchar(10)='" + salesReturnId + "', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + "', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr, T.Amount, T.IsAsset,T.InventoryReceiveDetailId
					FROM (
						SELECT  'Material' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.FixedAssetMasterId
							,  IRD.PostCrGLGeneralInfoId  GLGeneralInfoId , GL.AccountCode  GLGeneralInfoCode  ,GL.UserName GLGeneralInfoName
							,IRD.PostCrBudgetMasterId BudgetMasterId  ,B.Code BudgetCode  ,B.UserName BudgetName  ,IRD.PostCrActivityId ActivityId,A.Code ActivityCode 
							,A.UserName ActivityName  , SUM(PRD.TransactionAmount) AS Dr , NULL Cr
							, SUM(PRD.TransactionAmount) AS Amount
                            ,MM.IsAsset,IRD.Id AS  InventoryReceiveDetailId
						FROM TRN.SalesReturnDetail PRD
						JOIN [TRN].[SalesMaterial] AS IRD ON IRD.Id=PRD.SalesMaterialId
						LEFT JOIN [TRN].[SalesReturn] AS SR ON PRD.SalesReturnId=SR.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON PRD.MaterialMasterId=MM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON IRD.PostCrGLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON IRD.PostCrBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON IRD.PostCrActivityId= A.Id
						WHERE PRD.SalesReturnId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, IRD.PostCrGLGeneralInfoId, GL.AccountCode, GL.UserName, IRD.PostCrBudgetMasterId, B.Code, B.UserName, IRD.PostCrActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,IRD.Id
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId
					UNION
					SELECT R.OtherName, R.TrnType, R.MaterialGroupMasterId, R.TaxCategoryId
						, R.GLGeneralInfoId, R.GLGeneralInfoCode, R.GLGeneralInfoName , R.BudgetMasterId, R.BudgetCode, R.BudgetName
						, R.ActivityId, R.ActivityCode,R.ActivityName , R.Dr , R.Cr, R.Amount, R.IsAsset,R.InventoryReceiveDetailId 
					FROM (
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName , T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr Dr , T.Cr, T.Amount Amount, T.IsAsset,T.InventoryReceiveDetailId,T.SalesReturnId
					FROM (
						SELECT  'Return' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,NULL FixedAssetMasterId
							,  MGGL.CreditNoteGLId  GLGeneralInfoId , GL.AccountCode  GLGeneralInfoCode  ,GL.UserName GLGeneralInfoName
							,MGGL.CreditNoteBudgetMasterId BudgetMasterId  ,B.Code BudgetCode  ,B.UserName BudgetName  ,MGGL.CreditNoteActivityId ActivityId,A.Code ActivityCode 
							,A.UserName ActivityName , NULL Dr , SUM(SRD.TransactionAmount+ISNULL(SRD.TaxAmount,0)) AS Cr
							, SUM(SRD.TransactionAmount+ISNULL(SRD.TaxAmount,0) ) AS Amount ,MM.IsAsset , NULL InventoryReceiveDetailId,SRD.SalesReturnId
						FROM TRN.SalesReturnDetail SRD
						LEFT JOIN [MST].[MaterialMaster] AS MM ON SRD.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.CreditNoteGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.CreditNoteBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.CreditNoteActivityId= A.Id
						WHERE SRD.SalesReturnId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.CreditNoteGLId, GL.AccountCode, GL.UserName, MGGL.CreditNoteBudgetMasterId, B.Code, B.UserName
						, MGGL.CreditNoteActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId ,SRD.SalesReturnId
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount,T.SalesReturnId, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId
					)
					R 
					GROUP BY R.MaterialGroupMasterId, R.GLGeneralInfoId, R.GLGeneralInfoCode, R.GLGeneralInfoName, R.BudgetMasterId, R.BudgetCode, R.BudgetName, R.ActivityId
                    , R.ActivityCode, R.ActivityName, R.Dr, R.Cr, R.Amount, R.OtherName, R.TrnType,R.TaxCategoryId,R.IsAsset, R.InventoryReceiveDetailId
					UNION ALL
					SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, ITD.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, ITD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName , ITD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRT.Amount) AS  Dr ,  NULL Cr , SUM(IRT.Amount) AS Amount
                        , 0 IsAsset, NULL InventoryReceiveDetailId
					FROM [TRN].[SalesReturnTax] AS IRT
					LEFT JOIN [TRN].[SalesReturnDetail] AS IRD ON IRT.SalesReturnDetailId=IRD.Id
                    LEFT JOIN [TRN].[SalesReturn] AS PR ON IRD.SalesReturnId=PR.Id
					LEFT JOIN TRN.[Sales] AS IR ON IR.Id=PR.SalesId
					LEFT JOIN TRN.InvoiceTax IT ON IT.VoucherId=IR.VoucherId and IRT.TaxCategoryId=IT.TaxCategoryId
					LEFT JOIN TRN.InvoiceTaxDetail ITD ON ITD.InvoiceTaxId=IT.Id 
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ITD.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON ITD.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON ITD.ActivityId= A.Id
					WHERE IRD.SalesReturnId=@receiveId  AND IRT.SalesReturnDetailId<>'' AND ITD.AType='Cr'
					GROUP BY  IRT.TaxCategoryId, ITD.GLGeneralInfoId, GL.AccountCode, GL.UserName, ITD.BudgetMasterId, B.Code, B.UserName, ITD.ActivityId, A.Code, A.UserName
                    ORDER BY T.TrnType DESC";
					return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> GetSalesReturnDetailGLUpdateData(string companyId, string plantId, string salesReturnId)
		{
			try
			{
				var sql = @"DECLARE @receiveId varchar(10)='" + salesReturnId + "', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"'
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr, T.Amount, T.IsAsset,T.InventoryReceiveDetailId,T.SalesReturnDetailId
					FROM (
						SELECT  'Material' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.FixedAssetMasterId
							,  IRD.PostCrGLGeneralInfoId  GLGeneralInfoId , GL.AccountCode  GLGeneralInfoCode  ,GL.UserName GLGeneralInfoName
							,IRD.PostCrBudgetMasterId BudgetMasterId  ,B.Code BudgetCode  ,B.UserName BudgetName  ,IRD.PostCrActivityId ActivityId,A.Code ActivityCode 
							,A.UserName ActivityName  , SUM(PRD.BooksCurrencyTransactionAmount) AS Dr , NULL Cr
							, SUM(PRD.BooksCurrencyTransactionAmount) AS Amount
                            ,MM.IsAsset,IRD.Id AS  InventoryReceiveDetailId,PRD.Id SalesReturnDetailId
						FROM TRN.SalesReturnDetail PRD
						JOIN [TRN].[SalesMaterial] AS IRD ON IRD.Id=PRD.SalesMaterialId
						LEFT JOIN [TRN].[SalesReturn] AS SR ON PRD.SalesReturnId=SR.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON PRD.MaterialMasterId=MM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON IRD.PostCrGLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON IRD.PostCrBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON IRD.PostCrActivityId= A.Id
						WHERE PRD.SalesReturnId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, IRD.PostCrGLGeneralInfoId, GL.AccountCode, GL.UserName, IRD.PostCrBudgetMasterId, B.Code, B.UserName, IRD.PostCrActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,IRD.Id,PRD.Id
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId,T.SalesReturnDetailId
					UNION ALL
					SELECT R.OtherName, R.TrnType, R.MaterialGroupMasterId, R.TaxCategoryId
						, R.GLGeneralInfoId, R.GLGeneralInfoCode, R.GLGeneralInfoName , R.BudgetMasterId, R.BudgetCode, R.BudgetName
						, R.ActivityId, R.ActivityCode,R.ActivityName , R.Dr , R.Cr, R.Amount, R.IsAsset,R.InventoryReceiveDetailId,R.SalesReturnDetailId 
					FROM (
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName , T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr Dr , T.Cr, T.Amount Amount, T.IsAsset,T.InventoryReceiveDetailId,T.SalesReturnId,T.SalesReturnDetailId
					FROM (
						SELECT  'Return' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,NULL FixedAssetMasterId
							,  MGGL.CreditNoteGLId  GLGeneralInfoId , GL.AccountCode  GLGeneralInfoCode  ,GL.UserName GLGeneralInfoName
							,MGGL.CreditNoteBudgetMasterId BudgetMasterId  ,B.Code BudgetCode  ,B.UserName BudgetName  ,MGGL.CreditNoteActivityId ActivityId,A.Code ActivityCode 
							,A.UserName ActivityName , NULL Dr , SUM(SRD.BooksCurrencyTransactionAmount+ISNULL(SRD.TaxAmount,0)) AS Cr
							, SUM(SRD.BooksCurrencyTransactionAmount+ISNULL(SRD.TaxAmount,0) ) AS Amount ,MM.IsAsset , NULL InventoryReceiveDetailId,SRD.SalesReturnId,SRD.Id SalesReturnDetailId
						FROM TRN.SalesReturnDetail SRD
						LEFT JOIN [MST].[MaterialMaster] AS MM ON SRD.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.CreditNoteGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.CreditNoteBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.CreditNoteActivityId= A.Id
						WHERE SRD.SalesReturnId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.CreditNoteGLId, GL.AccountCode, GL.UserName, MGGL.CreditNoteBudgetMasterId, B.Code, B.UserName
						, MGGL.CreditNoteActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId ,SRD.SalesReturnId,SRD.Id 
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount,T.SalesReturnId, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId,T.SalesReturnDetailId
					)
					R 
					GROUP BY R.MaterialGroupMasterId, R.GLGeneralInfoId, R.GLGeneralInfoCode, R.GLGeneralInfoName, R.BudgetMasterId, R.BudgetCode, R.BudgetName, R.ActivityId
                    , R.ActivityCode, R.ActivityName, R.Dr, R.Cr, R.Amount, R.OtherName, R.TrnType,R.TaxCategoryId,R.IsAsset, R.InventoryReceiveDetailId,R.SalesReturnDetailId
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

		public IEnumerable<object> GetSalesReturnPostedData(string column, string value, string plantId)
		{
			try
			{
				string strkey = "1=1";
				if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
					strkey = column + " like '%" + value + "%'";
				var sql = @"select top 300 * from (SELECT  II.Id,II.DocRefNo,II.SalesReturnDate,S.ToCurrencyRate,V.CurrencyId,II.SalesReturnDate DocDate,II.VoucherId,V.VoucherNo,V.PostingDate,V.SourceType, P.UserName PartyName,
								[Park/Post]=CASE WHEN V.IsPark=0 THEN 'Posted' ELSE 'Parked' END,
                                IID.TransactionQty Qty,II.Narration,II.SalesId,II.AddedDate,ADN.Id AdditionalTaxId,ADN.VoucherId TDSTaxVoucherId
								,VADN.VoucherNo TDSVoucherNo,IsTDSTaxPost=case when VADN.IsPark=0 then 'TDSPosted'  when VADN.IsPark=1 then 'TDSParked' ELSE '' END
                                FROM [TRN].[SalesReturn] AS II
                                JOIN (SELECT SUM(TransactionQty) TransactionQty,SalesReturnId FROM TRN.SalesReturnDetail GROUP BY SalesReturnId) AS IID ON IID.SalesReturnId=II.Id
								JOIN TRN.Sales S ON S.Id=II.SalesId
								LEFT JOIN TRN.AdjustmentNote AN ON AN.SalesReturnId=II.Id
								LEFT JOIN TRN.AdditionalTax ADN ON ADN.AdjustmentNoteId=AN.Id
								LEFT JOIN TRN.Voucher V ON V.Id=II.VoucherId
								LEFT JOIN TRN.Voucher VADN ON VADN.Id=ADN.VoucherId
								LEFT JOIN HKP.Party P ON P.Id=S.PartyId
								WHERE II.VoucherId<>'') AS TEMP WHERE " + strkey + @"
								Order By AddedDate desc";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IEnumerable<object> GetCreditNoteAdditionalTaxDetail(string additionalTaxId)
		{
			try
			{
				var sql = @"SELECT  GL.AccountCode+' - '+ GL.UserName GLName,BU.UserName BudgetName,A.UserName ActivityName,0 DrAmount,ATD.Amount CrAmount,ATD.AdditionalTaxId
                            ,ATD.GLGeneralInfoId, ATD.BudgetMasterId, ATD.ActivityId,TC.Id TaxCategoryId,ATD.TaxCodeId,ATD.AType
                            FROM TRN.AdditionalTaxDetail ATD 
                            JOIN TRN.AdditionalTax ATX ON ATX.Id=ATD.AdditionalTaxId
                            LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=ATD.GLGeneralInfoId
                            LEFT JOIN MST.BudgetMaster BM ON BM.Id=ATD.BudgetMasterId
                            LEFT JOIN HKP.Budget BU ON BU.Id=BM.BudgetId
                            LEFT JOIN HKP.Activity A ON A.Id=ATD.ActivityId
							LEFT JOIN MST.TaxCode TAC ON TAC.Id=ATD.TaxCodeId
							LEFT JOIN MST.TaxCategory TC ON TC.Id=TAC.TaxCategoryId
							WHERE ATX.Id='" + additionalTaxId + @"'

                            UNION
							SELECT  GL.AccountCode+' - '+ GL.UserName GLName,BU.UserName BudgetName,A.UserName ActivityName,ATX.TaxAmount DrAmount,0 CrAmount,ATX.Id AdditionalTaxId
							,IVD.GLGeneralInfoId, IVD.BudgetMasterId, IVD.ActivityId,NULL TaxCategoryId,NULL TaxCodeId,'Dr' AType
                            FROM  TRN.AdditionalTax ATX 
							LEFT JOIN TRN.AdjustmentNote IV ON IV.Id=ATX.AdjustmentNoteId
							LEFT JOIN TRN.AdjustmentNoteDetail IVD ON IVD.AdjustmentNoteId=IV.Id
                            LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=IVD.GLGeneralInfoId
                            LEFT JOIN MST.BudgetMaster BM ON BM.Id=IVD.BudgetMasterId
                            LEFT JOIN HKP.Budget BU ON BU.Id=BM.BudgetId
                            LEFT JOIN HKP.Activity A ON A.Id=IVD.ActivityId
							WHERE ATX.Id='" + additionalTaxId + "' ";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

	}
}
