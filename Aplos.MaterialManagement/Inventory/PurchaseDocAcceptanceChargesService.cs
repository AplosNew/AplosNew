using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Model.Taxations;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.ViewModel.Inventory;
using Library.ViewModel.Materials;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace Library.MaterialManagement.Inventory
{
    public class PurchaseDocAcceptanceChargesService : Service<PurchaseDocAcceptanceCharges>, IPurchaseDocAcceptanceChargesService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;

        public PurchaseDocAcceptanceChargesService(
             IRepositoryAsync<PurchaseDocAcceptanceCharges> purchaseDocAcceptanceServiceRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(purchaseDocAcceptanceServiceRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
        }

        #endregion Constructor
        
      //  public IEnumerable<object> GetPOWithLCList(string plantId, string PoType)
      //  {
      //      try
      //      {
               
      //          var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
      //                             SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
      //                               , CP.UserName AS PartyAccountGroupName
      //                                     , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
      //                                     --, IR.GateEntryNo
      //                                        --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
      //                                        , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
      //                                     , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
      //                                     , IR.FixedAssetOrInventory, IR.PODepended
      //                                        --, IR.AlongwithInvoice
      //                                        --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
      //                                     , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
      //                                     , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
      //                                        , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
      //          			, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
      //                      ,IPP.UserName As InvoicingByName
      //                      ,pgl.CtnId
      //                       	,PLC.Id AS PurchaseLCNO
						//	,IR.ContractId
						//	, REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') AS LCOpeningDate
						//	, REPLACE(CONVERT(CHAR(11), PLC.AddedDate, 106),' ','-') AS LCEntryDate							
						//	,BM.AccountTitle LCOpeningBank
      //                            FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
      //                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
      //                               ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
      //                            JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
      //                            JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
      //                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
      //                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
      //                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
      //                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
      //                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
      //                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
      //                            LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
      //                            LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
      //          LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
      //                            LEFT JOIN (SELECT A.InventoryReceiveId,A.QtyStatus, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
      //                                  JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId,A.QtyStatus) AS IRD ON IRD.InventoryReceiveId=IR.Id
      //                            LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
      //                                  WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
      //                            LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
      //                            LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=IR.Id
      //                            LEFT JOIN dbo.PurchaseLC PLC ON PLC.id= IR.PurchaseLCId
						//		  LEFT JOIN [MST].[BankMaster] BM ON BM.Id=PLC.OpeningBankMasterId

      //                            WHERE IR.PlantId='" + plantId + @"' 
      //                            AND IR.IsClosed=0 and IRD.QtyStatus=0  AND IR.POType='" + PoType + @"' AND pgl.CtnId is not null
      //                             AND IR.ContractId IS NOT NULL AND IR.PurchaseLCId IS NOT NULL
      //                            Order by IR.PODate ASC";
      //          return _sqlRepository.GetDataCollection(Sql);
      //      }
      //      catch (Exception ex)
      //      {
      //          throw new CustomException(ex.Message, ex,
      //              Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
      //              ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
      //      }
      //  }

   
      //  public IEnumerable<object> GetAcceptanceCharges()
      //  {
      //      try
      //      {

      //          var Sql = @"select *  from [HKP].LCChargesType where Type = '"+ChargesType.Acceptance.ToString()+"'";
      //          return _sqlRepository.GetDataCollection(Sql);
      //      }
      //      catch (Exception ex)
      //      {
      //          throw new CustomException(ex.Message, ex,
      //              Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
      //              ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
      //      }
      //  }

      //  public GridModel QueryOnlyPO(GridParameter parameters, string inveReveiveId)
      //  {
      //      //try
      //      //{
      //      //    parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
      //      //                       , @totalReceiveAmount DECIMAL(18, 4)=0
      //      //                       , @totalServiceAmount DECIMAL(18, 4)=0
      //      //                       , @totalSvcTaxAmount DECIMAL(18, 4)=0
      //      //            SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(TransactionAmount, 0)),1) FROM [TRN].[PurchaseOrderDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
      //      //            SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount - GRNServiceAmount, 0)),0) As Amount FROM [TRN].[POService] WHERE InventoryReceiveId=@inventoryReceiveId)
      //      //            SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
      //      //            SELECT 
      //      //                  --IM.Id
      //      //                 IR.Id AS POID,IRD.Id AS PODetailsID
      //      //                ,IRD.Id AS InventoryReceiveDetailId
      //      //                , MGM.UserName AS MaterialGroupMasterName
      //      //                , IM.MaterialMasterId, MM.UserName
      //      //                ,IRD.MaterialStorageId
      //      //                ,IRD.BaseUOMId
      //      //                , IM.ArticleId, ART.StandardName
      //      //                , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
      //      //                , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
      //      //                , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
      //      //                , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
      //      //                , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
      //      //                , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
      //      //                , IRD.TransactionQty AS POQty
      //      //                , ISNULL(IRD.GRNRcvQty,0) AS GRNRcvQty
      //      //                ,(IRD.TransactionQty-ISNULL(IRD.GRNRcvQty,0)) AS TransactionQty
      //      //                ,ISNULL(IRD.QtyStatus,0) QtyStatus
      //      //                , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM, IRD.TransactionRate, CU.Code AS CurrencyName, IR.ToCurrencyRate
      //      //                --, (IRD.TransactionQty*IRD.TransactionRate) AS TrnAmount
      //      //                 ,((IRD.TransactionQty-ISNULL(IRD.GRNRcvQty,0))*IRD.TransactionRate) AS TrnAmount
      //      //                --, IRD.BaseAmount
      //      //                --,(((IRD.TransactionQty-ISNULL(IRD.GRNRcvQty,0))*IRD.TransactionRate)*IR.ToCurrencyRate)  AS BaseAmount

      //      //                , IRD.TotalTaxAmount AS BaseTaxAmount
      //      //             , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
      //      //             , IRD.ChargesAmount
      //      //             --, ServiceCharge=(@totalServiceAmount/@totalReceiveAmount)*IRD.TransactionAmount
      //      //             --, ServiceTax=(@totalSvcTaxAmount/@totalReceiveAmount)*IRD.TransactionAmount
      //      //                 ,ServiceCharge=ROUND((@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.TransactionAmount,3)
      //      //             , ServiceTax=ROUND((@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.TransactionAmount,3)

      //      //             , IRD.CountryId
      //      //                ,'True' enableid
      //      //                ,null POMaterialTaxList
      //      //                ,TotalMaterialTranAmount= case when IR.IsNonCreditable=1 Then ROUND(((ROUND((IRD.TransactionQty*IRD.TransactionRate)-(ISNULL(IRD.GRNRcvQty,0)),2)) + (SELECT SUM(TaxAmount) FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId=IRD.Id) + ((@totalServiceAmount/@totalReceiveAmount)*IRD.TransactionAmount) + ((@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.TransactionAmount)),3)   
      //      //                --else  IRD.BaseAmount +((@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.TransactionAmount) END
      //      //                else (((IRD.TransactionQty-ISNULL(IRD.GRNRcvQty,0))*IRD.TransactionRate) + (@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.TransactionAmount) END
      //      //                ,ToTalMaterialBooksCurrencyAmount = case when IR.IsNonCreditable=1 Then (ROUND(((ROUND((IRD.TransactionQty*IRD.TransactionRate)-(ISNULL(IRD.GRNRcvQty,0)),2)) + (SELECT SUM(TaxAmount) FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId=IRD.Id) + ((@totalServiceAmount/@totalReceiveAmount)*IRD.TransactionAmount) + ((@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.TransactionAmount)),3) * IR.ToCurrencyRate)
      //      //                else ((((IRD.TransactionQty-ISNULL(IRD.GRNRcvQty,0))*IRD.TransactionRate) + (@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.TransactionAmount) * IR.ToCurrencyRate) END
      //      //               ,IR.InvoicingByAddress,IR.DeliveryByAddress
      //      //            FROM TRN.POMaterial AS IM
      //      //            JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
      //      //            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
      //      //            LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
      //      //            LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
      //      //            LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
      //      //            LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
      //      //            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
      //      //            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
      //      //            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
      //      //            JOIN [TRN].[PurchaseOrderDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
      //      //            JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
      //      //            JOIN [TRN].[PurchaseOrder] AS IR ON IRD.InventoryReceiveId=IR.Id
      //      //            JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
      //      //            WHERE IRD.InventoryReceiveId=@inventoryReceiveId and IRD.QtyStatus=0";
      //      //    return _sqlRepository.GetDifferentGridData(parameters);
      //      //}
      //      //catch (Exception ex)
      //      //{
      //      //    throw new CustomException(ex.Message, ex,
      //      //        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
      //      //        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
      //      //}
      //      try
      //      {
      //          parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
	     //                             , @totalReceiveAmount DECIMAL(18, 4)=0
	     //                             , @totalServiceAmount DECIMAL(18, 4)=0
	     //                             , @totalSvcTaxAmount DECIMAL(18, 4)=0
      //                  SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(TransactionAmount, 0)),1) FROM [TRN].[PurchaseOrderDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
      //                  SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount - GRNServiceAmount, 0)),0) As Amount FROM [TRN].[POService] WHERE InventoryReceiveId=@inventoryReceiveId)
      //                  SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
      //                     SELECT 
      //                        --IM.Id
      //                       IR.Id AS POID,IRD.Id AS PODetailsID
      //                      ,IRD.Id AS InventoryReceiveDetailId
      //                      , MGM.UserName AS MaterialGroupMasterName
      //                      , MM.Id MaterialMasterId
						//	, MM.UserName
      //                      ,IRD.MaterialStorageId
      //                      ,IRD.BaseUOMId
      //                      , IRD.ArticleId, ART.StandardName
      //                      , IRD.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
      //                      , IRD.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
      //                      , IRD.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
      //                      , IRD.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
      //                      , IRD.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
      //                      , IRD.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
      //                      , IRD.TransactionQty AS POQty
      //                      , ISNULL(IRD.GRNRcvQty,0) AS GRNRcvQty                           
      //                      --,(IRD.TransactionQty - ISNULL(IRD.GRNRcvQty,0)) AS TransactionQty
      //                        ,'' AS TransactionQty
      //                       ,(IRD.TransactionQty-IRD.GRNRcvQty) As Balance
      //                      ,ISNULL(IRD.QtyStatus,0) QtyStatus
      //                      , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM, IRD.TransactionRate, CU.Code AS CurrencyName, IR.ToCurrencyRate
                           
      //                      ,0 AS TrnAmount  
      //                      ,0 AS BaseTaxAmount
      //                      ,0 AS TaxAmount
	     //                   , 0 AS ChargesAmount
      //                      ,0 AS  ServiceCharge
      //                      , 0 AS ServiceTax
	     //                   , IRD.CountryId
      //                      ,'True' enableid
      //                      ,null POMaterialTaxList                            
      //                      ,0 AS TotalMaterialTranAmount
      //                      , 0 AS ToTalMaterialBooksCurrencyAmount
      //                     ,IR.InvoicingByAddress,IR.DeliveryByAddress
      //                     ,IRD.RequisitionId
						//   ,IRD.RequisitionDetailId
      //                     --,MRD.MaterialDetail
      //                     ,null AS [check] ,IRD.Description MaterialDetail
      //                   FROM TRN.PurchaseOrderDetail AS IRD
						//--LEFT JOIN TRN.PurchaseOrderDetail AS IRD ON IRD.InventoryMaterialId=PM.Id
      //                   left JOIN MST.MaterialMaster AS MM ON IRD.InventoryMaterialId=MM.Id
      //                  LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
      //                  LEFT JOIN MST.MaterialMasterArticle AS ART ON IRD.ArticleId=ART.Id
      //                  LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId=FC.Id
      //                  LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId=SC.Id
      //                  LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId=TC.Id
      //                  LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId=FCV.Id
      //                  LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId=SCV.Id
      //                  LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId=TCV.Id
      //                 -- JOIN [TRN].[PurchaseOrderDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
      //                  LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
      //                  LEFT JOIN [TRN].[PurchaseOrder] AS IR ON IRD.InventoryReceiveId=IR.Id
      //                  LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
      //                  LEFT join [trn].MaterialRequsitionDetails MRD on MRD.Id=IRD.RequisitionDetailId
      //                  WHERE IRD.InventoryReceiveId=@inventoryReceiveId and IRD.QtyStatus=0 and IRD.InventoryMaterialId is not null 				
						
						
      //         Union ALL
					 //    SELECT 
      //                        --IM.Id
      //                       IR.Id AS POID,IRD.Id AS PODetailsID
      //                      ,IRD.Id AS InventoryReceiveDetailId
      //                      , MGM.UserName AS MaterialGroupMasterName
      //                      , IRD.InventoryMaterialId MaterialMasterId
						//	, MM.UserName
      //                      ,IRD.MaterialStorageId
      //                      ,IRD.BaseUOMId
      //                      , IRD.ArticleId, ART.StandardName
      //                      , IRD.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
      //                      , IRD.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
      //                      , IRD.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
      //                      , IRD.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
      //                      , IRD.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
      //                      , IRD.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
      //                      , IRD.TransactionQty AS POQty
      //                      , ISNULL(IRD.GRNRcvQty,0) AS GRNRcvQty                           
      //                      --,(IRD.TransactionQty - ISNULL(IRD.GRNRcvQty,0)) AS TransactionQty
      //                        ,'' AS TransactionQty
      //                       ,(IRD.TransactionQty-IRD.GRNRcvQty) As Balance
      //                      ,ISNULL(IRD.QtyStatus,0) QtyStatus
      //                      , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM, IRD.TransactionRate, CU.Code AS CurrencyName, IR.ToCurrencyRate
                           
      //                      ,0 AS TrnAmount  
      //                      ,0 AS BaseTaxAmount
      //                      ,0 AS TaxAmount
	     //                   , 0 AS ChargesAmount
      //                      ,0 AS  ServiceCharge
      //                      , 0 AS ServiceTax
	     //                   , IRD.CountryId
      //                      ,'True' enableid
      //                      ,null POMaterialTaxList                            
      //                      ,0 AS TotalMaterialTranAmount
      //                      , 0 AS ToTalMaterialBooksCurrencyAmount
      //                     ,IR.InvoicingByAddress,IR.DeliveryByAddress
      //                     ,IRD.RequisitionId
						//   ,IRD.RequisitionDetailId
      //                     --,MRD.MaterialDetail
      //                     ,null AS [check] ,IRD.Description MaterialDetail
      //                   FROM TRN.PurchaseOrderDetail AS IRD
						//--LEFT JOIN TRN.PurchaseOrderDetail AS IRD ON IRD.InventoryMaterialId=PM.Id
      //                   left JOIN MST.MaterialMaster AS MM ON IRD.InventoryMaterialId=MM.Id
      //                  LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
      //                  LEFT JOIN MST.MaterialMasterArticle AS ART ON IRD.ArticleId=ART.Id
      //                  LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId=FC.Id
      //                  LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId=SC.Id
      //                  LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId=TC.Id
      //                  LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId=FCV.Id
      //                  LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId=SCV.Id
      //                  LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId=TCV.Id
      //                 -- JOIN [TRN].[PurchaseOrderDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
      //                  LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
      //                  LEFT JOIN [TRN].[PurchaseOrder] AS IR ON IRD.InventoryReceiveId=IR.Id
      //                  LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
      //                  LEFT join [trn].MaterialRequsitionDetails MRD on MRD.Id=IRD.RequisitionDetailId
      //                  WHERE IRD.InventoryReceiveId=@inventoryReceiveId and IRD.QtyStatus=0 and IRD.InventoryMaterialId is null 				";
      //          return _sqlRepository.GetDifferentGridData(parameters);
      //      }
      //      catch (Exception ex)
      //      {
      //          throw new CustomException(ex.Message, ex,
      //              Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
      //              ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
      //      }

      //  }
      //  private string GetPK()
      //  {
      //      string sID = string.Empty;
      //      bplib.clsGenID objGenID = new bplib.clsGenID();
      //      objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(PurchaseDocAcceptance), out sID);
      //      return sID;
      //  }
      //  public void InsertOrUpdateGraphNew(PurchaseDocAcceptance entity, IEnumerable<PurchaseDocAcceptanceDetailViewModel> PurchaseDocAcceptanceDetail, IEnumerable<PurchaseDocAcceptanceServiceViewModel> AcceptancechargesList)
      //  {
      //      var flag = false;

      //      try
      //      {
      //          _unitOfWork.BeginTransaction();

      //          flag = true;
      //          entity.Id = GetPK();               
      //          _purchaseDocumentAcceptanceService.Insert(entity);
      //          var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

      //          var currentId1 = _purchaseDocAcceptanceDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceDetail]  WHERE PurchaseDocAcceptanceId ='{entity.Id}'").First();
      //          //var Temppodetailid = "";

      //          var grndId = "";

      //          foreach (var itemDetail in PurchaseDocAcceptanceDetail)
      //          {
      //              //itemDetail.CompanyGroupId = identity.CompanyGroupId;
      //              //itemDetail.CompanyId = identity.CompanyId;
      //              //itemDetail.PlantId = identity.PlantId;
      //              //Temppodetailid = itemDetail.InventoryReceiveDetailId;
      //              //itemDetail.IsNonCreditable = entity.IsNonCreditable;
      //              //if (CheckItemExist(itemDetail))
      //              //    throw new CustomException(itemDetail.MaterialMasterName + " already received");

      //              //ResetCurrencyRate(itemDetail);

      //              if (itemDetail.IsNotNull())
      //              {
      //                  ////Added DAte 22-10-2019
      //                  //// var ratio = _inventoryReceiveService.GetChargesRatio(itemDetail.InventoryReceiveId, itemDetail.Id, Convert.ToDecimal(itemDetail.TrnAmount), null, 0, itemDetail.IsNonCreditable);
      //                  //// var ratioServiceTax = _inventoryReceiveService.GetChargesTaxRatio(itemDetail.InventoryReceiveId, itemDetail.Id, Convert.ToDecimal(itemDetail.TrnAmount), null, 0, itemDetail.IsNonCreditable);
      //                  ////End

      //                  //var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(itemDetail);
      //                  //if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
      //                  /////TODO : Get total qyt and amount by country and issue qty
      //                  //itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
      //                  //itemDetail.IssueQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueQty).Sum());
      //                  //var ShortageQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ShortageQty).Sum();
      //                  //var RejectionQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.RejectionQty).Sum();
      //                  //var ApprovedQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ApprovedQty).Sum();


      //                  //var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.TotalMaterialTranAmount).Sum();

      //                  //var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
      //                  //var altUomIds = new string[] { itemDetail.TransactionUoMId };
      //                  //var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

      //                  //if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId
      //                  //     && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
      //                  //{
      //                  //    //itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
      //                  //    //itemDetail.BaseQty = Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);

      //                  //    //ShortageQty = Convert.ToDecimal(itemDetail.ShortageQty * itemDetail.BaseUoMFactor);
      //                  //    //RejectionQty = Convert.ToDecimal(itemDetail.RejectionQty * itemDetail.BaseUoMFactor);
      //                  //    //ApprovedQty = Convert.ToDecimal(itemDetail.ApprovedQty * itemDetail.BaseUoMFactor);

      //                  //    //itemDetail.TotalMaterialTranAmount = itemDetail.MaterialTranAmount * itemDetail.ToCurrencyRate;

      //                  //    ////entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
      //                  //    ////entity.AvgRate = Convert.ToDecimal((totalAmount + entity.TotalMaterialTranAmount) / entity.TotalQty);
      //                  //    ///Added Date 22-10-19
      //                  //    itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
      //                  //    itemDetail.BaseQty = Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
      //                  //    itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
      //                  //    itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
      //                  //    itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
      //                  //    if (itemDetail.TotalTaxAmount == null)
      //                  //        itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
      //                  //    itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
      //                  //      Convert.ToDecimal(itemDetail.ChargesTranAmount);
      //                  //    itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

      //                  //    itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
      //                  //             Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
      //                  //    itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
      //                  //    itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;
      //                  //    //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
      //                  //    //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.TotalMaterialTranAmount) / entity.TotalQty);
      //                  //}
      //                  //else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
      //                  //{
      //                  //    //itemDetail.BaseQty = itemDetail.TransactionQty;
      //                  //    //ShortageQty = itemDetail.ShortageQty;
      //                  //    //RejectionQty = itemDetail.RejectionQty;
      //                  //    //ApprovedQty = itemDetail.ApprovedQty;
      //                  //    //itemDetail.BaseUoMFactor = itemDetail.TransactionQty;							
      //                  //    //itemDetail.TotalMaterialTranAmount = itemDetail.MaterialTranAmount * itemDetail.ToCurrencyRate;
      //                  //    //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
      //                  //    //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.TotalMaterialTranAmount) / entity.TotalQty);

      //                  //    //added date 22-10-2019
      //                  //    itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
      //                  //    itemDetail.BaseQty = Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
      //                  //    //itemDetail.TotalMaterialTranAmount = itemDetail.TransactionAmount;
      //                  //    itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
      //                  //    itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
      //                  //    itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
      //                  //    if (itemDetail.TotalTaxAmount == null)
      //                  //        itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
      //                  //    itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
      //                  //      Convert.ToDecimal(itemDetail.ChargesTranAmount);
      //                  //    itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

      //                  //    itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
      //                  //             Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
      //                  //    itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
      //                  //    itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

      //                  //    //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
      //                  //    //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.TotalMaterialTranAmount) / entity.TotalQty);
      //                  //}
      //                  //else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == itemDetail.BaseCurrencyId
      //                  //    && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
      //                  //{
      //                  //    //itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
      //                  //    //itemDetail.BaseQty = Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
      //                  //    //ShortageQty = Convert.ToDecimal(itemDetail.ShortageQty * itemDetail.BaseUoMFactor);
      //                  //    //RejectionQty = Convert.ToDecimal(itemDetail.RejectionQty * itemDetail.BaseUoMFactor);
      //                  //    //ApprovedQty = Convert.ToDecimal(itemDetail.ApprovedQty * itemDetail.BaseUoMFactor);							
      //                  //    //itemDetail.TotalMaterialTranAmount = itemDetail.MaterialTranAmount;
      //                  //    //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
      //                  //    //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.TotalMaterialTranAmount) / entity.TotalQty);
      //                  //    //AddedDate
      //                  //    itemDetail.BaseUoMFactor = 1;
      //                  //    itemDetail.BaseQty = itemDetail.TransactionQty;
      //                  //    itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
      //                  //    itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
      //                  //    itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
      //                  //    if (itemDetail.TotalTaxAmount == null)
      //                  //        itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
      //                  //    itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
      //                  //      Convert.ToDecimal(itemDetail.ChargesTranAmount);
      //                  //    itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
      //                  //    //itemDetail.ChargesTranAmount = itemDetail.MaterialTranAmount * ratio;
      //                  //    itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
      //                  //             Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
      //                  //    itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
      //                  //    itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

      //                  //    //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
      //                  //    //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.TotalMaterialTranAmount) / entity.TotalQty);
      //                  //}
      //                  //else
      //                  //{
      //                  //    //itemDetail.BaseUoMFactor = itemDetail.BaseUoMFactor;
      //                  //    //itemDetail.BaseQty = itemDetail.TransactionQty;
      //                  //    ////ShortageQty = itemDetail.ShortageQty;
      //                  //    ////RejectionQty = itemDetail.RejectionQty;
      //                  //    ////ApprovedQty = itemDetail.ApprovedQty;
      //                  //    ////itemDetail.TransactionAmount = itemDetail.TransactionAmount;
      //                  //    ////itemDetail.TransactionAmount = itemDetail.TrnAmount;
      //                  //    //itemDetail.MaterialTranAmount = itemDetail.TrnAmount;
      //                  //    ////itemDetail.TotalMaterialTranAmount = itemDetail.TotalMaterialTranAmount;
      //                  //    //itemDetail.TotalMaterialTranAmount = itemDetail.MaterialTranAmount;
      //                  //    //Added Date :22-10-2019
      //                  //    //itemDetail.BaseUoMFactor = itemDetail.TransactionQty;
      //                  //    itemDetail.BaseUoMFactor = 1;
      //                  //    itemDetail.BaseQty = itemDetail.TransactionQty;
      //                  //    itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
      //                  //    itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
      //                  //    itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
      //                  //    if (itemDetail.TotalTaxAmount == null)
      //                  //        itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;

      //                  //    itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
      //                  //      Convert.ToDecimal(itemDetail.ChargesTranAmount);

      //                  //    itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

      //                  //    itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
      //                  //             Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);

      //                  //    itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
      //                  //    itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

      //                  //}
      //                  ////  var PoMasterIds = entityMat.Select(r => r.POID);
      //                  ////   var POMasterList = _poRepository.Query(r => PoMasterIds.Contains(r.Id)).Select().ToList();
      //                  //var poDetail = _poDetailRepository.Query(r => r.Id == itemDetail.PODetailsID).Select().FirstOrDefault();
      //                  //// var IRDDetail = _receiveDetailRepository.Query(r => r.Id == itemDetail.PODetailsID).Select().FirstOrDefault();
      //                  //if (poDetail == null)
      //                  //    throw new CustomException("PO Details Or Inventory Details not found!");
      //                  ////if (poDetail == null )
      //                  ////    throw new CustomException("PO not found!");                        
      //                  ////if (IRDDetail == null)
      //                  ////    throw new CustomException("Receive Detail not found!");

      //                  //poDetail.GRNRcvQty += itemDetail.TransactionQty;
      //                  //// if (poDetail.BaseQty < poDetail.GRNRcvQty)
      //                  //if (poDetail.TransactionQty < poDetail.GRNRcvQty)
      //                  //    throw new CustomException("Received Qty can not cross balance Qty.");
      //                  ////if (poDetail.TransactionQty=="")
      //                  ////    throw new CustomException("Received Qty can not cross balance Qty.");
      //                  //poDetail.QtyStatus = poDetail.BaseQty == poDetail.GRNRcvQty;
      //                  //AuditService.UpdatedLog(poDetail);
      //                  //_poDetailRepository.Update(poDetail);






      //                  // Insert in receive detail
      //                  if (string.IsNullOrEmpty(itemDetail.Id))
      //                  {
      //                      var NewId = entity.Id + "-";
      //                      //var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 1) AS INT)), 0) Id FROM [TRN].[MaterialRequsitionDetails] WHERE //MaterialReqqusitionMasterId='{entity.MaterialReqqusitionMasterId}'").First();
      //                      //currentId++;

      //                      currentId1++;
      //                      grndId = NewId + currentId1;
      //                      var receiveDetail = new PurchaseDocAcceptanceDetail
      //                      {                                
      //                         Id = NewId + currentId1,
      //                          PurchaseDocAcceptanceId = itemDetail.PurchaseDocAcceptanceId,
      //                          MaterialMasterId = itemDetail.MaterialMasterId,
      //                          ArticleId = itemDetail.ArticleId,
      //                          FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
      //                          FirstCharacteristicsValueId = itemDetail.FirstCharacteristicsValueId,
      //                          SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
      //                          SecondCharacteristicsValueId = itemDetail.SecondCharacteristicsValueId,
      //                          ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
      //                          ThirdCharacteristicsValueId = itemDetail.ThirdCharacteristicsValueId,
      //                          TransactionQty = itemDetail.TransactionQty,
      //                          TransactionUoMId = itemDetail.TransactionUoMId,
      //                          MaterialTranRate = itemDetail.MaterialTranRate,
      //                          MaterialTranAmount = itemDetail.MaterialTranAmount,

      //                      };
      //                      try
      //                      {

      //                          //itemDetail.InventoryReceiveDetailId = receiveDetail.Id;
      //                          AuditService.AddedLog(receiveDetail);
      //                          ////var ratio = _inventoryReceiveService.GetChargesRatio(receiveDetail.InventoryReceiveId, receiveDetail.Id, receiveDetail.MaterialTranAmount, null, 0, itemDetail.IsNonCreditable);

      //                          //////receiveDetail.ChargesAmount = receiveDetail.TransactionAmount * ratio;
      //                          ////receiveDetail.ChargesTranAmount = receiveDetail.ChargesTranAmount;
      //                          //////receiveDetail.WithInvoiceRate = itemDetail.IsNonCreditable ? (receiveDetail.TransactionAmount + receiveDetail.TotalTaxAmount + receiveDetail.ChargesAmount) / receiveDetail.TransactionQty
      //                          //////                        : (receiveDetail.TransactionAmount + receiveDetail.ChargesAmount) / receiveDetail.TransactionQty;
      //                          ////receiveDetail.TrnCurrencyBaseRate = itemDetail.IsNonCreditable ? (receiveDetail.MaterialTranAmount + receiveDetail.TotalTaxAmount + receiveDetail.ChargesTranAmount + receiveDetail.ChargesTaxTranAmount) / receiveDetail.TransactionQty
      //                          //// : (receiveDetail.MaterialTranAmount + receiveDetail.ChargesTranAmount) / receiveDetail.TransactionQty;
      //                          //////receiveDetail.AfterInvoiceRate = receiveDetail.WithInvoiceRate;
      //                          ////receiveDetail.BooksCurrencyBaseRate = receiveDetail.TrnCurrencyBaseRate;

      //                          ////receiveDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + receiveDetail.ChargesAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
      //                          ////     Convert.ToDecimal(receiveDetail.ChargesAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);

      //                          ////itemDetail.TotalQty = Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty);
      //                          ////itemDetail.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.TotalMaterialTranAmount) / itemDetail.TotalQty);
      //                          //itemDetail.TotalQty = (Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty)) - Convert.ToDecimal(itemDetail.IssueQty);
      //                          //itemDetail.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.TotalMaterialTranAmount) / itemDetail.TotalQty);



      //                          ////itemDetail.ShortageQty = itemDetail.ShortageQty;
      //                          ////itemDetail.RejectionQty = itemDetail.RejectionQty;
      //                          ////itemDetail.ApprovedQty = itemDetail.ApprovedQty;

      //                          //itemDetail.ShortageQty = Convert.ToDecimal(itemDetail.ShortageQty + ShortageQty);
      //                          //itemDetail.RejectionQty = Convert.ToDecimal(itemDetail.RejectionQty + RejectionQty);
      //                          //itemDetail.ApprovedQty = Convert.ToDecimal(itemDetail.ApprovedQty + ApprovedQty);

      //                          //_inventoryMaterialMasterService.InsertOrUpdateFromReceive(itemDetail);
      //                          //receiveDetail.InventoryMaterialId = itemDetail.InventoryMaterialId;
      //                          _purchaseDocumentAcceptanceDetailService.InsertGraph(receiveDetail);
      //                          foreach (var ServiceitemDetail in AcceptancechargesList)
      //                          {

      //                              int rejectDetailId = 1;
      //                              var PurchaseDoService = new PurchaseDocAcceptanceService
      //                              {
      //                                  //Id = grndId.ToString() + rejectDetailId,
      //                                  //GRNDeailsId = grndId,
      //                                  //RejectionQty = Convert.ToDecimal(itemDetail.ShortageQty),
      //                                  //RejectionUoMId = itemDetail.TransactionUoMId,
      //                                  //BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
      //                                  //BaseUOMId = itemDetail.BaseUOMId,
      //                                  //RejectionRate = Convert.ToDecimal(itemDetail.RejectionRate),
      //                                  //RejeactionValue = Convert.ToDecimal(itemDetail.RejectionValue),
      //                                  Id = grndId.ToString() + rejectDetailId,
      //                                  //  public string Id { get; set; }
      //                                  PurchaseDocAcceptanceId = itemDetail.TransactionUoMId,
      //                                  Amount = Convert.ToDecimal(ServiceitemDetail.Amount),
      //                                  TotalTaxAmount = Convert.ToDecimal(ServiceitemDetail.TotalTaxAmount),

      //                              };
      //                              AuditService.AddedLog(PurchaseDoService);
      //                              _gRNRejectionDetailsRepository.Insert(PurchaseDoService);
      //                              //UpdateInventoryDetail(receiveDetail, ratio, Convert.ToDecimal(itemDetail.ToCurrencyRate), itemDetail.IsNonCreditable);
      //                          }
      //                      }
      //                      catch (DivideByZeroException ex)
      //                      {

      //                      }
      //                      finally
      //                      {

      //                      }
      //                  }
      //              }

      //              // insert in receive tax
      //              if (taxCategoryList.IsNotNull())
      //              {
      //                  var currentId = 0;
      //                  //var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId='{itemDetail.InventoryReceiveDetailId}'").First();
      //                  foreach (var item in taxCategoryList.Where(r => r.InventoryReceiveDetailId == Temppodetailid))
      //                  {
      //                      currentId++;
      //                      item.Id = MakePK(itemDetail.InventoryReceiveDetailId, currentId, 2);
      //                      item.InventoryReceiveId = entity.Id;//itemDetail.InventoryReceiveId;
      //                      item.InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId;
      //                      item.InventoryServiceId = null;
      //                      AuditService.AddedLog(item);
      //                      _receiveTaxRepository.Insert(item);
      //                  }
      //              }
      //          }
      //          _unitOfWork.SaveChanges();
      //          flag = false;
      //          _unitOfWork.Commit();
      //      }
      //      catch (CustomException)
      //      {
      //          throw;
      //      }
      //      catch (Exception ex)
      //      {
      //          throw new CustomException(ex.Message, ex,
      //          Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
      //           ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
      //      }
      //      finally
      //      {
      //          if (flag)
      //          {
      //              _unitOfWork.Rollback();
      //          }
      //      }
      //  }


    }
}