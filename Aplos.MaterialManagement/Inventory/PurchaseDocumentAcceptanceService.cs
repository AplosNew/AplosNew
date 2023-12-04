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
using Library.Service.Logs;
using Library.Service.Systems;
using Library.ViewModel.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.MaterialManagement.Inventory
{
    public class PurchaseDocumentAcceptanceService : Service<PurchaseDocAcceptance>, IPurchaseDocumentAcceptanceService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<PurchaseDocAcceptanceDetail> _purchaseDocAcceptanceDetailRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IPurchaseDocumentAcceptanceDetailService _purchaseDocumentAcceptanceDetailService;
        private readonly IPurchaseDocAcceptanceChargesService _purchaseDocAcceptanceChargesService;
        private readonly IPurchaseDocAcceptancePOMapService _PurchaseDocAcceptancePOMapService;
        private readonly IRepositoryAsync<PurchaseDocAcceptanceService> _purchaseDocAcceptanceServiceService;
        private readonly IRepositoryAsync<PurchaseDocAcceptanceCharges> _purchaseDocAcceptanceChargesRepository;
        private readonly IRepositoryAsync<PurchaseDocAcceptanceTax> _purchaseDocAcceptanceTax;
        private readonly IRepositoryAsync<PurchaseOrderTax> _receiveTaxRepository;
        private readonly IRepositoryAsync<ServicePOTax> _ServicePOTaxRepository;
        private readonly IPurchaseOrderServiceService _inventoryService;
        private readonly IRepositoryAsync<PurchaseOrderDetail> _poDetailRepository;

        public PurchaseDocumentAcceptanceService(
            IRepositoryAsync<PurchaseDocAcceptance> purchaseDocAcceptanceRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<PurchaseDocAcceptanceDetail> purchaseDocAcceptanceDetailRepository
            , IPurchaseDocumentAcceptanceDetailService purchaseDocumentAcceptanceDetailService
            , IPurchaseDocAcceptanceChargesService purchaseDocAcceptanceChargesService
            , IRepositoryAsync<PurchaseOrderDetail> poDetailRepository
            , IPurchaseDocAcceptancePOMapService PurchaseDocAcceptancePOMapService
            , IRepositoryAsync<PurchaseDocAcceptanceService> purchaseDocAcceptanceServiceService
            , IRepositoryAsync<PurchaseDocAcceptanceCharges> purchaseDocAcceptanceChargesRepository
            , IRepositoryAsync<PurchaseDocAcceptanceTax> purchaseDocAcceptanceServiceTax
            , IRepositoryAsync<PurchaseOrderTax> receiveTaxRepository
            , IPurchaseOrderServiceService inventoryService
            , IRepositoryAsync<ServicePOTax> ServicePOTaxRepository
            ) : base(purchaseDocAcceptanceRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _purchaseDocAcceptanceDetailRepository = purchaseDocAcceptanceDetailRepository;
            _purchaseDocumentAcceptanceDetailService = purchaseDocumentAcceptanceDetailService;
            _purchaseDocAcceptanceChargesService = purchaseDocAcceptanceChargesService;
            _poDetailRepository = poDetailRepository;
            _PurchaseDocAcceptancePOMapService = PurchaseDocAcceptancePOMapService;
            _purchaseDocAcceptanceServiceService = purchaseDocAcceptanceServiceService;
            _purchaseDocAcceptanceTax = purchaseDocAcceptanceServiceTax;
            _purchaseDocAcceptanceChargesRepository = purchaseDocAcceptanceChargesRepository;
            _receiveTaxRepository = receiveTaxRepository;
            _inventoryService = inventoryService;
            _ServicePOTaxRepository = ServicePOTaxRepository;
        }

        #endregion Constructor

        #region Operaiton 

        public IEnumerable<object> GetPOWithLCList(string plantId, string PoType)
        {
            try
            {
                var Sql = @"SELECT 
		                     PLC.id PurchaseLCNO
		                     ,PLC.ContractId
		                     ,p.UserName PartyName
		                     ,FORMAT( PLC.LCDate,'dd-MMM-yyyy') AS LCOpeningDate
		                     ,REPLACE(CONVERT(CHAR(11), PLC.ExpiryDate, 106),' ','-') AS LCExpiryDate							
		                     ,BM.AccountTitle LCOpeningBank	
							 ,PLC.VendorId PartyId
							 --,PP.Id PartyPlantId, PP.UserName PartyPlant
,PLC.LCRef,PLC.CurrencyId,CN.Code CurrencyName,PLC.Tenure,PLC.OpeningBankMasterId
							 ,BM.CurrencyId LCOBCurrencyId,BMC.Code OBCurrencyCode,ISNULL(C.ContractNo,'')ContractNo,AcceptanceFirst=CASE WHEN PLC.IsAccepptanceFirst=1 THEN 'Yes' ELSE 'No' END
							  ,ISNULL(PT.UserName,'') CustomerName,ISNULL(C.UDNo,'') UDNo,ISNULL(MLC.LCRef,'')MasterLCRef,PLC.Amount LCAmount
                    FROM dbo.PurchaseLC PLC  
                    LEFT JOIN dbo.[Contract] C On C.Id=PLC.ContractId
                    LEFT JOIN dbo.[MasterLC] MLC ON MLC.Id=C.MasterLCId
					LEFT JOIN [HKP].[Party] AS PT ON C.CustomerId=PT.Id
                    LEFT JOIN [MST].[BankMaster] BM ON BM.Id=PLC.OpeningBankMasterId
                    LEFT JOIN [SCS].[Currency] BMC ON BMC.Id=BM.CurrencyId
                    JOIN [HKP].[Party] AS P ON PLC.VendorId=P.Id
					--JOIN HKP.PartyPlant PP ON PP.PartyId=P.Id
                    JOIN SCS.Currency CN ON CN.Id=PLC.CurrencyId
                    WHERE PLC.Status='Active' AND PLC.PlantId='" + plantId + "' ORDER BY PLC.AddedDate DESC";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetLCWisePOList(string plantId, string PoType, string PurchaseLCNo)
        {
            try
            {

                var Sql = @"SELECT Flag='MaterialPO'
                                            , IR.Id
                                            , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
                                            , P.UserName AS PartyName
                                            , IR.MaterialStorageId, IR.DocRefNo
                                            , REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
                                            ,PD.TransactionAmount
                                            ,0 AS 'Active'
                                            FROM [TRN].[PurchaseOrder] AS IR 
                                            JOIN (SELECT SUM(TransactionAmount) TransactionAmount,InventoryReceiveId FROM [TRN].[PurchaseOrderDetail] GROUP BY InventoryReceiveId) PD ON PD.InventoryReceiveId=IR.Id
                                            JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                                            LEFT JOIN dbo.PurchaseLC PLC ON PLC.id= IR.PurchaseLCId
                                            LEFT JOIN [MST].[BankMaster] BM ON BM.Id=PLC.OpeningBankMasterId
                                           WHERE IR.PlantId='" + plantId + @"' 
                                           AND IR.IsClosed=0  
                                           AND IR.PurchaseLCId IS NOT NULL AND IR.PurchaseLCId='" + PurchaseLCNo + @"'
                    UNION 
                        SELECT Flag='ServicePO'
                                            , IR.Id
                                            , REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate
                                            , P.UserName AS PartyName
                                            , IR.MaterialStorageId, IR.DocRefNo
                                            , REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
                                            ,PD.TransactionAmount
                                            ,0 AS 'Active'
                                            FROM [TRN].[ServicePOMaster] AS IR 
                                            JOIN (SELECT SUM(Amount) TransactionAmount,ServicePOMasterId FROM [TRN].[ServicePODetail] GROUP BY ServicePOMasterId) PD ON PD.ServicePOMasterId=IR.Id
                                            JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                                            LEFT JOIN dbo.PurchaseLC PLC ON PLC.id= IR.PurchaseLCId
                                            LEFT JOIN [MST].[BankMaster] BM ON BM.Id=PLC.OpeningBankMasterId
                                           WHERE IR.PlantId='" + plantId + @"'
                                           AND IR.IsClosed=0  
                                           AND IR.PurchaseLCId IS NOT NULL AND IR.PurchaseLCId='" + PurchaseLCNo + @"'";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetGRNList(string plantId, string purchaseLCId)
        {
            try
            {
                //var Sql = @"SELECT Convert(bit,0) Active,IR.Id,RD.TotalMaterialTranAmount,PO.Id POId, PO.DocRefNo PODocRefNo,IR.DocRefNo
                //            ,P.UserName AS PartyName,REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
                //            ,IR.GateEntryNo,C.Code Currency,POD.TransactionAmount
                //            FROM [TRN].[InventoryReceive] AS IR 
                //            JOIN (SELECT SUM(TotalMaterialTranAmount) TotalMaterialTranAmount,InventoryReceiveId,POId FROM [TRN].[InventoryReceiveDetail] GROUP BY InventoryReceiveId,POId) RD ON RD.InventoryReceiveId=IR.Id
                //            LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=RD.POId
                //            JOIN(SELECT SUM(TransactionAmount) TransactionAmount,InventoryReceiveId FROM [TRN].[PurchaseOrderDetail] GROUP BY InventoryReceiveId)POD ON POD.InventoryReceiveId=PO.Id
                //            JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                //            JOIN [SCS].[Currency] C ON C.Id=IR.CurrencyId
                //            WHERE IR.PlantId='"+plantId+@"' AND ISNULL(IR.VoucherId,'')<>'' AND IR.[Status]='Posting' AND IR.IsApproved=1 AND PO.PurchaseLCId='"+purchaseLCId+@"'";
                string Sql = @"SELECT Convert(bit,0) Active,IR.Id,SUM(RD.TotalMaterialTranAmount) TotalMaterialTranAmount
                                ,IR.DocRefNo,P.UserName AS PartyName,REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
                                ,IR.GateEntryNo,C.Code Currency
                                ,PODocRefNo= STUFF((select distinct ','+PO.DocRefNo
			                                from TRN.POGGRNMap PG 
                                            LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=PG.POId	  
			                                where PG.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                --,POId= STUFF((select distinct ','+PG.POId
			                             --   FROM TRN.POGGRNMap PG 
                                --            LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=PG.POId	  
			                             --   WHERE PG.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
											,RD.POId,RD.PODetailsId
                                FROM [TRN].[InventoryReceive] AS IR 
                                LEFT JOIN [TRN].[InventoryReceiveDetail] RD ON RD.InventoryReceiveId=IR.Id
                                LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                                LEFT JOIN [SCS].[Currency] C ON C.Id=IR.CurrencyId
                                WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.VoucherId,'')<>'' 
                                AND IR.[Status]='Posting' AND IR.IsApproved=1 AND  RD.POId IN (SELECT Id From TRN.PurchaseOrder Where PurchaseLCId='" + purchaseLCId + @"')
                                GROUP BY IR.Id,IR.DocRefNo,P.UserName,IR.DocDate,IR.GateEntryNo,C.Code,RD.POId,RD.PODetailsId";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public IEnumerable<object> GetRecordDoubleClickMaster(string plantId, string Id, string PoType)
        {
            try
            {

                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                                   SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id AS POId, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
                                     , CP.UserName AS PartyAccountGroupName
                                           , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
                                           --, IR.GateEntryNo
                                              --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                              , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
                                           , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
                                           , IR.FixedAssetOrInventory, IR.PODepended
                                              --, IR.AlongwithInvoice
                                              --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
                                           , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
                                           , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                              , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
                			, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                            ,IPP.UserName As InvoicingByName
                            ,pgl.CtnId
                             	,PLC.Id AS PurchaseLCNO
							,IR.ContractId
                            ,CU.Code AS CurrencyName
							, REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') AS LCOpeningDate
							, REPLACE(CONVERT(CHAR(11), PLC.ExpiryDate, 106),' ','-') AS LCExpiryDate							
							,BM.AccountTitle LCOpeningBank,PT.UserName PaymentTermName,Acc.Id,Acc.AcceptanceDate,Acc.AcceptanceNo,Acc.AcceptanceDate,Acc.Remarks
                                  FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                                  LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                                     ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                                  JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                  JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                                  LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                                  LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                                  LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                                  LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                                  LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                                  LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                                  LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                                  LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                                  LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                                  LEFT JOIN (SELECT A.InventoryReceiveId,A.QtyStatus, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
                                        JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId,A.QtyStatus) AS IRD ON IRD.InventoryReceiveId=IR.Id
                                  LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
                                        WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                                  LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                                  LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=IR.Id
                                  LEFT JOIN dbo.PurchaseLC PLC ON PLC.id= IR.PurchaseLCId
								  LEFT JOIN [MST].[BankMaster] BM ON BM.Id=PLC.OpeningBankMasterId
                                left join trn.PurchaseDocAcceptance Acc ON Acc.POId=IR.Id
                                  WHERE IR.PlantId='" + plantId + @"' ANd Acc.Id='" + Id + @"'
                                  --AND IR.IsClosed=0 and IRD.QtyStatus=0  AND IR.POType='" + PoType + @"' AND pgl.CtnId is not null
                                   --AND IR.ContractId IS NOT NULL AND IR.PurchaseLCId IS NOT NULL
                                  Order by IR.PODate ASC";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public IEnumerable<object> GetRecordDoubleClickDetail(string plantId, string Id, string PoType)
        {
            try
            {

                var Sql = @"DECLARE @inventoryReceiveId VARCHAR(10) = ''
	                                , @totalReceiveAmount DECIMAL(18, 4)= 0
	                                , @totalServiceAmount DECIMAL(18, 4)= 0
	                                , @totalSvcTaxAmount DECIMAL(18, 4)= 0
                    -- SET @totalReceiveAmount = (SELECT ISNULL(SUM(ISNULL(TransactionAmount, 0)), 1) FROM[TRN].[PurchaseOrderDetail] WHERE InventoryReceiveId = @inventoryReceiveId)
                    -- SET @totalServiceAmount = (SELECT ISNULL(SUM(ISNULL(Amount - GRNServiceAmount, 0)), 0) As Amount FROM[TRN].[POService] WHERE InventoryReceiveId = @inventoryReceiveId)
                    --SET @totalSvcTaxAmount = (SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)), 0) FROM[TRN].[PurchaseOrderTax] WHERE InventoryReceiveId = @inventoryReceiveId AND InventoryServiceId<>'')
                        SELECT
                            --IM.Id
		                    PDA.Id
                           ,PDAD.Id As AcceptenceDetailId
                            ,IR.Id AS POID,IRD.Id AS PODetailsID
                        ,IRD.Id AS InventoryReceiveDetailId
                        , MGM.UserName AS MaterialGroupMasterName
                        , MM.Id MaterialMasterId
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
                        -- , ISNULL(IRD.AcceptanceRcvQty-PDAD.TransactionQty,0) AS GRNRcvQty
						  , ISNULL(PAD.AcptTransactionQty-PDAD.TransactionQty,0) AS GRNRcvQty
			          --  , ISNULL(IRD.AcceptanceRcvQty, 0) AS PreviousRcvQty
						 , ISNULL(PAD.AcptTransactionQty, 0) AS PreviousRcvQty
                        ,ISNULL(PDAD.TransactionQty, 0) AS TransactionQty
						--,ISNULL(IRD.AcceptanceRcvQty-PDAD.TransactionQty,0) Otherqty
						,ISNULL(PAD.AcptTransactionQty-PDAD.TransactionQty,0) Otherqty
                      --  ,(IRD.TransactionQty - IRD.AcceptanceRcvQty) As Balance
						 ,(IRD.TransactionQty - PAD.AcptTransactionQty) As Balance
                        , ISNULL(IRD.QtyStatus, 0) QtyStatus
                        , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM, PDAD.MaterialTranRate TransactionRate, CU.Code AS CurrencyName, IR.ToCurrencyRate
                           
                        ,PDAD.MaterialTranAmount AS TrnAmount
                        ,0 AS BaseTaxAmount
                        , 0 AS ChargesAmount
                        ,0 AS ServiceCharge
                        , 0 AS ServiceTax
                        , IRD.CountryId
                        ,'True' enableid
                        ,null POMaterialTaxList
                        , IR.InvoicingByAddress,IR.DeliveryByAddress
                        ,IRD.RequisitionId
	                    ,IRD.RequisitionDetailId
                        ,IRD.Description MaterialDetail
                         ,ISNULL(PDAD.TotalMaterialTranAmount,0) TotalMaterialTranAmount,ISNULL(PDAD.TaxAmount,0) TaxAmount,ISNULL(PDAD.ChargesTranAmount,0) ChargesTranAmount,ISNULL(PDAD.ChargesTaxTranAmount,0) ChargesTaxTranAmount,''TaxList
                       ,[Active]=CAST (CASE WHEN PDAD.Id IS NULL THEN 0 ELSE 1 END AS bit)
                        FROM TRN.PurchaseDocAcceptanceDetail PDAD
						LEFT JOIN (SELECT POId,PODetailId,Sum(TransactionQty) AcptTransactionQty FROM TRN.PurchaseDocAcceptanceDetail GROUP BY POId,PODetailId) PAD ON PAD.POId=PDAD.POId AND PAD.PODetailId=PDAD.PODetailId
	                    LEFT JOIN TRN.PurchaseOrderDetail AS IRD  ON PDAD.PODetailId=IRD.Id AND PDAD.POId=IRD.InventoryReceiveId
                        left JOIN MST.MaterialMaster AS MM ON PDAD.MaterialMasterId = MM.Id
                    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                    LEFT JOIN MST.MaterialMasterArticle AS ART ON IRD.ArticleId = ART.Id
                    LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId = FC.Id
                    LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId = SC.Id
                    LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId = TC.Id
                    LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId = FCV.Id
                    LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId = SCV.Id
                    LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId = TCV.Id
                    -- JOIN[TRN].[PurchaseOrderDetail] AS IRD ON IRD.InventoryMaterialId = IM.Id
                    LEFT JOIN[SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    LEFT JOIN[TRN].[PurchaseOrder] AS IR ON IRD.InventoryReceiveId= IR.Id
                    LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId= CU.Id
                    --LEFT join [trn].MaterialRequsitionDetails MRD on MRD.Id= IRD.RequisitionDetailId
                    LEFT JOIN TRN.PurchaseDocAcceptance PDA ON PDA.Id=PDAD.PurchaseDocAcceptanceId
                    WHERE PDA.Id='" + Id + "' AND ISNULL(PDAD.POId,'')<>'' AND ISNULL(PDAD.PODetailId,'')<>''";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetPurchaseDocAcceptanceTax(string Id)
        {
            try
            {
                var Sql = @"SELECT PT.Id,PT.PurchaseDocAcceptanceId,PT.PurchaseDocAcceptanceDetailId,PT.PODetailId,PT.TaxCategoryId
                        ,PT.[Percentage],PT.HSNCodeId,PT.TaxAmount,TC.UserName TaxCategory 
                        FROM trn.PurchaseDocAcceptanceTax PT
                        LEFT JOIN MST.TaxCategory TC ON TC.Id=PT.TaxCategoryId
                        WHERE PT.PurchaseDocAcceptanceId='" + Id + "' AND PT.PurchaseDocAcceptanceServiceId IS NULL";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetPurchaseDocAcceptanceServiceTax(string Id)
        {
            try
            {
                var Sql = @"SELECT PT.Id, PT.PurchaseDocAcceptanceId, PT.PurchaseDocAcceptanceDetailId, PT.PODetailId, PT.TaxCategoryId
                        , PT.[Percentage], PT.HSNCodeId,PT.TaxAmount, TC.UserName TaxCategory , TC.UserName, PDS.ServiceMasterId ,PT.PurchaseDocAcceptanceServiceId
                        FROM trn.PurchaseDocAcceptanceTax PT
						LEFT JOIN TRN.PurchaseDocAcceptanceService PDS ON PDS.Id=PurchaseDocAcceptanceServiceId
                        LEFT JOIN MST.TaxCategory TC ON TC.Id=PT.TaxCategoryId
                        WHERE PT.PurchaseDocAcceptanceId='" + Id + "' AND PT.PurchaseDocAcceptanceDetailId IS NULL";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetAcceptanceCharges()
        {
            try
            {

                var Sql = @"select LT.*, null HSNCodeId, null HSNCode  from [HKP].OverHeadType LT where LT.Type = '" + ChargesType.Acceptance.ToString() + "'";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public GridModel QueryOnlyPO(GridParameter parameters, string inveReveiveId)
        {

            try
            {
                parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
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
                            , MM.Id MaterialMasterId
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
                            , ISNULL(PAD.AcptTransactionQty,0) AS GRNRcvQty     
                              ,'' AS TransactionQty, ISNULL(PAD.AcptTransactionQty,0) Otherqty							 
							  ,(IRD.TransactionQty-PAD.AcptTransactionQty) As Balance
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
                             ,IRD.TransactionQty*IRD.TransactionRate AS TotalMaterialTranAmount
                            , 0 AS ToTalMaterialBooksCurrencyAmount
                           ,IR.InvoicingByAddress,IR.DeliveryByAddress
                           ,IRD.RequisitionId
						   ,IRD.RequisitionDetailId
                           --,MRD.MaterialDetail
                           ,null AS [check] ,IRD.Description MaterialDetail,0 As 'Active'
                         FROM TRN.PurchaseOrderDetail AS IRD
						--LEFT JOIN TRN.PurchaseOrderDetail AS IRD ON IRD.InventoryMaterialId=PM.Id
                         left JOIN MST.MaterialMaster AS MM ON IRD.InventoryMaterialId=MM.Id
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
						LEFT JOIN (SELECT POId,PODetailId,Sum(TransactionQty) AcptTransactionQty FROM TRN.PurchaseDocAcceptanceDetail GROUP BY POId,PODetailId) PAD ON PAD.POId=IRD.InventoryReceiveId AND PAD.PODetailId=IRD.Id
                        WHERE IRD.InventoryReceiveId=@inventoryReceiveId and IRD.QtyStatus=0 ";
                return _sqlRepository.GetDifferentGridData(parameters);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }

        }

        public GridModel GetGRNDetailData(GridParameter parameters, string inveReveiveId, string PurchaseDocAcceptanceId)
        {
            try
            {
                if (string.IsNullOrEmpty(PurchaseDocAcceptanceId) || PurchaseDocAcceptanceId == "undefined")
                {
                    parameters.CmdText = @"SELECT [Active]=CAST (CASE WHEN PACD.Id IS NULL THEN 0 ELSE 1 END AS bit),PACD.Id, POD.InventoryReceiveId POId,POD.Id PODetailId,MGM.UserName AS MaterialGroupMasterName,MM.Id MaterialMasterId
	                        ,MM.UserName,POD.MaterialStorageId,POD.BaseUOMId,POD.ArticleId,ART.StandardName,POD.FirstCharacteristicsId,FC.UserName AS FirstCharacteristics
	                        ,POD.FirstCharacteristicsValueId,FCV.UserName AS FirstCharacteristicsValue,POD.SecondCharacteristicsId,SC.UserName AS SecondCharacteristics
	                        ,POD.SecondCharacteristicsValueId,SCV.UserName AS SecondCharacteristicsValue,POD.ThirdCharacteristicsId,TC.UserName AS ThirdCharacteristics
	                        ,POD.ThirdCharacteristicsValueId,TCV.UserName AS ThirdCharacteristicsValue,0 AS BaseTaxAmount,0 AS TaxAmount,0 AS ChargesAmount
	                        ,0 AS ServiceCharge,0 AS ServiceTax,POD.CountryId,NULL POMaterialTaxList,POD.TransactionQty POQty,SUM(IRD.TransactionQty) AS GRNRcvQty,SUM(IRD.MaterialTranAmount) AS TotalGRNValue
	                        ,ISNULL(PACD.TransactionQty, 0) AS TransactionQty,ISNULL(PAD.AcptTransactionQty, 0) Otherqty
                            ,ISNULL(((SELECT Min(v) FROM (VALUES  (POD.TransactionQty), (SUM(IRD.TransactionQty))) AS value(v)) -(ISNULL(PAD.AcptTransactionQty, 0)+ PACD.TransactionQty)),0) AS Balance
	                        ,POD.TransactionAmount TotalPOValue,ISNULL(PAD.TotalAcptValue,0) TotalAcptValue,POD.TransactionRate,POD.TransactionRate MaterialTranRate,ISNULL(PACD.MaterialTranAmount,0) TrnAmount,ISNULL(PACD.TotalMaterialTranAmount,0)TotalMaterialTranAmount,0 AS ToTalMaterialBooksCurrencyAmount
							,POD.TransactionUoMId,TUoM.UserName AS TransactionUoM,CU.Code AS CurrencyName,PO.ToCurrencyRate
                        FROM TRN.PurchaseOrderDetail AS POD  
                        LEFT JOIN MST.MaterialMaster AS MM ON POD.InventoryMaterialId = MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON POD.ArticleId = ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON POD.FirstCharacteristicsId = FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON POD.SecondCharacteristicsId = SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON POD.ThirdCharacteristicsId = TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON POD.FirstCharacteristicsValueId = FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON POD.SecondCharacteristicsValueId = SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON POD.ThirdCharacteristicsValueId = TCV.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON POD.TransactionUoMId = TUoM.Id
						LEFT JOIN TRN.[InventoryReceiveDetail] AS IRD ON POD.InventoryReceiveId = IRD.POId AND POD.Id=IRD.PODetailsId
                        LEFT JOIN [TRN].[PurchaseOrder] AS PO ON POD.InventoryReceiveId = PO.Id                       
                        LEFT JOIN [SCS].[Currency] AS CU ON PO.CurrencyId = CU.Id
                        LEFT JOIN TRN.PurchaseDocAcceptanceDetail PACD ON PACD.POId = POD.InventoryReceiveId AND PACD.PODetailId = POD.Id  AND PurchaseDocAcceptanceId='" + PurchaseDocAcceptanceId + @"'
                        LEFT JOIN (SELECT POId,PODetailId,Sum(TransactionQty) AcptTransactionQty,SUM(TotalMaterialTranAmount) TotalAcptValue  FROM TRN.PurchaseDocAcceptanceDetail WHERE PurchaseDocAcceptanceId<>'" + PurchaseDocAcceptanceId + @"' GROUP BY POId,PODetailId) PAD ON PAD.POId = POD.InventoryReceiveId AND PAD.PODetailId = POD.Id
                        WHERE IRD.InventoryReceiveId " + inveReveiveId + @"
					    GROUP BY PACD.Id,POD.InventoryReceiveId,POD.Id,MGM.UserName,MM.Id,MM.UserName,POD.MaterialStorageId,POD.BaseUOMId,POD.ArticleId,ART.StandardName,POD.FirstCharacteristicsId
						,FC.UserName,POD.FirstCharacteristicsValueId,FCV.UserName,POD.SecondCharacteristicsId,SC.UserName,POD.SecondCharacteristicsValueId,SCV.UserName,POD.ThirdCharacteristicsId
	                    ,TC.UserName,POD.ThirdCharacteristicsValueId,TCV.UserName,POD.CountryId,POD.TransactionQty,PACD.TransactionQty,PAD.AcptTransactionQty,POD.TransactionQty,POD.TransactionAmount,PAD.TotalAcptValue,POD.TransactionRate,PACD.MaterialTranAmount
	                    ,PACD.TotalMaterialTranAmount,POD.TransactionUoMId,TUoM.UserName,CU.Code,PO.ToCurrencyRate";
                }
                else
                {
                    parameters.CmdText = @"SELECT [Active]=CAST (CASE WHEN PACD.Id IS NULL THEN 0 ELSE 1 END AS bit),PACD.Id, POD.InventoryReceiveId POId,POD.Id PODetailId,MGM.UserName AS MaterialGroupMasterName,MM.Id MaterialMasterId
	                        ,MM.UserName,POD.MaterialStorageId,POD.BaseUOMId,POD.ArticleId,ART.StandardName,POD.FirstCharacteristicsId,FC.UserName AS FirstCharacteristics
	                        ,POD.FirstCharacteristicsValueId,FCV.UserName AS FirstCharacteristicsValue,POD.SecondCharacteristicsId,SC.UserName AS SecondCharacteristics
	                        ,POD.SecondCharacteristicsValueId,SCV.UserName AS SecondCharacteristicsValue,POD.ThirdCharacteristicsId,TC.UserName AS ThirdCharacteristics
	                        ,POD.ThirdCharacteristicsValueId,TCV.UserName AS ThirdCharacteristicsValue,0 AS BaseTaxAmount,0 AS TaxAmount,0 AS ChargesAmount
	                        ,0 AS ServiceCharge,0 AS ServiceTax,POD.CountryId,NULL POMaterialTaxList,POD.TransactionQty POQty,SUM(IRD.TransactionQty) AS GRNRcvQty,SUM(IRD.MaterialTranAmount) AS TotalGRNValue
	                        ,ISNULL(PACD.TransactionQty, 0) AS TransactionQty,ISNULL(PAD.AcptTransactionQty, 0) Otherqty
                            ,ISNULL(((SELECT Min(v) FROM (VALUES  (POD.TransactionQty), (SUM(IRD.TransactionQty))) AS value(v)) -(ISNULL(PAD.AcptTransactionQty, 0)+ PACD.TransactionQty)),0) AS Balance
	                        ,POD.TransactionAmount TotalPOValue,ISNULL(PAD.TotalAcptValue,0) TotalAcptValue,POD.TransactionRate,POD.TransactionRate MaterialTranRate,ISNULL(PACD.MaterialTranAmount,0) TrnAmount,ISNULL(PACD.TotalMaterialTranAmount,0)TotalMaterialTranAmount,0 AS ToTalMaterialBooksCurrencyAmount
							,POD.TransactionUoMId,TUoM.UserName AS TransactionUoM,CU.Code AS CurrencyName,PO.ToCurrencyRate 
                        FROM TRN.PurchaseOrderDetail AS POD
                        LEFT JOIN MST.MaterialMaster AS MM ON POD.InventoryMaterialId = MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON POD.ArticleId = ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON POD.FirstCharacteristicsId = FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON POD.SecondCharacteristicsId = SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON POD.ThirdCharacteristicsId = TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON POD.FirstCharacteristicsValueId = FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON POD.SecondCharacteristicsValueId = SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON POD.ThirdCharacteristicsValueId = TCV.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON POD.TransactionUoMId = TUoM.Id
						LEFT JOIN TRN.[InventoryReceiveDetail] AS IRD ON POD.InventoryReceiveId = IRD.POId AND POD.Id=IRD.PODetailsId
                        LEFT JOIN [TRN].[PurchaseOrder] AS PO ON POD.InventoryReceiveId = PO.Id                       
                        LEFT JOIN [SCS].[Currency] AS CU ON PO.CurrencyId = CU.Id
                        LEFT JOIN TRN.PurchaseDocAcceptanceDetail PACD ON PACD.POId = POD.InventoryReceiveId AND PACD.PODetailId = POD.Id AND PurchaseDocAcceptanceId='" + PurchaseDocAcceptanceId + @"'
                        LEFT JOIN (SELECT POId,PODetailId,Sum(TransactionQty) AcptTransactionQty,SUM(TotalMaterialTranAmount) TotalAcptValue FROM TRN.PurchaseDocAcceptanceDetail WHERE PurchaseDocAcceptanceId<>'" + PurchaseDocAcceptanceId + @"' GROUP BY POId,PODetailId) PAD ON PAD.POId = POD.InventoryReceiveId AND PAD.PODetailId = POD.Id
                        WHERE IRD.InventoryReceiveId " + inveReveiveId + @"
					    GROUP BY PACD.Id,POD.InventoryReceiveId,POD.Id,MGM.UserName,MM.Id,MM.UserName,POD.MaterialStorageId,POD.BaseUOMId,POD.ArticleId,ART.StandardName,POD.FirstCharacteristicsId
						,FC.UserName,POD.FirstCharacteristicsValueId,FCV.UserName,POD.SecondCharacteristicsId,SC.UserName,POD.SecondCharacteristicsValueId,SCV.UserName,POD.ThirdCharacteristicsId
	,TC.UserName,POD.ThirdCharacteristicsValueId,TCV.UserName,POD.CountryId,POD.TransactionQty,PACD.TransactionQty,PAD.AcptTransactionQty,POD.TransactionQty,POD.TransactionAmount,PAD.TotalAcptValue,POD.TransactionRate,PACD.MaterialTranAmount
	,PACD.TotalMaterialTranAmount,POD.TransactionUoMId,TUoM.UserName,CU.Code,PO.ToCurrencyRate";
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

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(PurchaseDocAcceptance), out sID);
            return sID;
        }
        private string GetPKAccMap()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(PurchaseDocAcceptancePOMap), out sID);
            return sID;
        }

        public void InsertOrUpdateGraphNew(PurchaseDocAcceptance entity, IEnumerable<PurchaseDocAcceptanceDetailViewModel> PurchaseDocAcceptanceDetail
            //, IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptanceTax, IEnumerable<PurchaseDocAcceptanceChargesViewModel> AcceptancechargesList
            //, IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptancechargesTax
            //, IEnumerable<PurchaseDocAcceptanceService> purchaseDocAcceptanceService, IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptanceServiceTax
            )
        {
            var flag = false;

            try
            {
                _unitOfWork.BeginTransaction();

                flag = true;
                entity.Id = GetPK();
                base.Insert(entity);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var currentId1 = _purchaseDocAcceptanceDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceDetail]  WHERE PurchaseDocAcceptanceId ='{entity.Id}'").First();

                int currentId = _purchaseDocAcceptanceTax.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceTax] WHERE PurchaseDocAcceptanceId='" + entity.Id + "'").First();

                var servicecurrentId = _purchaseDocAcceptanceServiceService.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceService] WHERE PurchaseDocAcceptanceId='{entity.Id}'").First();

                var AcceptanceId = "";
                foreach (var itemDetail in PurchaseDocAcceptanceDetail)
                {
                    if (itemDetail.IsNotNull())
                    {
                        var poDetail = _poDetailRepository.Query(r => r.Id == itemDetail.PODetailsID).Select().FirstOrDefault();
                        if (poDetail == null)
                            throw new CustomException("PO Details Or Inventory Details not found!");

                        poDetail.AcceptanceRcvQty += itemDetail.TransactionQty;
                        if (poDetail.TransactionQty < poDetail.AcceptanceRcvQty)
                            throw new CustomException("Received Qty can not cross balance Qty.");
                        poDetail.AcceptanceRcvStatusQty = poDetail.BaseQty == poDetail.AcceptanceRcvQty;
                        AuditService.UpdatedLog(poDetail);
                        _poDetailRepository.Update(poDetail);

                        if (string.IsNullOrEmpty(itemDetail.Id))
                        {
                            var NewId = entity.Id + "-";

                            currentId1++;
                            AcceptanceId = NewId + currentId1;
                            var receiveDetail = new PurchaseDocAcceptanceDetail
                            {
                                Id = NewId + currentId1,
                                PurchaseDocAcceptanceId = entity.Id,//itemDetail.PurchaseDocAcceptanceId,
                                MaterialMasterId = itemDetail.MaterialMasterId,
                                ArticleId = itemDetail.ArticleId,
                                FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
                                FirstCharacteristicsValueId = itemDetail.FirstCharacteristicsValueId,
                                SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
                                SecondCharacteristicsValueId = itemDetail.SecondCharacteristicsValueId,
                                ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
                                ThirdCharacteristicsValueId = itemDetail.ThirdCharacteristicsValueId,
                                TransactionQty = itemDetail.TransactionQty,
                                TransactionUoMId = itemDetail.TransactionUoMId,
                                MaterialTranRate = itemDetail.TransactionRate,
                                MaterialTranAmount = itemDetail.TransactionQty * itemDetail.TransactionRate,
                                POId = itemDetail.POId,
                                PODetailId = itemDetail.PODetailsID,
                                TaxAmount = itemDetail.TaxAmount,
                                TotalMaterialTranAmount = itemDetail.TotalMaterialTranAmount,
                                ChargesTaxTranAmount = itemDetail.ChargesTaxTranAmount,
                                ChargesTranAmount = itemDetail.ChargesTranAmount,
                                AcceptanceRate = entity.AcceptanceRate

                            };

                            AuditService.AddedLog(receiveDetail);
                            _purchaseDocumentAcceptanceDetailService.InsertGraph(receiveDetail);

                            var POTaxList = _receiveTaxRepository.Query(r => r.InventoryReceiveDetailId == itemDetail.PODetailsID).Select().ToList();
                            if (POTaxList != null)
                            {

                                foreach (var item in POTaxList)
                                {

                                    currentId++;
                                    var docAcceptanceTax = new PurchaseDocAcceptanceTax
                                    {
                                        PurchaseDocAcceptanceDetailId = receiveDetail.Id,
                                        PurchaseDocAcceptanceId = entity.Id,
                                        PODetailId = itemDetail.PODetailsID,
                                        TaxAmount = item.TaxAmount,
                                        TaxCategoryId = item.TaxCategoryId,
                                        HSNCodeId = item.HSNCodeId,
                                        Percentage = item.Percentage,
                                        PurchaseDocAcceptanceServiceId = null,
                                        Id = "MT" + MakePK(entity.Id, currentId, 2)
                                    };
                                    AuditService.AddedLog(docAcceptanceTax);
                                    _purchaseDocAcceptanceTax.Insert(docAcceptanceTax);


                                }
                            }



                        }
                    }
                }

                var poId = PurchaseDocAcceptanceDetail.Select(r => r.POId).FirstOrDefault();

                    var POServiceList = _inventoryService.Query(r => r.InventoryReceiveId == poId).Select().ToList();
                    var purchaseDocAcceptanceServiceTax = _receiveTaxRepository.Query(r => r.InventoryReceiveId == poId && r.InventoryReceiveDetailId == null).Select().ToList();


                var AcceptdocMap = new PurchaseDocAcceptancePOMap
                {
                    Id = GetPKAccMap(),
                    PurchaseDocAcceptanceId = entity.Id,
                    POId = poId
                };

                AuditService.AddedLog(AcceptdocMap);
                _PurchaseDocAcceptancePOMapService.InsertGraph(AcceptdocMap);

                if (POServiceList != null)
                {
                    foreach (var item in POServiceList)
                    {
                        PurchaseDocAcceptanceService service = new PurchaseDocAcceptanceService();

                        servicecurrentId++;
                        service.Id = MakePK(entity.Id + 2, servicecurrentId, 2);
                        service.PurchaseDocAcceptanceId = entity.Id;
                        service.Amount = item.Amount;
                        service.TotalTaxAmount = item.TotalTaxAmount;
                        service.ServiceMasterId = item.ServiceMasterId;
                        service.State = "PO";
                        AuditService.AddedLog(service);
                        _purchaseDocAcceptanceServiceService.Insert(service);


                        if (purchaseDocAcceptanceServiceTax.IsNotNull())
                        {

                            foreach (var POserviceTax in purchaseDocAcceptanceServiceTax)
                            {
                                PurchaseDocAcceptanceTax purchaseDocAcceptanceTax = new PurchaseDocAcceptanceTax();

                                currentId++;
                                purchaseDocAcceptanceTax.Id = "ST" + MakePK(entity.Id, currentId, 2);
                                purchaseDocAcceptanceTax.PurchaseDocAcceptanceId = entity.Id;
                                purchaseDocAcceptanceTax.PurchaseDocAcceptanceDetailId = null;
                                purchaseDocAcceptanceTax.AcceptanceServiceId = service.Id;
                                purchaseDocAcceptanceTax.TaxCategoryId = POserviceTax.TaxCategoryId;
                                purchaseDocAcceptanceTax.HSNCodeId = POserviceTax.HSNCodeId;
                                purchaseDocAcceptanceTax.Percentage = POserviceTax.Percentage;
                                purchaseDocAcceptanceTax.TaxAmount = POserviceTax.TaxAmount;

                                AuditService.AddedLog(purchaseDocAcceptanceTax);
                                _purchaseDocAcceptanceTax.Insert(purchaseDocAcceptanceTax);

                            }
                        }


                    }
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public IEnumerable<object> GetIsAccepptanceFirstData(string masterId, string plantId)
        {
            try
            {
                var sql = @"SELECT IsAccepptanceFirst from dbo.PurchaseLC  Where id='" + masterId + @"' AND PlantId='" + plantId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void SaveMaterialTax(IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptanceTax, string PurchaseDocAcceptanceId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                int currentId = _purchaseDocAcceptanceTax.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceTax] WHERE PurchaseDocAcceptanceId='" + PurchaseDocAcceptanceId + "'").First();
                if (purchaseDocAcceptanceTax != null)
                {
                    foreach (var item in purchaseDocAcceptanceTax)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            currentId++;
                            var docAcceptanceTax = new PurchaseDocAcceptanceTax
                            {
                                PurchaseDocAcceptanceDetailId = item.PurchaseDocAcceptanceDetailId,
                                PurchaseDocAcceptanceId = item.PurchaseDocAcceptanceId,
                                PODetailId = item.PODetailId,
                                TaxAmount = item.TaxAmount,
                                TaxCategoryId = item.TaxCategoryId,
                                HSNCodeId = item.HSNCodeId,
                                Percentage = item.Percentage,
                                PurchaseDocAcceptanceServiceId = null,
                                Id = "MT" + MakePK(item.PurchaseDocAcceptanceId, currentId, 2)
                            };
                            AuditService.AddedLog(docAcceptanceTax);
                            _purchaseDocAcceptanceTax.Insert(docAcceptanceTax);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(item.Id))
                            {
                                var data = _purchaseDocAcceptanceTax.Find(item.Id);
                                data.Percentage = item.Percentage;
                                AuditService.UpdatedLog(data);
                                _purchaseDocAcceptanceTax.Update(data);
                            }
                        }
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public void SaveOrUpdateServiceTax(IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptanceServiceTax, string PurchaseDocAcceptanceId, string PurchaseDocAcceptanceServiceId)
        {
            var flag = false;
            decimal TotalTaxAmount = 0;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                int currentId = _purchaseDocAcceptanceTax.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceTax] WHERE PurchaseDocAcceptanceId='" + PurchaseDocAcceptanceId + "'").First();
                //var serviceId = _purchaseDocAcceptanceServiceService.Query(r => r.PurchaseDocAcceptanceId == PurchaseDocAcceptanceId).Select(r => r.Id).FirstOrDefault();

                var purchaseDocAcceptanceService = _purchaseDocAcceptanceServiceService.Find(PurchaseDocAcceptanceServiceId);
                if (purchaseDocAcceptanceServiceTax.IsNotNull())
                {

                    foreach (var item in purchaseDocAcceptanceServiceTax)
                    {


                        if (string.IsNullOrEmpty(item.Id))
                        {
                            currentId++;
                            var docAcceptanceTax = new PurchaseDocAcceptanceTax
                            {
                                PurchaseDocAcceptanceDetailId = item.PurchaseDocAcceptanceDetailId,
                                PurchaseDocAcceptanceId = PurchaseDocAcceptanceId,
                                TaxAmount = item.TaxAmount,
                                TaxCategoryId = item.TaxCategoryId,
                                HSNCodeId = item.HSNCodeId,
                                Percentage = item.Percentage,
                                PurchaseDocAcceptanceServiceId = item.PurchaseDocAcceptanceServiceId,
                                Id = MakePK(item.PurchaseDocAcceptanceServiceId, currentId, 2)
                            };
                            AuditService.AddedLog(docAcceptanceTax);
                            _purchaseDocAcceptanceTax.Insert(docAcceptanceTax);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(item.Id))
                            {
                                var data = _purchaseDocAcceptanceTax.Find(item.Id);
                                data.Percentage = item.Percentage;
                                data.TaxAmount = item.TaxAmount;
                                AuditService.UpdatedLog(data);
                                _purchaseDocAcceptanceTax.Update(data);
                            }
                        }

                        TotalTaxAmount += Convert.ToDecimal(item.TaxAmount);
                    }
                    if (purchaseDocAcceptanceService != null)
                    {
                        purchaseDocAcceptanceService.TotalTaxAmount = TotalTaxAmount;
                        AuditService.UpdatedLog(purchaseDocAcceptanceService);
                        _purchaseDocAcceptanceServiceService.Update(purchaseDocAcceptanceService);
                    }

                }



                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public void SaveServiceAndServiceTax(IEnumerable<PurchaseDocAcceptanceService> purchaseDocAcceptanceService, IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptanceServiceTax, string PurchaseDocAcceptanceId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                if (purchaseDocAcceptanceService == null || purchaseDocAcceptanceServiceTax == null)
                {
                    throw new Exception("Service and Tax is required.");
                }
                if (purchaseDocAcceptanceService != null)
                {
                    foreach (var purDocAccService in purchaseDocAcceptanceService)
                    {
                        var service = new PurchaseDocAcceptanceService
                        {

                            PurchaseDocAcceptanceId = PurchaseDocAcceptanceId,
                            ServiceMasterId = purDocAccService.ServiceMasterId,
                            Amount = purDocAccService.Amount,
                            TotalTaxAmount = purDocAccService.TotalTaxAmount,
                            State = purDocAccService.State
                        };

                        if (purDocAccService.Id == null)
                        {
                            //if (Convert.ToBoolean(_purchaseDocAcceptanceServiceService.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT * FROM TRN.PurchaseDocAcceptanceService WHERE PurchaseDocAcceptanceId='" + purDocAccService.PurchaseDocAcceptanceId + "' AND ServiceMasterId='" + purDocAccService.ServiceMasterId + "') AS A) SELECT 1 ELSE SELECT 0 RETURN").First()))
                            //    throw new CustomException("This service already taken.");

                            var currentId = _purchaseDocAcceptanceServiceService.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceService] WHERE PurchaseDocAcceptanceId='{purDocAccService.PurchaseDocAcceptanceId}'").First();
                            currentId++;
                            service.Id = MakePK(PurchaseDocAcceptanceId + 2, currentId, 2);
                            AuditService.AddedLog(service);
                            _purchaseDocAcceptanceServiceService.Insert(service);
                        }
                        else
                        {
                            if (purDocAccService.Id != null)
                            {
                                service.Id = purDocAccService.Id;
                                AuditService.UpdatedLog(service);
                                _purchaseDocAcceptanceServiceService.Update(service);
                            }
                        }

                        if (purchaseDocAcceptanceServiceTax.IsNotNull())
                        {
                            var crrId = _purchaseDocAcceptanceServiceService.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceTax] WHERE PurchaseDocAcceptanceServiceId='{service.Id}'").First();
                            foreach (var item in purchaseDocAcceptanceServiceTax)
                            {
                                if (item.PurchaseDocAcceptanceId == null)
                                {
                                    if (item.ServiceMasterId == service.ServiceMasterId)
                                    {
                                        crrId++;
                                        item.Id = MakePK(service.Id, crrId, 2);
                                        item.PurchaseDocAcceptanceId = PurchaseDocAcceptanceId;
                                        item.PurchaseDocAcceptanceDetailId = null;
                                        item.PurchaseDocAcceptanceServiceId = service.Id;
                                        AuditService.AddedLog(item);
                                        _purchaseDocAcceptanceTax.Insert(item);
                                    }

                                }
                                else
                                {
                                    AuditService.UpdatedLog(item);
                                    _purchaseDocAcceptanceTax.Update(item);
                                }
                            }
                        }
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public void SaveServiceChargesAndChargesTax(IEnumerable<PurchaseDocAcceptanceChargesViewModel> AcceptancechargesList, IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptancechargesTax, PurchaseDocAcceptance entity)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                if (AcceptancechargesList == null)
                {
                    throw new Exception("Service Charges is required.");
                }
                if (AcceptancechargesList != null)
                {

                    var AcceptanceChargesId = _purchaseDocAcceptanceChargesRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceCharges] WHERE PurchaseDocAcceptanceId='{entity.Id}'").First();

                    foreach (var ServiceitemDetail in AcceptancechargesList)
                    {
                        var PurchaseDoService = new PurchaseDocAcceptanceCharges
                        {

                            PurchaseDocAcceptanceId = entity.Id,
                            CurrencyId = ServiceitemDetail.CurrencyId,
                            OpeningBankMasterId = ServiceitemDetail.OpeningBankMasterId,
                            AcceptanceServiceId = ServiceitemDetail.AcceptanceServiceId,
                            Amount = Convert.ToDecimal(ServiceitemDetail.Amount),
                            PartyId = entity.PartyId,
                            PartyPlantId = entity.PartyPlantId,
                            BankAmount = Convert.ToDecimal(ServiceitemDetail.BankAmount),
                            Rate = 0,
                            TotalTaxAmount = Convert.ToDecimal(ServiceitemDetail.TotalTaxAmount),


                        };
                        if (ServiceitemDetail.Id == null)
                        {
                            AcceptanceChargesId++;
                            PurchaseDoService.Id = MakePK(PurchaseDoService.PurchaseDocAcceptanceId, AcceptanceChargesId, 2);
                            AuditService.AddedLog(PurchaseDoService);
                            _purchaseDocAcceptanceChargesService.InsertGraph(PurchaseDoService);
                        }
                        else
                        {
                            PurchaseDoService.Id = ServiceitemDetail.Id;
                            AuditService.UpdatedLog(PurchaseDoService);
                            _purchaseDocAcceptanceChargesService.Update(PurchaseDoService);
                        }

                        if (purchaseDocAcceptancechargesTax.IsNotNull())
                        {
                            var crrId = _purchaseDocAcceptanceServiceService.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceTax] WHERE PurchaseDocAcceptanceChargesId='{PurchaseDoService.Id}'").First();
                            foreach (var item in purchaseDocAcceptancechargesTax)
                            {
                                if (item.PurchaseDocAcceptanceId == null)
                                {
                                    if (item.AcceptanceServiceId == PurchaseDoService.AcceptanceServiceId)
                                    {
                                        crrId++;
                                        item.Id = "CT" + MakePK(PurchaseDoService.Id, crrId, 2);
                                        item.PurchaseDocAcceptanceId = entity.Id;
                                        item.PurchaseDocAcceptanceDetailId = null;
                                        item.PurchaseDocAcceptanceDetailId = null;
                                        item.PurchaseDocAcceptanceChargesId = PurchaseDoService.Id;
                                        AuditService.AddedLog(item);
                                        _purchaseDocAcceptanceTax.Insert(item);
                                    }

                                }
                                else
                                {
                                    if (item.AcceptanceServiceId == PurchaseDoService.AcceptanceServiceId)
                                    {
                                        AuditService.UpdatedLog(item);
                                        _purchaseDocAcceptanceTax.Update(item);
                                    }
                                }
                            }
                        }
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }


        public void InsertOrUpdate(PurchaseDocAcceptance entity, IEnumerable<PurchaseDocAcceptanceDetailViewModel> PurchaseDocAcceptanceDetail
            , IEnumerable<PurchaseDocAcceptanceDetailViewModel> PurchaseDocAcceptanceServiceDetail
            , IEnumerable<PurchaseDocAcceptanceService> purchaseDocAcceptanceService, IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptanceServiceTax
            )
        {
            var flag = false;

            try
            {
                _unitOfWork.BeginTransaction();

                flag = true;
                AuditService.UpdatedLog(entity);
                base.Update(entity);

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var currentId1 = _purchaseDocAcceptanceDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceDetail]  WHERE PurchaseDocAcceptanceId ='{entity.Id}'").First();


                int currentId = _purchaseDocAcceptanceTax.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceTax] WHERE PurchaseDocAcceptanceId='" + entity.Id + "'").First();

                var servicecurrentId = _purchaseDocAcceptanceServiceService.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceService] WHERE PurchaseDocAcceptanceId='{entity.Id}'").First();

                var AcceptanceId = "";
                if (PurchaseDocAcceptanceDetail != null)
                {
                    foreach (var itemDetail in PurchaseDocAcceptanceDetail)
                    {
                        if (itemDetail.IsNotNull())
                        {
                            var poDetail = _poDetailRepository.Query(r => r.Id == itemDetail.PODetailsID).Select().FirstOrDefault();
                            if (poDetail == null)
                                throw new CustomException("PO Details Or Inventory Details not found!");
                            poDetail.AcceptanceRcvQty = itemDetail.TransactionQty;
                            if (poDetail.TransactionQty < poDetail.GRNRcvQty)
                                throw new CustomException("Received Qty can not cross balance Qty.");
                            poDetail.AcceptanceRcvStatusQty = poDetail.TransactionQty == poDetail.AcceptanceRcvQty;
                            AuditService.UpdatedLog(poDetail);
                            _poDetailRepository.Update(poDetail);
                            // Insert in receive detail
                            if (!string.IsNullOrEmpty(itemDetail.Id))
                            {
                                var NewId = entity.Id + "-";
                                currentId1++;
                                AcceptanceId = NewId + currentId1;
                                var receiveDetail = new PurchaseDocAcceptanceDetail
                                {
                                    Id = itemDetail.AcceptenceDetailId,
                                    PurchaseDocAcceptanceId = entity.Id,//itemDetail.PurchaseDocAcceptanceId,
                                    MaterialMasterId = itemDetail.MaterialMasterId,
                                    ArticleId = itemDetail.ArticleId,
                                    FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
                                    FirstCharacteristicsValueId = itemDetail.FirstCharacteristicsValueId,
                                    SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
                                    SecondCharacteristicsValueId = itemDetail.SecondCharacteristicsValueId,
                                    ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
                                    ThirdCharacteristicsValueId = itemDetail.ThirdCharacteristicsValueId,
                                    TransactionQty = itemDetail.TransactionQty,
                                    TransactionUoMId = itemDetail.TransactionUoMId,
                                    MaterialTranRate = itemDetail.TransactionRate,
                                    MaterialTranAmount = itemDetail.TransactionQty * itemDetail.TransactionRate,
                                    POId = itemDetail.POId,
                                    PODetailId = itemDetail.PODetailsID,
                                    TotalMaterialTranAmount = itemDetail.TotalMaterialTranAmount,
                                    ChargesTranAmount = itemDetail.ChargesTranAmount,
                                    ChargesTaxTranAmount = itemDetail.ChargesTaxTranAmount,
                                    TaxAmount = itemDetail.TaxAmount,
                                    AcceptanceRate = entity.AcceptanceRate
                                };

                                AuditService.UpdatedLog(receiveDetail);
                                _purchaseDocumentAcceptanceDetailService.Update(receiveDetail);


                                var PurchaseDocAcceptancePOMap = _PurchaseDocAcceptancePOMapService.Query(r => r.PurchaseDocAcceptanceId == entity.Id && r.POId == itemDetail.POId).Select().FirstOrDefault();

                                if (!string.IsNullOrEmpty(PurchaseDocAcceptancePOMap.Id))
                                {
                                    var AcceptdocMap = new PurchaseDocAcceptancePOMap
                                    {
                                        Id = PurchaseDocAcceptancePOMap.Id,
                                        PurchaseDocAcceptanceId = entity.Id,
                                        POId = receiveDetail.POId
                                    };

                                    AuditService.AddedLog(AcceptdocMap);
                                    _PurchaseDocAcceptancePOMapService.UpdateGraph(AcceptdocMap);
                                }
                                else
                                {
                                    var AcceptdocMap = new PurchaseDocAcceptancePOMap
                                    {
                                        Id = GetPKAccMap(),
                                        PurchaseDocAcceptanceId = entity.Id,
                                        POId = receiveDetail.POId
                                    };

                                    AuditService.AddedLog(AcceptdocMap);
                                    _PurchaseDocAcceptancePOMapService.InsertGraph(AcceptdocMap);
                                }

                            }
                            else
                            {

                                if (itemDetail.IsNotNull())
                                {

                                    poDetail = _poDetailRepository.Query(r => r.Id == itemDetail.PODetailsID).Select().FirstOrDefault();
                                    if (poDetail == null)
                                        throw new CustomException("PO Details Or Inventory Details not found!");

                                    poDetail.AcceptanceRcvQty = itemDetail.TransactionQty;
                                    if (poDetail.TransactionQty < poDetail.AcceptanceRcvQty)
                                        throw new CustomException("Received Qty can not cross balance Qty.");
                                    poDetail.AcceptanceRcvStatusQty = poDetail.BaseQty == poDetail.AcceptanceRcvQty;
                                    AuditService.UpdatedLog(poDetail);
                                    _poDetailRepository.Update(poDetail);
                                    if (string.IsNullOrEmpty(itemDetail.Id))
                                    {
                                        var NewId = entity.Id + "-";

                                        currentId1++;
                                        AcceptanceId = NewId + currentId1;
                                        var receiveDetail = new PurchaseDocAcceptanceDetail
                                        {
                                            Id = NewId + currentId1,
                                            PurchaseDocAcceptanceId = entity.Id,//itemDetail.PurchaseDocAcceptanceId,
                                            MaterialMasterId = itemDetail.MaterialMasterId,
                                            ArticleId = itemDetail.ArticleId,
                                            FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
                                            FirstCharacteristicsValueId = itemDetail.FirstCharacteristicsValueId,
                                            SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
                                            SecondCharacteristicsValueId = itemDetail.SecondCharacteristicsValueId,
                                            ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
                                            ThirdCharacteristicsValueId = itemDetail.ThirdCharacteristicsValueId,
                                            TransactionQty = itemDetail.TransactionQty,
                                            TransactionUoMId = itemDetail.TransactionUoMId,
                                            MaterialTranRate = itemDetail.TransactionRate,
                                            MaterialTranAmount = itemDetail.TransactionQty * itemDetail.TransactionRate,
                                            POId = itemDetail.POId,
                                            PODetailId = itemDetail.PODetailsID,
                                            TaxAmount = itemDetail.TaxAmount,
                                            TotalMaterialTranAmount = itemDetail.TotalMaterialTranAmount,
                                            ChargesTaxTranAmount = itemDetail.ChargesTaxTranAmount,
                                            ChargesTranAmount = itemDetail.ChargesTranAmount,
                                            AcceptanceRate = entity.AcceptanceRate

                                        };

                                        AuditService.AddedLog(receiveDetail);
                                        _purchaseDocumentAcceptanceDetailService.InsertGraph(receiveDetail);

                                        var POTaxList = _receiveTaxRepository.Query(r => r.InventoryReceiveDetailId == itemDetail.PODetailsID).Select().ToList();
                                        if (POTaxList != null)
                                        {

                                            foreach (var item in POTaxList)
                                            {

                                                currentId++;
                                                var docAcceptanceTax = new PurchaseDocAcceptanceTax
                                                {
                                                    PurchaseDocAcceptanceDetailId = receiveDetail.Id,
                                                    PurchaseDocAcceptanceId = entity.Id,
                                                    PODetailId = itemDetail.PODetailsID,
                                                    TaxAmount = item.TaxAmount,
                                                    TaxCategoryId = item.TaxCategoryId,
                                                    HSNCodeId = item.HSNCodeId,
                                                    Percentage = item.Percentage,
                                                    PurchaseDocAcceptanceServiceId = null,
                                                    Id = "MT" + MakePK(entity.Id, currentId, 2)
                                                };
                                                AuditService.AddedLog(docAcceptanceTax);
                                                _purchaseDocAcceptanceTax.Insert(docAcceptanceTax);


                                            }
                                        }




                                    }
                                    //if (string.IsNullOrEmpty(itemDetail.Id))
                                    //{
                                    //    var NewId = entity.Id + "-";
                                    //    currentId1++;
                                    //    AcceptanceId = NewId + currentId1;
                                    //    var receiveDetail = new PurchaseDocAcceptanceDetail
                                    //    {
                                    //        Id = NewId + currentId1,
                                    //        PurchaseDocAcceptanceId = entity.Id,//itemDetail.PurchaseDocAcceptanceId,
                                    //        MaterialMasterId = itemDetail.MaterialMasterId,
                                    //        ArticleId = itemDetail.ArticleId,
                                    //        FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
                                    //        FirstCharacteristicsValueId = itemDetail.FirstCharacteristicsValueId,
                                    //        SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
                                    //        SecondCharacteristicsValueId = itemDetail.SecondCharacteristicsValueId,
                                    //        ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
                                    //        ThirdCharacteristicsValueId = itemDetail.ThirdCharacteristicsValueId,
                                    //        TransactionQty = itemDetail.TransactionQty,
                                    //        TransactionUoMId = itemDetail.TransactionUoMId,
                                    //        MaterialTranRate = itemDetail.TransactionRate,
                                    //        MaterialTranAmount = itemDetail.TransactionQty * itemDetail.TransactionRate,
                                    //        POId = itemDetail.POId,
                                    //        PODetailId = itemDetail.PODetailsID,
                                    //        AcceptanceRate = entity.AcceptanceRate

                                    //    };
                                    //    AuditService.AddedLog(receiveDetail);
                                    //    _purchaseDocumentAcceptanceDetailService.InsertGraph(receiveDetail);
                                    //    var currentId = 0;
                                    //    //foreach (var item in purchaseDocAcceptanceTax.Where(r => r.PODetailId == itemDetail.PODetailsID))
                                    //    //{
                                    //    //    if (!string.IsNullOrEmpty(item.Id))
                                    //    //    {
                                    //    //        var docAcceptanceTax = new PurchaseDocAcceptanceTax
                                    //    //        {
                                    //    //            PurchaseDocAcceptanceDetailId = receiveDetail.Id,
                                    //    //            PurchaseDocAcceptanceId = entity.Id,
                                    //    //            PODetailId = item.PODetailId,
                                    //    //            TaxAmount = item.TaxAmount,
                                    //    //            TaxCategoryId = item.TaxCategoryId,
                                    //    //            HSNCodeId = item.HSNCodeId,
                                    //    //            Percentage = item.Percentage,
                                    //    //            PurchaseDocAcceptanceServiceId = null,
                                    //    //            Id = item.Id
                                    //    //        };
                                    //    //        AuditService.UpdatedLog(docAcceptanceTax);
                                    //    //        _purchaseDocAcceptanceTax.Update(docAcceptanceTax);
                                    //    //    }
                                    //    //    else if (string.IsNullOrEmpty(item.Id))
                                    //    //    {
                                    //    //        currentId++;
                                    //    //        var docAcceptanceTax = new PurchaseDocAcceptanceTax
                                    //    //        {
                                    //    //            PurchaseDocAcceptanceDetailId = receiveDetail.Id,
                                    //    //            PurchaseDocAcceptanceId = entity.Id,
                                    //    //            PODetailId = item.PODetailId,
                                    //    //            TaxAmount = item.TaxAmount,
                                    //    //            TaxCategoryId = item.TaxCategoryId,
                                    //    //            HSNCodeId = item.HSNCodeId,
                                    //    //            Percentage = item.Percentage,
                                    //    //            PurchaseDocAcceptanceServiceId = null,
                                    //    //            Id = MakePK(receiveDetail.Id, currentId, 2)
                                    //    //        };
                                    //    //        AuditService.AddedLog(docAcceptanceTax);
                                    //    //        _purchaseDocAcceptanceTax.Insert(docAcceptanceTax);
                                    //    //    }
                                    //    //}
                                    //    var AcceptdocMap = new PurchaseDocAcceptancePOMap
                                    //    {
                                    //        Id = GetPKAccMap(),
                                    //        PurchaseDocAcceptanceId = entity.Id,
                                    //        POId = receiveDetail.POId
                                    //    };
                                    //    AuditService.AddedLog(AcceptdocMap);
                                    //    _PurchaseDocAcceptancePOMapService.InsertGraph(AcceptdocMap);
                                    //}
                                }
                            }
                        }
                    }
                }

                //var poId = PurchaseDocAcceptanceDetail.Select(r => r.POId).FirstOrDefault();
                //var POServiceList = _inventoryService.Query(r => r.InventoryReceiveId == poId).Select().ToList();
                //var _purchaseDocAcceptanceServiceTax = _receiveTaxRepository.Query(r => r.InventoryReceiveId == poId && r.InventoryReceiveDetailId == null).Select().ToList();

                //if (POServiceList != null)
                //{
                //    foreach (var item in POServiceList)
                //    {
                //        PurchaseDocAcceptanceService service = new PurchaseDocAcceptanceService();

                //        servicecurrentId++;
                //        service.Id = MakePK(entity.Id + 2, servicecurrentId, 2);
                //        service.PurchaseDocAcceptanceId = entity.Id;
                //        service.Amount = item.Amount;
                //        service.TotalTaxAmount = item.TotalTaxAmount;
                //        service.ServiceMasterId = item.ServiceMasterId;
                //        service.State = "PO";
                //        AuditService.AddedLog(service);
                //        _purchaseDocAcceptanceServiceService.Insert(service);


                //        if (_purchaseDocAcceptanceServiceTax.IsNotNull())
                //        {

                //            foreach (var POserviceTax in _purchaseDocAcceptanceServiceTax)
                //            {
                //                PurchaseDocAcceptanceTax purchaseDocAcceptanceTax = new PurchaseDocAcceptanceTax();

                //                currentId++;
                //                purchaseDocAcceptanceTax.Id = "ST" + MakePK(entity.Id, currentId, 2);
                //                purchaseDocAcceptanceTax.PurchaseDocAcceptanceId = entity.Id;
                //                purchaseDocAcceptanceTax.PurchaseDocAcceptanceDetailId = null;
                //                purchaseDocAcceptanceTax.AcceptanceServiceId = service.Id;
                //                purchaseDocAcceptanceTax.TaxCategoryId = POserviceTax.TaxCategoryId;
                //                purchaseDocAcceptanceTax.HSNCodeId = POserviceTax.HSNCodeId;
                //                purchaseDocAcceptanceTax.Percentage = POserviceTax.Percentage;
                //                purchaseDocAcceptanceTax.TaxAmount = POserviceTax.TaxAmount;

                //                AuditService.AddedLog(purchaseDocAcceptanceTax);
                //                _purchaseDocAcceptanceTax.Insert(purchaseDocAcceptanceTax);

                //            }
                //        }


                //    }
                //}


                if (PurchaseDocAcceptanceServiceDetail != null)
                {
                    foreach (var itemDetail in PurchaseDocAcceptanceServiceDetail)
                    {
                        if (itemDetail.IsNotNull())
                        {

                            // Insert in receive detail
                            if (!string.IsNullOrEmpty(itemDetail.Id))
                            {
                                var NewId = entity.Id + "-";
                                currentId1++;
                                AcceptanceId = NewId + currentId1;
                                var ServicePODetail = new PurchaseDocAcceptanceDetail
                                {
                                    Id = itemDetail.Id,
                                    PurchaseDocAcceptanceId = entity.Id,//itemDetail.PurchaseDocAcceptanceId,
                                    MaterialMasterId = itemDetail.MaterialMasterId,
                                    ArticleId = itemDetail.ArticleId,
                                    FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
                                    FirstCharacteristicsValueId = itemDetail.FirstCharacteristicsValueId,
                                    SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
                                    SecondCharacteristicsValueId = itemDetail.SecondCharacteristicsValueId,
                                    ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
                                    ThirdCharacteristicsValueId = itemDetail.ThirdCharacteristicsValueId,
                                    TransactionQty = itemDetail.TransactionQty,
                                    TransactionUoMId = itemDetail.TransactionUoMId,
                                    MaterialTranRate = itemDetail.TransactionRate,
                                    MaterialTranAmount = itemDetail.TransactionQty * itemDetail.TransactionRate,
                                    ServicePOMasterId = itemDetail.ServicePOMasterId,
                                    ServicePODetailId = itemDetail.ServicePODetailId,
                                    TotalMaterialTranAmount = itemDetail.TotalMaterialTranAmount,
                                    ChargesTranAmount = itemDetail.ChargesTranAmount,
                                    ChargesTaxTranAmount = itemDetail.ChargesTaxTranAmount,
                                    TaxAmount = itemDetail.TaxAmount,
                                    AcceptanceRate = entity.AcceptanceRate
                                };

                                AuditService.UpdatedLog(ServicePODetail);
                                _purchaseDocumentAcceptanceDetailService.Update(ServicePODetail);



                                var PurchaseDocAcceptancePOMap = _PurchaseDocAcceptancePOMapService.Query(r => r.PurchaseDocAcceptanceId == entity.Id && r.ServicePOMasterId == itemDetail.ServicePOMasterId).Select().FirstOrDefault();

                                if (!string.IsNullOrEmpty(PurchaseDocAcceptancePOMap.Id))
                                {
                                    var AcceptdocMap = new PurchaseDocAcceptancePOMap
                                    {
                                        Id = PurchaseDocAcceptancePOMap.Id,
                                        PurchaseDocAcceptanceId = entity.Id,
                                        ServicePOMasterId = ServicePODetail.ServicePOMasterId
                                    };

                                    AuditService.AddedLog(AcceptdocMap);
                                    _PurchaseDocAcceptancePOMapService.UpdateGraph(AcceptdocMap);
                                }
                                else
                                {
                                    var AcceptdocMap = new PurchaseDocAcceptancePOMap
                                    {
                                        Id = GetPKAccMap(),
                                        PurchaseDocAcceptanceId = entity.Id,
                                        ServicePOMasterId = ServicePODetail.ServicePOMasterId
                                    };

                                    AuditService.AddedLog(AcceptdocMap);
                                    _PurchaseDocAcceptancePOMapService.InsertGraph(AcceptdocMap);
                                }

                            }

                        }
                    }
                }

                //////
                if (purchaseDocAcceptanceService != null)
                {
                    foreach (var purDocAccService in purchaseDocAcceptanceService)
                    {
                        var service = new PurchaseDocAcceptanceService
                        {

                            PurchaseDocAcceptanceId = entity.Id,
                            ServiceMasterId = purDocAccService.ServiceMasterId,
                            Amount = purDocAccService.Amount,
                            TotalTaxAmount = purDocAccService.TotalTaxAmount
                        };

                        if (purDocAccService.Id == null)
                        {
                            if (Convert.ToBoolean(_purchaseDocAcceptanceServiceService.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT * FROM TRN.PurchaseDocAcceptanceService WHERE PurchaseDocAcceptanceId='" + purDocAccService.PurchaseDocAcceptanceId + "' AND ServiceMasterId='" + purDocAccService.ServiceMasterId + "') AS A) SELECT 1 ELSE SELECT 0 RETURN").First()))
                                throw new CustomException("This service already taken.");

                            var _currentId = _purchaseDocAcceptanceServiceService.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceService] WHERE PurchaseDocAcceptanceId='{purDocAccService.PurchaseDocAcceptanceId}'").First();
                            _currentId++;
                            service.Id = MakePK(entity.Id + 2, _currentId, 2);
                            AuditService.AddedLog(service);
                            _purchaseDocAcceptanceServiceService.Insert(service);
                        }
                        else
                        {
                            if (purDocAccService.Id != null)
                            {
                                service.Id = purDocAccService.Id;
                                AuditService.UpdatedLog(service);
                                _purchaseDocAcceptanceServiceService.Update(service);
                            }
                        }

                        if (purchaseDocAcceptanceServiceTax.IsNotNull())
                        {
                            var crrId = _purchaseDocAcceptanceServiceService.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceTax] WHERE PurchaseDocAcceptanceServiceId='{service.Id}'").First();
                            foreach (var item in purchaseDocAcceptanceServiceTax)
                            {
                                if (item.PurchaseDocAcceptanceId == null)
                                {
                                    if (item.ServiceMasterId == service.ServiceMasterId)
                                    {
                                        crrId++;
                                        item.Id = MakePK(service.Id, crrId, 2);
                                        item.PurchaseDocAcceptanceId = entity.Id;
                                        item.PurchaseDocAcceptanceDetailId = null;
                                        item.PurchaseDocAcceptanceServiceId = service.Id;
                                        AuditService.AddedLog(item);
                                        _purchaseDocAcceptanceTax.Insert(item);
                                    }

                                }
                                else
                                {
                                    AuditService.UpdatedLog(item);
                                    _purchaseDocAcceptanceTax.Update(item);
                                }
                            }
                        }
                    }
                }


                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public void InsertOrUpdatePurchaseDocAcceptanceService(PurchaseDocAcceptanceService entity, IEnumerable<PurchaseDocAcceptanceTax> taxCategoryList)
        {
            if (Convert.ToBoolean(_purchaseDocAcceptanceServiceService.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT * FROM TRN.PurchaseDocAcceptanceService WHERE PurchaseDocAcceptanceId='" + entity.PurchaseDocAcceptanceId + "' AND ServiceMasterId='" + entity.ServiceMasterId + "') AS A) SELECT 1 ELSE SELECT 0 RETURN").First()))
                throw new CustomException("This service already taken.");

            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                if (entity.IsNotNull())
                {

                    var currentId = _purchaseDocAcceptanceServiceService.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceService] WHERE PurchaseDocAcceptanceId='{entity.PurchaseDocAcceptanceId}'").First();
                    currentId++;
                    var service = new PurchaseDocAcceptanceService
                    {
                        Id = MakePK(entity.PurchaseDocAcceptanceId + 2, currentId, 2),
                        PurchaseDocAcceptanceId = entity.PurchaseDocAcceptanceId,
                        ServiceMasterId = entity.ServiceMasterId,
                        Amount = entity.Amount,
                        TotalTaxAmount = taxCategoryList.Sum(r => r.TaxAmount)//TODO
                    };
                    AuditService.AddedLog(service);
                    _purchaseDocAcceptanceServiceService.Insert(service);
                    if (taxCategoryList.IsNotNull())
                    {
                        var crrId = _purchaseDocAcceptanceServiceService.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceTax] WHERE PurchaseDocAcceptanceServiceId='{service.Id}'").First();
                        foreach (var item in taxCategoryList)
                        {
                            crrId++;
                            item.Id = MakePK(service.Id, crrId, 2);
                            item.PurchaseDocAcceptanceId = entity.PurchaseDocAcceptanceId;
                            item.PurchaseDocAcceptanceDetailId = null;
                            item.PurchaseDocAcceptanceServiceId = service.Id;
                            AuditService.AddedLog(item);

                            _purchaseDocAcceptanceTax.Insert(item);
                        }
                    }
                    //var isNonCreditable = _purchaseDocAcceptanceServiceService.Query(t => t.Id == service.InventoryReceiveId).Select(t => t.IsNonCreditable).FirstOrDefault();
                    //var ratio = GetChargesRatio(service.InventoryReceiveId, null, 0, service.Id, isNonCreditable ? (service.Amount + service.TotalTaxAmount) : service.Amount, isNonCreditable);
                    //var ratioServiceTax = GetChargesTaxRatio(service.InventoryReceiveId, null, 0, service.Id, isNonCreditable ? (service.TotalTaxAmount) : service.TotalTaxAmount, isNonCreditable);
                    //if (entity.CurrencyId != entity.BaseCurrencyId)
                    //    UpdateInventoryDetail(service, ratioServiceTax, ratio, Convert.ToDecimal(entity.ToCurrencyRate), entity.IsNonCreditable);
                    //else if (entity.CurrencyId == entity.BaseCurrencyId)
                    //    UpdateInventoryDetail(service, ratioServiceTax, ratio, 1, entity.IsNonCreditable);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public IEnumerable<object> GetPurchaseDocAcceptanceService(string purchaseDocAcceptanceId)
        {
            try
            {

                //var sql = @"SELECT A.Id
                //        , A.PurchaseDocAcceptanceId
                //        , A.ServiceMasterId
                //        , B.UserName AS ServiceMasterName
                //         ,A.Amount
                //        , A.TotalTaxAmount
                //        FROM [TRN].[PurchaseDocAcceptanceService] AS A 
                //        JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id 
                //        WHERE A.PurchaseDocAcceptanceId='" + purchaseDocAcceptanceId + "'";

                string sql = @"SELECT A.Id, A.PurchaseDocAcceptanceId
	                                , A.ServiceMasterId
	                                , B.UserName AS ServiceMasterName
	                                , POS.Amount As POAmount                           
	                                ,POT.TaxAmount As TotalTaxAmount
	                                ,null accServiceTaxList
	                                ,'True' enableid1
	                                ,0 GRNServiceAmount
	                                ,A.Amount
	                                ,0 AmountStatus
                                    ,A.[State]
	                                FROM TRN.PurchaseDocAcceptanceService AS A
	                                INNER JOIN[HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id
	                                LEFT JOIN (select PurchaseDocAcceptanceId, Sum(TaxAmount) as TaxAmount from TRN.PurchaseDocAcceptanceTax group by PurchaseDocAcceptanceId) AS POT on A.PurchaseDocAcceptanceId=POT.PurchaseDocAcceptanceId
	                                LEFT JOIN [TRN].[PurchaseDocAcceptancePOMap] APOM ON APOM.PurchaseDocAcceptanceId=A.PurchaseDocAcceptanceId
	                                LEFT JOIN [TRN].POService POS ON POS.InventoryReceiveId=APOM.POId
                           WHERE A.PurchaseDocAcceptanceId='" + purchaseDocAcceptanceId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetServiceTaxList(string serviceId)
        {
            try
            {
                var sql = @"SELECT A.Id,A.PurchaseDocAcceptanceServiceId, A.TaxCategoryId, TC.UserName AS TaxCategory, A.HSNCodeId, HN.Code AS HSNCode, A.[Percentage], A.TaxAmount,A.PurchaseDocAcceptanceId,A.PurchaseDocAcceptanceDetailId
                        FROM [TRN].[PurchaseDocAcceptanceTax] AS A JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                        LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
                        WHERE A.PurchaseDocAcceptanceServiceId='" + serviceId + @"' AND A.PurchaseDocAcceptanceDetailId IS NULL ORDER BY TC.[Sequence]";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public decimal GetChargesRatio(string receiveId, string detailId, decimal detailTotalAmnt, string serviceId, decimal svcTotalAmnt, bool isNonCreditable)
        {
            try
            {
                decimal svcAmount = 0;
                if (isNonCreditable)
                    svcAmount = _purchaseDocAcceptanceServiceService.SqlQuery<decimal>("SELECT ISNULL(SUM(Amount), 0) FROM TRN.PurchaseDocAcceptanceService WHERE PurchaseDocAcceptanceId='" + receiveId + "' AND ISNULL(Id, '')<>'" + serviceId + "'").First();//+ISNULL(SUM(TotalTaxAmount), 0)
                else
                    svcAmount = _purchaseDocAcceptanceServiceService.SqlQuery<decimal>("SELECT ISNULL(SUM(Amount), 0) FROM TRN.PurchaseDocAcceptanceService WHERE PurchaseDocAcceptanceId='" + receiveId + "' AND ISNULL(Id, '')<>'" + serviceId + "'").First();
                if (svcTotalAmnt > 0) svcAmount += svcTotalAmnt;
                else svcAmount -= svcTotalAmnt;

                var detailAmount = _purchaseDocAcceptanceServiceService.SqlQuery<decimal>("SELECT ISNULL(SUM(MaterialTranAmount), 1) FROM TRN.PurchaseDocAcceptanceDetail WHERE PurchaseDocAcceptanceId='" + receiveId + "' AND ISNULL(Id, '')<>'" + detailId + "'").First();
                if (detailTotalAmnt > 0)
                {
                    detailAmount += detailTotalAmnt;
                }

                else
                {
                    detailAmount -= detailTotalAmnt;
                    //detailAmount = 1;
                }

                return svcAmount == 0 && detailAmount == 0 ? 0 : (svcAmount / detailAmount);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public decimal GetChargesTaxRatio(string receiveId, string detailId, decimal detailTotalAmnt, string serviceId, decimal svcTotalAmnt, bool isNonCreditable)
        {
            try
            {
                decimal svcAmount = 0;
                if (isNonCreditable)
                    svcAmount = _purchaseDocAcceptanceServiceService.SqlQuery<decimal>("SELECT ISNULL(SUM(TotalTaxAmount), 0) FROM TRN.PurchaseDocAcceptanceService WHERE PurchaseDocAcceptanceId='" + receiveId + "' AND ISNULL(Id, '')<>'" + serviceId + "'").First();
                else
                    svcAmount = _purchaseDocAcceptanceServiceService.SqlQuery<decimal>("SELECT ISNULL(SUM(TotalTaxAmount), 0) FROM TRN.PurchaseDocAcceptanceService WHERE PurchaseDocAcceptanceId='" + receiveId + "' AND ISNULL(Id, '')<>'" + serviceId + "'").First();
                if (svcTotalAmnt > 0) svcAmount += svcTotalAmnt;
                else svcAmount -= svcTotalAmnt;

                var detailAmount = _purchaseDocAcceptanceServiceService.SqlQuery<decimal>("SELECT ISNULL(SUM(MaterialTranAmount), 1) FROM TRN.PurchaseDocAcceptanceDetail WHERE PurchaseDocAcceptanceId='" + receiveId + "' AND ISNULL(Id, '')<>'" + detailId + "'").First();
                if (detailTotalAmnt > 0)
                {
                    detailAmount += detailTotalAmnt;
                }

                else
                {
                    detailAmount -= detailTotalAmnt;
                    //detailAmount = 1;
                }

                return svcAmount == 0 && detailAmount == 0 ? 0 : (svcAmount / detailAmount);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetAcceptanceList(string plantId)
        {
            try
            {
                var sql = @"SELECT PDA.Id, PDA.CompanyGroupId, PDA.CompanyId, PDA.PlantId, PDA.AcceptanceNo, FORMAT(PDA.EntryDate,'dd-MMM-yyyy') EntryDate, PDA.AddedBy, FORMAT(PDA.AddedDate,'dd-MMM-yyyy') AddedDate, PDA.AddedFromIP, PDA.UpdatedBy, FORMAT(PDA.UpdatedDate,'dd-MMM-yyyy') UpdatedFromIP, FORMAT(PDA.AcceptanceDate,'dd-MMM-yyyy')AcceptanceDate, PDA.POId, PDA.CheckedBy, PDA.CheckedByStatus, PDA.AuthorizedBy, PDA.AuthorizedByStatus, PDA.Remarks, PDA.PurchaseLCId, PDA.AcceptancePaymentSource, FORMAT(PDA.DueDate,'dd-MMM-yyyy') DueDate, FORMAT(PDA.InvoiceDate,'dd-MMM-yyyy') InvoiceDate, PDA.VoucherId, PDA.PartyId, PDA.PartyPlantId, PDA.AcceptanceRate, PDA.IsNonCreditable, PDA.InvoiceNo, PDA.PrePurchaseInvoiceId, PDA.ServiceVoucherId, PDA.AcceptanceAmount
                ,V.VoucherNo,C.Code CurrencyName,PLC.CurrencyId, P.UserName Party,PLC.ContractId
                ,PLC.Tenure,PLC.OpeningBankMasterId,BM.CurrencyId LCOBCurrencyId,BMC.Code OBCurrencyCode
                ,NonCreditable =case when PDA.IsNonCreditable=1 then 'Yes' else 'No' end
                ,PLC.LCRef,CN.ContractNo,ISNULL(CN.UDNo,'') UDNo,ISNULL(MLC.LCRef,'') MasterLCRef,AcceptanceFirst =case when PLC.IsAccepptanceFirst=1 then 'Yes' else 'No' end,PCN.UserName CustomerName,PLC.Amount LCAmount
                ,PDAD.MaterialTranAmount TotalAcptAmount, PDAD.CurrentQty,[Status]=CASE WHEN PDA.VoucherId IS NULL THEN 'Parked' ELSE 'Posted' END
                FROM TRN.PurchasedocAcceptance AS PDA
                LEFT JOIN(
                SELECT SUM(ISNULL(MaterialTranAmount,0)) MaterialTranAmount,SUM(ISNULL(TransactionQty,0)) CurrentQty,PurchaseDocAcceptanceId 
                FROM  TRN.PurchasedocAcceptanceDetail
                GROUP BY PurchaseDocAcceptanceId
                ) AS PDAD ON PDAD.PurchaseDocAcceptanceId=PDA.Id
                LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=PDA.PurchaseLCId
                LEFT JOIN SCS.Currency C ON C.Id=PLC.CurrencyId
                LEFT JOIN HKP.Party P ON P.Id=PDA.PartyId
                LEFT JOIN TRN.Voucher V ON V.Id=PDA.VoucherId
                LEFT JOIN [MST].[BankMaster] BM ON BM.Id=PLC.OpeningBankMasterId
                LEFT JOIN [SCS].[Currency] BMC ON BMC.Id=BM.CurrencyId
                LEFT JOIN dbo.[Contract] CN ON CN.Id=PLC.ContractId
                LEFT JOIN HKP.Party PCN ON PCN.Id=CN.CustomerId
                LEFT JOIN dbo.[MasterLC] MLC ON MLC.Id=CN.MasterLCId
                WHERE PDA.PlantId='" + plantId + @"' ORDER BY PDA.AddedDate DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetAcceptanceDetailList(string plantId, string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = @"SELECT
	                    --PDACD.Username As EntityName
	                    --MRM.EntityId
	                    --,Bu.Code
	                    --,Bu.UserName
	                    --,Us.FullName AddedBy
	                    --,MRM.Id RequisitionNo
	                    PDACD.ArticleId
	                    --,Dp.UserName DepartmentName
	                    ,MGM.UserName MaterialMasterGroupName
	                    ,mm.UserName MaterialMasterName
	                    ,ART.StandardName
	                    --,MT.UserName MaterialType
	                    ,PDACD.FirstCharacteristicsId
	                    ,FC.UserName AS FirstCharacteristics
	                    ,PDACD.FirstCharacteristicsValueId
	                    ,FCV.UserName AS FirstCharacteristicsValue
	                    ,PDACD.SecondCharacteristicsId
	                    ,SC.UserName AS SecondCharacteristics
	                    ,PDACD.SecondCharacteristicsValueId
	                    ,SCV.UserName AS SecondCharacteristicsValue
	                    ,PDACD.ThirdCharacteristicsId
	                    ,TC.UserName AS ThirdCharacteristics
	                    ,PDACD.ThirdCharacteristicsValueId
	                    ,TCV.UserName AS ThirdCharacteristicsValue
                    FROM [TRN].[PurchaseDocAcceptanceDetail] PDACD
                    LEFT JOIN MST.MaterialMaster AS MM ON PDACD.MaterialMasterId = MM.Id
                    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                    LEFT JOIN MST.MaterialMasterArticle AS ART ON PDACD.ArticleId = ART.Id
                    LEFT JOIN HKP.Characteristics AS FC ON PDACD.FirstCharacteristicsId = FC.Id
                    LEFT JOIN HKP.Characteristics AS SC ON PDACD.SecondCharacteristicsId = SC.Id
                    LEFT JOIN HKP.Characteristics AS TC ON PDACD.ThirdCharacteristicsId = TC.Id
                    LEFT JOIN HKP.CharacteristicsValue AS FCV ON PDACD.FirstCharacteristicsValueId = FCV.Id
                    LEFT JOIN HKP.CharacteristicsValue AS SCV ON PDACD.SecondCharacteristicsValueId = SCV.Id
                    LEFT JOIN HKP.CharacteristicsValue AS TCV ON PDACD.ThirdCharacteristicsValueId = TCV.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON PDACD.TransactionUoMId = TUoM.Id
                    LEFT JOIN [TRN].[PurchaseDocAcceptance] AS PDA ON PDACD.PurchaseDocAcceptanceId = PDA.Id
                    WHERE PDA.Id ='" + Id + @"' AND PlantId ='" + plantId + @"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetMaterialById(string Id, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = @"SELECT PDA.Id As AcceptenceId,PDACD.Id AS RowId,  PDACD.ArticleId ,MGM.UserName MaterialMasterGroup
	                    ,mm.UserName MaterialMaster,ART.StandardName Article,PDACD.FirstCharacteristicsId
	                    ,PDACD.FirstCharacteristicsValueId ,FCV.UserName AS SKU1
	                    ,PDACD.SecondCharacteristicsId ,PDACD.SecondCharacteristicsValueId
	                    ,SCV.UserName AS SKU2 ,PDACD.ThirdCharacteristicsId,PDACD.ThirdCharacteristicsValueId,TCV.UserName AS SKU3
                        ,TUoM.UserName AS TransactionUoM,PDACD.MaterialTranRate Rate,PDACD.TransactionQty,PDACD.MaterialTranAmount Amount
                    FROM [TRN].[PurchaseDocAcceptanceDetail] PDACD
                    LEFT JOIN MST.MaterialMaster AS MM ON PDACD.MaterialMasterId = MM.Id
                    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                    LEFT JOIN MST.MaterialMasterArticle AS ART ON PDACD.ArticleId = ART.Id
                    LEFT JOIN HKP.Characteristics AS FC ON PDACD.FirstCharacteristicsId = FC.Id
                    LEFT JOIN HKP.Characteristics AS SC ON PDACD.SecondCharacteristicsId = SC.Id
                    LEFT JOIN HKP.Characteristics AS TC ON PDACD.ThirdCharacteristicsId = TC.Id
                    LEFT JOIN HKP.CharacteristicsValue AS FCV ON PDACD.FirstCharacteristicsValueId = FCV.Id
                    LEFT JOIN HKP.CharacteristicsValue AS SCV ON PDACD.SecondCharacteristicsValueId = SCV.Id
                    LEFT JOIN HKP.CharacteristicsValue AS TCV ON PDACD.ThirdCharacteristicsValueId = TCV.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON PDACD.TransactionUoMId = TUoM.Id
                    LEFT JOIN [TRN].[PurchaseDocAcceptance] AS PDA ON PDACD.PurchaseDocAcceptanceId = PDA.Id
                    WHERE PDA.PlantId ='" + plantId + @"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetAcceptanceServiceList(string plantId, string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = @"SELECT 
                           PDAS.Id
                          ,PurchaseDocAcceptanceId
                          ,PDAS.AcceptanceServiceId
                          ,LCT.UserName ChargeName
                          ,PDAS.Amount
                          ,PDAS.TotalTaxAmount
                          ,PDAS.AddedBy
                          ,PDAS.AddedDate
                          ,PDAS.AddedFromIP
                          ,PDAS.UpdatedBy
                          ,PDAS.UpdatedDate
                          ,PDAS.UpdatedFromIP
						  ,PDAS.CurrencyId
						  ,PDAS.OpeningBankMasterId
						  ,PDAS.BankAmount
						  ,C.Id BankCurrencyId,OB.AccountTitle OpeningBankMaster,C.Code CurrencyName,PDAS.VoucherId
						  ,OB.CurrencyId LCOBCurrencyId,C.Code OBCurrencyCode, PDAS.TotalTaxAmount
                        FROM [TRN].[PurchaseDocAcceptanceCharges] PDAS
  	                    LEFT JOIN [HKP].OverHeadType AS LCT ON PDAS.AcceptanceServiceId=LCT.Id
  	                    LEFT JOIN trn.PurchasedocAcceptance AS PDC ON PDAS.PurchaseDocAcceptanceId=PDC.Id
						LEFT JOIN MST.BankMaster OB ON OB.Id=PDAS.OpeningBankMasterId
						LEFT JOIN SCS.Currency C ON C.Id=OB.CurrencyId
	                    Where  PDAS.PurchaseDocAcceptanceId='" + Id + @"'";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetAcceptanceChargesTaxList(string plantId, string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = @"SELECT A.Id,A.PurchaseDocAcceptanceServiceId, A.TaxCategoryId, TC.UserName, A.HSNCodeId, HN.Code AS HSNCode
                        , A.[Percentage], A.TaxAmount,A.PurchaseDocAcceptanceId,A.PurchaseDocAcceptanceDetailId
						,A.PurchaseDocAcceptanceChargesId,PDAC.AcceptanceServiceId
                        FROM [TRN].[PurchaseDocAcceptanceTax] AS A 
						JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                        LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
						LEFT JOIN TRN.PurchaseDocAcceptanceCharges PDAC ON PDAC.Id=A.PurchaseDocAcceptanceChargesId
                        WHERE A.PurchaseDocAcceptanceId='" + Id + @"' 
						AND A.PurchaseDocAcceptanceChargesId<>'' ORDER BY TC.[Sequence]";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public void Delete(string id, string POID, string PODetailsID, decimal Qty)
        {
            try
            {

                //var detail = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.InventoryReceiveDetail WHERE InventoryReceiveId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                //var service = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.InventoryService WHERE InventoryReceiveId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                //if (!detail && !service)
                //{
                //    var data = base.Find(id);
                //    if (data.IsNull()) throw new CustomException(ServiceResources.RecordNoLonger);
                //    base.Delete(data);
                //}
                //else throw new CustomException("Please delete first line item.");

                var ResStatus = "";
                var getPOdetails = _poDetailRepository.SqlQuery<decimal>(@"select AcceptanceRcvQty from trn.PurchaseOrderDetail where InventoryReceiveId='" + POID + "' and id='" + PODetailsID + "'").First();
                decimal qty = Convert.ToDecimal(Qty);
                var res = Convert.ToDecimal(getPOdetails) - Convert.ToDecimal(Qty);
                if (Convert.ToDecimal(res) == 0.00m)
                {
                    ResStatus = "1";
                }

                else
                {
                    ResStatus = "0";
                }
                var sql1 = @"Update trn.PurchaseOrderDetail set AcceptanceRcvQty='" + res + "',AcceptanceRcvStatusQty='" + ResStatus + "' where InventoryReceiveId='" + POID + "' and id='" + PODetailsID + "'";
                _sqlRepository.GetDataCollection(sql1);
                var sql2 = @"delete from trn.PurchaseDocAcceptanceTax where PurchaseDocAcceptanceDetailId='" + id + "'";
                _sqlRepository.GetDataCollection(sql2);
                var sql = @"delete from trn.PurchasedocAcceptanceDetail where id='" + id + "'";
                _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> LCDetails(string plantId, string LCID)
        {
            try
            {
                var Sql = @"SELECT 
                     REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') AS LCOpeningDate
                    , REPLACE(CONVERT(CHAR(11), PLC.ExpiryDate, 106),' ','-') AS LCExpiryDate
                    ,BM.AccountTitle LCOpeningBank
                    ,PLC.ContractId
                    ,PLC.Id PurchaseLCNO
                    ,p.UserName PartyName, p.Id PartyId, PP.UserName PartyPlant, PP.Id PartyPlantId, PLC.OpeningBankMasterId, PLC.LCRef, PLC.CurrencyId
                   -- , PO.Id
					, CN.Code CurrencyName, PLC.Tenure,AcceptanceFirst=CASE WHEN PLC.IsAccepptanceFirst=1 THEN 'Yes' ELSE 'No' END
                    ,PCN.UserName CustomerName,CNT.ContractNo,CNT.UDNo,MLC.LCRef MasterLCRef,PLC.Amount LCAmount
                    FROM dbo.PurchaseLC PLC
                    LEFT JOIN [MST].[BankMaster] BM ON BM.Id=PLC.OpeningBankMasterId
                    LEFT JOIN hkp.Party p ON p.id = PLC.VendorId
                    JOIN HKP.PartyPlant PP ON PP.PartyId=P.Id
					JOIN (select distinct PurchaseLCId from TRN.PurchaseOrder )PO ON PO.PurchaseLCId=PLC.Id
                    JOIN SCS.Currency CN ON CN.Id=PLC.CurrencyId
                    LEFT JOIN dbo.[Contract] CNT ON CNT.Id=PLC.ContractId
                    LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CNT.MasterLCId
					LEFT JOIN HKP.Party PCN ON PCN.Id=CNT.CustomerId
                    Where PLC.Id='" + LCID + "'";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public void DeleteACPOmapTabledata(string id, string POID, string PODetailsID, string Qty)
        {
            try
            {
                var sql = @"delete from TRN.PurchaseDocAcceptancePOMap where PurchaseDocAcceptanceId='" + id + "' AND POId='" + POID + "'";
                _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)='" + receiveId + @"'
                                  , @partyState varchar(30)
                                  , @partyCountry varchar(10)
                                  , @plantState varchar(30)
                                  , @plantCountry varchar(10)
                                  , @plantId varchar(30)='" + plantId + @"'
                                  , @hsnCodeId varchar(30)='" + hsnCodeId + @"'
                   SET @partyCountry =(SELECT AM.CountryId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id WHERE PP.PartyId=@receiveId)

                    SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id WHERE PP.PartyId=@receiveId)

                    SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SELECT TVD.Id, TVD.TaxCategoryId, HP.HSNCodeId, HN.Code AS HSNCode, TC.UserName, ISNULL(HP.[Percentage], 0) AS [Percentage], NULL TotalAmount, NULL ServiceMasterId
                    FROM [MST].[TaxVariantDetail] AS TVD
                    JOIN [MST].[TaxVariant] AS TV ON TVD.TaxVariantId=TV.Id
                    JOIN [MST].[TaxCategory] AS TC ON TVD.TaxCategoryId=TC.Id
                    --LEFT JOIN (SELECT * FROM [MST].[HSNTaxPercentage] WHERE HSNCodeId=@hsnCodeId) AS HP ON HP.TaxCategoryId=TC.Id
					LEFT JOIN (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY TaxCategoryId, HSNCodeId ORDER BY EffectiveDate DESC) AS RN
								FROM [MST].[HSNTaxPercentage] WHERE CountryId=@plantCountry AND HSNCodeId=@hsnCodeId) AS TBL WHERE RN=1) AS HP ON HP.TaxCategoryId=TC.Id

                    LEFT JOIN [HKP].[HSNCode] AS HN ON HP.HSNCodeId=HN.Id
                    WHERE TV.CompanyGroupId='" + companyGroupId + @"' AND TV.CountryId=@plantCountry --AND HP.HSNCodeId=@hsnCodeId
                    AND TV.TaxFor=CASE WHEN @partyCountry=@plantCountry THEN '" + TaxFor.DomesticPurchase + @"'
				                        WHEN @partyCountry<>@plantCountry THEN '" + TaxFor.OverseasPurchase + @"' END
                    AND (TV.Different=CASE WHEN @partyCountry=@plantCountry AND @partyState=@plantState AND TV.DifferentIn='State' THEN 'Same'
					                       WHEN @partyCountry=@plantCountry AND @partyState<>@plantState AND TV.DifferentIn='State' THEN 'Different' END
	                    OR TV.Different IS NULL)
                    ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetOtherAcptQtyValue(string POId, string PurchaseDocAcceptanceId)
        {
            try
            {
                var Sql = @"SELECT SUM(ISNULL(D.MaterialTranAmount,0)) OtherTotalAcptValue,SUM(D.TransactionQty) OtherTotalQty 
                            FROM TRN.PurchasedocAcceptanceDetail D LEFT JOIN TRN.PurchaseOrder P ON P.Id=D.POId WHERE D.POId " + POId + " AND D.PurchaseDocAcceptanceId<>'" + PurchaseDocAcceptanceId + "' GROUP BY D.POId";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        #region ServicePO Acceptance

        public void InsertOrUpdateServicePOAcceptance(PurchaseDocAcceptance entity, IEnumerable<PurchaseDocAcceptanceDetailViewModel> PurchaseDocAcceptanceDetail)
        {
            var flag = false;

            try
            {
                _unitOfWork.BeginTransaction();

                flag = true;
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = GetPK();
                    base.Insert(entity);
                }
                else
                {
                    base.Update(entity);
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var currentId1 = _purchaseDocAcceptanceDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceDetail]  WHERE PurchaseDocAcceptanceId ='{entity.Id}'").First();

                int currentId = _purchaseDocAcceptanceTax.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceTax] WHERE PurchaseDocAcceptanceId='" + entity.Id + "'").First();

                var servicecurrentId = _purchaseDocAcceptanceServiceService.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceService] WHERE PurchaseDocAcceptanceId='{entity.Id}'").First();

                var AcceptanceId = "";
                foreach (var itemDetail in PurchaseDocAcceptanceDetail)
                {
                    if (itemDetail.IsNotNull())
                    {
                        //var poDetail = _poDetailRepository.Query(r => r.Id == itemDetail.PODetailsID).Select().FirstOrDefault();
                        //if (poDetail == null)
                        //    throw new CustomException("PO Details Or Inventory Details not found!");

                        //poDetail.AcceptanceRcvQty += itemDetail.TransactionQty;
                        //if (poDetail.TransactionQty < poDetail.AcceptanceRcvQty)
                        //    throw new CustomException("Received Qty can not cross balance Qty.");
                        //poDetail.AcceptanceRcvStatusQty = poDetail.BaseQty == poDetail.AcceptanceRcvQty;
                        //AuditService.UpdatedLog(poDetail);
                        //_poDetailRepository.Update(poDetail);

                        if (string.IsNullOrEmpty(itemDetail.Id))
                        {
                            var NewId = entity.Id + "-";

                            currentId1++;
                            AcceptanceId = NewId + currentId1;
                            var receiveDetail = new PurchaseDocAcceptanceDetail
                            {
                                Id = NewId + currentId1,
                                PurchaseDocAcceptanceId = entity.Id,//itemDetail.PurchaseDocAcceptanceId,
                                MaterialMasterId = itemDetail.MaterialMasterId,
                                ArticleId = itemDetail.ArticleId,
                                FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
                                FirstCharacteristicsValueId = itemDetail.FirstCharacteristicsValueId,
                                SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
                                SecondCharacteristicsValueId = itemDetail.SecondCharacteristicsValueId,
                                ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
                                ThirdCharacteristicsValueId = itemDetail.ThirdCharacteristicsValueId,
                                TransactionQty = itemDetail.TransactionQty,
                                TransactionUoMId = itemDetail.TransactionUoMId,
                                MaterialTranRate = itemDetail.TransactionRate,
                                MaterialTranAmount = itemDetail.TransactionQty * itemDetail.TransactionRate,
                                ServicePOMasterId = itemDetail.ServicePOMasterId,
                                ServicePODetailId = itemDetail.ServicePODetailId,
                                TaxAmount = itemDetail.TaxAmount,
                                TotalMaterialTranAmount = itemDetail.TotalMaterialTranAmount,
                                ChargesTaxTranAmount = itemDetail.ChargesTaxTranAmount,
                                ChargesTranAmount = itemDetail.ChargesTranAmount,
                                AcceptanceRate = entity.AcceptanceRate

                            };

                            AuditService.AddedLog(receiveDetail);
                            _purchaseDocumentAcceptanceDetailService.InsertGraph(receiveDetail);

                            var POTaxList = _ServicePOTaxRepository.Query(r => r.ServicePODetailId == itemDetail.ServicePODetailId).Select().ToList();
                            if (POTaxList != null)
                            {

                                foreach (var item in POTaxList)
                                {

                                    currentId++;
                                    var docAcceptanceTax = new PurchaseDocAcceptanceTax
                                    {
                                        PurchaseDocAcceptanceDetailId = receiveDetail.Id,
                                        PurchaseDocAcceptanceId = entity.Id,
                                        ServicePODetailId = itemDetail.ServicePODetailId,
                                        TaxAmount = item.TaxAmount,
                                        TaxCategoryId = item.TaxCategoryId,
                                        HSNCodeId = item.HSNCodeId,
                                        Percentage = item.Percentage,
                                        PurchaseDocAcceptanceServiceId = null,
                                        Id = "SP" + MakePK(entity.Id, currentId, 2)
                                    };
                                    AuditService.AddedLog(docAcceptanceTax);
                                    _purchaseDocAcceptanceTax.Insert(docAcceptanceTax);


                                }
                            }


                            var AcceptdocMap = new PurchaseDocAcceptancePOMap
                            {
                                Id = GetPKAccMap(),
                                PurchaseDocAcceptanceId = entity.Id,
                                ServicePOMasterId = receiveDetail.ServicePOMasterId
                            };

                            AuditService.AddedLog(AcceptdocMap);
                            _PurchaseDocAcceptancePOMapService.InsertGraph(AcceptdocMap);
                        }
                    }
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }


        #endregion

        #endregion

        #region Document Acceptance Post

        public IEnumerable<object> GetAcceptanceServiceListForPost(string plantId, string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = @"SELECT 
                           PDAS.Id
                          ,PurchaseDocAcceptanceId
                          ,PDAS.AcceptanceServiceId
                          ,LCT.UserName ChargeName
                          ,PDAS.Amount
                          ,PDAS.TotalTaxAmount
                          ,PDAS.AddedBy
                          ,PDAS.AddedDate
                          ,PDAS.AddedFromIP
                          ,PDAS.UpdatedBy
                          ,PDAS.UpdatedDate
                          ,PDAS.UpdatedFromIP
						  ,PDAS.CurrencyId
						  ,PDAS.OpeningBankMasterId,OB.AccountTitle OpeningBankMaster
						  ,PDAS.BankAmount
						  ,C.Id BankCurrencyId, OB.AccountTitle OpeningBankMaster
						  ,LCGL.ExpensesGLId, LCGL.ExpensesBudgetMasterId, LCGL.ExpensesActivityId
						  ,OB.GLGeneralInfoId, OB.BudgetMasterId, OB.ActivityId
						  ,PDAS.CurrencyId, 1 Rate,PDAS.BankAmount
                        FROM [TRN].[PurchaseDocAcceptanceCharges] PDAS
  	                    LEFT JOIN [HKP].OverHeadType AS LCT ON PDAS.AcceptanceServiceId=LCT.Id
						LEFT JOIN HKP.OverHeadTypeGL AS LCGL ON LCGL.OverHeadTypeId=LCT.Id AND LCGL.GLType='Purchase'
  	                    LEFT JOIN trn.PurchasedocAcceptance AS PDC ON PDAS.PurchaseDocAcceptanceId=PDC.Id
						LEFT JOIN MST.BankMaster OB ON OB.Id=PDAS.OpeningBankMasterId
						LEFT JOIN SCS.Currency C ON C.Id=OB.CurrencyId
	                    Where  PDAS.PurchaseDocAcceptanceId='" + Id + @"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IEnumerable<object> GetAcceptanceServiceDeailsListForPost(string plantId, string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = @"SELECT 
                           PDAS.Id
                          ,PurchaseDocAcceptanceId
                          ,PDAS.AcceptanceServiceId
                          ,SM.UserName ChargeName
                          ,PDAS.Amount
                          ,PDAS.TotalTaxAmount
                          ,PDAS.AddedBy
                          ,PDAS.AddedDate
                          ,PDAS.AddedFromIP
                          ,PDAS.UpdatedBy
                          ,PDAS.UpdatedDate
                          ,PDAS.UpdatedFromIP
						  ,PDAS.CurrencyId
						  ,PDAS.OpeningBankMasterId,OB.AccountTitle OpeningBankMaster
						  ,PDAS.BankAmount
						  ,C.Id BankCurrencyId, OB.AccountTitle OpeningBankMaster
						  ,SCGL.ServiceGLId ExpensesGLId, SCGL.ServiceBudgetMasterId ExpensesBudgetMasterId, SCGL.ServiceActivityId ExpensesActivityId
						  ,OB.GLGeneralInfoId, OB.BudgetMasterId, OB.ActivityId
						  ,PDAS.CurrencyId, 1 Rate,PDAS.BankAmount
                        FROM trn.PurchaseDocAcceptanceService PDAS
  	                    LEFT JOIN [HKP].ServiceMaster AS SM ON PDAS.ServiceMasterId=SM.Id
  	                    LEFT JOIN [HKP].ServiceGroup AS SG ON SM.ServiceGroupId=SG.Id
						LEFT JOIN HKP.ServiceGroupGL AS SCGL ON SG.Id=SCGL.ServiceGroupId 
  	                    LEFT JOIN trn.PurchasedocAcceptance AS PDC ON PDAS.PurchaseDocAcceptanceId=PDC.Id
  	                    LEFT JOIN dbo.PurchaseLC AS PLC ON PDC.PurchaseLCId=PLC.Id
						LEFT JOIN MST.BankMaster OB ON OB.Id=PLC.OpeningBankMasterId
						LEFT JOIN SCS.Currency C ON C.Id=OB.CurrencyId
	                    Where  PDAS.PurchaseDocAcceptanceId='" + Id + @"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }


        public IEnumerable<object> GetAcceptanceDetailForPost(string companyId, string plantId, string Id, string PoType)
        {
            try
            {
                var Sql = @"DECLARE @docAcceptanceId VARCHAR(10) = '" + Id + @"'
					DECLARE @companyId VARCHAR(10) = '" + companyId + @"'
					DECLARE @plantId VARCHAR(10) = '" + plantId + @"'
	                              
					SELECT
                              MGPGL.GLGeneralInfoId,MGPGL.BudgetMasterId,MGPGL.ActivityId
                            , ClearingAccountGLId=CASE WHEN PLC.IsAccepptanceFirst=1 THEN MGGL.InventoryInTransitGLId ELSE GRNMAP.PostCRGLGeneralInfoId END
							,ClearingAccountBudgetMasterId=CASE WHEN PLC.IsAccepptanceFirst=1 THEN MGGL.InventoryInTransitBudgetMasterId ELSE GRNMAP.PostCRBudgetMasterId END
                            ,ClearingAccountActivityId= CASE WHEN PLC.IsAccepptanceFirst=1 THEN MGGL.InventoryInTransitActivityId ELSE GRNMAP.PostCRActivityId END
							, PLC.IsAccepptanceFirst
		                    , NULL TrnType,PDA.Id
                            , PDAD.Id As AcceptenceDetailId
                            , IR.Id AS POID,IRD.Id AS PODetailsID
                        ,  IRD.Id AS InventoryReceiveDetailId
                        ,  MGM.UserName AS MaterialGroupMasterName
                        ,  MM.Id MaterialMasterId
                        ,  MM.UserName
                        ,  IRD.MaterialStorageId
                        , IRD.BaseUOMId
                        , IRD.ArticleId, ART.StandardName
                        , IRD.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                        , IRD.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                        , IRD.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                        , IRD.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                        , IRD.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                        , IRD.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
                        , IRD.TransactionQty AS POQty

                        , ISNULL(PAD.AcptTransactionQty,0) AS GRNRcvQty
                        --, ISNULL(IRD.AcceptanceRcvQty-PDAD.TransactionQty,0) AS GRNRcvQty
			            --, ISNULL(IRD.AcceptanceRcvQty, 0) AS PreviousRcvQty
						, ISNULL(PAD.AcptTransactionQty,0) PreviousRcvQty	
                        ,ISNULL(PDAD.TransactionQty, 0) AS TransactionQty
						, ISNULL(PAD.AcptTransactionQty,0) Otherqty
						--,ISNULL(IRD.AcceptanceRcvQty-PDAD.TransactionQty,0) Otherqty
                       -- ,(IRD.TransactionQty - IRD.AcceptanceRcvQty) As Balance
							  ,(IRD.TransactionQty-PAD.AcptTransactionQty) As Balance
                            , ISNULL(IRD.QtyStatus, 0) QtyStatus
                        , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM, PDAD.MaterialTranRate TransactionRate, CU.Code AS CurrencyName, IR.ToCurrencyRate
                           
                        ,PDAD.MaterialTranAmount AS TrnAmount
						,PDAD.ChargesTranAmount
						,PDAD.ChargesTaxTranAmount
						,PDAD.TotalMaterialTranAmount
						,PDAD.TaxAmount
                        ,0 AS BaseTaxAmount
                        , IRD.CountryId
                        ,'True' enableid
                        ,null POMaterialTaxList
                        , 0 AS ToTalMaterialBooksCurrencyAmount
                        , IR.InvoicingByAddress,IR.DeliveryByAddress
                        ,IRD.RequisitionId
	                    ,IRD.RequisitionDetailId
                        --,MRD.MaterialDetail
                        ,null AS[check] ,IRD.Description MaterialDetail
                        ,PDASD.ChargeName,PDASD.ServiceAmount,PDASD.ServiceMasterId,PDASD.ServiceGLId,PDASD.ServiceBudgetMasterId,PDASD.ServiceActivityId
                        FROM TRN.PurchaseDocAcceptanceDetail PDAD
	                    LEFT JOIN TRN.PurchaseOrderDetail AS IRD  ON PDAD.PODetailId=IRD.Id
                        left JOIN MST.MaterialMaster AS MM ON PDAD.MaterialMasterId = MM.Id
                    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
					LEFT JOIN HKP.MaterialGroupGL MGGL ON MGGL.MaterialGroupMasterId=MGM.Id
                    LEFT JOIN MST.MaterialMasterArticle AS ART ON IRD.ArticleId = ART.Id
                    LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId = FC.Id
                    LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId = SC.Id
                    LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId = TC.Id
                    LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId = FCV.Id
                    LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId = SCV.Id
                    LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId = TCV.Id
                    LEFT JOIN[SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    LEFT JOIN[TRN].[PurchaseOrder] AS IR ON IRD.InventoryReceiveId= IR.Id
                    LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId= CU.Id
                    LEFT JOIN TRN.PurchaseDocAcceptance PDA ON PDA.Id=PDAD.PurchaseDocAcceptanceId
					LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=PDA.PurchaseLCId
					LEFT JOIN (SELECT MGGL1.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL1 ON C.COAId=MGGL1.COAId WHERE C.Id=@companyId)
								AS MGGL1 ON MM.MaterialGroupMasterId = MGGL1.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Vendor')AS CP ON PDA.PartyId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN ( SELECT DISTINCT IRD.PostCRGLGeneralInfoId,IRD.PostCRBudgetMasterId,IRD.PostCRActivityId,GRM.PurchaseDocumentAcceptanceId FROM TRN.InventoryReceiveDetail IRD 
									LEFT JOIN TRN.GRNAcceptanceMap GRM ON GRM.GRNId=IRD.InventoryReceiveId
									
						) GRNMAP ON GRNMAP.PurchaseDocumentAcceptanceId=PDA.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id
                    LEFT JOIN (SELECT POId,PODetailId,Sum(TransactionQty) AcptTransactionQty FROM TRN.PurchaseDocAcceptanceDetail GROUP BY POId,PODetailId) PAD ON PAD.POId=IRD.InventoryReceiveId AND PAD.PODetailId=IRD.Id
                    LEFT JOIN(SELECT PurchaseDocAcceptanceId,PDAS.ServiceMasterId  ,SM.UserName ChargeName ,PDAS.Amount ServiceAmount  ,PDAS.TotalTaxAmount ,PDAS.CurrencyId ,SCGL.ServiceGLId , SCGL.ServiceBudgetMasterId , SCGL.ServiceActivityId 
                        FROM trn.PurchaseDocAcceptanceService PDAS
  	                    LEFT JOIN [HKP].ServiceMaster AS SM ON PDAS.ServiceMasterId=SM.Id
  	                    LEFT JOIN [HKP].ServiceGroup AS SG ON SM.ServiceGroupId=SG.Id
						LEFT JOIN HKP.ServiceGroupGL AS SCGL ON SG.Id=SCGL.ServiceGroupId 
  	                    LEFT JOIN trn.PurchasedocAcceptance AS PDC ON PDAS.PurchaseDocAcceptanceId=PDC.Id
  	                    LEFT JOIN dbo.PurchaseLC AS PLC ON PDC.PurchaseLCId=PLC.Id
	                    Where  PDAS.PurchaseDocAcceptanceId=@docAcceptanceId)PDASD ON PDASD.PurchaseDocAcceptanceId=PDA.Id
                    WHERE PDA.Id=@docAcceptanceId";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetGRNAcceptanceDetailForPost(string Id, string companyId, string plantId)
        {
            try
            {
                var Sql = @"SELECT MGPGL.GLGeneralInfoId,MGPGL.BudgetMasterId,MGPGL.ActivityId
                            , ClearingAccountGLId=CASE WHEN PLC.IsAccepptanceFirst=1 THEN MGGL.InventoryInTransitGLId ELSE GRNMAP.PostCRGLGeneralInfoId END
							,ClearingAccountBudgetMasterId=CASE WHEN PLC.IsAccepptanceFirst=1 THEN MGGL.InventoryInTransitBudgetMasterId ELSE GRNMAP.PostCRBudgetMasterId END
                            ,ClearingAccountActivityId= CASE WHEN PLC.IsAccepptanceFirst=1 THEN MGGL.InventoryInTransitActivityId ELSE GRNMAP.PostCRActivityId END
							,PACD.Id AcceptenceDetailId, POD.InventoryReceiveId POId,POD.Id PODetailId,MGM.UserName AS MaterialGroupMasterName,MM.Id MaterialMasterId
                        ,MM.UserName,POD.MaterialStorageId,POD.BaseUOMId,POD.ArticleId,ART.StandardName,POD.FirstCharacteristicsId,FC.UserName AS FirstCharacteristics
                        ,POD.FirstCharacteristicsValueId,FCV.UserName AS FirstCharacteristicsValue,POD.SecondCharacteristicsId,SC.UserName AS SecondCharacteristics
                        ,POD.SecondCharacteristicsValueId,SCV.UserName AS SecondCharacteristicsValue,POD.ThirdCharacteristicsId,TC.UserName AS ThirdCharacteristics
                        ,POD.ThirdCharacteristicsValueId,TCV.UserName AS ThirdCharacteristicsValue,0 AS BaseTaxAmount,0 AS TaxAmount,0 AS ChargesAmount
                        ,0 AS ServiceCharge,0 AS ServiceTax,POD.CountryId,NULL POMaterialTaxList,POD.TransactionQty POQty,SUM(IRD.TransactionQty) AS GRNRcvQty,SUM(IRD.MaterialTranAmount) AS TotalGRNValue
                        ,ISNULL(PACD.TransactionQty, 0) AS TransactionQty,ISNULL(PAD.AcptTransactionQty, 0) Otherqty
                        ,ISNULL(((SELECT Min(v) FROM (VALUES (POD.TransactionQty), (SUM(IRD.TransactionQty))) AS value(v)) -(ISNULL(PAD.AcptTransactionQty, 0)+ PACD.TransactionQty)),0) AS Balance
                        ,POD.TransactionAmount TotalPOValue,ISNULL(PAD.TotalAcptValue,0) TotalAcptValue,POD.TransactionRate,POD.TransactionRate MaterialTranRate,ISNULL(PACD.MaterialTranAmount,0) TrnAmount,ISNULL(PACD.TotalMaterialTranAmount,0)TotalMaterialTranAmount,0 AS ToTalMaterialBooksCurrencyAmount
                        ,POD.TransactionUoMId,TUoM.UserName AS TransactionUoM,CU.Code AS CurrencyName,PO.ToCurrencyRate,PDASD.ServiceAmount 
                        FROM TRN.PurchaseOrderDetail AS POD
                        LEFT JOIN MST.MaterialMaster AS MM ON POD.InventoryMaterialId = MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
						LEFT JOIN HKP.MaterialGroupGL MGGL ON MGGL.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON POD.ArticleId = ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON POD.FirstCharacteristicsId = FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON POD.SecondCharacteristicsId = SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON POD.ThirdCharacteristicsId = TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON POD.FirstCharacteristicsValueId = FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON POD.SecondCharacteristicsValueId = SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON POD.ThirdCharacteristicsValueId = TCV.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON POD.TransactionUoMId = TUoM.Id
                        LEFT JOIN TRN.[InventoryReceiveDetail] AS IRD ON POD.InventoryReceiveId = IRD.POId AND POD.Id=IRD.PODetailsId
                        LEFT JOIN [TRN].[PurchaseOrder] AS PO ON POD.InventoryReceiveId = PO.Id                       
                        LEFT JOIN [SCS].[Currency] AS CU ON PO.CurrencyId = CU.Id
						LEFT JOIN TRN.PurchaseDocAcceptanceDetail PACD ON PACD.POId = POD.InventoryReceiveId AND PACD.PODetailId = POD.Id AND PurchaseDocAcceptanceId='" + Id + @"'
						LEFT JOIN TRN.PurchaseDocAcceptance PDA ON PDA.Id=PACD.PurchaseDocAcceptanceId
					LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=PDA.PurchaseLCId
						LEFT JOIN (SELECT MGGL1.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL1 ON C.COAId=MGGL1.COAId WHERE C.Id='" + companyId + @"')
								AS MGGL1 ON MM.MaterialGroupMasterId = MGGL1.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId='" + plantId + @"' AND PartyType='Vendor')AS CP ON PDA.PartyId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN ( SELECT DISTINCT IRD.PostCRGLGeneralInfoId,IRD.PostCRBudgetMasterId,IRD.PostCRActivityId,GRM.PurchaseDocumentAcceptanceId FROM TRN.InventoryReceiveDetail IRD 
									LEFT JOIN TRN.GRNAcceptanceMap GRM ON GRM.GRNId=IRD.InventoryReceiveId
									
						) GRNMAP ON GRNMAP.PurchaseDocumentAcceptanceId=PDA.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id
                        
                        LEFT JOIN (SELECT POId,PODetailId,Sum(TransactionQty) AcptTransactionQty,SUM(TotalMaterialTranAmount) TotalAcptValue FROM TRN.PurchaseDocAcceptanceDetail WHERE PurchaseDocAcceptanceId<>'" + Id + @"' GROUP BY POId,PODetailId) PAD ON PAD.POId = POD.InventoryReceiveId AND PAD.PODetailId = POD.Id
                        LEFT JOIN(SELECT PurchaseDocAcceptanceId,PDAS.ServiceMasterId  ,PDAS.Amount ServiceAmount FROM trn.PurchaseDocAcceptanceService PDAS Where  PDAS.PurchaseDocAcceptanceId='" + Id + @"')PDASD ON PDASD.PurchaseDocAcceptanceId=PDA.Id
                        WHERE IRD.InventoryReceiveId IN(Select GRNId from TRN.GRNAcceptanceMap where PurchaseDocumentAcceptanceId='" + Id + @"')
                        GROUP BY 
						MGPGL.GLGeneralInfoId,MGPGL.BudgetMasterId,MGPGL.ActivityId
						,PACD.Id,POD.InventoryReceiveId,POD.Id,MGM.UserName,MM.Id,MM.UserName,POD.MaterialStorageId,POD.BaseUOMId,POD.ArticleId,ART.StandardName,POD.FirstCharacteristicsId
                        ,FC.UserName,POD.FirstCharacteristicsValueId,FCV.UserName,POD.SecondCharacteristicsId,SC.UserName,POD.SecondCharacteristicsValueId,SCV.UserName,POD.ThirdCharacteristicsId
                        ,TC.UserName,POD.ThirdCharacteristicsValueId,TCV.UserName,POD.CountryId,POD.TransactionQty,PACD.TransactionQty,PAD.AcptTransactionQty,POD.TransactionQty,POD.TransactionAmount,PAD.TotalAcptValue,POD.TransactionRate,PACD.MaterialTranAmount
                        ,PACD.TotalMaterialTranAmount,POD.TransactionUoMId,TUoM.UserName,CU.Code,PO.ToCurrencyRate, PLC.IsAccepptanceFirst, MGGL.InventoryInTransitGLId,GRNMAP.PostCRGLGeneralInfoId
						,MGGL.InventoryInTransitBudgetMasterId, GRNMAP.PostCRBudgetMasterId,MGGL.InventoryInTransitActivityId,GRNMAP.PostCRActivityId,PDASD.ServiceAmount ORDER BY PACD.Id";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public void DeletePurchaseDocAcceptancePost(string pdocAccpId, string voucherId)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                //var voucher = _voucherService.FindVoucher(voucherId);
                //if (voucher.IsPark == false)
                //    throw new CustomException("Delete is not allow after post ! ");

                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";
                

                vendorAdWrsql = @"delete trn.VoucherDetailCurrency where VoucherId = '" + voucherId + "'";
                vendorAdWr.Append(vendorAdWrsql);
                //vendorAdWrsql = @"delete from trn.GLTransactionDetail where VoucherDetailId in (select Id from TRN.VoucherDetail  where VoucherId= '" + voucherId + "')";
                //vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.VoucherDetail where VoucherId= '" + voucherId + "'";
                vendorAdWr.Append(vendorAdWrsql);

                vendorAdWrsql = @"update TRN.PurchaseDocAcceptance set VoucherId=null where Id ='" + pdocAccpId + "'";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from TRN.InvoiceTaxDetail where InvoiceTaxId in (select Id from TRN.InvoiceTax  where VoucherId = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from TRN.InvoiceTax  where VoucherId = '" + voucherId + "'";
                vendorAdWr.Append(vendorAdWrsql);

                vendorAdWrsql = @"delete from TRN.InvoiceDetail where InvoiceId in (select Id from TRN.Invoice  where VoucherId = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from TRN.Invoice  where VoucherId = '" + voucherId + "'";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from TRN.voucher  where Id = '" + voucherId + "'";
                vendorAdWr.Append(vendorAdWrsql);

                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());

                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        #endregion

    }
}