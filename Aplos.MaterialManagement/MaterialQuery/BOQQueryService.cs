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

        public IEnumerable<object> GetSpecificMaterialStockBOQ(string companyId, string plantId,string POId, string ContractId, string masterOrderitemId, string salesOrderId, string issueDate)
        {
            try
            {
                var sql = "";
               
                    sql = @"select * from(
                        SELECT IRD.InventoryReceiveId, IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId,grnmap.BOQDetailId
 , IRD.InventoryMaterialId, P.Code AS PartyCode, P.UserName AS PartyName
	                     , IsPosting=CASE WHEN IR.[Status] IS NULL THEN 0 else 1 END
						, IsApproved=CASE WHEN IR.IsApproved= 0 THEN 0 else 1 END
						, IR.Id AS GRNNo, IRD.POId AS PONo, TUoM.UserName AS TUoM, BUoM.UserName AS BUoM, IRD.TransactionUoMId,  IRD.BaseUOMId, IRD.BaseUoMFactor,IRD.BaseUoMFactor GRNBaseUoMFactor
                        , round(IRD.MaterialTranRate,4) MaterialTranRate,  TCU.Code AS TCurrency, BCU.Code AS BCurrency, IRD.MaterialTranAmount
                        --, BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.MaterialTranAmount/IRD.BaseQty ELSE IRD.BooksCurrencyBaseRate END
						, IRD.TrnCurrencyBaseRate BaseRate
                        , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
                        --,Round((IRD.MaterialTranRate * IR.ToCurrencyRate),4) 
						 ,Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4) BaseCurrencyRate
						, IRD.TransactionQty, IRD.BaseQty,grnmap.BaseQty GRNBOQQty
						,ISNULL(IRD.BaseQty,0) - ISNULL(II.IssueQty, 0) StockQty
						, ISNULL(II.IssueQty,0) IssueQty, ISNULL(II.IssueQty,0) BaseIssueQty, ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty,ISNULL(IRD.IssueReturnQty,0) IssueReturnQty,ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty,ISNULL(IRD.InventorySalesQty,0) InventorySalesQty,ISNULL(IRD.InventoryScrapQty,0) InventoryScrapQty,ISNULL(IRD.InventoryTransferQty,0) InventoryTransferQty
						 ,((((((ISNULL(IRD.BaseQty,0) - ISNULL(II.IssueQty, 0)-ISNULL(IRD.PurchaseReturnQty,0))+ISNULL(IRD.IssueReturnQty,0))-ISNULL(IRD.ReductionByAdjustmentQty,0))-ISNULL(IRD.InventorySalesQty,0))-ISNULL(IRD.InventoryScrapQty,0))-ISNULL(IRD.InventoryTransferQty,0)) AS BalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						 ,Round(ISNULL(IRD.BooksCurrencyBaseRate,0),4) BooksCurrencyBaseRate
						 ,round(ISNULL(IRD.TrnCurrencyBaseRate,0),4) TrnCurrencyBaseRate
                        ,round(ISNULL(II.IssueAmount,0),4) TotalIssueAmount
                         ,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END
                        ,C.Id CountryId,C.UserName CountryName--,null AS [Flag] 
                        ,0 SalesRate
						,0 TotalAmount
                        ,IM.MaterialMasterId
						,MM.UserName MaterialMasterName
						,IM.ArticleId
						,MMA.StandardName ArticleName
						,IM.FirstCharacteristicsValueId
						,IM.SecondCharacteristicsValueId
						,IM.ThirdCharacteristicsValueId
						
						,IM.FirstCharacteristicsId
						,IM.SecondCharacteristicsId
						,IM.ThirdCharacteristicsId,IssueByUoM=CASE WHEN MM.IssueByUoM=0 THEN 'No' ELSE 'Yes' END
                        ,TrasactopmUomQty=(((ISNULL(IRD.BaseQty,0) - ISNULL(II.IssueQty, 0))*BaseUoMFactor)/BaseUoMFactor) 
						--,0 TrasactopmUomQty
						,'' IssueTransactionUoMId
						,'' IssueTransactionUoM
                    FROM TRN.GRNPORequisitionAllocation grnmap
					join [TRN].[InventoryReceiveDetail] AS IRD  on grnmap.InventoryReceiveDetailId=ird.Id
                    left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
					LEFT JOIN mst.MaterialMaster MM ON MM.Id=Im.MaterialMasterId
					LEFT JOIN mst.MaterialMasterArticle MMA ON MMA.Id=Im.ArticleId
                    left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                    left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
					LEFT JOIN (
									    select IID.InventoryMaterialId,IH.InventoryReceiveDetailId, II.MaterialStorageId
                                        , Sum(ISNULL(IH.Qty,0)) IssueQty , Sum(ISNULL(IH.TotalMaterialBooksCurrencyAmount,0)) IssueAmount,IID.IsAsset
									    FROM TRN.InventoryIssueDetail IID  
									    LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									    LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									    WHERE --convert(Date,II.IssueDate) <= CAST('" + issueDate + @"' AS DATE)  AND 
                                        II.PlantId='" + plantId + @"'   
									    GROUP BY IID.InventoryMaterialId,IID.IsAsset,IH.InventoryReceiveDetailId, II.MaterialStorageId
									    ) II ON II.InventoryReceiveDetailId=IRD.Id and II.MaterialStorageId=IRD.MaterialStorageId 
                    left JOIN SCS.Country C On C.Id=IM.CountryId
                    WHERE  IM.CompanyId='" + companyId + "' AND IM.PlantId='"+plantId+@"'
                    --AND IR.[Status]='Posting' AND IR.IsFOC=0
                    ----AND ISNULL(IM.ArticleId,'')='5777' AND ISNULL(IM.FirstCharacteristicsValueId,'')='423' AND  ISNULL(IM.SecondCharacteristicsValueId,'')=''
                    --AND ISNULL(IM.ThirdCharacteristicsValueId,'')='' AND ISNULL(IM.CountryId,'')='' 
					AND IRD.MaterialStorageId='7' 
                    --AND ISNULL(IRD.IssueQty, 1)>0 
					AND (IRD.POId IN ("+POId+ @") OR IRD.POId IN (''))
					AND IRD.BaseQty !=ISNULL(II.IssueQty,0)
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

    }
}
