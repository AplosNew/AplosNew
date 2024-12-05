using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Inventory;
using Library.Model.Parties;
using Library.Model.Taxations;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Aplos.MaterialManagement.MaterialQuery
{
    public class BOQQueryService
    {
        private readonly ISqlRepository _sqlRepository;
        public BOQQueryService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }

        public List<Dictionary<string, object>> GetCompanyBOQPartyListNew(string companyGroupId, string companyId, string plantId, string column, string value, string customerVendor)
        {
            try
            {
                string temp = null;
                if (customerVendor == "Vendor" || customerVendor == "Customer")
                {
                    temp = customerVendor;
                }
                if (customerVendor == null)
                {
                    temp = "Vendor" + "','" + "Customer";
                }
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"select top 100 * from (SELECT  P.Id AS PartyId, P.Code AS PartyCode, P.UserName AS PartyName, P.Id, P.Code, P.UserName, CP.PartyType, CP.PartyAccountGroupId, PAG.Code AS PartyAccountGroupCode, PAG.UserName AS PartyAccountGroupName, CP.CurrencyId, C.Code AS CurrencyCode, C.[Name] AS CurrencyName
                                    , CP.PaymentTermId, PT.Code AS PaymentTermCode, PT.UserName AS PaymentTermName, CP.IsPaymentTermChangeable
                                    , NULL AS InvoicingPartyPlantId, NULL AS DeliveryPartyPlantId, CO.Code AS CountryCode, CO.UserName AS CountryName, S.Code AS StateCode, S.UserName AS StateName
                                    , RGL.ReconciliationGLId, RGL.ReconciliationGLCode, RGL.ReconciliationGLName
                                    , RGL.ReconciliationBudgetId, RGL.ReconciliationBudgetCode, RGL.ReconciliationBudgetName
                                    , RGL.ReconciliationActivityId, RGL.ReconciliationActivityCode, RGL.ReconciliationActivityName
                                    , CP.TaxApplicable, CP.IsTaxApplicableChangeable
									, (SELECT COUNT(Id) FROM [HKP].[PartyPlant] WHERE PartyId=P.Id) AS TotalPartyPlant
                                    FROM [HKP].[Party] AS P
                                    LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id
                                    LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
                                    LEFT JOIN [MST].[PaymentTerm] AS PT ON PT.Id=CP.PaymentTermId
                                    LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=P.AddressMasterId
									LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
									LEFT JOIN [SCS].[State] AS S ON S.Id=AM.StateId
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS ReconciliationGLId, GL.AccountCode AS ReconciliationGLCode, GL.UserName AS ReconciliationGLName
                                    , CPGL.BudgetMasterId AS ReconciliationBudgetId, B.Code AS ReconciliationBudgetCode, B.UserName AS ReconciliationBudgetName
                                    , CPGL.ActivityId AS ReconciliationActivityId, A.Code AS ReconciliationActivityCode, A.UserName AS ReconciliationActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.ReconciliationGL + @"'
                                    ) AS RGL ON RGL.CompanyPartyId=CP.Id
                                    JOIN (SELECT DISTINCT VendorId FROM  BOQ )AS boq ON boq.VendorId=P.Id 
                                    
                                    WHERE P.Archive=0 AND P.Active=1 AND P.CompanyGroupId='" + companyGroupId + "' AND CP.PartyType IN ('" + temp + "') AND CP.CompanyId='" + companyId + "' AND CP.PlantId='" + plantId + @"'
                                    ) AS TEMP WHERE " + strkey + " order by Code ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
        public IEnumerable<PurchaseOrderTax> GetPOBOQTaxCategoryList(string companyGroupId, string partyPlantId, string plantId, string hsnCodeId)
        {
            try
            {
                var sql = @"DECLARE @partyPlantId varchar(10)='" + partyPlantId + @"'
                                  , @partyState varchar(30)
                                  , @partyCountry varchar(10)
                                  , @plantState varchar(30)
                                  , @plantCountry varchar(10)
                                  , @plantId varchar(30)='" + plantId + @"'
                                  , @hsnCodeId varchar(30)='" + hsnCodeId + @"'
                    SET @partyCountry =(SELECT AM.CountryId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                                    WHERE PP.Id=@partyPlantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                    WHERE PP.Id=@partyPlantId)-- AND AD.Active=1 AND AD.Archive=0)

                    SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SELECT TVD.Id, TVD.TaxCategoryId, HP.HSNCodeId, HN.Code AS HSNCode, TC.UserName, ISNULL(HP.[Percentage], 0) AS [Percentage], NULL TotalAmount
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
                return _sqlRepository.GetModelCollection<PurchaseOrderTax>(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetSpecificMaterialStockBOQ(string companyId, string plantId,string POId, string ContractId, string masterOrderitemId, string salesOrderId, string issueDate, string materialStorageId)
        {
            try
            {
                var sql = "";
               
                    sql = @"select * from(
                        SELECT IRD.InventoryReceiveId, IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId,grnmap.BOQDetailId
 , IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
	                     , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 0 else 1 END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 0 else 1 END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor,IRD.BaseUoMFactor GRNBaseUoMFactor, 1 TempBaseUoMFactor
                        , round(IRD.MaterialTranRate,4) MaterialTranRate, round(IRD.MaterialTranRate,4) Rate,  TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
                        --, BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.MaterialTranAmount/IRD.BaseQty ELSE IRD.BooksCurrencyBaseRate END
						, IRD.TrnCurrencyBaseRate BaseRate
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty, 0 AS Qty
                        --,Round((IRD.MaterialTranRate * IR.ToCurrencyRate),4) 
						 ,Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4) BaseCurrencyRate
						, grnmap.TransactionQty, grnmap.BaseQty,grnmap.BaseQty GRNBOQQty
						,ISNULL(grnmap.BaseQty,0) - ISNULL(II.IssueQty, 0) StockQty
						, ISNULL(II.IssueQty,0)  IssueQty, ISNULL(II.IssueQty,0)  BaseIssueQty
						, ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty,ISNULL(IRD.IssueReturnQty,0) IssueReturnQty,ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty,ISNULL(IRD.InventorySalesQty,0) InventorySalesQty,ISNULL(IRD.InventoryScrapQty,0) InventoryScrapQty,ISNULL(IRD.InventoryTransferQty,0) InventoryTransferQty
						 ,((((((ISNULL(grnmap.BaseQty,0) - ISNULL(II.IssueQty, 0)-ISNULL(IRD.PurchaseReturnQty,0))+ISNULL(IRD.IssueReturnQty,0))-ISNULL(IRD.ReductionByAdjustmentQty,0))-ISNULL(IRD.InventorySalesQty,0))-ISNULL(IRD.InventoryScrapQty,0))-ISNULL(IRD.InventoryTransferQty,0)) AS BalanceStock
                        ,((((((ISNULL(IRD.BaseQty,0) - ISNULL(IIH.ActualIssueQty, 0)-ISNULL(IRD.PurchaseReturnQty,0))+ISNULL(IRD.IssueReturnQty,0))-ISNULL(IRD.ReductionByAdjustmentQty,0))-ISNULL(IRD.InventorySalesQty,0))-ISNULL(IRD.InventoryScrapQty,0))-ISNULL(IRD.InventoryTransferQty,0)) AS ActualBalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						 ,Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4) BooksCurrencyBaseRate
						 ,round(ISNULL(IRD.TrnCurrencyBaseRate,0),4) TrnCurrencyBaseRate
                        --,round(ISNULL(II.IssueAmount,0),4) TotalIssueAmount
                         ,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END
                        ,C.Id CountryId,C.UserName CountryName--,null AS [Flag] 
                        ,0 SalesRate
						,0 TotalAmount
                        ,IM.MaterialMasterId
						,MM.UserName MaterialMasterName
						,IM.ArticleId
						,MMA.StandardName ArticleName
                        ,IM.FirstCharacteristicsValueId
						,FC.UserName SKU1
						,IM.SecondCharacteristicsValueId
						,SC.UserName SKU2
						,IM.ThirdCharacteristicsValueId
						
						,IM.FirstCharacteristicsId
						,IM.SecondCharacteristicsId
						,IM.ThirdCharacteristicsId,IssueByUoM=CASE WHEN MM.IssueByUoM=0 THEN 'No' ELSE 'Yes' END
                        ,TrasactopmUomQty=(((ISNULL(IRD.BaseQty,0) - ISNULL(II.IssueQty, 0))*BaseUoMFactor)/BaseUoMFactor) 
                        ,TempTrasactopmUomQty=(((ISNULL(IRD.BaseQty,0) - ISNULL(II.IssueQty, 0))*BaseUoMFactor)/BaseUoMFactor) 
						,IRD.BaseUOMId IssueTransactionUoMId,IRD.MaterialStorageId,MS.UserName MaterialStorage
						,'' IssueTransactionUoM,ISNULL(IRD.LotNumber,'') LotNumber
                     FROM TRN.GRNPORequisitionAllocation grnmap
					join [TRN].[InventoryReceiveDetail] AS IRD  on grnmap.InventoryReceiveDetailId=ird.Id
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    LEFT JOIN HKP.CharacteristicsValue FC ON FC.Id=IM.FirstCharacteristicsValueId
					LEFT JOIN HKP.CharacteristicsValue SC ON SC.Id=IM.SecondCharacteristicsValueId
					LEFT JOIN mst.MaterialMaster MM ON MM.Id=Im.MaterialMasterId
					LEFT JOIN mst.MaterialMasterArticle MMA ON MMA.Id=Im.ArticleId
                    LEFT JOIN [HKP].[MaterialStorage] MS ON MS.Id=IRD.MaterialStorageId
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
					LEFT JOIN (
									    select IHB.InventoryReceiveDetailId
                                        , Sum(ISNULL(IHB.Qty,0)) IssueQty 
										,IHB.BOQDetailId
									    FROM TRN.InventoryIssueHistoryBOQ IHB 
									    GROUP BY IHB.InventoryReceiveDetailId
										,IHB.BOQDetailId
									    ) II ON II.InventoryReceiveDetailId=grnmap.InventoryReceiveDetailId 
										and ii.BOQDetailId=grnmap.BOQDetailId
                    LEFT JOIN (
									    select IH.InventoryReceiveDetailId
                                        , Sum(ISNULL(IH.Qty,0)) ActualIssueQty 
									    FROM TRN.InventoryIssueHistory IH 
									    GROUP BY IH.InventoryReceiveDetailId
									    ) IIH ON IIH.InventoryReceiveDetailId=IRD.Id
                    left JOIN SCS.Country C On C.Id=IM.CountryId
                    WHERE  IM.CompanyId='" + companyId + "' AND IM.PlantId='"+plantId+@"'
                    AND IR.[Status]='Posting' AND IR.IsFOC=0
                    ----AND ISNULL(IM.ArticleId,'')='5777' AND ISNULL(IM.FirstCharacteristicsValueId,'')='423' AND  ISNULL(IM.SecondCharacteristicsValueId,'')=''
                    --AND ISNULL(IM.ThirdCharacteristicsValueId,'')='' AND ISNULL(IM.CountryId,'')='' 
					AND IRD.MaterialStorageId='"+ materialStorageId + @"' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
					AND (IRD.POId IN ("+POId+ @") OR IRD.POId IN (''))
					 AND grnmap.BaseQty !=ISNULL(II.IssueQty,0)  
                    AND CAST(IR.GRNDate AS DATE)<=CAST('" + issueDate + @"' AS DATE) 
					) x";
               
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetSearchDistinctMaterialBOQ(string companyId, string plantId, string materialStorageId)
        {
            try
            {
                var sql = @"SELECT X.*, PONo=STUFF((select distinct ','+XSO.Id
		                                         from    trn.PurchaseOrder XSO 	 
												 JOIN [TRN].[PurchaseOrderDetail] AS XIRD ON XSO.Id=XIRD.InventoryReceiveId
									                where XIRD.InventoryMaterialId=X.MaterialMasterId AND XIRD.ArticleId=X.ArticleId AND XSO.IsClosed=0	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								 
FROM (SELECT  distinct IM.MaterialMasterId ,MT.UserName MaterialType,MMG.UserName MaterialGroup,MM.Code MaterialCode,MM.UserName MaterialMasterName
						,IM.ArticleId ,MMA.StandardName ArticleName,0 IsSelect
                    FROM TRN.GRNPORequisitionAllocation grnmap
					JOIN [TRN].[InventoryReceiveDetail] AS IRD  on grnmap.InventoryReceiveDetailId=ird.Id
                    LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
					LEFT JOIN MST.MaterialMaster MM ON MM.Id=Im.MaterialMasterId
					LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=Im.ArticleId
					LEFT JOIN Mst.MaterialGroupMaster MMG ON MMG.Id=MM.MaterialGroupMasterId
					LEFT JOIN HKP.MaterialType MT ON MT.Id=MMG.MaterialTypeId
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
					LEFT JOIN (
								 SELECT IHB.InventoryReceiveDetailId, SUM(ISNULL(IHB.Qty,0)) IssueQty ,IHB.BOQDetailId
								 FROM TRN.InventoryIssueHistoryBOQ IHB 
								 GROUP BY IHB.InventoryReceiveDetailId ,IHB.BOQDetailId
								 ) II ON II.InventoryReceiveDetailId=grnmap.InventoryReceiveDetailId  
                    WHERE  IM.CompanyId='" + companyId + @"' AND IM.PlantId='"+ plantId + "' AND IRD.MaterialStorageId='"+ materialStorageId + @"'
                    	AND IRD.POId<>'' AND grnmap.BOQDetailId<>''
                    AND IR.[Status]='Posting' AND IR.IsFOC=0 AND IRD.BaseQty !=ISNULL(II.IssueQty,0) 
                    AND MM.Id IN(SELECT MBP.MaterialMasterId FROM [MST].[MaterialMasterBusinessProcess] AS MBP
                        LEFT JOIN [SCS].[BusinessProcess] AS BP ON MBP.BusinessProcessId = BP.Id
                        WHERE BP.BusinessProcessName='BOM'))X";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetItemListDetailsByList(string MaterialIds, string ArticleIds, string VendorRefNos, string CustomerRefNos, string OwnReferenceNo, string PartyId)
        {
            try
            {
                var sql = @" SELECT distinct Convert(bit, 'False') IsActives,POD.InventoryReceiveId POId,P.UserName CustomerName,C.ContractNo,mo.Id MasterOrderId
                            ,boq.SalesOrderId
							,PO.CurrencyId,CU.Code CurrencyCode
							,StyleNo=STUFF((select distinct ','+xmoi.BuyerReferenceNo from
							TRN.MasterOrderItem xmoi 
									LEFT JOIN TRN.SalesOrder so on moi.Id=so.MasterOrderItemId
									LEFT JOIN TRN.MasterOrder mo on mo.Id=moi.MasterOrderId
									LEFT JOIN TRN.POBOQMAP pomap on pomap.BOQDetailId=boq.Id
									LEFT JOIN TRN.PurchaseOrderDetail xPOD ON xPOD.Id=pomap.PODetailId
								where xPOD.Id=POD.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,moi.OwnReferenceNo
							FROM BOQ  boq 
							LEFT JOIN MST.MaterialMaster mm on mm.Id=boq.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle mma on mma.Id=boq.ArticleId
							LEFT JOIN TRN.MasterOrderItem moi on moi.Id=boq.MasterOrderItemId
                            LEFT JOIN TRN.SalesOrder so on moi.Id=so.MasterOrderItemId
							LEFT JOIN TRN.MasterOrder mo on mo.Id=moi.MasterOrderId
							LEFT JOIN TRN.POBOQMAP pomap on pomap.BOQDetailId=boq.Id
							LEFT JOIN HKP.Party P ON P.Id=mo.PartyId
							LEFT JOIN TRN.PurchaseOrderDetail POD ON POD.Id=pomap.PODetailId
							LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=POD.InventoryReceiveId
							LEFT JOIN SCS.Currency CU ON CU.Id=PO.CurrencyId
                            LEFT JOIN dbo.Contract C on C.Id=so.ContractId
							WHERE boq.MaterialMasterId IN (" + MaterialIds + ") AND boq.ArticleId IN (" + ArticleIds + @")
                            AND ISNULL(boq.OwnReferenceNo,'') in (" + OwnReferenceNo + ") AND ISNULL(boq.RMCustomerSpec,'') IN (" + CustomerRefNos + @")
							AND ISNULL(boq.RMVendorSpec,'') IN (" + VendorRefNos + @") AND  PO.PartyId='" + PartyId + @"' AND POD.InventoryReceiveId<>''";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetSelectedItemListDetailsByList(string POId, string ContractId, string masterOrderitemId, string SalesOrderId, string MaterialMasterId, string ArticleId)
        {
            try
            {
                var sql = @"DECLARE @totalReceiveAmount DECIMAL(18, 4) = 0
                            	,@totalServiceAmount DECIMAL(18, 4) = 0
                            	,@totalSvcTaxAmount DECIMAL(18, 4) = 0
                            
                            -- SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(TransactionAmount, 0)),1) FROM [TRN].[PurchaseOrderDetail] WHERE IRD.InventoryReceiveId in('','21180'))
                            --SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount - GRNServiceAmount, 0)),0) As Amount FROM [TRN].[POService] WHERE IRD.InventoryReceiveId in('','21180'))
                            --SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[PurchaseOrderTax] WHERE IRD.InventoryReceiveId in('','21180') AND InventoryServiceId<>'')
                            SELECT
                            	--IM.Id
                            	IR.Id AS POID
                            	,IRD.Id AS PODetailsID
                            	,IRD.Id AS InventoryReceiveDetailId
                            	,MGM.UserName AS MaterialGroupMasterName
                            	,MM.Id MaterialMasterId
                            	,MM.UserName
                            	,IRD.MaterialStorageId
                            	,IRD.BaseUOMId
                            	,IRD.ArticleId
                            	,ART.StandardName
                            	,IRD.FirstCharacteristicsId
                            	,FC.UserName AS FirstCharacteristics
                            	,IRD.FirstCharacteristicsValueId
                            	,FCV.UserName AS FirstCharacteristicsValue
                            	,IRD.SecondCharacteristicsId
                            	,SC.UserName AS SecondCharacteristics
                            	,IRD.SecondCharacteristicsValueId
                            	,SCV.UserName AS SecondCharacteristicsValue
                            	,IRD.ThirdCharacteristicsId
                            	,TC.UserName AS ThirdCharacteristics
                            	,IRD.ThirdCharacteristicsValueId
                            	,TCV.UserName AS ThirdCharacteristicsValue
                            	,IRD.TransactionQty AS POQty
                            	--, ISNULL(IRD.GRNRcvQty,0) AS GRNRcvQty 
                            	,ISNULL(aa.TransactionQty, 0) AS GRNRcvQty
                            	--,(IRD.TransactionQty - ISNULL(IRD.GRNRcvQty,0)) AS TransactionQty
                            	,'' AS TransactionQty
                            	,(IRD.TransactionQty - ISNULL(aa.TransactionQty, 0)) AS Balance
                            	,ISNULL(IRD.QtyStatus, 0) QtyStatus
                            	,IRD.TransactionUoMId
                            	,TUoM.UserName AS TransactionUoM
                            	,IRD.TransactionRate
                            	,CU.Code AS CurrencyName
                            	,IR.ToCurrencyRate
                            	,IRD.TransactionAmount
                            	,0 AS TrnAmount
                            	,0 AS BaseTaxAmount
                            	,0 AS TaxAmount
                            	,0 AS ChargesAmount
                            	,0 AS ServiceCharge
                            	,0 AS ServiceTax
                            	,IRD.CountryId
                            	,'True' enableid
                            	,NULL POMaterialTaxList
                            	,0 AS TotalMaterialTranAmount
                            	,0 AS ToTalMaterialBooksCurrencyAmount
                            	,IR.InvoicingByAddress
                            	,IR.DeliveryByAddress
                            	,IRD.RequisitionId
                            	,IRD.RequisitionDetailId
                            	,0 ShortageQty
                            	,0 RejectionQty
                            	--,MRD.MaterialDetail
                            	,NULL AS [check]
                            	,IRD.Description MaterialDetail
                            	,'null' PurchaseDocAcceptanceDetailId
                            	,0 POClosStatus
                            	,C.UserName CountryName
                            	,C.Id CountryId
                            	,MM.IsAsset
                            	,IRD.TotalTaxAmount
                            	,0 GrossAmount
                            	,0 DiscountAmount
                            	,'' QualityStatus
                            	,IRD.TransactionUoMId POUoMId
                            	,IRD.Tolerance
                            	,IRD.RefferenceNo
                            FROM TRN.PurchaseOrderDetail AS IRD
                            --LEFT JOIN TRN.PurchaseOrderDetail AS IRD ON IRD.InventoryMaterialId=PM.Id
                            LEFT JOIN MST.MaterialMaster AS MM ON IRD.InventoryMaterialId = MM.Id
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                            LEFT JOIN MST.MaterialMasterArticle AS ART ON IRD.ArticleId = ART.Id
                            LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId = FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId = SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId = TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId = FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId = SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId = TCV.Id
                            -- JOIN [TRN].[PurchaseOrderDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                            LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
                            LEFT JOIN [TRN].[PurchaseOrder] AS IR ON IRD.InventoryReceiveId = IR.Id
                            LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
                            LEFT JOIN [trn].MaterialRequsitionDetails MRD ON MRD.Id = IRD.RequisitionDetailId
                            LEFT JOIN scs.country C ON C.Id = IRD.CountryId
                            LEFT JOIN (
                            	SELECT PODetailsId
                            		,Sum(TransactionQty) TransactionQty
                            	FROM trn.InventoryReceiveDetail
                            	WHERE isnull(POId,'null') IN (" + POId + @")
                            	GROUP BY PODetailsId
                            	) aa ON aa.PODetailsId = IRD.Id
                            WHERE IRD.QtyStatus = 0
                            	AND IRD.InventoryMaterialId IS NOT NULL
                            	AND isnull(IRD.InventoryReceiveId,'null') IN (" + POId + @")
                            
                            UNION ALL
                            
                            SELECT
                            	--IM.Id
                            	NULL POID
                            	,NULL PODetailsID
                            	,NULL InventoryReceiveDetailId
                            	,MGA.UserName AS MaterialGroupMasterName
                            	,b.MaterialMasterId
                            	,MM.UserName
                            	,NULL MaterialStorageId
                            	,b.BaseUoMId
                            	,b.ArticleId
                            	,mma.StandardName
                            	,V1.CharacteristicsId FirstCharacteristicsId
                            	,FC.UserName AS FirstCharacteristics
                            	,b.FirstCharacteristicsValueId
                            	,V1.UserName AS FirstCharacteristicsValue
                            	,V2.CharacteristicsId SecondCharacteristicsId
                            	,SC.UserName AS SecondCharacteristics
                            	,b.SecondCharacteristicsValueId
                            	,V2.UserName AS SecondCharacteristicsValue
                            	,V3.CharacteristicsId ThirdCharacteristicsId
                            	,TC.UserName AS ThirdCharacteristics
                            	,b.ThirdCharacteristicsValueId
                            	,V3.UserName AS ThirdCharacteristicsValue
                            	,b.RequiredQty AS POQty
                            	,0 AS GRNRcvQty
                            	--,(IRD.TransactionQty - ISNULL(IRD.GRNRcvQty,0)) AS TransactionQty
                            	,'' AS TransactionQty
                            	,(b.RequiredQty) AS Balance
                            	,NULL QtyStatus
                            	,b.UoMId TransactionUoMId
                            	,TUoM.UserName AS TransactionUoM
                            	,b.Rate
                            	,NULL CurrencyName
                            	,0 ToCurrencyRate
                            	,b.RequiredQty * b.Rate TransactionAmount
                            	,0 AS TrnAmount
                            	,0 AS BaseTaxAmount
                            	,0 AS TaxAmount
                            	,0 AS ChargesAmount
                            	,0 AS ServiceCharge
                            	,0 AS ServiceTax
                            	,NULL CountryId
                            	,'True' enableid
                            	,NULL POMaterialTaxList
                            	,0 AS TotalMaterialTranAmount
                            	,0 AS ToTalMaterialBooksCurrencyAmount
                            	,NULL InvoicingByAddress
                            	,NULL DeliveryByAddress
                            	,NULL RequisitionId
                            	,NULL RequisitionDetailId
                            	,0 ShortageQty
                            	,0 RejectionQty
                            	--,MRD.MaterialDetail
                            	,NULL AS [check]
                            	,NULL MaterialDetail
                            	,'null' PurchaseDocAcceptanceDetailId
                            	,0 POClosStatus
                            	,NULL CountryName
                            	,NULL CountryId
                            	,MM.IsAsset
                            	,0 TotalTaxAmount
                            	,0 GrossAmount
                            	,0 DiscountAmount
                            	,'' QualityStatus
                            	,b.UoMId POUoMId
                            	,0 Tolerance
                            	,NULL RefferenceNo
                            FROM BOQ AS b
                            LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id = b.MaterialMasterId
                            LEFT OUTER JOIN MST.MaterialGroupMaster AS MGA ON MGA.Id = mm.MaterialGroupMasterId
                            LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id = b.ArticleId
                            LEFT OUTER JOIN [HKP].[CharacteristicsValue] V1 ON v1.Id = b.FirstCharacteristicsValueId
                            LEFT OUTER JOIN [HKP].[CharacteristicsValue] V2 ON v2.Id = b.SecondCharacteristicsValueId
                            LEFT OUTER JOIN [HKP].[CharacteristicsValue] V3 ON v3.Id = b.ThirdCharacteristicsValueId
                            LEFT JOIN HKP.Characteristics AS FC ON FC.Id = V1.CharacteristicsId
                            LEFT JOIN HKP.Characteristics AS SC ON SC.Id = V2.CharacteristicsId
                            LEFT JOIN HKP.Characteristics AS TC ON TC.Id = V3.CharacteristicsId
                            -- JOIN [TRN].[PurchaseOrderDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                            LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON b.UoMId = TUoM.Id
                            --LEFT JOIN [TRN].[PurchaseOrder] AS IR ON IRD.InventoryReceiveId=IR.Id
                            --LEFT JOIN [SCS].[Currency] AS CU ON moi.CurrencyId=CU.Id
                            --LEFT join [trn].MaterialRequsitionDetails MRD on MRD.Id=IRD.RequisitionDetailId
                            --left join scs.country C On C.Id=IRD.CountryId	
                            LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id = b.MasterOrderItemId
                            WHERE (isnull(MOI.ContractId, 'null') IN (" + ContractId + @"))
                            	--AND (isnull(b.masterOrderitemId,'null') in (" + masterOrderitemId + @"))
                            	AND (
                            		isnull(b.SalesOrderId, 'null') IN (" + SalesOrderId + @")
                            		)
                            	AND (isnull(b.MaterialMasterId, 'null') IN (" + MaterialMasterId + @"))
                            	AND (isnull(b.ArticleId, 'null') IN (" + ArticleId + @"))
                                    AND b.Id NOT IN (
                            		
                            	SELECT p.BOQDetailId
                            	FROM trn.POBOQMAP AS p
                            	JOIN trn.PurchaseOrderDetail AS pod ON pod.id =p.PODetailId
                            	WHERE isnull(pod.InventoryReceiveId ,'null') IN (" + POId + @"))";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetPOBOQItemForGRN(string POId, string ContractId, string masterOrderitemId, string SalesOrderId, string MaterialMasterId, string ArticleId)
        {
            try
            {
                var sql = @"DECLARE @totalReceiveAmount DECIMAL(18, 4) = 0
                            	,@totalServiceAmount DECIMAL(18, 4) = 0
                            	,@totalSvcTaxAmount DECIMAL(18, 4) = 0
                             SELECT
                            	
                            	poboq.BOQDetailId AS BOQDetailId
                            	, IR.Id AS POID
                            	,IRD.Id AS PODetailsID
                            	,IRD.Id AS InventoryReceiveDetailId
                            	,MGM.UserName AS MaterialGroupMasterName
                            	,MM.Id MaterialMasterId
                            	,MM.UserName
                            	,IRD.MaterialStorageId
                            	
                            	,IRD.ArticleId
                            	,ART.StandardName
                            	,IRD.FirstCharacteristicsId
                            	,FC.UserName AS FirstCharacteristics
                            	,IRD.FirstCharacteristicsValueId
                            	,FCV.UserName AS FirstCharacteristicsValue
                            	,IRD.SecondCharacteristicsId
                            	,SC.UserName AS SecondCharacteristics
                            	,IRD.SecondCharacteristicsValueId
                            	,SCV.UserName AS SecondCharacteristicsValue
                            	,IRD.ThirdCharacteristicsId
                            	,TC.UserName AS ThirdCharacteristics
                            	,IRD.ThirdCharacteristicsValueId
                            	,TCV.UserName AS ThirdCharacteristicsValue
                                ,poboq.TransactionQty AS POQty
                            	,poboq.TransactionQty*(IRD.Tolerance/100) AS ToleranceQty
								 ,poboq.TransactionQty+poboq.TransactionQty*(IRD.Tolerance/100) AS TotalPOQty
                            	,ISNULL(aa.TransactionQty, 0) AS GRNRcvQty
                            	,'' AS TransactionQty
                            	,((poboq.TransactionQty+poboq.TransactionQty*(IRD.Tolerance/100)) - ISNULL(aa.TransactionQty, 0)) AS Balance
                            	,ISNULL(IRD.QtyStatus, 0) QtyStatus
								,IRD.BaseUOMId
								,IRD.BaseUoMFactor
                            	,IRD.TransactionUoMId
                            	,TUoM.UserName AS TransactionUoM
                            	,IRD.TransactionRate
                            	,IRD.TransactionRate POTrnRate
                            	,CU.Code AS CurrencyName
                            	,IR.ToCurrencyRate
                            	,poboq.POBOQQty*IRD.TransactionRate TransactionAmount
                            	,0 AS TrnAmount
                            	,0 AS BaseTaxAmount
                            	,0 AS TaxAmount
                            	,0 AS ChargesAmount
                            	,0 AS ServiceCharge
                            	,0 AS ServiceTax
                            	,IRD.CountryId
                            	,'True' enableid
                            	,NULL POMaterialTaxList
                            	,0 AS TotalMaterialTranAmount
                            	,0 AS ToTalMaterialBooksCurrencyAmount
                            	,IR.InvoicingByAddress
                            	,IR.DeliveryByAddress
                            	,IRD.RequisitionId
                            	,IRD.RequisitionDetailId
                            	,0 ShortageQty
                            	,0 RejectionQty
                            	--,MRD.MaterialDetail
                            	,NULL AS [check]
                            	,IRD.Description MaterialDetail
                            	,'null' PurchaseDocAcceptanceDetailId
                            	,0 POClosStatus
                            	,C.UserName CountryName
                            	,C.Id CountryId
                            	,MM.IsAsset
                            	,IRD.TotalTaxAmount
                            	,0 GrossAmount
                            	,0 DiscountAmount
                            	,'' QualityStatus
                            	,IRD.TransactionUoMId POUoMId
                            	,IRD.Tolerance
                            	,IRD.RefferenceNo,boq.RMDescription,ART.MinimumValue,ART.MaximumValue
                            FROM TRN.POBOQMAP AS poboq
							LEFT JOIN TRN.PurchaseOrderDetail AS IRD ON IRD.Id=poboq.PODetailId
                            LEFT JOIN BOQ boq ON boq.Id=poboq.BOQDetailId
                            --LEFT JOIN TRN.PurchaseOrderDetail AS IRD ON IRD.InventoryMaterialId=PM.Id
                            LEFT JOIN MST.MaterialMaster AS MM ON IRD.InventoryMaterialId = MM.Id
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                            LEFT JOIN MST.MaterialMasterArticle AS ART ON IRD.ArticleId = ART.Id
                            LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId = FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId = SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId = TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId = FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId = SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId = TCV.Id
                            -- JOIN [TRN].[PurchaseOrderDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                            LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
                            LEFT JOIN [TRN].[PurchaseOrder] AS IR ON IRD.InventoryReceiveId = IR.Id
                            LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
                            LEFT JOIN [trn].MaterialRequsitionDetails MRD ON MRD.Id = IRD.RequisitionDetailId
                            LEFT JOIN scs.country C ON C.Id = IRD.CountryId
                            LEFT JOIN (
                            	SELECT ird.PODetailsId ,boqd.BOQDetailId
								,sum(sum(boqd.TransactionQty)) over (partition by boqd.BOQDetailId,ird.PODetailsId) as TransactionQty
                            	FROM trn.InventoryReceiveDetail ird left join trn.GRNPORequisitionAllocation boqd on boqd.InventoryReceiveDetailId=ird.Id
                            	WHERE isnull(POId,'null') IN (" + POId + @")
                            	GROUP BY PODetailsId,boqd.BOQDetailId
                            	) aa ON aa.PODetailsId = IRD.Id and aa.BOQDetailId=poboq.BOQDetailId
                            WHERE IRD.QtyStatus = 0
                            	AND IRD.InventoryMaterialId IS NOT NULL
                            	AND ISNULL(IRD.InventoryReceiveId,'null') IN (" + POId + @")
                                AND ISNULL(MM.Id,'null') IN ("+ MaterialMasterId + ") AND ISNULL(ART.Id,'null') IN ("+ ArticleId + @")
                            AND IR.IsApproved=1  and IR.IsClosed=0 ";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPOBOQMapListForUpdate(string POId,string poDetailId)
        {
            try
            {
                var sql = @"SELECT
                            	
                            	poboq.Id
                            	,poboq.BOQDetailId AS BOQDetailId
                            	, IR.Id AS POID
                            	,IRD.Id AS PODetailsID
                            	,IRD.Id AS InventoryReceiveDetailId
                            	,MGM.UserName AS MaterialGroupMasterName
                            	,MM.Id MaterialMasterId
                            	,MM.UserName
                            	,IRD.MaterialStorageId
                            	
                            	,IRD.ArticleId
                            	,ART.StandardName
                            	,IRD.FirstCharacteristicsId
                            	,FC.UserName AS FirstCharacteristics
                            	,IRD.FirstCharacteristicsValueId
                            	,FCV.UserName AS FirstCharacteristicsValue
                            	,IRD.SecondCharacteristicsId
                            	,SC.UserName AS SecondCharacteristics
                            	,IRD.SecondCharacteristicsValueId
                            	,SCV.UserName AS SecondCharacteristicsValue
                            	,IRD.ThirdCharacteristicsId
                            	,TC.UserName AS ThirdCharacteristics
                            	,IRD.ThirdCharacteristicsValueId
                            	,TCV.UserName AS ThirdCharacteristicsValue
								,boq.RequiredQty BOQQty
                            	,poboq.POBOQQty AS POQty
                            	,ISNULL(aa.TransactionQty, 0) AS OtherPOQty
                            	,poboq.POBOQQty TransactionQty
                            	,(boq.RequiredQty - ISNULL(aa.TransactionQty, 0)) AS Balance
                            	,ISNULL(IRD.QtyStatus, 0) QtyStatus
								,IRD.BaseUOMId
								,IRD.BaseUoMFactor
                            	,IRD.TransactionUoMId
                            	,TUoM.UserName AS TransactionUoM
                            	,IRD.TransactionRate
                            	,CU.Code AS CurrencyName
                            	,IR.ToCurrencyRate
                            	,poboq.POBOQQty*IRD.TransactionRate TransactionAmount
                            	,0 AS TrnAmount
                            	,0 AS BaseTaxAmount
                            	,0 AS TaxAmount
                            	,0 AS ChargesAmount
                            	,0 AS ServiceCharge
                            	,0 AS ServiceTax
                            	,IRD.CountryId
                            	,'True' enableid
                            	,NULL POMaterialTaxList
                            	,0 AS TotalMaterialTranAmount
                            	,0 AS ToTalMaterialBooksCurrencyAmount
                            	,IR.InvoicingByAddress
                            	,IR.DeliveryByAddress
                            	,IRD.RequisitionId
                            	,IRD.RequisitionDetailId
                            	,0 ShortageQty
                            	,0 RejectionQty
                            	--,MRD.MaterialDetail
                            	,NULL AS [check]
                            	,IRD.Description MaterialDetail
                            	,'null' PurchaseDocAcceptanceDetailId
                            	,0 POClosStatus
                            	,C.UserName CountryName
                            	,C.Id CountryId
                            	,MM.IsAsset
                            	,IRD.TotalTaxAmount
                            	,0 GrossAmount
                            	,0 DiscountAmount
                            	,'' QualityStatus
                            	,IRD.TransactionUoMId POUoMId
                            	,IRD.Tolerance
                            	,IRD.RefferenceNo,IRD.DeliveryDate
                            FROM TRN.POBOQMAP AS poboq
							JOIN BOQ boq ON boq.Id=poboq.BOQDetailId
							LEFT JOIN TRN.PurchaseOrderDetail AS IRD ON IRD.Id=poboq.PODetailId
                            --LEFT JOIN TRN.PurchaseOrderDetail AS IRD ON IRD.InventoryMaterialId=PM.Id
                            LEFT JOIN MST.MaterialMaster AS MM ON IRD.InventoryMaterialId = MM.Id
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                            LEFT JOIN MST.MaterialMasterArticle AS ART ON IRD.ArticleId = ART.Id
                            LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId = FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId = SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId = TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId = FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId = SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId = TCV.Id
                            -- JOIN [TRN].[PurchaseOrderDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                            LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
                            LEFT JOIN [TRN].[PurchaseOrder] AS IR ON IRD.InventoryReceiveId = IR.Id
                            LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
                            LEFT JOIN [trn].MaterialRequsitionDetails MRD ON MRD.Id = IRD.RequisitionDetailId
                            LEFT JOIN scs.country C ON C.Id = IRD.CountryId
                            LEFT JOIN (
                            	SELECT BOQDetailId,SUM(TransactionQty) TransactionQty,SUM(BaseQty) BaseQty
                            	FROM trn.POBOQMAP 
                            	WHERE  BOQDetailId IN ( SELECT BOQDetailId FROM TRN.POBOQMAP WHERE PODetailId='" + poDetailId+ @"') AND PODetailId NOT IN ('" + poDetailId + @"')
                            	GROUP BY BOQDetailId
                            	) aa ON aa.BOQDetailId = poboq.BOQDetailId
                            WHERE IRD.QtyStatus = 0
                            	AND IRD.InventoryMaterialId IS NOT NULL
                            	AND isnull(IRD.InventoryReceiveId,'null') IN ('" + POId+ @"')  AND ISNULL(poboq.PODetailId,'null') IN ('" + poDetailId + @"')";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetPOBOQMapListForUpdateS(string POId)
        {
            try
            {
                var sql = @"SELECT
                            	
                            	poboq.Id
                            	,poboq.BOQDetailId AS BOQDetailId
                            	, IR.Id AS POID
                            	,IRD.Id AS PODetailsID
                            	,IRD.Id AS InventoryReceiveDetailId
                            	,MGM.UserName AS MaterialGroupMasterName
                            	,MM.Id MaterialMasterId
                            	,MM.UserName
                            	,IRD.MaterialStorageId
                            	
                            	,IRD.ArticleId
                            	,ART.StandardName
                            	,IRD.FirstCharacteristicsId
                            	,FC.UserName AS FirstCharacteristics
                            	,IRD.FirstCharacteristicsValueId
                            	,FCV.UserName AS FirstCharacteristicsValue
                            	,IRD.SecondCharacteristicsId
                            	,SC.UserName AS SecondCharacteristics
                            	,IRD.SecondCharacteristicsValueId
                            	,SCV.UserName AS SecondCharacteristicsValue
                            	,IRD.ThirdCharacteristicsId
                            	,TC.UserName AS ThirdCharacteristics
                            	,IRD.ThirdCharacteristicsValueId
                            	,TCV.UserName AS ThirdCharacteristicsValue
								,boq.RequiredQty BOQQty
                            	,poboq.POBOQQty AS POQty
                            	--,ISNULL(aa.TransactionQty, 0) AS OtherPOQty
                            	,poboq.POBOQQty TransactionQty
                            	--,(boq.RequiredQty - ISNULL(aa.TransactionQty, 0)) AS Balance
                            	,ISNULL(IRD.QtyStatus, 0) QtyStatus
								,IRD.BaseUOMId
								,IRD.BaseUoMFactor
                            	,IRD.TransactionUoMId
                            	,TUoM.UserName AS TransactionUoM
                            	,IRD.TransactionRate
                            	,CU.Code AS CurrencyName
                            	,IR.ToCurrencyRate
                            	,poboq.POBOQQty*IRD.TransactionRate TransactionAmount
                            	,0 AS TrnAmount
                            	,0 AS BaseTaxAmount
                            	,0 AS TaxAmount
                            	,0 AS ChargesAmount
                            	,0 AS ServiceCharge
                            	,0 AS ServiceTax
                            	,IRD.CountryId
                            	,'True' enableid
                            	,NULL POMaterialTaxList
                            	,0 AS TotalMaterialTranAmount
                            	,0 AS ToTalMaterialBooksCurrencyAmount
                            	,IR.InvoicingByAddress
                            	,IR.DeliveryByAddress
                            	,IRD.RequisitionId
                            	,IRD.RequisitionDetailId
                            	,0 ShortageQty
                            	,0 RejectionQty
                            	--,MRD.MaterialDetail
                            	,NULL AS [check]
                            	,IRD.Description MaterialDetail
                            	,'null' PurchaseDocAcceptanceDetailId
                            	,0 POClosStatus
                            	,C.UserName CountryName
                            	,C.Id CountryId
                            	,MM.IsAsset
                            	,IRD.TotalTaxAmount
                            	,0 GrossAmount
                            	,0 DiscountAmount
                            	,'' QualityStatus
                            	,IRD.TransactionUoMId POUoMId
                            	,IRD.Tolerance
                            	,IRD.RefferenceNo
                            FROM TRN.POBOQMAP AS poboq
							JOIN BOQ boq ON boq.Id=poboq.BOQDetailId
							LEFT JOIN TRN.PurchaseOrderDetail AS IRD ON IRD.Id=poboq.PODetailId
                            --LEFT JOIN TRN.PurchaseOrderDetail AS IRD ON IRD.InventoryMaterialId=PM.Id
                            LEFT JOIN MST.MaterialMaster AS MM ON IRD.InventoryMaterialId = MM.Id
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                            LEFT JOIN MST.MaterialMasterArticle AS ART ON IRD.ArticleId = ART.Id
                            LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId = FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId = SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId = TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId = FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId = SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId = TCV.Id
                            -- JOIN [TRN].[PurchaseOrderDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                            LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
                            LEFT JOIN [TRN].[PurchaseOrder] AS IR ON IRD.InventoryReceiveId = IR.Id
                            LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
                            LEFT JOIN [trn].MaterialRequsitionDetails MRD ON MRD.Id = IRD.RequisitionDetailId
                            LEFT JOIN scs.country C ON C.Id = IRD.CountryId
                            
                            WHERE IRD.QtyStatus = 0
                            	AND IRD.InventoryMaterialId IS NOT NULL
                            	AND isnull(IRD.InventoryReceiveId,'null') IN ('" + POId + @"')";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public GridModel QueryOnlyPOBOQ(GridParameter parameters, string inveReveiveId, string AcceptanceId)
        {
            string paramter = "";
            string paramter1 = "";
            if (inveReveiveId != "")
            {
                if (paramter == "")
                {
                    paramter += "IRD.InventoryReceiveId in(" + inveReveiveId + ")";
                    paramter1 += "POId in(" + inveReveiveId + ")";
                }
                else
                {

                    paramter += " AND IRD.InventoryReceiveId in(" + inveReveiveId + ")";
                    paramter1 += "POId in(" + inveReveiveId + ")";
                }
            }

            try
            {
                if (AcceptanceId == "undefined")
                    AcceptanceId = null;
                if (AcceptanceId == "null")
                    AcceptanceId = null;
                if (AcceptanceId == "")
                    AcceptanceId = null;
                if (inveReveiveId == "'','undefined'")
                    inveReveiveId = null;
                if (!string.IsNullOrEmpty(inveReveiveId) && AcceptanceId == null)
                {


                    parameters.CmdText = @"DECLARE @totalReceiveAmount DECIMAL(18, 4)=0
	                                  , @totalServiceAmount DECIMAL(18, 4)=0
	                                  , @totalSvcTaxAmount DECIMAL(18, 4)=0
                       -- SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(TransactionAmount, 0)),1) FROM [TRN].[PurchaseOrderDetail] WHERE " + paramter + @")
                        --SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount - GRNServiceAmount, 0)),0) As Amount FROM [TRN].[POService] WHERE " + paramter + @")
                        --SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[PurchaseOrderTax] WHERE " + paramter + @" AND InventoryServiceId<>'')
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
                            --, ISNULL(IRD.GRNRcvQty,0) AS GRNRcvQty 
							 , ISNULL(aa.TransactionQty,0) AS GRNRcvQty        
                            --,(IRD.TransactionQty - ISNULL(IRD.GRNRcvQty,0)) AS TransactionQty
                              ,'' AS TransactionQty
                             ,(IRD.TransactionQty-ISNULL(aa.TransactionQty,0)) As Balance
                            ,ISNULL(IRD.QtyStatus,0) QtyStatus
                            , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM, IRD.TransactionRate, CU.Code AS CurrencyName, IR.ToCurrencyRate
                            ,IRD.TransactionAmount
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
                           ,0 ShortageQty
						   ,0 RejectionQty
                           --,MRD.MaterialDetail
                           ,null AS [check] ,IRD.Description MaterialDetail,'null' PurchaseDocAcceptanceDetailId,0 POClosStatus,C.UserName CountryName,C.Id CountryId ,MM.IsAsset,IRD.TotalTaxAmount,0 GrossAmount,0 DiscountAmount,'' QualityStatus
						,IRD.TransactionUoMId POUoMId,IRD.Tolerance,IRD.RefferenceNo
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
                        left join scs.country C On C.Id=IRD.CountryId		
						LEFT JOIN (Select PODetailsId ,Sum(TransactionQty) TransactionQty from trn.InventoryReceiveDetail where " + paramter1 + @" group by PODetailsId) aa ON  aa.PODetailsId=IRD.Id	
                        WHERE  IRD.QtyStatus=0 and IRD.InventoryMaterialId is not null 	AND " + paramter + @"			
					
						
               Union ALL
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
                            ,IRD.TransactionAmount
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
                            ,0 ShortageQty
						   ,0 RejectionQty
                           --,MRD.MaterialDetail
                           ,null AS [check] ,IRD.Description MaterialDetail,'null' PurchaseDocAcceptanceDetailId,0 POClosStatus,C.UserName CountryName,C.Id CountryId ,MM.IsAsset,IRD.TotalTaxAmount,0 GrossAmount,0 DiscountAmount,'' QualityStatus
							,IRD.TransactionUoMId POUoMId,IRD.Tolerance,IRD.RefferenceNo
					    
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
                        left join scs.country C On C.Id=IRD.CountryId	
						
                        WHERE IRD.QtyStatus=0 and IRD.InventoryMaterialId is null AND " + paramter + @"";
                }
                else
                {
                    parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + AcceptanceId + @"'
	                                  , @totalReceiveAmount DECIMAL(18, 4)=0
	                                  , @totalServiceAmount DECIMAL(18, 4)=0
	                                  , @totalSvcTaxAmount DECIMAL(18, 4)=0
                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(TransactionAmount, 0)),1) FROM [TRN].[PurchaseOrderDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount - GRNServiceAmount, 0)),0) As Amount FROM [TRN].[POService] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
                            SELECT 
                              --IM.Id
                             IR.Id AS POID
							 ,IRD.Id AS PODetailsID
							 ,PDAD.Id PurchaseDocumentAcceptanceDetailId 
							 ,PDAD.PurchaseDocAcceptanceId PurchaseDocumentAcceptanceId
                            ,IRD.Id AS InventoryReceiveDetailId
                            , MGM.UserName AS MaterialGroupMasterName
                            , MM.Id MaterialMasterId
							, MM.UserName
                            ,IRD.MaterialStorageId
                            ,IRD.BaseUOMId
                            , PDAD.ArticleId, ART.StandardName
                            , PDAD.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , PDAD.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , PDAD.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , PDAD.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , PDAD.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , PDAD.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
                            ,IRD.TransactionQty AS OriginalPOQty
                            , PDAD.TransactionQty AS POQty
                             ,ISNULL(PDAD1.OtherReceive,0) AS GRNRcvQty                                 
                            --,(IRD.TransactionQty - ISNULL(IRD.GRNRcvQty,0)) AS TransactionQty
                              ,0 AS TransactionQty
                              ,(ISNULL(IRD.TransactionQty,0)-(ISNULL(PDAD.TransactionQty,0)+ISNULL(PDAD1.OtherReceive,0))) As Balance
							 ,PDAD.TransactionQty  ApprovedQty
							 ,PDAD.TransactionQty  NetQty
							 , IRD.TransactionRate
							 ,IRD.TransactionRate *PDAD.TransactionQty TrnAmount
							 ,PDAD.TaxAmount BaseTaxAmount
							  ,PDAD.ChargesTranAmount ServiceCharge
							 ,PDAD.ChargesTaxTranAmount ServiceTax
							  ,PDAD.TotalMaterialTranAmount TotalMaterialTranAmount
							 ,PDAD.TotalMaterialTranAmount * PDA.AcceptanceRate TotalMaterialBaseAmount
                            ,ISNULL(IRD.QtyStatus,0) QtyStatus
                            , IRD.TransactionUoMId
							, TUoM.UserName AS TransactionUoM
							
							, CU.Code AS CurrencyName
							, IR.ToCurrencyRate 
					       ,IRD.TransactionAmount
                            ,0 AS TrnAmount  
                            
                            ,0 AS TaxAmount
	                        , 0 AS ChargesAmount
                            
	                        , IRD.CountryId
                            ,'True' enableid
                            ,null POMaterialTaxList                            
                            ,0 AS TotalMaterialTranAmount
                            , 0 AS ToTalMaterialBooksCurrencyAmount
                           ,IR.InvoicingByAddress,IR.DeliveryByAddress
                           ,IRD.RequisitionId
						   ,IRD.RequisitionDetailId
                            ,0 ShortageQty
						   ,0 RejectionQty
                           --,MRD.MaterialDetail
                           ,null AS [check] ,IRD.Description MaterialDetail
                           ,IsNonCreditable= CASE WHEN PDA.IsNonCreditable=1 then 1 Else 0 END ,MM.IsAsset ,CU.Id CurrencyId,IRD.BaseUOMId POUoMId
                        FROM TRN.PurchaseDocAcceptanceDetail AS PDAD						
                        left JOIN MST.MaterialMaster AS MM ON PDAD.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON PDAD.ArticleId=ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON PDAD.FirstCharacteristicsId=FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON PDAD.SecondCharacteristicsId=SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON PDAD.ThirdCharacteristicsId=TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON PDAD.FirstCharacteristicsValueId=FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON PDAD.SecondCharacteristicsValueId=SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON PDAD.ThirdCharacteristicsValueId=TCV.Id
                       -- JOIN [TRN].[PurchaseOrderDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON PDAD.TransactionUoMId=TUoM.Id
						LEFT JOIN TRN.PurchaseDocAcceptance AS PDA ON PDA.Id=PDAD.PurchaseDocAcceptanceId
						LEFT JOIN [TRN].[PurchaseOrder] AS IR ON IR.Id=PDAD.POId
						LEFT JOIN TRN.PurchaseOrderDetail AS IRD ON IRD.id=pDAD.PODetailId  
                        --left join (select PODetailsId,sum(TransactionQty) OtherReceive from TRN.InventoryReceiveDetail group by PODetailsId) AS PDAD1 ON PDAD1.PODetailsId =IRD.id
						left join (select PurchaseDocumentAcceptanceDetailId,PurchaseDocumentAcceptanceId ,sum(TransactionQty) OtherReceive from TRN.InventoryReceiveDetail where PurchaseDocumentAcceptanceId !=@inventoryReceiveId  group by PurchaseDocumentAcceptanceDetailId,PurchaseDocumentAcceptanceId) AS PDAD1 ON PDAD1.PurchaseDocumentAcceptanceDetailId =PDAD.id
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT join [trn].MaterialRequsitionDetails MRD on MRD.Id=IRD.RequisitionDetailId
                        WHERE PDA.Id=@inventoryReceiveId 
                       --and IRD.QtyStatus=0 and IRD.InventoryMaterialId is not null
                       ";
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
    }
}
