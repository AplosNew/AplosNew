using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Model.Parties;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.MaterialManagement.Inventory
{
	public class InventoryMaterialService : Service<InventoryMaterial>, IInventoryMaterialService
	{
		#region Constructor

		private readonly IRepositoryAsync<InventoryReceive> _inventoryReceiveRepository;
		private readonly IRepositoryAsync<CompanyParty> _companyPartyRepository;
		private readonly IRepositoryAsync<InventoryMaterial> _inventoryMaterialRepository;
		private readonly ISqlRepository _sqlRepository;

		public InventoryMaterialService(
			IRepositoryAsync<InventoryMaterial> inventoryMaterialRepository
			, IRepositoryAsync<InventoryReceive> inventoryReceiveRepository
			, IRepositoryAsync<CompanyParty> companyPartyRepository
			, IPKGeneratorService pkGeneratorService
			, IUnitOfWork unitOfWork
			, ISqlRepository sqlRepository
			) : base(inventoryMaterialRepository, unitOfWork, pkGeneratorService)
		{
			_inventoryMaterialRepository = inventoryMaterialRepository;
			_inventoryReceiveRepository = inventoryReceiveRepository;
			_companyPartyRepository = companyPartyRepository;
			_sqlRepository = sqlRepository;
		}

		#endregion Constructor

		private string GetPK()
		{
			return base.GetAutoNumber(nameof(InventoryMaterial), PKGeneratorEnum.Auto, null, DateTime.Now);
		}
		public GridModel Querywithoutpo(GridParameter parameters, string inveReveiveId)
		{
			try
			{
				parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
				                             , @totalReceiveAmount DECIMAL(18, 4)=0
				                             , @totalServiceAmount DECIMAL(18, 4)=0
				                             , @totalSvcTaxAmount DECIMAL(18, 4)=0
				                             , @totalAdditionalServiceAmount DECIMAL(18, 4)=0
				                             , @totalAdditionalSvcTaxAmount DECIMAL(18, 4)=0
				                  SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
				                  SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@inventoryReceiveId and IsOtherVendor=0)
				                  SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'' AND InventoryServiceId in (select Id from trn.InventoryService where InventoryReceiveId=@inventoryReceiveId and IsOtherVendor=0) )
				                  
								  SET @totalAdditionalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@inventoryReceiveId and IsOtherVendor=1)
				                  SET @totalAdditionalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'' AND InventoryServiceId in (select Id from trn.InventoryService where InventoryReceiveId=@inventoryReceiveId and IsOtherVendor=1) )
								  
								  SELECT IM.Id, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
				                      , MGM.UserName AS MaterialGroupMasterName
				                      , IM.MaterialMasterId, MM.UserName
				                      , IM.ArticleId, ART.StandardName
				                      , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
				                      , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
				                      , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
				                      , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
				                      , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
				                      , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
				                      , IRD.GRNQty TransactionQty
				                      , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
				                      , IRD.MaterialTranRate AS TransactionRate
				                      , CU.Code AS CurrencyName, IR.ToCurrencyRate
				                       , (IRD.MaterialTranAmount) AS TrnAmount
				                      , IRD.ToTalMaterialBooksCurrencyAmount AS BaseAmount
                                         
				                      , IRD.TotalTaxAmount AS BaseTaxAmount
				                   , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
				                   , IRD.ChargesTranAmount AS ChargesAmount
				                   ,ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
	                               , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
				                  ,AdditionalChargesAmount=(@totalAdditionalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
	                               , AdditionalChargesTax=(@totalAdditionalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
				                 

				                   , IRD.CountryId
				                    , IRD.TotalMaterialTranAmount AS TotalMaterialTranAmount  
                                    ,null TaxList
                                    ,IRD.InventoryReceiveId,IRD.BaseUOMId,IRD.InventoryMaterialId, IRD.MaterialStorageId
                                    ,isnull(IRD.ShortageQty,0) ShortageQty,isnull(IRD.RejectionQty,0) RejectionQty,isnull(IRD.ApprovedQty,0) ApprovedQty
                                    ,Isnull(IRD.TransactionQty,0) AS NetQty
                                    ,IRD.BaseQty
									,IRD.BaseUoMFactor,IRD.Description
                                    ,null DataChangeFlag,IRD.ShortRejFlag
                                   ,IRD.ShortageRatePercent ShortageRate,IRD.ShortageValue,IRD.RejectRatePercent RejectionRate,IRD.RejectValue RejectionValue,IRD.RejectClamPercent RejectionClamRate
                                   ,C.Id CountryId,C.UserName CountryName,IRD.LotNumber,IRD.Diameter,IRD.Type ,MS.UserName MaterialStorageName,IRD.QualityStatus,MOI.MasterOrderId
				                  FROM TRN.InventoryMaterial AS IM
				                  JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
				                  LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
				                  LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
				                  LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
				                  LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
				                  LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
				                  LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
				                  LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
				                  LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
				                  JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
				                  JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
				                  JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
				                  JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                  left Join [SCS].[Country] AS C ON C.Id=IM.CountryId
								  left JOIN [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
								  LEFT JOIN [TRN].[MasterOrderItem] MOI ON IRD.MasterOrderItemId=MOI.Id
				                  WHERE IRD.InventoryReceiveId=@inventoryReceiveId And Im.MaterialMasterId is not null --Order BY IRD.Id ASC

                                  UNION ALL

								SELECT  IM.Id, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
				                      , '' AS MaterialGroupMasterName
				                      , '' MaterialMasterId
									  ,'' UserName
				                      , '' ArticleId
									  , '' StandardName
				                      , IM.FirstCharacteristicsId, '' AS FirstCharacteristics
				                      , IM.FirstCharacteristicsValueId, '' AS FirstCharacteristicsValue
				                      , IM.SecondCharacteristicsId, '' AS SecondCharacteristics
				                      , IM.SecondCharacteristicsValueId, '' AS SecondCharacteristicsValue
				                      , IM.ThirdCharacteristicsId, '' AS ThirdCharacteristics
				                      , IM.ThirdCharacteristicsValueId, '' AS ThirdCharacteristicsValue
				                      , IRD.GRNQty TransactionQty
				                      , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
				                      , IRD.MaterialTranRate AS TransactionRate
				                      , CU.Code AS CurrencyName, IR.ToCurrencyRate
				                       , (IRD.MaterialTranAmount) AS TrnAmount
				                      , IRD.ToTalMaterialBooksCurrencyAmount AS BaseAmount
                                         
				                      , IRD.TotalTaxAmount AS BaseTaxAmount
				                   , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
				                   , IRD.ChargesTranAmount AS ChargesAmount
				                   ,ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
	                               , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
									,AdditionalChargesAmount=(@totalAdditionalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
	                               , AdditionalChargesTax=(@totalAdditionalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
				                 
				                   , IRD.CountryId
				                    , IRD.TotalMaterialTranAmount AS TotalMaterialTranAmount  
                                    ,null TaxList
                                    ,IRD.InventoryReceiveId,IRD.BaseUOMId,IRD.InventoryMaterialId, IRD.MaterialStorageId
                                    ,isnull(IRD.ShortageQty,0) ShortageQty,isnull(IRD.RejectionQty,0) RejectionQty,isnull(IRD.ApprovedQty,0) ApprovedQty
                                    ,ISNULL(IRD.TransactionQty,0) AS NetQty
                                    ,IRD.BaseQty
									,IRD.BaseUoMFactor,IRD.Description
                                    ,null DataChangeFlag,IRD.ShortRejFlag
                                    ,IRD.ShortageRatePercent ShortageRate,IRD.ShortageValue,IRD.RejectRatePercent RejectionRate,IRD.RejectValue RejectionValue,IRD.RejectClamPercent RejectionClamRate
                                    ,C.Id CountryId,C.UserName CountryName,IRD.LotNumber,IRD.Diameter,IRD.Type ,MS.UserName MaterialStorageName,IRD.QualityStatus,MOI.MasterOrderId
				                  FROM TRN.InventoryMaterial AS IM
				                 
				                  JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
				                  JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
				                  JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
				                  JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                  left Join [SCS].[Country] AS C ON C.Id=IM.CountryId
								  left JOIN [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
								  LEFT JOIN [TRN].[MasterOrderItem] MOI ON IRD.MasterOrderItemId=MOI.Id
				                  WHERE IRD.InventoryReceiveId=@inventoryReceiveId And Im.MaterialMasterId is null Order BY IRD.Id ASC";

				return _sqlRepository.GetDifferentGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public GridModel Query(GridParameter parameters, string inveReveiveId, string POID, string AcceptanceId)
		{
			try
			{
				
				if (string.IsNullOrEmpty(AcceptanceId) || AcceptanceId == "undefined")
				{

					parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
                                                  , @totalReceiveAmount DECIMAL(18, 4)=0
                                             , @totalServiceAmount DECIMAL(18, 4)=0
                                             , @totalSvcTaxAmount DECIMAL(18, 4)=0
                                  SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                                  SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@inventoryReceiveId)
                                  SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
                                  SELECT IM.Id, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
                                      ,REPLACE(CONVERT(CHAR(11), PID.AddedDate, 106),' ','-') AS AddedDate
                                      , MGM.UserName AS MaterialGroupMasterName
                                      , IM.MaterialMasterId, MM.UserName
                                      , IM.ArticleId, ART.StandardName
                                      , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                                      , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                                      , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                                      , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                                      , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                                      , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
                                      , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
                                      , IRD.MaterialTranRate AS TransactionRate
                                      , CU.Code AS CurrencyName, IR.ToCurrencyRate
                                      , IRD.MaterialTranAmount AS TrnAmount
                                      , IRD.ToTalMaterialBooksCurrencyAmount AS BaseAmount
                                      , IRD.TotalTaxAmount AS BaseTaxAmount
                                   , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
                                   , IRD.ChargesTranAmount AS ChargesAmount	                      
                                      ,ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
                                   , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
                                   
                                       ,PID.TransactionQty AS POQty
                                       ,ISNULL(Pre.OtherReceived,0) OtherReceived	                       
                                      ,IRD.GRNQty TransactionQty                         
                					 ,(PID.TransactionQty-IRD.GRNQty-ISNULL(Pre.OtherReceived,0)) AS Balance                        
									  ,IRD.TransactionUoMId
                					  ,IRD.BaseUOMId   
                                      ,IRD.TotalMaterialTranAmount
                                      ,IRD.TotalMaterialBooksCurrencyAmount AS TotalMaterialBaseAmount
                                       ,IRD.ShortageQty
                					   ,IRD.RejectionQty
                					   ,IRD.ApprovedQty
                                      ,IRD.TransactionQty AS PreviousQty
                                      ,IRD.ShortageRatePercent AS ShortageRate,IRD.ShortageValue,IRD.RejectRatePercent AS RejectionRate,IRD.RejectValue AS RejectionValue,IRD.RejectClamPercent RejectionClamRate,IR.CheckedBy,PID.Description MaterialDetail,c.UserName CountryName,C.Id CountryId ,IRD.GRNQty-IRD.ShortageQty AS NetQty,MS.Id MaterialStorageId,IRD.GrossAmount,IRD.DiscountAmount,IRD.QualityStatus
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
                                  LEFT jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id and ird.InventoryReceiveId='" + inveReveiveId + @"'
                                  LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId   
                                  LEFT JOIN (select PODetailsId,  Sum(TransactionQty) as OtherReceived 
                                  from trn.InventoryReceiveDetail where InventoryReceiveId not in('" + inveReveiveId + "') --AND POid='" + POID + @"'
                                  Group By PODetailsId) AS Pre on pre.PODetailsId=IRD.PODetailsId

                        JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                        JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID=PID.RequisitionDetailId
                        left join scs.country C ON C.Id=IM.CountryId 
						LEFT JOIN  [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                        WHERE IRD.InventoryReceiveId=@inventoryReceiveId and IM.MaterialMasterId IS not null UNION ALL SELECT IM.Id, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId,REPLACE(CONVERT(CHAR(11), PID.AddedDate, 106),' ','-') AS AddedDate, MGM.UserName AS MaterialGroupMasterName, IM.MaterialMasterId, MM.UserName, IM.ArticleId, ART.StandardName, IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics, IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics, IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics, IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue   , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM, IRD.MaterialTranRate AS TransactionRate, CU.Code AS CurrencyName, IR.ToCurrencyRate, (IRD.TransactionQty*IRD.MaterialTranRate) AS TrnAmount, IRD.ToTalMaterialBooksCurrencyAmount AS BaseAmount, IRD.TotalTaxAmount AS BaseTaxAmount, TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id), IRD.ChargesTranAmount AS ChargesAmount	,ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount, ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount, PID.TransactionQty AS POQty,ISNULL(Pre.OtherReceived,0) OtherReceived ,IRD.GRNQty TransactionQty  ,(PID.TransactionQty-IRD.TransactionQty-ISNULL(Pre.OtherReceived,0)) AS Balance   ,IRD.TransactionUoMId,IRD.BaseUOMId   ,IRD.TotalMaterialTranAmount,IRD.ToTalMaterialBooksCurrencyAmount AS TotalMaterialBaseAmount,IRD.ShortageQty,IRD.RejectionQty,IRD.ApprovedQty ,IRD.TransactionQty AS PreviousQty
                       ,IRD.ShortageRatePercent AS ShortageRate,IRD.ShortageValue,IRD.RejectRatePercent AS RejectionRate,IRD.RejectValue AS RejectionValue,IRD.RejectClamPercent RejectionClamRate,IR.CheckedBy,PID.Description MaterialDetail,c.UserName CountryName,C.Id CountryId,IRD.GRNQty-IRD.ShortageQty AS NetQty,MS.Id MaterialStorageId,IRD.GrossAmount,IRD.DiscountAmount,IRD.QualityStatus from TRN.InventoryMaterial AS IM left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id   LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id  LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id  LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id  LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id   LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id    LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id   LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id   LEFT jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id and ird.InventoryReceiveId='" + inveReveiveId + "' LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId     LEFT JOIN (select PODetailsId,  Sum(TransactionQty) as OtherReceived 	from trn.InventoryReceiveDetail where InventoryReceiveId not in('" + inveReveiveId + "') AND POid='" + POID + @"' Group By PODetailsId) AS Pre on pre.PODetailsId=IRD.PODetailsId  LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id   LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID=PID.RequisitionDetailId left join scs.country C ON C.Id=IM.CountryId LEFT JOIN  [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId WHERE IRD.InventoryReceiveId=@inventoryReceiveId  and IM.MaterialMasterId IS  null";
				}
				else
				{
					parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
                                        , @totalReceiveAmount DECIMAL(18, 4)=0
                                        , @totalServiceAmount DECIMAL(18, 4)=0
                                        , @totalSvcTaxAmount DECIMAL(18, 4)=0
                                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                                        SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@inventoryReceiveId)
                                        SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
                                        SELECT IM.Id, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
                                        ,REPLACE(CONVERT(CHAR(11), PID.AddedDate, 106),' ','-') AS AddedDate
                                        , MGM.UserName AS MaterialGroupMasterName
                                        , IM.MaterialMasterId, MM.UserName
                                        , IM.ArticleId, ART.StandardName
                                        , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                                        , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                                        , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                                        , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                                        , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                                        , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
                                        , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
                                        , IRD.MaterialTranRate AS TransactionRate
                                        , CU.Code AS CurrencyName, IR.ToCurrencyRate
                                        , (IRD.TransactionQty*IRD.MaterialTranRate) AS TrnAmount
                                        , IRD.ToTalMaterialBooksCurrencyAmount AS BaseAmount
                                        , IRD.TotalTaxAmount AS BaseTaxAmount
                                        , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
                                        , IRD.ChargesTranAmount AS ChargesAmount	                      
                                        ,ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
                                        , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
                                        , IRD.CountryId
                                        ,POD.TransactionQty AS OriginalPOQty
                                        ,isnull(PID.TransactionQty,0) AS POQty
                                        ,isnull(Pre.OtherReceived,0) OtherReceived                    
                                        ,isnull(IRD.TransactionQty,0)  TransactionQty                       
                                        --,(isnull(PID.TransactionQty,0)-isnull(IRD.TransactionQty,0)) AS Balance     
										,(POD.TransactionQty-(Isnull((isnull(PID.TransactionQty,0) + isnull(Pre.OtherReceived,0)),0))) AS Balance                   
                                        ,IRD.TransactionUoMId
                                        ,IRD.BaseUOMId   
                                        ,IRD.TotalMaterialTranAmount
                                        ,IRD.TotalMaterialBooksCurrencyAmount AS TotalMaterialBaseAmount

                                        ,IRD.ShortageQty
                                        ,IRD.RejectionQty
                                        ,IRD.ApprovedQty
                                        ,IRD.TransactionQty AS PreviousQty
                                        ,IRD.ShortageRatePercent AS ShortageRate,IRD.ShortageValue,IRD.RejectRatePercent AS RejectionRate,IRD.RejectValue AS RejectionValue
                                        ,IRD.RejectClamPercent RejectionClamRate,IR.CheckedBy
                                        --,PID.Description MaterialDetail
                                       ,IRD.PurchaseDocumentAcceptanceId,IRD.PurchaseDocumentAcceptanceDetailId,MS.Id MaterialStorageId
                                        FROM TRN.InventoryMaterial AS IM
                                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                                        LEFT jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id and ird.InventoryReceiveId='" + inveReveiveId + @"'
                                        LEFT JOIN [TRN].[PurchaseDocAcceptanceDetail] AS PID on PID.Id=IRD.PurchaseDocumentAcceptanceDetailId
                                        LEFT JOIN (select PODetailsId,  Sum(TransactionQty) as OtherReceived 
			                                        from trn.InventoryReceiveDetail where InventoryReceiveId not in('" + inveReveiveId + "') --AND POid='" + POID + @"'
			                                        Group By PODetailsId) AS Pre on pre.PODetailsId=IRD.PODetailsId

                                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                                        LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                                        LEFT JOIN (Select POD.id, sum(POD.TransactionQty) TransactionQty from  TRN.PurchaseOrderDetail AS POD 
													LEFT JOIN  [TRN].[PurchaseOrder] AS PO ON PO.Id=POD.InventoryReceiveId
													 Group By POD.id
													)POD ON POD.Id=PID.PODetailId
                                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                        --LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID=PID.RequisitionDetailId
										LEFT JOIN  [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                                        WHERE IRD.InventoryReceiveId=@inventoryReceiveId and IM.MaterialMasterId IS not null";

				}
				return _sqlRepository.GetDifferentGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

        public GridModel QueryBOQ(GridParameter parameters, string inveReveiveId, string POID, string AcceptanceId)
        {
            try
            {

                if (string.IsNullOrEmpty(AcceptanceId) || AcceptanceId == "undefined")
                {

                    parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
                                                  , @totalReceiveAmount DECIMAL(18, 4)=0
                                             , @totalServiceAmount DECIMAL(18, 4)=0
                                             , @totalSvcTaxAmount DECIMAL(18, 4)=0
                                  SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                                  SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@inventoryReceiveId)
                                  SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
                                  SELECT IM.Id,IRD.InventoryReceiveId, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
                                      ,REPLACE(CONVERT(CHAR(11), PID.AddedDate, 106),' ','-') AS AddedDate
                                      , MGM.UserName AS MaterialGroupMasterName
                                      , IM.MaterialMasterId, MM.UserName
                                      , IM.ArticleId, ART.StandardName
                                      , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                                      , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                                      , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                                      , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                                      , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                                      , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
                                      , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
                                      , IRD.MaterialTranRate AS TransactionRate
                                      , CU.Code AS CurrencyName, IR.ToCurrencyRate
                                      , IRD.MaterialTranAmount AS TrnAmount
                                      , IRD.ToTalMaterialBooksCurrencyAmount AS BaseAmount
                                      , IRD.TotalTaxAmount AS BaseTaxAmount
                                   , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
                                   , IRD.ChargesTranAmount AS ChargesAmount	                      
                                      ,ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
                                   , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
                                   
                                       ,PID.TransactionQty AS POQty
                                       ,ISNULL(Pre.OtherReceived,0) OtherReceived	                       
                                      ,IRD.GRNQty TransactionQty                         
                					 ,(PID.TransactionQty-IRD.GRNQty-ISNULL(Pre.OtherReceived,0)) AS Balance                        
									  ,IRD.TransactionUoMId
                					  ,IRD.BaseUOMId   
                                      ,IRD.TotalMaterialTranAmount
                                      ,IRD.TotalMaterialBooksCurrencyAmount AS TotalMaterialBaseAmount
                                       ,IRD.ShortageQty
                					   ,IRD.RejectionQty
                					   ,IRD.ApprovedQty
                                      ,IRD.TransactionQty AS PreviousQty,IRD.LotNumber
                                      ,IRD.ShortageRatePercent AS ShortageRate,IRD.ShortageValue,IRD.RejectRatePercent AS RejectionRate,IRD.RejectValue AS RejectionValue,IRD.RejectClamPercent RejectionClamRate,IR.CheckedBy,PID.Description MaterialDetail,c.UserName CountryName,C.Id CountryId ,IRD.GRNQty-IRD.ShortageQty AS NetQty,MS.Id MaterialStorageId,IRD.GrossAmount,IRD.DiscountAmount,IRD.QualityStatus
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
                                  LEFT jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id and ird.InventoryReceiveId='" + inveReveiveId + @"'
                                  LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId   
                                  LEFT JOIN (select PODetailsId,  Sum(TransactionQty) as OtherReceived 
                                  from trn.InventoryReceiveDetail where InventoryReceiveId not in('" + inveReveiveId + "') --AND POid='" + POID + @"'
                                  Group By PODetailsId) AS Pre on pre.PODetailsId=IRD.PODetailsId

                        JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                        JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID=PID.RequisitionDetailId
                        left join scs.country C ON C.Id=IM.CountryId 
						LEFT JOIN  [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                        WHERE IRD.InventoryReceiveId=@inventoryReceiveId and IM.MaterialMasterId IS not null 
                        UNION ALL 
                        SELECT IM.Id,IRD.InventoryReceiveId, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
                        ,REPLACE(CONVERT(CHAR(11), PID.AddedDate, 106),' ','-') AS AddedDate, MGM.UserName AS MaterialGroupMasterName, IM.MaterialMasterId
                        , MM.UserName, IM.ArticleId, ART.StandardName, IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics, IM.FirstCharacteristicsValueId
                        , FCV.UserName AS FirstCharacteristicsValue , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics, IM.SecondCharacteristicsValueId
                        , SCV.UserName AS SecondCharacteristicsValue , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics, IM.ThirdCharacteristicsValueId
                        , TCV.UserName AS ThirdCharacteristicsValue   , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM, IRD.MaterialTranRate AS TransactionRate
                        , CU.Code AS CurrencyName, IR.ToCurrencyRate, (IRD.TransactionQty*IRD.MaterialTranRate) AS TrnAmount, IRD.ToTalMaterialBooksCurrencyAmount AS BaseAmount
                        , IRD.TotalTaxAmount AS BaseTaxAmount, TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
                        , IRD.ChargesTranAmount AS ChargesAmount	,ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
                        , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount, PID.TransactionQty AS POQty,ISNULL(Pre.OtherReceived,0) OtherReceived
                        ,IRD.GRNQty TransactionQty  ,(PID.TransactionQty-IRD.TransactionQty-ISNULL(Pre.OtherReceived,0)) AS Balance   ,IRD.TransactionUoMId,IRD.BaseUOMId   
                        ,IRD.TotalMaterialTranAmount,IRD.ToTalMaterialBooksCurrencyAmount AS TotalMaterialBaseAmount,IRD.ShortageQty,IRD.RejectionQty,IRD.ApprovedQty 
                        ,IRD.TransactionQty AS PreviousQty,IRD.LotNumber
                                               ,IRD.ShortageRatePercent AS ShortageRate,IRD.ShortageValue,IRD.RejectRatePercent AS RejectionRate
                        ,IRD.RejectValue AS RejectionValue,IRD.RejectClamPercent RejectionClamRate,IR.CheckedBy,PID.Description MaterialDetail,c.UserName CountryName
                        ,C.Id CountryId,IRD.GRNQty-IRD.ShortageQty AS NetQty,MS.Id MaterialStorageId,IRD.GrossAmount,IRD.DiscountAmount,IRD.QualityStatus 
                        from TRN.InventoryMaterial AS IM left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id 
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id   
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id  
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id  
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id  
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id   
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id    
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id   
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id   
                        LEFT jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id and ird.InventoryReceiveId='" + inveReveiveId + "' " +
                        "LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId     " +
                        "LEFT JOIN (select PODetailsId,  Sum(TransactionQty) as OtherReceived 	from trn.InventoryReceiveDetail where InventoryReceiveId not in('" + inveReveiveId + "') AND POid='" + POID + @"' Group By PODetailsId) AS Pre 
                        on pre.PODetailsId=IRD.PODetailsId  
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id 
                        LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id   
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id 
                        LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID=PID.RequisitionDetailId 
                        left join scs.country C ON C.Id=IM.CountryId 
                        LEFT JOIN  [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId 
                        WHERE IRD.InventoryReceiveId=@inventoryReceiveId  and IM.MaterialMasterId IS  null";
                }
                else
                {
                    parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
                                        , @totalReceiveAmount DECIMAL(18, 4)=0
                                        , @totalServiceAmount DECIMAL(18, 4)=0
                                        , @totalSvcTaxAmount DECIMAL(18, 4)=0
                                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                                        SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@inventoryReceiveId)
                                        SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
                                        SELECT IM.Id, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
                                        ,REPLACE(CONVERT(CHAR(11), PID.AddedDate, 106),' ','-') AS AddedDate
                                        , MGM.UserName AS MaterialGroupMasterName
                                        , IM.MaterialMasterId, MM.UserName
                                        , IM.ArticleId, ART.StandardName
                                        , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                                        , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                                        , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                                        , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                                        , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                                        , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
                                        , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
                                        , IRD.MaterialTranRate AS TransactionRate
                                        , CU.Code AS CurrencyName, IR.ToCurrencyRate
                                        , (IRD.TransactionQty*IRD.MaterialTranRate) AS TrnAmount
                                        , IRD.ToTalMaterialBooksCurrencyAmount AS BaseAmount
                                        , IRD.TotalTaxAmount AS BaseTaxAmount
                                        , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
                                        , IRD.ChargesTranAmount AS ChargesAmount	                      
                                        ,ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
                                        , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
                                        , IRD.CountryId
                                        ,POD.TransactionQty AS OriginalPOQty
                                        ,isnull(PID.TransactionQty,0) AS POQty
                                        ,isnull(Pre.OtherReceived,0) OtherReceived                    
                                        ,isnull(IRD.TransactionQty,0)  TransactionQty                       
                                        --,(isnull(PID.TransactionQty,0)-isnull(IRD.TransactionQty,0)) AS Balance     
										,(POD.TransactionQty-(Isnull((isnull(PID.TransactionQty,0) + isnull(Pre.OtherReceived,0)),0))) AS Balance                   
                                        ,IRD.TransactionUoMId
                                        ,IRD.BaseUOMId   
                                        ,IRD.TotalMaterialTranAmount
                                        ,IRD.TotalMaterialBooksCurrencyAmount AS TotalMaterialBaseAmount

                                        ,IRD.ShortageQty
                                        ,IRD.RejectionQty
                                        ,IRD.ApprovedQty
                                        ,IRD.TransactionQty AS PreviousQty,IRD.LotNumber
                                        ,IRD.ShortageRatePercent AS ShortageRate,IRD.ShortageValue,IRD.RejectRatePercent AS RejectionRate,IRD.RejectValue AS RejectionValue
                                        ,IRD.RejectClamPercent RejectionClamRate,IR.CheckedBy
                                        --,PID.Description MaterialDetail
                                       ,IRD.PurchaseDocumentAcceptanceId,IRD.PurchaseDocumentAcceptanceDetailId,MS.Id MaterialStorageId
                                        FROM TRN.InventoryMaterial AS IM
                                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                                        LEFT jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id and ird.InventoryReceiveId='" + inveReveiveId + @"'
                                        LEFT JOIN [TRN].[PurchaseDocAcceptanceDetail] AS PID on PID.Id=IRD.PurchaseDocumentAcceptanceDetailId
                                        LEFT JOIN (select PODetailsId,  Sum(TransactionQty) as OtherReceived 
			                                        from trn.InventoryReceiveDetail where InventoryReceiveId not in('" + inveReveiveId + "') --AND POid='" + POID + @"'
			                                        Group By PODetailsId) AS Pre on pre.PODetailsId=IRD.PODetailsId

                                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                                        LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                                        LEFT JOIN (Select POD.id, sum(POD.TransactionQty) TransactionQty from  TRN.PurchaseOrderDetail AS POD 
													LEFT JOIN  [TRN].[PurchaseOrder] AS PO ON PO.Id=POD.InventoryReceiveId
													 Group By POD.id
													)POD ON POD.Id=PID.PODetailId
                                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                        --LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID=PID.RequisitionDetailId
										LEFT JOIN  [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                                        WHERE IRD.InventoryReceiveId=@inventoryReceiveId and IM.MaterialMasterId IS not null";

                }
                return _sqlRepository.GetDifferentGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GRNDetailsData(string inveReveiveId, string POID)

		{
			//var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{

				var sql = @"DECLARE @inventoryReceiveId VARCHAR(10) = '"+ inveReveiveId + @"'
									,@totalReceiveAmount DECIMAL(18, 4) = 0
									,@totalServiceAmount DECIMAL(18, 4) = 0
									,@totalSvcTaxAmount DECIMAL(18, 4) = 0

								SET @totalReceiveAmount = (
										SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)), 1)
										FROM [TRN].[InventoryReceiveDetail]
										WHERE InventoryReceiveId = @inventoryReceiveId
										)
								SET @totalServiceAmount = (
										SELECT ISNULL(SUM(ISNULL(Amount, 0)), 0)
										FROM [TRN].[InventoryService]
										WHERE InventoryReceiveId = @inventoryReceiveId
										)
								SET @totalSvcTaxAmount = (
										SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)), 0)
										FROM [TRN].[InventoryReceiveTax]
										WHERE InventoryReceiveId = @inventoryReceiveId
											AND InventoryServiceId <> ''
										)

								SELECT IM.Id
									,IRD.Id AS InventoryReceiveDetailId
									,IRD.id AS RCBDetailsID
									,IRD.PODetailsId
									,IRD.POId
									,IRD.InventoryReceiveId
									,REPLACE(CONVERT(CHAR(11), PID.AddedDate, 106), ' ', '-') AS AddedDate
									,MGM.UserName AS MaterialGroupName
									,IM.MaterialMasterId
									,MM.UserName MaterialName
									,ART.StandardName Article
									,IM.FirstCharacteristicsId
									,FC.UserName AS SKU1 
									,IM.FirstCharacteristicsValueId
									,FCV.UserName AS FirstCharacteristicsValue
									,IM.SecondCharacteristicsId
									,SC.UserName AS SKU2
									,IM.SecondCharacteristicsValueId
									,SCV.UserName AS SecondCharacteristicsValue
									,IM.ThirdCharacteristicsId
									,TC.UserName AS SKU3
									,IM.ThirdCharacteristicsValueId
									,TCV.UserName AS ThirdCharacteristicsValue
									,IRD.TransactionUoMId
									,TUoM.UserName AS TransactionUoM
									,IRD.MaterialTranRate AS TransactionRate
									,CU.Code AS CurrencyName
									,IR.ToCurrencyRate
									,(IRD.TransactionQty * IRD.MaterialTranRate) AS TrnAmount
									,IRD.TotalMaterialBooksCurrencyAmount AS BaseAmount
									--,IRD.TotalTaxAmount AS BaseTaxAmount
	
									--,TaxAmount = (
									--	SELECT SUM(TaxAmount)
										--FROM [TRN].[InventoryReceiveTax]
										--WHERE InventoryReceiveDetailId = IRD.Id
										--)
									--,IRD.ChargesTranAmount AS ChargesAmount
									--,ServiceCharge = (@totalServiceAmount / ISNULL(NULLIF(@totalReceiveAmount, 0), 1)) * IRD.MaterialTranAmount
									--,ServiceTax = (@totalSvcTaxAmount / ISNULL(NULLIF(@totalReceiveAmount, 0), 1)) * IRD.MaterialTranAmount
									--,IRD.CountryId
									--,PID.TransactionQty AS POQty
									--,ISNULL(Pre.OtherReceived, 0) OtherReceived
									,IRD.TransactionQty
									,(PID.TransactionQty - IRD.TransactionQty - ISNULL(Pre.OtherReceived, 0)) AS Balance
									,IRD.TransactionUoMId
									,IRD.BaseUOMId
									,IRD.TotalMaterialTranAmount
									,IRD.TotalMaterialBooksCurrencyAmount
									--,IRD.ShortageQty
									--,IRD.RejectionQty
									--,IRD.ApprovedQty
								--,IRD.ShortageQty
									--,IRD.RejectionQty
									--,IRD.ApprovedQty
									--,IRD.TransactionQty AS PreviousQty
									--,IRD.ShortageRatePercent AS ShortageRate
									--,IRD.ShortageValue
									--,IRD.RejectRatePercent AS RejectionRate
									--,IRD.RejectValue AS RejectionValue
									--,IRD.RejectClamPercent RejectionClamRate
									,IR.CheckedBy
									,MRD.MaterialDetail
                                    ,C.Id,C.UserName CountryName,IRD.GrossAmount,IRD.DiscountAmount,MOI.MasterOrderId MasterOrderNo,IRD.MaterialFor
								FROM TRN.InventoryMaterial AS IM
								LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
								LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
								LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
								LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
								LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
								LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
								LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
								LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId = IM.Id									
								LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID ON PID.Id = IRD.PODetailsId
								LEFT JOIN (
									SELECT PODetailsId
										,Sum(TransactionQty) AS OtherReceived
									FROM trn.InventoryReceiveDetail	
									GROUP BY PODetailsId
									) AS Pre ON pre.PODetailsId = IRD.PODetailsId
								LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
								LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id
								LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
								LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID = PID.RequisitionDetailId
                                Left Join SCS.Country C ON C.Id=IM.CountryId
								 LEFT JOIN [TRN].[MasterOrderItem] MOI ON IRD.MasterOrderItemId=MOI.Id
								WHERE IM.MaterialMasterId IS NOT NULL 
	                     AND ISNULL(IR.[Status],'')<>'Posting' 
                         AND IR.OpeningBalanceId IS NULL   
                         And IR.IsApproved =0 
                         AND IR.CheckedBy is not null  
                         AND IR.CheckedByStatus='ForChecked' 

								UNION ALL

								SELECT IM.Id
									,IRD.Id AS InventoryReceiveDetailId
									,IRD.id AS RCBDetailsID
									,IRD.PODetailsId
									,IRD.POId
									,IRD.InventoryReceiveId
									,REPLACE(CONVERT(CHAR(11), PID.AddedDate, 106), ' ', '-') AS AddedDate
									,MGM.UserName AS MaterialGroupName
									,IM.MaterialMasterId
									,MM.UserName MaterialName
									,ART.StandardName Article
									,IM.FirstCharacteristicsId
									,FC.UserName AS SKU1
									,IM.FirstCharacteristicsValueId
									,FCV.UserName AS FirstCharacteristicsValue
									,IM.SecondCharacteristicsId
									,SC.UserName AS SKU2
									,IM.SecondCharacteristicsValueId
									,SCV.UserName AS SecondCharacteristicsValue
									,IM.ThirdCharacteristicsId
									,TC.UserName AS SKU3
									,IM.ThirdCharacteristicsValueId
									,TCV.UserName AS ThirdCharacteristicsValue
									,IRD.TransactionUoMId
									,TUoM.UserName AS TransactionUoM
									,IRD.MaterialTranRate AS TransactionRate
									,CU.Code AS CurrencyName
									,IR.ToCurrencyRate
									,(IRD.TransactionQty * IRD.MaterialTranRate) AS TrnAmount
									,IRD.TotalMaterialBooksCurrencyAmount AS BaseAmount
									--,IRD.TotalTaxAmount AS BaseTaxAmount
	
									--,TaxAmount = (
									--	SELECT SUM(TaxAmount)
										--FROM [TRN].[InventoryReceiveTax]
										--WHERE InventoryReceiveDetailId = IRD.Id
										--)
									--,IRD.ChargesTranAmount AS ChargesAmount
									--,ServiceCharge = (@totalServiceAmount / ISNULL(NULLIF(@totalReceiveAmount, 0), 1)) * IRD.MaterialTranAmount
									--,ServiceTax = (@totalSvcTaxAmount / ISNULL(NULLIF(@totalReceiveAmount, 0), 1)) * IRD.MaterialTranAmount
									--,IRD.CountryId
									--,PID.TransactionQty AS POQty
									--,ISNULL(Pre.OtherReceived, 0) OtherReceived
									,IRD.TransactionQty
									,(PID.TransactionQty - IRD.TransactionQty - ISNULL(Pre.OtherReceived, 0)) AS Balance
									,IRD.TransactionUoMId
									,IRD.BaseUOMId
									,IRD.TotalMaterialTranAmount
									,IRD.TotalMaterialBooksCurrencyAmount
									--,IRD.ShortageQty
									--,IRD.RejectionQty
									--,IRD.ApprovedQty
									--,IRD.TransactionQty AS PreviousQty
									--,IRD.ShortageRatePercent AS ShortageRate
									--,IRD.ShortageValue
									--,IRD.RejectRatePercent AS RejectionRate
									--,IRD.RejectValue AS RejectionValue
									--,IRD.RejectClamPercent RejectionClamRate
									,IR.CheckedBy
									,MRD.MaterialDetail
	                                ,C.Id,C.UserName CountryName,IRD.GrossAmount,IRD.DiscountAmount,MOI.MasterOrderId MasterOrderNo,IRD.MaterialFor
								FROM TRN.InventoryMaterial AS IM
								LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
								LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
								LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
								LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
								LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
								LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
								LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
								LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId = IM.Id									
								LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID ON PID.Id = IRD.PODetailsId
								LEFT JOIN (
									SELECT PODetailsId
										,Sum(TransactionQty) AS OtherReceived
									FROM trn.InventoryReceiveDetail	
									GROUP BY PODetailsId
									) AS Pre ON pre.PODetailsId = IRD.PODetailsId
								LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
								LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id
								LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
								LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID = PID.RequisitionDetailId
                                Left Join SCS.Country C ON C.Id=IM.CountryId
								 LEFT JOIN [TRN].[MasterOrderItem] MOI ON IRD.MasterOrderItemId=MOI.Id
								WHERE IM.MaterialMasterId IS NULL";

				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
       

        public GridModel Query1(GridParameter parameters, string inveReveiveId)
		{
			string paramter = "";
			if (inveReveiveId != "")
			{
				if (paramter == "")
					paramter += "IRD.InventoryReceiveId in(" + inveReveiveId + ")";
				else
					paramter += " AND IRD.InventoryReceiveId in(" + inveReveiveId + ")";
			}
			try
			{
				parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)=''
                                        , @totalReceiveAmount DECIMAL(18, 4)=0
                                        , @totalServiceAmount DECIMAL(18, 4)=0
                                        , @totalSvcTaxAmount DECIMAL(18, 4)=0
                                SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(TransactionAmount, 0)),1) FROM [TRN].[PurchaseOrderDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                                SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount - GRNServiceAmount, 0)),0) As Amount FROM [TRN].[POService] WHERE InventoryReceiveId=@inventoryReceiveId)
                                SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
                                SELECT 
                                    --IM.Id
                                    IR.Id AS POID,IRD.Id AS PODetailsID
                                    ,IRD.Id AS InventoryReceiveDetailId
                                    , MGM.UserName AS MaterialGroupMasterName
                                    , IRD.InventoryMaterialId MaterialMasterId
		                        , MM.UserName
                                    ,IRD.MaterialStorageId
                                    ,IRD.BaseUOMId
                                    , IRD.ArticleId, ART.StandardName
                                    , IRD.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                                    , IRD.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                                    , IRD.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                                    , IRD.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                                    , IRD.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                                    , IRD.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
                                    , IRD.TransactionQty AS POQty
                                    , ISNULL(IRD.GRNRcvQty,0) AS GRNRcvQty                           
                                    --,(IRD.TransactionQty - ISNULL(IRD.GRNRcvQty,0)) AS TransactionQty
                                    ,'' AS TransactionQty
                                    ,(IRD.TransactionQty-IRD.GRNRcvQty) As Balance
                                    ,ISNULL(IRD.QtyStatus,0) QtyStatus
                                    , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM, IRD.TransactionRate, CU.Code AS CurrencyName, IR.ToCurrencyRate

                                    ,0 AS TrnAmount  
                                    ,0 AS BaseTaxAmount
                                    ,0 AS TaxAmount
                                , 0 AS ChargesAmount
                                    ,0 AS  ServiceCharge
                                    , 0 AS ServiceTax
                                , IRD.CountryId
                                    ,'True' enableid
                                    ,null POMaterialTaxList                            
                                    ,0 AS TotalMaterialTranAmount
                                    , 0 AS ToTalMaterialBooksCurrencyAmount
                                ,IR.InvoicingByAddress,IR.DeliveryByAddress
                                ,IRD.RequisitionId
                                    ,IRD.RequisitionDetailId
                                ,MRD.MaterialDetail
                                ,null AS [check]
                                    ,0 ShortageQty
                                ,0 RejectionQty
                                ,0 POClosStatus ,C.UserName CountryName,c.Id CountryId
                                FROM TRN.PurchaseOrderDetail AS IRD
                                JOIN MST.MaterialMaster AS MM ON IRD.InventoryMaterialId=MM.Id
                                LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                LEFT JOIN MST.MaterialMasterArticle AS ART ON IRD.ArticleId=ART.Id
                                LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId=FC.Id
                                LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId=SC.Id
                                LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId=TC.Id
                                LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId=FCV.Id
                                LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId=SCV.Id
                                LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId=TCV.Id
                            -- JOIN [TRN].[PurchaseOrderDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                                LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                                LEFT JOIN [TRN].[PurchaseOrder] AS IR ON IRD.InventoryReceiveId=IR.Id
                                LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                LEFT join [trn].MaterialRequsitionDetails MRD on MRD.Id=IRD.RequisitionDetailId
		                        left join scs.country C On C.Id=IRD.CountryId
                                WHERE IRD.QtyStatus=0 and IRD.InventoryMaterialId is not null AND " + paramter + @"  
								 
		                        union all  
                        SELECT  IR.Id AS POID,IRD.Id AS PODetailsID 
                        ,IRD.Id AS InventoryReceiveDetailId   ,
                        MGM.UserName AS MaterialGroupMasterName,
                        IRD.InventoryMaterialId MaterialMasterId
                        , MM.UserName   
                        ,IRD.MaterialStorageId   
                        ,IRD.BaseUOMId   
                        , IRD.ArticleId
                        , ART.StandardName  
                        , IRD.FirstCharacteristicsId
                        , FC.UserName AS FirstCharacteristics  
                        , IRD.FirstCharacteristicsValueId
                        , FCV.UserName AS FirstCharacteristicsValue                            
                        , IRD.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics                            
                        , IRD.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue                            
                        , IRD.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics                            
                        , IRD.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                            
                        , IRD.TransactionQty AS POQty                            
                        , ISNULL(IRD.GRNRcvQty,0) AS GRNRcvQty   ,'' AS TransactionQty                             
                        ,(IRD.TransactionQty-IRD.GRNRcvQty) As Balance                            
                        ,ISNULL(IRD.QtyStatus,0) QtyStatus                            
                        , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
                        , IRD.TransactionRate, CU.Code AS CurrencyName
                        , IR.ToCurrencyRate ,0 AS TrnAmount                              
                        ,0 AS BaseTaxAmount                            
                        ,0 AS TaxAmount	                        
                        , 0 AS ChargesAmount                            
                        ,0 AS  ServiceCharge                            
                        , 0 AS ServiceTax	                        
                        , IRD.CountryId                            
                        ,'True' enableid                            
                        ,null POMaterialTaxList 
                        ,0 AS TotalMaterialTranAmount                           
                        , 0 AS ToTalMaterialBooksCurrencyAmount
                        ,IR.InvoicingByAddress,IR.DeliveryByAddress   
                        ,IRD.RequisitionId,IRD.RequisitionDetailId
                        ,MRD.MaterialDetail,null AS [check]  
                        ,0 ShortageQty
                        ,0 RejectionQty  ,0 POClosStatus
                        ,C.UserName CountryName,c.Id CountryId
                        FROM TRN.PurchaseOrderDetail AS IRD  left JOIN MST.MaterialMaster AS MM ON IRD.InventoryMaterialId=MM.Id    
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id   
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IRD.ArticleId=ART.Id 
                        LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId=FC.Id 
                        LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId=SC.Id     
                        LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId=TC.Id 
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId=FCV.Id    
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId=SCV.Id   
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId=TCV.Id  
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id  
                        left JOIN [TRN].[PurchaseOrder] AS IR ON IRD.InventoryReceiveId=IR.Id   
                        Left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id 
                        left join [trn].MaterialRequsitionDetails MRD on MRD.Id=IRD.RequisitionDetailId  
                        left join scs.country C On C.Id=IRD.CountryId
                        WHERE  IRD.QtyStatus=0  and IRD.InventoryMaterialId is null AND " + paramter + @"";
				return _sqlRepository.GetDifferentGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}

		}

        public GridModel GetIssueMaterial(GridParameter parameters, string issueId, string companyId)
		{
			try
			{
				parameters.CmdText = @"DECLARE  @issueId varchar(10)='" + issueId + "', @companyId varchar(10)='" + companyId + @"'
                                SELECT IR.Id InventoryIssueId,IRD.Id InventoryIssueDetailId,IR.CompanyGroupId
                                ,IR.CompanyId
                                ,Plant.GSTIN 
								,IR.Id PONumber                                 
                                ,REPLACE(Convert(VARCHAR(11), IR.IssueDate, 106), ' ', '-') AS PODate
		                        ,IOM.MaterialMasterId
		                        ,IR.AddedBy
		                        ,IR.AddedDate
		                        ,IR.UpdatedBy
		                        ,IR.UpdatedDate
	                          ,MM.UserName 
	                          ,MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMasterName
	                          ,IOM.ArticleId
	                          ,MMA.StandardName 
	                          ,FC.Id FirstCharId
	                          ,FC.UserName FirstChar
                              ,IOM.FirstCharacteristicsValueId
	                          ,FCV.UserName AS FirstCharacteristicsValue
                              ,IOM.SecondCharacteristicsValueId
	                          ,SCV.UserName AS SecondCharacteristicsValue
	                          ,IOM.ThirdCharacteristicsValueId
	                          ,TCV.UserName AS ThirdCharacteristicsValue
	                          ,SC.Id SecondCharId
	                          ,SC.UserName SecondChar
	                          ,TC.Id ThirdCharId
	                          ,TC.UserName ThirdChar
	                          ,ROUND(IRD.TransactionQty, 2) TransactionQty
	                          ,ROUND(IRD.PolicyRate, 2) TransactionRate
	                          ,ROUND((IRD.PolicyAmount), 2) AS TrnAmount
	                          ,IRD.BaseUOMId
	                          ,TUoM.UserName AS TransactionUoM
							  --,BI.UserName BudgetName
							  --,AI.UserName ActivityName
							  ,GLGeneralInfoId=CASE WHEN IRD.BudgetMasterId<>'' THEN BMI.GLGeneralInfoId ELSE  MGGL.ExpenseGLId END
								,GLGeneralInfoCode=CASE WHEN IRD.BudgetMasterId<>'' THEN IRD.BudgetMasterId ELSE GL.AccountCode END
								,GLGeneralInfoName=CASE WHEN IRD.BudgetMasterId<>'' THEN GLI.UserName ELSE GL.UserName END
								,GLName=CASE WHEN IRD.BudgetMasterId<>'' THEN GLI.AccountCode +'-'+ GLI.UserName ELSE GL.AccountCode +'-'+ GL.UserName END
	                            ,BudgetMasterId=CASE WHEN IRD.BudgetMasterId<>'' THEN IRD.BudgetMasterId ELSE MGGL.ExpenseBudgetMasterId END
								,BudgetCode=CASE WHEN IRD.BudgetMasterId<>'' THEN BI.Code ELSE B.Code END
								,BudgetName=CASE WHEN IRD.BudgetMasterId<>'' THEN BI.UserName ELSE B.UserName END
								,ActivityId=CASE WHEN IRD.ActivityId<>'' THEN IRD.ActivityId ELSE MGGL.ExpenseActivityId END
								,ActivityCode=CASE WHEN IRD.ActivityId<>'' THEN AI.Code ELSE A.Code END
								,ActivityName=CASE WHEN IRD.ActivityId<>'' THEN AI.UserName ELSE A.UserName END
                                ,IRD.BudgetMasterId IssueBudgetMasterId,IRD.ActivityId IssueActivityId
								,MGGL.ExpenseBudgetMasterId,MGGL.ExpenseActivityId
                              FROM TRN.InventoryIssue IR
                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                         LEFT JOIN trn.InventoryIssueDetail IRD ON IR.Id = IRD.InventoryIssueId						                                   
                         LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
                         INNER JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
                         LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                         LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
                         LEFT JOIN HKP.Characteristics AS FC ON IOM.FirstCharacteristicsId = FC.Id
                         LEFT JOIN HKP.Characteristics AS SC ON IOM.SecondCharacteristicsId = SC.Id
                         LEFT JOIN HKP.Characteristics AS TC ON IOM.ThirdCharacteristicsId = TC.Id
                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON IOM.FirstCharacteristicsValueId = FCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON IOM.SecondCharacteristicsValueId = SCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON IOM.ThirdCharacteristicsValueId = TCV.Id
                         LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
						 LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
		                        AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGGL.ExpenseGLId=GL.Id
                        LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.ExpenseBudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON MGGL.ExpenseActivityId= A.Id
                        LEFT JOIN[MST].[BudgetMaster] AS BMI ON IRD.BudgetMasterId= BMI.Id
                        LEFT JOIN [HKP].[Budget] AS BI ON BMI.BudgetId= BI.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GLI ON BMI.GLGeneralInfoId=GLI.Id
                        LEFT JOIN [HKP].[Activity] AS AI ON IRD.ActivityId= AI.Id
                         WHERE IR.Id=@issueId";
				return _sqlRepository.GetDifferentGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
        public GridModel GetIssueReturnMaterial(GridParameter parameters, string issueId, string companyId)
        {
            try
            {
                parameters.CmdText = @"DECLARE  @issueId varchar(10)='" + issueId + "', @companyId varchar(10)='" + companyId + @"'
                                SELECT IR.Id InventoryIssueId,IRD.Id InventoryIssueDetailId,IR.CompanyGroupId
                                ,IR.CompanyId
                                ,Plant.GSTIN 
								,IR.Id PONumber                                 
                                ,REPLACE(Convert(VARCHAR(11), IR.IssueDate, 106), ' ', '-') AS PODate
		                        ,IOM.MaterialMasterId
		                        ,IR.AddedBy
		                        ,IR.AddedDate
		                        ,IR.UpdatedBy
		                        ,IR.UpdatedDate
	                          ,MM.UserName 
	                          ,MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMasterName
	                          ,IOM.ArticleId
	                          ,MMA.StandardName 
	                          ,FC.Id FirstCharId
	                          ,FC.UserName FirstChar
                              ,IOM.FirstCharacteristicsValueId
	                          ,FCV.UserName AS FirstCharacteristicsValue
                              ,IOM.SecondCharacteristicsValueId
	                          ,SCV.UserName AS SecondCharacteristicsValue
	                          ,IOM.ThirdCharacteristicsValueId
	                          ,TCV.UserName AS ThirdCharacteristicsValue
	                          ,SC.Id SecondCharId
	                          ,SC.UserName SecondChar
	                          ,TC.Id ThirdCharId
	                          ,TC.UserName ThirdChar
	                          ,ROUND(IRDH.Qty, 2) TransactionQty
	                          ,ROUND(IRDH.Rate, 2) TransactionRate
	                          ,ROUND((IRDH.TotalAmount), 2) AS TrnAmount
	                          ,IRD.BaseUOMId
	                          ,TUoM.UserName AS TransactionUoM
							
                         ,GLGeneralInfoId=CASE WHEN IRD.BudgetMasterId<>'' THEN BMI.GLGeneralInfoId ELSE  MGGL.ExpenseGLId END
                        ,GLGeneralInfoCode=CASE WHEN IRD.BudgetMasterId<>'' THEN IRD.BudgetMasterId ELSE GL.AccountCode END
                        ,GLGeneralInfoName=CASE WHEN IRD.BudgetMasterId<>'' THEN GLI.UserName ELSE GL.UserName END
                        ,GLName=CASE WHEN IRD.BudgetMasterId<>'' THEN GLI.AccountCode +'-'+ GLI.UserName ELSE GL.AccountCode +'-'+ GL.UserName END
                        ,BudgetMasterId=CASE WHEN IRD.BudgetMasterId<>'' THEN IRD.BudgetMasterId ELSE MGGL.ExpenseBudgetMasterId END
                        ,BudgetCode=CASE WHEN IRD.BudgetMasterId<>'' THEN BI.Code ELSE B.Code END
                        ,BudgetName=CASE WHEN IRD.BudgetMasterId<>'' THEN BI.UserName ELSE B.UserName END
                        ,ActivityId=CASE WHEN IRD.ActivityId<>'' THEN IRD.ActivityId ELSE MGGL.ExpenseActivityId END
                        ,ActivityCode=CASE WHEN IRD.ActivityId<>'' THEN AI.Code ELSE A.Code END
                        ,ActivityName=CASE WHEN IRD.ActivityId<>'' THEN AI.UserName ELSE A.UserName END
                        ,IRD.BudgetMasterId IssueBudgetMasterId,IRD.ActivityId IssueActivityId
                        ,MGGL.ExpenseBudgetMasterId,MGGL.ExpenseActivityId
                              FROM TRN.InventoryIssueReturn IR
                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
						  LEFT JOIN trn.InventoryIssueReturnHistory IRDH ON IR.Id = IRDH.InventoryIssueReturnId						                                   
                         LEFT JOIN trn.InventoryIssueDetail IRD ON IRD.Id = IRDH.InventoryIssueDetailId						                                   
                         LEFT JOIN trn.InventoryMaterial AS IOM ON IRDH.InventoryMaterialId = IOM.Id
                         INNER JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
                         LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                         LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
                         LEFT JOIN HKP.Characteristics AS FC ON IOM.FirstCharacteristicsId = FC.Id
                         LEFT JOIN HKP.Characteristics AS SC ON IOM.SecondCharacteristicsId = SC.Id
                         LEFT JOIN HKP.Characteristics AS TC ON IOM.ThirdCharacteristicsId = TC.Id
                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON IOM.FirstCharacteristicsValueId = FCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON IOM.SecondCharacteristicsValueId = SCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON IOM.ThirdCharacteristicsValueId = TCV.Id
                         LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
						 LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
		                        AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGGL.ExpenseGLId=GL.Id
                        LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.ExpenseBudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON MGGL.ExpenseActivityId= A.Id
                        LEFT JOIN[MST].[BudgetMaster] AS BMI ON IRD.BudgetMasterId= BMI.Id
                        LEFT JOIN [HKP].[Budget] AS BI ON BMI.BudgetId= BI.Id
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GLI ON BMI.GLGeneralInfoId=GLI.Id
                        LEFT JOIN [HKP].[Activity] AS AI ON IRD.ActivityId= AI.Id
                         WHERE IR.Id=@issueId";
                return _sqlRepository.GetDifferentGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public GridModel GetPayableShortageMaterial(GridParameter parameters, string inveReveiveId)
		{
			try
			{
				parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
                                        , @totalReceiveAmount DECIMAL(18, 4)=0
                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SELECT IM.Id, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
                            , MGM.UserName AS MaterialGroupMasterName
                            , IM.MaterialMasterId, MM.UserName
                            , IM.ArticleId, ART.StandardName
                            , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
                            , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
                            , IRD.MaterialTranRate AS TransactionRate
                            , CU.Code AS CurrencyName, IR.ToCurrencyRate
                            , (IRD.ShortageQty*IRD.MaterialTranRate) AS TrnAmount
                              --, (IRD.ShortageQty*IRD.MaterialTranRate) +(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id) AS BaseAmount
                              , (IRD.ShortageQty*IRD.MaterialTranRate) AS BaseAmount
	                        , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
	                        , IRD.CountryId
                             ,PID.TransactionQty AS POQty
                             ,ISNULL(Pre.OtherReceived,0) OtherReceived	                       
                            ,IRD.TransactionQty                         
                            ,IRD.ShortageQty                         
							,(PID.TransactionQty-IRD.TransactionQty-ISNULL(Pre.OtherReceived,0)) AS Balance                   
					        ,IRD.TransactionUoMId
							,IRD.BaseUOMId   
					  from TRN.InventoryMaterial AS IM
                        JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                        LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id and ird.InventoryReceiveId=@inventoryReceiveId
                        LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId   
                        LEFT JOIN (select PODetailsId,  Sum(TransactionQty) as OtherReceived 
						from trn.InventoryReceiveDetail where InventoryReceiveId not in(@inventoryReceiveId)
                        Group By PODetailsId) AS Pre on pre.PODetailsId=IRD.PODetailsId

                        JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                        JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        WHERE IRD.InventoryReceiveId=@inventoryReceiveId AND IRD.ShortageQty >0";
				return _sqlRepository.GetDifferentGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public GridModel GetPayableRejectMaterial(GridParameter parameters, string inveReveiveId)
		{
			try
			{
				parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
                                        , @totalReceiveAmount DECIMAL(18, 4)=0
                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SELECT IM.Id, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
                            , MGM.UserName AS MaterialGroupMasterName
                            , IM.MaterialMasterId, MM.UserName
                            , IM.ArticleId, ART.StandardName
                            , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
                            , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
                            , IRD.MaterialTranRate AS TransactionRate
                            , CU.Code AS CurrencyName, IR.ToCurrencyRate
                            , (IRD.ShortageQty*IRD.MaterialTranRate) AS TrnAmount
                              --, (IRD.ShortageQty*IRD.MaterialTranRate) +(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id) AS BaseAmount
                              , (IRD.ShortageQty*IRD.MaterialTranRate) AS BaseAmount
	                        , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
	                        , IRD.CountryId
                             ,PID.TransactionQty AS POQty
                             ,ISNULL(Pre.OtherReceived,0) OtherReceived	                       
                            ,IRD.TransactionQty                         
                            ,IRD.ShortageQty                         
							,(PID.TransactionQty-IRD.TransactionQty-ISNULL(Pre.OtherReceived,0)) AS Balance                   
					        ,IRD.TransactionUoMId
							,IRD.BaseUOMId   
					  from TRN.InventoryMaterial AS IM
                        JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                        LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id and ird.InventoryReceiveId=@inventoryReceiveId
                        LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId   
                        LEFT JOIN (select PODetailsId,  Sum(TransactionQty) as OtherReceived 
						from trn.InventoryReceiveDetail where InventoryReceiveId not in(@inventoryReceiveId)
                        Group By PODetailsId) AS Pre on pre.PODetailsId=IRD.PODetailsId

                        JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                        JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        WHERE IRD.InventoryReceiveId=@inventoryReceiveId AND IRD.RejectionQty >0";
				return _sqlRepository.GetDifferentGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public GridModel QueryForPurchaseOrderDetail(GridParameter parameters, string inveReveiveId)
		{
			try
			{
				parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
	                                  , @totalReceiveAmount DECIMAL(18, 4)=0
	                                  , @totalServiceAmount DECIMAL(18, 4)=0
	                                  , @totalSvcTaxAmount DECIMAL(18, 4)=0
                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(TransactionAmount, 0)),1) FROM [TRN].[PurchaseOrderDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
                        SELECT IM.Id, IRD.Id AS InventoryReceiveDetailId
                            , MGM.UserName AS MaterialGroupMasterName
                            , IM.MaterialMasterId, MM.UserName
                            , IM.ArticleId, ART.StandardName
                            , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
                            , ROUND(IRD.TransactionQty,2) TransactionQty, IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM, ROUND(IRD.TransactionRate,2) TransactionRate , CU.Code AS CurrencyName, IR.ToCurrencyRate
                            , ROUND((IRD.TransactionQty*IRD.TransactionRate),2) AS TrnAmount
                            , IRD.BaseAmount
                            , IRD.TotalTaxAmount AS BaseTaxAmount
	                        , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId=IRD.Id)
	                        , IRD.ChargesAmount
	                        , ServiceCharge=(@totalServiceAmount/@totalReceiveAmount)*IRD.TransactionAmount
	                        , ServiceTax=(@totalSvcTaxAmount/@totalReceiveAmount)*IRD.TransactionAmount
	                        , IRD.CountryId
                        FROM TRN.InventoryMaterial AS IM
                        JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                        JOIN [TRN].[PurchaseOrderDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                        JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                        JOIN [TRN].[PurchaseOrder] AS IR ON IRD.InventoryReceiveId=IR.Id
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        WHERE IRD.InventoryReceiveId=@inventoryReceiveId";
				return _sqlRepository.GetDifferentGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public IEnumerable<object> GetVendorPayableGLBudgetActivity(string receiveId, string companyId, string plantId)
		{
			var invReceive = _inventoryReceiveRepository.Find(receiveId);
			var companyParty = _companyPartyRepository.Query(r => r.PartyId == invReceive.PartyId && r.PlantId == plantId).Select().FirstOrDefault();
			var sql = @"DECLARE @receiveId varchar(10)='" + receiveId + "', @companyId varchar(10)='" + companyId + "', @plantId varchar(30)='" + plantId + "', @partyAccountGruopId varchar(10)='" + companyParty.PartyAccountGroupId + @"',@countryId varchar(10)

                            SELECT distinct IR.Id,IRD.Id AS InventoryReceiveDetailId,ISNULL(PLC.IsAccepptanceFirst,0) IsAccepptanceFirst, 'Vendor' AS OtherName, 'Cr' AS TrnType ,MM.MaterialGroupMasterId, NULL AS TaxCategoryId
                            ,GLGeneralInfoId =case WHEN MM.IsAsset=1 THEN FAG.VendorReconGLId WHEN ISNULL(PLC.IsAccepptanceFirst,0)=0 THEN MGPGL.GLGeneralInfoId  ELSE MGGL.ClearingAccountGLId  END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=1 THEN GLF.AccountCode WHEN ISNULL(PLC.IsAccepptanceFirst,0)=0 THEN GL.AccountCode  ELSE GLC.AccountCode  END
							,GLGeneralInfoName =case WHEN MM.IsAsset=1 THEN GLF.UserName WHEN ISNULL(PLC.IsAccepptanceFirst,0)=0 THEN GL.UserName  ELSE GLC.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=1 THEN FAG.VendorReconBudgetMasterId WHEN ISNULL(PLC.IsAccepptanceFirst,0)=0 THEN MGPGL.BudgetMasterId  ELSE MGGL.ClearingAccountBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=1 THEN BF.Code WHEN ISNULL(PLC.IsAccepptanceFirst,0)=0 THEN B.Code  ELSE BC.Code END
							,BudgetName =case WHEN MM.IsAsset=1 THEN BF.UserName WHEN ISNULL(PLC.IsAccepptanceFirst,0)=0 THEN B.UserName  ELSE BC.UserName END
							,ActivityId =case WHEN MM.IsAsset=1 THEN FAG.VendorReconActivityId WHEN ISNULL(PLC.IsAccepptanceFirst,0)=0 THEN MGPGL.ActivityId  ELSE MGGL.ClearingAccountActivityId END
							,ActivityCode =case WHEN MM.IsAsset=1 THEN AF.Code WHEN ISNULL(PLC.IsAccepptanceFirst,0)=0 THEN A.Code  ELSE AC.Code END
							,ActivityName =case WHEN MM.IsAsset=1 THEN AF.UserName WHEN ISNULL(PLC.IsAccepptanceFirst,0)=0 THEN A.UserName  ELSE AC.UserName END
							
						FROM [TRN].[InventoryReceiveDetail] AS IRD 
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
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

						LEFT JOIN[HKP].[GLGeneralInfo] AS GLC ON MGGL.ClearingAccountGLId=GLC.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMC ON MGGL.ClearingAccountBudgetMasterId= BMC.Id
						LEFT JOIN [HKP].[Budget] AS BC ON BMC.BudgetId= BC.Id
						LEFT JOIN [HKP].[Activity] AS AC ON MGGL.ClearingAccountActivityId= AC.Id

						LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=IRD.POId
						LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=PO.PurchaseLCId

						WHERE IRD.InventoryReceiveId=@receiveId";
			return _sqlRepository.GetDataCollection(sql);
		}


		public IEnumerable<object> GetInventoryTaxList(string inveReveiveId)
		{
			try
			{
				var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"'

					SELECT  IRT.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRT.TaxAmount * IR.ToCurrencyRate) AS  Dr, NULL Cr
						, SUM(IRT.TaxAmount * IR.ToCurrencyRate) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId
					FROM [TRN].[InventoryReceiveTax] AS IRT
					
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRT.InventoryReceiveId=IR.Id
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE IRT.InventoryReceiveId=@receiveId 
					GROUP BY  IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
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


		public IEnumerable<object> GetInventoryMaterialListForPOUpdate(string inveReveiveId, string InventoryReceiveId, string MaterialMasterId, string InventoryReceiveDetailId)
		{
			try
			{
				var _sql = "";
				var MaterialMasterIda = MaterialMasterId;
				
				_sql = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + InventoryReceiveId + @"'
				                               , @totalReceiveAmount DECIMAL(18, 4)=0
				                               , @totalServiceAmount DECIMAL(18, 4)=0
				                               , @totalSvcTaxAmount DECIMAL(18, 4)=0
				                    SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(TransactionAmount, 0)),1) FROM [TRN].[PurchaseOrderDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
				                    SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[POService] WHERE InventoryReceiveId=@inventoryReceiveId)
				                    SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
				                    SELECT  IR.Id AS POmasterId,IM.InventoryReceiveId
				              ,IM.Id AS InventoryReceiveDetailId
				              ,MGM.UserName AS MaterialGroupMasterName
				              ,IM.InventoryMaterialId AS MaterialMasterId
				              ,MM.UserName
				              ,IM.ArticleId
				              ,ART.StandardName
				              ,IM.FirstCharacteristicsId
				              ,FC.UserName AS FirstCharacteristics
				              ,IM.FirstCharacteristicsValueId
				              ,FCV.UserName AS FirstCharacteristicsValue
				              ,IM.SecondCharacteristicsId
				              ,SC.UserName AS SecondCharacteristics
				              ,IM.SecondCharacteristicsValueId
				              ,SCV.UserName AS SecondCharacteristicsValue
				              ,IM.ThirdCharacteristicsId
				              ,TC.UserName AS ThirdCharacteristics
				              ,IM.ThirdCharacteristicsValueId
				              ,TCV.UserName AS ThirdCharacteristicsValue
				              ,ROUND(IM.TransactionQty, 2) PORaisedQty
				                    ,MRMD.TransactionQty AS ReqQty
				                    ,POREQD.TransactionQty AS POTransactionQty
				                    ,'' AS TransactionQty
				                    ,POREQD.TransactionQty AS PreviousQty
				              ,IM.TransactionUoMId
				              ,TUoM.UserName AS TransactionUoM
				              ,ROUND(IM.TransactionRate, 2) TransactionRate
				              ,CU.Code AS CurrencyName
				              ,IR.ToCurrencyRate
				              ,ROUND((IM.TransactionQty * IM.TransactionRate), 2) AS TrnAmount
				                    ,BaseAmount
				              --,BaseAmount = CASE 
				               -- WHEN IR.IsNonCreditable = 1
				                --  THEN (
					                --    (ROUND((IM.TransactionQty * IM.TransactionRate), 2)) + (
						                --     SELECT SUM(TaxAmount)
						                --     FROM [TRN].[PurchaseOrderTax]
						                --     WHERE InventoryReceiveDetailId = IM.Id
						                --     )
					                --    )
				               -- ELSE IM.BaseAmount
				               -- E
				              ,BaseTaxAmount = (
				               SELECT SUM(TaxAmount)
				               FROM [TRN].[PurchaseOrderTax]
				               WHERE InventoryReceiveDetailId = IM.Id
				               )
				              ,IM.ChargesAmount
				              ,ServiceCharge = (@totalServiceAmount / @totalReceiveAmount) * IM.TransactionAmount
				              ,ServiceTax = (@totalSvcTaxAmount / @totalReceiveAmount) * IM.TransactionAmount
				              ,IM.CountryId
				              ,NULL TaxList
				              ,IR.InvoicingPartyPlantId
				              ,AMD.StateId AS InvoicingStateId
				              ,AMP.StateId AS PlantStateId
				              ,IR.DeliveryInstruction
				              ,IR.SpecialInstruction
				              ,IM.Description
				              ,Replace(CONVERT(VARCHAR(11), IM.DeliveryDate, 106), ' ', '-') DeliveryDate
				              ,Im.RequisitionId
				              ,POREQD.RequisitionDetailId
				                    ,POREQD.Id
				                    ,TaxAmount
				                    --,ReqTransactionQty ReqQty
				                    ,(ReqTransactionQty-ROUND(IM.TransactionQty, 2)) BalanceQty
				                    ,(ROUND(IM.TransactionQty, 2)*ROUND(IM.TransactionRate, 2) ) TransactionAmount
				                    ,MM.HSNCodeId
				                    ,IRD.Id AS GRNId
						,IRD.ApprovedQty AS GRNQty
						,IRD.RejectionQty AS RejectionQty
				                    ,POREQD.Id AS POReqDetailsID
				                    ,GA.allowQty
				             FROM [TRN].[PurchaseOrderDetail] AS IM
				             left JOIN MST.MaterialMaster AS MM ON IM.InventoryMaterialId = MM.Id
				             LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
				             LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
				             LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
				             LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
				             LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
				             LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
				             LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
				             LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
				             JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId = TUoM.Id
				             JOIN [TRN].[PurchaseOrder] AS IR ON IM.InventoryReceiveId = IR.Id
				             JOIN [SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
				             LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id = IR.InvoicingPartyPlantId
				             LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id = PPI.AddressMasterId
				             LEFT JOIN [SCS].[State] AS ST ON ST.Id = AM.StateId
				             LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id = IR.DeliveryPartyPlantId
				             LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id = PPD.AddressMasterId
				             LEFT JOIN [SCS].[State] AS STD ON STD.Id = AMD.StateId
				             LEFT JOIN [ORG].[Plant] AS PT ON PT.Id = IR.PlantId
				             LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id = PT.AddressMasterId
				                Left join (select InventoryReceiveId,sum(TaxAmount) TaxAmount from TRN.purchaseOrderTax where InventoryReceiveId=@inventoryReceiveId and InventoryServiceId is null group by InventoryReceiveId) aaa On aaa.InventoryReceiveId=IM.InventoryReceiveId
				                LEFT JOIN (select MRD.Id, Sum(MRD.TransactionQty) ReqTransactionQty from TRN.MaterialRequsitionDetails MRD group By MRD.Id) ccc On ccc.Id=IM.RequisitionDetailId
				                LEFT JOIN TRN.PoRequisitionDetail POREQD On POREQD.PoDetailId= IM.Id
				                Left JOIN TRN.MaterialRequsitionDetails MRMD ON MRMD.Id=POREQD.RequisitionDetailId
				                LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.PODetailsId=POREQD.PoDetailId
				                LEFT JOIN (select POReqDetailsID, sum(qty) allowQty from TRN.GRNPORequisitionAllocation group BY POReqDetailsID) GA On GA.POReqDetailsID=POREQD.Id
				             WHERE IRD.Id='" + InventoryReceiveDetailId + "'--IRD.PODetailsId = @inventoryReceiveId AND ";
				return _sqlRepository.GetDataCollection(_sql);
				//}

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public Dictionary<string, object> GetStock(InventoryMaterialViewModel entity, string issueDate)
		{
			try
			{
				var sql = @"
                    SELECT  sum(t.TotalQty) TotalQty,sum(t.PostingQty) PostingQty,sum(t.PostingQty) PostingQuantity,sum(t.ApprovedQty) ApprovedQty,sum(t.UnApprovedQty) UnApprovedQty
                        from(
		              SELECT TotalQty=(((SUM(ISNULL(IRD.BaseQty,0))-SUM(ISNULL(IRD.ShortageQty, 0)) - SUM(ISNULL(IIH.Qty, 0))-SUM(ISNULL(TIRD.TransferBaseQty,0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 PostingQty, 0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    LEFT JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					LEFT JOIN(SELECT IH.InventoryReceiveDetailId,IH.MaterialStorageId,sum(isnull(IH.Qty,0)) Qty FROM  TRN.InventoryIssueHistory IH 
								JOIN TRN.InventoryIssueDetail IID ON IID.Id=IH.InventoryIssueDetailId 
								JOIN TRN.InventoryIssue II ON II.Id=IID.InventoryIssueId WHERE IH.MaterialStorageId='" + entity.MaterialStorageId+ @"' 
								AND II.CompanyId='" + entity.CompanyId + @"' AND II.PlantId='" + entity.PlantId + @"' 
								GROUP BY IH.InventoryReceiveDetailId,IH.MaterialStorageId
								) IIH ON 
								IIH.InventoryReceiveDetailId=IRD.Id AND IIH.MaterialStorageId=IRD.MaterialStorageId
                    LEFT JOIN (SELECT TIRD.TransferedFromGrnId,SUM(ISNULL(TIRD.BaseQty,0)) TransferBaseQty 
								FROM TRN.InventoryReceiveDetail TIRD GROUP BY TIRD.TransferedFromGrnId
								) TIRD ON TIRD.TransferedFromGrnId=IRD.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE) AND IRD.Id NOT IN (SELECT InventoryReceiveDetailId FROM TRN.CapitalizationMasterDetail WHERE InventoryIssueHistoryId IS NULL)
            UNION ALL
            SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.ShortageQty, 0)) - SUM(ISNULL(IIH.Qty, 0))-SUM(ISNULL(TIRD.TransferBaseQty,0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					LEFT JOIN(SELECT IH.InventoryReceiveDetailId,IH.MaterialStorageId,sum(isnull(IH.Qty,0)) Qty FROM  TRN.InventoryIssueHistory IH 
								JOIN TRN.InventoryIssueDetail IID ON IID.Id=IH.InventoryIssueDetailId 
								JOIN TRN.InventoryIssue II ON II.Id=IID.InventoryIssueId WHERE IH.MaterialStorageId='" + entity.MaterialStorageId+ @"' 
								AND II.CompanyId='" + entity.CompanyId + @"' AND II.PlantId='" + entity.PlantId + @"' 
								GROUP BY IH.InventoryReceiveDetailId,IH.MaterialStorageId
								) IIH ON 
								IIH.InventoryReceiveDetailId=IRD.Id AND IIH.MaterialStorageId=IRD.MaterialStorageId
                    LEFT JOIN (SELECT TIRD.TransferedFromGrnId,SUM(ISNULL(TIRD.BaseQty,0)) TransferBaseQty 
								FROM TRN.InventoryReceiveDetail TIRD GROUP BY TIRD.TransferedFromGrnId
								) TIRD ON TIRD.TransferedFromGrnId=IRD.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status]='Posting' AND IR.IsFOC=0
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE) AND IRD.Id NOT IN (SELECT InventoryReceiveDetailId FROM TRN.CapitalizationMasterDetail WHERE InventoryIssueHistoryId IS NULL)

                    UNION ALL
SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.ShortageQty, 0)) - SUM(ISNULL(IIH.Qty, 0))-SUM(ISNULL(TIRD.TransferBaseQty,0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					LEFT JOIN(SELECT IH.InventoryReceiveDetailId,IH.MaterialStorageId,sum(isnull(IH.Qty,0)) Qty FROM  TRN.InventoryIssueHistory IH 
								JOIN TRN.InventoryIssueDetail IID ON IID.Id=IH.InventoryIssueDetailId 
								JOIN TRN.InventoryIssue II ON II.Id=IID.InventoryIssueId WHERE IH.MaterialStorageId='" + entity.MaterialStorageId + @"' 
								AND II.CompanyId='" + entity.CompanyId + @"' AND II.PlantId='" + entity.PlantId + @"' 
								GROUP BY IH.InventoryReceiveDetailId,IH.MaterialStorageId
								) IIH ON 
								IIH.InventoryReceiveDetailId=IRD.Id AND IIH.MaterialStorageId=IRD.MaterialStorageId
                    LEFT JOIN (SELECT TIRD.TransferedFromGrnId,SUM(ISNULL(TIRD.BaseQty,0)) TransferBaseQty 
								FROM TRN.InventoryReceiveDetail TIRD GROUP BY TIRD.TransferedFromGrnId
								) TIRD ON TIRD.TransferedFromGrnId=IRD.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.IsApproved=1 AND IR.IsFOC=1
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE) AND IRD.Id NOT IN (SELECT InventoryReceiveDetailId FROM TRN.CapitalizationMasterDetail WHERE InventoryIssueHistoryId IS NULL)

                    UNION ALL
                    SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.ShortageQty, 0)) - SUM(ISNULL(IIH.Qty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					LEFT JOIN(SELECT IH.InventoryReceiveDetailId,IH.MaterialStorageId,sum(isnull(IH.Qty,0)) Qty FROM  TRN.InventoryIssueHistory IH 
								JOIN TRN.InventoryIssueDetail IID ON IID.Id=IH.InventoryIssueDetailId 
								JOIN TRN.InventoryIssue II ON II.Id=IID.InventoryIssueId WHERE IH.MaterialStorageId='" + entity.MaterialStorageId+ @"' 
								AND II.CompanyId='" + entity.CompanyId + "' AND II.PlantId='" + entity.PlantId + @"' 
								GROUP BY IH.InventoryReceiveDetailId,IH.MaterialStorageId
								) IIH ON 
								IIH.InventoryReceiveDetailId=IRD.Id AND IIH.MaterialStorageId=IRD.MaterialStorageId
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status]='Posting' AND IR.IsApproved=1 AND IR.RequiredPosting=1 AND IR.GRNType='MaterialTransfer'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE) AND IRD.Id NOT IN (SELECT InventoryReceiveDetailId FROM TRN.CapitalizationMasterDetail WHERE InventoryIssueHistoryId IS NULL)
                    UNION ALL
                    SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.ShortageQty, 0)) - SUM(ISNULL(IIH.Qty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					LEFT JOIN(SELECT IH.InventoryReceiveDetailId,IH.MaterialStorageId,sum(isnull(IH.Qty,0)) Qty FROM  TRN.InventoryIssueHistory IH 
								JOIN TRN.InventoryIssueDetail IID ON IID.Id=IH.InventoryIssueDetailId 
								JOIN TRN.InventoryIssue II ON II.Id=IID.InventoryIssueId WHERE IH.MaterialStorageId='" + entity.MaterialStorageId+ @"' 
								AND II.CompanyId='" + entity.CompanyId + "' AND II.PlantId='" + entity.PlantId + @"' 
								GROUP BY IH.InventoryReceiveDetailId,IH.MaterialStorageId
								) IIH ON 
								IIH.InventoryReceiveDetailId=IRD.Id AND IIH.MaterialStorageId=IRD.MaterialStorageId
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status] IS NULL AND IR.IsApproved=1 AND IR.RequiredPosting=0 AND IR.GRNType='MaterialTransfer'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE) AND IRD.Id NOT IN (SELECT InventoryReceiveDetailId FROM TRN.CapitalizationMasterDetail WHERE InventoryIssueHistoryId IS NULL)
            UNION ALL
           SELECT 0 TotalQty,0 PostingQty,ApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.ShortageQty, 0)) - SUM(ISNULL(IIH.Qty, 0))-SUM(ISNULL(TIRD.TransferBaseQty,0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					LEFT JOIN(SELECT IH.InventoryReceiveDetailId,IH.MaterialStorageId,sum(isnull(IH.Qty,0)) Qty FROM  TRN.InventoryIssueHistory IH 
								JOIN TRN.InventoryIssueDetail IID ON IID.Id=IH.InventoryIssueDetailId 
								JOIN TRN.InventoryIssue II ON II.Id=IID.InventoryIssueId WHERE IH.MaterialStorageId='" + entity.MaterialStorageId+ @"' 
								AND II.CompanyId='" + entity.CompanyId + "' AND II.PlantId='" + entity.PlantId + @"' 
								GROUP BY IH.InventoryReceiveDetailId,IH.MaterialStorageId
								) IIH ON 
								IIH.InventoryReceiveDetailId=IRD.Id AND IIH.MaterialStorageId=IRD.MaterialStorageId
                    LEFT JOIN (SELECT TIRD.TransferedFromGrnId,SUM(ISNULL(TIRD.BaseQty,0)) TransferBaseQty 
								FROM TRN.InventoryReceiveDetail TIRD GROUP BY TIRD.TransferedFromGrnId
								) TIRD ON TIRD.TransferedFromGrnId=IRD.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.IsApproved=1
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE) AND IRD.Id NOT IN (SELECT InventoryReceiveDetailId FROM TRN.CapitalizationMasterDetail WHERE InventoryIssueHistoryId IS NULL)
                UNION ALL
                SELECT 0 TotalQty,0 PostingQty,0 ApprovedQty, UnApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.ShortageQty, 0)) - SUM(ISNULL(IIH.Qty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))))
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					LEFT JOIN(SELECT IH.InventoryReceiveDetailId,IH.MaterialStorageId,sum(isnull(IH.Qty,0)) Qty FROM  TRN.InventoryIssueHistory IH 
								JOIN TRN.InventoryIssueDetail IID ON IID.Id=IH.InventoryIssueDetailId 
								JOIN TRN.InventoryIssue II ON II.Id=IID.InventoryIssueId WHERE IH.MaterialStorageId='" + entity.MaterialStorageId+ @"' 
								AND II.CompanyId='" + entity.CompanyId + @"' AND II.PlantId='" + entity.PlantId + @"' 
								GROUP BY IH.InventoryReceiveDetailId,IH.MaterialStorageId
								) IIH ON 
								IIH.InventoryReceiveDetailId=IRD.Id AND IIH.MaterialStorageId=IRD.MaterialStorageId
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.IsApproved=0
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE) AND IRD.Id NOT IN (SELECT InventoryReceiveDetailId FROM TRN.CapitalizationMasterDetail WHERE InventoryIssueHistoryId IS NULL)
			) AS t";
				return _sqlRepository.GetData(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}


		public Dictionary<string, object> GetStockCountryWise(InventoryMaterialViewModel entity, string issueDate)
		{
			try
			{
				var sql = @"--SELECT IM.TotalQty FROM TRN.InventoryMaterial AS IM WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "'AND IM.PlantId='" + entity.PlantId + @"' 
                            --AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + @"'
                            --AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                            --AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + @"'
                            --AND IM.Id IN(SELECT DISTINCT A.InventoryMaterialId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
                            --        WHERE A.MaterialStorageId='" + entity.MaterialStorageId + "' AND CAST(B.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE))
 SELECT  sum(t.TotalQty) TotalQty,sum(t.PostingQty) PostingQty,sum(t.PostingQty) PostingQuantity,sum(t.ApprovedQty) ApprovedQty,sum(t.UnApprovedQty) UnApprovedQty
                      --SELECT  REPLACE(CONVERT(varchar(20), (CAST(max(t.TotalQty) AS money)), 1), '.00', '') AS TotalQty
					  --,REPLACE(CONVERT(varchar(20), (CAST(max(t.PostingQty) AS money)), 1), '.00', '') AS PostingQty
					  --,max(t.PostingQty) AS PostingQuantity
					  --,REPLACE(CONVERT(varchar(20), (CAST(max(t.ApprovedQty) AS money)), 1), '.00', '') AS ApprovedQty
					  --, REPLACE(CONVERT(varchar(20), (CAST(max(t.UnApprovedQty) AS money)), 1), '.00', '') AS UnApprovedQty 
                        from(
		              SELECT TotalQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0)))), 0 PostingQty, 0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND ISNULL(IM.CountryId,'')='" + entity.CountryId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
            UNION ALL
            SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status]='Posting' 
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND ISNULL(IM.CountryId,'')='" + entity.CountryId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
            UNION ALL
           SELECT 0 TotalQty,0 PostingQty,ApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0)))), 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.IsApproved=1
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND ISNULL(IM.CountryId,'')='" + entity.CountryId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
                UNION ALL
                SELECT 0 TotalQty,0 PostingQty,0 ApprovedQty, UnApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))))
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.IsApproved=0
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND ISNULL(IM.CountryId,'')='" + entity.CountryId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
) AS t";
				return _sqlRepository.GetData(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> GetSpecificMaterialStock(InventoryMaterialViewModel entity, string issueDate)
		{
			try
			{
				var sql = "";
                var tempsql = "";
                if (!string.IsNullOrEmpty(entity.MaterialStorageId) && entity.IsAsset==false)
                    tempsql = "IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' AND IRD.IsAsset=0 ";
               else if (!string.IsNullOrEmpty(entity.MaterialStorageId) && entity.IsAsset == true)
                    tempsql = "IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"'  AND IRD.IsAsset=1 ";
                else if (string.IsNullOrEmpty(entity.MaterialStorageId) && entity.IsAsset == true)
                    tempsql = "IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"'  AND IRD.IsAsset=1 ";
                else
                    tempsql = " IM.CompanyGroupId = '" + entity.CompanyGroupId + "' AND IM.CompanyId = '" + entity.CompanyId + "' AND IM.PlantId = '" + entity.PlantId + @"' ";

                if (!string.IsNullOrEmpty(entity.LotNumber))
                {
                    tempsql += " AND IRD.LotNumber='"+entity.LotNumber+@"'";
                }
                
                if (string.IsNullOrEmpty(entity.SalesOrderId))
				{
					sql = @"select * from(
                         SELECT IRD.InventoryReceiveId, IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
	                     , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 0 else 1 END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 0 else 1 END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor,IRD.BaseUoMFactor GRNBaseUoMFactor
                        , round(IRD.MaterialTranRate,4) MaterialTranRate,  TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
                        --, BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.MaterialTranAmount/IRD.BaseQty ELSE IRD.BooksCurrencyBaseRate END
						, round(ISNULL(IRD.TrnCurrencyBaseRate,0),4)++Round(ISNULL((case when ird.AdditionalChargesAmount>0 then ird.AdditionalChargesAmount/ird.BaseQty else 0 end),0),4) BaseRate
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                        --,Round((IRD.MaterialTranRate * IR.ToCurrencyRate),4) 
						 ,Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4)+Round(ISNULL((case when ird.AdditionalChargesAmount>0 then ird.AdditionalChargesAmount/ird.BaseQty else 0 end) * ir.ToCurrencyRate,0),4) BaseCurrencyRate
						, IRD.TransactionQty, IRD.BaseQty
						,ISNULL(IRD.BaseQty-IRD.ShortageQty,0) - ISNULL(II.IssueQty, 0) StockQty
						, ISNULL(II.IssueQty,0) IssueQty, ISNULL(II.IssueQty,0) BaseIssueQty, ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty,ISNULL(IRD.IssueReturnQty,0) IssueReturnQty,ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty,ISNULL(IRD.InventorySalesQty,0) InventorySalesQty,ISNULL(IRD.InventoryScrapQty,0) InventoryScrapQty
                        ,ISNULL(IRD.ShortageQty,0) ShortageQty
						 ,((((((ISNULL(IRD.BaseQty-IRD.ShortageQty,0) - ISNULL(II.IssueQty, 0)-ISNULL(IRD.PurchaseReturnQty,0)))-ISNULL(IRD.ReductionByAdjustmentQty,0))-ISNULL(IRD.InventorySalesQty,0))-ISNULL(IRD.InventoryScrapQty,0))-ISNULL(IRD.InventoryTransferQty,0)) AS BalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						, Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4)+Round(ISNULL((case when ird.AdditionalChargesAmount>0 then ird.AdditionalChargesAmount/ird.BaseQty else 0 end) * ir.ToCurrencyRate,0),4) BooksCurrencyBaseRate
						 ,round(ISNULL(IRD.TrnCurrencyBaseRate,0),4)++Round(ISNULL((case when ird.AdditionalChargesAmount>0 then ird.AdditionalChargesAmount/ird.BaseQty else 0 end),0),4) TrnCurrencyBaseRate
                        ,round(ISNULL(II.IssueAmount,0),4) TotalIssueAmount
                        , ISNULL(IRD.AdditionalChargesAmount,0) AdditionalChargesAmount
                         ,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END
                        ,C.Id CountryId,C.UserName CountryName--,null AS [Flag] 
                        ,0 SalesRate
						,0 TotalAmount
                        ,IM.MaterialMasterId
						,IM.ArticleId
						,IM.FirstCharacteristicsValueId
						,IM.SecondCharacteristicsValueId
						,IM.ThirdCharacteristicsValueId
						
						,IM.FirstCharacteristicsId
						,IM.SecondCharacteristicsId
						,IM.ThirdCharacteristicsId,IssueByUoM=CASE WHEN MM.IssueByUoM=0 THEN 'No' ELSE 'Yes' END
                        ,TrasactopmUomQty=(((ISNULL(IRD.BaseQty,0) - ISNULL(II.IssueQty, 0))*BaseUoMFactor)/BaseUoMFactor) 
						--,0 TrasactopmUomQty
						,'' IssueTransactionUoMId
						,'' IssueTransactionUoM,ird.MaterialStorageId,MS.UserName MaterialStorage,IRD.LotNumber
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
					left join mst.MaterialMaster MM ON MM.Id=Im.MaterialMasterId
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
					LEFT JOIN (
									    select IID.InventoryMaterialId,IH.InventoryReceiveDetailId, IH.MaterialStorageId
                                        , Sum(ISNULL(IH.Qty,0)-ISNULL(IH.IssueReturnQty,0)) IssueQty , Sum(ISNULL(IH.TotalMaterialBooksCurrencyAmount,0)) IssueAmount,IID.IsAsset
									    FROM TRN.InventoryIssueDetail IID  
									    LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									    LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									    WHERE --convert(Date,II.IssueDate) <= CAST('" + issueDate + @"' AS DATE)  AND 
                                        II.PlantId='" + entity.PlantId + @"'   
									    GROUP BY IID.InventoryMaterialId,IID.IsAsset,IH.InventoryReceiveDetailId, IH.MaterialStorageId
									    ) II ON II.InventoryReceiveDetailId=IRD.Id and II.MaterialStorageId=IRD.MaterialStorageId 
                    left JOIN SCS.Country C On C.Id=IM.CountryId
                    LEFT JOIN [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    WHERE " + tempsql + @"
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND IR.[Status]='Posting' AND IR.IsFOC=0
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND ISNULL(IM.CountryId,'')='" + entity.CountryId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
					AND IRD.BaseQty !=(ISNULL(II.IssueQty,0))
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE) 

                    UNION ALL

                    SELECT IRD.InventoryReceiveId, IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
	                     , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 0 else 1 END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 0 else 1 END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor,IRD.BaseUoMFactor GRNBaseUoMFactor
                        , round(IRD.MaterialTranRate,4) MaterialTranRate,  TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
                        --, BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.MaterialTranAmount/IRD.BaseQty ELSE IRD.BooksCurrencyBaseRate END
						, IRD.TrnCurrencyBaseRate BaseRate
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                        --,Round((IRD.MaterialTranRate * IR.ToCurrencyRate),4) 
						 ,Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4) BaseCurrencyRate
						, IRD.TransactionQty, IRD.BaseQty
						,ISNULL(IRD.BaseQty-IRD.ShortageQty,0) - ISNULL(II.IssueQty, 0)  StockQty
						, ISNULL(II.IssueQty,0) IssueQty, ISNULL(II.IssueQty,0) BaseIssueQty, ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty,ISNULL(IRD.IssueReturnQty,0) IssueReturnQty,ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty,ISNULL(IRD.InventorySalesQty,0) InventorySalesQty,ISNULL(IRD.InventoryScrapQty,0) InventoryScrapQty
                        ,ISNULL(IRD.ShortageQty,0) ShortageQty
						 ,((((((ISNULL(IRD.BaseQty-IRD.ShortageQty,0) - ISNULL(II.IssueQty, 0)-ISNULL(IRD.PurchaseReturnQty,0)))-ISNULL(IRD.ReductionByAdjustmentQty,0))-ISNULL(IRD.InventorySalesQty,0))-ISNULL(IRD.InventoryScrapQty,0))-ISNULL(IRD.InventoryTransferQty,0)) AS BalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						 ,Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4) BooksCurrencyBaseRate
						 ,round(ISNULL(IRD.TrnCurrencyBaseRate,0),4) TrnCurrencyBaseRate
                        ,round(ISNULL(II.IssueAmount,0),4) TotalIssueAmount
                        , ISNULL(IRD.AdditionalChargesAmount,0) AdditionalChargesAmount
                         ,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END
                        ,C.Id CountryId,C.UserName CountryName--,null AS [Flag] 
                        ,0 SalesRate
						,0 TotalAmount
                        ,IM.MaterialMasterId
						,IM.ArticleId
						,IM.FirstCharacteristicsValueId
						,IM.SecondCharacteristicsValueId
						,IM.ThirdCharacteristicsValueId
						
						,IM.FirstCharacteristicsId
						,IM.SecondCharacteristicsId
						,IM.ThirdCharacteristicsId,IssueByUoM=CASE WHEN MM.IssueByUoM=0 THEN 'No' ELSE 'Yes' END
                        ,TrasactopmUomQty=(((ISNULL(IRD.BaseQty,0) - ISNULL(II.IssueQty, 0))*BaseUoMFactor)/BaseUoMFactor) 
						--,0 TrasactopmUomQty
						,'' IssueTransactionUoMId
						,'' IssueTransactionUoM,ird.MaterialStorageId,MS.UserName MaterialStorage,IRD.LotNumber
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
					left join mst.MaterialMaster MM ON MM.Id=Im.MaterialMasterId
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
					LEFT JOIN (
									    select IID.InventoryMaterialId,IH.InventoryReceiveDetailId, IH.MaterialStorageId
                                        , Sum(ISNULL(IH.Qty,0)-ISNULL(IH.IssueReturnQty,0)) IssueQty , Sum(ISNULL(IH.TotalMaterialBooksCurrencyAmount,0)) IssueAmount,IID.IsAsset
									    FROM TRN.InventoryIssueDetail IID  
									    LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									    LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									    WHERE --convert(Date,II.IssueDate) <= CAST('" + issueDate + @"' AS DATE)  AND 
                                        II.PlantId='" + entity.PlantId + @"'   
									    GROUP BY IID.InventoryMaterialId,IID.IsAsset,IH.InventoryReceiveDetailId, IH.MaterialStorageId
									    ) II ON II.InventoryReceiveDetailId=IRD.Id and II.MaterialStorageId=IRD.MaterialStorageId 
                    left JOIN SCS.Country C On C.Id=IM.CountryId
                    LEFT JOIN [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    WHERE " + tempsql + @"
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND IR.IsApproved=1 AND IR.IsFOC=1
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND ISNULL(IM.CountryId,'')='" + entity.CountryId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
					AND IRD.BaseQty !=ISNULL(II.IssueQty,0)
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE) 

					Union ALL
					SELECT IRD.InventoryReceiveId, IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
	                     , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 0 else 1 END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 0 else 1 END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor,IRD.BaseUoMFactor GRNBaseUoMFactor
                        , round(IRD.MaterialTranRate,4) MaterialTranRate,  TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
                        ,round(ISNULL(IRD.TrnCurrencyBaseRate,0),4) BaseRate
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                       ,Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4) BaseCurrencyRate
						, IRD.TransactionQty, IRD.BaseQty
						,ISNULL(IRD.BaseQty-IRD.ShortageQty,0) - ISNULL(IRD.BaseIssueQty, 0) StockQty
						, ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty, ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty,ISNULL(IRD.IssueReturnQty,0) IssueReturnQty,ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty,ISNULL(IRD.InventorySalesQty,0) InventorySalesQty,ISNULL(IRD.InventoryScrapQty,0) InventoryScrapQty
                        ,ISNULL(IRD.ShortageQty,0) ShortageQty
						 ,((((((ISNULL(IRD.BaseQty-IRD.ShortageQty,0) - ISNULL(IRD.BaseIssueQty, 0)-ISNULL(IRD.PurchaseReturnQty,0))+ISNULL(IRD.IssueReturnQty,0))-ISNULL(IRD.ReductionByAdjustmentQty,0))-ISNULL(IRD.InventorySalesQty,0))-ISNULL(IRD.InventoryScrapQty,0))-ISNULL(IRD.InventoryTransferQty,0)) AS BalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						 ,Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4) BooksCurrencyBaseRate
						 ,round(ISNULL(IRD.TrnCurrencyBaseRate,0),4) TrnCurrencyBaseRate
                         ,0 TotalIssueAmount
                        , ISNULL(IRD.AdditionalChargesAmount,0) AdditionalChargesAmount
                         ,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END
                        ,C.Id CountryId,C.UserName CountryName--,null AS [Flag] 
                        ,0 SalesRate
						,0 TotalAmount
                        ,IM.MaterialMasterId
						,IM.ArticleId
						,IM.FirstCharacteristicsValueId
						,IM.SecondCharacteristicsValueId
						,IM.ThirdCharacteristicsValueId
						
						,IM.FirstCharacteristicsId
						,IM.SecondCharacteristicsId
						,IM.ThirdCharacteristicsId,IssueByUoM=CASE WHEN MM.IssueByUoM=0 THEN 'No' ELSE 'Yes' END
                        ,TrasactopmUomQty=(((ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0))*BaseUoMFactor)/BaseUoMFactor) 
						--,0 TrasactopmUomQty
						,'' IssueTransactionUoMId
						,'' IssueTransactionUoM,ird.MaterialStorageId,MS.UserName MaterialStorage,IRD.LotNumber
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
					left join mst.MaterialMaster MM ON MM.Id=Im.MaterialMasterId
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
                    left JOIN SCS.Country C On C.Id=IM.CountryId
                    LEFT JOIN [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    WHERE " + tempsql + @"
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND IR.[Status]='Posting' AND IR.IsApproved=1 AND IR.RequiredPosting=1 AND IR.GRNType='MaterialTransfer'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND ISNULL(IM.CountryId,'')='" + entity.CountryId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
					AND IRD.BaseQty !=IRD.BaseIssueQty
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)							

					Union ALL
					SELECT IRD.InventoryReceiveId, IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
	                     , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 0 else 1 END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 0 else 1 END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor,IRD.BaseUoMFactor GRNBaseUoMFactor
                        , round(IRD.MaterialTranRate,4) MaterialTranRate,  TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
                        ,round(ISNULL(IRD.TrnCurrencyBaseRate,0),4) BaseRate
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                         ,Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4) BaseCurrencyRate
						, IRD.TransactionQty, IRD.BaseQty
						,ISNULL(IRD.BaseQty-IRD.ShortageQty,0) - ISNULL(IRD.BaseIssueQty, 0)+ISNULL(IRD.IssueReturnQty,0) StockQty
						, ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty, ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty,ISNULL(IRD.IssueReturnQty,0) IssueReturnQty,ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty,ISNULL(IRD.InventorySalesQty,0) InventorySalesQty,ISNULL(IRD.InventoryScrapQty,0) InventoryScrapQty
                        ,ISNULL(IRD.ShortageQty,0) ShortageQty
						 ,((((((ISNULL(IRD.BaseQty-IRD.ShortageQty,0) - ISNULL(IRD.BaseIssueQty, 0)-ISNULL(IRD.PurchaseReturnQty,0))+ISNULL(IRD.IssueReturnQty,0))-ISNULL(IRD.ReductionByAdjustmentQty,0))-ISNULL(IRD.InventorySalesQty,0))-ISNULL(IRD.InventoryScrapQty,0))-ISNULL(IRD.InventoryTransferQty,0)) AS BalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						 ,Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4) BooksCurrencyBaseRate
						 ,round(ISNULL(IRD.TrnCurrencyBaseRate,0),4) TrnCurrencyBaseRate
                         ,0 TotalIssueAmount
                         , ISNULL(IRD.AdditionalChargesAmount,0) AdditionalChargesAmount
                         ,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END
                        ,C.Id CountryId,C.UserName CountryName--,null AS [Flag] 
                        ,0 SalesRate
						,0 TotalAmount
                        ,IM.MaterialMasterId
						,IM.ArticleId
						,IM.FirstCharacteristicsValueId
						,IM.SecondCharacteristicsValueId
						,IM.ThirdCharacteristicsValueId
						
						,IM.FirstCharacteristicsId
						,IM.SecondCharacteristicsId
						,IM.ThirdCharacteristicsId,IssueByUoM=CASE WHEN MM.IssueByUoM=0 THEN 'No' ELSE 'Yes' END
                        ,TrasactopmUomQty=(((ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0))*BaseUoMFactor)/BaseUoMFactor) 
						--,0 TrasactopmUomQty
						,'' IssueTransactionUoMId
						,'' IssueTransactionUoM,ird.MaterialStorageId,MS.UserName MaterialStorage,IRD.LotNumber
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
					left join mst.MaterialMaster MM ON MM.Id=Im.MaterialMasterId
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
                    left JOIN SCS.Country C On C.Id=IM.CountryId
                    LEFT JOIN [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    WHERE " + tempsql + @"
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND IR.[Status] IS null And IR.IsApproved=1 AND IR.RequiredPosting=0 AND IR.GRNType='MaterialTransfer'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND ISNULL(IM.CountryId,'')='" + entity.CountryId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
					AND IRD.BaseQty !=IRD.BaseIssueQty
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE) )x WHERE x.BalanceStock>0 
					";
				}
                else
				{
					sql = @"select 
								IRD.InventoryReceiveId
								,allocation.SalesOrderId
								, IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
								, IsPosting=CASE WHEN IR.[Status] IS NULL THEN 0 else 1 END
								, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 0 else 1 END
								,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END
								,C.Id CountryId,C.UserName CountryName--,null AS [Flag] 
								,0 SalesRate
								,0 TotalAmount
								,IM.MaterialMasterId
								,IM.ArticleId
								,IM.FirstCharacteristicsValueId
								,IM.SecondCharacteristicsValueId
								,IM.ThirdCharacteristicsValueId
								,IM.CountryId
								,IM.FirstCharacteristicsId
								,IM.SecondCharacteristicsId
								,IM.ThirdCharacteristicsId,IssueByUoM=CASE WHEN MM.IssueByUoM=0 THEN 'No' ELSE 'Yes' END
                       
								,0 TrasactopmUomQty
								,'' IssueTransactionUoMId
								,'' IssueTransactionUoM


								, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor
								, round(IRD.MaterialTranRate,4) MaterialTranRate, IRD.BooksCurrencyBaseRate, TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
								, BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.MaterialTranAmount/IRD.TransactionQty ELSE IRD.BooksCurrencyBaseRate END
								, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
								,Round((IRD.MaterialTranRate * IR.ToCurrencyRate),4) BaseCurrencyRate
								, IRD.TransactionQty, IRD.BaseQty
								,sum(ISNULL(allocation.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0)) StockQty
								, sum(ISNULL(IRD.IssueQty,0)) IssueQty
								, sum(ISNULL(IRD.BaseIssueQty,0)) BaseIssueQty
								, sum(ISNULL(IRD.PurchaseReturnQty,0)) PurchaseReturnQty
								,sum(ISNULL(IRD.IssueReturnQty,0)) IssueReturnQty
								,sum(ISNULL(IRD.ReductionByAdjustmentQty,0)) ReductionByAdjustmentQty
								,sum(ISNULL(IRD.InventorySalesQty,0)) InventorySalesQty
								,sum(ISNULL(IRD.InventoryScrapQty,0)) InventoryScrapQty
								,sum(ISNULL(IRD.InventoryTransferQty,0)) InventoryTransferQty

								, ((((((Sum(ISNULL(allocation.BaseQty,0)) - Sum(ISNULL(IRD.BaseIssueQty, 0))-Sum(ISNULL(IRD.PurchaseReturnQty,0)))+Sum(ISNULL(IRD.IssueReturnQty,0)))-Sum(ISNULL(IRD.ReductionByAdjustmentQty,0)))-Sum(ISNULL(IRD.InventorySalesQty,0)))-Sum(ISNULL(IRD.InventoryScrapQty,0)))-Sum(ISNULL(IRD.InventoryTransferQty,0))) AS BalanceStock

								,sum(ISNULL(IRD.TotalMaterialTranAmount,0)) TotalMaterialTranAmount
								,sum(ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0)) TotalMaterialBooksCurrencyAmount
								,sum(Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4)) BooksCurrencyBaseRate
								,sum(round(ISNULL(IRD.TrnCurrencyBaseRate,0),4)) TrnCurrencyBaseRate
								,TrasactopmUomQty=Sum(((ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0))*BaseUoMFactor)/BaseUoMFactor) ,ird.MaterialStorageId,MS.UserName MaterialStorage
								from trn.GRNPORequisitionAllocation allocation
								left join trn.InventoryReceiveDetail IRD on IRD.Id=allocation.InventoryReceiveDetailId
								left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
								left join mst.MaterialMaster MM ON MM.Id=Im.MaterialMasterId
								left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
								LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
								left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
								left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
								left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
								left JOIN SCS.Country C On C.Id=IM.CountryId
                                LEFT JOIN [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND IR.[Status]='Posting' 
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND ISNULL(IM.CountryId,'')='" + entity.CountryId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
					--And ((((((ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0)-ISNULL(IRD.PurchaseReturnQty,0))+ISNULL(IRD.IssueReturnQty,0))-ISNULL(IRD.ReductionByAdjustmentQty,0))-ISNULL(IRD.InventorySalesQty,0))-ISNULL(IRD.InventoryScrapQty,0))-ISNULL(IRD.InventoryTransferQty,0))>0
					AND IRD.BaseQty !=IRD.BaseIssueQty
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + "' AS DATE) AND allocation.SalesOrderId in('" + entity.SalesOrderId + @"')  group by IRD.InventoryReceiveId,allocation.SalesOrderId, IRD.POId, IRD.PODetailsId, IRD.Id , IRD.InventoryMaterialId
                    , P.Code , P.UserName, CASE WHEN IR.[Status] IS NULL THEN 0 else 1 END,CASE WHEN IR.IsApproved = 0 THEN 0 else 1 END,CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END,C.Id,C.UserName,IM.MaterialMasterId,IM.ArticleId
                    ,IM.FirstCharacteristicsValueId,IM.SecondCharacteristicsValueId,IM.ThirdCharacteristicsValueId,IM.CountryId,IM.FirstCharacteristicsId,IM.SecondCharacteristicsId,IM.ThirdCharacteristicsId,CASE WHEN MM.IssueByUoM = 0 THEN 'No' ELSE 'Yes' END
                , IR.Id, IRD.POId , TUoM.UserName , BUoM.UserName, IRD.TransactionUoMId,IRD.BaseUOMId, IRD.BaseUoMFactor, round(IRD.MaterialTranRate, 4), IRD.BooksCurrencyBaseRate, TCU.Code, BCU.Code, IRD.MaterialTranAmount
                , CASE WHEN IRD.TransactionUoMId<> IRD.BaseUOMId THEN IRD.MaterialTranAmount / IRD.BaseQty ELSE IRD.BooksCurrencyBaseRate END, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106), ' ', '-') , REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106), ' ', '-') 
                , Round((IRD.MaterialTranRate * IR.ToCurrencyRate), 4) , IRD.TransactionQty, IRD.BaseQty,IR.AddedDate ORDER BY IR.AddedDate";

				}
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> GetSpecificMaterialTransferStock(InventoryMaterialViewModel entity, string issueDate)
		{
			try
			{
				
				var sql = @"SELECT IRD.InventoryReceiveId,IRD.TransferedFromGrnId , IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
	                    , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 'NO' else 'YES' END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 'NO' else 'YES' END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor
                        , IRD.MaterialTranRate, IRD.BooksCurrencyBaseRate, TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
                        , BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.MaterialTranAmount/IRD.BaseQty ELSE IRD.BooksCurrencyBaseRate END
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                        ,(IRD.MaterialTranRate * IR.ToCurrencyRate) BaseCurrencyRate
						, IRD.TransactionQty, IRD.BaseQty
						,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) StockQty
						, ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty, ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty,ISNULL(IRD.IssueReturnQty,0) IssueReturnQty,ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty,ISNULL(IRD.InventorySalesQty,0) InventorySalesQty,ISNULL(IRD.InventoryScrapQty,0) InventoryScrapQty,ISNULL(IRD.InventoryTransferQty,0) InventoryTransferQty
						 ,((((((ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0)-ISNULL(IRD.PurchaseReturnQty,0))+ISNULL(IRD.IssueReturnQty,0))-ISNULL(IRD.ReductionByAdjustmentQty,0))-ISNULL(IRD.InventorySalesQty,0))-ISNULL(IRD.InventoryScrapQty,0))-ISNULL(IRD.InventoryTransferQty,0)) AS BalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						 ,ISNULL(IRD.BooksCurrencyBaseRate,0) BooksCurrencyBaseRate
						 ,ISNULL(IRD.TrnCurrencyBaseRate,0) TrnCurrencyBaseRate
                         ,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END
                        ,C.Id CountryId,C.UserName CountryName--,null AS [Flag] 
                        ,0 SalesRate
						,0 TotalAmount
                        ,IM.MaterialMasterId
						,IM.ArticleId
						,IM.FirstCharacteristicsValueId
						,IM.SecondCharacteristicsValueId
						,IM.ThirdCharacteristicsValueId
						,IM.CountryId
						,IM.FirstCharacteristicsId
						,IM.SecondCharacteristicsId
						,IM.ThirdCharacteristicsId
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
                    left JOIN SCS.Country C On C.Id=IM.CountryId
					WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"'
					AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND IR.[Status]='Posting' 
					AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
					AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND ISNULL(IM.CountryId,'')='" + entity.CountryId + "' AND IRD.MaterialStorageId='" + entity.FromMaterialStorageId + @"' 
					--AND ISNULL(IRD.IssueQty, 1)>0 
					AND IRD.BaseQty !=IRD.BaseIssueQty
					AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)

					Union ALL
					SELECT IRD.InventoryReceiveId,IRD.TransferedFromGrnId , IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
	                    , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 'NO' else 'YES' END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 'NO' else 'YES' END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor
                        , IRD.MaterialTranRate, IRD.BooksCurrencyBaseRate, TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
                        , BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.MaterialTranAmount/IRD.BaseQty ELSE IRD.BooksCurrencyBaseRate END
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                        ,(IRD.MaterialTranRate * IR.ToCurrencyRate) BaseCurrencyRate
						, IRD.TransactionQty, IRD.BaseQty
						,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) StockQty
						, ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty, ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty,ISNULL(IRD.IssueReturnQty,0) IssueReturnQty,ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty,ISNULL(IRD.InventorySalesQty,0) InventorySalesQty,ISNULL(IRD.InventoryScrapQty,0) InventoryScrapQty,ISNULL(IRD.InventoryTransferQty,0) InventoryTransferQty
						 ,((((((ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0)-ISNULL(IRD.PurchaseReturnQty,0))+ISNULL(IRD.IssueReturnQty,0))-ISNULL(IRD.ReductionByAdjustmentQty,0))-ISNULL(IRD.InventorySalesQty,0))-ISNULL(IRD.InventoryScrapQty,0))-ISNULL(IRD.InventoryTransferQty,0)) AS BalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						 ,ISNULL(IRD.BooksCurrencyBaseRate,0) BooksCurrencyBaseRate
						 ,ISNULL(IRD.TrnCurrencyBaseRate,0) TrnCurrencyBaseRate
                         ,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END
                        ,C.Id CountryId,C.UserName CountryName--,null AS [Flag] 
                        ,0 SalesRate
						,0 TotalAmount
                        ,IM.MaterialMasterId
						,IM.ArticleId
						,IM.FirstCharacteristicsValueId
						,IM.SecondCharacteristicsValueId
						,IM.ThirdCharacteristicsValueId
						,IM.CountryId
						,IM.FirstCharacteristicsId
						,IM.SecondCharacteristicsId
						,IM.ThirdCharacteristicsId
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
                    left JOIN SCS.Country C On C.Id=IM.CountryId
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"'
					AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND IR.[Status]='Posting' AND IR.IsApproved=1 AND IR.RequiredPosting=1 AND IR.GRNType='MaterialTransfer'
					AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
					AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND ISNULL(IM.CountryId,'')='" + entity.CountryId + "' AND IRD.MaterialStorageId='" + entity.FromMaterialStorageId + @"' 
					--AND ISNULL(IRD.IssueQty, 1)>0 
					AND IRD.BaseQty !=IRD.BaseIssueQty
					AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)			

					Union ALL
					SELECT IRD.InventoryReceiveId,IRD.TransferedFromGrnId , IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
	                    , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 'NO' else 'YES' END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 'NO' else 'YES' END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor
                        , IRD.MaterialTranRate, IRD.BooksCurrencyBaseRate, TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
                        , BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.MaterialTranAmount/IRD.BaseQty ELSE IRD.BooksCurrencyBaseRate END
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                        ,(IRD.MaterialTranRate * IR.ToCurrencyRate) BaseCurrencyRate
						, IRD.TransactionQty, IRD.BaseQty
						,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) StockQty
						, ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty, ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty,ISNULL(IRD.IssueReturnQty,0) IssueReturnQty,ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty,ISNULL(IRD.InventorySalesQty,0) InventorySalesQty,ISNULL(IRD.InventoryScrapQty,0) InventoryScrapQty,ISNULL(IRD.InventoryTransferQty,0) InventoryTransferQty
						 ,((((((ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0)-ISNULL(IRD.PurchaseReturnQty,0))+ISNULL(IRD.IssueReturnQty,0))-ISNULL(IRD.ReductionByAdjustmentQty,0))-ISNULL(IRD.InventorySalesQty,0))-ISNULL(IRD.InventoryScrapQty,0))-ISNULL(IRD.InventoryTransferQty,0)) AS BalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						 ,ISNULL(IRD.BooksCurrencyBaseRate,0) BooksCurrencyBaseRate
						 ,ISNULL(IRD.TrnCurrencyBaseRate,0) TrnCurrencyBaseRate
                         ,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END
                        ,C.Id CountryId,C.UserName CountryName--,null AS [Flag] 
                        ,0 SalesRate
						,0 TotalAmount
                        ,IM.MaterialMasterId
						,IM.ArticleId
						,IM.FirstCharacteristicsValueId
						,IM.SecondCharacteristicsValueId
						,IM.ThirdCharacteristicsValueId
						,IM.CountryId
						,IM.FirstCharacteristicsId
						,IM.SecondCharacteristicsId
						,IM.ThirdCharacteristicsId
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
                    left JOIN SCS.Country C On C.Id=IM.CountryId
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"'
					AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND IR.[Status] is null
					AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
					AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND ISNULL(IM.CountryId,'')='" + entity.CountryId + "' AND IRD.MaterialStorageId='" + entity.FromMaterialStorageId + @"' 
					--AND ISNULL(IRD.IssueQty, 1)>0 
					AND IRD.BaseQty !=IRD.BaseIssueQty
					And IR.IsApproved=1 AND IR.RequiredPosting=0 AND IR.GRNType='MaterialTransfer'
					AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
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

		public IEnumerable<object> GetSpecificMaterialStockForAdjustment(InventoryMaterialViewModel entity, string issueDate)
		{
			try
			{
				var sql = @"SELECT IRD.InventoryReceiveId, IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
	                    , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 'NO' else 'YES' END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 'NO' else 'YES' END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor
                        , IRD.MaterialTranRate, IRD.BooksCurrencyBaseRate, TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
                        , BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.MaterialTranAmount/IRD.BaseQty ELSE IRD.BooksCurrencyBaseRate END
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                        ,(IRD.MaterialTranRate * IR.ToCurrencyRate) BaseCurrencyRate
						, IRD.TransactionQty, IRD.BaseQty
						,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) StockQty
						, ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty, ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty,ISNULL(IRD.IssueReturnQty,0) IssueReturnQty,ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty
						 ,(((ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0)-ISNULL(IRD.PurchaseReturnQty,0))+ISNULL(IRD.IssueReturnQty,0))-ISNULL(IRD.ReductionByAdjustmentQty,0)) AS BalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						 ,ISNULL(IRD.BooksCurrencyBaseRate,0) BooksCurrencyBaseRate
						 ,ISNULL(IRD.TrnCurrencyBaseRate,0) TrnCurrencyBaseRate
                         ,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND IR.[Status]='Posting' 
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
					AND IRD.BaseQty !=IRD.BaseIssueQty
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + "' AS DATE) ORDER BY IR.AddedDate";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> GetApprovedStockDetail(InventoryMaterialViewModel entity, string issueDate)
		{
			try
			{
				var sql = @"SELECT IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
                        
	                    , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 'NO' else 'YES' END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 'NO' else 'YES' END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor
                        , IRD.MaterialTranRate, IRD.BooksCurrencyBaseRate, TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
	                    --, IssueQty=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId  THEN ISNULL(IRD.IssueQty, 0)/BaseUoMFactor
					                --    WHEN IRD.IssueQty IS NULL THEN 0 ELSE IRD.IssueQty END
						, IRD.TransactionQty, IRD.BaseQty
						,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) StockQty
						, ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty
						 ,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) AS BalanceStock

                        , BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.MaterialTranAmount/IRD.BaseQty ELSE IRD.BooksCurrencyBaseRate END
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                        ,(IRD.MaterialTranRate*IR.ToCurrencyRate) BaseCurrencyRate
                        ,GateEntryNo
						,pwg.UserName GateName
                        ,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
                    left join trn.GateEntry G On G.id=IR.GateEntryNo
					Left Join dbo.PlantWiseGate pwg On pwg.Id=G.PlantWiseGateId
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND IR.IsApproved=1
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + "' AS DATE) ORDER BY CAST(IRD.AddedDate AS DATE)";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public IEnumerable<object> GetApprovedStockDetailBeyondIssueDate(InventoryMaterialViewModel entity, string issueDate)
		{
			try
			{
				var sql = @"SELECT IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
                        
	                    , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 'NO' else 'YES' END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 'NO' else 'YES' END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor
                        , IRD.MaterialTranRate, IRD.BooksCurrencyBaseRate, TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
	                    --, IssueQty=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId  THEN ISNULL(IRD.IssueQty, 0)/BaseUoMFactor
					                --    WHEN IRD.IssueQty IS NULL THEN 0 ELSE IRD.IssueQty END
						, IRD.TransactionQty, IRD.BaseQty
						,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) StockQty
						, ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty
						 ,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) AS BalanceStock

                        , BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.MaterialTranAmount/IRD.BaseQty ELSE IRD.BooksCurrencyBaseRate END
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                        ,(IRD.MaterialTranRate*IR.ToCurrencyRate) BaseCurrencyRate

                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND IR.IsApproved=1
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.FromMaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE) > CAST('" + issueDate + "' AS DATE) ORDER BY CAST(IRD.AddedDate AS DATE)";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}


		public IEnumerable<object> GetUnApprovedStockDetail(InventoryMaterialViewModel entity, string issueDate)
		{
			try
			{
				var sql = @"SELECT IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
                        
	                    , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 'NO' else 'YES' END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 'NO' else 'YES' END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor
                        , IRD.MaterialTranRate, IRD.BooksCurrencyBaseRate, TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
	                    --, IssueQty=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId  THEN ISNULL(IRD.IssueQty, 0)/BaseUoMFactor
					                --    WHEN IRD.IssueQty IS NULL THEN 0 ELSE IRD.IssueQty END
						, IRD.TransactionQty, IRD.BaseQty
						,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) StockQty
						, ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty
						 ,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) AS BalanceStock

                        , BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.MaterialTranAmount/IRD.BaseQty ELSE IRD.BooksCurrencyBaseRate END
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                        ,(IRD.MaterialTranRate*IR.ToCurrencyRate) BaseCurrencyRate
						,GateEntryNo
						,pwg.UserName GateName,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END
                        FROM [TRN].[InventoryReceiveDetail] AS IRD
						left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
						left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
						left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
						left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
						left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
                        left join trn.GateEntry G On G.id=IR.GateEntryNo
					    Left Join dbo.PlantWiseGate pwg On pwg.Id=G.PlantWiseGateId
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND IR.IsApproved=0
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + @"' AND IRD.MaterialStorageId='" + entity.FromMaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + "' AS DATE) ORDER BY CAST(IRD.AddedDate AS DATE)";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public IEnumerable<object> GetUnApprovedStockDetailBeyondIssueDate(InventoryMaterialViewModel entity, string issueDate)
		{
			try
			{
				var sql = @"SELECT IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
                        
	                    , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 'NO' else 'YES' END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 'NO' else 'YES' END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor
                        , IRD.MaterialTranRate, IRD.BooksCurrencyBaseRate, TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
	                    --, IssueQty=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId  THEN ISNULL(IRD.IssueQty, 0)/BaseUoMFactor
					                --    WHEN IRD.IssueQty IS NULL THEN 0 ELSE IRD.IssueQty END
						, IRD.TransactionQty, IRD.BaseQty
						,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) StockQty
						, ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty
						 ,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) AS BalanceStock

                        , BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.MaterialTranAmount/IRD.BaseQty ELSE IRD.BooksCurrencyBaseRate END
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                        ,(IRD.MaterialTranRate*IR.ToCurrencyRate) BaseCurrencyRate

                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id

                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND IR.IsApproved=0
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.FromMaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)> CAST('" + issueDate + "' AS DATE) ORDER BY CAST(IRD.AddedDate AS DATE)";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> GetPostingStockDetail(InventoryMaterialViewModel entity, string issueDate)
		{
			try
			{
				var sql = @"SELECT IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
                        
	                    , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 'NO' else 'YES' END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 'NO' else 'YES' END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor
                        , FORMAT(IRD.MaterialTranRate,'N4') MaterialTranRate,FORMAT(IRD.BooksCurrencyBaseRate,'N4') BooksCurrencyBaseRate, TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
	                    --, IssueQty=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId  THEN ISNULL(IRD.IssueQty, 0)/BaseUoMFactor
					                --    WHEN IRD.IssueQty IS NULL THEN 0 ELSE IRD.IssueQty END
						, IRD.TransactionQty, IRD.BaseQty
						,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) StockQty
						, ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty
						 ,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) AS BalanceStock

                        , BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN format(IRD.MaterialTranAmount/IRD.BaseQty,'N4') ELSE format(IRD.BooksCurrencyBaseRate,'N4') END
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                        ,FORMAT((IRD.MaterialTranRate*IR.ToCurrencyRate),'N4') BaseCurrencyRate
                        ,GateEntryNo
						,pwg.UserName GateName,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END
                        ,V.VoucherNo
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    LEFT JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
                    left join trn.GateEntry G On G.id=IR.GateEntryNo
					Left Join dbo.PlantWiseGate pwg On pwg.Id=G.PlantWiseGateId

                    left join trn.invoice I ON I.InventoryReceiveId=IR.Id
					left join trn.Voucher V On V.Id=I.VoucherId

                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND IR.[Status]='Posting' 
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.FromMaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
					AND IRD.BaseQty !=IRD.BaseIssueQty
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + "' AS DATE) ORDER BY CAST(IRD.AddedDate AS DATE)";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public IEnumerable<object> GetPostingStockDetailBeyondIssueDate(InventoryMaterialViewModel entity, string issueDate)
		{
			try
			{
				var sql = @"SELECT IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
                        
	                    , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 'NO' else 'YES' END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 'NO' else 'YES' END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor
                         , FORMAT(IRD.MaterialTranRate,'N4') MaterialTranRate,FORMAT(IRD.BooksCurrencyBaseRate,'N4') BooksCurrencyBaseRate, TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
	                    --, IssueQty=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId  THEN ISNULL(IRD.IssueQty, 0)/BaseUoMFactor
					                --    WHEN IRD.IssueQty IS NULL THEN 0 ELSE IRD.IssueQty END
						, IRD.TransactionQty, IRD.BaseQty
						,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) StockQty
						, ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty
						 ,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) AS BalanceStock

                         ,BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN format(IRD.MaterialTranAmount/IRD.BaseQty,'N4') ELSE format(IRD.BooksCurrencyBaseRate,'N4') END
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                        ,FORMAT((IRD.MaterialTranRate*IR.ToCurrencyRate),'N4') BaseCurrencyRate
                        ,V.VoucherNo
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
                    left join trn.invoice I ON I.InventoryReceiveId=IR.Id
					left join trn.Voucher V On V.Id=I.VoucherId

                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND IR.[Status]='Posting' 
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.FromMaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE) <= CAST('" + issueDate + "' AS DATE) ORDER BY CAST(IRD.AddedDate AS DATE)";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> GetRequisitionList(string issueDetailId)
		{
			try
			{
				var sql = @"SELECT IIH.Id, IIH.InventoryIssueDetailId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
                            , IRD.TransactionQty, StockQty=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN (IRD.BaseQty-ISNULL(IRD.IssueQty, 0))/BaseUoMFactor ELSE IRD.TransactionQty-ISNULL(IRD.IssueQty, 0) END
                            , TUoM.UserName AS TUoM, IRD.TransactionUoMId, IRD.BaseQty, IRD.BaseUOMId, IRD.BaseUoMFactor, IR.Id AS GRNNo, IRD.POId AS PONo
                            , IRD.MaterialTranRate, IRD.TrnCurrencyBaseRate BooksCurrencyBaseRate, TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.TotalMaterialBooksCurrencyAmount BooksCurrencyBaseAmount
                            , IssueQty=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId  THEN ISNULL(IRD.IssueQty, 0)/BaseUoMFactor
				                            WHEN IRD.IssueQty IS NULL THEN 0 ELSE IRD.IssueQty END
                            , BaseRate=IRD.BooksCurrencyBaseRate
                            , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, IIH.Qty AS RequisitionQty
                        FROM [TRN].[InventoryIssueHistory] AS IIH 
                        JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IIH.InventoryReceiveDetailId=IRD.Id
                        JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                        JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                        JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                        JOIN [TRN].[InventoryIssueDetail] AS IID ON IIH .InventoryIssueDetailId=IID.Id
                        JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id
                        WHERE IIH.InventoryIssueDetailId='" + issueDetailId + "'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public void InsertOrUpdateFromReceive(InventoryMaterialViewModel entity)
		{
			try
			{
				if (string.IsNullOrEmpty(entity.InventoryMaterialId)) //&& string.IsNullOrEmpty(entity.ArticleId) && string.IsNullOrEmpty(entity.CountryId) && string.IsNullOrEmpty(entity.FirstCharacteristicsValueId) && string.IsNullOrEmpty(entity.SecondCharacteristicsValueId) && string.IsNullOrEmpty(entity.ThirdCharacteristicsValueId))
				{
					entity.InventoryMaterialId = GetPK();
					var material = ValueAssignInventoryMaterial(entity);
					AuditService.AddedLog(material);
					InsertGraph(material);
				}
				else
				{
					var material = ValueAssignInventoryMaterial(entity);
					AuditService.UpdatedLog(material);
					UpdateGraph(material);
				}
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public void UpdateFromReceive(string id, string receiveDetailId)
		{
			try
			{
				var totalQty = _inventoryMaterialRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(BaseQty),0) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryMaterialId='" + id + "' AND Id NOT IN ('" + receiveDetailId + "')").First();
				var avgRate = _inventoryMaterialRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(MaterialTranAmount)/SUM(BaseQty),0) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryMaterialId='" + id + "' AND Id NOT IN ('" + receiveDetailId + "')").First();
				var data = Find(id);
				if (data.IsNotNull())
				{
					data.TotalQty = totalQty;
					data.AvgRate = avgRate;
					base.UpdateGraph(data);
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		private static InventoryMaterial ValueAssignInventoryMaterial(InventoryMaterialViewModel entity)
		{
			return new InventoryMaterial
			{
				Id = entity.InventoryMaterialId,
				CountryId = entity.CountryId,
				CompanyGroupId = entity.CompanyGroupId,
				CompanyId = entity.CompanyId,
				PlantId = entity.PlantId,
				MaterialStorageId = null,
				OpeningBalanceId = entity.OpeningBalanceId,
				MaterialMasterId = entity.MaterialMasterId,
				ArticleId = entity.ArticleId,
				FirstCharacteristicsId = entity.FirstCharacteristicsId,
				FirstCharacteristicsValueId = entity.FirstCharacteristicsValueId,
				SecondCharacteristicsId = entity.SecondCharacteristicsId,
				SecondCharacteristicsValueId = entity.SecondCharacteristicsValueId,
				ThirdCharacteristicsId = entity.ThirdCharacteristicsId,
				ThirdCharacteristicsValueId = entity.ThirdCharacteristicsValueId,
				TotalQty = entity.TotalQty,
				AvgRate = entity.AvgRate,
				ShortageQty = Convert.ToDecimal(entity.ShortageQty),
				RejectionQty = Convert.ToDecimal(entity.RejectionQty),
				ApprovedQty = Convert.ToDecimal(entity.ApprovedQty)
			};
		}

		public InventoryMaterial GetInventoryMaterialByUpToSku(InventoryMaterialViewModel entity)
		{
			return Query(t => t.MaterialMasterId == entity.MaterialMasterId && t.ArticleId == entity.ArticleId
								&& t.FirstCharacteristicsId == entity.FirstCharacteristicsId && t.FirstCharacteristicsValueId == entity.FirstCharacteristicsValueId
								&& t.SecondCharacteristicsId == entity.SecondCharacteristicsId && t.SecondCharacteristicsValueId == entity.SecondCharacteristicsValueId
								&& t.ThirdCharacteristicsId == entity.ThirdCharacteristicsId && t.ThirdCharacteristicsValueId == entity.ThirdCharacteristicsValueId
								&& t.CountryId == entity.CountryId && t.CompanyId == entity.CompanyId && t.PlantId == entity.PlantId).Select().FirstOrDefault();
		}

		public IEnumerable<InventoryMaterial> GetInventoryMaterialListByUpToSku(IEnumerable<InventoryMaterialViewModel> entities, string companyId, string plantId)
		{
			var materialIds = entities.Select(t => t.MaterialMasterId);
			var articleIds = entities.Select(t => t.ArticleId);
			var firstValueIds = entities.Select(t => t.FirstCharacteristicsValueId);
			var secondValueIds = entities.Select(t => t.SecondCharacteristicsValueId);
			var thirdValueIds = entities.Select(t => t.ThirdCharacteristicsValueId);
			var countryIds = entities.Select(t => t.CountryId);

			return Query(t => materialIds.Contains(t.MaterialMasterId) && articleIds.Contains(t.ArticleId)
							&& firstValueIds.Contains(t.FirstCharacteristicsValueId)
							&& secondValueIds.Contains(t.SecondCharacteristicsValueId)
							&& thirdValueIds.Contains(t.ThirdCharacteristicsValueId)
							&& t.CompanyId == companyId
							&& t.PlantId == plantId
							 //&& countryIds.Contains(t.CountryId)
							 ).Select().ToList();
		}

		public IEnumerable<InventoryMaterial> GetJWInventoryMaterialListByUpToSku(IEnumerable<InventoryMaterialViewModel> entities, string companyId, string plantId)
		{
			var materialIds = entities.Select(t => t.MaterialMasterId);
			var articleIds = entities.Select(t => t.ArticleId);
			var firstValueIds = entities.Select(t => t.FirstCharacteristicsValueId);
			var secondValueIds = entities.Select(t => t.SecondCharacteristicsValueId);
			var thirdValueIds = entities.Select(t => t.ThirdCharacteristicsValueId);
			var countryIds = entities.Select(t => t.CountryId);

			return Query(t => materialIds.Contains(t.MaterialMasterId) && articleIds.Contains(t.ArticleId)
							//&& firstValueIds.Contains(t.FirstCharacteristicsValueId)
							//&& secondValueIds.Contains(t.SecondCharacteristicsValueId)
							//&& thirdValueIds.Contains(t.ThirdCharacteristicsValueId)
							//&& t.CompanyId == companyId
							&& t.PlantId == plantId
							 //&& countryIds.Contains(t.CountryId)
							 ).Select().ToList();
		}
		public IEnumerable<InventoryMaterial> GetInventoryMaterialListByUpToSkuSales(IEnumerable<InventoryMaterialViewModel> entities, string companyId, string plantId)
		{
			var materialIds = entities.Select(t => t.MaterialMasterId);
			var articleIds = entities.Select(t => t.ArticleId);
			var firstValueIds = entities.Select(t => t.FirstCharacteristicsValueId);
			var secondValueIds = entities.Select(t => t.SecondCharacteristicsValueId);
			var thirdValueIds = entities.Select(t => t.ThirdCharacteristicsValueId);
			var countryIds = entities.Select(t => t.CountryId);

			return Query(t => materialIds.Contains(t.MaterialMasterId) && articleIds.Contains(t.ArticleId) &&
							 firstValueIds.Contains(t.FirstCharacteristicsValueId) && secondValueIds.Contains(t.SecondCharacteristicsValueId) &&
							 thirdValueIds.Contains(t.ThirdCharacteristicsValueId) && t.CompanyId == companyId && t.PlantId == plantId
							 //&& countryIds.Contains(t.CountryId)
							 ).Select().ToList();
		}
		public IEnumerable<InventoryMaterial> GetInventoryMaterialListByUpToSkuScrap(IEnumerable<InventoryMaterialViewModel> entities, string companyId, string plantId)
		{
			var materialIds = entities.Select(t => t.MaterialMasterId);
			var articleIds = entities.Select(t => t.ArticleId);
			var firstValueIds = entities.Select(t => t.FirstCharacteristicsValueId);
			var secondValueIds = entities.Select(t => t.SecondCharacteristicsValueId);
			var thirdValueIds = entities.Select(t => t.ThirdCharacteristicsValueId);
			var countryIds = entities.Select(t => t.CountryId);

			return Query(t => materialIds.Contains(t.MaterialMasterId) && articleIds.Contains(t.ArticleId) &&
							 firstValueIds.Contains(t.FirstCharacteristicsValueId) && secondValueIds.Contains(t.SecondCharacteristicsValueId) &&
							 thirdValueIds.Contains(t.ThirdCharacteristicsValueId) && t.CompanyId == companyId && t.PlantId == plantId
							 //&& countryIds.Contains(t.CountryId)
							 ).Select().ToList();
		}

		public IEnumerable<InventoryMaterial> GetInventoryIssueMaterialListByUpToSku(IEnumerable<RequisitionIssueDetailViewModel> entities, string companyId, string plantId)
		{
			var materialIds = entities.Select(t => t.MaterialMasterId);
			var articleIds = entities.Select(t => t.ArticleId);
			var firstValueIds = entities.Select(t => t.FirstCharacteristicsValueId);
			var secondValueIds = entities.Select(t => t.SecondCharacteristicsValueId);
			var thirdValueIds = entities.Select(t => t.ThirdCharacteristicsValueId);
			var countryIds = entities.Select(t => t.CountryId);

			return Query(t => materialIds.Contains(t.MaterialMasterId) && articleIds.Contains(t.ArticleId) &&
							 firstValueIds.Contains(t.FirstCharacteristicsValueId) && secondValueIds.Contains(t.SecondCharacteristicsValueId) &&
							 thirdValueIds.Contains(t.ThirdCharacteristicsValueId) && t.CompanyId == companyId && t.PlantId == plantId
							 //&& countryIds.Contains(t.CountryId)
							 ).Select().ToList();
		}



		public IEnumerable<object> PurchaseReturnDetailsData(string PurchaseReturnId, string POID)

		{
			//var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{

				var sql = @"DECLARE @inventoryReceiveId VARCHAR(10)='19123'
                                                     , @totalReceiveAmount DECIMAL(18, 4)=0
                                                     , @totalServiceAmount DECIMAL(18, 4)=0
                                                     , @totalSvcTaxAmount DECIMAL(18, 4)=0
                                  SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                                  SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@inventoryReceiveId)
                                  SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
                                  SELECT IM.Id PurchaseReturnDetailId,IM.PurchaseReturnId
                                        --,REPLACE(CONVERT(CHAR(11), PID.AddedDate, 106),' ','-') AS AddedDate
                                        , MGM.UserName AS MaterialGroupMasterName
                                        , IM.MaterialMasterId, MM.UserName MaterialName
                                        , IM.ArticleId, ART.StandardName Article
                                        , IM.FirstCharacteristicsId, FC.UserName AS SKU1
                                        , IM.FirstCharacteristicsValueId, FCV.UserName AS SKU2
                                        , IM.SecondCharacteristicsId, SC.UserName AS SKU3
                                        , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                                        , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                                        , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
                                        , IM.TransactionUoMId, TUoM.UserName AS TransactionUoM
                                        , IM.MaterialTranRate AS TransactionRate
                                        , CU.Code AS CurrencyName, IR.ToCurrencyRate
                                        , (IM.TransactionQty*IM.MaterialTranRate) AS TrnAmount
                                        , IM.ToTalMaterialBooksCurrencyAmount AS BaseAmount
                                        , IM.TotalTaxAmount AS BaseTaxAmount
                                        , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IM.Id)
                                        , IM.ChargesTranAmount AS ChargesAmount	                      
                                        , ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IM.MaterialTranAmount
                                        , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IM.MaterialTranAmount
                                        , IM.CountryId
                                       -- , PID.TransactionQty AS POQty
                                        --, ISNULL(Pre.OtherReceived,0) OtherReceived	                       
                                        , IM.TransactionQty TransactionQty   
                                       -- , IM.BaseIssueQty
                                        --, (ISNULl(IM.TransactionQty,0)-ISNULL(IM.BaseIssueQty,0)) AS Balance
                                        ,0 OtherReturned
                                        --, (ISNULl(IM.TransactionQty,0)-ISNULL(IM.BaseIssueQty,0)) TransactionQty
                                        , IM.TransactionUoMId
                                        , IM.BaseUOMId   
                                        , IM.TotalMaterialTranAmount
                                        , IM.TotalMaterialBooksCurrencyAmount AS TotalMaterialBaseAmount
                                       
                                        --,IM.PurchaseDocumentAcceptanceDetailId
										--,IM.PurchaseDocumentAcceptanceId
                                       -- , IM.ShortageQty
                                       -- , IM.RejectionQty
                                        --, IM.ApprovedQty
                                        , IM.TransactionQty AS PreviousQty
                                        , EI.EmployeeName CheckedByName
										, EI1.EmployeeName ApprovedByName,MS.UserName MaterialStorage
                                        --, IM.ShortageRatePercent AS ShortageRate,IM.ShortageValue,IM.RejectRatePercent AS RejectionRate,IM.RejectValue AS RejectionValue,IM.RejectClamPercent RejectionClamRate
										,IR.CheckedBy
										,IM.Description MaterialDetail
                                  from TRN.PurchaseReturnDetail AS IM
                                  left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                                  LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                  LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                                  LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                                  LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                                  LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                                  --LEFT jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id and ird.InventoryReceiveId='19123'
                                  --LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId   
             --                     LEFT JOIN (select PODetailsId,  Sum(TransactionQty) as OtherReceived 
											  --from trn.InventoryReceiveDetail where InventoryReceiveId not in('19123') --AND POid='null'
											  --Group By PODetailsId
											  --) AS Pre on pre.PODetailsId=IRD.PODetailsId
                                LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId=TUoM.Id
                                LEFT JOIN [TRN].[InventoryReceive] AS IR ON IM.InventoryReceiveId=IR.Id
                                LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                --LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID=PID.RequisitionDetailId
                                LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
								LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                                Left join [HKP].[MaterialStorage] MS on MS.id=IM.MaterialStorageId
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

		public Dictionary<string, object> GetStockForPhysicalStock(InventoryMaterialViewModel entity, string issueDate)
		{
			try
			{
				var sql = @"--SELECT IM.TotalQty FROM TRN.InventoryMaterial AS IM WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "'AND IM.PlantId='" + entity.PlantId + @"' 
                            --AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + @"'
                            --AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                            --AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + @"'
                            --AND IM.Id IN(SELECT DISTINCT A.InventoryMaterialId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
                            --        WHERE A.MaterialStorageId='" + entity.MaterialStorageId + "' AND CAST(B.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE))
 SELECT  sum(t.TotalQty) TotalQty,sum(t.PostingQty) PostingQty,sum(t.PostingQty) PostingQuantity,sum(t.ApprovedQty) ApprovedQty,sum(t.UnApprovedQty) UnApprovedQty
                      --SELECT  REPLACE(CONVERT(varchar(20), (CAST(max(t.TotalQty) AS money)), 1), '.00', '') AS TotalQty
					  --,REPLACE(CONVERT(varchar(20), (CAST(max(t.PostingQty) AS money)), 1), '.00', '') AS PostingQty
					  --,max(t.PostingQty) AS PostingQuantity
					  --,REPLACE(CONVERT(varchar(20), (CAST(max(t.ApprovedQty) AS money)), 1), '.00', '') AS ApprovedQty
					  --, REPLACE(CONVERT(varchar(20), (CAST(max(t.UnApprovedQty) AS money)), 1), '.00', '') AS UnApprovedQty 
                        from(
		              SELECT TotalQty=(SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0)) + SUM(ISNULL(IRD.IssueReturnQty,0)) - SUM(ISNULL(IRD.PurchaseReturnQty,0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty,0))- SUM(ISNULL(IRD.InventorySalesQty,0))- SUM(ISNULL(IRD.InventoryScrapQty,0))), 0 PostingQty, 0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' --AND IR.[Status]='Posting' 
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
            UNION ALL
            SELECT 0 TotalQty, PostingQty=(SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0)) + SUM(ISNULL(IRD.IssueReturnQty,0)) - SUM(ISNULL(IRD.PurchaseReturnQty,0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty,0))- SUM(ISNULL(IRD.InventorySalesQty,0))- SUM(ISNULL(IRD.InventoryScrapQty,0))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status]='Posting' 
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
                    UNION ALL
                    SELECT 0 TotalQty, PostingQty=(SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0)) + SUM(ISNULL(IRD.IssueReturnQty,0)) - SUM(ISNULL(IRD.PurchaseReturnQty,0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty,0))- SUM(ISNULL(IRD.InventorySalesQty,0))- SUM(ISNULL(IRD.InventoryScrapQty,0))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status]='Posting' AND IR.IsApproved=1 AND IR.RequiredPosting=1 AND IR.GRNType='MaterialTransfer'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
                    UNION ALL
                    SELECT 0 TotalQty, PostingQty=(SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0)) + SUM(ISNULL(IRD.IssueReturnQty,0)) - SUM(ISNULL(IRD.PurchaseReturnQty,0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty,0))- SUM(ISNULL(IRD.InventorySalesQty,0))- SUM(ISNULL(IRD.InventoryScrapQty,0))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status] IS NULL AND IR.IsApproved=1 AND IR.RequiredPosting=0 AND IR.GRNType='MaterialTransfer'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
            UNION ALL
           SELECT 0 TotalQty,0 PostingQty,ApprovedQty=(SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0)) + SUM(ISNULL(IRD.IssueReturnQty,0)) - SUM(ISNULL(IRD.PurchaseReturnQty,0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty,0))- SUM(ISNULL(IRD.InventorySalesQty,0))- SUM(ISNULL(IRD.InventoryScrapQty,0))), 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.IsApproved=1
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
                UNION ALL
                SELECT 0 TotalQty,0 PostingQty,0 ApprovedQty, UnApprovedQty=(SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0)) + SUM(ISNULL(IRD.IssueReturnQty,0)) - SUM(ISNULL(IRD.PurchaseReturnQty,0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty,0))- SUM(ISNULL(IRD.InventorySalesQty,0))- SUM(ISNULL(IRD.InventoryScrapQty,0)))
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.IsApproved=0
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
) AS t";
				return _sqlRepository.GetData(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> GetSpecificMaterialStockForPhysicalStock(InventoryMaterialViewModel entity, string issueDate)
		{
			try
			{
				var sql = @"SELECT IRD.InventoryReceiveId, IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
	                    , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 'NO' else 'YES' END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 'NO' else 'YES' END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor
                        , IRD.MaterialTranRate, IRD.BooksCurrencyBaseRate, TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
                        , BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.MaterialTranAmount/IRD.BaseQty ELSE IRD.BooksCurrencyBaseRate END
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                        ,(IRD.MaterialTranRate * IR.ToCurrencyRate) BaseCurrencyRate
						, IRD.TransactionQty, IRD.BaseQty
						,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) StockQty
						, ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty, ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty, ISNULL(IRD.IssueReturnQty,0) IssueReturnQty, ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty , ISNULL(IRD.InventorySalesQty,0) InventorySalesQty, ISNULL(IRD.InventoryScrapQty,0) InventoryScrapQty,ISNULL(IRD.InventoryTransferQty,0) InventoryTransferQty
	                    ,(ISNULL(IRD.BaseQty,0) - (ISNULL(IRD.BaseIssueQty, 0) + ISNULL(IRD.PurchaseReturnQty,0)+ ISNULL(IRD.ReductionByAdjustmentQty,0)+ISNULL(IRD.InventorySalesQty,0)+ISNULL(IRD.InventoryScrapQty,0)+ISNULL(IRD.InventoryTransferQty,0)) + ISNULL(IRD.IssueReturnQty,0)) AS BalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						 ,ISNULL(IRD.BooksCurrencyBaseRate,0) BooksCurrencyBaseRate
						 ,ISNULL(IRD.TrnCurrencyBaseRate,0) TrnCurrencyBaseRate
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' --AND IR.[Status]='Posting' 
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
					AND IRD.BaseQty !=IRD.BaseIssueQty
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + "' AS DATE) ORDER BY IR.AddedDate";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public Dictionary<string, object> GetStockSales(InventoryMaterialViewModel entity, string issueDate)
		{
			try
			{
				var sql = @"--SELECT IM.TotalQty FROM TRN.InventoryMaterial AS IM WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "'AND IM.PlantId='" + entity.PlantId + @"' 
                            --AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + @"'
                            --AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                            --AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + @"'
                            --AND IM.Id IN(SELECT DISTINCT A.InventoryMaterialId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
                            --        WHERE A.MaterialStorageId='" + entity.MaterialStorageId + "' AND CAST(B.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE))
 SELECT  sum(t.TotalQty) TotalQty,sum(t.PostingQty) PostingQty,sum(t.PostingQty) PostingQuantity,sum(t.ApprovedQty) ApprovedQty,sum(t.UnApprovedQty) UnApprovedQty
                      --SELECT  REPLACE(CONVERT(varchar(20), (CAST(max(t.TotalQty) AS money)), 1), '.00', '') AS TotalQty
					  --,REPLACE(CONVERT(varchar(20), (CAST(max(t.PostingQty) AS money)), 1), '.00', '') AS PostingQty
					  --,max(t.PostingQty) AS PostingQuantity
					  --,REPLACE(CONVERT(varchar(20), (CAST(max(t.ApprovedQty) AS money)), 1), '.00', '') AS ApprovedQty
					  --, REPLACE(CONVERT(varchar(20), (CAST(max(t.UnApprovedQty) AS money)), 1), '.00', '') AS UnApprovedQty 
                        from(
		              SELECT TotalQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))-SUM(ISNULL(IRD.InventoryTransferQty, 0)))), 0 PostingQty, 0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    LEFT JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
            UNION ALL
            SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))-SUM(ISNULL(IRD.InventoryTransferQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    LEFT JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status]='Posting' 
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
                    UNION ALL
                    SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))-SUM(ISNULL(IRD.InventoryTransferQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    LEFT JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status]='Posting' AND IR.IsApproved=1 AND IR.RequiredPosting=1 AND IR.GRNType='MaterialTransfer'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
                    UNION ALL
                    SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))-SUM(ISNULL(IRD.InventoryTransferQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    LEFT JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status] IS NULL AND IR.IsApproved=1 AND IR.RequiredPosting=0 AND IR.GRNType='MaterialTransfer'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
            UNION ALL
           SELECT 0 TotalQty,0 PostingQty,ApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))-SUM(ISNULL(IRD.InventoryTransferQty, 0)))), 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    LEFT JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.IsApproved=1
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
                UNION ALL
                SELECT 0 TotalQty,0 PostingQty,0 ApprovedQty, UnApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))-SUM(ISNULL(IRD.InventoryTransferQty, 0))))
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    LEFT JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.IsApproved=0
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
) AS t";
				return _sqlRepository.GetData(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public Dictionary<string, object> GetStockScrap(InventoryMaterialViewModel entity, string issueDate)
		{
			try
			{
				var sql = @"--SELECT IM.TotalQty FROM TRN.InventoryMaterial AS IM WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "'AND IM.PlantId='" + entity.PlantId + @"' 
                            --AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + @"'
                            --AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                            --AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + @"'
                            --AND IM.Id IN(SELECT DISTINCT A.InventoryMaterialId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
                            --        WHERE A.MaterialStorageId='" + entity.MaterialStorageId + "' AND CAST(B.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE))
                    SELECT  sum(t.TotalQty) TotalQty,sum(t.PostingQty) PostingQty,sum(t.PostingQty) PostingQuantity,sum(t.ApprovedQty) ApprovedQty,sum(t.UnApprovedQty) UnApprovedQty
                      --SELECT  REPLACE(CONVERT(varchar(20), (CAST(max(t.TotalQty) AS money)), 1), '.00', '') AS TotalQty
					  --,REPLACE(CONVERT(varchar(20), (CAST(max(t.PostingQty) AS money)), 1), '.00', '') AS PostingQty
					  --,max(t.PostingQty) AS PostingQuantity
					  --,REPLACE(CONVERT(varchar(20), (CAST(max(t.ApprovedQty) AS money)), 1), '.00', '') AS ApprovedQty
					  --, REPLACE(CONVERT(varchar(20), (CAST(max(t.UnApprovedQty) AS money)), 1), '.00', '') AS UnApprovedQty 
                        from(
		              SELECT TotalQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 PostingQty, 0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
            UNION ALL
            SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status]='Posting' 
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
                    UNION ALL
                    SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status]='Posting' AND IR.IsApproved=1 AND IR.RequiredPosting=1 AND IR.GRNType='MaterialTransfer' 
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
                    UNION ALL
                    SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status] IS NULL AND IR.IsApproved=1 AND IR.RequiredPosting=0 AND IR.GRNType='MaterialTransfer' 
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
            UNION ALL
           SELECT 0 TotalQty,0 PostingQty,ApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.IsApproved=1
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
                UNION ALL
                SELECT 0 TotalQty,0 PostingQty,0 ApprovedQty, UnApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))))
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.IsApproved=0
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
) AS t";
				return _sqlRepository.GetData(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}


		public Dictionary<string, object> GetMaterialTransferStock(InventoryMaterialViewModel entity, string issueDate)
		{
			try
			{
				var sql = @"--SELECT IM.TotalQty FROM TRN.InventoryMaterial AS IM WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "'AND IM.PlantId='" + entity.PlantId + @"' 
                            --AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + @"'
                            --AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                            --AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + @"'
                            --AND IM.Id IN(SELECT DISTINCT A.InventoryMaterialId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
                            --        WHERE A.MaterialStorageId='" + entity.MaterialStorageId + "' AND CAST(B.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE))
                    SELECT  sum(t.TotalQty) TotalQty,sum(t.PostingQty) PostingQty,sum(t.PostingQty) PostingQuantity,sum(t.ApprovedQty) ApprovedQty,sum(t.UnApprovedQty) UnApprovedQty
                      --SELECT  REPLACE(CONVERT(varchar(20), (CAST(max(t.TotalQty) AS money)), 1), '.00', '') AS TotalQty
					  --,REPLACE(CONVERT(varchar(20), (CAST(max(t.PostingQty) AS money)), 1), '.00', '') AS PostingQty
					  --,max(t.PostingQty) AS PostingQuantity
					  --,REPLACE(CONVERT(varchar(20), (CAST(max(t.ApprovedQty) AS money)), 1), '.00', '') AS ApprovedQty
					  --, REPLACE(CONVERT(varchar(20), (CAST(max(t.UnApprovedQty) AS money)), 1), '.00', '') AS UnApprovedQty 
                        from(
		              SELECT TotalQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))-SUM(ISNULL(IRD.InventoryTransferQty, 0)))), 0 PostingQty, 0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    LEFT JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.FromMaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
            UNION ALL
            SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))-SUM(ISNULL(IRD.InventoryTransferQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    LEFT JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status]='Posting' 
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.FromMaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
                    UNION ALL
                    SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))-SUM(ISNULL(IRD.InventoryTransferQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    LEFT JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status]='Posting' AND IR.IsApproved=1 AND IR.RequiredPosting=1 AND IR.GRNType='MaterialTransfer'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.FromMaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
                    UNION ALL
                    SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))-SUM(ISNULL(IRD.InventoryTransferQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    LEFT JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status] IS NULL AND IR.IsApproved=1 AND IR.RequiredPosting=0 AND IR.GRNType='MaterialTransfer'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.FromMaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
            UNION ALL
           SELECT 0 TotalQty,0 PostingQty,ApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))-SUM(ISNULL(IRD.InventoryTransferQty, 0)))), 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    LEFT JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.IsApproved=1
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.FromMaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
                UNION ALL
                SELECT 0 TotalQty,0 PostingQty,0 ApprovedQty, UnApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))-SUM(ISNULL(IRD.InventoryTransferQty, 0))))
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    LEFT JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.IsApproved=0
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.FromMaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
                ) AS t";
				return _sqlRepository.GetData(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		//public Dictionary<string, object> GetPopUpShowStorageLocation(InventoryMaterialViewModel entity, string issueDate) 
		public IEnumerable<object> GetPopUpShowStorageLocation(InventoryMaterialViewModel entity, string issueDate)
		{
			try
			{
				var sql = @"
                    SELECT  StorageLocationName,sum(t.TotalQty) TotalQty,sum(t.PostingQty) PostingQty,sum(t.PostingQty) PostingQuantity,sum(t.ApprovedQty) ApprovedQty,sum(t.UnApprovedQty) UnApprovedQty
                      
                        from(
		              SELECT TotalQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(II.IssueQty, 0))-SUM(ISNULL(PurchaseReturnData.Qty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(ISD.InvSalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 PostingQty, 0 ApprovedQty, 0 UnApprovedQty
                        ,MS.UserName StorageLOcationName                    
                        FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					left JOIn [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    left join (select IID.InventoryMaterialId,ii.MaterialStorageId,IH.InventoryReceiveDetailId, Sum(IH.Qty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
								WHERE convert(Date,II.IssueDate) <= CAST('" + issueDate + @"' AS DATE) AND II.PlantId='" + entity.PlantId + @"'
								GROUP BY IID.InventoryMaterialId,ii.MaterialStorageId,IH.InventoryReceiveDetailId
								) II On II.InventoryMaterialId=IM.Id and II.MaterialStorageId=IRD.MaterialStorageId AND II.InventoryReceiveDetailId=IRD.Id
					Left join (select IH.InventoryMaterialId,II.MaterialStorageId,sum(IH.BaseQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) <= CAST('" + issueDate + @"' AS DATE) AND II.PlantId='" + entity.PlantId + @"' and IH.InventoryMaterialId=1900 
										GROUP BY IH.InventoryMaterialId,II.MaterialStorageId 
								 )PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id and PurchaseReturnData.MaterialStorageId=IRD.MaterialStorageId
					Left join (select ISD.InventoryMaterialId,ins.MaterialStorageId,ISH.InventoryReceiveDetailId,sum(ISH.Qty) InvSalesQty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 WHERE convert(Date,Ins.SalesDate) <= CAST('" + issueDate + @"' AS DATE) AND Ins.PlantId='" + entity.PlantId + @"'  
									 GROUP BY ISD.InventoryMaterialId,ins.MaterialStorageId,ISH.InventoryReceiveDetailId
								 )ISD ON ISD.InventoryMaterialId=IM.Id and ISD.MaterialStorageId=IRD.MaterialStorageId  AND   ISD.InventoryReceiveDetailId=IRD.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + @"'  
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
					Group BY MS.UserName
            UNION ALL
            SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(II.IssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
,MS.UserName StorageLOcationName                        
FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					left JOIn [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    left join (select IID.InventoryMaterialId,ii.MaterialStorageId,IH.InventoryReceiveDetailId, Sum(IH.Qty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
								WHERE II.PlantId='" + entity.PlantId + @"'
								GROUP BY IID.InventoryMaterialId,ii.MaterialStorageId,IH.InventoryReceiveDetailId
								) II On II.InventoryMaterialId=IM.Id and II.MaterialStorageId=IRD.MaterialStorageId AND II.InventoryReceiveDetailId=IRD.Id
					Left join (select IH.InventoryMaterialId,II.MaterialStorageId,sum(IH.BaseQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE  II.PlantId='" + entity.PlantId + @"' and IH.InventoryMaterialId=1900 
										GROUP BY IH.InventoryMaterialId,II.MaterialStorageId
								 )PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id and PurchaseReturnData.MaterialStorageId=IRD.MaterialStorageId
					Left join (select ISD.InventoryMaterialId,ins.MaterialStorageId,ISH.InventoryReceiveDetailId,sum(ISH.Qty) InvSalesQty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 WHERE Ins.PlantId='" + entity.PlantId + @"'  
									 GROUP BY ISD.InventoryMaterialId,ins.MaterialStorageId,ISH.InventoryReceiveDetailId
								 )ISD ON ISD.InventoryMaterialId=IM.Id and ISD.MaterialStorageId=IRD.MaterialStorageId  AND   ISD.InventoryReceiveDetailId=IRD.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status]='Posting' 
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
					Group BY MS.UserName
                    UNION ALL
                    SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
,MS.UserName StorageLOcationName                    
FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					left JOIn [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status]='Posting' AND IR.IsApproved=1 AND IR.RequiredPosting=1 AND IR.GRNType='MaterialTransfer'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
					Group BY MS.UserName
                    UNION ALL
                    SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
,MS.UserName StorageLOcationName                    
FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					left JOIn [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status] IS NULL AND IR.IsApproved=1 AND IR.RequiredPosting=0 AND IR.GRNType='MaterialTransfer'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
					Group BY MS.UserName
            UNION ALL
           SELECT 0 TotalQty,0 PostingQty,ApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(II.IssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 UnApprovedQty
,MS.UserName StorageLOcationName                    
FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					left JOIn [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    left join (select IID.InventoryMaterialId,ii.MaterialStorageId,IH.InventoryReceiveDetailId, Sum(IH.Qty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
								WHERE  II.PlantId='20171'
								GROUP BY IID.InventoryMaterialId,ii.MaterialStorageId,IH.InventoryReceiveDetailId
								) II On II.InventoryMaterialId=IM.Id and II.MaterialStorageId=IRD.MaterialStorageId AND II.InventoryReceiveDetailId=IRD.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.IsApproved=1
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + @"'  
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
					Group BY MS.UserName
                UNION ALL
                SELECT 0 TotalQty,0 PostingQty,0 ApprovedQty, UnApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))))
				,MS.UserName StorageLOcationName                    
FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    LEFT JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					left JOIn [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.IsApproved=0
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + @"'  
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
					Group BY MS.UserName
			) AS t Group By StorageLocationName";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

        public IEnumerable<object> StorageLocationStockWise(string MaterialMstId, string ArticleId, string issueDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT  StorageLocationName,sum(t.TotalQty) TotalQty,sum(t.PostingQty) PostingQty,sum(t.PostingQty) PostingQuantity,sum(t.ApprovedQty) ApprovedQty,sum(t.UnApprovedQty) UnApprovedQty
                        from(
		              SELECT TotalQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 PostingQty, 0 ApprovedQty, 0 UnApprovedQty
,MS.UserName StorageLOcationName                    
FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					left JOIn [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    WHERE IM.CompanyGroupId='"+identity.CompanyGroupId+@"' AND IM.CompanyId='"+identity.CompanyId+@"' AND IM.PlantId='"+identity.PlantId+@"' 
                    AND IM.MaterialMasterId='"+ MaterialMstId + @"'
                    AND ISNULL(IM.ArticleId,'')='"+ ArticleId + @"' AND ISNULL(IM.FirstCharacteristicsValueId,'')='' AND  ISNULL(IM.SecondCharacteristicsValueId,'')=''
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')=''  
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('"+ issueDate + @"' AS DATE)
					Group BY MS.UserName
            UNION ALL
            SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
,MS.UserName StorageLOcationName                        
FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					left JOIn [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    WHERE IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"' 
                    AND IR.[Status]='Posting' 
                    AND IM.MaterialMasterId='" + MaterialMstId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + ArticleId + @"' AND ISNULL(IM.FirstCharacteristicsValueId,'')='' AND  ISNULL(IM.SecondCharacteristicsValueId,'')=''
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
					Group BY MS.UserName
                    UNION ALL
                    SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
,MS.UserName StorageLOcationName                    
FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					left JOIn [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    WHERE IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"' 
                    AND IR.[Status]='Posting' AND IR.IsApproved=1 AND IR.RequiredPosting=1 AND IR.GRNType='MaterialTransfer'
                    AND IM.MaterialMasterId='" + MaterialMstId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + ArticleId + @"' AND ISNULL(IM.FirstCharacteristicsValueId,'')='' AND  ISNULL(IM.SecondCharacteristicsValueId,'')=''
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
					Group BY MS.UserName
                    UNION ALL
                    SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
,MS.UserName StorageLOcationName                    
FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					left JOIn [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    WHERE IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"' 
                    AND IR.[Status] IS NULL AND IR.IsApproved=1 AND IR.RequiredPosting=0 AND IR.GRNType='MaterialTransfer'
                    AND IM.MaterialMasterId='" + MaterialMstId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + ArticleId + @"' AND ISNULL(IM.FirstCharacteristicsValueId,'')='' AND  ISNULL(IM.SecondCharacteristicsValueId,'')=''
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
					Group BY MS.UserName
            UNION ALL
           SELECT 0 TotalQty,0 PostingQty,ApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 UnApprovedQty
,MS.UserName StorageLOcationName                    
FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					left JOIn [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    WHERE IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"' 
                    AND IR.IsApproved=1
                    AND IM.MaterialMasterId='" + MaterialMstId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + ArticleId + @"' AND ISNULL(IM.FirstCharacteristicsValueId,'')='' AND  ISNULL(IM.SecondCharacteristicsValueId,'')=''
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')=''  
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
					Group BY MS.UserName
                UNION ALL
                SELECT 0 TotalQty,0 PostingQty,0 ApprovedQty, UnApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))))
				,MS.UserName StorageLOcationName                    
FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    LEFT JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					left JOIn [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    WHERE IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"' 
                    AND IR.IsApproved=0
                    AND IM.MaterialMasterId='" + MaterialMstId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + ArticleId + @"' AND ISNULL(IM.FirstCharacteristicsValueId,'')='' AND  ISNULL(IM.SecondCharacteristicsValueId,'')=''
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')=''  
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
					Group BY MS.UserName
			) AS t Group By StorageLocationName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public void JWInsertOrUpdateFromReceive(InventoryMaterialViewModel entity)
		{
			try
			{
				if (string.IsNullOrEmpty(entity.InventoryMaterialId)) //&& string.IsNullOrEmpty(entity.ArticleId) && string.IsNullOrEmpty(entity.CountryId) && string.IsNullOrEmpty(entity.FirstCharacteristicsValueId) && string.IsNullOrEmpty(entity.SecondCharacteristicsValueId) && string.IsNullOrEmpty(entity.ThirdCharacteristicsValueId))
				{
					entity.InventoryMaterialId = GetPK();
					var material = ValueAssignInventoryMaterial(entity);
					AuditService.AddedLog(material);
					InsertGraph(material);
				}
				else
				{
					var material = ValueAssignInventoryMaterial(entity);
					AuditService.UpdatedLog(material);
					UpdateGraph(material);
				}
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public InventoryMaterial JWGetInventoryMaterialByUpToSku(InventoryMaterialViewModel entity)
		{
			return Query(t => t.MaterialMasterId == entity.MaterialMasterId
						&& t.ArticleId == entity.ArticleId
						//&& t.FirstCharacteristicsId == entity.FirstCharacteristicsId && t.FirstCharacteristicsValueId == entity.FirstCharacteristicsValueId
						//&& t.SecondCharacteristicsId == entity.SecondCharacteristicsId && t.SecondCharacteristicsValueId == entity.SecondCharacteristicsValueId
						//&& t.ThirdCharacteristicsId == entity.ThirdCharacteristicsId && t.ThirdCharacteristicsValueId == entity.ThirdCharacteristicsValueId
						//&& t.CountryId == entity.CountryId 
						&& t.CompanyId == entity.CompanyId
						&& t.PlantId == entity.PlantId).Select().FirstOrDefault();
		}



		public IEnumerable<object> JWOutPutQuery(string inveReveiveId)
		{
			try
			{
				var sql = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
                                  ,@totalReceiveAmount DECIMAL(18, 4)=0
                                  ,@totalServiceAmount DECIMAL(18, 4)=0
                                  ,@totalSvcTaxAmount DECIMAL(18, 4)=0
                                  SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                                  SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@inventoryReceiveId)
                                  SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
                                  SELECT IM.Id, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
										,tc1.Id OSTransformationPOId
										,mp.Id OSTransformationPODetailId
										,jwi.UserName as JWOutputItem
										,jwa.UserName as JobWorkActivity
                                      
                                      , MGM.UserName AS MaterialGroupMasterName
                                      , IM.MaterialMasterId, MM.UserName
                                      , IM.ArticleId, ART.StandardName
                                      , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                                      , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                                      , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                                      , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                                      , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                                      , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
                                      , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
                                      , IRD.MaterialTranRate AS TransactionRate
                                      , CU.Code AS CurrencyName, IR.ToCurrencyRate
                                      , IRD.MaterialTranAmount AS TrnAmount
                                      , IRD.ToTalMaterialBooksCurrencyAmount AS BaseAmount
                                      , IRD.TotalTaxAmount AS BaseTaxAmount
                                      , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
                                      , IRD.ChargesTranAmount AS ChargesAmount	                      
                                      , ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
                                      , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
                                   
                                      , mp.Quantity AS PlanQuantity
                                       ,ISNULL(Pre.GRNRcvQty,'0') AS GRNRcvQty                       
                                      ,IRD.GRNQty TransactionQty                         
                					  ,(mp.Quantity-IRD.GRNQty-ISNULL(Pre.GRNRcvQty,0)) AS Balance                            
									  ,IRD.TransactionUoMId
                					  ,IRD.BaseUOMId   
                                      ,IRD.TotalMaterialTranAmount
                                      ,IRD.TotalMaterialBooksCurrencyAmount AS TotalMaterialBaseAmount
                                      ,IRD.ShortageQty
                					  ,IRD.RejectionQty
                					  ,IRD.ApprovedQty
                                      ,IRD.TransactionQty AS PreviousQty
                                      ,IRD.ShortageRatePercent AS ShortageRate,IRD.ShortageValue,IRD.RejectRatePercent AS RejectionRate,IRD.RejectValue AS RejectionValue,IRD.RejectClamPercent RejectionClamRate,IR.CheckedBy,c.UserName CountryName,C.Id CountryId ,IRD.GRNQty-IRD.ShortageQty AS NetQty,MS.Id MaterialStorageId,IRD.GrossAmount,IRD.DiscountAmount,IRD.QualityStatus
								  FROM TRN.InventoryMaterial AS IM
                                  LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                                  LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                  LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                                  LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                                  LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                                  LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                                  LEFT jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id and ird.InventoryReceiveId='" + inveReveiveId + @"'
                                  LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId   
                                  LEFT JOIN (select OSTransformationPODetailId,  Sum(TransactionQty) as GRNRcvQty 
                                  from trn.InventoryReceiveDetail where InventoryReceiveId not in('" + inveReveiveId + @"') 
                                  Group By OSTransformationPODetailId) AS Pre on pre.OSTransformationPODetailId=IRD.OSTransformationPODetailId

								JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
								JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
								JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
								LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID=PID.RequisitionDetailId
								left join scs.country C ON C.Id=IM.CountryId 
								LEFT JOIN  [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
								Left JOIN dbo.OSTransformationPODetail mp On mp.Id=IRD.OSTransformationPODetailId 
								--left join dbo.JobWorkTransformationContract tc1 on tc1.Id=mp.OSTransformationPOId
								left join dbo.OSTransformationPO tc1 on tc1.Id=mp.OSTransformationPOId
								left join hkp.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
								left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
								WHERE IRD.InventoryReceiveId=@inventoryReceiveId and IM.MaterialMasterId IS not null AND IRD.MaterialFor='JWOUTPUTMaterial'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		// Job Work

		public IEnumerable<object> JobWorkOutPutQuery(string inveReveiveId)
		{
			try
			{
				var sql = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
                                  ,@totalReceiveAmount DECIMAL(18, 4)=0
                                  ,@totalServiceAmount DECIMAL(18, 4)=0
                                  ,@totalSvcTaxAmount DECIMAL(18, 4)=0
                                  SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                                  SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@inventoryReceiveId)
                                  SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
                                  SELECT IM.Id, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
										,tc1.Id JWTransformationPOId
										,mp.Id JWTransformationPODetailId
										,jwi.UserName as JWOutputItem
										,jwa.UserName as JobWorkActivity
                                      
                                      , MGM.UserName AS MaterialGroupMasterName
                                      , IM.MaterialMasterId, MM.UserName
                                      , IM.ArticleId, ART.StandardName
                                      , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                                      , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                                      , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                                      , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                                      , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                                      , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
                                      , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
                                      , IRD.MaterialTranRate AS TransactionRate
                                      , CU.Code AS CurrencyName, IR.ToCurrencyRate
                                      , IRD.MaterialTranAmount AS TrnAmount
                                      , IRD.ToTalMaterialBooksCurrencyAmount AS BaseAmount
                                      , IRD.TotalTaxAmount AS BaseTaxAmount
                                      , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
                                      , IRD.ChargesTranAmount AS ChargesAmount	                      
                                      , ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
                                      , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
                                   
                                      , mp.Quantity AS PlanQuantity
                                       ,ISNULL(Pre.GRNRcvQty,'0') AS GRNRcvQty                       
                                      ,IRD.GRNQty TransactionQty                         
                					  ,(mp.Quantity-IRD.GRNQty-ISNULL(Pre.GRNRcvQty,0)) AS Balance                            
									  ,IRD.TransactionUoMId
                					  ,IRD.BaseUOMId   
                                      ,IRD.TotalMaterialTranAmount
                                      ,IRD.TotalMaterialBooksCurrencyAmount AS TotalMaterialBaseAmount
                                      ,IRD.ShortageQty
                					  ,IRD.RejectionQty
                					  ,IRD.ApprovedQty
                                      ,IRD.TransactionQty AS PreviousQty
                                      ,IRD.ShortageRatePercent AS ShortageRate,IRD.ShortageValue,IRD.RejectRatePercent AS RejectionRate,IRD.RejectValue AS RejectionValue,IRD.RejectClamPercent RejectionClamRate,IR.CheckedBy,c.UserName CountryName,C.Id CountryId ,IRD.GRNQty-IRD.ShortageQty AS NetQty,MS.Id MaterialStorageId,IRD.GrossAmount,IRD.DiscountAmount,IRD.QualityStatus
								  FROM TRN.InventoryMaterial AS IM
                                  LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                                  LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                  LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                                  LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                                  LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                                  LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                                  LEFT jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id and ird.InventoryReceiveId='"+ inveReveiveId + @"'
                                  LEFT JOIN dbo.JWTransformationPODetail AS PID on PID.Id=IRD.JWTransformationPODetailId   
                                  LEFT JOIN (select JWTransformationPODetailId,  Sum(TransactionQty) as GRNRcvQty 
                                  from trn.InventoryReceiveDetail where InventoryReceiveId not in('" + inveReveiveId + @"') 
                                  Group By JWTransformationPODetailId) AS Pre on pre.JWTransformationPODetailId=IRD.JWTransformationPODetailId

								JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
								JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
								JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
							--	LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID=PID.RequisitionDetailId
								left join scs.country C ON C.Id=IM.CountryId 
								LEFT JOIN  [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
								Left JOIN dbo.JWTransformationPODetail mp On mp.Id=IRD.JWTransformationPODetailId 
								--left join dbo.JobWorkTransformationContract tc1 on tc1.Id=mp.OSTransformationPOId
								left join dbo.JWTransformationPO tc1 on tc1.Id=mp.JWTransformationPOId
								left join hkp.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
								left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
								WHERE IRD.InventoryReceiveId=@inventoryReceiveId and IM.MaterialMasterId IS not null AND IRD.MaterialFor='JobWorkOUTPUTMaterial'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> JWByProductQuery(string inveReveiveId)
		{
			try
			{
				var sql = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
                                  ,@totalReceiveAmount DECIMAL(18, 4)=0
                                  ,@totalServiceAmount DECIMAL(18, 4)=0
                                  ,@totalSvcTaxAmount DECIMAL(18, 4)=0
                                  SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                                  SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@inventoryReceiveId)
                                  SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
                                  SELECT IM.Id, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
										,mi.Id OSTransformationPOInputMaterialId
										,tbp.Id OSTransformationPOByProductId
										,jwit.UserName as JWOutputItem
										,jwi.UserName as ByProductItem
                                      
                                      , MGM.UserName AS MaterialGroupMasterName
                                      , IM.MaterialMasterId, MM.UserName
                                      , IM.ArticleId, ART.StandardName
                                      , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                                      , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                                      , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                                      , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                                      , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                                      , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
                                      , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
                                      , IRD.MaterialTranRate AS TransactionRate
                                      , CU.Code AS CurrencyName, IR.ToCurrencyRate
                                      , IRD.MaterialTranAmount AS TrnAmount
                                      , IRD.ToTalMaterialBooksCurrencyAmount AS BaseAmount
                                      , IRD.TotalTaxAmount AS BaseTaxAmount
                                      , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
                                      , IRD.ChargesTranAmount AS ChargesAmount	                      
                                      , ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
                                      , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
                                   
                                      , ((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) AS PlanQuantity
                                       ,ISNULL(Pre.GRNRcvQty,'0') AS GRNRcvQty                       
                                      ,IRD.GRNQty TransactionQty                         
                					  ,(((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100)-IRD.GRNQty-ISNULL(Pre.GRNRcvQty,0)) AS Balance                            
									  ,IRD.TransactionUoMId
                					  ,IRD.BaseUOMId   
                                      ,IRD.TotalMaterialTranAmount
                                      ,IRD.TotalMaterialBooksCurrencyAmount AS TotalMaterialBaseAmount
                                      ,IRD.ShortageQty
                					  ,IRD.RejectionQty
                					  ,IRD.ApprovedQty
                                      ,IRD.TransactionQty AS PreviousQty
                                      ,IRD.ShortageRatePercent AS ShortageRate,IRD.ShortageValue,IRD.RejectRatePercent AS RejectionRate,IRD.RejectValue AS RejectionValue,IRD.RejectClamPercent RejectionClamRate,IR.CheckedBy,c.UserName CountryName,C.Id CountryId ,IRD.GRNQty-IRD.ShortageQty AS NetQty,MS.Id MaterialStorageId,IRD.GrossAmount,IRD.DiscountAmount,IRD.QualityStatus
								  FROM TRN.InventoryMaterial AS IM
                                  LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                                  LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                  LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                                  LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                                  LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                                  LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                                  LEFT jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id and ird.InventoryReceiveId='" + inveReveiveId + @"'
                                  LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId   
                                  LEFT JOIN (select OSTransformationPOByProductId,  Sum(TransactionQty) as GRNRcvQty 
                                  from trn.InventoryReceiveDetail where InventoryReceiveId not in('" + inveReveiveId + @"') 
                                  Group By OSTransformationPOByProductId) AS Pre on pre.OSTransformationPOByProductId=IRD.OSTransformationPOByProductId

								JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
								JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
								JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
								LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID=PID.RequisitionDetailId
								left join scs.country C ON C.Id=IM.CountryId 
								LEFT JOIN  [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
						--		LEFT JOIN dbo.JobWorkTransformationContractChild4 tbp ON tbp.Id=IRD.OSTransformationPOByProductId  
								LEFT JOIN dbo.OSTransformationPOByProduct tbp ON tbp.Id=IRD.OSTransformationPOByProductId  
                                left join HKP.JobWorkItem jwi on jwi.Id=tbp.JobWorkItemId
                        --        left join dbo.JobWorkTransformationContractChild3 mi on mi.Id=tbp.JobWorkTransformationContractChild3MasterId
								left join dbo.OSTransformationPOInputMaterial mi on mi.Id=tbp.OSTransformationPOInputMaterialId
                        --        left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
								left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
                                left join HKP.JobWorkItem jwit on jwit.Id=mp.JobWorkItemMasterId
								WHERE IRD.InventoryReceiveId=@inventoryReceiveId and IM.MaterialMasterId IS not null AND IRD.MaterialFor='JWBYPRODUCTMaterial'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		// job Work

		public IEnumerable<object> JobWorkByProductQuery(string inveReveiveId)
		{
			try
			{
				var sql = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
                                  ,@totalReceiveAmount DECIMAL(18, 4)=0
                                  ,@totalServiceAmount DECIMAL(18, 4)=0
                                  ,@totalSvcTaxAmount DECIMAL(18, 4)=0
                                  SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                                  SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@inventoryReceiveId)
                                  SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
                                  SELECT IM.Id, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
										,mi.Id JWTransformationPOInputMaterialId
										,tbp.Id JWTransformationPOByProductId
										,jwit.UserName as JWOutputItem
										,jwi.UserName as ByProductItem
                                      
                                      , MGM.UserName AS MaterialGroupMasterName
                                      , IM.MaterialMasterId, MM.UserName
                                      , IM.ArticleId, ART.StandardName
                                      , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                                      , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                                      , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                                      , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                                      , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                                      , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
                                      , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
                                      , IRD.MaterialTranRate AS TransactionRate
                                      , CU.Code AS CurrencyName, IR.ToCurrencyRate
                                      , IRD.MaterialTranAmount AS TrnAmount
                                      , IRD.ToTalMaterialBooksCurrencyAmount AS BaseAmount
                                      , IRD.TotalTaxAmount AS BaseTaxAmount
                                      , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
                                      , IRD.ChargesTranAmount AS ChargesAmount	                      
                                      , ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
                                      , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
                                   
                                      , ((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) AS PlanQuantity
                                       ,ISNULL(Pre.GRNRcvQty,'0') AS GRNRcvQty                       
                                      ,IRD.GRNQty TransactionQty                         
                					  ,(((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100)-IRD.GRNQty-ISNULL(Pre.GRNRcvQty,0)) AS Balance                            
									  ,IRD.TransactionUoMId
                					  ,IRD.BaseUOMId   
                                      ,IRD.TotalMaterialTranAmount
                                      ,IRD.TotalMaterialBooksCurrencyAmount AS TotalMaterialBaseAmount
                                      ,IRD.ShortageQty
                					  ,IRD.RejectionQty
                					  ,IRD.ApprovedQty
                                      ,IRD.TransactionQty AS PreviousQty
                                      ,IRD.ShortageRatePercent AS ShortageRate,IRD.ShortageValue,IRD.RejectRatePercent AS RejectionRate,IRD.RejectValue AS RejectionValue,IRD.RejectClamPercent RejectionClamRate,IR.CheckedBy,c.UserName CountryName,C.Id CountryId ,IRD.GRNQty-IRD.ShortageQty AS NetQty,MS.Id MaterialStorageId,IRD.GrossAmount,IRD.DiscountAmount,IRD.QualityStatus
								  FROM TRN.InventoryMaterial AS IM
                                  LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                                  LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                  LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                                  LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                                  LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                                  LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                                  LEFT jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id and ird.InventoryReceiveId='"+ inveReveiveId + @"'
                                  LEFT JOIN dbo.JWTransformationPODetail AS PID on PID.Id=IRD.JWTransformationPODetailId   
                                  LEFT JOIN (select JWTransformationPOByProductId,  Sum(TransactionQty) as GRNRcvQty 
                                  from trn.InventoryReceiveDetail where InventoryReceiveId not in('" + inveReveiveId + @"') 
                                  Group By JWTransformationPOByProductId) AS Pre on pre.JWTransformationPOByProductId=IRD.JWTransformationPOByProductId

								JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
								JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
								JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						--		LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID=PID.RequisitionDetailId
								left join scs.country C ON C.Id=IM.CountryId 
								LEFT JOIN  [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
						--		LEFT JOIN dbo.JobWorkTransformationContractChild4 tbp ON tbp.Id=IRD.OSTransformationPOByProductId  
								LEFT JOIN dbo.JWTransformationPOByProduct tbp ON tbp.Id=IRD.JWTransformationPOByProductId  
                                left join HKP.JobWorkItem jwi on jwi.Id=tbp.JobWorkItemId
                        --        left join dbo.JobWorkTransformationContractChild3 mi on mi.Id=tbp.JobWorkTransformationContractChild3MasterId
								left join dbo.JWTransformationPOInputMaterial mi on mi.Id=tbp.JWTransformationPOInputMaterialId
                        --        left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
								left join dbo.JWTransformationPODetail mp on mp.Id=mi.JWTransformationPODetailId
                                left join HKP.JobWorkItem jwit on jwit.Id=mp.JobWorkItemMasterId
								WHERE IRD.InventoryReceiveId=@inventoryReceiveId and IM.MaterialMasterId IS not null AND IRD.MaterialFor='JobWorkBYPRODUCTMaterial'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public Dictionary<string, object> GetJWStock(InventoryMaterialViewModel entity, string issueDate)
		{
			try
			{
				var sql = @"--SELECT IM.TotalQty FROM TRN.InventoryMaterial AS IM WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "'AND IM.PlantId='" + entity.PlantId + @"' 
                            --AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + @"'
                            --AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                            --AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + @"'
                            --AND IM.Id IN(SELECT DISTINCT A.InventoryMaterialId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
                            --        WHERE A.MaterialStorageId='" + entity.MaterialStorageId + "' AND CAST(B.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE))
                    SELECT  sum(t.TotalQty) TotalQty,sum(t.PostingQty) PostingQty,sum(t.PostingQty) PostingQuantity,sum(t.ApprovedQty) ApprovedQty,sum(t.UnApprovedQty) UnApprovedQty
                      --SELECT  REPLACE(CONVERT(varchar(20), (CAST(max(t.TotalQty) AS money)), 1), '.00', '') AS TotalQty
					  --,REPLACE(CONVERT(varchar(20), (CAST(max(t.PostingQty) AS money)), 1), '.00', '') AS PostingQty
					  --,max(t.PostingQty) AS PostingQuantity
					  --,REPLACE(CONVERT(varchar(20), (CAST(max(t.ApprovedQty) AS money)), 1), '.00', '') AS ApprovedQty
					  --, REPLACE(CONVERT(varchar(20), (CAST(max(t.UnApprovedQty) AS money)), 1), '.00', '') AS UnApprovedQty 
                        from(
		              SELECT TotalQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 PostingQty, 0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
            UNION ALL
            SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status]='Posting' 
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
                    UNION ALL
                    SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status]='Posting' AND IR.IsApproved=1 AND IR.RequiredPosting=1 AND IR.GRNType='MaterialTransfer'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
                    UNION ALL
                    SELECT 0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.[Status] IS NULL AND IR.IsApproved=1 AND IR.RequiredPosting=0 AND IR.GRNType='MaterialTransfer'
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
            UNION ALL
           SELECT 0 TotalQty,0 PostingQty,ApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 UnApprovedQty
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.IsApproved=1
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
                UNION ALL
                SELECT 0 TotalQty,0 PostingQty,0 ApprovedQty, UnApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))))
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + @"' 
                    AND IR.IsApproved=0
                    AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)
			) AS t";
				return _sqlRepository.GetData(sql);
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
