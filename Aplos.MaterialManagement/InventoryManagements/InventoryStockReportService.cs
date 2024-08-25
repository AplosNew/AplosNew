using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;

#region Using

using Library.Service.Enums;
using Library.Service.Logs;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.Collections.Specialized;
using System.Linq;
#endregion Using

namespace Library.MaterialManagement.InventoryManagements
{
    public class InventoryStockReportService
    {
        private readonly SqlRepository _sqlRepository;

        #region Constructor
        public InventoryStockReportService()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion Constructor

        public void CreateMaterialStockBalanceSheet(ref IWorksheet sheet1, ref IWorksheet sheet2, ReportUtility report, string sheet1Name, string sheet2Name, string CompanyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string Asset, string Inventory, string Country, string materialStorage)
        {

            var cmdText = "";
            //#region Asset/Inventory
            var assetInvStatus = "";
            if (Asset == "true" && Inventory == "true")
            { assetInvStatus = " "; }
            else if (Asset == "true") { assetInvStatus = "AND MM.IsAsset=1"; } else { assetInvStatus = "AND MM.IsAsset=0"; }

                
                if (materialStorage == "false")
                {
                    cmdText = @"SELECT * FROM (
						SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]             
							  , isnull(MT.UserName,'') MaterialType
							  , isnull(MGM.UserName,'') AS MaterialGroup						
							 ,isnull(MM.UserName,'') MaterialMasterName	
                               ,MC.UserName MaterialCategory
							 ,MM.Id	MaterialMasterId	
							,HSNC.Code HSNCode
							,isnull( ART.StandardName,'') ArticleName	
							 ,ART.Id ArticleId		
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
							,TUoM.UserName UOM,  MM.IsAsset
							
                            --Opening Balance
                           ,isnull(opbal.TransactionQty,0) As OpeningBalance	
							,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount

						    --Receive

							,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
							--Issue
                         
							,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--Issue Return
							,isnull(IssueReturnData.Qty,0) IssueReturnQtyForThePeriod	
							,isnull(IssueReturnData.IssueReturnAmount,0) IssueReturnForThePeriodAmount	

							--Purchase Return
							,isnull(PurchaseReturnData.Qty,0) PurchaseReturnQtyForThePeriod	
							,isnull(PurchaseReturnData.PurchaseReturnAmount,0) PurchaseReturnForThePeriodAmount	

							--Adjust Return
							,isnull(AdjustmentData.Qty,0) AdjustmentQtyForThePeriod	
							,isnull(AdjustmentData.AdjustmentAmount,0) AdjustmentForThePeriodAmount	

                               --Inventory Sales
							,isnull(InventorySalesData.Qty,0) InventorySalesQtyForThePeriod	
							,isnull(InventorySalesData.InventorySalesAmount,0) InventorySalesForThePeriodAmount	
							--Inventory Scrap
							,isnull(InventoryScrapData.Qty,0) InventoryScrapQtyForThePeriod	
							,isnull(InventoryScrapData.InventoryScrapAmount,0) InventoryScrapForThePeriodAmount	

							--Inventory Transfer Data
							,isnull(InventoryTransferData.CapitalizeQty,0) InventoryTransferQtyForThePeriod	
							,isnull(InventoryTransferData.CapitalizeAmount,0) InventoryTransferForThePeriodAmount

							--Balance    
							,(((((((isnull(opbal.TransactionQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0)-isnull(PurchaseReturnData.Qty,0))-isnull(AdjustmentData.Qty,0))+isnull(IssueReturnData.Qty,0))-isnull(InventorySalesData.Qty,0))-isnull(InventoryScrapData.Qty,0))-isnull(InventoryTransferData.CapitalizeQty,0)) Closing 
							,Round((((((((isnull(opbal.TotalMaterialBooksCurrencyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(opbal2.ShortageValue,0)-isnull(IFD1.PolicyAmount,0)-isnull(AdjustmentData.AdjustmentAmount,0))-isnull(PurchaseReturnData.PurchaseReturnAmount,0))+isnull(IssueReturnData.IssueReturnAmount,0)-isnull(InventorySalesData.InventorySalesAmount,0))-isnull(InventoryScrapData.InventoryScrapAmount,0))-isnull(InventoryTransferData.CapitalizeAmount,0))),2) ClosingAmount

						from TRN.InventoryMaterial AS IM
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
						left join hkp.MaterialCategory as MC on MC.Id = MM.MaterialCategoryId
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						left join( 
			                        SELECT IRD.InventoryMaterialId ,Sum(IRD.BaseQty-ISNULL(IRD.ShortageQty,0)) AS TransactionQty, Sum((ROUND(IRD.BaseQty*ird.MaterialTranRate*IR.ToCurrencyRate,2)+IRD.ChargesTranAmount)) TotalMaterialBooksCurrencyAmount,SUM(ROUND(IRD.ShortageQty*ird.MaterialTranRate,2)) ShortageValue
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.OpeningBalanceId IS NOT NULL 
                                    --AND IRD.Id  NOT IN (SELECT ISNULL(InventoryReceiveDetailId,'') FROM [TRN].[CapitalizationMasterDetail]  where InventoryIssueHistoryId IS NULL)
									group By IRD.InventoryMaterialId
                                    ) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"' 
                        
                        LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id
						left join(  select x.InventoryMaterialId,sum(x.transactionqty) transactionqty,sum(x.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount, SUM(X.ShortageValue) ShortageValue from (
                                    SELECT IRD.InventoryMaterialId,  IRD.BaseQty-ISNULL(IRD.ShortageQty,0)  AS TransactionQty ,   (ROUND(IRD.BaseQty*ird.MaterialTranRate*IR.ToCurrencyRate,2)+IRD.ChargesTranAmount) TotalMaterialBooksCurrencyAmount, ROUND(IRD.ShortageQty*ird.MaterialTranRate,2) ShortageValue
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) > '" + toDate + @"'   --AND IRD.Id  NOT IN (SELECT ISNULL(InventoryReceiveDetailId,'') FROM [TRN].[CapitalizationMasterDetail]  where InventoryIssueHistoryId IS NULL) --) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                                    UNION ALL
                                    SELECT IRD.InventoryMaterialId, IRD.BaseQty AS TransactionQty ,  (ROUND(IRD.BaseQty*ird.MaterialTranRate*IR.ToCurrencyRate,2)+IRD.ChargesTranAmount) TotalMaterialBooksCurrencyAmount, ROUND(IRD.ShortageQty*ird.MaterialTranRate,2) ShortageValue
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) = '" + toDate + @"'  
                                    ) x
									group by x.InventoryMaterialId) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

						left join(SELECT IRD.InventoryMaterialId, Sum(IRD.BaseQty-ISNULL(IRD.ShortageQty,0)) AS TransactionQty ,  Sum((ROUND(IRD.BaseQty*ird.MaterialTranRate*IR.ToCurrencyRate,2)+IRD.ChargesTranAmount)) TotalMaterialBooksCurrencyAmount,SUM(IRD.ShortageQty*IRD.MaterialTranRate) ShortageValue
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate)	<= '" + toDate + @"'  AND IR.OpeningBalanceId IS  NULL --AND IRD.Id  NOT IN (SELECT ISNULL(InventoryReceiveDetailId,'') FROM [TRN].[CapitalizationMasterDetail]  WHERE InventoryIssueHistoryId IS NULL)
                                    --AND (ISNULL(IR.AuthorizedByStatus,'')!='Reject') AND   ISNULL(IR.CheckedByStatus,'')!='Reject'
                                    GROUP BY IRD.InventoryMaterialId
                                    ) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
         
						left join (select IID.InventoryMaterialId, Sum(IH.Qty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"'  GROUP BY IID.InventoryMaterialId
									) IFD On IFD.InventoryMaterialId=IM.Id

						left join (select IID.InventoryMaterialId, Sum(IH.Qty) IssueQty , Sum(IH.TotalAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
								WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' 
                                AND ISNULL(IH.inventoryreceivedetailId,'')  NOT IN (SELECT inventoryreceivedetailId FROM [TRN].[CapitalizationMasterDetail] where  InventoryIssueHistoryId is null and Source='AUC' )
                                GROUP BY IID.InventoryMaterialId
								) IFD1 On IFD1.InventoryMaterialId=IM.Id


                       --Issue Return
                        Left join (select IH.InventoryMaterialId,sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty*IRD.MaterialTranRate)) IssueReturnAmount from trn.InventoryIssueReturnHistory IH
									 Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
					    --Purchase return
                       Left join (select IH.InventoryMaterialId,sum(IH.TransactionQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum((IH.TransactionQty*IRD.MaterialTranRate)+IH.ChargesTranAmount)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id

                       -- Adjustment
						Left join (select psad.InventoryMaterialId,sum(IH.Qty) Qty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) AdjustmentAmount 
					                 from trn.PhysicalStockAdjustmentHistory IH
									 Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
									 Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY psad.InventoryMaterialId
								 )AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id

						-- InventorySales
						Left join (select ISD.InventoryMaterialId,sum(ISH.Qty) Qty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty*ISH.BaseRate)) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) <= '" + toDate + @"' AND Ins.PlantId='" + plantId + @"' GROUP BY ISD.InventoryMaterialId
								 )InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                -- where IM.PlantId='20201' AND MM.UserName is not null --AND MM.UserName like '%Bed Sheet%'

                   --InventoryScrap
								Left join (select ISCD.InventoryMaterialId,sum(ISCH.Qty) Qty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty*ISCH.Rate)) InventoryScrapAmount 
					                 from [TRN].[InventoryScrapHistory] ISCH
									 Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
									 Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,ISC.ScrapDate) <= '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' GROUP BY ISCD.InventoryMaterialId
								 )InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id   
					--Capitalize
								Left join ( SELECT IRD.InventoryMaterialId,IR.PlantId,SUM(IRD.TransactionQty) CapitalizeQty,SUM(CMD.Amount) CapitalizeAmount 
											FROM  [TRN].[CapitalizationMasterDetail] CMD 
											JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=CMD.InventoryReceiveDetailId 
											JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId 
											JOIN [TRN].[CapitalizationMaster] CM ON CM.Id=CMD.CapitalizationMasterId
											where CMD.InventoryIssueHistoryId IS NULL AND convert(Date,CM.CapitalizationDate) <= '" + toDate + @"' and IR.PlantId='" + plantId + @"'
									 GROUP BY IRD.InventoryMaterialId,IR.PlantId
								 )InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id      
                                      
                                 where   IM.PlantId='" + plantId + @"' AND MM.UserName is not null " + assetInvStatus + ") X    ";

                }
                else
                {
                    cmdText = @"SELECT * FROM (
						SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]             
							  ,isnull(MT.UserName,'') MaterialType
							 , isnull(MGM.UserName,'') AS MaterialGroup						
							 ,isnull(MM.UserName,'') MaterialMasterName	
                               ,MC.UserName MaterialCategory
							 ,MM.Id	MaterialMasterId,MaterialStorageLocation= MS.UserName	
							,HSNC.Code HSNCode
							,isnull( ART.StandardName,'') ArticleName	
							 ,ART.Id ArticleId		
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
							,TUoM.UserName UOM,  MM.IsAsset
							
                            --Opening Balance
                           ,isnull(opbal.TransactionQty,0) As OpeningBalance	
							,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount

						    --Receive

							,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
							--Issue
                         
							,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--Issue Return
							,isnull(IssueReturnData.Qty,0) IssueReturnQtyForThePeriod	
							,isnull(IssueReturnData.IssueReturnAmount,0) IssueReturnForThePeriodAmount	

							--Purchase Return
							,isnull(PurchaseReturnData.Qty,0) PurchaseReturnQtyForThePeriod	
							,isnull(PurchaseReturnData.PurchaseReturnAmount,0) PurchaseReturnForThePeriodAmount	

							--Adjust Return
							,isnull(AdjustmentData.Qty,0) AdjustmentQtyForThePeriod	
							,isnull(AdjustmentData.AdjustmentAmount,0) AdjustmentForThePeriodAmount	

                               --Inventory Sales
							,isnull(InventorySalesData.Qty,0) InventorySalesQtyForThePeriod	
							,isnull(InventorySalesData.InventorySalesAmount,0) InventorySalesForThePeriodAmount	
							--Inventory Scrap
							,isnull(InventoryScrapData.Qty,0) InventoryScrapQtyForThePeriod	
							,isnull(InventoryScrapData.InventoryScrapAmount,0) InventoryScrapForThePeriodAmount	

								--Inventory Transfer Data
							,isnull(InventoryTransferData.CapitalizeQty,0) InventoryTransferQtyForThePeriod	
							,isnull(InventoryTransferData.CapitalizeAmount,0) InventoryTransferForThePeriodAmount

							--Balance    
							,(((((((isnull(opbal.TransactionQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0)-isnull(PurchaseReturnData.Qty,0))-isnull(AdjustmentData.Qty,0))+isnull(IssueReturnData.Qty,0))-isnull(InventorySalesData.Qty,0))-isnull(InventoryScrapData.Qty,0))-isnull(InventoryTransferData.CapitalizeQty,0)) Closing 
							,(((((((isnull(opbal.TotalMaterialBooksCurrencyAmount-opbal.ShortageValue,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount-opbal2.ShortageValue,0))-isnull(IFD1.PolicyAmount,0)-isnull(AdjustmentData.AdjustmentAmount,0))-isnull(PurchaseReturnData.PurchaseReturnAmount,0))+isnull(IssueReturnData.IssueReturnAmount,0)-isnull(InventorySalesData.InventorySalesAmount,0))-isnull(InventoryScrapData.InventoryScrapAmount,0))-isnull(InventoryTransferData.CapitalizeAmount,0))) ClosingAmount


						from TRN.InventoryMaterial AS IM
                        left join (select distinct InventoryMaterialId,MaterialStorageId from trn.InventoryReceiveDetail) as IRS on im.Id=irs.InventoryMaterialId
						left join [HKP].[MaterialStorage] MS on ms.id=irs.MaterialStorageId
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
						left join hkp.MaterialCategory as MC on MC.Id = MM.MaterialCategoryId
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						left join( 
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId
                                    ,Sum(IRD.BaseQty-ISNULL(IRD.ShortageQty,0)) AS TransactionQty, Sum((ROUND(IRD.BaseQty*ird.MaterialTranRate*IR.ToCurrencyRate,2)+IRD.ChargesTranAmount)) TotalMaterialBooksCurrencyAmount,SUM(ROUND(IRD.ShortageQty*ird.MaterialTranRate,2)) ShortageValue
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        WHERE convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.OpeningBalanceId IS NOT NULL  
									group By IRD.InventoryMaterialId,IRD.MaterialStorageId
                                    ) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"' and opbal.MaterialStorageId=IRS.MaterialStorageId
                        
                        LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id

						left join( select x.InventoryMaterialId,x.MaterialStorageId,sum(x.TransactionQty) TransactionQty
						,sum(x.TotalMaterialBooksCurrencyAmount)TotalMaterialBooksCurrencyAmount,SUM(x.ShortageValue) ShortageValue from ( 
						
						SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId, (IRD.BaseQty-ISNULL(IRD.ShortageQty,0)) AS TransactionQty 
						,  ((ROUND(IRD.BaseQty*ird.MaterialTranRate*IR.ToCurrencyRate,2)+IRD.ChargesTranAmount)) TotalMaterialBooksCurrencyAmount
						,(ROUND(IRD.ShortageQty*ird.MaterialTranRate,2)) ShortageValue
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) > '" + toDate + @"'   --group By IRD.InventoryMaterialId, IRD.MaterialStorageId
                                    UNION ALL
                                    SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId, (IRD.BaseQty-ISNULL(IRD.ShortageQty,0)) AS TransactionQty 
									,  ((ROUND(IRD.BaseQty*ird.MaterialTranRate*IR.ToCurrencyRate,2)+IRD.ChargesTranAmount)) TotalMaterialBooksCurrencyAmount,(ROUND(IRD.ShortageQty*ird.MaterialTranRate,2)) ShortageValue
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) = '" + toDate + @"' 
									) x group By x.InventoryMaterialId, x.MaterialStorageId
									) AS opbal1 
                                    ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"' and opbal1.MaterialStorageId=IRS.MaterialStorageId


						left join(SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId, Sum(IRD.BaseQty-ISNULL(IRD.ShortageQty,0)) AS TransactionQty ,  Sum((ROUND(IRD.BaseQty*ird.MaterialTranRate*IR.ToCurrencyRate,2)+IRD.ChargesTranAmount)) TotalMaterialBooksCurrencyAmount,SUM(ROUND(IRD.ShortageQty*ird.MaterialTranRate,2)) ShortageValue
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate)	<= '" + toDate + @"'   AND IR.OpeningBalanceId IS  NULL 
                                    --AND (ISNULL(IR.AuthorizedByStatus,'')!='Reject') AND   ISNULL(IR.CheckedByStatus,'')!='Reject'
                                    GROUP BY IRD.InventoryMaterialId, IRD.MaterialStorageId) AS opbal2 
                                        ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"' and opbal2.MaterialStorageId=IRS.MaterialStorageId
         
						left join (select IID.InventoryMaterialId, IH.MaterialStorageId, Sum(IH.Qty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"'  GROUP BY IID.InventoryMaterialId, IH.MaterialStorageId
									) IFD On IFD.InventoryMaterialId=IM.Id and IFD.MaterialStorageId=IRS.MaterialStorageId

						left join (select IID.InventoryMaterialId, IH.MaterialStorageId, Sum(IH.Qty) IssueQty , Sum(IH.TotalAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
								WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' 
                                AND ISNULL(IH.inventoryreceivedetailId,'')  NOT IN (SELECT inventoryreceivedetailId FROM [TRN].[CapitalizationMasterDetail] where  InventoryIssueHistoryId is null and Source='AUC' )
                                GROUP BY IID.InventoryMaterialId, IH.MaterialStorageId
								) IFD1 On IFD1.InventoryMaterialId=IM.Id and IFD1.MaterialStorageId=IRS.MaterialStorageId


                       --Issue Return
                        Left join (select IH.InventoryMaterialId,II.MaterialStorageId,sum(IH.Qty) Qty, (sum(IH.Qty*IRD.MaterialTranRate)) IssueReturnAmount  from trn.InventoryIssueReturnHistory IH
									 Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId,II.MaterialStorageId
								 )IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id and IssueReturnData.MaterialStorageId=IRS.MaterialStorageId
					    --Purchase return
                       Left join (select IH.InventoryMaterialId,II.MaterialStorageId,sum(IH.TransactionQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum((IH.TransactionQty*IRD.MaterialTranRate)+IH.ChargesTranAmount)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId,II.MaterialStorageId
								 )PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id and opbal2.MaterialStorageId=PurchaseReturnData.MaterialStorageId

                       -- Adjustment
						Left join (select psad.InventoryMaterialId,sum(IH.Qty) Qty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) AdjustmentAmount 
					                 from trn.PhysicalStockAdjustmentHistory IH
									 Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
									 Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY psad.InventoryMaterialId
								 )AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id
						-- InventorySales
						Left join (select ISD.InventoryMaterialId,Ins.MaterialStorageId,sum(ISH.Qty) Qty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty*ISH.BaseRate)) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) <= '" + toDate + @"' AND Ins.PlantId='" + plantId + @"' GROUP BY ISD.InventoryMaterialId,Ins.MaterialStorageId
								 )InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id and opbal2.MaterialStorageId=InventorySalesData.MaterialStorageId   

                   --InventoryScrap
								Left join (select ISCD.InventoryMaterialId,sum(ISCH.Qty) Qty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
					                 from [TRN].[InventoryScrapHistory] ISCH
									 Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
									 Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,ISC.ScrapDate) <= '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' GROUP BY ISCD.InventoryMaterialId
								 )InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id   
					--Capitalize
								Left join ( SELECT IRD.InventoryMaterialId,IRD.MaterialStorageId,IR.PlantId,SUM(IRD.TransactionQty) CapitalizeQty,SUM(CMD.Amount) CapitalizeAmount 
											FROM  [TRN].[CapitalizationMasterDetail] CMD 
											JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=CMD.InventoryReceiveDetailId 
											JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId 
											JOIN [TRN].[CapitalizationMaster] CM ON CM.Id=CMD.CapitalizationMasterId
											where CMD.InventoryIssueHistoryId IS NULL AND convert(Date,CM.CapitalizationDate) <= '" + toDate + @"' and IR.PlantId='" + plantId + @"'
									 GROUP BY IRD.InventoryMaterialId,IR.PlantId,IRD.MaterialStorageId
								 )InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id and opbal2.MaterialStorageId=InventoryTransferData.MaterialStorageId  
                                 where   IM.PlantId='" + plantId + @"' AND MM.UserName is not null  " + assetInvStatus + ") X   ";
                }

            
            var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);
            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();


            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");

            var _rowd = 4;

            if (fromDate != "" && toDate != "")
            {
                sheet1[_rowd, 3].Text = fromDate + " " + "To" + " " + toDate;
                sheet1.Range[_rowd, 3, _rowd, 6].CellStyle.Font.Size = 8;
                sheet1.Range[_rowd, 3, _rowd, 6].CellStyle.Font.Bold = false;
                sheet1.Range[_rowd, 3, _rowd, 6].Merge();
                //sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }

            else
            {
                sheet1.Range[_rowd, 3, _rowd, 4].Text = toDate;
                sheet1.Range[_rowd, 3, _rowd, 4].CellStyle.Font.Size = 8;
                sheet1.Range[_rowd, 3, _rowd, 4].CellStyle.Font.Bold = false;
                sheet1.Range[_rowd, 3, _rowd, 4].Merge();
                //sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

            }
            var _rows = 5;
            sheet1[_rows, 3].Text = "Report Ref No: ";
            sheet1.Range[_rows, 3, _rows, 6].CellStyle.Font.Size = 8;
            sheet1.Range[_rows, 3, _rows, 6].Merge();
            sheet1.Range[_rows, 3].CellStyle.Font.Bold = false;

            var _row = 7;
            var _rowL = _row;
            var row = _row + 1;

            var sheet1headreColIndex = 1;
            _rowL += 1;
            row++;

            int StartRange = 0;
            int EndRange = 0;
            if (fromDate != "" && toDate != "")
            {

                if (Amount != "" && Qty != "")
                {
                    if (Country == "true")
                    {
                        sheet1.Range[_row, 15, _row, 16].Text = "Opening Balance";
                        sheet1.Range[_row, 15, _row, 16].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 15, _row, 16].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 15, _row, 16].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 15, _row, 16].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 15, _row, 16].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 15, _row, 16].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 15, _row, 16].Merge();

                        sheet1.Range[_row, 17, _row, 18].Text = "Material Receipts";
                        sheet1.Range[_row, 17, _row, 18].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 17, _row, 18].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 17, _row, 18].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 17, _row, 18].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 17, _row, 18].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 17, _row, 18].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 17, _row, 18].Merge();



                        sheet1.Range[_row, 19, _row, 20].Text = "Issue Material";
                        sheet1.Range[_row, 19, _row, 20].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 19, _row, 20].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 19, _row, 20].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 19, _row, 20].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 19, _row, 20].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 19, _row, 20].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 19, _row, 20].Merge();

                        sheet1.Range[_row, 21, _row, 22].Text = "Issue Material Return";
                        sheet1.Range[_row, 21, _row, 22].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 21, _row, 22].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 21, _row, 22].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 21, _row, 22].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 21, _row, 22].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 21, _row, 22].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 21, _row, 22].Merge();


                        sheet1.Range[_row, 23, _row, 24].Text = "Purchase Material Return";
                        sheet1.Range[_row, 23, _row, 24].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 23, _row, 24].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 23, _row, 24].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 23, _row, 24].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 23, _row, 24].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 23, _row, 24].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 23, _row, 24].Merge();

                        sheet1.Range[_row, 25, _row, 26].Text = "Adjustment Material";
                        sheet1.Range[_row, 25, _row, 26].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 25, _row, 26].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 25, _row, 26].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 25, _row, 26].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 25, _row, 26].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 25, _row, 26].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 25, _row, 26].Merge();

                        sheet1.Range[_row, 27, _row, 28].Text = "Inventory Sales";
                        sheet1.Range[_row, 27, _row, 28].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 27, _row, 28].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 27, _row, 28].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 27, _row, 28].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 27, _row, 28].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 27, _row, 28].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 27, _row, 28].Merge();


                        sheet1.Range[_row, 29, _row, 30].Text = "Inventory Scrap";
                        sheet1.Range[_row, 29, _row, 30].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 29, _row, 30].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 29, _row, 30].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 29, _row, 30].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 29, _row, 30].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 29, _row, 30].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 29, _row, 30].Merge();

                        sheet1.Range[_row, 31, _row, 32].Text = "Inventory Transfer";
                        sheet1.Range[_row, 31, _row, 32].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 31, _row, 32].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 31, _row, 32].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 31, _row, 32].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 31, _row, 32].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 31, _row, 32].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 31, _row, 32].Merge();

                        sheet1.Range[_row, 33, _row, 34].Text = "Closing Balance";
                        sheet1.Range[_row, 33, _row, 34].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 33, _row, 34].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 33, _row, 34].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 33, _row, 34].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 33, _row, 34].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 33, _row, 34].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 33, _row, 34].Merge();
                    }
                    else
                    {
                        if (Country == "undefined" || Country == "null") Country = "false";

                        StartRange = 13;
                        EndRange = StartRange + 1;
                        if (materialStorage == "undefined" || materialStorage == "null") materialStorage = "false";

                        if (materialStorage == "true")
                        {
                            StartRange = 14;
                            EndRange = StartRange + 1;
                        }

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Opening Balance";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;


                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Material Receipts";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Issue Material";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Issue Material Return";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Purchase Material Return";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Adjustment Material";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Inventory Sales";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Inventory Scrap";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Inventory Transfer";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();

                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;
                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Inventory Sales Return";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;
                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Closing Balance";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();

                    }


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SL";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 5;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Group";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Category";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Mat.Mst.Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                   

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 23;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    if (Country == "true")
                    {

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Country Name";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;

                    }

                    if (materialStorage == "true")
                    {

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Storage";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;

                    }

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "UoM";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Is Asset";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;
                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;

                    if (Country == "false")
                    {
                        //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Country Name");
                        //sheet1headreColIndex++;
                        sheet1headreColIndex++;
                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;

                        //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;


                    }

                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;

                }
                else if (Amount != "" && Qty == "")
                {

                    //               
                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SL";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 5;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Mat.Mst.Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 23;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    if (Country == "true")
                    {
                        //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Country Name");
                        //sheet1headreColIndex++;

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Country Name";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;



                    }

                    if (materialStorage == "true")
                    {

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Storage";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;

                    }

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Is Asset";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Opening Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;




                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Receipts Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Return Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Return Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Purchase Material Return Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Purchase Material Return Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Adjustment Material Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Adjustment Material Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Sales Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;




                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Scrap Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Transfer Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Closing Qty");


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Closing Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;


                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;

                }
                else if (Amount == "" && Qty != "")
                {
                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SL";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 5;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Group";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Category";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Mat.Mst.Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 23;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    if (Country == "true")
                    {
                        //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Country Name");
                        //sheet1headreColIndex++;

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Country Name";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;
                    }
                    if (materialStorage == "true")
                    {

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Storage";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;

                    }
                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "UOM");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "UOM";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Is Asset";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Opening Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Opening Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Receipts Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Receipts Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Return Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Return Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Purchase Material Return Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Purchase Material Return Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Adjustment Material Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Adjustment Material Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 24;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Sales Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;




                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Scrap Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Transfer Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 22;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Closing Qty");


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Closing Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;



                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;


                }
            }

            if (fromDate == "" && toDate != "")
            {


                if (Amount != "" && Qty != "")
                {
                    if (Country == "true")
                    {
                        sheet1.Range[_row, 14, _row, 15].Text = "Opening Balance";
                        sheet1.Range[_row, 14, _row, 15].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 14, _row, 15].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 14, _row, 15].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 14, _row, 15].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 14, _row, 15].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 14, _row, 15].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 14, _row, 14].Merge();

                        sheet1.Range[_row, 16, _row, 17].Text = "Material Receipts";
                        sheet1.Range[_row, 16, _row, 17].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 16, _row, 17].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 16, _row, 17].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 16, _row, 17].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 16, _row, 17].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 16, _row, 17].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 16, _row, 17].Merge();



                        sheet1.Range[_row, 18, _row, 19].Text = "Issue Material";
                        sheet1.Range[_row, 18, _row, 19].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 18, _row, 19].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 18, _row, 19].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 18, _row, 19].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 18, _row, 19].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 18, _row, 19].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 18, _row, 19].Merge();

                        sheet1.Range[_row, 20, _row, 21].Text = "Issue Material Return";
                        sheet1.Range[_row, 20, _row, 21].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 20, _row, 21].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 20, _row, 21].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 20, _row, 21].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 20, _row, 21].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 20, _row, 21].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 20, _row, 21].Merge();


                        sheet1.Range[_row, 22, _row, 23].Text = "Purchase Material Return";
                        sheet1.Range[_row, 22, _row, 23].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 22, _row, 23].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 22, _row, 23].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 22, _row, 23].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 22, _row, 23].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 22, _row, 23].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 22, _row, 23].Merge();

                        sheet1.Range[_row, 24, _row, 25].Text = "Adjustment Material";
                        sheet1.Range[_row, 24, _row, 25].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 24, _row, 25].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 24, _row, 25].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 24, _row, 25].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 24, _row, 25].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 24, _row, 25].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 24, _row, 25].Merge();

                        sheet1.Range[_row, 26, _row, 27].Text = "Inventory Sales";
                        sheet1.Range[_row, 26, _row, 27].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 26, _row, 27].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 26, _row, 27].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 26, _row, 27].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 26, _row, 27].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 26, _row, 27].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 26, _row, 27].Merge();


                        sheet1.Range[_row, 28, _row, 29].Text = "Inventory Scrap";
                        sheet1.Range[_row, 28, _row, 29].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 28, _row, 29].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 28, _row, 29].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 28, _row, 29].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 28, _row, 29].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 28, _row, 29].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 28, _row, 29].Merge();

                        sheet1.Range[_row, 30, _row, 31].Text = "Inventory Transfer";
                        sheet1.Range[_row, 30, _row, 31].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 30, _row, 31].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 30, _row, 31].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 30, _row, 31].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 30, _row, 31].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 30, _row, 31].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 30, _row, 31].Merge();

                        sheet1.Range[_row, 32, _row, 33].Text = "Closing Balance";
                        sheet1.Range[_row, 32, _row, 33].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 32, _row, 33].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 32, _row, 33].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 32, _row, 33].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 32, _row, 33].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 32, _row, 33].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 32, _row, 33].Merge();
                    }
                    else
                    {
                        StartRange = 15;
                        EndRange = StartRange + 1;
                        if (materialStorage == "true")
                        {
                            StartRange = 16;
                            EndRange = StartRange + 1;
                        }

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Opening Balance";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;
                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Material Receipts";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;


                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Issue Material";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;
                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Issue Material Return";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Purchase Material Return";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;
                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Adjustment Material";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;
                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Inventory Sales";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Inventory Scrap";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;
                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Capitalized";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;
                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Closing Balance";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                    }

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SL";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 5;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Group";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Category";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Mat.Mst.Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 23;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    if (Country == "true")
                    {
                        //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Country Name");
                        //sheet1headreColIndex++;

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Country Name";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;


                    }
                    if (materialStorage == "true")
                    {

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Storage";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;

                    }

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "UOM";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Is Asset";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;
                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;



                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;


                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;

                }
                else if (Amount != "" && Qty == "")
                {

                    //               
                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SL";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 5;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Mat.Mst.Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 23;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;
                    if (Country == "true")
                    {
                        //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Country Name");
                        //sheet1headreColIndex++;

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Country Name";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;
                    }
                    if (materialStorage == "true")
                    {

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Storage";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;

                    }
                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Is Asset";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Opening Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Opening Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Receipts Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Receipts Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;




                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Return Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Return Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Purchase Material Return Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Purchase Material Return Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Adjustment Material Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Adjustment Material Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Sales Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;




                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Scrap Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Capitalized Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;
                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Closing Amount");

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Closing Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;


                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;

                }
                else if (Amount == "" && Qty != "")
                {



                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SL";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 5;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Group";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Category";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Mat.Mst.Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 23;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 35;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    if (Country == "true")
                    {
                        //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Country Name");
                        //sheet1headreColIndex++;

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Country Name";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;




                    }
                    if (materialStorage == "true")
                    {

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Storage";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 18;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;

                    }
                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "UOM");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "UOM";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 6;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Is Asset";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Opening Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Opening Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Receipts Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Receipts Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Return Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Return Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Purchase Material Return Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Purchase Material Return Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Adjustment Material Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Adjustment Material Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Sales Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;




                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Scrap Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Transfer Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 22;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Closing Qty");


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Closing Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;



                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;


                }
            }
            int sl = 0;
            var Row_Total_Start = _rowL + 1;
            int col = 0;
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                sl++;
                _rowL++;
                //int col = 0;
                if (fromDate != "" && toDate != "")
                {
                    if (Amount != "" && Qty != "")
                    {
                        col = 1;
                        report.SetText(ref sheet1, _rowL, col, sl.ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialCategory"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString()); col++;
                        //report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["MaterialStorageLocation"].ToString());
                        //report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["GRNDate"].ToString());
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["HSNCode"].ToString()); col++;

                        if (materialStorage == "true")
                        {
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialStorageLocation"].ToString()); col++;

                        }

                        if (Country == "true")
                        {
                            report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["CountryName"].ToString());
                            report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["UOM"].ToString());
                            report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
                            report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
                            report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));



                            report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 22, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 23, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 24, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 25, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 26, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 27, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 28, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 29, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 30, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 31, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));
                            report.SetText(ref sheet1, _rowL, 32, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString()));
                        }
                        else
                        {
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["UOM"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["IsAsset"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString())); col++;



                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString())); col++;



                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesReturnQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesReturnForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString())); col++;
                        }

                    }
                    else if (Amount != "" && Qty == "")
                    {
                        col = 1;
                        report.SetText(ref sheet1, _rowL, col, sl.ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialCategory"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["HSNCode"].ToString());
                        if (materialStorage == "true")
                        { //col=10
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialStorageLocation"].ToString()); col++;
                        }
                        if (Country == "true")
                        {
                            report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["CountryName"].ToString());
                            report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
                            //report.SetText(ref sheet1, _rowL, 6, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
                            report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

                            //report.SetText(ref sheet1, _rowL, 8, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

                            //report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));


                            // report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

                            // report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

                            //report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString()));


                            report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString()));
                        }
                        else
                        {
                            //report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["CountryName"].ToString());
                            //report.SetText(ref sheet1, _rowL, 6, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

                            //report.SetText(ref sheet1, _rowL, 8, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

                            //report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));


                            // report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

                            // report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

                            //report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriodAmount"].ToString()));


                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString()));

                        }
                    }
                    else if (Amount == "" && Qty != "")
                    {
                        col = 1;
                        report.SetText(ref sheet1, _rowL, col, sl.ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialType"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialGroup"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialCategory"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["HSNCode"].ToString()); col++;
                        if (materialStorage == "true")
                        { //col=10
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialStorageLocation"].ToString()); col++;

                        }
                        if (Country == "true")
                        {
                            report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["CountryName"].ToString());
                            report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["UOM"].ToString());
                            report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
                            report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
                            //report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
                            //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));

                            //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
                            //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
                            // report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
                            //report.SetText(ref sheet1, _rowL,11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));


                            report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString()));

                            report.SetText(ref sheet1, _rowL, 22, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));
                        }
                        else
                        {

                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["UOM"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["IsAsset"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString())); col++;

                            //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString())); col++;
                            // report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString())); col++;

                        }

                    }
                }
                if (fromDate == "" && toDate != "")
                {

                    if (Amount != "" && Qty != "")
                    {
                        col = 1;
                        report.SetText(ref sheet1, _rowL, col, sl.ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialType"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialGroup"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialCategory"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["HSNCode"].ToString()); col++;
                        if (materialStorage == "true")
                        {
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialStorageLocation"].ToString()); col++;

                        }
                        if (Country == "true")
                        {
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["CountryName"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["UOM"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["IsAsset"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString())); col++;



                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString())); col++;
                        }
                        else
                        {
                            //col=10;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["UOM"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["IsAsset"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString())); col++;
                        
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString())); col++;
                        }
                    }

                    else if (Amount != "" && Qty == "")
                        {
                            col = 1;
                            report.SetText(ref sheet1, _rowL, col, sl.ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialCategory"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleId"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleName"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["HSNCode"].ToString()); col++;
                            if (materialStorage == "true")
                            {
                                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialStorageLocation"].ToString()); col++;

                            }
                            if (Country == "true")
                            {//col=10;
                                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["CountryName"].ToString()); col++;
                                //report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["UOM"].ToString());
                                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["IsAsset"].ToString()); col++;
                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString())); col++;
                                //report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString())); col++;
                                //report.SetText(ref sheet1, _rowL,9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString())); col++;

                                //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString())); col++;
                                //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString())); col++;
                                // report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString())); col++;
                                //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString())); col++;
                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString())); col++;
                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString())); col++;
                            }
                            else
                            {

                                //report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["UOM"].ToString());
                                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["IsAsset"].ToString()); col++;
                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString())); col++;
                                //report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString())); col++;
                                //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString())); col++;

                                //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString())); col++;
                                //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString())); col++;
                                // report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString())); col++;
                                //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString())); col++;
                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString())); col++;





                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString())); col++;

                            }
                        }
                        else if (Amount == "" && Qty != "")
                        {
                            col = 1;
                            report.SetText(ref sheet1, _rowL, col, sl.ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialType"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialGroup"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialCategory"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleId"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleName"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["HSNCode"].ToString()); col++;
                            if (materialStorage == "true")
                            {
                                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialStorageLocation"].ToString()); col++;

                            }
                            if (Country == "true")
                            {
                                col = 10;
                                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["CountryName"].ToString());
                                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["UOM"].ToString());
                                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
                                //report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
                                //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));

                                //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
                                //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
                                // report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
                                //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));


                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));
                            }
                            else
                            {

                                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["UOM"].ToString()); col++;
                                report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["IsAsset"].ToString()); col++;
                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString())); col++;
                                //report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString())); col++;
                                //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString())); col++;

                                //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString())); col++;
                                //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString())); col++;
                                // report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString())); col++;
                                //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));col++;
                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString())); col++;

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString())); col++;

                                report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString())); col++;

                            }

                        }


                    }


                }


                sheet1.Range[(Row_Total_Start), 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 8;
                sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
                //_rowL++;
                sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);

                sheet1.Name = sheet1Name;
                sheet1.UsedRange.WrapText = true;
                //sheet1.UsedRange.CellStyle.Font.Size = 8;
                sheet1.IsGridLinesVisible = false;
                report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
                report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);


            }

            public List<Dictionary<string, object>> GetRequisitionStockBalance(string plantId, string requisitionDate, string materialMasterId, string articleId)
            {
                try
                {
                    var sql = @"SELECT * FROM (
						SELECT  isnull(MM.UserName,'') MaterialMasterName	
                                ,MC.UserName MaterialCategory
							 ,MM.Id	MaterialMasterId	
							,isnull( ART.StandardName,'') ArticleName	
							 ,ART.Id ArticleId		
							,MaterialStorageLocation= MS.UserName
                           ,FCV.UserName SKU1,SCV.UserName SKU2
							--Balance    
							,Stock=(((((((isnull(opbal.TransactionQty,0)- isnull(IFDOB.IssueQty,0)-isnull(PurchaseReturnOBData.Qty,0)-isnull(InventorySalesOBData.Qty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0)-isnull(PurchaseReturnData.Qty,0))-isnull(AdjustmentData.Qty,0))+isnull(IssueReturnData.Qty,0))-isnull(InventorySalesData.Qty,0))-isnull(InventoryScrapData.Qty,0))-isnull(InventoryTransferData.Qty,0))  


						from TRN.InventoryMaterial AS IM
                        left join (select distinct InventoryMaterialId,MaterialStorageId from trn.InventoryReceiveDetail) as IRS on im.Id=irs.InventoryMaterialId
						left join [HKP].[MaterialStorage] MS on ms.id=irs.MaterialStorageId
                        left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                        left join hkp.MaterialCategory as MC on MC.Id = MM.MaterialCategoryId
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						left join( 
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId
                                    ,IRD.IsAsset,Sum(IRD.BaseQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) <= '" + requisitionDate + @"' AND IR.OpeningBalanceId IS NOT NULL 
									group By IRD.InventoryMaterialId,IRD.MaterialStorageId
                                    ,IRD.IsAsset
			                        UNION ALL 
									SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId
                                    ,IRD.IsAsset,Sum(IRD.BaseQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) < '" + requisitionDate + @"' AND isnull(IR.OpeningBalanceId,NULL) IS NULL  
									group By IRD.InventoryMaterialId,IRD.MaterialStorageId
                                    ,IRD.IsAsset
			                        
                                    ) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"' and opbal.MaterialStorageId=IRS.MaterialStorageId
                        LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id
						
						left join(SELECT IRD.InventoryMaterialId, Sum(IRD.BaseQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
											,IRD.MaterialStorageId
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate)	BETWEEN  '" + requisitionDate + @"' AND  '" + requisitionDate + @"' and IRD.IsAsset=0 AND IR.OpeningBalanceId IS  NULL 
									group By IRD.InventoryMaterialId ,IRD.MaterialStorageId) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"' and opbal2.MaterialStorageId=IRS.MaterialStorageId

						left join (select IID.InventoryMaterialId,IH.MaterialStorageId, Sum(IH.Qty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
								WHERE convert(Date,II.IssueDate) BETWEEN  '" + requisitionDate + @"' AND  '" + requisitionDate + @"'  AND II.PlantId='" + plantId + @"'  
								GROUP BY IID.InventoryMaterialId,IH.MaterialStorageId
								) IFD1 On IFD1.InventoryMaterialId=IM.Id and IFD1.MaterialStorageId=IRS.MaterialStorageId

                               --Issue OB
								left join (select IID.InventoryMaterialId,IH.MaterialStorageId, Sum(IH.Qty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
								WHERE convert(Date,II.IssueDate) < '" + requisitionDate + @"' AND II.PlantId='" + plantId + @"'
								GROUP BY IID.InventoryMaterialId,IH.MaterialStorageId
								) IFDOB On IFDOB.InventoryMaterialId=IM.Id and IFDOB.MaterialStorageId=IRS.MaterialStorageId
                       --Issue Return
                        Left join (select IH.InventoryMaterialId,II.MaterialStorageId,sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount from trn.InventoryIssueReturnHistory IH
									 Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.IssueDate) BETWEEN   '" + requisitionDate + "' AND  '" + requisitionDate + @"' AND II.PlantId='" + plantId + @"'
										GROUP BY IH.InventoryMaterialId,II.MaterialStorageId
								 )IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id and IssueReturnData.MaterialStorageId=IRS.MaterialStorageId
					    --Purchase return
                       Left join (select IH.InventoryMaterialId,sum(IH.TransactionQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) BETWEEN  '" + requisitionDate + "' AND  '" + requisitionDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id

                        --Purchase return OB
						 Left join (select IH.InventoryMaterialId,sum(IH.TransactionQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) < '" + requisitionDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )PurchaseReturnOBData ON PurchaseReturnOBData.InventoryMaterialId=IM.Id

                       -- Adjustment
						Left join (select psad.InventoryMaterialId,sum(IH.Qty) Qty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) AdjustmentAmount 
					                 from trn.PhysicalStockAdjustmentHistory IH
									 Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
									 Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
									 WHERE convert(Date,II.IssueDate) BETWEEN  '" + requisitionDate + "' AND  '" + requisitionDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY psad.InventoryMaterialId
								 )AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id

                               --  where  opbal.IsAsset=0 AND IM.PlantId='202034' AND MM.UserName is not null 
						-- InventorySales
						Left join (select ISD.InventoryMaterialId,sum(ISH.Qty) Qty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + requisitionDate + "' AND  '" + requisitionDate + @"' AND Ins.PlantId='" + plantId + @"' GROUP BY ISD.InventoryMaterialId
								 )InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                        -- InventorySales OB 
								 Left join (select ISD.InventoryMaterialId,sum(ISH.Qty) Qty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 WHERE convert(Date,Ins.SalesDate) < '" + requisitionDate + @"' AND Ins.PlantId='" + plantId + @"' GROUP BY ISD.InventoryMaterialId
								 )InventorySalesOBData ON InventorySalesOBData.InventoryMaterialId=IM.Id 
                   --InventoryScrap
								Left join (select ISCD.InventoryMaterialId,sum(ISCH.Qty) Qty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
					                 from [TRN].[InventoryScrapHistory] ISCH
									 Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
									 Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
									 WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + requisitionDate + "' AND  '" + requisitionDate + @"' AND ISC.PlantId='" + plantId + @"' GROUP BY ISCD.InventoryMaterialId
								 )InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id   
					--InventoryTransfer
								Left join ( select IRD.InventoryMaterialId,sum(IRD.InventoryTransferQty) Qty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					                 from [TRN].[InventoryTransferHistory] ITH
									 Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
									 Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
									 WHERE convert(Date,IR.GRNDate) BETWEEN  '" + requisitionDate + "' AND  '" + requisitionDate + @"' AND IR.PlantId='" + plantId + @"'
									 GROUP BY IRD.InventoryMaterialId
								 )InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id      
                                 where   IM.PlantId='" + plantId + @"' AND MM.UserName is not null ) X where X.MaterialMasterId='" + materialMasterId + "' And x.ArticleId='" + articleId + @"'";
                    return _sqlRepository.GetDataCollection(sql);
                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message, ex,
                        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
                }
            }

        public void CreateMaterialStockBalanceForThePeriodSheet(ref IWorksheet sheet1, ref IWorksheet sheet2, ReportUtility report, string sheet1Name, string sheet2Name, string CompanyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string Asset, string Inventory, string Country, string materialStorage)
        {

            var cmdText = "";
            #region Asset/Inventory
            var assetInvStatus = "";
            if (Asset == "true" && Inventory == "true")
            { assetInvStatus = ""; }
            else if (Asset == "true") { assetInvStatus = "and MM.IsAsset=1"; }  else { assetInvStatus = "and MM.IsAsset=0"; }

            #endregion

            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                if (materialStorage == "undefined" || materialStorage == "null")
                    materialStorage = "false";
                #region without materialStorage
                if (materialStorage == "false")
                {
                    cmdText = @"SELECT * FROM (
						SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]             
							  ,isnull(MT.UserName,'') MaterialType
							  , isnull(MGM.UserName,'') AS MaterialGroup						
							 ,isnull(MM.UserName,'') MaterialMasterName	
                            ,MC.UserName MaterialCategory
							 ,MM.Id	MaterialMasterId	
							,HSNC.Code HSNCode
							,isnull( ART.StandardName,'') ArticleName	
							 ,ART.Id ArticleId		
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
							,TUoM.UserName UOM,  MM.IsAsset
							
                             --Opening Balance
                            ,isnull(opbal.TransactionQty,0) As OpeningBalance	
							,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount
						    --Receive

							,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
							--Issue
                         
							,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--Issue Return
							,isnull(IssueReturnData.Qty,0) IssueReturnQtyForThePeriod	
							,isnull(IssueReturnData.IssueReturnAmount,0) IssueReturnForThePeriodAmount	

							--Purchase Return
							,isnull(PurchaseReturnData.Qty,0) PurchaseReturnQtyForThePeriod	
							,isnull(PurchaseReturnData.PurchaseReturnAmount,0) PurchaseReturnForThePeriodAmount	

							--Adjust Return
							,isnull(AdjustmentData.Qty,0) AdjustmentQtyForThePeriod	
							,isnull(AdjustmentData.AdjustmentAmount,0) AdjustmentForThePeriodAmount	

                               --Inventory Sales
							,isnull(InventorySalesData.Qty,0) InventorySalesQtyForThePeriod	
							,isnull(InventorySalesData.InventorySalesAmount,0) InventorySalesForThePeriodAmount	
							--Inventory Scrap
							,isnull(InventoryScrapData.Qty,0) InventoryScrapQtyForThePeriod	
							,isnull(InventoryScrapData.InventoryScrapAmount,0) InventoryScrapForThePeriodAmount	

							--Inventory Transfer Data
							,isnull(InventoryTransferData.CapitalizeQty,0) InventoryTransferQtyForThePeriod	
							,isnull(InventoryTransferData.CapitalizeAmount,0) InventoryTransferForThePeriodAmount

                            --Inventory Sales Return Data
                            ,0 InventorySalesReturnQtyForThePeriod
					        ,0 InventorySalesReturnForThePeriodAmount
					
							--Balance    
							,(((((((isnull(opbal.TransactionQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0)-isnull(PurchaseReturnData.Qty,0))-isnull(AdjustmentData.Qty,0))+isnull(IssueReturnData.Qty,0))-isnull(InventorySalesData.Qty,0))-isnull(InventoryScrapData.Qty,0))-isnull(InventoryTransferData.CapitalizeQty,0)) Closing 
							,(((((((isnull(opbal.TotalMaterialBooksCurrencyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0)-isnull(AdjustmentData.AdjustmentAmount,0))-isnull(PurchaseReturnData.PurchaseReturnAmount,0))+isnull(IssueReturnData.IssueReturnAmount,0)-isnull(InventorySalesData.InventorySalesAmount,0))-isnull(InventoryScrapData.InventoryScrapAmount,0))-isnull(InventoryTransferData.CapitalizeAmount,0))) ClosingAmount


						from TRN.InventoryMaterial AS IM
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                        left join hkp.MaterialCategory as MC on MC.Id = MM.MaterialCategoryId
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						left join( 
			                        SELECT IRD.InventoryMaterialId
                                    ,Sum(IRD.BaseQty-ISNULL(IRD.ShortageQty,0))- sum(isnull(II.IssueQty,0))+SUM(ISNULL(IIR.IssueReturnQty,0))-SUM(isnull(ISD.Qty,0))-sum(ISNULL(PurReturnOBData.Qty,0)) AS TransactionQty
                                , Sum(IRD.TotalMaterialBooksCurrencyAmount)-sum(II.PolicyAmount)-SUM(ISD.InventorySalesAmount)-sum(ISNULL(PurReturnOBData.PurchaseReturnAmount,0)) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
                                    -- InventoryIssue OB
                                    LEFT JOIN (
									    select IID.InventoryMaterialId,IH.InventoryReceiveDetailId, Sum(ISNULL(IH.Qty,0)) IssueQty , Sum(ISNULL(IH.TotalAmount,0)) PolicyAmount
									    FROM TRN.InventoryIssueDetail IID  
									    LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									    LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									    WHERE convert(Date,II.IssueDate) <= '" + fromDate + @"'  AND II.PlantId='" + plantId + @"'   
									    GROUP BY IID.InventoryMaterialId,IH.InventoryReceiveDetailId
									    ) II ON II.InventoryReceiveDetailId=IRD.Id
                                    -- InventoryReturnIssue OB
                                    LEFT JOIN (
									    select IRH.InventoryMaterialId,IRH.InventoryReceiveDetailId, Sum(ISNULL(IRH.Qty,0)) IssueReturnQty , Sum(ISNULL(IRH.TotalAmount,0)) PolicyAmount
									    FROM TRN.InventoryIssueReturnHistory IRH  
									    LEFT JOIN TRN.InventoryIssueReturn II ON II.Id=IRH.InventoryIssueReturnId
										Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IRH.InventoryReceiveDetailId
									    WHERE convert(Date,II.IssueDate) <= '" + fromDate + @"'  AND II.PlantId='" + plantId + @"'  
									    GROUP BY IRH.InventoryMaterialId,IRH.InventoryReceiveDetailId
									    ) IIR ON IIR.InventoryReceiveDetailId=IRD.Id
                                -- InventorySales OB 
								    Left join (select ISD.InventoryMaterialId,ISH.InventoryReceiveDetailId,sum(ISNULL(ISH.Qty,0)) Qty,sum(ISH.BaseRate) Rate, (sum(ISNULL(ISH.BooksCurrencyBaseAmount,0))) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) <= '" + fromDate + @"' AND Ins.PlantId='" + plantId + @"' GROUP BY ISD.InventoryMaterialId,ISH.InventoryReceiveDetailId
								        ) ISD ON ISD.InventoryReceiveDetailId=IRD.Id
                                    Left join (select IH.InventoryReceiveDetailId,IH.InventoryMaterialId,sum(IH.TransactionQty) Qty
								, (sum((IH.TransactionQty*IRD.MaterialTranRate)+IH.ChargesTranAmount)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) <= '" + fromDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryReceiveDetailId,IH.InventoryMaterialId
								 )PurReturnOBData ON PurReturnOBData.InventoryReceiveDetailId=IRD.Id
			                        where convert(Date,IR.GRNDate) <= '" + fromDate + @"' AND IR.OpeningBalanceId IS NOT NULL
									group By IRD.InventoryMaterialId 

			                        UNION ALL 
                                    select * from (
									SELECT IRD.InventoryMaterialId
                                    ,Sum(IRD.BaseQty-ISNULL(IRD.ShortageQty,0))- sum(isnull(II.IssueQty,0))+SUM(ISNULL(IIR.IssueReturnQty,0))-SUM(isnull(ISD.Qty,0))-sum(ISNULL(PurReturnOBData.Qty,0))-sum(ISNULL(InventoryTransferData.CapitalizeQty,0)) AS TransactionQty
									, Sum(IRD.TotalMaterialBooksCurrencyAmount)-sum(ISNULL(II.PolicyAmount,0))-SUM(ISNULL(ISD.InventorySalesAmount,0))-sum(ISNULL(PurReturnOBData.PurchaseReturnAmount,0))-sum(ISNULL(InventoryTransferData.CapitalizeAmount,0)) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
                                    LEFT JOIN (
									    select IID.InventoryMaterialId,IH.InventoryReceiveDetailId, Sum(ISNULL(IH.Qty,0)) IssueQty , Sum(ISNULL(IH.TotalAmount,0)) PolicyAmount
									    FROM TRN.InventoryIssueDetail IID  
									    LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									    LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									    WHERE convert(Date,II.IssueDate) < '" + fromDate + @"'  AND II.PlantId='" + plantId + @"'   
									    GROUP BY IID.InventoryMaterialId,IH.InventoryReceiveDetailId
									    ) II ON II.InventoryReceiveDetailId=IRD.Id
                                    -- InventoryReturnIssue OB
                                    LEFT JOIN (
									    select IRH.InventoryMaterialId,IRH.InventoryReceiveDetailId, Sum(ISNULL(IRH.Qty,0)) IssueReturnQty , Sum(ISNULL(IRH.TotalAmount,0)) PolicyAmount
									    FROM TRN.InventoryIssueReturnHistory IRH  
									    LEFT JOIN TRN.InventoryIssueReturn II ON II.Id=IRH.InventoryIssueReturnId
										Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IRH.InventoryReceiveDetailId
									    WHERE convert(Date,II.IssueDate) < '" + fromDate + @"'  AND II.PlantId='" + plantId + @"' 
									    GROUP BY IRH.InventoryMaterialId,IRH.InventoryReceiveDetailId
									    ) IIR ON IIR.InventoryReceiveDetailId=IRD.Id
                                    -- InventorySales OB 
								    Left join (select ISD.InventoryMaterialId,ISH.InventoryReceiveDetailId,sum(ISNULL(ISH.Qty,0)) Qty,sum(ISH.BaseRate) Rate, (sum(ISNULL(ISH.BooksCurrencyBaseAmount,0))) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) < '" + fromDate + @"' AND Ins.PlantId='" + plantId + @"' 
                                    GROUP BY ISD.InventoryMaterialId,ISH.InventoryReceiveDetailId
								        )ISD ON ISD.InventoryReceiveDetailId=IRD.Id
                                --Purchase Return OB
								Left join (select IH.InventoryReceiveDetailId,IH.InventoryMaterialId,sum(IH.TransactionQty) Qty
								, (sum((IH.TransactionQty*IRD.MaterialTranRate)+IH.ChargesTranAmount)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) < '" + fromDate + @"' AND II.PlantId='" + plantId + @"'  GROUP BY IH.InventoryReceiveDetailId,IH.InventoryMaterialId
								 )PurReturnOBData ON PurReturnOBData.InventoryReceiveDetailId=IRD.Id
			                      
                                --Capitalize
								Left join ( SELECT IRD.InventoryMaterialId,CMD.InventoryReceiveDetailId,IR.PlantId,SUM(IRD.TransactionQty) CapitalizeQty,SUM(CMD.Amount) CapitalizeAmount 
											FROM  [TRN].[CapitalizationMasterDetail] CMD 
											JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=CMD.InventoryReceiveDetailId 
											JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId 
											JOIN [TRN].[CapitalizationMaster] CM ON CM.Id=CMD.CapitalizationMasterId
											where CMD.InventoryIssueHistoryId IS NULL AND convert(Date,CM.CapitalizationDate) < '" + fromDate + @"'  AND IR.PlantId='" + plantId + @"'
									 GROUP BY IRD.InventoryMaterialId,IR.PlantId,CMD.InventoryReceiveDetailId
								 )InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IRD.InventoryMaterialId AND  InventoryTransferData.InventoryReceiveDetailId=IRD.Id
                                    WHERE convert(Date,IR.GRNDate) < '" + fromDate + @"' AND isnull(IR.OpeningBalanceId,NULL) IS NULL
									GROUP By IRD.InventoryMaterialId--,IRD.MaterialStorageId
                                     ) x
			                        where x.TransactionQty>0
                                    ) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"' 
                        
                        --left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
                        LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id
						
						left join(SELECT IRD.InventoryMaterialId, Sum(IRD.BaseQty-ISNULL(IRD.ShortageQty,0)) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate)	BETWEEN  '" + fromDate + @"' AND  '" + toDate + @"' AND IR.OpeningBalanceId IS  NULL 
                           --AND (ISNULL(IR.AuthorizedByStatus,'')!='Reject') AND   ISNULL(IR.CheckedByStatus,'')!='Reject' 
                           GROUP BY IRD.InventoryMaterialId
                        ) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
         

						left join (select IID.InventoryMaterialId, Sum(IH.Qty) IssueQty , Sum(ISNULL(IH.TotalAmount,0)) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
                                    LEFT JOIN TRN.inventoryreceiveDetail IRD On IRD.Id=IH.inventoryreceiveDetailId
								WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND  '" + toDate + @"' AND II.PlantId='" + plantId + @"'  GROUP BY IID.InventoryMaterialId
								) IFD1 On IFD1.InventoryMaterialId=IM.Id
                               
                       --Issue Return
                        Left join (select IH.InventoryMaterialId,sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount from trn.InventoryIssueReturnHistory IH
									 Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND  '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
					    --Purchase return
                       Left join (select IH.InventoryMaterialId,sum(IH.TransactionQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum((IH.TransactionQty*IRD.MaterialTranRate)+IH.ChargesTranAmount)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) BETWEEN  '" + fromDate + @"' AND  '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id

                       -- Adjustment
						Left join (select psad.InventoryMaterialId,sum(IH.Qty) Qty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) AdjustmentAmount 
					                 from trn.PhysicalStockAdjustmentHistory IH
									 Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
									 Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND  '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY psad.InventoryMaterialId
								 )AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id

						-- InventorySales
						Left join (select ISD.InventoryMaterialId,sum(ISH.Qty) Qty,sum(ISH.BaseRate) Rate, (sum(ISNULL(ISH.BooksCurrencyBaseAmount,0))) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND  '" + toDate + @"' AND Ins.PlantId='" + plantId + @"' GROUP BY ISD.InventoryMaterialId
								 )InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                -- where IM.PlantId='20201' AND MM.UserName is not null --AND MM.UserName like '%Bed Sheet%'
                         
                   --InventoryScrap
								Left join (select ISCD.InventoryMaterialId,sum(ISCH.Qty) Qty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
					                 from [TRN].[InventoryScrapHistory] ISCH
									 Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
									 Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND  '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' GROUP BY ISCD.InventoryMaterialId
								 )InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id   
					--Capitalize
								Left join ( SELECT IRD.InventoryMaterialId,IR.PlantId,SUM(IRD.TransactionQty) CapitalizeQty,SUM(CMD.Amount) CapitalizeAmount 
											FROM  [TRN].[CapitalizationMasterDetail] CMD 
											JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=CMD.InventoryReceiveDetailId 
											JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId 
											JOIN [TRN].[CapitalizationMaster] CM ON CM.Id=CMD.CapitalizationMasterId
											where CMD.InventoryIssueHistoryId IS NULL AND convert(Date,CM.CapitalizationDate) BETWEEN  '" + fromDate + @"' AND  '" + toDate + @"'  and IR.PlantId='" + plantId + @"'
									 GROUP BY IRD.InventoryMaterialId,IR.PlantId
								 )InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id  
                                 where   IM.PlantId='" + plantId + @"' AND MM.UserName is not null "+ assetInvStatus + @" ) X 
                WHERE X.OpeningBalance+X.ReceivedForThePeriod+X.IssueForThePeriod+X.Closing>0";

                }
                else
                {
                    cmdText = @"SELECT * FROM (
						SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]             
							  ,isnull(MT.UserName,'') MaterialType
							  , isnull(MGM.UserName,'') AS MaterialGroup					
							 ,isnull(MM.UserName,'') MaterialMasterName	
                            ,MC.UserName MaterialCategory
							 ,MM.Id	MaterialMasterId	
							,HSNC.Code HSNCode
							,isnull( ART.StandardName,'') ArticleName	
							 ,ART.Id ArticleId		
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
							,MaterialStorageLocation= MS.UserName
							,TUoM.UserName UOM,  IsAsset=MM.IsAsset
							
                             --Opening Balance
                            ,isnull(opbal.TransactionQty,0) As OpeningBalance	
							,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount

						    --Receive

							,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
							--Issue
                         
							,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--Issue Return
							,isnull(IssueReturnData.Qty,0) IssueReturnQtyForThePeriod	
							,isnull(IssueReturnData.IssueReturnAmount,0) IssueReturnForThePeriodAmount	

							--Purchase Return
							,isnull(PurchaseReturnData.Qty,0) PurchaseReturnQtyForThePeriod	
							,isnull(PurchaseReturnData.PurchaseReturnAmount,0) PurchaseReturnForThePeriodAmount	

							--Adjust Return
							,isnull(AdjustmentData.Qty,0) AdjustmentQtyForThePeriod	
							,isnull(AdjustmentData.AdjustmentAmount,0) AdjustmentForThePeriodAmount	

                               --Inventory Sales
							,isnull(InventorySalesData.Qty,0) InventorySalesQtyForThePeriod	
							,isnull(InventorySalesData.InventorySalesAmount,0) InventorySalesForThePeriodAmount	
							--Inventory Scrap
							,isnull(InventoryScrapData.Qty,0) InventoryScrapQtyForThePeriod	
							,isnull(InventoryScrapData.InventoryScrapAmount,0) InventoryScrapForThePeriodAmount	

							--Inventory Transfer Data
							,isnull(InventoryTransferData.CapitalizeQty,0) InventoryTransferQtyForThePeriod	
							,isnull(InventoryTransferData.CapitalizeAmount,0) InventoryTransferForThePeriodAmount

                            --Inventory Sales Return Data
                            ,0 InventorySalesReturnQtyForThePeriod
					        ,0 InventorySalesReturnForThePeriodAmount
					
							--Balance    
							,(((((((isnull(opbal.TransactionQty,0)+ isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0)-isnull(PurchaseReturnData.Qty,0))-isnull(AdjustmentData.Qty,0))+isnull(IssueReturnData.Qty,0))-isnull(InventorySalesData.Qty,0))-isnull(InventoryScrapData.Qty,0))-isnull(InventoryTransferData.CapitalizeQty,0)) Closing 
							,(((((((isnull(opbal.TotalMaterialBooksCurrencyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0)-isnull(AdjustmentData.AdjustmentAmount,0))-isnull(PurchaseReturnData.PurchaseReturnAmount,0))+isnull(IssueReturnData.IssueReturnAmount,0)-isnull(InventorySalesData.InventorySalesAmount,0))-isnull(InventoryScrapData.InventoryScrapAmount,0))-isnull(InventoryTransferData.CapitalizeAmount,0))) ClosingAmount


						from TRN.InventoryMaterial AS IM
                        left join (select distinct InventoryMaterialId,MaterialStorageId from trn.InventoryReceiveDetail) as IRS on im.Id=irs.InventoryMaterialId
						left join [HKP].[MaterialStorage] MS on ms.id=irs.MaterialStorageId
                        left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id     
                        left join hkp.MaterialCategory as MC on MC.Id = MM.MaterialCategoryId
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						left join( 
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId
                                    ,Sum(IRD.BaseQty-IRD.ShortageQty)-sum(isnull(ii.IssueQty,0))+SUM(ISNULL(IIR.IssueReturnQty,0))-sum(isnull(ISD.Qty,0)) AS TransactionQty, Sum((ROUND(IRD.BaseQty*ird.MaterialTranRate*IR.ToCurrencyRate,2)+IRD.ChargesTranAmount)-Round(IRD.ShortageQty*IRD.MaterialTranRate,2))-SUM(ISNULL(II.IssueAmount,0))-SUM(ISNULL(ISD.InventorySalesAmount,0)) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId

                                    -- InventoryIssue OB
                                    LEFT JOIN (
									    select IID.InventoryMaterialId,IH.InventoryReceiveDetailId, IH.MaterialStorageId
                                        , Sum(ISNULL(IH.Qty,0)) IssueQty , Sum(ISNULL(IH.TotalAmount,0)) IssueAmount
									    FROM TRN.InventoryIssueDetail IID  
									    LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									    LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									    WHERE convert(Date,II.IssueDate) <= '" + fromDate + @"'  AND II.PlantId='" + plantId + @"' 
                                        AND ISNULL(IH.inventoryreceivedetailId,'')  NOT IN (SELECT inventoryreceivedetailId FROM [TRN].[CapitalizationMasterDetail] where  InventoryIssueHistoryId is null and Source='AUC' )
									    GROUP BY IID.InventoryMaterialId,IH.InventoryReceiveDetailId, IH.MaterialStorageId
									    ) II ON II.InventoryReceiveDetailId=IRD.Id and II.MaterialStorageId=IRD.MaterialStorageId
                                    -- InventoryReturnIssue OB
                                    LEFT JOIN (
									    select IRH.InventoryMaterialId,IRH.InventoryReceiveDetailId, II.MaterialStorageId, Sum(ISNULL(IRH.Qty,0)) IssueReturnQty , Sum(ISNULL(IRH.TotalAmount,0)) PolicyAmount
									    FROM TRN.InventoryIssueReturnHistory IRH  
									    LEFT JOIN TRN.InventoryIssueReturn II ON II.Id=IRH.InventoryIssueReturnId
										Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IRH.InventoryReceiveDetailId
									    WHERE convert(Date,II.IssueDate) <= '" + fromDate + @"'  AND II.PlantId='" + plantId + @"'  
									    GROUP BY IRH.InventoryMaterialId,IRH.InventoryReceiveDetailId, II.MaterialStorageId
									    ) IIR ON IIR.InventoryReceiveDetailId=IRD.Id and II.MaterialStorageId=IRD.MaterialStorageId
                                -- InventorySales OB 
								    Left join (select ISD.InventoryMaterialId,ISH.InventoryReceiveDetailId,Ins.MaterialStorageId,sum(ISNULL(ISH.Qty,0)) Qty,sum(ISH.BaseRate) Rate, (sum(ISNULL(ISH.BooksCurrencyBaseAmount,0))) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 WHERE convert(Date,Ins.SalesDate) <= '" + fromDate + @"' AND Ins.PlantId='" + plantId + @"' 
									 GROUP BY ISD.InventoryMaterialId,ISH.InventoryReceiveDetailId,Ins.MaterialStorageId
								        ) ISD ON ISD.InventoryReceiveDetailId=IRD.Id and ISD.MaterialStorageId=IRD.MaterialStorageId
                                  --Purchase Return OB
								Left join (select IH.InventoryReceiveDetailId,II.MaterialStorageId,IH.InventoryMaterialId,sum(IH.TransactionQty) Qty
								, (sum((IH.TransactionQty*IRD.MaterialTranRate)+IH.ChargesTranAmount)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) < '" + fromDate + @"' AND II.PlantId='" + plantId + @"'  
                                    GROUP BY IH.InventoryReceiveDetailId,IH.InventoryMaterialId,II.MaterialStorageId
								 )PurReturnOBData ON PurReturnOBData.InventoryReceiveDetailId=IRD.Id and PurReturnOBData.MaterialStorageId=IRD.MaterialStorageId

			                        WHERE convert(Date,IR.GRNDate) <= '" + fromDate + @"' AND IR.OpeningBalanceId IS NOT NULL
                                    AND IRD.Id  NOT IN (SELECT ISNULL(InventoryReceiveDetailId,'') FROM [TRN].[CapitalizationMasterDetail]  where InventoryIssueHistoryId IS NULL)
									group By IRD.InventoryMaterialId,IRD.MaterialStorageId
                                    

			                        UNION ALL 
                                    SELECT * FROM (
									SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId
                                    ,Sum(IRD.BaseQty-IRD.ShortageQty)+SUM(ISNULL(IIR.IssueReturnQty,0))- sum(isnull(II.IssueQty,0))-SUM(isnull(ISD.Qty,0))-sum(ISNULL(PurReturnOBData.Qty,0)) AS TransactionQty
									, Sum((ROUND(IRD.BaseQty*ird.MaterialTranRate*IR.ToCurrencyRate,2)+IRD.ChargesTranAmount)-Round(IRD.ShortageQty*IRD.MaterialTranRate,2))+SUM(ISNULL(IIR.IssueReturnAmount,0))-sum(ISNULL(II.IssueAmount,0))-SUM(ISNULL(ISD.InventorySalesAmount,0))-sum(ISNULL(PurReturnOBData.PurchaseReturnAmount,0)) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
                                    
                                    LEFT JOIN (
									    select IID.InventoryMaterialId,IH.InventoryReceiveDetailId, IH.MaterialStorageId, Sum(ISNULL(IH.Qty,0)) IssueQty , Sum(ISNULL(IH.TotalAmount,0)) IssueAmount
									    FROM TRN.InventoryIssueDetail IID  
									    LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									    LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									    WHERE convert(Date,II.IssueDate) < '" + fromDate + @"'  AND II.PlantId='" + plantId + @"' 
                                        AND ISNULL(IH.inventoryreceivedetailId,'')  NOT IN (SELECT inventoryreceivedetailId FROM [TRN].[CapitalizationMasterDetail] where  InventoryIssueHistoryId is null and Source='AUC' )
									    GROUP BY IID.InventoryMaterialId,IH.InventoryReceiveDetailId, IH.MaterialStorageId
									    ) II ON II.InventoryReceiveDetailId=IRD.Id and II.MaterialStorageId=IRD.MaterialStorageId
                                    -- InventoryReturnIssue OB
                                    LEFT JOIN (
									    select IRH.InventoryMaterialId,IRH.InventoryReceiveDetailId, II.MaterialStorageId, Sum(ISNULL(IRH.Qty,0)) IssueReturnQty  , Sum(ISNULL(IRH.TotalAmount,0)) IssueReturnAmount
									    FROM TRN.InventoryIssueReturnHistory IRH  
									    LEFT JOIN TRN.InventoryIssueReturn II ON II.Id=IRH.InventoryIssueReturnId
										Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IRH.InventoryReceiveDetailId
									    WHERE convert(Date,II.IssueDate) <= '" + fromDate + @"'  AND II.PlantId='" + plantId + @"'  
									    GROUP BY IRH.InventoryMaterialId,IRH.InventoryReceiveDetailId, II.MaterialStorageId
									    ) IIR ON IIR.InventoryReceiveDetailId=IRD.Id and II.MaterialStorageId=IRD.MaterialStorageId
                                    -- InventorySales OB 
								    Left join (select ISD.InventoryMaterialId,ISH.InventoryReceiveDetailId, Ins.MaterialStorageId,sum(ISNULL(ISH.Qty,0)) Qty,sum(ISH.BaseRate) Rate, (sum(ISNULL(ISH.BooksCurrencyBaseAmount,0))) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) < '" + fromDate + @"' AND Ins.PlantId='" + plantId + @"' 
									 GROUP BY ISD.InventoryMaterialId,ISH.InventoryReceiveDetailId, Ins.MaterialStorageId
								        )ISD ON ISD.InventoryReceiveDetailId=IRD.Id and ISD.MaterialStorageId=IRD.MaterialStorageId
                                --Purchase Return OB
								Left join (select IH.InventoryReceiveDetailId,II.MaterialStorageId,IH.InventoryMaterialId,sum(IH.TransactionQty) Qty
								, (sum((IH.TransactionQty*IRD.MaterialTranRate)+IH.ChargesTranAmount)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) < '" + fromDate + @"' AND II.PlantId='" + plantId + @"'  
                                    GROUP BY IH.InventoryReceiveDetailId,IH.InventoryMaterialId,II.MaterialStorageId
								 )PurReturnOBData ON PurReturnOBData.InventoryReceiveDetailId=IRD.Id and PurReturnOBData.MaterialStorageId=IRD.MaterialStorageId
			                        where convert(Date,IR.GRNDate) < '" + fromDate + @"' AND isnull(IR.OpeningBalanceId,NULL) IS NULL
                                    AND IRD.Id  NOT IN (SELECT ISNULL(InventoryReceiveDetailId,'') FROM [TRN].[CapitalizationMasterDetail]  where InventoryIssueHistoryId IS NULL)
									group By IRD.InventoryMaterialId,IRD.MaterialStorageId
                                    )X
                                    WHERE X.TransactionQty>0
			                        
                                    ) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"' and opbal.MaterialStorageId=IRS.MaterialStorageId
                        LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id
						
						left join(SELECT IRD.InventoryMaterialId, Sum(IRD.BaseQty) AS TransactionQty ,  Sum((ROUND(IRD.BaseQty*ird.MaterialTranRate*IR.ToCurrencyRate,2)+IRD.ChargesTranAmount)-Round(IRD.ShortageQty*IRD.MaterialTranRate,2)) TotalMaterialBooksCurrencyAmount
											,IRD.MaterialStorageId
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate)	BETWEEN  '" + fromDate + @"' AND  '" + toDate + @"' AND IR.OpeningBalanceId IS  NULL 
                            AND IRD.Id  NOT IN (SELECT ISNULL(InventoryReceiveDetailId,'') FROM [TRN].[CapitalizationMasterDetail]  where InventoryIssueHistoryId IS NULL)
									group By IRD.InventoryMaterialId ,IRD.MaterialStorageId ) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"' 
                            and opbal2.MaterialStorageId=IRS.MaterialStorageId

						        left join (select IID.InventoryMaterialId,IH.MaterialStorageId, Sum(IH.Qty) IssueQty , Sum(IH.TotalAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
                            LEFT JOIN TRN.inventoryreceiveDetail IRD On IRD.Id=IH.inventoryreceiveDetailId
								WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND  '" + toDate + @"' AND II.PlantId='" + plantId + @"' 
                                AND ISNULL(IH.inventoryreceivedetailId,'')  NOT IN (SELECT inventoryreceivedetailId FROM [TRN].[CapitalizationMasterDetail] where  InventoryIssueHistoryId is null and Source='AUC' )
								GROUP BY IID.InventoryMaterialId,IH.MaterialStorageId
								) IFD1 On IFD1.InventoryMaterialId=IM.Id and IFD1.MaterialStorageId=IRS.MaterialStorageId

                               
                       --Issue Return
                        Left join (select IH.InventoryMaterialId,II.MaterialStorageId,sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount from trn.InventoryIssueReturnHistory IH
									 Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.IssueDate) BETWEEN   '" + fromDate + @"' AND  '" + toDate + @"' AND II.PlantId='" + plantId + @"' 
										GROUP BY IH.InventoryMaterialId,II.MaterialStorageId
								 )IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id and IssueReturnData.MaterialStorageId=IRS.MaterialStorageId
					    --Purchase return
                       Left join (select IH.InventoryMaterialId,II.MaterialStorageId,sum(IH.TransactionQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum((IH.TransactionQty*IRD.MaterialTranRate)+IH.ChargesTranAmount)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) BETWEEN  '" + fromDate + @"' AND  '" + toDate + @"' AND II.PlantId='" + plantId + @"' 
                                GROUP BY IH.InventoryMaterialId,II.MaterialStorageId
								 )PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id AND PurchaseReturnData.MaterialStorageId=IRS.MaterialStorageId

                      
                       -- Adjustment
						Left join (select psad.InventoryMaterialId,sum(IH.Qty) Qty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) AdjustmentAmount 
					                 from trn.PhysicalStockAdjustmentHistory IH
									 Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
									 Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
									 WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND  '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY psad.InventoryMaterialId
								 )AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id
						-- InventorySales
						Left join (select ISD.InventoryMaterialId,ins.MaterialStorageId,sum(ISH.Qty) Qty,sum(ISH.BaseRate) Rate, (sum(ISNULL(ISH.BooksCurrencyBaseAmount,0))) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND  '" + toDate + @"' AND Ins.PlantId='" + plantId + @"' 
                                GROUP BY ISD.InventoryMaterialId,ins.MaterialStorageId
								 )InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id   and InventorySalesData.MaterialStorageId=IRS.MaterialStorageId  
                       
                   --InventoryScrap
								Left join (select ISCD.InventoryMaterialId,sum(ISCH.Qty) Qty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
					                 from [TRN].[InventoryScrapHistory] ISCH
									 Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
									 Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
									 WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND  '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' GROUP BY ISCD.InventoryMaterialId
								 )InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id   
					--Capitalize
								Left join ( SELECT IRD.InventoryMaterialId,IRD.MaterialStorageId,IR.PlantId,SUM(IRD.TransactionQty) CapitalizeQty,SUM(CMD.Amount) CapitalizeAmount 
											FROM  [TRN].[CapitalizationMasterDetail] CMD 
											JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=CMD.InventoryReceiveDetailId 
											JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId 
											JOIN [TRN].[CapitalizationMaster] CM ON CM.Id=CMD.CapitalizationMasterId
											where CMD.InventoryIssueHistoryId IS NULL AND convert(Date,CM.CapitalizationDate) BETWEEN  '" + fromDate + @"' AND  '" + toDate + @"'  and IR.PlantId='" + plantId + @"'
									 GROUP BY IRD.InventoryMaterialId,IR.PlantId,IRD.MaterialStorageId
								 )InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id and opbal2.MaterialStorageId=InventoryTransferData.MaterialStorageId 
                                 where   IM.PlantId='" + plantId + @"' AND MM.UserName is not null " + assetInvStatus + @") X 
                                 WHERE X.OpeningBalance+X.ReceivedForThePeriod+X.IssueForThePeriod+X.Closing>0";
                }
                #endregion

            }
            
            var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);
            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();


            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");

            var _rowd = 4;

            if (fromDate != "" && toDate != "")
            {
                sheet1[_rowd, 3].Text = fromDate + " " + "To" + " " + toDate;
                sheet1.Range[_rowd, 3, _rowd, 6].CellStyle.Font.Size = 8;
                sheet1.Range[_rowd, 3, _rowd, 6].CellStyle.Font.Bold = false;
                sheet1.Range[_rowd, 3, _rowd, 6].Merge();
                //sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }

            else
            {
                sheet1.Range[_rowd, 3, _rowd, 4].Text = toDate;
                sheet1.Range[_rowd, 3, _rowd, 4].CellStyle.Font.Size = 8;
                sheet1.Range[_rowd, 3, _rowd, 4].CellStyle.Font.Bold = false;
                sheet1.Range[_rowd, 3, _rowd, 4].Merge();
                //sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

            }
            var _rows = 5;
            sheet1[_rows, 3].Text = "Report Ref No: ";
            sheet1.Range[_rows, 3, _rows, 6].CellStyle.Font.Size = 8;
            sheet1.Range[_rows, 3, _rows, 6].Merge();
            sheet1.Range[_rows, 3].CellStyle.Font.Bold = false;

            var _row = 7;
            var _rowL = _row;
            var row = _row + 1;

            var sheet1headreColIndex = 1;
            _rowL += 1;
            row++;

            int StartRange = 0;
            int EndRange = 0;
            if (fromDate != "" && toDate != "")
            {

                if (Amount != "" && Qty != "")
                {
                    if (Country == "true")
                    {
                        sheet1.Range[_row, 15, _row, 16].Text = "Opening Balance";
                        sheet1.Range[_row, 15, _row, 16].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 15, _row, 16].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 15, _row, 16].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 15, _row, 16].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 15, _row, 16].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 15, _row, 16].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 15, _row, 16].Merge();

                        sheet1.Range[_row, 17, _row, 18].Text = "Material Receipts";
                        sheet1.Range[_row, 17, _row, 18].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 17, _row, 18].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 17, _row, 18].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 17, _row, 18].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 17, _row, 18].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 17, _row, 18].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 17, _row, 18].Merge();



                        sheet1.Range[_row, 19, _row, 20].Text = "Issue Material";
                        sheet1.Range[_row, 19, _row, 20].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 19, _row, 20].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 19, _row, 20].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 19, _row, 20].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 19, _row, 20].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 19, _row, 20].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 19, _row, 20].Merge();

                        sheet1.Range[_row, 21, _row, 22].Text = "Issue Material Return";
                        sheet1.Range[_row, 21, _row, 22].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 21, _row, 22].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 21, _row, 22].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 21, _row, 22].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 21, _row, 22].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 21, _row, 22].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 21, _row, 22].Merge();


                        sheet1.Range[_row, 23, _row, 24].Text = "Purchase Material Return";
                        sheet1.Range[_row, 23, _row, 24].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 23, _row, 24].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 23, _row, 24].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 23, _row, 24].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 23, _row, 24].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 23, _row, 24].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 23, _row, 24].Merge();

                        sheet1.Range[_row, 25, _row, 26].Text = "Adjustment Material";
                        sheet1.Range[_row, 25, _row, 26].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 25, _row, 26].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 25, _row, 26].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 25, _row, 26].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 25, _row, 26].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 25, _row, 26].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 25, _row, 26].Merge();

                        sheet1.Range[_row, 27, _row, 28].Text = "Inventory Sales";
                        sheet1.Range[_row, 27, _row, 28].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 27, _row, 28].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 27, _row, 28].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 27, _row, 28].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 27, _row, 28].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 27, _row, 28].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 27, _row, 28].Merge();


                        sheet1.Range[_row, 29, _row, 30].Text = "Inventory Scrap";
                        sheet1.Range[_row, 29, _row, 30].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 29, _row, 30].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 29, _row, 30].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 29, _row, 30].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 29, _row, 30].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 29, _row, 30].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 29, _row, 30].Merge();

                        sheet1.Range[_row, 31, _row, 32].Text = "Inventory Transfer";
                        sheet1.Range[_row, 31, _row, 32].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 31, _row, 32].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 31, _row, 32].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 31, _row, 32].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 31, _row, 32].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 31, _row, 32].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 31, _row, 32].Merge();

                        sheet1.Range[_row, 33, _row, 34].Text = "Closing Balance";
                        sheet1.Range[_row, 33, _row, 34].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 33, _row, 34].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 33, _row, 34].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 33, _row, 34].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 33, _row, 34].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 33, _row, 34].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 33, _row, 34].Merge();
                    }
                    else
                    {
                        if (Country == "undefined" || Country == "null") Country = "false";

                        StartRange = 15;
                        EndRange = StartRange + 1;
                        if (materialStorage == "undefined" || materialStorage == "null") materialStorage = "false";

                        if (materialStorage == "true")
                        {
                            StartRange = 16;
                            EndRange = StartRange + 1;
                        }

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Opening Balance";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;


                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Material Receipts";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Issue Material";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Issue Material Return";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Purchase Material Return";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Adjustment Material";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Inventory Sales";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Inventory Scrap";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Capitalized";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();

                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;
                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Inventory Sales Return";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;
                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Closing Balance";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();

                    }


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SL";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 5;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Group";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Category";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Mat.Mst.Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 23;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    if (Country == "true")
                    {

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Country Name";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;

                    }

                    if (materialStorage == "true")
                    {

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Storage";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;

                    }

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "UoM";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Is Asset";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;
                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;

                    if (Country == "false")
                    {
                        //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Country Name");
                        //sheet1headreColIndex++;
                        sheet1headreColIndex++;
                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;

                        //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;


                    }

                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;

                }
                else if (Amount != "" && Qty == "")
                {

                    //               
                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SL";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 5;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Category";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Mat.Mst.Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 23;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    if (Country == "true")
                    {
                        //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Country Name");
                        //sheet1headreColIndex++;

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Country Name";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;



                    }

                    if (materialStorage == "true")
                    {

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Storage";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;

                    }

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Is Asset";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Opening Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;




                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Receipts Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Return Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Return Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Purchase Material Return Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Purchase Material Return Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Adjustment Material Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Adjustment Material Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Sales Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;




                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Scrap Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Capitalized Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Closing Qty");


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Closing Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;


                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;

                }
                else if (Amount == "" && Qty != "")
                {
                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SL";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 5;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Group";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Category";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Mat.Mst.Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 23;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    if (Country == "true")
                    {
                        //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Country Name");
                        //sheet1headreColIndex++;

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Country Name";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;
                    }
                    if (materialStorage == "true")
                    {

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Storage";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;

                    }
                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "UOM");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "UOM";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Is Asset";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Opening Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Opening Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Receipts Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Receipts Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Return Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Return Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Purchase Material Return Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Purchase Material Return Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Adjustment Material Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Adjustment Material Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 24;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Sales Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;




                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Scrap Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Capitalized Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 22;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Closing Qty");


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Closing Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;



                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;


                }
            }

            if (fromDate == "" && toDate != "")
            {


                if (Amount != "" && Qty != "")
                {
                    if (Country == "true")
                    {
                        sheet1.Range[_row, 14, _row, 15].Text = "Opening Balance";
                        sheet1.Range[_row, 14, _row, 15].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 14, _row, 15].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 14, _row, 15].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 14, _row, 15].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 14, _row, 15].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 14, _row, 15].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 14, _row, 15].Merge();

                        sheet1.Range[_row, 16, _row, 17].Text = "Material Receipts";
                        sheet1.Range[_row, 16, _row, 17].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 16, _row, 17].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 16, _row, 17].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 16, _row, 17].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 16, _row, 17].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 16, _row, 17].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 16, _row, 17].Merge();



                        sheet1.Range[_row, 18, _row, 19].Text = "Issue Material";
                        sheet1.Range[_row, 18, _row, 19].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 18, _row, 19].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 18, _row, 19].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 18, _row, 19].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 18, _row, 19].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 18, _row, 19].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 18, _row, 19].Merge();

                        sheet1.Range[_row, 20, _row, 21].Text = "Issue Material Return";
                        sheet1.Range[_row, 20, _row, 21].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 20, _row, 21].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 20, _row, 21].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 20, _row, 21].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 20, _row, 21].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 20, _row, 21].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 20, _row, 21].Merge();


                        sheet1.Range[_row, 22, _row, 23].Text = "Purchase Material Return";
                        sheet1.Range[_row, 22, _row, 23].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 22, _row, 23].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 22, _row, 23].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 22, _row, 23].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 22, _row, 23].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 22, _row, 23].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 22, _row, 23].Merge();

                        sheet1.Range[_row, 24, _row, 25].Text = "Adjustment Material";
                        sheet1.Range[_row, 24, _row, 25].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 24, _row, 25].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 24, _row, 25].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 24, _row, 25].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 24, _row, 25].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 24, _row, 25].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 24, _row, 25].Merge();

                        sheet1.Range[_row, 26, _row, 27].Text = "Inventory Sales";
                        sheet1.Range[_row, 26, _row, 27].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 26, _row, 27].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 26, _row, 27].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 26, _row, 27].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 26, _row, 27].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 26, _row, 27].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 26, _row, 27].Merge();


                        sheet1.Range[_row, 28, _row, 29].Text = "Inventory Scrap";
                        sheet1.Range[_row, 28, _row, 29].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 28, _row, 29].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 28, _row, 29].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 28, _row, 29].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 28, _row, 29].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 28, _row, 29].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 28, _row, 29].Merge();

                        sheet1.Range[_row, 30, _row, 31].Text = "Inventory Transfer";
                        sheet1.Range[_row, 30, _row, 31].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 30, _row, 31].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 30, _row, 31].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 30, _row, 31].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 30, _row, 31].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 30, _row, 31].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 30, _row, 31].Merge();

                        sheet1.Range[_row, 32, _row, 33].Text = "Closing Balance";
                        sheet1.Range[_row, 32, _row, 33].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, 32, _row, 33].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, 32, _row, 33].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, 32, _row, 33].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 32, _row, 33].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, 32, _row, 33].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, 32, _row, 33].Merge();
                    }
                    else
                    {
                        StartRange = 15;
                        EndRange = StartRange + 1;
                        if (materialStorage == "true")
                        {
                            StartRange = 16;
                            EndRange = StartRange + 1;
                        }

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Opening Balance";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;
                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Material Receipts";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;


                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Issue Material";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;
                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Issue Material Return";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Purchase Material Return";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;
                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Adjustment Material";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;
                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Inventory Sales";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;

                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Inventory Scrap";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;
                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Capitalized";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                        StartRange = EndRange + 1;
                        EndRange = StartRange + 1;
                        sheet1.Range[_row, StartRange, _row, EndRange].Text = "Closing Balance";
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Size = 10;
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.Font.Bold = true;
                        sheet1.Range[_row, StartRange, _row, EndRange].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_row, StartRange, _row, EndRange].CellStyle.FillBackground = ExcelKnownColors.Tan;
                        sheet1.Range[_row, StartRange, _row, EndRange].Merge();
                    }

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SL";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 5;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Group";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Category";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Mat.Mst.Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 23;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    if (Country == "true")
                    {
                        //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Country Name");
                        //sheet1headreColIndex++;

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Country Name";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;


                    }
                    if (materialStorage == "true")
                    {

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Storage";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;

                    }

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "UOM";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Is Asset";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;
                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");
                    //sheet1headreColIndex++;



                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Amount");

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;


                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;

                }
                else if (Amount != "" && Qty == "")
                {

                    //               
                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SL";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 5;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Category";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Mat.Mst.Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 23;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;
                    if (Country == "true")
                    {
                        //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Country Name");
                        //sheet1headreColIndex++;

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Country Name";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;
                    }
                    if (materialStorage == "true")
                    {

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Storage";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;

                    }
                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Is Asset";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Opening Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Opening Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Receipts Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Receipts Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;




                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Return Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Return Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Purchase Material Return Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Purchase Material Return Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Adjustment Material Amount");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Adjustment Material Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Sales Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;




                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Scrap Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Capitalized Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;
                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Closing Amount");

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Closing Amount";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;


                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;

                }
                else if (Amount == "" && Qty != "")
                {



                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SL";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 5;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Group";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Category";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Mat.Mst.Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 23;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article Id";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 35;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    if (Country == "true")
                    {
                        //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Country Name");
                        //sheet1headreColIndex++;

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Country Name";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;




                    }
                    if (materialStorage == "true")
                    {

                        sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Storage";
                        sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 18;
                        sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                        sheet1headreColIndex++;

                    }
                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "UOM");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "UOM";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 6;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Is Asset";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Opening Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Opening Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Receipts Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Receipts Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;



                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Material Return Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Material Return Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Purchase Material Return Qty");
                    //sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Purchase Material Return Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Adjustment Material Qty");
                    //sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Adjustment Material Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;

                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Sales Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;




                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Scrap Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Capitalized Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 22;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
                    sheet1headreColIndex++;


                    //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Closing Qty");


                    sheet1.Range[_rowL, sheet1headreColIndex].Text = "Closing Qty";
                    sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
                    sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;



                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
                    sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;


                }
            }
            int sl = 0;
            var Row_Total_Start = _rowL + 1;
            int col = 0;
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                sl++;
                _rowL++;
                //int col = 0;
                if (fromDate != "" && toDate != "")
                {
                    if (Amount != "" && Qty != "")
                    {
                        col = 1;
                        report.SetText(ref sheet1, _rowL, col, sl.ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialType"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialGroup"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialCategory"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString()); col++;
                        //report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["MaterialStorageLocation"].ToString());
                        //report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["GRNDate"].ToString());
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["HSNCode"].ToString()); col++;
                        if (materialStorage == "true")
                        {
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialStorageLocation"].ToString()); col++;

                        }

                        if (Country == "true")
                        {
                            report.SetText(ref sheet1, _rowL, 13, inventoryMaterialList.Rows[n]["UOM"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, 14, inventoryMaterialList.Rows[n]["IsAsset"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, 15, inventoryMaterialList.Rows[n]["CountryName"].ToString());
                            report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
                            report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, 22, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 23, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, 24, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 25, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, 26, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 27, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, 28, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 29, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, 30, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 31, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, 32, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 33, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, 34, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));
                            report.SetText(ref sheet1, _rowL, 35, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString()));
                        }
                        else
                        {
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["UOM"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["IsAsset"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString())); col++;



                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString())); col++;


                            //report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesReturnQtyForThePeriod"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesReturnForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriodAmount"].ToString())); col++;


                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString())); col++;
                        }

                    }
                    else if (Amount != "" && Qty == "")
                    {
                        col = 1;
                        report.SetText(ref sheet1, _rowL, col, sl.ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialCategory"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["HSNCode"].ToString());
                        if (materialStorage == "true")
                        { //col=10
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialStorageLocation"].ToString()); col++;
                        }
                        if (Country == "true")
                        {
                            report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["CountryName"].ToString());
                            report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
                            //report.SetText(ref sheet1, _rowL, 6, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
                            report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

                            //report.SetText(ref sheet1, _rowL, 8, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

                            //report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));


                            // report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

                            // report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

                            //report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString()));


                            report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString()));
                        }
                        else
                        {
                            //report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["CountryName"].ToString());
                            //report.SetText(ref sheet1, _rowL, 6, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

                            //report.SetText(ref sheet1, _rowL, 8, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

                            //report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));


                            // report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

                            // report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

                            //report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriodAmount"].ToString()));


                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString()));

                        }
                    }
                    else if (Amount == "" && Qty != "")
                    {
                        col = 1;
                        report.SetText(ref sheet1, _rowL, col, sl.ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialType"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialGroup"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialCategory"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["HSNCode"].ToString()); col++;
                        if (materialStorage == "true")
                        { //col=10
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialStorageLocation"].ToString()); col++;

                        }
                        if (Country == "true")
                        {
                            report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["CountryName"].ToString());
                            report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["UOM"].ToString());
                            report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
                            report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
                            //report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
                            //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));

                            //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
                            //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
                            // report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
                            //report.SetText(ref sheet1, _rowL,11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));


                            report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString()));

                            report.SetText(ref sheet1, _rowL, 22, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));
                        }
                        else
                        {

                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["UOM"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["IsAsset"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString())); col++;

                            //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString())); col++;
                            // report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString())); col++;

                        }

                    }
                }
                if (fromDate == "" && toDate != "")
                {

                    if (Amount != "" && Qty != "")
                    {
                        col = 1;
                        report.SetText(ref sheet1, _rowL, col, sl.ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialType"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialGroup"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialCategory"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["HSNCode"].ToString()); col++;
                        if (materialStorage == "true")
                        {
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialStorageLocation"].ToString()); col++;

                        }
                        if (Country == "true")
                        {
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["CountryName"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["UOM"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["IsAsset"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString())); col++;



                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString())); col++;
                        }
                        else
                        {
                            //col=10;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["UOM"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["IsAsset"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriodAmount"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString())); col++;
                        }
                    }

                    else if (Amount != "" && Qty == "")
                    {
                        col = 1;
                        report.SetText(ref sheet1, _rowL, col, sl.ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialCategory"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["HSNCode"].ToString()); col++;
                        if (materialStorage == "true")
                        {
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialStorageLocation"].ToString()); col++;

                        }
                        if (Country == "true")
                        {//col=10;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["CountryName"].ToString()); col++;
                            //report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["UOM"].ToString());
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["IsAsset"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL,9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString())); col++;

                            //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString())); col++;
                            // report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString())); col++;


                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriod"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString())); col++;
                        }
                        else
                        {

                            //report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["UOM"].ToString());
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["IsAsset"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString())); col++;

                            //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString())); col++;
                            // report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriod"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString())); col++;

                        }
                    }
                    else if (Amount == "" && Qty != "")
                    {
                        col = 1;
                        report.SetText(ref sheet1, _rowL, col, sl.ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialType"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialGroup"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialCategory"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleId"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ArticleName"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString()); col++;
                        report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["HSNCode"].ToString()); col++;
                        if (materialStorage == "true")
                        {
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["MaterialStorageLocation"].ToString()); col++;

                        }
                        if (Country == "true")
                        {
                            col = 10;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["CountryName"].ToString());
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["UOM"].ToString());
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
                            //report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
                            //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));

                            //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
                            //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
                            // report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
                            //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));


                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriod"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));
                        }
                        else
                        {

                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["UOM"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["IsAsset"].ToString()); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString())); col++;

                            //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString())); col++;
                            // report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString())); col++;
                            //report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString())); col++;
                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriod"].ToString())); col++;

                            report.SetText(ref sheet1, _rowL, col, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString())); col++;

                        }

                    }


                }


            }


            sheet1.Range[(Row_Total_Start), 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 8;
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            //_rowL++;
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);

            sheet1.Name = sheet1Name;
            sheet1.UsedRange.WrapText = true;
            //sheet1.UsedRange.CellStyle.Font.Size = 8;
            sheet1.IsGridLinesVisible = false;
            report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);


        }

    }
}
