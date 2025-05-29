using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.ViewModel.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.MaterialManagement.Inventory
{
    public class POMaterialService : Service<POMaterial>, IPOMaterialService
    {
        #region Constructor

        private readonly IRepositoryAsync<POMaterial> _inventoryMaterialRepository;
        private readonly ISqlRepository _sqlRepository;

        public POMaterialService(
            IRepositoryAsync<POMaterial> inventoryMaterialRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(inventoryMaterialRepository, unitOfWork, pkGeneratorService)
        {
            _inventoryMaterialRepository = inventoryMaterialRepository;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return base.GetAutoNumber(nameof(POMaterial), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public GridModel Query(GridParameter parameters, string inveReveiveId)
        {
            try
            {
                parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
	                                  , @totalReceiveAmount DECIMAL(18, 4)=0
	                                  , @totalServiceAmount DECIMAL(18, 4)=0
	                                  , @totalSvcTaxAmount DECIMAL(18, 4)=0
                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(TransactionAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
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
                            , IRD.TransactionQty, IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM, IRD.TransactionRate, CU.Code AS CurrencyName, IR.ToCurrencyRate
                            , (IRD.TransactionQty*IRD.TransactionRate) AS TrnAmount
                            , IRD.BaseAmount
                            , IRD.TotalTaxAmount AS BaseTaxAmount
	                        , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
	                        , IRD.ChargesAmount
	                        , ServiceCharge=(@totalServiceAmount/@totalReceiveAmount)*IRD.TransactionAmount
	                        , ServiceTax=(@totalSvcTaxAmount/@totalReceiveAmount)*IRD.TransactionAmount
	                        , IRD.CountryId
                        FROM TRN.POMaterial AS IM
                        JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                        JOIN [TRN].[PurchasdeOrderDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                        JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                        JOIN [TRN].[PurchasdeOrder] AS IR ON IRD.InventoryReceiveId=IR.Id
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

        public GridModel QueryForPurchaseOrderDetail(GridParameter parameters, string inveReveiveId)
        {
            try
            {
                parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
	                                  , @totalReceiveAmount DECIMAL(18, 4)=0
	                                  , @totalServiceAmount DECIMAL(18, 4)=0
	                                  , @totalSvcTaxAmount DECIMAL(18, 4)=0
                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(TransactionAmount, 0)),1) FROM [TRN].[PurchaseOrderDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[POService] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
                       SELECT IRD.Id,IRD.Id AS InventoryReceiveDetailId
                            , MGM.UserName AS MaterialGroupMasterName,Im.MaterialMasterId
                            , IRD.InventoryMaterialId, MM.UserName
                            , IRD.ArticleId, ART.StandardName
                            , IRD.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , IRD.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , IRD.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , IRD.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , IRD.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , IRD.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
                            , ROUND(IRD.TransactionQty,2) TransactionQty, IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM 
                            , TransactionRate=CASE WHEN ROUND(ISNULL(IRD.TransactionRate,''),4)=0 then null else ROUND(ISNULL(IRD.TransactionRate,''),4) end
                            , CU.Code AS CurrencyName, IR.ToCurrencyRate
                            , ROUND((IRD.TransactionQty*IRD.TransactionRate),2) AS TrnAmount
                            --, IRD.BaseAmount
                            ,BaseAmount= case when IR.IsNonCreditable=1 Then CONVERT(DECIMAL(10,2),((ROUND((IRD.TransactionQty*IRD.TransactionRate),2))+ (SELECT ROUND(SUM(TaxAmount),2) FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId=IRD.Id)) )   
										 else CONVERT(DECIMAL(12,2),IRD.BaseAmount)  END
                            --, IRD.TotalTaxAmount AS BaseTaxAmount                            
	                        , BaseTaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId=IRD.Id)
                            ,TotalAmount=(case when IR.IsNonCreditable=1 Then CONVERT(DECIMAL(10,2),((ROUND((IRD.TransactionQty*IRD.TransactionRate),2))+ (SELECT ROUND(SUM(TaxAmount),2) FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId=IRD.Id)) )   
										 else CONVERT(DECIMAL(12,2),IRD.BaseAmount)  END)+IRD.TotalTaxAmount
	                        , IRD.ChargesAmount
	                        --, ServiceCharge=(@totalServiceAmount/@totalReceiveAmount)*IRD.TransactionAmount
	                        --, ServiceTax=(@totalSvcTaxAmount/@totalReceiveAmount)*IRD.TransactionAmount
	                        , IRD.CountryId
                            ,null TaxList
                            ,IR.InvoicingPartyPlantId
                            ,AMD.StateId as InvoicingStateId
							,AMP.StateId  As PlantStateId
                            ,IR.DeliveryInstruction
                            ,IR.SpecialInstruction
                            ,IRD.Description
                            ,IRD.RefferenceNo,IRD.BaseUoMFactor
                            ,Replace(CONVERT(VARCHAR(11), IRD.DeliveryDate, 106), ' ', '-') DeliveryDate
                            ,C.UserName CountryName,C.Id CountryId,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,HN.Code HSNCode,IRD.Tolerance
                            ,ISNULL(RD.GRNTotalAmount,0) GRNAmount,ISNULL(ACPT.ACPTTotalAmount,0) ACPTAmount,PO.POType
                        FROM [TRN].[PurchaseOrderDetail] AS IRD 
						LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=IRD.InventoryReceiveId
                        left JOIN MST.MaterialMaster AS MM ON IRD.InventoryMaterialId=MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IRD.ArticleId=ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId=FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId=SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId=TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId=FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId=SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId=TCV.Id
                        LEFT JOIN Trn.InventoryMaterial AS im ON im.Id = IRD.InventoryMaterialId
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                        left JOIN [TRN].[PurchaseOrder] AS IR ON IRD.InventoryReceiveId=IR.Id
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id 
                        LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=IR.InvoicingPartyPlantId
						LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
						LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId  
						
						LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
						LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
						LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId       
						LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=IR.PlantId
						LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PT.AddressMasterId  
                       Left join scs.country C On C.Id=IRD.CountryId
                            LEFT JOIN [HKP].[HSNCode] AS HN ON MM.HSNCodeId=HN.Id
						LEFT JOIN(SELECT SUM(TotalMaterialTranAmount) GRNTotalAmount,PODetailsId FROM TRN.InventoryReceiveDetail GROUP BY PODetailsId) RD ON RD.PODetailsId=IRD.Id
						LEFT JOIN(SELECT SUM(TotalMaterialTranAmount) ACPTTotalAmount,PODetailId FROM TRN.PurchaseDocAcceptanceDetail GROUP BY PODetailId) ACPT ON ACPT.PODetailId=IRD.Id
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
        public IEnumerable<object> GetPOTaxUpdateList(string poId)
        {
            var sql = @"select * from trn.PurchaseOrderTax where InventoryReceiveId='"+ poId + @"' and InventoryServiceId is null";
            return _sqlRepository.GetDataCollection(sql);
        }
        public GridModel GetPOBOQMAPList(GridParameter parameters, string inveReveiveId)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM trn.POBOQMAP AS p WHERE p.PODetailId IN (SELECT Id FROM trn.PurchaseOrderDetail AS pod WHERE pod.InventoryReceiveId ='"+ inveReveiveId + @"')";
                return _sqlRepository.GetDifferentGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetInventoryMaterialForImprestPayable(string companyId, string plantId, string inveReveiveId)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @countryId varchar(10)
                    SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
	                    , T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
	                    , T.BudgetMasterId, T.BudgetCode, T.BudgetName
	                    , T.ActivityId, T.ActivityCode, T.ActivityName
	                    , T.Dr + (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount FROM [TRN].[InventoryReceiveTax] AS IRTS JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
	                    WHERE IRTS.InventoryReceiveId=@receiveId AND IR.IsNonCreditable=1 AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ()) AS Dr
	                    , T.Cr, T.Amount + (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount FROM [TRN].[InventoryReceiveTax] AS IRTS 
		                    JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
		                    WHERE IRTS.InventoryReceiveId=@receiveId AND IR.IsNonCreditable=1 AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ())AS Amount
                    FROM (
	                    SELECT  'Material' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId
		                    , MGGL.InventoryGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
		                    , MGGL.InventoryBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
		                    , MGGL.InventoryActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
		                    , SUM(IRD.BaseAmount) AS Dr, NULL Cr
		                    , SUM(IRD.BaseAmount) AS Amount
	                    FROM [TRN].[PurchasdeOrderDetail] AS IRD
	                    JOIN [TRN].[PurchasdeOrder] AS IR ON IRD.InventoryReceiveId=IR.Id
	                    JOIN [TRN].[POMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
	                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
	                    JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
			                    AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
	                    LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
	                    LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
	                    LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
	                    LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
	                    WHERE IRD.InventoryReceiveId=@receiveId
	                    GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
                    ) AS T
                    GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId
                    UNION
                    SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, IRT.TaxCategoryId
	                    , TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
	                    , TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
	                    , TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
	                    , SUM(IRT.TaxAmount) AS  Dr, NULL Cr
	                    , SUM(IRT.TaxAmount) AS Amount
                    FROM [TRN].[InventoryReceiveTax] AS IRT
                    JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRT.InventoryReceiveDetailId=IRD.Id
                    JOIN [TRN].[POMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
                    JOIN (SELECT C.AddressMasterId, MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
		                    AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
                    JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
                    JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
                    LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
                    LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                    LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
                    WHERE IRT.InventoryReceiveId=@receiveId AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM'
                    GROUP BY MM.MaterialGroupMasterId, IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
                    UNION
                    SELECT 'Svc' AS OtherName, 'Dr' AS TrnType, NULL AS MaterialGroupMasterId, IRTS.TaxCategoryId
	                    , TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
	                    , TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
	                    , TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
	                    , SUM(IRTS.TaxAmount) AS  Dr, NULL Cr
	                    , SUM(IRTS.TaxAmount) AS Amount
	                    --, IRTS.TaxAmount
	                    --, IRTS.TaxAmount
                    FROM [TRN].[InventoryReceiveTax] AS IRTS
                    JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
                    JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRTS.TaxCategoryId
                    JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id AND IRTS.TaxCategoryId=TC.Id
                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
                    LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
                    LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                    LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
                    WHERE IRTS.InventoryReceiveId=@receiveId AND IR.IsNonCreditable=0 AND IRTS.InventoryServiceId<>'' AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM'
                    GROUP BY IRTS.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
                    UNION
                    SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
	                    , T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
	                    , T.BudgetMasterId, T.BudgetCode, T.BudgetName
	                    , T.ActivityId, T.ActivityCode, T.ActivityName
	                    , T.Dr, T.Cr + (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount FROM [TRN].[InventoryReceiveTax] AS IRTS JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
		                    WHERE IRTS.InventoryReceiveId=@receiveId AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ()) AS Cr
	                    , T.Amount + (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount FROM [TRN].[InventoryReceiveTax] AS IRTS JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
		                    WHERE IRTS.InventoryReceiveId=@receiveId AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ())AS Amount
                    FROM (
	                    SELECT 'Vendor' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId
		                    , NULL AS GLGeneralInfoId, NULL AS GLGeneralInfoCode, NULL AS GLGeneralInfoName
		                    , NULL AS BudgetMasterId, NULL AS BudgetCode, NULL AS BudgetName
		                    , NULL ActivityId, NULL AS ActivityCode, NULL AS ActivityName
		                    , NULL Dr, SUM(IRD.BaseAmount) + SUM(IRD.TotalTaxAmount) AS  Cr
		                    , SUM(IRD.BaseAmount) + SUM(IRD.TotalTaxAmount) AS Amount
	                    FROM [TRN].[POMaterial] AS IM
	                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
	                    LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
	                    JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
	                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
	                    WHERE IRD.InventoryReceiveId=@receiveId
	                    GROUP BY MM.MaterialGroupMasterId
                    ) AS T
                    GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetInventoryMaterialWithoutReversChargePayable(string companyId, string plantId, string inveReveiveId)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr + (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount FROM [TRN].[InventoryReceiveTax] AS IRTS JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
						WHERE IRTS.InventoryReceiveId=@receiveId AND IR.IsNonCreditable=1 AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ()) AS Dr
						, T.Cr, T.Amount + (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount FROM [TRN].[InventoryReceiveTax] AS IRTS 
							JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
							WHERE IRTS.InventoryReceiveId=@receiveId AND IR.IsNonCreditable=1 AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ())AS Amount
					FROM (
						SELECT  'Material' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId
							, MGGL.InventoryGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
							, MGGL.InventoryBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
							, MGGL.InventoryActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
							, SUM(IRD.TransactionAmount) AS Dr, NULL Cr
							, SUM(IRD.TransactionAmount) AS Amount
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						JOIN [TRN].[POMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
					) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId
					
                    UNION
					SELECT  'Charge' AS OtherName, 'Dr' AS TrnType, MM.ServiceMasterId, NULL AS TaxCategoryId
							, MGGL.ExpenseGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
							, MGGL.ExpenseBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
							, MGGL.ExpenseActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
							, SUM(IRD.ChargesAmount) AS Dr, NULL Cr
							, SUM(IRD.ChargesAmount) AS Amount
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						JOIN [TRN].[InventoryService] AS IM ON IRD.InventoryReceiveId=IM.InventoryReceiveId
						JOIN [HKP].[CompanyServiceMaster] AS MM ON IM.ServiceMasterId=MM.ServiceMasterId
						JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[ServiceGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.ServiceMasterId = MGGL.ServiceGroupId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.ExpenseGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.ExpenseBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.ExpenseActivityId= A.Id
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY MM.ServiceMasterId, MGGL.ExpenseGLId, GL.AccountCode, GL.UserName, MGGL.ExpenseBudgetMasterId, B.Code, B.UserName, MGGL.ExpenseActivityId, A.Code, A.UserName
                        
                    UNION
					SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, IRT.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRT.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRT.TaxAmount) AS Amount
					FROM [TRN].[InventoryReceiveTax] AS IRT
					JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRT.InventoryReceiveDetailId=IRD.Id
					JOIN [TRN].[POMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
					JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
					JOIN (SELECT C.AddressMasterId, MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
							AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
					JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
					JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE IRT.InventoryReceiveId=@receiveId AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM'
					GROUP BY MM.MaterialGroupMasterId, IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					UNION
					SELECT 'Svc' AS OtherName, 'Dr' AS TrnType, NULL AS MaterialGroupMasterId, IRTS.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRTS.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRTS.TaxAmount) AS Amount
						--, IRTS.TaxAmount
						--, IRTS.TaxAmount
					FROM [TRN].[PurchasdeOrderTax] AS IRTS
					JOIN [TRN].[PurchasdeOrder] AS IR ON IRTS.InventoryReceiveId=IR.Id
					JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRTS.TaxCategoryId
					JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id AND IRTS.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE IRTS.InventoryReceiveId=@receiveId AND IR.IsNonCreditable=0 AND IRTS.InventoryServiceId<>'' AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM'
					GROUP BY IRTS.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					UNION
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr + (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount FROM [TRN].[InventoryReceiveTax] AS IRTS JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
							WHERE IRTS.InventoryReceiveId=@receiveId AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ()) AS Cr
						, T.Amount + (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount FROM [TRN].[InventoryReceiveTax] AS IRTS JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
							WHERE IRTS.InventoryReceiveId=@receiveId AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ())AS Amount
					FROM (
						SELECT 'Vendor' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
							, MGPGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
							, MGPGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
							, MGPGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
							, NULL Dr, SUM(IRD.TransactionAmount) + SUM(IRD.TotalTaxAmount)+ SUM(IRD.ChargesAmount) AS  Cr
							, SUM(IRD.TransactionAmount) + SUM(IRD.TotalTaxAmount)+ SUM(IRD.ChargesAmount) AS Amount
						FROM [TRN].[POMaterial] AS IM
						JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [TRN].[PurchasdeOrderDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
						JOIN [TRN].[PurchasdeOrder] AS IR ON IRD.InventoryReceiveId=IR.Id
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Vendor')AS CP ON IR.PartyId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY  MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName
					) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId
					--UNION
					--SELECT 'Svc' AS OtherName, 'Cr' AS TrnType, NULL AS MaterialGroupMasterId, IRTS.TaxCategoryId
					--	, TCGL.LiabilityGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
					--	, TCGL.LiabilityBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
					--	, TCGL.LiabilityActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
					--	, NULL AS  Dr, SUM(IRTS.TaxAmount) Cr
					--	, SUM(IRTS.TaxAmount) AS Amount
					--FROM [TRN].[PurchasdeOrderTax] AS IRTS
					--JOIN [TRN].[PurchasdeOrder] AS IR ON IRTS.InventoryReceiveId=IR.Id
					--JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRTS.TaxCategoryId
					--JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id AND IRTS.TaxCategoryId=TC.Id
					--LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					--LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					--LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					--LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					--WHERE IRTS.InventoryReceiveId=@receiveId AND IR.IsNonCreditable=0 AND IRTS.InventoryServiceId<>'' AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM'
					--GROUP BY IRTS.TaxCategoryId, TCGL.LiabilityGLId, GL.AccountCode, GL.UserName, TCGL.LiabilityBudgetMasterId, B.Code, B.UserName, TCGL.LiabilityActivityId, A.Code, A.UserName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetInventoryMaterialReversChargePayable(string companyId, string plantId, string inveReveiveId)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @countryId varchar(10)
                    SET @countryId =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)
                    SELECT  'Material' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId
	                        , MGGL.InventoryGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
	                        , MGGL.InventoryBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
                            , MGGL.InventoryActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
	                        , SUM(IRD.BaseAmount) AS Dr, NULL Cr
		                    , SUM(IRD.BaseAmount) AS Amount
                    FROM [TRN].[PurchasdeOrderDetail] AS IRD
                    JOIN [TRN].[PurchasdeOrder] AS IR ON IRD.InventoryReceiveId=IR.Id
                    JOIN [TRN].[POMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
                    JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
		                    AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
                    LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
                    LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
                    LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                    LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
                    WHERE IRD.InventoryReceiveId=@receiveId
                    GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
                    UNION
                    SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, IRT.TaxCategoryId
	                        , TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
	                        , TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
                            , TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
	                        , SUM(IRT.TaxAmount) AS  Dr, NULL Cr
		                    , SUM(IRT.TaxAmount) AS Amount
                    FROM [TRN].[PurchasdeOrderTax] AS IRT
                    JOIN [TRN].[PurchasdeOrderDetail] AS IRD ON IRT.InventoryReceiveDetailId=IRD.Id
                    JOIN [TRN].[POMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
                    JOIN (SELECT C.AddressMasterId, MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
		                    AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
                    JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
                    JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
                    LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
                    LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                    LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
                    WHERE IRT.InventoryReceiveId=@receiveId AND TCGL.InputTaxOutPutTax='Input' AND TCGL.TaxType='RCM'
                    GROUP BY MM.MaterialGroupMasterId, IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
                    UNION
					SELECT 'Svc' AS OtherName, 'Dr' AS TrnType, NULL AS MaterialGroupMasterId, IRTS.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRTS.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRTS.TaxAmount) AS Amount
						--, IRTS.TaxAmount
						--, IRTS.TaxAmount
					FROM [TRN].[PurchasdeOrderTax] AS IRTS
					JOIN [TRN].[PurchasdeOrder] AS IR ON IRTS.InventoryReceiveId=IR.Id
					JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRTS.TaxCategoryId
					JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id AND IRTS.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE IRTS.InventoryReceiveId=@receiveId AND IR.IsNonCreditable=1 AND IRTS.InventoryServiceId<>'' AND TCGL.InputTaxOutPutTax='Input' AND TCGL.TaxType='RCM'
					GROUP BY IRTS.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName                        
					UNION
                    SELECT 'Vendor' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId
	                    , MGPGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
	                    , MGPGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
	                    , MGPGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                        , NULL Dr, SUM(IRD.BaseAmount) AS  Cr
	                    , SUM(IRD.BaseAmount) AS Amount
                    FROM [TRN].[POMaterial] AS IM
                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
                    LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
                    JOIN [TRN].[PurchasdeOrderDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    JOIN [TRN].[PurchasdeOrder] AS IR ON IRD.InventoryReceiveId=IR.Id
                    JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                    JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
                            AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
                    LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Vendor')AS CP ON IR.PartyId = CP.PartyId
                    LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
                    LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id
                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
                    LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
                    LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
                    LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id
                    WHERE IRD.InventoryReceiveId=@receiveId
                    GROUP BY MM.MaterialGroupMasterId, MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName
                    UNION
                    SELECT 'Tax' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, IRT.TaxCategoryId
	                        , TCGL.LiabilityGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
	                        , TCGL.LiabilityBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
                            , TCGL.LiabilityActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
	                        , NULL AS  Dr, SUM(IRT.TaxAmount) Cr
		                    , SUM(IRT.TaxAmount) AS Amount
                    FROM [TRN].[PurchasdeOrderTax] AS IRT
                    JOIN [TRN].[PurchasdeOrderDetail] AS IRD ON IRT.InventoryReceiveDetailId=IRD.Id
                    JOIN [TRN].[PurchasdeOrder] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
                    JOIN (SELECT C.AddressMasterId, MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
		                    AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
                    JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
                    JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.LiabilityGLId=GL.Id
                    LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.LiabilityBudgetMasterId= BM.Id
                    LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                    LEFT JOIN [HKP].[Activity] AS A ON TCGL.LiabilityActivityId= A.Id
                    WHERE IRT.InventoryReceiveId=@receiveId AND TCGL.InputTaxOutPutTax='Input' AND TCGL.TaxType='RCM'
                    GROUP BY MM.MaterialGroupMasterId, IRT.TaxCategoryId, TCGL.LiabilityGLId, GL.AccountCode, GL.UserName, TCGL.LiabilityBudgetMasterId, B.Code, B.UserName, TCGL.LiabilityActivityId, A.Code, A.UserName
					UNION
					SELECT 'Svc' AS OtherName, 'Cr' AS TrnType, NULL AS MaterialGroupMasterId, IRTS.TaxCategoryId
						, TCGL.LiabilityGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.LiabilityBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.LiabilityActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, NULL AS Dr, SUM(IRTS.TaxAmount) AS Cr
						, SUM(IRTS.TaxAmount) AS Amount
					FROM [TRN].[PurchasdeOrderTax] AS IRTS
					JOIN [TRN].[PurchasdeOrder] AS IR ON IRTS.InventoryReceiveId=IR.Id
					JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRTS.TaxCategoryId
					JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id AND IRTS.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE IRTS.InventoryReceiveId=@receiveId AND IR.IsNonCreditable=1 AND IRTS.InventoryServiceId<>'' AND TCGL.InputTaxOutPutTax='Input' AND TCGL.TaxType='RCM'
					GROUP BY IRTS.TaxCategoryId, TCGL.LiabilityGLId, GL.AccountCode, GL.UserName, TCGL.LiabilityBudgetMasterId, B.Code, B.UserName, TCGL.LiabilityActivityId, A.Code, A.UserName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public decimal GetStock(InventoryMaterialViewModel entity, string issueDate)
        {
            try
            {
                var sql = @"--SELECT IM.TotalQty FROM TRN.POMaterial AS IM WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "'AND IM.PlantId='" + entity.PlantId + @"' 
                            --AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"' AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + @"'
                            --AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                            --AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + @"'
                            --AND IM.Id IN(SELECT DISTINCT A.InventoryMaterialId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
                            --        WHERE A.MaterialStorageId='" + entity.MaterialStorageId + "' AND CAST(B.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE))

                    SELECT TotalQty=SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId  THEN ISNULL(IRD.IssueQty, 0)
					                    WHEN IRD.IssueQty IS NULL THEN 0 ELSE IRD.IssueQty END,0))
                    FROM [TRN].[PurchasdeOrderDetail] AS IRD
                    JOIN [TRN].[POMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[PurchasdeOrder] AS IR ON IRD.InventoryReceiveId=IR.Id
                    JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + "' AND IR.[Status]='Posting' AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + "' AND ISNULL(IRD.IssueQty, 1)>0 AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE)";
                return _inventoryMaterialRepository.SqlQuery<decimal>(sql).FirstOrDefault();
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
                var sql = @"SELECT IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
                        , IRD.TransactionQty, StockQty=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN (BaseQty-ISNULL(IRD.IssueQty, 0))/BaseUoMFactor ELSE IRD.TransactionQty-ISNULL(IRD.IssueQty, 0) END
	                    , TUoM.UserName AS TUoM, IRD.TransactionUoMId, IRD.BaseQty, IRD.BaseUOMId, IRD.BaseUoMFactor
                        , IRD.TransactionRate, IRD.WithInvoiceRate, TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.BaseAmount
	                    , IssueQty=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId  THEN ISNULL(IRD.IssueQty, 0)/BaseUoMFactor
					                    WHEN IRD.IssueQty IS NULL THEN 0 ELSE IRD.IssueQty END
                        , BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.BaseAmount/IRD.BaseQty ELSE IRD.WithInvoiceRate END
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                    FROM [TRN].[PurchasdeOrderDetail] AS IRD
                    JOIN [TRN].[POMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [TRN].[PurchasdeOrder] AS IR ON IRD.InventoryReceiveId=IR.Id
                    JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    WHERE IM.CompanyGroupId='" + entity.CompanyGroupId + "' AND IM.CompanyId='" + entity.CompanyId + "' AND IM.PlantId='" + entity.PlantId + "' AND IR.[Status]='Posting' AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                    AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + "' AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + "' AND  ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                    AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + "' AND IRD.MaterialStorageId='" + entity.MaterialStorageId + "' AND ISNULL(IRD.IssueQty, 1)>0 AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + "' AS DATE) ORDER BY CAST(IRD.AddedDate AS DATE)";
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
                            , IRD.TransactionQty, StockQty=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN (BaseQty-ISNULL(IRD.IssueQty, 0))/BaseUoMFactor ELSE IRD.TransactionQty-ISNULL(IRD.IssueQty, 0) END
                            , TUoM.UserName AS TUoM, IRD.TransactionUoMId, IRD.BaseQty, IRD.BaseUOMId, IRD.BaseUoMFactor
                            , IRD.TransactionRate, IRD.WithInvoiceRate, TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.BaseAmount
                            , IssueQty=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId  THEN ISNULL(IRD.IssueQty, 0)/BaseUoMFactor
				                            WHEN IRD.IssueQty IS NULL THEN 0 ELSE IRD.IssueQty END
                            , BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.BaseAmount/IRD.BaseQty ELSE IRD.WithInvoiceRate END
                            , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, IIH.Qty AS RequisitionQty
                        FROM [TRN].[InventoryIssueHistory] AS IIH 
                        JOIN [TRN].[PurchasdeOrderDetail] AS IRD ON IIH.InventoryReceiveDetailId=IRD.Id
                        JOIN [TRN].[POMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                        JOIN [TRN].[PurchasdeOrder] AS IR ON IRD.InventoryReceiveId=IR.Id
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
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (string.IsNullOrEmpty(entity.InventoryMaterialId))
                {
                    entity.InventoryMaterialId = GetPK();
                    var material = ValueAssignInventoryMaterial(entity);
                    material.CompanyGroupId = identity.CompanyGroupId;
                    material.CompanyId = identity.CompanyId;
                    material.PlantId = identity.PlantId;
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
                var totalQty = _inventoryMaterialRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(BaseQty),0) FROM [TRN].[PurchaseOrderDetail] WHERE InventoryMaterialId='" + id + "' AND Id NOT IN ('" + receiveDetailId + "')").First();
                var avgRate = _inventoryMaterialRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(TransactionAmount)/SUM(BaseQty),0) FROM [TRN].[PurchaseOrderDetail] WHERE InventoryMaterialId='" + id + "' AND Id NOT IN ('" + receiveDetailId + "')").First();
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

        private static POMaterial ValueAssignInventoryMaterial(InventoryMaterialViewModel entity)
        {
            return new POMaterial
            {
                Id = entity.InventoryMaterialId,
                //CountryId = entity.CountryId,
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
            };
        }

        public POMaterial GetInventoryMaterialByUpToSku(InventoryMaterialViewModel entity)
        {
            return Query(t => t.MaterialMasterId == entity.MaterialMasterId && t.ArticleId == entity.ArticleId
                                && t.FirstCharacteristicsId == entity.FirstCharacteristicsId && t.FirstCharacteristicsValueId == entity.FirstCharacteristicsValueId
                                && t.SecondCharacteristicsId == entity.SecondCharacteristicsId && t.SecondCharacteristicsValueId == entity.SecondCharacteristicsValueId
                                && t.ThirdCharacteristicsId == entity.ThirdCharacteristicsId && t.ThirdCharacteristicsValueId == entity.ThirdCharacteristicsValueId
                                && t.CompanyId == entity.CompanyId && t.PlantId == entity.PlantId).Select().FirstOrDefault();
        }

       


        public IEnumerable<POMaterial> GetInventoryMaterialListByUpToSku(IEnumerable<InventoryMaterialViewModel> entities, string companyId, string plantId)
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



        #region POBy Requisition




        public GridModel GetInventoryMaterialListPoByReq(GridParameter parameters, string inveReveiveId)
        {
            try
            {
                //parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
                //                            , @totalReceiveAmount DECIMAL(18, 4)=0
                //                            , @totalServiceAmount DECIMAL(18, 4)=0
                //                            , @totalSvcTaxAmount DECIMAL(18, 4)=0
                //                 SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(TransactionAmount, 0)),1) FROM [TRN].[PurchaseOrderDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                //                 SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[POService] WHERE InventoryReceiveId=@inventoryReceiveId)
                //                 SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
                //                 SELECT POmasterId,MaterialGroupMasterName,InventoryReceiveDetailId
                //                  ,UserName
                //                  ,InventoryReceiveId
                //                  ,MaterialMasterId
                //                  ,StandardName
                //                  ,FirstCharacteristics
                //                  ,FirstCharacteristicsValue
                //                  ,SecondCharacteristics
                //                  ,SecondCharacteristicsValue
                //                  ,ThirdCharacteristics
                //                  ,ThirdCharacteristicsValue
                //                  ,sum(TransactionQty) AS TransactionQty
                //                  ,TransactionUoM                            
                //                  ,TransactionRate
                //                  ,CurrencyName
                //                  ,ToCurrencyRate
                //                  ,TrnAmount = (sum(TransactionQty) * TransactionRate)
                //                  ,count(RequisitionId) AS TotalReq
                //                     ,Sum(TaxAmount) BaseTaxAmount
                //                     ,sum(BaseAmount) BaseAmount
                //                     ,sum(ReqTransactionQty) ReqTransactionQty
                //                     ,sum(ServiceCharge) ServiceCharge
                //,Sum(ServiceTax) ServiceTax
                //                 FROM (
                //                  SELECT  IR.Id AS POmasterId,IM.InventoryReceiveId
                //                   ,IM.Id AS InventoryReceiveDetailId
                //                   ,MGM.UserName AS MaterialGroupMasterName
                //                   ,IM.InventoryMaterialId AS MaterialMasterId
                //                   ,MM.UserName
                //                   ,IM.ArticleId
                //                   ,ART.StandardName
                //                   ,IM.FirstCharacteristicsId
                //                   ,FC.UserName AS FirstCharacteristics
                //                   ,IM.FirstCharacteristicsValueId
                //                   ,FCV.UserName AS FirstCharacteristicsValue
                //                   ,IM.SecondCharacteristicsId
                //                   ,SC.UserName AS SecondCharacteristics
                //                   ,IM.SecondCharacteristicsValueId
                //                   ,SCV.UserName AS SecondCharacteristicsValue
                //                   ,IM.ThirdCharacteristicsId
                //                   ,TC.UserName AS ThirdCharacteristics
                //                   ,IM.ThirdCharacteristicsValueId
                //                   ,TCV.UserName AS ThirdCharacteristicsValue
                //                   ,ROUND(IM.TransactionQty, 2) TransactionQty
                //                   ,IM.TransactionUoMId
                //                   ,TUoM.UserName AS TransactionUoM
                //                   ,ROUND(IM.TransactionRate, 2) TransactionRate
                //                   ,CU.Code AS CurrencyName
                //                   ,IR.ToCurrencyRate
                //                   ,ROUND((IM.TransactionQty * IM.TransactionRate), 2) AS TrnAmount
                //                         ,BaseAmount
                //                   --,BaseAmount = CASE 
                //                   -- WHEN IR.IsNonCreditable = 1
                //                   --  THEN (
                //                   --    (ROUND((IM.TransactionQty * IM.TransactionRate), 2)) + (
                //                   --     SELECT SUM(TaxAmount)
                //                   --     FROM [TRN].[PurchaseOrderTax]
                //                   --     WHERE InventoryReceiveDetailId = IM.Id
                //                   --     )
                //                   --    )
                //                   -- ELSE IM.BaseAmount
                //                   -- E
                //                   ,BaseTaxAmount = (
                //                    SELECT SUM(TaxAmount)
                //                    FROM [TRN].[PurchaseOrderTax]
                //                    WHERE InventoryReceiveDetailId = IM.Id
                //                    )
                //                   ,IM.ChargesAmount
                //                    ,ServiceCharge = (@totalServiceAmount / ISNULL(NULLIF(@totalReceiveAmount,0), 1) ) * IM.TransactionAmount
                //                   ,ServiceTax = (@totalSvcTaxAmount / ISNULL(NULLIF(@totalReceiveAmount,0), 1) ) * IM.TransactionAmount
                //                   ,IM.CountryId
                //                   ,NULL TaxList
                //                   ,IR.InvoicingPartyPlantId
                //                   ,AMD.StateId AS InvoicingStateId
                //                   ,AMP.StateId AS PlantStateId
                //                   ,IR.DeliveryInstruction
                //                   ,IR.SpecialInstruction
                //                   ,IM.Description
                //                   ,Replace(CONVERT(VARCHAR(11), IM.DeliveryDate, 106), ' ', '-') DeliveryDate
                //                   ,Im.RequisitionId
                //                   ,IM.RequisitionDetailId
                //                         ,TaxAmount
                //                         ,ReqTransactionQty
                //                  FROM [TRN].[PurchaseOrderDetail] AS IM
                //                  JOIN MST.MaterialMaster AS MM ON IM.InventoryMaterialId = MM.Id
                //                  LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                //                  LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
                //                  LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
                //                  LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
                //                  LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
                //                  LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
                //                  LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
                //                  LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
                //                  JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId = TUoM.Id
                //                  JOIN [TRN].[PurchaseOrder] AS IR ON IM.InventoryReceiveId = IR.Id
                //                  JOIN [SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
                //                  LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id = IR.InvoicingPartyPlantId
                //                  LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id = PPI.AddressMasterId
                //                  LEFT JOIN [SCS].[State] AS ST ON ST.Id = AM.StateId
                //                  LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id = IR.DeliveryPartyPlantId
                //                  LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id = PPD.AddressMasterId
                //                  LEFT JOIN [SCS].[State] AS STD ON STD.Id = AMD.StateId
                //                  LEFT JOIN [ORG].[Plant] AS PT ON PT.Id = IR.PlantId
                //                  LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id = PT.AddressMasterId
                //                     Left join (select InventoryReceiveId,sum(TaxAmount) TaxAmount from TRN.purchaseOrderTax where InventoryReceiveId=@inventoryReceiveId and InventoryServiceId is null group by InventoryReceiveId) aaa On aaa.InventoryReceiveId=IM.InventoryReceiveId
                //                     LEFT JOIN (select MRD.Id, Sum(MRD.TransactionQty) ReqTransactionQty from TRN.MaterialRequsitionDetails MRD group By MRD.Id) ccc On ccc.Id=IM.RequisitionDetailId
                //                  WHERE IM.InventoryReceiveId = @inventoryReceiveId 
                //                  ) xyz
                //                   WHERE InventoryReceiveId=@inventoryReceiveId
                //                     GROUP BY POmasterId,MaterialGroupMasterName
                //                      ,UserName
                //                      ,InventoryReceiveId
                //                      ,MaterialMasterId
                //                      ,StandardName
                //                      ,FirstCharacteristics
                //                      ,FirstCharacteristicsValue
                //                      ,SecondCharacteristics
                //                      ,SecondCharacteristicsValue
                //                      ,ThirdCharacteristics
                //                      ,ThirdCharacteristicsValue
                //                      ,TransactionUoM	                            
                //                      ,CurrencyName
                //                      ,ToCurrencyRate
                //	,TransactionRate,InventoryReceiveDetailId";

                parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
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
                         ,ROUND(IM.TransactionQty, 2) TransactionQty
                         ,IM.TransactionUoMId
                         ,TUoM.UserName AS TransactionUoM
                         ,TransactionRate= CASE WHEN ROUND(IM.TransactionRate, 4)=0 THEN null ELSE ROUND(IM.TransactionRate, 4) END
                         ,CU.Code AS CurrencyName
                         ,IR.ToCurrencyRate
                         ,ROUND((IM.TransactionQty * IM.TransactionRate), 2) AS TrnAmount
                            --,BaseAmount
                         ,BaseAmount = CASE 
                           WHEN IR.IsNonCreditable = 1
                            THEN (
                               (ROUND((IM.TransactionQty * IM.TransactionRate), 2)) + (
                                  SELECT SUM(TaxAmount)
                                  FROM [TRN].[PurchaseOrderTax]
                                  WHERE InventoryReceiveDetailId = IM.Id
                                  )
                                )
                           ELSE IM.BaseAmount
                            END
                         ,BaseTaxAmount = (
                          SELECT SUM(TaxAmount)
                          FROM [TRN].[PurchaseOrderTax]
                          WHERE InventoryReceiveDetailId = IM.Id
                          )
                         ,IM.ChargesAmount
                          ,ServiceCharge = (@totalServiceAmount / ISNULL(NULLIF(@totalReceiveAmount,0), 1) ) * IM.TransactionAmount
                         ,ServiceTax = (@totalSvcTaxAmount / ISNULL(NULLIF(@totalReceiveAmount,0), 1) ) * IM.TransactionAmount
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
                         ,IM.RequisitionDetailId
                            ,TaxAmount
                            ,ReqTransactionQty
                            ,MM.HSNCodeId
                            ,ddd.RequisitionDetailId AS TotalReq
							,ccc.MaterialDetail
                        ,Tolerance= CASE WHEN ISNULL(IM.Tolerance,0)=0 THEN null ELSE ISNULL(IM.Tolerance,0) END
                        FROM [TRN].[PurchaseOrderDetail] AS IM
                        JOIN MST.MaterialMaster AS MM ON IM.InventoryMaterialId = MM.Id
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
                        LEFT JOIN (select MRD.Id,MRD.MaterialDetail, Sum(MRD.TransactionQty) ReqTransactionQty from TRN.MaterialRequsitionDetails MRD group By MRD.Id,MRD.MaterialDetail) ccc On ccc.Id=IM.RequisitionDetailId
                        Left JOIn (select pod.PoDetailId ,count(pod.RequisitionDetailId) AS  RequisitionDetailId from TRN.PoRequisitionDetail  pod  group by PoDetailId) ddd ON  ddd.PoDetailId=IM.Id
                        WHERE IM.InventoryReceiveId = @inventoryReceiveId And IM.InventoryMaterialId is not null UNION ALL SELECT  IR.Id AS POmasterId,IM.InventoryReceiveId ,IM.Id AS InventoryReceiveDetailId    ,''  MaterialGroupMasterName ,'' MaterialMasterId    ,'' UserName    ,'' ArticleId    ,'' StandardName    ,'' FirstCharacteristicsId    ,'' FirstCharacteristics    ,'' FirstCharacteristicsValueId    ,'' FirstCharacteristicsValue    ,'' SecondCharacteristicsId    ,'' SecondCharacteristics    ,'' SecondCharacteristicsValueId    ,'' SecondCharacteristicsValue    ,'' ThirdCharacteristicsId    ,'' ThirdCharacteristics    ,'' ThirdCharacteristicsValueId    ,'' ThirdCharacteristicsValue    ,ROUND(IM.TransactionQty, 2) TransactionQty    ,IM.TransactionUoMId    ,TUoM.UserName AS TransactionUoM    ,ROUND(IM.TransactionRate, 2) TransactionRate    ,CU.Code AS CurrencyName    ,IR.ToCurrencyRate    ,ROUND((IM.TransactionQty * IM.TransactionRate), 2) AS TrnAmount  ,BaseAmount = CASE WHEN IR.IsNonCreditable = 1 THEN ((ROUND((IM.TransactionQty * IM.TransactionRate), 2)) + ( SELECT SUM(TaxAmount)   FROM [TRN].[PurchaseOrderTax]  WHERE InventoryReceiveDetailId = IM.Id ))    ELSE IM.BaseAmount    END    ,BaseTaxAmount = (    SELECT SUM(TaxAmount)    FROM [TRN].[PurchaseOrderTax]    WHERE InventoryReceiveDetailId = IM.Id    )    ,IM.ChargesAmount    ,ServiceCharge = (@totalServiceAmount / ISNULL(NULLIF(@totalReceiveAmount,0), 1) ) * IM.TransactionAmount    ,ServiceTax = (@totalSvcTaxAmount / ISNULL(NULLIF(@totalReceiveAmount,0), 1) ) * IM.TransactionAmount    ,IM.CountryId    ,NULL TaxList    ,IR.InvoicingPartyPlantId    ,AMD.StateId AS InvoicingStateId    ,AMP.StateId AS PlantStateId    ,IR.DeliveryInstruction    ,IR.SpecialInstruction    ,IM.Description    ,Replace(CONVERT(VARCHAR(11), IM.DeliveryDate, 106), ' ', '-') DeliveryDate    ,Im.RequisitionId    ,IM.RequisitionDetailId    ,TaxAmount   ,ReqTransactionQty    ,MM.HSNCodeId   ,ddd.RequisitionDetailId AS TotalReq,ccc.MaterialDetail,ISNULL(IM.Tolerance,0) Tolerance FROM [TRN].[PurchaseOrderDetail] AS IM left JOIN MST.MaterialMaster AS MM ON IM.InventoryMaterialId = MM.Id LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId = TUoM.Id left JOIN [TRN].[PurchaseOrder] AS IR ON IM.InventoryReceiveId = IR.Id left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id = IR.InvoicingPartyPlantId LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id = PPI.AddressMasterId LEFT JOIN [SCS].[State] AS ST ON ST.Id = AM.StateId LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id = IR.DeliveryPartyPlantId LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id = PPD.AddressMasterId LEFT JOIN [SCS].[State] AS STD ON STD.Id = AMD.StateId LEFT JOIN [ORG].[Plant] AS PT ON PT.Id = IR.PlantId LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id = PT.AddressMasterId Left join (select InventoryReceiveId,sum(TaxAmount) TaxAmount from TRN.purchaseOrderTax where InventoryReceiveId=@inventoryReceiveId and InventoryServiceId is null group by InventoryReceiveId) aaa On aaa.InventoryReceiveId=IM.InventoryReceiveId LEFT JOIN (select MRD.Id,MRD.MaterialDetail, Sum(MRD.TransactionQty) ReqTransactionQty from TRN.MaterialRequsitionDetails MRD group By MRD.Id,MRD.MaterialDetail) ccc On ccc.Id=IM.RequisitionDetailId Left JOIN (select pod.PoDetailId ,count(pod.RequisitionDetailId) AS  RequisitionDetailId from TRN.PoRequisitionDetail  pod  group by PoDetailId) ddd ON  ddd.PoDetailId=IM.Id WHERE IM.InventoryReceiveId = @inventoryReceiveId  And IM.InventoryMaterialId is null";
                return _sqlRepository.GetDifferentGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        } 

        public IEnumerable<object> GetInventoryMaterialListPoByReqDetail(string inveReveiveId)
        {
            try
            {
                var _sql = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
	                                  , @totalReceiveAmount DECIMAL(18, 4)=0
	                                  , @totalServiceAmount DECIMAL(18, 4)=0
	                                  , @totalSvcTaxAmount DECIMAL(18, 4)=0
                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(TransactionAmount, 0)),1) FROM [TRN].[PurchaseOrderDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[POService] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
                       SELECT  IR.Id AS POmasterId,IM.InventoryReceiveId
		                        ,IM.Id AS InventoryReceiveDetailId
		                        ,MGM.UserName AS MaterialGroupName
		                        ,IM.InventoryMaterialId AS MaterialMasterId
		                        ,MM.UserName MaterialName
		                        ,IM.ArticleId
		                        ,ART.StandardName AS Article
		                        ,IM.FirstCharacteristicsId
		                        ,FC.UserName AS FirstCharacteristics
		                        ,IM.FirstCharacteristicsValueId
		                        ,FCV.UserName AS Sku1
		                        ,IM.SecondCharacteristicsId
		                        ,SC.UserName AS SecondCharacteristics
		                        ,IM.SecondCharacteristicsValueId
		                        ,SCV.UserName AS Sku2
		                        ,IM.ThirdCharacteristicsId
		                        ,TC.UserName AS ThirdCharacteristics
		                        ,IM.ThirdCharacteristicsValueId
		                        ,TCV.UserName AS Sku3
		                        ,ROUND(IM.TransactionQty, 2) TransactionQty
		                        ,IM.TransactionUoMId
		                        ,TUoM.UserName AS TransactionUoM
		                        ,ROUND(IM.TransactionRate, 4) TransactionRate
		                        ,CU.Code AS CurrencyName
		                        ,IR.ToCurrencyRate
		                        ,ROUND((IM.TransactionQty * IM.TransactionRate), 2) AS TotalAmount
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
		                        ,IM.RequisitionDetailId
                                ,TaxAmount
                                 ,ccc.ReqTransactionQty
	                             ,IM.RefferenceNo
								,ccc.MaterialDetail,IM.GRNRcvQty,Balance=(ROUND(IM.TransactionQty, 2)-IM.GRNRcvQty)
	                        FROM [TRN].[PurchaseOrderDetail] AS IM
	                        LEFT JOIN MST.MaterialMaster AS MM ON IM.InventoryMaterialId = MM.Id
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
                            LEFT JOIN (select MRD.Id,MRD.MaterialDetail, Sum(MRD.TransactionQty) ReqTransactionQty from TRN.MaterialRequsitionDetails MRD group By MRD.Id,MRD.MaterialDetail) ccc On ccc.Id=IM.RequisitionDetailId  
                            where IM.InventoryMaterialId is not  null AND IM.QtyStatus=0

	                      -- WHERE IM.InventoryReceiveId = @inventoryReceiveId
                          
                          Union All
							SELECT  IR.Id AS POmasterId,IM.InventoryReceiveId
		                        ,IM.Id AS InventoryReceiveDetailId
		                        ,'' AS MaterialGroupName
		                        ,'' AS MaterialMasterId
		                        ,'' MaterialName
		                        ,IM.ArticleId
		                        ,'' Article
		                        ,IM.FirstCharacteristicsId
		                        ,'' AS FirstCharacteristics
		                        ,IM.FirstCharacteristicsValueId
		                        ,'' AS Sku1
		                        ,IM.SecondCharacteristicsId
		                        ,'' SecondCharacteristics
		                        ,IM.SecondCharacteristicsValueId
		                        ,'' AS Sku2
		                        ,IM.ThirdCharacteristicsId
		                        ,'' AS ThirdCharacteristics
		                        ,IM.ThirdCharacteristicsValueId
		                        ,'' AS Sku3
		                        ,ROUND(IM.TransactionQty, 2) TransactionQty
		                        ,IM.TransactionUoMId
		                        ,TUoM.UserName AS TransactionUoM
		                        ,ROUND(IM.TransactionRate, 2) TransactionRate
		                        ,CU.Code AS CurrencyName
		                        ,IR.ToCurrencyRate
		                        ,ROUND((IM.TransactionQty * IM.TransactionRate), 2) AS TotalAmount
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
		                        ,IM.RequisitionDetailId
                                ,TaxAmount
                                 ,ccc.ReqTransactionQty
	                            ,IM.RefferenceNo
								,ccc.MaterialDetail,IM.GRNRcvQty,Balance=(ROUND(IM.TransactionQty, 2)-IM.GRNRcvQty)
	                        FROM [TRN].[PurchaseOrderDetail] AS IM
	                        --JOIN MST.MaterialMaster AS MM ON IM.InventoryMaterialId = MM.Id
	                        --LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
	                        --LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
	                        --LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
	                        --LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
	                        --LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
	                        --LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
	                        --LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
	                        --LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
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
                            LEFT JOIN (select MRD.Id,MRD.MaterialDetail, Sum(MRD.TransactionQty) ReqTransactionQty from TRN.MaterialRequsitionDetails MRD group By MRD.Id,MRD.MaterialDetail) ccc On ccc.Id=IM.RequisitionDetailId  
                            where IM.InventoryMaterialId is  null AND IM.QtyStatus=0";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public IEnumerable<object> GetInventoryMaterialListForPOUpdate(string inveReveiveId, string MaterialMasterId, string ArticleId, string FirstCharacteristicsValueId, string SecondCharacteristicsValueId, string ThirdCharacteristicsValueId)
        {
            try
            {
                var _sql = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
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
		                ,ROUND(POREQD.TransactionQty, 2) PORaisedQty
                        ,POREQD.TransactionQty AS TransactionQty
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
                        ,ReqTransactionQty ReqQty
                        ,(ReqTransactionQty-ROUND(IM.TransactionQty, 2)) BalanceQty
                        ,(ROUND(IM.TransactionQty, 2)*ROUND(IM.TransactionRate, 2) ) TransactionAmount
                        ,MM.HSNCodeId
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
                      
                    LEFT JOIN TRN.PoRequisitionDetail POREQD On POREQD.PoDetailId= IM.Id
                    LEFT JOIN (select MRD.Id, Sum(MRD.TransactionQty) ReqTransactionQty from TRN.MaterialRequsitionDetails MRD group By MRD.Id) ccc On ccc.Id=POREQD.RequisitionDetailId
	                WHERE IM.Id = @inventoryReceiveId
					UNION ALL
					  SELECT  '' AS POmasterId,'' InventoryReceiveId
		                ,'' AS InventoryReceiveDetailId
		                ,MGM.UserName AS MaterialGroupMasterName
		                ,IM.MaterialMasterId AS MaterialMasterId
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
		                ,'0' PORaisedQty
                        ,'0' AS TransactionQty
                        ,'0' AS PreviousQty
		                ,IM.TransactionUoMId
		                ,TUoM.UserName AS TransactionUoM
		                ,'0' TransactionRate
		                ,''CurrencyName
		                ,'0' ToCurrencyRate
		                ,'0' AS TrnAmount
                        ,'0' BaseAmount
		               
		                ,'0' ASBaseTaxAmount 
		                ,'0' ChargesAmount
		                ,'0' TransactionAmount
		                ,'0' TransactionAmount
		                ,'' CountryId
		                ,NULL TaxList
		                ,'' InvoicingPartyPlantId
		                ,'' AS InvoicingStateId
		                ,'' AS PlantStateId
		                ,'' DeliveryInstruction
		                ,'' SpecialInstruction
		                , '' Description
		                ,Replace(CONVERT(VARCHAR(11), IM.DeliveryDate, 106), ' ', '-') DeliveryDate
		                ,'' RequisitionId
		                ,IM.Id AS RequisitionDetailId
                        ,POREQD.Id
                        ,'0' TaxAmount
                        ,IM.TransactionQty ReqQty
                        ,(ccc.ReqTransactionQty-ROUND(IM.TransactionQty, 2)) BalanceQty
                        ,(ROUND(IM.TransactionQty, 2)*ROUND(IM.EstimatedRate, 2) ) TransactionAmount
                        ,MM.HSNCodeId
	                FROM [TRN].[MaterialRequsitionDetails] AS IM
	                left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
	                LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
	                LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
	                LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
	                LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
	                LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
	                LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
	                LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
	                LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
	                JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId = TUoM.Id
	                JOIN [TRN].MaterialRequsitionMaster AS IR ON IM.MaterialReqqusitionMasterId = IR.Id
	                --JOIN [SCS].[Currency] AS CU ON IR.c = CU.Id
	               --LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id = IR.InvoicingPartyPlantId
	                --LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id = PPI.AddressMasterId
	                --LEFT JOIN [SCS].[State] AS ST ON ST.Id = AM.StateId
	               -- LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id = IR.DeliveryPartyPlantId
	               -- LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id = PPD.AddressMasterId
	               -- LEFT JOIN [SCS].[State] AS STD ON STD.Id = AMD.StateId
	              --  LEFT JOIN [ORG].[Plant] AS PT ON PT.Id = IR.PlantId
	               -- LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id = PT.AddressMasterId
                   -- Left join (select InventoryReceiveId,sum(TaxAmount) TaxAmount from TRN.purchaseOrderTax where InventoryReceiveId=@inventoryReceiveId and InventoryServiceId is null group by InventoryReceiveId) aaa On aaa.InventoryReceiveId=IM.              
                    LEFT JOIN TRN.PoRequisitionDetail POREQD On POREQD.PoDetailId= IM.Id
					LEFT JOIN (select MRD.Id, Sum(MRD.TransactionQty) ReqTransactionQty from TRN.MaterialRequsitionDetails MRD group By MRD.Id) ccc On ccc.Id=POREQD.RequisitionDetailId
	                WHERE IM.MaterialMasterId='"+ MaterialMasterId + "' and ArticleId='"+ ArticleId + "' and IM.FirstCharacteristicsValueId='"+ FirstCharacteristicsValueId + "' And IM.PORcvQty=0";
                return _sqlRepository.GetDataCollection(_sql);
				//string MaterialMasterId, string ArticleId, string FirstCharacteristicsValueId, string SecondCharacteristicsValueId, string ThirdCharacteristicsValueId
			}
			catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        #endregion
    }
}